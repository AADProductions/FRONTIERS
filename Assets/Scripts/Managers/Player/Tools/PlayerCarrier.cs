using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;
using ExtensionMethods;

namespace Frontiers {
	//a stripped-down version of Tool that only equips / unequips
	public class PlayerCarrier : PlayerScript
	{
		public PlayerToolState ToolState = PlayerToolState.Unequipped;
		public bool Holstered = false;
		public vp_FPWeapon FPSWeapon = null;
		//stupid vp_FPWeapon component that I'd love to get rid of
		public GameObject FireObject = null;
		public GameObject ToolDoppleganger = null;
		public Transform tr;
		public List <GameObject> EffectDopplegangers = new List<GameObject> ();
		public List <Collider> ToolColliders = new List<Collider> ( );

		public override void Start ()
		{
			enabled = true;
		}

		public override void Initialize ()
		{
			tr = transform;

			base.Initialize ();

			Player.Get.AvatarActions.Subscribe (AvatarAction.ItemACIChange, new ActionListener (ItemACIChange));
			Player.Get.UserActions.Subscribe (UserActionType.ToolHolster, new ActionListener (ToolHolster));

			FPSWeapon = player.FPSWeaponCarry;
		}

		public bool IsEquipped {
			get {
				return (HasWorldItem
				&& (ToolState ==PlayerToolState.Equipped)
				&& worlditem.Is (WIMode.Equipped));
			}
		}

		public WorldItem worlditem;
		protected WorldItem mPreviousWorldItem;

		public bool HasWorldItem {
			get {
				return worlditem != null;
			}
		}

		public void FixedUpdate ( )
		{
			if (GameManager.Is (FGameState.InGame) && !mUpdatingTool) {
				StartCoroutine (UpdateTool ());
			}
		}

		#region action receivers

		public bool ToolHolster (double timeStamp) {

			if (IsEquipped) {
				Holstered = true;
				AddAction (ToolAction.Unequip);
			} else {
				Holstered = false;
				AddAction (ToolAction.Equip);
			}
			return true;
		}

		public bool ItemACIChange (double timeStamp)
		{
			if (!HasWorldItem
				|| worlditem != player.Inventory.ActiveCarryItem
				|| !worlditem.Is (WIMode.Equipped)) {
				//unequip, then requip if anything has changed
				AddAction (ToolAction.Unequip);
				AddAction (ToolAction.Equip);
			}
			return true;
		}
		
		public void UnlockWorldItem ()
		{
			if (HasWorldItem) {
				worlditem.UnlockTransform (tr);
			}
		}

		#endregion

		#region enumerators

		public override bool LockQuickslots {
			get {
				return !CanUnequip ();
			}
		}

		protected IEnumerator Equip ()
		{
			if (Holstered) {
				Debug.Log ("Holstered");
				yield break;
			}

			ToolState = PlayerToolState.Equipping;
			//set equipped mode
			worlditem.SetMode (WIMode.Equipped);
			//clear all actions
			mActions.Clear ();
			//see if the worlditem is a weapon
			//if it is, find a projectile for it
			Weapon weapon = null;
			Equippable equippable = null;
			if (worlditem.Is <Equippable> (out equippable)) {//tell equippable that we're equipping
				equippable.EquipStart ();
			}

			//refresh our dopplegangers
			//we do this BEFORE playing the generic equip animation
			//so it actually has something to show
			RefreshToolDoppleganger (true);
			if (equippable != null) {
				equippable.EquipFinish ();
			}

			yield return StartCoroutine (PlayAnimation ("ToolGenericEquip"));

			ToolState = PlayerToolState.Equipped;
			yield break;
		}

		protected IEnumerator Unequip ()
		{
			//turn off the player projection (even if it's not already on)
			player.Projections.WeaponTrajectory.Hide ();
			ToolState = PlayerToolState.Unequipping;
			//clear all actions
			mActions.Clear ();

			yield return StartCoroutine (PlayAnimation ("ToolGenericUnequip"));

			ToolState = PlayerToolState.Unequipped;
			RefreshToolDoppleganger (false);

			if (HasWorldItem) {
				//parent the worlditem back under its group just in case
				//this will become unnecessary eventually
				//worlditem.UnlockTransform (tr);
				UnlockWorldItem ();
				worlditem.SetMode (WIMode.Stacked);
				worlditem = null;
			}

			//if we're unequipping because the AQI is changing
			//then wait here until it's finished
			while (player.Inventory.LockQuickslots) {
				yield return null;
			}

			yield break;
		}

		public IEnumerator PlayAnimation (string animationName) {

			////Debug.Log ("Playing animation " + animationName);
			mDopplegangerLocked = true;
			GetComponent<Animation>().Play (animationName, AnimationPlayMode.Stop);
			while (GetComponent<Animation>() [animationName].normalizedTime < 1f) {	//wait for animation to play out
				yield return null;
			}
			mDopplegangerLocked = false;

			yield break;
		}

		public IEnumerator UpdateTool ()
		{
			mUpdatingTool = true;
			while (mUpdatingTool) {
				while (!GameManager.Is (Frontiers.FGameState.InGame | Frontiers.FGameState.GamePaused)
					|| !player.HasSpawned
					//|| player.IsHijacked
					|| player.IsDead
					|| player.Status.State.IsSleeping
					|| Cutscene.IsActive) {
					if (IsEquipped) {
						yield return StartCoroutine (Unequip ());
					}
					//wait it out
					yield return null;
				}

				if (HasWorldItem) {
					if (!worlditem.Is (WIMode.Equipped)) {
						yield return StartCoroutine (OnLoseTool ());
					}
				}

				ToolAction nextAction = ToolAction.None;
				
				switch (ToolState) {
				case PlayerToolState.Unequipping:
				case PlayerToolState.Equipping:
					//wait
					break;
					
				case PlayerToolState.Unequipped:
				default:
					nextAction = NextAction ();
					switch (nextAction) {
					case ToolAction.Equip:
						if (CanEquip ()) {	//if we're unequipped, start equipping
							yield return StartCoroutine (Equip ());
						}
						break;
						
					default:
						//check to see if there's an AQI
						break;
					}
					break;
					
				case PlayerToolState.Equipped:
					//handle actions
					nextAction = NextAction ();
					switch (nextAction) {
					case ToolAction.Equip:
					case ToolAction.Unequip:
						//equipping/unequipping are handled here
						if (CanUnequip ()) {	//if we can unequip, get to it right away
							yield return StartCoroutine (Unequip ());
						}
						//after that's done...
						yield return null;
						//wait a tick...
						if (CanEquip ()) {	//this will automatically fail if there's nothing to equip
							yield return StartCoroutine (Equip ());
						}
						break;
						
					default:
						break;
					}
					break;
				}
				//wait a tick
				yield return null;
			}
			mUpdatingTool = false;
			yield break;
		}

		public IEnumerator OnLoseTool ()
		{
			//this can happen with stuff like item placement / dropping
			//we want to reset the tool so it's ready for the next item
			ToolState = PlayerToolState.Unequipped;
			worlditem = null;
			BreakDownToolDoppleganger ();
			yield return StartCoroutine (PlayAnimation ("ToolGenericUnequip"));
			yield break;
		}

		#endregion

		#region tool use

		public virtual void OnHit ()
		{
			
		}

		public bool CanUnequip ()
		{
			bool canUnequip = true;
			if (HasWorldItem) {
				Equippable equippable = null;
				if (worlditem.Is <Equippable> (out equippable)) {	//see if the equippable will allow us to
					//unequip (we use this for certain tools)
					if (equippable.ForceEquip) {
						canUnequip = false;
					}
				}
			}
			return canUnequip;
		}

		public bool CanEquip ()
		{
			if (Holstered) {
				return false;
			}

			if (!HasWorldItem && player.Inventory.HasActiveCarryItem) {
				worlditem = player.Inventory.ActiveCarryItem;
				return true;
			}
			return false;
		}

		#endregion

		#region fx

		protected void BreakDownToolDoppleganger ( )
		{
			ToolColliders.Clear ();
			foreach (GameObject fx in EffectDopplegangers) {
				if (fx != null) {
					GameObject.Destroy (fx);
				}
			}
			if (ToolDoppleganger != null) {
				ToolDoppleganger.SetActive (false);
			}
		}

		public void RefreshToolDoppleganger (bool equipping)
		{
			if (mDopplegangerLocked) {
				//rogue call, probably from OnModeChange or something
				return;
			}

			if (!equipping) {
				BreakDownToolDoppleganger ();
				//nothing else to do here
				return;
			} else if (!HasWorldItem) {
				BreakDownToolDoppleganger ();
			} else {
				//we're equippping and we have a worlditem
				bool useDoppleganger = true;
				bool displayAsTool = true;
				Vector3 equipPos = worlditem.Props.Global.PivotOffset;
				Vector3 equipRot = worlditem.Props.Global.BaseRotation;
				Equippable equippable = null;
				if (worlditem.Is <Equippable> (out equippable)) {
					displayAsTool = equippable.DisplayAsTool;
					useDoppleganger = equippable.UseDoppleganger;
					//prep these just in case, doesn't cost us much
					equipPos += equippable.EquipOffset;
					equipRot += equippable.EquipRotation;
					//invert the equippable X
					equipPos.x = -equipPos.x;
				}
				//now, are we actually using a doppleganger?
				if (!displayAsTool) {
					BreakDownToolDoppleganger ();
					return;
				} else if (!useDoppleganger) {
					BreakDownToolDoppleganger ();
					worlditem.LockTransform (tr);
					worlditem.tr.localPosition = equipPos;
					worlditem.tr.localRotation = Quaternion.Euler (equipRot);
					worlditem.GetComponent<Rigidbody>().isKinematic = true;
					ToolColliders.AddRange (worlditem.Colliders);
					//and we're done
					return;
				} else {
					ToolDoppleganger = WorldItems.GetDoppleganger (worlditem, transform, ToolDoppleganger);
					ToolColliders.AddRange (ToolDoppleganger.GetComponentsInChildren <Collider> ());
					//the equipped mode will apply equipped etc. offset
					//so don't bother to set that here
					ToolDoppleganger.SetActive (true);
				}
			}
		}

		#endregion

		protected ToolAction NextAction ()
		{
			ToolAction nextAction = ToolAction.None;
			if (mActions.Count > 0) {
				nextAction = mActions.Dequeue ();
				while (mActions.Count > 0 && mActions.Peek () == nextAction) {	//remove duplicates
					mActions.Dequeue ();
				}
			}
			return nextAction;
		}

		public void AddAction (ToolAction newAction)
		{
			// HACK: Issue with tool / coroutine temp fix
			if (!gameObject.activeSelf)
				return;

			mActions.Enqueue (newAction);
		}

		protected Queue <ToolAction> mActions = new Queue <ToolAction> ();
		protected bool mUpdatingTool = false;
		protected bool mQueueUse = false;
		protected bool mDopplegangerLocked = false;
		protected float mTension = 0.0f;
	}
}
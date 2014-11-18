using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;
using ExtensionMethods;

namespace Frontiers {
	//[RequireComponent (typeof (Animation))]
	public class PlayerTool : PlayerScript
	{
		//how much to increase force on each cycle
		public static float gHoldForceIncrease = 0.1f;
		public PlayerToolType Type = PlayerToolType.Generic;
		public PlayerToolState ToolState = PlayerToolState.Unequipped;
		public vp_FPWeapon FPSWeapon = null;//stupid vp_FPWeapon component that I'd love to get rid of
		//active item pieces / dopplegangers
		public Placeable Placeable = null;
		public ParticleSystem CursedParticles = null;
		public Material CursedMaterial = null;
		public GameObject FireObject = null;
		public GameObject ToolDoppleganger = null;
		public Transform ToolActionPointObject = null;
		public GameObject ProjectileDoppleganger = null;
		public MegaMorph TensionMorph = null;
		public MegaMorphChan TensionChannel = null;
		public Transform tr;
		public List <GameObject> EffectDopplegangers = new List<GameObject> ();
		public List <Collider> ToolColliders = new List<Collider> ( );
		//projectile launcher variables
		public float LaunchForce = 0f;
		IWIBase ProjectileObject;
		WIStack ProjectileStack;
		//convenience for throwing things
		public Vector3 ItemPosition {
			get {
				if (IsEquipped) {
					if (ToolDoppleganger != null) {
						return ToolDoppleganger.transform.position;
					} else {
						return worlditem.tr.position;
					}
				}
				return tr.position;
			}
		}
		//convenience for throwing things
		public Quaternion ItemRotation {
			get {
				if (IsEquipped) {
					if (ToolDoppleganger != null) {
						return ToolDoppleganger.transform.rotation;
					} else {
						return worlditem.tr.rotation;
					}
				}
				return tr.rotation;
			}
		}

		public override void Start ()
		{
			enabled = true;
		}

		public override void Initialize ()
		{
			tr = transform;

			base.Initialize ();

			Player.Get.UserActions.Subscribe (UserActionType.ToolUse, new ActionListener (ToolUse));
			Player.Get.UserActions.Subscribe (UserActionType.ToolUseHold, new ActionListener (ToolUseHold));
			Player.Get.UserActions.Subscribe (UserActionType.ToolUseRelease, new ActionListener (ToolUseRelease));
			Player.Get.UserActions.Subscribe (UserActionType.ActionCancel, new ActionListener (ActionCancel));
			Player.Get.UserActions.Subscribe (UserActionType.ToolCycleNext, new ActionListener (ToolCycleNext));
			Player.Get.UserActions.Subscribe (UserActionType.ToolCyclePrev, new ActionListener (ToolCyclePrev));

			Player.Get.AvatarActions.Subscribe (AvatarAction.ItemAQIChange, new ActionListener (ItemAQIChange));

			FPSWeapon = player.FPSWeapon;
		}

		public float ToolSwayAmplitude {
			get {
				return mToolSwayAmplitude;
			}
		}

		public float ToolBobAmplitude {
			get {
				return mToolBobAmplitude;
			}
		}

		protected float mToolSwayAmplitude = 1.0f;
		protected float mToolBobAmplitude = 1.0f;

		public bool UsesTensionMorph {
			get {
				return TensionMorph != null && TensionChannel != null;
			}
		}

		public bool HasProjectile {
			get {
				return ProjectileObject != null && !ProjectileObject.Is (WIMode.RemovedFromGame);
			}
		}

		public bool IsEquipped {
			get {
				return (HasWorldItem
				&& (ToolState ==PlayerToolState.Equipped || ToolState ==PlayerToolState.InUse)
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

		#region action receivers

		public bool ToolCyclePrev (double timeStamp)
		{
			AddAction (ToolAction.CyclePrev);
			return true;
		}

		public bool ToolCycleNext (double timeStamp)
		{
			AddAction (ToolAction.CycleNext);
			return false;
		}

		public bool ToolUse (double timeStamp)
		{
			if (!IsEquipped) {
				return true;
			}
			AddAction (ToolAction.UseStart);
			return true;
		}

		public bool ToolUseHold (double timeStamp)
		{
			AddAction (ToolAction.UseHold);
			return true;
		}

		public bool ToolUseRelease (double timeStamp)
		{
			AddAction (ToolAction.UseRelease);
			return true;
		}

		public bool ActionCancel (double timeStamp)
		{
			AddAction (ToolAction.UseFinish);
			return true;
		}

		public bool ItemAQIChange (double timeStamp)
		{
			if (!HasWorldItem
				|| worlditem != player.Inventory.ActiveQuickslotItem
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

		public void FixedUpdate ( )
		{
			if (GameManager.Is (FGameState.InGame) && !mUpdatingTool) {
				StartCoroutine (UpdateTool ());
			}

			if (!HasWorldItem) {
				mToolBobAmplitude = 1.0f;
				mToolSwayAmplitude = 1.0f;
				return;
			}

			//Equippable equippable = null;
			if (worlditem.Is <Equippable> (out mEquippable)) {
				mToolBobAmplitude = mEquippable.ToolHandling;
				mToolSwayAmplitude = mEquippable.ToolHandling;
			}
		}

		protected IEnumerator Equip ()
		{
			ToolState = PlayerToolState.Equipping;
			//set equipped mode
			worlditem.SetMode (WIMode.Equipped);
			//clear all actions
			mActions.Clear ();
			LaunchForce = 0f;
			//see if the worlditem is a weapon
			//if it is, find a projectile for it
			if (worlditem.Is <Equippable> (out mEquippable)) {//tell equippable that we're equipping
				mEquippable.EquipStart ();
				if (worlditem.Is <Weapon> (out mWeapon)) {
					player.FPSMelee.CurrentWeapon = mWeapon;
					bool loadProjectile = false;
					if (mWeapon.Style == PlayerToolStyle.ProjectileLaunch) {
						//first see if the projectile is compaitble	
						if (HasProjectile) {
							if (!Weapon.CanLaunch (mWeapon, ProjectileObject)) {
								//if we can't launch it, load it
								loadProjectile = true;
							}
						} else {
							//if we don't have one, find it and load it
							loadProjectile = true;
						}
					}
					if (loadProjectile) {
						//load the projectile before we create the doppleganger
						//don't play animations because we're already playing the equipping animation
						yield return StartCoroutine (FindAndLoadProjectile (mEquippable, mWeapon, false));
					}
				}
			}

			//refresh our dopplegangers
			//we do this BEFORE playing the generic equip animation
			//so it actually has something to show
			RefreshToolDoppleganger (true);
			if (mEquippable != null) {
				mEquippable.EquipFinish ();
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
			LaunchForce = 0f;

			yield return StartCoroutine (PlayAnimation ("ToolGenericUnequip"));

			//clear dopplegangers, projectiles, etc
			if (HasProjectile) {
				WIStackError stackError = WIStackError.None;
				if (ProjectileStack != null) {
					//push the projectile onto the bottom of its stack
					//this will prevent rampant AQI switching
					Stacks.Push.Item (ProjectileStack, ProjectileObject, false, StackPushMode.Auto, ref stackError);
				} else {
					//if we don't have the stack any more just add it to the player inventory
					player.Inventory.AddItems (ProjectileObject, ref stackError);
				}
				ProjectileObject = null;
				ProjectileStack = null;
			}

			ToolState = PlayerToolState.Unequipped;
			RefreshToolDoppleganger (false);

			if (HasWorldItem) {
				//parent the worlditem back under its group just in case
				//this will become unnecessary eventually
				//worlditem.UnlockTransform (tr);
				UnlockWorldItem ();
				if (worlditem.Is (WIMode.Equipped)) {
					worlditem.SetMode (WIMode.Stacked);
				}
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

			mDopplegangerLocked = true;
			animation.Play (animationName, AnimationPlayMode.Stop);
			while (animation [animationName].normalizedTime < 1f) {	//wait for animation to play out
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
					//|| player.IsHijacked - get rid of this to keep it during conversations
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
						////Debug.Log ("We're equipped and our next action is to equip...");
						if (CanUnequip ()) {	//if we can unequip, get to it right away
							////Debug.Log ("Unequipping in update");
							yield return StartCoroutine (Unequip ());
						}
						//after that's done...
						yield return null;
						//wait a tick...
						if (CanEquip ()) {	//this will automatically fail if there's nothing to equip
							////Debug.Log ("Equipping in update");
							yield return StartCoroutine (Equip ());
						}
						break;
						
					case ToolAction.UseStart:
						if (CanUse ()) {	//if we can use, get to it!
							ToolState = PlayerToolState.InUse;
							yield return StartCoroutine (UseStart ());
						}
						break;

					case ToolAction.CycleNext:
					case ToolAction.CyclePrev:
						if (CanCycle ()) {
							ToolState = PlayerToolState.Cycling;
							yield return StartCoroutine (CycleTool (nextAction));
						}
						break;
						
					default:
						break;
					}
					break;

				case PlayerToolState.IncreasingForce:
					//this means we've equipped a projectile launcher and we're increasing force
					nextAction = NextAction ();
					switch (nextAction) {
					case ToolAction.UseHold:
						//add force to the projectile
						yield return StartCoroutine (UseHold ());
						break;

					case ToolAction.UseRelease:
						//launch the projectile by calling use finish
						yield return StartCoroutine (UseRelease ());
						break;

					case ToolAction.Unequip:
					default:
						//if we unequip, or anything else
						//OnFinishUsing
						yield return StartCoroutine (UseCancel ());
						break;
					}
					break;
					
				case PlayerToolState.InUse:
					nextAction = NextAction ();
					switch (nextAction) {
					case ToolAction.UseFinish:
					default:
						//Equippable equippable = null;
						if (worlditem.Is <Equippable> (out mEquippable)) {
							mEquippable.UseFinish ();
						}
						ToolState = PlayerToolState.Equipped;
						break;
						
					case ToolAction.Equip:
						if (CanUnequip ()) {	//if we can unequip, get to it right away
							////Debug.Log ("Unequipping due to equip call");
							yield return StartCoroutine (Unequip ());
							if (CanEquip ()) {
								yield return StartCoroutine (Equip ());
							}
						}
						break;
						
					case ToolAction.Unequip:
						if (CanUnequip ()) {	//if we can unequip, get to it right away
							////Debug.Log ("Unequipping due to unequip call");
							yield return StartCoroutine (Unequip ());
						}
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

		protected IEnumerator UseStart ()
		{
			//(no equippable)									- (do nothing)
			//Equippable/Generic + (no weapon)					- (do nothing)
			//Equippable/Generic + Weapon/Swing 				- UseStartSwing
			//Equippable/Generic + Weapon/Slice 				- UseStartSlice
			//Equippable/Generic + Weapon/ProjectileLauncher 	- UseStartProjectile
			//Equippable/GenericUsable 							- UseStartStatic

			//Equippable equippable = null;
			if (worlditem.Is <Equippable> (out mEquippable)) {
				mEquippable.UseStart ();
				switch (mEquippable.Type) {
				case PlayerToolType.Generic:
					//get the weapon type
					//Weapon weapon = null;
					if (worlditem.Is <Weapon> (out mWeapon)) {
						switch (mWeapon.Style) {
						case PlayerToolStyle.Swing:
							yield return StartCoroutine (UseStartSwing (mWeapon));
							break;

						case PlayerToolStyle.Slice:
							yield return StartCoroutine (UseStartSlice (mWeapon));
							break;

						case PlayerToolStyle.ProjectileLaunch:
							yield return StartCoroutine (UseStartProjectile (mEquippable, mWeapon));
							break;

						default:
							//do nothing
							ToolState = PlayerToolState.Equipped;
							break;
						}

					} else {
						//if we're generic equippable and have no weapon
						//we're done here
						ToolState = PlayerToolState.Equipped;
					}
					break;

				case PlayerToolType.GenericUsable:
					//just spawn an option menu
					yield return StartCoroutine (UseStartGenericUsable ());
					ToolState = PlayerToolState.Equipped;
					break;

				case PlayerToolType.CustomAction:
					//this kind of equippable performs an action
					//start the action, then wait
					yield return StartCoroutine (UseStartCustomAction (mEquippable));
					ToolState = PlayerToolState.Equipped;
					break;

				//case PlayerToolType.PathEditor:
				default:
					//TODO re-implement path editor
					break;
				}
			} else {
				//if we're not equippable then we're done
				ToolState = PlayerToolState.Equipped;
			}

			yield break;
		}

		protected IEnumerator UseHold ()
		{
			if (!HasProjectile) {
				ToolState = PlayerToolState.Equipped;
				yield break;
			}
			//the only tool type where use hold applies is equippable + weapon/ProjectileLauncher
			//Equippable equippable = null;
			//Weapon weapon = null;
			if (worlditem.Is <Equippable> (out mEquippable)
			    && worlditem.Is <Weapon> (out mWeapon)
				&& mWeapon.Style == PlayerToolStyle.ProjectileLaunch) {
				//meets all the requirements
				LaunchForce = Mathf.Clamp (LaunchForce + gHoldForceIncrease, mWeapon.MinLaunchForce, mWeapon.MaxLaunchForce);
				//player.Projections.WeaponTrajectory.UpdateForce (LaunchForce);
				if (UsesTensionMorph) {
					TensionChannel.Percent = ((LaunchForce - mWeapon.MinLaunchForce) / mWeapon.MaxLaunchForce) * 100f;
				}
			} else {
				ToolState = PlayerToolState.Equipped;
			}
			//wait a tick
			yield return null;
			yield break;
		}

		protected IEnumerator UseRelease ()
		{
			//the only tool type where use hold applies is equippable + weapon/ProjectileLauncher
			//Equippable equippable = null;
			//Weapon weapon = null;
			if (worlditem.Is <Equippable> (out mEquippable)
			    && worlditem.Is <Weapon> (out mWeapon)
				&& mWeapon.Style == PlayerToolStyle.ProjectileLaunch) {
				//meets all the requirements
				player.Projections.WeaponTrajectory.Hide ();
				yield return StartCoroutine (LaunchProjectile (mEquippable, mWeapon));
				yield return StartCoroutine (FindAndLoadProjectile (mEquippable, mWeapon, true));
			}
			//wait a tick
			yield return null;
			//once we've sent the projectile we're just equipped
			player.Projections.WeaponTrajectory.Hide ();
			LaunchForce = 0f;
			ToolState = PlayerToolState.Equipped;
			yield break;
		}

		protected IEnumerator UseCancel ()
		{
			LaunchForce = 0f;
			//return projectile to inventory if we have one
			//turn off trajectory
			player.Projections.WeaponTrajectory.Hide ();
			//set state to equipped
			ToolState = PlayerToolState.Equipped;
			yield break;
		}

		protected IEnumerator UseStartGenericUsable ()
		{
			WorldItemUsable usable = worlditem.MakeUsable ();
			usable.RequirePlayerFocus = false;
			usable.ShowDoppleganger = true;
			Frontiers.GUI.GUIOptionListDialog dialog = null;
			usable.TryToSpawn (true, out dialog);
			yield break;
		}

		protected IEnumerator UseStartCustomAction (Equippable equippable)
		{
			while (!mEquippable.CustomActionFinished) {
				yield return null;
			}
			yield break;
		}

		protected IEnumerator UseStartSwing (Weapon weapon)
		{
			//start swinging the weapon
			//this will trigger a melee weapon animation
			//at a certain point the animation will trigger OnSwingImpact
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.ToolUse, WorldClock.Time);
			//after OnSwingImpact the state will change
			//don't change it yet
			yield break;
		}

		protected IEnumerator UseStartSlice (Weapon weapon)
		{
			//TODO implement
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.ToolUse, WorldClock.Time);
			yield break;
		}

		protected IEnumerator UseStartProjectile (Equippable equippable, Weapon weapon)
		{
			if (!HasProjectile) {
				yield return StartCoroutine (FindAndLoadProjectile (equippable, weapon, true));
			}
			//if we still don't have a projectile, do nothing
			if (!HasProjectile) {
				//just play the sound without launching anything
				mEquippable.UseSuccessfully ();
				//MasterAudio.PlaySound (mEquippable.Sounds.SoundType, ToolDoppleganger.transform, mEquippable.Sounds.SoundUseSuccessfully);
				ToolState = PlayerToolState.Equipped;
				yield break;
			}

			//show the trajectory of the projectile
			LaunchForce = weapon.MinLaunchForce;
			//set its force to min projectile force
			player.Projections.WeaponTrajectory.Show (ToolActionPointObject, LaunchForce, 1f);
			//set state to increasing force so use hold and use release have the right effect
			ToolState = PlayerToolState.IncreasingForce;
			yield break;
		}

		protected IEnumerator LaunchProjectile (Equippable equippable, Weapon weapon)
		{
			if (!HasProjectile) {
				//if we don't have a projectile it doesn't matter
				//MasterAudio.PlaySound (mEquippable.Sounds.SoundType, ToolDoppleganger.transform, mEquippable.Sounds.SoundUseUnuccessfully);
				mEquippable.UseUnsuccessfully ();
				ToolState = PlayerToolState.Equipped;
				yield break;
			}
			//convert the projectile object to a world item if it isn't already
			Projectile projectile = null;
			if (ProjectileObject.IsWorldItem) {
				//get the projectile from the existing worlditem
				projectile = ProjectileObject.worlditem.Get <Projectile> ();
			} else {
				WorldItem worlditemProjectile = null;
				//clone the projectile from the stack item
				WorldItems.CloneFromStackItem (ProjectileObject.GetStackItem (WIMode.Stacked), WIGroups.Get.Player, out worlditemProjectile);
				//initialize immediately
				worlditemProjectile.Initialize ();
				worlditemProjectile.transform.position = ToolActionPointObject.position;
				worlditemProjectile.transform.rotation = ToolActionPointObject.rotation;
				//set the projectile object to null
				ProjectileObject = null;
				//give it a second to initialze
				projectile = worlditemProjectile.Get <Projectile> ();
			}

			if (projectile != null) {
				projectile.Launch (ToolActionPointObject, weapon, LaunchForce);
				//play the launching sound
				mEquippable.UseSuccessfully ();
				//MasterAudio.PlaySound (mEquippable.Sounds.SoundType, ToolDoppleganger.transform, mEquippable.Sounds.SoundUseSuccessfully);
				yield return null;
			}
			RefreshToolDoppleganger (false);
			yield return null;
			if (UsesTensionMorph) {
				TensionChannel.Percent = 0f;
			}
			yield break;
		}

		protected IEnumerator CycleTool (ToolAction cycleType)
		{
			//Equippable equippable = null;
			if (worlditem.Is <Equippable> (out mEquippable)) {
				if (cycleType == ToolAction.CycleNext) {
					mEquippable.OnCycleNext.SafeInvoke ();
				} else {
					mEquippable.OnCyclePrev.SafeInvoke ();
				}
				while (!mEquippable.CyclingFinished) {
					yield return null;
				}
			}
			ToolState = PlayerToolState.Equipped;
		}

		protected IEnumerator FindAndLoadProjectile (Equippable equippable, Weapon weapon, bool playLoadAnimation)
		{
			//get the projectile - if we don't find one, we don't need to do anything else
			if (!player.Inventory.FindProjectileForWeapon (weapon, out ProjectileObject, out ProjectileStack)) {
				//this will shut off the doppleganger
				//RefreshToolDoppleganger (false);
				yield break;
			}
			//otherwise move on
			//if playLoadAnimation is true, play the unequip/equip animations to simulate reaching down for the projectile
			if (playLoadAnimation) {
				yield return StartCoroutine (PlayAnimation ("ToolGenericUnequip"));
			}
			//refresh the doppleganger here to add it to the weapon
			RefreshToolDoppleganger (false);
			//if we're playing the animation, we have to play the 'equip' animation here to bring the weapon back up
			if (playLoadAnimation) {
				yield return StartCoroutine (PlayAnimation ("ToolGenericEquip"));
			}
			ToolState = PlayerToolState.Equipped;
			yield break;
		}

		#endregion

		#region tool use

		public bool CanUse ()
		{
			if (GameManager.Is (FGameState.InGame) && !player.IsDead && !player.IsHijacked) {
				return true;
			}
			return false;
		}

		public bool CanCycle ()
		{
			//Equippable equippable = null;
			if (worlditem.Is <Equippable> (out mEquippable)) {
				return mEquippable.CanCycle;
			}
			return false;
		}

		public virtual void OnFinishUsing ()
		{
			AddAction (ToolAction.UseFinish);
		}

		public virtual void OnCancelUsing ()
		{
			AddAction (ToolAction.UseFinish);
		}

		public virtual void TryToThrow ()
		{

		}

		public virtual void OnThrowing ()
		{
			 
		}

		public void OnSwingFinish ()
		{
			OnFinishUsing ();
		}

		protected IItemOfInterest mThingToHit = null;

		public bool OnSwingImpact ()
		{
			if (!HasWorldItem) {
				return false;
			}

			mThingToHit = null;
			if (player.Focus.IsFocusingOnSomething) {
				mThingToHit = player.Focus.LastFocusedObject;
			} else if (player.Surroundings.IsSomethingInRange) {
				mThingToHit = player.Surroundings.ClosestObjectInRange;
				if (player.Surroundings.IsWorldItemInRange) {
					//give priority to world items over other stuff
					//TODO verify this
					mThingToHit = player.Surroundings.WorldItemFocus;
				}
			}

			if (mThingToHit != null) {
				DamagePackage damage = null;
				Vector3 point = player.Surroundings.ClosestObjectInRangeHitInfo.point;
				//Weapon weapon = null;
				if (worlditem.Is <Weapon> (out mWeapon)) {
					damage = mWeapon.GetDamagePackage (point, mThingToHit);
				} else {
					damage = new DamagePackage ( );
					damage.Point = point;
					damage.SenderName = worlditem.Props.Name.DisplayName;
					damage.SenderMaterial = worlditem.Props.Global.MaterialType;
					damage.DamageSent = 1.0f;//absolute minimum
					damage.ForceSent = 1.0f;//absolute minimum
					damage.Target = mThingToHit;
				}
				damage.Source = Player.Local;
				DamageManager.Get.SendDamage (damage);

				if (damage.HitTarget) {
					//Equippable equippable = null;
					if (worlditem.Is <Equippable> (out mEquippable)) {
						mEquippable.UseSuccessfully ();
					}
				}
				
				OnFinishUsing ();
				return true;
			} else {
				//Equippable equippable = null;
				if (worlditem.Is <Equippable> (out mEquippable)) {
					mEquippable.UseFinish ();
				}
			}
			return false;
		}

		protected Equippable mEquippable = null;
		protected Weapon mWeapon = null;

		public virtual void OnHit ()
		{
			
		}

		public bool CanUnequip ()
		{
			bool canUnequip = true;
			if (HasWorldItem) {
				//Equippable equippable = null;
				if (worlditem.Is <Equippable> (out mEquippable)) {	//see if the equippable will allow us to
					//unequip (we use this for certain tools)
					if (mEquippable.ForceEquip) {
						canUnequip = false;
					}
				}
			}
			return canUnequip;
		}

		public bool CanEquip ()
		{
			if (!HasWorldItem && player.Inventory.HasActiveQuickslotItem) {
				worlditem = player.Inventory.ActiveQuickslotItem;
				return true;
			}
			return false;
		}

		public virtual bool Place ()
		{
			return false;
		}

		public virtual bool Place (GameObject recepticle, Vector3 surfacePoint, Vector3 surfaceNormal)
		{
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
				//Equippable equippable = null;
				if (worlditem.Is <Equippable> (out mEquippable)) {
					displayAsTool = mEquippable.DisplayAsTool;
					useDoppleganger = mEquippable.UseDoppleganger;
					//prep these just in case, doesn't cost us much
					equipPos += mEquippable.EquipOffset;
					equipRot += mEquippable.EquipRotation;
				}
				//now, are we actually using a doppleganger?
				if (!displayAsTool) {
					BreakDownToolDoppleganger ();
					return;
				} else if (!useDoppleganger) {
					//alright no doppleganger, but we still equip the worlditem as a tool
					BreakDownToolDoppleganger ();
					worlditem.LockTransform (tr);
					worlditem.tr.localPosition = equipPos;
					worlditem.tr.localRotation = Quaternion.Euler (equipRot);
					worlditem.rigidbody.isKinematic = true;
					ToolColliders.AddRange (worlditem.Colliders);
					//and we're done
					return;
				} else {
					//actually use doppleganger! here we go
					ToolDoppleganger = WorldItems.GetDoppleganger (worlditem, transform, ToolDoppleganger);
					ToolColliders.AddRange (ToolDoppleganger.GetComponentsInChildren <Collider> ());
					ToolActionPointObject = ToolDoppleganger.FindOrCreateChild ("ToolActionPointObject");
					//the equipped mode will apply equipped etc. offset
					//so don't bother to set that here
					ToolDoppleganger.SetActive (true);

					//get any tension morph animations
					TensionMorph = ToolDoppleganger.GetComponent <MegaMorph> ();
					if (TensionMorph != null) {
						TensionChannel = TensionMorph.GetChannel ("Tension");
					}

					//get any projectiles
					//Weapon weapon = null;
					if (worlditem.Is <Weapon> (out mWeapon)) {
						//update our action point
						//the action point is a gameobject in the weapon that indicates where hits collide
						//and/or where projectiles launch from - it's safe to access because it's a property
						ToolActionPointObject.transform.localPosition = mWeapon.ActionPointObject.transform.localPosition;
						ToolActionPointObject.transform.localRotation = mWeapon.ActionPointObject.transform.localRotation;

						if (HasProjectile) {
							//if we've found a projectile, create a doppleganger for it
							ProjectileDoppleganger = WorldItems.GetDoppleganger (
								ProjectileObject.PackName,
								ProjectileObject.PrefabName,
								ToolActionPointObject.transform, ProjectileDoppleganger,
								WIMode.Equipped,
								ProjectileObject.StackName,
								ProjectileObject.State,
								ProjectileObject.Subcategory,
								1f);
							ProjectileDoppleganger.SetActive (true);

						} else if (ProjectileDoppleganger != null) {
							ProjectileDoppleganger.SetActive (false);
						}
					} else if (ProjectileDoppleganger != null) {
						ProjectileDoppleganger.SetActive (false);
					}
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
		protected ParticleEmitter mParticleEmitter;
		protected float mTension = 0.0f;
	}

	public enum ToolAction
	{
		None,
		Equip,
		Unequip,
		UseStart,
		UseHold,
		UseRelease,
		UseFinish,
		Reload,
		Throw,
		CycleNext,
		CyclePrev,
	}

	[Serializable]
	public enum PlayerToolType
	{
		Generic,
		GenericUsable,
		PathEditor,
		CustomAction,
	}

	public enum PlayerToolState
	{
		Equipping,
		Equipped,
		CancelEquip,
		Unequipping,
		Unequipped,
		LoadingProjectile,
		LaunchingProjectile,
		IncreasingForce,
		ReleasingForce,
		InMotion,
		InUse,
		Cycling,
	}

	public enum PlayerToolStyle
	{
		Swing,
		Slice,
		TensionRelease,
		ProjectileLaunch,
		Static,
	}
}
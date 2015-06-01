using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class ReceptaclePivot : MonoBehaviour
	{
		public Receptacle ParentReceptacle = null;
		public ReceptaclePivotState	State = null;
		public GameObject OccupantDoppleganger = null;
		public Transform tr;
		public bool UsingSkillList = false;
		public static bool RemovingItemUsingSkill = false;

		public Bounds PivotBounds {
			get {
				return mBounds;
			}
		}

		protected Bounds mBounds;

		public ReceptacleSettings Settings {
			get {
				if (!State.InheritParentSettings) {
					return State.Settings;
				}
				return ParentReceptacle.State.Settings;
			}
		}

		public bool IsRemote {
			get {
				return tr.parent != ParentReceptacle.transform;
			}
		}

		public WorldItem Occupant {
			get {
				if (mOccupant == null) {
					if (mParentStack.HasTopItem) {
						IWIBase topItem = mParentStack.TopItem;
						if (!topItem.IsWorldItem) {
							Stacks.Convert.TopItemToWorldItem (mParentStack, out mOccupant);
							mOccupant.OnRemoveFromStack += ParentReceptacle.Refresh;
							mOccupant.OnUnloaded += ParentReceptacle.Refresh;
							mOccupant.OnAddedToPlayerInventory += ParentReceptacle.Refresh;
						} else {
							mOccupant = topItem.worlditem;
						}
					}
				}
				return mOccupant;
			}
		}

		protected WorldItem mOccupant = null;

		public void OnInitialized ()
		{
			tr = transform;

			if (State.InheritParentSettings) {
				State.Settings = ParentReceptacle.State.Settings;
			}

			mRefreshAction = Refresh;
			if (State.Index < ParentReceptacle.worlditem.StackContainer.StackList.Count) {
				mParentStack = ParentReceptacle.worlditem.StackContainer.StackList [State.Index];//subscribe to changes in the stack
				//subscribe to changes in the stack
				mParentStack.RefreshAction += mRefreshAction;
			}

			mBounds.center = tr.position + Vector3.up * 0.05f;
			mBounds.size = Vector3.one * 0.1f;

			name = ParentReceptacle.name + "_" + State.Index;
			State.Offset.ApplyTo (transform);
			Refresh ();
		}

		public void FocusOnOccupant (bool focus)
		{
			if (IsOccupied) {
				if (OccupantDoppleganger != null) {
					PlayerFocusHighlight.FocusHighlightDoppleganger (OccupantDoppleganger, focus);
				} else {
					if (focus) {
						Occupant.OnGainPlayerFocus.SafeInvoke ();
					} else {
						Occupant.OnLosePlayerFocus.SafeInvoke ();
					}
				}
			}
		}

		public void LockOccupant (bool locked)
		{
			if (IsOccupied) {
				if (locked && !Occupant.LockedParent == tr) {
					if (!Occupant.LockTransform (tr)) {
						Debug.Log ("Couldn't lock occupant, already locked by " + Occupant.LockedParent.name);
					}
				} else { 
					if (!Occupant.UnlockTransform (tr)) {
						if (Occupant.LockedParent != null) {
							Debug.Log ("already locked by " + Occupant.LockedParent.name);
						}
					}
				}
			}
		}

		public void Refresh ()
		{
			if (mRefreshing) {
				return;
			}

			mRefreshing = true;
			if (IsOccupied) {
				//check to see if we're still actually occupied
				if (Occupant != mParentStack.TopItem) {
					mOccupant = null;
				} else {
					//lock the transform to make sure it won't be moved by its group
					LockOccupant (true);
					Occupant.ParentPivot = this;
					Occupant.SetMode (WIMode.Placed);
					Occupant.LastActiveDistanceToPlayer = Occupant.ActiveRadius * 0.5f;
					Occupant.ActiveStateLocked = false;
					Occupant.ActiveState = WIActiveState.Active;
					Occupant.worlditem.tr.localPosition = Occupant.worlditem.BasePivotOffset;
					Occupant.worlditem.tr.localRotation = Quaternion.identity;

					mBounds.center = Occupant.Position;
					mBounds.size = Vector3.one * 0.001f;

					for (int i = 0; i < Occupant.Renderers.Count; i++) {
						if (Occupant.Renderers [i].enabled) {
							mBounds.Encapsulate (Occupant.Renderers [i].bounds);
						}
					}

					if (OccupantDoppleganger != null) {
						GameObject.Destroy (OccupantDoppleganger);
					}
				}
			} else if (OccupantDoppleganger != null) {
				GameObject.Destroy (OccupantDoppleganger);
				mBounds.center = tr.position + Vector3.up * 0.05f;
				mBounds.size = Vector3.one * 0.1f;
			}

			mRefreshing = false;
		}

		protected bool mRefreshing = false;

		public bool IsOccupied {
			get {
				if (mParentStack == null) {
					mParentStack = ParentReceptacle.worlditem.StackContainer;
				}
				if (mParentStack.HasTopItem && !mParentStack.TopItem.Is (WIMode.RemovedFromGame)) {
					return true;
				} else {
					mOccupant = null;
					return false;
				}
			}
		}

		protected void CreatePlacementDoppleganger ()
		{
			OccupantDoppleganger = WorldItems.GetDoppleganger (Occupant, transform, OccupantDoppleganger, WIMode.Placing);
		}

		public void OnDrawGizmos ()
		{
			Gizmos.color = Colors.Alpha (Color.blue, 0.1f);
			Gizmos.DrawSphere (transform.position, WorldItems.WISizeToFloat (State.MaxSize));
			Gizmos.DrawWireSphere (transform.position, 0.1f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, transform.position + transform.up);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine (transform.position, transform.position + transform.forward);
			Gizmos.color = Colors.Alpha (Color.yellow, 0.5f);
			Gizmos.DrawCube (mBounds.center, mBounds.size);
		}

		protected void UseSkillsToPickUpItem ( )
		{
			if (UsingSkillList)
				return;

			//add the option list we'll use to select the skill
			SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList> ();
			optionsList.Message = "Use a skill";
			optionsList.MessageType = "Pick up item";
			optionsList.FunctionName = "OnSelectRemoveSkill";
			optionsList.RequireManualEnable = false;
			optionsList.OverrideBaseAvailabilty = true;
			optionsList.FunctionTarget = gameObject;
			optionsList.ScreenTarget = transform;
			optionsList.ScreenTargetCamera = GameManager.Get.GameCamera.camera;
			mRemoveSkillList.Clear ();
			mRemoveSkillList.AddRange (Skills.Get.SkillsByName (mRemoveItemSkillNames));
			foreach (Skill removeItemSkill in mRemoveSkillList) {
				optionsList.AddOption (removeItemSkill.GetListOption (mSkillUseTarget.worlditem));
			}
			optionsList.AddOption (new WIListOption ("Cancel"));
			optionsList.ShowDoppleganger = false;
			GUIOptionListDialog dialog = null;
						if (optionsList.TryToSpawn (true, out dialog, null)) {
				UsingSkillList = true;
			}
		}

		public void OnSelectRemoveSkill (System.Object result)
		{
			UsingSkillList = false;

			WIListResult dialogResult = result as WIListResult;
			RemoveItemSkill skillToUse = null;
			foreach (Skill removeSkill in mRemoveSkillList) {
				if (removeSkill.name == dialogResult.Result) {
					skillToUse = removeSkill as RemoveItemSkill;
					break;
				}
			}

			if (skillToUse != null) {
				//set this global flag to true
				//this will prevent anything from closing
				RemovingItemUsingSkill = true;
				//SKILL USE
				//getting here guarantees that:
				//a) our selected stack is empty and
				//b) our stack has item
				//so proceed as though we know those are true
				skillToUse.TryToRemoveItem (mSkillUseTarget, Occupant, Player.Local.Inventory, ParentReceptacle.OnItemRemovedFromReceptacle, dialogResult.SecondaryResultFlavor);
				//now we just have to wait!
				//the skill will move stuff around
				//refresh requests will be automatic
			}

			mRemoveItemSkillNames.Clear ();
		}

		protected static HashSet <string> mRemoveItemSkillNames = new HashSet <string> ();
		protected static List <Skill> mRemoveSkillList = new List <Skill> ( );
		protected Action mRefreshAction = null;
		protected WIStack mParentStack = null;
		protected IStackOwner mSkillUseTarget = null;
		protected WorldItem mPickUpTarget = null;
	}

	[Serializable]
	public class ReceptaclePivotState
	{
		public int Index = 0;
		public bool InheritParentSettings = true;
		[HideInInspector]
		public STransform Offset = STransform.zero;
		public WISize MaxSize = WISize.Large;
		public ReceptacleSettings Settings = null;
		public BodyOrientation Orientation = BodyOrientation.Both;
	}

	[Serializable]
	public class ReceptacleSettings
	{
		public WISize MaxSize = WISize.Large;
		public List <string> PermittedItems = new List <string> ();
		public List <string> PermittedScripts = new List <string> ();
		public List <string> PermittedSubcats = new List <string> ();
		[BitMask (typeof(WIMaterialType))]
		public WIMaterialType PermittedMaterials = WIMaterialType.None;
	}
}
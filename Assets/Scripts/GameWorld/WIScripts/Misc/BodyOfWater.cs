using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class BodyOfWater : WIScript, IBodyOfWater
	{
		public BodyOfWaterState State = new BodyOfWaterState ();
		public GameObject WaterPivot;
		public GameObject WaterBottomCollider;
		public GameObject WaterTriggerCollider;
		public GameObject WaterSurface;
		public WaterAnimScrolling WaterAnimation;
		public List <SubmergedObject> SubmergedObjects = new List <SubmergedObject> ();
		public WaterSubmergeObjects SubmergeTrigger;
		public WaterLeviathan Leviathan = null;
		BoxCollider surfaceCollider;

		public float WaterHeightAtPosition (Vector3 position)
		{ 
			return mWaterHeight;
		}

		public float TargetWaterLevel {
			get {
				return State.TargetWaterLevel;
			}
			set {
				State.TargetWaterLevel = value;
			}
		}

		public override void Awake ()
		{
			base.Awake ();
			surfaceCollider = WaterTriggerCollider.GetComponent <BoxCollider> (); 
		}

		public override void OnInitialized ()
		{
			WaterSubmergeObjects wso = WaterTriggerCollider.GetComponent <WaterSubmergeObjects> ();
			mTriggerCollider = wso.GetComponent <BoxCollider> ();
			wso.Water = this;

			base.OnInitialized ();
			Vector3 scale = new Vector3 (State.Scale, 1f, State.Scale);
			WaterPivot.transform.localScale = scale;
			if (State.UseTrigger) {
				WaterTriggerCollider.layer = Globals.LayerNumFluidTerrain;
			} else {
				WaterTriggerCollider.layer = Globals.LayerNumHidden;
			}
			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundEnter, new ActionListener (LocationUndergroundEnter));
			Player.Get.AvatarActions.Subscribe (AvatarAction.LocationUndergroundEnter, new ActionListener (LocationUndergroundExit));

			mWaterLevelPosition = WaterPivot.transform.localPosition;
			mWaterLevelPosition.y = State.CurrentWaterLevel;
			WaterPivot.transform.localPosition = mWaterLevelPosition;

			worlditem.OnAddedToGroup += OnAddedToGroup;
			SubmergeTrigger.OnItemOfInterestEnterWater += OnItemOfInterestEnterWater;
			SubmergeTrigger.OnItemOfInterestExitWater += OnItemOfInterestExitWater;
		}

		public void OnAddedToGroup () {
			mWaterHeight = WaterSurface.transform.position.y;
		}

		public void Update ()
		{
			if (!Mathf.Approximately (State.CurrentWaterLevel, State.TargetWaterLevel)) {
				State.CurrentWaterLevel = Mathf.Lerp (State.CurrentWaterLevel, State.TargetWaterLevel, (float)(WorldClock.ARTDeltaTime * State.WaterLevelChangeSpeed));
				mWaterLevelPosition.y = State.CurrentWaterLevel;
				WaterPivot.transform.localPosition = mWaterLevelPosition;
			}
			mWaterHeight = WaterSurface.transform.position.y;
		}

		protected float mWaterHeight;

		public bool LocationUndergroundEnter (double timeStamp)
		{
			WIActiveState activeState = WIActiveState.Active;
			if (worlditem.Group.Props.TerrainType == LocationTerrainType.AboveGround) {
				activeState = WIActiveState.Invisible;
			}
			worlditem.ActiveStateLocked = false;
			worlditem.ActiveState = activeState;
			worlditem.ActiveStateLocked = true;
			return true;
		}

		public bool SortSubmergedObjects (IItemOfInterest seeker, double interestInterval, out SubmergedObject subjergedObject)
		{
			subjergedObject = null;
			for (int i = SubmergedObjects.LastIndex (); i >= 0; i--) {
				SubmergedObject subObj = SubmergedObjects [i];
				if (subObj == null || subObj.Target == null || subObj.Target.Destroyed || (subObj.Target.IOIType == ItemOfInterestType.WorldItem && subObj.Target.worlditem.Is <WaterTrap> ())) {
					SubmergedObjects.RemoveAt (i);
				} else {
					subObj.Seeker = seeker;
					if (subObj.Target.IOIType == ItemOfInterestType.Scenery) {
						//it's probably a fish or something
						//there's a very low probability that we care
						if (UnityEngine.Random.value < 0.0005f) {
							subObj.IsOfInterest = true;
						} else {
							subObj.IsOfInterest = false;
							SubmergedObjects.RemoveAt (i);
						}
					} else if (subObj.HasExitedWater && (WorldClock.AdjustedRealTime - subObj.TimeExitedWater) > interestInterval) {
						//just in case we're already targeting a submerged object
						subObj.IsOfInterest = false;
						SubmergedObjects.RemoveAt (i);
					} else if (subObj.Target.IOIType == ItemOfInterestType.Player && subObj.Target.player.IsDead) {
						subObj.IsOfInterest = false;
						SubmergedObjects.RemoveAt (i);
						/*} else if (subObj.Target.Position.y > mWaterHeight) {
						//if the target's position is higher than the water position then it can't be underwater
						subObj.IsOfInterest = true;
						SubmergedObjects.RemoveAt (i);*/
					} else {
						subObj.IsOfInterest = true;
					}
				}
			}
			if (SubmergedObjects.Count > 0) {
				SubmergedObjects.Sort ();
				if (SubmergedObjects [0].IsOfInterest) {
					subjergedObject = SubmergedObjects [0];
				}
			}
			return subjergedObject != null;
		}

		public void OnItemOfInterestEnterWater ()
		{
			if (Leviathan == null)
				return;

			IItemOfInterest target = SubmergeTrigger.LastSubmergedItemOfInterest;
			for (int i = 0; i < SubmergedObjects.Count; i++) {
				//reset instead of adding new
				if (SubmergedObjects [i].Target == target) {
					SubmergedObjects [i].TimeExitedWater = -1f;
					return;
				}
			}
			SubmergedObjects.Add (new SubmergedObject (Leviathan, target, (float)WorldClock.AdjustedRealTime));
		}

		public void OnItemOfInterestExitWater ()
		{
			if (Leviathan == null)
				return;

			IItemOfInterest target = SubmergeTrigger.LastExitedItemOfInterest;
			for (int i = SubmergedObjects.Count - 1; i >= 0; i--) {
				if (SubmergedObjects [i].Target == target) {
					SubmergedObjects [i].TimeExitedWater = (float)WorldClock.AdjustedRealTime;
					SubmergedObjects.RemoveAt (i);
					break;
				}
			}
		}

		public bool LocationUndergroundExit (double timeStamp)
		{
			WIActiveState activeState = WIActiveState.Active;
			if (worlditem.Group.Props.TerrainType == LocationTerrainType.BelowGround) {
				activeState = WIActiveState.Active;
			}
			worlditem.ActiveStateLocked = false;
			worlditem.ActiveState = activeState;
			worlditem.ActiveStateLocked = true;
			return true;
		}

		protected Vector3 mWaterLevelPosition;
		protected BoxCollider mTriggerCollider;
		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			State.Scale = WaterPivot.transform.localScale.x;
			State.CurrentWaterLevel = WaterPivot.transform.localPosition.y;
		}

		public override void OnEditorLoad ()
		{
			WaterPivot.transform.localScale = Vector3.one * State.Scale;
			WaterPivot.transform.localPosition = new Vector3 (0f, State.CurrentWaterLevel, 0f);
		}
		#endif
	}

	[Serializable]
	public class BodyOfWaterState
	{
		public float Scale = 1f;
		public float CurrentWaterLevel = 0f;
		public float TargetWaterLevel = 0f;
		public float WaterLevelChangeSpeed = 1f;
		public bool UseTrigger = true;
	}
}
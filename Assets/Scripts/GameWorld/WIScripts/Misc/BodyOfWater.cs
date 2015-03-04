using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class BodyOfWater : WIScript, IBodyOfWater
		{
				public BodyOfWaterState State = new BodyOfWaterState();
				public GameObject WaterPivot;
				public GameObject WaterBottomCollider;
				public GameObject WaterTriggerCollider;
				public GameObject WaterSurface;
				public WaterAnimScrolling WaterAnimation;

				public float WaterHeightAtPosition(Vector3 position)
				{ 
						return mTriggerCollider.bounds.max.y;
				}

				public float TargetWaterLevel {
						get {
								return State.TargetWaterLevel;
						}
						set {
								State.TargetWaterLevel = value;
						}
				}

				public override void OnInitialized()
				{
						WaterSubmergeObjects wso = WaterTriggerCollider.GetComponent <WaterSubmergeObjects>();
						mTriggerCollider = wso.GetComponent <BoxCollider>();
						wso.Water = this;

						base.OnInitialized();
						Vector3 scale = new Vector3(State.Scale, 1f, State.Scale);
						WaterPivot.transform.localScale = scale;
						if (State.UseTrigger) {
								WaterTriggerCollider.layer = Globals.LayerNumFluidTerrain;
						} else {
								WaterTriggerCollider.layer = Globals.LayerNumHidden;
						}
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationUndergroundEnter, new ActionListener(LocationUndergroundEnter));
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationUndergroundEnter, new ActionListener(LocationUndergroundExit));

						mWaterLevelPosition = WaterPivot.transform.localPosition;
						mWaterLevelPosition.y = State.CurrentWaterLevel;
						WaterPivot.transform.localPosition = mWaterLevelPosition;
				}

				public void Update()
				{
						if (!Mathf.Approximately(State.CurrentWaterLevel, State.TargetWaterLevel)) {
								State.CurrentWaterLevel = Mathf.Lerp(State.CurrentWaterLevel, State.TargetWaterLevel, (float)(WorldClock.ARTDeltaTime * State.WaterLevelChangeSpeed));
								mWaterLevelPosition.y = State.CurrentWaterLevel;
								WaterPivot.transform.localPosition = mWaterLevelPosition;
						}
				}

				public bool LocationUndergroundEnter(double timeStamp)
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

				public bool LocationUndergroundExit(double timeStamp)
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
				public override void OnEditorRefresh()
				{
						State.Scale = WaterPivot.transform.localScale.x;
						State.CurrentWaterLevel = WaterPivot.transform.localPosition.y;
				}

				public override void OnEditorLoad()
				{
						WaterPivot.transform.localScale = Vector3.one * State.Scale;
						WaterPivot.transform.localPosition = new Vector3(0f, State.CurrentWaterLevel, 0f);
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
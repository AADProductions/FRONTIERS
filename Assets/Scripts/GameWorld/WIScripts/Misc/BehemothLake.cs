using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers;

namespace Frontiers.World.WIScripts
{
		public class BehemothLake : WIScript
		{
				public BodyOfWater bodyofwater;
				public RiverAvatar river;
				public RiverAvatar riverEnd;
				public InlandLeviathan ActiveLeviathan;
				public Spline WanderPatternSpline;
				public Transform FollowSplineTransform;

				public bool HasActiveLeviathan {
						get {
								return ActiveLeviathan != null;
						}
				}

				public bool Wandering = false;
				public string RiverEndName = "Blacklake River 2";
				public string RiverName = "Blacklake River";
				public string MissionName = "Behemoth";
				public string VariableName = "SuccessfullyBlewUpDam";
				public BehemothLakeState State = new BehemothLakeState();

				public override void OnInitialized()
				{
						bodyofwater = worlditem.GetComponent <BodyOfWater>();

						bool flooded = false;
						MissionStatus status = MissionStatus.Dormant;
						if (Missions.Get.MissionStatusByName(MissionName, ref status)) {
								if (Flags.Check((uint)MissionStatus.Dormant, (uint)status, Flags.CheckType.MatchAny)) {
										//if the mission hasn't started, water level is normal
										//Debug.Log ("Mission status was dormant, water is flooded");
										flooded = true;
								} else {
										//if the mission has started OR been completed, water level is based on variable
										if (Missions.Get.GetVariableValue(MissionName, VariableName) <= 0) {
												//Debug.Log ("Mission variable was less than or equal to zero, water is flooded");
												flooded = true;
										}
								}
						}

						if (flooded) {
								worlditem.OnVisible += OnVisible;
								worlditem.OnInvisible += OnInvisible;
						}

						StartCoroutine (SetFlooded(flooded));

						if (flooded) {
								Player.Get.AvatarActions.Subscribe(AvatarAction.MissionVariableChange, new ActionListener(MissionVariableChange));
						}
				}

				protected IEnumerator SetFlooded(bool flooded)
				{
						while (worlditem.Group == null) {
								yield return null;
						}

						while (river == null) {
								worlditem.Group.GetParentChunk().GetRiver(RiverName, out river);
								worlditem.Group.GetParentChunk().GetRiver(RiverEndName, out riverEnd);
								yield return null;
						}

						if (flooded) {
								//	Debug.Log ("SETTING FLOODED");
								//the dam still isn't blown, make everything chaos
								bodyofwater.TargetWaterLevel = State.LakeWaterLevelHigh;
								river.TargetWaterLevel = State.RiverWaterLevelHigh;
								riverEnd.TargetWaterLevel = State.RiverEndWaterLevelLow;

								FollowSplineTransform = gameObject.CreateChild("FollowSplineTransform");
								Transform wanderPatternSplineTransform = gameObject.CreateChild("WanderPatternSpline");
								/*WanderPatternSpline = wanderPatternSplineTransform.gameObject.AddComponent <Spline>();
								for (int i = 0; i < State.WanderPatternNodes.Count; i++) {
										GameObject node = WanderPatternSpline.AddSplineNode();
										node.transform.parent = WanderPatternSpline.transform;
										node.transform.localPosition = State.WanderPatternNodes[i];
								}
								WanderPatternSpline.UpdateSpline();
								WanderPatternSpline.updateMode = Spline.UpdateMode.DontUpdate;
								WanderPatternSpline.autoClose = true;
								WanderPatternSpline.rotationMode = Spline.RotationMode.Tangent;
								WanderPatternSpline.interpolationMode = Spline.InterpolationMode.Hermite;
								WanderPatternSpline.normalMode = Spline.NormalMode.UseGlobalSplineNormal;*/

								enabled = true;
						} else {
								//Debug.Log ("SETTING NORMAL");
								//we've blown up the dam and everything's fine, set the bodies of water to normal
								bodyofwater.TargetWaterLevel = State.LakeWaterLevelNormal;
								river.TargetWaterLevel = State.RiverWaterLevelNormal;
								riverEnd.TargetWaterLevel = State.RiverEndWaterLevelNormal;

								if (HasActiveLeviathan != null) {
										GameObject.Destroy(ActiveLeviathan.gameObject);
								}
								if (WanderPatternSpline != null) {
										GameObject.Destroy(WanderPatternSpline.gameObject);
										GameObject.Destroy(FollowSplineTransform.gameObject);
								}
								enabled = false;
						}
				}

				public bool MissionVariableChange(double timeStamp)
				{
						if (Missions.Get.GetVariableValue(MissionName, VariableName) > 0) {
								SetFlooded(false);
						}
						return true;
				}

				public void OnInvisible()
				{

				}

				public void OnVisible()
				{

						if (!HasActiveLeviathan) {
								GameObject leviathanGameObject = GameObject.Instantiate(Ocean.Get.InlandLeviathanPrefab, worlditem.Position, Quaternion.identity) as GameObject;
								ActiveLeviathan = leviathanGameObject.GetComponent <InlandLeviathan>();
						}

						if (!mUpdatingLeviathanBehavior) {
								mUpdatingLeviathanBehavior = true;
								StartCoroutine(UpdateLeviathanBehavior());
						}
				}

				public void Update()
				{
						if (HasActiveLeviathan) {
								//move the wander position regardless of what the leviathan is doing
								State.LastWanderParam += (float)(State.WanderSpeed * WorldClock.ARTDeltaTime);
								//FollowSplineTransform.position = WanderPatternSpline.GetPositionOnSpline(State.LastWanderParam);
								ActiveLeviathan.transform.position = FollowSplineTransform.position;
						}
				}

				protected IEnumerator UpdateLeviathanBehavior()
				{
						while (worlditem.Is(WIActiveState.Visible | WIActiveState.Active)) {
								yield return null;
								if (!HasActiveLeviathan) {
										yield break;
								}
						}
						yield break;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.WanderPatternNodes.Clear();
						foreach (SplineNode node in WanderPatternSpline.splineNodesArray) {
								State.WanderPatternNodes.Add(node.transform.localPosition);
						}
				}

				public void OnDrawGizmos()
				{
						if (FollowSplineTransform != null) {
								Gizmos.color = Color.red;
								Gizmos.DrawSphere(FollowSplineTransform.position, 1f);
						}
				}
				#endif
				protected bool mUpdatingLeviathanBehavior = false;
		}

		[Serializable]
		public class BehemothLakeState
		{
				public List <SVector3> WanderPatternNodes = new List <SVector3>();
				public float LastWanderParam = 0f;
				public float WanderSpeed = 0.1f;
				public float LakeWaterLevelHigh;
				public float LakeWaterLevelNormal;
				public float RiverWaterLevelHigh;
				public float RiverWaterLevelNormal;
				public float RiverEndWaterLevelLow;
				public float RiverEndWaterLevelNormal;
		}
}

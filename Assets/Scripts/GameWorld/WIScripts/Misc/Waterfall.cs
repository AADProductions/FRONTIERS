using UnityEngine;
using System;
using System.Collections;

namespace Frontiers.World
{
		public class Waterfall : WIScript
		{
				public WaterfallState State = new WaterfallState();
				#if UNITY_EDITOR
				public BodyOfWater TopTargetLevelBoW;
				public BodyOfWater BottomTargetLevelBoW;
				public RiverAvatar TopTargetLevelRiver;
				public RiverAvatar BottomTargetLevelRiver;
				#endif
				public Transform WaterfallTop;
				public Transform WaterfallBottom;
				public SphereCollider WaterfallTopCollider;
				public SphereCollider WaterfallBottomCollider;
				public ParticleSystem FlowBottom;
				public ParticleSystem MistBottom;
				public ParticleSystem SplashBottom;
				public ParticleSystem FoamBottom;
				public ParticleSystem FlowTop;
				public ParticleSystem FallTop;
				public ParticleSystem SprayTop;
				public ParticleSystem StreakTop;
				public Transform WaterfallTopFollow;
				public Transform WaterfallBottomFollow;

				public bool TopFollowsSomething {
						get {
								return WaterfallTopFollow != null;
						}
				}

				public bool BottomFollowsSomething {
						get {
								return WaterfallBottomFollow != null;
						}
				}

				public bool FallEnabled {
						get {
								return Mathf.Abs(WaterfallTop.position.y - WaterfallBottom.position.y) > MinFallHeight;
						}
				}

				public float MinFallHeight = 1f;

				public override void OnInitialized()
				{
						if (State.TopFollowsSeaLevel) {
								WaterfallTopFollow = Ocean.Get.Pivot;
						} else if (!string.IsNullOrEmpty(State.TopRiverName)) {
								RiverAvatar river = null;
								if (worlditem.Group.GetParentChunk().GetRiver(State.TopRiverName, out river)) {
										WaterfallTopFollow = river.transform;
								}
						} else if (!string.IsNullOrEmpty(State.TopBodyOfWaterPath)) {
								mWaitingForBodiesOfWater = true;
								StartCoroutine(WaitForBodiesOfWater());
						}

						if (State.BottomFollowsSeaLevel) {
								WaterfallBottomFollow = Ocean.Get.Pivot;
						} else if (!string.IsNullOrEmpty(State.BottomRiverName)) {
								RiverAvatar river = null;
								if (worlditem.Group.GetParentChunk().GetRiver(State.BottomRiverName, out river)) {
										WaterfallBottomFollow = river.transform;
								}
						} else if (!string.IsNullOrEmpty(State.BottomBodyOfWaterPath)) {
								if (!mWaitingForBodiesOfWater) {
										mWaitingForBodiesOfWater = true;
										StartCoroutine(WaitForBodiesOfWater());
								}
						}

						State.TopFall.ApplyTo(FallTop.transform, true);
						State.TopFlow.ApplyTo(FlowTop.transform, true);
						State.TopSpray.ApplyTo(SprayTop.transform, true);
						State.TopStreak.ApplyTo(StreakTop.transform, true);
						State.TopPosition.ApplyTo(WaterfallTop, false);

						State.BottomFlow.ApplyTo(FlowBottom.transform, true);
						State.BottomFoam.ApplyTo(FoamBottom.transform, true);
						State.BottomMist.ApplyTo(MistBottom.transform, true);
						State.BottomSplash.ApplyTo(SplashBottom.transform, true);
						State.BottomPosition.ApplyTo(WaterfallBottom, false);

						FallTop.emissionRate = State.TopFallEmissionRate;
						FlowTop.emissionRate = State.TopFlowEmissionRate;
						SprayTop.emissionRate = State.TopSprayEmissionRate;
						StreakTop.emissionRate = State.TopStreakEmissionRate;

						FlowBottom.emissionRate = State.BottomFlowEmissionRate;
						FoamBottom.emissionRate = State.BottomFoamEmissionRate;
						MistBottom.emissionRate = State.BottomMistEmissionRate;
						SplashBottom.emissionRate = State.BottomSplashEmissionRate;

						if (TopFollowsSomething || BottomFollowsSomething) {
								enabled = true;
						}
				}

				protected IEnumerator WaitForBodiesOfWater()
				{
						bool waitForTop = !string.IsNullOrEmpty(State.TopBodyOfWaterPath);
						bool waitForBottom = !string.IsNullOrEmpty(State.BottomBodyOfWaterPath);

						while (waitForTop | waitForBottom) {
								if (waitForTop) {
										WorldItem bodyOfWaterWorlditem = null;
										if (WIGroups.FindChildItem(State.TopBodyOfWaterPath, out bodyOfWaterWorlditem)) {
												BodyOfWater bodyOfWater = bodyOfWaterWorlditem.Get <BodyOfWater>();
												WaterfallTopFollow = bodyOfWater.WaterPivot.transform;
												waitForTop = false;
										}
								}
								if (waitForBottom) {
										WorldItem bodyOfWaterWorlditem = null;
										if (WIGroups.FindChildItem(State.BottomBodyOfWaterPath, out bodyOfWaterWorlditem)) {
												BodyOfWater bodyOfWater = bodyOfWaterWorlditem.Get <BodyOfWater>();
												WaterfallBottomFollow = bodyOfWater.WaterPivot.transform;
												waitForBottom = false;
										}
								}
								//Debug.Log ("Waiting for top / bottom: " + waitForTop.ToString () + " / " + waitForBottom.ToString ());
								yield return new WaitForSeconds(0.5f);
						}
						enabled = true;
						yield break;
				}

				protected static Color gStartColor;

				public void LateUpdate()
				{
						if (BottomFollowsSomething) {
								mBottomPosition = WaterfallBottom.position;
								mBottomPosition.y = WaterfallBottomFollow.position.y;// + worlditem.tr.position.y;
								WaterfallBottom.position = mBottomPosition;
						}
						if (TopFollowsSomething) {
								mTopPosition = WaterfallTop.position;
								mTopPosition.y = WaterfallTopFollow.position.y;
								WaterfallTop.position = mTopPosition;
						}

						gStartColor = Color.Lerp(Color.white, Colors.Alpha(RenderSettings.fogColor, 1f), 0.5f);

						FlowBottom.startColor = gStartColor;
						MistBottom.startColor = gStartColor;
						SplashBottom.startColor = gStartColor;
						FoamBottom.startColor = gStartColor;

						FlowTop.startColor = gStartColor;
						FallTop.startColor = gStartColor;
						SprayTop.startColor = gStartColor;
						StreakTop.startColor = gStartColor;

						if (FallEnabled) {
								StreakTop.enableEmission = true;
								SprayTop.enableEmission = true;
								SplashBottom.enableEmission = true;
								MistBottom.enableEmission = true;
						} else {
								StreakTop.enableEmission = false;
								SprayTop.enableEmission = false;
								SplashBottom.enableEmission = false;
								MistBottom.enableEmission = false;
						}
				}

				protected Vector3 mBottomPosition;
				protected Vector3 mTopPosition;
				protected bool mWaitingForBodiesOfWater = false;
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						WaterfallTop.localPosition = Vector3.zero;
						WaterfallTop.localRotation = Quaternion.identity;

						State.TopRadius = WaterfallTopCollider.radius;
						State.BottomRadius = WaterfallBottomCollider.radius;

						State.TopFall.CopyFrom(FallTop.transform);
						State.TopFlow.CopyFrom(FlowTop.transform);
						State.TopSpray.CopyFrom(SprayTop.transform);
						State.TopStreak.CopyFrom(StreakTop.transform);
						State.TopPosition.CopyFrom(WaterfallTop);

						State.BottomFlow.CopyFrom(FlowBottom.transform);
						State.BottomFoam.CopyFrom(FoamBottom.transform);
						State.BottomMist.CopyFrom(MistBottom.transform);
						State.BottomSplash.CopyFrom(SplashBottom.transform);
						State.BottomPosition.CopyFrom(WaterfallBottom);

						State.TopFallEmissionRate = FallTop.emissionRate;
						State.TopFlowEmissionRate = FlowTop.emissionRate;
						State.TopSprayEmissionRate = SprayTop.emissionRate;
						State.TopStreakEmissionRate = StreakTop.emissionRate;

						State.BottomFlowEmissionRate = FlowBottom.emissionRate;
						State.BottomFoamEmissionRate = FoamBottom.emissionRate;
						State.BottomMistEmissionRate = MistBottom.emissionRate;
						State.BottomSplashEmissionRate = SplashBottom.emissionRate;

						if (TopTargetLevelBoW != null) {
								State.TopBodyOfWaterPath = TopTargetLevelBoW.worlditem.StaticReference.FullPath;
						}
						if (BottomTargetLevelBoW != null) {
								State.BottomBodyOfWaterPath = BottomTargetLevelBoW.worlditem.StaticReference.FullPath;
						}
						if (TopTargetLevelRiver != null) {
								State.TopRiverName = TopTargetLevelRiver.Props.Name;
						}
						if (BottomTargetLevelRiver != null) {
								State.BottomRiverName = BottomTargetLevelRiver.Props.Name;
						}
				}
				#endif
				public void OnDrawGizmos()
				{
						Gizmos.color = Color.blue;
						DrawArrow.ForGizmo(WaterfallTop.position, WaterfallTop.forward * 2);
						DrawArrow.ForGizmo(WaterfallTop.position + WaterfallTop.forward, -WaterfallBottom.up * 2);
						DrawArrow.ForGizmo(WaterfallBottom.position - WaterfallBottom.forward + WaterfallBottom.up, -WaterfallBottom.up * 2);
						DrawArrow.ForGizmo(WaterfallBottom.position, WaterfallBottom.forward * 2);
				}
		}

		[Serializable]
		public class WaterfallState
		{
				public float TopRadius;
				public float BottomRadius;
				public STransform TopFlow = new STransform();
				public STransform TopFall = new STransform();
				public STransform TopSpray = new STransform();
				public STransform TopStreak = new STransform();
				public STransform TopPosition = new STransform();
				public STransform BottomFlow = new STransform();
				public STransform BottomSplash = new STransform();
				public STransform BottomMist = new STransform();
				public STransform BottomFoam = new STransform();
				public STransform BottomPosition = new STransform();
				public float TopFlowEmissionRate;
				public float TopFallEmissionRate;
				public float TopSprayEmissionRate;
				public float TopStreakEmissionRate;
				public float BottomFlowEmissionRate;
				public float BottomSplashEmissionRate;
				public float BottomMistEmissionRate;
				public float BottomFoamEmissionRate;
				public bool BottomFollowsSeaLevel = false;
				public bool TopFollowsSeaLevel = false;
				public string TopBodyOfWaterPath;
				public string BottomBodyOfWaterPath;
				public string TopRiverName;
				public string BottomRiverName;
		}
}

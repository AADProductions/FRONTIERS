using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class Thermal : WIScript
		{
				public ThermalState State = new ThermalState();

				public GameObject ThermalFX;
				public CapsuleCollider Collider = null;
				public Transform Audio;
				public Visitable visitable = null;
				public Transform Pivot;
				public List <Transform> PivotPoints = new List <Transform>();
				public bool IsMobile = false;

				public override bool EnableAutomatically {
						get {
								return true;
						}
				}
				public override void OnInitialized()
				{
						Location location = worlditem.Get <Location>();
						location.State.UnloadOnInvisible = false;

						Collider.radius = State.Radius;
						Collider.height = State.Height;
						Collider.center = new Vector3(0f, State.Offset, 0f);
						Collider.direction = 1;

						//turn it on and keep it on
						worlditem.ActiveState = WIActiveState.Active;
						worlditem.ActiveStateLocked = true;

						visitable = worlditem.Get <Visitable>();

						ThermalFX = FXManager.Get.GetOrSpawnFx(ThermalFX, Pivot.gameObject, State.ThermalFXName);

						if (State.PivotPoints.Count > 0) {
								IsMobile = true;
								PivotPoints.Clear();
								for (int i = 0; i < State.PivotPoints.Count; i++) {
										Transform pivotPointTranform = gameObject.FindOrCreateChild("PivotPoint-" + i.ToString());
										pivotPointTranform.localPosition = State.PivotPoints[i];
										PivotPoints.Add(pivotPointTranform);
								}
						} else {
								IsMobile = false;
						}

						GetNextTarget();
				}

				public bool HasReachedTarget {
						get {
								if (mCurrentTarget != null && Pivot != null) {
										return Vector3.Distance(Pivot.position, mCurrentTarget) < 1f;
								}
								return false;
						}
				}

				public void GetNextTarget()
				{
						if (IsMobile) {
								mCurrentPivot = PivotPoints.NextIndex(mCurrentPivot);
								mCurrentTarget = PivotPoints[mCurrentPivot].position;
						}
				}

				public void Update()
				{
						if (IsMobile) {
								//move the thermal
								Pivot.position = Vector3.MoveTowards(Pivot.position, mCurrentTarget, (float)(WorldClock.ARTDeltaTime * (State.MovementSpeed * gThermalSpeedMultiplier)));
						}
				}

				public void FixedUpdate()
				{
						if (!mInitialized) {
								return;
						}

						if (IsMobile && HasReachedTarget) {
								GetNextTarget();
						}

						if (!visitable.IsVisiting) {
								mSmoothForce = 0f;
								Audio.localPosition = Vector3.Lerp(Audio.localPosition, Vector3.zero, (float)(WorldClock.ARTDeltaTime * 0.05f));
								return;
						}

						if (Player.Local.FPSController.IsMounted) {
								Vehicle mount = Player.Local.FPSController.Mount;
								if (mount.MountType == PlayerMountType.Air) {
										float normalizedY = (Player.Local.Position.y - Pivot.position.y) / Collider.height;
										float strength = Mathf.Lerp(State.ThermalStrengthBottom, State.ThermalStrengthTop, normalizedY);
										mSmoothForce = Mathf.Lerp(mSmoothForce, strength, (float)(WorldClock.ARTDeltaTime * gThermalSmoothForceMultiplier)) * gThermalForceMultiplier;
										Player.Local.FPSController.AddForce(Vector3.up * (mSmoothForce));
										Player.Local.FPSController.FallSpeed = 0f;
										GameWorld.Get.AddTemperatureOverride(GameWorld.ClampTemperature(State.Temperature, mount.MinTemperature, mount.MaxTemperature), 1.0f);

										Audio.localPosition = Vector3.zero;
										Vector3 audioPosition = Audio.position;
										audioPosition.y = Player.Local.Position.y;
										Audio.position = Vector3.Lerp(Audio.position, audioPosition, Time.deltaTime * 0.05f);
								}
						}
				}

				public void OnDrawGizmos()
				{
						Vector3 currentPosition = Vector3.zero;
						Vector3 lastPosition = Vector3.zero;
						Vector3 firstPosition = Vector3.zero;

						for (int i = 0; i < PivotPoints.Count; i++) {
								currentPosition = PivotPoints[i].position;
								Gizmos.color = Colors.Alpha(Colors.Saturate(Colors.ColorFromString(name, 100)), 0.25f);
								Gizmos.DrawSphere(currentPosition, Collider.radius);
								if (i == 0) {
										firstPosition = currentPosition;
								} else {
										Gizmos.color = Colors.Saturate(Colors.ColorFromString(name, 100));
										Gizmos.DrawLine(lastPosition, currentPosition);
										if (i == PivotPoints.LastIndex()) {
												Gizmos.DrawLine(currentPosition, firstPosition);
										}
								}
								lastPosition = currentPosition;
						}
				}

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.Radius = Collider.radius;
						State.Height = Collider.height;
						State.Offset = Collider.center.y;

						State.PivotPoints.Clear();
						for (int i = 0; i < PivotPoints.Count; i++) {
								State.PivotPoints.Add(new SVector3(PivotPoints[i].localPosition));
						}
				}
				#endif

				protected Vector3 mCurrentTarget = Vector3.zero;
				protected float mSmoothForce = 0f;
				protected int mCurrentPivot = 0;
				protected static float gThermalSmoothForceMultiplier = 2f;
				protected static float gThermalSpeedMultiplier = 0.0125f;
				protected static float gThermalForceMultiplier = 0.25f;

		}

		[Serializable]
		public class ThermalState
		{
				public string ThermalFXName = "ThermalEffect";
				public TemperatureRange Temperature;
				public float MovementSpeed = 1f;
				public float Radius = 35f;
				public float Height = 125f;
				public float Offset = 85f;
				public float ThermalStrengthBottom = 0.125f;
				public float ThermalStrengthTop = 0.05f;
				public bool AlwaysVisible = false;
				public List <SVector3> PivotPoints = new List <SVector3>();
		}
}
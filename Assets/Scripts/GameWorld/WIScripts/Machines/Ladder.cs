using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
		public class Ladder : WIScript
		{
				public LadderState State = new LadderState();
				public Transform MountPoint;
				public Transform DismountPoint;
				public bool Climbing = false;
				public float ClimbSpeed = 2f;

				public override void OnInitialized()
				{
						worlditem.OnPlayerUse += OnPlayerUse;

						State.MountPoint.ApplyTo(MountPoint, false);
						State.DismountPoint.ApplyTo(DismountPoint, false);
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.MountPoint.CopyFrom(MountPoint);
						State.DismountPoint.CopyFrom(DismountPoint);
				}

				public override void InitializeTemplate()
				{
						State.MountPoint.CopyFrom(MountPoint);
						State.DismountPoint.CopyFrom(DismountPoint);
				}
				#endif
				public void StopClimbing(bool addJumpForce)
				{
						if (Climbing) {
								Player.Local.RestoreControl(true);
								Player.Local.Position = endPoint.position - endPoint.forward;//bump us up a bit to match the gizmo pisition
								Player.Local.Rotation = endPoint.rotation;
								Climbing = false;
								enabled = false;
						}
				}

				public void OnPlayerUse()
				{
						if (Climbing) {
								return;
						}
						Climbing = true;
						//set the up vector for the two points to up
						//MountPoint.up = Vector3.up;
						//DismountPoint.up = Vector3.up;//never mind this fucks things up
						climbLookTarget = gameObject.FindOrCreateChild("ClimbLookTarget");

						float distanceToMountPoint = Vector3.Distance(Player.Local.Position, MountPoint.position);
						float distanceToDismountPoint = Vector3.Distance(Player.Local.Position, DismountPoint.position);
						if (distanceToMountPoint > distanceToDismountPoint) {
								startPoint = DismountPoint;
								endPoint = MountPoint;
								climbOffset = -endPoint.forward;
						} else {
								startPoint = MountPoint;
								endPoint = DismountPoint;
								climbOffset = -endPoint.forward + Player.Local.Height;
						}
						climbLookTarget.position = startPoint.position;
						Player.Local.HijackControl();
						//Player.Local.State.HijackMode = PlayerHijackMode.OrientToTarget;
						mStartTime = WorldClock.AdjustedRealTime;
						mEndTime = mStartTime + ClimbSpeed;
						enabled = true;
				}

				public void Update()
				{
						bool reachedEnd = false;
						float normalizedClimbTime = (float)((WorldClock.AdjustedRealTime - mStartTime) / (mEndTime - mStartTime));
						climbLookTarget.position = Vector3.Lerp(startPoint.position, endPoint.position, normalizedClimbTime);
						climbLookTarget.LookAt(endPoint.position + climbOffset);
						Player.Local.SetHijackTargets(climbLookTarget, climbLookTarget);

						if (normalizedClimbTime > 1f) {
								StopClimbing(false);
						}
				}

				protected Transform startPoint = null;
				protected Transform endPoint = null;
				protected Transform climbLookTarget = null;
				protected Vector3 climbOffset;
				protected double mStartTime = 0f;
				protected double mEndTime = 0f;
				#if UNITY_EDITOR
				public void OnDrawGizmos()
				{
						Gizmos.color = Color.white;
						Gizmos.DrawLine(MountPoint.position, DismountPoint.position);
						Gizmos.color = Colors.Alpha(Color.yellow, 0.2f);
						Gizmos.DrawSphere(MountPoint.position, 1f);
						Gizmos.DrawSphere(DismountPoint.position, 1f);
						Gizmos.color = Color.yellow;
						DrawArrow.ForGizmo(MountPoint.position + MountPoint.forward, -MountPoint.forward, 0.25f, 15f);
						DrawArrow.ForGizmo(DismountPoint.position, -DismountPoint.forward, 0.25f, 15f);
						Gizmos.color = Colors.Alpha(Color.red, 0.2f);
						DrawArrow.ForGizmo(DismountPoint.position - DismountPoint.forward, Vector3.down, 0.25f);
						Gizmos.DrawSphere(DismountPoint.position - DismountPoint.forward, 0.5f);
						Gizmos.DrawSphere(MountPoint.position + DismountPoint.forward, 0.5f);
						UnityEditor.Handles.color = Color.red;
						UnityEditor.Handles.DrawWireDisc(DismountPoint.position - DismountPoint.forward, Vector3.up, 0.5f);
						UnityEditor.Handles.color = Color.red;
						UnityEditor.Handles.DrawWireDisc(MountPoint.position + DismountPoint.forward, Vector3.up, 0.5f);

						if (Climbing) {
								Gizmos.color = Color.green;
								Gizmos.DrawSphere(climbLookTarget.position, 0.25f);
								DrawArrow.ForGizmo(climbLookTarget.position, climbLookTarget.forward, 0.25f);
						}
				}
				#endif
		}

		[Serializable]
		public class LadderState
		{
				public STransform MountPoint = new STransform();
				public STransform DismountPoint = new STransform();
		}
}
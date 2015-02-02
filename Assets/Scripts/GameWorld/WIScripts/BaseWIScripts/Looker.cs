using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Looker : WIScript, IAwarenessBubbleUser
		{
				public Transform transform { get { return worlditem.tr; } }

				public LookerState State = new LookerState();

				public void LookForStuff(LookerBubble sharedLookerCollider)
				{
						sharedLookerCollider.StartUsing(this, worlditem.Colliders, 0.25f);
				}

				public void SeeItemsOfInterest(List <IVisible> visibleItems)
				{
						LastSeenPlayer = null;
						LastSeenActionNode = null;
						LastSeenWorldItem = null;
						LastSeenItemOfInterest = null;
						for (int i = 0; i < visibleItems.Count; i++) {
								IVisible visibleItem = visibleItems[i];
								if (CanSeeVisibleItem(visibleItem, true)) {
										switch (visibleItem.IOIType) {
												case ItemOfInterestType.Player:
														LastSeenPlayer = visibleItems[i].player;
														break;

												case ItemOfInterestType.WorldItem:
														LastSeenItemOfInterest = visibleItems[i].worlditem;
														break;

												case ItemOfInterestType.ActionNode:
														LastSeenActionNode = visibleItems[i].node;
														break;

												default:
														break;
										}
										LastSeenItemOfInterest = visibleItems[i];
								}
						}

						if (LastSeenPlayer != null) {
								SawPlayerGizmo = 1f;
								OnSeePlayer.SafeInvoke();
						}
						if (LastSeenItemOfInterest != null) {
								OnSeeWorldItem.SafeInvoke();
						}
						if (LastSeenActionNode != null) {
								OnSeeActionNode.SafeInvoke();
						}
						if (LastSeenItemOfInterest != null) {
								OnSeeItemOfInterest.SafeInvoke();
						}
				}

				public bool CanSeeVisibleItem(IVisible visibleItem, bool requireFieldOfView)
				{
						//we calculate this every time because it could change due to external modifiers
						mAwarenessDistance = Looker.AwarenessDistanceTypeToVisibleDistance(State.AwarenessDistance);
						if (State.ManualAwarenessDistance > 0) {
								mAwarenessDistance = State.ManualAwarenessDistance;
						}

						if (Looker.IsInVisibleRange(worlditem.Position, visibleItem.Position, mAwarenessDistance) &&
						 (!requireFieldOfView || Looker.IsInFieldOfView(worlditem.tr.forward, worlditem.Position, State.FieldOfView, visibleItem.Position))) {
								//it's in our field of view AND it's close enough to see
								//check to see if we can see it with the visible item's modifiers in place
								if (visibleItem.IsVisible && Looker.IsInVisibleRange(worlditem.Position, visibleItem.Position, mAwarenessDistance * visibleItem.AwarenessDistanceMultiplier) &&
								(!requireFieldOfView || Looker.IsInFieldOfView(worlditem.tr.forward, worlditem.Position, State.FieldOfView * visibleItem.FieldOfViewMultiplier, visibleItem.Position))) {
										return true;
								}
						}

						visibleItem.LookerFailToSee();
						return false;
				}

				protected IVisible mVisibleItemCheck = null;

				public bool CanSeeItemOfInterest(IItemOfInterest itemOfInterest, bool requireFieldOfView)
				{
						//if it's a visible item
						//we'll want to access its visibility modifiers
						IVisible mVisibleItemCheck = (IVisible)itemOfInterest;
						if (mVisibleItemCheck != null) {
								return CanSeeVisibleItem(mVisibleItemCheck, requireFieldOfView);
						}

						//otherwise we'll just check the item without multipliers
						//we calculate this every time because it could change due to external modifiers
						float mAwarenessDistance = Looker.AwarenessDistanceTypeToVisibleDistance(State.AwarenessDistance);
						if (State.ManualAwarenessDistance > 0) {
								mAwarenessDistance = State.ManualAwarenessDistance;
						}

						if (Looker.IsInVisibleRange(worlditem.Position, itemOfInterest.Position, mAwarenessDistance)) {
								if (!requireFieldOfView || Looker.IsInFieldOfView(worlditem.tr.forward, worlditem.Position, State.FieldOfView, itemOfInterest.Position)) {
										//it's in our field of view AND it's close enough to see
										//check to see if we can see it with the visible item's modifiers in place
										return true;
								}
						}

						return false;
				}

				public Action OnSeeWorldItem;
				public Action OnSeePlayer;
				public Action OnSeeActionNode;
				public Action OnSeeItemOfInterest;
				public WorldItem LastSeenWorldItem;
				public PlayerBase LastSeenPlayer;
				public ActionNode LastSeenActionNode;
				public IItemOfInterest LastSeenItemOfInterest;
				protected float mAwarenessDistance = 0f;
				protected WorldItem mWorldItemGoal = null;
				protected ActionNode mActionNodeGoal = null;

				public void OnDrawGizmos()
				{
						if (worlditem == null || worlditem.tr == null)
								return;

						SawPlayerGizmo = Mathf.Lerp(SawPlayerGizmo, 0.15f, (float)WorldClock.ARTDeltaTime);

						Gizmos.color = Colors.Alpha(Color.green, SawPlayerGizmo);
						float totalFOV = State.FieldOfView * 120f;
						float rayRange = AwarenessDistanceTypeToVisibleDistance(State.AwarenessDistance);
						float halfFOV = totalFOV / 2.0f;
						Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
						Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
						Vector3 leftRayDirection = leftRayRotation * worlditem.tr.forward;
						Vector3 rightRayDirection = rightRayRotation * worlditem.tr.forward;
						Gizmos.DrawRay(worlditem.tr.position, leftRayDirection * rayRange);
						Gizmos.DrawRay(worlditem.tr.position, rightRayDirection * rayRange);

						Gizmos.color = Colors.Alpha(Color.red, SawPlayerGizmo);
						totalFOV = totalFOV * Player.Local.FieldOfViewMultiplier;
						rayRange = rayRange * Player.Local.AwarenessDistanceMultiplier;
						halfFOV = totalFOV / 2f;
						leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
						rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
						leftRayDirection = leftRayRotation * worlditem.tr.forward;
						rightRayDirection = rightRayRotation * worlditem.tr.forward;
						Gizmos.DrawRay(worlditem.tr.position, leftRayDirection * rayRange);
						Gizmos.DrawRay(worlditem.tr.position, rightRayDirection * rayRange);

						Gizmos.color = Color.cyan;
						DrawArrow.ForGizmo(worlditem.tr.position, worlditem.tr.forward, 0.25f, 20f);
				}

				protected float SawPlayerGizmo;

				#region static helper functions

				public static bool IsInVisibleRange(Vector3 lookerPosition, Vector3 visiblePosition, float awarenessDistance)
				{
						return Vector3.Distance(lookerPosition, visiblePosition) < awarenessDistance;
				}

				public static bool IsInFieldOfView(Vector3 viewForward, Vector3 viewPosition, float fieldOfView, Vector3 visiblePosition, float proximalAwarenessDistance)
				{
						//TEMP
						return true;
						//Vector3 targetVector = Vector3.Normalize (targetPosition - viewPosition);
						//float angle = Vector3.Angle (viewForward, targetVector);
						//return angle < (fieldOfView * 0.5f);//half the field of view (?)
				}

				public static bool IsInFieldOfView(Vector3 viewForward, Vector3 viewPosition, float fieldOfView, Vector3 visiblePosition)
				{
						Vector3 targetVector = Vector3.Normalize(viewPosition - visiblePosition);
						float angleOfView = 180f - Vector3.Angle(viewForward, targetVector);
						return angleOfView < ((fieldOfView * Globals.MaxFieldOfView) / 2);
				}

				public static float AwarenessDistanceTypeToVisibleDistance(AwarnessDistanceType awarenessDistanceType)
				{
						float awarenessDistance = 1.0f;
						switch (awarenessDistanceType) {
								case AwarnessDistanceType.Poor:
										awarenessDistance = 2.0f;
										break;
				
								case AwarnessDistanceType.Fair:
										awarenessDistance = 3.0f;
										break;
				
								case AwarnessDistanceType.Good:
										awarenessDistance = 5.0f;
										break;
				
								case AwarnessDistanceType.Excellent:
										awarenessDistance = 7.0f;
										break;
				
								case AwarnessDistanceType.Prescient:
										awarenessDistance = 10.0f;
										break;
				
								default:
										break;
						}
						return awarenessDistance;
				}

				#endregion

		}

		[Serializable]
		public class LookerState
		{
				public float FieldOfView = 0.5f;
				public AwarnessDistanceType AwarenessDistance = AwarnessDistanceType.Good;
				public float ManualAwarenessDistance = -1f;
				[BitMaskAttribute(typeof(ItemOfInterestType))]
				public ItemOfInterestType VisibleTypesOfInterest = ItemOfInterestType.All;
				public List <string> ItemsOfInterest = new List <string>();
		}
}
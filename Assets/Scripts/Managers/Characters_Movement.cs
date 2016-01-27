using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;
using System.Collections.Generic;
using ExtensionMethods;
using Frontiers.World;
using System;
using System.Xml.Serialization;
using Frontiers.Data;

namespace Frontiers.World
{
	public partial class Characters
	{
		#region motile actions

		public static bool StartAction (Motile m, MotileAction action)
		{
			//Debug.Log("trying to start action");
			//open up the door for mod-supplied delegtates
			//do some general cleanup - if we're starting an action, we want to start from scratch
			if (m.GoalObject == null) {
				Debug.Log("Goal object is null, action was cancelled");
				action.Cancel ();
				return true;
			}

			if (!GetUpdateCoroutine (m, action)) {
				Debug.Log("Couldn't get coroutine");
				action.Cancel ();
				return true;
			}
			//reset this just in case
			m.AvoidingObstacle = false;
			m.GoalObject.parent = m.worlditem.Group.transform;
			m.GoalObject.position = m.LastKnownPosition;// + transform.forward;
			//m.rvoController.PositionLocked = false;
			//m.rvoController.RotationLocked = false;
			m.TargetMovementSpeed = 0.0f;
			m.CurrentRotationChangeSpeed = m.State.MotileProps.RotationChangeSpeed;
			//get the default pathfinding method
			action.Method = GetDefaultGoToMethod (m.State.MotileProps.DefaultGoToMethod, action.Method);

			//okay, now handle the new action
			if (action.State != MotileActionState.Waiting || !action.ResetAfterInterrupt) {	//if we're NOT resuming OR we're supposed to reset on resuming
				action.WTStarted = WorldClock.AdjustedRealTime;
			}
			bool started = false;
			switch (action.Type) {
			case MotileActionType.FocusOnTarget:
			case MotileActionType.FollowRoutine:
				started = true;
				break;

			case MotileActionType.FollowGoal:
				if (action.HasLiveTarget) {
					//goal objects can be moved externally
					//so we don't need a live target
					m.GoalObject.position = action.LiveTarget.Position;
				}
				started = true;
				break;

			case MotileActionType.FleeGoal:
				if (action.HasLiveTarget) {
					m.GoalObject.position = action.LiveTarget.Position;
				}
				started = true;
				break;

			case MotileActionType.WanderIdly:
				m.GoalObject.position = m.worlditem.Position;
				started = true;
				break;

			case MotileActionType.FollowTargetHolder:
				if (!action.HasLiveTarget) {
					//TODO get live target?
				}
				if (action.HasLiveTarget) {
					m.GoalHolder = action.LiveTarget.gameObject.GetOrAdd <RVOTargetHolder> ();
				} else {
					m.GoalHolder = action.LiveTargetHolder;
				}
				started = true;
				break;

			case MotileActionType.GoToActionNode:
				//wait a tick to let the live target load
				if (m.LastOccupiedNode != null) {
					m.LastOccupiedNode.VacateNode (m.worlditem);
				}
				if (!action.HasLiveTarget) {
					//get live target
					ActionNodeState nodeState = null;
				}
				started = true;
				break;

			case MotileActionType.Wait:
			default:
				m.TargetMovementSpeed = 0.0f;
				started = true;
				break;
			}

			if (started) {
				if (action.State != MotileActionState.Error) {	//preserve the error
					action.State = MotileActionState.Started;
				}
				//send messages
				action.OnStartAction.SafeInvoke ();
				action.OnStartAction = null;
			}

			return started;
		}

		public static bool FinishAction (Motile m, MotileAction action)
		{
			//Debug.Log("Finishing action " + action.Name);
			if (action.BaseAction) {
				return false;
			}

			m.AvoidingObstacle = false;

			action.State = MotileActionState.Finishing;
			bool finished = false;
			switch (action.Type) {
			case MotileActionType.FocusOnTarget:
				finished = true;
				break;

			case MotileActionType.FollowGoal:
				finished = true;
				break;

			case MotileActionType.FollowRoutine:
				finished = true;
				break;

			case MotileActionType.WanderIdly:
				finished = true;
				break;

			case MotileActionType.FollowTargetHolder:
				m.GoalHolder = null;
				//move goal to group transform
				//this will stop the target holder from using it
				finished = true;
				break;

			case MotileActionType.GoToActionNode:
				//if we're at the action node, vacate the node
				//if we're not at the action node, do nothing
				if (m.LastOccupiedNode == null && action.HasLiveTarget && action.LiveTarget.IOIType == ItemOfInterestType.ActionNode) {	//if we actually have a live target
					ActionNode node = action.LiveTarget.node;
					if (!node.IsOccupant (m.worlditem)) {	//try to occupy it one last time
						node.TryToOccupyNode (m.worlditem);
						//if we don't make it oh well
					}
				}
				finished = true;
				break;

			case MotileActionType.Wait:
			default:
				finished = true;
				break;
			}

			if (finished) {
				if (action.State != MotileActionState.Error) {	//preserve the error
					action.State = MotileActionState.Finished;
				}
				action.UpdateCoroutine = null;//reset this
				action.WTFinished = WorldClock.AdjustedRealTime;
				m.LastFinishedAction = action;
				//send final messages and whatnot
				action.OnFinishAction.SafeInvoke ();
				action.OnFinishAction = null;
				if (m != null && m.GoalObject != null) {
					m.GoalObject.parent = m.worlditem.Group.transform;
				}
			}
			//wait for finish to end (not implemented)
			//force refresh hud
			//m.rvoController.PositionLocked = false;
			//m.rvoController.RotationLocked = false;
			if (m != null) {
				m.worlditem.RefreshHud ();
			}
			//action state is finsihed
			return finished;
		}

		public static void InterruptAction (Motile m, MotileAction action)
		{
			action.State = MotileActionState.Waiting;
			action.OnInterruptAction.SafeInvoke ();
			action.OnInterruptAction = null;
		}

		public static bool UpdateExpiration (Motile m, MotileAction action)
		{
			if (action.BaseAction)
				return false;

			bool expire = false;
			switch (action.Expiration) {
			case MotileExpiration.Duration:
				double expireTime = action.WTStarted + action.RTDuration;
				expire = WorldClock.AdjustedRealTime > expireTime;
				break;

			case MotileExpiration.TargetInRange:
				if (action.HasLiveTarget) {	//are we close enough to our target?
					expire = (Vector3.Distance (action.LiveTarget.Position, m.LastKnownPosition) < action.Range);
				} else {
					expire = (Vector3.Distance (m.GoalObject.position, m.LastKnownPosition) < action.Range);
				}
				//if no live target get non-live target and measure (?)
				break;

			case MotileExpiration.TargetOutOfRange:
				if (action.HasLiveTarget) {	//are we too far from our target?
					expire = (Vector3.Distance (action.LiveTarget.Position, m.LastKnownPosition) > action.OutOfRange);
				} else {
					expire = (Vector3.Distance (m.GoalObject.position, m.LastKnownPosition) > action.OutOfRange);
				}
				//if no live target get non-live target and measure (?)
				break;

			case MotileExpiration.Never:
			default:
				break;
			}
			#if UNITY_EDITOR
			if (expire) {
			//Debug.Log ("action " + action.Name + " expired because " + action.Expiration.ToString ());
			}
			#endif
			return expire;
		}

		public static IEnumerator UpdateFollowTargetHolder (Motile m, MotileAction action)
		{
			float minimumSpeed = 0f;
			float minimumRotationChangeSpeed = 0f;
			if (m.GoalHolder == null) {//if the target holder is gone or the target holder is no longer managing our goal
				//we're finished if we don't have a target
				//Debug.Log ("Goal holder was null in update follow target holder");
				FinishAction (m, action);
			} else {
				//otherwise proceed normally
				//first make sure the target holder is actually managing us
				if (m.AvoidingObstacle && m.GoalObject.parent != null) {
					//set the goal object to null for a moment and let it hang out
					m.GoalObject.parent = null;
				} else if (m.GoalObject.parent != m.GoalHolder.transform) {
					//oh snape we've lost the connection, try to get it back
					switch (action.FollowType) {
					case MotileFollowType.Follower:
						//Debug.Log ("Adding ground follower to target " + m.GoalHolder.name);
						minimumSpeed = m.State.MotileProps.SpeedIdleWalk;
						m.GoalHolder.AddGroundFollower (m.GoalObject);
						break;

					case MotileFollowType.Stalker:
						minimumSpeed = m.State.MotileProps.SpeedIdleWalk;
						m.GoalHolder.AddGroundStalker (m.GoalObject);
						break;

					case MotileFollowType.Attacker:
						//attacking is a much more aggressive form of stalking
						//use our top speed to get where we need to go
						minimumSpeed = m.State.MotileProps.SpeedAttack;
						minimumRotationChangeSpeed = m.State.MotileProps.RotationChangeSpeed * 2f;
						m.GoalHolder.AttackOrStalk (m.GoalObject, true, ref action.FollowDirection);
						break;

					case MotileFollowType.Companion:
					default:
						minimumSpeed = m.State.MotileProps.SpeedIdleWalk;
						m.GoalHolder.AddCompanion (m.GoalObject);
						break;
					}
				}
				//are we close enough to stop?
				//TargetRotation = m.rvoController.targetRotation;
				float distanceFromHolder = Vector3.Distance (m.worlditem.Position, m.GoalObject.position);
				float distanceFromTarget = Vector3.Distance (m.worlditem.Position, m.GoalHolder.tr.position);
				//use the range variable for our distance check
				if (distanceFromTarget <= action.Range) {	//don't spaz out - stop moving and face the target
					//if this results in a 'TargetInRange' expiration it'll be handled below
					action.IsInRange = true;
					if (action.FollowType == MotileFollowType.Companion) {
						//companions wait within the range for instructions
						m.TargetMovementSpeed = 0f;
					} else {
						//other follow types keep trying to make it to their target
						m.TargetMovementSpeed = m.State.MotileProps.SpeedIdleWalk;
					}
				} else if (distanceFromTarget <= action.Range * 1.5) {
					action.IsInRange = false;
					//m.rvoController.PositionLocked = false;
					//m.rvoController.RotationLocked = false;
					//don't stop, but do slow down a bit
					m.TargetMovementSpeed = m.State.MotileProps.SpeedWalk;
				} else {	//run and catch up!
					action.IsInRange = false;
					//m.rvoController.PositionLocked = false;
					//m.rvoController.RotationLocked = false;
					m.TargetMovementSpeed = m.State.MotileProps.SpeedRun;
				}
				//we don't need to update this one very often
				m.TargetMovementSpeed = Mathf.Max ((float)m.TargetMovementSpeed, minimumSpeed);
				m.CurrentRotationChangeSpeed = Mathf.Max (m.State.MotileProps.RotationChangeSpeed, minimumRotationChangeSpeed);
				yield return null;
			}
		}

		public static IEnumerator UpdateFollowGoal (Motile m, MotileAction action)
		{
			action.IsInRange = false;
			if (!m.AvoidingObstacle && action.LiveTarget != null) {
				//only update the goal object position if we're not avoiding an obstacle
				m.GoalObject.position = action.LiveTarget.Position;
			}
			float distanceFromTarget = Vector3.Distance (m.worlditem.Position, m.GoalObject.position);
			action.IsInRange = distanceFromTarget < action.Range;
			if (action.IsInRange) {
				m.TargetMovementSpeed = m.State.MotileProps.SpeedIdleWalk;
			} else {
				if (distanceFromTarget <= action.Range * 2.0f || action.WalkingSpeed) {
					m.TargetMovementSpeed = m.State.MotileProps.SpeedWalk;
				} else {
					m.TargetMovementSpeed = m.State.MotileProps.SpeedRun;
				}
			}
			//wait a tick
			yield return null;
			yield break;
		}

		public static IEnumerator UpdateFollowRoutine (Motile m, MotileAction action)
		{
			yield return null;
			yield break;
			//TODO: Come back and fix this.
		}

		public static IEnumerator UpdateGoToActionNode (Motile m, MotileAction action)
		{
			if (action.HasLiveTarget) {
				//m.rvoController.usePath = (action.Method == MotileGoToMethod.Pathfinding);
				m.GoalObject.position = action.LiveTarget.Position;
				ActionNode node = null;
				if (action.LiveTarget.IOIType == ItemOfInterestType.ActionNode) {	//see if we're there yet
					node = action.LiveTarget.node;
					float distanceFromTarget = Vector3.Distance (m.worlditem.Position, m.GoalObject.position);
					//use the range variable for our distance check
					if (distanceFromTarget <= action.Range) {
						m.TargetMovementSpeed = m.State.MotileProps.SpeedWalk;
						//can we occupy this thing?
						if (node.CanOccupy (m.worlditem)) {	//hooray! we can occupy it
							if (node.TryToOccupyNode (m.worlditem)) {	//we've occupied it, huzzah
								m.LastOccupiedNode = node;
								m.TargetMovementSpeed = 0.0f;
								FinishAction (m, action);
								yield break;
							}
							//if we didn't occupy it, it might mean we're not close enough
							//because our range may be larger than the node range
							//so try again next frame
						} else {	//whoops, node is inaccessible
							//set to error
							action.State = MotileActionState.Error;
							action.Error = MotileActionError.TargetInaccessible;
						}
					} else if (distanceFromTarget <= action.Range * 1.5) {
						//don't stop, but do slow down a bit
						m.TargetMovementSpeed = m.State.MotileProps.SpeedWalk;
					} else {
						//run and catch up!
						m.TargetMovementSpeed = m.State.MotileProps.SpeedRun;
					}
				} else {	//weird, it got unloaded for some reason
					action.State = MotileActionState.Error;
					action.Error = MotileActionError.TargetNotLoaded;
				}
			} else {	//weird, live target is gone for some reason
				//try to get it again
				//(not implemented)
				action.State = MotileActionState.Error;
				action.Error = MotileActionError.TargetNotLoaded;
			}
			//otherwise get live target
			yield return null;
			yield break;
		}

		public static IEnumerator UpdateFocusOnTarget (Motile m, MotileAction action)
		{
			if (m == null || m.GoalObject == null) {
				yield break;
			}
			//Debug.Log("Focusing on target in " + action.Name);
			if (action.HasLiveTarget) {	//move the focus object to the live target's position
				m.GoalObject.position = action.LiveTarget.Position;
			}
			//if we don't have a live target then it's probably being manipulated externally
			m.TargetMovementSpeed = 0.0f;
			//wait a bit
			yield return null;
			//we're done
			yield break;
		}

		public static IEnumerator UpdateFleeGoal (Motile m, MotileAction action)
		{
			m.TargetMovementSpeed = m.State.MotileProps.SpeedRun;

			if (action.LiveTarget == null) {
				action.TryToFinish ();
				yield break;
			}

			if (m.AvoidingObstacle) {
				yield break;
			}

			mAvoid = action.LiveTarget.Position;
			mFleeDirection = (m.LastKnownPosition - mAvoid).normalized;
			mGoalPosition = m.LastKnownPosition;

			if (action.TerritoryType == MotileTerritoryType.Den) {
				//chose a position that's within the den radius
				float distanceToDenEdge = Vector3.Distance (action.TerritoryBase.Position, m.LastKnownPosition);
				mGoalPosition = m.LastKnownPosition + (mFleeDirection * Mathf.Min (distanceToDenEdge, action.Range));
			} else {
				//just move the goal
				mGoalPosition = m.LastKnownPosition + (mFleeDirection * action.Range);
			}
			mRandomTerrainHit.groundedHeight = m.terrainHit.groundedHeight;
			mRandomTerrainHit.overhangHeight = m.terrainHit.overhangHeight;
			mRandomTerrainHit.feetPosition = mGoalPosition;
			mGoalPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mRandomTerrainHit);
			m.GoalObject.position = mGoalPosition;
			double finishTime = WorldClock.AdjustedRealTime + 0.1f;
			while (WorldClock.AdjustedRealTime < finishTime) {
				yield return null;
			}

			yield break;
		}

		public static IEnumerator UpdateWanderIdly (Motile m, MotileAction action)
		{
			//m.rvoController.PositionLocked = false;
			//m.rvoController.RotationLocked = false;
			m.TargetMovementSpeed = m.State.MotileProps.SpeedIdleWalk;
			IEnumerator sendGoal = null;
			if (m.HasReachedGoal (m.State.MotileProps.RVORadius)) {
				if (UnityEngine.Random.value < (m.State.MotileProps.IdleWanderThreshold / 100)) {
					//choose a new direction no matter what
					switch (action.TerritoryType) {
					case MotileTerritoryType.Den:
						sendGoal = SendGoalToRandomPosition (m, action.TerritoryBase.Position, action.TerritoryBase.Radius, action.TerritoryBase.InnerRadius);
						while (sendGoal.MoveNext ()) {
							yield return sendGoal.Current;
						}
						break;

					case MotileTerritoryType.None:
					default:
						sendGoal = SendGoalToRandomPosition (m, m.GoalObject.position, action.Range, action.Range / 2f);
						while (sendGoal.MoveNext ()) {
							yield return sendGoal.Current;
						}
						break;
					}
				}
			} else if (!m.AvoidingObstacle && UnityEngine.Random.value < (m.State.MotileProps.IdleWaitThreshold / 100)) {
				sendGoal = SendGoalToRandomPosition (m, m.worlditem.Position, action.Range, 0.5f);//this will make us look around
				while (sendGoal.MoveNext ()) {
					yield return sendGoal.Current;
				}
			}
			double finishTime = WorldClock.AdjustedRealTime + 0.25f;
			while (WorldClock.AdjustedRealTime < finishTime) {
				yield return null;
			}
			yield break;
		}

		public static IEnumerator UpdateWait (Motile m, MotileAction action)
		{
			if (m == null || m.GoalObject == null) {
				yield break;
			}

			m.TargetMovementSpeed = 0.0f;
			if (action.HasLiveTarget) {
				if (!m.AvoidingObstacle) {
					m.GoalObject.position = action.LiveTarget.Position;
				}
			} else {
				//look at stuff randomly
				if (UnityEngine.Random.value < (m.State.MotileProps.IdleWanderThreshold / 100)) {
					//Debug.Log ("Waiting in motile, sending goal to random position");
					var sendGoal = SendGoalToRandomPosition (m, m.worlditem.Position, 0.125f, 0.125f);
					while (sendGoal.MoveNext ()) {
						yield return sendGoal.Current;
					}
				}
			}
			double finishTime = WorldClock.AdjustedRealTime + 0.125f;
			while (!action.IsFinished && WorldClock.AdjustedRealTime < finishTime) {
				yield return null;
			}
			yield break;
		}

		public static IEnumerator UpdateReturnToTerritoryBase (Motile m, MotileAction action)
		{
			if (m == null || m.GoalObject == null) {
				yield break;
			}

			if (action.TerritoryType == MotileTerritoryType.Den
				&& !Physics.CheckSphere (m.LastKnownPosition, action.TerritoryBase.InnerRadius)) {
				m.TargetMovementSpeed = m.State.MotileProps.SpeedRun;
				if (!m.AvoidingObstacle) {
					m.GoalObject.position = action.TerritoryBase.Position;
				}
				double waitUntil = WorldClock.AdjustedRealTime + 2f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
			}
			yield break;
		}

		public static bool GetUpdateCoroutine (Motile m, MotileAction action)
		{
			if (action.TerritoryType == MotileTerritoryType.Den
				&& !Physics.CheckSphere (m.LastKnownPosition, action.TerritoryBase.Radius)) {
				//we've exceeded our territory bounds
				//return to our territory
				action.UpdateCoroutine = UpdateReturnToTerritoryBase (m, action);
				return true;
			}

			switch (action.Type) {
			case MotileActionType.FocusOnTarget:
				action.UpdateCoroutine = UpdateFocusOnTarget (m, action);
				break;

			case MotileActionType.FollowRoutine:
				action.UpdateCoroutine = UpdateFollowRoutine (m, action);
				break;

			case MotileActionType.FollowGoal:
				action.UpdateCoroutine = UpdateFollowGoal (m, action);
				break;

			case MotileActionType.FleeGoal:
				action.UpdateCoroutine = UpdateFleeGoal (m, action);
				break;

			case MotileActionType.WanderIdly:
				action.UpdateCoroutine = UpdateWanderIdly (m, action);
				break;

			case MotileActionType.FollowTargetHolder:
				action.UpdateCoroutine = UpdateFollowTargetHolder (m, action);
				break;

			case MotileActionType.GoToActionNode:
				action.UpdateCoroutine = UpdateGoToActionNode (m, action);
				break;

			case MotileActionType.Wait:
			default:
				action.UpdateCoroutine = UpdateWait (m, action);
				break;
			}
			return action.UpdateCoroutine != null;
		}

		public static void SendGoalAwayFromObstacle (Motile m, Vector3 origin, float maxRange, float minRange, Bounds mObstacleBounds)
		{
			mRandomPosition = m.LastKnownPosition + (UnityEngine.Random.onUnitSphere.WithY (0f) * UnityEngine.Random.Range (minRange, maxRange));
			if (m.worlditem.Group.Props.Interior) {
				//TODO get interior position
			} else {
				mRandomPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mRandomTerrainHit);
			}
			m.GoalObject.position = mRandomPosition;
		}

		public static IEnumerator SendGoalToRandomPosition (Motile m, Vector3 origin, float maxRange, float minRange)
		{
			if (m == null || m.GoalObject == null)
				yield break;

			//Debug.Log (m.name + " Sending goal to random position in " + mRandomPosition.ToString ());
			mRandomDirection = UnityEngine.Random.insideUnitSphere.WithY (0f) * UnityEngine.Random.Range (minRange, maxRange);
			mRandomPosition = mRandomDirection + origin;
			mRandomTerrainHit.groundedHeight = m.terrainHit.groundedHeight;
			mRandomTerrainHit.overhangHeight = m.terrainHit.overhangHeight;
			mRandomTerrainHit.feetPosition = mRandomPosition;
			mRandomTerrainHit.ignoreWater = false;
			mRandomTerrainHit.hitWater = true;
			//keep trying until we stop hitting water
			while (mRandomTerrainHit.hitWater) {
				if (m.worlditem.Group.Props.Interior) {
					//TODO get interior position
				} else {
					mRandomPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mRandomTerrainHit);
				}
				yield return null;
			}
			if (m != null && m.GoalObject != null) {
				m.GoalObject.position = mRandomPosition;
			}
			yield break;
		}

		public static MotileGoToMethod GetDefaultGoToMethod (MotileGoToMethod Default, MotileGoToMethod Failsafe)
		{
			if (Default == MotileGoToMethod.UseDefault) {
				return Failsafe;
			}
			return Default;
		}

		protected static Vector3 mRandomPosition;
		protected static Vector3 mRandomDirection;
		protected static GameWorld.TerrainHeightSearch mRandomTerrainHit;
		protected static Vector3 mAvoid;
		protected static Vector3 mFleeDirection;
		protected static Vector3 mGoalPosition;

		#endregion

		#region

		public static bool CheckForObstacles (Motile m, ref Bounds obstacleBounds) {

			if (Physics.CapsuleCast (m.worlditem.Position,
				(m.worlditem.Position + Vector3.up * m.State.MotileProps.RVORadius),
				m.State.MotileProps.RVORadius,
				m.worlditem.tr.forward,
				out gHitInfo,
				m.State.MotileProps.RVORadius * 1.125f,
				Globals.LayerSolidTerrain | Globals.LayerStructureTerrain | Globals.LayerObstacleTerrain)) {
				if (!gHitInfo.collider.isTrigger && !gHitInfo.collider.CompareTag (Globals.TagGroundTerrain)) {
					obstacleBounds = gHitInfo.collider.bounds;
					return true;
				}
			}
			return false;
		}

		protected static BodyPart gHead;
		protected static RaycastHit gHitInfo;

		#endregion

	}

	[Serializable]
	public class MotileAction
	{
		public static MotileAction GoTo (MobileReference target)
		{
			MotileAction newAction = new MotileAction ();
			newAction.Type = MotileActionType.GoToActionNode;
			newAction.Target = target;
			newAction.Expiration = MotileExpiration.Never;
			newAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
			return newAction;
		}

		public static MotileAction GoTo (ActionNodeState state)
		{
			MotileAction newAction = new MotileAction ();
			newAction.Type = MotileActionType.GoToActionNode;
			if (state.IsLoaded) {
				newAction.LiveTarget = state.actionNode;
			}
			newAction.Target = new MobileReference (state.Name, state.ParentGroupPath);
			newAction.Expiration = MotileExpiration.Never;
			newAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
			newAction.IdleAnimation = state.IdleAnimation;
			return newAction;
		}

		public static MotileAction Wait (ActionNodeState state)
		{
			MotileAction newAction = new MotileAction ();
			newAction.Type = MotileActionType.Wait;
			if (state.IsLoaded) {
				newAction.LiveTarget = state.actionNode;
			}
			newAction.Target = new MobileReference (state.Name, state.ParentGroupPath);
			newAction.Expiration = MotileExpiration.Never;
			newAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
			newAction.IdleAnimation = state.IdleAnimation;
			return newAction;
		}

		public static MotileAction Wait (int IdleAnimation)
		{
			MotileAction newAction = new MotileAction ();
			newAction.Type = MotileActionType.Wait;
			newAction.Target = MobileReference.Empty;
			newAction.Expiration = MotileExpiration.Never;
			newAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
			newAction.IdleAnimation	= IdleAnimation;
			return newAction;
		}

		public static MotileAction FocusOnPlayerInRange (float range)
		{
			MotileAction newAction = new MotileAction ();
			newAction.Type = MotileActionType.FocusOnTarget;
			newAction.Target.FileName = "[Player]";
			newAction.LiveTarget = Player.Local;
			newAction.Expiration = MotileExpiration.TargetOutOfRange;
			newAction.OutOfRange = range;
			newAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
			return newAction;
		}

		public static MotileAction TalkToPlayer {
			get {
				MotileAction newAction = new MotileAction ();
				newAction.Type = MotileActionType.FocusOnTarget;
				newAction.Target.FileName = "[Player]";
				newAction.LiveTarget = Player.Local;
				newAction.Expiration = MotileExpiration.Never;
				newAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
				newAction.IdleAnimation = GameWorld.Get.FlagByName ("CharacterIdleAnimation", "Conversation");
				return newAction;
			}
		}

		public IEnumerator WaitForActionToStart (float interval)
		{
			bool wait = !BaseAction;
			while (wait) {
				wait = (State != MotileActionState.Finished && State != MotileActionState.Error && State != MotileActionState.Started);
				if (interval > 0f) {
					double intervalEnd = WorldClock.AdjustedRealTime + interval;
					while (WorldClock.AdjustedRealTime < intervalEnd) {
						yield return null;
					}
				} else {
					yield return null;
				}
			}
			yield break;
		}

		public IEnumerator WaitForActionToFinish (float interval)
		{
			if (interval <= 0f) {
				interval = 0.1f;
			}

			bool wait = !BaseAction;
			while (wait) {
				double intervalEnd = WorldClock.AdjustedRealTime + interval;
				while (WorldClock.AdjustedRealTime < intervalEnd) {
					if (State == MotileActionState.Finished || State == MotileActionState.Error) {
						yield break;
					}
					yield return null;
				}
				yield return null;
			}
			yield break;
		}

		public void Reset ()
		{
			mFinishCalledExternally = false;
			WTAdded = -1.0f;
			WTStarted = -1.0f;
			WTFinished = -1.0f;
			Error = MotileActionError.None;
			State = MotileActionState.NotStarted;
			OnFinishAction = null;
		}

		public void TryToFinish ()
		{
			if (!BaseAction) {
				mFinishCalledExternally = true;
			}
		}

		public void Cancel ()
		{
			if (!BaseAction) {
				State = MotileActionState.Error;
				Error = MotileActionError.Canceled;
			}
		}

		public bool HasLiveTarget {
			get {
				return LiveTarget != null;
			}
		}

		public bool FinishCalledExternally {
			get {
				if (!BaseAction && (State == MotileActionState.Finished || State == MotileActionState.Finishing)) {	//if the state is finished then it doesn't matter any more
					return false;
				}
				return mFinishCalledExternally;
			}
		}

		public bool IsFinished {
			get {
				return (!BaseAction && (State == MotileActionState.Error)
					|| State == MotileActionState.Finished);
			}
		}

		public bool HasStarted {
			get { return State == MotileActionState.Started; }
		}

		public bool IsInRange {
			get;
			set;
		}

		public void CopyFrom (MotileAction action)
		{
			Type = action.Type;
			Target = action.Target;
			FollowType = action.FollowType;
			FollowDirection = action.FollowDirection;
			Method = action.Method;
			Expiration = action.Expiration;
			YieldBehavior = action.YieldBehavior;
			Instructions = action.Instructions;
			RTDuration = action.RTDuration;
			Range = action.Range;
			OutOfRange = action.OutOfRange;
			PathName = action.PathName;
			IdleAnimation = action.IdleAnimation;
			LiveTarget = action.LiveTarget;

			LiveTarget = action.LiveTarget;
			TerritoryBase = action.TerritoryBase;
			OnFinishAction = action.OnFinishAction;
		}

		public string Name = "[Noname]";

		public TerrainType Terrain = TerrainType.All;
		public MotileActionState State = MotileActionState.NotStarted;
		public MotileActionType Type = MotileActionType.GoToActionNode;
		public MobileReference Target = new MobileReference ();
		public MotileFollowType FollowType = MotileFollowType.Follower;
		[XmlIgnore]
		public MapDirection FollowDirection = MapDirection.I_None;
		public MotileGoToMethod Method = MotileGoToMethod.Pathfinding;
		public MotileExpiration Expiration = MotileExpiration.Duration;
		public MotileYieldBehavior YieldBehavior = MotileYieldBehavior.YieldAndWait;
		public MotileActionError Error = MotileActionError.None;
		[XmlIgnore]
		public IEnumerator UpdateCoroutine;

		public MotileTerritoryType TerritoryType {
			get {
				if (TerritoryBase != null) {
					return MotileTerritoryType.Den;
				}
				return MotileTerritoryType.None;
			}
		}

		[BitMask (typeof(MotileInstructions))]
		public MotileInstructions Instructions = MotileInstructions.InheritFromBase;
		public bool ResetAfterInterrupt = true;
		public bool BaseAction = false;
		public double WTAdded = -1.0f;
		public double WTStarted = -1.0f;
		public double WTFinished = -1.0f;
		public double RTDuration = 0.0f;
		public float Range = 1.0f;
		public float OutOfRange = 10.0f;
		public string PathName = null;
		public bool WalkingSpeed = false;
		[FrontiersBitMask ("IdleAnimation")]
		public int IdleAnimation = 1;
		public Icon HudIcon = Icon.Empty;
		[XmlIgnore]
		[NonSerialized]
		public IItemOfInterest LiveTarget = null;
		[XmlIgnore]
		[NonSerialized]
		public RVOTargetHolder LiveTargetHolder = null;

		[XmlIgnore]
		//[NonSerialized]
		public ITerritoryBase TerritoryBase {
			get {
				return mTerritoryBase;
			}
			set {
				mTerritoryBase = value;
			}
		}

		[NonSerialized]
		protected ITerritoryBase mTerritoryBase = null;
		protected bool mFinishCalledExternally = false;
		[XmlIgnore]
		[NonSerialized]
		public Action OnStartAction;
		[XmlIgnore]
		[NonSerialized]
		public Action OnInterruptAction;
		[XmlIgnore]
		[NonSerialized]
		public Action OnFinishAction;
	}
}
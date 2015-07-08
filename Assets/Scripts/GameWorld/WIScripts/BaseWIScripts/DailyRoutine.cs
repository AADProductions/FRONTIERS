using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class DailyRoutine : WIScript
	{
		public DailyRoutineState State = new DailyRoutineState ();
		public IMovementNodeSet ParentSite;
		public Motile motile;
		public Character character;
		public MovementNode LastMovementNode;
		public List <int> PreviousIndexes;
		public int OccupationFlags = Int32.MaxValue;

		public override void OnStartup ()
		{
			OccupationFlags = Int32.MaxValue;
		}

		public override void OnInitialized ()
		{
			motile = worlditem.Get <Motile> ();
			character = worlditem.Get <Character> ();
			worlditem.OnAddedToGroup += OnAddedToGroup;
		}

		public void OnAddedToGroup () {
			if (!mFollowingRoutine) {
				mFollowingRoutine = true;
				StartCoroutine (FollowRoutineOverTime ());
			}
		}

		public override void OnFinish ()
		{
			//Debug.Log ("FINISHED in daily routine in " + name);
		}

		public IEnumerator FollowRoutineOverTime ()
		{
			while (!motile.Initialized || !motile.HasBody) {
				yield return null;
			}

			//wait for a random interval to reduce the load on the city
			double waitUntil = WorldClock.AdjustedRealTime + UnityEngine.Random.value;
			while (WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			if (ParentSite == null) {
				//Debug.Log ("Parent site was null in " + name + ", removing routine");
				Finish ();
				yield break;
			}

			//Debug.Log (worlditem.Group.Props.TerrainType.ToString () + " in " + worlditem.name);
			//if we don't have a parent site within a second of spawning then we're toast
			if (!ParentSite.HasMovementNodes (worlditem.Group.Props.TerrainType, worlditem.Group.Props.Interior, OccupationFlags)) {
				//Debug.Log ("Parent site " + ParentSite.name + " had no movement nodes, quitting routine");
				Finish ();
				yield break;
			}

			while (!ParentSite.IsActive (worlditem.Group.Props.TerrainType, worlditem.Group.Props.Interior, OccupationFlags)) {
				yield return null;
			}

			//get the very first node we'll be using
			LastMovementNode = ParentSite.GetNodeNearest (worlditem.Position, worlditem.Group.Props.TerrainType, worlditem.Group.Props.Interior, OccupationFlags);

			//DAILY ROUTINE START
			while (!(mFinished | mDestroyed)) {
				//wait a random amount
				waitUntil = WorldClock.AdjustedRealTime + (UnityEngine.Random.value * 4f);
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
				//if we're not visible then just hang out
				while (worlditem.Is (WIActiveState.Invisible)) {
					waitUntil = WorldClock.AdjustedRealTime + 0.25f;
					while (WorldClock.AdjustedRealTime < waitUntil) {
						yield return null;
					}
				}
				//get the next movement node
				LastMovementNode = ParentSite.GetNextNode (LastMovementNode, worlditem.Group.Props.TerrainType, worlditem.Group.Props.Interior, OccupationFlags);
				if (MovementNode.IsEmpty (LastMovementNode)) {
					//Debug.Log ("Node was empty in routine from " + ParentSite.name + ", ending routine");
					mFollowingRoutine = false;
					Finish ();
					yield break;
				}

				//Debug.Log ("Going to last movement node " + LastMovementNode.Index.ToString () + " at position " + LastMovementNode.Position.ToString ());
				motile.GoalObject.position = LastMovementNode.Position;
				MotileAction goToAction = character.GoToThing (null);
				goToAction.Range = 1.5f;
				goToAction.WalkingSpeed = true;
				//now wait until we get there
				float maxWaitTime = 30f;
				double startTime = WorldClock.AdjustedRealTime;
				var waitUntilFinished = goToAction.WaitForActionToFinish (0.15f);
				while (waitUntilFinished.MoveNext ()) {
					//Debug.Log ("Waiting for action to finish, state is " + goToAction.State.ToString ());
					if (goToAction.State != MotileActionState.Waiting) {
						motile.GoalObject.position = LastMovementNode.Position;
					}
					yield return waitUntilFinished.Current;
					if (WorldClock.AdjustedRealTime > startTime + maxWaitTime) {
						//Debug.Log ("Timed out, finishing now");
						goToAction.TryToFinish ();
						break;
					}
				}
				yield return null;
			}
			mFollowingRoutine = false;
			yield break;
		}

		protected bool mFollowingRoutine = false;

		void OnDrawGizmos ( ) {
			Gizmos.color = Color.yellow;
			if (!MovementNode.IsEmpty (LastMovementNode)) {
				Gizmos.DrawWireSphere (LastMovementNode.Position, 0.5f);
				Gizmos.DrawLine (transform.position, LastMovementNode.Position);
			} else {
				Gizmos.color = Color.red;
				Gizmos.DrawCube (transform.position, Vector3.one);
			}
		}
	}

	[Serializable]
	public class RoutineStop
	{
		public DailyRoutineGoal RoutineGoal = DailyRoutineGoal.RandomActionNode;
		[HideInInspector]
		public TimeOfDay HourOfDay = TimeOfDay.aa_TimeMidnight;
		public DailyRoutineBehavior BehaviorOnArrival = DailyRoutineBehavior.StayAndPlayGoalAnimation;
		public MobileReference ActionNodeReference = MobileReference.Empty;
		public bool PersistOnError = false;
	}

	[Serializable]
	public class DailyRoutineState
	{
		public bool Paused = false;
		public TimeOfDay LastStopTime = TimeOfDay.aa_TimeMidnight;
		public int LastHourChecked = -1;
		public RoutineStop TimeMidnight = new RoutineStop ();
		//12am
		public RoutineStop TimePostMidnight = new RoutineStop ();
		// 2am
		public RoutineStop TimePreDawn = new RoutineStop ();
		// 4am
		public RoutineStop TimeDawn = new RoutineStop ();
		// 6am
		public RoutineStop TimePostDawn = new RoutineStop ();
		// 8am
		public RoutineStop TimePreNoon = new RoutineStop ();
		//10am
		public RoutineStop TimeNoon = new RoutineStop ();
		//12pm
		public RoutineStop TimePostNoon = new RoutineStop ();
		// 2pm
		public RoutineStop TimePreDusk = new RoutineStop ();
		// 4pm
		public RoutineStop TimeDusk = new RoutineStop ();
		// 6pm
		public RoutineStop TimePostDusk = new RoutineStop ();
		// 8pm
		public RoutineStop TimePreMidnight = new RoutineStop ();
		//10pm
	}
}
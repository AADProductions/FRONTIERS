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
				public DailyRoutineState State = new DailyRoutineState();

				public RoutineStop CurrentRoutineStop {
						get {
								int numTries = 0;
								return GetStop(State.LastStopTime, ref numTries);
						}
				}

				public void SetPaused(bool paused)
				{
						State.Paused = paused;
				}

				public override void OnInitialized()
				{
						if (!mFollowingRoutine) {
								StartCoroutine(FollowRoutineOverTime());
						}
						//set the hour of day on all routine stops
						State.TimeMidnight.HourOfDay = TimeOfDay.aa_TimeMidnight;
						State.TimePostMidnight.HourOfDay = TimeOfDay.ab_TimePostMidnight;
						State.TimePreDawn.HourOfDay = TimeOfDay.ac_TimePreDawn;
						State.TimeDawn.HourOfDay = TimeOfDay.ad_TimeDawn;
						State.TimePostDawn.HourOfDay = TimeOfDay.ae_TimePostDawn;
						State.TimePreNoon.HourOfDay = TimeOfDay.af_TimePreNoon;
						State.TimeNoon.HourOfDay = TimeOfDay.ag_TimeNoon;
						State.TimePostNoon.HourOfDay = TimeOfDay.ah_TimePostNoon;
						State.TimePreDusk.HourOfDay = TimeOfDay.ai_TimePreDusk;
						State.TimeDusk.HourOfDay = TimeOfDay.aj_TimeDusk;
						State.TimePostDusk.HourOfDay = TimeOfDay.ak_TimePostDusk;
						State.TimePreMidnight.HourOfDay = TimeOfDay.al_TimePreMidnight;
						//set last stop based on current hour of day
						State.LastHourChecked = -1;
				}

				public IEnumerator FollowRoutineOverTime()
				{
						mFollowingRoutine = true;
						while (worlditem.Mode != WIMode.Destroyed) {
								if (State.Paused) {
										double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.5f;
										while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
												yield return null;
										}
								} else {	//if we've moved on from the last time
										if (State.LastStopTime != WorldClock.TimeOfDayCurrent || State.LastHourChecked < 0) {	//get the new time
												State.LastHourChecked = WorldClock.Get.HourOfDay;
												State.LastStopTime = WorldClock.TimeOfDayCurrent;
												//send a motile action based on the routine stop's information
												RoutineStop currentStop	= CurrentRoutineStop;

												Motile motile = null;
												if (worlditem.Is <Motile>(out motile)) {
														if (mLastActionSent == null) {
																mLastActionSent = new MotileAction();
														}
														mLastActionSent.Reset();
														mLastActionSent.Type = MotileActionType.GoToActionNode;
														mLastActionSent.Expiration = MotileExpiration.Never;
														if (MobileReference.IsNullOrEmpty(currentStop.ActionNodeReference)) {	//if we haven't specified an action node then get one from the game world
																switch (currentStop.RoutineGoal) {
																		case DailyRoutineGoal.RandomActionNode:
																				ActionNodeState nodeState = null;
																				if (worlditem.Group.GetParentChunk().GetRandomNodeForLocation(worlditem.Group.Props.PathName, currentStop.HourOfDay, out nodeState)) {	//Debug.Log ("Found action node for daily routine");
																						mLastActionSent.LiveTarget = nodeState.actionNode;
																				} else {
																						//Debug.Log ("NO NODE FOUND for daily routine!");
																				}
																				break;

																		default:
									//we should have specified the node so we're screwed
																				break;
																}
														}
														//since this is a daily routine, we want to go back to it when we're interrupted
														mLastActionSent.YieldBehavior	= MotileYieldBehavior.YieldAndWait;
														//semd motile action
														//Debug.Log ("Sent routine stop motile action to " + worlditem.FileName + " at " + State.LastStopTime.ToString ( ));
														motile.PushMotileAction(mLastActionSent, MotileActionPriority.ForceTop);
												}
										}
										//wait a while
										double waitUntil = Frontiers.WorldClock.AdjustedRealTime + Mathf.Min(1.0f, UnityEngine.Random.value * 10.0f);
										while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
												yield return null;
										}
										//check the motile action's status
								}
						}
						mFollowingRoutine = false;
						yield break;
				}

				protected RoutineStop GetStop(TimeOfDay timeOfDay, ref int numTries)
				{
						if (numTries > 12) {
								//Debug.Log ("Hit 12 tries, stopping now");
								return State.TimePostMidnight;
						}
						numTries++;
						RoutineStop checkStop = null;
						switch (State.LastStopTime) {
								case TimeOfDay.ab_TimePostMidnight://	= 2,		// 2am
										checkStop = State.TimePostMidnight;
										break;
								case TimeOfDay.ac_TimePreDawn://		= 4,		// 4am
										checkStop = State.TimePreDawn;
										break;
								case TimeOfDay.ad_TimeDawn://			= 8,		// 6am
										checkStop = State.TimeDawn;
										break;
								case TimeOfDay.ae_TimePostDawn://		= 16,		// 8am
										checkStop = State.TimePostDawn;
										break;
								case TimeOfDay.af_TimePreNoon://		= 32,		//10am
										checkStop = State.TimePreNoon;
										break;
								case TimeOfDay.ag_TimeNoon://			= 64,		//12pm
										checkStop = State.TimeNoon;
										break;
								case TimeOfDay.ah_TimePostNoon://		= 128,		// 2pm
										checkStop = State.TimePostNoon;
										break;
								case TimeOfDay.ai_TimePreDusk://		= 256,		// 4pm
										checkStop = State.TimePreDusk;
										break;
								case TimeOfDay.aj_TimeDusk://			= 512,		// 6pm
										checkStop = State.TimeDusk;
										break;
								case TimeOfDay.ak_TimePostDusk://		= 1024,		// 8pm
										checkStop = State.TimePostDusk;
										break;
								case TimeOfDay.al_TimePreMidnight://	= 2048,		//10pm
										checkStop = State.TimePreMidnight;
										break;
								case TimeOfDay.aa_TimeMidnight://		= 1,		//12am
										checkStop = State.TimeMidnight;
										break;
								default:
										break;
						}

						if (checkStop.RoutineGoal == DailyRoutineGoal.InheritPrior) {	//if this routine goal inherits its prior goal
								//and we haven't gone all the way around the clock yet
								//get the previous stop
								timeOfDay = WorldClock.Get.TimeOfDayBefore(timeOfDay);
								checkStop = GetStop(timeOfDay, ref numTries);
						}
						return checkStop;
				}

				protected bool mFollowingRoutine = false;
				protected MotileAction mLastActionSent = null;
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
				public RoutineStop TimeMidnight = new RoutineStop();
//12am
				public RoutineStop TimePostMidnight = new RoutineStop();
// 2am
				public RoutineStop TimePreDawn = new RoutineStop();
// 4am
				public RoutineStop TimeDawn = new RoutineStop();
// 6am
				public RoutineStop TimePostDawn = new RoutineStop();
// 8am
				public RoutineStop TimePreNoon = new RoutineStop();
//10am
				public RoutineStop TimeNoon = new RoutineStop();
//12pm
				public RoutineStop TimePostNoon = new RoutineStop();
// 2pm
				public RoutineStop TimePreDusk = new RoutineStop();
// 4pm
				public RoutineStop TimeDusk = new RoutineStop();
// 6pm
				public RoutineStop TimePostDusk = new RoutineStop();
// 8pm
				public RoutineStop TimePreMidnight = new RoutineStop();
//10pm
		}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
using Frontiers.Data;
using ExtensionMethods;

namespace Frontiers.World.WIScripts
{
		public class Pilgrim : WIScript, IItemOfInterest//implements item of interest to be a live target for motile actions
		{
				//this class is being reworked to use the newer better path system
				//so there's a lot of old junk waiting to be cleared out
				public PilgrimState State = new PilgrimState();

				public Path ActivePath {
						get {
								return mActivePath;
						}
						set {
								if (!mInitialized) {
										//just set the path, OnInitialized will handle it
										mActivePath = value;
										return;
								}

								if (value == null) {
										//if we're setting the path to null stop following
										State.PathMode = FollowPathMode.None;
										mActivePath = null;
								} else {
										//if we're setting it to a new path
										//start folllowing the path
										mActivePath = value;
										FollowPath();
								}
						}
				}

				public PathMarkerInstanceTemplate LastMarker;
				public PathMarkerInstanceTemplate NextMarker;
				public Character character;
				protected Path mActivePath;

				public bool HasActivePath {
						get {
								return mActivePath != null;
						}
				}
				//public static float gMaxPilgrimStopRange = 5.0f;
				public static float gPathFollowInterval = 0.25f;
				//public PathAvatar CurrentPath = null;
				//public Location StartLocationTarget = null;
				//public Location LastLocationReached = null;
				//public Location NextLocationTarget = null;
				//public Location EndLocationTarget = null;
				//public Obstruction CurrentObstruction = null;
				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnActive += OnActive;
						character = worlditem.Get <Character>();
				}

				public void OnVisible()
				{
						if (HasActivePath && !mFollowingPathOverTime) {
								FollowPath();
						}
				}

				public void OnActive()
				{
						if (HasActivePath && !mFollowingPathOverTime) {
								FollowPath();
						}
				}

				public void AddPilgrimStop(Location location)
				{
						//hooray, we've intercepted a pilgrim stop!
//						mPilgrimStops.Add(location);
						//are we taking directions?
//						if (State.TookDirection && State.PathMode == FollowPathMode.FollowingPath
//						 &&	location.AttachedPaths.Contains(CurrentPath)) {
//								State.PathMode = FollowPathMode.ReachedPilgrimStop;
//						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
//						if (mPilgrimStops.Count > 0) {
//								WIListOption giveDirections = new WIListOption("SkillIconGuildFollowTrail", "Give Directions", "GiveDirections");
//								options.Add(giveDirections);
//						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
//						WIListResult dialogResult = secondaryResult as WIListResult;
//						switch (dialogResult.SecondaryResult) {
//								case "GiveDirections":
//										StartGivingDirections();
//										break;
//
//								default:
//										break;
//						}
				}

				public void StartGivingDirections()
				{
//						if (mFollowingPathOverTime) {
//								//Debug.Log ("canceling following path");
//								StartCoroutine(FinishFollowingPath());
//						}
//						if (mWaitingForDirections) {
//								//Debug.Log ("canceling waiting for directions");
//								CancelWaitingForDirections();
//						}
//
//						CurrentPath = null;
//						bool foundStartLocation = false;
//
//						List <Location> locations	= new List <Location>(mPilgrimStops);
//						Location startLocation = null;
//						float closestDistanceSoFar = Mathf.Infinity;
//						for (int i = locations.Count - 1; i >= 0; i--) {	//cull locations that are null or too far away
//								Location location = locations[i];
//								if (location == null) {	//if it's null, remove it
//										locations.RemoveAt(i);
//								} else {
//										float distance = Vector3.Distance(location.transform.position, transform.position);
//										if (distance > gMaxPilgrimStopRange) {	//if it's too fara way, remove it
//												locations.RemoveAt(i);
//										} else {	//even if it's not the one we want, still add it
//												mPilgrimStops.Add(location);
//												if (distance < closestDistanceSoFar) {
//														closestDistanceSoFar = distance;
//														startLocation = location;
//														foundStartLocation = true;
//												}
//										}
//								}
//						}
//
//						if (!foundStartLocation) {	//well shit they're not near a start location
//								//but don't let that stop us yet - check if the player is visiting an appropriate location
//								Player.Local.Surroundings.CleanPilgrimStops();
//								//Debug.Log ("Checking pilgrim surroundings");
//								if (Player.Local.Surroundings.IsVisitingPilgrimStop) {
//										//Debug.Log ("Visiting pilgrim stop");
//										PilgrimStop pilgrimStop = Player.Local.Surroundings.CurrentPilgrimStop;
//										if (pilgrimStop.worlditem.Is <Location>(out startLocation)) {
//												foundStartLocation = true;
//												//Debug.Log ("Player is visiting a start location");
//										}
//								}
//						}
//
//						if (!foundStartLocation) {
//								if (Paths.HasActivePath) {	//we were almost down for the count but we have a path
//										PathAvatar activePath = Paths.ActivePath;
//										float meters = activePath.MetersFromPosition(transform.position);
//										startLocation = Paths.ActivePath.LocationNearestMeters(meters);
//								}
//						}
//
//						if (foundStartLocation) {	//if we DID find a start location
//								//highlight directions away from paths
//								State.TookDirection = true;
//								Player.Local.Projections.HighlightAttachedPaths(startLocation, new PilgrimCallback(FollowPath));
//								//send information motile
//								Motile motile = null;
//								if (worlditem.Is <Motile>(out motile)) {
//										if (mWaitingForDirectionsAction == null) {
//												mWaitingForDirectionsAction = MotileAction.FocusOnPlayerInRange(25.0f);
//										}
//										mWaitingForDirectionsAction.Reset();
//										motile.PushMotileAction(mWaitingForDirectionsAction, MotileActionPriority.ForceTop);
//
//										if (!mWaitingForDirections) {
//												StartCoroutine(WaitingForDirections(startLocation));
//										}
//								}
//						} else {
//								GUIManager.PostWarning("Neither you nor " + worlditem.FileName + " are near a path location");
//						}
				}

				protected void FollowPath()
				{
						if (LastMarker == null) {
								//if we don't have a 'last marker'
								//get one by finding the closest marker
								LastMarker = Paths.GetMarkerClosestTo(mActivePath, worlditem.Position);
						}
						if (NextMarker == null) {
								//if we don't find a marker in this direction
								if (!Paths.GetNextMarkerInDirection(mActivePath, State.Direction, LastMarker, out NextMarker)) {
										//try another direction
										State.Direction = Paths.ReverseDirection(State.Direction);
										if (!Paths.GetNextMarkerInDirection(mActivePath, State.Direction, LastMarker, out NextMarker)) {
												Debug.Log("Still failed to get next marker, how is that possible?");
												//kill the script
												Finish();
												return;
										}
								}
						}

						if (!mFollowingPathOverTime) {
								mFollowingPathOverTime = true;
								StartCoroutine(FollowPathOverTime());
						}

						//TODO use an animation override in the motile action
						//this is strictly temporary
						character.Body.Animator.animator.SetBool("Walking", true);

				}

				public void FollowPath(MobileReference start, MobileReference target, PathAvatar path, PathDirection direction)
				{
//						//we know the path contains the start destination
//						//now search for the end destination
//						if (path == null || !path.ContainsLocation(start.FileName, out StartLocationTarget)) {	////Debug.Log ("whoops didn't find path or start location");
//								return;
//						}
//
//						//load all this stuff into local vars
//						//we don't need to stop following current path, that'll fix itself
//						CurrentPath = path;
//						//set the first pilgrim stop to the start location and set to reached pilgrim stop
//						//that way we'll force find next neighbor on the first update
//						LastLocationReached = StartLocationTarget;
//						State.PathMode = FollowPathMode.ReachedPilgrimStop;
//						State.LastPathUsed = path.name;
//						State.CurrentStart = start;
//						State.CurrentTarget = target;
//						State.CurrentMeters = path.MetersFromPosition(StartLocationTarget.transform.position);
//						State.StartMeters = State.CurrentMeters;
//						State.Direction = direction;
//
//						if (!MobileReference.IsNullOrEmpty(target)) {
//								if (path.ContainsLocation(target.FileName, out EndLocationTarget)) {
//										State.TargetMeters = path.MetersFromPosition(EndLocationTarget.transform.position);
//								} else {
//										State.TargetMeters = path.LengthInMeters;
//								}
//						} else {
//								EndLocationTarget = null;
//						}
//
//						//send motile action telling character to follow the goal
//						if (mFollowPathAction == null) {
//								mFollowPathAction = new MotileAction();
//						}
//						mFollowPathAction.Reset();
//						mFollowPathAction.Type = MotileActionType.FollowGoal;
//						mFollowPathAction.Expiration = MotileExpiration.Never;
//						mFollowPathAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
//						//mFollowPathAction.LiveTarget = PathFollower; //TODO fix this
//						mFollowPathAction.Target.FileName = "[Pilgrim]";
//						PathFollower.transform.position = transform.position;
//
//						Motile motile = null;
//						if (worlditem.Is <Motile>(out motile)) {
//								motile.PushMotileAction(mFollowPathAction, MotileActionPriority.ForceTop);
//						}
//
//						if (!mFollowingPathOverTime) {
//								StartCoroutine(FollowPathOverTime());
//						}
				}

				#region enumerators

				protected IEnumerator WaitingForDirections(Location startLocation)
				{
//						////Debug.Log ("Starting to wait for directions");
//						mWaitingForDirections = true;
//						bool keepWaiting = true;
//						while (keepWaiting) {
//								switch (mWaitingForDirectionsAction.State) {
//										case MotileActionState.NotStarted:
//										case MotileActionState.Waiting:
//										case MotileActionState.Starting:
//					////Debug.Log ("Getting highlight direction in giving directions");
//												break;
//
//										case MotileActionState.Error:
//										case MotileActionState.Finishing:
//										case MotileActionState.Finished:
//												keepWaiting = false;
//					////Debug.Log ("Error or finished in giving directions");
//												break;
//
//										default:
//												break;
//								}
//
//								if (mWaitingForDirectionsAction.State == MotileActionState.Error) {	//if it's an error then we're done - we cancelled
//										yield break;
//								}
//
//								if (startLocation.worlditem.Is <Visitable>()) {
//										if (!Player.Local.Surroundings.IsVisiting(startLocation)) {	////Debug.Log ("Player is not visiting location any more, so no longer giving directions");
//												keepWaiting = false;
//										}
//								} else {
//										float distance = Vector3.Distance(startLocation.transform.position, Player.Local.Position);
//										if (distance > startLocation.worlditem.ActiveRadius * 2.0f) {
//												////Debug.Log ("Player is not near the location any more, so no longer giving directions");
//												keepWaiting = false;
//										}
//								}
//
//								if (CurrentPath == null) {
//										yield return new WaitForSeconds(0.05f);
//								} else {	////Debug.Log ("Current path is not null");
//										keepWaiting = false;
//								}
//						}
//						GiveDirections();
//						mWaitingForDirections = false;
						yield break;
				}

				protected IEnumerator FollowPathOverTime()
				{
						//create the motile action and push it first
						yield return null;
						if (mFollowPathAction == null) {
								mFollowPathAction = new MotileAction();
								mFollowPathAction.Type = MotileActionType.FollowGoal;
								mFollowPathAction.Expiration = MotileExpiration.Never;
								mFollowPathAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
								mFollowPathAction.LiveTarget = this;
								mFollowPathAction.Range = 2f;
								mFollowPathAction.OutOfRange = 100f;
								mFollowPathAction.Name = "PilgrimFollowPath";
						} else {
								mFollowPathAction.Reset();
						}

						Motile motile = worlditem.Get <Motile>();
						motile.PushMotileAction(mFollowPathAction, MotileActionPriority.ForceTop);
						State.PathMode = FollowPathMode.FollowingPath;

						yield return null;
						while (State.PathMode != FollowPathMode.None) {	//check the state of the motile action we submitted
								switch (mFollowPathAction.State) {
										case MotileActionState.Started:
										case MotileActionState.Starting:
												//hooray, it has started, update the path
												//set it to reached pilgrim stop to force us to go to the next stop
												var updateFollowPath = UpdateFollowPath();
												while (updateFollowPath.MoveNext()) {
														yield return updateFollowPath.Current;
												}
												//now see if the path has finished or whatever
												switch (State.PathMode) {
														case FollowPathMode.None:
																//case FollowPathMode.ReachedEndOfPath:
																//hooray we're done
																//yield return StartCoroutine(FinishFollowingPath());
																break;

														default:
																//keep going
																//this includes waiting for an obstruction
																break;
												}
												break;

										case MotileActionState.Error:
										case MotileActionState.Finished:
												//whoops, we're done on the Motile end for some reason
												//maybe we're dead or maybe we got a conflicting command
												yield return StartCoroutine(FinishFollowingPath());
												break;

										case MotileActionState.Waiting:
												//we're being talked to or something, just hang out
												break;

										default:
												break;
								}
								//wait for a bit
								yield return gWaitForFollowPath;
						}
						mFollowingPathOverTime = false;
						yield break;
				}

				protected static WaitForSeconds gWaitForFollowPath = new WaitForSeconds (gPathFollowInterval);

				protected IEnumerator FinishFollowingPath()
				{
						State.PathMode = FollowPathMode.None;
//						CurrentPath = null;
//						CurrentObstruction = null;
//						EndLocationTarget = null;
//						StartLocationTarget = null;
//						NextLocationTarget = null;
//						LastLocationReached = null;

						mFollowPathAction.TryToFinish();

						if (State.TookDirection) {
								GUI.GUIManager.PostWarning(worlditem.DisplayName + " needs directions.");
						}
						yield break;
				}

				public void OnFastTravelFrame()
				{
					
				}

				protected IEnumerator UpdateFollowPath()
				{
						//check if we're still supposed to be doing this
						if (!HasActivePath) {//whoops stuff is gone
								State.PathMode = FollowPathMode.None;
								yield break;
						} else {
								switch (State.PathMode) {
										case FollowPathMode.ReachedEndOfPath:
												//TODO instead of ping-ponging, make it possible for pilgrims
												//to choose another path to use
												//Paths.ReverseDirection(State.Direction);
												//Paths.GetNextMarkerInDirection(ActivePath, State.Direction, LastMarker, out NextMarker);
												State.PathMode = FollowPathMode.FollowingPath;
												break;

										case FollowPathMode.ReachedPilgrimStop:
//												PilgrimStop pilgrimStop = null;
//												if (LastLocationReached.worlditem.Is <PilgrimStop>(out pilgrimStop)
//												&&	LastLocationReached != StartLocationTarget
//												&&	State.TookDirection) {	//if we've hit a pilgrim stop AND we're not at the start location AND we took direction
//														//move to the pilgrim stop after which we'll wait for directions
//														State.PathMode = FollowPathMode.MovingToPilgrimStop;
//														PathFollower.transform.position = LastLocationReached.RandomPilgrimPosition + Vector3.up;
//												} else if (CurrentPath.GetNeighbor(LastLocationReached, State.Direction, new List <string>(), out NextLocationTarget)) {	//if we've reached the pilgrim stop and we can still get a neighbor
//														//set the follower to the next node
//														////Debug.Log ("Reached location " + LastLocationReached.name + ", got next in line, going to " + NextLocationTarget.name);
//														State.PathMode = FollowPathMode.FollowingPath;
//														PathFollower.transform.position = NextLocationTarget.transform.position + Vector3.up;
//												} else {	//otherwise we've reached the end of the path
//														////Debug.Log ("Reached end of path after hitting pilgrim stop");
//														State.PathMode = FollowPathMode.ReachedEndOfPath;
//												}
												break;

										case FollowPathMode.MovingToPilgrimStop:
												//move next to the pilgrim stop
//												if (Vector3Extensions.Distance2D(transform.position, PathFollower.transform.position) < 1.0f) {	//if we're really close then we've reached end of path
//														State.PathMode = FollowPathMode.ReachedEndOfPath;
//												}
												break;

										case FollowPathMode.WaitingForObstruction:
												//if the obstruction is null or no longer obstructs the path
//												if (CurrentObstruction == null || !CurrentObstruction.ObstructedPaths.Contains(State.LastPathUsed)) {	//continue walking along the path
//														State.PathMode = FollowPathMode.FollowingPath;
//												}
												break;

										case FollowPathMode.FollowingPath:
										default:
//												State.CurrentMeters = CurrentPath.MetersFromPosition(transform.position);
//												see if we're ready to move to next point along meters
//												Vector3 pathPoint 		= PathFollower.position;
//												Vector3 normal			= Vector3.zero;
//												bool hitWater			= false;
//												bool hitTerrainMesh		= false;
//
//												if (WorldItems.IsInActiveRange (NextLocationTarget.worlditem, transform.position, gMaxPilgrimStopRange)) {	//if we've gotten to the location
//													////Debug.Log ("Reached location target " + NextLocationTarget.name);
//													State.PathMode = FollowPathMode.ReachedPilgrimStop;
//													LastLocationReached = NextLocationTarget;
//													//wait a tad
//													yield return new WaitForSeconds (0.5f);
//												}
//												if (Vector3.Distance (pathPoint, transform.position) < gPathFollowGoalRange)
//												{	////Debug.Log ("Moved goal object");
//													//we're ready to move the goal farther along the path
//													else if (Vector3.Distance (transform.position, EndLocationTarget.transform.position) < gPathFollowGoalRange)
//													{	//if this returns false, that means we've reached the goal meters
//														//Debug.Log ("Reached end of path at " + State.CurrentMeters + " due to proximity");
//														State.PathMode = FollowPathMode.ReachedEndOfPath;
//													}
//													pathPoint 	= CurrentPath.PositionFromMeters (State.CurrentMeters);
//													pathPoint.y = GameWorld.Get.TerrainHeightAtInGamePosition (pathPoint, true, ref hitWater, ref hitTerrainMesh, ref normal) + 0.5f;
//												}
												if (mFollowPathAction.IsInRange) {
														//get the next marker after the current marker
														PathMarkerInstanceTemplate newGoal = null;
														if (Paths.GetNextMarkerInDirection(mActivePath, State.Direction, NextMarker, out newGoal)) {
																LastMarker = NextMarker;
																NextMarker = newGoal;
														} else {
																//reverse direction!
																State.Direction = Paths.ReverseDirection(State.Direction);
																//go to the last marker instead
																newGoal = LastMarker;
																LastMarker = NextMarker;
														}
												}
												break;
								}
								//wait for a tick
								yield return null;
						}
						yield break;
				}

				protected void CancelWaitingForDirections()
				{
						if (mWaitingForDirections) {
								////Debug.Log ("stopped waiting for player to give directions");
								Player.Local.Projections.StopHighlightingPaths(false);
								mWaitingForDirectionsAction.Cancel();
						}
				}

				protected void GiveDirections()
				{
						if (mWaitingForDirections) {
								////Debug.Log ("stopped waiting for player to give directions");
								Player.Local.Projections.StopHighlightingPaths(true);
								//mWaitingForDirectionsAction.TryToFinish ( );
						}
				}

				#endregion

				public void OnDrawGizmos()
				{
						if (HasActivePath) {
								Gizmos.color = Color.cyan;
								if (State.PathMode == FollowPathMode.WaitingForObstruction) {
										Gizmos.color = Color.red;
								}
								Gizmos.DrawWireSphere(LastMarker.Position, 0.5f);
								if (NextMarker != null) {
										Gizmos.color = Color.green;
										Gizmos.DrawWireSphere(NextMarker.Position, 0.5f);
								}
						}
				}

				#region IItemOfInterest implementation

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.None; } }

				public Vector3 Position {
						get {
								if (NextMarker != null) {
										return NextMarker.Position;
								} else {
										return worlditem.Position;
								}
						}
				}

				public Vector3 FocusPosition {
					get {
						return Position;
					}
				}

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						return false;
				}

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public GameObject gameObject { get { return null; } }

				public bool Destroyed { get { return false; } }

				public bool HasPlayerFocus { get; set; }

				#endregion

				protected bool mWaitingForDirections = false;
				protected bool mFollowingPathOverTime = false;
				protected MotileAction mFollowPathAction = null;
				protected MotileAction mWaitingForDirectionsAction = null;
				//protected HashSet <Location> mPilgrimStops = new HashSet <Location>();
				//protected Dictionary <int, KeyValuePair <string,MobileReference>> mLastOptionListFlavors = new Dictionary <int, KeyValuePair <string,MobileReference>>();
		}

		[Serializable]
		public class PilgrimState
		{
				public FollowPathMode PathMode = FollowPathMode.None;
				public PathDirection Direction = PathDirection.None;
				public bool TookDirection = false;
				public float StartMeters = 0.0f;
				public float CurrentMeters = 0.0f;
				public float TargetMeters = 0.0f;
				public string LastPathUsed = string.Empty;
				public MobileReference CurrentTarget = MobileReference.Empty;
				public MobileReference CurrentStart = MobileReference.Empty;
		}
}
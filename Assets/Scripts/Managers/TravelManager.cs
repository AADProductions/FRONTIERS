using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;
using Frontiers.GUI;
using System;

namespace Frontiers
{
		public class TravelManager : Manager
		{
				public static TravelManager Get;

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						DesiredLocationTypes = PathMarkerType.PathOrigin;
						State = FastTravelState.None;
				}

				public FastTravelState State = FastTravelState.None;
				public float TimeScaleTravel = 1.0f;
				public float MaxFastTravelWaitRadius = 3f;
				public float MovementThreshold = 0.1f;
				public GUIFastTravelInterface FastTravelInterface;
				public PathMarkerType DesiredLocationTypes;
				public StatusKeeper StrengthStatusKeeper;
				public List<FastTravelChoice> AvailablePathMarkers = new List <FastTravelChoice>();
				public FastTravelChoice CurrentChoice;

				public int StartMarkerIndex {
						get {
								if (CurrentChoice != null) {
										return CurrentChoice.StartMarkerIndex;
								}
								return 0;
						}
				}

				public int EndMarkerIndex {
						get {
								if (CurrentChoice != null) {
										return CurrentChoice.EndMarkerIndex;
								}
								return 0;
						}
				}

				public PathDirection CurrentDirection {
						get {
								if (CurrentChoice != null) {
										return CurrentChoice.Direction;
								}
								return PathDirection.None;
						}
				}

				public PathMarker LastVisitedPathMarker;
				public PathMarkerInstanceTemplate LastFastTravelStartPathMarker;
				public PathSegment CurrentSegment;
				public float PathStartMeters;
				public float PathEndMeters;
				public float PathCurrentMeters;
				public Vector3 CurrentOrientation;
				public Vector3 CurrentPosition;
				public string LastChosenPath;
				public PathMarkerInstanceTemplate LastChosenPathMarker;

				public bool HasReachedOrPassedDestination {
						get {
								if (CurrentDirection == PathDirection.Forward) {
										//Debug.Log("We're going forward, is path end position less than current meters?" + (PathEndMeters <= PathCurrentMeters).ToString());
										return PathEndMeters <= PathCurrentMeters;
								} else {
										//Debug.Log("We're going backwards, is path end position greater than current meters?" + (PathEndMeters >= PathCurrentMeters).ToString());
										return PathEndMeters >= PathCurrentMeters;
								}
						}
				}

				public override void OnGameStart()
				{
						FastTravelInterface.TimeScaleTravelMax = Globals.TimeScaleTravelMax;
						FastTravelInterface.TimeScaleTravelMin = Globals.TimeScaleTravelMin;
						FastTravelInterface.TimeScaleTravelSlider.sliderValue = 1f;
				}

				public bool FastTravel(PathMarker startingPathMarker)
				{
						if (State == FastTravelState.None) {
								if (startingPathMarker.HasPathMarkerProps) {
										OnStartTraveling(startingPathMarker.Props, startingPathMarker.Props.PathName);
										State = FastTravelState.ArrivingAtDestination;
								} else {
										Debug.Log("Starting path maker had no properties");
								}
						}
						return true;
				}

				public void CancelTraveling()
				{
						if (State != FastTravelState.None) {
								State = FastTravelState.Finished;
								Player.Local.Projections.ClearDirectionalArrows();
						}
				}

				protected void OnStartTraveling(PathMarkerInstanceTemplate startingPathMarker, string pathName)
				{
						//Debug.Log("Starting traveling on path " + pathName);
						Player.Get.AvatarActions.ReceiveAction((AvatarAction.FastTravelStart), WorldClock.AdjustedRealTime);

						LastFastTravelStartPathMarker = startingPathMarker;
						LastChosenPathMarker = startingPathMarker;

						Player.Local.Status.GetStatusKeeper("Strength", out StrengthStatusKeeper);
				}

				protected void OnFinishTraveling()
				{
						//Debug.Log("Finished traveling");
						Player.Local.Projections.ClearDirectionalArrows();
						State = FastTravelState.None;
						Player.Get.AvatarActions.ReceiveAction((AvatarAction.FastTravelStop), WorldClock.AdjustedRealTime);
						FastTravelInterface.Minimize();
						CurrentChoice = null;

						if (Player.Local.IsHijacked) {
								mTerrainHit.feetPosition = CurrentPosition;
								CurrentPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit) + 0.05f;//just in case, pad it out
								//WorldClock.Get.SetTargetSpeed (1.0f);
								Player.Local.HijackedPosition.position = CurrentPosition + Player.Local.Height;
								Player.Local.Position = CurrentPosition;
								Player.Local.RestoreControl(true);
						}
				}

				public void ConsiderChoice(FastTravelChoice choice)
				{
						//TODO show the path name or something
				}

				protected float mLookMetersDownPath;
				protected Vector3 mLookMetersPosition;
				protected PathDirection mLookDirection;

				public void MakeChoice(FastTravelChoice choice)
				{
						Player.Local.HijackControl();
						Player.Local.HijackLookSpeed = 0.05f;

						CurrentChoice = choice;
						//convert the current choice's indexes & whatnot into start / end spline params
						LastChosenPath = choice.ConnectingPath;
						Paths.SetActivePath(LastChosenPath, GameWorld.Get.PrimaryChunk);
						CurrentPosition = Player.Local.Position;
						CurrentOrientation = Player.Local.Rotation.eulerAngles;
						PathEndMeters = Paths.ActivePath.MetersFromPosition(CurrentChoice.EndMarker.Position);
						PathStartMeters = Paths.ActivePath.MetersFromPosition(CurrentChoice.StartMarker.Position);
						PathCurrentMeters = PathStartMeters;
						//go!
						State = FastTravelState.Traveling;
						//enabled = true;
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "FastTravelMakeChoice");
				}

				public void OnConfirmStop()
				{
						CancelTraveling();
				}

				public void Update()
				{
						switch (State) {
								default:
								case FastTravelState.None:
										break;

								case FastTravelState.ArrivingAtDestination:
										WaitForNextChoice();
										break;

								case FastTravelState.WaitingForNextChoice:
										//this will loop about until traveling is either started or cancelled
										if ((mNextChoiceWaitStartTime + 2f) < WorldClock.AdjustedRealTime) {
												if (Vector3.Distance(Player.Local.Position, Player.Local.Projections.DirectionArrowsParent.position) > MaxFastTravelWaitRadius) {
														//Debug.Log("Wait radius exceeded wait distance " + Vector3.Distance(Player.Local.Position, Player.Local.Projections.DirectionArrowsParent.position).ToString());
														State = FastTravelState.Finished;
												}
										}
										return;

								case FastTravelState.Traveling:
										//this will update our position over time
										UpdateTraveling();
										break;

								case FastTravelState.Finished:
										OnFinishTraveling();
										break;
						}
				}

				protected GameWorld.TerrainHeightSearch mTerrainHit;
				protected double mMovePlayerInerval = 0.5f;
				protected double mNextMovePlayerTime = 0f;

				protected void UpdateTraveling()
				{
						if (!Paths.HasActivePath) {
								return;
						}

						if (Paths.IsEvaluating) {
								return;
						}

						if (StrengthStatusKeeper.NormalizedValue < 0f) {
								GUIManager.PostDanger("You're too exhausted to fast travel");
								State = FastTravelState.Finished;
								return;
						}

						//Debug.Log("Updating traveling... we're at " + PathCurrentMeters.ToString() + " and moving towards " + PathEndMeters.ToString());

						CurrentSegment = Paths.ActivePath.SegmentFromMeters(PathCurrentMeters);
						float metersToMove = (float)(TimeScaleTravel * WorldClock.RTDeltaTime);
						PathCurrentMeters = Paths.MoveAlongPath(PathCurrentMeters, metersToMove, CurrentDirection);
						CurrentPosition = Paths.ActivePath.PositionFromMeters(PathCurrentMeters);
						mTerrainHit.feetPosition = CurrentPosition;
						mTerrainHit.overhangHeight = 2f;
						mTerrainHit.groundedHeight = 2f;
						mTerrainHit.ignoreWorldItems = false;
						CurrentPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit) + Player.Local.Height.y;

						//Debug.Log("We're now at " + PathCurrentMeters.ToString());

						if (mTerrainHit.hitWater) {
								GUIManager.PostDanger("You can't fast travel over water");
								CurrentPosition.y += 2f;//TEMP just to be safe
								CancelTraveling();
								return;
						}

						//make sure hijacked position is facing the right direction, etc
						Player.Local.State.HijackMode = PlayerHijackMode.OrientToTarget;
						Player.Local.HijackedPosition.position = CurrentPosition;
						Player.Local.HijackedPosition.rotation = Quaternion.identity;
						Player.Local.HijackLookSpeed = Globals.PlayerHijackLerp;


						CurrentOrientation = Paths.ActivePath.OrientationFromMeters(PathCurrentMeters, true);
						Player.Local.HijackedPosition.transform.Rotate(CurrentOrientation);
						if (CurrentDirection == PathDirection.Backwards) {
								Player.Local.HijackedPosition.transform.Rotate(0f, 180f, 0f);
						}

						if (HasReachedOrPassedDestination) {
								//Debug.Log("Has reached destination...");
								ArriveAtDestination();
						}

						//update the passage of time
						//fast travel time is updated according to how many meters the player has moved since the last update
						//the number of meters per second is deterimed by the player's motor throttle speed
						//plus the skill level of fast travel
						//(i've tried it a half-dozen different ways and this one is the most stable)
						double timeAdvanced = (metersToMove / Player.Local.MotorAccelerationMultiplier) * 0.1f;
						StrengthStatusKeeper.ChangeValue(Globals.FastTravelStrengthReducedPerMeterTraveled * metersToMove, StatusSeekType.Negative);
						WorldClock.AddARTDeltaTime(timeAdvanced);
				}

				protected void ArriveAtDestination()
				{
						LastChosenPathMarker = CurrentChoice.EndMarker;
						CurrentChoice = null;
						State = FastTravelState.ArrivingAtDestination;
				}

				protected void WaitForNextChoice()
				{
						mNextChoiceWaitStartTime = WorldClock.AdjustedRealTime;
						AvailablePathMarkers.Clear();
						Paths.GetAllNeighbors(LastChosenPathMarker, DesiredLocationTypes, AvailablePathMarkers);
						//see if the player is holding down the forward key - if they are, keep moving
						bool canSkipNextJunction = AvailablePathMarkers.Count > 1;
						Vector3 currentDirection = Player.Local.ForwardVector;
						FastTravelChoice bestChoiceSoFar = AvailablePathMarkers[0];
						if ((Mathf.Abs(UserActionManager.RawMovementAxisX) > MovementThreshold || Math.Abs(UserActionManager.RawMovementAxisY) > MovementThreshold) && canSkipNextJunction) {
								Debug.Log("Skipping junction");
								if (AvailablePathMarkers.Count == 2) {
										//choose the one opposite us
										if (bestChoiceSoFar.ConnectingPath == LastChosenPath) {
												bestChoiceSoFar = AvailablePathMarkers[1];
										}
								} else {
										float smallestDotSoFar = Mathf.Infinity;
										foreach (FastTravelChoice choice in AvailablePathMarkers) {
												Vector3 nextDirection = choice.StartMarker.Position - choice.FirstInDirection.Position;
												float dot = Vector3.Dot(currentDirection.normalized, nextDirection.normalized);
												if (dot < smallestDotSoFar) {
														smallestDotSoFar = dot;
														bestChoiceSoFar = choice;
												}
										}
								}
								MakeChoice(bestChoiceSoFar);
						} else {
								if (Player.Local.IsHijacked) {
										//let the player walk around again
										mTerrainHit.feetPosition = CurrentPosition;
										mTerrainHit.overhangHeight = 2f;
										mTerrainHit.groundedHeight = 3f;
										mTerrainHit.ignoreWorldItems = true;
										CurrentPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit) + 0.0125f;//just in case, pad it out
										Player.Local.Position = CurrentPosition;
										Debug.Log(CurrentPosition.y.ToString());
										Player.Local.RestoreControl(true);
								}
								Player.Local.Projections.ShowFastTravelChoices(LastChosenPathMarker, AvailablePathMarkers);
								FastTravelInterface.Maximize();
								State = FastTravelState.WaitingForNextChoice;
						}
				}

				protected double mNextChoiceWaitStartTime = 0f;
				protected double mLastTimePressedForward = 0f;
				protected double mPressedForwardInterval = 0.5f;

				[Serializable]
				public class FastTravelChoice
				{
						public PathMarkerInstanceTemplate StartMarker;
						public PathMarkerInstanceTemplate FirstInDirection;
						public PathMarkerInstanceTemplate EndMarker;
						public int StartMarkerIndex;
						public int EndMarkerIndex;
						public PathDirection Direction;
						public string ConnectingPath;
				}
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;
using Frontiers.GUI;

namespace Frontiers
{
		public class TravelManager : Manager
		{
				public static TravelManager Get;

				public override void WakeUp()
				{
						Get = this;
						DesiredLocationTypes = PathMarkerType.PathOrigin;
						State = FastTravelState.None;
				}

				public FastTravelState State = FastTravelState.None;
				public float TimeScaleTravel = 1.0f;
				public PathMarker LastVisitedPathMarker;
				public PathMarker LastFastTravelStartPathMarker;
				public string LastChosenPath;
				public PathSegment CurrentSegment;
				public float PathStartPosition;
				public float PathEndPosition;
				public float PathCurrentMeters;
				public float LastChosenPathMarkerPathPosition;
				public Vector3 CurrentOrientation;
				public Vector3 CurrentPosition;
				public int StartMarkerIndexInPath;
				public int EndMarkerIndexInPath;
				public Dictionary <PathMarkerInstanceTemplate,int> AvailablePathMarkers = new Dictionary<PathMarkerInstanceTemplate, int>();
				public PathDirection CurrentDirection;
				public PathMarkerInstanceTemplate LastChosenPathMarker;
				public GUIFastTravelInterface FastTravelInterface;
				public PathMarkerType DesiredLocationTypes;
				public StatusKeeper StrengthStatusKeeper;
				#if UNITY_EDITOR
				public List <string> BranchesInLastChoice = new List <string>();
				public List <string> BranchesInCurrentChoice = new List <string>();
				#endif
				public bool HasReachedOrPassedLastChosenPathMarker {
						get {
								if (CurrentDirection == PathDirection.Forward) {
										return LastChosenPathMarkerPathPosition <= PathCurrentMeters;
								} else {
										return LastChosenPathMarkerPathPosition >= PathCurrentMeters;
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
								OnStartTraveling(startingPathMarker);
						}
						return true;
				}

				public void CancelTraveling()
				{
						State = FastTravelState.Finished;
				}

				protected void OnStartTraveling(PathMarker startingPathMarker)
				{
						State = FastTravelState.ArrivingAtDestination;
						Player.Get.AvatarActions.ReceiveAction((AvatarAction.FastTravelStart), WorldClock.AdjustedRealTime);

						LastFastTravelStartPathMarker = startingPathMarker;
						LastChosenPathMarker = startingPathMarker.Props;
						StartMarkerIndexInPath = LastChosenPathMarker.IndexInParentPath;
						LastChosenPath = LastChosenPathMarker.ParentPath.Name;
						Paths.SetActivePath(LastChosenPath, GameWorld.Get.PrimaryChunk);
						PathStartPosition = 0f;
						PathEndPosition = Paths.ActivePath.MetersFromPosition(LastChosenPathMarker.Position);
						PathCurrentMeters = Paths.ActivePath.MetersFromPosition(LastChosenPathMarker.Position);
						CurrentPosition = Player.Local.Position;
						Player.Local.Status.GetStatusKeeper("Strength", out StrengthStatusKeeper);

						#if UNITY_EDITOR
						BranchesInLastChoice.Clear();
						BranchesInCurrentChoice.Clear();

						foreach (KeyValuePair <string,int> branch in LastChosenPathMarker.Branches) {
								BranchesInLastChoice.Add(branch.Key + " - " + branch.Value.ToString());
						}
						#endif

						Player.Local.HijackControl();
						Player.Local.HijackLookSpeed = 0.05f;
				}

				protected void OnFinishTraveling()
				{
						State = FastTravelState.None;
						Player.Get.AvatarActions.ReceiveAction((AvatarAction.FastTravelStop), WorldClock.AdjustedRealTime);
						FastTravelInterface.Minimize();
						mTerrainHit.feetPosition = CurrentPosition;
						CurrentPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit) + 0.25f;//just in case, pad it out
						Player.Local.Position = CurrentPosition;
						//WorldClock.Get.SetTargetSpeed (1.0f);

						Player.Local.RestoreControl(true);
				}

				public void ConsiderChoice(KeyValuePair <PathMarkerInstanceTemplate,int> choice, WorldChunk chunk)
				{
						if (State == FastTravelState.WaitingForNextChoice) {
								#if UNITY_EDITOR
								BranchesInCurrentChoice.Clear();
								foreach (KeyValuePair <string,int> branch in choice.Key.Branches) {
										BranchesInCurrentChoice.Add(branch.Key + " - " + branch.Value.ToString());
								}
								#endif

								//okay, we're considering this choice
								//that means we want the path that attaches our current path marker to the new choice
								//first check if they both belong to the same path
								LastChosenPath = string.Empty;
								if (choice.Key.PathName == LastChosenPathMarker.PathName) {
										LastChosenPath = choice.Key.PathName;
								} else {
										//look in all the branches of our last chosen path marker
										//and find the one that links them
										foreach (KeyValuePair <string,int> lastChoiceBranch in LastChosenPathMarker.Branches) {
												foreach (KeyValuePair <string,int> currentChoiceBranch in choice.Key.Branches) {
														if (lastChoiceBranch.Key == currentChoiceBranch.Key) {
																//found the link
																LastChosenPath = currentChoiceBranch.Key;
																break;
														}
												}
										}
								}
								//look in that direction - approximately 10 meters down the path
								Paths.SetActivePath(LastChosenPath, GameWorld.Get.PrimaryChunk);
								mLookMetersDownPath = Paths.ActivePath.MetersFromPosition(choice.Key.Position);
								if (LastChosenPathMarker.IndexInParentPath < choice.Value) {
										mLookDirection = PathDirection.Forward;
								} else {
										mLookDirection = PathDirection.Backwards;
								}
								mLookMetersDownPath = Paths.MoveAlongPath(mLookMetersDownPath, 10f, mLookDirection);
								mLookMetersPosition = Paths.ActivePath.PositionFromMeters(mLookMetersDownPath);

								Player.Local.HijackedPosition.position = LastChosenPathMarker.Position + Player.Local.Height;
								Player.Local.HijackedLookTarget.position = mLookMetersPosition;
								Player.Local.HijackLookSpeed = 0.05f;//slow this way down so we're not zipping around
						}
				}

				protected float mLookMetersDownPath;
				protected Vector3 mLookMetersPosition;
				protected PathDirection mLookDirection;

				public void MakeChoice(KeyValuePair <PathMarkerInstanceTemplate,int> choice, WorldChunk chunk)
				{
						#if UNITY_EDITOR
						BranchesInLastChoice.Clear();
						BranchesInLastChoice.AddRange(BranchesInCurrentChoice);
						BranchesInCurrentChoice.Clear();
						#endif

						//the choice will always be considered before it's made
						//so last chosen path will be active
						//figure out whether we're going forwards or backwards
						StartMarkerIndexInPath = EndMarkerIndexInPath;
						EndMarkerIndexInPath = choice.Value;
						if (StartMarkerIndexInPath < EndMarkerIndexInPath) {
								CurrentDirection = PathDirection.Forward;
						} else {
								CurrentDirection = PathDirection.Backwards;
						}

						//our start position is the meters from the position of our last chosen marker
						PathStartPosition = Paths.ActivePath.MetersFromPosition(LastChosenPathMarker.Position);
						//we're starting at this position
						PathCurrentMeters = PathStartPosition;
						LastChosenPathMarker = choice.Key;
						//our end position is the meters from the position of our newly chosen marker
						LastChosenPathMarkerPathPosition = Paths.ActivePath.MetersFromPosition(LastChosenPathMarker.Position);
						PathEndPosition = LastChosenPathMarkerPathPosition;
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

						CurrentSegment = Paths.ActivePath.SegmentFromMeters(PathCurrentMeters);
						float metersToMove = (float)(TimeScaleTravel * WorldClock.RTDeltaTime);// * Paths.PathDifficultyToMetersPerHour (CurrentSegment.Difficulty));
						PathCurrentMeters = Paths.MoveAlongPath(PathCurrentMeters, metersToMove, CurrentDirection);
						CurrentPosition = Paths.ActivePath.PositionFromMeters(PathCurrentMeters);
						mTerrainHit.feetPosition = CurrentPosition;
						mTerrainHit.overhangHeight = 100f;
						mTerrainHit.groundedHeight = 10f;
						CurrentPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);//CurrentPosition, passOverSolidTerrain, ref hitWater, ref hitTerrainMesh, ref normal);

						if (mTerrainHit.hitWater) {
								GUIManager.PostDanger("You can't fast travel over water");
								CurrentPosition.y += 2f;//TEMP just to be safe
								CancelTraveling();
								return;
						}

						//make sure hijacked position is facing the right direction, etc
						Player.Local.State.HijackMode = PlayerHijackMode.OrientToTarget;
						Player.Local.HijackedPosition.position = CurrentPosition + Player.Local.Height;
						Player.Local.HijackedPosition.rotation = Quaternion.identity;
						Player.Local.HijackLookSpeed = Globals.PlayerHijackLerp;

						CurrentOrientation = Paths.ActivePath.OrientationFromMeters(PathCurrentMeters, true);
						Player.Local.HijackedPosition.transform.Rotate(CurrentOrientation);
						if (CurrentDirection == PathDirection.Backwards) {
								Player.Local.HijackedPosition.transform.Rotate(0f, 180f, 0f);
						}

						if (HasReachedOrPassedLastChosenPathMarker) {
								State = FastTravelState.ArrivingAtDestination;
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

				protected void WaitForNextChoice()
				{
						//WorldClock.Get.SetTargetSpeed (0f);
						AvailablePathMarkers.Clear();
						Paths.GetAllNeighbors(LastChosenPathMarker, DesiredLocationTypes, AvailablePathMarkers);
						Player.Local.State.HijackMode = PlayerHijackMode.LookAtTarget;
						Player.Local.HijackedPosition.transform.position = LastChosenPathMarker.Position + Player.Local.Height;
						FastTravelInterface.AddAvailablePathMarkers(AvailablePathMarkers);
						if (FastTravelInterface.Maximize()) {
								State = FastTravelState.WaitingForNextChoice;
						} else {
								//something's up, we can't maximize interface
								State = FastTravelState.Finished;
						}
				}
		}
}
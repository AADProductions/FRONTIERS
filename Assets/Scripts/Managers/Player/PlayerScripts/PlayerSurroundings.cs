using UnityEngine;
using System;
using Frontiers.Data;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using System.Text;

namespace Frontiers
{
		public class PlayerSurroundings : PlayerScript, IPhotosensitive
		{
				public PlayerSurroundingsState State = new PlayerSurroundingsState();
				//TODO remove this
				public List <string> VisitNotificationTypes = new List <string> { "City" };
				public Structure StartupStructure = null;

				#region initialization

				public override void OnGameStart()
				{
						if (GameManager.Get.TestingEnvironment)
								return;

						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveEnterWater, new ActionListener(MoveEnterWater));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveExitWater, new ActionListener(MoveExitWater));

						for (int i = 0; i < GameWorld.Get.Settings.DefaultRevealedLocations.Count; i++) {
								Profile.Get.CurrentGame.RevealedLocations.SafeAdd(GameWorld.Get.Settings.DefaultRevealedLocations[i]);
						}

						for (int i = 0; i < GameWorld.Get.Regions.Count; i++) {
								State.VisitedRespawnStructures.SafeAdd(GameWorld.Get.Regions[i].DefaultRespawnStructure);
						}

						FXManager.Get.SpawnMapMarkers(State.ActiveMapMarkers);

						enabled = true;
				}

				public override void OnLocalPlayerDie()
				{
						FireSources.Clear();
						LightSources.Clear();
				}

				public override void OnLocalPlayerSpawn()
				{
						if (StartupStructure != null) {
								StructureEnter(StartupStructure);
								StartupStructure = null;
						}

						if (SpawnManager.Get.UseStartupPosition) {
								PlayerStartupPosition psp = SpawnManager.Get.CurrentStartupPosition;

								if (psp.ClearRevealedLocations) {
										Profile.Get.CurrentGame.RevealedLocations.Clear();
								}

								State.IsInWater = false;
								State.GroundBeneathPlayer = GroundType.Dirt;
								State.LastChunkID = psp.ChunkID;
								State.LastPosition.CopyFrom(psp.WorldPosition);
								State.VisitingLocations.SafeAdd(psp.LocationReference);
								Profile.Get.CurrentGame.RevealedLocations.SafeAdd(psp.LocationReference);
								State.IsInsideStructure = psp.Interior;
								if (State.IsInsideStructure) {
										State.LastStructureEntered = psp.LocationReference.AppendLocation(psp.StructureName);
										Profile.Get.CurrentGame.RevealedLocations.SafeAdd(State.LastStructureEntered);
								}
								//add the latest respawn structures to our state
								for (int i = 0; i < SpawnManager.Get.CurrentStartupPosition.NewVisitedRespawnStructures.Count; i++) {
										State.VisitedRespawnStructures.SafeAdd(SpawnManager.Get.CurrentStartupPosition.NewVisitedRespawnStructures[i]);
								}
						}

						if (!mCheckingSurroundings) {
								mCheckingSurroundings = true;
								StartCoroutine(CheckSurroundings());
						}
				}

				public override void OnLocalPlayerDespawn()
				{
						LeaveAllLocations();
						State.IsInWater = false;
						State.GroundBeneathPlayer = GroundType.Dirt;
				}

				public override void AdjustPlayerMotor(ref float mMotorAccelerationMultiplier, ref float mMotorJumpForceMultiplier, ref float mMotorSlopeAngleMultiplier)
				{
						if (IsInWater) {
								mMotorAccelerationMultiplier *= Globals.DefaultWaterAccelerationPenalty;
								mMotorJumpForceMultiplier *= Globals.DefaultWaterJumpPenalty;
						}
				}

				#endregion

				#region immediate surroundings

				//convenience functions - null checks were ambiguous
				//these make it easier to tell what i'm asking
				public string CurrentDescription {
						get {
								BuildDescription();
								return mCurrentDescription;
						}
				}

				protected string mCurrentDescription = string.Empty;

				public bool IsSomethingBelowPlayer {
						get {
								return ((WorldItemBelow != null && !WorldItemBelow.Destroyed)
								|| (TerrainBelow != null && !TerrainBelow.Destroyed));
						}
				}

				public bool IsSomethingAbovePlayer {
						get {
								return ((WorldItemAbove != null && !WorldItemAbove.Destroyed)
								|| (TerrainAbove != null && !TerrainAbove.Destroyed));
						}
				}

				public bool IsSomethingInFrontofPlayer {
						get {
								return ((WorldItemForward != null && !WorldItemForward.Destroyed)
								|| (TerrainForward != null && !TerrainForward.Destroyed));
						}
				}

				public bool IsSomethingInPlayerFocus {
						get {
								return ((WorldItemFocus != null && !WorldItemFocus.Destroyed)
								|| (TerrainFocus != null && !TerrainFocus.Destroyed));
						}
				}

				public bool IsSomethingInRange {
						get {
								return (ClosestObjectInRange != null && !ClosestObjectInRange.Destroyed);
						}
				}

				public bool IsWorldItemInPlayerFocus {
						get {
								return (WorldItemFocus != null && !WorldItemFocus.Destroyed);
						}
				}

				public bool IsTerrainInPlayerFocus {
						get {
								return (TerrainFocus != null && !TerrainFocus.Destroyed);
						}
				}

				public bool IsWorldItemInRange {
						get {
								return (IsWorldItemInPlayerFocus
								&& Vector3.Distance(player.Position, WorldItemFocusHitInfo.point) < Globals.PlayerPickUpRange
								&& ClosestObjectFocus == WorldItemFocus);
						}
				}

				public bool IsTerrainInRange {
						get {
								return (IsTerrainInPlayerFocus && Vector3.Distance(player.Position, TerrainFocusHitInfo.point) < Globals.PlayerPickUpRange);
						}
				}

				public bool IsTerrainUnderGrabber {
						get {
								return (TerrainUnderGrabber != null && Vector3.Distance(player.Position, TerrainUnderGrabberHitInfo.point) < Globals.PlayerPickUpRange);
						}
				}

				public bool IsWorldItemUnderGrabber {
						get {
								return (WorldItemUnderGrabber != null && Vector3.Distance(player.Position, WorldItemUnderGrabberHitInfo.point) < Globals.PlayerPickUpRange);
						}
				}

				public bool IsReceptacleInPlayerFocus {
						get {
								return ReceptacleInPlayerFocus != null;
						}
				}

				public bool IsReceptacleUnderGrabber {
						get {
								return ReceptacleUnderGrabber != null;
						}
				}

				public bool IsInWater {
						get {
								return State.IsInWater;
						}
				}

				public bool IsOnMovingPlatform {
						get {
								return MovingPlatformUnderPlayer != null;
						}
				}

				protected void BuildDescription()
				{
						StringBuilder sb = new StringBuilder();
						switch (player.Status.LatestTemperatureExposure) {
								case TemperatureRange.A_DeadlyCold:
										sb.AppendLine("Your surroundings are deadly cold");
										break;

								case TemperatureRange.B_Cold:
										sb.AppendLine("Your surroundings are uncomfortably cold");
										break;

								case TemperatureRange.C_Warm:
										sb.AppendLine("Your surroundings are comfortably warm");
										break;

								case TemperatureRange.D_Hot:
										sb.AppendLine("Your surroundings are uncomfortably hot");
										break;

								case TemperatureRange.E_DeadlyHot:
										sb.AppendLine("Your surroundings are deadly hot");
										break;
						}


						if (IsUnderground) {
								sb.AppendLine("You are underground");
						} else if (IsInSafeLocation) {
								sb.AppendLine("You are in a safe location");
						} else if (IsInsideStructure) {
								sb.AppendLine("You are inside a structure");
						}
						if (IsInCivilization) {
								sb.AppendLine("You are in a location with ties to civilization");
						} else {
								sb.AppendLine("You are in the wild");
						}

						mCurrentDescription = sb.ToString();
						sb.Clear();
						sb = null;
				}

				public Color TerrainType = Color.black;
				public float YDistanceFromCoast = 10.0f;
				public Vector3 LastPositionOnland;
				public IItemOfInterest ClosestObjectBelow;
				public IItemOfInterest ClosestObjectAbove;
				public IItemOfInterest ClosestObjectForward;
				public IItemOfInterest ClosestObjectFocus;
				public IItemOfInterest ClosestObjectInRange;
				public RaycastHit ClosestObjectBelowHitInfo;
				public RaycastHit ClosestObjectAboveHitInfo;
				public RaycastHit ClosestObjectForwardHitInfo;
				public RaycastHit ClosestObjectFocusHitInfo;
				public RaycastHit ClosestObjectInRangeHitInfo;
				public IItemOfInterest WorldItemFocus;
				public IItemOfInterest TerrainFocus;
				public RaycastHit WorldItemFocusHitInfo;
				public RaycastHit TerrainFocusHitInfo;
				public IItemOfInterest WorldItemBelow;
				public IItemOfInterest TerrainBelow;
				public RaycastHit WorldItemBelowHitInfo;
				public RaycastHit TerrainBelowHitInfo;
				public IItemOfInterest WorldItemForward;
				public IItemOfInterest TerrainForward;
				public RaycastHit WorldItemForwardHitInfo;
				public RaycastHit TerrainForwardHitInfo;
				public IItemOfInterest WorldItemAbove;
				public IItemOfInterest TerrainAbove;
				public RaycastHit WorldItemAboveHitInfo;
				public RaycastHit TerrainAboveHitInfo;
				public IItemOfInterest TerrainUnderGrabber;
				public IItemOfInterest WorldItemUnderGrabber;
				public RaycastHit TerrainUnderGrabberHitInfo;
				public RaycastHit WorldItemUnderGrabberHitInfo;
				public Receptacle ReceptacleInPlayerFocus;
				public Receptacle ReceptacleUnderGrabber;
				public MovingPlatform MovingPlatformUnderPlayer;

				#endregion

				#region structures and locations

				public void CheckCivilization()
				{
						bool isInCivilization = IsInCivilization;
						if (!isInCivilization && mInCivilizationLastFrame) {
								OnCivilizationExit();
						} else if (isInCivilization && !mInCivilizationLastFrame) {
								OnCivilizationEnter();
						}
						mInCivilizationLastFrame = isInCivilization;
				}

				public void AddCivilizationBoost(float effectTime)
				{
						mCivilizationBoostEnd = WorldClock.AdjustedRealTime + effectTime;
				}

				public Structure LastStructureEntered {
						get {
								return mLastStructureEntered;
						}
						set {
								mLastStructureEntered = value;
						}
				}

				protected double mCivilizationBoostEnd;
				protected Structure mLastStructureEntered = null;
				public Structure LastStructureExited = null;
				public Location LastLocationRevealed = null;
				public List <PilgrimStop> PilgrimStops = new List <PilgrimStop>();
				public List <Location> VisitingLocations = new List <Location>();

				public PilgrimStop CurrentPilgrimStop { get { return PilgrimStops[0]; } }

				public Location CurrentLocation { get { return VisitingLocations[0]; } }

				public Structure CurrentStructure { get { return LastStructureEntered; } }

				public bool IsVisitingLocation { get { return VisitingLocations.Count > 0; } }

				public bool IsVisitingPilgrimStop { get { return PilgrimStops.Count > 0; } }

				public bool IsInsideStructure {
						get {
								//this is meant to be 'self reparing'
								//every time we ask it makes sure the answer is still valid
								if (player.HasSpawned) {
										if (LastStructureEntered != null) {
												if (LastStructureEntered.IsDestroyed) {
														LastStructureEntered = null;
												}
												if (LastStructureEntered != null) {
														State.IsInsideStructure = true;
														State.LastStructureEntered = LastStructureEntered.worlditem.StaticReference;
												} else {
														State.IsInsideStructure = false;
												}
										} else {
												State.IsInsideStructure = false;
										}
								}
								return State.IsInsideStructure;
						}
				}

				public bool IsVisitingStructure(Structure structure)
				{
						if (IsInsideStructure) {
								return structure == LastStructureEntered;
						}
						return false;
				}

				public void PassThroughEntrance(Dynamic entrance, bool enter)
				{
						if (entrance.ParentStructure.StructureShingle.PropertyIsDestroyed) {
								return;
						}

						if (entrance.State.Type == WorldStructureObjectType.OuterEntrance) {
								if (IsVisitingStructure(entrance.ParentStructure)) {
										StructureExit(entrance.ParentStructure);
								} else {
										StructureEnter(entrance.ParentStructure);
								}
						}
				}

				public bool IsOutside { get { return State.IsOutside; } }

				public bool IsInCivilization {
						get {//TODO link this to following a path
								//either we're in a location that's civilized
								//or else we're standing on terrain type civilized
								if (!mInitialized) {
										return true;
								}
								if (WorldClock.AdjustedRealTime < mCivilizationBoostEnd) {
										//we've been given a boost, probably by a spyglass
										return true;
								}

								return (IsVisitingLocation && CurrentLocation.State.IsCivilized)
								|| TerrainType.b > 0f
								|| (Paths.HasActivePath && Paths.ActivePath.IsAttachedToCivilization);
						}
				}

				public bool IsUnderground {
						get {
								return State.IsUnderground;
						}
				}

				public bool IsInSafeLocation {
						get {
								if (IsInsideStructure) {
										return LastStructureEntered.State.IsSafeLocation;
								}
								return false;
						}
				}

				public bool IsInCreatureDen {
						get {
								return CreatureDens.Count > 0;
						}
				}

				protected bool mInCivilizationLastFrame = false;

				#endregion

				public IEnumerator CheckSurroundings()
				{
						mCheckingSurroundings = true;
						while (mCheckingSurroundings) {
								while (!GameManager.Is(FGameState.InGame)
								       || !player.HasSpawned || player.IsHijacked) {	//wait it out
										yield return null;
								}
								//put a tick between each of these
								//TODO may have to move RaycastFocus to separate coroutine
								ClearSurroundings();
								try {
										RaycastAllFocus();
								} catch (Exception e) {
										Debug.LogException(e);
								}
								yield return null;
								try {
										RaycastAllUp();
								} catch (Exception e) {
										Debug.LogException(e);
								}
								yield return null;
								CheckDirectSunlight();
								yield return null;
								CheckTerrainType();
								yield return null;
								CheckDanger();
								yield return null;
								CheckAnimalDens();
								yield return null;
								//TODO this is kind of a kludge... put this somewhere else
								YDistanceFromCoast = player.Position.y - Biomes.Get.TideWaterElevation;
								yield return null;
								CheckExposure();

								State.LastChunkID = GameWorld.Get.PrimaryChunkID;
								State.LastPosition.CopyFrom(player.tr);
								yield return null;
						}
						mCheckingSurroundings = false;
						yield break;
				}

				protected bool mCheckingSurroundings = false;

				#region audio

				public bool IsSoundAudible(Vector3 origin, float audibleRadius)
				{	//leaving room for audibility skills here
						return (Vector3.Distance(player.Position, origin) <= audibleRadius * 2.0f);//TEMP
				}

				#endregion

				#region enter / exit

				public void EnterUnderground()
				{
						State.IsUnderground = true;
						State.EnterUndergroundTime = WorldClock.Time;
						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationUndergroundEnter), WorldClock.Time);
						GUIManager.PostInfo("You are under ground.");
						//player.SaveState ();
				}

				public void ExitUnderground()
				{
						State.IsUnderground = false;
						State.ExitUndergroundTime = WorldClock.Time;
						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationUndergroundExit), WorldClock.Time);
						GUIManager.PostInfo("You are above ground.");
						//player.SaveState ();
				}

				public bool MoveEnterWater(double timeStamp)
				{
						player.Status.AddCondition("Wet");//do this here so it's immediate
						State.IsInWater = true;
						return true;
				}

				public bool MoveExitWater(double timeStamp)
				{
						State.IsInWater = false;
						return true;
				}

				public void StructureEnter(Structure structure)
				{
						if (structure.IsDestroyed) {
								return;
						}
						LastStructureEntered = structure;
						State.LastStructureEntered = structure.worlditem.StaticReference;
						State.EnterStructureTime = WorldClock.Time;
						structure.worlditem.Get <Revealable>().State.UnknownUntilVisited = false;
						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationStructureEnter), WorldClock.Time);

						if (structure.State.IsRespawnStructure) {
								State.VisitedRespawnStructures.SafeAdd(structure.worlditem.StaticReference);
						}
						//player.SaveState ();
				}

				public void StructureExit(Structure structure)
				{
						LastStructureEntered = null;
						State.LastStructureExited = structure.worlditem.StaticReference;
						State.ExitStructureTime = WorldClock.Time;
						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationStructureExit), WorldClock.Time);
						//player.SaveState ();
				}

				public bool IsVisiting(MobileReference location)
				{
						return State.VisitingLocations.Contains(location);
				}

				public bool IsVisiting(Location location)
				{
						return State.VisitingLocations.Contains(location.worlditem.StaticReference);
				}

				public void PilgrimStopVisit(PilgrimStop pilgrimStop)
				{
						CleanPilgrimStops();
						if (!PilgrimStops.Contains(pilgrimStop)) {
								PilgrimStops.Add(pilgrimStop);
						}
				}

				public void CleanPilgrimStops()
				{
						//clean existing
						for (int i = PilgrimStops.Count - 1; i >= 0; i--) {
								Location location = null;
								if (PilgrimStops[i] != null && PilgrimStops[i].worlditem.Is <Location>(out location)) {
										//if (!WorldItems.IsInActiveRange (location.worlditem, player.Position, 1.0f)) {
										//	PilgrimStops.RemoveAt (i);
										//}
								}
						}
				}

				public void LeaveAllLocations()
				{
						for (int i = 0; i < VisitingLocations.Count; i++) {
								Visitable visitable = null;
								if (VisitingLocations[i] != null && VisitingLocations[i].worlditem.Is<Visitable>(out visitable)) {
										Leave(visitable);
								}
						}
						State.VisitingLocations.Clear();
						VisitingLocations.Clear();
				}

				public bool Reveal(MobileReference mr)
				{
						if (Profile.Get.CurrentGame.RevealedLocations.SafeAdd(mr)) {
								Profile.Get.CurrentGame.NewLocations.SafeAdd(mr);
								Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationReveal), WorldClock.Time);
								return true;
						} else {
								return false;
						}
				}

				public void Reveal(Revealable revealable)
				{
						Reveal(revealable.worlditem.StaticReference);
				}

				public void Visit(Visitable visitable)
				{
						Location location = null;
						if (visitable.worlditem.Is <Location>(out location)) {
								if (VisitingLocations.SafeAdd(location)) {
										VisitingLocations.Sort();
										State.VisitingLocations.Add(location.worlditem.StaticReference);
										State.LastLocationVisited = location.worlditem.StaticReference;
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationVisit), WorldClock.Time);
								}
								//player.SaveState ();
						}
				}

				public void Leave(Visitable visitable)
				{
						Location location = null;
						if (visitable.worlditem.Is<Location>(out location)) {
								State.VisitingLocations.Remove(location.worlditem.StaticReference);
								State.LastLocationExited = location.worlditem.StaticReference;
								VisitingLocations.Sort();
								VisitingLocations.Remove(location);
								Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.LocationLeave), WorldClock.Time);
								//player.SaveState ();
						}
				}

				public void OnCivilizationExit()
				{
						//these notifcations are no longer necessary thanks to our active state check
						//GUIManager.PostWarning ("You have entered the wilderness.");
						//player.SaveState ();
				}

				public void OnCivilizationEnter()
				{
						//these notifcations are no longer necessary thanks to our active state check
						//GUIManager.PostInfo ("You have returned to civilization.");
						//player.SaveState ();
				}

				public void OnDangerEnter()
				{
						//these notifcations are no longer necessary thanks to our active state check
						//player.SaveState ();
				}

				public void OnDangerExit()
				{
						//these notifcations are no longer necessary thanks to our active state check
						//player.SaveState ();
				}

				public void AddMapMarker(Vector3 mapMarkerLocation)
				{
						WorldChunk chunk = null;
						if (!GameWorld.Get.ChunkAtPosition(mapMarkerLocation, out chunk)) {
								Debug.Log("Couldn't find chunk at " + mapMarkerLocation.ToString());
						}
						//TEMP for now, only one at a time
						if (State.ActiveMapMarkers.Count > 0) {
								//move the existing marker
								State.ActiveMapMarkers[0].ChunkPosition = WorldChunk.WorldPositionToChunkPosition(chunk.ChunkBounds, mapMarkerLocation - chunk.ChunkOffset);
								State.ActiveMapMarkers[0].ChunkID = chunk.State.ID;
								State.ActiveMapMarkers[0].ChunkOffset = chunk.ChunkOffset;
						} else {
								MapMarker mm = new MapMarker();
								mm.ChunkPosition = WorldChunk.WorldPositionToChunkPosition(chunk.ChunkBounds, mapMarkerLocation - chunk.ChunkOffset);
								;
								mm.ChunkID = chunk.State.ID;
								mm.ChunkOffset = chunk.ChunkOffset;
								State.ActiveMapMarkers.Add(mm);
						}
						FXManager.Get.SpawnMapMarkers(State.ActiveMapMarkers);
				}

				#endregion

				public void Update()
				{
						//we have to do this every frame to identify moving platforms etc
						RaycastAllDown();
				}

				#region hostiles and danger

				protected int mCheckHostiles = 0;
				public List <IHostile> Hostiles = new List <IHostile>();
				public int HostilesTargetingPlayer = 0;
				public List <CreatureDen> CreatureDens = new List<CreatureDen>();

				public void FixedUpdate()
				{
						if (!GameManager.Is(FGameState.InGame)) {
								return;
						}

						LightManager.RefreshExposure(this);

						mCheckHostiles++;
						if (mCheckHostiles > 5) {//TODO maybe put this in a coroutine
								mCheckHostiles = 0;
								HostilesTargetingPlayer = 0;
								for (int i = Hostiles.LastIndex(); i >= 0; i--) {
										IHostile hostile = Hostiles[i];
										if (hostile == null || !hostile.HasPrimaryTarget || hostile.Mode == HostileMode.CoolingOff) {
												Hostiles.RemoveAt(i);
										} else if (hostile.PrimaryTarget == player) {
												//Debug.Log ("Hostile primary target is player, we're still in danger");
												HostilesTargetingPlayer++;
										}
								}
						}
				}

				public bool HasHostiles {
						get {
								return HostilesTargetingPlayer > 0;
						}
				}

				public bool IsInDanger {
						get {
								return HasHostiles || IsVisitingLocation && CurrentLocation.State.IsDangerous;
						}
				}

				public void CheckDanger()
				{
						//check existing hostiles for duds
						bool isInDanger = IsInDanger;
						if (mInDangerLastFrame) {
								if (!isInDanger) {
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalDangerExit, WorldClock.Time);
								}
						} else if (isInDanger) {
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalDangerEnter, WorldClock.Time);
						}
						mInDangerLastFrame = isInDanger;
				}

				public void CheckAnimalDens()
				{
						for (int i = CreatureDens.Count - 1; i >= 0; i--) {
								if (CreatureDens[i] == null) {
										CreatureDens.RemoveAt(i);
								}
						}

						bool isInCreatureDen = IsInCreatureDen;
						if (mInCreatureDenLastFrame) {
								if (!isInCreatureDen) {
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalCreatureDenExit, WorldClock.Time);
								}
						} else if (isInCreatureDen) {
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalCreatureDenEnter, WorldClock.Time);
						}
						mInCreatureDenLastFrame = isInCreatureDen;
				}

				public void RemoveHostile(Hostile hostile)
				{
						if (Hostiles.Remove(hostile)) {
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalHostileDeaggro, WorldClock.Time);
						}
				}

				public void AddHostile(IHostile hostile)
				{
						if (Hostiles.SafeAdd(hostile)) {
								if (hostile.PrimaryTarget == player) {
										HostilesTargetingPlayer++;
								}
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalHostileAggro, WorldClock.Time);
						}
				}

				public void CreatureDenEnter(CreatureDen creatureDen)
				{
						if (!CreatureDens.Contains(creatureDen)) {
								CreatureDens.Add(creatureDen);
						}
				}

				public void CreatureDenExit(CreatureDen creatureDen)
				{
						CreatureDens.Remove(creatureDen);
				}

				protected bool mInDangerLastFrame = false;
				protected bool mInCreatureDenLastFrame = false;

				#endregion

				#region weather

				//these functions aren't used any more
				//i'm keeping them around in case i want to bring them back
				public void CheckDirectSunlight()
				{
						if (WorldClock.Is(TimeOfDay.ba_LightSunLight)) {
								RaycastHit sunlightHit;
								if (Physics.Raycast(player.HeadPosition, Biomes.Get.SunLightPosition, out sunlightHit, Vector3.Distance(player.HeadPosition, Biomes.Get.SunLightPosition), Globals.LayersActive)) {
										State.InDirectSunlight = false;
								} else {
										State.InDirectSunlight = true;
								}
						} else {
								State.InDirectSunlight = false;
						}
				}

				public void CheckExposure()
				{
						if (State.ExposedToRain) {
								if (!mExposedToRainLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsExposeToRain), WorldClock.Time);
								}
						} else {
								if (mExposedToRainLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsShieldFromRain), WorldClock.Time);
								}
						}
						mExposedToRainLastFrame = State.ExposedToRain;

						if (State.ExposedToSun) {
								if (!mExposedToSunLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsExposeToSun), WorldClock.Time);
								}
						} else {
								if (mExposedToSunLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsShieldFromSun), WorldClock.Time);
								}
						}
						mExposedToSunLastFrame = State.ExposedToRain;

						if (State.ExposedToSky) {
								if (!mExposedToSkyLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsExposeToSky), WorldClock.Time);
								}
						} else {
								if (mExposedToSkyLastFrame) {
										Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.SurroundingsShieldFromSky), WorldClock.Time);
								}
						}
						mExposedToSkyLastFrame = State.ExposedToSky;
				}

				protected bool mExposedToSunLastFrame = false;
				protected bool mExposedToRainLastFrame = false;
				protected bool mExposedToSkyLastFrame = false;

				#endregion

				#region raycasts

				public void ClearSurroundings()
				{
						ClosestObjectBelow = null;
						ClosestObjectAbove = null;
						ClosestObjectForward = null;
						ClosestObjectFocus = null;
						ClosestObjectInRange = null;

						WorldItemFocus = null;
						TerrainFocus = null;

						WorldItemBelow = null;
						TerrainBelow = null;

						WorldItemForward = null;
						TerrainForward = null;

						WorldItemAbove = null;
						TerrainAbove = null;

						WorldItemUnderGrabber = null;
						TerrainUnderGrabber = null;

						ReceptacleInPlayerFocus	= null;
						ReceptacleUnderGrabber = null;
				}

				public void RaycastAllDown()
				{
						//if we're on a moving platform take the platform's velocity into account when doing raycasts
						//we don't want to lose the platform
						downRaycastStart = player.Position;
						if (IsOnMovingPlatform) {
								downRaycastStart.y += MovingPlatformUnderPlayer.VelocityLastFrame.y * 1.5f;
						}

						if (Physics.Raycast(downRaycastStart, player.DownVector, out downHit, Globals.RaycastAllDownDistance, Globals.LayersActive)) {
								//check for moving platforms directly
								if (downHit.collider.attachedRigidbody != null) {
										MovingPlatformUnderPlayer = downHit.collider.attachedRigidbody.GetComponent <MovingPlatform>();
								}
								downItemOfInterest = null;
								if (WorldItems.GetIOIFromCollider(downHit.collider, out downItemOfInterest)) {
										switch (downItemOfInterest.IOIType) {
												case ItemOfInterestType.WorldItem:
														WorldItemBelowHitInfo = downHit;
														WorldItemBelow = downItemOfInterest;
														CheckForClosest(ref ClosestObjectBelow, ref ClosestObjectBelowHitInfo, WorldItemBelow, downHit, false);
														break;

												case ItemOfInterestType.Scenery:
														TerrainBelowHitInfo = downHit;
														TerrainBelow = downItemOfInterest;
														CheckForClosest(ref ClosestObjectBelow, ref ClosestObjectBelowHitInfo, TerrainBelow, downHit, false);
														break;

												default:
														break;
										}
								}

								CheckGroundType++;
								if (CheckGroundType > 4) {
										CheckGroundType = 0;
										if (State.IsInWater) {
												State.GroundBeneathPlayer = GroundType.Water;
										} else {
												LastPositionOnland = player.Position;
												State.GroundBeneathPlayer = GroundType.Dirt;
												if (IsSomethingBelowPlayer) {
														switch (ClosestObjectBelow.gameObject.tag) {
																case Globals.TagGroundDirt:
																		State.GroundBeneathPlayer = GroundType.Dirt;
																		break;

																case Globals.TagGroundLeaves:
																		State.GroundBeneathPlayer = GroundType.Leaves;
																		break;

																case Globals.TagGroundMetal:
																		State.GroundBeneathPlayer = GroundType.Metal;
																		break;

																case Globals.TagGroundMud:
																		State.GroundBeneathPlayer = GroundType.Mud;
																		break;

																case Globals.TagGroundSnow:
																		State.GroundBeneathPlayer = GroundType.Snow;
																		break;

																case Globals.TagGroundStone:
																		State.GroundBeneathPlayer = GroundType.Stone;
																		break;

																case Globals.TagGroundWater:
																		State.GroundBeneathPlayer = GroundType.Water;
																		break;

																case Globals.TagGroundWood:
																		State.GroundBeneathPlayer = GroundType.Wood;
																		break;

																case Globals.TagGroundTerrain:
																		State.GroundBeneathPlayer = GameWorld.Get.GroundTypeAtInGamePosition(player.Position, IsUnderground);
																		break;

																default:
								//Debug.Log ("Tag is " + ClosestObjectBelow.gameObject.tag);
																		State.GroundBeneathPlayer = GroundType.Dirt;
																		break;
														}
												}
										}
								}
						} else {
								MovingPlatformUnderPlayer = null;
						}
				}

				public void RaycastAllFocus()
				{
						focusItemOfInterest = null;
						//check for terrain in front of us - used mostly for placement of stuff
						if (Physics.Raycast(player.HeadPosition, player.FocusVector, out terrainHit, Globals.RaycastAllFocusDistance, Globals.LayersTerrain)) {
								//check for structure terrain layer first - it will be the parent of the collider
								bool foundTerrainLayer = WorldItems.GetIOIFromCollider(terrainHit.collider, out focusItemOfInterest);
								if (!foundTerrainLayer && terrainHit.collider.transform.parent != null) {
										//check for structure terrain layer - it will be the parent of the collider
										focusItemOfInterest = (IItemOfInterest)terrainHit.collider.transform.parent.GetComponent(typeof(IItemOfInterest));
										foundTerrainLayer = focusItemOfInterest != null;
								}
								if (foundTerrainLayer) {
										TerrainFocus = focusItemOfInterest;
										TerrainFocusHitInfo = terrainHit;
										CheckForClosest(ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, focusItemOfInterest, terrainHit, true);
								}
						}

						//we can't do spherecasts for triggers, unfortunately
						//and we need to check for water triggers
						//so do that here, then do the rest as spherecast
						if (Physics.Raycast(player.HeadPosition, player.FocusVector, out worldItemHit, Globals.RaycastAllFocusDistance, Globals.LayerWorldItemActive)) {
								//if (!worldItemHit.collider.isTrigger) {
								if (WorldItems.GetIOIFromCollider(worldItemHit.collider, out focusItemOfInterest)) {
										focusItemOfInterest = CheckForCarried(focusItemOfInterest);
										focusItemOfInterest = CheckForEquipped(focusItemOfInterest);
										CheckForClosest(ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, focusItemOfInterest, worldItemHit, true);
										CheckForClosest(ref WorldItemFocus, ref WorldItemFocusHitInfo, focusItemOfInterest, worldItemHit, true);
								}
								//}
						}
						//this result will override any fluid hit results
						//this is desired behavior because we want to be able to pick up objects through triggers
						sphereCastHits = Physics.SphereCastAll(player.HeadPosition, 0.1f, player.FocusVector, Globals.RaycastAllFocusDistance, Globals.LayerWorldItemActive);
						if (sphereCastHits.Length > 0) {
								for (int i = 0; i < sphereCastHits.Length; i++) {
										worldItemHit = sphereCastHits[i];
										if (WorldItems.GetIOIFromCollider(worldItemHit.collider, out focusItemOfInterest)) {
												//make sure we're not carrying or equipping this item
												//(we no longer have to check for body parts, get ioi from collider does that for us)
												focusItemOfInterest = CheckForCarried(focusItemOfInterest);
												focusItemOfInterest = CheckForEquipped(focusItemOfInterest);
												CheckForClosest(ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, focusItemOfInterest, worldItemHit, true);
												CheckForClosest(ref WorldItemFocus, ref WorldItemFocusHitInfo, focusItemOfInterest, worldItemHit, true);
										}
								}
								Array.Clear(sphereCastHits, 0, sphereCastHits.Length);
						}
						if (IsWorldItemInPlayerFocus) {
								ReceptacleInPlayerFocus = WorldItemFocus.worlditem.Get <Receptacle>();
						}
				}

				public void RaycastAllForward()
				{
						hitsForward = Physics.RaycastAll(player.HeadPosition, player.ForwardVector, Globals.RaycastAllForwardDistance, Globals.LayersActive);

						if (hitsForward.Length > 0) {
								for (int i = 0; i < hitsForward.Length; i++) {
										hitForward = hitsForward[i];
										IItemOfInterest itemOfInterest = null;
										if (WorldItems.GetIOIFromCollider(hitForward.collider, out itemOfInterest)) {
												switch (itemOfInterest.IOIType) {
														case ItemOfInterestType.WorldItem:
																WorldItemForward = itemOfInterest.worlditem;
																WorldItemForwardHitInfo = hitForward;
																CheckForClosest(ref ClosestObjectForward, ref ClosestObjectForwardHitInfo, WorldItemForward, hitForward, false);
																break;

														case ItemOfInterestType.Scenery:
																TerrainForwardHitInfo = hitForward;
																TerrainForward = itemOfInterest;
																CheckForClosest(ref ClosestObjectForward, ref ClosestObjectForwardHitInfo, TerrainForward, hitForward, false);
																break;

														default:
																break;
												}
										}
								}
								Array.Clear(hitsForward, 0, hitsForward.Length);
						}
				}

				public void RaycastAllUp()
				{
						if (Physics.Raycast(player.Position, player.UpVector, out hitUp, Globals.RaycastAllUpDistance, Globals.LayersActive)) {
								upItemOfInterest = null;
								if (WorldItems.GetIOIFromCollider(hitUp.collider, out upItemOfInterest)) {
										switch (upItemOfInterest.IOIType) {
												case ItemOfInterestType.WorldItem:
														WorldItemAbove = upItemOfInterest;
														WorldItemAboveHitInfo = hitUp;
														CheckForClosest(ref ClosestObjectAbove, ref ClosestObjectAboveHitInfo, WorldItemAbove, hitUp, false);
														break;

												case ItemOfInterestType.Scenery:
														TerrainAboveHitInfo = hitUp;
														TerrainAbove = upItemOfInterest;
														CheckForClosest(ref ClosestObjectAbove, ref ClosestObjectAboveHitInfo, TerrainAbove, hitUp, false);
														break;

												default:
														break;
										}
								}
						}
				}

				protected void CheckForClosest(ref IItemOfInterest currentClosest, ref RaycastHit currentHit, IItemOfInterest contender, RaycastHit hit, bool checkForRange)
				{
						if (contender == null)
								return;

						float currentDistance = Vector3.Distance(player.HeadPosition, currentHit.point);
						float contenderDistance = Vector3.Distance(player.HeadPosition, hit.point);

						if (currentClosest == null) {
								currentClosest = contender;
								currentHit = hit;
								currentDistance = contenderDistance;
						} else if (currentDistance > contenderDistance) {
								currentClosest = contender;
								currentHit = hit;
								currentDistance = contenderDistance;
						}

						//this gets weird in the case of bodies of water
						//TODO return 'closest' as objects under water when appropriate

						if (checkForRange && currentDistance < Globals.PlayerPickUpRange) {
								if (ClosestObjectInRange == null) {
										ClosestObjectInRange = currentClosest;
										ClosestObjectInRangeHitInfo = currentHit;
								} else if (Vector3.Distance(ClosestObjectInRange.Position, player.HeadPosition) < currentDistance) {
										ClosestObjectInRange = currentClosest;
										ClosestObjectInRangeHitInfo = currentHit;
								}
						}
				}

				protected IItemOfInterest finalHitObject;

				protected IItemOfInterest CheckForBodyParts(IItemOfInterest hitObject)
				{
						finalHitObject = hitObject;
						switch (hitObject.gameObject.tag) {
								case "BodyArm":
								case "BodyLeg":
								case "BodyHead":
								case "BodyTorso":
								case "BodyGeneral":
										BodyPart bodyPart = hitObject.gameObject.GetComponent <BodyPart>();
										finalHitObject = bodyPart.Owner;
										break;

								default:
										break;
						}
						return finalHitObject;
				}

				protected IItemOfInterest CheckForEquipped(IItemOfInterest hitObject)
				{
						if (Player.Local.Tool.HasWorldItem && Player.Local.Tool.worlditem == hitObject) {
								//Debug.Log ("This item is equipped");
								hitObject = null;
						}
						return hitObject;
				}

				protected IItemOfInterest CheckForCarried(IItemOfInterest hitObject)
				{
						if (player.ItemPlacement.IsCarryingSomething && player.ItemPlacement.CarryObject == hitObject.worlditem) {
								//Debug.Log ("We're carring this item");
								hitObject = null;
						}
						return hitObject;
				}

				protected void CheckTerrainType()
				{
						mTerrainTypeCheck++;
						//only do this once in a while TODO maybe a coroutine would be better
						if (mTerrainTypeCheck > 10) {
								mTerrainTypeCheck = 0;
								TerrainType = GameWorld.Get.TerrainTypeAtInGamePosition(player.Position, State.IsUnderground);
								//this reduces the 'coastal' terrain type based on distance from water
								//move these to global variables
								//float maxDistanceFromCoast = 25.0f;
								//float minMultiplier = 0.25f;
								//float coastalMultiplier = Mathf.Clamp ((1.0f - (YDistanceFromCoast / maxDistanceFromCoast)), minMultiplier, 1.0f);
								//TerrainType.r = TerrainType.r * coastalMultiplier;
						}
				}
				//store all of these locally to avoid allocations
				protected RaycastHit downHit;
				protected IItemOfInterest downItemOfInterest;
				protected Vector3 downRaycastStart;
				protected RaycastHit worldItemHit;
				protected RaycastHit terrainHit;
				protected RaycastHit[] sphereCastHits;
				protected IItemOfInterest focusItemOfInterest = null;
				protected RaycastHit hitUp;
				protected IItemOfInterest upItemOfInterest = null;
				protected RaycastHit[ ] hitsForward;
				protected RaycastHit hitForward;
				protected int CheckGroundType = 0;
				protected double mTerrainTypeCheckInterval = 0.5f;
				protected int mTerrainTypeCheck = 0;
				protected float mForwardVectorRange = Globals.ForwardVectorRange;
				protected float mDownVectorRange = Globals.DownVectorRange;
				protected float mRangeMultiplier = Globals.VectorRangeMultiplier;

				#endregion

				#region IPhotosensitive implementation

				public WorldLight NearestLight {
						get {
								WorldLight nearestLight = null;
								float nearestDistance = Mathf.Infinity;
								for (int i = LightSources.LastIndex(); i >= 0; i--) {
										if (LightSources[i] == null) {
												LightSources.RemoveAt(i);
										} else {
												float distance = Vector3.Distance(transform.position, LightSources[i].transform.position);
												if (distance < nearestDistance) {
														nearestDistance = distance;
														nearestLight = LightSources[i];
												}
										}
								}
								return nearestLight;
						}
				}

				public Fire NearestFire {
						get {
								Fire fire = null;
								float nearestDistance = Mathf.Infinity;
								for (int i = FireSources.LastIndex(); i >= 0; i--) {
										if (FireSources[i] == null) {
												FireSources.RemoveAt(i);
										} else {
												float distance = Vector3.Distance(transform.position, FireSources[i].transform.position);
												if (distance < nearestDistance) {
														nearestDistance = distance;
														fire = FireSources[i];
												}
										}
								}
								return fire;
						}
				}

				public bool HasNearbyLights {
						get {
								return LightSources.Count > 0;
						}
				}

				public bool HasNearbyFires {
						get {
								return FireSources.Count > 0;
						}
				}

				public float Radius { get; set; }

				public Vector3 Position { get { return transform.position; } }

				public float LightExposure { get; set; }

				public float HeatExposure { get; set; }

				public List <WorldLight> LightSources {
						get {
								if (mLightSources == null) {
										mLightSources = new List <WorldLight>();
								}
								return mLightSources;
						}
						set {
								mLightSources = value;
						}
				}

				protected List <WorldLight> mLightSources = null;

				public List <Fire> FireSources {
						get {
								if (mFireSources == null) {
										mFireSources = new List <Fire>();
								}
								return mFireSources;
						}
						set {
								mFireSources = value;
						}
				}

				protected List <Fire> mFireSources = null;

				public Action OnExposureIncrease { get; set; }

				public Action OnExposureDecrease { get; set; }

				public Action OnHeatIncrease { get; set; }

				public Action OnHeatDecrease { get; set; }

				public float DistanceToNearestFire = Mathf.Infinity;

				#endregion

		}

		[Serializable]
		public class PlayerSurroundingsState
		{
				public TerrainType TerrainAroundPlayer = TerrainType.Civilization;
				public int LastChunkID = 0;
				public STransform LastPosition = new STransform();
				public GroundType GroundBeneathPlayer = GroundType.Dirt;
				public MobileReference LastStructureEntered = new MobileReference();
				public MobileReference LastStructureExited = new MobileReference();
				public MobileReference LastCityEntered = new MobileReference();
				public MobileReference LastCityExited = new MobileReference();
				public MobileReference LastLocationVisited = MobileReference.Empty;
				public MobileReference LastLocationExited = MobileReference.Empty;
				public MobileReference LastLocationRevealed = MobileReference.Empty;
				public List <MobileReference> Hostiles = new List <MobileReference>();
				public List <MobileReference> VisitedRespawnStructures = new List <MobileReference>();
				public List <MobileReference> VisitingLocations = new List <MobileReference>();
				public List <MapMarker> ActiveMapMarkers = new List <MapMarker>();

				public bool ExposedToSun {
						get {
								return InDirectSunlight;// && Biomes.CurrentWeather.IsSunny;
						}
				}

				public bool ExposedToSky = false;
				public bool ExposedToRain = false;
				public bool IsInWater = false;
				public bool IsInsideStructure = false;
				public bool IsInCity = false;

				public bool IsVisitingLocation {
						get {
								return VisitingLocations.Count > 0;
						}
				}

				public bool IsUnderground = false;

				public bool IsOutside {
						get {
								return (!IsInsideStructure && !IsUnderground);
						}
				}

				public double TimeSinceEnteredUnderground {
						get {
								return WorldClock.Time - EnterUndergroundTime;
						}
				}

				public double TimeSinceExitedUnderground {
						get {
								return WorldClock.Time - ExitUndergroundTime;
						}
				}

				public bool InDirectSunlight = false;
				public double EnterStructureTime = 0.0f;
				public double ExitStructureTime = 0.0f;
				public double EnterUndergroundTime = 0.0f;
				public double ExitUndergroundTime = 0.0f;
		}

		[Serializable]
		public class MapMarker
		{
				//TODO fold this into the worlditem location class
				public int ChunkID;
				public SVector3 ChunkPosition;
				public SVector3 ChunkOffset;
		}

		public enum TerrainType
		{
				Coastal,
				Civilization,
				OpenField,
				LightForest,
				DeepForest,
		}

		public enum GroundType
		{
				Dirt,
				Leaves,
				Metal,
				Mud,
				Snow,
				Stone,
				Water,
				Wood,
		}
}
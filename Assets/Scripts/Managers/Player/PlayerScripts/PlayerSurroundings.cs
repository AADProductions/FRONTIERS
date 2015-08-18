using UnityEngine;
using System;
using Frontiers.Data;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System.Text;
using Frontiers.GUI;
using Frontiers.World.WIScripts;

namespace Frontiers
{
	public class PlayerSurroundings : PlayerScript, IPhotosensitive
	{
		public PlayerSurroundingsState State = new PlayerSurroundingsState ();
		//TODO remove this
		public List <string> VisitNotificationTypes = new List <string> { "City" };
		public Structure StartupStructure = null;

		#region initialization

		public override void OnGameStart ()
		{
			if (GameManager.Get.TestingEnvironment)
				return;

			Player.Get.AvatarActions.Subscribe (AvatarAction.MoveEnterWater, new ActionListener (MoveEnterWater));
			Player.Get.AvatarActions.Subscribe (AvatarAction.MoveExitWater, new ActionListener (MoveExitWater));

			for (int i = 0; i < GameWorld.Get.Settings.DefaultRevealedLocations.Count; i++) {
				Profile.Get.CurrentGame.RevealedLocations.SafeAdd (GameWorld.Get.Settings.DefaultRevealedLocations [i]);
			}

			for (int i = 0; i < GameWorld.Get.Regions.Count; i++) {
				State.VisitedRespawnStructures.SafeAdd (GameWorld.Get.Regions [i].DefaultRespawnStructure);
			}

			FXManager.Get.SpawnMapMarkers (State.ActiveMapMarkers);

			enabled = true;
		}

		public override void OnLocalPlayerDie ()
		{
			FireSources.Clear ();
			LightSources.Clear ();
			//cancel all hostile threats
			for (int i = 0; i < Hostiles.Count; i++) {
				if (Hostiles [i] != null && Hostiles [i].PrimaryTarget == player) {
					Hostiles [i].CoolOff ();
				}
			}
			Hostiles.Clear ();
			HostilesTargetingPlayer = 0;
		}

		public override void OnLocalPlayerSpawn ()
		{
			if (StartupStructure != null) {
				StructureEnter (StartupStructure);
				StartupStructure = null;
			}

			if (SpawnManager.Get.UseStartupPosition) {
				PlayerStartupPosition psp = SpawnManager.Get.CurrentStartupPosition;

				if (psp.ClearRevealedLocations) {
					Profile.Get.CurrentGame.RevealedLocations.Clear ();
				}

				State.IsInWater = false;
				State.GroundBeneathPlayer = GroundType.Dirt;
				State.LastChunkID = psp.ChunkID;
				State.LastPosition.CopyFrom (psp.WorldPosition);
				State.VisitingLocations.SafeAdd (psp.LocationReference);
				Profile.Get.CurrentGame.RevealedLocations.SafeAdd (psp.LocationReference);
				State.IsInsideStructure = psp.Interior;
				if (State.IsInsideStructure) {
					State.LastStructureEntered = psp.LocationReference.AppendLocation (psp.StructureName);
					Profile.Get.CurrentGame.RevealedLocations.SafeAdd (State.LastStructureEntered);
				}
				//add the latest respawn structures to our state
				for (int i = 0; i < SpawnManager.Get.CurrentStartupPosition.NewVisitedRespawnStructures.Count; i++) {
					State.VisitedRespawnStructures.SafeAdd (SpawnManager.Get.CurrentStartupPosition.NewVisitedRespawnStructures [i]);
				}
			}

			if (!mCheckingSurroundings) {
				mCheckingSurroundings = true;
				StartCoroutine (CheckSurroundings ());
			}

			mTerrainType = Colors.Alpha (Color.black, 0f);
		}

		public override void OnLocalPlayerDespawn ()
		{
			LeaveAllLocations ();
			State.IsInWater = false;
			State.GroundBeneathPlayer = GroundType.Dirt;
		}

		public override void AdjustPlayerMotor (ref float mMotorAccelerationMultiplier, ref float mMotorJumpForceMultiplier, ref float mMotorSlopeAngleMultiplier)
		{
			if (IsInWater) {
				mMotorAccelerationMultiplier *= Globals.DefaultWaterAccelerationPenalty;
				mMotorJumpForceMultiplier *= Globals.DefaultWaterJumpPenalty;
			}
		}

		public override void OnGameSaveStart ()
		{
			Debug.Log ("Save starting in player surroundings, last chunk ID was " + GameWorld.Get.PrimaryChunkID.ToString ());
			State.LastChunkID = GameWorld.Get.PrimaryChunkID;
			//if we haven't spawned all this stuff will be set by the spawn manager
			if (player.HasSpawned) {
				//but if we have spawned we need to
				//make sure our state is up-to-date
				State.LastPosition.CopyFrom (player.tr);
				//if we're not inside a structure
				//see if we're on top of one
				if (State.IsInsideStructure) {
					State.IsOnTopOfMeshTerrain = false;
				} else if (IsSomethingBelowPlayer) {
					if (ClosestObjectBelow.IOIType == ItemOfInterestType.Scenery) {
						Debug.Log ("Something below player: " + ClosestObjectBelow.gameObject.name);
						switch (ClosestObjectBelow.gameObject.tag) {
						case Globals.TagGroundTerrain:
																//we're standing on terrain
																//get our last chunk ID from this in case it's an arbitrarily-placed chunk
							Terrain t = ClosestObjectBelow.gameObject.GetComponent <Terrain> ();
							int chunkID = 0;
							if (GameWorld.Get.ChunkIDByTerrain (t, out chunkID)) {
								Debug.Log ("Got chunk ID " + chunkID.ToString () + " from terrain");
								State.LastChunkID = chunkID;
							}
							break;

						default:
																//we're standing on something else
							Debug.Log ("Standing on mesh terrain");
							State.IsOnTopOfMeshTerrain = true;
																//we may be able to figure out which chunk it belongs to
							DamageableScenery ds = null;
							if (ClosestObjectBelow.gameObject.HasComponent <DamageableScenery> (out ds)) {
								Debug.Log ("Got chunk ID " + ds.ParentChunk.State.ID.ToString () + " from mesh");
								State.LastChunkID = ds.ParentChunk.State.ID;
							}
							break;
						}
					}
				}
			}
		}

		public override void OnStateLoaded ()
		{
			Debug.Log ("State was loaded, last chunk ID is: " + State.LastChunkID.ToString ());
		}

		#endregion

		#region immediate surroundings

		//convenience functions - null checks were ambiguous
		//these make it easier to tell what i'm asking
		public string CurrentDescription {
			get {
				BuildDescription ();
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
				return (WorldItemFocus != null && !WorldItemFocus.Destroyed && WorldItemFocus.IOIType == ItemOfInterestType.WorldItem);
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
				&& Vector3.Distance (player.Position, WorldItemFocusHitInfo.point) < Globals.PlayerPickUpRange
				&& ClosestObjectFocus == WorldItemFocus);
			}
		}

		public bool IsTerrainInRange {
			get {
				return (IsTerrainInPlayerFocus && Vector3.Distance (player.Position, TerrainFocusHitInfo.point) < Globals.PlayerPickUpRange);
			}
		}

		public bool IsTerrainUnderGrabber {
			get {
				return (TerrainUnderGrabber != null && Vector3.Distance (player.Position, TerrainUnderGrabberHitInfo.point) < Globals.PlayerPickUpRange);
			}
		}

		public bool IsWorldItemUnderGrabber {
			get {
				return (WorldItemUnderGrabber != null && Vector3.Distance (player.Position, WorldItemUnderGrabberHitInfo.point) < Globals.PlayerPickUpRange);
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

		protected void BuildDescription ()
		{
			StringBuilder sb = new StringBuilder ();

			if (IsUnderground) {
				sb.AppendLine ("You are underground");
			} else if (IsInSafeLocation) {
				sb.AppendLine ("You are in a safe location");
			} else if (IsInsideStructure) {
				sb.AppendLine ("You are inside a structure");
			}
			if (IsInCivilization) {
				sb.AppendLine ("You are in a location with ties to civilization.");
			} else {
				sb.AppendLine ("You are in the wild.");
			}

			switch (player.Status.LatestTemperatureExposure) {
			case TemperatureRange.A_DeadlyCold:
				sb.AppendLine ("Your surroundings are deadly cold");
				break;

			case TemperatureRange.B_Cold:
				sb.AppendLine ("Your surroundings are uncomfortably cold");
				break;

			case TemperatureRange.C_Warm:
				sb.AppendLine ("Your surroundings are comfortably warm");
				break;

			case TemperatureRange.D_Hot:
				sb.AppendLine ("Your surroundings are uncomfortably hot");
				break;

			case TemperatureRange.E_DeadlyHot:
				sb.AppendLine ("Your surroundings are deadly hot");
				break;
			}

			mCurrentDescription = sb.ToString ();
			sb.Clear ();
			sb = null;
		}

		public Color TerrainType {
			get {
				return mTerrainType;
			}
		}

		public float YDistanceFromCoast = 10.0f;
		public Vector3 LastPositionOnland;
		public IItemOfInterest ClosestObjectBelow;
		public IItemOfInterest ClosestObjectAbove;
		public IItemOfInterest ClosestObjectForward;
		public IItemOfInterest ClosestObjectFocus;
		public IItemOfInterest ClosestObjectInRange;
		public BodyPart ClosestBodyPartInRange;
		public RaycastHit ClosestObjectBelowHitInfo;
		public RaycastHit ClosestObjectAboveHitInfo;
		public RaycastHit ClosestObjectForwardHitInfo;
		public RaycastHit ClosestObjectFocusHitInfo;
		public RaycastHit ClosestObjectInRangeHitInfo;
		public IItemOfInterest WorldItemFocus;
		public BodyPart BodyPartFocus;
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

		public void CheckCivilization ()
		{
			bool isInCivilization = IsInCivilization;
			if (!isInCivilization && mInCivilizationLastFrame) {
				OnCivilizationExit ();
			} else if (isInCivilization && !mInCivilizationLastFrame) {
				OnCivilizationEnter ();
			}
			mInCivilizationLastFrame = isInCivilization;
		}

		public void AddCivilizationBoost (float effectTime)
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
		public List <PilgrimStop> PilgrimStops = new List <PilgrimStop> ();
		public List <Location> VisitingLocations = new List <Location> ();

		public PilgrimStop CurrentPilgrimStop { get { return PilgrimStops [0]; } }

		public Location CurrentLocation { get { return VisitingLocations [0]; } }

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

		public bool IsVisitingStructure (Structure structure)
		{
			if (IsInsideStructure) {
				return structure == LastStructureEntered;
			}
			return false;
		}

		public void PassThroughEntrance (Dynamic entrance, bool enter)
		{
			if (entrance.ParentStructure.StructureShingle.PropertyIsDestroyed) {
				return;
			}

			if (entrance.State.Type == WorldStructureObjectType.OuterEntrance) {
				if (IsVisitingStructure (entrance.ParentStructure)) {
					StructureExit (entrance.ParentStructure);
				} else {
					StructureEnter (entrance.ParentStructure);
				}
			}
		}

		public bool IsOutside {
			get {
				return State.IsOutside;
			}
		}

		public bool IsInCivilization {
			get {//TODO link this to following a path
				//either we're in a location that's civilized
				//or else we're standing on terrain type civilized
				if (!mInitialized || !player.HasSpawned) {
					return true;
				}
				if (WorldClock.AdjustedRealTime < mCivilizationBoostEnd) {
					//we've been given a boost, probably by a spyglass
					return true;
				}
				if (Paths.HasActivePath) {
					return true;
				}
				if (IsVisitingLocation) {
					for (int i = 0; i < VisitingLocations.Count; i++) {
						if (VisitingLocations [i].IsCivilized) {
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsUnderground {
			get {
				return State.IsUnderground;
			}
		}

		public bool IsInSafeLocation {
			get {
				if (!mInitialized || !player.HasSpawned) {
					return false;
				}

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

		public IEnumerator CheckSurroundings ()
		{
			mCheckingSurroundings = true;
			while (mCheckingSurroundings) {
				while (!player.HasSpawned || player.IsHijacked || !GameManager.Is (FGameState.InGame)) {
					//wait it out, we'll get bad data
					yield return null;
				}
				//put a tick between each of these
				//TODO may have to move RaycastFocus to separate coroutine
				ClearSurroundings ();
				try {
					mPlayerHeadPosition = player.HeadPosition;
					RaycastAllFocus ();
				} catch (Exception e) {
					Debug.LogException (e);
				}
				yield return null;
				try {
					RaycastAllUp ();
				} catch (Exception e) {
					Debug.LogException (e);
				}
				//boy these checks every frame are annoying, TODO fix these please
				yield return null;
				if (player.HasSpawned && !player.IsHijacked && GameManager.Is (FGameState.InGame)) {
					mPlayerHeadPosition = player.HeadPosition;
					CheckDirectSunlight ();
				}
				yield return null;
				if (player.HasSpawned && !player.IsHijacked && GameManager.Is (FGameState.InGame)) {
					mPlayerHeadPosition = player.HeadPosition;
					CheckTerrainType ();
				}
				yield return null;
				if (player.HasSpawned && !player.IsHijacked && GameManager.Is (FGameState.InGame)) {
					CheckDanger ();
				}
				yield return null;
				if (player.HasSpawned && !player.IsHijacked && GameManager.Is (FGameState.InGame)) {
					CheckAnimalDens ();
				}
				yield return null;
				if (player.HasSpawned && !player.IsHijacked && GameManager.Is (FGameState.InGame)) {
					//TODO this is kind of a kludge... put this somewhere else
					YDistanceFromCoast = player.Position.y - Biomes.Get.TideWaterElevation;
					yield return null;
					CheckExposure ();

					State.LastChunkID = GameWorld.Get.PrimaryChunkID;
					State.LastPosition.CopyFrom (player.tr);
				}
				yield return null;
			}
			mCheckingSurroundings = false;
			yield break;
		}

		protected bool mCheckingSurroundings = false;

		#region audio

		public bool IsSoundAudible (Vector3 origin, float audibleRadius)
		{	//leaving room for audibility skills here
			return (Vector3.Distance (player.Position, origin) <= audibleRadius * 2.0f);//TEMP
		}

		#endregion

		#region enter / exit

		public void EnterUnderground ()
		{
			State.IsUnderground = true;
			State.EnterUndergroundTime = WorldClock.AdjustedRealTime;
			Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationUndergroundEnter), WorldClock.AdjustedRealTime);
			GUIManager.PostInfo ("You are under ground.");
			//player.SaveState ();
		}

		public void ExitUnderground ()
		{
			State.IsUnderground = false;
			State.ExitUndergroundTime = WorldClock.AdjustedRealTime;
			Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationUndergroundExit), WorldClock.AdjustedRealTime);
			GUIManager.PostInfo ("You are above ground.");
			//player.SaveState ();
		}

		public bool MoveEnterWater (double timeStamp)
		{
			player.Status.AddCondition ("Wet");//do this here so it's immediate
			State.IsInWater = true;
			return true;
		}

		public bool MoveExitWater (double timeStamp)
		{
			State.IsInWater = false;
			return true;
		}

		public void StructureEnter (Structure structure)
		{
			if (structure.IsDestroyed) {
				return;
			}
			if (LastStructureEntered == structure) {
				return;
			}

			structure.OnPlayerEnter.SafeInvoke ();
			LastStructureEntered = structure;
			State.LastStructureEntered = structure.worlditem.StaticReference;
			State.EnterStructureTime = WorldClock.AdjustedRealTime;
			structure.worlditem.Get <Revealable> ().State.UnknownUntilVisited = false;
			Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationStructureEnter), WorldClock.AdjustedRealTime);

			if (structure.State.IsRespawnStructure) {
				State.VisitedRespawnStructures.SafeAdd (structure.worlditem.StaticReference);
			}
			//player.SaveState ();
		}

		public void StructureExit (Structure structure)
		{
			structure.OnPlayerExit.SafeInvoke ();
			LastStructureEntered = null;
			State.LastStructureExited = structure.worlditem.StaticReference;
			State.ExitStructureTime = WorldClock.AdjustedRealTime;
			Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationStructureExit), WorldClock.AdjustedRealTime);
			//player.SaveState ();
		}

		public bool IsVisiting (MobileReference location)
		{
			return State.VisitingLocations.Contains (location);
		}

		public bool IsVisiting (Location location)
		{
			return State.VisitingLocations.Contains (location.worlditem.StaticReference);
		}

		public void PilgrimStopVisit (PilgrimStop pilgrimStop)
		{
			CleanPilgrimStops ();
			if (!PilgrimStops.Contains (pilgrimStop)) {
				PilgrimStops.Add (pilgrimStop);
			}
		}

		public void CleanPilgrimStops ()
		{
			//clean existing
			for (int i = PilgrimStops.Count - 1; i >= 0; i--) {
				Location location = null;
				if (PilgrimStops [i] != null && PilgrimStops [i].worlditem.Is <Location> (out location)) {
					//if (!WorldItems.IsInActiveRange (location.worlditem, player.Position, 1.0f)) {
					//	PilgrimStops.RemoveAt (i);
					//}
				}
			}
		}

		public void LeaveAllLocations ()
		{
			for (int i = 0; i < VisitingLocations.Count; i++) {
				Visitable visitable = null;
				if (VisitingLocations [i] != null && VisitingLocations [i].worlditem.Is<Visitable> (out visitable)) {
					Leave (visitable);
				}
			}
			State.VisitingLocations.Clear ();
			VisitingLocations.Clear ();
		}

		public bool Reveal (MobileReference mr)
		{
			if (Profile.Get.CurrentGame.RevealedLocations.SafeAdd (mr)) {
				Profile.Get.CurrentGame.NewLocations.SafeAdd (mr);
				Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationReveal), WorldClock.AdjustedRealTime);
				return true;
			} else {
				return false;
			}
		}

		public void Reveal (Revealable revealable)
		{
			Reveal (revealable.worlditem.StaticReference);
		}

		public void Visit (Visitable visitable)
		{
			Location location = null;
			if (visitable.worlditem.Is <Location> (out location)) {
				if (VisitingLocations.SafeAdd (location)) {
					VisitingLocations.Sort ();
					State.VisitingLocations.SafeAdd (location.worlditem.StaticReference);
					State.LastLocationVisited = location.worlditem.StaticReference;
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationVisit), WorldClock.AdjustedRealTime);
				}
				//player.SaveState ();
			}
		}

		public void Leave (Visitable visitable)
		{
			Location location = null;
			if (visitable.worlditem.Is<Location> (out location)) {
				State.VisitingLocations.Remove (location.worlditem.StaticReference);
				State.LastLocationExited = location.worlditem.StaticReference;
				VisitingLocations.Sort ();
				VisitingLocations.Remove (location);
				Player.Get.AvatarActions.ReceiveAction ((AvatarAction.LocationLeave), WorldClock.AdjustedRealTime);
				//player.SaveState ();
			}
		}

		public void OnCivilizationExit ()
		{
			//these notifcations are no longer necessary thanks to our active state check
			//GUIManager.PostWarning ("You have entered the wilderness.");
			//player.SaveState ();
		}

		public void OnCivilizationEnter ()
		{
			//these notifcations are no longer necessary thanks to our active state check
			//GUIManager.PostInfo ("You have returned to civilization.");
			//player.SaveState ();
		}

		public void OnDangerEnter ()
		{
			//these notifcations are no longer necessary thanks to our active state check
			//player.SaveState ();
		}

		public void OnDangerExit ()
		{
			//these notifcations are no longer necessary thanks to our active state check
			//player.SaveState ();
		}

		public void AddMapMarker (Vector3 mapMarkerLocation)
		{
			WorldChunk chunk = null;
			if (!GameWorld.Get.ChunkAtPosition (mapMarkerLocation, out chunk)) {
				Debug.Log ("Couldn't find chunk at " + mapMarkerLocation.ToString ());
			}
			//TEMP for now, only one at a time
			if (State.ActiveMapMarkers.Count > 0) {
				//move the existing marker
				State.ActiveMapMarkers [0].ChunkPosition = WorldChunk.WorldPositionToChunkPosition (chunk.ChunkBounds, mapMarkerLocation - chunk.ChunkOffset);
				State.ActiveMapMarkers [0].ChunkID = chunk.State.ID;
				State.ActiveMapMarkers [0].ChunkOffset = chunk.ChunkOffset;
			} else {
				MapMarker mm = new MapMarker ();
				mm.ChunkPosition = WorldChunk.WorldPositionToChunkPosition (chunk.ChunkBounds, mapMarkerLocation - chunk.ChunkOffset);
				;
				mm.ChunkID = chunk.State.ID;
				mm.ChunkOffset = chunk.ChunkOffset;
				State.ActiveMapMarkers.Add (mm);
			}
			FXManager.Get.SpawnMapMarkers (State.ActiveMapMarkers);
		}

		#endregion

		public void Update ()
		{
			//check to see if we have field of view overrides
			if (!GameManager.Is (FGameState.Cutscene)) {
				if (IsVisitingLocation && CurrentLocation.State.RenderDistanceOverride > 0f) {
					GameManager.Get.GameCamera.farClipPlane = Globals.ClippingDistanceFar * CurrentLocation.State.RenderDistanceOverride;
				} else {
					GameManager.Get.GameCamera.farClipPlane = Globals.ClippingDistanceFar;
				}
			}

			if (GameManager.Is (FGameState.InGame) && player.HasSpawned) {
				//we have to do this every frame to identify moving platforms etc
				RaycastAllDown ();
				mSanityCheck++;
				if (mSanityCheck > 20) {
					mSanityCheck = 0;
					GroundSanityCheck ();
				}
			}
		}

		#region hostiles and danger

		protected int mCheckHostiles = 0;
		protected int mSanityCheck = 0;
		protected IItemOfInterest mLastEncounteredScenery;
		public List <IHostile> Hostiles = new List <IHostile> ();
		public int HostilesTargetingPlayer = 0;
		public List <CreatureDen> CreatureDens = new List<CreatureDen> ();

		public void FixedUpdate ()
		{
			if (!GameManager.Is (FGameState.InGame)) {
				return;
			}

			LightManager.RefreshExposure (this);

			mCheckHostiles++;
			if (mCheckHostiles > 5) {//TODO maybe put this in a coroutine
				mCheckHostiles = 0;
				HostilesTargetingPlayer = 0;
				for (int i = Hostiles.LastIndex (); i >= 0; i--) {
					IHostile hostile = Hostiles [i];
					if (hostile == null || !hostile.HasPrimaryTarget || hostile.Mode == HostileMode.CoolingOff) {
						Hostiles.RemoveAt (i);
					} else if (hostile.PrimaryTarget == player) {
						//Debug.Log ("Hostile primary target is player, we're still in danger");
						HostilesTargetingPlayer++;
					}
				}
			}

			//check for encounters
			try {
				if (IsTerrainInPlayerFocus && TerrainFocus != mLastEncounteredScenery) {
					mLastEncounteredScenery = TerrainFocus;
					mLastEncounteredScenery.gameObject.collider.attachedRigidbody.SendMessage ("OnPlayerEncounter", SendMessageOptions.DontRequireReceiver);
				}
			} catch (Exception e) {
				//no big deal
				//Debug.LogError("Proceeding normally: Error when sending OnPlayerEncounter to scenery");
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

		public void CheckDanger ()
		{
			//check existing hostiles for duds
			bool isInDanger = IsInDanger;
			if (mInDangerLastFrame) {
				if (!isInDanger) {
					Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalDangerExit, WorldClock.AdjustedRealTime);
				}
			} else if (isInDanger) {
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalDangerEnter, WorldClock.AdjustedRealTime);
			}
			mInDangerLastFrame = isInDanger;
		}

		public void CheckAnimalDens ()
		{
			for (int i = CreatureDens.Count - 1; i >= 0; i--) {
				if (CreatureDens [i] == null) {
					CreatureDens.RemoveAt (i);
				}
			}

			bool isInCreatureDen = IsInCreatureDen;
			if (mInCreatureDenLastFrame) {
				if (!isInCreatureDen) {
					Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalCreatureDenExit, WorldClock.AdjustedRealTime);
				}
			} else if (isInCreatureDen) {
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalCreatureDenEnter, WorldClock.AdjustedRealTime);
			}
			mInCreatureDenLastFrame = isInCreatureDen;
		}

		public void RemoveHostile (Hostile hostile)
		{
			if (Hostiles.Remove (hostile)) {
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalHostileDeaggro, WorldClock.AdjustedRealTime);
			}
		}

		public void AddHostile (IHostile hostile)
		{
			if (Hostiles.SafeAdd (hostile)) {
				if (hostile.PrimaryTarget == player) {
					HostilesTargetingPlayer++;
				}
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.SurvivalHostileAggro, WorldClock.AdjustedRealTime);
			}
		}

		public void CreatureDenEnter (CreatureDen creatureDen)
		{
			if (!CreatureDens.Contains (creatureDen)) {
				CreatureDens.Add (creatureDen);
			}
		}

		public void CreatureDenExit (CreatureDen creatureDen)
		{
			CreatureDens.Remove (creatureDen);
		}

		protected bool mInDangerLastFrame = false;
		protected bool mInCreatureDenLastFrame = false;

		#endregion

		#region weather

		//these functions aren't used any more
		//i'm keeping them around in case i want to bring them back
		public void CheckDirectSunlight ()
		{
			if (WorldClock.Is (TimeOfDay.ba_LightSunLight)) {
				RaycastHit sunlightHit;
				if (Physics.Raycast (mPlayerHeadPosition, Biomes.Get.SunLightPosition, out sunlightHit, Vector3.Distance (mPlayerHeadPosition, Biomes.Get.SunLightPosition), Globals.LayersActive)) {
					State.InDirectSunlight = false;
				} else {
					State.InDirectSunlight = true;
				}
			} else {
				State.InDirectSunlight = false;
			}
		}

		public void CheckExposure ()
		{
			if (State.ExposedToRain) {
				if (!mExposedToRainLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsExposeToRain), WorldClock.AdjustedRealTime);
				}
			} else {
				if (mExposedToRainLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsShieldFromRain), WorldClock.AdjustedRealTime);
				}
			}
			mExposedToRainLastFrame = State.ExposedToRain;

			if (State.ExposedToSun) {
				if (!mExposedToSunLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsExposeToSun), WorldClock.AdjustedRealTime);
				}
			} else {
				if (mExposedToSunLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsShieldFromSun), WorldClock.AdjustedRealTime);
				}
			}
			mExposedToSunLastFrame = State.InDirectSunlight;

			if (State.ExposedToSky) {
				if (!mExposedToSkyLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsExposeToSky), WorldClock.AdjustedRealTime);
				}
			} else {
				if (mExposedToSkyLastFrame) {
					Player.Get.AvatarActions.ReceiveAction ((AvatarAction.SurroundingsShieldFromSky), WorldClock.AdjustedRealTime);
				}
			}
			mExposedToSkyLastFrame = State.ExposedToSky;
		}

		protected bool mExposedToSunLastFrame = false;
		protected bool mExposedToRainLastFrame = false;
		protected bool mExposedToSkyLastFrame = false;

		#endregion

		#region raycasts

		public void ClearSurroundings ()
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

		public void GroundSanityCheck ()
		{
			if (!IsUnderground && !player.Status.IsStateActive ("Traveling")) {
				Terrain t = null;
				Vector3 playerPosition = player.Position;
				float height = playerPosition.y;
				if (IsSomethingBelowPlayer && ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundTerrain)) {
					t = TerrainBelow.gameObject.GetComponent <Terrain> ();
				} else {
					downRaycastStart = playerPosition + Vector3.up * 100;
					if (Physics.Raycast (downRaycastStart, player.DownVector, out downHit, 150f, Globals.LayersSolidTerrain)) {
						t = downHit.collider.gameObject.GetComponent <Terrain> ();
					}
				}

				if (t != null) {
					height = t.SampleHeight (playerPosition) + t.transform.position.y;
					if (playerPosition.y < height) {
						playerPosition.y = height + 0.25f;
						player.Position = playerPosition;
					}
				}
			}
		}

		public void RaycastAllDown ()
		{
			//if we're on a moving platform take the platform's velocity into account when doing raycasts
			//we don't want to lose the platform
			downRaycastStart = player.Position + Vector3.up * 0.01f;
			float distance = Globals.RaycastAllDownDistance + 0.01f;
			if (IsOnMovingPlatform) {
				downRaycastStart.y = MovingPlatformUnderPlayer.tr.position.y + 0.15f;
				distance += 0.15f;
			}
			downItemOfInterest = null;
			MovingPlatform mp = null;
			RaycastHit[] hits = Physics.RaycastAll (downRaycastStart, player.DownVector, distance, Globals.LayersActive);
			for (int i = 0; i < hits.Length; i++) {
				downHit = hits [i];
				//check for moving platforms directly
				if (mp == null && downHit.collider.attachedRigidbody != null) {
					mp = downHit.collider.attachedRigidbody.GetComponent <MovingPlatform> ();
				}
				if (WorldItems.GetIOIFromCollider (downHit.collider, true, out downItemOfInterest)) {
					switch (downItemOfInterest.IOIType) {
					case ItemOfInterestType.WorldItem:
						WorldItemBelowHitInfo = downHit;
						WorldItemBelow = downItemOfInterest;
						CheckForClosest (ref ClosestObjectBelow, ref ClosestObjectBelowHitInfo, WorldItemBelow, downHit, false);
						break;

					case ItemOfInterestType.Scenery:
						TerrainBelowHitInfo = downHit;
						TerrainBelow = downItemOfInterest;
						CheckForClosest (ref ClosestObjectBelow, ref ClosestObjectBelowHitInfo, TerrainBelow, downHit, false);
						break;

					default:
						break;
					}
				}
			}
			Array.Clear (hits, 0, hits.Length);
			hits = null;

			CheckGroundType++;
			if (CheckGroundType > 4) {
				CheckGroundType = 0;
				if (State.IsInWater) {
					State.GroundBeneathPlayer = GroundType.Water;
				} else {
					LastPositionOnland = player.Position;
					State.GroundBeneathPlayer = GroundType.Dirt;
					if (IsSomethingBelowPlayer) {
						//using if/else instead of a switch so we can use CompareTag
						//saves us a bunch of allocations -_-
						if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundDirt)) {
							State.GroundBeneathPlayer = GroundType.Dirt;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundLeaves)) {
							State.GroundBeneathPlayer = GroundType.Leaves;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundMetal)) {
							State.GroundBeneathPlayer = GroundType.Metal;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundMud)) {
							State.GroundBeneathPlayer = GroundType.Mud;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundSnow)) {
							State.GroundBeneathPlayer = GroundType.Snow;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundStone)) {
							State.GroundBeneathPlayer = GroundType.Stone;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundWater)) {
							State.GroundBeneathPlayer = GroundType.Water;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundWood)) {
							State.GroundBeneathPlayer = GroundType.Wood;
						} else if (ClosestObjectBelow.gameObject.CompareTag (Globals.TagGroundTerrain)) {
							State.GroundBeneathPlayer = GameWorld.Get.GroundTypeAtInGamePosition (player.Position, IsUnderground);
						} else {
							State.GroundBeneathPlayer = GroundType.Dirt;
						}
					}
				}
			}

			if (IsOnMovingPlatform) {
				if (mp == null) {
					if (downHit.collider != null) {
						Debug.Log ("Stepping off moving platform, thing below player is: " + downHit.collider.name);
					} else {
						Debug.Log ("Stepping off moving platform, nothing below player");
					}
				}
			} else {
				if (mp != null) {
					if (downHit.collider != null) {
						Debug.Log ("Stepping on moving platform, thing below player is: " + downHit.collider.name);
					} else {
						Debug.Log ("Stepping on moving platform, nothing below player");
					}
				}
			}
			MovingPlatformUnderPlayer = mp;
		}

		public void RaycastAllFocus ()
		{
			focusItemOfInterest = null;
			//check for terrain in front of us - used mostly for placement of stuff
			if (Physics.Raycast (mPlayerHeadPosition, player.FocusVector, out terrainHit, Globals.RaycastAllFocusDistance, Globals.LayersTerrain)) {
				//check for structure terrain layer first - it will be the parent of the collider
				bool foundTerrainLayer = WorldItems.GetIOIFromCollider (terrainHit.collider, false, out focusItemOfInterest);
				if (!foundTerrainLayer && terrainHit.collider.transform.parent != null) {
					//check for structure terrain layer - it will be the parent of the collider
					focusItemOfInterest = (IItemOfInterest)terrainHit.collider.transform.parent.GetComponent (typeof(IItemOfInterest));
					foundTerrainLayer = focusItemOfInterest != null;
				}
				if (foundTerrainLayer) {
					TerrainFocus = focusItemOfInterest;
					TerrainFocusHitInfo = terrainHit;
					CheckForClosest (ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, focusItemOfInterest, terrainHit, true);
				}
			}

			//we can't do spherecasts for triggers, unfortunately
			//and we need to check for water triggers
			//so do that here, then do the rest as spherecast
			if (Physics.Raycast (mPlayerHeadPosition, player.FocusVector, out worldItemHit, Globals.RaycastAllFocusDistance, Globals.LayerWorldItemActive)) {
				//if (!worldItemHit.collider.isTrigger) {
				if (WorldItems.GetIOIFromCollider (worldItemHit.collider, out focusItemOfInterest, out bodyPartHit)) {
					focusItemOfInterest = CheckForCarried (focusItemOfInterest);
					focusItemOfInterest = CheckForEquipped (focusItemOfInterest);
					if (focusItemOfInterest != null) {
						CheckForClosest (ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, ref ClosestBodyPartInRange, focusItemOfInterest, worldItemHit, bodyPartHit, true);
						CheckForClosest (ref WorldItemFocus, ref WorldItemFocusHitInfo, ref BodyPartFocus, focusItemOfInterest, worldItemHit, bodyPartHit, true);
					}
				}
				//}
			}
			//this result will override any fluid hit results
			//this is desired behavior because we want to be able to pick up objects through triggers
			sphereCastHits = Physics.SphereCastAll (mPlayerHeadPosition, 0.1f, player.FocusVector, Globals.RaycastAllFocusDistance, Globals.LayerWorldItemActive | Globals.LayerBodyPart);
			bool checkObstruction = false;
			if (sphereCastHits.Length > 0) {
				for (int i = 0; i < sphereCastHits.Length; i++) {
					worldItemHit = sphereCastHits [i];
					//we have to check for non-interactive colliders that can block our line of sight
					if (worldItemHit.collider.CompareTag (Globals.TagNonInteractive)) {
						//see if this obstruction is closer than the last one we've hit
						if (checkObstruction) {
							float existingObstructionDistance = Vector3.Distance (mPlayerHeadPosition, mObstructionHit.point);
							float contenderObstructionDistance = Vector3.Dot (mPlayerHeadPosition, worldItemHit.point);
							if (contenderObstructionDistance < existingObstructionDistance) {
								mObstructionHit = worldItemHit;
							}
						} else {
							mObstructionHit = worldItemHit;
							checkObstruction = true;
						}
					} else if (WorldItems.GetIOIFromCollider (worldItemHit.collider, out focusItemOfInterest, out bodyPartHit)) {
						//make sure we're not carrying or equipping this item
						//(we no longer have to check for body parts, get ioi from collider does that for us)
						focusItemOfInterest = CheckForCarried (focusItemOfInterest);
						focusItemOfInterest = CheckForEquipped (focusItemOfInterest);
						if (focusItemOfInterest != null) {
							CheckForClosest (ref ClosestObjectFocus, ref ClosestObjectFocusHitInfo, ref ClosestBodyPartInRange, focusItemOfInterest, worldItemHit, bodyPartHit, true);
							CheckForClosest (ref WorldItemFocus, ref WorldItemFocusHitInfo, ref BodyPartFocus, focusItemOfInterest, worldItemHit, bodyPartHit, true);
						}
					}
				}
				Array.Clear (sphereCastHits, 0, sphereCastHits.Length);
			}

			if (checkObstruction) {
				//see if the closest focus item is closer than our closest obstruction
				float obstructionDistance = Vector3.Distance (mPlayerHeadPosition, mObstructionHit.point);
				if (ClosestObjectFocus != null) {
					float closestObjectDistance = Vector3.Distance (mPlayerHeadPosition, ClosestObjectFocusHitInfo.point);
					if (closestObjectDistance > obstructionDistance) {
						Debug.Log ("Obstruction was closer than closest object, setting to null");
						ClosestObjectFocus = null;
					}
				}
				if (WorldItemFocus != null) {
					float closestWorldItemDistance = Vector3.Distance (mPlayerHeadPosition, WorldItemFocusHitInfo.point);
					if (closestWorldItemDistance > obstructionDistance) {
						Debug.Log ("Obstruction was closer than closest worlditem, setting to null");
						WorldItemFocus = null;
					}
				}
			}

			if (IsWorldItemInPlayerFocus && WorldItemFocus.IOIType == ItemOfInterestType.WorldItem) {
				ReceptacleInPlayerFocus = WorldItemFocus.worlditem.Get <Receptacle> ();
			}
		}

		protected RaycastHit mObstructionHit;
		protected Vector3 mPlayerHeadPosition;

		public void RaycastAllForward ()
		{
			hitsForward = Physics.RaycastAll (mPlayerHeadPosition, player.ForwardVector, Globals.RaycastAllForwardDistance, Globals.LayersActive);

			if (hitsForward.Length > 0) {
				for (int i = 0; i < hitsForward.Length; i++) {
					hitForward = hitsForward [i];
					IItemOfInterest itemOfInterest = null;
					if (WorldItems.GetIOIFromCollider (hitForward.collider, out itemOfInterest)) {
						switch (itemOfInterest.IOIType) {
						case ItemOfInterestType.WorldItem:
							WorldItemForward = itemOfInterest.worlditem;
							WorldItemForwardHitInfo = hitForward;
							CheckForClosest (ref ClosestObjectForward, ref ClosestObjectForwardHitInfo, WorldItemForward, hitForward, false);
							break;

						case ItemOfInterestType.Scenery:
							TerrainForwardHitInfo = hitForward;
							TerrainForward = itemOfInterest;
							CheckForClosest (ref ClosestObjectForward, ref ClosestObjectForwardHitInfo, TerrainForward, hitForward, false);
							break;

						default:
							break;
						}
					}
				}
				Array.Clear (hitsForward, 0, hitsForward.Length);
			}
		}

		public void RaycastAllUp ()
		{
			if (Physics.Raycast (player.Position, player.UpVector, out hitUp, Globals.RaycastAllUpDistance, Globals.LayersActive)) {
				upItemOfInterest = null;
				if (WorldItems.GetIOIFromCollider (hitUp.collider, out upItemOfInterest)) {
					switch (upItemOfInterest.IOIType) {
					case ItemOfInterestType.WorldItem:
						WorldItemAbove = upItemOfInterest;
						WorldItemAboveHitInfo = hitUp;
						CheckForClosest (ref ClosestObjectAbove, ref ClosestObjectAboveHitInfo, WorldItemAbove, hitUp, false);
						break;

					case ItemOfInterestType.Scenery:
						TerrainAboveHitInfo = hitUp;
						TerrainAbove = upItemOfInterest;
						CheckForClosest (ref ClosestObjectAbove, ref ClosestObjectAboveHitInfo, TerrainAbove, hitUp, false);
						break;

					default:
						break;
					}
				}
			}
		}

		protected void CheckForClosest (ref IItemOfInterest currentClosest, ref RaycastHit currentHit, IItemOfInterest contender, RaycastHit hit, bool checkForRange)
		{
			CheckForClosest (ref currentClosest, ref currentHit, ref bodyPartCheck, contender, hit, null, checkForRange);
		}

		protected void CheckForClosest (ref IItemOfInterest currentClosest, ref RaycastHit currentHit, ref BodyPart currentBodyPart, IItemOfInterest contender, RaycastHit hit, BodyPart contenderBodyPart, bool checkForRange)
		{
			if (contender == null)
				return;

			float currentDistance = Vector3.Distance (mPlayerHeadPosition, currentHit.point);
			float contenderDistance = Vector3.Distance (mPlayerHeadPosition, hit.point);

			if (currentClosest == null) {
				currentClosest = contender;
				currentHit = hit;
				currentDistance = contenderDistance;
				currentBodyPart = contenderBodyPart;
			} else if (currentDistance > contenderDistance) {
				currentClosest = contender;
				currentHit = hit;
				currentDistance = contenderDistance;
				currentBodyPart = contenderBodyPart;
			}

			//this gets weird in the case of bodies of water
			//TODO return 'closest' as objects under water when appropriate

			if (checkForRange && currentDistance < Globals.PlayerPickUpRange) {
				if (ClosestObjectInRange == null) {
					ClosestObjectInRange = currentClosest;
					ClosestObjectInRangeHitInfo = currentHit;
					ClosestBodyPartInRange = currentBodyPart;
				} else if (Vector3.Distance (ClosestObjectInRange.Position, mPlayerHeadPosition) < currentDistance) {
					ClosestObjectInRange = currentClosest;
					ClosestObjectInRangeHitInfo = currentHit;
					ClosestBodyPartInRange = currentBodyPart;
				}
			}
		}

		protected IItemOfInterest finalHitObject;

		protected IItemOfInterest CheckForBodyParts (IItemOfInterest hitObject)
		{
			finalHitObject = hitObject;
			if (hitObject.gameObject.CompareTag (Globals.TagBodyArm)
			    || hitObject.gameObject.CompareTag (Globals.TagBodyLeg)
			    || hitObject.gameObject.CompareTag (Globals.TagBodyHead)
			    || hitObject.gameObject.CompareTag (Globals.TagBodyTorso)
			    || hitObject.gameObject.CompareTag (Globals.TagBodyGeneral)) {
				BodyPart bodyPart = hitObject.gameObject.GetComponent <BodyPart> ();
				finalHitObject = bodyPart.Owner;
			}
			return finalHitObject;
		}

		protected IItemOfInterest CheckForEquipped (IItemOfInterest hitObject)
		{
			if (player.Tool.HasWorldItem && player.Tool.worlditem == hitObject) {
				//Debug.Log ("This item is equipped");
				hitObject = null;
			}
			return hitObject;
		}

		protected IItemOfInterest CheckForCarried (IItemOfInterest hitObject)
		{
			if (player.ItemPlacement.IsCarryingSomething && player.ItemPlacement.CarryObject == hitObject.worlditem) {
				//Debug.Log ("We're carring this item");
				hitObject = null;
			}
			return hitObject;
		}

		protected void CheckTerrainType ()
		{
			mTerrainTypeCheck++;
			//only do this once in a while TODO maybe a coroutine would be better
			if (mTerrainTypeCheck > 10) {
				mTerrainTypeCheck = 0;
				if (!player.HasSpawned) {
					mTerrainType = mEmptyTerrainType;
					return;
				}
				mTerrainType = GameWorld.Get.TerrainTypeAtInGamePosition (player.Position, State.IsUnderground);
				//Color color = mTerrainType;
				//Debug.Log ("Got terrain type rgba " + color.r.ToString() + ", " + color.g.ToString() + ", " + color.b.ToString() + ", " + color.a.ToString());
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
		protected BodyPart bodyPartHit;
		protected RaycastHit terrainHit;
		protected RaycastHit[] sphereCastHits;
		protected IItemOfInterest focusItemOfInterest = null;
		protected RaycastHit hitUp;
		protected IItemOfInterest upItemOfInterest = null;
		protected RaycastHit[ ] hitsForward;
		protected RaycastHit hitForward;
		protected BodyPart bodyPartCheck;
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
				for (int i = LightSources.LastIndex (); i >= 0; i--) {
					if (LightSources [i] == null) {
						LightSources.RemoveAt (i);
					} else {
						float distance = Vector3.Distance (transform.position, LightSources [i].transform.position);
						if (distance < nearestDistance) {
							nearestDistance = distance;
							nearestLight = LightSources [i];
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
				for (int i = FireSources.LastIndex (); i >= 0; i--) {
					if (FireSources [i] == null) {
						FireSources.RemoveAt (i);
					} else {
						float distance = Vector3.Distance (transform.position, FireSources [i].transform.position);
						if (distance < nearestDistance) {
							nearestDistance = distance;
							fire = FireSources [i];
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
					mLightSources = new List <WorldLight> ();
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
					mFireSources = new List <Fire> ();
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

		protected Color mTerrainType = Colors.Alpha (Color.black, 0f);
		protected Color mEmptyTerrainType = Colors.Alpha (Color.black, 0f);
	}

	[Serializable]
	public class PlayerSurroundingsState
	{
		public TerrainType TerrainAroundPlayer = TerrainType.Civilization;
		public int LastChunkID = 0;
		public STransform LastPosition = new STransform ();
		public GroundType GroundBeneathPlayer = GroundType.Dirt;
		public MobileReference LastStructureEntered = MobileReference.Empty;
		public MobileReference LastStructureExited = MobileReference.Empty;
		public MobileReference LastCityEntered = MobileReference.Empty;
		public MobileReference LastCityExited = MobileReference.Empty;
		public MobileReference LastLocationVisited = MobileReference.Empty;
		public MobileReference LastLocationExited = MobileReference.Empty;
		public MobileReference LastLocationRevealed = MobileReference.Empty;
		public MobileReference StructureBelowFeet = MobileReference.Empty;
		public List <MobileReference> Hostiles = new List <MobileReference> ();
		public List <MobileReference> VisitedRespawnStructures = new List <MobileReference> ();
		public List <MobileReference> VisitingLocations = new List <MobileReference> ();
		public List <MapMarker> ActiveMapMarkers = new List <MapMarker> ();

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
		public bool IsOnTopOfMeshTerrain = false;

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
				return WorldClock.AdjustedRealTime - EnterUndergroundTime;
			}
		}

		public double TimeSinceExitedUnderground {
			get {
				return WorldClock.AdjustedRealTime - ExitUndergroundTime;
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
}
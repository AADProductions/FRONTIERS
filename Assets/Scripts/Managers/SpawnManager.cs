using UnityEngine;
using System.Collections;
using Frontiers.World;
using Frontiers.Data;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;
using System.Collections.Generic;
using System;

namespace Frontiers
{
		public class SpawnManager : Manager
		{
				public static SpawnManager Get;

				public bool UseStartupPosition {
						get {
								return CurrentStartupPosition != null;
						}
				}

				public PlayerStartupPosition CurrentStartupPosition;

				public bool SpawningPlayer(out string loadingInfo)
				{
						loadingInfo = mLoadingInfo;
						return mSpawningPlayer;
				}

				public override void WakeUp()
				{
						Get = this;
				}

				public HouseOfHealing LastHouseOfHealing = null;

				public void SpawnInClosestStructure(Vector3 despawnPosition, List <MobileReference> structureReferences, Action <Bed> OnFinishAction)
				{
						StartCoroutine(SpawnInClosestStructureOverTime(despawnPosition, structureReferences, OnFinishAction));
				}

				public void SpawnInBed(MobileReference bedReference, Action <Bed> OnFinishAction)
				{
						StartCoroutine(SpawnInBedOverTime(bedReference, OnFinishAction));
				}

				public override void OnModsLoadStart()
				{
						CurrentStartupPosition = null;
				}

				public override void OnGameLoadFirstTime()
				{
						Debug.Log("Loading game for first time");
						string startupPositionName = GameWorld.Get.Settings.FirstStartupPosition;
						if (!string.IsNullOrEmpty(Profile.Get.CurrentGame.CustomStartupPosition)) {
								startupPositionName = Profile.Get.CurrentGame.CustomStartupPosition;
						}
						for (int i = 0; i < GameWorld.Get.WorldStartupPositions.Count; i++) {
								if (GameWorld.Get.WorldStartupPositions[i].Name == startupPositionName) {
										CurrentStartupPosition = GameWorld.Get.WorldStartupPositions[i];
										break;
								}
						}
				}

				public override void OnGameLoadFinish()
				{
						StartCoroutine(SendPlayerToStartupPosition(CurrentStartupPosition));
						mGameLoaded = true;
				}

				protected IEnumerator SpawnInBedOverTime(MobileReference bedReference, Action <Bed> OnFinishAction)
				{
						if (mSpawningPlayer) {
								//whoops
								yield break;
						}
						mSpawningPlayer = true;

						CurrentStartupPosition = null;

						Bed loadedBed = null;
						mSpawnBedWorldItem = null;
						//super-load the item
						StartCoroutine(WIGroups.SuperLoadChildItem(bedReference.GroupPath, bedReference.FileName, SpawnBedLoaded, 0f));

						while (mSpawnBedWorldItem == null || !mSpawnBedWorldItem.Is(WILoadState.Initialized)) {
								//waiting for bed
								yield return null;
						}

						loadedBed = mSpawnBedWorldItem.Get <Bed>();

						OnFinishAction(loadedBed);
						mSpawningPlayer = false;
						yield break;
				}
				//this function is meant to be used with the HouseOfHealing skill
				//i've only been able to get this process to work a few times
				//the idea is to super-load the last visited HOH
				//then load its interior and put the player in one of its beds
				//great in theory but in practice something always fucks up
				protected IEnumerator SpawnInClosestStructureOverTime(Vector3 despawnPosition, List <MobileReference> structureReferences, Action <Bed> OnFinishAction)
				{
						if (mSpawningPlayer) {
								//whoops
								yield break;
						}
						mSpawningPlayer = true;

						CurrentStartupPosition = null;

						float closestDistanceSoFar = Mathf.Infinity;
						StackItem closestStructureStateSoFar = null;
						StackItem currentStructureState = null;
						MobileReference currentStructureReference = null;
						MobileReference closestStructureReferenceSoFar = null;
						Vector3 closestStructurePositionSoFar = despawnPosition;
						WorldChunk currentChunk = null;

						//find out which structure is the closest
						for (int i = 0; i < structureReferences.Count; i++) {
								currentStructureReference = structureReferences[i];
								//Debug.Log ("SPAWNMANAGER: Checking structure reference " + currentStructureReference.FullPath.ToString ());
								if (WIGroups.LoadStackItem(currentStructureReference, out currentStructureState)) {
										if (currentStructureState.Is <Structure>()) {
												//get the chunk for this item
												if (GameWorld.Get.ChunkByID(currentStructureReference.ChunkID, out currentChunk)) {
														Vector3 structureWorldPosition = WorldChunk.ChunkPositionToWorldPosition(currentChunk.ChunkBounds, currentStructureState.ChunkPosition);
														structureWorldPosition += currentChunk.ChunkOffset;
														float currentDistance = Vector3.Distance(despawnPosition, structureWorldPosition);
														if (currentDistance < closestDistanceSoFar) {
																closestDistanceSoFar = currentDistance;
																closestStructureStateSoFar = currentStructureState;
																closestStructureReferenceSoFar = currentStructureReference;
																closestStructurePositionSoFar = structureWorldPosition;
														}

												}
										}
								} 
						}

						if (closestStructureStateSoFar == null) {
								yield break;
						}

						//move the player to the position of the new item
						//this will help us to super-load the structure
						Player.Local.Position = closestStructurePositionSoFar;

						//reset the current loading structure
						mSpawnStructureWorldItem = null;
						Structure spawnStructure = null;
						Bed loadedBed = null;

						//super-load the item
						//yield return null;
						//WorldItems.Get.SuspendActiveStateChecking = true;
						StartCoroutine(WIGroups.SuperLoadChildItem(closestStructureReferenceSoFar.GroupPath, closestStructureReferenceSoFar.FileName, SpawnStructureLoaded, 0f));
						WIGroup lastGroupLoaded = null;

						while (mSpawnStructureWorldItem == null) {
								yield return null;
						}

						//WorldItems.Get.SuspendActiveStateChecking = false;
						//okay next we have to load the structure interior
						mSpawnStructureWorldItem.ActiveStateLocked = false;
						mSpawnStructureWorldItem.ActiveState = WIActiveState.Active;
						mSpawnStructureWorldItem.ActiveStateLocked = true;
						yield return null;
						//now that the player is where they're supposed to be we can resume active state checking
						//but keep the structure itself locked
						//Player.Local.Position = mSpawnStructureWorldItem.Position;
						//ColoredDebug.Log ("Sending player to spawn structure position " + mSpawnStructureWorldItem.Position.ToString (), "Yellow");

						spawnStructure = mSpawnStructureWorldItem.Get <Structure>();
						spawnStructure.LoadPriority = StructureLoadPriority.SpawnPoint;
						if (spawnStructure.Is(StructureLoadState.ExteriorUnloaded)) {
								yield return StartCoroutine(spawnStructure.CreateStructureGroups(StructureLoadState.ExteriorLoaded));
								Structures.AddExteriorToLoad(spawnStructure);
								while (spawnStructure.Is(StructureLoadState.ExteriorLoading | StructureLoadState.ExteriorWaitingToLoad)) {
										yield return null;
								}
						}
						if (spawnStructure.Is(StructureLoadState.ExteriorLoaded)) {
								Structures.AddInteriorToLoad(spawnStructure);
								while (spawnStructure.Is(StructureLoadState.InteriorWaitingToLoad | StructureLoadState.InteriorLoading)) {
										yield return null;
								}
								spawnStructure.RefreshColliders(true);
								spawnStructure.RefreshRenderers(true);
								//finally we search for a bed
						}
						spawnStructure.StructureGroup.Load();
						while (!spawnStructure.StructureGroup.Is(WIGroupLoadState.Loaded)) {
								yield return null;
						}

						yield return null;
						//wait a tick to let the group catch up
						//wait a tick to let the group catch up
						yield return null;

						List <WorldItem> bedWorldItems = new List<WorldItem>();
						while (bedWorldItems.Count == 0) {
								yield return StartCoroutine(WIGroups.GetAllChildrenByType(spawnStructure.StructureGroup.Props.PathName, new List<string>() { "Bed" }, bedWorldItems, spawnStructure.worlditem.Position, Mathf.Infinity, 1));
								//can't use WaitForSeconds because timescale will be zero
								yield return WorldClock.WaitForRTSeconds(0.1f);
						}

						WorldItem bedWorldItem = bedWorldItems[0];
						while (!bedWorldItem.Is(WILoadState.Initialized)) {
								yield return null;
						}
						loadedBed = bedWorldItem.Get <Bed>();
						//Player.Local.Position = loadedBed.BedsidePosition;
						mSpawningPlayer = false;
						mSpawnStructureWorldItem.ActiveStateLocked = false;
						OnFinishAction(loadedBed);
						Player.Local.Surroundings.StructureEnter(spawnStructure);
						yield break;
				}

				public void SpawnStructureLoaded(WorldItem spawnStructureWorldItem)
				{
						mSpawnStructureWorldItem = spawnStructureWorldItem;
				}

				public void SpawnBedLoaded(WorldItem spawnBedWorldItem)
				{
						mSpawnBedWorldItem = spawnBedWorldItem;
				}

				public IEnumerator SendPlayerToStartupPosition(string startupPositionName, float delay)
				{
						if (mSpawningPlayer) {
								//whoops
								yield break;
						}
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + delay;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}

						PlayerStartupPosition firstStartupPosition = null;
						for (int i = 0; i < GameWorld.Get.WorldStartupPositions.Count; i++) {
								if (GameWorld.Get.WorldStartupPositions[i].Name == startupPositionName) {
										firstStartupPosition = GameWorld.Get.WorldStartupPositions[i];
										break;
								}
						}

						if (firstStartupPosition == null) {
								firstStartupPosition = GameWorld.Get.WorldStartupPositions[0];
						}

						yield return StartCoroutine(SendPlayerToStartupPosition(firstStartupPosition));
						yield break;
				}

				public IEnumerator SendPlayerToStartupPosition(PlayerStartupPosition startupPosition)
				{
						if (mSpawningPlayer) {
								//whoops
								yield break;
						}

						mSpawningPlayer = true;

						if (!GUILoading.IsLoading) {
								//let that go on its own, don't wait for it
								StartCoroutine(GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack));
						}

						//if the startup position is null
						//use the character's state as a startup position
						if (startupPosition == null) {
								//the player state should be not-null by now
								Debug.Log("Using player startup position");
								startupPosition = Player.Local.GetStartupPosition();
						}

						if (Player.Local.HasSpawned) {
								Player.Local.Despawn();
						}
						//set the current spawn position
						CurrentStartupPosition = startupPosition;
						yield return null;

						//set all existing world chunks to unclaimed
						mLoadingInfo = "Unloading chunks";
						//suspend chunk loading so the player's position doesn't get the world confused
						GameWorld.Get.SuspendChunkLoading = true;
						for (int i = 0; i < GameWorld.Get.WorldChunks.Count; i++) {
								if (GameWorld.Get.WorldChunks[i].State.ID != startupPosition.ChunkID) {
										GameWorld.Get.WorldChunks[i].TargetMode = ChunkMode.Unloaded;
								}
						}
						//suspend worlditem loading so worlditems don't start spawning stuff till we're ready
						WorldItems.Get.SuspendWorldItemUpdates = true;
						//WorldItems.Get.SetAllWorldItemsToInvisible();
						//wait a moment to let that sink in
						yield return null;
						//put the player in the middle of the chunk to be loaded first
						WorldChunk startChunk = null;
						if (GameWorld.Get.ChunkByID(startupPosition.ChunkID, out startChunk)) {
								Player.Local.Position = startChunk.ChunkOffset + startupPosition.ChunkPosition.Position;
						} else {
								mLoadingInfo = "ERROR: Couldn't find startup chunk";
						}
						//we send the player to the chunk / location specified in the world settings
						mLoadingInfo = "Sending player to startup position";
						//re-enable chunk loading so everything can load
						GameWorld.Get.SuspendChunkLoading = false;
						WorldItems.Get.SuspendWorldItemUpdates = false;
						//set the primary chunk and let it load
						//put the player in the middle of the chunk to be loaded first
						mLoadingInfo = "Waiting for chunks to load";
						while (!GameWorld.Get.ChunksLoaded) {
								yield return null;
						}
						if (!GameWorld.Get.SetPrimaryChunk(startupPosition.ChunkID)) {
								mLoadingInfo = "Couldn't set primary chunk, halting load: " + startupPosition.ChunkID.ToString();
								yield break;
						}
						//set the startup position so managers know where the player will end up
						startupPosition.WorldPosition.Position = (GameWorld.Get.PrimaryChunk.ChunkOffset + startupPosition.ChunkPosition.Position);
						startupPosition.WorldPosition.Rotation = startupPosition.ChunkPosition.Rotation;
						//wait for the rest of the chunk to load
						while (GameWorld.Get.PrimaryChunk.CurrentMode != ChunkMode.Primary) {
								mLoadingInfo = "Waiting for primary chunk to load";
								yield return null;
						}

						//initialize time
						if (startupPosition.RequiresStructure) {
								mLoadingInfo = "Loading structures";
								yield return StartCoroutine(SendPlayerToStructure(
										startupPosition.ChunkID,
										startupPosition.LocationReference,
										startupPosition.StructureName,
										startupPosition.Interior,
										startupPosition.ChunkPosition,
										startChunk.ChunkOffset));
						} else {
								mLoadingInfo = "Loading location";
								yield return StartCoroutine(SendPlayerToLocation(
										startupPosition.ChunkID,
										startupPosition.LocationReference.GroupPath,
										startupPosition.LocationReference.FileName,
										startupPosition.ChunkPosition,
										startChunk.ChunkOffset,
										0.1f));

								//since we're not inside a structure
								//we could be standing on something
								//either a minor structure or chunk prefab
								if (startupPosition.RequiresMeshTerrain) {
										//wait for something to show up below the player before letting them go
										GameWorld.TerrainHeightSearch terrainHit = new GameWorld.TerrainHeightSearch();
										terrainHit.feetPosition = startupPosition.WorldPosition.Position;
										terrainHit.groundedHeight = Globals.DefaultCharacterGroundedHeight;
										terrainHit.overhangHeight = Globals.DefaultCharacterHeight;
										bool foundSomethingBelowPlayer = false;
										double timeOut = WorldClock.RealTime + 30f;
										mLoadingInfo = "Waiting for mesh below player's feet";
										while (WorldClock.RealTime < timeOut) {
												GameWorld.Get.TerrainHeightAtInGamePosition(ref terrainHit);
												//see if we hit a mesh
												if (terrainHit.hitTerrainMesh) {
														break;
												}
												yield return null;
										}
								}
						}

						//immediately add the game offset
						if (startupPosition.AbsoluteTime) {
								WorldClock.ResetAbsoluteTime();
								Profile.Get.CurrentGame.SetWorldTimeOffset(
										startupPosition.TimeHours,
										startupPosition.TimeDays,
										startupPosition.TimeMonths,
										startupPosition.TimeYears);
						} else {
								Profile.Get.CurrentGame.AddWorldTimeOffset(
										startupPosition.TimeHours,
										startupPosition.TimeDays,
										startupPosition.TimeMonths,
										startupPosition.TimeYears);
						}

						//unload anything we're not using
						GC.Collect();
						Resources.UnloadUnusedAssets();
						yield return null;

						if (GUILoading.IsLoading) {
								StartCoroutine(GUILoading.LoadFinish());
						}

						Player.Local.SpawnAtPosition(startupPosition.WorldPosition);

						mSpawningPlayer = false;
						yield break;
				}

				protected IEnumerator SendPlayerToStructure(int chunkID, MobileReference locationReference, string structureName, bool interior, STransform chunkPosition, Vector3 chunkOffset)
				{
						mStartupStructure = null;
						StartCoroutine(WIGroups.SuperLoadChildItem(
								locationReference.GroupPath,
								locationReference.FileName,
								StartupStructureLoaded,
								0f));

						while (mStartupStructure == null) {
								yield return null;
						}

						double startBuildTime = WorldClock.RealTime;
						mStartupStructure.LoadPriority = StructureLoadPriority.SpawnPoint;
						Structures.AddExteriorToLoad(mStartupStructure);

						while (!mStartupStructure.Is(StructureLoadState.ExteriorLoaded | StructureLoadState.InteriorLoading | StructureLoadState.InteriorLoaded)) {
								mLoadingInfo = "Waiting for exterior to build";
								if ((WorldClock.RealTime - startBuildTime) > 20f) {
										mLoadingInfo = "Exterior build timeout!";
										yield break;
								}
								yield return null;
						}
						Structures.AddInteriorToLoad(mStartupStructure);
						startBuildTime = WorldClock.RealTime;

						while (!mStartupStructure.Is(StructureLoadState.InteriorLoaded)) {
								mLoadingInfo = "Waiting for interior to build";
								if ((WorldClock.RealTime - startBuildTime) > 20f) {
										mLoadingInfo = "Interior build timeout!";
										yield break;
								}
								yield return null;
						}
						mLoadingInfo = "Interior structure is built";
						//wait for primary chunk to add details
						//TODO move this into game world
						while (!GameWorld.Get.PrimaryChunk.HasLoadedTerrainDetails || !GameWorld.Get.PrimaryChunk.HasAddedTerrainTrees) {
								mLoadingInfo = "Waiting for chunk to load details";
								yield return null;
						}
						Player.Local.Surroundings.StartupStructure = mStartupStructure;

						yield return null;
						yield break;
				}

				protected Structure mStartupStructure = null;

				protected IEnumerator SendPlayerToActionNode(int chunkID, string actionNodeName, float delay)
				{
						if (!GUILoading.IsLoading)
								yield return GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack);
						Player.Local.Despawn();
						//start off by setting the primary chunk
						GameWorld.Get.SetPrimaryChunk(chunkID);
						//action nodes won't exist unless it's primary
						while (GameWorld.Get.PrimaryChunk.CurrentMode != ChunkMode.Primary) {//wait for the chunk to load before we send it there
								yield return null;
						}
						ActionNodeState nodeState = null;
						if (GameWorld.Get.PrimaryChunk.GetNode(actionNodeName, false, out nodeState)) {
								//send player to location
								string locationPath = WIGroup.AllButLastInPath(nodeState.ParentGroupPath);
								string locationName = WIGroup.LastInPath(nodeState.ParentGroupPath);
								STransform spawnPosition = nodeState.Transform;
								if (nodeState.IsLoaded) {
										spawnPosition = new STransform(nodeState.actionNode.transform, false);
								}
								yield return StartCoroutine(SendPlayerToLocation(chunkID, locationPath, locationName, spawnPosition, GameWorld.Get.PrimaryChunk.ChunkOffset, delay));
						}
						if (GUILoading.IsLoading)
								yield return GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack);
						yield break;
				}

				protected IEnumerator SendPlayerToLocation(int chunkID, string locationPath, string locationName, STransform spawnPosition, Vector3 chunkOffset, float delay)
				{
						//save our target info for when the location loads
						mTargetSpawnPosition = ObjectClone.Clone <STransform>(spawnPosition);
						mTargetSpawnPosition.Position = chunkOffset + mTargetSpawnPosition.Position;
						mTargetLocationName = locationName;
						//set the primary chunk
						GameWorld.Get.SetPrimaryChunk(chunkID);
						while (GameWorld.Get.PrimaryChunk.CurrentMode != ChunkMode.Primary) {
								mLoadingInfo = "Waiting for primary chunk to load";
								yield return null;
						}
						//use a SuperLoader to load the location
						yield return WIGroups.SuperLoadChildItem(locationPath, locationName, null, delay);
						//alright! we've found the location
						//now we can send the player to our target position and spawn
						//send the player to the target position
						yield break;
				}

				protected void StartupStructureLoaded(WorldItem mStartupStructureWorldItem)
				{
						mStartupStructureWorldItem.Initialize();
						mStartupStructure = mStartupStructureWorldItem.Get <Structure>();
				}

				public override void OnLocalPlayerDie()
				{
						CurrentStartupPosition = null;
				}

				public override void OnLocalPlayerSpawn()
				{
						if (UseStartupPosition) {

								if (CurrentStartupPosition.ClearInventory) {
										Player.Local.Inventory.ClearInventory(CurrentStartupPosition.DestroyClearedItems);
								}
								if (CurrentStartupPosition.ClearWearables) {
										Player.Local.Wearables.ClearWearables();
								}
								if (!string.IsNullOrEmpty(CurrentStartupPosition.InventoryFillCategory)) {
										Player.Local.Inventory.FillInventory(CurrentStartupPosition.InventoryFillCategory);
								}
								if (!string.IsNullOrEmpty(CurrentStartupPosition.WearableFillCategory)) {
										Player.Local.Wearables.FillWearables(CurrentStartupPosition.WearableFillCategory);
								}
								for (int i = 0; i < CurrentStartupPosition.CurrencyToAdd.Count; i++) {
										Player.Local.Inventory.InventoryBank.Add(CurrentStartupPosition.CurrencyToAdd[i].Number, CurrentStartupPosition.CurrencyToAdd[i].Type);
								}
								if (CurrentStartupPosition.ClearLog) {
										Books.ClearLog();
										Missions.ClearLog();
										Conversations.ClearLog();
										Blueprints.ClearLog();
								}
								for (int i = 0; i < CurrentStartupPosition.BooksToAdd.Count; i++) {
										Books.AquireBook(CurrentStartupPosition.BooksToAdd[i]);
								}
						}
				}

				protected WorldItem mSpawnStructureWorldItem = null;
				protected WorldItem mSpawnBedWorldItem = null;
				protected string mTargetLocationName = string.Empty;
				protected STransform mTargetSpawnPosition = STransform.zero;
				protected bool mSpawningPlayer;
				protected string mLoadingInfo;
		}
}
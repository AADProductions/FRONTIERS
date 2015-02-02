using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Pathfinding;
using ExtensionMethods;
using Frontiers.GUI;
using Pathfinding.RVO;

public partial class GameWorld : Manager
{
		public static GameWorld Get;
		public RVOSimulator Simulator;
		public WorldSettings Settings = new WorldSettings();
		public WorldState State = new WorldState();
		//mod data
		public List <WorldChunk> WorldChunks = new List <WorldChunk>();
		public List <WorldChunk> ImmediateChunks = new List<WorldChunk>();
		public List <Region> Regions = new List <Region>();
		public List <Biome> Biomes = new List <Biome>();
		public List <AudioProfile> AudioProfiles = new List <AudioProfile>();
		public List <FlagSet> WorldFlags = new List <FlagSet>();
		public List <PlayerStartupPosition> WorldStartupPositions = new List <PlayerStartupPosition>();
		public List <Material> TerrainMaterialList = new List<Material>();
		public List <Terrain> TerrainObjects = new List <Terrain>();
		//current data
		public List <int> ChunkIDs = new List <int>();
		public Region CurrentRegion;
		public Biome CurrentBiome;
		public AudioProfile CurrentAudioProfile;
		public Color32 CurrentRegionData = Color.black;
		public TOD_Sky Sky;
		public TOD_Animation SkyAnimation;
		//tree colliders
		public GameObject TreeColliderPrefab;
		public List <TreeCollider> ActiveColliders = new List <TreeCollider>();
		public Dictionary <TreeCollider, TreeInstanceTemplate> ColliderMappings = new Dictionary <TreeCollider, TreeInstanceTemplate>();
		//loading / unloading
		public bool SuspendChunkLoading = false;
		public bool WorldLoaded = false;
		public bool EditorMode = false;
		public bool ChunksLoaded = false;
		public int TotalChunkPrefabs = 0;

		public int PrimaryChunkID { get { return mPrimaryChunkID; } }

		public WorldChunk PrimaryChunk {
				get {
						if (mPrimaryChunkIndex < WorldChunks.Count) {
								return WorldChunks[mPrimaryChunkIndex];
						}
						return null;
				}
		}
		//terrains
		public List <Terrain> TerrainPool = new List<Terrain>();
		public Queue <Terrain> UnusuedTerrains = new Queue<Terrain>();
		public List <AStarGridSlice> GridSlices = new List <AStarGridSlice>();
		public List <TerrainNode> PointGraphNodes = new List <TerrainNode>();
		public List <WorldItem> ActiveQuestItems = new List <WorldItem>();
		public GameObject EmptyTerrainPrefab = null;
		public GameObject RiverPrefab = null;
		public float TideBaseElevationAtPlayerPosition;

		#region terrain & chunks

		public bool ChunkIDByTerrain(Terrain t, out int id)
		{
				id = -1;
				for (int i = 0; i < WorldChunks.Count; i++) {
						if (WorldChunks[i].HasPrimaryTerrain && WorldChunks[i].PrimaryTerrain == t) {
								id = WorldChunks[i].State.ID;
								break;
						}
				}
				return id > 0;
		}

		public bool ChunkByID(int chunkID, out WorldChunk chunk)
		{
				chunk = null;
				if (mChunkLookup.TryGetValue(chunkID, out chunk)) {
						if (chunk == null) {
								//remove it from the lookup
								mChunkLookup.Remove(chunkID);
								return false;
						} else {
								return true;
						}
				}
				return false;
		}

		public bool SetPrimaryChunk(int chunkID)
		{
				if (PrimaryChunk != null && PrimaryChunk.State.ID == chunkID) {
						PrimaryChunk.TargetMode = ChunkMode.Primary;
						return true;
				}

				WorldChunk newPrimaryChunk = null;
				if (mChunkLookup.TryGetValue(chunkID, out newPrimaryChunk)) {
						mPrimaryChunkIndex = WorldChunks.IndexOf(newPrimaryChunk);
						mPrimaryChunkID = chunkID;
						newPrimaryChunk.TargetMode = ChunkMode.Primary;
						return true;
				} else {
						Debug.Log("Couldn't find primary chunk ID " + chunkID);
				}
				return false;
		}

		public void ShowAboveGround(bool show)
		{
				if (!mShowingAboveGround) {
						mShowingAboveGround = true;
						//done because we can't enable stuff within a physics call
						StartCoroutine(ShowAboveGroundOverTime(show));
				}
		}

		public IEnumerator ShowAboveGroundOverTime(bool show)
		{
				SuspendChunkLoading = true;
				yield return new WaitForEndOfFrame();
				for (int i = 0; i < WorldChunks.Count; i++) {
						WorldChunks[i].ShowAboveGround(show);
						yield return null;
				}
				SuspendChunkLoading = false;
				mShowingAboveGround = false;
				yield break;
		}

		public bool ClaimTerrain(out Terrain newTerrain)
		{
				newTerrain = null;
				if (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
						return false;
				}
				if (UnusuedTerrains.Count > 0) {
						newTerrain = UnusuedTerrains.Dequeue();
				}
				return newTerrain != null;
		}

		public void ReleaseTerain(Terrain terrain)
		{
				if (terrain == null) {
						return;
				}
				if (!UnusuedTerrains.Contains(terrain)) {
						UnusuedTerrains.Enqueue(terrain);
				}
		}

		public Terrain CurrentTerrain {
				get {
						return PrimaryChunk.PrimaryTerrain;
				}
		}

		public void RefreshTerrainDetailSettings()
		{
				float terrainGrassDistance = 0f;
				float terrainGrassDensity = 0f;
				float terrainTreeDistance = 0f;
				float terrainTreeBillboardDistance = 0f;
				float terrainDetail = 0f;
				int terrainMaxMeshTrees = 0;

				for (int i = 0; i < WorldChunks.Count; i++) {
						if (WorldChunks[i].HasPrimaryTerrain) {
								Terrain terrain = WorldChunks[i].PrimaryTerrain;
								terrainGrassDistance = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainGrassDistance;
								terrainGrassDensity = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainGrassDensity;
								terrainTreeBillboardDistance = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainTreeBillboardDistance;
								terrainTreeDistance = Mathf.FloorToInt(Globals.ChunkTerrainTreeDistance * Frontiers.Profile.Get.CurrentPreferences.Video.TerrainTreeDistance);
								terrainMaxMeshTrees = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainMaxMeshTrees;

								switch (WorldChunks[i].CurrentMode) {
										case ChunkMode.Immediate:
										case ChunkMode.Primary:
												terrainDetail = Globals.ChunkTerrainDetailImmediate * Frontiers.Profile.Get.CurrentPreferences.Video.TerrainDetail;
												break;

										case ChunkMode.Adjascent:
												terrainDetail = Globals.ChunkTerrainDetailAdjascent * Frontiers.Profile.Get.CurrentPreferences.Video.TerrainDetail;
												break;

										case ChunkMode.Distant:
												terrainDetail = Globals.ChunkTerrainDetailDistant * Frontiers.Profile.Get.CurrentPreferences.Video.TerrainDetail;
												break;

										case ChunkMode.Unloaded:
										default:
												terrainDetail = 50f;
												break;
								}

								terrain.detailObjectDistance = terrainGrassDistance;
								terrain.detailObjectDensity = terrainGrassDensity;
								terrain.treeBillboardDistance = terrainTreeBillboardDistance;
								terrain.treeCrossFadeLength = 30f;
								terrain.heightmapPixelError = terrainDetail;
								terrain.treeDistance = terrainTreeDistance;
								terrain.treeMaximumFullLODCount = terrainMaxMeshTrees;
								terrain.castShadows = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainShadows;

								Mats.Get.AFS.DetailDistanceForGrassShader = terrainGrassDistance;
								Mats.Get.AFS.BillboardFadeLenght = 30f;
								Mats.Get.AFS.BillboardStart = terrainTreeBillboardDistance;
								Mats.Get.AFS.BillboardFadeOutLength = 50f;
						}
				}
		}
		//this function checks if the chunk at the position is the primary chunk
		protected bool CheckIfPrimary(WorldChunk chunk, int chunkIndex, Vector3 position, bool setIfTrue, out bool isNewPrimary)
		{
				bool isPrimary = chunk.ChunkBounds.Contains(position);
				isNewPrimary = false;
				if (isPrimary) {
						isNewPrimary = (chunk.State.ID != mPrimaryChunkID);
						if (setIfTrue) {//this does the same job as SetPrimaryChunk but without the lookups
								mPrimaryChunkID = chunk.State.ID;
								mPrimaryChunkIndex	= chunkIndex;
								//don't worry about un-setting the previous chunk
								//that will be taken care of by UpdateChunkModes
						}
				}
				return isPrimary;
		}
		//this function will determine if a chunk is immediate, adjascent, distant or unloaded in relation to position
		//it will NOT check if a chunk is primary
		protected ChunkMode GetChunkMode(WorldChunk chunk, int chunkIndex, Vector3 position, bool setMode)
		{
				ChunkMode mode = ChunkMode.Unloaded;
				float distance = Vector3.Distance(position.To2D(), chunk.ChunkBounds.center.To2D());
				if (distance < Globals.ChunkUnloadedDistance) {
						if (distance > Globals.ChunkAdjascentDistance) {
								mode = ChunkMode.Distant;
						} else if (distance > Globals.ChunkImmediateDistance) {
								mode = ChunkMode.Adjascent;
						} else {
								mode = ChunkMode.Immediate;
						}
				}
				if (setMode) {
						chunk.TargetMode = mode;
				}
				return mode;
		}

		#endregion

		#region game init / save / load / unload

		public override void WakeUp()
		{
				Get = this;
				mParentUnderManager = false;

				WorldLoaded = false;
				ChunksLoaded = false;

				CurrentBiome = null;
				CurrentRegion = null;
				CurrentAudioProfile = null;

				#if UNITY_EDITOR
				//this is used for editor flags
				mFlagSetLookup.Clear();
				for (int i = 0; i < WorldFlags.Count; i++) {
						mFlagSetLookup.Add(WorldFlags[i].Name, WorldFlags[i]);
				}
				#endif
		}

		public override void Initialize()
		{
				string errorMessage = string.Empty;
				WorldFlags.Clear();
				WorldStartupPositions.Clear();
				Biomes.Clear();
				Regions.Clear();
				AudioProfiles.Clear();

				GameData.IO.LoadWorld(ref Settings, "FRONTIERS", out errorMessage);

				mInitialized = true;
		}

		public override void OnTextureLoadStart()
		{
				//request all color overlay maps since these can be compressed
				//TODO this didn't work so well due to the mipmap issue
				//Mods.Get.LoadAvailableGenericTextures("ColorOverlay", "ChunkMap", false, Globals.WorldChunkColorOverlayResolution, Globals.WorldChunkColorOverlayResolution, null);
		}

		public override void OnModsLoadStart()
		{
				Mods.Get.Runtime.LoadAvailableMods <FlagSet>(WorldFlags, "FlagSet");
				Mods.Get.Runtime.LoadAvailableMods <Biome>(Biomes, "Biome");
				Mods.Get.Runtime.LoadAvailableMods <Region>(Regions, "Region");
				Mods.Get.Runtime.LoadAvailableMods <AudioProfile>(AudioProfiles, "AudioProfile");
				Mods.Get.Runtime.LoadAvailableMods <PlayerStartupPosition>(WorldStartupPositions, "PlayerStartupPosition");

				//generate the average temperature for each biome
				for (int i = 0; i < Biomes.Count; i++) {
						Biome biome = Biomes[i];
						int sumTemps = 0;

						sumTemps += ((int)biome.StatusTempsSummer.StatusTempQuarterMorning
						+ (int)biome.StatusTempsSummer.StatusTempQuarterAfternoon
						+ (int)biome.StatusTempsSummer.StatusTempQuarterEvening
						+ (int)biome.StatusTempsSummer.StatusTempQuarterNight);

						sumTemps += ((int)biome.StatusTempsAutumn.StatusTempQuarterMorning
						+ (int)biome.StatusTempsAutumn.StatusTempQuarterAfternoon
						+ (int)biome.StatusTempsAutumn.StatusTempQuarterEvening
						+ (int)biome.StatusTempsAutumn.StatusTempQuarterNight);

						sumTemps += ((int)biome.StatusTempsWinter.StatusTempQuarterMorning
						+ (int)biome.StatusTempsWinter.StatusTempQuarterAfternoon
						+ (int)biome.StatusTempsWinter.StatusTempQuarterEvening
						+ (int)biome.StatusTempsWinter.StatusTempQuarterNight);

						sumTemps += ((int)biome.StatusTempsSpring.StatusTempQuarterMorning
						+ (int)biome.StatusTempsSpring.StatusTempQuarterAfternoon
						+ (int)biome.StatusTempsSpring.StatusTempQuarterEvening
						+ (int)biome.StatusTempsSpring.StatusTempQuarterNight);

						//that's 12 total temperatures
						biome.StatusTempAverage = (TemperatureRange)(sumTemps / 12);
						//Debug.Log("Average temperature of biome " + biome.Name + ": " + biome.StatusTempAverage.ToString());
				}

				mFlagSetLookup.Clear();
				for (int i = 0; i < WorldFlags.Count; i++) {
						mFlagSetLookup.Add(WorldFlags[i].Name, WorldFlags[i]);
						WorldFlags[i].Refresh();
				}

				for (int i = 0; i < Biomes.Count; i++) {
						if (string.Equals(Biomes[i].Name, Settings.DefaultBiome)) {
								CurrentBiome = Biomes[i];
						}
						Biomes[i].GenerateAlmanac();
				}

				CurrentAudioProfile = new AudioProfile();
				CurrentAudioProfile.AmbientAudio = Settings.DefaultAmbientAudio;
				CurrentBiome = Biomes[0];
				CurrentRegion = Regions[0];

				TerrainPool.Clear();

				//create our terrain pool
				//create primary terrain
				int[,] emptyDetailLayer = new int [1, 1];
				for (int i = 0; i < Globals.MaxSpawnedChunks; i++) {
						GameObject newTerrainGameObject = GameObject.Instantiate(GameWorld.Get.EmptyTerrainPrefab) as GameObject;
						newTerrainGameObject.name = "TerrainObject " + i.ToString();
						//newTerrainGameObject.transform.parent = transform;
						Terrain newTerrain = newTerrainGameObject.GetComponent <Terrain>();
						newTerrain.transform.localPosition = Vector3.zero;
						newTerrain.gameObject.isStatic = true;

						TerrainData newTerrainData = new TerrainData();
						newTerrainData.baseMapResolution = newTerrain.terrainData.baseMapResolution;
						newTerrainData.heightmapResolution = newTerrain.terrainData.heightmapResolution;
						newTerrainData.alphamapResolution = newTerrain.terrainData.alphamapResolution;
						newTerrainData.SetDetailResolution(newTerrain.terrainData.detailResolution, 8);
						newTerrainData.detailPrototypes = Plants.Get.DefaultDetailPrototypes;
						for (int j = 0; j < Globals.WorldChunkDetailLayers; j++) {
								//this is just to ensure that a detail layer is created for all placeholder prototypes
								newTerrainData.SetDetailLayer(0, 0, j, emptyDetailLayer);
						}

						newTerrain.terrainData = newTerrainData;
						TerrainCollider terrainCollider = newTerrain.GetComponent <TerrainCollider>();
						terrainCollider.terrainData = newTerrainData;
						Material newTerrainMaterial = new Material(newTerrain.materialTemplate);
						newTerrainMaterial.name = newTerrainGameObject.name;
						newTerrain.materialTemplate = newTerrainMaterial;
						Renderer r = newTerrainGameObject.GetComponent <Renderer>();
						r.sharedMaterial = newTerrainMaterial;

						TerrainMaterialList.Add(newTerrainMaterial);
						TerrainObjects.Add(newTerrain);

						newTerrain.enabled = false;
						TerrainPool.Add(newTerrain);
						UnusuedTerrains.Enqueue(newTerrain);
				}

				mModsLoaded = true;
		}

		public IEnumerator LoadChunks()
		{
				WorldChunks.Clear();
				mChunkLookup.Clear();
				mNumChunksLoading = 0;
				TotalChunkPrefabs = 0;

				//create tree colliders
				//add 'dummy' tree instances
				for (int i = 0; i < Globals.NumActiveTreeColliders; i++) {
						GameObject newTreeColliderObject = GameObject.Instantiate(TreeColliderPrefab, Vector3.one * i, Quaternion.identity) as GameObject;
						newTreeColliderObject.transform.parent = transform;
						TreeCollider treeCollider = newTreeColliderObject.GetComponent <TreeCollider>();
						ColliderMappings.Add(treeCollider, null);
						ActiveColliders.Add(treeCollider);
				}

				//ignore collisions between colliders
				for (int i = 0; i < ActiveColliders.Count; i++) {
						for (int j = 0; j < ActiveColliders.Count; j++) {
								if (j != i) {
										Physics.IgnoreCollision(ActiveColliders[i].MainCollider, ActiveColliders[j].MainCollider);
										Physics.IgnoreCollision(ActiveColliders[i].MainCollider, ActiveColliders[j].SecondaryCollider);
										Physics.IgnoreCollision(ActiveColliders[i].SecondaryCollider, ActiveColliders[j].MainCollider);
										Physics.IgnoreCollision(ActiveColliders[i].SecondaryCollider, ActiveColliders[j].SecondaryCollider);
								}
						}
				}

				//set up the chunk global data arrays
				WorldChunk.gHeights = new float [Globals.WorldChunkTerrainHeightmapResolution, Globals.WorldChunkTerrainHeightmapResolution];

				List <string> chunkNames = Mods.Get.ModDataNames("Chunk");
				yield return null;
				foreach (string chunkName in chunkNames) {
						mLoadingInfo = "Loading Chunk " + chunkName;
						//wait a tick
						ChunkState chunkState = null;
						if (Mods.Get.Runtime.LoadMod <ChunkState>(ref chunkState, "Chunk", chunkName)) {
								yield return null;
								GameObject newChunkGameObject = WIGroups.Get.World.gameObject.CreateChild(chunkName).gameObject;
								WorldChunk newChunk = newChunkGameObject.AddComponent <WorldChunk>();
								newChunk.State = chunkState;
								//initialize the new chunk over time
								mLoadingInfo = "Initializing Chunk " + chunkName;
								//this doesn't actually create the chunk
								//it just creates an empty object
								newChunk.Initialize();
								TotalChunkPrefabs += newChunk.SceneryData.TotalChunkPrefabs;
								//once we're done add it to the chunk list
								WorldChunks.Add(newChunk);
								mChunkLookup.Add(newChunk.State.ID, newChunk);
								ChunkIDs.Add(newChunk.State.ID);
						}
				}

				System.GC.Collect();
				Resources.UnloadUnusedAssets();

				mLoadingWorld = false;
				WorldLoaded = true;
				ChunksLoaded = true;

				StartCoroutine(UpdateTreeColliders());

				yield break;
		}

		public override void OnGameSaveStart()
		{
				SuspendChunkLoading = true;
				mGameSaved = false;
				for (int i = 0; i < WorldChunks.Count; i++) {
						WorldChunks[i].OnGameSave();
				}
		}

		public override void OnGameSave()
		{
				SuspendChunkLoading = false;
				mGameSaved = true;
		}

		public override void OnGameUnload()
		{
				mUnloadingWorld = true;
				WorldLoaded = false;
				mLoadingInfo = "Unloading world";
				StartCoroutine(UnloadChunks());
		}

		public override void OnGameLoadFirstTime()
		{
				//this only needs to be done the first time
				//after that it'll be stored in the profile
				Frontiers.Profile.Get.CurrentGame.AddWorldTimeOffset(
						Settings.TimeHours,
						Settings.TimeDays,
						Settings.TimeMonths,
						Settings.TimeYears);
		}

		public override void OnModsLoadFinish()
		{
				mModsLoaded = true;
				mLoadingWorld = true;
				//if the player startup position is null
				//create one from the player's last state
				mLoadingInfo = "Loading game world";
				//put the player at our starting point before we load chunks
				//that way the correct chunks will load
				StartCoroutine(LoadChunks());
		}

		public override void OnLocalPlayerSpawn()
		{
//				if (SpawnManager.Get.UseStartupPosition) {
//						Debug.Log("Using startup position to set biome and region");
//						FindBiomeAndRegion(
//								SpawnManager.Get.CurrentStartupPosition.WorldPosition.Position,
//								ref CurrentRegionData,
//								ref CurrentRegion,
//								ref CurrentBiome,
//								ref CurrentAudioProfile);
//				} else {
//						Debug.Log("Using player local position to set biome and region");
//						FindBiomeAndRegion(
//								Player.Local.Position,
//								ref CurrentRegionData,
//								ref CurrentRegion,
//								ref CurrentBiome,
//								ref CurrentAudioProfile);
//				}
		}

		public override void OnGameStart()
		{
				if (SpawnManager.Get.UseStartupPosition) {
						Debug.Log("Using startup position to set biome and region");
						FindBiomeAndRegion(
								SpawnManager.Get.CurrentStartupPosition.WorldPosition.Position,
								ref CurrentRegionData,
								ref CurrentRegion,
								ref CurrentBiome,
								ref CurrentAudioProfile);
				} else {
						Debug.Log("Using player local position to set biome and region");
						FindBiomeAndRegion(
								Player.Local.Position,
								ref CurrentRegionData,
								ref CurrentRegion,
								ref CurrentBiome,
								ref CurrentAudioProfile);
				}
				//now that we've safely placed the player in the opening chunk
				//we can start loading other chunks
				StartCoroutine(UpdateChunkModes());
		}

		public bool LoadingGameWorld()
		{
				return mLoadingWorld;
		}

		public bool LoadingGameWorld(out string detailInfo)
		{
				detailInfo = mLoadingInfo;
				return mLoadingWorld;
		}

		public bool UnloadingGameWorld(out string detailInfo)
		{
				detailInfo = mLoadingInfo;
				return mUnloadingWorld;
		}

		public IEnumerator UnloadChunks()
		{
				for (int i = 0; i < WorldChunks.Count; i++) {
						mLoadingInfo = "Destroying chunk " + WorldChunks[i].Name;
						GameObject.Destroy(WorldChunks[i].gameObject);
						yield return null;
				}
				mLoadingInfo = "Finished destroying chunks";
				mUnloadingWorld = false;
				mGameLoaded = false;
				yield break;
		}

		#endregion

		#region data search

		//used mostly by motile
		public float InteriorHeightAtInGamePosition(ref TerrainHeightSearch terrainHit)
		{
				terrainHit.normal = Vector3.up;
				terrainHit.overhangNormal = Vector3.down;
				terrainHit.hitWater = false;
				terrainHit.hitTerrain = false;
				terrainHit.hitTerrainMesh = false;
				terrainHit.isGrounded = false;
				terrainHit.terrainHeight = terrainHit.feetPosition.y;//default
				//----overhang
				// |
				// |
				//mRaycastStartPosition
				// |
				// |
				//terrainHit.feetPosition
				//------ground

				float raycastDistance = Mathf.Max(terrainHit.groundedHeight * 2f, 1f);
				mRaycastStartPosition.x = terrainHit.feetPosition.x;
				mRaycastStartPosition.y = terrainHit.feetPosition.y + raycastDistance;
				mRaycastStartPosition.z = terrainHit.feetPosition.z;
				//first get the ground
				int layerMask = Globals.LayersTerrain;
				if (!terrainHit.ignoreWorldItems) {
						layerMask |= Globals.LayerWorldItemActive;
				}
				if (Physics.Raycast(mRaycastStartPosition, Vector3.down, out mHitGround, raycastDistance + terrainHit.groundedHeight, layerMask)) {
						terrainHit.normal = mHitGround.normal;
						terrainHit.isGrounded = true;
						terrainHit.terrainHeight = mHitGround.point.y;
						switch (mHitGround.collider.gameObject.layer) {
								case Globals.LayerNumSolidTerrain:
								default:
										terrainHit.hitTerrain = true;
										if (mHitGround.collider.CompareTag("GroundTerrain")) {
												//only terrian has the GroundTerrain tag
												terrainHit.hitTerrain = true;
										} else {
												terrainHit.hitTerrainMesh = true;
										}
										break;

								case Globals.LayerNumFluidTerrain:
										terrainHit.hitWater = true;
										break;

								case Globals.LayerNumWorldItemActive:
										terrainHit.hitTerrainMesh = true;
										break;
						}
				}

				//now try for the overhang
				if (Physics.Raycast(mRaycastStartPosition, Vector3.up, out mHitOverhang, raycastDistance, Globals.LayersTerrain)) {
						terrainHit.hitOverhang = true;
						terrainHit.overhangHeight = mHitOverhang.point.y;
						terrainHit.overhangNormal = mHitOverhang.normal;
				}

				return terrainHit.terrainHeight;
		}
		//used by pretty much everything - even a minor improvement in this fuction has significant benefits
		public float TerrainHeightAtInGamePosition(ref TerrainHeightSearch terrainHit)
		{
				terrainHit.normal = Vector3.up;
				terrainHit.overhangNormal = Vector3.down;
				terrainHit.hitWater = false;
				terrainHit.hitTerrain = false;
				terrainHit.hitTerrainMesh = false;
				terrainHit.isGrounded = false;
				terrainHit.terrainHeight = terrainHit.feetPosition.y;//default
				//----overhang
				// |
				// |
				//mRaycastStartPosition
				// |
				// |
				//terrainHit.feetPosition
				//------ground

				float raycastDistance = Mathf.Max(terrainHit.groundedHeight * 2f, 1f);
				mRaycastStartPosition.x = terrainHit.feetPosition.x;
				mRaycastStartPosition.y = terrainHit.feetPosition.y + raycastDistance;
				mRaycastStartPosition.z = terrainHit.feetPosition.z;
				//first get the ground
				int layerMask = Globals.LayersTerrain;
				if (!terrainHit.ignoreWorldItems) {
						layerMask |= Globals.LayerWorldItemActive;
				}
				if (Physics.Raycast(mRaycastStartPosition, Vector3.down, out mHitGround, raycastDistance + terrainHit.groundedHeight, layerMask)) {
						terrainHit.normal = mHitGround.normal;
						terrainHit.isGrounded = true;
						terrainHit.terrainHeight = mHitGround.point.y;
						switch (mHitGround.collider.gameObject.layer) {
								case Globals.LayerNumSolidTerrain:
								default:
										terrainHit.hitTerrain = true;
										if (mHitGround.collider.CompareTag("GroundTerrain")) {
												//only terrian has the GroundTerrain tag
												terrainHit.hitTerrain = true;
										} else {
												terrainHit.hitTerrainMesh = true;
										}
										break;

								case Globals.LayerNumFluidTerrain:
										terrainHit.hitWater = true;
										break;

								case Globals.LayerNumWorldItemActive:
										terrainHit.hitTerrainMesh = true;
										break;
						}
				}

				//now try for the overhang
				if (Physics.Raycast(mRaycastStartPosition, Vector3.up, out mHitOverhang, raycastDistance, Globals.LayersTerrain)) {
						terrainHit.hitOverhang = true;
						terrainHit.overhangHeight = mHitOverhang.point.y;
						terrainHit.overhangNormal = mHitOverhang.normal;
				}

				return terrainHit.terrainHeight;
		}

		public Color32 RegionDataAtPosition(Vector3 position)
		{
				WorldChunk chunk = null;
				bool foundChunk = false;
				for (int i = 0; i < WorldChunks.Count; i++) {
						chunk = WorldChunks[i];
						if (!chunk.TerrainData.PassThroughChunkData && chunk.ChunkBounds.Contains(position)) {
								foundChunk = true;
								break;
						}
				}
				if (foundChunk) {
						Texture2D regionData = null;
						if (chunk.ChunkDataMaps.TryGetValue("RegionData", out regionData)) {
								//if (Mods.Get.Runtime.ChunkMap (ref regionData, chunk.Name, "RegionData")) {
								mUv = SplatmapUVFromInGamePosition(position, chunk);
								mRegionData = regionData.GetPixel(Mathf.FloorToInt(mUv.x * regionData.width), Mathf.FloorToInt(mUv.y * regionData.height));
						}
				}
				return mRegionData;
		}

		public bool RegionByName(string regionName, out Region region)
		{
				region = null;
				for (int i = 0; i < Regions.Count; i++) {
						if (Regions[i].Name == regionName) {
								region = Regions[i];
								break;
						}
				}
				return region != null;
		}

		public Color32 DistributionDataAtPosition(Vector3 position)
		{
				WorldChunk chunk = null;
				bool foundChunk = false;
				for (int i = 0; i < WorldChunks.Count; i++) {
						chunk = WorldChunks[i];
						if (!chunk.TerrainData.PassThroughChunkData && chunk.ChunkBounds.Contains(position)) {
								foundChunk = true;
								break;
						}
				}
				if (foundChunk) {
						Texture2D regionData = null;
						if (Mods.Get.Runtime.ChunkMap(ref regionData, chunk.Name, "DistributionData")) {
								mUv = SplatmapUVFromInGamePosition(position, chunk);
								mRegionData = regionData.GetPixel(Mathf.FloorToInt(mUv.x * regionData.width), Mathf.FloorToInt(mUv.y * regionData.height));
						}
				}
				return mRegionData;
		}

		public bool RegionAtPosition(Vector3 position, out Region region)
		{
				mRegionData = RegionDataAtPosition(position);
				region = null;
				for (int i = 0; i < Regions.Count; i++) {
						if (Regions[i].RegionID == mRegionData.b) {
								region = Regions[i];
								break;
						}
				}
				return region != null;
		}

		protected Vector2 mUv;
		protected Color32 mRegionData;

		public int FlagByName(string flagSetName, string flag)
		{
				FlagSet fs = null;
				if (FlagSetByName(flagSetName, out fs)) {
						return fs.GetFlagValue(flag);
				}
				return 0;
		}

		public bool FlagSetByName(string flagSetName, out FlagSet flagSet)
		{
				return mFlagSetLookup.TryGetValue(flagSetName, out flagSet);
		}

		public void FindBiomeAndRegion(Vector3 worldPosition, ref Color32 regionData, ref Region region, ref Biome biome, ref AudioProfile audioProfile)
		{
				regionData = RegionDataAtPosition(worldPosition);

				for (int i = 0; i < Regions.Count; i++) {
						if (Regions[i].RegionID == regionData.b) {
								region = Regions[i];
								break;
						}
				}

				for (int i = 0; i < Biomes.Count; i++) {
						if (Biomes[i].BiomeID == regionData.r) {
								biome = Biomes[i];
								for (int j = 0; j < AudioProfiles.Count; j++) {
										//TODO make this correctly seasonal
										if (string.Equals(AudioProfiles[j].Name, biome.SummerAudioProfile)) {
												audioProfile = AudioProfiles[j];
												break;
										}
								}
								break;
						}
				}
		}

		public bool GetNearestCategoryLocation(string category, Vector3 position, out MobileReference mr)
		{
				mr = null;
				return mr != null;
		}

		public bool ChunkAtPosition(Vector3 position, out WorldChunk chunk)
		{
				chunk = null;
				bool foundChunk = false;
				for (int i = 0; i < WorldChunks.Count; i++) {
						chunk = WorldChunks[i];
						if (chunk.ChunkBounds.Contains(position)) {
								foundChunk = true;
								break;
						}
				}
				return foundChunk;
		}

		public Color TerrainTypeAtInGamePosition(Vector3 position, bool underGround)
		{
				if (GameManager.Get.TestingEnvironment)
						return Color.white;

				mColorResult = Settings.DefaultTerrainType;
				if (!Player.Local.HasSpawned) {
						return Settings.DefaultTerrainType;
				}
				WorldChunk chunk = null;
				if (ChunkAtPosition(position, out chunk)) {
						bool foundMap = false;
						if (underGround) {
								foundMap = chunk.ChunkDataMaps.TryGetValue("BelowGroundTerrainType", out mTerrainTypeMap);
						} else {
								foundMap = chunk.ChunkDataMaps.TryGetValue("AboveGroundTerrainType", out mTerrainTypeMap);
						}

						if (foundMap) {
								mUvCoords = SplatmapUVFromInGamePosition(position, chunk);
								mColorResult = mTerrainTypeMap.GetPixel(Mathf.FloorToInt(mUvCoords.x * mTerrainTypeMap.width), Mathf.FloorToInt(mUvCoords.y * mTerrainTypeMap.height));
								mColorResult.a = Colors.InverseLuminance(mColorResult);
						} 
				}
				return mColorResult;
		}

		public Vector2 SplatmapUVFromInGamePosition(Vector3 position, WorldChunk chunk)
		{
				if (!WorldLoaded || chunk == null || chunk.PrimaryTerrain == null) {
						mUvCoords.x = 0f;
						mUvCoords.y = 0f;
						return mUvCoords;
				}

				mTerrainLocalPos = position - chunk.PrimaryTerrain.transform.position;
				mUvCoords.x = Mathf.InverseLerp(0.0f, chunk.PrimaryTerrain.terrainData.size.x, mTerrainLocalPos.x);
				mUvCoords.y = Mathf.InverseLerp(0.0f, chunk.PrimaryTerrain.terrainData.size.z, mTerrainLocalPos.z);
				return mUvCoords;
		}

		public GroundType GroundTypeAtInGamePosition(Vector3 position, bool underGround)
		{
				if (underGround) {
						return GroundType.Dirt;
				}

				WorldChunk chunkAtPosition = null;
				if (ChunkAtPosition(position, out chunkAtPosition)) {
						if (chunkAtPosition.ChunkDataMaps.TryGetValue("Splat1", out mSplatTexture)) {
								mSplatPosition = SplatmapUVFromInGamePosition(position, chunkAtPosition);
								mSplatColor = mSplatTexture.GetPixel(Mathf.FloorToInt(mSplatPosition.x * mSplatTexture.width), Mathf.FloorToInt(mSplatPosition.y * mSplatTexture.height));
								int largestIndex = 0;
								if (mSplatColor == Color.black) {
										//whoops, we're on the next set
										largestIndex = 4;//rgba
										if (chunkAtPosition.ChunkDataMaps.TryGetValue("Splat2", out mSplatTexture)) {
												mSplatColor = mSplatTexture.GetPixel(Mathf.FloorToInt(mSplatPosition.x * mSplatTexture.width), Mathf.FloorToInt(mSplatPosition.y * mSplatTexture.height));
										} else {
												//nothing to be done!
												return GroundType.Dirt;
										}
								}
								largestIndex += Colors.MaxIndexRGBA(mSplatColor);
								if (largestIndex < chunkAtPosition.TerrainData.SplatmapGroundTypes.Count) {
										return chunkAtPosition.TerrainData.SplatmapGroundTypes[largestIndex];
								}
						}
				}
				//if we can't find this info it's no big deal, don't bother with an error
				return GroundType.Dirt;
		}

		public bool QuestItem(string itemName, out WorldItem questItem)
		{
				questItem = null;
				for (int i = ActiveQuestItems.LastIndex(); i >= 0; i--) {
						if (ActiveQuestItems[i] == null) {
								ActiveQuestItems.RemoveAt(i);
						} else if (ActiveQuestItems[i].QuestName == itemName) {
								questItem = ActiveQuestItems[i];
								break;
						}
				}
				return questItem != null;
		}
		//wth do i need this for? why?
		public static float RandomValue(Vector3 seed)
		{
				return UnityEngine.Random.value;
		}

		[Serializable]
		public struct TerrainHeightSearch
		{
				//input
				public Vector3 feetPosition;
				public float overhangHeight;
				public float groundedHeight;
				//result
				public float terrainHeight;
				public Vector3 normal;
				public Vector3 overhangNormal;
				public bool ignoreWorldItems;
				public bool isGrounded;
				public bool hitTerrain;
				public bool hitTerrainMesh;
				public bool hitWater;
				public bool hitOverhang;
		}

		#endregion

		#region updates

		//this function cycles through chunks and figures out whether they should be
		//Primary 		- the chunk immediately beneath the player's feet
		//Immediate 	- chunks surrounding the primary - apart from not being primary the state is identical
		//Adjascent		- chunks beyond the Immediate chunks
		//Distant		- anything beyond that
		//Disabled		- shut off completely
		protected WaitForSeconds mWaitForSpawn = new WaitForSeconds(0.05f);
		protected WaitForSeconds mWaitForChunkLoop = new WaitForSeconds(1f);
		protected WaitForSeconds mWaitForTreeLoop = new WaitForSeconds(0.5f);

		protected IEnumerator UpdateChunkModes()
		{
				while (!WorldLoaded) {	//wait for world to finish loading
						yield return null;
				}

				while (WorldLoaded) {
						if (GameManager.Get.TestingEnvironment) {
								yield break;
						}

						while (!Player.Local.HasSpawned) {	//if we're loading a chunk manually we don't want to screw it up
								//so wait for the chunk to load
								yield return mWaitForSpawn;
						}

						while (SuspendChunkLoading) {
								yield return null;
						}

						mLatestPlayerPosition = Player.Local.Position;
						FindBiomeAndRegion(mLatestPlayerPosition, ref CurrentRegionData, ref CurrentRegion, ref CurrentBiome, ref CurrentAudioProfile);
						//calculate the current tide base height
						//if the value is <= 0, use the last base height
						if (CurrentRegionData.a > 0) {
								TideBaseElevationAtPlayerPosition = ((float)CurrentRegionData.a) / 255;
						}
						//check to see which chunk is directly below the player's feet
						bool setNewPrimary = false;
						for (int i = 0; i < WorldChunks.Count; i++) {	//check if chunk i is the primary chunk - set it to the primary chunk if true
								//if the player is waiting to go to a position then use the eventual position
								//otherwise use the player's actual position
								if (CheckIfPrimary(WorldChunks[i], i, mLatestPlayerPosition, true, out setNewPrimary)) {	//we've found the primary chunk so we're done with this loop
										break;
								}
						}
						//did we set a new primary chunk? (this would require state changes on all chunks)
						//now it's possible that chunks already have ChunkModeUpdaters attached that are setting now-inaccurate modes
						//so we're going to loop through them really quick and set the modes straight away
						//that way they won't accidentally delete assets we need
						ImmediateChunks.Clear();
						for (int i = 0; i < WorldChunks.Count; i++) {
								ChunkMode currentMode = WorldChunks[i].CurrentMode;
								if (currentMode == ChunkMode.Immediate || currentMode == ChunkMode.Primary) {
										ImmediateChunks.Add(WorldChunks[i]);
								}
								//if we've found the primary index
								if (i == mPrimaryChunkIndex) {	//it may already be trying to go primary but set it anyway
										WorldChunks[i].TargetMode = ChunkMode.Primary;
								} else {
										GetChunkMode(WorldChunks[i], i, Player.Local.Position, true);
								}
								yield return null;
						}
						yield return mWaitForChunkLoop;
				}
		}

		public IEnumerator UpdateTreeColliders()
		{
				while (WorldLoaded) {
						while (!GameManager.Is(FGameState.InGame)) {
								yield return null;
						}
						yield return mWaitForTreeLoop;
						//get all the now-irrelevant colliders
						var enumerator = ColliderAssigner.FindIrrelevantColliders(Player.Local, this).GetEnumerator();
						while (enumerator.MoveNext()) {
								//foreach (TreeCollider irrelevantCollider in ColliderAssigner.FindIrrelevantColliders (Player.Local, this)) {
								if (!GameManager.Is(FGameState.InGame)) {
										//whoops, stop now
										break;
								}
								TreeCollider irrelevantCollider = enumerator.Current;
								//for each one, get the closest tree in need of a tree
								TreeInstanceTemplate closestTree = ColliderAssigner.FindClosestTreeRequiringCollider(Player.Local, this);
								//set the collider position to the closest tree position
								if (closestTree != TreeInstanceTemplate.Empty) {
										closestTree.HasInstance = true;
										irrelevantCollider.Position = closestTree.Position;
										//get the tree collider template from the tree's parent chunk
										if (closestTree.PrototypeIndex < closestTree.ParentChunk.ColliderTemplates.Length) {
												irrelevantCollider.CopyFrom(closestTree.ParentChunk.ColliderTemplates[closestTree.PrototypeIndex]);
												irrelevantCollider.ParentChunk = closestTree.ParentChunk;
										}
										//we can guarantee that collider mappings will have an entry
										//but we don't know if it will be null or not
										TreeInstanceTemplate existingTree = ColliderMappings[irrelevantCollider];
										if (existingTree != null) {
												//if it's not null, let it know that it has no collider
												existingTree.HasInstance = false;
										}
										//update the reference
										ColliderMappings[irrelevantCollider] = closestTree;
								}
								//wait a tick
								yield return null;
						}
				}
				yield break;
		}

		public void RefreshAstarPathSlices()
		{
				bool scanGridSlices = false;
				foreach (AStarGridSlice gridSlice in GridSlices) {	//refresh each grid slice
						scanGridSlices |= gridSlice.Refresh(Player.Local.Position);
				}
		}

		public void DetatchChunkNeighbors()
		{
				for (int i = 0; i < TerrainObjects.Count; i++) {
						TerrainObjects[i].SetNeighbors(null, null, null, null);
				}
		}

		public void ReattachChunkNeighbors()
		{
				WorldChunk cTop = null;
				WorldChunk cBot = null;
				WorldChunk cLft = null;
				WorldChunk cRgt = null;

				Terrain current = null;
				Terrain tTop = null;
				Terrain tBot = null;
				Terrain tLft = null;
				Terrain tRgt = null;

				for (int i = 0; i < WorldChunks.Count; i++) {
						WorldChunk chunk = WorldChunks[i];
						tTop = null;
						tBot = null;
						tLft = null;
						tRgt = null;
						if (chunk.HasPrimaryTerrain) {
								current = chunk.PrimaryTerrain;
								//now get each neighboring chunk by ID
								if (mChunkLookup.TryGetValue(chunk.State.NeighboringChunkTop, out cTop) && cTop.HasPrimaryTerrain) {
										tTop = cTop.PrimaryTerrain;
								}
								if (mChunkLookup.TryGetValue(chunk.State.NeighboringChunkBot, out cBot) && cBot.HasPrimaryTerrain) {
										tBot = cBot.PrimaryTerrain;
								}
								if (mChunkLookup.TryGetValue(chunk.State.NeighboringChunkLeft, out cLft) && cLft.HasPrimaryTerrain) {
										tLft = cLft.PrimaryTerrain;
								}
								if (mChunkLookup.TryGetValue(chunk.State.NeighboringChunkRight, out cRgt) && cRgt.HasPrimaryTerrain) {
										tRgt = cRgt.PrimaryTerrain;
								}
								//now set neighbors
								current.SetNeighbors(tLft, tTop, tRgt, tBot);
						}
				}

				//flush everything
				for (int i = 0; i < TerrainObjects.Count; i++) {
						TerrainObjects[i].Flush();
				}
		}

		#endregion

		#region temperature

		//this is used by locations like thermals to make things temporarily different
		public void AddTemperatureOverride(TemperatureRange temp, float rtSeconds)
		{		//TODO now that we're not using global states in our status temp functions
				//figure out how to implement this again...
				mTemperatureOverride = temp;
				mTemperatureOverrideEndTime = WorldClock.AdjustedRealTime + rtSeconds;
		}
		//returns a temperature range adjusted for above ground / below ground, structure and civilization modifiers
		public TemperatureRange StatusTemperature(Vector3 worldPosition, TimeOfDay timeOfDay, TimeOfYear timeOfYear, bool underground, bool insideStructure, bool inCivlization)
		{
				if (insideStructure || inCivlization) {
						//a civilized structure always has a nice warm temperature
						return TemperatureRange.C_Warm;
				} else if (underground) {
						//use an average temperature to make caves useful as shelter
						return AverageStatusTemperature(worldPosition);
				} else {
						//get the normal temp for the area
						return StatusTemperature(worldPosition, timeOfDay, timeOfYear);
				}
		}

		public TemperatureRange AverageStatusTemperature(Vector3 worldPosition)
		{
				Biome biome = null;
				BiomeStatusTemps currentSeason = null;
				//get the region data for this position and look up the biome
				Color32 regionData = RegionDataAtPosition(worldPosition);
				//TODO maybe put this in a lookup? less than 10 biomes, whatever doesn't matter
				for (int i = 0; i < Biomes.Count; i++) {
						if (Biomes[i].BiomeID == regionData.r) {
								biome = Biomes[i];
								break;
						}
				}
				if (biome == null) {
						Debug.Log("Couldn't find biome " + regionData.r.ToString() + ", using default");
						biome = CurrentBiome;
				}
				return biome.StatusTempAverage;
		}
		//returns a raw temperature based on time of day, time of year and elevation
		//this is not modified by civilization or anything 'man-made'
		public TemperatureRange StatusTemperature(Vector3 worldPosition, TimeOfDay timeOfDay, TimeOfYear timeOfYear)
		{		
				Biome biome = null;
				BiomeStatusTemps currentSeason = null;
				//get the region data for this position and look up the biome
				Color32 regionData = RegionDataAtPosition(worldPosition);
				//TODO maybe put this in a lookup? less than 10 biomes, probably doesn't matter
				for (int i = 0; i < Biomes.Count; i++) {
						if (Biomes[i].BiomeID == regionData.r) {
								biome = Biomes[i];
								break;
						}
				}
				if (biome == null) {
						Debug.Log("Couldn't find biome " + regionData.r.ToString() + ", using default");
						biome = CurrentBiome;
				}
				//now look up the temperature range for this time of year / this time of day
				if (Flags.Check((uint)timeOfYear, (uint)TimeOfYear.SeasonWinter, Flags.CheckType.MatchAny)) {
						currentSeason = biome.StatusTempsWinter;
				} else if (Flags.Check((uint)timeOfYear, (uint)TimeOfYear.SeasonSpring, Flags.CheckType.MatchAny)) {
						currentSeason = biome.StatusTempsSpring;
				} else if (Flags.Check((uint)timeOfYear, (uint)TimeOfYear.SeasonSummer, Flags.CheckType.MatchAny)) {
						currentSeason = biome.StatusTempsSummer;
				} else {
						currentSeason = biome.StatusTempsAutumn;
				}
				if (Flags.Check((uint)timeOfDay, (uint)TimeOfDay.ca_QuarterMorning, Flags.CheckType.MatchAny)) {
						return currentSeason.StatusTempQuarterMorning;
				} else if (Flags.Check((uint)timeOfDay, (uint)TimeOfDay.cb_QuarterAfternoon, Flags.CheckType.MatchAny)) {
						return currentSeason.StatusTempQuarterAfternoon;
				} else if (Flags.Check((uint)timeOfDay, (uint)TimeOfDay.cc_QuarterEvening, Flags.CheckType.MatchAny)) {
						return currentSeason.StatusTempQuarterEvening;
				} else {
						return currentSeason.StatusTempQuarterNight;
				}
		}
		//helper functions, usually i just cast to (int) but i'm keeping them around just in case
		public static TemperatureComparison CompareTemperatures(TemperatureRange temp1, TemperatureRange temp2)
		{
				int temp1Int = (int)temp1;
				int temp2Int = (int)temp2;
				if (temp1Int == temp2Int) {
						return TemperatureComparison.Same;
				} else if (temp1Int > temp2Int) {
						return TemperatureComparison.Warmer;
				} else {
						return TemperatureComparison.Colder;
				}
		}

		public static bool IsColderThan(TemperatureRange temp1, TemperatureRange temp2)
		{
				return ((int)temp1 < ((int)temp2));
		}

		public static bool IsHotterThan(TemperatureRange temp1, TemperatureRange temp2)
		{
				return ((int)temp1 > ((int)temp2));

		}

		public static float TemperatureRangeToFloat(TemperatureRange temp)
		{
				switch (temp) {
						case TemperatureRange.A_DeadlyCold:
						default:
								return 0.05f;

						case TemperatureRange.B_Cold:
								return 0.25f;

						case TemperatureRange.C_Warm:
								return 0.5f;

						case TemperatureRange.D_Hot:
								return 0.75f;

						case TemperatureRange.E_DeadlyHot:
								return 0.95f;
				}
		}

		public static TemperatureRange MaxTemperature(TemperatureRange temp1, TemperatureRange temp2)
		{
				if ((int)temp1 > (int)temp2) {
						return temp1;
				}
				return temp2;
		}

		public static TemperatureRange ClampTemperature(TemperatureRange temperature, TemperatureRange minTemperature, TemperatureRange maxTemperature)
		{
				return (TemperatureRange)Mathf.Clamp((int)temperature, (int)minTemperature, (int)maxTemperature);
		}

		protected TemperatureRange mTemperatureOverride = TemperatureRange.C_Warm;
		protected double mTemperatureOverrideEndTime = 0f;

		#endregion

		//search & update variables
		//keep them here to avoid allocations
		protected RaycastHit mHitGround;
		protected RaycastHit mHitOverhang;
		protected TerrainHeightSearch mTerrainHit;
		protected Vector3 mLatestPlayerPosition;
		protected Vector3 mRaycastStartPosition;
		protected Vector2 mUvCoords;
		protected Vector3 mTerrainLocalPos;
		protected Color mColorResult;
		protected Texture2D mTerrainTypeMap;
		protected Vector2 mSplatPosition;
		protected Color mSplatColor;
		protected Texture2D mSplatTexture;
		//status
		protected static int mNumChunksLoading = 0;
		protected bool mShowingAboveGround = false;
		protected bool mLoadingWorld = false;
		protected bool mUnloadingWorld = false;
		protected string mLoadingInfo = string.Empty;
		protected int mPrimaryChunkID = 0;
		protected int mPrimaryChunkIndex = 0;
		protected bool mGraphIsDirty = false;
		protected float mLastGraphUpdate = 0.0f;
		protected float mMinGraphUpdateInterval = 5.0f;
		protected bool mSaveState = false;
		protected GameObject mOcean = null;
		//lookups
		protected Dictionary <int, WorldChunk> mChunkLookup = new Dictionary <int, WorldChunk>();
		protected Dictionary <string, FlagSet> mFlagSetLookup = new Dictionary<string, FlagSet>();
}
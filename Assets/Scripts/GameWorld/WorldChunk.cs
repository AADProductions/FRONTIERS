using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.Gameplay;
using ExtensionMethods;
using System.Xml.Serialization;
using System.Text;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	[ExecuteInEditMode]
	public partial class WorldChunk : MonoBehaviour
	{
		//world chunk is where 99% of all non-worlditem 'stuff' is managed
		//rivers, scenery, terrain and so on

		#region basic properties

		public string Name {
			get {
				#if UNITY_EDITOR
				if (!Application.isPlaying) {
					return ChunkName (State);
				}
				#endif
				return mChunkName;
			}
		}

		public Vector3 ChunkOffset {
			get {
				return State.TileOffset;
			}
		}

		public Vector3 ChunkScale {
			get {
				return mChunkScale;
			}
		}

		public Bounds ChunkBounds {
			get {
				return mChunkBounds;
			}
		}

		public bool Is (ChunkMode chunkMode)
		{
			return Flags.Check ((int)chunkMode, (int)mCurrentMode, Flags.CheckType.MatchAny);
		}

		public bool HasCollider {
			get {
				return PrimaryCollider != null && PrimaryCollider.enabled;
			}
		}

		protected Vector3 mChunkScale;
		protected Bounds mChunkBounds;

		#endregion

		public WIGroup ChunkGroup = null;
		public WIGroup WorldItemsGroup;
		public WIGroup AboveGroundGroup;
		public WIGroup BelowGroundGroup;
		//data that gets saved to disk
		public ChunkState State = new ChunkState ();
		public ChunkPlantData PlantData = new ChunkPlantData ();
		public ChunkTreeData TreeData = new ChunkTreeData ();
		public ChunkNodeData NodeData = new ChunkNodeData ();
		public ChunkSceneryData SceneryData = new ChunkSceneryData ();
		public ChunkTriggerData TriggerData = new ChunkTriggerData ();
		public ChunkTerrainData TerrainData = new ChunkTerrainData ();
		//stuff that's generated at runtime
		public List <River> Rivers = new List <River> ();
		public Material PrimaryMaterial = null;
		public Terrain PrimaryTerrain = null;
		public TerrainCollider PrimaryCollider = null;
		//public Rigidbody PrimaryRigidBody = null;
		public ChunkTransforms Transforms = new ChunkTransforms ();
		//these are only for maps that are *necessarily* kept in memory when the chunk is loaded
		//'transient' maps like splat maps and detail maps are NOT kept in this list
		//ground maps are also NOT kept in this list as they may be unloaded based on distance
		//the complete list is kept in MODS
		public Dictionary <string,Texture2D> ChunkDataMaps = new Dictionary<string, Texture2D> ();
		public List <WorldTrigger> Triggers = new List <WorldTrigger> ();

		#region chunk mode

		//these are set by the game world based on distance from player
		public ChunkMode TargetMode {
			get {
				return mTargetMode;
			}
			set {
				mTargetMode = value;
				if (mTargetMode != mCurrentMode) {
					//if we're switching to a new target mode
					//add a chunk mode changer and set its target
					gameObject.GetOrAdd <ChunkModeChanger> ();
				}
			}
		}

		public ChunkMode CurrentMode { get { return mCurrentMode; } set { mCurrentMode = value; } }
		//a lot of these can be removed now that we're loading all trees / details immediately
		public bool HasColliderTemplates {
			get {
				return ColliderTemplates != null && ColliderTemplates.Length > 0;
			}
		}

		public bool HasLoadedTerrainDetails {
			get {
				return mHasLoadedTerrainDetails;
			}
		}

		public bool HasAddedTerrainTrees {
			get {
				return mHasAddedTerrainTrees;
			}
		}

		public bool GroupsUnloaded {
			get {
				if (AboveGroundGroup != null && BelowGroundGroup != null) {
					return AboveGroundGroup.Is (WIGroupLoadState.Unloaded) && BelowGroundGroup.Is (WIGroupLoadState.Unloaded);
				}
				return true;
			}
		}

		protected bool mHasLoadedTerrainObjects;
		protected bool mHasLoadedTerrainTextures;
		protected bool mHasLoadedTerrainDetails;
		protected bool mReduceTreeVariation;
		protected bool mHasAddedTerrainTrees;
		protected bool mHasAddedRivers;
		protected bool mHasGeneratedTerrain;
		protected bool mHasLoadedTransforms;
		protected bool mHasLoadedChunkMaps;
		protected ChunkMode mTargetMode = ChunkMode.Unloaded;
		protected ChunkMode mCurrentMode = ChunkMode.Unloaded;
		protected bool mInitialized = false;
		protected string mChunkName;
		protected string mChunkDataDirectory;
		protected bool mHasLoadedChildren = false;

		#endregion

		#region tree instances & quads

		public TreeColliderTemplate[] ColliderTemplates;

		public TreeInstanceTemplate[] TreeInstances {
			get {
				if (!mInitialized) {
					if (gEmptyTreeInstances == null) {
						gEmptyTreeInstances = new TreeInstanceTemplate [0];
					}
					return gEmptyTreeInstances;
				}
				return TreeData.TreeInstances;
			}
		}

		public PlantInstanceTemplate [] PlantInstances {
			get {
				if (!mInitialized) {
					if (gEmptyPlantInstances == null) {
						gEmptyPlantInstances = new PlantInstanceTemplate [0];
					}
					return gEmptyPlantInstances;
				}
				return PlantData.PlantInstances;
			}
		}

		public QuadTree <TreeInstanceTemplate> TreeInstanceQuad = new QuadTree <TreeInstanceTemplate> (new Bounds (), 0, null);
		public QuadTree <PlantInstanceTemplate> PlantInstanceQuad = new QuadTree <PlantInstanceTemplate> (new Bounds (), 0, null);
		public QuadTree <PathMarkerInstanceTemplate> PathMarkerInstanceQuad = new QuadTree <PathMarkerInstanceTemplate> (new Bounds (), 0, null);

		#endregion

		public bool Initialized {
			get {
				return mInitialized;
			}
		}

		public bool HasPrimaryTerrain {
			get {
				return PrimaryTerrain != null;
			}
		}
		//used to display what's up on loading screen via GameWorld
		public string LoadingInfo = string.Empty;

		#region initialization

		public void Awake ()
		{
			LoadChunkTransforms (false);
			mCurrentMode = ChunkMode.Unloaded;
			mTargetMode = ChunkMode.Unloaded;
		}

		public void Initialize ()
		{	//this just creates a basic chunk object
			if (mInitialized)
				return;

			mChunkName = ChunkName (State);
			mChunkDataDirectory = ChunkDataDirectory (mChunkName);
			gameObject.name = mChunkName;
			//initialize assumes that the chunk state has been loaded
			transform.position = State.TileOffset;
			ChunkGroup = WIGroups.GetOrAdd (gameObject, mChunkName, WIGroups.Get.World, null);
			ChunkGroup.Props.IgnoreOnSave = true;

			//set this so we know if we're supposed to substitute tree instances
			mReduceTreeVariation = Profile.Get.CurrentPreferences.Video.TerrainReduceTreeVariation;

			Mods.Get.Runtime.LoadMod <ChunkTriggerData> (ref TriggerData, mChunkDataDirectory, "Triggers");
			Mods.Get.Runtime.LoadMod <ChunkNodeData> (ref NodeData, mChunkDataDirectory, "Nodes");
			Mods.Get.Runtime.LoadMod <ChunkSceneryData> (ref SceneryData, mChunkDataDirectory, "Scenery");
			Mods.Get.Runtime.LoadMod <ChunkTerrainData> (ref TerrainData, mChunkDataDirectory, "Terrain");

			for (int i = 0; i < SceneryData.AboveGround.RiverNames.Count; i++) {
				//Debug.Log("Loading river " + SceneryData.AboveGround.RiverNames[i]);
				River river = null;
				if (Mods.Get.Runtime.LoadMod <River> (ref river, "River", SceneryData.AboveGround.RiverNames [i])) {
					Rivers.Add (river);
				}
			}

			Vector3 boundsSize = Vector3.zero;
			Vector3 boundsOffset = Vector3.zero;
			boundsSize = new Vector3 (State.SizeX, Globals.ChunkMaximumYBounds, State.SizeZ);
			boundsOffset = transform.position + Vector3.one * (boundsSize.x / 2);
			boundsOffset.y = State.YOffset;
			mChunkBounds = new Bounds (boundsOffset, boundsSize);

			mChunkScale.Set (State.SizeX, Globals.ChunkMaximumYBounds, State.SizeZ);

			//load tree data
			if (Mods.Get.Runtime.LoadMod <ChunkTreeData> (ref TreeData, mChunkDataDirectory, "Trees")) {
				//update our tree instances with our offset and create our quad tree
				//make sure not to use the TreeInstances convenience property
				for (int i = 0; i < TreeData.TreeInstances.Length; i++) {
					TreeInstanceTemplate tit = TreeData.TreeInstances [i];
					tit.ParentChunk = this;
					tit.ChunkOffset = ChunkOffset;
					tit.ChunkScale = ChunkScale;
				}
				TreeInstanceQuad = new QuadTree <TreeInstanceTemplate> (
					ChunkBounds,
					Math.Max (TreeInstances.Length / QuadTreeMaxContentScaler, QuadTreeMaxContentMinimum),
					TreeData.TreeInstances);
			}

			//load plant data
			//make sure not to use the PlantInstances convenience property
			//it will return an empty array
			if (Mods.Get.Runtime.LoadMod <ChunkPlantData> (ref PlantData, mChunkDataDirectory, "Plants")) {
				for (int i = 0; i < PlantData.PlantInstances.Length; i++) {
					PlantInstanceTemplate pit = PlantData.PlantInstances [i];
					pit.HasInstance = false;
					pit.ChunkOffset = ChunkOffset;
					pit.ChunkScale = ChunkScale;
					pit.ParentChunk = this;
				}
				PlantInstanceQuad = new QuadTree <PlantInstanceTemplate> (
					ChunkBounds,
					Math.Max (PlantData.PlantInstances.Length / QuadTreeMaxContentScaler, QuadTreeMaxContentMinimum),
					PlantData.PlantInstances);
			}

			//Dictionary <string,Texture2D> matChunkMaps = new Dictionary <string, Texture2D> ();
			for (int groundIndex = 0; groundIndex < TerrainData.TextureTemplates.Count; groundIndex++) {
				TerrainTextureTemplate ttt = TerrainData.TextureTemplates [groundIndex];
				Texture2D Diffuse = null;
				if (Mats.Get.GetTerrainGroundTexture (ttt.DiffuseName, out Diffuse)) {
					ChunkDataMaps.Add ("Ground" + groundIndex.ToString (), Diffuse);
				}
			}
			Texture2D chunkMap = null;
			//Debug.Log ("Getting terrain color overlay in " + Name);
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "ColorOverlay")) {
				ChunkDataMaps.Add ("ColorOverlay", chunkMap);
			}
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "AboveGroundTerrainType")) {
				ChunkDataMaps.Add ("AboveGroundTerrainType", chunkMap);
			}
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "BelowGroundTerrainType")) {
				ChunkDataMaps.Add ("BelowGroundTerrainType", chunkMap);
			}
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "RegionData")) {
				ChunkDataMaps.Add ("RegionData", chunkMap);
			}
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "Splat1")) {
				ChunkDataMaps.Add ("Splat1", chunkMap);
			}
			if (Mods.Get.Runtime.ChunkMap (ref chunkMap, Name, "Splat2")) {
				ChunkDataMaps.Add ("Splat2", chunkMap);
			}

			//now start coroutines that load the nodes
			CreateNodesAndTriggers ();

			mInitialized = true;
		}

		public void LoadChunkTransforms (bool editor)
		{
			if (mHasLoadedTransforms && !editor) {
				return;
			}

			transform.name = Name;

			Transforms.Plants = gameObject.FindOrCreateChild ("PL");
			Transforms.Terrain = gameObject.FindOrCreateChild ("TR");
			Transforms.WorldItems = gameObject.FindOrCreateChild ("WI");
			Transforms.Nodes = gameObject.FindOrCreateChild ("NODES");
			Transforms.Triggers = gameObject.FindOrCreateChild ("TRIGGERS");

			Transforms.AboveGround = Transforms.Terrain.gameObject.FindOrCreateChild ("AG");
			Transforms.BelowGround = Transforms.Terrain.gameObject.FindOrCreateChild ("BG");

			Transforms.AboveGroundStaticImmediate = Transforms.AboveGround.gameObject.FindOrCreateChild ("ST");
			Transforms.AboveGroundStaticAdjascent = Transforms.AboveGround.gameObject.FindOrCreateChild ("ST_ADJ");
			Transforms.AboveGroundStaticDistant = Transforms.AboveGround.gameObject.FindOrCreateChild ("ST_DST");
			Transforms.AboveGroundGenerated = Transforms.AboveGround.gameObject.FindOrCreateChild ("GN");
			Transforms.AboveGroundOcean = Transforms.AboveGround.gameObject.FindOrCreateChild ("WR");
			Transforms.AboveGroundFX = Transforms.AboveGround.gameObject.FindOrCreateChild ("FX");
			Transforms.AboveGroundAudio = Transforms.AboveGround.gameObject.FindOrCreateChild ("AU");
			Transforms.AboveGroundRivers = Transforms.AboveGround.gameObject.FindOrCreateChild ("RV");

			Transforms.AboveGroundWorldItems = Transforms.WorldItems.gameObject.FindOrCreateChild ("AG");
			Transforms.BelowGroundWorldItems = Transforms.WorldItems.gameObject.FindOrCreateChild ("BG");

			Transforms.BelowGroundRivers = Transforms.BelowGround.gameObject.FindOrCreateChild ("RV");
			Transforms.BelowGroundStatic = Transforms.BelowGround.gameObject.FindOrCreateChild ("ST");
			Transforms.BelowGroundGenerated = Transforms.BelowGround.gameObject.FindOrCreateChild ("GN");
			Transforms.BelowGroundFX = Transforms.BelowGround.gameObject.FindOrCreateChild ("FX");
			Transforms.BelowGroundAudio = Transforms.BelowGround.gameObject.FindOrCreateChild ("AU");

			Transforms.Paths = gameObject.FindOrCreateChild ("PATHS");

			mHasLoadedTransforms = false;
		}

		#endregion

		#region generation

		public IEnumerator GenerateTerrain (Terrain newPrimaryTerrain, bool showAboveGround)
		{
			PrimaryTerrain = newPrimaryTerrain;
			PrimaryTerrain.transform.position = State.TileOffset;
			ShowAboveGround (showAboveGround);//TODO necessary?

			if (Mods.Get.Runtime.TerrainHeights (gHeights, TerrainData.HeightmapResolution, Name, 0)) {
				PrimaryTerrain.terrainData.SetHeights (0, 0, gHeights);
				PrimaryTerrain.terrainData.size = new Vector3 (State.SizeX, TerrainData.HeightmapHeight, State.SizeZ);
			}
			//clear the heights, they're in the terrain now
			Array.Clear (gHeights, 0, gHeights.GetLength (0) * gHeights.GetLength (1));
			PrimaryTerrain.enabled = true;
			PrimaryCollider = PrimaryTerrain.GetComponent <TerrainCollider> ();
			//PrimaryRigidBody = PrimaryTerrain.GetComponent <Rigidbody>();
			if (PrimaryCollider.terrainData != PrimaryTerrain.terrainData) {
				PrimaryCollider.terrainData = PrimaryTerrain.terrainData;
			}
			//PrimaryRigidBody.detectCollisions = true;
			PrimaryCollider.enabled = true;
			PrimaryMaterial = PrimaryTerrain.materialTemplate;
			//PrimaryMaterial = PrimaryTerrain.GetComponent <Renderer> ().sharedMaterial;
			PrimaryTerrain.Flush ();
			yield return null;
			//give the terrain a second to breathe
			LoadChunkTransforms (false);
			var loadChunkGroups = LoadChunkGroups ();
			while (loadChunkGroups.MoveNext ()) {
				yield return loadChunkGroups.Current;
			}
			mHasGeneratedTerrain = true;
			yield break;
		}

		public IEnumerator RefreshTerrainTextures ()
		{
			//Debug.Log("Refreshing terrain textures");
			try {
				PrimaryMaterial.name = Name + " Material";
				TerrainData.MaterialSettings.ApplySettings (PrimaryMaterial);
				TerrainData.MaterialSettings.ApplyMaps (PrimaryMaterial, Name, ChunkDataMaps);
			} catch (Exception e) {
				Debug.Log ("Error while refreshing terrain textures, proceeding normally: " + e.ToString ());
			}
			yield break;
		}

		public IEnumerator RefreshTerrainObjects ()
		{
			if (PrimaryTerrain == null) {
				//whoops, no big deal
				yield break;
			}
			DetailPrototype[] detailPrototypes = null;
			if (VRManager.VRMode && Profile.Get.CurrentPreferences.Video.VRDisableExtraGrassLayers) {
				Debug.Log ("VR mode and disable extra grass layers");
				//don't add any grass
				//TODO add one layer of default grass specified in WorldChunk
				detailPrototypes = new DetailPrototype [0];
			} else {
				//replace existing prototypes with new ones
				int numDetailLayers = Frontiers.Profile.Get.CurrentPreferences.Video.TerrainMaxDetailLayers;
				detailPrototypes = new DetailPrototype [numDetailLayers];
				for (int i = 0; i < numDetailLayers; i++) {
					if (i < TerrainData.DetailTemplates.Count) {
						TerrainPrototypeTemplate tpt = TerrainData.DetailTemplates [i];
						DetailPrototype detailPrototype = new DetailPrototype ();
						detailPrototype.bendFactor = tpt.BendFactor;
						detailPrototype.dryColor = tpt.DryColor;
						detailPrototype.healthyColor = tpt.HealthyColor;
						detailPrototype.maxHeight = tpt.MaxHeight;
						detailPrototype.maxWidth = tpt.MaxWidth;
						detailPrototype.minHeight = tpt.MinHeight;
						detailPrototype.maxHeight = tpt.MaxHeight;
						detailPrototype.noiseSpread = tpt.NoiseSpread;
						detailPrototype.renderMode = tpt.RenderMode;
						detailPrototype.usePrototypeMesh = tpt.UsePrototypeMesh;

						if (detailPrototype.usePrototypeMesh) {	//load a gameobject prototype
							GameObject prototype = null;
							if (Plants.Get.GetTerrainPlantPrototype (tpt.AssetName, out prototype)) {
								detailPrototype.prototype = prototype;
								if (tpt.AssetName.Contains ("Static_")) {
									//static objects aren't affected by wind
									//and they always use the grass render mode
									detailPrototype.dryColor = Colors.Alpha (tpt.DryColor, 0f);
									detailPrototype.healthyColor = Colors.Alpha (tpt.HealthyColor, 0f);
									detailPrototype.renderMode = DetailRenderMode.Grass;
								}
							}
						} else {//just load a texture
							Texture2D grassTexture = null;
							if (Mats.Get.GetTerrainGrassTexture (tpt.AssetName, out grassTexture)) {
								detailPrototype.prototypeTexture = grassTexture;
								detailPrototype.renderMode = DetailRenderMode.Grass;//never use billboard
							}
						}
						//set the new prototype
						detailPrototypes [i] = detailPrototype;
					} else {
						//pad out the detail prototypes so we always end up with max layers
						detailPrototypes [i] = Plants.Get.DefaultDetailPrototype;
					}
					yield return null;
				}
			}

			if (!GameManager.Get.NoTreesMode) {
				TreePrototype[] treePrototypes = null;
				if (ColliderTemplates != null) {
					Array.Clear (ColliderTemplates, 0, ColliderTemplates.Length);
					ColliderTemplates = null;
				}
				//Debug.Log("Getting tree prototypes for " + Name);
				Plants.Get.GetTerrainPlantPrototypes (TerrainData.TreeTemplates, TreeData.TreeInstances, ref treePrototypes, ref ColliderTemplates);
				PrimaryTerrain.terrainData.treePrototypes = treePrototypes;
			}

			PrimaryTerrain.terrainData.detailPrototypes = detailPrototypes;
			//PrimaryTerrain.terrainData.RefreshPrototypes();
			PrimaryTerrain.Flush ();

			mHasLoadedTerrainObjects = true;
			yield break;
		}

		public IEnumerator AddTerainFX (ChunkMode mode)
		{
			//this has been slowly broken up and moved into worlditems
			//eg waterfalls
			//but particle effects are making a comeback! so i'm keeping it around for later
			yield break;
		}

		public IEnumerator AddTerrainDetails (ChunkMode targetMode)
		{
			if (VRManager.VRMode && Profile.Get.CurrentPreferences.Video.VRDisableExtraGrassLayers) {
				Debug.Log ("Skipping terrain details because we're in vr mode and we're skipping extra grass layers in " + mChunkName);
				//TODO add 1 grass layer
				mAddingTerrainDetails = false;
				mHasLoadedTerrainDetails = true;
				yield break;
			}

			//whoops got called twice, no big deal
			if (mAddingTerrainDetails) {
				Debug.Log ("Already adding terrain details in " + mChunkName + ", quitting");
				yield break;
			}

			mAddingTerrainDetails = true;

			if (mDetailSlice == null) {
				mDetailSlice = new int [Globals.WorldChunkDetailSliceResolution, Globals.WorldChunkDetailSliceResolution];
			}
			if (mFilledSlices == null) {
				mFilledSlices = new HashSet <string> ();
			}

			int numDetailLayers = PrimaryTerrain.terrainData.detailPrototypes.Length;
			int numSlices = Globals.WorldChunkDetailResolution / Globals.WorldChunkDetailSliceResolution;
			int xOffset = 0;
			int yOffset = 0;
			float waitTime = 0f;
			bool needToClearArray = true;//always clear it the first time
			bool needToSetData = false;
			string sliceName = string.Empty;

			for (int layerNum = 0; layerNum < numDetailLayers; layerNum++) {
				if (!HasPrimaryTerrain || mTargetMode == ChunkMode.Unloaded) {
					Debug.Log ("Stopping terrain data filling due to data changing in " + Name);
					mAddingTerrainDetails = false;
					yield break;
				}
				//Debug.Log("Attempting to load " + layerNum.ToString() + " out of " + numDetailLayers.ToString() + " in chunk " + mChunkName);
				//check to see if this is an 'empty' layer
				if (PrimaryTerrain.terrainData.detailPrototypes [layerNum] == Plants.Get.DefaultDetailPrototype) {
					//if it is, skip it
					//Debug.Log("Skipping default detail prototype layer " + layerNum.ToString() + " in chunk " + mChunkName);
					continue;
				}
				//if it's a legit layer
				//get the slices for this layer
				for (int xs = 0; xs < numSlices; xs++) {
					if (!HasPrimaryTerrain || mTargetMode == ChunkMode.Unloaded) {
						Debug.Log ("Stopping terrain data filling due to data changing in " + Name);
						mAddingTerrainDetails = false;
						yield break;
					}

					for (int ys = 0; ys < numSlices; ys++) {
						//are we still trying to load?
						if (!HasPrimaryTerrain || mTargetMode == ChunkMode.Unloaded) {
							Debug.Log ("Stopping terrain data filling due to data changing in " + Name);
							mAddingTerrainDetails = false;
							yield break;
						}

						xOffset = xs * Globals.WorldChunkDetailSliceResolution;
						yOffset = ys * Globals.WorldChunkDetailSliceResolution;
						sliceName = SliceName (layerNum, xs, ys);
						if (!Mods.Get.Runtime.LoadTerrainDetailSlice (mDetailSlice, ChunkDataDirectory (Name), sliceName)) {
							//if it doesn't we need to make sure this spot is empty
							//have we filled this spot before?
							if (mFilledSlices.Remove (sliceName)) {
								//we have! it's been removed now so we'll know it's empty next time
								needToSetData = true;
							}
							if (needToClearArray) {
								//if we have to, get rid of the data already in the array
								Array.Clear (mDetailSlice, 0, mDetailSlice.GetLength (0) * mDetailSlice.GetLength (1));
								needToClearArray = false;
							}
						} else {
							mFilledSlices.Add (sliceName);//we'll have to erase this later so add the slice name
							needToClearArray = true;//we'll have to clear the detail slice array next time around
							needToSetData = true;//we've found a slice so we need to set it
						}
						//if we actually need to do something...
						if (needToSetData) {
							//Debug.Log("Setting detail layer section in " + layerNum.ToString() + " in " + Name);
							//now set the detail in the terrain
							//invert the x and y offset
							//why? either it's some commonly understood array slicing thing
							//or else unity is just fucking with me
							//i don't know take your pick
							PrimaryTerrain.terrainData.SetDetailLayer (yOffset, xOffset, layerNum, mDetailSlice);
							if (Player.Local.HasSpawned) {
								if (mTargetMode == ChunkMode.Distant) {
									waitTime = 0.15f;
								} else if (mTargetMode == ChunkMode.Adjascent) {
									waitTime = 0.1f;
								} else if (mTargetMode != ChunkMode.Primary) {
									waitTime = 0.05f;
								} else {
									waitTime = 0.005f;
								}
								double start = Frontiers.WorldClock.RealTime;
								while (Frontiers.WorldClock.RealTime < start + waitTime) {
									yield return null;
								}
							} else {
								yield return null;
							}
						}
					}
				}
			}

			//this gobbles up a lot of memory
			//this is probably where the prologue crash is hitting hardest
			PrimaryTerrain.Flush ();
			mAddingTerrainDetails = false;
			mHasLoadedTerrainDetails = true;
			yield break;
		}

		public void SetReducedTreeVariation (bool reduceTreeVariation)
		{
			if (mReduceTreeVariation != reduceTreeVariation) {

			}
			mReduceTreeVariation = reduceTreeVariation;
		}

		public IEnumerator AddTerrainTrees ()
		{
			if (GameManager.Get.NoTreesMode) {
				mHasAddedTerrainTrees = true;
				yield break;
			}

			//sort trees into groups
			int numPrototypes = TerrainData.TreeTemplates.Count;// PrimaryTerrain.terrainData.treePrototypes.Length;
			int numAddedThisFrame = 0;
			TreeInstanceTemplate[] trees = TreeData.TreeInstances;
			/*List <TreeInstance> treeInstances = new List<TreeInstance>();
			int numTreesAdded = 0;
			for (int i = 0; i < trees.Length; i++) {
				treeInstances.Add(trees[i].ToInstance);
				if (numTreesAdded > 50) {
					numTreesAdded = 0;
					yield return null;
				}
				numTreesAdded++;
			}
			yield return null;
			PrimaryTerrain.terrainData.treeInstances = treeInstances.ToArray ();
			treeInstances.Clear();*/
			//for (int p = 0; p < numPrototypes; p++) {
			for (int i = 0; i < trees.Length; i++) {
				//if (trees[i].FinalPrototypeIndex == p) {
				//unfortunately this all has to be done in ONE frame
				//otherwise unity's terrain will crash
				PrimaryTerrain.AddTreeInstance (TreeInstances [i].ToInstance);
				//}
				numAddedThisFrame++;
				if (numAddedThisFrame > 100) {
					if (mTargetMode == ChunkMode.Unloaded) {
						Debug.Log ("Chunk is unloaded, skipping adding terrain trees");
						yield break;
					}
					if (Player.Local.HasSpawned && mTargetMode != ChunkMode.Primary) {
						PrimaryTerrain.Flush ();
						numAddedThisFrame = 0;
						yield return null;
					}
				}
			}
			//after we've added all trees of one type
			//take a break
			//}
			//flush this right away since other stuff will take a while to add
			PrimaryTerrain.Flush ();
			mHasAddedTerrainTrees = true;
			yield return null;
			yield break;
		}

		protected static bool gAddingTreesToChunk = false;

		public IEnumerator AddRivers (ChunkMode targetMode)
		{
			for (int i = 0; i < Rivers.Count; i++) {
				River river = Rivers [i];
				if (river.river == null) {
					CreateRiver (river);
				}
				if (Player.Local.HasSpawned && !Player.Local.Surroundings.IsOutside) {
					//give it a sec
					double waitUntil = WorldClock.RealTime + 0.125f + UnityEngine.Random.value;
					while (WorldClock.RealTime < waitUntil) {
						yield return null;
					}
				}
			}
			yield break;
		}

		protected void CreateRiver (River river)
		{
			Transform agRivers = Transforms.AboveGroundRivers;
			GameObject riverAvatarObject = null;
			if (river.DynamicMode) {
				riverAvatarObject = GameObject.Instantiate (GameWorld.Get.RiverPrefabDynamic, Transforms.AboveGroundRivers.position, Quaternion.identity) as GameObject;
			} else {
				riverAvatarObject = GameObject.Instantiate (GameWorld.Get.RiverPrefabStatic, Transforms.AboveGroundRivers.position, Quaternion.identity) as GameObject;
			}
			riverAvatarObject.transform.parent = Transforms.AboveGroundRivers;
			RiverAvatar riverAvatar = riverAvatarObject.GetComponent <RiverAvatar> ();
			river.river = riverAvatar;
			riverAvatar.ParentChunk = this;
			riverAvatar.Props = river;
			riverAvatar.RefreshProps ();
		}

		public void CreateNodesAndTriggers ()
		{
			foreach (KeyValuePair <string, KeyValuePair <string, string>> triggerStatePair in TriggerData.TriggerStates) {
				AddTrigger (triggerStatePair, Transforms.Triggers, false);
			}

			foreach (KeyValuePair <string, List <ActionNodeState>> actionNodeStateList in NodeData.NodeStates) {
				for (int i = 0; i < actionNodeStateList.Value.Count; i++) {
					ActionNodeState actionNodeState = actionNodeStateList.Value [i];
					GameObject newNodeGameObject = Transforms.Nodes.gameObject.FindOrCreateChild (actionNodeState.FullName).gameObject;
					ActionNode newNode = newNodeGameObject.GetOrAdd <ActionNode> ();
					newNode.State = actionNodeState;
					newNode.State.Transform.ApplyTo (newNode.transform);
					newNode.State.actionNode = newNode;
					newNode.State.ParentGroupPath = actionNodeStateList.Key;
				}
			}
		}

		public ActionNode SpawnActionNode (WIGroup group, ActionNodeState actionNodeState, Transform nodeParentTransform)
		{
			ActionNode actionNode = null;
			if (!actionNodeState.IsLoaded) {
				GameObject newNodeGameObject = nodeParentTransform.gameObject.CreateChild (actionNodeState.FullName).gameObject;
				actionNode = newNodeGameObject.AddComponent <ActionNode> ();
				actionNode.State = actionNodeState;
				//since we're spawning this in the chunk we have to apply the group's transforms to the node
				actionNode.State.Transform.ApplyTo (actionNode.transform);
				//now move it to the final point
				actionNode.transform.parent = Transforms.Nodes;
				actionNode.State.actionNode = actionNode;
			} else {
				actionNode = actionNodeState.actionNode;
			}
			//add the action node state to the lookup
			actionNodeState.ParentGroupPath = group.Path;
			List <ActionNodeState> nodeStates = null;
			if (!NodeData.NodeStates.TryGetValue (group.Path, out nodeStates)) {
				nodeStates = new List<ActionNodeState> ();
				NodeData.NodeStates.Add (group.Path, nodeStates);
			}
			nodeStates.SafeAdd (actionNodeState);
			return actionNode;
		}

		public List <ActionNode> AddNodesToGroup (List <ActionNodeState> actionNodeStates, WIGroup group, Transform nodeParentTransform)
		{
			List <ActionNode> actionNodes = new List<ActionNode> ();
			List <ActionNodeState> nodeStates = null;
			for (int i = 0; i < actionNodeStates.Count; i++) {
				actionNodes.Add (SpawnActionNode (group, actionNodeStates [i], nodeParentTransform));
			}
			return actionNodes;
		}

		public void AddTrigger (KeyValuePair <string, KeyValuePair<string, string>> triggerStatePair, Transform triggerParentTransform, bool addToTriggerStates)
		{
			string triggerName = triggerStatePair.Key;
			string triggerScriptName = triggerStatePair.Value.Key;
			string triggerState = triggerStatePair.Value.Value;

			GameObject newTriggerObject = triggerParentTransform.gameObject.CreateChild (triggerName).gameObject;
			WorldTrigger worldTriggerScript	= newTriggerObject.AddComponent (triggerScriptName) as WorldTrigger;
			worldTriggerScript.UpdateTriggerState (triggerState, this);
			newTriggerObject.transform.parent = Transforms.Triggers;
			//this will update its local transform so the next time it loads it'll be in the right spot
			worldTriggerScript.RefreshTransform ();
			Triggers.SafeAdd (worldTriggerScript);
		}

		public void ShowAboveGround (bool show)
		{
			if (HasPrimaryTerrain) {
				if (show) {
					PrimaryTerrain.gameObject.layer = Globals.LayerNumSolidTerrain;
				} else {
					PrimaryTerrain.gameObject.layer = Globals.LayerNumHidden;
				}
				//don't enable or disable the collider this causes a huge physics lurch
			}

			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
				ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabs [i];
				if (chunkPrefab.IsLoaded) {
					chunkPrefab.LoadedObject.ShowAboveGround (show);
				}
			}

			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabsAdjascent.Count; i++) {
				ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabsAdjascent [i];
				if (chunkPrefab.IsLoaded) {
					chunkPrefab.LoadedObject.ShowAboveGround (show);
				}
			}

			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabsDistant.Count; i++) {
				ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabsDistant [i];
				if (chunkPrefab.IsLoaded) {
					chunkPrefab.LoadedObject.ShowAboveGround (show);
				}
			}

			for (int i = 0; i < SceneryData.BelowGround.SolidTerrainPrefabs.Count; i++) {
				//Debug.Log ("Showing below ground prefab " + SceneryData.BelowGround.SolidTerrainPrefabs [i].Name);
				ChunkPrefab chunkPrefab = SceneryData.BelowGround.SolidTerrainPrefabs [i];
				if (chunkPrefab.IsLoaded) {
					chunkPrefab.LoadedObject.ShowAboveGround (show);
				}
			}

		}

		protected bool mAddingTerrainDetails = false;

		#endregion

		#region load / save / unload

		protected Vector3[] mHeightMapVertices = null;

		public void OnGameUnload ()
		{
			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
				Structures.UnloadChunkPrefab (SceneryData.AboveGround.SolidTerrainPrefabs [i]);
			}
			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
				Structures.UnloadChunkPrefab (SceneryData.AboveGround.SolidTerrainPrefabsAdjascent [i]);
			}
			for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
				Structures.UnloadChunkPrefab (SceneryData.AboveGround.SolidTerrainPrefabsDistant [i]);
			}
		}

		public void OnGameSave ()
		{
			//Debug.Log("Saving triggers, nodes and plants in chunk");
			SaveTriggers ();
			Mods.Get.Runtime.SaveMod <ChunkNodeData> (NodeData, mChunkDataDirectory, "Nodes");
			Mods.Get.Runtime.SaveMod <ChunkPlantData> (PlantData, mChunkDataDirectory, "Plants");
		}

		public void SaveTriggers ()
		{
			TriggerData.TriggerStates.Clear ();
			string triggerState = null;
			for (int i = 0; i < Triggers.Count; i++) {
				if (Triggers [i].GetTriggerState (out triggerState)) {
					if (!TriggerData.TriggerStates.ContainsKey (Triggers [i].name)) {
						TriggerData.TriggerStates.Add (Triggers [i].name, new KeyValuePair <string, string> (Triggers [i].ScriptName, triggerState));
					} else {
						Debug.LogError ("ERROR: Attempting to save the same trigger twice: " + Triggers [i].name);
					}
				}
			}
			Mods.Get.Runtime.SaveMod <ChunkTriggerData> (TriggerData, mChunkDataDirectory, "Triggers");
		}

		public IEnumerator LoadChunkGroups ()
		{
			ChunkGroup.Load ();

			while (!ChunkGroup.Is (WIGroupLoadState.Loaded)) {
				//be patient
				yield return null;
			}

			if (WorldItemsGroup == null) {
				WorldItemsGroup = WIGroups.GetOrAdd (Transforms.WorldItems.gameObject, Transforms.WorldItems.name, ChunkGroup, null);
			}

			WorldItemsGroup.Load ();

			while (!WorldItemsGroup.Is (WIGroupLoadState.Loaded)) {
				//be patient
				yield return null;
			}

			if (AboveGroundGroup == null) {
				AboveGroundGroup = WIGroups.GetOrAdd (Transforms.AboveGroundWorldItems.gameObject, Transforms.AboveGroundWorldItems.name, WorldItemsGroup, null);
				AboveGroundGroup.Props.IgnoreOnSave = true;
				AboveGroundGroup.Props.TerrainType = LocationTerrainType.AboveGround;
			}

			if (BelowGroundGroup == null) {
				BelowGroundGroup = WIGroups.GetOrAdd (Transforms.BelowGroundWorldItems.gameObject, Transforms.BelowGroundWorldItems.name, WorldItemsGroup, null);
				BelowGroundGroup.Props.IgnoreOnSave = true;
				BelowGroundGroup.Props.TerrainType = LocationTerrainType.BelowGround;
			}

			AboveGroundGroup.Load ();
			BelowGroundGroup.Load ();
			yield break;
		}

		public void Refresh ()
		{
			if (HasPrimaryTerrain) {
				//PrimaryRigidBody.detectCollisions = true;
				PrimaryCollider.enabled = true;
			}
		}

		public void SavePlants ()
		{
			Mods.Get.Runtime.SaveMod <ChunkPlantData> (PlantData, mChunkDataDirectory, "Plants");
		}

		public void UnloadTerrain ()
		{
			PrimaryCollider = null;
			//PrimaryRigidBody = null;
			PrimaryMaterial = null;
			if (HasPrimaryTerrain) {
				RemoveTerrainPrototypes ();
				GameWorld.Get.ReleaseTerain (PrimaryTerrain);
			}
			PrimaryTerrain = null;
			if (mDetailSlice != null) {
				Array.Clear (mDetailSlice, 0, mDetailSlice.GetLength (0) * mDetailSlice.GetLength (1));
				mDetailSlice = null;
			}
			mHasAddedTerrainTrees = false;
			mHasLoadedTerrainDetails = false;
			//try to reclaim our detail layer memory
			//System.GC.Collect();
			//Resources.UnloadUnusedAssets();
			//mHasAddedRivers = false; keep rivers around indefinitely?
		}

		public void RemoveTerrainPrototypes ()
		{
			//clear all the crap from the prototypes and such
			PrimaryTerrain.terrainData.treeInstances = new TreeInstance [0];
			PrimaryTerrain.terrainData.treePrototypes = new TreePrototype [0];
			//never clear this - it'll cause a huge lurch while we change the number of active detail layers
			DetailPrototype [] detailPrototypes = PrimaryTerrain.terrainData.detailPrototypes;
			for (int i = 0; i < detailPrototypes.Length; i++) {
				detailPrototypes [i] = Plants.Get.DefaultDetailPrototype;
			}
			PrimaryTerrain.terrainData.detailPrototypes = detailPrototypes;
			PrimaryTerrain.terrainData.RefreshPrototypes ();
		}

		public IEnumerator ClearTransforms ()
		{
			/*
						DestroyChildren (Transforms.AboveGroundStaticImmediate);
						DestroyChildren (Transforms.AboveGroundGenerated);
						DestroyChildren (Transforms.AboveGroundOcean);
						DestroyChildren (Transforms.AboveGroundFX);
						DestroyChildren (Transforms.AboveGroundAudio);

						DestroyChildren (Transforms.AboveGroundWorldItems);
						DestroyChildren (Transforms.BelowGroundWorldItems);

						DestroyChildren (Transforms.BelowGroundStatic);
						DestroyChildren (Transforms.BelowGroundGenerated);
						DestroyChildren (Transforms.BelowGroundFX);
						DestroyChildren (Transforms.BelowGroundAudio);
						*/
			yield break;
		}

		#endregion

		#region action node & trigger add/search

		public bool GetTriggerState (string triggerName, out WorldTriggerState worldTriggerState)
		{
			worldTriggerState = null;
			for (int i = 0; i < Triggers.Count; i++) {
				WorldTrigger trigger = Triggers [i];
				if (trigger.BaseState.Name.Equals (triggerName)) {
					worldTriggerState = trigger.BaseState;
					break;
				}
			}
			return worldTriggerState != null;
		}

		public bool GetNodes (List <string> actionNodeNames, bool skipReserved, List <ActionNodeState> nodeStates)
		{
			ActionNodeState nodeState = null;
			bool foundAtLeastOne = false;
			for (int i = 0; i < actionNodeNames.Count; i++) {
				if (GetNode (actionNodeNames [i], skipReserved, out nodeState)) {
					nodeStates.Add (nodeState);
					foundAtLeastOne = true;
				}
			}
			return foundAtLeastOne;
		}

		public bool GetNode (string actionNodeName, bool skipReserved, out ActionNodeState nodeState)
		{
			bool foundNode = false;
			nodeState = null;
			foreach (Transform nodeTransform in Transforms.Nodes) {
				if (nodeTransform.name.Contains (actionNodeName)) {
					ActionNode actionNode = null;
					if (nodeTransform.gameObject.HasComponent <ActionNode> (out actionNode)
					         &&	actionNode.State.Name == actionNodeName) {
						if (!(skipReserved && actionNode.IsReserved)) {
							nodeState = actionNode.State;
							foundNode = true;
							break;
						}
					}
				}
			}
			return foundNode;
		}

		public bool GetRandomNodeForLocation (string groupPath, TimeOfDay timeOfDay, out ActionNodeState nodeState)
		{
			bool foundNode = false;
			nodeState = null;
			List <ActionNodeState> nodeStates = null;
			if (GetNodesForLocation (groupPath, out nodeStates)) {
				List <int> numbersTried = new List <int> ();
				//try random numbers until we have none left
				while (numbersTried.Count <= nodeStates.Count) {
					int randomIndex = UnityEngine.Random.Range (0, nodeStates.Count);
					//get another number that we haven't tried yet
					while (numbersTried.Contains (randomIndex)) {
						randomIndex = UnityEngine.Random.Range (0, nodeStates.Count);
					}
					numbersTried.Add (randomIndex);
					//check to see if the node's time of day lines up
					ActionNodeState checkState = nodeStates [randomIndex];
					if (Flags.Check ((uint)checkState.RoutineHours, (uint)timeOfDay, Flags.CheckType.MatchAny)) {
						foundNode = true;
						nodeState = checkState;
						break;
					}
				}
			}
			return foundNode;
		}

		public bool GetRandomNodeForLocation (string groupPath, Vector3 nearPoint, float range, out ActionNodeState nodeState)
		{
			bool foundNode = false;
			nodeState = null;
			List <ActionNodeState> nodeStates = null;
			if (GetNodesForLocation (groupPath, out nodeStates)) {
				List <int> numbersTried = new List <int> ();
				//try random numbers until we have none left
				while (numbersTried.Count <= nodeStates.Count) {
					int randomIndex = UnityEngine.Random.Range (0, nodeStates.Count);
					//get another number that we haven't tried yet
					while (numbersTried.Contains (randomIndex)) {
						randomIndex = UnityEngine.Random.Range (0, nodeStates.Count);
					}
					numbersTried.Add (randomIndex);
					//check to see if the node is in range
					ActionNodeState checkState = nodeStates [randomIndex];
					if (checkState.IsLoaded && Vector3.Distance (checkState.actionNode.transform.position, nearPoint) < range) {
						foundNode = true;
						nodeState = checkState;
						break;
					}
				}
			}
			return foundNode;
		}

		public bool GetNodesForLocation (string groupPath, out List <ActionNodeState> nodeStates)
		{
			return NodeData.NodeStates.TryGetValue (groupPath, out nodeStates);
		}

		public bool GetOrCreateNode (WIGroup group, Transform parentTransform, string nodeName, out ActionNodeState nodeState)
		{
			nodeState = null;
			if (GetNode (nodeName, false, out nodeState)) {
				return true;
			} else {
				nodeState = new ActionNodeState ();
				nodeState.Name = nodeName;
				SpawnActionNode (group, nodeState, parentTransform);
			}
			return nodeState != null;
		}

		public bool ContainsPoint (Vector3 point)
		{
			return ChunkBounds.Contains (point);
		}

		public bool GetRiver (string riverName, out RiverAvatar river)
		{
			river = null;
			//lol
			for (int i = 0; i < Rivers.Count; i++) {
				if (Rivers [i].Name == riverName) {
					if (Rivers [i].river == null) {
						CreateRiver (Rivers [i]);
					}
					river = Rivers [i].river;
					break;
				}
			}
			return river != null;
		}

		#endregion

		#region dungeons

		//dungeons are no longer in use
		//keeping this here in case i bring them back
		public bool GetOrCreateDungeon (string dungeonName, out Dungeon dungeon)
		{
			dungeon = null;
			GameObject dungeonObject = Transforms.BelowGroundGenerated.gameObject.FindOrCreateChild (dungeonName).gameObject;
			dungeon = dungeonObject.GetOrAdd <Dungeon> ();
			dungeon.ParentChunk = this;
			return dungeon;
		}

		#endregion

		#region static helper functions

		public static void DestroyChildren (Transform start)
		{
			if (Application.isEditor) {
				List <Transform> thingsToDestroy = new List<Transform> ();
				foreach (Transform child in start) {
					thingsToDestroy.Add (child);
				}
				foreach (Transform thingToDestroy in thingsToDestroy) {
					GameObject.DestroyImmediate (thingToDestroy.gameObject);
				}
			} else {
				foreach (Transform child in start) {
					GameObject.Destroy (start.gameObject, 0.05f);
				}
			}
		}

		public static Vector3 WorldPositionToChunkPosition (Bounds chunkBounds, Vector3 position)
		{
			position.x = position.x / chunkBounds.size.x;
			position.y = position.y / chunkBounds.size.y;
			position.z = position.z / chunkBounds.size.z;
			return position;
		}

		public static Vector3 ChunkPositionToWorldPosition (Bounds chunkBounds, Vector3 position)
		{
			position.x = (position.x * chunkBounds.size.x);
			position.y = (position.y * chunkBounds.size.y);
			position.z = (position.z * chunkBounds.size.z);
			return position;
		}

		public static string ChunkDataDirectory (string chunkName)
		{
			return System.IO.Path.Combine ("Chunk", chunkName);
		}

		public static string ChunkName (ChunkState state)
		{
			if (gNameBuilder == null) {
				gNameBuilder = new StringBuilder ();
			}
			gNameBuilder.Clear ();
			gNameBuilder.Append (gChunkNamePrefix);
			gNameBuilder.Append (state.XTilePosition);
			gNameBuilder.Append ("-");
			gNameBuilder.Append (state.ZTilePosition);
			gNameBuilder.Append ("-");
			gNameBuilder.Append (state.ID);
			return gNameBuilder.ToString ();
		}
		//TODO move to GameData
		public static string SliceName (int detailLayer, int x, int y)
		{
			if (gNameBuilder == null) {
				gNameBuilder = new StringBuilder ();
			}
			gNameBuilder.Clear ();
			gNameBuilder.Append (gDetailLayerPrefix);
			gNameBuilder.Append (detailLayer);
			gNameBuilder.Append (gSlicePrefix);
			gNameBuilder.Append (x);
			gNameBuilder.Append ("-");
			gNameBuilder.Append (y);
			return gNameBuilder.ToString ();
		}

		#endregion

		protected static TreeInstanceTemplate[] gEmptyTreeInstances;
		protected static PlantInstanceTemplate[] gEmptyPlantInstances;
		protected static List <PathMarkerInstanceTemplate> gEmptyPathMarkerInstances;
		protected const int QuadTreeMaxContentScaler = 50;
		protected const int QuadTreeMaxContentMinimum = 10;
		//heights can be static because no two chunks will be loading a heightmap at the same time
		public static float[,] gHeights = null;
		//detail layers unfortunately have to be stored one per chunk
		//but we only need as many as there are chunks at any given time
		public HashSet <string> mFilledSlices;
		public int[,] mDetailSlice;
		protected static StringBuilder gNameBuilder;
		protected static string gDetailLayerPrefix = "Detail-";
		protected static string gSlicePrefix = "-Slice-";
		protected static string gChunkNamePrefix = "C-";
	}

	[Serializable]
	public class DetailSlice : Mod
	{
		//a little chunk of terrain detail that's used to store/retrieve grass and stuff
		public int OffsetX = 0;
		public int OffsetY = 0;
		public byte[,] DetailData;
	}

	[Serializable]
	public class ChunkTransforms
	{
		//convenience class for keeping track of these
		//instead of searching for them all the damn time
		public Transform Plants;
		public Transform Terrain;
		public Transform Nodes;
		public Transform Triggers;
		public Transform Paths;
		public Transform WorldItems;
		public Transform AboveGround;
		public Transform BelowGround;
		public Transform AboveGroundStaticImmediate;
		public Transform AboveGroundStaticAdjascent;
		public Transform AboveGroundStaticDistant;
		public Transform AboveGroundGenerated;
		public Transform AboveGroundOcean;
		public Transform AboveGroundFX;
		public Transform AboveGroundAudio;
		public Transform BelowGroundStatic;
		public Transform BelowGroundGenerated;
		public Transform BelowGroundFX;
		public Transform BelowGroundAudio;
		public Transform AboveGroundWorldItems;
		public Transform BelowGroundWorldItems;
		public Transform AboveGroundRivers;
		public Transform BelowGroundRivers;
	}
}
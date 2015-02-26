using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.BaseWIScripts;
using ExtensionMethods;

namespace Frontiers.World
{
		public partial class WorldChunk
		{
				#if UNITY_EDITOR
				public string CurrentWorldName = "FRONTIERS";

				public bool LockTileOffset = true;
				public bool SaveSceneryOnSave = true;
				public bool SavePathsOnSave = true;
				public bool SaveWorldItemsOnSave = true;
				public bool SaveDetailsOnSave = true;
				public bool SaveRiversOnSave = true;
				public bool SaveTriggersOnSave = true;

				public void EditorLoadScenery(bool load)
				{
						if (load) {
								for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabs.Count; i++) {
										ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabs[i];
										Structures.Get.EditorInstantiateChunkPrefab(chunkPrefab, Transforms.AboveGroundStaticImmediate);
								}
								for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabsAdjascent.Count; i++) {
										ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabsAdjascent[i];
										Structures.Get.EditorInstantiateChunkPrefab(chunkPrefab, Transforms.AboveGroundStaticAdjascent);
								}
								for (int i = 0; i < SceneryData.AboveGround.SolidTerrainPrefabsDistant.Count; i++) {
										ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabsDistant[i];
										Structures.Get.EditorInstantiateChunkPrefab(chunkPrefab, Transforms.AboveGroundStaticDistant);
								}
								for (int i = 0; i < SceneryData.BelowGround.SolidTerrainPrefabs.Count; i++) {
										ChunkPrefab chunkPrefab = SceneryData.AboveGround.SolidTerrainPrefabsDistant[i];
										Structures.Get.EditorInstantiateChunkPrefab(chunkPrefab, Transforms.BelowGroundStatic);
								}
						} else {
								DestroyChildren(Transforms.AboveGroundStaticImmediate);
								DestroyChildren(Transforms.AboveGroundStaticAdjascent);
								DestroyChildren(Transforms.AboveGroundStaticDistant);
						}
				}

				public void EditorLoadChunk()
				{
						LoadChunkTransforms(true);
						WIGroup chunkGroup = gameObject.GetOrAdd <WIGroup>();
						WIGroup wiGroup = Transforms.WorldItems.gameObject.GetOrAdd <WIGroup>();
						wiGroup.ParentGroup = chunkGroup;
						WIGroup agGroup = Transforms.AboveGround.gameObject.GetOrAdd <WIGroup>();
						agGroup.ParentGroup = wiGroup;
						WIGroup bgGroup = Transforms.BelowGround.gameObject.GetOrAdd <WIGroup>();
						bgGroup.ParentGroup = wiGroup;

						string chunkDirectoryName = ChunkDataDirectory(Name);
						Mods.Get.Editor.LoadMod <ChunkTriggerData>(ref TriggerData, chunkDirectoryName, "Triggers");
						Mods.Get.Editor.LoadMod <ChunkNodeData>(ref NodeData, chunkDirectoryName, "Nodes");
						Mods.Get.Editor.LoadMod <ChunkSceneryData>(ref SceneryData, chunkDirectoryName, "Scenery");
						Mods.Get.Editor.LoadMod <ChunkTerrainData>(ref TerrainData, chunkDirectoryName, "Terrain");
						Mods.Get.Editor.LoadMod <ChunkPlantData>(ref PlantData, chunkDirectoryName, "Plants");
						Mods.Get.Editor.LoadMod <ChunkTreeData>(ref TreeData, chunkDirectoryName, "Trees");

						CreateNodesAndTriggers();

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						agGroup.LoadEditor();
						bgGroup.LoadEditor();
				}

				public void ExportSplats(List <Texture2D> textures)
				{
						foreach (Texture2D texture in textures) {
								Texture2D flippedTexture = new Texture2D(texture.width, texture.height);
								flippedTexture.SetPixels(texture.GetPixels());
								Hydrogen.Texture.FlipVertically(flippedTexture);
								var bytes = flippedTexture.EncodeToPNG();
								string fileName	= texture.name.Replace("SplatAlpha ", "Splat") + ".png";
								fileName = fileName.Replace("1", "2");
								fileName = fileName.Replace("0", "1");
								if (!Manager.IsAwake<Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.EditorCurrentWorldName = this.CurrentWorldName;
								Mods.Get.Editor.InitializeEditor(false);
								string directory = System.IO.Path.Combine(GameData.IO.gCurrentWorldModsPath, "ChunkMap");// + Name + "-" + fileName;
								//string path = Application.dataPath + "/Resources/Chunks/" + Name + "/" + Name + "-" + fileName;
								string path = System.IO.Path.Combine(directory, Name + "-" + fileName);
								System.IO.File.WriteAllBytes(path, bytes);
						}
				}

				public void EditorDraw()
				{

						UnityEngine.GUI.color = Color.yellow;
						string actionNodesLabel = string.Empty;
						foreach (KeyValuePair <string, List <ActionNodeState>> nodeStateList in NodeData.NodeStates) {
								actionNodesLabel += nodeStateList.Value.Count.ToString() + " in " + nodeStateList.Key + "\n";
						}
						GUILayout.Label("ActionNodes:\n" + actionNodesLabel);

						GUILayout.Label("Plant instances: " + PlantData.PlantInstances.Length.ToString());

						int numPrefabs = SceneryData.AboveGround.SolidTerrainPrefabs.Count
						                 + SceneryData.AboveGround.SolidTerrainPrefabsAdjascent.Count
						                 + SceneryData.AboveGround.SolidTerrainPrefabsDistant.Count + 1;
						UnityEngine.GUI.color = Color.Lerp(Color.green, Color.red, ((float)numPrefabs) / Globals.MaxChunkPrefabsPerChunk);
						GUILayout.Label("Chunk Prefabs: " + numPrefabs.ToString());

						if (gameObject.HasComponent <ChunkModeChanger>()) {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Label("CHANGING MODE");
						}
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.Button("Current Mode: " + CurrentMode.ToString());
						if (CurrentMode != TargetMode) {
								UnityEngine.GUI.color = Color.red;
						}
						GUILayout.Button("Target Mode: " + CurrentMode.ToString());
						UnityEngine.GUI.color = Color.yellow;
						if (GUILayout.Button("\nSave Chunk\n")) {
								SaveChunkEditor();
						}
						if (GUILayout.Button("\nSave Chunk Scenery\n")) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor();

								SceneryData.AboveGround = new ChunkSceneryPrefabs();
								SceneryData.BelowGround = new ChunkSceneryPrefabs();
								SceneryData.Transitions = new ChunkSceneryPrefabs();

								//gather all the data
								//above ground
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabs, Transforms.AboveGroundStaticImmediate, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabsAdjascent, Transforms.AboveGroundStaticAdjascent, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabsDistant, Transforms.AboveGroundStaticDistant, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.BelowGround.SolidTerrainPrefabs, Transforms.BelowGroundStatic, LocationTerrainType.BelowGround);

								GetTerrainData(TerrainData, PrimaryTerrain);
								//save the chunk state and node group
								SaveTerrainDetailLayers();

								string chunkDirectoryName = ChunkDataDirectory(Name);
								Mods.Get.Editor.SaveMod <ChunkSceneryData>(SceneryData, chunkDirectoryName, "Scenery");
						}

						UnityEngine.GUI.color = Color.red;
						if (GUILayout.Button("\nLoad Chunk From Scratch\n")) {
								EditorLoadChunk();
						}
						UnityEngine.GUI.color = Color.yellow;
						if (GUILayout.Button("\nLoad Chunk Scenery\n")) {
								EditorLoadScenery(true);
						}
						if (GUILayout.Button("\nUn-Load Chunk Scenery\n")) {
								EditorLoadScenery(false);
						}
						if (GUILayout.Button("\nLoad Chunk Map\n")) {
								if (PrimaryMaterial != null) {
										if (!Manager.IsAwake <Mods>()) {
												Manager.WakeUp <Mods>("__MODS");
										}
										Mods.Get.Editor.InitializeEditor();
										Texture2D overlay = null;
										if (Mods.Get.Editor.ChunkMap(ref overlay, Name, "ColorOverlay")) {
												PrimaryMaterial.SetTexture("_CustomColorMap", overlay);
										}
								}
						}

						foreach (KeyValuePair <string,Texture2D> chunkMap in ChunkDataMaps) {
								GUILayout.Label(chunkMap.Key);
								GUILayout.Box(chunkMap.Value);
						}
				}

				public static void EditorSelectAllPlantAssets(Terrain terrain)
				{
						List <UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>();
						foreach (DetailPrototype dp in terrain.terrainData.detailPrototypes) {
								selectedObjects.Add(dp.prototype);
						}
						foreach (TreePrototype tp in terrain.terrainData.treePrototypes) {
								selectedObjects.Add(tp.prefab);
						}
						UnityEditor.Selection.objects = selectedObjects.ToArray();
				}

				public void EditorLoadTreesFromFile()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						ChunkTreeData treeData = null;
						if (Mods.Get.Editor.LoadMod <ChunkTreeData>(ref treeData, ChunkDataDirectory(Name), "Trees")) {
								List <TreeInstance> treeInstances = new List<TreeInstance>();
								foreach (TreeInstanceTemplate tit in treeData.TreeInstances) {
										tit.ParentChunk = this;
										treeInstances.Add(tit.ToInstance);
								}
								PrimaryTerrain.terrainData.treeInstances = treeInstances.ToArray();
						}
				}

				public void RefreshPathsEditor()
				{
						/*
						HashSet <SplineNode> splineNodes = new HashSet <SplineNode>();
						HashSet <SplineNode> sharedSplineNodes = new HashSet <SplineNode>();
						foreach (Transform pathTransform in Transforms.Paths) {
								Spline spline = null;
								if (pathTransform.gameObject.HasComponent <Spline>(out spline)) {
										foreach (SplineNode node in spline.splineNodesArray) {
												//first see if it's a location
												Location location = null;
												PathMarkerTemplateEditor pmte = null;
												if (node.gameObject.HasComponent <Location>(out location)) {
														pmte = node.gameObject.GetOrAdd <PathMarkerTemplateEditor>();
														pmte.Template.Type = PathMarkerType.Location;
												} else {
														//if it's already contained then it's used by more than one path
														if (splineNodes.Contains(node)) {
																//add the template thing to the path marker
																pmte = node.gameObject.GetOrAdd <PathMarkerTemplateEditor>();
																pmte.Template.Type = PathMarkerType.CrossMarker;
																//add it to the shared spline
																sharedSplineNodes.Add(node);
														} else {
																//if it's not already contained, add it now
																splineNodes.Add(node);
														}
												}
										}
								}
								//create a shared node container
								Transform sharedPathMarkerTransform = Transforms.Paths.gameObject.FindOrCreateChild("SharedPathMarkers");
								foreach (SplineNode sharedNode in sharedSplineNodes) {
										sharedNode.transform.parent = sharedPathMarkerTransform;
								}
						}
						*/
				}

				public void SaveChunkEditor()
				{
						//this should only be called in the editor
						if (Application.isPlaying) {
								return;
						}

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.EditorCurrentWorldName = CurrentWorldName;
						Mods.Get.Editor.InitializeEditor();

						Mods.Get.Editor.DeleteMod("Group", ChunkGroup.Props.PathName);

						LoadChunkTransforms(true);

						State.SizeX = (int)PrimaryTerrain.terrainData.size.x;
						State.SizeZ = (int)PrimaryTerrain.terrainData.size.z;
						State.YOffset = PrimaryTerrain.transform.localPosition.y;
						TerrainData.HeightmapHeight = (int)PrimaryTerrain.terrainData.size.y;
						TerrainData.HeightmapResolution = PrimaryTerrain.terrainData.heightmapResolution;
						if (!State.ArbitraryPosition && !LockTileOffset) {
								State.TileOffset = new SVector3(
										(Globals.WorldChunkSize * State.XTilePosition) + Globals.WorldChunkOffsetX,
										State.YOffset,
										(Globals.WorldChunkSize * State.ZTilePosition) + Globals.WorldChunkOffsetZ);
						}

						Vector3 boundsSize = Vector3.zero;
						Vector3 boundsOffset = Vector3.zero;
						boundsSize = new Vector3(State.SizeX, Globals.ChunkMaximumYBounds, State.SizeZ);
						boundsOffset.y = State.YOffset;
						mChunkBounds = new Bounds(boundsOffset, boundsSize);

						mChunkScale.Set(State.SizeX, Globals.ChunkMaximumYBounds, State.SizeZ);

						if (SaveSceneryOnSave) {
								//clear all the old stuff
								SceneryData.AboveGround = new ChunkSceneryPrefabs();
								SceneryData.BelowGround = new ChunkSceneryPrefabs();
								SceneryData.Transitions = new ChunkSceneryPrefabs();

								//gather all the data
								//above ground
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabs, Transforms.AboveGroundStaticImmediate, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabsAdjascent, Transforms.AboveGroundStaticAdjascent, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.AboveGround.SolidTerrainPrefabsDistant, Transforms.AboveGroundStaticDistant, LocationTerrainType.AboveGround);
								GetChunkScenery(SceneryData.BelowGround.SolidTerrainPrefabs, Transforms.BelowGroundStatic, LocationTerrainType.BelowGround);
						}

						if (SaveDetailsOnSave) {
								GetTerrainData(TerrainData, PrimaryTerrain);
								//save the chunk state and node group
								SaveTerrainDetailLayers();
						}

						if (SaveRiversOnSave) {
								SceneryData.AboveGround.RiverNames.Clear();
								SceneryData.BelowGround.RiverNames.Clear();
								foreach (Transform riverChild in Transforms.AboveGroundRivers) {
										RiverAvatar ra = null;
										if (riverChild.gameObject.HasComponent <RiverAvatar>(out ra)) {
												ra.SaveEditor();
												SceneryData.AboveGround.RiverNames.Add(ra.Props.Name);
										}
								}
								foreach (Transform riverChild in Transforms.BelowGroundRivers) {
										RiverAvatar ra = null;
										if (riverChild.gameObject.HasComponent <RiverAvatar>(out ra)) {
												ra.SaveEditor();
												SceneryData.BelowGround.RiverNames.Add(ra.Props.Name);
										}
								}
						}

						string chunkDirectoryName = ChunkDataDirectory(Name);
						Mods.Get.Editor.SaveMod <ChunkState>(State, "Chunk", Name);

						if (SaveSceneryOnSave) {
								Mods.Get.Editor.SaveMod <ChunkPlantData>(PlantData, chunkDirectoryName, "Plants");
								Mods.Get.Editor.SaveMod <ChunkTreeData>(TreeData, chunkDirectoryName, "Trees");
								Mods.Get.Editor.SaveMod <ChunkSceneryData>(SceneryData, chunkDirectoryName, "Scenery");
								Mods.Get.Editor.SaveMod <ChunkTerrainData>(TerrainData, chunkDirectoryName, "Terrain");
						}

						if (SaveWorldItemsOnSave) {
								SaveWorldItemsToGroups();
								//node data is gathered by worlditems
						}

						if (SaveTriggersOnSave) {
								Mods.Get.Editor.SaveMod <ChunkTriggerData>(TriggerData, chunkDirectoryName, "Triggers");
								Mods.Get.Editor.SaveMod <ChunkNodeData>(NodeData, chunkDirectoryName, "Nodes");
						}

						/*
						if (SavePathsOnSave) {
								List <PathEditor> pes = new List<PathEditor>();
								foreach (Transform pathChild in Transforms.Paths) {
										PathEditor pe = null;
										if (pathChild.gameObject.HasComponent <PathEditor>(out pe) && !pe.DoNotRefreshOrSave) {
												pe.EditorRefresh();
												pes.Add(pe);
										}
								}
								foreach (PathEditor pe in pes) {
										pe.EditorSave();
								}
						}
						*/
				}

				protected void GetChunkScenery(List <ChunkPrefab> prefabs, Transform startTerrain, LocationTerrainType TerrainType)
				{
						if (!Manager.IsAwake <Structures>()) {
								Manager.WakeUp <Structures>("Frontiers_Structures");
								Structures.Get.Initialize();
						}

						prefabs.Clear();
						MeshFilter[] meshFilters = startTerrain.GetComponentsInChildren <MeshFilter>();
						int numPrefabs = 0;

						foreach (MeshFilter prefabMf in meshFilters) {
								GameObject prefabGo = prefabMf.gameObject;

								if (prefabGo.layer == Globals.LayerNumHidden) {
										continue;
								} else {
										//used as a guide to see how many chunk prefabs we have in the scene
										numPrefabs++;
								}

								MeshRenderer prefabMr = prefabGo.GetComponent <MeshRenderer>();

								if (prefabMr != null) {
										ChunkPrefab chunkPrefab = new ChunkPrefab();
										chunkPrefab.Name = prefabGo.name;
										chunkPrefab.Transform.Position = prefabGo.transform.position;
										chunkPrefab.Transform.Rotation = prefabGo.transform.rotation.eulerAngles;
										chunkPrefab.Transform.Scale = prefabGo.transform.lossyScale;
										chunkPrefab.Tag = prefabGo.tag;
										chunkPrefab.Layer = prefabGo.layer;
										chunkPrefab.TerrainType = TerrainType;

										float xScale = prefabGo.transform.localScale.x;
										float yScale = prefabGo.transform.localScale.y;
										float zScale = prefabGo.transform.localScale.z;

										if (Mathf.Approximately(xScale, yScale) && Mathf.Approximately(xScale, zScale)) {
												//we're uniformly scaled, good
										} else {
												Transform nus = prefabGo.transform.parent.gameObject.FindOrCreateChild("NON_UNIFORM_SCALE").transform;
												prefabGo.transform.parent = nus;
												//we're non-uniformly scaled, move it so we can potentially fix it
										}


										foreach (Material material in prefabMr.sharedMaterials) {
												if (material == null) {
														Debug.Log("MATERIAL WAS NULL IN " + prefabGo.name);
												} else {
														if (material.name == "SnowOverlayMaterial") {// Mats.Get.SnowOverlayMaterial.name) {
																chunkPrefab.EnableSnow = true;
														} else {
																chunkPrefab.SharedMaterialNames.Add(material.name);
														}
												}
										}

										MeshCollider mc = prefabGo.GetComponent <MeshCollider>();
										if (mc != null) {
												chunkPrefab.UseMeshCollider = true;
												chunkPrefab.UseConvexMesh = mc.convex;
										} else {
												chunkPrefab.UseMeshCollider = false;
										}

										chunkPrefab.PackName = Structures.Get.PackName(prefabGo.name);
										chunkPrefab.PrefabName = prefabGo.name;

										SceneryScript[] sceneryScripts = prefabGo.GetComponents <SceneryScript>();
										for (int i = 0; i < sceneryScripts.Length; i++) {
												SceneryScript script = sceneryScripts[i];
												script.OnEditorRefresh();
												script.RefreshState();//chunk triggers are not saved locally
												string scriptState = string.Empty;
												string sceneryScriptName = script.ScriptName;
												if (script.GetSceneryState(out scriptState)) {
														chunkPrefab.SceneryScripts.Add(sceneryScriptName, scriptState);
												}
										}

										prefabs.Add(chunkPrefab);
								}
						}

						if (numPrefabs > Globals.MaxChunkPrefabsPerChunk) {
								Debug.LogWarning ("WARNING: This chunk has too many prefabs! " + numPrefabs.ToString ());
						} else {
								Debug.Log (numPrefabs.ToString () + " Chunk prefabs found");
						}

						//TODO add FX to chunk scenery
						/*
						prefabs.FX.Clear ();
						foreach (Transform fxChild in FX) {
							WorldFX worldFX = null;
							if (fxChild.gameObject.HasComponent <WorldFX> (out worldFX)) {
								prefabs.FX.Add (new FXTemplate (worldFX));
							} else if (fxChild.light != null) {
								prefabs.FX.Add (new FXTemplate (fxChild.light));
							}
						}
						*/
				}

				protected void GetTerrainData(ChunkTerrainData terrainData, Terrain terrain)
				{
						terrainData.MaterialSettings.GetSettings(PrimaryMaterial);

						terrainData.GrassTint = terrain.terrainData.wavingGrassTint;
						terrainData.WindBending = terrain.terrainData.wavingGrassAmount;
						terrainData.WindSize = terrain.terrainData.wavingGrassStrength;
						terrainData.WindSpeed = terrain.terrainData.wavingGrassSpeed;

						terrainData.DetailTemplates.Clear();
						terrainData.TreeTemplates.Clear();
						terrainData.TextureTemplates.Clear();

						int plantInstancePrototypeIndex = -1;

						foreach (DetailPrototype detailPrototype in terrain.terrainData.detailPrototypes) {
								TerrainPrototypeTemplate tpt = new TerrainPrototypeTemplate();
								tpt.RenderMode = detailPrototype.renderMode;
								tpt.BendFactor = detailPrototype.bendFactor;
								tpt.NoiseSpread = detailPrototype.noiseSpread;
								tpt.MinHeight = detailPrototype.minHeight;
								tpt.MinWidth = detailPrototype.minWidth;
								tpt.MaxHeight = detailPrototype.maxHeight;
								tpt.MaxWidth = detailPrototype.maxWidth;
								tpt.DryColor = detailPrototype.dryColor;
								tpt.HealthyColor = detailPrototype.healthyColor;
								tpt.UsePrototypeMesh = detailPrototype.usePrototypeMesh;

								if (detailPrototype.usePrototypeMesh) {
										tpt.Type = PrototypeTemplateType.DetailMesh;
										tpt.RenderMode = DetailRenderMode.Grass;//vertex lit means custom grass texture
										tpt.AssetName = detailPrototype.prototype.name;
								} else {
										tpt.Type = PrototypeTemplateType.DetailTexture;
										tpt.RenderMode = DetailRenderMode.Grass;//never billboard
										tpt.AssetName = detailPrototype.prototypeTexture.name;
								}

								terrainData.DetailTemplates.Add(tpt);
						}

						for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++) {
								TreePrototype treePrototype = terrain.terrainData.treePrototypes[i];
								//if it's a tree, add the prototype to the data
								TerrainPrototypeTemplate tpt = new TerrainPrototypeTemplate();
								tpt.AssetName = treePrototype.prefab.name;
								tpt.BendFactor = treePrototype.bendFactor;
								tpt.Type = PrototypeTemplateType.TreeMesh;

								terrainData.TreeTemplates.Add(tpt);
								if (treePrototype.prefab.name == "PlantInstancePrefab") {
										//save this info for later
										plantInstancePrototypeIndex = i;
								}
						}

						foreach (SplatPrototype splatPrototype in terrain.terrainData.splatPrototypes) {
								TerrainTextureTemplate ttt = new TerrainTextureTemplate();
								ttt.DiffuseName = splatPrototype.texture.name;
								if (splatPrototype.normalMap != null) {
										ttt.NormalName = splatPrototype.normalMap.name;
								}
								ttt.Size = splatPrototype.tileSize;
								ttt.Offset = splatPrototype.tileOffset;

								terrainData.TextureTemplates.Add(ttt);
						}
						//get the tree instances from the actual terrain
						//this will include some plant instances
						TreeInstance[] treeInstances = terrain.terrainData.treeInstances;
						List <TreeInstanceTemplate> treeInstanceTemplates = new List<TreeInstanceTemplate>();
						List <PlantInstanceTemplate> plantInstanceTemplates = new List<PlantInstanceTemplate>();
						for (int i = 0; i < treeInstances.Length; i++) {
								if (treeInstances[i].prototypeIndex == plantInstancePrototypeIndex) {
										//not a tree, add this to the plant instances
										PlantInstanceTemplate plantInstance = new PlantInstanceTemplate(treeInstances[i], TerrainData.HeightmapHeight, ChunkOffset, ChunkScale);
										plantInstance.AboveGround = true;
										plantInstanceTemplates.Add(plantInstance);
								} else {
										treeInstanceTemplates.Add(new TreeInstanceTemplate(treeInstances[i], TerrainData.HeightmapHeight, ChunkOffset, ChunkScale));
								}
						}

						TreeData.TreeInstances = treeInstanceTemplates.ToArray();
						PlantData.PlantInstances = plantInstanceTemplates.ToArray();
						//path markers are already saved
						//done!
				}

				public void SaveTerrainDetailLayers()
				{
						TerrainData terrainData = PrimaryTerrain.terrainData;
						int numDetailLayers = terrainData.detailPrototypes.Length;
						int detailLayerResolution = terrainData.detailResolution;
						int[,] detailSlice = new int [Globals.WorldChunkDetailSliceResolution, Globals.WorldChunkDetailSliceResolution];
						for (int detailLayer = 0; detailLayer < numDetailLayers; detailLayer++) {
								//create little slices of the terrain from the main array
								int[,] currentDetailLayer = terrainData.GetDetailLayer(0, 0, detailLayerResolution, detailLayerResolution, detailLayer);

								int numSlices = Globals.WorldChunkDetailResolution / Globals.WorldChunkDetailSliceResolution;
								int xOffset = 0;
								int yOffset = 0;

								for (int xs = 0; xs < numSlices; xs++) {
										for (int ys = 0; ys < numSlices; ys++) {
												xOffset = xs * Globals.WorldChunkDetailSliceResolution;
												yOffset = ys * Globals.WorldChunkDetailSliceResolution;
												bool saveThisSlice = false;
												for (int x = 0; x < Globals.WorldChunkDetailSliceResolution; x++) {
														for (int y = 0; y < Globals.WorldChunkDetailSliceResolution; y++) {
																int nextValue = currentDetailLayer[xOffset + x, yOffset + y];
																detailSlice[x, y] = nextValue;
																//make sure there's something in this before we save it
																saveThisSlice |= (nextValue > 0);
														}
												}
												//now that we've copied the slice data
												//save it
												if (saveThisSlice) {
														Mods.Get.Editor.SaveTerrainDetailSlice(detailSlice, Name, SliceName(detailLayer, xs, ys));
												}
										}
								}
						}
				}

				public void SetStartupPosition()
				{
						if (!Manager.IsAwake <GameWorld>()) {
								Manager.WakeUp <GameWorld>("__WORLD");
						}

						GameObject spawnPointObject = null;
						PlayerStartupPosition pos = null;
						if (UnityEditor.Selection.activeGameObject != gameObject) {
								//we're not looking for a generic spawn point
								spawnPointObject = UnityEditor.Selection.activeGameObject;
								ActionNode node = spawnPointObject.GetComponent <ActionNode>();
								pos = new PlayerStartupPosition();
								pos.ChunkID = State.ID;
								pos.ChunkPosition = new STransform(node.transform.position);
								pos.Name = node.State.Name;
								//get our location reference
								//action nodes can be in buildings or in locations
								GameObject locationObject = null;
								if (node.transform.parent.name == "__ACTION_NODES") {
								/*
								if (node.transform.parent.parent.name.Contains ("=VARIANT")) {
									//action nodes->interior->variant->normal->structure base->location
									locationObject = node.transform.parent.parent.parent.parent.parent.parent.gameObject;//jesus christ...
									pos.Interior = true;
									pos.StructureName = locationObject.name;
								} else {
									//action nodes->interior->normal->structure base->location
									locationObject = node.transform.parent.parent.parent.parent.parent.gameObject;//ugh
									pos.Interior = false;
									pos.StructureName = locationObject.name;
								}
								*/
										//do nothing
								} else {
										locationObject = node.transform.parent.gameObject;
								}

								WorldItem locationWorldItem = null;
								if (!locationObject.HasComponent <WorldItem>(out locationWorldItem)) {
										Debug.Log ("No location world itme on object");
								} else {
										MobileReference mb = locationWorldItem.StaticReference;
										pos.LocationReference = new MobileReference(mb.FileName, mb.GroupPath);
								}
						} else {
								//find and create generic spawn point
								spawnPointObject = GameObject.Find("StartupPosition");
								Vector3 spawnPoint = spawnPointObject.transform.position;
								pos = new PlayerStartupPosition();
								pos.ChunkPosition = new STransform(spawnPoint);
								pos.ChunkID = State.ID;
								pos.Name = "StartupPosition";
								//get the location path from the spawn point's parent
								GameObject locationObject = spawnPointObject.transform.parent.gameObject;
								WorldItem locationWorldItem = locationObject.GetComponent <WorldItem>();
								MobileReference mb = locationWorldItem.StaticReference;
								pos.LocationReference = new MobileReference(mb.FileName, mb.GroupPath);
						}
						bool foundExisting = false;
						for (int i = 0; i < GameWorld.Get.WorldStartupPositions.Count; i++) {
								if (GameWorld.Get.WorldStartupPositions[i].Name == pos.Name) {
										foundExisting = true;
										GameWorld.Get.WorldStartupPositions[i] = pos;
										break;
								}
						}
						if (!foundExisting) {
								GameWorld.Get.WorldStartupPositions.Add(pos);
						}
				}

				public void SaveWorldItemsToGroups()
				{
						ChunkGroup.RefreshEditor();
						//this is where we get our chunk nodes
						//so clear this here
						TriggerData.TriggerStates.Clear();
						NodeData.NodeStates.Clear();
						NodeData.TerrainNodes.Clear();
						NodeData.Name = Name;
						List <string> ignoreTypes = new List<string>() {
								"PathMarker",
								"City",
								"Woods",
								"District",
								"Den"
						};

						List <string> existingActionNodes = new List<string>();

						foreach (Transform child in Transforms.Nodes) {
								NodeData.TerrainNodes.Add(new TerrainNode(child.localPosition + State.TileOffset));
						}

						foreach (Transform child in transform) {
								SaveWorldItemsToGroups(child, ignoreTypes, existingActionNodes);
						}
				}

				protected void SaveWorldItemsToGroups(Transform start, List <string> ignoreTypes, List <string> existingActionNodes)
				{
						StructureBuilder sb = start.gameObject.GetComponent <StructureBuilder>();
						if (sb != null) {
								Debug.Log ("Hit structure builder, stopping");
								return;
						}

						foreach (Transform agWiChild in start) {
								WorldItem worlditem = agWiChild.GetComponent <WorldItem>();
								Location location = agWiChild.GetComponent <Location>();
								City city = agWiChild.GetComponent <City>();
								Structure structure = agWiChild.GetComponent <Structure>();
								WIGroup worlditemGroup = agWiChild.GetComponent <WIGroup>();
								WorldTrigger trigger = agWiChild.GetComponent <WorldTrigger>();

								if (worlditem != null) {
										UnityEditor.EditorUtility.SetDirty(worlditem);
								}
								if (location != null) {
										UnityEditor.EditorUtility.SetDirty(location);
								}
								if (city != null) {
										UnityEditor.EditorUtility.SetDirty(city);
								}
								if (structure != null) {
										UnityEditor.EditorUtility.SetDirty(structure);
								}
								UnityEditor.EditorUtility.SetDirty(start.gameObject);

								if (trigger != null) {
										trigger.OnEditorRefresh();
										trigger.RefreshState(false);//chunk triggers are not saved locally
										string triggerName = trigger.name;
										string triggerState = string.Empty;
										string triggerScriptName = trigger.ScriptName;
										if (trigger.GetTriggerState(out triggerState)) {
												TriggerData.TriggerStates.Add(triggerName, new KeyValuePair <string, string>(triggerScriptName, triggerState));
										}
								} else if (worlditem != null) {
										worlditem.IsTemplate = false;
										//set the worlditem's chunk position
										worlditem.Props.Local.ChunkPosition = WorldPositionToChunkPosition(ChunkBounds, worlditem.transform.position);
										//add to location lookup as long as it's something that can be added to a path
										#if UNITY_EDITOR
										UnityEditor.EditorUtility.SetDirty(worlditem);
										#endif
										if (location != null) {
												SphereCollider sphereColloder = worlditem.gameObject.GetComponent <SphereCollider>();
												if (sphereColloder != null) {
														worlditem.Props.Local.ActiveRadius = (int)sphereColloder.radius;
														if (worlditem.Props.Local.ActiveRadius < worlditem.ActiveRadius * 2) {
																worlditem.Props.Local.VisibleDistance = worlditem.ActiveRadius * 2;
														}
												}
												location.State.Transform = new STransform(location.transform);
												//add a group to the location if it's a parent group
												location.gameObject.GetOrAdd <WIGroup>();
										}
								} else if (structure != null) {
										structure.State.MinorStructures.Clear();
										foreach (Transform structureChild in structure.transform) {
												StructureBuilder builder = structureChild.GetComponent <StructureBuilder>();
												if (builder != null) {
														/*
														if (builder != structure.PrimaryBuilder) {
															MinorStructure minorStructure = new MinorStructure ();
															minorStructure.InteriorVariant	= 0;
															minorStructure.TemplateName = StructureBuilder.GetTemplateName (builder.name);
															minorStructure.Position = builder.transform.localPosition;
															minorStructure.Rotation = builder.transform.localRotation.eulerAngles;

															structure.State.MinorStructures.Add (minorStructure);
														} else {
															structure.State.PrimaryBuilderOffset	= new STransform (builder.transform);
															structure.State.TemplateName = StructureBuilder.GetTemplateName (builder.name);
														}
														if (builder.transform.childCount > 0) {
															builder.SaveEditorStructureToTemplate ();
														}
														*/
												}
										}
								} else if (city != null) {
										city.State.MinorStructures.Clear();
										foreach (Transform cityChild in structure.transform) {
												StructureBuilder builder = cityChild.GetComponent <StructureBuilder>();
												if (builder != null) {
														MinorStructure minorStructure = new MinorStructure();
														minorStructure.InteriorVariant	= 0;
														minorStructure.TemplateName = StructureBuilder.GetTemplateName(builder.name);
														minorStructure.Position = builder.transform.localPosition;
														minorStructure.Rotation = builder.transform.localRotation.eulerAngles;

														city.State.MinorStructures.Add(minorStructure);
												}
										}
								}
								//always save the group regardless of other
								//scripts attached
								if (worlditemGroup != null) {
										List <ActionNodeState> localNodeList = null;
										//get all action nodes
										bool foundExistingList = true;
										bool addedAtLeastOne = false;
										if (!NodeData.NodeStates.TryGetValue(worlditemGroup.Props.PathName, out localNodeList)) {
												foundExistingList = false;
												localNodeList = new List <ActionNodeState>();
										} else {
												foundExistingList = true;//AAARG
												Debug.Log ("There's already an entry for " + worlditemGroup.Props.PathName + " so we're just going to use that");
										}
										List <ActionNode> actionNodesToRefresh = new List<ActionNode>();
										foreach (Transform actionNodeChild in worlditemGroup.transform) {
												//Debug.Log ("Checking potential action node child " + actionNodeChild.name);
												ActionNode node = actionNodeChild.GetComponent <ActionNode>();
												if (node != null) {
														/*
														string[] splitNodeName = node.State.Name.Split (new String [] { "-" }, StringSplitOptions.RemoveEmptyEntries);
														string finalNodeName = splitNodeName [0];
														int increment = 1;
														while (existingActionNodes.Contains (finalNodeName)) {
															increment++;
															finalNodeName = splitNodeName [0];
															finalNodeName = finalNodeName + "-" + increment.ToString ();
														}
														node.name = finalNodeName;
														node.State.Name = finalNodeName;
														*/
														actionNodesToRefresh.Add(node);
														//add node to local list
														localNodeList.Add(node.State);
														addedAtLeastOne = true;
														//existingActionNodes.Add (node.State.Name);
												}
										}

										//can't unparent in the middle of foreach because it rearranges transform order
										//so do it here
										foreach (ActionNode node in actionNodesToRefresh) {
												node.State.ChunkOffset = State.TileOffset;
												////Debug.Log ("Found child " + node.State.Name + " in group " + worlditemGroup.Props.PathName);
												node.State.ParentGroupPath = worlditemGroup.Props.PathName;
												//move to nodes parent and save transform
												node.transform.parent = Transforms.Nodes;
												node.Refresh();
												//move node back to original parent
												node.transform.parent = worlditemGroup.transform;
										}
										if (!foundExistingList && localNodeList.Count > 0 && addedAtLeastOne) {
												NodeData.NodeStates.Add(worlditemGroup.Props.PathName, localNodeList);
										}
										//TODO re-implement name check
										//steal from WIGroup name check it works fine
										/*
										//if we got any nodes
										if (localNodeList.Count > 0) {
											List <ActionNodeState> existingNodeList = null;
											if (NodeData.NodeStates.TryGetValue (worlditemGroup.Props.PathName, out existingNodeList)) {	//if it exists already, add to it
												for (int i = 0; i < localNodeList.Count; i++) {
													existingNodeList.SafeAdd (localNodeList [i]);
												}
											} else {
												NodeData.NodeStates.Add (worlditemGroup.Props.PathName, localNodeList);
												//otherwise just add the local list
											}
										}
										*/
										worlditemGroup.SaveEditor();
								}

								SaveWorldItemsToGroups(agWiChild, ignoreTypes, existingActionNodes);
						}
				}

				private void OnDrawGizmos()
				{
						Color gc = Color.white;
						float alpha = 0.1f;
						switch (mCurrentMode) {
								case ChunkMode.Unloaded:
								default:
										gc = Color.red;
										break;

								case ChunkMode.Distant:
										gc = Color.yellow;
										break;

								case ChunkMode.Adjascent:
										gc = Color.green;
										break;

								case ChunkMode.Immediate:
										gc = Color.cyan;
										break;

								case ChunkMode.Primary:
										gc = Color.blue;
										alpha = 1.0f;
										break;
						}
						gc.a = 0.08f;
						Gizmos.color = Colors.Alpha(gc, alpha);
						Bounds chunkBounds = ChunkBounds;
						Gizmos.DrawWireCube(chunkBounds.center, chunkBounds.size);

						//draws plants and stuff
						//really noisy, i keep it off
						/*
						if (Application.isPlaying) {
								Bounds colliderBounds = new Bounds(Player.Local.Position, Vector3.one * (Player.Local.ColliderRadius * 2));
								if (mInitialized && mCurrentMode == ChunkMode.Primary) {
										foreach (PlantInstanceTemplate pi in PlantInstances) {
												Gizmos.color = Colors.Alpha(Color.green, 0.25f);
												Gizmos.DrawLine(pi.Position, pi.Position + (Vector3.up * 1));
												if (colliderBounds.Contains(pi.Position)) {
														Gizmos.DrawWireSphere(pi.Position, 1.0f);
												}
										}
										foreach (TreeInstanceTemplate ti in TreeInstances) {
												Gizmos.DrawLine(ti.Position, ti.Position + (Vector3.up * 10));
												if (colliderBounds.Contains(ti.Position)) {
														Gizmos.DrawWireSphere(ti.Position, 1.0f);
												}
										}
										if (TreeInstanceQuad.Children != null) {
												foreach (QuadNode <TreeInstanceTemplate> node in TreeInstanceQuad.Children) {
														if (node != null) {
																DrawQuadNode(node, Color.magenta);
														}
												}
										}
										if (PlantInstanceQuad.Children != null) {
												foreach (QuadNode <PlantInstanceTemplate> node in PlantInstanceQuad.Children) {
														if (node != null) {
																DrawQuadNode(node, Color.magenta);
														}
												}
										}
								}
						}
						*/
				}

				protected void DrawQuadNode(QuadNode <TreeInstanceTemplate> node, Color color)
				{
						Color nodeColor = Color.Lerp(Colors.Alpha(color, 0.1f), Color.white, 0.1f);
						Gizmos.DrawWireCube(node.Boundaries.center, node.Boundaries.size);
						if (node.Children != null) {
								foreach (QuadNode<TreeInstanceTemplate> child in node.Children) {
										DrawQuadNode(child, nodeColor);
								}
						}
				}

				protected void DrawQuadNode(QuadNode <PlantInstanceTemplate> node, Color color)
				{
						Color nodeColor = Color.Lerp(Colors.Alpha(color, 0.1f), Color.white, 0.1f);
						Gizmos.DrawWireCube(node.Boundaries.center, node.Boundaries.size);
						if (node.Children != null) {
								foreach (QuadNode<PlantInstanceTemplate> child in node.Children) {
										DrawQuadNode(child, nodeColor);
								}
						}
				}
				#endif
		}
}
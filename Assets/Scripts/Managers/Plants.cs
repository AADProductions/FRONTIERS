using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;
using Hydrogen.Threading.Jobs;
using ExtensionMethods;
using Frontiers.World.WIScripts;

namespace Frontiers
{
		[ExecuteInEditMode]
		public class Plants : Manager
		{
				public static Plants Get;
				public GameObject Prototypes;
				public List <GameObject> PathWeedPrototypes = new List<GameObject>();
				public List <GameObject> TerrainPlantPrototypes = new List<GameObject>();
				public DamagePackage ThornDamage = new DamagePackage();
				public static float MinimumGatheringSkillToRevealBasicProps = 0.25f;
				public static float MinimumGatheringSkillToRevealEdibleProps = 0.5f;
				public DetailPrototype[] DefaultDetailPrototypes = null;
				public DetailPrototype DefaultDetailPrototype;
				public List <GameObject> TreePrototypes = new List<GameObject>();
				public Material AtsBarkMaterial;
				public Material AtsLeavesMaterial;

				public string[] AvailablePlantNames {
						get {
								if (mAvailablePlantNames == null) {
										mAvailablePlantNames = new string [TreePrototypes.Count];
										for (int i = 0; i < mAvailablePlantNames.Length; i++) {
												mAvailablePlantNames[i] = TreePrototypes[i].name;
										}
								}
								return mAvailablePlantNames;
						}
				}

				[NonSerialized]
				protected string[] mAvailablePlantNames = null;

				public List <Plant> KnownPlants(ClimateType climate, TimeOfYear seasonality, bool aboveGround, bool requireRevealed)
				{
						mKnownPlantsResult.Clear();
						for (int i = 0; i < PlantList.Count; i++) {
								Plant plant = PlantList[i];
								if (!requireRevealed || plant.Revealed) {
										if (plant.Climate == climate && Flags.Check((uint)plant.Seasonality, (uint)seasonality, Flags.CheckType.MatchAny) && plant.AboveGround == aboveGround) {
												mKnownPlantsResult.Add(plant);
										}
								}
						}
						return mKnownPlantsResult;
				}

				public int TotalPlantsInClimate(ClimateType selectedClimate, bool aboveGround)
				{
						int totalPlants = 0;
						for (int i = 0; i < PlantList.Count; i++) {
								if (PlantList[i].AboveGround == aboveGround && PlantList[i].Climate == selectedClimate) {
										totalPlants++;
								}
						}
						return totalPlants;
				}

				protected List <Plant> mKnownPlantsResult = new List <Plant>();

				public bool GetTerrainPlantPrototype(string terrainPlantPrototypeName, out GameObject terrainPlantPrototype)
				{
						//terrainPlantPrototype = null;
						return mTerrainPlantLookup.TryGetValue(terrainPlantPrototypeName, out terrainPlantPrototype);
						/*foreach (GameObject tpt in TerrainPlantPrototypes) {
							if (string.Equals(tpt.name, terrainPlantPrototypeName)) {
								terrainPlantPrototype = tpt;
								break;
							}
						}
						return terrainPlantPrototype != null;*/
				}

				public bool GetTerrainPlantPrototypes(List<TerrainPrototypeTemplate> treeTemplates, TreeInstanceTemplate[] treeInstances, ref TreePrototype[] treePrototypes, ref TreeColliderTemplate[] colliderTemplates)
				{
						bool useSubstitutes = Profile.Get.CurrentPreferences.Video.TerrainReduceTreeVariation;
						GameObject prefab = null;
						TreeColliderTemplate colliderTemplate = null;
						List <TreeColliderTemplate> colliderTemplatesList = new List<TreeColliderTemplate>();
						List <TreePrototype> treePrototypesList = new List<TreePrototype>();
						for (int i = 0; i < treeTemplates.Count; i++) {	
								if (treeTemplates[i].AssetName == "PlantInstancePrefab") {
										Debug.LogError("Skipping plant instance prototype, shouldn't be here!");
								} else {
										if (GetTerrainPlantPrototype(treeTemplates[i].AssetName, out prefab)) {
												colliderTemplate = prefab.GetComponent <TreeColliderTemplate>();
												if (colliderTemplate == null) {
														Debug.LogError("No collider template for " + prefab.name);
												} else if (useSubstitutes && colliderTemplate.HasLowQualitySubstitute) {
														//only one level of substitution
														if (GetTerrainPlantPrototype(colliderTemplate.SubstituteOnLowQuality, out prefab)) {
																colliderTemplate = prefab.GetComponent <TreeColliderTemplate>();
																if (colliderTemplate == null) {
																		Debug.LogError("No collider template for " + prefab.name);
																}
														}
												}
										}

										if (colliderTemplate != null) {
												if (useSubstitutes) {
														//if we're using substitutes, make sure we don't already have this one in our list
														int existingPrototypeIndex = -1;
														for (int j = 0; j < treePrototypesList.Count; j++) {
																if (treePrototypesList[j].prefab == prefab) {
																		existingPrototypeIndex = j;
																		break;
																}
														}
														if (existingPrototypeIndex >= 0) {
																//Debug.Log("Using existing prototype " + existingPrototypeIndex.ToString() + " (" + prefab.name + ")");
																//if it already exists, make sure the substitutions are updated in the instances
																//int numReplaced = 0;
																for (int j = 0; j < treeInstances.Length; j++) {
																		if (treeInstances[j].PrototypeIndex == i) {
																				//this will make it use the right index when converted to an instance
																				TreeInstanceTemplate t = treeInstances[j];
																				t.UsePrototypeSubstitute = true;
																				t.PrototypeSubstituteIndex = existingPrototypeIndex;
																				treeInstances[j] = t;
																				//numReplaced++;
																		}
																}
																//Debug.Log("Set " + numReplaced.ToString() + " tree instances to substitute " + existingPrototypeIndex.ToString());
														} else {
																//if it doesn't exist yet, just create a new prototype
																TreePrototype t = new TreePrototype();
																t.prefab = prefab;
																treePrototypesList.Add(t);
																colliderTemplatesList.Add(colliderTemplate);
														}
												} else {
														//otherwise just add it
														TreePrototype t = new TreePrototype();
														t.prefab = prefab;
														treePrototypesList.Add(t);
														colliderTemplatesList.Add(colliderTemplate);
												}
										}
								}
						}
						treePrototypes = treePrototypesList.ToArray();
						colliderTemplates = colliderTemplatesList.ToArray();
						return true;
				}

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;

						mAvailablePlantNames = null;

						mGeneralInfo = new WIExamineInfo();
						mEdibleInfo = new WIExamineInfo();

						for (int i = TerrainPlantPrototypes.LastIndex(); i >= 0; i--) {
								if (TerrainPlantPrototypes[i] == null) {
										TerrainPlantPrototypes.RemoveAt(i);
								}
						}

						if (gPlantBuilderHelper == null) {
								gPlantBuilderHelper = gameObject.FindOrCreateChild("PlantBuilderHelper").transform;
								gPlantFlowerBuilderHelper = gameObject.FindOrCreateChild("PlantFlowerBuilderHelper").transform;
								Prototypes = gameObject.FindOrCreateChild("Prototypes").gameObject;
						}
				}

				public override void Initialize()
				{
						if (GameManager.Get.TestingEnvironment) {
								mInitialized = true;
								return;
						}

						PullDataFromPrefabs();

						mPlantMaterialLookup.Clear();
						BodyMaterialHash = Hydrogen.Material.GetDataHashCode(BodyMaterial);
						FlowerMaterialHash = Hydrogen.Material.GetDataHashCode(FlowerMaterial);
						ThornMaterialHash = Hydrogen.Material.GetDataHashCode(ThornMaterial);
						RootMaterialHash = Hydrogen.Material.GetDataHashCode(RootMaterial);

						mPlantMaterialLookup.Add(BodyMaterialHash, BodyMaterial);
						mPlantMaterialLookup.Add(FlowerMaterialHash, FlowerMaterial);
						mPlantMaterialLookup.Add(ThornMaterialHash, ThornMaterial);
						mPlantMaterialLookup.Add(RootMaterialHash, RootMaterial);

						//set up the empty grass array
						DefaultDetailPrototype = new DetailPrototype();
						Texture2D emptyGrassTexture = null;
						if (Mats.Get.GetTerrainGrassTexture("EmptyGrass", out emptyGrassTexture)) {
								DefaultDetailPrototype.prototypeTexture = emptyGrassTexture;
								DefaultDetailPrototype.usePrototypeMesh = false;
						}
						//make it super tiny
						DefaultDetailPrototype.maxWidth = 0.0001f;
						DefaultDetailPrototype.maxHeight = 0.0001f;
						DefaultDetailPrototype.dryColor = Colors.Alpha(Color.grey, 0f);
						DefaultDetailPrototype.healthyColor = DefaultDetailPrototype.dryColor;

						DefaultDetailPrototypes = new DetailPrototype [Globals.WorldChunkDetailLayers];
						for (int i = 0; i < DefaultDetailPrototypes.Length; i++) {
								DefaultDetailPrototypes[i] = DefaultDetailPrototype;
						}

						mInitialized = true;
				}

				public override void OnModsLoadFinish()
				{
						StartCoroutine(LoadPlantDataAndBuildMeshes());
						for (int i = 0; i < TerrainPlantPrototypes.Count; i++) {
								mTerrainPlantLookup.Add(TerrainPlantPrototypes[i].name, TerrainPlantPrototypes[i]);
						}
						mModsLoaded = true;
				}

				public override void OnGameStart()
				{
						STransform trn = STransform.zero;
						WorldItem plantPrefab = null;
						if (WorldItems.Get.PackPrefab("Plants", "WorldPlant", out plantPrefab)) {
								//create all of our world plants
								for (int i = 0; i < Globals.MaxSpawnedPlants; i++) {
										WorldItem worlditem = null;
										if (WorldItems.CloneWorldItem(plantPrefab, WIGroups.Get.Plants, out worlditem)) {
												worlditem.SetMode(WIMode.Hidden);
												WorldPlant worldPlant = worlditem.GetComponent <WorldPlant>();
												ActivePlants.Add(worldPlant);
												PlantInstanceMappings.Add(worldPlant, PlantInstanceTemplate.Empty);
										}
								}
						}
						StartCoroutine(UpdateWorldPlants());
				}

				public void PullDataFromPrefabs()
				{
						ThornBaseMeshes.Meshes.Clear();
						BodyBaseMeshes.Clear();
						FlowerBaseMeshes.Clear();
						RootBaseMeshes.Clear();

						foreach (Transform thornChild in ThornPrefab.transform) {
								if (thornChild.name.Contains("Thorn")) {
										MeshFilter tcmf = thornChild.GetComponent <MeshFilter>();
										ThornBaseMeshes.Meshes.Add(tcmf.sharedMesh);
										ThornBaseMeshes.BufferedMeshes.Add(CreateBufferedMesh(tcmf.sharedMesh));
								}
						}

						foreach (GameObject bodyBasePrefab in BodyBasePrefabs) {
								PlantBodyMeshList pbml = new PlantBodyMeshList();
								BodyBaseMeshes.Add(pbml);
								foreach (Transform bodyVariation in bodyBasePrefab.transform) {
										if (bodyVariation.name.Contains("Body")) {
												MeshFilter bcmf = bodyVariation.GetComponent <MeshFilter>();
												pbml.Meshes.Add(bcmf.sharedMesh);
												pbml.BufferedMeshes.Add(CreateBufferedMesh(bcmf.sharedMesh));
												List <STransform> thornPoints = new List<STransform>();
												List <STransform> flowerPoints = new List<STransform>();
												pbml.ThornPoints.Add(thornPoints);
												pbml.FlowerPoints.Add(flowerPoints);
												//now look for thorns and flowers
												foreach (Transform variationChild in bodyVariation) {
														switch (variationChild.name) {
																case "FlowerPoint":
																		flowerPoints.Add(new STransform(variationChild, true));
																		break;

																case "ThornPoint":
																		thornPoints.Add(new STransform(variationChild, true));
																		break;

																default:
																		break;
														}
												}
										}
								}
						}

						foreach (GameObject flowerBasePrefab in FlowerBasePrefabs) {
								PlantMeshList fbpl = new PlantMeshList();
								FlowerBaseMeshes.Add(fbpl);
								foreach (Transform flowerVariation in flowerBasePrefab.transform) {
										if (flowerVariation.name.Contains("Flower")) {
												MeshFilter fvmf = flowerVariation.GetComponent <MeshFilter>();
												fbpl.Meshes.Add(fvmf.sharedMesh);
												fbpl.BufferedMeshes.Add(CreateBufferedMesh(fvmf.sharedMesh));
										}
								}
						}

						foreach (GameObject rootBasePrefab in RootBasePrefabs) {
								PlantMeshList rbpl = new PlantMeshList();
								RootBaseMeshes.Add(rbpl);
								foreach (Transform rootVariation in rootBasePrefab.transform) {
										if (rootVariation.name.Contains("Root")) {
												MeshFilter rvmf = rootVariation.GetComponent <MeshFilter>();
												rbpl.Meshes.Add(rvmf.sharedMesh);
												rbpl.BufferedMeshes.Add(CreateBufferedMesh(rvmf.sharedMesh));
										}
								}
						}
				}

				public void Demonstrate()
				{
						PullDataFromPrefabs();

						Plant newPlant = new Plant();
						PlantSeasonalSettings season = new PlantSeasonalSettings();
						//newPlant.HasThorns = true;
						season.Flowers = true;
						season.FlowerDensity = 1f;

						newPlant.BodyType = UnityEngine.Random.Range(0, BodyBaseMeshes.Count);
						newPlant.BodyVariation = UnityEngine.Random.Range(0, BodyBaseMeshes[newPlant.BodyType].Meshes.Count);
						newPlant.BodyTexture = UnityEngine.Random.Range(0, BodyTextures[newPlant.BodyType].Textures.Count);
						newPlant.RootType = (PlantRootType)UnityEngine.Random.Range(0, RootBaseMeshes.Count);
						newPlant.RootVariation = UnityEngine.Random.Range(0, RootBaseMeshes[(int)newPlant.RootType].Meshes.Count);
						newPlant.RootTexture = UnityEngine.Random.Range(0, RootTextures[(int)newPlant.RootType].Textures.Count);
						newPlant.ThornVariation = UnityEngine.Random.Range(0, ThornBaseMeshes.Meshes.Count);
						newPlant.ThornTexture = UnityEngine.Random.Range(0, ThornTextures.Count);

						newPlant.FlowerType = UnityEngine.Random.Range(0, FlowerBaseMeshes.Count);
						newPlant.FlowerVariation = UnityEngine.Random.Range(0, FlowerBaseMeshes[newPlant.FlowerType].Meshes.Count);
						newPlant.FlowerTexture = UnityEngine.Random.Range(0, FlowerTextures[newPlant.FlowerType].Textures.Count);

						StartCoroutine(BuildPlantSeasonalMesh(newPlant, season));

						/*
						 * these steps are no longer necessary
							float xPos = 0;
							GameObject plantObj = null;
							for (int i = 0; i < BodyBaseMeshes.Count; i++) {

								PlantBodyMeshList bodyMeshList = BodyBaseMeshes [i];
								int bodyVariation = UnityEngine.Random.Range (0, bodyMeshList.Meshes.Count);
								int textureVariation = UnityEngine.Random.Range (0, BodyTextures [i].Textures.Count);

								plantObj = new GameObject ("Plant " + i);
								plantObj.AddComponent <MeshFilter> ().sharedMesh = bodyMeshList.Meshes [bodyVariation];
								MeshRenderer bodyMr = plantObj.AddComponent <MeshRenderer> ();
								bodyMr.material = BodyMaterial;
								bodyMr.material.SetTexture ("_MainTex", BodyTextures [i].Textures [textureVariation]);

								int rootType = UnityEngine.Random.Range (0, RootBaseMeshes.Count);
								PlantMeshList rootMeshList = RootBaseMeshes [rootType];
								int rootVariation = UnityEngine.Random.Range (0, rootMeshList.Meshes.Count);
								int rootTextureVariation = UnityEngine.Random.Range (0, RootTextures [rootType].Textures.Count);
								GameObject rootObj = plantObj.CreateChild ("Root").gameObject;
								rootObj.AddComponent <MeshFilter> ().sharedMesh = rootMeshList.Meshes [rootVariation];
								MeshRenderer rMr = rootObj.AddComponent <MeshRenderer> ();
								rMr.material = RootMaterial;
								rMr.material.SetTexture ("_MainTex", RootTextures [rootType].Textures [rootTextureVariation]);


								int thornVariation = UnityEngine.Random.Range (0, ThornBaseMeshes.Meshes.Count);
								Mesh thornMesh = ThornBaseMeshes.Meshes [thornVariation];
								int thornTextureVariation = UnityEngine.Random.Range (0, ThornTextures.Count);

								int flowerType = UnityEngine.Random.Range (0, FlowerBaseMeshes.Count);
								int flowerVariation = UnityEngine.Random.Range (0, FlowerBaseMeshes [flowerType].Meshes.Count);
								////Debug.Log ("Getting flower type " + flowerType + " variation " + flowerVariation);
								Mesh flowerMesh = FlowerBaseMeshes [flowerType].Meshes [flowerVariation];
								int flowerTextureVariation = UnityEngine.Random.Range (0, FlowerTextures [flowerType].Textures.Count);

								List <STransform> thornList = bodyMeshList.ThornPoints [bodyVariation];
								List <STransform> flowerList = bodyMeshList.FlowerPoints [bodyVariation];
								foreach (STransform thornPoint in thornList) {
									GameObject thorn = plantObj.CreateChild ("Thorn").gameObject;
									thorn.AddComponent <MeshFilter> ().sharedMesh = thornMesh;
									MeshRenderer thornMr = thorn.AddComponent <MeshRenderer> ();
									thornMr.material = ThornMaterial;
									thornMr.material.SetTexture ("_MainTex", ThornTextures [thornTextureVariation]);
									thornPoint.ApplyTo (thorn.transform, false);
								}

								foreach (STransform flowerPoint in flowerList) {
									GameObject flower = plantObj.CreateChild ("Flower").gameObject;
									flower.AddComponent <MeshFilter> ().sharedMesh = flowerMesh;
									MeshRenderer fMr = flower.AddComponent <MeshRenderer> ();
									fMr.material = FlowerMaterial;
									fMr.material.SetTexture ("_MainTex", FlowerTextures [flowerType].Textures [flowerTextureVariation]);
									flowerPoint.ApplyTo (flower.transform, false);
								}
								plantObj.transform.position = new Vector3 (xPos, 0f, 0f);
								xPos += 1f;
							}
							*/
				}

				public bool PlantProps(string plantName, ref Plant plantProps)
				{
						if (plantName != null) {

								return mPlantPropsLookup.TryGetValue(plantName, out plantProps);
						}
						return false;
				}

				public bool InitializeWorldPlantGameObject(GameObject worldPlantGameObject, string plantName, TimeOfYear season)
				{	//takes a newly initialize world plant and creates / adds all the meshes to its states
						//plant states:
						//Raw - default
						//Cooked - after cooked
						//Dried - after picked
						Dictionary <TimeOfYear,GameObject> seasonLookup = null;
						GameObject plantPrototype = null;
						if (plantName != null && mPlantPrototypeLookup.TryGetValue(plantName, out seasonLookup)) {
								//Debug.Log ("Found plant " + plantName);
								if (seasonLookup.TryGetValue(season, out plantPrototype)) {
										//we can just instantiate the prototype, because it doesn't have anything tricky attached to it
										//no WIscripts etc.
										//find or create a copy of the prefab child in the plant child
										//then dupe all the properites from the mesh filter and renderer
										CreatePlantStateChild(plantPrototype, worldPlantGameObject, "Raw");
										CreatePlantStateChild(plantPrototype, worldPlantGameObject, "Cooked");
										//Debug.Log("Found season, creating state child");
										return true;
								}
						}
						//Debug.Log("Didn't find plant, or didn't find season " + season.ToString ());
						return false;
						//done!
				}

				public void InitializeWorldPlantFoodStuff(WorldPlant worldPlant, Plant plant)
				{
						//that will give us the right look - now let's update our edibles
						FoodStuff foodStuff = worldPlant.worlditem.Get <FoodStuff>();
						if (!foodStuff.HasProps) {
								foodStuff.State.PotentialProps.Clear();
								plant.RawProps.Name = "Raw";
								plant.RawProps.Type = FoodStuffEdibleType.Edible;
								plant.CookedProps.Name = "Cooked";
								plant.CookedProps.Type = FoodStuffEdibleType.Edible;
								foodStuff.State.PotentialProps.Add(plant.RawProps);
								foodStuff.State.PotentialProps.Add(plant.CookedProps);
								foodStuff.RefreshFoodStuffProps();
						}
				}

				protected GameObject CreatePlantStateChild(GameObject prototypeObject, GameObject plantObject, string stateName)
				{
						Transform prefabChild = prototypeObject.transform;
						GameObject plantChild = plantObject.FindOrCreateChild("Raw").gameObject;
						plantChild.layer = Globals.LayerNumWorldItemActive;
						plantChild.tag = "StateChild";
						//prefabChild.ApplyTo (plantChild.transform, false);

						MeshRenderer prefabMr = prefabChild.GetComponent <MeshRenderer>();
						MeshRenderer plantMr = plantChild.GetOrAdd <MeshRenderer>();
						plantMr.sharedMaterials = prefabMr.sharedMaterials;

						MeshFilter prefabMf = prefabChild.GetComponent <MeshFilter>();
						MeshFilter plantMf = plantChild.GetOrAdd <MeshFilter>();
						plantMf.sharedMesh = prefabMf.sharedMesh;

						BoxCollider prefabCol = prefabChild.GetComponent <BoxCollider>();
						BoxCollider plantCol = plantChild.GetOrAdd <BoxCollider>();
						plantCol.center = prefabCol.center;
						plantCol.size = prefabCol.size;

						return plantChild;
				}

				public List <WorldPlant> ActivePlants = new List <WorldPlant>();
				public Dictionary <WorldPlant, PlantInstanceTemplate> PlantInstanceMappings = new Dictionary <WorldPlant, PlantInstanceTemplate>();
				protected Dictionary <string, GameObject> mTerrainPlantLookup = new Dictionary<string, GameObject>();

				public void SpawnPlant(PlantInstanceTemplate plantInstanceTemplate, ClimateType climate, bool aboveGround, TimeOfYear timeOfYear)
				{

				}

				public static void InstantiateAllPrefabs()
				{
						foreach (GameObject plantPrefab in Get.TerrainPlantPrototypes) {
								GameObject.Instantiate(plantPrefab, Vector3.zero, Quaternion.identity);
						}
				}

				public static void SaveProps(Plant props)
				{
						Mods.Get.Runtime.SaveMod <Plant>(props, "Plant", props.Name);
				}

				public static void Dispose(WorldPlant worldPlant)
				{
						//let the plant instance mapping know that it no longer has a plant
						PlantInstanceTemplate plantInstance = null;
						if (Get.PlantInstanceMappings.TryGetValue(worldPlant, out plantInstance) && !plantInstance.IsEmpty) {
								//now that it has been picked
								//anything can grow there
								//let it know it's no longer planted
								plantInstance.PickedTime = WorldClock.AdjustedRealTime;//changes ReadyForPlant
								//plantInstance.NextGrowTime = WorldClock.AdjustedRealTime + Globals.PlantAutoRegrowInterval;
								plantInstance.HasInstance = false;
								//tell the parent chunk to save
								//plantInstance.ParentChunk.SavePlants ();
						}
						//send the old plant back to the nursury
						WIGroups.Get.Plants.AddChildItem(worldPlant.worlditem);
						worldPlant.worlditem.SetMode(WIMode.Hidden);
						worldPlant.transform.ResetLocal();
				}

				public static void Pick(WorldPlant worldPlant, bool addToInventory)
				{
						PlantState state = null;
						if (Mods.Get.Runtime.LoadMod <PlantState>(ref state, "Plant", PlantStateName(worldPlant.State.PlantName))) {
								state.NumTimesPicked++;
								state.NumTimesSpawned--;
								Mods.Get.Runtime.SaveMod <PlantState>(state, "Plant", PlantStateName(worldPlant.State.PlantName));
						}

						//check for thorns and prick the player if they're not wearing gloves
						if (worldPlant.Props.HasThorns) {
								if (!Player.Local.Wearables.IsWearing(WearableType.Armor, BodyPartType.Hand, BodyOrientation.Both)) {
										Get.ThornDamage.Target = Player.Local;
										DamageManager.Get.SendDamage(Get.ThornDamage);
								}
						}
						bool dispose = true;
						if (addToInventory) {
								//okay, first we get a stack item of the plant
								//don't destroy the original - just send it back to the nursury
								worldPlant.State.TimePicked = WorldClock.AdjustedRealTime;
								StackItem pickedPlant = worldPlant.worlditem.GetStackItem(WIMode.None);
								//then add the picked plant to the inventory
								//if we're successful, update general plant data
								WIStackError error = WIStackError.None;
								dispose = Player.Local.Inventory.AddItems(pickedPlant, ref error);
								//update the plant data so we know that an instance of this plant has been picked
						}

						if (dispose) {
								Dispose(worldPlant);
						}
				}
				//Temp
				public List <Plant> PlantList = new List<Plant>();
				public List <List <Plant>> BGPlantsByClimate = new List <List <Plant>>();
				public List <List <Plant>> AGPlantsByClimate = new List <List <Plant>>();
				public GameObject ThornPrefab;
				public List <GameObject> RootBasePrefabs = new List<GameObject>();
				public List <GameObject> FlowerBasePrefabs = new List<GameObject>();
				public List <GameObject> BodyBasePrefabs = new List<GameObject>();
				public PlantMeshList ThornBaseMeshes = new PlantMeshList();
				public List <PlantMeshList> RootBaseMeshes = new List <PlantMeshList>();
				public List <PlantMeshList> FlowerBaseMeshes = new List <PlantMeshList>();
				public List <PlantBodyMeshList> BodyBaseMeshes = new List <PlantBodyMeshList>();
				public List <Texture2D> ThornTextures = new List <Texture2D>();
				public List <TextureList> RootTextures = new List <TextureList>();
				public List <TextureList> FlowerTextures = new List <TextureList>();
				public List <TextureList> BodyTextures = new List <TextureList>();
				public Material RootMaterial;
				public Material BodyMaterial;
				public Material FlowerMaterial;
				public Material ThornMaterial;
				public int RootMaterialHash;
				public int BodyMaterialHash;
				public int FlowerMaterialHash;
				public int ThornMaterialHash;
				[FrontiersBitMaskAttribute("Climate")]
				public int ArcticClimateFlags;
				[FrontiersBitMaskAttribute("Climate")]
				public int TropicalCoastClimateFlags;
				[FrontiersBitMaskAttribute("Climate")]
				public int WetlandClimateFlags;
				[FrontiersBitMaskAttribute("Climate")]
				public int DesertClimateFlags;
				[FrontiersBitMaskAttribute("Climate")]
				public int TemperateClimateFlags;
				public System.Random ElevationNumberGenerator = null;

				public IEnumerator UpdateWorldPlants()
				{	//update the plants surrounding the player
						while (!GameWorld.Get.ChunksLoaded) {
								yield return null;
						}
						ElevationNumberGenerator = new System.Random(Profile.Get.CurrentGame.Seed);
						int currentIndex = 0;
						while (GameWorld.Get.ChunksLoaded) {
								while (!GameManager.Is(FGameState.InGame) || !mPlantsLoaded) {
										yield return null;
								}
								//get all the now-irrelevant instances
								var enumerator = PlantAssigner.FindIrrelevantInstances(Player.Local, this);
								while (enumerator.MoveNext()) {
										//foreach (WorldPlant irrelevantInstance in PlantAssigner.FindIrrelevantInstances (Player.Local, this)) {
										irrelevantInstance = enumerator.Current;
										//for each one, get the closest tree in need of a tree
										PlantInstanceTemplate closestPlant = PlantAssigner.FindClosestPlantRequiringInstance(Player.Local, GameWorld.Get);
										//set the instance position to the closest tree position
										if (closestPlant != PlantInstanceTemplate.Empty) {
												if (closestPlant.Climate < 0 || closestPlant.Climate > AGPlantsByClimate.Count) {
														//the climate hasn't been assigned yet - do that now
//														Color32 regionData = GameWorld.Get.RegionDataAtPosition(closestPlant.Position);
//														closestPlant.Climate = regionData.b;
														//use the player's climate instead, odds of them being different are slim
														closestPlant.Climate = GameWorld.Get.CurrentRegionData.b;
												}
												//get the plants by climate
												List <Plant> plantsByClimate = null;
												if (closestPlant.AboveGround && closestPlant.Climate < AGPlantsByClimate.Count) {
														plantsByClimate = AGPlantsByClimate[closestPlant.Climate];
												} else if (closestPlant.Climate < BGPlantsByClimate.Count) {
														plantsByClimate = BGPlantsByClimate[closestPlant.Climate];
												}
												//elevation determines probability of spawning in this spot
												//for each plant we generate a random number
												//if that number is less than the distance between the elevations
												//it will spawn
												if (plantsByClimate != null && plantsByClimate.Count > 0) {
														Plant currentPlant = null;
														currentIndex++;
														int elevationCheck = 0;
														int elevationDifference = 0;
														int plantElevation = (int)closestPlant.Y;
														string plantName = null;

														if (closestPlant.AboveGround) {
																elevationCheck = ElevationNumberGenerator.Next(Globals.ElevationLow, Globals.ElevationHigh);
														}

														for (int i = 0; i < plantsByClimate.Count; i++) {
																//go through the plants once - of we find nothing, keep it empty
																currentIndex = plantsByClimate.NextIndex(currentIndex);
																currentPlant = plantsByClimate[currentIndex];
																if (currentPlant.AboveGround) {
																		//if we're above ground do an elevation check
																		elevationDifference = Mathf.Abs(Plant.ElevationTypeToInt(currentPlant.Elevation) - plantElevation);
																		if (elevationDifference > elevationCheck) {
																				//the difference is too high, skip this one, it's not for us
																				continue;
																		}
																}
																//TODO other checks here
																plantName = currentPlant.Name;
																break;
														}

														if (string.IsNullOrEmpty(plantName)) {
																//couldn't find a plant, oh well!
																continue;
														}

														closestPlant.HasInstance = true;
														closestPlant.PlantName = plantName;
														if (closestPlant.PlantedTime < 0) {
																closestPlant.PlantedTime = WorldClock.AdjustedRealTime;
														}

														irrelevantInstance.transform.position = closestPlant.Position;
														irrelevantInstance.worlditem.SetMode(WIMode.Frozen);
														irrelevantInstance.State.TimePicked = -1f;//just in case
														//update the plant data
														irrelevantInstance.State.PlantName = plantName;
														irrelevantInstance.RefreshPlantProps();
														//we can guarantee that instance mappings will have an entry
														//but we don't know if it will be null or not
														PlantInstanceTemplate existingPlant = PlantInstanceMappings[irrelevantInstance];
														if (existingPlant != null) {
																//if it's not null, let it know that it has no instance
																existingPlant.HasInstance = false;
														}
														//update the reference
														PlantInstanceMappings[irrelevantInstance] = closestPlant;
												}
										}
										//wait a tick
										yield return null;
								}

								double waitUntil = WorldClock.AdjustedRealTime + 0.1f;
								while (WorldClock.AdjustedRealTime < waitUntil) {
										yield return null;
								}
						}
						yield break;
				}

				WorldPlant irrelevantInstance;

				public IEnumerator BuildPlantMeshes(Plant plant)
				{
						foreach (PlantSeasonalSettings seasonalSettings in plant.SeasonalSettings) {
								var buildPlantSeasonalMesh = BuildPlantSeasonalMesh(plant, seasonalSettings);
								while (buildPlantSeasonalMesh.MoveNext()) {
										yield return buildPlantSeasonalMesh.Current;
								}
						}
						yield break;
				}

				protected IEnumerator BuildPlantSeasonalMesh(Plant plant, PlantSeasonalSettings seasonalSettings)
				{
						PlantCombiner.ClearMeshes();
						PlantCombiner.ClearMaterials();

						gPlantBuilderHelper.localRotation = Quaternion.identity;
						gPlantBuilderHelper.localPosition = Vector3.zero;

						MeshCombiner.MeshInput rootInput = new MeshCombiner.MeshInput();
						gPlantBuilderHelper.localScale = Vector3.one * (int)plant.RootSize;
						rootInput.Mesh = RootBaseMeshes[(int)plant.RootType].BufferedMeshes[plant.RootVariation];
						rootInput.ScaleInverted = false;
						rootInput.WorldMatrix = gPlantBuilderHelper.localToWorldMatrix;
						rootInput.Materials = new int [] { RootMaterialHash };
						PlantCombiner.AddMesh(rootInput);

						MeshCombiner.MeshInput bodyInput = new MeshCombiner.MeshInput();
						gPlantBuilderHelper.localScale = Vector3.one * (int)seasonalSettings.BodyHeight;
						bodyInput.Mesh = BodyBaseMeshes[plant.BodyType].BufferedMeshes[plant.BodyVariation];
						bodyInput.ScaleInverted = false;
						bodyInput.WorldMatrix = gPlantBuilderHelper.localToWorldMatrix;
						bodyInput.Materials = new int [] { BodyMaterialHash };
						PlantCombiner.AddMesh(bodyInput);

						//leave the plant builder helper alone now
						//its job is to transform the flower and thorn points

						gPlantFlowerBuilderHelper.transform.parent = gPlantBuilderHelper;
						gPlantFlowerBuilderHelper.transform.ResetLocal();

						MeshCombiner.BufferedMesh flowerBaseMesh = null;
						if (seasonalSettings.Flowers) {
								//plant.FlowerVariation = Mathf.Clamp (plant.FlowerVariation, 0, FlowerBaseMeshes
								try {
										List <STransform> flowerPoints = BodyBaseMeshes[plant.BodyType].FlowerPoints[plant.BodyVariation];
										PlantMeshList flowerBaseMeshList = FlowerBaseMeshes[Mathf.Clamp(plant.FlowerType, 0, FlowerBaseMeshes.Count - 1)];
										if (flowerBaseMeshList.BufferedMeshes.Count > 0) {
												flowerBaseMesh = flowerBaseMeshList.BufferedMeshes[Mathf.Clamp(plant.FlowerVariation, 0, flowerBaseMeshList.BufferedMeshes.Count - 1)];
												int totalFlowerPoints = Mathf.CeilToInt(flowerPoints.Count * seasonalSettings.FlowerDensity);
												gPlantFlowerBuilderHelper.localScale = Vector3.one * Plant.FlowerSizeToFloat(seasonalSettings.FlowerSize);
												for (int i = 0; i < totalFlowerPoints; i++) {
														STransform flowerPoint = flowerPoints[i];
														flowerPoint.ApplyTo(gPlantFlowerBuilderHelper, false);

														MeshCombiner.MeshInput flowerInput = new MeshCombiner.MeshInput();
														flowerInput.Mesh = flowerBaseMesh;
														flowerInput.ScaleInverted = false;
														flowerInput.WorldMatrix = gPlantFlowerBuilderHelper.localToWorldMatrix;
														flowerInput.Materials = new int [] { FlowerMaterialHash };
														PlantCombiner.AddMesh(flowerInput);
												}
										}
								} catch (Exception e) {
										Debug.LogException(e);
								}
						}

						if (plant.HasThorns) {
								List <STransform> thornPoints = BodyBaseMeshes[plant.BodyType].ThornPoints[plant.BodyVariation];
								gPlantBuilderHelper.localScale = Vector3.one;
								foreach (STransform thornPoint in thornPoints) {
										thornPoint.ApplyTo(gPlantFlowerBuilderHelper, false);

										MeshCombiner.MeshInput thornInput = new MeshCombiner.MeshInput();
										thornInput.Mesh = ThornBaseMeshes.BufferedMeshes[plant.ThornVariation];
										thornInput.ScaleInverted = false;
										thornInput.WorldMatrix = gPlantFlowerBuilderHelper.localToWorldMatrix;
										thornInput.Materials = new int [] { ThornMaterialHash };
										PlantCombiner.AddMesh(thornInput);
								}
						}

						PlantCombiner.Combine(PlantCombinerCallback);
						mWaitingForCallback = true;
						while (mWaitingForCallback) {
								PlantCombiner.Check();
								yield return null;
						}
						//get the final mesh object from the combiner
						if (mCurrentMeshOutputs != null && mCurrentMeshOutputs.Length > 0) {
								MeshCombiner.MeshObject newPlantMesh = PlantCombiner.CreateMeshObject(mCurrentMeshOutputs[0], mPlantMaterialLookup);
								//create a prototype and add it to the lookup
								GameObject newPlantPrototype = Prototypes.CreateChild(plant.Name + seasonalSettings.Seasonality.ToString()).gameObject;
								newPlantPrototype.transform.localPosition = new Vector3(mPrototypPositionX, 0f, 0f);
								mPrototypPositionX += 1.5f;
								newPlantPrototype.AddComponent <MeshFilter>().mesh = newPlantMesh.Mesh;
								MeshRenderer mr = newPlantPrototype.AddComponent <MeshRenderer>();
								mr.enabled = false;
								mr.materials = newPlantMesh.Materials;
								//this will create material instances and that's OK
								//temp

								float saturation = 1.0f;
								foreach (Material mat in mr.materials) {
										if (mat.name.Contains("Body")) {
												mat.SetTexture("_MainTex", BodyTextures[plant.BodyType].Textures[plant.BodyTexture]);
												mat.SetColor("_Color", Colors.ShiftHue(Color.red, seasonalSettings.BodyHueShift, 0.1f));
										} else if (mat.name.Contains("Root")) {
												mat.SetTexture("_MainTex", RootTextures[(int)plant.RootType].Textures[plant.RootTexture]);
												mat.SetColor("_Color", Colors.ShiftHue(Color.red, plant.RootHueShift, 0.1f));
										} else if (mat.name.Contains("Flower")) {
												mat.SetTexture("_MainTex", FlowerTextures[plant.FlowerType].Textures[plant.FlowerTexture]);
												mat.SetTexture("_MaskTex", FlowerTextures[plant.FlowerType].MaskTextures[plant.FlowerTexture]);
												mat.SetColor("_EyeColor", Colors.ShiftHue(Color.red, seasonalSettings.FlowerHueShift, saturation)); //R channel
										} else if (mat.name.Contains("Thorn")) {
												mat.SetTexture("_MainTex", ThornTextures[plant.ThornTexture]);
										}
								}
								BoxCollider bc = newPlantPrototype.AddComponent <BoxCollider>();
								bc.enabled = false;

								//save plant for later
								Dictionary <TimeOfYear, GameObject> seasonalLookup = null;
								List <TimeOfYear> seasonToAdd = new List<TimeOfYear>();
								//split wetlands times of year into separate season
								if (seasonalSettings.Seasonality == TimeOfYear.WetSeason) {
										seasonToAdd.Add(TimeOfYear.SeasonWinter);
										seasonToAdd.Add(TimeOfYear.SeasonSpring);
								} else if (seasonalSettings.Seasonality == TimeOfYear.DrySeason) {
										seasonToAdd.Add(TimeOfYear.SeasonAutumn);
										seasonToAdd.Add(TimeOfYear.SeasonSummer);
								} else {
										seasonToAdd.Add(seasonalSettings.Seasonality);
								}

								foreach (TimeOfYear toy in seasonToAdd) {
										if (!mPlantPrototypeLookup.TryGetValue(plant.Name, out seasonalLookup)) {
												seasonalLookup = new Dictionary<TimeOfYear, GameObject>();
												mPlantPrototypeLookup.Add(plant.Name, seasonalLookup);
										}
										seasonalLookup.Add(toy, newPlantPrototype);
								}
						}

						//clear the combiner for the next batch
						PlantCombiner.ClearMeshes();
						PlantCombiner.ClearMaterials();
						mCurrentMeshOutputs = null;
						mCurrentHash = 0;
						yield break;
				}

				protected MeshCombiner.BufferedMesh CreateBufferedMesh(Mesh mesh)
				{
						MeshCombiner.BufferedMesh bufferedMesh = new MeshCombiner.BufferedMesh();
						bufferedMesh.Name = mesh.name;
						bufferedMesh.Vertices = mesh.vertices;
						bufferedMesh.Normals = mesh.normals;
						bufferedMesh.Colors = mesh.colors;
						bufferedMesh.Tangents = mesh.tangents;
						bufferedMesh.UV = mesh.uv;
						bufferedMesh.UV1 = mesh.uv1;
						bufferedMesh.UV2 = mesh.uv2;

						bufferedMesh.Topology = new MeshTopology [mesh.subMeshCount];

						for (var i = 0; i < mesh.subMeshCount; i++) {
								bufferedMesh.Topology[i] = mesh.GetTopology(i);

								// Check for Unsupported Mesh Topology
								switch (bufferedMesh.Topology[i]) {
										case MeshTopology.Lines:
										case MeshTopology.LineStrip:
										case MeshTopology.Points:
												Debug.LogWarning("The MeshCombiner does not support this meshes (" + bufferedMesh.Name + "topology (" + bufferedMesh.Topology[i] + ")");
												break;
								}
								bufferedMesh.Indexes.Add(mesh.GetIndices(i));
						}
						return bufferedMesh;
				}

				protected IEnumerator LoadPlantDataAndBuildMeshes()
				{
						Mods.Get.Runtime.LoadAvailableMods(PlantList, "Plant");
						SortPlantsByClimateAndSeason();
						//create the meshes for the plant
						for (int i = 0; i < PlantList.Count; i++) {
								var buildPlantMeshes = BuildPlantMeshes(PlantList[i]);
								while (buildPlantMeshes.MoveNext()) {
										yield return buildPlantMeshes.Current;
								}
						}
						mPlantsLoaded = true;
						yield break;
				}

				protected void SortPlantsByClimateAndSeason()
				{
						Plant plant = null;
						int[] climateTypes = null;
						List <List <Plant>> plantsByClimate = null;
						int rarity = 0;
						mPlantPropsLookup.Clear();

						for (int i = 0; i < PlantList.Count; i++) {
								plant = PlantList[i];
								//add the plant props to our name lookup
								mPlantPropsLookup.Add(plant.Name, plant);
								//set the climate flags based on climate property
								switch (plant.Climate) {
										case ClimateType.Arctic:
												plant.ClimateFlags = ArcticClimateFlags;
												break;

										case ClimateType.Temperate:
										default:
												plant.ClimateFlags = TemperateClimateFlags;
												break;

										case ClimateType.TropicalCoast:
												plant.ClimateFlags = TropicalCoastClimateFlags;
												break;

										case ClimateType.Wetland:
												plant.ClimateFlags = WetlandClimateFlags;
												break;

										case ClimateType.Desert:
												plant.ClimateFlags = DesertClimateFlags;
												break;
								}

								for (int j = 0; j < plant.SeasonalSettings.Count; j++) {
										plant.Seasonality |= plant.SeasonalSettings[j].Seasonality;
										if (Flags.Check((uint)WorldClock.SeasonCurrent, (uint)plant.SeasonalSettings[j].Seasonality, Flags.CheckType.MatchAny)) {
												plant.CurrentSeason = plant.SeasonalSettings[j];
												break;
										}
								}

								if (plant.CurrentSeason != null) {
										//Debug.Log ("Adding plant " + plant.Name + " to seasonal lookup, it's in season");
										//sort the plant by climate
										rarity = WIFlags.RarityToInt(plant.Rarity, 5);
										climateTypes = FlagSet.GetFlagValues(plant.ClimateFlags);
										if (plant.AboveGround) {
												plantsByClimate = AGPlantsByClimate;
										} else {
												plantsByClimate = BGPlantsByClimate;
										}
										for (int j = 0; j < climateTypes.Length; j++) {
												int climateType = climateTypes[j];
												//add enough to the list to accomodate this climate type
												if (climateType >= plantsByClimate.Count) {
														for (int k = plantsByClimate.LastIndex(); k <= climateType; k++) {
																plantsByClimate.Add(new List <Plant>());
														}
												}
												//add it to the list for this climate a number of times determined by rarity
												for (int k = 0; k < rarity; k++) {
														plantsByClimate[climateType].Add(plant);
												}
										}
								}
						}
						//shuffle the plants so they appear in a random non-alphabetized order
						System.Random orderShuffler = new System.Random(Profile.Get.CurrentGame.Seed);
						for (int i = 0; i < plantsByClimate.Count; i++) {
								plantsByClimate[i].Shuffle(orderShuffler);
						}
				}

				public void PlantCombinerCallback(int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
				{
						//////Debug.Log ("Got the callback");
						mCurrentHash = hash;
						mCurrentMeshOutputs = meshOutputs;
						mWaitingForCallback = false;
				}
				//this is where we store all the pre-built plant meshes
				protected List <string> mPlantNames = new List<string>();
				protected Dictionary <string, Plant> mPlantPropsLookup = new Dictionary<string, Plant>();
				protected Dictionary <string, Dictionary <TimeOfYear, GameObject>> mPlantPrototypeLookup = new Dictionary <string, Dictionary <TimeOfYear, GameObject>>();
				protected MeshCombiner PlantCombiner = new MeshCombiner();
				protected Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] mCurrentMeshOutputs;
				protected Dictionary <int,Material> mPlantMaterialLookup = new Dictionary<int, Material>();
				protected int mCurrentHash = 0;
				protected bool mWaitingForCallback = false;
				protected float mPrototypPositionX = -8000f;
				protected bool mPlantsLoaded = false;
				protected Transform gPlantFlowerBuilderHelper;
				protected Transform gPlantBuilderHelper;

				public static void Examine(Plant props, List<WIExamineInfo> examine)
				{		//just create a bunch of garbage, i don't care right now
						//we'll clean this up later
						List <string> unknownTimesOfYear = new List <string>() {
								TimeOfYear.SeasonSummer.ToString(),
								TimeOfYear.SeasonAutumn.ToString(),
								TimeOfYear.SeasonWinter.ToString(),
								TimeOfYear.SeasonSpring.ToString()
						};
						List <string> knownTimesOfYear = new List <string>();
						List <string> readTimesOfyear = new List <string>();

						List <string> unknownFlowerTimesOfYear = new List <string>() {
								TimeOfYear.SeasonSummer.ToString(),
								TimeOfYear.SeasonAutumn.ToString(),
								TimeOfYear.SeasonWinter.ToString(),
								TimeOfYear.SeasonSpring.ToString()
						};
						List <string> knownFlowerTimesOfYear = new List <string>();
						List <string> readFlowerTimesOfYear = new List <string>();

						for (int i = 0; i < props.SeasonalSettings.Count; i++) {
								bool knowsTimeOfYear = Flags.Check((uint)props.EncounteredTimesOfYear, (uint)props.SeasonalSettings[i].Seasonality, Flags.CheckType.MatchAny);
								//check to see if we've encountered it in this time of year
								switch (props.SeasonalSettings[i].Seasonality) {
										case TimeOfYear.WetSeason:
												readTimesOfyear.Add(TimeOfYear.SeasonWinter.ToString());
												readTimesOfyear.Add(TimeOfYear.SeasonSpring.ToString());
												if (props.SeasonalSettings[i].Flowers) {
														readFlowerTimesOfYear.Add(TimeOfYear.SeasonWinter.ToString());
														readFlowerTimesOfYear.Add(TimeOfYear.SeasonSpring.ToString());
												}
												if (knowsTimeOfYear) {
														knownTimesOfYear.Add(TimeOfYear.SeasonWinter.ToString());
														knownTimesOfYear.Add(TimeOfYear.SeasonSpring.ToString());
														unknownTimesOfYear.Remove(TimeOfYear.SeasonWinter.ToString());
														unknownTimesOfYear.Remove(TimeOfYear.SeasonSpring.ToString());
														unknownFlowerTimesOfYear.Remove(TimeOfYear.SeasonWinter.ToString());
														unknownFlowerTimesOfYear.Remove(TimeOfYear.SeasonSpring.ToString());
												}
												break;

										case TimeOfYear.DrySeason:
												readTimesOfyear.Add(TimeOfYear.SeasonSummer.ToString());
												readTimesOfyear.Add(TimeOfYear.SeasonAutumn.ToString());
												if (props.SeasonalSettings[i].Flowers) {
														readFlowerTimesOfYear.Add(TimeOfYear.SeasonSummer.ToString());
														readFlowerTimesOfYear.Add(TimeOfYear.SeasonAutumn.ToString());
												}
												if (knowsTimeOfYear) {
														knownTimesOfYear.Add(TimeOfYear.SeasonSummer.ToString());
														knownTimesOfYear.Add(TimeOfYear.SeasonAutumn.ToString());
														unknownTimesOfYear.Remove(TimeOfYear.SeasonSummer.ToString());
														unknownTimesOfYear.Remove(TimeOfYear.SeasonAutumn.ToString());
														unknownFlowerTimesOfYear.Remove(TimeOfYear.SeasonSummer.ToString());
														unknownFlowerTimesOfYear.Remove(TimeOfYear.SeasonAutumn.ToString());
												}
												break;

										default:
												readTimesOfyear.Add(props.SeasonalSettings[i].Seasonality.ToString());
												if (props.SeasonalSettings[i].Flowers) {
														readFlowerTimesOfYear.Add(props.SeasonalSettings[i].Seasonality.ToString());
												}
												if (knowsTimeOfYear) {
														knownTimesOfYear.Add(props.SeasonalSettings[i].Seasonality.ToString());
														unknownTimesOfYear.Remove(props.SeasonalSettings[i].Seasonality.ToString());
														unknownFlowerTimesOfYear.Remove(props.SeasonalSettings[i].Seasonality.ToString());
														if (props.SeasonalSettings[i].Flowers) {
																knownFlowerTimesOfYear.Add(props.SeasonalSettings[i].Seasonality.ToString());
														}
												}
												break;
								}
						}

						mGeneralInfo.StaticExamineMessage = props.CommonName + ", formally known as " + props.ScientificName;
						mGeneralInfo.ExamineMessageOnFail = "This " + props.Climate.ToString() + " plant is called " + props.CommonName + ", but I don't know its scientific name.";
						mGeneralInfo.StaticExamineMessage += (" It grows in " + GameData.CommaJoinWithLast(readTimesOfyear, "and").Replace("Season", "") + ".");
						if (readFlowerTimesOfYear.Count > 0) {
								mGeneralInfo.StaticExamineMessage += (" It also flowers in "
								+ GameData.CommaJoinWithLast(readFlowerTimesOfYear, "and").Replace("Season", "")) + ".";
						}
						if (knownTimesOfYear.Count > 0) {
								mGeneralInfo.ExamineMessageOnFail += (" I know that it grows in "
								+ GameData.CommaJoinWithLast(knownTimesOfYear, "and").Replace("Season", "") + ", but I'm not sure whether it grows in "
								+ GameData.CommaJoinWithLast(unknownTimesOfYear, "or").Replace("Season", "") + ".");
								if (knownFlowerTimesOfYear.Count > 0) {
										mGeneralInfo.ExamineMessageOnFail += (" I also know that it flowers in "
										+ GameData.CommaJoinWithLast(knownFlowerTimesOfYear, "and").Replace("Season", "") + " but I'm not sure about "
										+ GameData.CommaJoinWithLast(unknownFlowerTimesOfYear, "or").Replace("Season", "") + ".");
								}
						} else {
								mGeneralInfo.ExamineMessageOnFail += " I'm not sure when it grows or when it flowers.";
						}

						if (!props.AboveGround) {
								mGeneralInfo.StaticExamineMessage += " It grows below ground.";
								mGeneralInfo.ExamineMessageOnFail += " It grows below ground.";
						}

						if (props.EncounteredTimesOfYear == TimeOfYear.None) {
								mGeneralInfo.StaticExamineMessage += "\nI've never encountered this plant in the wild.";
								mGeneralInfo.ExamineMessageOnFail += "\nI've never encountered this plant in the wild.";
						}

						mEdibleInfo.StaticExamineMessage = "";
						mEdibleInfo.ExamineMessageOnFail = "";

						mEdibleInfo.StaticExamineMessage += FoodStuff.DescribeProperties(props.RawProps, "raw") + " ";
						if (props.RawPropsRevealed) {
								mEdibleInfo.ExamineMessageOnFail += FoodStuff.DescribeProperties(props.RawProps, "raw") + " ";
						} else {
								mEdibleInfo.ExamineMessageOnFail += "I'm not sure whether it's edible when raw. ";
						}

						mEdibleInfo.StaticExamineMessage += FoodStuff.DescribeProperties(props.CookedProps, "cooked");
						if (props.CookedPropsRevealed) {
								mEdibleInfo.ExamineMessageOnFail += FoodStuff.DescribeProperties(props.CookedProps, "cooked");
						} else {
								if (!props.RawPropsRevealed) {
										mEdibleInfo.ExamineMessageOnFail += " I'm also not sure whether it's edible when cooked.";
								} else {
										mEdibleInfo.ExamineMessageOnFail += " I'm not sure whether it's edible when cooked.";
								}
						}

						mGeneralInfo.RequiredSkill = "Gathering";
						mGeneralInfo.RequiredSkillUsageLevel = MinimumGatheringSkillToRevealBasicProps;
						mEdibleInfo.RequiredSkill = "Gathering";
						mEdibleInfo.RequiredSkillUsageLevel = MinimumGatheringSkillToRevealEdibleProps;

						examine.Add(mGeneralInfo);
						examine.Add(mEdibleInfo);
				}

				protected static StringBuilder mExamineString = new StringBuilder();
				protected static WIExamineInfo mGeneralInfo;
				protected static WIExamineInfo mEdibleInfo;

				protected static string PlantStateName(string plantName)
				{
						return plantName + "-State"; 
				}
		}

		[Serializable]
		public class MeshList
		{
				public List <Mesh> Meshes = new List<Mesh>();
		}

		[Serializable]
		public class TextureList
		{
				public List <Texture2D> Textures = new List<Texture2D>();
				public List <Texture2D> MaskTextures = new List<Texture2D>();
		}

		[Serializable]
		public class PlantBodyMeshList
		{
				public List <Mesh> Meshes = new List <Mesh>();
				public List <List <STransform>> FlowerPoints = new List<List<STransform>>();
				public List <List <STransform>> ThornPoints = new List<List<STransform>>();
				[HideInInspector]
				public List <MeshCombiner.BufferedMesh> BufferedMeshes = new List<MeshCombiner.BufferedMesh>();
		}

		[Serializable]
		public class PlantMeshList
		{
				public List <Mesh> Meshes = new List <Mesh>();
				[HideInInspector]
				public List <MeshCombiner.BufferedMesh> BufferedMeshes = new List<MeshCombiner.BufferedMesh>();
		}
}
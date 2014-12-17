using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using Hydrogen.Threading.Jobs;
using System.IO;

namespace Frontiers
{
		public partial class Structures : Manager
		{
				public static Structures Get;

				public static bool StructureShadows = true;
				public static bool SceneryObjectShadows = true;
				public static List <Structure> ExteriorsWaitingToLoad;
				public static List <Structure> InteriorsWaitingToLoad;
				public static List <MinorStructure> MinorsWaitingToLoad;
				public static List <Structure> LoadedStructures;
				public static List <Structure> InteriorsWaitingToUnload;
				public static List <Structure> ExteriorsWaitingToUnload;
				public static List <MinorStructure> MinorsWaitingToUnload;
				public static List <MinorStructure> LoadedMinorStructures;
				public GameObject EditorStructureParent;
				public string EditorSelectedPack;
				public string EditorSelectedItem;
				//builders for generating exterior, interior and minor structures
				public StructureBuilder ExteriorBuilder;
				public StructureBuilder InteriorBuilder;
				public StructureBuilder MinorBuilder;
				public MeshCombiner ExteriorLODCombiner;
				public MeshCombiner ExteriorLODCombinerDestroyed;
				public MeshCombiner MinorLODCombiner;
				//super-awesome combiners for combining meshes
				public MeshCombiner ExteriorCombiner;
				public MeshCombiner ExteriorCombinerDestroyed;
				public MeshCombiner InteriorCombiner;
				public MeshCombiner InteriorCombinerDestroyed;
				public MeshCombiner MinorCombiner;
				public MeshCombiner MinorCombinerDestroyed;
				public MeshCombiner MinorLODCombinerDestroyed;
				//super awesome combiners for combining collision meshes
				public MeshCombiner ExteriorColliderCombiner;
				public MeshCombiner InteriorColliderCombiner;
				public MeshCombiner MinorColliderCombiner;

				#if UNITY_EDITOR
				//data for loading/refreshing structure packs
				public string LocalStructurePacksPath;
				public string LocalColliderMeshesPath;
				public string LocalLODMeshesPath;
				public string LocalLODPrefabsPath;
				public string LocalSharedMaterialsPath;
				public List <string> ColliderMeshPaths;
				public List <string> LODMeshPaths;
				public List <string> LODPrefabPaths;
				public List <string> SharedMaterialPaths;
				public string MaterialsFolder;
				public string StaticPrefabsFolder;
				public string DynamicPrefabsFolder;
				public string MeshesFolder;
				public string TexturesFolder;
				#endif

				public List <Dynamic> DynamicTargets;
				public List <StructurePackPaths> PackPaths;
				public List <Material> SharedMaterials;
				public List <GameObject> ColliderMeshPrefabs;
				public List <GameObject> LodMeshPrefabs;
				public Stack <BoxCollider> BoxColliderPool;
				public Stack <MeshCollider> MeshColliderPool;
				public Stack <ChunkPrefabObject> SceneryObjectPool;
				public List <ChunkPrefabObject> SceneryObjects;
				public List <WorldItemPack> DynamicWorldItemPacks;
				public List <StructurePack> StructurePacks;
				public Transform ColliderKeeper;
				public Mesh DefaultMeshColliderMesh;
				public Texture2D DefaultDetailTexture;
				public Texture2D DefaultDetailBump;
				public Texture2D DefaultBump;

				public MeshCollider MeshColliderFromPool()
				{
						if (MeshColliderPool.Count > 0) {
								return MeshColliderPool.Pop();
						} else {
								//TODO implement a proper pool with a max object count
								//this needs to be watched closely to avoid out of memory errors
								return CreateMeshCollider();
						}
				}

				public BoxCollider BoxColliderFromPool()
				{
						if (BoxColliderPool.Count > 0) {
								return BoxColliderPool.Pop();
						} else {
								//TODO implement a proper pool with a max object count
								//this needs to be watched closely to avoid out of memory errors
								return CreateBoxCollider();
						}
				}

				public Dynamic DynamicTarget(string name)
				{
						Dynamic dynamicTarget = null;
						foreach (Dynamic target in DynamicTargets) {
								if (target.State.MakePublic && target.State.Name == name) {
										dynamicTarget = target;
										break;
								}
						}
						return dynamicTarget;
				}

				protected Dictionary <string, StructurePack> mStructurePackLookup;
				protected Dictionary <string, Material> mSharedMaterialLookup;
				protected Dictionary <string, Mesh> mColliderMeshes;
				protected Dictionary <string, Mesh> mLodMeshes;
				protected Dictionary <string, Mesh> mGeneratedMeshes;

				#region initialization

				public override void WakeUp()
				{
						if (mIsAwake)//the editor will call this sometimes
				return;

						Get = this;

						ExteriorsWaitingToLoad = new List <Structure>();
						InteriorsWaitingToLoad = new List <Structure>();
						MinorsWaitingToLoad = new List <MinorStructure>();
						LoadedStructures = new List <Structure>();
						LoadedMinorStructures = new List <MinorStructure>();
						InteriorsWaitingToUnload = new List <Structure>();
						ExteriorsWaitingToUnload = new List <Structure>();
						MinorsWaitingToUnload = new List <MinorStructure>();

						mStructurePackLookup = new Dictionary <string, StructurePack>();
						mSharedMaterialLookup = new Dictionary <string, Material>();
						mColliderMeshes = new Dictionary <string, Mesh>();
						mLodMeshes = new Dictionary <string, Mesh>();
						mGeneratedMeshes = new Dictionary <string, Mesh>();

						ColliderKeeper.rigidbody.detectCollisions = false;
						ColliderKeeper.rigidbody.isKinematic = true;
						ColliderKeeper.gameObject.layer = Globals.LayerNumHidden;
						ColliderKeeper.gameObject.SetActive(false);

						mSharedMaterialLookup.Clear();
						mStructurePackLookup.Clear();
						mColliderMeshes.Clear();
						mLodMeshes.Clear();

						SceneryObjectPool = new Stack <ChunkPrefabObject>();
						BoxColliderPool = new Stack<BoxCollider>();
						MeshColliderPool = new Stack<MeshCollider>();

						DynamicWorldItemPacks.Clear();

						for (int i = SharedMaterials.LastIndex(); i >= 0; i--) {
								if (SharedMaterials[i] == null) {
										SharedMaterials.RemoveAt(i);
								} else {
										mSharedMaterialLookup.Add(SharedMaterials[i].name, SharedMaterials[i]);
								}
						}

						for (int i = 0; i < ColliderMeshPrefabs.Count; i++) {
								//name_COL
								GameObject colliderMeshPrefab = ColliderMeshPrefabs[i];
								MeshFilter mf = colliderMeshPrefab.GetComponent <MeshFilter>();

								if (mf == null) {
										Debug.Log ("MESH FILTER WAS NULL ON COLLIDER " + colliderMeshPrefab.name);
								} else {
										Mesh colliderMesh = mf.sharedMesh;
										string lookupName = colliderMeshPrefab.name.Replace("_COL", "");
										//Debug.Log ("Adding collider lookup " + lookupName);
										mColliderMeshes.Add(lookupName, colliderMesh);
								}
						}

						for (int i = 0; i < LodMeshPrefabs.Count; i++) {
								//name_LOD
								GameObject lodMeshPrefab = LodMeshPrefabs[i];
								MeshFilter mf = lodMeshPrefab.GetComponent <MeshFilter>();

								if (mf == null) {
										Debug.Log ("MESH FILTER WAS NULL ON LOD " + lodMeshPrefab.name);
								} else {
										Mesh lodMesh = mf.sharedMesh;
										string lookupName = lodMeshPrefab.name;
										int lodNumber = 0;
										if (lookupName.Contains("_LOD1")) {
												lodNumber = 1;
										} else if (lookupName.Contains("_LOD2")) {
												lodNumber = 2;
										} else if (lookupName.Contains("_LOD3")) {
												lodNumber = 3;
										}

										lookupName = lookupName.Replace("_LOD1", "");
										lookupName = lookupName.Replace("_LOD2", "");
										lookupName = lookupName.Replace("_LOD3", "");

										Mesh existingLodMesh = null;
										if (mLodMeshes.ContainsKey(lookupName)) {
												//Debug.Log ("FOUND DUPLICATE LOD: " + lodMeshPrefab.name);
										} else {
												mLodMeshes.Add(lookupName, lodMesh);
										}
								}
						}

						foreach (KeyValuePair <string, Mesh> colliderMesh in mColliderMeshes) {
								//collider meshes can double as lod meshes
								//so if we don't find one in the lod, substitute it
								if (!mLodMeshes.ContainsKey(colliderMesh.Key)) {
										//Debug.Log ("SUBSTITUTING " + colliderMesh.Key + " FOR LOD MESH");
										mLodMeshes.Add(colliderMesh.Key, colliderMesh.Value);
								}
						}

						//add the regular colliders to the lookup too
						foreach (StructurePack pack in StructurePacks) {
								foreach (GameObject prefab in pack.StaticPrefabs) {
										if (!mColliderMeshes.ContainsKey(prefab.name)) {
												Mesh lodMesh = null;
												if (mLodMeshes.TryGetValue(prefab.name, out lodMesh)) {
														//use the LOD mesh as a collider substitute
														mColliderMeshes.Add(prefab.name, lodMesh);
												} else {
														//otherwise use our mesh filter's mesh
														//this is inefficient but it's better than broken colliders
														MeshFilter mf = prefab.GetComponent <MeshFilter>();
														if (mf != null) {
																mColliderMeshes.Add(mf.name, mf.sharedMesh);
														}
												}
										}
								}
						}

						WorldItemPack wip = null;

						for (int i = 0; i < StructurePacks.Count; i++) {
								wip = null;

								StructurePack structurePack = StructurePacks[i];
								mStructurePackLookup[structurePack.Name] = structurePack;
								structurePack.DynamicPrefabLookup.Clear();
								structurePack.StaticPrefabLookup.Clear();
								structurePack.LODMeshLookup.Clear();

								if (structurePack.DynamicPrefabs.Count > 0) {
										//create a world item lookup pack for loading dynamic prefabs
										//this will be used the second time a structure is loaded
										//and it generates its dynamic prefabs from stackitems
										//the first time it's loaded, it'll be instantiated from the structure pack
										wip = new WorldItemPack();
										wip.Name = structurePack.Name;
								}

								for (int j = 0; j < structurePack.DynamicPrefabs.Count; j++) {
										DynamicPrefab dp = structurePack.DynamicPrefabs[j].GetComponent <DynamicPrefab>();
										if (dp != null) {
												structurePack.DynamicPrefabLookup.Add(dp.name, dp);
												if (dp.worlditem != null) {
														//set the world item transform base to the dynamic prefab transform
														//this will ensure that the item's position etc. is manuplated properly
														Dynamic dynamic = null;
														if (dp.worlditem.gameObject.HasComponent <Dynamic>(out dynamic)) {
																dynamic.DynamicPrefabBase = dp;
														}
														dp.worlditem.Props.Global.FileNameBase = dp.worlditem.Props.Name.FileName;
														dp.worlditem.Props.Global.DynamicPrefab = true;
														if (wip != null) {
																wip.Prefabs.Add(dp.worlditem.gameObject);
														}
												}
										} else {
												Debug.Log("PREFAB WAS NULL: " + structurePack.DynamicPrefabs[j].name);
										}
								}

								for (int j = 0; j < structurePack.StaticPrefabs.Count; j++) {
										StructurePackPrefab spp = new StructurePackPrefab();
										spp.Prefab = structurePack.StaticPrefabs[j];
										if (spp.Prefab == null) {
												Debug.Log("PREFAB NULL IN SP " + structurePack.Name);
										}
										spp.MFilter = spp.Prefab.GetComponent <MeshFilter>();
										spp.MRenderer = spp.Prefab.GetComponent <MeshRenderer>();
										if (spp.MFilter != null) {
												mLodMeshes.TryGetValue(spp.Prefab.name, out spp.LodMesh);
												mColliderMeshes.TryGetValue(spp.Prefab.name, out spp.ColliderMesh);

												spp.BufferedMesh = new MeshCombiner.BufferedMesh();
												Structures.CopyMeshData(spp.MFilter.sharedMesh, spp.BufferedMesh);

												if (spp.LodMesh != null) {
														spp.BufferedLodMesh = new MeshCombiner.BufferedMesh();
														Structures.CopyMeshData(spp.LodMesh, spp.BufferedLodMesh);
												}
												structurePack.StaticPrefabLookup.Add(spp.Prefab.name, spp);
										}
								}

								if (wip != null) {
										//Debug.Log ("Added dynamic world item pack " + wip.Name);
										DynamicWorldItemPacks.SafeAdd(wip);
								}
						}
						//do this now while the player won't care
						System.GC.Collect();
						Resources.UnloadUnusedAssets();
				}

				public override void Initialize()
				{
						if (Application.isPlaying) {
								//create and initialize structure builders
								ExteriorBuilder = gameObject.FindOrCreateChild("ExteriorBuilder").gameObject.GetOrAdd <StructureBuilder>();
								InteriorBuilder = gameObject.FindOrCreateChild("InteriorBuilder").gameObject.GetOrAdd <StructureBuilder>();
								MinorBuilder = gameObject.FindOrCreateChild("MinorBuilder").gameObject.GetOrAdd <StructureBuilder>();

								ExteriorBuilder.Mode = StructureBuilder.BuilderMode.Exterior;
								InteriorBuilder.Mode = StructureBuilder.BuilderMode.Interior;
								MinorBuilder.Mode = StructureBuilder.BuilderMode.Minor;

								ExteriorBuilder.State = StructureBuilder.BuilderState.Dormant;
								InteriorBuilder.State = StructureBuilder.BuilderState.Dormant;
								MinorBuilder.State = StructureBuilder.BuilderState.Dormant;

								//create and assign mesh combiners
								ExteriorCombiner = new MeshCombiner();
								ExteriorCombinerDestroyed = new MeshCombiner();
								ExteriorLODCombiner = new MeshCombiner();
								ExteriorLODCombinerDestroyed = new MeshCombiner();

								InteriorCombiner = new MeshCombiner();
								InteriorCombinerDestroyed = new MeshCombiner();
								//InteriorLODCombiner = new MeshCombiner ();

								MinorCombiner = new MeshCombiner();
								MinorCombinerDestroyed = new MeshCombiner();
								MinorLODCombiner = new MeshCombiner();
								MinorLODCombinerDestroyed = new MeshCombiner();

								ExteriorBuilder.PrimaryCombiner = ExteriorCombiner;
								ExteriorBuilder.LODCombiner = ExteriorLODCombiner;
								ExteriorBuilder.DestroyedCombiner = ExteriorCombinerDestroyed;
								ExteriorBuilder.LODDestroyedCombiner = ExteriorLODCombinerDestroyed;
								ExteriorBuilder.ColliderCombiner = ExteriorColliderCombiner;

								InteriorBuilder.PrimaryCombiner = InteriorCombiner;
								InteriorBuilder.DestroyedCombiner = InteriorCombinerDestroyed;
								InteriorBuilder.LODCombiner = null;//InteriorLODCombiner;
								InteriorBuilder.LODDestroyedCombiner = null;
								InteriorBuilder.ColliderCombiner = InteriorColliderCombiner;

								MinorBuilder.PrimaryCombiner = MinorCombiner;
								MinorBuilder.DestroyedCombiner = MinorCombinerDestroyed;
								MinorBuilder.LODCombiner = MinorLODCombiner;
								MinorBuilder.LODDestroyedCombiner = MinorLODCombinerDestroyed;
								MinorBuilder.ColliderCombiner = MinorColliderCombiner;

								StartCoroutine(BuildChunkPrefabs());
						}
				}

				protected IEnumerator PrecomputePhysXMeshes()
				{
						GameObject meshCacheObject = new GameObject("MeshCacheObject");
						meshCacheObject.transform.position = new Vector3(8000f, 1000f, 0f);//move it out of the way
						MeshCollider mc = meshCacheObject.AddComponent <MeshCollider>();
						Rigidbody rb = meshCacheObject.AddComponent <Rigidbody>();
						rb.isKinematic = true;
						mc.enabled = false;
						foreach (Mesh collisionMesh in mColliderMeshes.Values) {
								mc.sharedMesh = collisionMesh;
								mc.enabled = true;
								//this should trigger collision data creation
								mc.enabled = false;
						}
						GameObject.Destroy(meshCacheObject);
						yield break;
				}

				protected IEnumerator BuildChunkPrefabs()
				{
						int numBuiltThisFrame = 0;
						for (int i = 0; i < Globals.MaxChunkPrefabsPerChunk * Globals.MaxSpawnedChunks; i++) {
								numBuiltThisFrame++;
								SceneryObjectPool.Push(CreateEmptyPrefabObject());
								if (numBuiltThisFrame > 10) {
										numBuiltThisFrame = 0;
										yield return null;
								}
						}

						numBuiltThisFrame = 0;
						for (int i = 0; i < Globals.BoxColliderPoolCount; i++) {
								numBuiltThisFrame++;
								BoxColliderPool.Push(CreateBoxCollider());
								if (numBuiltThisFrame > 50) {
										numBuiltThisFrame = 0;
										yield return null;
								}
						}

						enabled = true;
						mInitialized = true;
				}

				public void RefreshStructureShadowSettings(bool structureShadows, bool sceneryObjectShadows)
				{
						if (StructureShadows != structureShadows) {
								StructureShadows = structureShadows;
								for (int i = 0; i < LoadedStructures.Count; i++) {
										if (LoadedStructures[i] != null) {
												LoadedStructures[i].RefreshShadowCasters();
										}
								}
								for (int i = LoadedMinorStructures.LastIndex(); i >= 0; i--) {
										if (LoadedMinorStructures[i] != null) {
												LoadedMinorStructures[i].RefreshShadowCasters();
										} else {
												LoadedMinorStructures.RemoveAt(i);
										}
								}
						}
						if (SceneryObjectShadows != sceneryObjectShadows) {
								SceneryObjectShadows = sceneryObjectShadows;
								for (int i = 0; i < SceneryObjects.Count; i++) {
										SceneryObjects[i].RefreshShadowCasters(sceneryObjectShadows);
								}
						}
				}

				protected MeshCollider CreateMeshCollider()
				{
						GameObject meshColliderObject = gameObject.CreateChild("MeshCollider").gameObject;
						meshColliderObject.transform.position = Vector3.one * ((UnityEngine.Random.value * 1000f) - 2000f);
						meshColliderObject.layer = Globals.LayerNumHidden;
						meshColliderObject.transform.parent = ColliderKeeper;
						meshColliderObject.isStatic = true;
						//meshColliderObject.SetActive (false);
						MeshCollider mc = meshColliderObject.AddComponent <MeshCollider>();
						//add a box mesh to the collider so it doesn't freak out
						mc.sharedMesh = DefaultMeshColliderMesh;
						Rigidbody rb = meshColliderObject.AddComponent <Rigidbody>();
						rb.detectCollisions = false;
						rb.isKinematic = true;
						rb.constraints = RigidbodyConstraints.FreezeAll;
						rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
						rb.inertiaTensor = Vector3.one;
						rb.inertiaTensorRotation = Quaternion.identity;
						//mc.enabled = false;
						return mc;
				}

				protected BoxCollider CreateBoxCollider()
				{
						GameObject boxColliderObject = gameObject.CreateChild("BoxCollider").gameObject;
						boxColliderObject.transform.position = Vector3.one * ((UnityEngine.Random.value * 1000f) - 2000f);
						boxColliderObject.layer = Globals.LayerNumHidden;
						boxColliderObject.transform.parent = ColliderKeeper;
						boxColliderObject.isStatic = true;
						BoxCollider bc = boxColliderObject.AddComponent <BoxCollider>();
						Rigidbody rb = boxColliderObject.AddComponent <Rigidbody>();
						rb.detectCollisions = false;
						rb.isKinematic = true;
						rb.constraints = RigidbodyConstraints.FreezeAll;
						rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
						//bc.enabled = false;
						return bc;
				}

				public override void OnGameLoadStart()
				{
						//now we want to pre-compute all of our physx mesh lookup data
						//doing this now will cache this data with the meshes
						//and they'll be available when we use those meshes to create structures
						StartCoroutine(PrecomputePhysXMeshes());
				}

				#endregion

				#region structure loading

				public static void AddExteriorToLoad(Structure unloadedExterior)
				{
						if (!Globals.BuildStructures)
								return;

						if (ExteriorsWaitingToLoad.SafeAdd(unloadedExterior)) {
								unloadedExterior.OnPreparingToBuild.SafeInvoke();
						}
				}

				public static void AddInteriorToLoad(Structure unloadedInterior)
				{
						if (!Globals.BuildStructures)
								return;

						if (InteriorsWaitingToLoad.SafeAdd(unloadedInterior)) {
								unloadedInterior.OnPreparingToBuild.SafeInvoke();
						}
				}

				public static void AddExteriorToUnload(Structure loadedExterior)
				{
						ExteriorsWaitingToUnload.SafeAdd(loadedExterior);
				}

				public static void AddInteriorToUnload(Structure loadedInterior)
				{
						InteriorsWaitingToUnload.SafeAdd(loadedInterior);
				}

				public static void AddMinorToload(MinorStructure unloadedMinor, int structureNumber, WorldItem worlditem)
				{
						if (!Globals.BuildStructures)
								return;

						if (unloadedMinor.LoadState == StructureLoadState.ExteriorUnloaded) {
								if (!MinorsWaitingToLoad.Contains(unloadedMinor)) {
										unloadedMinor.Number = structureNumber;
										unloadedMinor.StructureOwner = worlditem;
										unloadedMinor.Chunk = worlditem.Group.GetParentChunk();
										unloadedMinor.LoadState = StructureLoadState.ExteriorWaitingToLoad;
										MinorsWaitingToLoad.Add(unloadedMinor);
								}
						} else {
								//Debug.Log ("SKIPPING UNLOADED MINOR STRUCTURE " + unloadedMinor.ToString ( ) + " because load state is " + unloadedMinor.LoadState.ToString ());
						}
				}

				public static void AddMinorToUnload(MinorStructure loadedMinor)
				{
						MinorsWaitingToUnload.SafeAdd(loadedMinor);
				}

				protected bool GetNextStructure(StructureLoadState desiredState, StructureLoadState precursorState, List <Structure> fromList, StructureLoadState disqualifiedState, out Structure structure)
				{
						structure = null;
						for (int i = fromList.LastIndex(); i >= 0; i--) {
								Structure nextStructure = fromList[i];
								//if it's null or it's no longer waiting to load, ditch it
								//this could happen if the active state changed while waiting
								if (nextStructure == null) {
										fromList.RemoveAt(i);
										//if the structure has the right load state
								} else if (nextStructure.Is(desiredState) || nextStructure.Is(precursorState)) {
										nextStructure.LoadState = desiredState;
										//if the world item isn't initialized - this includes unloading - ditch it
										if (nextStructure.Initialized) {
												//if (!nextStructure.worlditem.Is (WILoadState.Initialized)) {
												//fromList.RemoveAt (i);
												//} else {
												//otherwise we're good to go
												//remove it from the list
												fromList.RemoveAt(i);
												structure = nextStructure;
												//}
										}
								} else if (nextStructure.Is(disqualifiedState) && nextStructure.LoadPriority != StructureLoadPriority.SpawnPoint) {
										fromList.RemoveAt(i);
								}
								break;
						}

						if (structure != null) {
								return true;
						}
						return false;
				}

				public void Update()
				{
						if (!Profile.Get.HasCurrentGame) {
								return;
						}

						//TODO look into making this a coroutine (?)
						if (GameManager.Is(FGameState.InGame) && mUpdateStructures < 9) {
								mUpdateStructures++;
								return;
						} else {
								mUpdateStructures = 0;
						}

						if (ExteriorBuilder.State == StructureBuilder.BuilderState.Dormant ||
						 ExteriorBuilder.State == StructureBuilder.BuilderState.Error ||
						 ExteriorBuilder.State == StructureBuilder.BuilderState.Finished) {
								//Debug.Log ("Exterior builder is finished, starting new load structure");
								StartCoroutine(LoadStructures(
										ExteriorsWaitingToLoad,
										StructureLoadState.ExteriorWaitingToLoad,
										StructureLoadState.ExteriorUnloaded,
										StructureLoadState.ExteriorLoading | StructureLoadState.ExteriorLoaded
										| StructureLoadState.InteriorWaitingToLoad
										| StructureLoadState.InteriorLoading
										| StructureLoadState.InteriorLoaded,
										StructureLoadState.ExteriorLoading,
										StructureLoadState.ExteriorLoaded,
										ExteriorBuilder));
						}

						if (InteriorBuilder.State == StructureBuilder.BuilderState.Dormant ||
						 InteriorBuilder.State == StructureBuilder.BuilderState.Error ||
						 InteriorBuilder.State == StructureBuilder.BuilderState.Finished) {
								//Debug.Log ("Interior builder is finished, starting new load structure");
								StartCoroutine(LoadStructures(
										InteriorsWaitingToLoad,
										StructureLoadState.InteriorWaitingToLoad,
										StructureLoadState.ExteriorLoaded,
										StructureLoadState.InteriorLoading | StructureLoadState.InteriorLoaded,
										StructureLoadState.InteriorLoading,
										StructureLoadState.InteriorLoaded,
										InteriorBuilder));
						}

						if (ExteriorsWaitingToUnload.Count > 0 && !ExteriorBuilder.IsUnloading) {
								StartCoroutine(UnloadStructures(
										ExteriorsWaitingToUnload,
										StructureLoadState.ExteriorWaitingToUnload,
										StructureLoadState.ExteriorLoaded,
										StructureLoadState.ExteriorUnloading | StructureLoadState.ExteriorUnloaded,
										StructureLoadState.ExteriorUnloading,
										StructureLoadState.ExteriorUnloaded,
										ExteriorBuilder));
						}

						if (InteriorsWaitingToUnload.Count > 0 && !InteriorBuilder.IsUnloading) {
								StartCoroutine(UnloadStructures(
										InteriorsWaitingToUnload,
										StructureLoadState.InteriorWaitingToUnload,
										StructureLoadState.InteriorLoaded,
										StructureLoadState.ExteriorLoaded
										| StructureLoadState.ExteriorLoading
										| StructureLoadState.ExteriorWaitingToLoad
										| StructureLoadState.ExteriorUnloaded
										| StructureLoadState.ExteriorUnloading
										| StructureLoadState.ExteriorWaitingToUnload
										| StructureLoadState.InteriorUnloading,
										StructureLoadState.InteriorUnloading,
										StructureLoadState.ExteriorLoaded,
										InteriorBuilder));
						}

						mloadMinors++;
						if (mloadMinors > 3 && MinorsWaitingToLoad.Count > 0 &&
						 (MinorBuilder.State == StructureBuilder.BuilderState.Dormant ||
						 MinorBuilder.State == StructureBuilder.BuilderState.Error ||
						 MinorBuilder.State == StructureBuilder.BuilderState.Finished)) {
								mloadMinors = 0;
								StartCoroutine(LoadMinorStructure(MinorsWaitingToLoad[0]));
						}

						if (MinorsWaitingToUnload.Count > 0 && !MinorBuilder.IsUnloading) {
								StartCoroutine(UnloadMinorStructure(MinorsWaitingToUnload[0]));
						}
				}

				protected bool mUnloadingExteriors = false;
				protected int mUpdateStructures = 0;
				protected int mloadMinors = 0;

				protected IEnumerator LoadStructures(
						List <Structure> fromList,
						StructureLoadState desiredState,
						StructureLoadState precursorState,
						StructureLoadState disqualifiedState,
						StructureLoadState intermediateState,
						StructureLoadState finalState,
						StructureBuilder withBuilder)
				{
						//first sort the structures by load priority
						fromList.Sort(new Comparison <Structure>(CompareStructurePriority));
						Structure structure = null;
						//getting this removes it from the waiting queue
						if (GetNextStructure(desiredState, precursorState, fromList, disqualifiedState, out structure)) {
								StructureTemplate mainTemplate = null;
								if (Mods.Get.Runtime.LoadMod(ref mainTemplate, "Structure", structure.State.TemplateName)) {
										mainTemplate.Name = structure.State.TemplateName;
										//wuhoo got the template, time to rock and roll
										structure.LoadState = intermediateState;
										yield return StartCoroutine(structure.CreateStructureGroups(finalState));
										//this will run in parallel with the exterior building
										structure.LoadMinorStructures(finalState);
										//initialize structure (it'll wait until it's ready to start), then build the exterior
										yield return StartCoroutine(withBuilder.Initialize(structure, mainTemplate, structure.LoadPriority));
										//first build the structure meshes
										//then add doors, windows and worlditems
										//this prevents live objects and characters etc from falling through things
										//TODO add some kind of time-out system
										yield return StartCoroutine(withBuilder.GenerateStructureMeshes());
										if (withBuilder.State != StructureBuilder.BuilderState.Error && structure.Is(intermediateState)) {
												//if there's an error it usually means the structure was 'unbuilt' before it could finish
												//and if the exterior is no longer loading then it may have been asked to unload
												yield return StartCoroutine(withBuilder.GenerateStructureItems());
										}
										if (withBuilder.State != Builder.BuilderState.Error) {
												//let the structure know it's been built
												structure.LoadState = finalState;
												LoadedStructures.SafeAdd(structure);
												structure.OnLoadFinish(finalState);
												//reset the builder state so we can use it again
										} else {
												Debug.Log("ERROR in structure builder when building " + mainTemplate.Name);
										}
										withBuilder.Reset();
								}

								mainTemplate = null;
								//TODO see if this is really necessary...
								System.GC.Collect(0, System.GCCollectionMode.Forced);
								yield return null;
								yield return null;
						}
						yield break;
				}

				protected IEnumerator UnloadStructures(
						List <Structure> fromList,
						StructureLoadState desiredState,
						StructureLoadState precursorState,
						StructureLoadState disqualifiedState,
						StructureLoadState intermediateState,
						StructureLoadState finalState,
						StructureBuilder withBuilder)
				{
						withBuilder.IsUnloading = true;
						//don't bother to sort the list, unload them as they come
						Structure structure = null;
						//getting this removes it from the waiting queue
						if (GetNextStructure(desiredState, precursorState, fromList, disqualifiedState, out structure)) {
								Debug.Log("Unloading structure " + structure.name + " in STRUCTURES");
								structure.LoadState = intermediateState;
								//we always want to close all entrances
								//so wait for that before proceeding
								structure.UnloadMinorStructures(finalState);
								yield return StartCoroutine(CloseOuterEntrances(structure));
								//now send the structure to the mesh builder to be broken down
								//the structure builder's mode will figure out which meshes need to be destroyed
								yield return StartCoroutine(StructureBuilder.UnloadStructureMeshes(withBuilder, structure));
								StructureBuilder.ReclaimColliders(withBuilder, structure);
								structure.LoadState = finalState;
								structure.OnUnloadFinish(finalState);
						}
						withBuilder.IsUnloading = false;
						yield break;
				}

				protected IEnumerator LoadMinorStructure(MinorStructure minorStructure)
				{
						//minor structures are 'unimportant' so we don't assign them a build priority
						//just go through the list one by one
						MinorsWaitingToLoad.Remove(minorStructure);
						if (minorStructure != null && minorStructure.LoadState == StructureLoadState.ExteriorWaitingToLoad) {
								StructureTemplate template = null;
								if (Mods.Get.Runtime.LoadMod(ref template, "Structure", minorStructure.TemplateName)) {
										//initialize (it will wait until it's ready to start)
										yield return StartCoroutine(MinorBuilder.Initialize(minorStructure, template, minorStructure.LoadPriority));
										//build the meshes, there's no need to build worlditems or anything
										yield return StartCoroutine(MinorBuilder.GenerateStructureMeshes());
										//reset the builder
										minorStructure.LoadState = StructureLoadState.ExteriorLoaded;
										minorStructure.RefreshColliders();
										minorStructure.RefreshRenderers(true);
										MinorBuilder.Reset();
										LoadedMinorStructures.SafeAdd(minorStructure);
										yield return new WaitForSeconds(0.05f);
								}

								template = null;
								//TODO see if this is really necessary
								System.GC.Collect(0, System.GCCollectionMode.Forced);
						}
				}

				protected IEnumerator UnloadMinorStructure(MinorStructure minorStructure)
				{
						MinorsWaitingToUnload.Remove(minorStructure);
						if (minorStructure != null && minorStructure.LoadState == StructureLoadState.ExteriorWaitingToUnload || minorStructure.LoadState == StructureLoadState.ExteriorLoaded) {
								MinorBuilder.IsUnloading = true;
								minorStructure.LoadState = StructureLoadState.ExteriorUnloading;
								yield return StartCoroutine(StructureBuilder.UnloadStructureMeshes(minorStructure.ExteriorMeshes));
								minorStructure.LoadState = StructureLoadState.ExteriorUnloaded;
						}
						MinorBuilder.IsUnloading = false;
						yield break;
				}

				public IEnumerator CloseOuterEntrances(Structure structure)
				{
						Debug.Log("Closing outer entrances in structure " + structure.name);
						Dynamic dyn = null;
						Door door = null;
						Window window = null;
						for (int i = 0; i < structure.OuterEntrances.Count; i++) {
								dyn = structure.OuterEntrances[i];
								if (dyn != null) {
										if (dyn.worlditem.Is <Window>(out window)) {
												yield return StartCoroutine(window.ForceClose());
										} else if (dyn.worlditem.Is <Door>(out door)) {
												yield return StartCoroutine(door.ForceClose());
										}
								}
						}
						yield break;
				}

				#endregion

				#region pack lookup

				public static bool LoadedStructure(string structureFileName, out Structure structure)
				{
						structure = null;
						for (int i = LoadedStructures.LastIndex(); i >= 0; i--) {
								if (LoadedStructures[i] == null || !LoadedStructures[i].Is(StructureLoadState.ExteriorLoaded | StructureLoadState.InteriorLoading | StructureLoadState.InteriorLoaded)) {
										LoadedStructures.RemoveAt(i);
								} else if (LoadedStructures[i].worlditem.FileName == structureFileName) {
										structure = LoadedStructures[i];
										break;
								}
						}
						return structure != null;
				}

				public static void ReclaimBoxColliders(List <BoxCollider> boxColliders)
				{
						if (boxColliders == null)
								return;

						Transform tr = Get.transform;
						for (int i = 0; i < boxColliders.Count; i++) {
								BoxCollider bc = boxColliders[i];
								if (bc != null) {
										bc.transform.parent = Get.ColliderKeeper;
										bc.gameObject.layer = Globals.LayerNumHidden;
										//bc.enabled = false;
										//bc.gameObject.SetActive (false);
										//bc.transform.parent = tr;
										Get.BoxColliderPool.Push(bc);
								}
						}
						boxColliders.Clear();
				}

				public DynamicStructureTemplatePiece StackItemFromName(string prefabName)
				{
						DynamicStructureTemplatePiece newTemplate = null;
						foreach (StructurePack pack in StructurePacks) {
								foreach (GameObject prefab in pack.DynamicPrefabs) {
										if (prefab.name == prefabName) {
												newTemplate	= new DynamicStructureTemplatePiece();
												newTemplate.Props.Name.PrefabName = prefab.name;
												newTemplate.Props.Name.PackName = pack.Name;
												newTemplate.Props.Local.Mode = WIMode.Frozen;
												break;
										}
								}
						}
						return newTemplate;
				}

				public bool DynamicPrefab(string prefabName, out GameObject prefab)
				{
						prefab = null;
						for (int i = 0; i < StructurePacks.Count; i++) {
								StructurePack pack = StructurePacks[i];
								for (int j = 0; j < pack.DynamicPrefabs.Count; j++) {
										if (pack.DynamicPrefabs[i].name == prefabName) {
												prefab = pack.DynamicPrefabs[i];
												break;
										}
								}
						}
						return prefab != null;
				}

				public bool PackDynamicPrefab(string structurePackName, string prefabName, out DynamicPrefab prefab)
				{
						StructurePack pack = null;
						prefab = null;
						if (mStructurePackLookup.TryGetValue(structurePackName, out pack)) {
								return pack.DynamicPrefabLookup.TryGetValue(prefabName, out prefab);
						}
						return false;
				}

				public bool SharedMaterial(string materialName, out Material sharedMaterial)
				{
						return mSharedMaterialLookup.TryGetValue(materialName, out sharedMaterial);
				}

				public GameObject PackStaticInstance(string structurePackname, string prefabName)
				{
						GameObject instance = null;
						StructurePackPrefab spp = null;
						if (PackStaticPrefab(structurePackname, prefabName, out spp)) {
								instance = GameObject.Instantiate(spp.Prefab) as GameObject;
								instance.name = spp.Prefab.name;
						}
						return instance;
				}

				public bool PackStaticMesh(string packName, string prefabName, out MeshCombiner.BufferedMesh bufferedMesh)
				{
						GameObject prefab = null;
						bufferedMesh = null;
						return false;
				}

				public bool ColliderMesh(string prefabName, out Mesh colliderMesh)
				{
						return mColliderMeshes.TryGetValue(prefabName, out colliderMesh);
				}

				public bool PackStaticPrefab(string structurePackName, string prefabName, out StructurePackPrefab prefab)
				{
						StructurePack pack = null;
						prefab = null;
						if (mStructurePackLookup.TryGetValue(structurePackName, out pack)) {
								return pack.StaticPrefabLookup.TryGetValue(prefabName, out prefab);
						}
						return false;
				}

				public string PackName(string prefabName)
				{
						foreach (StructurePack pack in StructurePacks) {
								foreach (GameObject staticPrefab in pack.StaticPrefabs) {
										if (staticPrefab.name == prefabName) {
												return pack.Name;
										}
								}
								foreach (GameObject dynamicPrefab in pack.DynamicPrefabs) {
										if (dynamicPrefab.name == prefabName) {
												return pack.Name;
										}
								}
						}
						//		//Debug.Log ("No pack found for " + prefabName);
						return string.Empty;
				}

				protected int CompareStructurePriority(Structure s1, Structure s2)
				{
						//numbers are reversed
						if (s1.LoadPriority == s2.LoadPriority) {
								return (s2.worlditem.LastActiveDistanceToPlayer - s2.worlditem.ActiveRadius).CompareTo(s1.worlditem.LastActiveDistanceToPlayer - s1.worlditem.ActiveRadius);
						}
						int priority1 = (int)s1.LoadPriority;
						int priority2 = (int)s2.LoadPriority;
						return priority2.CompareTo(priority1);
				}

				#endregion

				#region chunk prefabs

				public static void UnloadChunkPrefab(ChunkPrefab chunkPrefab)//, WorldChunk chunk, ChunkMode mode)
				{
						if (chunkPrefab == null || !chunkPrefab.IsLoaded) {
								return;
						}

						ChunkPrefabObject cpo = chunkPrefab.LoadedObject;
						cpo.Deactivate();
						Get.SceneryObjectPool.Push(cpo);
						chunkPrefab.LoadedObject = null;
				}

				public static void LoadChunkPrefab(ChunkPrefab chunkPrefab, WorldChunk chunk, ChunkMode mode)
				{
						if (chunkPrefab == null || chunkPrefab.IsLoaded) {
								return;
						}

						if (string.IsNullOrEmpty(chunkPrefab.PackName)) {
								return;
						}

						//see if we can build this prefab
						if (!Get.PackStaticPrefab(chunkPrefab.PackName, chunkPrefab.PrefabName, out gCfoSpp)) {
								Debug.Log("CHUNK PREFAB " + chunkPrefab.Name + " USES MESH WE COULDN'T FIND");
								return;
						}

						//get a chunk prefab from the pool
						gCfo = Get.SceneryObjectPool.Pop();
						gCfo.Layer = Globals.LayerNumSolidTerrain;//chunkPrefab.Layer;
						gCfo.TerrainType = chunkPrefab.TerrainType;
						gCfo.go.SetActive(true);
						gCfo.rb.detectCollisions = true;
						gCfo.go.name = chunkPrefab.Name;
						gCfo.go.SetLayerRecursively(gCfo.Layer);
						gCfo.go.tag = chunkPrefab.Tag;
						gCfo.PrimaryMeshFilter.tag = chunkPrefab.Tag;
						//gCfo.tr.parent = chunk.Transforms.AboveGroundStaticImmediate;
						gCfo.tr.position = chunk.ChunkOffset + chunkPrefab.Transform.Position;
						gCfo.tr.rotation = Quaternion.Euler(chunkPrefab.Transform.Rotation);
						gCfo.PrimaryTransform.localScale = chunkPrefab.Transform.Scale.x * Vector3.one; //non-uniform scales NOT ALLOWED!
						gCfo.LodTransform.localScale = gCfo.PrimaryTransform.localScale;
						//update the chunk prefab - set the parent and move it into position
						chunkPrefab.ParentChunk = chunk;
						chunkPrefab.LoadedObject = gCfo;

						if (chunkPrefab.UseMeshCollider) {
								if (gCfoSpp.ColliderMesh != null) {
										gCfo.PrimaryCollider.sharedMesh = gCfoSpp.ColliderMesh;
								} else {
										gCfo.PrimaryCollider.sharedMesh = gCfoSpp.MFilter.sharedMesh;
								}
								gCfo.PrimaryCollider.convex = chunkPrefab.UseConvexMesh;
								gCfo.PrimaryCollider.enabled = true;
						}

						if (chunkPrefab.UseBoxColliders) {
								//TODO instantiate box colliders
						}

						//add scripts
						foreach (KeyValuePair <string,string> sceneryScript in chunkPrefab.SceneryScripts) {
								SceneryScript script = (SceneryScript)chunkPrefab.LoadedObject.go.GetOrAdd(sceneryScript.Key);
								script.cfo = gCfo;
								script.UpdateSceneryState(sceneryScript.Value, chunk);
								chunkPrefab.LoadedObject.CfSceneryScripts.Add(script);
						}

						//set the shared materials
						if (gCfoSharedMaterialList == null) {
								gCfoSharedMaterialList = new List<Material>();
						} else {
								gCfoSharedMaterialList.Clear();
						}
						for (int i = 0; i < chunkPrefab.SharedMaterialNames.Count; i++) {
								if (Get.mSharedMaterialLookup.TryGetValue(chunkPrefab.SharedMaterialNames[i], out gCfoSharedMaterial)) {
										gCfoSharedMaterialList.Add(gCfoSharedMaterial);
								} else {
										Debug.Log("COULDN'T FIND SHARED MATERIAL " + chunkPrefab.SharedMaterialNames[i] + ", using empty material");
										gCfoSharedMaterialList.Add(null);
								}
						}
						if (chunkPrefab.EnableSnow) {
								gCfoSharedMaterialList.Add(Mats.Get.SnowOverlayMaterial);
						}
						gCfoSharedMaterialArray = gCfoSharedMaterialList.ToArray();
						gCfo.PrimaryRenderer.sharedMaterials = gCfoSharedMaterialArray;
						gCfo.LodRenderer.sharedMaterials = gCfoSharedMaterialArray;

						//set the primary and LOD mesh
						gCfo.PrimaryMeshFilter.sharedMesh = gCfoSpp.MFilter.sharedMesh;
						gCfo.LodMeshFilter.sharedMesh = gCfoSpp.LodMesh;
						gCfo.PrimaryRenderer.enabled = true;
						gCfo.PrimaryRenderer.castShadows = false;
						gCfo.PrimaryRenderer.receiveShadows = false;
						gCfo.LodRenderer.enabled = true;
						gCfo.Lod.enabled = true;
						//set the game object to active
						gCfo.rb.detectCollisions = true;
						gCfo.go.SetActive(true);
						//gCfo.ShowAboveGround (!Player.Local.Surroundings.IsUnderground);

						gCfo.Lod.RecalculateBounds();
						//TODO add any scripts & script states in the chunk prefab
				}

				public static void CopyMeshData(Mesh mesh, MeshCombiner.BufferedMesh bufferedMesh)
				{
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
					//Debug.LogWarning ("The MeshCombiner does not support this meshes (" +
					//bufferedMesh.Name + "topology (" + bufferedMesh.Topology [i] + ")");
												break;
								}
								bufferedMesh.Indexes.Add(mesh.GetIndices(i));
						}
				}

				public ChunkPrefabObject CreateEmptyPrefabObject()
				{
						ChunkPrefabObject cfo = new ChunkPrefabObject();
						SceneryObjects.Add(cfo);

						cfo.go = new GameObject("Chunk Prefab Object");
						//cfo.go.transform.parent = transform;
						cfo.go.SetActive(false);
						cfo.go.isStatic = true;

						cfo.rb = cfo.go.AddComponent <Rigidbody>();
						cfo.rb.isKinematic = true;
						cfo.rb.detectCollisions = false;
						cfo.rb.constraints = RigidbodyConstraints.FreezeAll;

						GameObject primaryObject = cfo.go.CreateChild("PrimaryObject").gameObject;
						GameObject lodObject = cfo.go.CreateChild("LodObject").gameObject;
						primaryObject.layer = Globals.LayerNumSolidTerrain;
						lodObject.layer = Globals.LayerNumSolidTerrain;
						cfo.PrimaryTransform = primaryObject.transform;
						cfo.LodTransform = lodObject.transform;

						cfo.PrimaryMeshFilter = primaryObject.AddComponent <MeshFilter>();
						cfo.PrimaryRenderer = primaryObject.AddComponent <MeshRenderer>();
						cfo.LodMeshFilter = lodObject.AddComponent <MeshFilter>();
						cfo.LodRenderer = lodObject.AddComponent <MeshRenderer>();

						cfo.PrimaryRenderer.enabled = false;
						cfo.PrimaryRenderer.castShadows = Structures.SceneryObjectShadows;
						cfo.LodRenderer.enabled = false;
						cfo.LodRenderer.castShadows = Structures.SceneryObjectShadows;

						cfo.Lod = cfo.go.AddComponent <LODGroup>();
						cfo.PrimaryCollider = primaryObject.AddComponent <MeshCollider>();
						cfo.PrimaryCollider.enabled = false;

						Renderer[] primaryRenderers = new Renderer [] { cfo.PrimaryRenderer };
						Renderer[] lodRenderers = new Renderer [] { cfo.LodRenderer };

						LOD primary = new LOD(Globals.SceneryLODRatioPrimary, primaryRenderers);
						LOD lod = new LOD(Globals.SceneryLODRatioSecondary, lodRenderers);
						LOD[] lods = new LOD [] { primary, lod };

						cfo.Lod.SetLODS(lods);

						cfo.tr = cfo.go.transform;
						return cfo;
				}

				protected static Material[] gCfoSharedMaterialArray;
				protected static Material gCfoSharedMaterial = null;
				protected static List <Material> gCfoSharedMaterialList;
				protected static ChunkPrefabObject gCfo = null;
				protected static StructurePackPrefab gCfoSpp = null;
				protected static Mesh gCfoPrimaryMesh = null;
				protected static Mesh gCfoLodMesh = null;

				#endregion

		}
}
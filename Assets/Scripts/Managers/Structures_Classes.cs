using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Hydrogen.Threading.Jobs;
using System.IO;

namespace Frontiers
{
		[Serializable]
		public class ChunkPrefabObject
		{
				public ChunkPrefabObject()
				{ 
						Layer = Globals.LayerNumSolidTerrain;
						CfSceneryScripts = new List <SceneryScript>();
				}

				public void ShowAboveGround(bool show)
				{
						bool showPrefab = false;

						switch (TerrainType) {
								case LocationTerrainType.Transition:
								default:
										return;

								case LocationTerrainType.AboveGround:
										showPrefab = show;
										break;

								case LocationTerrainType.BelowGround:
										showPrefab = !show;
										break;
						}

						try {
							if (showPrefab) {
									rb.detectCollisions = true;
									PrimaryRenderer.gameObject.layer = Layer;
									LodRenderer.gameObject.layer = Layer;
									PrimaryRenderer.castShadows = Structures.SceneryObjectShadows;
									LodRenderer.castShadows = Structures.SceneryObjectShadows;
							} else {
									rb.detectCollisions = false;
									PrimaryRenderer.gameObject.layer = Globals.LayerNumHidden;
									LodRenderer.gameObject.layer = Globals.LayerNumHidden;
							}
						}
						catch (Exception e) {
								Debug.LogError("Error while changing chunk prefab, proceeding normally: " + e.ToString());
						}
				}

				public void Deactivate()
				{
						rb.detectCollisions = false;
						PrimaryCollider.sharedMesh = null;
						PrimaryMeshFilter.sharedMesh = null;
						LodMeshFilter.sharedMesh = null;
						Structures.ReclaimBoxColliders(BoxColliders);
						if (CfSceneryScripts.Count > 0) {
								for (int i = 0; i < CfSceneryScripts.Count; i++) {
										GameObject.Destroy(CfSceneryScripts[i]);
								}
								CfSceneryScripts.Clear();
						}
						go.SetActive(false);
				}

				public void RefreshShadowCasters(bool terrainShadows)
				{
						PrimaryRenderer.castShadows = terrainShadows;
						PrimaryRenderer.receiveShadows = terrainShadows;
				}

				public LocationTerrainType TerrainType = LocationTerrainType.AboveGround;
				public int Layer;
				// = Globals.LayerNumSolidTerrain;
				public Rigidbody rb;
				public Transform tr;
				public GameObject go;
				public LODGroup Lod;
				public Transform PrimaryTransform;
				public MeshFilter PrimaryMeshFilter;
				public MeshRenderer PrimaryRenderer;
				public MeshCollider PrimaryCollider;
				public Transform LodTransform;
				public MeshFilter LodMeshFilter;
				public MeshRenderer LodRenderer;
				public List <BoxCollider> BoxColliders;
				// = new List <BoxCollider> ();
				//additional scripts and script states
				public List <SceneryScript> CfSceneryScripts;
				// = new List <SceneryScript> ( );
		}

		[Serializable]
		public class MaterialSubstitution {
				public Material SubstituteMaterial;
				public List <Material> OriginalMaterials;
		}

		[Serializable]
		public class StructurePack
		{
				public string Name = "StructurePack";
				public List <GameObject> StaticPrefabs = new List <GameObject>();
				public List <GameObject> DynamicPrefabs = new List <GameObject>();
				public List <Mesh> LODMeshes = new List <Mesh>();
				[NonSerialized]
				public Dictionary <GameObject, Mesh> LODMeshLookup = new Dictionary<GameObject, Mesh>();
				[NonSerialized]
				public Dictionary <string, StructurePackPrefab> StaticPrefabLookup = new Dictionary <string, StructurePackPrefab>();
				[NonSerialized]
				public Dictionary <string, DynamicPrefab> DynamicPrefabLookup = new Dictionary <string, DynamicPrefab>();
		}

		[Serializable]
		public class StructurePackPrefab
		{
				public GameObject Prefab;
				public MeshFilter MFilter;
				public MeshRenderer MRenderer;
				public Hydrogen.Threading.Jobs.MeshCombiner.BufferedMesh BufferedMesh;
				public Hydrogen.Threading.Jobs.MeshCombiner.BufferedMesh BufferedLodMesh;
				public Mesh LodMesh;
				public Mesh ColliderMesh;
		}

		[Serializable]
		public class MinorStructure
		{
				public MinorStructure()
				{
						LoadState = StructureLoadState.ExteriorUnloaded;
						ExteriorMeshes = new List <MeshFilter>();
						ExteriorRenderers = new List <Renderer>();
						ExteriorLODRenderers = new List<Renderer>();
						ExteriorRenderersDestroyed = new List <Renderer>();
						ExteriorLODRenderersDestroyed = new List<Renderer>();
						ExteriorLayers = new List <StructureTerrainLayer>();
						ExteriorColliders = new List <Collider>();
						ExteriorBoxColliders = new List <BoxCollider>();
						ExteriorMeshColliders = new List <MeshCollider>();
				}

				public string TemplateName = "Structure";
				public SVector3 Position = SVector3.zero;
				public SVector3 Rotation = SVector3.zero;
				public StructureLoadPriority LoadPriority = StructureLoadPriority.Immediate;
				public bool UseMeshColliders = true;
				public int InteriorVariant = 0;
				[XmlIgnore]
				public MinorStructureContainer Container;

				public void ClearStructure()
				{
						LoadState = StructureLoadState.ExteriorUnloaded;
						Structures.ReclaimBoxColliders(ExteriorBoxColliders);
						ExteriorMeshes.Clear();
						ExteriorRenderers.Clear();
						ExteriorLODRenderers.Clear();
						ExteriorRenderersDestroyed.Clear();
						ExteriorLODRenderersDestroyed.Clear();
						ExteriorColliders.Clear();
						ExteriorBoxColliders.Clear();
						ExteriorMeshColliders.Clear();
				}

				#region runtime stuff

				[XmlIgnore]
				public WorldItem StructureOwner = null;
				[XmlIgnore]
				public int Number = 0;
				[XmlIgnore]
				public StructureLoadState LoadState = StructureLoadState.ExteriorUnloaded;
				[XmlIgnore]
				public List <MeshFilter> ExteriorMeshes;
				[XmlIgnore]
				public List <Renderer> ExteriorRenderers;
				[XmlIgnore]
				public List <Renderer> ExteriorLODRenderers;
				[XmlIgnore]
				public List <Renderer> ExteriorRenderersDestroyed;
				[XmlIgnore]
				public List <Renderer> ExteriorLODRenderersDestroyed;
				[XmlIgnore]
				public List <StructureTerrainLayer> ExteriorLayers;
				[XmlIgnore]
				public List <Collider> ExteriorColliders;
				[XmlIgnore]
				public List <BoxCollider> ExteriorBoxColliders;
				[XmlIgnore]
				public List <MeshCollider> ExteriorMeshColliders;
				[XmlIgnore]
				public WorldChunk Chunk;

				#endregion

				public void RefreshRenderers(bool renderersEnabled)
				{
						for (int i = ExteriorRenderers.LastIndex(); i >= 0; i--) {
								if (ExteriorRenderers[i] != null) {
										ExteriorRenderers[i].enabled = renderersEnabled;
								} else {
										ExteriorRenderers.RemoveAt(i);
								}
						}
				}

				public void RefreshColliders()
				{
						//get rid of pairs
						//get rid of terrain collision
						for (int i = ExteriorColliders.LastIndex(); i >= 0; i--) {
								if (ExteriorColliders[i] != null) {
										if (!ExteriorColliders[i].gameObject.activeSelf) {
												ExteriorColliders[i].gameObject.SetActive(true);
										}
										if (!ExteriorColliders[i].enabled) {
												ExteriorColliders[i].enabled = true;
												ExteriorRenderers[i].castShadows = Structures.StructureShadows;
												ExteriorRenderers[i].receiveShadows = Structures.StructureShadows;
										}
								} else {
										ExteriorColliders.RemoveAt(i);
								}
						}
				}

				public void RefreshShadowCasters()
				{
						for (int i = ExteriorRenderers.LastIndex(); i >= 0; i--) {
								if (ExteriorRenderers[i] != null) {
										ExteriorRenderers[i].castShadows = Structures.StructureShadows;
										ExteriorRenderers[i].receiveShadows = Structures.StructureShadows;
								}
						}
				}
		}

		[Serializable]
		public class StructurePackPaths
		{
				public string Name = string.Empty;
				public string PackPath = string.Empty;
				public List <string> StaticPrefabs = new List <string>();
				public List <string> DynamicPrefabs = new List <string>();
				public List <string> Meshes = new List <string>();
				public List <string> LODMeshes = new List<string>();
				public List <string> Materials = new List <string>();
				public List <string> Textures = new List <string>();
		}

		[Serializable]
		public class WorldItemPack : IComparable <WorldItemPack>
		{
				public string Name = "WorldItemPack";
				public List <GameObject> Prefabs = new List <GameObject>();
				//public List <Mesh> MeshVariations = new List <Mesh>();
				//public List <Material> MaterialVariations = new List <Material>();
				public List <WorldItem> WorldItems = new List <WorldItem>();

				public int CompareTo(WorldItemPack other)
				{
						return Name.CompareTo(other.Name);
				}

				public void RefreshLookup()
				{
						WorldItems.Clear();
						mWorldItemPrefabLookup.Clear();
						for (int i = Prefabs.Count - 1; i >= 0; i--) {
								if (Prefabs[i] != null) {
										//Debug.Log ("Getting worlditem prefab for " + Prefabs [i].name);
										WorldItem worldItemPrefab = Prefabs[i].GetComponent <WorldItem>();
										WorldItems.Add(worldItemPrefab);
										//worldItemPrefab.Props.Global.FileNameBase = worldItemPrefab.Props.Name.FileNameBase;
										//UnityEditor.EditorUtility.SetDirty (worldItemPrefab);
										//Debug.Log ("Adding worlditem prefab " + worldItemPrefab.name + " with prefab name " + worldItemPrefab.Props.Name.PrefabName);
										mWorldItemPrefabLookup.Add(worldItemPrefab.Props.Name.PrefabName, worldItemPrefab);
								} else {
										Prefabs.RemoveAt(i);
								}
						}
				}

				public bool GetWorldItemPrefab(string prefabName, out WorldItem worldItemPrefab)
				{
						return mWorldItemPrefabLookup.TryGetValue(prefabName, out worldItemPrefab);
				}

				[NonSerialized]
				protected Dictionary <string, WorldItem> mWorldItemPrefabLookup = new Dictionary <string, WorldItem>();
		}
}
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Hydrogen.Threading.Jobs;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Frontiers
{
	public partial class Structures : Manager
	{
		#if UNITY_EDITOR
		protected IEnumerator BuildAllStructures ()
		{
			List <string> templateNames = Mods.Get.Available ("Structure");
			foreach (string allTemplateName in templateNames) {
				Structure structure = BuildStructureWithoutParent (allTemplateName);
				while (!structure.Is (StructureLoadState.ExteriorLoaded)) {
					yield return null;
				}
				//yield return WorldClock.WaitForRTSeconds(0.1f);
			}
			yield break;
		}

		public static void BuildPrefabStructure (string templateName)
		{
			//		GameObject structureGameObject = new GameObject (templateName + "-STR");
			//		StructureBuilder sb = structureGameObject.AddComponent <StructureBuilder> ();
			//		sb.CreateTemporaryPrefab (templateName);
		}

		public static Structure BuildStructureWithoutParent (string templateName)
		{
			if (templateName == "all") {
				Get.StartCoroutine (Get.BuildAllStructures ());
			} else {
				WorldItem structureItem = null;
				WorldItems.CloneWorldItem ("WorldPathMarkers", "Shingle 1", STransform.zero, true, WIGroups.Get.World, out structureItem);
				Structure structure = structureItem.GetOrAdd <Structure> ();
				structureItem.Props.Name.FileName = templateName;
				structure.State.TemplateName = templateName;
				AddExteriorToLoad (structure);
				return structure;
			}
			return null;
		}

		public void RefreshItemLayers ()
		{
			foreach (StructurePack pack in StructurePacks) {
				foreach (GameObject prefab in pack.StaticPrefabs) {
					if (!(prefab.layer == Globals.LayerNumStructureCustomCollider || prefab.layer == Globals.LayerNumStructureIgnoreCollider)) {
						BoxCollider bc = prefab.GetComponent <BoxCollider> ();
						if (bc != null) {
							prefab.layer = Globals.LayerNumStructureIgnoreCollider;
						} else {
							prefab.layer = Globals.LayerNumStructureCustomCollider;
						}
					}
				}
			}
		}

		public void RefreshPaths ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
				Mods.Get.Editor.InitializeEditor ();
			}
			if (!Manager.IsAwake <Structures> ()) {
				Manager.WakeUp <Structures> ("Frontiers_Structures");
			}

			string structurePacksPath = Structures.Get.LocalStructurePacksPath;
			structurePacksPath = System.IO.Path.Combine (Application.dataPath, structurePacksPath);
			if (Directory.Exists (structurePacksPath)) {
				//Debug.Log ("Found path " + structurePacksPath);
			} else {
				//Debug.Log ("Path " + structurePacksPath + " does not exist");
				return;
			}

			string[] pathNames = Directory.GetDirectories (structurePacksPath);

			Structures.Get.PackPaths.Clear ();

			foreach (string packPath in pathNames) {

				//Debug.Log ("Checking structure pack " + packPath);
				string[] subDirectoryNames = Directory.GetDirectories (packPath);
				if (subDirectoryNames.Length == 0) {
					//Debug.Log ("No subfolders in structure pack - skipping");
				} else {
					StructurePackPaths packPaths = new StructurePackPaths ();
					packPaths.PackPath = packPath.Replace (Application.dataPath, string.Empty);
					packPaths.Name = System.IO.Path.GetFileName (packPaths.PackPath);

					int numAssets = 0;
					foreach (string subDirectory in subDirectoryNames) {
						//Debug.Log ("Checking sub directory " + subDirectory);
						string currentFolderName = System.IO.Path.GetFileName (subDirectory);
						string[] assetPaths = Directory.GetFiles (subDirectory);
						List <string> ListToAddTo = null;

						switch (currentFolderName) {
						case "StaticPrefabs":
							ListToAddTo = packPaths.StaticPrefabs;
							foreach (string assetPath in assetPaths) {
								string extension = System.IO.Path.GetExtension (assetPath);
								//Debug.Log ("Extension: " + extension);
								if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
									string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
									if (!ListToAddTo.Contains (prefabAssetPath)) {
										ListToAddTo.Add (prefabAssetPath);
									}
									numAssets++;
								}
							}
							break;

						case "DynamicPrefabs":
							ListToAddTo = packPaths.DynamicPrefabs;
							foreach (string assetPath in assetPaths) {
								string extension = System.IO.Path.GetExtension (assetPath);
								if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
									string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
									if (!ListToAddTo.Contains (prefabAssetPath)) {
										ListToAddTo.Add (prefabAssetPath);
									}
									numAssets++;
								}
							}
							break;

						case "Meshes":
							ListToAddTo = packPaths.Meshes;
							foreach (string assetPath in assetPaths) {
								string extension = System.IO.Path.GetExtension (assetPath);
								if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".FBX", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".fbx", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".lxo", string.Empty);
									string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
									if (!ListToAddTo.Contains (prefabAssetPath)) {
										ListToAddTo.Add (prefabAssetPath);
									}
									numAssets++;
								}
							}
							break;

						case "Textures":
							ListToAddTo = packPaths.Textures;
							foreach (string assetPath in assetPaths) {
								string extension = System.IO.Path.GetExtension (assetPath);
								if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".PSD", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".psd", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".jpg", string.Empty);
									string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
									if (!ListToAddTo.Contains (prefabAssetPath)) {
										ListToAddTo.Add (prefabAssetPath);
									}
									numAssets++;
								}
							}
							break;

						case "Materials":
							ListToAddTo = packPaths.Materials;
							foreach (string assetPath in assetPaths) {
								string extension = System.IO.Path.GetExtension (assetPath);
								if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".material", string.Empty);
//								prefabAssetPath = prefabAssetPath.Replace (".mat", string.Empty);
									string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
									if (!ListToAddTo.Contains (prefabAssetPath)) {
										ListToAddTo.Add (prefabAssetPath);
									}
									numAssets++;
								}
							}
							break;
						default:
								//Debug.Log ("Folder name " + currentFolderName + " not recognized");
							break;
						}
					}
					//Debug.Log ("Num assets in pack: " + numAssets);
					Structures.Get.PackPaths.Add (packPaths);
				}
			}

			//get mesh collider paths
			ColliderMeshPaths.Clear ();
			string colliderMeshesPath = LocalColliderMeshesPath;
			colliderMeshesPath = System.IO.Path.Combine (Application.dataPath, colliderMeshesPath);
			if (Directory.Exists (structurePacksPath)) {
				string[] assetPaths = Directory.GetFiles (colliderMeshesPath);
				foreach (string assetPath in assetPaths) {
					string extension = System.IO.Path.GetExtension (assetPath);
					//Debug.Log ("Extension: " + extension);
					if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
						string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
						ColliderMeshPaths.SafeAdd (prefabAssetPath);
					}
				}
			} else {
				Debug.Log ("Path " + colliderMeshesPath + " does not exist");
			}

			//get LOD prefab paths
			LODPrefabPaths.Clear ();
			string lodPrefabsPath = LocalLODPrefabsPath;
			lodPrefabsPath = System.IO.Path.Combine (Application.dataPath, lodPrefabsPath);
			if (Directory.Exists (lodPrefabsPath)) {
				string[] assetPaths = Directory.GetFiles (lodPrefabsPath);
				foreach (string assetPath in assetPaths) {
					string extension = System.IO.Path.GetExtension (assetPath);
					//Debug.Log ("Extension: " + extension);
					if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
						string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
						LODPrefabPaths.SafeAdd (prefabAssetPath);
					}
				}
			} else {
				Debug.Log ("Path " + lodPrefabsPath + " does not exist");
			}

			//get shared material paths
			SharedMaterialPaths.Clear ();
			string sharedMaterialsPath = LocalSharedMaterialsPath;
			sharedMaterialsPath = System.IO.Path.Combine (Application.dataPath, sharedMaterialsPath);
			if (Directory.Exists (sharedMaterialsPath)) {
				string[] subDirectoryNames = Directory.GetDirectories (sharedMaterialsPath);
				foreach (string subDirectory in subDirectoryNames) {
					string[] assetPaths = Directory.GetFiles (subDirectory);
					foreach (string assetPath in assetPaths) {
						string extension = System.IO.Path.GetExtension (assetPath);
						//Debug.Log ("Extension: " + extension);
						if (extension != ".meta" && extension != string.Empty) {
//								string prefabAssetPath = assetPath.Replace (".prefab", string.Empty);
							string prefabAssetPath = assetPath.Replace (Application.dataPath, string.Empty);
							SharedMaterialPaths.SafeAdd (prefabAssetPath);
						}
					}
				}
			} else {
				Debug.Log ("Path " + sharedMaterialsPath + " does not exist");
			}

		}

		public void RebuildPacks ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
				Mods.Get.Editor.InitializeEditor ();
			}
			if (!Manager.IsAwake <Structures> ()) {
				Manager.WakeUp <Structures> ("Frontiers_Structures");
			}
			//Debug.Log ("Rebuilding structure manager packs from paths");
			Structures.Get.StructurePacks.Clear ();
			foreach (StructurePackPaths packPaths in Structures.Get.PackPaths) {
				//Debug.Log ("Loading pack " + packPaths.PackPath);
				StructurePack structurePack = new StructurePack ();
				structurePack.Name = System.IO.Path.GetFileName (packPaths.PackPath);

				foreach (string staticPrefabPath in packPaths.StaticPrefabs) {
					string finalPath = ("Assets" + staticPrefabPath);
					//Debug.Log ("Adding statpre " + finalPath);
					UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath (finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
					GameObject prefabAsGameObject = prefab as GameObject;
					if (prefabAsGameObject != null) {
						structurePack.StaticPrefabs.Add (prefabAsGameObject);
					}
				}

				foreach (string dynamicPrefab in packPaths.DynamicPrefabs) {
					string finalPath = ("Assets" + dynamicPrefab);
					//Debug.Log ("Adding dynpre " + finalPath);
					UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath (finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
					if (prefab != null) {
						GameObject prefabGameObject = prefab as GameObject;
						DynamicPrefab dynPrefab = prefabGameObject.GetComponent <DynamicPrefab> ();
						if (dynPrefab != null && dynPrefab.worlditem != null) {
							dynPrefab.worlditem.OnEditorRefresh ();
							dynPrefab.worlditem.Props.Name.FileName = prefabGameObject.name;
							dynPrefab.worlditem.Props.Name.PrefabName = prefabGameObject.name;
							dynPrefab.worlditem.Props.Name.PackName = structurePack.Name;
						}
						structurePack.DynamicPrefabs.Add (prefabGameObject);
					} else {
						//Debug.Log ("PREFAB WAS NULL " + System.IO.Path.GetFileName (dynamicPrefab));
					}
				}

				//Debug.Log ("Added structure pack FINISHED");
				Structures.Get.StructurePacks.Add (structurePack);
			}

			ColliderMeshPrefabs.Clear ();
			foreach (string colliderMeshPrefabPath in ColliderMeshPaths) {
				string finalPath = ("Assets" + colliderMeshPrefabPath);
				//Debug.Log ("Adding statpre " + finalPath);
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath (finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
				GameObject prefabAsGameObject = prefab as GameObject;
				if (prefabAsGameObject != null) {
					ColliderMeshPrefabs.SafeAdd (prefabAsGameObject);
				}
			}

			LodMeshPrefabs.Clear ();
			foreach (string lodMeshPrefabPath in LODPrefabPaths) {
				string finalPath = ("Assets" + lodMeshPrefabPath);
				//Debug.Log ("Adding statpre " + finalPath);
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath (finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
				GameObject prefabAsGameObject = prefab as GameObject;
				if (prefabAsGameObject != null) {
					LodMeshPrefabs.SafeAdd (prefabAsGameObject);
				}
			}

			SharedMaterials.Clear ();
			foreach (string sharedMaterialPath in SharedMaterialPaths) {
				string finalPath = ("Assets" + sharedMaterialPath);
				//Debug.Log ("Adding statpre " + finalPath);
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath (finalPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
				Material prefabAsMaterial = prefab as Material;
				if (prefabAsMaterial != null) {
					SharedMaterials.SafeAdd (prefabAsMaterial);
				}
			}
		}

		public void DrawEditor ()
		{

			if (GUILayout.Button ("Disable object colliders")) {
				foreach (StructurePack pack in StructurePacks) {
					Debug.Log ("Disabling colliders in " + pack.Name);
					foreach (GameObject prefab in pack.StaticPrefabs) {
						Collider[] colliders = prefab.GetComponents <Collider> ();
						Debug.Log ("Disabling " + colliders.Length.ToString () + " in " + prefab.name);
						foreach (Collider c in colliders) {
							c.enabled = false;
						}
						foreach (Transform child in prefab.transform) {
							if (child.collider != null) {
								child.collider.enabled = false;
							}
						}
					}
				}
			}

			if (GUILayout.Button ("Refresh Structure Manager")) {
				RefreshPaths ();
				RebuildPacks ();
				UnityEditor.EditorUtility.SetDirty (this);
				UnityEditor.EditorUtility.SetDirty (gameObject);
			}

			if (GUILayout.Button ("Prep LOD Prefabs")) {
				foreach (GameObject gObject in UnityEditor.Selection.gameObjects) {
					MeshFilter finalMFilter = gObject.GetOrAdd <MeshFilter> ();
					foreach (Transform child in gObject.transform) {
						MeshFilter childMFilter = child.gameObject.GetComponent <MeshFilter> ();
						finalMFilter.sharedMesh = childMFilter.sharedMesh;
						GameObject.DestroyImmediate (child.gameObject);
						break;
					}

					GameObject prefab = UnityEditor.PrefabUtility.CreatePrefab ("Assets/Artwork/Packs/_LODPrefabs/" + gObject.name + ".prefab", gObject, ReplacePrefabOptions.Default);
					LodMeshPrefabs.Add (prefab);
				}
			}

			if (GUILayout.Button ("Prep Material Substitutions")) {
				foreach (MaterialSubstitution m in Substitutions) {
					m.OriginalMaterials.Sort (delegate(Material m1, Material m2) {
						return m1.name.CompareTo (m2.name);
					});
					//sort and get rid of all duplicates across the board
					for (int i = m.OriginalMaterials.LastIndex (); i >= 0; i--) {
						for (int j = m.OriginalMaterials.LastIndex (); j >= 0; j--) {
							if (j == i) {
								continue;
							}
							if (m.OriginalMaterials [j] == m.OriginalMaterials [i]) {
								m.OriginalMaterials.RemoveAt (i);
							}
						}
						foreach (MaterialSubstitution otherM in Substitutions) {
							if (m == otherM) {
								continue;
							}
							foreach (Material om in otherM.OriginalMaterials) {
								if (om == m.OriginalMaterials [i]) {
									m.OriginalMaterials.RemoveAt (i);
								}
							}
						}
					}
				}
			}

			if (GUILayout.Button ("Convert materials to detail shader")) {
				foreach (UnityEngine.Object matObject in UnityEditor.Selection.objects) {
					/*					
					 		_DetailBlendBias ("Detail Blend Bias", Range (0, 1)) = 1
							_DetailBlend ("Detail Blend (R)", 2D) = "black" {}
							
							_MainSpecLvl ("Main Shininess", Range(0.01, 1.0)) = 0.8
							_MainGlosLvl ("Main Glossiness Level", Float) = 0.5
					      	_MainSpecColor ("Main Specular Color", Color) = (0.5, 0.5, 0.5, 1)
							_MainTintColor ("Main Tint Color", Color) = (1,1,1,1)
							_MainDiffMap ("Main Diffuse (RGB) Gloss (A)", 2D) = "white" {}
							
							_MainBumpiness ("Main Bumpiness", Range (0, 1)) = 1.0
							_MainBumpMap ("Main Bump Map (Normalmap)", 2D) = "bump" {}

							_DetailSpecLvl ("Detail Shininess", Range(0.01, 1.0)) = 0.8
							_DetailGlosLvl ("Detail Glossiness Level", Float) = 0.5
					      	_DetailSpecColor ("Detail Specular Color", Color) = (0.5, 0.5, 0.5, 1)
							_DetailTintColor ("Detail Tint Color", Color) = (1,1,1,1)
							_DetailDiffMap ("Detail Diffuse (RGB) Gloss (A)", 2D) = "black" {}
							
							_DetailBumpiness ("Detail Bumpiness", Range (0, 1)) = 1.0
							_DetailBumpMap ("Detail Bump Map (Normalmap)", 2D) = "bump" {}
					*/
					/*
					_Color ("Main Color", Color) = (1,1,1,1)
					                                       _Opacity ("Color over opacity", Range (0, 1)) = 1
					                                       _MainTex ("Color over (RGBA)", 2D) = "white" {}
					_BumpMap ("Normalmap over", 2D) = "bump" {}
					_MainTex2 ("Color under (RGBA)", 2D) = "white" {}
					_BumpMap2 ("Normalmap under", 2D) = "bump" {}
					*/

					Material mat = matObject as Material;

					Texture mainTexure = mat.GetTexture ("_MainTex2");
					Texture mainBump = mat.GetTexture ("_BumpMap2");
					Texture detailTexture = mat.GetTexture ("_MainTex");
					Texture detailBump = mat.GetTexture ("_BumpMap");
					Color mainColor = Color.white;
					Color specColor = Color.white;
					Vector2 tilingDetail = new Vector2 (5f, 5f);
					Vector2 tilingMain = new Vector2 (1f, 1f);

					float opacity = 1.0f;
					float shininess = 0.5f;
					bool apply = true;
					if (mat != null) {
						if (mat.shader.name.Contains ("BumpedDiffuseOverlay")) {
							mainTexure = mat.GetTexture ("_MainTex2");
							mainBump = mat.GetTexture ("_BumpMap2");
							detailTexture = mat.GetTexture ("_MainTex");
							detailBump = mat.GetTexture ("_BumpMap");
							mainColor = mat.GetColor ("_Color");
							opacity = mat.GetFloat ("_Opacity");
							tilingDetail = mat.GetTextureScale ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex2");

						} else if (mat.shader.name.Contains ("Bumped Specular")) {
							mainTexure = mat.GetTexture ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex");
							mainBump = mat.GetTexture ("_BumpMap");
							mainColor = mat.GetColor ("_Color");
							specColor = mat.GetColor ("_SpecColor");
							shininess = mat.GetFloat ("_Shininess");

							detailTexture = DefaultDetailTexture;
							detailBump = DefaultDetailBump;

						} else if (mat.shader.name.Contains ("Bumped Diffuse")) {
							mainTexure = mat.GetTexture ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex");
							mainBump = mat.GetTexture ("_BumpMap");
							mainColor = mat.GetColor ("_Color");
							detailTexture = DefaultDetailTexture;
							detailBump = DefaultDetailBump;
						} else if (mat.shader.name.Contains ("Diffuse Detail")) {
							mainTexure = mat.GetTexture ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex");
							mainColor = mat.GetColor ("_Color");
							detailTexture = mat.GetTexture ("_Detail");
							tilingDetail = mat.GetTextureScale ("_Detail");
							detailBump = DefaultDetailBump;
							mainBump = DefaultBump;

						} else if (mat.shader.name.Contains ("Diffuse")) {
							mainTexure = mat.GetTexture ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex");
							mainColor = mat.GetColor ("_Color");
							detailTexture = DefaultDetailTexture;
							detailBump = DefaultDetailBump;
							mainBump = DefaultBump;
						} else if (mat.shader.name.Contains ("Specular")) {
							mainTexure = mat.GetTexture ("_MainTex");
							tilingMain = mat.GetTextureScale ("_MainTex");
							mainBump = DefaultBump;
							mainColor = mat.GetColor ("_Color");
							specColor = mat.GetColor ("_SpecColor");
							shininess = mat.GetFloat ("_Shininess");

							detailTexture = DefaultDetailTexture;
							detailBump = DefaultDetailBump;
						} else {
							apply = false;
						}

						if (apply) {
							mat.shader = Shader.Find ("Detail/BumpedSpecularWithDetail");
							mat.SetTexture ("_MainDiffMap", mainTexure);
							mat.SetTexture ("_MainBumpMap", mainBump);
							mat.SetTextureScale ("_MainDiffMap", tilingMain);
							mat.SetTextureScale ("_MainBumpMap", tilingMain);
							mat.SetFloat ("_MainBumpiness", 1.0f);

							mat.SetColor ("_DetailTintColor", mainColor);
							mat.SetColor ("_MainTintColor", mainColor);
							mat.SetColor ("_MainSpecColor", specColor);
							mat.SetColor ("_DetailSpecColor", specColor);

							mat.SetTexture ("_DetailDiffMap", detailTexture);
							mat.SetTexture ("_DetailBumpMap", detailBump);
							mat.SetTextureScale ("_DetailDiffMap", tilingDetail);
							mat.SetTextureScale ("_DetailBumpMap", tilingDetail);
							mat.SetFloat ("_DetailBumpiness", 1.0f);

							mat.SetTexture ("_DetailBlend", mainTexure);
							mat.SetFloat ("_DetailBlendBias", opacity);

							mat.SetFloat ("_MainSpecLvl", shininess);
							mat.SetFloat ("_DetailSpecLvl", shininess);
						}
					}
				}
			}

			if (!Application.isPlaying)
				return;

			UnityEngine.GUI.color = Color.green;
			GUILayout.Label ("Exteriors waiting to load:");
			foreach (Structure structure in ExteriorsWaitingToLoad) {
				//for (int i = ExteriorsWaitingToLoad.LastIndex(); i >= 0; i--) {
				//Structure structure = ExteriorsWaitingToLoad[i];
				if (structure != null) {
					if (GUILayout.Button (structure.name + " - " + structure.LoadState.ToString ())) {
						//ExteriorsWaitingToLoad.RemoveAt (i);
					}
				}
			}


			UnityEngine.GUI.color = Color.cyan;
			GUILayout.Label ("\nInteriors waiting to load:");
			foreach (Structure structure in InteriorsWaitingToLoad) {
				if (structure != null) {
					if (GUILayout.Button (structure.name + " - " + structure.LoadState.ToString ())) {
						//InteriorsWaitingToLoad.RemoveAt (i);
					}
				}
			}

			UnityEngine.GUI.color = Color.blue;
			GUILayout.Label ("\nMinor structures waiting to load:");
			foreach (MinorStructure structure in MinorsWaitingToLoad) {
				if (structure != null) {
					if (GUILayout.Button (structure.TemplateName + " - " + structure.LoadState.ToString ())) {
						//MinorsWaitingToLoad.RemoveAt (i);
					}
				}
			}

			UnityEngine.GUI.color = Color.red;
			GUILayout.Label ("\nExteriors waiting to unload:");
			foreach (Structure structure in ExteriorsWaitingToLoad) {
				if (structure != null) {
					if (GUILayout.Button (structure.name + " - " + structure.LoadState.ToString ())) {
						//ExteriorsWaitingToUnload.RemoveAt (i);
					}
				}
			}


			UnityEngine.GUI.color = Color.yellow;
			GUILayout.Label ("\nInteriors waiting to unload:");
			foreach (Structure structure in InteriorsWaitingToLoad) {
				if (structure != null) {
					if (GUILayout.Button (structure.name + " - " + structure.LoadState.ToString ())) {
						//InteriorsWaitingToUnload.RemoveAt (i);
					}
				}
			}


			UnityEngine.GUI.color = Color.yellow;
			GUILayout.Label ("\nMinor structures waiting to unload:");
			foreach (MinorStructure structure in MinorsWaitingToUnload) {
				if (structure != null) {
					if (GUILayout.Button (structure.TemplateName + " - " + structure.LoadState.ToString ())) {
						//MinorsWaitingToUnload.RemoveAt (i);
					}
				}
			}

			UnityEditor.EditorUtility.SetDirty (gameObject);
			UnityEditor.EditorUtility.SetDirty (this);
		}

		public void EditorInstantiateChunkPrefab (ChunkPrefab chunkPrefab, Transform tr)
		{
			StructurePack pack = null;
			if (mStructurePackLookup.TryGetValue (chunkPrefab.PackName, out pack)) {
				StructurePackPrefab prefab = null;
				if (pack.StaticPrefabLookup.TryGetValue (chunkPrefab.PrefabName, out prefab)) {
					GameObject scenery = GameObject.Instantiate (prefab.Prefab) as GameObject;
					scenery.transform.parent = tr;
					chunkPrefab.Transform.ApplyTo (scenery.transform, true);
					scenery.name = chunkPrefab.Name;
					scenery.tag = chunkPrefab.Tag;

					List <Material> materials = new List<Material> ();
					foreach (string materialName in chunkPrefab.SharedMaterialNames) {
						Material mat = null;
						if (mSharedMaterialLookup.TryGetValue (materialName, out mat)) {
							materials.Add (mat);
						}
					}

					MeshRenderer mr = scenery.GetComponent <MeshRenderer> ();
					mr.sharedMaterials = materials.ToArray ();
				}
			}
		}

		public static void InstantiateAllPrefabs ()
		{//for memory testing
			foreach (StructurePack pack in Get.StructurePacks) {
				foreach (GameObject packPrefab in pack.StaticPrefabs) {
					GameObject.Instantiate (packPrefab, Vector3.zero, Quaternion.identity);
				}
			}
		}
		#endif
	}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;
using Hydrogen.Threading.Jobs;

namespace Frontiers.World
{
	public partial class StructureBuilder : Builder
	{
		public static IEnumerator InstanceMeshes (
			Builder.BuilderMode mode,
			MinorStructure minorParent,
			Transform structureBase,
			StructureTemplateGroup structureGroup,
			string childName,
			List <Renderer> renderers,
			List <Renderer> lodRenderers,
			float lodRatio,
			bool destroyed)
		{
			renderers.Clear ();
			if (lodRenderers != null) {
				lodRenderers.Clear ();
			}

			Transform structurePiece = structureBase.gameObject.CreateChild (childName);
			structurePiece.parent = null;
			if (mode == Builder.BuilderMode.Minor) {
				//minor structures have to set their own offsets
				structurePiece.position = minorParent.Position;
				structurePiece.rotation = Quaternion.Euler (minorParent.Rotation);
			} else {
				structurePiece.ResetLocal ();
			}
			structurePiece.gameObject.layer = Globals.LayerNumStructureCustomCollider;//TODO this may be unnecessary
			for (int i = 0; i < structureGroup.StaticStructureLayers.Count; i++) {
				structureGroup.StaticStructureLayers [i].LayerNum = i;
				StructureTemplate.InstantiateStructureLayer (structureGroup.StaticStructureLayers [i], structurePiece);
			}

			structurePiece.parent = structureBase;
			if (mode == Builder.BuilderMode.Minor) {
				structurePiece.localPosition = minorParent.Position;
				structurePiece.localRotation = Quaternion.Euler (minorParent.Rotation);
			} else {
				structurePiece.ResetLocal ();
			}
			yield break;
		}

		public static IEnumerator GenerateColliders (
			string childName,
			List <StructureTerrainLayer> layers,
			List <BoxCollider> boxColliders,
			List <MeshCollider> meshColliders,
			List <BoxCollider> boxCollidersDestroyed,
			List <MeshCollider> meshCollidersDestroyed,
			List <MeshFilter> meshes,
			List <StructureLayer> structureColliders,
			bool interior,
			Builder builder)
		{
			if (builder.Mode == BuilderMode.Minor) {
				builder.StructurePiece = builder.MinorParent.StructureOwner.gameObject.FindOrCreateChild (childName + "_MINOR_COL");
				builder.StructurePiece.localPosition = builder.MinorParent.Position;
				builder.StructurePiece.localEulerAngles = builder.MinorParent.Rotation;
			} else {
				builder.StructurePiece = builder.StructureBase.gameObject.FindOrCreateChild (childName + "_COL");
			}

			StructureLayer colliderLayer = null;
			ChildPiece childPiece = ChildPiece.Empty;
			BoxCollider boxCollider = null;
			MeshCollider meshCollider = null;
			Transform customColliderTr = null;
			Mesh sharedMesh = null;
			List <BoxCollider> bcList = null;
			List <MeshCollider> mcList = null;

			for (int i = 0; i < structureColliders.Count; i++) {
				colliderLayer = structureColliders [i];

				if (colliderLayer.DestroyedBehavior == StructureDestroyedBehavior.Destroy) {
					bcList = boxCollidersDestroyed;
					mcList = meshCollidersDestroyed;
				} else {
					bcList = boxColliders;
					mcList = meshColliders;
				}

				if (colliderLayer.InstancePieces != null) {
					switch (colliderLayer.PackName) {
					case "MeshCollider":
						if (Structures.Get.ColliderMesh (colliderLayer.PrefabName, out sharedMesh)) {
							for (int j = 0; j < colliderLayer.InstancePieces.Count; j++) {
								childPiece = colliderLayer.InstancePieces [j];
								//get the collider from the child piece pack/prefab name
								meshCollider = Structures.Get.MeshColliderFromPool ();
								meshCollider.name = colliderLayer.PrefabName;
								customColliderTr = meshCollider.transform;
								customColliderTr.parent = builder.StructurePiece;

								meshCollider.tag = colliderLayer.Tag;
								meshCollider.gameObject.layer = Globals.LayerNumStructureTerrain;

								customColliderTr.localPosition = childPiece.Position;
								customColliderTr.localRotation = Quaternion.Euler (childPiece.Rotation);
								customColliderTr.localScale = childPiece.Scale;

								meshCollider.sharedMesh = sharedMesh;

								if (meshColliders != null) {
									meshColliders.Add (meshCollider);
								}
							}
						} else {
							//Debug.Log("Couldn't get mesh collider " + colliderLayer.PrefabName);
						}
						break;

					case "BoxCollider":
						for (int j = 0; j < colliderLayer.InstancePieces.Count; j++) {
							childPiece = colliderLayer.InstancePieces [j];
							boxCollider = Structures.Get.BoxColliderFromPool ();
							boxCollider.name = colliderLayer.PrefabName;
							customColliderTr = boxCollider.transform;
							customColliderTr.parent = builder.StructurePiece;

							boxCollider.tag = colliderLayer.Tag;
							boxCollider.gameObject.layer = Globals.LayerNumStructureTerrain;

							customColliderTr.localPosition = childPiece.Position;
							customColliderTr.localRotation = Quaternion.Euler (childPiece.Rotation);
							customColliderTr.localScale = childPiece.Scale;

							if (boxColliders != null) {
								boxColliders.Add (boxCollider);
							}
						}
						break;

					default:
						break;
					}
					//yield return null;
					StructureTerrainLayer terrainLayer = builder.StructurePiece.gameObject.GetOrAdd <StructureTerrainLayer> ();
					//terrainLayer.rb.detectCollisions = false;
					layers.SafeAdd (terrainLayer);
				}

				if (!interior && builder.Priority != StructureLoadPriority.SpawnPoint && Player.Local.HasSpawned) {
					//if we don't have to get inside this structure >rightaway<
					//smooth out the wait a bit
					double start = Frontiers.WorldClock.RealTime;
					while (Frontiers.WorldClock.RealTime < start + 0.01f) {
						yield return null;
					}
				}
			}
			//}
			yield break;
		}

		public static IEnumerator InstantiateMeshes (
			Structure parentStructure,
			Structure cachedStructure,
			Builder builder)
		{
			//lock the structure so it won't unload while we're doing this
			//cachedStructure.InUseAsTemplate++;
			//duplicate all of its meshes piece for piece
			//start with the normal meshes
			builder.StructurePiece = builder.StructureBase.FindOrCreateChild (GetChildName (false, false));
			for (int i = 0; i < cachedStructure.ExteriorRenderers.Count; i++) {
				try {
					GameObject newRenderer = GameObject.Instantiate (cachedStructure.ExteriorRenderers [i].gameObject) as GameObject;
					newRenderer.transform.parent = builder.StructurePiece;
					newRenderer.transform.localPosition = Vector3.zero;
					newRenderer.transform.localRotation = Quaternion.identity;
				} catch (Exception e) {
					Debug.LogError ("Proceeding normally: Error duplicating mesh in " + builder.StructurePiece.name + ": " + e.ToString ());
				}
			}
			//now do the destroyed meshes if there are any
			if (cachedStructure.ExteriorRenderersDestroyed.Count > 0) {
				builder.StructurePiece = builder.StructureBase.FindOrCreateChild (GetChildName (false, true));
				for (int i = 0; i < cachedStructure.ExteriorRenderers.Count; i++) {
					try {
						Transform newRendererTransform = GameObject.Instantiate (cachedStructure.ExteriorRenderers [i].gameObject) as Transform;
						newRendererTransform.parent = builder.StructurePiece;
						newRendererTransform.localPosition = Vector3.zero;
						newRendererTransform.localRotation = Quaternion.identity;
					} catch (Exception e) {
						Debug.LogError ("Proceeding normally: Error duplicating mesh in " + builder.StructurePiece.name + ": " + e.ToString ());
					}
				}
			}
			//done!
			//let the cached structure unload if it wants
			//cachedStructure.InUseAsTemplate--;
			yield break;
		}

		public static IEnumerator GenerateCachedMesh (
			Structures.CachedMesh cachedMesh,
			string childName,
			List <MeshFilter> meshes,
			List <Renderer> renderers,
			List <Renderer> renderersDestroyed,
			List <Renderer> lodRenderers,
			List <Renderer> lodRenderersDestroyed,
			bool interior,
			Builder builder)
		{
			while (!cachedMesh.Finished) {
				Debug.Log ("Waiting for cached mesh to be finished");
				yield return null;
			}

			for (int i = 0; i < cachedMesh.meshes.Count; i++) {
				//Debug.Log ("GENERATING CACHED MESH IN STRUCTURE");
				GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild (cachedMesh.meshes [i].name).gameObject;
				subMeshGo.layer = cachedMesh.GameObjectLayer;
				subMeshGo.tag = cachedMesh.Tag;
				MeshFilter meshFilter = subMeshGo.AddComponent <MeshFilter> ();
				Renderer meshRenderer = subMeshGo.AddComponent <MeshRenderer> ();
				meshRenderer.enabled = false;
				meshFilter.sharedMesh = cachedMesh.meshes [i];
				meshRenderer.sharedMaterials = cachedMesh.materials [i];
				meshRenderer.gameObject.isStatic = true;
				meshes.Add (meshFilter);

				//create an LOD version for exteriors that use different materials
				if (!interior) {
					GameObject subMeshLOD = subMeshGo.CreateChild ("LOD").gameObject;
					subMeshLOD.layer = subMeshGo.layer;
					Renderer lodRenderer = subMeshLOD.AddComponent <MeshRenderer> ();
					MeshFilter lodMf = subMeshLOD.AddComponent <MeshFilter> ();
					lodMf.sharedMesh = meshFilter.sharedMesh;
					if (cachedMesh.lodMaterials [i] == null) {
						cachedMesh.lodMaterials [i] = Structures.Get.LODMaterials (cachedMesh.materials [i]);
					}
					lodRenderer.sharedMaterials = cachedMesh.lodMaterials [i];

					LOD[] lods = new LOD [] {
						new LOD (0.6f,
							new Renderer [] { meshRenderer }),
						new LOD (0.05f,
							new Renderer [] { lodRenderer })
					};
					LODGroup lod = subMeshGo.AddComponent <LODGroup> ();
					lod.SetLODS (lods);
					lod.RecalculateBounds ();
					lodRenderers.Add (lodRenderer);
				}

				if (cachedMesh.Destroyed) {
					if (cachedMesh.LOD) {
						lodRenderersDestroyed.Add (meshRenderer);
					} else {
						renderersDestroyed.Add (meshRenderer);
					}
				} else {
					if (cachedMesh.LOD) {
						lodRenderers.Add (meshRenderer);
					} else {
						renderers.Add (meshRenderer);
					}
				}
				if (!interior && Player.Local.HasSpawned && TravelManager.Get.State != FastTravelState.Traveling) {
					yield return null;
				}
			}

			yield break;
		}

		public static IEnumerator GenerateMeshes (
			StructureTemplateGroup structureGroup,
			MeshCombiner combiner,
			MeshCombiner lodCombiner,
			MeshCombiner destroyedCombiner,
			MeshCombiner destroyedLodCombiner,
			string childName,
			List <MeshFilter> meshes,
			List <Renderer> renderers,
			List <Renderer> renderersDestroyed,
			List <Renderer> lodRenderers,
			List <Renderer> lodRenderersDestroyed,
			bool interior,
			Builder builder,
			string templateName)
		{
			if (structureGroup.StaticStructureLayers.Count <= 0) {
				yield break;
			}

			if (gHelperTransform == null) {
				gHelperTransform = Structures.Get.gameObject.CreateChild ("Structure Helper Transform");
			}

			//only generate LOD meshes if we have a combiner and a renderer for the task
			bool createLodMeshes = false;//(lodCombiner != null && lodRenderers != null);
			bool createDestroyedMeshes = false;
			bool createLodDestroyedMeshes = false;
			bool meshesFinished = false;
			bool lodMeshesFinished = false;
			bool meshesDestroyedFinished = false;
			bool lodDestroyedMeshesFinished = false;

			if (!createLodMeshes) {
				lodMeshesFinished = true;
				lodDestroyedMeshesFinished = true;
			}

			//create the transform that the newly built exterior will exist under
			//it may exist from a previous build so use find or create
			builder.StructurePiece = builder.StructureBase.FindOrCreateChild (childName);
			builder.StructurePiece.parent = null;
			if (builder.Mode == Builder.BuilderMode.Minor) {
				//minor structures have to set their own offsets
				builder.StructurePiece.position = builder.MinorParent.Position;
				builder.StructurePiece.rotation = Quaternion.Euler (builder.MinorParent.Rotation);
				//the minor structure container aids in the disposal of minor structure components
				MinorStructureContainer msc = builder.StructurePiece.gameObject.GetOrAdd <MinorStructureContainer> ();
				msc.Parent = builder.MinorParent;
				builder.MinorParent.Container = msc;
			} else {
				builder.StructurePiece.ResetLocal ();
			}
			builder.StructurePiece.gameObject.layer = Globals.LayerNumStructureTerrain;//TODO this may be unnecessary
			//we use this to generate our LOD at the end

			//see if we've created the meshes before
			MeshCombinerResult result = new MeshCombinerResult ();
			MeshCombinerResult resultDestroyed = new MeshCombinerResult ();
			MeshCombinerResult lodResult = null;
			MeshCombinerResult lodResultDestroyed = null;

			renderers.Clear ();
			if (createLodMeshes) {
				lodResult = new MeshCombinerResult ();
				lodRenderers.Clear ();
				lodResultDestroyed = new MeshCombinerResult ();
				lodRenderersDestroyed.Clear ();
			}
			builder.MaterialLookup.Clear ();

			#region structure layer
			bool generatedMeshes = false;
			//split the structure group's static pieces into ChildPieces
			//they should be arranged by layer and tag
			//each set will be a different mesh combiner job
			//int vertexCount = 0;
			StructureLayer staticLayer = null;
			for (int i = 0; i < structureGroup.StaticStructureLayers.Count; i++) {
				staticLayer = structureGroup.StaticStructureLayers [i];
				staticLayer.LayerNum = i;

				bool generateMesh = false;
				//see if we've got a mesh cached for the main undestroyed non-lod portion
				Structures.CachedMesh cachedMesh;
				if (Structures.Get.GetCachedMesh (templateName, interior, false, false, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag, out cachedMesh)) {
					var cachedMeshEnum = GenerateCachedMesh (
						                     cachedMesh,
						                     childName,
						                     meshes,
						                     renderers,
						                     renderersDestroyed,
						                     lodRenderers,
						                     lodRenderersDestroyed,
						                     interior,
						                     builder);
					while (cachedMeshEnum.MoveNext ()) {
						yield return cachedMeshEnum.Current;
					}
				} else {
					generateMesh = true;
				}
				//see if we've got our lod mesh cached
				if (createLodMeshes) {
					if (Structures.Get.GetCachedMesh (templateName, interior, false, false, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag, out cachedMesh)) {
						var cachedMeshEnum = GenerateCachedMesh (
							                     cachedMesh,
							                     childName,
							                     meshes,
							                     renderers,
							                     renderersDestroyed,
							                     lodRenderers,
							                     lodRenderersDestroyed,
							                     interior,
							                     builder);
						while (cachedMeshEnum.MoveNext ()) {
							yield return cachedMeshEnum.Current;
						}
					} else {
						generateMesh = true;
					}
				}
				//see if we've got our destroyed meshes cached
				if (createDestroyedMeshes) {
					if (Structures.Get.GetCachedMesh (templateName, interior, true, false, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag, out cachedMesh)) {
						var cachedMeshEnum = GenerateCachedMesh (
							                     cachedMesh,
							                     childName,
							                     meshes,
							                     renderers,
							                     renderersDestroyed,
							                     lodRenderers,
							                     lodRenderersDestroyed,
							                     interior,
							                     builder);
						while (cachedMeshEnum.MoveNext ()) {
							yield return cachedMeshEnum.Current;
						}
					} else {
						generateMesh = true;
					}
					if (createLodDestroyedMeshes) {
						if (Structures.Get.GetCachedMesh (templateName, interior, true, true, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag, out cachedMesh)) {
							var cachedMeshEnum = GenerateCachedMesh (
								                     cachedMesh,
								                     childName,
								                     meshes,
								                     renderers,
								                     renderersDestroyed,
								                     lodRenderers,
								                     lodRenderersDestroyed,
								                     interior,
								                     builder);
							while (cachedMeshEnum.MoveNext ()) {
								yield return cachedMeshEnum.Current;
							}
						} else {
							generateMesh = true;
						}
					}
				}

				if (generateMesh) {
					//looks like we couldn't find it cached
					//so create it now
					//this means we'll have to wait on meshes
					generatedMeshes = true;
					//send it on over
					builder.ChildPiecesSent = false;
					var childPiecesEnum = SendChildPieceToMeshCombiner (staticLayer, combiner, lodCombiner, destroyedCombiner, destroyedLodCombiner, builder.MaterialLookup, builder.StructurePiece, builder);
					while (childPiecesEnum.MoveNext ()) {
						yield return childPiecesEnum.Current;
					}

					if (builder.ChildPiecesSent) {
						yield return null;
					} else {
						//Debug.Log("DIDN'T SEND CHILD PIECES TO MESH COMBINER in " + builder.name);
						//yiked clear everything out
						builder.State = BuilderState.Error;
						combiner.ClearMeshes ();
						combiner.ClearMaterials ();
						if (lodCombiner != null) {
							lodCombiner.ClearMeshes ();
							lodCombiner.ClearMaterials ();
						}
						if (destroyedCombiner != null) {
							destroyedCombiner.ClearMeshes ();
							destroyedCombiner.ClearMaterials ();
						}
						if (destroyedLodCombiner != null) {
							destroyedLodCombiner.ClearMeshes ();
							destroyedLodCombiner.ClearMaterials ();
						}
						Debug.Log ("Child pieces not built, breaking");
						break;
					}
				}
			}
			#endregion

			#region deal with generated meshes

			double timeout = WorldClock.AdjustedRealTime + 10f;

			if (generatedMeshes) {
				//create all the cached meshes immediately, so other structures know we got here first
				Structures.CachedMesh normalCachedMesh = new Structures.CachedMesh (interior, false, false, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag);
				Structures.CachedMesh destroyedCachedMesh = null;
				Structures.CachedMesh lodCachedMeshes = null;
				Structures.CachedMesh lodDestroyedCachedMeshes = null;

				builder.State = Builder.BuilderState.WaitingForMeshes;
				//combine the meshes!
				combiner.Combine (result.MeshCombinerCallback);
				//are we creating lod meshes
				if (createLodMeshes) {
					lodCachedMeshes = new Structures.CachedMesh (interior, false, true, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag);
					if (lodCombiner.MeshInputCount > 0) {
						lodCombiner.Combine (lodResult.MeshCombinerCallback);
						//only create lod destroyed meshes if we're creating lod meshes
						if (destroyedLodCombiner.MeshInputCount > 0) {
							destroyedLodCombiner.Combine (lodResultDestroyed.MeshCombinerCallback);
							createLodDestroyedMeshes = true;
							lodDestroyedCachedMeshes = new Structures.CachedMesh (interior, false, true, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag);
							Structures.Get.AddCachedMesh (templateName, lodDestroyedCachedMeshes);
						} else {
							lodResultDestroyed.Clear ();
							lodDestroyedMeshesFinished = true;
							createLodDestroyedMeshes = false;
						}
					} else {
						lodResult.Clear ();
						createLodMeshes = false;
						lodMeshesFinished = true;
						lodDestroyedMeshesFinished = true;
					}
				}
				//do we have any destroyed meshes to create? if so build them
				if (destroyedCombiner.MeshInputCount > 0) {
					destroyedCombiner.Combine (resultDestroyed.MeshCombinerCallback);
					createDestroyedMeshes = true;
					destroyedCachedMesh = new Structures.CachedMesh (interior, true, false, staticLayer.LayerNum, staticLayer.Layer, staticLayer.Tag);
				} else {
					meshesDestroyedFinished = true;
					createDestroyedMeshes = false;
				}

				//add the cached meshes immediately
				Structures.Get.AddCachedMesh (templateName, normalCachedMesh);
				Structures.Get.AddCachedMesh (templateName, destroyedCachedMesh);
				Structures.Get.AddCachedMesh (templateName, lodCachedMeshes);
				Structures.Get.AddCachedMesh (templateName, lodDestroyedCachedMeshes);

				while (builder.State == Builder.BuilderState.WaitingForMeshes) {
					if (!meshesFinished) {
						combiner.Check ();
						if (result.MeshOutputs != null) {
							meshesFinished = true;
						}
					}
					if (!meshesDestroyedFinished) {
						destroyedCombiner.Check ();
						if (resultDestroyed.MeshOutputs != null) {
							meshesDestroyedFinished = true;
						}
					}

					if (!lodMeshesFinished) {
						lodCombiner.Check ();
						if (lodResult.MeshOutputs != null) {
							lodMeshesFinished = true;
						}
					}
					if (!lodDestroyedMeshesFinished) {
						destroyedLodCombiner.Check ();
						if (lodResultDestroyed.MeshOutputs != null) {
							lodDestroyedMeshesFinished = true;
						}
					}

					if (meshesFinished && lodMeshesFinished && meshesDestroyedFinished && lodDestroyedMeshesFinished) {
						//Debug.Log ("Meshes finished, moving on");
						builder.State = Builder.BuilderState.HandlingMeshes;
					}
					yield return null;//TODO add some sort of timeout?
					if (WorldClock.AdjustedRealTime > timeout) {
						builder.State = BuilderState.Error;
						Debug.LogError ("Timed out when building minor structure, returning now");
						yield break;
					}
				}
				//the mesh results should have been sent to our public props
				//take them and put them in the right place now
				if (builder.StructurePiece == null || builder.StructureBase == null) {
					builder.State = Builder.BuilderState.Error;
					//something happened while we were away
					//cancel operation
				} else {
					for (int j = 0; j < result.MeshOutputs.Length; j++) {
						var newMesh = combiner.CreateMeshObject (result.MeshOutputs [j], builder.MaterialLookup);
						//create a new child under the structure piece
						if (gNameBuilder == null) {
							gNameBuilder = new System.Text.StringBuilder ();
						}
						gNameBuilder.Clear ();
						gNameBuilder.Append (GetMeshPrefix (interior, false));
						gNameBuilder.Append (j.ToString ());
						gNameBuilder.Append (".");
						gNameBuilder.Append (staticLayer.Tag);
						gNameBuilder.Append (".");
						gNameBuilder.Append (staticLayer.Layer.ToString ());
						newMesh.Mesh.name = gNameBuilder.ToString ();

						GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild (newMesh.Mesh.name).gameObject;
						subMeshGo.layer = staticLayer.Layer;
						subMeshGo.tag = staticLayer.Tag;
						MeshFilter meshFilter = subMeshGo.AddComponent <MeshFilter> ();
						Renderer meshRenderer = subMeshGo.AddComponent <MeshRenderer> ();
						//make sure we don't see it zipping around
						meshRenderer.enabled = interior;
						//meshRenderer.enabled = false;
						meshFilter.sharedMesh = newMesh.Mesh;
						meshRenderer.sharedMaterials = newMesh.Materials;
						meshRenderer.gameObject.isStatic = true;
						meshes.Add (meshFilter);
						renderers.Add (meshRenderer);
						//wait a while before we create the next one
						//longer if we're not immediate
						normalCachedMesh.meshes.Add (meshFilter.sharedMesh);
						Material[] materials = meshRenderer.sharedMaterials;
						Material[] lodMaterials = Structures.Get.LODMaterials (materials);
						normalCachedMesh.materials.Add (materials);
						normalCachedMesh.lodMaterials.Add (lodMaterials);

						if (!interior) {
							GameObject subMeshLOD = subMeshGo.CreateChild ("LOD").gameObject;
							subMeshLOD.layer = subMeshGo.layer;
							Renderer lodRenderer = subMeshLOD.AddComponent <MeshRenderer> ();
							MeshFilter lodMf = subMeshLOD.AddComponent <MeshFilter> ();
							lodMf.sharedMesh = meshFilter.sharedMesh;
							lodRenderer.sharedMaterials = lodMaterials;

							LOD[] lods = new LOD [] {
								new LOD (0.6f,
									new Renderer [] { meshRenderer }),
								new LOD (0.05f,
									new Renderer [] { lodRenderer })
							};
							LODGroup lod = subMeshGo.AddComponent <LODGroup> ();
							lod.SetLODS (lods);
							lod.RecalculateBounds ();
							lodRenderers.Add (lodRenderer);
						}
					}
					normalCachedMesh.Finished = true;

					//if we have destroyed meshes, add them here
					if (createDestroyedMeshes) {
						for (int i = 0; i < resultDestroyed.MeshOutputs.Length; i++) {
							var newMesh = destroyedCombiner.CreateMeshObject (resultDestroyed.MeshOutputs [i], builder.MaterialLookup);
							//create a new child under the structure piece
							gNameBuilder.Clear ();
							gNameBuilder.Append (GetMeshPrefix (interior, true));
							gNameBuilder.Append (i.ToString ());
							gNameBuilder.Append (".");
							gNameBuilder.Append (staticLayer.Tag);
							gNameBuilder.Append (".");
							gNameBuilder.Append (staticLayer.Layer.ToString ());
							newMesh.Mesh.name = gNameBuilder.ToString ();

							GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild (newMesh.Mesh.name).gameObject;
							subMeshGo.layer = staticLayer.Layer;
							subMeshGo.tag = staticLayer.Tag;
							MeshFilter meshFilter = subMeshGo.AddComponent <MeshFilter> ();
							Renderer meshRenderer = subMeshGo.AddComponent <MeshRenderer> ();
							meshRenderer.enabled = true;
							//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
							meshFilter.sharedMesh = newMesh.Mesh;
							meshRenderer.sharedMaterials = newMesh.Materials;
							meshRenderer.gameObject.isStatic = true;
							meshes.Add (meshFilter);
							renderersDestroyed.Add (meshRenderer);
							//wait a while before we create the next one
							//longer if we're not immediate
							destroyedCachedMesh.meshes.Add (meshFilter.sharedMesh);
							Material[] materials = meshRenderer.sharedMaterials;
							Material[] lodMaterials = Structures.Get.LODMaterials (materials);
							destroyedCachedMesh.materials.Add (materials);
							destroyedCachedMesh.lodMaterials.Add (lodMaterials);

							if (!interior) {
								GameObject subMeshLOD = subMeshGo.CreateChild ("LOD").gameObject;
								subMeshLOD.layer = subMeshGo.layer;
								Renderer lodRenderer = subMeshLOD.AddComponent <MeshRenderer> ();
								MeshFilter lodMf = subMeshLOD.AddComponent <MeshFilter> ();
								lodMf.sharedMesh = meshFilter.sharedMesh;
								lodRenderer.sharedMaterials = lodMaterials;

								LOD[] lods = new LOD [] {
									new LOD (0.6f,
										new Renderer [] { meshRenderer }),
									new LOD (0.05f,
										new Renderer [] { lodRenderer })
								};
								LODGroup lod = subMeshGo.AddComponent <LODGroup> ();
								lod.SetLODS (lods);
								lod.RecalculateBounds ();
								lodRenderers.Add (lodRenderer);
							}
						}
						destroyedCachedMesh.Finished = true;
					}

					//if we have LOD meshes, add them here
					if (createLodMeshes) {
						for (int j = 0; j < lodResult.MeshOutputs.Length; j++) {
							var newMesh = lodCombiner.CreateMeshObject (lodResult.MeshOutputs [j], builder.MaterialLookup);
							//create a new child under the structure piece
							gNameBuilder.Clear ();
							gNameBuilder.Append (GetMeshPrefix (interior, false));
							gNameBuilder.Append (j.ToString ());
							gNameBuilder.Append (".");
							gNameBuilder.Append (staticLayer.Tag);
							gNameBuilder.Append (".");
							gNameBuilder.Append (staticLayer.Layer.ToString ());
							gNameBuilder.Append ("_LOD");
							newMesh.Mesh.name = gNameBuilder.ToString ();

							GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild (newMesh.Mesh.name).gameObject;
							subMeshGo.layer = staticLayer.Layer;
							subMeshGo.tag = staticLayer.Tag;
							MeshFilter meshFilter = subMeshGo.AddComponent <MeshFilter> ();
							Renderer meshRenderer = subMeshGo.AddComponent <MeshRenderer> ();
							meshRenderer.enabled = false;
							//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
							meshFilter.sharedMesh = newMesh.Mesh;
							meshRenderer.sharedMaterials = newMesh.Materials;
							meshRenderer.gameObject.isStatic = true;
							meshes.Add (meshFilter);
							lodRenderers.Add (meshRenderer);

							lodCachedMeshes.meshes.Add (meshFilter.sharedMesh);
							lodCachedMeshes.materials.Add (meshRenderer.sharedMaterials);
						}
						//set the meshes to finished
						lodCachedMeshes.Finished = true;

						if (createLodDestroyedMeshes) {
							for (int k = 0; k < lodResultDestroyed.MeshOutputs.Length; k++) {
								var newMesh = destroyedLodCombiner.CreateMeshObject (lodResultDestroyed.MeshOutputs [k], builder.MaterialLookup);
								//create a new child under the structure piece
								gNameBuilder.Clear ();
								gNameBuilder.Append (GetMeshPrefix (interior, true));
								gNameBuilder.Append (k.ToString ());
								gNameBuilder.Append (".");
								gNameBuilder.Append (staticLayer.Tag);
								gNameBuilder.Append (".");
								gNameBuilder.Append (staticLayer.Layer.ToString ());
								gNameBuilder.Append ("_LOD");
								newMesh.Mesh.name = gNameBuilder.ToString ();

								GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild (newMesh.Mesh.name).gameObject;
								subMeshGo.layer = staticLayer.Layer;
								subMeshGo.tag = staticLayer.Tag;
								MeshFilter meshFilter = subMeshGo.AddComponent <MeshFilter> ();
								Renderer meshRenderer = subMeshGo.AddComponent <MeshRenderer> ();
								meshRenderer.enabled = false;
								//meshRenderer.enabled = false;
								//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
								meshFilter.sharedMesh = newMesh.Mesh;
								meshRenderer.sharedMaterials = newMesh.Materials;
								meshRenderer.gameObject.isStatic = true;
								meshes.Add (meshFilter);
								lodRenderersDestroyed.Add (meshRenderer);

								lodDestroyedCachedMeshes.meshes.Add (meshFilter.sharedMesh);
								lodDestroyedCachedMeshes.materials.Add (meshRenderer.sharedMaterials);
							}
							//set the meshes to finished
							lodDestroyedCachedMeshes.Finished = true;
						}
					}
				}
			}
			#endregion

			#region clean up
			gHelperTransform.transform.parent = Structures.Get.transform;
			//we're officially done with childPieces so we'll clear it now
			//clear the combiner and get ready to build the next layer/tag combo
			//get rid of trash
			if (combiner != null) {
				combiner.ClearMeshes ();
				combiner.ClearMaterials ();
			}
			if (destroyedCombiner != null) {
				destroyedCombiner.ClearMeshes ();
				destroyedCombiner.ClearMaterials ();
			}

			if (result != null)
				result.Clear ();

			if (resultDestroyed != null)
				resultDestroyed.Clear ();

			if (createLodMeshes) {
				lodCombiner.ClearMeshes ();
				lodCombiner.ClearMaterials ();
				lodResult.Clear ();

				destroyedLodCombiner.ClearMeshes ();
				destroyedLodCombiner.ClearMaterials ();
				lodResultDestroyed.Clear ();
			}

			builder.MaterialLookup.Clear ();

			if (builder.State != Builder.BuilderState.Error) {
				//once that's done put the structure piece in the right place
				builder.StructurePiece.parent = builder.StructureBase.transform;

				if (builder.Mode == Builder.BuilderMode.Minor) {
					builder.StructurePiece.localPosition = builder.MinorParent.Position;
					builder.StructurePiece.localRotation = Quaternion.Euler (builder.MinorParent.Rotation);
				} else {
					builder.StructurePiece.ResetLocal ();
				}
			}
			#endregion
			yield break;
		}

		public static IEnumerator GenerateExteriorItems (
			Structure parentStructure,
			StructureTemplateGroup structureGroup,
			WIGroup group,
			Transform structurePiece)
		{
			if (parentStructure.State.ExteriorLoadedOnce) {
				//if we've already added everything to the group once
				//then all we have to do this time is load the group
				//and all that stuff will be in there again
				//Debug.LogError ("Already spawned exterior once, not adding items again");
				yield break;
			}

			//structurePiece.parent = parentStructure.StructureGroup.transform;
			structurePiece.ResetLocal ();
			//now that the structure is built add all the bits and pieces over time
			var nextTask = AddGenericDoorsToStructure (structureGroup.GenericDoorsPieces, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericWindowsToStructure (structureGroup.GenericWindowsPieces, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericDynamicToStructure (structureGroup.GenericDynamicPieces, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericWorldItemsToStructure (structureGroup.GenericWItemsPieces, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddUniqueDynamicToStructure (structureGroup.UniqueDynamic, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddFXPiecesToStructure (structureGroup.GenericLightsPieces, structurePiece, true, group);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddUniqueWorldItemsToStructure (structureGroup.UniqueWorlditems, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddCatItemsToStructure (structureGroup.CategoryWorldItems, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddTriggersToStructure (structureGroup.Triggers, structurePiece, true, group);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			WorldChunk chunk = parentStructure.worlditem.Group.GetParentChunk ();
			//Debug.Log ("Adding nodes to group " + structureGroup.ActionNodes.Count.ToString ());
			chunk.AddNodesToGroup (structureGroup.ActionNodes, group, structurePiece);
			yield break;
		}

		public static IEnumerator GenerateInteriorItems (
			Structure parentStructure,
			int interiorVariant,
			StructureTemplateGroup structureGroup,
			WIGroup group,
			Transform structurePiece)
		{
			//structurePiece.parent = group.transform;
			structurePiece.ResetLocal ();
			//now that the structure is built add all the bits and pieces over time
			var nextTask = AddGenericDoorsToStructure (structureGroup.GenericDoorsPieces, structurePiece, false, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericWindowsToStructure (structureGroup.GenericWindowsPieces, structurePiece, false, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericDynamicToStructure (structureGroup.GenericDynamicPieces, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddGenericWorldItemsToStructure (structureGroup.GenericWItemsPieces, structurePiece, false, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddUniqueDynamicToStructure (structureGroup.UniqueDynamic, structurePiece, true, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddFXPiecesToStructure (structureGroup.GenericLightsPieces, structurePiece, false, group);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddUniqueWorldItemsToStructure (structureGroup.UniqueWorlditems, structurePiece, false, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddCatItemsToStructure (structureGroup.CategoryWorldItems, structurePiece, false, group, parentStructure);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			nextTask = AddTriggersToStructure (structureGroup.Triggers, structurePiece, false, group);
			while (nextTask.MoveNext ()) {
				yield return nextTask.Current;
			}

			List <ActionNodeState> interiorActionNodes = null;
			//Debug.Log ("Adding " + structureGroup.ActionNodes.Count.ToString ( ) + " interior action nodes for variant " + interiorVariant.ToString ());
			WorldChunk chunk = parentStructure.worlditem.Group.GetParentChunk ();
			chunk.AddNodesToGroup (structureGroup.ActionNodes, group, structurePiece);
			yield break;
		}

		public static IEnumerator GenerateMinorItems (
			StructureTemplateGroup structureGroup,
			Transform structurePiece,
			WIGroup minorGroup)
		{
			yield return gCRunner.StartCoroutine (AddGenericWorldItemsToStructure (structureGroup.GenericWItemsPieces, minorGroup.transform, true, minorGroup, null));
			yield return gCRunner.StartCoroutine (AddUniqueWorldItemsToStructure (structureGroup.UniqueWorlditems, minorGroup.transform, true, minorGroup, null));
			yield break;
		}

		public static List <Material> gMaterialsList = new List <Material> ();

		public static IEnumerator SendChildPieceToMeshCombiner (
			StructureLayer staticLayer,
			MeshCombiner combiner,
			MeshCombiner lodCombiner,
			MeshCombiner destroyedCombiner,
			MeshCombiner destroyedLodCombiner,
			Dictionary <int,Material> materialLookup,
			Transform structurePiece,
			Builder builder)
		{
			ChildPiece childPiece = ChildPiece.Empty;
			StructurePackPrefab prefab = null;
			bool createLodMesh = false;
			bool destroyed = staticLayer.DestroyedBehavior != StructureDestroyedBehavior.None;
			if (staticLayer.InstancePieces != null) {
				//send the child pieces to the mesh combiner and wait for it to finish
				if (Structures.Get.PackStaticPrefab (staticLayer.PackName, staticLayer.PrefabName, out prefab)) {
					MeshCombiner.BufferedMesh bufferedMesh = prefab.BufferedMesh;
					MeshCombiner.BufferedMesh bufferedLodMesh = prefab.BufferedLodMesh;
					if (bufferedLodMesh != null && lodCombiner != null) {
						createLodMesh = true;
					}
					MeshRenderer pmr = prefab.MRenderer;
					MeshFilter pmf = prefab.MFilter;
					gMaterialsList.Clear ();
					gMaterialsList.AddRange (pmr.sharedMaterials);
					//copy the mesh data
					//swap in substitutions
					if (staticLayer.Substitutions != null && staticLayer.Substitutions.Count > 0) {
						string newMaterialName = string.Empty;
						for (int j = 0; j < gMaterialsList.Count; j++) {
							if (staticLayer.Substitutions.TryGetValue (gMaterialsList [j].name, out newMaterialName)) {
								Material sharedMaterial = null;
								if (Structures.Get.SharedMaterial (newMaterialName, out sharedMaterial)) {
									gMaterialsList [j] = sharedMaterial;
								}
							}
						}
					}
					if (staticLayer.AdditionalMaterials != null && staticLayer.AdditionalMaterials.Count > 0) {
						for (int i = 0; i < staticLayer.AdditionalMaterials.Count; i++) {
							Material sharedMaterial = null;
							if (Structures.Get.SharedMaterial (staticLayer.AdditionalMaterials [i], out sharedMaterial)) {
								gMaterialsList.Add (sharedMaterial);
							}
						}
					}
					if (staticLayer.EnableSnow) {
						//TODO this isn't working in-game find out why
						gMaterialsList.Add (Mats.Get.SnowOverlayMaterial);
					}
					//add the materials to the material lookup
					int[] matHashes = new int [gMaterialsList.Count];
					for (int i = 0; i < gMaterialsList.Count; i++) {
						int matHash = 0;
						try {
							matHash = Hydrogen.Material.GetDataHashCode (gMaterialsList [i]);
						} catch (Exception e) {
							Debug.Log ("Exception when getting mat hash for " + staticLayer.PackName + ", " + staticLayer.PrefabName + ": " + e.ToString ());
						}
						matHashes [i] = matHash;
						if (!materialLookup.ContainsKey (matHash)) {
							materialLookup.Add (matHash, gMaterialsList [i]);
						}
					}
					gMaterialsList.Clear ();
					if ((builder.Priority == StructureLoadPriority.Adjascent || builder.Priority == StructureLoadPriority.Distant) && Player.Local.HasSpawned) {
						yield return null;
					}
					//take a breather
					//then add the child pieces to the mesh combiner
					if (staticLayer.InstancePieces != null) {
						for (int i = 0; i < staticLayer.InstancePieces.Count; i++) {
							childPiece = staticLayer.InstancePieces [i];
							//use the helper to create a world matrix
							gHelperTransform.transform.parent = structurePiece;
							gHelperTransform.localPosition = childPiece.Position;
							gHelperTransform.localRotation = Quaternion.Euler (childPiece.Rotation);
							gHelperTransform.localScale = childPiece.Scale;
							//create a standard mesh input
							MeshCombiner.MeshInput meshInput = new MeshCombiner.MeshInput ();
							meshInput.Mesh = bufferedMesh;
							//copy the prefab's mesh data into the shared mesh
							meshInput.ScaleInverted = false; //TODO check if this is true
							meshInput.WorldMatrix = gHelperTransform.localToWorldMatrix;
							meshInput.Materials = matHashes;
							//add the mesh!
							if (!destroyed) {
								combiner.AddMesh (meshInput);
							} else {
								destroyedCombiner.AddMesh (meshInput);
							}
							//create an LOD mesh input if applicable
							if (createLodMesh) {
								MeshCombiner.MeshInput lodMeshInput = new MeshCombiner.MeshInput ();
								lodMeshInput.Mesh = bufferedLodMesh;
								//copy the prefab's mesh data into the shared mesh
								lodMeshInput.ScaleInverted = false; //TODO check if this is true
								lodMeshInput.WorldMatrix = gHelperTransform.localToWorldMatrix;
								lodMeshInput.Materials = matHashes;
								//add the mesh!
								if (!destroyed) {
									lodCombiner.AddMesh (lodMeshInput);
								} else {
									destroyedLodCombiner.AddMesh (lodMeshInput);
								}
							}
						}
					}
					builder.ChildPiecesSent = true;
					yield break;
				}
			} else {
				Debug.LogError ("Didn't find prefab " + staticLayer.PackName + ", " + staticLayer.PrefabName);
			}
			yield break;
		}

		public static IEnumerator AddGenericWorldItemsToStructure (
			List <ChildPiece> addChildPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			if (addChildPieces.Count > 0) {
				for (int i = 0; i < addChildPieces.Count; i++) {
					ChildPiece childPiece = addChildPieces [i];
					WorldItem worlditem = null;
					WorldItems.CloneWorldItem (childPiece.PackName, childPiece.ChildName, childPiece.Transform, false, group, out worlditem);
					worlditem.Initialize ();
					if (exterior && Player.Local.HasSpawned && parentStructure != null && !parentStructure.worlditem.Is (WIActiveState.Active)) {
						yield return null;
					}
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericDoorsToStructure (
			List <ChildPiece> addChildPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			for (int i = 0; i < addChildPieces.Count; i++) {
				ChildPiece childPiece = addChildPieces [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate (dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = childPiece.ChildName;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition = childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation = Quaternion.identity;
					instantiatedPrefab.transform.Rotate (childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab> ();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					Door door = null;
					if (worlditem.gameObject.HasComponent <Door> (out door)) {
						door.State.OuterEntrance = exterior;
						door.IsGeneric = true;//this will help it set up its locks correctly the first time
					}
					worlditem.Initialize ();
				}
			}
			//}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericWindowsToStructure (
			List <ChildPiece> addChildPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			for (int i = 0; i < addChildPieces.Count; i++) {
				ChildPiece childPiece = addChildPieces [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate (dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = childPiece.ChildName;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition	= childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation	= Quaternion.identity;
					instantiatedPrefab.transform.Rotate (childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab> ();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					bool addToLOD = (!worlditem.CanEnterInventory && !worlditem.CanBeCarried);
					worlditem.Group = group;
					Window window = null;
					if (worlditem.gameObject.HasComponent <Window> (out window)) {
						window.State.OuterEntrance = exterior;
						window.IsGeneric = true;//this will help it set up its locks correctly the first time
					}
					worlditem.Initialize ();
				}
			}
			//}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericDynamicToStructure (
			List <ChildPiece> addChildPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			for (int i = 0; i < addChildPieces.Count; i++) {
				ChildPiece childPiece = addChildPieces [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate (dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = dynamicPrefab.name;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition = childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation = Quaternion.identity;
					instantiatedPrefab.transform.Rotate (childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab> ();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					worlditem.Initialize ();
					if (exterior && Player.Local.HasSpawned && !parentStructure.worlditem.Is (WIActiveState.Active)) {
						yield return null;
					}
				}
			}
			//}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddFiresToStructure (
			List <ChildPiece> addChildPieces,
			Transform parentTransform)
		{
			for (int i = 0; i < addChildPieces.Count; i++) {
				ChildPiece piece = addChildPieces [i];
				FXManager.Get.SpawnFire (piece.ChildName, parentTransform, piece.Position, piece.Rotation, piece.Scale.x, false);
			}
			//}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddFXPiecesToStructure (
			List <LightPiece> lightPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			for (int i = 0; i < lightPieces.Count; i++) {
				Light light = StructureTemplate.LightFromLightPiece (lightPieces [i], parentTransform);
			}
			//}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddUniqueDynamicToStructure (
			List <StackItem> dynamicPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			DynamicPrefab dynamicPrefab = null;
			WorldItem newWorldItem = null;
			for (int i = 0; i < dynamicPieces.Count; i++) {
				StackItem piece = dynamicPieces [i];
				if (Structures.Get.PackDynamicPrefab (piece.PackName, piece.PrefabName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate (dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = dynamicPrefab.name;
					instantiatedPrefab.transform.parent = parentTransform;
					//we can do this here because the piece isn't cleared by adding set state
					piece.Transform.ApplyTo (instantiatedPrefab.transform);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab> ();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					worlditem.ReceiveState (ref piece);
					worlditem.Initialize ();
				} else {
					//Debug.Log ("Couldn't get dynamic prefab");
				}
				if (exterior && Player.Local.HasSpawned && !parentStructure.worlditem.Is (WIActiveState.Active)) {
					yield return null;
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddUniqueWorldItemsToStructure (
			List <StackItem> wiPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			for (int i = 0; i < wiPieces.Count; i++) {
				StackItem piece = wiPieces [i];
				WorldItem newWorldItem = null;
				if (WorldItems.CloneFromStackItem (piece, group, out newWorldItem)) {
					newWorldItem.transform.parent = parentTransform;
					//TODO this shouldn't be necessary any more, verify that this is needed
					newWorldItem.Props.Local.Transform.ApplyTo (newWorldItem.transform);
					newWorldItem.Initialize ();
				} else {
					//Debug.Log ("Couldn't spawn " + piece.PackName + " / " + piece.PrefabName + " for some goddamn reason");
				}
				if (exterior && Player.Local.HasSpawned && parentStructure != null && !parentStructure.worlditem.Is (WIActiveState.Active)) {
					yield return null;
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddTriggersToStructure (
			SDictionary <string, KeyValuePair <string,string>> triggers,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			WorldChunk chunk = group.GetParentChunk ();
			var enumerator = triggers.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				//foreach (KeyValuePair <string, KeyValuePair <string,string>> triggerStatePair in triggers) {
				//this will add it to the chunk
				//at which point it will be managed by the chunk and not the structure
				chunk.AddTrigger (enumerator.Current, parentTransform, true);
				if (exterior && Player.Local.HasSpawned) {
					yield return null;
				}
			}

			yield break;
		}

		public static IEnumerator AddCatItemsToStructure (
			List <WICatItem> wiCatItems,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			int parentHashCode = parentStructure.worlditem.GetHashCode () + Profile.Get.CurrentGame.Seed;
			for (int i = 0; i < wiCatItems.Count; i++) {
				WICatItem catItem = wiCatItems [i];
				//if (UnityEngine.Random.value <= catItem.DropoutProbability) {
				//skip this one, move on to the next
				//continue;
				//}
				WorldItem newWorldItem = null;
				int spawnCode = catItem.SpawnCode;
				int spawnIndex = catItem.SpawnIndex;
				if (spawnCode < 0) {
					spawnCode = parentHashCode;
				}
				if (spawnIndex < 0) {
					spawnIndex = i + parentHashCode;
				}
				//for now assume a one-time spawn
				//we'll do the multiple spawn thing later
				catItem.Flags.Union (parentStructure.State.StructureFlags);
				WICategory category = null;
				if (WorldItems.CloneRandomFromCategory (catItem.WICategoryName, group, catItem.Transform, catItem.Flags, spawnCode, spawnIndex, out newWorldItem)) {
					newWorldItem.transform.parent = parentTransform;
					FillStackContainer fillStackContainer = null;
					if (catItem.UseSettingsOnSpawnedContainers && newWorldItem.gameObject.HasComponent <FillStackContainer> (out fillStackContainer)) {
						catItem.ContainerSettings.Flags.Union (catItem.Flags);
						fillStackContainer.State.Flags.CopyFrom (catItem.ContainerSettings.Flags, false);//don't include size
						fillStackContainer.State.WICategoryName = catItem.ContainerSettings.WICategoryName;// = catItem.ContainerSettings;
					}
				} else {
					//Debug.Log ("Couldn't clone random from category " + catItem.WICategoryName + " with flags " + catItem.Flags.ToString ( ));
				}
				if (exterior && Player.Local.HasSpawned && !parentStructure.worlditem.Is (WIActiveState.Active)) {
					yield return null;
				}
			}
			//TODO split this up
			yield break;
		}

		public static float PriorityToSeconds (StructureLoadPriority priority)
		{	//TODO make these global
			switch (priority) {
			case StructureLoadPriority.SpawnPoint:
				return 0.05f;
			case StructureLoadPriority.Immediate:
			default:
				return 0.15f;
			case StructureLoadPriority.Adjascent:
				return 0.25f;
			case StructureLoadPriority.Distant:
				return 0.5f;
			}
			return 0f;
		}

		public static void ReclaimColliders (Builder builder, Structure structure)
		{
			List <BoxCollider> collidersToReclaim = null;
			if (builder.Mode == BuilderMode.Exterior) {
				collidersToReclaim = structure.ExteriorBoxColliders;
			} else {
				collidersToReclaim = structure.InteriorBoxColliders;
			}
			Structures.ReclaimBoxColliders (collidersToReclaim);
			collidersToReclaim.Clear ();
		}

		public static List <Renderer> renderers;
		public static List <Renderer> lodRenderers;
		public static List <Renderer> renderersDestroyed;
		public static List <Renderer> lodRenderersDestroyed;
		public static List <MeshFilter> structureMeshes;

		public static IEnumerator UnloadStructureMeshes (Builder builder, Structure structure)
		{
			//Debug.Log("Unloading structure meshes for " + structure.name);
			if (builder.Mode == Builder.BuilderMode.Exterior) {
				structureMeshes = structure.ExteriorMeshes;
				renderers = structure.ExteriorRenderers;
			} else {
				structureMeshes = structure.InteriorMeshes;
				renderers = structure.InteriorRenderers;
			}

			for (int i = 0; i < renderers.Count; i++) {
				if (renderers [i] != null) {
					GameObject.Destroy (renderers [i].gameObject);
				}
			}
			renderers.Clear ();
			yield return null;

			if (lodRenderers != null) {
				for (int i = 0; i < lodRenderers.Count; i++) {
					if (renderers [i] != null) {
						GameObject.Destroy (lodRenderers [i].gameObject);
					}
				}
				lodRenderers.Clear ();
				yield return null;
			}


			if (renderersDestroyed != null) {
				for (int i = 0; i < renderersDestroyed.Count; i++) {
					if (renderers [i] != null) {
						GameObject.Destroy (renderersDestroyed [i].gameObject);
					}
				}
				renderersDestroyed.Clear ();
				yield return null;
			}

			if (lodRenderersDestroyed != null) {
				for (int i = 0; i < lodRenderersDestroyed.Count; i++) {
					if (renderers [i] != null) {
						GameObject.Destroy (lodRenderersDestroyed [i].gameObject);
					}
				}
				lodRenderersDestroyed.Clear ();
				yield return null;
			}

			yield break;
		}

		public static IEnumerator UnloadStructureMeshes (List <MeshFilter> structureMeshes)
		{
			//the goal here is to find every mesh associated with the struture
			//then destroy that mesh - and actually destroy the thing so it's not in memory!
			for (int i = 0; i < structureMeshes.Count; i++) {
				if (structureMeshes [i] != null) {
					GameObject.Destroy (structureMeshes [i].sharedMesh);
					GameObject.Destroy (structureMeshes [i].gameObject);
				}
			}
			structureMeshes.Clear ();
			yield break;
		}

		public static string GetChildName (bool interior, bool destroyed)
		{
			return GetChildName (interior, destroyed, -1);
		}

		public static string GetChildName (bool interior, bool destroyed, int minorStructureNum)
		{
			if (gChildNameBuilder == null) {
				gChildNameBuilder = new System.Text.StringBuilder ();
			}

			gChildNameBuilder.Clear ();
			gChildNameBuilder.Append ("__");
			if (minorStructureNum >= 0) {
				gChildNameBuilder.Append ("MINOR_");
				gChildNameBuilder.Append (minorStructureNum.ToString ());
			} else if (interior) {
				gChildNameBuilder.Append ("INTERIOR_");
			} else {
				gChildNameBuilder.Append ("EXTERIOR_");
			}
			if (destroyed) {
				gChildNameBuilder.Append ("_DESTROYED_");
			}
			return gChildNameBuilder.ToString ();
		}

		public static string GetMeshPrefix (bool interior, bool destroyed)
		{
			if (interior) {
				return gIntMeshPrefix;
			} else {
				if (destroyed) {
					return gDstMeshPrefix;
				}
			}
			return gExtMeshPrefix;
		}

		public static string GetMeshName (string templateName, string meshPrefix)
		{
			return meshPrefix;
		}

		protected static string gIntMeshPrefix = "__StrMesh-INT";
		protected static string gExtMeshPrefix = "__StrMesh-EXT";
		protected static string gDstMeshPrefix = "__StrMesh-DST";
		protected static System.Text.StringBuilder gChildNameBuilder;
		protected static System.Text.StringBuilder gNameBuilder;

		public static string GetTemplateName (string rootName)
		{
			string[] templateName = rootName.Split ('-');
			return templateName [0];
		}

		protected static Transform gHelperTransform;
		protected static StructureBuilder gCRunner;

		public sealed class MeshCombinerResult
		{
			public int Hash;
			public Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] MeshOutputs;

			public void Clear ()
			{
				if (MeshOutputs != null) {
					for (int i = 0; i < MeshOutputs.Length; i++) {
						if (MeshOutputs [i] != null) {
							MeshOutputs [i].Clear ();
						}
					}
					Array.Clear (MeshOutputs, 0, MeshOutputs.Length);
				}
			}

			public void MeshCombinerCallback (int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
			{
				Hash = hash;
				MeshOutputs = meshOutputs;
			}
		}
	}
}
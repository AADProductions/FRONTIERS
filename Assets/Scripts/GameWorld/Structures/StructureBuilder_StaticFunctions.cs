#pragma warning disable 0219
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers;
using Frontiers.World.Gameplay;
using Hydrogen.Threading.Jobs;

namespace Frontiers.World
{
	public partial class StructureBuilder : Builder
	{
		public static IEnumerator InstanceMeshes(
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
			//Debug.Log ("Instancing meshes");
			renderers.Clear();
			if (lodRenderers != null) {
				lodRenderers.Clear();
			}

			Transform structurePiece = structureBase.gameObject.CreateChild(childName);
			structurePiece.parent = null;
			if (mode == Builder.BuilderMode.Minor) {
				//minor structures have to set their own offsets
				structurePiece.position = minorParent.Position;
				structurePiece.rotation = Quaternion.Euler(minorParent.Rotation);
			} else {
				structurePiece.ResetLocal();
			}
			structurePiece.gameObject.layer = Globals.LayerNumStructureCustomCollider;//TODO this may be unnecessary
			for (int i = 0; i < structureGroup.StaticStructureLayers.Count; i++) {
				StructureTemplate.InstantiateStructureLayer(structureGroup.StaticStructureLayers[i], structurePiece);
			}

			structurePiece.parent = structureBase;
			if (mode == Builder.BuilderMode.Minor) {
				structurePiece.localPosition = minorParent.Position;
				structurePiece.localRotation = Quaternion.Euler(minorParent.Rotation);
			} else {
				structurePiece.ResetLocal();
			}
			yield break;
		}

		public static IEnumerator GenerateColliders(
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
				builder.StructurePiece = builder.MinorParent.StructureOwner.gameObject.FindOrCreateChild(childName + "_MINOR_COL");
				builder.StructurePiece.localPosition = builder.MinorParent.Position;
				builder.StructurePiece.localEulerAngles = builder.MinorParent.Rotation;
			} else {
				builder.StructurePiece = builder.StructureBase.gameObject.FindOrCreateChild(childName + "_COL");
			}

			StructureLayer colliderLayer = null;
			ChildPiece[] childPieces = null;
			ChildPiece childPiece = ChildPiece.Empty;
			BoxCollider boxCollider = null;
			MeshCollider meshCollider = null;
			Transform customColliderTr = null;
			Mesh sharedMesh = null;
			List <BoxCollider> bcList = null;
			List <MeshCollider> mcList = null;

			for (int i = 0; i < structureColliders.Count; i++) {

				colliderLayer = structureColliders[i];

				if (colliderLayer.DestroyedBehavior == StructureDestroyedBehavior.Destroy) {
					bcList = boxCollidersDestroyed;
					mcList = meshCollidersDestroyed;
				} else {
					bcList = boxColliders;
					mcList = meshColliders;
				}

				childPieces = StructureTemplate.ExtractChildPiecesFromLayer(colliderLayer.Instances);
				//TODO send the child pieces to the mesh combiner and wait for it to finish?
				switch (colliderLayer.PackName) {
					case "MeshCollider":
						if (Structures.Get.ColliderMesh(colliderLayer.PrefabName, out sharedMesh)) {
							for (int j = 0; j < childPieces.Length; j++) {
								childPiece = childPieces[j];
								//get the collider from the child piece pack/prefab name
								meshCollider = Structures.Get.MeshColliderFromPool();
								meshCollider.name = colliderLayer.PrefabName;
								customColliderTr = meshCollider.transform;
								customColliderTr.parent = builder.StructurePiece;

								meshCollider.tag = colliderLayer.Tag;
								meshCollider.gameObject.layer = Globals.LayerNumStructureTerrain;

								customColliderTr.localPosition = childPiece.Position;
								customColliderTr.localRotation = Quaternion.Euler(childPiece.Rotation);
								customColliderTr.localScale = childPiece.Scale;

								meshCollider.sharedMesh = sharedMesh;

								if (meshColliders != null) {
									meshColliders.Add(meshCollider);
								}
							}
						} else {
							Debug.Log("Couldn't get mesh collider " + colliderLayer.PrefabName);
						}
						break;

					case "BoxCollider":
						for (int j = 0; j < childPieces.Length; j++) {
							childPiece = childPieces[j];
							boxCollider = Structures.Get.BoxColliderFromPool();
							boxCollider.name = colliderLayer.PrefabName;
							customColliderTr = boxCollider.transform;
							customColliderTr.parent = builder.StructurePiece;

							boxCollider.tag = colliderLayer.Tag;
							boxCollider.gameObject.layer = Globals.LayerNumStructureTerrain;

							customColliderTr.localPosition = childPiece.Position;
							customColliderTr.localRotation = Quaternion.Euler(childPiece.Rotation);
							customColliderTr.localScale = childPiece.Scale;

							if (boxColliders != null) {
								boxColliders.Add(boxCollider);
							}
						}
						break;

					default:
						Debug.Log("Couldn't identify collider pack name " + childPiece.PackName);
						break;
				}
				Array.Clear(childPieces, 0, childPieces.Length);
				//yield return null;
				StructureTerrainLayer terrainLayer = builder.StructurePiece.gameObject.GetOrAdd <StructureTerrainLayer>();
				//terrainLayer.rb.detectCollisions = false;
				layers.SafeAdd(terrainLayer);
				//builder.StructurePiece.gameObject.SetActive (true);
				//terrainLayer.rb.detectCollisions = true;
				childPieces = null;

				if (builder.Priority != StructureLoadPriority.SpawnPoint) {
					//if we don't have to get inside this structure >rightaway<
					//smooth out the wait a bit
					yield return WorldClock.WaitForRTSeconds(0.05f);
				}
			}
			//}
			yield break;
		}

		public static IEnumerator GenerateMeshes(
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
			Builder builder)
		{
			if (structureGroup.StaticStructureLayers.Count <= 0) {
				yield break;
			}

			if (gHelperTransform == null) {
				gHelperTransform = Structures.Get.gameObject.CreateChild("Structure Helper Transform");
			}

			//only generate LOD meshes if we have a combiner and a renderer for the task
			bool createLodMeshes = (lodCombiner != null && lodRenderers != null);
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
			MeshCombinerResult result = new MeshCombinerResult();
			MeshCombinerResult resultDestroyed = new MeshCombinerResult();
			MeshCombinerResult lodResult = null;
			MeshCombinerResult lodResultDestroyed = null;

			renderers.Clear();
			if (createLodMeshes) {
				lodResult = new MeshCombinerResult();
				lodRenderers.Clear();
				lodResultDestroyed = new MeshCombinerResult();
				lodRenderersDestroyed.Clear();
			}
			builder.MaterialLookup.Clear();

			//create the transform that the newly built exterior will exist under
			//it may exist from a previous build so use find or create
			builder.StructurePiece = builder.StructureBase.gameObject.FindOrCreateChild(childName);
			builder.StructurePiece.parent = null;
			if (builder.Mode == Builder.BuilderMode.Minor) {
				//minor structures have to set their own offsets
				builder.StructurePiece.position = builder.MinorParent.Position;
				builder.StructurePiece.rotation = Quaternion.Euler(builder.MinorParent.Rotation);
				//the minor structure container aids in the disposal of minor structure components
				MinorStructureContainer msc = builder.StructurePiece.gameObject.GetOrAdd <MinorStructureContainer>();
				msc.Parent = builder.MinorParent;
				builder.MinorParent.Container = msc;
			} else {
				builder.StructurePiece.ResetLocal();
			}
			builder.StructurePiece.gameObject.layer = Globals.LayerNumStructureTerrain;//TODO this may be unnecessary
			//we use this to generate our LOD at the end

			//split the structure group's static pieces into ChildPieces
			//they should be arranged by layer and tag
			//each set will be a different mesh combiner job
			//int vertexCount = 0;
			StructureLayer staticLayer = null;
			for (int i = 0; i < structureGroup.StaticStructureLayers.Count; i++) {
				staticLayer = structureGroup.StaticStructureLayers[i];
				//vertexCount += staticLayer.NumVertices;
				//if this return true then it means we actually sent stuff
				bool sendChildPieces = false;

				try {
					sendChildPieces = SendChildPieceToMeshCombiner(staticLayer, combiner, lodCombiner, destroyedCombiner, destroyedLodCombiner, builder.MaterialLookup, builder.StructurePiece);
				} catch (Exception e) {
					Debug.LogError(e);
					sendChildPieces = false;
				}

				if (sendChildPieces) {
					//wait for it to call back
					yield return null;
				} else {
					Debug.Log("DIDN'T SEND CHILD PIECES TO MESH COMBINER");
					//yiked clear everything out
					builder.State = BuilderState.Error;
					combiner.ClearMeshes();
					combiner.ClearMaterials();
					if (lodCombiner != null) {
						lodCombiner.ClearMeshes();
						lodCombiner.ClearMaterials();
					}
					if (destroyedCombiner != null) {
						destroyedCombiner.ClearMeshes();
						destroyedCombiner.ClearMaterials();
					}
					if (destroyedLodCombiner != null) {
						destroyedLodCombiner.ClearMeshes();
						destroyedLodCombiner.ClearMaterials();
					}
					yield break;
				}
			}

			builder.State = Builder.BuilderState.WaitingForMeshes;
			//combine the meshes!
			combiner.Combine(result.MeshCombinerCallback);
			//are we creating lod meshes
			if (createLodMeshes) {
				if (lodCombiner.MeshInputCount > 0) {
					lodCombiner.Combine(lodResult.MeshCombinerCallback);
					//only create lod destroyed meshes if we're creating lod meshes
					if (destroyedLodCombiner.MeshInputCount > 0) {
						destroyedLodCombiner.Combine(lodResultDestroyed.MeshCombinerCallback);
						createLodDestroyedMeshes = true;
					} else {
						lodResultDestroyed.Clear();
						lodDestroyedMeshesFinished = true;
						createLodDestroyedMeshes = false;
					}

				} else {
					//Debug.Log ("LOD COMBINER MESH INPUT WAS ZERO, NOT CREATING LODS");
					lodResult.Clear();
					createLodMeshes = false;
					lodMeshesFinished = true;
					lodDestroyedMeshesFinished = true;
				}
			}
			//do we have any destroyed meshes to create? if so build them
			if (destroyedCombiner.MeshInputCount > 0) {
				//Debug.Log ("Destroyed combiner mesh input count was greater than zero, calling destroyed combiner");
				destroyedCombiner.Combine(resultDestroyed.MeshCombinerCallback);
				createDestroyedMeshes = true;
			} else {
				meshesDestroyedFinished = true;
				createDestroyedMeshes = false;
			}

			while (builder.State == Builder.BuilderState.WaitingForMeshes) {
				if (!meshesFinished) {
					combiner.Check();
					if (result.MeshOutputs != null) {
						meshesFinished = true;
					}
				}
				if (!meshesDestroyedFinished) {
					destroyedCombiner.Check();
					if (resultDestroyed.MeshOutputs != null) {
						meshesDestroyedFinished = true;
					}
				}

				if (!lodMeshesFinished) {
					lodCombiner.Check();
					if (lodResult.MeshOutputs != null) {
						lodMeshesFinished = true;
					}
				}
				if (!lodDestroyedMeshesFinished) {
					destroyedLodCombiner.Check();
					if (lodResultDestroyed.MeshOutputs != null) {
						lodDestroyedMeshesFinished = true;
					}
				}

				if (meshesFinished && lodMeshesFinished && meshesDestroyedFinished && lodDestroyedMeshesFinished) {
					//Debug.Log ("Meshes finished, moving on");
					builder.State = Builder.BuilderState.HandlingMeshes;
				}
				yield return null;//TODO add some sort of timeout?
			}
			//the mesh results should have been sent to our public props
			//take them and put them in the right place now
			if (builder.StructurePiece == null) {
				builder.State = Builder.BuilderState.Error;
				//something happened while we were away
				//cancel operation
				Debug.Log("STRUCTURE BUILDER: STRUCTURE PIECE WAS NULL, NOT BUILDING!!! " + builder.Mode.ToString());
			} else {
				// Make our meshes in Unity
				for (int j = 0; j < result.MeshOutputs.Length; j++) {
					var newMesh = combiner.CreateMeshObject(result.MeshOutputs[j], builder.MaterialLookup);
					//create a new child under the structure piece
					if (gNameBuilder == null) {
						gNameBuilder = new System.Text.StringBuilder();
					}
					gNameBuilder.Clear();
					gNameBuilder.Append(GetMeshPrefix(interior, false));
					gNameBuilder.Append(j.ToString());
					gNameBuilder.Append(".");
					gNameBuilder.Append(staticLayer.Tag);
					gNameBuilder.Append(".");
					gNameBuilder.Append(staticLayer.Layer.ToString());

					GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild(gNameBuilder.ToString()).gameObject;
					subMeshGo.layer = staticLayer.Layer;
					subMeshGo.tag = staticLayer.Tag;
					MeshFilter meshFilter = subMeshGo.GetOrAdd <MeshFilter>();
					Renderer meshRenderer = subMeshGo.GetOrAdd <MeshRenderer>();
					//meshRenderer.enabled = false;
					meshFilter.sharedMesh = newMesh.Mesh;
					meshRenderer.sharedMaterials = newMesh.Materials;
					meshRenderer.gameObject.isStatic = true;
					meshes.Add(meshFilter);
					renderers.Add(meshRenderer);
					//wait a while before we create the next one
					//longer if we're not immediate
				}
				//if we have destroyed meshes, add them here
				if (createDestroyedMeshes) {
					for (int i = 0; i < resultDestroyed.MeshOutputs.Length; i++) {
						var newMesh = destroyedCombiner.CreateMeshObject(resultDestroyed.MeshOutputs[i], builder.MaterialLookup);
						//create a new child under the structure piece
						gNameBuilder.Clear();
						gNameBuilder.Append(GetMeshPrefix(interior, true));
						gNameBuilder.Append(i.ToString());
						gNameBuilder.Append(".");
						gNameBuilder.Append(staticLayer.Tag);
						gNameBuilder.Append(".");
						gNameBuilder.Append(staticLayer.Layer.ToString());

						GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild(gNameBuilder.ToString()).gameObject;
						subMeshGo.layer = staticLayer.Layer;
						subMeshGo.tag = staticLayer.Tag;
						MeshFilter meshFilter = subMeshGo.GetOrAdd <MeshFilter>();
						Renderer meshRenderer = subMeshGo.GetOrAdd <MeshRenderer>();
						//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
						meshFilter.sharedMesh = newMesh.Mesh;
						meshRenderer.sharedMaterials = newMesh.Materials;
						meshRenderer.gameObject.isStatic = true;
						meshes.Add(meshFilter);
						renderersDestroyed.Add(meshRenderer);
						//wait a while before we create the next one
						//longer if we're not immediate
					}
				}

				//if we have LOD meshes, add them here
				if (createLodMeshes) {
					for (int j = 0; j < lodResult.MeshOutputs.Length; j++) {
						var newMesh = lodCombiner.CreateMeshObject(lodResult.MeshOutputs[j], builder.MaterialLookup);
						//create a new child under the structure piece
						gNameBuilder.Clear();
						gNameBuilder.Append(GetMeshPrefix(interior, false));
						gNameBuilder.Append(j.ToString());
						gNameBuilder.Append(".");
						gNameBuilder.Append(staticLayer.Tag);
						gNameBuilder.Append(".");
						gNameBuilder.Append(staticLayer.Layer.ToString());
						gNameBuilder.Append("_LOD");

						GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild(gNameBuilder.ToString()).gameObject;
						subMeshGo.layer = staticLayer.Layer;
						subMeshGo.tag = staticLayer.Tag;
						MeshFilter meshFilter = subMeshGo.GetOrAdd <MeshFilter>();
						Renderer meshRenderer = subMeshGo.GetOrAdd <MeshRenderer>();
						//meshRenderer.enabled = false;
						//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
						meshFilter.sharedMesh = newMesh.Mesh;
						meshRenderer.sharedMaterials = newMesh.Materials;
						meshRenderer.gameObject.isStatic = true;
						meshes.Add(meshFilter);
						meshRenderer.enabled = false;
						lodRenderers.Add(meshRenderer);
					}

					if (createLodDestroyedMeshes) {
						for (int k = 0; k < lodResultDestroyed.MeshOutputs.Length; k++) {
							var newDestroyedMesh = destroyedLodCombiner.CreateMeshObject(lodResultDestroyed.MeshOutputs[k], builder.MaterialLookup);
							//create a new child under the structure piece
							gNameBuilder.Clear();
							gNameBuilder.Append(GetMeshPrefix(interior, true));
							gNameBuilder.Append(k.ToString());
							gNameBuilder.Append(".");
							gNameBuilder.Append(staticLayer.Tag);
							gNameBuilder.Append(".");
							gNameBuilder.Append(staticLayer.Layer.ToString());
							gNameBuilder.Append("_LOD");

							GameObject subMeshGo = builder.StructurePiece.gameObject.CreateChild(gNameBuilder.ToString()).gameObject;
							subMeshGo.layer = staticLayer.Layer;
							subMeshGo.tag = staticLayer.Tag;
							MeshFilter meshFilter = subMeshGo.GetOrAdd <MeshFilter>();
							Renderer meshRenderer = subMeshGo.GetOrAdd <MeshRenderer>();
							//meshRenderer.enabled = false;
							//MeshCollider meshCollider = subMeshGo.GetOrAdd <MeshCollider> ();
							meshFilter.sharedMesh = newDestroyedMesh.Mesh;
							meshRenderer.sharedMaterials = newDestroyedMesh.Materials;
							meshRenderer.gameObject.isStatic = true;
							meshes.Add(meshFilter);
							lodRenderersDestroyed.Add(meshRenderer);
						}
					}
				}
			}
			gHelperTransform.transform.parent = Structures.Get.transform;
			//we're officially done with childPieces so we'll clear it now
			//clear the combiner and get ready to build the next layer/tag combo
			//get rid of trash
			combiner.ClearMeshes();
			combiner.ClearMaterials();
			destroyedCombiner.ClearMeshes();
			destroyedCombiner.ClearMaterials();

			result.Clear();
			resultDestroyed.Clear();

			if (createLodMeshes) {
				lodCombiner.ClearMeshes();
				lodCombiner.ClearMaterials();
				lodResult.Clear();

				destroyedLodCombiner.ClearMeshes();
				destroyedLodCombiner.ClearMaterials();
				lodResultDestroyed.Clear();
			}

			builder.MaterialLookup.Clear();

			if (builder.State != Builder.BuilderState.Error) {
				//once that's done put the structure piece in the right place
				builder.StructurePiece.parent = builder.StructureBase.transform;

				if (builder.Mode == Builder.BuilderMode.Minor) {
					builder.StructurePiece.localPosition = builder.MinorParent.Position;
					builder.StructurePiece.localRotation = Quaternion.Euler(builder.MinorParent.Rotation);
				} else {
					builder.StructurePiece.ResetLocal();
				}
//				//add our mesh colliders//TODO make this more, erm, efficient
//				//TODO link renderer meshes to lighter collider meshes
//				for (int i = 0; i < renderers.Count; i++) {
//					Renderer renderer = renderers [i];
//					MeshFilter mf = renderer.GetComponent <MeshFilter> ();
//					MeshCollider col = renderer.gameObject.AddComponent <MeshCollider> ();
//					col.enabled = false;
//					col.sharedMesh = mf.sharedMesh;
//					generatedColliders.Add (col);
//				}
			}

			yield return null;//why am i doing this?
			yield break;
		}

		public static IEnumerator GenerateExteriorItems(
			Structure parentStructure,
			StructureTemplateGroup structureGroup,
			WIGroup group,
			Transform structurePiece)
		{
			if (parentStructure.State.ExteriorLoadedOnce) {
				//if we've already added everything to the group once
				//then all we have to do this time is load the group
				//and all that stuff will be in there again
				//Debug.Log ("Already spawned exterior once, not adding items again");
				yield break;
			}

			//structurePiece.parent = parentStructure.StructureGroup.transform;
			structurePiece.ResetLocal();
			//now that the structure is built add all the bits and pieces over time
			yield return group.StartCoroutine(AddGenericDoorsToStructure(structureGroup.GenericDoors, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddGenericWindowsToStructure(structureGroup.GenericWindows, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddGenericDynamicToStructure(structureGroup.GenericDynamic, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddGenericWorldItemsToStructure(structureGroup.GenericWItems, structurePiece, true, group));
			yield return group.StartCoroutine(AddUniqueDynamicToStructure(structureGroup.UniqueDynamic, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddFXPiecesToStructure(structureGroup.GenericLights, structurePiece, true, group));
			yield return group.StartCoroutine(AddUniqueWorldItemsToStructure(structureGroup.UniqueWorlditems, structurePiece, true, group));
			yield return group.StartCoroutine(AddCatItemsToStructure(structureGroup.CategoryWorldItems, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddTriggersToStructure(structureGroup.Triggers, structurePiece, true, group));
			WorldChunk chunk = parentStructure.worlditem.Group.GetParentChunk();
			//Debug.Log ("Adding nodes to group " + structureGroup.ActionNodes.Count.ToString ());
			chunk.AddNodesToGroup(structureGroup.ActionNodes, group, structurePiece);
			yield break;
		}

		public static IEnumerator GenerateInteriorItems(
			Structure parentStructure,
			int interiorVariant,
			StructureTemplateGroup structureGroup,
			WIGroup group,
			Transform structurePiece)
		{
			//structurePiece.parent = group.transform;
			structurePiece.ResetLocal();
			//now that the structure is built add all the bits and pieces over time
			yield return group.StartCoroutine(AddGenericDoorsToStructure(structureGroup.GenericDoors, structurePiece, false, group, parentStructure));
			yield return group.StartCoroutine(AddGenericWindowsToStructure(structureGroup.GenericWindows, structurePiece, false, group, parentStructure));
			yield return group.StartCoroutine(AddGenericDynamicToStructure(structureGroup.GenericDynamic, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddGenericWorldItemsToStructure(structureGroup.GenericWItems, structurePiece, false, group));
			yield return group.StartCoroutine(AddUniqueDynamicToStructure(structureGroup.UniqueDynamic, structurePiece, true, group, parentStructure));
			yield return group.StartCoroutine(AddFXPiecesToStructure(structureGroup.GenericLights, structurePiece, false, group));
			yield return group.StartCoroutine(AddUniqueWorldItemsToStructure(structureGroup.UniqueWorlditems, structurePiece, false, group));
			yield return group.StartCoroutine(AddCatItemsToStructure(structureGroup.CategoryWorldItems, structurePiece, false, group, parentStructure));
			yield return group.StartCoroutine(AddTriggersToStructure(structureGroup.Triggers, structurePiece, false, group));
			List <ActionNodeState> interiorActionNodes = null;
			//Debug.Log ("Adding " + structureGroup.ActionNodes.Count.ToString ( ) + " interior action nodes for variant " + interiorVariant.ToString ());
			WorldChunk chunk = parentStructure.worlditem.Group.GetParentChunk();
			chunk.AddNodesToGroup(structureGroup.ActionNodes, group, structurePiece);
			yield break;
		}

		public static IEnumerator GenerateMinorItems(
			StructureTemplateGroup structureGroup,
			Transform structurePiece,
			WIGroup minorGroup)
		{
			yield return gCRunner.StartCoroutine(AddGenericWorldItemsToStructure(structureGroup.GenericWItems, minorGroup.transform, true, minorGroup));
			yield return gCRunner.StartCoroutine(AddUniqueWorldItemsToStructure(structureGroup.UniqueWorlditems, minorGroup.transform, true, minorGroup));
			yield break;
		}

		public static bool SendChildPieceToMeshCombiner(
			StructureLayer staticLayer,
			MeshCombiner combiner,
			MeshCombiner lodCombiner,
			MeshCombiner destroyedCombiner,
			MeshCombiner destroyedLodCombiner,
			Dictionary <int,Material> materialLookup,
			Transform structurePiece)
		{
			ChildPiece childPiece = ChildPiece.Empty;
			StructurePackPrefab prefab = null;
			bool createLodMesh = false;
			bool destroyed = staticLayer.DestroyedBehavior != StructureDestroyedBehavior.None;
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(staticLayer.Instances);
			//send the child pieces to the mesh combiner and wait for it to finish
			if (Structures.Get.PackStaticPrefab(staticLayer.PackName, staticLayer.PrefabName, out prefab)) {
				MeshCombiner.BufferedMesh bufferedMesh = prefab.BufferedMesh;
				MeshCombiner.BufferedMesh bufferedLodMesh = prefab.BufferedLodMesh;
				if (bufferedLodMesh != null && lodCombiner != null) {
					createLodMesh = true;
				}
				MeshRenderer pmr = prefab.MRenderer;
				MeshFilter pmf = prefab.MFilter;
				//TODO get rid of this allocation
				List <Material> materialsList = new List<Material>(pmr.sharedMaterials);
				//Material[] materialsArray = null;//pmr.sharedMaterials;//new Material [] { Mats.Get.DefaultDiffuseMaterial };
				//copy the mesh data
				//swap in substitutions
				if (staticLayer.Substitutions != null && staticLayer.Substitutions.Count > 0) {
					string newMaterialName = string.Empty;
					for (int j = 0; j < materialsList.Count; j++) {
						if (staticLayer.Substitutions.TryGetValue(materialsList[j].name, out newMaterialName)) {
							Material sharedMaterial = null;
							if (Structures.Get.SharedMaterial(newMaterialName, out sharedMaterial)) {
								materialsList[j] = sharedMaterial;
							}
						}
					}
				}
				if (staticLayer.AdditionalMaterials != null && staticLayer.AdditionalMaterials.Count > 0) {
					for (int i = 0; i < staticLayer.AdditionalMaterials.Count; i++) {
						Material sharedMaterial = null;
						if (Structures.Get.SharedMaterial(staticLayer.AdditionalMaterials[i], out sharedMaterial)) {
							materialsList.Add(sharedMaterial);
						}
					}
				}
				if (staticLayer.EnableSnow) {
					//TODO this isn't working in-game find out why
					materialsList.Add(Mats.Get.SnowOverlayMaterial);
				}
//				//add the materials to the material lookup
				int[] matHashes = new int [materialsList.Count];
				for (int i = 0; i < materialsList.Count; i++) {
					int matHash = 0;
					try {
						matHash = Hydrogen.Material.GetDataHashCode(materialsList[i]);
					} catch (Exception e) {
						Debug.Log("Exception when getting mat hash for " + staticLayer.PackName + ", " + staticLayer.PrefabName + ": " + e.ToString());
					}
					matHashes[i] = matHash;
					if (!materialLookup.ContainsKey(matHash)) {
						materialLookup.Add(matHash, materialsList[i]);
					}
				}
				materialsList.Clear();
				materialsList = null;
				//add the child pieces to the mesh combiner
				if (childPieces.Length > 0) {
					for (int i = 0; i < childPieces.Length; i++) {
						childPiece = childPieces[i];
						//use the helper to create a world matrix
						gHelperTransform.transform.parent = structurePiece;
						gHelperTransform.localPosition = childPiece.Position;
						gHelperTransform.localRotation = Quaternion.Euler(childPiece.Rotation);
						gHelperTransform.localScale = childPiece.Scale;
						//create a standard mesh input
						MeshCombiner.MeshInput meshInput = new MeshCombiner.MeshInput();
						meshInput.Mesh = bufferedMesh;
						//copy the prefab's mesh data into the shared mesh
						meshInput.ScaleInverted = false; //TODO check if this is true
						meshInput.WorldMatrix = gHelperTransform.localToWorldMatrix;
						meshInput.Materials = matHashes;
						//add the mesh!
						if (!destroyed) {
							combiner.AddMesh(meshInput);
						} else {
							destroyedCombiner.AddMesh(meshInput);
						}
						//create an LOD mesh input if applicable
						if (createLodMesh) {
							MeshCombiner.MeshInput lodMeshInput = new MeshCombiner.MeshInput();
							lodMeshInput.Mesh = bufferedLodMesh;
							//copy the prefab's mesh data into the shared mesh
							lodMeshInput.ScaleInverted = false; //TODO check if this is true
							lodMeshInput.WorldMatrix = gHelperTransform.localToWorldMatrix;
							lodMeshInput.Materials = matHashes;
							//add the mesh!
							if (!destroyed) {
								lodCombiner.AddMesh(lodMeshInput);
							} else {
								destroyedLodCombiner.AddMesh(lodMeshInput);
							}
						}
					}
				}
				Array.Clear(childPieces, 0, childPieces.Length);
				childPieces = null;
				return true;
			} else {
				Debug.Log("Didn't find prefab " + staticLayer.PackName + ", " + staticLayer.PrefabName);
			}
			return false;
		}

		public static IEnumerator AddGenericWorldItemsToStructure(
			string pieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(pieces);
			for (int i = 0; i < childPieces.Length; i++) {
				ChildPiece childPiece = childPieces[i];
				WorldItem worlditem = null;
				WorldItems.CloneWorldItem(childPiece.PackName, childPiece.ChildName, childPiece.Transform, false, group, out worlditem);
				worlditem.Initialize();
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericDoorsToStructure(
			string pieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(pieces);
			for (int i = 0; i < childPieces.Length; i++) {
				ChildPiece childPiece = childPieces[i];
				//Debug.Log ("Adding door " + childPiece.ChildName);
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab(childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate(dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = childPiece.ChildName;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition = childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation = Quaternion.identity;
					instantiatedPrefab.transform.Rotate(childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab>();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					Door door = null;
					if (worlditem.gameObject.HasComponent <Door>(out door)) {
						door.State.OuterEntrance = exterior;
						door.IsGeneric = true;//this will help it set up its locks correctly the first time
					}
					worlditem.Initialize();
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericWindowsToStructure(
			string pieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(pieces);
			for (int i = 0; i < childPieces.Length; i++) {
				ChildPiece childPiece = childPieces[i];
				//Debug.Log ("Adding window " + childPiece.ChildName);
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab(childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate(dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = childPiece.ChildName;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition	= childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation	= Quaternion.identity;
					instantiatedPrefab.transform.Rotate(childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab>();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					bool addToLOD = (!worlditem.CanEnterInventory && !worlditem.CanBeCarried);
					worlditem.Group = group;
					Window window = null;
					if (worlditem.gameObject.HasComponent <Window>(out window)) {
						window.State.OuterEntrance = exterior;
						window.IsGeneric = true;//this will help it set up its locks correctly the first time
					}
					worlditem.Initialize();
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddGenericDynamicToStructure(
			string pieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(pieces);
			for (int i = 0; i < childPieces.Length; i++) {
				ChildPiece childPiece = childPieces[i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab(childPiece.PackName, childPiece.ChildName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate(dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = dynamicPrefab.name;
					instantiatedPrefab.transform.parent = parentTransform;
					instantiatedPrefab.transform.localPosition = childPiece.Position;
					instantiatedPrefab.transform.localScale = childPiece.Scale;
					instantiatedPrefab.transform.localRotation = Quaternion.identity;
					instantiatedPrefab.transform.Rotate(childPiece.Rotation);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab>();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					worlditem.Initialize();
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddFiresToStructure(
			string fires,
			Transform parentTransform)
		{
			ChildPiece[] firePieces = StructureTemplate.ExtractChildPiecesFromLayer(fires);
			for (int i = 0; i < firePieces.Length; i++) {
				ChildPiece piece = firePieces[i];
				FXManager.Get.SpawnFire(piece.ChildName, parentTransform, piece.Position, piece.Rotation, piece.Scale.x, false);
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddFXPiecesToStructure(
			string lights,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			LightPiece[] lightPieces = StructureTemplate.ExtractLightPiecesFromLayer(lights);
			for (int i = 0; i < lightPieces.Length; i++) {
				Light light = StructureTemplate.LightFromLightPiece(lightPieces[i], parentTransform);
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddUniqueDynamicToStructure(
			List <StackItem> dynamicPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			DynamicPrefab dynamicPrefab = null;
			WorldItem newWorldItem = null;
			for (int i = 0; i < dynamicPieces.Count; i++) {
				StackItem piece = dynamicPieces[i];
				if (Structures.Get.PackDynamicPrefab(piece.PackName, piece.PrefabName, out dynamicPrefab)) {
					GameObject instantiatedPrefab = GameObject.Instantiate(dynamicPrefab.gameObject) as GameObject;
					instantiatedPrefab.name = dynamicPrefab.name;
					instantiatedPrefab.transform.parent = parentTransform;
					piece.Transform.ApplyTo(instantiatedPrefab.transform);

					DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab>();
					WorldItem worlditem = dynPre.worlditem;
					worlditem.IsTemplate = false;
					worlditem.Group = group;
					worlditem.ReceiveState(piece);
					worlditem.Initialize();
				} else {
					//Debug.Log ("Couldn't get dynamic prefab");
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddUniqueWorldItemsToStructure(
			List <StackItem> wiPieces,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			for (int i = 0; i < wiPieces.Count; i++) {
				StackItem piece = wiPieces[i];
				WorldItem newWorldItem = null;
				if (WorldItems.CloneFromStackItem(piece, group, out newWorldItem)) {
					newWorldItem.transform.parent = parentTransform;
					piece.Transform.ApplyTo(newWorldItem.transform);
					newWorldItem.Initialize();
				} else {
					//Debug.Log ("Couldn't spawn " + piece.PackName + " / " + piece.PrefabName + " for some goddamn reason");
				}
			}
			//TODO split this up
			yield break;
		}

		public static IEnumerator AddTriggersToStructure(
			SDictionary <string, KeyValuePair <string,string>> triggers,
			Transform parentTransform,
			bool exterior,
			WIGroup group)
		{
			WorldChunk chunk = group.GetParentChunk();
			foreach (KeyValuePair <string, KeyValuePair <string,string>> triggerStatePair in triggers) {
				//this will add it to the chunk
				//at which point it will be managed by the chunk and not the structure
				chunk.AddTrigger(triggerStatePair, parentTransform, true);
			}

			yield break;
		}

		public static IEnumerator AddCatItemsToStructure(
			List <WICatItem> wiCatItems,
			Transform parentTransform,
			bool exterior,
			WIGroup group,
			Structure parentStructure)
		{
			int parentHashCode = parentStructure.worlditem.GetHashCode() + Profile.Get.CurrentGame.Seed;
			for (int i = 0; i < wiCatItems.Count; i++) {
				WICatItem catItem = wiCatItems[i];
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
				catItem.Flags.Union(parentStructure.State.StructureFlags);
				//Debug.Log ("Searching for category " + catItem.WICategoryName + " with flags " + catItem.Flags.ToString ());
				WICategory category = null;
				if (WorldItems.CloneRandomFromCategory(catItem.WICategoryName, group, catItem.Transform, catItem.Flags, spawnCode, spawnIndex, out newWorldItem)) {
					newWorldItem.transform.parent = parentTransform;
					FillStackContainer fillStackContainer = null;
					if (catItem.UseSettingsOnSpawnedContainers && newWorldItem.gameObject.HasComponent <FillStackContainer>(out fillStackContainer)) {
						catItem.ContainerSettings.Flags.Union(catItem.Flags);
						fillStackContainer.State.Flags.CopyFrom(catItem.ContainerSettings.Flags, false);//don't include size
						fillStackContainer.State.WICategoryName = catItem.ContainerSettings.WICategoryName;// = catItem.ContainerSettings;
					}
				}
				else {
					//Debug.Log ("Couldn't clone random from category " + catItem.WICategoryName + " with flags " + catItem.Flags.ToString ( ));
				}
			}
			//TODO split this up
			yield break;
		}

		public static float PriorityToSeconds(StructureLoadPriority priority)
		{	//TODO make thees global
			switch (priority) {
				case StructureLoadPriority.SpawnPoint:
					return 0.1f;
				case StructureLoadPriority.Immediate:
				default:
					return 0.25f;
				case StructureLoadPriority.Adjascent:
					return 0.5f;
				case StructureLoadPriority.Distant:
					return 1.0f;
			}
			return 0f;
		}

		public static void ReclaimColliders(Builder builder, Structure structure)
		{
			List <BoxCollider> collidersToReclaim = null;
			if (builder.Mode == BuilderMode.Exterior) {
				collidersToReclaim = structure.ExteriorBoxColliders;
			} else {
				collidersToReclaim = structure.InteriorBoxColliders;
			}
			Structures.ReclaimBoxColliders(collidersToReclaim);
			collidersToReclaim.Clear();
		}

		public static List <Renderer> renderers;
		public static List <Renderer> lodRenderers;
		public static List <Renderer> renderersDestroyed;
		public static List <Renderer> lodRenderersDestroyed;
		public static List <MeshFilter> structureMeshes;

		public static IEnumerator UnloadStructureMeshes(Builder builder, Structure structure)
		{
			Debug.Log("Unloading structure meshes for " + structure.name);
			if (builder.Mode == Builder.BuilderMode.Exterior) {
				structureMeshes = structure.ExteriorMeshes;
				renderers = structure.ExteriorRenderers;
			} else {
				structureMeshes = structure.InteriorMeshes;
				renderers = structure.InteriorRenderers;
			}

			for (int i = 0; i < renderers.Count; i++) {
				if (renderers[i] != null) {
					GameObject.Destroy(renderers[i].gameObject);
				}
			}
			renderers.Clear();
			yield return null;

			if (lodRenderers != null) {
				for (int i = 0; i < lodRenderers.Count; i++) {
					if (renderers[i] != null) {
						GameObject.Destroy(lodRenderers[i].gameObject);
					}
				}
				lodRenderers.Clear();
				yield return null;
			}


			if (renderersDestroyed != null) {
				for (int i = 0; i < renderersDestroyed.Count; i++) {
					if (renderers[i] != null) {
						GameObject.Destroy(renderersDestroyed[i].gameObject);
					}
				}
				renderersDestroyed.Clear();
				yield return null;
			}

			if (lodRenderersDestroyed != null) {
				for (int i = 0; i < lodRenderersDestroyed.Count; i++) {
					if (renderers[i] != null) {
						GameObject.Destroy(lodRenderersDestroyed[i].gameObject);
					}
				}
				lodRenderersDestroyed.Clear();
				yield return null;
			}

			yield break;
		}

		public static IEnumerator UnloadStructureMeshes(List <MeshFilter> structureMeshes)
		{
			//the goal here is to find every mesh associated with the struture
			//then destroy that mesh - and actually destroy the thing so it's not in memory!
			for (int i = 0; i < structureMeshes.Count; i++) {
				if (structureMeshes[i] != null) {
					GameObject.Destroy(structureMeshes[i].sharedMesh);
					GameObject.Destroy(structureMeshes[i].gameObject);
				}
			}
			structureMeshes.Clear();
			yield break;
		}

		public static string GetChildName(bool interior, bool destroyed)
		{
			return GetChildName(interior, destroyed, -1);
		}

		public static string GetChildName(bool interior, bool destroyed, int minorStructureNum)
		{
			if (gChildNameBuilder == null) {
				gChildNameBuilder = new System.Text.StringBuilder();
			}

			gChildNameBuilder.Clear();
			gChildNameBuilder.Append("__");
			if (minorStructureNum >= 0) {
				gChildNameBuilder.Append("MINOR_");
				gChildNameBuilder.Append(minorStructureNum.ToString());
			} else if (interior) {
				gChildNameBuilder.Append("INTERIOR_");
			} else {
				gChildNameBuilder.Append("EXTERIOR_");
			}
			if (destroyed) {
				gChildNameBuilder.Append("_DESTROYED_");
			}
			return gChildNameBuilder.ToString();
		}

		public static string GetMeshPrefix(bool interior, bool destroyed)
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

		protected static string gIntMeshPrefix = "__StrMesh-INT";
		protected static string gExtMeshPrefix = "__StrMesh-EXT";
		protected static string gDstMeshPrefix = "__StrMesh-DST";
		protected static System.Text.StringBuilder gChildNameBuilder;
		protected static System.Text.StringBuilder gNameBuilder;
		public static string GetTemplateName(string rootName)
		{
			string[] templateName = rootName.Split('-');
			return templateName[0];
		}

		protected static Transform gHelperTransform;
		protected static StructureBuilder gCRunner;

		public sealed class MeshCombinerResult
		{
			public int Hash;
			public Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] MeshOutputs;

			public void Clear()
			{
				if (MeshOutputs != null) {
					for (int i = 0; i < MeshOutputs.Length; i++) {
						if (MeshOutputs[i] != null) {
							MeshOutputs[i].Clear();
						}
					}
					Array.Clear(MeshOutputs, 0, MeshOutputs.Length);
				}
			}

			public void MeshCombinerCallback(int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
			{
				Hash = hash;
				MeshOutputs = meshOutputs;
			}
		}
	}
}
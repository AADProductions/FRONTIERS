using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Hydrogen.Threading.Jobs;

namespace Frontiers.World
{
		public partial class StructureBuilder : Builder
		{
				public Structure ParentStructure;
				public StructureTemplate Template;
				public MeshCombiner PrimaryCombiner;
				public MeshCombiner LODCombiner;
				public MeshCombiner DestroyedCombiner;
				public MeshCombiner LODDestroyedCombiner;
				public MeshCombiner ColliderCombiner;
				public WorldChunk StructureChunk;
				public List <int> InteriorVariants = new List <int>();
				//these are set by the callback when the meshes are finished
				public int CombinerHash;
				public MeshCombiner.MeshOutput[] MeshOutputs;
				public int LODCombinerHash;
				public MeshCombiner.MeshOutput[] LODMeshOutputs;
				public WIGroup MinorGroup;
				public bool IsUnloading = false;

				public IEnumerator Initialize(Structure parentStructure, StructureTemplate template, StructureLoadPriority priority)
				{		//if we're still doing another job (for some reason) wait for it to finish
						while (State != BuilderState.Dormant) {
								yield return null;
						}

						gHelperTransform = gameObject.FindOrCreateChild("StructureBuilderHelper_" + name);

						try {
								StructureBase = parentStructure.StructureGroup.gameObject;
								ParentStructure = parentStructure;
								Template = template;
								State = BuilderState.Initialized;
								Priority = priority;
								StructureChunk = parentStructure.worlditem.Group.GetParentChunk();
								InteriorVariants.Clear();
								InteriorVariants.Add(ParentStructure.State.BaseInteriorVariant);
								InteriorVariants.AddRange(ParentStructure.State.AdditionalInteriorVariants);
						} catch (Exception e) {
								Debug.LogError("Error in structure builder, proceeding normally:" + e.ToString());
								State = BuilderState.Error;
						}

						yield break;
				}

				public IEnumerator Initialize(MinorStructure minorParent, StructureTemplate template, StructureLoadPriority priority)
				{	//if we're still doing another job (for some reason) wait for it to finish
						while (State != BuilderState.Dormant) {
								yield return null;
						}

						gHelperTransform = gameObject.FindOrCreateChild("StructureBuilderHelper_" + name);

						try {
								MinorParent = minorParent;
								Template = template;
								State = BuilderState.Initialized;
								Priority = priority;
								StructureChunk = MinorParent.Chunk;
								StructureBase = MinorParent.StructureOwner.gameObject;
						} catch (Exception e) {
								Debug.LogException(e);
								State = BuilderState.Error;
						}
						yield break;
				}

				public void Reset()
				{
						StructureBase = null;
						StructurePiece = null;
						ParentStructure = null;
						MinorParent = null;
						if (MaterialLookup != null) {
								MaterialLookup.Clear();
						}
						if (Template != null) {
								Template.Clear();
						}
						Template = null;
						if (gHelperTransform != null) {
								gHelperTransform.transform.parent = transform;
						}
						State = BuilderState.Dormant;
				}

				#region editing structures

				#if UNITY_EDITOR
				public void DuplicateAsMeshStructure()
				{
						Transform normal = transform.FindChild("==NORMAL==");
						if (normal != null) {
								Debug.Log("Can't create template - not empty");
								return;
						}
						StructureBuilder.CreateTemporaryPrefab(this, transform, StructureBuilder.GetTemplateName(name));
				}

				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.cyan;
						if (GUILayout.Button("Load Next Available Template")) {
								LoadNextAvailabeTemplate();
						}
						if (GUILayout.Button("Create Structure Transforms")) {
								StructureBuilder.CreateStructureTransforms(transform);
						}
						if (GUILayout.Button("\nCalculate Structure Footprint\n")) {
								CalculateFootprint();
						}
						if (GUILayout.Button("\nCreate Structure Footprint\n")) {
								CreateStructureFootprint();
						}
						if (GUILayout.Button("\nDuplicate Foundation\n")) {
								CreateFoundation();
						}
						if (GUILayout.Button("\nCopy Colliders\n")) {

								StructureBuilder.EditorCopyStaticColliders(transform);
						}
						if (GUILayout.Button("\nDuplicate as mesh structure\n")) {
								DuplicateAsMeshStructure();
						}
						if (GUILayout.Button("\nToggle Destroyed\n")) {
								StructureBuilder.ToggleDestroyedPrefabs();
						}

						UnityEngine.GUI.color = Color.yellow;
						GUILayout.Label("FILE SAVE AND LOAD OPTIONS:");

						if (GUILayout.Button("\n(SAVE TEMPLATE)\n")) {
								StructureBuilder.SaveEditorStructureToTemplate(transform);
						}

						GUILayout.Label("Exterior Custom Colliders Normal: " + NumExteriorCustomCollidersNormal.ToString());
						GUILayout.Label("Exterior Custom Colliders Destroyed: " + NumExteriorCustomCollidersDestroyed.ToString());
						GUILayout.Label("Exterior Static Colliders Normal: " + NumExteriorStaticCollidersNormal.ToString());
						GUILayout.Label("Exterior Static Colliders Destroyed: " + NumExteriorStaticCollidersDestroyed.ToString());

						GUILayout.Label("Interior Custom Colliders Normal: " + NumInteriorCustomCollidersNormal.ToString());
						GUILayout.Label("Interior Custom Colliders Destroyed: " + NumInteriorCustomCollidersDestroyed.ToString());
						GUILayout.Label("Interior Static Colliders Normal: " + NumInteriorStaticCollidersNormal.ToString());
						GUILayout.Label("Interior Static Colliders Destroyed: " + NumInteriorStaticCollidersDestroyed.ToString());

						if (Application.isPlaying) {
								if (ParentStructure != null) {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Label("BUILDING STRUCTURE: " + ParentStructure.name);
								}
						} else {
								if (Template != null) {

								}
						}
				}

				public void CreateFoundation()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor(true);

						if (!Manager.IsAwake <Structures>()) {
								Manager.WakeUp <Structures>("Frontiers_Structures");
						}
						Structures.Get.Initialize();

						if (!Manager.IsAwake <WorldItems>()) {
								Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
						}
						WorldItems.Get.Initialize();

						StructureTemplate template = null;
						if (Mods.Get.Editor.LoadMod <StructureTemplate>(ref template, "Structure", StructureBuilder.GetTemplateName(name))) {
								GameObject foundation = gameObject.FindOrCreateChild("__TEMP_FOUNDATION").gameObject;
								foreach (StructureLayer staticLayer in template.Exterior.StaticStructureLayers) {
										string prefabName = staticLayer.PrefabName.Trim().ToLower();
										if (prefabName.Contains("foundation")) {
												StructurePackPrefab prefab = null;
												if (Structures.Get.PackStaticPrefab(staticLayer.PackName, staticLayer.PrefabName, out prefab)) {
														List <ChildPiece> staticPieces = new List<ChildPiece>();
														StructureTemplate.ExtractChildPiecesFromLayer(staticPieces, staticLayer.Instances);
														//ChildPiece[] staticPieces = StructureTemplate.ExtractChildPiecesFromLayer(staticLayer.Instances);
														for (int i = 0; i < staticPieces.Count; i++) {
																ChildPiece piece = staticPieces[i];
																GameObject instantiatedPrefab = UnityEditor.PrefabUtility.InstantiatePrefab(prefab.Prefab) as GameObject;
																//instantiate a new prefab - keep it as a prefab!
																instantiatedPrefab.name = prefab.Prefab.name;
																instantiatedPrefab.transform.parent = foundation.transform;
																instantiatedPrefab.tag = staticLayer.Tag;
																instantiatedPrefab.layer = staticLayer.Layer;
																//put it in the right place
																instantiatedPrefab.transform.localPosition = piece.Position;
																instantiatedPrefab.transform.localRotation = Quaternion.identity;
																instantiatedPrefab.transform.Rotate(piece.Rotation);
																instantiatedPrefab.transform.localScale = piece.Scale;

																Material[] variationsArray = null;
																if (staticLayer.Substitutions != null && staticLayer.Substitutions.Count > 0) {
																		MeshRenderer pmr = prefab.MRenderer;
																		variationsArray = pmr.sharedMaterials;
																		string newMaterialName = string.Empty;
																		for (int j = 0; j < variationsArray.Length; j++) {
																				if (staticLayer.Substitutions.TryGetValue(variationsArray[j].name, out newMaterialName)) {
																						Material sharedMaterial = null;
																						if (Structures.Get.SharedMaterial(newMaterialName, out sharedMaterial)) {
																								variationsArray[j] = sharedMaterial;
																						}
																				}
																		}
																		instantiatedPrefab.renderer.materials = variationsArray;
																}
														}
														staticPieces.Clear();
														staticPieces = null;
												}
										}
								}
						}
				}

				public void CreateStructureFootprint()
				{
						StructureTemplate template = null;
						if (Mods.Get.Editor.LoadMod <StructureTemplate>(ref template, "Structure", StructureBuilder.GetTemplateName(name))) {
								GameObject footprint = gameObject.FindOrCreateChild("__FOOTPRINT").gameObject;
								GameObject shingle = gameObject.FindOrCreateChild("__SHINGLE").gameObject;
								shingle.transform.localPosition = template.CommonShingleOffset;
								for (int i = 0; i < template.Footprint.Count; i++) {
										GameObject fp = footprint.FindOrCreateChild("Footprint" + i.ToString()).gameObject;
										fp.transform.localPosition = template.Footprint[i].Position;
										fp.transform.localScale = template.Footprint[i].Scale;
										fp.GetOrAdd <StructureFootprint>();
								}
						}
				}

				public void CalculateFootprint()
				{
						Renderer[] renderers = transform.GetComponentsInChildren <Renderer>(true);
						if (renderers.Length == 0) {
								//Debug.Log ("COULDN'T FIND ANY RENDERERS");
								return;
						}
						Bounds renderBounds = renderers[0].bounds;
						for (int i = 0; i < renderers.Length; i++) {
								renderBounds.Encapsulate(renderers[i].bounds);
						}
						GameObject footprintParent = gameObject.FindOrCreateChild("__FOOTPRINT").gameObject;
						GameObject footprint = footprintParent.FindOrCreateChild("Footprint1").gameObject;
						footprint.transform.position = renderBounds.center;
						footprint.transform.localScale = renderBounds.size;
						footprint.GetOrAdd <StructureFootprint>();
				}
				#endif
				#endregion

				#region generating structures

				public IEnumerator GenerateStructureMeshes()
				{
						State = BuilderState.BuildingMeshes;
						switch (Mode) {
								case BuilderMode.Exterior:
								default:
										mCurrentTemplateGroup = Template.Exterior;
										//extract the fires from the template and add it to the template
										if (ParentStructure.DestroyedFires == null) {
												ParentStructure.DestroyedFires = new List<ChildPiece>();
										}
										StructureTemplate.ExtractChildPiecesFromLayer(ParentStructure.DestroyedFires, Template.Exterior.DestroyedFires);
										ParentStructure.DestroyedFX = StructureTemplate.ExtractFXPiecesFromLayer(Template.Exterior.DestroyedFX);
										switch (Template.BuildMethod) {
												case StructureBuildMethod.MeshCombiner:
														//before we actually generate the meshes
														//see if it's cached
														/*Structure cachedInstance = null;
														if (Structures.Get.GetCachedInstance(Template.Name, ParentStructure, out cachedInstance)) {
																//hooray it's cached
																//duplicate it piece for piece
																yield return StartCoroutine(StructureBuilder.InstantiateMeshes(
																		ParentStructure,
																		cachedInstance,
																		this));

														} else {*/
														//otherwise just generate the meshes normally
														var generateMeshes = StructureBuilder.GenerateMeshes(
																                     mCurrentTemplateGroup,
																                     PrimaryCombiner,
																                     LODCombiner,
																                     DestroyedCombiner,
																                     LODDestroyedCombiner,
																                     GetChildName(false, false),
																                     ParentStructure.ExteriorMeshes,
																                     ParentStructure.ExteriorRenderers,
																                     ParentStructure.ExteriorRenderersDestroyed,
																                     ParentStructure.ExteriorLodRenderers,
																                     ParentStructure.ExteriorLodRenderersDestroyed,
																                     false,
																                     this);
														while (generateMeshes.MoveNext()) {
																yield return generateMeshes.Current;
														}
														/*}*/
														//non-destroyed custom colliders
														var generateCustomColliders = StructureBuilder.GenerateColliders(
																                        GetChildName(false, false),
																                        ParentStructure.ExteriorLayers,
																                        ParentStructure.ExteriorBoxColliders,
																                        ParentStructure.ExteriorMeshColliders,
																                        ParentStructure.ExteriorBoxCollidersDestroyed,
																                        ParentStructure.ExteriorMeshCollidersDestroyed,
																                        ParentStructure.ExteriorMeshes,
																                        mCurrentTemplateGroup.CustomStructureColliders,
																                        false,//exterior
																                        this);
														while (generateCustomColliders.MoveNext()) {
																yield return generateCustomColliders.Current;
														}
														//non-destroyed static colliders
														var generateStaticColliders = StructureBuilder.GenerateColliders(
																GetChildName(false, false),
																ParentStructure.ExteriorLayers,
																ParentStructure.ExteriorBoxColliders,
																ParentStructure.ExteriorMeshColliders,
																ParentStructure.ExteriorBoxCollidersDestroyed,
																ParentStructure.ExteriorMeshCollidersDestroyed,
																ParentStructure.ExteriorMeshes,
																mCurrentTemplateGroup.StaticStructureColliders,
																false,//exterior
																this);
														while (generateStaticColliders.MoveNext()) {
																yield return generateStaticColliders.Current;
														}

														break;

												case StructureBuildMethod.MeshInstances:
														//extract the fires from the template and add it to the template
														if (ParentStructure.DestroyedFires == null) {
																ParentStructure.DestroyedFires = new List<ChildPiece>();
														}
														StructureTemplate.ExtractChildPiecesFromLayer(ParentStructure.DestroyedFires, Template.Exterior.DestroyedFires);
														ParentStructure.DestroyedFX = StructureTemplate.ExtractFXPiecesFromLayer(Template.Exterior.DestroyedFX);
														var instanceMeshes = InstanceMeshes(
																                     mCurrentTemplateGroup,
																                     GetChildName(false, false),
																                     mExteriorRenderers,
																                     mExteriorLODRenderers,
																                     Globals.StructureExteriorLODRatio);
														while (instanceMeshes.MoveNext()) {
																yield return instanceMeshes.Current;
														}
														break;
										}
										mCurrentTemplateGroup = null;
										break;

								case BuilderMode.Interior:
										//we're not caching interior meshes yet
										for (int i = 0; i < InteriorVariants.Count; i++) {
												int interiorVariant = InteriorVariants[i];
												if (interiorVariant < Template.InteriorVariants.Count) {
														mCurrentTemplateGroup = Template.InteriorVariants[interiorVariant];
														var generateMeshes = StructureBuilder.GenerateMeshes(
																                     mCurrentTemplateGroup,
																                     PrimaryCombiner,
																                     LODCombiner,
																                     DestroyedCombiner,
																                     LODDestroyedCombiner,
																                     GetChildName(true, false),
																                     ParentStructure.InteriorMeshes,
																                     ParentStructure.InteriorRenderers,
																                     ParentStructure.InteriorRenderersDestroyed,
																                     null,
																                     null,
																                     true,
																                     this);
														while (generateMeshes.MoveNext()) {
																yield return generateMeshes.Current;
														}
														MaterialLookup.Clear();
														//non-destroyed custom colliders
														var generateCustomColliders = StructureBuilder.GenerateColliders(
																                              GetChildName(true, false),
																                              ParentStructure.InteriorLayers,
																                              ParentStructure.InteriorBoxColliders,
																                              ParentStructure.InteriorMeshColliders,
																                              ParentStructure.InteriorBoxCollidersDestroyed,
																                              ParentStructure.InteriorMeshCollidersDestroyed,
																                              ParentStructure.InteriorMeshes,
																                              mCurrentTemplateGroup.CustomStructureColliders,
																                              true,//interior
																                              this);
														while (generateCustomColliders.MoveNext()) {
																yield return generateCustomColliders.Current;
														}
														//non-destroyed static colliders
														var generateStaticColliders = StructureBuilder.GenerateColliders(
																                              GetChildName(true, true),
																                              ParentStructure.InteriorLayers,
																                              ParentStructure.InteriorBoxColliders,
																                              ParentStructure.InteriorMeshColliders,
																                              ParentStructure.InteriorBoxCollidersDestroyed,
																                              ParentStructure.InteriorMeshCollidersDestroyed,
																                              ParentStructure.InteriorMeshes,
																                              mCurrentTemplateGroup.StaticStructureColliders,
																                              true,//interior
																                              this);
														while (generateStaticColliders.MoveNext()) {
																yield return generateStaticColliders.Current;
														}
														yield return null;
														//Debug.Log ("Done generating interior variant " + interiorVariant.ToString () + ", state is " + State.ToString ());
												} else {
														Debug.Log ("Interior variant " + i.ToString () + " is out of range");
												}
										}
										//Debug.Log ("Finished generating states");
										mCurrentTemplateGroup = null;
										break;

								case BuilderMode.Minor:
										mCurrentTemplateGroup = Template.Exterior;
										switch (Template.BuildMethod) {
												case StructureBuildMethod.MeshCombiner:
														var generateMeshes = StructureBuilder.GenerateMeshes(
																                     mCurrentTemplateGroup,
																                     PrimaryCombiner,
																                     LODCombiner,
																                     DestroyedCombiner,
																                     LODDestroyedCombiner,
																                     GetChildName(false, false, MinorParent.Number),
																                     MinorParent.ExteriorMeshes,
																                     MinorParent.ExteriorRenderers,
																                     MinorParent.ExteriorRenderersDestroyed,
																                     MinorParent.ExteriorLODRenderers,
																                     MinorParent.ExteriorLODRenderersDestroyed,
																                     false,
																                     this);
														while (generateMeshes.MoveNext()) {
																yield return generateMeshes.Current;
														}
														StructurePiece.transform.ResetLocal();

														var generateStaticColliders = StructureBuilder.GenerateColliders(
																                              GetChildName(false, false, MinorParent.Number),
																                              MinorParent.ExteriorLayers,
																                              MinorParent.ExteriorBoxColliders,
																                              MinorParent.ExteriorMeshColliders,
																                              MinorParent.ExteriorBoxColliders,
																                              MinorParent.ExteriorMeshColliders,
																                              MinorParent.ExteriorMeshes,
																                              mCurrentTemplateGroup.CustomStructureColliders,
																                              false,
																                              this);
														while (generateStaticColliders.MoveNext()) {
																yield return generateStaticColliders.Current;
														}
														//StructurePiece.transform.ResetLocal ();

														var generateCustomColliders = StructureBuilder.GenerateColliders(
																GetChildName(false, false, MinorParent.Number),
																MinorParent.ExteriorLayers,
																MinorParent.ExteriorBoxColliders,
																MinorParent.ExteriorMeshColliders,
																MinorParent.ExteriorBoxColliders,
																MinorParent.ExteriorMeshColliders,
																MinorParent.ExteriorMeshes,
																mCurrentTemplateGroup.StaticStructureColliders,
																false,
																this);
														while (generateCustomColliders.MoveNext()) {
																yield return generateCustomColliders.Current;
														}
														//StructurePiece.transform.ResetLocal ();
														break;

												case StructureBuildMethod.MeshInstances:
														var instanceMeshes = InstanceMeshes(
																                     mCurrentTemplateGroup,
																                     GetChildName(false, false, MinorParent.Number),
																                     mExteriorRenderers,
																                     mExteriorLODRenderers,
																                     Globals.StructureExteriorLODRatio);
														while (instanceMeshes.MoveNext()) {
																yield return instanceMeshes.Current;
														}
														break;
										}
										//StructurePiece.transform.ResetLocal();
										mCurrentTemplateGroup = null;
										break;
						}
						State = Builder.BuilderState.Finished;
				}

				protected StructureTemplateGroup mCurrentTemplateGroup = null;

				public IEnumerator GenerateStructureItems()
				{
						yield return null;
						Transform structureItems = null;
						WIGroup group = null;
						State = BuilderState.BuildingItems;
						switch (Mode) {
								case BuilderMode.Exterior:
								default:
										group = ParentStructure.StructureGroup;
										structureItems = group.gameObject.FindOrCreateChild("_ITEMS_EXT");
										var generateExtItems = StructureBuilder.GenerateExteriorItems(
												                       ParentStructure,
												                       Template.Exterior,
												                       group,
												                       structureItems);
										while (generateExtItems.MoveNext()) { 
												yield return generateExtItems.Current;
										}
										break;

								case BuilderMode.Interior:
										for (int i = 0; i < InteriorVariants.Count; i++) {
												int interiorVariant = InteriorVariants[i];
												if (!ParentStructure.StructureGroupInteriors.ContainsKey(interiorVariant)) {
														Debug.Log("Didn't find interior variant " + interiorVariant.ToString() + " in " + ParentStructure.name + " - waiting for a bit...");
														double timeOut = WorldClock.RealTime + 10f;
														while (ParentStructure != null && !ParentStructure.StructureGroupInteriors.ContainsKey(interiorVariant)) {
																//waiting for interior groups to spawn
																yield return null;
																if (WorldClock.RealTime > timeOut) {
																		Debug.Log("Timed out waiting for interior group " + interiorVariant.ToString() + " in " + ParentStructure.name);
																		break;
																}
														}
												}

												try {
													group = ParentStructure.StructureGroupInteriors[interiorVariant];
													structureItems = group.gameObject.FindOrCreateChild("_ITEMS_INT_" + interiorVariant.ToString());
												} catch (Exception e) {
													Debug.LogError(e.ToString());
													continue;
												}

												//Debug.Log ("Generating items for interior variant " + interiorVariant.ToString ());
												if (!ParentStructure.State.InteriorsLoadedOnce.Contains(interiorVariant) && interiorVariant < Template.InteriorVariants.Count) {
														var generateIntItems = StructureBuilder.GenerateInteriorItems(
																                       ParentStructure,
																                       interiorVariant,
																                       Template.InteriorVariants[interiorVariant],
																                       group,
																                       structureItems);
														while (generateIntItems.MoveNext()) { 
																yield return generateIntItems.Current;
														}
												}

												yield return null;
										}
										break;

								case BuilderMode.Minor:
										//minor structures don't generate items
										break;
						}
				}

				protected IEnumerator InstanceMeshes(
						StructureTemplateGroup structureGroup,
						string childName,
						List <Renderer> renderers,
						List <Renderer> lodRenderers,
						float lodRatio)
				{
						renderers.Clear();
						if (lodRenderers != null) {
								lodRenderers.Clear();
						}

						StructurePiece = StructureBase.CreateChild(childName);
						StructurePiece.parent = null;
						if (Mode == BuilderMode.Minor) {
								//minor structures have to set their own offsets
								StructurePiece.position = MinorParent.Position;
								StructurePiece.rotation = Quaternion.Euler(MinorParent.Rotation);
						} else {
								StructurePiece.ResetLocal();
						}
						StructurePiece.gameObject.layer = Globals.LayerNumSolidTerrain;//TODO this may be unnecessary
						for (int i = 0; i < structureGroup.StaticStructureLayers.Count; i++) {
								StructureTemplate.InstantiateStructureLayer(structureGroup.StaticStructureLayers[i], StructurePiece);
						}

						StructurePiece.parent = StructureBase.transform;
						if (Mode == BuilderMode.Minor) {
								StructurePiece.localPosition = MinorParent.Position;
								StructurePiece.localRotation = Quaternion.Euler(MinorParent.Rotation);
						} else {
								StructurePiece.ResetLocal();
						}
						yield break;
				}

				protected List <Renderer> mInteriorRenderers = new List <Renderer>();
				protected List <Renderer> mExteriorRenderers = new List <Renderer>();
				protected List <Renderer> mExteriorLODRenderers = new List<Renderer>();

				protected IEnumerator GenerateMinorItems()
				{
						StructureTemplateGroup structureGroup = Template.Exterior;
						yield return StartCoroutine(StructureBuilder.AddGenericWorldItemsToStructure(structureGroup.GenericWItems, MinorGroup.transform, true, MinorGroup));
						yield return StartCoroutine(StructureBuilder.AddUniqueWorldItemsToStructure(structureGroup.UniqueWorlditems, MinorGroup.transform, true, MinorGroup));
						yield break;
				}

				public void MeshCombinerCallback(int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
				{
						CombinerHash = hash;
						MeshOutputs = meshOutputs;
						State = BuilderState.HandlingMeshes;
				}

				public void LODMeshCombinerCallback(int hash, Hydrogen.Threading.Jobs.MeshCombiner.MeshOutput[] meshOutputs)
				{
						LODCombinerHash = hash;
						LODMeshOutputs = meshOutputs;
				}

				#endregion

		}

		public class StructurePiece
		{
				public StructurePiece(string templateName, bool exterior, int interiorVariant, bool isMinorStructure, int minorParentNumber, MinorStructure minor)
				{
						TemplateName = templateName;
						Exterior = exterior;
						InteriorVariant = interiorVariant;
						IsMinorStructure = isMinorStructure;
						MinorParentNumber = minorParentNumber;
						Minor = minor;
						IsDestroyed = false;
				}

				public string TemplateName;
				public bool Exterior;
				public int InteriorVariant;
				public bool IsMinorStructure;
				public int MinorParentNumber;
				public MinorStructure Minor;
				public bool IsDestroyed;
		}

		public enum PieceType
		{
				Collider,
				Dynamic,
				FX,
				Static,
				Substructure,
				Worlditem,
				Character,
		}
}

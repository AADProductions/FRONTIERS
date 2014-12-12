using UnityEngine;

//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using Frontiers;
using Frontiers.World.Gameplay;
using Hydrogen.Threading.Jobs;
using System.Xml.Serialization;

namespace Frontiers.World
{
	[Serializable]
	public class StructureTemplate : Mod
	{
		public SVector3 CommonShingleOffset = new SVector3();
		public STransform CommonSignboardOffset = new STransform();
		public StructureBuildMethod BuildMethod = StructureBuildMethod.MeshCombiner;
		public StructureTemplateGroup Exterior = new StructureTemplateGroup();
		public List <StructureTemplateGroup> InteriorVariants = new List <StructureTemplateGroup>();
		public List <STransform> Footprint = new List <STransform>();

		public int NumInteriorVariants {
			get {
				return InteriorVariants.Count;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < InteriorVariants.Count; i++) {
				ClearTemplateGroup(InteriorVariants[i]);
			}
			ClearTemplateGroup(Exterior);
		}

		#region static helper functions

		public static void ClearTemplateGroup(StructureTemplateGroup stg)
		{
			stg.ActionNodes.Clear();
			stg.StaticStructureLayers.Clear();
			stg.UniqueDoors.Clear();
			stg.UniqueDynamic.Clear();
			stg.UniqueWindows.Clear();
			stg.UniqueWorlditems.Clear();

			stg.UniqueDoors = null;
			stg.UniqueDynamic = null;
			stg.UniqueWindows = null;
			stg.UniqueWorlditems = null;
			stg.StaticStructureLayers = null;
			stg.ActionNodes = null;
			stg.DestroyedFX = null;
			stg.DestroyedFires = null;
			stg.GenericDoors = null;
			stg.GenericDynamic = null;
			stg.GenericLights = null;
			stg.GenericWindows = null;
			stg.GenericWItems = null;
		}

		public static void InstantiateStructureLayer(StructureLayer staticLayer, Transform childParent)
		{
			//instantiate the child objects
			ChildPiece childPiece = ChildPiece.Empty;
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(staticLayer.Instances);
			//send the child pieces to the mesh combiner and wait for it to finish
			StructurePackPrefab prefab = null;
			if (Structures.Get.PackStaticPrefab(staticLayer.PackName, staticLayer.PrefabName, out prefab)) {
				//add the child pieces to the mesh combiner
				if (childPieces.Length > 0) {
					for (int j = 0; j < childPieces.Length; j++) {
						childPiece = childPieces[j];
						//use the helper to create a world matrix
						GameObject instantiatedPrefab = GameObject.Instantiate(prefab.Prefab) as GameObject;
						instantiatedPrefab.transform.parent = childParent;
						instantiatedPrefab.transform.localPosition = childPiece.Position;
						instantiatedPrefab.transform.localRotation = Quaternion.Euler(childPiece.Rotation);
						instantiatedPrefab.transform.localScale = childPiece.Scale;
						instantiatedPrefab.layer = staticLayer.Layer;
						instantiatedPrefab.tag = staticLayer.Tag;
					}
				}
				Array.Clear(childPieces, 0, childPieces.Length);
				childPieces = null;
			} else {	
				//Debug.Log ("Couldn't load " + staticLayer.PackName + ", " + staticLayer.PrefabName + " in " + childParent.name);
			}
		}

		public static void InstantiateGenericDynamic(string dynamicInstances, Transform childParent, WIGroup group)
		{
			//instantiate the child objects
			ChildPiece childPiece = ChildPiece.Empty;
			ChildPiece[] childPieces = StructureTemplate.ExtractChildPiecesFromLayer(dynamicInstances);
			//send the child pieces to the mesh combiner and wait for it to finish
			DynamicPrefab prefab = null;
			if (childPieces.Length > 0) {
				for (int j = 0; j < childPieces.Length; j++) {
					childPiece = childPieces[j];
					//use the helper to create a world matrix
					if (Structures.Get.PackDynamicPrefab(childPiece.PackName, childPiece.ChildName, out prefab)) {
						//add the child pieces to the mesh combiner
						GameObject instantiatedPrefab = GameObject.Instantiate(prefab.gameObject) as GameObject;
						instantiatedPrefab.name = prefab.name;
						instantiatedPrefab.transform.parent = childParent;
						instantiatedPrefab.transform.localPosition = childPiece.Position;
						instantiatedPrefab.transform.localRotation = Quaternion.Euler(childPiece.Rotation);
						instantiatedPrefab.transform.localScale = childPiece.Scale;
						DynamicPrefab dynPre = instantiatedPrefab.GetComponent <DynamicPrefab>();
						if (dynPre.worlditem != null) {
							dynPre.worlditem.Group = group;
							WorldItems.InitializeWorldItem(dynPre.worlditem);
						}
					} else {	
						//Debug.Log ("Couldn't load " + childPiece.PackName + ", " + childPiece.ChildName + " in " + childParent.name);
					}
				}
			}
			Array.Clear(childPieces, 0, childPieces.Length);
			childPieces = null;
		}

		public static void AddSubstructuresToStructureTemplate(Transform start, ref string substructures)
		{
			foreach (Transform child in start) {
				string packName = Structures.Get.PackName(child.name);
				StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
				StructureDestroyResult str = null;
				if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
					behavior = str.Behavior;
				}
				StructureTemplate.AppendGenericChildPieceToLayer(packName, child.name, child, null, ref substructures, false, behavior);
			}
		}

		public static void AddStaticCollidersToStructureTemplate(Transform start, List <StructureLayer> templateLayers, Transform module)
		{
			Transform staticColliderHelper = start.gameObject.FindOrCreateChild("StaticColliderHelper");

			foreach (Transform child in start) {
				if (child.renderer == null && child.collider == null && module == null) {
					//this is a module, not a static child
					AddStaticChildrenToStructureTemplate(child, start, templateLayers);
				} else {
					bool addChild = true;
					if (child.gameObject.layer == Globals.LayerNumHidden || child.gameObject.layer == Globals.LayerNumStructureIgnoreCollider) {
						//Debug.Log ("Structure ignore collider or hidden layer found, ignoring layer");
						addChild = false;
					}

					if (addChild) {
						//clean up layers and tags
						if (child.tag == "Untagged") {
							child.tag = "GroundWood";
						}
						//Debug.Log ("Checking child " + child.name);
						//sort children by pack and prefab name
						int layer = child.gameObject.layer;
						string tag = child.gameObject.tag;
						StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
						StructureDestroyResult str = null;
						if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
							behavior = str.Behavior;
						}

						List <Collider> childColliders = new List <Collider>();
						if (child.collider != null) {
							childColliders.Add(child.collider);
						} else {
							//Debug.Log ("Child collider was null, looking for sub colliders");
							Collider[] childColliderComponents = child.GetComponentsInChildren <Collider>(true);
							childColliders.AddRange(childColliderComponents);
							//Debug.Log ("Found " + childColliders.Count.ToString () + " colliders");
						}

						for (int i = 0; i < childColliders.Count; i++) {
							Collider childCollider = childColliders[i];
							//the pack name is based on the collider type (sphere, box, whatever)
							//if it's a mesh collider then we save the mesh collider name
							string packName = childCollider.GetType().Name;
							string prefabName = childCollider.name;

							//look for an existing layer that matches our own
							StructureLayer structureLayer = null;
							for (int j = 0; j < templateLayers.Count; j++) {
								if (templateLayers[j].PackName == packName
								            && templateLayers[j].PrefabName == prefabName
								            && templateLayers[j].Layer == layer
								            && templateLayers[j].Tag == tag
								            && templateLayers[j].DestroyedBehavior == behavior) {
									structureLayer = templateLayers[j];
									break;
								}
							}
							//if we didn't find it, make a new layer
							if (structureLayer == null) {
								structureLayer = new StructureLayer();
								structureLayer.PackName = packName;
								structureLayer.PrefabName = prefabName;
								structureLayer.Layer = layer;
								structureLayer.Tag = tag;
								structureLayer.DestroyedBehavior = behavior;
								//add it to the template
								templateLayers.Add(structureLayer);
							}
							//colliders may have odd dimensions and offsets
							//to prevent us from getting a '1,1,1' collider
							//use the helper transform to get the actual dimensions
							staticColliderHelper.parent = childCollider.transform;
							staticColliderHelper.ResetLocal();
							staticColliderHelper.parent = start;
							BoxCollider bc = childCollider as BoxCollider;
							if (bc != null && bc.size != Vector3.one) {
								staticColliderHelper.localScale = Vector3.Scale(staticColliderHelper.transform.localScale, bc.size);
								staticColliderHelper.Translate(bc.center);
							}

							//add another instance to this layer
							structureLayer.NumInstances++;
							structureLayer.Instances = StructureLayer.AddInstance(structureLayer.Instances, staticColliderHelper, module, behavior);
						}
					}
				}
			}

			GameObject.DestroyImmediate(staticColliderHelper.gameObject);
		}

		public static void AddCustomCollidersToStructureTemplate(Transform start, List <StructureLayer> colliderLayers)
		{
			foreach (Transform child in start) {
				string packName = child.collider.GetType().Name;
				string prefabName = child.name;
				string tag = child.tag;
				int layer = child.gameObject.layer;
				StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
				StructureDestroyResult str = null;
				if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
					behavior = str.Behavior;
				}

				//look for an existing layer that matches our own
				StructureLayer colliderLayer = null;
				for (int i = 0; i < colliderLayers.Count; i++) {
					if (colliderLayers[i].PackName == packName
					         && colliderLayers[i].PrefabName == prefabName
					         && colliderLayers[i].Layer == layer
					         && colliderLayers[i].Tag == tag
					         && colliderLayers[i].DestroyedBehavior == behavior) {
						//we don't need to match substitutions
						colliderLayer = colliderLayers[i];
						break;
					}
				}
				//if we didn't find it, make a new layer
				if (colliderLayer == null) {
					colliderLayer = new StructureLayer();
					colliderLayer.PackName = packName;
					colliderLayer.PrefabName = prefabName;
					colliderLayer.Layer = layer;
					colliderLayer.Tag = tag;
					colliderLayer.DestroyedBehavior = behavior;
					//add it to the template
					colliderLayers.Add(colliderLayer);
					//Debug.Log ("Added another layer");
				}
				//add another instance to this layer
				colliderLayer.NumInstances++;
				colliderLayer.Instances = StructureLayer.AddInstance(colliderLayer.Instances, child, null, behavior);
			}
		}

		public static void AddGenericDynamicToStructureTemplate(Transform start, ref string genericDynamic)
		{
			foreach (Transform child in start) {
				string packName = Structures.Get.PackName(child.name);
				StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
				StructureDestroyResult str = null;
				if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
					behavior = str.Behavior;
				}
				StructureTemplate.AppendGenericChildPieceToLayer(packName, child.name, child, null, ref genericDynamic, false, behavior);
			}
		}

		public static void AddUniqueDynamicToStructureTemplate(Transform start, List <StackItem> dynamicTemplatePieces)
		{
			foreach (Transform child in start) {
				DynamicPrefab dynPre = null;
				WorldItem worlditem = null;
				if (child.gameObject.HasComponent <DynamicPrefab>(out dynPre)) {
					worlditem = child.GetComponent <DynamicPrefab>().worlditem;
				} else {
					worlditem = child.GetComponent <WorldItem>();
				}
				worlditem.IsTemplate = false;
				#if UNITY_EDITOR
				worlditem.OnEditorRefresh();
				#endif
				StackItem template = worlditem.GetStackItem(WIMode.World);
				if (template != null) {
					template.Transform.CopyFrom(child.transform);
					dynamicTemplatePieces.Add(template);
				} else {
					//Debug.Log ("Template was null in " + child.name);
				}
			}
		}

		public static void AddWindowsToStructureTemplate(Transform start, List <DynamicStructureTemplatePiece> dynamicTemplatePieces)
		{
			foreach (Transform child in start) {
				WorldItem worlditem = child.GetComponent <DynamicPrefab>().worlditem;
				if (worlditem != null) {
					DynamicStructureTemplatePiece template = Structures.Get.StackItemFromName(child.name);
					if (template != null) {
						template.SaveState = worlditem.GetSaveState(true);
						template.Transform.CopyFrom(child.transform);
						dynamicTemplatePieces.Add(template);
					}
				}
			}
		}

		public static void AddActionNodesToTemplate(Transform start, List <ActionNodeState> actionNodes)
		{
			Dictionary <string,int> numberedNodes = new Dictionary <string, int>();
			HashSet <string> nodeNamesRequiringNumbers = new HashSet <string>();
			List <ActionNode> nodes = new List<ActionNode>();
			foreach (Transform child in start) {
				ActionNode node = child.GetComponent <ActionNode>();
				if (node != null) {
					nodes.Add(node);
					node.Refresh();
					string nodeName = node.State.Name;
					if (node.State.Name.Contains("-")) {
						nodeName = nodeName.Split(new string [] { "-" }, StringSplitOptions.RemoveEmptyEntries)[0];
						node.State.Name = nodeName;
					}
					if (numberedNodes.ContainsKey(nodeName)) {
						//Debug.Log (nodeName + " will have to be numbererd, it's already int the list");
						nodeNamesRequiringNumbers.Add(nodeName);
					} else {
						numberedNodes.Add(nodeName, 1);
					}
				}
			}
			//now that we've found them all, rename them and add them to template
			numberedNodes.Clear();
			foreach (ActionNode node in nodes) {
				//rename it with the new number
				int numberOfDuplicates = 1;
				if (nodeNamesRequiringNumbers.Contains(node.State.Name)) {
					//Debug.Log ("Node name " + node.State.Name + " requires a number");
					if (numberedNodes.TryGetValue(node.State.Name, out numberOfDuplicates)) {
						numberOfDuplicates = numberOfDuplicates + 1;
						numberedNodes[node.State.Name] = numberOfDuplicates;
					} else {
						numberOfDuplicates = 1;
						numberedNodes.Add(node.State.Name, numberOfDuplicates);
					}
					string newName = node.State.Name + "-" + numberOfDuplicates.ToString().PadLeft(3, '0');
					node.State.Name = newName;
				}
				node.Refresh();
				actionNodes.Add(node.State);
			}
		}

		public static void AddStaticChildrenToStructureTemplate(Transform start, List <StructureLayer> templateLayers)
		{
			AddStaticChildrenToStructureTemplate(start, null, templateLayers);
		}

		public static void AddStaticChildrenToStructureTemplate(Transform start, Transform module, List <StructureLayer> templateLayers)
		{
			foreach (Transform child in start) {
				if (child.renderer == null && child.collider == null && module == null) {
					//this is a module, not a static child
					AddStaticChildrenToStructureTemplate(child, start, templateLayers);
				} else {
					bool addChild = true;
					StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
					StructureDestroyResult str = null;
					if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
						behavior = str.Behavior;
					}
					if (child.gameObject.layer == Globals.LayerNumHidden) {
						addChild = false;
					}
					if (addChild) {
						//sort children by pack and prefab name
						//clean child object tags and layers
						if (child.gameObject.layer == Globals.LayerNumDefault) {
							//don't permit default layers
							child.gameObject.layer = Globals.LayerNumSolidTerrain;
						}
						if (child.tag == "Untagged") {
							child.tag = "GroundWood";
						}

						string prefabName = child.name;
						string packName = Structures.Get.PackName(prefabName);
						int layer = child.gameObject.layer;
						int numVertices = 0;
						string tag = child.gameObject.tag;
						bool enableSnow = false;
						//get a material variation list going
						SDictionary <string,string> substitutions = null;
						List <string> additionalMaterials = null;
						MeshRenderer mr = child.GetComponent <MeshRenderer>();
						//check for renderers in case we're adding colliders
						MeshFilter pmf = child.GetComponent <MeshFilter>();
						if (pmf != null) {
							numVertices = pmf.sharedMesh.vertexCount;
						}
						if (mr != null) {
							additionalMaterials = new List <string>();
							substitutions = new SDictionary <string, string>();
							Material[] childMaterials = mr.sharedMaterials;
							Material[] prefabMaterials = null;
							StructurePackPrefab prefab = null;
							if (Structures.Get.PackStaticPrefab(packName, prefabName, out prefab)) {
								MeshRenderer pmr = prefab.MRenderer;
								prefabMaterials = pmr.sharedMaterials;

								for (int i = 0; i < childMaterials.Length; i++) {
									if (childMaterials[i] != null && childMaterials[i].name == "SnowOverlayMaterial") {
										enableSnow = true;
									}
									//see if it's the same material in the same spot
									//if it isn't, add a substitution
									if (i < childMaterials.Length && i < prefabMaterials.Length) {
										if (prefabMaterials[i] != null && childMaterials[i] != null && childMaterials[i].name != "SnowOverlayMaterial") {
											if (prefabMaterials[i].name != childMaterials[i].name && !substitutions.ContainsKey(prefabMaterials[i].name)) {
												//Debug.Log ("Substituting " + childMaterials [i].name + " for " + prefabMaterials [i].name + " in child " + child.name);
												substitutions.Add(prefabMaterials[i].name, childMaterials[i].name);
											}
										}
									} else if (i >= prefabMaterials.Length) {
										//we've gone beyond the existing materials
										additionalMaterials.Add(childMaterials[i].name);
									}
								}
							}
						}

						//look for an existing layer that matches our own
						StructureLayer structureLayer = null;
						for (int i = 0; i < templateLayers.Count; i++) {
							if (templateLayers[i].PackName == packName
							           && templateLayers[i].PrefabName == prefabName
							           && templateLayers[i].Layer == layer
							           && templateLayers[i].Tag == tag
							           && templateLayers[i].EnableSnow == enableSnow
							           && templateLayers[i].DestroyedBehavior == behavior) {
								//so far so good - does the substitution match?
								//if we're dealing with colliders, they always will
								if (StructureLayer.SubstitutionsMatch(substitutions, templateLayers[i].Substitutions)
								            && StructureLayer.AdditionalMaterialsMatch(additionalMaterials, templateLayers[i].AdditionalMaterials)
								            && enableSnow == templateLayers[i].EnableSnow) {
									structureLayer = templateLayers[i];
									break;
								}
							}
						}
						//if we didn't find it, make a new layer
						if (structureLayer == null) {
							structureLayer = new StructureLayer();
							structureLayer.PackName = packName;
							structureLayer.PrefabName = prefabName;
							structureLayer.Layer = layer;
							structureLayer.Tag = tag;
							structureLayer.Substitutions = substitutions;
							structureLayer.AdditionalMaterials = additionalMaterials;
							structureLayer.DestroyedBehavior = behavior;
							structureLayer.EnableSnow = enableSnow;
							//add it to the template
							templateLayers.Add(structureLayer);
						}
						//add another instance to this layer
						structureLayer.NumInstances++;
						structureLayer.NumVertices += numVertices;
						structureLayer.Instances = StructureLayer.AddInstance(structureLayer.Instances, child, module);
					}
				}
			}
		}

		public static void AddFiresToStructureTemplate(Transform start, ref string fires)
		{
			if (fires == string.Empty) {
				fires = "\n";
			}

			foreach (Transform child in start) {
				if (child.tag == "Fire") {
					StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
					StructureDestroyResult str = null;
					if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
						behavior = str.Behavior;
					}
					StructureTemplate.AppendGenericChildPieceToLayer("Fire", child.name, child, null, ref fires, false, behavior);
				}
			}
		}

		public static void AddLightsToStructureTemplate(Transform start, ref string lights)
		{
			if (lights == string.Empty) {
				lights = "\n";
			}

			foreach (Transform child in start) {
				Light childLight = child.light;
				if (childLight != null) {
					bool addChild = true;
					StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
					StructureDestroyResult str = null;
					if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
						behavior = str.Behavior;
					}
					if (addChild) {
						StructureTemplate.AppendGenericLightPieceToLayer(child.name, childLight, ref lights);
					}
				}
			}
		}

		public static void AddFXToStructureTemplate(Transform start, ref string fx)
		{
			if (fx == string.Empty) {
				fx = "\n";
			}

			foreach (Transform child in start) {
				FXPieceTemplate childFX = child.GetComponent <FXPieceTemplate>();
				if (childFX != null) {
					bool addChild = true;
					StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
					StructureDestroyResult str = null;
					if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
						behavior = str.Behavior;
					}
					if (addChild) {
						StructureTemplate.AppendFXPieceToLayer(childFX, ref fx);
					}
				}
			}
		}

		public static void AddGenericWorldItemsToStructureTemplate(Transform start, ref string worlditems)
		{
			foreach (Transform child in start) {
				WorldItem worlditem = null;
				if (child.gameObject.HasComponent <WorldItem>(out worlditem)) {
					worlditem.IsTemplate = false;
					string packName = worlditem.Props.Name.PackName;
					string prefabName = worlditem.Props.Name.PrefabName;
					StructureDestroyedBehavior behavior = StructureDestroyedBehavior.None;
					StructureDestroyResult str = null;
					if (child.gameObject.HasComponent <StructureDestroyResult>(out str)) {
						behavior = str.Behavior;
					}
					StructureTemplate.AppendGenericChildPieceToLayer(packName, prefabName, child, null, ref worlditems, false, behavior);
				}
			}
		}

		public static void AddUniqueWorldItemsToStructureTemplate(Transform start, List <StackItem> uniqueWorldItems)
		{
			foreach (Transform child in start) {
				WorldItem worlditem = null;
				if (child.gameObject.HasComponent <WorldItem>(out worlditem)) {
					worlditem.IsTemplate = false;
					#if UNITY_EDITOR
					worlditem.OnEditorRefresh();
					#endif
					StackItem newStackItem = worlditem.GetStackItem(WIMode.Frozen);
					if (newStackItem != null) {
						uniqueWorldItems.Add(newStackItem);
					}
				}
			}
		}

		public static void AddCatWorldItemsToStructureTemplate(Transform start, List <WICatItem> categoryWorldItems)
		{
			List <WICategoryPlaceholder> catPlaceholders = new List <WICategoryPlaceholder>();

			foreach (Transform child in start) {
				WICategoryPlaceholder placeholder = null;
				if (child.gameObject.HasComponent <WICategoryPlaceholder>(out placeholder)) {
					catPlaceholders.Add(placeholder);
				}
			}

			for (int i = 0; i < catPlaceholders.Count; i++) {
				//check for duplicates
				for (int j = 0; j < catPlaceholders.Count; j++) {
					if (i != j && catPlaceholders[j] != null && catPlaceholders[i] != null && catPlaceholders[i].transform.position == catPlaceholders[j].transform.position) {
						GameObject.DestroyImmediate(catPlaceholders[i].gameObject);
					}
				}
				if (catPlaceholders[i] != null) {
					catPlaceholders[i].Item.Transform.CopyFrom(catPlaceholders[i].transform);
					categoryWorldItems.Add(catPlaceholders[i].Item);
				}
			}
		}

		public static void AddTriggersToStructureTemplate(Transform start, SDictionary <string,KeyValuePair <string, string>> triggers)
		{
			foreach (Transform child in start) {
				WorldTrigger trigger = null;
				if (child.gameObject.HasComponent <WorldTrigger>(out trigger)) {
					trigger.RefreshState(true);
					#if UNITY_EDITOR
					trigger.OnEditorRefresh();
					#endif
					string triggerState = string.Empty;
					if (trigger.GetTriggerState(out triggerState)) {
						//Debug.Log ("Got trigger state: " + triggerState);
						triggers.Add(trigger.name, new KeyValuePair <string,string>(trigger.ScriptName, triggerState));
					}
				}
			}
		}

		public static Light LightFromLightPiece(LightPiece piece, Transform parentTransform)
		{
			GameObject newLightGameObject = new GameObject(piece.LightName);
			newLightGameObject.transform.parent = parentTransform;
			newLightGameObject.transform.localPosition = piece.Position;
			newLightGameObject.transform.localRotation = Quaternion.Euler(piece.Rotation);

			Light newLight = newLightGameObject.AddComponent <Light>();
			newLight.type = (LightType)Enum.Parse(typeof(LightType), piece.LightType);
			newLight.color = piece.LightColor;
			newLight.intensity = piece.LightIntensity;
			newLight.range = piece.LightRange;
			newLight.spotAngle = piece.LightSpotAngle;
			newLight.cullingMask = Globals.LayersLightWorld;

			return newLight;
		}

		public static void AppendFXPieceToLayer(FXPieceTemplate piece, ref string fx)
		{
			List <string> fxPieces = new List <string>();
			//fxname, soundname, soundtype, fxcolr, fxcolg, fxcolb, delay, duration, posx, posy, posz, rotx, roty, rotz
			fxPieces.Add(piece.FXName);
			fxPieces.Add(piece.SoundName);
			fxPieces.Add(piece.SoundType.ToString());
			fxPieces.Add(piece.FXColor.r.ToString());
			fxPieces.Add(piece.FXColor.g.ToString());
			fxPieces.Add(piece.FXColor.b.ToString());
			fxPieces.Add(piece.FXDelay.ToString());
			fxPieces.Add(piece.FXDuration.ToString());
			fxPieces.Add(piece.transform.localPosition.x.ToString());
			fxPieces.Add(piece.transform.localPosition.y.ToString());
			fxPieces.Add(piece.transform.localPosition.z.ToString());
			fxPieces.Add(piece.transform.localRotation.eulerAngles.x.ToString());
			fxPieces.Add(piece.transform.localRotation.eulerAngles.y.ToString());
			fxPieces.Add(piece.transform.localRotation.eulerAngles.z.ToString());
			fxPieces.Add(piece.Explosion.ToString());
			fxPieces.Add(piece.JustForShow.ToString());

			fx = fx + fxPieces.JoinToString(",\t") + "\n";
		}

		public static void AppendGenericLightPieceToLayer(string lightName, Light light, ref string lights)
		{
			List <string> lightPieces = new List <string>();
			//name, type, intensity, colr, colg, colb, range, spotangle, posx, posy, posz, rotx, roty, rotz
			lightPieces.Add(lightName);
			lightPieces.Add(light.type.ToString());
			lightPieces.Add(light.intensity.ToString());
			lightPieces.Add(light.color.r.ToString());
			lightPieces.Add(light.color.g.ToString());
			lightPieces.Add(light.color.b.ToString());
			lightPieces.Add(light.range.ToString());
			lightPieces.Add(light.spotAngle.ToString());
			lightPieces.Add(light.transform.localPosition.x.ToString());
			lightPieces.Add(light.transform.localPosition.y.ToString());
			lightPieces.Add(light.transform.localPosition.z.ToString());
			lightPieces.Add(light.transform.localRotation.eulerAngles.x.ToString());
			lightPieces.Add(light.transform.localRotation.eulerAngles.y.ToString());
			lightPieces.Add(light.transform.localRotation.eulerAngles.z.ToString());

			lights = lights + lightPieces.JoinToString(",\t") + "\n";
		}

		public static void AppendGenericChildPieceToLayer(string packName, string childName, Transform child, Transform module, ref string childLayer, bool searchMats, StructureDestroyedBehavior behavior)
		{
			List <string> childLayerPieces	= new List <string>();
			List <string> childMaterials	= new List <string>();
			//packname,childname,xpos,ypos,zpos,xrot,yrot,zrot,xscl,yscl,zscl,mat1|mat2|mat3,behavior\n\r
			childLayerPieces.Add(packName);
			childLayerPieces.Add(childName);

			if (module == null) {
				childLayerPieces.Add(child.transform.localPosition.x.ToString());
				childLayerPieces.Add(child.transform.localPosition.y.ToString());
				childLayerPieces.Add(child.transform.localPosition.z.ToString());
				childLayerPieces.Add(child.transform.localRotation.eulerAngles.x.ToString());
				childLayerPieces.Add(child.transform.localRotation.eulerAngles.y.ToString());
				childLayerPieces.Add(child.transform.localRotation.eulerAngles.z.ToString());
				childLayerPieces.Add(child.transform.localScale.x.ToString());
				childLayerPieces.Add(child.transform.localScale.y.ToString());
				childLayerPieces.Add(child.transform.localScale.z.ToString());
			} else {
				//if module is not null then we need to transform the position using the module transform
				Vector3 childPosition = module.InverseTransformPoint(child.position);
				Vector3 childRotation = (Quaternion.Inverse(module.localRotation) * child.localRotation).eulerAngles;
				Vector3 childScale = child.lossyScale;

				childLayerPieces.Add(childPosition.x.ToString());
				childLayerPieces.Add(childPosition.y.ToString());
				childLayerPieces.Add(childPosition.z.ToString());
				childLayerPieces.Add(childRotation.x.ToString());
				childLayerPieces.Add(childRotation.y.ToString());
				childLayerPieces.Add(childRotation.z.ToString());
				childLayerPieces.Add(childScale.x.ToString());
				childLayerPieces.Add(childScale.y.ToString());
				childLayerPieces.Add(childScale.z.ToString());
			}

			string matNames = "[Default]";
			if (searchMats && child.transform.renderer != null) {
				foreach (Material childMaterial in child.transform.renderer.sharedMaterials) {
					childMaterials.Add(childMaterial.name);
				}
				matNames = childMaterials.JoinToString("|");
			}
			childLayerPieces.Add(matNames);
			childLayerPieces.Add(((int)behavior).ToString());
			//add all the pieces together and add a newline to the end
			childLayer = childLayer + (childLayerPieces.JoinToString(",\t") + "\n");
		}

		public static ChildPiece [] ExtractChildPiecesFromLayer(string layer)
		{
			//Debug.Log ("Exctracing layer " + layer);
			string[] splitLayer = layer.Split(new string [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			ChildPiece[] childPieces = new ChildPiece [splitLayer.Length];
			for (int i = 0; i < splitLayer.Length; i++) {
				ChildPiece childPiece = new ChildPiece();
				string[] splitChild = splitLayer[i].Split(new string [] { ",\t" }, StringSplitOptions.None);
				//0:packname, 1:childname, 2:xpos, 3:ypos, 4:zpos, 5:xrot, 6:yrot, 7:zrot, 8:xscl, 9:yscl, 10:zscl, 11:mat1|mat2|mat3\n\r
				childPiece.PackName = splitChild[0];
				childPiece.ChildName = splitChild[1];
				childPiece.Position = Vector3.zero;
				childPiece.Position.x = float.Parse(splitChild[2]);
				childPiece.Position.y = float.Parse(splitChild[3]);
				childPiece.Position.z = float.Parse(splitChild[4]);
				childPiece.Rotation = Vector3.zero;
				childPiece.Rotation.x = float.Parse(splitChild[5]);
				childPiece.Rotation.y = float.Parse(splitChild[6]);
				childPiece.Rotation.z = float.Parse(splitChild[7]);
				childPiece.Scale = Vector3.zero;
				childPiece.Scale.x = float.Parse(splitChild[8]);
				childPiece.Scale.y = float.Parse(splitChild[9]);
				childPiece.Scale.z = float.Parse(splitChild[10]);
				if (splitChild[11] == "[Default]") {
					childPiece.Materials = new string [1] { "[Default]" };
				} else if (splitChild[11].Contains("|")) {
					childPiece.Materials = splitChild[11].Split(new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
				} else {
					childPiece.Materials = new string [] { splitChild[11] };
				}
				//not all child pieces will have a behavior so check first
				if (splitChild.Length > 12) {
					childPiece.DestroyedBehavior = Int32.Parse(splitChild[12]);
				} else {
					childPiece.DestroyedBehavior = 0;//None
				}
				childPieces[i] = childPiece;
			}
			return childPieces;
		}

		public static LightPiece [] ExtractLightPiecesFromLayer(string layer)
		{
			string[] splitLayer = layer.Split(new string [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			LightPiece[] lightPieces = new LightPiece [splitLayer.Length];
			for (int i = 0; i < splitLayer.Length; i++) {
				LightPiece lightPiece = new LightPiece();
				string[] splitChild = splitLayer[i].Split(new string [] { ",\t" }, StringSplitOptions.RemoveEmptyEntries);
				//0:name, 1:type, 2:intensity, 3:colr, 4:colg, 5:colb, 6:range, 7:spotangle, 8:posx, 9:posy, 10:posz, 11:rotx, 12:roty, 13rotz
				lightPiece.LightName = splitChild[0];
				lightPiece.LightType = splitChild[1];
				lightPiece.LightIntensity	= float.Parse(splitChild[2]);
				lightPiece.LightColor = Color.white;
				lightPiece.LightColor.r = float.Parse(splitChild[3]);
				lightPiece.LightColor.g = float.Parse(splitChild[4]);
				lightPiece.LightColor.b = float.Parse(splitChild[5]);
				lightPiece.LightRange = float.Parse(splitChild[6]);
				lightPiece.LightSpotAngle	= float.Parse(splitChild[7]);
				lightPiece.Position = Vector3.zero;
				lightPiece.Position.x = float.Parse(splitChild[8]);
				lightPiece.Position.y = float.Parse(splitChild[9]);
				lightPiece.Position.z = float.Parse(splitChild[10]);
				lightPiece.Rotation = Vector3.zero;
				lightPiece.Rotation.x = float.Parse(splitChild[11]);
				lightPiece.Rotation.y = float.Parse(splitChild[12]);
				lightPiece.Rotation.z = float.Parse(splitChild[13]);

				lightPieces[i] = lightPiece;
			}
			return lightPieces;
		}

		public static FXPiece [] ExtractFXPiecesFromLayer(string layer)
		{
			string[] splitLayer = layer.Split(new string [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			FXPiece[] fxPieces = new FXPiece [splitLayer.Length];
			for (int i = 0; i < splitLayer.Length; i++) {
				FXPiece fxPiece = new FXPiece();
				string[] splitChild = splitLayer[i].Split(new string [] { ",\t" }, StringSplitOptions.RemoveEmptyEntries);
				//0:fxname, 1:soundname, 2:soundtype, 3:fxcolr, 4:fxcolg, 5:fxcolb, 6:delay, 7:duration, 8:posx, 9:posy, 10:posz, 11:rotx, 12:roty, 13:rotz
				fxPiece.FXName = splitChild[0];
				fxPiece.SoundName = splitChild[1];
				fxPiece.SoundType = (MasterAudio.SoundType)Enum.Parse(typeof(MasterAudio.SoundType), splitChild[2]);
				fxPiece.FXColor = Color.white;
				fxPiece.FXColor.r = float.Parse(splitChild[3]);
				fxPiece.FXColor.g = float.Parse(splitChild[4]);
				fxPiece.FXColor.b = float.Parse(splitChild[5]);
				fxPiece.Delay = float.Parse(splitChild[6]);
				fxPiece.Duration = float.Parse(splitChild[7]);
				fxPiece.Position = Vector3.zero;
				fxPiece.Position.x = float.Parse(splitChild[8]);
				fxPiece.Position.y = float.Parse(splitChild[9]);
				fxPiece.Position.z = float.Parse(splitChild[10]);
				fxPiece.Rotation = Vector3.zero;
				fxPiece.Rotation.x = float.Parse(splitChild[11]);
				fxPiece.Rotation.y = float.Parse(splitChild[12]);
				fxPiece.Rotation.z = float.Parse(splitChild[13]);
				fxPiece.Explosion = bool.Parse(splitChild[14]);
				fxPiece.JustForShow = bool.Parse(splitChild[15]);

				fxPieces[i] = fxPiece;
			}
			return fxPieces;
		}

		#endregion

	}
}
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
		#if UNITY_EDITOR
		public int NumInteriorCustomCollidersNormal = 0;
		public int NumInteriorCustomCollidersDestroyed = 0;
		public int NumExteriorCustomCollidersNormal = 0;
		public int NumExteriorCustomCollidersDestroyed = 0;
		public int NumInteriorStaticCollidersNormal = 0;
		public int NumInteriorStaticCollidersDestroyed = 0;
		public int NumExteriorStaticCollidersNormal = 0;
		public int NumExteriorStaticCollidersDestroyed = 0;

		public void LoadNextAvailabeTemplate ()
		{
			if (Application.isPlaying)
				return;

			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);

			string currentTemplateName = StructureBuilder.GetTemplateName (gameObject.name);
			List <string> availableTemplates = Mods.Get.Editor.Available ("Structure");
			int nextIndex = 0;
			for (int i = 0; i < availableTemplates.Count; i++) {
				if (availableTemplates [i] == currentTemplateName) {
					nextIndex = i + 1;
					break;
				}
			}
			if (nextIndex >= availableTemplates.Count) {
				nextIndex = 0;
			}

			string newTemplateName = availableTemplates [nextIndex];
			if (Mods.Get.Editor.LoadMod <StructureTemplate> (ref Template, "Structure", newTemplateName)) {
				gameObject.name = newTemplateName;
				EditorClearStructure (transform);
			}
		}

		public static void CreateStructureTransforms (Transform start)
		{
			Transform normal = null;
			Transform normalInt = null;
			Transform shingle = null;
			Transform signboard = null;

			normal = start.gameObject.FindOrCreateChild ("==NORMAL==");
			normalInt = normal.gameObject.FindOrCreateChild ("__INTERIOR");
			shingle = start.gameObject.FindOrCreateChild ("__SHINGLE");
			signboard = start.gameObject.FindOrCreateChild ("__SIGNBOARD");

			Transform variant = null;
			Transform normalIntFX = null;
			Transform normalIntDstFx = null;
			Transform normalIntDstFires = null;
			Transform normalIntCol = null;
			Transform normalIntColDst = null;
			Transform normalIntWIsGen = null;
			Transform normalIntWIsUnique = null;
			Transform normalIntWIsCats = null;
			Transform normalIntChrs = null;
			Transform normalIntDyn = null;
			Transform normalIntDynUnique = null;
			Transform normalIntStat = null;
			Transform normalIntStatDst = null;
			Transform normalIntSub = null;
			Transform normalIntWindows = null;
			Transform normalIntWindowsGen = null;
			Transform normalIntDoorsGen = null;
			Transform normalIntTriggers = null;
			Transform normalIntColliders = null;

			//arbitrarily set max interiors to 9
			//move this to a global constant
			for (int i = 0; i <= 9; i++) {
				variant = normalInt.gameObject.FindOrCreateChild ("=VARIANT_" + i.ToString () + "=");
				normalIntFX = variant.gameObject.FindOrCreateChild ("__FX");
				normalIntDstFx = variant.gameObject.FindOrCreateChild ("__DESTROYED_FX");
				normalIntDstFires = variant.gameObject.FindOrCreateChild ("__DESTROYED_FIRES");
				normalIntCol = variant.gameObject.FindOrCreateChild ("__COLLIDERS");
				normalIntColDst = variant.gameObject.FindOrCreateChild ("__COLLIDERS_DESTRUCTIBLE");
				normalIntWIsGen = variant.gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
				normalIntWindowsGen = variant.gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
				normalIntDoorsGen = variant.gameObject.FindOrCreateChild ("__DOORS_GENERIC");
				normalIntWIsUnique = variant.gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
				normalIntWIsCats = variant.gameObject.FindOrCreateChild ("__WORLDITEMS_CATS");
				normalIntChrs = variant.gameObject.FindOrCreateChild ("__ACTION_NODES");
				normalIntDyn = variant.gameObject.FindOrCreateChild ("__DYNAMIC");
				normalIntDynUnique = variant.gameObject.FindOrCreateChild ("__DYNAMIC_UNIQUE");
				normalIntStat = variant.gameObject.FindOrCreateChild ("__STATIC");
				normalIntStatDst = variant.gameObject.FindOrCreateChild ("__STATIC_DESTRUCTIBLE");
				normalIntSub = variant.gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
				normalIntWindows = variant.gameObject.FindOrCreateChild ("__WINDOWS");
				normalIntTriggers = variant.gameObject.FindOrCreateChild ("__TRIGGERS");

			}

			Transform normalExt = normal.gameObject.FindOrCreateChild ("__EXTERIOR");
			Transform normalExtFX = normalExt.gameObject.FindOrCreateChild ("__FX");
			Transform normalExtDstFx = normalExt.gameObject.FindOrCreateChild ("__DESTROYED_FX");
			Transform normalExtDstFires = normalExt.gameObject.FindOrCreateChild ("__DESTROYED_FIRES");
			Transform normalExtCol = normalExt.gameObject.FindOrCreateChild ("__COLLIDERS");
			Transform normalExtColDst = normalExt.gameObject.FindOrCreateChild ("__COLLIDERS_DESTRUCTIBLE");
			Transform normalExtWIsGen = normalExt.gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
			Transform normalExtWindowsGen	= normalExt.gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
			Transform normalExtDoorsGen = normalExt.gameObject.FindOrCreateChild ("__DOORS_GENERIC");
			Transform normalExtWIsUnique = normalExt.gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
			Transform normalExtWIsCats = normalExt.gameObject.FindOrCreateChild ("__WORLDITEMS_CATS");
			Transform normalExtChrs = normalExt.gameObject.FindOrCreateChild ("__ACTION_NODES");
			Transform normalExtDyn = normalExt.gameObject.FindOrCreateChild ("__DYNAMIC");
			Transform normalExtDynUnique = normalExt.gameObject.FindOrCreateChild ("__DYNAMIC_UNIQUE");
			Transform normalExtStat = normalExt.gameObject.FindOrCreateChild ("__STATIC");
			Transform normalExtStatDst = normalExt.gameObject.FindOrCreateChild ("__STATIC_DESTRUCTIBLE");
			Transform normalExtSub = normalExt.gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
			Transform normalExtWindows = normalExt.gameObject.FindOrCreateChild ("__WINDOWS");
			Transform normalExtTriggers = normalExt.gameObject.FindOrCreateChild ("__TRIGGERS");
		}

		public static void SaveEditorStructureToTemplate (Transform start)
		{
			if (!Manager.IsAwake <WorldItems> ()) {
				Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
			}
			if (!Manager.IsAwake <Structures> ()) {
				Manager.WakeUp <Structures> ("Frontiers_Structures");
			}
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);

			StructureTemplate newTemplate = new StructureTemplate ();
			Transform normal = start.gameObject.FindOrCreateChild ("==NORMAL==");
			Transform normalInt = normal.gameObject.FindOrCreateChild ("__INTERIOR");
			Transform normalExt = normal.gameObject.FindOrCreateChild ("__EXTERIOR");

			//NORMAL
			AddChildrenToStructureTemplateGroup (normalExt, newTemplate.Exterior);
			for (int i = 0; i <= 9; i++) {
				Transform variant = normalInt.gameObject.transform.FindChild ("=VARIANT_" + i.ToString () + "=");
				if (variant != null) {
					//Debug.Log ("Found variant " + i.ToString ());
					StructureTemplateGroup interiorVariant = new StructureTemplateGroup ();
					StructureTemplateGroupEditor editor = variant.gameObject.GetOrAdd <StructureTemplateGroupEditor> ();
					AddChildrenToStructureTemplateGroup (variant, interiorVariant);
					if (!interiorVariant.IsEmpty) {
						interiorVariant.Description = editor.Description;
						newTemplate.InteriorVariants.Add (interiorVariant);
						//Debug.Log ("Variant layers: " + interiorVariant.StaticStructureLayers.Count.ToString ());
					}
				}
			}

			//Debug.Log ("Total interior variants: " + newTemplate.InteriorVariants.Count.ToString ());
			//get the footprint and common offset
			GameObject commonOffset = start.gameObject.FindOrCreateChild ("__SHINGLE").gameObject;
			newTemplate.CommonShingleOffset = new SVector3 (commonOffset.transform.localPosition);

			GameObject commonSignboard = start.gameObject.FindOrCreateChild ("__SIGNBOARD").gameObject;
			newTemplate.CommonSignboardOffset.CopyFrom (commonSignboard.transform);

			GameObject footprint = start.gameObject.FindOrCreateChild ("__FOOTPRINT").gameObject;
			StructureFootprint[] footprints = footprint.transform.GetComponentsInChildren <StructureFootprint> (true);
			for (int i = 0; i < footprints.Length; i++) {
				newTemplate.Footprint.Add (new STransform (footprint.transform, true));
			}
			//Debug.Log ("Added " + newTemplate.Footprint.Count.ToString () + " Footprints");
			//Debug.Log ("Total interior variants: " + newTemplate.InteriorVariants.Count.ToString ());
			////Debug.Log ("Variant layers: " + newTemplate.InteriorVariants [0].StaticStructureLayers.Count.ToString ());
			Mods.Get.Editor.SaveMod <StructureTemplate> (newTemplate, "Structure", GetTemplateName (start.name));

			//Debug.Log ("Post save: Total interior variants: " + newTemplate.InteriorVariants.Count.ToString ());
			////Debug.Log ("Post save: Variant layers: " + newTemplate.InteriorVariants [0].StaticStructureLayers.Count.ToString ());
		}

		public static void AddChildrenToStructureTemplateGroup (Transform start, StructureTemplateGroup templateGroup)
		{
			Transform startFX = start.gameObject.FindOrCreateChild ("__FX");
			Transform startDstFx = start.gameObject.FindOrCreateChild ("__DESTROYED_FX");
			Transform startFires = start.gameObject.FindOrCreateChild ("__DESTROYED_FIRES");

			Transform startCol = start.gameObject.FindOrCreateChild ("__COLLIDERS");
			Transform startColDst = start.gameObject.FindOrCreateChild ("__COLLIDERS_DESTRUCTIBLE");

			Transform startWIsGen = start.gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
			Transform startWinGen = start.gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
			Transform startDoorGen = start.gameObject.FindOrCreateChild ("__DOORS_GENERIC");
			Transform startWisUnique = start.gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
			Transform startWisCats = start.gameObject.FindOrCreateChild ("__WORLDITEMS_CATS");
			Transform startNodes = start.gameObject.FindOrCreateChild ("__ACTION_NODES");
			Transform startDyn = start.gameObject.FindOrCreateChild ("__DYNAMIC");
			Transform startDynUnique = start.gameObject.FindOrCreateChild ("__DYNAMIC_UNIQUE");

			Transform startStat = start.gameObject.FindOrCreateChild ("__STATIC");
			Transform startStatDst = start.gameObject.FindOrCreateChild ("__STATIC_DESTRUCTIBLE");

			Transform startSub = start.gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
			Transform startWin = start.gameObject.FindOrCreateChild ("__WINDOWS");
			Transform startTriggers = start.gameObject.FindOrCreateChild ("__TRIGGERS");
			//move destroyed stuff to the proper location
			CheckDestroyedBehavior (startStat, startStatDst);
			CheckDestroyedBehavior (startCol, startColDst);

			//get placeholders from children
			WICategoryPlaceholder[] staticPlaceholders = startStat.GetComponentsInChildren <WICategoryPlaceholder> ();
			foreach (WICategoryPlaceholder staticPlaceholder in staticPlaceholders) {
				if (staticPlaceholder.SaveToStructure) {
					staticPlaceholder.SaveToStructure = false;
					UnityEditor.EditorUtility.SetDirty (staticPlaceholder);
					GameObject newPlaceholderGameObject = startWisCats.gameObject.CreateChild (staticPlaceholder.Item.WICategoryName).gameObject;
					newPlaceholderGameObject.transform.position = staticPlaceholder.transform.position;
					newPlaceholderGameObject.transform.rotation = staticPlaceholder.transform.rotation;
					WICategoryPlaceholder newPlaceholder = newPlaceholderGameObject.AddComponent <WICategoryPlaceholder> ();
					newPlaceholder.Item = ObjectClone.Clone <WICatItem> (staticPlaceholder.Item);
				}
			}

			StructureTemplate.AddStaticChildrenToStructureTemplate (startStat, templateGroup.StaticStructureLayers);
			StructureTemplate.AddStaticChildrenToStructureTemplate (startStatDst, templateGroup.StaticStructureLayers);

			StructureTemplate.AddCustomCollidersToStructureTemplate (startCol, templateGroup.CustomStructureColliders);
			StructureTemplate.AddCustomCollidersToStructureTemplate (startColDst, templateGroup.CustomStructureColliders);

			StructureTemplate.AddStaticCollidersToStructureTemplate (startStat, templateGroup.StaticStructureColliders, null);
			StructureTemplate.AddStaticCollidersToStructureTemplate (startStatDst, templateGroup.StaticStructureColliders, null);

			StructureTemplate.AddLightsToStructureTemplate (startFX, ref templateGroup.GenericLights);
			StructureTemplate.AddFXToStructureTemplate (startDstFx, ref templateGroup.DestroyedFX);

			StructureTemplate.AddActionNodesToTemplate (startNodes, templateGroup.ActionNodes);
			StructureTemplate.AddFiresToStructureTemplate (startFires, ref templateGroup.DestroyedFires);
			StructureTemplate.AddGenericWorldItemsToStructureTemplate (startWIsGen, ref templateGroup.GenericWItems);
			StructureTemplate.AddGenericDynamicToStructureTemplate (startWinGen, ref templateGroup.GenericWindows);
			StructureTemplate.AddGenericDynamicToStructureTemplate (startDoorGen, ref templateGroup.GenericDoors);
			StructureTemplate.AddGenericDynamicToStructureTemplate (startDyn, ref templateGroup.GenericDynamic);
			StructureTemplate.AddUniqueDynamicToStructureTemplate (startDynUnique, templateGroup.UniqueDynamic);
			StructureTemplate.AddUniqueWorldItemsToStructureTemplate (startWisUnique, templateGroup.UniqueWorlditems);
			StructureTemplate.AddCatWorldItemsToStructureTemplate (startWisCats, templateGroup.CategoryWorldItems);
			StructureTemplate.AddTriggersToStructureTemplate (startTriggers, templateGroup.Triggers);
		}

		public static void CheckDestroyedBehavior (Transform normal, Transform destroyed)
		{
			List <Transform> childrenToMoveToDestroyed = new List<Transform> ();
			List <Transform> childrenToMoveToNormal = new List<Transform> ();
			foreach (Transform child in normal) {
				StructureDestroyResult str = null;
				if (child.gameObject.HasComponent <StructureDestroyResult> (out str)) {
					if (str.Behavior != StructureDestroyedBehavior.None) {
						childrenToMoveToDestroyed.Add (child);
					}
				}
			}
			foreach (Transform child in destroyed) {
				StructureDestroyResult str = null;
				if (!child.gameObject.HasComponent <StructureDestroyResult> (out str) || str.Behavior == StructureDestroyedBehavior.None) {
					childrenToMoveToNormal.Add (child);
				}
			}
			foreach (Transform childToMove in childrenToMoveToDestroyed) {
				Debug.Log ("Moving " + childToMove.name + " to destroyed");
				childToMove.parent = destroyed;
			}
			foreach (Transform childToMove in childrenToMoveToNormal) {
				Debug.Log ("Moving " + childToMove.name + " to normal");
				childToMove.parent = normal;
			}
			childrenToMoveToDestroyed.Clear ();
			childrenToMoveToNormal.Clear ();
		}

		public static void CreateTemporaryPrefab (StructureBuilder builder, Transform start, string templateName)
		{
			StructureTemplate template = null;
			if (GameManager.Get != null && GameManager.Get.TestingEnvironment) {
				if (!Mods.Get.Runtime.LoadMod <StructureTemplate> (ref template, "Structure", templateName)) {
					////Debug.Log ("Couldn't load " + templateName);
					return;
				}
			} else {
				if (Application.isPlaying)
					return;

				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				if (!Manager.IsAwake <Structures> ()) {
					Manager.WakeUp <Structures> ("Frontiers_Structures");
				}
				Structures.Get.Initialize ();

				if (!Manager.IsAwake <WorldItems> ()) {
					Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
				}
				WorldItems.Get.Initialize ();

				if (!Mods.Get.Editor.LoadMod <StructureTemplate> (ref template, "Structure", templateName)) {
					////Debug.Log ("Couldn't load " + templateName);
					return;
				}
			}

			Transform normal = start.gameObject.FindOrCreateChild ("==NORMAL==");
			Transform shingle = start.gameObject.FindOrCreateChild ("__SHINGLE");
			Transform signboard = start.gameObject.FindOrCreateChild ("__SIGNBOARD");

			shingle.localPosition = template.CommonShingleOffset;
			template.CommonSignboardOffset.ApplyTo (signboard, false);

			if (normal.childCount > 0) {
				//Debug.Log ("Already built!");
				return;
			}

			Transform normalInt = normal.gameObject.FindOrCreateChild ("__INTERIOR");
			Transform normalExt = normal.gameObject.FindOrCreateChild ("__EXTERIOR");

			normalExt.gameObject.SetActive (true);
			normalInt.gameObject.SetActive (true);

			builder.NumInteriorCustomCollidersNormal = 0;
			builder.NumInteriorCustomCollidersDestroyed = 0;
			builder.NumExteriorCustomCollidersNormal = 0;
			builder.NumExteriorCustomCollidersDestroyed = 0;

			builder.NumInteriorStaticCollidersNormal = 0;
			builder.NumInteriorStaticCollidersDestroyed = 0;
			builder.NumExteriorStaticCollidersNormal = 0;
			builder.NumExteriorStaticCollidersDestroyed = 0;

			CreateTemporaryPrefabs (template.Exterior, normalExt, ref builder.NumExteriorStaticCollidersNormal, ref builder.NumExteriorStaticCollidersDestroyed, ref builder.NumExteriorCustomCollidersNormal, ref builder.NumExteriorCustomCollidersDestroyed);

			for (int i = 0; i < template.InteriorVariants.Count; i++) {
				////Debug.Log ("Creating variant " + i.ToString ());
				Transform variant = normalInt.gameObject.FindOrCreateChild ("=VARIANT_" + i.ToString () + "=");
				StructureTemplateGroupEditor editor = variant.gameObject.GetOrAdd <StructureTemplateGroupEditor> ();
				editor.Description = template.InteriorVariants [i].Description;
				CreateTemporaryPrefabs (template.InteriorVariants [i], variant, ref builder.NumInteriorStaticCollidersNormal, ref builder.NumInteriorStaticCollidersDestroyed, ref builder.NumInteriorCustomCollidersNormal, ref builder.NumInteriorCustomCollidersDestroyed);
			}
		}

		public static void ToggleDestroyedPrefabs ()
		{
	//			Transform normal = gameObject.FindOrCreateChild ("==NORMAL==");
	//			Transform destroyed = gameObject.FindOrCreateChild ("==DESTROYED==");
	//			Transform normalInt = normal.gameObject.FindOrCreateChild ("__INTERIOR");
	//			Transform normalExt = normal.gameObject.FindOrCreateChild ("__EXTERIOR");
	//			Transform destroyedExt = destroyed.gameObject.FindOrCreateChild ("__EXTERIOR");
	//
	//			bool destroy = !destroyedExt.gameObject.activeSelf;
	//
	//			StructureDestroyResult[] strExtList = normalExt.GetComponentsInChildren <StructureDestroyResult> (true);
	//			StructureDestroyResult[] strIntList = normalInt.GetComponentsInChildren <StructureDestroyResult> (true);
	//
	//			foreach (StructureDestroyResult strExt in strExtList) {
	//				if (Flags.Check <StructureDestroyedBehavior> (strExt.Behavior, StructureDestroyedBehavior.Destroy, Flags.CheckType.MatchAny)) {
	//					strExt.gameObject.SetActive (!destroy);
	//				}
	//			}
	//
	//			foreach (StructureDestroyResult strInt in strIntList) {
	//				if (Flags.Check <StructureDestroyedBehavior> (strInt.Behavior, StructureDestroyedBehavior.Destroy, Flags.CheckType.MatchAny)) {
	//					strInt.gameObject.SetActive (!destroy);
	//				}
	//			}
	//
	//			destroyedExt.gameObject.SetActive (destroy);
		}

		public static void EditorCopyStaticColliders (Transform start)
		{
			Transform normal = start.gameObject.FindOrCreateChild ("==NORMAL==");
			Transform normalInt = normal.gameObject.FindOrCreateChild ("__INTERIOR");
			Transform normalExt = normal.gameObject.FindOrCreateChild ("__EXTERIOR");

			for (int i = 0; i <= 9; i++) {
				Transform variant = normalInt.FindChild ("=VARIANT_" + i.ToString () + "=");
				if (variant != null) {
					Transform normalIntStat = variant.gameObject.FindOrCreateChild ("__STATIC");
					Transform normalIntCol = variant.gameObject.FindOrCreateChild ("__COLLIDERS");

					EditorCopyStaticColliders (normalIntStat, normalIntCol);
				}
			}

			Transform normalExtStat = normalExt.gameObject.FindOrCreateChild ("__STATIC");
			Transform normalExtCol = normalExt.gameObject.FindOrCreateChild ("__COLLIDERS");

			EditorCopyStaticColliders (normalExtStat, normalExtCol);
		}

		public static void EditorCopyStaticColliders (Transform lookIn, Transform copyTo)
		{
			BoxCollider [] colliders = lookIn.GetComponentsInChildren <BoxCollider> ();
			foreach (BoxCollider collider in colliders) {
				if (collider.renderer == null && collider.name == "Collider") {
					GameObject newCollider = copyTo.gameObject.CreateChild ("Collider").gameObject;
					newCollider.transform.position = collider.transform.position;
					newCollider.transform.rotation = collider.transform.rotation;
					newCollider.transform.localScale = collider.transform.localScale;
					newCollider.AddComponent <BoxCollider> ();
					newCollider.layer = Globals.LayerNumStructureCustomCollider;
					newCollider.tag = collider.tag;
				}
			}
		}

		public static void EditorConvertColliders ( )
		{
			for (int i = 0; i < UnityEditor.Selection.gameObjects.Length; i++) {
				GameObject existingCollider = UnityEditor.Selection.gameObjects [i];
				BoxCollider existingColliderCollider = existingCollider.GetComponent <BoxCollider> ();
				existingCollider.transform.localScale = existingColliderCollider.size;
				existingCollider.transform.Translate (existingColliderCollider.center);
				existingColliderCollider.size = Vector3.one;
				existingColliderCollider.center = Vector3.zero;
			}
		}

		public static void EditorCreateCollider (string colliderName, Color colliderColor) {
			GameObject newCollider = new GameObject (colliderName);
			Bounds bounds = new Bounds ();
			Transform parentObject = null;
			bool foundFirst = false;
			for (int i = 0; i < UnityEditor.Selection.gameObjects.Length; i++) {
				GameObject existingCollider = UnityEditor.Selection.gameObjects [i];
				Collider existingColliderCollider = existingCollider.collider;
				existingCollider.layer = Globals.LayerNumStructureIgnoreCollider;
				if (!foundFirst) {
					bool destroyAfterUse = false;
					parentObject = existingCollider.transform.parent.parent.FindChild ("__COLLIDERS");
					foundFirst = true;
					//newCollider.transform.rotation = existingCollider.transform.rotation;
					if (existingColliderCollider == null) {
						existingColliderCollider = existingCollider.AddComponent <BoxCollider> ();
						destroyAfterUse = true;
					}
					bounds.center = existingColliderCollider.bounds.center;
					bounds.Encapsulate (existingColliderCollider.bounds);
					if (destroyAfterUse) {
						GameObject.DestroyImmediate (existingColliderCollider);
					}
				} else if (existingColliderCollider != null) {
					bounds.Encapsulate (existingColliderCollider.bounds);
				} else {
					existingColliderCollider = existingCollider.AddComponent <BoxCollider> ();
					bounds.Encapsulate (existingColliderCollider.bounds);
					GameObject.DestroyImmediate (existingColliderCollider);
				}
			}
			newCollider.transform.parent = parentObject;
			newCollider.transform.position = bounds.center;
			newCollider.transform.localScale = bounds.size;
			BoxCollider bc = newCollider.AddComponent <BoxCollider> ();

			CrucialColliderGizmo ccz = newCollider.GetOrAdd <CrucialColliderGizmo> ();
			ccz.fillColor = Colors.Alpha (colliderColor, 0.5f);
			ccz.wireColor = colliderColor;
		}

		public static void CreateTemporaryPrefabs (StructureTemplateGroup templateGroup, Transform start, ref int staticCollidersNormal, ref int staticCollidersDestroyed, ref int customCollidersNormal, ref int customCollidersDestroyed)
		{
			Transform startFX = start.gameObject.FindOrCreateChild ("__FX");
			Transform startDstFx = start.gameObject.FindOrCreateChild ("__DESTROYED_FX");
			Transform startCol = start.gameObject.FindOrCreateChild ("__COLLIDERS");
			Transform startColDst = start.gameObject.FindOrCreateChild ("__COLLIDERS_DESTRUCTIBLE");
			Transform startWIsGen = start.gameObject.FindOrCreateChild ("__WORLDITEMS_GENERIC");
			Transform startWinGen = start.gameObject.FindOrCreateChild ("__WINDOWS_GENERIC");
			Transform startDoorGen = start.gameObject.FindOrCreateChild ("__DOORS_GENERIC");
			Transform startWisUnique = start.gameObject.FindOrCreateChild ("__WORLDITEMS_UNIQUE");
			Transform startWisCats = start.gameObject.FindOrCreateChild ("__WORLDITEMS_CATS");
			Transform startNodes = start.gameObject.FindOrCreateChild ("__ACTION_NODES");
			Transform startDyn = start.gameObject.FindOrCreateChild ("__DYNAMIC");
			Transform startDynUnique = start.gameObject.FindOrCreateChild ("__DYNAMIC_UNIQUE");
			Transform startStat = start.gameObject.FindOrCreateChild ("__STATIC");
			Transform startStatDst = start.gameObject.FindOrCreateChild ("__STATIC_DESTRUCTIBLE");
			Transform startSub = start.gameObject.FindOrCreateChild ("__SUBSTRUCTURES");
			Transform startWin = start.gameObject.FindOrCreateChild ("__WINDOWS");
			Transform startFires = start.gameObject.FindOrCreateChild ("__DESTROYED_FIRES");
			Transform startTriggers = start.gameObject.FindOrCreateChild ("__TRIGGERS");

			foreach (KeyValuePair <string, KeyValuePair <string,string>> triggerStatePair in templateGroup.Triggers) {
				string triggerName = triggerStatePair.Key;
				string triggerScriptName = triggerStatePair.Value.Key;
				string triggerState = triggerStatePair.Value.Value;

				GameObject newTriggerObject = startTriggers.gameObject.CreateChild (triggerName).gameObject;
				WorldTrigger worldTriggerScript	= newTriggerObject.AddComponent (triggerScriptName) as WorldTrigger;
				worldTriggerScript.UpdateTriggerState (triggerState, null);
			}

			foreach (StructureLayer staticLayer in templateGroup.StaticStructureLayers) {
				StructurePackPrefab prefab = null;
				if (Structures.Get.PackStaticPrefab (staticLayer.PackName, staticLayer.PrefabName, out prefab)) {
					ChildPiece[] staticPieces = StructureTemplate.ExtractChildPiecesFromLayer (staticLayer.Instances);
					for (int i = 0; i < staticPieces.Length; i++) {
						ChildPiece piece = staticPieces [i];
						GameObject instantiatedPrefab = UnityEditor.PrefabUtility.InstantiatePrefab (prefab.Prefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedPrefab.name = prefab.Prefab.name;
						instantiatedPrefab.transform.parent = startStat;
						instantiatedPrefab.tag = staticLayer.Tag;
						instantiatedPrefab.layer = staticLayer.Layer;
						if (instantiatedPrefab.layer != Globals.LayerNumStructureIgnoreCollider) {
							instantiatedPrefab.layer = Globals.LayerNumStructureCustomCollider;
						}
						//put it in the right place
						instantiatedPrefab.transform.localPosition = piece.Position;
						instantiatedPrefab.transform.localRotation = Quaternion.identity;
						instantiatedPrefab.transform.Rotate (piece.Rotation);
						instantiatedPrefab.transform.localScale = piece.Scale;

						if (staticLayer.DestroyedBehavior != 0) {
							Debug.Log ("Destroyed behavior was not zero, adding sdb");
							instantiatedPrefab.AddComponent <StructureDestroyResult> ().Behavior = (StructureDestroyedBehavior) staticLayer.DestroyedBehavior;
						}

						Material[] variationsArray = null;
						if (staticLayer.Substitutions != null && staticLayer.Substitutions.Count > 0) {
							MeshRenderer pmr = prefab.MRenderer;
							variationsArray = pmr.sharedMaterials;
							string newMaterialName = string.Empty;
							for (int j = 0; j < variationsArray.Length; j++) {
								if (staticLayer.Substitutions.TryGetValue (variationsArray [j].name, out newMaterialName)) {
									Material sharedMaterial = null;
									if (Structures.Get.SharedMaterial (newMaterialName, out sharedMaterial)) {
										variationsArray [j] = sharedMaterial;
									}
								}
							}
							instantiatedPrefab.renderer.materials = variationsArray;
						}
					}
				}
			}

			foreach (StructureLayer colliderLayer in templateGroup.CustomStructureColliders) {
				ChildPiece[] customColliders = StructureTemplate.ExtractChildPiecesFromLayer (colliderLayer.Instances);
				for (int i = 0; i < customColliders.Length; i++) {
					ChildPiece piece = customColliders [i];
					GameObject customColliderObject = startCol.gameObject.CreateChild (colliderLayer.PrefabName).gameObject;
					customColliderObject.tag = colliderLayer.Tag;
					customColliderObject.layer = colliderLayer.Layer;

					Color colliderColor = Color.red;

					if (piece.DestroyedBehavior != 0) {
						customCollidersDestroyed++;
						customColliderObject.GetOrAdd <StructureDestroyResult> ().Behavior = (StructureDestroyedBehavior)piece.DestroyedBehavior;
					} else {
						colliderColor = Color.green;
						customCollidersNormal++;
					}

					customColliderObject.AddComponent (colliderLayer.PackName);
					customColliderObject.transform.localPosition = piece.Position;
					customColliderObject.transform.localRotation = Quaternion.Euler (piece.Rotation);
					customColliderObject.transform.localScale = piece.Scale;

					CrucialColliderGizmo ccg = customColliderObject.AddComponent <CrucialColliderGizmo> ();
					Color gizmoColor = Colors.Saturate (Colors.ColorFromString (piece.ChildName, 100));
					ccg.fillColor = Colors.Alpha (colliderColor, 0.5f);
					ccg.wireColor = gizmoColor;
					ccg.centerColor = gizmoColor;
				}
			}

			foreach (StructureLayer colliderLayer in templateGroup.StaticStructureColliders) {
				ChildPiece[] customColliders = StructureTemplate.ExtractChildPiecesFromLayer (colliderLayer.Instances);
				for (int i = 0; i < customColliders.Length; i++) {
					ChildPiece piece = customColliders [i];
//					GameObject customColliderObject = startCol.gameObject.CreateChild (colliderLayer.PrefabName).gameObject;
//					customColliderObject.tag = colliderLayer.Tag;
//					customColliderObject.layer = colliderLayer.Layer;

					if (piece.DestroyedBehavior > 0) {
						staticCollidersDestroyed++;
//						customColliderObject.AddComponent <StructureDestroyResult> ().Behavior = (StructureDestroyedBehavior) piece.DestroyedBehavior;
					} else {
						staticCollidersNormal++;
					}

//					customColliderObject.AddComponent (colliderLayer.PackName);
//					customColliderObject.transform.localPosition = piece.Position;
//					customColliderObject.transform.localRotation = Quaternion.Euler (piece.Rotation);
//					customColliderObject.transform.localScale = piece.Scale;
//
//					CrucialColliderGizmo ccg = customColliderObject.AddComponent <CrucialColliderGizmo> ();
//					Color gizmoColor = Colors.Saturate (Colors.ColorFromString (piece.ChildName, 100));
//					ccg.fillColor = Colors.Alpha (gizmoColor, 0.5f);
//					ccg.wireColor = gizmoColor;
//					ccg.centerColor = gizmoColor;
				}
			}

			ChildPiece[] genericDoors = StructureTemplate.ExtractChildPiecesFromLayer (templateGroup.GenericDoors);
			for (int i = 0; i < genericDoors.Length; i++) {
				ChildPiece piece = genericDoors [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (piece.PackName, piece.ChildName, out dynamicPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (dynamicPrefab.gameObject);
					if (basePrefab != null) {
						GameObject instantiatedGenDoor = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedGenDoor.name = basePrefab.name;
						instantiatedGenDoor.transform.parent = startDoorGen;
						//put it in the right place
						instantiatedGenDoor.transform.localPosition = piece.Position;
						instantiatedGenDoor.transform.localRotation = Quaternion.identity;
						instantiatedGenDoor.transform.Rotate (piece.Rotation);
					}
				} 

			}

			ChildPiece[] genericWindows = StructureTemplate.ExtractChildPiecesFromLayer (templateGroup.GenericWindows);
			for (int i = 0; i < genericWindows.Length; i++) {
				ChildPiece piece = genericWindows [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (piece.PackName, piece.ChildName, out dynamicPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (dynamicPrefab.gameObject);
					if (basePrefab != null) {
						GameObject instantiatedGenWin = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedGenWin.name = basePrefab.name;
						instantiatedGenWin.transform.parent = startWinGen;
						//put it in the right place
						instantiatedGenWin.transform.localPosition = piece.Position;
						instantiatedGenWin.transform.localRotation = Quaternion.identity;
						instantiatedGenWin.transform.Rotate (piece.Rotation);
					}
				}

			}

			ChildPiece[] genericDynamic = StructureTemplate.ExtractChildPiecesFromLayer (templateGroup.GenericDynamic);
			for (int i = 0; i < genericDynamic.Length; i++) {
				ChildPiece piece = genericDynamic [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (piece.PackName, piece.ChildName, out dynamicPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (dynamicPrefab.gameObject);
					if (basePrefab != null) {
						GameObject instantiatedGenDyn = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedGenDyn.name = basePrefab.name;
						instantiatedGenDyn.transform.parent = startDyn;
						//put it in the right place
						instantiatedGenDyn.transform.localPosition = piece.Position;
						instantiatedGenDyn.transform.localRotation = Quaternion.identity;
						instantiatedGenDyn.transform.Rotate (piece.Rotation);
					}
				} 

			}

			for (int i = 0; i < templateGroup.UniqueDynamic.Count; i++) {
				StackItem piece = templateGroup.UniqueDynamic [i];
				DynamicPrefab dynamicPrefab = null;
				if (Structures.Get.PackDynamicPrefab (piece.PackName, piece.PrefabName, out dynamicPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (dynamicPrefab.gameObject);
					if (basePrefab != null) {
						GameObject instantiatedUniqueDyn = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedUniqueDyn.name = basePrefab.name;
						instantiatedUniqueDyn.transform.parent = startDynUnique;
						//put it in the right place
						instantiatedUniqueDyn.transform.localPosition = piece.Transform.Position;
						instantiatedUniqueDyn.transform.localRotation = Quaternion.identity;
						instantiatedUniqueDyn.transform.Rotate (piece.Transform.Rotation);

						WorldItem worlditem = null;
						DynamicPrefab dynPre = instantiatedUniqueDyn.GetComponent <DynamicPrefab> ();
						worlditem = dynPre.worlditem;
						worlditem.IsTemplate = false;
						worlditem.ReceiveState (piece);
					}
				} 

			}

			GameObject lastInstantied = null;

			ChildPiece[] genericWorldItems = StructureTemplate.ExtractChildPiecesFromLayer (templateGroup.GenericWItems);
			for (int i = 0; i < genericWorldItems.Length; i++) {
				ChildPiece piece = genericWorldItems [i];
				WorldItem packPrefab = null;
				if (WorldItems.Get.PackPrefab (piece.PackName, piece.ChildName, out packPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (packPrefab.gameObject);
					if (basePrefab != null) {
						GameObject instantiatedGenWi = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedGenWi.name = basePrefab.name;
						instantiatedGenWi.transform.parent = startWIsGen;
						////Debug.Log ("Instantiated prefab " + instantiatedGenWi.name + " has been parent under " + startWIsGen.name);
						//put it in the right place
						instantiatedGenWi.transform.localPosition = piece.Position;
						instantiatedGenWi.transform.localRotation = Quaternion.identity;
						instantiatedGenWi.transform.Rotate (piece.Rotation);
						instantiatedGenWi.transform.localScale = basePrefab.transform.localScale;
						////Debug.Log ("Instanitated prefab position is " + instantiatedGenWi.transform.localPosition.ToString () + ", rotation is " + instantiatedGenWi.transform.localRotation.eulerAngles.ToString ( ));
						////Debug.Log ("Instantiated prefab's parent is " + instantiatedGenWi.transform.parent.name);
						lastInstantied = instantiatedGenWi.gameObject;
						WorldItem instantiatedWorldItem = instantiatedGenWi.GetComponent <WorldItem> ();
						instantiatedWorldItem.IsTemplate = false;
					}
				}
			}

			ChildPiece[] fires = StructureTemplate.ExtractChildPiecesFromLayer (templateGroup.DestroyedFires);
			for (int i = 0; i < fires.Length; i++) {
				ChildPiece fire = fires [i];
				Transform newFire = null;
				if (FXManager.Get != null) {
					for (int j = 0; j < FXManager.Get.FirePrefabs.Count; j++) {
						if (FXManager.Get.FirePrefabs [j].name == fire.ChildName) {
							GameObject newFireGo = GameObject.Instantiate (FXManager.Get.FirePrefabs [i]) as GameObject;
							newFire = newFireGo.transform;
							newFire.parent = startFires;
						}
					}
				} else {
					newFire = startFires.gameObject.CreateChild (fire.ChildName);
				}
				newFire.name = fire.ChildName;
				newFire.localPosition = fire.Position;
				newFire.localRotation = Quaternion.Euler (fire.Rotation);
				newFire.localScale = fire.Scale;
				newFire.gameObject.tag = "Fire";
			}

			FXPiece[] fxPieces = StructureTemplate.ExtractFXPiecesFromLayer (templateGroup.DestroyedFX);
			for (int i = 0; i < fxPieces.Length; i++) {
				FXPiece piece = fxPieces [i];
				GameObject fxPieceTemplateGameObject = startDstFx.gameObject.CreateChild (piece.FXName).gameObject;
				FXPieceTemplate fxPieceTemplate = fxPieceTemplateGameObject.AddComponent <FXPieceTemplate> ();
				fxPieceTemplate.Initialize (piece);
			}

			foreach (StackItem stackItem in templateGroup.UniqueWorlditems) {
				WorldItem packPrefab = null;
				if (WorldItems.Get.PackPrefab (stackItem.PackName, stackItem.PrefabName, out packPrefab)) {
					GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot (packPrefab.gameObject);
					if (basePrefab != null) {
						Debug.Log ("Creating non-generic world item " + basePrefab.name);
						GameObject instantiatedUnWi = UnityEditor.PrefabUtility.InstantiatePrefab (basePrefab) as GameObject;
						//instantiate a new prefab - keep it as a prefab!
						instantiatedUnWi.name = basePrefab.name;
						instantiatedUnWi.transform.parent = startWisUnique;
						//put it in the right place
						stackItem.Props.Local.Transform.ApplyTo (instantiatedUnWi.transform, false);
						//instantiatedUnWi.transform.localScale = Vector3.one * packPrefab.Props.Global.ScaleModifier;

						WorldItem instantiatedWorldItem = instantiatedUnWi.GetComponent <WorldItem> ();
						instantiatedWorldItem.IsTemplate = false;
						instantiatedWorldItem.Props.Global = packPrefab.Props.Global;
						instantiatedWorldItem.ReceiveState (stackItem);
					} else {

					}
				}

			}

			foreach (WICatItem catItem in templateGroup.CategoryWorldItems) {
				//Debug.Log ("added cat item " + catItem.WICategoryName);
				GameObject catItemGameObject = startWisCats.gameObject.CreateChild (catItem.WICategoryName).gameObject;
				WICategoryPlaceholder wiCatPlaceHolder = catItemGameObject.AddComponent <WICategoryPlaceholder> ();
				catItem.Transform.ApplyTo (catItemGameObject.transform);
				wiCatPlaceHolder.Item = catItem;
			}

			LightPiece[] genericLights = StructureTemplate.ExtractLightPiecesFromLayer (templateGroup.GenericLights);
			for (int i = 0; i < genericLights.Length; i++) {
				StructureTemplate.LightFromLightPiece (genericLights [i], startFX);
			}

			foreach (ActionNodeState actionNodeState in templateGroup.ActionNodes) {
				GameObject newActionNode = new GameObject (actionNodeState.FullName);
				newActionNode.transform.parent = startNodes;
				ActionNode actionNode = newActionNode.AddComponent <ActionNode> ();
				actionNode.State = actionNodeState;
				actionNodeState.Transform.ApplyTo (newActionNode.transform, false);
				actionNode.Refresh ();
			}

			//move all static, FX and colliders to the proper area
			//CheckDestroyedBehavior (startFX, startDstFx);
			CheckDestroyedBehavior (startStat, startStatDst);
			CheckDestroyedBehavior (startCol, startColDst);
		}

		public static void EditorClearStructure (Transform start)
		{
			Transform destroyed = start.gameObject.FindOrCreateChild ("==DESTROYED==");
			Transform normal = start.gameObject.FindOrCreateChild ("==NORMAL==");
			Transform footprint = start.gameObject.FindOrCreateChild ("__FOOTPRINT");
			Transform shingle = start.gameObject.FindOrCreateChild ("__SHINGLE");
			GameObject.DestroyImmediate (destroyed.gameObject);
			GameObject.DestroyImmediate (normal.gameObject);
			GameObject.DestroyImmediate (footprint.gameObject);
			GameObject.DestroyImmediate (shingle.gameObject);
		}
		#endif
	}

}
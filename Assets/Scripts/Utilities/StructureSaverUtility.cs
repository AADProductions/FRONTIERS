using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System.Collections.Generic;

[ExecuteInEditMode]
public class StructureSaverUtility : MonoBehaviour {

	public string StartStructure;
	public string EndStructure;
	public StructureBuilder builder;
	public int NumStructuresSaved = 0;
	public bool FinishedSavingStructures = false;

	// Use this for initialization
	void OnEnable () {
//		if (string.IsNullOrEmpty (StartStructure) || string.IsNullOrEmpty (EndStructure)) {
//			enabled = false;
//			return;
//		}
//
//		NumStructuresSaved = 0;
//		FinishedSavingStructures = false;
//		builder.name = StartStructure;
//		RunEditorCoroutine ();
		if (!Manager.IsAwake <Mods> ()) {
			Manager.WakeUp <Mods> ("__MODS");
		}
		Mods.Get.Editor.InitializeEditor (true);

		List <string> structuresToSave = Mods.Get.Editor.Available ("Structure");
		StructureTemplate currentTemplate = null;
		foreach (string structureToSave in structuresToSave) {
			if (Mods.Get.Editor.LoadMod <StructureTemplate> (ref currentTemplate, "Structure", structureToSave)) {
//				SaveStackItems (currentTemplate.Exterior.UniqueDoors);
//				SaveStackItems (currentTemplate.Exterior.UniqueDynamic);
//				SaveStackItems (currentTemplate.Exterior.UniqueWindows);
//				SaveStackItems (currentTemplate.Exterior.UniqueWorlditems);
//				foreach (StructureTemplateGroup intGroup in currentTemplate.InteriorVariants) {
//					SaveStackItems (intGroup.UniqueDoors);
//					SaveStackItems (intGroup.UniqueDynamic);
//					SaveStackItems (intGroup.UniqueWindows);
//					SaveStackItems (intGroup.UniqueWorlditems);
//				}
				Mods.Get.Editor.SaveMod <StructureTemplate> (currentTemplate, "Structure", structureToSave);
			}
		}
	}
//
//	protected void SaveStackItems (List <StackItem> items) {
//		foreach (StackItem item in items) {
//			item.CopyFromSaveState ();
//		}
//	}
//
//	protected void SaveStackItems (List <DynamicStructureTemplatePiece> items) {
//		foreach (DynamicStructureTemplatePiece item in items) {
//			item.CopyFromSaveState ();
//		}
//	}

//	void OnDisable ( ) {
//		FinishedSavingStructures = true;
//	}

//	protected void RunEditorCoroutine ( ) {
//		IEnumerator e = SaveStructuresOverTime ();
//		while (e.MoveNext ()) {
//
//		}
////	}
//	
//	protected IEnumerator SaveStructuresOverTime ( ) {
//		while (!FinishedSavingStructures) {
//			builder.DuplicateAsMeshStructure ();
//			yield return null;
//			yield return null;
//			StructureBuilder.SaveEditorStructureToTemplate (builder.transform);
//			NumStructuresSaved++;
//			builder.LoadNextAvailabeTemplate ();
//			yield return null;
//			yield return null;
//			if (builder.name == EndStructure) {
//				FinishedSavingStructures = true;
//				yield break;
//			}
//			//yes do this every frame
//			Resources.UnloadUnusedAssets ();
//		}
//		yield break;
//	}
//
//	protected bool mSavingStructures = false;
}

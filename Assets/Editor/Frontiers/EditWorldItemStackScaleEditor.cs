using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(EditWorldItemStackScale))]
public class EditWorldItemStackScaleEditor : Editor
{
	protected EditWorldItemStackScale targetEditor;

	public void Awake ( )
	{
		targetEditor = (EditWorldItemStackScale) target;
	}

	public override void OnInspectorGUI ( )
	{
		if (!Manager.IsAwake <WorldItems> ( ))
		{
			Manager.WakeUp <WorldItems> ("Frontiers_WorldItems");
		}

		EditorStyles.textField.wordWrap 			= true;
		EditorStyles.label.wordWrap 				= true;
		EditorStyles.miniLabel.wordWrap 			= true;
		EditorStyles.miniTextField.wordWrap 		= true;
		EditorStyles.whiteMiniLabel.wordWrap	 	= true;
		EditorStyles.miniButton.wordWrap 			= true; 

		EditorStyles.textField.fontStyle 			= FontStyle.Normal;
		EditorStyles.label.fontStyle 				= FontStyle.Normal;
		EditorStyles.miniLabel.fontStyle 			= FontStyle.Normal;
		EditorStyles.miniTextField.fontStyle 		= FontStyle.Normal;
		EditorStyles.whiteMiniLabel.fontStyle	 	= FontStyle.Normal;
		EditorStyles.miniButton.fontStyle		 	= FontStyle.Normal;

		if (targetEditor.currentPack < WorldItems.Get.WorldItemPacks.Count && targetEditor.currentWorldItem < WorldItems.Get.WorldItemPacks [targetEditor.currentPack].Prefabs.Count) {
			GUI.color = Color.white;
			GUILayout.Label ("Current pack: " + targetEditor.currentPack.ToString () + " (" + WorldItems.Get.WorldItemPacks [targetEditor.currentPack].Name + ")");
			GUILayout.Label ("Current prefab: " + targetEditor.currentWorldItem.ToString () + " (" + WorldItems.Get.WorldItemPacks [targetEditor.currentPack].Prefabs [targetEditor.currentWorldItem].name + ")");
			GUI.color = Color.yellow;

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("\n<--Prev Pack\n", EditorStyles.miniButton)) {
				targetEditor.GetPrevPack ();
			}
			if (GUILayout.Button ("\nNext Pack-->\n", EditorStyles.miniButton)) {
				targetEditor.GetNextPack ();
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			GUI.color = Color.Lerp (Color.yellow, Color.red, 0.5f);
			if (GUILayout.Button ("\n<--Prev Worlditem\n", EditorStyles.miniButton)) {
				targetEditor.GetPrevPrefab (0);
			}
			if (GUILayout.Button ("\nNext Worlditem-->\n", EditorStyles.miniButton)) {
				targetEditor.GetNextPrefab (0);
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			GUI.color = Color.Lerp (Color.blue, Color.white, 0.5f);
			if (GUILayout.Button ("\nAuto-scale\n", EditorStyles.miniButton)) {
				targetEditor.AutoScale ();
			}
			if (GUILayout.Button ("\nRotate\n", EditorStyles.miniButton)) {
				targetEditor.Rotate ();
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			GUI.color = Color.Lerp (Color.blue, Color.white, 0.5f);
			if (GUILayout.Button ("\nCopy Rotation\n", EditorStyles.miniButton)) {
				targetEditor.CopyRotation ();
			}
			if (GUILayout.Button ("\nPaste Rotation\n", EditorStyles.miniButton)) {
				targetEditor.PasteRotation ();
			}

			GUILayout.EndHorizontal ();

			GUI.color = Color.cyan;
			if (GUILayout.Button ("\n\nSAVE PREFAB\n\n", EditorStyles.miniButton)) {
				targetEditor.SavePrefab ();
			}
		} else {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("\n<--Prev Pack\n", EditorStyles.miniButton)) {
				targetEditor.GetPrevPack ();
			}
			if (GUILayout.Button ("\nNext Pack-->\n", EditorStyles.miniButton)) {
				targetEditor.GetNextPack ();
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			GUI.color = Color.Lerp (Color.yellow, Color.red, 0.5f);
			if (GUILayout.Button ("\n<--Prev Worlditem\n", EditorStyles.miniButton)) {
				targetEditor.GetPrevPrefab (0);
			}
			if (GUILayout.Button ("\nNext Worlditem-->\n", EditorStyles.miniButton)) {
				targetEditor.GetNextPrefab (0);
			}
			GUILayout.EndHorizontal ();
		}
	}
}
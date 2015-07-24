using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[CustomEditor (typeof(PathEditor))]
public class PathEditorEditor : Editor
{
	protected PathEditor pathEditor;

	public void Awake ()
	{
		pathEditor = (PathEditor)target;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		EditorStyles.textField.wordWrap = true;

		GUI.color = Color.yellow;
		foreach (KeyValuePair <string,int> branch in pathEditor.AttachedTo) {
			if (branch.Key != pathEditor.Name) {
				GUILayout.Label (branch.Value.ToString () + ": " + branch.Key);
			}
		}

		GUI.color = Color.cyan;
		if (GUILayout.Button ("\nSave Path\n", EditorStyles.miniButton)) {
			pathEditor.EditorSave ();
		}
		if (GUILayout.Button ("\nLoad Path\n", EditorStyles.miniButton)) {
			pathEditor.EditorLoad ();
		}
		if (GUILayout.Button ("\nRefresh Path\n", EditorStyles.miniButton)) {
			pathEditor.EditorRefresh ();
		}
		GUI.color = Color.yellow;
		if (GUILayout.Button ("\nRebuild Path Spacing\n", EditorStyles.miniButton)) {
			pathEditor.RebuildPathSpacing ();
		}
		if (GUILayout.Button ("\nReverse Spline Node Order\n", EditorStyles.miniButton)) {
			pathEditor.ReverseSplineNodeOrder ();
		}
		if (GUILayout.Button ("\nFind Ground\n", EditorStyles.miniButton)) {
			pathEditor.EditorFindGround ();
		}
	}
}

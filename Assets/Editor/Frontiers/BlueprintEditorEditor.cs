using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
//i used to draw all the editor stuff in these classes
//now they're just a way to tell the node to draw its own editor
//i found it a lot easier to simply put #if UNITY_EDITOR / #endif
//inside of the main class
[CustomEditor(typeof(BlueprintEditor))]
public class BlueprintEditorEditor : Editor
{
		protected BlueprintEditor be;

		public void Awake()
		{
				be = (BlueprintEditor)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				DrawDefaultInspector();
				be.DrawEditor();
		}
}

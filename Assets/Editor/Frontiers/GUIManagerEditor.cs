using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;

[CustomEditor(typeof(GUIManager))]
public class GUIManagerEditor : Editor
{
		protected GUIManager guiManager;

		public void Awake()
		{
				guiManager = (GUIManager)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				guiManager.DrawEditorGUI();
				DrawDefaultInspector();
		}
}

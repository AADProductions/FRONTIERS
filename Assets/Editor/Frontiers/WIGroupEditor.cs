using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(WIGroup))]
public class WIGroupEditor : Editor
{
		protected WIGroup group;
		protected string GroupTest;
		protected string UniqueIDTest;

		public void Awake()
		{
				group = (WIGroup)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				EditorStyles.label.wordWrap = true;
				EditorStyles.miniLabel.wordWrap = true;
				EditorStyles.miniTextField.wordWrap = true;
				EditorStyles.whiteMiniLabel.wordWrap = true;
				EditorStyles.miniButton.wordWrap = true;

				EditorStyles.textField.fontStyle = FontStyle.Normal;
				EditorStyles.label.fontStyle = FontStyle.Normal;
				EditorStyles.miniLabel.fontStyle = FontStyle.Normal;
				EditorStyles.miniTextField.fontStyle = FontStyle.Normal;
				EditorStyles.whiteMiniLabel.fontStyle = FontStyle.Normal;
				EditorStyles.miniButton.fontStyle = FontStyle.Normal;

				DrawDefaultInspector();
				group.DrawEditor();

				GUI.color = Color.cyan;
				EditorGUILayout.LabelField(new GUIContent("---Unique ID testing---"));
				if (string.IsNullOrEmpty(GroupTest)) {
						GroupTest = group.Path;
				}
				GroupTest = EditorGUILayout.TextField(GroupTest);
				if (GUILayout.Button("Test Unique ID")) {
						UniqueIDTest = ShortUrl.GetUniqueID(GroupTest);
				}
				EditorGUILayout.LabelField(new GUIContent(UniqueIDTest));
		}
}
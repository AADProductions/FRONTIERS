using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(Skills))]
public class SkillsEditor : Editor
{
		protected Skills skillsManager;

		public void Awake()
		{
				skillsManager = (Skills)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;

				DrawDefaultInspector();
		
				GUI.color = Color.cyan;
				if (GUILayout.Button("\nLoad Skills from Disk\n", EditorStyles.miniButton)) {
						skillsManager.LoadEditor();
				}
				if (GUILayout.Button("\nSave skills to Disk\n", EditorStyles.miniButton)) {
						skillsManager.SaveEditor();
				}
		}
}

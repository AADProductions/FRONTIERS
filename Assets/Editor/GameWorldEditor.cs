using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(GameWorld))]
public class GameWorldEditor : Editor
{
		protected GameWorld gameWorld;

		public void Awake()
		{
				gameWorld = (GameWorld)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;

				DrawDefaultInspector();
		
				GUI.color = Color.cyan;
				if (GUILayout.Button("\nSave Settings\n", EditorStyles.miniButton)) {
						gameWorld.EditorSaveSettings();
				}
				if (GUILayout.Button("\nLoad Settings\n", EditorStyles.miniButton)) {
						gameWorld.EditorLoadSettings();
				}
				if (GUILayout.Button("\nSort flags\n", EditorStyles.miniButton)) {
						gameWorld.EditorSortFlags();
				}
				if (GUILayout.Button("\nNormalize Biomes\n", EditorStyles.miniButton)) {
						gameWorld.EditorNormalizeBiomes();
				}
		}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(PotionAvatar))]
public class PotionAvatarEditor : Editor
{
		protected PotionAvatar potionAvatar;

		public void Awake()
		{
				potionAvatar = (PotionAvatar)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;

				DrawDefaultInspector();

				GUI.color = Color.cyan;
				if (GUILayout.Button("\nRefresh Colors\n", EditorStyles.miniButton)) {
						potionAvatar.EditorRefreshColors();
				}
				if (GUILayout.Button("\nSave Potion to Disk\n", EditorStyles.miniButton)) {
						potionAvatar.EditorSavePotion();
				}
				GUI.color = Color.yellow;
				if (GUILayout.Button("\nLoad Potion from Disk\n", EditorStyles.miniButton)) {
						potionAvatar.EditorLoadPotion();
				}
		}
}

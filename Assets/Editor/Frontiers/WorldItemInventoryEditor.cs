using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

[CustomEditor(typeof(Inventory))]
public class WorldItemInventoryEditor : Editor
{
		protected Inventory inventory;

		public void Awake()
		{
				inventory = (Inventory)target;
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

				GUI.color = Color.cyan;
				List <WIStackContainer> activeStackContainers = inventory.ActiveStackContainers;
				GUILayout.Label("Num stack containers: " + activeStackContainers.Count, EditorStyles.whiteMiniLabel);
				GUILayout.Label("Gold pieces: " + inventory.GoldPieces, EditorStyles.whiteMiniLabel);
				GUILayout.Label("Type: " + inventory.Type.ToString(), EditorStyles.whiteMiniLabel);
				int stackContainerIndex = 1;
				foreach (WIStackContainer container in activeStackContainers) {
						GUILayout.Label("Stack container " + stackContainerIndex.ToString() + " items: " + container.NumItems);
						stackContainerIndex++;
				}
		}
}
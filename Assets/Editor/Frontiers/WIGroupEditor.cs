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
		}
}
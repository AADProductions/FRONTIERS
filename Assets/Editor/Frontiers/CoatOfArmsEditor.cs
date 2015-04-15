using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
using Frontiers.World.WIScripts;

[CustomEditor(typeof(CoatOfArms))]
public class CoatOfArmsEditor : Editor
{
		protected CoatOfArms sigil;

		public void Awake()
		{
				sigil = (CoatOfArms)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				DrawDefaultInspector();
				sigil.DrawEditor();
		}
}

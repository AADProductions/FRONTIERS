using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(WorldPathsEditor))]
public class WorldPathsEditorEditor : Editor
{
		protected WorldPathsEditor editor;

		public void Awake()
		{
				editor = (WorldPathsEditor)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				editor.DrawEditor();
		}
}

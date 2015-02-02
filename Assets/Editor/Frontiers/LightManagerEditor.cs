using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;

[CustomEditor(typeof(LightManager))]
public class LightManagerEditor : Editor
{
		protected LightManager lightManager;

		public void Awake()
		{
				lightManager = (LightManager)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				GUI.color = Color.yellow;
				if (GUILayout.Button("Save Light Templates")) {
						lightManager.EditorSaveLightTemplates();
				}
				if (GUILayout.Button("Load Light Templates")) {
						lightManager.EditorLoadLightTemplates();
				}
		}
}

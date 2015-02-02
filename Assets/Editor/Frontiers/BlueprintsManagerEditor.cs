using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;

[CustomEditor(typeof(Blueprints))]
public class BlueprintsManagerEditor : Editor
{
		Blueprints blueprints = null;

		public void Awake()
		{
				blueprints = (Blueprints)target;
		}

		public override void OnInspectorGUI()
		{		
				blueprints.InitializeEditor();
				DrawDefaultInspector();
				blueprints.DrawEditor();			
		}
}
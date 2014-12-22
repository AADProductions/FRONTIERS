using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(WorldItems))]
public class WorldItemsEditor : Editor
{
		protected WorldItems worlditems;

		public void Awake()
		{
				worlditems = (WorldItems)target;
		}

		public void OnSceneGUI()
		{
				worlditems.HandleSceneGUI();
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				worlditems.DrawEditor();
		}
}

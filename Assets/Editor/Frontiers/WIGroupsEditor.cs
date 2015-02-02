using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(WIGroups))]
public class WIGroupsEditor : Editor
{
		protected WIGroups wiGroups;

		public void Awake()
		{
				wiGroups = (WIGroups)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				wiGroups.DrawEditor();
		}
}

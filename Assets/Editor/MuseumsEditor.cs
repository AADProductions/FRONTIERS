using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(Museums))]
public class MuseumsEditor : Editor
{
		protected Museums m;

		public void Awake()
		{
				m = (Museums)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				m.DrawEditor();
		}
}

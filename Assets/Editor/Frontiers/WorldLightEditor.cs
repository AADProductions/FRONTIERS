using UnityEngine;
using UnityEditor;
using System.Collections;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(WorldLight))]
public class WorldLightEditor : Editor
{
		protected WorldLight wl;

		public void Awake()
		{
				wl = (WorldLight)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				wl.DrawEditor();
		}
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(Structures))]
public class StructuresEditor : Editor
{
		protected Structures structures;

		public void Awake()
		{
				structures = (Structures)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				structures.DrawEditor();
		}
}

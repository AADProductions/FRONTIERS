using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

[CustomEditor(typeof(Structure))]
public class StructureEditor : Editor
{
		protected Structure structure;
		public StructureTemplate template;

		public void Awake()
		{
				structure = (Structure)target;
		}

		public override void OnInspectorGUI()
		{
				if (structure.ShowDefaultEditor) {
						DrawDefaultInspector();
				}
				structure.DrawEditor();
		}
}

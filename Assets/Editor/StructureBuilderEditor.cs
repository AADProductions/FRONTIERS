using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[CustomEditor(typeof(StructureBuilder))]
public class StructureBuilderEditor : Editor
{
		protected StructureBuilder structurebuilder;
		protected bool openFileLoadSave	= false;

		public void Awake()
		{
				structurebuilder = (StructureBuilder)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
		
				structurebuilder.DrawEditor();
		}
}

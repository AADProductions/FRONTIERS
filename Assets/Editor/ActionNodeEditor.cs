using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;

[CustomEditor(typeof(ActionNode))]
public class ActionNodeEditor : Editor
{
		protected ActionNode node;

		public void Awake()
		{
				node = (ActionNode)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				//node.Refresh ();
		}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				GameManager.DrawEditor();
		}
}
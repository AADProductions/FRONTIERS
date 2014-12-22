using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(Frontiers.Potions))]
public class PotionsEditor : Editor
{
		Frontiers.Potions potions = null;

		public void Awake()
		{
				potions = (Frontiers.Potions)target;
		}

		public override void OnInspectorGUI()
		{		
				DrawDefaultInspector();
				potions.DrawEditor();			
		}
}
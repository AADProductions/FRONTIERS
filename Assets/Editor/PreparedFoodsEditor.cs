using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(PreparedFoods))]
public class PreparedFoodsEditor : Editor
{
		protected PreparedFoods foods;

		public void Awake()
		{
				foods = (PreparedFoods)target;
		}

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				foods.DrawEditor();
		}
}

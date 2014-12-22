using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;

[CustomEditor(typeof(Conditions))]
public class ConditionsEditor : Editor
{
	protected Conditions conditionsManager;

	public void Awake()
	{
		conditionsManager = (Conditions)target;
	}

	public override void OnInspectorGUI()
	{
		EditorStyles.textField.wordWrap = true;
		DrawDefaultInspector();
		conditionsManager.DrawEditor();
	}
}

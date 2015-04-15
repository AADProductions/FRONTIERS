using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

[CustomEditor(typeof(PathMarker))]
public class PathMarkerEditor : Editor
{
	protected PathMarker pathMarker;

	public void Awake()
	{
		pathMarker = (PathMarker)target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		pathMarker.DrawEditor();
	}
}
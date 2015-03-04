#pragma warning disable 0618

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;
using Frontiers.World.Gameplay;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;

[CustomEditor(typeof(Dungeon))]
public class DungeonEditor : Editor
{
	protected Dungeon dungeonBuilder;
	
	public void Awake ( )
	{
		dungeonBuilder = (Dungeon) target;
	}
	
	public override void OnInspectorGUI ( )
	{
		EditorStyles.textField.wordWrap = true;
		
		GUI.color = Color.cyan;
		if (GUILayout.Button ("Set up dungeon", EditorStyles.miniButton))
		{
			dungeonBuilder.CreateDungeonTransforms ();
			dungeonBuilder.EditorFindDungeonPieces ();
			dungeonBuilder.LinkOcclusionGroups ();
		}

		GUI.color = Color.yellow;
		GUILayout.Label ("FILE SAVE AND LOAD OPTIONS:");
		if (GUILayout.Button ("\n(SAVE TEMPLATE)\n", EditorStyles.miniButton))
		{
			dungeonBuilder.EditorSaveDungeonToTemplate ();
		}
	}
}

using System;
using UnityEngine;
using UnityEditor;
using Frontiers;
using Frontiers.World;

[CustomEditor (typeof(WorldChunk))]
public class WorldChunkEditor : Editor
{
	protected WorldChunk chunk;

	public void Awake ()
	{
		chunk = (WorldChunk)target;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();
		chunk.EditorDraw ();
	}
}

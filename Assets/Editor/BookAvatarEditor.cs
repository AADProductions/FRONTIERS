using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;

[CustomEditor(typeof(BookAvatar))]
public class BookAvatarEditor : Editor
{
	protected BookAvatar bookAvatar;
	
	public void Awake ( )
	{
		bookAvatar = (BookAvatar) target;
	}
	
	public override void OnInspectorGUI ( )
	{
		EditorStyles.textField.wordWrap = true;

		DrawDefaultInspector ();

		GUI.color = Color.yellow;
		if (GUILayout.Button ("Save Template", EditorStyles.miniButton)) {
			bookAvatar.EditorSaveTemplate ();
		}
		if (GUILayout.Button ("Load Template", EditorStyles.miniButton)) {
			bookAvatar.EditorLoadTemplate ();
		}
	}
}

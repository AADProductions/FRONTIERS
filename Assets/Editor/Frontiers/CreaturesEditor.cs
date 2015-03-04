using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;

[CustomEditor (typeof(Creatures))]
public class CreaturesEditor : Editor
{
	GUIStyle miniButtonStyle;
	GUIStyle textFieldStyle;
	GUIStyle LabelStyle;
	GUIStyle MiniLabelStyle;
	GUIStyle MiniTextStyle;
	GUIStyle ToolbarButtonStyle;
	GUIStyle BoldLabelStyle;
	protected Creatures targetEditor;

	public void Awake ()
	{
		targetEditor = (Creatures)target;
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		miniButtonStyle = new GUIStyle (EditorStyles.miniButton);
		textFieldStyle = new GUIStyle (EditorStyles.textField);
		LabelStyle = new GUIStyle (EditorStyles.label);
		MiniLabelStyle = new GUIStyle (EditorStyles.whiteMiniLabel);
		MiniTextStyle = new GUIStyle (EditorStyles.miniTextField);
		ToolbarButtonStyle	= new GUIStyle (EditorStyles.toolbarButton);
		BoldLabelStyle = new GUIStyle (EditorStyles.boldLabel);
		
		textFieldStyle.wordWrap = true;
		LabelStyle.wordWrap = true;
		MiniLabelStyle.wordWrap = true;
		MiniTextStyle.wordWrap = true;
		MiniLabelStyle.wordWrap = true;
		miniButtonStyle.wordWrap = true;
		
		textFieldStyle.fontStyle = FontStyle.Normal;
		LabelStyle.fontStyle = FontStyle.Normal;
		MiniLabelStyle.fontStyle = FontStyle.Normal;
		ToolbarButtonStyle.fontStyle = FontStyle.Normal;
		MiniLabelStyle.fontStyle = FontStyle.Normal;
		miniButtonStyle.fontStyle = FontStyle.Normal;
		
		miniButtonStyle.stretchWidth = true;
		miniButtonStyle.alignment = TextAnchor.MiddleCenter;

//		Color drawColor = Color.cyan;
//		foreach (CharacterTemplate template in targetEditor.CharacterTemplates) {
//			drawColor = Color.Lerp (drawColor, Color.yellow, 0.25f);
//			GUI.color = drawColor;
//			DrawCharacterTemplate (template);
//		}

		GUI.color = Color.yellow;
		if (GUILayout.Button ("\nSave Creature Templates\n")) {
			targetEditor.EditorSaveTemplates ();
		}
		if (GUILayout.Button ("\nLoad Creature Templates\n")) {
			targetEditor.EditorLoadTemplates ();
		}
		if (GUILayout.Button ("\nSort Creature Templates\n")) {
			targetEditor.EditorSortTemplates ();
		}
	}
}
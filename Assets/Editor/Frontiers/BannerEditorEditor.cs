using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
[CustomEditor(typeof(BannerEditor))]
public class BannerEditorEditor : Editor
{
		protected BannerEditor be;

		public void Awake()
		{
				be = (BannerEditor)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				DrawDefaultInspector();
				be.DrawEditor();
		}
}

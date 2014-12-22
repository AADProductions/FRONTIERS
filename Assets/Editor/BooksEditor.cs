using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

[CustomEditor(typeof(Books))]
public class BooksEditor : Editor
{
		protected Books books;

		public void Awake()
		{
				books = (Books)target;
		}

		public override void OnInspectorGUI()
		{
				EditorStyles.textField.wordWrap = true;
				DrawDefaultInspector();
				books.DrawEditor();
		}
}

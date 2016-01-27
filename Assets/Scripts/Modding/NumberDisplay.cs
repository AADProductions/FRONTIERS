// This goes in the Unity project
// ------------------------------
// 
// Make sure hModding is attached to 'Main Camera' or whatever.
// Same with this script
//
// Make sure you change the path in the Awake function of hModding.cs
// 
// Run the program, if the number on the screen is static then the mod
// didn't work. If it constantly changes, then it's running and works.
//
using UnityEngine;
using System.Collections;

public class NumberDisplay : MonoBehaviour
{
	public int Number;
	// Use this for initialization
	private void Start ()
	{
		Number = 100;
	}

	private void OnGUI ()
	{
		GUI.Label (new Rect (100, 100, 100, 100), Number.ToString ());
	}
}
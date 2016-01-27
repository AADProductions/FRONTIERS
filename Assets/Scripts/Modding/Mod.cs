// This file goes in the Mod DLL
// -----------------------------
// 
// Compile AFTER the Unity program has compiled
// 
// Link to UnityEngine.dll
// Link to the Assembly-CSharp.dll
// 
// build as dll.
//
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DisplayRigger : MonoBehaviour
{
	private NumberDisplay numberDisplay;

	public void Start ()
	{
		GameObject go = GameObject.Find ("Main Camera");
		numberDisplay = go.GetComponent<NumberDisplay> ();
	}

	public void Update ()
	{
		numberDisplay.Number = (int)(UnityEngine.Random.value * 10000.0F);
	}
}

public class Mod
{
	private GameObject test;

	public String GetInfo ()
	{
		UnityEngine.Debug.Log ("Hello from Application Start");
		return "ModTest v 1.0";
	}

	public void OnQuit ()
	{
		UnityEngine.Debug.Log ("Goodbye from Application Stop");
	}

	public void OnLoad ()
	{
		test = new GameObject ("Mod Testing");
		test.AddComponent<DisplayRigger> ();
		UnityEngine.Debug.Log ("Mod Enabled");
	}

	public void OnUnload ()
	{
		UnityEngine.Object.Destroy (test);
		test = null;
		UnityEngine.Debug.Log ("Mod Disabled");
	}
}

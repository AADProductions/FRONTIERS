using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class CleanUpStructure : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Vector3 position = transform.localPosition;
		Vector3 rotation = transform.localRotation.eulerAngles;
		
		position.x = Mathf.Round (position.x * 4) / 4;
		position.y = Mathf.Round (position.y * 4) / 4;
		position.z = Mathf.Round (position.z * 4) / 4;
		
		rotation.x = Mathf.Round (rotation.x / 5.0f) * 5;
		rotation.y = Mathf.Round (rotation.y / 5.0f) * 5;
		rotation.z = Mathf.Round (rotation.z / 5.0f) * 5;
		
		transform.localPosition = position;
		transform.localRotation = Quaternion.identity;
		transform.Rotate (rotation);
		
		GameObject.DestroyImmediate (this);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

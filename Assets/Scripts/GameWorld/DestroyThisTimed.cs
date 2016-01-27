using UnityEngine;
using System.Collections;

public class DestroyThisTimed : MonoBehaviour {

	public float DestroyTime = 1f;
	public float StartTime = 0f;
	// Use this for initialization
	void Start () {
		StartTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > StartTime + DestroyTime) {
			GameObject.Destroy (gameObject, 0.01f);
			enabled = false;
		}
	}
}

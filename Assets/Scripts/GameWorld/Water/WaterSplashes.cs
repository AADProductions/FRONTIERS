using UnityEngine;
using System.Collections;

public class WaterSplashes : MonoBehaviour
{
	public GameObject 	SplashPrefab;
	
	public void 		OnTriggerEnter (Collider other)
	{
//		//Debug.Log ("Collided with " + other.name);		
		GameObject.Instantiate (SplashPrefab, other.transform.position, Quaternion.identity);
	}
}

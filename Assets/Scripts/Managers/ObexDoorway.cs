//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers;
//using ExtensionMethods;
//
//
//public class ObexDoorway : MonoBehaviour {
//
//	public GameObject DynamicObject;
//	public List <GameObject> EffectsOnStart = new List<GameObject> ();
//	public List <ParticleSystem> ParticleSystemsOnStart;
//	public List <ParticleSystem> ParticleSystemsDuration;
//	public List <ParticleSystem> ParticleSystemsOnFinish;
//	public List <GameObject> RigidBodiesOnFinish = new List<GameObject> ();
//	public List <GameObject> EffectsOnFinish = new List<GameObject> ();
//
//	public void Start ( )
//	{
//		foreach (ParticleSystem onStartSystem in ParticleSystemsOnStart) {
//			onStartSystem.enableEmission = false;
//		}
//		foreach (ParticleSystem durationSystem in ParticleSystemsOnStart) {
//			durationSystem.enableEmission = false;
//		}
//		foreach (ParticleSystem onFinishSystem in ParticleSystemsOnStart) {
//			onFinishSystem.enableEmission = false;
//		}
//	}
//
//	public void Update ( )
//	{
//		if (Input.GetKeyDown (KeyCode.F2)) {
//			OpenDoor ();
//		}
//	}
//
//	public void OpenDoor ( )
//	{
//		StartCoroutine (OpenDoorOverTime ());
//	}
//
//	public IEnumerator OpenDoorOverTime ( )
//	{
//		foreach (GameObject fxOnStart in EffectsOnStart) {
//			fxOnStart.SetActive (true);
//			yield return new WaitForSeconds (0.05f);
//		}
//
//		DynamicObject.animation.Play ();
//
//		foreach (ParticleSystem onStartSystem in ParticleSystemsOnStart) {
//			onStartSystem.enableEmission = true;
//		}
//		foreach (ParticleSystem durationSystem in ParticleSystemsOnStart) {
//			durationSystem.enableEmission = true;
//		}
//
//		while (DynamicObject.animation.isPlaying) {
//			yield return null;
//		}
//
//		foreach (ParticleSystem onStartSystem in ParticleSystemsOnStart) {
//			onStartSystem.enableEmission = false;
//		}
//		foreach (ParticleSystem durationSystem in ParticleSystemsOnStart) {
//			durationSystem.enableEmission = false;
//		}
//		foreach (ParticleSystem onFinishSystem in ParticleSystemsOnStart) {
//			onFinishSystem.enableEmission = true;
//		}
//		foreach (GameObject fxOnFinish in EffectsOnFinish) {
//			fxOnFinish.SetActive (true);
//			yield return null;
//		}
//		foreach (GameObject rbOnFinish in RigidBodiesOnFinish) {
//			rbOnFinish.layer = Globals.LayerNumWorldItemActive;
//			rbOnFinish.GetOrAdd <BoxCollider> ();
//			Rigidbody rb = rbOnFinish.GetOrAdd <Rigidbody> ();
//			rb.transform.parent = transform;
//			rb.isKinematic = false;
//			rb.useGravity = true;
//		}
//	}
//}

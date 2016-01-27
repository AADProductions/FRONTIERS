using UnityEngine;
using System.Collections;

public class TestPassThroughTriggerPair : MonoBehaviour {

	public bool Inside = false;

	public void OnPassThrough ( ) {
		Inside = !Inside;
	}

	public void OnDrawGizmos ( ) {
		Gizmos.color = Color.red;
		if (Inside) {
			Gizmos.color = Color.green;
		}
		Gizmos.DrawSphere(transform.position, 1f);
	}
}

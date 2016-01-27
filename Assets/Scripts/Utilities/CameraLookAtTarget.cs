using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraLookAtTarget : MonoBehaviour {

	public Transform LookTarget;

	public void Update ( )
	{
		transform.LookAt (LookTarget);
	}
}

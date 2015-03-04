using UnityEngine;
using System.Collections;

public class OrientAlongVector : MonoBehaviour{

	public Vector3 UpVector = new Vector3 (0f, 1f, 0f);

	public void LateUpdate ( )
	{
		transform.up = UpVector;
	}
}

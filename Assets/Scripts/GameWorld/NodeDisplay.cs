using UnityEngine;
using System.Collections;

public class NodeDisplay : MonoBehaviour {

	void OnDrawGizmos ( )
	{
		foreach (Transform child in transform)
		{
			if (child.tag == "Trigger")
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.cyan;
			}
			Gizmos.DrawWireCube (child.transform.position, Vector3.one * 1.25f);
		}
	}
}

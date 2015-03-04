using UnityEngine;
using System.Collections;
using Frontiers;

[ExecuteInEditMode]
public class StructureFootprint : MonoBehaviour {

	public void Awake ( )
	{
		name = "Footprint";
		gameObject.GetOrAdd <BoxCollider> ();
	}

	public void OnDrawGizmos ( )
	{
		Bounds bounds = new Bounds (Vector3.zero, Vector3.one);
		Vector3 v3Center = bounds.center;
		Vector3 v3Extents = bounds.extents;

		Color color = Color.yellow;
//		color.a = 0.15f;
		Gizmos.color = color;

		Vector3 v3FrontTopLeft;
		Vector3 v3FrontTopRight;
		Vector3 v3FrontBottomLeft;
		Vector3 v3FrontBottomRight;
		Vector3 v3BackTopLeft;
		Vector3 v3BackTopRight;
		Vector3 v3BackBottomLeft;
		Vector3 v3BackBottomRight;

		v3FrontTopLeft = new Vector3 (v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
		v3FrontTopRight = new Vector3 (v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
		v3FrontBottomLeft = new Vector3 (v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
		v3FrontBottomRight = new Vector3 (v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
		v3BackTopLeft = new Vector3 (v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
		v3BackTopRight = new Vector3 (v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
		v3BackBottomLeft = new Vector3 (v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
		v3BackBottomRight = new Vector3 (v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

		v3FrontTopLeft = transform.TransformPoint (v3FrontTopLeft);
		v3FrontTopRight = transform.TransformPoint (v3FrontTopRight);
		v3FrontBottomLeft = transform.TransformPoint (v3FrontBottomLeft);
		v3FrontBottomRight = transform.TransformPoint (v3FrontBottomRight);
		v3BackTopLeft = transform.TransformPoint (v3BackTopLeft);
		v3BackTopRight = transform.TransformPoint (v3BackTopRight);
		v3BackBottomLeft = transform.TransformPoint (v3BackBottomLeft);
		v3BackBottomRight = transform.TransformPoint (v3BackBottomRight);  

		Debug.DrawLine (v3FrontBottomRight, v3FrontBottomLeft, color);
		Gizmos.DrawLine (v3FrontBottomLeft, v3FrontTopLeft);
		Debug.DrawLine (v3BackBottomRight, v3BackBottomLeft, color);
		Gizmos.DrawLine (v3BackBottomLeft, v3BackTopLeft);
		Debug.DrawLine (v3FrontBottomRight, v3BackBottomRight, color);
		Gizmos.DrawLine (v3FrontBottomLeft, v3BackBottomLeft);

		color = Color.red;
//		color.a = 0.5f;
		Gizmos.color = color;

		Gizmos.DrawLine (v3FrontTopLeft, v3FrontTopRight);
		Gizmos.DrawLine (v3FrontTopRight, v3FrontBottomRight);
		Gizmos.DrawLine (v3BackTopLeft, v3BackTopRight);
		Gizmos.DrawLine (v3BackTopRight, v3BackBottomRight);
		Gizmos.DrawLine (v3FrontTopLeft, v3BackTopLeft);
		Gizmos.DrawLine (v3FrontTopRight, v3BackTopRight);
	}
}

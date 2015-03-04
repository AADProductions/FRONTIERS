using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

[ExecuteInEditMode]
public class PrepWorldItemScript : MonoBehaviour
{

	// Use this for initialization
	void Start () {
//		List <Transform> children = new List<Transform> ( );
//		foreach (Transform child in transform)
//		{
//			if (child.gameObject.HasComponent <SkinnedMeshRenderer> ( ))
//			{
//				SkinnedMeshRenderer smr = child.gameObject.GetComponent <SkinnedMeshRenderer> ( );
//				MeshRenderer mr 		= gameObject.AddComponent <MeshRenderer> ( );
//				MeshFilter mf 			= gameObject.AddComponent <MeshFilter> ( );
//				mr.sharedMaterials		= smr.sharedMaterials;
//				mf.sharedMesh			= smr.sharedMesh;
//				GameObject.DestroyImmediate (smr);
//			}
//			children.Add (child);			
//		}
//		
//		foreach (Transform childToDestroy in children)
//		{
//			GameObject.DestroyImmediate (childToDestroy.gameObject);
//		}
//		Stackable stackable			= gameObject.GetComponent <Stackable> ( );
//		stackable.SquareOffset.x 	= gameObject.transform.localPosition.x;
//		stackable.SquareOffset.y 	= gameObject.transform.localPosition.y;
//		stackable.SquareOffset.z 	= gameObject.transform.localPosition.z;
//		
//		stackable.SquareRotation 	= gameObject.transform.localRotation.eulerAngles;
//		stackable.SquareScale		= gameObject.transform.localScale.x;
		
//		gameObject.transform.localPosition = new Vector3 (stackable.SquareOffset.x, stackable.SquareOffset.y, stackable.SquareOffset.z);
//		gameObject.transform.localRotation = Quaternion.identity;
//		gameObject.transform.Rotate (stackable.SquareRotation.x, stackable.SquareRotation.y, stackable.SquareRotation.z);
//		gameObject.transform.localScale		= Vector3.one * stackable.SquareScale;
//		stackable.SquareOffset.y 	= gameObject.transform.localPosition.y;
//		stackable.SquareOffset.z 	= gameObject.transform.localPosition.z;
//		
//		stackable.SquareRotation 	= gameObject.transform.localRotation.eulerAngles;
//		stackable.SquareScale		= gameObject.transform.localScale.x;		
		
//		Equippable equippable		= gameObject.GetComponent <Equippable> ( );
//		equippable.EquipOffset.x 	= gameObject.transform.localPosition.x;
//		equippable.EquipOffset.y 	= gameObject.transform.localPosition.y;
//		equippable.EquipOffset.z 	= gameObject.transform.localPosition.z;
////		
//		equippable.EquipRotation 	= gameObject.transform.localRotation.eulerAngles;
		
		GameObject.DestroyImmediate (this);
	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
}

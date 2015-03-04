//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers;
//using Frontiers.World;
//
//public class SpawnFruitOnTree : MonoBehaviour
//{
//	public GameObject 			FruitTemplate;
//	public PickableFruitTree	FruitTree;
//	
//	public void					Awake ( )
//	{
//		FruitTree = gameObject.GetComponent <PickableFruitTree> ( );
//		if (FruitTree == null)
//		{
//			GameObject.Destroy (gameObject);
//		}
//	}
//	
//	public void 				Start ( )
//	{		
//		foreach (Transform child in transform)
//		{
//			if (child.name == WorldItems.CleanWorldItemName (FruitTemplate.name))
//			{
//				GameObject newFruit 			= GameObject.Instantiate (FruitTemplate, child.transform.position, Quaternion.identity) as GameObject;
//				newFruit.name 					= FruitTemplate.name;
//				newFruit.rigidbody.isKinematic 	= true;
//				FruitTree.FruitList.Add (newFruit);
//				GameObject.Destroy (child.gameObject, 4.0f);
//			}
//		}
//	}
//	
//	public void					Update ( )
//	{
//		GameObject.Destroy (this);
//	}
//}

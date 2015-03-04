//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers.World;
//
//public class ShakeFruitOffTree : MonoBehaviour
//{
//	public PickableFruitTree	FruitTree;
//	public float 				MinimumDamageAmount 	= 1.0f;
//	public float 				OddsOfFruitFalling 		= 0.1f;
//	public Tree					TreeObject;
//	
//	public void					Awake ( )
//	{
//		TreeObject 	= gameObject.GetComponent <Tree> ( );
//		FruitTree 	= gameObject.GetComponent <PickableFruitTree> ( );
//	}
//	
//	public void 				OnTakeDamage (float damageAmount)
//	{
//		//Debug.Log ("Shaking fruit off trees");
//		ShakeLeaves ( );		
//		
//		if (damageAmount > MinimumDamageAmount)
//		{
//			for (int i = FruitTree.FruitList.Count - 1; i >= 0; i--)
//			{
//				if (Random.value > OddsOfFruitFalling)
//				{
//					FruitTree.FruitList [i].SendMessage ("SetWorldMode", SendMessageOptions.DontRequireReceiver);
//					FruitTree.FruitList.RemoveAt (i);
//				}
//			}
//		}
//	}
//	
//	public void					ShakeLeaves ( )
//	{
//		
//	}
//}

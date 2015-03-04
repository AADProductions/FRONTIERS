//using UnityEngine;
//using System;
//using System.Runtime.Serialization;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers;
//using ExtensionMethods;
//
//namespace Frontiers.World
//{
//	public class Flower : WIScript
//	{
//		public string				FlowerPrefabName;
//		public List <GameObject>	Flowers				= new List <GameObject> ( );
//		public MinMax				FlowerScale;
//		public float				FlowerNumber		= 1.0f;
//		public float 				BaseHueShift		= 0.0f;
//		public float				HueLimitL			= 0.0f;
//		public float				HueLimitU			= 0.0f;
//		public bool					HueLimitInvert		= false;
//		
//		public void					GrowFlowers (GameObject stem)
//		{
//			if (Flowers.Count > 0)
//			{
//				return;
//			}
//			
//			foreach (Transform budPosition in stem.FindChildren ("BudPosition"))
//			{
//				if (UnityEngine.Random.value <= FlowerNumber)
//				{
//					Flowers.Add (Plant.LoadPlantPiece (FlowerPrefabName, "Flower", FlowerScale.Current, budPosition));
//				}
//			}
//		}
//		
//		public void					UpdateScale ( )
//		{
//			foreach (GameObject flower in Flowers)
//			{
//				flower.transform.localScale = Vector3.one * FlowerScale.Value;
//			}
//		}
//		
//		public void					UpdateHue ( )
//		{
////			//Debug.Log ("Updating hue");
//			foreach (GameObject flower in Flowers)
//			{
//				flower.renderer.sharedMaterial.SetFloat ("_Shift", (BaseHueShift - 0.5f) * 2.0f);
//			}
//		}
//	}
//}
//
//[Serializable]
//public class MinMax
//{
//	public MinMax ( )
//	{
//		Current = 1.0f;
//		Min		= 0.0f;
//		Max		= 1.0f;
//		Avg 	= 0.5f;
//		Var		= 0.0f;
//	}
//	
//	public float 	Current;
//	public float 	Min;
//	public float 	Max;
//	public float	Avg;
//	public float	Var;
//	public float	Value
//	{
//		get
//		{
//			if (Var > 0.0f)
//			{
//				return UnityEngine.Random.Range (Mathf.Clamp ((Current - Var), Min, Max), Mathf.Clamp ((Current + Var), Min, Max));
//			}
//			else
//			{
//				return Current;
//			}
//		}
//	}
//}
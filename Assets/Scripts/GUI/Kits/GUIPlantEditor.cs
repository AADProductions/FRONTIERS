//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers;
//using Frontiers.World;
//
//public class GUIPlantEditor : MonoBehaviour
//{
//	public WorldItem 			worlditem;
//	public Plant				plant;
//	public Flower				flower;
//	
//	public GameObject			PlantCameraPivot;
//	public GameObject			GroundObject;
//	public GameObject			GrassObject;
//	public Camera				PlantCamera;
//	
//	public UICheckbox			RotateCameraCheckbox;
//	public UICheckbox			ShowGroundCheckbox;
//	public UICheckbox			ShowGrassCheckbox;
//	public UISlider				CameraZoomSlider;
//	public float				CameraZoomMultiplier 	= 100.0f;
//	public float				CameraZoomMin			= 50.0f;
//
//	public float				StartPosition;
//	public float				PositionOffset;
//
//	public List <GameObject> 	RootPrefabs 			= new List<GameObject> ( );
//	public UISlider				RootPrefab;
//	public float				RootSizeMin				= 0.25f;
//	public UISlider				RootSize;
//	public float				RootSizeMultiplier		= 5.0f;
//
//	public List <GameObject>	StemPrefabs				= new List <GameObject> ( );
//	public UISlider				StemPrefab;
//	public UISlider				StemSize;
//	public float				StemSizeMin				= 0.25f;
//	public float				StemSizeMultiplier		= 5.0f;
//	public UISlider				StemHue;
//
//	public List <GameObject>	FlowerPrefabs			= new List <GameObject> ( );
//	public UISlider				FlowerPrefab;
//	public UISlider				FlowerSize;
//	public float				FlowerSizeMin			= 0.25f;
//	public float				FlowerSizeMultiplier	= 5.0f;
//	public UISlider				FlowerHue;
//	public UISlider				FlowerVariation;
//	public UISlider				FlowerNumber;
//	
//	public void					Update ( )
//	{
//		PlantCamera.fieldOfView = (CameraZoomSlider.sliderValue * CameraZoomMultiplier) + CameraZoomMin;
//		
//		if (RotateCameraCheckbox.isChecked)
//		{
//			PlantCameraPivot.transform.Rotate (0f, 0.02f, 0f);
//		}
//		
//		if (ShowGroundCheckbox.isChecked)
//		{
//			GroundObject.SetActive (true);
//			if (ShowGrassCheckbox.isChecked)
//			{
//				GrassObject.SetActive (true);
//			}
//			else
//			{
//				GrassObject.SetActive (false);
//			}
//		}
//		else
//		{
//			GroundObject.SetActive (false);
//		}
//	}
//
//	public void					Start ( )
//	{
//		plant 		= worlditem.Get <Plant> ( );
//		flower	 	= worlditem.Get <Flower> ( );
//		
//		transform.localPosition 		= new Vector3 (StartPosition, 0f, 0f);
//
//		StemPrefab.numberOfSteps 		= StemPrefabs.Count;
//		StemPrefab.sliderValue			= FindIndexByName (plant.StemPrefabName, StemPrefabs);
//		StemPrefab.eventReceiver		= gameObject;
//		StemPrefab.functionName			= "OnChangeStemPrefab";
//		
//		StemSize.eventReceiver			= gameObject;
//		StemSize.functionName			= "OnChangeStemSize";
//		
//		StemHue.eventReceiver			= gameObject;
//		StemHue.functionName			= "OnChangeStemHue";
//
//		FlowerPrefab.numberOfSteps		= FlowerPrefabs.Count;
//		FlowerPrefab.sliderValue		= FindIndexByName (flower.FlowerPrefabName, FlowerPrefabs);
//		FlowerPrefab.eventReceiver		= gameObject;
//		FlowerPrefab.functionName		= "OnChangeFlowerPrefab";
//		
//		FlowerSize.eventReceiver		= gameObject;
//		FlowerSize.functionName			= "OnChangeFlowerSize";
//		
//		FlowerHue.sliderValue			= 0.0f;
//		FlowerHue.eventReceiver			= gameObject;
//		FlowerHue.functionName			= "OnChangeFlowerHue";
//		
//		FlowerVariation.eventReceiver	= gameObject;
//		FlowerVariation.functionName	= "OnChangeFlowerVariation";
//		
//		FlowerNumber.eventReceiver		= gameObject;
//		FlowerNumber.functionName		= "OnChangeFlowerNumber";
//
//		RootPrefab.numberOfSteps		= RootPrefabs.Count;
//		RootPrefab.sliderValue			= FindIndexByName (plant.RootPrefabName, RootPrefabs);
//		RootPrefab.eventReceiver		= gameObject;
//		RootPrefab.functionName			= "OnChangeRootPrefab";
//		
//		RootSize.eventReceiver			= gameObject;
//		RootSize.functionName			= "OnChangeRootSize";
//	}
//
//	public void					OnClickNext ( )
//	{
//		mInitialized				= true;		
//		
//		MoveNext ( );
//	}
//
//	public void					OnClickPrev ( )
//	{
//		MovePrev ( );
//	}
//
//	public void 				MoveNext ( )
//	{
//		transform.localPosition = transform.localPosition + new Vector3 (PositionOffset, 0f, 0f);
//	}
//
//	public void					MovePrev ( )
//	{
//		transform.localPosition = transform.localPosition - new Vector3 (PositionOffset, 0f, 0f);
//	}
//
//	public void					OnChangeStemPrefab ( )
//	{
//		if (!mInitialized) { return; }
//
//		int stemPrefabIndex = Mathf.CeilToInt (StemPrefab.sliderValue * (StemPrefabs.Count - 1));
//		//Debug.Log ("Changing stem to " + stemPrefabIndex);
//		GameObject prefab = StemPrefabs [stemPrefabIndex];
//		plant.StemPrefabName = prefab.name;
//		worlditem.SendMessage ("StartFromScratch");
//	}
//	
//	public void					OnChangeStemSize ( )
//	{
//		if (!mInitialized) { return; }
//
//		plant.StemScale.Current	= (StemSize.sliderValue * StemSizeMultiplier) + StemSizeMin;
//		worlditem.SendMessage ("UpdateScale");
//	}
//
//	public void					OnChangeFlowerPrefab ( )
//	{
//		if (!mInitialized) { return; }
//
//		int flowerPrefabIndex = Mathf.CeilToInt (FlowerPrefab.sliderValue * (FlowerPrefabs.Count - 1));
//		//Debug.Log ("Changing flower to " + flowerPrefabIndex);
//		GameObject prefab = FlowerPrefabs [flowerPrefabIndex];
//		flower.FlowerPrefabName = prefab.name;
//		worlditem.SendMessage ("StartFromScratch");
//	}
//	
//	public void					OnChangeFlowerSize ( )
//	{
//		if (!mInitialized) { return; }
//		
//		flower.FlowerScale.Current = (FlowerSize.sliderValue * FlowerSizeMultiplier) + FlowerSizeMin;
//		worlditem.SendMessage ("UpdateScale");
//	}	
//
//	public void					OnChangeRootPrefab ( )
//	{
//		if (!mInitialized) { return; }
//
//		//Debug.Log ("Changing root");
//		int rootPrefabIndex = Mathf.CeilToInt (RootPrefab.sliderValue * (RootPrefabs.Count - 1));
//		GameObject prefab = RootPrefabs [rootPrefabIndex];
//		plant.RootPrefabName = prefab.name;
//		worlditem.SendMessage ("StartFromScratch");
//
//	}
//	
//	public void					OnChangeRootSize ( )
//	{
//		if (!mInitialized) { return; }
//
//		plant.RootScale.Current	= (RootSize.sliderValue * RootSizeMultiplier) + RootSizeMin;
//		worlditem.SendMessage ("UpdateScale");
//	}
//	
//	public void					OnChangeFlowerHue ( )
//	{
//		if (!mInitialized) { return; }
//				
//		flower.BaseHueShift	= FlowerHue.sliderValue;
//		worlditem.SendMessage ("UpdateHue");
//	}
//	
//	public void 				OnChangeFlowerVariation ( )
//	{
//		if (!mInitialized) { return; }
//				
//		flower.FlowerScale.Var = FlowerVariation.sliderValue;
//		worlditem.SendMessage ("UpdateScale");		
//	}
//	
//	public void					OnChangeFlowerNumber ( )
//	{
//		if (!mInitialized) { return; }
//				
//		flower.FlowerNumber = FlowerNumber.sliderValue;
//		worlditem.SendMessage ("StartFromScratch");			
//	}
//	
//	public void					OnChangeStemHue ( )
//	{
//		if (!mInitialized) { return; }
//				
//		//Debug.Log ("Changing stem hue");
//		plant.StemBaseHueShift	= StemHue.sliderValue;
//		worlditem.SendMessage ("UpdateHue");		
//	}
//
//	public static int			FindIndexByName (string name, List <GameObject> list)
//	{
//		int index = -1;
//		for (int i = 0; i < list.Count; i++)
//		{
//			if (list [i].name == name)
//			{
//				index = i;
//				break;
//			}
//		}
//		return index;
//	}
//
//	protected bool				mInitialized = false;
//}

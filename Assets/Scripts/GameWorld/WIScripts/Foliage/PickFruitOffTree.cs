//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers;
//using Frontiers.World;
//
//public class PickFruitOffTree : MonoBehaviour, IProgressDialogObject
//{	
//	public float				RTSecondsPerFruit		= 0.5f;
//	
//	public float				ProgressValue			{ get; set; }
//	public string				ProgressMessage			{ get; set; }
//	public string 				ProgressIconName		{ get; set; }
//	public string				ProgressObjectName		{ get; set; }
//	public bool					ProgressFinished		{ get; set; }
//	public bool					ProgressCanceled		{ get; set; }
//	
//	public PickableFruitTree	FruitTree;
//	
//	public bool					PickableByHand;
//	
//	public void					Awake ( )
//	{
//		FruitTree = gameObject.GetComponent <PickableFruitTree> ( );
//		FruitTree.MakeUsable ( );
//	}
//	
//	public void 				OnGainPlayerFocus ( )
//	{		
//		if (FruitTree.IsPickable && PickableByHand && !FruitTree.HasBeenPicked)
//		{
//			FruitTree.Usable.AddOption (new WIListOption ("Pick All Fruit"));
//		}
//		else
//		{
//			FruitTree.Usable.RemoveOption ("Pick All Fruit");
//		}
//	}
//	
//	public void					OnLosePlayerFocus ( ) 
//	{
//		if (mIsPicking)
//		{
//			mIsPicking 			= false;
//			ProgressCanceled 	= true;
//		}
//	}
//	
//	public void					OnPlayerUseWorldItem (string message)
//	{
//		switch (message)
//		{
//		case "Pick All Fruit":
//			mIsPicking			= true;
//			
//			ProgressObjectName 	= "PICK " + FruitTree.StackName.ToUpper ( );
//			ProgressValue		= 0.0f;
//			ProgressFinished	= false;
//			ProgressCanceled	= false;
//			
//			GameObject childEditor = GUIManager.SpawnNGUIChildEditor (gameObject, GUIManager.Get.NGUIProgressDialog, false);
//			GUIManager.SendEditObjectToChildEditor <IProgressDialogObject> (new ChildEditorCallback <IProgressDialogObject> (OnFinishPickingFruit),childEditor, this);
//			
//			mStartPickTime 		= Time.time;
//			mEndPickTime 		= Time.time + (FruitTree.FruitList.Count * RTSecondsPerFruit);
//			mTimePerFruit 		= (mEndPickTime - mStartPickTime) / FruitTree.FruitList.Count;
//			mNextFruitPickTime	= mStartPickTime;
//			
//			StartCoroutine (PickFruitOverTime ( ));
//			break;
//			
//		default:
//			break;
//		}
//	}
//				
//	public void 				OnFinishPickingFruit (IProgressDialogObject editObject, IGUIChildEditor <IProgressDialogObject> childEditor)
//	{
//		GUIManager.ScaleDownEditor (childEditor.gameObject).Proceed (true);
//		mIsPicking = false;
//	}
//	
//	public IEnumerator			PickFruitOverTime ( )
//	{
//		int numFruit 	= FruitTree.FruitList.Count;
//		
//		while (Time.time < mEndPickTime && mIsPicking)
//		{
//			if (FruitTree.FruitList.Count > 0)
//			{
//				ProgressValue 	= (Time.time - mStartPickTime ) / (mEndPickTime - mStartPickTime);			
//				ProgressMessage = ("Picking " + (Mathf.FloorToInt (numFruit * ProgressValue) + 1) + " of " + numFruit);
//				
//				if (Time.time > mNextFruitPickTime)
//				{
//					mNextFruitPickTime += mTimePerFruit;
//					GameObject nextFruit = FruitTree.FruitList [0];
//					FruitTree.FruitList.RemoveAt (0);
//					Player.Local.Inventory.AddItems (nextFruit.GetComponent <WorldItem> ( ));
//				}
//			}
//			
//			yield return new WaitForSeconds (0.05f);
//		}
//		
//		ProgressValue		= 1.0f;
//		ProgressFinished 	= true;
//		
//		yield break;
//	}
//	
//	protected float				mStartPickTime;
//	protected float				mEndPickTime;
//	protected float				mTimePerFruit;
//	protected float				mNextFruitPickTime;
//	protected bool				mIsPicking;
//}

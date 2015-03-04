using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

public class WorldItemGraveYard : MonoBehaviour
{
	public static GameObject 	gGraveYard;
	
	public void 				Awake ( )
	{
		DontDestroyOnLoad (this);
		
		gameObject.layer = Globals.LayerNumHidden;
	}
	
	public void 				Update ( )
	{
		while (mIncoming.Count > 0)	
		{
			WorldItem deadWorldItem 				= mIncoming.Dequeue ( );
			deadWorldItem.transform.parent 			= transform;
			deadWorldItem.gameObject.layer 			= Globals.LayerNumHidden;
			deadWorldItem.transform.localPosition 	= Vector3.zero;
			deadWorldItem.StopAllCoroutines ();
			deadWorldItem.gameObject.SetActive (false);
			
			GameObject.Destroy (deadWorldItem.gameObject, 1.0f);
		}
	}
	
	public static void 			SendWorldItemToGraveYard (WorldItem deadWorldItem)
	{
		mIncoming.Enqueue (deadWorldItem);
	}
	
	protected static Queue <WorldItem> mIncoming = new Queue <WorldItem> ( );
}

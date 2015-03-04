using UnityEngine;
using System.Collections;
using Frontiers;


public class WaterSubmergePlayer : MonoBehaviour
{
	public void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.layer != Globals.LayerNumPlayer)
		{
			return;
		}
				
//		//Debug.Log ("Player is underwater");
		Player.Get.AvatarActions.ReceiveAction ((AvatarAction.MoveEnterWater), WorldClock.AdjustedRealTime);
	}
	
	public void OnTriggerExit (Collider other)
	{
		if (other.gameObject.layer != Globals.LayerNumPlayer)
		{
			return;
		}
//		//Debug.Log ("Player is not underwater");
		Player.Get.AvatarActions.ReceiveAction ((AvatarAction.MoveExitWater), WorldClock.AdjustedRealTime);
	}
}

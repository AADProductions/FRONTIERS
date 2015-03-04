using UnityEngine;
using System.Collections;
using Frontiers;

public class CharacterHeadTrackPlayer : MonoBehaviour
{

	public float 		MinimimRange 		= 8.0f;
	public float 		MinRotation 		= -70.0f;
	public float 		MaxRotation 		= 70.0f;
	public Vector3		UpVector;
	public Transform	HeadTransform;
	
	public void			Start ( )
	{
//		HeadTransform = transform.FindChild ("Fbx01_Head");
	}
	
	// Update is called once per frame
	void Update ( )
	{
//		if (GameManager.GameState != FrontiersGameState.InWorld || HeadTransform == null)
//		{
//			return;
//		}
		
		Quaternion startRotation 	= HeadTransform.transform.localRotation;
		Quaternion lookRotation 	= Quaternion.identity;
		
		if (Vector3.Distance (Player.Local.Position, HeadTransform.transform.position) <= MinimimRange)
		{
			HeadTransform.transform.LookAt (Player.Local.EyePosition, UpVector);
			
			float xRotation = HeadTransform.transform.localRotation.eulerAngles.x;
			if (xRotation > 180)
			{
				xRotation -= 360.0f;
			}
			
			lookRotation = HeadTransform.transform.localRotation;
			if ((xRotation > MaxRotation) && (xRotation < MinRotation))
			{
				lookRotation.x = xRotation;
			}
		}
		
		HeadTransform.transform.localRotation = Quaternion.Lerp (startRotation, lookRotation, 0.15f);
	}
}

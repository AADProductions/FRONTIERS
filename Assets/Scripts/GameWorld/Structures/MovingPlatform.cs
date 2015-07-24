using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
	//I hate to have this thing calculate its velocity every frame
	//but I don't want to deal with dynamic worlditems turning it off and on
	//especially since there are only 2-3 in the world at any given time
	//this value is used by the player controller in FixedMove
	public Transform tr;
	public Vector3 CurrentVelocity = Vector3.zero;
	public Vector3 CurrentPosition = Vector3.zero;
	[HideInInspector]
	public Vector3 PreviousPosition;
	public bool YOnly = true;

	public void Start ()
	{
		tr = transform;
		tr.gameObject.layer = Globals.LayerNumSolidTerrain;
		PreviousPosition = tr.position;
		CurrentVelocity = Vector3.zero;
	}

	public void FixedUpdate ()
	{
		PreviousPosition = CurrentPosition;
		CurrentPosition = tr.position;
		CurrentVelocity = CurrentPosition - PreviousPosition;

		if (YOnly) {
			CurrentVelocity.x = 0f;
			CurrentVelocity.z = 0f;
		}

		//don't bother with negative y velocities
		//controller will take care of that
		if (CurrentVelocity.y < 0) {
			CurrentVelocity.y = 0f;
		}
	}
}

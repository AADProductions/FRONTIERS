using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
		//I hate to have this thing calculate its velocity every frame
		//but I don't want to deal with dynamic worlditems turning it off and on
		//especially since there are only 2-3 in the world at any given time
		//this value is used by the player controller in FixedMove
		public Transform tr;
		public Vector3 VelocityLastFrame = Vector3.zero;
		public Vector3 PositionLastFrame = Vector3.zero;
		public bool YOnly = true;

		public void Start()
		{
				tr = transform;
				PositionLastFrame = tr.position;
				VelocityLastFrame = Vector3.zero;
		}

		public void Update()
		{
				VelocityLastFrame = tr.position - PositionLastFrame;
				PositionLastFrame = tr.position;

				if (YOnly) {
						VelocityLastFrame.x = 0f;
						VelocityLastFrame.z = 0f;
				}

				//don't bother with negative y velocities
				//controller will take care of that
				if (VelocityLastFrame.y < 0) {
						VelocityLastFrame.y = 0f;
				}
		}
}

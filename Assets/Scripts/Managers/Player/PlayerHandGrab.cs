using UnityEngine;
using System.Collections;

public class PlayerHandGrab : MonoBehaviour
{
		//this will eventually be used for moving the character avater's hand
		//to grip any tools the player is holding
		//probably won't get to this for a while though
		public Animator animator;
		public bool HandIKkActive = false;
		public bool HeadIKActive = true;
		public Transform rightHandObj = null;
		public Transform HeadTransform;

		public void OnAnimatorMove()
		{
				//Debug.Log ("OnAnimatorMove Getting called");
		}

		public void OnAnimatorIK()
		{
				if (HandIKkActive) {

						//weight = 1.0 for the right hand means position and rotation will be at the IK goal (the place the character wants to grab)
						animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
						animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

						//set the position and the rotation of the right hand where the external object is
						if (rightHandObj != null) {
								animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
								animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
						}
				}
		}
}

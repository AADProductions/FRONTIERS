using UnityEngine;
using System.Collections;

namespace Frontiers {
	public class PlayerGrabber : MonoBehaviour
	{
		public Transform Target;

		public void Awake ( )
		{
			RB = gameObject.AddComponent <Rigidbody> ();
			RB.isKinematic = false;
			RB.useGravity = false;
			RB.detectCollisions = true;
			RB.constraints = RigidbodyConstraints.FreezeRotation;
			RB.interpolation = RigidbodyInterpolation.Interpolate;

			Joint = gameObject.AddComponent <FixedJoint> ();
			Joint.breakForce = Mathf.Infinity;
			Joint.breakTorque = Mathf.Infinity;
			Joint.autoConfigureConnectedAnchor = false;
			Joint.anchor = Vector3.zero;
			Joint.connectedAnchor = Vector3.zero;
			Joint.enableCollision = true;

			tr = transform;
		}

		public void FixedUpdate ( )
		{
			if (GameManager.Is (FGameState.InGame)) {
				RB.velocity = (Target.position - RB.position) * 24f;
			}
		}

		public Vector3 Position {
			get {
				return RB.position;
			}
			set {
				RB.MovePosition (value);
			}
		}

		public Rigidbody RB;
		public FixedJoint Joint;
		protected Transform tr;
	}
}
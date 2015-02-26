using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;
using System.Collections.Generic;

public class BodyPart : MonoBehaviour
{
		public IItemOfInterest Owner;
		public Transform tr;
		public BodyPartType Type = BodyPartType.Head;
		public BodyPart ParentPart = null;
		public bool IgnoreOnConvertToRagdoll;
		public string FXOnDestroy;
		public Collider PartCollider;
		public Vector3 LocalPositionBeforeRagdoll;
		public Quaternion LocalRotationBeforeRagdoll;
		public Rigidbody RagdollRB = null;
		public Vector3 ForceOnConvertToRagdoll {
				set {
						if (RagdollRB != null) {
								RagdollRB.AddForce(value * Globals.BodyPartDamageForceMultiplier);
						} else {
								mForceOnConvertToRagdoll = value;
						}
				}
		}

		public void ConvertToRagdoll()
		{
				if (RagdollRB != null) {
						//already converted
						return;
				}

				if (IgnoreOnConvertToRagdoll) {
						return;
				}

				LocalPositionBeforeRagdoll = tr.localPosition;
				LocalRotationBeforeRagdoll = tr.localRotation;

				GameObject ragdollRbObject = gameObject;//new GameObject (Type.ToString ());
				gameObject.layer = Globals.LayerNumWorldItemActive;//make sure it collides with the ground

				PartCollider.enabled = true;
				RagdollRB = ragdollRbObject.GetOrAdd <Rigidbody>();
				RagdollRB.isKinematic = false;
				RagdollRB.useGravity = true;
				RagdollRB.velocity = Vector3.zero;
				RagdollRB.angularVelocity = Vector3.zero;
				RagdollRB.mass = 1f;
				RagdollRB.drag = 0.95f;
				RagdollRB.angularDrag = 0.95f;
				RagdollRB.collisionDetectionMode = CollisionDetectionMode.Continuous;

				enabled = true;
		}

		public void LinkRagdollParts(WorldBody body)
		{
				if (Type != BodyPartType.Chest && RagdollRB != null) {
						if (ParentPart == null) {
								if (!body.GetBodyPart(BodyPartType.Chest, out ParentPart)) {
										if (Type != BodyPartType.Hip) {
												body.GetBodyPart(BodyPartType.Hip, out ParentPart);
										}
								}
						}

						if (ParentPart != null && ParentPart.RagdollRB != null) {
								//RagdollRB.constraints = RigidbodyConstraints.FreezeAll;
								CharacterJoint joint = gameObject.AddComponent<CharacterJoint>();//ParentPart.RagdollRB.gameObject.AddComponent <ConfigurableJoint>();
								joint.connectedBody = ParentPart.RagdollRB;//RagdollRB;
								//RagdollRB.constraints = RigidbodyConstraints.FreezePosition;
								//joint.connectedBody = RagdollRB;
								joint.breakForce = Mathf.Infinity;
								joint.enableCollision = false;
						}

						RagdollRB.WakeUp();
				}
		}

		public void ConvertToAnimated()
		{
				if (RagdollRB != null) {
						GameObject.Destroy(RagdollRB);
						CharacterJoint [] joints = gameObject.GetComponents<CharacterJoint>();
						for (int i = 0; i < joints.Length; i++) {
								GameObject.Destroy(joints[i]);
						}
				}

				tr.localPosition = LocalPositionBeforeRagdoll;
				tr.localRotation = LocalRotationBeforeRagdoll;
				if (Type == BodyPartType.Head) {
						gameObject.layer = Globals.LayerNumAwarenessReceiver;//the head is what listens for sound
				} else {
						gameObject.layer = Globals.LayerNumBodyPart;
				}
				PartCollider.enabled = true;
				enabled = false;
		}

		public void Awake()
		{
				enabled = false;
				gameObject.layer = Globals.LayerNumWorldItemActive;
				tr = transform;
				PartCollider = collider;
				PartCollider.enabled = true;
		}

		public void Initialize(IItemOfInterest newOwner, List <BodyPart> otherBodyParts)
		{
				switch (Type) {
						case BodyPartType.Head:
								gameObject.tag = "BodyHead";
								gameObject.layer = Globals.LayerNumAwarenessReceiver;//the head is what listens for sound
								break;

						case BodyPartType.Face:
						case BodyPartType.Neck:
								gameObject.tag = "BodyHead";
								break;
			
						case BodyPartType.Arm:
						case BodyPartType.Hand:
						case BodyPartType.Finger:
						case BodyPartType.Wrist:
								gameObject.tag = "BodyArm";
								break;
			
						case BodyPartType.Leg:
						case BodyPartType.Foot:
						case BodyPartType.Shin:
								gameObject.tag = "BodyLeg";
								break;
			
						case BodyPartType.Chest:
						case BodyPartType.Shoulder:
						case BodyPartType.Hip:
								gameObject.tag = "BodyTorso";
								break;
			
						default:
								gameObject.tag = "BodyGeneral";
								break;			
				}

				//which kind of collider are we?
				PartCollider.isTrigger = false;	
				mColliderType = PartCollider.GetType();
			
				Owner = newOwner;

				for (int i = 0; i < otherBodyParts.Count; i++) {
						if (otherBodyParts[i] != this) {
								Physics.IgnoreCollision(PartCollider, otherBodyParts[i].PartCollider);
						}
				}

				if (Owner == null) {
						gameObject.layer = Globals.LayerNumScenery;
				} else if (Owner.IOIType == ItemOfInterestType.Player) {
						gameObject.layer = Globals.LayerNumPlayer;
				} else {
						gameObject.layer = Globals.LayerNumBodyPart;
				}
		}

		public void FixedUpdate(){

				if (RagdollRB != null && mForceOnConvertToRagdoll != Vector3.zero) {
						RagdollRB.AddForce (mForceOnConvertToRagdoll * Globals.BodyPartDamageForceMultiplier);
						mForceOnConvertToRagdoll = Vector3.zero;
				}
		}

		public void LateUpdate() {
				/*if (Type != BodyPartType.Hip && Type != BodyPartType.Chest && RagdollRB != null && ParentPart != null) {
						tr.localPosition = LocalPositionBeforeRagdoll;
				}*/
		}

		public void OnDrawGizmos()
		{
				if (RagdollRB != null) {
						Gizmos.color = Color.red;
						Gizmos.DrawSphere(RagdollRB.worldCenterOfMass, 0.1f);
				}
		}

		protected Type mColliderType;
		protected Vector3 mForceOnConvertToRagdoll;
		protected static Type gCapsuleColliderType = typeof(CapsuleCollider);
		protected static Type gBoxColliderType = typeof(BoxCollider);
		protected static Type gSphereColliderType = typeof(SphereCollider);
}

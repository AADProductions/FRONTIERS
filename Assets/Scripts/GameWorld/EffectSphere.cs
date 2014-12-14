using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using Frontiers;
using Frontiers.World;

public class EffectSphere : MonoBehaviour
{		//used by skills and other items to see what's in the immediate area
		//kind of like a looker / listener
		//but it expands over time
		public Action OnIntersectItemOfInterest;
		public Action OnDepleted;
		public bool Depleted = false;
		public float TargetRadius = 1.0f;

		public float NormalizedRadius {
				get {
						return Collider.radius / TargetRadius;
				}
		}

		public float NormalizedExpansion = 0f;
		public double StartTime = -1;
		public double RTDuration;
		public double RTExpansionTime = 0.5f;
		public double RTCooldownTime = 1.0f;
		public bool RequireLineOfSight = true;
		public int OcclusionLayerMask = Globals.LayerSolidTerrain | Globals.LayerObstacleTerrain;
		public bool IsInEffect = false;
		public SphereCollider Collider;
		public Queue <IItemOfInterest> ItemsOfInterest = new Queue <IItemOfInterest>();
		public bool Canceled = false;
		public Rigidbody rb = null;
		public Transform tr = null;

		public void Awake()
		{
				Collider = gameObject.GetOrAdd <SphereCollider>();
				Collider.enabled = true;
				Collider.radius = 0.05f;
				Collider.isTrigger = true;
				Collider.gameObject.layer = Globals.LayerNumTrigger;
				rb = gameObject.GetOrAdd <Rigidbody>();
				//to detect other kinematic rigid bodies it must NOT be kinematic
				rb.isKinematic = false;
				rb.useGravity = false;
				rb.detectCollisions = true;
				tr = transform;
		}

		public virtual void Start()
		{
				StartTime = WorldClock.AdjustedRealTime;

				IsInEffect = true;

				if (!mUpdatingEffectSphereOverTime) {
						mUpdatingEffectSphereOverTime = true;
						StartCoroutine(UpdateEffectSphereOverTime());
				}
		}

		public IEnumerator UpdateEffectSphereOverTime()
		{
				while (!Canceled && (WorldClock.AdjustedRealTime - StartTime < RTExpansionTime)) {
						NormalizedExpansion = (float)((WorldClock.AdjustedRealTime - StartTime) / RTExpansionTime);
						Collider.radius = Mathf.Lerp(0.01f, TargetRadius, NormalizedExpansion);
						OnUpdateRadius();
						yield return null;
						CheckItemsOfInterest();
				}

				Collider.radius = TargetRadius;

				while (!Canceled && WorldClock.AdjustedRealTime < StartTime + RTDuration) {
						OnUpdateRadius();
						yield return null;
						CheckItemsOfInterest();
				}

				IsInEffect = false;
				mCooldownStartTime = WorldClock.AdjustedRealTime;
				mCooldownEndTime = WorldClock.AdjustedRealTime + RTCooldownTime;
				yield return null;
				CheckItemsOfInterest();

				//do the cooldown even if the effect is canceled
				while (!Canceled && WorldClock.AdjustedRealTime >= (StartTime + RTDuration) + RTCooldownTime) {
						OnCooldown();
						yield return null;
				}

				Depleted = true;
				OnDepleted.SafeInvoke();

				Collider.enabled = false;
				GameObject.Destroy(gameObject, 1.0f);
				yield break;
		}

		public void OnTriggerEnter(Collider other)
		{
				if (!IsInEffect) {
						return;
				}

				bool intersection = true;
				if (RequireLineOfSight) {
						Debug.Log("Requires line of sight, checking now...");
						//check if we can see it
						RaycastHit hitInfo;
						if (Physics.Raycast(
								    tr.position,
								    Vector3.Normalize(tr.position - other.transform.position),
								    out hitInfo,
								    Collider.radius * 1.25f,
								    OcclusionLayerMask)) {
								//we hit one of our occlusion layers on our way to the object
								//so this doesn't count as a hit
								intersection = false;
						}
				}
				IItemOfInterest ioi = null;
				if (WorldItems.GetIOIFromCollider(other, out ioi)) {
						ItemsOfInterest.SafeEnqueue(ioi);
				}
		}

		public void CheckItemsOfInterest()
		{
				if (!IsInEffect) {
						return;
				}

				if (ItemsOfInterest.Count > 0) {
						OnIntersectItemOfInterest.SafeInvoke();
						//OnIntersectItem is assumed to clear the queue
						//just in case, clear it anyway
				}
				ItemsOfInterest.Clear();
		}

		public void CancelEffect()
		{
				Canceled = true;
		}

		protected virtual void OnUpdateRadius()
		{
				return;
		}

		protected virtual void OnCooldown()
		{
				return;
		}

		protected double mCooldownStartTime = 0f;
		protected double mCooldownEndTime = 0f;
		protected bool mUpdatingEffectSphereOverTime = false;
}

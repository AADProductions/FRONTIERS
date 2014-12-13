using UnityEngine;
using System.Collections;
using Frontiers;
using ExtensionMethods;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class AwarenessBubble <T> : MonoBehaviour where T : IAwarenessBubbleUser
		{
				public T ParentObject;
				public CapsuleCollider Collider;
				public Rigidbody rb;
				//public List <IVisible> VisibleItems = new List <IVisible> ();
				public bool IsInUse {
						get {
								if (ParentObject == null) {
										FinishUsing();
								} else if (mUsedAtLeastOneCycle && WorldClock.Time > (mUseStartTime + (WorldClock.RTSecondsToGameSeconds(mRTDuration)))) {
										FinishUsing();
								}
								return mIsInUse;
						}
				}

				public virtual void Awake()
				{
						mTransform = transform;
						gameObject.layer = Globals.LayerNumAwarenessReceiver;
						rb = gameObject.AddComponent <Rigidbody>();
						rb.isKinematic = true;
						rb.detectCollisions = false;
						Collider = gameObject.GetOrAdd <CapsuleCollider>();
						Collider.enabled = false;
						Collider.isTrigger = true;
				}

				public void StartUsing(T parentObject, List <Collider> ignoreColliders, float duration)
				{
						if (mIsInUse) {
								if (ParentObject.transform != parentObject.transform) {
										//if we're getting a new parent
										//interrupt and finish first
										//then proceed normally
										OnInterruptUsing();
										FinishUsing();
								}
						}

						ParentObject = parentObject;
						mParentTrans = ParentObject.transform;
						rb.position = mParentTrans.position;
						rb.rotation = mParentTrans.rotation;

						//finish using time will be set in the first fixed update
						//that way it won't be over before we've started
						mUsedAtLeastOneCycle = false;
						mUseStartTime = Mathf.Infinity;
						mRTDuration = duration;
						mIsInUse = true;
						enabled = true;
						rb.detectCollisions = true;
						Collider.enabled = true;

						//this will keep us from seeing ourselves
						if (ignoreColliders != null) {
								for (int i = 0; i < ignoreColliders.Count; i++) {
										if (ignoreColliders[i] != null && ignoreColliders[i].enabled) {
												Physics.IgnoreCollision(Collider, ignoreColliders[i]);
										}
								}
						}

						OnStartUsing();
				}

				public void FinishUsing()
				{
						mIsInUse = false;
						enabled = false;
						rb.detectCollisions = false;
						Collider.enabled = false;
						OnFinishUsing();
				}

				public void OnTriggerEnter(Collider other)
				{
						if (!mIsInUse)//something weird has happened, stop looking
				return;

						if (other.isTrigger) {
								//the only triggers we care about are action nodes
								//TODO figure out how to look for them later, for now just filter out all triggers
								return;
						}

						HandleEncounter(other);
				}

				public void FixedUpdate()
				{
						if (!mUsedAtLeastOneCycle) {
								mUseStartTime = WorldClock.Time;
								mUsedAtLeastOneCycle = true;
						}

						rb.position = mParentTrans.position;
						rb.rotation = mParentTrans.rotation;

						OnUpdateAwareness();

						if (!IsInUse) {
								FinishUsing();
						}
				}

				protected virtual void OnInterruptUsing()
				{

				}

				protected virtual void OnUpdateAwareness()
				{

				}

				protected virtual void OnStartUsing()
				{

				}

				protected virtual void OnFinishUsing()
				{

				}

				protected virtual void HandleEncounter(Collider other)
				{

				}

				public void OnDestroy()
				{

				}

				protected bool mUsedAtLeastOneCycle = false;
				protected double mUseStartTime = 0f;
				protected double mRTDuration = 0f;
				protected Transform mTransform = null;
				protected Transform mParentTrans = null;
				protected bool mIsInUse = false;
		}
}
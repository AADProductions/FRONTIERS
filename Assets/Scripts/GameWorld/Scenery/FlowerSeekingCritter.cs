﻿﻿﻿﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	//a critter that looks for flowers
	//is the base of butterfly
	public class FlowerSeekingCritter : Critter
	{
		public WorldPlant CurrentFlower;
		public WorldPlant LastFlower;
		public double TimeStartedResting = -1f;
		public double StartleTimeEnd = -1;
		public float LandingDistance = 3f;
		public float RestOnFlowerTime = 3f;
		public bool WaitingForFlower = false;
		public bool RestingOnFlower = false;
		public float StartleThreshold = 2f;
		public bool CurrentFlowerHasMoved {
			get {
				return CurrentFlower == null || mCurrentFlowerPosition != CurrentFlower.worlditem.Position;
			}
		}
		#if UNITY_EDITOR
		public override void Start ()
		{
			base.Start ();
			Flies = true;
			StartCoroutine (FindFlowersOverTime ());
		}

		public override void UpdateMovement (Vector3 playerPosition)
		{
			if (CurrentFlower != null && !CurrentFlowerHasMoved) {
				if (!Friendly && (Vector3.Distance (playerPosition, mCurrentPosition) < StartleThreshold) && !Player.Local.IsCrouching) {
					CurrentFlower = null;
					RestingOnFlower = false;
					WaitingForFlower = false;
					rb.isKinematic = false;
					base.UpdateMovement (playerPosition);
					StartleTimeEnd = WorldClock.AdjustedRealTime + RestOnFlowerTime;
					return;
				}

				if (RestingOnFlower) {
					if (WorldClock.AdjustedRealTime < (TimeStartedResting + RestOnFlowerTime)) {
						//wait on the flower and don't apply forces
						//rb.isKinematic = true;
						//move it spot onto the flower
						rb.isKinematic = true;
						mCurrentPosition = Vector3.Lerp (mCurrentPosition, mRandomFlowerPosition, 0.125f);
						rb.MovePosition (mCurrentPosition);
					} else {
						//done!
						rb.isKinematic = false;
						RestingOnFlower = false;
						WaitingForFlower = false;
						if (CurrentFlower != null && CurrentFlower.OccupyingCritter == this) {
							CurrentFlower.OccupyingCritter = null;
						}
						LastFlower = CurrentFlower;
						CurrentFlower = null;
						base.UpdateMovement (playerPosition);
					}
				} else {
					//move towards the flower
					rb.isKinematic = false;
					if (FlyTowardsFlower (playerPosition, WaitingForFlower)) {
						//if we're close enough to land next frame we'll go kinematic	
						RestingOnFlower = true;
						TimeStartedResting = WorldClock.AdjustedRealTime;
					}
				}
			} else {
				rb.isKinematic = false;
				//set it to null in case it's just inactive
				RestingOnFlower = false;
				//rb.isKinematic = false;
				base.UpdateMovement (playerPosition);
			}
		}

		protected bool FlyTowardsFlower (Vector3 playerPosition, bool waitingForFlower) {
			rb.AddForce (mForceDirection);
			//return true if we're close enough to the flower to rest on it and we're not waiting
			mCurrentPosition = rb.position;
			if (!waitingForFlower) {
				float distanceToFlower = Vector3.Distance (mCurrentPosition, mRandomFlowerPosition);
				if (distanceToFlower < LandingDistance) {
					return true;
				}
				//y force variation lessens the closer we get to the flower
				mForceDirection = (mRandomFlowerPosition - mCurrentPosition).normalized;
				if (mRandomFlowerPosition.y < mCurrentPosition.y) {
					mForceDirection.y = Mathf.Max (mForceDirection.y + MaxSpeed);
				}
			} else {
				//y force variation is normal
				mForceDirection = (mRandomFlowerPosition - mCurrentPosition).normalized;
				mForceDirection.y = (Mathf.Max (playerPosition.y, mRandomFlowerPosition.y) + MaxSpeed) - mCurrentPosition.y;
			}
			if (waitingForFlower) {
				//if we're waiting for the flower fly a bit wonky
				if (WorldClock.AdjustedRealTime > mNextChangeTime) {
					mNextChangeTime = WorldClock.AdjustedRealTime + ChangeDirectionInterval + UnityEngine.Random.value;
					mRandomForceDirection.x = UnityEngine.Random.Range (-MaxSpeed, MaxSpeed);
					mRandomForceDirection.z = UnityEngine.Random.Range (-MaxSpeed, MaxSpeed);
				}
				mForceDirection = Vector3.Lerp ((mRandomFlowerPosition - mCurrentPosition).normalized, mRandomForceDirection, 0.65f);
			}
			gVelocityCheck = rb.velocity;
			mSmoothVelocity = Vector3.Lerp (mSmoothVelocity, gVelocityCheck, 0.1f);
			if (mSmoothVelocity != Vector3.zero) {
				rb.MoveRotation (Quaternion.LookRotation (mSmoothVelocity));
			}
			return false;
		}

		protected IEnumerator FindFlowersOverTime ()
		{
			//give ourselves a second before looking for a flower
			double waitUntil = WorldClock.AdjustedRealTime + 1f;
			while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
				yield return null;
			}

			while (!mDestroyed) {
				while (RestingOnFlower) {
					//don't get a flower we're not resting on
					yield return null;
				}

				while (WorldClock.AdjustedRealTime < StartleTimeEnd) {
					yield return null;
				}

				while (CurrentFlower == null) {
					//find something between the player and the critter, closer if it's friendlier
					if (Plants.Get.NearestFloweringPlant (Player.Local.Position, 5f, LastFlower, ref CurrentFlower)) {//, mCurrentPosition, Friendly ? 0.1f : 0.8f), 4f, LastFlower, ref CurrentFlower)) {
						mCurrentFlowerPosition = CurrentFlower.worlditem.Position;
						mRandomFlowerPosition = CurrentFlower.GetRandomFlowerPosition ();
						//wait by default, we'll find out if we can occupy it later
						WaitingForFlower = true;
						mMaxWaitTime = WorldClock.AdjustedRealTime + (RestOnFlowerTime * 2);
					} else {
						waitUntil = WorldClock.AdjustedRealTime + 0.5f;
						while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
							yield return null;
						}
					}
				}

				while (WaitingForFlower) {
					if (CurrentFlowerHasMoved) {
						//whoops, we'll have to start over again
						CurrentFlower = null;
						WaitingForFlower = false;
						yield return null;
					} else {
						//see if it's occupied
						if (!CurrentFlower.IsOccupied) {
							CurrentFlower.OccupyingCritter = this;
							WaitingForFlower = false;
							yield return null;
						} else {
							if (WorldClock.AdjustedRealTime > mMaxWaitTime) {
								WaitingForFlower = false;
								CurrentFlower = null;
							} else {
								waitUntil = WorldClock.AdjustedRealTime + (Random.value * 0.5f) + 0.1f;
								while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
									yield return null;
								}
							}
						}
					}
				}

				waitUntil = WorldClock.AdjustedRealTime + 0.5f;
				while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
					yield return null;
				}
				yield return null;
			}
		}

		void OnDrawGizmos () {
			if (CurrentFlower == null) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube (mCurrentPosition, Vector3.one * 0.1f);
			} else {
				Gizmos.color = Color.blue;
				if (WaitingForFlower) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube (mCurrentPosition, Vector3.one * 0.1f);
				} else if (RestingOnFlower) {
					Gizmos.color = Color.green;
					Gizmos.DrawWireCube (mCurrentPosition, Vector3.one * 0.1f);
				}
				Gizmos.DrawSphere (mRandomFlowerPosition, 0.25f);
				Gizmos.DrawLine (mCurrentPosition, mRandomFlowerPosition);
			}
		}
		#endif

		protected Vector3 mCurrentFlowerPosition;
		protected Vector3 mRandomFlowerPosition;
		protected Vector3 mRandomForceDirection;
		protected WorldPlant mLastFlower;
		protected double mMaxWaitTime;
	}
}

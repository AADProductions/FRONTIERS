using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	//a critter that seeks out valuable items
	//used for special friendly butterflies
	public class TreasureSeekingCritter : Critter
	{
		public static List <string> ValuableScripts = new List<string> {
			"Currency",
			"Luminite",
			"Hidden",
			"Artifact",
			"ArtifactShard",
			"DestructableWall",
			"Secret"//TODO create a secret script for these things to find
		};

		public WorldItem CurrentValuable;
		public WorldItem LastValuable;
		public float SearchRadius = 3f;
		public double TimeStartedResting = -1f;
		public double StartleTimeEnd = -1;
		public float LandingDistance = 3f;
		public float StartleThreshold = 2f;
		public bool CurrentValuableHasMoved {
			get {
				return CurrentValuable == null || mCurrentValuablePosition != CurrentValuable.worlditem.Position;
			}
		}
		bool valuableIsHeldByPlayer = false;

		#if UNITY_EDITOR
		public override void Start ()
		{
			base.Start ();
			Flies = true;
			StartCoroutine (FindValuablesOverTime ());
		}

		public void Update () {

		}

		public override void UpdateMovement (Vector3 playerPosition)
		{
			if (CurrentValuable != null) {
				if (Vector3.Distance (CurrentValuable.FocusPosition, mCurrentPosition) > SearchRadius) {
					CurrentValuable = null;
				} else if (!valuableIsHeldByPlayer && CurrentValuable.FocusPosition != mCurrentValuablePosition) {
					CurrentValuable = null;
				} else {
					rb.isKinematic = false;
					if (valuableIsHeldByPlayer) {
						mForceDirection = (CurrentValuable.FocusPosition - mCurrentPosition);
					} else {
						mForceDirection = (mRandomValuablePosition - mCurrentPosition);
					}
					mForceDirection += Random.onUnitSphere * 0.25f;
					mForceDirection.Normalize ();
					base.UpdateMovement (playerPosition);
				}
			}
		}

		protected IEnumerator FindValuablesOverTime ()
		{
			//give ourselves a second before looking for a flower
			double waitUntil = WorldClock.AdjustedRealTime + 1f;
			while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
				yield return null;
			}

			while (!mDestroyed) {
				while (WorldClock.AdjustedRealTime < StartleTimeEnd) {
					yield return null;
				}

				while (GameWorld.Get.PrimaryChunk == null) {
					yield return null;
				}

				if (CurrentValuable == null) {
					List <WorldItem> valuables = new List <WorldItem> ();
					//find something between the player and the critter, closer if it's friendlier
					//TODO make this work in immediate groups
					var searchEnum = WIGroups.GetAllChildrenByTypeInLocation (GameWorld.Get.PrimaryChunk.AboveGroundGroup.Path, ValuableScripts, valuables, Position, SearchRadius, false, 1);
					while (searchEnum.MoveNext ()) {
						yield return searchEnum.Current;
					}
					if (valuables.Count > 0) {
						valuableIsHeldByPlayer = false;
						CurrentValuable = valuables [0];
						mCurrentValuablePosition = valuables [0].worlditem.FocusPosition;
						mRandomValuablePosition = valuables [0].worlditem.FocusPosition;
					}
				}

				if (CurrentValuable == null) {
					//find something between the player and the critter, closer if it's friendlier
					//TODO make this work in immediate groups
					List <WorldItem> valuables = WIGroups.Get.Player.GetChildrenOfType (ValuableScripts);
					if (valuables.Count > 0) {
						valuableIsHeldByPlayer = true;
						CurrentValuable = valuables [0];
						mCurrentValuablePosition = valuables [0].worlditem.FocusPosition;
						mRandomValuablePosition = valuables [0].worlditem.FocusPosition;
					}
				}

				waitUntil = WorldClock.AdjustedRealTime + 1.5f;
				while (WorldClock.AdjustedRealTime < waitUntil && !mDestroyed) {
					yield return null;
				}
				yield return null;
			}
		}

		void OnDrawGizmos () {
			if (CurrentValuable == null) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube (mCurrentPosition, Vector3.one * 0.1f);
			} else {
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube (mCurrentPosition, Vector3.one * 0.1f);
				Gizmos.DrawSphere (mRandomValuablePosition, 0.25f);
				Gizmos.DrawLine (mCurrentPosition, mRandomValuablePosition);
			}
		}
		#endif

		protected Vector3 mCurrentValuablePosition;
		protected Vector3 mRandomValuablePosition;
		protected Vector3 mRandomForceDirection;
		protected WorldItem mLastValuable;
	}
}

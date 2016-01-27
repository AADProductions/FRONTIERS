using UnityEngine;
using System.Collections;
using Frontiers.Data;
using Frontiers.World.WIScripts;

namespace Frontiers.World {
	public class ElevatorTriggerLoad : MonoBehaviour {
		public MobileReference TargetStructureReference;
		public StructureLoadState TargetLoadState = StructureLoadState.InteriorLoaded;
		public Structure TargetStructure;
		public Transform tr;
		double mPaddedStartTime = -1;

		public bool WaitForStructure ( ) {
			if (TargetStructure == null) {
				WorldItem childItem = null;
				if (WIGroups.FindChildItem (TargetStructureReference.GroupPath, TargetStructureReference.FileName, out childItem)) {
					TargetStructure = childItem.Get <Structure> ();
					if (TargetStructure != null) {
						TargetStructure.worlditem.ActiveState = WIActiveState.Active;
						//TODO remove this lock, could be dangerous
						TargetStructure.worlditem.ActiveStateLocked = true;
						Structures.AddInteriorToLoad (TargetStructure);
					} else {
						Debug.LogError ("Target structure " + TargetStructureReference.FullPath + " had no structure wiscript");
						return true;
					}
				} else {
					Debug.LogError ("Couldn't find structure " + TargetStructureReference.FullPath);
					return true;
				}
			}

			if (TargetStructure.Is (TargetLoadState)) {
				if (mPaddedStartTime < 0) {
					mPaddedStartTime = WorldClock.AdjustedRealTime + 1.5;
					return true;
				} else if (WorldClock.AdjustedRealTime > mPaddedStartTime) {
					return false;
				}
			} else {
				mPaddedStartTime = -1;
			}
			return true;
		}

		void Awake () {
			tr = transform;
		}

		void OnDrawGizmos () {
			if (tr == null) {
				tr = transform;
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube (tr.position, Vector3.one * 2f);
		}
	}
}
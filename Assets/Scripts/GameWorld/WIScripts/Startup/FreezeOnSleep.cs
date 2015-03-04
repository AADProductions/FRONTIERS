using UnityEngine;
using System.Collections;

namespace Frontiers.World {
	public class FreezeOnSleep : MonoBehaviour {

		public WorldItem worlditem;
		public double TimeStarted;

		public void Awake ()
		{
			TimeStarted = WorldClock.AdjustedRealTime;
		}

		public void FixedUpdate () {
			if (worlditem.Destroyed) {
				return;
			}

			if ((WorldClock.AdjustedRealTime > TimeStarted + worlditem.Props.Local.FreezeTimeout) || worlditem.rb.IsSleeping ()) {
				worlditem.rb.isKinematic = true;
				GameObject.Destroy (this);
			}
		}
	}
}
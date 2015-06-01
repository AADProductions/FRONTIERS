using UnityEngine;
using System.Collections;

namespace Frontiers.World {
	public class WorldLightMessenger : MonoBehaviour {

		public WorldLight Owner;

		public void OnTriggerEnter (Collider other)
		{
			if (other.isTrigger && !other.CompareTag (Globals.TagLightSensitiveTrigger))
				return;

			LightManager.OnTriggerEnter (Owner, other);
		}

		public void OnTriggerExit (Collider other)
		{
			if (other.isTrigger && !other.CompareTag (Globals.TagLightSensitiveTrigger))
				return;

			LightManager.OnTriggerExit (Owner, other);
		}
	}
}
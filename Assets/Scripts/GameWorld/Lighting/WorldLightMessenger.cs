using UnityEngine;
using System.Collections;

namespace Frontiers.World {
	public class WorldLightMessenger : MonoBehaviour {

		public WorldLight Owner;

		public void OnTriggerEnter (Collider other)
		{
			if (other.isTrigger)
				return;

			LightManager.OnTriggerEnter (Owner, other);
		}

		public void OnTriggerExit (Collider other)
		{
			if (other.isTrigger)
				return;

			LightManager.OnTriggerExit (Owner, other);
		}
	}
}
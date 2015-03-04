using UnityEngine;
using System.Collections;

namespace Frontiers.World {
	public class WorldLightMessenger : MonoBehaviour {

		public WorldLight Owner;

		public void OnTriggerEnter (Collider other)
		{
			LightManager.OnTriggerEnter (Owner, other);
		}

		public void OnTriggerExit (Collider other)
		{
			LightManager.OnTriggerExit (Owner, other);
		}
	}
}
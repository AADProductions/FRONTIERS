using UnityEngine;
using System.Collections;

namespace Frontiers {
	public class MinorStructureContainer : MonoBehaviour {

		public MinorStructure Parent;

		public void OnGroupUnloaded ( ) {
			//Debug.Log ("On group unloaded called in minor structure container");
			if (Parent != null) {
				Parent.ClearStructure ();
			}
		}
	}
}
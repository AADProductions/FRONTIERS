using UnityEngine;
using System.Collections;

namespace Frontiers {
	public class EquippablePart : MonoBehaviour {

		public EquippableType Type = EquippableType.Weapon;

		public void OnDrawGizmos ( )
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (transform.position, 0.025f);
		}
	}
}
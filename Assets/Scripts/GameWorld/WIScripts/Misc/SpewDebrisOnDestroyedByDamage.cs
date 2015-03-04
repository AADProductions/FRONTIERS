using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
	public class SpewDebrisOnKilledByDamage : WIScript
	{
		public GameObject DebrisPrefab;
		
		public void OnDie ( )
		{
			GameObject.Instantiate (DebrisPrefab, transform.position, Quaternion.identity);
		}
	}
}
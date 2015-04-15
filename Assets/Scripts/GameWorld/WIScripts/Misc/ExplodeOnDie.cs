using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{
	public class ExplodeOnKilled : WIScript
	{
		public void OnDieFrom (string cause)
		{
			GameObject.Destroy (gameObject, 0.05f);
		}
	}
}
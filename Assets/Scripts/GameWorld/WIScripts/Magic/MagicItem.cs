using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
	public class MagicItem : WIScript
	{
		public GameObject Particles;
		
		public void OnUseAsTool ( )
		{
//			GameObject.Instantiate (Particles, Player.Local.Tool.worlditemDoppleGanger.transform.position, Quaternion.identity);
		}
	}
}
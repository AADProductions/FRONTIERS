using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
	public class MagicCurse : WIScript
	{
		public int stackEffects = 0;
		
		public void Inflict ( )
		{
			if (!worlditem.Is<Damageable> ( ))
			{	//if it's not damageable, forget it
				GameObject.Destroy (this);
				return;
			}		
			else
			{
				//worlditem.Get<Damageable> ( ).Curse = this;
			}
			
			stackEffects++;
		}
	}
}

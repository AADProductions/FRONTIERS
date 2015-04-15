using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
	public class FireStarter : WIScript
	{	
		[FrontiersFXAttribute]
		public string FXOnUse;
		public int TotalUses = 25;

		public FireStarterState State = new FireStarterState ();

		public void Use ( )
		{
			State.NumUses++;
			FXManager.Get.SpawnFX (worlditem.tr.position, FXOnUse);
			if (State.NumUses >= TotalUses) {
				worlditem.SetMode (WIMode.RemovedFromGame);
			}
		}
	}

	[Serializable]
	public class FireStarterState {
		public int NumUses = 0;
	}
}
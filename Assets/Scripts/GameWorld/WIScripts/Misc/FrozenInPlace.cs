using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{
	public class FrozenInPlace : WIScript {
		public override void OnInitialized ()
		{
			worlditem.OnAddedToPlayerInventory += OnAddedToPlayerInventory;
		}

		public override void OnModeChange ()
		{
			if (worlditem.Is (WIMode.World)) {
				worlditem.SetMode (WIMode.Frozen);
			}
		}

		public void OnAddedToPlayerInventory ( ) {
			Finish ();
		}
	}
}
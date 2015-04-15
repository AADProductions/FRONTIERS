using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts
{
	public class TimmornsBrew : WIScript {

		public override void OnInitialized ()
		{
			worlditem.OnPlayerUse += OnPlayerUse;
		}

		public void OnPlayerUse ()
		{
			Debug.Log ("On player interact in timmorns brew");
			if (worlditem.Is (WIMode.Equipped)) {
				Debug.Log ("We're equipped");
				//the only reason this will happen is if we're prompted by something
				//so insta-kill whatever we're looking at and distroy ourselves
				Damageable damageable = null;
				if (Player.Local.Surroundings.IsWorldItemInPlayerFocus && Player.Local.Surroundings.WorldItemFocus.worlditem.Is <Damageable> (out damageable)) {
					damageable.InstantKill (worlditem.DisplayName);
					worlditem.SetMode (WIMode.RemovedFromGame);
				}
			}
		}
	}
}
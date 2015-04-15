using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;


namespace Frontiers.World.WIScripts
{
		public class Obstruction : WIScript
		{
				public HashSet <string> ObstructedPaths = new HashSet <string>();

				public override void OnInitialized()
				{
						worlditem.OnPlayerEncounter += OnPlayerEncounter;
				}

				public void OnPlayerEncounter()
				{
						if (Paths.HasActivePath) {
								//TODO make it stop the player (?)
						}
				}
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class UpdateMap : Skill
		{
				public List <string> ImportantLocationTypes = new List <string>() { "City", "District", "Campsite", "Forest" };

				public override bool ActionUse(double timeStamp)
				{
						Usage.VisibleInInterface = false;
						if (GameManager.Is(FGameState.InGame)) {
								if (Player.Local.Surroundings.LastLocationRevealed != null
								&& ImportantLocationTypes.Contains(Player.Local.Surroundings.LastLocationRevealed.State.Type)) {
										Usage.VisibleInInterface = true;
								}
						}
						UseStart(true);
						return true;
				}
		}
}
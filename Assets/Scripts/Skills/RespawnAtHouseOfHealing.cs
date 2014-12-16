using UnityEngine;
using System.Collections;

namespace Frontiers.World.Gameplay
{
		public class RespawnAtHouseOfHealing : RespawnSkill
		{
				public override bool RequirementsMet {
						get {
								return true;
						}
				}

				public override bool RequiresAtLeastOneEquippedWorldItem {
						get {
								return false;
						}
				}

				public override bool ExtensionRequirementsMet {
						get {
								return true;
						}
				}

				protected override void RespawnPlayer(LocalPlayer player)
				{
						player.Position = player.Surroundings.LastPositionOnland;
						//TEMP
						//TODO get SpawnInClosestStructure working
						//for now just spawn in place
						player.Spawn();
						//SpawnManager.Get.SpawnInClosestStructure (Player.Local.Position, Player.Local.Surroundings.State.VisitedRespawnStructures, Player.Local.Status.OnRespawnBedFound);
				}

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						return targetObject.IOIType == ItemOfInterestType.Player;
				}
		}
}
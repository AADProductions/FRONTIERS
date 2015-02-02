using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
		public class FastTravel : Skill
		{
				public override void Initialize()
				{
						base.Initialize();
						Player.Get.AvatarActions.Subscribe(AvatarAction.PathEncounterObstruction, new ActionListener(PathEncounterObstruction));
				}

				public bool PathEncounterObstruction(double timeStamp)
				{
						if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused))
								return true;

						if (TravelManager.Get.State == FastTravelState.Traveling) {
								//check the last encountered obstructions and see if they're close enough to cause a ruckus
								float checkDistance = 0f;
								foreach (WorldItem obstruction in Player.Local.EncountererObject.LastItemsEncountered) {
										if (Vector3.Distance(Player.Local.Position, obstruction.Position) < 2f) {
												//TODO tie this to effect radius!
												GUIManager.PostDanger("Encountered obstruction");
												TravelManager.Get.CancelTraveling();
										}
								}
						}
						return true;
				}

				public override bool Use(int flavorIndex)
				{
						return false;
				}
		}
}
using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class DespawnOnPlayerLeave : WIScript
		{
				public override void OnInitialized()
				{
						worlditem.OnAddedToPlayerInventory += OnAddToPlayerInventory;
						worlditem.OnInactive += OnInactive;
				}

				public void OnInactive()
				{
						/*if (worlditem.Is(WIMode.World) && worlditem.Group != WIGroups.Get.Player) {
								worlditem.RemoveFromGame();
						} else {
								Finish();
						}*/
				}

				public void OnAddToPlayerInventory()
				{
						Finish();
				}
		}
}

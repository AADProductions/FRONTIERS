using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.World;
using ExtensionMethods;

public class WaterSubmergeObjects : MonoBehaviour
{
		public Action OnItemOfInterestEnterWater;
		public Action OnItemOfInterestExitWater;
		public IItemOfInterest LastSubmergedItemOfInterest;
		public IItemOfInterest LastExitedItemOfInterest;

		public void OnTriggerEnter(Collider other)
		{
				if (other.isTrigger || !GameManager.Is(FGameState.InGame))
						return;

				IItemOfInterest ioi = null;
				if (WorldItems.GetIOIFromCollider(other, out ioi)) {
						if (ioi != LastSubmergedItemOfInterest) {
								FXManager.Get.SpawnFX(ioi.Position, "Water Splash 1");
								AudioManager.MakeWorldSound(ioi, MasterAudio.SoundType.JumpLandWater, "Land");
								LastSubmergedItemOfInterest = ioi;
								if (ioi.IOIType == ItemOfInterestType.Player) {
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.MoveEnterWater, WorldClock.AdjustedRealTime);
								}
								OnItemOfInterestEnterWater.SafeInvoke();
								if (LastExitedItemOfInterest == ioi) {
										LastExitedItemOfInterest = null;
								}
						}
				}
		}

		public void OnTriggerExit(Collider other)
		{
				if (other.isTrigger || !GameManager.Is(FGameState.InGame))
						return;

				IItemOfInterest ioi = null;
				if (WorldItems.GetIOIFromCollider(other, out ioi)) {
						if (LastExitedItemOfInterest != ioi) {
								LastExitedItemOfInterest = ioi;
								if (ioi.IOIType == ItemOfInterestType.Player) {
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.MoveExitWater, WorldClock.AdjustedRealTime);
								}
								OnItemOfInterestExitWater.SafeInvoke();
								if (LastSubmergedItemOfInterest == ioi) {
										LastSubmergedItemOfInterest = null;
								}
						}
				}
		}
}

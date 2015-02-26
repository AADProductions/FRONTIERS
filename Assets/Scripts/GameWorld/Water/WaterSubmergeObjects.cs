using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.World;
using ExtensionMethods;
using Frontiers.World.BaseWIScripts;

public class WaterSubmergeObjects : MonoBehaviour
{
		public Action OnItemOfInterestEnterWater;
		public Action OnItemOfInterestExitWater;
		public IItemOfInterest LastSubmergedItemOfInterest;
		public IItemOfInterest LastExitedItemOfInterest;
		public IBodyOfWater Water = null;

		public void OnTriggerEnter(Collider other)
		{
				if (other.isTrigger || !GameManager.Is(FGameState.InGame))
						return;

				IItemOfInterest ioi = null;
				if (WorldItems.GetIOIFromCollider(other, out ioi)) {
						if (ioi != LastSubmergedItemOfInterest) {
								LastSubmergedItemOfInterest = ioi;
								if (ioi.IOIType == ItemOfInterestType.Player) {
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.MoveEnterWater, WorldClock.AdjustedRealTime);
										FXManager.Get.SpawnFX(ioi.Position, "Water Splash 1");
										AudioManager.MakeWorldSound(ioi, MasterAudio.SoundType.JumpLandWater, "Land");
								} else if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.worlditem.Is (WILoadState.Initialized)) {
										FXManager.Get.SpawnFX(ioi.Position, "Water Splash 1");
										AudioManager.MakeWorldSound(ioi, MasterAudio.SoundType.JumpLandWater, "Land");
										Buoyant b = null;
										if (ioi.worlditem.Is <Buoyant>(out b)) {
												b.Water = Water;
										}
										ioi.worlditem.SetMode(WIMode.World);
										ioi.worlditem.OnEnterBodyOfWater.SafeInvoke();
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
								} else if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.worlditem.Is (WILoadState.Initialized)) {
										ioi.worlditem.OnExitBodyOfWater.SafeInvoke();
								}
								OnItemOfInterestExitWater.SafeInvoke();
								if (LastSubmergedItemOfInterest == ioi) {
										LastSubmergedItemOfInterest = null;
								}
						}
				}
		}
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
		public class LiquidSource : WIScript
		{
				public static Transform LiquidSourceLookPoint;

				public LiquidSourceState State = new LiquidSourceState();

				public override void OnInitialized()
				{
						worlditem.OnPlayerUse += OnPlayerUse;
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						lastHudPriority++;
						if (LiquidSourceLookPoint == null) {
								LiquidSourceLookPoint = new GameObject("Liquid Source Look Point").transform;
						}
						enabled = true;
						GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Drink", LiquidSourceLookPoint, GameManager.Get.GameCamera);
						return lastHudPriority;
				}

				public void Update () {
						if (worlditem.HasPlayerFocus) {
								LiquidSourceLookPoint.position = Player.Local.Surroundings.ClosestObjectFocusHitInfo.point;
						} else {
								enabled = false;
								return;
						}
				}

				public override void PopulateOptionsList(System.Collections.Generic.List<WIListOption> options, List <string> message)
				{
						if (mGenericLiquid == null) {
								if (!WorldItems.GetRandomGenericWorldItemFromCatgeory(State.LiquidCategory, out mGenericLiquid)) {
										return;
								}
						}

						mOptionsListItems.Clear();
						LiquidContainer liquidContainer = null;
						if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <LiquidContainer>(out liquidContainer)) {
								options.Add(new WIListOption("Fill " + liquidContainer.worlditem.DisplayName, "Fill"));
						}

						options.Add(new WIListOption("Drink " + mGenericLiquid.DisplayName, "Drink"));
				}

				public void OnPlayerUse()
				{
						Drink();
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						if (dialogResult.SecondaryResult.Contains("Drink")) {
								Drink();
						} else {
								LiquidContainer liquidContainer = null;
								if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <LiquidContainer>(out liquidContainer)) {
										LiquidContainerState liquidContainerState = liquidContainer.State;
										int numFilled = 0;
										string errorMessage = string.Empty;
										if (liquidContainerState.TryToFillWith(mGenericLiquid, Int32.MaxValue, out numFilled, out errorMessage)) {
												GUIManager.PostInfo("Filled " + liquidContainer.worlditem.DisplayName + " with " + numFilled.ToString() + " " + mGenericLiquid.PrefabName + "(s)");
												MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "FillLiquidContainer");
										}
								}
						}
						mOptionsListItems.Clear();
				}

				protected void Drink()
				{
						if (mGenericLiquid == null) {
								if (!WorldItems.GetRandomGenericWorldItemFromCatgeory(State.LiquidCategory, out mGenericLiquid)) {
										return;
								}
						}

						WorldItem worlditem = null;
						if (WorldItems.Get.PackPrefab(mGenericLiquid.PackName, mGenericLiquid.PrefabName, out worlditem)) {
								FoodStuff foodStuff = null;
								if (worlditem.gameObject.HasComponent <FoodStuff>(out foodStuff)) {
										FoodStuff.Drink(foodStuff);
								}
						}
				}

				protected GenericWorldItem mGenericLiquid = null;
				protected Dictionary <string, IWIBase> mOptionsListItems = new Dictionary <string, IWIBase>();
		}

		[Serializable]
		public class LiquidSourceState
		{
				public string LiquidCategory;
		}
}
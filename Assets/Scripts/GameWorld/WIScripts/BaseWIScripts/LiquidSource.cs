using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.BaseWIScripts
{
		public class LiquidSource : WIScript
		{
				public LiquidSourceState State = new LiquidSourceState();

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

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						if (dialogResult.SecondaryResult.Contains("Drink")) {
								WorldItem worlditem = null;
								if (WorldItems.Get.PackPrefab(mGenericLiquid.PackName, mGenericLiquid.PrefabName, out worlditem)) {
										FoodStuff foodStuff = null;
										if (worlditem.gameObject.HasComponent <FoodStuff>(out foodStuff)) {
												FoodStuff.Drink(foodStuff);
										}
								}
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

				protected GenericWorldItem mGenericLiquid = null;
				protected Dictionary <string, IWIBase> mOptionsListItems = new Dictionary <string, IWIBase>();
		}

		[Serializable]
		public class LiquidSourceState
		{
				public string LiquidCategory;
		}
}
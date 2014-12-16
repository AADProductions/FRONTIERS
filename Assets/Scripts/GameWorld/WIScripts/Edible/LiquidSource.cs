using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class LiquidSource : WIScript
		{
				public LiquidSourceState State = new LiquidSourceState();

				public override void PopulateOptionsList(System.Collections.Generic.List<GUIListOption> options, List <string> message)
				{
						if (mGenericLiquid == null) {
								if (!WorldItems.GetRandomGenericWorldItemFromCatgeory(State.LiquidCategory, out mGenericLiquid)) {
										return;
								}
						}

						mOptionsListItems.Clear();
						LiquidContainer liquidContainer = null;
						if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <LiquidContainer>(out liquidContainer)) {
								options.Add(new GUIListOption("Fill " + liquidContainer.worlditem.DisplayName, "Fill"));
						}
//			Dictionary <int,IWIBase> QuickslotItems = Player.Local.Inventory.QuickslotItems;
//			foreach (KeyValuePair <int,IWIBase> quickslotItem in QuickslotItems) {
//				IWIBase qsItem = quickslotItem.Value;
//				//foreach liquid container in quickslots
//				if (qsItem.Is <LiquidContainer> () && qsItem.IsWorldItem) {
//					//TODO make sure we can actually fill the item
//					options.Add (new GUIListOption ("Fill " + qsItem.DisplayName, qsItem.FileName));
//					mOptionsListItems.Add (qsItem.FileName, qsItem);
//				}
//			}

						options.Add(new GUIListOption("Drink " + mGenericLiquid.DisplayName, "Drink"));
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;

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
//				IWIBase qsItem = null;
//				if (mOptionsListItems.TryGetValue (dialogResult.SecondaryResult, out qsItem)) {
//					System.Object liquidContainerStateObject = null;
//					if (qsItem.GetStateOf <LiquidContainer> (out liquidContainerStateObject)) {
//						LiquidContainerState liquidContainerState = liquidContainerStateObject as LiquidContainerState;
//						if (liquidContainerState != null) {
//							int numFilled = 0;
//							string errorMessage = string.Empty;
//							if (liquidContainerState.TryToFillWith (mGenericLiquid, Int32.MaxValue, out numFilled, out errorMessage)) {
//								GUIManager.PostInfo ("Filled " + qsItem.DisplayName + " with " + numFilled.ToString () + " " + mGenericLiquid.PrefabName + "(s)");
//								MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "FillLiquidContainer");
//							}
//						}
//					}
//				}
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
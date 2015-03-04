using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.BaseWIScripts;
using Frontiers.Story;

namespace Frontiers.World
{
		public class Bartender : WIScript
		{
				public BartenderState State = new BartenderState();

				public int PricePerRound {
						get {
								return (int)Globals.BarBasePricePerRoundOfDrinks;
						}
				}

				public int PricePerDrink {
						get {
								return (int)Globals.BarBasePricePerDrink;
						}
				}

				public bool CanBuyDrink {
						get {
								//it's been 24 hours since we've bought a drink, reset tonight's drink counter
								if (WorldClock.AdjustedRealTime > State.LastTimeBoughtDrink + WorldClock.HoursToSeconds(18)) {
										State.NumTimesBoughtDrinkTonight = 0;
								}
								if (State.NumTimesBoughtDrinkTonight <= 5) {
										return true;
								}
								return false;
						}
				}

				public bool CanBuyRound {
						get {
								return WorldClock.AdjustedRealTime > State.LastTimeBoughtRound + WorldClock.HoursToSeconds(18);
						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List<string> message)
				{
						gBuyDrinkOption.RequiredCurrencyType = WICurrencyType.A_Bronze;
						gBuyRoundOption.RequiredCurrencyType = WICurrencyType.A_Bronze;
						gBuyDrinkOption.CurrencyValue = PricePerDrink;
						gBuyRoundOption.CurrencyValue = PricePerRound;
						gBuyDrinkOption.Disabled = true;
						gBuyRoundOption.Disabled = true;
						if (Player.Local.Inventory.InventoryBank.CanAfford(PricePerDrink)) {
								gBuyDrinkOption.Disabled = false;
						}
						if (CanBuyRound && Player.Local.Inventory.InventoryBank.CanAfford(PricePerRound)) {
								gBuyRoundOption.Disabled = false;
						}
						options.Add(gBuyDrinkOption);
						options.Add(gBuyRoundOption);
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						switch (dialogResult.SecondaryResult) {
								case "Drink":
										if (!CanBuyDrink) {
												if (gBartenderSpeech == null) {
														gBartenderSpeech = new Speech();
												}
												gBartenderSpeech.Text = "That's enough for one night, {lad/lass}.";
												worlditem.Get <Talkative>().SayDTS(gBartenderSpeech);
										} else {
												if (Player.Local.Inventory.InventoryBank.TryToRemove(PricePerDrink)) {
														Profile.Get.CurrentGame.Character.Rep.GainGlobalReputation(1);
														//spawn a cup and put some mead in it
														WorldItem cup = null;
														if (WorldItems.CloneRandomFromCategory("BartenderCups", WIGroups.Get.World, out cup)) {
																cup.Initialize();
																LiquidContainer container = null;
																if (cup.Is <LiquidContainer>(out container)) {
																		container.State.Contents.CopyFrom(BartenderDrinkContents);
																		container.State.Contents.InstanceWeight = container.State.Capacity;
																}
																//try to equip it in the player's hands
																Player.Local.Inventory.TryToEquip(cup);
														}
														State.NumTimesBoughtDrink++;
														State.NumTimesBoughtDrinkTonight++;
														State.LastTimeBoughtDrink = WorldClock.AdjustedRealTime;
												}
										}
										break;

								case "Round":
										if (Player.Local.Inventory.InventoryBank.TryToRemove(PricePerRound)) {
												if (gBartenderSpeech == null) {
														gBartenderSpeech = new Speech();
												}
												gBartenderSpeech.Text = "Looks like this round's on {PlayerFirstName}!";
												worlditem.Get <Talkative>().SayDTS(gBartenderSpeech);
												Profile.Get.CurrentGame.Character.Rep.GainGlobalReputation(5);
												State.NumTimesBoughtRound++;
												State.LastTimeBoughtRound = WorldClock.AdjustedRealTime;
										}
										break;
				
								default:
										break;
						}
				}

				protected static GenericWorldItem BartenderDrinkContents {
						get {
								if (gBartenderDrinkContents == null) {
										gBartenderDrinkContents = new GenericWorldItem();
										gBartenderDrinkContents.PackName = "Edibles";
										gBartenderDrinkContents.PrefabName = "Mead";
								}
								return gBartenderDrinkContents;
						}
				}

				protected static Speech gBartenderSpeech;
				protected static GenericWorldItem gBartenderDrinkContents = null;
				protected static WIListOption gBuyDrinkOption = new WIListOption("Buy a drink", "Drink");
				protected static WIListOption gBuyRoundOption = new WIListOption("Buy a round", "Round");
		}

		public class BartenderState
		{
				public int NumTimesBoughtDrink = 0;
				public int NumTimesBoughtDrinkTonight = 0;
				public int NumTimesBoughtRound = 0;
				public double LastTimeBoughtDrink;
				public double LastTimeBoughtRound;
		}
}
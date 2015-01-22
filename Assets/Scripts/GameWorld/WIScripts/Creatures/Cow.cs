using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World.Gameplay
{
		public class Cow : WIScript
		{
				public string MilkCategory = "FreshMilk";

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						if (Player.Local.Tool.IsEquipped) {
								LiquidContainer container = null;
								if (Player.Local.Tool.worlditem.Is <LiquidContainer>(out container)) {
										if (container.State.IsEmpty) {
												options.Add(new WIListOption("Milk"));
										}
								}
						}

						if (!worlditem.Get <Creature>().IsStunned) {
								options.Add(new WIListOption("Tip"));
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;			
						switch (dialogResult.SecondaryResult) {
								case "Milk":
										LiquidContainer container = null;
										if (Player.Local.Tool.worlditem.Is <LiquidContainer>(out container)) {
												if (container.State.IsEmpty) {
														WICategory category = null;
														System.Random random = new System.Random(Profile.Get.CurrentGame.Seed);
														if (WorldItems.Get.Category(MilkCategory, out category)) {
																GenericWorldItem milkItem = null;
																if (category.GenericWorldItems.Count > 0) {
																		milkItem = category.GenericWorldItems[random.Next(0, category.GenericWorldItems.Count)];
																} else {
																		milkItem = category.GenericWorldItems[0];
																}
																container.State.Contents.CopyFrom(milkItem);
																container.State.Contents.InstanceWeight = container.State.Capacity;
																//make the cow make a noise
																worlditem.Get<Creature>().OnTakeDamage();
														}
												}
										}
										break;

								case "Tip":
										worlditem.Get <Creature>().OnTakeDamage();//make a noise and flip out
										worlditem.Get <Creature>().TryToStun(10f);
										break;
				
								default:
										break;
						}
				}
		}
}
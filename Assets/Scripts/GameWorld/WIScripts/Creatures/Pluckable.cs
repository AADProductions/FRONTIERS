using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class Pluckable : WIScript
		{
				public static GenericWorldItem PluckedItem {
						get {
								if (gPluckedItem == null) {
										gPluckedItem = new GenericWorldItem();
										gPluckedItem.PackName = "Decorations";
										gPluckedItem.PrefabName = "Quill 1";
								}
								return gPluckedItem;
						}
				}

				Creature creature = null;

				public override void OnInitialized()
				{
						creature = worlditem.Get <Creature>();
				}

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						if (gPluckOption == null) {
								gPluckOption = new WIListOption("Pluck", "Pluck");
						}

						if (!creature.IsDead) {
								options.Add(gPluckOption);
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{

						WIListResult dialogResult = secondaryResult as WIListResult;			
						switch (dialogResult.SecondaryResult) {
								case "Pluck":
										WIStackError error = WIStackError.None;
										WorldItem pluckedWorldItem = null;
										if (WorldItems.CloneWorldItem(PluckedItem, STransform.zero, false, WIGroups.GetCurrent(), out pluckedWorldItem)) {
												pluckedWorldItem.Initialize();
										}
										Player.Local.Inventory.AddItems(pluckedWorldItem, ref error);
										creature.OnTakeDamage();
										creature.FleeFromThing(Player.Local);
										break;

								default:
										break;
						}
				}

				protected static WIListOption gPluckOption;
				protected static GenericWorldItem gPluckedItem;
		}
}
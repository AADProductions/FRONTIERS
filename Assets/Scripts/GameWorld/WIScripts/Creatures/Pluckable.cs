using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class Pluckable : WIScript
		{
				public static GenericWorldItem PluckedItem {
						get {
								if (gPluckedItem == null) {
										gPluckedItem = new GenericWorldItem();
										gPluckedItem.PackName = "Decorations";
										gPluckedItem.PrefabName = "Quill";
								}
								return gPluckedItem;
						}
				}

				Creature creature = null;

				public override void OnInitialized()
				{
						creature = worlditem.Get <Creature>();
				}

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (!creature.IsDead) {
								options.Add(gPluckOption);
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						if (gPluckOption == null) {
								gPluckOption = new GUIListOption("Pluck", "Pluck");
						}

						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "Pluck":
										WIStackError error = WIStackError.None;
										Player.Local.Inventory.AddItems(PluckedItem.ToStackItem(), ref error);
										creature.OnTakeDamage();
										break;

								default:
										break;
						}
				}

				protected static GUIListOption gPluckOption;
				protected static GenericWorldItem gPluckedItem;
		}
}
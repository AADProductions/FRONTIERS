using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class BlueprintSquareDisplay : GUICircularBrowserObject
		{
				public SquareDisplayMode DisplayMode;
				public WIBlueprint Blueprint = null;

				public bool HasBlueprint {
						get {
								return Blueprint != null && !Blueprint.IsEmpty;
						}
				}

				public void UpdateDisplay()
				{
						if (!HasBlueprint) {
								if (mDoppleGanger != null) {
										GameObject.Destroy(mDoppleGanger);
										DisplayMode = SquareDisplayMode.Disabled;
										InventoryItemName.text = "(Click to browse)";
										return;
								}
						} else {
								mDoppleGanger = WorldItems.GetDoppleganger(Blueprint.GenericResult.PackName, Blueprint.GenericResult.PrefabName, transform, mDoppleGanger, WIMode.Stacked);
								DisplayMode = SquareDisplayMode.Enabled;
								InventoryItemName.text = Blueprint.CleanName;
						}

						Color backgroundColor = Color.white;
					
						switch (DisplayMode) {
								case SquareDisplayMode.Empty:
										backgroundColor = Color.gray;
										break;
				
								case SquareDisplayMode.Disabled:
										backgroundColor = Color.gray;
										break;
				
								case SquareDisplayMode.Enabled:
								default:
										break;
						}
			
						Background.color = backgroundColor;
				}

				protected GameObject mDoppleGanger	= null;
		}
}
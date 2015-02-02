using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class WIFoodDoppleganger : WIDoppleganger
		{		//custom doppleganger script for prepared foods
				public Bounds ItemBounds = new Bounds();

				public override GameObject GetDoppleganger(WorldItem item, Transform dopplegangerParent, string dopplegangerName, WIMode mode, string state, string subcat, float scaleMultiplier, TimeOfDay tod, TimeOfYear toy)
				{
						GameObject doppleganger = dopplegangerParent.gameObject.FindOrCreateChild(dopplegangerName).gameObject;
						//use the subcat to get our blueprint result
						Vector3 offset = Vector3.zero;
						ItemBounds.size = Vector3.one;
						ItemBounds.center = Vector3.zero;
						doppleganger.transform.parent = null;
						doppleganger.transform.ResetLocal();
						PreparedFoods.InitializePreparedFoodGameObject(doppleganger, subcat, true, ref ItemBounds);
						doppleganger.transform.parent = dopplegangerParent;
						if (Flags.Check((uint)mode, (uint)(WIMode.Stacked | WIMode.Selected | WIMode.Crafting | WIMode.Wear), Flags.CheckType.MatchAny)) {
								WorldItems.AutoScaleDoppleganger(dopplegangerParent, doppleganger, ItemBounds, ref scaleMultiplier, ref offset);
						}
						WorldItems.ApplyDopplegangerMode(item, doppleganger, mode, scaleMultiplier, offset);
						WorldItems.ApplyDopplegangerMaterials(doppleganger, mode);
						return doppleganger;
				}
		}
}
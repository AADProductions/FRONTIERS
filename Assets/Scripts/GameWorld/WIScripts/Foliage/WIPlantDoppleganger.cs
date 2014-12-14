using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class WIPlantDoppleganger : WIDoppleganger
		{
				//custom doppleganger script for plants
				public override GameObject GetDoppleganger(WorldItem item, Transform dopplegangerParent, string dopplegangerName, WIMode mode, string state, string subcat, float scaleMultiplier, TimeOfDay tod, TimeOfYear toy)
				{
						GameObject doppleganger = dopplegangerParent.gameObject.FindOrCreateChild(dopplegangerName).gameObject;
						Vector3 offset = Vector3.zero;
						Plants.Get.InitializeWorldPlantGameObject(doppleganger, subcat, toy);
						if (Flags.Check((uint)mode, (uint)(WIMode.Stacked | WIMode.Selected | WIMode.Crafting | WIMode.Wear), Flags.CheckType.MatchAny)) {
								WorldItems.AutoScaleDoppleganger(dopplegangerParent, doppleganger, item.BaseObjectBounds, ref scaleMultiplier, ref offset);
						}
						WorldItems.ApplyDopplegangerMode(item, doppleganger, mode, scaleMultiplier, offset);
						return doppleganger;
				}
		}
}
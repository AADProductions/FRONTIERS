using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class WIBookAvatarDoppleganger : WIDoppleganger
		{
				public override GameObject GetDoppleganger(WorldItem item, Transform dopplegangerParent, string dopplegangerName, WIMode mode, string state, string subcat, float scaleMultiplier, TimeOfDay tod, TimeOfYear toy)
				{
						GameObject doppleganger = dopplegangerParent.gameObject.FindOrCreateChild(dopplegangerName).gameObject;
						Books.Get.InitializeBookAvatarGameObject(doppleganger, item.StackName, subcat);
						WorldItems.ApplyDopplegangerMode(item, doppleganger, mode, scaleMultiplier);
						return doppleganger;
				}
		}
}

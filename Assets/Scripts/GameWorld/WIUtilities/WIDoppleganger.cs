using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class WIDoppleganger : MonoBehaviour
		{		//used by WorldItems manager to create a custom doppleganger
				public virtual GameObject GetDoppleganger(WorldItem item, Transform dopplegangerParent, string dopplegangerName, WIMode mode, string state, string subcat, float scaleMultiplier, TimeOfDay tod, TimeOfYear toy)
				{
						return null;
				}
		}
}
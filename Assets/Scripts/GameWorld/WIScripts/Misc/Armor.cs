using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.World.WIScripts
{
		public class Armor : WIScript
		{
				public int BaseDamageProtection;
				[BitMaskAttribute(typeof(WIMaterialType))]
				public WIMaterialType MaterialTypes	= WIMaterialType.Fabric;

				public int ArmorLevel(WIMaterialType materialType)
				{
						if (Flags.Check((uint)MaterialTypes, (uint)materialType, Flags.CheckType.MatchAny)) {
								return BaseDamageProtection;
						}
						return 0;
				}
		}
}
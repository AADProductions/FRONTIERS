using UnityEngine;
using System;
using System.Collections;
using Frontiers;

namespace Frontiers.World
{
		public class StructureDestroyResult : MonoBehaviour
		{
				[BitMaskAttribute(typeof(StructureDestroyedBehavior))]
				public StructureDestroyedBehavior Behavior = StructureDestroyedBehavior.Destroy;
		}

		[Flags]
		public enum StructureDestroyedBehavior
		{
				None = 0,
				Ignite = 1,
				Destroy = 2,
				Unfreeze = 4,
				Freeze = 8,
				IgniteAndUnfreeze = 16,
		}
}

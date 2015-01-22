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
}

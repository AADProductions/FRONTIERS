using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class BakedGood : WIScript
		{
		}

		public class BakedGoodState
		{
				public BakedGoodStyle Stlye = BakedGoodStyle.LoafOfBread;
				public SColor ToppingColor = Color.white;
		}
}
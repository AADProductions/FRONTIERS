using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class Sprint : Skill
		{
				public override bool RequirementsMet {
						get {
								return !Player.Local.Surroundings.IsInWater && base.RequirementsMet;
						}
				}
		}
}
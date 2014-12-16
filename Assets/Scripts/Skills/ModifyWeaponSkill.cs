using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class ModifyWeaponSkill : Skill
		{
				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						if (base.Use(targetObject, flavorIndex)) {
								WeaponSkillModifier cursed = targetObject.gameObject.GetOrAdd <WeaponSkillModifier>();
								cursed.TimeApplied = WorldClock.Time;
								cursed.ParentSkillName = name;
								return true;
						}
						return false;
				}
		}
}
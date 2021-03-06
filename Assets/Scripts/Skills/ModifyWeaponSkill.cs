using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
		public class ModifyWeaponSkill : Skill
		{
				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						if (base.Use(targetObject, flavorIndex)) {
								WeaponSkillModifier cursed = targetObject.gameObject.GetOrAdd <WeaponSkillModifier>();
								cursed.TimeApplied = WorldClock.AdjustedRealTime;
								cursed.ParentSkillName = name;
								return true;
						}
						return false;
				}
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.Gameplay
{
		public class RespawnSkill : Skill
		{
				public override void Initialize()
				{
						base.Initialize();
						//respawn skills are always going to be one-time-use only
						//and have no duration
						//make sure these are set correclty or spawning won't work
						Usage.CooldownInterval = 0f;
						Usage.RealTimeDuration = true;
						Usage.Type = SkillUsageType.Once;
				}

				public virtual bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						if (targetObject.IOIType == ItemOfInterestType.Player) {
								RespawnPlayer(Player.Local);//todo make it work with anything
								return true;
						}
						return base.Use(flavorIndex);
				}

				protected virtual void RespawnPlayer(LocalPlayer player)
				{ 

				}
		}
}
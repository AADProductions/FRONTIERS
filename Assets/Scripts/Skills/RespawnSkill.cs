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
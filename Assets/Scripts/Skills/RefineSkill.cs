using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.World.Gameplay
{
		public class RefineSkill : Skill
		{
				public RefineSkillExtensions Extensions = new RefineSkillExtensions();

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (base.DoesContextAllowForUse(targetObject)) {
								if (Player.Local.Tool.IsEquipped) {
										if (Player.Local.Tool.worlditem.IsMadeOf(Extensions.RefineMaterialType)) {
												return true;
										} else {
												return false;
										}
								}
						}
						return false;
				}
		}

		[Serializable]
		public class RefineSkillExtensions
		{
				public WIMaterialType RefineMaterialType = WIMaterialType.None;
		}
}
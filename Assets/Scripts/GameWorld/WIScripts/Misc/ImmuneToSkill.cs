using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{		//can make any item not affected by any skill
		//eg, an item can't be stolen
		public class ImmuneToSkill : WIScript
		{
				public ImmuneToSkillState State = new ImmuneToSkillState();

				public bool IsImmuneTo(Skill skill)
				{
						return State.IsImmuneTo(skill, out mMessageCheck);
				}

				public override void PopulateExamineList(List <WIExamineInfo> examine)
				{
						for (int i = 0; i < State.Immunities.Count; i++) {
								examine.Add(new WIExamineInfo(State.Immunities[i].ReasonDescription));
						}
				}

				protected static string mMessageCheck = null;
		}

		[Serializable]
		public class ImmuneToSkillState
		{
				public List <SkillImmunity> Immunities = new List <SkillImmunity>();

				public bool IsImmuneTo(Skill skill, out string message)
				{
						for (int i = 0; i < Immunities.Count; i++) {
								if (Immunities[i].SkillName.Equals(skill.name)) {
										message = Immunities[i].ReasonDescription;
										return true;
								}
						}
						message = null;
						return false;
				}
		}

		[Serializable]
		public class SkillImmunity
		{
				[FrontiersAvailableMods("Skill")]
				public string SkillName;
				public string ReasonDescription;
		}
}
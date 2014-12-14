using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
		public class Refinable : WIScript
		{
				public bool CanBeRefined = true;
				public RefineResultType ResultType = RefineResultType.SetState;
				public string RefinedStateName = "Refined";
				public string BrokenStateName = string.Empty;
				public GenericWorldItem RefinedReplacement = new GenericWorldItem();
				public GenericWorldItem BrokenReplacement = new GenericWorldItem();
				public Action OnRefine;

				public bool Refine(Skill skill, float refineryMinimumSkill)
				{

						bool successfullyRefined = false;

						if (skill.LastSkillRoll == SkillRollType.Success) {
								successfullyRefined = true;
								switch (ResultType) {
										case RefineResultType.SetState:
										default:
												if (skill.LastSkillValue >= refineryMinimumSkill) {
														worlditem.State = RefinedStateName;
												} else {
														worlditem.State = BrokenStateName;
												}
												break;

										case RefineResultType.Replace:
												if (skill.LastSkillValue >= refineryMinimumSkill) {
														WorldItems.ReplaceWorldItem(worlditem, RefinedReplacement);
												} else {
														WorldItems.ReplaceWorldItem(worlditem, BrokenReplacement);
												}
												break;
								}
								//we can't be refined again
								Finish();
						}
						return successfullyRefined;
				}
		}

		public enum RefineResultType
		{
				SetState,
				Replace
		}
}
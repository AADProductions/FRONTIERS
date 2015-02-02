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
														Debug.Log("Refined skill value within range, setting to state refined");
														worlditem.State = RefinedStateName;
												} else {
														Debug.Log("Refined skill value NOT within range, setting to state broken");
														GUI.GUIManager.PostWarning("Your lack of skill broke the item.");
														worlditem.State = BrokenStateName;
												}
												break;

										case RefineResultType.Replace:
												if (skill.LastSkillValue >= refineryMinimumSkill) {
														Debug.Log("Refined skill value NOT within range, replacing worlditem");
														Debug.Log("Refined skill value within range");
														WorldItems.ReplaceWorldItem(worlditem, RefinedReplacement);
												} else {
														Debug.Log("Refined skill value NOT within range, replacing with broken replacement");
														GUI.GUIManager.PostWarning("Your lack of skill broke the item.");
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
}
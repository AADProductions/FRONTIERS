using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class ReverseEngineer : Skill
		{
				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						WIBlueprint blueprint = null;
						if (Blueprints.Get.BlueprintExistsForItem(targetObject.worlditem, out blueprint) && !blueprint.Revealed) {
								return true;
						}
						return false;
				}

				protected override void OnUseFinish()
				{
						//get the blueprint for the item if one exists
						//if it exists reveal it
						//TODO make this a timed skill
						WIBlueprint blueprint = null;
						if (Blueprints.Get.BlueprintExistsForItem(LastSkillTarget.worlditem, out blueprint)) {
								Blueprints.Get.Reveal(blueprint.Name, BlueprintRevealMethod.ReverseEngineer, DisplayName);
						}

				}
		}
}
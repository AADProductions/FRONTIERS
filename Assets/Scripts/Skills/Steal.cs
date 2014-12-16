using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class Steal : RemoveItemSkill
		{
				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						bool result = false;
						//the target object is going to be a container
						//get the group owner of the container
						IStackOwner owner = null;

						GUIManager.PostDanger("Stole item");
						GUIManager.PostDanger("Your crime was imperfect - traces were left behind.");
						GUIManager.PostDanger("Your reputation has suffered.");

						if (targetObject.worlditem.Group.HasOwner(out owner)) {
								//Debug.Log ("Item has owner " + owner.DisplayName);
								//check if the owner can see the player
								//use a random skill etc.
								float skillUseValue = UnityEngine.Random.value;
								if (skillUseValue > Mathf.Max(State.NormalizedUsageLevel, Globals.SkillFailsafeMasteryLevel)) {
										OnFailure();
										result = false;
								}
								OnSuccess();
								result = true;
						} else {
								//if it has no owner then we're successful by default
								result = true;
						}
						return result;
				}
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
	public class PrepareFood : Skill
	{
		public override bool DoesContextAllowForUse (IItemOfInterest targetObject)
		{
			if (base.DoesContextAllowForUse (targetObject)) {
				CraftingItem craftingItem = targetObject.gameObject.GetComponent <CraftingItem> ();
				if (craftingItem.SkillToUse == name) {
					return true;
				}
			}
			return false;
		}

		public override bool Use (IItemOfInterest targetObject, int flavorIndex)
		{
			//assume we're looking at a crafting object by this point
			targetObject.gameObject.SendMessage ("OpenCraftingInterface");
			return true;
		}
	}
}
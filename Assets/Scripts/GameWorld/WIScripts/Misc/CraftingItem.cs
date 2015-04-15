using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
		public class CraftingItem : WIScript
		{
				//used by skills like 'Prepare Food' and 'Potions'
				//the crafting interface does most of the work, this just IDs the object as a crafting item
				public string SkillToUse = "Craft";
				public float SpeedBonus = 1.0f;

				public void OpenCraftingInterface()
				{			
						Frontiers.GUI.PrimaryInterface.MaximizeInterface("Inventory", "CraftingViaItem", gameObject);
				}
		}
}
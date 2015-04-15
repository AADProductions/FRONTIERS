using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
	public class AlterPath : PathSkill
	{
		protected const int flavorIndexRemove = 0;
		protected const int flavorIndexMove = 1;
		protected const int flavorIndexAdd = 2;

		public override bool Use (IItemOfInterest targetObject, int flavorIndex)
		{			
			if (targetObject.IOIType != ItemOfInterestType.WorldItem) {
				return false;
			}

			PathMarker pathMarker = null;
			if (targetObject.worlditem.Is <PathMarker> (out pathMarker)) {
				switch (flavorIndex) {
				case flavorIndexMove:
					return pathMarker.Move ();

				case flavorIndexRemove:
					return pathMarker.RemoveFromPath ();

				case flavorIndexAdd:
					break;

				default:
					break;
				}
			}
			return false;
		}

		public override WIListOption GetListOption (IItemOfInterest targetObject)
		{
			if (targetObject.IOIType != ItemOfInterestType.WorldItem) {
				return WIListOption.Empty;
			}

			PathMarker pathMarker = null;
			if (!Paths.HasActivePath || !targetObject.worlditem.Is <PathMarker> (out pathMarker)) {
				return WIListOption.Empty;
			}

			WIListOption listOption = base.GetListOption (targetObject);
			List <string> flavors = new List <string> ();

			flavors.Add ("Remove");
			flavors.Add ("Move");

			listOption.Flavors = flavors;

			return listOption;
		}
	}

	public class PathSkill : Skill
	{

	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class Bind : Skill
		{
				public GameObject spellEffect;

				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						//SKILL USE
						if (base.Use(targetObject, flavorIndex)) {
								Vector3 spellPositon = Player.Local.Position + (Player.Local.ForwardVector * 5.0f);
								spellPositon.y = GameWorld.Get.CurrentTerrain.SampleHeight(spellPositon);
								GameObject.Instantiate(spellEffect, spellPositon, Quaternion.identity);
								return true;
						}

						return false;
				}
		}
}
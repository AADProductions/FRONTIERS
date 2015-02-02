using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class ProjectileTarget : WIScript
		{
				public Transform Bullseye;
				public float BullseyeRange = 1.0f;

				public void OnHitByProjectile(Projectile projectile, Vector3 hitPoint)
				{ 
						if (Vector3.Distance(hitPoint, Bullseye.position) <= BullseyeRange) {
								GUI.GUIManager.PostSuccess("Bullseye!");
								Skill bowSkill = null;
								//TODO make this work with other skills
								if (Skills.Get.SkillByName("Bow", out bowSkill)) {
										//SKILL USE
										bowSkill.Use(true);
								}
						}
				}
		}
}

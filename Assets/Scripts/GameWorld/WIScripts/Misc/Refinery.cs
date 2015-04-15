using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class Refinery : WIScript
		{		//used with Refinable plus a Refine skill to refine goods
				[FrontiersFXAttribute]
				public string FXOnRefine;
				public GameObject FXParent;
				public WIMaterialType RefinesMaterial = WIMaterialType.Metal;
				public float MinimumSkill = 0.5f;

				public override void OnInitialized()
				{
						if (FXParent == null) {
								FXParent = gameObject;
						}
				}

				public bool CanRefine {
						get {
								//returns a value to the skill
								Refinable refinable = null;
								if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <Refinable>(out refinable)) {
										return refinable.worlditem.IsMadeOf(RefinesMaterial) && refinable.CanBeRefined;
								}
								return false;
						}
				}

				public bool Refine(Skill skill)
				{		//returns a value to the skill
						if (Player.Local.Tool.IsEquipped) {
								Refinable refinable = null;
								if (Player.Local.Tool.worlditem.Is <Refinable>(out refinable)) {
										if (refinable.Refine(skill, MinimumSkill)) {
												FXManager.Get.SpawnFX(FXParent, FXOnRefine);
												return true;
										}
								}
						}
						return false;
				}
		}
}

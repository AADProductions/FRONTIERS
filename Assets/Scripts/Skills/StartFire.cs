using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
		public class StartFire : Skill
		{
				public bool UsedFireStarter = false;

				public override bool RequiresAtLeastOneEquippedWorldItem {
						get {
								return false;
						}
				}

				public override float EffectTime {
						get {
								if (UsedFireStarter) {
										return 0f;
								}
								return base.EffectTime;
						}
				}

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (targetObject.IOIType == ItemOfInterestType.WorldItem) {
								Character character = null;
								if (targetObject.worlditem.Is <Character>(out character)) {
										if (!(character.IsDead || character.IsStunned)) {
												return false;
										}
								}
								Creature creature = null;
								if (targetObject.worlditem.Is <Creature>(out creature)) {
										if (!(creature.IsDead || creature.IsStunned)) {
												return false;
										}
								}
						}
						return true;
				}

				public override GUIListOption GetListOption(IItemOfInterest targetObject)
				{
						base.GetListOption(targetObject);

						UsedFireStarter = false;

						if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <FireStarter>()) {
								UsedFireStarter = true;
						}
			
						Flammable flammable = null;
						if (targetObject.worlditem.Is <Flammable>(out flammable)) {
								if (flammable.IsOnFire && flammable.CanBeExtinguished) {
										mListOption.OptionText = "Extinguish Fire";
										mListOption.NegateIcon = true;
										Usage.ProgressDialogMessage = "Extinguishing fire...";
								} else if (flammable.CanBeIgnited) {
										mListOption.OptionText = DisplayName;
										mListOption.NegateIcon = false;
										Usage.ProgressDialogMessage = "Starting fire...";
								}
						}
						return mListOption;
				}

				public override bool Use(bool successfully)
				{
						//SKILL USE
						return base.Use(successfully);
				}
		}
}
using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Terrified : WIScript, ISkillEffect
		{
				//TODO modify this to work better with character

				public Skill ParentSkill { get; set; }

				public double RTUpdateInterval { get; set; }

				public double RTEffectTime { get; set; }

				public string FXOnUpdate { get; set; }

				public IItemOfInterest TargetFXObject { get; set; }

				public double StartTime { get; set; }

				public IItemOfInterest ThingToFlee;
				Creature creature = null;
				Character character = null;

				public override bool EnableAutomatically {
						get {
								return true;
						}
				}

				public override void OnInitialized()
				{
						if (!worlditem.Is <Creature>(out creature)) {
								Finish();
								return;
						}

						if (ThingToFlee == null) {
								ThingToFlee = Player.Local;
						} else if (ParentSkill != null) {
								ThingToFlee = ParentSkill.Caster;
						}
				}

				public void Update()
				{
						if (WorldClock.AdjustedRealTime > mLastUpdateTime + RTUpdateInterval) {
								mLastUpdateTime = WorldClock.AdjustedRealTime;
								creature.FleeFromThing(ThingToFlee);
								FXManager.Get.SpawnFX(TargetFXObject, FXOnUpdate);
						}

						if (WorldClock.AdjustedRealTime > StartTime + RTEffectTime) {
								enabled = false;
								Finish();
						}
				}

				protected double mLastUpdateTime = 0f;
		}
}
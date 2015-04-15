using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
		public class InflictBlackDeath : SkillEffectScript
		{
				public Damageable TargetDamageable;

				public override void OnEffectStart()
				{
						//temp
						Creature creature = null;
						if (gameObject.HasComponent <Creature>(out creature)) {
								creature.TryToStun(10.0f);
						}
						if (!gameObject.HasComponent <Damageable>(out TargetDamageable)) {
								Finish();
						}
				}

				public override void UpdateEffects()
				{
						float actualDamage = 0f;
						bool isDead = false;
						float attemptedDamage = TargetDamageable.State.Durability * ParentSkill.State.NormalizedUsageLevel;
						TargetDamageable.TakeDamage(WIMaterialType.Crystal, TargetDamageable.worlditem.tr.position, attemptedDamage, Vector3.zero, ParentSkill.DisplayName, out actualDamage, out isDead);
						if (isDead) {
								Finish();
						}
				}
		}
}
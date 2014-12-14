using UnityEngine;
using System.Collections;

namespace Frontiers.World.Gameplay
{
		public class AbsorbHealthFromDamageable : SkillEffectScript
		{
				public Damageable TargetDamageable;

				public override void OnEffectStart()
				{
						if (!gameObject.HasComponent <Damageable>(out TargetDamageable)) {
								Finish();
						}
				}

				public override void UpdateEffects()
				{
						float actualDamage = 0f;
						bool isDead = false;
						float attemptedDamage = TargetDamageable.State.Durability * ParentSkill.State.NormalizedUsageLevel;
						if (TargetDamageable.TakeDamage(WIMaterialType.Crystal, TargetDamageable.worlditem.tr.position, attemptedDamage, Vector3.zero, ParentSkill.DisplayName, out actualDamage, out isDead)) {
								actualDamage = actualDamage / TargetDamageable.State.Durability;
								Player.Local.Status.RestoreStatus(actualDamage, "Health");
						}
						if (isDead) {
								Finish();
						}
				}
		}
}
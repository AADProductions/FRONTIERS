using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class SkillEffectScript : MonoBehaviour, ISkillEffect
		{
				public Skill ParentSkill { get; set; }

				public double RTUpdateInterval { get; set; }

				public double RTEffectTime { get; set; }

				public IItemOfInterest TargetFXObject { get; set; }

				public string FXOnUpdate { get; set; }

				public double StartTime { get; set; }

				public void Start()
				{
						StartTime = WorldClock.Time;
						StartCoroutine(UpdateSkillEffectsOverTime());
						FXOnUpdate = ParentSkill.Effects.FXOnSuccess;
				}

				public virtual void UpdateEffects()
				{

				}

				public virtual void OnEffectStart()
				{

				}

				public IEnumerator UpdateSkillEffectsOverTime()
				{
						OnEffectStart();
						while (WorldClock.Time < (StartTime + WorldClock.RTSecondsToGameSeconds(RTEffectTime))) {
								FXManager.Get.SpawnFX(TargetFXObject, FXOnUpdate);
								UpdateEffects();
								yield return new WaitForSeconds((float)RTUpdateInterval);
						}
						Finish();
						yield break;
				}

				public void Finish()
				{
						GameObject.Destroy(this);
				}
		}

		public interface ISkillEffect
		{
				Skill ParentSkill { get; set; }

				double RTUpdateInterval { get; set; }

				double RTEffectTime { get; set; }

				string FXOnUpdate { get; set; }

				IItemOfInterest TargetFXObject { get; set; }
		}
}

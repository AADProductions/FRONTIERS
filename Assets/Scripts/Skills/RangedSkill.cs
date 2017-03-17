using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.World.Gameplay
{
		public class RangedSkill : Skill
		{
				public EffectSphere SkillSphere = null;
				public RangedSkillExtensions Extensions = new RangedSkillExtensions();

				protected override void OnUseStart()
				{
						GameObject skillSphere = new GameObject(name);
						skillSphere.transform.position = Player.Local.Position;
						SkillSphere = skillSphere.AddComponent (Type.GetType ("Frontiers.World.Gameplay." + Extensions.EffectSphereScriptName))  as EffectSphere;
						//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(skillSphere, "Assets/Scripts/Skills/RangedSkill.cs (18,21)", Extensions.EffectSphereScriptName) as EffectSphere;

						SkillSphere.TargetRadius = EffectRadius;
						SkillSphere.StartTime = WorldClock.AdjustedRealTime;
						SkillSphere.RTDuration = EffectTime;
						SkillSphere.RTCooldownTime = Usage.CooldownInterval;
						SkillSphere.RTExpansionTime = 1.0f;
						SkillSphere.OnIntersectItemOfInterest += OnIntersectItemOfInterest;
						SkillSphere.RequireLineOfSight = false;
				}

				public void OnIntersectItemOfInterest()
				{
						if (!IsInUse)
								return;

						if (!string.IsNullOrEmpty(Extensions.AddComponentOnUse)) {
								while (SkillSphere.ItemsOfInterest.Count > 0) {
										IItemOfInterest ioi = SkillSphere.ItemsOfInterest.Dequeue();
										//see if this is what we want
										if (ioi != null) {
												if (!(Extensions.IgnoreCaster && ioi.IOIType == ItemOfInterestType.Player && ioi.player == Player.Local)) {
														if (Flags.Check((uint)ioi.IOIType, (uint)Usage.TargetItemsOfInterest, Flags.CheckType.MatchAny) && ioi.HasAtLeastOne(Usage.TargetWIScriptNames)) {
																OnIntersectTarget(ioi);
														}
												}
										}
								}
						}
				}

				protected virtual void OnIntersectTarget(IItemOfInterest itemOfInterest)
				{
						if (!string.IsNullOrEmpty(Extensions.AddComponentOnUse)) {
								ISkillEffect effect = (ISkillEffect)itemOfInterest.gameObject.AddComponent (Type.GetType ("Frontiers.World.Gameplay." + Extensions.AddComponentOnUse));
								//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(itemOfInterest.gameObject, "Assets/Scripts/Skills/RangedSkill.cs (52,45)", Extensions.AddComponentOnUse);
								if (effect != null) {
										effect.ParentSkill = this;
										effect.RTEffectTime = EffectTime;
										if (Effects.SpawnFXOnTarget) {
												effect.TargetFXObject = itemOfInterest;
										} else {
												effect.TargetFXObject = Caster;
										}
										effect.FXOnUpdate = Effects.FXOnSuccess;
										effect.RTUpdateInterval = Extensions.SkillUpdateInterval;
								}
						}
				}

				protected override void OnUseFinish()
				{
						SkillSphere.CancelEffect();
				}
		}

		[Serializable]
		public class RangedSkillExtensions
		{
				public string EffectSphereScriptName = "EffectSphere";
				public string AddComponentOnUse = string.Empty;
				public string SendMessageOnTriggerExit = string.Empty;
				public float SkillUpdateInterval = 0.5f;
				public bool IgnoreCaster = true;
		}
}
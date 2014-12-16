using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;

namespace Frontiers
{
		public class PlayerDamageHandler : PlayerScript, IDamageable
		{
				protected float MinimumFallImpactThreshold = 5.0f;
				protected float MinimumBrokenBoneFallDamage	= 18.0f;
				protected float BrokenBoneProbability = 0.25f;
				protected float MaximumBrokenBoneFallDamage	= 30.0f;
				protected float MaximumFallImpactThreshold	= 45.0f;
				public float MinimumDamageThreshold = 1.25f;
				public float FallDamageMultiplier = 5f;
				public float DamageLastTaken = 0.0f;
				public string DamageLastSource = string.Empty;
				public float MeterValueMultiplier = 100;
				public AnimationCurve FallDamageCurve;

				public IItemOfInterest LastDamageSource { get; set; }

				public bool IsRigidBody {
						get {
								return false;
						}
				}

				public override void OnGameStart()
				{
						//keep this handy
						player.Status.GetStatusKeeper("Health", out mHealthStatusKeeper);
				}

				public float NormalizedDamage {
						get {
								return (1.0f - mHealthStatusKeeper.NormalizedValue);
						}
				}

				public float DamageTaken {
						get {
								return (100 - (mHealthStatusKeeper.NormalizedValue * MeterValueMultiplier));
						}
				}

				public float Durability {
						get {
								return MeterValueMultiplier;
						}
				}

				public bool IsDead {
						get {
								//let the status decide when we're actually dead
								return player.IsDead;
						}
				}

				public WIMaterialType BaseMaterialType {
						get {
								return WIMaterialType.Flesh;
						}
				}

				public WIMaterialType ArmorMaterialTypes {
						get {
								return player.Wearables.ArmorMaterialTypes;
						}
				}

				public int ArmorLevel(WIMaterialType type)
				{
						return player.Wearables.ArmorLevel(type);
				}

				public Shield DamageAbsorber;

				public bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string source, out float actualDamage, out bool isDead)
				{
						if (player.IsHijacked || !player.HasSpawned) {
								isDead = false;
								actualDamage = 0f;
								return false;
						}

						if (attemptedDamage < MinimumDamageThreshold || !mInitialized || !player.HasSpawned) {
								isDead = false;
								actualDamage = 0.0f;
								return false;
						}

						if (source != "FallDamage") {
								//do we have a shield? if so pass the damage along to the shield
								//other armor stuff has already been calculated by the damage manager
								//this is the only bit that it can't know about
								//(on characters a shield is treated as a piece of armor)
								bool hasDamageAbsorber = false;
								if (player.Carrier.HasWorldItem) {
										hasDamageAbsorber = player.Carrier.worlditem.Is <Shield>(out DamageAbsorber);
								} else if (player.Tool.HasWorldItem) {
										hasDamageAbsorber = player.Tool.worlditem.Is <Shield>(out DamageAbsorber);
								}

								if (hasDamageAbsorber) {
										bool absorberIsDead = false;
										bool absorbedDamage = DamageAbsorber.worlditem.Get <Damageable>().TakeDamage(
												                 materialType,
												                 damagePoint,
												                 attemptedDamage,
												                 attemptedForce,
												                 source,
												                 out actualDamage,
												                 out absorberIsDead);
										isDead = false;//obviously since the absorber took the heat
										return absorbedDamage;
								}
						}

						//otherwise just take the damage
						actualDamage = attemptedDamage;
						DamageLastTaken = actualDamage;
						DamageLastSource = source;
						isDead = IsDead;
						//we don't set actual death here
						Player.Local.Status.ReduceStatus(actualDamage / MeterValueMultiplier, "Health");
						Player.Local.FPSCamera.DoBomb(Vector3.one, 0.0001f * attemptedForce.magnitude, 0.001f * attemptedForce.magnitude);
						Player.Local.FPSController.AddSoftForce(attemptedForce, 3f);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalTakeDamage, WorldClock.Time);
						return true;
				}

				public void TakeFallDamage(float fallImpact)
				{
						if (player.IsHijacked || !player.HasSpawned || GameManager.Get.JustLookingMode) {
								return;
						}

						float actualDamage;
						float attemptedDamage = fallImpact * Globals.DamageFallDamageMultiplier;
						bool isDead;
						WIMaterialType fallOn = DamageManager.GroundTypeToMaterialType(Player.Local.Surroundings.State.GroundBeneathPlayer);

						if (attemptedDamage < Globals.DamageMinimumFallImpactThreshold) {
								return;
						} else if (attemptedDamage > Globals.DamageMaximumFallImpactThreshold) {
								player.Die("FallDamage");
								return;
						} else {
								TakeDamage(fallOn, Player.Local.Position, attemptedDamage, Vector3.zero, "FallDamage", out actualDamage, out isDead);
								bool breakBone = false;
								if (actualDamage >= MaximumBrokenBoneFallDamage) {
										breakBone = true;
								} else if (actualDamage >= MinimumBrokenBoneFallDamage) {
										if (Random.value >= BrokenBoneProbability) {
												breakBone = true;
										}
								}
				
								if (breakBone) {
										Player.Local.Status.AddCondition("BrokenBone");
								}
						}
				}

				protected StatusKeeper mHealthStatusKeeper;
		}
}
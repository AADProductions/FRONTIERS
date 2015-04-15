using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
		public class Damageable : WIScript, IDamageable
		{
				//what most WorldItems will use to take damage
				//implements IDamageable which is what the damage manager uses to apply damage
				//other WIScripts subscribe to its actions to make noise, play animations, explode, whatever
				public DamageableState State = new DamageableState();
				public Action OnTakeDamage;
				public Action OnForceApplied;
				public Action OnTakeCriticalDamage;
				public Action OnTakeOverkillDamage;
				public Action OnDie;
				public bool ApplyForceAutomatically = true;

				public IItemOfInterest LastDamageSource { get; set; }

				public BodyPart LastBodyPartHit { get; set; }

				public WIMaterialType BaseMaterialType {
						get {
								return worlditem.Props.Global.MaterialType;
						}
				}

				public WIMaterialType ArmorMaterialTypes {
						get {
								Armor armor = null;
								if (worlditem.Has <Armor>(out armor)) {
										return armor.MaterialTypes;
								}
								return WIMaterialType.None;
						}
				}

				public int ArmorLevel(WIMaterialType materialType)
				{
						Armor armor = null;
						if (worlditem.Has <Armor>(out armor)) {
								return armor.ArmorLevel(materialType);
						}
						return 0;
				}

				public bool IsDead {
						get {
								return State.IsDead;
						}
						set {
								if (value && !State.IsDead) {
										InstantKill(string.Empty);
								}
						}
				}

				public float NormalizedDamage {
						get {
								return State.NormalizedDamage;
						}
				}

				public void ResetDamage()
				{
						State.DamageTaken = 0f;
				}

				public void InstantKill(IItemOfInterest causeOfDeath) {
						Debug.Log("Instant kill in damageable");
						State.LastDamageTaken = Mathf.Clamp((State.Durability - State.DamageTaken), 0f, Mathf.Infinity);
						State.LastDamagePoint = transform.position;
						State.LastDamageMaterial = WIMaterialType.None;
						OnDieResult();
				}

				public void InstantKill(string causeOfDeath)
				{
						State.LastDamageTaken = Mathf.Clamp((State.Durability - State.DamageTaken), 0f, Mathf.Infinity);
						State.LastDamagePoint = transform.position;
						State.LastDamageMaterial = WIMaterialType.None;
						State.CauseOfDeath = causeOfDeath;
						OnDieResult();
				}

				public void InstantKill(WIMaterialType type, string causeOfDeath, bool spawnDamage)
				{
						State.LastDamageTaken = Mathf.Clamp((State.Durability - State.DamageTaken), 0f, Mathf.Infinity);
						State.LastDamagePoint = transform.position;
						State.LastDamageMaterial	= WIMaterialType.None;
						State.CauseOfDeath = causeOfDeath;

						if (spawnDamage) {
								//spawn damage
						}
						worlditem.SetMode(WIMode.Destroyed);
				}

				public virtual bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string sourceName, out float actualDamage, out bool isDead)
				{
						if (!mInitialized) {
								actualDamage = 0f;
								isDead = false;
								return false;
						}

						if (State.IsDead) {
								actualDamage = 0.0f;
								isDead = true;
								worlditem.ApplyForce(attemptedForce, damagePoint);
								if (LastBodyPartHit != null) {
										LastBodyPartHit.ForceOnConvertToRagdoll = attemptedForce;
								}
								return false;
						}

						actualDamage = attemptedDamage;			
						//this is where we apply body part modifiers and succeptibilities
						if (Flags.Check((uint)State.MaterialPenalties, (uint)materialType, Flags.CheckType.MatchAny)) {
								actualDamage *= Globals.DamageMaterialBonusMultiplier;//apply the bonus to this source
						} else if (Flags.Check((uint)State.MaterialBonuses, (uint)materialType, Flags.CheckType.MatchAny)) {
								actualDamage *= Globals.DamageMaterialPenaltyMultiplier;//apply the penalty to this source
						}

						if (State.SourcePenalties.Contains(sourceName)) {
								actualDamage *= Globals.DamageMaterialBonusMultiplier;//apply the bonus to this source
						} else if (State.SourceBonuses.Contains(sourceName)) {
								actualDamage *= Globals.DamageMaterialPenaltyMultiplier;//apply the penalty to this source
						}
			
						if (actualDamage > State.MinimumDamageThreshold) {
								//if we haven't taken a reputation penalty for damaging someone else's property
								//check and see if we're owned by someone else
								if (!State.HasCausedReputationPenalty) {
										if (WorldItems.IsOwnedBySomeoneOtherThanPlayer(worlditem, out mCheckOwner)) {
												//TODO tie reputation loss to item value
												Profile.Get.CurrentGame.Character.Rep.LosePersonalReputation(mCheckOwner.worlditem.FileName, mCheckOwner.worlditem.DisplayName, 1);
												State.HasCausedReputationPenalty = true;
										}
								}

								State.LastDamagePoint = damagePoint;
								State.LastDamageMaterial = materialType;
								State.LastDamageSource = sourceName;
								State.LastDamageForce = attemptedForce;
				
								State.LastDamageTaken = actualDamage;
								State.DamageTaken += actualDamage;

								//see if the force exceeds our 'throw' threshold
								if (attemptedForce.magnitude > State.MinimumForceThreshold) {
										if (ApplyForceAutomatically) {
												worlditem.ApplyForce(attemptedForce, damagePoint);
										} else {
												OnForceApplied.SafeInvoke();
										}
								}
								if (LastBodyPartHit != null) {
										LastBodyPartHit.ForceOnConvertToRagdoll = attemptedForce;
								}

								//now that we've set everything up, send the damage messages
								OnTakeDamage.SafeInvoke();
				
								if (actualDamage >= State.OverkillDamageThreshold) {
										State.LastDamageTaken = actualDamage * State.OverkillDamageMultiplier;
										State.DamageTaken += actualDamage * State.OverkillDamageMultiplier;
										OnTakeOverkillDamage.SafeInvoke();
								} else if (actualDamage >= State.CriticalDamageThreshold) {
										State.LastDamageTaken = actualDamage * State.CriticalDamageMultiplier;
										State.DamageTaken += actualDamage * State.CriticalDamageMultiplier;
										OnTakeCriticalDamage.SafeInvoke();
								}

								//now check to see if we're dead
								isDead = State.IsDead;
	
								if (isDead) {
										OnDieResult();
								}
								return true;
						}

						isDead = false;
						return false;
				}

				protected void OnDieResult()
				{
						State.TimeKilled = WorldClock.AdjustedRealTime;
						OnDie.SafeInvoke();
						switch (State.Result) {
								case DamageableResult.None:
								default:
										break;

								case DamageableResult.Die:
										worlditem.SetMode(WIMode.Destroyed);
										break;

								case DamageableResult.RemoveFromGame:
										worlditem.SetMode(WIMode.RemovedFromGame);
										break;

								case DamageableResult.State:
										if (!string.IsNullOrEmpty(State.StateResult)) {
												worlditem.State = State.StateResult;
										} else {
												Debug.Log("State was empty in damageable, setting to removed from game");
												worlditem.SetMode(WIMode.RemovedFromGame);
										}
										break;
						}
				}

				protected static Character mCheckOwner;
		}

		[Serializable]
		public class DamageableState
		{
				public bool IsDead {
						get {
								return DamageTaken >= Durability;
						}
				}

				public float Durability = 10.0f;
				public float DamageTaken = 0.0f;
				public float MinimumForceThreshold = 1f;
				public double TimeKilled = 0f;

				public virtual float NormalizedDamage {
						get {
								if (DamageTaken > 0.0f) {
										return Mathf.Clamp01(DamageTaken / Durability);
								} else {
										return 0.0f;
								}
						}
				}

				public DamageableResult Result = DamageableResult.RemoveFromGame;
				[BitMaskAttribute(typeof(WIMaterialType))]
				public WIMaterialType MaterialPenalties = WIMaterialType.None;
				public List <string> SourcePenalties = new List <string>();
				[BitMaskAttribute(typeof(WIMaterialType))]
				public WIMaterialType MaterialBonuses = WIMaterialType.None;
				public List <string> SourceBonuses = new List <string>();
				public float LastDamageTaken = 0.0f;
				public SVector3 LastDamagePoint = SVector3.zero;
				public SVector3 LastDamageForce = SVector3.zero;
				public WIMaterialType LastDamageMaterial = WIMaterialType.None;
				public string LastDamageSource = string.Empty;
				public float CriticalDamageThreshold = 10.0f;
				public float CriticalDamageMultiplier = 2.0f;
				public float OverkillDamageThreshold = 20.0f;
				public float OverkillDamageMultiplier = 3.0f;
				public float MinimumDamageThreshold = 0.25f;
				public string CauseOfDeath = string.Empty;
				public string StateResult;
				public bool HasCausedReputationPenalty = false;
		}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class Weapon : WIScript, IMeleeWeapon
		{
				public WeaponState State = new WeaponState();
				public PlayerToolStyle Style = PlayerToolStyle.Swing;
				public string ProjectileType = "Projectile";
				public float MinLaunchForce = 2.0f;
				public float MaxLaunchForce = 5.0f;

				#region IMeleeWeapon implementation

				public float ImpactTime { get { return State.BaseImpactTime; } }

				public float UseSpeed { get { return State.BaseUseSpeed; } }

				public float WindupDelay { get { return State.BaseWindupDelay; } }

				public float SwingDelay { get { return State.BaseSwingDelay; } }

				public float SwingRate { get { return State.BaseSwingRate; } }

				public float SwingDuration { get { return State.BaseSwingDuration; } }

				public float SwingImpactForce { get { return State.BaseSwingImpactForce; } }

				public float StrengthDrain { get { return State.BaseStrengthDrain; } }

				public float RecoilIntensity { get { return State.BaseRecoilIntensity; } }

				public bool RandomSwingDirection { get { return Style == PlayerToolStyle.Slice; } }

				public string AttackState { get { return State.BaseAttackState; } }
				//these variables are redundant so they will appear in the Unity editor
				//as well as being served up by the interface
				public float BaseImpactTime = 0.11f;
				public float BaseUseSpeed = 0.5f;
				public float BaseWindupDelay = 0.05f;
				public float BaseSwingDelay = 0.5f;
				public float BaseSwingRate = 0.5f;
				public float BaseSwingDuration = 0.5f;
				public float BaseSwingImpactForce = 1.0f;
				public float BaseStrengthDrain = 0.05f;
				public float BaseRecoilIntensity = 1.0f;
				public float BaseStrengthOnSwing = 0.25f;
				public string BaseAttackState = "ToLeft";
				public bool TakeCollisionDamage = false;

				#endregion

				public Transform ActionPointObject {
						get {
								if (mActionPointObject == null) {
										//should find one, creates one if it doesn't exist
										mActionPointObject = gameObject.FindOrCreateChild("ActionPointObject");
								}
								return mActionPointObject;
						}
				}

				public override void OnStartup()
				{
						State.Damage.SenderName = State.WeaponName;
						State.Damage.MaterialBonus = State.MaterialBonus;
						State.Damage.MaterialPenalty = State.MaterialPenalty;
						State.Damage.SenderMaterial = State.WeaponMaterial;
						State.Damage.OnPackageReceived += OnPackageReceived;
				}

				public override void InitializeTemplate()
				{
						base.InitializeTemplate();
						if (!State.HasInitializedValues) {
								State.BaseImpactTime = BaseImpactTime;
								State.BaseUseSpeed = BaseUseSpeed;
								State.BaseWindupDelay = BaseWindupDelay;
								State.BaseSwingDelay = BaseSwingDelay;
								State.BaseSwingRate = BaseSwingRate;
								State.BaseSwingDuration = BaseSwingDuration;
								State.BaseSwingImpactForce = BaseSwingImpactForce;
								State.BaseStrengthDrain = BaseStrengthDrain;
								State.BaseRecoilIntensity = BaseRecoilIntensity;
								State.BaseStrengthOnSwing = BaseStrengthOnSwing;
								State.BaseAttackState = BaseAttackState;
								State.ProjectileType = ProjectileType;
								State.HasInitializedValues = true;
						}
				}

				public override void OnInitialized()
				{
						if (!State.HasInitializedValues) {
								State.BaseImpactTime = BaseImpactTime;
								State.BaseUseSpeed = BaseUseSpeed;
								State.BaseWindupDelay = BaseWindupDelay;
								State.BaseSwingDelay = BaseSwingDelay;
								State.BaseSwingRate = BaseSwingRate;
								State.BaseSwingDuration = BaseSwingDuration;
								State.BaseSwingImpactForce = BaseSwingImpactForce;
								State.BaseStrengthDrain = BaseStrengthDrain;
								State.BaseRecoilIntensity = BaseRecoilIntensity;
								State.BaseStrengthOnSwing = BaseStrengthOnSwing;
								State.BaseAttackState = BaseAttackState;
								State.ProjectileType = ProjectileType;
								State.HasInitializedValues = true;
						} else {
								BaseImpactTime = State.BaseImpactTime;
								BaseUseSpeed = State.BaseUseSpeed;
								BaseWindupDelay = State.BaseWindupDelay;
								BaseSwingDelay = State.BaseSwingDelay;
								BaseSwingRate = State.BaseSwingRate;
								BaseSwingDuration = State.BaseSwingDuration;
								BaseSwingImpactForce = State.BaseSwingImpactForce;
								BaseStrengthDrain = State.BaseStrengthDrain;
								BaseRecoilIntensity = State.BaseRecoilIntensity;
								BaseStrengthOnSwing = State.BaseStrengthOnSwing;
								BaseAttackState = State.BaseAttackState;
						}

						Equippable equippable = null;
						if (worlditem.Is <Equippable>(out equippable)) {
								equippable.OnUseStart += OnUseStart;
						}
				}

				public bool ModifyWeapon(Skill skill)
				{
						WeaponSkillModifier wsm = null;
						if (worlditem.Has <WeaponSkillModifier>(out wsm)) {
								GUI.GUIManager.PostDanger("This weapon is already modified with " + wsm.ParentSkill.DisplayName);
								return false;
						} else {
								wsm = worlditem.GetOrAdd <WeaponSkillModifier>();
								wsm.State.TimeApplied = WorldClock.AdjustedRealTime;
								wsm.State.Duration = skill.EffectTime;
								wsm.ParentSkill = skill as RangedSkill;
						}
						return true;
				}

				public void Improve()
				{
						State.NumTimesImproved++;
				}

				public void OnUseStart()
				{
						State.StrengthOnLastSwing = Player.Local.Status.GetStatusValue("Strength");
						Player.Local.Status.ReduceStatus(PlayerStatusRestore.F_Full, "Strength", StrengthDrain);
				}

				public void OnMiss()
				{

				}

				public DamagePackage GetDamagePackage(Vector3 point, IItemOfInterest target)
				{
						State.Damage.Point = point;
						State.Damage.Origin = Player.Local.Position;
						State.Damage.DamageSent = DamagePerHit(this);
						State.Damage.ForceSent = ForcePerHit(this);
						State.Damage.Target = target;
						State.Damage.Source = worlditem;

						return State.Damage;
				}

				public void OnPackageSent()
				{
						State.NumTimesUsed++;
						State.DamageSent += State.Damage.DamageSent;
				}

				public void OnPackageReceived()
				{
						if (State.Damage.HitTarget) {
								State.NumTimesUsedSuccessfully++;
								State.DamageDealt += State.Damage.DamageDealt;
								if (State.Damage.TargetIsDead) {
										State.NumTimesKilledTarget++;
								}
						}
				}

				protected Transform mActionPointObject = null;

				public static float HitInterval(Weapon weapon)
				{
						float hitInterval = weapon.State.BaseHitInterval;
						return hitInterval;
				}

				public static float DamagePerHit(Weapon weapon)
				{		
						float damagePerHit = weapon.State.BaseDamagePerHit * Mathf.Max(weapon.BaseStrengthOnSwing, weapon.State.StrengthOnLastSwing);
						/*
						if (weapon.State.HasBeenImproved)
						{
							damagePerHit += (weapon.State.BaseDamagePerHit * weapon.State.NumTimesImproved);
						}
						if (weapon.worlditem.Is<Cursed> ( ))
						{
							damagePerHit += (weapon.State.BaseDamagePerHit * 3.0f);
						}
						*/
						return damagePerHit;
				}

				public static float ForcePerHit(Weapon weapon)
				{
						//TODO improve
						return weapon.SwingImpactForce * Mathf.Max(weapon.BaseStrengthOnSwing, weapon.State.StrengthOnLastSwing);
				}

				public static bool CanLaunch(Weapon weapon, IWIBase projectileBase)
				{
						if (weapon == null || projectileBase == null) {
								return false;//whoops
						}

						if (!projectileBase.Is <Projectile>()) {
								return false;//can't launch it if it's not a projectile
						}

						//find out of the projectile keyword matches
						WorldItem projectileWorldItem = null;
						if (projectileBase.IsWorldItem) {
								projectileWorldItem = projectileBase.worlditem;
						} else if (!WorldItems.Get.PackPrefab(projectileBase.PackName, projectileBase.PrefabName, out projectileWorldItem)) {
								return false;
						}
						//alright, see if the projectile type matches
						Projectile projectile = null;
						if (projectileWorldItem.Is <Projectile>(out projectile)) {
								return weapon.ProjectileType == projectile.ProjectileType;
						}
				
						return false;
				}

				public static int CalculateLocalPrice(int baseValue, IWIBase item)
				{
						if (item == null)
								return baseValue;

						object weaponStateObject = null;
						if (item.GetStateOf <Weapon>(out weaponStateObject)) {
								WeaponState w = (WeaponState)weaponStateObject;
								if (w != null) {
										//Debug.Log("Adding to base value of weapon, " + baseValue.ToString());
										float delays = w.BaseSwingDelay + w.BaseWindupDelay + w.BaseEquipInterval + w.BaseHitInterval + w.BaseSwingDuration + w.BaseSwingRate + w.BaseImpactTime;
										baseValue += Mathf.CeilToInt(Mathf.Clamp(20f - delays, 0f, Mathf.Infinity) * Globals.BaseValueWeaponDelayInterval);
										baseValue += Mathf.CeilToInt(w.BaseDamagePerHit * Globals.BaseValueWeaponDamagePerHit);
										baseValue += Mathf.CeilToInt(w.BaseForcePerHit * Globals.BaseValueWeaponForcePerHit);
										baseValue += Mathf.CeilToInt(0.5f - w.BaseStrengthDrain * Globals.BaseValueWeaponStrengthDrain);
										if (w.HasBeenImproved) {
												baseValue = baseValue * w.NumTimesImproved;
										}
										baseValue += w.NumTimesUsedSuccessfully;
										baseValue += w.NumTimesKilledTarget * 10;
										if (!string.IsNullOrEmpty(w.ProjectileType)) {
												baseValue = Mathf.CeilToInt(baseValue * Globals.BaseValueWeaponProjectileMultiplier);
										}
								}
						} else {
								Debug.Log("Couldn't get state");
						}
						return baseValue;
				}
		}

		[Serializable]
		public class WeaponState
		{
				public DamagePackage Damage = new DamagePackage();

				public bool HasBeenImproved {
						get {
								return NumTimesImproved > 0;
						}
				}

				public string ProjectileType;
				public bool HasInitializedValues = false;
				public float BaseImpactTime = 0.11f;
				public float BaseUseSpeed = 0.5f;
				public float BaseWindupDelay = 0.05f;
				public float BaseSwingDelay = 0.5f;
				public float BaseSwingRate = 0.5f;
				public float BaseSwingDuration = 0.5f;
				public float BaseSwingImpactForce = 1.0f;
				public float BaseStrengthDrain = 0.05f;
				public float BaseRecoilIntensity = 1.0f;
				public float BaseStrengthOnSwing = 0.25f;
				public string BaseAttackState = "ToLeft";
				public float StrengthOnLastSwing = 1.0f;
				public string WeaponName;
				public int NumTimesImproved = 0;
				public int MaxTimesImproved = 10;
				public int NumTimesUsed = 0;
				public int NumTimesUsedSuccessfully	= 0;
				public int NumTimesKilledTarget = 0;
				public float DamageDealt = 0.0f;
				public float DamageSent = 0.0f;
				public float BaseForcePerHit = 1.0f;
				public float BaseDamagePerHit = 10.0f;
				public float BaseHitInterval = 1.0f;
				public float BaseEquipInterval = 0.25f;
				public string BaseRequiredSkill = string.Empty;
				public WIMaterialType WeaponMaterial = WIMaterialType.Metal;
				public WIMaterialType MaterialBonus = WIMaterialType.None;
				public WIMaterialType MaterialPenalty = WIMaterialType.None;
		}
}
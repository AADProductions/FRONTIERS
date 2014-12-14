using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class Weapon : WIScript, IMeleeWeapon
		{
				public WeaponState State = new WeaponState();
				public PlayerToolStyle Style = PlayerToolStyle.Swing;
				public string ProjectileType = "Projectile";
				public float MinLaunchForce = 2.0f;
				public float MaxLaunchForce = 5.0f;

				#region IMeleeWeapon implementation

				public float ImpactTime { get { return BaseImpactTime; } }

				public float UseSpeed { get { return BaseUseSpeed; } }

				public float WindupDelay { get { return BaseWindupDelay; } }

				public float SwingDelay { get { return BaseSwingDelay; } }

				public float SwingRate { get { return BaseSwingRate; } }

				public float SwingDuration { get { return BaseSwingDuration; } }

				public float SwingImpactForce { get { return BaseSwingImpactForce; } }

				public float StrengthDrain { get { return BaseStrengthDrain; } }

				public float RecoilIntensity { get { return BaseRecoilIntensity; } }

				public bool RandomSwingDirection { get { return Style == PlayerToolStyle.Slice; } }

				public string AttackState { get { return BaseAttackState; } }

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
				public float StrengthOnLastSwing = 1.0f;
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

				public override void OnInitialized()
				{
						Equippable equippable = null;
						if (worlditem.Is <Equippable>(out equippable)) {
								equippable.OnUseStart += OnUseStart;
						}
				}

				public bool ModifyWeapon(Skill skill)
				{
						WeaponSkillModifier wsm = null;
						if (worlditem.Has <WeaponSkillModifier>(out wsm)) {
								GUIManager.PostDanger("This weapon is already modified with " + wsm.ParentSkill.DisplayName);
								return false;
						} else {
								wsm = worlditem.GetOrAdd <WeaponSkillModifier>();
								wsm.State.TimeApplied = WorldClock.Time;
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
						StrengthOnLastSwing = Player.Local.Status.GetStatusValue("Strength");
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
						float damagePerHit = weapon.State.BaseDamagePerHit * Mathf.Max(weapon.BaseStrengthOnSwing, weapon.StrengthOnLastSwing);
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
						return weapon.SwingImpactForce * Mathf.Max(weapon.BaseStrengthOnSwing, weapon.StrengthOnLastSwing);
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
		}

		public interface IMeleeWeapon
		{
				float ImpactTime { get; }

				float UseSpeed { get; }

				float SwingDelay { get; }

				float WindupDelay { get; }

				float SwingRate { get; }

				float SwingDuration { get; }

				float SwingImpactForce { get; }

				float StrengthDrain { get; }

				float RecoilIntensity { get; }

				bool RandomSwingDirection { get; }

				string AttackState { get; }
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
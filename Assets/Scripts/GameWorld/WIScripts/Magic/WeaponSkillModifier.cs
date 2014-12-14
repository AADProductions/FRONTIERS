using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
		public class WeaponSkillModifier : WIScript
		{
				public WeaponSkillModifierState State = new WeaponSkillModifierState();

				public double TimeApplied {
						get {
								return State.TimeApplied;
						}
						set {
								State.TimeApplied = value;
						}
				}

				public RangedSkill ParentSkill {
						get {
								if (mParentSkill == null) {
										Skill skill = null;
										if (Skills.Get.SkillByName(State.ParentSkillName, out skill)) {
												mParentSkill = skill as RangedSkill;
										}
								}
								return mParentSkill;
						}
						set {
								mParentSkill = value;
						}
				}

				public string ParentSkillName {
						get {
								return State.ParentSkillName;
						}
						set {
								State.ParentSkillName = value;
								Skill skill = null;
								if (Skills.Get.SkillByName(State.ParentSkillName, out skill)) {
										mParentSkill = skill as RangedSkill;
								}
						}
				}

				public override void OnInitialized()
				{
						Weapon weapon = null;
						if (worlditem.Is <Weapon>(out weapon)) {
								//subscribe to this so we know when we hit something with this weapon
								weapon.State.Damage.OnPackageReceived += OnPackageReceived;
						}
						Equippable equippable = null;
						if (worlditem.Is <Equippable>(out equippable)) {
								//subscribe to this so we know when the weapons is equipped
								//(for fx purposes)
								equippable.OnEquip += OnEquip;
								equippable.OnUnequip += OnUnequip;
						}
				}

				public void OnEquip()
				{

				}

				public void OnUnequip()
				{

				}

				public void OnPackageReceived()
				{
						Weapon weapon = worlditem.Get <Weapon>();
						if (weapon.State.Damage.HitTarget) {
								Skill skill = null;
								if (Skills.Get.SkillByName(ParentSkillName, out skill)) {
										//apply curse skill to target
										SkillEffectScript ses = weapon.State.Damage.Target.gameObject.AddComponent(mParentSkill.Extensions.AddComponentOnUse) as SkillEffectScript;
										ses.ParentSkill = ParentSkill;
								}
						}
				}

				protected RangedSkill mParentSkill;
		}

		[Serializable]
		public class WeaponSkillModifierState
		{
				public double TimeApplied;
				public double Duration;
				public string ParentSkillName;
		}
}
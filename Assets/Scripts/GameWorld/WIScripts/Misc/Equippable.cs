using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class Equippable : WIScript
		{
				public ToolEffects Effects = new ToolEffects();
				public ToolStates States = new ToolStates();
				public ToolSounds Sounds = new ToolSounds();
				public Vector3 EquipOffset = Vector3.zero;
				public Vector3 EquipRotation = Vector3.zero;
				public PlayerToolType Type = PlayerToolType.Generic;
				public bool ForceEquip = false;
				public bool DisplayAsTool = true;
				public bool UseDoppleganger = true;
				public string HandlingSkill = string.Empty;
				public float DefaultToolHandling = 1.0f;

				public Action OnEquip;
				public Action OnUnequip;
				public Action OnUseStart;
				public Action OnUseFinish;
				public Action OnCyclePrev;
				public Action OnCycleNext;

				public IEquippableAction CustomAction;

				public bool UseSkillForHandling {
						get {
								return !string.IsNullOrEmpty(HandlingSkill);
						}
				}

				public bool HasCustomAction {
						get {
								return CustomAction != null;
						}
				}

				public void OnStateChange()
				{
						//make sure we're still ignoring collisions
						mRefreshColliders = true;
				}

				public void EquipStart()
				{
						if (!mCheckingEquippedStatus) {
								mCheckingEquippedStatus = true;
								StartCoroutine(CheckEquippedStatus());
						}
						//do this next frame
						mRefreshColliders = true;
				}

				public void RefreshColliders ()
				{
						mRefreshColliders = true;
				}

				public void EquipFinish()
				{
						OnEquip.SafeInvoke();
				}

				public float ToolHandling {
						get {
								if (UseSkillForHandling) {
										if (mToolHandlingSkill == null) {
												Skills.Get.SkillByName(HandlingSkill, out mToolHandlingSkill);
										}
										//skill handling is based on the inverse of normalized usage
										//then multiplied by the default tool handling
										//as skill level increases
										//tool handling multiplier decreases
										//which reduces bob and weave
										return (1.0f - mToolHandlingSkill.State.NormalizedUsageLevel) * DefaultToolHandling;
								}
								//if we're not using a skill for handling
								//return the default skill
								return DefaultToolHandling;
						}
				}

				public void UseStart()
				{
						OnUseStart.SafeInvoke();
						MasterAudio.PlaySound(Sounds.SoundType, Player.Local.Tool.transform, Sounds.SoundUseStart);
				}

				public void UseFinish()
				{ 
						OnUseFinish.SafeInvoke();
				}

				public void UseSuccessfully()
				{
						if (Sounds.HasUseSuccessfullySound) {
								MasterAudio.PlaySound(Sounds.SoundType, Player.Local.Tool.transform, Sounds.SoundUseSuccessfully);
						}
				}

				public void UseUnsuccessfully()
				{
						if (Sounds.HasUseUnsuccessfullySound) {
								MasterAudio.PlaySound(Sounds.SoundType, Player.Local.Tool.transform, Sounds.SoundUseUnuccessfully);
						}
				}

				public bool CanCycle {
						get {
								if (HasCustomAction) {
										return CustomAction.CanCycle;
								}
								return false;
						}
				}

				public bool CyclingFinished {
						get {
								if (HasCustomAction) {
										return !CustomAction.IsCycling;
								}
								return true;
						}
				}

				public bool CustomActionFinished {
						get {
								return HasCustomAction && !CustomAction.IsActive;
						}
				}

				public override void OnModeChange()
				{
						if (worlditem.Is(WIMode.Equipped)) {
								worlditem.ActiveStateLocked = false;
								worlditem.ActiveState = WIActiveState.Visible;//we don't want our colliders to be enabled
								worlditem.ActiveStateLocked = true;
								worlditem.rigidbody.isKinematic	= true;
								//this will put us back in our group where we belong
								worlditem.UnlockTransform();
								//the tool will lock our transform on equip
								mRefreshColliders = true;
						}
				}

				protected IEnumerator CheckEquippedStatus()
				{
						//periodically check that we're still equipped
						//if we're not, send on unequipped
						//this should cover being destroyed
						while (worlditem.Is(WIMode.Equipped)) {
								//this is turning out to be necessary more often than i thought
								//so i'm just doing it every frame
								//it's a cheap process so whatever
								//if (mRefreshColliders) {
										for (int i = 0; i < worlditem.Colliders.Count; i++) {
												if (worlditem.Colliders[i].enabled && worlditem.Colliders[i].gameObject.activeSelf && Player.Local.Controller.enabled) {
														//this will get reset the next time they're disabled
														Physics.IgnoreCollision(worlditem.Colliders[i], Player.Local.Controller);
												}
										}
								//mRefreshColliders = false;
								//}
								yield return null;
						}
						mCheckingEquippedStatus = false;
						OnUnequip.SafeInvoke();
				}

				protected bool mRefreshColliders = false;
				protected bool mCheckingEquippedStatus = false;
				protected Skill mToolHandlingSkill = null;
		}

		[Serializable]
		public class ToolEffects
		{
				public string EffectOnEquip = string.Empty;
				public string EffectOnUnequip = string.Empty;
				public string EffectOnUseStart = string.Empty;
				public string EffectOnUseFinish = string.Empty;
				public List <string> EffectOnUseSuccessfully = new List <string>();
		}

		[Serializable]
		public class ToolStates
		{
				public string BaseToolState = string.Empty;
				public List <string> UseToolStates = new List <string>();
		}

		[Serializable]
		public class ToolSounds
		{
				public bool HasUseSuccessfullySound {
						get { 
								return SoundsUseSuccessfully.Count > 0;
						}
				}

				public bool HasUseUnsuccessfullySound {
						get { 
								return SoundsUseUnsuccessfully.Count > 0;
						}
				}

				public MasterAudio.SoundType SoundType = MasterAudio.SoundType.WeaponsMelee;
				public string SoundOnEquip = string.Empty;
				public string SoundOnUnequip = string.Empty;
				public string SoundUseStart = string.Empty;
				public string SoundUseFinish = string.Empty;

				public string SoundUseSuccessfully {
						get {
								if (SoundsUseUnsuccessfully.Count > 0) {
										return SoundsUseUnsuccessfully[UnityEngine.Random.Range(0, SoundsUseUnsuccessfully.Count)];
								}
								return string.Empty;
						}
				}

				public string SoundUseUnuccessfully {
						get {
								if (SoundsUseSuccessfully.Count > 0) {
										return SoundsUseSuccessfully[UnityEngine.Random.Range(0, SoundsUseSuccessfully.Count)];
								}
								return string.Empty;
						}
				}

				public List <string> SoundsUseSuccessfully	= new List <string>();
				public List <string> SoundsUseUnsuccessfully	= new List <string>();
		}
}
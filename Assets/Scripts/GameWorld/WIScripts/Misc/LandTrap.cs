using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.World.Locations;

namespace Frontiers.World
{
		public class LandTrap : WIScript, ITrap
		{
				public LandTrapState State = new LandTrapState();

				#region ITrap implementation

				public double TimeLastChecked { get { return State.TimeLastChecked; } set { State.TimeLastChecked = value; } }

				public double TimeSet { get { return State.TimeSet; } }

				public float SkillOnSet { get { return State.SkillOnSet; } }

				public WorldItem Owner { get { return worlditem; } }

				public string TrappingSkillName { get { return "Trapping"; } }

				public List <string> Exceptions {
						get {
								return gExceptions;
						}
				}

				public List <string> CanCatch {
						get {
								return gCanCatch;
						}
				}

				public bool SkillUpdating { get; set; }

				public TrapMode Mode {
						get {
								return State.Mode;
						}
						set {
								State.Mode = value;
								Refresh();
						}
				}

				public List <CreatureDen> IntersectingDens { get { return mIntersectingDens; } }

				#endregion

				public string AnimationOpenClipName;
				public string AnimationCloseClipName;
				public float BaseDamageOnTrap = 0f;

				public override bool CanBeCarried {
						get {
								return Mode != TrapMode.Set;
						}
				}

				public override bool CanEnterInventory {
						get {
								return Mode != TrapMode.Set;
						}
				}

				public override void OnInitialized()
				{
						switch (Mode) {
								case TrapMode.Set:
										animation.Play(AnimationOpenClipName);
										break;

								default:
										animation.Play(AnimationCloseClipName);
										break;
						}
				}

				public override void PopulateExamineList(List <WIExamineInfo> examine)
				{
						switch (Mode) {
								case TrapMode.Disabled:
										examine.Add(new WIExamineInfo("It has not been set."));
										break;

								case TrapMode.Misfired:
										examine.Add(new WIExamineInfo("It has misfired."));
										break;

								case TrapMode.Set:
										examine.Add(new WIExamineInfo("It has been set with " + Skill.MasteryAdjective(State.SkillOnSet) + " skill."));
										break;

								case TrapMode.Triggered:
										examine.Add(new WIExamineInfo("It has triggered successfully."));
										break;
						}
				}

				public void Refresh()
				{
						if (Mode == TrapMode.Set && !SkillUpdating) {
								Skill skill = null;
								if (Skills.Get.HasLearnedSkill(TrappingSkillName, out skill)) {
										TrappingSkill trappingSkill = skill as TrappingSkill;
										trappingSkill.UpdateTrap(this);
								}
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (other.isTrigger) {
								return;
						}

						IItemOfInterest target = null;
						if (!WorldItems.GetIOIFromCollider(other, out target)) {
								return;
						}

						switch (Mode) {
								case TrapMode.Set:
										//uh oh
										//what kind of object are we
										switch (target.IOIType) {
												case ItemOfInterestType.Player:
														//there's a chance it won't trigger
														Skill lightStepSkill = null;
														if (Skills.Get.LearnedSkill("LightStep", out lightStepSkill)) {
																//TODO this is kind of a kludge, it should be a player script modifier
																//a la motor or visibility
																//we may be able to make it not trigger at all
																if (lightStepSkill.State.MasteryLevel > UnityEngine.Random.value) {
																		//don't trigger the trap, just alert the player
																		//SKILL USE
																		lightStepSkill.Use(true);
																		//the skill use will announce what happens
																		//we don't trigger it!
																		return;
																}
														}
														if (TryToTrigger(target)) {
																Player.Local.Status.AddCondition("BrokenBone");
														}
														break;

												case ItemOfInterestType.WorldItem:
														TryToTrigger(target);
														break;

												default:
														break;
										}
										break;

								default:
										break;
						}
				}

				public bool TryToTrigger(IItemOfInterest target)
				{
						//otherwise just trust to chance
						//how well was the trap set?
						if (UnityEngine.Random.value > State.SkillOnSet) {
								Misfire();
								return false;
						} else {
								Trigger(target);
								return true;
						}
				}

				public void Misfire()
				{
						//TODO play misfire sound
						Mode = TrapMode.Misfired;
						animation.Play(AnimationCloseClipName);
						State.NumTimesMisfired++;
				}

				public bool TryToSet(float skillLevel)
				{
						if (Mode == TrapMode.Set) {
								Mode = TrapMode.Disabled;
								animation.Play(AnimationCloseClipName);
								return true;
						} else {
								Mode = TrapMode.Set;
								MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapSet");
								State.SkillOnSet = skillLevel;// TEMP TODO link this to skill
								animation.Play(AnimationOpenClipName);
								return true;
						}
				}

				protected void Trigger(IItemOfInterest target)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapTrigger");
						//AUGH we're damaged
						State.NumTimesTriggered++;
						Mode = TrapMode.Triggered;
						if (target != null) {
								//send damage
								State.Damage.DamageSent = BaseDamageOnTrap * State.SkillOnSet;
								State.Damage.Point = transform.position;
								State.Damage.SenderMaterial = worlditem.Props.Global.MaterialType;
								State.Damage.SenderName = worlditem.DisplayName;
								State.Damage.ForceSent = 0f;
								State.Damage.Target = target;
								DamageManager.Get.SendDamage(State.Damage);
						}
						animation.Play(AnimationCloseClipName);
				}

				protected List <CreatureDen> mIntersectingDens = new List<CreatureDen>();
				protected List <string> gExceptions = new List <string>() { "Fish" };
				protected List <string> gCanCatch = new List <string>();
		}

		public interface ITrap
		{
				double TimeLastChecked { get; set; }

				double TimeSet { get; }

				float SkillOnSet { get; }

				WorldItem Owner { get; }

				bool IsFinished { get; }

				string TrappingSkillName { get; }

				TrapMode Mode { get; set; }

				bool SkillUpdating { get; set; }

				List <string> CanCatch { get; }

				List <string> Exceptions { get; }

				List <CreatureDen> IntersectingDens { get; }
		}

		[Serializable]
		public class LandTrapState
		{
				public double TimeLastChecked = 0f;
				public TrapMode Mode = TrapMode.Disabled;
				public int NumTimesTriggered = 0;
				public int NumTimesMisfired = 0;
				public float SkillOnSet = 0f;
				public double TimeSet = 0f;
				public DamagePackage Damage = new DamagePackage();
		}

		public enum TrapMode
		{
				Set,
				Triggered,
				Misfired,
				Disabled,
		}
}
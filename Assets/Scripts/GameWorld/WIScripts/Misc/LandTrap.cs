using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class LandTrap : WIScript, ITrap
		{
				public LandTrapState State = new LandTrapState();

				#region ITrap implementation

				public double TimeLastChecked { get { return State.TimeLastChecked; } set { State.TimeLastChecked = value; } }

				public double TimeSet { get { return State.TimeSet; } }

				public float SkillOnSet { 
						get { return State.SkillOnSet; } 
						set {
								State.SkillOnSet = value;
								worlditem.RefreshHud();
						}
				}

				public bool RequiresMinimumPlayerDistance { get { return true; } }

				public WorldItem Owner { get { return worlditem; } }

				public string TrappingSkillName { get { return "Trapping"; } }

				public bool LastTriggerWasSuccessful { get; set; }

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

				public float NormalizedChanceOfSuccess {
						get {
								float chanceOfSuccess = 0f;
								int numDens = IntersectingDens.Count;
								//Debug.Log("Checking normalized chance of success - num intersecting dens: " + numDens.ToString());
								for (int i = 0; i < IntersectingDens.Count; i++) {
										float distanceToDen = Vector3.Distance(IntersectingDens[i].Position, worlditem.Position) - IntersectingDens[i].InnerRadius;
										float normalizedChanceForThisDen = (distanceToDen / (IntersectingDens[i].Radius - IntersectingDens[i].InnerRadius));
										chanceOfSuccess += normalizedChanceForThisDen;
										//Debug.Log("Distance to den: " + distanceToDen.ToString() + ", normalized chance: " + normalizedChanceForThisDen.ToString());
								}
								if (numDens > 0) {
										chanceOfSuccess /= numDens;
								}
								return (chanceOfSuccess + State.SkillOnSet) / 2;
						}
				}

				public TrapMode Mode {
						get {
								return State.Mode;
						}
						set {
								State.Mode = value;
								Refresh();
						}
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						if (Mode == TrapMode.Set) {
								GUI.GUIHud.Get.ShowProgressBar(Colors.Get.GenericHighValue, Colors.Get.GenericLowValue, NormalizedChanceOfSuccess);
						}
						return lastHudPriority;
				}

				public List <ICreatureDen> IntersectingDens { get { return mIntersectingDens; } }

				public void OnCatchTarget(float skillRoll)
				{
						Mode = TrapMode.Triggered;
						LastTriggerWasSuccessful = true;
				}

				#endregion

				public string AnimationOpenClipName;
				public string AnimationCloseClipName;
				public float BaseDamageOnTrap = 0f;
				public Collider TrapCollider;

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
						SkillUpdating = false;
						switch (Mode) {
								case TrapMode.Set:
										animation.Play(AnimationOpenClipName);
										Refresh();
										break;

								default:
										animation.Play(AnimationCloseClipName);
										break;
						}
						TrapCollider.enabled = true;
						TrapCollider.isTrigger = true;
						TrapCollider.gameObject.layer = Globals.LayerNumTrigger;

						worlditem.OnPlayerEncounter += OnPlayerEncounter;
				}

				public void OnPlayerEncounter()
				{
						if (LastTriggerWasSuccessful) {
								LastTriggerWasSuccessful = false;
								GUI.GUIManager.PostSuccess("Trap successfully caught something");
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
										if (IntersectingDens.Count > 0) {
												examine.Add(new WIExamineInfo("It has been set with " + Skill.MasteryAdjective(State.SkillOnSet) + " skill. The odds of it catching something are " + Skill.MasteryAdjective(NormalizedChanceOfSuccess)));
										} else {
												examine.Add(new WIExamineInfo("It has been set with " + Skill.MasteryAdjective(State.SkillOnSet) + " skill. There is no chance it will catch something because there are no animals nearby."));
										}
										break;

								case TrapMode.Triggered:
										examine.Add(new WIExamineInfo("The trap is full."));
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
								Debug.Log("Misfired!");
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
						MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapTrigger");
						GUI.GUIManager.PostWarning("Trap misfired");
						State.NumTimesMisfired++;
				}

				public bool TryToSet(float skillLevel)
				{
						if (Mode == TrapMode.Set) {
								Mode = TrapMode.Disabled;
								animation.Play(AnimationCloseClipName);
								worlditem.RefreshHud();
								return true;
						} else {
								Mode = TrapMode.Set;
								MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapSet");
								State.SkillOnSet = skillLevel;
								animation.Play(AnimationOpenClipName);
								worlditem.RefreshHud();
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

				protected List <ICreatureDen> mIntersectingDens = new List<ICreatureDen>();
				protected List <string> gExceptions = new List <string>() { "Fish", "Orb" };
				protected List <string> gCanCatch = new List <string>();
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
				public float NormalizedDistanceFromNearestCreatureDen = 0f;
				public DamagePackage Damage = new DamagePackage();
		}
}
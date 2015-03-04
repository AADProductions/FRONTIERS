using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class WaterTrap : WIScript, ITrap
		{
				public Container container = null;
				public WaterTrapState State = new WaterTrapState();
				public string FillCategoryName = "WaterTrapItems";

				#region ITrap implementation

				public override int OnRefreshHud(int lastHudPriority)
				{
						container = worlditem.Get <Container>();

						lastHudPriority++;
						if (Mode == TrapMode.Set && Skills.Get.HasLearnedSkill("Fishing")) {
								GUI.GUIHud.Get.ShowProgressBar(Colors.Get.GenericHighValue, Colors.Get.GenericLowValue, NormalizedChanceOfSuccess);
						}
						if (worlditem.StackContainer.IsEmpty) {
								container.CanOpen = false;
								container.CanUseToOpen = false;
								GUI.GUIHud.Get.ShowActions(worlditem, UserActionType.ItemUse, UserActionType.ItemInteract, "Pick up (Empty)", "Interact", worlditem.HudTarget, GameManager.Get.GameCamera);
						} else if (worlditem.StackContainer.IsFull) {
								GUI.GUIHud.Get.ShowActions(worlditem, UserActionType.ItemUse, UserActionType.ItemInteract, "Pick up (Full)", "Interact", worlditem.HudTarget, GameManager.Get.GameCamera);
								container.CanOpen = true;
								container.CanUseToOpen = true;
								container.OpenText = "Open Trap";
						} else {
								GUI.GUIHud.Get.ShowActions(worlditem, UserActionType.ItemUse, UserActionType.ItemInteract, "Pick up " + worlditem.StackContainer.NumItems.ToString() + " item(s)", "Interact", worlditem.HudTarget, GameManager.Get.GameCamera);
								container.CanOpen = true;
								container.CanUseToOpen = true;
								container.OpenText = "Open Trap";
						}
						return lastHudPriority;
				}

				public double TimeLastChecked { get { return State.TimeLastChecked; } set { State.TimeLastChecked = value; } }

				public double TimeSet { get { return State.TimeSet; } }

				public float SkillOnSet { 
						get { return State.SkillOnSet; } 
						set {
								State.SkillOnSet = value;
								worlditem.RefreshHud();
						}
				}

				public WorldItem Owner { get { return worlditem; } }

				public string TrappingSkillName { get { return "Fishing"; } }

				public bool LastTriggerWasSuccessful { get; set; }

				public bool RequiresMinimumPlayerDistance { get { return false; } }

				public List <string> CanCatch {
						get {
								return gCanCatch;
						}
				}

				public List <string> Exceptions {
						get {
								return gExceptions;
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

				public List <ICreatureDen> IntersectingDens { get { return mIntersectingDens; } }

				public void OnCatchTarget(float skillRoll)
				{
						//Debug.Log("Caught target with skill roll " + skillRoll.ToString() + " in trap " + name);
						//unlike land traps fish traps just fill a container with fish items
						//so do that now
						Mode = TrapMode.Triggered;
						LastTriggerWasSuccessful = true;
						FillTrapWithGoodies(skillRoll);
				}

				#endregion

				public string AnimationOpenClipName;
				public string AnimationCloseClipName;
				public string AnimationTriggerClipName;
				public float BaseDamageOnTrap = 0f;
				public Collider TrapCollider;

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
										return (chanceOfSuccess + State.SkillOnSet) / 2;
								} else {
										return 0f;
								}
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
										if (Skills.Get.HasLearnedSkill("Fishing")) {
												if (IntersectingDens.Count > 0) {
														examine.Add(new WIExamineInfo("It has been set with " + Skill.MasteryAdjective(State.SkillOnSet) + " skill. The odds of it catching something are " + Skill.MasteryAdjective(NormalizedChanceOfSuccess)));
												} else {
														examine.Add(new WIExamineInfo("It has been set with " + Skill.MasteryAdjective(State.SkillOnSet) + " skill, but there are no fish nearby to catch."));
												}
										} else {
												examine.Add(new WIExamineInfo("It has been set."));
										}
										break;

								case TrapMode.Triggered:
										if (worlditem.StackContainer.IsFull) {
												examine.Add(new WIExamineInfo("The trap is full"));
										} else {
												examine.Add(new WIExamineInfo("The trap has caught " + worlditem.StackContainer.NumItems.ToString() + " item(s)."));
										}
										break;
						}
				}

				public bool TryToSet(float skillLevel)
				{
						if (Mode == TrapMode.Set) {
								Debug.Log("Mode was set, un-setting");
								Mode = TrapMode.Disabled;
								PlayAnimation(AnimationCloseClipName);
								worlditem.RefreshHud();
								return true;
						} else {
								Debug.Log("Wasn't set, setting now");
								Mode = TrapMode.Set;
								MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapSet");
								State.SkillOnSet = skillLevel;
								PlayAnimation(AnimationOpenClipName);
								worlditem.RefreshHud();
								return true;
						}
				}

				protected void Trigger(IItemOfInterest target)
				{
						Debug.Log("Triggering on target");
						MasterAudio.PlaySound(MasterAudio.SoundType.Machines, transform, "HuntingTrapTrigger");
						//AUGH we're damaged
						State.NumTimesTriggered++;
						PlayAnimation(AnimationCloseClipName);
						Mode = TrapMode.Triggered;
				}

				public override void OnInitialized()
				{
						container = worlditem.Get <Container>();
						container.CanOpen = false;
						container.CanUseToOpen = false;

						SkillUpdating = false;
						Refresh();
						TrapCollider.enabled = true;
						TrapCollider.isTrigger = true;
						TrapCollider.gameObject.layer = Globals.LayerNumTrigger;

						worlditem.OnEnterBodyOfWater += OnEnterBodyOfWater;

						worlditem.OnVisible += Refresh;
				}

				public void Refresh()
				{
						if (Mode == TrapMode.Set && !SkillUpdating) {
								Skill skill = null;
								if (Skills.Get.HasLearnedSkill(TrappingSkillName, out skill)) {
										TrappingSkill trappingSkill = skill as TrappingSkill;
										Debug.Log("Asking skill to update trap");
										trappingSkill.UpdateTrap(this);
								} else {
										Debug.Log("Haven't learned this skill");
								}
						}

						if (Mode == TrapMode.Set) {
								PlayAnimation(AnimationOpenClipName);
						} else {
								PlayAnimation(AnimationCloseClipName);
						}
				}

				public void OnEnterBodyOfWater()
				{
						if (Mode != TrapMode.Set) {
								TryToSet(0f);
						}
				}

				protected void PlayAnimation(string clipName)
				{
						if (!animation.IsPlaying(clipName)) {
								//Debug.Log("Playing animation: " + clipName);
								animation.Play(clipName);
						}
				}

				protected void FillTrapWithGoodies(float skillRoll)
				{
						int numGoodies = Mathf.Max(1, Mathf.FloorToInt(skillRoll * Globals.TrappingNumWaterGoodiesPerSkillRoll));
						StackItem goodie = null;
						WICategory category = null;
						WIStackError error = WIStackError.None;
						if (WorldItems.Get.Category(FillCategoryName, out category)) {
								for (int i = 0; i < numGoodies; i++) {
										if (WorldItems.RandomStackItemFromCategory(category, out goodie)) {
												Stacks.Push.Item(worlditem.StackContainer, goodie, ref error);
										}
								}
						}
						if (worlditem.StackContainer.IsFull) {
								Trigger(null);
						} else {
								PlayAnimation(AnimationTriggerClipName);
						}
				}

				protected List <ICreatureDen> mIntersectingDens = new List<ICreatureDen>();
				protected static List <string> gCanCatch = new List <string>() { "Fish" };
				protected static List <string> gExceptions = new List <string>();
		}

		[Serializable]
		public class WaterTrapState
		{
				public double TimeLastChecked = 0f;
				public TrapMode Mode = TrapMode.Disabled;
				public int NumTimesTriggered = 0;
				public int NumTimesMisfired = 0;
				public float SkillOnSet = 0f;
				public double TimeSet = 0f;
				public float NormalizedDistanceFromNearestCreatureDen = 0f;
		}
}

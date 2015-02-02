using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Reflection;
using Frontiers.GUI;
using Frontiers.Data;

namespace Frontiers.World.Gameplay
{
		[ExecuteInEditMode]
		public class Skill : MonoBehaviour, IComparable <Skill>, IProgressDialogObject
		{
				public string FullDescription {
						get {
								return Info.Description;
						}
				}

				public SkillInfo Info = new SkillInfo();
				public SkillState State = new SkillState();
				public SkillUsage Usage = new SkillUsage();
				public SkillEffects Effects = new SkillEffects();
				public SkillRequirements Requirements = new SkillRequirements();

				#region last uses

				//these are cleared after every action
				public IItemOfInterest Caster;
				public Action OnNextAttempt;
				public Action OnNextUse;
				public Action OnNextSuccess;
				public Action OnNextFailure;
				public Action OnNextCriticalFailure;
				public bool LastSkillResult;
				public bool LastUseImmune;
				public float LastSkillValue;
				public int LastSkillFlavor;
				public SkillRollType LastSkillRoll;
				public IItemOfInterest LastSkillTarget;

				public void ClearLast()
				{
						Caster = Player.Local;
						OnNextAttempt = null;
						OnNextUse = null;
						OnNextSuccess = null;
						OnNextFailure = null;
						OnNextCriticalFailure = null; 
				}

				#endregion

				#region IComparable

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						Skill other = obj as Skill;

						if (other == null) {
								return false;
						}

						return (this == other || this.Info.GivenName == other.Info.GivenName);
				}

				public bool Equals(Skill p)
				{
						if (p == null) {
								return false;
						}

						return (this == p || this.Info.GivenName == p.Info.GivenName);
				}

				public int CompareTo(Skill other)
				{
						return DisplayName.CompareTo(other.DisplayName);
				}

				public override int GetHashCode()
				{
						return DisplayName.GetHashCode();
				}

				#endregion

				#region IProgressDialogObject

				public virtual float ProgressValue { get { return NormalizedEffectTimeSoFar; } set { } }

				public virtual string ProgressMessage { get { return Usage.ProgressDialogMessage; } }

				public virtual string ProgressObjectName { get { return DisplayName; } }

				public virtual string ProgressIconName { get { return Info.IconName; } }

				public bool ProgressFinished { get { return mProgressDialogFinished; } }

				public bool ProgressCanceled {
						get { return Usage.CanCancel && mCancelled; }
						set {
								if (Usage.CanCancel) {
										mCancelled = value;
								}
						}
				}

				#endregion

				#region enabling / disabling

				public void DiscoverSkill()
				{
						switch (State.KnowledgeState) {
								case SkillKnowledgeState.Unknown:
										State.KnowledgeState = SkillKnowledgeState.Known;
										Profile.Get.CurrentGame.Character.Exp.AddExperience(Info.ExperienceValueDiscover, Info.ExperienceGainFlagset);
										Player.Get.AvatarActions.ReceiveAction((AvatarAction.SkillDiscover), WorldClock.AdjustedRealTime);
										State.GetPlayerAttention = true;
										Save();
										break;

								default:
										break;
						}
				}

				public void MasterSkill()
				{
						LearnSkill();
						State.NumTimesSuccessful += State.MasteryLevel;
						State.GetPlayerAttention = true;
				}

				public void LearnSkill()
				{
						switch (State.KnowledgeState) {
								case SkillKnowledgeState.Unknown:
								case SkillKnowledgeState.Known:
										DiscoverSkill();
										if (PrerequisiteRequirementsMet) {
												////////Debug.Log ("Learned! Posting gained item");
												Profile.Get.CurrentGame.Character.Exp.AddExperience(Info.ExperienceValueLearn, Info.ExperienceGainFlagset);
												Player.Get.AvatarActions.ReceiveAction((AvatarAction.SkillLearn), WorldClock.AdjustedRealTime);
												GUIManager.PostGainedItem(this);
												State.GetPlayerAttention = true;
												State.KnowledgeState = SkillKnowledgeState.Learned;
												Save();
										} else {
												GUIManager.PostWarning("You can't learn this skill yet - you need to learn " + Skills.Get.SkillDisplayName(Requirements.PrerequisiteSkillName) + " first.");
										}
										break;

								default:
										break;
						}

						if (!string.IsNullOrEmpty(Info.BlueprintOnLearn)) {
								Blueprints.Get.Reveal(Info.BlueprintOnLearn, BlueprintRevealMethod.Skill, DisplayName);
						}
				}

				public void DevEnable()
				{
						State.KnowledgeState = SkillKnowledgeState.Enabled;
						Requirements.RequiredCredentials = 0;
						Requirements.RequiredCredentialsFlagset = string.Empty;
				}

				public void RefreshPrerequisites()
				{
						if (!HasBeenLearned && RequiresPrerequisite && Skills.Get.HasMasteredSkill(Requirements.PrerequisiteSkillName)) {
								LearnSkill();
						}
				}

				public void Refresh()
				{	//if we know our skill
						RefreshPrerequisites();
				}

				#endregion

				#region usage life cycle

				public virtual bool ActionUse(double timeStamp)
				{
						////Debug.Log ("Action use in " + name);
						if (RequirementsMet) {
								Use(0);
						} else {
								if (Requirements.FailedRequirementsCountsAsFailedUse) {
										FailImmediately();
								}
						}
						return true;
				}

				public virtual bool ActionFinish(double timeStamp)
				{
						if (IsInUse) {
								mManualFinish = true;
						}
						return true;
				}

				public virtual bool RollDice(out float skillUseValue, out SkillRollType rollType)
				{	//gets a random value, then checks for success or failure
						//this has no effect on actual skill usage
						//to apply the result, Use (successfully) must be called
						bool result = true;
						rollType = SkillRollType.Success;
						skillUseValue = UnityEngine.Random.value;
						if (Usage.CanFail) {
								//check this value against our mastery level
								//if the value is GREATER than our mastery level, we fail
								//use our failsafe for difficulty levels
								if (skillUseValue > State.NormalizedUsageLevel) {
										result = false;
										if (skillUseValue > Globals.SkillCriticalFailure) {
												rollType = SkillRollType.CriticalFailure;
										}
								}
						} else {
								if (skillUseValue < Globals.SkillCriticalSuccess) {
										rollType = SkillRollType.CriticalSuccess;
								}
						}
						return result;
				}

				public virtual bool Use(bool successfully)
				{
						if (IsInUse) {
								return false;
						}
						if (!RequirementsMet) {
								if (Requirements.FailedRequirementsCountsAsFailedUse) {
										FailImmediately();
								}
								return false;
						}
						LastSkillFlavor = 0;
						UseStart(successfully);
						return true;
				}

				public virtual bool Use(int flavorIndex)
				{
						if (IsInUse) {
								return false;
						}
						if (!RequirementsMet) {
								if (Requirements.FailedRequirementsCountsAsFailedUse) {
										FailImmediately();
								}
								return false;
						}
						LastSkillFlavor = flavorIndex;
						UseStart(true);
						return true;
				}

				public virtual bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						if (IsInUse || !TargetRequirementsMet(targetObject)) {
								return false;
						}

						if (!RequirementsMet) {
								if (Requirements.FailedRequirementsCountsAsFailedUse) {
										FailImmediately();
								}
								return false;
						}

						ProgressCanceled = false;
						LastSkillFlavor = flavorIndex;
						LastSkillTarget = targetObject;
						UseStart(false);
						//this will update skill usage based on type
						//if it's a one-off it'll return immediately
						//then return the result
						//the result is pre-determined
						//so even if we're only sending it after a duration
						//this result will still be valid
						return LastSkillResult;
				}

				protected void FailImmediately()
				{
						//get a dice roll
						RollDice(out LastSkillValue, out LastSkillRoll);
						//but set it to false regardless
						LastSkillResult = false;
						if (Usage.BroadcastType != SkillBroadcastResultTime.OnUseFinish) {
								TryToBroadcastSkillUseResult(Usage.BroadcastType);
						}
						TryToBroadcastSkillUseResult(SkillBroadcastResultTime.OnUseFinish);
						OnUseFinish();
				}

				protected void UseStart(bool forceSuccess)
				{
						State.NumTimesAttempted++;

						LastSkillValue = 0f;
						LastSkillRoll = SkillRollType.Success;
						LastSkillResult = RollDice(out LastSkillValue, out LastSkillRoll) || forceSuccess;
						LastSkillUsed = this;
						LastTimeSkillUsed = WorldClock.AdjustedRealTime;
						if (Usage.RealTimeDuration) {
								mUseStartTime = WorldClock.RealTime;
						} else {
								mUseStartTime = WorldClock.AdjustedRealTime;
						}

						mIsInUse = true;
						Skills.Get.SkillsInUse.SafeAdd(this);

						mManualFinish = false;
						mCancelled = false;
						mProgressDialogFinished = false;
						//broadcast on use start
						TryToBroadcastSkillUseResult(SkillBroadcastResultTime.OnUseStart);
						//broadcast the fact that a skill was used
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.SkillUse, WorldClock.AdjustedRealTime);
						//if we suffer a reputation hit this is where it happens
						Profile.Get.CurrentGame.Character.Rep.ChangeGlobalReputation(RepChange);

						//update usage over time
						OnUseStart();

						if (!mUpdatingUsage) {
								////Debug.Log ("use start in skill " +name +", updating usage now");
								mUpdatingUsage = true;
								StartCoroutine(UpdateUsage());
						}
				}

				protected virtual void OnUseStart()
				{

				}

				public virtual void Cancel()
				{
						mCancelled = true;
				}

				protected void UseFinish()
				{
						if (mUpdatingCooldown) {
								return;
						}

						//broadcast on use finish
						TryToBroadcastSkillUseResult(SkillBroadcastResultTime.OnUseFinish);
						//let the player know we've finished using a skill
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.SkillUseFinish, WorldClock.AdjustedRealTime);
						//call OnUseFinish
						OnUseFinish();
						//reset everything
						mManualFinish = false;
						mCancelled = false;
						//cool down even if our cooldown interval is 0
						mUpdatingCooldown = true;
						StartCoroutine(UpdateCooldown());
				}

				protected virtual void OnUseFinish()
				{
						//yup
				}

				protected IEnumerator UpdateUsage()
				{
						switch (Usage.Type) {
								case SkillUsageType.Once:
								default:
				//we're done, we'll just cool down
										break;

								case SkillUsageType.Duration:
				//if we're using a dialog then get it now
										if (Usage.DisplayProgressDialogByDefault && EffectTime > 0f) {
												GetProgressDialog();
												while (NormalizedEffectTimeLeft > 0f) {
														if (ProgressCanceled) {
																break;
														} else if (!FocusRequirementsMet) {
																//if we're supposed to be looking at somethin but aren't
																//cancel our progress
																ProgressCanceled = true;
																break;
														}
														yield return null;
												}
												mProgressDialogFinished = true;
										} else {
												//otherwise just wait it out until the effect is over
												while (NormalizedEffectTimeLeft > 0f) {
														yield return null;
												}
										}
										break;

								case SkillUsageType.Manual:
				//wait until a manual finish is called
										while (!mManualFinish) {
												yield return null;
										}
										break;
						}

						mUpdatingUsage = false;
						UseFinish();
						yield break;
				}

				protected IEnumerator UpdateCooldown()
				{
						if (Usage.CooldownInterval > 0f) {
								if (Usage.RealTimeDuration) {
										double cooldownEnd = WorldClock.RealTime + Usage.CooldownInterval;
										while (WorldClock.RealTime < cooldownEnd) {
												yield return null;
										}
								} else {
										yield return WorldClock.WaitForSeconds(Usage.CooldownInterval);
								}
						}
						//broadcast on cooldown
						TryToBroadcastSkillUseResult(SkillBroadcastResultTime.OnCooldownEnd);
						//we're no longer in use
						//the skills manager will remove us from the queue
						mIsInUse = false;
						mCancelled = false;
						mManualFinish = false;
						mUpdatingCooldown = false;
						yield break;
				}

				protected void SendMessagesToTarget(IItemOfInterest targetObject, bool result, float skillUseValue, SkillRollType rollType)
				{
						if (targetObject == null) {
								return;
						}

						if (((result || !Usage.CanFail) || Usage.SendMessageOnFail)) {
								if (!string.IsNullOrEmpty(Usage.SendMessageToTargetObject)) {
										if (!string.IsNullOrEmpty(Usage.SendMessageArgument)) {
												switch (Usage.SendMessageArgument) {
												//TODO this sucks, use an enum ffs
														case "[Skill]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, this.gameObject, SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillRollSuccess]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, result, SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillRollType]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, rollType.ToString(), SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillUseValue]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, skillUseValue, SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillMasteryLevel]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, State.NormalizedMasteryLevel, SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillUsageLevel]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, State.NormalizedUsageLevel, SendMessageOptions.DontRequireReceiver);
																break;

														case "[SkillEffectTime]":
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, EffectTime, SendMessageOptions.DontRequireReceiver);
																break;

														default:
																targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, Usage.SendMessageArgument, SendMessageOptions.DontRequireReceiver);
																break;
												}
										} else {
												targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject, SendMessageOptions.DontRequireReceiver);
										}

										if (!string.IsNullOrEmpty(Usage.SendMessageToTargetObjectMaster)) {
												targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObjectMaster, SendMessageOptions.DontRequireReceiver);
										}
								}
						}
				}

				protected void TryToBroadcastSkillUseResult(SkillBroadcastResultTime broadcastType)
				{
						if (broadcastType == Usage.BroadcastType && !ProgressCanceled) {
								if (Usage.GetFinalResultFromMessage) {
										//get the target WIScript from the target object
										//use reflection to get the result from a method
										WorldItem worlditem = LastSkillTarget.worlditem;
										if (worlditem != null) {
												WIScript wiScript = null;
												string targetScriptName = Requirements.RequiredWIScriptNames[0];
												if (worlditem.Is(targetScriptName, out wiScript)) {
														MethodInfo method = wiScript.ScriptType.GetMethod(Usage.SendMessageToTargetObject);
														if (method != null) {
																bool targetResult = false;
																if (string.IsNullOrEmpty(Usage.SendMessageArgument)) {
																		targetResult = (bool)method.Invoke(wiScript, null);
																} else {
																		targetResult = (bool)method.Invoke(wiScript, new System.Object [] { this });
																}
																LastSkillResult = targetResult;
														}
												}
										}
								} else {
										SendMessagesToTarget(LastSkillTarget, LastSkillResult, LastSkillValue, LastSkillRoll);
								}

								if (!LastSkillResult) {
										OnFailure();
								} else {
										OnSuccess();
								}
						}

						if (broadcastType == SkillBroadcastResultTime.OnUseFinish) {
								//we do this regardless of our broadcast type
								if (Usage.AvatarActionOnFinish != AvatarAction.NoAction) {
										Player.Get.AvatarActions.ReceiveAction(Usage.AvatarActionOnFinish, WorldClock.AdjustedRealTime);
								}
						}
				}

				public virtual GUIProgressDialog GetProgressDialog()
				{
						if (mProgressDialog == null || mProgressDialog.IsFinished) {
								mProgressDialogFinished = false;
								GameObject progressDialogGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIProgressDialog);
								GUIManager.SendEditObjectToChildEditor(new ChildEditorCallback <IProgressDialogObject>(OnProgressDialogFinished), progressDialogGameObject, this);
								mProgressDialog = progressDialogGameObject.GetComponent <GUIProgressDialog>();
						}
						return mProgressDialog;
				}

				protected void OnProgressDialogFinished(IProgressDialogObject editObject, IGUIChildEditor<IProgressDialogObject> childEditor)
				{
						GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
						mProgressDialogFinished = true;
						mProgressDialog = null;
				}

				public virtual WIListOption GetListOption(IItemOfInterest targetObject)
				{
						if (!Usage.AppearInContextMenus) {
								return WIListOption.Empty;
						}

						if (mListOption == null) {
								mListOption = new WIListOption(Info.IconName, DisplayName, name);
						}

						string listOptionName = DisplayName;
						if (!string.IsNullOrEmpty(Usage.ListOptionDisplayName)) {
								listOptionName = Usage.ListOptionDisplayName;
						}

						mListOption.OptionText = listOptionName;

						if (!DoesContextAllowForUse(targetObject)) {
								mListOption.IsValid = false;
								return mListOption;
						}

						switch (KnowledgeState) {
								case SkillKnowledgeState.Unknown:
										mListOption.IsValid = false;
										break;

								case SkillKnowledgeState.Enabled:
								case SkillKnowledgeState.Learned:
										mListOption.IsValid = true;
										mListOption.Disabled = false;
										break;

								case SkillKnowledgeState.Known:
								default:
										mListOption.IsValid = true;
										mListOption.Disabled = true;
										break;
						}

						if (!FocusRequirementsMet) {
								mListOption.Disabled = true;
						}

						if (Usage.Type == SkillUsageType.Duration && IsInUse) {
								mListOption.Disabled = true;
						}

						if (!TargetRequirementsMet(targetObject)) {
								mListOption.Disabled = true;
						}

						mListOption.BackgroundColor = SkillBorderColor;
						mListOption.TextColor = Colors.Lighten(mListOption.BackgroundColor, 0.9f, 1.0f);
						if (RequiresCredentials) {
								mListOption.CredentialsIconName = Mats.Get.Icons.GetIconNameFromFlagset(Requirements.RequiredCredentials, Requirements.RequiredCredentialsFlagset);
						} else {
								mListOption.CredentialsIconName = string.Empty;
						}
						mListOption.IconColor = SkillIconColor;

						return mListOption;
				}

				#endregion

				#region result functions

				protected void OnSuccess()
				{
						bool skillMasteredBeforeUse = State.HasBeenMastered;

						State.NumTimesSuccessful++;

						if (!skillMasteredBeforeUse && State.HasBeenMastered) {
								//we mastered it this time! do we have any next in line?
								Skills.OnSkillMastered();
						}

						if (!string.IsNullOrEmpty(Usage.AddedCondition)) {
								Player.Local.Status.AddCondition(Usage.AddedCondition);
						}

						if (!string.IsNullOrEmpty(Usage.RemovedConditon)) {
								Player.Local.Status.RemoveCondition(Usage.RemovedConditon);
						}

						if (!string.IsNullOrEmpty(Usage.ReduceStatus)) {
								Player.Local.Status.ReduceStatus(Usage.ReduceStatusAmount, Usage.ReduceStatus, Usage.ReduceStatusMultiplier);
						}

						if (!string.IsNullOrEmpty(Usage.RestoreStatus)) {
								Player.Local.Status.RestoreStatus(Usage.RestoreStatusAmount, Usage.RestoreStatus, Usage.RestoreStatusMultiplier);
						}

						if (Usage.AvatarActionOnUse != AvatarAction.NoAction) {
								Player.Get.AvatarActions.ReceiveAction((Usage.AvatarActionOnUse), WorldClock.AdjustedRealTime);
						}

						if (RequiresWorldItems) {
								if (Requirements.ConsumeRequiredWorldItems) {
										WorldItem firstFoundItem = null;
										foreach (string requiredWorldItemName in Requirements.RequiredWorldItemNames) {
												if (Player.Local.Inventory.RemoveFirstByKeyword(requiredWorldItemName, out firstFoundItem)) {
														firstFoundItem.SetMode(WIMode.RemovedFromGame);
												}
										}
								}
						}

						Profile.Get.CurrentGame.Character.Exp.AddExperience(Info.ExperienceValueUse + Mathf.CeilToInt(Info.ExperienceValueUse * LastSkillValue), Info.ExperienceGainFlagset);

						//spawn FX on success
						if (LastSkillTarget != null && !string.IsNullOrEmpty(Effects.FXOnSuccess)) {
								GameObject fxTarget = Player.Local.gameObject;
								if (Effects.SpawnFXOnTarget) {
										fxTarget = LastSkillTarget.gameObject;
								}
								FXManager.Get.SpawnFX(fxTarget, Effects.FXOnSuccess);
								if (HasBeenMastered && !string.IsNullOrEmpty(Effects.FXOnSuccessMasteredBooster)) {
										FXManager.Get.SpawnFX(fxTarget, Effects.FXOnSuccessMasteredBooster);
								}
						}
				}

				protected void OnFailure()
				{
						if (!string.IsNullOrEmpty(Usage.ReduceStatus) && Usage.ReduceOnFailure) {
								Player.Local.Status.ReduceStatus(Usage.ReduceStatusAmount, Usage.ReduceStatus, Usage.ReduceStatusMultiplier);
						}

						if (LastSkillTarget != null && !string.IsNullOrEmpty(Effects.FXOnFailure)) {
								GameObject fxTarget = Player.Local.gameObject;
								if (Effects.SpawnFXOnTarget) {
										fxTarget = LastSkillTarget.gameObject;
								}
								FXManager.Get.SpawnFX(fxTarget, Effects.FXOnFailure);
						}
				}

				protected void OnCriticalFailure()
				{
						if (LastSkillTarget != null && !string.IsNullOrEmpty(Effects.FXOnFailureCriticalBooster)) {
								GameObject fxTarget = Player.Local.gameObject;
								if (Effects.SpawnFXOnTarget) {
										fxTarget = LastSkillTarget.gameObject;
								}
								FXManager.Get.SpawnFX(fxTarget, Effects.FXOnFailureCriticalBooster);
						}
				}

				#endregion

				#region Requirements

				public virtual bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (!EquippedRequirementsMet || !WorldItemRequirementsMet) {
								return false;
						}

						return true;
				}

				public virtual bool RequirementsMet {
						get {
								if (GameManager.Get.EnableAllSkills)
										return true;

								if (!HasBeenLearned) {
										return false;
								}

								if (RequiresStatusLevel) {
										StatusKeeper statusKeeper = null;
										if (Player.Local.Status.GetStatusKeeper(Requirements.RequiredStatusType, out statusKeeper)) {
												if (statusKeeper.NormalizedValue < (Status.RestoreToFloat(Requirements.RequiredStatusAmount) * Requirements.RequiredStatusMultiplier)) {
														return false;
												}
										}
								}

								if (RequiresCredentials && !Skills.Get.HasCredentials(Requirements.RequiredCredentials, Requirements.RequiredCredentialsFlagset)) {
										return false;
								}

								if (!FocusRequirementsMet || !EquippedRequirementsMet || !WorldItemRequirementsMet || !ExtensionRequirementsMet) {
										return false;
								}

								return true;
						}
				}

				public bool RequiresStatusLevel {
						get {
								return (!string.IsNullOrEmpty(Requirements.RequiredStatusType));
						}
				}

				public bool RequiresCredentials {
						get {
								return false;//TEMP until I get credentials sorted
								//return Requirements.RequiredCredentials > 0 && !string.IsNullOrEmpty (Requirements.RequiredCredentialsFlagset);
						}
				}

				public bool RequiresWIScripts {
						get {
								return Requirements.RequiredWIScriptNames.Count > 0;
						}
				}

				public bool RequiresWorldItems {
						get {
								return Requirements.RequiredWorldItemNames.Count > 0;
						}
				}

				public virtual bool RequiresAtLeastOneEquippedWorldItem {
						get {
								return Requirements.RequiredEquippedWorldItems.Count > 0 || Requirements.RequiredEquippedWIScriptNames.Count > 0;
						}
				}

				public bool RequiresPlayerFocusItem {
						get {
								return !string.IsNullOrEmpty(Requirements.RequiredPlayerFocusItemName) || Requirements.RequiredPlayerFocusWIScriptNames.Count > 0;
						}
				}

				public bool EquippedRequirementsMet {
						get {
								bool result = true;
								if (RequiresAtLeastOneEquippedWorldItem) {
										if (!Player.Local.Tool.IsEquipped) {
												result = false;
										} else {
												WorldItem equippedWorldItem = Player.Local.Tool.worlditem;
												for (int i = 0; i < Requirements.RequiredEquippedWorldItems.Count; i++) {
														if (!Stacks.Can.Stack(
																equippedWorldItem,
																Requirements.RequiredEquippedWorldItems[i])) {
																result = false;
																break;
														}
												}

												if (!equippedWorldItem.HasAtLeastOne(Requirements.RequiredEquippedWIScriptNames)) {
														result = false;
												}
										}
								}
								return result;
						}
				}

				public bool PrerequisiteRequirementsMet {
						get {
								if (RequiresPrerequisite) {
										return Skills.Get.HasLearnedSkill(Requirements.PrerequisiteSkillName);
								}
								return true;
						}
				}

				protected static ImmuneToSkill mImmunityCheck = null;
				protected static string mImmunityMessage = string.Empty;

				public virtual bool TargetRequirementsMet(IItemOfInterest targetObject)
				{
						if (RequiresWIScripts) {
								WorldItem worlditem = null;
								if (targetObject.gameObject.HasComponent <WorldItem>(out worlditem)) {
										for (int i = 0; i < Requirements.RequiredWIScriptNames.Count; i++) {
												WIScript script = null;
												if (worlditem.Is(Requirements.RequiredWIScriptNames[i], out script)) {
														if (!string.IsNullOrEmpty(Requirements.TargetRequirementCheckProperty)) {
																PropertyInfo propInfo = script.ScriptType.GetProperty(Requirements.TargetRequirementCheckProperty);
																bool requirementCheck = false;
																if (propInfo != null) {
																		requirementCheck = (bool)propInfo.GetValue(script, null);
																		if (!requirementCheck) {
																				return false;
																		}
																}
														}
												} else {
														return false;
												}
										}
								}
						}

						return CheckForSkillImmunity(targetObject, this, out mImmunityMessage);
				}

				public static bool CheckForSkillImmunity(IItemOfInterest targetObject, Skill skill, out string immunityMessage)
				{
						if (targetObject != null && targetObject.IOIType == ItemOfInterestType.WorldItem && targetObject.worlditem.Is <ImmuneToSkill>(out mImmunityCheck)) {
								if (mImmunityCheck.State.IsImmuneTo(skill, out immunityMessage)) {
										skill.LastUseImmune = true;
										return false;
								}
						}
						skill.LastUseImmune = false;
						immunityMessage = null;
						return true;
				}

				public static bool CheckForSkillImmunity(IWIBase targetObject, Skill skill, out string immunityMessage)
				{
						if (targetObject != null && targetObject.Is <ImmuneToSkill>()) {
								System.Object immuneToSkillStateObject = null;
								if (targetObject.GetStateOf <ImmuneToSkill>(out immuneToSkillStateObject)) {
										ImmuneToSkillState immuneToSkillState = (ImmuneToSkillState)immuneToSkillStateObject;
										if (immuneToSkillState.IsImmuneTo(skill, out immunityMessage)) {
												skill.LastUseImmune = true;
												return false;
										}
								}
						}
						skill.LastUseImmune = false;
						immunityMessage = null;
						return true;
				}

				public bool FocusRequirementsMet {
						get {
								if (RequiresPlayerFocusItem) {
										if (Player.Local.Surroundings.IsWorldItemInPlayerFocus) {
												if (!string.IsNullOrEmpty(Requirements.RequiredPlayerFocusItemName) && !Stacks.Can.Stack(Player.Local.Surroundings.WorldItemFocus.worlditem, Requirements.RequiredPlayerFocusItemName)) {
														return false;
												} else {
														for (int i = 0; i < Requirements.RequiredPlayerFocusWIScriptNames.Count; i++) {
																WorldItem worlditem = null;
																if (!Player.Local.Surroundings.WorldItemFocus.worlditem.HasAll(Requirements.RequiredPlayerFocusWIScriptNames)) {
																		return false;
																}
														}
												}
										} else {
												return false;
										}
								}
								return true;
						}
				}

				public bool WorldItemRequirementsMet {
						get {
								if (RequiresWorldItems) {
										for (int i = 0; i < Requirements.RequiredWorldItemNames.Count; i++) {
												if (!Player.Local.Inventory.HasItem(Requirements.RequiredWorldItemNames[i])) {
														return false;
												}
										}
								}
								return true;
						}
				}

				public virtual bool ExtensionRequirementsMet {
						get {
								return true;
						}
				}

				#endregion

				#region convenience props

				public virtual bool IsInUse {
						get {
								//passive skills are always in use
								return mIsInUse;
						}
				}

				public bool IsDormant {
						get {
								//only applies to passive skills
								return Usage.PassiveUsage && NormalizedEffectTimeLeft <= 0f;
						}
				}

				public virtual string DisplayName {
						get {
								if (State.KnowledgeState == SkillKnowledgeState.Unknown) {
										return "(Unknown)";
								}

								if (string.IsNullOrEmpty(Info.GivenName)) {
										return GameData.AddSpacesToSentence(name);
								}
								return Info.GivenName;
						}
				}

				public bool RequiresPrerequisite {
						get {
								return !string.IsNullOrEmpty(Requirements.PrerequisiteSkillName);
						}
				}

				public bool HasBeenMastered {
						get {
								return State.HasBeenMastered;
						}
						set {
								State.KnowledgeState = SkillKnowledgeState.Learned;
								State.NumTimesAttempted = State.MasteryLevel + 1;
								State.NumTimesSuccessful = State.MasteryLevel + 1;
						}
				}

				public bool GetPlayerAttention {
						get {
								return State.GetPlayerAttention;
						}
						set {
								State.GetPlayerAttention = value;
						}
				}

				public Color SkillIconColor {
						get {
								if (HasBeenLearned) {
										return Colors.BlendThree(Colors.Get.SkillLearnedColorLow, Colors.Get.SkillLearnedColorMid, Colors.Get.SkillLearnedColorHigh, State.NormalizedUsageLevel);
								} else if (HasBeenDiscovered) {
										return Colors.Get.SkillKnownColor;
								} else {
										return Color.black;
								}
						}
				}

				public Color SkillBorderColor {
						get {
								if (RequiresCredentials) {
										return Colors.Get.ColorFromFlagset(Requirements.RequiredCredentials, Requirements.RequiredCredentialsFlagset);
								}
								return Colors.Get.ByName(mSkillBorderColorName);
						}
				}

				public Color SkillGlowColor {
						get {
								//TEMP
								return Color.white;
						}
				}

				public float EffectRadius {
						get {
								return SkillEffectRadius(Effects.UnskilledEffectRadius, Effects.SkilledEffectRadius, Effects.MasteredEffectRadius, State.NormalizedUsageLevel, State.HasBeenMastered);
						}
				}

				public static float SkillEffectRadius(float unskilledRadius, float skilledRadius, float masteredRadiusModifier, float normalizedUsage, bool mastered)
				{
						float effectRadius = Mathf.Lerp(unskilledRadius, skilledRadius, normalizedUsage);
						if (mastered) {
								effectRadius *= masteredRadiusModifier;
						}
						return effectRadius;
				}

				public virtual int RepChange {
						get {
								int repChange = Mathf.FloorToInt(Mathf.Lerp(Effects.UnskilledRepChange, Effects.SkilledRepChange, State.NormalizedMasteryLevel));
								if (HasBeenMastered) {
										repChange = Mathf.FloorToInt(repChange * Effects.MasterRepChange);
								}
								return repChange;
						}
				}

				public virtual float EffectTime {
						get {
								float effectTime = Mathf.Lerp(Effects.UnskilledEffectTime, Effects.SkilledEffectTime, State.NormalizedUsageLevel);
								if (HasBeenMastered) {
										effectTime *= Effects.MasteredEffectTime;
								}
								return effectTime;
						}
				}

				public float NormalizedEffectTimeSoFar {
						get {
								return 1.0f - NormalizedEffectTimeLeft;
						}
				}

				public virtual float NormalizedEffectTimeLeft {
						get {
								if (!mIsInUse)
										return 0f;

								switch (Usage.Type) {
										case SkillUsageType.Duration:
												double effectTime = EffectTime;
												if (Usage.RealTimeDuration) {
														double timeLeft = 1.0 - ((WorldClock.RealTime - mUseStartTime) / effectTime);
														return (float)timeLeft;
												} else {
														double timeLeft = 1.0 - ((WorldClock.AdjustedRealTime - mUseStartTime) / effectTime); 
														return (float)timeLeft;
												}

										case SkillUsageType.Manual:
										case SkillUsageType.Once:
										default:
												return 1.0f;
								}
						}
				}

				public bool CanLearn {
						get {
								if (RequiresPrerequisite) {
										return Skills.Get.HasLearnedSkill(Requirements.PrerequisiteSkillName);
								}
								return true;
						}
				}

				public virtual SkillKnowledgeState KnowledgeState {
						get { return State.KnowledgeState; }
				}

				public bool HasBeenDiscovered {
						get {
								return State.KnowledgeState != SkillKnowledgeState.Unknown;
						}
				}

				public bool HasBeenLearned {
						get {
								return State.KnowledgeState == SkillKnowledgeState.Learned
								|| State.KnowledgeState == SkillKnowledgeState.Enabled;
						}
				}

				#endregion

				#region save / load / init

				public SkillSaveState SaveState {
						get {
								SkillSaveState newSaveState = new SkillSaveState();
								newSaveState.Name = name;
								newSaveState.ClassName = this.GetType().Name;
								newSaveState.Info = Info;
								newSaveState.State = State;
								newSaveState.Usage = Usage;
								newSaveState.Effects = Effects;
								newSaveState.Requirements = Requirements;
								newSaveState.ExtensionsState = ExtensionsState;
								return newSaveState;
						}
						set {
								Info = value.Info;
								State = value.State;
								Usage = value.Usage;
								Effects = value.Effects;
								Requirements = value.Requirements;
								ExtensionsState = value.ExtensionsState;
								name = value.Name;
						}
				}

				public string ExtensionsState {
						get {
								string saveData = string.Empty;
								if (mHasSkillExtensionsField) {
										try {
												saveData = WIScript.XmlSerializeToString(mSkillExtensionsField.GetValue(this));
										} catch (Exception e) {
												return string.Empty;
										}
								}
								return saveData;
						}
						set {
								if (mHasSkillExtensionsField && !string.IsNullOrEmpty(value)) {
										object triggerStateValue = null;
										try {
												if (WIScript.XmlDeserializeFromString(value, mSkillExtensionsField.FieldType, out triggerStateValue)) {
														mSkillExtensionsField.SetValue(this, triggerStateValue);
												}
										} catch (Exception e) {
												Debug.LogError("Skill init from save data error! E: " + e.ToString());
										}
								}
						}
				}

				public void Save()
				{
						SkillSaveState newSaveState = SaveState;
						Mods.Get.Runtime.SaveMod <SkillSaveState>(newSaveState, "Skill", newSaveState.Name);
				}

				public virtual void Initialize()
				{
						if (!Application.isPlaying)
								return;

						Caster = Player.Local;

						CheckExtensionProps();

						if (GameManager.Get.EnableAllSkills) {
								State.KnowledgeState = SkillKnowledgeState.Learned;
						}

						if (Usage.UserActionUse != UserActionType.NoAction) {
								Player.Get.UserActions.Subscribe(Usage.UserActionUse, new ActionListener(ActionUse));
						}

						if (Usage.AvatarActionUse != AvatarAction.NoAction) {
								Player.Get.AvatarActions.Subscribe(Usage.AvatarActionUse, new ActionListener(ActionUse));
						}

						if (Usage.UserActionFinish != UserActionType.NoAction) {
								Player.Get.UserActions.Subscribe(Usage.UserActionFinish, new ActionListener(ActionFinish));
						}

						if (Usage.AvatarActionFinish != AvatarAction.NoAction) {
								Player.Get.AvatarActions.Subscribe(Usage.AvatarActionFinish, new ActionListener(ActionFinish));
						}

						mSkillBorderColorName = "Skill" + Info.SkillGroup;
				}

				public void CheckExtensionProps()
				{
						Type skillType = GetType();
						mSkillExtensionsField	= skillType.GetField("Extensions");

						if (mSkillExtensionsField != null) {
								mHasSkillExtensionsField = true;
						} else {
								mHasSkillExtensionsField = false;
						}
				}

				#endregion

				protected WIListOption mListOption;
				protected GUIProgressDialog mProgressDialog;
				protected bool mProgressDialogFinished = false;
				protected bool mIsInUse = false;
				protected bool mCancelled = false;
				protected bool mManualFinish = false;
				protected bool mUpdatingUsage = false;
				protected bool mUpdatingCooldown = false;
				protected double mUseStartTime = 0f;
				protected string mSkillBorderColorName = string.Empty;
				protected bool mHasSkillExtensionsField = false;
				protected FieldInfo mSkillExtensionsField;

				#region static helper functions & fields

				public static string MasteryAdjective(float masteryLevel)
				{
						if (masteryLevel >= 1.0f) {
								return "masterful";
						}
						if (masteryLevel >= 0.8f) {
								return "high";
						}
						if (masteryLevel >= 0.6f) {
								return "moderate";
						}
						if (masteryLevel >= 0.4f) {
								return "fair";
						}
						if (masteryLevel >= 0.2f) {
								return "poor";
						}
						return "abysmal";
				}

				public static Skill LastSkillUsed;
				public static double LastTimeSkillUsed = 0.0f;

				#endregion

		}

		[Serializable]
		public class SkillEffects
		{
				public float UnskilledEffectRadius = 5f;
				public float SkilledEffectRadius = 10f;
				public float MasteredEffectRadius = 1.0f;
				public float UnskilledEffectTime = 1f;
				public float SkilledEffectTime = 10f;
				public float MasteredEffectTime = 1.0f;
				//how improvement affects effects (lol)
				public int UnskilledRepChange = 0;
				public int SkilledRepChange = 0;
				public float MasterRepChange = 1.0f;
				public bool SpawnFXOnTarget = false;
				[FrontiersFXAttribute]
				public string FXOnSuccess;
				[FrontiersFXAttribute]
				public string FXOnSuccessMasteredBooster;
				[FrontiersFXAttribute]
				public string FXOnFailure;
				[FrontiersFXAttribute]
				public string FXOnFailureCriticalBooster;
		}

		[Serializable]
		public class SkillInfo
		{
				public int SkillLevel;
//arbitrary skill level assigned for book distribution
				public string GivenName = string.Empty;
				[Multiline]
				public string Description = string.Empty;
				[Multiline]
				public string Instructions = string.Empty;
				public string IconName;
				public string SkillGroup = "Guild";
				public string SkillSubgroup = "Applicant";
				public int SkillDisplayOrder = 0;
				public float TimeToLearn = 10.0f;
				public string ExperienceGainFlagset = "GuildCredentials";
				public int ExperienceValueDiscover = 100;
				public int ExperienceValueLearn = 100;
				public int ExperienceValueUse = 100;
				public int ExperienceValueMaster = 100;
				public string BlueprintOnLearn = string.Empty;
		}

		[Serializable]
		public class SkillUsage
		{
				public SkillUsageType Type = SkillUsageType.Once;
				public bool PassiveUsage = false;
//skill is 'always on'
				public bool RealTimeDuration = false;
//pauses when the game is paused
				public SkillBroadcastResultTime BroadcastType = SkillBroadcastResultTime.OnUseStart;
				public bool AppearInContextMenus = true;
				public bool CanFail = false;
				public bool GetFinalResultFromMessage = false;
				public bool SendMessageOnFail = true;
				public float CooldownInterval = 0f;
				public bool VisibleInInterface = true;
				//these can stop and start usage
				public UserActionType UserActionUse = UserActionType.NoAction;
				public AvatarAction AvatarActionUse = AvatarAction.NoAction;
				public UserActionType UserActionFinish = UserActionType.NoAction;
				public AvatarAction AvatarActionFinish = AvatarAction.NoAction;
				//these are sent on use start and on use finish
				public AvatarAction AvatarActionOnUse = AvatarAction.NoAction;
				public AvatarAction AvatarActionOnFinish = AvatarAction.NoAction;
				[FrontiersAvailableModsAttribute("StatusKeeper")]
				public string ReduceStatus = "Health";
				public PlayerStatusRestore ReduceStatusAmount = PlayerStatusRestore.A_None;
				public float ReduceStatusMultiplier = 1.0f;
				public bool ReduceOnFailure = false;
				[FrontiersAvailableModsAttribute("StatusKeeper")]
				public string RestoreStatus = "Health";
				public PlayerStatusRestore RestoreStatusAmount = PlayerStatusRestore.A_None;
				public float RestoreStatusMultiplier = 1.0f;
				public string AddedCondition = string.Empty;
				public string RemovedConditon = string.Empty;
				public string SendMessageToTargetObject = string.Empty;
				public string SendMessageArgument = string.Empty;
				public string SendMessageToTargetObjectMaster = string.Empty;
				public string SendMessageArgumentMaster = string.Empty;
				public List <string> TargetWIScriptNames = new List <string>();
				public ItemOfInterestType TargetItemsOfInterest = ItemOfInterestType.All;
				public string ListOptionDisplayName = string.Empty;
				public bool CanCancel = true;
				public bool DisplayProgressDialogByDefault = false;
				public string ProgressDialogMessage = string.Empty;
				public string ConfirmationMessageOnFirstUse = string.Empty;
				public bool DisableConfirmationMessage = false;
		}

		[Serializable]
		public class SkillExtensions
		{
				//this empty class is used as a base for other classes to extend their properties
		}

		[Serializable]
		public class SkillRequirements
		{
				//required credentials
				public bool FailedRequirementsCountsAsFailedUse = false;
				public string RequiredCredentialsFlagset;
				[FrontiersBitMask("GuildCredentials")]//TODO fix this (somehow?)
		public int RequiredCredentials = 0;
				//target names and scripts
				public List <string> RequiredWorldItemNames = new List <string>();
				public List <string> RequiredWIScriptNames = new List <string>();
				public string TargetRequirementCheckProperty = string.Empty;
				//equipped item names and scripts
				public List <string> RequiredEquippedWorldItems = new List <string>();
				public List <string> RequiredEquippedWIScriptNames = new List<string>();
				//focus names and scripts
				public string RequiredPlayerFocusItemName = string.Empty;
				public List <string> RequiredPlayerFocusWIScriptNames = new List <string>();
				//player state requirements
				[FrontiersAvailableModsAttribute("StatusKeeper")]
				public string RequiredStatusType = string.Empty;
				public PlayerStatusRestore RequiredStatusAmount = PlayerStatusRestore.A_None;
				public float RequiredStatusMultiplier = 1.0f;
				public string PrerequisiteSkillName = string.Empty;
				public bool ConsumeRequiredWorldItems = false;
		}

		[Serializable]
		public class SkillState
		{
				public float CriticalFailure {
						get {
								return 0.0f;
						}
				}

				public float NormalizedMasteryLevel {
						get {
								return Mathf.Max(Mathf.Max(NormalizedLearningBonus, ((float)NumTimesSuccessful / (float)MasteryLevel)), Globals.SkillFailsafeMasteryLevel);
						}
				}
				//this returns a value from -1 to 1
				//useful for penalties / bonuses
				public float NormalizedOffsetUsageLevel {
						get {
								return (NormalizedUsageLevel * 2f) - 1f;
						}
				}

				public float NormalizedUsageLevel {
						get {
								return Mathf.Max(NormalizedMasteryLevel, BaseUsageValue);
						}
				}

				public float NormalizedLearningBonus {
						get {
								return 0.0f;//if you've learned it from a book or person you get a bonus
						}
				}

				public bool HasBeenMastered {
						get {
								return NormalizedMasteryLevel >= 1.0f;
						}
				}

				public SkillKnowledgeState KnowledgeState;
				public int MasteryLevel = 100;
				public int NumTimesAttempted;
				public int NumTimesSuccessful;
				public int NumTimesUpgraded;
				//TODO incorporate UpgradeUsageValue into usage value
				public float TimeFirstLearned;
				public float LastTimeSkillUsed;
				public float FirstTimeSkillUsed;
				public float BaseUsageValue = 0.25f;
				public float UpgradeUsageValue = 0.05f;
				public bool GetPlayerAttention;
		}

		[Serializable]
		public class SkillSaveState : Mod
		{
				public string ClassName = "Skill";
				public SkillInfo Info = new SkillInfo();
				public SkillState State = new SkillState();
				public SkillUsage Usage = new SkillUsage();
				public SkillEffects Effects = new SkillEffects();
				public SkillRequirements Requirements = new SkillRequirements();
				public string ExtensionsState = null;
		}
}
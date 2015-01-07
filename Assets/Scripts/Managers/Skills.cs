using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.World;

using System;

namespace Frontiers
{
		public class Skills : Manager
		{		//the skills manager makes it easier to look up skills by name etc.
				//it also does the work of associating worlditems with skills
				//and provides some convenience functions like LearnSkill
				public static Skills Get;
				public List <Skill> SkillList = new List <Skill>();
				public List <Skill> SkillsInUse = new List <Skill>();
				public List <CredentialLevel> CredentialLevels = new List <CredentialLevel>();
				public GameObject EffectSpherePrefab;
				public bool DebugSkills = true;

				public int Credentials(string flagSet)
				{
						int credentials = 0;
						Dictionary <int,int> lookup = new Dictionary <int, int>();
						if (mCredentialLevelsLookup.TryGetValue(flagSet, out lookup)) {
								int experience = Profile.Get.CurrentGame.Character.Exp.ExpByFlagset(flagSet);
								int greatestExperience = 0;
								foreach (KeyValuePair <int,int> credExpPair in lookup) {
										if (experience >= credExpPair.Value && credExpPair.Value > greatestExperience) {
												credentials = credExpPair.Key;
												greatestExperience = credExpPair.Value;
										}
								}
						}
						return credentials;
				}

				public bool HasCredentials(int credentials, string flagSet)
				{
						int experience = Profile.Get.CurrentGame.Character.Exp.ExpByFlagset(flagSet);
						int requiredExperience = Int32.MaxValue;
						Dictionary <int,int> lookup = null;
						if (mCredentialLevelsLookup.TryGetValue(flagSet, out lookup)) {
								lookup.TryGetValue(credentials, out requiredExperience);
						}
						return experience >= requiredExperience;
				}

				public float NormalizedExpToNextCredentials(string flagSet)
				{
						float exp = 0f;
						if (!mNormalizedExpToNextCredentials.TryGetValue(flagSet, out exp)) {
								exp = 1f;
						}
						return exp;
				}

				public bool HasMasteredSkill(string skillName)
				{
						Skill learnedSkill = null;
						if (SkillByName(skillName, out learnedSkill)) {
								return learnedSkill.HasBeenMastered;
						}
						return false;
				}

				public bool HasLearnedSkill(string skillName, out Skill learnedSkill)
				{
						//TEMP
						learnedSkill = null;
						if (SkillByName(skillName, out learnedSkill)) {
								SkillKnowledgeState state = learnedSkill.KnowledgeState;
								return (state == SkillKnowledgeState.Enabled || state == SkillKnowledgeState.Learned) || GameManager.Get.EnableAllSkills;
						}
						return false | GameManager.Get.EnableAllSkills;
				}

				public bool HasLearnedSkill(string skillName, out float skillUsageLevel)
				{
						//TEMP
						skillUsageLevel = 0f;
						Skill learnedSkill = null;
						if (SkillByName(skillName, out learnedSkill)) {
								SkillKnowledgeState state = learnedSkill.KnowledgeState;
								skillUsageLevel = learnedSkill.State.NormalizedUsageLevel;
								return (state == SkillKnowledgeState.Enabled || state == SkillKnowledgeState.Learned) || GameManager.Get.EnableAllSkills;
						}
						return false | GameManager.Get.EnableAllSkills;
				}

				public bool HasLearnedSkill(string skillName)
				{
						//TEMP
						Skill learnedSkill = null;
						if (SkillByName(skillName, out learnedSkill)) {
								SkillKnowledgeState state = learnedSkill.KnowledgeState;
								return (state == SkillKnowledgeState.Enabled || state == SkillKnowledgeState.Learned) || GameManager.Get.EnableAllSkills;
						}
						return false | GameManager.Get.EnableAllSkills;
				}

				public override void WakeUp()
				{
						Get = this;
				}

				public override void OnModsLoadFinish()
				{
						LoadSkills();
						RefreshSkills();
						mModsLoaded = true;
				}

				public override void OnGameUnload()
				{
						for (int i = 0; i < SkillList.Count; i++) {
								GameObject.Destroy(SkillList[i].gameObject);
						}
						SkillList.Clear();
						SkillsInUse.Clear();
						mSkillsLookup.Clear();
						mSkillsLookupByWIScript.Clear();
						mSkillsLookupByWorldItem.Clear();
				}

				public override void OnGameStart()
				{
						Player.Get.AvatarActions.Subscribe(AvatarAction.SkillExperienceGain, new ActionListener(SkillExperienceGain));
						StartCoroutine(UpdateSkillsInUse());
				}

				public RemoveItemSkill RemoveItemSkillFromName(string skillName)
				{
						return null;
				}

				public bool SkillExperienceGain(double timeStamp)
				{
						if (!mCheckingCredentials) {
								StartCoroutine(CheckCredentials());
						}
						return true;
				}

				public List <Skill> SkillsAssociatedWith(WorldItem targetObject)
				{
						HashSet <Skill> skills = new HashSet <Skill>();
						Skill examineSkill = null;
						Skill reverseEngineerSkill = null;
						if (mSkillsLookup.TryGetValue("Examine", out examineSkill)) {
								skills.Add(examineSkill);
						}
						if (mSkillsLookup.TryGetValue("ReverseEngineer", out reverseEngineerSkill)) {
								skills.Add(reverseEngineerSkill);
						}
			
						List <Skill> associatedNameSkills;
						if (mSkillsLookupByWorldItem.TryGetValue(targetObject.StackName, out associatedNameSkills)) {
								foreach (Skill associatedNameSkill in associatedNameSkills) {
										skills.Add(associatedNameSkill);
								}
						}
						WorldItem worlditem = targetObject.GetComponent <WorldItem>();
						if (worlditem != null) {
								foreach (string wiScriptName in worlditem.ScriptNames) {
										List <Skill> associatedScriptSkills = null;
										if (mSkillsLookupByWIScript.TryGetValue(wiScriptName, out associatedScriptSkills)) {
												foreach (Skill associatedScriptSkill in associatedScriptSkills) {
														skills.Add(associatedScriptSkill);
												}
										}
								}
						}

						List <Skill> finalSkillList = new List<Skill>(skills);
						finalSkillList.Sort();
						return finalSkillList;
				}

				public List <Skill> SkillsByGroup(string skillGroup)
				{
						List <Skill> skills = new List<Skill>();
						for (int i = 0; i < SkillList.Count; i++) {
								Skill skill = SkillList[i];
								if (skill.Info.SkillGroup == skillGroup && skill.HasBeenDiscovered) {
										skills.Add(skill);
								}
						}
						return skills;
				}

				public string SkillDisplayName(string skillName)
				{
						Skill skill = null;
						if (mSkillsLookup.TryGetValue(skillName, out skill)) {
								return skill.DisplayName;
						}
						return string.Empty;
				}

				public List <Skill> SkillsByName(IEnumerable <string> names)
				{
						List <Skill> skills = new List <Skill>();
						foreach (string skillName in names) {
								Skill skillByName = null;
								if (mSkillsLookup.TryGetValue(skillName, out skillByName)) {
										skills.Add(skillByName);
								}
						}
						return skills;
				}

				public List <T> SkillsByType <T>() where T : Skill
				{
						List <T> skillsByType = new List <T>();
						foreach (Skill skill in SkillList) {
								T skillAsType = skill as T;
								if (skillAsType != null) {
										skillsByType.Add(skillAsType);
								}
						}
						return skillsByType;
				}

				public int SkillLevelByName(string skillName)
				{
						Skill skill = null;
						if (mSkillsLookup.TryGetValue(skillName, out skill)) {
								return skill.Info.SkillLevel;
						}
						return 0;
				}

				public static void OnSkillMastered()
				{		//here's where we see if it's been learned
						for (int i = 0; i < Get.SkillList.Count; i++) {
								Get.SkillList[i].Refresh();
						}
						//here's where we see if it's been replaced
						for (int i = 0; i < Get.SkillList.Count; i++) {
								Get.SkillList[i].RefreshPrerequisites();
						}
				}

				public void MasterSkill(string skillName)
				{
						Skill skillToMaster = null;
						if (Get.mSkillsLookup.TryGetValue(skillName, out skillToMaster)) {
								skillToMaster.MasterSkill();
						}
				}

				public static void MarkSkill(string skillName)
				{
						Skill skillToLearn = null;
						if (Get.mSkillsLookup.TryGetValue(skillName, out skillToLearn)) {
								skillToLearn.State.GetPlayerAttention = true;
						}
				}

				public static void LearnSkill(string skillName)
				{
						Skill skillToLearn = null;
						if (Get.mSkillsLookup.TryGetValue(skillName, out skillToLearn)) {
								skillToLearn.LearnSkill();
						}
				}

				public static void RevealSkill(string skillName)
				{
						Skill skillToLearn = null;
						if (Get.mSkillsLookup.TryGetValue(skillName, out skillToLearn)) {
								skillToLearn.DiscoverSkill();
						}
				}

				public bool SkillByName(string skillName, out Skill skill)
				{
						return mSkillsLookup.TryGetValue(skillName, out skill);
				}

				protected void RefreshSkills()
				{
						for (int i = 0; i < SkillList.Count; i++) {
								Skill newSkill = SkillList[i];
								newSkill.Initialize();

								mSkillsLookup.Add(newSkill.name, newSkill);

								List <string> associatedWorldItems = new List <string>();
								List <string> associatedWIScripts = new List <string>();
								associatedWorldItems.AddRange(newSkill.Requirements.RequiredEquippedWorldItems);
								associatedWorldItems.AddRange(newSkill.Requirements.RequiredWorldItemNames);
								associatedWorldItems.Add(newSkill.Requirements.RequiredPlayerFocusItemName);

								associatedWIScripts.AddRange(newSkill.Usage.TargetWIScriptNames);
								associatedWIScripts.AddRange(newSkill.Requirements.RequiredWIScriptNames);
								associatedWIScripts.AddRange(newSkill.Requirements.RequiredEquippedWIScriptNames);
								associatedWIScripts.AddRange(newSkill.Requirements.RequiredPlayerFocusWIScriptNames);

								if (newSkill.RequiresPlayerFocusItem) {
										associatedWorldItems.Add(newSkill.Requirements.RequiredPlayerFocusItemName);
								}

								foreach (string associatedWorldItemName in associatedWorldItems) {
										if (!string.IsNullOrEmpty(associatedWorldItemName)) {
												List <Skill> associatedSkillsList = null;
												if (!mSkillsLookupByWorldItem.TryGetValue(associatedWorldItemName, out associatedSkillsList)) {
														associatedSkillsList = new List <Skill>();
														mSkillsLookupByWorldItem.Add(associatedWorldItemName, associatedSkillsList);
												}
												associatedSkillsList.Add(newSkill);
										}
								}

								foreach (string associatedWIScriptName in associatedWIScripts) {
										if (!string.IsNullOrEmpty(associatedWIScriptName)) {
												List <Skill> associatedWIScriptsList = null;
												if (!mSkillsLookupByWIScript.TryGetValue(associatedWIScriptName, out associatedWIScriptsList)) {
														associatedWIScriptsList = new List <Skill>();
														mSkillsLookupByWIScript.Add(associatedWIScriptName, associatedWIScriptsList);
												}
												associatedWIScriptsList.Add(newSkill);
										}
								}
						}

						for (int i = 0; i < SkillList.Count; i++) {
								SkillList[i].RefreshPrerequisites();
						}
				}

				protected void LoadSkills()
				{
						//destroy / clear all existing skills
						foreach (Skill skill in SkillList) {
								if (skill != null) {
										GameObject.DestroyImmediate(skill.gameObject);
								}
						}
						SkillList.Clear();
						mSkillsLookup.Clear();
						mSkillsLookupByWorldItem.Clear();
						mSkillsLookupByWIScript.Clear();

						//load all skills from mods
						List <string> skillNames = Mods.Get.ModDataNames("Skill");
						foreach (string skillName in skillNames) {
								Transform parentObject = gameObject.FindOrCreateChild("Skills");
								SkillSaveState skillSaveState = null;
								if (Mods.Get.Runtime.LoadMod <SkillSaveState>(ref skillSaveState, "Skill", skillName)) {
										GameObject newSkillObject = new GameObject(skillName);
										Skill newSkill = null;
										if (!string.IsNullOrEmpty(skillSaveState.ClassName)) {
												//if we use a custom script then add it now
												Component skillComponent = newSkillObject.AddComponent(skillSaveState.ClassName);
												newSkill = skillComponent as Skill;
										} else {
												//otherwise just use the normal skill
												newSkill = newSkillObject.AddComponent <Skill>();
										}
										if (newSkill != null) {
												//load the state
												newSkill.CheckExtensionProps();
												newSkill.SaveState = skillSaveState;
												//add it to the list
												SkillList.Add(newSkill);
												//create the subgroup lookup and parent it underneath
												if (!string.IsNullOrEmpty(newSkill.Info.SkillGroup)) {
														parentObject = parentObject.gameObject.FindOrCreateChild(newSkill.Info.SkillGroup);
														if (!string.IsNullOrEmpty(newSkill.Info.SkillSubgroup)) {
																parentObject = parentObject.gameObject.FindOrCreateChild(newSkill.Info.SkillSubgroup);
														}
												}
												newSkill.transform.parent = parentObject.transform;
										}
								}
						}
				}

				public bool IsUsingAtLeastOne(List <string> skillNames)
				{
						for (int i = 0; i < SkillsInUse.Count; i++) {
								if (SkillsInUse[i] != null) {
										if (skillNames.Contains(SkillsInUse[i].name))
												return true;
								}
						}
						return false;
				}

				public bool IsSkillInUse(string skillName)
				{
						for (int i = 0; i < SkillsInUse.Count; i++) {
								if (SkillsInUse[i] != null) {
										if (SkillsInUse[i].name == skillName) {
												return true;
										}
								}
						}
						return false;
				}

				public bool LearnedSkill(string skillName, out Skill skill)
				{
						skill = null;
						if (mSkillsLookup.TryGetValue(skillName, out skill)) {
								if (skill.State.KnowledgeState == SkillKnowledgeState.Learned
								    || skill.State.KnowledgeState == SkillKnowledgeState.Enabled) {
										return true;
								}
						}
						return false;
				}

				protected void SaveSkills()
				{
						foreach (Skill skill in SkillList) {
								if (skill != null) {
										SkillSaveState skillSaveState = skill.SaveState;
										Mods.Get.Runtime.SaveMod <SkillSaveState>(skillSaveState, "Skill", skillSaveState.Name);
								}
						}
				}

				protected void SaveSkill(string skillName)
				{
						Skill skill = null;
						if (mSkillsLookup.TryGetValue(skillName, out skill)) {
								SkillSaveState skillSaveState = skill.SaveState;
								Mods.Get.Runtime.SaveMod <SkillSaveState>(skillSaveState, "Skill", skillSaveState.Name);
						}
				}
				#if UNITY_EDITOR
				public void LoadEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						//load all skills from mods
						List <string> skillNames = Mods.Get.ModDataNames("Skill");
						foreach (string skillName in skillNames) {
								Transform parentObject = gameObject.FindOrCreateChild("Skills");
								SkillSaveState skillSaveState = null;
								if (Mods.Get.Editor.LoadMod <SkillSaveState>(ref skillSaveState, "Skill", skillName)) {
										GameObject newSkillObject = new GameObject(skillName);
										Skill newSkill = null;
										if (!string.IsNullOrEmpty(skillSaveState.ClassName)) {
												//if we use a custom script then add it now
												Component skillComponent = newSkillObject.AddComponent(skillSaveState.ClassName);
												newSkill = skillComponent as Skill;
										} else {
												//otherwise just use the normal skill
												newSkill = newSkillObject.AddComponent <Skill>();
										}
										if (newSkill != null) {
												//load the state'
												newSkill.CheckExtensionProps();
												newSkill.SaveState = skillSaveState;
												//add it to the list
												SkillList.Add(newSkill);
												//create the subgroup lookup and parent it underneath
												if (!string.IsNullOrEmpty(newSkill.Info.SkillGroup)) {
														parentObject = parentObject.gameObject.FindOrCreateChild(newSkill.Info.SkillGroup);
														if (!string.IsNullOrEmpty(newSkill.Info.SkillSubgroup)) {
																parentObject = parentObject.gameObject.FindOrCreateChild(newSkill.Info.SkillSubgroup);
														}
												}
												newSkill.transform.parent = parentObject.transform;
										}
								}
						}
				}

				public void SaveEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						foreach (Skill skill in SkillList) {
								if (skill != null) {
										skill.CheckExtensionProps();
										SkillSaveState skillSaveState = skill.SaveState;
										Mods.Get.Editor.SaveMod <SkillSaveState>(skillSaveState, "Skill", skillSaveState.Name);
								}
						}
				}
				#endif
				public IEnumerator UpdateSkillsInUse()
				{
						while (GameManager.State != FGameState.Quitting) {
								yield return new WaitForSeconds(0.2f);
								for (int i = SkillsInUse.Count - 1; i >= 0; i--) {
										if (SkillsInUse[i] == null || (!SkillsInUse[i].IsInUse && !SkillsInUse[i].Usage.PassiveUsage)) {
												SkillsInUse.RemoveAt(i);
										}
								}
								yield return null;
								//make sure all passive skills are in use
								for (int i = 0; i < SkillList.Count; i++) {
										if (SkillList[i].Usage.PassiveUsage && SkillList[i].HasBeenLearned) {
												SkillsInUse.SafeAdd(SkillList[i]);
										}
								}
						}
				}

				public IEnumerator CheckCredentials()
				{
						mCheckingCredentials = true;
						yield return null;//wait for all experience to come in

						foreach (KeyValuePair <string, int> exp in Profile.Get.CurrentGame.Character.Exp.ExperienceByFlagset) {
								//look at all the different kinds of experience the player has accumulated
								//get the last known credentials for the player
								int currentCredentials = Profile.Get.CurrentGame.Character.Exp.LastCredByFlagset(exp.Key);
								//check what the credentials SHOULD be
								int newCredentials = currentCredentials;
								int currentCredentialsExperience = 0;
								Dictionary <int,int> credExpLookup = null;
								if (mCredentialLevelsLookup.TryGetValue(exp.Key, out credExpLookup)) {
										credExpLookup.TryGetValue(currentCredentials, out currentCredentialsExperience);
										//look at each experience level
										//record the greatest level of experience
										int greatestCredSoFar = 0;
										foreach (KeyValuePair <int, int> credExpPair in credExpLookup) {
												//if this is our current credentials, get the needed experience while we're here
												if (credExpPair.Key != currentCredentials) {
														//if we have enough for this credential
														//and this credential requires more than the last credential
														//our new credentials is this
														if (exp.Value >= credExpPair.Value
														    && credExpPair.Value > currentCredentialsExperience
														    && credExpPair.Value > greatestCredSoFar) {
																newCredentials = credExpPair.Key;
																greatestCredSoFar = newCredentials;
														}
												}
										}
								}
								//after all that, see if our cred exp pair is the same
								//if it is, then we've gained credentials
								if (newCredentials != currentCredentials) {
										//this will automatically tell objects to update their icons, etc.
										Profile.Get.CurrentGame.Character.Exp.SetLastCredentials(exp.Key, newCredentials);
								}
								//calculate how far we have to go until the next credentials
								//get the next experience level for this flagset
								int nextExperienceLevel = 0;
								float normalizedExpToNextCredentials = 1.0f;
								if (GetNextExperienceLevel(newCredentials, exp.Key, out nextExperienceLevel)) {
										//if there is a next experience level, get the normalized value to the next credential
										int currentExpInThisCredential = exp.Value - currentCredentialsExperience;
										int expNeededForNextCredential = nextExperienceLevel - currentCredentialsExperience;
										normalizedExpToNextCredentials = ((float)currentExpInThisCredential / (float)expNeededForNextCredential);
								}
								//in either case, set the normalized value in the lookup
								if (!mNormalizedExpToNextCredentials.ContainsKey(exp.Key)) {
										mNormalizedExpToNextCredentials.Add(exp.Key, normalizedExpToNextCredentials);
								} else {
										mNormalizedExpToNextCredentials[exp.Key] = normalizedExpToNextCredentials;
								}

						}

						foreach (Skill skill in SkillList) {
								//now see if we've learned any skills
								if (skill.RequiresCredentials && HasCredentials(skill.Requirements.RequiredCredentials, skill.Requirements.RequiredCredentialsFlagset)) {
										//if the skill requires credentials
										//and the player has gained those credentials
										//the skill has been learned
										skill.LearnSkill();
								}
								//yield return null;
						}
						mCheckingCredentials = false;
						yield break;
				}

				public bool GetNextExperienceLevel(int credentials, string flagset, out int nextExperienceLevel)
				{
						int currentExperiencelevel = 0;
						bool foundNext = false;
						nextExperienceLevel = 0;
						Dictionary <int,int> credExpLookup = null;
						if (mCredentialLevelsLookup.TryGetValue(flagset, out credExpLookup)) {
								//get the current experience needed for this
								credExpLookup.TryGetValue(credentials, out currentExperiencelevel);
								int smallestDifferenceSoFar = Int32.MaxValue;
								foreach (KeyValuePair <int,int> credExpPair in credExpLookup) {
										//if there's a level above ours
										if (credExpPair.Value > currentExperiencelevel) {
												foundNext = true;
												//if the difference between these two is less than previous
												int difference = Mathf.Abs(currentExperiencelevel - credExpPair.Value);
												if (difference < smallestDifferenceSoFar) {
														nextExperienceLevel = credExpPair.Value;
														smallestDifferenceSoFar = nextExperienceLevel - currentExperiencelevel;
												}
										}
								}
						}
						return foundNext;
				}

				protected bool mCheckingCredentials = false;
				protected Dictionary <string, float> mNormalizedExpToNextCredentials = new Dictionary <string, float>();
				protected Dictionary <string, Dictionary <int,int>> mCredentialLevelsLookup = new Dictionary <string, Dictionary <int,int>>();
				protected Dictionary <string, Skill> mSkillsLookup = new Dictionary <string, Skill>();
				protected Dictionary <string, List <Skill>> mSkillsLookupByWorldItem = new Dictionary <string, List<Skill>>();
				protected Dictionary <string, List <Skill>> mSkillsLookupByWIScript = new Dictionary <string, List<Skill>>();
		}

		[Serializable]
		public class CredentialLevel
		{
				public string Flagset;
				[FrontiersBitMaskAttribute("GuildCredentials")]//TODO fix this
				public int Credentials;
				public int Experience;
		}
}
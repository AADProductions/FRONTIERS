using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers
{
		//this class will eventually be used to manage fx and sounds etc associated with conditions
		//it looks sparse now but it will play a pretty big role in multiplayer
		public class Conditions : Manager
		{
				public static Conditions Get;
				public List <Condition> ConditionList = new List <Condition>();

				public override void WakeUp()
				{
						Get = this;
				}

				public bool ConditionByName(string conditionName, out Condition condition)
				{
						condition = null;
						Condition findCondition = null;
						if (!mConditionLookup.TryGetValue(conditionName, out findCondition)) {
								if (Mods.Get.Runtime.LoadMod <Condition>(ref findCondition, "Condition", conditionName)) {
										mConditionLookup.Add(conditionName, findCondition);
								}
						}
						if (findCondition == null) {
								condition = null;
								return false;
						}
						//clone the object
						condition = ObjectClone.Clone <Condition>(findCondition);
						return true;
				}

				public void LoadConditions()
				{
						ConditionList.Clear();

						List <string> conditionNames = Mods.Get.ModDataNames("Condition");
						foreach (string conditionName in conditionNames) {
								Condition condition = null;
								if (Mods.Get.Runtime.LoadMod <Condition>(ref condition, "Condition", conditionName)) {
										ConditionList.Add(condition);
								}
						}
				}

				protected Dictionary <string,Condition> mConditionLookup = new Dictionary<string, Condition>();
				#if UNITY_EDITOR
				//i use this manager as an editor for status keepers & difficulty settings in the Unity editor
				//it doesn't actually manage either of these things it's just convenient
				public List <StatusKeeper> StatusKeepers = new List <StatusKeeper>();
				public List <DifficultySetting> DifficultySettings = new List <DifficultySetting>();

				public void DrawEditor()
				{
						UnityEngine.GUI.color = Color.cyan;
						if (GUILayout.Button("\nLoad Conditions from Disk\n")) {
								LoadEditor();
						}
						if (GUILayout.Button("\nSave Conditions to Disk\n")) {
								SaveEditor();
						}
						if (GUILayout.Button("\nCreate Conditions from Potions\n")) {
								bool foundCondition = false;
								if (!Manager.IsAwake <Potions>()) {
										Manager.WakeUp <Potions>("Frontiers_ObjectManagers");
								}
								foreach (Potion potion in Potions.Get.PotionList) {
										if (!string.IsNullOrEmpty(potion.EdibleProps.ConditionName)) {
												foreach (Condition condition in ConditionList) {
														if (potion.EdibleProps.ConditionName == condition.Name) {
																foundCondition = true;
																break;
														}
												}
												if (!foundCondition) {
														Condition newCondition = new Condition();
														newCondition.Name = potion.EdibleProps.ConditionName;
														ConditionList.Add(newCondition);
												}
										}
								}
						}

				}

				public void LoadStatusKeepers()
				{
						StatusKeepers.Clear();
						List <string> statusKeeperNames = Mods.Get.ModDataNames("StatusKeeper");
						foreach (string statusKeeperName in statusKeeperNames) {
								StatusKeeper keeper = null;
								if (Mods.Get.Runtime.LoadMod <StatusKeeper>(ref keeper, "StatusKeeper", statusKeeperName)) {
										StatusKeepers.Add(keeper);
								}
						}
				}

				public void LoadDifficultySettings()
				{
						DifficultySettings.Clear();
						List <string> difficultySettingNames = Mods.Get.ModDataNames("DifficultySetting");
						foreach (string difficultySettingName in difficultySettingNames) {
								DifficultySetting setting = null;
								if (Mods.Get.Editor.LoadMod <DifficultySetting>(ref setting, "DifficultySetting", difficultySettingName)) {
										DifficultySettings.Add(setting);
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

						foreach (Condition condition in ConditionList) {
								Mods.Get.Editor.SaveMod <Condition>(condition, "Condition", condition.Name);
						}
						foreach (StatusKeeper keeper in StatusKeepers) {
								Mods.Get.Editor.SaveMod <StatusKeeper>(keeper, "StatusKeeper", keeper.Name);
						}
						foreach (DifficultySetting setting in DifficultySettings) {
								Mods.Get.Editor.SaveMod <DifficultySetting>(setting, "DifficultySetting", setting.Name);
						}
				}

				public void LoadEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						LoadConditions();
						LoadStatusKeepers();
						LoadDifficultySettings();
				}
				#endif
		}
}
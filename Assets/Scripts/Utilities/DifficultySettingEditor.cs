using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Frontiers
{		[ExecuteInEditMode]
		public class DifficultySettingEditor : MonoBehaviour
		{
				#if UNITY_EDITOR
				[FrontiersAvailableModsAttribute("DifficultySetting")]
				public string DifficultySettingName;
				public string FlagToRemove = string.Empty;
				public int FlagToEdit = -1;
				public DifficultySetting Setting = null;
				public DifficultySettingGlobal CurrentGlobal;

				public void DrawEditor()
				{
						if (Application.isPlaying) {
								return;
						}

						UnityEngine.GUI.color = Color.cyan;
						GUILayout.Label("Death style:");
						Setting.DeathStyle = (DifficultyDeathStyle)EditorGUILayout.EnumPopup(Setting.DeathStyle);
						GUILayout.Label("Global settings:");
						if (GUILayout.Button("Add global variable")) {
								Setting.GlobalVariables.Add(new DifficultySettingGlobal());
						}
						foreach (DifficultySettingGlobal dsg in Setting.GlobalVariables) {
								DrawGlobalVariable(dsg);
						}
						UnityEngine.GUI.color = Color.green;
						GUILayout.Label("Difficulty flags:");
						if (GUILayout.Button("Add flag")) {
								Setting.DifficultyFlags.Add("NewFlag");
						}
						for (int i = 0; i < Setting.DifficultyFlags.Count; i++) {
								Setting.DifficultyFlags[i] = DrawDifficultyFlag(Setting.DifficultyFlags[i], i);
						}
						UnityEngine.GUI.color = Color.yellow;
						GUILayout.Label("_");
						if (GUILayout.Button("Save setting")) {
								SaveSetting();
						}
						if (GUILayout.Button("Load setting")) {
								LoadSetting();
						}

						if (!string.IsNullOrEmpty(FlagToRemove)) {
								Setting.DifficultyFlags.Remove(FlagToRemove);
								FlagToRemove = null;
								FlagToEdit = -1;
						}

				}

				public void Update()
				{
						if (Setting == null) {
								LoadSetting();
						} else if (Setting.Name != DifficultySettingName) {
								LoadSetting();
						}
				}

				public string DrawDifficultyFlag(string flag, int flagIndex)
				{
						UnityEditor.EditorGUILayout.BeginHorizontal();
						if (flagIndex == FlagToEdit) {
								flag = EditorGUILayout.TextField(flag);
						} else {
								if (GUILayout.Button(flag)) {
										FlagToEdit = flagIndex;
								}
						}
						if (GUILayout.Button("Remove")) {
								FlagToRemove = flag;
						}
						UnityEditor.EditorGUILayout.EndHorizontal();
						return flag;
				}

				public void DrawGlobalVariable(DifficultySettingGlobal dsg)
				{
						UnityEditor.EditorGUILayout.BeginHorizontal();
						if (dsg == CurrentGlobal) {
								dsg.GlobalVariableName = Mods.ModsEditor.GUILayoutGLobal(dsg.GlobalVariableName);
								dsg.VariableValue = Mods.ModsEditor.GUILayoutGlobalValue(dsg.VariableValue, dsg.GlobalVariableName);
								if (GUILayout.Button("Done")) {
										CurrentGlobal = null;
								}
						} else {
								GUILayout.Button(dsg.GlobalVariableName);
								dsg.VariableValue = EditorGUILayout.TextField(dsg.VariableValue);
								if (GUILayout.Button("Edit")) {
										CurrentGlobal = dsg;
								}
						}
						UnityEditor.EditorGUILayout.EndHorizontal();
						dsg.Description = EditorGUILayout.TextArea(dsg.Description);
				}

				public void LoadSetting()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor(true);
						Mods.Get.Editor.LoadMod <DifficultySetting>(ref Setting, "DifficultySetting", DifficultySettingName);
						FlagToEdit = -1;
						FlagToRemove = null;
						CurrentGlobal = null;
				}

				public void SaveSetting()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor(true);
						DifficultySettingName = Setting.Name;
						Mods.Get.Editor.SaveMod <DifficultySetting>(Setting, "DifficultySetting", DifficultySettingName);
						FlagToEdit = -1;
						FlagToRemove = null;
						CurrentGlobal = null;
				}
				#endif
		}
}
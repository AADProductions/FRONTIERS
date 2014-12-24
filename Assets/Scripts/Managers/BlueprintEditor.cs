using UnityEngine;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		[ExecuteInEditMode]
		public class BlueprintEditor : MonoBehaviour
		{
				#if UNITY_EDITOR
				[FrontiersAvailableModsAttribute("Blueprint")]
				public string BlueprintName = string.Empty;
				public WIBlueprint Blueprint = new WIBlueprint();
				public bool CreatingNewBlueprint = false;
				int selectedItemIndex = 0;
				int selectedRowIndex = 0;
				bool openWindow = false;
				bool chooseResult = false;

				public void Update()
				{
						if (!Application.isPlaying) {
								LoadBlueprint(false);
						}
				}

				public void DrawEditor()
				{
						if (Application.isPlaying) {
								return;
						}

						if (CreatingNewBlueprint) {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Label("[Editing new blueprint - this blueprint has not been saved]");
						}
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						if (!chooseResult) {
								if (Blueprint.GenericResult != null && !Blueprint.GenericResult.IsEmpty) {
										if (GUILayout.Button(Blueprint.GenericResult.PrefabName + " (Click to change)", UnityEditor.EditorStyles.miniButton)) {	
												chooseResult = true;
										}
								} else {
										if (GUILayout.Button("(Choose Result)", UnityEditor.EditorStyles.miniButton)) {
												chooseResult = true;						
										}
								}
						} else {
								DrawGenericWorldItemSelector(Blueprint.GenericResult, ref chooseResult, false);
								if (!string.IsNullOrEmpty(Blueprint.GenericResult.PrefabName)) {
										Blueprint.Name = Blueprint.GenericResult.PrefabName.Replace(" ", "");
										Blueprint.Title = Blueprint.GenericResult.PrefabName;
								}
						}

						GUILayout.EndHorizontal();
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						GUILayout.Label("Name:");
						Blueprint.Name = GUILayout.TextField(Blueprint.Name);
						if (string.IsNullOrEmpty(Blueprint.Name)) {
								if (!Blueprint.GenericResult.IsEmpty) {
										Blueprint.Name = Blueprint.GenericResult.PrefabName;
								}
						}
						GUILayout.Label("Title:");
						Blueprint.Title = GUILayout.TextField(Blueprint.Title);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("Description:");
						Blueprint.Description = GUILayout.TextArea(Blueprint.Description);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						Blueprint.Revealed = GUILayout.Toggle(Blueprint.Revealed, "Revealed");
						Blueprint.UseGenericResult = GUILayout.Toggle(Blueprint.UseGenericResult, "Generic Result");
						Blueprint.BaseCraftTime = UnityEditor.EditorGUILayout.FloatField(Blueprint.BaseCraftTime);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("Row 1:");
						for (int i = 0; i < Blueprint.Row1.Count; i++) {
								DrawBlueprintRowItem(Blueprint.Row1[i], i, 1);
						}
						GUILayout.EndHorizontal();
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						GUILayout.Label("Row 2:");
						for (int i = 0; i < Blueprint.Row2.Count; i++) {
								DrawBlueprintRowItem(Blueprint.Row2[i], i, 2);
						}
						GUILayout.EndHorizontal();
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						GUILayout.Label("Row 3:");
						for (int i = 0; i < Blueprint.Row3.Count; i++) {
								DrawBlueprintRowItem(Blueprint.Row3[i], i, 3);
						}
						GUILayout.EndHorizontal();

						UnityEngine.GUI.color = Color.cyan;
						if (GUILayout.Button("\nSave Blueprint\n")) {
								SaveBlueprint();
						}
						if (GUILayout.Button("\nRe-Load Blueprint\n")) {
								LoadBlueprint(true);
						}
						if (GUILayout.Button("\nNEW Blueprint\n")) {
								NewBlueprint();
						}
						if (GUILayout.Button("\nREPAIR Blueprints\n")) {
								RepairBlueprints();
						}
				}

				public void RepairBlueprints()
				{
						List <WIBlueprint> blueprints = new List<WIBlueprint>();
						Mods.Get.Editor.LoadAvailableMods(blueprints, "Blueprint");
						WorldItem prefab = null;
						foreach (WIBlueprint bp in blueprints) {
								if (bp.RequiredSkill == "Craft") {
										bp.Title = WorldItems.CleanWorldItemName(bp.GenericResult.DisplayName);
								} else {
										bp.Revealed = true;
								}
								List <List<GenericWorldItem>> rows = new List<List<GenericWorldItem>>();
								rows.Add(bp.Row1);
								rows.Add(bp.Row2);
								rows.Add(bp.Row3);
								foreach (List<GenericWorldItem> row in rows) {
										bp.Strictness = BlueprintStrictness.StackName;
										if (WorldItems.Get.PackPrefab(bp.GenericResult.PackName, bp.GenericResult.PrefabName, out prefab)) {
												if (string.IsNullOrEmpty(bp.GenericResult.DisplayName)) {
														bp.GenericResult.DisplayName = prefab.DisplayName;
												}
												if (string.IsNullOrEmpty(bp.GenericResult.StackName)) {
														bp.GenericResult.StackName = prefab.StackName;
												}
												if (string.IsNullOrEmpty(bp.GenericResult.Subcategory)) {
														bp.GenericResult.Subcategory = prefab.Subcategory;
												}
												if (string.IsNullOrEmpty(bp.GenericResult.State)) {
														bp.GenericResult.State = prefab.State;
												}
										}
										foreach (GenericWorldItem gwi in row) {
												if (!gwi.IsEmpty) {
														if (WorldItems.Get.PackPrefab(gwi.PackName, gwi.PrefabName, out prefab)) {
																if (string.IsNullOrEmpty(gwi.DisplayName)) {
																		gwi.DisplayName = prefab.DisplayName;
																}
																if (string.IsNullOrEmpty(gwi.StackName)) {
																		gwi.StackName = prefab.StackName;
																}
																if (string.IsNullOrEmpty(gwi.Subcategory)) {
																		gwi.Subcategory = prefab.Subcategory;
																}
																if (string.IsNullOrEmpty(gwi.State)) {
																		gwi.State = prefab.State;
																}
														} else {
																gwi.Clear();
														}
												}
										}
								}
								Mods.Get.Editor.SaveMod <WIBlueprint>(bp, "Blueprint", bp.Name);
						}
				}

				public void DrawBlueprintRowItem(GenericWorldItem item, int itemIndex, int rowIndex)
				{
						if (selectedItemIndex == itemIndex && rowIndex == selectedRowIndex && openWindow) {
								UnityEngine.GUI.color = Color.yellow;
								DrawGenericWorldItemSelector(item, ref openWindow, true);
						} else {
								UnityEngine.GUI.color = Color.white;
								if (item == null || item.IsEmpty) {	
										UnityEngine.GUI.color = Color.Lerp(Color.grey, Color.black, 0.5f);
										if (GUILayout.Button("\n\n\n =(Empty)= \n\n\n", UnityEditor.EditorStyles.miniButton)) {
												selectedRowIndex = rowIndex;
												selectedItemIndex = itemIndex;
												openWindow = true;				
										}
								} else {
										UnityEngine.GUI.color = Color.Lerp(Color.green, Colors.ColorFromString(item.PrefabName, 150), 0.85f);
										string buttonName = item.PrefabName;
										int numReturns = 4;
										if (!string.IsNullOrEmpty(item.StackName)) {
												buttonName += "\n(" + item.StackName + ")";
												numReturns--;
										}
										if (!string.IsNullOrEmpty(item.State) && item.State != "Default") {
												buttonName += "\n(" + item.State + ")";
												numReturns--;
										}
										if (!string.IsNullOrEmpty(item.Subcategory)) {
												buttonName += "\n(" + item.Subcategory + ")";
												numReturns--;
										}
										string returns = string.Empty;
										for (int i = 0; i < numReturns; i++) {
												returns += "\n";
										}
										if (GUILayout.Button(returns + buttonName + returns, UnityEditor.EditorStyles.miniButton)) {
												selectedRowIndex = rowIndex;
												selectedItemIndex = itemIndex;
												openWindow = true;			
										}
								}
						}						
				}

				public void DrawGenericWorldItemSelector(GenericWorldItem item, ref bool open, bool useStrictness)
				{

						if (WorldItems.Get == null) {
								Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
								WorldItems.Get.Initialize();
						}

						bool drawEditor = true;
						UnityEditor.EditorStyles.miniButton.stretchWidth = true;
						UnityEditor.EditorStyles.textField.stretchWidth = true;
						UnityEditor.EditorStyles.popup.stretchWidth = true;

						GUILayout.BeginVertical();

						if (string.IsNullOrEmpty(item.PackName)) {
								drawEditor = false;
								UnityEngine.GUI.color = Color.green;
								if (GUILayout.Button("(Add)", UnityEditor.EditorStyles.miniButton)) {
										item.PackName = "AncientArtifacts";
										item.PrefabName = string.Empty;
										drawEditor = true;
								}
								UnityEngine.GUI.color = Color.yellow;
								if (GUILayout.Button("(Done)", UnityEditor.EditorStyles.miniButton)) {
										drawEditor = false;
										selectedItemIndex = -1;
										selectedRowIndex = -1;
								}
						} else {
								UnityEngine.GUI.color = Color.red;
								if (GUILayout.Button("(Clear)", UnityEditor.EditorStyles.miniButton)) {
										item.Clear();
										drawEditor = false;
								}
						}

						if (drawEditor) {
								UnityEngine.GUI.color = Color.yellow;
								int packIndex = 0;
								List <string> packNames = new List<string>();
								WorldItemPack pack = null;
								WorldItem worlditem = null;
								for (int i = 0; i < WorldItems.Get.WorldItemPacks.Count; i++) {
										string packName = WorldItems.Get.WorldItemPacks[i].Name;
										packNames.Add(packName);
										if (item.PackName == packName) {
												packIndex = i;
												pack = WorldItems.Get.WorldItemPacks[i];
										}
								}
								if (pack == null) {
										pack = WorldItems.Get.WorldItemPacks[0];
								}

								packIndex = UnityEditor.EditorGUILayout.Popup("Pack:", packIndex, packNames.ToArray(), UnityEditor.EditorStyles.popup);
								List <string> prefabNames = new List<string>();
								int prefabIndex = 0;
								for (int i = 0; i < pack.Prefabs.Count; i++) {
										prefabNames.Add(pack.Prefabs[i].name);
										if (item.PrefabName == pack.Prefabs[i].name) {
												prefabIndex = i;
												worlditem = pack.Prefabs[i].GetComponent <WorldItem>();
										}
								}
								prefabIndex = UnityEditor.EditorGUILayout.Popup("Prefab:", prefabIndex, prefabNames.ToArray(), UnityEditor.EditorStyles.popup);

								item.PackName = packNames[packIndex];
								item.PrefabName = prefabNames[prefabIndex];
			
								if (worlditem != null) {
										UnityEngine.GUI.color = Color.yellow;
										item.DisplayName = worlditem.DisplayName;
										if (!useStrictness || Flags.Check((uint)Blueprint.Strictness, (uint)BlueprintStrictness.StackName, Flags.CheckType.MatchAny)) {
												GUILayout.BeginHorizontal();
												GUILayout.Label("StackName: ");
												if (string.IsNullOrEmpty(item.StackName) || !worlditem.StackName.Contains(item.StackName)) {
														item.StackName = worlditem.StackName;
												}
												item.StackName = GUILayout.TextField(item.StackName);
												GUILayout.EndHorizontal();
										}
										if (!useStrictness || Flags.Check((uint)Blueprint.Strictness, (uint)BlueprintStrictness.StateName, Flags.CheckType.MatchAny)) {
												GUILayout.BeginHorizontal();
												int stateIndex = 0;
												List <string> stateNames = new List<string>();
												stateNames.Add("Default");
												WIStates states = worlditem.GetComponent <WIStates>();
												if (states != null) {
														for (int i = 0; i < states.States.Count; i++) {
																stateNames.Add(states.States[i].Name);
																if (item.State == states.States[i].Name) {
																		stateIndex = i + 1;//since the first one is Default
																}
														}
												}
												stateIndex = UnityEditor.EditorGUILayout.Popup("State:", stateIndex, stateNames.ToArray(), UnityEditor.EditorStyles.popup);
												GUILayout.EndHorizontal();
												item.State = stateNames[stateIndex];
										}
								}
								UnityEngine.GUI.color = Color.white;
								if (!useStrictness || Flags.Check((uint)Blueprint.Strictness, (uint)BlueprintStrictness.Subcategory, Flags.CheckType.MatchAny)) {
										GUILayout.BeginHorizontal();
										GUILayout.Label("Subcategory: ");
										item.Subcategory = GUILayout.TextField(item.Subcategory);
										GUILayout.EndHorizontal();
								}
								UnityEngine.GUI.color = Color.yellow;
								if (GUILayout.Button("(Done)", UnityEditor.EditorStyles.miniButton)) {
										open = false;
								}
						}

						GUILayout.EndVertical();

						UnityEditor.EditorStyles.miniButton.stretchWidth = true;
						UnityEditor.EditorStyles.textField.stretchWidth = true;
						UnityEditor.EditorStyles.popup.stretchWidth = true;
				}

				public void NewBlueprint()
				{
						CreatingNewBlueprint = true;
						selectedRowIndex = -1;
						selectedItemIndex = -1;
						Blueprint = new WIBlueprint();
						Blueprint.Clear(true);
						Blueprint.Name = "EmptyBlueprint";
				}

				public void LoadBlueprint(bool force)
				{
						if (CreatingNewBlueprint && !force) {
								return;
						}
						if (Blueprint.Name != BlueprintName || force) {
								if (!Manager.IsAwake <Mods>()) {
										Manager.WakeUp <Mods>("__MODS");
								}
								Mods.Get.Editor.InitializeEditor(true);

								Mods.Get.Editor.LoadMod <WIBlueprint>(ref Blueprint, "Blueprint", BlueprintName);
						}
				}

				public void SaveBlueprint()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor(true);
						BlueprintName = Blueprint.Name;
						Mods.Get.Editor.SaveMod <WIBlueprint>(Blueprint, "Blueprint", BlueprintName);
						CreatingNewBlueprint = false;
				}
				#endif
		}
}
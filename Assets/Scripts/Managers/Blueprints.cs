using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System.Xml;
using System.Xml.Serialization;
using Frontiers.World.Gameplay;

namespace Frontiers
{
		public class Blueprints : Manager
		{
				public static Blueprints Get;
				public List <WIBlueprint> LoadedBlueprints = new List<WIBlueprint> ();

				public override void WakeUp()
				{
						Get = this;
						Debug.Log("Just woke up in blueprints");
				}

				public List <string> Categories {
						get {
								List <string> categories = new List <string>();
								categories.AddRange(mCategories.Keys);
								if (mEditorBlueprints.Count > 0) {
										categories.Add("(Unsaved)");
								}
								return categories;
						}
				}

				public static void ClearLog()
				{
						Mods.Get.Runtime.ResetProfileData("Blueprint");
						Get.RefreshCategories();
				}

				public override void Initialize()
				{
						base.Initialize();
						mAllCategory = new List <string>();
						mLoadedBlueprints = new Dictionary <string, WIBlueprint>();
						mPatternLookup = new Dictionary<int, List <WIBlueprint>>();
						mBlueprintAssociations = new Dictionary <GenericWorldItem, WIBlueprint>();
						mEditorBlueprints = new List <WIBlueprint>();
						mAssociationWorldItem = new GenericWorldItem();
				}

				public override void OnGameLoadStart()
				{
						Mods.Get.Runtime.LoadAvailableMods <WIBlueprint>(LoadedBlueprints, "Blueprint");
						for (int i = 0; i < LoadedBlueprints.Count; i++) {
								//add them to the name lookup for easy retrieval
								mLoadedBlueprints.Add(LoadedBlueprints[i].Name, LoadedBlueprints[i]);
								//generate a lookup pattern to make matching easier
								int pattern = GeneratePattern(LoadedBlueprints[i]);
								//add that to a list
								List <WIBlueprint> patternList = null;
								if (!mPatternLookup.TryGetValue(pattern, out patternList)) {
										patternList = new List<WIBlueprint>();
										mPatternLookup.Add(pattern, patternList);
								}
								patternList.Add(LoadedBlueprints[i]);
						}
						RefreshCategories();
				}

				public bool BlueprintsByPattern (int pattern, List <WIBlueprint> blueprints) {
						List <WIBlueprint> blueprintList = null;
						if (mPatternLookup.TryGetValue (pattern, out blueprintList)) {
								blueprints.AddRange (blueprintList);
								return true;
						}
						return false;
				}

				public int GeneratePattern (WIBlueprint blueprint) {
						//generates a bitmask that can be used to look up blueprints more quickly
						int pattern = 0;
						//i try to avoid actually naming the number of columns in a blueprint
						//this is to leave the door open for more than 3 columns
						blueprint.Rows = new List<GenericWorldItem>();
						blueprint.Rows.AddRange(blueprint.Row1);
						blueprint.Rows.AddRange(blueprint.Row2);
						blueprint.Rows.AddRange(blueprint.Row3);
						for (int i = 0; i < blueprint.Rows.Count; i++) {
								if (!blueprint.Rows[i].IsEmpty) {
										pattern |= 1 << i;
								}
						}
						//set the pattern in case we want to use it later (?)
						blueprint.Pattern = pattern;
						return pattern;
				}

				public bool Blueprint(string blueprintName, out WIBlueprint blueprint, bool onlyIfRevealed)
				{
						//Temp TODO remove this
						onlyIfRevealed = false;
						blueprint = null;
						if (mLoadedBlueprints.TryGetValue(blueprintName, out blueprint)) {
								if (onlyIfRevealed) {
										return blueprint.Revealed;
								} else {
										return true;
								}
						}
						//we're loading all blueprints on startup now so this is unnecessary
						/* else if (Mods.Get.Runtime.LoadMod(ref blueprint, "Blueprint", blueprintName)) {
								//get the description for the blueprint
								WorldItem prefab = null;
								if (WorldItems.Get.PackPrefab(blueprint.GenericResult.PackName, blueprint.GenericResult.PrefabName, out prefab)) {
										//blueprint.Description = prefab.DisplayName;
										if (!string.IsNullOrEmpty(prefab.Props.Global.ExamineInfo.StaticExamineMessage)) {
												blueprint.Description = prefab.Props.Global.ExamineInfo.StaticExamineMessage;
										}
								}
								mLoadedBlueprints.Add(blueprintName, blueprint);
								if (onlyIfRevealed) {
										return blueprint.Revealed;
								} else {
										return true;
								}
						}*/
						return false;
				}

				public bool HasBlueprint(string blueprintName)
				{
						WIBlueprint blueprint = null;
						return Blueprint(blueprintName, out blueprint, true);
				}

				public bool Blueprint(string blueprintName, out WIBlueprint blueprint)
				{
						blueprint = null;
						if (mLoadedBlueprints.TryGetValue(blueprintName, out blueprint)) {
								return true;
						}
						//we're loading all blueprints on startup now so this is unnecessary
						/*else if (Mods.Get.Runtime.LoadMod(ref blueprint, "Blueprint", blueprintName)) {
								mLoadedBlueprints.Add(blueprintName, blueprint);
								return true;
						}*/
						return false;
				}

				public bool Category(string categoryName, out List <string> blueprintNames)
				{
						//TEMP
						//blueprintNames = mAllCategory;
						if (!mCategories.TryGetValue(categoryName, out blueprintNames)) {//just give it an empty list
								//cooking, crafting etc. will be asking for these a lot
								blueprintNames = new List <string>();
								return false;
						}
						return true;
				}

				public List <WIBlueprint> BlueprintsByCategory(string categoryName)
				{
						if (categoryName == "(Unsaved)") {
								return mEditorBlueprints;
						}

						List <WIBlueprint> blueprints = new List<WIBlueprint>();
						List <string> blueprintNames = new List<string>();
						if (Category(categoryName.ToLower().Trim(), out blueprintNames)) {
								foreach (string blueprintName in blueprintNames) {
										WIBlueprint blueprint = null;
										if (Blueprint(blueprintName, out blueprint, true) && blueprint.Revealed) {
												blueprints.Add(blueprint);
										}
								}
						}
						return blueprints;
				}

				protected void RefreshCategories()
				{
						//get all available blueprints
						//then check each and put revealed blueprints in their categories
						mCategories = new Dictionary <string, List<string>>();
						mAllCategory.Clear();
						mEditorBlueprints.Clear();
						mBlueprintAssociations.Clear();

						for (int i = 0; i < LoadedBlueprints.Count; i++) {
								WIBlueprint blueprint = LoadedBlueprints[i];
								if (blueprint.IsEmpty) {
										Debug.Log("Empty blueprint, adding temporary result");
										blueprint.UseGenericResult = true;
										blueprint.GenericResult = new GenericWorldItem("Edibles", "Bacon");
								}

								if (!mBlueprintAssociations.ContainsKey(blueprint.GenericResult)) {
										mBlueprintAssociations.Add(blueprint.GenericResult, blueprint);
								}
								mAllCategory.Add(blueprint.Name);
								List <string> blueprintNameList = null;
								if (!mCategories.TryGetValue(blueprint.RequiredSkill.ToLower(), out blueprintNameList)) {
										blueprintNameList = new List <string>();
										mCategories.Add(blueprint.RequiredSkill.ToLower(), blueprintNameList);
								}
								blueprintNameList.Add(blueprint.Name);
						}
				}

				public bool IsCraftable(IWIBase craftableItem, out string blueprintName)
				{
						blueprintName = string.Empty;
						return false;
				}

				public bool IsCraftable(GenericWorldItem craftableItem, out string blueprintName, bool requireRevealed)
				{
						blueprintName = string.Empty;
						foreach (KeyValuePair <GenericWorldItem,WIBlueprint> association in mBlueprintAssociations) {
								if (craftableItem.Equals(association.Key) && (!requireRevealed || association.Value.Revealed)) {
										blueprintName = association.Value.Name;
										return true;
								}
						}
						return false;
				}

				public bool BlueprintExistsForItem(WorldItem worlditem, out WIBlueprint blueprint)
				{
						blueprint = null;
						mAssociationWorldItem.CopyFrom(worlditem);
						//Debug.Log ("Checking if blueprint exists for " + mAssociationWorldItem.PrefabName);
						foreach (KeyValuePair <GenericWorldItem,WIBlueprint> keyValue in mBlueprintAssociations) {
								if (string.Equals(keyValue.Key.PackName, mAssociationWorldItem.PackName)
								    && string.Equals(keyValue.Key.PrefabName, mAssociationWorldItem.PrefabName)) {
										blueprint = keyValue.Value;
										break;
								}
						}
						return blueprint != null;
						//return mBlueprintAssociations.TryGetValue (mAssociationWorldItem, out blueprint);
				}

				public void Reveal(string blueprintName, BlueprintRevealMethod method, string source)
				{
						WIBlueprint blueprint = null;
						if (Mods.Get.Runtime.LoadMod(ref blueprint, "Blueprint", blueprintName)) {
								if (!blueprint.Revealed) {
										blueprint.Revealed = true;
										blueprint.RevealMethod = method;
										blueprint.RevealSource = source;
										GUI.GUIManager.PostGainedItem(blueprint);
								}
								blueprint.Instances++;
								Mods.Get.Runtime.SaveMod(blueprint, "Blueprint", blueprintName);
						}
				}

				public WIBlueprint EditorCreateBlueprint(string blueprintName, string requiredSkillName)
				{
						WIBlueprint newBlueprint = new WIBlueprint();
						newBlueprint.Name = blueprintName;
						newBlueprint.RequiredSkill = requiredSkillName;
						mEditorBlueprints.Add(newBlueprint);
						return newBlueprint;
				}
				#if UNITY_EDITOR
				public void InitializeEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						if (mCategories == null) {
								RefreshCategoriesEditor();
						}
				}

				protected void RefreshCategoriesEditor()
				{
						//get all available blueprints
						//then check each and put revealed blueprints in their categories
						mCategories = new Dictionary <string, List<string>>();
						mBlueprintAssociations.Clear();
						mAllCategory.Clear();
						mEditorBlueprints.Clear();
						List <string> blueprintNames = Mods.Get.Available("Blueprint", DataType.World);
						foreach (string blueprintName in blueprintNames) {	//Debug.Log ("refreshing blueprint " + blueprintName);
								WIBlueprint blueprint = null;
								if (Mods.Get.Editor.LoadMod <WIBlueprint>(ref blueprint, "Blueprint", blueprintName)) {
										if (blueprint.Revealed) {
												if (!mBlueprintAssociations.ContainsKey(blueprint.GenericResult)) {
														mBlueprintAssociations.Add(blueprint.GenericResult, blueprint);
												}
												mAllCategory.Add(blueprint.Name);
												List <string> blueprintNameList = null;
												if (!mCategories.TryGetValue(blueprint.RequiredSkill.ToLower(), out blueprintNameList)) {
														blueprintNameList = new List <string>();
														mCategories.Add(blueprint.RequiredSkill.ToLower(), blueprintNameList);
												}
												blueprintNameList.Add(blueprint.Name);
										}
								}
						}
				}

				public void SaveBlueprintsEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						foreach (WIBlueprint blueprint in mLoadedBlueprints.Values) {
								Mods.Get.Editor.SaveMod <WIBlueprint>(blueprint, "Blueprint", blueprint.Name);
						}
						foreach (WIBlueprint newBlueprint in mEditorBlueprints) {
								Mods.Get.Editor.SaveMod <WIBlueprint>(newBlueprint, "Blueprint", newBlueprint.Name);
						}
				}

				public void LoadBlueprintsEditor()
				{
						if (Application.isPlaying)
								return;

						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						RefreshCategoriesEditor();
				}

				string selectedCategory = string.Empty;
				string selectedBlueprintName = string.Empty;
				WIBlueprint selectedBlueprint = null;
				string newCatName = string.Empty;
				bool openWindow = false;
				List <GenericWorldItem> rowSelection;
				int rowIndex;
				string selectedPack = string.Empty;

				public void SelectWorldItem()
				{	
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.Label("SELECT WORLD ITEM:");
						if (GUILayout.Button("CLOSE")) {
								openWindow = false;
						}
						foreach (WorldItemPack pack in WorldItems.Get.WorldItemPacks) {
								if (pack.Name == selectedPack) {
										UnityEngine.GUI.color = Color.yellow;
										foreach (GameObject prefab in pack.Prefabs) {
												if (GUILayout.Button(prefab.name)) {
														WorldItem worlditem = prefab.GetComponent <WorldItem>();
														if (rowSelection == null && rowIndex == 100) {
																selectedBlueprint.GenericResult = new GenericWorldItem(worlditem);
																rowIndex = 0;
																openWindow = false;
														} else {
																rowSelection[rowIndex] = new GenericWorldItem(worlditem);
																openWindow = false;
														}
												}
										}
								} else if (GUILayout.Button(pack.Name)) {
										UnityEngine.GUI.color = Color.cyan;
										selectedPack = pack.Name;
								} else {
										UnityEngine.GUI.color = Color.cyan;
								}
						}
				}

				public void DrawEditor()
				{
						if (WorldItems.Get == null) {
								Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
								WorldItems.Get.Initialize();
						}

						GUILayout.Label("Blueprint categories:");

						if (openWindow) {
								SelectWorldItem();
						}

						UnityEngine.GUI.color = Color.cyan;
						if (!string.IsNullOrEmpty(selectedCategory)) {
								GUILayout.Label("Selected category: " + selectedCategory);
								List <WIBlueprint> blueprintList = BlueprintsByCategory(selectedCategory);
								foreach (WIBlueprint blueprint in blueprintList) {				
										//				GUILayout.BeginHorizontal ( );			
										UnityEngine.GUI.color = Color.yellow;
										if (blueprint == selectedBlueprint) {
												UnityEngine.GUI.color = Color.cyan;
												GUILayout.Label("Selected blueprint:");
												GUILayout.BeginHorizontal();
												if (blueprint.GenericResult != null && !string.IsNullOrEmpty(blueprint.GenericResult.PrefabName)) {
														if (GUILayout.Button(blueprint.GenericResult.PrefabName)) {	
																rowSelection = null;
																rowIndex = 100;
																openWindow = true;
														}
												} else {
														if (GUILayout.Button("(Choose result)")) {
																rowSelection = null;
																rowIndex = 100;
																openWindow = true;							
														}
												}
												GUILayout.EndHorizontal();

												GUILayout.BeginHorizontal();
												GUILayout.Label("Name:");
												blueprint.Name = GUILayout.TextField(blueprint.Name);
												if (string.IsNullOrEmpty(blueprint.Name)) {
														if (!blueprint.GenericResult.IsEmpty) {
																blueprint.Name = blueprint.GenericResult.PrefabName;
														}
												}
												GUILayout.Label("Required Skill:");
												blueprint.RequiredSkill = GUILayout.TextField(blueprint.RequiredSkill);
												GUILayout.Label("Title:");
												blueprint.Title = GUILayout.TextField(blueprint.Title);
												GUILayout.EndHorizontal();

												GUILayout.BeginHorizontal();
												blueprint.Revealed = GUILayout.Toggle(blueprint.Revealed, "Revealed");
												blueprint.UseGenericResult = GUILayout.Toggle(blueprint.UseGenericResult, "Generic Result");
												blueprint.BaseCraftTime = UnityEditor.EditorGUILayout.FloatField(blueprint.BaseCraftTime);
												GUILayout.EndHorizontal();

												GUILayout.BeginHorizontal();
												GUILayout.Label("Row 1:");
												for (int i = 0; i < selectedBlueprint.Row1.Count; i++) {
														if (i == rowIndex && selectedBlueprint.Row1 == rowSelection) {
																UnityEngine.GUI.color = Color.yellow;
														} else {
																UnityEngine.GUI.color = Color.white;
														}						

														if (selectedBlueprint.Row1[i] == null || selectedBlueprint.Row1[i].StackName == string.Empty) {	
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.gray, 0.5f);
																if (GUILayout.Button("\n\n -(Empty)= \n\n")) {
																		rowSelection = selectedBlueprint.Row1;
																		rowIndex = i;
																		openWindow = true;					
																}
														} else {
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.gray, 0.5f);
																if (GUILayout.Button("\n\n" + selectedBlueprint.Row1[i].StackName + "\n\n")) {
																		rowSelection = selectedBlueprint.Row1;
																		rowIndex = i;
																		openWindow = true;	
																}
														}
												}
												GUILayout.EndHorizontal();
												UnityEngine.GUI.color = Color.cyan;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Row 2:");
												for (int i = 0; i < selectedBlueprint.Row2.Count; i++) {
														if (i == rowIndex && selectedBlueprint.Row2 == rowSelection) {
																UnityEngine.GUI.color = Color.yellow;
														} else {
																UnityEngine.GUI.color = Color.white;
														}							

														if (selectedBlueprint.Row2[i] == null || selectedBlueprint.Row2[i].StackName == string.Empty) {
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.gray, 0.5f);
																if (GUILayout.Button("\n\n -(Empty)= \n\n")) {
																		rowSelection = selectedBlueprint.Row2;
																		rowIndex = i;
																		openWindow = true;	
																}
														} else {
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.white, 0.5f);
																if (GUILayout.Button("\n\n" + selectedBlueprint.Row2[i].StackName + "\n\n")) {
																		rowSelection = selectedBlueprint.Row2;
																		rowIndex = i;
																		openWindow = true;	
																}
														}
												}
												GUILayout.EndHorizontal();
												UnityEngine.GUI.color = Color.cyan;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Row 3:");
												for (int i = 0; i < selectedBlueprint.Row3.Count; i++) {
														if (i == rowIndex && selectedBlueprint.Row3 == rowSelection) {
																UnityEngine.GUI.color = Color.yellow;
														} else {
																UnityEngine.GUI.color = Color.white;
														}						

														if (selectedBlueprint.Row3[i] == null || selectedBlueprint.Row3[i].StackName == string.Empty) {
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.gray, 0.5f);
																if (GUILayout.Button("\n\n -(Empty)= \n\n")) {
																		rowSelection = selectedBlueprint.Row3;
																		rowIndex = i;
																		openWindow = true;	
																}
														} else {
																UnityEngine.GUI.color = Color.Lerp(UnityEngine.GUI.color, Color.white, 0.5f);
																if (GUILayout.Button("\n\n" + selectedBlueprint.Row3[i].StackName + "\n\n")) {
																		rowSelection = selectedBlueprint.Row3;
																		rowIndex = i;
																		openWindow = true;	
																}
														}
												}
												GUILayout.EndHorizontal();
										} else {
												if (blueprint.GenericResult != null) {
														if (GUILayout.Button(blueprint.GenericResult.StackName)) {
																selectedBlueprint = blueprint;
														}
												} else {
														if (GUILayout.Button(" (empty blueprint) ")) {
																selectedBlueprint = blueprint;
														}
												}
										}
								}
								UnityEngine.GUI.color = Color.cyan;
								GUILayout.BeginHorizontal();
								GUILayout.Label("     ");
								if (GUILayout.Button("Create blueprint")) {
										EditorCreateBlueprint("New blueprint", selectedCategory);
								}			
								GUILayout.Label("     ");
								GUILayout.EndHorizontal();

								if (GUILayout.Button("\nTOP\n")) {
										selectedCategory = null;
								}
						} else {
								UnityEngine.GUI.color = Color.cyan;
								if (GUILayout.Button("\nTOP\n")) {
										selectedCategory = null;
								} else {
										UnityEngine.GUI.color = Color.cyan;
										foreach (string category in Categories) {
												if (GUILayout.Button(category)) {
														selectedCategory = category;
														return;
												}
										}
								}
						}

						UnityEngine.GUI.color = Color.yellow;
						if (GUILayout.Button("\n SAVE TO DISK \n")) {
								SaveBlueprintsEditor();
						}
						if (GUILayout.Button("\n LOAD FROM DISK \n")) {
								LoadBlueprintsEditor();
						}
				}
				#endif
				protected Dictionary <string, List <string>> mCategories = null;
				protected List <string> mAllCategory;
				protected Dictionary <string, WIBlueprint> mLoadedBlueprints;
				protected Dictionary<int, List <WIBlueprint>> mPatternLookup;
				protected Dictionary <GenericWorldItem, WIBlueprint> mBlueprintAssociations;
				protected List <WIBlueprint> mEditorBlueprints;
				protected GenericWorldItem mAssociationWorldItem;
		}

		[Serializable]
		public class WIBlueprint : Mod
		{
				public WIBlueprint()
				{
						Type = "Blueprint";
						Clear(false);
				}

				public string CleanName {
						get {
								if (!string.IsNullOrEmpty(Title)) {
										return Title;
								}
								if (!string.IsNullOrEmpty(GenericResult.StackName)) {
										return WorldItems.CleanWorldItemName(GenericResult.StackName);
								}
								return WorldItems.CleanWorldItemName(Name);
						}
				}

				public string ContentsList {
						get {
								return GetContentsList(this);
								//			if (string.IsNullOrEmpty (mContentsList)) {
								//				mContentsList = GetContentsList (this);
								//			}
								//			return mContentsList;
						}
				}

				public string Title = string.Empty;
				public int Instances = 0;
				public bool Revealed = false;
				//pattern is a bitmask used as a first-round match
				//for now it's implemented as flags 1-9 corresponding to filled squares
				//this is applied on startup so it could be anything really
				[XmlIgnore]
				public int Pattern;
				public BlueprintRevealMethod RevealMethod = BlueprintRevealMethod.None;
				public string RevealSource = string.Empty;
				[BitMaskAttribute(typeof(BlueprintStrictness))]
				public BlueprintStrictness Strictness = BlueprintStrictness.Default;

				public void Clear(bool fillEmpty)
				{
						if (Row1 == null) {
								Row1 = new List <GenericWorldItem>(3);
								Row2 = new List <GenericWorldItem>(3);
								Row3 = new List <GenericWorldItem>(3);
						} else {
								Row1.Clear();
								Row2.Clear();
								Row3.Clear();
						}

						if (fillEmpty) {
								for (int i = 0; i < 3; i++) {
										Row1.Add(null);
										Row2.Add(null);
										Row3.Add(null);
								}
						}

						GenericResult = null;
						//CustomResult	= null;
				}

				[FrontiersAvailableModsAttribute("Skill")]
				public string RequiredSkill = "Craft";
				public bool UseGenericResult = true;

				public bool IsEmpty {
						get {
								return Name == null
								|| (UseGenericResult && (GenericResult == null || GenericResult.IsEmpty))
								|| (!UseGenericResult && string.IsNullOrEmpty(CustomResultScript));
						}
				}

				[XmlIgnore]
				public string IconName {
						get {
								if (string.IsNullOrEmpty(mIconName)) {
										Skill skill = null;
										if (Skills.Get.SkillByName(RequiredSkill, out skill)) {
												mIconName = skill.Info.IconName;
										}
								}
								return mIconName;
						}
				}

				public Color IconColor {
						get {
								return Color.white;
						}
				}

				public Color BackgroundColor {
						get {
								return Color.white;
						}
				}

				public float BaseCraftTime = 1.0f;
				public List <GenericWorldItem> Row1;
				public List <GenericWorldItem> Row2;
				public List <GenericWorldItem> Row3;
				//this is generated on startup by Blueprints along with pattern
				//i don't want to be locked in to a certain way of storing the rows
				[XmlIgnore]
				public List <GenericWorldItem> Rows;
				public GenericWorldItem GenericResult = null;
				public string CustomResultScript = string.Empty;
				public string CustomResultScriptState = string.Empty;
				protected string mIconName = string.Empty;
				//protected string mContentsList = string.Empty;
				protected static string GetContentsList(WIBlueprint blueprint)
				{
						Dictionary <string,int> contents = new Dictionary<string, int>();
						AddContentsToList(blueprint.Row1, contents);
						AddContentsToList(blueprint.Row2, contents);
						AddContentsToList(blueprint.Row3, contents);

						List <string> contentsList = new List<string>();
						foreach (KeyValuePair <string,int> contentsKey in contents) {
								contentsList.Add(" - " + contentsKey.Value.ToString() + " " + contentsKey.Key);
						}
						string contentsListString = contentsList.JoinToString("\n");
						contentsList.Clear();
						contents.Clear();
						return contentsListString;
				}

				protected static void AddContentsToList(List <GenericWorldItem> row, Dictionary <string,int> contents)
				{
						for (int i = 0; i < row.Count; i++) {
								string stackName = row[i].DisplayName.Trim();
								if (!string.IsNullOrEmpty(row[i].State) && row[i].State != "Default") {
										stackName = stackName + " (" + row[i].State + ")";
								}
								if (!string.IsNullOrEmpty(stackName)) {
										if (contents.ContainsKey(stackName)) {
												int numInList = contents[stackName];
												contents[stackName] = numInList + 1;
										} else {
												contents.Add(stackName, 1);
										}
								}
						}
				}
		}

		[Flags]
		public enum BlueprintStrictness
		{
				None = 0,
				StackName = 1,
				PrefabName = 2,
				StateName = 4,
				Subcategory = 8,
				Default = StackName,
		}

		public enum BlueprintRevealMethod
		{
				None,
				Book,
				Character,
				ReverseEngineer,
				Skill,
		}

		public enum CraftingType
		{
				Craft,
				Brew,
				Refine,
				Cook,
		}
}
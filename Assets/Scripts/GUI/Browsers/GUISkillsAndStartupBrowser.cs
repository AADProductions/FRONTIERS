using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUISkillsAndStartupBrowser : GUIBrowserSelectView <SkillStartupSetting>
		{
				public GUINewGameDialog ParentDialog;
				public GUITabPage ControllingTabPage;
				public UIButtonMessage PrevStartupPosition;
				public UIButtonMessage NextStartupPosition;
				public UIButtonMessage PrevItemsCategory;
				public UIButtonMessage NextItemsCategory;
				public UIButtonMessage PrevClothingCategory;
				public UIButtonMessage NextClothingCategory;
				public List <PlayerStartupPosition> AvailableStartupPositions = new List<PlayerStartupPosition>();
				public List <WICategory> AvailableCategories = new List<WICategory>();
				public List <WICategory> AvailableClothingCategories = new List<WICategory>();
				[HideInInspector]
				public PlayerStartupPosition CurrentStartupPosition;
				[HideInInspector]
				public WICategory CurrentClothingCategory;
				[HideInInspector]
				public WICategory CurrentItemCategory;
				public UILabel CategoryLabel;
				public UILabel CategoryDescriptionLabel;
				public UILabel ClothingLabel;
				public UILabel ClothingDescriptionLabel;
				public UILabel StartupPositionLabel;
				public UILabel StartupPositionDescriptionLabel;

				public void OnDestroy()
				{
						AvailableCategories.Clear();
						AvailableStartupPositions.Clear();
				}

				public override void WakeUp()
				{
						base.WakeUp();

						CurrentStartupPosition = null;
						CurrentClothingCategory = null;
						CurrentItemCategory = null;

						ControllingTabPage.OnSelected += Show;
						ControllingTabPage.OnDeselected += Hide;

						NextStartupPosition.target = gameObject;
						PrevStartupPosition.target = gameObject;

						NextClothingCategory.target = gameObject;
						PrevClothingCategory.target = gameObject;

						NextItemsCategory.target = gameObject;
						PrevItemsCategory.target = gameObject;

						NextItemsCategory.functionName = "OnClickNextItemsCategory";
						PrevItemsCategory.functionName = "OnClickPrevItemsCategory";
						NextClothingCategory.functionName = "OnClickNextClothingCategory";
						PrevClothingCategory.functionName = "OnClickPrevClothingCategory";
						NextStartupPosition.functionName = "OnClickNextStartupPosition";
						PrevStartupPosition.functionName = "OnClickPrevStartupPosition";
				}

				public void OnClickNextItemsCategory ( ){
						int currentIndex = AvailableCategories.IndexOf(CurrentItemCategory);
						CurrentItemCategory = AvailableCategories.NextItem(currentIndex);
						CurrentStartupPosition.InventoryFillCategory = CurrentItemCategory.Name;
						Mods.Get.Runtime.SaveMod(CurrentStartupPosition, "PlayerStartupPosition", CurrentStartupPosition.Name);
						RefreshCategory(CurrentItemCategory, CategoryLabel, CategoryDescriptionLabel);
				}

				public void OnClickPrevItemsCategory () {
						int currentIndex = AvailableCategories.IndexOf(CurrentItemCategory);
						CurrentItemCategory = AvailableCategories.PrevItem(currentIndex);
						CurrentStartupPosition.InventoryFillCategory = CurrentItemCategory.Name;
						Mods.Get.Runtime.SaveMod(CurrentStartupPosition, "PlayerStartupPosition", CurrentStartupPosition.Name);
						RefreshCategory(CurrentItemCategory, CategoryLabel, CategoryDescriptionLabel);
				}

				public void OnClickNextClothingCategory(){
						int currentIndex = AvailableClothingCategories.IndexOf(CurrentClothingCategory);
						CurrentClothingCategory = AvailableClothingCategories.NextItem(currentIndex);
						CurrentStartupPosition.WearableFillCategory = CurrentClothingCategory.Name;
						Mods.Get.Runtime.SaveMod(CurrentStartupPosition, "PlayerStartupPosition", CurrentStartupPosition.Name);
						RefreshCategory(CurrentClothingCategory, ClothingLabel, ClothingDescriptionLabel);
				}

				public void OnClickPrevClothingCategory() {
						int currentIndex = AvailableClothingCategories.IndexOf(CurrentItemCategory);
						CurrentClothingCategory = AvailableClothingCategories.PrevItem(currentIndex);
						CurrentStartupPosition.WearableFillCategory = CurrentClothingCategory.Name;
						Mods.Get.Runtime.SaveMod(CurrentStartupPosition, "PlayerStartupPosition", CurrentStartupPosition.Name);
						RefreshCategory(CurrentClothingCategory, ClothingLabel, ClothingDescriptionLabel);
				}

				public void OnClickNextStartupPosition() {
						int currentIndex = AvailableStartupPositions.IndexOf(CurrentStartupPosition);
						CurrentStartupPosition = AvailableStartupPositions.NextItem(currentIndex);
						RefreshStartupPosition();
				}

				public void OnClickPrevStartupPosition(){
						int currentIndex = AvailableStartupPositions.IndexOf(CurrentStartupPosition);
						CurrentStartupPosition = AvailableStartupPositions.PrevItem(currentIndex);
						RefreshStartupPosition();
				}

				public override IEnumerable <SkillStartupSetting> FetchItems()
				{
//						if (Profile.Get.Current.SkillStartupSettings == null) {
//								Profile.Get.CurrentGame.SkillStartupSettings = new List<SkillStartupSetting>();
//								foreach (SkillStartupSetting setting in Skills.Get.DefaultSkillStartupSettings) {
//										SkillStartupSetting settingCopy = ObjectClone.Clone <SkillStartupSetting>(setting);
//										Profile.Get.CurrentGame.SkillStartupSettings.Add(settingCopy);
//								}
//						}
						return Skills.Get.DefaultSkillStartupSettings;//Profile.Get.CurrentGame.SkillStartupSettings as IEnumerable <SkillStartupSetting>;
				}

				public override void Show()
				{
						base.Show();

						if (AvailableStartupPositions.Count == 0) {
								Mods.Get.Runtime.LoadAvailableMods(AvailableStartupPositions, "PlayerStartupPosition");
								for (int i = AvailableStartupPositions.LastIndex(); i >= 0; i--) {
										if (!AvailableStartupPositions[i].CanBeUsedForNewGame) {
												AvailableStartupPositions.RemoveAt(i);
										}
								}
						}
						if (AvailableCategories.Count == 0) {
								List <WICategory> categories = new List<WICategory>();
								Mods.Get.Runtime.LoadAvailableMods(categories, "Category");
								for (int i = 0; i < categories.Count; i++) {
										if (categories[i].StartupItemsCategory) {
												AvailableCategories.Add(categories[i]);
										}
										if (categories[i].StartupClothingCategory) {
												AvailableClothingCategories.Add(categories[i]);
										}
								}
						}

						//see if we need to set any of this stuff
						if (CurrentStartupPosition == null) {
								//get the default from the world settings
								for (int i = 0; i < AvailableStartupPositions.Count; i++) {
										if (AvailableStartupPositions[i].Name == ParentDialog.CurrentWorld.FirstStartupPosition) {
												CurrentStartupPosition = AvailableStartupPositions[i];
												break;
										}
								}
						}

						RefreshStartupPosition();

				}

				public void RefreshCategory(WICategory category, UILabel nameLabel, UILabel descriptionLabel)
				{
						if (category != null) {
								nameLabel.text = Data.GameData.AddSpacesToSentence(category.Name);
								List <string> items = new List<string>();
								for (int i = 0; i < category.GenericWorldItems.Count; i++) {
										string item = System.Text.RegularExpressions.Regex.Replace(category.GenericWorldItems[i].PrefabName, @" \d", "");
										if (category.GenericWorldItems[i].InstanceWeight > 1) {
												item = category.GenericWorldItems[i].InstanceWeight.ToString() + " " + item;
										}
										items.Add(item);
								}
								descriptionLabel.text = Data.GameData.CommaJoinWithLast(items, "and");
						} else {
								nameLabel.text = "(None)";
								descriptionLabel.text = "(No items)";
						}
				}

				public void RefreshStartupPosition()
				{
						StartupPositionLabel.text = Data.GameData.AddSpacesToSentence(CurrentStartupPosition.Name);
						if (CurrentStartupPosition.Name == ParentDialog.CurrentWorld.FirstStartupPosition) {
								StartupPositionDescriptionLabel.text = "This is the default startup position for this world. Choose this if you want to experience the story from the beginning, complete with prologue.";
								Profile.Get.CurrentGame.CustomStartupPosition = string.Empty;
						} else {
								StartupPositionDescriptionLabel.text = CurrentStartupPosition.Description;
								Profile.Get.CurrentGame.CustomStartupPosition = CurrentStartupPosition.Name;
						}

						CurrentItemCategory = null;
						for (int i = 0; i < AvailableCategories.Count; i++) {
								if (AvailableCategories[i].Name == CurrentStartupPosition.InventoryFillCategory) {
										CurrentItemCategory = AvailableCategories[i];
										break;
								}
						}

						CurrentClothingCategory = null;
						for (int i = 0; i < AvailableClothingCategories.Count; i++) {
								if (AvailableClothingCategories[i].Name == CurrentStartupPosition.WearableFillCategory) {
										CurrentClothingCategory = AvailableClothingCategories[i];
										break;
								}
						}

						RefreshCategory(CurrentItemCategory, CategoryLabel, CategoryDescriptionLabel);
						RefreshCategory(CurrentClothingCategory, ClothingLabel, ClothingDescriptionLabel);
				}

				public override bool PushToViewerAutomatically {
						get {
								return true;
						}
				}

				public override void ReceiveFromParentEditor(IEnumerable<SkillStartupSetting> editObject, ChildEditorCallback<IEnumerable<SkillStartupSetting>> callBack)
				{
						mEditObject = editObject;
						mCallBack = callBack;
						HasFocus = true;
						PushEditObjectToNGUIObject();
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(SkillStartupSetting editObject)
				{
						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = editObject.SkillName;
						GUIGenericBrowserObject skillBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();
						skillBrowserObject.AutoSelect = false;

						skillBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						skillBrowserObject.Icon.spriteName = "SkillIconCraftRepairMachine";
						skillBrowserObject.EditButton.target = this.gameObject;
						skillBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						SetBrowserObjectColors(skillBrowserObject, editObject);

						return newBrowserObject;
				}

				public override void PushSelectedObjectToViewer()
				{
						if (mBrowserObject == null) {
								return;
						}
						//this means we've clicked the object
						//change the selected object
						//then change the browser object to reflect it
						//the progression is:
						//unknown->known->learned/enabled->mastered->
						if (mSelectedObject.Mastered) {
								mSelectedObject.Mastered = false;
								mSelectedObject.KnowledgeState = SkillKnowledgeState.Unknown;
						} else {
								switch (mSelectedObject.KnowledgeState) {
										case SkillKnowledgeState.Unknown:
										default:
												mSelectedObject.KnowledgeState = SkillKnowledgeState.Known;
												break;

										case SkillKnowledgeState.Known:
												mSelectedObject.KnowledgeState = SkillKnowledgeState.Enabled;
												break;

										case SkillKnowledgeState.Enabled:
										case SkillKnowledgeState.Learned:
												mSelectedObject.Mastered = true;
												break;
								}
						}
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
						GUIGenericBrowserObject bo = mBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();
						SetBrowserObjectColors(bo, mSelectedObject);
				}

				protected void SetBrowserObjectColors(GUIGenericBrowserObject skillBrowserObject, SkillStartupSetting editObject)
				{
						if (editObject.Mastered) {
								skillBrowserObject.Name.text = editObject.SkillName + " (Mastered)";
								skillBrowserObject.Background.color = Colors.Get.SkillMasteredColor;
								skillBrowserObject.Name.color = Colors.InvertColor(Colors.Get.SkillMasteredColor);
						} else {
								switch (editObject.KnowledgeState) {
										case SkillKnowledgeState.Enabled:
										case SkillKnowledgeState.Learned:
										default:
												skillBrowserObject.Name.text = editObject.SkillName + " (" + editObject.KnowledgeState.ToString() + ")";
												skillBrowserObject.Background.color = Colors.Get.SkillLearnedColorLow;
												skillBrowserObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
												break;

										case SkillKnowledgeState.Known:
												skillBrowserObject.Name.text = editObject.SkillName + " (" + editObject.KnowledgeState.ToString() + ")";
												skillBrowserObject.Background.color = Colors.Get.SkillKnownColor;
												skillBrowserObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
												break;

										case SkillKnowledgeState.Unknown:
												skillBrowserObject.Name.text = "(Unknown)";
												skillBrowserObject.Name.color = Colors.Darken(Colors.Get.MenuButtonTextColorDefault);
												skillBrowserObject.Background.color = Colors.Get.SkillUnknownColor;
												break;

								}
						}

				}
		}
}
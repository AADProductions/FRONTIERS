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
		public class GUIBlueprintBrowser : GUIBrowserSelectView <WIBlueprint>
		{
				GUITabPage TabPage;
				public GUITabs SubSelectionTabs;
				public string BlueprintCategory;
				public string SkillDisplayName;
				public bool CreateEmptyDivider;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUIEditorID; }
						//this will get everything on all tabs
						GUILogInterface.Get.GetActiveInterfaceObjects(currentObjects, flag);
						base.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override void WakeUp()
				{
						base.WakeUp();

						TabPage = gameObject.GetComponent <GUITabPage>();
						TabPage.OnDeselected += OnDeselected;
						SubSelectionTabs = gameObject.GetComponent <GUITabs>();
						SubSelectionTabs.OnSetSelection += OnSetSelection;
				}

				public override void Start()
				{
						base.Start();
						//we're a parent of the inventory
						NGUICamera = GUIManager.Get.PrimaryCamera;
				}

				public void OnDeselected()
				{
						if (HasFocus) {
								GUIManager.Get.ReleaseFocus(this);
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						CreateEmptyDivider = true;
						base.PushEditObjectToNGUIObject();
				}

				public override IEnumerable <WIBlueprint> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						return Blueprints.Get.BlueprintsByCategory(BlueprintCategory).AsEnumerable();
				}

				public void OnSetSelection()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return;
						}

						BlueprintCategory = SubSelectionTabs.SelectedTab;
						Skill skill = null;
						if (Skills.Get.SkillByName(BlueprintCategory, out skill)) {
								SkillDisplayName = skill.DisplayName;
						} else {
								SkillDisplayName = BlueprintCategory;
						}

						ReceiveFromParentEditor(FetchItems());
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						newDivider = CreateDivider();
						dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "a_empty";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;

						if (CreateEmptyDivider) {
								dividerObject.Name.text = "You have no " + SkillDisplayName + " blueprints";
						} else {
								dividerObject.Name.text = SkillDisplayName + " blueprints:";
						}
						dividerObject.Initialize("Divider");
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(WIBlueprint editObject)
				{
						CreateEmptyDivider = false;

						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = editObject.Name + "_" + editObject.RequiredSkill;
						GUIGenericBrowserObject blueprintBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						#if UNITY_EDITOR
						if (VRManager.VRDeviceAvailable | VRManager.VRTestingModeEnabled) {
						#else
						if (VRManager.VRDeviceAvailable) {
						#endif
								newBrowserObject.AutoSelect = false;
						} else {
								newBrowserObject.AutoSelect = true;
						}

						blueprintBrowserObject.EditButton.target = this.gameObject;
						blueprintBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						string displayText = editObject.CleanName;
						string wrappedDescription = " - " + editObject.Description.Replace("\n", "");
						if (wrappedDescription.Length > 200) {
								wrappedDescription = wrappedDescription.Substring(0, 200) + "...";
						}
						displayText += Colors.ColorWrap(wrappedDescription, Colors.Darken(blueprintBrowserObject.Name.color));
						blueprintBrowserObject.Name.text = displayText;
						blueprintBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						blueprintBrowserObject.Icon.spriteName = editObject.IconName;

						blueprintBrowserObject.BackgroundHighlight.enabled = false;
						//blueprintBrowserObject.GeneralColor = editObject.SkillBorderColor;

						//blueprintBrowserObject.BackgroundHighlight.enabled = false;
						//blueprintBrowserObject.Icon.color = editObject.SkillIconColor;
						//blueprintBrowserObject.IconBackround.color = editObject.SkillBorderColor;

						blueprintBrowserObject.Initialize(editObject.Name);
						blueprintBrowserObject.Refresh();

						return newBrowserObject;
				}

				public void OnClickCraftNow()
				{
						PrimaryInterface.MaximizeInterface("Inventory", "CraftBlueprint", mSelectedObject.Name);
				}

				public override void PushSelectedObjectToViewer()
				{
						////Debug.Log ("Pushing skill" + mSelectedObject.name + " to GUIDetails page");
						List <string> detailText = new List <string>();
						detailText.Add("(" + SkillDisplayName + ")");
						detailText.Add(mSelectedObject.Description.Replace("\n", "").Replace("\r", ""));
						detailText.Add("_");
						detailText.Add("Contents:");
						detailText.Add(mSelectedObject.ContentsList);

						GUIDetailsPage.Get.DisplayDetail(
								this,
								mSelectedObject.CleanName,
								detailText.JoinToString("\n"),
								mSelectedObject.IconName,
								Mats.Get.IconsAtlas,
								Color.white,
								Color.white,
								mSelectedObject.GenericResult);
			
						//see if we can craft this right now
						GUIDetailsPage.Get.DisplayDopplegangerButton("Craft Now", "OnClickCraftNow", gameObject);
				}
		}
}
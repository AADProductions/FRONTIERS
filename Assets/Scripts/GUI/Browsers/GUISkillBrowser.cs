using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.GUI
{
		public class GUISkillBrowser : GUIBrowserSelectView <Skill>
		{
				GUITabPage TabPage;
				public GUITabs SubSelectionTabs;
				public string SkillGroup;
				public bool CreateLearnedDivider;
				public bool CreateUnlearnedDivider;
				public bool CreateEmptyDivider;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUILogInterface.Get.GUIEditorID; }
						//this will get everything on all tabs
						GUILogInterface.Get.GetActiveInterfaceObjects(currentObjects, flag);
						base.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override void WakeUp()
				{
						base.WakeUp();

						SubSelectionTabs = gameObject.GetComponent <GUITabs>();
						SubSelectionTabs.OnSetSelection += OnSetSelection;
						TabPage = gameObject.GetComponent <GUITabPage>();
						TabPage.OnDeselected += OnDeselected;
				}

				public override void Start()
				{
						base.Start();
						//we're a parent of the log
						NGUICamera = GUIManager.Get.PrimaryCamera;
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						if (CreateLearnedDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_learnedSkills";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Learned:";
								dividerObject.Initialize("Divider");
						}

						if (CreateUnlearnedDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "c_knownSkills";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Not learned:";
								dividerObject.Initialize("Divider");
						}
				}

				public void OnDeselected()
				{
						if (HasFocus) {
								GUIManager.Get.ReleaseFocus(this);
						}
				}

				public override IEnumerable <Skill> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						return Skills.Get.SkillsByGroup(SkillGroup).AsEnumerable();
				}

				public void OnSetSelection()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return;
						}

						if (Skills.MostRecentlyLearnedSkill != null && SubSelectionTabs.SelectedTab != Skills.MostRecentlyLearnedSkill.Info.SkillGroup) {
								SubSelectionTabs.SetSelection(Skills.MostRecentlyLearnedSkill.Info.SkillGroup);
								Skills.MostRecentlyLearnedSkill = null;
						}
						SkillGroup = SubSelectionTabs.SelectedTab;
						IEnumerable <Skill> skills = Skills.Get.SkillsByGroup(SkillGroup).AsEnumerable();
						ReceiveFromParentEditor(skills);
				}

				public override void PushEditObjectToNGUIObject()
				{
						CreateLearnedDivider = false;
						CreateUnlearnedDivider = false;
						CreateEmptyDivider = true;
						base.PushEditObjectToNGUIObject();
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(Skill editObject)
				{
						CreateEmptyDivider = false;

						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = editObject.Info.SkillGroup + "_" + editObject.Info.SkillSubgroup + "_" + editObject.DisplayName;
						GUISkillBrowserObject skillBrowserObject = newBrowserObject.gameObject.GetComponent <GUISkillBrowserObject>();

						#if UNITY_EDITOR
						if (VRManager.VRDeviceAvailable | VRManager.VRTestingMode) {
						#else
						if (VRManager.VRDeviceAvailable) {
						#endif
								newBrowserObject.AutoSelect = false;
						} else {
								newBrowserObject.AutoSelect = true;
						}

						Skill prereq = null;
						if (editObject.RequiresPrerequisite) {
								Skills.Get.SkillByName(editObject.Requirements.PrerequisiteSkillName, out prereq);
						}
						skillBrowserObject.SetPrereq(prereq);
						skillBrowserObject.NormalizedMasteryLevel = editObject.State.NormalizedMasteryLevel;
						skillBrowserObject.HasBeenMastered = editObject.HasBeenMastered;
						skillBrowserObject.GetPlayerAttention = editObject.GetPlayerAttention;
						skillBrowserObject.SetColors(editObject.SkillIconColor, editObject.SkillBorderColor, editObject.KnowledgeState, editObject.Info.IconName);
						skillBrowserObject.SetName(editObject.DisplayName, editObject.Info.SkillSubgroup, editObject.Info.Description);
						skillBrowserObject.EditButton.target = this.gameObject;
						skillBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						if (editObject.GetPlayerAttention) {
								newBrowserObject.name = "b_" + newBrowserObject.name;
						} else if (editObject.KnowledgeState == SkillKnowledgeState.Learned || editObject.KnowledgeState == SkillKnowledgeState.Enabled) {
								newBrowserObject.name = "b_" + newBrowserObject.name;
								CreateLearnedDivider = true;
						} else if (editObject.KnowledgeState == SkillKnowledgeState.Known) {
								newBrowserObject.name = "d_" + newBrowserObject.name;
								CreateUnlearnedDivider = true;
						}

						skillBrowserObject.Initialize(editObject.name);
						skillBrowserObject.Refresh();

						return newBrowserObject;
				}

				public override void PushSelectedObjectToViewer()
				{
						mSelectedObject.GetPlayerAttention = false;
						List <string> detailText = new List <string>();
						if (!string.IsNullOrEmpty(mSelectedObject.Info.SkillSubgroup)) {
								detailText.Add(Colors.ColorWrap("(" + mSelectedObject.Info.SkillSubgroup + ")", Colors.Dim(Colors.Get.MenuButtonTextColorDefault)));
						}
						detailText.Add(mSelectedObject.Info.Description.Replace("\n", "").Replace("\r", ""));
						if (!string.IsNullOrEmpty(mSelectedObject.Info.Instructions)) {
								detailText.Add("_");
								detailText.Add(mSelectedObject.Info.Instructions.Replace("\"", ""));
						}
						if (mSelectedObject.RequiresPrerequisite) {
								detailText.Add("_");
								detailText.Add("Prerequisite Skill: " + mSelectedObject.Requirements.PrerequisiteSkillName);
						}

						GUIDetailsPage.Get.DisplayDetail(
								this,
								mSelectedObject.DisplayName,
								detailText.JoinToString("\n"),
								mSelectedObject.Info.IconName,
								Mats.Get.IconsAtlas,
								mSelectedObject.SkillIconColor,
								mSelectedObject.SkillBorderColor);
				}
		}
}
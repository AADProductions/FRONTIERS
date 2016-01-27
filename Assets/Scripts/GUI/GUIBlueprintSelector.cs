using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIBlueprintSelector : GUIBrowserSelectView <WIBlueprint>
		{
				public GUITabPage TabPage;
				public GUITabs SubSelectionTabs;
				public string BlueprintCategory;

				public override void WakeUp()
				{
						base.WakeUp();

						TabPage = gameObject.GetComponent <GUITabPage>();
						SubSelectionTabs = gameObject.GetComponent <GUITabs>();
						if (SubSelectionTabs != null) {
								SubSelectionTabs.OnSetSelection += OnSetSelection;
						} else {
								//Debug.Log ("SUBSELECTION TABS WAS NULL IN " + name);
						}
				}

				public override void Start()
				{
						base.Start();
						//we're a parent of the inventory
						NGUICamera = GUIManager.Get.PrimaryCamera;
				}

				public void OnSetSelection()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return;
						}
						Debug.Log("Setting selection");
						BlueprintCategory = SubSelectionTabs.SelectedTab;
						ReceiveFromParentEditor(FetchItems());
				}

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						TabPage.TabParent.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override IEnumerable <WIBlueprint> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						mLastCategoryLoaded = BlueprintCategory;
						return Blueprints.Get.BlueprintsByCategory(BlueprintCategory).AsEnumerable();
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(WIBlueprint editObject)
				{
						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIGenericBrowserObject browserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						newBrowserObject.name = editObject.CleanName;

						browserObject.EditButton.target = this.gameObject;
						browserObject.EditButton.functionName = "OnClickBrowserObject";

						browserObject.Name.text = editObject.CleanName;
						string description = editObject.Description.Trim();
						if (!string.IsNullOrEmpty(description)) {
								browserObject.Name.text = browserObject.Name.text + Colors.ColorWrap(" - " + description, Colors.Darken(browserObject.Name.color));
						}
						browserObject.Icon.atlas = Mats.Get.IconsAtlas;
						browserObject.Icon.spriteName = editObject.IconName;
						browserObject.Icon.color = editObject.IconColor;
						browserObject.GeneralColor = editObject.BackgroundColor;

						browserObject.Initialize(editObject.RequiredSkill + "_" + editObject.Name);
						browserObject.Refresh();

						return newBrowserObject;
				}

				public override void OnClickBrowserObject(GameObject obj)
				{
						if (!IsOurBrowserObject(obj, mGUIEditorID)) {
								return;
						}

						IGUIBrowserObject browserObject = (IGUIBrowserObject)obj.GetComponent(typeof(IGUIBrowserObject));
						mSelectedObject = mEditObjectLookup[browserObject];
						Debug.Log("Selecting blueprint " + mSelectedObject.CleanName);
						GUIInventoryInterface.Get.CraftingInterface.OnSelectBlueprint(mSelectedObject);
				}

				protected string mLastCategoryLoaded = string.Empty;
		}
}
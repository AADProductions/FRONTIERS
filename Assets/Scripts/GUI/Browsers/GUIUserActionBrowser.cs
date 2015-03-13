using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIUserActionBrowser : GUIBrowserSelectView <ActionSetting>
		{
				public GUITabPage ControllingTabPage;
				public GameObject ApplySettingsButton;
				public UILabel ConfirmMessageLabel;

				public override IEnumerable<ActionSetting> FetchItems()
				{
						mLastUserActionSettings.Clear();
						mLastInterfaceActionSettings.Clear();
						mActionSettings.Clear();
						//copy each item so we're working with a fresh copy
						foreach (ActionSetting a in UserActionManager.Get.CurrentActionSettings) {
								mLastUserActionSettings.Add(ObjectClone.Clone <ActionSetting>(a));

						}
						UserActionManager.Get.GetAvailableBindings(mLastUserActionSettings);

						foreach (ActionSetting a in InterfaceActionManager.Get.CurrentActionSettings) {
								mLastInterfaceActionSettings.Add(ObjectClone.Clone <ActionSetting>(a));
						}
						InterfaceActionManager.Get.GetAvailableBindings(mLastInterfaceActionSettings);

						mActionSettings.AddRange(mLastInterfaceActionSettings);
						mActionSettings.AddRange(mLastUserActionSettings);
						return mActionSettings;
				}

				public override bool PushToViewerAutomatically {
						get { 
								return false;
						}
				}

				public override void ReceiveFromParentEditor(IEnumerable<ActionSetting> editObject, ChildEditorCallback<IEnumerable<ActionSetting>> callBack)
				{
						mEditObject = editObject;
						mCallBack = callBack;
						HasFocus = true;
						ConfirmMessageLabel.text = string.Empty;
						PushEditObjectToNGUIObject();
				}

				public override void WakeUp()
				{
						base.WakeUp();

						ControllingTabPage.OnSelected += Show;
						ControllingTabPage.OnDeselected += Hide;
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(ActionSetting editObject)
				{
						IGUIBrowserObject browserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIUserActionBrowserObject uabo = browserObject.gameObject.GetComponent <GUIUserActionBrowserObject>();
						uabo.name = editObject.ActionDescription;
						uabo.Setting = editObject;
						return browserObject;
				}

				public void OnClickSaveChangesButton()
				{
						UserActionManager.Get.PushSettings(mLastUserActionSettings);
						InterfaceActionManager.Get.PushSettings(mLastInterfaceActionSettings);
						ReceiveFromParentEditor(FetchItems(), null);
						Profile.Get.SaveCurrent(ProfileComponents.Preferences);
						ShowConfirmMessage();
				}

				public void OnClickResetAllButton()
				{
						UserActionManager.Get.PushSettings(UserActionManager.Get.GenerateDefaultActionSettings());
						InterfaceActionManager.Get.PushSettings(InterfaceActionManager.Get.GenerateDefaultActionSettings());
						ReceiveFromParentEditor(FetchItems(), null);
						Profile.Get.SaveCurrent(ProfileComponents.Preferences);
						ShowConfirmMessage();
				}

				public void ShowConfirmMessage()
				{
						ConfirmMessageLabel.text = "Changes saved.";
				}

				protected List <ActionSetting> mLastUserActionSettings = new List<ActionSetting>();
				protected List <ActionSetting> mLastInterfaceActionSettings = new List<ActionSetting>();
				protected List <ActionSetting> mActionSettings = new List<ActionSetting> ();
		}
}
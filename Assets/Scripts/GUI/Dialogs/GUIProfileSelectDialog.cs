using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIProfileSelectDialog : GUIEditor <PlayerProfileSelectionResult>
		{
				public UILabel MessageType;
				public UILabel Message;
				public UIButton CreateButton;
				public UIInput ProfileCreateResult;
				public bool ValidExistingSelection	= false;
				public bool ValidNewProfileCreation	= false;
				public GUIProfileBrowser ProfileBrowser;
				public GUITabs Tabs;

				public override void Start()
				{
						Tabs.Initialize(this);
						Tabs.OnSetSelection += OnSetSelection;
						ProfileBrowser.OnSelectProfileName += OnSelectProfileName;
						base.Start();

						List <string> profiles = Profile.Get.ProfileNames(false);
						if (profiles.Count == 0) {
								Tabs.SetSelection("Create");
						}
				}

				public void OnSetSelection()
				{
						if (Tabs.SelectedTab == "Create") {
								GUIManager.Get.GetFocus(this);
								//set focus to the text
								UICamera.selectedObject = ProfileCreateResult.gameObject;
						}
				}

				public override void DisableInput()
				{
						//don't change layers
						return;
				}

				public void OnSelectProfileName()
				{
						string errorMessage = string.Empty;
						if (Profile.Get.SetOrCreateProfile(ProfileBrowser.LastSelectedProfile, out errorMessage)) {
								Finish();
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						return;
				}

				public override bool ActionCancel(double timeStamp)
				{
						//can't cancel
						return true;
				}

				public void OnCreateProfileName()
				{
						RefreshSelection();
				}

				public void OnCreateProfileNameSubmit()
				{
						RefreshSelection();
				}

				public void OnSelectExistingProfile()
				{
						RefreshSelection();
				}

				public void RefreshSelection()
				{
						if (mRefreshingSelection) {
								return;
						}

						mRefreshingSelection = true;

						ValidNewProfileCreation = false;
						ValidExistingSelection	= false;

						if (Tabs.SelectedTab == "Create") {
								string createError = string.Empty;
								string cleanAlternative = string.Empty;
								if (Profile.Get.ValidateNewProfileName(ProfileCreateResult.text, out createError, out cleanAlternative)) {
										CreateButton.SendMessage("SetEnabled");
								} else {
										CreateButton.SendMessage("SetDisabled");
								}
								Message.enabled = true;
								Message.text = createError;
								ProfileCreateResult.text = cleanAlternative;
						}
						mRefreshingSelection = false;
				}

				public void OnClickCreateButton()
				{
						if (Profile.Get.CreateProfile(ProfileCreateResult.text)) {
								Finish();
						} else {
								Message.text = "Couldn't create profile";
						}		
				}

				protected bool mRefreshingSelection = false;
		}

		public class PlayerProfileSelectionResult
		{
				public string ProfileCreateResult;
				public string ProfileSelectResult;
				public List <string> ProfileSelectionOptions = new List <string>();
		}
}

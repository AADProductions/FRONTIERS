using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.GUI
{
		public class GUIProfileBrowser : GUIBrowserSelectView <string>
		{
				public string LastSelectedProfile;
				public Action OnSelectProfileName;

				public override IEnumerable <string> FetchItems()
				{
						return Profile.Get.ProfileNames(false) as IEnumerable <string>;
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(string editObject)
				{
						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = editObject;
						GUIGenericBrowserObject browserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();
						browserObject.EditButton.target = gameObject;
						browserObject.EditButton.functionName = "OnClickEditButton";

						browserObject.Name.text = editObject;
						browserObject.BackgroundHighlight.enabled = true;
						browserObject.BackgroundHighlight.alpha = 0f;
						browserObject.Background.color = Colors.Darken(Colors.Saturate(Colors.ColorFromString(editObject, 150)));
						UIButton button = browserObject.EditButton.GetComponent <UIButton>();
						button.hover = Colors.Get.GeneralHighlightColor;
						button.pressed = Colors.Get.GeneralHighlightColor;

						return newBrowserObject;
				}

				public void OnClickEditButton(GameObject editButton)
				{
						//this sends us the edit button
						//we pass that along to the base browser
						base.OnClickBrowserObject(editButton.transform.parent.gameObject);
				}

				public override bool ActionCancel(double timeStamp)
				{
						//can't cancel
						return true;
				}

				public override void DeleteSelectedObject()
				{
						if (!mDeletingProfile) {
								mDeletingProfile = true;
								StartCoroutine(DeleteProfileOverTime(mSelectedObject));
						}
				}

				public override bool PushToViewerAutomatically {
						get {
								return false;
						}
				}

				public IEnumerator DeleteProfileOverTime(string profileName)
				{
						YesNoCancelDialogResult result = new YesNoCancelDialogResult();
						result.CancelButton = false;
						result.Message = "Delete Profile";
						result.Message = "Are you SURE you want to delete the profile '" + profileName + "'? " +
						                                         "All the save games and mods associated with it will be lost. " +
						"I mean, it's your profile, so do what you like. I'm just saying.";

						GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog);
						GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
						childEditor.ReceiveFromParentEditor(result);

						while (!childEditor.IsFinished) {
								yield return null;
						}

						if (result.Result == DialogResult.Yes) {
								Profile.Get.DeleteProfile(profileName);
								//wait a tick for this to go through
								yield return null;
								ClearBrowserObjects();
								yield return null;
								PushEditObjectToNGUIObject();
						}
						mDeletingProfile = false;
						yield break;
				}

				protected bool mDeletingProfile = false;

				public override void PushSelectedObjectToViewer()
				{
						Debug.Log("PushSelectedObjectToViewer in GUIProfileBrowser");
						LastSelectedProfile = mSelectedObject;
						OnSelectProfileName.SafeInvoke();
				}
		}
}

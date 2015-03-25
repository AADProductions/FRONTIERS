using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
		public class GUIFocusOnDialog : GUIEditor<MessageCancelDialogResult>
		{
				public UICheckbox DontShowUntilNextUpdate;
				public UICheckbox NeverShowAgain;

				public override Widget FirstInterfaceObject {
						get {
								Widget w = base.FirstInterfaceObject;
								w.BoxCollider = DontShowUntilNextUpdate.GetComponent<BoxCollider>();
								return w;
						}
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (DontShowUntilNextUpdate.isChecked) {
								Profile.Get.CurrentPreferences.HideDialogs.Add(GameManager.FocusOnSubject);
						}
						if (NeverShowAgain.isChecked) {
								Profile.Get.CurrentPreferences.HideDialogs.Add("HideAllFocusUpdates");
						}
						//save our checkbox prefs
						Profile.Get.SaveCurrent(ProfileComponents.Preferences);
						return base.ActionCancel(timeStamp);
				}

				public override void PushEditObjectToNGUIObject()
				{
						return;
				}

				public void OnClickCancelButton()
				{
						ActionCancel(WorldClock.AdjustedRealTime);
				}
		}
}
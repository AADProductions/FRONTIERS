using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIYesNoCancelDialog : GUIEditor <YesNoCancelDialogResult>
		{
				public UILabel MessageType;
				public UILabel Message;
				public UIButton CancelButton;
				public UIButton YesButton;
				public UIButton NoButton;
				public UICheckbox DoNotShowCheckbox;
				public Vector3 CheckboxCancelOn;
				public Vector3 CheckboxCancelOff;

				public override void PushEditObjectToNGUIObject()
				{		
						MessageType.text = EditObject.MessageType;
						Message.text = EditObject.Message;
						DoNotShowCheckbox.isChecked = false;

						if (EditObject.CancelButton) {
								NGUITools.SetActive(CancelButton.gameObject, true);
								DoNotShowCheckbox.transform.localPosition = CheckboxCancelOn;
						} else {
								NGUITools.SetActive(CancelButton.gameObject, false);
								DoNotShowCheckbox.transform.localPosition = CheckboxCancelOff;
						}
						NGUITools.SetActive(DoNotShowCheckbox.gameObject, EditObject.DontShowInFutureCheckbox);
				}

				public virtual void OnClickDoNotShowCheckbox()
				{
						EditObject.DontShowInFutureCheckbox = DoNotShowCheckbox.isChecked;
				}

				public virtual void OnClickYesButton()
				{
						EditObject.Result = DialogResult.Yes;
						Finish();
				}

				public virtual void OnClickNoButton()
				{
						EditObject.Result = DialogResult.No;		
						Finish();
				}

				public virtual void OnClickCancelButton()
				{
						EditObject.Result = DialogResult.Cancel;		
						Finish();
				}

				protected override void OnFinish()
				{
						if (EditObject.DontShowInFutureCheckbox && !string.IsNullOrEmpty(EditObject.DialogName) && DoNotShowCheckbox.isChecked) {
								Profile.Get.CurrentPreferences.HideDialogs.Add(EditObject.DialogName);
								Profile.Get.SaveCurrent(ProfileComponents.Profile);
						}
						base.OnFinish();
				}
		}

		public class YesNoCancelDialogResult
		{
				public string MessageType = "Question";
				public string Message = "Dialog Question";
				public DialogResult Result = DialogResult.None;
				public bool CancelButton = true;
				public string DialogName = string.Empty;
				public bool DontShowInFutureCheckbox = false;
		}
}
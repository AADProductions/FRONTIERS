using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.Data;

namespace Frontiers.GUI
{
		public class GUIStringDialog : GUIEditor <StringDialogResult>
		{
				public UILabel MessageType;
				public UILabel Message;
				public UIButton OKButton;
				public UIInput Result;

				public override void PushEditObjectToNGUIObject()
				{
						MessageType.text = EditObject.MessageType;
						Message.text = EditObject.Message;
						Result.text = EditObject.Result;

						Message.text = mEditObject.Message.Replace("{Result}", Result.text);
						//make sure to give the input focus
						UICamera.selectedObject = Result.gameObject;
				}

				public void OnSubmit()
				{
						if (mSubmitting) {
								return;
						}
						mSubmitting = true;
						Result.text = GameData.RemoveIllegalCharacters(Result.text);
						Message.text = mEditObject.Message.Replace("{Result}", Result.text);
						mSubmitting = false;
				}

				public void OnSubmitWithEnter()
				{
						OnSubmit();
						EditObject.Result = Result.text;
						if (!mEditObject.AllowEmptyResult && string.IsNullOrEmpty(EditObject.Result)) {
								Message.text = "You didn't enter anything.";
								return;
						} else {
								Finish();
						}
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (!mEditObject.AllowEmptyResult && string.IsNullOrEmpty(EditObject.Result)) {
								Message.text = "You didn't enter anything.";
								return true;
						} else {
								mEditObject.Cancelled = true;
								return base.ActionCancel(timeStamp);
						}
						return true;
				}

				public virtual void OnClickFinishedButton()
				{
						if (!mEditObject.AllowEmptyResult && string.IsNullOrEmpty(EditObject.Result)) {
								Message.text = "You didn't enter anything.";
								return;
						}
						EditObject.Result = Result.text;			
						Finish();
				}

				protected bool mSubmitting;
		}

		public class StringDialogResult
		{
				public string Message;
				public string MessageType;
				public string Result;
				public bool Cancelled = false;
				public bool AllowEmptyResult = false;
		}
}
using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
		public class GUIMessageCancelDialog : GUIEditor<MessageCancelDialogResult>
		{
				public GameObject CancelButton;
				public UILabel CancelButtonLabel;
				public UILabel MessageLabel;

				public override bool ActionCancel(double timeStamp)
				{
						mEditObject.Cancelled = true;
						return base.ActionCancel(timeStamp);
				}

				public override void PushEditObjectToNGUIObject()
				{
						MessageLabel.text = EditObject.Message;
						CancelButtonLabel.text = EditObject.CancelButton;
						if (!EditObject.CanCancel) {
								CancelButton.SetActive(false);
						}
				}

				public void OnClickCancelButton()
				{
						ActionCancel(WorldClock.AdjustedRealTime);
				}
		}

		[Serializable]
		public class MessageCancelDialogResult : GenericDialogResult
		{
				public string Message;
				public string CancelButton;
				public bool CanCancel;
		}
}
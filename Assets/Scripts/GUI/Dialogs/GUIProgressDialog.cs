using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIProgressDialog : GUIEditor <IProgressDialogObject>
		{
				public GameObject MessageObject;
				public UILabel ProgressMessage;
				public UILabel ProgressObjectName;
				public UISlicedSprite GlowSprite;
				public UISlicedSprite ProgressBarForeground;
				public UISlider ProgressBar;
				public GameObject IconObject;
				public UISprite IconSprite;

				public override void PushEditObjectToNGUIObject()
				{
						ProgressObjectName.text = EditObject.ProgressObjectName;
						ProgressMessage.text = EditObject.ProgressMessage;
						ProgressBar.sliderValue = EditObject.ProgressValue;
				}

				public override bool ActionCancel(double timeStamp)
				{
						EditObject.ProgressCanceled = true;
						return base.ActionCancel(timeStamp);
				}

				public void Update()
				{		
						if (mFinished || EditObject == null) {
								return;
						}

						if (EditObject.ProgressFinished) {
								Finish();
						} else if (EditObject.ProgressCanceled) {
								Finish();
						}

						IconSprite.spriteName = EditObject.ProgressIconName;
						ProgressMessage.text = EditObject.ProgressMessage;
						ProgressBar.sliderValue = Mathf.Lerp(ProgressBar.sliderValue, EditObject.ProgressValue, 0.5f);
						GlowSprite.alpha = Mathf.PingPong(Time.realtimeSinceStartup, 1.5f) * 0.35f;
				}
		}
}
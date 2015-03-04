using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.GUI
{
		public class GUIButtonHover : MonoBehaviour
		{
				public bool ShowCursorOnHover = true;
				public Action OnButtonHover;

				public bool ShowCursor {
						get {
								return ShowCursorOnHover;
						}
				}

				void OnHover(bool isOver)
				{
						if (mDisabled)
								return;

						if (isOver) {
								GUIManager.Get.ActiveButton = this;
								OnButtonHover.SafeInvoke();
						} else {
								if (GUIManager.Get.ActiveButton == this) {
										GUIManager.Get.ActiveButton = null;
								}
						}

						if (!mIsPressed && isOver && Time.time > mNextHover) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseOver");
						}
				}

				void OnPress(bool isPressed)
				{
						mIsPressed = isPressed;
						mNextHover = Time.time + 2.0f;
				}

				void OnClick()
				{
						if (!mDisabled) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
						} else {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickDisabled");
						}
				}

				protected bool mDisabled = false;
				protected bool mIsPressed = false;
				protected float mNextHover = 0.0f;
		}
}
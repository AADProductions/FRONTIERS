using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIButtonSounds : MonoBehaviour
		{
				void OnHover(bool isOver)
				{
						if (!mDisabled && !mIsPressed && isOver && Time.time > mNextHover) {
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
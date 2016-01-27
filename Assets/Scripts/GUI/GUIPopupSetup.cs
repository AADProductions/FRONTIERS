using UnityEngine;
using System.Collections;
using Frontiers;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUIPopupSetup : MonoBehaviour
		{
				public bool DisableOnStartup = false;

				public void RefreshColors()
				{
						UIButton button = gameObject.GetComponent <UIButton>();
						if (button.enabled) {
								LightenButton();
						} else {
								DarkenButton();
						}
				}

				public void DarkenButton()
				{
						Transform background = transform.FindChild("SlicedSprite");
						if (background != null) {
								UISprite backgroundSprite = background.gameObject.GetComponent <UISprite>();
								backgroundSprite.color = Colors.Darken(Colors.Get.PopupListForegroundColor);
						} else {
								return;
						}
			
						UIButton button = gameObject.GetComponent <UIButton>();
						button.pressed = Colors.Darken(Colors.Get.PopupListBackgroundColor);
						button.hover = Colors.Darken(Colors.Get.GeneralHighlightColor);
			
						UILabel label = transform.FindChild("Label").GetComponent<UILabel>();
						label.color = Colors.Get.MenuButtonTextColorDefault;
						label.effectStyle = UILabel.Effect.Shadow;
						label.effectColor = Colors.Get.MenuButtonTextOutlineColor;

						UIPopupList popupList = gameObject.GetComponent <UIPopupList>();
						//popupList.textScale = label.font.size;
						popupList.textColor = Colors.Darken(Colors.Get.MenuButtonTextColorDefault);
						popupList.backgroundColor = Colors.Darken(Colors.Get.GeneralHighlightColor);
						popupList.highlightColor = Colors.Darken(Colors.Get.GeneralHighlightColor);
				}

				public void LightenButton()
				{
						Transform background = transform.FindChild("SlicedSprite");
						if (background != null) {
								UISprite backgroundSprite = background.gameObject.GetComponent <UISprite>();
								backgroundSprite.color = Colors.Get.MenuButtonBackgroundColorDefault;
						} else {
								return;
						}
			
						UIButton button = gameObject.GetComponent <UIButton>();
						button.pressed = Colors.Get.PopupListBackgroundColor;
						button.hover = Colors.Get.GeneralHighlightColor;

						UILabel label = transform.FindChild("Label").GetComponent<UILabel>();
						label.color = Colors.Get.MenuButtonTextColorDefault;
						label.effectColor = Colors.Get.MenuButtonTextOutlineColor;
			
						UIPopupList popupList = gameObject.GetComponent <UIPopupList>();
						//popupList.textScale = label.font.size;
						popupList.textColor = Colors.Get.MenuButtonTextColorDefault;
						popupList.backgroundColor = Colors.Get.GeneralHighlightColor;
						popupList.highlightColor = Colors.Get.GeneralHighlightColor;
				}

				public void	SetDisabled()
				{		
						DarkenButton();
						UIPopupList popupList = gameObject.GetComponent <UIPopupList>();
						UIButton button = gameObject.GetComponent <UIButton>();
						if (button == null) {
								return;
						}
						button.enabled = false;
						popupList.enabled = false;
						gameObject.collider.enabled	= false;
						mStartupSet = true;
				}

				public void SetEnabled()
				{
						LightenButton();
						UIPopupList popupList = gameObject.GetComponent <UIPopupList>();
						UIButton	button = gameObject.GetComponent <UIButton>();
						if (button == null) {
								return;
						}		
						button.enabled = true;
						popupList.enabled = true;
						gameObject.collider.enabled	= true;
						mStartupSet = true;
				}

				public void Start()
				{
						if (!mStartupSet) {
								if (DisableOnStartup) {
										SetDisabled();
								} else {
										SetEnabled();
								}
						}
				}

				void OnHover(bool isOver)
				{
						mIsOver = isOver;
						if (!mDisabled && !mIsPressed && mIsOver && WorldClock.RealTime > mNextHover) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseOver");
						}
				}

				void OnSelectionChange()
				{
						if (!mDisabled) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
						}
				}

				void OnPress(bool isPressed)
				{
						mIsPressed = isPressed;
						mNextHover = (float)WorldClock.RealTime + 2.0f;
				}

				void OnClick()
				{
						if (!mDisabled) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
						} else {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickDisabled");
						}
				}

				protected bool mStartupSet = false;
				protected bool mDisabled = false;
				protected bool mIsPressed = false;
				protected bool mIsOver = false;
				protected float mNextHover = 0.0f;
		}
}
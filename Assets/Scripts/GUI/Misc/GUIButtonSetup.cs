using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.GUI
{
		public class GUIButtonSetup : MonoBehaviour
		{
				public bool DisableOnStartup = false;
				public bool HandleButtonSetup = true;
				public string BackgroundName = "Background";
				public string OverlayName = "Overlay";
				public string SelectionName = "Selection";
				public string LabelName = "Label";
				public float ButtonAlpha = 0.0f;

				public void EnableButton(bool enabled)
				{
						if (enabled) {
								SetEnabled();
						} else {
								SetDisabled();
						}
				}

				public void RefreshColors()
				{
						if (enabled) {
								SetEnabled();
						} else {
								SetDisabled();
						}
				}

				public void DarkenButton()
				{
						if (HandleButtonSetup) {	
								SetColor(mBackground, Colors.Darken(Colors.Get.MenuButtonBackgroundColorDefault));
								SetColor(mOverlay, Colors.Darken(Colors.Get.MenuButtonOverlayColorDefault));
								SetColor(mSelection, Colors.Darken(Colors.Get.GeneralHighlightColor, 0f));
								SetLabel(mLabel, Colors.Darken(Colors.Get.MenuButtonTextColorDefault),
										Colors.Get.MenuButtonTextOutlineColor,
										UILabel.Effect.Shadow);//UILabel.Effect.Outline);
						}
				}

				public void LightenButton()
				{
						if (HandleButtonSetup) {			
								SetColor(mBackground, Colors.Get.MenuButtonBackgroundColorDefault);
								SetColor(mOverlay, Colors.Get.MenuButtonOverlayColorDefault);
								SetColor(mSelection, Colors.Get.GeneralHighlightColor);
								SetLabel(mLabel, Colors.Get.MenuButtonTextColorDefault,
										Colors.Get.MenuButtonTextOutlineColor,
										UILabel.Effect.Shadow);//UILabel.Effect.Outline);
						}
				}

				public void	SetDisabled()
				{
						////Debug.Log ("Setting disabled button " + name);
						if (HandleButtonSetup) {
								SetColor(mBackground, Colors.Disabled(Colors.Get.MenuButtonBackgroundColorDefault));
								SetColor(mOverlay, Colors.Disabled(Colors.Get.MenuButtonOverlayColorDefault));
								SetColor(mSelection, Colors.Darken(Colors.Get.GeneralHighlightColor, 0f));
								SetLabel(mLabel, Colors.Disabled(Colors.Get.MenuButtonTextColorDefault),
										Colors.Get.MenuButtonTextOutlineColor,
										UILabel.Effect.Shadow);//UILabel.Effect.Outline);
				
								SetScale(mScale, false,
										Vector3.one,
										Vector3.one,
										0.1f);
								SetMessage(mMessage, false);
								SetButton(mButton, false,
										mTweenTarget,
										Colors.Get.GeneralHighlightColor,
										Colors.Alpha(Colors.Get.MenuButtonBackgroundColorDefault, ButtonAlpha),
										//Colors.Darken (Colors.Get.GeneralHighlightColor, ButtonAlpha),
										0.1f);
						}
						mStartupSet = true;
						mDisabled = true;
				}

				public void SetEnabled()
				{
						if (HandleButtonSetup) {	
				
								SetColor(mBackground, Colors.Get.MenuButtonBackgroundColorDefault);
								SetColor(mOverlay, Colors.Get.MenuButtonOverlayColorDefault);
								SetColor(mSelection, Colors.Darken(Colors.Get.GeneralHighlightColor, 0f));
								SetLabel(mLabel, Colors.Get.MenuButtonTextColorDefault,
										Colors.Get.MenuButtonTextOutlineColor,
										UILabel.Effect.Shadow);//UILabel.Effect.Outline);
												
								SetScale(mScale, true,
										Vector3.one * 1.035f,
										Vector3.one,
										0.1f);
								SetMessage(mMessage, true);
								SetButton(mButton,	true,
										mTweenTarget,
										Colors.Get.GeneralHighlightColor,
										Colors.Alpha(Colors.Get.MenuButtonBackgroundColorDefault, ButtonAlpha),
								//Colors.Darken (Colors.Get.GeneralHighlightColor, ButtonAlpha),
										0.1f);
						}
						mStartupSet = true;
						mDisabled = false;
				}

				public void Awake()
				{
						if (!string.IsNullOrEmpty(BackgroundName)) {
								mBackground = transform.FindChild(BackgroundName).GetComponent <UISprite>();
						} else {
								mBackground = gameObject.GetComponent <UISprite>();
						}

						if (!string.IsNullOrEmpty(OverlayName)) {
								mOverlay = transform.FindChild(OverlayName).GetComponent <UISprite>();
						}

						if (!string.IsNullOrEmpty(SelectionName)) {
								mSelection = transform.FindChild(SelectionName).GetComponent <UISprite>();
						}

						if (!string.IsNullOrEmpty(LabelName)) {
								mLabel = transform.FindChild(LabelName).GetComponent <UILabel>();
								mLabel.transform.localScale = Vector3.one * (Mathf.Max(mLabel.transform.localScale.x, 24f));
						}

						mButton = gameObject.GetComponent <UIButton>();
						mMessage = gameObject.GetComponent <UIButtonMessage>();
						mScale = gameObject.GetComponent <UIButtonScale>();

						if (mSelection != null) {
								mTweenTarget = mSelection.gameObject;
						} else if (mBackground != null) {
								mTweenTarget = mBackground.gameObject;
						} else {
								mTweenTarget = gameObject;
						}		
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
						if (!mDisabled && !mIsPressed && isOver && WorldClock.RealTime > mNextHover) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseOver");
						}
				}

				void OnPress(bool isPressed)
				{
						mIsPressed = isPressed;
						mNextHover = WorldClock.RealTime + 2.0f;
				}

				void OnClick()
				{
						if (!mDisabled) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickEnabled");
						} else {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonClickDisabled");
						}
				}

				protected static void SetColor(UISprite sprite, Color color)
				{
						if (sprite != null) {
								sprite.color = color;
						}
				}

				protected static void SetLabel(UILabel label, Color color, Color effectColor, UILabel.Effect effect)
				{
						if (label != null) {
								label.color = color;
								label.effectStyle = UILabel.Effect.Shadow;
						}
				}

				protected static void SetScale(UIButtonScale scale, bool scaleEnabled, Vector3 hover, Vector3 pressed, float duration)
				{
						if (scale != null) {
								scale.enabled = scaleEnabled;
								scale.hover = hover;
								scale.pressed	= pressed;
								scale.duration	= duration;
						}
				}

				protected static void SetMessage(UIButtonMessage message, bool messageEnabled)
				{
						if (message != null) {
								message.enabled = messageEnabled;
						}
				}

				protected static void SetButton(UIButton button, bool buttonEnabled, GameObject tweenTarget, Color hover, Color defaultColor, float duration)
				{
						if (button != null) {
								button.enabled = buttonEnabled;
								button.tweenTarget = tweenTarget;
								button.hover = hover;
								button.defaultColor = defaultColor;
								button.duration = duration;
								if (button.GetComponent<Collider>() != null) {
										button.GetComponent<Collider>().enabled = buttonEnabled;
								}
						}
				}

				protected static void SetButton(UIButton button, bool buttonEnabled)
				{
						if (button != null) {
								button.enabled = buttonEnabled;
						}
				}

				protected bool mStartupSet = false;
				protected bool mDisabled = false;
				protected bool mIsPressed = false;
				protected double mNextHover = 0.0f;
				protected UISprite mBackground;
				protected UISprite mOverlay;
				protected UISprite mSelection;
				protected UILabel mLabel;
				protected UIButton mButton;
				protected UIButtonScale mScale;
				protected UIButtonMessage mMessage;
				protected GameObject mTweenTarget;
		}
}
using UnityEngine;
using System.Collections;
using Frontiers;
using InControl;

namespace Frontiers.GUI
{
		public class GUIHudMiniAction : MonoBehaviour
		{
				public void Awake() {
						KeystrokeLabel.useDefaultLabelFont = false;
				}

				public void Reset()
				{
						DescriptionLabel.enabled = false;
						KeystrokeSprite.enabled = false;
						KeystrokeLabel.enabled = false;
						ControllerButtonSprite.enabled = false;
						ControllerArrowsHorizontal.enabled = false;
						ControllerArrowsVertical.enabled = false;
						ControllerDPadSprite.enabled = false;
						ControllerDPadUp.enabled = false;
						ControllerDPadDown.enabled = false;
						ControllerDPadLeft.enabled = false;
						ControllerDPadRight.enabled = false;
						MouseIconBase.enabled = false;
						MouseIconLeftButton.enabled = false;
						MouseIconRightButton.enabled = false;
						MouseIconMiddleButton.enabled = false;
						LStickSprite.enabled = false;
						RStickSprite.enabled = false;
						LTriggerSprite.enabled = false;
						RTriggerSprite.enabled = false;
						LBumperSprite.enabled = false;
						RBumperSprite.enabled = false;
						StartButtonSprite.enabled = false;
						BackButtonSprite.enabled = false;
				}

				public void SetAlpha(float alpha)
				{
						DescriptionLabel.alpha = alpha;
						KeystrokeSprite.alpha = alpha;
						KeystrokeLabel.alpha = alpha;
						ControllerButtonSprite.alpha = alpha;
						ControllerArrowsHorizontal.alpha = alpha;
						ControllerArrowsVertical.alpha = alpha;
						ControllerDPadSprite.alpha = alpha;
						ControllerDPadUp.alpha = alpha;
						ControllerDPadDown.alpha = alpha;
						ControllerDPadLeft.alpha = alpha;
						ControllerDPadRight.alpha = alpha;
						MouseIconBase.alpha = alpha;
						MouseIconLeftButton.alpha = alpha;
						MouseIconRightButton.alpha = alpha;
						MouseIconMiddleButton.alpha = alpha;
						LStickSprite.alpha = alpha;
						RStickSprite.alpha = alpha;
						LTriggerSprite.alpha = alpha;
						RTriggerSprite.alpha = alpha;
						LBumperSprite.alpha = alpha;
						RBumperSprite.alpha = alpha;
						StartButtonSprite.alpha = alpha;
						BackButtonSprite.alpha = alpha;
				}

				public bool SetKey(KeyCode key, string description)
				{
						//Debug.Log("Set key " + key.ToString() + " in " + name);

						Reset();

						DescriptionLabel.enabled = true;
						DescriptionLabel.text = description;

						KeystrokeLabel.enabled = true;
						KeystrokeSprite.enabled = true;

						KeystrokeLabel.transform.localPosition = KeystrokeLabelPosition;

						bool wideFormat;
						string text = string.Empty;
						InterfaceActionManager.GetKeyCodeLabelText(key, false, out text, out wideFormat);
						if (wideFormat) {
								KeystrokeSprite.transform.localScale = WideKeystrokeScale;
						} else {
								KeystrokeSprite.transform.localScale = ThinKeystrokeScale;
						}
						KeystrokeLabel.text = text;

						return true;
				}

				public bool SetMouse(ActionSetting.MouseAction mouse, string description)
				{
						//Debug.Log("Set mouse: " + mouse.ToString() + " in " + name);

						Reset();

						DescriptionLabel.enabled = true;
						DescriptionLabel.text = description;

						bool result = true;

						MouseIconBase.enabled = true;
						switch (mouse) {
								case ActionSetting.MouseAction.Left:
										MouseIconLeftButton.enabled = true;
										break;

								case ActionSetting.MouseAction.Right:
										MouseIconRightButton.enabled = true;
										break;

								case ActionSetting.MouseAction.Middle:
										MouseIconMiddleButton.enabled = true;
										break;

								default:
										result = false;
										break;
						}

						return result;
				}

				public bool SetControl(InputControlType control, string description, string spriteSuffix)
				{
						//Debug.Log("Set control: " + control.ToString() + " in " + name);
						Reset();

						bool result = true;

						switch (control) {
								case InputControlType.Action1:
								case InputControlType.Action2:
								case InputControlType.Action3:
								case InputControlType.Action4:
										ControllerButtonSprite.enabled = true;
										ControllerButtonSprite.spriteName = control.ToString() + "_" + spriteSuffix;
										break;

								case InputControlType.LeftStickButton:
										LStickSprite.enabled = true;
										break;

								case InputControlType.RightStickButton:
										RStickSprite.enabled = true;
										break;

								case InputControlType.LeftBumper:
										LBumperSprite.enabled = true;
										break;

								case InputControlType.RightBumper:
										RBumperSprite.enabled = true;
										break;

								case InputControlType.LeftTrigger:
										LTriggerSprite.enabled = true;
										break;

								case InputControlType.RightTrigger:
										RTriggerSprite.enabled = true;
										break;

								case InputControlType.DPadLeft:
										ControllerDPadSprite.enabled = true;
										ControllerDPadLeft.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadRight.enabled = true;
										}
										break;

								case InputControlType.DPadRight:
										ControllerDPadSprite.enabled = true;
										ControllerDPadRight.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadLeft.enabled = true;
										}
										break;

								case InputControlType.DPadUp:
										ControllerDPadSprite.enabled = true;
										ControllerDPadUp.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadDown.enabled = true;
										}
										break;

								case InputControlType.DPadDown:
										ControllerDPadSprite.enabled = true;
										ControllerDPadDown.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadUp.enabled = true;
										}
										break;

								case InputControlType.DPadX:
										ControllerDPadSprite.enabled = true;
										ControllerDPadLeft.enabled = true;
										ControllerDPadRight.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadUp.enabled = true;
												ControllerDPadDown.enabled = true;
										}
										break;

								case InputControlType.DPadY:
										ControllerDPadSprite.enabled = true;
										ControllerDPadUp.enabled = true;
										ControllerDPadDown.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerDPadLeft.enabled = true;
												ControllerDPadRight.enabled = true;
										}
										break;

								case InputControlType.LeftStickX:
										LStickSprite.enabled = true;
										ControllerArrowsHorizontal.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerArrowsVertical.enabled = true;
										}
										break;

								case InputControlType.LeftStickY:
										LStickSprite.enabled = true;
										ControllerArrowsVertical.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerArrowsHorizontal.enabled = true;
										}
										break;

								case InputControlType.RightStickX:
										RStickSprite.enabled = true;
										ControllerArrowsHorizontal.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerArrowsVertical.enabled = true;
										}
										break;

								case InputControlType.RightStickY:
										RStickSprite.enabled = true;
										ControllerArrowsVertical.enabled = true;
										if (IncludeOpposingAxisByDefault) {
												ControllerArrowsHorizontal.enabled = true;
										}
										break;

								case InputControlType.Start:
										StartButtonSprite.enabled = true;
										break;

								case InputControlType.Back:
										BackButtonSprite.enabled = true;
										break;

								default:
										Debug.Log("Couldn't show action, trued to show " + control.ToString());
										result = false;
										break;
						}

						DescriptionLabel.enabled = true;
						DescriptionLabel.text = description;

						return result;
				}

				public bool IncludeOpposingAxisByDefault = false;
				public UILabel DescriptionLabel;
				public UISprite KeystrokeSprite;
				public UILabel KeystrokeLabel;
				public UISprite ControllerButtonSprite;
				public UISprite ControllerArrowsHorizontal;
				public UISprite ControllerArrowsVertical;
				public UISprite ControllerDPadSprite;
				public UISprite ControllerDPadUp;
				public UISprite ControllerDPadDown;
				public UISprite ControllerDPadLeft;
				public UISprite ControllerDPadRight;
				public UISprite MouseIconBase;
				public UISprite MouseIconLeftButton;
				public UISprite MouseIconRightButton;
				public UISprite MouseIconMiddleButton;
				public UISprite LStickSprite;
				public UISprite RStickSprite;
				public UISprite LTriggerSprite;
				public UISprite RTriggerSprite;
				public UISprite LBumperSprite;
				public UISprite RBumperSprite;
				public UISprite StartButtonSprite;
				public UISprite BackButtonSprite;
				public static Vector3 ThinKeystrokeScale = new Vector3(25f, 25f, 1f);
				public static Vector3 WideKeystrokeScale = new Vector3(50f, 25f, 1f);
				public static Vector3 KeystrokeLabelPosition = new Vector3(-27f, 18f, -20f);
		}
}
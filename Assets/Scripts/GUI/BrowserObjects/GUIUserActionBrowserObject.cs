using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;
using System;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUIUserActionBrowserObject : GUIGenericBrowserObject
		{
				public ActionSetting Setting {
						get {
								return mSetting;
						}
						set {
								mSetting = value;
								Refresh();
						}
				}

				public UILabel ActionName;
				public UILabel KeyBinding;
				public UILabel KeyAxis1Binding;
				public UILabel KeyAxis2Binding;
				public UILabel ControllerBinding;
				public UILabel MouseBinding;
				public GameObject MainButton;
				public GameObject OKButton;
				public GameObject ChangeKeyButton;
				public GameObject ChangeKeyAxis1Button;
				public GameObject ChangeKeyAxis2Button;
				public GameObject ChangeControllerButton;
				public GameObject ChangeMouseButton;
				public UIInputRaw KeyInput;
				public UIInputRaw KeyAxisInput1;
				public UIInputRaw KeyAxisInput2;

				public void Start()
				{
						ResetState();
						enabled = false;
				}

				public void Update()
				{
						if (KeyAxisInput1.selected || KeyAxisInput2.selected || KeyInput.selected) {
								return;
						}

						UserActionManager.Suspended = false;
						InterfaceActionManager.Suspended = false;
						enabled = false;
				}

				public void OnClickChangeKeyAxis1Button()
				{
						UserActionManager.Suspended = true;
						InterfaceActionManager.Suspended = true;
						enabled = true;

						//clear this so we can have a 'none' option
						Setting.KeyX = KeyCode.None;

						OKButton.SetActive(true);
						MainButton.SetActive(false);
						ChangeKeyAxis1Button.SetActive(false);
						ChangeKeyAxis2Button.SetActive(true);

						KeyAxisInput1.gameObject.SetActive(true);
						KeyAxisInput2.gameObject.SetActive(false);

						KeyAxisInput1.text = string.Empty;
						KeyAxisInput1.label.text = string.Empty;//Setting.KeyX.ToString();
						UICamera.selectedObject = KeyAxisInput1.gameObject;

						if (gActiveObject != null && gActiveObject != this) {
								gActiveObject.OnClickOKButton();
						}

						gActiveObject = this;
				}

				public void OnClickChangeKeyAxis2Button()
				{
						UserActionManager.Suspended = true;
						InterfaceActionManager.Suspended = true;
						enabled = true;

						//clear this so we can have a 'none' option
						Setting.KeyY = KeyCode.None;

						OKButton.SetActive(true);
						MainButton.SetActive(false);
						ChangeKeyAxis1Button.SetActive(true);
						ChangeKeyAxis2Button.SetActive(false);

						KeyAxisInput1.gameObject.SetActive(false);
						KeyAxisInput2.gameObject.SetActive(true);

						KeyAxisInput2.text = string.Empty;
						KeyAxisInput2.label.text = string.Empty;//Setting.KeyX.ToString();
						UICamera.selectedObject = KeyAxisInput2.gameObject;

						if (gActiveObject != null && gActiveObject != this) {
								gActiveObject.OnClickOKButton();
						}

						gActiveObject = this;
				}

				public void OnClickChangeKeyButton()
				{
						//we need to be able to intercept ALL keystrokes
						//so suspend until we're done
						UserActionManager.Suspended = true;
						InterfaceActionManager.Suspended = true;
						enabled = true;

						Setting.Key = KeyCode.None;

						OKButton.SetActive(true);
						MainButton.SetActive(false);
						ChangeKeyButton.SetActive(false);

						KeyInput.gameObject.SetActive(true);
						KeyInput.text = string.Empty;
						KeyInput.label.text = string.Empty;//Setting.Key.ToString();
						UICamera.selectedObject = KeyInput.gameObject;

						if (gActiveObject != null && gActiveObject != this) {
								gActiveObject.OnClickOKButton();
						}

						gActiveObject = this;
				}

				public void OnClickChangeControllerButton()
				{
						int currentItem = Setting.AvailableControllerButtons.IndexOf(Setting.Controller);
						Setting.Controller = Setting.AvailableControllerButtons.NextItem(currentItem);
						Refresh();
				}

				public void OnClickOKButton()
				{
						if (gActiveObject == this) {
								gActiveObject = null;
								Refresh();
						}
						ResetState();
				}

				public void OnSubmitKey()
				{
						if (Setting.AxisSetting) {
								if (KeyAxisInput1.selected) {
										if (!string.IsNullOrEmpty(KeyAxisInput1.text)) {
												KeyCode keyX = (KeyCode)System.Enum.Parse(typeof(KeyCode), KeyAxisInput1.text);
												if (Setting.AvailableKeys.Contains(keyX)) {
														Setting.KeyX = keyX;
														//move selection away from the key immediately
														KeyAxisInput1.selected = false;
														UICamera.selectedObject = gameObject;
														OnClickOKButton();
												} else {
														KeyAxisInput1.text = string.Empty;
														KeyAxisInput1.label.text = string.Empty;
												}
										}
								}
								if (KeyAxisInput2.selected) {
										if (!string.IsNullOrEmpty(KeyAxisInput2.text)) {
												KeyCode keyY = (KeyCode)System.Enum.Parse(typeof(KeyCode), KeyAxisInput2.text);
												if (Setting.AvailableKeys.Contains(keyY)) {
														Setting.KeyY = keyY;
														//move selection away from the key immediately
														KeyAxisInput2.selected = false;
														UICamera.selectedObject = gameObject;
														OnClickOKButton();
												} else {
														KeyAxisInput2.text = string.Empty;
														KeyAxisInput2.label.text = string.Empty;
												}
										}
								}
						} else {
								if (KeyInput.selected) {
										if (!string.IsNullOrEmpty(KeyInput.text)) {
												KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), KeyInput.text);
												if (Setting.AvailableKeys.Contains(key)) {
														Setting.Key = key;
														//move selection away from the key immediately
														KeyInput.selected = false;
														UICamera.selectedObject = gameObject;
														OnClickOKButton();
												} else {
														KeyInput.text = string.Empty;
														KeyInput.label.text = string.Empty;
												}
										}
								}
						}
				}

				public void OnClickChangeMouseButton()
				{
						int index = Setting.AvailableMouseButtons.IndexOf(Setting.Mouse);
						Setting.Mouse = Setting.AvailableMouseButtons.NextItem(index);
						Refresh();
				}

				public void ResetState()
				{
						OKButton.SetActive(false);
						KeyInput.gameObject.SetActive(false);
						KeyAxisInput1.gameObject.SetActive(false);
						KeyAxisInput2.gameObject.SetActive(false);

						MainButton.SetActive(true);
						ChangeControllerButton.SetActive(true);
						ChangeMouseButton.SetActive(true);

						if (Setting.AxisSetting) {
								ChangeKeyButton.SetActive(false);
								ChangeKeyAxis1Button.SetActive(true);
								ChangeKeyAxis2Button.SetActive(true);
								if (Setting.HasAvailableKeys) {
										ChangeKeyAxis1Button.SendMessage("SetEnabled");
										ChangeKeyAxis2Button.SendMessage("SetEnabled");
								} else {
										ChangeKeyAxis1Button.SendMessage("SetDisabled");
										ChangeKeyAxis2Button.SendMessage("SetDisabled");
								}
						} else {
								ChangeKeyButton.SetActive(true);
								ChangeKeyAxis1Button.SetActive(false);
								ChangeKeyAxis2Button.SetActive(false);
								if (Setting.HasAvailableKeys) {
										ChangeKeyButton.SendMessage("SetEnabled");
								} else {
										ChangeKeyButton.SendMessage("SetDisabled");
								}
						}

						if (Setting.HasAvailableMouseButtons) {
								ChangeMouseButton.SendMessage("SetEnabled");
						} else {
								ChangeMouseButton.SendMessage("SetDisabled");
						}

						if (Setting.HasAvailableControllerButtons) {
								ChangeControllerButton.SendMessage("SetEnabled");
						} else {
								ChangeControllerButton.SendMessage("SetDisabled");
								
						}
				}

				public void Refresh()
				{
						ActionName.text = mSetting.ActionDescription;
						if (mSetting.AxisSetting) {
								KeyAxis1Binding.text = mSetting.KeyX.ToString().Replace("None", "");
								KeyAxis2Binding.text = mSetting.KeyY.ToString().Replace("None", "");
						} else {
								KeyBinding.text = mSetting.Key.ToString().Replace("None", "");
						}
						ControllerBinding.text = mSetting.Controller.ToString().Replace("None", "");
						MouseBinding.text = mSetting.Mouse.ToString().Replace("None", "");
				}

				protected ActionSetting mSetting;
				public static GUIUserActionBrowserObject gActiveObject = null;
		}
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using InControl;

namespace Frontiers
{
		public class InterfaceActionManager : ActionManager <InterfaceActionType>
		{
				public static InterfaceActionManager Get;
				//used for controller-driven mouse movement
				//TODO hook these up to player preferences
				public static bool ControllerDrivenMouseMovement = false;
				public static float ControllerMouseSensitivity = 0.05f;
				public static int ControllerMouseSmoothSteps = 5;
				//used for NGUI events
				public static bool CursorClickDown;
				public static bool CursorClickUp;
				public static bool CursorRightClickDown;
				public static bool CursorRightClickUp;
				InputControlType CursorClickAction = InputControlType.Action1;
				InputControlType CursorRightClickAction = InputControlType.Action2;

				public static bool Suspended {
						get {
								//daaaaangerous!
								return Get.mSuspended;

						}set {
								//daaaaangerous!
								Get.mSuspended = value;
						}
				}

				public override void Awake()
				{
						Get = this;		
						base.Awake();
				}

				protected override void CreateKeyboardAndMouseProfile()
				{
						KeyboardAndMouseProfile = new InterfaceKeyboardAndMouseProfile(this);
				}

				protected override void PushDefaulSettings()
				{
						MouseXAxis = InputControlType.RightStickX;
						MouseYAxis = InputControlType.RightStickY;
						MovementXAxis = InputControlType.LeftStickX;
						MovementYAxis = InputControlType.LeftStickY;

						//AddKeyDown(KeyCode.Alpha1, "Quickslot 1", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha2, "Quickslot 2", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha3, "Quickslot 3", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha4, "Quickslot 4", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha5, "Quickslot 5", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha6, "Quickslot 6", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha7, "Quickslot 7", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha8, "Quickslot 8", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha9, "Quickslot 9", InterfaceActionType.SelectionNumeric, false);
						//AddKeyDown(KeyCode.Alpha0, "Quickslot 10", InterfaceActionType.SelectionNumeric, false);

						AddMapping(InputControlType.Button0, ActionSettingType.Down, InterfaceActionType.ToggleInventory);
						AddMapping(InputControlType.Button1, ActionSettingType.Down, InterfaceActionType.ToggleLog);
						AddMapping(InputControlType.Button2, ActionSettingType.Down, InterfaceActionType.ToggleMap);
						AddMapping(InputControlType.Action1, ActionSettingType.Down, InterfaceActionType.CursorClick);
						AddMapping(InputControlType.Action2, ActionSettingType.Down, InterfaceActionType.CursorRightClick);
						AddMapping(InputControlType.Start, ActionSettingType.Down, InterfaceActionType.ToggleInterfaceNext);
						AddMapping(InputControlType.DPadLeft, ActionSettingType.Down, InterfaceActionType.SelectionPrev);
						AddMapping(InputControlType.DPadRight, ActionSettingType.Down, InterfaceActionType.SelectionNext);

						AddAxisChange(MouseXAxis, InterfaceActionType.CursorMove);
						AddAxisChange(MouseXAxis, InterfaceActionType.CursorMove);
				}

				protected override void OnUpdate()
				{
						CursorClickDown = InputManager.ActiveDevice.GetControl(CursorClickAction).IsPressed;
						CursorClickUp = InputManager.ActiveDevice.GetControl(CursorClickAction).WasReleased;
						CursorRightClickDown = InputManager.ActiveDevice.GetControl(CursorRightClickAction).IsPressed;
						CursorRightClickUp = InputManager.ActiveDevice.GetControl(CursorClickAction).WasReleased;
				}

				public void LateUpdate()
				{
						//see if the mouse is moving this frame
						if (Input.GetAxis("mouse x") != 0f || Input.GetAxis("mouse y") != 0f) {
								MouseSmoothX.Clear();
								MouseSmoothY.Clear();
								ControllerDrivenMouseMovement = false;
						} else {
								//if it's not moving, our controller drives the mouse
								ControllerDrivenMouseMovement = true;
								if (RawMouseAxisX != 0f || RawMouseAxisY != 0f) {
										int mouseChangeX = (int)(RawMouseAxisX * ControllerMouseSensitivity * Screen.width);
										int mouseChangeY = (int)(RawMouseAxisY * ControllerMouseSensitivity * Screen.height);
										int mouseX = Mathf.Clamp((int)Input.mousePosition.x + mouseChangeX, 0, Screen.width);
										int mouseY = Mathf.Clamp((int)Input.mousePosition.y + mouseChangeY, 0, Screen.height);

										MouseSmoothX.Add(mouseX);
										MouseSmoothY.Add(mouseY);

										if (MouseSmoothX.Count > ControllerMouseSmoothSteps) {
												MouseSmoothX.RemoveAt(0);
										}
										if (MouseSmoothY.Count > ControllerMouseSmoothSteps) {
												MouseSmoothY.RemoveAt(0);
										}

										mouseX = 0;
										for (int i = 0; i < MouseSmoothX.Count; i++) {
												mouseX += MouseSmoothX[i];
										}

										mouseY = 0;
										for (int i = 0; i < MouseSmoothY.Count; i++) {
												mouseY += MouseSmoothY[i];
										}

										mouseX = mouseX / MouseSmoothX.Count;
										mouseY = mouseY / MouseSmoothY.Count;

										ProMouse.Instance.SetCursorPosition(mouseX, mouseY);
								}
						}
				}

				protected List <int> MouseSmoothX = new List<int>();
				protected List <int> MouseSmoothY = new List<int>();
		}

		public class InterfaceKeyboardAndMouseProfile : UnityInputDeviceProfile
		{
				public InterfaceKeyboardAndMouseProfile(InterfaceActionManager a) : base()
				{
						ButtonMappings = new[] {
								new InputControlMapping {
										Handle = InterfaceActionType.CursorClick.ToString(),
										Target = InputControlType.Action1,
										Source = MouseButton0
								},
								new InputControlMapping {
										Handle = InterfaceActionType.CursorRightClick.ToString(),
										Target = InputControlType.Action2,
										Source = MouseButton1
								},
						};

						AnalogMappings = new[] {
								new InputControlMapping {
										Handle = InterfaceActionType.ToggleInventory.ToString(),
										Target = InputControlType.Start,
										Source = KeyCodeButton(KeyCode.P)
								},

								new InputControlMapping {
										Handle = InterfaceActionType.ToggleInventory.ToString(),
										Target = InputControlType.Button0,
										Source = KeyCodeButton(KeyCode.Tab)
								},
								new InputControlMapping {
										Handle = InterfaceActionType.ToggleInventory.ToString(),
										Target = InputControlType.Button0,
										Source = KeyCodeButton(KeyCode.I)
								},
								new InputControlMapping {
										Handle = InterfaceActionType.ToggleLog.ToString(),
										Target = InputControlType.Button1,
										Source = KeyCodeButton(KeyCode.L)
								},
								new InputControlMapping {
										Handle = InterfaceActionType.ToggleMap.ToString(),
										Target = InputControlType.Button2,
										Source = KeyCodeButton(KeyCode.M)
								},

								new InputControlMapping {
										Handle = InterfaceActionType.SelectionNext.ToString(),
										Target = InputControlType.DPadLeft,
										Source = MouseScrollWheel,
										SourceRange = InputControlMapping.Range.Positive
								},
								new InputControlMapping {
										Handle = InterfaceActionType.SelectionPrev.ToString(),
										Target = InputControlType.DPadRight,
										Source = MouseScrollWheel,
										SourceRange = InputControlMapping.Range.Negative
								},
						};
				}
		}
}
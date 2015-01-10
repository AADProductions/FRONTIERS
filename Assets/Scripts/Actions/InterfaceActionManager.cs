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

				public static bool Suspended {
						get {
								return Get.mSuspended;

						}set {
								Get.mSuspended = value;
						}
				}

				public override void Awake()
				{
						Get = this;		
						base.Awake();
				}

				protected override void AddDaisyChains()
				{
						AddAxisChange(MouseXAxis, InterfaceActionType.CursorMove);
						AddAxisChange(MouseYAxis, InterfaceActionType.CursorMove);
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
										int mouseX = (int)Input.mousePosition.x + (int)(RawMouseAxisX * ControllerMouseSensitivity * Screen.width);
										int mouseY = (int)Input.mousePosition.y + (int)(RawMouseAxisY * ControllerMouseSensitivity * Screen.height);
										//keep the mouse from going to a second monitor
										if (Screen.fullScreen) {
												mouseX = Mathf.Clamp(mouseX, 0, Screen.width);
												mouseY = Mathf.Clamp(mouseY, 0, Screen.height);
										}

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

				protected override void OnPushSettings()
				{
						if (Profile.Get.HasSelectedProfile) {
								Profile.Get.CurrentPreferences.Controls.InterfaceActionSettings.Clear();
								Profile.Get.CurrentPreferences.Controls.InterfaceActionSettings.AddRange(CurrentActionSettings);
						}
				}

				protected override void OnUpdate()
				{
						//TEMP TODO this is to get around the fact that mouse scroll wheel is so flakey
						//it won't be necessary eventually...
						if (RawScrollWheelAxis > Globals.MouseScrollSensitivity) {
								Send(InterfaceActionType.SelectionPrev, TimeStamp);
						}
						if (RawScrollWheelAxis < -Globals.MouseScrollSensitivity) {
								Send(InterfaceActionType.SelectionNext, TimeStamp);
						}
				}

				public override List<ActionSetting> GenerateDefaultActionSettings()
				{
						MouseXAxis = InputControlType.RightStickX;
						MouseYAxis = InputControlType.RightStickY;
						MovementXAxis = InputControlType.LeftStickX;
						MovementYAxis = InputControlType.LeftStickY;
						ScrollWheelAxis = InputControlType.DPadX;

						List <ActionSetting> actionSettings = new List<ActionSetting>();
						ActionSetting aSetting = null;

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Interface Mouse Y";
						aSetting.Controller = MouseYAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MouseY;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Interface Mouse X";
						aSetting.Controller = MouseXAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MouseX;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Selection L / R";
						aSetting.ActionOnX = (int)InterfaceActionType.SelectionNext;
						aSetting.ActionOnY = (int)InterfaceActionType.SelectionPrev;
						aSetting.Controller = ScrollWheelAxis;
						aSetting.KeyX = KeyCode.RightBracket;
						aSetting.KeyY = KeyCode.LeftBracket;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.Mouse = ActionSetting.MouseAction.Wheel;//read-only
						aSetting.Axis = ActionSetting.InputAxis.ScrollWheel;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)InterfaceActionType.ToggleInventory;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.ToggleInventory.ToString());
						aSetting.Controller = InputControlType.Button3;
						aSetting.Key = KeyCode.Tab;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)InterfaceActionType.ToggleLog;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.ToggleLog.ToString());
						aSetting.Controller = InputControlType.Button2;
						aSetting.Key = KeyCode.L;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)InterfaceActionType.ToggleMap;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.ToggleMap.ToString());
						aSetting.Controller = InputControlType.Button1;
						aSetting.Key = KeyCode.M;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)InterfaceActionType.ToggleInterfaceNext;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.ToggleInterfaceNext.ToString());
						aSetting.Controller = InputControlType.Start;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.Action = (int)InterfaceActionType.CursorClick;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.CursorClick.ToString());
						aSetting.Controller = InputControlType.Action1;
						aSetting.Mouse = ActionSetting.MouseAction.Left;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						aSetting.Cursor = ActionSetting.CursorAction.Click;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.Action = (int)InterfaceActionType.CursorRightClick;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.CursorRightClick.ToString());
						aSetting.Controller = InputControlType.Action2;
						aSetting.Mouse = ActionSetting.MouseAction.Right;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						aSetting.Cursor = ActionSetting.CursorAction.RightClick;
						actionSettings.Add(aSetting);

						return actionSettings;
				}

				protected List <int> MouseSmoothX = new List<int>();
				protected List <int> MouseSmoothY = new List<int>();
		}
}
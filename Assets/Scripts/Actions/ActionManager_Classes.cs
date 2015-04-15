using UnityEngine;
using System.Collections;
using InControl;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;

namespace Frontiers
{
		public class UserKeyboardAndMouseProfile<T> : UnityInputDeviceProfile where T : struct, IConvertible, IComparable, IFormattable
		{
				public UserKeyboardAndMouseProfile(ActionManager<T> u) : base()
				{
						// This profile only works on desktops.
						SupportedPlatforms = new[] {
								"Windows",
								"Mac",
								"Linux"
						};

						Sensitivity = 1.0f;
						LowerDeadZone = 0.0f;
						UpperDeadZone = 1.0f;

						Name = u.GetType().Name;

						List <InputControlMapping> analogMappings = new List<InputControlMapping>();
						List <InputControlMapping> buttonMappings = new List<InputControlMapping>();
						KeyCode key = KeyCode.None;
						KeyCode keyX = KeyCode.None;
						KeyCode keyY = KeyCode.None;
						ActionSetting.MouseAction mouse = ActionSetting.MouseAction.None;

						#region scroll wheel
						//bind our scroll wheel to the scroll wheel axis
						switch (u.ScrollWheelAxis) {
								case InputControlType.DPadX:
										analogMappings.Add(
												new InputControlMapping {
														Handle = "Look Z X",
														Target = InputControlType.DPadRight,
														Source = MouseScrollWheel,
														Raw = true,
														SourceRange = InputControlMapping.Range.Negative
												}
										);
										analogMappings.Add(
												new InputControlMapping {
														Handle = "Look Z Y",
														Target = InputControlType.DPadLeft,
														Source = MouseScrollWheel,
														Raw = true,
														SourceRange = InputControlMapping.Range.Positive
												}
										);
										break;

								case InputControlType.DPadY:
										analogMappings.Add(
												new InputControlMapping {
														Handle = "Look Z X",
														Target = InputControlType.DPadUp,
														Source = MouseScrollWheel,
														Raw = true,
														SourceRange = InputControlMapping.Range.Negative
												}
										);
										analogMappings.Add(
												new InputControlMapping {
														Handle = "Look Z Y",
														Target = InputControlType.DPadDown,
														Source = MouseScrollWheel,
														Raw = true,
														SourceRange = InputControlMapping.Range.Positive
												}
										);
										break;

								default:
										analogMappings.Add(
												new InputControlMapping {
														Handle = "Look Z",
														Target = u.ScrollWheelAxis,
														Source = MouseScrollWheel,
														Raw = true,
												}
										);
										break;
						}
						#endregion

						#region axis mappings
						//bind keys to our axis
						if (u.GetKeyAxis(u.ScrollWheelAxis, ref keyX, ref keyY)) {
								switch (u.ScrollWheelAxis) {
										case InputControlType.DPadX:
												buttonMappings.Add(
														new InputControlMapping {
																Handle = "Scroll Right",
																Target = InputControlType.DPadRight,
																Source = KeyCodeButton(keyX)
														}
												);
												buttonMappings.Add(
														new InputControlMapping {
																Handle = "Scroll Left",
																Target = InputControlType.DPadLeft,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										case InputControlType.DPadY:
												//Debug.Log ("binding mouse scroll axis dpady");
												buttonMappings.Add(
														new InputControlMapping {
																Handle = "Scroll Right",
																Target = InputControlType.DPadUp,
																Source = KeyCodeButton(keyX)
														}
												);
												buttonMappings.Add(
														new InputControlMapping {
																Handle = "Scroll Left",
																Target = InputControlType.DPadDown,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										default:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Scroll left / right",
																Target = u.ScrollWheelAxis,
																Source = KeyCodeAxis(keyX, keyY)
														}
												);
												break;
								}
						}

						if (u.GetKeyAxis(u.MovementXAxis, ref keyX, ref keyY)) {
								switch (u.MovementXAxis) {
										case InputControlType.DPadX:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Right",
																Target = InputControlType.DPadRight,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Left",
																Target = InputControlType.DPadLeft,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										case InputControlType.DPadY:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Right",
																Target = InputControlType.DPadUp,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Left",
																Target = InputControlType.DPadDown,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										default:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move left / right",
																Target = u.MovementXAxis,
																Source = KeyCodeAxis(keyX, keyY)
														}
												);
												break;
								}
						}

						if (u.GetKeyAxis(u.MovementYAxis, ref keyX, ref keyY)) {
								switch (u.MovementYAxis) {
										case InputControlType.DPadX:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Forward",
																Target = InputControlType.DPadRight,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Back",
																Target = InputControlType.DPadLeft,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										case InputControlType.DPadY:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Forward",
																Target = InputControlType.DPadUp,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move Back",
																Target = InputControlType.DPadDown,
																Source = KeyCodeButton(keyY)
														}
												);
												break;


										default:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Move forward / back",
																Target = u.MovementYAxis,
																Source = KeyCodeAxis(keyX, keyY)
														}
												);
												break;
								}
						}

						if (u.MouseXAxis != InputControlType.None) {
								switch (u.MovementYAxis) {
										case InputControlType.DPadX:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse X X",
																Target = InputControlType.DPadRight,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse X Y",
																Target = InputControlType.DPadLeft,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										case InputControlType.DPadY:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse X X",
																Target = InputControlType.DPadUp,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse X Y",
																Target = InputControlType.DPadDown,
																Source = KeyCodeButton(keyY)
														}
												);
												break;


										default:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse X axis",
																Target = u.MouseXAxis,
																Source = MouseXAxis,
																Scale = 0.1f,
																Raw = true,
														}
												);
												break;
								}
						}

						if (u.MouseYAxis != InputControlType.None) {
								switch (u.MovementYAxis) {
										case InputControlType.DPadX:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse Y X",
																Target = InputControlType.DPadRight,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse Y Y",
																Target = InputControlType.DPadLeft,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										case InputControlType.DPadY:
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse Y X",
																Target = InputControlType.DPadUp,
																Source = KeyCodeButton(keyX)
														}
												);
												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse Y Y",
																Target = InputControlType.DPadDown,
																Source = KeyCodeButton(keyY)
														}
												);
												break;

										default:

												analogMappings.Add(
														new InputControlMapping {
																Handle = "Mouse Y axis",
																Target = u.MouseYAxis,
																Source = MouseYAxis,
																Scale = 0.1f,
																Raw = true,
														}
												);
												break;
								}
						}

						#endregion

						#region button mappings

						List <InputControlMapping> mappings = null;
						foreach (ActionSetting a in u.CurrentActionSettings) {
								if (a.IsAnalog) {
										mappings = analogMappings;
								} else {
										mappings = buttonMappings;
								}
								//if this has a controller setting
								if (a.Controller != InputControlType.None) {
										//bind it to mouse and keys
										if (a.Key != KeyCode.None) {
												//Debug.Log("Binding " + a.ActionDescription + " to key " + a.Key.ToString() + " - mouse is " + a.Mouse.ToString());
												mappings.Add(
														new InputControlMapping {
																Handle = a.ActionDescription,
																Target = a.Controller,
																Source = KeyCodeButton(a.Key)
														}
												);
										}

										switch (a.Mouse) {
												case ActionSetting.MouseAction.Left:
														mappings.Add(
																new InputControlMapping {
																		Handle = a.ActionDescription,
																		Target = a.Controller,
																		Source = MouseButton0
																}
														);
														break;

												case ActionSetting.MouseAction.Right:
														mappings.Add(
																new InputControlMapping {
																		Handle = a.ActionDescription,
																		Target = a.Controller,
																		Source = MouseButton1
																}
														);
														break;

												case ActionSetting.MouseAction.Middle:
														mappings.Add(
																new InputControlMapping {
																		Handle = a.ActionDescription,
																		Target = a.Controller,
																		Source = MouseButton2
																}
														);
														break;

												default:
														//other keys are descriptive, don't bind them
														break;
										}
								}
						}

						#endregion

						AnalogMappings = analogMappings.ToArray();
						ButtonMappings = buttonMappings.ToArray();
				}
		}

		[Serializable]
		public class ActionSetting
		{
				public static ActionSetting Button {
						get {
								ActionSetting setting = new ActionSetting();
								setting.IsAnalog = false;
								return setting;
						}
				}

				public static ActionSetting Analog {
						get {
								ActionSetting setting = new ActionSetting();
								return setting;
						}
				}

				public bool IsBindable {
						get {
								if (AxisSetting) {
										return ActionOnX > 0 && ActionOnY > 0 && Controller != InputControlType.None;
								} else {
										return Action > 0 && Controller != InputControlType.None;
								}
						}
				}

				public bool AxisSetting {
						get {
								return Axis != InputAxis.None;
						}
				}

				public bool IsAnalog = true;
				public int Action = -1;
				public int ActionOnHold = -1;
				public int ActionOnRelease = -1;
				public int ActionOnX = -1;
				public int ActionOnY = -1;
				public string ActionDescription;
				public InputAxis Axis = InputAxis.None;
				public InputControlType Controller = InputControlType.None;
				public KeyCode Key = KeyCode.None;
				public KeyCode KeyX = KeyCode.None;
				public KeyCode KeyY = KeyCode.None;
				public MouseAction Mouse = MouseAction.None;
				public CursorAction Cursor = CursorAction.None;
				//these props are saved to the profile
				//if they're true then available buttons are added on display
				public bool HasAvailableKeys = false;
				public bool HasAvailableMouseButtons = false;
				public bool HasAvailableControllerButtons = false;
				//these lists aren't saved because it would be terribly redundant
				[XmlIgnore]
				public List <InputControlType> AvailableControllerButtons {
						get {
								return mAvailableControllerButtons;
						}
						set {
								HasAvailableControllerButtons = false;
								mAvailableControllerButtons = value;
								if (mAvailableControllerButtons != null && mAvailableControllerButtons.Count > 0) {
										HasAvailableControllerButtons = true;
								}
						}
				}

				[XmlIgnore]
				public List <KeyCode> AvailableKeys {
						get {
								return mAvailableKeys;
						}
						set {
								HasAvailableKeys = false;
								mAvailableKeys = value;
								if (mAvailableKeys != null && mAvailableKeys.Count > 0) {
										HasAvailableKeys = true;
								}
						}
				}

				[XmlIgnore]
				public List <MouseAction> AvailableMouseButtons { 
						get {
								return mAvailableMouseButtons;
						}
						set {
								HasAvailableMouseButtons = false;
								mAvailableMouseButtons = value;
								if (mAvailableMouseButtons != null && mAvailableMouseButtons.Count > 0) {
										HasAvailableMouseButtons = true;
								}
						}
				}

				protected List <InputControlType> mAvailableControllerButtons;
				protected List <KeyCode> mAvailableKeys;
				protected List <MouseAction> mAvailableMouseButtons;

				public enum InputAxis
				{
						None,
						MouseX,
						MouseY,
						MovementX,
						MovementY,
						ScrollWheel,
						InterfaceX,
						InterfaceY,
				}

				public enum MouseAction
				{
						None,
						Left,
						Right,
						Middle,
						AxisX,
						AxisY,
						Wheel,
				}

				public enum CursorAction
				{
						None,
						Click,
						RightClick,
				}
		}
}

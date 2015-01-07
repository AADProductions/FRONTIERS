using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using System.Xml.Serialization;

namespace Frontiers
{
		public class UserActionManager : ActionManager <UserActionType>
		{
				public static UserActionManager Get;

				public override string GameObjectName {
						get {
								return "Frontiers_InputManager";
						}
				}

				public static bool Suspended {
						get {
								return Get.mSuspended;

						}set {
								Get.mSuspended = value;
						}
				}

				public override void WakeUp()
				{
						Get	= this;
				}

				protected override void PushDefaulSettings()
				{
						//AddKeyDown(KeyCode.Escape, "Cancel", UserActionType.ActionCancel, false);
						//AddKeyDown(KeyCode.Space, "Skip", UserActionType.ActionSkip, false);

						//AddKeyDown(KeyCode.Space, "Jump", UserActionType.MoveJump, false);
						//AddKeyDown(KeyCode.C, "Crouch", UserActionType.MoveCrouch, false);
						//AddKeyDown(KeyCode.E, "Item Use", UserActionType.ItemUse, false);
						//AddMouseButtonDown(1, "Item Interact", UserActionType.ItemInteract, false);
						//AddKeyDown(KeyCode.F, "Item Place", UserActionType.ItemPlace, false);
						//AddKeyDown(KeyCode.G, "Item Throw", UserActionType.ItemThrow, false);
						//AddMouseButtonDown(0, "Tool Use", UserActionType.ToolUse, false);
						//AddMouseButtonHold(0, "Tool Use", UserActionType.ToolUseHold, false);
						//AddMouseButtonUp(0, "Tool Release", UserActionType.ToolUseRelease, true);
						//AddKeyDown(KeyCode.Q, "Tool Holster", UserActionType.ToolHolster, false);
						//AddKeyDown(KeyCode.R, "Tool Next Setting", UserActionType.ToolCyclePrev, false);
						//AddKeyDoubleTap(KeyCode.W, "Sprint", UserActionType.MoveSprint, false);

						//AddKeyDown(KeyCode.H, "Tool Swap", UserActionType.ToolSwap, false);
						//AddKeyDown(KeyCode.R, "Tool Next Setting", UserActionType.ToolCyclePrev, false);
						//AddKeyDown(KeyCode.T, "Tool Prev Setting", UserActionType.ToolCycleNext, false);
						//AddKeyDown(KeyCode.Q, "Tool Holster", UserActionType.ToolHolster, false);

						MouseXAxis = InputControlType.RightStickX;
						MouseYAxis = InputControlType.RightStickY;
						MovementXAxis = InputControlType.LeftStickX;
						MovementYAxis = InputControlType.LeftStickY;

						AddMapping(InputControlType.Action4, ActionSettingType.Down, UserActionType.ActionCancel);
						AddMapping(InputControlType.Action4, ActionSettingType.Down, UserActionType.ActionSkip);

						AddMapping(InputControlType.RightStickButton, ActionSettingType.Down, UserActionType.MoveJump);
						AddMapping(InputControlType.LeftStickButton, ActionSettingType.Down, UserActionType.MoveCrouch);
						AddMapping(InputControlType.Action1, ActionSettingType.Down, UserActionType.ItemUse);
						AddMapping(InputControlType.Action2, ActionSettingType.Down, UserActionType.ItemInteract);
						AddMapping(InputControlType.Action3, ActionSettingType.Down, UserActionType.ItemPlace);
						AddMapping(InputControlType.RightTrigger, ActionSettingType.Down, UserActionType.ItemThrow);
						AddMapping(InputControlType.RightBumper, ActionSettingType.Down, UserActionType.ToolUse);
						AddMapping(InputControlType.RightBumper, ActionSettingType.Hold, UserActionType.ToolUseHold);
						AddMapping(InputControlType.RightBumper, ActionSettingType.Up, UserActionType.ToolUseRelease);
						AddMapping(InputControlType.LeftTrigger, ActionSettingType.Down, UserActionType.ToolCycleNext);
						AddMapping(InputControlType.LeftBumper, ActionSettingType.Down, UserActionType.MoveSprint);

						AddAxisChange(MovementXAxis, UserActionType.MovementAxisChange);
						AddAxisChange(MovementYAxis, UserActionType.MovementAxisChange);
						AddAxisChange(MouseXAxis, UserActionType.LookAxisChange);
						AddAxisChange(MouseYAxis, UserActionType.LookAxisChange);
				}

				protected override void CreateKeyboardAndMouseProfile()
				{
						KeyboardAndMouseProfile = new UserKeyboardAndMouseProfile(this);
				}

				protected override void AddDaisyChains()
				{
						AddDaisyChain(UserActionType.MoveJump, UserActionType.MoveStand);
						AddDaisyChain(UserActionType.MoveForward, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveRun, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveLeft, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveRight, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveJump, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveStand, UserActionType.Move);
						AddDaisyChain(UserActionType.MoveCrouch, UserActionType.Move);
				}

				public class UserKeyboardAndMouseProfile : UnityInputDeviceProfile
				{
						public UserKeyboardAndMouseProfile(UserActionManager u) : base()
						{
								ButtonMappings = new[] {
										new InputControlMapping {
												Handle = UserActionType.ToolUse.ToString(),
												Target = InputControlType.Action1,
												Source = MouseButton0
										},
										new InputControlMapping {
												Handle = UserActionType.ItemInteract.ToString(),
												Target = InputControlType.Action2,
												Source = MouseButton1
										},
								};

								AnalogMappings = new[] {
										new InputControlMapping {
												Handle = UserActionType.MoveLeft.ToString (),//and right
												Target = u.MovementXAxis,
												Source = KeyCodeAxis(KeyCode.A, KeyCode.D)
										},
										new InputControlMapping {
												Handle = UserActionType.MoveForward.ToString (),//and back
												Target = u.MovementYAxis,
												Source = KeyCodeAxis(KeyCode.S, KeyCode.W)
										},

										new InputControlMapping {
												Handle = UserActionType.ActionCancel.ToString(),
												Target = InputControlType.Action4,
												Source = KeyCodeButton(KeyCode.Escape)
										},
										new InputControlMapping {
												Handle = UserActionType.MoveJump.ToString(),
												Target = InputControlType.Action4,
												Source = KeyCodeButton(KeyCode.Escape)
										},
										new InputControlMapping {
												Handle = UserActionType.MoveCrouch.ToString(),
												Target = InputControlType.LeftStickButton,
												Source = KeyCodeButton(KeyCode.C)
										},
										new InputControlMapping {
												Handle = UserActionType.ToolCycleNext.ToString(),
												Target = InputControlType.LeftStickButton,
												Source = KeyCodeButton(KeyCode.C)
										},
										new InputControlMapping {
												Handle = UserActionType.ToolCycleNext.ToString(),
												Target = InputControlType.LeftBumper,
												Source = KeyCodeButton(KeyCode.R)
										},
										new InputControlMapping {
												Handle = UserActionType.MoveSprint.ToString(),
												Target = InputControlType.LeftBumper,
												Source = KeyCodeButton(KeyCode.LeftShift)
										},
										new InputControlMapping {
												Handle = UserActionType.MoveSprint.ToString(),
												Target = InputControlType.LeftTrigger,
												Source = KeyCodeButton(KeyCode.RightShift)
										},
								};
						}
				}
		}
}
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

				protected override void OnPushSettings ( ) {
						if (Profile.Get.HasSelectedProfile) {
								Profile.Get.CurrentPreferences.Controls.UserActionSettings.Clear();
								Profile.Get.CurrentPreferences.Controls.UserActionSettings.AddRange(CurrentActionSettings);
						}
				}

				public override List<ActionSetting> GenerateDefaultActionSettings()
				{
						MouseXAxis = InputControlType.RightStickX;
						MouseYAxis = InputControlType.RightStickY;
						MovementXAxis = InputControlType.LeftStickX;
						MovementYAxis = InputControlType.LeftStickY;

						List <ActionSetting> actionSettings = new List<ActionSetting>();
						ActionSetting aSetting = null;

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Controller Mouse U / D";
						aSetting.Controller = MouseYAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MouseY;
						aSetting.Mouse = ActionSetting.MouseAction.AxisY;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Controller Mouse L / R";
						aSetting.Controller = MouseXAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MouseX;
						aSetting.Mouse = ActionSetting.MouseAction.AxisX;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.ActionDescription = "Movement B / F";
						aSetting.Controller = MovementYAxis;
						aSetting.KeyX = KeyCode.S;
						aSetting.KeyY = KeyCode.W;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.Axis = ActionSetting.InputAxis.MovementY;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.ActionDescription = "Movement L / R";
						aSetting.Controller = MovementXAxis;
						aSetting.KeyX = KeyCode.A;
						aSetting.KeyY = KeyCode.D;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.Axis = ActionSetting.InputAxis.MovementX;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.MoveSprint;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.MoveSprint.ToString());
						aSetting.Controller = InputControlType.LeftTrigger;
						aSetting.Key = KeyCode.LeftShift;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.MoveJump;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.MoveJump.ToString());
						aSetting.Controller = InputControlType.RightStickButton;
						aSetting.Key = KeyCode.Space;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.MoveCrouch;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.MoveCrouch.ToString());
						aSetting.Controller = InputControlType.LeftStickButton;
						aSetting.Key = KeyCode.C;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ItemUse;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ItemUse.ToString());
						aSetting.Controller = InputControlType.Action1;
						aSetting.Key = KeyCode.E;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ItemInteract;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ItemInteract.ToString());
						aSetting.Controller = InputControlType.Action2;
						aSetting.Mouse = ActionSetting.MouseAction.Right;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						//aSetting.Cursor = ActionSetting.CursorAction.RightClick;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ItemPlace;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ItemPlace.ToString());
						aSetting.Controller = InputControlType.Action3;
						aSetting.Key = KeyCode.F;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ActionCancel;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ActionCancel.ToString());
						aSetting.Controller = InputControlType.Action4;
						aSetting.Key = KeyCode.Escape;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ItemThrow;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ItemThrow.ToString());
						aSetting.Controller = InputControlType.RightTrigger;
						aSetting.Key = KeyCode.G;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ToolUse;
						aSetting.ActionOnHold = (int)UserActionType.ToolUseHold;
						aSetting.ActionOnRelease = (int)UserActionType.ToolUseRelease;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ToolUse.ToString());
						aSetting.Controller = InputControlType.RightBumper;
						aSetting.Mouse = ActionSetting.MouseAction.Left;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						//aSetting.Cursor = ActionSetting.CursorAction.Click;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ToolCyclePrev;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ToolCyclePrev.ToString());
						aSetting.Controller = InputControlType.LeftTrigger;
						aSetting.Key = KeyCode.R;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ToolCycleNext;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ToolCycleNext.ToString());
						aSetting.Key = KeyCode.T;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)UserActionType.ToolHolster;
						aSetting.Controller = InputControlType.Menu;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(UserActionType.ToolHolster.ToString());
						aSetting.Key = KeyCode.Q;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						actionSettings.Add(aSetting);

						return actionSettings;
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

						AddAxisChange(MovementXAxis, UserActionType.MovementAxisChange);
						AddAxisChange(MovementYAxis, UserActionType.MovementAxisChange);
						AddAxisChange(MouseXAxis, UserActionType.LookAxisChange);
						AddAxisChange(MouseYAxis, UserActionType.LookAxisChange);
				}
		}
}
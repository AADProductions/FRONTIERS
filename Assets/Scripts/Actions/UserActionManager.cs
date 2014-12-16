using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers {
	public class UserActionManager : ActionManager <UserActionType>
	{	
		public static UserActionManager Get;
		public List <UserActionSetting> Settings;

		public override string GameObjectName {
			get {
				return "Frontiers_InputManager";
			}
		}

		public static bool Suspended
		{
			get {
				return Get.mSuspended;

			}set {
				Get.mSuspended = value;
			}
		}

		public override void WakeUp ( )
		{
			Get	= this;
		}

		public override void Initialize ( )
		{
			if (Settings == null) {
				Settings = new List<UserActionSetting>();
			} else {
				Settings.Clear();
			}

			AddKeyDown			(KeyCode.Escape,			UserActionType.ActionCancel);

			AddKeyDown			(KeyCode.Space,				UserActionType.ActionSkip);
			AddMouseButtonDown	(0,							UserActionType.ActionSkip);

			AddKeyDown 			(KeyCode.W, 				UserActionType.MoveForward);
			AddKeyHold 			(KeyCode.W, 				UserActionType.MoveForward);
			AddKeyHold 			(KeyCode.S, 				UserActionType.MoveRun);
			AddKeyHold 			(KeyCode.A, 				UserActionType.MoveLeft);
			AddKeyHold 			(KeyCode.D, 				UserActionType.MoveRight);
			AddKeyDown 			(KeyCode.Space, 			UserActionType.MoveJump);
			AddKeyDown			(KeyCode.Space,				UserActionType.MoveStand);
			AddKeyDown 			(KeyCode.C, 				UserActionType.MoveCrouch);
			AddKeyUp			(KeyCode.C, 				UserActionType.MoveStand);
			AddKeyDoubleTap		(KeyCode.W,					UserActionType.MoveSprint);
			AddKeyUp			(KeyCode.W,			 		UserActionType.MoveWalk);
			AddKeyDown			(KeyCode.W,			 		UserActionType.MoveWalk);

			AddKeyHold 			(KeyCode.W, 				UserActionType.Move);
			AddKeyHold 			(KeyCode.S, 				UserActionType.Move);
			AddKeyHold 			(KeyCode.A, 				UserActionType.Move);
			AddKeyHold 			(KeyCode.D, 				UserActionType.Move);
			AddKeyDown 			(KeyCode.Space, 			UserActionType.Move);
			AddKeyHold 			(KeyCode.C, 				UserActionType.Move);
			AddKeyUp			(KeyCode.C, 				UserActionType.Move);

			AddKeyDown			(KeyCode.E,					UserActionType.ItemUse);
			AddMouseButtonDown	(1,							UserActionType.ItemInteract);
			AddKeyDown			(KeyCode.G,					UserActionType.ItemThrow);
			AddKeyDown			(KeyCode.F,					UserActionType.ItemPlace);

			AddMouseButtonDown	(0,							UserActionType.ToolUse);
			AddMouseButtonHold  (0, 						UserActionType.ToolUseHold);
			AddMouseButtonUp	(0, 						UserActionType.ToolUseRelease);
			AddKeyDown			(KeyCode.H, 				UserActionType.ToolSwap);
			AddKeyUp			(KeyCode.LeftShift,			UserActionType.ToolUse);
			AddKeyHold			(KeyCode.LeftShift,			UserActionType.ToolUseHold);
			AddKeyDown			(KeyCode.Z,					UserActionType.ToolUseRelease);
			AddKeyDown 			(KeyCode.R, 				UserActionType.ToolCyclePrev);
			AddKeyDown 			(KeyCode.T, 				UserActionType.ToolCycleNext);
			AddKeyDown 			(KeyCode.Q, 				UserActionType.ToolHolster);

			AddMouseMove		(UserActionType.LookAxisChange);	
			AddAxisMove			(UserActionType.MovementAxisChange);	

			UserActionSetting uas = null;

			foreach (KeyValuePair <KeyCode,List<UserActionType>> keyMapping in mKeyDownMappings) {
				foreach (UserActionType actionType in keyMapping.Value) {
					uas = new UserActionSetting();
					uas.Key = keyMapping.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.KeyDown;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <KeyCode,List<UserActionType>> keyMapping in mKeyUpMappings) {
				foreach (UserActionType actionType in keyMapping.Value) {
					uas = new UserActionSetting();
					uas.Key = keyMapping.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.KeyUp;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <KeyCode,List<UserActionType>> keyMapping in mKeyHoldMappings) {
				foreach (UserActionType actionType in keyMapping.Value) {
					uas = new UserActionSetting();
					uas.Key = keyMapping.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.KeyHold;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <KeyCode,List<UserActionType>> keyMapping in mKeyDoubleTapMappings) {
				foreach (UserActionType actionType in keyMapping.Value) {
					uas = new UserActionSetting();
					uas.Key = keyMapping.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.KeyDoubleTap;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <int,List<UserActionType>> mouseButton in mButtonDownMappings) {
				foreach (UserActionType actionType in mouseButton.Value) {
					uas = new UserActionSetting();
					uas.MouseButton = mouseButton.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.MouseButtonDown;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <int,List<UserActionType>> mouseButton in mButtonUpMappings) {
				foreach (UserActionType actionType in mouseButton.Value) {
					uas = new UserActionSetting();
					uas.MouseButton = mouseButton.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.MouseButtonUp;
					Settings.Add(uas);
				}
			}

			foreach (KeyValuePair <int,List<UserActionType>> mouseButton in mButtonHoldMappings) {
				foreach (UserActionType actionType in mouseButton.Value) {
					uas = new UserActionSetting();
					uas.MouseButton = mouseButton.Key;
					uas.Action = actionType;
					uas.Type = UserActionSettingType.MouseButtonHold;
					Settings.Add(uas);
				}
			}

			uas = new UserActionSetting ();
			uas.AxisName = "MouseWheelDown";
			uas.Action = mMouseWheelDown;
			uas.Type = UserActionSettingType.InputAxis;
			Settings.Add (uas);

			uas = new UserActionSetting ();
			uas.AxisName = "MouseWheelUp";
			uas.Action = mMouseWheelUp;
			uas.Type = UserActionSettingType.InputAxis;
			Settings.Add (uas);

			base.Initialize ( );
		}
	}

	[Serializable]
	public class UserActionSetting {
			public string ActionName = "Action";
			public string BindingDescription {
				get {
					return Key.ToString();
				}
			}	
			public string BindingDescriptionDefault {
				get {
					return Key.ToString();
				}
			}
			public UserActionSettingType Type = UserActionSettingType.None;
			public UserActionType Action = UserActionType.NoAction;
			public KeyCode Key = KeyCode.A;
			public int MouseButton = 0;
			public string AxisName = string.Empty;
	}

	[Flags]//used to store user settings
	public enum UserActionSettingType {
			None = 0,
			KeyDown = 1,
			KeyUp = 2,
			KeyHold = 4,
			KeyDoubleTap = 8,
			MouseButtonDown = 16,
			MouseButtonHold = 32,
			MouseButtonUp = 64,
			MouseMove = 128,
			ControllerButtonDown = 256,
			ControllerButtonHold = 512,
			ControllerButtonUp = 1024,
			InputAxis = 2048,
	}

	[Flags]
	public enum UserActionType : int
	{
		NoAction					= 1,			//0

		Move						= 2,			//1
		MoveForward					= 4,			//2
		MoveRun						= 8,			//3
		MoveLeft					= 16,			//4
		MoveRight					= 32,			//5
		MoveJump					= 64,			//6
		MoveCrouch					= 128,			//7
		MoveStand					= 256,			//8
		MovePlantFeet				= 512,			//9
		MoveSprint					= 1024,			//10
		MoveWalk					= 2048,			//11

		ItemPickUp					= 4096,			//12
		ItemThrow					= 8192,			//13
		ItemUse						= 16384,		//14
		ItemInteract				= 32768,		//15

		ToolUse						= 65536,		//16
		ToolUseHold					= 131072,		//17
		ToolUseRelease				= 262144,		//18
		ToolHolster					= 524288,		//19

		ActionConfirm				= 1048576,		//20
		ActionCancel				= 2097152,		//21
		ActionSkip					= 4194304,		//22

		LookAxisChange				= 8388608,		//23
		MovementAxisChange			= 16777216,		//24

		ToolCyclePrev				= 33554432,		//25
		ToolCycleNext				= 67108864,		//26
		ToolSwap					= 134217728,	//27

		FlagsMovement				= 		Move | MoveForward | MoveRun | MoveLeft | MoveRight | MoveJump | MoveCrouch | MoveStand | MovePlantFeet | MoveSprint | MoveWalk | MovementAxisChange,
		FlagsItems					=		ItemPickUp | ItemThrow | ItemUse | ItemInteract,
		FlagsTools					= 		ToolUse | ToolUseHold | ToolUseRelease | ToolHolster | ToolCyclePrev | ToolCycleNext | ToolSwap,
		FlagsActions				=		ActionConfirm | ActionCancel | ActionSkip,
		FlagsAll					=		FlagsMovement | FlagsItems | FlagsTools | FlagsActions | LookAxisChange,
		FlagsAllButActions			=		FlagsMovement | FlagsItems | FlagsTools | LookAxisChange,
		FlagsAllButMovement			=		FlagsItems | FlagsTools | FlagsActions | LookAxisChange,
		FlagsAllButLookAxis			=		FlagsMovement | FlagsItems | FlagsTools | FlagsActions,
		FlagsBasicMovement			=		Move | MoveForward | MoveRun | MoveLeft | MoveRight | MoveSprint,

		ItemPlace					= 		ItemThrow | ItemUse,
	}
}
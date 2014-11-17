using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class UserActionManager : ActionManager <UserActionType>
{	
	public static UserActionManager Get;

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

		base.Initialize ( );
	}
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
using UnityEngine;
using System.Collections;
using System;

public class InterfaceActionManager : ActionManager <InterfaceActionType>
{	
	public static InterfaceActionManager Get;

	public static bool Suspended
	{
		get {
			//daaaaangerous!
			return Get.mSuspended;

		}set {
			//daaaaangerous!
			Get.mSuspended = value;
		}
	}

	public override void Awake ( )
	{
		Get = this;		
		base.Awake ( );
	}

	public override void Initialize ( )
	{
		mMouseWheelDown 	= 							InterfaceActionType.SelectionPrev;
		mMouseWheelUp 		= 							InterfaceActionType.SelectionNext;

		AddKeyDown 			(KeyCode.Alpha1, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha2, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha3, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha4, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha5, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha6, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha7, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha8, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha9, 			InterfaceActionType.SelectionNumeric);
		AddKeyDown 			(KeyCode.Alpha0, 			InterfaceActionType.SelectionNumeric);

		AddKeyDown			(KeyCode.LeftArrow, 		InterfaceActionType.SelectionPrev);
		AddKeyDown			(KeyCode.RightArrow, 		InterfaceActionType.SelectionNext);

		AddKeyDown 			(KeyCode.LeftBracket, 		InterfaceActionType.InventoryNextQuickslot);
		AddKeyDown 			(KeyCode.RightBracket, 		InterfaceActionType.InventoryPrevQuickslot);
		
		AddKeyDown			(KeyCode.Tab,				InterfaceActionType.ToggleInventory);
		AddKeyDown			(KeyCode.V,					InterfaceActionType.ToggleInventoryCrafting);
		AddKeyDown			(KeyCode.G,					InterfaceActionType.ToggleInventoryClothing);
		
		AddKeyDown			(KeyCode.M,					InterfaceActionType.ToggleMap);
		
		AddKeyDown			(KeyCode.T, 				InterfaceActionType.ToggleStatus);
		
		AddKeyDown			(KeyCode.L,					InterfaceActionType.ToggleLog);
		AddKeyDown			(KeyCode.B,					InterfaceActionType.ToggleLogBooks);
		AddKeyDown			(KeyCode.K, 				InterfaceActionType.ToggleLogSkills);
		AddKeyDown 			(KeyCode.N, 				InterfaceActionType.ToggleLogMissions);
		
		//AddKeyDown			(KeyCode.I,					InterfaceActionType.ToggleInterface);		
		
		base.Initialize ( );
	}
}

[Flags]
public enum InterfaceActionType : int
{
	NoAction					= 1,			//0

	InventoryNextQuickslot		= 2,			//1
	InventoryPrevQuickslot		= 4,			//2

	ToggleInterface				= 8,			//3	
	ToggleInventory				= 16,			//4
	ToggleInventoryClothing		= 32,			//5
	ToggleInventoryCrafting		= 64,			//6
	ToggleStatus				= 128,			//7
	ToggleMap					= 256,			//8
	ToggleLog					= 512,			//9
	ToggleLogMissions			= 1024,			//10
	ToggleLogSkills				= 2048,			//11
	ToggleLogBooks				= 4096,			//12
	ToggleLogPeople				= 8192,			//13

	SelectionUp					= 16384,		//14
	SelectionDown				= 32768,		//15
	SelectionLeft				= 65536,		//16
	SelectionRight				= 131072,		//17	
	SelectionAdd				= 262144,		//18
	SelectionRemove				= 524288,		//19
	SelectionReplace			= 1048576,		//20	
	SelectionNext				= 2097152,		//21
	SelectionPrev				= 4194304,		//22

	CursorMove					= 8388608,		//23
	CursorClick					= 16777216,		//24

	SelectionNumeric			= 33554432,		//25

	FlagsTogglePrimary			= ToggleInventory | ToggleStatus | ToggleMap | ToggleLog,
	FlagsSelectionNextPrev		= SelectionNext | SelectionPrev,

	FlagsAll					= InventoryNextQuickslot | InventoryPrevQuickslot
	                                  | ToggleInterface | ToggleInventory | ToggleInventoryClothing
	                                  | ToggleInventoryCrafting | ToggleStatus | ToggleMap
	                                  | ToggleLog | ToggleLogMissions | ToggleLogSkills
	                                  | ToggleLogBooks | ToggleLogPeople
	                                  | SelectionUp | SelectionDown | SelectionLeft | SelectionRight
	                                  | SelectionAdd | SelectionRemove | SelectionReplace | SelectionNext | SelectionPrev
	                                  | CursorMove | CursorClick,
}
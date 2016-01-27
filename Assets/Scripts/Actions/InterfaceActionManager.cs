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
		public static bool SuspendCursorControllerMovement = false;
		//this is the sprite suffix for controller buttons
		public static string ActionSpriteSuffix = "XBox";

		public static bool Suspended {
			get {
				return Get.mSuspended;

			}set {
				Get.mSuspended = value;
			}
		}

		public override void Awake ()
		{
			Get = this;		
			base.Awake ();
		}

		protected override void AddDaisyChains ()
		{
			AddAxisChange (MouseXAxis, InterfaceActionType.CursorMove);
			AddAxisChange (MouseYAxis, InterfaceActionType.CursorMove);
		}

		public void SetMousePosition (int x, int y)
		{
			MouseSmoothX.Clear ();
			MouseSmoothY.Clear ();
			if (SoftwareMouse) {
				MousePosition.x = x;
				MousePosition.y = y;
			} else {
				ProMouse.Instance.SetCursorPosition (x, y);
			}
			mSetMousePositionThisFrame = true;
		}

		public void LateUpdate ()
		{
			if (mSetMousePositionThisFrame) {
				mSetMousePositionThisFrame = false;
				return;
			}
			//see if the mouse is moving this frame
			if (Input.GetAxis ("mouse x") != 0f || Input.GetAxis ("mouse y") != 0f) {
				MouseSmoothX.Clear ();
				MouseSmoothY.Clear ();
				ControllerDrivenMouseMovement = false;
			} else if (Profile.Get.CurrentPreferences.Controls.UseControllerMouse && !SuspendCursorControllerMovement) {
				//if it's not moving, our controller drives the mouse
				ControllerDrivenMouseMovement = true;
				if (RawMouseAxisX != 0f || RawMouseAxisY != 0f) {
					int mouseX = 0;
					int mouseY = 0;
					if (SoftwareMouse) {
						mouseX = (int)MousePosition.x + (int)(RawMouseAxisX * ControllerMouseSensitivity * Screen.width);
						mouseY = (int)MousePosition.y + (int)(RawMouseAxisY * ControllerMouseSensitivity * Screen.height);
					} else {
						mouseX = (int)Input.mousePosition.x + (int)(RawMouseAxisX * ControllerMouseSensitivity * Screen.width);
						mouseY = (int)Input.mousePosition.y + (int)(RawMouseAxisY * ControllerMouseSensitivity * Screen.height);
					}

					MouseSmoothX.Add (mouseX);
					MouseSmoothY.Add (mouseY);

					if (MouseSmoothX.Count > ControllerMouseSmoothSteps) {
						MouseSmoothX.RemoveAt (0);
					}
					if (MouseSmoothY.Count > ControllerMouseSmoothSteps) {
						MouseSmoothY.RemoveAt (0);
					}

					mouseX = 0;
					for (int i = 0; i < MouseSmoothX.Count; i++) {
						mouseX += MouseSmoothX [i];
					}

					mouseY = 0;
					for (int i = 0; i < MouseSmoothY.Count; i++) {
						mouseY += MouseSmoothY [i];
					}

					mouseX = mouseX / MouseSmoothX.Count;
					mouseY = mouseY / MouseSmoothY.Count;

					//Debug.Log("Mouse x: " + mouseX.ToString());
					//Debug.Log("Mouse y: " + mouseY.ToString());

					if (SoftwareMouse) {
						//TODO what exactly?
					} else {
						ProMouse.Instance.SetCursorPosition (mouseX, mouseY);
					}
				}
			}
		}

		protected override void OnPushSettings ()
		{
			if (Profile.Get.HasSelectedProfile) {
				Profile.Get.CurrentPreferences.Controls.InterfaceActionSettings.Clear ();
				Profile.Get.CurrentPreferences.Controls.InterfaceActionSettings.AddRange (CurrentActionSettings);
			}
			//figure out what kind of controller we have plugged in
			foreach (InputDevice d in InputManager.Devices) {
				string nameCheck = d.Name.ToLower ();
				if (nameCheck.Contains ("xbox") || nameCheck.Contains ("logitech")) {
					ActionSpriteSuffix = "XBox";
				} else if (nameCheck.Contains ("ps2") || nameCheck.Contains ("ps3")) {
					ActionSpriteSuffix = "PS3";
				} else if (nameCheck.Contains ("steam")) {
					ActionSpriteSuffix = "Steam";
				} else {
					//Debug.Log("Setting action sprite suffix to default");
					ActionSpriteSuffix = Globals.ControllerDefaultActionSpriteSuffix;
				}
				//Debug.Log("Setting action sprite suffix to " + ActionSpriteSuffix);
			}
		}

		protected override void OnUpdate ()
		{
			XStickX = RawInterfaceAxisX;
			XStickY = RawInterfaceAxisY;

			//TEMP TODO this is to get around the fact that mouse scroll wheel is so flakey
			//it won't be necessary eventually...
			if (RawScrollWheelAxis > Globals.MouseScrollSensitivity) {
				Send (InterfaceActionType.SelectionPrev, TimeStamp);
			}
			if (RawScrollWheelAxis < -Globals.MouseScrollSensitivity) {
				Send (InterfaceActionType.SelectionNext, TimeStamp);
			}

			//TEMP - this is where I add 'focus update' functionality
			/*if (GameManager.Is(FGameState.InGame | FGameState.GamePaused) && Player.Local.HasSpawned) {
								bool hitKey = false;
								//get the input string and see if it contains a cheat code
								if (AvailableKeyDown) {
										string nextChar = LastKey.ToString().ToLower();
										if (nextChar.Length == 1) {
												hitKey = true;
												mLastTimeHitKey = WorldClock.RealTime;
												//skip stuff like F1, shift etc
												mCheatCodeSoFar += nextChar;
												//Debug.Log("Cheat code so far: " + mCheatCodeSoFar);
												if (GameManager.FocusOnCheatCode.Equals(mCheatCodeSoFar)) {
														//Debug.Log("Entered cheat code, resetting");
														Skills.LearnSkill("Trapping");
														Skills.LearnSkill("Fishing");

														Frontiers.World.GenericWorldItem genericTrap = new Frontiers.World.GenericWorldItem();
														genericTrap.PackName = "Tools";
														genericTrap.PrefabName = "Animal Trap";
														Frontiers.World.StackItem stackItemTrap = genericTrap.ToStackItem();
														Frontiers.World.WorldItem trap = null;
														if (Frontiers.World.WorldItems.CloneFromStackItem(stackItemTrap, WIGroups.Get.Player, out trap)) {
																Debug.Log("Adding animal trap");
																trap.Props.Local.CraftedByPlayer = true;
																trap.Initialize();
																trap.ActiveState = WIActiveState.Active;
																trap.Props.Local.FreezeOnStartup = false;
																trap.tr.rotation = Quaternion.identity;
																trap.SetMode(WIMode.World);
																trap.tr.position = Player.Local.ItemPlacement.GrabberIdealPosition;
																trap.LastActiveDistanceToPlayer = 0f;
																//then force the player to carry the item
																WIStackError error = WIStackError.None;
																if (!Player.Local.Inventory.AddItems(trap, ref error)) {
																		Debug.Log("Couldn't add animal trap to inventory");
																}
														} else {
																Debug.Log("Couldn't add animal trap");
														}

														genericTrap.PrefabName = "Fishnet 2";
														stackItemTrap = genericTrap.ToStackItem();
														if (Frontiers.World.WorldItems.CloneFromStackItem(stackItemTrap, WIGroups.Get.Player, out trap)) {
																Debug.Log("Adding fish trap");
																trap.Props.Local.CraftedByPlayer = true;
																trap.Initialize();
																trap.ActiveState = WIActiveState.Active;
																trap.Props.Local.FreezeOnStartup = false;
																trap.tr.rotation = Quaternion.identity;
																trap.SetMode(WIMode.World);
																trap.tr.position = Player.Local.ItemPlacement.GrabberIdealPosition;
																trap.LastActiveDistanceToPlayer = 0f;
																//then force the player to carry the item
																WIStackError error = WIStackError.None;
																if (!Player.Local.Inventory.AddItems(trap, ref error)) {
																		Debug.Log("Couldn't add fish trap to inventory");
																}
														} else {
																Debug.Log("Couldn't add fish trap");
														}

														mCheatCodeSoFar = string.Empty;
												} else if (!GameManager.FocusOnCheatCode.Contains(mCheatCodeSoFar)) {
														//Debug.Log("Whoops, last char broke cheat code: " + mCheatCodeSoFar);
														mCheatCodeSoFar = string.Empty;
												}
										}
								}

								if (!hitKey && WorldClock.RealTime > mLastTimeHitKey + 3) {
										//Debug.Log("Resetting cheat code");
										mCheatCodeSoFar = string.Empty;
										mLastTimeHitKey = WorldClock.RealTime;
								}
						}*/
		}

		protected string mCheatCodeSoFar = string.Empty;
		protected double mLastTimeHitKey = 0f;
		protected bool mSetMousePositionThisFrame = false;
		public float XStickX;
		public float XStickY;

		public override List<ActionSetting> GenerateDefaultActionSettings ()
		{
			MouseXAxis = InputControlType.RightStickX;
			MouseYAxis = InputControlType.RightStickY;
			ScrollWheelAxis = InputControlType.DPadX;

			List <ActionSetting> actionSettings = new List<ActionSetting> ();
			ActionSetting aSetting = null;

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Interface Mouse Y";
			aSetting.Controller = MouseYAxis;
			aSetting.AvailableControllerButtons = DefaultAvailableAxis;
			aSetting.Axis = ActionSetting.InputAxis.MouseY;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Interface Mouse X";
			aSetting.Controller = MouseXAxis;
			aSetting.AvailableControllerButtons = DefaultAvailableAxis;
			aSetting.Axis = ActionSetting.InputAxis.MouseX;
			actionSettings.Add (aSetting);

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
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Button Left";
			aSetting.Action = (int)InterfaceActionType.SelectionLeft;
			aSetting.Controller = InputControlType.LeftStickLeft;
			aSetting.Key = KeyCode.LeftArrow;
			aSetting.Axis = ActionSetting.InputAxis.InterfaceLeft;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.OpposingActions = new int [] {
				(int)InterfaceActionType.SelectionDown,
				(int)InterfaceActionType.SelectionUp,
				(int)InterfaceActionType.SelectionRight,
			};
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Button Right";
			aSetting.Action = (int)InterfaceActionType.SelectionRight;
			aSetting.Controller = InputControlType.LeftStickRight;
			aSetting.Key = KeyCode.RightArrow;
			aSetting.Axis = ActionSetting.InputAxis.InterfaceRight;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.OpposingActions = new int [] {
				(int)InterfaceActionType.SelectionDown,
				(int)InterfaceActionType.SelectionLeft,
				(int)InterfaceActionType.SelectionUp,
			};
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Button Up";
			aSetting.Action = (int)InterfaceActionType.SelectionUp;
			aSetting.Controller = InputControlType.LeftStickUp;
			aSetting.Key = KeyCode.UpArrow;
			aSetting.Axis = ActionSetting.InputAxis.InterfaceUp;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.OpposingActions = new int [] {
				(int)InterfaceActionType.SelectionDown,
				(int)InterfaceActionType.SelectionLeft,
				(int)InterfaceActionType.SelectionRight,
			};
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.ActionDescription = "Button Down";
			aSetting.Action = (int)InterfaceActionType.SelectionDown;
			aSetting.Controller = InputControlType.LeftStickDown;
			aSetting.Key = KeyCode.DownArrow;
			aSetting.Axis = ActionSetting.InputAxis.InterfaceDown;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.OpposingActions = new int [] {
				(int)InterfaceActionType.SelectionUp,
				(int)InterfaceActionType.SelectionLeft,
				(int)InterfaceActionType.SelectionRight,
			};
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.ToggleInventory;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.ToggleInventory.ToString ());
			aSetting.Controller = InputControlType.Button3;
			aSetting.Key = KeyCode.Tab;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.ToggleLog;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.ToggleLog.ToString ());
			aSetting.Controller = InputControlType.Button2;
			aSetting.Key = KeyCode.L;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.ToggleMap;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.ToggleMap.ToString ());
			aSetting.Controller = InputControlType.Button1;
			aSetting.Key = KeyCode.M;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.ToggleInterfaceNext;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.ToggleInterfaceNext.ToString ());
			aSetting.Controller = InputControlType.Start;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.Action = (int)InterfaceActionType.CursorClick;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.CursorClick.ToString ());
			aSetting.Controller = InputControlType.Action1;
			aSetting.Mouse = ActionSetting.MouseAction.Left;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.Key = KeyCode.Return;
			aSetting.Cursor = ActionSetting.CursorAction.Click;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Analog;
			aSetting.Action = (int)InterfaceActionType.CursorRightClick;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.CursorRightClick.ToString ());
			aSetting.Controller = InputControlType.Action2;
			aSetting.Mouse = ActionSetting.MouseAction.Right;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			aSetting.Key = KeyCode.Quote;
			aSetting.Cursor = ActionSetting.CursorAction.RightClick;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.InterfaceHide;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.InterfaceHide.ToString ());
			aSetting.Controller = InputControlType.Button10;
			aSetting.Key = KeyCode.F2;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			actionSettings.Add (aSetting);

			aSetting = ActionSetting.Button;
			aSetting.Action = (int)InterfaceActionType.StackSplit;
			aSetting.ActionDescription = Data.GameData.AddSpacesToSentence (InterfaceActionType.StackSplit.ToString ());
			aSetting.Controller = InputControlType.RightTrigger;
			aSetting.Key = KeyCode.LeftControl;
			aSetting.AvailableControllerButtons = DefaultAvailableActions;
			aSetting.AvailableKeys = DefaultAvailableKeys;
			actionSettings.Add (aSetting);

			return actionSettings;
		}

		public static void GetKeyCodeLabelText (KeyCode key, bool longForm, out string text, out bool wideFormat)
		{
			wideFormat = false;
			text = string.Empty;
			switch (key) {
//								case KeyCode.LeftApple:
			case KeyCode.LeftCommand:
				text = longForm ? "L Comm" : "LCom";
				wideFormat = true;
				break;
//
//								case KeyCode.RightApple:
			case KeyCode.RightCommand:
				text = longForm ? "R Comm" : "RCom";
				wideFormat = true;
				break;

			case KeyCode.LeftShift:
				text = longForm ? "L Shift" : "LShf";
				wideFormat = true;
				break;

			case KeyCode.RightShift:
				text = longForm ? "R Shift" : "RShf";
				wideFormat = true;
				break;

			case KeyCode.Escape:
				text = longForm ? "Escape" : "Esc";
				wideFormat = true;
				break;

			case KeyCode.Delete:
				text = longForm ? "Delete" : "Del";
				wideFormat = true;
				break;

			case KeyCode.Space:
				text = longForm ? "Space" : "Spc";
				wideFormat = true;
				break;

			case KeyCode.Backspace:
				text = longForm ? "Back" : "Bck";
				wideFormat = true;
				break;

			case KeyCode.Alpha0:
				text = "0";
				wideFormat = false;
				break;

			case KeyCode.Alpha1:
				text = "1";
				wideFormat = false;
				break;

			case KeyCode.Alpha2:
				text = "2";
				wideFormat = false;
				break;

			case KeyCode.Alpha3:
				text = "3";
				wideFormat = false;
				break;

			case KeyCode.Alpha4:
				text = "4";
				wideFormat = false;
				break;

			case KeyCode.Alpha5:
				text = "5";
				wideFormat = false;
				break;

			case KeyCode.Alpha6:
				text = "6";
				wideFormat = false;
				break;

			case KeyCode.Alpha7:
				text = "7";
				wideFormat = false;
				break;

			case KeyCode.Alpha8:
				text = "8";
				wideFormat = false;
				break;

			case KeyCode.Alpha9:
				text = "9";
				wideFormat = false;
				break;

			case KeyCode.Keypad0:
				text = "K0";
				wideFormat = true;
				break;

			case KeyCode.Keypad1:
				text = "K1";
				wideFormat = true;
				break;

			case KeyCode.Keypad2:
				text = "K2";
				wideFormat = true;
				break;

			case KeyCode.Keypad3:
				text = "K3";
				wideFormat = true;
				break;

			case KeyCode.Keypad4:
				text = "K4";
				wideFormat = true;
				break;

			case KeyCode.Keypad5:
				text = "K5";
				wideFormat = true;
				break;

			case KeyCode.Keypad6:
				text = "K6";
				wideFormat = true;
				break;

			case KeyCode.Keypad7:
				text = "K7";
				wideFormat = true;
				break;

			case KeyCode.Keypad8:
				text = "K8";
				wideFormat = true;
				break;

			case KeyCode.Keypad9:
				text = "K9";
				wideFormat = true;
				break;

			case KeyCode.CapsLock:
				text = "Caps";
				wideFormat = true;
				break;

			case KeyCode.Comma:
				text = ",";
				wideFormat = false;
				break;

			case KeyCode.Colon:
				text = ":";
				wideFormat = false;
				break;

			case KeyCode.DownArrow:
				text = longForm ? "Down" : "Dwn";
				wideFormat = true;
				break;

			case KeyCode.UpArrow:
				text = "Up";
				wideFormat = true;
				break;

			case KeyCode.LeftArrow:
				text = longForm ? "Left" : "Lft";
				wideFormat = true;
				break;

			case KeyCode.RightArrow:
				text = longForm ? "Right" : "Rgt";
				wideFormat = true;
				break;

			case KeyCode.PageUp:
				text = longForm ? "Pg Up" : "PUp";
				wideFormat = true;
				break;

			case KeyCode.PageDown:
				text = longForm ? "Pg Down" : "PDwn";
				wideFormat = true;
				break;

			case KeyCode.Period:
				text = ".";
				wideFormat = false;
				break;

			case KeyCode.Plus:
				text = "+";
				wideFormat = false;
				break;

			case KeyCode.Minus:
				text = "-";
				wideFormat = false;
				break;

			case KeyCode.KeypadPlus:
				text = "K+";
				wideFormat = true;
				break;

			case KeyCode.KeypadMinus:
				text = "K-";
				wideFormat = true;
				break;

			case KeyCode.Ampersand:
				text = "&";
				wideFormat = false;
				break;

			case KeyCode.Equals:
				text = "=";
				wideFormat = false;
				break;

			case KeyCode.F1:
			case KeyCode.F2:
			case KeyCode.F3:
			case KeyCode.F4:
			case KeyCode.F5:
			case KeyCode.F6:
			case KeyCode.F7:
			case KeyCode.F8:
			case KeyCode.F9:
			case KeyCode.F10:
			case KeyCode.F11:
			case KeyCode.F12:
			case KeyCode.F13:
			case KeyCode.F14:
			case KeyCode.F15:
				text = key.ToString ();
				wideFormat = true;
				break;

			case KeyCode.Backslash:
				text = "\\";
				wideFormat = false;
				break;

			case KeyCode.Slash:
				text = "/";
				wideFormat = false;
				break;

			case KeyCode.LeftBracket:
				text = "[";
				wideFormat = false;
				break;

			case KeyCode.RightBracket:
				text = "]";
				wideFormat = false;
				break;

			case KeyCode.Numlock:
				text = longForm ? "Numlock" : "Num";
				wideFormat = true;
				break;

			case KeyCode.Quote:
				text = "'";
				wideFormat = false;
				break;

			case KeyCode.Return:
				text = longForm ? "Return" : "Ret";
				wideFormat = true;
				break;

			case KeyCode.RightAlt:
				text = longForm ? "R Alt" : "RAlt";
				wideFormat = true;
				break;

			case KeyCode.LeftAlt:
				text = longForm ? "L Alt" : "LAlt";
				wideFormat = true;
				break;

			case KeyCode.Tab:
				text = "Tab";
				wideFormat = true;
				break;

			case KeyCode.LeftControl:
				text = longForm ? "L Ctrl" : "LCtl";
				wideFormat = true;
				break;

			case KeyCode.RightControl:
				text = longForm ? "R Ctrl" : "RCtl";
				wideFormat = true;
				break;

			default:
				text = key.ToString ();
				wideFormat = false;
				break;
			}

		}

		protected List <int> MouseSmoothX = new List<int> ();
		protected List <int> MouseSmoothY = new List<int> ();
	}
}
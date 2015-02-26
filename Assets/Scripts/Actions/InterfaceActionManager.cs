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
				//this is the sprite suffix for controller buttons
				public static string ActionSpriteSuffix = "XBox";

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

				public void SetMousePosition(int x, int y)
				{
						MouseSmoothX.Clear();
						MouseSmoothY.Clear();
						ProMouse.Instance.SetCursorPosition(x, y);
				}

				public void LateUpdate()
				{
						//see if the mouse is moving this frame
						if (Input.GetAxis("mouse x") != 0f || Input.GetAxis("mouse y") != 0f) {
								MouseSmoothX.Clear();
								MouseSmoothY.Clear();
								ControllerDrivenMouseMovement = false;
						} else if (Profile.Get.CurrentPreferences.Controls.UseControllerMouse) {
								//if it's not moving, our controller drives the mouse
								ControllerDrivenMouseMovement = true;
								if (RawMouseAxisX != 0f || RawMouseAxisY != 0f) {
										int mouseX = (int)Input.mousePosition.x + (int)(RawMouseAxisX * ControllerMouseSensitivity * Screen.width);
										int mouseY = (int)Input.mousePosition.y + (int)(RawMouseAxisY * ControllerMouseSensitivity * Screen.height);

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

										//Debug.Log("Mouse x: " + mouseX.ToString());
										//Debug.Log("Mouse y: " + mouseY.ToString());

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
						//figure out what kind of controller we have plugged in
						foreach (InputDevice d in InputManager.Devices) {
								string nameCheck = d.Name.ToLower();
								if (nameCheck.Contains("xbox") || nameCheck.Contains("logitech")) {
										ActionSpriteSuffix = "XBox";
								} else if (nameCheck.Contains("ps2") || nameCheck.Contains("ps3")) {
										ActionSpriteSuffix = "PS3";
								} else if (nameCheck.Contains("steam")) {
										ActionSpriteSuffix = "Steam";
								} else {
										//Debug.Log("Setting action sprite suffix to default");
										ActionSpriteSuffix = Globals.ControllerDefaultActionSpriteSuffix;
								}
								//Debug.Log("Setting action sprite suffix to " + ActionSpriteSuffix);
						}
				}

				protected override void OnUpdate()
				{
						XStickX = RawInterfaceAxisX;
						XStickY = RawInterfaceAxisY;

						//TEMP TODO this is to get around the fact that mouse scroll wheel is so flakey
						//it won't be necessary eventually...
						if (RawScrollWheelAxis > Globals.MouseScrollSensitivity) {
								Send(InterfaceActionType.SelectionPrev, TimeStamp);
						}
						if (RawScrollWheelAxis < -Globals.MouseScrollSensitivity) {
								Send(InterfaceActionType.SelectionNext, TimeStamp);
						}

						//TEMP - this is where I add 'focus update' functionality
						if (GameManager.Is(FGameState.InGame | FGameState.GamePaused) && Player.Local.HasSpawned) {
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
						}
				}

				protected string mCheatCodeSoFar = string.Empty;
				protected double mLastTimeHitKey = 0f;
				public float XStickX;
				public float XStickY;

				public override List<ActionSetting> GenerateDefaultActionSettings()
				{
						MouseXAxis = InputControlType.RightStickX;
						MouseYAxis = InputControlType.RightStickY;
						MovementXAxis = InputControlType.DPadX;
						MovementYAxis = InputControlType.DPadY;
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
						aSetting.ActionDescription = "Interface Movement Y";
						aSetting.Controller = MovementXAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MovementY;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Interface Movement X";
						aSetting.Controller = MovementYAxis;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.Axis = ActionSetting.InputAxis.MovementX;
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

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Button L / R";
						aSetting.ActionOnX = (int)InterfaceActionType.SelectionLeft;
						aSetting.ActionOnY = (int)InterfaceActionType.SelectionRight;
						aSetting.Controller = InputControlType.LeftStickX;
						aSetting.KeyX = KeyCode.LeftArrow;
						aSetting.KeyY = KeyCode.RightArrow;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.Axis = ActionSetting.InputAxis.InterfaceX;
						actionSettings.Add(aSetting);

						aSetting = ActionSetting.Analog;
						aSetting.ActionDescription = "Button U / D";
						aSetting.ActionOnX = (int)InterfaceActionType.SelectionDown;
						aSetting.ActionOnY = (int)InterfaceActionType.SelectionUp;
						aSetting.Controller = InputControlType.LeftStickY;
						aSetting.KeyX = KeyCode.DownArrow;
						aSetting.KeyY = KeyCode.UpArrow;
						aSetting.AvailableControllerButtons = DefaultAvailableAxis;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						aSetting.Axis = ActionSetting.InputAxis.InterfaceY;
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

						aSetting = ActionSetting.Button;
						aSetting.Action = (int)InterfaceActionType.InterfaceHide;
						aSetting.ActionDescription = Data.GameData.AddSpacesToSentence(InterfaceActionType.InterfaceHide.ToString());
						aSetting.Controller = InputControlType.Button10;
						aSetting.Key = KeyCode.F2;
						aSetting.AvailableControllerButtons = DefaultAvailableActions;
						aSetting.AvailableMouseButtons = DefaultAvailableMouseButtons;
						aSetting.AvailableKeys = DefaultAvailableKeys;
						actionSettings.Add(aSetting);

						return actionSettings;
				}

				protected List <int> MouseSmoothX = new List<int>();
				protected List <int> MouseSmoothY = new List<int>();
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.GUI;
using Frontiers.Data;
using System;

namespace Frontiers.GUI
{
		public class GUIManager : Manager
		{
				public static GUIManager Get;

				public override string GameObjectName {
						get {
								return "Frontiers_GUIManager";
						}
				}

				public static bool SuspendMessages {
						get {
								if (Cutscene.IsActive) {
										return true;
								}
								if (Get.TopInterface != null) {
										return Get.TopInterface.SuspendMessages;
								}
								return false;
						}
				}

				public static bool ImageFocus {
						get {
								return CameraFX.Get.Default.Blur.enabled;
						}
						set {
								CameraFX.Get.Default.Blur.enabled = value;
						}
				}

				public static bool ManuallyPaused = false;
				public bool PrimaryInterfacesEnabled = true;

				bool HasActiveButton {
						get {
								return ActiveButton != null;
						}
				}

				public static bool ShowCursor {
						get {
								if (ManuallyPaused) {
										return true;
								} else if (Get.HasActiveInterface) {
										return Get.TopInterface.ShowCursor;
								} else if (Cutscene.IsActive) {
										return Cutscene.CurrentCutscene.ShowCursor;
								} else if (Get.HasActiveButton) {
										return Get.ActiveButton.ShowCursor;
								}
								return false;
						}
				}

				public static bool HideCrosshair {
						get {
								if (!Player.HideCrosshair) {
										return false;
								}

								if (Get.HasActiveInterface) {
										return Get.TopInterface.HideCrosshair;
								} else if (Cutscene.IsActive) {
										return true;
								}
								return false;
						}
				}

				public List <GameObject> DialogPrefabs = new List <GameObject>();
				public List <GameObject> GUIComponentPrefabs = new List <GameObject>();
				//Main interface
				//TODO move anchors into dictionary
				public UILabel VersionNumber;
				public UILabel PausedLabel;
				public Camera PrimaryCamera;
				public Camera SecondaryCamera;
				public Camera BaseCamera;
				public Camera HudCamera;
				public UICamera NGUIPrimaryCamera;
				public UIRoot NGUIPrimaryRoot;
				public UIAnchor NGUIPrimaryCenterAnchor;
				public UIAnchor NGUIPrimaryBottomAnchor;
				public UICamera NGUISecondaryCamera;
				public UIRoot NGUISecondaryRoot;
				public UIAnchor NGUISecondaryCenterAnchor;
				public UIAnchor NGUISecondaryLeftAnchor;
				public UIAnchor NGUISecondaryBottomAnchor;
				public UICamera NGUIBaseCamera;
				public UIRoot NGUIBaseRoot;
				public UIAnchor NGUIBaseCenterAnchor;
				public UIAnchor NGUIBaseBottomAnchor;
				public UIAnchor NGUITrash;
				public GUICrosshair Crosshair;
				public UILabel TitleCardLabel;
				//Editors
				//TODO move these into DialogPrefabs array
				public GameObject NGUIBookReader;
				public GameObject NGUIOptionsListDialog;
				public GameObject NGUIStringDialog;
				public GameObject NGUIYesNoCancelDialog;
				public GameObject NGUIProgressDialog;
				public GameObject NGUICircleBrowserGeneric;
				public GameObject NGUIMessageCancelDialog;
				public GameObject NGUISelectProfileDialog;
				public GameObject NGUILoadGameDialog;
				public GameObject NGUISaveGameDialog;
				public GameObject NGUINewGameDialog;
				public GameObject NGUIOptionsDialog;
				public GameObject NGUIMultiplayerDialog;
				public GameObject NGUIStartMenu;
				public GameObject NGUIBarter;
				//Pieces
				//TODO move these into array
				public GameObject InventoryBank;
				public GameObject InventorySquare;
				public GameObject InventorySquareDisplay;
				public GameObject InventorySquareEnabler;
				public GameObject InventorySquareEnablerDisplay;
				public GameObject InventorySquareCurrencySmall;
				public GameObject InventorySquareCurrencyLarge;
				public GameObject InventorySquareBarterOffer;
				public GameObject InventorySquareBarterGoods;
				public GameObject InventorySquareWearable;
				public GameObject InventorySquareCraftingResult;
				public GameObject InventorySquareCrafting;
				public GameObject StackContainerDisplay;
				public GameObject BarterContainerDisplay;
				public GameObject BlueprintSquareDisplay;
				public GameObject SkillObject;
				//Message Display
				public GUIMessageDisplay NGUIMessageDisplay;
				public GUIIntrospectionDisplay NGUIIntrospectionDisplay;
				//cursor control
				public GUIButtonHover ActiveButton;
				public DebugConsole Console;
				public MissionTestingUtility Missions;
				public GroupTestingUtility GroupTesting;
				protected GUIControlsCheatSheetDialog mControlCheatSheet;
				#if UNITY_EDITOR
				public int ScreenAspectRatioMaxVR = 900;
				#endif
				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						Console = GameManager.Get.GameCamera.GetComponent <DebugConsole>();
						Missions = GameManager.Get.GameCamera.GetComponent <MissionTestingUtility>();
						GroupTesting = GameManager.Get.GameCamera.GetComponentInParent <GroupTestingUtility>();
						GroupTesting.enabled = false;
						VersionNumber.text = "Frontiers Beta v." + GameManager.Version;
						PrimaryCamera = NGUIPrimaryCamera.cachedCamera;
						//UNITY turn off the damn OnMouse events
						Camera[] cameras = FindObjectsOfType <Camera>();
						foreach (Camera cam in cameras) {
								cam.eventMask = 0;
						}
						//set up our cursor to listen for interface movements
				}

				public override void Initialize()
				{
						UserActionManager.InterfaceReceiver = new ActionReceiver <UserActionType>(ReceiveUserAction);
						InterfaceActionManager.InterfaceReceiver = new ActionReceiver <InterfaceActionType>(ReceiveInterfaceAction);
						//GUITabs.InitializeAllTabs ();
						PrimaryInterface.MinimizeAll();

						mInitialized = true;
				}

				public override void OnCutsceneStart()
				{
						Get.NGUIMessageDisplay.HideImmediately();
				}

				public void ClearFocus()
				{
						for (int i = SecondaryInterfaces.items.LastIndex(); i >= 0; i--) {
								if (SecondaryInterfaces.items[i] != null) {
										SecondaryInterfaces.items[i].HasFocus = false;
								}
						}
						for (int i = 0; i < PrimaryInterfaces.Count; i++) {
								if (PrimaryInterfaces[i] != null) {
										PrimaryInterfaces[i].HasFocus = false;
								}
						}
						for (int i = 0; i < BaseInterfaces.Count; i++) {
								if (BaseInterfaces[i] != null) {
										BaseInterfaces[i].HasFocus = false;
								}
						}
				}

				public float ScreenAspectRatio {
						get {
								return (((float)Screen.width) / ((float)Screen.height));
						}
				}

				public static int Frame;

				public void Update()
				{
						Frame++;

						if (!mInitialized) {
								return;
						}

						if (GameManager.Is(FGameState.InGame)) {
								if (Input.GetKeyDown(KeyCode.F3)) {
										if (mControlCheatSheet == null) {
												GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIControlsCheatSheetDialog"), false);
												YesNoCancelDialogResult editObject = new YesNoCancelDialogResult();
												GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(new ChildEditorCallback <YesNoCancelDialogResult>(ControlDialogCallback), dialog, editObject);
												mControlCheatSheet = dialog.GetComponent <GUIControlsCheatSheetDialog>();
										}
								}
						}

						if (Input.GetKeyDown(KeyCode.F5)) {
								Missions.ShowEditor = !Missions.ShowEditor;
								Missions.enabled = Missions.ShowEditor;
						}

						if (Input.GetKeyDown(KeyCode.F6)) {
								GroupTesting.enabled = !GroupTesting.enabled;
						}

						if (Input.GetKeyDown(KeyCode.F7)) {
								Console.showWorldItems = !Console.showWorldItems;
						}

						if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F1)) {
								Console.enabled = !Console.enabled;
								UserActionManager.Suspended = Console.enabled;
								InterfaceActionManager.Suspended = Console.enabled;
						}

						//this is kind of a kludge but we want scrollbars to work whenever they're being hovered over
						//so if there's a scrollbar being hovered over OR a draggable panel, we want to grab it
						if (UICamera.hoveredObject == null) {
								mHoveredObjectParent = null;
								mActiveScrollBar = null;
								mActiveDraggablePanel = null;
								mActiveSlider = null;
						} else if (UICamera.hoveredObject.CompareTag(Globals.TagActiveObject)) {
								//first check to see if we have a slider
								if (mActiveSlider == null || mActiveSlider.gameObject != UICamera.hoveredObject) {
										mActiveSlider = UICamera.hoveredObject.GetComponent <UISlider>();
								}
								//if that doesn't work see if it's a scrollbar
								if (mActiveSlider == null) {
										mHoveredObjectParent = UICamera.hoveredObject.transform.parent.gameObject;
										mActiveScrollBar = mHoveredObjectParent.GetComponent <UIScrollBar>();
								}
								//finally, if THAT doesn't work see if it's a scrolling panel
								if (mActiveSlider == null) {
										if (UICamera.hoveredObject.HasComponent <UIDraggablePanel>(out mActiveDraggablePanel)) {
												mActiveScrollBar = mActiveDraggablePanel.verticalScrollBar;
										}
								}
						} else if (UICamera.hoveredObject.CompareTag(Globals.TagBrowserObject)) {
								GUIBrowserObject.GetBrowserObjectScrollbar(UICamera.hoveredObject.transform, out mActiveScrollBar);
						} else {
								mHoveredObjectParent = null;
								mActiveScrollBar = null;
								mActiveDraggablePanel = null;
								mActiveSlider = null;
						}

						//this is ALSO a kludge, we want to intercept key actions if an input field is active
						if (UICamera.selectedObject != null && UICamera.selectedObject.CompareTag(Globals.TagGuiInputObject)) {
								InterfaceActionManager.InputFieldActive = true;
								UserActionManager.InputFieldActive = true;
						} else {
								InterfaceActionManager.InputFieldActive = false;
								UserActionManager.InputFieldActive = false;
						}

						if (HasActiveSecondaryInterface) {
								NGUISecondaryCamera.useMouse = true;
								NGUISecondaryCamera.useKeyboard = true;
								NGUIPrimaryCamera.useMouse = false;
								NGUIPrimaryCamera.useKeyboard = false;
								NGUIBaseCamera.useMouse = false;
								NGUIBaseCamera.useKeyboard = false;

								NGUISecondaryCamera.enabled = true;

						} else if (HasActivePrimaryInterface) {
								NGUISecondaryCamera.useMouse = false;
								NGUISecondaryCamera.useKeyboard = false;
								NGUIPrimaryCamera.useMouse = true;
								NGUIPrimaryCamera.useKeyboard = true;
								NGUIBaseCamera.useMouse = false;
								NGUIBaseCamera.useKeyboard = false;

								NGUISecondaryCamera.enabled = false;

						} else if (HasActiveBaseInterface) {
								NGUISecondaryCamera.useMouse = false;
								NGUISecondaryCamera.useKeyboard = false;
								NGUIPrimaryCamera.useMouse = false;
								NGUIPrimaryCamera.useKeyboard = false;
								NGUIBaseCamera.useMouse = true;
								NGUIBaseCamera.useKeyboard = true;

								NGUISecondaryCamera.enabled = false;
						} else {
								NGUISecondaryCamera.useMouse = false;
								NGUISecondaryCamera.useKeyboard = false;
								NGUIPrimaryCamera.useMouse = false;
								NGUIPrimaryCamera.useKeyboard = false;
								NGUIBaseCamera.useMouse = false;
								NGUIBaseCamera.useKeyboard = true;

								NGUISecondaryCamera.enabled = false;
						}

						PrimaryCamera.enabled = PrimaryInterfacesEnabled;
						VersionNumber.enabled = true;

						#if UNITY_EDITOR
						if ((VRManager.VRMode | VRManager.VRTestingModeEnabled)) {
								VersionNumber.enabled = false;
								NGUIPrimaryRoot.manualHeight = ScreenAspectRatioMaxVR;
								NGUISecondaryRoot.manualHeight = ScreenAspectRatioMaxVR;
								NGUIBaseRoot.manualHeight = ScreenAspectRatioMaxVR;
								GUILoading.Get.Root.manualHeight = ScreenAspectRatioMaxVR;
								#else
						if (VRManager.VRMode) {
							VersionNumber.enabled = false;
							NGUIPrimaryRoot.manualHeight = Globals.ScreenAspectRatioMaxVR;
							NGUISecondaryRoot.manualHeight = Globals.ScreenAspectRatioMaxVR;
							NGUIBaseRoot.manualHeight = Globals.ScreenAspectRatioMaxVR;
							GUILoading.Get.Root.manualHeight = Globals.ScreenAspectRatioMaxVR;
								#endif
						} else {
								if (ScreenAspectRatio < Globals.ScreenAspectRatioSqueezeMaximum) {
										//adjust the screen to fit
										float normalizedScreenAdjust = (Globals.ScreenAspectRatioSqueezeMaximum - ScreenAspectRatio) / (Globals.ScreenAspectRatioSqueezeMaximum - Globals.ScreenAspectRatioSqueezeMinimum);
										NGUIPrimaryRoot.manualHeight = Mathf.FloorToInt(Mathf.Lerp(Globals.ScreenAspectRatioMax, Globals.ScreenAspectRatioMin, normalizedScreenAdjust));
										NGUISecondaryRoot.manualHeight = NGUIPrimaryRoot.manualHeight;
										NGUIBaseRoot.manualHeight = NGUIPrimaryRoot.manualHeight;
										GUILoading.Get.Root.manualHeight = NGUIPrimaryRoot.manualHeight;
								} else {
										NGUIPrimaryRoot.manualHeight = Globals.ScreenAspectRatioMax;
										NGUISecondaryRoot.manualHeight = Globals.ScreenAspectRatioMax;
										NGUIBaseRoot.manualHeight = Globals.ScreenAspectRatioMax;
										GUILoading.Get.Root.manualHeight = Globals.ScreenAspectRatioMax;
								}
						}

						if (ManuallyPaused) {
								if (GameManager.Is(FGameState.InGame)) {
										GameManager.Pause();
								}
								//if we don't have any active interfaces going
								//enable our 'paused' display
								PausedLabel.enabled = true;
						} else {
								PausedLabel.enabled = false;
								if (GameManager.Is(FGameState.GamePaused)) {
										if (HasActiveInterface) {
												switch (TopInterface.Pause) {
														case PauseBehavior.DoNotPause:
																GameManager.Continue();
																break;

														case PauseBehavior.PassThrough:
														//see if the next interface pauses
																break;

														default:
																break;
												}
										} else {
												////Debug.Log ("We're paused and we have no top interface");
												GameManager.Continue();
										}
								} else if (GameManager.Is(FGameState.InGame)) {
										if (HasActiveInterface) {
												switch (TopInterface.Pause) {
														case PauseBehavior.Pause:
																GameManager.Pause();
																break;

														case PauseBehavior.PassThrough:
														//see if the next interface pauses
																break;

														default:
																break;
												}
										}
								}
						}

						RefreshObjects();

						#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(this);
						#endif
				}

				public void LateUpdate()
				{
						//this has to be called each frame
						//to clear its toggle interface action
						PrimaryInterface.ResetToggle();
				}

				protected void ControlDialogCallback(YesNoCancelDialogResult editObject, IGUIChildEditor <YesNoCancelDialogResult> childEditor)
				{
						GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
						//if the result is yes, open up the options dialog
						if (editObject.Result == DialogResult.Yes) {
								if (PrimaryInterface.MinimizeAll()) {
										StartMenuResult result = new StartMenuResult();
										result.ClickedOptions = true;
										result.TabSelection = "Controls";
										GameManager.Get.SpawnStartMenu(GameManager.State, GameManager.State, result);
								}
						}
				}

				protected Transform mBrowserObjectTransform;
				protected GameObject mHoveredObjectParent;
				protected UIScrollBar mActiveScrollBar;
				protected UISlider mActiveSlider;
				protected UIDraggablePanel mActiveDraggablePanel;

				#region refreshing

				public static void RefreshObject(GUIObject guiObject)
				{
						if (gRefreshingGUIObjects) {
								gObjectsToRefreshNextFrame.SafeAdd(guiObject);
						} else {
								gObjectsToRefresh.SafeAdd(guiObject);
						}
				}

				protected void RefreshObjects()
				{
						gRefreshingGUIObjects = true;
						for (int i = 0; i < gObjectsToRefresh.Count; i++) {
								//foreach (GUIObject objectToRefresh in gObjectsToRefresh) {
								if (gObjectsToRefresh[i] != null && !gObjectsToRefresh[i].IsDestroyed) {
										gObjectsToRefresh[i].Refresh();
								}
						}
						gObjectsToRefresh.Clear();
						for (int i = 0; i < gObjectsToRefreshNextFrame.Count; i++) {
								//foreach (GUIObject objectToRefreshNextFrame in gObjectsToRefreshNextFrame) {
								gObjectsToRefresh.Add(gObjectsToRefreshNextFrame[i]);
						}
						gObjectsToRefreshNextFrame.Clear();
						gRefreshingGUIObjects = false;
				}

				protected static List <GUIObject> gObjectsToRefresh = new List <GUIObject>();
				protected static List <GUIObject> gObjectsToRefreshNextFrame = new List <GUIObject>();
				protected static bool gRefreshingGUIObjects = false;

				#endregion

				#region interfaces

				public SemiStack <FrontiersInterface> SecondaryInterfaces = new SemiStack <FrontiersInterface>();
				public List <FrontiersInterface> PrimaryInterfaces = new List <FrontiersInterface>();
				public List <FrontiersInterface> BaseInterfaces = new List <FrontiersInterface>();

				public FrontiersInterface LastActiveSecondaryInterface {
						get {
								return SecondaryInterfaces.Peek();
						}
				}

				public FrontiersInterface LastActivePrimaryInterface = null;
				public FrontiersInterface LastActiveBaseInterface = null;

				public FrontiersInterface TopInterface {
						get {
								if (HasActiveSecondaryInterface) {
										return LastActiveSecondaryInterface;
								} else if (HasActivePrimaryInterface) {
										return LastActivePrimaryInterface;
								} else if (HasActiveBaseInterface) {
										return LastActiveBaseInterface;
								}
								return null;
						}
				}

				public Camera ActiveCamera {
						get {
								if (HasActiveInterface) {
										return TopInterface.NGUICamera;
								}
								return BaseCamera;
						}
				}

				public bool HasActiveSecondaryInterface {
						get {
								if (SecondaryInterfaces.Count > 0) {
										for (int i = SecondaryInterfaces.items.LastIndex(); i >= 0; i--) {
												if (SecondaryInterfaces.items[i] == null || SecondaryInterfaces.items[i].IsFinished || SecondaryInterfaces.items[i].IsDestroyed) {
														SecondaryInterfaces.items.RemoveAt(i);
												}
										}
								}
								return SecondaryInterfaces.Count > 0;
						}
				}

				public bool HasActivePrimaryInterface {
						get {
								bool result = false;
								for (int i = 0; i < PrimaryInterfaces.Count; i++) {
										if (PrimaryInterfaces[i].HasFocus) {
												LastActivePrimaryInterface = PrimaryInterfaces[i];
												result = true;
												break;
										}
								}
								return result;
						}
				}

				public bool HasActiveBaseInterface {
						get {
								bool result = false;
								for (int i = 0; i < BaseInterfaces.Count; i++) {
										if (BaseInterfaces[i].HasFocus) {
												LastActiveBaseInterface = BaseInterfaces[i];
												result = true;
												break;
										}
								}
								return result;
						}
				}

				public bool HasActiveInterface {
						get {
								return HasActiveSecondaryInterface || HasActivePrimaryInterface || HasActiveBaseInterface;
						}
				}

				public bool ReceiveUserAction(UserActionType action, double timeStamp)
				{
						bool passThrough = true;
						if (HasActiveSecondaryInterface) {
								passThrough = LastActiveSecondaryInterface.UserActions.ReceiveAction(action, timeStamp);
						}
						//pass-through on secondary interfaces go to primary interfaces
						//primary interfaces ignore actions when cutscenes are active
						if (passThrough && !Cutscene.IsActive && HasActivePrimaryInterface) {//TODO make this a global setting
								//only intercept active stuff
								passThrough = LastActivePrimaryInterface.UserActions.ReceiveAction(action, timeStamp);
						}
						//pass through on primary interfaces goes to base interfaces
						if (passThrough && HasActiveBaseInterface) {
								passThrough = LastActiveBaseInterface.UserActions.ReceiveAction(action, timeStamp);
						}
						//if it goes through the base interface, pass it along
						return passThrough;
				}

				public bool ReceiveInterfaceAction(InterfaceActionType action, double timeStamp)
				{
						if (action == InterfaceActionType.InterfaceHide) {
								//Debug.Log("interface hide action");
								PrimaryInterfacesEnabled = !PrimaryInterfacesEnabled;
								return true;
						}

						//intercept manual pause requests
						if (action == InterfaceActionType.GamePause) {
								if (!HasActiveInterface) {
										ManuallyPaused = !ManuallyPaused;
										return false;
								}
						}

						//intercept scrolling - we want to use it for our scrollbars and sliders
						if (action == InterfaceActionType.SelectionPrev || action == InterfaceActionType.SelectionNext) {
								//scrollbars come first, then sliders
								if (mActiveScrollBar != null && mActiveScrollBar.alpha > 0) {
										//use the interface action mouse wheel
										if (action == InterfaceActionType.SelectionPrev) {
												mActiveScrollBar.scrollValue = Mathf.Clamp01(mActiveScrollBar.scrollValue - 0.1f);
										} else {
												mActiveScrollBar.scrollValue = Mathf.Clamp01(mActiveScrollBar.scrollValue + 0.1f);
										}
								} else if (mActiveSlider != null) {
										if (action == InterfaceActionType.SelectionPrev) {
												mActiveSlider.sliderValue = Mathf.Clamp01(mActiveSlider.sliderValue - 0.1f);
										} else {
												mActiveSlider.sliderValue = Mathf.Clamp01(mActiveSlider.sliderValue + 0.1f);
										}
								}
						}

						//intercept interface cycle requests
						if (action == InterfaceActionType.ToggleInterfaceNext) {
								//this is specifically to overcome difficulties with controller support
								//it lets you cycle through map / inventory / log at the moment
								//more interfaces (eg status) will be added soon
								if (HasActivePrimaryInterface) {
										switch (LastActivePrimaryInterface.Name) {
												case "Inventory":
														PrimaryInterface.MaximizeInterface("Log");
														break;

												case "Log":
														PrimaryInterface.MaximizeInterface("WorldMap");
														break;

												case "WorldMap":
														PrimaryInterface.MinimizeAll();
														break;

												default:
														break;
										}
								} else {
										PrimaryInterface.MaximizeInterface("Inventory");
								}
								return false;
						}

						//send it through GUI cursor
						//don't bother to check it for pass-through
						//it doesn't filter anything
						GUICursor.Get.ReceiveAction(action, timeStamp);

						bool passThrough = true;
						if (HasActiveSecondaryInterface) {
								passThrough = LastActiveSecondaryInterface.ReceiveAction(action, timeStamp);
						}
						//pass-through on secondary interfaces go to primary interfaces
						//primary interfaces ignore actions when cutscenes are active
						if (passThrough && !Cutscene.IsActive) {
								for (int i = 0; i < PrimaryInterfaces.Count; i++) {
										FrontiersInterface primaryInterface = PrimaryInterfaces[i];
										passThrough = primaryInterface.ReceiveAction(action, timeStamp);
										if (!passThrough) {
												break;
										}
								}
						}
						//pass through on primary interfaces goes to base interfaces
						if (passThrough && HasActiveBaseInterface) {
								passThrough = LastActiveBaseInterface.ReceiveAction(action, timeStamp);
						}
						//if it goes through the base interface, pass it along
						return passThrough;
				}

				public void Deactivate(FrontiersInterface fInterface)
				{
						SecondaryInterfaces.Remove(fInterface);
				}

				public bool GetFocus(FrontiersInterface focusObject)
				{
						if (focusObject.HasFocus) {
								return true;
						}
						bool result = false;
						switch (focusObject.Type) {
								case InterfaceType.Secondary:
										if (SecondaryInterfaces.Count > 0) {
												result = true;
												for (int i = SecondaryInterfaces.items.LastIndex(); i >= 0; i--) {
														FrontiersInterface secondaryInterface = SecondaryInterfaces.items[i];
														if (secondaryInterface == null || secondaryInterface.IsDestroyed) {
																SecondaryInterfaces.items.RemoveAt(i);
														} else if (secondaryInterface != focusObject && secondaryInterface.HasFocus) {
																//secondaryInterface is NOT the item we want to gain focus for, losing
																result &= secondaryInterface.LoseFocus();
														}
												}
										} else {
												result = true;
										}

										if (result) {
												if (!SecondaryInterfaces.Contains(focusObject)) {
														SecondaryInterfaces.Push(focusObject);
												} else {
														SecondaryInterfaces.Remove(focusObject);
														SecondaryInterfaces.Push(focusObject);
												}
												result = focusObject.GainFocus();

												int depth = 0;
												foreach (FrontiersInterface secondaryInterface in SecondaryInterfaces.items) {
														secondaryInterface.SetDepth(depth);
														depth++;
												}
										}
										break;

								case InterfaceType.Primary:
										if (!GameManager.Is(FGameState.InGame)) {
												result = false;
										}
										if (HasActiveSecondaryInterface) {
												result = LastActiveSecondaryInterface.LoseFocus();
										} else if (HasActivePrimaryInterface) {//if we alread have an active primary interface, see if it'll yield
												if (LastActivePrimaryInterface.LoseFocus()) {
														//active interface yielded focus, so proceeding
														result = true;
												} else {
														//last active primary interface would not yield focus
												}
										} else {
												//no active primary interface, so just give it focus
												result = true;
										}

										if (result) {
												focusObject.GainFocus();
												LastActivePrimaryInterface = focusObject;
										}
										break;

								case InterfaceType.Base:
										if (!BaseInterfaces.Contains(focusObject)) {
												BaseInterfaces.Add(focusObject);
										}
										//base interface can't get focus unless everything else is gone
										if (HasActiveSecondaryInterface || HasActivePrimaryInterface) {
												result = false;
										} else {
												focusObject.GainFocus();
												result = true;
										}
										break;

								default:
										break;
						}

						#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(gameObject);
						#endif

						return result;
				}

				public bool ReleaseFocus(FrontiersInterface focusObject)
				{
						if (!focusObject.HasFocus) {
								return true;
						}
						bool result = false;
						switch (focusObject.Type) {
								case InterfaceType.Secondary:
										if (HasActiveSecondaryInterface) {
												if (focusObject == LastActiveSecondaryInterface) {
														//if we have an active secondary interface
														//and this is it, lose focus and pop it off the stack
														focusObject.LoseFocus();
														SecondaryInterfaces.Pop();
														if (HasActiveSecondaryInterface) {
																//if we still have another one on the stack
																//that secondary interface now has focus
																LastActiveSecondaryInterface.GainFocus();
														}
														result = true;
												}
												//otherwise the focus automatically goes to the first
												//active primary / base interface, so no need to check those
										}
										break;

								case InterfaceType.Primary:
										if (HasActiveSecondaryInterface) {
												//TEMP - MAY BE BUGGY?
												result = true;
										} else {
												focusObject.LoseFocus();
												result = true;
										}
										break;

								case InterfaceType.Base:
										focusObject.LoseFocus();
										result = true;
										break;

								default:
										break;
						}
						return result;

						#if UNITY_EDITOR
						UnityEditor.EditorUtility.SetDirty(gameObject);
						#endif
				}

				public GameObject Dialog(string dialogName)
				{	//TODO is it worth it to put this in a dictionary?
						for (int i = 0; i < DialogPrefabs.Count; i++) {
								if (DialogPrefabs[i].name == dialogName) {
										return DialogPrefabs[i];
								}
						}
						return null;
				}

				public GameObject InterfacePiece(string pieceName)
				{
						//TODO implement!
						return null;
				}

				public static void PostStackError(WIStackError error)
				{	//todo move this somewhere else
						//also make this more descriptive
						switch (error) {
								case WIStackError.IsFull:
										PostMessage(GUIMessageDisplay.Type.Warning, "Container is full");
										break;

								case WIStackError.TooLarge:
										PostMessage(GUIMessageDisplay.Type.Warning, "That won't fit in container");
										break;

								default:
										PostMessage(GUIMessageDisplay.Type.Warning, "You can't do that");
										break;
						}
				}

				#endregion

				#region oculus

				public void SetOculusMode(bool enabled)
				{
						if (enabled) {
								//set up our cameras so they're rendering to render textures
						} else {
								//disable the render textures
						}
				}

				#endregion

				#region messages

				//these are conveneince functions for posting messages
				//i may change how messages are posted later
				//so having everything talk to messages / introspection through the gui manager
				//makes it easier to swap out systems
				protected static void PostMessage(GUIMessageDisplay.Type type, string message)
				{
						switch (type) {
								case GUIMessageDisplay.Type.Info:
										PostInfo(message);
										break;

								case GUIMessageDisplay.Type.Danger:
										PostDanger(message);
										break;

								case GUIMessageDisplay.Type.Warning:
										PostWarning(message);
										break;

								case GUIMessageDisplay.Type.Success:
										PostSuccess(message);
										break;

								default:
										break;
						}
				}

				public static void PostLongFormIntrospection(string message)
				{
						Get.NGUIIntrospectionDisplay.AddLongFormMessage(message, false);
				}

				public static void PostLongFormIntrospection(string message, bool centerText)
				{
						Get.NGUIIntrospectionDisplay.AddLongFormMessage(message, centerText);
				}

				public static void PostLongFormIntrospection(string message, bool centerText, bool force)
				{
						Get.NGUIIntrospectionDisplay.AddLongFormMessage(message, centerText, force);
				}

				public static void PostIntrospection(string message, bool force)
				{
						Get.NGUIIntrospectionDisplay.AddMessage(message, 0.0f, string.Empty, force);
				}

				public static void PostIntrospection(string message, float delay)
				{
						Get.NGUIIntrospectionDisplay.AddMessage(message, delay, string.Empty, false);
				}

				public static void PostIntrospection(string message, string missionName, float delay)
				{
						Get.NGUIIntrospectionDisplay.AddMessage(message, delay, missionName, false);
				}

				public static void PostIntrospection(string message, bool activateMission, string missionName)
				{
						Get.NGUIIntrospectionDisplay.AddMessage(message, 0.0f, missionName, false);
				}

				public static void PostIntrospection(string message)
				{
						Get.NGUIIntrospectionDisplay.AddMessage(message, 0.0f, string.Empty, false);
				}

				public static void PostInfo(string message)
				{
						if (!SuspendMessages) {
								Get.NGUIMessageDisplay.PostMessage(message, GUIMessageDisplay.Type.Info);
						} else {
								mCachedMessages.Enqueue(new CachedMessage(GUIMessageDisplay.Type.Info, message));
						}
				}

				public static void PostWarning(string message)
				{
						if (!SuspendMessages && Get.NGUIMessageDisplay != null) {
								Get.NGUIMessageDisplay.PostMessage(message, GUIMessageDisplay.Type.Warning);
						} else {
								mCachedMessages.Enqueue(new CachedMessage(GUIMessageDisplay.Type.Warning, message));
						}
				}

				public static void PostDanger(string message)
				{
						if (!SuspendMessages) {
								Get.NGUIMessageDisplay.PostMessage(message, GUIMessageDisplay.Type.Danger);
						} else {
								mCachedMessages.Enqueue(new CachedMessage(GUIMessageDisplay.Type.Danger, message));
						}
				}

				public static void PostSuccess(string message)
				{
						if (!SuspendMessages) {
								Get.NGUIMessageDisplay.PostMessage(message, GUIMessageDisplay.Type.Success);
						} else {
								mCachedMessages.Enqueue(new CachedMessage(GUIMessageDisplay.Type.Success, message));
						}
				}

				public static void PostGainedItem(Frontiers.World.BaseWIScripts.QuestItemState questItem)
				{
						InterfaceActionType a = InterfaceActionType.ToggleInventory;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleInventory);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Added " + questItem.DisplayName + " to inventory",
								0.0,
								"Mission Item",
								GainedSomethingType.QuestItem,
								a,
								"Inventory");
				}

				public static void PostGainedItem(Frontiers.World.PurseState purseState)
				{
						InterfaceActionType a = InterfaceActionType.ToggleInventory;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleInventory);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Added " + purseState.TotalValue.ToString() + " to bank",
								0.0,
								"Currency",
								GainedSomethingType.Currency,
								a,
								"Inventory");
				}

				public static void PostGainedItem(MobileReference structure)
				{
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"You have aquired a structure",
								0.0,
								structure.FullPath,
								GainedSomethingType.Structure,
								InterfaceActionType.NoAction,
								string.Empty);
				}

				public static void PostGainedItem(Frontiers.World.Book book)
				{
						//we can get to the log by cycling interface or by selecting it manually
						//see which one we're using here
						InterfaceActionType a = InterfaceActionType.ToggleLog;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleLog);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						//set the default tab on log to skills
						GUILogInterface.Get.Tabs.DefaultPanel = "BooksAndLetters";
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								book.CleanTitle + " added to Log",
								0.0,
								book.Name,
								GainedSomethingType.Book,
								a,
								"View Log");
				}

				public static void PostGainedItem(Skill skill)
				{
						InterfaceActionType a = InterfaceActionType.ToggleLog;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleLog);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						GUILogInterface.Get.Tabs.DefaultPanel = "Skills";
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Learned skill: " + skill.DisplayName + "\n",
								0.0,
								skill.name,
								GainedSomethingType.Skill,
								a,
								"View Log");
				}

				public static void PostGainedItem(int currency, WICurrencyType type)
				{
						InterfaceActionType a = InterfaceActionType.ToggleInventory;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleInventory);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Added " + currency.ToString() + Frontiers.World.Currency.TypeToString(type) + " to Currency",
								0.0,
								"Currency",
								GainedSomethingType.Currency,
								a,
								"Inventory");
				}

				public static void PostGainedItem(int credentials, string credentialsFlagset)
				{
						/*Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Gained credentials: " + credentialsFlagset,
								0.0,
								credentialsFlagset,
								GainedSomethingType.Credential);*/
				}

				public static void PostGainedItem(Mission mission)
				{
						InterfaceActionType a = InterfaceActionType.ToggleLog;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleLog);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						string message = "New mission: " + mission.State.Title + "\n";
						if (mission.State.ObjectivesCompleted) {
								message = "Completed " + mission.State.Title;
						}
						GUILogInterface.Get.Tabs.DefaultPanel = "Missions";
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								message,
								0.0,
								mission.State.Name,
								GainedSomethingType.Mission,
								a,
								"View Log");
				}

				public static void PostGainedItem(WIBlueprint blueprint)
				{
						InterfaceActionType a = InterfaceActionType.ToggleLog;
						InControl.InputControlType c = InterfaceActionManager.Get.GetActionBinding((int)InterfaceActionType.ToggleLog);
						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								a = InterfaceActionType.ToggleInterfaceNext;
						}
						Get.NGUIIntrospectionDisplay.AddGainedSomethingMessage(
								"Acquired blueprint: " + blueprint.CleanName,
								0.0,
								blueprint.Name,
								GainedSomethingType.Blueprint,
								a,
								"View Log");
				}

				public static void PostTutorialMessage(string message)
				{

				}

				protected static void PurgeCachedMessages()
				{
						while (mCachedMessages.Count > 0) {
								CachedMessage message = mCachedMessages.Dequeue();
								PostMessage(message.Type, message.Message);
						}
				}

				protected static Queue <CachedMessage> mCachedMessages = new Queue <CachedMessage>();

				public class CachedMessage
				{
						public CachedMessage(GUIMessageDisplay.Type type, string message)
						{
								Type = type;
								Message = message;
						}

						public GUIMessageDisplay.Type Type;
						public string Message;
				}

				#endregion

				#region secondary interface functions

				public static GameObject SpawnNGUIInterface(GameObject NGUIInterface)
				{
						//otherwise check existing NGUI objects to see if they exist
						GameObject newNGUIInterface = null;
						newNGUIInterface = GameObject.Instantiate(NGUIInterface) as GameObject;
						newNGUIInterface.name = NGUIInterface.name;

						//return child editor
						return newNGUIInterface;
				}

				public static GameObject SpawnNGUIChildEditor(GameObject parentNGUIEditor, GameObject NGUIEditor)
				{
						//used when the parent editor and the parentNGUIeditor are the same
						return SpawnNGUIChildEditor(parentNGUIEditor, NGUIEditor, parentNGUIEditor, false);
				}

				public static GameObject SpawnNGUIChildEditor(GameObject parentNGUIEditor, GameObject NGUIEditor, bool parentTransition)
				{
						//used when the parent editor and the parentNGUIeditor are the same
						return SpawnNGUIChildEditor(parentNGUIEditor, NGUIEditor, parentNGUIEditor, parentTransition);
				}

				public static GameObject SpawnNGUIChildEditor(GameObject parentEditor, GameObject NGUIEditor, GameObject ParentNGUIEditor, bool parentTransition)
				{
						Get.NGUIMessageDisplay.HideImmediately();

						//otherwise check existing NGUI objects to see if they exist
						GameObject newNGUIEditor = null;
						//figure out which anchor to use
						UIAnchor anchor = Get.NGUISecondaryCenterAnchor;
						FrontiersInterface fi = null;

						if (NGUIEditor.HasComponent <FrontiersInterface>(out fi)) {
								switch (fi.AnchorSide) {
										case UIAnchor.Side.Center:
										default:
												break;

										case UIAnchor.Side.Left:
												anchor = Get.NGUISecondaryLeftAnchor;
												break;

										case UIAnchor.Side.Right:
												break;

										case UIAnchor.Side.Top:
												break;

										case UIAnchor.Side.Bottom:
												anchor = Get.NGUISecondaryBottomAnchor;
												break;
								}
						}

						newNGUIEditor = NGUITools.AddChild(anchor.gameObject, NGUIEditor);
						newNGUIEditor.name = NGUIEditor.name;

						//set the transition parent
						List<IGUIChildEditor> childEditors = GetGUIChildEditors(newNGUIEditor);
						foreach (IGUIChildEditor childEditor in childEditors) {
								childEditor.NGUIParentObject = ParentNGUIEditor;
						}
						//do transition
						ScaleUpEditor(newNGUIEditor).Proceed();
						if (parentTransition) {
								ScaleDownEditor(ParentNGUIEditor).Proceed();
						}
						//return child editor
						return newNGUIEditor;
				}

				public static List<IGUIChildEditor> GetGUIChildEditors(GameObject NGUIEditor)
				{
						UnityEngine.Object[] objectList = NGUIEditor.GetComponents(typeof(IGUIChildEditor)) as UnityEngine.Object[];
						List<IGUIChildEditor> childEditorList = new List<IGUIChildEditor>();

						foreach (UnityEngine.Object obj in objectList) {
								IGUIChildEditor childEditor = (IGUIChildEditor)obj;
								if (childEditor != null) {
										childEditorList.Add(childEditor);
								}
						}

						return childEditorList;
				}

				public static IGUIChildEditor<R> GetGUIChildEditor<R>(GameObject NGUIEditor)
				{
						return NGUIEditor.GetComponent(typeof(IGUIChildEditor<R>)) as IGUIChildEditor<R>;
				}

				public static void GetSelectedObject<R>(IGUIChildEditor<List<R>> childEditor, ref R selectedObject)
				{
						GUIBrowser<R> browser = (GUIBrowser<R>)childEditor;

						if (browser != null) {
								selectedObject = browser.SelectedObject;
						}
				}

				public static void SendEditObjectToChildEditor <R>(GameObject NGUIEditor, R editObject)
				{
						IGUIChildEditor<R> childEditor = GetGUIChildEditor<R>(NGUIEditor);

						if (childEditor == null) {
								return;
						}
						childEditor.ReceiveFromParentEditor(editObject, null);
				}

				public static void SendEditObjectToChildEditor<R>(IGUIParentEditor<R> parentEditor, GameObject NGUIEditor, R editObject)
				{
						IGUIChildEditor<R> childEditor = GetGUIChildEditor<R>(NGUIEditor);

						if (childEditor == null) {
								return;
						}
						ChildEditorCallback<R> callBack = new ChildEditorCallback<R>(parentEditor.ReceiveFromChildEditor);
						childEditor.ReceiveFromParentEditor(editObject, callBack);
				}

				public static void SendEditObjectToChildEditor<R>(ChildEditorCallback<R> callBack, GameObject NGUIEditor, R editObject)
				{
						IGUIChildEditor<R> childEditor = GetGUIChildEditor<R>(NGUIEditor);
						if (childEditor == null) {
								return;
						}
						childEditor.ReceiveFromParentEditor(editObject, callBack);
				}

				public static void RetireGUIChildEditor(GameObject finishedEditor)
				{
						NGUITools.SetActive(finishedEditor, false);
						finishedEditor.transform.localPosition = new Vector3(-12000f, 0f, 0f);
						GameObject.Destroy(finishedEditor, 0.1f);
				}

				public static GUITransition ScaleUpEditor(GameObject editor)
				{
						return editor.AddComponent <ScaleUpTransition>();
				}

				public static GUITransition	ScaleDownEditor(GameObject editor)
				{
						if (editor != null) {
								Vector3 newZPosition = editor.transform.localPosition;
								newZPosition.z += 100f;
								editor.transform.localPosition = newZPosition;
								ScaleDownTransition scaleDown = editor.GetOrAdd <ScaleDownTransition>();
								return scaleDown;
						}
						return null;
				}

				public static void TrashNGUIObject(GameObject trashObject)
				{
						trashObject.transform.parent = Get.NGUITrash.transform;
						trashObject.transform.localPosition = new Vector3(-150000f, 0f, 0f);
						NGUITools.Destroy(trashObject);
				}

				#endregion

				#if UNITY_EDITOR
				public void DrawEditorGUI()
				{
						UnityEngine.GUI.color = Color.cyan;
						GUILayout.Label("\nSecondary Interfaces: (" + SecondaryInterfaces.Count.ToString() + ")");
						foreach (FrontiersInterface si in SecondaryInterfaces.items) {
								UnityEngine.GUI.color = Color.cyan;
								if (si != null) {
										if (si.HasFocus) {
												UnityEngine.GUI.color = Color.green;
										}
										if (si != TopInterface) {
												UnityEngine.GUI.color = Color.Lerp(Color.gray, UnityEngine.GUI.color, 0.5f);
										}
										if (GUILayout.Button(si.Name)) {
												ReleaseFocus(si);
										}
								} else {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Button("NULL");
								}
						}

						UnityEngine.GUI.color = Color.yellow;
						GUILayout.Label("\nPrimary Interfaces:");
						foreach (FrontiersInterface pi in PrimaryInterfaces) {
								UnityEngine.GUI.color = Color.yellow;
								if (pi != null) {
										if (pi.HasFocus) {
												UnityEngine.GUI.color = Color.green;
										}
										if (pi != TopInterface) {
												UnityEngine.GUI.color = Color.Lerp(Color.gray, UnityEngine.GUI.color, 0.5f);
										}
										if (GUILayout.Button(pi.Name)) {
												ReleaseFocus(pi);
										}
								} else {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Button("NULL");
								}
						}

						UnityEngine.GUI.color = Color.Lerp(Color.white, Color.blue, 0.5f);
						GUILayout.Label("\nBase Interfaces:");
						foreach (FrontiersInterface bi in BaseInterfaces) {
								UnityEngine.GUI.color = Color.Lerp(Color.white, Color.blue, 0.5f);
								if (bi != null) {
										if (bi.HasFocus) {
												UnityEngine.GUI.color = Color.green;
										}
										if (bi != TopInterface) {
												UnityEngine.GUI.color = Color.Lerp(Color.gray, UnityEngine.GUI.color, 0.5f);
										}
										if (GUILayout.Button(bi.Name)) {
												ReleaseFocus(bi);
										}
								} else {
										UnityEngine.GUI.color = Color.red;
										GUILayout.Button("NULL");
								}
						}
				}
				#endif
				public static int GetNextGUIID()
				{	//do we even need this any more?
						gGUIID++;
						return gGUIID;
				}

				protected static int gGUIID = 100;
		}

		[Serializable]
		public class SemiStack <T> //TODO clean up enumerable
		{
				public List<T> items = new List<T>();

				public bool Contains(T item)
				{
						return items.Contains(item);
				}

				public int Count {
						get {
								return items.Count;
						}
				}

				public void Push(T item)
				{
						items.Add(item);
				}

				public T Peek()
				{
						return items[items.LastIndex()];
				}

				public T Pop()
				{
						if (items.Count > 0) {
								T temp = items[items.Count - 1];
								items.RemoveAt(items.Count - 1);
								return temp;
						} else
								return default(T);
				}

				public void Remove(T item)
				{
						items.Remove(item);
				}

				public void Remove(int itemAtPosition)
				{
						items.RemoveAt(itemAtPosition);
				}
		}
}
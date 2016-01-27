using UnityEngine;
using System.Collections;
using Frontiers.World;
using InControl;
using System;

namespace Frontiers.GUI
{
		public class GUIHud : MonoBehaviour
		{
				public static GUIHud Get;

				public void Start()
				{
						Get = this;
						FollowTarget.disableIfInvisible = false;
						FollowTarget.mUICamera = GUIManager.Get.HudCamera;
						Prompt1.Clear();
						Prompt2.Clear();
				}

				public GUIHudMode Mode = GUIHudMode.MouseAndKeyboard;
				public UIPanel ControllerPanel;
				//the HUD can show up to 2 prompts + a bar
				public HudPrompt Prompt1;
				public HudPrompt Prompt2;
				public GUIHudMiniAction HudAction1;
				public GUIHudMiniAction HudAction2;
				public UISlider ProgressBar;
				public UISprite ProgressBarBackground;
				public UISprite ProgressBarForeground;
				public UISprite ProgressBarPing;
				public IItemOfInterest FocusItem;
				public bool RequireFocus = false;
				public UIFollowTarget FollowTarget;

				public void ClearFocusItem(IItemOfInterest focusItem)
				{
						if (FocusItem == focusItem) {
								//Debug.Log("Clearing focus item");
								FocusItem = null;
								Prompt1.Clear();
								Prompt2.Clear();
								mRefreshNextUpdate = true;
						}
				}

				public void RefreshColors()
				{

				}

				public void ShowAction(UserActionType action, string description, Transform hudTarget, Camera followCamera)
				{
						ShowAction(null, action, description, hudTarget, followCamera);
				}

				public void ShowAction(InterfaceActionType action, string description, Transform hudTarget, Camera followCamera)
				{
						ShowAction(null, action, description, hudTarget, followCamera);
				}

				public void ShowActions(UserActionType action1, UserActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						ShowActions(null, action1, action2, description1, description2, hudTarget, followCamera);
				}

				public void ShowActions(InterfaceActionType action1, InterfaceActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						ShowActions(null, action1, action2, description1, description2, hudTarget, followCamera);
				}

				public void ShowActions(UserActionType action1, InterfaceActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						ShowActions(null, action1, action2, description1, description2, hudTarget, followCamera);
				}

				public void ShowActions(InterfaceActionType action1, UserActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						ShowActions(null, action1, action2, description1, description2, hudTarget, followCamera);
				}

				public void ShowAction(IItemOfInterest ioi, UserActionType action, string description, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						if (!Prompt1.Visible || Prompt1.UserAction == action) {
								Prompt1 = GetBindings(new HudPrompt(action, description));
						} else if (!Prompt2.Visible || Prompt2.UserAction == action) {
								Prompt2 = GetBindings(new HudPrompt(action, description));
						} else {
								//Debug.Log("Couldn't add prompt, both prompt already visible");
						}
				}

				public void ShowAction(IItemOfInterest ioi, InterfaceActionType action, string description, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						if (!Prompt1.Visible || Prompt1.InterfaceAction == action) {
								Prompt1 = GetBindings(new HudPrompt(action, description));
						} else if (!Prompt2.Visible || Prompt2.InterfaceAction == action) {
								Prompt2 = GetBindings(new HudPrompt(action, description));
						} else {
								//Debug.Log("Couldn't add prompt, both prompt already visible");
						}
				}

				public void ShowActions(IItemOfInterest ioi, UserActionType action1, UserActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						Prompt1 = GetBindings(new HudPrompt(action1, description1));
						Prompt2 = GetBindings(new HudPrompt(action2, description2));
				}

				public void ShowActions(IItemOfInterest ioi, InterfaceActionType action1, InterfaceActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						Prompt1 = GetBindings(new HudPrompt(action1, description1));
						Prompt2 = GetBindings(new HudPrompt(action2, description2));
				}

				public void ShowActions(IItemOfInterest ioi, UserActionType action1, InterfaceActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						Prompt1 = GetBindings(new HudPrompt(action1, description1));
						Prompt2 = GetBindings(new HudPrompt(action2, description2));
				}

				public void ShowActions(IItemOfInterest ioi, InterfaceActionType action1, UserActionType action2, string description1, string description2, Transform hudTarget, Camera followCamera)
				{
						SetTargets(ioi, hudTarget, followCamera);
						Prompt1 = GetBindings(new HudPrompt(action1, description1));
						Prompt2 = GetBindings(new HudPrompt(action2, description2));
				}

				public void Refresh()
				{
						Prompt1 = RefreshHudAction(Prompt1, HudAction1, Mode, true);
						Prompt2 = RefreshHudAction(Prompt2, HudAction2, Mode, true);
						/*if (Prompt1.Visible) {
								//Debug.Log("Prompt 1 visible");
								if (Mode == GUIHudMode.MouseAndKeyboard) {
										//key takes priority over mouse
										if (Prompt1.Key != KeyCode.None) {
												HudAction1.SetKey(Prompt1.Key, Prompt1.Description);
										} else {
												//mouse
												HudAction1.SetMouse(Prompt1.Mouse, Prompt1.Description);
										}
								} else {
										HudAction1.SetControl(Prompt1.Control, Prompt1.Description, InterfaceActionManager.ActionSpriteSuffix);
								}
						} else {
								//Debug.Log("Prompt 1 not visible, restting");
								HudAction1.Reset();
								Prompt1.Clear();
						}

						if (Prompt2.Visible) {
								//Debug.Log("Prompt 2 visible");
								if (Mode == GUIHudMode.MouseAndKeyboard) {
										//key takes priority over mouse
										if (Prompt2.Key != KeyCode.None) {
												HudAction2.SetKey(Prompt2.Key, Prompt2.Description);
										} else {
												//mouse
												HudAction2.SetMouse(Prompt2.Mouse, Prompt2.Description);
										}
								} else {
										HudAction2.SetControl(Prompt2.Control, Prompt2.Description, InterfaceActionManager.ActionSpriteSuffix);
								}
						} else {
								//Debug.Log("Neither is visible, resetting");
								HudAction2.Reset();
								Prompt2.Clear();
						}*/
				}

				protected void SetTargets(IItemOfInterest ioi, Transform hudTarget, Camera followCamera)
				{
						if (ioi != FocusItem) {
								//if we're looking at something different, clear the existing prompts
								//Debug.Log("New focus item, clearing now");
								Prompt1.Clear();
								Prompt2.Clear();
								ResetAlpha();
						}
						FocusItem = ioi;
						if (FocusItem == null) {
								RequireFocus = false;
						} else {
								RequireFocus = true;
						}
						FollowTarget.target = hudTarget;
						mShowStartTime = WorldClock.RealTime;
						mRefreshNextUpdate = true;
				}

				public void ShowProgressBar(Color fgColor, Color bgColor, float initialValue)
				{
						RefreshColors();
						////Debug.Log ("Showing progress bar");
						mShowProgressBarStartTime = WorldClock.RealTime;
						ProgressBarBackground.color = bgColor;
						ProgressBarForeground.color = fgColor;
						ProgressBar.sliderValue = initialValue;
				}

				public void ResetAlpha()
				{
						HudAction1.SetAlpha(0f);
						HudAction2.SetAlpha(0f);
						SetProgressBarAlpha(0f);
				}

				public void SetProgressBarAlpha(float alpha)
				{
						ProgressBarBackground.alpha = alpha;
						ProgressBarForeground.alpha = alpha;
				}

				public void Update()
				{
						if (!GameManager.Is(FGameState.InGame)
						 || !Profile.Get.CurrentPreferences.Immersion.WorldItemHUD
						 || GUIManager.HideCrosshair
						 || (RequireFocus && (FocusItem == null || FocusItem.Destroyed || !FocusItem.HasPlayerFocus))
						 || (FollowTarget.target == null)) {

								if (ControllerPanel.enabled) {
										////Debug.Log ("Shutting down");
										FocusItem = null;
										ControllerPanel.enabled = false;
										mShowStartTime = 0f;
										mShowProgressBarStartTime = 0f;
										ResetAlpha();
								}
								return;
						}

						if (Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								Mode = GUIHudMode.Controller;
						} else {
								Mode = GUIHudMode.MouseAndKeyboard;
						}

						FollowTarget.alwaysInCenter = Profile.Get.CurrentPreferences.Immersion.WorldItemHUDInCenter;

						if (mRefreshNextUpdate) {
								mRefreshNextUpdate = false;
								Refresh();
						}

						ControllerPanel.enabled = true;
						FollowTarget.enabled = true;

						if (WorldClock.RealTime < ShowEndTime) {
								if (Prompt1.Visible) {
										Prompt1.Alpha = Mathf.Lerp(Prompt1.Alpha, 1f, 0.5f);
								}
								if (Prompt2.Visible) {
										Prompt2.Alpha = Mathf.Lerp(Prompt2.Alpha, 1f, 0.5f);
								}
						} else {
								//Debug.Log("No longer showing");
								if (Prompt1.Visible) {
										Prompt1.Alpha = Mathf.Lerp(Prompt1.Alpha, 0f, 0.25f);
								}
								if (Prompt2.Visible) {
										Prompt2.Alpha = Mathf.Lerp(Prompt2.Alpha, 0f, 0.25f);
								}
						}

						HudAction1.SetAlpha(Prompt1.Alpha);
						HudAction2.SetAlpha(Prompt2.Alpha);

						if (WorldClock.RealTime < ShowProgressBarEndTime) {
								SetProgressBarAlpha(Mathf.Lerp(ProgressBarForeground.alpha, 1f, 0.25f));
						} else {
								SetProgressBarAlpha(Mathf.Lerp(ProgressBarForeground.alpha, 0f, 0.25f));
						}
				}

				public static HudPrompt RefreshHudAction(HudPrompt prompt, GUIHudMiniAction hudAction, GUIHudMode mode, bool clearPromptOnEmpty)
				{
						if (prompt.Visible) {
								if (mode == GUIHudMode.MouseAndKeyboard) {
										//key takes priority over mouse
										if (prompt.Key != KeyCode.None) {
												if (!hudAction.SetKey(prompt.Key, prompt.Description)) {
														if (clearPromptOnEmpty) {
																prompt.Clear();
														}
														hudAction.Reset();
														prompt.IsEmpty = true;
												} else {
														prompt.IsEmpty = false;
												}
										} else {
												//mouse
												if (!hudAction.SetMouse(prompt.Mouse, prompt.Description)) {
														if (clearPromptOnEmpty) {
																prompt.Clear();
														}
														hudAction.Reset();
														prompt.IsEmpty = true;
												} else {
														prompt.IsEmpty = false;
												}
										}
								} else {
										if (!hudAction.SetControl(prompt.Control, prompt.Description, InterfaceActionManager.ActionSpriteSuffix)) {
												if (clearPromptOnEmpty) {
														prompt.Clear();
												}
												hudAction.Reset();
												prompt.IsEmpty = true;
										} else {
												prompt.IsEmpty = false;
										}
								}
						} else {
								hudAction.Reset();
								if (clearPromptOnEmpty) {
										prompt.Clear();
								}
								prompt.IsEmpty = true;
						}
						return prompt;
				}

				public static HudPrompt GetBindings(HudPrompt forPrompt)
				{
						return GetBindings(forPrompt, true);
				}

				public static HudPrompt GetBindings(HudPrompt forPrompt, bool axisX)
				{
						if (!forPrompt.Visible) {
								return forPrompt;
						}

						if (forPrompt.IsUserAxis) {
								forPrompt.Control = UserActionManager.Get.GetActionAxis(forPrompt.UserAxis);
								if (forPrompt.HasActionBinding) {
										UserActionManager.Get.GetKeyBinding(forPrompt.Control, true, axisX, ref forPrompt.Key);
										UserActionManager.Get.GetMouseBinding(forPrompt.Control, ref forPrompt.Mouse);
								}
						} else if (forPrompt.IsInterfaceAxis) {
								forPrompt.Control = InterfaceActionManager.Get.GetActionAxis(forPrompt.InterfaceAxis);
								if (forPrompt.HasActionBinding) {
										UserActionManager.Get.GetKeyBinding(forPrompt.Control, true, axisX, ref forPrompt.Key);
										UserActionManager.Get.GetMouseBinding(forPrompt.Control, ref forPrompt.Mouse);
								}
						} else if (forPrompt.IsUserAction) {
								forPrompt.Control = UserActionManager.Get.GetActionBinding((int)forPrompt.UserAction);
								if (forPrompt.HasActionBinding) {
										UserActionManager.Get.GetKeyBinding(forPrompt.Control, ref forPrompt.Key);
										UserActionManager.Get.GetMouseBinding(forPrompt.Control, ref forPrompt.Mouse);
								}
						} else {
								forPrompt.Control = InterfaceActionManager.Get.GetActionBinding((int)forPrompt.InterfaceAction);
								if (forPrompt.HasActionBinding) {
										InterfaceActionManager.Get.GetKeyBinding(forPrompt.Control, ref forPrompt.Key);
										InterfaceActionManager.Get.GetMouseBinding(forPrompt.Control, ref forPrompt.Mouse);
								}
						}
						return forPrompt;
				}

				protected bool mRefreshNextUpdate = true;
				protected double mShowStartTime = 0f;
				protected double mShowProgressBarStartTime = 0f;

				protected double ShowEndTime {
						get {
								return mShowStartTime + Profile.Get.CurrentPreferences.Immersion.HUDPersistTime;
						}
				}

				protected double ShowProgressBarEndTime {
						get {
								return mShowProgressBarStartTime + Profile.Get.CurrentPreferences.Immersion.HUDPersistTime;
						}
				}

				public enum GUIHudMode
				{
						MouseAndKeyboard,
						Controller,
				}
				//simple struct for keeping track of what we're displaying
				[Serializable]
				public struct HudPrompt
				{
						public HudPrompt(ActionSetting.InputAxis userAxis, ActionSetting.InputAxis interfaceAxis, string description)
						{
								Description = description;
								UserAxis = userAxis;
								InterfaceAxis = interfaceAxis;
								InterfaceAction = InterfaceActionType.NoAction;
								UserAction = UserActionType.NoAction;
								Control = InputControlType.None;
								Key = KeyCode.None;
								Mouse = ActionSetting.MouseAction.None;
								Alpha = 0f;
								IsEmpty = false;
						}

						public HudPrompt(InterfaceActionType interfaceAction, string description)
						{
								Description = description;
								UserAxis = ActionSetting.InputAxis.None;
								InterfaceAxis = ActionSetting.InputAxis.None;
								InterfaceAction = interfaceAction;
								UserAction = UserActionType.NoAction;
								Control = InputControlType.None;
								Key = KeyCode.None;
								Mouse = ActionSetting.MouseAction.None;
								Alpha = 0f;
								IsEmpty = false;
						}

						public HudPrompt(UserActionType userAction, string description)
						{
								Description = description;
								UserAxis = ActionSetting.InputAxis.None;
								InterfaceAxis = ActionSetting.InputAxis.None;
								InterfaceAction = InterfaceActionType.NoAction;
								UserAction = userAction;
								Control = InputControlType.None;
								Key = KeyCode.None;
								Mouse = ActionSetting.MouseAction.None;
								Alpha = 0f;
								IsEmpty = false;
						}

						public void Clear()
						{ 
								Description = string.Empty;
								UserAxis = ActionSetting.InputAxis.None;
								InterfaceAxis = ActionSetting.InputAxis.None;
								InterfaceAction = InterfaceActionType.NoAction;
								UserAction = UserActionType.NoAction;
								Control = InputControlType.None;
								Key = KeyCode.None;
								Mouse = ActionSetting.MouseAction.None;
								Alpha = 0f;
								IsEmpty = false;
						}

						public bool Visible {
								get {
										return InterfaceAction != InterfaceActionType.NoAction
										|| UserAction != UserActionType.NoAction
										|| InterfaceAxis != ActionSetting.InputAxis.None
										|| UserAxis != ActionSetting.InputAxis.None;
								}
						}

						public bool HasActionBinding {
								get {
										return Control != InputControlType.None;
								}
						}

						public bool IsUserAction {
								get {
										return UserAction != UserActionType.NoAction && InterfaceAction == InterfaceActionType.NoAction;
								}
						}

						public bool IsInterfaceAction {
								get {
										return UserAction == UserActionType.NoAction && InterfaceAction != InterfaceActionType.NoAction;
								}
						}

						public bool IsUserAxis {
								get {
										return UserAxis != ActionSetting.InputAxis.None;
								}
						}

						public bool IsInterfaceAxis {
								get {
										return InterfaceAxis != ActionSetting.InputAxis.None;
								}
						}

						public string Description;
						public UserActionType UserAction;
						public InterfaceActionType InterfaceAction;
						public InputControlType Control;
						public KeyCode Key;
						public ActionSetting.MouseAction Mouse;
						public ActionSetting.InputAxis UserAxis;
						public ActionSetting.InputAxis InterfaceAxis;
						public float Alpha;
						public bool IsEmpty;
				}
		}
}
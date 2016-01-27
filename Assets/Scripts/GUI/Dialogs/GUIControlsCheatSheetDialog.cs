using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
	public class GUIControlsCheatSheetDialog : GUIEditor<YesNoCancelDialogResult>
	{
		public GUIHudMiniAction RotateCameraMouse;
		public GUIHudMiniAction SelectUp;
		public GUIHudMiniAction SelectLeft;
		public GUIHudMiniAction SelectDown;
		public GUIHudMiniAction SelectRight;
		public GUIHudMiniAction CursorClick;
		public GUIHudMiniAction CursorRightClick;
		//these are weird, they don't use ngui textures
		public MeshRenderer IconResetCameraBig;
		public MeshRenderer IconLockAxisBig;
		public MeshRenderer IconCursorLockBig;
		public MeshRenderer IconReorientBig;
		public MeshRenderer IconLockAxisLittleOn;
		public MeshRenderer IconLockAxisLittleOff;
		public MeshRenderer IconLockAxisLittleForceOn;
		public MeshRenderer IconLockAxisLittleForceOff;
		public MeshRenderer IconCursorLockLittleOn;
		public MeshRenderer IconCursorLockLittleOff;
		public MeshRenderer IconCursorLockLittleForceOn;
		public MeshRenderer IconCursorLockLittleForceOff;
		public UIGrid MovementAndActionsGrid;
		public UIGrid InterfaceGrid;
		public GUITabs Tabs;
		public GameObject MiniActionPrefab;
		public GameObject CloseButton;
		public GameObject ConfigureControlsButton;
		public UISlider MouseSensitivityInterface;
		public UISlider MouseSensitivityFPSCamera;
		public UICheckbox VREnableRotation;
		public UICheckbox VREnableInstaRotation;
		public UICheckbox MouseInvertYAxis;
		public UICheckbox MovementInvertYAxis;
		public UICheckbox InterfaceInvertYAxis;
		public UICheckbox ControllerCursorCheckbox;
		public UICheckbox CustomDeadZonesCheckbox;
		public UICheckbox ControllerPrompts;
		public UICheckbox ShowCPromptsWhenControllerIsPresent;
		public UILabel ControllerPromptsCheckboxLabel;
		//dead zone stuff
		public UISlider DeadZoneLeftStickUpper;
		public UISlider DeadZoneRightStickUpper;
		public UISlider DeadZoneDPadUpper;
		public UISlider DeadZoneLeftStickLower;
		public UISlider DeadZoneRightStickLower;
		public UISlider DeadZoneDPadLower;
		public UISlider DPadSensitivitySlider;
		public UISlider LeftStickSensitivitySlider;
		public UISlider RightStickSensitivitySlider;
		public UILabel DeadZoneLeftStickUpperLabel;
		public UILabel DeadZoneRightStickUpperLabel;
		public UILabel DeadZoneDPadUpperLabel;
		public UILabel DeadZoneLeftStickLowerLabel;
		public UILabel DeadZoneRightStickLowerLabel;
		public UILabel DeadZoneDPadLowerLabel;
		public UILabel DPadSensitivitySliderLabel;
		public UILabel LeftStickSensitivitySliderLabel;
		public UILabel RightStickSensitivitySliderLabel;
		public List <GUIHud.HudPrompt> UserPrompts = new List<GUIHud.HudPrompt> ();
		public List <GUIHudMiniAction> UserMiniActions = new List<GUIHudMiniAction> ();
		public List <GUIHud.HudPrompt> InterfacePrompts = new List<GUIHud.HudPrompt> ();
		public List <GUIHudMiniAction> InterfaceMiniActions = new List<GUIHudMiniAction> ();
		public float MouseSensitivityFPSMin	= 1.0f;
		public float MouseSensitivityFPSMax	= 10.0f;

		public override void GetActiveInterfaceObjects (List<Widget> currentObjects, int flag)
		{
			if (flag < 0) {
				flag = GUIEditorID;
			}

			Tabs.GetActiveInterfaceObjects (currentObjects, flag);
			Widget w = new Widget (flag);
			w.SearchCamera = NGUICamera;
			w.BoxCollider = CloseButton.GetComponent <BoxCollider> ();
			currentObjects.Add (w);
			w.BoxCollider = ConfigureControlsButton.GetComponent <BoxCollider> ();
			currentObjects.Add (w);
		}

		public override Widget FirstInterfaceObject {
			get {
				Widget w = base.FirstInterfaceObject;
				if (Tabs.Buttons.Count > 0) {
					w.BoxCollider = Tabs.Buttons [0].Collider;
				}
				return w;
			}
		}

		public override void WakeUp ()
		{
			base.WakeUp ();
			Tabs.Initialize (this);
			Tabs.OnSetSelection += OnSetSelection;
			ControllerPrompts.functionName = "OnControlSettingsChange";
			ControllerPrompts.eventReceiver = gameObject;
			ControllerCursorCheckbox.functionName = "OnControlSettingsChange";
			ControllerCursorCheckbox.eventReceiver = gameObject;
			ShowCPromptsWhenControllerIsPresent.functionName = "OnControlSettingsChange";
			ShowCPromptsWhenControllerIsPresent.eventReceiver = gameObject;

			MouseSensitivityInterface.functionName = "OnControlSettingsChange";
			MouseSensitivityInterface.eventReceiver = gameObject;
			MouseSensitivityFPSCamera.functionName = "OnControlSettingsChange";
			MouseSensitivityFPSCamera.eventReceiver = gameObject;
			MouseInvertYAxis.functionName = "OnControlSettingsChange";
			MouseInvertYAxis.eventReceiver = gameObject;
			MovementInvertYAxis.functionName = "OnControlSettingsChange";
			MovementInvertYAxis.eventReceiver = gameObject;
			InterfaceInvertYAxis.functionName = "OnControlSettingsChange";
			InterfaceInvertYAxis.eventReceiver = gameObject;
			ControllerCursorCheckbox.functionName = "OnControlSettingsChange";
			ControllerCursorCheckbox.eventReceiver = gameObject;
			CustomDeadZonesCheckbox.functionName = "OnControlSettingsChange";
			CustomDeadZonesCheckbox.eventReceiver = gameObject;
			VREnableRotation.functionName = "OnControlSettingsChange";
			VREnableRotation.eventReceiver = gameObject;
			VREnableInstaRotation.functionName = "OnControlSettingsChange";
			VREnableInstaRotation.eventReceiver = gameObject;

			DeadZoneLeftStickUpper.eventReceiver = gameObject;
			DeadZoneRightStickUpper.eventReceiver = gameObject;
			DeadZoneDPadUpper.eventReceiver = gameObject;
			DeadZoneLeftStickLower.eventReceiver = gameObject;
			DeadZoneRightStickLower.eventReceiver = gameObject;
			DeadZoneDPadLower.eventReceiver = gameObject;
			DPadSensitivitySlider.eventReceiver = gameObject;
			LeftStickSensitivitySlider.eventReceiver = gameObject;
			RightStickSensitivitySlider.eventReceiver = gameObject;

			DeadZoneLeftStickUpper.functionName = "OnControlSettingsChange";
			DeadZoneRightStickUpper.functionName = "OnControlSettingsChange";
			DeadZoneDPadUpper.functionName = "OnControlSettingsChange";
			DeadZoneLeftStickLower.functionName = "OnControlSettingsChange";
			DeadZoneRightStickLower.functionName = "OnControlSettingsChange";
			DeadZoneDPadLower.functionName = "OnControlSettingsChange";
			DPadSensitivitySlider.functionName = "OnControlSettingsChange";
			LeftStickSensitivitySlider.functionName = "OnControlSettingsChange";
			RightStickSensitivitySlider.functionName = "OnControlSettingsChange";

			IconCursorLockBig.material.color = Colors.Get.VRIconColorOn;
			IconLockAxisBig.material.color = Colors.Get.VRIconColorOn;
			IconReorientBig.material.color = Colors.Get.VRIconColorOn;
			IconResetCameraBig.material.color = Colors.Get.VRIconColorOn;

			//IconLockAxisLittleOn.material;
			//IconCursorLockLittleOn.material;
			IconLockAxisLittleOff.material.color = Colors.Get.VRIconColorOff;
			IconCursorLockLittleOff.material = IconLockAxisLittleOff.material;
			IconLockAxisLittleForceOn.material.color = Colors.Get.VRIconColorForceOn;
			IconCursorLockLittleForceOn.material = IconLockAxisLittleForceOn.material;
			IconLockAxisLittleForceOff.material.color = Colors.Get.VRIconColorForceOff;
			IconCursorLockLittleForceOff.material = IconLockAxisLittleForceOff.material;

			Profile.Get.CurrentPreferences.Controls.RefreshCustomDeadZoneSettings (InterfaceActionManager.Get.Device);
		}

		protected void OnSetSelection ()
		{
			if (Tabs.SelectedTab == "VR") {
				IconResetCameraBig.enabled = true;
				IconLockAxisBig.enabled = true;
				IconCursorLockBig.enabled = true;
				IconReorientBig.enabled = true;

				IconLockAxisLittleOn.enabled = true;
				IconLockAxisLittleOff.enabled = true;
				IconLockAxisLittleForceOn.enabled = true;
				IconLockAxisLittleForceOff.enabled = true;

				IconCursorLockLittleOn.enabled = true;
				IconCursorLockLittleOff.enabled = true;
				IconCursorLockLittleForceOn.enabled = true;
				IconCursorLockLittleForceOff.enabled = true;
			} else {
				IconResetCameraBig.enabled = false;
				IconLockAxisBig.enabled = false;
				IconCursorLockBig.enabled = false;
				IconReorientBig.enabled = false;

				IconLockAxisLittleOn.enabled = false;
				IconLockAxisLittleOff.enabled = false;
				IconLockAxisLittleForceOn.enabled = false;
				IconLockAxisLittleForceOff.enabled = false;

				IconCursorLockLittleOn.enabled = false;
				IconCursorLockLittleOff.enabled = false;
				IconCursorLockLittleForceOn.enabled = false;
				IconCursorLockLittleForceOff.enabled = false;
			}
		}

		public override void PushEditObjectToNGUIObject ()
		{
			MouseSensitivityFPSMax = Globals.MouseSensitivityFPSMax;
			MouseSensitivityFPSMin = Globals.MouseSensitivityFPSMin;

			foreach (ActionSetting action in UserActionManager.Get.CurrentActionSettings) {
				if (action.AxisSetting) {
					GUIHud.HudPrompt p = new GUIHud.HudPrompt (action.Axis, ActionSetting.InputAxis.None, action.ActionDescription);
					p = GUIHud.GetBindings (p);
					UserPrompts.Add (p);
				} else {
					GUIHud.HudPrompt p = new GUIHud.HudPrompt ((UserActionType)action.Action, action.ActionDescription);
					p = GUIHud.GetBindings (p);
					UserPrompts.Add (p);
				}

				GameObject miniActionGameObject = NGUITools.AddChild (MovementAndActionsGrid.gameObject, MiniActionPrefab);
				GUIHudMiniAction miniAction = miniActionGameObject.GetComponent <GUIHudMiniAction> ();
				//add a box collider so GUI will pick it up
				miniAction.gameObject.GetOrAdd <BoxCollider> ();
				UserMiniActions.Add (miniAction);
			}
			foreach (ActionSetting action in InterfaceActionManager.Get.CurrentActionSettings) {
				if (action.AxisSetting) {
					GUIHud.HudPrompt p = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, action.Axis, action.ActionDescription);
					p = GUIHud.GetBindings (p);
					InterfacePrompts.Add (p);
				} else {
					GUIHud.HudPrompt p = new GUIHud.HudPrompt ((InterfaceActionType)action.Action, action.ActionDescription);
					p = GUIHud.GetBindings (p);
					InterfacePrompts.Add (p);
				}

				GameObject miniActionGameObject = NGUITools.AddChild (InterfaceGrid.gameObject, MiniActionPrefab);
				GUIHudMiniAction miniAction = miniActionGameObject.GetComponent <GUIHudMiniAction> ();
				miniAction.gameObject.GetOrAdd <BoxCollider> ();
				InterfaceMiniActions.Add (miniAction);
			}

			mInitialized = true;

			Refresh ();
			ControlsRefresh ();
		}

		public override void Refresh ()
		{
			if (Profile.Get.HasControllerPluggedIn) {
				ControllerPromptsCheckboxLabel.text = "Use Controller Prompts (Controller available)";
			} else {
				ControllerPromptsCheckboxLabel.text = "Use Controller Prompts (No controllers found)";
			}

			GUIHud.GUIHudMode mode = GUIHud.GUIHudMode.MouseAndKeyboard;
			//ViewAnyTimeLabel.enabled = true;
			if (ControllerPrompts.isChecked) {
				mode = GUIHud.GUIHudMode.Controller;
				//ViewAnyTimeLabel.enabled = false;
			}
			for (int i = 0; i < UserPrompts.Count; i++) {
				UserPrompts [i] = GUIHud.RefreshHudAction (UserPrompts [i], UserMiniActions [i], mode, false);
				//Debug.Log("Refreshing user prompt " + UserPrompts[i].Description);
				UserMiniActions [i].gameObject.SetActive (!UserPrompts [i].IsEmpty);
			}
			for (int i = 0; i < InterfacePrompts.Count; i++) {
				InterfacePrompts [i] = GUIHud.RefreshHudAction (InterfacePrompts [i], InterfaceMiniActions [i], mode, false);
				//Debug.Log("Refreshing interface prompt " + InterfacePrompts[i].Description);
				InterfaceMiniActions [i].gameObject.SetActive (!InterfacePrompts [i].IsEmpty);
			}
			MovementAndActionsGrid.maxPerLine = 7;
			InterfaceGrid.maxPerLine = 7;

			MovementAndActionsGrid.hideInactive = true;
			InterfaceGrid.hideInactive = true;

			MovementAndActionsGrid.Reposition ();
			InterfaceGrid.Reposition ();

			GUIHud.HudPrompt vrp = new GUIHud.HudPrompt (ActionSetting.InputAxis.MouseX, ActionSetting.InputAxis.None, "Rotate Camera");
			vrp = GUIHud.GetBindings (vrp, true);
			vrp = GUIHud.RefreshHudAction (vrp, RotateCameraMouse, mode, false);

			vrp = new GUIHud.HudPrompt ((InterfaceActionType)InterfaceActionType.CursorClick, "Click");
			vrp = GUIHud.GetBindings (vrp);
			vrp = GUIHud.RefreshHudAction (vrp, CursorClick, mode, false);

			vrp = new GUIHud.HudPrompt ((InterfaceActionType)InterfaceActionType.CursorRightClick, "Right-Click");
			vrp = GUIHud.GetBindings (vrp);
			vrp = GUIHud.RefreshHudAction (vrp, CursorRightClick, mode, false);

			vrp = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.None, "Button Up");
			vrp = GUIHud.GetBindings (vrp, true);
			vrp = GUIHud.RefreshHudAction (vrp, SelectUp, mode, false);

			vrp = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.None, "Button Down");
			vrp = GUIHud.GetBindings (vrp, false);
			vrp = GUIHud.RefreshHudAction (vrp, SelectDown, mode, false);

			vrp = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.None, "Button Left");
			vrp = GUIHud.GetBindings (vrp, true);
			vrp = GUIHud.RefreshHudAction (vrp, SelectLeft, mode, false);

			vrp = new GUIHud.HudPrompt (ActionSetting.InputAxis.None, ActionSetting.InputAxis.None, "Button Right");
			vrp = GUIHud.GetBindings (vrp, false);
			vrp = GUIHud.RefreshHudAction (vrp, SelectRight, mode, false);

			if (CustomDeadZonesCheckbox.isChecked) {
				DeadZoneLeftStickUpper.gameObject.SendMessage ("SetEnabled");
				DeadZoneRightStickUpper.gameObject.SendMessage ("SetEnabled");
				DeadZoneDPadUpper.gameObject.SendMessage ("SetEnabled");
				DeadZoneLeftStickLower.gameObject.SendMessage ("SetEnabled");
				DeadZoneRightStickLower.gameObject.SendMessage ("SetEnabled");
				DeadZoneDPadLower.gameObject.SendMessage ("SetEnabled");
				DPadSensitivitySlider.gameObject.SendMessage ("SetEnabled");
				LeftStickSensitivitySlider.gameObject.SendMessage ("SetEnabled");
				RightStickSensitivitySlider.gameObject.SendMessage ("SetEnabled");
			} else {
				DeadZoneLeftStickUpper.gameObject.SendMessage ("SetDisabled");
				DeadZoneRightStickUpper.gameObject.SendMessage ("SetDisabled");
				DeadZoneDPadUpper.gameObject.SendMessage ("SetDisabled");
				DeadZoneLeftStickLower.gameObject.SendMessage ("SetDisabled");
				DeadZoneRightStickLower.gameObject.SendMessage ("SetDisabled");
				DeadZoneDPadLower.gameObject.SendMessage ("SetDisabled");
				DPadSensitivitySlider.gameObject.SendMessage ("SetDisabled");
				LeftStickSensitivitySlider.gameObject.SendMessage ("SetDisabled");
				RightStickSensitivitySlider.gameObject.SendMessage ("SetDisabled");
			}

			DeadZoneLeftStickUpperLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickUpper.ToString ("P1");
			DeadZoneRightStickUpperLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickUpper.ToString ("P1");
			DeadZoneDPadUpperLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadUpper.ToString ("P1");
			DeadZoneLeftStickLowerLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickLower.ToString ("P1");
			DeadZoneRightStickLowerLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickLower.ToString ("P1");
			DeadZoneDPadLowerLabel.text = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadLower.ToString ("P1");
			DPadSensitivitySliderLabel.text = Profile.Get.CurrentPreferences.Controls.SensitivityDPad.ToString ("P1");
			LeftStickSensitivitySliderLabel.text = Profile.Get.CurrentPreferences.Controls.SensitivityLStick.ToString ("P1");
			RightStickSensitivitySliderLabel.text = Profile.Get.CurrentPreferences.Controls.SensitivityRStick.ToString ("P1");

			mRefreshingControls = true;

			DeadZoneLeftStickUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickUpper;
			DeadZoneRightStickUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickUpper;
			DeadZoneDPadUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadUpper;
			DeadZoneLeftStickLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickLower;
			DeadZoneRightStickLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickLower;
			DeadZoneDPadLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadLower;
			DPadSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityDPad;
			LeftStickSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityLStick;
			RightStickSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityRStick;

			mRefreshingControls = false;
		}

		public void ControlsRefresh ()
		{
			if (!mInitialized || mRefreshingControls || mFinished) {
				return;
			}

			mRefreshingControls = true;

			MouseSensitivityFPSCamera.sliderValue = (Profile.Get.CurrentPreferences.Controls.MouseSensitivityFPSCamera - MouseSensitivityFPSMin) / (MouseSensitivityFPSMax - MouseSensitivityFPSMin);
			MouseInvertYAxis.isChecked = Profile.Get.CurrentPreferences.Controls.MouseInvertYAxis;
			InterfaceInvertYAxis.isChecked = Profile.Get.CurrentPreferences.Controls.InvertRawInterfaceAxis;
			MovementInvertYAxis.isChecked = Profile.Get.CurrentPreferences.Controls.InvertRawMovementAxis;
			ControllerCursorCheckbox.isChecked = Profile.Get.CurrentPreferences.Controls.UseControllerMouse;
			CustomDeadZonesCheckbox.isChecked = Profile.Get.CurrentPreferences.Controls.UseCustomDeadZoneSettings;

			if (Profile.Get.CurrentPreferences.Controls.ShowCPromptsWhenControllerIsPresent && Profile.Get.HasControllerPluggedIn) {
				Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts = true;
			}
			ShowCPromptsWhenControllerIsPresent.isChecked = Profile.Get.CurrentPreferences.Controls.ShowCPromptsWhenControllerIsPresent;
			ControllerPrompts.isChecked = Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts;

			DeadZoneLeftStickUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickUpper;
			DeadZoneRightStickUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickUpper;
			DeadZoneDPadUpper.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadUpper;
			DeadZoneLeftStickLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneLStickLower;
			DeadZoneRightStickLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneRStickLower;
			DeadZoneDPadLower.sliderValue = Profile.Get.CurrentPreferences.Controls.DeadZoneDPadLower;
			DPadSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityDPad;
			LeftStickSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityLStick;
			RightStickSensitivitySlider.sliderValue = Profile.Get.CurrentPreferences.Controls.SensitivityRStick;
			VREnableRotation.isChecked = Profile.Get.CurrentPreferences.Controls.VREnableRotation;
			VREnableInstaRotation.isChecked = Profile.Get.CurrentPreferences.Controls.VRInstaRotation;

			mRefreshingControls = false;
		}

		public void OnControlSettingsChange ()
		{
			if (!mInitialized || mRefreshingControls || mFinished) {
				return;
			}

			Profile.Get.CurrentPreferences.Controls.MouseSensitivityFPSCamera = (MouseSensitivityFPSMin + ((MouseSensitivityFPSMax - MouseSensitivityFPSMin) * MouseSensitivityFPSCamera.sliderValue));
			//TempControlPrefs.MouseSensitivityInterface = MouseSensitivityInterface.sliderValue;
			Profile.Get.CurrentPreferences.Controls.MouseInvertYAxis = MouseInvertYAxis.isChecked;
			Profile.Get.CurrentPreferences.Controls.InvertRawInterfaceAxis = InterfaceInvertYAxis.isChecked;
			Profile.Get.CurrentPreferences.Controls.InvertRawMovementAxis = MovementInvertYAxis.isChecked;
			Profile.Get.CurrentPreferences.Controls.UseControllerMouse = ControllerCursorCheckbox.isChecked;
			Profile.Get.CurrentPreferences.Controls.UseCustomDeadZoneSettings = CustomDeadZonesCheckbox.isChecked;
			Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts = ControllerPrompts.isChecked;
			Profile.Get.CurrentPreferences.Controls.ShowCPromptsWhenControllerIsPresent = ShowCPromptsWhenControllerIsPresent.isChecked;
			Profile.Get.CurrentPreferences.Controls.DeadZoneLStickUpper = DeadZoneLeftStickUpper.sliderValue;
			Profile.Get.CurrentPreferences.Controls.DeadZoneRStickUpper = DeadZoneRightStickUpper.sliderValue;
			Profile.Get.CurrentPreferences.Controls.DeadZoneDPadUpper = DeadZoneDPadUpper.sliderValue;
			Profile.Get.CurrentPreferences.Controls.DeadZoneLStickLower = DeadZoneLeftStickLower.sliderValue;
			Profile.Get.CurrentPreferences.Controls.DeadZoneRStickLower = DeadZoneRightStickLower.sliderValue;
			Profile.Get.CurrentPreferences.Controls.DeadZoneDPadLower = DeadZoneDPadLower.sliderValue;
			Profile.Get.CurrentPreferences.Controls.SensitivityDPad = DPadSensitivitySlider.sliderValue;
			Profile.Get.CurrentPreferences.Controls.SensitivityLStick = LeftStickSensitivitySlider.sliderValue;
			Profile.Get.CurrentPreferences.Controls.SensitivityRStick = RightStickSensitivitySlider.sliderValue;
			Profile.Get.CurrentPreferences.Controls.VREnableRotation = VREnableRotation.isChecked;
			Profile.Get.CurrentPreferences.Controls.VRInstaRotation = VREnableInstaRotation.isChecked;

			Profile.Get.CurrentPreferences.Controls.Apply (false);
			Profile.Get.SaveCurrent (ProfileComponents.Preferences);

			Refresh ();
		}

		public void OnClickConfigureControlsButton ()
		{
			EditObject.Result = DialogResult.Yes;
			Finish ();
		}

		public void OnClickCloseButton ()
		{
			if (HasEditObject) {
				EditObject.Result = DialogResult.No;
			}
			ActionCancel (WorldClock.AdjustedRealTime);
		}

		protected bool mInitialized = false;
		protected bool mRefreshingControls = false;
	}
}
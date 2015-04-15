using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.Data;
using System.Collections.Generic;

namespace Frontiers.GUI
{
	public class GUIStartMenu : GUIEditor <StartMenuResult>, IGUIParentEditor
	{
		public UIButton ContinueButton;
		public UIButton NewButton;
		public UIButton LoadButton;
		public UIButton SaveButton;
		public UIButton OptionsButton;
		public UIButton ControlsButton;
		public UIButton MultiplayerButton;
		public UIButton QuitButton;
		public UILabel ChangeLogLabel;
		public UILabel ChangeLogTitleLabel;
		public GameObject ChangeLog;
		public UIPanel MainPanel;
		public bool VRMode = false;
		public Vector3 ContinueButtonPositionNormal;
		public Vector3 NewButtonPositionNormal;
		public Vector3 LoadButtonPositionNormal;
		public Vector3 SaveButtonPositionNormal;
		public Vector3 OptionsButtonPositionNormal;
		public Vector3 MultiplayerButtonPositionNormal;
		public Vector3 ControlsButtonNormal;
		public Vector3 QuitButtonPositionNormal;
		public Vector3 ContinueButtonPositionVR;
		public Vector3 NewButtonPositionVR;
		public Vector3 LoadButtonPositionVR;
		public Vector3 SaveButtonPositionVR;
		public Vector3 OptionsButtonPositionVR;
		public Vector3 MultiplayerButtonPositionVR;
		public Vector3 ControlsButtonVR;
		public Vector3 QuitButtonPositionVR;
		public Vector4 MainPanelClippingNormal;
		public Vector4 MainPanelClippingVR;

		public override Widget FirstInterfaceObject {
			get {
				Widget w = base.FirstInterfaceObject;
				if (EditObject.EnableContinueButton) {
					w.BoxCollider = ContinueButton.GetComponent <BoxCollider>();
				} else {
					w.BoxCollider = NewButton.GetComponent <BoxCollider>();
				}
				return w;
			}
		}

		public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
		{
			if (flag < 0) {
				flag = GUIEditorID;
			}

			Widget w = new Widget(flag);
			w.SearchCamera = NGUICamera;

			w.BoxCollider = ContinueButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w); 

			w.BoxCollider = NewButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = LoadButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = SaveButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = OptionsButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = MultiplayerButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = ControlsButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);

			w.BoxCollider = QuitButton.gameObject.GetComponent<BoxCollider>();
			currentObjects.Add(w);
		}

		public override bool ActionCancel(double timeStamp)
		{
			if (EditObject.EnterGameState == FGameState.GamePaused) {
				Finish();
			}
			return true;
		}

		public override void PushEditObjectToNGUIObject()
		{
			//the start menu automatically un-pauses a manual pause
			//a little weird but it feels right in game
			GUIManager.ManuallyPaused = false;

			if (Profile.Get.Current.HasLastPlayedGame) {
				EditObject.EnableContinueButton = true;
			} else {
				EditObject.EnableContinueButton = false;
			}
			EditObject.EnableOptionsButton = true;
			EditObject.EnableQuitButton = true;
			EditObject.EnableSaveButton = false;

			GameManager.SetState(EditObject.EnterGameState);

			PrimaryInterface.MinimizeAll();

			//since we're pulling up the start menu, which will cause a pause
			//WHY NOT CALL THE GC?
			System.GC.Collect();

			if (!Profile.Get.CurrentPreferences.HideDialogs.Contains("HideAllFocusUpdates") && !Profile.Get.CurrentPreferences.HideDialogs.Contains(GameManager.FocusOnSubject)) {
				GameObject editor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIThisWeekFocusDialog"));
			}

			#if UNITY_EDITOR
			VRMode = VRManager.VRMode | VRManager.VRTestingMode;
			#else
						VRMode = VRManager.VRMode;
			#endif
			RefreshChangeLog();
			RefreshButtons();

			//now see if the edit object asked for any clicks or tabs
			if (EditObject.ClickedOptions) {
				if (!string.IsNullOrEmpty(EditObject.TabSelection)) {
					GUIOptionsDialog optionsDialog = OnClickOptionsButton();
					optionsDialog.Tabs.DefaultPanel = EditObject.TabSelection;
					optionsDialog.Tabs.SetSelection(EditObject.TabSelection);
				}
			}
		}

		public void RefreshChangeLog()
		{
			if (VRMode) {
				ChangeLog.SetActive(false);
				MainPanel.clipRange = MainPanelClippingVR;
			} else {
				string changeLogText = string.Empty;
				System.DateTime changeLogTime = new System.DateTime();
				GameData.IO.GetChangeLog(ref changeLogText, ref changeLogTime);
				ChangeLog.SetActive(true);
				ChangeLogLabel.text = changeLogText;
				ChangeLogTitleLabel.text = "Most recent changes: " + changeLogTime.ToString();
			}
		}

		public void RefreshButtons()
		{
			switch (EditObject.EnterGameState) {
				case FGameState.GamePaused:
				default:
					if (EditObject.EnableContinueButton) {
						ContinueButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					} else {
						ContinueButton.SendMessage("SetDisabled", SendMessageOptions.RequireReceiver);
					}
					if (Profile.Get.HasSelectedGame) {
						NewButton.SendMessage("SetDisabled", SendMessageOptions.RequireReceiver);
						LoadButton.SendMessage("SetDisabled", SendMessageOptions.RequireReceiver);
					} else {
						NewButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
						LoadButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					}
					SaveButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					OptionsButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					MultiplayerButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					ControlsButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					QuitButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					break;

				case FGameState.WaitingForGame:
					if (EditObject.EnableContinueButton) {
						ContinueButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					} else {
						ContinueButton.SendMessage("SetDisabled", SendMessageOptions.RequireReceiver);
					}
					NewButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					LoadButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					SaveButton.SendMessage("SetDisabled", SendMessageOptions.RequireReceiver);
					OptionsButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					MultiplayerButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					ControlsButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					QuitButton.SendMessage("SetEnabled", SendMessageOptions.RequireReceiver);
					break;
			}

			if (VRMode) {
				//make the buttons vertical
				ContinueButton.transform.localPosition = ContinueButtonPositionVR;
				NewButton.transform.localPosition = NewButtonPositionVR;
				LoadButton.transform.localPosition = LoadButtonPositionVR;
				SaveButton.transform.localPosition = SaveButtonPositionVR;
				OptionsButton.transform.localPosition = OptionsButtonPositionVR;
				MultiplayerButton.transform.localPosition = MultiplayerButtonPositionVR;
				ControlsButton.transform.localPosition = ControlsButtonVR;
				QuitButton.transform.localPosition = QuitButtonPositionVR;
				MainPanel.clipRange = MainPanelClippingVR;
			} else {
				//use the normal button positions
				ContinueButton.transform.localPosition = ContinueButtonPositionNormal;
				NewButton.transform.localPosition = NewButtonPositionNormal;
				LoadButton.transform.localPosition = LoadButtonPositionNormal;
				SaveButton.transform.localPosition = SaveButtonPositionNormal;
				OptionsButton.transform.localPosition = OptionsButtonPositionNormal;
				MultiplayerButton.transform.localPosition = MultiplayerButtonPositionNormal;
				ControlsButton.transform.localPosition = ControlsButtonNormal;
				QuitButton.transform.localPosition = QuitButtonPositionNormal;
				MainPanel.clipRange = MainPanelClippingNormal;
			}
		}

		public void Update()
		{
			if (mEditObject == null) {
				return;
			}

			#if UNITY_EDITOR
			if ((VRManager.VRMode | VRManager.VRTestingMode) != VRMode) {
				#else
						if (VRManager.VRMode != VRMode) {
				#endif
				VRMode = VRManager.VRMode;
				RefreshChangeLog();
				RefreshButtons();
			}
		}

		#region button clicks//TODO make this all coroutines

		public void OnClickContinueButton()
		{
			Finish();
		}

		public void OnClickNewButton()
		{
			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUINewGameDialog, false);
			NewGameDialogResult editObject = new NewGameDialogResult();
			GUIManager.SendEditObjectToChildEditor <NewGameDialogResult>(new ChildEditorCallback <NewGameDialogResult>(NewGameDialogCallback), dialog, editObject);
			DisableInput();
		}

		protected void NewGameDialogCallback(NewGameDialogResult editObject, IGUIChildEditor <NewGameDialogResult> childEditor)
		{
			if (mDestroyed) {
				return;
			}

			EnableInput();
			if (!editObject.Cancelled) {
				GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
				GameManager.Load();
				Finish();
			}
		}

		public void OnClickLoadButton()
		{
			if (mDestroyed) {
				return;
			}

			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUILoadGameDialog, false);
			GUILoadGameDialog loadGameDialog = dialog.GetComponent <GUILoadGameDialog>();
			loadGameDialog.OnLoseFocus += OnLoadGameFinish;
			loadGameDialog.Show();
			DisableInput();
		}

		protected void OnLoadGameFinish()
		{
			if (mDestroyed) {
				return;
			}

			if (GameManager.Is(FGameState.GameLoading)) {
				Finish();
			} else {
				//GUIManager.Get.GetFocus (this);
				EnableInput();
			}
		}

		public void OnClickSaveButton()
		{
			if (mDestroyed) {
				return;
			}

			DisableInput();
			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUISaveGameDialog, false);
			GUISaveGameDialog saveGameDialog = dialog.GetComponent <GUISaveGameDialog>();
			saveGameDialog.OnLoseFocus += OnSaveGameFinish;
			saveGameDialog.Show();
		}

		protected void OnSaveGameFinish()
		{
			if (mDestroyed) {
				return;
			}

			EnableInput();
		}

		public void OnClickMultiplayerButton()
		{
			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIMultiplayerDialog, false);
			MultiplayerSession editObject = new MultiplayerSession();
			GUIManager.SendEditObjectToChildEditor <MultiplayerSession>(new ChildEditorCallback <MultiplayerSession>(MultiplayerDialogCallback), dialog, editObject);
			DisableInput();
		}

		protected void MultiplayerDialogCallback(MultiplayerSession editObject, IGUIChildEditor <MultiplayerSession> childEditor)
		{
			if (mDestroyed) {
				return;
			}

			EnableInput();
			GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
		}

		public void OnClickControlsButton()
		{
			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIControlsCheatSheetDialog"), false);
			YesNoCancelDialogResult editObject = new YesNoCancelDialogResult();
			GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(new ChildEditorCallback <YesNoCancelDialogResult>(ControlDialogCallback), dialog, editObject);
			DisableInput();
		}

		protected void ControlDialogCallback(YesNoCancelDialogResult editObject, IGUIChildEditor <YesNoCancelDialogResult> childEditor)
		{
			if (mDestroyed) {
				return;
			}

			GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
			//if the result is yes, open up the options dialog
			if (editObject.Result == DialogResult.Yes) {
				GUIOptionsDialog optionsDialog = OnClickOptionsButton();
				optionsDialog.Tabs.DefaultPanel = "Controls";
				optionsDialog.Tabs.SetSelection("Controls");
			} else {
				EnableInput();
			}
		}

		public void OnClickQuitButton()
		{
			Application.Quit();
			//GameManager.Quit ();
		}

		public GUIOptionsDialog OnClickOptionsButton()
		{
			if (mDestroyed) {
				return null;
			}

			GameObject dialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIOptionsDialog, false);
			GUIManager.SendEditObjectToChildEditor <PlayerPreferences>(new ChildEditorCallback <PlayerPreferences>(OptionsDialogCallback), dialog, Profile.Get.CurrentPreferences);
			GUIOptionsDialog optionsDialog = dialog.GetComponent <GUIOptionsDialog>();
			DisableInput();
			return optionsDialog;
		}

		protected void OptionsDialogCallback(PlayerPreferences editObject, IGUIChildEditor <PlayerPreferences> childEditor)
		{
			EnableInput();
			GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
		}

		protected override void OnFinish()
		{
			GameManager.SetState(EditObject.ExitGameState);
			base.OnFinish();
		}

		#endregion

		protected GameObject mOptions;
	}

	public class StartMenuResult : GenericDialogResult
	{
		//TODO clean this up we don't need most of this
		public FGameState EnterGameState;
		public FGameState ExitGameState;
		public bool EnableContinueButton = true;
		public bool EnableNewButton = true;
		public bool EnableSaveButton = true;
		public bool EnableLoadButton = true;
		public bool EnableMultiplayerButton = true;
		public bool EnableOptionsButton = true;
		public bool EnableReturnButton = true;
		public bool EnableQuitButton = true;
		public bool ClickedContinue = false;
		public bool ClickedNew = false;
		public bool	ClickedSave = false;
		public bool	ClickedLoad = false;
		public bool	ClickedOptions = false;
		public bool ClickedMultiplayer = false;
		public bool	ClickedReturn = false;
		public bool	ClickedQuit = false;
		public string TabSelection = string.Empty;
	}
}
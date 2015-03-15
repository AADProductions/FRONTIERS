using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using System;

namespace Frontiers.GUI
{
		public class GUINewGameDialog : GUIEditor <NewGameDialogResult>
		{
				[HideInInspector]
				[NonSerialized]
				public WorldSettings CurrentWorld = null;
				public bool CurrentWorldConfirmed = false;
				public bool CurrentGameConfirmed = false;
				public bool DevLockOverride = false;

				public UIButtonMessage OKWorldButton;
				public UIButtonMessage OKNameButton;
				public UIButtonMessage StartGameButton;
				public UIButtonMessage CancelButton;
				public GameObject SelectWorldParent;
				public GameObject SelectNameParent;
				public UILabel CustomizeLabel;
				public UILabel WorldLabel;
				public UILabel WorldDescription;
				public UILabel GameNameLabel;
				public UIInput GameNameInput;
				public UISprite LockedOverlay;
				public UILabel LockedLabel;
				public int SelectedWorldIndex = -1;
				public List <string> AvailableWorlds = new List <string>();
				public double InGameMinutesPerRealTimeSecond = Globals.DefaultInGameMinutesPerRealTimeSecond;
				public bool CurrentWorldLocked = false;
				public Texture2D WorldTexture;
				public GameObject WorldTexturePlane;
				public GUITabs Tabs;
				public GUITabPage ControllingTabPage;
				public UISlider DayNightCycleSlider;

				public override Widget FirstInterfaceObject {
						get {
								Widget w = new Widget();
								w.SearchCamera = NGUICamera;
								w.BoxCollider = GameNameInput.GetComponent<BoxCollider>();
								return w;
						}
				}

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects)
				{
						Tabs.GetActiveInterfaceObjects(currentObjects);
				}

				public bool HasCurrentWorld {
						get {
								return CurrentWorld != null;
						}
				}

				public override void WakeUp()
				{
						base.WakeUp();

						SelectedWorldIndex = -1;
						DayNightCycleSlider.sliderValue = 0.5f;//default
						Tabs.Initialize(this);
						ControllingTabPage.OnSelected += ShowNewGamePage;
						ControllingTabPage.OnDeselected += HideNewGamePage;

						OKWorldButton.target = gameObject;
						OKWorldButton.functionName = "OnClickOKWorldButton";
						OKNameButton.target = gameObject;
						OKNameButton.functionName = "OnClickOKNameButton";
						StartGameButton.target = gameObject;
						StartGameButton.functionName = "OnClickStartGameButton";
						CancelButton.target = gameObject;
						CancelButton.functionName = "OnClickCancelButton";

						CurrentWorld = null;
						CurrentGameConfirmed = false;
						CurrentWorldConfirmed = false;
				}

				public void ShowNewGamePage()
				{
						//this is the only bit of UI that isn't managed by NGUI
						//so we have to turn it on / off ourselves
						WorldTexturePlane.gameObject.SetActive(true);
						Refresh();
				}

				public void HideNewGamePage()
				{
						WorldTexturePlane.gameObject.SetActive(false);
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (mDestroyed) {
								return true;
						}

						mEditObject.Cancelled = true;
						Finish();
						return true;
				}

				public void Refresh()
				{
						if (!CurrentWorldConfirmed) {
								SelectWorldParent.gameObject.SetActive(true);
								SelectNameParent.gameObject.SetActive(false);
								Tabs.SetTabsDisabled(true);
								CustomizeLabel.enabled = false;

								AvailableWorlds.Clear();
								AvailableWorlds.AddRange(Mods.Get.Available("World", DataType.Base));
								if (SelectedWorldIndex < 0) {
										RefreshWorld();
								}
								RefreshDescriptions();

								StartGameButton.gameObject.SendMessage("SetDisabled");
						} else if (!CurrentGameConfirmed) {
								SelectWorldParent.gameObject.SetActive(false);
								SelectNameParent.gameObject.SetActive(true);
								Tabs.SetTabsDisabled(true);
								CustomizeLabel.enabled = false;

								RefreshGameName();

								StartGameButton.gameObject.SendMessage("SetDisabled");
						} else {
								SelectWorldParent.gameObject.SetActive(false);
								SelectNameParent.gameObject.SetActive(false);
								Tabs.SetTabsDisabled(false);
								CustomizeLabel.enabled = true;
								StartGameButton.gameObject.SendMessage("SetEnabled");
						}
				}

				public void RefreshGameName()
				{
						if (mRefreshingGameName) {
								return;
						}
			
						mRefreshingGameName = true;
			
						string cleanAlternative = string.Empty;
						if (!CurrentWorldLocked && Profile.Get.ValidateNewGameName(AvailableWorlds[SelectedWorldIndex], GameNameInput.text, out cleanAlternative)) {
								OKNameButton.SendMessage("SetEnabled");
						} else {
								OKNameButton.SendMessage("SetDisabled");
						}
						GameNameInput.text = cleanAlternative;
						GameNameInput.label.text = cleanAlternative;
			
						mRefreshingGameName = false;
				}

				public void OnDayNightCycleSliderChange(float value)
				{
						float normalizedValue = (DayNightCycleSlider.sliderValue * 2) - 1f;
						if (Mathf.Approximately(normalizedValue, 0f)) {
								//the middle is normal
								InGameMinutesPerRealTimeSecond = Globals.DefaultInGameMinutesPerRealTimeSecond;
						} else {
								if (normalizedValue < 0f) {
										InGameMinutesPerRealTimeSecond = Mathf.Lerp(Globals.DefaultInGameMinutesPerRealTimeSecond, Globals.MaxInGameMinutesPerRealtimeSecond, Mathf.Abs(normalizedValue));
								} else {
										//the abs of the value will be from 0 to max
										InGameMinutesPerRealTimeSecond = Mathf.Lerp(Globals.DefaultInGameMinutesPerRealTimeSecond, Globals.MinInGameMinutesPerRealtimeSecond, normalizedValue);
								}
						}
				}

				protected bool mUpdatingCycleSlider = false;

				public void RefreshDescriptions()
				{
						WorldDescription.text = Mods.Get.WorldDescription(AvailableWorlds[SelectedWorldIndex]);
				}

				public void OnClickOKWorldButton()
				{
						CurrentWorldConfirmed = true;
						Refresh();
				}

				public void OnClickOKNameButton()
				{		//we want to save the game, then load it into the live game temporarily
						if (Profile.Get.SetWorldAndGame(AvailableWorlds[SelectedWorldIndex], GameNameInput.text, true, true)) {
								//delete LiveGame
								//it may have junk in it
								CurrentGameConfirmed = true;
						}
						Refresh();
				}

				public void OnClickStartGameButton()
				{
						//we assume everything is valid at this point
						//we don't want to load the game again because we'll be wiping out LiveGame
						if (Profile.Get.SetWorldAndGame(AvailableWorlds[SelectedWorldIndex], GameNameInput.text, true, false)) {
								//load the new world settings into the new world
								CancelButton.gameObject.SendMessage("SetDisabled");
								Profile.Get.CurrentGame.InGameMinutesPerRealtimeSecond = InGameMinutesPerRealTimeSecond;
								//save the game to a fresh game file once we're loaded
								Mods.Get.SaveLiveGame(GameNameInput.text);
								Finish();
						}
				}

				public void OnClickCancelButton()
				{
						ActionCancel(WorldClock.RealTime);
				}

				public void OnChangeGameName()
				{
						RefreshGameName();
				}

				public void RefreshWorld()
				{
						//get our current world first
						AvailableWorlds.Clear();
						AvailableWorlds.AddRange(GameData.IO.GetWorldNames());

						string error = string.Empty;
						if (!HasCurrentWorld) {
								//if we've already started a game then use our current world
								if (Profile.Get.HasCurrentGame && Profile.Get.CurrentGame.HasStarted) {
										for (int i = 0; i < AvailableWorlds.Count; i++) {
												if (AvailableWorlds[i] == GameWorld.Get.Settings.Name) {
														SelectedWorldIndex = i;
														break;
												}
										}
								} else {
										//get the default world
										for (int i = 0; i < AvailableWorlds.Count; i++) {
												if (AvailableWorlds[i] == Globals.DefaultWorldName) {
														SelectedWorldIndex = i;
														//load the world settings
														break;
												}
										}
								}
						}

						if (SelectedWorldIndex < 0) {
								SelectedWorldIndex = 0;
						}

						if (!GameData.IO.InitializeLocalDataPaths(Profile.Get.Current.Name, AvailableWorlds[SelectedWorldIndex], "_LiveGame", out error)) {
								Debug.Log(error);
						}

						if (!GameData.IO.LoadWorld(ref CurrentWorld, AvailableWorlds[SelectedWorldIndex], out error)) {
								WorldDescription.text = error;
								return;
						}

						CurrentWorldLocked = false;
						if (CurrentWorld.RequiresCompletedWorlds && !DevLockOverride) {
								foreach (string requiredCompletedWorld in CurrentWorld.RequiredCompletedWorlds) {
										if (!Profile.Get.Current.CompletedWorlds.Contains(requiredCompletedWorld)) {
												CurrentWorldLocked = true;
												break;
										}
								}
						}

						GameData.IO.LoadWorldDescriptionTexture(WorldTexture, AvailableWorlds[SelectedWorldIndex]);
						WorldLabel.text = AvailableWorlds[SelectedWorldIndex];
						WorldDescription.text = CurrentWorld.Description;

						if (CurrentWorldLocked) {
								LockedLabel.alpha = 1f;
								LockedOverlay.alpha = 1f;
								OKWorldButton.gameObject.SendMessage("SetDisabled");
						} else {
								LockedLabel.alpha = 0f;
								LockedOverlay.alpha = 0f;
								OKWorldButton.gameObject.SendMessage("SetEnabled");
						}
				}

				public void OnClickPrevWorld()
				{
						int previousIndex = SelectedWorldIndex;
						SelectedWorldIndex--;
						if (SelectedWorldIndex < 0) {
								SelectedWorldIndex = AvailableWorlds.LastIndex();
						}
						RefreshWorld();
				}

				public void OnClickNextWorld()
				{
						int previousIndex = SelectedWorldIndex;
						SelectedWorldIndex++;
						if (SelectedWorldIndex >= AvailableWorlds.Count) {
								SelectedWorldIndex = 0;
						}
						RefreshWorld();
				}

				public void Update(){
						if (Input.GetKeyDown(KeyCode.F7)) {
								DevLockOverride = true;
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						Refresh();
				}

				protected bool mRefreshingGameName = false;
		}

		public class NewGameDialogResult : GenericDialogResult
		{
		}
		//TODO put this in GUIEditor
		public class GenericDialogResult
		{
				public bool EditorFinished = false;
				public bool Cancelled = false;
				public System.Action RefreshAction;
		}
}
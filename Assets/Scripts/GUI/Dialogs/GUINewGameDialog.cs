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
				public UIButton CreateGameButton;
				public UILabel DifficultyLabel;
				public UILabel DifficultyDescription;
				public UILabel WorldLabel;
				public UILabel WorldDescription;
				public UILabel GameNameLabel;
				public UIInput GameNameInput;
				public UISprite LockedOverlay;
				public UILabel LockedLabel;
				public int SelectedWorldIndex = -1;
				public int SelectedDifficultyIndex = -1;
				public List <string> AvailableWorlds = new List <string>();
				public List <string> AvailableDifficulties = new List <string>();
				[HideInInspector]
				[NonSerialized]
				public WorldSettings CurrentWorld = null;
				public bool CurrentWorldLocked = false;
				public Texture2D WorldTexture;

				public bool HasCurrentWorld {
						get {
								return CurrentWorld != null;
						}
				}

				public override void WakeUp()
				{
						SelectedWorldIndex = -1;
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
						AvailableWorlds.Clear();
						AvailableDifficulties.Clear();
						AvailableWorlds.AddRange(Mods.Get.Available("World", DataType.Base));
						AvailableDifficulties.AddRange(Mods.Get.Available("DifficultySetting", DataType.Base));

						if (SelectedWorldIndex < 0) {
								RefreshWorld();
						}
			
						if (AvailableDifficulties.Count > 0) {
								//startup
								if (SelectedDifficultyIndex < 0) {
										foreach (string availableDifficulty in AvailableDifficulties) {
												if (availableDifficulty == Globals.DefaultDifficultyName) {
														break;
												}
												SelectedDifficultyIndex++;
										}
								}
								DifficultyLabel.text = AvailableDifficulties[SelectedDifficultyIndex];
						}

						RefreshDescriptions();
						RefreshGameName();
				}

				public void RefreshGameName()
				{
						if (mRefreshingGameName) {
								return;
						}
			
						mRefreshingGameName = true;
			
						string error = string.Empty;
						string cleanAlternative = string.Empty;
						if (!CurrentWorldLocked && Profile.Get.ValidateNewGameName(AvailableWorlds[SelectedWorldIndex], GameNameInput.text, out cleanAlternative, out error)) {
								CreateGameButton.SendMessage("SetEnabled");
						} else {
								CreateGameButton.SendMessage("SetDisabled");
						}
						GameNameLabel.text = error;
						GameNameInput.text = cleanAlternative;
			
						mRefreshingGameName = false;
				}

				public void OnClickLowerDifficulty()
				{
						SelectedDifficultyIndex--;
						if (SelectedDifficultyIndex < 0) {
								SelectedDifficultyIndex = AvailableDifficulties.LastIndex();
						}
						Refresh();
						OnSelectDifficulty();
				}

				public void OnClickHigherDifficulty()
				{
						SelectedDifficultyIndex++;
						if (SelectedDifficultyIndex > AvailableDifficulties.LastIndex()) {
								SelectedDifficultyIndex = 0;
						}
						Refresh();
						OnSelectDifficulty();
				}

				public void RefreshDescriptions()
				{
						WorldDescription.text = Mods.Get.WorldDescription(AvailableWorlds[SelectedWorldIndex]);
						if (AvailableDifficulties.Count > 0) {
								DifficultyDescription.text = Mods.Get.Description <DifficultySetting>("DifficultySetting", AvailableDifficulties[SelectedDifficultyIndex]);
						}
				}

				public void OnSelectDifficulty()
				{
						if (AvailableDifficulties.Count > 0) {
								Profile.Get.SetDifficulty(AvailableDifficulties[SelectedDifficultyIndex]);
								RefreshDescriptions();
						}
				}

				public void OnClickCreateButton()
				{
						//we assume everything is valid at this point
						if (Profile.Get.SetWorldAndGame(AvailableWorlds[SelectedWorldIndex], GameNameInput.text, true)) {
								Finish();
						}
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
						if (CurrentWorld.RequiresCompletedWorlds) {
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
						} else {
								LockedLabel.alpha = 0f;
								LockedOverlay.alpha = 0f;
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
						RefreshGameName();
				}

				public void OnClickNextWorld()
				{
						int previousIndex = SelectedWorldIndex;
						SelectedWorldIndex++;
						if (SelectedWorldIndex >= AvailableWorlds.Count) {
								SelectedWorldIndex = 0;
						}
						RefreshWorld();
						RefreshGameName();
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
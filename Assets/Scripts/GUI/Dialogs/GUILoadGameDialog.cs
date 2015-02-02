using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using System;

namespace Frontiers.GUI
{
		public class GUILoadGameDialog : GUIBrowserSelectView <PlayerGame>
		{
				public UIButton LoadButton;
				public UIButton CancelButton;
				public UILabel LoadButtonLabel;
				public UILabel WorldLabel;
				public UILabel WorldDescription;
				public UILabel SelectedGameLabel;
				public UISprite LockedOverlay;
				public UILabel LockedLabel;
				public int SelectedWorldIndex = -1;
				public List <string> AvailableWorlds = new List <string>();
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

				public override IEnumerable <PlayerGame> FetchItems()
				{
						if (SelectedWorldIndex < 0) {
								RefreshWorld();
						}

						LoadButton.SendMessage("SetDisabled");
						SelectedGameLabel.text = "(No game selected)";

						List <PlayerGame> saveGames = new List <PlayerGame>();

						List <string> gameNames = Profile.Get.GameNames(CurrentWorld.Name, false);
						foreach (string gameName in gameNames) {
								PlayerGame game = null;
								if (Mods.Get.Runtime.LoadGame(ref game, CurrentWorld.Name, gameName)) {
										saveGames.Add(game);
								} else {
										Debug.Log("Couldn't load game " + CurrentWorld.Name + ", " + gameName);
								}
						}
						return saveGames as IEnumerable <PlayerGame>;
				}

				protected override GameObject ConvertEditObjectToBrowserObject(PlayerGame editObject)
				{
						GameObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						//we want most recent to least recent
						TimeSpan timeSinceSaved = DateTime.Now - editObject.LastTimeSaved;
						newBrowserObject.name = ((int)timeSinceSaved.TotalMinutes).ToString().PadLeft(10, '0');
						GUIGenericBrowserObject gameBrowserObject = newBrowserObject.GetComponent <GUIGenericBrowserObject>();

						gameBrowserObject.EditButton.target = this.gameObject;
						gameBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						gameBrowserObject.Name.text = Colors.ColorWrap(
								editObject.Name,
								Colors.Get.MenuButtonTextColorDefault) + "\n" + Colors.ColorWrap(editObject.DifficultyName 
								        + "Saved " + editObject.LastTimeSaved.ToLongDateString().ToLower() 
										+ "\nat " + editObject.LastTimeSaved.ToLongTimeString().ToLower() + " (Hours Played: " + WorldClock.SecondsToHours (editObject.GameTimeOffset).ToString ("0.#") + ")",
								Colors.Darken(Colors.Get.MenuButtonTextColorDefault));
						gameBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						gameBrowserObject.Icon.spriteName = "IconMission";

						Color gameColor = Colors.Get.MenuButtonBackgroundColorDefault;

						gameBrowserObject.BackgroundHighlight.enabled = false;
						gameBrowserObject.BackgroundHighlight.color = gameColor;
						gameBrowserObject.Icon.color = gameColor;

						gameBrowserObject.Initialize(editObject.Name);
						gameBrowserObject.Refresh();

						return newBrowserObject;
				}

				public void RefreshWorld()
				{
						ClearAll();
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

						SelectedGameLabel.text = string.Empty;

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

				public override void PushSelectedObjectToViewer()
				{
						if (HasCurrentWorld && SelectedObject != null && SelectedObject.WorldName == CurrentWorld.Name) {
								//the selected item won't be reset just because we selecte a new world
								//so we have to make sure the world matches the game's world name
								LoadButton.SendMessage("SetEnabled");
								SelectedGameLabel.text = mSelectedObject.Name;
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
						ReceiveFromParentEditor(FetchItems(), null);
				}

				public void OnClickNextWorld()
				{
						int previousIndex = SelectedWorldIndex;
						SelectedWorldIndex++;
						if (SelectedWorldIndex >= AvailableWorlds.Count) {
								SelectedWorldIndex = 0;
						}
						RefreshWorld();
						ReceiveFromParentEditor(FetchItems(), null);
				}

				public void OnClickCancelButton (){
						ActionCancel(WorldClock.RealTime);
				}

				public void  OnClickLoadButton()
				{
						CancelButton.gameObject.SendMessage("SetDisabled");
						LoadButton.gameObject.SendMessage("SetDisabled");
						StartCoroutine(LoadGameOverTime());
				}

				public IEnumerator LoadGameOverTime()
				{		//TODO this no longer requires a coroutine
						LoadButtonLabel.text = "Loading...";
						//ok we're done unloading the current game
						if (Profile.Get.SetWorldAndGame(CurrentWorld.Name, SelectedGameLabel.text, true, true)) {
								GameManager.Load();
								Finish();
						}
						yield break;
				}
		}

		public class LoadGameDialogResult : GenericDialogResult
		{
				public int SelectedWorldIndex;
				public List <string> AvailableWorlds;
				public List <string> GameOptionsList;
		}
}
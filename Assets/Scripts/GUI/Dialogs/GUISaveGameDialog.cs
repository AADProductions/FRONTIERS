using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.GUI
{
		public class GUISaveGameDialog : GUIBrowserSelectView <PlayerGame>
		{
				public UILabel MessageType;
				public UILabel Message;
				public GameObject SaveButton;
				public GameObject DeleteButton;
				public GameObject CancelButton;
				public UILabel SaveButtonLabel;
				public UIInput GameCreateResult;
				//public UILabel CreateMessageLabel;
				public bool ValidExistingGame = false;
				public bool ValidNewGame = false;
				public string LastSelectedGame = string.Empty;

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(PlayerGame editObject)
				{
						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						TimeSpan timeSinceSaved = DateTime.Now - editObject.LastTimeSaved;
						newBrowserObject.name = ((int)timeSinceSaved.TotalMinutes).ToString().PadLeft(10, '0');
						GUIGenericBrowserObject gameBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();

						gameBrowserObject.EditButton.target = this.gameObject;
						gameBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						gameBrowserObject.Name.text = Colors.ColorWrap(
								editObject.Name,
								Colors.Get.MenuButtonTextColorDefault) + Colors.ColorWrap(
										" Saved " + editObject.LastTimeSaved.ToLongDateString().ToLower() 
										+ "\nat " + editObject.LastTimeSaved.ToLongTimeString().ToLower() + " (Hours Played: " + WorldClock.SecondsToHours (editObject.GameTimeOffset).ToString ("0.#") + ")",
										Colors.Darken(Colors.Get.MenuButtonTextColorDefault));

						/*gameBrowserObject.Name.text = Colors.ColorWrap(
								editObject.Name, Colors.Get.MenuButtonTextColorDefault) + "\n" +
						Colors.ColorWrap(editObject.DifficultyName + "Saved " + editObject.LastTimeSaved.ToLongDateString().ToLower() + "\nat " + editObject.LastTimeSaved.ToLongTimeString().ToLower(),
								Colors.Darken(Colors.Get.MenuButtonTextColorDefault));*/
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

				public override IEnumerable <PlayerGame> FetchItems()
				{
						List <PlayerGame> saveGames = new List <PlayerGame>();
						List <string> gameNames = Profile.Get.GameNames(Profile.Get.CurrentGame.WorldName, false);
						foreach (string gameName in gameNames) {
								PlayerGame game = null;
								if (Mods.Get.Runtime.LoadGame(ref game, Profile.Get.CurrentGame.WorldName, gameName)) {
										saveGames.Add(game);
								}
						}
						return saveGames as IEnumerable <PlayerGame>;
				}

				public override void PushSelectedObjectToViewer()
				{
						GameCreateResult.text = mSelectedObject.Name;
						RefreshSelection();
				}

				public void RefreshSelection()
				{
						if (mRefreshingSelection || mChildEditor != null) {
								return;
						}

						List <string> gameOptionsList = Profile.Get.GameNames(Profile.Get.CurrentGame.WorldName, false);

						mRefreshingSelection = true;

						ValidNewGame = false;
						ValidExistingGame = false;

						string createError = string.Empty;
						string cleanAlternative = string.Empty;

						if (Profile.Get.ValidateNewGameName(Profile.Get.CurrentGame.WorldName, GameCreateResult.text, out cleanAlternative, out createError)) {
								ValidNewGame = true;
								Message.text = string.Empty;
						} else if (Profile.Get.ValidateExistingGameName(Profile.Get.CurrentGame.WorldName, GameCreateResult.text, out createError)) {
								ValidExistingGame = true;
								Message.text = "This will override existing game " + GameCreateResult.text;
						}
						//CreateMessageLabel.text = createError;
						GameCreateResult.text = cleanAlternative;

						if (ValidNewGame) {
								SaveButton.SendMessage("SetEnabled");
								DeleteButton.SendMessage("SetDisabled");
						} else if (ValidExistingGame) {
								SaveButton.SendMessage("SetEnabled");
								DeleteButton.SendMessage("SetEnabled");
						} else {
								SaveButton.SendMessage("SetDisabled");
								DeleteButton.SendMessage("SetDisabled");
						}

						mRefreshingSelection = false;
				}

				public override void Refresh()
				{
						RefreshSelection();
				}

				public void OnChangeGameName()
				{
						RefreshSelection();
				}

				public void OnSelectGameOption()
				{
						RefreshSelection();
				}

				public void OnClickSaveButton()
				{
						if (ValidNewGame || ValidExistingGame) {
								CancelButton.SendMessage("SetDisabled");
								SaveButton.SendMessage("SetDisabled");
								StartCoroutine(SaveGameOverTime());
						}
				}

				public void OnClickCancelButton(){
						ActionCancel(WorldClock.RealTime);
				}

				public void OnClickDeleteButton()
				{
						if (mChildEditor == null && !Profile.Get.CurrentPreferences.HideDialogs.Contains("DeleteGameWarning")) {
								YesNoCancelDialogResult result = new YesNoCancelDialogResult();
								result.DontShowInFutureCheckbox = true;
								result.DialogName = "DeleteGameWarning";
								result.Message = "Are you sure you want to delete this game? This cannot be undone";
								result.MessageType = "Delete " + GameCreateResult.text;
								result.CancelButton = false;
								mChildEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog);
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(new ChildEditorCallback <YesNoCancelDialogResult>(OnConfirmDelete), mChildEditor, result);
						} else {
								StartCoroutine(DeleteSaveGameOverTime(GameCreateResult.text));
						}
				}

				protected void OnConfirmDelete(YesNoCancelDialogResult result, IGUIChildEditor <YesNoCancelDialogResult> childEditor)
				{
						if (result.Result == DialogResult.Yes) {
								StartCoroutine(DeleteSaveGameOverTime(GameCreateResult.text));
						}
				}

				protected IEnumerator DeleteSaveGameOverTime(string gameName)
				{
						Mods.Get.Runtime.DeleteGame(gameName);
						yield return null;
						GameCreateResult.text = string.Empty;
						Refresh();
						ClearBrowserObjects();
						ReceiveFromParentEditor(FetchItems());
						yield break;
				}

				protected IEnumerator SaveGameOverTime()
				{
						//this renames the current game
						Profile.Get.CurrentGame.Name = GameCreateResult.text;
						//this saves all current progress to _Live
						GameManager.Save();
						while (!Manager.FinishedSaving) {
								Debug.Log("Waiting for save to finish...");
								yield return null;
						}
						//this saves the current game and profile, naming it properly
						Profile.Get.SaveImmediately(ProfileComponents.All);
						//this copies _Live to the game of our choice
						Mods.Get.SaveLiveGame(GameCreateResult.text);
						Finish();
						yield break;
				}

				protected GameObject mChildEditor;
				protected bool mRefreshingSelection = false;
				protected float mButtonLabelSize = 0f;
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using System.Text.RegularExpressions;
using System;

namespace Frontiers
{
		[ExecuteInEditMode]
		public class Profile : Manager
		{
				public static Profile Get;

				public PlayerProfile Current {
						get {
								return mCurrentProfile;
						}
				}

				public PlayerPreferences CurrentPreferences {
						get {
								return mCurrentPreferences;
						}
				}

				public PlayerGame CurrentGame {
						get {
								return mCurrentGame;
						}
				}

				public bool HasCurrentProfile {
						get {
								return mCurrentProfile != null;
						}
				}

				public bool HasCurrentGame {
						get {
								return mCurrentGame != null;
						}
				}

				public bool HasSelectedProfile = false;
				public bool HasSelectedGame = false;
				public bool HasControllerPluggedIn = false;//TODO move to interface manager

				public override void WakeUp()
				{
						Get = this;
						mParentUnderManager = false;
						HasSelectedProfile	= false;
						HasSelectedGame = false;
						mCurrentGame = null;

						//check to see if we have a controller plugged in
						if (Input.GetJoystickNames().Length > 0) {
								HasControllerPluggedIn = true;
								foreach (string joystickName in Input.GetJoystickNames()) {
										Debug.Log("===Found attached controller device: " + joystickName);
								}

						} else {
								HasControllerPluggedIn = false;
						}
				}

				public void ApplyPreferences()
				{
						Debug.Log("Applying preferences");
						CurrentPreferences.Apply(false);
				}

				public List <string> ProfileNames(bool toLower)
				{
						if (!toLower) {
								return GameData.IO.GetFolderNamesInDirectory(GameData.IO.gGlobalProfilesPath);
						} else {
								List <string> profileNames = GameData.IO.GetFolderNamesInDirectory(GameData.IO.gGlobalProfilesPath);
								for (int i = 0; i < profileNames.Count; i++) {
										profileNames[i] = profileNames[i].ToLower();
								}
								return profileNames;
						}
				}

				public void DeleteProfile(string profileName)
				{
						GameData.IO.DeleteProfileData(profileName);
				}

				public List <string> GameNames(string worldName, bool toLower)
				{
						Debug.Log("Getting game names for " + worldName);
						List <string> gameNames = null;
						if (HasCurrentProfile) {
								string gameName = string.Empty;
								string errorMessage = string.Empty;
								if (HasCurrentGame) {
										gameName = CurrentGame.Name;
								}
								if (!GameData.IO.InitializeLocalDataPaths(mCurrentProfile.Name, worldName, gameName, out errorMessage)) {
										Debug.Log(errorMessage);
										gameNames = new List<string>();
								} else {
										if (!toLower) {
												gameNames = GameData.IO.GetFolderNamesInDirectory(GameData.IO.gCurrentWorldPath);
										} else {
												gameNames = GameData.IO.GetFolderNamesInDirectory(GameData.IO.gCurrentWorldPath);
												for (int i = 0; i < gameNames.Count; i++) {
														gameNames[i] = gameNames[i].ToLower();
												}
										}
								}
						}
						return gameNames;
				}

				public override void OnLocalPlayerSpawn()
				{
						//SaveCurrent(ProfileComponents.Profile);
				}

				public bool SetOrCreateProfile(string profileName, out string errorMessage)
				{
						errorMessage = string.Empty;
						List <string> profileNames = ProfileNames(true);
						if (profileNames.Contains(profileName.ToLower())) {
								SetProfile(profileName, out errorMessage);
						} else {
								CreateProfile(profileName);
						}
						return true;
				}

				public bool SetProfile(string profileName, out string errorMessage)
				{
						HasSelectedProfile = false;
						Debug.Log("Setting profile to " + profileName);
						PlayerProfile profile = null;
						PlayerPreferences prefs = null;
						if (GameData.IO.LoadProfile(ref profile, profileName, out errorMessage)) {
								mCurrentProfile = profile;
								Debug.Log("Current profile is now " + profileName);
								HasSelectedProfile = true;

								if (GameData.IO.LoadPreferences(ref prefs, profileName, out errorMessage)) {
										//make sure these prefs are legitimate
										//otherwise they might have wonky values
										/*if (prefs.Version != GameManager.VersionString || !prefs.InitializedAsDefault) {
											prefs = PlayerPreferences.Default();
											//save the new preferences immediately
											GameData.IO.SavePreferences(profileName, prefs);
										}*/
										mCurrentPreferences = prefs;
										//now's the time to detect if we have a controller plugged in
										if (HasControllerPluggedIn) {
												Debug.Log("===Found controller plugged in - showing controller prompts");
												mCurrentPreferences.Controls.ShowControllerPrompts = true;
										} else {
												Debug.Log("===Didn't find controller plugged in - defaulting to mouse & keyboard prompts");								
												mCurrentPreferences.Controls.ShowControllerPrompts = false;
										}
								} else {
										Debug.Log("Couldn't load preferences when setting profle");
								}

						}
						return HasSelectedProfile;
				}

				public bool CreateProfile(string profileName)
				{
						Debug.Log("Creating profile inside of profile manager");
						profileName = GameData.IO.CreateProfileDirectory(profileName);
						if (profileName != "Error") {
								PlayerProfile profile = new PlayerProfile();
								profile.Name = profileName;
								mCurrentProfile = profile;
								mCurrentPreferences = PlayerPreferences.Default();
								GameData.IO.SaveProfile(mCurrentProfile);
								GameData.IO.SavePreferences(mCurrentProfile.Name, mCurrentPreferences);
								HasSelectedProfile = true;
								return true;
						} else {
								return false;
						}
				}

				public void SaveCurrent(ProfileComponents components)
				{
						Debug.Log("PROFILE: Saving current " + components.ToString());
						mSaveNextAvailable |= components;
						enabled = true;
				}

				public void SaveImmediately(ProfileComponents components)
				{
						Debug.Log("PROFILE: Saving profile " + components.ToString() + " immediately");
						if (Flags.Check((uint)components, (uint)ProfileComponents.Preferences, Flags.CheckType.MatchAny)) {
								GameData.IO.SavePreferences(Current.Name, CurrentPreferences);
						}

						if (Flags.Check((uint)components, (uint)ProfileComponents.Game, Flags.CheckType.MatchAny)) {
								//save the whole world
								CurrentGame.LastTimeSaved = DateTime.Now;
								CurrentGame.GameTimeOffset = WorldClock.AdjustedRealTimeWithoutOffset;
								GameData.IO.SaveGame(CurrentGame);
						} else if (Flags.Check((uint)components, (uint)ProfileComponents.Character, Flags.CheckType.MatchAny)) {
								//just save the character
								//don't bother with the rest of the world
								CurrentGame.LastTimeSaved = DateTime.Now;
								CurrentGame.GameTimeOffset = WorldClock.AdjustedRealTimeWithoutOffset;
								GameData.IO.SaveGame(CurrentGame);
						}

						if (Flags.Check((uint)components, (uint)ProfileComponents.Profile, Flags.CheckType.MatchAny)) {
								GameData.IO.SaveProfile(Current);
						}
				}

				public void Update()
				{
						if (mSaveNextAvailable != ProfileComponents.None) {
								if (Flags.Check((uint)mSaveNextAvailable, (uint)ProfileComponents.Preferences, Flags.CheckType.MatchAny)) {
										CurrentPreferences.Version = GameManager.VersionString;
										GameData.IO.SavePreferences(Current.Name, CurrentPreferences);
										mSaveNextAvailable &= ~ProfileComponents.Preferences;
								}

								if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused | FGameState.Saving)) {
										//Debug.Log ("Can't save profile components yet, we're not InGame, GamePaused or Saving");
										return;
								}

								if (Flags.Check((uint)mSaveNextAvailable, (uint)ProfileComponents.Game, Flags.CheckType.MatchAny)) {
										//save the whole world
										//TODO get all game data
										CurrentGame.LastTimeSaved = DateTime.Now;
										CurrentGame.GameTime = WorldClock.AdjustedRealTime;
										CurrentGame.Version = GameManager.VersionString;
										GameData.IO.SaveGame(CurrentGame);
								} else if (Flags.Check((uint)mSaveNextAvailable, (uint)ProfileComponents.Character, Flags.CheckType.MatchAny)) {
										//just save the character
										//don't bother with the rest of the world
										CurrentGame.LastTimeSaved = DateTime.Now;
										CurrentGame.GameTime = WorldClock.AdjustedRealTime;
										CurrentGame.Version = GameManager.VersionString;
										GameData.IO.SaveGame(CurrentGame);
								}

								if (Flags.Check((uint)mSaveNextAvailable, (uint)ProfileComponents.Profile, Flags.CheckType.MatchAny)) {
										Current.Version = GameManager.VersionString;
										GameData.IO.SaveProfile(Current);
								}

								mSaveNextAvailable = ProfileComponents.None;
								enabled = false;
						}
				}

				public bool SetWorldAndGame(string worldName, string gameName, bool saveGame, bool loadGame)
				{
						Debug.Log("PROFILE: Setting world name to " + worldName);
						if (!HasSelectedProfile || !HasCurrentProfile)
								return false;

						string errorMessage = string.Empty;
						Debug.Log("PROFILE: INITIALIZE DATA PATHS TO : " + Current.Name + ", " + worldName + ", " + gameName);
						if (!GameData.IO.InitializeLocalDataPaths(Current.Name, worldName, gameName, out errorMessage)) {
								Frontiers.GUI.GUILoading.DisplayError(errorMessage);
								Debug.LogError(errorMessage);
								return false;
						}

						//if we haven't already created this game
						if (!HasCurrentGame || CurrentGame.WorldName != worldName || CurrentGame.Name != gameName) {
								//try to load the game
								//if it doesn't exist, create it
								if (!Mods.Get.Runtime.LoadGame(ref mCurrentGame, worldName, gameName)) {//if the game doesn't exist, create it, then save it immediately
										mCurrentGame = new PlayerGame();
										mCurrentGame.Version = GameManager.VersionString;
										mCurrentGame.LastTimeSaved = DateTime.Now;
										mCurrentGame.GameTime = 0;
										mCurrentGame.WorldName = worldName;
										mCurrentGame.Name = gameName;
										mCurrentGame.HasStarted	= false;
										//create a default character for this game
										mCurrentGame.Character = PlayerCharacter.Default();
										Debug.Log("PROFILE: Didn't find game " + gameName + ", saving now");
								}
						}

						if (saveGame) {
								Mods.Get.Runtime.SaveGame(mCurrentGame);
						}

						if (loadGame) {
								Mods.Get.LoadLiveGame(gameName);
						}

						HasSelectedGame = true;
						return true;
				}

				public bool SetDifficulty(DifficultySetting setting)
				{
						if (!HasCurrentProfile) {
								Debug.Log("Couldn't set difficulty, had no profile");
								return false;
						}

						if (CurrentGame == null) {
								mCurrentGame = new PlayerGame();
						}

						CurrentGame.DifficultyName = setting.Name;
						CurrentGame.Difficulty = setting;
						return true;
				}

				public bool SetDifficulty(string difficultySettingName)
				{
						if (!HasCurrentProfile) {
								return false;
						}

						if (mCurrentGame == null) {
								mCurrentGame = new PlayerGame();
						}

						if (mCurrentGame.Difficulty == null || mCurrentGame.DifficultyName != difficultySettingName) {
								if (Mods.Get.Runtime.LoadMod <DifficultySetting>(ref mCurrentGame.Difficulty, "DifficultySetting", difficultySettingName)) {
										CurrentGame.DifficultyName = difficultySettingName;
								}
						}
						if (CurrentGame.Difficulty != null) {
								CurrentGame.Difficulty.Apply();
								return true;
						} else {
								Debug.Log("Couldn't set difficulty, none existed");
								return false;
						}
				}

				public bool SetCharacter()
				{
						return false;
				}

				public bool ValidateLoadGameNames(string worldName, string gameName)
				{
						List <string> gameNames = GameNames(worldName, false);
						if (gameNames.Contains(gameName)) {
								return true;
						}
						return false;
				}

				public override void OnGameStart()
				{
						SetDifficulty(mCurrentGame.DifficultyName);
						mCurrentGame.HasStarted = true;
				}

				public override void OnGameLoadFirstTime()
				{
						//copy the game from the live game immediately
						CurrentGame.HasLoadedOnce = true;
						Mods.Get.SaveLiveGame(CurrentGame.Name);
				}

				public override void OnGameLoadFinish()
				{
						CurrentGame.HasLoadedOnce = true;
						mGameLoaded = true;
				}

				public bool ValidateExistingGameName(string worldName, string gameName, out string error)
				{		//TODO remove this we don't need it any more
						error = "Choose an existing game";
						return true;
				}

				public bool ValidateNewGameName(string worldName, string gameName, out string cleanAlternative, out string error)
				{	
						Debug.Log("Validating new game name for " + worldName);
						//TODO remove error string we don't need it any more
						error = "Enter a game name:";
						cleanAlternative = GameData.IO.CleanGameName(gameName);
						if (string.IsNullOrEmpty(cleanAlternative) || cleanAlternative == "(Enter name)") {
								return false;
						} else {
								if (cleanAlternative.Length < Globals.MinProfileNameCharacters) {
										error = ("Names must be at least " + Globals.MinProfileNameCharacters.ToString() + " long");
										return false;
								}

								List <string> gameNames = GameNames(worldName, true);
								if (gameNames.Contains(cleanAlternative.ToLower())) {
										error = "That game name is taken";
										return false;
								}
						}
						return true;
				}

				public bool ValidateExistingProfileName(string profileName, out string error)
				{
						error = string.Empty;
						if (string.IsNullOrEmpty(profileName) || profileName == "(None)") {
								error = string.Empty;
								return false;
						}
						return true;
				}

				public bool ValidateNewProfileName(string profileName, out string error, out string cleanAlternative)
				{
						cleanAlternative = GameData.IO.CleanProfileName(profileName);

						if (string.IsNullOrEmpty(cleanAlternative) || cleanAlternative == "(Enter Name)") {
								error = string.Empty;
								cleanAlternative = string.Empty;
								return false;
						} else {
								if (cleanAlternative.Length < Globals.MinProfileNameCharacters) {
										error = ("Names must be at least " + Globals.MinProfileNameCharacters.ToString() + " long");
										return false;
								}
								//check against uppercase profile names to ensure no
								//duplicates with upper/lower variations
								List <string> profileNames = ProfileNames(true);
								if (profileNames.Contains(cleanAlternative.ToLower())) {
										error = "That profile name is taken";
										return false;
								}
						}
						error = string.Empty;
						return true;
				}

				public PlayerProfile mCurrentProfile = null;
				public PlayerGame mCurrentGame = null;
				public PlayerPreferences mCurrentPreferences = null;
				protected ProfileComponents mSaveNextAvailable = ProfileComponents.None;
		}
}
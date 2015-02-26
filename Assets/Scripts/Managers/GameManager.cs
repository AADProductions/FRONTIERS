using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Hydrogen.Serialization;
using Frontiers.Story;
using Frontiers.GUI;

namespace Frontiers
{
		public class GameManager : Manager
		{
				public static GameManager Get;
				public Camera GameCamera;
				public GameObject StartupScenePrefab;
				public static int BuildNumber;
				//TODO get this from steam somehow
				public static readonly System.Version Version = new Version(0, 4, 0);
				//since we're using this everywhere we don't want to call ToString on Version
				//believe it or not this actually has an effect on allocations / garbage
				public static readonly string VersionString = Version.ToString();
				public static readonly uint SteamAppID = 293480;
				public static readonly string FocusOnCheatCode = "skiptotheend";
				public static readonly string FocusOnSubject = "TrappingAndFishing";

				public static bool Is(FGameState state)
				{
						if (Cutscene.IsActive) {
								return Flags.Check((uint)FGameState.Cutscene, (uint)state, Flags.CheckType.MatchAny);
						}
						return Flags.Check((uint)State, (uint)state, Flags.CheckType.MatchAny);
				}
				//status of the game
				public static FGameState State {
						get {
								return gState;

						}
				}

				public static void SetState(FGameState newGameState)
				{
						switch (gState) {
								case FGameState.InGame:
										switch (newGameState) {
												case FGameState.GamePaused:
														Manager.GamePause();
														break;

												default:
														break;
										}
										break;

								case FGameState.GamePaused:
										switch (newGameState) {
												case FGameState.InGame:
														Manager.GameContinue();
														break;

												default:
														break;
										}
										break;

								case FGameState.WaitingForGame:
										break;

								default:
										break;
						}
				}
				//status of the host
				//not relevant outside of multiplayer
				//synced when client
				//not synced when host
				public static NHostState HostState {
						get {
								return gNHostState;
						}
						set {
								gNHostState = value;
						}
				}
				//status of the client
				//not relevant outside of multiplayer
				//ignored when host
				public static NClientState ClientState {
						get {
								return gNClientState;
						}
						set {
								gNClientState = value;
						}
				}
				//this property is duplicated for auto TNet syncing
				public int SyncedHostState {
						get {
								return (int)gNHostState;
						}
						set {
								gNHostState = (NHostState)value;
						}
				}
				//TODO remove this, Is (state) makes it unnecessary
				public static bool IsGameInProgress {
						get {
								return gState != FGameState.Startup;
						}
				}
				//dev tools, will remove
				public bool TestingEnvironment = false;
				public bool EnableAllSkills = true;
				public bool TerrainDetails = true;
				public bool Ocean = true;
				public bool JustLookingMode = false;
				public bool ConversationsInterpretScripts = true;
				public bool ConversationsWrapBracketedDialog = true;
				public bool NoSaveMode = false;
				public bool NoTreesMode = false;
				public TNObject NObject;

				public bool Is64Bit()
				{
						string pa = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
						return ((System.String.IsNullOrEmpty(pa) || pa.Substring(0, 3) == "x86") ? false : true);
				}

				public override void WakeUp()
				{
						//Application.targetFrameRate = 60; yeah right
						ExceptionHandler.SetupExceptionHandling();

						Debug.Log("Waking up game manager");
						Get = this;
						mParentUnderManager = false;
						//initialize data paths so we can load stuff
						string errorMessage = string.Empty;
						if (!GameData.IO.InitializeSystemPaths(out errorMessage)) {//aw shit son what did you do
								GUILoading.DisplayError(errorMessage);
								//StartCoroutine(WaitForAnyKeyToQuit());
								return;
						}
						//load our globals so we can initialize stuff
						List <KeyValuePair <string,string>> globalPairs = null;
						if (!GameData.IO.LoadGlobals(ref globalPairs, out errorMessage)) {
								GUILoading.DisplayError(errorMessage);
								return;
						}
						Globals.LoadData(globalPairs);
						GameData.IO.SaveGlobals(Globals.GetData());
						//set some stuff right off the bat
						//these are taken care of elsewhere after we've started up
						GameCamera.farClipPlane = Globals.ClippingDistanceFar;
						GameCamera.nearClipPlane = Globals.ClippingDistanceNear;
						//(removed rest)
				}

				public void SetOculusMode(bool oculusMode)
				{

				}

				public IEnumerator UpdateState()
				{		//the equvialent of main
						gState = FGameState.Startup;
						HostState = NHostState.WaitingToStart;
						ClientState = NClientState.WaitingToConnect;

						while (State != FGameState.Quitting) {
								switch (State) {
										default:
										case FGameState.Startup:
												break;

										case FGameState.WaitingForGame:
												break;

										case FGameState.GameLoading:
												break;

										case FGameState.Unloading:
												break;

										case FGameState.GameStarting:
												break;

										case FGameState.Cutscene:
										case FGameState.GamePaused:
												if (NetworkManager.Instance.IsHost) {
														if (NetworkManager.Instance.IsConnected) {
																HostState = NHostState.Started;
														} else {
																HostState = NHostState.WaitingToStart;
														}
														//HostState = NHostState.Paused;
												} else {
														//ClientState = NClientState.Paused;
												}
												//wait a tick
												yield return null;
												break;

										case FGameState.InGame:
												if (NetworkManager.Instance.IsHost) {
														//if we're the host, then the game can begin here
														HostState = NHostState.Started;
												}
												//TODO update stuff
												yield return null;
												break;

										case FGameState.Saving:
												break;

										case FGameState.Quitting:
												if (NetworkManager.Instance.IsHost) {
														//HostState = NHostState.Disconnected;
												}
												//quit after loop
												break;
								}
								yield return null;
						}
						//do quit stuff here
						yield break;
				}

				public void Update()
				{
						if (!mUpdatingState) {
								mUpdatingState = true;
								StartCoroutine(StartupGame());
								enabled = false;
						}

						if (mPauseOnNextUpdate) {
								mPauseOnNextUpdate = false;
								mContinueOnNextUpdate = false;
								Manager.GamePause();
								enabled = false;
						} else if (mContinueOnNextUpdate) {
								mContinueOnNextUpdate = false;
								mPauseOnNextUpdate = false;
								Manager.GameContinue();
								enabled = false;
						}
				}

				public IEnumerator InitializeGameEnvironment()
				{
						if (mInitializing || mInitialized)
								yield break;

						mInitializing = true;
						//wait a tick
						yield return WorldClock.WaitForRTSeconds(0.01f);
						//TODO move these into globals, use strings for types
						//to let modders add their own manager classes on startup
						//base
						yield return StartCoroutine(Manager.WakeUpAndInitialize <SteamManager>("Initializing Steam Manager"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Mods>("Initializing MODS"));
						//interface, input, GUI
						yield return StartCoroutine(Manager.WakeUpAndInitialize <UserActionManager>("Initializing user action manager"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <InterfaceActionManager>("Initializing interface action manager"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <GUIManager>("Initializing GUI manager"));
						//display & sound
						yield return StartCoroutine(Manager.WakeUpAndInitialize <CameraFX>("Initializing Camera FX"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <AudioManager>("Initializing audio manager"));
						//gameplay data
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Biomes>("Initializing Biomes"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Mats>("Frontiers_ArtResourceManagers"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Meshes>("Frontiers_ArtResourceManagers"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Creatures>("Initializing Creatures"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Critters>("Initializing Critters"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Characters>("Initializing Characters"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <WorldItems>("Initializing WorldItems"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <WIGroups>("Initializing WIGroups"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Structures>("Initializing Structures"));
						//gameplay objects
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Conversations>("Initializing Conversations"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Missions>("Initializing Missions"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Books>("Initializing Books"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Museums>("Initializing Museums"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Skills>("Initializing Skills"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Potions>("Initializing Potions"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Plants>("Initializing Plants"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Conditions>("Initializing Status Conditions"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Blueprints>("Initializing Blueprints"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <PreparedFoods>("Initializing Foods"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Moneylenders>("Initializing Moneylenders"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Paths>("Initializing Paths"));
						//world and player
						yield return StartCoroutine(Manager.WakeUpAndInitialize <GameWorld>("Initializing Game World"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Ocean>("Initializing Ocean"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <LightManager>("Initializing Lights"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <DarkrotManager>("Initializing Darkrot"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <SpawnManager>("Initializing Spawn Manager"));
						yield return StartCoroutine(Manager.WakeUpAndInitialize <Player>("Initializing Player"));

						Manager.FinishedInitializing();
						//load all data from disk
						//wait a tick
						yield return null;
						//create the startup scene
						mStartupScenePrefab = GameObject.Instantiate(StartupScenePrefab) as GameObject;
						Biomes.Get.UseTimeOfDayOverride = true;
						Biomes.Get.HourOfDayOverride = UnityEngine.Random.Range(0f, 24f);
						mInitializing = false;
						mInitialized = true;
						//pause after initializing
						//do this now while the player won't notice a hitch
						System.GC.Collect();
						Resources.UnloadUnusedAssets();
						//we'll be waiting for the start menu
				}

				protected IEnumerator StartupGame()
				{
						while (GUILoading.SplashScreen) {	//wait for splash screen
								yield return null;
						}
						yield return StartCoroutine(GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack));
						GUILoading.Lock(this);
						GUILoading.DetailsInfo = "Starting up...";
						yield return StartCoroutine(InitializeGameEnvironment());
						GUILoading.Unlock(this);
						yield return StartCoroutine(GUILoading.LoadFinish());
						//initialization only happens once
						//after we're done, wait for the start menu to choose a game
						gState = FGameState.WaitingForGame;
						if (TestingEnvironment) {
								yield return StartCoroutine(WaitForGameTestingEnvironment());
						} else {
								yield return StartCoroutine(WaitForGame());
						}
						yield break;
				}

				protected IEnumerator WaitForGame()
				{
						//otherwise wait for the player to select a profile
						GameObject profileChildEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUISelectProfileDialog, false);
						PlayerProfileSelectionResult profileSelectResult = new PlayerProfileSelectionResult();
						GUIManager.SendEditObjectToChildEditor <PlayerProfileSelectionResult>(profileChildEditor, profileSelectResult);
						//don't bother with a callback, just chill out
						while (!Profile.Get.HasSelectedProfile) {
								yield return null;
						}
						//apply settings
						Profile.Get.ApplyPreferences();//necessary?
						//wait a tick
						yield return null;
						//now launch the start menu
						SpawnStartMenu(FGameState.WaitingForGame, FGameState.WaitingForGame);
						//we can't do anything until player has selected game
						yield break;
				}

				protected IEnumerator UnloadOverTime()
				{		//TODO look for other spots where we can unload assets
						//music, etc.
						System.GC.Collect();
						Resources.UnloadUnusedAssets();
						yield break;
				}
				//loads the current game and world
				//starts it up
				protected IEnumerator LoadOverTime()
				{
						Biomes.Get.UseTimeOfDayOverride = false;
						if (mStartupScenePrefab != null) {
								GameObject.Destroy(mStartupScenePrefab, 0.5f);
						}
						//Load all the things!
						yield return StartCoroutine(GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack));
						GUILoading.Lock(this);
						string detailsInfo = string.Empty;
						//load all textures first
						//------------------
						Manager.TexturesLoadStart();
						//------------------
						yield return null;
						GUILoading.ActivityInfo = "Loading Textures";
						GUILoading.DetailsInfo = "Compiling mods";
						while (!Manager.FinishedLoadingTextures) {
								yield return null;
						}
						GUILoading.ActivityInfo = "Generating World";
						GUILoading.DetailsInfo = "Compiling mods";
						yield return null;
						//------------------
						Manager.ModsLoadStart();
						//------------------
						//wait for the actual mods manager to finish loading mods
						while (!Mods.Get.ModsLoaded) {
								yield return null;
						}
						//------------------
						Manager.ModsLoadFinish();
						//------------------
						yield return null;
						//then jump straight ahead to the world loading
						GUILoading.ActivityInfo = "Loading World";
						GUILoading.DetailsInfo = "Loading mods from generated world";
						//during this section we report on what the world manager is doing
						//it accomplishes nothing but looks nice on the loading screen
						GUILoading.ActivityInfo = "Creating World";
						while (GameWorld.Get.LoadingGameWorld(out detailsInfo)) {//report GameWorld progress as it loads the game
								yield return null;
								GUILoading.DetailsInfo = detailsInfo;
						}
						//wait for other mods as normal
						//------------------
						while (!Manager.FinishedLoadingMods) {
								//Debug.Log ("Waiting for mods to finish loading");
								yield return null;
						}
						//this only happens once per game
						if (!Profile.Get.CurrentGame.HasLoadedOnce) {
								GUILoading.ActivityInfo = "Creating World for First Time";
								//Debug.Log ("Creating world for first time");
								//------------------
								Manager.GameLoadStartFirstTime();
								//------------------
						}
						yield return null;
						//------------------
						Manager.GameLoadStart();
						//------------------
						//give managers a tick to figure out what they're doing
						yield return null;
						//------------------
						Manager.GameLoadFinish();
						//------------------
						while (!Manager.FinishedLoading) {
								//Debug.Log ("Waiting to finish loading...");
								yield return null;
						}
						//during this section we report on what the spawn manager is up to
						while (SpawnManager.Get.SpawningPlayer(out detailsInfo)) {
								yield return null;
								GUILoading.DetailsInfo = detailsInfo;
						}
						//start the game!
						gState = FGameState.GameStarting;
						//wait a tick
						yield return null;
						//this only happens once per game
						if (!Profile.Get.CurrentGame.HasStarted) {
								//------------------
								Manager.GameStartFirstTime();
								//------------------
								Profile.Get.CurrentGame.HasStarted = true;
								yield return null;
						}
						//------------------
						Manager.GameStart();
						//------------------
						yield return null;
						if (NetworkManager.Instance.IsHost) {
								//if we're the host, then the game can begin here
								//HostState = NHostState.Started;
						}
						//we pause the game immediately while we wait for the player to spawn
						//------------------
						Manager.GamePause();
						//------------------
						yield return null;
						//save the game so we know that we've started once
						Profile.Get.SaveCurrent(ProfileComponents.Profile);
						//wait for the player to finish spawning then turn off the loading screen
						while (!Player.Local.HasSpawned) {
								Debug.Log("Waiting for player to spawn");
								yield return null;
						}
						//turn off the loading screen
						GUILoading.Unlock(this);
						yield return StartCoroutine(GUILoading.LoadFinish());
						//now that the player has spawned continue the game
						//------------------
						Manager.GameContinue();
						//------------------
						//this will set the state to InGame
						yield break;
				}

				protected IEnumerator SaveOverTime()
				{
						Debug.Log("Game Manager: Saving over time");
						Manager.GamePause();
						Manager.GameSave();
						while (!Manager.FinishedSaving) {
								//Debug.Log("Waiting for save...");
								yield return null;
						}
						Manager.GameContinue();
						yield break;
				}

				public override void OnCutsceneStart()
				{

				}

				public override void OnCutsceneFinished()
				{
						GameObject.DestroyImmediate(Cutscene.CurrentCutscene.gameObject);
						Cutscene.CurrentCutscene = null;
						Get.StartCoroutine(Get.UnloadUnusedAssets());
						GC.Collect();
				}

				protected IEnumerator UnloadUnusedAssets()
				{	//wait a tick
						yield return null;
						Resources.UnloadUnusedAssets();
				}
				//TODO make this ia function not a property
				public static bool ReadyToQuit {
						get {
								if (Is(FGameState.Startup | FGameState.Quitting | FGameState.WaitingForGame)) {
										Debug.Log("We're startup / quitting / waiting for game - read to quit");
										return true;
								}

								if (Is(FGameState.GameLoading)) {
										Debug.Log("We're loading - have to wait for load to stop - NOT ready to quit");
										return false;
								}

								if (!Profile.Get.HasCurrentGame) {
										Debug.Log("We don't have a current game - ready to quit");
										return true;
								}

								if (Profile.Get.CurrentGame.HasBeenSavedRecently) {
										Debug.Log("Our game was saved recently - ready to quit");
										return true;
								}

								return false;
						}
				}

				protected IEnumerator WaitForQuit()
				{
						while (!Manager.FinishedQuitting) {
								yield return null;
						}
						Application.Quit();
				}

				protected IEnumerator WaitForUnload()
				{
						yield return StartCoroutine(GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack));
						GUILoading.Lock(this);
						GUILoading.ActivityInfo = "Unloading World";
						string detailsInfo = "Unloading in game manager";
						GUILoading.DetailsInfo = "Waiting for managers to unload...";
						while (!Manager.FinishedUnloading) {
								while (GameWorld.Get.UnloadingGameWorld(out detailsInfo)) {	//report GameWorld progress as it loads the game
										yield return null;
										GUILoading.DetailsInfo = detailsInfo;
								}
								Debug.Log("Waiting for manager to finish unloading...");
								yield return null;
						}
						Application.LoadLevel("BlankLevel");//this nukes anything but managers and worlditems
						GUILoading.Unlock(this);
						yield return StartCoroutine(GUILoading.LoadFinish());
						gState = FGameState.GameLoading;
				}

				protected IEnumerator WaitForGameTestingEnvironment()
				{
						//set the default profile
						//tell the game world to load a 'blank slate'
						//set the mode to in-game
						string errorMessage;
						Profile.Get.SetOrCreateProfile("Testing", out errorMessage);
						Profile.Get.SetWorldAndGame(GameData.IO.gModWorldFolderName, "Game01", true, true);
						Profile.Get.CurrentGame.HasStarted = true;
						Manager.GameStart();
						//yield return StartCoroutine (GameWorld.Get.LoadBlankChunk ());
						yield break;
				}

				#region manager overrides

				public override void OnGameSave()
				{
						gState = FGameState.Saving;
						mGameSaved = true;
				}

				public override void OnLocalPlayerDie()
				{
						/*
					if (State == FGameState.InGame) {
						State = FGameState.GamePaused;
					}
					*/
				}

				public override void OnLocalPlayerSpawn()
				{
						if (gState == FGameState.GamePaused) {
								Manager.GameContinue();
						}
				}

				public override void OnLocalPlayerDespawn()
				{
						/*
						if (State == FGameState.InGame) {
							State = FGameState.GamePaused;
						}
						*/
				}

				public override void OnGamePause()
				{
						gState = FGameState.GamePaused;
				}

				public override void OnGameContinue()
				{
						gState = FGameState.InGame;
				}

				public override void OnGameStart()
				{
						gState = FGameState.InGame;
				}

				public override void OnGameQuit()
				{
						Debug.Log("OnGameQuit in game manager");
						gState = FGameState.Quitting;
						StartCoroutine(WaitForQuit());
				}

				#endregion

				protected bool mUpdatingState = false;
				protected bool mPauseOnNextUpdate = false;
				protected bool mContinueOnNextUpdate = false;

				#region convenience functions

				//these functions make it easy to set the game state quickly
				//but there are rules to prevent infinite loops
				//----
				public static void Pause()
				{
						//pause and continue take place in Update
						//this is to prevent 'pause spamming'
						if (gState == FGameState.InGame && !Get.mPauseOnNextUpdate) {
								Get.mPauseOnNextUpdate = true;
								Get.enabled = true;
						}

						/*
						if (gState == FGameState.InGame) {
							Manager.GamePause ();
						}
						*/
				}

				public static void Continue()
				{
						//pause and continue take place in Update
						//this is to prevent 'pause spamming'
						if (gState == FGameState.GamePaused && !Get.mContinueOnNextUpdate) {
								Get.mContinueOnNextUpdate = true;
								Get.enabled = true;
						}

						/*
						if (gState == FGameState.Paused) {
							Manager.GameContinue ();
						}
						*/
				}

				public static void Load()
				{
						gState = FGameState.GameLoading;
						Get.StartCoroutine(Get.LoadOverTime());
				}

				public static void Save()
				{
						gState = FGameState.Saving;
						Get.StartCoroutine(Get.SaveOverTime());
				}

				public static void Unload()
				{
						gState = FGameState.Unloading;
						Get.StartCoroutine(Get.UnloadOverTime());
				}

				public static void Quit()
				{
						if (!ReadyToQuit) {
								GameManager.Get.StartCoroutine(GameManager.Get.TryToQuitOverTime());
						} else {
								Manager.GameQuit();
						}
				}

				public void GameOver(string reason)
				{
						//TODO implement game over
				}

				#endregion

				public void OnApplicationQuit()
				{
						Debug.Log("OnApplicationQuit called in gamemanager");
						if (!ReadyToQuit) {
								Debug.Log("We're not ready to quit, canceling quit");
								//this causes weird problems so i'm disabling it
								//unity isn't very good about handling quit / cancel quit
								//Application.CancelQuit ();
						} else {
								Debug.Log("We're ready to quit");
						}
				}

				public IEnumerator TryToQuitOverTime()
				{

						Debug.Log("Quitting over time");
						if (Profile.Get.HasCurrentGame && !Profile.Get.CurrentGame.HasBeenSavedRecently) {
								yield return StartCoroutine(SpawnSaveGameDialog(true, false));
						}

						if (ReadyToQuit) {
								Manager.GameQuit();
						}
						yield break;
				}

				public IEnumerator SpawnSaveGameDialog(bool confirmFirst, bool canCancel)
				{
						bool spawnSaveGameDialog = true;

						if (confirmFirst) {
								spawnSaveGameDialog = false;
								GameObject yesNoDialogChildEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog, false);
								GUIYesNoCancelDialog confirmDialog = yesNoDialogChildEditor.GetComponent <GUIYesNoCancelDialog>();
								YesNoCancelDialogResult confirmResult = new YesNoCancelDialogResult();
								confirmResult.CancelButton = canCancel;
								confirmResult.MessageType = "Save Current Game?";
								//confirmResult.Message = "Click 'Yes' to save before quitting. Click 'Cancel' to return to the game.";
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(yesNoDialogChildEditor, confirmResult);

								while (!confirmDialog.IsFinished) {
										yield return null;
								}

								//deal with the result
								switch (confirmResult.Result) {
										case DialogResult.Cancel:
										default:
												//we'll exit without setting the confirm game state
												spawnSaveGameDialog = false;
												break;

										case DialogResult.No:
												Profile.Get.CurrentGame.LastTimeDeclinedToSave = System.DateTime.Now;
												spawnSaveGameDialog = false;
												break;

										case DialogResult.Yes:
												Debug.Log("Save game");
												spawnSaveGameDialog = true;
												break;
								}
						}

						if (spawnSaveGameDialog) {
								/*
							GameObject saveGameEditor = GUIManager.SpawnNGUIChildEditor (gameObject, GUIManager.Get.NGUISaveGameDialog, false);
							GUISaveGameDialog saveGameDialog = saveGameEditor.GetComponent <GUISaveGameDialog> ();
							SaveGameDialogResult saveGameResult = new SaveGameDialogResult ();
							GUIManager.SendEditObjectToChildEditor <SaveGameDialogResult> (saveGameEditor, saveGameResult);
							while (!saveGameDialog.IsFinished) {
								yield return null;
							}
							*/
						}

						yield break;
				}

				public void SpawnStartMenu(FGameState enterGameState, FGameState exitGameState)
				{

						GameObject startMenuChildEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIStartMenu, false);
						//GUIStartMenu startMenu = startMenuChildEditor.GetComponent <GUIStartMenu> ();
						StartMenuResult result = new StartMenuResult();
						result.EnterGameState = enterGameState;
						result.ExitGameState = exitGameState;
						GUIManager.SendEditObjectToChildEditor <StartMenuResult>(startMenuChildEditor, result);
				}

				protected bool mInitializing = false;
				protected GameObject mStartupScenePrefab = null;
				protected static FGameState gState = FGameState.Startup;
				protected static NHostState gNHostState = NHostState.None;
				protected static NClientState gNClientState = NClientState.None;
				#if UNITY_EDITOR
				public static void DrawEditor()
				{
						GUILayout.Label("State: " + GameManager.State.ToString());
						GUILayout.Label("Game in progress: " + GameManager.IsGameInProgress.ToString());
						GUILayout.Label("Global data path: " + GameData.IO.gGlobalDataPath);
						GUILayout.Label("Global profiles path: " + GameData.IO.gGlobalProfilesPath);
						GUILayout.Label("Current game path: " + GameData.IO.gCurrentGamePath);
						GUILayout.Label("Current profile live game path: " + GameData.IO.gCurrentProfileLiveGamePath);
						GUILayout.Label("Base world path: " + GameData.IO.gBaseWorldPath);
						GUILayout.Label("Current world path: " + GameData.IO.gCurrentWorldPath);
						if (Application.isPlaying && Get != null) {
								UnityEditor.EditorUtility.SetDirty(Get.gameObject);
						}
				}
				#endif
		}

		public static class ExceptionHandler
		{
				static bool isExceptionHandlingSetup;

				public static void SetupExceptionHandling()
				{
						if (!isExceptionHandlingSetup) {
								isExceptionHandlingSetup = true;
								Application.RegisterLogCallback(HandleException);
						}
				}

				static void HandleException(string condition, string stackTrace, LogType type)
				{
						if (type == LogType.Error || type == LogType.Exception) {
								Debug.Log("CUSTOM HANDLING: " + type.ToString() + ": " + condition + "\n" + stackTrace);
						}
				}
		}
}
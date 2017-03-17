using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
	//[ExecuteInEditMode]
	public class Manager : MonoBehaviour
	{
		//load order:
		//
		//---Player starts program
		//
		//WakeUp - called by Awake in each manger
		//Initialize - called in arbitrary order by GameManager
		//OnInitialized - called externally after ALL managers report Initialized
		//
		//---Player sets profile and game
		//---Player starts new game and/or loads game
		//
		//ModsLoadStart->OnModsLoadStart
		//ModsLoadFinish->OnModsLoadFinish - called after ALL managers report ModsLoaded
		//ModsLoaded->OnModsLoaded - called after ModsLoadFinish
		//TextureLoadStart->OnTextureLoadStart - we put this in its own step because unity's memory is so fragile
		//TextureLoadFinish->OnTextureLoadFinish - called after ALL managers report TexturesLoaded
		//GameLoadStart->OnGameLoadStart - games are 'loaded' even when there is no save game
		//GameLoadFinish->OnGameLoadFinish - called after ALL managers report GameLoaded
		//CreateWorld->OnCreateWorld - called in arbitrary order after GameLoaded
		//GameStartFirstTime->OnGameStartFirstTime - called if this game has never been started before
		//GameStart->OnGameStart - called immediately after OnGameStartFirstTime
		//LocalPlayerSpawn->OnLocalPlayerSpawn - called when player spawns. this happens every time the player dies and respawns as well.
		//RemotePlayerSpawn->OnRemotePlayerSpawn - called when remote player spawns. this happens every time the remote player dies and respawns as well.
		//LocalPlayerDie->OnLocalPlayerDie
		//RemotePlayerDie->OnRemotePlayerDie
		//
		//---Player plays game
		//
		//GamePause->OnGamePause, OnGameTimePause - the application loses focus or goes to the main menu
		//GameContinue->OnGameContinue, OnGameTimeContinue - the application regains focus or the main menu closes
		//GameTimePause->OnGameTimePause - the world clock stops, usually because of interface
		//GameTimeContinue->OnGameTimeContinue - the world clock resumes, usually because interface is closed
		//
		//GameSaveStart->OnGameSaveStart - called when the player forces a save
		//GameSaveFinish->OnGameSaveFinish - called after ALL managers report GameSaved (GameSaved is reset afterwards)
		//
		//GameEndStart->OnGameEndStart - called when player loads a new game or quits
		//GameEndFinish->OnGameEndFinish - called when ALL managers report GameEnded
		//GameEnd->OnGameEnd - one last chance to clean everything up before the world is destroyed
		//DestroyWorld->OnDestroyWorld - break down the world. it's assumed that everything is saved at this point
		//
		//---Player quits
		public static GameObject ManagersParent;
		public static string DetailsInfo;

		public virtual string GameObjectName {
			get {
				return "Frontiers_Manager";
			}
		}

		public bool Initialized {
			get {
				return mInitialized;	
			}
		}

		public bool TexturesLoaded {
			get {
				return mTexturesLoaded;
			}
		}

		public bool ModsLoaded {
			get {
				return mModsLoaded;
			}
		}

		public bool GameLoaded {
			get {
				return mGameLoaded;
			}
		}

		public bool GameSaved {
			get {
				return mGameSaved;
			}
		}

		public bool GameEnded {
			get {
				return mGameEnded;
			}
		}

		protected bool mParentUnderManager	= true;

		#region broadcasters

		public static void 							FinishedInitializing ()
		{
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnInitialized ();
			}
		}

		public static void							TexturesLoadStart ()
		{
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnTextureLoadStart ();
			}
		}

		public static void							TexturesLoadFinish ()
		{
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnTextureLoadFinish ();
			}
		}

		public static void							ModsLoadStart ()
		{	//Debug.Log ("MANAGER: MODS LOAD START");
			//this is a period where managers have free reign to load assets via Mods
			//all override artwork & meshes etc. are loaded at this time
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnModsLoadStart ();
			}
		}

		public static void							ModsLoadFinish ()
		{	//Debug.Log ("MANAGER: MODS LOAD FINISH");
			//after all managers have reported back 'ModsLoaded' this is called
			//OnModsLoadFinish is called in all managers, where texture & prefab references are destroyed
			//UnloadUnusedAssets is called
			//finally OnModsLoaded is called in all managers
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnModsLoadFinish ();
			}
		}

		public static void							LocalPlayerSpawn ()
		{	//Debug.Log ("MANAGER: LOCAL PLAYER SPAWN");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnLocalPlayerSpawn ();
			}
		}

		public static void							LocalPlayerDespawn ()
		{	//Debug.Log ("MANAGER: LOCAL PLAYER DESPAWN");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnLocalPlayerDespawn ();
			}
		}

		public static void							RemotePlayerSpawn ()
		{	//Debug.Log ("MANAGER: REMOTE PLAYER SPAWN");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnRemotePlayerSpawn ();
			}
		}

		public static void							LocalPlayerDie ()
		{	//Debug.Log ("MANAGER: LOCAL PLAYER DIE");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnLocalPlayerDie ();
			}
		}

		public static void							RemotePlayerDie ()
		{	//Debug.Log ("MANAGER: REMOTE PLAYER DIE");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnRemotePlayerDie ();
			}
		}

		public static void							GameStartFirstTime ()
		{	//Debug.Log ("MANAGER: GAME START FIRST TIME");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameStartFirstTime ();
		}

		public static void							GameStart ()
		{	//Debug.Log ("MANAGER: GAME START");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameStart ();
		}

		public static void							GameSave ()
		{
			Debug.Log ("MANAGER: GAME SAVE START");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				//Debug.Log("Save starting in " + awakeManager.name);
				awakeManager.OnGameSaveStart ();
			}
			Debug.Log ("MANAGER: GAME SAVE");
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				//Debug.Log("Save in " + awakeManager.name);
				awakeManager.OnGameSave ();
			}
		}

		public static void							GameLoadStartFirstTime ()
		{	//Debug.Log ("MANAGER: GAME LOAD START FIRST TIME");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameLoadFirstTime ();
		}

		public static void							GameLoadStart ()
		{	//Debug.Log ("MANAGER: GAME LOAD START");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameLoadStart ();
		}

		public static void							GameLoadFinish ()
		{	//Debug.Log ("MANAGER: GAME LOAD FINISH");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameLoadFinish ();
		}

		public static void							GameUnload ()
		{	//Debug.Log ("MANAGER: GAME UNLOAD");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameUnload ();
		}

		public static void							GamePause ()
		{	//Debug.Log ("MANAGER: GAME PAUSE");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGamePause ();
		}

		public static void							GameContinue ()
		{	//Debug.Log ("MANAGER: GAME CONTINUE");
			foreach (Manager awakeManager in mAwakeManagers.Values)
				awakeManager.OnGameContinue ();
		}

		public static void GameQuit ()
		{	//Debug.Log ("MANAGER: GAME QUIT");
			if (!Application.isEditor) {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					awakeManager.OnGameQuit ();
				}
			}
		}

		public static void CutsceneStart ()
		{
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnCutsceneStart ();
			}
		}

		public static void CutsceneFinished ()
		{
			foreach (Manager awakeManager in mAwakeManagers.Values) {
				awakeManager.OnCutsceneFinished ();
			}
		}

		#endregion

		public static bool FinishedLoadingTextures {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (!awakeManager.TexturesLoaded) {
						//Debug.Log ("Manager " + awakeManager.name + " is not finished loading textures");
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED LOADING MODS");
				return true;
			}
		}

		public static bool FinishedLoadingMods {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (!awakeManager.ModsLoaded) {
						////Debug.Log (awakeManager.name + " hasn't loaded mods");
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED LOADING MODS");
				return true;
			}
		}

		public static bool FinishedLoading {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (!awakeManager.GameLoaded) {
						//Debug.Log (awakeManager.name + " hasn't finished loading");
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED LOADING TRUE");
				return true;
			}
		}

		public static bool FinishedUnloading {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (awakeManager.GameLoaded) {
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED UNLOADING TRUE");
				return true;
			}
		}

		public static bool FinishedQuitting {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (!awakeManager.GameEnded) {
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED QUITTING TRUE");
				return true;
			}
		}

		public static bool FinishedSaving {
			get {
				foreach (Manager awakeManager in mAwakeManagers.Values) {
					if (!awakeManager.GameSaved) {
						//Debug.Log ("Manager " + awakeManager.name + " hasn't finished saving yet");
						return false;
					}
				}
				//Debug.Log ("MANAGER: FINISHED SAVING TRUE");
				return true;
			}
		}

		public static IEnumerator WakeUpAndInitialize <T> (string detailsInfo) where T : Manager
		{
			if (Application.isPlaying) {
				try {
					Debug.Log ("Waking up and initializing " + typeof(T).ToString ());
				} catch (Exception e) {
					Debug.LogError ("ERROR DURING WAKINT UP " + typeof(T).ToString () + " : " + e.InnerException.ToString ());
				}
			}
			bool objectAwake = false;
			DetailsInfo = detailsInfo;
			while (!objectAwake) {
				try {
					objectAwake = IsAwake <T> ();
				} catch (Exception e) {
					Debug.LogError ("ERROR DURING WAKINT UP " + typeof(T).ToString () + " : " + e.InnerException.ToString ());
				}
				yield return null;
			}
			bool objectInitialized = false;
			try {
				Initialize <T> ();
			} catch (Exception e) {
				Debug.LogError ("ERROR DURING INTIALIZING " + typeof(T).ToString () + " : " + e.ToString ());
			}
			while (!objectInitialized) {
				////Debug.Log ("Waiting for " + typeof(T).ToString () + " to initialize");
				objectInitialized = IsInitialized <T> ();
				yield return null;
			}
			yield break;
		}

		public static void WakeUp <T> (string objectName) where T : Manager
		{
			GameObject managerObject = GameObject.Find (objectName);
			T manager = managerObject.GetComponent <T> ();
			manager.WakeUp ();
		}

		public static bool IsAwake <T> () where T : Manager
		{
			if (mAwakeManagers == null) {
				mAwakeManagers = new Dictionary<Type, Manager> ();
				return false;
			}
			return mAwakeManagers.ContainsKey (typeof(T));
		}

		public static bool IsInitialized <T> () where T : Manager
		{
			Manager awakeManager = null;
			if (mAwakeManagers.TryGetValue (typeof(T), out awakeManager)) {
				return awakeManager.Initialized;
			}
			return false;
		}

		public static void Initialize <T> () where T : Manager
		{
			Manager awakeManager = null;
			if (mAwakeManagers.TryGetValue (typeof(T), out awakeManager)) {
				if (!awakeManager.Initialized) {
					awakeManager.Initialize ();
					awakeManager.transform.SetAsFirstSibling ();
				}
			}
		}

		public virtual void WakeUp ()
		{

		}

		public virtual void Awake ()
		{	
			WakeUp ();

			if (mIsAwake) {	//sometimes necessary for managers that execute in edit mode
				//unity will call Awake twice on some singletons
				return;
			}

			mIsAwake = true;

			if (Application.isPlaying && this.transform.root == null) {
				DontDestroyOnLoad (this.transform);		
			}
			if (ManagersParent == null) {
				ManagersParent = GameObject.Find ("=MANAGERS=");
				if (ManagersParent == null) {
					ManagersParent = new GameObject ("=MANAGERS=");
				}
				if (Application.isPlaying) {
					DontDestroyOnLoad (ManagersParent);
				}
			}
			if (mParentUnderManager) {
				transform.parent = ManagersParent.transform;
			}

			if (mAwakeManagers == null) {
				mAwakeManagers = new Dictionary <Type, Manager> ();
			}
			if (mAwakeManagers.ContainsKey (this.GetType ())) {
				mAwakeManagers [this.GetType ()] = this;
			} else {
				mAwakeManagers.Add (this.GetType (), this);
			}
		}

		public virtual void Initialize ()
		{
			mInitialized = true;
		}

		public virtual void OnInitialized ()
		{

		}

		public virtual void OnTextureLoadStart ()
		{

		}

		public virtual void OnTextureLoadFinish ()
		{
			mTexturesLoaded = true;
		}

		public virtual void OnModsLoadStart ()
		{
			mModsLoaded = true;
		}

		public virtual void OnModsLoadFinish ()
		{
			mModsLoaded = true;
		}

		public virtual void OnGameStartFirstTime ()
		{

		}

		public virtual void OnGameReset ()
		{
			
		}

		public virtual void OnGameSaveStart ()
		{
			//mGameSaved = false;
		}

		public virtual void OnGameSave ()
		{
			mGameSaved = true;	
		}

		public virtual void OnGameLoadFirstTime ()
		{

		}

		public virtual void OnGameLoadStart ()
		{
			mGameLoaded = false;
		}

		public virtual void OnGameLoadFinish ()
		{
			mGameLoaded = true;
		}

		public virtual void OnLocalPlayerSpawn ()
		{
			//mGameSaved = false;
		}

		public virtual void OnLocalPlayerDespawn ()
		{
			//mGameSaved = false;
		}

		public virtual void OnRemotePlayerSpawn ()
		{
			//mGameSaved = false;
		}

		public virtual void OnLocalPlayerDie ()
		{
			//mGameSaved = false;
		}

		public virtual void OnRemotePlayerDie ()
		{
			//mGameSaved = false;
		}

		public virtual void OnGameStart ()
		{
			//mGameSaved = false;
		}

		public virtual void OnGameUnload ()
		{
			mGameLoaded = false;
		}

		public virtual void OnGamePause ()
		{
			//mGameSaved = false;
		}

		public virtual void OnGameContinue ()
		{
			//mGameSaved = false;
		}

		public virtual void OnExitProgram ()
		{
			
		}

		public virtual void OnGameQuit ()
		{
			//mGameEnded = true;
		}

		public virtual void OnCutsceneStart ()
		{
			//mGameEnded = true;
		}

		public virtual void OnCutsceneFinished ()
		{
			//mGameEnded = true;
		}

		protected bool mIsAwake = false;
		protected bool mInitialized = false;
		protected bool mTexturesLoaded = false;
		protected bool mModsLoaded = false;
		protected bool mGameLoaded = false;
		protected bool mGameSaved = false;
		protected bool mGameEnded = false;
		protected static Dictionary <Type,Manager> mAwakeManagers;
		// = new Dictionary <Type,Manager> ();
	}

	public class ManagerBase : MonoBehaviour
	{
	}
}


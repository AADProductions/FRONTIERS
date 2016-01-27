using UnityEngine;
using System.Collections;
using Steamworks;

namespace Frontiers
{
		public class SteamManager : Manager
		{
				public static SteamManager Get;

				public override string GameObjectName {
						get {
								return "Frontiers_SteamManager";
						}
				}

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
				}

				public SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

				protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
				{
						Debug.LogWarning(pchDebugText);
				}

				public override void Initialize()
				{
						//#if UNITY_EDITOR
						mInitialized = true;
						return;
						//#endif

						try {
								// If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the 
								// Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

								// Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
								// remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
								// See the Valve documentation for more information: https://partner.steamgames.com/documentation/drm#FAQ
								if (SteamAPI.RestartAppIfNecessary((AppId_t)GameManager.SteamAppID)) {
										Debug.Log("APPLICATION NOT RUNNNING WITHIN STEAM - QUITTING");
										Application.Quit();
										return;
								}
						} catch (System.DllNotFoundException e) { // We catch this exception here, as it will be the first occurence of it.
								Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

								Application.Quit();
								return;
						}

						// Initialize the SteamAPI, if Init() returns false this can happen for many reasons.
						// Some examples include:
						// Steam Client is not running.
						// Launching from outside of steam without a steam_appid.txt file in place.
						// https://partner.steamgames.com/documentation/example // Under: Common Build Problems
						// https://partner.steamgames.com/documentation/bootstrap_stats // At the very bottom

						// If you're running into Init issues try running DbgView prior to launching to get the internal output from Steam.
						// http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx
						mInitialized = SteamAPI.Init();
						if (!mInitialized) {
								Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

								Application.Quit();
								return;
						}

						// Ensure that the user has logged into Steam. This will always return true if the game is launched
						// from Steam, but if Steam is at the login prompt when you run your game from outside of Steam,
						// while steam_appid.txt is present will return false.
						if (!SteamUser.BLoggedOn()) {
								Debug.LogError("[Steamworks.NET] Steam user must be logged in to play this game (SteamUser()->BLoggedOn() returned false).", this);

								Application.Quit();
								return;
						}

						base.Initialize();
				}
				// This should only ever get called after an Assembly reload, You should never Disable the Steamworks Manager yourself.
				private void OnEnable()
				{
						if (!mInitialized) {
								return;
						}

						if (m_SteamAPIWarningMessageHook == null) {
								// Set up our callback to recieve warning messages from Steam.
								// You must launch with "-debug_steamapi" in the launch args to recieve warnings.
								m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
								SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
						}
				}

				public static void WriteMiniDump(uint errorID)
				{
//			System.IntPtr errorIDPtr = new System.IntPtr (errorID);
//			uint exceptionCode = 1;
//			Steamworks.NativeMethods.SteamAPI_WriteMiniDump (exceptionCode, errorIDPtr, (uint) NativeMethods.ISteamAppList_GetAppBuildId ((AppId_t) GameManager.SteamAppID));
				}

				private void OnApplicationQuit()
				{
						if (!mInitialized) {
								return;
						}

						SteamAPI.Shutdown();
				}

				private void Update()
				{
						if (!mInitialized) {
								return;
						}

						// Run Steam client callbacks
						SteamAPI.RunCallbacks();
				}
		}
}
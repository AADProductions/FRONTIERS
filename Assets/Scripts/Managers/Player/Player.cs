using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using ExtensionMethods;
using Frontiers.Story.Conversations;

namespace Frontiers
{
		public class Player : Manager
		{
				public static LocalPlayer Local;
				public static bool HideCrosshair = true;

				public static bool ByID(PlayerIDFlag ID, out PlayerBase player)
				{
						return Get.mPlayers.TryGetValue(ID, out player);
				}

				public static IEnumerable <PlayerBase> Players {
						get {
								return Get.mPlayers.Values;
						}
				}

				public static Player Get;
				//manager class
				//action receivers (last stop for avatar and user actions)
				public UserActionReceiver UserActions;
				public AvatarActionReceiver AvatarActions;
				//prefabs for building local player
				public GameObject LocalFPSCameraSeatPrefab;
				public GameObject LocalPlayerBasePrefab;
				public GameObject LocalToolOffsetPrefab;
				public GameObject LocalCarrierOffsetPrefab;
				public GameObject LocalToolPrefab;
				public GameObject LocalCarrierPrefab;
				public GameObject LocalWindZone;

				public static int GetPlayerVariable(string variableName)
				{
						int defaultValue = 0;
						return GetPlayerVariable(variableName, ref defaultValue);
				}

				public static float NormalizePlayerVariable(int variableValue, int maxValue)
				{
						return ((float)variableValue) / maxValue;
				}

				public static int GetPlayerVariable(string variableName, ref int defaultValue)
				{
						defaultValue = 0;
						int variableValue = 0;
						switch (variableName) {
								default:
										break;

								case "PlayerMoney":
										return Local.Inventory.InventoryBank.BaseCurrencyValue;

								case "TotalBaseCurrency":
										return Local.Inventory.InventoryBank.BaseCurrencyValue;

								case "BronzeCurrency":
										return Local.Inventory.InventoryBank.Bronze;

								case "SilverCurrency":
										return Local.Inventory.InventoryBank.Silver;

								case "GoldCurrency":
										return Local.Inventory.InventoryBank.Gold;

								case "LumenCurrency":
										return Local.Inventory.InventoryBank.Lumen;

								case "WarlockCurrency":
										return Local.Inventory.InventoryBank.Warlock;

								case "GlobalReputation":
										return Profile.Get.CurrentGame.Character.Rep.GlobalReputation;

								case "PersonalReputation":
										if (Conversation.LastInitiatedConversation != null && Conversation.LastInitiatedConversation.SpeakingCharacter != null) {
												return Profile.Get.CurrentGame.Character.Rep.GetReputation(Conversation.LastInitiatedConversation.SpeakingCharacter.worlditem.FileName).FinalReputation;
										} else if (Barter.IsBartering) {
												return Profile.Get.CurrentGame.Character.Rep.GetReputation(Barter.CurrentSession.BarteringCharacter.worlditem.FileName).FinalReputation;
										}
										return Profile.Get.CurrentGame.Character.Rep.DefaultReputation;

								case "MovementSpeed":
										return Mathf.FloorToInt(Player.Local.MotorAccelerationMultiplier * 100);

								case "JumpForce":
										return Mathf.FloorToInt(Player.Local.MotorJumpForceMultiplier * 100);

								case "Visibility":
										return Mathf.FloorToInt(Player.Local.VisibilityMultiplier * 100);
						}
						return variableValue;
				}

				public PlayerBase CreateRemotePlayer(string remoteCharacterData)
				{
						//creates a remote player object
						//adds it to the lookup
						//creates its body
						//creates its group
						PlayerIDFlag ID = PlayerIDFlag.Player01;//get remote ID
						RemotePlayer remotePlayer = gameObject.CreateChild(ID.ToString()).gameObject.AddComponent <RemotePlayer>();
						mPlayers.Add(ID, remotePlayer);
						remotePlayer.ID = ID;
						remotePlayer.Group = WIGroups.GetOrAdd(ID.ToString(), WIGroups.Get.Player, remotePlayer);
						remotePlayer.Body = Characters.Get.PlayerBody(remotePlayer);

						return remotePlayer;
						//remote player will initialize itself after finding/creating all its parts
				}

				public void CreateLocalPlayer()
				{
						PlayerIDFlag ID = PlayerIDFlag.Player01;//get remote ID
						GameObject localPlayerBaseObject = GameObject.Instantiate(LocalPlayerBasePrefab) as GameObject;
						localPlayerBaseObject.transform.parent = transform;
						localPlayerBaseObject.name = "LocalPlayer";
						LocalPlayer localPlayer = localPlayerBaseObject.AddComponent <LocalPlayer>();
						mPlayers.Add(ID, localPlayer);
						localPlayer.ID = ID;
						localPlayer.Group = WIGroups.GetOrAdd(ID.ToString(), WIGroups.Get.Player, localPlayer);
						localPlayer.Body = Characters.Get.PlayerBody(localPlayer);

						Local = localPlayer;
						//local player will initialize itself after finding/creating all its parts
				}

				public override void WakeUp()
				{
						mParentUnderManager = false;
						DontDestroyOnLoad(transform.parent);
						Get = this;
						UserActionManager.PlayerReceiver = new ActionReceiver <UserActionType>(UserActions.ReceiveAction);
				}

				public override void Initialize()
				{
						CreateLocalPlayer();
						mInitialized = true;
				}

				public override void OnGameSaveStart()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameSaveStart();
						}
				}

				public override void OnGameSave()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameSave();
						}
						mGameSaved = true;
				}

				public override void OnModsLoadStart()
				{
						foreach (PlayerBase player in Players) {
								player.OnModsLoadStart();
						}
				}

				public override void OnModsLoadFinish()
				{
						foreach (PlayerBase player in Players) {
								player.OnModsLoadFinish();
						}

						mModsLoaded = true;
				}

				public override void OnGameLoadStart()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameLoadStart();
						}
				}

				public override void OnGameLoadFinish()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameLoadFinish();
						}

						mGameLoaded = true;
				}

				public override void OnGameStartFirstTime()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameStartFirstTime();
						}
				}

				public override void OnGameUnload()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameUnload();
								//this will cause the player to spawn
								//when the local player spawns
								//OnPlayerSpawn will be called
						}
						mGameLoaded = false;
				}

				public override void OnGameStart()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameStart();
								//this will cause the player to spawn
								//when the local player spawns
								//OnPlayerSpawn will be called
						}
				}

				public override void OnLocalPlayerSpawn()
				{
						foreach (PlayerBase player in Players) {
								player.OnLocalPlayerSpawn();
						}
				}

				public override void OnLocalPlayerDespawn()
				{
						foreach (PlayerBase player in Players) {
								player.OnLocalPlayerDespawn();
						}
				}

				public override void OnRemotePlayerSpawn()
				{
						foreach (PlayerBase player in Players) {
								player.OnRemotePlayerSpawn();
						}
				}

				public override void OnLocalPlayerDie()
				{
						foreach (PlayerBase player in Players) {
								player.OnLocalPlayerDie();
						}
				}

				public override void OnRemotePlayerDie()
				{
						foreach (PlayerBase player in Players) {
								player.OnRemotePlayerDie();
						}
				}

				public override void OnGamePause()
				{
						foreach (PlayerBase player in Players) {
								player.OnGamePause();
						}
				}

				public override void OnGameContinue()
				{
						foreach (PlayerBase player in Players) {
								player.OnGameContinue();
						}
				}

				protected Dictionary <PlayerIDFlag, PlayerBase> mPlayers = new Dictionary <PlayerIDFlag, PlayerBase>();
		}
}
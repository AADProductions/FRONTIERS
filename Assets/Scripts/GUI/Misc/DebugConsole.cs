using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using Frontiers.World;
using Steamworks;
using Frontiers.Data;
using Frontiers.World.Gameplay;

namespace Frontiers
{
		public class DebugConsole : MonoBehaviour
		{
				//this is an ugly memory-hungry dev tool, it'll be removed in the final game
				public static DebugConsole Get;
				public bool showConvoVariables = true;
				public bool show = false;
				public bool showHelp = true;
				public string helpLabel;
				public string posLabel;
				public string ConsoleText;
				public string BuildID = string.Empty;
				public List <string> Log = new List <string>();
				public int lastIndex = 0;
				public int consoleColorIndex = 0;
				public List <Color> ConsoleColors = new List<Color>() {
						Color.red,
						Color.white,
						Color.yellow,
						Color.black,
						Color.blue
				};
				Motile lastMotile;
				Listener lastListener;
				Looker lastLooker;
				Creature lastCreature;
				Photosensitive lastPs;
				Hostile lastHostile;

				public void Awake()
				{
						Get = this;
						List <string> help = new List<string>();
						help.Add("======== keys =======");
						help.Add("LEFT / RIGHT: prev / next character");
						help.Add("PAGE UP / DOWN: prev / next conversation");
						help.Add("HOME: enable / disable conversation debug labels");
						help.Add("UP / DOWN: cycle previous commands");
						help.Add("[ & ]: cycle console colors");
						help.Add("======== commands ========");
						help.Add(" (testing environment) ");
						help.Add(" (press left/right arrow to select spawned character) ");
						help.Add("spawn [character]");
						help.Add("list [typeofobject]");
						help.Add("list [typeofobject] [startswithstring]");
						help.Add("talk (initiates conversation with selected character)");
						help.Add("set character [field]=[value]");
						help.Add("set character talkative [conversation]");
						help.Add("set mission [variable]=[value]");
						help.Add("mission activate [mission]");
						help.Add("mission complete [mission]");
						help.Add("mission activate objective [mission] [objective]");
						help.Add("mission complete objective [mission] [objective]");
						help.Add("mission fail [mission]");
						help.Add("mission fail objective [mission] [objective]");
						help.Add("mission variable [missionname] [variablename] [variablevalue]");
						help.Add("reset (resets current character)");
						help.Add("reset [datatype] [dataname]");
						help.Add("reset [datatype] all");
						help.Add("import all");
						help.Add("import Book all");
						help.Add("import Mission all");
						help.Add("import Conversation all");
						help.Add("import Speech all");
						help.Add("import Book [book]");
						help.Add("import Mission [mission]");
						help.Add("import Conversation [conversation]");
						help.Add("import Speech [speech]");
						help.Add("learn skill [skill]");
						help.Add("learn blueprint [blueprint]");
						help.Add("add [packname] [prefabname]");
						help.Add("add [packname] [prefabname] [number]");
						help.Add("add questitem [itemname]");
						help.Add("book read [book]");
						help.Add("grow prefabs");
						help.Add("build structure [structure]");
						help.Add("build prefabstructure [structure]");
						help.Add("build prebuiltstructure [structure]");
						help.Add("build prefabs");
						help.Add("help (toggles this list)");

						helpLabel = help.JoinToString("\n");
				}

				protected double nextConvoSelectTime = 0f;
				protected bool showWorldItems = false;

				public void Update()
				{
						if (Input.GetKeyDown(KeyCode.F3)) {
								showWorldItems = !showWorldItems;
						}

						if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F1)) {
								show = !show;
								UserActionManager.Suspended = show;
								InterfaceActionManager.Suspended = show;
						}

						if (show) {
								if (!GameManager.Get.TestingEnvironment) {
										List <string> posLabelList = new List<string>();
										Vector3 position = GameManager.Get.GameCamera.transform.position;
										posLabelList.Add("-----------VERSION: " + GameManager.Version + " -------------");
										posLabelList.Add("-----------BUILD: " + BuildID + " -------------");
										if (GameWorld.Get.WorldLoaded) {
												Color currentColor = Player.Local.Surroundings.TerrainType;
												Color32 currentData = GameWorld.Get.CurrentRegionData;
												posLabelList.Add("X: " + position.x.ToString("0.###"));
												posLabelList.Add("Y: " + position.y.ToString("0.###"));
												posLabelList.Add("Z: " + position.z.ToString("0.###"));
												posLabelList.Add(WorldClock.Time.ToString("0.#") + " - Time of day: " + WorldClock.DayCycleCurrentNormalized.ToString("0.##"));
												posLabelList.Add("Paused? " + GameManager.Is(FGameState.GamePaused).ToString());
												posLabelList.Add("Day: " + WorldClock.DaysSinceBeginningOfTime.ToString());
												posLabelList.Add("Timescale: " + WorldClock.Get.TimeScale.ToString("0.##"));
												posLabelList.Add("IsDaylight: " + WorldClock.IsDay.ToString());
												posLabelList.Add("Raw Temp: " + Player.Local.Status.LatestTemperatureRaw.ToString());
												posLabelList.Add("Adjusted Temp: " + Player.Local.Status.LatestTemperatureAdjusted.ToString());
												posLabelList.Add("Exposure Temp: " + Player.Local.Status.LatestTemperatureExposure.ToString());
												posLabelList.Add("Primary Chunk (Above Ground): " + GameWorld.Get.PrimaryChunk.Name);
												posLabelList.Add("Coastal: [-]" + currentColor.r.ToString("0.###") + " (Player Coastal: " + Player.Local.Surroundings.TerrainType.r.ToString("0.###"));
												posLabelList.Add("Forest: [-]" + currentColor.g.ToString("0.###"));
												posLabelList.Add("Civilized: [-]" + currentColor.b.ToString("0.###"));
												posLabelList.Add("OpenField: [-]" + currentColor.a.ToString("0.###"));
												posLabelList.Add("Player audible range: " + Player.Local.AudibleRange.ToString("0.###"));
												posLabelList.Add("Player awareness distance multiplier: " + Player.Local.AwarenessDistanceMultiplier.ToString("0.###"));
												posLabelList.Add("Player field of view multiplier: " + Player.Local.FieldOfViewMultiplier.ToString("0.###"));
												posLabelList.Add("Current region: " + currentData.b.ToString());
												posLabelList.Add("Current biome: " + currentData.r.ToString() + "(" + GameWorld.Get.CurrentBiome.Name + ")");
												posLabelList.Add("Current sway amp: " + Player.Local.Tool.ToolSwayAmplitude.ToString());
												posLabelList.Add("Current bob amp: " + Player.Local.Tool.ToolBobAmplitude.ToString());
												posLabelList.Add("Current weapon shake speed: " + Player.Local.FPSWeapon.m_Shake.ToString());
												posLabelList.Add("Camera shake speed: " + Player.Local.FPSCamera.m_Shake.ToString());
												posLabelList.Add("Platform below player? " + Player.Local.Surroundings.IsOnMovingPlatform.ToString());
												if (Player.Local.Surroundings.IsOnMovingPlatform) {
														posLabelList.Add("Platform velocity: " + Player.Local.Surroundings.MovingPlatformUnderPlayer.VelocityLastFrame.ToString());
												}
												posLabelList.Add("Light sources / exposure: " + Player.Local.Surroundings.LightSources.Count.ToString() + " / " + Player.Local.Surroundings.LightExposure.ToString());
												posLabelList.Add("Fires / heat: " + Player.Local.Surroundings.FireSources.Count.ToString() + " / " + Player.Local.Surroundings.HeatExposure.ToString());
										}
										posLabelList.Add("-----------MULTIPLAYER-------------");
										posLabelList.Add("IP: " + TNet.Tools.localAddress.ToString());
										posLabelList.Add("Host state: " + GameManager.HostState.ToString());
										posLabelList.Add("Client state: " + GameManager.ClientState.ToString());
										posLabelList.Add("Is connected? " + NetworkManager.Instance.IsConnected.ToString());
										posLabelList.Add("Is trying to connect? " + TNManager.isTryingToConnect.ToString());
										posLabelList.Add("Is host? " + NetworkManager.Instance.IsHost.ToString());
										posLabelList.Add("-----------ACTIVE STATES-------------");
										foreach (string state in Player.Local.Status.ActiveStateList) {
												posLabelList.Add(state);
										}
										if (GameManager.Is(FGameState.InGame)) {
												posLabelList.Add("-----------QUEST ITEMS-------------");
												foreach (string questItemName in Player.Local.Inventory.State.QuestItemsAcquired) {
														posLabelList.Add(questItemName);
												}
										}
										posLabel = posLabelList.JoinToString("\n");
								}

								if (Input.GetKeyDown(KeyCode.LeftArrow)) {
										Characters.Get.SelectedCharacter--;
								}
								if (Input.GetKeyDown(KeyCode.RightArrow)) {
										Characters.Get.SelectedCharacter++;
								}
								if (Input.GetKeyDown(KeyCode.PageUp) || (Input.GetKey(KeyCode.PageUp) && WorldClock.RealTime > nextConvoSelectTime)) {
										Characters.Get.SetSelectedCharacterConvo(true);
										nextConvoSelectTime = WorldClock.RealTime + 0.5f;
								}
								if (Input.GetKeyDown(KeyCode.PageDown) || (Input.GetKey(KeyCode.PageDown) && WorldClock.RealTime > nextConvoSelectTime)) {
										Characters.Get.SetSelectedCharacterConvo(false);
										nextConvoSelectTime = WorldClock.RealTime + 0.5f;
								}
								if (Input.GetKeyDown(KeyCode.LeftBracket)) {
										consoleColorIndex++;
								}
								if (Input.GetKeyDown(KeyCode.RightBracket)) {
										consoleColorIndex--;
								}
								if (Input.GetKeyDown(KeyCode.Home)) {
										showConvoVariables = !showConvoVariables;
								}

								if (Input.GetKeyDown(KeyCode.UpArrow)) {

										bool foundLine = false;
										int numTries = 0;
										while (!foundLine) {
												numTries++;
												if (numTries > 100) {
														ConsoleText = string.Empty;
														break;
												}


												if (lastIndex > 0) {
														lastIndex--;
												} else {
														lastIndex = Log.LastIndex();
												}
												if (!Log[lastIndex].StartsWith("#")) {
														ConsoleText = Log[lastIndex];
														foundLine = true;
												}
										}
								}

								if (Input.GetKeyDown(KeyCode.DownArrow)) {
										bool foundLine = false;
										int numTries = 0;
										while (!foundLine) {
												numTries++;
												if (numTries > 100) {
														ConsoleText = string.Empty;
														break;
												}


												if (lastIndex < Log.LastIndex()) {
														lastIndex++;
												} else {
														lastIndex = 0;
												}
												if (!Log[lastIndex].StartsWith("#")) {
														ConsoleText = Log[lastIndex];
														foundLine = true;
												}
										}
								}
			
								//add console keys
								foreach (char c in Input.inputString) {
										if (c == '\b') {
												if (ConsoleText.Length > 0) {
														ConsoleText = ConsoleText.Substring(0, ConsoleText.Length - 1);
												}
										} else if ((c != '\n') && (c != '`') && (c != '\t') && (c != '[' && c != ']') && (c != '+')) {
												ConsoleText += c;
										}
								}

								if (Input.GetKeyDown(KeyCode.Return)) {
										ConsoleText = ConsoleText.Trim();
										if (!string.IsNullOrEmpty(ConsoleText)) {
												if (ConsoleText.ToLower().Trim() == "help") {
														showHelp = !showHelp;
												} else {
														Log.Add(ConsoleText);
														lastIndex = Log.LastIndex();

														try {
																ConsoleCommand(ConsoleText);
														} catch (Exception e) {
																Log.Add("#" + e.ToString());
																Log.Add("#(This usually means you're not following the format)");
																Debug.Log(e.ToString());
														}
												}
										}
										ConsoleText = string.Empty;
								}
						}
				}

				protected Rect consoleRect = new Rect();
				protected Rect debugRect = new Rect();
				protected Rect helpRect = new Rect(10, 25, 500, 1080);
				protected Rect versionRect = new Rect();
				protected Rect testingRect = new Rect(10, 25, 500, 1080);

				public void OnGUI()
				{
						if (!show) {
								return;
						}

						if (showWorldItems && WorldItems.Get != null) {
								GUILayout.Button("Num active worlditems: " + WorldItems.Get.ActiveWorldItems.Count.ToString());
								GUILayout.Button("Num visible worlditems: " + WorldItems.Get.VisibleWorldItems.Count.ToString());
								GUILayout.Button("Num invisible worlditems: " + WorldItems.Get.InvisibleWorldItems.Count.ToString());
								GUILayout.Button("Num locked worlditems: " + WorldItems.Get.LockedWorldItems.Count.ToString());
								GUILayout.Button("Total worlditems: " + (WorldItems.Get.ActiveWorldItems.Count + WorldItems.Get.VisibleWorldItems.Count + WorldItems.Get.InvisibleWorldItems.Count + WorldItems.Get.LockedWorldItems.Count).ToString());
								GUILayout.Label("Groups:");
								GUILayout.Button("Total loaded groups: " + WIGroups.Get.Groups.Count.ToString());
								GUILayout.Button("Groups waiting to load: " + WIGroups.GroupsToLoad.Count.ToString());
								GUILayout.Button("Groups loading: " + WIGroups.GroupsLoading.Count.ToString());
						}

						/*
						if (GameManager.Is (FGameState.InGame)) {
							Motile newMotile = null;
							if (lastMotile == null) {
								if (Player.Local.Surroundings.IsWorldItemInRange) {
									Player.Local.Surroundings.WorldItemFocus.worlditem.Is <Motile> (out lastMotile);
								}
							} else if (Player.Local.Surroundings.IsWorldItemInRange && Player.Local.Surroundings.WorldItemFocus.worlditem.Is<Motile> (out newMotile)) {
								if (newMotile != lastMotile) {
									lastMotile = newMotile;
								}
							}

							if (lastMotile != null) {
								lastListener = lastMotile.worlditem.Get <Listener> ();
								lastLooker = lastMotile.worlditem.Get <Looker> ();
								lastCreature = lastMotile.worlditem.Get <Creature> ();
								lastPs = lastMotile.worlditem.Get <Photosensitive> ();
								lastHostile = lastMotile.worlditem.Get <Hostile> ();

								UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
								GUILayout.Button (lastMotile.State.BaseAction.Type.ToString () + " : " + lastMotile.State.BaseAction.State.ToString ());

								if (lastLooker != null) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
									GUILayout.Label ("\nLOOKER");
									if (lastLooker.LastSeenItemOfInterest == null) {
										UnityEngine.GUI.color = Color.Lerp (Color.white, Color.red, 0.25f);
										GUILayout.Label ("Nothing last seen");
									} else {
										UnityEngine.GUI.color = Color.green;
										GUILayout.Label ("Last thing seen: " + lastLooker.LastSeenItemOfInterest.IOIType.ToString ());
									}
								}

								if (lastListener != null) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
									GUILayout.Label ("\nLISTENER");
									if (lastListener.LastHeardItemOfInterest == null) {
										UnityEngine.GUI.color = Color.Lerp (Color.white, Color.red, 0.25f);
										GUILayout.Label ("Nothing last heard");
									} else {
										UnityEngine.GUI.color = Color.green;
										GUILayout.Label ("Last thing heard: " + lastListener.LastHeardItemOfInterest.IOIType.ToString ());
									}
								}

								if (lastCreature != null) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
									GUILayout.Label ("\nCREATURE");
									if (!lastCreature.CurrentThought.HasItemOfInterest) {
										UnityEngine.GUI.color = Color.Lerp (Color.white, Color.red, 0.25f);
										GUILayout.Label ("Thinking about nothing");
									} else {
										UnityEngine.GUI.color = Color.green;
										GUILayout.Label ("Thinking about " + lastCreature.CurrentThought.CurrentItemOfInterest.IOIType.ToString ());
									}
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
									GUILayout.Label (lastCreature.CurrentThought.ToString ());
								}

								if (lastPs != null) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
									GUILayout.Label ("\nLIGHT SOURCES: ");
									GUILayout.Label ("Has nearby lights? " + lastPs.HasNearbyLights.ToString ());
									GUILayout.Label ("Has nearby fires? " + lastPs.HasNearbyFires.ToString ());
									GUILayout.Label ("Light exposure: " + lastPs.LightExposure.ToString () + " - Heat exposure: " + lastPs.HeatExposure.ToString ());
								}

								if (lastHostile != null) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.red, 0.25f);
									GUILayout.Label ("\nHOSTILE MODE: " + lastHostile.Mode.ToString( ));
									string primaryTarget = "(None)";
									if (lastHostile.HasPrimaryTarget) {
										primaryTarget = lastHostile.PrimaryTarget.gameObject.name;
									}
									GUILayout.Label ("\nHOSTILE TARGET: " + primaryTarget);
								}

								UnityEngine.GUI.color = Color.Lerp (Color.white, Color.green, 0.25f);
								GUILayout.Label ("MOTILE");
								GUILayout.Button ("Motile : " + lastMotile.worlditem.name + " : Num Actions: " + lastMotile.State.Actions.Count.ToString ());
								for (int i = 0; i < lastMotile.State.Actions.Count; i++) {
									UnityEngine.GUI.color = Color.Lerp (Color.white, Color.red, 0.25f);
									if (lastMotile.State.Actions [i].State == MotileActionState.Started) {
										UnityEngine.GUI.color = Color.green;
									}
									GUILayout.Button (lastMotile.State.Actions [i].Type.ToString () + " : " + lastMotile.State.Actions [i].State.ToString ());
								}
							}
						}

						if (GameManager.Is (FGameState.InGame)) {
							if (Player.Local.Surroundings.IsSomethingInRange) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Something in range: " + Player.Local.Surroundings.ClosestObjectInRange.gameObject.name);
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(Nothing in range)");
							}

							if (Player.Local.Surroundings.IsWorldItemInRange) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("World item in range: " + Player.Local.Surroundings.WorldItemFocus.worlditem.name);
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(No worlditem in range)");
							}

							if (Player.Local.Surroundings.IsTerrainInRange) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Terrain in range: " + Player.Local.Surroundings.TerrainFocus.gameObject.name);
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(No terrain in range)");
							}

							if (Player.Local.Surroundings.IsReceptacleInPlayerFocus) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Receptacle in focus: " + Player.Local.Surroundings.ReceptacleInPlayerFocus.worlditem.name);
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(No recepticle in focus)");
							}

							if (Player.Local.ItemPlacement.PlacementPossible) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Placement possible");
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(Placement NOT possible)");
							}

							if (Player.Local.ItemPlacement.PlacementPossible && Player.Local.ItemPlacement.PlacementPermitted) {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Placement permitted");
							} else {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button ("(Placement NOT permitted)");
							}
						}

						if (Structures.Get != null) {
							if (Structures.Get.HallA == null) {
								UnityEngine.GUI.color = Color.gray;
								GUILayout.Button ("(No hall A yet)");
							} else {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Hall A World Item Active State: " + Structures.Get.HallA.worlditem.ActiveState.ToString () + "\nStructure state: " + Structures.Get.HallA.LoadState.ToString ());
								if (Structures.Get.HallA.ExteriorMeshes.Count > 0 && Structures.Get.HallA.ExteriorMeshes [0] == null) {
									UnityEngine.GUI.color = Color.red;
									GUILayout.Button ("Meshes DESTROYED");
								} else {
									UnityEngine.GUI.color = Color.green;
									GUILayout.Button ("Meshes present");
								}
								Location location = null;
								if (Structures.Get.HallA.worlditem.Is <Location> (out location) && location.LocationGroup != null) {
									GUILayout.Button ("Location group state: " + location.LocationGroup.LoadState.ToString ());
								}
							}

							if (Structures.Get.LectureHall == null) {
								UnityEngine.GUI.color = Color.gray;
								GUILayout.Button ("(No Lecture hall yet)");
							} else {
								UnityEngine.GUI.color = Color.green;
								GUILayout.Button ("Lecture hall Item Active State: " + Structures.Get.LectureHall.worlditem.ActiveState.ToString () + "\nStructure state: " + Structures.Get.LectureHall.LoadState.ToString ());
								if (Structures.Get.LectureHall.ExteriorMeshes.Count > 0 && Structures.Get.LectureHall.ExteriorMeshes [0] == null) {
									UnityEngine.GUI.color = Color.red;
									GUILayout.Button ("Meshes DESTROYED");
								} else {
									UnityEngine.GUI.color = Color.green;
									GUILayout.Button ("Meshes present");
								}
								Location location = null;
								if (Structures.Get.LectureHall.worlditem.Is <Location> (out location) && location.LocationGroup != null) {
									GUILayout.Button ("Location group state: " + location.LocationGroup.LoadState.ToString ());
								}
							}
						}

									versionRect.x = Screen.width - 185f;
						versionRect.y = 5f;
						versionRect.width = 185f;
						versionRect.height = 25f;
						UnityEngine.GUI.color = Colors.Alpha (Color.white, 0.5f);
						UnityEngine.GUI.Label (versionRect, "Frontiers Beta v." + GameManager.Version);
						*/

						if (show) {
								GUIStyle style;

								string showConsolteText = "_";
								if (!string.IsNullOrEmpty(ConsoleText)) {
										showConsolteText = ConsoleText;
								}
								style = new GUIStyle();
								style.fontSize = 14;
								if (consoleColorIndex < 0) {
										consoleColorIndex = ConsoleColors.LastIndex();
								} else if (consoleColorIndex > ConsoleColors.LastIndex()) {
										consoleColorIndex = 0;
								}
								UnityEngine.GUI.color = ConsoleColors[consoleColorIndex];
								consoleRect.x = Screen.width - 500f;
								consoleRect.y = 10f;
								consoleRect.width = 800f;
								consoleRect.height = 25f;
								UnityEngine.GUI.Label(consoleRect, showConsolteText);
								string prevConsoleText = string.Empty;
								for (int i = Log.LastIndex(); i >= 0; i--) {
										prevConsoleText += Log[i] + "\n";
								}

								consoleRect.x = Screen.width - 500f;
								consoleRect.y = 25f;
								consoleRect.width = 800f;
								consoleRect.height = 500f;
								UnityEngine.GUI.Label(consoleRect, prevConsoleText);

								if (showHelp) {
										UnityEngine.GUI.Label(helpRect, helpLabel);
								} else {
										if (!GameManager.Get.TestingEnvironment) {
												UnityEngine.GUI.Label(testingRect, posLabel);
										} else {
												UnityEngine.GUI.Label(helpRect, "type 'help' for help");
										}
								}
						}
				}

				public static void ConsoleCommand(string command)
				{		//hahaha this function sucks so much
						//parse command
						string[] splitCommand = command.Split(new string [] { " " }, StringSplitOptions.RemoveEmptyEntries);
						//wrap this in a try because it's bound to go wrong
						//spawn or set
						switch (splitCommand[0].Trim().ToLower()) {
								case "clearfocus":
										GUIManager.Get.ClearFocus();
										break;

								case "unlock":
										Locked locked = null;
										if (Player.Local.Surroundings.IsWorldItemInPlayerFocus && Player.Local.Surroundings.WorldItemFocus.worlditem.Is <Locked>(out locked)) {
												locked.State.HasBeenUnlocked = true;
										}
										break;

								case "justlooking":
										GameManager.Get.JustLookingMode = !GameManager.Get.JustLookingMode;
										DebugConsole.Get.Log.Add("#Just looking mode set to " + GameManager.Get.JustLookingMode.ToString());
										break;

								case "wordcount":
										GameObject counterObject = new GameObject("DataImporter");
										DataImporter counter = counterObject.GetOrAdd <DataImporter>();
										counter.WordCounter();
										GameObject.Destroy(counterObject);
										break;

								case "die":
										Player.Local.Die("Consoleitis");
										break;

								case "time":
										int hoursToSkip = Int32.Parse(splitCommand[1].Trim());
										float timeToSkip = (float)(hoursToSkip * WorldClock.gHourCycleSeconds);
										//WorldClock.AddTime (timeToSkip);
										break;

								case "list":
										string typeOfObject = splitCommand[1].Trim();
										string startsWith = string.Empty;
										if (splitCommand.Length > 2) {
												startsWith = splitCommand[2].Trim();
										}
										List <string> objects = Mods.Get.Available(typeOfObject);
										DebugConsole.Get.Log.Add("#------");
										for (int i = objects.LastIndex(); i >= 0; i--) {
												if (string.IsNullOrEmpty(startsWith)) {
														DebugConsole.Get.Log.Add("#" + objects[i]);
												} else if (objects[i].ToLower().StartsWith(startsWith.ToLower())) {
														DebugConsole.Get.Log.Add("#" + objects[i]);
												}
										}
										DebugConsole.Get.Log.Add("#------");
										if (!string.IsNullOrEmpty(startsWith)) {
												DebugConsole.Get.Log.Add("#Listing all " + typeOfObject + " that start with " + startsWith + " (" + objects.Count.ToString() + " found)");
										} else {
												DebugConsole.Get.Log.Add("#Listing all " + typeOfObject + "(" + objects.Count.ToString() + " found)");
										}
										break;

								case "talk":
										Characters.Get.InitiateConversation();
										break;

								case "set":
										switch (splitCommand[1]) {
												case "character":
														switch (splitCommand[2]) {
																case "talkative":
																		string conversationName = splitCommand[3].Trim().ToLower();
																		conversationName = DataImporter.GetNameFromDialogName(conversationName);
																		Characters.Get.SetTalkativeOnSelectedCharacter(conversationName);
																		break;

																default:
																		//splitCharacterSetCommand
																		//0		1			2				3
																		//set 	character 	characterName 	wiScript.Variable=Value
																		string setCharacterName = splitCommand[2];
																		string[] splitCharacterSetCommand = splitCommand[3].Split(new string [] {
																				"=",
																				"."
																		}, StringSplitOptions.RemoveEmptyEntries);
																		string scriptName = splitCharacterSetCommand[0];
																		string scriptStateVar = splitCharacterSetCommand[1];
																		string scriptStateVal = splitCharacterSetCommand[2];
																		Character spawnedCharacter = null;
																		if (Characters.Get.SpawnedCharacter(setCharacterName, out spawnedCharacter)) {
																				WIScript script = null;
																				if (spawnedCharacter.worlditem.Is(scriptName, out script)) {
																						//see if the script has a script state object
																						System.Reflection.FieldInfo scriptStateField = script.GetType().GetField("State");
																						if (scriptStateField != null) {	//get the state field for the state object
																								System.Object stateObject = scriptStateField.GetValue(script);
																								//get the variable field for the state object
																								System.Reflection.FieldInfo stateField = stateObject.GetType().GetField(scriptStateVar);
																								if (stateField != null) {
																										stateField.SetValue(stateObject, scriptStateVal);
																										//Debug.Log ("Setting state field " + stateField.GetType ().Name + " var " + scriptStateVar + " to " + scriptStateVal);
																								} else {
																										DebugConsole.Get.Log.Add("Character " + setCharacterName + " script " + scriptName + " doesn't have state field " + scriptStateVar);
																								}
																						} else {
																								DebugConsole.Get.Log.Add("Character " + setCharacterName + " script " + scriptName + " doesn't have state field");
																						}
																				} else {
																						DebugConsole.Get.Log.Add("Character " + setCharacterName + " doesn't have script " + scriptName);
																				}
																		} else {
																				DebugConsole.Get.Log.Add("Couldn't find character " + setCharacterName);
																		}
																		break;
														}
														break;

												case "mission":
														break;

												case "questitem":
														if (Player.Local.Surroundings.IsWorldItemInRange) {
																WorldItem wi = Player.Local.Surroundings.ClosestObjectInRange.worlditem;
																QuestItem qi = wi.GetOrAdd <QuestItem>();
																qi.State.QuestName = splitCommand[2].Trim();
																wi.Props.Name.QuestName = qi.State.QuestName;
														} else if (Player.Local.Tool.HasWorldItem) {
																WorldItem wi = Player.Local.Tool.worlditem;
																QuestItem qi = wi.GetOrAdd <QuestItem>();
																qi.State.QuestName = splitCommand[2].Trim();
																wi.Props.Name.QuestName = qi.State.QuestName;
														}
														break;
										}
										break;

								case "mission":
										string missionCommand = splitCommand[1].Trim();
										string missionItem = splitCommand[2].Trim();
										string missionName = string.Empty;
										string objectiveName = string.Empty;
										string variableName = string.Empty;
										switch (missionCommand) {
												case "variable":
														missionName = missionItem;
														variableName = splitCommand[3].Trim();
														int variableValue = Int32.Parse(splitCommand[4].Trim());
														Missions.Get.SetVariableValue(missionName, variableName, variableValue);
														break;

												case "activate":
														switch (missionItem) {
																case "objective":
																		missionName = splitCommand[3].Trim();
																		objectiveName = splitCommand[4].Trim();
																		Missions.Get.ActivateObjective(missionName, objectiveName, MissionOriginType.None, string.Empty);
																		break;

																default:
																		Missions.Get.ActivateMission(missionItem, MissionOriginType.None, string.Empty);
																		break;

														}
														break;

												case "complete":
														switch (missionItem) {
																case "objective":
																		missionName = splitCommand[3].Trim();
																		objectiveName = splitCommand[4].Trim();
																		Missions.Get.ForceCompleteObjective(missionName, objectiveName);
																		break;

																default:
																		Missions.Get.ForceCompleteMission(missionItem);
																		break;
														}
														break;

												case "fail":
														switch (missionItem) {
																case "objective":
																		missionName = splitCommand[3].Trim();
																		objectiveName = splitCommand[4].Trim();
																		Missions.Get.ForceFailObjective(missionName, objectiveName);
																		break;

																default:
																		Missions.Get.ForceFailMission(missionItem);
																		break;
														}
														break;

												default:
														DebugConsole.Get.Log.Add("Not activating or completing mission");
														break;
										}
										break;

								case "master":
										string skillName = splitCommand[1].Trim();
										DebugConsole.Get.Log.Add("#Mastering " + skillName);
										Skills.Get.MasterSkill(skillName);
										break;

								case "learn":
										string itemType = splitCommand[1].Trim();
										DebugConsole.Get.Log.Add("#Learning " + itemType);
										switch (itemType) {
												case "skill":
												default:
														string learnSkillName = splitCommand[2].Trim();
														Skill skillToLearn = null;
														if (Skills.Get.SkillByName(learnSkillName, out skillToLearn)) {
																skillToLearn.LearnSkill();
														}
														break;

												case "blueprint":
														string blueprintName = splitCommand[2].Trim();
														DebugConsole.Get.Log.Add("#Learning blueprint " + blueprintName);
														if (blueprintName == "all") {
																List <string> allBlueprints = Mods.Get.Available("Blueprint");
																foreach (string allBlueprint in allBlueprints) {
																		Blueprints.Get.Reveal(allBlueprint, BlueprintRevealMethod.None, string.Empty);
																}
														} else {
																Blueprints.Get.Reveal(blueprintName, BlueprintRevealMethod.None, string.Empty);
														}
														break;
										}
										break;

								case "devsprint":
										Player.Local.FPSController.DevSprint = !Player.Local.FPSController.DevSprint;
										break;

								case "add":
										WIStackError error = WIStackError.None;
										string packName = splitCommand[1].Trim();
										if (packName.Contains("$")) {
												//we're adding money
												packName = packName.Replace("$", "");
												int numBase = Int32.Parse(packName);
												WICurrencyType currencyType = WICurrencyType.A_Bronze;
												if (splitCommand.Length > 2) {
														//we're adding it as something
														string asType = splitCommand[2].Trim().ToLower();
														switch (asType) {
																case "grain":
																case "bronze":
																default:
																		currencyType = WICurrencyType.A_Bronze;
																		break;

																case "quarter":
																case "silver":
																		currencyType = WICurrencyType.B_Silver;
																		break;

																case "dram":
																case "gold":
																		currencyType = WICurrencyType.C_Gold;
																		break;

																case "mark":
																case "lumenite":
																		currencyType = WICurrencyType.D_Luminite;
																		break;

																case "warlock":
																		currencyType = WICurrencyType.E_Warlock;
																		break;
														}
												}
												Player.Local.Inventory.InventoryBank.Add(numBase, currencyType);
										} else if (packName == "ingredients") {
												string bpName = splitCommand[2].Trim();
												WIBlueprint bp = null;
												if (Blueprints.Get.Blueprint(bpName, out bp, false)) {
														GenericWorldItem bpItem = null;
														int numToAdd = 1;
														for (int i = 0; i < bp.Row1.Count; i++) {
																bpItem = bp.Row1[i];
																numToAdd = bpItem.InstanceWeight;
																for (int j = 0; j < numToAdd; j++) {
																		Player.Local.Inventory.AddItem(bpItem.ToStackItem());
																}
														}
														for (int i = 0; i < bp.Row2.Count; i++) {
																bpItem = bp.Row2[i];
																numToAdd = bpItem.InstanceWeight;
																for (int j = 0; j < numToAdd; j++) {
																		Player.Local.Inventory.AddItem(bpItem.ToStackItem());
																}

														}
														for (int i = 0; i < bp.Row3.Count; i++) {
																bpItem = bp.Row3[i];
																numToAdd = bpItem.InstanceWeight;
																for (int j = 0; j < numToAdd; j++) {
																		Player.Local.Inventory.AddItem(bpItem.ToStackItem());
																}
														}
												}
										} else if (packName == "questitem") {
												Player.Local.Inventory.State.QuestItemsAcquired.SafeAdd(splitCommand[2].Trim());
												Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemQuestItemAddToInventory, WorldClock.Time);
										} else {
												if (splitCommand.Length < 2) {
														//just add a random object from the pack
														for (int i = 0; i < WorldItems.Get.WorldItemPacks.Count; i++) {
																WorldItemPack pack = WorldItems.Get.WorldItemPacks[i];
																if (pack.Name == packName) {
																		int randomIndex = UnityEngine.Random.Range(0, pack.Prefabs.Count);
																		StackItem item = pack.Prefabs[randomIndex].GetComponent <WorldItem>().GetStackItem(WIMode.None);
																		Player.Local.Inventory.AddItems(item, ref error);
																}
																break;
														}
												} else {
														string prefabName = splitCommand[2].Trim().Replace("_", " ");
														string stateName = "Default";
														int numToAdd = 1;
														//see if we're setting the state too
														if (splitCommand.Length == 4) {
																if (!Int32.TryParse(splitCommand[3], out numToAdd)) {
																		numToAdd = 1;
																		stateName = splitCommand[3];
																}
														}
														if (splitCommand.Length == 5) {
																stateName = splitCommand[3];
																numToAdd = Mathf.Max(1, Int32.Parse(splitCommand[4]));
														}
														StackItem stackItem = null;
														int numAdded = 0;
														for (int i = 0; i < numToAdd; i++) {
																if (WorldItems.Get.StackItemFromPack(packName, prefabName, out stackItem)) {
																		numAdded++;
																		stackItem.SaveState.LastState = stateName;
																		Player.Local.Inventory.AddItems(stackItem, ref error);
																}
														}
														DebugConsole.Get.Log.Add("#Added " + numAdded.ToString() + " of " + packName + ", " + prefabName + " to inventory");
												}
										}
										break;

								case "status":
										switch (splitCommand[1]) {
												case "restore":
														Player.Local.Status.RestoreStatus(0.25f, splitCommand[2]);
														break;

												case "reduce":
														Player.Local.Status.ReduceStatus(0.25f, splitCommand[2]);
														break;

												default:
														break;
										}
										break;

								case "condition":
										string conditionName = splitCommand[2];
										switch (splitCommand[1]) {
												case "cure":
														foreach (Condition condition in Player.Local.Status.State.ActiveConditions) {
																if (conditionName == "all") {
																		condition.Cancel();
																} else if (conditionName.ToLower() == condition.Name.ToLower()) {
																		condition.Cancel();
																}
														}
														break;

												case "add":
														Player.Local.Status.AddCondition(conditionName);
														break;

												default:
														break;
										}
										break;

								case "respawn":
										Player.Local.Spawn();
										break;

								case "resetstatus":
										Player.Local.Status.ResetStatusKeepers();
										break;

								case "spawn":
										Vector3 characterSpawnPoint = GameManager.Get.GameCamera.transform.position - (Vector3.up * 1.25f);
										characterSpawnPoint += GameManager.Get.GameCamera.transform.forward * 2.0f;
										GameObject gameObject = new GameObject("SpawnActionNode");
										gameObject.transform.position = characterSpawnPoint;
										gameObject.transform.LookAt(GameManager.Get.GameCamera.transform.position - (Vector3.up * 1.25f));
										ActionNode node = gameObject.AddComponent <ActionNode>();
										Character character = null;

										if (splitCommand[1] == "random") {
												string randomCharacterName = splitCommand[2];
												if (Characters.SpawnRandomCharacter(node, randomCharacterName, GameWorld.Get.Settings.DefaultResidentFlags, WIGroups.Get.World, out character)) {
														DebugConsole.Get.Log.Add("#Spawned character " + randomCharacterName);
												} else {
														DebugConsole.Get.Log.Add("#Couldn't spawn " + randomCharacterName);
												}
										} else {
												string spawnCharacterName = splitCommand[1];
												if (Characters.GetOrSpawnCharacter(node, spawnCharacterName, WIGroups.Get.World, out character)) {
														DebugConsole.Get.Log.Add("#Spawned character " + spawnCharacterName);
												} else {
														DebugConsole.Get.Log.Add("#Couldn't spawn " + spawnCharacterName);
												}
										}
										break;

								case "find":
										List <string> chunks = Mods.Get.Available("Chunk");
										List <ChunkTerrainData> terrainData = new List<ChunkTerrainData>();
										List <ChunkTerrainData> otherTerrainData = new List<ChunkTerrainData>();
										foreach (string chunkName in chunks) {
												ChunkState cs = null;
												if (Mods.Get.Runtime.LoadMod <ChunkState>(ref cs, "Chunk", chunkName)) {
														ChunkTerrainData td = null;
														if (Mods.Get.Runtime.LoadMod <ChunkTerrainData>(ref td, WorldChunk.ChunkDataDirectory(cs.Name), "Terrain")) {
																if (cs.ID == 100 || cs.ID == 2 || cs.ID == 8) {
																		terrainData.Add(td);
																} else {
																		otherTerrainData.Add(td);
																}
														}
												}
										}

										Dictionary <string,int> sharedAssets = new Dictionary<string, int>();
										foreach (ChunkTerrainData ctd in terrainData) {
												for (int i = 0; i < ctd.TreeTemplates.Count; i++) {
														if (!sharedAssets.ContainsKey(ctd.TreeTemplates[i].AssetName)) {
																sharedAssets.Add(ctd.TreeTemplates[i].AssetName, 1);
														} else {
																sharedAssets[ctd.TreeTemplates[i].AssetName] = sharedAssets[ctd.TreeTemplates[i].AssetName] + 1;
														}
												}
												for (int i = 0; i < ctd.DetailTemplates.Count; i++) {
														if (!sharedAssets.ContainsKey(ctd.DetailTemplates[i].AssetName)) {
																sharedAssets.Add(ctd.DetailTemplates[i].AssetName, 1);
														} else {
																sharedAssets[ctd.DetailTemplates[i].AssetName] = sharedAssets[ctd.DetailTemplates[i].AssetName] + 1;
														}
												}
										}

										foreach (ChunkTerrainData otherctd in otherTerrainData) {
												for (int i = 0; i < otherctd.TreeTemplates.Count; i++) {
														if (sharedAssets.ContainsKey(otherctd.TreeTemplates[i].AssetName)) {
																sharedAssets.Remove(otherctd.TreeTemplates[i].AssetName);
														}
												}
												for (int i = 0; i < otherctd.DetailTemplates.Count; i++) {
														if (sharedAssets.ContainsKey(otherctd.DetailTemplates[i].AssetName)) {
																sharedAssets.Remove(otherctd.DetailTemplates[i].AssetName);
														}
												}
										}

										foreach (KeyValuePair <string,int> assetCount in sharedAssets) {
												if (assetCount.Value >= 3) {
														DebugConsole.Get.Log.Add("Asset shared in all 3 chunks: " + assetCount.Value);
												}
										}
										break;

								case "reset":
										if (splitCommand.Length < 2) {
												Characters.Get.Reset();
												Conversations.Get.Reset();
												DebugConsole.Get.Log.Add("#Resetting game state");
												Mods.Get.Runtime.ResetProfileData("all");
										} else {
												string dataType = splitCommand[1].Trim();
												string dataName = splitCommand[2].Trim();
												if (dataName == "all") {
														Mods.Get.Runtime.ResetProfileData(dataType);
														DebugConsole.Get.Log.Add("#Resetting all " + dataType + " to base state");
												} else {
														Mods.Get.Runtime.ResetProfileData(dataType, dataName);
														DebugConsole.Get.Log.Add("#Resetting " + dataType + " " + dataName + " to base state");
												}
												if (string.Equals(dataType.Trim().ToLower(), "character")) {
														//Debug.Log ("Resetting character...");
														Characters.Get.ResetCharacter(dataName);
												}
												if (string.Equals(dataType.Trim().ToLower(), "player")) {
														Player.Local.Inventory.State.QuestItemsAcquired.Clear();
														Player.Local.Inventory.State.QuestItemsRemoved.Clear();
														Player.Local.OnGameSave();
												}
												GameWorld.Get.State.DestroyedQuestItems.Clear();
										}
										break;

								case "goto":
										string startupPositionName = splitCommand[1];
										Get.StartCoroutine(SpawnManager.Get.SendPlayerToStartupPosition(startupPositionName, 0f));
										break;

								case "book":
										string bookAction = splitCommand[1];
										string bookName = splitCommand[2];
										if (bookAction == "read") {
												if (bookName == "all") {
														Books.ReadAll();
												} else {
														Books.ReadBook(bookName, null);
												}
										} else if (bookAction == "aquire") {
												Books.AquireBook(bookName);
										}
										break;

								case "grow":
										switch (splitCommand[1]) {
												case "prefabs":
												default:
														DebugConsole.Get.Log.Add("Instanitating all prefabs");
														Plants.InstantiateAllPrefabs();
														break;
										}
										break;

								case "build":
										DebugConsole.Get.Log.Add("Building something...");
										string templateName = string.Empty;
										switch (splitCommand[1]) {
												case "structure":
														templateName = splitCommand[2];
														break;

												case "prefabstructure":
														templateName = splitCommand[2];
														break;

												case "prebuiltstructure":
														templateName = splitCommand[2];
														break;

												case "prefabs":
												default:
														break;
										}
										break;

								case "import":
										Debug.Log("Importing data");
										string importDataType = splitCommand[1];
										string importDataName = splitCommand[2];
										List <string> dataTypes = null;
										List <string> dataNames = null;
										if (importDataType == "all") {
												dataTypes = new List <string>() {
														"Book",
														"Mission",
														"Conversation",
														"Character",
														"Speech"
												};
										} else {
												dataTypes = new List <string>() { importDataType };
										}
										if (importDataName == "all") {
												dataNames = new List <string>();
										} else {
												dataNames = new List<string>() { importDataName };
										}
										GameObject importerObject = new GameObject("DataImporter");
										DataImporter importer = importerObject.GetOrAdd <DataImporter>();
										Debug.Log("Created data importer");
										string errorMessage = string.Empty;
										if (!GameData.IO.InitializeSystemPaths(out errorMessage)) {	//aw shit son what did you do
												return;
										}
										if (!Manager.IsAwake <WorldItems>()) {
												Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
										}
										importer.ImportDataNames = dataNames;
										importer.ImportDataTypes = dataTypes;
										importer.ImportAssets();

										if (DebugConsole.Get != null) {
												DebugConsole.Get.Log.Add("#Data types " + dataTypes.JoinToString(", ") + " imported. You may need to reload to see changes");
										}
										break;


								default:
										DebugConsole.Get.Log.Add("#catch all for some reason - try help for commands");
										break;
						}
				}

		}
}
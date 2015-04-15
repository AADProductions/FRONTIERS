using UnityEngine;
using System.Collections;
using ExtensionMethods;
using Frontiers.Gameplay;
using Frontiers.World.Gameplay;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

//dev tool
public class MissionTestingUtility : MonoBehaviour
{
		public Mission CurrentMission;
		public bool ShowEditor = false;
		public int OffsetX;
		public int OffsetY;
		public int OffsetXMissions;
		public int OffsetYMissions;
		public int OffsetXMissionsEnd;
		public int OffsetXConvos;
		public int OffsetYConvos;
		public int OffsetXConvosEnd;
		public Character character;
		public Talkative talkative;
		public List <string> AvailableConvos;
		public List <string> AvailableDTS;

		public void Update()
		{
				OffsetX = (int)(Screen.width * 0.75f);
				OffsetY = (int)(Screen.height * 0.1f);

				OffsetXMissions = (int)(Screen.width * 0.25f);
				OffsetYMissions = (int)(Screen.height * 0.1f);
				OffsetXMissionsEnd = (int)(Screen.width * 0.75f);

				OffsetXConvos = (int)(Screen.width * 0.5f);
				OffsetYConvos = (int)(Screen.height * 0.5f);
				OffsetXConvosEnd = (int)(Screen.width * 0.75f);

				if (CurrentMission == null) {
						if (Missions.Get != null && GameManager.Is(FGameState.InGame)) {
								List <Mission> activeMissions = Missions.Get.ActiveMissions;
								if (activeMissions.Count > 0) {
										CurrentMission = activeMissions[0];
								}
						}
				}
		}

		public void OnGUI()
		{
				if (!ShowEditor) {
						return;
				}

				GUILayout.BeginArea(new Rect(OffsetXMissions, OffsetYMissions, OffsetXMissionsEnd, Screen.height - OffsetYMissions));
				GUI.color = Color.white;
				GUI.backgroundColor = Colors.Alpha(Color.black, 1.0f);
				List <string> AvailableMissions = Mods.Get.Available("Mission");
				for (int i = 0; i < AvailableMissions.Count; i++) {
						if (GUILayout.Button(AvailableMissions[i], GUILayout.MaxWidth(200))) {
								Missions.Get.ActivateMission(AvailableMissions[i], MissionOriginType.None, string.Empty);
						}
				}
				GUILayout.EndArea();

				GUILayout.BeginArea(new Rect(OffsetXConvos, OffsetYConvos, OffsetXConvosEnd, Screen.height - OffsetYConvos));
				GUI.color = Color.white;
				GUILayout.Label("Conversation switcher:");
				bool seesSomething = false;
				if (Player.Local.Focus.IsFocusingOnSomething) {
						if (Player.Local.Focus.LastFocusedObject.IOIType == Frontiers.ItemOfInterestType.WorldItem) {
								if (character == null) {
										if (Player.Local.Focus.LastFocusedObject.worlditem.Is <Character>(out character)) {
												talkative = character.worlditem.Get <Talkative>();
												string searchString = character.State.Name.FileName;
												if (string.IsNullOrEmpty(searchString)) {
														searchString = character.State.Name.GenericIdentifier;
												}
												if (string.IsNullOrEmpty(searchString)) {
														searchString = character.State.TemplateName;
												}
												AvailableConvos = Mods.Get.Available("Conversation", searchString);
												AvailableDTS = Mods.Get.Available("Speech", searchString);
										}
								}
								seesSomething = true;
						}
				}
				if (seesSomething && character != null) {
						GUILayout.BeginHorizontal();
						GUI.color = Color.white;
						if (!talkative.State.DefaultToDTS) {
								GUI.color = Color.green;
						}
						if (GUILayout.Button("Use Conversation", GUILayout.MaxWidth(300))) {
								talkative.State.DefaultToDTS = false;
						}
						GUI.color = Color.white;
						if (talkative.State.DefaultToDTS) {
								GUI.color = Color.green;
						}
						if (GUILayout.Button("Use DTS", GUILayout.MaxWidth(300))) {
								talkative.State.DefaultToDTS = true;
						}
						if (GUILayout.Button("Remove DTS Override", GUILayout.MaxWidth(300))) {
								Conversations.Get.RemoveDTSOverride(talkative.State.ConversationName, talkative.State.DTSSpeechName, character.worlditem.FileName);
						}
						GUILayout.EndHorizontal();
						GUI.color = Color.cyan;
						GUILayout.Label("Choose conversation:");
						GUI.color = Color.white;
						GUILayout.Label("Available conversations (searching for " + character.worlditem.DisplayName + "):");
						for (int i = 0; i < AvailableConvos.Count; i++) {
								if (!AvailableConvos[i].Contains("-State")) {
										GUILayout.BeginHorizontal();
										GUI.color = Color.white;
										if (talkative.State.ConversationName == AvailableConvos[i]) {
												GUI.color = Color.green;
										}
										if (GUILayout.Button(AvailableConvos[i], GUILayout.MaxWidth(400))) {
												talkative.State.ConversationName = AvailableConvos[i];
										}
										GUI.color = Color.red;
										if (GUILayout.Button("RESET", GUILayout.MaxWidth(75))) {
												Conversations.Get.ClearLocalConversation();
												Mods.Get.Runtime.DeleteMod("Conversation", AvailableConvos[i] + "-State");
										}
										GUILayout.EndHorizontal();
								}
						}
						GUI.color = Color.cyan;
						GUILayout.Label("Choose DTS:");
						GUI.color = Color.white;
						GUILayout.Label("Available DTS:");
						for (int i = 0; i < AvailableDTS.Count; i++) {
								GUI.color = Color.white;
								if (talkative.State.DTSSpeechName == AvailableDTS[i]) {
										GUI.color = Color.green;
								}
								if (GUILayout.Button(AvailableDTS[i], GUILayout.MaxWidth(400))) {
										talkative.State.DTSSpeechName = AvailableDTS[i];
								}
						}
				} else {
						character = null;
						talkative = null;
						GUILayout.Label("(No character in focus)");
				}
				GUILayout.EndArea();


				GUILayout.BeginArea(new Rect(OffsetX, OffsetY, Screen.width - OffsetX, Screen.height - OffsetY));

				GUI.color = Color.white;
				GUI.backgroundColor = Colors.Alpha(Color.black, 1.0f);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Next Mission")) {
						List <Mission> activeMissions = Missions.Get.ActiveMissions;
						for (int i = 0; i < activeMissions.Count; i++) {
								if (CurrentMission == null) {
										CurrentMission = activeMissions[i];
										break;
								} else {
										if (CurrentMission == activeMissions[i]) {
												CurrentMission = activeMissions[activeMissions.NextIndex(i)];
												break;
										}
								}
						}
				}

				if (GUILayout.Button("Prev Mission")) {
						List <Mission> activeMissions = Missions.Get.ActiveMissions;
						for (int i = 0; i < activeMissions.Count; i++) {
								if (CurrentMission == null) {
										CurrentMission = activeMissions[i];
										break;
								} else {
										if (CurrentMission == activeMissions[i]) {
												CurrentMission = activeMissions[activeMissions.PrevIndex(i)];
												break;
										}
								}
						}
				}
				GUILayout.EndHorizontal();

				if (CurrentMission == null) {
						GUILayout.BeginHorizontal();
						GUI.color = Color.red;
						GUILayout.Button("No mission");
						GUILayout.EndHorizontal();
						GUILayout.EndArea();
						return;
				}

				DrawMission(CurrentMission);
				GUILayout.EndArea();

		}

		public void DrawMission(Mission mission)
		{
				//GUILayout.FlexibleSpace ();
				GUILayout.BeginHorizontal();
				GUI.color = Color.cyan;
				if (mission.State.ObjectivesCompleted) {
						GUI.color = Color.green;
				}

				if (GUILayout.Button("Title: " + mission.State.Title + " (Click to complete)")) {
						mission.ForceComplete();
				}
				GUILayout.Label("Objectives Complted: " + mission.State.ObjectivesCompleted.ToString());
				GUILayout.Label("State: " + mission.State.Status.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("MISSION VARIABLES:");
				foreach (KeyValuePair <string,int> mv in mission.State.Variables) {
						GUI.color = Colors.ColorFromString(mv.Key, 200);
						GUILayout.Label(mv.Key + ": " + mv.Value.ToString() + " - ");
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical();
				GUILayout.Label("MISSION OBJECTIVES:");

				int depth = 1;
				List <string> objectiveNames = mission.State.GetObjectiveNames();
				for (int i = 0; i < objectiveNames.Count; i++) {
						ObjectiveState objective = mission.State.GetObjective(objectiveNames[i]);
						DrawObjective(objective, null, 0, depth);
				}
				GUILayout.EndVertical();
		}

		public void DrawObjective(ObjectiveState objective, ObjectiveState parentObjective, int currentChildObjective, int depth)
		{
				Color mainColor = Color.white;
				GUI.color = mainColor;
				GUILayout.BeginHorizontal();
				GUILayout.Space(depth * 20);

				mainColor = Color.cyan;
				if (objective.Completed || Flags.Check((uint)objective.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.green, 0.5f);
				}
				if (Flags.Check((uint)objective.Status, (uint)MissionStatus.Completed, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.green, 0.5f);
				}		
				if (Flags.Check((uint)objective.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.red, 0.5f);
				}

				if (GUILayout.Button(objective.Name + " (" + objective.FileName + ")")) {
						if (objective.Status == MissionStatus.Dormant) {
								objective.ParentObjective.mission.ActivateObjective(objective.FileName, MissionOriginType.None, string.Empty);
						} else {
								objective.ParentObjective.ForceComplete();
						}
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Space(depth * 20);
				GUILayout.Label("Completed: " + objective.Completed.ToString());
				GUILayout.Label("Status: " + objective.Status.ToString());
				GUILayout.EndHorizontal();
		}
}

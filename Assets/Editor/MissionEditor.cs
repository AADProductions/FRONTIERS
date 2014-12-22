using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
using Frontiers.World.Gameplay;
//like the conversation editor this class is very out of date
//i don't use it to actually edit missions any more
//but it can be useful at runtime to see what's happening
using Frontiers.Data;


[CustomEditor(typeof(Mission))]
public class MissionEditor : Editor
{
		protected Mission mission;

		public void Awake()
		{
				mission = (Mission)target;
		}

		ObjectiveState parentObjectiveForDelete = null;
		int childObjectiveToDelete = -1;
		ObjectiveState showScriptOptions = null;
		int objectiveScriptIndexToDelete = -1;
		ObjectiveState deleteScriptObjective = null;
		GUIStyle miniButtonStyle;
		GUIStyle textFieldStyle;
		GUIStyle LabelStyle;
		GUIStyle MiniLabelStyle;
		GUIStyle MiniTextStyle;
		GUIStyle ToolbarButtonStyle;
		//	GUIStyle BoldLabelStyle;
		public override void OnInspectorGUI()
		{
				parentObjectiveForDelete = null;
				deleteScriptObjective = null;

				miniButtonStyle = new GUIStyle(EditorStyles.miniButton);
				textFieldStyle = new GUIStyle(EditorStyles.textField);
				LabelStyle = new GUIStyle(EditorStyles.label);
				MiniLabelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
				MiniTextStyle = new GUIStyle(EditorStyles.miniTextField);
				ToolbarButtonStyle	= new GUIStyle(EditorStyles.toolbarButton);

				textFieldStyle.wordWrap = true;
				LabelStyle.wordWrap = true;
				MiniLabelStyle.wordWrap = true;
				MiniTextStyle.wordWrap = true;
				MiniLabelStyle.wordWrap = true;
				miniButtonStyle.wordWrap = true;

				textFieldStyle.fontStyle = FontStyle.Normal;
				LabelStyle.fontStyle = FontStyle.Normal;
				MiniLabelStyle.fontStyle = FontStyle.Normal;
				ToolbarButtonStyle.fontStyle = FontStyle.Normal;
				MiniLabelStyle.fontStyle = FontStyle.Normal;
				miniButtonStyle.fontStyle = FontStyle.Normal;

				miniButtonStyle.stretchWidth = true;
				miniButtonStyle.alignment = TextAnchor.MiddleCenter;

				GUILayout.BeginHorizontal();
				GUI.color = Color.cyan;
				if (mission.State.ObjectivesCompleted) {
						GUI.color = Color.green;
				}
				GUILayout.Toggle(mission.State.ObjectivesCompleted, "Objectives Complted");
				GUILayout.Label("Title:", MiniLabelStyle);
				mission.State.Name = EditorGUILayout.TextArea(mission.State.Name, textFieldStyle);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Status:", MiniLabelStyle);
				mission.State.Status = (MissionStatus)EditorGUILayout.EnumMaskField(mission.State.Status);
				GUILayout.Label("Completion Type:", EditorStyles.whiteMiniLabel);
				mission.State.CompletionType = (MissionCompletion)EditorGUILayout.EnumPopup(mission.State.CompletionType);
				GUILayout.EndHorizontal();

				GUILayout.Label("Description:", MiniLabelStyle);
				mission.State.Description = GUILayout.TextArea(mission.State.Description);

				GUILayout.Label("MISSION VARIABLES:", MiniLabelStyle);
				foreach (KeyValuePair <string,int> mv in mission.State.Variables) {
						GUILayout.Label(mv.Key + ": " + mv.Value.ToString(), MiniLabelStyle);
				}

				GUILayout.Label("MISSION OBJECTIVES:", MiniLabelStyle);

				int depth = 1;
				for (int i = 0; i < mission.State.FirstObjectives.Count; i++) {
						DrawObjective(mission.State.FirstObjectives[i], null, 0, ref depth);
				}

				miniButtonStyle.stretchWidth = true;
				GUI.color = Color.yellow;
				GUILayout.Label("SAVE AND LOAD:", MiniLabelStyle);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("\nSAVE\n", miniButtonStyle)) {
						mission.EditorSave();
				}
				if (GUILayout.Button("\nLOAD\n", miniButtonStyle)) {
						mission.EditorLoad();
				}
				GUILayout.EndHorizontal();

				if (parentObjectiveForDelete != null) {
						parentObjectiveForDelete.NextObjectiveStates.RemoveAt(childObjectiveToDelete);
				}

				if (deleteScriptObjective != null) {
						deleteScriptObjective.Scripts.RemoveAt(objectiveScriptIndexToDelete);
				}
		}

		public void DrawObjective(ObjectiveState objective, ObjectiveState parentObjective, int currentChildObjective, ref int depth)
		{
				miniButtonStyle.stretchWidth = false;
				MiniLabelStyle.stretchWidth = false;

				Color mainColor = Color.gray;
				GUI.color = mainColor;
				GUILayout.Label("_____________________________________________", MiniLabelStyle);
				GUILayout.BeginHorizontal();
				for (int i = 0; i < depth; i++) {
						GUILayout.Button("->", miniButtonStyle);
				}

				mainColor = Color.cyan;
				if (Flags.Check((uint)objective.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.green, 0.5f);
				}
				if (Flags.Check((uint)objective.Status, (uint)MissionStatus.Completed, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.green, 0.5f);
				}		
				if (Flags.Check((uint)objective.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAll)) {
						mainColor = Color.Lerp(mainColor, Color.red, 0.5f);
				}

				GUILayout.Label("OBJECTIVE:", MiniLabelStyle);
				objective.Name = EditorGUILayout.TextArea(objective.Name, textFieldStyle);
				GUILayout.Label("File name:", MiniLabelStyle);
				objective.FileName = EditorGUILayout.TextArea(objective.FileName, textFieldStyle);
				miniButtonStyle.stretchWidth = false;
				GUI.color = Color.red;
				if (depth > 1) {
						if (GUILayout.Button("X", miniButtonStyle)) {
								parentObjectiveForDelete = parentObjective;
								childObjectiveToDelete = currentChildObjective;
						}
				}
				GUI.color = mainColor;
				GUILayout.EndHorizontal();

				miniButtonStyle.stretchWidth = true;
				GUILayout.BeginHorizontal();
				GUILayout.Toggle(objective.Completed, "Complted");
				GUILayout.Label("Status:", MiniLabelStyle);
				objective.Status = (MissionStatus)EditorGUILayout.EnumMaskField(objective.Status);
				GUILayout.Label("Type:", MiniLabelStyle);
				objective.Type = (ObjectiveType)EditorGUILayout.EnumPopup(objective.Type);
				GUILayout.Label("Activation:", MiniLabelStyle);
				objective.Activation = (ObjectiveActivation)EditorGUILayout.EnumPopup(objective.Activation);
				GUILayout.EndHorizontal();

				GUILayout.Label("Description:", MiniLabelStyle);
				objective.Description = GUILayout.TextArea(objective.Description);
				GUILayout.Label("New Mission Description", MiniLabelStyle);
				objective.NewMissionDescription = GUILayout.TextArea(objective.NewMissionDescription);

				GUILayout.Label("SCRIPTS:", MiniLabelStyle);

				mainColor = Color.Lerp(Color.yellow, Color.white, 0.5f);

				GUI.color = mainColor;
				miniButtonStyle.stretchWidth = true;
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Add script", miniButtonStyle)) {
						showScriptOptions = objective;
				}
				if (GUILayout.Button("Add next objective", miniButtonStyle)) {
						objective.NextObjectiveStates.Add(new ObjectiveState());
				}
				GUILayout.EndHorizontal();

				if (showScriptOptions != null && showScriptOptions == objective) {
						GUI.color = Color.cyan;
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Complete Conversation Exchange")) {
								objective.Scripts.Add(new ObjectiveConversationExchange());
						}
						if (GUILayout.Button("Get Quest Item")) {
								objective.Scripts.Add(new ObjectiveGetQuestItem());
						}
						if (GUILayout.Button("Character Reach Quest Node")) {
								objective.Scripts.Add(new ObjectiveCharacterReachQuestNode());
						}
						if (GUILayout.Button("Enter Structure")) {
								objective.Scripts.Add(new ObjectiveEnterStructure());
						}
						if (GUILayout.Button("Visit Location")) {
								objective.Scripts.Add(new ObjectiveVisitLocation());
						}
						if (GUILayout.Button("Prevent Char Death")) {
								objective.Scripts.Add(new ObjectivePreventCharacterDeath());
						}
						GUILayout.EndHorizontal();
				}

				GUI.color = mainColor;

				bool finishedDrawing	= false;
				bool finishedLine = false;
				int currentIndex = 0;
				int lineIndex = 0;

				if (objective.Scripts.Count > 0) {
						foreach (ObjectiveScript script in objective.Scripts) {
								GUI.color = mainColor;
								GUILayout.BeginHorizontal();
								if (script.HasCompleted) {
										GUI.color = Color.green;
								}
								GUILayout.Toggle(script.HasCompleted, ("Script: " + script.GetType().Name));
								script.RequiresAll = GUILayout.Toggle(script.RequiresAll, ("Requires All: " + script.RequiresAll));
								GUI.color = mainColor;
								GUILayout.EndHorizontal();
								GUILayout.BeginHorizontal();
								DrawScript(script, objective, currentIndex);
								GUILayout.EndHorizontal();
						}
						/*
						GUILayout.BeginHorizontal ( );
						while (!finishedDrawing)
						{
							if (lineIndex == 0)
							{
								finishedLine = false;
							}

							GUI.color = mainColor;
							DrawScript (objective.Scripts [currentIndex], objective, currentIndex);

							lineIndex++;
							currentIndex++;

							if (lineIndex >= 5)
							{
								GUILayout.EndHorizontal ( );
								GUILayout.BeginHorizontal ( );
								finishedLine 	= true;
								lineIndex 		= 0;
							}

							if (currentIndex >= objective.Scripts.Count - 1)
							{
								finishedDrawing = true;
							}
						}

						if (!finishedLine)
						{
							GUILayout.EndHorizontal ( );
						}
						*/
				}

				depth++;

				int nextChildObjective = 0;
				foreach (ObjectiveState nextObjective in objective.NextObjectiveStates) {
						DrawObjective(nextObjective, objective, nextChildObjective, ref depth);
						nextChildObjective++;
				}
		}

		protected void DrawScript(ObjectiveScript script, ObjectiveState objective, int scriptIndex)
		{
				miniButtonStyle.stretchWidth = true;

				switch (script.GetType().Name) {
						//should be done with reflection
						case "ObjectiveGetQuestItem":
								ObjectiveGetQuestItem ogqe = (ObjectiveGetQuestItem)script;
								GUILayout.Label("Item Name:", MiniLabelStyle);
								ogqe.ItemName = GUILayout.TextField(ogqe.ItemName);
								break;

						case "ObjectiveConversationExchange":
								ObjectiveConversationExchange oce = (ObjectiveConversationExchange)script;
								GUILayout.Label("Conversation Name:", MiniLabelStyle);
								oce.ConversationName = GUILayout.TextField(oce.ConversationName);
								GUILayout.Label("Exchange Name:", MiniLabelStyle);
								oce.ExchangeName = GUILayout.TextField(oce.ExchangeName);
								break;

						case "ObjectiveCharacterReachQuestNode":
								ObjectiveCharacterReachQuestNode ocrqn = (ObjectiveCharacterReachQuestNode)script;
								GUILayout.Label("Character Name:", MiniLabelStyle);
								ocrqn.CharacterName = GUILayout.TextField(ocrqn.CharacterName);
								GUILayout.Label("Quest Node Name:", MiniLabelStyle);
								ocrqn.QuestNodeName = GUILayout.TextField(ocrqn.QuestNodeName);
								break;

						case "ObjectiveVisitLocation":
								ObjectiveVisitLocation ovl = (ObjectiveVisitLocation)script;
								DrawMobileReference(ovl.LocationReference);
								break;

						case "ObjectivePreventCharacterDeath":
								ObjectivePreventCharacterDeath opcd = (ObjectivePreventCharacterDeath)script;
								GUILayout.Label("Character Name:", MiniLabelStyle);
								opcd.CharacterName = GUILayout.TextField(opcd.CharacterName);
								break;

						default:
								GUILayout.Label("(Unknown script type)", MiniLabelStyle);
								break;
				}

				miniButtonStyle.stretchWidth = false;
				GUI.color = Color.red;
				if (GUILayout.Button("X")) {
						deleteScriptObjective = objective;
						objectiveScriptIndexToDelete	= scriptIndex;
				}
		}

		protected void DrawMobileReference (MobileReference mr)
		{		//TODO move this to Frontiers.Data, this is actually useful
				bool stretchWidthBeforeEntry	= miniButtonStyle.stretchWidth;
				miniButtonStyle.stretchWidth = false;
				GUI.color = Color.yellow;
		
				string finalPath = string.Empty;
				Stack <string> newGroupStack	= new Stack <string>();
				Stack <string> groupMinusLast	= new Stack <string>();
				bool doLastGroupDropDown = false;
				bool doChildItemDropDown = false;

				if (string.IsNullOrEmpty(mr.GroupPath)) {
						mr.GroupPath = "Root";
				}
				Stack <string> splitGroup = WIGroup.SplitPath(mr.GroupPath);
				//it's always root
				string lastGroupInPath = string.Empty;

				if (splitGroup.Count == 1) {
						lastGroupInPath = splitGroup.Pop();
						groupMinusLast.Push(lastGroupInPath);
						newGroupStack.Push(lastGroupInPath);
						GUILayout.Button(lastGroupInPath, miniButtonStyle);
				} else {
						while (splitGroup.Count > 0) {
								lastGroupInPath = splitGroup.Pop();
								if (splitGroup.Count > 0) {	//if there's still at least one more to go...
										//add it to both stacks
										groupMinusLast.Push(lastGroupInPath);
										newGroupStack.Push(lastGroupInPath);
										doLastGroupDropDown = true;
										GUILayout.Button(lastGroupInPath, miniButtonStyle);
								}
						}
				}

				bool madeGroupSelection = false;
				string newGroupSelection = string.Empty;
				string groupPathMinusLast = WIGroup.CombinePath(groupMinusLast);
				if (doLastGroupDropDown) {
						//get the path of the group, minus the last group
						List <string> currentGroups = Mods.Get.Editor.GroupChildGroupNames(groupPathMinusLast);
						if (currentGroups.Count > 0) {
								GUI.color = Color.Lerp(Color.yellow, Color.gray, 0.5f);
								int indexOfLastGroup = currentGroups.IndexOf(lastGroupInPath);
								if (indexOfLastGroup < 0) {
										indexOfLastGroup = 0;
								}
								int indexOfGroupSelection	= EditorGUILayout.Popup(indexOfLastGroup, currentGroups.ToArray());
								if (indexOfGroupSelection >= 0 && indexOfGroupSelection < currentGroups.Count) {
										newGroupSelection = currentGroups[indexOfGroupSelection];
										madeGroupSelection = true;
								}
						}
				}

				//if we picked a new group add the selected group
				//otherwise just add the old group
				if (madeGroupSelection) {
						newGroupStack.Push(newGroupSelection);
				} else {
						newGroupStack.Push(lastGroupInPath);
				}

				List <string> nextGroups = Mods.Get.Editor.GroupChildGroupNames(mr.GroupPath);
				if (nextGroups.Count > 0) {
						GUI.color = Color.green;
						if (GUILayout.Button("+", miniButtonStyle)) {	//if we click the add button, change the group
								newGroupStack.Push(nextGroups[0]);
						}
				}
		
				finalPath = WIGroup.CombinePath(newGroupStack);
		
				GUI.color = Color.red;
				if (GUILayout.Button("-", miniButtonStyle)) {
						//set the last group path to the one BEFORE the current last
						finalPath = groupPathMinusLast;
				}

				mr.GroupPath = finalPath;

				miniButtonStyle.stretchWidth = true;
				GUI.color = Color.white;
				//now get all child items
				List <string> childItemsInGroup = Mods.Get.Editor.GroupChildItemNames(mr.GroupPath);
				if (childItemsInGroup.Count == 0) {
						GUILayout.Label("(No child items)");
				} else {
						string newChildItemSelection	= string.Empty;
						int indexChildSelection = childItemsInGroup.IndexOf(mr.FileName);
						if (indexChildSelection < 0) {
								indexChildSelection = 0;
						}
						int indexOfChildItemSelection = EditorGUILayout.Popup(indexChildSelection, childItemsInGroup.ToArray());
						if (indexOfChildItemSelection >= 0 && indexOfChildItemSelection < childItemsInGroup.Count) {
								newChildItemSelection = childItemsInGroup[indexOfChildItemSelection];
								mr.FileName = newChildItemSelection;
						}
				}

				//mr.GroupPath = newGroupPath;
				//mr.FileName = newChildItemSelection;

				miniButtonStyle.stretchWidth = stretchWidthBeforeEntry;
		}
}

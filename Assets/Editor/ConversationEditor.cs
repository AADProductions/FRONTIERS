using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers;
using Frontiers.Story.Conversations;
//i used to edit conversations in-editor with this class
//i don't any more but it can still occationally be
//useful to see what's going on at runtime
[CustomEditor(typeof(Conversation))]
public class ConversationEditor : Editor
{
		protected Conversation conversation;
		protected bool openFileLoadSave	= false;
		protected static Exchange lastClickedExchange	= null;
		protected int maxDepth = 10;
		protected bool renamingExchange	= false;
		protected string exchangeRename = string.Empty;
		protected bool addingScript = false;
		protected bool addingOutgoingLink	= false;
		protected string outgoingLinkSearch	= string.Empty;
		protected bool drawExchanges = true;
		protected bool reflectionMode = false;
		protected int selectedScript = -1;
		protected int scriptToDelete = -1;
		GUIStyle miniButtonStyle;
		GUIStyle textFieldStyle;
		GUIStyle LabelStyle;
		GUIStyle MiniLabelStyle;
		GUIStyle MiniTextStyle;
		GUIStyle ToolbarButtonStyle;
		GUIStyle BoldLabelStyle;

		public void Awake()
		{
				conversation = (Conversation)target;
		}

		public override void OnInspectorGUI()
		{
				miniButtonStyle = new GUIStyle(EditorStyles.miniButton);
				textFieldStyle = new GUIStyle(EditorStyles.textField);
				LabelStyle = new GUIStyle(EditorStyles.label);
				MiniLabelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
				MiniTextStyle = new GUIStyle(EditorStyles.miniTextField);
				ToolbarButtonStyle	= new GUIStyle(EditorStyles.toolbarButton);
				BoldLabelStyle = new GUIStyle(EditorStyles.boldLabel);

				scriptToDelete = -1;

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

				GUI.color = Color.cyan;
				if (GUILayout.Button("(Refresh)", miniButtonStyle)) {
						conversation.Refresh();
				}
				GUI.color = Color.Lerp(Color.cyan, Color.gray, 0.5f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Conversation Name:", MiniLabelStyle);

				if (conversation.Props == null) {
						return;
				}

				conversation.Props.Name = GUILayout.TextField(conversation.Props.Name);
				conversation.Props.IsGeneric = GUILayout.Toggle(conversation.Props.IsGeneric, "Is Generic");
				GUILayout.EndHorizontal();

				miniButtonStyle.stretchWidth = false;
				textFieldStyle.stretchWidth = false;

				reflectionMode = GUILayout.Toggle(reflectionMode, "Reflection Mode");

				GUILayout.BeginHorizontal();
				GUILayout.Label("Availability Behavior:", MiniLabelStyle);
				conversation.Props.Availability = (AvailabilityBehavior)EditorGUILayout.EnumPopup(conversation.Props.Availability, miniButtonStyle);
				GUILayout.Label("Max times available:", MiniLabelStyle);
				conversation.Props.MaxTimesInitiated = EditorGUILayout.IntField(conversation.Props.MaxTimesInitiated);
				GUILayout.Label("Num times triggered: " + conversation.State.NumTimesInitiated.ToString(), MiniLabelStyle);
				GUILayout.EndHorizontal();

				string variableLabel = "Variables: ";
				foreach (KeyValuePair <string, SimpleVar> var in conversation.State.ConversationVariables) {
						variableLabel += var.Key + " (" + var.Value.DefaultValue.ToString() + "), ";
				}
				GUILayout.Label(variableLabel, MiniLabelStyle);

				miniButtonStyle.stretchWidth = true;
				textFieldStyle.stretchWidth = true;

				GUILayout.Label("Exchange failure response:", MiniLabelStyle);
				conversation.State.InitiateFailureResponse = EditorGUILayout.TextArea(conversation.State.InitiateFailureResponse, textFieldStyle);

				if (conversation.OpeningExchange != null) {
						DrawExchange(conversation.OpeningExchange, -1);
				}

				miniButtonStyle.stretchWidth = true;
				miniButtonStyle.alignment = TextAnchor.MiddleCenter;

				GUI.color = Color.yellow;
				GUILayout.Label("FILE SAVE AND LOAD OPTIONS:");
				if (!openFileLoadSave) {
						if (GUILayout.Button("(Click to open)", miniButtonStyle)) {
								openFileLoadSave = true;
						}
				} else {
						if (GUILayout.Button("\n(SAVE CONVERSATION TO FILE)\n", miniButtonStyle)) {
//				GameData.IO.InitializeSystemPaths ( );

								conversation.EditorSave();
								//Debug.Log ("Saved to disk");
						}
						if (GUILayout.Button("\n(LOAD CONVERSATION FROM FILE)\n", miniButtonStyle)) {
								conversation.EditorLoad();
								//Debug.Log ("Loaded from disk");
						}
				}

				if (scriptToDelete > -1) {
						lastClickedExchange.Scripts.RemoveAt(scriptToDelete);
				}		

				drawExchanges = true;		
		}

		public void DrawExchange(Exchange exchange, int drawDepth)
		{
				if (!drawExchanges) {
						return;
				}

				drawDepth++;
				Color depthColor	= Color.Lerp(Color.green, Color.white, (((float)drawDepth) / maxDepth));
				Color responseColor = GUI.color = Color.Lerp(Color.yellow, Color.red, 0.2f);
				Color selectedColor = Color.white;
				float blend = 0.0f;

				if (!exchange.HasParentExchange && drawDepth > 0) {
						GUI.color = Color.red;
				} else {
						GUI.color = depthColor;
				}


				if (lastClickedExchange != null) {
						if (lastClickedExchange == exchange) {
								blend = 0.75f;
						} else if (exchange.ParentExchange == lastClickedExchange) {
								blend = 0.25f;
						}
				}

				miniButtonStyle.stretchWidth = false;
				miniButtonStyle.alignment = TextAnchor.MiddleLeft;
				miniButtonStyle.fontSize = 10;

				ToolbarButtonStyle.stretchWidth = false;
				ToolbarButtonStyle.fixedWidth = 25f;
				ToolbarButtonStyle.alignment = TextAnchor.MiddleCenter;
				ToolbarButtonStyle.fontSize = 10;
				ToolbarButtonStyle.fontStyle = FontStyle.Bold;

				GUILayout.BeginHorizontal();
				GUI.color = Color.Lerp(Color.gray, selectedColor, blend);
				for (int i = 0; i <= drawDepth; i++) {
						GUILayout.Button(i.ToString() + ">", ToolbarButtonStyle);
				}
				GUI.color = Color.Lerp(depthColor, selectedColor, blend);
				if (exchange.Disable) {
						GUI.color = Color.Lerp(GUI.color, Color.black, 0.5f);
				}
				miniButtonStyle.stretchWidth = true;
				if (GUILayout.Button(exchange.PlayerDialog, miniButtonStyle)) {
						if (lastClickedExchange == exchange) {
								lastClickedExchange = null;
								selectedScript = -1;
								addingScript = false;
								addingOutgoingLink = false;
						} else {
								lastClickedExchange = exchange;
								selectedScript = -1;
								addingScript = false;
								addingOutgoingLink = false;
						}
				}
				miniButtonStyle.stretchWidth = true;
				miniButtonStyle.alignment = TextAnchor.MiddleRight;
				GUI.color = Color.Lerp(responseColor, selectedColor, blend);
				if (exchange.Disable) {
						GUI.color = Color.Lerp(GUI.color, Color.black, 0.5f);
				}
				if (GUILayout.Button(exchange.CharacterResponse, miniButtonStyle)) {
						if (lastClickedExchange == exchange) {
								lastClickedExchange = null;
								selectedScript = -1;
								addingScript = false;
								addingOutgoingLink = false;
						} else {
								lastClickedExchange = exchange;
								selectedScript = -1;
								addingScript = false;
								addingOutgoingLink = false;
						}
				}
				GUI.color = Color.red;
				miniButtonStyle.stretchWidth	= false;
				miniButtonStyle.alignment = TextAnchor.MiddleCenter;
				if (GUILayout.Button("X", miniButtonStyle)) {
						//conversation.DeleteExchange (exchange);
				}
				GUILayout.EndHorizontal();

				if (lastClickedExchange != exchange) {
						foreach (Exchange outgoingExchange in exchange.OutgoingChoices) {
								DrawExchange(outgoingExchange, drawDepth);
						}
						return;
				}

				miniButtonStyle.stretchWidth	= false;
				miniButtonStyle.alignment = TextAnchor.MiddleLeft;

				BoldLabelStyle.stretchWidth = false;
				BoldLabelStyle.alignment = TextAnchor.MiddleLeft;

				GUI.color = Color.cyan;

		
				if (!exchange.RequirementsAreMet) {
						GUI.color = Color.Lerp(GUI.color, Color.red, 0.5f);
				}

				GUILayout.BeginHorizontal();
				if (renamingExchange) {
						GUI.color = Color.red;
						exchangeRename = GUILayout.TextField(exchangeRename);
						if (GUILayout.Button("Rename")) {
//				if (conversation.RenameExchange (exchange, exchangeRename))
//				{
//					renamingExchange = false;
//					exchangeRename = string.Empty;
//				}
						}
						if (GUILayout.Button("Cancel")) {
								renamingExchange = false;
								exchangeRename = string.Empty;
						}
				} else {
						GUI.color = Color.cyan;
						GUILayout.Label(" " + exchange.Name + " ", BoldLabelStyle);
						if (GUILayout.Button("Rename")) {
								renamingExchange = true;
						}
				}
				miniButtonStyle.alignment = TextAnchor.MiddleCenter;
				miniButtonStyle.stretchWidth	= true;
				GUI.color = Color.cyan;
				exchange.Disable = GUILayout.Toggle(exchange.Disable, " Disable", EditorStyles.toggle);
				exchange.Availability = (AvailabilityBehavior)EditorGUILayout.EnumPopup(exchange.Availability, miniButtonStyle);
				GUILayout.Label("Max Times: ", MiniLabelStyle);
				exchange.MaxTimesChosen	= EditorGUILayout.IntField(exchange.MaxTimesChosen);

				if (exchange.HasParentExchange) {
						GUILayout.Label("Parent: " + exchange.ParentExchange.Name);
				} else if (drawDepth > 0) {
						GUI.color = Color.red;
						GUILayout.Label("ERROR No parent, please refresh!");
				}

				if (exchange.ParentConversation == null) {
						GUI.color = Color.red;
						GUILayout.Label("ERROR No parent conversation, please refresh!");
				}
				exchange.OutgoingStyle	= (ExchangeOutgoingStyle)EditorGUILayout.EnumPopup(exchange.OutgoingStyle, miniButtonStyle);
				GUILayout.EndHorizontal();

				Color scriptsColor = Color.Lerp(Color.white, Color.yellow, 0.25f);

				GUI.color = Color.cyan;
				if (exchange.HasIncomingChoices) {
						GUI.color = responseColor;
						GUILayout.Label("Incoming player dialog:", MiniLabelStyle);
						foreach (Exchange incomingChoice in exchange.IncomingChoices) {
								if (GUILayout.Button(incomingChoice.PlayerDialog, miniButtonStyle)) {

								}
						}
				}

				textFieldStyle.alignment = TextAnchor.MiddleLeft;
				textFieldStyle.stretchWidth = true;
				MiniLabelStyle.stretchWidth = false;

				GUI.color = Color.Lerp(Color.white, Color.blue, 0.25f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Player:", MiniLabelStyle);
				exchange.PlayerDialog = EditorGUILayout.TextArea(exchange.PlayerDialog, textFieldStyle);
				GUILayout.Label("Summary:", MiniLabelStyle);
				exchange.PlayerDialogSummary = EditorGUILayout.TextArea(exchange.PlayerDialogSummary, textFieldStyle);
				GUILayout.EndHorizontal();
				GUILayout.Label("Clean: " + exchange.CleanPlayerDialog, MiniLabelStyle);

				textFieldStyle.alignment = TextAnchor.MiddleLeft;
				textFieldStyle.stretchWidth = true;
				MiniLabelStyle.stretchWidth = false;

				GUI.color = responseColor;//Color.Lerp (Color.yellow, Color.red, 0.5f);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Character:", MiniLabelStyle);
				exchange.CharacterResponse = EditorGUILayout.TextArea(exchange.CharacterResponse, textFieldStyle);
				GUILayout.EndHorizontal();
				GUILayout.Label("Clean: " + exchange.CleanCharacterResponse, MiniLabelStyle);

				GUI.color = scriptsColor;
				string scriptsTitle = "Scripts: ";
				if (exchange.Scripts.Count == 0) {
						scriptsTitle += "(None)";
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label(scriptsTitle, MiniLabelStyle);
				if (!addingScript) {
						if (GUILayout.Button("(Add New Script)")) {
								addingScript = true;
						}
						GUILayout.EndHorizontal();
				} else {
						GUI.color = Color.Lerp(Color.black, scriptsColor, 0.85f);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Require World Item", miniButtonStyle)) {
								exchange.AddScript <RequireWorldItem>();
								addingScript = false;
						}
						if (GUILayout.Button("Require Character WI Script", miniButtonStyle)) {
								exchange.AddScript <RequireCharWIScript>();
								addingScript = false;
						}
						if (GUILayout.Button("Require Exchange Enabled", miniButtonStyle)) {
								exchange.AddScript <RequireExchangeEnabled>();
								addingScript = false;
						}
						if (GUILayout.Button("Set Character Emotion", miniButtonStyle)) {
								exchange.AddScript <ChangeCharacterEmotion>();
								addingScript = false;
						}
						if (GUILayout.Button("Give Items to Player", miniButtonStyle)) {
								exchange.AddScript <GiveGenericItemsToPlayer>();
								addingScript = false;
						}
						if (GUILayout.Button("Change Conv. Variable", miniButtonStyle)) {
								exchange.AddScript <ChangeConversationVariable>();
								addingScript = false;
						}
						if (GUILayout.Button("Add WIScript to Char", miniButtonStyle)) {
								exchange.AddScript <AddWIScriptToCharacter>();
								addingScript = false;
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Activate Mission", miniButtonStyle)) {
								exchange.AddScript <ActivateMission>();
								addingScript = false;
						}
						if (GUILayout.Button("Activate Mission Objective", miniButtonStyle)) {
								exchange.AddScript <ActivateMissionObjective>();
								addingScript = false;
						}
						if (GUILayout.Button("Require Mission Status", miniButtonStyle)) {
								exchange.AddScript <RequireMissionStatus>();
								addingScript = false;
						}
						if (GUILayout.Button("Require Objective Status", miniButtonStyle)) {
								exchange.AddScript <RequireObjectiveStatus>();
								addingScript = false;
						}
						if (GUILayout.Button("Require Conv. Variable", miniButtonStyle)) {
								exchange.AddScript <RequireConversationVariable>();
								addingScript = false;
						}
						if (GUILayout.Button("RequireQuestItem", miniButtonStyle)) {
								exchange.AddScript <RequireQuestItem>();
								addingScript = false;
						}
						GUILayout.EndHorizontal();
				}
				GUI.color = scriptsColor;
				for (int i = 0; i < exchange.Scripts.Count; i++) {
						DrawScript(exchange.Scripts[i], i, scriptsColor);
				}

				GUI.color = Color.cyan;
				string mergedStrings = string.Join(", ", exchange.LinkedOutgoingChoices.ToArray());
				GUILayout.Label("Linked outgoing options: " + mergedStrings, MiniLabelStyle);

				/*
				GUI.color = Color.cyan;
				GUILayout.BeginHorizontal ( );
				GUILayout.Label ("Outgoing options:", MiniLabelStyle);
				if (addingOutgoingLink)
				{
					GUILayout.BeginHorizontal ( );
					GUILayout.Label ("Exchange name:", MiniLabelStyle);
					outgoingLinkSearch = GUILayout.TextField (outgoingLinkSearch);
					GUILayout.EndHorizontal ( );

					int currentColumn 		= 0;
					bool includeLastEnd = false;
					foreach (Exchange outgoingExchange in conversation.GetExchanges (outgoingLinkSearch))
					{
						if (currentColumn == 0)
						{
							GUILayout.BeginHorizontal ( );
							includeLastEnd = true;
						}

						GUI.color = Color.cyan;
						if (exchange.LinkedOutgoingChoices.Contains (outgoingExchange.Name))
						{
							GUI.color = Color.Lerp (Color.cyan, Color.white, 0.5f);
							GUILayout.Button (outgoingExchange.Name);
						}
						else if (GUILayout.Button (outgoingExchange.Name))
						{
							exchange.LinkedOutgoingChoices.Add (outgoingExchange.Name);
							addingOutgoingLink = false;
							outgoingLinkSearch = string.Empty;
						}

						currentColumn++;
						if (currentColumn >= 5)
						{
							GUILayout.EndHorizontal ( );
							includeLastEnd = false;
							currentColumn = 0;
						}
						GUI.color = Color.cyan;
					}

					if (includeLastEnd)
					{
						GUILayout.EndHorizontal ( );
					}
				}
				else if (GUILayout.Button ("(Add linked outgoing choice)", miniButtonStyle))
				{
					addingOutgoingLink = true;
					outgoingLinkSearch = string.Empty;
				}
				if (GUILayout.Button ("(Create child outgoing choice)", miniButtonStyle))
				{
				 	lastClickedExchange = conversation.CreateOutgoingChoice (exchange);
					drawExchanges 		= false;
					selectedScript 		= -1;
					addingScript 		= false;
					addingOutgoingLink	= false;
					conversation.Refresh ( );
				}
				GUILayout.EndHorizontal ( );
				*/

				GUI.color = Color.cyan;
				foreach (Exchange outgoingExchange in exchange.OutgoingChoices) {
						DrawExchange(outgoingExchange, drawDepth);
				}
		}

		public void DrawScript(ExchangeScript script, int index, Color scriptsColor)
		{
				GUI.color = scriptsColor;
				MiniLabelStyle.wordWrap = false;
				string scriptType = script.GetType().Name;

				if (reflectionMode) {
						//List <FieldInfo> field = System.Reflection.Fiel
				} else {

						if (index == selectedScript) {
								GUILayout.BeginHorizontal();
								miniButtonStyle.stretchWidth = false;
								GUI.color = Color.red;
								if (GUILayout.Button("X", miniButtonStyle)) {
										scriptToDelete = index;
								}
								GUI.color = Color.yellow;
								GUILayout.Label(scriptType, BoldLabelStyle);
								GUILayout.Label("Num times triggered: " + script.NumTimesTriggered.ToString(), MiniLabelStyle);
								GUILayout.EndHorizontal();

								GUI.color = Color.yellow;
								miniButtonStyle.stretchWidth = true;
								miniButtonStyle.alignment = TextAnchor.MiddleCenter;

								GUILayout.BeginHorizontal();
								GUILayout.Label("Description: ", MiniLabelStyle);
								script.Description = GUILayout.TextField(script.Description);
								GUILayout.Label("Summary: ", MiniLabelStyle);
								script.Summary = GUILayout.TextField(script.Summary);
								GUILayout.EndHorizontal();
				
								GUILayout.BeginHorizontal();
								script.CallOn = (ExchangeAction)EditorGUILayout.EnumPopup(script.CallOn);	
								script.IgnoreChoiceToLeave	= GUILayout.Toggle(script.IgnoreChoiceToLeave, "Ignore choice to leave");
								GUILayout.EndHorizontal();

								switch (scriptType) {
										//this is out of date, obviously
										//and would be a lot better using reflection
										case "RequireWorldItem":
												RequireWorldItem requireWorldItem = (RequireWorldItem)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("World Item Name: ", MiniLabelStyle);
												requireWorldItem.WorldItemName = GUILayout.TextField(requireWorldItem.WorldItemName);
												GUILayout.EndHorizontal();
												break;

										case "RequireCharWIScript":
												RequireCharWIScript requireCharWiScript = (RequireCharWIScript)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("WIScript Name: ", MiniLabelStyle);
												requireCharWiScript.WIScriptName = GUILayout.TextField(requireCharWiScript.WIScriptName);
												GUILayout.EndHorizontal();
												break;

										case "RequireExchangeEnabled":
												RequireExchangeEnabled requireExchangeEnabled = (RequireExchangeEnabled)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Exchange Name: ", MiniLabelStyle);
												requireExchangeEnabled.ExchangeName = GUILayout.TextField(requireExchangeEnabled.ExchangeName);
												requireExchangeEnabled.Enabled = GUILayout.Toggle(requireExchangeEnabled.Enabled, "Enabled");
												GUILayout.EndHorizontal();
												break;
					
										case "ChangeCharacterEmotion":
												ChangeCharacterEmotion changeCharacterEmotion = (ChangeCharacterEmotion)script;
												GUILayout.BeginHorizontal();
												changeCharacterEmotion.Emotion = (EmotionalState)EditorGUILayout.EnumPopup(changeCharacterEmotion.Emotion);
												GUILayout.EndHorizontal();
												break;
					
										case "GiveGenericItemsToPlayer":
												GiveGenericItemsToPlayer giveGenericItemsToPlayer = (GiveGenericItemsToPlayer)script;
												GUILayout.BeginHorizontal();
												giveGenericItemsToPlayer.PackName = GUILayout.TextField(giveGenericItemsToPlayer.PackName);
												giveGenericItemsToPlayer.PrefabName = GUILayout.TextField(giveGenericItemsToPlayer.PrefabName);
												giveGenericItemsToPlayer.NumItems = EditorGUILayout.IntField(giveGenericItemsToPlayer.NumItems);
												GUILayout.EndHorizontal();
												break;	
					
										case "ActivateMission":
												ActivateMission activateMission = (ActivateMission)script;
												GUILayout.BeginHorizontal();
												activateMission.MissionName = GUILayout.TextField(activateMission.MissionName);
												activateMission.OriginTypeOverride = (MissionOriginType)EditorGUILayout.EnumPopup(activateMission.OriginTypeOverride);
												activateMission.OriginNameOverride = GUILayout.TextField(activateMission.OriginNameOverride);
												GUILayout.EndHorizontal();
												break;	

										case "ChangeMissionDescription":
												ChangeMissionDescription changeMissionDescription = (ChangeMissionDescription)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Mission Name: ", MiniLabelStyle);
												changeMissionDescription.MissionName	= GUILayout.TextField(changeMissionDescription.MissionName);
												GUILayout.Label("New Description: ", MiniLabelStyle);
												changeMissionDescription.NewDescription	= GUILayout.TextField(changeMissionDescription.NewDescription);
												changeMissionDescription.OnlyFirstTime = GUILayout.Toggle(changeMissionDescription.OnlyFirstTime, "Only First Time");
												changeMissionDescription.OnlyWhenActive	= GUILayout.Toggle(changeMissionDescription.OnlyWhenActive, "Only When Active");
												GUILayout.EndHorizontal();
												break;	
					
										case "ActivateMissionObjective":
												ActivateMissionObjective activateMissionObjective = (ActivateMissionObjective)script;
												GUILayout.BeginHorizontal();
												activateMissionObjective.MissionName = GUILayout.TextField(activateMissionObjective.MissionName);
												activateMissionObjective.ObjectiveName = GUILayout.TextField(activateMissionObjective.ObjectiveName);
												activateMissionObjective.OriginTypeOverride = (MissionOriginType)EditorGUILayout.EnumPopup(activateMissionObjective.OriginTypeOverride);
												activateMissionObjective.OriginNameOverride = GUILayout.TextField(activateMissionObjective.OriginNameOverride);
												GUILayout.EndHorizontal();
												break;	

										case "RequireMissionStatus":
												RequireMissionStatus requireMissionStatus = (RequireMissionStatus)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Mission Name: ", MiniLabelStyle);
												requireMissionStatus.MissionName = GUILayout.TextField(requireMissionStatus.MissionName);
												requireMissionStatus.Status = (MissionStatus)EditorGUILayout.EnumPopup(requireMissionStatus.Status);
												GUILayout.EndHorizontal();
												break;	

										case "RequireObjectiveStatus":
												RequireObjectiveStatus requireObjectiveStatus = (RequireObjectiveStatus)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Mission Name: ", MiniLabelStyle);
												requireObjectiveStatus.MissionName = GUILayout.TextField(requireObjectiveStatus.MissionName);
												GUILayout.Label("Objective Name: ", MiniLabelStyle);
												requireObjectiveStatus.ObjectiveName = GUILayout.TextField(requireObjectiveStatus.ObjectiveName);
												requireObjectiveStatus.Status = (MissionStatus)EditorGUILayout.EnumPopup(requireObjectiveStatus.Status);
												GUILayout.EndHorizontal();
												break;	

										case "RequireMissionVariable":
												RequireMissionVariable requireMissionVariable = (RequireMissionVariable)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Variable Name: ", MiniLabelStyle);
												requireMissionVariable.VariableName = GUILayout.TextField(requireMissionVariable.VariableName);
												GUILayout.Label("Check Type: ", MiniLabelStyle);
												requireMissionVariable.CheckType = (VariableCheckType)EditorGUILayout.EnumPopup(requireMissionVariable.CheckType);
												switch (requireMissionVariable.CheckType) {
														case VariableCheckType.GreaterThan:
														default:
																GUILayout.Label("Variable is > this value: ", MiniLabelStyle);
																break;
						
														case VariableCheckType.GreaterThanOrEqualTo:
																GUILayout.Label("Variable is >= this value: ", MiniLabelStyle);
																break;
						
														case VariableCheckType.LessThan:
																GUILayout.Label("Variable is < this value: ", MiniLabelStyle);
																break;
						
														case VariableCheckType.LessThanOrEqualTo:
																GUILayout.Label("Variable is <= this value: ", MiniLabelStyle);
																break;
												}
												requireMissionVariable.VariableValue = EditorGUILayout.IntField(requireMissionVariable.VariableValue);
												GUILayout.EndHorizontal();
												break;	

										case "ChangeConversationVariable":
												ChangeConversationVariable changeConversationVariable = (ChangeConversationVariable)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Variable Name: ", MiniLabelStyle);
												changeConversationVariable.VariableName = GUILayout.TextField(changeConversationVariable.VariableName);
												GUILayout.Label("Change Type: ", MiniLabelStyle);
												changeConversationVariable.ChangeType = (ChangeVariableType)EditorGUILayout.EnumPopup(changeConversationVariable.ChangeType);
												switch (changeConversationVariable.ChangeType) {
														case ChangeVariableType.Increment:
														default:
																GUILayout.Label("Num times to increment: ", MiniLabelStyle);
																changeConversationVariable.SetValue	= EditorGUILayout.IntField(Mathf.Clamp(changeConversationVariable.SetValue, 1, 100));
																break;

														case ChangeVariableType.Decrement:
																GUILayout.Label("Num times to decrement: ", MiniLabelStyle);
																changeConversationVariable.SetValue	= EditorGUILayout.IntField(Mathf.Clamp(changeConversationVariable.SetValue, 1, 100));
																break;

														case ChangeVariableType.SetValue:
																GUILayout.Label("New Value: ", MiniLabelStyle);
																changeConversationVariable.SetValue	= EditorGUILayout.IntField(changeConversationVariable.SetValue);
																break;
												}
												GUILayout.EndHorizontal();
												break;

										case "RequireConversationVariable":
												RequireConversationVariable requireConversationVariable = (RequireConversationVariable)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Variable Name: ", MiniLabelStyle);
												requireConversationVariable.VariableName = GUILayout.TextField(requireConversationVariable.VariableName);
												GUILayout.Label("Check Type: ", MiniLabelStyle);
												requireConversationVariable.CheckType = (VariableCheckType)EditorGUILayout.EnumPopup(requireConversationVariable.CheckType);
												switch (requireConversationVariable.CheckType) {
														case VariableCheckType.GreaterThan:
														default:
																GUILayout.Label("Variable is > this value: ", MiniLabelStyle);
																break;

														case VariableCheckType.GreaterThanOrEqualTo:
																GUILayout.Label("Variable is >= this value: ", MiniLabelStyle);
																break;

														case VariableCheckType.LessThan:
																GUILayout.Label("Variable is < this value: ", MiniLabelStyle);
																break;

														case VariableCheckType.LessThanOrEqualTo:
																GUILayout.Label("Variable is <= this value: ", MiniLabelStyle);
																break;
												}
												requireConversationVariable.VariableValue = EditorGUILayout.IntField(requireConversationVariable.VariableValue);
												GUILayout.EndHorizontal();
												break;

										case "RequireQuestItem":
												RequireQuestItem requireQuestItem = (RequireQuestItem)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Quest Item Name: ", MiniLabelStyle);
												requireQuestItem.ItemName = GUILayout.TextField(requireQuestItem.ItemName);
												GUILayout.EndHorizontal();
												break;

										case "AddWIScriptToCharacter":
												AddWIScriptToCharacter addWIScriptToCharacter = (AddWIScriptToCharacter)script;
												GUILayout.BeginHorizontal();
												GUILayout.Label("Character name: ", MiniLabelStyle);
												addWIScriptToCharacter.CharacterName = GUILayout.TextField(addWIScriptToCharacter.CharacterName);
												GUILayout.Label("Script name: ", MiniLabelStyle);
												addWIScriptToCharacter.ScriptName = GUILayout.TextField(addWIScriptToCharacter.ScriptName);
												GUILayout.EndHorizontal();
												break;
								}
						} else {
								GUI.color = scriptsColor;
								GUILayout.BeginHorizontal();
								miniButtonStyle.stretchWidth = true;
								if (GUILayout.Button(scriptType)) {
										selectedScript = index;
								}
								miniButtonStyle.stretchWidth = false;
								GUI.color = Color.red;
								if (GUILayout.Button("X", miniButtonStyle)) {
										scriptToDelete = index;
								}
								GUILayout.EndHorizontal();
						}
				}

				miniButtonStyle.stretchWidth = true;
		}

		public void DrawAvailabilityProperties(ExchangeScript script, int index)
		{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Call on: ", MiniLabelStyle);
				script.CallOn = (ExchangeAction)EditorGUILayout.EnumPopup(script.CallOn);
				GUILayout.Label("Availability: ", MiniLabelStyle);
				script.Availability = (AvailabilityBehavior)EditorGUILayout.EnumPopup(script.Availability);
				GUILayout.Label("Max times triggered: ", MiniLabelStyle);
				script.MaxTimesTriggered = EditorGUILayout.IntField(script.MaxTimesTriggered);
				GUILayout.EndHorizontal();
		}

		public string GetSpaces(int numSpaces)
		{
				string newString = string.Empty;
				for (int i = 0; i <= numSpaces; i++) {
						newString = newString + i.ToString() + ".";
				}
				return newString;
		}
}

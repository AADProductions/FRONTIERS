using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.Story.Conversations
{
		[ExecuteInEditMode]
		public class Conversation : MonoBehaviour
		{
				public string ConversationName;
				public static bool ConversationInProgress = false;
				[NonSerialized]
				public static Conversation LastInitiatedConversation;
				[NonSerialized]
				public static Exchange LastChosenExchange;
				[HideInInspector]
				public ConversationProps Props;
				[HideInInspector]
				public ConversationState State;
				protected Dictionary <string, Exchange> mExchangeLookup;

				public bool Initiating {
						get {
								return mInitiating;
						}
				}

				public void Awake()
				{
						mExchangeLookup = new Dictionary<string, Exchange>();
						mRunningOutgoingChoices = new HashSet <Exchange>();
						DisplayVariables = new List <string>();
						mExchanges = new List <Exchange>();
						mAlwaysInclude = new HashSet <Exchange>();
						mChangedNames = new Dictionary <string, string>();
						mInitiating = false;
				}

				public bool IsActive {
						get {
								return SpeakingCharacter != null;
						}
				}

				public bool IsAvailable {
						get {
								bool isAvailable = true;

								switch (Props.Availability) {
										case AvailabilityBehavior.Once:
												isAvailable = (State.NumTimesInitiated <= 0);
												break;

										case AvailabilityBehavior.Max:
												isAvailable = (State.NumTimesInitiated <= Props.MaxTimesInitiated);
												break;

										default:
												break;
								}
								return isAvailable;
						}
				}

				public bool Continues {
						get {
								return mRunningOutgoingChoices.Count > 0;
						}
				}

				public Exchange OpeningExchange {
						get {
								return Props.DefaultOpeningExchange;
						}
				}

				public Exchange LatestExchange = null;

				public List <Exchange> Exchanges {
						get {
								return mExchanges;
						}
				}

				public Character SpeakingCharacter;
				public Talkative SpeakingCharacterTalkative;
				public List <string> DisplayVariables;
				// = new List <string> ();
				public bool DespawnCharacterAfterConversation = false;

				public List <Exchange> RunningOutgoingChoices {
						get {
								List <Exchange> runningOutgoingChoices = new List <Exchange>();
								runningOutgoingChoices.AddRange(mRunningOutgoingChoices);
								runningOutgoingChoices.Sort();
								return runningOutgoingChoices;
						}
				}

				protected HashSet <Exchange> mRunningOutgoingChoices;
				// = new HashSet <Exchange> ();
				public bool CanLeave {
						get {
								return State.CanLeave;
						}
				}

				public virtual string CleanDialog(string dialog)
				{
						if (string.IsNullOrEmpty(dialog)) {
								return string.Empty;
						}
						//now that we've repalced all the character names, wrap the other
						//bracketed text to make it a different color
						if (GameManager.Get.ConversationsInterpretScripts) {
								//TODO make this regex / put it in GameData
								dialog = GameData.InterpretScripts(dialog, Profile.Get.CurrentGame.Character, this);
								dialog = dialog.Replace("[break]", "\n");
								dialog = dialog.Replace("[Break]", "\n");
						}
						if (GameManager.Get.ConversationsWrapBracketedDialog) {
								dialog = WrapBracketedDialogInColor(dialog);
						}
						return dialog;
				}

				public void Initiate(Character speakingCharacter, Talkative talkative)
				{
						Debug.Log("Initiating conversation with " + speakingCharacter.FullName);
						SpeakingCharacter = speakingCharacter;
						SpeakingCharacterTalkative = talkative;

						if (SpeakingCharacter.State.Emotion == EmotionalState.Angry) {
								GUI.GUIManager.PostInfo(SpeakingCharacter.State.Name.FirstName + " doesn't want to speak to you.");
								return;
						} else {
								mInitiating = true;
								StartCoroutine(InitiateoverTime());
						}
				}

				protected IEnumerator InitiateoverTime ( ) {

						//at this point the convo will have been loaded
						mExchanges.Clear();
						mExchangeLookup.Clear();
						mAlwaysInclude.Clear();

						//add all the exchanges to the lookup
						mExchanges.Add(Props.DefaultOpeningExchange);
						Exchange exchange = null;
						for (int i = 0; i < Props.Exchanges.Count; i++) {
								exchange = Props.Exchanges[i];
								exchange.ParentConversation = this;
								mExchanges.SafeAdd(exchange);
						}
						yield return null;
						//refresh the lookup table
						for (int i = 0; i < mExchanges.Count; i++) {
								exchange = mExchanges[i];
								mExchangeLookup.Add(exchange.Name, exchange);
								if (exchange.AlwaysInclude) {
										mAlwaysInclude.Add(exchange);
								}
						}
						yield return null;
						Exchange parentExchange = null;
						//now that the lookup table is refreshed
						//set the owners and refresh each exchange
						for (int i = 0; i < mExchanges.Count; i++) {
								exchange = mExchanges[i];
								if (!string.IsNullOrEmpty(exchange.ParentExchangeName)) {
										mExchangeLookup.TryGetValue(exchange.ParentExchangeName, out parentExchange);
								}
								exchange.SetOwners(this, parentExchange);
								exchange.Refresh();
						}
						yield return null;
						//alright everything's linked up, let's get this party started
						if (OpeningExchange.IsAvailable) {
								ConversationInProgress = true;
								SpeakingCharacter.State.KnowsPlayer = true;
								LastInitiatedConversation = this;
								State.NumTimesInitiated++;
								LatestExchange = null;
								mRunningOutgoingChoices.Clear();
								OpeningExchange.Refresh();
								string dtsOnFailure = string.Empty;
								MakeOutgoingChoice(OpeningExchange, out dtsOnFailure);

								Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcConverseStart, WorldClock.AdjustedRealTime);
						} else {
								Debug.Log("Opening exchange is not available");
								ConversationInProgress = false;
								SpeakingCharacter.worlditem.RefreshHud();
								if (!string.IsNullOrEmpty(OpeningExchange.DtsOnFailure)) {
										SpeakingCharacterTalkative.SayDTS(OpeningExchange.DtsOnFailure);
								}
						}
						mInitiating = false;
						yield break;
				}

				public void Clear()
				{

				}

				public virtual bool End()
				{
						if (LastChosenExchange != null) {
								LastChosenExchange.OnConcludeExchange();
						}
						if (State.LeaveOnEnd || DespawnCharacterAfterConversation) {
								SpeakingCharacter.Leave();
								//reset this so the next convo doesn't do the same thing
								DespawnCharacterAfterConversation = false;
						}
						Talkative talkative = null;
						if (SpeakingCharacter.worlditem.Is <Talkative>(out talkative)) {
								talkative.EndConversation();
						}
						LastChosenExchange = null;
						//save state
						Save();
						//broadcast conclusion
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcConverseEnd, WorldClock.AdjustedRealTime);
						//destroy this a little bit
						mConcluded = true;
						SpeakingCharacter = null;
						GameObject.Destroy(this, 0.25f);
						ConversationInProgress = false;
						//if we've been asked to play a cutscene do it now
						if (!string.IsNullOrEmpty(gCutsceneOnFinishName)) {
								Cutscene.CurrentCutsceneAnchor = gCutsceneOnFinishAnchor;
								Application.LoadLevelAdditive(gCutsceneOnFinishName);
						}
						//then reset
						gCutsceneOnFinishName = string.Empty;
						gCutsceneOnFinishAnchor = null;
						return true;
				}

				public void ShowCutsceneOnFinish(string cutsceneName, GameObject anchor)
				{
						gCutsceneOnFinishName = cutsceneName;
						gCutsceneOnFinishAnchor = anchor;
				}

				protected static string gCutsceneOnFinishName;
				protected static GameObject gCutsceneOnFinishAnchor;

				public void ForceEnd()
				{	//no muss, no fuss
						if (IsActive) {
								GUI.GUIManager.PostWarning(SpeakingCharacter.FullName + " has ended the conversation.");
						}
						End();
				}

				public List <Exchange> GetExchanges(List <int> exchangeIndexes)
				{
						List <Exchange> exchanges = new List <Exchange>();
						foreach (int exchangeIndex in exchangeIndexes) {
								Exchange nextExchange = null;
								string exchangeName = string.Empty;
								if (State.ExchangeNames.TryGetValue(exchangeIndex, out exchangeName)) {
										if (mExchangeLookup.TryGetValue(exchangeName, out nextExchange)) {
												exchanges.Add(nextExchange);
										}
								}
						}
						return exchanges;
				}

				public List <Exchange> GetExchanges(List <string> exchangeNames)
				{
						List <Exchange> exchanges = new List <Exchange>();
						foreach (string exchangeName in exchangeNames) {
								Exchange nextExchange = null;
								if (mExchangeLookup.TryGetValue(exchangeName, out nextExchange)) {
										exchanges.Add(nextExchange);
								}
						}
						return exchanges;
				}

				public List <Exchange> GetExchanges(string searchString)
				{
						List <Exchange> exchanges = new List <Exchange>();
						foreach (Exchange exchange in mExchanges) {
								if (exchange.Name.Contains(searchString)) {
										exchanges.Add(exchange);
								}
						}
						return exchanges;
				}

				public bool GetExchange(string exchangeName, out Exchange exchange)
				{
						exchange = null;
						bool result = false;
						if (mExchangeLookup.TryGetValue(exchangeName, out exchange)) {
								result = true;
						}
						return result;
				}

				public bool GetPage(out string characterResponse, int pageNumber, ref bool continues, ref int lastPageNumber)
				{
						bool result = false;
						characterResponse = string.Empty;

						if (LatestExchange == null) {
								return false;
						}

						string[] characterResponsePages = LatestExchange.CharacterResponse.Split(PageBreakStrings, StringSplitOptions.RemoveEmptyEntries);//TODO make this regex
						if (characterResponsePages.Length > pageNumber) {
								characterResponse = CleanDialog(characterResponsePages[pageNumber]);
								result = true;
								continues = (pageNumber < characterResponsePages.Length - 1);
						} else {
								continues = false;
						}
						lastPageNumber = characterResponsePages.Length - 1;
						return result;
				}

				public bool HasHadExchange(string exchangeName)
				{
						foreach (Exchange exchange in Exchanges) {
								if (exchange.Name == exchangeName && exchange.NumTimesChosen > 0) {
										return true;
								}
						}
						return false;
				}

				//TODO move this into GameData
				protected string WrapBracketedDialogInColor(string dialog)
				{
						string pattern = @"\[([^)]*)\]";
						MatchCollection matches = Regex.Matches(dialog, pattern);
						//redo this to follow how things are done in interpretscripts
						foreach (Match match in matches) {
								if (match.Groups.Count > 0) {
										string matchString = "[" + match.Groups[1].Value + "]";
										//keep matchstring for later
										string wrappedText = matchString.Replace("[", "(");
										wrappedText = wrappedText.Replace("]", ")");
										wrappedText = Colors.ColorWrap(wrappedText, Colors.Get.ConversationBracketedText, true);
										//wrappedText = "\n" + wrappedText + "\n";
										dialog = dialog.Replace(matchString, wrappedText);
								}
						}
						//do a final check for \n at the start
						if (dialog.EndsWith("\n")) {
								dialog = dialog.Substring(0, dialog.Length - 1);
						}
						if (dialog.StartsWith("\n")) {
								dialog = dialog.Substring(1);
						}
						return dialog;
				}

				public void ShowVariable(string variableName, bool show)
				{
						if (show && !DisplayVariables.Contains(variableName)) {
								DisplayVariables.Add(variableName);
						} else {
								DisplayVariables.Remove(variableName);
						}
				}

				public void GotoExchange(string exchangeName)
				{
						Exchange gotoExchange = null;
						if (mExchangeLookup.TryGetValue(exchangeName, out gotoExchange)) {

						}
				}

				protected void RepairNames(List <string> names)
				{
						for (int i = names.Count - 1; i > 0; i--) {
								string currentName = names[i];
								if (string.IsNullOrEmpty(currentName)) {
										names.RemoveAt(i);
								} else {
										string newName = string.Empty;
										if (mChangedNames.TryGetValue(name, out newName)) {
												names[i] = newName;
										}
								}
						}
				}

				public void MakeOutgoingChoice(Exchange nextExchange, out string dtsOnFailure)
				{
						MakeOutgoingChoice(nextExchange, false, out dtsOnFailure);
				}

				public void MakeOutgoingChoice(Exchange nextExchange, bool gotoOnly, out string dtsOnFailure)
				{
						dtsOnFailure = string.Empty;
						string latestExchangeName = string.Empty;
						if (LatestExchange != null) {
								//conclude the current exchange
								//because we're on our way to the next one
								latestExchangeName = LatestExchange.Name;
								LatestExchange.OnConcludeExchange();
						}

						//now that we've concluded we call on conclude
						foreach (ExchangeScript globalScript in State.GlobalScripts) {
								globalScript.OnConclude(LatestExchange.Name);
						}

						//set running outgoing choices
						bool addOutgoing = true;
						bool clearSiblngs = false;
						//choose the choice
						if (!gotoOnly) {//if we're not just doing a goto
								//send the choose message to the exchange
								nextExchange.Choose(LatestExchange);
								OnChooseExchange(nextExchange, latestExchangeName);
						} else {
								mRunningOutgoingChoices.Clear();
						}
						//get the next choices
						switch (nextExchange.OutgoingStyle) {
								case ExchangeOutgoingStyle.Normal:
								default:
										break;

								case ExchangeOutgoingStyle.SiblingsOff:
										clearSiblngs = true;
										break;

								case ExchangeOutgoingStyle.ManualOnly:
										//all existing options are cleared and only manual #on options are added
										mRunningOutgoingChoices.Clear();
										clearSiblngs = true;
										break;

								case ExchangeOutgoingStyle.Stop:
										addOutgoing = false;
										break;
						}

						HashSet <Exchange> finalOutgoingChoices = new HashSet <Exchange>();
						if (addOutgoing) {
								//add up all the various outgoing choices we've got
								//then we'll go through them one by one, figure out which ones are valid
								//and add it to the final outgoing choices array
								HashSet <Exchange> runningOutgoingChoices = new HashSet<Exchange>();
								foreach (Exchange runningOutgoingChoice in mRunningOutgoingChoices) {
										//if we haven't picked an option from earlier, let it tag along
										if (runningOutgoingChoice.NumTimesChosen < 1) {
												runningOutgoingChoices.Add(runningOutgoingChoice);
										}
								}

								foreach (Exchange alwaysInclude in mAlwaysInclude) {
										runningOutgoingChoices.Add(alwaysInclude);
								}

								foreach (Exchange outgoingChoice in runningOutgoingChoices) {
										if (!outgoingChoice.IsAvailable) {
										} else if (outgoingChoice == LastChosenExchange
										      || (clearSiblngs && AreSiblings(outgoingChoice, nextExchange))
										      || !outgoingChoice.RequirementsAreMet
										      || nextExchange.DisabledIncomingChoices.Contains(outgoingChoice.Name)) {
												if (!string.IsNullOrEmpty(outgoingChoice.DtsOnFailure)) {
														Debug.Log("Outgoing choice requirements are not met - dts on failure is " + outgoingChoice.DtsOnFailure);
														dtsOnFailure = outgoingChoice.DtsOnFailure;
												}
										} else {
												finalOutgoingChoices.Add(outgoingChoice);
										}
								}
								//finally, add any outgoing choices
								//these are considered 'on' manually so turning off siblings doesn't apply to them
								//only 'off' can disable them
								foreach (Exchange playerOutgoingChoice in LastChosenExchange.PlayerOutgoingChoices) {
										if (playerOutgoingChoice.IsAvailable && !nextExchange.DisabledIncomingChoices.Contains(playerOutgoingChoice.Name)) {
												finalOutgoingChoices.Add(playerOutgoingChoice);
										}
								}
						}
						mRunningOutgoingChoices = finalOutgoingChoices;
						LatestExchange = nextExchange;
				}

				public int NumTimesChosen(string exchangeName)
				{
						int numTimesChosen = 0;
						State.CompletedExchanges.TryGetValue(exchangeName, out numTimesChosen);
						return numTimesChosen;
				}

				public bool AreSiblings(Exchange e1, Exchange e2)
				{
						return e1.ParentExchange == e2.ParentExchange;
				}

				public void OnChooseExchange(Exchange exchange, string lastIncomingChoice)
				{
						LastChosenExchange = exchange;
						LatestExchange = exchange;
						int numTimesChosen = 0;
						if (!State.CompletedExchanges.TryGetValue(exchange.Name, out numTimesChosen)) {
								State.CompletedExchanges.Add(exchange.Name, 0);
						}
						numTimesChosen++;
						State.CompletedExchanges[exchange.Name] = numTimesChosen;
						//save before broadcasting so mission scripts etc. can access data
						//Save();
						//broadcast choice
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.NpcConverseExchange, WorldClock.AdjustedRealTime);

						//now that we've concluded we call on choose for global scripts
						for (int i = 0; i < State.GlobalScripts.Count; i++) {
								ExchangeScript globalScript = State.GlobalScripts[i];
								globalScript.OnChoose(lastIncomingChoice);
						}

				}

				public float GetVariableValueNormalized(string variableName)
				{
						float normalizedValue = 0.0f;
						SimpleVar stateVar;
						if (State.ConversationVariables.TryGetValue(variableName, out stateVar)) {
								normalizedValue = stateVar.NormalizedValue;
						}
						return normalizedValue;
				}

				public int GetVariableValue(string variableName)
				{
						int variableValue = 0;
						SimpleVar stateVar;
						if (State.ConversationVariables.TryGetValue(variableName, out stateVar)) {
								variableValue = stateVar.Value;
						}
						return variableValue;
				}

				public void SetVariableValue(string variableName, int variableValue)
				{
						if (State.ConversationVariables.ContainsKey(variableName)) {
								SimpleVar simpleVar = State.ConversationVariables[variableName];
								simpleVar.Value = variableValue;
								State.ConversationVariables[variableName] = simpleVar;
						}
				}

				public void IncrementVariable(string variableName)
				{
						int variableValue = 0;
						SimpleVar stateVar;
						if (State.ConversationVariables.TryGetValue(variableName, out stateVar)) {
								variableValue = stateVar.Value;
								variableValue++;
								SimpleVar simpleVar = State.ConversationVariables[variableName];
								simpleVar.Value = variableValue;
								State.ConversationVariables[variableName] = simpleVar;
								if (DisplayVariables.Contains(variableName)) {
										MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "SetVariableIncrement");
								}
						}
				}

				public void DecrementValue(string variableName)
				{
						int variableValue = 0;
						SimpleVar stateVar;
						if (State.ConversationVariables.TryGetValue(variableName, out stateVar)) {
								variableValue = stateVar.Value;
								variableValue--;
								stateVar.Value = variableValue;
								State.ConversationVariables[variableName] = stateVar;
								if (DisplayVariables.Contains(variableName)) {
										MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "SetVariableDecrement");
								}
						}
				}

				public void SetDisabled(string exchangeName, bool disabled)
				{
						if (disabled) {
								State.DisabledExchanges.Add(exchangeName);
						} else {
								State.DisabledExchanges.Remove(exchangeName);
						}
				}

				public bool IsDisabled(string exchangeName)
				{
						return State.DisabledExchanges.Contains(exchangeName);
				}

				public void Load(ConversationState state, string conversationName)
				{
						ConversationName = conversationName;
						ConversationProps props = null;
						if (Mods.Get.Runtime.LoadMod <ConversationProps>(ref props, "Conversation", ConversationName)) {
							State = state;
							Props = props;
						}
				}

				public void Save()
				{
						State.ListInAvailable = false;
						State.Name = Props.Name + "-State";
						Mods.Get.Runtime.SaveMod <ConversationState>(State, "Conversation", State.Name);
				}

				public void RefreshImmediately () {
						//at this point the convo will have been loaded
						mExchanges.Clear();
						mExchangeLookup.Clear();
						mAlwaysInclude.Clear();

						//add all the exchanges to the lookup
						mExchanges.Add(Props.DefaultOpeningExchange);
						Exchange exchange = null;
						for (int i = 0; i < Props.Exchanges.Count; i++) {
								exchange = Props.Exchanges[i];
								exchange.ParentConversation = this;
								mExchanges.SafeAdd(exchange);
						}
						//refresh the lookup table
						for (int i = 0; i < mExchanges.Count; i++) {
								exchange = mExchanges[i];
								mExchangeLookup.Add(exchange.Name, exchange);
								if (exchange.AlwaysInclude) {
										mAlwaysInclude.Add(exchange);
								}
						}
						Exchange parentExchange = null;
						//now that the lookup table is refreshed
						//set the owners and refresh each exchange
						for (int i = 0; i < mExchanges.Count; i++) {
								exchange = mExchanges[i];
								if (!string.IsNullOrEmpty(exchange.ParentExchangeName)) {
										mExchangeLookup.TryGetValue(exchange.ParentExchangeName, out parentExchange);
								}
								exchange.SetOwners(this, parentExchange);
								exchange.Refresh();
						}
				}

				#if UNITY_EDITOR
				public void EditorSave()
				{
						Mods.Get.Editor.InitializeEditor(true);
						State.ListInAvailable = false;
						Mods.Get.Editor.SaveMod <ConversationProps>(Props, "Conversation", Props.Name);
						Mods.Get.Editor.SaveMod <ConversationState>(State, "Conversation", Props.Name + "-State");
				}

				public void EditorLoad()
				{
						Mods.Get.Editor.InitializeEditor(true);
						ConversationState state = null;
						ConversationProps props = null;
						if (Mods.Get.Runtime.LoadMod <ConversationProps>(ref props, "Conversation", Props.Name)
						 &&	Mods.Get.Runtime.LoadMod <ConversationState>(ref state, "Conversation", Props.Name + "-State")) {
								Props = props;
								State = state;
						}
				}
				#endif
				protected bool mConcluded = false;
				[NonSerialized]
				protected List <Exchange> mExchanges;
				[NonSerialized]
				protected HashSet <Exchange> mAlwaysInclude;
				protected Dictionary <string, string> mChangedNames;
				protected string mUniqueNameJoin = ".";
				protected bool mInitiating = false;
				public static string[] PageBreakStrings = new string [] { "{PageBreak}", "{pagebreak}", "{Pagebreak}" };
				//TODO move this into GameData - do we even need it any more?
				public static string WrapText(string the_string, int width)
				{
						int pos, next;
						StringBuilder sb = new StringBuilder();
						string _newline = "\n";

						// Lucidity check
						if (width < 1)
								return the_string;

						// Parse each line of text
						for (pos = 0; pos < the_string.Length; pos = next) {
								// Find end of line
								int eol = the_string.IndexOf(_newline, pos);

								if (eol == -1)
										next = eol = the_string.Length;
								else
										next = eol + _newline.Length;

								// Copy this line of text, breaking into smaller lines as needed
								if (eol > pos) {
										do {
												int len = eol - pos;

												if (len > width)
														len = BreakLine(the_string, pos, width);

												sb.Append(the_string, pos, len);
												sb.Append(_newline);

												// Trim whitespace following break
												pos += len;

												while (pos < eol && Char.IsWhiteSpace(the_string[pos]))
														pos++;

										} while (eol > pos);
								} else
										sb.Append(_newline); // Empty line
						}

						return sb.ToString();
				}

				private static int BreakLine(string text, int pos, int max)
				{
						// Find last whitespace in line
						int i = max;
						while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
								i--;

						// If no whitespace found, break at maximum length
						if (i < 0)
								return max;

						// Find start of whitespace
						while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
								i--;

						// Return length of text before whitespace
						return i + 1;
				}
		}
}
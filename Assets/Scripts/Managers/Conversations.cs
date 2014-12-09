using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;
using Frontiers.Data;
using Frontiers.Story.Conversations;
using System;

namespace Frontiers
{
		public class Conversations : Manager
		{
				public static Conversations Get;
				public Conversation LocalConversation;
				public GameObject ConversationObject;

				public override void WakeUp()
				{
						Get = this;
				}

				public override void Initialize()
				{
						mInitialized = true;
						mLoadedSpeeches = new Dictionary <string, Speech>();
				}

				public void Reset()
				{
						LocalConversation.State = new ConversationState();
				}

				public static void ClearLog()
				{
						Mods.Get.Runtime.ResetProfileData("Conversation");
				}

				public void ClearLocalConversation()
				{
						if (LocalConversation != null) {
								LocalConversation.Props = new ConversationProps();
								LocalConversation.State = new ConversationState();
								GameObject.Destroy(LocalConversation);
						}
				}

				public void AddDTSOverride(string oldConversationName, string dtsConversation, string characterName)
				{
						ConversationState state = null;
						if (GetConversationState(oldConversationName, ref state)) {
								if (!state.DTSOverrides.ContainsKey(characterName)) {
										state.DTSOverrides.Add(characterName, dtsConversation);
								} else {
										state.DTSOverrides[characterName] = dtsConversation;
								}
								state.ListInAvailable = false;
								state.Name = oldConversationName + "-State";
								Mods.Get.Runtime.SaveMod <ConversationState>(state, "Conversation", state.Name);
						}
				}

				public void RemoveDTSOverride(string oldConversationName, string dtsConversation, string characterName)
				{
						ConversationState state = null;
						if (GetConversationState(oldConversationName, ref state)) {
								if (state.DTSOverrides.ContainsKey(characterName)) {
										if (state.DTSOverrides[characterName] == dtsConversation) {
												state.DTSOverrides.Remove(characterName);
										} else {
												Debug.Log("Current DTS didn't match " + dtsConversation + ", not removing");
										}
								} else {
										Debug.Log("No key found for character " + characterName);
								}
								state.ListInAvailable = false;
								state.Name = oldConversationName + "-State";
								Mods.Get.Runtime.SaveMod <ConversationState>(state, "Conversation", state.Name);
						}
				}

				public void AddSubstitution(string oldConversationName, string newConversationName, string characterName)
				{
						ConversationState state = null;
						if (GetConversationState(oldConversationName, ref state)) {
								if (!state.Substitutions.ContainsKey(characterName)) {
										state.Substitutions.Add(characterName, newConversationName);
								} else {
										state.Substitutions[characterName] = newConversationName;
								}
						}
				}

				public bool ConversationByName(string conversationName, string characterName, out Conversation conversation, out string DTSOverride)
				{
						DTSOverride = string.Empty;
						conversation = null;
						if (LocalConversation != null) {//get rid of the existing conversation if it's not already destroyed
								LocalConversation.Props = new ConversationProps();
								LocalConversation.State = new ConversationState();
								GameObject.Destroy(LocalConversation);
						}
						bool foundConversation = false;
						int numSubstitutions = 0;
						int maxSubstitutions = 100;
						bool finishedSubstituting = false;
						ConversationState state = null;

						while (!finishedSubstituting) {
								if (GetConversationState(conversationName, ref state)) {	//get the requested state
										if (state.DTSOverrides.ContainsKey(characterName)) {
												DTSOverride = state.DTSOverrides[characterName];
												//that's all we need - DTS overrides break all
												return false;
										}

										if (state.Substitutions.ContainsKey(characterName)) {	//if the conversation state contains a substitution for this character
												//set the conversation name to that substitution
												conversationName = state.Substitutions[characterName];
												numSubstitutions++;
										} else {//if it doesn't have a substitution then we're finished
												finishedSubstituting = true;
												foundConversation = true;
												LocalConversation = ConversationObject.AddComponent <Conversation>();
												LocalConversation.Props.Name = conversationName;
												LocalConversation.Load(state);
												conversation = LocalConversation;
										}
								} else {	//damn didn't find the substituted conversation
										finishedSubstituting = true;
								}

								if (numSubstitutions > maxSubstitutions) {	//if we're over our limit, bail
										finishedSubstituting = true;
								}
						}
						return foundConversation;
				}

				public string ExchangeNameFromIndex(string conversationName, int exchangeIndex)
				{
						string exchangeName = string.Empty;
						ConversationState state = null;
						if (GetConversationState(conversationName, ref state)) {
								Exchange exchange = null;
								state.ExchangeNames.TryGetValue(exchangeIndex, out exchangeName);
						}
						return exchangeName;
				}

				public int NumTimesInitiated(string conversationName)
				{
						ConversationState state = null;
						int numTimesInitiated = 0;
						if (GetConversationState(conversationName, ref state)) {
								numTimesInitiated = state.NumTimesInitiated;
						}
						return numTimesInitiated;
				}

				public bool HasCompletedExchange(string conversationName, string exchangeName)
				{
						bool result = false;
			
						ConversationState state = null;
						if (GetConversationState(conversationName, ref state)) {
								int numTimes = 0;
								if (state.CompletedExchanges.TryGetValue(exchangeName, out numTimes)) {
										result = true;
								}			
						}
			
						return result;
				}

				public bool HasCompletedExchange(string conversationName, string exchangeName, out int numTimes)
				{
						bool result = false;
						numTimes = 0;
						ConversationState state = null;
						if (GetConversationState(conversationName, ref state)) {
								if (state.CompletedExchanges.TryGetValue(exchangeName, out numTimes)) {
										result = true;
								}			
						}

						return result;
				}

				public void SaveSpeech(string speechName)
				{
						Speech speech = null;
						if (mLoadedSpeeches.TryGetValue(speechName, out speech)) {
								Mods.Get.Runtime.SaveMod(speech, "Speech", speech.Name);
								if (speech.NumUsers <= 0) {	//if nobody's giving the speech any more, unload it
										mLoadedSpeeches.Remove(speechName);
								}
						}
				}

				public bool GetOrLoadSpeech(string speechName, out Speech speech)
				{
						bool result = false;
						if (mLoadedSpeeches.TryGetValue(speechName, out speech)) {	//if it's loaded, we're good
								result = true;
						} else if (Mods.Get.Runtime.LoadMod(ref speech, "Speech", speechName)) {	//if it hasn't been loaded, get it from mods and add it to lookup
								mLoadedSpeeches.Add(speechName, speech);
								result = true;
						}
						return result;
				}

				protected bool GetConversationState(string conversationName, ref ConversationState state)
				{
						state = null;
						if (LocalConversation != null && LocalConversation.Props.Name == conversationName) {	//first check if it's the one we already have loaded
								state = LocalConversation.State;
								return true;
						} else {	//otherwise load the data from disk and search
								if (Mods.Get.Runtime.LoadMod <ConversationState>(ref state, "Conversation", conversationName + "-State")) {
										state.ListInAvailable = false;
										return true;
								}
						}
						return false;
				}

				protected Dictionary <string,Speech> mLoadedSpeeches;
// = new Dictionary <string, Speech> ();
		}
}

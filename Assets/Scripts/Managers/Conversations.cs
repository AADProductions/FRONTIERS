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

		public override void WakeUp ()
		{
			base.WakeUp ();

			Get = this;
		}

		public override void Initialize ()
		{
			mInitialized = true;
			mLoadedSpeeches = new Dictionary <string, Speech> ();
			mLoadedConversations = new Dictionary<string, ConversationState> ();
			Player.Get.AvatarActions.Subscribe (AvatarAction.NpcConverseEnd, new ActionListener (NpcConverseEnd));
		}

		public bool NpcConverseEnd (double timeStamp)
		{
			if (LocalConversation != null) {
				GameObject.Destroy (LocalConversation, 0.1f);
			}
			return true;
		}

		public void Reset ()
		{
			LocalConversation.State = new ConversationState ();
		}

		public static void ClearLog ()
		{
			Mods.Get.Runtime.ResetProfileData ("Conversation");
		}

		public void ClearLocalConversation ()
		{
			if (LocalConversation != null) {
				GameObject.Destroy (LocalConversation);
			}
		}

		public void AddDTSOverride (string oldConversationName, string dtsConversation, string characterName)
		{
			ConversationState state = null;
			if (GetConversationState (oldConversationName, ref state)) {
				//Debug.Log("Adding DTS override " + dtsConversation + " for character " + characterName);
				if (!state.DTSOverrides.ContainsKey (characterName)) {
					state.DTSOverrides.Add (characterName, dtsConversation);
				} else {
					state.DTSOverrides [characterName] = dtsConversation;
				}
				state.ListInAvailable = false;
				state.Name = oldConversationName + "-State";
				Mods.Get.Runtime.SaveMod <ConversationState> (state, "Conversation", state.Name);
			}
		}

		public void RemoveDTSOverride (string oldConversationName, string dtsConversation, string characterName)
		{
			ConversationState state = null;
			if (GetConversationState (oldConversationName, ref state)) {
				if (state.DTSOverrides.ContainsKey (characterName)) {
					if (state.DTSOverrides [characterName] == dtsConversation) {
						Debug.Log ("Removed DTS override " + dtsConversation + " for " + characterName);
						state.DTSOverrides.Remove (characterName);
					} else {
						Debug.Log ("Current DTS " + state.DTSOverrides [characterName] + " didn't match " + dtsConversation + ", not removing");
					}
				} else {
					Debug.Log ("No key found for character " + characterName);
				}
				state.ListInAvailable = false;
				state.Name = oldConversationName + "-State";
				Mods.Get.Runtime.SaveMod <ConversationState> (state, "Conversation", state.Name);
			} else {
				Debug.Log ("Couldn't find conversation " + oldConversationName + " in remove dts override");
			}
		}

		public void AddSubstitution (string oldConversationName, string newConversationName, string characterName)
		{
			ConversationState state = null;
			if (GetConversationState (oldConversationName, ref state)) {
				if (!state.Substitutions.ContainsKey (characterName)) {
					state.Substitutions.Add (characterName, newConversationName);
				} else {
					state.Substitutions [characterName] = newConversationName;
				}
			}
		}

		public bool ConversationByName (string conversationName, string characterName, out Conversation conversation, out string DTSOverride)
		{
			DTSOverride = string.Empty;
			conversation = null;
			if (LocalConversation != null) {//get rid of the existing conversation if it's not already destroyed
				LocalConversation.Props = null;
				LocalConversation.State = null;
				GameObject.Destroy (LocalConversation);
			}
			bool foundConversation = false;
			int numSubstitutions = 0;
			int maxSubstitutions = 100;
			bool finishedSubstituting = false;
			ConversationState state = null;

			#if UNITY_EDITOR
			Debug.Log ("Looking for conversation " + conversationName + " by character " + characterName);
			#endif

			while (!finishedSubstituting) {
				if (GetConversationState (conversationName, ref state)) {	//get the requested state
					if (state.DTSOverrides.ContainsKey (characterName)) {
						DTSOverride = state.DTSOverrides [characterName];
						//that's all we need - DTS overrides break all
						#if UNITY_EDITOR
						Debug.Log ("We have a DTS override in conversation state " + state.Name);
						#endif
						return false;
					} else {
						#if UNITY_EDITOR
						Debug.Log ("Found no DTS override for " + characterName);
						#endif
					}

					if (state.Substitutions.ContainsKey (characterName)) {	//if the conversation state contains a substitution for this character
						//set the conversation name to that substitution
						conversationName = state.Substitutions [characterName];
						#if UNITY_EDITOR
						Debug.Log ("We have a substitute in " + state.Name);
						#endif
						numSubstitutions++;
					} else {//if it doesn't have a substitution then we're finished
						#if UNITY_EDITOR
						Debug.Log ("Found conversation");
						#endif
						finishedSubstituting = true;
						foundConversation = true;
						LocalConversation = ConversationObject.AddComponent <Conversation> ();
						LocalConversation.Load (state, conversationName);
						conversation = LocalConversation;
					}
				} else {	//damn didn't find the substituted conversation
					#if UNITY_EDITOR
					Debug.Log ("Didn't find substituted conversation");
					#endif
					finishedSubstituting = true;
				}

				if (numSubstitutions > maxSubstitutions) {	//if we're over our limit, bail
					#if UNITY_EDITOR
					Debug.Log ("Max substitutions, bailing");
					#endif
					finishedSubstituting = true;
				}
			}
			return foundConversation;
		}

		public string ExchangeNameFromIndex (string conversationName, int exchangeIndex)
		{
			string exchangeName = string.Empty;
			ConversationState state = null;
			if (GetConversationState (conversationName, ref state)) {
				Exchange exchange = null;
				state.ExchangeNames.TryGetValue (exchangeIndex, out exchangeName);
			}
			return exchangeName;
		}

		public int NumTimesInitiated (string conversationName)
		{
			ConversationState state = null;
			int numTimesInitiated = 0;
			if (GetConversationState (conversationName, ref state)) {
				numTimesInitiated = state.NumTimesInitiated;
			}
			return numTimesInitiated;
		}

		public bool HasCompletedExchange (string conversationName, string exchangeName, bool requireConversationInitiated)
		{
			bool result = false;
			
			ConversationState state = null;
			if (GetConversationState (conversationName, ref state)) {
				if (requireConversationInitiated && state.NumTimesInitiated <= 0) {
					#if DEBUG_CONVOS
					Debug.Log ("---- Hasn't initiated conversation " + conversationName + " so returning true for " + exchangeName);
					#endif
					result = true;
				} else {
					int numTimes = 0;
					if (state.CompletedExchanges.TryGetValue (exchangeName, out numTimes)) {
						#if DEBUG_CONVOS
						Debug.Log ("---- HAS completed exchange " + conversationName + " " + exchangeName);
						#endif
						result = numTimes > 0;
					}
					#if DEBUG_CONVOS
					else { Debug.Log ("---- HAS NOT completed exchange " + conversationName + " " + exchangeName); }
					#endif
				}
			}
			#if DEBUG_CONVOS
			else { Debug.Log ("---- Couldn't get conversation state " + conversationName); }
			#endif
			
			return result;
		}

		public bool HasCompletedExchange (string conversationName, string exchangeName, bool requireConversationInitiated, out int numTimes)
		{
			bool result = false;
			numTimes = 0;
			ConversationState state = null;
			if (GetConversationState (conversationName, ref state)) {
				if (requireConversationInitiated && state.NumTimesInitiated <= 0) {
					#if DEBUG_CONVOS
					Debug.Log ("---- Hasn't initiated conversation " + conversationName + " so returning true for " + exchangeName);
					#endif
					result = true;
				} else if (state.CompletedExchanges.TryGetValue (exchangeName, out numTimes)) {
					#if DEBUG_CONVOS
					Debug.Log ("---- HAS completed exchange " + conversationName + " " + exchangeName);
					#endif
					result = true;
				}			
			}
			#if DEBUG_CONVOS
			else { Debug.Log ("---- Couldn't get conversation state " + conversationName); }
			#endif

			return result;
		}

		public void SaveSpeech (string speechName)
		{
			Speech speech = null;
			if (mLoadedSpeeches.TryGetValue (speechName, out speech)) {
				Mods.Get.Runtime.SaveMod (speech, "Speech", speech.Name);
				if (speech.NumUsers <= 0) {	//if nobody's giving the speech any more, unload it
					mLoadedSpeeches.Remove (speechName);
				}
			}
		}

		public override void OnGameSave ()
		{
			foreach (ConversationState state in mLoadedConversations.Values) {
				Mods.Get.Runtime.SaveMod <ConversationState> (state, "Conversation", state.Name);
			}
			if (LocalConversation != null) {
				GameObject.Destroy (LocalConversation);
			}
			mLoadedConversations.Clear ();
			mGameSaved = true;
		}

		public bool GetOrLoadSpeech (string speechName, out Speech speech)
		{
			bool result = false;
			if (mLoadedSpeeches.TryGetValue (speechName, out speech)) {	//if it's loaded, we're good
				result = true;
			} else if (Mods.Get.Runtime.LoadMod (ref speech, "Speech", speechName)) {	//if it hasn't been loaded, get it from mods and add it to lookup
				mLoadedSpeeches.Add (speechName, speech);
				result = true;
			}
			return result;
		}

		protected bool GetConversationState (string conversationName, ref ConversationState state)
		{
			state = null;
			if (mLoadedConversations.TryGetValue (conversationName, out state)) {//first check if it's the one we already have loaded
				return true;
			} else {
				if (Mods.Get.Runtime.LoadMod <ConversationState> (ref state, "Conversation", conversationName + "-State")) {
					state.ListInAvailable = false;
					mLoadedConversations.Add (conversationName, state);
					return true;
				}
			}
			return false;
		}

		protected Dictionary <string,ConversationState> mLoadedConversations;
		protected Dictionary <string,Speech> mLoadedSpeeches;
	}
}

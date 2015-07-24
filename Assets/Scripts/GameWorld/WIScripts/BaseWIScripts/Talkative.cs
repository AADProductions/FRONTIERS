using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Story;
using Frontiers.World.Gameplay;
using Frontiers.GUI;
using Frontiers.Story.Conversations;

namespace Frontiers.World.WIScripts
{
	public class Talkative : WIScript
	{
		Character character;
		public TalkativeState State = new TalkativeState ();

		public override void OnInitialized ()
		{
			character = worlditem.Get<Character> ();
			worlditem.OnPlayerEncounter += OnPlayerEncounter;
			worlditem.OnPlayerUse += OnPlayerUse;

			if (string.IsNullOrEmpty (State.DTSSpeechName)) {
				State.DTSOnPlayerEncounter = true;
			}
		}

		public void OnPlayerUse ()
		{
			if (character.IsDead || character.IsStunned || character.IsSleeping) {
				return;
			}

			if (!mInitiatingConversation) {
				mInitiatingConversation = true;
				StartCoroutine (InitiateConversation ());
			}
		}

		public void OnPlayerEncounter ()
		{
			if (character.IsDead || character.IsStunned || character.IsSleeping) {
				return;
			}

			if (State.DTSOnPlayerEncounter && !string.IsNullOrEmpty (State.DTSSpeechName)) {
				Motile motile = null;
				if (worlditem.Is <Motile> (out motile)) {//get the listener target = use focus object
					//send a motile action to keep the character in place
					//the FocusObject will be moved around by the speech bubble each page
					if (mSpeechMotileAction == null) {
						mSpeechMotileAction = new MotileAction ();
					}
					mSpeechMotileAction.Reset ();
					mSpeechMotileAction.Type = MotileActionType.FocusOnTarget;
					mSpeechMotileAction.Expiration = MotileExpiration.Never;
					mSpeechMotileAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
					mSpeechMotileAction.IdleAnimation = GameWorld.Get.FlagByName ("IdleAnimation", "Talking");
					mSpeechMotileAction.LiveTarget = Player.Local;
					//we want normal because we want to reach the action node first
					motile.PushMotileAction (mSpeechMotileAction, MotileActionPriority.Next);
				}
				GiveSpeech (State.DTSSpeechName, null);
			}
		}

		public override void PopulateOptionsList (System.Collections.Generic.List<WIListOption> options, List <string> message)
		{
			if (character.IsDead || character.IsStunned || character.IsSleeping) {
				return;
			}

			if (!string.IsNullOrEmpty (State.DTSSpeechName) || !string.IsNullOrEmpty (State.ConversationName)) {
				WIListOption talkOption = new WIListOption ("Talk");
				if (State.GivingSpeech && !mSpeech.speech.CanBeInterrupted) {
					talkOption.Disabled = true;
				}
				options.Add (talkOption);
			}
		}

		public void OnPlayerUseWorldItemSecondary (object dialogResult)
		{
			WIListResult result = (WIListResult)dialogResult;

			switch (result.SecondaryResult) {
			case "Talk":
				if (!worlditem.HasPlayerAttention) {
					Player.Local.Focus.GetOrReleaseAttention (worlditem);
				}
				if (!mInitiatingConversation) {
					mInitiatingConversation = true;
					StartCoroutine (InitiateConversation ());
				}
				break;

			default:
				break;
			}
		}

		public void ForceConversation ()
		{
			mInitiatingConversation = true;
			Character character = null;
			if (worlditem.Is <Character> (out character)) {
				mTalkMotileAction = character.LookAtPlayer ();
			}
			Conversation conversation = null;
			string DTSOverride = string.Empty;
			if (Conversations.Get.ConversationByName (State.ConversationName, worlditem.FileName, out conversation, out DTSOverride)) {
				State.ConversationName = conversation.Props.Name;
				conversation.Initiate (character, this);
			} else if (!string.IsNullOrEmpty (DTSOverride)) {
				State.DTSSpeechName = DTSOverride;
				mInitiatingConversation	= false;
				Speech speech = null;
				if (Mods.Get.Runtime.LoadMod (ref speech, "Speech", State.DTSSpeechName)) {
					SayDTS (speech);
				}
			}
		}

		public IEnumerator InitiateConversation ()
		{
			mInitiatingConversation = true;
			if (!worlditem.HasPlayerAttention) {
				//Debug.Log("We don't have player's attention");
				mInitiatingConversation = false;
				yield break;
			}
			//make the character stand still
			Character character = null;
			if (worlditem.Is <Character> (out character)) {
				mTalkMotileAction = character.LookAtPlayer ();
			}

			yield return StartCoroutine (mTalkMotileAction.WaitForActionToStart (0f));

			if (mTalkMotileAction.IsFinished) {
				//Debug.Log("Talk motile action got finished for some reason, quitting");
				mInitiatingConversation = false;
				yield break;
			}

			if (State.DefaultToDTS) {
				Speech speech = null;
				mInitiatingConversation = false;
				if (Mods.Get.Runtime.LoadMod (ref speech, "Speech", State.DTSSpeechName)) {
					SayDTS (speech);
				}
				//Debug.Log("Defaulting to DTS");
				mInitiatingConversation = false;
				yield break;
			}
			yield return null;
			Conversation conversation = null;
			string DTSOverride = string.Empty;
			if (Conversations.Get.ConversationByName (State.ConversationName, worlditem.FileName, out conversation, out DTSOverride)) {
				//wuhoo we got the conversation
				State.ConversationName = conversation.Props.Name;
				conversation.Initiate (character, this);
				while (conversation.Initiating) {
					yield return null;
				}
				//this will load the conversation without hitches
				//now we may just have a dts - in which case the conversation won't be in progress
				if (!Conversation.ConversationInProgress) {
					//Debug.Log("Conversation not in progress, must have defaulted to DTS");
					mInitiatingConversation = false;
					yield break;
				}
			} else if (!string.IsNullOrEmpty (DTSOverride)) {
				//Debug.Log("We have a dts override: " + DTSOverride);
				//whoa we have a DTS override
				State.DTSSpeechName = DTSOverride;
				mInitiatingConversation	= false;
				Speech speech = null;
				if (Mods.Get.Runtime.LoadMod (ref speech, "Speech", State.DTSSpeechName)) {
					SayDTS (speech);
				}
				mInitiatingConversation = false;
				yield break;
			} else {
				//Debug.Log("Couldn't get the conversation, canceling");
				//aw shit we never got the conversation
				mInitiatingConversation = false;
				yield break;
			}

			yield return null;
			while (conversation.IsActive) {
				//wait for the player to end the conversation
				yield return null;
			}
			mInitiatingConversation = false;
			yield break;
		}

		public void EndConversation ()
		{
			if (GameManager.Get.TestingEnvironment)
				return;

			if (mTalkMotileAction.State != MotileActionState.Finished) {
				mTalkMotileAction.TryToFinish ();
			}
		}

		public Speech CurrentSpeech {
			get {
				return mSpeech.speech;
			}
		}

		public void TryToInterruptSpeech ()
		{
			if (State.GivingSpeech) {
				mInterruptionRequest = true;
			}
		}

		public void SayDTS (string speechName)
		{
			Speech speech = null;
			if (Mods.Get.Runtime.LoadMod (ref speech, "Speech", Frontiers.Data.DataImporter.GetNameFromDialogName (speechName))) {
				SayDTS (speech);
			}
		}

		public void SayDTS (Speech speech)
		{
			if (character.IsDead || character.IsStunned || character.IsSleeping) {
				return;
			}

			if (!mSayingDTS) {	
				//Debug.Log("Saying DTS " + speech.Name);
				mDTS = speech;
				StartCoroutine (SayDTSOverTime ());
			}
		}

		public void GiveSpeech (string speechName, ActionNode dispatcher)
		{
			Speech speech = null;
			if (Mods.Get.Runtime.LoadMod <Speech> (ref speech, "Speech", speechName)) {
				GiveSpeech (speech, dispatcher);
			} else {
				Debug.Log ("Couldn't load speech " + speechName);
			}
		}

		public void GiveSpeech (Speech speech, ActionNode dispatcher)
		{
			if (character.IsDead || character.IsStunned || character.IsSleeping) {
				return;
			}

			if (speech == null) {
				Debug.Log ("Speech was null, returning");
			}

			if (mSpeech != null && speech.Name == mSpeech.speech.Name) {//we already have it
				//Debug.Log ("Already giving speech");
				return;
			}

			foreach (DispatchedSpeech existingSpeech in mNewSpeeches) {
				if (speech.Name == existingSpeech.speech.Name) {//we already have it
					return;
				}
			}

			mNewSpeeches.Enqueue (new DispatchedSpeech (dispatcher, speech));
			if (!mGivingSpeeches) {
				StartCoroutine (GiveSpeechesOverTime ());
			}
		}

		protected IEnumerator StartSpeech (DispatchedSpeech speech)
		{
			if (speech.speech == null) {
				Debug.Log ("Speech is null, breaking");
			}

			State.LastSpeechName = speech.speech.Name;
			State.LastSpeechStarted = WorldClock.AdjustedRealTime;
			State.LastSpeechPage	= -1;

			speech.speech.StartSpeech (worlditem.FileName);
			Mods.Get.Runtime.SaveMod <Speech> (speech.speech, "Speech", speech.speech.Name);
			//send motile action
			Transform listenerTarget = null;
			Motile motile = null;
			Debug.Log ("Giving speech over time in character...");
			if (worlditem.Is <Motile> (out motile)) {//get the listener target = use focus object
				listenerTarget = motile.GoalObject;
				//send a motile action to keep the character in place
				//the FocusObject will be moved around by the speech bubble each page
				if (mSpeechMotileAction == null) {
					mSpeechMotileAction = new MotileAction ();
				}
				mSpeechMotileAction.Reset ();
				mSpeechMotileAction.Type = MotileActionType.FocusOnTarget;
				mSpeechMotileAction.Expiration = MotileExpiration.Never;
				if (speech.speech.CanBeInterrupted) {	//if we can be interrupted, let the speech cut off
					mSpeechMotileAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
				} else {	//if it can't be interrupted, do not yield to other actions
					mSpeechMotileAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
				}
				mSpeechMotileAction.IdleAnimation = GameWorld.Get.FlagByName ("IdleAnimation", "Talking");
				//we want normal because we want to reach the action node first
				motile.PushMotileAction (mSpeechMotileAction, MotileActionPriority.Next);
			} else {
				listenerTarget = worlditem.tr;
			}

			//create speech bubble
			mSpeechBubble = CreateSpeechBubble (speech.speech, speech.dispatcher, listenerTarget);
			yield break;
		}

		protected SpeechBubble CreateSpeechBubble (Speech speech, ActionNode dispatcher, Transform listenerTarget)
		{
			GameObject speechBubbleGameObject = gameObject.CreateChild ("SpeechBubble " + speech.Name).gameObject;
			SpeechBubble speechBubble = speechBubbleGameObject.AddComponent <SpeechBubble> ();
			speechBubble.ParentSpeech = speech;
			speechBubble.Dispatcher = dispatcher;
			speechBubble.Speaker = this;
			speechBubble.ListenerTarget = listenerTarget;
			return speechBubble;
		}

		protected IEnumerator FinishSpeech (DispatchedSpeech speech)
		{
			if (speech.speech == null) {
				Debug.Log ("Speech was null, breaking without finishing");
				yield break;
			}

			State.LastSpeechFinished = WorldClock.AdjustedRealTime;
			//update the speech data and save it to disk
			//note: this might be a problem since multiple NPCs will be giving speeches...
			//the data could get messed up
			if (mSpeechMotileAction != null) {	//try to finish it manually
				mSpeechMotileAction.TryToFinish ();
			}
			speech.speech.FinishSpeech (worlditem.FileName);
			mSpeechBubble.FinishSpeech ();
			if (speech.dispatcher != null) {
				speech.dispatcher.OnFinishSpeech ();
			}
			Mods.Get.Runtime.SaveMod <Speech> (speech.speech, "Speech", speech.speech.Name);
			//if the speech has messages and we have listeners, send message now
			mSpeechBubble = null;
			//if we still have our saved motile action
			if (!string.IsNullOrEmpty (speech.speech.OnFinishMessage)) {
				if (!string.IsNullOrEmpty (speech.speech.OnFinishMessageParam)) {
					gameObject.SendMessage (speech.speech.OnFinishMessage, speech.speech.OnFinishMessageParam, SendMessageOptions.DontRequireReceiver);
				} else {
					gameObject.SendMessage (speech.speech.OnFinishMessage, SendMessageOptions.DontRequireReceiver);
				}
			}
			yield break;
		}

		protected IEnumerator InterruptSpeech (DispatchedSpeech speech)
		{
			State.LastSpeechInterrupted = WorldClock.AdjustedRealTime;
			//update the speech data and save it to disk
			//note: this might be a problem since multiple NPCs will be giving speeches...
			//the data could get messed up
			speech.speech.InterruptSpeech (worlditem.FileName);
			mSpeechBubble.InterruptSpeech ();
			Mods.Get.Runtime.SaveMod <Speech> (speech.speech, "Speech", speech.speech.Name);
			//if the speech has messages and we have listeners, send message now
			mSpeechBubble = null;
			yield break;
		}

		protected IEnumerator GiveSpeechesOverTime ()
		{
			mGivingSpeeches	= true;
			while (mNewSpeeches.Count > 0 || mSpeech != null) {	//if we already have a speech and it's not finished
				if (mSpeech == null) {
					mSpeech = mNewSpeeches.Dequeue ();
					yield return StartCoroutine (StartSpeech (mSpeech));
				}
				if (mSpeech.speech == null) {
					Debug.Log ("Dispatched speech was null");
					yield break;
				}
				//give this speech
				State.GivingSpeech = true;
				State.LastSpeechPage = -1;
				string pageText = string.Empty;
				float pageDuration = 0.0f;
				while (State.GivingSpeech) {
					if (mInterruptionRequest) {
						if (mSpeech.speech.CanBeInterrupted) {	//interrupt this speech and clear the rest
							//speeches will all stop after the next loop
							yield return StartCoroutine (InterruptSpeech (mSpeech));
							mNewSpeeches.Clear ();
						}
						mInterruptionRequest = false;
					} else {//getting next page of speech
						bool isFinished = mSpeech.speech.GetPage (ref pageText, ref pageDuration, ref State.LastSpeechPage, true);
						if (State.LastSpeechPage >= 0) {
							//Debug.Log ("Putting speech on page");
							if (Player.Local.Surroundings.IsSoundAudible (worlditem.tr.position, mSpeech.speech.AudibleRange)) {
								//player can hear speech, put it on screen
								//Debug.Log ("Speech is audible");
								Motile motile = worlditem.Get<Motile> ();
								FXManager.Get.SpawnFX (motile.Body.Transforms.HeadTop.gameObject, "SpeakEffect", Vector3.zero);
								NGUIScreenDialog.AddSpeech (pageText, worlditem.DisplayName, pageDuration);
							}
							//TEMP - implement listener target on pages
							mSpeechBubble.NextPage ("[Random]");
							double waitUntil = Frontiers.WorldClock.AdjustedRealTime + pageDuration;
							while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
							}
						}

						if (isFinished) {
							//Debug.Log ("Finished");
							var finishSpeech = FinishSpeech (mSpeech);
							while (finishSpeech.MoveNext ()) {
								yield return finishSpeech.Current;
							}
							mSpeech = null;
							State.GivingSpeech	= false;
						}
					}
					//we're done giving the speech
				}
			}
			mGivingSpeeches	= false;
			yield break;
		}

		protected IEnumerator SayDTSOverTime ()
		{
			//spawn an effect over the characters' head
			Motile motile = worlditem.Get<Motile> ();
			FXManager.Get.SpawnFX (motile.Body.Transforms.HeadTop.gameObject, "SpeakEffect", Vector3.zero);

			mSayingDTS = true;
			mLastDTSPage = -1;
			string pageText = string.Empty;
			float pageDuration	= 0.0f;
			bool continueDTS = true;
			if (!string.IsNullOrEmpty (mDTS.OnFinishCommand)) {
				mSpeechBubble = CreateSpeechBubble (mDTS, null, worlditem.tr);
			}
			yield return null;
			while (continueDTS) {
				continueDTS = mDTS.GetPage (ref pageText, ref pageDuration, ref mLastDTSPage, true);
				if (continueDTS) {
					NGUIScreenDialog.AddSpeech (pageText, worlditem.DisplayName, pageDuration);
					double waitUntil = Frontiers.WorldClock.AdjustedRealTime + pageDuration;
					while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
						yield return null;
					}
				}
			}
			if (mSpeechBubble != null) {
				mSpeechBubble.FinishSpeech ();
			}
			if (!string.IsNullOrEmpty (mDTS.OnFinishMissionActivate)) {
				if (!string.IsNullOrEmpty (mDTS.OnFinishObjectiveActivate)) {
					Missions.Get.ActivateObjective (mDTS.OnFinishMissionActivate, mDTS.OnFinishObjectiveActivate, MissionOriginType.Character, worlditem.FileName);
				} else {
					Missions.Get.ActivateMission (mDTS.OnFinishMissionActivate, MissionOriginType.Character, worlditem.FileName);
				}
			}
			mDTS = null;
			mSayingDTS	= false;
			yield break;
		}

		protected bool mInterruptionRequest = false;
		protected bool mGivingSpeeches = false;
		protected bool mInitiatingConversation = false;
		protected bool mSayingDTS = false;
		protected int mLastDTSPage = 0;
		protected MotileAction mSpeechMotileAction = null;
		protected MotileAction mTalkMotileAction = null;
		protected SpeechBubble mSpeechBubble = null;
		protected DispatchedSpeech mSpeech = null;
		protected Speech mDTS = null;
		protected Queue <DispatchedSpeech> mNewSpeeches = new Queue <DispatchedSpeech> ();
		//TODO make this a struct
		protected class DispatchedSpeech
		{
			public DispatchedSpeech (ActionNode d, Speech s)
			{
				dispatcher = d;
				speech = s;
			}

			public ActionNode dispatcher;
			public Speech speech;
		}
	}

	[Serializable]
	public class TalkativeState
	{
		public string ConversationName = string.Empty;
		public bool DefaultToDTS = false;
		public string DTSSpeechName = string.Empty;
		public bool DTSOnPlayerEncounter = false;
		public bool GivingSpeech = false;
		public string LastSpeechName = string.Empty;
		public int LastSpeechPage = 0;
		public double LastSpeechStarted = 0.0f;
		public double LastSpeechInterrupted	= 0.0f;
		public double LastSpeechFinished = 0.0f;
	}
}
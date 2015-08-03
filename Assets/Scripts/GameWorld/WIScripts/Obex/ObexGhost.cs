using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.Story.Conversations;

namespace Frontiers.World.WIScripts {
	public class ObexGhost : WIScript, IConversationIntermediary {

		public Transform CharacterBase;
		public Transform LookTarget;
		public Transform ActiveFX;
		public Light ActiveLight;
		public MasterAudio.SoundType SoundType = MasterAudio.SoundType.Obex;
		public string SoundOnActivated;
		public string SoundOnDeactivated;
		public string[] GlitchSounds = new string[] { "Static1", "Static2", "Static3" };
		public float PulseSpeed = 2f;
		public bool Activated = false;
		public float Glitch = 0f;
		public Vector3 ActiveFXOnPosition;

		public ObexGhostState State = new ObexGhostState ();

		public Character SpeakingCharacter {
			get {
				return mSpeakingCharacter;
			}
		}

		public Transform CameraTarget {
			get {
				return LookTarget;
			}
		}

		public override bool UnloadWhenStacked {
			get {
				return false;
			}
		}

		public override void OnInitialized ()
		{
			if (State.HasNewMessage) {
				ActivateGhost ();	 
			} else {
				DeactivateGhost ();
			}
			Conversation.OnExchangeChosen += OnExchangeChosen;
		}

		public override void PopulateOptionsList (List<WIListOption> options, List<string> message)
		{
			if (mActivating | mDeactivating)
				return;

			if (Activated) {
				options.Add (new WIListOption ("Deactivate"));
			} else {
				options.Add (new WIListOption ("Activate"));
			}
		}

		public void OnPlayerUseWorldItemSecondary (object secondaryResult)
		{
			WIListResult dialogResult = secondaryResult as WIListResult;			
			switch (dialogResult.SecondaryResult) {
			case "Activate":
				ActivateGhost ();
				break;

			case "Deactivate":
				DeactivateGhost ();
				break;

			default:
				break;
			}
		}

		public void FinishConversation () {
			DeactivateGhost ();
		}

		public void ActivateGhost () {

			if (mActivating | mDeactivating)
				return;

			Activated = true;

			if (mSpeakingCharacter == null || mSpeakingCharacter.IsDestroyed) {
				if (mCharacterSpawnPoint == null) {
					WorldChunk c = worlditem.Group.GetParentChunk ();
					ActionNodeState actionNodeState = null;
					string nodeName = worlditem.FileName + "GhostNode";
					if (!c.GetOrCreateNode (worlditem.Group, worlditem.tr, worlditem.FileName + "GhostNode", out actionNodeState)) {
						Debug.LogError ("Couldn't create spawn node " + nodeName + " in ghost to create character");
						return;
					}
					actionNodeState.actionNode.transform.position = worlditem.tr.position;
					actionNodeState.CustomConversation = State.CharacterConversation;
					if (!Characters.GetOrSpawnCharacter (actionNodeState.actionNode, State.CharacterName, worlditem.Group, out mSpeakingCharacter)) {
						Debug.LogError ("Couldn't spawn character " + State.CharacterName + " in ghost");
					} else {
						mSpeakingCharacter.worlditem.ActiveState = WIActiveState.Visible;
						mSpeakingCharacter.worlditem.ActiveStateLocked = true;
						Debug.Log ("Setting character's conversation name to " + State.CharacterConversation);
						//we don't need motile for this character
						Motile m = null;
						if (mSpeakingCharacter.worlditem.Is <Motile> (out m)) {
							m.Finish ();
						}
						mSpeakingCharacter.Ghost = true;
						mSpeakingCharacter.Body.OnSpawn (mSpeakingCharacter);
					}
				}
			}

			if (mSpeakingCharacter != null) {

				Talkative t = mSpeakingCharacter.worlditem.Get <Talkative> ();
				t.State.ConversationName = State.CharacterConversation;
				t.State.DefaultToDTS = false;
				//don't show the body until we're sure it has its materials
				mSpeakingCharacter.Body.SetVisible (false);
				mSpeakingCharacter.Body.IgnoreCollisions (true);
				mSpeakingCharacter.Body.LockVisible = true;
				mActivating = true;
				StartCoroutine (ActivateOverTime ());
				enabled = true;
			}
		}

		public void DeactivateGhost () {
			if (mActivating | mDeactivating)
				return;

			ActiveLight.enabled = false;
			mDeactivating = true;
			StartCoroutine (DeactivateOverTime ());

			MasterAudio.PlaySound (SoundType, worlditem.tr, SoundOnDeactivated);
		}

		public void OnExchangeChosen () {
			if (!Activated || mSpeakingCharacter == null) {
				return;
			}

			Glitch = 0.95f;
			MasterAudio.PlaySound (MasterAudio.SoundType.Obex, worlditem.tr, GlitchSounds [UnityEngine.Random.Range (0, GlitchSounds.Length)]);
		}

		public void Update () {
			if (!Activated || mSpeakingCharacter == null) {
				Activated = false;
				enabled = false;
				return;
			}

			if (mBodyMaterial == null) {
				//wait for us to get it
				return;
			}

			ActiveLight.intensity = Mathf.Abs (Mathf.Sin (Time.realtimeSinceStartup * PulseSpeed) + UnityEngine.Random.Range (-Glitch, Glitch));
			mCurrentColor = Color.Lerp (mCurrentColor, mTargetColor, (float)(WorldClock.RTDeltaTime) + UnityEngine.Random.Range (-Glitch, Glitch));
			if (Glitch > 0.2f) {
				mCurrentColor = Color.Lerp (mCurrentColor, Colors.RandomColor (), Glitch);
			}
			mBodyMaterial.SetColor ("_RimColor", mCurrentColor);
			mFaceMaterial.SetColor ("_RimColor", mCurrentColor);
			Glitch = Mathf.Lerp (Glitch, 0f, (float)WorldClock.RTDeltaTime * 5);

			ActiveFX.transform.localPosition = ActiveFXOnPosition;
			ActiveFX.Rotate (0f, (float)WorldClock.RTDeltaTime, 0f);

			mSpeakingCharacter.worlditem.tr.position = CharacterBase.position;
			mSpeakingCharacter.worlditem.tr.rotation = CharacterBase.rotation;
			mSpeakingCharacter.Body.transform.position = CharacterBase.position;
			mSpeakingCharacter.Body.transform.rotation = CharacterBase.rotation;
			mBodyCurrentScale = Vector3.Lerp (mBodyCurrentScale, mBodyTargetScale + (Vector3.one * UnityEngine.Random.Range (-Glitch, Glitch)), (float)(WorldClock.RTDeltaTime));
			mSpeakingCharacter.Body.transform.localScale = mBodyCurrentScale;
		}

		protected IEnumerator ActivateOverTime () {
			//wait for body to be added to group etc
			yield return null;
			State.HasNewMessage = true;
			mTargetColor = Colors.Get.GhostColor;
			if (mBodyBaseScale == Vector3.zero) {
				//only set it the first time or he'll shrink continuously
				mBodyBaseScale = mSpeakingCharacter.Body.transform.localScale;
			}
			mBodyTargetScale = mBodyBaseScale * CharacterBase.localScale.y;
			mCurrentColor = Color.clear;
			CharacterBody body = (CharacterBody)mSpeakingCharacter.Body;
			mBodyMaterial = body.MainMaterial;
			mFaceMaterial = body.FaceMaterial;
			mBodyMaterial.SetColor ("_RimColor", mCurrentColor);
			mFaceMaterial.SetColor ("_RimColor", mCurrentColor);
			body.SetVisible (true);

			double waitUntil = WorldClock.AdjustedRealTime + gWarmUpDelay;
			while (WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			ActiveLight.intensity = 0f;
			ActiveLight.enabled = true;

			//if the ghost is already equipped or is in the world, we're finished
			//otherwise wait for the player to equip us
			while (!worlditem.Is (WIMode.Equipped | WIMode.World | WIMode.Frozen)) {
				waitUntil = WorldClock.AdjustedRealTime + gWarmUpDelay;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
				//play a sound to let the player know we're active
				MasterAudio.PlaySound (SoundType, worlditem.tr, SoundOnActivated);
			}

			bool inRangeOrReady = false;
			Equippable equippable = worlditem.Get<Equippable> ();

			while (!inRangeOrReady) {
				if (worlditem.Is (WIMode.Equipped)) {
					if (equippable.FullyEquipped) {
						inRangeOrReady = true;
					}
				} else {
					//then wait until the player is in range before initiating conversation
					if (Vector3.Distance (Player.Local.Position, worlditem.tr.position) < gMinimumSpeakDistance) {
						waitUntil = WorldClock.AdjustedRealTime + gWarmUpDelay;
						while (WorldClock.AdjustedRealTime < waitUntil) {
							yield return null;
						}
						MasterAudio.PlaySound (SoundType, worlditem.tr, SoundOnActivated);
					}
				}
				yield return null;
			}

			//we may become inactive at some point and get destroyed
			if (worlditem.Destroyed) {
				yield break;
			}

			//now that the player is in range and we're equipped or active, start the conversation
			Talkative t = mSpeakingCharacter.worlditem.Get <Talkative> ();
			t.State.ConversationName = State.CharacterConversation;
			t.State.DefaultToDTS = false;
			t.SpeakThroughIntermediary (this);
			State.HasNewMessage = false;
		
			mActivating = false;
			yield break;
		}

		protected IEnumerator DeactivateOverTime () {

			if (mSpeakingCharacter != null) {
				mBodyTargetScale = Vector3.one * 0.1f;
				mTargetColor = Color.clear;
			}

			double waitUntil = WorldClock.AdjustedRealTime + gWarmUpDelay;
			while (WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			if (mSpeakingCharacter != null) {
				mSpeakingCharacter.Body.SetVisible (false);
			}

			mDeactivating = false;
			Activated = false;
			ActiveFX.transform.localPosition = Vector3.zero;
			yield break;
		}

		protected bool mActivating = false;
		protected bool mDeactivating = false;
		protected Character mSpeakingCharacter;
		protected ActionNode mCharacterSpawnPoint;
		protected Color mTargetColor = Color.white;
		protected Color mCurrentColor = Color.clear;
		protected Material mBodyMaterial;
		protected Material mFaceMaterial;
		protected Vector3 mBodyBaseScale;
		protected Vector3 mBodyCurrentScale;
		protected Vector3 mBodyTargetScale;
		protected double mChangeStart = 0f;
		protected static float gWarmUpDelay = 2f;
		protected static float gMinimumSpeakDistance = 3f;
	}

	[Serializable]
	public class ObexGhostState {
		public string CharacterName;
		public string CharacterConversation;
		public bool HasNewMessage = false;
	}
}

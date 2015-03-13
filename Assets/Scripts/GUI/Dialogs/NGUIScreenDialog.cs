using UnityEngine;
using System.Collections;
using Frontiers.Story.Conversations;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class NGUIScreenDialog : BaseInterface
		{
				public static NGUIScreenDialog Get;
				public GameObject ConversationBubblePrototype;
				public UIPanel Panel;
				public Color CharacterColor;
				public GameObject BubbleParent;
				public GUIConversationBubble CurrentBubble;
				public float TargetSpeechCharacterAlpha = 0f;
				public float TargetSpeechAlpha = 0f;
				public double FadeSpeed = 4f;
				public double FadeOutSpeed = 8f;

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
				}

				public override void Start()
				{
						UserActions.Subscribe(UserActionType.ActionSkip, new ActionListener(ActionSkip));	
						UserActions.Filter = UserActionType.ActionCancel | UserActionType.ItemUse;//intercept all clicks and esc
						OnLoseFocus += ClearSpeeches;

						Panel.enabled = false;
						base.Start();
				}

				public bool ActionSkip(double timeStamp)
				{
						mCurrentSpeech = CharacterSpeech.Empty;
						return true;
				}

				public void ClearSpeeches()
				{
						//Debug.Log("Clearing speeches");
						GUIManager.Get.ReleaseFocus(this);
						mCurrentSpeech = CharacterSpeech.Empty;
						mUpdatingSpeeches = false;
						Get.mSpeechesToDisplay.Clear();
						if (CurrentBubble != null) {
								CurrentBubble.DestroyBubble();
								CurrentBubble = null;
						}
				}

				public static void AddSpeech(string speech, string characterName, double duration)
				{
						//Debug.Log("Adding speech");
						Get.mSpeechesToDisplay.Enqueue(new CharacterSpeech(speech, characterName, duration, WorldClock.AdjustedRealTime));
						if (!mUpdatingSpeeches) {
								mUpdatingSpeeches = true;
								Get.StartCoroutine(Get.UpdateSpeeches());
						}
				}

				public override bool GainFocus()
				{
						if (!mUpdatingSpeeches) {
								//if something took focus from us, then we shouldn't be active
								ClearSpeeches();
								return false;
						} else {
								return base.GainFocus();
						}
				}

				protected IEnumerator UpdateSpeeches()
				{
						//Debug.Log("Updating speeches now");
						if (!HasFocus) {
								if (!GUIManager.Get.GetFocus(this)) {
										//Debug.Log("Couldn't get focus, not updating speeches");
										ClearSpeeches();
										yield break;
								}
						}

						mCurrentSpeech = mSpeechesToDisplay.Dequeue();

						if (mCurrentSpeech.IsEmpty) {
								yield break;
						}

						mCurrentSpeech.StartTime = WorldClock.AdjustedRealTime;
						CharacterColor = Colors.Saturate(Colors.ColorFromString(mCurrentSpeech.CharacterName, 100));
						Panel.enabled = true;

						if (CurrentBubble != null && !CurrentBubble.IsDestroying) {
								//we may be able to use the same bubble
								//if the character's name is the same
								if (mCurrentSpeech.CharacterName != CurrentBubble.CharacterName.text) {
										CurrentBubble.DestroyBubble();
										CurrentBubble = CreateBubble();
								}
						} else {
								CurrentBubble = CreateBubble();
						}
						//set the properties on the speech bubble
						//it will fade in on its own
						CurrentBubble.SetProps(mCurrentSpeech.Speech, mCurrentSpeech.CharacterName, CharacterColor, Colors.Get.MenuButtonTextColorDefault);

						while (CurrentBubble.Alpha < 1f) {
								//Debug.Log("Waiting for alwpha to hit 1...");
								if (!HasFocus || mCurrentSpeech.IsEmpty || GUIManager.Get.HasActivePrimaryInterface || GUIManager.Get.HasActiveSecondaryInterface) {
										ClearSpeeches();
										yield break;
								}
								yield return null;
						}
						//now wait for the speech duration to end before making it fade out
						double waitUntil = mCurrentSpeech.StartTime + (mCurrentSpeech.Duration * Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed);
						while (WorldClock.AdjustedRealTime < waitUntil) {
								//Debug.Log("Waiting until finished...");
								if (!HasFocus || mCurrentSpeech.IsEmpty || GUIManager.Get.HasActivePrimaryInterface || GUIManager.Get.HasActiveSecondaryInterface) {
										ClearSpeeches();
										yield break;
								}
								yield return null;
						}

						if (!HasFocus || mCurrentSpeech.IsEmpty) {
								ClearSpeeches();
								yield break;
						}

						//now let it fade out on its own
						CurrentBubble.SetProps(string.Empty, string.Empty, CharacterColor, Colors.Get.MenuButtonTextColorDefault);
						while (CurrentBubble.Alpha > 0f) {
								//Debug.Log("Waiting for alwpha to hit 0...");
								if (!HasFocus || mCurrentSpeech.IsEmpty || GUIManager.Get.HasActivePrimaryInterface || GUIManager.Get.HasActiveSecondaryInterface) {
										ClearSpeeches();
										yield break;
								}
								yield return null;
						}

						//if we're all done then fade out and close the panel over time
						if (mSpeechesToDisplay.Count > 0) {
								//Debug.Log("Getting next speech...");
								//whoops, one was added while we were waiting
								//re-start this coroutine and exit immediately
								StartCoroutine(UpdateSpeeches());
								yield break;
						}
						ClearSpeeches();
						mUpdatingSpeeches = false;
						yield break;
				}

				protected GUIConversationBubble CreateBubble()
				{
						//create the new conversation bubble
						GameObject newBubbleGameObject = NGUITools.AddChild(BubbleParent, ConversationBubblePrototype);
						GUIConversationBubble bubble = newBubbleGameObject.GetComponent <GUIConversationBubble>();
						bubble.SetProps(mCurrentSpeech.Speech, mCurrentSpeech.CharacterName, CharacterColor, Colors.Get.MenuButtonTextColorDefault);
						bubble.FadedColor = false;
						bubble.FadeDelay = 0.125f;
						bubble.transform.localPosition = new Vector3(-bubble.Collider.center.x, 0f, -10f);
						return bubble;
				}

				public struct CharacterSpeech
				{
						public CharacterSpeech(string speech, string characterName, double rtDuration, double addTime)
						{
								Speech = speech;
								CharacterName = characterName;
								Duration = rtDuration;
								AddTime = addTime;
								StartTime = 0f;
						}

						public static CharacterSpeech Empty {
								get {
										return gEmptyCharacterSpeech;
								}
						}

						public bool IsEmpty {
								get {
										return string.IsNullOrEmpty(Speech);
								}
						}

						public string Speech;
						public string CharacterName;
						public double Duration;
						public double AddTime;
						public double StartTime;
						public static CharacterSpeech gEmptyCharacterSpeech;
				}

				protected static bool mUpdatingSpeeches = false;
				protected CharacterSpeech mCurrentSpeech = CharacterSpeech.Empty;
				protected Queue <CharacterSpeech> mSpeechesToDisplay = new Queue <CharacterSpeech>();
		}
}
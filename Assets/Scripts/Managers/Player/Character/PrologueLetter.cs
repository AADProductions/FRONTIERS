using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay.Story
{
		public class PrologueLetter : MonoBehaviour
		{
				public bool FinishedWriting = false;
				public bool WaitForNextPage = false;
				public int CurrentPageIndex	= 0;
				public int NumCharactersPerFrame = 5;
				public double TimePerPage = 1.5f;
				public List <PrologueLetterPage> LetterPages = new List <PrologueLetterPage>();
				PrologueLetterPage CurrentPage = null;
				public GUILetterWriter LetterWriter = null;
				public GUICharacterCreator QuickCreateDialog;
				public CharacterCreator Creator = new CharacterCreator();
				public float MaxWidth = 10.0f;
				public Renderer LetterTextHighlight;
				public TextMesh LetterTextLabel;
				public Color HighlightColor;
				public TextSize Size;
				public PrologueLetterAction CurrentAction;
				public float HighlightSpeed = 10f;
				public MasterAudio.SoundType SoundType;
				public List <string> WritingSounds = new List<string>() {
						"Writing",
						"Writing2",
						"Writing3",
						"Writing4",
						"Writing5",
						"Writing6"
				};

				public string PageTurnSound;

				public void OnQuickCreateStart()
				{
						GameObject characterCreatorDialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUICharacterCreator"));
						GUIManager.SendEditObjectToChildEditor <CharacterCreator>(new ChildEditorCallback <CharacterCreator>(OnQuickCreateFinish), characterCreatorDialog, Creator);
						QuickCreateDialog = characterCreatorDialog.GetComponent <GUICharacterCreator>();
				}

				public void OnCharacterCreated ( ){
						if (!mFinishingWritingLetter) {
								mFinishingWritingLetter = true;
								StartCoroutine(FinishWritingLetter());
						}
				}

				public void OnCutsceneFinished()
				{
						//no longer necessary
//						if (!Creator.Confirmed) {
//								//we haven't made our character - create a default character
//								string errorMessage = string.Empty;
//								Creator.Confirm (out errorMessage);
//								Profile.Get.SaveCurrent (ProfileComponents.Character);
//						}
				}

				public void OnQuickCreateFinish(CharacterCreator editObject, IGUIChildEditor <CharacterCreator> childEditor)
				{
						if (editObject.Confirmed && !editObject.Cancelled) {
								//we're done here
								FinishedWriting = true;
								LetterWriter.WritingLetter = false;
								OnCharacterCreated();
						} else {
								//otherwise continue with letter
								LetterWriter.QuickCreateMode = false;
								LetterWriter.QuickCreateButton.SendMessage ("SetEnabled");
						}
				}

				public void SkipToNextChoice()
				{

				}

				public void Start()
				{
						Size = new TextSize(LetterTextLabel);
						LetterTextLabel.text = string.Empty;
						LetterWriter.WritingLetter = true;
						LetterWriter.OnQuickCreateStart += OnQuickCreateStart;
						Creator.StartEditing (Profile.Get.CurrentGame.Character);
						HighlightColor = Color.white;
						HighlightColor.a = 0f;
				}

				public void OnNextPage()
				{
						MasterAudio.PlaySound(SoundType, PageTurnSound);
						WaitForNextPage = false;
				}

				public void OnMakeChoice(string variableResult)
				{
						CurrentAction.Completed = true;
						CurrentAction.Confirmed = true;
						CurrentAction.VariableResult = variableResult;
						//temp
						LetterWriter.LastChoiceConfirmed = true;
						CurrentAction.Confirmed = true;

						string errorMessage = string.Empty;
						switch (CurrentAction.VariableRequest) {
								case "Name":
										Creator.SetName(CurrentAction.VariableResult);
										break;

								case "Gender":
										switch (CurrentAction.VariableResult) {
												case "Male":
														Creator.SetGender(CharacterGender.Male);
														CurrentAction.DisplayResult = "nephew";
														break;

												case "Female":
														Creator.SetGender(CharacterGender.Female);
														CurrentAction.DisplayResult = "niece";
														break;
										}
										break;

								case "Age":
										Creator.SetAge(Int32.Parse(CurrentAction.VariableResult));
										CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
										break;

								case "Ethnicity":
										switch (CurrentAction.VariableResult) {
												case "Pink":
														Creator.SetEthnicity(CharacterEthnicity.Caucasian);
														break;

												case "Brown":
														Creator.SetEthnicity(CharacterEthnicity.BlackCarribean);
														break;

												case "Olive":
														Creator.SetEthnicity(CharacterEthnicity.HanChinese);
														break;

												case "Tan":
														Creator.SetEthnicity(CharacterEthnicity.EastIndian);
														break;
										}
										CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
										break;

								case "EyeColor":
										Creator.SetEyeColor((CharacterEyeColor)Enum.Parse(typeof(CharacterEyeColor), CurrentAction.VariableResult, true));
										CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
										break;

								case "HairColor":
										Creator.SetHairColor((CharacterHairColor)Enum.Parse(typeof(CharacterHairColor), CurrentAction.VariableResult, true));
										CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
										break;

								case "HairLength":
										Creator.SetHairLength((CharacterHairLength)Enum.Parse(typeof(CharacterHairLength), CurrentAction.VariableResult, true));
										CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
										break;

								default:
										break;
						}
						//give the highlight a bump
						HighlightColor.a = 1.0f;
				}

				public void OnCutsceneIdleStart()
				{
						StartCoroutine(WriteLetter());
				}

				protected bool mFinishingWritingLetter = false;
				protected bool mHasMadeOneChoice = false;
				protected double mNextSoundPlayTime = 0;

				public IEnumerator WriteLetter()
				{
						CurrentPageIndex = 0;
						CurrentPage = LetterPages[CurrentPageIndex];

						while (!FinishedWriting) {
								if (LetterWriter.QuickCreateMode) {
										//wait for quick create to finish
										yield return null;
								} else {
										//otherwise write the letter
										if (mHasMadeOneChoice && WorldClock.RealTime > mNextSoundPlayTime) {
												MasterAudio.PlaySound(SoundType, WritingSounds[UnityEngine.Random.Range(0, WritingSounds.Count)]);
												mNextSoundPlayTime = WorldClock.RealTime + 1.0;
										}
										HighlightColor.a = Mathf.Lerp(HighlightColor.a, 0f, (float)(HighlightSpeed * Frontiers.WorldClock.ARTDeltaTime));
										if (HighlightColor.a < 0.001f) {	//snap to zero so we're not waiting forever
												HighlightColor.a = 0f;
										}

										CurrentPage.Refresh(Creator.Character);
										bool newAction = false;
										for (int i = 0; i < NumCharactersPerFrame * WorldClock.RTDeltaTime; i++) {
												if (CurrentPage.GoToNextChar(out CurrentAction)) {
														//do we need to do this? have we created the character already?
														newAction = true;
														MasterAudio.StopAllOfSound(MasterAudio.SoundType.PlayerInterface);
														mNextSoundPlayTime = 0;
														break;
												}
										}

										if (newAction) {////Debug.Log ("Going to next action");
												//if we have a new action to wait for
												//update the page first
												LetterTextLabel.text = CurrentPage.DisplayText;
												Size.FitToWidth(MaxWidth);
												//then dispatch the action and wait for a result
												yield return StartCoroutine(WaitForCurrentAction());
										} else if (LetterWriter.SkipToNextChoice) { //if the player wants to skip ahead
												//Debug.Log ("PROLOGUE LETTER: Skipping to next choice...");
												LetterWriter.SkipToNextChoice = false;
												if (CurrentPage.GoToNextChoice(out CurrentAction)) {
														//if we have a new action on this page
														//update the page
														//then wait for current action
														LetterTextLabel.text = CurrentPage.DisplayText;
														Size.FitToWidth(MaxWidth);
														yield return StartCoroutine(WaitForCurrentAction());
												} else { //otherwise go to the next page
														yield return StartCoroutine(GoToNextPage());
												}
										} else {	//otherwise just update the page and wait the time alotted
												LetterTextLabel.text = CurrentPage.DisplayText;
												Size.FitToWidth(MaxWidth);
												yield return null;
										}

										if (CurrentPage.GoToNextPage) {	////Debug.Log ("Time to go to next page");
												yield return StartCoroutine(GoToNextPage());
										}
										yield return null;
								}
						}
						yield break;
				}

				protected IEnumerator GoToNextPage()
				{
						HighlightColor.a = 0f;
						while (HighlightColor.a > 0f) {	//wait for the highlight to run down first
								HighlightColor.a = Mathf.Lerp(HighlightColor.a, 0f, (float)(HighlightSpeed * Frontiers.WorldClock.ARTDeltaTime));
								if (HighlightColor.a < 0.001f) {	//snap to zero so we're not waiting forever
										HighlightColor.a = 0f;
								}
						}

						////Debug.Log ("Going to next page");
						CurrentPageIndex++;
						if (CurrentPageIndex < LetterPages.Count) {
								CurrentPage	= LetterPages[CurrentPageIndex];
								CurrentPage.LastCharDisplayed = -1;
								CurrentPage.Refresh(Creator.Character);
								WaitForNextPage = true;
								LetterWriter.WaitForNextPage();
								//wait for player to confirm they want to move forward
								while (WaitForNextPage) {
										yield return null;
								}
						} else {
								FinishedWriting = true;
								LetterWriter.WritingLetter = false;
								OnCharacterCreated();
						}
						yield break;
				}

				protected IEnumerator WaitForCurrentAction()
				{					
						if (Creator.Character.CreatedManually) {
								CurrentAction.Completed = true;
								CurrentAction.Confirmed = true;
								switch (CurrentAction.VariableRequest) {
										case "Name":
										default:
												CurrentAction.VariableResult = Creator.Character.FirstName;
												break;

										case "Gender":
												switch (Creator.Character.Gender) {
														case CharacterGender.Male:
														default:
																CurrentAction.VariableResult = "Male";
																CurrentAction.DisplayResult = "nephew";
																break;

														case CharacterGender.Female:
																CurrentAction.VariableResult = "Female";
																CurrentAction.DisplayResult = "neice";
																break;
												}
												break;

										case "Age":
												CurrentAction.VariableResult = Creator.Character.Age.ToString();
												break;

										case "Ethnicity":
												switch (Creator.Character.Ethnicity) {
														case CharacterEthnicity.Caucasian:
														default:
																CurrentAction.VariableResult = "Pink";
																break;

														case CharacterEthnicity.BlackCarribean:
																CurrentAction.VariableResult = "Brown";
																break;

														case CharacterEthnicity.HanChinese:
																CurrentAction.VariableResult = "Olive";
																break;

														case CharacterEthnicity.EastIndian:
																CurrentAction.VariableResult = "Tan";
																break;
												}
												CurrentAction.DisplayResult = CurrentAction.VariableResult.ToLower();
												break;

										case "EyeColor":
												CurrentAction.VariableResult = Creator.Character.EyeColor.ToString().ToLower();
												break;

										case "HairColor":
												CurrentAction.VariableResult = Creator.Character.HairColor.ToString().ToLower();
												break;

										case "HairLength":
												CurrentAction.VariableResult = Creator.Character.HairLength.ToString().ToLower();
												break;

								}
								//temp
								LetterWriter.LastChoiceConfirmed = true;
								CurrentAction.Confirmed = true;
								yield break;
						}

						////Debug.Log ("Waiting for next action");
						//move the letter highlight to the correct position & scale
						HighlightColor.a = 0f;
						LetterTextHighlight.transform.localPosition = CurrentAction.HighlightPosition;
						LetterTextHighlight.transform.localScale	= CurrentAction.HighlightSize;

						//tell the letter writer to display the variable we want
						LetterWriter.OpenChooser(CurrentAction.VariableRequest);

						while (CurrentAction != null && !CurrentAction.Confirmed) {	//fade in letter highlight
								HighlightColor.a = Mathf.Lerp(HighlightColor.a, 0.75f, (float)(HighlightSpeed * Frontiers.WorldClock.ARTDeltaTime));
								//if the current action hasn't been confirmed yet
								//chill out here
								yield return null;
						}
						mHasMadeOneChoice = true;
						//let it sink in
						double waitUntil = WorldClock.RealTime + 0.5f;
						while (WorldClock.RealTime < waitUntil) {
								yield return null;
						}
						////Debug.Log ("Finished waiting for next action");
						yield break;
				}

				protected IEnumerator FinishWritingLetter ( ) {
						//fade out to make the letter disappear
						double waitUntil = WorldClock.RealTime + 2f;
						while (WorldClock.RealTime < waitUntil) {
								yield return null;
						}
						//use the loading camera instead of fade in/out
						GUILoading.Get.BackgroundSprite.alpha = 0f;
						GUILoading.Get.BackgroundSprite.enabled = true;
						GUILoading.Get.gameObject.SetActive(true);
						GUILoading.Get.LoadingCamera.enabled = true;
						double fadeStartTime = WorldClock.RealTime;
						double fadeDuration = 2.0;
						while (GUILoading.Get.BackgroundSprite.alpha < 1f) {
								GUILoading.Get.BackgroundSprite.alpha = (float)WorldClock.Lerp (GUILoading.Get.BackgroundSprite.alpha, 1f, (WorldClock.RealTime - fadeStartTime) / fadeDuration);
								if (GUILoading.Get.BackgroundSprite.alpha > 0.99f) {
										GUILoading.Get.BackgroundSprite.alpha = 1f;
								}
								yield return null;
						}
						//turn everything off and swap out the letters
						for (int i = 0; i < LetterWriter.LetterPages.Count; i++) {
								LetterWriter.LetterPages[i].SetActive(false);
						}
						LetterWriter.LetterTextObject.SetActive(false);
						MasterAudio.PlaySound(SoundType, WritingSounds[UnityEngine.Random.Range(0, WritingSounds.Count)]);
						LetterWriter.FinalLetter.SetActive(true);
						waitUntil = WorldClock.RealTime + 0.25f;
						while (WorldClock.RealTime < waitUntil) {
								yield return null;
						}
						MasterAudio.PlaySound(SoundType, PageTurnSound);
						Cutscene.CurrentCutscene.TryToFinish();
						waitUntil = WorldClock.RealTime + 0.25f;
						while (WorldClock.RealTime < waitUntil) {
								yield return null;
						}
						fadeStartTime = WorldClock.RealTime;
						while (GUILoading.Get.BackgroundSprite.alpha > 0f) {
								GUILoading.Get.BackgroundSprite.alpha = (float)WorldClock.Lerp (GUILoading.Get.BackgroundSprite.alpha, 0f, (WorldClock.RealTime - fadeStartTime) / fadeDuration);
								if (GUILoading.Get.BackgroundSprite.alpha < 0.01f) {
										GUILoading.Get.BackgroundSprite.alpha = 0f;
								}
								yield return null;
						}
						GUILoading.Get.BackgroundSprite.enabled = false;
						GUILoading.Get.LoadingCamera.enabled = false;
						GUILoading.Get.gameObject.SetActive(false);
						////Debug.Log ("GUI LETTER WRITER: Cutscene finished, releasing focus and destroying");
						//GUIManager.Get.ReleaseFocus (this);
						//GameObject.Destroy (gameObject, 0.5f);
						//tell the cutscene we're finished
				}

				[Serializable]
				public class PrologueLetterPage
				{
						public void Refresh(PlayerCharacter character)
						{
								FinalText = LetterText;
								FinalText = FinalText.Replace("{charname}", character.FirstName);
								foreach (PrologueLetterAction action in LetterActions) {
										if (action.Confirmed) {
												FinalText = FinalText.Replace(action.ReplaceString, action.Result);
										} else {
												action.TriggerCharacter = FinalText.IndexOf(action.ReplaceString);
										}
								}
						}

						public bool GoToNextChar(out PrologueLetterAction newAction)
						{
								bool triggeredNewAction	= false;
								newAction = null;

								LastCharDisplayed++;
								if (LastCharDisplayed <= FinalText.Length) {
										DisplayText = FinalText.Substring(0, LastCharDisplayed);
								}
								foreach (PrologueLetterAction action in LetterActions) {
										if (!action.Activated && action.TriggerCharacter == LastCharDisplayed) {
												action.Activated = true;
												newAction = action;
												triggeredNewAction	= true;
												break;
										}
								}
								return triggeredNewAction;
						}

						public bool GoToNextChoice(out PrologueLetterAction newAction)
						{
								//Debug.Log ("PROLOGUE LETTER: Going to next choice in prologue letter page");
								bool triggeredNewAction = false;
								newAction = null;
								LastCharDisplayed = DisplayText.Length - 1;//the end of the page
								foreach (PrologueLetterAction action in LetterActions) {
										//check and see if we have any actions left, and what character will
										//trigger them if we do have one
										if (!action.Activated) {
												if (action.TriggerCharacter < LastCharDisplayed) {
														//if the trigger character is less than the last one
														//this action comes first
														//we do it this way because no guarantee of correct order
														triggeredNewAction = true;
														newAction = action;
														LastCharDisplayed = action.TriggerCharacter;
												}
										}
								}
								if (triggeredNewAction) {
										//Debug.Log ("Skipping to action " + newAction.ReplaceString);
								}
								return triggeredNewAction;
						}

						public int LastCharDisplayed = -1;
						[Multiline]
						public string LetterText = string.Empty;
						[Multiline]
						public string DisplayText = string.Empty;
						[Multiline]
						public string FinalText = string.Empty;
						public List <PrologueLetterAction>	LetterActions = new List <PrologueLetterAction>();

						public bool GoToNextPage {
								get {
										bool goToNextPage = LastCharDisplayed >= FinalText.Length;
										foreach (PrologueLetterAction action in LetterActions) {
												if (!action.Confirmed) {
														goToNextPage = false;
														break;
												}
										}
										return goToNextPage;
								}
						}
				}

				[Serializable]
				public sealed class PrologueLetterAction
				{
						public bool Activated = false;
						public bool Completed = false;
						public bool Confirmed = false;
						public int TriggerCharacter	= 0;
						public string	ReplaceString = string.Empty;
						public Vector3	HighlightPosition	= Vector3.zero;
						public Vector3	HighlightSize = Vector3.zero;
						public string	VariableRequest = string.Empty;
						public string	VariableResult = string.Empty;
						public string	DisplayResult = string.Empty;

						public string	Result {
								get {
										if (!string.IsNullOrEmpty(DisplayResult)) {
												return DisplayResult;
										}
										return VariableResult;
								}
						}
				}
		}
}
using UnityEngine;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using System;

namespace Frontiers.GUI
{
		public class GUILetterWriter : CutsceneInterface
		{
				public override bool AxisLock {
						get {
								return false;
						}
				}

				public override bool CursorLock {
						get {
								return false;
						}
				}

				public override bool CustomVRSettings {
						get {
								return true;
						}
				}

				public override Vector3 LockOffset {
						get {
								return CustomLockOffset;
						}
				}

				public bool WritingLetter = false;
				public bool MakingChoice = false;
				public bool SkipToNextChoice = false;
				public GameObject OnChooseTarget;
				public string OnChooseMessage;
				public string LastChoiceMade;
				public string LastRequestMade;
				public Vector3 CustomLockOffset;
				public Vector3 ChooserOpenPosition;
				public Vector3 ChooserClosedPosition;
				public Vector3 TargetPosition;
				public Vector3 ShadowTargetPosition;
				public Vector3 ShadowOpenPosition;
				public Vector3 ShadowClosedPosition;
				public Action OnQuickCreateStart;
				public Vector3 QuickCreateOpenPosition;
				public Vector3 QuickCreateClosedPosition;
				public GameObject ShadowObject;
				public GameObject CurrentChooser = null;
				public GameObject EyeColorChooser;
				public GameObject GenderChooser;
				public GameObject EthnicityChooser;
				public GameObject HairColorChooser;
				public GameObject NameChooser;
				public GameObject HairLengthChooser;
				public GameObject AgeChooser;
				public GameObject ChooserPanel;
				public GameObject QuickCreatePanel;
				public GameObject IconGroup;
				public GameObject NextPageGroup;
				public GameObject NextPageButton;
				public GameObject LetterTextObject;
				public GameObject QuickCreateButton;
				public UIAnchor RightAnchor;
				public List <GameObject> LetterPages = new List <GameObject>();
				public GameObject FinalLetter;
				public bool WaitingForNextPage;
				public double SpeedMultiplier = 5.0;
				public int SelectedAge = ((Globals.MinCharacterAge + Globals.MaxCharacterAge) / 2);
				public UILabel AgeLabel;
				public UISlider AgeSlider;
				public UIInput NameChooserInput;
				public bool LastChoiceConfirmed	= false;
				public bool QuickCreateMode = false;
				public bool SetLastCursorPosition = false;
				public int QuickCharacterCreationID;

				public override void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						if (QuickCreateMode) {
								return;
						}

						if (flag < 0) { flag = GUIEditorID; }

						FrontiersInterface.Widget w = new FrontiersInterface.Widget(flag);

						if (WaitingForNextPage) {
								FrontiersInterface.GetActiveInterfaceObjectsInTransform(NextPageGroup.transform, NGUICamera, currentObjects, flag);
						} else if (CurrentChooser != null) {
								FrontiersInterface.GetActiveInterfaceObjectsInTransform(CurrentChooser.transform, NGUICamera, currentObjects, flag);
								if (LastChoiceMade == "Age") {
										w.SearchCamera = NGUICamera;
										w.BoxCollider = AgeSlider.foreground.GetComponent <BoxCollider>();
										currentObjects.Add(w);
								}
						}

						w.Flag = QuickCharacterCreationID;
						w.SearchCamera = NGUICamera;
						w.BoxCollider = QuickCreateButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
				}

				public void Start()
				{
						QuickCharacterCreationID = GUIManager.GetNextGUIID();

						ChooserPanel.transform.localPosition = ChooserClosedPosition;
			
						EyeColorChooser.transform.localPosition = ChooserClosedPosition;
						GenderChooser.transform.localPosition = ChooserClosedPosition;
						EthnicityChooser.transform.localPosition = ChooserClosedPosition;
						HairColorChooser.transform.localPosition = ChooserClosedPosition;
						HairLengthChooser.transform.localPosition = ChooserClosedPosition;
						AgeChooser.transform.localPosition = ChooserClosedPosition;
						NameChooser.transform.localPosition = ChooserClosedPosition;

						ShadowTargetPosition = ShadowClosedPosition;
						ShadowObject.transform.localPosition = ShadowClosedPosition;

						NameChooserInput.functionName = "OnSubmit";
						NameChooserInput.functionNameEnter = "OnSubmitWithEnter";
						NameChooserInput.eventReceiver = gameObject;
						NameChooserInput.text = Profile.Get.Current.Name;
						NameChooserInput.label.text = Profile.Get.Current.Name;
			
						OnChangeAgeValue();

						//TODO I think this is what's keeping the gameobject alive
						//UserActions.Subscribe (UserActionType.ActionSkip, new ActionListener (ActionSkip));
				}

				public void OnSubmit ()
				{
						//Debug.Log("On submit in text: " + NameChooserInput.text);
						NameChooserInput.label.text = NameChooserInput.text + "[FF00FF]" + NameChooserInput.caratChar + "[-]";
				}

				public bool ActionSkip(double timeStamp)
				{
						////Debug.Log ("Skipping to next choice in GUILetterWriter");
						SkipToNextChoice = true;
						return true;
				}

				public void OnClickQuickCreateButton()
				{
						QuickCreateButton.SendMessage("SetDisabled");
						QuickCreateMode = true;
						OnQuickCreateStart.SafeInvoke();
				}

				public void OnCutsceneIdleStart()
				{
						ShadowTargetPosition = ShadowOpenPosition;
						//GUIManager.Get.GetFocus (this);
				}

				public void OnCutsceneFinished()
				{
						ShadowTargetPosition = ShadowClosedPosition;
				}

				public void OnChangeAgeValue()
				{
						SelectedAge = Mathf.FloorToInt((AgeSlider.sliderValue * (Globals.MaxCharacterAge - Globals.MinCharacterAge)) + Globals.MinCharacterAge);
						AgeLabel.text = SelectedAge.ToString();
				}

				public void WaitForNextPage()
				{
						//TargetPosition = ChooserOpenPosition;
						NextPageButton.SendMessage("SetEnabled");
						WaitingForNextPage = true;
						//force the cursor to attach to the next page widget
						FrontiersInterface.Widget w = new FrontiersInterface.Widget(GUIEditorID);
						w.SearchCamera = NGUICamera;
						w.BoxCollider = NextPageButton.GetComponent <BoxCollider>();
						GUICursor.Get.SelectWidget(w);
				}

				public void OnClickNextPage()
				{
						NextPageButton.SendMessage("SetDisabled");
						OnChooseTarget.SendMessage("OnNextPage");
						WaitingForNextPage = false;
				}

				public void ConfirmLastChoice()
				{
						LastChoiceConfirmed = true;
				}

				public void OnMakeChoice(GameObject sender)
				{
						if (!MakingChoice) {
								return;
						}
						//yadda yadda make choice
						switch (LastRequestMade) {
								case "EyeColor":
								case "Gender":
								case "Ethnicity":
								case "HairColor":
								case "HairLength":
								//sender will be a button
										LastChoiceMade = sender.name;
										break;

								case "Age":
								//yeesh, double to int, then parsing the int later?
								//yuck, but whatever
										LastChoiceMade = SelectedAge.ToString();
										break;
				
								case "Name":
								default:
								//sender will be a field
										LastChoiceMade = NameChooserInput.text;
										NameChooserInput.selected = false;
										UIInput.current = null;
										break;
						}
						CloseChooser();
				}

				public void OpenChooser(string request)
				{
						if (MakingChoice) {
								return;
						}

						SetLastCursorPosition = false;

						EyeColorChooser.transform.localPosition = ChooserClosedPosition;
						GenderChooser.transform.localPosition = ChooserClosedPosition;
						EthnicityChooser.transform.localPosition = ChooserClosedPosition;
						HairColorChooser.transform.localPosition = ChooserClosedPosition;
						HairLengthChooser.transform.localPosition = ChooserClosedPosition;
						AgeChooser.transform.localPosition = ChooserClosedPosition;
						NameChooser.transform.localPosition = ChooserClosedPosition;

						MakingChoice = true;
						LastChoiceConfirmed = false;
						LastRequestMade = request;

						NameChooserInput.selected = false;
						UIInput.current = null;

						switch (LastRequestMade) {
								case "EyeColor":
										CurrentChooser = EyeColorChooser;
										break;

								case "Gender":
										CurrentChooser = GenderChooser;
										break;

								case "Ethnicity":
										CurrentChooser = EthnicityChooser;
										break;

								case "HairColor":
										CurrentChooser = HairColorChooser;
										break;

								case "Age":
										CurrentChooser = AgeChooser;
										break;

								case "HairLength":
										CurrentChooser = HairLengthChooser;
										break;

								case "Name":
								default:
										CurrentChooser = NameChooser;
										NameChooserInput.selected = true;
										UIInput.current = NameChooserInput;
										FrontiersInterface.Widget w = new FrontiersInterface.Widget();
										w.SearchCamera = NGUICamera;
										w.BoxCollider = NameChooserInput.GetComponent<BoxCollider>();
										GUICursor.Get.SelectWidget(w);
										break;
						}

						CurrentChooser.transform.localPosition = ChooserClosedPosition;
						TargetPosition	= ChooserOpenPosition;
				}

				public void CloseChooser()
				{
						StartCoroutine(WaitATickAfterChoosing(CurrentChooser));
				}

				public void Update()
				{
						if (!Cutscene.IsActive)
								return;

						if (MakingChoice || WaitingForNextPage) {
								Cutscene.CurrentCutscene.ShowCursor = true;
						} else if (GUICursor.Get.LastSelectedWidgetFlag == GUIEditorID) {
								Cutscene.CurrentCutscene.ShowCursor = false;
						}

						if (QuickCreateMode) {
								ChooserPanel.transform.localPosition = Vector3.Lerp(ChooserPanel.transform.localPosition, ChooserClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
								QuickCreatePanel.transform.localPosition = Vector3.Lerp(QuickCreatePanel.transform.localPosition, QuickCreateClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime)); 
						} else if (WritingLetter) {
								ChooserPanel.transform.localPosition = Vector3.Lerp(ChooserPanel.transform.localPosition, ChooserOpenPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
								QuickCreatePanel.transform.localPosition = Vector3.Lerp(QuickCreatePanel.transform.localPosition, QuickCreateOpenPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));

								if (WaitingForNextPage) {
										NextPageGroup.transform.localPosition = Vector3.Lerp(NextPageGroup.transform.localPosition, ChooserOpenPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
										IconGroup.transform.localPosition = Vector3.Lerp(IconGroup.transform.localPosition, ChooserClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
										if (CurrentChooser != null) {
												CurrentChooser.transform.localPosition = Vector3.Lerp(CurrentChooser.transform.localPosition, ChooserClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
										}
								} else {
										NextPageGroup.transform.localPosition = Vector3.Lerp(NextPageGroup.transform.localPosition, ChooserClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
										IconGroup.transform.localPosition = Vector3.Lerp(IconGroup.transform.localPosition, TargetPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
										if (CurrentChooser != null) {
												CurrentChooser.transform.localPosition = Vector3.Lerp(CurrentChooser.transform.localPosition, TargetPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
												if (!SetLastCursorPosition) {
														//tell the cursor to pick something
														FrontiersInterface.Widget w = new FrontiersInterface.Widget(GUIEditorID);
														w.SearchCamera = NGUICamera;
														//look for the first box collider in the current chooser
														foreach (Transform child in CurrentChooser.transform) {
																w.BoxCollider = child.GetComponent <BoxCollider>();
																if (w.BoxCollider != null) {
																		break;
																}
														}
														GUICursor.Get.SelectWidget(w);
														SetLastCursorPosition = true;
												}
										}
								}
								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingMode) {
										#else
										if (VRManager.VRMode) {
										#endif
										//tell the cursor to follow us if we're selected
										GUICursor.Get.TryToFollowCurrentWidget(GUIEditorID);
								}
						} else {
								ChooserPanel.transform.localPosition = Vector3.Lerp(ChooserPanel.transform.localPosition, ChooserClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
								QuickCreatePanel.transform.localPosition = Vector3.Lerp(QuickCreatePanel.transform.localPosition, QuickCreateClosedPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime)); 
						}

						#if UNITY_EDITOR
						if (VRManager.VRMode | VRManager.VRTestingMode) {
						#else
						if (VRManager.VRMode) {
						#endif
								ShadowObject.SetActive(false);
						} else {
								ShadowObject.SetActive(true);
								ShadowObject.transform.localPosition = Vector3.Lerp(ShadowObject.transform.localPosition, ShadowTargetPosition, (float)(SpeedMultiplier * Frontiers.WorldClock.RTDeltaTime));
						}
				}

				protected IEnumerator WaitATickAfterChoosing(GameObject currentChooser)
				{
						yield return null;
						OnChooseTarget.SendMessage(OnChooseMessage, LastChoiceMade);
						while (!LastChoiceConfirmed) {
								yield return null;
						}
						MakingChoice = false;
						TargetPosition = ChooserClosedPosition;
						double waitUntil = WorldClock.RealTime + 1f;
						while (WorldClock.RealTime < waitUntil) {
								yield return null;
						}
						currentChooser.transform.localPosition = ChooserClosedPosition;
						yield break;
				}
		}
}
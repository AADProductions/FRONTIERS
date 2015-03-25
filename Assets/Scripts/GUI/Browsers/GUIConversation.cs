using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Story;
using Frontiers.World.Gameplay;
using Frontiers.Story.Conversations;

namespace Frontiers.GUI
{
		public class GUIConversation : PrimaryInterface
		{
				public UIAnchor StatusKeepersAnchor;
				public UIAnchor TextAnchor;
				public Vector2 TextAnchorOffsetVR = new Vector2(0.18f, 0f);
				public Vector2 TextAnchorOffset = Vector2.zero;
				public GameObject PlayerDialogChoicePrototype;
				public GameObject ConversationVariablePrefab;
				public GameObject PlayerDialogChoicesParent;
				public GameObject CharacterResponseParent;
				public GameObject StatusKeeperParent;
				public UIGrid ConversationVariableParent;
				public UIScrollBar PlayerDialogChoicesScrollbar;
				public UIScrollBar CharacterResponseScrollbar;
				public UILabel ConversationFinishedLabel;
				public UILabel DebugLabel;
				public UIButton LeaveButton;
				public int CurrentPageNumber = 0;
				public int LastPageNumber = 0;
				public Color CharacterColor;
				public Color CharacterTextColor;
				public int ReputationChange = 0;
				public float CameraCenterOffset = 0.1f;
				public float CameraFaceOffset = 1f;
				public float StatusKeeperScaleMin = 10f;
				public float StatusKeeperScaleMax = 20f;
				public Vector3 CameraHeadOffset = new Vector3(0f, -0.5f, 0f);
				public Vector3 ResponseScrollingPanel = new Vector3(50f, -6f, -75f);
				public Light HeadLight;

				public Conversation Conv {
						get {
								return Conversation.LastInitiatedConversation;
						}
				}

				public GUIConversationBubble ResponseBubble;
				public List <NGUIConversationVariable> Variables = new List <NGUIConversationVariable>();
				public List <GUIStatusKeeper> StatusKeepers = new List <GUIStatusKeeper>();
				public Vector4 DialogPanelClipping;
				public float TargetOffset = -1.0f;

				public override bool CustomVRSettings {
						get {
								return true;
						}
				}

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

				public override Vector3 LockOffset {
						get {
								return Vector3.zero;
						}
				}
				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUIEditorID; }
						//add the player choices but not the character's choice
						FrontiersInterface.GetActiveInterfaceObjectsInTransform(PlayerDialogChoicesParent.transform, NGUICamera, currentObjects, flag);
						//also add the status keepers
						FrontiersInterface.Widget w = new Widget(flag);
						w.SearchCamera = NGUICamera;
						for (int i = 0; i < StatusKeepers.Count; i++) {
								w.BoxCollider = StatusKeepers[i].GetComponent <BoxCollider>();
								currentObjects.Add(w);
						}
						if (LeaveButton.gameObject.activeSelf) {
								w.BoxCollider = LeaveButton.GetComponent <BoxCollider>();		
								currentObjects.Add(w);
						}
				}

				public override Widget FirstInterfaceObject {
						get {
								FrontiersInterface.Widget w = new Widget(-1);
								w.SearchCamera = NGUICamera;
								if (PlayerDialogChoicesParent.transform.childCount == 0) {
										w.BoxCollider = LeaveButton.GetComponent <BoxCollider>();
								} else {
										w.BoxCollider = PlayerDialogChoicesParent.transform.GetChild(0).GetComponent <BoxCollider> ();
								}
								return w;
						}
				}

				public override bool ShowQuickslots {
						get {
								return !Maximized;
						}
						set { }
				}

				public override void Start()
				{
 						DebugLabel.enabled = false;

						base.Start();

						Player.Get.AvatarActions.Subscribe((AvatarAction.NpcConverseStart), new ActionListener(NpcConverseStart));
						Player.Get.AvatarActions.Subscribe((AvatarAction.NpcConverseEnd), new ActionListener(NpcConverseEnd));

						mFocusPoint = new GameObject("ConversationFocusPoint").transform;
						mCameraHelper	= new GameObject("ConversationCameraHelper").transform;

						DontDestroyOnLoad(mFocusPoint);
						DontDestroyOnLoad(mCameraHelper);

						gameObject.SetActive(false);
				}

				public override void Update()
				{
						if (!Maximized)
								return;

						base.Update();

						if (HasFocus) {
								SendCharacterCameraToCharacter();
						}

						if (Conv != null) {

								float maxHeight = 0f;
								for (int i = 0; i < StatusKeepers.Count; i++) {
										maxHeight = Mathf.Max(maxHeight, StatusKeepers[i].MeterSize * StatusKeepers[i].TargetScale);
								}
								StatusKeeperParent.transform.localPosition = new Vector3(0f, maxHeight / 2f, 0f);

								mVaraiblesDisplayed.Clear();
								mVaraiblesDisplayed.AddRange(Conv.DisplayVariables);
								for (int i = Variables.Count - 1; i >= 0; i--) {
										if (Variables[i] == null || !Conv.DisplayVariables.Contains(Variables[i].name)) {
												if (Variables[i] != null) {
														GameObject.Destroy(Variables[i].gameObject);
												}
												Variables.RemoveAt(i);
										} else {
												mVaraiblesDisplayed.Remove(Variables[i].name);
												//update variable
												Variables[i].NormalizedValue = Conv.GetVariableValueNormalized(Variables[i].name);
										}
								}
								if (mVaraiblesDisplayed.Count > 0) {
										foreach (string leftoverVariable in mVaraiblesDisplayed) {
												GameObject newVarGameObject = NGUITools.AddChild(ConversationVariableParent.gameObject, ConversationVariablePrefab);
												newVarGameObject.name = leftoverVariable;
												newVarGameObject.transform.localScale	= ConversationVariablePrefab.transform.localScale;
												NGUIConversationVariable newVar = newVarGameObject.GetComponent <NGUIConversationVariable>();
												newVar.Initialize(Conv.GetVariableValueNormalized(leftoverVariable));
												Variables.Add(newVar);
										}
										ConversationVariableParent.Reposition();
								}

								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
								#else
								if (VRManager.VRMode) {
								#endif
										TextAnchor.relativeOffset = TextAnchorOffsetVR;
								}
								else {
										TextAnchor.relativeOffset = TextAnchorOffset;
								}
						}
				}

				protected List <string> mVaraiblesDisplayed = new List<string>();

				public void MakeDialogChoice(GameObject sender)
				{
						if (!mDialogChoicesReady) {
								Debug.Log("Dialog choices aren't ready yet");
								return;
						}

						GUIPlayerDialogChoice playerDialogChoice = sender.gameObject.GetComponent <GUIPlayerDialogChoice>();
						Exchange choice = playerDialogChoice.Choice;

						if (choice == null) {//null means '(more)'
								ShowNextPage();
								if (Conv.Continues) {
										LeaveButton.gameObject.SetActive(false);
								}
								return;
						}

						string dtsOnFailure;
						Conv.MakeOutgoingChoice(choice, out dtsOnFailure);
						LoadNextExchange();
				}

				public void ShowNextPage()
				{
						CurrentPageNumber++;
						string nextPage = string.Empty;
						bool continues = true;
						Conv.GetPage(out nextPage, CurrentPageNumber, ref continues, ref LastPageNumber);

						ResponseBubble.FadeDelay = 0.05f;
						ResponseBubble.EnableAutomatically = true;
						ResponseBubble.SetProps(nextPage, Conv.SpeakingCharacter.worlditem.DisplayName, CharacterColor, CharacterTextColor, true, CharacterResponseScrollbar);
						ResponseBubble.Text.useDefaultLabelFont = true;

						if (continues) {
								mDialogChoicesReady = true;
								ClearDialogChoices();
								CreateNextPageChoice();
						} else {
								mDialogChoicesReady = false;
								LastPageNumber = 0;
								ClearDialogChoices();
								CreateDialogChoices();
						}

						RefreshLeaveButton();
				}

				public void RefreshLeaveButton()
				{
						if (Conv.Continues || CurrentPageNumber < LastPageNumber) {
								LeaveButton.gameObject.SetActive(false);
								ConversationFinishedLabel.enabled = false;
						} else if (Conv.CanLeave) {
								LeaveButton.gameObject.SetActive(true);
								ConversationFinishedLabel.enabled = true;
								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingModeEnabled | Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								#else
								if (VRManager.VRMode | Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
								#endif
										GUICursor.Get.SelectWidget (FirstInterfaceObject);
								}
						}
				}

				public void LoadNextExchange()
				{
						CharacterTextColor = Colors.Get.MenuButtonTextColorDefault;
						ClearDialogChoices();
						CurrentPageNumber = -1;
						ShowNextPage();
				}

				public void OnClickLeaveButton()
				{
						if (TryToLeave()) {
								Conversation.LastInitiatedConversation.End();
						}
				}

				protected void CreateNextPageChoice()
				{
						GameObject newPlayerDialogChoiceGameObject = NGUITools.AddChild(PlayerDialogChoicesParent, PlayerDialogChoicePrototype);
						GUIPlayerDialogChoice playerDialogChoice = newPlayerDialogChoiceGameObject.GetComponent <GUIPlayerDialogChoice>();

						playerDialogChoice.Choice = null;
						playerDialogChoice.ChoiceButtonMessage.target = gameObject;
						playerDialogChoice.ChoiceButtonMessage.functionName	= "MakeDialogChoice";
						playerDialogChoice.gameObject.name = "Choice_" + (-1).ToString();
						playerDialogChoice.transform.localPosition = new Vector3(0f, -250f, 0f);

						GUIConversationBubble bubble = playerDialogChoice.GetComponent <GUIConversationBubble>();
						bubble.EnableAutomatically = true;
						bubble.FadedColor = false;
						bubble.FadeDelay = 0.05f;
						bubble.SetProps("(More)", Colors.Get.ConversationPlayerBackground, Colors.Get.ConversationPlayerOption);
						playerDialogChoice.Offset = 0.0f;
				}

				protected IEnumerator WaitForDialogChoicesToBeReady(List<GUIConversationBubble> choices)
				{
						while (!mDialogChoicesReady) {
								if (!Maximized) {
										yield break;
								}
								yield return null;
								mDialogChoicesReady = true;
								for (int i = 0; i < choices.Count; i++) {
										if (choices[i] == null || choices[i].IsDestroying) {
												//something's gone wrong, exit now
												yield break;
										} else {
												if (!choices[i].ReadyToBeClicked) {
														mDialogChoicesReady = false;
												}
										}
								}
								if (mDialogChoicesReady) {
										break;
								}
						}
						yield return null;
						for (int i = 0; i < choices.Count; i++) {
								choices[i].Collider.enabled = true;
						}
						choices.Clear();
						//if we're in VR mode, move the mouse to the best position
						#if UNITY_EDITOR
						if (VRManager.VRMode | VRManager.VRTestingModeEnabled | Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
						#else
						if (VRManager.VRMode | Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts) {
						#endif
							GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						yield break;
				}

				protected bool mDialogChoicesReady = false;

				protected void CreateDialogChoices()
				{
						if (Conv.Continues) {
								float offset = 0.0f;
								float fadeDelay = 0.05f;
								List <Exchange> RunningOutgoingChoices = Conv.RunningOutgoingChoices;
								List <GUIConversationBubble> choices = new List<GUIConversationBubble>();
								foreach (Exchange outgoingChoice in RunningOutgoingChoices) {
										GameObject newPlayerDialogChoiceGameObject = NGUITools.AddChild(PlayerDialogChoicesParent, PlayerDialogChoicePrototype);
										GUIPlayerDialogChoice playerDialogChoice = newPlayerDialogChoiceGameObject.GetComponent <GUIPlayerDialogChoice>();

										playerDialogChoice.Choice = outgoingChoice;
										playerDialogChoice.ChoiceButtonMessage.target = gameObject;
										playerDialogChoice.ChoiceButtonMessage.functionName	= "MakeDialogChoice";
										playerDialogChoice.gameObject.name = "Choice_" + outgoingChoice.Name;

										GUIConversationBubble bubble = playerDialogChoice.GetComponent <GUIConversationBubble>();
										bubble.FadedColor = (outgoingChoice.NumTimesChosen > 0);
										bubble.FadeDelay = fadeDelay;
										bubble.EnableAutomatically = false;
										playerDialogChoice.Offset = offset;
										bubble.SetProps(outgoingChoice.CleanPlayerDialog, Colors.Get.ConversationPlayerBackground, Colors.Get.ConversationPlayerOption);
										offset -= bubble.Height;
										fadeDelay += 0.05f;
										choices.Add(bubble);
								}
								PlayerDialogChoicesScrollbar.scrollValue = 0.0f;
								PlayerDialogChoicesScrollbar.ForceUpdate();
								StartCoroutine(WaitForDialogChoicesToBeReady(choices));
						}
				}

				protected void ClearDialogChoices()
				{
						List<Transform> childrenToMove = new List<Transform>();
						foreach (Transform child in PlayerDialogChoicesParent.transform) {
								childrenToMove.Add(child);
						}
						foreach (Transform childToMove in childrenToMove) {
								GUIConversationBubble bubble = childToMove.GetComponent<GUIConversationBubble>();
								bubble.DestroyBubble();
								childToMove.parent = childToMove.parent.parent;
						}
						PlayerDialogChoicesScrollbar.scrollValue = 0f;
						PlayerDialogChoicesScrollbar.ForceUpdate();
				}

				protected bool TryToLeave()
				{
						if (Conv == null || (!Conv.Continues && Conv.CanLeave)) {
								foreach (NGUIConversationVariable variable in Variables) {
										GameObject.Destroy(variable.gameObject);
								}
								Variables.Clear();
								return true;
						} else {
								GUIManager.PostWarning("You can't leave in the middle of a conversation");
								return false;
						}
				}

				public override bool ActionCancel(double timeStamp)
				{
						if (TryToLeave()) {
								Conv.End();
						}
						return true;
				}

				public bool NpcConverseStart(double timeStamp)
				{
						OnConversationStart();
						return true;
				}

				public bool NpcConverseEnd(double timeStamp)
				{
						OnConversationEnd();
						return true;
				}

				protected void OnConversationStart()
				{
						HoldFocus = true;

						if (!base.Maximize()) {
								Debug.Log("Couldn't start conversation for some reason");
								return;
						}

						RefreshLeaveButton();
					
						SuspendMessages = true;

						StatusKeepersAnchor.gameObject.SetActive(true);
						PlayerDialogChoicesParent.SetActive(true);
						CharacterResponseParent.SetActive(true);

						TargetOffset = 0.0f;

						SendCharacterCameraToCharacter();

						CharacterResponseScrollbar.scrollValue = 0.5f;

						CharacterColor = Colors.Saturate(Colors.ColorFromString(Conv.SpeakingCharacter.FullName, 125));
						//set the rep color in load next exchange, since it won't change
						mDialogChoicesReady = false;
						StartCoroutine(WaitForConversationToInitiate());
				}

				protected IEnumerator WaitForConversationToInitiate()
				{
						while (Conv.Initiating) {
								yield return null;
						}

						//create the reputation status keepers
						int statusKeeperIndex = -1;
						GUIStatusKeeper lastStatusKeeper = null;
						for (int i = 0; i < Player.Local.Status.StatusKeepers.Count; i++) {
								StatusKeeper sk = Player.Local.Status.StatusKeepers[i];
								if (sk.ShowInConversationInterface) {
										statusKeeperIndex++;
										GameObject newGUIStatusKeeperGameObject = NGUITools.AddChild(StatusKeeperParent, GUIPlayerStatusInterface.Get.GUIStatusKeeperPrefab);
										GUIStatusKeeper newGUIStatusKeeper = newGUIStatusKeeperGameObject.GetComponent <GUIStatusKeeper>();
										newGUIStatusKeeper.Initialize(sk, lastStatusKeeper, statusKeeperIndex, 2f);
										newGUIStatusKeeper.PositionScale = -1f;
										newGUIStatusKeeper.TargetScaleMin = StatusKeeperScaleMin;
										newGUIStatusKeeper.TargetScaleMax = StatusKeeperScaleMax;
										StatusKeepers.Add(newGUIStatusKeeper);
										lastStatusKeeper = newGUIStatusKeeper;
								}
						}

						yield return null;
						LoadNextExchange();

						yield break;
				}

				protected float GetCharacterReputation()
				{
						if (!HasFocus) {
								return 0f;
						}
						return ReputationState.NormalizeRep(Profile.Get.CurrentGame.Character.Rep.GetPersonalReputation(Conversation.LastInitiatedConversation.SpeakingCharacter.State.Name.FileName));
				}

				protected void OnConversationEnd()
				{
						HoldFocus = false;
						SuspendMessages = false;

						for (int i = 0; i < StatusKeepers.Count; i++) {
								GameObject.Destroy(StatusKeepers[i].gameObject);
						}
						StatusKeepers.Clear();

						if (HeadLight != null) {
								GameObject.Destroy(HeadLight.gameObject);
						}

						Player.Local.RestoreControl(false);
						TargetOffset = -1.0f;

						base.Minimize();
				}

				public override bool Minimize()
				{
						if (TryToLeave() && base.Minimize()) {
								StatusKeepersAnchor.gameObject.SetActive(false);
								PlayerDialogChoicesParent.SetActive(false);
								CharacterResponseParent.SetActive(false);
								return true;
						}
						return false;
				}

				protected void SendCharacterCameraToCharacter()
				{
						#if UNITY_EDITOR
						if ((VRManager.VRMode | VRManager.VRTestingModeEnabled)) {
						#else
						if (VRManager.VRMode) {
						#endif
						return;
						}

						Transform headTransform = Conversation.LastInitiatedConversation.SpeakingCharacter.Body.Transforms.HeadConvo;
						mCameraHelper.position = headTransform.position;
						mCameraHelper.rotation = headTransform.rotation;
						mFocusPoint.position = headTransform.position + mCameraHelper.forward;
						mFocusPoint.rotation = headTransform.rotation;

						Player.Local.HijackControl();
						Player.Local.State.HijackMode = PlayerHijackMode.LookAtTarget;
						Player.Local.SetHijackTargets(mCameraHelper.gameObject, mCameraHelper.gameObject);
						GameManager.Get.GameCamera.fieldOfView = 60f;

						if (HeadLight == null) {
							Transform lightParent = Conversation.LastInitiatedConversation.SpeakingCharacter.Body.Transforms.HeadTop;
							Transform headLightTransform = lightParent.gameObject.CreateChild("HeadLight");
							HeadLight = headLightTransform.gameObject.AddComponent <Light>();
							HeadLight.type = LightType.Spot;
							HeadLight.spotAngle = 30;
							HeadLight.range = 10;
							HeadLight.intensity = 1f;
							headLightTransform.localRotation = Quaternion.Euler(70f, 200f, 200f);
							headLightTransform.localPosition = new Vector3(0f, 2f, 1f);
						}
				}

				protected Transform mFocusPoint;
				protected Transform mCameraHelper;
				public static bool IsActive	= false;
		}
}

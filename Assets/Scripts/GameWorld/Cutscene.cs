using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers.GUI;

namespace Frontiers
{
		public class Cutscene : MonoBehaviour
		{
				public static Cutscene CurrentCutscene;
				public static GameObject CurrentCutsceneAnchor;
				public static Action OnCutsceneStart;
				public static bool SuspendCutsceneStart = false;
				public Color TerrainColor;
				public float WindIntensity;
				public float ThunderIntensity;
				public float RainIntensity;

				public static bool IsActive {
						get {
								return CurrentCutscene != null;
						}
				}

				public CutsceneState State = CutsceneState.NotStarted;
				public bool UseCutsceneMusic = false;
				[FrontiersAvailableModsAttribute("Music")]
				public string CutsceneMusic = string.Empty;
				public Vector3 AnchorOffset;
				public PlayerHijackMode HijackMode;
				public GameObject CameraLookTarget;
				public GameObject CameraSeat;
				public GameObject Interfaces;
				public GameObject Props;
				public bool ShowCursor = true;
				public bool FreezeApparentTime	= true;
				public float ApparentHourOfDay	= 12f;
				public bool UnfreezeOnFinish	= true;
				public string CameraAnimationStarting;
				public string CameraAnimationFinishing;
				public string LookTargetAnimationStarting;
				public string LookTargetAnimationFinishing;
				public bool WaitingForFade = false;
				public bool FadeIn = true;
				public bool FadeOut = true;
				public Color FadeInColor = Color.black;
				public Color FadeOutColor = Color.black;
				public float FadeInDuration = 2.0f;
				public float FadeOutDuration = 2.0f;
				public string TitleCard = string.Empty;
				public string IntrospectionOnFinished = string.Empty;
				public string MissionToActivate = string.Empty;
				public bool FinishAutomatically	= true;
				public float CameraClipDistance = Globals.ClippingDistanceFar;
				public float CameraClipDistanceNear = Globals.ClippingDistanceNear;
				public float CameraFieldOfView = -1f;
				public bool RequireConfirmationToExit = false;
				public bool RequireConfirmationDuringStart = true;
				public bool RequireConfirmationDuringIdle = true;
				public bool RequireConfirmationDuringEnd = false;
				public string ConfirmationMessage = string.Empty;
				protected Transform mCameraParentOnStart;
				protected float mCameraClipDistanceOnStartup;
				protected float mCameraNearClipDistanceOnStartup;

				public void Awake()
				{	
						if (CurrentCutsceneAnchor == null) {
								return;
						}
						//there can only be one cutscene at a time
//			GameObject.Destroy (this);
//			return;
						State = CutsceneState.NotStarted;
						CurrentCutscene = this;
						transform.parent = CurrentCutsceneAnchor.transform;
						transform.localPosition	= AnchorOffset;
						transform.localRotation	= Quaternion.identity;

						mCameraClipDistanceOnStartup = GameManager.Get.GameCamera.farClipPlane;
						mCameraNearClipDistanceOnStartup = GameManager.Get.GameCamera.nearClipPlane;
						mCameraParentOnStart = GameManager.Get.GameCamera.transform.parent;

						GameManager.Get.GameCamera.transform.parent = null;

						OnCutsceneStart.SafeInvoke();
				}

				public static void Interrupt()
				{
						throw new NotImplementedException();
				}

				public void OnDestroy ( ) {
						mDestroyed = true;
				}

				public void Start()
				{
						if (CurrentCutsceneAnchor == null) {
								return;
						}

						Player.Get.UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(ActionCancel));

						if (!mUpdatingCutscene) {
								StartCoroutine(UpdateCutscene());
						}
				}

				public void RefreshCamera()
				{
						Player.Local.State.HijackMode = HijackMode;
						Player.Local.SetHijackTargets(CameraSeat.transform, CameraLookTarget.transform);
						GameManager.Get.GameCamera.farClipPlane = CameraClipDistance;
						GameManager.Get.GameCamera.nearClipPlane = CameraClipDistanceNear;
						if (CameraFieldOfView > 0) {
								GameManager.Get.GameCamera.fieldOfView = CameraFieldOfView;
						}
				}

				public bool ActionCancel (double timeStamp) {
						if (mDestroyed)
								return true;

						if (!(WaitingForFade || SuspendCutsceneStart)) {
								if (RequireConfirmationToExit) {
										bool spawnDialog = false;
										switch (State) {
												case CutsceneState.Idling:
														spawnDialog = RequireConfirmationDuringIdle;
														break;
												case CutsceneState.Finishing:
														spawnDialog = RequireConfirmationDuringEnd;
														break;

												case CutsceneState.Starting:
														spawnDialog = RequireConfirmationDuringStart;
														break;

												default:
														break;
										}

										if (spawnDialog) {
												Debug.Log("Requires confirmation to exit");
												if (mConfirmationDialog == null) {
														Debug.Log("Spawning NGUI manager");
														mConfirmationDialog = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog);
														GUI.YesNoCancelDialogResult result = new GUI.YesNoCancelDialogResult();
														result.CancelButton = false;
														result.Message = ConfirmationMessage;
														result.MessageType = "Skip Cutscene";
														GUIManager.SendEditObjectToChildEditor <GUI.YesNoCancelDialogResult>(new ChildEditorCallback <GUI.YesNoCancelDialogResult>(ConfirmCancel), mConfirmationDialog, result);
												}
										} else {
												OnFinished();
										}
								} else {
										OnFinished();
								}
						}
						return true;
				}

				public void ConfirmCancel(GUI.YesNoCancelDialogResult result, IGUIChildEditor <GUI.YesNoCancelDialogResult> childEditor)
				{
						Debug.Log("Confirm cancel in cutscene");
						if (result.Result == DialogResult.Yes) {
								Debug.Log("Result was yes, finished");
								OnFinished();
						}
						mConfirmationDialog = null;
				}

				public void LateUpdate()
				{
						if (IsActive) {
								RefreshCamera();
						}
				}

				public IEnumerator UpdateCutscene()
				{
						mUpdatingCutscene = true;

						while (SuspendCutsceneStart) {
								yield return null;
						}

						Manager.CutsceneStart();
//						if (FadeIn) {
//								Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, false, 0.0f, 0f, () => {
//										WaitingForFade = false;
//								});
//								WaitingForFade = true;
//								//once the fade out leading into the fade in is ready
//								//start the actual fade in
//								while (WaitingForFade) {
//										yield return null;
//								}
//								Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, true, FadeInDuration);
//						}

						bool fadeTitleCard = !string.IsNullOrEmpty(TitleCard);
						if (fadeTitleCard) {
								StartCoroutine(DisplayTitleCard(TitleCard, FadeInDuration, 1.0f, FadeOutDuration));
						}

						if (!string.IsNullOrEmpty(CameraAnimationStarting)) {
								CameraSeat.animation.Play(CameraAnimationStarting, AnimationPlayMode.Stop);
						}
						if (!string.IsNullOrEmpty(LookTargetAnimationStarting)) {
								CameraLookTarget.animation.Play(LookTargetAnimationStarting, AnimationPlayMode.Stop);
						}
						//get the camera animatinon started before hijacking
						yield return null;

						State = CutsceneState.Starting;
						Player.Local.HijackControl();
						RefreshCamera();
						Player.Local.SnapToHijackedPosition();

						Interfaces.BroadcastMessage("OnCutsceneStart", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneStart", SendMessageOptions.DontRequireReceiver);
						if (!string.IsNullOrEmpty(CameraAnimationStarting)) {
								while (CameraSeat.animation[CameraAnimationStarting].normalizedTime < 1f) {
										yield return null;
								}
						}

						State = CutsceneState.Idling;
						RefreshCamera();
						Interfaces.BroadcastMessage("OnCutsceneIdleStart", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneIdleStart", SendMessageOptions.DontRequireReceiver);

						if (!FinishAutomatically) {
								while (State == CutsceneState.Idling) {
										RefreshCamera();
										yield return null;
								}
						}

						Interfaces.BroadcastMessage("OnCutsceneIdleEnd", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneIdleEnd", SendMessageOptions.DontRequireReceiver);

						State = CutsceneState.Finishing;
						if (!string.IsNullOrEmpty(CameraAnimationFinishing)) {
								while (CameraSeat.animation[CameraAnimationFinishing].normalizedTime < 1f) {
										yield return null;
								}
						}

						if (FadeOut) {
								Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, false, FadeOutDuration, 0f, () => {
										WaitingForFade = false;
								});
								WaitingForFade = true;
								while (WaitingForFade) {
										yield return null;
								}
								Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, true, 0.5f);
						}

						State = CutsceneState.Finished;
						OnFinished();
						yield break;

				}

				public void TryToFinish()
				{
						if (State == CutsceneState.Idling) {
								State = CutsceneState.Finishing;
								if (!string.IsNullOrEmpty(CameraAnimationFinishing)) {
										CameraSeat.animation.Play(CameraAnimationFinishing, AnimationPlayMode.Stop);
								}
								if (!string.IsNullOrEmpty(LookTargetAnimationFinishing)) {
										CameraLookTarget.animation.Play(LookTargetAnimationStarting, AnimationPlayMode.Stop);
								}
						}
				}

				protected IEnumerator DisplayTitleCard(string titleCardText, float fadeInTime, float holdTime, float fadeOutTime)
				{
						UILabel titleCardLabel = GUIManager.Get.TitleCardLabel;
						titleCardLabel.text = titleCardText;
						titleCardLabel.enabled = true;
						titleCardLabel.alpha = 0f;
						double startTime = WorldClock.RealTime;
						float normalizedFadeTime = 0f;
						while (normalizedFadeTime < 1f) {
								normalizedFadeTime = (float)((WorldClock.RealTime - startTime) / fadeInTime);
								titleCardLabel.alpha = normalizedFadeTime;

								if (GUIManager.Get.HasActiveSecondaryInterface) {
										titleCardLabel.alpha = 0f;
										titleCardLabel.enabled = false;
										yield break;
								}

								yield return null;
						}
						titleCardLabel.alpha = 1.0f;
						startTime = WorldClock.RealTime;
						while (WorldClock.RealTime < startTime + holdTime) {

								if (GUIManager.Get.HasActiveSecondaryInterface) {
										titleCardLabel.alpha = 0f;
										titleCardLabel.enabled = false;
										yield break;
								}

								yield return null;
						}
						startTime = WorldClock.RealTime;
						normalizedFadeTime = 0f;
						while (normalizedFadeTime < 1f) {
								normalizedFadeTime = (float)((WorldClock.RealTime - startTime) / fadeOutTime);
								titleCardLabel.alpha = 1.0f - normalizedFadeTime;

								if (GUIManager.Get.HasActiveSecondaryInterface) {
										titleCardLabel.alpha = 0f;
										titleCardLabel.enabled = false;
										yield break;
								}

								yield return null;
						}
						titleCardLabel.alpha = 0f;
						titleCardLabel.enabled = false;
						yield break;
				}

				protected void OnFinished()
				{
						Cutscene.CurrentCutsceneAnchor.SendMessage("OnCutsceneFinished", SendMessageOptions.DontRequireReceiver);
						Player.Local.RestoreControl(false);
						if (!string.IsNullOrEmpty(IntrospectionOnFinished)) {
								if (!string.IsNullOrEmpty(MissionToActivate)) {
										GUIManager.PostIntrospection(IntrospectionOnFinished);
								} else {
										GUIManager.PostIntrospection(IntrospectionOnFinished, MissionToActivate, 0.25f);
										MissionToActivate = string.Empty;
								}
						}
						if (!string.IsNullOrEmpty(MissionToActivate)) {
								Missions.Get.ActivateMission(MissionToActivate, Frontiers.MissionOriginType.None, "Cutscene");
						}
						GUIManager.Get.TitleCardLabel.enabled = false;
						GameManager.Get.GameCamera.farClipPlane = mCameraClipDistanceOnStartup;
						GameManager.Get.GameCamera.nearClipPlane = mCameraNearClipDistanceOnStartup;
						GameManager.Get.GameCamera.transform.parent = mCameraParentOnStart;
						Manager.CutsceneFinished();
				}

				protected bool mUpdatingCutscene = false;
				protected GameObject mConfirmationDialog = null;
				protected bool mDestroyed = false;
		}
}

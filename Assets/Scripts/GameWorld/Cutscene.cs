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
				public static bool StartSuspended = false;
				public Color TerrainColor;
				public float WindIntensity;
				public float ThunderIntensity;
				public float RainIntensity;

				public static bool IsActive {
						get {
								return CurrentCutscene != null;
						}
				}

				public static bool SuspendCutsceneStart()
				{
						if (Cutscene.IsActive && !Cutscene.CurrentCutscene.IgnoreSuspend) {
								StartSuspended = true;
								return true;
						}
						return false;
				}

				public static void Unsuspend()
				{
						if (Cutscene.IsActive) {
								StartSuspended = false;
						}
				}

				public static void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						if (Cutscene.IsActive) {
								for (int i = 0; i < CurrentCutscene.InterfaceList.Count; i++) {
										CurrentCutscene.InterfaceList[i].GetActiveInterfaceObjects(currentObjects, flag);
								}
						}
				}

				public bool HasActiveInterfaces {
						get {
								return InterfaceList.Count > 0;
						}
				}

				public Camera ActiveCamera {
						get {
								if (HasActiveInterfaces) {
										//TODO actually find the active camera
										return InterfaceList[0].NGUICamera;
								}
								return null;
						}
				}

				public CutsceneState State = CutsceneState.NotStarted;
				public bool UseCutsceneMusic = false;
				public bool IgnoreSuspend = false;
				[FrontiersAvailableModsAttribute("Music")]
				public string CutsceneMusic = string.Empty;
				public Vector3 AnchorOffset;
				public PlayerHijackMode HijackMode;
				public GameObject CameraLookTarget;
				public GameObject CameraSeat;
				public GameObject StaticCameraSeatStart;
				public GameObject StaticCameraSeatIdle;
				public GameObject StaticCameraSeatEnd;
				public GameObject Interfaces;
				public GameObject Props;
				public float HoldStaticCameraStart = -1f;
				public float HoldStaticCameraEnd = -1f;
				public List <CutsceneInterface> InterfaceList = new List<CutsceneInterface>();
				public bool ShowCursor = true;
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
				public bool VRMode = false;

				public void Awake()
				{	
						if (CurrentCutsceneAnchor == null) {
								return;
						}
						//there can only be one cutscene at a time
						//GameObject.Destroy (this);
						//return;
						State = CutsceneState.NotStarted;
						CurrentCutscene = this;
						transform.parent = CurrentCutsceneAnchor.transform;
						transform.localPosition	= AnchorOffset;
						transform.localRotation	= Quaternion.identity;

						mCameraClipDistanceOnStartup = GameManager.Get.GameCamera.farClipPlane;
						mCameraNearClipDistanceOnStartup = GameManager.Get.GameCamera.nearClipPlane;

						#if UNITY_EDITOR
						VRMode = (VRManager.VRMode | VRManager.VRTestingModeEnabled && Profile.Get.CurrentPreferences.Video.VRStaticCameraCutscenes);
						#else
						VRMode = (VRManager.VRMode && Profile.Get.CurrentPreferences.Video.VRStaticCameraCutscenes);
						#endif

						if (!VRMode) {
								mCameraParentOnStart = GameManager.Get.GameCamera.transform.parent;
								GameManager.Get.GameCamera.transform.parent = null;
						}

						OnCutsceneStart.SafeInvoke();
				}

				public static void Interrupt()
				{
						throw new NotImplementedException();
				}

				public void OnDestroy()
				{
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

				public void RefreshCamera( )
				{
						//this will get picked up by CameraFX for vr
						GameManager.Get.GameCamera.farClipPlane = CameraClipDistance;
						GameManager.Get.GameCamera.nearClipPlane = CameraClipDistanceNear;
						if (VRMode) {
								//use the static cam alternative
								Player.Local.State.HijackMode = HijackMode;
								switch (this.State) {
										case CutsceneState.NotStarted:
										case CutsceneState.Starting:
										default:
												Player.Local.SetHijackTargets (StaticCameraSeatStart.transform, StaticCameraSeatStart.transform);
												//GameManager.Get.GameCamera.transform.position = StaticCameraSeatStart.transform.position;
												//GameManager.Get.GameCamera.transform.rotation = StaticCameraSeatStart.transform.rotation;
												break;

										case CutsceneState.Idling:
												Player.Local.SetHijackTargets (StaticCameraSeatIdle.transform, StaticCameraSeatIdle.transform);
												//GameManager.Get.GameCamera.transform.position = StaticCameraSeatIdle.transform.position;
												//GameManager.Get.GameCamera.transform.rotation = StaticCameraSeatIdle.transform.rotation;
												break;

										case CutsceneState.Finishing:
										case CutsceneState.Finished:
												Player.Local.SetHijackTargets (StaticCameraSeatEnd.transform, StaticCameraSeatEnd.transform);
												//GameManager.Get.GameCamera.transform.position = StaticCameraSeatEnd.transform.position;
												//GameManager.Get.GameCamera.transform.rotation = StaticCameraSeatEnd.transform.rotation;
												break;
								}
						} else {
								Player.Local.State.HijackMode = HijackMode;
								Player.Local.SetHijackTargets(CameraSeat.transform, CameraLookTarget.transform);
								if (CameraFieldOfView > 0) {
										GameManager.Get.GameCamera.fieldOfView = CameraFieldOfView;
								}
						}
				}

				public bool ActionCancel(double timeStamp)
				{
						if (mDestroyed)
								return true;

						if (!(WaitingForFade || StartSuspended)) {
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
														//see if our top interface is using custom vr settings
														if (VRManager.VRMode && HasActiveInterfaces) {
																GUIYesNoCancelDialog dialog = mConfirmationDialog.GetComponent <GUIYesNoCancelDialog>();
																dialog.CustomVRSettings = InterfaceList[0].CustomVRSettings;
																dialog.AxisLock = InterfaceList[0].AxisLock;
																dialog.CursorLock = InterfaceList[0].CursorLock;
																dialog.LockOffset = InterfaceList[0].LockOffset;
														}
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
						if (result.Result == DialogResult.Yes) {
								OnFinished();
						}
						mConfirmationDialog = null;
				}

				public void LateUpdate()
				{
						if (IsActive) {
								RefreshCamera( );
						}
				}

				public IEnumerator UpdateCutscene()
				{
						mUpdatingCutscene = true;

						while (StartSuspended) {
								yield return null;
						}

						Manager.CutsceneStart();

						if (FadeIn | VRMode) {
								//we have to fade in for vr mode or we'll get sick
								Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, true, FadeInDuration);
						}

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
						Player.Local.HijackControl();
						RefreshCamera();
						Player.Local.SnapToHijackedPosition();
						yield return null;

						State = CutsceneState.Starting;

						Interfaces.BroadcastMessage("OnCutsceneStart", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneStart", SendMessageOptions.DontRequireReceiver);

						double startTime = WorldClock.RealTime;
						float startDuration = 0.01f;
						//if we're in vr mode and we have a specified hold period, use that for timing
						if (VRMode) {
								//this will end up waiting for zero seconds if there's no intro stuff set, which is fine
								if (HoldStaticCameraStart >= 0) {
										startTime += HoldStaticCameraStart;
								} else if (!string.IsNullOrEmpty(CameraAnimationStarting)) {
										startTime += CameraSeat.animation[CameraAnimationStarting].length;
								}
						} else {
								//otherwise just use the camera animation
								if (!string.IsNullOrEmpty(CameraAnimationStarting)) {
										startTime += CameraSeat.animation[CameraAnimationStarting].length;
								}
						}

						while (WorldClock.RealTime < (startTime + startDuration)) {
								yield return null;
						}

						State = CutsceneState.Idling;

						if (VRMode && StaticCameraSeatStart != StaticCameraSeatIdle) {
								//we have to fade out/in before switching to a new static camera
								mWaitingForStaticFade = true;
								Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), false, 0.5f, 0f, () => {
										mWaitingForStaticFade = false;
								});
								while (mWaitingForStaticFade) {
										yield return null;
								}
								mWaitingForStaticFade = true;
								Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), false, 0.5f, 0f, () => {
										mWaitingForStaticFade = false;
								});
								while (mWaitingForStaticFade) {
										yield return null;
								}
						}

						RefreshCamera();
						Interfaces.BroadcastMessage("OnCutsceneIdleStart", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneIdleStart", SendMessageOptions.DontRequireReceiver);

						if (!FinishAutomatically) {
								while (State == CutsceneState.Idling) {
										yield return null;
								}
						}

						Interfaces.BroadcastMessage("OnCutsceneIdleEnd", SendMessageOptions.DontRequireReceiver);
						Props.BroadcastMessage("OnCutsceneIdleEnd", SendMessageOptions.DontRequireReceiver);

						State = CutsceneState.Finishing;

						if (VRMode && StaticCameraSeatIdle != StaticCameraSeatEnd) {
								//we have to fade out/in before switching to a new static camera
								mWaitingForStaticFade = true;
								Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), false, 0.5f, 0f, () => {
										mWaitingForStaticFade = false;
								});
								while (mWaitingForStaticFade) {
										yield return null;
								}
								mWaitingForStaticFade = true;
								Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), false, 0.5f, 0f, () => {
										mWaitingForStaticFade = false;
								});
								while (mWaitingForStaticFade) {
										yield return null;
								}
						}

						RefreshCamera();
						//if we're in vr mode and we have a specified hold period, use that for timing
						double endStartTime = WorldClock.RealTime;
						float endDuration = 0.01f;
						if (VRMode) {
								if (HoldStaticCameraStart >= 0) {
										endDuration = HoldStaticCameraEnd;
								} else if (!string.IsNullOrEmpty(CameraAnimationFinishing)) {
										endDuration = CameraSeat.animation[CameraAnimationFinishing].length;
								}
								//in vr mode we need at least 1 second to fade out
								endDuration = Mathf.Min(1f, endDuration);
						} else {
								//otherwise just use the camera animation
								if (!string.IsNullOrEmpty(CameraAnimationFinishing)) {
										endDuration = CameraSeat.animation[CameraAnimationFinishing].length;
								}
						}

						//see if we need to start fading out before the animation is done
						double fadeOutStartTime = -1f;
						bool shouldFade = false;
						if (FadeOut | VRMode) {
								//we HAVE to fade out for vr mode or we'll get sick
								shouldFade = true;
								FadeOutDuration = Mathf.Min(FadeOutDuration, endDuration);
								fadeOutStartTime = endStartTime + endDuration - FadeOutDuration;
						}

						while (WorldClock.RealTime < (endStartTime + endDuration)) {
								//see if we're supposed to start fading
								if (shouldFade && WorldClock.RealTime > fadeOutStartTime) {
										Debug.Log("Starting fade out");
										shouldFade = false;
										Frontiers.GUI.CameraFade.StartAlphaFade(FadeOutColor, false, FadeOutDuration);
								}
								yield return null;
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
						if (!VRManager.VRMode) {
								GameManager.Get.GameCamera.transform.parent = mCameraParentOnStart;
						}
						Manager.CutsceneFinished();
				}

				protected bool mWaitingForStaticFade = false;
				protected bool mUpdatingCutscene = false;
				protected GameObject mConfirmationDialog = null;
				protected bool mDestroyed = false;
		}
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;
//using Ovr;
using System.IO;

namespace Frontiers
{
		public class VRManager : Manager, IFrontiersInterface
		{
				public static VRManager Get;

				public Camera NGUICamera {
						get {
								if (mProxyCamera != null) {
										return mProxyCamera;
								}
								return GUIManager.Get.BaseCamera;
						}
						set {
								return;
						}
				}

				public bool VRSettingsOverride { get; set; }

				public int GUIEditorID {
						get {
								if (mGUIEditorID < 0) {
										mGUIEditorID = GUIManager.GetNextGUIID();
								}
								return mGUIEditorID;
						}
				}

				//public OVRManager OvrManager;
				public GameObject OvrCameraRig;
				//used for gui bounds, doesn't actually render anything
				public Camera OvrCenterCamera;
				public Transform FocusTransform;
				public Transform TrackingSpaceTransform;
				public RenderTexture TargetRenderTexture;
				public Transform VRProxyButtonsParent;
				public Transform DirectionTransform;
				public Transform CursorOffset;
				public bool UsePaperRenderer = false;
				public Renderer PaperRenderer;
				public Renderer PaperRendererLocked;

				public bool CustomVRSettings { get { return true; } }

				public Vector3 ForwardOffset {
						get {
								mForwardOffset.x = 0f;
								mForwardOffset.z = 0f;
								mForwardOffset.y = FocusTransform.localEulerAngles.y;
								return mForwardOffset;
						}
				}

				public Vector3 LockOffset {
						get {
								if (GUILoading.IsLoading) {
										return Vector3.zero;
								} else if (GUIManager.Get.HasActiveInterface) {
										if (GUIManager.Get.TopInterface.CustomVRSettings) {
												return GUIManager.Get.TopInterface.LockOffset;
										} else {
												return mLockOffset;
										}
								} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
										if (Cutscene.CurrentCutscene.InterfaceList[0].CustomVRSettings) {
												return Cutscene.CurrentCutscene.InterfaceList[0].LockOffset;
										} else {
												return mLockOffset;
										}
								} else {
										return Vector3.zero;
										return ForwardOffset;
								}
						}
						set {
								if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.CustomVRSettings) {
										GUIManager.Get.TopInterface.LockOffset = value;
								} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces && Cutscene.CurrentCutscene.InterfaceList[0].CustomVRSettings) {
										Cutscene.CurrentCutscene.InterfaceList[0].LockOffset = value;
								} else {
										mLockOffset = value;
								}
						}
				}

				public bool AxisLock {
						get {
								if (GUILoading.IsLoading) {
										IconColorLockAxis = Colors.Get.VRIconColorForceOn;
										return true;
								} else if (GUIManager.Get.HasActiveInterface) {
										if (GUIManager.Get.TopInterface.CustomVRSettings) {
												if (GUIManager.Get.TopInterface.AxisLock) {
														IconColorLockAxis = Colors.Get.VRIconColorForceOn;
														return true;
												} else {
														IconColorLockAxis = Colors.Get.VRIconColorForceOff;
														return false;
												}
										} else {
												IconColorLockAxis = mAxisLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
												return mAxisLock;
										}
								} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
										if (Cutscene.CurrentCutscene.InterfaceList[0].CustomVRSettings) {
												if (Cutscene.CurrentCutscene.InterfaceList[0].AxisLock) {
														IconColorLockAxis = Colors.Get.VRIconColorForceOn;
														return true;
												} else {
														IconColorLockAxis = Colors.Get.VRIconColorForceOff;
														return false;
												}
										} else {
												IconColorLockAxis = mAxisLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
												return mAxisLock;
										}
								} else {
										IconColorLockAxis = mAxisLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
										return mAxisLock;
								}
						}
						set {
								mAxisLock = value;
						}
				}

				public bool CursorLock {
						get {
								if (GUILoading.IsLoading) {
										IconColorLockCursor = Colors.Get.VRIconColorForceOff;
										return false;
								} else if (GUIManager.Get.HasActiveInterface) {
										if (GUIManager.Get.TopInterface.CustomVRSettings) {
												if (GUIManager.Get.TopInterface.CursorLock) {
														IconColorLockCursor = Colors.Get.VRIconColorForceOn;
														return true;
												} else {
														IconColorLockCursor = Colors.Get.VRIconColorForceOff;
														return false;
												}
										} else {
												IconColorLockCursor = mCursorLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
												return mCursorLock;
										}
								} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
										if (Cutscene.CurrentCutscene.InterfaceList[0].CustomVRSettings) {
												if (Cutscene.CurrentCutscene.InterfaceList[0].CursorLock) {
														IconColorLockCursor = Colors.Get.VRIconColorForceOn;
														return true;
												} else {
														IconColorLockCursor = Colors.Get.VRIconColorForceOff;
														return false;
												}
										} else {
												IconColorLockCursor = mCursorLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
												return mCursorLock;
										}
								} else {
										IconColorLockCursor = mCursorLock ? Colors.Get.VRIconColorOn : Colors.Get.VRIconColorOff;
										return mCursorLock;
								}
						}
						set {
								mCursorLock = value;
						}
				}

				public float QuadZOffset {
						get {
								float quadZOffset = DefaultRenderQuadOffset.z;
								if (!GUILoading.IsLoading) {
										if (GUIManager.Get.HasActiveInterface && GUIManager.Get.TopInterface.CustomVRSettings) {
												quadZOffset = GUIManager.Get.TopInterface.QuadZOffset;
										} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces && Cutscene.CurrentCutscene.InterfaceList[0].CustomVRSettings) {
												quadZOffset = Cutscene.CurrentCutscene.InterfaceList[0].QuadZOffset;
										}
								}
								return quadZOffset + Profile.Get.CurrentPreferences.Video.VRQuadZOffset;
						}
				}

				public MeshRenderer RenderQuad;
				public MeshRenderer LockedRenderQuad;
				public MeshRenderer LockedCursorSprite;
				public MeshRenderer FadeQuad;
				public Transform RenderParent;
				public Transform CursorTargetPosition;
				public UIButtonMessage AxisLockButton;
				public UIButtonMessage ResetCameraButton;
				public UIButtonMessage CursorLockButton;
				public UIButtonMessage ReorientButton;
				public MeshRenderer AxisLockRenderer;
				public MeshRenderer ResetCameraRenderer;
				public MeshRenderer CursorLockButtonRenderer;
				public MeshRenderer ReorientButtonRenderer;
				public float YRotationOffset;
				public Quaternion RotationOffset;
				public Vector3 VRButtonsProxyOffset = new Vector3(0f, -200f, 0f);
				public Vector3 DefaultRenderQuadOffset = new Vector3(0f, -0.15f, 0.65f);
				public Vector3 RenderQuadOffset = new Vector3(0f, -0.15f, 0.65f);
				public Vector3 CursorTargetOffset;
				public Vector3 RenderQuadTarget;
				protected Color IconColorLockAxis;
				protected Color IconColorLockCursor;

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						if (flag < 0) {
								flag = GUIEditorID;
						}

						FrontiersInterface.Widget w = new FrontiersInterface.Widget(GUIEditorID);
						w.SearchCamera = NGUICamera;

						w.BoxCollider = AxisLockButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
						w.BoxCollider = ResetCameraButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
						w.BoxCollider = ReorientButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
						w.BoxCollider = CursorLockButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
				}

				public FrontiersInterface.Widget FirstInterfaceObject {
						get {
								FrontiersInterface.Widget w = new FrontiersInterface.Widget(GUIEditorID);
								w.SearchCamera = NGUICamera;
								w.BoxCollider = AxisLockRenderer.GetComponent <BoxCollider>();
								return w;
						}
				}

				public override void WakeUp()
				{
						base.WakeUp();

						Get = this;
						DirectionTransform = new GameObject("DirectionTransform").transform;
						CursorLock = true;

			//TODO UNITY 5
						//OvrManager.resetTrackerOnLoad = true;
						gOculusModeEnabled = false;

						DetectDirectToRiftMode();
				}

				public override void OnLocalPlayerSpawn()
				{
						ResetCameraForward();
						ResetInterfacePosition();
				}

				public override void Initialize()
				{
			//TODO UNITY 5
						VRMode = false;
						/*DirectionTransform.parent = OvrCameraRig.centerEyeAnchor;
						DirectionTransform.localPosition = Vector3.zero;
						DirectionTransform.localRotation = Quaternion.identity;

						AxisLockButton.target = gameObject;
						AxisLockButton.functionName = "OnClickVRLockAxisButton";
						ResetCameraButton.target = gameObject;
						ResetCameraButton.functionName = "OnClickResetCameraView";
						ReorientButton.target = gameObject;
						ReorientButton.functionName = "OnClickVRReorientButton";
						CursorLockButton.target = gameObject;
						CursorLockButton.functionName = "OnClickVRCursorLockButton";

						OvrCenterCamera = OvrCameraRig.centerEyeAnchor.gameObject.AddComponent <Camera>();
						OvrCenterCamera.enabled = false;
						OvrCenterCamera.orthographic = false;
						OvrCenterCamera.cullingMask = 0;
						OvrCenterCamera.eventMask = 0;
						OvrCenterCamera.nearClipPlane = 0.001f;
						OvrCenterCamera.farClipPlane = 1f;

						//TODO is this necessary? OVR probably doesn't work like this
						Rect left = CameraFX.Get.OvrLeft.cam.rect;
						Rect right = CameraFX.Get.OvrRight.cam.rect;
						Rect center = new Rect();
						center.center = Vector2.Lerp(left.center, right.center, 0.5f);
						center.min = Vector2.Lerp(left.min, right.min, 0.5f);
						center.max = Vector2.Lerp(left.max, right.max, 0.5f);

						OvrCenterCamera.aspect = CameraFX.Get.OvrLeft.cam.aspect;
						FocusTransform = OvrCenterCamera.transform;

						PaperRenderer.enabled = false;
						PaperRendererLocked.enabled = false;

						base.Initialize();

						if (VRMode) {
								Debug.Log("Refreshing settings in vr manager");
								RefreshSettings(true);
						} else {
								Debug.Log("NOT refreshing settings in vr manager");
						}*/

			base.Initialize();
				}

				public void OnClickResetCameraView()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
						ResetCameraOrientation(0f);
				}

				public void OnClickVRLockAxisButton()
				{
						mAxisLock = !mAxisLock;
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
				}

				public void OnClickVRReorientButton()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
						ResetInterfacePosition();
				}

				public void OnClickVRCursorLockButton()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ButtonMouseNavigate");
						mCursorLock = !mCursorLock;
				}
				#if UNITY_EDITOR
				public static bool VRTestingMode = false;
				#endif
				public static bool VRMode {
						get {
								if (gDirectToRiftMode) {
										return true;
								}

								return gOculusModeEnabled;
						}
						set {
								if (gDirectToRiftMode) {
										Debug.Log("Can't toggle vr mode in direct-to-rift mode");
										return;
								}

								if (Cutscene.IsActive || GUILoading.IsLoading) {
										Debug.Log("Can't toggle vr mode when cutscene is active or while we're loading");
										return;
								}
								gOculusModeEnabled = value;
								#if UNITY_EDITOR
								if (gOculusModeEnabled) {
										VRTestingMode = false;//don't need it
								}
								#endif
								Get.RefreshSettings(true);
						}
				}

				public static bool VRDeviceAvailable {
						get {
				//TODO UNITY 5
								/*if (OVRManager.display != null) {
										return OVRManager.display.isPresent;
								}*/
								return false;
						}
				}

				public void RefreshSettings(bool resetInterface)
				{
						//cutscene interfaces are handled on the fly by the interfaces themselves
						//since they're created / destroyed on the fly
						#if UNITY_EDITOR
						if (VRMode | VRTestingMode) {
						#else
						if (VRMode) {
								#endif
								try {
									if (TargetRenderTexture == null) {
												TargetRenderTexture = RenderTexture.GetTemporary (2048, 1024, 24);//, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default, 2);
												VRManager.Get.RenderQuad.sharedMaterial.mainTexture = TargetRenderTexture;
									}

									Debug.Log("Oculus mode enabled, refreshing settings - reset interface? " + resetInterface.ToString());
									//first set the NGUI atlases to use the correct materials
									Mats.Get.SetNGUIOculusShaders(true);

									GUIManager.Get.PrimaryCamera.targetTexture = TargetRenderTexture;
									GUIManager.Get.PrimaryCamera.clearFlags = CameraClearFlags.SolidColor;
									GUIManager.Get.PrimaryCamera.backgroundColor = Colors.Alpha(Color.black, 0f);

									GUIManager.Get.SecondaryCamera.targetTexture = TargetRenderTexture;
									GUIManager.Get.SecondaryCamera.clearFlags = CameraClearFlags.Depth;

									GUIManager.Get.BaseCamera.targetTexture = TargetRenderTexture;
									GUIManager.Get.BaseCamera.clearFlags = CameraClearFlags.Depth;

									GUIManager.Get.HudCamera.targetTexture = TargetRenderTexture;
									GUIManager.Get.HudCamera.clearFlags = CameraClearFlags.Depth;

									GUILoading.Get.LoadingCamera.targetTexture = TargetRenderTexture;
									GUILoading.Get.LoadingCamera.clearFlags = CameraClearFlags.SolidColor;

									NGUIWorldMap.Get.MapBackgroundCamera.targetTexture = TargetRenderTexture;
									NGUIWorldMap.Get.MapBackgroundCamera.clearFlags = CameraClearFlags.Depth;

									CameraFX.Get.RefreshOculusMode();
								} catch (Exception e) {
										Debug.Log ("Error setting rift mode, proceeding normally: " + e.ToString());
								}

				//TODO UNITY 5
								/*if (!OvrManager.enabled) {
										Debug.Log ("Setting ovr manager & ovr camera rig to enabled");
										OvrManager.enabled = true;
								}
								if (!OvrCameraRig.enabled) {
										OvrCameraRig.enabled = true;
								}*/

								InterfaceActionManager.SoftwareMouse = true;

								if (!gDirectToRiftMode && Profile.Get.HasCurrentProfile) {
										Debug.Log ("Setting oculus mode to true in preferences");
										Profile.Get.CurrentPreferences.Video.OculusMode = true;
								}

								if (resetInterface) {
										AxisLock = true;
										CursorLock = true;
										if (Player.Local != null && Player.Local.Initialized) {
												ResetCameraOrientation(Player.Local.FPSCameraSeat.eulerAngles.y);
										} else {
												ResetCameraOrientation(0f);
										}
										ResetInterfacePosition();
								}

						} else {
								Debug.Log("Oculus mode disabled, refreshing settings");
								if (GameManager.Is(FGameState.InGame)) {
										Player.Local.FPSCameraSeat.localRotation = Quaternion.identity;
										GameManager.Get.GameCamera.transform.localRotation = Quaternion.identity;
										DirectionTransform.ResetLocal();
								}

								RenderQuad.enabled = false;
								LockedRenderQuad.enabled = false;
								LockedCursorSprite.enabled = false;

								try {
									Mats.Get.SetNGUIOculusShaders(false);

									GUIManager.Get.PrimaryCamera.targetTexture = null;
									GUIManager.Get.PrimaryCamera.clearFlags = CameraClearFlags.Depth;

									GUIManager.Get.SecondaryCamera.targetTexture = null;
									GUIManager.Get.SecondaryCamera.clearFlags = CameraClearFlags.Depth;

									GUIManager.Get.BaseCamera.targetTexture = null;
									GUIManager.Get.BaseCamera.clearFlags = CameraClearFlags.Depth;

									GUIManager.Get.HudCamera.targetTexture = null;
									GUIManager.Get.HudCamera.clearFlags = CameraClearFlags.Depth;

									GUILoading.Get.LoadingCamera.targetTexture = null;
									GUILoading.Get.LoadingCamera.clearFlags = CameraClearFlags.Depth;

									NGUIWorldMap.Get.MapBackgroundCamera.targetTexture = null;
									NGUIWorldMap.Get.MapBackgroundCamera.clearFlags = CameraClearFlags.Depth;

									CameraFX.Get.RefreshOculusMode();

										if (TargetRenderTexture != null) {
												RenderTexture.ReleaseTemporary (TargetRenderTexture);
										}
								} catch (Exception e) {
										Debug.Log ("Error setting rift mode, proceeding normally: " + e.ToString());
								}


				//TODO UNITY 5
								
								/*if (OvrManager.enabled) {
									Debug.Log ("Setting ovr manager & ovr camera rig to disabled");
									OvrManager.enabled = false;
								}
								if (OvrCameraRig.enabled) {
									OvrCameraRig.enabled = false;
								}*/

								InterfaceActionManager.SoftwareMouse = false;

								if (Profile.Get.HasCurrentProfile) {
										Profile.Get.CurrentPreferences.Video.OculusMode = false;
								}
						}
				}

				public override void OnCutsceneFinished()
				{
						//if our vr icons are in a cutscene interface, move them just in case
						if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
								VRProxyButtonsParent.parent = GUIManager.Get.NGUIBaseBottomAnchor.transform;
						}
				}

				public void LateUpdate()
				{
						if (!mInitialized)
								return;

						if (VRMode) {
								ResetDirectionTransform();
								if (mRecenteredPoseLastFrame) {
										//set the vr to in front of player
										ResetInterfacePosition();
										mRecenteredPoseLastFrame = false;
								}
								//turn everything off
								AxisLockRenderer.enabled = false;
								ResetCameraRenderer.enabled = false;
								CursorLockButtonRenderer.enabled = false;
								ReorientButtonRenderer.enabled = false;
								LockedCursorSprite.enabled = false;
								PaperRenderer.enabled = false;
								PaperRendererLocked.enabled = false;
								GUICursor.Get.HideSoftwareCursorSprite = false;
								mProxyCamera = null;
								CursorTargetPosition.localPosition = CursorTargetOffset;
								RenderQuadTarget = DefaultRenderQuadOffset;

								bool highlightAxisLock = false;
								bool highlightResetCam = false;
								bool highlightReorient = false;
								bool highlightCursorLock = false;
							
								if (GUIManager.Get.HasActiveInterface) {
										mProxyCamera = GUIManager.Get.TopInterface.NGUICamera;
										switch (GUIManager.Get.TopInterface.Type) {
												case InterfaceType.Base:
												default:
														VRProxyButtonsParent.parent = GUIManager.Get.NGUIBaseBottomAnchor.transform;
														break;

												case InterfaceType.Primary:
														VRProxyButtonsParent.parent = GUIManager.Get.NGUIPrimaryBottomAnchor.transform;
														break;

												case InterfaceType.Secondary:
														VRProxyButtonsParent.parent = GUIManager.Get.NGUISecondaryBottomAnchor.transform;
														break;

										}
								} else if (Cutscene.IsActive && Cutscene.CurrentCutscene.HasActiveInterfaces) {
										mProxyCamera = Cutscene.CurrentCutscene.InterfaceList[0].NGUICamera;
										VRProxyButtonsParent.parent = Cutscene.CurrentCutscene.InterfaceList[0].BottomAnchor.transform;
								} else {
										//just in case the cutscene interface is being destroyed
										//don't bother setting proxy cam
										VRProxyButtonsParent.parent = GUIManager.Get.NGUIBaseBottomAnchor.transform;
								}
								//set this regardless
								VRProxyButtonsParent.localPosition = VRButtonsProxyOffset;
								VRProxyButtonsParent.localRotation = Quaternion.identity;

								if (mProxyCamera != null) {
										//turn on our vr mini-icons
										AxisLockRenderer.enabled = true;
										ResetCameraRenderer.enabled = true;
										CursorLockButtonRenderer.enabled = true;
										ReorientButtonRenderer.enabled = true;
										//update the icon colors based on what's highlighted / selected
										highlightAxisLock = (AxisLockButton.gameObject == UICamera.selectedObject);
										highlightResetCam = (ResetCameraButton.gameObject == UICamera.selectedObject);
										highlightReorient = (ReorientButton.gameObject == UICamera.selectedObject);
										highlightCursorLock = (CursorLockButton.gameObject == UICamera.selectedObject);

										ResetCameraRenderer.material.color = Color.Lerp(Colors.Get.VRIconColorOff, Colors.Get.GeneralHighlightColor, highlightResetCam ? 0.35f : 0f);
										ReorientButtonRenderer.material.color = Color.Lerp(Colors.Get.VRIconColorOff, Colors.Get.GeneralHighlightColor, highlightReorient ? 0.35f : 0f);
										AxisLockRenderer.material.color = Color.Lerp(IconColorLockAxis, Colors.Get.GeneralHighlightColor, highlightAxisLock ? 0.35f : 0f);
										CursorLockButtonRenderer.material.color = Color.Lerp(IconColorLockCursor, Colors.Get.GeneralHighlightColor, highlightCursorLock ? 0.35f : 0f);
										//update cursor follow
										if (GUICursor.Get.LastSelectedWidgetFlag == GUIEditorID) {
												//disable regular software cursor and use our own cursor
												//because our mini-vr interface is weird and the cursor won't work anyway
												GUICursor.Get.HideSoftwareCursorSprite = true;
												LockedCursorSprite.enabled = true;
												if (highlightAxisLock) {
														LockedCursorSprite.transform.position = AxisLockRenderer.transform.position + (AxisLockRenderer.transform.forward * -0.01f);
														LockedCursorSprite.transform.rotation = AxisLockRenderer.transform.rotation;
												} else if (highlightResetCam) {
														LockedCursorSprite.transform.position = ResetCameraRenderer.transform.position + (ResetCameraRenderer.transform.forward * -0.01f);
														LockedCursorSprite.transform.rotation = ResetCameraRenderer.transform.rotation;
												} else if (highlightReorient) {
														LockedCursorSprite.transform.position = ReorientButtonRenderer.transform.position + (ReorientButtonRenderer.transform.forward * -0.01f);
														LockedCursorSprite.transform.rotation = ReorientButtonRenderer.transform.rotation;
												} else {
														LockedCursorSprite.transform.position = CursorLockButtonRenderer.transform.position + (CursorLockButtonRenderer.transform.forward * -0.01f);
														LockedCursorSprite.transform.rotation = CursorLockButtonRenderer.transform.rotation;
												}
										} else {
												//only update our cursor lock if we're not focusing on our mini-vr interface
												if (CursorLock) {
														GUICursor.Get.HideSoftwareCursorSprite = true;
														LockedCursorSprite.enabled = true;
														LockedCursorSprite.transform.localPosition = -DefaultRenderQuadOffset;
														LockedCursorSprite.transform.localScale = Vector3.one * 0.055f;
														//the goal is for the cursor to always end up in the center at CursorTarget
														//get the local position of the cursor within the gui parent
														Vector3 cursorPosition = RenderParent.InverseTransformPoint(GUICursor.Get.SoftwareCursorSprite.transform.position);
														//and get the local position of the cursor target, which is already parented under RenderParent
														//apply the difference to the render quad
														//then adjust the local z pos to make sure everything's good
														Vector3 adjustedPosition = CursorTargetPosition.localPosition - cursorPosition;
														//do a sanity check - if the adjusted position is really far away from center
														//that means we need to recenter
														if (RenderQuadTarget.x > 2048 | RenderQuadTarget.y > 1024) {
																ResetInterfacePosition();
																RenderQuadTarget = DefaultRenderQuadOffset;
														} else {
																RenderQuadTarget = adjustedPosition;
														}
												} else {
														GUICursor.Get.HideSoftwareCursorSprite = false;
														LockedCursorSprite.enabled = false;
												}
										}
								} else {
										//reset the target position to the default for quickslots etc
										GUICursor.Get.HideSoftwareCursorSprite = false;
										//mProxyCamera = GUIManager.Get.BaseCamera;//OvrCenterCamera
								}

								//get the quad z offset if available
								RenderQuadTarget.z = QuadZOffset;

								if (CursorLock) {
										//show the highlight cursor
										LockedRenderQuad.enabled = true;
										RenderQuad.enabled = false;
										if (UsePaperRenderer) {
												PaperRendererLocked.enabled = true;
										}
										LockedRenderQuad.transform.localPosition = Vector3.Lerp(LockedRenderQuad.transform.localPosition, RenderQuadTarget, 0.5f);
								} else {
										//show the regular cursor
										LockedRenderQuad.enabled = false;
										RenderQuad.enabled = true;
										if (UsePaperRenderer) {
												PaperRenderer.enabled = true;
										}
										RenderQuad.transform.localPosition = Vector3.Lerp(RenderQuad.transform.localPosition, RenderQuadTarget, 0.5f);
								}

								if (AxisLock) {
										if (RenderParent.parent != FocusTransform) {
												RenderParent.parent = FocusTransform;
												ResetInterfacePosition();
										}
										RenderParent.localPosition = Vector3.zero;//-FocusTransform.localPosition;
										RenderParent.localRotation = Quaternion.identity;
								} else {
										if (RenderParent.parent == FocusTransform) {
												RenderParent.parent = TrackingSpaceTransform;
												ResetInterfacePosition();
										}
										RenderParent.localPosition = Vector3.zero;
										RenderParent.localRotation = Quaternion.identity;
								}

								RenderParent.Rotate(LockOffset);

								//set the scale because these occasionally get screwed up for unknown reasons
								//their scale is correct, but they need to be tweaked in the editor before they display right
								//no clue why, probably something to do with parenting / unparenting
								AxisLockRenderer.transform.localScale = Vector3.one * (highlightAxisLock ? 0.075f : 0.065f);
								ReorientButtonRenderer.transform.localScale = Vector3.one * (highlightReorient ? 0.075f : 0.065f);
								CursorLockButtonRenderer.transform.localScale = Vector3.one * (highlightCursorLock ? 0.075f : 0.065f);
								ResetCameraRenderer.transform.localScale = Vector3.one * (highlightResetCam ? 0.075f : 0.065f);
								LockedCursorSprite.transform.localScale = Vector3.one * 0.075f;
								AxisLockRenderer.transform.hasChanged = true;
								ReorientButtonRenderer.transform.hasChanged = true;
								CursorLockButtonRenderer.transform.hasChanged = true;
								ResetCameraRenderer.transform.hasChanged = true;
								LockedCursorSprite.transform.hasChanged = true;

						} else {
								AxisLockRenderer.enabled = false;
								ResetCameraRenderer.enabled = false;
								CursorLockButtonRenderer.enabled = false;
								ReorientButtonRenderer.enabled = false;
								LockedCursorSprite.enabled = false;
								PaperRenderer.enabled = false;
								PaperRendererLocked.enabled = false;
						}
				}

				public void FixedUpdate()
				{
			//TODO UNITY 5
			/*
						if (mResetCameraForwardNextFrame && OVRManager.display != null) {
								ResetCameraForward();
								Debug.Log("refreshing camera settings with reset interface");
								RefreshSettings(true);
								mResetCameraForwardNextFrame = false;
						}
						*/

						if (!mInitialized)
								return;

						if (!gDirectToRiftMode && Profile.Get.HasCurrentProfile) {
								//see if our preferences have changed
								if (Profile.Get.CurrentPreferences.Video.OculusMode != gOculusModeEnabled) {
										VRMode = Profile.Get.CurrentPreferences.Video.OculusMode;
										//Profile.Get.CurrentPreferences.Video.OculusMode = VRMode;
								}
						}
				}

				public void Update()
				{
						if (!mInitialized)
								return;

						//OvrCenterCamera.fieldOfView = CameraFX.Get.OvrLeft.cam.fieldOfView;

						if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftShift)) {
								ResetCameraForward();
						}

						if (Input.GetKeyDown(KeyCode.PageDown)) {
								if (VRDeviceAvailable) {
										Debug.Log("Setting mode to enabled");
										VRMode = true;
								} else {
										Debug.Log("No VR device available");
										//VRTestingMode = true;
										RefreshSettings(true);
								}
						} else if (Input.GetKeyDown(KeyCode.PageUp)) {
								Debug.Log("Setting mode to disabled");
								VRMode = false;
								//VRTestingMode = false;
								RefreshSettings(true);
						}
				}

				public void ResetInterfacePosition()
				{
						if (AxisLock) {
								LockOffset = Vector3.zero;
						} else {
								LockOffset = ForwardOffset;
						}
						InterfaceActionManager.Get.SetMousePosition(1024, 512);
				}

				public void ResetCameraForward()
				{
						if (VRMode) {
								//we're setting our camera seat's rotation to match our camera's rotation
								//then resetting the camera's rotation to zero
								//that will make the new 'forward' direction the direction the camera was facing
								if (Player.Local != null) {
										float yLookRotation = FocusTransform.localEulerAngles.y;
										Vector3 fpsCameraSeatRotation = Player.Local.FPSCameraSeat.localEulerAngles;
										fpsCameraSeatRotation.y = fpsCameraSeatRotation.y + yLookRotation;
										Player.Local.FPSCameraSeat.localEulerAngles = fpsCameraSeatRotation;
										YRotationOffset = fpsCameraSeatRotation.y;
								}
				//TODO UNITY 5
				/*
								if (OVRManager.display != null) {
									Debug.Log("Resetting display center pose");
									OVRManager.display.RecenterPose();
								} else {
									mResetCameraForwardNextFrame = true;
								}
								*/
								mRecenteredPoseLastFrame = true;
						}
				}

				public void ResetCameraOrientation(float yRotationOffset)
				{
						if (VRMode) {
								if (Player.Local != null) {
										Debug.Log("Reset camera orientation to " + yRotationOffset.ToString());
										Vector3 fpsCameraSeatEulerAngles = Player.Local.FPSCameraSeat.localEulerAngles;
										fpsCameraSeatEulerAngles.y = yRotationOffset;
										YRotationOffset = yRotationOffset;
										Player.Local.FPSCameraSeat.localEulerAngles = fpsCameraSeatEulerAngles;
								}
						}
				}

				public void ResetDirectionTransform()
				{ 
						DirectionTransform.ResetLocal();
						Vector3 directionTransformEulerAngles = DirectionTransform.eulerAngles;
						directionTransformEulerAngles.x = 0f;
						directionTransformEulerAngles.z = 0f;
						DirectionTransform.eulerAngles = directionTransformEulerAngles;
				}


				protected bool DetectDirectToRiftMode ()
				{
						gDirectToRiftMode = false;

						switch (Application.platform) {
								case RuntimePlatform.LinuxPlayer:
								case RuntimePlatform.WindowsEditor:
								case RuntimePlatform.WindowsPlayer:
										//TODO find a way to do this on osx
										long exeSize = 0;
										FileInfo exeFile = new System.IO.FileInfo(Environment.GetCommandLineArgs()[0]);   // Path name of the .exe used to launch
										exeSize = exeFile.Length;   // exeFile.Length return the file size in bytes. Store it for comparison

										// Use file to determine which exe was launched. This should be stable even if a user changes the name of the .exe or uses a shortcut! =D
										// Direct Rift sizes: 184320 is 64bit size, 32 is 164864 (3rd check is for extended mode(NOT FULLY TESTED)) 
										// (You may want to use Debug.Log(exeSize); to double check the file size is the same on your match)

										if ((exeSize == 184320 || exeSize == 164864)) {
												// DirectToRift.exe
												gDirectToRiftMode = true;
										}
										break;

								case RuntimePlatform.OSXPlayer:
								case RuntimePlatform.OSXEditor:
								default:
										break;
						}

						if (gDirectToRiftMode) {
								Debug.Log("Direct to rift mode detected");
				//TODO UNITY 5
								/*OvrManager.enabled = true;
								OvrCameraRig.enabled = true;*/
								RefreshSettings(false);
								mResetCameraForwardNextFrame = true;
						}

						return gDirectToRiftMode;
				}

				protected static bool gOculusModeEnabled = false;
				protected static bool gDirectToRiftMode = false;

				protected Camera mProxyCamera;
				public Vector3 mLockOffset = Vector3.zero;
				public Vector3 mForwardOffset = Vector3.zero;
				public bool mRecenteredPoseLastFrame = false;
				public bool mResetCameraForwardNextFrame = false;
				public bool mCursorLock = true;
				public bool mAxisLock = true;
				protected int mGUIEditorID = -1;
		}
}
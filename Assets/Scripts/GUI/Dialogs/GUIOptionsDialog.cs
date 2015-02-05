using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{		//big huge ugly class for setting options
		//really needs to be broken into multiple sub-classes a la Log interfaces
		public class GUIOptionsDialog : GUIEditor <PlayerPreferences>
		{
				public GameObject VideoApplyButton;
				public UILabel VideoResolutionLabel;
				public UILabel VideoTextureResolutionLabel;
				public UILabel VideoLODDistanceBiasLabel;
				public UILabel VideoFOVLabel;
				public UILabel VideoLODDistanceBiasDescLabel;
				public UILabel VideoShadowLabel;
				public UICheckbox VideoFullScreen;
				public UICheckbox VideoPostFXGodRays;
				public UICheckbox VideoPostFXBloom;
				public UICheckbox VideoPostFXSSAO;
				public UICheckbox VideoPostFXGrain;
				public UICheckbox VideoPostFXMBlur;
				public UICheckbox VideoPostFXAA;
				public UICheckbox VideoPostFXGlobalFog;
				public UICheckbox VideoShadowObjects;
				public UICheckbox VideoShadowTerrain;
				public UICheckbox VideoShadowStructure;
				public UISlider VideoFieldOfView;
				public UISlider VideoLighting;
				public UICheckbox VideoHDR;
				public UISlider SoundGeneral;
				public UISlider SoundMusic;
				public UISlider SoundFX;
				public UISlider SoundAmbient;
				public UISlider SoundInterface;
				public UISlider SfxSoundFootstep;
				public UISlider SfxSoundPlayerVoice;
				public UISlider SfxSoundDynamicObjects;
				public UISlider SfxSoundCreaturs;
				public UISlider VideoTerrainGrassDensity;
				public UISlider VideoTerrainGrassDistance;
				public UISlider VideoTerrainMaxMeshTrees;
				public UILabel VideoTerrainMaxMeshTreesLabel;
				public UISlider VideoTerrainTerrainDetail;
				public UISlider VideoTerrainTreeBillboardDistance;
				public UISlider VideoTerrainTreeDistance;
				public UISlider VideoAmbientLightAtNight;
				public UISlider ImmersionCrosshairAlphaSlider;
				public UISlider ImmersionCrosshairInactiveAlphaSlider;
				public UISlider ImmersionPathGlowIntensitySlider;
				public UICheckbox ImmersionWorldItemsOverlay;
				public UICheckbox ImmersionWorldItemHUD;
				public UICheckbox ImmersionSpecialObjectsOverlay;
				public UISlider MouseSensitivityInterface;
				public UISlider MouseSensitivityFPSCamera;
				public UICheckbox MouseInvertYAxis;
				public UICheckbox ControllerCursorCheckbox;
				public UICheckbox CustomDeadZonesCheckbox;
				public UICheckbox OculusModeCheckbox;
				public UICheckbox ControllerPrompts;
				public UISlider AccessibilityTextSpeed;
				public float VideoFovMin = 60.0f;
				public float VideoFovMax = 120.0f;
				public int MaxMeshTreesMin = 16;
				public int MaxMeshTreesMax = 1024;
				public float TreeBillboardDistMin = 32f;
				public float TreeBillboardDistMax = 256f;
				public float GrassDistMax = 500f;
				public float GrassDistMin = 50f;
				public float TerrainDetailMax = 60f;
				public float TerrainDetailMin = 10f;
				public float MouseSensitivityFPSMin	= 1.0f;
				public float MouseSensitivityFPSMax	= 10.0f;
				public float AccessibilityTextSpeedMin = 0.5f;
				public float AccessibilityTextSpeedMax = 2f;
				//public PlayerPreferences.VideoPrefs VideoPrefs = PlayerPreferences.VideoPrefs.Default;
				public PlayerPreferences.VideoPrefs TempVideoPrefs;
				public GUIUserActionBrowser ActionBrowser;
				// = PlayerPreferences.VideoPrefs.Default;
				public GUITabs Tabs;
				public bool Initialized = false;

				public override void PushEditObjectToNGUIObject()
				{
						VideoFovMin = Globals.FOVMin;
						VideoFovMax = Globals.FOVMax;
						MaxMeshTreesMin = Globals.ChunkTerrainMaxMeshTreesMin;
						MaxMeshTreesMax = Globals.ChunkTerrainMaxMeshTreesMax;
						TreeBillboardDistMin = Globals.ChunkTerrainTreeBillboardDistMin;
						TreeBillboardDistMax = Globals.ChunkTerrainTreeBillboardDistMax;
						GrassDistMax = Globals.ChunkTerrainGrassDistanceMax;
						GrassDistMin = Globals.ChunkTerrainGrassDistanceMin;
						TerrainDetailMax = Globals.ChunkTerrainDetailMax;
						TerrainDetailMin = Globals.ChunkTerrainDetailMin;
						MouseSensitivityFPSMax = Globals.MouseSensitivityFPSMax;
						MouseSensitivityFPSMin = Globals.MouseSensitivityFPSMin;

						Tabs.Initialize(this);

						Initialized = true;

						TempVideoPrefs = ObjectClone.Clone <PlayerPreferences.VideoPrefs>(Profile.Get.CurrentPreferences.Video);
						TempVideoPrefs.RefreshPostFX();
						TempVideoPrefs.RefreshSupportedResolutions();
						VideoRefresh(TempVideoPrefs);
						SoundRefresh();
						ImmersionRefresh();
						ControlsRefresh();
						AccessibilityRefresh();
				}

				#region widgets changing

				public void OnChangeOculusMode()
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}
			
						mMadeVideoChanges = true;

						TempVideoPrefs.OculusMode = OculusModeCheckbox.isChecked;
				}

				public void OnImmersionSettingChange()
				{
						if (!Initialized || mRefreshingImmersion || mFinished) {
								return;
						}
			
						Profile.Get.CurrentPreferences.Immersion.CrosshairGeneral = ImmersionCrosshairAlphaSlider.sliderValue;
						Profile.Get.CurrentPreferences.Immersion.CrosshairWhenInactive = ImmersionCrosshairInactiveAlphaSlider.sliderValue;
						Profile.Get.CurrentPreferences.Immersion.WorldItemOverlay = ImmersionWorldItemsOverlay.isChecked;
						Profile.Get.CurrentPreferences.Immersion.PathGlowIntensity = ImmersionPathGlowIntensitySlider.sliderValue;
						Profile.Get.CurrentPreferences.Immersion.SpecialObjectsOverlay	= ImmersionSpecialObjectsOverlay.isChecked;
						Profile.Get.CurrentPreferences.Immersion.WorldItemHUD = ImmersionWorldItemHUD.isChecked;
				}

				public void OnControlSettingsChange()
				{
						if (!Initialized || mRefreshingControls || mFinished) {
								return;
						}
			
						Profile.Get.CurrentPreferences.Controls.MouseSensitivityFPSCamera = (MouseSensitivityFPSMin + ((MouseSensitivityFPSMax - MouseSensitivityFPSMin) * MouseSensitivityFPSCamera.sliderValue));
						//TempControlPrefs.MouseSensitivityInterface = MouseSensitivityInterface.sliderValue;
						Profile.Get.CurrentPreferences.Controls.MouseInvertYAxis = MouseInvertYAxis.isChecked;
						Profile.Get.CurrentPreferences.Controls.UseControllerMouse = ControllerCursorCheckbox.isChecked;
						Profile.Get.CurrentPreferences.Controls.UseCustomDeadZoneSettings = CustomDeadZonesCheckbox.isChecked;
						Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts = ControllerPrompts.isChecked;
						Profile.Get.CurrentPreferences.Controls.Apply();
				}

				public void OnTerrainSettingChange()
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}		
			
						mMadeVideoChanges = true;

						TempVideoPrefs.TerrainDetail = (TerrainDetailMin + ((TerrainDetailMax - TerrainDetailMin) * (1.0f - VideoTerrainTerrainDetail.sliderValue)));
						TempVideoPrefs.TerrainMaxMeshTrees = (int)(MaxMeshTreesMin + ((MaxMeshTreesMax - MaxMeshTreesMin) * VideoTerrainMaxMeshTrees.sliderValue));
						TempVideoPrefs.TerrainGrassDensity = VideoTerrainGrassDensity.sliderValue;
						TempVideoPrefs.TerrainGrassDistance = (GrassDistMin + ((GrassDistMax - GrassDistMin) * VideoTerrainGrassDistance.sliderValue));
						TempVideoPrefs.TerrainTreeBillboardDistance = (TreeBillboardDistMin + ((TreeBillboardDistMax - TreeBillboardDistMin) * VideoTerrainTreeBillboardDistance.sliderValue));
						TempVideoPrefs.TerrainTreeDistance = VideoTerrainTreeDistance.sliderValue;

						VideoRefresh(TempVideoPrefs);
				}

				public void OnClickCancelButton()
				{
						ActionCancel(WorldClock.RealTime);
				}

				public void OnClickVideoApply()
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}

						VideoApply();
				}

				public void OnClickLowerVideo(GameObject sender)
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}		
			
						switch (sender.name) {
								case "Resolution":
										Resolution prevRes = TempVideoPrefs.GetPrevResolution(TempVideoPrefs.ResolutionWidth, TempVideoPrefs.ResolutionHeight);
										TempVideoPrefs.ResolutionWidth = prevRes.width;
										TempVideoPrefs.ResolutionHeight = prevRes.height;
										mMadeVideoChanges = true;
										break;

								case "TextureResolution":
										TempVideoPrefs.TextureResolution = TempVideoPrefs.GetPrevTextureResolution(TempVideoPrefs.TextureResolution);
										mMadeVideoChanges = true;
										break;

								case "Shadows":
										TempVideoPrefs.Shadows = TempVideoPrefs.GetPrevShadowSetting(TempVideoPrefs.Shadows);
										mMadeVideoChanges = true;
										break;

								case "LOD":
										TempVideoPrefs.LodDistance = TempVideoPrefs.GetPrevLODDistance(TempVideoPrefs.LodDistance);
										mMadeVideoChanges = true;
										break;

								default:
										break;
						}

						if (mMadeVideoChanges) {
								VideoRefresh(TempVideoPrefs);
						}
				}

				public void OnClickHigherVideo(GameObject sender)
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}
			
						switch (sender.name) {
								case "Resolution":
										Resolution nextRes = TempVideoPrefs.GetNextResolution(TempVideoPrefs.ResolutionWidth, TempVideoPrefs.ResolutionHeight);
										TempVideoPrefs.ResolutionWidth = nextRes.width;
										TempVideoPrefs.ResolutionHeight = nextRes.height;
										mMadeVideoChanges = true;
										break;

								case "TextureResolution":
										TempVideoPrefs.TextureResolution = TempVideoPrefs.GetNextTextureResolution(TempVideoPrefs.TextureResolution);
										mMadeVideoChanges = true;
										break;

								case "LOD":
										TempVideoPrefs.LodDistance = TempVideoPrefs.GetNextLODDistance(TempVideoPrefs.LodDistance);
										mMadeVideoChanges = true;
										break;

								case "Shadows":
										TempVideoPrefs.Shadows = TempVideoPrefs.GetNextShadowSetting(TempVideoPrefs.Shadows);
										mMadeVideoChanges = true;
										break;

								default:
										break;
						}

						if (mMadeVideoChanges) {
								VideoRefresh(TempVideoPrefs);
						}
				}

				public void OnClickVideoCheckbox()
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}
			
						mMadeVideoChanges = true;

						TempVideoPrefs.PostFXGodRays = VideoPostFXGodRays.isChecked;
						TempVideoPrefs.PostFXBloom = VideoPostFXBloom.isChecked;
						TempVideoPrefs.PostFXSSAO = VideoPostFXSSAO.isChecked;
						TempVideoPrefs.PostFXGrain = VideoPostFXGrain.isChecked;
						TempVideoPrefs.PostFXMBlur = VideoPostFXMBlur.isChecked;
						TempVideoPrefs.PostFXAA = VideoPostFXAA.isChecked;
						TempVideoPrefs.Fullscreen = VideoFullScreen.isChecked;
						TempVideoPrefs.PostFXGlobalFog = VideoPostFXGlobalFog.isChecked;
						//TempVideoPrefs.HDR = VideoHDR.isChecked;
						TempVideoPrefs.ObjectShadows = VideoShadowObjects.isChecked;
						TempVideoPrefs.TerrainShadows = VideoShadowTerrain.isChecked;
						TempVideoPrefs.StructureShadows = VideoShadowStructure.isChecked;

						VideoRefresh(TempVideoPrefs);
				}

				public void OnVideoLightingChange(){
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}

						mMadeVideoChanges = true;

						TempVideoPrefs.AdjustBrightness = Mathf.CeilToInt (VideoLighting.sliderValue * 100);
						TempVideoPrefs.NightAmbientLightBooster = VideoAmbientLightAtNight.sliderValue;
						if (TempVideoPrefs.AdjustBrightness == 51 || VideoLighting.sliderValue == 49) {
								//snap to middle
								//that will disable the brightness adjustment
								VideoLighting.sliderValue = 50;
						}

						VideoRefresh(TempVideoPrefs);
				}

				public void OnVideoFOVChange()
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}		
			
						mMadeVideoChanges = true;

						TempVideoPrefs.FieldOfView = (int)(VideoFovMin + ((VideoFovMax - VideoFovMin) * VideoFieldOfView.sliderValue));

						VideoRefresh(TempVideoPrefs);
				}

				public void OnSoundLevelChange()
				{
						if (!Initialized || mRefreshingSound || mFinished) {
								return;
						}

						Profile.Get.CurrentPreferences.Sound.General = SoundGeneral.sliderValue;
						Profile.Get.CurrentPreferences.Sound.Music = SoundMusic.sliderValue;
						Profile.Get.CurrentPreferences.Sound.Sfx = SoundFX.sliderValue;
						Profile.Get.CurrentPreferences.Sound.Ambient = SoundAmbient.sliderValue;
						Profile.Get.CurrentPreferences.Sound.Interface = SoundInterface.sliderValue;
						Profile.Get.CurrentPreferences.Sound.SfxFootsteps = SfxSoundFootstep.sliderValue;
						Profile.Get.CurrentPreferences.Sound.SfxPlayerVoice = SfxSoundPlayerVoice.sliderValue;
						Profile.Get.CurrentPreferences.Sound.SfxDynamicObjects = SfxSoundDynamicObjects.sliderValue;
						Profile.Get.CurrentPreferences.Sound.SfxCreatures = SfxSoundCreaturs.sliderValue;
				}

				public void OnAccessibilitySettingsChange ( ) {
						if (!Initialized || mRefreshingAccessibility || mFinished) {
								return;
						}

						Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed = (AccessibilityTextSpeedMin + ((AccessibilityTextSpeedMax - AccessibilityTextSpeedMin) * AccessibilityTextSpeed.sliderValue));
				}

				#endregion

				#region applying

				public void VideoApply()
				{
						Debug.Log("Attempting to apply video...");
						if (!Initialized || !mMadeVideoChanges || mFinished) {
								Debug.Log("We haven't made any changes");
								return;
						}
						Debug.Log("Copying from temporary preferences");
						//set video prefs to be official
						Profile.Get.CurrentPreferences.Video.CopyFrom(TempVideoPrefs);
						Profile.Get.CurrentPreferences.Apply(true);
						//then create new temp prefs to match recently set prefs
						mMadeVideoChanges = false;
						Debug.Log("Refreshing from temporary preferences");
						TempVideoPrefs.CopyFrom(Profile.Get.CurrentPreferences.Video);
						VideoRefresh(TempVideoPrefs);
				}

				#endregion

				#region refreshing

				public void VideoRefresh(PlayerPreferences.VideoPrefs videoPrefs)
				{
						if (!Initialized || mRefreshingVideo || mFinished) {
								return;
						}

						mRefreshingVideo = true;

						VideoFullScreen.isChecked = (videoPrefs.Fullscreen);
						VideoHDR.isChecked = true;//videoPrefs.HDR;
						VideoHDR.gameObject.SetActive(false);
						VideoResolutionLabel.text = (videoPrefs.ResolutionWidth.ToString() + " x " + videoPrefs.ResolutionHeight.ToString());
						VideoFOVLabel.text = videoPrefs.FieldOfView.ToString();
						VideoLighting.sliderValue = ((float)videoPrefs.AdjustBrightness) / 100;
						VideoAmbientLightAtNight.sliderValue = videoPrefs.NightAmbientLightBooster;
			
						#region labels
						string textureResolution = string.Empty;
						switch (videoPrefs.TextureResolution) {
								case 0:
										textureResolution = "Full Res";
										break;

								case 1:
										textureResolution = "Half Res";
										break;

								case 2:
										textureResolution = "Quater Res";
										break;

								case 3:
										textureResolution = "Eighth Res";
										break;

								default:
										break;
						}
						VideoTextureResolutionLabel.text = textureResolution;

						string lodSetting = string.Empty;
						switch (videoPrefs.LodDistance) {
								case 0:
										lodSetting = "Very close";
										break;

								case 1:
										lodSetting = "Close";
										break;

								case 2:
										lodSetting = "Moderate";
										break;

								case 3:
										lodSetting = "Far";
										break;

								case 4:
										lodSetting = "Very Far";
										break;

								case 5:
										lodSetting = "Ultra";
										break;

								default:
										break;
						}
						VideoLODDistanceBiasLabel.text = lodSetting;

						string shadowSetting = string.Empty;
						switch (videoPrefs.Shadows) {
								case 0:
										shadowSetting = "Off";
										break;

								case 1:
										shadowSetting = "Low";
										break;

								case 2:
										shadowSetting = "Moderate";
										break;

								case 3:
										shadowSetting = "High";
										break;

								case 4:
										shadowSetting = "Very High";
										break;

								case 5:
										shadowSetting = "Ultra";
										break;
						}
						VideoShadowLabel.text = shadowSetting;
						#endregion

						VideoFieldOfView.sliderValue = (videoPrefs.FieldOfView - VideoFovMin) / (VideoFovMax - VideoFovMin);

						VideoTerrainTreeBillboardDistance.sliderValue = (videoPrefs.TerrainTreeBillboardDistance - TreeBillboardDistMin) / (TreeBillboardDistMax - TreeBillboardDistMin);
						VideoTerrainGrassDensity.sliderValue = videoPrefs.TerrainGrassDensity;
						VideoTerrainGrassDistance.sliderValue = (videoPrefs.TerrainGrassDistance - GrassDistMin) / (GrassDistMax - GrassDistMin);
						VideoTerrainTerrainDetail.sliderValue = 1.0f - (videoPrefs.TerrainDetail - TerrainDetailMin) / (TerrainDetailMax - TerrainDetailMin);
						VideoTerrainMaxMeshTrees.sliderValue = (float)(videoPrefs.TerrainMaxMeshTrees - MaxMeshTreesMin) / (MaxMeshTreesMax - MaxMeshTreesMin);
						VideoTerrainTreeDistance.sliderValue = videoPrefs.TerrainTreeDistance;
						VideoTerrainMaxMeshTreesLabel.text = "Max Mesh Trees: " + videoPrefs.TerrainMaxMeshTrees.ToString();

						VideoPostFXBloom.isChecked = videoPrefs.PostFXBloom;
						VideoPostFXSSAO.isChecked = videoPrefs.PostFXSSAO;
						VideoPostFXGodRays.isChecked = videoPrefs.PostFXGodRays;
						VideoPostFXGrain.isChecked = videoPrefs.PostFXGrain;
						VideoPostFXMBlur.isChecked = videoPrefs.PostFXMBlur;
						VideoPostFXAA.isChecked = videoPrefs.PostFXAA;
						VideoPostFXGlobalFog.isChecked = videoPrefs.PostFXGlobalFog;
						VideoShadowObjects.isChecked = videoPrefs.ObjectShadows;
						VideoShadowStructure.isChecked = videoPrefs.StructureShadows;
						VideoShadowTerrain.isChecked = videoPrefs.TerrainShadows;
			
						OculusModeCheckbox.isChecked = videoPrefs.OculusMode;

						if (mMadeVideoChanges) {
								VideoApplyButton.SendMessage("SetEnabled", SendMessageOptions.DontRequireReceiver);
						} else {
								VideoApplyButton.SendMessage("SetDisabled", SendMessageOptions.DontRequireReceiver);
						}

						//Profile.Get.CurrentPreferences.Video = TempVideoPrefs;
						mRefreshingVideo = false;
				}

				public void AccessibilityRefresh () {
						if (!Initialized || mRefreshingAccessibility || mFinished) {
								return;
						}

						mRefreshingAccessibility = true;

						AccessibilityTextSpeed.sliderValue = (Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed - AccessibilityTextSpeedMin) / (AccessibilityTextSpeedMax - AccessibilityTextSpeedMin);
				
						mRefreshingAccessibility = false;
				}

				public void ControlsRefresh()
				{
						if (!Initialized || mRefreshingControls || mFinished) {
								return;
						}
			
						mRefreshingControls = true;

						//ActionBrowser.ReceiveFromParentEditor(UserActionManager.Get.Settings);
			
						MouseSensitivityFPSCamera.sliderValue = (Profile.Get.CurrentPreferences.Controls.MouseSensitivityFPSCamera - MouseSensitivityFPSMin) / (MouseSensitivityFPSMax - MouseSensitivityFPSMin);
						MouseInvertYAxis.isChecked = Profile.Get.CurrentPreferences.Controls.MouseInvertYAxis;
						ControllerCursorCheckbox.isChecked = Profile.Get.CurrentPreferences.Controls.UseControllerMouse;
						CustomDeadZonesCheckbox.isChecked = Profile.Get.CurrentPreferences.Controls.UseCustomDeadZoneSettings;
						ControllerPrompts.isChecked = Profile.Get.CurrentPreferences.Controls.ShowControllerPrompts;
						mRefreshingControls = false;
				}

				public void ImmersionRefresh()
				{
						if (!Initialized || mRefreshingImmersion || mFinished) {
								return;
						}
			
						mRefreshingImmersion = true;
			
						ImmersionCrosshairAlphaSlider.sliderValue = Profile.Get.CurrentPreferences.Immersion.CrosshairGeneral;
						ImmersionCrosshairInactiveAlphaSlider.sliderValue = Profile.Get.CurrentPreferences.Immersion.CrosshairWhenInactive;
						ImmersionPathGlowIntensitySlider.sliderValue = Profile.Get.CurrentPreferences.Immersion.PathGlowIntensity;
						ImmersionSpecialObjectsOverlay.isChecked = Profile.Get.CurrentPreferences.Immersion.SpecialObjectsOverlay;
						ImmersionWorldItemsOverlay.isChecked = Profile.Get.CurrentPreferences.Immersion.WorldItemOverlay;
						ImmersionWorldItemHUD.isChecked = Profile.Get.CurrentPreferences.Immersion.WorldItemHUD;
			
						mRefreshingImmersion = false;
				}

				public void SoundRefresh()
				{
						if (!Initialized || mRefreshingSound || mFinished) {
								return;
						}

						mRefreshingSound = true;

						SoundGeneral.sliderValue = Profile.Get.CurrentPreferences.Sound.General;
						SoundMusic.sliderValue = Profile.Get.CurrentPreferences.Sound.Music;
						SoundFX.sliderValue = Profile.Get.CurrentPreferences.Sound.Sfx;
						SoundAmbient.sliderValue = Profile.Get.CurrentPreferences.Sound.Ambient;
						SoundInterface.sliderValue = Profile.Get.CurrentPreferences.Sound.Interface;
						SfxSoundFootstep.sliderValue = Profile.Get.CurrentPreferences.Sound.SfxFootsteps;
						SfxSoundPlayerVoice.sliderValue = Profile.Get.CurrentPreferences.Sound.SfxPlayerVoice;
						SfxSoundDynamicObjects.sliderValue = Profile.Get.CurrentPreferences.Sound.SfxDynamicObjects;
						SfxSoundCreaturs.sliderValue = Profile.Get.CurrentPreferences.Sound.SfxCreatures;

						mRefreshingSound = false;
				}

				#endregion

				protected bool mMadeVideoChanges = false;
				protected bool mRefreshingVideo = false;
				protected bool mRefreshingSound = false;
				protected bool mRefreshingImmersion = false;
				protected bool mRefreshingControls = false;
				protected bool mRefreshingAccessibility = false;
		}

		public class StartMenuOptionsResult
		{
		}
}
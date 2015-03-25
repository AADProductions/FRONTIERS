using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
	//big huge ugly class for setting options
	//really needs to be broken into multiple sub-classes a la Log interfaces
	public class GUIOptionsDialog : GUIEditor <PlayerPreferences>
	{
		public GameObject VideoApplyButton;
		public GameObject VideoCancelButton;
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
		public UICheckbox VideoVSync;
		public UISlider VideoFieldOfView;
		public UISlider VideoLighting;
		public UICheckbox VideoHDR;
		public UICheckbox VideoReduceTextureVariation;
		public UILabel VideoReduceTextureVariationLabel;
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
		public UICheckbox VideoTerrainReduceTreeVariation;
		public UILabel VideoTerrainReduceTreeVariationLabel;
		public UISlider ImmersionCrosshairAlphaSlider;
		public UISlider ImmersionCrosshairInactiveAlphaSlider;
		public UISlider ImmersionPathGlowIntensitySlider;
		public UISlider ImmersionWalkingSpeedSlider;
		public UISlider ImmersionCameraSmoothingSlider;
		public UICheckbox ImmersionWorldItemsOverlay;
		public UICheckbox ImmersionWorldItemHUD;
		public UICheckbox ImmersionWorldItemHUDInCenter;
		public UICheckbox ImmersionSpecialObjectsOverlay;
		public UICheckbox OculusModeCheckbox;
		public UICheckbox VRStaticCutsceneCamerasCheckbox;
		public UICheckbox VRStaticFastTravelCamerasCheckbox;
		public UICheckbox VRDisableScreenEffectsCheckbox;
		public UICheckbox VRDisableExtraGrassLayersCheckbox;
		public UILabel OculusModeLabelEnabled;
		public UILabel OculusModeLabelDisabled;
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
		public float AccessibilityTextSpeedMin = 0.5f;
		public float AccessibilityTextSpeedMax = 2f;
		//public PlayerPreferences.VideoPrefs VideoPrefs = PlayerPreferences.VideoPrefs.Default;
		public PlayerPreferences.VideoPrefs TempVideoPrefs;
		public GUIUserActionBrowser ActionBrowser;
		// = PlayerPreferences.VideoPrefs.Default;
		public GUITabs Tabs;
		public bool Initialized = false;

		public override Widget FirstInterfaceObject {
			get {
				Widget w = base.FirstInterfaceObject;
				if (Tabs.Buttons.Count > 0) {
					w.BoxCollider = Tabs.Buttons[0].Collider;
				}
				return w;
			}
		}

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
			VideoTerrainReduceTreeVariation.functionName = "OnTerrainSettingChange";

			SoundGeneral.functionName = "OnSoundLevelChange";
			SoundMusic.functionName = "OnSoundLevelChange";
			SoundFX.functionName = "OnSoundLevelChange";
			SoundAmbient.functionName = "OnSoundLevelChange";
			SoundInterface.functionName = "OnSoundLevelChange";
			SfxSoundFootstep.functionName = "OnSoundLevelChange";
			SfxSoundPlayerVoice.functionName = "OnSoundLevelChange";
			SfxSoundDynamicObjects.functionName = "OnSoundLevelChange";
			SfxSoundCreaturs.functionName = "OnSoundLevelChange";

			ImmersionCrosshairAlphaSlider.functionName = "OnImmersionSettingChange";
			ImmersionCrosshairAlphaSlider.eventReceiver = gameObject;
			ImmersionCrosshairInactiveAlphaSlider.functionName = "OnImmersionSettingChange";
			ImmersionCrosshairInactiveAlphaSlider.eventReceiver = gameObject;
			ImmersionWorldItemsOverlay.functionName = "OnImmersionSettingChange";
			ImmersionWorldItemsOverlay.eventReceiver = gameObject;
			ImmersionPathGlowIntensitySlider.functionName = "OnImmersionSettingChange";
			ImmersionPathGlowIntensitySlider.eventReceiver = gameObject;
			ImmersionSpecialObjectsOverlay.functionName = "OnImmersionSettingChange";
			ImmersionSpecialObjectsOverlay.eventReceiver = gameObject;
			ImmersionWorldItemHUD.functionName = "OnImmersionSettingChange";
			ImmersionWorldItemHUD.eventReceiver = gameObject;
			ImmersionWorldItemHUDInCenter.functionName = "OnImmersionSettingChange";
			ImmersionWorldItemHUDInCenter.eventReceiver = gameObject;
			ImmersionPathGlowIntensitySlider.functionName = "OnImmersionSettingChange";
			ImmersionPathGlowIntensitySlider.eventReceiver = gameObject;
			ImmersionCameraSmoothingSlider.functionName = "OnImmersionSettingChange";
			ImmersionCameraSmoothingSlider.eventReceiver = gameObject;

			OculusModeCheckbox.functionName = "OnClickOculusMode";
			OculusModeCheckbox.eventReceiver = gameObject;
			VRDisableScreenEffectsCheckbox.functionName = "OnClickVideoCheckbox";
			VRDisableScreenEffectsCheckbox.eventReceiver = gameObject;
			VRStaticCutsceneCamerasCheckbox.functionName = "OnClickVideoCheckbox";
			VRStaticCutsceneCamerasCheckbox.eventReceiver = gameObject;
			VRStaticFastTravelCamerasCheckbox.functionName = "OnClickVideoCheckbox";
			VRStaticFastTravelCamerasCheckbox.eventReceiver = gameObject;
			VRDisableExtraGrassLayersCheckbox.functionName = "OnClickVideoCheckbox";
			VRDisableExtraGrassLayersCheckbox.eventReceiver = gameObject;

			Tabs.Initialize(this);

			Initialized = true;

			TempVideoPrefs = ObjectClone.Clone <PlayerPreferences.VideoPrefs>(Profile.Get.CurrentPreferences.Video);
			TempVideoPrefs.RefreshPostFX();
			TempVideoPrefs.RefreshSupportedResolutions();
			VideoRefresh(TempVideoPrefs);
			SoundRefresh();
			ImmersionRefresh();
			AccessibilityRefresh();
		}

		public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
		{
			if (flag < 0) {
				flag = GUIEditorID;
			}

			Tabs.GetActiveInterfaceObjects(currentObjects, flag);
			if (VideoApplyButton.layer != Globals.LayerNumGUIRaycastIgnore) {
				Widget w = new Widget(flag);
				w.SearchCamera = NGUICamera;
				w.BoxCollider = VideoApplyButton.GetComponent<BoxCollider>();
				currentObjects.Add(w);
				w.BoxCollider = VideoCancelButton.GetComponent<BoxCollider>();
				currentObjects.Add(w);
			}
		}

		#region widgets changing

		public void Update()
		{
			if (VRManager.VRDeviceAvailable != mDeviceAvailableLastFrame) {
				VideoRefresh(Profile.Get.CurrentPreferences.Video);
			}
			mDeviceAvailableLastFrame = VRManager.VRDeviceAvailable;
		}

		protected bool mDeviceAvailableLastFrame = false;

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
			Profile.Get.CurrentPreferences.Immersion.WorldItemHUDInCenter = ImmersionWorldItemHUDInCenter.isChecked;
			Profile.Get.CurrentPreferences.Immersion.WalkingSpeed = ImmersionWalkingSpeedSlider.sliderValue;
			Profile.Get.CurrentPreferences.Immersion.CameraSmoothing = ImmersionCameraSmoothingSlider.sliderValue;
		}

		public void OnClickOculusMode () {
			if (!Initialized || mRefreshingVideo || mRefreshingOculusMode || mFinished) {
				return;
			}

			Debug.Log("On click oculus mode: " + OculusModeCheckbox.isChecked.ToString());

			mRefreshingOculusMode = true;

			TempVideoPrefs.CopyFrom(Profile.Get.CurrentPreferences.Video);
			TempVideoPrefs.OculusMode = OculusModeCheckbox.isChecked;
			mMadeVideoChanges = true;
			VideoApply();

			mRefreshingOculusMode = false;
		}

		protected bool mRefreshingOculusMode = false;

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
			TempVideoPrefs.TerrainReduceTreeVariation = VideoTerrainReduceTreeVariation.isChecked;

			if (Profile.Get.HasCurrentGame && Profile.Get.CurrentGame.HasStarted) {
				VideoTerrainReduceTreeVariationLabel.text = "(Requires restart)";
				VideoReduceTextureVariationLabel.text = "(Requires restart)";
			} else {
				VideoTerrainReduceTreeVariationLabel.text = string.Empty;
				VideoReduceTextureVariationLabel.text = string.Empty;
			}

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
			TempVideoPrefs.VSync = VideoVSync.isChecked;
			TempVideoPrefs.StructureReduceTextureVariation = VideoReduceTextureVariation.isChecked;

			TempVideoPrefs.VRDisableScreenEffects = VRDisableScreenEffectsCheckbox.isChecked;
			TempVideoPrefs.VRStaticCameraCutscenes = VRStaticCutsceneCamerasCheckbox.isChecked;
			TempVideoPrefs.VRStaticCameraFastTravel = VRStaticFastTravelCamerasCheckbox.isChecked;
			TempVideoPrefs.VRDisableExtraGrassLayers = VRDisableExtraGrassLayersCheckbox.isChecked;

			VideoRefresh(TempVideoPrefs);
		}

		public void OnVideoLightingChange()
		{
			if (!Initialized || mRefreshingVideo || mFinished) {
				return;
			}

			mMadeVideoChanges = true;

			TempVideoPrefs.AdjustBrightness = Mathf.CeilToInt(VideoLighting.sliderValue * 100);
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

			Profile.Get.CurrentPreferences.Sound.Apply();
		}

		public void OnAccessibilitySettingsChange()
		{
			if (!Initialized || mRefreshingAccessibility || mFinished) {
				return;
			}

			Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed = (AccessibilityTextSpeedMin + ((AccessibilityTextSpeedMax - AccessibilityTextSpeedMin) * AccessibilityTextSpeed.sliderValue));
		}

		#endregion

		#region applying

		public void VideoApply()
		{
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
			VideoReduceTextureVariation.isChecked = videoPrefs.StructureReduceTextureVariation;
			VideoHDR.gameObject.SetActive(false);
			VideoResolutionLabel.text = (videoPrefs.ResolutionWidth.ToString() + " x " + videoPrefs.ResolutionHeight.ToString());
			VideoFOVLabel.text = videoPrefs.FieldOfView.ToString();
			VideoLighting.sliderValue = ((float)videoPrefs.AdjustBrightness) / 100;
			VideoAmbientLightAtNight.sliderValue = videoPrefs.NightAmbientLightBooster;
			VideoVSync.isChecked = videoPrefs.VSync;
			
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
					textureResolution = "Quarter Res";
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
			VideoTerrainReduceTreeVariation.isChecked = TempVideoPrefs.TerrainReduceTreeVariation;
			if (Profile.Get.HasCurrentGame && Profile.Get.CurrentGame.HasStarted) {
				VideoTerrainReduceTreeVariationLabel.text = "(Requires restart)";
			} else {
				VideoTerrainReduceTreeVariationLabel.text = string.Empty;
			}

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

			VRStaticCutsceneCamerasCheckbox.isChecked = videoPrefs.VRStaticCameraCutscenes;
			VRStaticFastTravelCamerasCheckbox.isChecked = videoPrefs.VRStaticCameraFastTravel;
			VRDisableScreenEffectsCheckbox.isChecked = videoPrefs.VRDisableScreenEffects;
			VRDisableExtraGrassLayersCheckbox.isChecked = videoPrefs.VRDisableExtraGrassLayers;

			OculusModeCheckbox.isChecked = videoPrefs.OculusMode;
			Debug.Log("Just set oculus mode 'is checked' to " + OculusModeCheckbox.isChecked.ToString());

			if (VRManager.VRDeviceAvailable) {
				OculusModeLabelEnabled.enabled = true;
				OculusModeLabelDisabled.enabled = false;
				#if UNITY_EDITOR
				if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
				#else
				if (VRManager.VRMode) {
				#endif
					VRDisableScreenEffectsCheckbox.gameObject.SetActive(true);
					VRStaticCutsceneCamerasCheckbox.gameObject.SetActive(true);
					VRStaticFastTravelCamerasCheckbox.gameObject.SetActive(true);
					VRDisableExtraGrassLayersCheckbox.gameObject.SetActive(true);
				} else {
					VRDisableScreenEffectsCheckbox.gameObject.SetActive(false);
					VRStaticCutsceneCamerasCheckbox.gameObject.SetActive(false);
					VRStaticFastTravelCamerasCheckbox.gameObject.SetActive(false);
					VRDisableExtraGrassLayersCheckbox.gameObject.SetActive(false);
				}
			} else {
				OculusModeLabelEnabled.enabled = false;
				OculusModeLabelDisabled.enabled = true;
				VRDisableScreenEffectsCheckbox.gameObject.SetActive(false);
				VRStaticCutsceneCamerasCheckbox.gameObject.SetActive(false);
				VRStaticFastTravelCamerasCheckbox.gameObject.SetActive(false);
				VRDisableExtraGrassLayersCheckbox.gameObject.SetActive(false);
			}

			if (mMadeVideoChanges) {
				VideoApplyButton.SendMessage("SetEnabled", SendMessageOptions.DontRequireReceiver);
			} else {
				VideoApplyButton.SendMessage("SetDisabled", SendMessageOptions.DontRequireReceiver);
			}

			//Profile.Get.CurrentPreferences.Video = TempVideoPrefs;
			mRefreshingVideo = false;
		}

		public void AccessibilityRefresh()
		{
			if (!Initialized || mRefreshingAccessibility || mFinished) {
				return;
			}

			mRefreshingAccessibility = true;

			AccessibilityTextSpeed.sliderValue = (Profile.Get.CurrentPreferences.Accessibility.OnScreenTextSpeed - AccessibilityTextSpeedMin) / (AccessibilityTextSpeedMax - AccessibilityTextSpeedMin);
				
			mRefreshingAccessibility = false;
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
			ImmersionWorldItemHUDInCenter.isChecked = Profile.Get.CurrentPreferences.Immersion.WorldItemHUDInCenter;
			ImmersionWalkingSpeedSlider.sliderValue = Profile.Get.CurrentPreferences.Immersion.WalkingSpeed;
			ImmersionCameraSmoothingSlider.sliderValue = Profile.Get.CurrentPreferences.Immersion.CameraSmoothing;
			
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
		protected bool mRefreshingAccessibility = false;
	}

	public class StartMenuOptionsResult
	{
	}
}
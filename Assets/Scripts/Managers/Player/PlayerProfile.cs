using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Frontiers;
using Frontiers.World.Gameplay;
using System.Xml;
using System.Xml.Serialization;
using Frontiers.Data;
using Frontiers.World;
using InControl;

namespace Frontiers
{
	[Serializable]
	public class PlayerProfile
	{
		public string Name = Globals.DefaultProfileName;
		public string LastPlayedWorldName = string.Empty;
		public string LastPlayedGameName = string.Empty;
		public string Version = "0.0.0";
		public List <string> CompletedWorlds = new List <string> ();

		public bool HasLastPlayedGame {
			get {
				return (!string.IsNullOrEmpty (LastPlayedWorldName) && !string.IsNullOrEmpty (LastPlayedGameName));
			}
		}
	}

	[Serializable]
	public class PlayerGame
	{
		public PlayerGame ()
		{
			Character = new PlayerCharacter ();
			Difficulty = new DifficultySetting ();
			MarkedLocations = new List <MobileReference> ();
			NewLocations = new List <MobileReference> ();
			RevealedLocations = new List <MobileReference> ();
			LocationTypesToDisplay = new List <string> ();
		}

		public bool HasCreatedCharacter {
			get {
				return Character.HasBeenCreated;
			}
		}

		public int Seed {
			get {
				if (mSeed < 0) {
					mSeed = Mathf.Abs (Character.FirstName.GetHashCode ());
				}
				return mSeed;
			}
			set {
				mSeed = value;
			}
		}

		public bool HasBeenSavedRecently {
			get {
				return ((DateTime.Now.Second - LastTimeDeclinedToSave.Second) < Globals.SavedRecentlyTimeThreshold)
				|| ((DateTime.Now.Second - LastTimeSaved.Second) < Globals.SavedRecentlyTimeThreshold);
			}
		}

		public void AddGameTimeOffset (double hours, double days, double months, double years)
		{
			GameTimeOffset += (
			    WorldClock.HoursToSeconds (hours) +
			    WorldClock.DaysToSeconds (days) +
			    WorldClock.MonthsToSeconds (months) +
			    WorldClock.YearsToSeconds (years));
		}

		public void AddWorldTimeOffset (double hours, double days, double months, double years)
		{
			WorldTimeOffset += (
			    WorldClock.HoursToSeconds (hours) +
			    WorldClock.DaysToSeconds (days) +
			    WorldClock.MonthsToSeconds (months) +
			    WorldClock.YearsToSeconds (years));
		}

		public void SetWorldTimeOffset (double hours, double days, double months, double years)
		{
			WorldTimeOffset = (
			    WorldClock.HoursToSeconds (hours) +
			    WorldClock.DaysToSeconds (days) +
			    WorldClock.MonthsToSeconds (months) +
			    WorldClock.YearsToSeconds (years));
		}

		public void SetGameTimeOffset (double hours, double days, double months, double years)
		{
			GameTimeOffset = (
			    WorldClock.HoursToSeconds (hours) +
			    WorldClock.DaysToSeconds (days) +
			    WorldClock.MonthsToSeconds (months) +
			    WorldClock.YearsToSeconds (years));
		}

		[XmlIgnore]
		public System.DateTime LastTimeDeclinedToSave;
		public string Name = Globals.DefaultGameName;
		public string WorldName = Globals.DefaultWorldName;
		public int BuildNumber;
		public string Version = "0.0.0";
		public string DifficultyName = string.Empty;
		public bool HasStarted = false;
		public bool HasLoadedOnce = false;
		public double GameTime = 0.0f;
		public double GameTimeOffset = 0.0f;
		public double WorldTimeOffset = 0.0f;
		public double InGameMinutesPerRealtimeSecond = Globals.DefaultInGameMinutesPerRealTimeSecond;
		public System.DateTime LastTimeSaved;
		public PlayerCharacter Character;
		public List <MobileReference> MarkedLocations;
		public List <MobileReference> NewLocations;
		public List <MobileReference> RevealedLocations;
		public List <string> LocationTypesToDisplay;
		public List <SkillStartupSetting> SkillStartupSettings;
		public string CustomStartupPosition = string.Empty;
		[XmlIgnore]
		[NonSerialized]
		public DifficultySetting Difficulty;
		protected int mSeed = -1;
	}

	[Serializable]
	public class PlayerCharacter
	{
		public bool HasBeenCreated {
			get {
				return Age > 0;
			}
		}

		public static PlayerCharacter Default ()
		{
			PlayerCharacter defaultCharacter = new PlayerCharacter ();
			defaultCharacter.Age = 27;
			defaultCharacter.FirstName = "Player";
			defaultCharacter.BodyName = Globals.DefaultMalePlayerBody;
			defaultCharacter.FaceTextureName = Globals.DefaultMalePlayerFaceTexture;
			defaultCharacter.BodyTextureName = Globals.DefaultMalePlayerBodyTexture;
			defaultCharacter.Gender = CharacterGender.Male;
			defaultCharacter.Ethnicity = CharacterEthnicity.Caucasian;
			defaultCharacter.HairColor = CharacterHairColor.Brown;
			defaultCharacter.EyeColor = CharacterEyeColor.Blue;
			defaultCharacter.HairLength = CharacterHairLength.Short;
			defaultCharacter.OnCreated ();

			return defaultCharacter;
		}

		public bool CreatedManually = false;
		public string FirstName = string.Empty;
		public string NickName = string.Empty;
		public int Age = -1;
		public string Version = "0.0.0";
		public string FaceTextureName;
		public string BodyTextureName;
		public string HairTextureName;
		public string BodyName;
		public CharacterGender Gender = CharacterGender.None;
		public CharacterEthnicity Ethnicity = CharacterEthnicity.None;
		public CharacterHairColor HairColor = CharacterHairColor.None;
		public CharacterHairLength HairLength = CharacterHairLength.None;
		public CharacterEyeColor EyeColor = CharacterEyeColor.None;
		public PlayerExperience Exp = new PlayerExperience ();
		public PlayerReputation Rep = new PlayerReputation ();
		public List <string> CharactersSpokenTo = new List<string> ();

		public bool HasSpokenToCharacter (string characterName)
		{
			return CharactersSpokenTo.Contains (characterName);
		}

		public void RefreshVisual ()
		{
			//chooses body + face / body textures
			if (Gender == CharacterGender.Male) {
				BodyName = Globals.DefaultMalePlayerBody;
				FaceTextureName = Globals.DefaultMalePlayerFaceTexture;
				BodyTextureName = Globals.DefaultMalePlayerBodyTexture;
				HairTextureName = Globals.DefaultMalePlayerHairTexture;
			} else {
				BodyName = Globals.DefaultFemalePlayerBody;
				FaceTextureName = Globals.DefaultFemalePlayerFaceTexture;
				BodyTextureName = Globals.DefaultFemalePlayerBodyTexture;
				HairTextureName = Globals.DefaultFemalePlayerHairTexture;
			}

			switch (Ethnicity) {
			case CharacterEthnicity.Caucasian:
			default:
				break;

			case CharacterEthnicity.BlackCarribean:
				FaceTextureName = FaceTextureName.Replace ("_A", "_B");
				break;

			case CharacterEthnicity.EastIndian:
				FaceTextureName = FaceTextureName.Replace ("_A", "_C");
				break;

			case CharacterEthnicity.HanChinese:
				FaceTextureName = FaceTextureName.Replace ("_A", "_D");
				break;
			}
		}

		public void OnCreated ()
		{
			RefreshVisual ();
		}
	}

	[Serializable]
	public class PlayerPreferences
	{
		public static LightShadows DefaultShadowSetting = LightShadows.Soft;

		public static PlayerPreferences Default ()
		{	//TODO this isn't really necessary any more, remove it
			PlayerPreferences prefs = new PlayerPreferences ();
			/*prefs.InitializedAsDefault = true;
						prefs.Video = VideoPrefs.Default();
						prefs.Sound = SoundPrefs.Default();
						prefs.Controls = ControlPrefs.Default();
						prefs.Immersion = ImmersionPrefs.Default();
						prefs.Accessibility = AccessibilityPrefs.Default();
						prefs.Mods = ModPrefs.Default();*/
			return prefs;
		}

		public PlayerPreferences ()
		{
			Version = GameManager.VersionString;
			InitializedAsDefault = true;
			Video = VideoPrefs.Default ();
			Sound = SoundPrefs.Default ();
			Controls = ControlPrefs.Default ();
			Immersion = ImmersionPrefs.Default ();
			Accessibility = AccessibilityPrefs.Default ();
			Mods = ModPrefs.Default ();
			CustomColors = null;
			HideDialogs = new HashSet <string> ();
		}

		public void Apply (bool save)
		{
			Debug.Log ("PLAYERPROFILE: Applying preferences - saving? " + save.ToString ());
			Video.Apply ();
			Sound.Apply ();
			Controls.Apply (true);
			Immersion.Apply ();
			Mods.Apply ();
			Accessibility.Apply ();
			if (CustomColors != null && !CustomColors.IsEmpty) {
				Colors.Get.PushInterfaceColors (CustomColors.InterfaceColors);
			}

			if (save) {
				Profile.Get.SaveImmediately (ProfileComponents.Preferences);
			}
		}

		public string Version;
		public bool InitializedAsDefault = false;
		public VideoPrefs Video;
		public SoundPrefs Sound;
		public ControlPrefs Controls;
		public ImmersionPrefs Immersion;
		public AccessibilityPrefs Accessibility;
		public ModPrefs Mods;
		public HashSet <string> HideDialogs;
		public ColorScheme CustomColors;

		[Serializable]
		public class ImmersionPrefs
		{
			public static ImmersionPrefs Default ()
			{
				ImmersionPrefs prefs = new ImmersionPrefs ();
				prefs.CrosshairGeneral = 1.0f;
				prefs.CrosshairWhenInactive = 0.0f;
				prefs.WorldItemOverlay = true;
				prefs.WorldItemHUD = true;
				prefs.PathGlowIntensity = 1.0f;
				prefs.SpecialObjectsOverlay = true;
				prefs.WalkingSpeed = 1f;
				prefs.CameraSmoothing = 0f;
				prefs.WorldItemHUDInCenter = false;
				return prefs;
			}

			public float CrosshairGeneral = 1.0f;
			public float CrosshairWhenInactive	= 0.0f;
			public bool WorldItemOverlay = true;
			public bool WorldItemHUD = true;
			public bool WorldItemHUDInCenter = false;
			public float PathGlowIntensity = 1.0f;
			public bool SpecialObjectsOverlay = true;
			public double HUDPersistTime = 1.5;
			public float WalkingSpeed = 0.8f;
			public float CameraSmoothing = 0f;

			public void Apply ()
			{
				Frontiers.GUI.GUICrosshair.AlphaInGeneral = CrosshairGeneral;
				Frontiers.GUI.GUICrosshair.AlphaWhenInactive = CrosshairWhenInactive;
			}
		}

		[Serializable]
		public class SoundPrefs
		{
			public static SoundPrefs Default ()
			{
				SoundPrefs prefs = new SoundPrefs ();
				prefs.General = 0.35f;
				prefs.Music = 0.225f;
				prefs.Sfx = 1.0f;
				prefs.Ambient = 0.5f;
				prefs.Interface = 1.0f;
				prefs.SfxFootsteps = 0.5f;
				prefs.SfxPlayerVoice = 0.4f;
				prefs.SfxDynamicObjects = 1.0f;
				prefs.SfxCreatures = 1.0f;
				return prefs;
			}

			public float General = 0.35f;
			public float Music = 0.225f;
			public float Sfx = 1.0f;
			public float Ambient = 1.0f;
			public float Interface = 1.0f;
			public float SfxFootsteps = 0.5f;
			public float SfxPlayerVoice = 0.4f;
			public float SfxDynamicObjects	= 1.0f;
			public float SfxCreatures = 1.0f;

			public void Apply ()
			{
				AudioManager.Get.MasterAmbientVolume = Ambient;
				AudioManager.Get.MasterMusicVolume = Music;
				AudioListener.volume = General;
				MasterAudio.SetBusVolume (SfxDynamicObjects, 0);
				MasterAudio.SetBusVolume (SfxFootsteps, 1);
				MasterAudio.SetBusVolume (SfxCreatures, 2);
				MasterAudio.SetBusVolume (Interface, 3);
				MasterAudio.SetBusVolume (Ambient, 4);
			}
		}

		[Serializable]
		public class ControlPrefs
		{
			public static ControlPrefs Default ()
			{
				ControlPrefs prefs = new ControlPrefs ();
				return prefs;
			}

			public void Apply (bool applyBindings)
			{
				if (Player.Local != null) {
					Player.Local.FPSCamera.MouseSensitivity = new Vector2 (MouseSensitivityFPSCamera, MouseSensitivityFPSCamera);
				}

				if (InterfaceActionSettings == null || InterfaceActionSettings.Count == 0) {
					InterfaceActionSettings = InterfaceActionManager.Get.DefaultActionSettings;
				}
				if (UserActionSettings == null || UserActionSettings.Count == 0) {
					UserActionSettings = UserActionManager.Get.DefaultActionSettings;
				}

				if (applyBindings) {
					InterfaceActionManager.Get.PushSettings (InterfaceActionSettings);
					UserActionManager.Get.PushSettings (UserActionSettings);
				} else {
					//only push dead zones not bindings
					InterfaceActionManager.Get.PushDeadZoneSettings ();
					UserActionManager.Get.PushDeadZoneSettings ();
				}

				InterfaceActionManager.InvertRawMouseAxis = InvertRawInterfaceAxis;
				InterfaceActionManager.InvertRawInterfaceAxis = InvertRawInterfaceAxis;
				InterfaceActionManager.InvertRawMovementAxis = InvertRawMovementAxis;

				UserActionManager.InvertRawMouseAxis = MouseInvertYAxis;
				UserActionManager.InvertRawMovementAxis = InvertRawMovementAxis;
			}

			public void RefreshCustomDeadZoneSettings (InputDevice device)
			{
				if (device != InputDevice.Null) {
					if (DeadZoneLStickLower < 0 || DeadZoneLStickLower < 0 || SensitivityRStick < 0) {
						DeadZoneLStickLower = device.GetControl (InputControlType.LeftStickX).LowerDeadZone;
						DeadZoneLStickUpper = device.GetControl (InputControlType.LeftStickY).UpperDeadZone;
						SensitivityLStick = device.GetControl (InputControlType.LeftStickX).Sensitivity;
					}
					if (DeadZoneRStickLower < 0 || DeadZoneRStickUpper < 0 || SensitivityRStick < 0) {
						DeadZoneRStickLower = device.GetControl (InputControlType.RightStickX).LowerDeadZone;
						DeadZoneRStickUpper = device.GetControl (InputControlType.RightStickY).UpperDeadZone;
						SensitivityRStick = device.GetControl (InputControlType.RightStickX).Sensitivity;
					}
					if (DeadZoneDPadLower < 0 || DeadZoneDPadUpper < 0 || SensitivityDPad < 0) {
						DeadZoneDPadLower = device.GetControl (InputControlType.DPadUp).LowerDeadZone;
						DeadZoneDPadUpper = device.GetControl (InputControlType.DPadUp).UpperDeadZone;
						SensitivityDPad = device.GetControl (InputControlType.DPadUp).Sensitivity;
					}
				}
			}

			public List <ActionSetting> InterfaceActionSettings;
			public List <ActionSetting> UserActionSettings;
			public float MouseSensitivityFPSCamera = 5.0f;
			public float MouseSensitivityInterface = 0.25f;
			public bool MouseInvertYAxis = false;
			public bool InvertRawInterfaceAxis = false;
			public bool InvertRawMovementAxis = false;
			public bool UseControllerMouse = true;
			public bool UseCustomDeadZoneSettings = false;
			public bool ShowControllerPrompts = false;
			public bool ShowCPromptsWhenControllerIsPresent = true;
			public bool VREnableRotation = false;
			public bool VRInstaRotation = false;
			public float DeadZoneLStickLower = -1f;
			public float DeadZoneLStickUpper = -1f;
			public float SensitivityLStick = -1f;
			public float DeadZoneRStickLower = -1f;
			public float DeadZoneRStickUpper = -1f;
			public float SensitivityRStick = -1f;
			public float DeadZoneDPadLower = -1f;
			public float DeadZoneDPadUpper = -1f;
			public float SensitivityDPad = -1f;
		}

		[Serializable]
		public class AccessibilityPrefs
		{
			public static AccessibilityPrefs Default ()
			{
				AccessibilityPrefs prefs = new AccessibilityPrefs ();
				return prefs;
			}

			public string DefaultFont = Globals.DefaultFontName;
			public bool UseDyslexicFont = false;
			public bool ColorBlindMode = false;
			public bool ClosedCaptionMode = false;
			public float OnScreenTextSpeed = 1f;

			public void Apply ()
			{
				if (!Manager.IsAwake <Frontiers.Mats> ())
					return;

				Frontiers.Mats.Get.PushDefaultFont (DefaultFont);
			}
		}

		[Serializable]
		public class VideoPrefs
		{
			public static VideoPrefs Default ()
			{
				VideoPrefs prefs = new VideoPrefs ();
				prefs.OculusMode = false;

				prefs.Fullscreen = false;
				prefs.FieldOfView = 75.0f;
				prefs.PostFXGodRays = false;
				prefs.PostFXDof = false;
				prefs.PostFXBloom = true;
				prefs.PostFXSSAO = true;
				prefs.PostFXGrain = false;
				prefs.PostFXMBlur = false;
				prefs.PostFXAA = true;
				prefs.HDR = true;
				prefs.VSync = false;

				prefs.ResolutionWidth = 1920;
				prefs.ResolutionHeight = 1080;
				prefs.TextureResolution = 0;
				prefs.Shadows = 3;
				prefs.LodDistance = 3;
				prefs.AmbientLightBooster = 0f;
				prefs.AdjustBrightness = 50;

				prefs.TerrainGrassDistance = 100f;
				prefs.TerrainGrassDensity = 0.5f;
				prefs.TerrainDetail = 30.0f;
				prefs.TerrainTreeBillboardDistance	= 128f;
				prefs.TerrainTreeDistance = 2000f;
				prefs.TerrainMaxMeshTrees = 128;
				prefs.TerrainReduceTreeVariation = false;
				prefs.StructureReduceTextureVariation = false;
				prefs.TerrainShadows = true;
				prefs.TerrainMaxDetailLayers = Globals.WorldChunkDetailLayers;
				prefs.StructureShadows = true;
				prefs.ObjectShadows = true;
				//prefs.RefreshPostFX ();

				prefs.NightAmbientLightBooster = 0f;

				prefs.VRStaticCameraCutscenes = true;
				prefs.VRStaticCameraFastTravel = true;
				prefs.VRDisableScreenEffects = true;
				prefs.VRDisableExtraGrassLayers = true;

				return prefs;
			}

			public VideoPrefs ()
			{

			}

			public VideoPrefs (VideoPrefs copyFrom)
			{
				CopyFrom (copyFrom);
			}

			public void CopyFrom (VideoPrefs copyFrom)
			{		//TODO use reflection
				Fullscreen = copyFrom.Fullscreen;
				FieldOfView = copyFrom.FieldOfView;
				PostFXBloom = copyFrom.PostFXBloom;
				PostFXGodRays = copyFrom.PostFXGodRays;
				PostFXDof = copyFrom.PostFXDof;
				PostFXSSAO = copyFrom.PostFXSSAO;
				PostFXGrain = copyFrom.PostFXGrain;
				PostFXMBlur = copyFrom.PostFXMBlur;
				PostFXAA = copyFrom.PostFXAA;
				PostFXGlobalFog = copyFrom.PostFXGlobalFog;
				HDR = true;// copyFrom.HDR;
				VSync = copyFrom.VSync;

				ResolutionWidth = copyFrom.ResolutionWidth;
				ResolutionHeight = copyFrom.ResolutionHeight;
				TextureResolution = copyFrom.TextureResolution;
				Shadows = copyFrom.Shadows;
				LodDistance = copyFrom.LodDistance;
				AmbientLightBooster = copyFrom.AmbientLightBooster;
				AdjustBrightness = copyFrom.AdjustBrightness;
				OculusMode = copyFrom.OculusMode;

				TerrainGrassDistance = copyFrom.TerrainGrassDistance;
				TerrainGrassDensity = copyFrom.TerrainGrassDensity;
				TerrainDetail = copyFrom.TerrainDetail;
				TerrainTreeBillboardDistance = copyFrom.TerrainTreeBillboardDistance;
				TerrainTreeDistance = copyFrom.TerrainTreeDistance;
				TerrainMaxMeshTrees = copyFrom.TerrainMaxMeshTrees;
				TerrainShadows = copyFrom.TerrainShadows;
				TerrainReduceTreeVariation = copyFrom.TerrainReduceTreeVariation;
				TerrainMaxDetailLayers = copyFrom.TerrainMaxDetailLayers;
				StructureReduceTextureVariation = copyFrom.StructureReduceTextureVariation;
				StructureShadows = copyFrom.StructureShadows;
				ObjectShadows = copyFrom.ObjectShadows;

				NightAmbientLightBooster = copyFrom.NightAmbientLightBooster;

				VRStaticCameraCutscenes = copyFrom.VRStaticCameraCutscenes;
				VRStaticCameraFastTravel = copyFrom.VRStaticCameraFastTravel;
				VRDisableScreenEffects = copyFrom.VRDisableScreenEffects;
				VRDisableExtraGrassLayers = copyFrom.VRDisableExtraGrassLayers;
				VRQuadZOffset = copyFrom.VRQuadZOffset;
			}

			public void Apply ()
			{
				try {
					if (!Application.isEditor) {
						if (mSupportedResolutions.Count == 0) {
							RefreshSupportedResolutions ();
						}
					}

					bool applyShadows = Manager.IsAwake <Biomes> ();

					switch (Shadows) {
					case 0:
						QualitySettings.SetQualityLevel (0);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 8f;
						QualitySettings.shadowCascades = 0;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.None;
						}
						break;

					case 1:
						QualitySettings.SetQualityLevel (1);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 32f;
						QualitySettings.shadowCascades = 1;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.Hard;
						}
						break;

					case 2:
						QualitySettings.SetQualityLevel (2);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 64f;
						QualitySettings.shadowCascades = 1;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.Soft;
						}
						break;

					case 3:
						QualitySettings.SetQualityLevel (3);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 128f;
						QualitySettings.shadowCascades = 2;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.Soft;
						}
						break;

					case 4:
						QualitySettings.SetQualityLevel (4);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 256f;
						QualitySettings.shadowCascades = 4;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.Soft;
						}
						break;

					case 5:
						QualitySettings.SetQualityLevel (5);
						QualitySettings.pixelLightCount = 0;
						QualitySettings.shadowDistance = 512f;
						QualitySettings.shadowCascades = 4;
						if (applyShadows) {
							DefaultShadowSetting = LightShadows.Soft;

						}
						break;

					default:
						break;
					}

					Biomes.Get.SunLight.shadows = DefaultShadowSetting;

					if (!Application.isEditor) {
						if (!OculusMode && !VRManager.VRMode) {
							Debug.Log ("Not oculus or vr mode, applying screen resolution / full screen");
							if (!IsCurrentResolutionSupported) {
								RefreshSupportedResolutions ();
								FindDefaultResolution ();
							}
							if (Screen.currentResolution.width != ResolutionWidth
							    || Screen.currentResolution.height != ResolutionHeight
							    || Screen.fullScreen != Fullscreen) {
								Screen.SetResolution (ResolutionWidth, ResolutionHeight, Fullscreen);
							}
						}
					}

					if (Manager.IsAwake <Player> ()) {
						GameManager.Get.GameCamera.hdr = true;//HDR;
						GameManager.Get.GameCamera.fieldOfView = FieldOfView;

						CameraFX.Get.AdjustBrightness = AdjustBrightness;

						if (OculusMode && VRDisableScreenEffects) {
							SetOrDisablePostEffect (CameraFX.Get.Default.BloomEffect, false, ref PostFXBloom);
							CameraFX.Get.Default.SunShaftsEffect.enabled = false;
							CameraFX.Get.Default.SSAO.enabled = false;
							CameraFX.Get.Default.Grain.enabled = false;
							SetOrDisablePostEffect (CameraFX.Get.Default.AA, true, ref PostFXAA);
							if (Application.platform == RuntimePlatform.LinuxPlayer | Application.platform == RuntimePlatform.OSXPlayer) {
								bool fogOff = false;
								SetOrDisablePostEffect (CameraFX.Get.Default.Fog, true, ref fogOff);
							} else {
								SetOrDisablePostEffect (CameraFX.Get.Default.Fog, true, ref PostFXGlobalFog);
							}
							CameraFX.Get.Default.ColorGrading.enabled = true;
						} else {
							SetOrDisablePostEffect (CameraFX.Get.Default.BloomEffect, false, ref PostFXBloom);
							SetOrDisablePostEffect (CameraFX.Get.Default.SunShaftsEffect, false, ref PostFXGodRays);
							SetOrDisablePostEffect (CameraFX.Get.Default.SSAO, true, ref PostFXSSAO);
							SetOrDisablePostEffect (CameraFX.Get.Default.Grain, true, ref PostFXGrain);
							SetOrDisablePostEffect (CameraFX.Get.Default.AA, true, ref PostFXAA);
							if (Application.platform == RuntimePlatform.LinuxPlayer | Application.platform == RuntimePlatform.OSXPlayer) {
								bool fogOff = false;
								SetOrDisablePostEffect (CameraFX.Get.Default.Fog, true, ref fogOff);
							} else {
								SetOrDisablePostEffect (CameraFX.Get.Default.Fog, true, ref PostFXGlobalFog);
							}
						}

					}

					QualitySettings.masterTextureLimit = TextureResolution;
					QualitySettings.lodBias = ((float)LodDistance + 0.75f);

					if (Manager.IsAwake <GameWorld> ()) {
						GameWorld.Get.RefreshTerrainDetailSettings ();
					}

					if (Manager.IsAwake <WorldItems> ()) {
						WorldItems.Get.RefreshWorlditemShadowSettings (ObjectShadows);
					}
					if (Manager.IsAwake <Characters> ()) {
						Characters.Get.RefreshCharacterShadowSettings (ObjectShadows);
					}
					if (Manager.IsAwake <Creatures> ()) {
						Creatures.Get.RefreshCreatureShadowSettings (ObjectShadows);
					}
					if (Manager.IsAwake <Structures> ()) {
						Structures.Get.RefreshStructureShadowSettings (StructureShadows, TerrainShadows);
					}

					if (!OculusMode && !VRManager.VRMode) {
						if (VSync) {
							QualitySettings.vSyncCount = 1;
						} else {
							QualitySettings.vSyncCount = 0;
						}
					}

				} catch (Exception e) {
					Debug.LogException (e);
				}
			}

			public void RefreshPostFX ()
			{
				if (Manager.IsAwake <CameraFX> ()) {
					PostFXBloom = CameraFX.Get.Default.BloomEffect.enabled;
					PostFXGodRays = CameraFX.Get.Default.SunShaftsEffect.enabled;
					PostFXSSAO = CameraFX.Get.Default.SSAO.enabled;
					PostFXGrain = CameraFX.Get.Default.Grain.enabled;
					//PostFXMBlur = CameraFX.Get.Default.MotionBlur.enabled;
					PostFXAA = CameraFX.Get.Default.AA.enabled;
					HDR = GameManager.Get.GameCamera.hdr;
					AdjustBrightness = CameraFX.Get.AdjustBrightness;
				}

			}

			public bool OculusMode = false;
			public bool Fullscreen = false;
			public float FieldOfView = 60.0f;
			public bool PostFXGodRays = false;
			public bool PostFXDof = false;
			public bool PostFXBloom = false;
			public bool PostFXSSAO = false;
			public bool PostFXMBlur = false;
			public bool PostFXGrain = false;
			public bool PostFXAA = false;
			public bool PostFXGlobalFog = true;
			public bool HDR = true;
			public bool VSync = false;
			public int ResolutionWidth = 1920;
			public int ResolutionHeight = 1080;
			public int TextureResolution = 0;
			public int Shadows = 3;
			public float AmbientLightBooster = 0f;
			public int AdjustBrightness = 50;
			public bool ObjectShadows = true;
			public bool TerrainShadows = true;
			public bool StructureShadows = true;
			public int LodDistance = 3;
			public float TerrainGrassDistance = 50f;
			public float TerrainGrassDensity = 0.5f;
			public float TerrainDetail = 30.0f;
			public float TerrainTreeBillboardDistance = 128f;
			public float TerrainTreeDistance = 2000f;
			public int TerrainMaxDetailLayers = 12;
			public int TerrainMaxMeshTrees = 128;
			public bool TerrainReduceTreeVariation = false;
			public bool StructureReduceTextureVariation = false;
			public float NightAmbientLightBooster = 0f;
			public bool VRStaticCameraCutscenes = true;
			public bool VRStaticCameraFastTravel = true;
			public bool VRDisableScreenEffects = true;
			public bool VRDisableExtraGrassLayers = true;
			public float VRQuadZOffset = 0f;

			#region helper functions

			public float DeviceAspectRatio {
				get {
					return (float)Screen.width / (float)Screen.height;
				}
			}

			public float ResolutionAspectRatio {
				get {
					return (float)ResolutionWidth / (float)ResolutionHeight;
				}
			}

			public bool IsCurrentResolutionSupported {
				get {
					RefreshSupportedResolutions ();
					bool foundCurrentResolution = false;
					foreach (Resolution res in mSupportedResolutions) {
						if (res.width == ResolutionWidth && res.height == ResolutionHeight) {
							foundCurrentResolution = true;
							break;
						}
					}
					return foundCurrentResolution;
				}
			}

			public void FindDefaultResolution ()
			{
				bool setFirst = false;
				bool setDefault = false;
				foreach (Resolution res in mSupportedResolutions) {
					if (!setFirst) { //set the first so we have something
						ResolutionWidth = res.width;
						ResolutionHeight = res.height;
						setFirst = true;
					} else if (!setDefault) {
						if (res.width >= 1280) {
							ResolutionWidth = res.width;
							ResolutionHeight = res.height;
							setDefault = true;
						}
					} else {
						break;
					}
				}

				if (!setFirst && !setDefault) {
					ResolutionWidth = Screen.currentResolution.width;
					ResolutionHeight = Screen.currentResolution.height;
				}
			}

			public Resolution GetNextResolution (int width, int height)
			{
				Resolution nextResolution = Screen.currentResolution;
				bool foundCurrent = false;
				for (int i = 0; i < mSupportedResolutions.Count; i++) {
					if (!foundCurrent) {
						if (mSupportedResolutions [i].width == width && mSupportedResolutions [i].height == height) {
							foundCurrent = true;
						}
					} else {
						nextResolution = mSupportedResolutions [i];
						break;
					}
				}
				return nextResolution;
			}

			public Resolution GetPrevResolution (int width, int height)
			{
				Resolution prevResolution = Screen.currentResolution;
				bool foundCurrent = false;
				for (int i = mSupportedResolutions.Count - 1; i >= 0; i--) {
					if (!foundCurrent) {
						if (mSupportedResolutions [i].width == width && mSupportedResolutions [i].height == height) {
							foundCurrent = true;
						}
					} else {
						prevResolution = mSupportedResolutions [i];
						break;
					}
				}
				return prevResolution;
			}

			public int GetPrevTextureResolution (int index)
			{
				return Mathf.Clamp ((index - 1), 0, 3);
			}

			public int GetNextTextureResolution (int index)
			{
				return Mathf.Clamp ((index + 1), 0, 3);
			}

			public int GetNextLODDistance (int index)
			{
				return Mathf.Clamp ((index + 1), 0, 5);
			}

			public int GetPrevLODDistance (int index)
			{
				return Mathf.Clamp ((index - 1), 0, 5);
			}

			public int GetNextShadowSetting (int index)
			{
				return Mathf.Clamp ((index + 1), 0, 5);
			}

			public int GetPrevShadowSetting (int index)
			{
				return Mathf.Clamp ((index - 1), 0, 5);
			}

			protected bool SetOrDisablePostEffect (MonoBehaviour postEffect, bool requiresHDR, ref bool enabled)
			{
				if (postEffect == null) {
					enabled = false;
					return false;
				}
				bool checkHDR = false;
				if (requiresHDR && enabled && !HDR) {
					checkHDR = true;
					HDR = true;
				}
				postEffect.enabled = enabled;
				enabled = postEffect.enabled;
				if (checkHDR && !enabled) {
					HDR = false;
				}
				return enabled;
			}

			public void RefreshSupportedResolutions ()
			{
				if (mSupportedResolutions == null) {
					mSupportedResolutions = new List<Resolution> ();
				}

				mSupportedResolutions.Clear ();
				foreach (Resolution res in Screen.resolutions) {
					mSupportedResolutions.Add (res);
					/*
										float aspectRatio = ((float)res.width / (float)res.height);
										if (Mathf.Approximately(aspectRatio, DeviceAspectRatio)) {
												mSupportedResolutions.Add(res);
										}
										*/
				}
				mSupportedResolutions.Sort (delegate (Resolution r1, Resolution r2) {
					return r1.width.CompareTo (r2.width);
				});
			}

			[NonSerialized]
			[XmlIgnore]
			protected List <Resolution> mSupportedResolutions = new List <Resolution> ();

			#endregion

		}

		[Serializable]
		public class ModPrefs
		{
			public static ModPrefs Default ()
			{
				ModPrefs prefs = new ModPrefs ();
				prefs.CurrentWorld	= "FRONTIERS";
				prefs.EnabledMods	= new List<string> ();
				return prefs;
			}

			public void Apply ()
			{

			}

			public string CurrentWorld = string.Empty;
			public List <string> EnabledMods = new List <string> ();
		}
	}

	[Serializable]
	public class PlayerExperience
	{
		public SDictionary <string, int> ExperienceByFlagset = new SDictionary <string, int> ();
		public SDictionary <string, int> LastCredentialsByFlagset = new SDictionary <string, int> ();

		public void SetLastCredentials (string flagset, int credentials)
		{
			if (!LastCredentialsByFlagset.ContainsKey (flagset)) {
				LastCredentialsByFlagset.Add (flagset, credentials);
			} else {
				LastCredentialsByFlagset [flagset] = credentials;
			}
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.SkillCredentialsGain, WorldClock.AdjustedRealTime);
		}

		public int LastCredByFlagset (string flagset)
		{
			int cred = 0;
			LastCredentialsByFlagset.TryGetValue (flagset, out cred);
			return cred;
		}

		public int ExpByFlagset (string flagSet)
		{
			int exp = 0;
			ExperienceByFlagset.TryGetValue (flagSet, out exp);
			return exp;
		}

		public void AddExperience (int experience, string flagSet)
		{
			int currentExperience = 0;
			if (!ExperienceByFlagset.TryGetValue (flagSet, out currentExperience)) {
				ExperienceByFlagset.Add (flagSet, 0);
			}
			ExperienceByFlagset [flagSet] = currentExperience + experience;

			Player.Get.AvatarActions.ReceiveAction (AvatarAction.SkillExperienceGain, WorldClock.AdjustedRealTime);
			//credentials will be handled by Skills
		}

		public void RemoveExperience (int experience, string flagSet)
		{
			int currentExperience = 0;
			if (!ExperienceByFlagset.TryGetValue (flagSet, out experience)) {
				ExperienceByFlagset.Add (flagSet, 0);
			}
			ExperienceByFlagset [flagSet] = Mathf.Max (currentExperience - experience, 0);

			Player.Get.AvatarActions.ReceiveAction (AvatarAction.SkillExperienceLose, WorldClock.AdjustedRealTime);
			//credentials will be handled by Skills
		}
	}
}
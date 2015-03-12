using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//commenty comment comment hello 27
namespace Frontiers
{
		public class CameraFX : Manager
		{
				public static CameraFX Get;
				public AnimationCurve BrightCurve;
				public AnimationCurve DarkCurve;
				public AnimationCurve NormalCurve;
				//changed on the fly
				public AnimationCurve FinalCurve;

				public int AdjustBrightness {
						get {
								return mAdjustBrightness;
						}
						set {
								//this is ugly but whatever
								//good enough for 30 min of work
								mAdjustBrightness = value;
								if (mAdjustBrightness == 50) {
										//we can turn off the camera effect
										Default.CC.enabled = false;
								} else {
										Default.CC.enabled = true;
										//now set the brightness curves
										if (mAdjustBrightness < 50) {
												//blend the curve with 
												float blend = 1f - (((float)mAdjustBrightness) / 50);
												Keyframe[] keys = FinalCurve.keys;
												Keyframe[] normalKeys = NormalCurve.keys;
												Keyframe[] darkKeys = DarkCurve.keys;
												for (int i = 0; i < keys.Length; i++) {
														keys[i].value = Mathf.Lerp(normalKeys[i].value, darkKeys[i].value, blend);
														keys[i].inTangent = Mathf.Lerp(normalKeys[i].inTangent, darkKeys[i].inTangent, blend);
														keys[i].outTangent = Mathf.Lerp(normalKeys[i].outTangent, darkKeys[i].outTangent, blend);
												}
												FinalCurve.keys = keys;
										} else {
												float blend = ((float)mAdjustBrightness - 50) / 50;
												Keyframe[] keys = FinalCurve.keys;
												Keyframe[] normalKeys = NormalCurve.keys;
												Keyframe[] brightKeys = BrightCurve.keys;
												for (int i = 0; i < keys.Length; i++) {
														keys[i].value = Mathf.Lerp(normalKeys[i].value, brightKeys[i].value, blend);
														keys[i].inTangent = Mathf.Lerp(normalKeys[i].inTangent, brightKeys[i].inTangent, blend);
														keys[i].outTangent = Mathf.Lerp(normalKeys[i].outTangent, brightKeys[i].outTangent, blend);
												}
												FinalCurve.keys = keys;
										}
										Default.CC.redChannel.keys = FinalCurve.keys;
										Default.CC.greenChannel.keys = FinalCurve.keys;
										Default.CC.blueChannel.keys = FinalCurve.keys;
										Default.CC.UpdateTextures();
								}
						}
				}

				public OVRManager OvrManager;
				public OVRCameraRig OvrCameraRig;
				public ScreenEffectsSet Default;
				public ScreenEffectsSet OvrRight;
				public ScreenEffectsSet OvrLeft;
				public ParticleEmitter BurningParticles;
				public ParticleSystem LocalSnow;
				public ParticleSystem LocalRain;
				public ParticleSystem LocalWind;
				public ParticleSystem LocalDust;
				public ParticleSystem LocalFog;
				public Texture2D SpyglassDirtTexture;
				public Texture2D DamageOverlayTexture;
				public Texture2D FadeOutOverlayTexture;
				public float ContrastTargetStrength;
				public float MildHallucinationsStrength = 0.0f;
				public float MildHallucinationsStrengthTarget = 0.0f;
				public float ModerateHallucinationsStrengthTarget = 0.0f;
				public float ModerateHallucinationsStrength = 0.0f;
				public float RainEmissionRate = 200;
				public float SnowEmissionRate = 200;
				public Texture2D BackburnerLUT;

				public Texture2D CurrentLUT {
						get {
								return Default.ColorGrading.LutTexture;
						}
						set {
								Default.ColorGrading.LutTexture = value;
						}
				}

				public Texture2D BlendLUT {
						get {
								return Default.ColorGrading.LutBlendTexture;
						}
						set {
								Default.ColorGrading.LutBlendTexture = value;
						}
				}

				public override void Awake()
				{
						mParentUnderManager = false;
						base.Awake();
				}

				public override void WakeUp()
				{
						Get = this;
						BlendLUT = CurrentLUT;
						Default.ColorGrading.LutBlendTexture = BlendLUT;
						Default.ColorGrading.BlendAmount = 0f;

						//load all the screen effects components
						Default.Initialize();
						OvrLeft.Initialize();
						OvrRight.Initialize();
				}

				public override void OnGameStart()
				{
						ActionListener survivalTakeDamage = new ActionListener(SurvivalTakeDamage);

						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamage, survivalTakeDamage);
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamageCritical, survivalTakeDamage);
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamageOverkill, survivalTakeDamage);
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDie, new ActionListener(SurvivalDie));

						Default.Overlay.texture = DamageOverlayTexture;
						Default.Overlay.intensity = 0.0f;
						enabled = true;
				}

				public void	Update()
				{
						if (!mInitialized) {
								return;
						}

						//enable / disable cameras based on oculus mode
						if (VRManager.OculusModeEnabled) {
								OvrLeft.CopyFrom(Default);
								OvrRight.CopyFrom(Default);
								OvrLeft.cam.farClipPlane = Default.cam.farClipPlane;
								OvrRight.cam.farClipPlane = Default.cam.farClipPlane;
						}

						if (GameManager.Is(FGameState.InGame) && Player.Local != null) {
								UpdateBlackout();
								UpdateParticles();
								UpdateOverlay();
								UpdateHallucinations();
						}
				}

				public void RefreshOculusMode()
				{
						if (VRManager.OculusModeEnabled) {
								Default.cam.enabled = false;
								OvrLeft.cam.enabled = true;
								OvrRight.cam.enabled = true;
								OvrLeft.cam.cullingMask = Default.cam.cullingMask;
								OvrRight.cam.cullingMask = Default.cam.cullingMask;
								OvrManager.enabled = true;
								OvrCameraRig.enabled = true;
						} else {
								OvrManager.enabled = false;
								OvrCameraRig.enabled = false;
								OvrLeft.cam.enabled = false;
								OvrRight.cam.enabled = false;
								Default.cam.enabled = true;
						}
				}

				public string LUTName {
						get {
								if (CurrentLUT != null) {
										return CurrentLUT.name;
								}
								return string.Empty;
						}
				}

				public float LUTBlendAmount {
						get {
								return Default.ColorGrading.BlendAmount;
						}
						set {
								//Debug.Log ("Setting blend amount to " + value.ToString ());
								Default.ColorGrading.BlendAmount = value;
						}
				}

				public void SetLUT(Texture2D NewLUT)
				{
						//Debug.Log ("Setting LUT to " + NewLUT.name);
//			if (CurrentLUT.name == "Normal") {
//				CurrentLUT = NewLUT;
//				LUTBlendAmount = 0f;
//				return;
//			}
						if (mIsBlending) {
								if (BlendLUT.name != NewLUT.name) {
										//Debug.Log ("Well blend to " + NewLUT.name + " once we're done with current blend");
										BackburnerLUT = NewLUT;
								}
						} else {
								if (CurrentLUT.name != NewLUT.name) {
										BlendLUT = NewLUT;
										mIsBlending = true;
										//Debug.Log ("Beginning blend from " + CurrentLUT.name + " to " + NewLUT.name);
										StartCoroutine(BlendLUTOverTime(Globals.LUTBlendSpeed));
								}
						}
				}

				protected IEnumerator BlendLUTOverTime(float blendDuration)
				{

						if (!GameManager.Is(FGameState.InGame)) {
								//for cutscenes / loading / etc. we want to cut to the chase
								blendDuration = 0.01f;
						}

						float startTime = Time.time;//TEMP
						float endTime = startTime + blendDuration;
						float normalizedBlend = 0f;


						LUTBlendAmount = 0f;
						while (normalizedBlend < 1f) {
								normalizedBlend = (Time.time - startTime) / (endTime - startTime);
								LUTBlendAmount = normalizedBlend;
								yield return null;
						}
						//Debug.Log ("Done blending from " + Default.ColorGrading.LutTexture.name + " to " + Default.ColorGrading.LutBlendTexture.name);
						//now that we're done blending, move the blended LUT to the primary LUT and set the blend back to zero
						CurrentLUT = BlendLUT;
						LUTBlendAmount = 0f;
						//we may have been asked to blend to something
						if (BackburnerLUT != null) {
								//if we have, continue blending that
								BlendLUT = BackburnerLUT;
								BackburnerLUT = null;
								StartCoroutine(BlendLUTOverTime(blendDuration));
						} else {
								//otherwise just finish
								//BlendLUT = null;
								mIsBlending = false;
						}
						yield break;
				}

				public void	SetSunTransform(Transform sunTransform)
				{
						Default.SunShaftsEffect.sunTransform = sunTransform;
				}

				public void HallucinateMildStart()
				{
						MildHallucinationsStrengthTarget = 1.0f;
				}

				public void	HallucinateMildEnd()
				{
						MildHallucinationsStrengthTarget	= 0.0f;
				}

				public void	HallucinateModerateStart()
				{
						HallucinateMildStart();
				}

				public void	HallucinateModerateEnd()
				{
						HallucinateMildEnd();
				}

				public void	HallucinateStrongStart()
				{
						HallucinateMildStart();
				}

				public void	HallucinateStrongEnd()
				{
						HallucinateMildEnd();
				}

				public void SetBlind(bool blind)
				{
						mGoingBlind = blind;
						if (mGoingBlind) {
								mBlackingOut = false;
						}
				}

				public void SetSpyglass(bool spyglass)
				{
						mSpyglass = spyglass;
						if (!mSpyglass) {
								//set the vignette to zero right away
								//if we're blacking out this will be corrected immediately
								Default.Vignette.intensity = 0f;
								Default.BloomEffect.enabled = Profile.Get.CurrentPreferences.Video.PostFXBloom;
								Default.BloomEffect.lensDirtTexture = null;
								Default.BloomEffect.lensDirtIntensity = 0f;
						} else {
								Default.BloomEffect.enabled = true;
								Default.BloomEffect.lensDirtTexture = SpyglassDirtTexture;
								Default.BloomEffect.lensDirtIntensity = 1f;
						}
				}

				public void BlackOut(float duration, float intensity)
				{
						//if we're going blind there's no point
						if (mGoingBlind) {
								return;
						}
						mBlackingOut = true;
						//Debug.Log ("Blackout! " + duration.ToString ());
						mBlackoutTargetIntensity = intensity;
						mBlackoutDuration = duration;
						mBlackoutStart = WorldClock.AdjustedRealTime;
				}

				public void	FadeOut(GameObject waitingForFadeOut)
				{
						mWaitingForFadeOut = waitingForFadeOut;
						StartCoroutine(FadeOutOverTime());
				}

				public void	FadeIn(GameObject waitingForFadeIn)
				{
						mWaitingForFadeIn = waitingForFadeIn;
						StartCoroutine(FadeInOverTime());
				}

				public bool	SurvivalTakeDamage(double timeStamp)
				{
						AddDamageOverlay(Player.Local.DamageHandler.DamageLastTaken * mDamageIntensityMultiplier);
						return true;
				}

				public void AddDamageOverlay(float intensity)
				{
						Default.Overlay.texture = DamageOverlayTexture;
						//		Overlay.blendMode 		= ScreenOverlay.OverlayBlendMode.Additive;
						Default.Overlay.intensity = 0.0f;

						Default.Overlay.intensity = Mathf.Clamp((Default.Overlay.intensity + intensity * mDamageIntensityMultiplier), 0.0f, mDamageIntenistyMax);
				}

				protected void UpdateHallucinations()
				{
						//TODO implement hallucinations that won't make the player sick
//						bool hallucinations = false;
//						if (Default.MildHallucinations.enabled) {
//								hallucinations = true;
//								MildHallucinationsStrength = Mathf.Lerp(MildHallucinationsStrength, MildHallucinationsStrengthTarget, 0.1f);
//								Default.MildHallucinations.angle = MildHallucinationsStrength * ((Mathf.PingPong((float)(WorldClock.RealTime * 0.125f), 1.0f) - 0.5f) * 15.0f);
//								if (MildHallucinationsStrengthTarget == 0 && MildHallucinationsStrength <= 0.001f) {
//										Default.MildHallucinations.enabled = false;
//								}
//						}
//
//						if (hallucinations) {
//								ContrastTargetStrength = 1.5f;
//						} else {
//								ContrastTargetStrength = 0.0f;
//						}
//
//						if (ContrastTargetStrength == 0 && Default.HallucinationSharpen.intensity <= 0.001f) {
//								Default.HallucinationSharpen.intensity = 0f;
//						} else {
//								Default.HallucinationSharpen.intensity = Mathf.Lerp(Default.HallucinationSharpen.intensity, ContrastTargetStrength, 0.125f);
//						}
				}

				protected void UpdateOverlay()
				{
						if (Player.Local.IsDead) {
								Default.Overlay.intensity = 0.75f;
						} else {
								if (Default.Overlay.intensity > 0.0f) {
										Default.Overlay.intensity -= (float)(mDamageOverlayReductionRate * Frontiers.WorldClock.ARTDeltaTime);
								} else if (Default.Overlay.intensity < 0.0f) {
										Default.Overlay.intensity = 0.0f;
								}
						}
				}

				protected void UpdateParticles()
				{
						if (Player.Local.Status.HasCondition("BurnedByFire")) {
								BurningParticles.emit = true;
						} else {
								BurningParticles.emit = false;
						}

						if (Player.Local.Surroundings.IsOutside) {
								if (Biomes.Get.IsRaining) {
										LocalRain.emissionRate = Biomes.Get.RainIntensity * RainEmissionRate;
								} else {
										LocalRain.emissionRate = 0;
								}

								if (Biomes.Get.IsSnowing) {
										LocalSnow.emissionRate = Biomes.Get.SnowIntensity * SnowEmissionRate;
								} else {
										LocalSnow.emissionRate = 0;
								}
						} else {
								LocalSnow.emissionRate = 0;
								LocalRain.emissionRate = 0;
						}
				}

				protected void UpdateBlackout()
				{
						if (mSpyglass) {
								Default.Vignette.enabled = true;
								Default.Vignette.intensity = 5f;
								Default.Vignette.chromaticAberration = 5f;
						} else if (mGoingBlind) {
								//Debug.Log ("We're going blind!");
								//ramp up vingette intensity to max over time
								Default.Vignette.enabled = true;
								Default.Vignette.intensity = Mathf.Lerp(Default.Vignette.intensity, 1000f, 0.001f);
								Default.Vignette.blur = Mathf.Lerp(Default.Vignette.blur, 1000f, 0.05f);
								Default.Vignette.chromaticAberration = Mathf.Lerp(Default.Vignette.chromaticAberration, 1000f, 0.025f);
						} else if (mBlackingOut) {
								mBlackoutIntensity = Mathf.Lerp(mBlackoutIntensity, mBlackoutTargetIntensity, (float)WorldClock.ARTDeltaTime);

								if (Mathf.Approximately(mBlackoutIntensity, 0f)) {
										Default.Vignette.enabled = false;
										mBlackingOut = false;
								} else {
										//Debug.Log ("Blackout! " + mBlackoutTargetIntensity.ToString ());
										if (!Default.Vignette.enabled) {
												Default.Vignette.enabled = true;
												Default.Vignette.intensity = 0f;
										} else {
												Default.Vignette.intensity = mBlackoutIntensity * mBlackoutMultiplier;
												Default.Vignette.blur = mBlackoutIntensity;
												Default.Vignette.chromaticAberration = mBlackoutIntensity;
										}
										if (WorldClock.AdjustedRealTime > mBlackoutStart + mBlackoutDuration) {
												mBlackoutTargetIntensity = 0f;
										}
								}
						} else {
								if (Default.Vignette.enabled) {
										if (Mathf.Approximately(Default.Vignette.intensity, 0f)) {
												Default.Vignette.intensity = 0f;
												Default.Vignette.enabled = false;
										} else {
												Default.Vignette.intensity = Mathf.Lerp(Default.Vignette.intensity, 0f, (float)WorldClock.ARTDeltaTime);
												Default.Vignette.blur = Mathf.Lerp(Default.Vignette.blur, 0f, (float)WorldClock.ARTDeltaTime);
												Default.Vignette.chromaticAberration = Default.Vignette.blur;
										}
								}
						}

				}

				public bool SurvivalDie(double timeStamp)
				{
						return true;
				}

				public IEnumerator FadeInOverTime()
				{

						yield break;
				}

				public IEnumerator FadeOutOverTime()
				{
						yield break;
				}

				public int existingCullingMask = 0;
				protected int mAdjustBrightness = 50;
				protected bool mIsBlending = false;
				protected GameObject mWaitingForFadeOut = null;
				protected GameObject mWaitingForFadeIn = null;
				protected float mDamageIntensityMultiplier = 0.15f;
				protected float mDamageOverlayReductionRate = 0.5f;
				protected float mDamageIntenistyMax = 1.0f;
				protected float mBlackoutMultiplier = 2f;
				protected float mBlackoutIntensity = 0f;
				protected double mBlackoutStart = 0f;
				protected float mBlackoutTargetIntensity = 0f;
				protected float mBlackoutDuration = 0f;
				//vignette settings
				protected bool mGoingBlind = false;
				protected bool mBlackingOut = false;
				protected bool mSpyglass = false;
		}

		[Serializable]
		public class ScreenEffectsSet
		{
				public void Initialize()
				{
						BloomEffect = cam.GetComponent <SENaturalBloomAndDirtyLens>();
						Vignette = cam.GetComponent <Vignetting>();
						Blur = cam.GetComponent <BlurEffect>();
						ColorGrading = cam.GetComponent <Color3Grading>();
						Overlay = cam.GetComponent <ScreenOverlay>();
						HallucinationSharpen = cam.GetComponent <ContrastEnhance>();
						SunShaftsEffect = cam.GetComponent <SunShafts>();
						SSAO = cam.GetComponent <SSAOPro>();
						Grain = cam.GetComponent <NoiseAndGrain>();
						CC = cam.GetComponent <ColorCorrectionCurves>();
						AA = cam.GetComponent <AntialiasingAsPostEffect>();
						Fog = cam.GetComponent <AlphaSortedGlobalFog>();
						TimeOfDay = cam.GetComponent <TOD_Camera>();

						Components = new List<MonoBehaviour>();
						Components.Add(BloomEffect);
						Components.Add(Vignette);
						Components.Add(Blur);
						Components.Add(ColorGrading);
						Components.Add(Overlay);
						Components.Add(HallucinationSharpen);
						Components.Add(SunShaftsEffect);
						Components.Add(SSAO);
						Components.Add(Grain);
						Components.Add(CC);
						Components.Add(AA);
						Components.Add(Fog);
						//skip TOD_Camera
				}

				public void CopyFrom(ScreenEffectsSet sfxSet)
				{		//instead of setting values on fx etc. 3 times (left / right / normal) we just copy / paste from the default camera
						for (int i = 0; i < sfxSet.Components.Count; i++) {
								//TODO this is pretty absurd - we can cache the fields in the components list
								//for now, whatever, just copy them and allocate a bunch of crap...
								MonoBehaviour thisComponent = Components[i];
								MonoBehaviour otherComponent = sfxSet.Components[i];
								if (thisComponent != null && otherComponent != null) {
										thisComponent.enabled = otherComponent.enabled;
										/*Type type = thisComponent.GetType();
					System.Reflection.FieldInfo[] fields = type.GetFields(); 
					foreach (System.Reflection.FieldInfo field in fields) {
						field.SetValue(thisComponent, field.GetValue(otherComponent));
					}*/
								}
						}
				}

				public Camera cam;
				public SENaturalBloomAndDirtyLens BloomEffect;
				public Vignetting Vignette;
				public BlurEffect Blur;
				public Color3Grading ColorGrading;
				public ScreenOverlay Overlay;
				public ContrastEnhance HallucinationSharpen;
				public SunShafts SunShaftsEffect;
				public SSAOPro SSAO;
				public NoiseAndGrain Grain;
				public ColorCorrectionCurves CC;
				public AntialiasingAsPostEffect AA;
				public AlphaSortedGlobalFog Fog;
				public TOD_Camera TimeOfDay;
				public List <MonoBehaviour> Components;
		}
}
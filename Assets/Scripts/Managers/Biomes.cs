using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;

namespace Frontiers
{
		public class Biomes : Manager
		{
				public static Biomes Get;

				#region initialization

				public override void WakeUp()
				{
						Get = this;
				}

				public override void Initialize()
				{
						SunLight = GameWorld.Get.Sky.Components.LightSource;
						SunLight.transform.parent = null;
						SunLight.transform.localScale = Vector3.one;
						SunLight.transform.localPosition = Vector3.zero;
						SunPositionObject = gameObject.FindOrCreateChild("SunPositionObject").gameObject;
						SunPositionObject.transform.parent = GameWorld.Get.Sky.Components.LightTransform;
						SunPositionObject.transform.ResetLocal();
						CameraFX.Get.SetSunTransform(SunPositionObject.transform);
						RenderSettings.ambientLight = Color.black;

						mInitialized = true;
				}

				public override void OnGameStart()
				{
						InvokeRepeating("UpdateWeather", 0.001f, 5f);
				}

				#endregion

				public AtmoParticleManager AtmoParticles;
				public Light SunLight;
				public LightningBolt LightningBoltPrefab;
				public GameObject SunPositionObject;
				public float WindForceMultiplier;
				public float TideMaxDifference = 15.0f;
				public float TideElevationOffset = 0.0f;
				public float TideSpeed = 0.01f;
				public Color3Grading ColorGrading;
				public bool UseTimeOfDayOverride = false;
				public float HourOfDayOverride = 0f;
				public float PrecipitationIntensity;
				public float RainIntensity;
				public float SnowIntensity;
				public float WindIntensity;
				public float MistIntensity;

				public float ThunderIntensity {
						get {
								return Mathf.Clamp01(mAmbientThunder + mLightningThunder);
						}
				}

				public void LightingFlash(Vector3 worldPosition)
				{
						mLightningFlash = 1f;
						mLightningThunder = 1f;
						mDefaultBrightness = GameWorld.Get.Sky.Atmosphere.Brightness;
				}

				public bool IsRaining {
						get {
								return RainIntensity > 0.25f;
						}
				}

				public bool IsSnowing {
						get {
								return SnowIntensity > 0.25f;
						}
				}

				public Vector3 SunLightPosition {
						get {
								return SunLight.transform.position;
						}
				}

				public float TideWaterElevation;

				public float TideMaxElevation {
						get {
								return TideElevationOffset + TideMaxDifference;
						}
				}

				public void SpawnRandomLightning()
				{
						//spawn a lightning bolt randomly within [x] meters to player
						mTerrainHit.groundedHeight = 100f;
						mTerrainHit.feetPosition = Player.Local.Position + (new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)) * 500f);
						mTerrainHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);
						GameObject.Instantiate(LightningBoltPrefab, mTerrainHit.feetPosition, Quaternion.identity);
				}

				#region update

				public void Update()
				{
						if (!mInitialized || Player.Local == null || GameManager.Get.TestingEnvironment) {
								return;
						}

						if (GameManager.Is(FGameState.Cutscene)) {
								//cutscenes can make it look like a different time of day
								if (Cutscene.CurrentCutscene.FreezeApparentTime) {
										GameWorld.Get.Sky.Cycle.Hour = Cutscene.CurrentCutscene.ApparentHourOfDay;
								}
						}
		
						if (UseTimeOfDayOverride) {
								GameWorld.Get.Sky.Cycle.Hour = HourOfDayOverride;
						} else {
								//update the sky so it knows what time it is
								//smooth it out so we don't get herky jerky lighting
								//make sure not to smooth it if we've looped, though
								float newHour = (float)(WorldClock.DayCycleCurrentNormalized * 24.0);
								if (GameWorld.Get.Sky.Cycle.Hour > newHour) {
										GameWorld.Get.Sky.Cycle.Hour = newHour;
								} else {
										GameWorld.Get.Sky.Cycle.Hour = Mathf.Lerp(GameWorld.Get.Sky.Cycle.Hour, newHour, 0.5f);
								}
								GameWorld.Get.Sky.Cycle.MoonPhase = (float)(WorldClock.MoonCycleCurrentNormalized);
								if (WorldClock.SkippingAhead) {
										//make things look like they're rustling a lot
										GameWorld.Get.SkyAnimation.WindSpeed = WindIntensity + 5f;
								} else {
										//otherwise just look normal
										GameWorld.Get.SkyAnimation.WindSpeed = WindIntensity;
								}
						}
						//if we're not in game that's all we need to do right now
						if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused | FGameState.GameLoading)) {
								return;
						}

						bool isUnderground = Player.Local.Surroundings.State.IsUnderground;
						bool isOutside = Player.Local.Surroundings.IsOutside;
						float transitionTime = 0.0f;
						if (isUnderground) {
								transitionTime = (float)(Player.Local.Surroundings.State.TimeSinceEnteredUnderground / Globals.AmbientLightTransitionTime);
						} else {
								transitionTime = (float)(Player.Local.Surroundings.State.TimeSinceExitedUnderground / Globals.AmbientLightTransitionTime);
						}

						if (GameWorld.Get.WorldLoaded) {
								//wind and snow and rain
								if (Player.Local.Surroundings.IsOutside) {
										if (GameWorld.Get.CurrentBiome.Climate == ClimateType.Arctic || GameWorld.Get.CurrentRegionData.g > 0) {
												SnowIntensity = PrecipitationIntensity;
												RainIntensity = 0f;
										} else {
												RainIntensity = PrecipitationIntensity;
												SnowIntensity = 0f;
										}
								} else {
										RainIntensity = 0f;
										SnowIntensity = 0f;
								}
								//update tides
								double minorVariation = Math.Abs(Math.Sin(WorldClock.AdjustedRealTime * GameWorld.Get.CurrentBiome.WaveSpeed * Globals.WaveSpeed)) * GameWorld.Get.CurrentBiome.WaveIntensity;
								double tideWaterElevation = Math.Abs(Math.Sin(WorldClock.DayCycleCurrentNormalized)) * (GameWorld.Get.TideBaseElevationAtPlayerPosition * GameWorld.Get.CurrentBiome.TideVariation) + GameWorld.Get.CurrentBiome.TideBaseElevation;
								TideWaterElevation = (float)(tideWaterElevation + minorVariation);
								//TODO update rain and wind based on biomes
								PrecipitationIntensity = Mathf.Lerp(PrecipitationIntensity, mTargetPrecipitationIntensity, (float)WorldClock.ARTDeltaTime);
								WindIntensity = Mathf.Lerp(WindIntensity, mBaseWindIntensity, (float)WorldClock.ARTDeltaTime);

								//update season effects based on biome
								UpdateSeason();

								//update screen effects with biome information
								CameraFX.Get.Default.SunShaftsEffect.sunColor = GameWorld.Get.Sky.SunColor;
								if (isOutside) {
										CameraFX.Get.Default.SunShaftsEffect.sunShaftIntensity = 0.25f * GameWorld.Get.Sky.Atmosphere.AmbientIntensityMultiplier;
								} else {
										CameraFX.Get.Default.SunShaftsEffect.sunShaftIntensity = 1f * GameWorld.Get.Sky.Atmosphere.AmbientIntensityMultiplier;
								}
								//check if color correction is correct based on biome
								if (CameraFX.Get.LUTName != GameWorld.Get.CurrentBiome.ColorSetting) {
										Texture2D colorSetting = null;
										if (Mods.Get.Runtime.CameraLUT(ref colorSetting, GameWorld.Get.CurrentBiome.ColorSetting)) {
												CameraFX.Get.SetLUT(colorSetting);
										} else {
												Debug.Log("Couldn't load LUT setting");
										}
								}
								//finally update fog and ambient light based on biome
								float lightIntensityLerpTime = 0.1f;
								float ambientIntensityLerpTime = 0.1f;

								//since we clear to color this will prevent black outlines
								if (GameManager.Is(FGameState.GameLoading)) {
										mAmbientLightIntensity = Mathf.Lerp(mAmbientLightIntensity, 0f, 0.5f);
								} else {
										mAmbientColor = Color.black;
										//g = forest * 0.5
										//b = civilization * 1.05
										Color terrainType = Player.Local.Surroundings.TerrainType;
										//at night, forests are pitch black
										//in daytime, they're half dark
										//at night, civilization is lighter than usual
										//in daytime, it's normal
										if (isUnderground) {
												mAmbientLightIntensity = Globals.AmbientLightIntensityUnderground;
												mSunlightIntensity = 0f;
												lightIntensityLerpTime = 5f;
												ambientIntensityLerpTime = 3f;
												mAmbientColor = Colors.Get.BelowGroundAmbientColor;
										} else {
												mShadowStrengthBooster = terrainType.g;
												if (WorldClock.IsDay) {
														if (isOutside) {
																mAmbientColor = Colors.Get.AboveGroundDayAmbientColor;
																mAmbientLightIntensity = Globals.AmbientLightIntensityDay;//Mathf.Lerp (Globals.AmbientLightIntensity, Globals.AmbientLightIntensityForest, terrainType.g);
																mSunlightIntensity = 1.0f + (terrainType.g * 2);//adding the sunlight intensity offsets the loss of the ambient light
														} else {
																mAmbientColor = Colors.Get.InteriorAmbientColorDay;
																mAmbientLightIntensity = Globals.AmbientLightIntensityInteriorDay;
																mSunlightIntensity = 1.0f / Globals.AmbientLightIntensityInteriorDay;//adding the sunlight intensity offsets the loss of the ambient light
														}
														mSunlightIntensity = 1.0f;
												} else {
														if (isOutside) {
																mAmbientColor = Colors.Get.AboveGroundNightAmbientColor;
																mAmbientLightIntensity = Mathf.Lerp(Globals.AmbientLightIntensityNight, Globals.AmbientLightIntensityForest, terrainType.g);
																mSunlightIntensity = 1.0f + (terrainType.g * 2);//adding the sunlight intensity offsets the loss of the ambient light
														} else {
																mAmbientColor = Colors.Get.InteriorAmbientColorNight;
																mAmbientLightIntensity = Globals.AmbientLightIntensityNight * Globals.AmbientLightIntensityInteriorNight;
																mSunlightIntensity = 1.0f / Globals.AmbientLightIntensityInteriorNight;//adding the sunlight intensity offsets the loss of the ambient light
														}
												}
										}
										mAmbientLightIntensity *= GameWorld.Get.CurrentBiome.AmbientLightMultiplier;
										mSunlightIntensity *= GameWorld.Get.CurrentBiome.SunlightIntensityMultiplier;
								}

								mFogDistance = Globals.DefaultFogDistance * GameWorld.Get.CurrentBiome.FogDistanceMultiplier;
								//reduce fog intensity by rain amount
								//the more rain, the more fog
								mFogDistance = mFogDistance * (1.0f - RainIntensity);

								if (mCurrentWeatherQuarter != null) {
										if (mCurrentWeatherQuarter.Weather == TOD_Weather.WeatherType.Fog) {
												mFogDistance = mFogDistance * 0.5f;
										}
								}
								//mAmbientLightIntensity = Mathf.Lerp (mAmbientLightIntensity, 1.0f * primaryChunk.BiomeData.AmbientLightMultiplier, terrainType.b);
								mSmoothAmbientLightIntensity = Mathf.Lerp(mSmoothAmbientLightIntensity, mAmbientLightIntensity, (float)(WorldClock.RTDeltaTime * ambientIntensityLerpTime));
								mSmoothSunlightIntensity = Mathf.Lerp(mSmoothSunlightIntensity, mSunlightIntensity, (float)(WorldClock.RTDeltaTime * lightIntensityLerpTime));
								mSmoothShadowStrengthBooster = Mathf.Lerp(mSmoothShadowStrengthBooster, mShadowStrengthBooster, (float)WorldClock.RTDeltaTime);
								mAmbientColor = Color.Lerp(Color.black, mAmbientColor, mSmoothAmbientLightIntensity);
								mAmbientColor = Color.Lerp(GameWorld.Get.Sky.AmbientColor, mAmbientColor, 0.5f);
								mAmbientColorSmooth = Color.Lerp(mAmbientColorSmooth, mAmbientColor, (float)WorldClock.ARTDeltaTime);
								mSmoothFogDistance = Mathf.Lerp(mSmoothFogDistance, mFogDistance, lightIntensityLerpTime);

								GameWorld.Get.Sky.Atmosphere.AmbientIntensityMultiplier = mSmoothAmbientLightIntensity;
								GameWorld.Get.Sky.Day.LightIntensityMultiplier = mSmoothSunlightIntensity;
								GameWorld.Get.Sky.Day.ShadowStrengthBooster = mSmoothShadowStrengthBooster;
						}
						//do lightning flashes even if gameworld isn't loaded
						if (mLightningFlash > 0f) {
								RenderSettings.ambientLight = Color.Lerp(Colors.Get.LightningFlashColor, mAmbientColorSmooth, mLightningFlash);
								GameWorld.Get.Sky.Atmosphere.Brightness = mDefaultBrightness + mLightningFlash;
								mLightningFlash = Mathf.Lerp(mLightningFlash, 0f, 0.65f);
								if (mLightningFlash < 0.001f) {
										mLightningFlash = 0f;
								}
						} else {
								RenderSettings.ambientLight = mAmbientColorSmooth;
						}

						//finally, add the ambient light booster to ambient lighting
						RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, Color.white, Profile.Get.CurrentPreferences.Video.AmbientLightBooster * Globals.MaxAmbientLightBoost);

						RenderSettings.fog = true;
						RenderSettings.fogMode = FogMode.Linear;
						RenderSettings.fogStartDistance	= 0f;
						RenderSettings.fogEndDistance = mSmoothFogDistance;
				}

				public void FixedUpdate()
				{
						if (GameManager.Is(FGameState.InGame)) {
								//atmo particles will take care of the rest
								AtmoParticles.PlayerPosition = Player.Local.Position;
						}
				}

				public void UpdateSeason()
				{
						if (!mInitialized) {
								return;
						}
						//TODO implement tree color changes
						//grass density changes
						//snow
				}

				protected Color mAmbientColor;
				protected Color mAmbientColorSmooth;
				protected float mFogDistance;
				protected float mSmoothFogDistance;
				protected float mLightningFlash;
				protected float mDefaultBrightness;

				#endregion

				public static List <Vector3> GeneratePointsOnSphere(int numberOfPoints)
				{
						List<Vector3> upts = new List<Vector3>();
						float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
						float off = 2.0f / numberOfPoints;
						float x;
						float y;
						float z;
						float r;
						float phi;

						for (int k = 0; k < numberOfPoints; k++) {
								y = k * off - 1 + (off / 2);
								r = Mathf.Sqrt(1 - y * y);
								phi = k * inc;
								x = Mathf.Cos(phi) * r;
								z = Mathf.Sin(phi) * r;

								upts.Add(new Vector3(x, y, z));
						}

						return upts;
				}

				public static List <Vector3> GeneratePointsOnSphere(int numberOfPoints, float scale)
				{
						List<Vector3> upts = new List<Vector3>();
						float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
						float off = 2.0f / numberOfPoints;
						float x;
						float y;
						float z;
						float r;
						float phi;

						for (int k = 0; k < numberOfPoints; k++) {
								y = k * off - 1 + (off / 2);
								r = Mathf.Sqrt(1 - y * y);
								phi = k * inc;
								x = Mathf.Cos(phi) * r;
								z = Mathf.Sin(phi) * r;

								upts.Add(new Vector3(x, y, z) * scale);
						}

						return upts;
				}

				public static float GetHumidity(ChunkBiomeData biomeState, float worldTime, Vector3 position)
				{
						UnityEngine.Random.seed = biomeState.GetHashCode();
						return UnityEngine.Random.value;
				}

				protected float mTargetPrecipitationIntensity = 0.0f;
				protected float mBaseWindIntensity = 0.0f;
				protected float mAmbientLightIntensity = 1.0f;
				protected float mSunlightIntensity = 1.0f;
				protected float mSmoothSunlightIntensity = 1.0f;
				protected float mSmoothAmbientLightIntensity = 1.0f;
				protected float mShadowStrengthBooster = 1.0f;
				protected float mSmoothShadowStrengthBooster = 1.0f;
				protected float mSmoothMiddleGrey = 0.5f;
				protected float mSmoothLightVal = 1.0f;
				protected bool mWasDaytimeLastFrame;
				protected WeatherQuarter mCurrentWeatherQuarter = null;
				protected float mLightningThunder = 0f;
				protected float mAmbientThunder;

				protected void UpdateWeather()
				{
						if (GameManager.Is(FGameState.InGame)) {
								WeatherQuarter weather = GameWorld.Get.CurrentBiome.GetWeather(WorldClock.Get.DayOfYear, WorldClock.Get.HourOfDay);
								if (weather != null) {
										GameWorld.Get.Sky.Components.Weather.Weather = weather.Weather;
										GameWorld.Get.Sky.Components.Weather.Clouds = weather.CloudType;
										mTargetPrecipitationIntensity = weather.Precipitation;
										mBaseWindIntensity = weather.Wind;
										MistIntensity = weather.Mist;
										//get the mist intensity and wind intensity overrides from our current location
										if (Player.Local.Surroundings.IsVisitingLocation) {
												MistIntensity += Player.Local.Surroundings.CurrentLocation.State.MistIntensityOverride;
												WindIntensity += Player.Local.Surroundings.CurrentLocation.State.WindIntensityOverride;
										}
										if (IsRaining && Player.Local.Surroundings.IsOutside) {
												if (GameWorld.Get.CurrentBiome.Climate == ClimateType.Arctic || GameWorld.Get.CurrentRegionData.g > 0) {
														AtmoParticles.ChangeAtmoSettingDensity("Snow", RainIntensity);
														AtmoParticles.ChangeAtmoSettingDensity("Rain", 0f);
												} else {
														AtmoParticles.ChangeAtmoSettingDensity("Rain", RainIntensity);
														AtmoParticles.ChangeAtmoSettingDensity("Snow", 0f);
												}
										} else {
												AtmoParticles.ChangeAtmoSettingDensity("Rain", 0f);
												AtmoParticles.ChangeAtmoSettingDensity("Snow", 0f);
										}
										AtmoParticles.ChangeAtmoSettingDensity("Mist", MistIntensity);
										AtmoParticles.PlayerPosition = Player.Local.Position;
								}

								mLightningThunder = Mathf.Lerp(mLightningThunder, 0f, 0.125f);

								mLightningCheck++;
								if (mLightningCheck > 500) {
										if (UnityEngine.Random.value < weather.LightningFrequency) {
												SpawnRandomLightning();
										}
										mLightningCheck = 0;
								}
						}
				}

				protected int mLightningCheck = 0;
				protected GameWorld.TerrainHeightSearch mTerrainHit;
		}

		[Serializable]
		public class Lerpable
		{
				public float	Min;
				public float	Max;
				public float	Base;
				public float	Current;
				public float	Target;
		}

		[Serializable]
		public class CloudSetting
		{
				public Lerpable CloudChange;
				public Lerpable CloudElevation;
				public Lerpable CloudDensity;
				public Lerpable CloudCoverage;
				public Lerpable CloudWindSpeed;
		}

		[Serializable]
		public class WaterColors
		{
				public Color FogColorMidnight;
				public Color FogColorDawn;
				public Color FogColorNoon;
				public Color FogColorDusk;
				public Color FogColorCloudy;
				public Color WavesColorMidnight;
				public Color WavesColorDawn;
				public Color WavesColorNoon;
				public Color WavesColorDusk;
				public Color WavesColorCloudy;

				public Color FogColor(float normalizedTimeOfDay)
				{
						return Colors.BlendFour(FogColorMidnight, FogColorDawn, FogColorNoon, FogColorDusk, normalizedTimeOfDay);
				}

				public Color WavesColor(float normalizedTimeOfDay)
				{
						return Colors.BlendFour(WavesColorMidnight, WavesColorDawn, WavesColorNoon, WavesColorDusk, normalizedTimeOfDay);
				}

				public Color FoamColor(float normalizedTimeOfDay)
				{
						return Colors.Desaturate(Colors.Brighten(Colors.BlendFour(WavesColorMidnight, WavesColorDawn, WavesColorNoon, WavesColorDusk, normalizedTimeOfDay)));
				}

				public Color CrestColor(float normalizedTimeOfDay)
				{
						return Color.Lerp(Colors.BlendFour(WavesColorMidnight, WavesColorDawn, WavesColorNoon, WavesColorDusk, normalizedTimeOfDay), Color.white, 0.5f);
				}
		}

		[Serializable]
		public class WaterPreset
		{
				public Color IlluminColor = Color.black;
				//set colors
				public Color SpecColor = new Color(0.5019608f,	0.5019608f,	0.5019608f,	0.04313726f);
				public Color SurfaceColor = new Color(0.2506126f,	0.3815854f,	0.4477612f,	0f);
				public Color DepthColor = new Color(0.1344398f,	0.2199293f,	0.2537314f,	0.5607843f);
				public Color EdgeColor = new Color(0.5458343f,	0.7381463f,	0.9029851f,	0.3686275f);
				public Color DistColor = new Color(0.3835486f,	0.4653959f,	0.5298507f,	0.6313726f);
				public Color FoamColor = new Color(0.633787f, 0.6716418f,	0.536311f,	0.6901961f);
				public Color FogColor = new Color(0.5615616f,	0.6760732f,	0.7489667f,	0.05f);
				public Color ReflColor = new Color(0.358209f, 0.358209f,	0.358209f,	1f);
				public Color ReflColor2 = new Color(0.7164179f,	0.7164179f,	0.7164179f,	0.2862745f);
				public Color ReflColor3 = new Color(0.56f, 0.674f, 0.749f, 0.52f);
				//set fog and Depth parameters
				public float ReflDist = 50f;
				public float ReflBlend = 0.005f;
				public float FogFar = 1200f;
				public float FogAlpha = 0.1f;
				public float DepthAmt = 0.025f;
				public float EdgeAmt = 0.39f;
				public float EdgeBlend = 1f;
				public float DistFar = 18.45f;
				public float DistBlend = 0.006f;
				public float FoamAmt = 0.08f;
				public float FoamBlend = 1.53f;
				//set norm and spec parameters
				public float Emissive = 2.5f;
				public float BumpStrength = 1.07f;
				public float SpecStrength = 1.5f;
				public float Wetness = 0.5f;
				//set refraction parameters
				public float RefrStrength = 4.0f;
				public float RefrSpeed = 0.18f;
				//set animation speed parameter
				public float AnimSpeed = 1.0f;
		}
}
using UnityEngine;
using System;
using System.Collections;
using Frontiers;

[Serializable]
public class BiomeSkySetting
{
		public bool UseDefault = true;
		public string SkyDataName = string.Empty;
}

[Serializable]
public class BiomeSkyData : Mod
{
		public BiomeSkyData() : base()
		{
		}

		public TOD_NightParameters NightSky;
		public TOD_DayParameters DaySky;
		public TOD_AtmosphereParameters Atmosphere;
		public TOD_LightParameters Light;
		public TOD_CloudParameters Clouds;
		public float FogStartDistance = 0f;
		public float FogEndDistance = 2048f;

		public static void Lerp(BiomeSkyData from, BiomeSkyData to, float amount, BiomeSkyData output)
		{
//		output.DaySky.AmbientIntensity = Mathf.Lerp (from.DaySky.AmbientIntensity, to.DaySky.AmbientIntensity, amount);
//		output.DaySky.SkyMultiplier = Mathf.Lerp (from.DaySky.SkyMultiplier, to.DaySky.SkyMultiplier, amount);
//		output.DaySky.SunLightColor = Color.Lerp (from.DaySky.SunLightColor, to.DaySky.SunLightColor, amount);
//		output.DaySky.CloudMultiplier = Mathf.Lerp (from.DaySky.CloudMultiplier, to.DaySky.CloudMultiplier, amount);
//		output.DaySky.ShadowStrength = Mathf.Lerp (from.DaySky.ShadowStrength, to.DaySky.ShadowStrength, amount);
//		output.DaySky.SunLightIntensity = Mathf.Lerp (from.DaySky.SunLightIntensity, to.DaySky.SunLightIntensity, amount);
//
//		output.Atmosphere.Brightness = Mathf.Lerp (from.Atmosphere.Brightness, to.Atmosphere.Brightness, amount);
//		output.Atmosphere.Contrast = Mathf.Lerp (from.Atmosphere.Contrast, to.Atmosphere.Contrast, amount);
//		output.Atmosphere.Directionality = Mathf.Lerp (from.Atmosphere.Directionality, to.Atmosphere.Directionality, amount);
//		output.Atmosphere.Fogginess = Mathf.Lerp (from.Atmosphere.Fogginess, to.Atmosphere.Fogginess, amount);
//		output.Atmosphere.Haziness = Mathf.Lerp (from.Atmosphere.Haziness, to.Atmosphere.Haziness, amount);
//		output.Atmosphere.MieMultiplier = Mathf.Lerp (from.Atmosphere.MieMultiplier, to.Atmosphere.MieMultiplier, amount);
//		output.Atmosphere.ScatteringColor = Color.Lerp (from.Atmosphere.ScatteringColor, to.Atmosphere.ScatteringColor, amount);
//		output.Atmosphere.RayleighMultiplier = Mathf.Lerp (from.Atmosphere.RayleighMultiplier, to.Atmosphere.RayleighMultiplier, amount);
//
//		output.Light.CloudColoring = Mathf.Lerp (from.Light.CloudColoring, to.Light.CloudColoring, amount);
//		output.Light.Falloff = Mathf.Lerp (from.Light.Falloff, to.Light.Falloff, amount);
//		output.Light.SkyColoring = Mathf.Lerp (from.Light.SkyColoring, to.Light.SkyColoring, amount);
//
//		output.Clouds.Brightness = Mathf.Lerp (from.Clouds.Brightness, to.Clouds.Brightness, amount);
//		output.Clouds.Density = Mathf.Lerp (from.Clouds.Density, to.Clouds.Density, amount);
//		output.Clouds.Sharpness = Mathf.Lerp (from.Clouds.Sharpness, to.Clouds.Sharpness, amount);
//		output.Clouds.Scale1 = Mathf.Lerp (from.Clouds.Scale1, to.Clouds.Scale1, amount);
//		output.Clouds.Scale2 = Mathf.Lerp (from.Clouds.Scale2, to.Clouds.Scale2, amount);
//
//		output.FogStartDistance = Mathf.Lerp (from.FogStartDistance, to.FogStartDistance, amount);
//		output.FogEndDistance = Mathf.Lerp (from.FogEndDistance, to.FogEndDistance, amount);
		}
}

[Serializable]
public class BiomeHDRSettings
{
		public bool UseDefaultDay = true;
		public bool UseDefaultNight = true;
		public bool UseDefaultUnderground = true;
		public bool UseDefaultInterior = true;
		public float MiddleGreyDayOpen = 0.425f;
		public float MiddleGreyDayForest = 0.2f;
		public float BrightValDayOpen = 1.15f;
		public float BrightValDayForest = 1.0f;
		public float MiddleGreyNightOpen = 0.2f;
		public float MiddleGreyNightForest = 0.1f;
		public float BrightValNightOpen = 1.35f;
		public float BrightValNightForest = 1.0f;
		public float MiddleGreyUnderground = 0.25f;
		public float BrightValUnderground = 1.0f;
		public float InteriorMultiplier = 1.0f;
}

[Serializable]
public class BiomeStatusTemps
{
		public bool UseDefaults = true;
		public TemperatureRange StatusTempQuarterMorning = TemperatureRange.C_Warm;
		// 6am - 12pm (6 hours)
		public TemperatureRange StatusTempQuarterAfternoon = TemperatureRange.D_Hot;
		//12pm - 6pm  (6 hours)
		public TemperatureRange StatusTempQuarterEvening = TemperatureRange.C_Warm;
		// 6pm - 12am (6 hours)
		public TemperatureRange StatusTempQuarterNight = TemperatureRange.B_Cold;
		//12am - 6am  (6 hours)
		public TemperatureRange StatusTempsAverage {
				get {
						return (TemperatureRange) (((int)StatusTempQuarterMorning + (int)StatusTempQuarterAfternoon + (int)StatusTempQuarterEvening + (int)StatusTempQuarterNight) / 4);
				}
		}
}
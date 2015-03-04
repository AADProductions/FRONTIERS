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
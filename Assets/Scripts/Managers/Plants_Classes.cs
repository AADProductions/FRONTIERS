using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers
{
	[Serializable]
	public class Plant : Mod {
		//naming (inherits Name from Mod)
		public string CommonName;
		public string NickName;
		public string ScientificName;
		//location
		public bool AboveGround = true;
		public ElevationType Elevation = ElevationType.Medium;
		[FrontiersBitMaskAttribute ("Climate")]
		public int ClimateFlags;
		public ClimateType Climate = ClimateType.Temperate;
		//appearance
		public WIRarity Rarity = WIRarity.Uncommon;
		public bool HasThorns = false;
		public bool HasFlowers = true;
		public int ThornVariation = 0;
		public int ThornTexture = 0;
		public int BodyType = 0;
		public int BodyVariation = 0;
		public int BodyTexture = 0;
		public int FlowerType = 0;
		public int FlowerVariation = 0;
		public int FlowerTexture = 0;
		public PlantRootType RootType = PlantRootType.TypicalBranched;
		public int RootVariation = 0;
		public int RootTexture = 0;
		public int NumTimesEncountered = 0;
		//root props (not affected by season)
		public PlantRootSize RootSize = PlantRootSize.Medium;
		public SColor RootBaseColor = Color.white;
		public float RootHueShift = 0f;
		//season settings
		public List <PlantSeasonalSettings> SeasonalSettings = new List <PlantSeasonalSettings> ();
		public FoodStuffProps RawProps = new FoodStuffProps ( );
		public FoodStuffProps CookedProps = new FoodStuffProps ( );
		//times when the player has encountered the plant
		public TimeOfYear EncounteredTimesOfYear = TimeOfYear.None;
		[XmlIgnore]//this is calculated on startup
		public TimeOfYear Seasonality = TimeOfYear.None;
		[XmlIgnore]
		public PlantSeasonalSettings CurrentSeason = null;
		public bool Revealed = false;
		public bool RawPropsRevealed = false;
		public bool CookedPropsRevealed = false;

		public static float FlowerSizeToFloat (PlantFlowerSize size)
		{
			switch (size) {
			case PlantFlowerSize.Tiny:
				return 0.1f;

			case PlantFlowerSize.Small:
				return 0.25f;

			case PlantFlowerSize.Medium:
			default:
				return 0.5f;

			case PlantFlowerSize.Large:
				return 1.0f;

			case PlantFlowerSize.Giant:
				return 1.5f;
			}
		}

		public static int ElevationTypeToInt (ElevationType elevation)
		{
			switch (elevation) {
			case ElevationType.High:
			default:
				return Globals.ElevationHigh;

			case ElevationType.Medium:
				return Globals.ElevationMedium;

			case ElevationType.Low:
				return Globals.ElevationLow;
			}
		}
	}

	[Serializable]
	public class PlantState : Mod {
		public bool HasBeenResearched = false;
		public int NumTimesPicked = 0;
		public int NumTimesSpawned = 0;
		public int MaxConcurrentSpawns = 100;
	}

	[Serializable]
	public class PlantMesh : Mod {

	}

	[Serializable]
	public class PlantSeasonalSettings
	{
		public bool Revealed = false;
		[BitMaskAttribute (typeof (TimeOfYear))]
		public TimeOfYear Seasonality = TimeOfYear.SeasonSummer;
		public bool Flowers = false;
		public PlantBodyHeight BodyHeight = PlantBodyHeight.Medium;
		public PlantFlowerSize FlowerSize = PlantFlowerSize.Medium;
		public SColor BodyUnderColor = Color.white;
		public SColor FlowerUnderCOlor = Color.white;
		public float FlowerHueShift = 0f;
		public float BodyHueShift = 0f;
		public float FlowerDensity = 0.5f;
	}
}
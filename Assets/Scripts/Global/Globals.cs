
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class Globals
{
		public static float PlantBaseCurrencyValueArctic = 12.5f;
		public static float PlantBaseCurrencyValueTemperate = 5f;
		public static float PlantBaseCurrencyValueTropical = 10f;
		public static float PlantBaseCurrencyValueWetland = 7.5f;
		public static float PlantBaseCurrencyValueDesert = 15f;
		public static float PlantUndergroundMultiplier = 2.5f;
		public static float BaseValueWeaponDamagePerHit = 5f;
		public static float BaseValueWeaponForcePerHit = 2.5f;
		public static float BaseValueWeaponDelayInterval = 5f;
		public static float BaseValueWeaponStrengthDrain = 20f;
		public static float BaseValueWeaponProjectileMultiplier = 2.5f;
		public static float BaseValueWearable = 5f;
		public static float BaseValueLuminite = 10f;

		public static string PlayerManagerGameObjectName = "=PLAYER=";
		public static string GroupsManagerGameObjectName = "=GROUPS=";
		public static string ManagersGameObjectName = "=MANAGER=";
		public static string LoadingGameObjectName = "=LOADING=";
		public static string GameManagerGameObjectName = "=GAME=";

		public static int NumCachedSplinePositionsPerMeter = 5;

		#region difficulty
		[EditableDifficultySetting(0f, 10000000f, "Base price in grains for a book in the guild library")]
		public static float GuildLibraryBasePrice = 10000;
		[EditableDifficultySetting(0f, 10000000f, "Minimum price in grains for a book in the guild library")]
		public static float GuildLibraryMinimumPrice = 25;
		[EditableDifficultySetting(0, 100000, "Minimum delivery time in hours for books in the Guild Library. Used along with the catalogue's base order value.")]
		public static int GuildLibraryMinimumDeliveryTimeInHours = 2;
		[EditableDifficultySetting(0, 100000, "Base delivery time in hours for books in the Guild Library. Used along with the catalogue's base order value.")]
		public static int GuildLibraryBaseDeliveryTimeInHours = 5;
		[EditableDifficultySetting(0f, 1f, "Movement speed multiplier when player is in water")]
		public static float DefaultWaterAccelerationPenalty = 0.15f;
		[EditableDifficultySetting(0f, 1f, "Jump height multiplier when player is in water")]
		public static float DefaultWaterJumpPenalty = 0.15f;
		[EditableDifficultySetting(0f, 1f, "Amount of force rivers exert on the player")]
		public static float RiverFlowForceMultiplier = 0.1f;
		[EditableDifficultySetting(0f, 1f, "How quickly resting on a bench or chair restores player strength")]
		public static float RestStrengthRestoreSpeed = 0.05f;
		[EditableDifficultySetting(0.1f, 5f, "The timescale of status keepers relative to the global timescale")]
		public static double StatusKeeperTimecale = 1.0f;
		[EditableDifficultySetting(-5f, 5f, "Multiplier applied to negative changes in status keepers (like losing health, losing strength etc.)")]
		public static float StatusKeeperNegativeChangeMultiplier = 1f;
		[EditableDifficultySetting(-5f, 5f, "Multiplier applied to positive changes in status keepers (like gaining health, gaining strength etc.)")]
		public static float StatusKeeperPositiveChangeMultiplier = 1f;
		[EditableDifficultySetting(-5f, 5f, "Multiplier applied to negative overflows in status keepers (like high thirst reducing health)")]
		public static float StatusKeeperNegativeFlowMultiplier = 1f;
		[EditableDifficultySetting(-5f, 5f, "Multiplier applied to positive overflows in status keepers (like low thirst increasing strength)")]
		public static float StatusKeeperPositiveFlowMultiplier = 1f;
		[EditableDifficultySetting]
		public static double StatusKeeperNeutralChangeMultiplier = 1f;
		[EditableDifficultySetting(0f, 5f, "How quickly the player's strength is drained per meter traveled in fast-travel mode")]
		public static float FastTravelStrengthReducedPerMeterTraveled = 0.001f;
		[EditableDifficultySetting(0, 100, "These values are mostly used during conversations")]
		public static int ReputationChangeTiny = 1;
		[EditableDifficultySetting(0, 100, "These values are mostly used during conversations")]
		public static int ReputationChangeSmall = 2;
		[EditableDifficultySetting(0, 100, "These values are mostly used during conversations")]
		public static int ReputationChangeMedium = 6;
		[EditableDifficultySetting(0, 100, "These values are mostly used during conversations")]
		public static int ReputationChangeLarge = 10;
		[EditableDifficultySetting(0, 100, "These values are mostly used during conversations")]
		public static int ReputationChangeHuge = 20;
		[EditableDifficultySetting(0, 100, "The largest amount of reputation you can lose for any single action. Reserved for killing an innocent person.")]
		public static int ReputationChangeMurderer = 80;
		[EditableDifficultySetting(1f, 1000f, "The lowest amount of reputation you can have. Clamped at 1 to prevent some mission conversations from being inaccessible.")]
		public static float MinReputation = 1;
		[EditableDifficultySetting(1f, 1000f, "The max amount of reputation you can have. Values over 100 may produce odd behavior.")]
		public static float MaxReputation = 100;
		[EditableDifficultySetting(0f, 100f, "How much reputation you lose per base currency of a stolen item's worth")]
		public static float BaseCurrencyToReputationMultiplier = 0.1f;
		[EditableDifficultySetting(1f, 500f, "The maximum distance at which a character or creature can hear an audible item")]
		public static float MaxAudibleRange = 50f;
		[EditableDifficultySetting(1f, 500f, "The maximum distance at which a character or creature can spot a visible item")]
		public static float MaxAwarenessDistance = 50f;
		[EditableDifficultySetting(10f, 360f, "The maximum field of view for characters & creatures")]
		public static float MaxFieldOfView = 120f;
		[EditableDifficultySetting(0.1f, 100f, "Time in hours before a luminite crystal will re-grow after being mined")]
		public static float LuminiteRegrowTime = 5f;
		[EditableDifficultySetting(EditableDifficultySettingAttribute.SettingType.Bool, true, "Whether darkrot will spawn anywhere or just in forests (defined as g channel in chunk terrain type map)")]
		public static bool DarkrotSpawnsOnlyInForests = true;
		[EditableDifficultySetting(0.1f, 100f, "Duration of each darkrot 'step' towards the player")]
		public static float DarkrotMoveInterval = 2.0f;
		[EditableDifficultySetting(0.1f, 100f, "Interval between each darkrot 'step' toward the player")]
		public static float DarkrotWaitInterval = 2.0f;
		[EditableDifficultySetting(0.1f, 100f, "Duration of darkrot's 'warmpup' phase during which it can't attack the player")]
		public static float DarkrotWarmupTime = 2f;
		[EditableDifficultySetting(0.1f, 1000f, "Minimum amount of absorbable darkrot is contained in each spawned darkrot node")]
		public static float DarkrotMinAmount = 10;
		[EditableDifficultySetting(0.1f, 1000f, "Maximum amount of absorbable darkrot is contained in each spawned darkrot node")]
		public static float DarkrotMaxAmount = 100;
		[EditableDifficultySetting(0.1f, 1000f, "Average amount of absorbable darkrot is contained in each spawned darkrot node")]
		public static float DarkrotAvgAmount = 25;
		[EditableDifficultySetting(0f, 1f, "How likely darkrot is to emit a sound while taking a step towards the player")]
		public static float DarkrotEmitSoundProbability = 0.1f;
		[EditableDifficultySetting]
		public static float DarkrotUpdateInterval = 0.125f;
		[EditableDifficultySetting(0f, 100f, "How much light is required to disperse a darkrot node")]
		public static float DarkrotMaxLightAndHeatExposure = 10f;
		[EditableDifficultySetting(0f, 100f, "Average spawn distance from the player")]
		public static float DarkrotSpawnDistance = 10f;
		[EditableDifficultySetting(0f, 100f, "Number of seconds needed for darkrot to dissipate after being disperesed")]
		public static float DarkrotDissipationTime = 2f;
		[EditableDifficultySetting(0f, 1f, "Overall probability of darkrot spawning")]
		public static float DarkrotBaseSpawnProbability = 0.01f;
		[EditableDifficultySetting]
		public static float DarkrotPulseInterval = 2.0f;
		[EditableDifficultySetting(0, 1000, "Maximum number of active darkrot nodes")]
		public static int DarkrotMaxNodes = 100;
		[EditableDifficultySetting(0f, 100f, "Maximum speed at which darkrot moved towards player")]
		public static float DarkrotMaxSpeed = 1f;
		[EditableDifficultySetting(0f, 1000f, "Time in seconds before a picked plant will re-grow a new plant")]
		public static float PlantAutoRegrowInterval = 5;
		[EditableDifficultySetting]
		public static float PlantAutoReplantInterval = 100;
		[EditableDifficultySetting(0f, 10000f, "Base price in grains for the right to sleep at an inn")]
		public static float InnBasePricePerNight = 12f;
		[EditableDifficultySetting(0f, 1f, "Maximum effect of reputation & skill modifiers on barter prices")]
		public static float BarterMaximumPriceModifier = 0.25f;
		[EditableDifficultySetting(0, 100000, "Average net worth of a poor NPC")]
		public static int WealthLevelPoorBaseCurrency = 100;
		[EditableDifficultySetting(0, 100000, "Average net worth of a middle-class NPC")]
		public static int WealthLevelMiddleClassBaseCurrency = 1000;
		[EditableDifficultySetting(0, 100000, "Average net worth of a wealthy NPC")]
		public static int WealthLevelWealthyBaseCurrency = 10000;
		[EditableDifficultySetting(0, 100000, "Average net worth of a royal NPC")]
		public static int WealthLevelAristocracyBaseCurrency = 100000;
		[EditableDifficultySetting(0f, 100000f, "Average number of grains (bronze) in a purse")]
		public static float AveragePurseBronzeCurrency = 1000;
		[EditableDifficultySetting(0f, 100000f, "Average number of quarters (silver) in a purse")]
		public static float AveragePurseSilverCurrency = 100;
		[EditableDifficultySetting(0f, 100000f, "Average number of drams (gold) in a purse")]
		public static float AveragePurseGoldCurrency = 10;
		[EditableDifficultySetting(0f, 100000f, "Average number of marks (lumen) in a purse")]
		public static float AveragePurseLumenCurrency = 2;
		[EditableDifficultySetting(0, 100, "Number of fish that a trap catches, multiplied by skill roll on set")]
		public static int TrappingNumWaterGoodiesPerSkillRoll = 10;
		[EditableDifficultySetting(0f, 1000f, "Number of real-time seconds before live traps check to see if they've been triggered")]
		public static float TrappingMinimumRTCheckInterval = 30f;
		[EditableDifficultySetting(0f, 1f, "Overall probability of an unwatched trap being triggered")]
		public static float TrappingOddsTimeMultiplier = 0.001f;
		[EditableDifficultySetting(0f, 1000f, "Required distance between player and trap before the trap will spawn a corpse")]
		public static float TrappingMinimumCorpseSpawnDistance = 50f;
		[EditableDifficultySetting(0f, 100f, "Proability modifier for distance from optimal trap position")]
		public static float TrappingOddsDistanceMultiplier = 2.0f;
		[EditableDifficultySetting(0f, 10f, "Damage multiplier for penalties")]
		public static float DamageMaterialPenaltyMultiplier = 0.5f;
		[EditableDifficultySetting(0f, 10f, "Damage multiplier for forces")]
		public static float DamageSumForceMultiplier = 0.35f;
		[EditableDifficultySetting(0f, 10f, "Damage multiplier for fall damage on the 'Forgiving' setting")]
		public static float DamageFallDamageForgivingMultiplier = 1.25f;
		[EditableDifficultySetting(0f, 10f, "Damage multiplier for fall damage on the 'Realistic' setting")]
		public static float DamageFallDamageRealisticMultiplier = 5f;
		[EditableDifficultySetting(0f, 500f, "Maximum possible fall damage from any height on the 'Forgiving' setting")]
		public static float DamageFallDamageForgivingImpactThreshold = 5f;
		[EditableDifficultySetting(0f, 500f, "Maximum possible fall damage from any height on the 'Realistic' setting")]
		public static float DamageFallDamageRealisticImpactThreshold = 2f;
		[EditableDifficultySetting(0f, 100f, "Minimum impact required before fall damage will take effect on the 'Forgiving' setting")]
		public static float DamageFallDamageForgivingImpactDeathThreshold = 200f;
		[EditableDifficultySetting(0f, 100f, "Minimum impact required before fall damage will take effect on the 'Realistic' setting")]
		public static float DamageFallDamageRealisticImpactDeathThreshold = 50f;
		[EditableDifficultySetting(0f, 100f, "Minimum impact required before fall damage will cause a broken bone on the 'Forgiving' setting")]
		public static float DamageFallDamageForgivingBrokenBoneThreshold = 5f;
		[EditableDifficultySetting(0f, 100f, "Minimum impact required before fall damage will cause a broken bone on the 'Realistic' setting")]
		public static float DamageFallDamageRealisticBrokenBoneThreshold = 2f;
		[EditableDifficultySetting(0f, 100f, "Damage multiplier for material bonuses")]
		public static float DamageMaterialBonusMultiplier = 2.0f;
		[EditableDifficultySetting(0f, 1000f, "Damage caused by rolling stones during a rockslide")]
		public static float DamageOnRockslideHit = 25f;
		[EditableDifficultySetting(0f, 1000f, "Speed of Leviathan when tracking a target")]
		public static float LeviathanMoveSpeed = 0.25f;
		[EditableDifficultySetting(0f, 100f, "Minimum distance from target before a Leviathan can attack")]
		public static float LeviathanMinimumAttackDistance = 1.0f;
		[EditableDifficultySetting(0f, 1000f, "Leviathan spawn distance from target when initially summoned")]
		public static float LeviathanStartDistance = 50.0f;
		[EditableDifficultySetting(0f, 100f, "Minimum seconds a Leviathan must spend stalking before an attack is possible")]
		public static float LeviathanRTMinimumStalkInterval = 1.0f;
		[EditableDifficultySetting(0f, 1000f, "Seconds before a Leviathan will lose interest in tracking a lost target")]
		public static float LeviathanRTLoseInterestInterval = 5.0f;
		[EditableDifficultySetting(0f, 100f, "Multiplier for a fire's radius - anything inside this radius will burn")]
		public static float FireBurnDistance = 0.5f;
		[EditableDifficultySetting(0f, 100f, "Multiplier for a fire's radius - anything inside this radius will cook")]
		public static float FireCookDistance = 2.0f;
		[EditableDifficultySetting(0f, 100f, "Multiplier for a fire's radius - anything inside this radius will be warmed")]
		public static float FireWarmDistance = 4.0f;
		[EditableDifficultySetting(0f, 100f, "Multiplier for a fire's radius - anything that is afraid of fires will be scared at this radius")]
		public static float FireScareDistance = 8.0f;
		[EditableDifficultySetting(0f, 1000f, "Number of in-game hours of sleep required to get the Well Rested condition")]
		public static float WellRestedHours = 8.0f;
		[EditableDifficultySetting(0f, 1f, "Value of stolen goods when bartering")]
		public static float StolenGoodsValueMultiplier = 0.0f;
		[EditableDifficultySetting(0f, 1f, "Skill rolls below this value count as a critical failure")]
		public static float SkillCriticalFailure = 0.975f;
		[EditableDifficultySetting(0f, 1f, "Skill rolls above this value count as a critical success")]
		public static float SkillCriticalSuccess = 0.015f;
		[EditableDifficultySetting(0f, 1f, "The player automatically has at least this much proficiency in any acquired skill")]
		public static float SkillFailsafeMasteryLevel = 0.05f;
		[EditableDifficultySetting(0f, 100f, "Distance of player's encounter trigger. This affects creatures, characters and obstacles.")]
		public static float PlayerEncounterRadius = 7.0f;
		[EditableDifficultySetting]
		public static float PlayerControllerStepOffsetDefault = 0.3f;
		[EditableDifficultySetting(0f, 180f, "The steepest slope in degrees that a character can walk without jumping")]
		public static float PlayerControllerSlopeLimitDefault = 55f;
		[EditableDifficultySetting(1, 10000, "Maximum number of 'huge' sized items that can be placed in a stack")]
		public static int MaxHugeItemsPerStack = 1;
		[EditableDifficultySetting(1, 10000, "Maximum number of 'large' sized items that can be placed in a stack")]
		public static int MaxLargeItemsPerStack = 1;
		[EditableDifficultySetting(1, 10000, "Maximum number of 'medium' sized items that can be placed in a stack")]
		public static int MaxMediumItemsPerStack = 10;
		[EditableDifficultySetting(1, 10000, "Maximum number of 'small' sized items that can be placed in a stack")]
		public static int MaxSmallItemsPerStack = 100;
		[EditableDifficultySetting(1, 10000, "Maximum number of 'tiny' sized items that can be placed in a stack")]
		public static int MaxTinyItemsPerStack = 1000;
		[EditableDifficultySetting(0f, 100000f, "Average number of meters per hour during fast travel, before modifiers")]
		public static float PlayerAverageMetersPerHour = 500.0f;
		[EditableDifficultySetting(0f, 1000f, "If the player is at least this far away from an active path they are straying from the path")]
		public static float PathStrayDistanceInMeters = 6.0f;
		[EditableDifficultySetting(0f, 100f, "Number of seconds the player must be straying before they are warned that they are straying")]
		public static float PathStrayMinTimeInSeconds = 2.5f;
		[EditableDifficultySetting(0f, 100f, "Number of seconds the player must be straying before the are no longer following the active path")]
		public static float PathStrayMaxTimeInSeconds = 15.0f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for fast travel speed on an easy-level path")]
		public static float PathEasyMetersPerHour = PlayerAverageMetersPerHour;
		[EditableDifficultySetting(0f, 1f, "Multiplier for fast travel speed on an average-level path")]
		public static float PathModerateMetersPerHour = PlayerAverageMetersPerHour * 0.75f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for fast travel speed on an moderate-level path")]
		public static float PathDifficultMetersPerHour = PlayerAverageMetersPerHour * 0.65f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for fast travel speed on an difficult-level path")]
		public static float PathDeadlyMetersPerHour = PlayerAverageMetersPerHour * 0.55f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for fast travel speed on an impassible-level path")]
		public static float PathImpassibleMetersPerHour = PlayerAverageMetersPerHour * 0.35f;
		[EditableDifficultySetting(0f, 1000f, "Rate at which fires burn through their fuel")]
		public static double FireBurnFuelRate = 5f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for how much total currency a healer takes upon reviving a player")]
		public static float HouseOfHealingRevivalCost = 0.5f;
		[EditableDifficultySetting(0f, 1f, "Multiplier for how much total currency a healer takes for healing the player")]
		public static float HouseOfHealingHealCost = 0.05f;
		[EditableDifficultySetting(0, 100000, "Base cost in grains per negative symptom healed")]
		public static int HouseOfHealingCostPerNegativeSymptom = 150;
		[EditableDifficultySetting(0, 1000, "Number of hours a character's welcome will last before you must knock again")]
		public static int NpcRequiredKnockHours = 12;
		[EditableDifficultySetting(0, 100000, "Base cost for a structure in marks")]
		public static int StructureBaseValueInMarks = 5;
		[EditableDifficultySetting(0f, 100f, "Used to calculate building value - number of additional marks per byte of structure template's file size")]
		public static float StructureValueTemplateMultiplier = 0.01f;
		[EditableDifficultySetting(0f, 1000f, "Total length of a rockslide in seconds")]
		public static float RockslideInterval = 10f;
		[EditableDifficultySetting(0f, 100000f, "Total number of rocks spawned during a rockslide")]
		public static float RockslideNumRocksToSpawn = 10;
		[EditableDifficultySetting(0.1f, 20f, "Multiplier for skill radius of spyglass. Used for placing markers on terrain.")]
		public static float RaycastSpyGlassDistanceMultiplier = 3f;
		[EditableDifficultySetting(2f, 10f, "Multiplier for cook time that it takes to burn food items.")]
		public static float EdibleBurnTimeMultiplier = 4f;
		public static float BarBasePricePerRoundOfDrinks = 20f;
		public static float BarBasePricePerDrink = 1f;
		public static int BaseValueFoodStuff = 75;
		public static float BaseValueCraftingBonus = 1.25f;

		public static float DamageBodyPartEyeMultiplier = 10f;
		public static float DamageBodyPartHeadMultiplier = 1f;
		public static float DamageBodyPartTorsoMultiplier = 0.5f;
		public static float DamageBodyPartLimbMultiplier = 0.25f;
		public static float FallDamageImpactMultiplier = 100f;
		#endregion

		#region profile

		[ProfileSetting]
		public const float DoubleTapInterval = 0.25f;
		[ProfileSetting]
		public static float MouseScrollSensitivity = 0.075f;
		[ProfileSetting]
		public static float WorldItemVisibleDistanceMultiplier = 3f;
		[ProfileSetting]
		public static float PlayerMinimumActiveStateSortDistance = 5.0f;
		[ProfileSetting]
		public static float PlayerMinimumActiveStateSortDistanceTraveling = 25f;
		[ProfileSetting]
		public static float PlayerMovementSpeedToActiveStateCheck = 5f;
		[ProfileSetting]
		public static float MouseSensitivityFPSMin = 1.0f;
		[ProfileSetting]
		public static float MouseSensitivityFPSMax = 10.0f;
		[ProfileSetting]
		public static int MaxSpawnedPathMarkers = 25;
		[ProfileSetting]
		public static int MaxSpawnedPlants = 25;
		[ProfileSetting]
		public static int MaxSpawnedChunks = 16;
		[ProfileSetting]
		public static float ClippingDistanceNear = 0.175f;
		[ProfileSetting]
		public static float ClippingDistanceFar = 1024f;
		//2250f;

		#endregion

		#region world

		[WorldSetting]
		public static string DefaultFontName = "PrintingPress40";
		[WorldSetting]
		public static int MinCharacterAge = 17;
		[WorldSetting]
		public static int MaxCharacterAge = 27;
		[WorldSetting]
		public static float DefaultInGameMinutesPerRealTimeSecond = 60.0f;
		[WorldSetting]
		public static float MinInGameMinutesPerRealtimeSecond = 1f;
		[WorldSetting]
		public static float MaxInGameMinutesPerRealtimeSecond = 120f;
		[WorldSetting]
		public static string DefaultDifficultyName = "Normal";
		[WorldSetting]
		public static float FireIgnitionProbability = 0.1f;
		[WorldSetting]
		public static float FireIgnitionDistance = 0.75f;
		[WorldSetting]
		public const float InGameUnitsToMeters = 1.5f;
		[WorldSetting]
		public const float WorldMapUnitsToMeters = 400000.0f;
		[WorldSetting]
		public static float WeightInKgWeightless = 0.1f;
		[WorldSetting]
		public static float WeightInKgLight = 1.0f;
		[WorldSetting]
		public static float WeightInKgMedium = 10.0f;
		[WorldSetting]
		public static float WeightInKgHeavy = 100.0f;
		[WorldSetting]
		public static float WeightInKgUnliftable = 1000.0f;
		[WorldSetting]
		public static float CityMinimumRadius = 50f;
		[WorldSetting]
		public static float TownMinimumRadius = 25f;
		[WorldSetting]
		public static int DayHourStart = 5;
		[WorldSetting]
		public static int DayHourEnd = 19;
		[WorldSetting]
		public static float WaveSpeed = 0.25f;
		[WorldSetting]
		public static int ElevationLow = 50;
		[WorldSetting]
		public static int ElevationMedium = 150;
		[WorldSetting]
		public static int ElevationHigh = 450;
		[WorldSetting]
		public static float MaxRiverAudioDistance = 0.25f;
		[WorldSetting]
		public static int DefaultBookWealthValue = 1;
		[WorldSetting]
		public static float DefaultCharacterHeight = 2.0f;
		[WorldSetting]
		public static float DefaultCharacterGroundedHeight = 0.5f;
		[WorldSetting]
		public static float DefaultCharacterFallAcceleration = 0.05f;
		[WorldSetting]
		public static float MaxCharacterFallAcceleration = 5f;
		[WorldSetting]
		public static float PathOriginTriggerRadius = 10f;
		[WorldSetting]
		public static float WorldMapLocationRadiusMultipiler = 1f;
		[WorldSetting]
		public static float LightRangePadding = 5f;
		[WorldSetting]
		public static float LightExposureMultiplier = 0.1f;
		[WorldSetting]
		public static float HeatExposureMultiplier = 0.1f;
		[WorldSetting]
		public static float WorldLightLerpSpeed = 0.25f;
		[WorldSetting]
		public static float LightOutOfRangeBaseDistance = 150f;
		[WorldSetting]
		public static int MaxWorldLights = 50;
		[WorldSetting]
		public static float SpawnerRTYieldInterval = 1.0f;
		[WorldSetting]
		public static bool StructuresLoadEmptyMaterial = false;
		[WorldSetting]
		public static int MaxVisibleWorldItems = 100;
		[WorldSetting]
		public static int MaxActiveWorldItems = 50;
		[WorldSetting]
		public static float ChunkMaximumYBounds = 4096f;
		[WorldSetting]
		public static int WIRarityCommonProbability = 10;
		[WorldSetting]
		public static int WIRarityUncommonProbability = 5;
		[WorldSetting]
		public static int WIRarityRareProbability = 1;
		[WorldSetting]
		public static int WICategoryRandomTableProbability = 3;
		[WorldSetting]
		public const string WorldItemGenericPrefabName = "WorldItemGeneric";
		[WorldSetting]
		public const string WorldItemGenericRegionName = "Terrainia";
		[WorldSetting]
		public static float LeviathanMaxAudibleRange = 75f;
		[WorldSetting]
		public static int MaxPathMarkerAttachedPaths = 4;
		[WorldSetting]
		public static int MaxSplineNodesPerPath = 250;
		[WorldSetting]
		public static float FastTravelInertia = 5.0f;
		[WorldSetting]
		public static int MaxSpawnedPilgrims = 3;
		[WorldSetting]
		public static int BaseValueBronze = 1;
		[WorldSetting]
		public static int BaseValueSilver = BaseValueBronze * 4;
		[WorldSetting]
		public static int BaseValueGold = BaseValueSilver * 4;
		[WorldSetting]
		public static int BaseValueLumen = BaseValueGold * 4;
		[WorldSetting]
		public static int BaseValueWarlock = BaseValueGold;
		[WorldSetting]
		public static float PlayerColliderRadius = 50.0f;
		[WorldSetting]
		public static float ChunkUnloadedDistance = 2500f;
		[WorldSetting]
		public static float ChunkDistantDistance = 2000f;
		[WorldSetting]
		public static float ChunkAdjascentDistance = 1500f;
		[WorldSetting]
		public static float ChunkImmediateDistance = 1000f;
		[WorldSetting]
		public static int NumActiveTreeColliders = 50;
		[WorldSetting]
		public static float PathfindingMaxNearestNodeDistance = 50.0f;
		[WorldSetting]
		public static int PathfindingMaxNeighbors = 8;
		[WorldSetting]
		public static int PathfindingMaxClimbAxis = 1;
		[WorldSetting]
		public static float PathfindingMaxSlope = 70.0f;
		[WorldSetting]
		public static float PathfindingMaxClimb = 0.5f;
		[WorldSetting]
		public static float PlayerControllerHeightDefault = 1.65f;
		[WorldSetting]
		public static float PlayerControllerRadiusDefault = 0.45f;
		[WorldSetting]
		public static float PlayerControllerYCenterDefault = 0.825f;
		[WorldSetting]
		public static float PlayerControllerSkinWidthDefault = 0.1f;
		[WorldSetting]
		public static float PathSlopeDifficultyEasy = 0.05f;
		[WorldSetting]
		public static float PathSlopeDifficultyModerate = 0.1f;
		[WorldSetting]
		public static float PathSlopeDifficultyDifficult = 0.3f;
		[WorldSetting]
		public static float PathSlopeDifficultyDeadly = 0.5f;
		[WorldSetting]
		public static float PathSlopeDifficultyImpassible = 0.7f;
		[WorldSetting]
		public static int LargeCharacterBodyTextureResolution = 1024;
		[WorldSetting]
		public static int MediumCharacterBodyTextureResolution = 768;
		[WorldSetting]
		public static int SmallCharacterBodyTextureResolution = 512;
		[WorldSetting]
		public static int CharacterFaceTextureResolution = 256;
		[WorldSetting]
		public static int GroundCombinedNormalResolution = 1024;
		[WorldSetting]
		public static int GroundTextureResolution = 1024;
		[WorldSetting]
		public static int GrassTextureResolution = 128;
		[WorldSetting]
		public static float TimeScaleTravelMax = 1.5f;
		[WorldSetting]
		public static float TimeScaleTravelMin = 0.005f;
		[WorldSetting]
		public static string HouseOfHealingInteriorConversation = "Healer-Enc-Anytime-00";
		[WorldSetting]
		public static string HouseOfHealingExteriorConversation = "Healer-Enc-Anytime-01";
		[WorldSetting]
		public static int MaxPathMarkersInPath = 1024;
		#endregion

		#region visual

		[VisualSetting]
		public static float FishingHoldNumFishPerRadius = 2.5f;
		[VisualSetting]
		public static float TerrainWindIntensity = 0.5f;
		[VisualSetting]
		public static float MaxAmbientLightBoost = 0.25f;
		[VisualSetting]
		public static float SceneryLODRatioPrimary = 0.5f;
		[VisualSetting]
		public static float SceneryLODRatioSecondary = 0.25f;
		[VisualSetting]
		public static float SceneryLODRatioOff = 0.25f;
		[VisualSetting]
		public static float LuminiteEmissionToLightBrightnessMultiplier	= 1.0f;
		[VisualSetting]
		public static float SkyLightIntensityMultiplier = 5.0f;
		[VisualSetting]
		public static float SunLightIntensityMultiplier = 0.60f;
		[VisualSetting]
		public static float AmbientLightTransitionTime = 8.0f;
		[VisualSetting]
		public static float StructureInteriorLODRatio = 0.85f;
		[VisualSetting]
		public static float StructureExteriorLODRatio = 0.20f;
		[VisualSetting]
		public static float DefaultFogDistance = 800f;
		[VisualSetting]
		public static float AmbientLightIntensityDay = 1.0f;
		[VisualSetting]
		public static float AmbientLightIntensityNight = 0.5f;
		[VisualSetting]
		public static float AmbientLightIntensityInteriorDay = 0.5f;
		[VisualSetting]
		public static float AmbientLightIntensityInteriorNight = 0.5f;
		[VisualSetting]
		public static float AmbientLightIntensityForest = 0.15f;
		[VisualSetting]
		public static float AmbientLightIntensityUnderground = 0.05f;
		[VisualSetting]
		public static float LUTBlendSpeed = 0.5f;
		[VisualSetting]
		public static int GroundPathFollowerNodes = 20;
		[VisualSetting]
		public static float LeviathanRTBlowBubblesInterval = 0.5f;
		[VisualSetting]
		public static float PlayerHijackLerp = 0.25f;

		#endregion

		public static int MaxGroupsLoadedPerUpdate = 5;
		public static float GroupLoadRequestTimeout = 60f;
		public static float ChunkTerrainDetailMin = 60f;
		public static float ChunkTerrainDetailMax = 2f;
		public static float ChunkTerrainGrassDistanceMin = 500f;
		public static float ChunkTerrainGrassDistanceMax = 50f;
		public static float ChunkTerrainTreeBillboardDistMax = 256f;
		public static float ChunkTerrainTreeBillboardDistMin = 32f;
		public static int ChunkTerrainMaxMeshTreesMax = 1024;
		public static int ChunkTerrainMaxMeshTreesMin = 16;
		public static float ChunkTerrainTreeDistance = 3500f;
		public static float ChunkTerrainGrassDetailDistanceImmedate = 1f;
		public static float ChunkTerrainGrassDensityImmediate = 1f;
		public static float ChunkTerrainTreeBillboardDistanceImmediate = 1f;
		public static float ChunkTerrainDetailImmediate = 1f;
		public static float ChunkTerrainMaxMeshTreesImmediate = 1f;
		public static float ChunkTerrainGrassDetailDistanceAdjascent = 1f;
		public static float ChunkTerrainGrassDensityAdjascent = 1f;
		public static float ChunkTerrainTreeBillboardDistanceAdjascent = 1f;
		public static float ChunkTerrainDetailAdjascent = 1f;
		public static float ChunkTerrainMaxMeshTreesAdjascent = 1f;
		public static float ChunkTerrainGrassDetailDistanceDistant = 1f;
		public static float ChunkTerrainGrassDensityDistant = 1f;
		public static float ChunkTerrainTreeBillboardDistanceDistant = 1f;
		public static float ChunkTerrainDetailDistant = 1f;
		public static float ChunkTerrainMaxMeshTreesDistant = 1f;
		public static int MaxChunkPrefabsPerChunk = 500;
		public static int BoxColliderPoolCount = 5000;
		public static int MaxChunkSceneryObjects = 1000;
		public static float FOVMax = 120f;
		public static float FOVMin = 60.0f;
		public static int SavedRecentlyTimeThreshold = 10;
		public static float MusicCrossfadeSpeed = 1f;
		public static int MaxVerticesPerStructureMesh = 4000;
		public static Vector3 WorldItemInstantiationOffset = new Vector3(0f, 10000f, 0f);
		public static Vector3 PlayerDeathHijackedOffset = new Vector3(5f, 10f, 5f);
		public static float MaxHouseOfHealingSearchDistance = 1500f;
		public static float SecondaryInterfaceDepthMultiplier = -100f;
		public static float SecondaryInterfaceDepthBase = -100f;
		public static float PathfindingRepathRatePaused = 1000f;
		public static float PathfindingRepathRateNormal = 0.5f;
		public static float PathfindingRepathRateHigh = 0.25f;
		public static int PathfindingNumSlices = 50;
		public static float PathfindingNodeSize = 1.5f;
		public static int PathfindingGridSliceNodes = 100;
		public static int PathfindingGridSliceSize = 150;
		public static bool BuildStructures = true;
		public static bool BuildActionNodesOnly = false;
		public static float RootGroupRefreshInterval = 5.0f;
		public static float GroupMinUnloadedTime = 10.0f;
		public static float GroupMaxUnloadedTime = 10.0f;
		public static float GroupMinLoadedTime = 10.0f;
		public static float MaximumLocationTerrainOffset = 0.5f;
		public static float SunMaximumElevation = 8000.0f;
		public static int BookPageCharacterLimit = 800;
		public static string DefaultWorldName = "Frontiers";
		public static string DefaultGameName = "Game01";
		public static string DefaultProfileName = "Default";
		public static int MinProfileNameCharacters = 3;
		public static int MaxProfileNameCharacters = 20;
		public static int MaxGameNameCharacters = 30;
		public static int MaxWorldChunksX = 32;
		public static int MaxWorldChunksZ = 32;
		public static int WorldChunkSize = 1500;
		public static int WorldChunkOffsetX = 0;
		public static int WorldChunkOffsetZ = -6000;
		public static int WorldChunkElevation = 4500;
		public static int WorldChunkTerrainSize = 1500;
		public static int WorldChunkTerrainHeightmapResolution	= 1025;
		public static int WorldChunkTerrainHeight = 1000;
		public static int WorldChunkSplatMapResolution = 1024;
		public static int WorldChunkColorOverlayResolution = 512;
		public static int WorldChunkDetailSliceResolution = 128;
		public static int WorldChunkDetailResolution = 1024;
		public static int WorldChunkDataMapResolution = 128;
		public static string WorldChunkSplatMapNamePrefix = "Splat";
		public static string WorldChunkColorMapNamePrefix = "Color";
		public static int WorldChunkDetailLayers = 12;
		public static bool DevMode = true;
		//inventory
		public static int MaxStacksPerContainer = 10;
		public static int NumInventoryStackContainers = 5;
		public static float MaxInventoryBoundsSize = 5.0f;
		public static int NumItemsPerStack = 1024;
		//player
		public static float PlayerPickUpRange = 3.5f;
		public static float OculusModeCameraRotationSensitivity = 10f;
		public static string DefaultCharacterBodyName = "Body_A_M_4";
		public static string DefaultCharacterFaceTextureName = "Face_CC_M_A";
		public static string DefaultCharacterBodyTextureName = "Body_Med_A_Settler_M_1";
		//raycast variables
		public const float ForwardVectorRange = 16.0f;
		public const float DownVectorRange = 16.0f;
		public const float VectorRangeMultiplier = 1.0f;
		public const float RaycastAllForwardDistance = 8.0f;
		public const float RaycastAllFocusDistance = 8.0f;
		public const float RaycastAllDownDistance = 8.0f;
		public const float RaycastTerrainHeightDistance = 100.0f;
		public const float RaycastAllUpDistance = 128.0f;
		public const float RaycastLineOfSightDistance = 4096f;
		public const float RaycastSpyGlassDistance = 4096f;
		public static float ScreenAspectRatioSqueezeMaximum = 1.5f;
		public static float ScreenAspectRatioSqueezeMinimum = 1.33f;
		public static int ScreenAspectRatioMax = 1080;
		public static int ScreenAspectRatioMin = 1280;
		public static int ScreenAspectRatioMaxVR = 640;
		//collision layer variables
		public const int LayerNumDefault = 0;
		public const int LayerNumPlayer = 8;
		public const int LayerNumTrigger = 9;
		public const int LayerNumBodyPart = 10;
		public const int LayerNumGUIRaycastFallThrough = 11;
		public const int LayerNumSolidTerrain = 12;
		public const int LayerNumFluidTerrain = 13;
		public const int LayerNumObstacleTerrain = 14;
		public const int LayerNumAwarenessBroadcaster = 15;
		public const int LayerNumAwarenessReceiver = 16;
		public const int LayerNumAwarenessLight = 17;
		public const int LayerNumGUIMap = 18;
		public const int LayerNumStructureTerrain = 19;
		public const int LayerNumWorldItemActive = 20;
		public const int LayerNumLocationBroadcaster = 21;
		public const int LayerNumWorldItemInventory = 22;
		public const int LayerNumPlayerTool = 23;
		public const int LayerNumGUIRaycastIgnore = 24;
		public const int LayerNumGUIRaycastCustom = 25;
		public const int LayerNumGUIRaycast = 26;
		public const int LayerNumGUIHUD = 27;
		public const int LayerNumStructureIgnoreCollider = 28;
		public const int LayerNumStructureCustomCollider = 29;
		public const int LayerNumScenery = 30;
		public const int LayerNumHidden = 31;
		public const string TagDefault = "Untagged";
		public const string TagGroundDirt = "GroundDirt";
		public const string TagGroundLeaves = "GroundLeaves";
		public const string TagGroundMetal = "GroundMetal";
		public const string TagGroundMud = "GroundMud";
		public const string TagGroundSnow = "GroundSnow";
		public const string TagGroundStone = "GroundStone";
		public const string TagGroundWater = "GroundWater";
		public const string TagGroundWood = "GroundWood";
		public const string TagGroundTerrain = "GroundTerrain";
		public static string TagHideCursorOnHover = "HideCursorOnHover";
		public static string TagBrowserObject = "GuiBrowserObject";
		public static string TagActiveObject = "GuiActiveObject";
		public static string TagGuiInputObject = "GuiInputObject";
		public static string TagColliderFluid = "ColliderFluid";
		public static string TagBodyLeg = "BodyLeg";
		public static string TagBodyArm = "BodyArm";
		public static string TagBodyHead = "BodyHead";
		public static string TagBodyTorso = "BodyTorso";
		public static string TagBodyGeneral = "BodyGeneral";
		public static string TagStateChild = "StateChild";
		public static string TagNonInteractive = "NonInteractive";
		public static string TagIgnoreTab = "IgnoreTab";
		public static string TagIgnoreStackedDoppleganger = "IgnoreStackedDoppleganger";

		public static float BodyPartDamageForceMultiplier = 2500f;
		public static string ControllerDefaultActionSpriteSuffix = "XBox";
		public const int LayerPlayer = 1 << 8;
		public const int LayerTrigger = 1 << 9;
		public const int LayerBodyPart = 1 << 10;
		public const int LayerGUIRaycastFallThrough = 1 << 11;
		public const int LayerSolidTerrain = 1 << 12;
		public const int LayerFluidTerrain = 1 << 13;
		public const int LayerObstacleTerrain = 1 << 14;
		public const int LayerAwarenessBroadcaster = 1 << 15;
		public const int LayerAwarenessReceiver = 1 << 16;
		public const int LayerAwarenessLight = 1 << 17;
		public const int LayerGUIMap = 1 << 18;
		public const int LayerStructureTerrain = 1 << 19;
		public const int LayerWorldItemActive = 1 << 20;
		public const int LayerLocationBroadcaster = 1 << 21;
		public const int LayerWorldItemInventory = 1 << 22;
		public const int LayerPlayerTool = 1 << 23;
		public const int LayerGUIRaycastIgnore = 1 << 24;
		public const int LayerGUIRaycastCustom = 1 << 25;
		public const int LayerGUIRaycast = 1 << 26;
		public const int LayerGUIHUD = 1 << 27;
		public const int LayerScenery = 1 << 30;
		public const int LayerHidden = 1 << 31;
		public const int LayersLightWorld = 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7
		                                    | LayerPlayer | LayerTrigger | LayerBodyPart | LayerGUIRaycastFallThrough
		                                    | LayerSolidTerrain | LayerObstacleTerrain
		                                    | LayerGUIMap | LayerWorldItemActive
		                                    | /*LayerWorldItemInventory |*/ LayerPlayerTool
																			/*| LayerGUIRaycastIgnore | LayerGUIRaycastCustom | LayerGUIRaycast*/
		                                    | LayerGUIHUD | LayerScenery | LayerHidden;
		public const int LayersActive = LayerWorldItemActive | LayerSolidTerrain | LayerFluidTerrain | LayerStructureTerrain | LayerObstacleTerrain | LayerBodyPart;
		public const int LayersItemOfInterest = LayersActive | LayersPlayerAndTools | LayerBodyPart;
		public const int LayersInactiveAndHidden = LayerHidden;
		public const int LayersInterface = LayerGUIRaycast | LayerGUIRaycastCustom | LayerGUIRaycastIgnore | LayerWorldItemInventory;
		public const int LayersPlayerAndTools = LayerPlayer | LayerPlayerTool;
		public const int LayersTerrain = LayerSolidTerrain | LayerFluidTerrain | LayerStructureTerrain | LayerObstacleTerrain;
		public const int LayersSolidTerrain = LayerSolidTerrain | LayerStructureTerrain | LayerObstacleTerrain;

		public static string DefaultMalePlayerBody = "Body_C_M_1";
		public static string DefaultMalePlayerBodyTexture = "Body_Lrg_C_Settler_U_1";
		public static string DefaultMalePlayerFaceTexture = "Face_CC_Player_M_A";
		public static string DefaultMalePlayerHairTexture = "Body_Lrg_C_Settler_U_1";
		public static string DefaultFemalePlayerBody = "Body_C_F_5";
		public static string DefaultFemalePlayerBodyTexture = "Body_Med_C_Settler_F_1";
		public static string DefaultFemalePlayerFaceTexture = "Face_CC_Player_F_A";
		public static string DefaultFemalePlayerHairTexture = "Body_Med_C_Settler_F_1";

		public static Dictionary <string, FieldInfo> Fields {
				get {
						return mFields;
				}
		}

		static Dictionary <string, FieldInfo> mFields;
		static List <FieldInfo> mDifficultySettingFields;
		static FieldInfo mFieldInfoCheck;
		static System.Object mConvertedValue;

		public static List <string> GetDifficultySettingNames()
		{
				if (mDifficultySettingFieldNames.Count == 0) {
						System.Type type = typeof(Globals);
						FieldInfo[] fields = type.GetFields();
						foreach (FieldInfo f in fields) {
								if (f.IsDefined(typeof(EditableDifficultySettingAttribute), true)) {
										mDifficultySettingFieldNames.Add(f.Name);
								}
						}
				}
				return mDifficultySettingFieldNames;
		}

		static List <string> mDifficultySettingFieldNames = new List<string>();

		public static void LoadDifficultySettingData(List<KeyValuePair<string, string>> globalPairs)
		{
				System.Type type = typeof(Globals);

				//for later lookup
				RefreshLookup();

				foreach (KeyValuePair <string,string> globalPair in globalPairs) {
						FieldInfo field = type.GetField(globalPair.Key);
						if (field.IsDefined(typeof(EditableDifficultySettingAttribute), true)) {
								try {
										field.SetValue(null, Convert.ChangeType(globalPair.Value, field.FieldType));
								} catch (Exception e) {
										//Debug.LogException (e);
								}
						}
				}
		}

		public static void LoadData(List<KeyValuePair<string, string>> globalPairs)
		{
				System.Type type = typeof(Globals);

				//for later lookup
				RefreshLookup();

				foreach (KeyValuePair <string,string> globalPair in globalPairs) {
						FieldInfo field = type.GetField(globalPair.Key);
						try {
								field.SetValue(null, Convert.ChangeType(globalPair.Value, field.FieldType));
						} catch (Exception e) {
								//Debug.LogException (e);
						}
				}
		}

		public static List <KeyValuePair <string,string>> GetData()
		{
				RefreshLookup();

				List <KeyValuePair <string,string>> globalPairs = new List<KeyValuePair<string, string>>();
				foreach (KeyValuePair<string,FieldInfo> field in Fields) {
						if (!field.Value.IsLiteral) {
								//don't save const values
								try {
										globalPairs.Add(new KeyValuePair <string, string>(field.Value.Name, field.Value.GetValue(null).ToString()));
								} catch (Exception e) {
										Debug.LogError("Error when attempting to get field for " + field.Value.Name);
										e = null;
								}
						}
				}
				return globalPairs;
		}

		public static int GetGlobalVariable(string varName)
		{
				Debug.Log("Getting global variable " + varName);
				int value = 0;
				FieldInfo f = null;
				if (Fields.TryGetValue(varName.ToLower(), out f)) {
						if (f.FieldType.Equals(typeof(int))) {
								Debug.Log("It's an int");
								value = (int)f.GetValue(null);
						} else if (f.FieldType.Equals(typeof(float))) {
								Debug.Log("It's a float");
								value = Mathf.FloorToInt((float)f.GetValue(null));
						} else if (f.FieldType.Equals(typeof(double))) {
								Debug.Log("It's a double");
								value = (int)Math.Floor((double)f.GetValue(null));
						} else {
								Debug.Log("Couldn't get type");
						}
						Debug.Log("Got global variable " + varName + ", int value is " + value.ToString() + " from value " + f.GetValue(null).ToString());
				} else {
						Debug.Log("Couldn't find " + varName + " in globals");
				}
				return value;
		}

		public static void RefreshLookup()
		{
				if (mFields == null) {
						mFields = new Dictionary<string, FieldInfo>();
						mDifficultySettingFields = new List <FieldInfo>();
				} else {
						mFields.Clear();
						mDifficultySettingFields.Clear();
				}
				System.Type type = typeof(Globals);
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo f in fields) {
						Type fieldType = f.GetType();
						mFields.Add(f.Name.ToLower(), f);
						if (f.IsDefined(typeof(EditableDifficultySettingAttribute), true)) {
								mDifficultySettingFields.Add(f);
						}
				}
		}

		public static IEnumerable<FieldInfo> GetDifficultySettings()
		{
				return mDifficultySettingFields;
		}

		public static void SetDifficultyVariable(string globalVariableName, string variableValue)
		{
				if (Fields.TryGetValue(globalVariableName, out mFieldInfoCheck)) {
						if (!mFieldInfoCheck.IsDefined(typeof(EditableDifficultySettingAttribute), true)) {
								Debug.Log(globalVariableName + " is not a difficulty setting, not applying");
								return;
						}
						mConvertedValue = null;
						switch (mFieldInfoCheck.FieldType.Name) {
								case "Int32":
										mConvertedValue = (int)Int32.Parse(variableValue);
										break;

								case "Boolean":
										mConvertedValue = (bool)Boolean.Parse(variableValue);
										break;

								case "Single":
										mConvertedValue = (float)Single.Parse(variableValue);
										break;

								case "Double":
										mConvertedValue = (double)Double.Parse(variableValue);
										break;

								default:
										Debug.Log("Couldn't determine field type for global " + globalVariableName);
										break;
						}
						mFieldInfoCheck.SetValue(null, mConvertedValue);
				}
		}
}

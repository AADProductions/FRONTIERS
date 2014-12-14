using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class Globals
{
		public static string PlayerManagerGameObjectName = "=PLAYER=";
		public static string GroupsManagerGameObjectName = "=GROUPS=";
		public static string ManagersGameObjectName = "=MANAGER=";
		public static string LoadingGameObjectName = "=LOADING=";
		public static string GameManagerGameObjectName = "=GAME=";

		#region difficulty

		[DifficultySetting]
		public static float DefaultWaterAccelerationPenalty = 0.15f;
		[DifficultySetting]
		public static float DefaultWaterJumpPenalty = 0.15f;
		[DifficultySetting]
		public static float RiverFlowForceMultiplier = 0.1f;
		[DifficultySetting]
		public static float RestStrengthRestoreSpeed = 0.05f;
		[DifficultySetting]
		public static double StatusKeeperTimecale = 1.0f;
		[DifficultySetting]
		public static float StatusKeeperNegativeChangeMultiplier = 1f;
		[DifficultySetting]
		public static float StatusKeeperPositiveChangeMultiplier = 1f;
		[DifficultySetting]
		public static float StatusKeeperNegativeFlowMultiplier = 1f;
		[DifficultySetting]
		public static float StatusKeeperPositiveFlowMultiplier = 1f;
		[DifficultySetting]
		public static float FastTravelStrengthReducedPerMeterTraveled = 0.001f;
		[DifficultySetting]
		public static int ReputationChangeTiny = 1;
		[DifficultySetting]
		public static int ReputationChangeSmall = 2;
		[DifficultySetting]
		public static int ReputationChangeMedium = 6;
		[DifficultySetting]
		public static int ReputationChangeLarge = 10;
		[DifficultySetting]
		public static int ReputationChangeHuge = 20;
		[DifficultySetting]
		public static float MinReputation = 1;
		[DifficultySetting]
		public static float MaxReputation = 100;
		[DifficultySetting]
		public static float BaseCurrencyToReputationMultiplier = 0.1f;
		[DifficultySetting]
		public static float MaxAudibleRange = 50f;
		[DifficultySetting]
		public static float MaxAwarenessDistance = 50f;
		[DifficultySetting]
		public static float MaxFieldOfView = 120f;
		[DifficultySetting]
		public static float LuminiteRegrowTime = 5f;
		[DifficultySetting]
		public static bool DarkrotSpawnsOnlyInForests = true;
		[DifficultySetting]
		public static float DarkrotMoveInterval = 2.0f;
		[DifficultySetting]
		public static float DarkrotWaitInterval = 2.0f;
		[DifficultySetting]
		public static float DarkrotWarmupTime = 2f;
		[DifficultySetting]
		public static float DarkrotMinAmount = 10;
		[DifficultySetting]
		public static float DarkrotMaxAmount = 100;
		[DifficultySetting]
		public static float DarkrotAvgAmount = 25;
		[DifficultySetting]
		public static float DarkrotEmitSoundProbability = 0.1f;
		[DifficultySetting]
		public static float DarkrotUpdateInterval = 0.125f;
		[DifficultySetting]
		public static float DarkrotMaxLightAndHeatExposure = 10f;
		[DifficultySetting]
		public static float DarkrotSpawnDistance = 10f;
		[DifficultySetting]
		public static float DarkrotDissipationTime = 2f;
		[DifficultySetting]
		public static float DarkrotBaseSpawnProbability = 0.01f;
		[DifficultySetting]
		public static float DarkrotPulseInterval = 2.0f;
		[DifficultySetting]
		public static int DarkrotMaxNodes = 100;
		[DifficultySetting]
		public static float DarkrotMaxSpeed = 1f;
		[DifficultySetting]
		public static float PlantAutoRegrowInterval = 5;
		[DifficultySetting]
		public static float PlantAutoReplantInterval = 100;
		[DifficultySetting]
		public static float InnBasePricePerNight = 12f;
		[DifficultySetting]
		public static float BarterMaximumPriceModifier = 0.25f;
		[DifficultySetting]
		public static int WealthLevelPoorBaseCurrency = 100;
		[DifficultySetting]
		public static int WealthLevelMiddleClassBaseCurrency = 1000;
		[DifficultySetting]
		public static int WealthLevelWealthyBaseCurrency = 10000;
		[DifficultySetting]
		public static int WealthLevelAristocracyBaseCurrency = 100000;
		[DifficultySetting]
		public static float TrappingMinimumRTCheckInterval = 30f;
		[DifficultySetting]
		public static float TrappingOddsTimeMultiplier = 0.001f;
		[DifficultySetting]
		public static float TrappingMinimumCorpseSpawnDistance = 50f;
		[DifficultySetting]
		public static float TrappingOddsDistanceMultiplier = 2.0f;
		[DifficultySetting]
		public static float DamageMaterialPenaltyMultiplier = 0.5f;
		[DifficultySetting]
		public static float DamageSumForceMultiplier = 0.35f;
		[DifficultySetting]
		public static float DamageFallDamageMultiplier = 1.25f;
		[DifficultySetting]
		public static float DamageMinimumFallImpactThreshold = 5f;
		[DifficultySetting]
		public static float DamageMaximumFallImpactThreshold = 50f;
		[DifficultySetting]
		public static float DamageMaterialBonusMultiplier = 2.0f;
		[DifficultySetting]
		public static float DamageOnRockslideHit = 25f;
		[DifficultySetting]
		public static float LeviathanMoveSpeed = 0.25f;
		[DifficultySetting]
		public static float LeviathanMinimumAttackDistance = 1.0f;
		[DifficultySetting]
		public static float LeviathanStartDistance = 50.0f;
		[DifficultySetting]
		public static float LeviathanRTMinimumStalkInterval = 1.0f;
		[DifficultySetting]
		public static float LeviathanRTLoseInterestInterval = 5.0f;
		[DifficultySetting]
		public static float FireBurnDistance = 0.5f;
		[DifficultySetting]
		public static float FireCookDistance = 2.0f;
		[DifficultySetting]
		public static float FireWarmDistance = 4.0f;
		[DifficultySetting]
		public static float FireScareDistance = 8.0f;
		[DifficultySetting]
		public static float WellRestedHours = 8.0f;
		[DifficultySetting]
		public static float StolenGoodsValueMultiplier = 0.0f;
		[DifficultySetting]
		public static float SkillCriticalFailure = 0.975f;
		[DifficultySetting]
		public static float SkillCriticalSuccess = 0.015f;
		[DifficultySetting]
		public static float SkillFailsafeMasteryLevel = 0.05f;
		[DifficultySetting]
		public static float PlayerEncounterRadius = 7.0f;
		[DifficultySetting]
		public static float PlayerControllerStepOffsetDefault = 0.3f;
		[DifficultySetting]
		public static float PlayerControllerSlopeLimitDefault = 55f;
		[DifficultySetting]
		public static int MaxHugeItemsPerStack = 1;
		[DifficultySetting]
		public static int MaxLargeItemsPerStack = 1;
		[DifficultySetting]
		public static int MaxMediumItemsPerStack = 10;
		[DifficultySetting]
		public static int MaxSmallItemsPerStack = 100;
		[DifficultySetting]
		public static int MaxTinyItemsPerStack = 1000;
		[DifficultySetting]
		public static float RequiredFoodPerGameHour = 0.125f;
		[DifficultySetting]
		public static float RequiredWaterPerGameHour = 0.125f;
		[DifficultySetting]
		public static float PlayerAverageMetersPerHour = 500.0f;
		[DifficultySetting]
		public static float PathStrayDistanceInMeters = 6.0f;
		[DifficultySetting]
		public static float PathStrayMinTimeInSeconds = 2.5f;
		[DifficultySetting]
		public static float PathStrayMaxTimeInSeconds = 15.0f;
		[DifficultySetting]
		public static float PathEasyMetersPerHour = PlayerAverageMetersPerHour;
		[DifficultySetting]
		public static float PathModerateMetersPerHour = PlayerAverageMetersPerHour * 0.75f;
		[DifficultySetting]
		public static float PathDifficultMetersPerHour = PlayerAverageMetersPerHour * 0.65f;
		[DifficultySetting]
		public static float PathDeadlyMetersPerHour = PlayerAverageMetersPerHour * 0.55f;
		[DifficultySetting]
		public static float PathImpassibleMetersPerHour = PlayerAverageMetersPerHour * 0.35f;
		[DifficultySetting]
		public static float GuildLibraryBasePrice = 10000;

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
		public static float DefaultCharacterFallAcceleration = 0.25f;
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
		public static float TimeScaleTravelMax = 1.5f;
		[WorldSetting]
		public static float TimeScaleTravelMin = 0.005f;

		#endregion

		#region visual

		[VisualSetting]
		public static float SceneryLODRatioPrimary = 0.75f;
		[VisualSetting]
		public static float SceneryLODRatioSecondary = 0.5f;
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
		//collision layer variables
		public const int LayerNumDefault = 0;
		public const int LayerNumPlayer = 8;
		public const int LayerNumTrigger = 9;
		public const int LayerNumMapBounds = 10;
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
		public const int LayerNumWorldItemInactive = 21;
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
		public const int LayerPlayer = 1 << 8;
		public const int LayerTrigger = 1 << 9;
		public const int LayerMapBounds = 1 << 10;
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
		public const int LayerWorldItemInactive = 1 << 21;
		public const int LayerWorldItemInventory = 1 << 22;
		public const int LayerPlayerTool = 1 << 23;
		public const int LayerGUIRaycastIgnore = 1 << 24;
		public const int LayerGUIRaycastCustom = 1 << 25;
		public const int LayerGUIRaycast = 1 << 26;
		public const int LayerGUIHUD = 1 << 27;
		public const int LayerScenery = 1 << 30;
		public const int LayerHidden = 1 << 31;
		public const int LayersLightWorld = 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7
		                                   | LayerPlayer | LayerTrigger | LayerMapBounds | LayerGUIRaycastFallThrough
		                                   | LayerSolidTerrain | LayerObstacleTerrain
		                                   | LayerGUIMap | LayerWorldItemActive
		                                   | LayerWorldItemInactive | /*LayerWorldItemInventory |*/ LayerPlayerTool
																			/*| LayerGUIRaycastIgnore | LayerGUIRaycastCustom | LayerGUIRaycast*/
		                                   | LayerGUIHUD | LayerScenery | LayerHidden;
		public const int LayersActive = LayerWorldItemActive | LayerSolidTerrain | LayerFluidTerrain | LayerStructureTerrain | LayerObstacleTerrain;
		public const int LayersItemOfInterest = LayersActive | LayersPlayerAndTools;
		public const int LayersInactiveAndHidden = LayerHidden | LayerWorldItemInactive;
		public const int LayersInterface = LayerGUIRaycast | LayerGUIRaycastCustom | LayerGUIRaycastIgnore | LayerWorldItemInventory;
		public const int LayersPlayerAndTools = LayerPlayer | LayerPlayerTool;
		public const int LayersTerrain = LayerSolidTerrain | LayerFluidTerrain | LayerStructureTerrain | LayerObstacleTerrain;
		public static Dictionary <string, FieldInfo> Fields;
		static FieldInfo mFieldInfoCheck;
		static System.Object mConvertedValue;

		public static void LoadDifficultySettingData(List<KeyValuePair<string, string>> globalPairs)
		{
				System.Type type = typeof(Globals);

				//for later lookup
				RefreshLookup();

				foreach (KeyValuePair <string,string> globalPair in globalPairs) {
						FieldInfo field = type.GetField(globalPair.Key);
						if (field.IsDefined(typeof(DifficultySettingAttribute), true)) {
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
				List <KeyValuePair <string,string>> globalPairs = new List<KeyValuePair<string, string>>();
				System.Type type = typeof(Globals);
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo field in fields) {
						if (!field.IsLiteral) {
								//don't save const values
								globalPairs.Add(new KeyValuePair <string, string>(field.Name, field.GetValue(null).ToString()));
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
				if (Fields == null) {
						Fields = new Dictionary<string, FieldInfo>();
				}
				Fields.Clear();
				System.Type type = typeof(Globals);
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo f in fields) {
						Type fieldType = f.GetType();
						Fields.Add(f.Name.ToLower(), f);
				}
		}

		public static void SetDifficultyVariable(string globalVariableName, string variableValue)
		{
				if (Fields.TryGetValue(globalVariableName, out mFieldInfoCheck)) {
						if (!mFieldInfoCheck.IsDefined(typeof(DifficultySettingAttribute), true)) {
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

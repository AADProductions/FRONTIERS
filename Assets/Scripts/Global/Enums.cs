using UnityEngine;
using System;
using System.Collections;

namespace Frontiers
{
	public enum CreatureOtherType
	{
		Player,
		SameKind,
		DifferentKind,
		AnyCreature,
		AnyOther,
		SpecificKind
	}

	public enum MotileGoToMethod
	{
		Pathfinding,
		StraightShot,
		UseDefault,
	}

	public enum CreatureBehaviorType
	{
		FleeOther,
		AttackOther,
		FollowOther,
		Freeze,
		Ignore,
	}

	[Flags]
	public enum CreatureBehaviorCondition
	{
		None = 0,
		OnSeeOther = 1,
		OnIsSick = 2,
		OnIsWounded = 4,
		OnIsNearlyDead = 8,
		OnOtherIsNearSelf = 16,
		OnOtherIsNearDen = 32,
		OnOtherIsNearGoal = 64,
		OnOtherIsPursuing = 128,
		OnIsUnhealthy = OnIsSick | OnIsWounded | OnIsNearlyDead,
		OnOtherIsNear = OnOtherIsNearSelf | OnOtherIsNearDen | OnOtherIsNearGoal | OnOtherIsPursuing,
	}

	public enum ShortTermMemoryLength
	{
		Short,
		Medium,
		Long,
	}

	[Flags]
	public enum ResponseType
	{
		None = 0,
		ProximityToSelf = 1,
		ProximityToDen = 2,
		PackIsHostile = 4,
		DamageToSelf = 8,
		DamageToDen = 16,
		DamageToPack = 32,
	}

	public enum StubbornnessType
	{
		Passive,
		Independent,
		Willful,
		Untrainable
	}

	public enum GrudgeLengthType
	{
		None,
		Awhile,
		Forever
	}

	public enum AwarnessDistanceType
	{
		Poor,
		Fair,
		Good,
		Excellent,
		Prescient,
	}

	public enum BehaviorType
	{
		ForgetAfterTime,
		ForgetAfterAnyAction,
		ForgetAfterSpecificAction,
	}

	public enum CreatureAction
	{
		None,
		Eat,
		Sleep,
		EncounterPlayer,
		EncounterCreature,
		Attack,
		TakeDamage,
	}

	public enum CreatureType
	{
		LandHerbivore,
		LandCarnivore,
		WaterHerbivore,
		WaterCarnivore,
	}

	public enum AttitudeType
	{
		Curious,
		Skittish,
		Indiffierent,
		Frightened,
		Hostile,
	}

	public enum AwarenessLevelType
	{
		Unaware,
		Relaxed,
		Alarmed,
		Threatened,
	}

	public enum MotileActionError
	{
		None,
		Replaced,
		PriorityConflict,
		TargetInaccessible,
		TargetNotLoaded,
		TargetNotFound,
		Canceled,
		MotileIsDead,
	}

	public enum TriggerBehavior
	{
		Once,
		Toggle
	}

	public enum ConfirmationBehavior
	{
		Never,
		Once,
		Always,
	}

	public enum TriggerEvent
	{
		TriggerStart,
		TriggerCancel,
		TriggerComplete,
		TriggerFail,
	}

	public enum ButtonStyle
	{
		ReflectStateLiteral,
		ReflectStateToggle,
		Springy,
		Permanent,
	}

	public enum MotileActionState
	{
		NotStarted,
		Starting,
		Started,
		Waiting,
		Finishing,
		Finished,
		Error,
	}

	public enum MotileYieldBehavior
	{
		YieldAndFinish,
		YieldAndWait,
		DoNotYield
	}

	public enum ChangeVariableType
	{
		Increment,
		Decrement,
		SetValue
	}

	public enum MotileExpiration
	{
		Duration,
		TargetInRange,
		TargetOutOfRange,
		Never,
		NextNightfall,
		NextDaybreak,
	}

	public enum MotileActionType
	{
		FollowRoutine,
		FollowTargetHolder,
		FollowGoal,
		GoToActionNode,
		FocusOnTarget,
		WanderIdly,
		Wait,
		Die,
		FleeGoal,
	}

	public enum MotileInstructions
	{
		None,
		CompanionInstructions,
		PilgrimInstructions,
		InheritFromBase,
	}

	public enum MotileFollowType
	{
		Stalker,
		Companion,
		Follower,
		Attacker,
	}

	public enum MotileActionPriority
	{
		Normal,
		ForceTop,
		ForceBase,
		Next,
	}

	public enum FollowPathMode
	{
		None,
		FollowingPath,
		WaitingForObstruction,
		ReachedEndOfPath,
		ReachedPilgrimStop,
		MovingToPilgrimStop,
	}

	public enum GuildCredentials
	{
		None,
		Novice,
		Apprentice,
		Journeyman,
		Specialist,
		Master,
		Starwalker,
	}

	[Flags]
	public enum BodyPartType
	{
		Head,
		Face,
		Eye,
		Neck,
		Shoulder,
		Chest,
		Arm,
		Hand,
		Wrist,
		Finger,
		Hip,
		Leg,
		Shin,
		Foot,
		Segment,
		None,
	}

	[Flags]
	public enum BodyOrientation
	{
		None = 0,
		Left = 1,
		Right = 2,
		Both = Left | Right,
	}

	[Flags]
	public enum CharacterGender
	{
		None = 0,
		Male = 1,
		Female = 2,
	}

	public enum CharacterAction
	{
		MoveLieDown,
		MoveStand,
		MoveSit,
		MoveWalk,
		MoveRun,
		MoveJump,
		MoveCrouch,
		MoveWrite,
		ActionDie,
		ActionSleep,
		ActionWakeUp,
	}

	[Flags]
	public enum CharacterEyeColor
	{
		None = 0,
		Black = 1,
		Brown = 2,
		DarkBrown = 4,
		Gray = 8,
		Green = 16,
		LightBrown = 32,
		Purple = 64,
		Silver = 128,
		Blue = 256,
		LightBlue = 512,
	}

	[Flags]
	public enum CharacterHeight
	{
		Short = 1,
		BelowAverage	= 2,
		Average = 4,
		AboveAverage	= 8,
		Tall = 16
	}

	[Flags]
	public enum CharacterEyeState
	{
		Healthy = 1,
		Bloodshot = 2,
		Darkrot = 4,
		Blackened = 8,
	}

	[Flags]
	public enum CharacterGeneralAge
	{
		ChildToTeens = 1,
		TwentiesToThirties	= 2,
		FortiesToFifties	= 4,
		SixtiesToSeventies	= 8,
		Ancient = 16,
	}

	[Flags]
	public enum CharacterEthnicity
	{
		None = 0,
		Caucasian = 1,
		BlackCarribean	= 2,
		HanChinese = 4,
		EastIndian = 8,
	}

	[Flags]
	public enum CharacterFacialHair
	{
		NoBeard = 1,
		FatBeard = 2,
		MonkBeard = 4,
		RegalBeard = 8,
		StrongBeard = 16,
		SkinnyBeard = 32,
		LongBeard = 64,
		TextureBeard	= 128,
	}

	[Flags]
	public enum CharacterHairLength
	{
		None = 0,
		Long = 1,
		Short = 2,
	}

	[Flags]
	public enum CharacterHairColor
	{
		None = 0,
		Gray = 1,
		Black = 2,
		Brown = 4,
		Blonde = 8,
		Red = 16,
	}

	[Flags]
	public enum HairType
	{
		Bald = 0,
		ShortCropped	= 1,
		LongSeparate	= 2,
	}

	public enum ChunkMode
	{
		Primary,
		Immediate,
		Adjascent,
		Distant,
		Unloaded,
	}

	public enum CharacterElementType
	{
		BodyMesh,
		Texture,
		HairMesh,
	}

	public enum EmotionalState
	{
		Neutral,
		Angry,
		Excited,
		Happy,
		Sad,
		Scared,
		Surprised,
	}

	public enum LocationTerrainType
	{
		AboveGround,
		BelowGround,
		Transition,
	}

	public enum PlayerStatusOverTimeMethod
	{
		RapidPulse,
		SlowPulse,
		Continuous,
	}

	public enum PlayerStatusInterval
	{
		OnePing,
		HalfHour,
		OneHour,
		TwoHours,
		HalfDay,
		OneDay,
		Indefinite,
	}

	public enum HallucinogenicStrength
	{
		None,
		Mild,
		Moderate,
		Strong,
	}

	public enum PoisonStrength
	{
		None,
		Mild,
		Moderate,
		Strong,
		Deadly
	}

	public enum PlayerStatusRestore
	{
		A_None,
		B_OneFifth,
		C_TwoFifths,
		D_ThreeFifths,
		E_FourFifths,
		F_Full,
	}

	public enum WISize
	{
		Tiny,
		Small,
		Medium,
		Large,
		Huge,
		NoLimit,
	}

	public enum ItemWeight
	{
		Weightless,
		Light,
		Medium,
		Heavy,
		Unliftable
	}

	public enum LocationRevealMethod
	{
		None,
		ByDefault,
		ByTool,
		ByMagic,
		ByBook,
		ByCharacter,
		ByCompanion,
	}

	public enum LocationVisitMethod
	{
		None,
		ByDefault,
		ByTool,
		ByMagic,
		ByBook,
		ByCharacter,
		ByCompanion,
	}

	public enum WIColliderType
	{
		Sphere,
		Box,
		ConvexMesh,
		Capsule,
		UseExisting,
		None,
		Mesh,
	}

	public enum WorldItemAction
	{
		ChangeMode,
		TakeDamage,
		KilledByDamage,
		ChangeName,
	}

	public enum Immersion
	{
		None,
		Immersed,
		FullyImmersed,
		UnderLiquid
	}

	public enum PlayerGender
	{
		Male,
		Female,
		None,
	}

	public enum PlayerIllnessType
	{
		None,
		Cold,
		Flu,
		Pnemonia,
		FoodPoisoning,
		Dysentary
	}

	public enum PlayerStatusInfluence
	{
		Sunlight,
		ColdAmbientTemperature,
		WarmAmbientTemperature,
		ColdImmersion,
		WarmImmersion,
	}

	public enum InjuryType
	{
		None,
		Burn,
		Cut,
		Blunt,
		Sprain,
		Break
	}

	public enum InjuryDressing
	{
		None,
		Bandage,
		Turniquet,
		Cauterize
	}

	public enum InjuryComplication
	{
		None,
		Gangrenous,
		Infected,
		Poisoned
	}

	public enum PlayerIllnessSymptom
	{
		None = 0,
		Fever,
		Vomiting,
		Diarrhea,
		Fatigue,
		Chills,
		RespiratoryFailure,
	}

	[Serializable]
	[Flags]
	public enum TimeOfDay
	{
		a_None = 0,
		aa_TimeMidnight = 1,
		//12am
		ab_TimePostMidnight = 2,
		// 2am
		ac_TimePreDawn = 4,
		// 4am
		ad_TimeDawn = 8,
		// 6am
		ae_TimePostDawn = 16,
		// 8am
		af_TimePreNoon = 32,
		//10am
		ag_TimeNoon = 64,
		//12pm
		ah_TimePostNoon = 128,
		// 2pm
		ai_TimePreDusk = 256,
		// 4pm
		aj_TimeDusk = 512,
		// 6pm
		ak_TimePostDusk = 1024,
		// 8pm
		al_TimePreMidnight = 2048,
		//10pm
		ba_LightSunLight = ad_TimeDawn | ae_TimePostDawn | af_TimePreNoon | ag_TimeNoon | ah_TimePostNoon | ai_TimePreDusk | aj_TimeDusk,
		//6am - 8pm (14 hours)
		bb_LightMoonLight = ak_TimePostDusk | al_TimePreMidnight | aa_TimeMidnight | ab_TimePostMidnight | ac_TimePreDawn,
		//8pm - 6am (10 hours)
		ca_QuarterMorning = ad_TimeDawn | ae_TimePostDawn | af_TimePreNoon,
		// 6am - 12pm (6 hours)
		cb_QuarterAfternoon = ag_TimeNoon | ah_TimePostNoon | ai_TimePreDusk,
		//12pm - 6pm  (6 hours)
		cc_QuarterEvening = aj_TimeDusk | ak_TimePostDusk | al_TimePreMidnight,
		// 6pm - 12am (6 hours)
		cd_QuarterNight = aa_TimeMidnight | ab_TimePostMidnight | ac_TimePreDawn,
		//12am - 6am  (6 hours)
		da_WorkWakingHour = ac_TimePreDawn | ad_TimeDawn | ae_TimePostDawn,
		// 4am - 10am (6 hours)
		db_WorkWorkingHour = af_TimePreNoon | ag_TimeNoon | ah_TimePostNoon,
		//10am - 4pm  (6 hours)
		dc_WorkMagicHour = ai_TimePreDusk | aj_TimeDusk,
		// 4pm - 8pm  (4 hours)
		dd_WorkHuntingHour = ak_TimePostDusk | al_TimePreMidnight,
		// 8pm - 12am (4 hours)
		de_WorkWitchingHour = aa_TimeMidnight | ab_TimePostMidnight,
		//12am - 4am  (4 hours)
		ea_MealBreakfast = ae_TimePostDawn | af_TimePreNoon,
		// 8am - 12pm (4 hours)
		eb_MealLunch = ag_TimeNoon | ah_TimePostNoon,
		//12pm - 4pm  (4 hours)
		ec_MealDinner = aj_TimeDusk | ak_TimePostDusk,
		// 6pm - 10pm (4 hours)
		ed_MealFasting = al_TimePreMidnight | aa_TimeMidnight | ab_TimePostMidnight | ac_TimePreDawn | ad_TimeDawn,
		//(12 hours)
		ff_All = aa_TimeMidnight | ab_TimePostMidnight | ac_TimePreDawn | ad_TimeDawn | ae_TimePostDawn | af_TimePreNoon | ag_TimeNoon | ah_TimePostNoon | ai_TimePreDusk | aj_TimeDusk | ak_TimePostDusk | al_TimePreMidnight,
	}

	[Flags]
	public enum TimeOfYear
	{
		None = 0,
		MonthJanuary	= 1,
		MonthFebruary	= 2,
		MonthMarch = 4,
		MonthApril = 8,
		MonthMay = 16,
		MonthJune = 32,
		MonthJuly = 64,
		MonthAugust = 128,
		MonthSeptember	= 256,
		MonthOctober	= 512,
		MonthNovember	= 1024,
		MonthDecember	= 2048,
		SubSeasonPreWinter	= MonthDecember,
//11
		SubSeasonMidWinter	= MonthJanuary,
//0
		SubSeasonPostWinter	= MonthFebruary,
//1
		SubSeasonPreSpring	= MonthMarch,
//2
		SubSeasonMidSpring	= MonthApril,
//3
		SubSeasonPostSpring	= MonthMay,
//4
		SubSeasonPreSummer	= MonthJune,
//5
		SubSeasonMidSummer	= MonthJuly,
//6
		SubSeasonPostSummer	= MonthAugust,
//7
		SubSeasonPreAutumn	= MonthSeptember,
//8
		SubSeasonMidAutumn	= MonthOctober,
//9
		SubSeasonPostAutumn	= MonthNovember,
//10
		SeasonWinter = SubSeasonPreWinter | SubSeasonMidWinter | SubSeasonPostWinter,
		SeasonSpring = SubSeasonPreSpring | SubSeasonMidSpring | SubSeasonPostSpring,
		SeasonSummer = SubSeasonPreSummer | SubSeasonMidSummer | SubSeasonPostSummer,
		SeasonAutumn = SubSeasonPreAutumn | SubSeasonMidAutumn | SubSeasonPostAutumn,
		WetSeason = SeasonWinter | SeasonSpring,
		DrySeason = SeasonSummer | SeasonAutumn,
		AllYear = WetSeason | DrySeason,
	}

	[Flags]
	public enum GooNodeFlags : int
	{
		None = 0,
		Solid = 1,
		Liquid = 2,
		Gas = 4,
		BlobContainer = 8
	}

	public enum CycleInterpolation
	{
		EaseInOutSmooth,
	}

	[Flags]
	public enum NodeRelation : uint
	{
		None = 0,
		Self = 1,
		Sibling = 2,
		Cousin = 4,
		Parent = 8,
		Uncle = 16,
		GrandParent = 32,
		GrandUncle = 64,
		Child = 128,
		GrandChild = 256,
		Neighbor = 1024
	}

	public enum LayerMaskNoiseType
	{
		Smooth,
		Pink,
		VoronoiPits,
		Ridges,
		SharpRidges,
		RandomPits,
		MountainRidges,
	}

	[Flags, Serializable]
	public enum SurfaceOrientation : byte
	{
		None = 0,
		FlatFloor = 1,
		SteepFloor = 2,
		Floor = FlatFloor | SteepFloor,
		FlatCeiling = 4,
		SteepCeiling	= 8,
		Ceiling = FlatCeiling | SteepCeiling,
		FlatWall = 16,
		SteepWallTop	= 32,
		SteepWallBottom	= 64,
		Wall = FlatWall | SteepWallTop | SteepWallBottom,
		All = FlatFloor | SteepFloor | FlatCeiling | SteepCeiling | FlatWall | SteepWallTop | SteepWallBottom,
	}

	[Flags, Serializable]
	public enum ElevationCutoff : ushort
	{
		None = 0,
		TheDepths = 1,
		Chasm = 2,
		Valley = 4,
		Lowland = 8,
		SeaLevel = 16,
		Plain = 32,
		Hill = 64,
		Highland = 128,
		Mountain = 256
	}

	[Serializable]
	public enum PlayerClothingType
	{
		Head,
		Torso,
		Hands,
		Legs,
		Feet
	}

	[Serializable]
	public enum PlayerInventoryItemType
	{
		Generic,
		Tool,
		Clothing,
		FoodStuff,
		Money,
		Letter,
		InventoryEnabler,
	}

	public enum HealthState
	{
		Healthy,
		Wounded,
		Sick,
		Unconscious,
		Dying,
		Dead
	}

	[Flags]
	public enum WIMaterialType
	{
		None = 0,
		Dirt = 1,
		Stone = 2,
		Wood = 4,
		Metal = 8,
		Flesh = 16,
		Glass = 32,
		Liquid = 64,
		Fabric = 128,
		Fire = 256,
		Ice = 512,
		Bone = 1024,
		Plant = 2048,
		Food = 4096,
		Crystal = 8192
	}

	[Flags]
	public enum WIActiveState
	{
		Invisible = 0,
		Visible = 1,
		Active = 2,
	}

	[Flags]
	public enum WILoadState
	{
		None = 0,
		Uninitialized = 1,
		Initializing = 2,
		Initialized = 4,
		PreparingToUnload = 8,
		Unloading = 16,
		Unloaded = 32,
	}

	[Flags]
	public enum WIGroupLoadState
	{
		None = 0,
		Uninitialized = 1,
		Initializing = 2,
		Initialized = 4,
		PreparingToLoad = 8,
		Loading = 16,
		Loaded = 32,
		PreparingToUnload = 64,
		Unloading = 128,
		Unloaded = 256,
	}

	[Flags]
	public enum WIMode
	{
		None = 0,
		World = 1,
		Frozen = 2,
		Placed = 4,
		Stacked = 8,
		Wear = 16,
		Equipped = 32,
		Destroyed = 64,
		Hidden = 128,
		Selected = 256,
		Crafting = 512,
		Placing = 1024,
		Unloaded = 2048,
		RemovedFromGame = 4096,
	}

	[Flags]
	public enum OctantRegion : byte
	{
		ROOT = 0,
		YTop = 1,
		YBot = 2,
		ZFnt = 4,
		ZBck = 8,
		XLft = 16,
		XRgt = 32,
		YTopLft = YTop | XLft,
		YTopRht = YTop | XRgt,
		YTopFnt = YTop | ZFnt,
		YTopBck = YTop | ZBck,
		YBotLft = YBot | XLft,
		YBotRgt = YBot | XRgt,
		YBotFnt = YBot | ZFnt,
		YBotBck = YBot | ZBck,
		ZFntTop = ZFnt | YTop,
		ZFntBot = ZFnt | YBot,
		ZFntLft = ZFnt | XLft,
		ZFntRgt = ZFnt | XRgt,
		ZBckTop = ZBck | YTop,
		ZBckBot = ZBck | YBot,
		ZBckLft = ZBck | XLft,
		ZBckRgt = ZBck | XRgt,
		A_TopFntRgt = YTop | ZFnt | XRgt,
		B_TopBckRgt = YTop | ZBck | XRgt,
		C_TopBckLft = YTop | ZBck | XLft,
		D_TopFntLft = YTop | ZFnt | XLft,
		E_BotFntLft = YBot | ZFnt | XLft,
		F_BotBckLft = YBot | ZBck | XLft,
		G_BotBckRgt = YBot | ZBck | XRgt,
		H_BotFntRgt = YBot | ZFnt | XRgt,
	}

	public enum GrowableType
	{
		Seed,
		Branch,
		BranchChain,
		FeatherChain,
		MultiFork,
		Foliage,
		Flair,
		Tip,
		Edible
	}

	public enum GooThermalState
	{
		Normal,
		Igniting,
		Boiling,
		Melting,
		Smoking,
		Burning,
		Freezing,
		Smoldering,
	}

	[Flags]
	public enum GrowableSurface : ushort
	{
		Self = 0,
		OtherGrowers = 1,
		Floors = 2,
		Walls = 4,
		Ceilings = 8,
		FluidSurfaces = 16,
		FluidInteriors = 32,
		SpecificExceptions	= 64,
	}

	[Flags]
	public enum PlantType : ushort
	{
		Trees = 1,
		DeadTrees = 2,
		Bushes = 4,
		Grasses = 8,
		Clingers = 16,
		Fungus = 32,
		Crops = 64,
		Epiphytes = 128,
		Parasites = 256,
		BaseTerrainPlants	= Trees | DeadTrees | Bushes | Grasses,
		BioTerrainPlants	= Clingers | Fungus | Crops | Epiphytes | Parasites,
	}

	public enum DialogResult
	{
		None,
		Yes,
		No,
		Cancel
	}

	public enum PathDirection : int
	{
		None = 0,
		Backwards = -1,
		Forward = 1,
	}

	public enum PathType : int
	{
		None	= 0,
		Path	= 1,
		Trail	= 2,
		Track	= 3,
		Byway	= 4,
		Road	= 5,
	}

	public enum PathUsage : int
	{
		Never = 0,
		AlmostNever = 1,
		Seldom = 2,
		Occasionally = 3,
		Often = 4,
		Frequently = 5,
	}

	public enum PathDifficulty : int
	{
		None = 0,
		Easy = 1,
		Moderate = 2,
		Difficult = 3,
		Deadly = 4,
		Impassable = 5,
	}

	public enum WorldStructureObjectType
	{
		OuterEntrance,
		InnerEntrance,
		Secret,
		Trap,
		Room,
		Machine,
	}
}
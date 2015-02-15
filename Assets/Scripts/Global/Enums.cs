using UnityEngine;
using System;
using System.Collections;

//any enums that aren't used internally by a class are put here
//this is to help reduce references as much as possible
namespace Frontiers
{
		[Serializable]
		public enum BehaviorTOD
		{
				None,
				Diurnal,
				Nocturnal,
				CrepuscularDusk,
				CrepuscularDawn,
				CustomHours,
				All,
		}

		[Serializable]
		[Flags]
		public enum TreeColliderFlags
		{
				None = 0,
				Solid = 1,
				Impede = 2,
				Ignore = 4,
				Thorns = 8,
				Rustle = 16,
		}

		[Flags]//used to store user settings
		public enum ActionSettingType
		{
				None = 0,
				Down = 1,
				Up = 2,
				Hold = 4,
				Change = 8
		}

		[Flags]
		public enum UserActionType : int
		{
				NoAction = 1,
				//0
				Move = 2,
				//1
				MoveForward = 4,
				//2
				MoveRun = 8,
				//3
				MoveLeft = 16,
				//4
				MoveRight = 32,
				//5
				MoveJump = 64,
				//6
				MoveCrouch = 128,
				//7
				MoveStand = 256,
				//8
				MovePlantFeet = 512,
				//9
				MoveSprint = 1024,
				//10
				MoveWalk = 2048,
				//11
				ItemPickUp = 4096,
				//12
				ItemThrow = 8192,
				//13
				ItemUse = 16384,
				//14
				ItemInteract = 32768,
				//15
				ToolUse = 65536,
				//16
				ToolUseHold = 131072,
				//17
				ToolUseRelease = 262144,
				//18
				ToolHolster = 524288,
				//19
				ActionConfirm = 1048576,
				//20
				ActionCancel = 2097152,
				//21
				ActionSkip = 4194304,
				//22
				LookAxisChange = 8388608,
				//23
				MovementAxisChange = 16777216,
				//24
				ToolCyclePrev = 33554432,
				//25
				ToolCycleNext = 67108864,
				//26
				ToolSwap = 134217728,
				//27
				FlagsMovement = Move | MoveForward | MoveRun | MoveLeft | MoveRight | MoveJump | MoveCrouch | MoveStand | MovePlantFeet | MoveSprint | MoveWalk | MovementAxisChange,
				FlagsItems = ItemPickUp | ItemThrow | ItemUse | ItemInteract,
				FlagsTools = ToolUse | ToolUseHold | ToolUseRelease | ToolHolster | ToolCyclePrev | ToolCycleNext | ToolSwap,
				FlagsActions = ActionConfirm | ActionCancel | ActionSkip,
				FlagsAll = FlagsMovement | FlagsItems | FlagsTools | FlagsActions | LookAxisChange,
				FlagsAllButActions = FlagsMovement | FlagsItems | FlagsTools | LookAxisChange,
				FlagsAllButMovement = FlagsItems | FlagsTools | FlagsActions | LookAxisChange,
				FlagsAllButLookAxis = FlagsMovement | FlagsItems | FlagsTools | FlagsActions,
				FlagsBasicMovement = Move | MoveForward | MoveRun | MoveLeft | MoveRight | MoveSprint,
				ItemPlace = ItemThrow | ItemUse,
		}

		[Flags]
		[Serializable]
		public enum TimeActionType
		{
				NoAction = 0,
				DaytimeStart = 1,
				NightTimeStart = 2,
				HourStart = 4,
		}

		public enum PauseBehavior
		{
				Pause,
				DoNotPause,
				PassThrough,
		}

		[Flags]
		[Serializable]
		public enum InterfaceActionType : int
		{
				NoAction = 1,
				//0
				InventoryNextQuickslot = 2,
				//1
				InventoryPrevQuickslot = 4,
				//2
				ToggleInterface = 8,
				//3
				ToggleInventory = 16,
				//4
				ToggleInventoryClothing = 32,
				//5
				ToggleInventoryCrafting = 64,
				//6
				ToggleStatus = 128,
				//7
				ToggleMap = 256,
				//8
				ToggleLog = 512,
				//9
				ToggleInterfaceNext = 1024,
				//10
				ToggleLogSkills = 2048,
				//11
				ToggleLogBooks = 4096,
				//12
				ToggleLogPeople = 8192,
				//13
				SelectionUp = 16384,
				//14
				SelectionDown = 32768,
				//15
				SelectionLeft = 65536,
				//16
				SelectionRight = 131072,
				//17
				SelectionAdd = 262144,
				//18
				SelectionRemove = 524288,
				//19
				SelectionReplace = 1048576,
				//20
				SelectionNext = 2097152,
				//21
				SelectionPrev = 4194304,
				//22
				CursorMove = 8388608,
				//23
				CursorClick = 16777216,
				//24
				SelectionNumeric = 33554432,
				//25
				ToggleInventorySecondary = 67108864,
				//26
				GamePause = 134217728,
				//27
				CursorRightClick = 268435456,
				//28
				InterfaceHide = 536870912,

				FlagsTogglePrimary = ToggleInventory | ToggleStatus | ToggleMap | ToggleLog,
				FlagsSelectionNextPrev = SelectionNext | SelectionPrev,
				FlagsAll = InventoryNextQuickslot | InventoryPrevQuickslot
				| ToggleInterface | ToggleInventory | ToggleInventoryClothing
				| ToggleInventoryCrafting | ToggleStatus | ToggleMap
				| ToggleLog | ToggleInterfaceNext | ToggleLogSkills
				| ToggleLogBooks | ToggleLogPeople
				| SelectionUp | SelectionDown | SelectionLeft | SelectionRight
				| SelectionAdd | SelectionRemove | SelectionReplace | SelectionNext | SelectionPrev
				| CursorMove | CursorClick | GamePause | CursorRightClick,
		}

		[Flags]
		public enum PlayerIDFlag : int
		{
				Local = 1,
				Player01 = 2,
				Player02 = 4,
				Player03 = 8,
				Player04 = 16,
				Player05 = 32,
				Player06 = 64,
				Player07 = 128,
				Player08 = 256,
				Player09 = 512,
				Player10 = 1024,
				Player11 = 2048,
				Player12 = 4096,
				Player13 = 8192,
				Player14 = 16384,
				Player15 = 32768,
				Player16 = 65536,
				Player17 = 131072,
				Player18 = 262144,
				Player19 = 524288,
				Player20 = 1048576,
				Player21 = 2097152,
				Player22 = 4194304,
				Player23 = 8388608,
				Player24 = 16777216,
		}

		[Flags]
		public enum AvatarActionType : int
		{
				NoAction = 1,
				System = 2,
				Npc = 4,
				Barter = 8,
				Move = 16,
				Location = 32,
				Survival = 64,
				Surroundings = 128,
				Magic = 256,
				Skill = 512,
				Item = 1024,
				Path = 2048,
				Travel = 4096,
				Mission = 8192,
				Book = 16384,
				Tool = 32768,
				Control = 65536,
				Trigger = 131072,
				FlagsAll = NoAction | System | Npc | Barter | Move | Location | Survival | Surroundings | Magic | Skill | Item | Path | Travel | Mission | Book | Tool | Control | Trigger
		}

		public enum AvatarAction : int
		{
				NoAction,
				//	System
				SystemSpawn,
				SystemPause,
				SystemResume,
				SystemEnterGame,
				SystemLeaveGame,
				//	NPCs
				NpcConverseStart,
				NpcConverseEnd,
				NpcDie,
				NpcChangeEmotion,
				//	Barter
				BarterInitiate,
				BarterMakeTrade,
				BarterMakeTradeFail,
				BarterCancel,
				//	Movement
				Move,
				MoveJump,
				MoveSwim,
				MoveGlide,
				MoveLandOnGround,
				MoveStayOnGround,
				MoveWalk,
				MoveStand,
				MoveSit,
				MoveLayDown,
				MoveCrouch,
				MoveSprint,
				MoveSprintFaster,
				MoveEnterWater,
				MoveExitWater,
				MovePassThrough,
				MoveStopMoving,
				//	Locations
				LocationDiscover,
				LocationReveal,
				LocationVisit,
				LocationLeave,
				LocationAlter,
				LocationCreate,
				LocationDestroy,
				LocationUpgrade,
				LocationStructureEnter,
				LocationStructureExit,
				LocationUndergroundEnter,
				LocationUndergroundExit,
				LocationCityEnter,
				LocationCityExit,
				LocationRegionEnter,
				LocationRegionExit,
				LocationWorldRegionEnter,
				LocationWorldRegionExit,
				LocationPurchase,
				LocationSell,
				LocationAquire,
				//	Survival
				SurvivalSpawn,
				SurvivalDie,
				SurvivalResurrect,
				SurvivalSleep,
				SurvivalWakeUp,
				SurvivalTakeDamage,
				SurvivalTakeDamageCritical,
				SurvivalTakeDamageOverkill,
				SurvivalKilledByDamage,
				SurvivalLoseStatus,
				SurvivalRestoreStatus,
				SurvivalConditionAdd,
				SurvivalConditionRemove,
				SurvivalCivilizationEnter,
				SurvivalCivilizationLeave,
				SurvivalDangerEnter,
				SurvivalDangerExit,
				//	Surroundings
				SurroundingsExposeToRain,
				SurroundingsExposeToSun,
				SurroundingsExposeToSky,
				SurroundingsShieldFromRain,
				SurroundingsShieldFromSun,
				SurroundingsShieldFromSky,
				//	Magic
				MagicUse,
				MagicUseFail,
				//	Skills
				SkillUse,
				SkillLearn,
				SkillDiscover,
				SkillUpgrade,
				SkillUseFail,
				SkillExperienceGain,
				SkillExperienceLose,
				SkillCredentialsGain,
				//	Items
				ItemPickUp,
				ItemPlace,
				ItemThrow,
				ItemDrop,
				ItemCarry,
				ItemDamage,
				ItemDestroy,
				ItemPlaceFail,
				ItemGive,
				ItemGiveToNpc,
				ItemGiveToPlayer,
				ItemGiveToCreature,
				ItemGiveToWorldItem,
				ItemSteal,
				ItemStealFail,
				ItemLose,
				ItemUse,
				ItemInteract,
				ItemAddToInventory,
				ItemRemoveFromInventory,
				ItemTrigger,
				ItemRepair,
				ItemDisable,
				ItemCraft,
				ItemCraftFail,
				ItemCraftImprove,
				ItemCraftRefine,
				ItemCraftRepair,
				//	Paths
				PathMarkerReveal,
				PathMarkerVisit,
				PathCreate,
				PathAlter,
				PathStartFollow,
				PathStopFollow,
				//	Travel
				FastTravelStart,
				FastTravelStop,
				FastTravelCancel,
				FastTravelInterrupt,
				FastTravelChangeSpeed,
				FastTravelChooseRoute,
				//	Missions
				MissionActivate,
				MissionComplete,
				MissionIgnore,
				MissionFail,
				MissionObjectiveActiveate,
				MissionObjectiveComplete,
				MissionObjectiveFail,
				MissionObjectiveIgnore,
				//	Books
				BookAquire,
				BookRead,
				//	Tools
				ToolUse,
				ToolUseFail,
				ToolUseFinish,
				//	Control
				ControlHijack,
				ControlRestore,
				//	Food
				SurvivalFoodEat,
				SurvivalFoodDrink,
				//	Add-ons
				//TODO look into a way to re-organize these
				//without totally boning the serialized enum values in Unity objects
				PathEncounterObstruction,
				ItemQuestItemAddToInventory,
				NpcConverseExchange,
				ItemQuestItemDie,
				NpcSpeechStart,
				NpcSpeechFinish,
				ItemAQIChange,
				SkillUseFinish,
				SurvivalCreatureDenEnter,
				SurvivalCreatureDenExit,
				MissionVariableChange,
				ItemCurrencyExchange,
				NpcReputationGain,
				NpcReputationLose,
				NpcReputationChange,
				SurvivalHostileAggro,
				SurvivalHostileDeaggro,
				SurvivalDespawn,
				MissionUpdated,
				ItemQuestItemSetState,
				TriggerWorldTrigger,
				NpcFocus,
				ItemACIChange,
		}

		public enum BarterContainerMode
		{
				Goods,
				Offer
		}

		public enum BarterParty
		{
				Player,
				Character
		}

		[Serializable]
		public enum SkillBroadcastResultTime
		{
				OnUseStart,
				OnUseFinish,
				OnCooldownEnd,
		}

		[Serializable]
		public enum SkillUsageType
		{
				Once,
				Duration,
				Manual,
		}

		[Serializable]
		public enum SkillRollType
		{
				Success,
				Failure,
				CriticalFailure,
				CriticalSuccess,
		}

		[Serializable]
		public enum SkillKnowledgeState
		{
				Unknown,
				Known,
				Learned,
				Enabled,
		}

		[Serializable]
		public enum SkillUse
		{
				Automatic,
				Situational,
				Manual,
		}

		[Serializable]
		public enum SkillType
		{
				Guild,
				Magic,
				Crafting,
				Survival,
				Obex,
		}

		[Serializable]
		public enum SkillEffect
		{
				None,
				Increase,
				Decrease,
		}

		[Serializable]
		public enum CutsceneState
		{
				NotStarted,
				Starting,
				Idling,
				Finishing,
				Finished,
		}

		[Serializable]
		public enum LiveTargetType
		{
				None,
				Player,
				Character,
				Mission,
				Conversation,
		}

		[Serializable]
		public enum ExchangeOutgoingStyle
		{
				Normal,
				SiblingsOff,
				ManualOnly,
				Stop,
		}

		[Serializable]
		public enum ExchangeAction
		{
				Choose,
				Conclude,
				Both,
				None,
		}

		[Serializable]
		public enum CreatureOtherType
		{
				Player,
				SameKind,
				DifferentKind,
				AnyCreature,
				AnyOther,
				SpecificKind
		}

		[Serializable]
		public enum MotileGoToMethod
		{
				Pathfinding,
				StraightShot,
				UseDefault,
		}

		[Serializable]
		public enum CreatureBehaviorType
		{
				FleeOther,
				AttackOther,
				FollowOther,
				Freeze,
				Ignore,
		}

		[Flags]
		[Serializable]
		public enum ItemOfInterestType
		{
				None = 0,
				Player = 1,
				WorldItem = 2,
				ActionNode = 4,
				Scenery = 8,
				Light = 16,
				Fire = 32,
				All = Player | WorldItem | ActionNode | Scenery | Light | Fire
		}

		[Flags]
		[Serializable]
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

		[Serializable]
		public enum ShortTermMemoryLength
		{
				Short,
				Medium,
				Long,
		}

		[Flags]
		[Serializable]
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

		[Serializable]
		public enum GrudgeLengthType
		{
				None,
				Awhile,
				Forever
		}

		[Serializable]
		public enum AwarnessDistanceType
		{
				Poor,
				Fair,
				Good,
				Excellent,
				Prescient,
		}

		[Serializable]
		public enum BehaviorType
		{
				ForgetAfterTime,
				ForgetAfterAnyAction,
				ForgetAfterSpecificAction,
		}

		[Serializable]
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

		[Serializable]
		public enum CreatureType
		{
				LandHerbivore,
				LandCarnivore,
				WaterHerbivore,
				WaterCarnivore,
		}

		[Serializable]
		public enum AttitudeType
		{
				Curious,
				Skittish,
				Indiffierent,
				Frightened,
				Hostile,
		}

		[Serializable]
		public enum AwarenessLevelType
		{
				Unaware,
				Relaxed,
				Alarmed,
				Threatened,
		}

		[Serializable]
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

		[Serializable]
		public enum TriggerBehavior
		{
				Once,
				Toggle
		}

		[Serializable]
		public enum ConfirmationBehavior
		{
				Never,
				Once,
				Always,
		}

		[Serializable]
		public enum TriggerEvent
		{
				TriggerStart,
				TriggerCancel,
				TriggerComplete,
				TriggerFail,
		}

		[Serializable]
		public enum ButtonStyle
		{
				ReflectStateLiteral,
				ReflectStateToggle,
				Springy,
				Permanent,
		}

		[Serializable]
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

		[Serializable]
		public enum MotileYieldBehavior
		{
				YieldAndFinish,
				YieldAndWait,
				DoNotYield
		}

		[Serializable]
		public enum ChangeVariableType
		{
				Increment,
				Decrement,
				SetValue
		}

		[Serializable]
		public enum MotileExpiration
		{
				Duration,
				TargetInRange,
				TargetOutOfRange,
				Never,
				NextNightfall,
				NextDaybreak,
		}

		[Serializable]
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

		[Serializable]
		public enum MotileInstructions
		{
				None,
				CompanionInstructions,
				PilgrimInstructions,
				InheritFromBase,
		}

		[Serializable]
		public enum MotileFollowType
		{
				Stalker,
				Companion,
				Follower,
				Attacker,
		}

		[Serializable]
		public enum MotileActionPriority
		{
				Normal,
				ForceTop,
				ForceBase,
				Next,
		}

		[Serializable]
		public enum FollowPathMode
		{
				None,
				FollowingPath,
				WaitingForObstruction,
				ReachedEndOfPath,
				ReachedPilgrimStop,
				MovingToPilgrimStop,
		}

		[Serializable]
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
		[Serializable]
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
		[Serializable]
		public enum BodyOrientation
		{
				None = 0,
				Left = 1,
				Right = 2,
				Both = Left | Right,
		}

		[Flags]
		[Serializable]
		//TODO get rid of this, replace with world flag
		public enum CharacterGender
		{
				None = 0,
				Male = 1,
				Female = 2,
		}

		[Serializable]
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
		[Serializable]
		public enum CharacterEyeColor
		{
				None = 0,
				Black = 1,
				Brown = 2,
				DarkBrown = 4,
				Gray = 8,
				Grey = 8,
				Green = 16,
				LightBrown = 32,
				Purple = 64,
				Silver = 128,
				Blue = 256,
				LightBlue = 512,
		}

		[Flags]
		[Serializable]
		public enum CharacterHeight
		{
				Short = 1,
				BelowAverage	= 2,
				Average = 4,
				AboveAverage	= 8,
				Tall = 16
		}

		[Flags]
		[Serializable]
		public enum CharacterEyeState
		{
				Healthy = 1,
				Bloodshot = 2,
				Darkrot = 4,
				Blackened = 8,
		}

		[Flags]
		[Serializable]
		public enum CharacterGeneralAge
		{
				ChildToTeens = 1,
				TwentiesToThirties	= 2,
				FortiesToFifties	= 4,
				SixtiesToSeventies	= 8,
				Ancient = 16,
		}

		[Flags]
		[Serializable]
		public enum CharacterEthnicity
		{
				None = 0,
				Caucasian = 1,
				BlackCarribean	= 2,
				HanChinese = 4,
				EastIndian = 8,
		}

		[Flags]
		[Serializable]
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
		[Serializable]
		public enum CharacterHairLength
		{
				None = 0,
				Long = 1,
				Short = 2,
		}

		[Flags]
		[Serializable]
		public enum CharacterHairColor
		{
				None = 0,
				Gray = 1,
				Grey = 1,
				Black = 2,
				Brown = 4,
				Blonde = 8,
				Red = 16,
		}

		[Flags]
		[Serializable]
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

		[Serializable]
		public enum CharacterElementType
		{
				BodyMesh,
				Texture,
				HairMesh,
		}

		[Serializable]
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

		[Serializable]
		public enum LocationTerrainType
		{
				AboveGround,
				BelowGround,
				Transition,
		}

		[Serializable]
		public enum PlayerStatusOverTimeMethod
		{
				RapidPulse,
				SlowPulse,
				Continuous,
		}

		[Serializable]
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

		[Serializable]
		public enum HallucinogenicStrength
		{
				None,
				Mild,
				Moderate,
				Strong,
		}

		[Serializable]
		public enum PoisonStrength
		{
				None,
				Mild,
				Moderate,
				Strong,
				Deadly
		}

		[Serializable]
		public enum PlayerStatusRestore
		{
				A_None,
				B_OneFifth,
				C_TwoFifths,
				D_ThreeFifths,
				E_FourFifths,
				F_Full,
		}

		[Serializable]
		public enum WISize
		{
				Tiny,
				Small,
				Medium,
				Large,
				Huge,
				NoLimit,
		}

		[Serializable]
		public enum ItemWeight
		{
				Weightless,
				Light,
				Medium,
				Heavy,
				Unliftable
		}

		[Serializable]
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

		[Serializable]
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

		[Serializable]
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

		[Serializable]
		public enum WorldItemAction
		{
				ChangeMode,
				TakeDamage,
				KilledByDamage,
				ChangeName,
		}

		[Serializable]
		public enum Immersion
		{
				None,
				Immersed,
				FullyImmersed,
				UnderLiquid
		}

		[Serializable]
		public enum PlayerGender
		{
				Male,
				Female,
				None,
		}

		[Serializable]
		public enum PlayerIllnessType
		{
				None,
				Cold,
				Flu,
				Pnemonia,
				FoodPoisoning,
				Dysentary
		}

		[Serializable]
		public enum PlayerStatusInfluence
		{
				Sunlight,
				ColdAmbientTemperature,
				WarmAmbientTemperature,
				ColdImmersion,
				WarmImmersion,
		}

		[Serializable]
		public enum MissionStatusOperator
		{
				None,
				And,
				Or,
				Not,
		}

		[Serializable]
		public enum InjuryType
		{
				None,
				Burn,
				Cut,
				Blunt,
				Sprain,
				Break
		}

		[Serializable]
		public enum InjuryDressing
		{
				None,
				Bandage,
				Turniquet,
				Cauterize
		}

		[Serializable]
		public enum InjuryComplication
		{
				None,
				Gangrenous,
				Infected,
				Poisoned
		}

		[Serializable]
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
		[Serializable]
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
		[Serializable]
		public enum GooNodeFlags : int
		{
				None = 0,
				Solid = 1,
				Liquid = 2,
				Gas = 4,
				BlobContainer = 8
		}

		[Serializable]
		public enum CycleInterpolation
		{
				EaseInOutSmooth,
		}

		[Flags]
		[Serializable]
		public enum NodeRelation : int
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

		[Serializable]
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

		[Serializable]
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
		[Serializable]
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
		[Serializable]
		public enum WIActiveState
		{
				Invisible = 0,
				Visible = 1,
				Active = 2,
		}

		[Flags]
		[Serializable]
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
		[Serializable]
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
		[Serializable]
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
		[Serializable]
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

		[Serializable]
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

		[Serializable]
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
		[Serializable]
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
		[Serializable]
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

		[Serializable]
		public enum DialogResult
		{
				None,
				Yes,
				No,
				Cancel
		}

		[Serializable]
		public enum PathDirection : int
		{
				None = 0,
				Backwards = -1,
				Forward = 1,
		}

		[Serializable]
		public enum PathType : int
		{
				None	= 0,
				Path	= 1,
				Trail	= 2,
				Track	= 3,
				Byway	= 4,
				Road	= 5,
		}

		[Serializable]
		public enum PathUsage : int
		{
				Never = 0,
				AlmostNever = 1,
				Seldom = 2,
				Occasionally = 3,
				Often = 4,
				Frequently = 5,
		}

		[Serializable]
		public enum PathDifficulty : int
		{
				None = 0,
				Easy = 1,
				Moderate = 2,
				Difficult = 3,
				Deadly = 4,
				Impassable = 5,
		}

		[Serializable]
		public enum WorldStructureObjectType
		{
				OuterEntrance,
				InnerEntrance,
				Secret,
				Trap,
				Room,
				Machine,
		}

		[Serializable]
		public enum NGUIPrefab
		{
				None,
				Standard,
				Custom,
		}

		[Serializable]
		public enum NGUIElement
		{
				Panel,
				Sprite,
				SlicedSprite,
				TiledSprite,
				Label,
		}

		[Serializable]
		public enum NGUIFunction
		{
				None,
				Button,
		}

		[Serializable]
		public enum InterfaceType
		{
				Secondary,
				Primary,
				Base,
		}

		public enum GainedSomethingType
		{
		MissionItem,

				None = 0,
				Skill,
				Condition,
				Credential,
				Mission,
				Structure,
				Blueprint,
				Book,
				Currency,
				Money,
				QuestItem,
		}

		public enum SquareDisplayMode
		{
				Empty,
				Enabled,
				Disabled,
				Error,
				Success,
				GlassCase,
				SoldOut,
		}

		[Serializable]
		public enum ActionNodeEventType
		{
				SendSpeechToOccupant,
				SendMessageToOccupant,
				ChangeMissionVariable,
				ChangeConversationVariable,
				SpawnFX,
		}

		[Serializable]
		public enum ActionNodeSpeech
		{
				None,
				CustomCharOnly,
				CustomAnyone,
				SequenceCharOnly,
				RandomCharOnly,
				RandomAnyone,
		}

		[Serializable]
		public enum ActionNodeUsers
		{
				AnyOccupant,
				SpecifiedOccupantOnly,
				PlayerOnly,
		}

		[Serializable]
		public enum ActionNodeType
		{
				Generic,
				DailyRoutine,
				QuestNode,
				PlayerSpawn,
				StructureOwnerSpawn,
		}

		[Flags]
		[Serializable]
		public enum ActionNodeBehavior
		{
				None = 0,
				OnTriggerEnter = 1,
				OnTriggerExit = 2,
				OnOccupy = 4,
				OnVacate = 8,
				OnFinishSpeech = 16,
		}

		[Serializable]
		public enum BookSealStatus
		{
				None,
				Sealed,
				Broken,
		}

		[Serializable]
		[Flags]
		public enum BookStatus
		{
				None = 0,
				Dormant = 1,
				Received = 2,
				PartlyRead = 4,
				FullyRead = 8,
				Archived = 16,
				Read = FullyRead | PartlyRead,
		}

		[Serializable]
		[Flags]
		public enum BookType
		{
				None = 0,
				Book = 1,
				Diary = 2,
				Scripture = 3,
				Envelope = 4,
				Parchment = 5,
				PidgeonMessage = 6,
				Scrap = 7,
				Scroll = 8,
				Map = 9
		}

		[Serializable]
		public enum FireType
		{
				CampFire,
				Candle,
				Fire,
				OilFire,
				OilFireWindy,
				OilLeak,
				Fireplace,
		}

		[Serializable]
		public enum PathMarkerSize
		{
				Path,
				Street,
				Road,
		}

		[Flags]
		[Serializable]
		public enum PathMarkerType
		{
				None = 0,
				Marker = 1,
				Cross = 2,
				Location = 4,
				Campsite = 8,
				Path = 16,
				Street = 32,
				Road = 64,
				Landmark = 128,
				PathMarker = Marker | Path,
				CrossMarker = Cross | Path,
				StreetMarker = Marker | Street,
				CrossStreet = Cross | Street,
				RoadMarker = Marker | Road,
				CrossRoads = Cross | Road,
				PathOrigin = Cross | Campsite | Location | Landmark,
		}

		[Serializable]
		public enum OceanMode
		{
				Default,
				Full,
				Partial,
				Disabled,
		}

		[Flags]
		[Serializable]
		public enum StructureDestroyedBehavior
		{
				None = 0,
				Ignite = 1,
				Destroy = 2,
				Unfreeze = 4,
				Freeze = 8,
				IgniteAndUnfreeze = 16,
		}

		[Serializable]
		public enum StructureMode
		{
				Normal,
				Destroyed,
				Burning,
		}

		public enum StructureBuildMethod
		{
				MeshCombiner,
				MeshInstances,
		}

		[Serializable]
		public enum TriggerRequireType
		{
				None,
				RequireTriggered,
				RequireNotTriggered,
		}

		[Serializable]
		public enum MissionRequireType
		{
				None,
				RequireDormant,
				RequireActive,
				RequireActiveAndNotFailed,
				RequireCompleted,
				RequireNotCompleted,
		}

		[Flags]
		[Serializable]
		public enum WorldTriggerTarget
		{
				None = 0,
				Player = 1,
				Character = 2,
				Creature = 4,
				QuestItem = 8,
				WorldItem = 16,
		}

		[Serializable]
		public enum AvailabilityBehavior
		{
				Always,
				Once,
				Max
		}

		[Flags]
		[Serializable]
		public enum WIRarity
		{
				Rarity,
				Common = 1,
				Uncommon = 2,
				Rare = 4,
				Exclusive = 8,
				FlagsAll = Common | Uncommon | Rare | Exclusive,
		}

		public enum CharacterMovementMode
		{
				//TODO replace this with tags
				None,
				Sitting,
				Standing,
				Walking,
				Sprinting,
				Sneaking,
				Dancing,
				Dead,
				Talking,
				Attacking,
		}

		[Serializable]
		public enum CharacterWeaponMode
		{
				BareHands,
				SlashingWeapon,
				BowBasedWeapon,
				RangedWeapon,
		}

		[Serializable]
		public enum StatusSeekType
		{
				Positive,
				Negative,
				Neutral
		}

		[Serializable]
		public enum DailyRoutineBehavior
		{
				StayAndPlayGoalAnimation,
				GoToNextImmediately,
				WanderAround,
		}

		[Serializable]
		public enum DailyRoutineGoal
		{
				InheritPrior,
				RandomActionNode,
				SpecificActionNode,
				CharacterSpecificActionNode,
		}

		[Serializable]
		public enum OnNodeUnavailableBehavior
		{
				WaitUntilNextGoal,
				FindNearestSubstitute,
				GoToNextGoalImmediately,
		}

		public enum EquippableType
		{
				Weapon,
				Shield,
				Scabbard,
		}

		[Flags]
		[Serializable]
		public enum HostileMode
		{
				None = 0,
				Stalking = 1,
				Warning = 2,
				Attacking = 4,
				CoolingOff = 8,
				Dormant = 16
		}

		[Serializable]
		public enum HunterMode
		{
				Looking,
				Hunting,
				Gathering,
		}

		[Serializable]
		public enum GatherMode
		{
				KillHostile,
				AddToStackContainer,
		}

		[Serializable]
		public enum WorldItemInventoryType
		{
				Container,
				Character,
				Creature,
		}

		[Serializable]
		public enum MotileTerritoryType
		{
				None,
				Den,
		}

		[Serializable]
		public enum BakedGoodStyle
		{
				LoafOfBread,
				FrostedCake,
				Cheesecake,
				Cookie,
				Pie,
				FrostedCakeWithToppings,
				CheesecakeWithToppings,
		}

		[Flags]
		[Serializable]
		public enum FoodStuffEdibleType
		{
				None = 0,
				Edible = 1,
				Poisonous = 16,
				Hallucinogen = 32,
				Medicinal = 64,
				WellFed = 128,
		}

		[Serializable]
		public enum PreparedFoodType
		{
				PlateOrBowl,
				BakedGoods
		}

		[Serializable]
		public enum PreparedFoodStyle
		{
				PlateIngredients,
				PlateMound,
				PlateMoundToppings,
				BowlIngredients,
				BowlFlat,
				BowlFlatToppings,
				BowlMound,
				BowlMoundToppings,
		}

		[Serializable]
		public enum MapIconStyle
		{
				None,
				Small,
				Medium,
				Large,
				AlwaysVisible,
		}

		[Serializable]
		public enum MapLabelStyle
		{
				None,
				MouseOver,
				Descriptive,
				AlwaysVisible
		}

		[Serializable]
		public enum MissionActivatorType
		{
				Mission,
				Objective
		}

		[Serializable]
		public enum PropertyStatusType
		{
				Abandoned,
				ForSale,
				OwnedByCharacter,
				OwnedByPlayer,
				Destroyed,
				DestroyedForever,
				ReposessedByMoneylender,
		}

		[Flags]
		[Serializable]
		public enum StructureLoadState
		{
				None = 0,
				ExteriorUnloaded = 1,
				ExteriorWaitingToLoad = 2,
				ExteriorLoading = 4,
				ExteriorLoaded = 8,
				//aka interior unloaded
				InteriorWaitingToLoad = 16,
				InteriorLoading = 32,
				InteriorLoaded = 64,
				InteriorWaitingToUnload = 128,
				InteriorUnloading = 256,
				ExteriorWaitingToUnload = 512,
				ExteriorUnloading = 1024
		}

		[Serializable]
		public enum StructureLoadPriority
		{
				SpawnPoint = 0,
				Immediate = 1,
				Adjascent = 2,
				Distant = 3,
		}

		[Serializable]
		public enum EntranceState
		{
				Open,
				Opening,
				Closed,
				Closing,
		}

		[Serializable]
		public enum WICurrencyType
		{
				None,
				A_Bronze,
				B_Silver,
				C_Gold,
				D_Luminite,
				E_Warlock,
		}

		[Serializable]
		public enum DamageableResult
		{
				None,
				Break,
				Die,
				State,
				RemoveFromGame,
		}

		[Flags]
		[Serializable]
		public enum ContainerFillTime
		{
				None = 0,
				OnVisible = 1,
				OnOpen = 2,
				OnTrigger = 4,
				OnAddToPlayerInventory = 8,
				OnDie = 16,
		}

		[Serializable]
		public enum ContainerDuplicateTolerance
		{
				Low,
				Moderate,
				High
		}

		[Serializable]
		public enum ContainerFillInterval
		{
				Once,
				Hourly,
				Daily,
				Weekly,
				Monthly
		}

		[Serializable]
		public enum ContainerFillRandomness
		{
				Slight,
				Moderate,
				Extreme,
		}

		[Serializable]
		public enum ContainerFillMethod
		{
				AllRandomItemsFromCategory,
				OneRandomItemFromCategory,
				SpecificItems,
		}

		[Serializable]
		public enum IgnitionProbability
		{
				Impossible,
				Low,
				Moderate,
				High,
				Extreme,
		}

		[Serializable]
		public enum TrapMode
		{
				Set,
				Triggered,
				Misfired,
				Disabled,
		}

		[Serializable]
		public enum PlacementOrientation
		{
				Surface,
				InvertedSurface,
				Gravity,
				InvertedGravity,
				Random
		}

		[Serializable]
		public enum ReceptacleVisualStyle
		{
				Projector,
				GeneralDoppleganger,
				SpecificDoppleganger,
		}

		[Serializable]
		public enum RefineResultType
		{
				SetState,
				Replace
		}

		[Serializable]
		public enum MapDirection
		{
				A_North = 0,
				B_NorthEast = 45,
				C_East = 90,
				D_SEast = 135,
				E_South = 180,
				F_SouthWest = 225,
				G_West = 270,
				H_NorthWest = 315,
				I_None = 360,
		}

		[Serializable]
		public enum HeadstoneStyle
		{
				Headstone,
				StoneCross,
				Marker,
				WoodCross,
		}

		[Serializable]
		public enum PlatformState
		{
				Up,
				GoingUp,
				Down,
				GoingDown,
		}

		[Serializable]
		public enum ArtifactQuality
		{
				None = 0,
				VeryPoor = 1,
				Poor = 2,
				Fair = 3,
				Good = 4,
				VeryGood = 5,
				Excellent = 6,
				Perfect = 7
		}

		[Serializable]
		public enum ArtifactAge
		{
				Recent,
				Modern,
				Old,
				Antiquated,
				Ancient,
				Prehistoric,
		}

		[Serializable]
		public enum SigilType
		{
				BannerFourItems,
				BannerTwoItems,
				RoundShield,
		}

		[Serializable]
		public enum ContainerType
		{
				//TODO find a better way to restrict options
				PersonalEffects,
				ShopGoods,
		}

		[Serializable]
		public enum SpawnerPlacementMethod
		{
				TopDown,
				SherePoint,
				SpawnPoint,
		}

		[Serializable]
		public enum SpawnerType
		{
				WorldItems,
				Creatures,
				Critters,
				Characters,
		}

		[Serializable]
		public enum SpawnerAvailability
		{
				Always,
				Once,
				Max
		}

		[Serializable]
		public enum PlayerMountType
		{
				Air,
				Water,
				Ground,
		}

		[Serializable]
		public enum WearableStyle
		{
				//TODO replace with something human readable
				A,
				B,
				C,
				D,
				E,
				F,
				G,
				H
		}

		[Serializable]
		public enum WearableType
		{
				Armor = 1,
				Clothing = 2,
				Container = 4,
				Jewelry = 8,
		}

		[Serializable]
		public enum WearableMethod
		{
				Rigid,
				Skinned,
				Invisible,
		}

		[Serializable]
		public enum IOIReaction
		{
				IgnoreIt,
				WatchIt,
				FollowIt,
				EatIt,
				KillIt,
				FleeFromIt,
				MateWithIt,
		}

		[Serializable]
		public enum FightOrFlight
		{
				Fight,
				Flee,
		}

		[Serializable]
		public enum DomesticatedState
		{
				Wild,
				Domesticated,
				Tamed,
				Custom,
		}

		[Serializable]
		public enum HudElementType
		{
				Label,
				ProgressBar,
				Icon,
		}

		[Serializable]
		public enum PassThroughTriggerType
		{
				Inner,
				Outer,
		}

		[Serializable]
		public enum PassThroughState
		{
				InnerOn_OuterOff,
				InnerOn_OuterOn,
				InnerOff_OuterOn,
				InnerOff_OuterOff,
		}

		[Flags]
		[Serializable]
		public enum HudActiveType
		{
				None = 0,
				OnWorldMode = 1,
				OnDeadMode = 2,
				OnPlayerFocus = 4,
				OnPlayerAttention	= 8,
				All = OnWorldMode | OnDeadMode | OnPlayerFocus | OnPlayerAttention,
		}

		public enum MusicVolume
		{
				Default,
				Quiet,
		}

		public enum MusicType
		{
				None,
				MainMenu,
				Cutscene,
				Regional,
				Night,
				Underground,
				SafeLocation,
				Combat
		}

		public enum TemperatureComparison
		{
				Warmer,
				Colder,
				Same
		}

		[Serializable]
		public enum TemperatureRange : int
		{
				A_DeadlyCold = 0,
				B_Cold = 1,
				C_Warm = 2,
				D_Hot = 3,
				E_DeadlyHot = 4,
		}

		[Flags]
		[Serializable]
		public enum BlueprintStrictness
		{
				None = 0,
				StackName = 1,
				PrefabName = 2,
				StateName = 4,
				Subcategory = 8,
				Default = StackName,
		}

		[Serializable]
		public enum BlueprintRevealMethod
		{
				None,
				Book,
				Character,
				ReverseEngineer,
				Skill,
		}

		[Serializable]
		public enum CraftingType
		{
				//TODO replace this
				Craft,
				Brew,
				Refine,
				Cook,
		}

		[Serializable]
		public enum CharacterTemplateType
		{
				Generic,
				UniquePrimary,
				UniqueAlternate,
		}

		[Serializable]
		public enum ExplosionType
		{
				Base,
				Chunks,
				Crazysparks,
				Ignitor,
				Insanity,
				Multi,
				MushroomCloud,
				Simple,
				Spray,
				Tiny,
				Upwards,
				Wide,
				ExplosionArms,
		}

		[Serializable]
		public enum VariableCheckType
		{
				LessThanOrEqualTo,
				LessThan,
				GreaterThanOrEqualTo,
				GreaterThan,
				EqualTo,
		}

		public enum DataCompression
		{
				None,
				GZip,
		}

		public enum DataType
		{
				Base,
				World,
				Profile
		}

		[Serializable]
		[Flags]
		public enum FGameState
		{
				Startup = 1,
				WaitingForGame = 2,
				GameLoading = 4,
				GameStarting = 8,
				GamePaused = 16,
				InGame = 32,
				Cutscene = 64,
				Saving = 128,
				Quitting = 256,
				Unloading = 512,
		}

		[Serializable]
		public enum NClientState
		{
				None = 0,
				Disconnected = 1,
				WaitingToConnect = 2,
				Connected = 3,
				Paused = 4,
		}

		[Serializable]
		public enum NHostState
		{
				None = 0,
				Disconnected = 1,
				WaitingToStart = 2,
				Started = 3,
				Paused = 4,
		}

		[Serializable]
		public enum WorldLightType
		{
				Exterior,
				InteriorOrUnderground,
				Equipped,
				AlwaysOn,
		}

		[Serializable]
		public enum MissionCompletion
		{
				Automatic,
				Manual,
		}

		[Flags]
		public enum MissionStatus
		{
				Dormant = 1,
				Active = 2,
				Completed = 4,
				Ignored = 8,
				Failed = 16,
		}

		[Serializable]
		public enum ObjectiveType
		{
				Required,
				RequiredOnceActive,
				Optional,
		}

		[Serializable]
		public enum ObjectiveActivation
		{
				AutomaticOnMissionActivation,
				AutomaticOnPreviousCompletion,
				Manual,
		}

		[Serializable]
		public enum ObjectiveBehavior
		{
				Permanent,
				//once you succeed or fail, you can't revert
				Toggle,
				//you can toggle from failure to success
		}

		[Serializable]
		public enum MissionOriginType
		{
				None,
				Character,
				Encounter,
				Book,
				Mission,
				Introspection,
				Location,
		}

		[Serializable]
		public enum FallDamageStyle
		{
				None,
				Forgiving,
				Realistic,
		}

		[Serializable]
		public enum DifficultyDeathStyle
		{
				Blackout,
				//respawn in the nearest respawn structure
				PermaDeath,
				//hardcore mode
				NoDeath,
				Respawn = Blackout,
		}

		[Serializable]
		public enum PrototypeTemplateType
		{
				DetailTexture,
				DetailMesh,
				TreeMesh,
		}

		[Serializable]
		public enum MarkerAlterAction
		{
				None,
				AppendToPath,
				CreatePath,
				CreatePathAndBranch,
				CreateBranch,
		}

		[Serializable]
		public enum PlantRootType
		{
				ThinFibrous = 0,
				TypicalBranched = 1,
				ThickTaproot = 2,
		}

		[Serializable]
		public enum PlantFlowerSize
		{
				Tiny = 1,
				Small = 2,
				Medium = 3,
				Large = 4,
				Giant = 5,
		}

		[Serializable]
		public enum PlantBodyHeight
		{
				ExtraShort = 1,
				Short = 2,
				Medium = 3,
				Tall = 4,
				ExtraTall = 5,
		}

		[Serializable]
		public enum PlantRootSize
		{
				Small = 1,
				Medium = 2,
				Large = 3,
		}

		[Serializable]
		public enum ElevationType
		{
				Low,
				Medium,
				High,
		}

		[Serializable]
		public enum ClimateType
		{
				Arctic,
				Desert,
				Rainforest,
				Temperate,
				TropicalCoast,
				Wetland,
		}

		[Serializable]
		public enum PlayerAvatarState
		{
				Base,
				Climb,
				Swim,
				Fly,
				Combat,
				PhysX
		}

		[Serializable]
		public enum PlayerHijackMode
		{
				LookAtTarget,
				OrientToTarget,
		}

		public enum FXType
		{
				//TODO get rid of this
				None,
				FireParticles,
				DamageOverlay,
				MildDistortion,
				StrongDistortion,
				Vignette,
				Blur,
		}

		[Serializable]
		public enum TerrainType
		{
				Coastal,
				Civilization,
				OpenField,
				LightForest,
				DeepForest,
		}

		[Serializable]
		public enum GroundType
		{
				Dirt,
				Leaves,
				Metal,
				Mud,
				Snow,
				Stone,
				Water,
				Wood,
		}

		[Serializable]
		public enum ToolAction
		{
				None,
				Equip,
				Unequip,
				UseStart,
				UseHold,
				UseRelease,
				UseFinish,
				Reload,
				Throw,
				CycleNext,
				CyclePrev,
		}

		[Serializable]
		public enum PlayerToolType
		{
				Generic,
				GenericUsable,
				PathEditor,
				CustomAction,
		}

		[Serializable]
		public enum PlayerToolState
		{
				Equipping,
				Equipped,
				CancelEquip,
				Unequipping,
				Unequipped,
				LoadingProjectile,
				LaunchingProjectile,
				IncreasingForce,
				ReleasingForce,
				InMotion,
				InUse,
				Cycling,
		}

		[Serializable]
		public enum PlayerToolStyle
		{
				Swing,
				Slice,
				TensionRelease,
				ProjectileLaunch,
				Static,
		}

		[Flags]
		[Serializable]
		public enum ProfileComponents
		{
				None = 0,
				Profile = 1,
				Game = 2,
				Character = 4,
				Preferences = 8,
				All = Profile | Game | Character | Preferences
		}

		[Serializable]
		public enum WIStackError
		{
				None,
				IsFull,
				TooLarge,
				NotCompatible,
				InvalidOperation,
		}

		[Serializable]
		public enum StackPushMode
		{
				Auto,
				Manual
		}

		[Serializable]
		public enum WIStackMode
		{
				Generic,
				Enabler,
				Wearable,
				Receptacle,
		}

		public enum FastTravelState
		{
				None,
				ArrivingAtDestination,
				WaitingForNextChoice,
				Traveling,
				Finished
		}

		public enum GroupSearchType
		{
				LiveOnly,
				LiveThenSaved,
				SavedOnly
		}

		[Serializable]
		public enum ObjectiveTimeLimit
		{
				None,
				BeforeNextNightfall,
				BeforeNextMorning,
		}

		[Serializable]
		public enum TimeUnit
		{
				Hour,
				Day,
				Week,
				Month,
				Year
		}

		[Serializable]
		public enum GroupLookupType
		{
				Location,
				QuestItem,
				Character,
		}

		[Flags]
		[Serializable]
		public enum SpotlightDirection : int
		{
				None = 0,
				Top = 1,
				Bottom = 2,
				Front = 4,
				Back = 8,
				Left = 16,
				Right = 32,
		}

		[Serializable]
		public enum PassThroughBehavior
		{
				InterceptByFocus,
				InterceptByFilter,
				InterceptBySubscription,
				PassThrough,
				InterceptAll,
		}
}
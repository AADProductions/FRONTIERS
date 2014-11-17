using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class AvatarActionReceiver : ActionFilter <PlayerAvatarAction>
{
	public override void WakeUp ()
	{
		PlayerAvatarAction.LinkTypes ();
		mSubscriptionAdd = new SubscriptionAdd <PlayerAvatarAction> (SubscriptionAdd);
		mSubscriptionCheck = new SubscriptionCheck <PlayerAvatarAction> (SubscriptionCheck);
		mSubscribed = new PlayerAvatarAction (PlayerIDFlag.Local, AvatarActionType.NoAction, AvatarAction.NoAction);
		mSubscribersSet = true;
	}

	public void Subscribe (AvatarAction action, ActionListener listener)
	{
		Subscribe (new PlayerAvatarAction (action), listener);
	}

	public void SubscriptionAdd (ref PlayerAvatarAction subscription, PlayerAvatarAction action)
	{
		if (action.Action != AvatarAction.NoAction) {
			subscription.ActionType |= action.ActionType;
			subscription.PlayerID	|= action.PlayerID;
		}
	}

	public bool SubscriptionCheck (PlayerAvatarAction subscription, PlayerAvatarAction action)
	{
		return Flags.Check <AvatarActionType> (subscription.ActionType, action.ActionType, Flags.CheckType.MatchAny);
		//&& Flags.Check <PlayerIDFlag> (subscription.PlayerID, action.PlayerID, Flags.CheckType.MatchAny));
	}

	public bool ReceiveAction (AvatarAction action, double timeStamp)
	{
		return ReceiveAction (new PlayerAvatarAction (action), timeStamp);
	}

	public override bool ReceiveAction (PlayerAvatarAction action, double timeStamp)
	{
		return base.ReceiveAction (action, timeStamp);
	}
}

[Serializable]
public struct PlayerAvatarAction : IEqualityComparer <PlayerAvatarAction>
{
	public PlayerAvatarAction (PlayerIDFlag playerID, AvatarActionType type, AvatarAction action)
	{
		PlayerID = playerID;
		ActionType = type;
		Action = action;
		Target = null;
	}

	public PlayerAvatarAction (PlayerIDFlag playerID, AvatarActionType type, AvatarAction action, GameObject target)
	{
		PlayerID = playerID;
		ActionType	= type;
		Action = action;
		Target = target;
	}

	public bool Equals (PlayerAvatarAction pa1, PlayerAvatarAction pa2)
	{
		return (pa1.Action == pa2.Action && Flags.Check <PlayerIDFlag> (pa1.PlayerID, pa2.PlayerID, Flags.CheckType.MatchAny));
	}

	public int GetHashCode (PlayerAvatarAction a)
	{
		return (int)a.ActionType + (int)a.Action;
	}

	public PlayerAvatarAction (AvatarAction action, GameObject target)
	{
		PlayerID = PlayerIDFlag.Local;
		if (!gActionTypes.TryGetValue (action, out ActionType)) {
			ActionType = AvatarActionType.NoAction;
			//Debug.Log ("Action " + action + " had no pair");
		}
		Action = action;
		Target = target;
	}

	public PlayerAvatarAction (AvatarAction action)
	{
		PlayerID = PlayerIDFlag.Local;
		if (!gActionTypes.TryGetValue (action, out ActionType)) {
			ActionType = AvatarActionType.NoAction;
			//Debug.Log ("Action " + action + " had no pair");
		}
		Action = action;
		Target = null;
	}

	[BitMask (typeof(PlayerIDFlag))]
	public PlayerIDFlag PlayerID;
	[BitMask (typeof(AvatarActionType))]
	public AvatarActionType ActionType;
	public AvatarAction Action;
	public GameObject Target;

	public static void LinkTypes ()
	{
		if (gActionTypes.Count == 0) {
			List <string> avatarActionTypeNames = new List<string> ();
			avatarActionTypeNames.AddRange (Enum.GetNames (typeof(AvatarActionType)));

			List <string> avatarActionNames = new List<string> ();
			avatarActionNames.AddRange (Enum.GetNames (typeof(AvatarAction)));

			foreach (string avatarActionName in avatarActionNames) {
				foreach (string avatarActionTypeName in avatarActionTypeNames) {
					if (avatarActionName.Contains (avatarActionTypeName)) {
						AvatarAction action = (AvatarAction)Enum.Parse (typeof(AvatarAction), avatarActionName);
						AvatarActionType actionType = (AvatarActionType)Enum.Parse (typeof(AvatarActionType), avatarActionTypeName);
						gActionTypes.Add (action, actionType);
						////Debug.Log ("paired " + avatarActionName + " with " + avatarActionTypeName);
						break;
					}
				}
			}
		}
	}

	public static Dictionary <AvatarAction, AvatarActionType> gActionTypes = new Dictionary <AvatarAction, AvatarActionType> ();
}

[Flags]
public enum PlayerIDFlag : int
{
	Local			= 1,
	Player01		= 2,
	Player02		= 4,
	Player03		= 8,
	Player04		= 16,
	Player05		= 32,
	Player06		= 64,
	Player07		= 128,
	Player08		= 256,
	Player09		= 512,
	Player10		= 1024,
	Player11		= 2048,
	Player12		= 4096,
	Player13		= 8192,
	Player14		= 16384,
	Player15		= 32768,
	Player16		= 65536,
	Player17		= 131072,
	Player18		= 262144,
	Player19		= 524288,
	Player20		= 1048576,
	Player21		= 2097152,
	Player22		= 4194304,
	Player23		= 8388608,
	Player24		= 16777216,
}

[Flags]
public enum AvatarActionType
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

public enum AvatarAction
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
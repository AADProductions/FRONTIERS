using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Frontiers.Data;
using Frontiers.Story;

namespace Frontiers.World.Gameplay
{
	//this XmlInclude bullshit sucks
	//and makes modding really hard
	//TODO swap this bullshit out for the serialization method I use in WorldTrigger
	[Serializable]
	[XmlInclude(typeof(ObjectiveGetItemWithScript))]
	[XmlInclude(typeof(ObjectiveRequireStructureLoadState))]
	[XmlInclude(typeof(ObjectiveRequireTimeElapsed))]
	[XmlInclude(typeof(ObjectiveGetQuestItem))]
	[XmlInclude(typeof(ObjectiveWorldTrigger))]
	[XmlInclude(typeof(ObjectiveSetQuestItemState))]
	[XmlInclude(typeof(ObjectiveDestroyQuestItem))]
	[XmlInclude(typeof(ObjectiveConversationExchange))]
	[XmlInclude(typeof(ObjectiveCharacterReachQuestNode))]
	[XmlInclude(typeof(ObjectiveVisitLocation))]
	[XmlInclude(typeof(ObjectiveLeaveLocation))]
	[XmlInclude(typeof(ObjectiveEnterStructure))]
	[XmlInclude(typeof(ObjectivePreventCharacterDeath))]
	[XmlInclude(typeof(ObjectiveCharacterFinishSpeech))]
	[XmlInclude(typeof(ObjectivePlaceQuestItemInReceptacle))]
	[XmlInclude(typeof(ObjectiveCheckBookStatus))]
	[XmlInclude(typeof(ObjectiveInitiateConversation))]
	[XmlInclude(typeof(ObjectiveCompleteObjective))]
	[XmlInclude(typeof(ObjectiveCompleteObjectives))]
	[XmlInclude(typeof(ObjectiveDestroyQuestItem))]
	[XmlInclude(typeof(ObjectiveMissionVariableCheck))]
	[XmlInclude(typeof(ObjectiveMissionThreeVariableCheck))]
	[XmlInclude(typeof(ObjectivePlayerCurrencyCheck))]
	[XmlInclude(typeof(ObjectiveQuestItemEnterTrigger))]
	[XmlInclude(typeof(ObjectiveStatusKeeperValue))]
	[XmlInclude(typeof(ObjectiveFocusOnCharacter))]
	[XmlInclude(typeof(ObjectiveFocusOnQuestItem))]
	//	[XmlInclude (typeof (ObjectiveGiveQuestItemToCharacter))]
		//	[XmlInclude (typeof (ObjectiveGetQuestItemFromCharacter))]
		//	[XmlInclude (typeof (ObjectiveDestroyQuestCreature))]
		public class ObjectiveScript
	{
		[XmlIgnore]
		[NonSerialized]
		public MissionObjective
			objective = null;

		[XmlIgnore]
		public virtual List <AvatarAction>	Subscriptions {
			get {
				return new List <AvatarAction> ();
			}
		}

		public bool FinishedChecking = false;
		public bool HasCompleted = false;
		public bool RequiresAll = true;
		public MissionStatus Status = MissionStatus.Dormant;

		public virtual void OnActivated ()
		{

		}

		public virtual void CheckIfCompleted ()
		{
			//override functions MUST set FinishedChecking to true
			//at some point or the check will continue indefinitely
			FinishedChecking = true;
		}

		public void Reset ()
		{
			FinishedChecking = false;
			HasCompleted = false;
			Status = MissionStatus.Dormant;
		}
	}

	[Serializable]
	public class ObjectiveRequireStructureLoadState : ObjectiveScript
	{
		public MobileReference LocationReference = new MobileReference ();
		public StructureLoadState LoadState = StructureLoadState.ExteriorLoaded;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.LocationVisit, AvatarAction.LocationLeave };
			}
		}

		public override void CheckIfCompleted ()
		{
			if (mLoadingStackItem) {
				return;
			}

			mLoadingStackItem = true;
			WIGroups.SuperLoadStackItem (LocationReference.GroupPath, LocationReference.FileName, new IWIBaseCallback (FinishChecking));
		}

		public void FinishChecking (IWIBase iwiBase)
		{
			mLoadingStackItem = false;

			if (iwiBase == null) {
				FinishedChecking = true;
				HasCompleted = false;
				return;
			}

			System.Object structureStateObject = null;
			if (iwiBase.GetStateOf <Structure> (out structureStateObject)) {
				StructureState structureState = (StructureState)structureStateObject;
				HasCompleted = Flags.Check ((uint)LoadState, (uint)structureState.LoadState, Flags.CheckType.MatchAll);
				if (HasCompleted) {
					Status |= MissionStatus.Completed;
				}
			}
			FinishedChecking = true;
		}

		protected bool mLoadingStackItem = false;
	}

	[Serializable]
	public class ObjectiveRequireTimeElapsed : ObjectiveScript
	{
		public string MissionName;
		public string VariableName;
		public int InGameHours = 1;
		public bool RequireElapsed = true;
		//this assumes a mission variable was set to the current time in an earlier exchange
		public override void OnActivated ()
		{
			//subscribe to world clock time
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.HourStart, HourStart);
		}

		public bool HourStart (double timeStamp)
		{
			//ask our parent objective to check if completed
			return objective.Subscription (timeStamp);
		}

		public override void CheckIfCompleted ()
		{
			int worldTimeWhenSet = 0;
			if (Missions.Get.MissionVariable (MissionName, VariableName, ref worldTimeWhenSet)) {
				bool hasElapsed = (WorldClock.AdjustedRealTime >= (worldTimeWhenSet + WorldClock.HoursToSeconds (InGameHours)));
				if (RequireElapsed) {
					HasCompleted = hasElapsed;
				} else {
					HasCompleted = !hasElapsed;
				}
				if (HasCompleted) {
					Status |= MissionStatus.Completed;
				}
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveGetItemWithScript : ObjectiveScript
	{
		public string ScriptName = "Container";

		public override List <AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.ItemAddToInventory };
			}
		}

		public override void CheckIfCompleted ()
		{
			List <IWIBase> items = Player.Local.Inventory.LastAddedItems;
			for (int i = 0; i < items.Count; i++) {
				if (items [i] != null && items [i].Is (ScriptName)) {
					HasCompleted = true;
					Status |= MissionStatus.Completed;
					break;
				}
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveSetQuestItemState : ObjectiveScript
	{
		public string ItemName = "Item";
		public string State = "Default";

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.ItemQuestItemSetState };
			}
		}

		public override void CheckIfCompleted ()
		{
			WorldItem questItem = null;
			if (GameWorld.Get.QuestItem (ItemName, out questItem)) {
				if (questItem.State == State) {
					HasCompleted = true;
					Status |= MissionStatus.Completed;
				}
			}	
		}
	}

	[Serializable]
	public class ObjectiveWorldTrigger : ObjectiveScript
	{
		public string TriggerName = "Trigger";
		public int ChunkID = 0;
		public bool IgnorePreviousTriggers = true;
		public int NumTimesTriggeredOnActivated = 0;

		public override List <AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.TriggerWorldTrigger };
			}
		}

		public override void OnActivated ()
		{
			WorldChunk worldChunk = null;
			if (GameWorld.Get.ChunkByID (ChunkID, out worldChunk)) {
				WorldTriggerState worldTriggerState = null;
				if (worldChunk.GetTriggerState (TriggerName, out worldTriggerState)) {
					NumTimesTriggeredOnActivated = worldTriggerState.NumTimesTriggered;
				}
			}
			FinishedChecking = true;
		}

		public override void CheckIfCompleted ()
		{
			WorldChunk worldChunk = null;
			if (GameWorld.Get.ChunkByID (ChunkID, out worldChunk)) {
				WorldTriggerState worldTriggerState = null;
				if (worldChunk.GetTriggerState (TriggerName, out worldTriggerState)) {
					if (IgnorePreviousTriggers) {
						HasCompleted = worldTriggerState.NumTimesTriggered > NumTimesTriggeredOnActivated;
					} else {
						HasCompleted = worldTriggerState.NumTimesTriggered > 0;
					}
				}
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveGetQuestItem : ObjectiveScript
	{
		public string ItemName = "Item";

		public override List <AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.ItemQuestItemAddToInventory };
			}
		}

		public override void CheckIfCompleted ()
		{
			if (Player.Local.Inventory.HasQuestItem (ItemName)) {
				HasCompleted = true;
				Status |= MissionStatus.Completed;
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCompleteObjective : ObjectiveScript
	{
		public string MissionName = string.Empty;
		public string ObjectiveName = string.Empty;
		public bool FailureOK = false;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () {
										AvatarAction.MissionUpdated
								};
			}
		}

		public override void CheckIfCompleted ()
		{
			string missionName = MissionName;
			if (string.IsNullOrEmpty (missionName)) {
				missionName = objective.mission.name;
			}
			bool result = true;
			MissionStatus objectiveStatus = MissionStatus.Dormant;
			//if we're checking our mission do it locally, otherwise do it with the manager
			if (missionName == objective.mission.State.Name) {
				MissionObjective checkObjective = null;
				if (objective.mission.GetObjective (ObjectiveName, out checkObjective)) {
					bool hasCompleted = Flags.Check ((uint)checkObjective.State.Status, (uint)MissionStatus.Completed, Flags.CheckType.MatchAny);
					bool hasFailed = Flags.Check ((uint)checkObjective.State.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny);
					if (!hasCompleted || (hasFailed && !FailureOK)) {
						//Debug.Log ("Objective " + ObjectiveName + " hasn't completed, or it has failed and failure is not okay");
						result = false;
					}
				}
			} else {
				if (Missions.Get.ObjectiveStatusByName (missionName, ObjectiveName, ref objectiveStatus)) {
					bool hasCompleted = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Completed, Flags.CheckType.MatchAny);
					bool hasFailed = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny);
					if (!hasCompleted || (hasFailed && !FailureOK)) {
						result = false;
					}
				}
			}
			HasCompleted = result;
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCompleteObjectives : ObjectiveScript
	{
		public string MissionName = string.Empty;
		//by default it's this mission
		public List <string> ObjectiveNames = new List<string> ();
		public bool FailureOK = false;
		public bool RequireAll = true;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () {
										AvatarAction.MissionObjectiveComplete,
										AvatarAction.MissionObjectiveFail,
										AvatarAction.MissionObjectiveIgnore
								};
			}
		}

		public override void CheckIfCompleted ()
		{
			string missionName = objective.mission.name;
			if (!string.IsNullOrEmpty (missionName)) {
				missionName = MissionName;
			}
			bool result = true;
			if (RequireAll) {
				HasCompleted = CheckRequireAll (missionName);
			} else {
				HasCompleted = CheckRequireOne (missionName);
			}
			FinishedChecking = true;
		}

		protected bool CheckRequireAll (string missionName)
		{
			bool result = true;
			foreach (string objectiveName in ObjectiveNames) {
				MissionStatus objectiveStatus = MissionStatus.Dormant;
				if (Missions.Get.ObjectiveStatusByName (missionName, objectiveName, ref objectiveStatus)) {
					bool hasCompleted = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Completed, Flags.CheckType.MatchAny);
					bool hasFailed = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny);
					if (!hasCompleted || (hasFailed && !FailureOK)) {
						result = false;
						break;
					}
				}
			}
			return result;
		}

		protected bool CheckRequireOne (string missionName)
		{
			bool result = false;
			foreach (string objectiveName in ObjectiveNames) {
				MissionStatus objectiveStatus = MissionStatus.Dormant;
				if (Missions.Get.ObjectiveStatusByName (missionName, objectiveName, ref objectiveStatus)) {
					bool hasCompleted = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Completed, Flags.CheckType.MatchAny);
					bool hasFailed = Flags.Check ((uint)objectiveStatus, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny);
					if (hasCompleted && (!hasFailed || FailureOK)) {
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}

	[Serializable]
	public class ObjectiveMissionVariableCheck : ObjectiveScript
	{
		public string MissionName = string.Empty;
		public string VariableName = string.Empty;
		public int VariableValue = 0;
		public VariableCheckType CheckType = VariableCheckType.EqualTo;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.MissionVariableChange };
			}
		}

		public override void CheckIfCompleted ()
		{
			//Debug.Log ("ObjectiveMissionVariableCheck: Checking if completed in " + objective.mission.name);
			string missionName = objective.mission.name;
			if (!string.IsNullOrEmpty (MissionName)) {
				missionName = MissionName;
			}
			int currentValue = 0;
			if (Missions.Get.MissionVariable (missionName, VariableName, ref currentValue)) {
				HasCompleted = GameData.CheckVariable (CheckType, VariableValue, currentValue);
				Status |= MissionStatus.Completed;
			}
			//Debug.Log ("Result: " + Status.ToString ());
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveMissionThreeVariableCheck : ObjectiveScript
	{
		public string Mission1Name = string.Empty;
		public string Variable1Name = string.Empty;
		public int Variable1Value = 0;
		public VariableCheckType Check1Type = VariableCheckType.EqualTo;
		public string Mission2Name = string.Empty;
		public string Variable2Name = string.Empty;
		public int Variable2Value = 0;
		public VariableCheckType Check2Type = VariableCheckType.EqualTo;
		public string Mission3Name = string.Empty;
		public string Variable3Name = string.Empty;
		public int Variable3Value = 0;
		public VariableCheckType Check3Type = VariableCheckType.EqualTo;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.MissionVariableChange };
			}
		}

		public override void CheckIfCompleted ()
		{
			bool check1 = false;
			bool check2 = false;
			bool check3 = false;

			string missionName = string.Empty;
			int currentValue = 0;
			//check #1
			missionName = objective.mission.name;
			if (!string.IsNullOrEmpty (Mission1Name)) {
				missionName = Mission1Name;
			}
			currentValue = 0;
			if (Missions.Get.MissionVariable (missionName, Variable1Name, ref currentValue)) {
				check1 = GameData.CheckVariable (Check1Type, Variable1Value, currentValue);
			}
			//check #2
			missionName = objective.mission.name;
			if (!string.IsNullOrEmpty (Mission2Name)) {
				missionName = Mission2Name;
			}
			currentValue = 0;
			if (Missions.Get.MissionVariable (missionName, Variable2Name, ref currentValue)) {
				check2 = GameData.CheckVariable (Check2Type, Variable2Value, currentValue);
			}
			//check #3
			missionName = objective.mission.name;
			if (!string.IsNullOrEmpty (Mission3Name)) {
				missionName = Mission3Name;
			}
			currentValue = 0;
			if (Missions.Get.MissionVariable (missionName, Variable3Name, ref currentValue)) {
				check3 = GameData.CheckVariable (Check3Type, Variable3Value, currentValue);
			}

			HasCompleted = (check1 && check2);
			if (HasCompleted) {
				Status |= MissionStatus.Completed;
			}

			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveDestroyQuestItem : ObjectiveScript
	{
		public string ItemName = "Item";

		public override List <AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.ItemQuestItemDie };
			}
		}

		public override void CheckIfCompleted ()
		{
			if (GameWorld.Get.State.DestroyedQuestItems.Contains (ItemName)) {
				HasCompleted = true;
				Status |= MissionStatus.Completed;
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveConversationExchange : ObjectiveScript
	{
		public string ConversationName = "Conversation";
		public string ExchangeName = string.Empty;
		public int ExchangeIndex = 0;
		public bool RequirementsMetIfNotInitiated = false;

		public override List <AvatarAction>	Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.NpcConverseExchange };
			}
		}

		public override void CheckIfCompleted ()
		{
			//Debug.Log ("ObjectiveConversationExchange: Checking if completed in " + objective.mission.name);
			if (string.IsNullOrEmpty (ExchangeName)) {
				ExchangeName = Conversations.Get.ExchangeNameFromIndex (DataImporter.GetNameFromDialogName (ConversationName), ExchangeIndex);
			}
			if (Conversations.Get.HasCompletedExchange (DataImporter.GetNameFromDialogName (ConversationName), ExchangeName, RequirementsMetIfNotInitiated)) {
				HasCompleted = true;
				Status |= MissionStatus.Completed;
			}
			//Debug.Log ("Result: " + Status.ToString ());
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCharacterReachQuestNode : ObjectiveScript
	{
		public string CharacterName = "Character";
		public string QuestNodeName = "QuestNode";
		public bool TimeLimit = false;
		public float LimitFromActivation = 0.0f;

		public override void CheckIfCompleted ()
		{
			float time;
			if (Characters.Get.CharacterHasReachedQuestNode (CharacterName, QuestNodeName, out time)) {
				HasCompleted = true;
				Status |= MissionStatus.Completed;
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveLeaveLocation : ObjectiveScript
	{
		public MobileReference LocationReference = new MobileReference ();
		public bool VisitingLastTimeChecked = false;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.LocationVisit, AvatarAction.LocationLeave };
			}
		}

		public override void OnActivated ()
		{
			if (Player.Local.Surroundings.IsVisiting (LocationReference)) {
				VisitingLastTimeChecked = true;
			} else {
				VisitingLastTimeChecked = false;
			}
		}

		public override void CheckIfCompleted ()
		{
			if (VisitingLastTimeChecked) {
				if (!Player.Local.Surroundings.IsVisiting (LocationReference)) {
					Status |= MissionStatus.Completed;
					HasCompleted = true;
				}
			} else if (Player.Local.Surroundings.IsVisiting (LocationReference)) {
				VisitingLastTimeChecked = true;
			}

			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveVisitLocation : ObjectiveScript
	{
		public MobileReference LocationReference = new MobileReference ();
		public bool CompleteIfVisitingNow = true;
		public bool IgnorePreviousVisits = true;
		public int NumTimesVisitedOnActivation = 0;

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.LocationVisit };
			}
		}

		public override void OnActivated ()
		{
			if (IgnorePreviousVisits) {
				WIGroups.SuperLoadStackItem (LocationReference.GroupPath, LocationReference.FileName, new IWIBaseCallback (FinishCheckingPreviousVisits));
			}
		}

		public void FinishCheckingPreviousVisits (IWIBase iwiBase)
		{
			if (iwiBase == null) {
				//failure! since it's a Location - which should never return null - put out a warning
				Debug.LogError ("WARNING: Location returned null in objective script - this should never happen! looknig for" + LocationReference.FullPath);
			} else if (iwiBase.IsWorldItem) {
				Visitable visitable = null;
				if (iwiBase.worlditem.Is <Visitable> (out visitable)) {
					NumTimesVisitedOnActivation = visitable.State.NumTimesVisited;
				}
			} else {
				object stateData = null;
				if (iwiBase.GetStateOf <Visitable> (out stateData)) {
					VisitableState visitableState = stateData as VisitableState;
					NumTimesVisitedOnActivation = visitableState.NumTimesVisited;
				}
			}
		}

		public override void CheckIfCompleted ()
		{
			if (mLoadingStackItem) {
				return;
			}

			mLoadingStackItem = true;
			WIGroups.SuperLoadStackItem (LocationReference.GroupPath, LocationReference.FileName, new IWIBaseCallback (FinishChecking));
		}

		public void FinishChecking (IWIBase iwiBase)
		{
			mLoadingStackItem = false;

			if (iwiBase == null) {
				//failure! since it's a Location - which should never return null - put out a warning
				Debug.LogError ("WARNING: Location returned null in objective script - this should never happen!");
			} else if (iwiBase.IsWorldItem) {
				Visitable visitable = null;
				if (iwiBase.worlditem.Is <Visitable> (out visitable)) {
					if (IgnorePreviousVisits) {
						HasCompleted = visitable.State.NumTimesVisited > NumTimesVisitedOnActivation;
					} else {
						HasCompleted = visitable.State.NumTimesVisited > 0;
					}
				}
			} else {
				object stateData = null;
				if (iwiBase.GetStateOf <Visitable> (out stateData)) {
					VisitableState visitableState = stateData as VisitableState;
					if (IgnorePreviousVisits) {
						HasCompleted = visitableState.NumTimesVisited > NumTimesVisitedOnActivation;
					} else {
						HasCompleted = visitableState.NumTimesVisited > 0;
					}
				}
			}
			FinishedChecking = true;
		}

		protected bool mLoadingStackItem = false;
	}

	[Serializable]
	public class ObjectiveEnterStructure : ObjectiveScript
	{
		public string StructurePath = string.Empty;

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.LocationStructureEnter, AvatarAction.LocationStructureExit };
			}
		}

		public override void OnActivated ()
		{
			WorldItem structureWorldItem = null;
			StackItem structureStackitem = null;

			if (WIGroups.FindChildItem (StructurePath, out structureWorldItem)) {
				if (Player.Local.Surroundings.IsVisitingStructure (structureWorldItem.Get <Structure> ())) {
					HasCompleted = true;
				}
			}
		}

		public override void CheckIfCompleted ()
		{
			WorldItem structureWorldItem = null;
			StackItem structureStackitem = null;

			if (WIGroups.FindChildItem (StructurePath, out structureWorldItem)) {
				if (Player.Local.Surroundings.IsVisitingStructure (structureWorldItem.Get <Structure> ())) {
					HasCompleted = true;
				}
			}

			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectivePreventCharacterDeath : ObjectiveScript
	{
		public string CharacterName = "Character";

		public override List<AvatarAction> Subscriptions {
			get {
				return new List<AvatarAction> () { AvatarAction.NpcDie };
			}
		}

		public override void CheckIfCompleted ()
		{
			Character character = null;
			if (Characters.Get.SpawnedCharacter (CharacterName, out character)) {
				if (character.IsDead) {
					Status |= MissionStatus.Failed;
				}
			}
			HasCompleted = true;//true by default
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCharacterFinishSpeech : ObjectiveScript
	{
		public string CharacterName = "Character";
		public string SpeechName = "Speech";

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List<AvatarAction> () { AvatarAction.NpcSpeechFinish };
			}
		}

		public override void CheckIfCompleted ()
		{
			Character character = null;
			if (Characters.Get.SpawnedCharacter (CharacterName, out character)) {
				if (character.IsDead) {
					Status |= MissionStatus.Failed;
				}
			}
			HasCompleted = true;//true by default
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCompleteBeforeNightfall : ObjectiveScript
	{
	}

	[Serializable]
	public class ObjectivePreventQuestItemDeath : ObjectiveScript
	{
		public MobileReference ItemReference = new MobileReference ();

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List<AvatarAction> () { AvatarAction.ItemQuestItemDie };
			}
		}

		public override void CheckIfCompleted ()
		{
			if (mLoadingStackItem) {
				return;
			}
			WIGroups.SuperLoadStackItem (ItemReference.GroupPath, ItemReference.FileName, new IWIBaseCallback (FinishChecking));
		}

		public void FinishChecking (IWIBase iwiBase)
		{
			mLoadingStackItem = false;

			if (iwiBase == null) {
				//failure! since it's a Location - which should never return null - put out a warning
				Debug.LogError ("WARNING: Location returned null in objective script - this should never happen!");
			} else if (iwiBase.IsWorldItem) {
				if (iwiBase.worlditem.Mode == WIMode.Destroyed) {
					//Debug.Log ("FAILED to prevent death of " + ItemReference.FileName);
					Status |= MissionStatus.Failed;
				}
			} else {
				if (iwiBase.Mode == WIMode.Destroyed) {
					//Debug.Log ("FAILED to prevent death of " + ItemReference.FileName);
					Status |= MissionStatus.Failed;
				}
			}
			FinishedChecking = true;
			HasCompleted = true;//always true for this script
		}

		protected bool mLoadingStackItem = false;
	}

	[Serializable]
	public class ObjectivePlaceQuestItemInReceptacle : ObjectiveScript
	{
		public string ItemName = "Item";
		public string ReceptacleName = "Receptacle";

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List<AvatarAction> () { AvatarAction.ItemPlace };
			}
		}

		public override void CheckIfCompleted ()
		{
			//TEMP
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveInitiateConversation : ObjectiveScript
	{
		public string ConversationName = string.Empty;

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.NpcConverseStart };
			}
		}

		public override void CheckIfCompleted ()
		{
			string formattedConversationName = DataImporter.GetNameFromDialogName (ConversationName);
			if (Conversations.Get.NumTimesInitiated (formattedConversationName) > 0) {
				HasCompleted = true;
				Status |= MissionStatus.Completed;
			}
			FinishedChecking = true;
		}
	}

	[Serializable]
	public class ObjectiveCheckBookStatus : ObjectiveScript
	{
		public string BookName = "Book";
		[BitMask(typeof(BookStatus))]
		public BookStatus
			CheckStatus = BookStatus.Dormant;

		public override List<AvatarAction> 	Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.BookRead, AvatarAction.BookAquire };
			}
		}

		public override void CheckIfCompleted ()
		{
			BookStatus status = BookStatus.None;
			if (Books.GetBookStatus (BookName, out status)) {
				if (Flags.Check ((uint)status, (uint)CheckStatus, Flags.CheckType.MatchAny)) {
					HasCompleted = true;
					Status |= MissionStatus.Completed;
				}
			}
			FinishedChecking = true;
		}
	}

	public class ObjectiveFocusOnQuestItem : ObjectiveScript
	{
		public string ItemName = "QuestItem";

		public override void CheckIfCompleted ()
		{
			//not implemented
		}
	}

	public class ObjectiveFocusOnCharacter : ObjectiveScript
	{
		public string CharacterName = "Character";

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.NpcFocus };
			}
		}

		public override void CheckIfCompleted ()
		{
			if (Player.Local.Focus.IsFocusingOnSomething) {
				Character character = null;
				if (Player.Local.Focus.LastFocusedObject.IOIType == ItemOfInterestType.WorldItem
					&& Player.Local.Focus.LastFocusedObject.worlditem.Is <Character> (out character)) {
					HasCompleted = character.worlditem.FileName == CharacterName;
				}
			}
			if (HasCompleted) {
				Status |= MissionStatus.Completed;
			}
			FinishedChecking = true;
		}
	}

	public class ObjectiveStatusKeeperValue : ObjectiveScript
	{
		public string StatusKeeperName = "Health";
		public float Value = 1.0f;
		public VariableCheckType CheckType = VariableCheckType.GreaterThanOrEqualTo;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.SurvivalLoseStatus, AvatarAction.SurvivalRestoreStatus };
			}
		}

		public override void CheckIfCompleted ()
		{
			HasCompleted = false;
			StatusKeeper statusKeeper = null;
			if (Player.Local.Status.GetStatusKeeper (StatusKeeperName, out statusKeeper)) {
				HasCompleted = GameData.CheckVariable (CheckType, Value, statusKeeper.NormalizedValue);
				if (HasCompleted) {
					Status |= MissionStatus.Completed;
				}
			}
			FinishedChecking = true;
		}
	}

	public class ObjectiveQuestItemEnterTrigger : ObjectiveScript
	{
		public int ChunkID;
		public string TriggerName = "WorldTrigger";
		public string QuestItemName = "QuestItem";

		public override void CheckIfCompleted ()
		{
			//not implemented
		}
	}

	public class ObjectivePlayerCurrencyCheck : ObjectiveScript
	{
		public int BaseCurrencyValue = 0;
		public bool UseMissionVariableForValue = false;
		public string MissionName = string.Empty;
		public string VariableName = string.Empty;
		public VariableCheckType CheckType = VariableCheckType.EqualTo;

		public override List<AvatarAction> Subscriptions {
			get {
				return new List <AvatarAction> () { AvatarAction.ItemCurrencyExchange };
			}
		}

		public override void CheckIfCompleted ()
		{
			int currentValue = Player.Local.Inventory.InventoryBank.BaseCurrencyValue;
			int checkValue = BaseCurrencyValue;
			if (UseMissionVariableForValue) {
				string missionName = objective.mission.name;
				if (!string.IsNullOrEmpty (MissionName)) {
					missionName = MissionName;
				}
				Missions.Get.MissionVariable (missionName, VariableName, ref checkValue);
			}

			HasCompleted = GameData.CheckVariable (CheckType, checkValue, currentValue);
			FinishedChecking = true;
		}
	}
}

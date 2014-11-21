using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers {
	public class Missions : Manager
	{
		public static Missions Get;
		public static bool TryingToComplete {
			get {
				if (Get == null) {
					return false;
				}

				for (int i = 0; i < Get.ActiveMissions.Count; i++) {
					if (Get.ActiveMissions [i].TryingToComplete) {
						return true;
					}
				}
				return false;
			}
		}
		public Transform MissionParent;

		public List <Mission> ActiveMissions {
			get {
				if (!mInitialized) {
					return null;
				}
				List <Mission> activeMissions = new List<Mission> (mInstantiatedMissions.Values);
				return activeMissions;
			}
		}

		public override void WakeUp ()
		{
			Get = this;
			mInstantiatedMissions = new Dictionary <string, Mission> ();
			mActiveQuestItems = new Dictionary <string, WorldItem> ();
		}

		public static void ClearLog ()
		{
			Mods.Get.Runtime.ResetProfileData ("Mission");
			foreach (KeyValuePair <string, Mission> mission in Get.mInstantiatedMissions) {
				if (mission.Value != null) {
					mission.Value.Archive (false);
				}
			}
			Get.mInstantiatedMissions.Clear ();
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionActivate, WorldClock.Time);
		}

		public override void OnGameUnload ()
		{
			foreach (Mission mission in mInstantiatedMissions.Values) {
				GameObject.Destroy (mission.gameObject);
			}
			mInstantiatedMissions.Clear ();
		}

		public override void OnGameLoadStart ()
		{
			List <string> missionNames = Mods.Get.Available ("Mission");
			foreach (string missionName in missionNames) {
				MissionState missionState = null;
				if (Mods.Get.Runtime.LoadMod <MissionState> (ref missionState, "Mission", missionName)) {
					if (Flags.Check <MissionStatus> (missionState.Status, MissionStatus.Active, Flags.CheckType.MatchAny)) {	//store all active missions in the lookup
						CreateMissionFromState (missionState);
					}
				}
			}
			foreach (MissionState missionState in MissionStatesByStatus (MissionStatus.Active)) {
				//create the mission so it's actively looking for updates, etc
				CreateMissionFromState (missionState);
			}
		}

		#region get and set
		public int GetVariableValue (string missionName, string variableName)
		{
			MissionState State = MissionStateByName (missionName);
			int variableValue = 0;
			State.Variables.TryGetValue (variableName, out variableValue);
			return variableValue;
		}

		public bool ChangeVariableValue (string missionName, string variableName, int variableValue, ChangeVariableType changeType)
		{
			switch (changeType) {
			case ChangeVariableType.Increment:
			default:
				return IncrementVariable (missionName, variableName, variableValue);

			case ChangeVariableType.Decrement:
				return DecrementValue (missionName, variableName, variableValue);

			case ChangeVariableType.SetValue:
				return SetVariableValue (missionName, variableName, variableValue);
			}
			return false;
		}

		public bool SetVariableValue (string missionName, string variableName, int variableValue)
		{
			MissionState State = MissionStateByName (missionName);
			if (State.Variables.ContainsKey (variableName)) {
				//DebugConsole.Get.Log.Add ("#Setting " + missionName + " variable " + variableName + " to " + variableValue);
				State.Variables [variableName] = variableValue;
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.Time);
				return true;
			} else {
				//DebugConsole.Get.Log.Add ("#Variable " + variableName + " not found in mission");
			}
			return false;
		}

		public bool IncrementVariable (string missionName, string variableName, int setValue)
		{
			MissionState State = MissionStateByName (missionName);
			int variableValue = 0;
			if (State.Variables.TryGetValue (variableName, out variableValue)) {
				variableValue += setValue;
				State.Variables [variableName] = variableValue;
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.Time);
				return true;
			}
			return false;
		}

		public bool DecrementValue (string missionName, string variableName, int setValue)
		{
			MissionState State = MissionStateByName (missionName);
			int variableValue = 0;
			if (State.Variables.TryGetValue (variableName, out variableValue)) {
				variableValue -= setValue;
				State.Variables [variableName] = variableValue;
				Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.Time);
				return true;
			}
			return false;
		}
		#endregion

		#region activation and failure
		public void ActivateMission (string missionName, MissionOriginType originType, string originName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.Activate (originType, originName);
			}
		}

		public void ActivateObjective (string missionName, string objectiveName, MissionOriginType originType, string originName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.ActivateObjective (objectiveName, originType, originName);
			}
		}

		public void ForceFailObjective (string missionName, string objectiveName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.ForceFailObjective (objectiveName);
			}
		}

		public void ForceFailMission (string missionName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.ForceFail ( );
			}
		}

		public void IgnoreMission (string missionName)
		{
			//Temp
			//TODO fix this
			ForceFailMission (missionName);
		}

		public void IgnoreObjective (string missionName, string objectiveName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.IgnoreObjective (objectiveName);
			}		
		}

		public void ForceCompleteMission (string missionName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.ForceComplete ();
			}	
		}

		public void ForceCompleteObjective (string missionName, string objectiveName)
		{
			Mission mission = MissionByName (missionName);
			if (mission != null) {
				mission.ForceCompleteObjective (objectiveName);
			}	
		}
		#endregion

		public bool ActiveQuestItem (string itemName, out WorldItem questItem)
		{
			return mActiveQuestItems.TryGetValue (itemName, out questItem);
		}

		public void AddQuestItem (WorldItem questItem)
		{
			if (!mActiveQuestItems.ContainsKey (questItem.FileName)) {
				mActiveQuestItems.Add (questItem.FileName, questItem);
			}
		}

		public bool MissionCompletedByName (string missionName, ref bool completed)
		{
			MissionState missionState = MissionStateByName (missionName);
			if (missionState != null) {
				completed = missionState.ObjectivesCompleted;
				return true;
			}
			return false;
		}

		public bool ObjectiveCompletedByName (string missionName, string objectiveName, ref bool completed)
		{
			MissionState missionState = MissionStateByName (missionName);
			if (missionState != null && missionState.Status != MissionStatus.Dormant) {//<-May need to change this
				ObjectiveState objectiveState = missionState.GetObjective (objectiveName);
				if (objectiveState != null) {
					completed = objectiveState.Completed;
					return true;
				}
			}
			return false;		
		}

		public bool MissionVariable (string missionName, string variableName, ref int currentValue)
		{
			MissionState missionState = MissionStateByName (missionName);
			return missionState.Variables.TryGetValue (variableName, out currentValue);
		}

		public bool MissionStatusByName (string missionName, ref MissionStatus status)
		{
			MissionState missionState = MissionStateByName (missionName);
			if (missionState != null) {
				status = missionState.Status;
				//Debug.Log ("Found mission status for " + missionName + ", returning " + status.ToString ());
				return true;
			}
			return false;
		}

		public bool ObjectiveStatusByName (string missionName, string objectiveName, ref MissionStatus status)
		{
			MissionState missionState = MissionStateByName (missionName);
			MissionStatus objectiveStatus = MissionStatus.Dormant;
			if (missionState != null && missionState.Status != MissionStatus.Dormant) {//<-May need to change this
				ObjectiveState objectiveState = missionState.GetObjective (objectiveName);
				if (objectiveState != null) {
					status = objectiveState.Status;
					return true;
				}
			}
			return false;
		}

		public List <MissionState> MissionStatesByStatus (MissionStatus status)
		{
			List <MissionState> missionStates = new List <MissionState> ();
			List <string> missionNames = Mods.Get.Available ("Mission");
			for (int i = 0; i < missionNames.Count; i++) {
				MissionState missionState = Missions.Get.MissionStateByName (missionNames [i]);
				if (Flags.Check <MissionStatus> (missionState.Status, status, Flags.CheckType.MatchAny)) {
					////Debug.Log ("Mission status has " + status.ToString ( ));
					missionStates.Add (missionState);
				}
			}
			return missionStates;
		}

		//TODO make this follow the bool / out pattern
		public MissionState MissionStateByName (string missionName)
		{
			if (string.IsNullOrEmpty (missionName)) {
				return null;
			}
			//first check in active missions
			MissionState missionState = null;
			Mission mission = null;
			if (mInstantiatedMissions.TryGetValue (missionName, out mission)) {
				if (mission == null || mission.Archived) {
					mInstantiatedMissions.Remove (missionName);
				} else {
					missionState = mission.State;
				}
			}

			if (missionState == null) {//if that's a no-go, try just loading it outright
				Mods.Get.Runtime.LoadMod <MissionState> (ref missionState, "Mission", missionName);
			}
			return missionState;
		}

		//TODO make this follow the bool / out pattern
		public Mission MissionByName (string missionName)
		{
			//first check in active missions
			Mission mission = null;
			if (mInstantiatedMissions.TryGetValue (missionName, out mission)) {
				if (mission == null || mission.Archived) {
					mInstantiatedMissions.Remove (missionName);
					mission = null;
				}
			}
			if (mission == null) {
				//if that's a no-go, try just loading it outright
				//if it's null, oh well. we'll add error checking later
				MissionState missionState = null;
				if (Mods.Get.Runtime.LoadMod <MissionState> (ref missionState, "Mission", missionName)) {
					mission = CreateMissionFromState (missionState);
				} else {
					//Debug.Log ("Couldn't find mission " + missionName + " on disk");
				}
			}
			return mission;
		}

		protected Mission CreateMissionFromState (MissionState state)
		{
			//Debug.Log ("Creating mission " + state.Name);
			GameObject newMissionGameObject = new GameObject (state.Name);
			newMissionGameObject.transform.parent = MissionParent;
			Mission newMission = newMissionGameObject.AddComponent <Mission> ();
			newMission.State = state;
			newMission.OnLoaded ();
			if (!mInstantiatedMissions.ContainsKey (state.Name)) {
				mInstantiatedMissions.Add (state.Name, newMission);
			} else {
				//Debug.LogError ("WTF - mission " + state.Name + " is being added twice");
			}

			return newMission;
		}

		public Dictionary <string, Mission> mInstantiatedMissions;// = new Dictionary <string, Mission> ();
		public Dictionary <string, WorldItem> mActiveQuestItems;// = new Dictionary <string, WorldItem> ();
	}

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

	public enum ObjectiveType
	{
		Required,
		RequiredOnceActive,
		Optional,
	}

	public enum ObjectiveActivation
	{
		AutomaticOnMissionActivation,
		AutomaticOnPreviousCompletion,
		Manual,
	}

	public enum ObjectiveBehavior
	{
		Permanent,
		//once you succeed or fail, you can't revert
		Toggle,
		//you can toggle from failure to success
	}

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
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using System.Linq;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers
{
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

		public List <MissionState> MissionStates = new List<MissionState> ();

		public override void WakeUp ()
		{
			base.WakeUp ();

			Get = this;
			mInstantiatedMissions = new Dictionary <string, Mission> ();
			mActiveQuestItems = new Dictionary <string, WorldItem> ();
		}

		public static void ClearLog ()
		{
			Mods.Get.Runtime.ResetProfileData ("Mission");
			foreach (KeyValuePair <string, Mission> mission in Get.mInstantiatedMissions) {
				if (mission.Value != null) {
					GameObject.Destroy (mission.Value.gameObject);
				}
			}
			Get.mInstantiatedMissions.Clear ();
			Get.MissionStates.Clear ();
			Mods.Get.Runtime.LoadAvailableMods (Get.MissionStates, "Mission");
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionUpdated, WorldClock.AdjustedRealTime);
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
			MissionStates.Clear ();
			Mods.Get.Runtime.LoadAvailableMods (MissionStates, "Mission");
			for (int i = 0; i < MissionStates.Count; i++) {
				if (Flags.Check ((uint)MissionStates [i].Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {
					//if a mission is active create a gameobject for it
					CreateMissionFromState (MissionStates [i]);
				}
			}
		}

				#region get and set

		public int GetVariableValue (string missionName, string variableName)
		{
			MissionState missionState = null;
			int variableValue = 0;
			if (MissionStateByName (missionName, out missionState)) {
				missionState.Variables.TryGetValue (variableName, out variableValue);
			}
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
			MissionState missionState = null;
			if (MissionStateByName (missionName, out missionState)) {
				if (missionState.Variables.ContainsKey (variableName)) {
					missionState.Variables [variableName] = variableValue;
					Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.AdjustedRealTime);
					return true;
				} else {
					Debug.LogError ("Variable " + variableName + " not found in mission " + missionName + " - adding now");
					missionState.Variables.Add (variableName, variableValue);
				}
			}
			return false;
		}

		public bool IncrementVariable (string missionName, string variableName, int setValue)
		{
			MissionState missionState = null;
			if (MissionStateByName (missionName, out missionState)) {
				int variableValue = 0;
				if (missionState.Variables.TryGetValue (variableName, out variableValue)) {
					variableValue += setValue;
					missionState.Variables [variableName] = variableValue;
					Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.AdjustedRealTime);
					return true;
				} else {
					Debug.LogError ("Variable " + variableName + " not found in mission " + missionName + " - adding now");
					missionState.Variables.Add (variableName, variableValue + setValue);
				}
			}
			return false;
		}

		public bool DecrementValue (string missionName, string variableName, int setValue)
		{
			MissionState missionState = null;
			if (MissionStateByName (missionName, out missionState)) {
				int variableValue = 0;
				if (missionState.Variables.TryGetValue (variableName, out variableValue)) {
					variableValue -= setValue;
					missionState.Variables [variableName] = variableValue;
					Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionVariableChange, WorldClock.AdjustedRealTime);
					return true;
				} else {
					Debug.LogError ("Variable " + variableName + " not found in mission " + missionName + " - adding now");
					missionState.Variables.Add (variableName, variableValue);
				}
			}
			return false;
		}

				#endregion

				#region activation and failure

		public void ActivateMission (string missionName, MissionOriginType originType, string originName)
		{
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				mission.Activate (originType, originName);
			}
		}

		public void ActivateObjective (string missionName, string objectiveName, MissionOriginType originType, string originName)
		{
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				mission.ActivateObjective (objectiveName, originType, originName);
			}
		}

		public void ForceFailObjective (string missionName, string objectiveName)
		{
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				mission.ForceFailObjective (objectiveName);
			}
		}

		public void ForceFailMission (string missionName)
		{
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				mission.ForceFail ();
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
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				mission.IgnoreObjective (objectiveName);
			}		
		}

		public void ForceCompleteMission (string missionName)
		{
			Debug.Log ("Force-completing mission: " + missionName);
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				Debug.Log ("Mission found, force-completing now");
				mission.ForceComplete ();
			}	
		}

		public void ForceCompleteObjective (string missionName, string objectiveName)
		{
			Debug.Log ("Force-completing objective: " + missionName + ", " + objectiveName);
			Mission mission = null;
			if (MissionByName (missionName, out mission)) {
				Debug.Log ("Objective found, force-completing now");
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
			MissionState missionState = null;
			MissionStateByName (missionName, out missionState);
			if (missionState != null) {
				completed = missionState.ObjectivesCompleted;
				return true;
			}
			return false;
		}

		public bool ObjectiveCompletedByName (string missionName, string objectiveName, ref bool completed)
		{
			MissionState missionState = null;
			if (MissionStateByName (missionName, out missionState) && missionState.Status != MissionStatus.Dormant) {//<-May need to change this
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
			MissionState missionState = null;
			MissionStateByName (missionName, out missionState);
			return missionState.Variables.TryGetValue (variableName, out currentValue);
		}

		public bool MissionStatusByName (string missionName, ref MissionStatus status)
		{
			MissionState missionState = null;
			MissionStateByName (missionName, out missionState);
			if (missionState != null) {
				status = missionState.Status;
				//Debug.Log ("Found mission status for " + missionName + ", returning " + status.ToString ());
				return true;
			}
			return false;
		}

		public bool ObjectiveStatusByName (string missionName, string objectiveName, ref MissionStatus status)
		{
			MissionState missionState = null;
			if (MissionStateByName (missionName, out missionState) && missionState.Status != MissionStatus.Dormant) {
				MissionStatus objectiveStatus = MissionStatus.Dormant;
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
			for (int i = 0; i < MissionStates.Count; i++) {
				if (Flags.Check ((uint)MissionStates [i].Status, (uint)status, Flags.CheckType.MatchAny)) {
					missionStates.Add (MissionStates [i]);
				}
			}
			return missionStates;
		}
		//TODO make this follow the bool / out pattern
		public bool MissionStateByName (string missionName, out MissionState missionState)
		{
			missionState = null;
			if (!string.IsNullOrEmpty (missionName)) {
				for (int i = 0; i < MissionStates.Count; i++) {
					if (MissionStates [i].Name == missionName) {
						missionState = MissionStates [i];
						break;
					}
				}
			}
			return missionState != null;
		}

		public bool MissionByName (string missionName, out Mission mission)
		{				
			if (!mInstantiatedMissions.TryGetValue (missionName, out mission)) {
				//we need to create it
				//get the state
				MissionState missionState = null;
				if (MissionStateByName (missionName, out missionState)) {
					mission = CreateMissionFromState (missionState);
				}
			}
			return mission != null;
		}

		protected Mission CreateMissionFromState (MissionState missionState)
		{
			//Debug.Log ("Creating mission " + state.Name);
			GameObject newMissionGameObject = new GameObject (missionState.Name);
			newMissionGameObject.transform.parent = MissionParent;
			Mission newMission = newMissionGameObject.AddComponent <Mission> ();
			newMission.State = missionState;
			newMission.OnLoaded ();
			if (!mInstantiatedMissions.ContainsKey (missionState.Name)) {
				mInstantiatedMissions.Add (missionState.Name, newMission);
			} else {
				Debug.LogError ("Mission Strangeness - mission " + missionState.Name + " is being added twice");
			}

			return newMission;
		}

		public Dictionary <string, Mission> mInstantiatedMissions;
		public Dictionary <string, WorldItem> mActiveQuestItems;
	}
}
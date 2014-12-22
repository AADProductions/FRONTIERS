using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
	public class Mission : MonoBehaviour
	{
		[NonSerialized]
		public MissionState State = new MissionState ();
		[NonSerialized]
		public List <MissionObjective> FirstObjectives = new List<MissionObjective> ();

		public bool Archived {
			get {
				return mArchived;
			}
		}

		public bool TryingToComplete {
			get {
				return mTryingToCompleteMission;
			}
		}

		public void ActivateObjective (string objectiveName, MissionOriginType originType, string originName)
		{
			if (!Flags.Check ((uint)State.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {
				Activate (originType, originName);
			}
			MissionObjective objective = null;
			if (mObjectiveLookup.TryGetValue (objectiveName, out objective)) {
				objective.ActivateObjective (ObjectiveActivation.Manual, originType, originName);
			}
			State.GetPlayerAttention = true;
			Save ();
		}

		protected bool mArchived = false;

		public void Archive (bool save)
		{
			mArchived = true;
			StartCoroutine (ArchiveOverTime (save));
		}

		protected IEnumerator ArchiveOverTime (bool save) {

			while (mRefreshingMission) {
				yield return null;
			}

			if (save) {
				Save ();
			}

			GameObject.Destroy (gameObject, 0.1f);

			yield break;
		}

		public void IgnoreObjective (string objectiveName)
		{
			MissionObjective objective = null;
			if (mObjectiveLookup.TryGetValue (objectiveName, out objective)) {
				objective.IgnoreObjective ();
			}
			
			Save ();
		}

		public void Activate (MissionOriginType originType, string originName)
		{
			//if we're not completed and we're not active...
			if (!State.ObjectivesCompleted && !Flags.Check ((uint)State.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {
				State.OriginType = originType;
				State.OriginName = originName;
				State.TimeActivated = WorldClock.Time;
				State.Status = MissionStatus.Active;
				//create and activate the first objective
				for (int i = 0; i < FirstObjectives.Count; i++) {
					FirstObjectives [i].OnActivateMission ();
				}
				Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionActivate), WorldClock.Time);
				Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
				GUIManager.PostGainedItem (this);
				State.GetPlayerAttention = true;
				Save ();
			}
		}

		public void Refresh ()
		{
			if (mDestroyed) {
				return;
			}

			if (mRefreshingMission || mTryingToCompleteMission) {
				return;
			}

			if (!Flags.Check ((uint)State.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {
				return;
			}

			if (State.ObjectivesCompleted) {
				return;
			}
						
			mRefreshingMission = true;
			StartCoroutine (RefreshOverTime ());
		}

		public void ForceCompleteObjective (string objectiveName)
		{
			MissionObjective objective = null;
			if (mObjectiveLookup.TryGetValue (objectiveName, out objective)) {
				objective.ForceComplete ();
			}
			Refresh ();
			Save ();
		}

		public void ForceComplete ()
		{
			foreach (MissionObjective objective in mObjectiveLookup.Values) {
				objective.ForceComplete ();
			}
			Refresh ();
			Save ();
		}

		public void ForceFail ()
		{
			foreach (MissionObjective objective in mObjectiveLookup.Values) {
				objective.ForceFail ();
			}
			State.Status = MissionStatus.Failed;
			//Refresh ();
			Save ();
		}

		public void ForceFailObjective (string objectiveName)
		{
			MissionObjective objective = null;
			if (mObjectiveLookup.TryGetValue (objectiveName, out objective)) {
				objective.ForceFail ();
			}
			//Refresh ();
			Save ();
		}

		protected IEnumerator TryToCompleteMission (bool manualCompletion)
		{
			mTryingToCompleteMission = true;
			//we assume that we've checked that the mission is active
			//and that we're not completed at this point
			for (int i = 0; i < FirstObjectives.Count; i++) {
				FirstObjectives [i].Refresh (true);
			}

			if (State.CompletionType == MissionCompletion.Manual && !manualCompletion) {
				mTryingToCompleteMission = false;
				yield break;
			}

			//start completing mission objectives
			//this will set State.ObjectivesCompleted
			//reset State.ObjectivesCompleted and State.Completed
			for (int i = 0; i < FirstObjectives.Count; i++) {
				yield return StartCoroutine (FirstObjectives [i].TryToComplete ());
			}
			
			//all the objectives have set their completed state
			bool objectivesCompleted = true;
			foreach (MissionObjective objective in mObjectiveLookup.Values) {
				if (!objective.State.Completed && objective.State.MustBeCompleted) {	//oops
					objectivesCompleted = false;
					break;
				}
			}			
			State.ObjectivesCompleted = objectivesCompleted;
			
			if (State.ObjectivesCompleted && !State.HasAnnouncedCompletion) {
				//the objectives have returned their status
				//now check to see what that status is...
				//if any of the objectives are FAILED
				//and were required then the status will contain
				//the FAILED flag
				if (Flags.Check ((uint)State.Status, (uint)MissionStatus.Failed, Flags.CheckType.MatchAny)) {
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionFail), WorldClock.Time);
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
					GUIManager.PostDanger ("Failed Mission: " + State.Name);
				} else {// if (Flags.Check <MissionStatus> (State.Status, MissionStatus.Completed, Flags.CheckType.MatchAny)) not necessary
					State.Status = MissionStatus.Completed;
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionComplete), WorldClock.Time);
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
					GUIManager.PostGainedItem (this);
				}
				State.HasAnnouncedCompletion = true;
			}

			mTryingToCompleteMission = false;
			yield break;
		}

		protected IEnumerator RefreshOverTime ()
		{
			//wait a tad for all of this frame's updates to pile in
			yield return WorldClock.WaitForRTSeconds (0.025f);//this way it will update when the game is paused

			//if we're not already completed and we're active
			if (!State.ObjectivesCompleted && Flags.Check ((uint)State.Status, (uint)MissionStatus.Active, Flags.CheckType.MatchAny)) {	//try to complete the mission
				if (!mTryingToCompleteMission) {
					yield return StartCoroutine (TryToCompleteMission (false));
				}
			}		
			
			mRefreshingMission = false;
			Save ();
			yield break;
		}

		public void EditorSave ()
		{
			Mods.Get.Editor.InitializeEditor (true);
			Mods.Get.Editor.SaveMod <MissionState> (State, "Mission", State.Name);
		}

		public void EditorLoad ()
		{
			Mods.Get.Editor.InitializeEditor (true);
			Mods.Get.Editor.LoadMod <MissionState> (ref State, "Mission", State.Name);
		}

		public void Save ()
		{
			for (int i = 0; i < FirstObjectives.Count; i++) {
				FirstObjectives [i].OnSaveMission ();
			}
			
			Mods.Get.Runtime.SaveMod <MissionState> (State, "Mission", State.Name);
		}

		public void OnLoaded ()
		{
			for (int i = 0; i < FirstObjectives.Count; i++) {
				if (FirstObjectives [i] != null) {
					GameObject.Destroy (FirstObjectives [i].gameObject);
				}
			}
			FirstObjectives.Clear ();
			mObjectiveLookup.Clear ();
			//create the lookup and the first objectives
			for (int i = 0; i < State.Objectives.Count; i++) {
				MissionObjective objective = CreateObjectiveFromState (State.Objectives [i], transform);
				if (State.FirstObjectiveNames.Contains (objective.State.FileName)) {
					FirstObjectives.Add (objective);
				}
			}

			foreach (MissionObjective objective in mObjectiveLookup.Values) {
				objective.OnLoadMission (this);
			}
		}

		public bool GetObjective (string objectiveName, out MissionObjective objective)
		{
			return mObjectiveLookup.TryGetValue (objectiveName, out objective);
		}

		public bool GetObjectiveState (string objectiveName, out ObjectiveState objectiveState)
		{
			objectiveState = null;
			MissionObjective objective = null;
			if (mObjectiveLookup.TryGetValue (objectiveName, out objective)) {
				objectiveState = objective.State;
			}
			return objectiveState != null;
		}

		public MissionObjective CreateObjectiveFromState (ObjectiveState state, Transform parent)
		{
			GameObject newObjectiveGameObject = parent.gameObject.CreateChild (state.Name).gameObject;
			MissionObjective newObjective = newObjectiveGameObject.AddComponent <MissionObjective> ();
			newObjective.State = state;
			state.ParentObjective = newObjective;
			mObjectiveLookup.Add (newObjective.State.FileName, newObjective);			
			return newObjective;
		}

		public void OnDestroy ( )
		{
			mDestroyed = true;
		}

		protected bool mDestroyed = false;
		protected bool mTryingToCompleteMission = false;
		protected bool mRefreshingMission = false;
		protected Dictionary <string, MissionObjective> mObjectiveLookup = new Dictionary <string, MissionObjective> ();
	}

	[Serializable]
	public class MissionState : Mod
	{
		public string Title = "New Mission";
		public string IconName = string.Empty;
		public MissionOriginType OriginType = MissionOriginType.None;
		public string OriginName = string.Empty;
		public bool ObjectivesCompleted = false;
		public bool HasAnnouncedCompletion	= false;
		[BitMask (typeof(MissionStatus))]
		public MissionStatus Status = MissionStatus.Dormant;
		public double TimeActivated;
		public double TimeCompleted;
		public bool GetPlayerAttention;
		public MissionCompletion CompletionType = MissionCompletion.Automatic;
		public SDictionary <string, int> Variables = new SDictionary <string, int> ();
		public List <ObjectiveState> Objectives = new List <ObjectiveState> ( );
		public List <string> FirstObjectiveNames {
			get {
				if (mFirstObjectiveNames == null) {
					mFirstObjectiveNames = new List <string> ();
					if (mFirstObjectives != null) {
						for (int i = 0; i < mFirstObjectives.Count; i++) {
							mFirstObjectiveNames.Add (mFirstObjectives [i].FileName);
						}
					}
				}
				return mFirstObjectiveNames;
			}
			set {
				mFirstObjectiveNames = value;
			}
		}

		public List <ObjectiveState> FirstObjectives  {
			get {
				if (mFirstObjectives == null) {
					mFirstObjectives = new List<ObjectiveState> ();
				}
				return mFirstObjectives;
			}
		}

		public List <string> GetObjectiveNames ()
		{
			List <string> objectiveNames = new List <string> ();
			//objectiveNames.AddRange (FirstObjectiveNames);
			foreach (ObjectiveState objective in Objectives) {
				objectiveNames.Add (objective.FileName);
			}
			return objectiveNames;
		}

		public ObjectiveState GetObjective (string objectiveName)
		{
			for (int i = 0; i < Objectives.Count; i++) {
				if (Objectives [i].FileName == objectiveName) {
					return Objectives [i];
				}
			}
			return null;
		}

		public List <ObjectiveState> GetObjectives ()
		{
			return Objectives;
		}

		public List <string> NextMissions = new List <string> ();

		protected List <ObjectiveState> mFirstObjectives = null;
		protected List <string> mFirstObjectiveNames = null;
	}
}
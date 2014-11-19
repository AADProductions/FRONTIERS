using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System.Xml;
using System.Xml.Serialization;

namespace Frontiers.World.Gameplay
{
	[Serializable]
	public class MissionObjective : MonoBehaviour
	{
		public Mission mission = null;
		public ObjectiveState State = new ObjectiveState ();
		public List <MissionObjective> NextObjectives = new List <MissionObjective> ();

		public void OnSaveMission ()
		{
			State.NextObjectiveStates.Clear ();
			foreach (MissionObjective objective in NextObjectives) {
				//this probably isn't necessary, but you never know
				State.NextObjectiveStates.Add (objective.State);
				objective.OnSaveMission ();
			}
		}

		public void OnLoadMission (Mission parentMission)
		{
			mission = parentMission;
			for (int i = 0; i < State.Scripts.Count; i++) {
				State.Scripts [i].objective = this;
			}
			CreateNextObjectives ();
			for (int i= 0; i < NextObjectives.Count; i++) {
				NextObjectives [i].OnLoadMission (parentMission);
			}

			Refresh (false);
		}

		public void OnActivateMission ()
		{
			if (State.Activation == ObjectiveActivation.AutomaticOnMissionActivation) {
				ActivateObjective (ObjectiveActivation.AutomaticOnMissionActivation, MissionOriginType.None, string.Empty);
			}

			Refresh (false);
			//next objectives should have been created by ActivatObjective
			foreach (MissionObjective nextObjective in NextObjectives) {
				nextObjective.OnActivateMission ();
			}
		}

		public void IgnoreObjective ()
		{
			if (State.Completed) {
				return;
			}

			State.Status |= MissionStatus.Active;//ignoring is equivlant to activating it
			State.Status |= MissionStatus.Ignored;
			State.Status &= ~MissionStatus.Dormant;
			if (State.Type == ObjectiveType.Optional
			    || (State.Type == ObjectiveType.RequiredOnceActive && !Flags.Check <MissionStatus> (State.Status, MissionStatus.Active, Flags.CheckType.MatchAny))) {
				if (State.CompleteOnIgnore) {
					State.Completed = true;
				}
			} else {//if we ignore an objective an it's required
				//we've failed
				State.Completed = true;
				State.Status |= MissionStatus.Failed;
				State.Status &= ~MissionStatus.Dormant;
			}
			//refresh - force lower objectives to activate
			TryToComplete ();
		}

		public void ActivateObjective (ObjectiveActivation activation, MissionOriginType originType, string originName)
		{
			//if the attempted activation type is the same as our activation type
			if (!Flags.Check <MissionStatus> (State.Status, MissionStatus.Active | MissionStatus.Completed, Flags.CheckType.MatchAny)) {	//activate and announce activation
				State.OriginType = originType;
				State.OriginName = originName;
				//if the new mission description isn't empty, update our parent mission
				if (!Flags.Check <MissionStatus> (State.Status, MissionStatus.Ignored, Flags.CheckType.MatchAny)) {	//if we HAVEN'T ignored this mission then we'll update
					//if we have just pretend it doesn't exist
					if (!string.IsNullOrEmpty (State.NewMissionDescription)) {
						mission.State.Description = State.NewMissionDescription;
					}
					if (!string.IsNullOrEmpty (State.IntrospectionOnActivate)) {
						GUIManager.PostIntrospection (State.IntrospectionOnActivate);
					}
				}

				for (int i = 0; i < State.Scripts.Count; i++) {
					State.Scripts [i].OnActivated ();
				}

				State.Status = MissionStatus.Active;
				State.Status &= ~MissionStatus.Dormant;
				State.TimeActivated	= WorldClock.Time;

				try {
					//wrap this in a try catch to prevent random stuff listening from screwing up mission completion
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionObjectiveActiveate), WorldClock.Time);
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
				}
				catch (Exception e) {
					Debug.LogError (e.ToString ());
				}
			}
		}

		public void ForceComplete ( )
		{
			//Debug.Log ("Force completing objective");
			State.Completed = true;
			State.Status &= ~MissionStatus.Active;
			State.Status &= ~MissionStatus.Dormant;
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionObjectiveComplete, WorldClock.Time);
			Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
		}

		public void ForceFail ()
		{
			//Debug.Log ("Force failing objective");
			State.Completed = true;
			State.Status |= MissionStatus.Failed;
			State.Status &= ~MissionStatus.Active;
			State.Status &= ~MissionStatus.Dormant;
			Player.Get.AvatarActions.ReceiveAction (AvatarAction.MissionObjectiveFail, WorldClock.Time);
			Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
		}

		public void Refresh (bool refreshNextObjectives)
		{
			if (!mHasSubscribed) {//get all the avatar actions that our scripts want to know about
				//then subscrube to them. this only needs to happen once, on startup
				mSubscriptionListener = new ActionListener (Subscription);
				//get a hashset (to avoid duplicates)
				HashSet <AvatarAction> subscriptions = new HashSet <AvatarAction> ();
				foreach (ObjectiveScript script in State.Scripts) {
					if (script != null) {
						foreach (AvatarAction action in script.Subscriptions) {
							subscriptions.Add (action);
						}
					} else {
						//this should never happen
						Debug.LogError ("SCRIPT WAS NULL IN OBJECTIVE " + State.FileName);
					}
				}
				//add them to the list (I may remove this later, for now it's handy
				State.Subscriptions.Clear ();
				State.Subscriptions.AddRange (subscriptions);
				foreach (AvatarAction subscription in State.Subscriptions) {
					Player.Get.AvatarActions.Subscribe (new PlayerAvatarAction (subscription), mSubscriptionListener);
				}

				mHasSubscribed = true;
			}

			if (refreshNextObjectives) {
				foreach (MissionObjective nextObjective in NextObjectives) {
					if (nextObjective != null) {
						nextObjective.Refresh (true);
					} else {
						Debug.LogError ("OBJECTIVE WAS NULL IN MISSION " + name);
					}
				}
			}
		}

		public bool Subscription (double timeStamp)
		{	//when an avatar action is triggered,
			//refresh the entire mission
			mission.Refresh ();
			return true;
		}

		public IEnumerator TryToComplete ()
		{
			//have we already completed?
			bool checkThisObjective = true;

			//toggling objectives is buggy at the moment
			//and we don't have any missions that use them
			//so skip this for now
//			if (State.Completed) {
//				switch (State.Behavior) {
//				case ObjectiveBehavior.Permanent:
//					//don't bother to check for completion
//					//we're permanently complete
//					checkThisObjective = false;
//					break;
//
//				case ObjectiveBehavior.Toggle:
//				default:
//					//yup, check every time
//					break;
//				}
//			}

			if (!Flags.Check <MissionStatus> (State.Status, MissionStatus.Active, Flags.CheckType.MatchAny)) {
				//Debug.Log ("Objective " + State.Name + " status doesn't have Active, not checking objective");
				//if the objective isn't active yet then don't check
				checkThisObjective = false;
			}

			//are we supposed to check THIS objective? if so do it now
			//otherwise we've already completed - return true
			if (checkThisObjective) {//check each objective script
				//reset this objective's completed state
				State.ResetCompleted ();
				bool thisObjectiveCompleted = true;
				//this is used for scripts that complete and don't require all
				bool completedOverride = false;
				for (int i = 0; i < State.Scripts.Count; i++) {
					ObjectiveScript script = State.Scripts [i];
					yield return StartCoroutine (HasCompletedObjective (script));
					//if the script has completed AND it doesn't require all other scripts
					thisObjectiveCompleted &= script.HasCompleted;
					if (script.HasCompleted && !script.RequiresAll) {//then this objective is automatically completed regardless of the state of other scripts
						completedOverride = true;
					}
				}
				//ok we've checked all the scripts
				//now apply the completed override
				State.Completed = thisObjectiveCompleted | completedOverride;
				if (State.Completed) {
					State.Status &= ~MissionStatus.Active;
				}
			}
			
			//check next objectives now
			foreach (MissionObjective nextObjective in NextObjectives) {
				//see if the objective should be active but isn't
				//(usually happens in the case of 'ignore objective')
				if (!Flags.Check <MissionStatus> (nextObjective.State.Status, MissionStatus.Active, Flags.CheckType.MatchAny)
				    &&	nextObjective.State.Activation == ObjectiveActivation.AutomaticOnPreviousCompletion
				    &&	State.Completed) {
					//wrap this in a try/catch because we don't want random stuff listening for avatar actions to screw us up
					nextObjective.ActivateObjective (ObjectiveActivation.AutomaticOnPreviousCompletion, MissionOriginType.Mission, string.Empty);
				}
				//this will automatically set mission.State.ObjectivesCompleted
				yield return StartCoroutine (nextObjective.TryToComplete ());
			}
			
			if (!checkThisObjective) {
				//if we're not checking this objective then we're done
				yield break;
			}

			//alright we're checking whether we've completed
			//check to see whether this results in failure
			if (State.Completed) {
				State.TimeCompleted = WorldClock.Time;
				//the mission objective is complete! yay!
				//now check for success or failure
				switch (State.Type) {
				case ObjectiveType.Optional:
					//if the objective type is optional
					//then it doesn't matter what the result was
					//don't add our Status because it might contain a failure status
					//so just add Completed instead
					mission.State.Status |= MissionStatus.Completed;
					break;

				case ObjectiveType.Required:
					//if the objective type is required
					//then failure results in a failed mission
					//our Status already contains a failure status
					//so just add that
					mission.State.Status |= State.Status;
					break;

				case ObjectiveType.RequiredOnceActive:
					//if the objective is only required once active
					//first check to see if it's active
					if (Flags.Check <MissionStatus> (State.Status, MissionStatus.Active, Flags.CheckType.MatchAny)) {
						//if it's not, just add completed
						mission.State.Status |= MissionStatus.Completed;
					} else {
						//if it is active, add Status
						mission.State.Status |= State.Status;
					}
					break;
				}

				//now check whether we've failed or succeeded
				if (Flags.Check <MissionStatus> (State.Status, MissionStatus.Failed, Flags.CheckType.MatchAny) && !State.HasAnnouncedFailure) {
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionObjectiveFail), WorldClock.Time);
				Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
					if (!string.IsNullOrEmpty (State.IntrospectionOnFail)) {
						GUIManager.PostIntrospection (State.IntrospectionOnFail);
					}
					if (State.Hidden) {
						GUIManager.PostDanger ("Objective failed: " + State.Name);
					}
					State.HasAnnouncedFailure = true;
				} else if (!State.HasAnnouncedCompletion) {
					Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionObjectiveComplete), WorldClock.Time);
				Player.Get.AvatarActions.ReceiveAction (new PlayerAvatarAction (AvatarAction.MissionUpdated), WorldClock.Time);
					if (!string.IsNullOrEmpty (State.IntrospectionOnComplete)) {
						GUIManager.PostIntrospection (State.IntrospectionOnComplete);
					}
					if (!State.Hidden) {
						GUIManager.PostSuccess ("Objective complete: " + State.Name);
					}
					State.HasAnnouncedCompletion = true;
				}

				//finally, activate next objectives
				foreach (MissionObjective nextObjective in NextObjectives) {
					if (nextObjective.State.Activation == ObjectiveActivation.AutomaticOnPreviousCompletion) {
						nextObjective.ActivateObjective (ObjectiveActivation.AutomaticOnPreviousCompletion, MissionOriginType.None, string.Empty);
					}
				}
			}

			//add our status to the mission status
			mission.State.Status |= State.Status;

			yield break;
		}

		public IEnumerator HasCompletedObjective (ObjectiveScript script)
		{
			//reset the script's completed state
			script.Reset ();
			script.CheckIfCompleted ();
			//wait for the script to finish checking
			while (!script.FinishedChecking) {
				yield return null;
			}
			//add completed state to objective's completed state
			//TryToComplete will automatically break if State.Completed returns false
			State.Completed &= script.HasCompleted;
			State.Status |= script.Status;

			yield break;
		}

		protected void CreateNextObjectives ()
		{
			if (mHasGeneratedNextObjectives) {
				return;
			}

			for (int i = 0; i < State.NextObjectiveNames.Count; i++) {
				MissionObjective nextObjective = null;
				if (mission.GetObjective (State.NextObjectiveNames [i], out nextObjective)) {
					NextObjectives.Add (nextObjective);
				}
			}
			mHasGeneratedNextObjectives = true;
		}

		protected ActionListener mSubscriptionListener = null;
		protected bool mHasSubscribed = false;
		protected bool mHasGeneratedNextObjectives = false;
	}

	[Serializable]
	public class ObjectiveState
	{
		public string Name = "Objective";
		public string FileName = "NewObjective";
		public string Description = string.Empty;
		public string NewMissionDescription	= string.Empty;
		public string IntrospectionOnActivate	= string.Empty;
		public string IntrospectionOnComplete	= string.Empty;
		public string IntrospectionOnFail = string.Empty;
		public MissionOriginType OriginType = MissionOriginType.None;
		public string OriginName = string.Empty;
		[BitMask (typeof(MissionStatus))]
		public MissionStatus Status = MissionStatus.Dormant;
		public ObjectiveType Type = ObjectiveType.Required;
		public bool CompleteOnIgnore = false;
		public ObjectiveActivation Activation = ObjectiveActivation.AutomaticOnPreviousCompletion;
		public ObjectiveBehavior Behavior = ObjectiveBehavior.Permanent;
		public List <ObjectiveScript> Scripts = new List <ObjectiveScript> ();
		public List <AvatarAction> Subscriptions = new List <AvatarAction> ();
		public double TimeActivated = 0.0f;
		public double TimeCompleted = 0.0f;
		public ObjectiveTimeLimit TimeLimit = ObjectiveTimeLimit.None;
		public bool Hidden = false;
		public bool Completed {
			get {
				return mCompleted;
			}
			set {
				if (value) {
					Status |= MissionStatus.Completed;
				} else {
					Status &= ~MissionStatus.Completed;
				}
				mCompleted = value;
			}
		}

		public void ResetCompleted ()
		{
			mCompleted = true;
		}

		public bool MustBeCompleted {
			get{
				switch (Type) {
				case ObjectiveType.Required:
				default:
					return true;

				case ObjectiveType.Optional:
					return false;

				case ObjectiveType.RequiredOnceActive:
					if (Flags.Check <MissionStatus> (MissionStatus.Active, Status, Flags.CheckType.MatchAny)) {
						return true;
					}
					return false;
				}
				return true;
			}
		}
		public bool HasAnnouncedCompletion	= false;
		public bool HasAnnouncedFailure = false;

		[NonSerialized]
		[XmlIgnore]
		public MissionObjective ParentObjective;

		[XmlIgnore]
		public List <ObjectiveState> NextObjectiveStates {
			get {
				if (mNextObjectiveStates == null) {
					mNextObjectiveStates = new List <ObjectiveState> ();
				}
				return mNextObjectiveStates;
			}
		}

		public List <string> NextObjectiveNames {
			get {
				if (mNextObjectiveNames == null) {
					mNextObjectiveNames = new List <string> ();
				}
				return mNextObjectiveNames;
			}
			set {
				mNextObjectiveNames = value;
			}
		}

		protected List <string> mNextObjectiveNames = null;
		[NonSerialized]
		protected List <ObjectiveState> mNextObjectiveStates = null;

		protected bool mCompleted = false;
	}

	public enum ObjectiveTimeLimit
	{
		None,
		BeforeNextNightfall,
		BeforeNextMorning,
	}
}
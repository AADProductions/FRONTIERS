using UnityEngine;
using System.Collections;
using System;
using Frontiers.Story;
using System.Collections.Generic;
using Frontiers.World.WIScripts;
using Frontiers.GUI;

namespace Frontiers.World
{
	public class TriggerGuardIntervention : WorldTrigger
	{
		//this is used to create an impassable barrier
		//that a character uses to keep the player out of somewhere
		//eg a guard in front of a door
		//needs to be tweaked
		public Collider BarrierCollider;
		public ActionNode GuardNode;
		public ActionNode SuccessNode;
		public Character GuardCharacter;
		public TriggerGuardInterventionState State = new TriggerGuardInterventionState ();

		public override void OnInitialized ()
		{
			GameObject bcObject = gameObject.FindOrCreateChild ("BarrierCollider").gameObject;
			bcObject.layer = Globals.LayerNumObstacleTerrain;
			BarrierCollider = bcObject.GetOrAdd <BoxCollider> ();
			BarrierCollider.isTrigger = false;
			State.BarrierTriggerTransform.ApplyTo (bcObject.transform, true);

			if (mBaseState.NumTimesTriggered > 0) {
				//if we've triggered once then we're disabled
				BarrierCollider.enabled = false;
			} else {
				BarrierCollider.enabled = true;
			}

			if (Application.isPlaying) {
				ResumeGuard (false);
			}
		}

		public override bool OnPlayerEnter ()
		{
			if (mMovingPlayerToOtherSide) {
				//we wait for the first intersection then cancel
				mMovingPlayerToOtherSide = false;
				return false;
			}

			bool successfullyPassedTrigger = false;

			if (State.RequireUniform) {
				successfullyPassedTrigger = Player.Local.Wearables.IsWearing (State.UniformType, State.UniformBodyPart, State.UniformOrientation, State.UniformName);
			}

			if (State.RequireExchangesCompleted) {
				string finalConversationName = Frontiers.Data.DataImporter.GetNameFromDialogName (State.ConversationName);
				int exchangeIndex = 0;
				string finalExchangeName = State.ExchangeName;
				if (Int32.TryParse (finalExchangeName, out exchangeIndex)) {
					//it must be an integer
					//Debug.Log ("Getting exchange index in RequireExchangesConcluded - " + finalConversationName + ", " + finalExchangeName);
					finalExchangeName = Frontiers.Conversations.Get.ExchangeNameFromIndex (finalConversationName, exchangeIndex);
				}
				successfullyPassedTrigger = Conversations.Get.HasCompletedExchange (finalConversationName, finalExchangeName, false);
			}

			if (State.RequireMissionObjectiveComplated) {
				Missions.Get.ObjectiveCompletedByName (State.RequiredMissionName, State.RequiredObjectiveName, ref successfullyPassedTrigger);
			}

			if (!successfullyPassedTrigger) {
				ResumeGuard (true);
				return false;
			} else {
				SuspendGuard ();
			}
			return true;
		}

		public bool FindSuccessNode ()
		{

			if (SuccessNode == null) {
				ActionNodeState successState = null;
				if (ParentChunk.GetNode (State.SuccessActionNodeName, false, out successState)) {
					SuccessNode = successState.actionNode;
					if (SuccessNode == null) {
						//Debug.Log ("Couldn't find success node in " + name + " but it doesn't matter, still returning true");
						return false;
					}
				}
			}
			return true;
		}

		public bool FindGuardNode ()
		{
			if (GuardNode == null) {
				ActionNodeState guardNodeState = null;
				if (ParentChunk.GetNode (State.GuardActionNodeName, false, out guardNodeState)) {
					guardNodeState.OccupantIsDead = State.GuardIsDead;
					GuardNode = guardNodeState.actionNode;
					if (GuardNode == null) {
						//Debug.Log ("Couldn't get guard node from action node state, quitting in " + name);
						return false;
					}
				} else {
					//Debug.Log ("Couldn't get guard node from parent chunk, quitting in " + name);
					return false;
				}
			}
			return true;
		}

		protected bool FindGuardCharacter ()
		{
			if (GuardCharacter == null) {
				//use the guard node to get the character
				if (GuardNode.HasOccupant) {
					if (!GuardNode.Occupant.Is <Character> (out GuardCharacter) || GuardCharacter.IsDead) {
						//Debug.Log ("Couldn't get character from guard node occupant, quitting in " + name);
						return false;
					}
					DailyRoutine dr = null;
					if (GuardNode.Occupant.Has <DailyRoutine> (out dr)) {
						//guard nodes don't get to have routines
						dr.Finish ();
					}
				} else {
					//Debug.Log ("Guard node didn't have occupant, quitting in " + name);
					WIGroup group = null;
					if (WIGroups.FindGroup (GuardNode.State.ParentGroupPath, out group)) {
						Characters.GetOrSpawnCharacter (GuardNode, GuardNode.State.OccupantName, group, out GuardCharacter);
					} 
					return GuardCharacter != null;
				}
			}
			return true;
		}

		public bool SuspendGuard ()
		{
			if (!FindGuardNode ()) {
				BarrierCollider.enabled = false;
				return false;
			}
			if (!FindSuccessNode ()) {
				BarrierCollider.enabled = false;
				return false;
			}
			if (!FindGuardCharacter ()) {
				BarrierCollider.enabled = false;
				return false;
			}

			Motile motile = null;
			if (GuardCharacter.worlditem.Is <Motile> (out motile)) {
				GuardNode.VacateNode (GuardCharacter.worlditem);
				MotileAction goToNodeAction = MotileAction.GoTo (SuccessNode.State);
				motile.PushMotileAction (goToNodeAction, MotileActionPriority.ForceTop);
			}
			//do this second so the guard occupies the node first, then pays attention to the player
			if (!string.IsNullOrEmpty (State.DTSOnSuccess)) {
				Talkative talkative = null;
				if (GuardCharacter.worlditem.Is <Talkative> (out talkative)) {
					Speech dts = null;
					if (Mods.Get.Runtime.LoadMod <Speech> (ref dts, "Speech", State.DTSOnSuccess)) {
						talkative.SayDTS (dts);
					}
				}
			}
			BarrierCollider.enabled = false;
			GetComponent<Collider>().enabled = false;
			return true;
		}

		public bool ResumeGuard (bool sayDTS)
		{
			if (!FindGuardNode ()) {
				Debug.Log ("Couldn't find guard node");
				BarrierCollider.enabled = false;
				return false;
			}
			if (!FindSuccessNode ()) {
				Debug.Log ("Couldn't find success node");
				BarrierCollider.enabled = false;
				return false;
			}
			if (!FindGuardCharacter ()) {
				Debug.Log ("Couldn't find guard");
				BarrierCollider.enabled = false;
				return false;
			}

			if (GuardCharacter.IsStunned || GuardCharacter.IsSleeping || GuardCharacter.IsDead) {
				Debug.Log ("Guard is stunned, sleeping or dead");
				BarrierCollider.enabled = false;
				return true;
			}

			//Debug.Log ("Player isn't wearing uniform");
			if (!GuardNode.HasOccupant) {
				Motile motile = null;
				if (GuardCharacter.worlditem.Is <Motile> (out motile)) {
					GuardNode.VacateNode (SuccessNode.worlditem);
					MotileAction goToNodeAction = MotileAction.GoTo (GuardNode.State);
					motile.PushMotileAction (goToNodeAction, MotileActionPriority.ForceTop);
				}
			} else {
				GuardCharacter.LookAtPlayer ();
			}
			GuardCharacter.worlditem.GetOrAdd <Guard> ();
			//do this second so the guard occupies the node first, then pays attention to the player
			if (sayDTS && !string.IsNullOrEmpty (State.DTSOnFailure)) {
				Talkative talkative = null;
				if (GuardCharacter.worlditem.Is <Talkative> (out talkative)) {
					Speech dts = null;
					if (Mods.Get.Runtime.LoadMod <Speech> (ref dts, "Speech", State.DTSOnFailure)) {
						talkative.SayDTS (dts);
					}
				}
			}

			bool isOnBarrierSide = false;
			//use the node + barrier to determine direction
			Vector3 barrierPosition = BarrierCollider.bounds.center;
			barrierPosition.y = Player.Local.Position.y;
			Vector3 barrierDirection = (GuardNode.Position - barrierPosition).normalized;
			Vector3 playerDirection = (Player.Local.Position - barrierPosition).normalized;
			float dot = Vector3.Dot (barrierDirection, playerDirection);
			Debug.Log ("Dot: " + dot.ToString ());
			if (dot < 0f) {
				isOnBarrierSide = true;
			}

			//push the player away
			if (State.PushPlayer) {
				Player.Local.Audio.GetPushed ();
				if (isOnBarrierSide && State.EjectFromOppositeEnd) {
					mMovingPlayerToOtherSide = true;
					StartCoroutine (MovePlayerToOtherSide ());
				} else {
					Player.Local.FPSController.AddForce (Vector3.Normalize (GuardCharacter.worlditem.Position - Player.Local.Position) * -0.1f);
				}
			}
			BarrierCollider.enabled = true;
			GetComponent<Collider>().enabled = true;
			return true;
		}

		protected IEnumerator MovePlayerToOtherSide () {
			mWaitingForStaticFade = true;
			Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), false, 0.5f, 0f, () => {
				mWaitingForStaticFade = false;
			});
			while (mWaitingForStaticFade) {
				yield return null;
			}
			Player.Local.Position = (GuardNode.Position + GuardNode.transform.forward);
			mWaitingForStaticFade = true;
			Frontiers.GUI.CameraFade.StartAlphaFade(Colors.Alpha(Color.black, 1f), true, 0.5f, 0f, () => {
				mWaitingForStaticFade = false;
			});
			while (mWaitingForStaticFade) {
				yield return null;
			}
			yield break;
		}

		public void KillGuard ()
		{
			State.GuardIsDead = true;
			if (GuardCharacter != null && !GuardCharacter.IsDead) {
				Damageable damageable = null;
				if (GuardCharacter.worlditem.Is<Damageable> (out damageable)) {
					damageable.InstantKill (string.Empty);
				}
			}
			if (GuardNode != null) {
				GuardNode.State.OccupantIsDead = true;
			}
		}
		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			GameObject bcObject = gameObject.FindOrCreateChild ("BarrierCollider").gameObject;
			BarrierCollider = bcObject.GetOrAdd <BoxCollider> ();
			State.BarrierTriggerTransform.CopyFrom (bcObject.transform);
			State.GuardActionNodeName = GuardNode.State.Name;
			State.SuccessActionNodeName = SuccessNode.State.Name;
		}
		#endif

		protected bool mWaitingForStaticFade = false;
		protected bool mMovingPlayerToOtherSide = false;
	}

	[Serializable]
	public class TriggerGuardInterventionState : WorldTriggerState
	{
		public bool PushPlayer = true;
		public string DTSOnFailure;
		public string DTSOnSuccess;
		public string GuardActionNodeName;
		public string SuccessActionNodeName;
		public STransform BarrierTriggerTransform = new STransform ();
		public bool ForwardDirectionOnly = true;
		public bool RequireExchangesCompleted = false;
		public bool GuardIsDead = false;
		[FrontiersAvailableModsAttribute ("Conversation")]
		public string ConversationName = string.Empty;
		public string ExchangeName = string.Empty;
		public bool RequireUniform = false;
		public WearableType UniformType = WearableType.Jewelry;
		public BodyPartType UniformBodyPart = BodyPartType.Hand;
		public BodyOrientation UniformOrientation = BodyOrientation.Both;
		public string UniformName;
		public bool RequireMissionObjectiveComplated = false;
		public string RequiredMissionName = string.Empty;
		public string RequiredObjectiveName = string.Empty;
		public bool EjectFromOppositeEnd = true;
	}
}
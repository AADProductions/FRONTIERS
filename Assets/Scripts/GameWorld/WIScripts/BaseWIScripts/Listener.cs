using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class Listener : WIScript, IListener
	{
		public ListenerState State = new ListenerState ();
		public Action OnHearPlayer;
		public Action OnHearWorldItem;
		public Action OnHearItemOfInterest;
		public PlayerBase LastHeardPlayer;
		public WorldItem LastHeardWorldItem;
		public IItemOfInterest LastHeardItemOfInterest;

		public bool IsListeningTo (SpeechBubble audibleSpeech)
		{
			return (mListenAction != null
			&& mListenAction.State == MotileActionState.Started
				&&	mListenAction.LiveTarget ==	audibleSpeech.Speaker.worlditem);
		}

		public bool IsFollowing (SpeechBubble audibleSpeech)
		{
			return (mFollowAction != null
			&& mFollowAction.State == MotileActionState.Started
				&&	mFollowAction.LiveTarget ==	audibleSpeech.Speaker.worlditem);
		}

		public void HearCommand (SpeechBubble audibleSpeech, string command)
		{
			if (audibleSpeech.Speaker.worlditem == worlditem) {	//can't react to your own speeches
				return;
			}

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				MotileAction topAction = motile.TopAction;
				MotileAction newAction = new MotileAction ();
				newAction.Target = new MobileReference (audibleSpeech.Speaker.worlditem.FileName, audibleSpeech.Speaker.worlditem.Group.Props.PathName);
				newAction.LiveTarget = audibleSpeech.Speaker.worlditem;
				MotileActionPriority priority = MotileActionPriority.ForceTop;

				switch (command) {
				case "Listen":
					FXManager.Get.SpawnFX (motile.Body.Transforms.HeadTop, "ListenEffect");
					if (!IsListeningTo (audibleSpeech)) {
						if (topAction.Type == MotileActionType.GoToActionNode) {
							priority = MotileActionPriority.Next;
						}
						////Debug.Log ("Listener is listening to speaker " + audibleSpeech.Speaker.name);
						if (mListenAction == null) {
							mListenAction = new MotileAction ();
						}
						mListenAction.Type = MotileActionType.FocusOnTarget;
						mListenAction.Expiration = MotileExpiration.TargetOutOfRange;
						mListenAction.OutOfRange = audibleSpeech.ParentSpeech.AudibleRange * 2.0f;
						mListenAction.Target = new MobileReference (audibleSpeech.Speaker.worlditem.FileName, audibleSpeech.Speaker.worlditem.Group.Props.PathName);
						mListenAction.LiveTarget = audibleSpeech.Speaker.worlditem;
						mListenAction.YieldBehavior	= MotileYieldBehavior.YieldAndFinish;
						motile.PushMotileAction (mListenAction, priority);
					}
					break;

				case "FollowSpeaker":
					if (!IsFollowing (audibleSpeech)) {
						//Debug.Log ("Listener is following speaker " + audibleSpeech.Speaker.name);
						if (mFollowAction == null) {
							mFollowAction = new MotileAction ();
						}
						mFollowAction.Type = MotileActionType.FollowTargetHolder;
						mFollowAction.LiveTarget = audibleSpeech.Speaker.worlditem;
						mFollowAction.Target = new MobileReference (audibleSpeech.Speaker.worlditem.FileName, audibleSpeech.Speaker.worlditem.Group.Props.PathName);
						mFollowAction.Expiration = MotileExpiration.Duration;
						mFollowAction.RTDuration = 600.0f;
						mFollowAction.YieldBehavior	= MotileYieldBehavior.DoNotYield;
						motile.PushMotileAction (mFollowAction, priority);
					}
					break;

				case "StopListeningAndFollowing":
					if (IsListeningTo (audibleSpeech)) {
						mListenAction.TryToFinish ();
					}
					if (IsFollowing (audibleSpeech)) {
						mFollowAction.TryToFinish ();
					}
					break;

				case "StopListening":
					if (IsListeningTo (audibleSpeech)) {
						mListenAction.TryToFinish ();
					}
					break;

				case "StopFollowing":
					if (IsFollowing (audibleSpeech)) {
						mFollowAction.TryToFinish ();
					}
					break;

				case "LookAtPlayer":
					FXManager.Get.SpawnFX (motile.Body.Transforms.HeadTop, "ListenEffect", UnityEngine.Random.value * 3f);
					worlditem.Get <Character> ().LookAtPlayer ();
					break;

				default:
					break;
				}
			}
		}

		public void HearSound (IAudible source, MasterAudio.SoundType type, string sound)
		{
			//Debug.Log ("Potentially heard sound in " + name + ", it's a " + source.IOIType.ToString ());
			//first see if it's in audible range
			if (source.IsAudible && IsInAudibleRange (worlditem.tr.position, source.Position, AwarenessDistanceTypeToAudibleDistance (State.AudioAwarnessDistance), source.AudibleRange)) {
				//if it's in range AND we're able to hear it
				//Debug.Log ("Yup definitely heard it");
				if (IsAudible (worlditem.tr.position, source.Position, AwarenessDistanceTypeToAudibleSensitivity (State.AudioSensitivity), source.AudibleVolume)) {
					LastHeardPlayer = null;
					LastHeardItemOfInterest = null;
					switch (source.IOIType) {
					case ItemOfInterestType.Player:
						LastHeardPlayer = source.player;
						HeardPlayerGizmo = 1f;
						break;

					case ItemOfInterestType.WorldItem:
						LastHeardWorldItem = source.worlditem;
						break;

					default:
						break;
					}
					LastHeardItemOfInterest = source;

					if (LastHeardPlayer != null) {
						OnHearPlayer.SafeInvoke ();
					}
					if (LastHeardWorldItem != null) {
						OnHearWorldItem.SafeInvoke ();
					}
					OnHearItemOfInterest.SafeInvoke ();
				} else {
					source.ListenerFailToHear ();
				}
			}
		}

		public static bool IsInAudibleRange (Vector3 listenerPosition, Vector3 audiblePosition, float listenerRange, float audibleRange)
		{
			//TODO implement distance fading
			float distance = Vector3.Distance (listenerPosition, audiblePosition);
			//the distance needs to be both within the listener's range AND the audible range
			return distance < (listenerRange + audibleRange);
		}

		public static bool IsAudible (Vector3 listenerPosition, Vector3 audiblePosition, float listenerSensitivity, float audibleVolume)
		{
			//TODO implement distance fading
			return audibleVolume >= listenerSensitivity;
		}

		#if UNITY_EDITOR
		public void OnDrawGizmos ()
		{
			HeardPlayerGizmo = Mathf.Lerp (HeardPlayerGizmo, 0.1f, (float) WorldClock.ARTDeltaTime);
			UnityEditor.Handles.color = Colors.Alpha (Color.green, HeardPlayerGizmo);
			UnityEditor.Handles.DrawWireDisc (worlditem.tr.position, Vector3.up, AwarenessDistanceTypeToAudibleDistance (State.AudioAwarnessDistance));
		}
		#endif

		protected float HeardPlayerGizmo;

		public static float AwarenessDistanceTypeToAudibleDistance (AwarnessDistanceType awarenessDistanceType)
		{
			float awarenessDistance = Globals.MaxAudibleRange;
			switch (awarenessDistanceType) {
			case AwarnessDistanceType.Poor:
				awarenessDistance *= 0.1f;
				break;

			case AwarnessDistanceType.Fair:
				awarenessDistance *= 0.25f;
				break;

			case AwarnessDistanceType.Good:
				awarenessDistance *= 0.5f;
				break;

			case AwarnessDistanceType.Excellent:
				awarenessDistance *= 0.75f;
				break;

			case AwarnessDistanceType.Prescient:
				awarenessDistance *= 1.0f;
				break;

			default:
				break;
			}
			return awarenessDistance;
		}

		public static float AwarenessDistanceTypeToAudibleSensitivity (AwarnessDistanceType awarenessDistanceType)
		{
			float sensitivity = 0.5f;
			switch (awarenessDistanceType) {
			case AwarnessDistanceType.Poor:
				sensitivity = 0.25f;
				break;

			case AwarnessDistanceType.Fair:
				sensitivity = 0.5f;
				break;

			case AwarnessDistanceType.Good:
				sensitivity = 0.75f;
				break;

			case AwarnessDistanceType.Excellent:
				sensitivity = 0.9f;
				break;

			case AwarnessDistanceType.Prescient:
				sensitivity = 1.0f;
				break;

			default:
				break;
			}
			return sensitivity;
		}

		protected MotileAction	mListenAction = null;
		protected MotileAction mFollowAction = null;
	}

	[Serializable]
	public class ListenerState
	{
		public string LastSpeechHeard = string.Empty;
		public string LastSpeechCommand = string.Empty;
		public string LastSoundHeard = string.Empty;
		public AwarnessDistanceType AudioAwarnessDistance = AwarnessDistanceType.Good;
		public AwarnessDistanceType AudioSensitivity = AwarnessDistanceType.Good;
	}
}
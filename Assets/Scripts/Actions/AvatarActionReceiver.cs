using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class AvatarActionReceiver : ActionFilter <PlayerAvatarAction>
{
		public override void WakeUp()
		{
				PlayerAvatarAction.LinkTypes();
				mListeners = new Dictionary<PlayerAvatarAction, List<ActionListener>>();
				mSubscriptionAdd = new SubscriptionAdd <PlayerAvatarAction>(SubscriptionAdd);
				mSubscriptionCheck = new SubscriptionCheck <PlayerAvatarAction>(SubscriptionCheck);
				mSubscribed = new PlayerAvatarAction(PlayerIDFlag.Local, AvatarActionType.NoAction, AvatarAction.NoAction);
				mSubscribersSet = true;
		}

		public void Subscribe(AvatarAction action, ActionListener listener)
		{
				gAction.Action = action;
				Subscribe(gAction, listener);
		}

		public void SubscriptionAdd(ref PlayerAvatarAction subscription, PlayerAvatarAction action)
		{
				if (action.Action != AvatarAction.NoAction) {
						subscription.ActionType |= action.ActionType;
						subscription.PlayerID	|= action.PlayerID;
				}
		}

		public bool SubscriptionCheck(PlayerAvatarAction subscription, PlayerAvatarAction action)
		{
				return Flags.Check((uint)subscription.ActionType, (uint)action.ActionType, Flags.CheckType.MatchAny);
				//&& Flags.Check <PlayerIDFlag> (subscription.PlayerID, action.PlayerID, Flags.CheckType.MatchAny));
		}

		public bool ReceiveAction(AvatarAction action, double timeStamp)
		{
				gAction.Action = action;
				return ReceiveAction(gAction, timeStamp);
		}

		public override bool ReceiveAction(PlayerAvatarAction action, double timeStamp)
		{
				return base.ReceiveAction(action, timeStamp);
		}

		protected static PlayerAvatarAction gAction = new PlayerAvatarAction(AvatarAction.NoAction);
}

[Serializable]
public struct PlayerAvatarAction : IEqualityComparer <PlayerAvatarAction>
{
		public PlayerAvatarAction(PlayerIDFlag playerID, AvatarActionType type, AvatarAction action)
		{
				PlayerID = playerID;
				ActionType = type;
				Action = action;
				Target = null;
		}

		public PlayerAvatarAction(PlayerIDFlag playerID, AvatarActionType type, AvatarAction action, GameObject target)
		{
				PlayerID = playerID;
				ActionType	= type;
				Action = action;
				Target = target;
		}

		public bool Equals(PlayerAvatarAction pa1, PlayerAvatarAction pa2)
		{
				return (pa1.Action == pa2.Action && Flags.Check((uint)pa1.PlayerID, (uint)pa2.PlayerID, Flags.CheckType.MatchAny));
		}

		public int GetHashCode(PlayerAvatarAction a)
		{
				return (int)a.ActionType + (int)a.Action;
		}

		public PlayerAvatarAction(AvatarAction action, GameObject target)
		{
				PlayerID = PlayerIDFlag.Local;
				if (!gActionTypes.TryGetValue(action, out ActionType)) {
						ActionType = AvatarActionType.NoAction;
						//Debug.Log ("Action " + action + " had no pair");
				}
				Action = action;
				Target = target;
		}

		public PlayerAvatarAction(AvatarAction action)
		{
				PlayerID = PlayerIDFlag.Local;
				if (!gActionTypes.TryGetValue(action, out ActionType)) {
						ActionType = AvatarActionType.NoAction;
						//Debug.Log ("Action " + action + " had no pair");
				}
				Action = action;
				Target = null;
		}

		[BitMask(typeof(PlayerIDFlag))]
		public PlayerIDFlag PlayerID;
		[BitMask(typeof(AvatarActionType))]
		public AvatarActionType ActionType;
		public AvatarAction Action;
		public GameObject Target;

		public static void LinkTypes()
		{
				if (gActionTypes.Count == 0) {
						List <string> avatarActionTypeNames = new List<string>();
						avatarActionTypeNames.AddRange(Enum.GetNames(typeof(AvatarActionType)));

						List <string> avatarActionNames = new List<string>();
						avatarActionNames.AddRange(Enum.GetNames(typeof(AvatarAction)));

						foreach (string avatarActionName in avatarActionNames) {
								foreach (string avatarActionTypeName in avatarActionTypeNames) {
										if (avatarActionName.Contains(avatarActionTypeName)) {
												AvatarAction action = (AvatarAction)Enum.Parse(typeof(AvatarAction), avatarActionName);
												AvatarActionType actionType = (AvatarActionType)Enum.Parse(typeof(AvatarActionType), avatarActionTypeName);
												gActionTypes.Add(action, actionType);
												////Debug.Log ("paired " + avatarActionName + " with " + avatarActionTypeName);
												break;
										}
								}
						}
				}
		}

		public static Dictionary <AvatarAction, AvatarActionType> gActionTypes = new Dictionary <AvatarAction, AvatarActionType>();
}
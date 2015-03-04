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
public struct PlayerAvatarAction : IEqualityComparer <PlayerAvatarAction>, IConvertible, IComparable, IFormattable
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

		#region interfaces
		//more nonsense to get around C#'s enum restrictions
		public Boolean ToBoolean (IFormatProvider provider) {
				return false;
		}
		public Byte ToByte (IFormatProvider provider) {
				return 0;
		}
		public SByte ToSByte (IFormatProvider provider) {
				return 0;
		}
		public Char ToChar (IFormatProvider provider) {
				return '0';
		}
		public DateTime ToDateTime (IFormatProvider provider) {
				return DateTime.Now;
		}
		public Single ToSingle (IFormatProvider provider) {
				return 0.0f;
		}
		public Decimal ToDecimal (IFormatProvider provider) {
				return 0.0m;
		}
		public Double ToDouble (IFormatProvider provider) {
				return 0.0;
		}
		public Int16 ToInt16 (IFormatProvider provider) {
				return 0;
		}
		public Int32 ToInt32 (IFormatProvider provider) {
				return 0;
		}
		public Int64 ToInt64 (IFormatProvider provider) {
				return (int)Action;
		}
		public System.Object ToType (System.Type type, IFormatProvider provider) {
				return type;
		}
		public UInt16 ToUInt16 (IFormatProvider provider) {
				return 0;
		}
		public UInt32 ToUInt32 (IFormatProvider provider) {
				return (uint)Action;
		}
		public UInt64 ToUInt64 (IFormatProvider provider) {
				return (uint)Action;
		}
		public String ToString (string s, IFormatProvider provider) {
				return ToString();
		}
		public String ToString (IFormatProvider provider) {
				return ToString();
		}
		public TypeCode GetTypeCode ( ) {
				return TypeCode.UInt32;
		}

		public int CompareTo(PlayerAvatarAction o)
		{
				return Action.CompareTo (o.Action);
		}

		public int CompareTo(System.Object o)
		{
				PlayerAvatarAction other = (PlayerAvatarAction)o;
				if (o == null) {
						return 0;
				}
				return Action.CompareTo (other.Action);
		}

		public bool Equals(PlayerAvatarAction pa1, PlayerAvatarAction pa2)
		{
				return (pa1.Action == pa2.Action && Flags.Check((uint)pa1.PlayerID, (uint)pa2.PlayerID, Flags.CheckType.MatchAny));
		}

		public int GetHashCode(PlayerAvatarAction a)
		{
				return (int)a.ActionType + (int)a.Action;
		}
		#endregion

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
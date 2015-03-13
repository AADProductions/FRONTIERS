using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		public class UserActionReceiver : ActionFilter <UserActionType>
		{
				public override void WakeUp()
				{
						mListeners = new Dictionary<UserActionType, List<ActionListener>>(EnumComparer<UserActionType>.Instance);
						mSubscriptionAdd = new SubscriptionAdd <UserActionType>(SubscriptionAdd);
						mSubscriptionCheck	= new SubscriptionCheck <UserActionType>(SubscriptionCheck);
						mSubscribed = UserActionType.NoAction;
						mDefault = UserActionType.NoAction;

						if ((int)Filter == 0) {
								Filter = UserActionType.NoAction;
						}
						if ((int)FilterExceptions == 0) {
								FilterExceptions = UserActionType.NoAction;
						}
						mSubscribersSet = true;
						base.WakeUp();
				}

				public void SubscriptionAdd(ref UserActionType subscription, UserActionType action)
				{
						if (action != UserActionType.NoAction) {
								subscription |= action;
						}
				}

				public bool SubscriptionCheck(UserActionType subscription, UserActionType action)
				{
						if (subscription != UserActionType.NoAction && action != UserActionType.NoAction) {
								return Flags.Check((uint)subscription, (uint)action, Flags.CheckType.MatchAny);
						}
						return false;
				}
		}
}
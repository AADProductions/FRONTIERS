using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		public class InterfaceActionFilter : ActionFilter <InterfaceActionType>
		{
				public PauseBehavior Pause = PauseBehavior.PassThrough;
				public bool HideCrosshair = false;
				public bool ShowCursor = false;
				public bool SuspendMessages = false;

				public override void WakeUp()
				{
						mListeners = new Dictionary<InterfaceActionType, List<ActionListener>>(EnumComparer<InterfaceActionType>.Instance);
						mSubscriptionCheck = new SubscriptionCheck <InterfaceActionType>(SubscriptionCheck);
						mSubscriptionAdd = new SubscriptionAdd <InterfaceActionType>(SubscriptionAdd);
						mSubscribed = InterfaceActionType.NoAction;
						mDefault = InterfaceActionType.NoAction;
						mSubscribersSet = true;
						base.WakeUp();
				}

				protected void SubscriptionAdd(ref InterfaceActionType subscription, InterfaceActionType action)
				{
						subscription |= action;
				}

				protected bool SubscriptionCheck(InterfaceActionType subscription, InterfaceActionType action)
				{
						return Flags.Check((uint)subscription, (uint)action, Flags.CheckType.MatchAny);
				}
		}
}
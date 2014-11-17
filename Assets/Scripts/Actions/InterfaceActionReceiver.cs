using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers {
	public class InterfaceActionFilter : ActionFilter <InterfaceActionType>
	{
		public PauseBehavior Pause = PauseBehavior.PassThrough;
		public bool HideCrosshair = false;
		public bool ShowCursor = false;
		public bool SuspendMessages = false;

		public override void Awake ()
		{
			mSubscriptionCheck = new SubscriptionCheck <InterfaceActionType> (SubscriptionCheck);
			mSubscriptionAdd = new SubscriptionAdd <InterfaceActionType> (SubscriptionAdd);
			mSubscribed = InterfaceActionType.NoAction;
			mDefault = InterfaceActionType.NoAction;
			mSubscribersSet = true;
			base.Awake ();
		}

		protected void SubscriptionAdd (ref InterfaceActionType subscription, InterfaceActionType action)
		{
			subscription |= action;
		}

		protected bool SubscriptionCheck (InterfaceActionType subscription, InterfaceActionType action)
		{
			return Flags.Check <InterfaceActionType> (subscription, action, Flags.CheckType.MatchAny);
		}
	}

	public enum PauseBehavior {
		Pause,
		DoNotPause,
		PassThrough,
	}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class TimeActionReceiver : ActionFilter <TimeActionType>
{
	public override void Awake ()
	{
		mSubscriptionAdd = new SubscriptionAdd <TimeActionType> (SubscriptionAdd);
		mSubscriptionCheck = new SubscriptionCheck <TimeActionType> (SubscriptionCheck);
		mSubscribed = TimeActionType.NoAction;
		mDefault = TimeActionType.NoAction;
		
		if ((int)Filter == 0) {
			Filter = TimeActionType.NoAction;
		}
		if ((int)FilterExceptions == 0) {
			FilterExceptions = TimeActionType.NoAction;
		}
		mSubscribersSet = true;
		base.Awake ();
	}

	public void SubscriptionAdd (ref TimeActionType subscription, TimeActionType action)
	{
		if (action != TimeActionType.NoAction) {
			subscription |= action;
		}
	}

	public bool SubscriptionCheck (TimeActionType subscription, TimeActionType action)
	{
		if (subscription != TimeActionType.NoAction && action != TimeActionType.NoAction) {
			return Flags.Check ((uint)subscription, (uint)action, Flags.CheckType.MatchAny);
		}
		return false;
	}

	public override void Update ()
	{
		if (!GameManager.Is (FGameState.InGame))
			return;

		base.Update ();
	}
}

[Flags]
public enum TimeActionType
{
	NoAction = 0,
	DaytimeStart = 1,
	NightTimeStart = 2,
	HourStart = 4,
}
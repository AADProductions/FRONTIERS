using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers;
using System;

public class DarkrotManager : Manager
{
	public GameObject DarkrotNodePrefab;

	public override void OnGameStart ()
	{
		WorldClock.Get.TimeActions.Subscribe (TimeActionType.DaytimeStart, new ActionListener (DaytimeStart));
		WorldClock.Get.TimeActions.Subscribe (TimeActionType.NightTimeStart, new ActionListener (NightTimeStart));
	}

	public bool NightTimeStart (double timeStamp)
	{
		return true;
	}

	public bool DaytimeStart (double timeStamp)
	{
		return true;
	}
}
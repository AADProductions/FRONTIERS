using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class Guard : WIScript
	{
		public static List <string> MovementFlags = new List<string> () { "Guard" };

		public GuardState State = new GuardState ();

		public override void OnInitialized ()
		{
			DailyRoutine dailyRoutine = null;
			if (worlditem.Has <DailyRoutine> (out dailyRoutine)) {
				if (!State.UseDailyRoutine) {
					dailyRoutine.Finish ();
				} else {
					FlagSet occupationFlags = null;
					GameWorld.Get.FlagSetByName ("Occupation", out occupationFlags);
					dailyRoutine.OccupationFlags = occupationFlags.GetFlagValue (MovementFlags);
				}
			}
		}
	}

	[Serializable]
	public class GuardState {
		public bool UseDailyRoutine = false;
	}
}
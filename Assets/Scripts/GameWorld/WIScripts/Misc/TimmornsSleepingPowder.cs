using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts
{
	public class TimmornsSleepingPowder : WIScript {

		public string QuestItemName = "FamilyMessHallCauldron";
		public string QuestItemState = "Poisoned";

		public override void OnInitialized ()
		{
			worlditem.OnPlayerUse += OnPlayerUse;
		}

		public void OnPlayerUse ()
		{
			Debug.Log ("On player interact in timmorns brew");
			if (worlditem.Is (WIMode.Equipped)) {
				Debug.Log ("We're equipped");
				//the only reason this will happen is if we're prompted by something
				QuestItem questitem = null;
				if (Player.Local.Surroundings.IsWorldItemInPlayerFocus && Player.Local.Surroundings.WorldItemFocus.worlditem.Is <QuestItem> (out questitem)) {
					questitem.worlditem.State = QuestItemState;
					Debug.Log ("Setting questitem state to " + QuestItemState);
						worlditem.SetMode (WIMode.RemovedFromGame);
						Finish ();
				}
			}
		}
	}
}
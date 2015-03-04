using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{	//this script addresses a strange problem I ran into while scripting Legacy
	//conversations are supposed to change either directly or due to structure loading
	//we found that indress needs her conversation to change while you're in the same structure
	//rather than reorganizing the entire convo system I opted to write this band-aid fix
	public class Indress : WIScript
	{
		public string MissionName = "Legacy";
		public string ObjectiveName = "ReturnToIndress";
		public MissionStatus ObjectiveStatus = MissionStatus.Active;
		public string ConversationOnObjectiveStatus = "Guildmaster-Enc-Act-01-Legacy-01";
		Talkative talkative;

		public override void OnInitialized ()
		{
			talkative = worlditem.Get <Talkative> ();

			bool completed = false;
			if (Missions.Get.MissionCompletedByName (MissionName, ref completed) && completed) {
				//don't set the convo, it's already set, just move on
				Finish ();
			} else {
				CheckObjectiveStatus (WorldClock.AdjustedRealTime);
				Player.Get.AvatarActions.Subscribe (AvatarAction.MissionObjectiveActiveate, CheckObjectiveStatus);
			}
		}

		public bool CheckObjectiveStatus (double timeStamp) {
			MissionStatus objectiveStatusCheck = MissionStatus.Dormant;
			if (Missions.Get.ObjectiveStatusByName (MissionName, ObjectiveName, ref objectiveStatusCheck)) {
				if (Flags.Check ((uint)ObjectiveStatus, (uint)objectiveStatusCheck, Flags.CheckType.MatchAny)) {
					//we've hit the objective status check, set our convo now
					Debug.Log ("Objective " + ObjectiveName + " is of status " + ObjectiveStatus.ToString () + " so indress is changing to " + ConversationOnObjectiveStatus);
					talkative.State.ConversationName = ConversationOnObjectiveStatus;
				}
			}
			return true;
		}
	}
}
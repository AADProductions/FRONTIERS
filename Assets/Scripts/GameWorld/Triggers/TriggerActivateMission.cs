using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;

namespace Frontiers.World
{
		public class TriggerActivateMission : WorldTrigger
		{
				public TriggerActivateMissionState State = new TriggerActivateMissionState();

				public override bool OnPlayerEnter()
				{
						if (!string.IsNullOrEmpty(State.ActivatedObjectiveName)) {
								Missions.Get.ActivateObjective(State.ActivatedMissionName, State.ActivatedObjectiveName, State.ActivatedOriginType, State.ActivatedOriginName);
						} else if (!string.IsNullOrEmpty(State.ActivatedMissionName)) {
								Missions.Get.ActivateMission(State.ActivatedMissionName, State.ActivatedOriginType, State.ActivatedOriginName);
						}
						return true;
				}
		}

		[Serializable]
		public class TriggerActivateMissionState : WorldTriggerState
		{
				public string ActivatedMissionName = string.Empty;
				public string ActivatedObjectiveName = string.Empty;
				public MissionOriginType ActivatedOriginType = MissionOriginType.Location;
				public string ActivatedOriginName = string.Empty;
		}
}
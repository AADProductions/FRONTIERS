using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class MissionActivator : WIScript
		{
				public MissionActivatorState State = new MissionActivatorState();

				public void OnVisit()
				{
						if (State.OnVisit) {
								Activate();
						}
				}

				public void OnVisitFirstTime()
				{
						if (State.OnVisitFirstTime) {
								Activate();
						}			
				}

				public void OnLeave()
				{
						if (State.OnLeave) {
								Activate();
						}			
				}

				public void OnLeaveFirstTime()
				{
						if (State.OnLeaveFirstTime) {
								Activate();
						}
				}

				protected void Activate()
				{
						if (State.HasActivatedOnce && State.OnceOnly) {
								return;
						}
			
						State.HasActivatedOnce = true;
			
						string originName = worlditem.DisplayName;
						if (!string.IsNullOrEmpty(State.OriginNameOverride)) {
								originName = State.OriginNameOverride;
						}
			
						switch (State.Type) {
								case MissionActivatorType.Mission:
								default:
										Missions.Get.ActivateMission(State.MissionName, State.OriginType, originName);
										break;
				
								case MissionActivatorType.Objective:
										Missions.Get.ActivateObjective(State.MissionName, State.ObjectiveName, State.OriginType, originName);
										break;
						}
				}
		}

		[Serializable]
		public class MissionActivatorState
		{
				public MissionActivatorType Type = MissionActivatorType.Mission;
				public string MissionName = "MissionName";
				public string ObjectiveName = "Objective";
				public MissionOriginType OriginType = MissionOriginType.None;
				public string OriginNameOverride	= "MissionActivator";
				public bool HasActivatedOnce	= false;
				public bool OnceOnly = true;
				//when to trigger
				public bool OnVisit = false;
				public bool OnVisitFirstTime	= false;
				public bool OnLeave = false;
				public bool OnLeaveFirstTime	= false;
		}
}
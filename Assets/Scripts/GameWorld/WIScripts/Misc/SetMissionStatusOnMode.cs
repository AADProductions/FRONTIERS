using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class SetMissionStatusOnMode : WIScript
		{		//used for missions where you have to pick something up
				//or destroy it or whatever
				public WIMode Mode = WIMode.Stacked;
				public Mission AffectedMission;
				public MissionStatus Status = MissionStatus.Dormant;

				public override void OnModeChange()
				{
						if (AffectedMission != null) {
								if (worlditem.Mode == Mode) {
										AffectedMission.State.Status = Status;
										GameObject.Destroy(this);
								}
						}
				}
		}
}
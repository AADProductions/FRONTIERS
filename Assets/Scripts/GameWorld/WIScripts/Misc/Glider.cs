using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		//this class is pretty simple for now
		//will undoubtedly add more control tweaking based on user feedback
		public class Glider : WIScript
		{
				public GliderState State = new GliderState();

				public override void OnInitialized()
				{
						Vehicle vehicle = worlditem.Get <Vehicle>();
						vehicle.OnMount += OnMount;
						vehicle.OnDismount += OnDismount;
				}

				public void OnMount()
				{
						enabled = true;
						worlditem.State = "Mounted";
						if (!string.IsNullOrEmpty(State.MissionName)) {
								Missions.Get.ChangeVariableValue(State.MissionName, State.VariableName, State.VariableValue, State.ChangeType);
								State.MissionName = null;
						}
				}

				public void OnDismount()
				{
						enabled = false;
						worlditem.State = "Unpacked";
				}
		}

		[Serializable]
		public class GliderState
		{
				public string MissionName = "Family";
				public string VariableName = "HasFlownGlider";
				public int VariableValue = 1;
				public ChangeVariableType ChangeType = ChangeVariableType.SetValue;
		}
}
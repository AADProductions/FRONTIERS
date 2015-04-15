using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
	public class ChangeVariableOnExamine : WIScript {

		public ChangeVariableOnExamineState State = new ChangeVariableOnExamineState ();

		public override void OnInitialized ()
		{
			worlditem.OnExamine += OnExamine;
		}

		public void OnExamine ()
		{
			if (Missions.Get.ChangeVariableValue (State.MissionName, State.VariableName, State.VariableValue, State.ChangeType)) {
				Finish ();
			}
		}
	}

	[Serializable]
	public class ChangeVariableOnExamineState {
		[FrontiersAvailableModsAttribute ("Mission")]
		public string MissionName = string.Empty;
		public string VariableName = string.Empty;
		public int VariableValue = 1;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;
	}
}
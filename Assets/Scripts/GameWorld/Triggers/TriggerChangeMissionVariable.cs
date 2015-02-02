using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class TriggerChangeMissionVariable : WorldTrigger
		{
				public TriggerChangeMissionVariableState State = new TriggerChangeMissionVariableState();

				public override bool OnPlayerEnter()
				{
						Missions.Get.ChangeVariableValue(State.ChangeMissionName, State.ChangeVariableName, State.ChangeVariableValue, State.ChangeVariableType);
						if (!string.IsNullOrEmpty(State.Introspection)) {
								GUI.GUIManager.PostIntrospection(State.Introspection);
						}
						return true;
				}
		}

		[Serializable]
		public class TriggerChangeMissionVariableState : WorldTriggerState
		{
				public string ChangeMissionName = string.Empty;
				public string ChangeVariableName = string.Empty;
				public int ChangeVariableValue = 1;
				public ChangeVariableType ChangeVariableType = ChangeVariableType.Increment;
				public string Introspection = string.Empty;
		}
}
using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.Gameplay;
using System;
using Frontiers.Data;

namespace Frontiers.World {
	public class ChangeMissionVariable : WIScript {
		public ChangeMissionVariableState State = new ChangeMissionVariableState ( );

		public override void OnInitialized ()
		{
			worlditem.OnAddedToPlayerInventory += OnAddedToPlayerInventory;
		}

		public void OnStateChange ( ) {
			if (!string.IsNullOrEmpty (State.OnState) && worlditem.State == State.OnState) {
				if (!string.IsNullOrEmpty (State.VariableEval)) {
					State.VariableValue = GameData.Evaluate (State.VariableEval, null);
				}
				Missions.Get.ChangeVariableValue (State.MissionName, State.VariableName, State.VariableValue, State.ChangeType);
				Finish ();
			}
		}

		public void OnAddedToPlayerInventory ( ) {
			if (State.ChangeOnAddedToPlayerInventory) {
				if (!string.IsNullOrEmpty (State.VariableEval)) {
					State.VariableValue = GameData.Evaluate (State.VariableEval, null);
				}
				Missions.Get.ChangeVariableValue (State.MissionName, State.VariableName, State.VariableValue, State.ChangeType);
				Finish ();
			}
		}
	}

	[Serializable]
	public class ChangeMissionVariableState {
		public string MissionName = string.Empty;
		public string VariableName = string.Empty;
		public int VariableValue = 0;
		public string VariableEval = string.Empty;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;
		public string OnState;
		public bool ChangeOnAddedToPlayerInventory = true;
	}
}

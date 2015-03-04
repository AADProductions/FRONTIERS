using UnityEngine;
using System.Collections;
using System;
using Frontiers;
using Frontiers.Data;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World {
	public class ChangeMissionVariableOnReceptacleChanged : WIScript {
		public ChangeMissionVariableOnReceptacleChangedState State = new ChangeMissionVariableOnReceptacleChangedState ( );

		Receptacle recepticle;

		public override void OnInitialized ()
		{
			Debug.Log ("On initialized");
			recepticle = worlditem.Get <Receptacle> ();
			recepticle.OnItemPlacedInReceptacle += OnItemPlacedInReceptacle;
		}

		public void OnItemPlacedInReceptacle ( ) {
			Debug.Log ("Item placed in recepticle");
			if (State.OnQuestItemAdded) {
				if (recepticle.ContainsQuestItem (State.QuestItemName)) {
					Debug.Log ("Receptacle contains quest item, changing mission variable");
					ChangeMissionVariable ();
				}
			}
		}

		public void ChangeMissionVariable ( ) {
			Debug.Log ("Changing mission varaible and finishing");
			if (!string.IsNullOrEmpty (State.VariableEval)) {
				State.VariableValue = GameData.Evaluate (State.VariableEval, null);
			}
			Missions.Get.ChangeVariableValue (State.MissionName, State.VariableName, State.VariableValue, State.ChangeType);
			if (State.LockReceptacleOnItemAdded) {
				recepticle.SetLocked (true);
			}
			Finish ();
		}
	}

	[Serializable]
		public class ChangeMissionVariableOnReceptacleChangedState {
		public string MissionName;
		public string VariableName;
		public int VariableValue = 0;
		public string VariableEval;
		public ChangeVariableType ChangeType = ChangeVariableType.Increment;

		public bool OnQuestItemAdded = true;
		public string QuestItemName;
		public bool LockReceptacleOnItemAdded = true;
	}
}

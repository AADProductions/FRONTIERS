using UnityEngine;
using System.Collections;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers.World {
	public class MissionInteriorTesting : MonoBehaviour {
		public MissionInteriorControllerState State = new MissionInteriorControllerState ();
		public Structure structure = null;
		public MissionInteriorCondition LastTopCondition = null;

		public void TestState ( ) {
			MissionInteriorCondition newTopCondition = GetTopCondition ();
		}

		public MissionInteriorCondition GetTopCondition ( )
		{
			int topConditionIndex = 0;
			MissionInteriorCondition topCondition = null;
			if (MissionCondition.CheckConditions <MissionInteriorCondition> (State.Conditions, out topConditionIndex)) {
				topCondition = State.Conditions [topConditionIndex];
				//Debug.Log("Choosing condition " + topConditionIndex.ToString () + " in " + name + " - " + topCondition.MissionName + " is " + topCondition.Status.ToString ( ) + ", " + topCondition.ObjectiveName + " is " + topCondition.ObjectiveStatus.ToString ( ));
			} else {
				//Debug.Log("Choosing default in " + name);
				topCondition = State.Default;
			}
			return topCondition;
		}
	}
}
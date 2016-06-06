using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using System;

namespace Frontiers
{
	public class PlayerCritters : PlayerScript {
		public PlayerCrittersState State = new PlayerCrittersState ();

		void Update () {
			if (Input.GetKeyDown (KeyCode.C)) {
				CritterSaveState newCritterState = new CritterSaveState ();
				newCritterState.Name = "New Critter Blah " + State.Critters.Count.ToString ();
				newCritterState.Type = "Butterfly";
				newCritterState.Coloration = UnityEngine.Random.Range (0, 3);
				State.Critters.Add (newCritterState);
				Critters.Get.SpawnFriendlyFromSaveState (newCritterState);
			}
		}

		public override void OnLocalPlayerSpawn ()
		{
			for (int i = 0; i < State.Critters.Count; i++) {
				Critters.Get.SpawnFriendlyFromSaveState (State.Critters [i]);	
			}
		}
	}

	[Serializable]
	public class PlayerCrittersState {
		public List <CritterSaveState> Critters = new List <CritterSaveState> ();
	}

	[Serializable]
	public class CritterSaveState {
		public string Name = "Critter";
		public string Type = "Butterfly";
		public int Coloration = 0;
		public double TimeCreated = 0;
	}
}
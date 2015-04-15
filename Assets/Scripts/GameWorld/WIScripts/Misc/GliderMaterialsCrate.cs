using UnityEngine;
using System.Collections;
using System;
using Frontiers;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class GliderMaterialsCrate : WIScript {
		public string MaterialsObjective;
		public string MaterialsMission;
		public bool MissionCompleted;

		public List <Renderer> MaterialRenderers = new List <Renderer> ( );

		public override void OnInitialized ()
		{
			worlditem.OnActive += OnActive;
			Player.Get.AvatarActions.Subscribe (AvatarAction.MissionUpdated, new ActionListener (MissionUpdated));
		}

		public bool MissionUpdated (double timeStamp) {
			RefreshCrateProps ();
			return true;
		}

		public void OnActive ( ) {
			RefreshCrateProps ();
		}

		protected void RefreshCrateProps () {
			if (mDestroyed || mFinished) {
				return;
			}

			if (!MissionCompleted) {
				Missions.Get.ObjectiveCompletedByName (MaterialsMission, MaterialsObjective, ref MissionCompleted);
			}

			for (int i = 0; i < MaterialRenderers.Count; i++) {
				MaterialRenderers [i].enabled = MissionCompleted;
			}
		}
	}
}
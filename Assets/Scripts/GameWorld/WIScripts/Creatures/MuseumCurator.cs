using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World {
	public class MuseumCurator : WIScript {
		public MuseumCuratorState State = new MuseumCuratorState ( );

		public override void OnInitialized ()
		{
			//after conversations check the state of each museum item in case something changed
			Player.Get.AvatarActions.Subscribe (AvatarAction.NpcConverseEnd, NpcConverseEnd);
			Museums.Get.ActiveCurator = this;
		}

		public bool NpcConverseEnd (double timeStamp) {
			RefreshMuseumPieces ();
			return true;
		}

		protected void RefreshMuseumPieces ( ) {

		}
	}

	[Serializable]
	public class MuseumCuratorState {

	}
}

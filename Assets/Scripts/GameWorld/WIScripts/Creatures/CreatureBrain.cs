using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
	public class CreatureBrain : WIScript {
		public CreatureBrainState State = new CreatureBrainState ( );
	}

	[Serializable]
	public class CreatureBrainState {

	}
}
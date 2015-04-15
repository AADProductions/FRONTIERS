using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using System;

namespace Frontiers.World.WIScripts
{
	public class Landmark : WIScript
	{			
		public Signboard Sign;

		public LandmarkState State = new LandmarkState ( );

	}

	[Serializable]
	public class LandmarkState {

	}
}
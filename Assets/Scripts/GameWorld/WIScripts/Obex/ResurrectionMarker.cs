using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class ResurrectionMarker : WIScript {

		public ResurrectionMarkerState State = new ResurrectionMarkerState ( );

		public bool CanBeActivated {
			get {
				return !State.HasBeenActivated;
			}
		}

		public override bool CanEnterInventory {
			get {
				return !State.HasBeenActivated;
			}
		}

		public override bool CanBeCarried {
			get {
				return !State.HasBeenActivated;
			}
		}

		public bool ActivateMarker (Skill skill)
		{
			Resurrect resurrect = skill as Resurrect;
			if (resurrect != null) {
				resurrect.Extensions.ResurrectionMarker = worlditem.StaticReference;
				return true;
			}
			return false;
		}
	}

	[Serializable]
	public class ResurrectionMarkerState  {
		public bool HasBeenActivated;
	}
}
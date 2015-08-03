using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using Frontiers.Story;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class TriggerObexGhost : WorldTrigger
	{
		public TriggerObexGhostState State = new TriggerObexGhostState ();

		public override bool OnPlayerEnter ()
		{
			ObexGhost ghost = null;
			ObexGhost [] ghosts = GameObject.FindObjectsOfType <ObexGhost> ();
			for (int i = 0; i < ghosts.Length; i++) {
				if (ghosts [i].worlditem.FileName.Equals (State.ObexGhostName)) {
					ghost = ghosts [i];
				}
			}
			Array.Clear (ghosts, 0, ghosts.Length);
			ghosts = null;

			if (ghost != null) {
				StartCoroutine (ActivateGhostOverTime (ghost));
				return true;
			}
			return false;
		}

		protected IEnumerator ActivateGhostOverTime (ObexGhost ghost) {
			double waitUntil = WorldClock.AdjustedRealTime + State.TriggerDelay;
			while (WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			if (ghost != null) {
				ghost.ActivateGhost ();
			}
			yield break;
		}
	}

	[Serializable]
	public class TriggerObexGhostState : WorldTriggerState
	{
		public string ObexGhostName = string.Empty;
		public float TriggerDelay = 1f;
	}
}
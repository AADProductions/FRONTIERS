using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;

namespace Frontiers.World.BaseWIScripts
{
		public class Revealable : WIScript
		{
				public RevealableState State = new RevealableState();
				public Action OnReveal;

				public override void OnInitialized()
				{
						Visitable visitable = null;
						if (worlditem.Is<Visitable>(out visitable)) {
								visitable.OnPlayerVisitFirstTime += OnPlayerVisitFirstTime;
						}
						worlditem.OnPlayerEncounter += OnPlayerVisitFirstTime;
						worlditem.OnActive += OnPlayerVisitFirstTime;
				}

				public bool Reveal(LocationRevealMethod method)
				{
						if (State.RevealMethod == LocationRevealMethod.None) {
								State.RevealMethod = method;
								State.TimeRevealed	= WorldClock.AdjustedRealTime;
								OnReveal.SafeInvoke();
								Player.Local.Surroundings.Reveal(worlditem.StaticReference);
								return true;
						}
						return false;
				}

				public void OnPlayerVisitFirstTime()
				{
						Reveal(LocationRevealMethod.ByDefault);
				}
		}

		[Serializable]
		public class RevealableState
		{
				public bool HasBeenRevealed {
						get {
								return RevealMethod != LocationRevealMethod.None;
						}
				}

				public LocationRevealMethod RevealMethod = LocationRevealMethod.None;
				public double TimeRevealed = 0.0f;
				public bool UnknownUntilVisited = false;
				public bool MarkedForTriangulation = false;
				public bool CustomMapSettings = false;
				public SVector3 PlayerPositionWhenMarked = new SVector3();
				//map info
				public string IconName = "Outpost";
				public SColor IconColor;
				public MapIconStyle IconStyle = MapIconStyle.None;
				public MapLabelStyle LabelStyle = MapLabelStyle.None;
				public SVector3 IconOffset = SVector3.zero;
		}
}
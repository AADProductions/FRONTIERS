using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class Perishable : WIScript
		{
				public string RottenPrefix = "Spoiled";
				public bool StartRottingOnCreate	= true;
				public double TimeToRot = 1000.0f;
				public double RotStartTime = Mathf.NegativeInfinity;
				public Material	RottenMaterial;

				public double TimeRottedSoFar {
						get {
								if (RotStartTime > 0.0f) {
										return WorldClock.AdjustedRealTime - RotStartTime;
								}
								return 0.0f;
						}
				}

				public bool Rotten = false;

				public void	Start()
				{
						if (Rotten) {
								RotImmediately();
						}
			
						if (StartRottingOnCreate) {
								RotOverTime();
						}
				}

				public void RotImmediately()
				{
						Rotten = true;
			
						if (!transform.name.Contains(RottenPrefix)) {
								transform.name = RottenPrefix + " " + transform.name;
						}
			
						renderer.material = RottenMaterial;
				}

				public void RotOverTime()
				{
						if (Rotten) {
								return;
						}
			
						if (RotStartTime < 0) {
								RotStartTime = WorldClock.AdjustedRealTime;
						}
			
						if (TimeRottedSoFar > TimeToRot) {
								RotImmediately();
						}
				}
		}
}
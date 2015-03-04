using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World {
	public class NorthernLights : WIScript {
		public List <ParticleSystem> Particles = new List <ParticleSystem> ( );
		public bool Active = true;

		public override void OnInitialized ()
		{
//			worlditem.OnVisible += OnVisible;
//			worlditem.OnInvisible += OnInvisible;
		}

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public void Update ( ) {
			if (WorldClock.IsNight && worlditem.Is (WIActiveState.Visible | WIActiveState.Active)) {
				if (!Active) {
					for (int i = 0; i < Particles.Count; i++) {
						Particles [i].enableEmission = true;
					}
					Active = true;
				}
			} else {
				if (Active) {
					for (int i = 0; i < Particles.Count; i++) {
						Particles [i].enableEmission = false;
					}
				}
			}
		}
	}
}

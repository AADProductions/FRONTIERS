using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World {
	public class TrapObject : MonoBehaviour {
				//hunter-seeker type thing that gets spawned by traps
		public Trap ParentTrap;
		public Rigidbody rb;
		public double TimeStarted;
		public double Lifetime;
		public string FXOnSelfDestruct;
		public MasterAudio.SoundType SoundType;
		public string SoundOnTriggerEnter;
		public string SoundOnSelfDestruct;
		public bool SelfDestructOnTriggerEnter = true;
		public bool FollowPlayer = true;
		public bool RequireLineOfSight = true;
		public bool RequirePlayerVisible = true;
		public float MinimumVisibility = 0.01f;
		public float FollowStrength = 0.5f;

		public void Start ( ) {
			TimeStarted = WorldClock.AdjustedRealTime;
			rb = gameObject.rigidbody;
		}

		public void OnTriggerEnter (Collider other) {
			if (ParentTrap != null) {
				Debug.Log ("Entered collision, passing along to parent trap");
				if (ParentTrap.OnTrapObjectTriggerEnter (other)) {
					MasterAudio.PlaySound (SoundType, SoundOnTriggerEnter);
					if (SelfDestructOnTriggerEnter) {
						SelfDestruct ();
					}
				}
			}
		}

		public virtual void Update ( ) {
			if (WorldClock.AdjustedRealTime > TimeStarted + Lifetime) {
				SelfDestruct ();
			}
		}

		protected static RaycastHit mFollowPlayerHit;

		public virtual void FixedUpdate ( ) {
			if (FollowPlayer) {
				if (RequireLineOfSight && Physics.Linecast (Player.Local.Position, transform.position, out mFollowPlayerHit, Globals.LayerStructureTerrain | Globals.LayerSolidTerrain)) {
					return;
				}
				if (RequirePlayerVisible && !(Player.Local.IsVisible && Player.Local.VisibilityMultiplier > MinimumVisibility)) {
					return;
				}
				rb.AddForce (Vector3.Normalize (Player.Local.ChestPosition - transform.position) * FollowStrength);
			}
		}

		void SelfDestruct ()
		{
			FXManager.Get.SpawnFX (transform.position, FXOnSelfDestruct);
			MasterAudio.PlaySound (SoundType, transform, SoundOnSelfDestruct);
			GameObject.Destroy (gameObject, 0.05f);
			enabled = false;
		}
	}
}
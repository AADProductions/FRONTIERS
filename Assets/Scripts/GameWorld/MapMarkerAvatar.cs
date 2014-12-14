using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class MapMarkerAvatar : MonoBehaviour
		{
				public ParticleEmitter BeaconLines;
				public ParticleEmitter LightbeamsUp;
				public ParticleEmitter GroundFlash;
				public ParticleEmitter RuneCircleUp;
				public float BeaconDistance = 25f;
				public int UpdateParticles;
				public Transform tr;

				public void Awake()
				{
						tr = transform;
				}

				public void FixedUpdate()
				{
						UpdateParticles++;
						if (UpdateParticles > 30) {
								if (Vector3.Distance(Player.Local.Position, tr.position) < BeaconDistance) {
										BeaconLines.emit = false;
								} else {
										BeaconLines.emit = true;
								}
						}
				}
		}
}
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MaxParticleSizeNew : MonoBehaviour
{
		public float maxSize = 20;
		public static ParticleSystem.Particle[] particles;
		protected ParticleSystem system;

		void Start()
		{
				system = gameObject.GetComponent <ParticleSystem>();
				if (particles == null) {
						particles = new ParticleSystem.Particle[1024];
				}
		}

		void  Update()
		{
				int numParticles = system.GetParticles(particles);
				for (int i = 0; i < numParticles; i++) {
						if (particles[i].size > maxSize) {
								particles[i].size = maxSize;
						}
				}
				system.SetParticles(particles, numParticles);
		}
}
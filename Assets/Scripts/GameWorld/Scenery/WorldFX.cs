using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;

public class WorldFX : MonoBehaviour {

	public List <Renderer> Renderers = new List<Renderer> ( );
	public List <ParticleSystem> Particles = new List<ParticleSystem> ( );
	public List <Light> Lights = new List <Light> ( );
	public SphereCollider Collider;

	public void Awake ( )
	{
		Collider = GetComponent <SphereCollider> ();
		Collider.isTrigger = true;
		gameObject.layer = Globals.LayerNumScenery;
	}

	public void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.layer == Globals.LayerNumPlayer) {
			EnableFX ();
		}
	}

	public void OnTriggerExit (Collider other)
	{
		if (other.gameObject.layer == Globals.LayerNumPlayer) {
			DisableFX ();
		}
	}

	public void EnableFX ( )
	{
		foreach (Renderer renderer in Renderers) {
			renderer.enabled = true;
		}
		foreach (ParticleSystem particle in Particles) {
			particle.enableEmission = true;
		}
		foreach (Light light in Lights) {
			light.enabled = true;
		}
	}

	public void DisableFX ( )
	{
		foreach (Renderer renderer in Renderers) {
			renderer.enabled = false;
		}
		foreach (ParticleSystem particle in Particles) {
			particle.enableEmission = false;
		}
		foreach (Light light in Lights) {
			light.enabled = false;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class HideWithMagic : MonoBehaviour
{
		public SphereCollider Trigger;
		public float SendAwayDistance = 15.0f;
		public AudioClip MagicEffectLoop;
		public AudioClip SendAwayClip;
		public AudioClip DispelClip;
		public AudioSource Audio;
		public ParticleSystem Particles;
		public GameObject MagicBarrier;
		public List <GameObject> WorldObjectsToHide = new List <GameObject>();
		public List <GameObject> SolidTerrainToHide = new List <GameObject>();

		public void Awake()
		{
				Trigger = gameObject.GetComponent <SphereCollider>();
				Audio = gameObject.GetComponent <AudioSource>();
				Audio.rolloffMode = AudioRolloffMode.Linear;
				Audio.maxDistance	= Trigger.radius + 50.0f;
		}

		public void Start()
		{
				foreach (GameObject worldObjectToHide in WorldObjectsToHide) {
						if (worldObjectToHide != null) {
								worldObjectToHide.layer = Globals.LayerNumHidden;
						}
				}	
		
				foreach (GameObject solidTerrainToHide in SolidTerrainToHide) {
						if (solidTerrainToHide != null) {
								solidTerrainToHide.SetActive(false);
								solidTerrainToHide.layer = Globals.LayerNumHidden;
						}
				}
		}

		public void OnTriggerEnter(Collider other)
		{		
				switch (other.gameObject.layer) {
						case Globals.LayerNumPlayer:
								DisorientPlayer();
								break;
			
						default:
								break;
				}
		}

		public void Update()
		{
				if (MagicBarrier != null) {
						MagicBarrier.transform.localScale = Vector3.one * ((Trigger.radius * 2.0f) + Mathf.PingPong(5.0f, 2.5f));
				}
		}

		public void DisorientPlayer()
		{
//		Vector3 position = Vector3.MoveTowards (Player.Local.Position, transform.position, -(SendAwayDistance + Trigger.radius));
//		position.y = WorldRegionManager.Get.CurrentLoadedRegion.SampleHeightAtInGamePosition (position) + 1.0f;
//		Player.SendToPosition (position);
//		CameraFX.Get.Blur.enabled = true;
//		Player.Local.transform.Rotate (new Vector3 (0f, 180f, 0f));
//		Player.Local.FPSCamera.DoEarthQuake (0.25f, 2.25f, 0.25f, 0.25f, 0.25f);
//		StartCoroutine (StartIntrospection ( ));
		}

		public void Dispel(string counterSpell)
		{
				if (counterSpell != "RevealHidden") {
						return;
				}
		
				foreach (GameObject worldObjectToHide in WorldObjectsToHide) {
						if (worldObjectToHide != null) {
								worldObjectToHide.layer = Globals.LayerNumWorldItemActive;
						}
				}	
		
				foreach (GameObject solidTerrainToHide in SolidTerrainToHide) {
						if (solidTerrainToHide != null) {
								solidTerrainToHide.SetActive(true);
								solidTerrainToHide.layer = Globals.LayerNumSolidTerrain;
						}
				}
		
				MagicBarrier.renderer.enabled = false;
				Audio.PlayOneShot(DispelClip);
		
				GameObject.Destroy(gameObject, 1.5f);
		}

		public IEnumerator StartIntrospection()
		{
				yield return new WaitForSeconds (1.5f);
				CameraFX.Get.Default.Blur.enabled = false;
		}
}

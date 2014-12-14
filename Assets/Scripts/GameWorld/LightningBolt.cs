using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class LightningBolt : MonoBehaviour
		{
				public Light PointLight;
				public List <Renderer> BoltRenderers;
				public int BoltIndex = 0;
				public AudioSource LightningAudio;
				public Color FadeColor;

				public void Awake()
				{
						for (int i = 0; i < BoltRenderers.Count; i++) {
								BoltRenderers[i].enabled = false;
						}
						PointLight.enabled = false;
				}

				public void OnEnable()
				{
						BoltRenderers.Shuffle();
						BoltIndex = 0;
						BoltRenderers[0].enabled = true;
						Biomes.Get.LightingFlash(transform.position);
						PointLight.enabled = true;
						PointLight.intensity = 5f;
						LightningAudio.Play();
						FadeColor.r = 2f;
						FadeColor.g = 2f;
						FadeColor.b = 2f;
						FadeColor.a = 1f;
				}

				//TODO add an effect sphere to bolt so it pulverizes whatever it hits

				public void Update()
				{
						if (BoltIndex < BoltRenderers.Count) {
								PointLight.intensity = Mathf.Lerp(PointLight.intensity, 0f, 0.5f);
								for (int i = 0; i < BoltRenderers.Count; i++) {
										if (i == BoltIndex) {
												BoltRenderers[i].enabled = true;
										} else {
												BoltRenderers[i].enabled = false;
										}
								}
								BoltIndex++;
						} else if (FadeColor.r > 0f) {//set the tint for the final material to zero over time
								FadeColor.r = Mathf.Lerp(FadeColor.r, 0f, 0.35f);
								FadeColor.g = FadeColor.r;
								FadeColor.b = FadeColor.r;
								FadeColor.a = 1f;
								if (FadeColor.r < 0.005f) {
										FadeColor.r = 0f;
								}
								for (int i = 0; i < BoltRenderers.LastIndex(); i++) {
										BoltRenderers[i].enabled = false;
								}
								BoltRenderers[BoltRenderers.LastIndex()].enabled = true;
								BoltRenderers[BoltRenderers.LastIndex()].material.SetColor("_TintColor", FadeColor);
						} else {
								GameObject.Destroy(gameObject, 0.5f);
								enabled = false;
						}
				}
		}
}
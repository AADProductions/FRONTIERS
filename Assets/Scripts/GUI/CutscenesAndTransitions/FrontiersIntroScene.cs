using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

namespace Frontiers
{
		public class FrontiersIntroScene : MonoBehaviour
		{
				public GameObject FrontiersLogoCameraPosition;
				public ChunkBiomeData Biome	= new ChunkBiomeData();
				public TOD_CycleParameters Cycle = new TOD_CycleParameters();
				public float FieldOfView = 50f;
				public double TimeMultiplier = 1.0;
				public Color AmbientLight;
				public string CameraLUTName = "TemperateRegion";
				public Texture2D CameraLUT;
				public AudioSource AmbientAudio;
				public float AmbientAudioTargetVolume;

				public void Awake()
				{
						Frontiers.GUI.GUILoading.DetailsInfo = "Loading Intro Scene";
						AmbientAudio.volume = 0f;
				}

				public void Start()
				{
						//GameWorld.Get.Sky.Cycle 		= Cycle;
						//GameWorld.Get.Sky.Day			= Biome.DaySky;
						//GameWorld.Get.Sky.Night		= Biome.NightSky;
						//GameWorld.Get.Sky.Atmosphere 	= Biome.Atmosphere;
						if (Mods.Get.Runtime.CameraLUT(ref CameraLUT, CameraLUTName)) {
								CameraFX.Get.SetLUT(CameraLUT);
						} else {
								Debug.Log("Couldn't load LUT");
						}
				}

				public void FixedUpdate()
				{
						switch (GameManager.State) {
								case FGameState.GamePaused:
								case FGameState.GameLoading:
								case FGameState.Startup:
								case FGameState.WaitingForGame:
										if (!animation.isPlaying) {
												animation.Play();
										}
										GameManager.Get.GameCamera.transform.position = FrontiersLogoCameraPosition.transform.position;
										GameManager.Get.GameCamera.transform.rotation = FrontiersLogoCameraPosition.transform.rotation;
										GameManager.Get.GameCamera.camera.fieldOfView = FieldOfView;
										break;
						}
				}

				public void Update()
				{
						AmbientAudio.volume = Mathf.Lerp(AmbientAudio.volume, AmbientAudioTargetVolume, 0.125f);
						GameWorld.Get.Sky.Cycle.Hour = Cycle.Hour + (float)(WorldClock.RealTime * TimeMultiplier);
						RenderSettings.ambientLight = AmbientLight;
				}

				public void OnDestroy()
				{
						AmbientAudioTargetVolume = 0f;
				}
		}
}

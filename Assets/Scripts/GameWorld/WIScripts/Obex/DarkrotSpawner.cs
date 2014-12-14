using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class DarkrotSpawner : WIScript
		{
				public int NumSpawnedRecently = 0;
				public Vector3 SpawnOffset = Vector3.zero;
				public double LastDarkrotSpawned = 0;
				public double LastTimeEnabled = 0;
				public Light SpawnerLight;
				public string FXOnSpawn;
				public string SoundOnSpawn;
				public string SoundOnActive;
				public DarkrotSpawnerState State = new DarkrotSpawnerState();

				public override void OnInitialized()
				{
						worlditem.OnActive += OnActive;
				}

				public void OnActive()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Obex, worlditem.tr, SoundOnActive);
						NumSpawnedRecently = 0;
						LastTimeEnabled = WorldClock.AdjustedRealTime;
						enabled = true;
				}

				public void Update()
				{
						if (WorldClock.AdjustedRealTime < LastTimeEnabled + State.SpawnDelay) {
								return;
						}

						SpawnerLight.enabled = true;
						SpawnerLight.intensity = Mathf.Lerp(SpawnerLight.intensity, 0f, 0.125f);

						if (NumSpawnedRecently < State.MaxDarkrotAtOneTime) {
								if (WorldClock.AdjustedRealTime > LastDarkrotSpawned + State.SpawnInterval) {
										MasterAudio.PlaySound(MasterAudio.SoundType.Obex, worlditem.tr, SoundOnSpawn);
										FXManager.Get.SpawnFX(worlditem.tr, FXOnSpawn);

										SpawnerLight.intensity = 2f;
										Creatures.Get.SpawnDarkrotNode(worlditem.tr.TransformPoint(SpawnOffset));
										LastDarkrotSpawned = WorldClock.AdjustedRealTime;
										State.NumDarkrotSpawned++;
										NumSpawnedRecently++;
								}
						} else {
								if (!Mathf.Approximately(SpawnerLight.intensity, 0f)) {
										SpawnerLight.intensity = Mathf.Lerp(SpawnerLight.intensity, 0f, 0.125f);
								} else {
										SpawnerLight.enabled = false;
										enabled = false;
								}
						}
				}
		}

		[Serializable]
		public class DarkrotSpawnerState
		{
				public float SpawnDelay = 2f;
				public float SpawnInterval = 5f;
				public int MaxDarkrotAtOneTime = 5;
				public int NumDarkrotSpawned = 0;
		}
}

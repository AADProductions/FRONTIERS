using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Garden : WIScript
		{
				public GardenState State = new GardenState();
				public Spawner spawner;
				public int SpawnerSettingIndex = 0;
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.SpawnPoints.Clear();
						foreach (Transform spawnPoint in transform) {
								if (spawnPoint.name == "SpawnPoint") {
										State.SpawnPoints.Add(new STransform(spawnPoint, true));
								}
						}
				}

				public override void OnInitialized()
				{
						if (worlditem.Is <Spawner>(out spawner)) {
								spawner.State.SpawnerSettings[SpawnerSettingIndex].ManualSpawnPoints = State.SpawnPoints;
						}
				}
				#endif
				public void OnDrawGizmos()
				{
						Gizmos.color = Color.green;
						foreach (Transform spawnPoint in transform) {
								if (spawnPoint.name == "SpawnPoint") {
										Gizmos.DrawSphere(spawnPoint.position, 1f);
								}
						}
				}
		}

		[Serializable]
		public class GardenState
		{
				public List <STransform> SpawnPoints = new List <STransform>();
		}
}
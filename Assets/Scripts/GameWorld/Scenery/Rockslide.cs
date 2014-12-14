using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Rockslide : SceneryScript
		{
				public RockslideState State = new RockslideState();
				public float RockslideInterval = 10f;
				public int NumRocksToSpawn = 10;
				public Queue <GameObject> ActiveRocks = new Queue <GameObject>();

				protected override void OnInitialized()
				{
						tr = transform;
				}

				public override void OnPlayerEncounter()
				{
						if (mRockslideInProgress)
								return;

						if (WorldClock.AdjustedRealTime > NextRockslideTime) {
								if (State.LastRandomCheck < 0) {
										State.LastRandomCheck = Profile.Get.CurrentGame.Seed;
								}
								System.Random random = new System.Random(State.LastRandomCheck);
								State.LastRandomCheck = random.Next(0, 100);
								if (State.LastRandomCheck > State.ChanceOfRockslide) {
										mRockslideInProgress = true;
										StartCoroutine(RockslideOverTime(WorldClock.AdjustedRealTime + RockslideInterval, random));
								}
						}
				}

				public double NextRockslideTime {
						get {
								return State.LastRockslideTime + State.MinimumSecondsBetweenRockslides;
						}
				}

				protected IEnumerator RockslideOverTime(double rockslideEndTime, System.Random random)
				{
						Player.Local.DoEarthquake(0.5f);
						float timeBetweenRocks = RockslideInterval / NumRocksToSpawn;
						GameObject rock = null;
						MasterAudio.PlaySound(MasterAudio.SoundType.Explosions, Player.Local.tr, "Rockslide");
						mVertices = cfo.PrimaryCollider.sharedMesh.vertices;
						while (WorldClock.AdjustedRealTime < rockslideEndTime) {
								mTerrainHit.feetPosition = tr.TransformPoint(mVertices[random.Next(0, mVertices.Length)]);
								mTerrainHit.groundedHeight = 25f;
								mTerrainHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);
								rock = GameObject.Instantiate(FXManager.Get.RockslideRockPrefab, mTerrainHit.feetPosition, Quaternion.identity) as GameObject;
								//rock.transform.localScale = Vector3.one * ((float)random.Next(50, 100) / 100);//make it anywhere from .5 to 1 size
								ActiveRocks.Enqueue(rock);
								yield return new WaitForSeconds(timeBetweenRocks);//wait for ART seconds, not RT seconds
						}
						yield return null;//now we'll destroy any rocks that haven't been broken already
						while (ActiveRocks.Count > 0) {
								rock = ActiveRocks.Dequeue();
								if (rock != null) {
										FXManager.Get.SpawnFX(tr.position, "DustExplosion");
										GameObject.Destroy(rock);
								}
								yield return new WaitForSeconds(timeBetweenRocks);
						}
						Array.Clear(mVertices, 0, mVertices.Length);
						mVertices = null;
						State.LastRockslideTime = WorldClock.AdjustedRealTime;
						mRockslideInProgress = false;
				}

				protected Vector3[] mVertices = null;
				protected bool mRockslideInProgress = false;
				protected GameWorld.TerrainHeightSearch mTerrainHit;
				protected Transform tr;
		}

		[Serializable]
		public class RockslideState : SceneryScriptState
		{
				public double LastRockslideTime = -1f;
				public float MinimumSecondsBetweenRockslides = 60f;
				public int ChanceOfRockslide = 50;
				//out of 100
				public int LastRandomCheck = 0;
		}
}
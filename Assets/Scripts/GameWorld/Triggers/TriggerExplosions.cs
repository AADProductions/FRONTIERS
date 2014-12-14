using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class TriggerExplosions : WorldTrigger
		{
				public TriggerExplosionsState State = new TriggerExplosionsState();

				public override bool OnPlayerEnter()
				{
						if (!mSpawningExplosions) {
								mSpawningExplosions = true;
								StartCoroutine(SpawnExplosions());
								return true;
						}
						return false;
				}

				protected IEnumerator SpawnExplosions()
				{
						Transform explosionSource = gameObject.FindOrCreateChild("ExplosionSource").transform;
						double timeStarted = WorldClock.AdjustedRealTime;
						List <ExplosionTemplate> explosions = new List<ExplosionTemplate>();
						explosions.AddRange(State.Explosions);
						while (explosions.Count > 0) {
								double currentTime = WorldClock.AdjustedRealTime;
								for (int i = explosions.LastIndex(); i >= 0; i--) {
										ExplosionTemplate explosion = explosions[i];
										if (explosion.Delay < currentTime - timeStarted) {
												explosionSource.position = transform.position + explosion.Position;
												State.ExplosionDamage.Point = explosionSource.position;
												State.ExplosionDamage.SenderName = "Explosion";
												FXManager.Get.SpawnExplosion(
														explosion.Type,
														State.ExplosionDamage.Point,
														explosion.Radius,
														explosion.ForceAtEdge,
														explosion.MinimumForce,
														explosion.Duration,
														State.ExplosionDamage);
												MasterAudio.PlaySound(MasterAudio.SoundType.Explosions, explosionSource, explosion.ExplosionSound);
												Player.Local.DoEarthquake(explosion.Duration, explosion.BombShake);

												//spawn falling dust
												for (int j = 0; j < State.FallingSandPositions.Count; j++) {
														FXManager.Get.SpawnFX(transform.position + State.FallingSandPositions[j], State.FallingSandFX);
												}

												explosions.RemoveAt(i);
										}
								}
								yield return null;
						}
						mSpawningExplosions = false;
						GameObject.Destroy(explosionSource.gameObject, 0.5f);
						yield break;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.Explosions.Clear();
						foreach (Transform child in transform) {
								if (child.name == "FallingSand") {
										State.FallingSandPositions.Add(new SVector3(child.localPosition));
								} else {
										ExplosionTemplate explosion = new ExplosionTemplate();
										explosion.Delay = float.Parse(child.name);
										explosion.Radius = child.transform.localScale.x;
										explosion.Position = child.position - transform.position;
										explosion.Duration = 0.25f;
										explosion.ForceAtEdge = 1f;
										explosion.Type = FXManager.ExplosionType.Simple;
										State.Explosions.Add(explosion);
										State.TotalDuration = Mathf.Max(State.TotalDuration, explosion.Delay);
								}
						}
				}
				#endif
				public void OnDrawGizmos()
				{
						foreach (ExplosionTemplate explosion in State.Explosions) {
								Gizmos.color = Color.Lerp(Color.yellow, Color.red, explosion.Delay / State.TotalDuration);
								Gizmos.DrawWireSphere(transform.position + explosion.Position, explosion.Radius);
						}

						foreach (Transform child in transform) {
								if (child.gameObject.activeSelf) {
										Gizmos.color = Color.white;
										if (child.name == "FallingSand") {
												Gizmos.DrawWireCube(child.position, child.localScale);
										} else {
												Gizmos.DrawSphere(child.position, child.transform.localScale.x);
										}
								}
						}
				}

				protected bool mSpawningExplosions = false;
		}

		[Serializable]
		public class TriggerExplosionsState : WorldTriggerState
		{
				public List <ExplosionTemplate> Explosions = new List <ExplosionTemplate>();
				public List <SVector3> FallingSandPositions = new List <SVector3>();
				[FrontiersFXAttribute]
				public string FallingSandFX;
				public DamagePackage ExplosionDamage = new DamagePackage();
				public float TotalDuration = 0f;
		}

		[Serializable]
		public class ExplosionTemplate
		{
				public SVector3 Position = new SVector3();
				public FXManager.ExplosionType Type = FXManager.ExplosionType.Simple;
				public string ExplosionSound = "GenericExplosion";
				public float Radius = 3f;
				public float ForceAtEdge = 1f;
				public float MinimumForce = 0.1f;
				public float Duration = 0.25f;
				public float Delay = 0f;
				public float BombShake = 0.1f;
		}
}
#define TESTING_CRITTERS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using ExtensionMethods;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class Critters : Manager
	{
		public static Critters Get;
		public List <Critter> CritterTemplates = new List<Critter> ();
		public List <Critter> ActiveCritters = new List<Critter> ();
		public List <Critter> FriendlyCritters = new List<Critter> ();
		public List <string> CurrentCritterTypes = new List<string> ();
		public float CritterSpawnRange = 2.5f;
		public float MaxCritterRange = 5f;
		public int MaxCritters = 10;

		public override void WakeUp ()
		{
			base.WakeUp ();

			Get = this;
		}

		public void SpawnFriendlyFromSaveState (CritterSaveState state) {
			Critter friendly = null;
			if (SpawnCritter (state.Type, Player.Local.Position, out friendly)) {
				friendly.Friendly = true;
				friendly.Coloration = state.Coloration;
				friendly.Name = state.Name;
				friendly.name = friendly.Name;
				friendly.enabled = true;
				FriendlyCritters.Add (friendly);
			} else {
				Debug.LogError ("Couldn't spawn special critter from state " + state.Type);
			}
		}

		public override void OnLocalPlayerDespawn ()
		{
			for (int i = 0; i < FriendlyCritters.Count; i++) {
				if (FriendlyCritters [i] != null) {
					GameObject.Destroy (FriendlyCritters [i].gameObject);
				}
			}
			FriendlyCritters.Clear ();
		}

		public override void OnGameStart ()
		{
			for (int i = 0; i < CritterTemplates.Count; i++) {
				mCritterLookup.Add (CritterTemplates [i].name, CritterTemplates [i]);
			}
		}

		public void Update ()
		{
			if (!GameManager.Is (FGameState.InGame) || !Player.Local.HasSpawned) {
				return;
			}

			mPlayerPosition = Player.Local.Position;
			for (int i = 0; i < ActiveCritters.Count; i++) {
				ActiveCritters [i].UpdateMovement (mPlayerPosition);
			}
			for (int i = 0; i < FriendlyCritters.Count; i++) {
				FriendlyCritters [i].UpdateMovement (mPlayerPosition);
			}

			mPruneCritters++;
			if (mPruneCritters > 30) {
				mPruneCritters = 0;
				for (int i = ActiveCritters.LastIndex (); i >= 0; i--) {
					if (ActiveCritters [i].Destroyed) {
						ActiveCritters.RemoveAt (i);
					} else if (ActiveCritters [i].IsDead) {
						GameObject.Destroy (ActiveCritters [i].gameObject);
						ActiveCritters.RemoveAt (i);
					} else if (Vector3.Distance (ActiveCritters [i].Position, mPlayerPosition) > MaxCritterRange || ActiveCritters [i].Position.y < Biomes.Get.TideWaterElevation) {
						GameObject.Destroy (ActiveCritters [i].gameObject);
						ActiveCritters.RemoveAt (i);
					}
				}

				for (int i = FriendlyCritters.LastIndex (); i >= 0; i--) {
					if (FriendlyCritters [i] == null) {
						FriendlyCritters.RemoveAt (i);
					}
				}
			}

			mSpawnCritters++;
			if (mSpawnCritters > 40) {
				mSpawnCritters = 0;
				if (Player.Local.Surroundings.IsOutside && !Player.Local.Surroundings.IsOnMovingPlatform && !GameWorld.Get.CurrentBiome.OuterSpace) {
					#if !TESTING_CRITTERS
					CurrentCritterTypes.Clear ();
					if (WorldClock.IsDay) {
						CurrentCritterTypes.AddRange (GameWorld.Get.CurrentBiome.DayCritterTypes);
					} else {
						CurrentCritterTypes.AddRange (GameWorld.Get.CurrentBiome.NightCritterTypes);
					}
					#endif
					mPlayerVelocity = Player.Local.FPSController.Velocity;
					if (CurrentCritterTypes.Count > 0) {
						int numTries = 0;
						mRandomPosition.groundedHeight = 0.1f;
						mRandomPosition.overhangHeight = 10f;
						mRandomPosition.ignoreWorldItems = true;
						if (ActiveCritters.Count < (MaxCritters * GameWorld.Get.CurrentBiome.CritterDensity)) {
							mSpawnCritters = 35;
							string critterName = CurrentCritterTypes [UnityEngine.Random.Range (0, CurrentCritterTypes.Count)];
							if (mCritterLookup.TryGetValue (critterName, out mTemplateLookup)) {
								//get a random position around the player
								mRandomPosition.feetPosition.x = mPlayerPosition.x + UnityEngine.Random.Range (-CritterSpawnRange, CritterSpawnRange);
								mRandomPosition.feetPosition.z = mPlayerPosition.z + UnityEngine.Random.Range (-CritterSpawnRange, CritterSpawnRange);
								mRandomPosition.feetPosition.y = mPlayerPosition.y;
								mRandomPosition.feetPosition.x = mRandomPosition.feetPosition.x + mPlayerVelocity.x;
								mRandomPosition.feetPosition.z = mRandomPosition.feetPosition.z + mPlayerVelocity.z;
								mRandomPosition.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mRandomPosition);
								if (mRandomPosition.hitTerrain) {
									mRandomPosition.feetPosition.y = mRandomPosition.feetPosition.y + 0.1f;
									GameObject newCritterGameObject = GameObject.Instantiate (mTemplateLookup.gameObject, mRandomPosition.feetPosition, Quaternion.Euler (0f, UnityEngine.Random.value * 360f, 0f)) as GameObject;
									#if UNITY_EDITOR
									newCritterGameObject.name = mTemplateLookup.gameObject.name + "#" + ActiveCritters.Count.ToString();
									#endif
									Critter newCritter = newCritterGameObject.GetComponent <Critter> ();
									if (newCritter.BodyCollider.enabled && Player.Local.Controller.enabled) {
										Physics.IgnoreCollision (Player.Local.Controller, newCritter.BodyCollider);
									}
									ActiveCritters.Add (newCritter);
								}
							}
						}
					}
				} else if (ActiveCritters.Count > 0) {
					for (int i = 0; i < ActiveCritters.Count; i++) {
						GameObject.Destroy (ActiveCritters [i].gameObject);
					}
					ActiveCritters.Clear ();
				}
			}
		}

		public bool SpawnCritter (string critterName, Vector3 position, out Critter critter)
		{
			critter = null;
			if (mCritterLookup.TryGetValue (critterName, out mTemplateLookup)) {
				GameObject newCritterGameObject = GameObject.Instantiate (mTemplateLookup.gameObject, position, Quaternion.Euler (0f, UnityEngine.Random.value * 360f, 0f)) as GameObject;
				critter = newCritterGameObject.GetComponent <Critter> ();
				critter.enabled = true;
			}
			return critter != null;
		}

		protected Dictionary <string,Critter> mCritterLookup = new Dictionary<string, Critter> ();
		protected int mPruneCritters = 0;
		protected int mSpawnCritters = 0;
		protected Critter mTemplateLookup = null;
		protected Vector3 mPlayerPosition;
		protected Vector3 mPlayerVelocity;
		protected GameWorld.TerrainHeightSearch mRandomPosition;
	}
}
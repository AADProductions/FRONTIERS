using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;
using Frontiers.World;
using System;

namespace Frontiers {
	public class Meteors : Manager {

		public static Meteors Get;

		public Action OnMeteorSpawned;
		public List <Meteor> MeteorsSpawned = new List<Meteor> ();

		public bool SpawnMeteors;
		public GameObject FallingMeteorPrefab;
		public int AdjustedMaxMeteorsPerNight = 10;
		public List <FallingMeteor> ActiveMeteors = new List <FallingMeteor> ();
		public List <FallingMeteor> InactiveMeteors = new List<FallingMeteor> ();
		public Material RecentMeteorMaterial;
		public Color GlowMaterialColor;
		public Color GlowMaterialColorDark;

		public override void WakeUp ()
		{
			Get = this;
			GlowMaterialColorDark = Color.Lerp (GlowMaterialColor, Color.black, 0.5f);
		}

		public override void OnModsLoadStart ()
		{
			AdjustedMaxMeteorsPerNight = Globals.MaxMeteorsPerNight;

			Vector3 spawnPosition = Vector3.up * Globals.WorldChunkTerrainHeight;
			for (int i = 0; i < Globals.MaxMeteorsPerNight; i++) {
				GameObject meteorGameObject = GameObject.Instantiate (FallingMeteorPrefab, spawnPosition, Quaternion.identity) as GameObject;
				FallingMeteor f = meteorGameObject.GetComponent <FallingMeteor> ();
				f.Deactivate ();
				InactiveMeteors.Add (f);
			}
		}

		public override void OnLocalPlayerSpawn ()
		{
			mLastMeteorSpawnTime = WorldClock.AdjustedRealTime + Globals.MeteorMinimumSpawnTime;
		}

		public void Update () {

			if (!SpawnMeteors)
				return;

			RecentMeteorMaterial.color = Color.Lerp (GlowMaterialColorDark, GlowMaterialColor, Mathf.Abs (Mathf.Sin (Time.time)));

			#if UNITY_EDITOR
			if (Input.GetKeyDown (KeyCode.O)) {
				if (InactiveMeteors.Count > 0) {
					//yay spawn a new one
					FallingMeteor f = InactiveMeteors [0];
					InactiveMeteors.RemoveAt (0);
					ActiveMeteors.Add (f);
					f.Activate ();
				}
			}
			#endif

			if (Player.Local == null || !Player.Local.HasSpawned || !GameManager.Is (FGameState.InGame)) {
				return;
			}

			if (GameWorld.Get.CurrentBiome.OuterSpace) {
				return;
			}

			if (WorldClock.IsDay) {
				mMeteorsFallenTonight = 0;
				mCheckedTonight = false;
				mCheckMeteors++;
				if (mCheckMeteors > 30) {
					mCheckMeteors = 0;

					if (mMeteorsFallenToday < Globals.MaxMeteorsPerDay) {
						if (InactiveMeteors.Count > 0 && UnityEngine.Random.value < Globals.MeteorSpawnProbabilityDaytime && WorldClock.AdjustedRealTime > mLastMeteorSpawnTime + Globals.MeteorMinimumSpawnTime) {
							//yay spawn a new one
							FallingMeteor f = InactiveMeteors [0];
							InactiveMeteors.RemoveAt (0);
							ActiveMeteors.Add (f);
							f.Activate ();
							mMeteorsFallenToday++;
							mLastMeteorSpawnTime = WorldClock.AdjustedRealTime;
						}
					}

					if (ActiveMeteors.Count > 0) {
						for (int i = ActiveMeteors.LastIndex (); i >= 0; i--) {
							//remove any that have been killed
							FallingMeteor f = ActiveMeteors [i];
							if (f.IsDepleted) {
								InactiveMeteors.Add (f);
								ActiveMeteors.RemoveAt (i);
							}
						}
					}
				}
				return;
			} else {
				mMeteorsFallenToday = 0;

				if (!mCheckedTonight) {
					mCheckedTonight = true;
					if (Globals.OscillateMeteorSpawnAmount) {
						float period = (float)((WorldClock.AdjustedRealTime % Globals.MeteorSpawnOscillateDuration) / Globals.MeteorSpawnOscillateDuration);
						AdjustedMaxMeteorsPerNight = Mathf.Clamp (Mathf.CeilToInt (Mathf.Sin (period) * Globals.MaxMeteorsPerNight), Globals.MinMeteorsPerNight, Globals.MaxMeteorsPerNight);
					} else {
						AdjustedMaxMeteorsPerNight = Globals.MaxMeteorsPerNight;
					}
				}

				mCheckMeteors++;
				if (mCheckMeteors > 30) {
					mCheckMeteors = 0;
					if (mMeteorsFallenTonight < Globals.MaxMeteorsPerNight) {
						if (InactiveMeteors.Count > 0 && UnityEngine.Random.value < Globals.MeteorSpawnProbability && WorldClock.AdjustedRealTime > mLastMeteorSpawnTime + Globals.MeteorMinimumSpawnTime) {
							//yay spawn a new one
							FallingMeteor f = InactiveMeteors [0];
							InactiveMeteors.RemoveAt (0);
							ActiveMeteors.Add (f);
							f.Activate ();
							mMeteorsFallenTonight++;
							mLastMeteorSpawnTime = WorldClock.AdjustedRealTime;
							OnMeteorSpawned.SafeInvoke ();
						}
					}

					for (int i = ActiveMeteors.LastIndex (); i >= 0; i--) {
						//remove any that have been killed
						FallingMeteor f = ActiveMeteors [i];
						if (f.IsDepleted) {
							InactiveMeteors.Add (f);
							ActiveMeteors.RemoveAt (i);
							if (f.SpawnMeteor) {
								WorldChunk p = GameWorld.Get.PrimaryChunk;
								f.transform.parent = p.AboveGroundGroup.tr;
								mActiveMeteorPosition.Position = f.transform.localPosition;
								WorldItem newMeteorWorldItem = null;
								WorldItems.CloneWorldItem ("Crystals", "Falling Meteor", mActiveMeteorPosition, false, p.AboveGroundGroup, out newMeteorWorldItem);
								newMeteorWorldItem.Initialize ();
								newMeteorWorldItem.ActiveState = WIActiveState.Active;
								newMeteorWorldItem.ActiveStateLocked = true;
								MeteorsSpawned.Add (newMeteorWorldItem.Get <Meteor> ());
								//get rid of dead meteors to make it easier on orbs
								for (int j = MeteorsSpawned.LastIndex (); j >= 0; j--) {
									if (MeteorsSpawned [j] == null || MeteorsSpawned [j].IsDestroyed) {
										MeteorsSpawned.RemoveAt (j);
									}
								}

								OnMeteorSpawned.SafeInvoke ();
							}
						}
					}
				}
			}
		}

		protected STransform mActiveMeteorPosition = new STransform ();
		protected double mLastMeteorSpawnTime = -1f;
		protected int mMeteorsFallenTonight = 0;
		protected int mMeteorsFallenToday = 0;
		protected int mCheckMeteors = 0;
		protected bool mCheckedTonight = false;
	}
}
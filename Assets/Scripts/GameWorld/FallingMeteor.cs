using UnityEngine;
using System.Collections;

namespace Frontiers.World {
	public class FallingMeteor : MonoBehaviour {
		public DamagePackage MeteorDamage;
		public float MaxEndPositionRange = 100f;
		public float MaxStartPositionRange = 200f;
		public Vector3 StartPosition;
		public GameWorld.TerrainHeightSearch EndPosition;
		public float NormalizedSpeed;
		public float NormalizedPosition;
		public bool HasCrashed;
		public bool IsDepleted;
		public bool SpawnMeteor;
		public Light MeteorLight;
		public ParticleSystem FireParticles;
		public Rigidbody rb;
		public Transform FlarePlane;
		public Transform Meteor;
		//public TrailRenderer Trail;
		public AnimationCurve CrashSizeCurve;
		public float MaxFlareSize = 20f;
		public float MinFlareSize = 10f;
		public float DisperseTime = 2f;
		public AudioSource FallingSound;
		public AudioClip CrashingSound;
		public float EmissionRate = 100f;

		public void Awake () {
			rb = gameObject.GetComponent <Rigidbody> ();
			//Trail = gameObject.GetComponent <TrailRenderer> ();
			EndPosition.ignoreWater = false;
			EndPosition.ignoreWorldItems = false;
			EndPosition.overhangHeight = 10f;
			EndPosition.groundedHeight = 10f;
			HasCrashed = false;
			IsDepleted = false;
			Deactivate ();
		}

		public void Activate () {
			StartCoroutine (ActivateOverTime ());
		}

		public IEnumerator ActivateOverTime () {
			FlarePlane.GetComponent <Renderer> ().material.SetColor ("_TintColor", new Color (10f, 10f, 10f, 1f));
			Meteor.localScale = Vector3.one;
			HasCrashed = false;
			NormalizedPosition = 0f;
			EndPosition.feetPosition = Player.Local.Position;
			EndPosition.feetPosition.x = EndPosition.feetPosition.x + Random.Range (-MaxEndPositionRange, MaxEndPositionRange);
			EndPosition.feetPosition.z = EndPosition.feetPosition.z + Random.Range (-MaxEndPositionRange, MaxEndPositionRange);
			//get a position from the game world
			EndPosition.ignoreWater = false;

			bool foundEndPosition = false;
			Color colorAtPosition = Color.black;
			while (!foundEndPosition) {
				EndPosition.feetPosition.y = GameWorld.Get.TerrainHeightAtSkyPosition (ref EndPosition, Globals.WorldChunkTerrainHeight);
				colorAtPosition = GameWorld.Get.TerrainTypeAtInGamePosition (EndPosition.feetPosition, false);
				yield return null;
				if (GameWorld.CheckTerrainType (colorAtPosition, TerrainType.AllButCivilization, 0.1f)) {
					foundEndPosition = true;
				}
				yield return null;
			}

			SpawnMeteor = true;
			if (EndPosition.hitWater | EndPosition.hitStructureMesh) {
				SpawnMeteor = false;
			}

			StartPosition = Player.Local.Position;
			StartPosition.x = StartPosition.x + Random.Range (-MaxStartPositionRange, MaxStartPositionRange);
			StartPosition.z = StartPosition.z + Random.Range (-MaxStartPositionRange, MaxStartPositionRange);
			StartPosition.y = Globals.WorldChunkTerrainHeight;

			rb.MovePosition (StartPosition);

			enabled = true;
			MeteorLight.enabled = true;
			MeteorLight.intensity = 10f;
			gameObject.layer = Globals.LayerNumScenery;
			FlarePlane.gameObject.layer = Globals.LayerNumScenery;
			Meteor.gameObject.layer = Globals.LayerNumScenery;
			FireParticles.enableEmission = true;
			FireParticles.emissionRate = EmissionRate;
			if (!WorldClock.IsDay) {
				Biomes.Get.LightingFlash (StartPosition, false);
			}
			FallingSound.Play ();
			//Trail.enabled = true;
			yield break;

		}

		public void Deactivate () {
			enabled = false;
			gameObject.layer = Globals.LayerNumHidden;
			FlarePlane.gameObject.layer = Globals.LayerNumHidden;
			Meteor.gameObject.layer = Globals.LayerNumHidden;
			MeteorLight.enabled = false;
			rb.detectCollisions = false;
			FireParticles.emissionRate = 0f;
			FireParticles.enableEmission = false;
			//Trail.enabled = false;
			FallingSound.Stop ();

			if (EndPosition.hitWater) {
				//make a splash!
				FXManager.Get.SpawnFX (EndPosition.feetPosition, "Leviathan Splash");
			} else if (EndPosition.hitStructureMesh) {
				//make a boom!
				FXManager.Get.SpawnExplosionFX (ExplosionType.Base, null, EndPosition.feetPosition);
			}
		}

		//TODO add a trigger that kills anything it touches

		public void Update () {
			if (HasCrashed) {
				FallingSound.volume = Mathf.Lerp (FallingSound.volume, 0f, Time.deltaTime);
				FlarePlane.localScale = (Vector3.one * (MaxFlareSize * 5) * CrashSizeCurve.Evaluate ((float)(WorldClock.AdjustedRealTime - mCrashTime) / DisperseTime));
				Meteor.localScale = Vector3.Lerp (Meteor.localScale, Vector3.one * 0.1f, Time.deltaTime);
				MeteorLight.intensity = Mathf.Lerp (MeteorLight.intensity, 0f, Time.deltaTime * 5f);
				IsDepleted = (WorldClock.AdjustedRealTime > mCrashTime + DisperseTime);
				if (IsDepleted) {
					Deactivate ();
				}
			} else {
				FallingSound.volume = Mathf.Lerp (FallingSound.volume, 1f, Time.deltaTime);
				NormalizedPosition += (NormalizedSpeed * Time.deltaTime);
				rb.MovePosition (Vector3.Lerp (StartPosition, EndPosition.feetPosition, NormalizedPosition));
				FlarePlane.localScale = Vector3.one * Mathf.Lerp (MinFlareSize, MaxFlareSize, Random.value);
				if (Player.Local.Surroundings.IsOutside) {
					MeteorLight.intensity = Mathf.Lerp (10f, 3f, Random.value);
				} else {
					MeteorLight.intensity = Mathf.Lerp (MeteorLight.intensity, 0f, Time.deltaTime);
				}
				FlarePlane.LookAt (Player.Local.Position);
				//FlarePlane.Rotate (0f, 0f, Random.value * 360f);
				if (NormalizedPosition >= 0.999f) {
					Crash ();
				}
			}
		}

		protected void Crash () {
			FlarePlane.GetComponent <Renderer> ().material.SetColor ("_TintColor", new Color (0.5f, 0.5f, 0.5f, 0.25f));
			EndPosition.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref EndPosition);
			HasCrashed = true;
			IsDepleted = false;
			FXManager.Get.SpawnExplosion (ExplosionType.Simple, EndPosition.feetPosition, 50f, 100f, 1f, 1f, MeteorDamage);
			//Biomes.Get.LightingFlash (EndPosition.feetPosition);
			mCrashTime = WorldClock.AdjustedRealTime;
			FireParticles.emissionRate = 0f;
			FallingSound.PlayOneShot (CrashingSound);
		}

		protected double mCrashTime;
	}
}
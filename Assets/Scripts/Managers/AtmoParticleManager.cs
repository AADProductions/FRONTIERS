using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
	public class AtmoParticleManager : MonoBehaviour
	{
		private Vector3 playerPosition;

		public Vector3 PlayerPosition {
			get {
				return playerPosition;
			}

			set {
				playerPosition = value;
			}
		}

		private Dictionary <string, ParticleSystem>	emitterDict = new Dictionary<string, ParticleSystem> ();
		public float transitionSpeed;

		[Serializable]
		public class ParticleSystemSetting
		{
			public GameObject particlePrefab;
			public int maxParticles = 1000;
			public Vector3 particleOffset;
		}

		public ParticleSystemSetting[] particleSettings;
		public GameObject[] bugParticlePrefabs;
		private Dictionary<string, AtmoParticleSetting> particleSettingDict = new Dictionary<string, AtmoParticleSetting> ();

		public void ChangeAtmoSettingDensity (string atmoType, float atmoDensity)
		{
			// Updates to Density are Lerped by a set speed
			AtmoParticleSetting pSetting = GetAtmoParticleSetting (atmoType);
			pSetting.TargetDensity = atmoDensity;

			// Initializes Emitter
			if (GetParticleSystem (atmoType) == null) {
				// Used for debuging, make sure to remove all Debug.Logs before release
				Debug.Log ("AtmoParticleManager couldn't find prefab " + atmoType +
				". Emitter couldn't be created and no particles will be shown");
			}
		}

		public void SetAtmoSetting (string atmoType, AtmoParticleSetting pSetting)
		{
			// Used for getting saved state, or updating atmoparticlesettings
			particleSettingDict [atmoType] = pSetting;
		}

		public AtmoParticleSetting GetAtmoParticleSetting (string settingName)
		{
			// Is particle setting already instantiated?
			if (!particleSettingDict.ContainsKey (settingName))
				particleSettingDict [settingName] = new AtmoParticleSetting ();

			return particleSettingDict [settingName];
		}
		//this is set by the game's quality settings to reduce onscreen particles
		//all atmo particle densities are multiplied by GlobalDensityMultiplier
		public float GlobalDensityMultiplier	= 1.0f;
		//how far away from PlayerPosition a particle can get before it is deleted
		public float MaxParticleRadius = 10.0f;
		//horse flies will look different than gnats, etc.
		public List <BugParticleSystem> BugEmitters	= new List <BugParticleSystem> ();

		public void	AddBugs (List<BugParticleSetting> newBugs)
		{
			for (int i =0; i < newBugs.Count; i++) {
				//foreach (BugParticleSetting bugSetting in newBugs) {
				BugParticleSystem bugSystem = GetBugSystem (newBugs [i]);
				if (bugSystem == null)
					continue; // Couldn't find prefab to create from

				bugSystem.Setting = newBugs [i];
			}		
		}

		public void RemoveBugs (List<string> deadBugs)
		{
			for (int i = 0; i < deadBugs.Count; i++) {
				//foreach (string bugType in deadBugs) {
				for (int j = 0; j < BugEmitters.Count; j++) {
					if (BugEmitters [j] == null)
						continue;

					if (BugEmitters [j].Setting.BugType != deadBugs [i])
						continue;
					// Queue it for removal of bugs and bug system
					BugEmitters [j].Alive = false;
				}
			}
		}

		public void FixedUpdate ()
		{
			// Lerp Particle Settings (Density)
			LerpParticleSettings (transitionSpeed);

			//cull particles based on PlayerPosition / MaxParticleRadius
			//CullAllParticles();

			// If bugs are out of range, destroy and spawn randomly
			//CullBugs();
			//ClearDeadBugSystems();

			// Emit all particles that have density > 0
			EmitParticles ();

			// Emit all available bugs
			EmitBugSpots ();
		}

		private void LerpParticleSettings (float tSpeed)
		{
			var enumerator = particleSettingDict.GetEnumerator ();
			while (enumerator.MoveNext ()) {
			//foreach (var item in particleSettingDict.Values) {
				setting = enumerator.Current;
				setting.Value.Lerp (tSpeed);
			}		
		}
		KeyValuePair <string, AtmoParticleSetting> setting;

		// Used for accessing ParticleSystems directly, and also instantiates only when they are used
		private ParticleSystem GetParticleSystem (string emitterName)
		{
			// Is particle emitter already instantiated?
			if (emitterDict.ContainsKey (emitterName))
				return emitterDict [emitterName];

			// Find Particle Emitter that matches the same name
			// Add to dictionary, return newly instantiated emitter
			for (int i = 0; i < particleSettings.Length; i++) {
				//foreach (ParticleSystemSetting pEmitterSetting in particleSettings) {
				pEmitterSetting = particleSettings [i];
				if (pEmitterSetting.particlePrefab.transform.name == emitterName) {
					GameObject newParticleSystem = Instantiate (pEmitterSetting.particlePrefab) as GameObject;
					newParticleSystem.transform.name = pEmitterSetting.particlePrefab.transform.name;
					emitterDict [emitterName] = newParticleSystem.GetComponent<ParticleSystem> ();

					// Save position offset into AtmoParticleSetting, add any other additional initialization
					AtmoParticleSetting newParticleSetting = GetAtmoParticleSetting (emitterName);
					newParticleSetting.Offset = pEmitterSetting.particleOffset;
					SetAtmoSetting (emitterName, newParticleSetting);

					return emitterDict [emitterName];
				}
			} 

			// Can either return a null, empty object, or halt the code
			// Returning a null would require the receiving to do a null check, an empty object would have no effect except for 
			// taking up space
			return null;
		}
		protected ParticleSystemSetting pEmitterSetting;

		private void CullAllParticles ()
		{
			foreach (var item in emitterDict.Values) {
				CullParticles (item);
			}
		}

		private void CullParticles (ParticleSystem pEmitter)
		{
			// Cull Particles based on distance from player
			ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle [pEmitter.particleCount];
			int numParticles = pEmitter.GetParticles (particleArray);
			for (int i = 0; i < particleArray.Length; i++) {
				if (Vector3.Distance (particleArray [i].position, PlayerPosition) > MaxParticleRadius) {
					Vector3 randomInsideSphere = UnityEngine.Random.insideUnitSphere * MaxParticleRadius;
					randomInsideSphere.y = Mathf.Abs (randomInsideSphere.y);
					// Move the particle that is out of range infront of player
					particleArray [i].position += (randomInsideSphere * MaxParticleRadius) + playerPosition;
					//reset the startLifetime to avoid poppint
					particleArray [i].startLifetime = Time.time;
				}
			}
			pEmitter.SetParticles (particleArray, numParticles);
		}

		private void CullBugs ()
		{
			foreach (BugParticleSystem bugSystem in BugEmitters) {
				if (bugSystem == null)
					continue;

				for (int i = 0; i < bugSystem.Emitters.Count; i++) {
					if (Vector3.Distance (bugSystem.Emitters [i].transform.position, PlayerPosition) > bugSystem.Setting.MaxEmitterRange) {
						// Remove and Destroy
						Destroy (bugSystem.Emitters [i].transform.gameObject);
						bugSystem.Emitters.RemoveAt (i);
						break;
					}
				}
			}
		}

		private void EmitParticles ()
		{
			var enumerator = emitterDict.GetEnumerator ();
			while (enumerator.MoveNext ()) {
			//foreach (KeyValuePair<string, ParticleSystem> item in emitterDict) {
				item = enumerator.Current;
				atmoSetting = GetAtmoParticleSetting (item.Key);
				// Reposition Emitter to playerPosition + ParticleOffset
				item.Value.transform.position = Player.Local.Position + atmoSetting.Offset;
				if (HasParticlesToEmit (item.Value, atmoSetting)) {
					// Prevent a wall of particles to appear by adjusting the "maxEmission"
					// on the ParticleSystem, Note: maxEmission is per second
					item.Value.emissionRate = atmoSetting.MaxParticles * atmoSetting.Density;
					item.Value.enableEmission = true;
				} else {
					item.Value.enableEmission = false;
				}
			}
		}
		KeyValuePair<string, ParticleSystem> item;
		protected AtmoParticleSetting atmoSetting;

		private void EmitBugSpots ()
		{
			var enumerator = BugEmitters.GetEnumerator ();
			while (enumerator.MoveNext ()) {
			//foreach (BugParticleSystem bugSystem in BugEmitters) {
				bugSystem = enumerator.Current;
				if (bugSystem == null)
					continue;
				// Adjust Emitter Density for desired Effect, based on the range of MaxEmitterRange
				if (bugSystem.Emitters.Count < bugSystem.Setting.MaxEmitterRange * bugSystem.Setting.EmitterDensity) {
					GameObject newBugEmitter = Instantiate (bugSystem.BugPrefab) as GameObject;
					bugSystem.SpawnRandomly (PlayerPosition, newBugEmitter);
				}
			}
		}
		protected BugParticleSystem bugSystem;

		private void ClearDeadBugSystems ()
		{
			for (int i = 0; i < BugEmitters.Count; i++) {
				if (BugEmitters [i] == null)
					continue;
				if (!BugEmitters [i].Alive) {
					BugParticleSystem bugSystem = BugEmitters [i];
					BugEmitters.RemoveAt (i);
					// Bugs continue to exist until destroyed
					for (int j = 0; j < bugSystem.Emitters.Count; j++) {
						Destroy (bugSystem.Emitters [j].transform.gameObject);
					}

					break;
				}
			}
		}

		private bool HasParticlesToEmit (ParticleSystem pEmitter, AtmoParticleSetting pSetting)
		{
			if (pEmitter.particleCount < pSetting.Density * pSetting.MaxParticles * GlobalDensityMultiplier)
				return true;

			return false;
		}

		private BugParticleSystem GetBugSystem (BugParticleSetting bugSetting)
		{
			var systemEnumerator = BugEmitters.GetEnumerator ();
			while (systemEnumerator.MoveNext ()) {
			// Check if BugEmitter was already created
			//foreach (BugParticleSystem bugSystem in BugEmitters) {
				bugSystem = systemEnumerator.Current;
				if (bugSystem == null)
					continue;

				if (bugSystem.Setting.BugType == bugSetting.BugType) {
					return bugSystem;
				}
			}

			// Attempt to Create BugSystem
			// Can only create if AtmoParticleManager has the prefab
			for (int i = 0; i < bugParticlePrefabs.Length; i++) {
				bugPrefab = bugParticlePrefabs [i];
				if (bugPrefab.name == bugSetting.BugType) {
					BugParticleSystem newBugSystem = new BugParticleSystem ();
					newBugSystem.BugPrefab = bugPrefab;
					// Add to List to find later to change settings, destroy, or emit
					BugEmitters.Add (newBugSystem);
					return newBugSystem;
				}
			}

			return null;
		}
		protected GameObject bugPrefab;

		public void ClearAll ()
		{
			//hard reset, destroys all emitters
			foreach (var item in emitterDict.Values) {
				item.Clear ();
			}
		}
	}

	[Serializable]
	public class AtmoParticleSetting
	{
		//this class must be XML serializable because we'll be saving
		//and loading it to disk - that's why I'm using SColor and SVector3
		public float Density = 0.0f;
		public Color BaseColor = Color.white;
		public Vector3 Gravity = Vector3.zero;
		public Vector3 Offset = Vector3.zero;
		public int MaxParticles = 1000;
		// Used for Blending Density values
		private float targetDensity	= 0.0f;
		private float initialDensity = 1.0f;

		public float TargetDensity {
			get { return targetDensity; }
			set {
				initialDensity = Density;
				targetDensity = value;
				//Reset BlendTimer
				blendTimer = 0.0f;
			}

		}

		private float blendTimer = 1.1f;
		// Only blend when TargetDensity is set
		public void Lerp (float blendSpeed)
		{
			if (blendTimer > 1)
				return;
			blendTimer += (float)Frontiers.WorldClock.ARTDeltaTime * blendSpeed;

			// Blend the Density over time by a set speed
			Density = Mathf.Lerp (initialDensity, TargetDensity, blendTimer);
		}
	}

	[Serializable]
	public class BugParticleSetting
	{
		public string BugType = "Horsefly";
		public float EmitterDensity = 0.0f;
		public float MaxEmitterRange = 10.0f;

		public BugParticleSetting (string BugType, float EmitterDensity, float MaxEmitterRange)
		{
			this.BugType = BugType;
			this.EmitterDensity = EmitterDensity;
			this.MaxEmitterRange = MaxEmitterRange;
		}
	}

	public class BugParticleSystem
	{
		public BugParticleSetting Setting;
		public GameObject BugPrefab;
		public List<ParticleSystem> Emitters = new List<ParticleSystem> ();
		public bool Alive = true;

		public void SpawnRandomly (Vector3 startPosition, GameObject newBugEmitter)
		{
			Vector3 ranPosition = new Vector3 (UnityEngine.Random.Range (-1.0f, 1.0f), 0, UnityEngine.Random.Range (-1.0f, 1.0f));
			ranPosition.Normalize ();
			ranPosition *= UnityEngine.Random.Range (0.0f, Setting.MaxEmitterRange);
			ranPosition += startPosition;
			newBugEmitter.transform.position = ranPosition;

			Emitters.Add (newBugEmitter.GetComponent<ParticleSystem> ());
		}
	}
}

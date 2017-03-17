using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.World {
	public class ObexCreature : MonoBehaviour {

		public Mesh TailMesh;
		public Transform TailBase;
		public Collider TailCollider;
		public Material TailMaterialBase;
		public Material TailMaterialIllum;
		//public InteractiveCloth TailCloth;
		public Transform HeadPivot;
		public AnimationCurve HeadBobCurve;
		public Quaternion TargetRotation;
		public AudioSource TalkAudio;
		public AudioSource ScanAudio;
		public AudioClip WarnClip;
		public AudioClip[] IdleClips;
		public AudioClip AttackClip;
		public AudioClip ScanClip;
		public ParticleSystem[] Particles;
		public float NormalEmissionRate;
		public float NormalEmissionSpeed;
		public float ElevatedEmissionRate;
		public float ElevatedEmissionSpeed;
		public float HeadBobTime;
		public Transform LookLight;
		public Light WarnLight;
		public float WarnLightIntensity = 5f;

		public Transform AttackTarget;

		public PowerBeam BeamPrefab;
		public MagicEffectSphere EffectSpherePrefab;

		PowerBeam mPowerBeam;
		MagicEffectSphere mEffectSphere;

		public bool Attacking = false;
		public bool Warning = false;

		public Material [] IllumMaterials;

		// Use this for initialization
		void Start () {

			Particles = gameObject.GetComponentsInChildren <ParticleSystem> ();

			//WorldClock w = GameObject.Find ("Frontiers_WorldClock").GetComponent <WorldClock> ();
			//w.Initialize ();
			//w.OnGameStart ();

			/*TailCloth = TailBase.gameObject.AddComponent <InteractiveCloth> ();
			TailCloth.mesh = TailMesh;
			ClothRenderer cr = TailBase.gameObject.AddComponent <ClothRenderer> ();
			cr.sharedMaterials = new Material[] { TailMaterialBase, TailMaterialIllum };
			TailCloth.useGravity = false;
			TailCloth.selfCollision = false;
			TailCloth.damping = 0.5f;
			TailCloth.bendingStiffness = 0.5f;
			TailCloth.thickness = 0.1f;
			TailCloth.randomAcceleration = new Vector3 (5f, 1f, 5f);
			TailCloth.friction = 0.5f;
			TailCloth.density = 1f;
			TailCloth.pressure = 2.5f;
			TailCloth.collisionResponse = 0.99f;
			TailCloth.attachmentResponse = 0.99f;
			TailCloth.attachmentTearFactor = 1f;
			TailCloth.tearFactor = 0f;
			TailCloth.AttachToCollider (TailCollider, false);*/
		}
		
		// Update is called once per frame
		void Update () {

			LookLight.Rotate (0f, 0f, 10f * Time.deltaTime);

			if (Attacking || Warning) {
				Quaternion prevRotation = transform.rotation;
				transform.LookAt (AttackTarget.position);
				Vector3 eulerFix = transform.eulerAngles;
				eulerFix.x = 0f;
				eulerFix.z = 0f;
				transform.eulerAngles = eulerFix;
				TargetRotation = transform.rotation;
				transform.rotation = Quaternion.Slerp (prevRotation, TargetRotation, Time.deltaTime);
				WarnLight.intensity = Mathf.Lerp (WarnLight.intensity, WarnLightIntensity, Time.deltaTime);
				return;
			}

			if (WarnLight.intensity > 0.001f) {
				WarnLight.intensity = Mathf.Lerp (WarnLight.intensity, 0f, Time.deltaTime);
			} else {
				WarnLight.intensity = 0f;
			}


			if (!TalkAudio.isPlaying) {
				if (Random.value < 0.001f) {
					TalkAudio.clip = IdleClips [Random.Range (0, IdleClips.Length)];
					TalkAudio.pitch = Random.Range (0.5f, 0.6f);
					TalkAudio.Play ();
				}
			}

			HeadBobTime += Time.deltaTime;

			transform.Translate (Vector3.forward * 0.1f * Time.deltaTime);
			if (Random.value < 0.001f) {
				TargetRotation = Quaternion.Euler (0f, Random.value * 360f, 0f);
			}
			transform.rotation = Quaternion.Slerp (transform.rotation, TargetRotation, Time.deltaTime * 0.1f);
			if (Time.timeScale == 0f) {
				//TailCloth.enabled = false;
			} else {
				//TailCloth.enabled = true;
				//TailCloth.externalAcceleration = -TailBase.forward + TailBase.up;
			}
			HeadPivot.localRotation = Quaternion.Euler (HeadBobCurve.Evaluate (HeadBobTime), 0f, 0f);

			if (Input.GetKeyDown (KeyCode.W)) {
				Warning = true;
				StartCoroutine (WarnOverTime ());
			}

			if (Input.GetKeyDown (KeyCode.A)) {
				Attacking = true;
				StartCoroutine (AttackOverTime ());
			}
		}

		IEnumerator WarnOverTime () {
			TalkAudio.clip = WarnClip;
			TalkAudio.pitch = 0.5f;
			TalkAudio.Play ();

			ScanAudio.Play ();

			for (int i = 0; i < Particles.Length; i++) {
				Particles [i].emissionRate = ElevatedEmissionRate;
				Particles [i].startSpeed = ElevatedEmissionSpeed;
			}

			yield return new WaitForSeconds (2f);

			for (int i = 0; i < Particles.Length; i++) {
				Particles [i].emissionRate = NormalEmissionRate;
				Particles [i].startSpeed = NormalEmissionSpeed;
			}

			Warning = false;
			yield break;
		}

		IEnumerator AttackOverTime () {

			TalkAudio.clip = AttackClip;
			TalkAudio.Play ();

			ScanAudio.Play ();

			for (int i = 0; i < Particles.Length; i++) {
				Particles [i].emissionRate = ElevatedEmissionRate;
				Particles [i].startSpeed = ElevatedEmissionSpeed;
			}

			if (mPowerBeam == null) {
				mPowerBeam = (GameObject.Instantiate (BeamPrefab.gameObject, HeadPivot.position, HeadPivot.rotation) as GameObject).GetComponent <PowerBeam> ();
			}
			mEffectSphere = (GameObject.Instantiate (EffectSpherePrefab.gameObject) as GameObject).GetComponent <MagicEffectSphere> ();
			mEffectSphere.transform.position = AttackTarget.position;
			mEffectSphere.RTExpansionTime = 3f;
			mEffectSphere.RTDuration = 10f;
			mEffectSphere.RTCooldownTime = 3f;

			mPowerBeam.WarmUpColor = Color.yellow;
			mPowerBeam.FireColor = Color.yellow;
			mPowerBeam.AttachTo (HeadPivot, AttackTarget);
			mPowerBeam.WarmUp ();
			mPowerBeam.Fire (5f);

			yield return new WaitForSeconds (3f);

			for (int i = 0; i < Particles.Length; i++) {
				Particles [i].emissionRate = NormalEmissionRate;
				Particles [i].startSpeed = NormalEmissionSpeed;
			}

			mPowerBeam.StopFiring ();

			while (mEffectSphere != null && !mEffectSphere.Depleted) {
				yield return null;
			}

			GameObject.Destroy (mEffectSphere.gameObject);

			Attacking = false;
			yield break;
		}
	}
}
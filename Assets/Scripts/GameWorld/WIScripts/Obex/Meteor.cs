using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World.WIScripts
{
	public class Meteor : WIScript, IGatherableResource
	{
		public IItemOfInterest IncomingGatherer { get; set; }

		public MeteorState State = new MeteorState ();

		public Light MeteorLight;
		public AudioSource MeteorAudio;
		public AudioClip MeteorAudioClip;
		public bool CrackedOpen = false;
		public ParticleSystem MeteorSmoke;

		[FrontiersColorAttribute]
		public string LightColor;

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public override void OnInitialized ()
		{
			CrackedOpen = false;
			if (MeteorLight == null) {
				MeteorLight = gameObject.AddComponent <Light> ();
				MeteorLight.color = Colors.Get.ByName (LightColor);
				MeteorLight.intensity = 0f;
				MeteorLight.range = 20f;
			}
			if (MeteorAudio == null) {
				MeteorAudio = gameObject.AddComponent <AudioSource> ();
				MeteorAudio.loop = true;
				MeteorAudio.clip = MeteorAudioClip;
				MeteorAudio.Play ();
				MeteorAudio.maxDistance = 500f;
				MeteorAudio.minDistance = 5f;
				MeteorAudio.rolloffMode = AudioRolloffMode.Linear;
			}
			worlditem.Get <Damageable> ().OnDie += OnDie;
			mEmissionRateTarget = 5f;
			mIntensityTarget = 1f;
			MeteorSmoke.enableEmission = true;

			if (State.SpawnTime < 0) {
				State.SpawnTime = WorldClock.AdjustedRealTime;
			}
		}

		public void OnTakeDamage ( ){
			MeteorLight.intensity = 2f;
			MeteorSmoke.emissionRate = 30f;
		}

		public void OnDie () {
			if (MeteorAudio != null) {
				MeteorAudio.Stop ();
			}
			if (MeteorLight != null) {
				MeteorLight.enabled = false;
			}
			MeteorSmoke.enableEmission = false;
			CrackedOpen = true;
			FXManager.Get.SpawnExplosionFX (ExplosionType.Simple, null, worlditem.Position);
			WorldItems.RemoveItemFromGame (worlditem);
		}

		public override void BeginUnload ()
		{
			//unparent the smoke effect and self destruct it
			if (MeteorSmoke != null) {
				MeteorSmoke.transform.parent = null;
				MeteorSmoke.enableEmission = false;
				MeteorSmoke.gameObject.AddComponent <DestroyThisTimed> ().DestroyTime = MeteorSmoke.startLifetime * 2;
			}
		}

		public void Update () {
			if (mDestroyed || worlditem.Is (WILoadState.PreparingToUnload | WILoadState.Unloading | WILoadState.Unloaded)) {
				return;
			}

			if (WorldClock.IsDay && !CrackedOpen) {
				if (WorldClock.AdjustedRealTime > State.SpawnTime + Globals.MeteorDaytimeSurvivalTime) {
					worlditem.Get <Damageable> ().InstantKill ("Daytime");
					enabled = false;
					return;
				}
			}

			if (MeteorLight != null && !CrackedOpen) {
				mIntensityTarget = Mathf.Abs (Mathf.Sin (Time.time)) + 0.4f;
				MeteorLight.intensity = Mathf.Lerp (MeteorLight.intensity, mIntensityTarget, Time.deltaTime);
				MeteorSmoke.emissionRate = Mathf.Lerp (MeteorSmoke.emissionRate, mEmissionRateTarget, Time.deltaTime);
			}
		}

		protected float mIntensityTarget;
		protected float mEmissionRateTarget;
	}

	[Serializable]
	public class MeteorState
	{
		public double SpawnTime = -1;
	}
}
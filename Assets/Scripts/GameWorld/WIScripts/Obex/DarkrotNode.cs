using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using System.Reflection;

namespace Frontiers.World.WIScripts
{
	public class DarkrotNode : MonoBehaviour, IPhotosensitive, IDispersible, IItemOfInterest
	{
		public ParticleEmitter SmokeEmitter;
		public ParticleEmitter GlowEmitter;
		public ParticleEmitter AcidBlobs;
		public SphereCollider Collider;
		public GameObject CenterGlow;
		public Renderer CenterGlowRenderer;
		public Renderer CenterGlowFormRenderer;
		public Material CenterGlowMaterial;
		public Material CenterGlowFormMaterial;
		public GameObject TransitionExplosion;
		public Color EmitColor;
		public int DarkrotFlowTextureIndex;
		public double TimeOffset;
		public float DarkrotAmount;
		public bool CenterAbsorbed = false;
		public float SmoothMovement = 1f;
		public float CenterGlowPulse = 0f;
		public double WarmupTime;

		public bool IsTimeToMove {
			get {
				return mFormed && !(mDispersing | mIsDispersed) && mWaitEndTime < WorldClock.AdjustedRealTime;
			}
		}

		public bool IsDispersed {
			get {
				return mIsDispersed;
			}
			set {
				mIsDispersed = value;
			}
		}

		public bool Dispersing {
			get {
				return mDispersing;
			}
		}

		public void Awake ()
		{
			mSmokeEmitterTr = SmokeEmitter.transform;
			mCenterGlowTr = CenterGlow.transform;
			mTr = transform;

			CenterGlowRenderer.enabled = false;
			CenterGlowFormRenderer.enabled = true;
			CenterGlowFormMaterial = CenterGlowFormRenderer.material;//get an instance of the material
			CenterGlowMaterial = CenterGlowRenderer.material;
			CenterGlowFormMaterial.SetFloat ("_DisintegrateAmount", 1.0f);
			EmitColor = Colors.Get.ByName ("RawDarkLuminiteLightColor");

			mRandomRotation = new Vector3 (UnityEngine.Random.Range (-1f, 1f), UnityEngine.Random.Range (-1f, 1f), UnityEngine.Random.Range (-1f, 1f));

			LightSources = new List <WorldLight> ();
			mDispersing = false;
			mIsDispersed = false;
			AcidBlobs.emit = false;
			GlowEmitter.emit = false;
			SmokeEmitter.emit = false;

			//copy properties from prefab
			mMaxEmissionSize = SmokeEmitter.maxSize;
			mMinEmissionSize = SmokeEmitter.minSize;
			mMaxEnergy = SmokeEmitter.maxEnergy;
			mRadius = Collider.radius;
			mSmokeTargetPosition = mSmokeEmitterTr.localPosition;
			mTargetPosition = mSmokeTargetPosition;
			mSmokeEmitterTr.localPosition = Vector3.zero;

			//get the proportionate size of the center glow
			mCenterGlowRadius = CenterGlow.transform.localScale.x / mRadius;

			Collider.isTrigger = true;
			Collider.enabled = false;

			mWaitEndTime = -1f;
		}

		public void Update ()
		{
			if (!GameManager.Is (FGameState.InGame)) {
				return;
			}

			CenterGlowPulse = Mathf.Sin ((float)((WorldClock.AdjustedRealTime % WorldClock.gDayCycleRT) * Globals.DarkrotPulseInterval));
			if (mFormed) {
				mRefreshExposure++;
				if (mRefreshExposure > 30) {
					mRefreshExposure = 0;
					RefreshExposure ();
				}

				if (LightExposure + HeatExposure > Globals.DarkrotMaxLightAndHeatExposure) {
					////Debug.Log ("DisperseD DUE TO LIGHT IN DARKROT NODE");
					Disperse (Mathf.Infinity);
				}

				if (!mDispersing) {
					SmoothLightExposure = Mathf.Lerp (SmoothLightExposure, LightExposure, (float)WorldClock.ARTDeltaTime);

					//update the flowing material / glow
					DarkrotFlowTextureIndex = Creatures.Get.DarkrotFlowTextures.NextIndex (DarkrotFlowTextureIndex);
					CenterGlowMaterial.SetTexture ("_BumpMap", Creatures.Get.DarkrotFlowTextures [DarkrotFlowTextureIndex]);
					CenterGlowMaterial.SetColor ("_EmiTint", Colors.Alpha (EmitColor, SmoothLightExposure));

					//move the node to our position (if we're still moving)
					if (SmoothMovement < 1f) {
						SmoothMovement = Mathf.SmoothStep (SmoothMovement, 1f, (float)((WorldClock.AdjustedRealTime - mStartMoveTime) / Globals.DarkrotMoveInterval));
						mTr.position = Vector3.Lerp (mTr.position, mTargetPosition, SmoothMovement);
					} else if (mWaitEndTime == float.MaxValue) {
						mWaitEndTime = WorldClock.AdjustedRealTime + Globals.DarkrotWaitInterval;
					}
				}
			} else {
				//ramp it down
				SmoothLightExposure = Mathf.Lerp (SmoothLightExposure, 0f, (float)WorldClock.ARTDeltaTime);
			}

			mRandomExposure.x = UnityEngine.Random.Range (-1f, 1f) * SmoothLightExposure * 0.05f;
			mRandomExposure.y = UnityEngine.Random.Range (-1f, 1f) * SmoothLightExposure * 0.05f;
			mRandomExposure.z = UnityEngine.Random.Range (-1f, 1f) * SmoothLightExposure * 0.05f;

			mCenterGlowTr.localPosition = mRandomExposure;
			mCenterGlowTr.Rotate (mRandomRotation * (float)WorldClock.ARTDeltaTime);
			mCenterGlowTr.localScale = vp_Utility.NaNSafeVector3 (
				Vector3.one * (float)(((mCenterGlowRadius * WarmupTime) + (CenterGlowPulse * WarmupTime)) / 2) + mRandomExposure,
				mCenterGlowTr.localScale);
		}

		protected int mRefreshExposure = 0;

		public void Move (Vector3 newPosition)
		{
			if (mDispersing || mIsDispersed || !mFormed)
				return;

			mStartPosition = mTr.position;
			mTargetPosition = newPosition;
			mStartMoveTime = WorldClock.AdjustedRealTime;
			mWaitEndTime = float.MaxValue;
			SmoothMovement = 0f;
		}

		#region IItemOfInterest implementation

		public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

		public bool Has (string scriptName)
		{
			return false;
		}

		public bool HasAtLeastOne (List <string> scriptNames)
		{
			return scriptNames == null || scriptNames.Count == 0;
		}

		public WorldItem worlditem { get { return null; } }

		public PlayerBase player { get { return null; } }

		public ActionNode node { get { return null; } }

		public bool Destroyed { get { return false; } }

		public WorldLight worldlight { get { return null; } }

		public Fire fire { get { return null; } }

		public bool HasPlayerFocus { get; set; }

		#endregion

		#region IPhotosensitive implementation

		public float Radius { get { return Collider.radius; } }

		public Vector3 Position { get { return mTr.position + Collider.center; } }

		public float LightExposure { get; set; }

		public float HeatExposure { get; set; }

		public List <WorldLight> LightSources { 
			get {
				if (mLightSources == null) {
					mLightSources = new List <WorldLight> ();
				}
				return mLightSources;
			}
			set {
				mLightSources = value;
			}
		}

		protected List <WorldLight> mLightSources = null;

		public List <Fire> FireSources { 
			get {
				if (mFireSources == null) {
					mFireSources = new List <Fire> ();
				}
				return mFireSources;
			}
			set {
				mFireSources = value;
			}
		}

		protected List <Fire> mFireSources = null;

		public Action OnExposureIncrease { get; set; }

		public Action OnExposureDecrease { get; set; }

		public Action OnHeatIncrease { get; set; }

		public Action OnHeatDecrease { get; set; }

		#endregion

		public float SmoothLightExposure;

		public void RefreshExposure ()
		{
			if (mDispersing | mIsDispersed) {
				return;
			}

			if (mLastExposureUpdate < 0) {
				mLastExposureUpdate = WorldClock.AdjustedRealTime - WorldClock.ARTDeltaTime;
			}

			LightManager.CalculateExposure (this, (float)(WorldClock.AdjustedRealTime - mLastExposureUpdate));

			float dissipation = 1f - ((LightExposure + HeatExposure) / Globals.DarkrotMaxLightAndHeatExposure);
			SmokeEmitter.maxSize = mMaxEmissionSize * dissipation;
			SmokeEmitter.maxEnergy = mMaxEnergy * dissipation;
			SmokeEmitter.minSize = mMinEmissionSize * dissipation;

			mLastExposureUpdate = WorldClock.AdjustedRealTime;
		}

		public void ExposureIncrease ()
		{
			RefreshExposure ();
		}

		public void ExposureDecrease ()
		{
			RefreshExposure ();
		}

		protected IItemOfInterest mIoiCheck = null;
		protected Luminite mluminiteCheck = null;
		protected LuminiteNode mluminiteNodeCheck = null;

		public void OnTriggerEnter (Collider other)
		{
			if (other.isTrigger)
				return;//lights will be handled elsewhere

			if (mIsDispersed | mDispersing)
				return;

			switch (other.gameObject.layer) {
			case Globals.LayerNumPlayer:
				if (Vector3.Distance (Player.Local.Position, Position) > Collider.radius) {
					Player.Local.Status.AddCondition ("Darkrot");
					MasterAudio.PlaySound (MasterAudio.SoundType.Darkrot, mTr, "DarkrotInfect");
					FXManager.Get.SpawnFX (Position, "AcidShower");
					DarkrotAmount = 0f;
					Disperse (Mathf.Infinity);
				}
				break;

			case Globals.LayerNumWorldItemActive:
										//maybe we're Luminte?
				mIoiCheck = null;
				if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem) {
					mluminiteCheck = null;
					mluminiteNodeCheck = null;
					if (mIoiCheck.worlditem.Is <Luminite> (out mluminiteCheck) && !mluminiteCheck.IsDark) {
						DarkrotAmount = mluminiteCheck.AbsorbDarkrot (DarkrotAmount);
						MasterAudio.PlaySound (MasterAudio.SoundType.Darkrot, mTr, "DarkrotAbsorb");
					} else if (mIoiCheck.worlditem.Is <LuminiteNode> (out mluminiteNodeCheck) && !mluminiteNodeCheck.IsDark) {
						DarkrotAmount = mluminiteNodeCheck.AbsorbDarkrot (DarkrotAmount);
						MasterAudio.PlaySound (MasterAudio.SoundType.Darkrot, mTr, "DarkrotAbsorb");
					}
					if (DarkrotAmount <= 0f) {
						Disperse (Mathf.Infinity);
					}
				}
				break;

			default:
				break;
			}
		}

		public bool Disperse (float duration)
		{
			if (mDispersing || mIsDispersed)
				return false;

			mDispersing = true;
			StartCoroutine (DisperseOverTime ());
			return true;
		}

		public void Form (float darkrotAmount)
		{
			DarkrotAmount = darkrotAmount;

			mIsDispersed = false;
			mDispersing = false;
			mDestroying = false;
			mFormed = false;

			TimeOffset = WorldClock.AdjustedRealTime;

			StartCoroutine (WarmUp ());
		}

		public void Destroy ()
		{
			if (!mDestroying) {
				mDestroying = true;
				StartCoroutine (DestroyOverTime ());
			}
		}

		protected bool mDispersing = false;
		protected bool mIsDispersed = false;
		protected bool mDestroying = false;
		protected bool mFormed = false;
		protected float mMaxEmissionSize = 1f;
		protected float mMinEmissionSize = 0.5f;
		protected float mMaxEnergy = 1f;
		protected float mRadius = 1f;
		protected float mSize = 1f;
		protected float mCenterGlowRadius;
		protected double mWaitEndTime = 0f;
		protected double mLastExposureUpdate = -1f;
		protected double mStartMoveTime = 0f;
		protected PropertyInfo mSmokeField;
		protected PropertyInfo mGlowField;
		protected PropertyInfo mAcidField;
		protected Vector3 mRandomRotation;
		protected Vector3 mRandomExposure;
		protected Vector3 mSmokeTargetPosition;
		protected Vector3 mStartPosition;
		protected Vector3 mTargetPosition;
		public Transform mTr;
		public Transform mSmokeEmitterTr;
		public Transform mCenterGlowTr;

		protected IEnumerator DestroyOverTime ()
		{
			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 2;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			GameObject.Destroy (CenterGlow.renderer.material);
			GameObject.Destroy (gameObject);
		}

		protected IEnumerator WarmUp ()
		{
			double warmupStart = WorldClock.AdjustedRealTime;
			double warmupEnd = warmupStart + Globals.DarkrotWarmupTime;

			while (WorldClock.AdjustedRealTime < warmupEnd) {
				WarmupTime = (WorldClock.AdjustedRealTime - warmupStart) / (warmupEnd - warmupStart);
				CenterGlowFormMaterial.SetFloat ("_DisintegrateAmount", (float)(1.0 - WarmupTime));
				mSmokeEmitterTr.localPosition = Vector3.Lerp (mSmokeEmitterTr.localPosition, mSmokeTargetPosition, (float)WarmupTime);
				yield return null;
			}

			WarmupTime = 1.0f;
			//move the transition explosion forward so it's in front of the node
			TransitionExplosion.transform.position = Vector3.MoveTowards (TransitionExplosion.transform.position, Player.Local.Position, 1.0f);
			TransitionExplosion.SetActive (true);//this will self-destruct
			//disable the form renderer and enable the final shiny renderer
			CenterGlowFormRenderer.enabled = false;
			CenterGlowRenderer.enabled = true;
			SmoothLightExposure = 0f;
			mSmokeEmitterTr.localScale = Vector3.one;
			mSmokeEmitterTr.localPosition = mSmokeTargetPosition;

			Collider.enabled = true;
			GlowEmitter.emit = true;
			SmokeEmitter.emit = true;
			mFormed = true;
			yield break;
		}

		protected IEnumerator DisperseOverTime ()
		{
			double coolDownStart = WorldClock.AdjustedRealTime;
			double coolDownEnd = coolDownStart + Globals.DarkrotWarmupTime;

			FXManager.Get.SpawnFX (mSmokeEmitterTr.position, "DarkrotPoof");
			CenterGlowFormRenderer.enabled = true;
			CenterGlowRenderer.enabled = false;
			GlowEmitter.emit = true;
			SmokeEmitter.emit = false;

			AcidBlobs.emit = true;
			MasterAudio.PlaySound (MasterAudio.SoundType.Darkrot, transform, "DarkrotDissipate");
			SmokeEmitter.emit = false;
			Vector3 lastPosition = mSmokeEmitterTr.localPosition;
			float normalizedDissipationTime = 0f;
			while (normalizedDissipationTime < 1.0f) {
				normalizedDissipationTime = (float)((WorldClock.AdjustedRealTime - coolDownStart) / (coolDownEnd - coolDownStart));
				CenterGlowFormMaterial.SetFloat ("_DisintegrateAmount", normalizedDissipationTime);
				mSmokeEmitterTr.localPosition = lastPosition + (Vector3.up * (Mathf.Sin ((float)(WorldClock.AdjustedRealTime * 2 + TimeOffset) * 0.1f)));
				yield return null;
			}

			CenterGlowFormRenderer.enabled = false;
			GlowEmitter.emit = false;
			AcidBlobs.emit = false;
			Collider.enabled = false;
			DarkrotAmount = 0f;
			//wait for particles to power down
			//also power down sphere
			mIsDispersed = true;
			enabled = false;
			yield break;
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;

namespace Frontiers
{
	public class PlayerGroundPath : MonoBehaviour
	{
		//this script is responsible for the glowy path that follows the player around
		public ParticleSystem ParticlePath;
		float PathFollowSpeed = 0.125f;
		float DistanceBetweenNodes = 3.0f;
		float WaveAmount = 0.25f;
		float TimeModifier = 1.0f;
		public static int MaxParticles = 128;
		public int NumParticles;
		public float PlayerDistanceFromPath;
		public float RandomPositionScale = 0.25f;
		public float RandomPositionSpeed = 30f;
		public float BasePathSize = 1.125f;
		public float RotationSpeed = -6f;
		public Vector3 PositionAboveGround = Vector3.up * 0.65f;
		public ParticleSystem.Particle[] particles;
		public Vector3[] RandomPositions;
		public float[] RandomOpacity;
		public Color[] RandomColors;

		public void Awake()
		{
			gameObject.layer = Globals.LayerNumScenery;
			PlayerDistanceFromPath = 0f;
		}

		public void Start()
		{
			NumParticles = MaxParticles;
			ParticlePath = gameObject.AddComponent <ParticleSystem>();
			ParticlePath.gravityModifier = 0f;
			ParticlePath.enableEmission = true;
			ParticlePath.emissionRate = 0f;
			ParticlePath.startSize = 1f;
			ParticlePath.startColor = Colors.Get.PathEvaluatingColor1;
			ParticlePath.maxParticles = NumParticles;
			ParticlePath.simulationSpace = ParticleSystemSimulationSpace.World;
			ParticlePath.playOnAwake = false;
			ParticlePath.loop = false;
			ParticlePath.renderer.material = Mats.Get.WorldPathGroundParticleMaterial;
			ParticlePath.renderer.receiveShadows = false;
			ParticlePath.renderer.castShadows = false;
			ParticlePath.Stop();

			particles = new ParticleSystem.Particle [NumParticles];
			RandomPositions = new Vector3 [NumParticles / 2];
			RandomOpacity = new float[NumParticles / 2];
			RandomColors = new Color[NumParticles / 2];

			ParticlePath.Emit(NumParticles);
			ParticlePath.GetParticles(particles);
			Vector3 lastPosition = Vector3.zero;
			Vector3 nextPosition = Random.onUnitSphere;
			float lastOpacity = 1f;
			float nextOpacity = Random.Range(2f, 1f);
			int randomLerp = 0;

			for (int i = 0; i < NumParticles; i++) {
				ParticleSystem.Particle p = particles[i];
				p.lifetime = 100f;
				p.startLifetime = Time.time;
				p.size = 1f;
				p.velocity = Vector3.forward * i;
				p.angularVelocity = 0f;
				p.color = Color.magenta;
				particles[i] = p;
			}

			int numTilNext = 8;
			for (int i = 0; i < NumParticles / 2; i++) {
				if (i % numTilNext == 0) {
					randomLerp = 0;
					lastPosition = nextPosition;
					lastOpacity = nextOpacity;
					if (i + numTilNext >= NumParticles) {
						numTilNext = NumParticles - i;
						nextPosition = Vector3.zero;
						nextOpacity = 1f;
					} else {
						nextPosition = Random.onUnitSphere;
						nextOpacity = Random.Range(2f, 1f);
					}
				}
				RandomColors[i] = Color.Lerp(Colors.Get.GeneralHighlightColor, Colors.Get.GenericHighValue, Random.value);
				RandomOpacity[i] = Mathf.Lerp(lastOpacity, nextOpacity, (float)randomLerp / numTilNext);
				RandomPositions[i] = Vector3.Lerp(lastPosition, nextPosition, (float)randomLerp / numTilNext) + (Random.onUnitSphere * 0.015f);
				randomLerp++;
			}

			mTerrainHit.groundedHeight = 1.0f;
		}

		public void FixedUpdate()
		{
			mTargetColor.a = 1f * Profile.Get.CurrentPreferences.Immersion.PathGlowIntensity;
			mCurrentColor = Color.Lerp(mCurrentColor, mTargetColor, Time.fixedDeltaTime);

			if (!Paths.HasActivePath)
				return;

			switch (TravelManager.Get.State) {
				case FastTravelState.None:
					//only check this when we're not fast-traveling
					if (PlayerDistanceFromPath > Globals.PathStrayDistanceInMeters) {
						if (mTimeAwayFromPath > Globals.PathStrayMinTimeInSeconds) {
							if (mTimeAwayFromPath > Globals.PathStrayMaxTimeInSeconds) {
								GUIManager.PostWarning("Stopped following path");
								Paths.ClearActivePath();
								mTimeAwayFromPath = 0.0f;
								return;
							}
						}
						mTimeAwayFromPath += WorldClock.ARTDeltaTime;
					} else {
						mTimeAwayFromPath = 0.0f;
					}
					//see what color we're supposed to be every few seconds
					if (Paths.IsEvaluating) {
						mTargetColor = Color.Lerp(Colors.Get.PathEvaluatingColor1, Colors.Get.PathEvaluatingColor2, Mathf.Abs(Mathf.Sin((float)(WorldClock.RealTime * 2))));
					} else if (mCheckPathColor > 5) {
						mCheckPathColor = 0;
						//TODO update this later
						//float meters = Paths.ParamToMeters(Follower.param, Paths.ActivePath.spline.Length);// Paths.ActivePath.MetersFromPosition(Follower.transform.position);
						mTargetColor = Colors.GetColorFromWorldPathDifficulty(Paths.ActivePath.SegmentFromMeters(0f).Difficulty);
					} else {
						mCheckPathColor++;
					}
					break;

				case FastTravelState.WaitingForNextChoice:
					mTargetColor = Colors.Alpha(mTargetColor, 0f);
					break;

				default:
					break;
			}
		}

		public void Update()
		{
			if (!GameManager.Is(FGameState.InGame)) {
				return;
			}

			PathFollowSpeed = 0.125f;
			DistanceBetweenNodes = 2.0f;
			WaveAmount = 0.25f;
			TimeModifier = 1.0f;
			int startIndex = 0;

			if (Player.Local.State.IsHijacked) {
				PathFollowSpeed = 1.0f;
				DistanceBetweenNodes = 4.0f;
				WaveAmount = 0.05f;
			}

			transform.position = Player.Local.FPSCameraSeat.position + Player.Local.FPSCameraSeat.forward;

			if (!Paths.HasActivePath || !Paths.ActivePath.HasCachedSplinePositions) {
				mTargetColor = Colors.Alpha(Colors.GetColorFromWorldPathDifficulty(PathDifficulty.Easy), 0f);
				mTotalLength = -1f;
			} else if (Paths.HasActivePath) {
				mCachedSplinePositions.Clear();
				if (mTotalLength < 0) {
					mTargetColor = Colors.Alpha(Colors.GetColorFromWorldPathDifficulty(PathDifficulty.Easy), 0f);
					mTotalLength = Paths.ActivePath.LengthInMeters;
				}

				PlayerDistanceFromPath = Vector3.Distance(Paths.Get.ActivePathFollower.tr.position, Player.Local.FPSCameraSeat.position) * Globals.InGameUnitsToMeters;
				mGroundPathSmoothTarget = Mathf.Lerp(mGroundPathSmoothTarget, Paths.Get.ActivePathFollower.param, PathFollowSpeed);
				mGroundActivePathSmoothTarget = Mathf.Lerp(mGroundActivePathSmoothTarget, Paths.Get.ActivePathFollower.param, PathFollowSpeed);

				mNormalizedTargetLength = (Globals.GroundPathFollowerNodes * DistanceBetweenNodes) / mTotalLength;
				mNormalizedExtent = (mNormalizedTargetLength / 2.0f);
				mNormalizedMidPoint = mGroundActivePathSmoothTarget;
				//mNormalizedStartPoint = Mathf.Clamp01(mNormalizedMidPoint - mNormalizedExtent);
				//mNormalizedEndPoint = mNormalizedMidPoint + mNormalizedExtent;
				//mNormalizedDistanceBetweenNodes = DistanceBetweenNodes / mTotalLength;
				mNormalizedDistance = mNormalizedStartPoint;
				Paths.ActivePath.GetCachedSplinePositions(mCachedSplinePositions, mNormalizedMidPoint, NumParticles, out mPathStartPosition, out startIndex);
			}

			int randomPositionIndex = (startIndex + Mathf.CeilToInt(Time.time * RandomPositionSpeed)) % RandomPositions.Length;
			int randomOpacityIndex =  (startIndex + Mathf.CeilToInt(Time.time * RandomPositionSpeed / 2)) % RandomOpacity.Length;
			//float currentOffset = mNormalizedMidPoint - mNormalizedExtent;
			float distanceFromCenter = 0f;
			float distanceFromPath = Mathf.Clamp01 (Paths.NormalizedDistanceFromPath - 0.2f);
			int midPoint = NumParticles / 2;
			mClearColor = Colors.Alpha(mCurrentColor, 0f);
			for (int i = 0; i < NumParticles; i++) {
				if (i < midPoint) {
					randomPositionIndex = (randomPositionIndex + 1) % RandomPositions.Length;
					randomOpacityIndex = (randomOpacityIndex + 1) % RandomOpacity.Length;
					distanceFromCenter = 1f - ((float)i / midPoint);
				} else {
					randomPositionIndex = randomPositionIndex > 0 ? randomPositionIndex - 1 : RandomPositions.Length - 1;
					randomOpacityIndex = randomOpacityIndex > 0 ? randomOpacityIndex - 1 : RandomOpacity.Length - 1;
					distanceFromCenter = ((float)(i - midPoint) / midPoint);
				}
				ParticleSystem.Particle p = particles[i];
				p.rotation = Time.time + i * RotationSpeed;
				p.color = Color.Lerp (Color.Lerp (RandomColors [randomOpacityIndex], mCurrentColor, 0.75f), mClearColor, (distanceFromCenter /*/ RandomOpacity [randomOpacityIndex]*/ * mTargetColor.a));
				if (i < mCachedSplinePositions.Count) {
					p.position = mCachedSplinePositions[i] + PositionAboveGround + ((RandomPositions[randomPositionIndex] * RandomPositionScale));
					p.size = Mathf.Lerp(BasePathSize, 0f, distanceFromPath) * RandomOpacity [randomOpacityIndex];
					particles[i] = p;
				}
				particles[i] = p;
			}

			ParticlePath.SetParticles(particles, NumParticles);
		}

		void OnDrawGizmos () {
			Gizmos.color = mCurrentColor;
			foreach (Vector3 p in mCachedSplinePositions) {
				Gizmos.DrawWireSphere(p, 1f);
			}
		}

		protected float mTotalLength;
		protected float mNormalizedTargetLength;
		protected float mNormalizedExtent;
		protected float mNormalizedMidPoint;
		protected float mNormalizedStartPoint;
		protected float mNormalizedEndPoint;
		protected float mNormalizedDistanceBetweenNodes;
		protected float mNormalizedDistance;
		protected Vector3 mInterceptorPosition;
		protected Vector3 mPathStartPosition;
		protected List <Vector3> mCachedSplinePositions = new List<Vector3>();
		protected GameWorld.TerrainHeightSearch mTerrainHit;
		protected float mGroundPathSmoothTarget = 0.0f;
		protected float mGroundActivePathSmoothTarget = 0.0f;
		protected Color mTargetColor = Color.white;
		protected Color mCurrentColor = Color.white;
		protected Color mClearColor = Color.white;
		protected bool mHasNotifiedOfStraying = false;
		protected double mTimeAwayFromPath = 0.0f;
		protected int mCheckPathColor = 0;
	}
}
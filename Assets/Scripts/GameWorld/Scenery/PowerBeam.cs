using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class PowerBeam : MonoBehaviour
		{
				public AudioSource BeamAudio;
				public AudioClip BeamFinish;
				public Transform tr;
				public Spline MasterSpline;
				public SplineMesh Mesh;
				public MeshRenderer Renderer;
				public Transform StartNode;
				public Transform MiddleNode;
				public Transform EndNode;
				public Vector3 StaticEndPoint = Vector3.zero;
				public IItemOfInterest TargetObject;
				public IItemOfInterest OriginObject;
				public Transform TargetObjectTransform;
				public Transform OriginObjectTransform;

				public Vector3 TargetPosition {
						get {
								if (TargetObject != null) {
										return TargetObject.Position;
								} else if (TargetObjectTransform != null) {
										return TargetObjectTransform.position;
								} else {
										return StaticEndPoint;
								}
						}
				}

				public Vector3 OriginPosition {
						get {
								if (OriginObject != null) {
										return OriginObject.Position;
								} else if (OriginObjectTransform != null) {
										return OriginObjectTransform.position;
								} else {
										return tr.position;
								}
						}
				}

				public Vector3 RandomWiggleStart;
				public Vector3 RandomWiggleMiddle;
				public Vector3 RandomWiggleEnd;
				public ParticleSystem Particles;
				public Color WarmUpColor;
				public Color FireColor;
				public Color BeamColor;
				public bool RequiresOriginAndTarget = true;
				public float OriginWiggleRange = 0.001f;
				public float MiddleWiggleRange = 0.05f;
				public float ParticleSize = 0.05f;
				public Quaternion StartBaseRotation;
				public Quaternion EndBaseRotation;
				public Quaternion MiddleBaseRotation;
				public Vector3 MiddleParticleDirection = Vector3.up * 0.1f;
				public Light BeamLight;
				public float LightValue = 0f;
				public float LightIntensity = 5f;
				public float TimeStarted;
				public float RTBurstDuration;

				public bool IsDestroyed {
						get {
								return mDestroyed;
						}
				}

				public void WarmUp()
				{

						BeamLight.enabled = true;
						BeamColor = WarmUpColor;

						MasterSpline.enabled = false;
						Renderer.enabled = false;
						Mesh.enabled = false;
						BeamLight.enabled = false;
						Particles.enableEmission = false;

						mWarmingUp = true;
						enabled = true;
				}

				public void Fire(float rtDuration)
				{
						mWarmingUp = false;

						if (!mFiring) {
								BeamColor.a = 0f;
						} else {
								TimeStarted = (float)WorldClock.AdjustedRealTime;
								rtDuration = rtDuration;
						}

						BeamColor = FireColor;

						BeamAudio.Play();

						MasterSpline.enabled = true;
						Renderer.enabled = true;
						Mesh.enabled = true;
						BeamLight.enabled = true;
						Particles.enableEmission = true;

						mFiring = true;
						enabled = true;
				}

				public void StopFiring()
				{
						BeamAudio.Stop();
						BeamAudio.PlayOneShot(BeamFinish);

						mFiring = false;
						MasterSpline.enabled = false;
						Renderer.enabled = false;
						Mesh.enabled = false;
						BeamLight.enabled = false;
						Particles.enableEmission = false;
						enabled = false;
				}

				protected bool mFiring = false;
				protected bool mWarmingUp = false;

				public void Start()
				{
						StartBaseRotation = Quaternion.Euler(0f, 90f, 0f);
						MiddleBaseRotation = Quaternion.Euler(0f, 90f, 90f);
						EndBaseRotation = Quaternion.Euler(0f, 90f, 0f);
						tr = transform;
						mFiring = false;
				}

				public void Update()
				{
						if (mDestroyed)
								return;

						if (!mFiring && !mWarmingUp)
								return;

						//if either one is gone we have to destroy the beam
						if ((TargetObject == null || OriginObject == null) && RequiresOriginAndTarget) {
								if (!mDestroyingBeam) {
										mDestroyingBeam = true;
										StartCoroutine(DestroyBeamOverTime());
								}
						}
						//if either one still exists
						//update that one until we've been destroyed
						tr.position = OriginPosition;
						StartNode.localPosition = Vector3.Lerp((RandomWiggleStart * OriginWiggleRange), StartNode.localPosition, 0.25f);
						StartNode.localRotation = Quaternion.Lerp(StartNode.localRotation, Quaternion.Euler(RandomWiggleStart * 90), 0.15f) * StartBaseRotation;
						//then update the end node
						EndNode.position = TargetPosition + (RandomWiggleEnd * OriginWiggleRange);
						EndNode.localRotation = Quaternion.Lerp(EndNode.localRotation, Quaternion.Euler(RandomWiggleEnd * 90), 0.15f) * EndBaseRotation;
						//finally make the middle node wiggle
						//send it between the start and the end
						//then add a bit of randomness
						MiddleNode.position = Vector3.Lerp((Vector3.Lerp(StartNode.position, EndNode.position, 0.5f) + RandomWiggleMiddle), MiddleNode.position, 0.05f);
						MiddleNode.localRotation = Quaternion.Lerp(MiddleNode.localRotation, Quaternion.Euler(RandomWiggleMiddle * 90), 0.15f) * MiddleBaseRotation;

						if (mWarmingUp) {
								LightValue = UnityEngine.Random.value;
								BeamLight.intensity = LightValue * LightIntensity;
								BeamLight.color = BeamColor;
								return;
						}

						LightValue = UnityEngine.Random.value;
						BeamLight.intensity = LightValue * LightIntensity;
						BeamLight.color = BeamColor;
						Renderer.material.SetColor("_TintColor", Colors.Alpha(BeamColor, LightValue));
						//start and end are stable, middle is crazy
						RandomWiggleStart.x = (UnityEngine.Random.value - 0.5f);
						RandomWiggleStart.y = (UnityEngine.Random.value - 0.5f);
						RandomWiggleStart.z = (UnityEngine.Random.value - 0.5f);

						RandomWiggleEnd.x = RandomWiggleStart.z;
						RandomWiggleEnd.y = RandomWiggleStart.x;
						RandomWiggleEnd.z = RandomWiggleStart.y;

						RandomWiggleMiddle.x = (UnityEngine.Random.value - 0.5f) * MiddleWiggleRange;
						RandomWiggleMiddle.y = (UnityEngine.Random.value - 0.5f) * MiddleWiggleRange;
						RandomWiggleMiddle.z = (UnityEngine.Random.value - 0.5f) * MiddleWiggleRange;

						float targetAlpha = 1f;
						if (mDestroyingBeam) {
								targetAlpha = 0f;
						}
						BeamColor.a = Mathf.Lerp(BeamColor.a, targetAlpha, (float)Frontiers.WorldClock.ARTDeltaTime * 2f);
				}

				public void FixedUpdate()
				{
						if (mDestroyed || !mFiring) {
								return;
						}
						//emit some cool particles
						Particles.Emit(StartNode.position, (RandomWiggleStart * 0.2f) + Vector3.down, ParticleSize * RandomWiggleMiddle.y, 0.25f + RandomWiggleEnd.z, BeamColor);
						Particles.Emit(EndNode.position, (RandomWiggleEnd * 0.2f) + Vector3.down, ParticleSize * RandomWiggleMiddle.x, 0.25f + RandomWiggleStart.z, BeamColor);
						//emit one at a random length along the spline
						float randomParticleMiddle = UnityEngine.Random.value;
						Particles.Emit(Vector3.Lerp(StartNode.position, EndNode.position, randomParticleMiddle), RandomWiggleMiddle + MiddleParticleDirection, ParticleSize * RandomWiggleMiddle.z, 0.25f + RandomWiggleMiddle.z, BeamColor);
				}

				public void AttachTo(Transform originObject, Transform targetObject)
				{
						tr.position = originObject.position;
						OriginObjectTransform = originObject;
						TargetObjectTransform = targetObject;
				}

				public void AttachTo(Transform originObject, IItemOfInterest targetObject)
				{
						tr.position = originObject.position;
						OriginObjectTransform = originObject;
						TargetObject = targetObject;
				}

				public void AttachTo(IItemOfInterest originObject, Transform targetObject)
				{
						tr.position = originObject.Position;
						OriginObject = originObject;
						TargetObjectTransform = targetObject;
				}

				public void AttachTo(IItemOfInterest originObject, IItemOfInterest targetObject)
				{
						tr.position = originObject.Position;
						OriginObject = originObject;
						TargetObject = targetObject;
				}

				protected IEnumerator DestroyBeamOverTime()
				{
						while (BeamColor.a > 0.01f) {
								yield return null;
						}
						GameObject.Destroy(gameObject);
						yield break;
				}

				protected void OnDestroy()
				{
						mDestroyed = true;
				}

				protected bool mDestroyed = false;
				protected bool mDestroyingBeam = false;
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		//a super-lightweight creature script for things like butterflies
		//everything is cached and super efficient
		public class Critter : MonoBehaviour, IItemOfInterest, IDamageable
		{
				//putting this here because yes, using gameObject name is slow and created garbage :/
				public Transform tr;
				public Rigidbody rb;
				public Collider BodyCollider;
				public Animation Animation;
				public AudioSource Audio;
				public AudioClip RoamClip;
				public AudioClip DeathClip;
				public Light RoamLight;
				public ICreatureDen Den;
				public float TargetHeight;
				public bool UsePlayerTargetHeight = true;

				#region item of interest implementation

				public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

				public Vector3 Position {
						get {
								if (!mDestroyed) {
										return tr.position;
								}
								return Vector3.zero;
						}
				}

				public bool Has(string scriptName)
				{
						return false;
				}

				public bool HasAtLeastOne(List <string> scriptNames)
				{
						if (scriptNames == null || scriptNames.Count == 0) {
								return true;
						}
						return false;
				}

				public WorldItem worlditem { get { return null; } }

				public PlayerBase player { get { return null; } }

				public ActionNode node { get { return null; } }

				public WorldLight worldlight { get { return null; } }

				public Fire fire { get { return null; } }

				public bool Destroyed { get { return mDestroyed; } }

				public void InstantKill(IItemOfInterest causeOfDeath)
				{
						OnDie();
				}

				public bool HasPlayerFocus {
						get {
								return mHasPlayerFocus;
						}
						set {
								if (!mHasPlayerFocus) {
										if (value) {
												OnGainPlayerFocus();
										}
								}
								mHasPlayerFocus = value;
						}
				}

				#endregion

				#region damageable implementation

				public BodyPart LastBodyPartHit { get; set; }

				public IItemOfInterest LastDamageSource { get; set; }

				public bool IsDead { get; set; }

				public float NormalizedDamage { get { return 0f; } }

				public WIMaterialType BaseMaterialType { get { return WIMaterialType.Flesh; } }

				public WIMaterialType ArmorMaterialTypes { get { return WIMaterialType.None; } }

				public int ArmorLevel(WIMaterialType materialType)
				{
						return 0;
				}

				public virtual bool TakeDamage(WIMaterialType materialType, Vector3 damagePoint, float attemptedDamage, Vector3 attemptedForce, string damageSource, out float actualDamage, out bool isDead)
				{
						actualDamage = attemptedDamage;
						isDead = true;
						OnDie();
						return true;
				}

				#endregion

				public float MaxSpeed;
				public bool Flies = false;
				public bool PeriodicSound = true;
				public float ChangeDirectionInterval;

				public void Awake()
				{
						tr = gameObject.transform;
						rb = gameObject.rigidbody;
						rb.useGravity = !Flies;
						rb.constraints = RigidbodyConstraints.FreezeRotation;
						BodyCollider = gameObject.collider;
						BodyCollider.enabled = true;
						Animation = gameObject.animation;
						Audio = gameObject.audio;
						if (RoamLight != null) {
								mTargetIntensity = RoamLight.intensity;
								RoamLight.intensity = 0f;
						}
						if (Audio != null) {
								if (Flies && !PeriodicSound) {
										Audio.clip = RoamClip;
										Audio.loop = true;
										Audio.Play();
								}
						}
				}

				public void Start()
				{
						enabled = false;
				}

				public void UpdateMovement(Vector3 playerPosition)
				{
						rb.AddForce(mForceDirection);
						if (WorldClock.AdjustedRealTime > mNextChangeTime) {
								mNextChangeTime = WorldClock.AdjustedRealTime + ChangeDirectionInterval + UnityEngine.Random.value;
								mForceDirection.x = UnityEngine.Random.Range(-MaxSpeed, MaxSpeed);
								mForceDirection.z = UnityEngine.Random.Range(-MaxSpeed, MaxSpeed);

						}
						mCurrentPosition = tr.position;
						if (Den != null) {
								//make sure we're within the radius
								float distance = Vector3.Distance(mCurrentPosition, Den.Position);
								if (distance > Den.Radius) {
										mForceDirection = (Den.Position - mCurrentPosition).normalized;
								}
						}
						if (Flies) {
								if (UsePlayerTargetHeight) {
										mForceDirection.y = (playerPosition.y + 2f) - mCurrentPosition.y;
								} else {
										mForceDirection.y = (TargetHeight) - mCurrentPosition.y;
								}
						} else {
								mForceDirection.y = MaxSpeed;
						}
						gVelocityCheck = rb.velocity;
						mSmoothVelocity = Vector3.Lerp(mSmoothVelocity, gVelocityCheck, 0.1f);
						if (mSmoothVelocity != Vector3.zero) {
								tr.rotation = Quaternion.LookRotation(mSmoothVelocity);
						}
						if (Audio != null) {
								if (PeriodicSound && UnityEngine.Random.value < 0.001f) {
										Audio.PlayOneShot(RoamClip);
								}
						}
						if (RoamLight != null) {
								RoamLight.intensity = Mathf.Lerp(RoamLight.intensity, mTargetIntensity, 0.05f);
						}
				}

				public void OnGainPlayerFocus()
				{

				}

				public void OnDie()
				{
						//bluarg
						IsDead = true;
						if (Audio != null) {
								Audio.PlayOneShot(DeathClip);
						}
				}

				public void OnDestroy()
				{
						mDestroyed = true;
				}

				protected float mTargetIntensity;
				protected double mNextChangeTime;
				protected Vector3 mCurrentPosition;
				protected Vector3 mForceDirection;
				protected Vector3 mSmoothVelocity;
				protected static Vector3 gVelocityCheck;
				protected bool mHasPlayerFocus = false;
				protected bool mDestroyed = false;
		}
}

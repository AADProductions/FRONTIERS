using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using TNet;
using Frontiers.World;

namespace Frontiers.World
{
		public class WorldBody : TNBehaviour
		{		//used as a base for CharacterBody and CreatureBody
				//central hub for animation components, sound components etc.
				//this is also where the bulk of networking is done for creatures / characters
				//also handles the really messy business of converting a body to ragdoll and back
				public TNObject NObject;
				public IBodyOwner Owner;
				public bool DisplayMode = false;
				public Transform RotationPivot;
				public Transform MovementPivot;
				public Rigidbody rb;
				public bool IsRagdoll;

				[NObjectSync]
				public Vector3 SmoothPosition {
						get {
								if (IsRagdoll && RootBodyPart != null) {
										return RootBodyPart.tr.position;
								}
								return mSmoothPosition;
						}
						set {
								mSmoothPosition = value;
						}
				}

				[NObjectSync]
				public Quaternion SmoothRotation {
						get {
								return mSmoothRotation;
						}
						set {
								mSmoothRotation = value;
						}
				}

				public BodyAnimator Animator = null;
				public BodyTransforms Transforms = null;
				public BodySounds Sounds = null;
				public BodyPart RootBodyPart = null;
				public float FootstepDistance = 0.15f;
				public System.Collections.Generic.List <BodyPart> BodyParts = new System.Collections.Generic.List <BodyPart>();
				public System.Collections.Generic.List <WearablePart> WearableParts = new System.Collections.Generic.List <WearablePart>();
				public System.Collections.Generic.List <EquippablePart> EquippableParts = new System.Collections.Generic.List <EquippablePart>();
				public System.Collections.Generic.List <Renderer> Renderers = new System.Collections.Generic.List <Renderer>();
				public string TransformPrefix = "Base_Human";

				public Material MainMaterial {
						get {
								return mMainMaterial;
						}
						set {
								try {
										mMainMaterial = value;
										if (mBloodSplatterMaterial == null) {
												//make a copy of the local blood splatter material
												if (BloodSplatterMaterial == null) {
														BloodSplatterMaterial = Mats.Get.BloodSplatterMaterial;
												}
												mBloodSplatterMaterial = new Material(BloodSplatterMaterial);
												mBloodSplatterMaterial.SetFloat("_Cutoff", 1f);
										}
										for (int i = 0; i < Renderers.Count; i++) {
												Renderer r = Renderers[i];
												if (r.CompareTag("BodyGeneral")) {
														Material[] currentSharedMaterials = r.sharedMaterials;
														if (currentSharedMaterials.Length > 1) {
																//we'll have to check for blood splatter mats
																System.Collections.Generic.List <Material> newSharedMaterials = new System.Collections.Generic.List <Material>(currentSharedMaterials);
																bool foundBloodMat = false;
																for (int j = 0; j < newSharedMaterials.Count; j++) {
																		if (newSharedMaterials[j].name.Contains("Blood")) {
																				newSharedMaterials[j] = mBloodSplatterMaterial;
																				foundBloodMat = true;
																		} else if (newSharedMaterials[j].name.Contains("Body")) {
																				newSharedMaterials[j] = mMainMaterial;
																		}
																}
																if (!foundBloodMat) {
																		newSharedMaterials.Add(mBloodSplatterMaterial);
																}
																r.sharedMaterials = newSharedMaterials.ToArray();
																newSharedMaterials.Clear();
														} else if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Body")) {
																Material[] newSharedMaterials = new Material [2];
																newSharedMaterials[0] = mMainMaterial;
																newSharedMaterials[1] = mBloodSplatterMaterial;
																r.sharedMaterials = newSharedMaterials;
														}
												}
										}
								} catch (Exception e) {
										Debug.Log(e);
								}
						}
				}

				public Material BloodSplatterMaterial;
				public System.Collections.Generic.List <Renderer> EyeRenderers = new System.Collections.Generic.List <Renderer>();
				public Material EyeMaterial;
				public Color EyeColor;
				public float EyeBrightness;
				public Color ScaredEyeColor;
				public Color TimidEyeColor;
				public Color AggressiveEyeColor;
				public Color HostileEyeColor;
				public Color TargetEyeColor;
				public float TargetEyeBrightness;

				public BodyEyeMode EyeMode {
						get {
								return mEyeMode;
						}
						set {
								if (mEyeMode != value) {
										mEyeMode = value;
										switch (EyeMode) {
												case BodyEyeMode.Scared:
														TargetEyeColor = ScaredEyeColor;
														break;

												case BodyEyeMode.Timid:
												default:
														TargetEyeColor = TimidEyeColor;
														break;

												case BodyEyeMode.Aggressive:
														TargetEyeColor = AggressiveEyeColor;
														break;

												case BodyEyeMode.Hostile:
														TargetEyeColor = HostileEyeColor;
														break;

												case BodyEyeMode.Dead:
														TargetEyeBrightness = 0f;
														TargetEyeColor = Color.black;
														EyeColor = TargetEyeColor;
														EyeBrightness = TargetEyeBrightness;
														RefreshEyes();
														break;
										}
								}
						}
				}

				protected BodyEyeMode mEyeMode = BodyEyeMode.Timid;

				public bool HasOwner {
						get {
								return Owner != null;
						}
				}

				public virtual void Initialize(IItemOfInterest bodyPartOwner)
				{
						gameObject.tag = "BodyGeneral";

						//if we haven't created our main texture set it now
						if (mMainMaterial == null) {
								//TEMP
								//TODO figure this out another way
								try {
										MainMaterial = Renderers[0].material;
								} catch (Exception e) {
										//Debug.LogError (e);
								}
						}

						for (int i = 0; i < BodyParts.Count; i++) {
								//if this is set to null the body part will set its tag so that it won't be recognized
								BodyParts[i].Initialize(bodyPartOwner, BodyParts);
						}

						if (EyeRenderers.Count > 0) {
								EyeMaterial = EyeRenderers[0].material;
								for (int i = 0; i < EyeRenderers.Count; i++) {
										EyeRenderers[i].sharedMaterial = EyeMaterial;
								}
						}

						RefreshShadowCasters();
				}

				public virtual void SetBloodColor(Color bloodColor)
				{
						if (mBloodSplatterMaterial == null) {
								mBloodSplatterMaterial = new Material(BloodSplatterMaterial);
						}
						mBloodSplatterMaterial.color = bloodColor;
				}

				public virtual void SetBloodOpacity(float bloodOpacity)
				{
						if (mBloodSplatterMaterial == null) {
								mBloodSplatterMaterial = new Material(BloodSplatterMaterial);
						}
						mBloodSplatterMaterial.SetFloat("_Cutoff", Mathf.Max(1.0f - bloodOpacity, 0.025f));
				}

				public void IgnoreCollisions(bool ignore)
				{
						if (IsRagdoll) {
								for (int i = 0; i < BodyParts.Count; i++) {
										BodyParts[i].RagdollRB.isKinematic = ignore;
										BodyParts[i].RagdollRB.detectCollisions = !ignore;
								}
						} else {
								rb.detectCollisions = !ignore;
						}
				}

				public void SetVisible(bool visible)
				{
						if (mDestroyed) {
								return;
						}

						try {
								for (int i = 0; i < Renderers.Count; i++) {
										if (Renderers[i].CompareTag ("BodyGeneral")) {
											Renderers[i].enabled = visible;
										}
										else {
											Renderers[i].enabled = false;
										}
								}
						} catch (Exception e) {
								Debug.LogError("Warning: Renderer null in " + name);
						}
						//Animator.animator.enabled = !visible;
				}

				public virtual void OnSpawn(IBodyOwner owner)
				{
						if (RootBodyPart == null) {
								for (int i = 0; i < BodyParts.Count; i++) {
										if (BodyParts[i].Type == BodyPartType.Hip) {
												RootBodyPart = BodyParts[i];
												break;
										}
								}
						}

						Owner = owner;
						owner.Body = this;
						SetVisible(true);
						IgnoreCollisions(false);
						Animator.enabled = true;
						enabled = true;
				}

				public virtual void Awake()
				{		//we're guaranteed to have this
						rb = gameObject.GetOrAdd <Rigidbody>();
						rb.interpolation = RigidbodyInterpolation.None;
						gameObject.layer = Globals.LayerNumBodyPart;
						NObject = gameObject.GetComponent <TNObject>();
						MovementPivot = transform;
						if (RotationPivot == null) {
								RotationPivot = MovementPivot;
						}
						MovementPivot.localRotation = Quaternion.identity;
						RotationPivot.localRotation = Quaternion.identity;
						// _worldBodyNetworkUpdateTime = NetworkManager.WorldBodyUpdateRate;
						// _bodyAnimatorNetworkUpdateTime = NetworkManager.BodyAnimatorUpdateRate;

						Animator = gameObject.GetComponent <BodyAnimator>();
						Animator.animator = gameObject.GetComponent <Animator>();
						if (Animator.animator == null) {
								Animator.animator = RotationPivot.gameObject.GetComponent <Animator>();
						}
						Transforms = gameObject.GetComponent <BodyTransforms>();
						Sounds = gameObject.GetComponent <BodySounds>();
						if (Sounds != null) {
								Sounds.Animator = Animator;
						}

						SetVisible(false);
						IgnoreCollisions(true);
				}

				public virtual void Update()
				{
						if (!GameManager.Is(FGameState.InGame) || DisplayMode)
								return;

						if (Owner == null) {
								return;
						}

						if (Owner.IsDestroyed) {
								GameObject.Destroy(gameObject);
								enabled = false;
								return;
						}

						if (IsRagdoll != Owner.IsRagdoll) {
								SetRagdoll(Owner.IsRagdoll, 0.1f);
								//wait for this to finish before the next update
								return;
						}

						//if we're the brain then we're the one setting the position
						//update the position based on the owner's position
						if (NObject.isMine && HasOwner) {

								if (Owner.IsRagdoll) {
										//don't do anything
										//let the owner pick up its position from our position
										return;
								}

								SmoothPosition = Owner.Position;
								SmoothRotation = Owner.Rotation;

								if (!Owner.Initialized || Owner.IsImmobilized) {
										//just snap to the owner's position and return
										return;
								}
								//otherwise update the movement and smooth movement

								if (Animator != null) {
										Animator.VerticalAxisMovement = (float)Owner.CurrentMovementSpeed;
										Animator.HorizontalAxisMovement = (float)Owner.CurrentRotationSpeed;
										Animator.YRotation = SmoothRotation.y;
								}

								//TODO reenable
									/*
								 	Decrease Timer
									_worldBodyNetworkUpdateTime -= Time.deltaTime;
									if (_worldBodyNetworkUpdateTime <= 0) {
						
										tno.Send ("OnNetworkWorldBodyUpdate", Target.Others, new WorldBodyUpdate (
											SmoothPosition, SmoothRotation));
																
										// Reset to send again
										_worldBodyNetworkUpdateTime = NetworkManager.WorldBodyUpdateRate;
									}
						
									_bodyAnimatorNetworkUpdateTime -= Time.deltaTime;
									if (_bodyAnimatorNetworkUpdateTime <= 0) {
										tno.Send ("OnBodyAnimatorUpdate", Target.Others, new BodyAnimatorUpdate (Animator));
						
										_bodyAnimatorNetworkUpdateTime = NetworkManager.BodyAnimatorUpdateRate;
									}
									*/
						}
						//do this regardelss of network state
						//this will ensure a smooth transition even if the updates don't happen very often
						if (!IsRagdoll) {
								mDistanceSinceLastFootstep += Vector3.Distance(rb.position, mSmoothPosition);
								if (mDistanceSinceLastFootstep > FootstepDistance) {
										Sounds.MakeFootStep();
										mDistanceSinceLastFootstep = 0f;
								}
								if (mDistanceSinceLastFootstep > gSnapDistance) {
										mSmoothPosition = MovementPivot.position; 
								}
						}
						rb.position = Vector3.Lerp(MovementPivot.position, mSmoothPosition, 0.5f);
						rb.rotation = Quaternion.Lerp(RotationPivot.rotation, mSmoothRotation, 0.125f);
						//MovementPivot.position = Vector3.Lerp (MovementPivot.position, mSmoothPosition, 0.5f);
						//RotationPivot.rotation = Quaternion.Lerp (RotationPivot.rotation, mSmoothRotation, 0.5f);
						RefreshEyes();
				}

				public static float gSnapDistance = 5f;

				protected void RefreshEyes()
				{
						if (EyeMaterial != null) {
								EyeColor = Color.Lerp(EyeColor, TargetEyeColor, (float)WorldClock.ARTDeltaTime);
								EyeBrightness = Mathf.Lerp(EyeBrightness, TargetEyeBrightness, (float)WorldClock.ARTDeltaTime);
								EyeMaterial.SetColor("_RimColor", Colors.Alpha(EyeColor, EyeBrightness));
						}
				}

				#region Network Specific Code
				// Internal network timer, decreased and reset based on the update function
				internal float _worldBodyNetworkUpdateTime = 1f;
				internal float _bodyAnimatorNetworkUpdateTime = 1f;

				public class WorldBodyUpdate
				{
						public Vector3 Position;
						public Quaternion Rotation;

						public WorldBodyUpdate(Vector3 position, Quaternion rotation)
						{
								Position = position;
								Rotation = rotation;
						}
				}

				public class BodyAnimatorUpdate
				{
						public int BaseMovementMode;
						public int OverrideMovementMode;
						public float VerticalAxisMovement;
						public float HorizontalAxisMovement;
						public bool TakingDamage;
						public bool Dead;
						public bool Warn;
						public bool Attack1;
						public bool Attack2;
						public bool Grounded;
						public bool Jump;
						public bool Paused;
						public bool Idling;

						public BodyAnimatorUpdate(BodyAnimator target)
						{
								BaseMovementMode = target.BaseMovementMode;
								OverrideMovementMode = target.BaseMovementMode;
								VerticalAxisMovement = target.VerticalAxisMovement;
								HorizontalAxisMovement = target.HorizontalAxisMovement;
								TakingDamage = target.TakingDamage;
								Dead = target.Dead;
								Warn = target.Warn;
								Attack1 = target.Attack1;
								Attack2 = target.Attack2;
								Grounded = target.Grounded;
								Jump = target.Jump;
								Paused = target.Paused;
								Idling = target.Idling;
						}
				}

				[RFC]
				public void OnNetworkWorldBodyUpdate(WorldBodyUpdate update)
				{

						SmoothPosition = update.Position;
						SmoothRotation = update.Rotation;
				}

				[RFC]
				public void OnBodyAnimatorUpdate(BodyAnimatorUpdate update)
				{
						if (Animator == null)
								return;

						Animator.BaseMovementMode = update.BaseMovementMode;
						Animator.VerticalAxisMovement = update.VerticalAxisMovement;
						Animator.HorizontalAxisMovement = update.HorizontalAxisMovement;
						Animator.TakingDamage = update.TakingDamage;
						Animator.Dead = update.Dead;
						Animator.Warn = update.Warn;
						Animator.Attack1 = update.Attack1;
						Animator.Attack2 = update.Attack2;
						Animator.Grounded = update.Grounded;
						Animator.Jump = update.Jump;
						Animator.Paused = update.Paused;
						Animator.Idling = update.Idling;
				}

				#endregion

				public bool GetBodyPart(BodyPartType type, out BodyPart part)
				{
						part = null;
						for (int i = 0; i < BodyParts.Count; i++) {
								if (BodyParts[i].Type == type) {
										part = BodyParts[i];
										break;
								}
						}
						return part != null;
				}

				public void SetRagdoll(bool ragdoll, float delay)
				{
						//don't do it again if we're already a ragdoll
						if (mConvertingToRagdoll) {
								return;
						} else if (ragdoll == IsRagdoll) {
								return;
						}

						mConvertingToRagdoll = true;
						StartCoroutine(ConvertToRagdollAfterTime(ragdoll, delay));
				}

				public void RefreshShadowCasters()
				{
						for (int i = 0; i < Renderers.Count; i++) {
								if (Renderers[i] != null) {
										Renderers[i].castShadows = Characters.CharacterShadows;
										Renderers[i].receiveShadows = Characters.CharacterShadows;
								}
						}
				}

				protected IEnumerator ConvertToRagdollAfterTime(bool ragdoll, float delay)
				{
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + delay;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}
						if (ragdoll) {
								//if we're converting TO a ragdoll
								//disable rigid body and animator
								//then enable the body part rigid bodies
								Animator.animator.enabled = false;
								Animator.enabled = false;

								for (int i = BodyParts.Count - 1; i >= 0; i--) {
										BodyParts[i].ConvertToRagdoll();
								}

								IgnoreCollisions(false);
								yield return null;
								//wait for rigid bodies to get initialized
								//then link them up and set links to zero

								for (int i = 0; i < BodyParts.Count; i++) {
										BodyParts[i].LinkRagdollParts(this);
								}

								IsRagdoll = true;

						} else {
								//if we're converting FROM a ragdoll
								//disable the body part rigid bodies
								//then enable rigid body and animator
								Component[] Joints = gameObject.GetComponentsInChildren <Joint>();
								for (int i = 0; i < Joints.Length; i++) {
										GameObject.Destroy(Joints[i]);
								}
								for (int i = BodyParts.LastIndex(); i >= 0; i--) {
										if (BodyParts[i].RagdollRB != null) {
												GameObject.Destroy(BodyParts[i].RagdollRB);
										}
								}

								for (int i = BodyParts.LastIndex(); i >= 0; i--) {
										BodyParts[i].ConvertToAnimated();
								}

								//IgnoreCollisions (false);
								Animator.animator.enabled = true;
								Animator.enabled = true;
								IsRagdoll = false;
						}
						mConvertingToRagdoll = false;
						yield break;
				}

				public void OnDestroy()
				{
						mDestroyed = true;
				}

				protected bool mDestroyed = false;
				protected bool mConvertingToRagdoll = false;
				protected float mDistanceSinceLastFootstep = 0f;
				protected Vector3 mSmoothPosition;
				protected Quaternion mSmoothRotation;
				protected Material mMainMaterial;
				protected Material mBloodSplatterMaterial;
		}

		public enum BodyEyeMode
		{
				Scared,
				Timid,
				Aggressive,
				Hostile,
				Dead,
		}

}
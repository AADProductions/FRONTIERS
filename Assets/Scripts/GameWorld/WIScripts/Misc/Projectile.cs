using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World.BaseWIScripts
{
		public class Projectile : WIScript
		{
				public SurfaceOrientation StickyOrientations = SurfaceOrientation.All;
				public string ProjectileType = "Projectile";
				public bool IsSticky = true;
				public float LaunchForce = 0f;
				public float DamagePerHit = 10.0f;
				public float MaximumDot = 0.0f;
				public bool Launched = false;
				public Transform StuckTo = null;
				public Collider CollidedWith = null;
				public LiveProjectile LiveUpdater = null;

				public bool IsLive {
						get {
								return LiveUpdater != null;
						}
				}

				public override void OnInitialized()
				{
						if (StuckTo != null) {
								enabled = true;
						}
				}

				public void Launch(Transform actionPoint, Weapon fromWeapon, float initialLaunchForce)
				{
						if (Launched || IsLive)
								return;	//can't launch if we've already launched

						//freeze the projectile
						worlditem.SetMode(WIMode.Hidden);
						//disable the arrow's collider - we're not depending on it for collisions any more
						worlditem.ActiveState = WIActiveState.Invisible;
						worlditem.ActiveStateLocked = true;
						transform.position = actionPoint.position;
						transform.rotation = actionPoint.rotation;
						//add the live projectile
						LiveUpdater = gameObject.AddComponent <LiveProjectile>();
						LiveUpdater.Initialize(this, actionPoint, fromWeapon, initialLaunchForce);
						//we're a go!
						enabled = true;
						Launched = true;
				}

				public void OnReachEnd()
				{
						if (StuckTo == null && CollidedWith == null) {
								Debug.Log("Projectile stuck to nothing - destroying");
								worlditem.SetMode(WIMode.RemovedFromGame);
						}
						Launched = false;
						if (LiveUpdater != null) {
								GameObject.Destroy(LiveUpdater);
						}
				}

				public void Stick(Transform stickTo)
				{
						StuckTo = stickTo;
						//TEMP
						return;
						//TEMP
						/*
						GameObject.Destroy (Stabilizer);
						rigidbody.mass 						= MassWhenStuck;
						rigidbody.isKinematic				= true;
						Tracer.autodestruct					= true;
						StuckTo								= stickTo;
						Launched							= false;
						*/
				}

				public void OnCollide(Collider withObject, Vector3 hitPoint)
				{
						CollidedWith = withObject;
						worlditem.ActiveStateLocked = false;
						worlditem.ActiveState = WIActiveState.Active;
						worlditem.tr.position = hitPoint;
						IItemOfInterest ioi = null;
						BodyPart bodyPart = null;

						if (gStuckToHelper == null) {
								gStuckToHelper = new GameObject("StuckToHelper").transform;
						}

						if (WorldItems.GetIOIFromCollider(withObject, out ioi, out bodyPart)) {
								switch (ioi.IOIType) {
										case ItemOfInterestType.Player:
												break;

										case ItemOfInterestType.Scenery:
										default:
												StuckTo = withObject.transform;
												break;

										case ItemOfInterestType.WorldItem:
												ProjectileTarget projectileTarget = null;
												if (ioi.worlditem.Is <ProjectileTarget>(out projectileTarget)) {
														projectileTarget.OnHitByProjectile(this, hitPoint);
												}
												if (bodyPart != null) {
														StuckTo = bodyPart.tr;
												} else {
														StuckTo = ioi.worlditem.tr;
												}
												break;
								}
						}
						if (StuckTo != null) {
								gStuckToHelper.parent = StuckTo;
								gStuckToHelper.position = worlditem.tr.position;
								gStuckToHelper.rotation = worlditem.tr.rotation;
								mStuckToLocalPosition = gStuckToHelper.localPosition;
								mStuckToLocalRotation = gStuckToHelper.localRotation;
								enabled = true;
						}
						worlditem.SetMode(WIMode.Frozen);
				}

				public void LateUpdate()
				{
						if (StuckTo != null) {
								if (worlditem.Is(WIActiveState.Visible | WIActiveState.Active)) {
										if (gStuckToHelper == null) {
												gStuckToHelper = new GameObject("StuckToHelper").transform;
										}
										gStuckToHelper.parent = StuckTo;
										gStuckToHelper.localPosition = mStuckToLocalPosition;
										gStuckToHelper.localRotation = mStuckToLocalRotation;
										worlditem.tr.position = gStuckToHelper.position;
										worlditem.tr.rotation = gStuckToHelper.rotation;
										worlditem.gameObject.layer = Globals.LayerNumBodyPart;
								}
						} else {
								for (int i = 0; i < worlditem.Colliders.Count; i++) {
										worlditem.Colliders[i].isTrigger = false;
								}
								worlditem.gameObject.layer = Globals.LayerNumWorldItemActive;
								worlditem.SetMode(WIMode.World);
								enabled = false;
						}
				}

				public void SetStackedMode()
				{
						Launched = false;
						CollidedWith = null;
						StuckTo = null;
				}

				protected static Transform gStuckToHelper;
				protected Vector3 mStuckToLocalPosition;
				protected Quaternion mStuckToLocalRotation;

		}
}
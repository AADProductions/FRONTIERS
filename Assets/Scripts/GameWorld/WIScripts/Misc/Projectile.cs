using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World
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
								worlditem.SetMode(WIMode.RemovedFromGame);
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
						if (WorldItems.GetIOIFromCollider(withObject, out ioi)) {
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
												StuckTo = ioi.worlditem.tr;
												worlditem.tr.parent = ioi.worlditem.tr;
												break;
								}
						}
						worlditem.SetMode(WIMode.Frozen);
				}

				public void SetStackedMode()
				{
						Launched = false;
						CollidedWith = null;
						StuckTo = null;
				}
		}
}
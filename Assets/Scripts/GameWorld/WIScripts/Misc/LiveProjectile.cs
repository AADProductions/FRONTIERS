using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.WIScripts;
using ExtensionMethods;

namespace Frontiers.World
{
		public class LiveProjectile : MonoBehaviour
		{		//attached to arrows and potentially other projectiles
				//not a WIScript since it will never be saved or loaded
				public static float gForceMultiplier = 10f;
				public static float gGravityMultiplier = 1f;
				public static float gGravity = 9.8f;
				public Projectile LaunchedProjectile;
				public Weapon FromWeapon;
				public GameObject TrajectoryObject;
				public GameObject ProjectileDoppleganger;
				public TrailRenderer Trail = null;
				public float InitialAngleInDegrees;
				public float InitialLaunchForce;
				public double CurrentTime;
				public float CurrentForce;
				public Vector2 Current2DPosition;
				public Vector3 CurrentOrientation;
				public Vector3 Current3DPosition;
				public Vector3 Last3DPosition;
				public bool Cooldown = false;

				public void Initialize(Projectile launchedProjectile, Transform actionPoint, Weapon fromWeapon, float initialLaunchForce)
				{
						FromWeapon = fromWeapon;
						LaunchedProjectile = launchedProjectile;
						TrajectoryObject = new GameObject("TrajectoryObject");
						InitialLaunchForce = initialLaunchForce;

						//align the trajectory object with the launched object
						//disregard the x rotation, we only want 2D orientation
						//then set parent to null;
						//finally, parent the launched projectile under this object
						//its 2D motion will now translate into 3D space
						TrajectoryObject.transform.position = actionPoint.position;
						Vector3 actionPointRotation = actionPoint.transform.rotation.eulerAngles;
						TrajectoryObject.transform.rotation = Quaternion.Euler(actionPointRotation.x, actionPointRotation.y, 0f);
						ProjectileDoppleganger = WorldItems.GetDoppleganger(launchedProjectile.worlditem, TrajectoryObject.transform, ProjectileDoppleganger);
						ProjectileDoppleganger.layer = Globals.LayerNumWorldItemActive;
						//use the trajectory's forward vector to determine the angle

						Trail = ProjectileDoppleganger.AddComponent <TrailRenderer>();
						Trail.startWidth = 0.01f;
						Trail.endWidth = 0.5f;
						Trail.autodestruct = true;
						Trail.time = 0.65f;
						Trail.material = Mats.Get.TrailRendererMaterial;

						InitialAngleInDegrees = Mathf.Acos(Vector3.Dot(TrajectoryObject.transform.forward, actionPoint.forward)).Clean();

						//create a damage package
						mDamage = new DamagePackage();
						mDamage.DamageSent = LaunchedProjectile.DamagePerHit * initialLaunchForce;
						mDamage.SenderMaterial = LaunchedProjectile.worlditem.Props.Global.MaterialType;
						mDamage.Source = fromWeapon.worlditem;
						mDamage.SenderName = fromWeapon.worlditem.DisplayName;

						Current3DPosition = ProjectileDoppleganger.transform.position;

						mInitialized = true;
				}

				public void UpdatePositionAlongTrajectory(double deltaTime)
				{
						float currentX = (float)(((InitialLaunchForce * gForceMultiplier) * Mathf.Cos(InitialAngleInDegrees)) * deltaTime);
						float currentY = (float)(((InitialLaunchForce * gForceMultiplier) * Mathf.Sin(InitialAngleInDegrees)) * deltaTime - ((0.5f * (gGravity * gGravityMultiplier)) * (deltaTime * deltaTime)));
						Current2DPosition.Set(currentX.Clean(), currentY.Clean());
				}

				public void Update()
				{
						if (!mInitialized)
								return;

						if (Cooldown) {
								if (Trail == null || LaunchedProjectile == null) {
										//trail has self-destructed
										enabled = false;
										GameObject.Destroy(this);
										GameObject.Destroy(ProjectileDoppleganger);
								} else {
										ProjectileDoppleganger.transform.position = LaunchedProjectile.worlditem.tr.position;
										ProjectileDoppleganger.transform.rotation = LaunchedProjectile.worlditem.tr.rotation;
								}
								return;
						}

						//send the projectile along the parameter
						CurrentTime += WorldClock.RTDeltaTime;
						Last3DPosition = Current3DPosition;
						UpdatePositionAlongTrajectory(CurrentTime);
						ProjectileDoppleganger.transform.localPosition = new Vector3(0f, Current2DPosition.y, Current2DPosition.x);
						Current3DPosition = ProjectileDoppleganger.transform.position;
						ProjectileDoppleganger.transform.position = Current3DPosition;
						CurrentForce = Vector3.Distance(Current3DPosition, Last3DPosition);
						CurrentOrientation = (Current3DPosition - Last3DPosition).normalized;
						if (CurrentOrientation != Vector3.zero) {
								ProjectileDoppleganger.transform.rotation = Quaternion.LookRotation(CurrentOrientation, Vector3.up);
						}

						//let worlditems in our potential path know that the need to be 'active'
						//otherwise the projectile might go right through them
						WorldItems.Get.SetActiveStateOverride (Current3DPosition, CurrentForce * 2);

						RaycastHit hitInfo;
						if (Physics.Linecast(Last3DPosition, Current3DPosition, out hitInfo, Globals.LayersActive)) {
								IItemOfInterest target = null;
								BodyPart bodyPart = null;
								if (!hitInfo.collider.isTrigger) {
										if (WorldItems.GetIOIFromCollider(hitInfo.collider, out target, out bodyPart)) {
												if (target.IOIType == ItemOfInterestType.Player) {
														//Debug.Log ("Was player");
														return;
												}
												mDamage.Point = hitInfo.point;
												mDamage.ForceSent = (CurrentForce + InitialLaunchForce) / 2;
												mDamage.Target = target;
												DamageManager.Get.SendDamage(mDamage, bodyPart);
										}
										LaunchedProjectile.worlditem.tr.position = ProjectileDoppleganger.transform.position;
										LaunchedProjectile.worlditem.tr.rotation = ProjectileDoppleganger.transform.rotation;
										LaunchedProjectile.OnCollide(hitInfo.collider, hitInfo.point);
										OnReachEnd();
								}
						}
				}

				public void OnReachEnd()
				{
						Cooldown = true;
						LaunchedProjectile.OnReachEnd();
						ProjectileDoppleganger.transform.parent = null;
						GameObject.Destroy(TrajectoryObject);
				}

				protected DamagePackage mDamage = null;
				protected bool mInitialized = false;
		}
}
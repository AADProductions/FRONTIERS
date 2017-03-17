using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
		public class Fire : MonoBehaviour
		{		//fire is probably the oldest class in the game
				//most of its functionality has been absorbed by other classes esp WorldLight
				//could probably eliminate it but haven't gotten around to it
				public GooThermalState ThermalState;
				public FireType Type = FireType.CampFire;
				public float InternalFuel = 0.0f;
				public GameObject SmokeObjectTemplate;
				public GameObject FireObjectTemplate;
				public Vector3 Offset;
				public GameObject FireObject;
				public GameObject SmokeObject;
				public Flammable FuelSource = null;
				public Transform FuelSourceTransform;
				public float FireScale = 8.0f;
				public float FireScaleMultiplier = 1.0f;
				public WorldLight FireLight;
				public float BurnHeat = 1f;
				public float WarmHeat = 0.1f;
				public Transform tr;
				public List <ParticleEmitter> ParticleEmitters = new List<ParticleEmitter>();

				public bool IsBurning(Vector3 position)
				{
						return Mathf.Clamp((Vector3.Distance(position, tr.position) - BurnScale), 0f, float.MaxValue) < Globals.FireBurnDistance;
				}

				public float BurnScale {
						get {
								return FireScale * Globals.FireBurnDistance;
						}
				}

				public float CookScale {
						get {
								return FireScale * Globals.FireCookDistance;
						}
				}

				public float WarmScale {
						get {
								return FireScale * Globals.FireWarmDistance;
						}
				}

				public float ScareScale {
						get {
								return FireScale * Globals.FireScareDistance;
						}
				}

				public void Awake()
				{
						gameObject.AddComponent <Rigidbody>();
						gameObject.GetComponent<Rigidbody>().isKinematic = true;
						gameObject.tag = "Fire";
						gameObject.layer = Globals.LayerNumScenery;
						ThermalState = GooThermalState.Normal;
						tr = transform;
				}

				int mCheckUpdate = 0;

				public void FixedUpdate()
				{
						mCheckUpdate++;
						if (mCheckUpdate < 10)
								return;

						mCheckUpdate = 0;
						if (FuelSourceTransform != null && FuelSourceTransform.hasChanged) {
								tr.position = FuelSourceTransform.position;
						}

						switch (ThermalState) {
								case GooThermalState.Igniting:
										Ignite();
										break;

								case GooThermalState.Burning:
										Burn();
										UpdateFireSize();
										UpdateVisibility();
										break;

								case GooThermalState.Smoldering:
										Smolder();
										UpdateFireSize();
										break;

								case GooThermalState.Normal:
										break;

								default:
										break;
						}
				}

				public void Burn()
				{
						float fuelBurned = (float)(Frontiers.WorldClock.ARTDeltaTime * Globals.FireBurnFuelRate);
			
						if (FuelSource == null) {
								if (InternalFuel <= 0.0f) {
										Extinguish();
								} else {
										InternalFuel -= fuelBurned;
								}
						} else {
								//set this so we can use it in fixed update
								if (FuelSourceTransform == null) {
										FuelSourceTransform = FuelSource.transform;
								}

								if (FuelSource.BurnFuel(fuelBurned) == false) {				
										Extinguish();
								}				
						}
				}

				public void Smolder()
				{
	
				}

				public void Ignite()
				{
						if (SmokeObject != null) {
								GameObject.DestroyImmediate(SmokeObject);
						}
	
						if (FireObject == null) {
								if (FireObjectTemplate == null) {
										FireObjectTemplate = FXManager.Get.SpawnFire(Type.ToString(), transform, Offset, Vector3.zero, FireScale, false);
								}
								FireObject = GameObject.Instantiate(FireObjectTemplate) as GameObject;
								FireObject.transform.parent = tr;
								FireObject.transform.localPosition = Offset;
								ParticleEmitters.AddRange(tr.GetComponentsInChildren <ParticleEmitter>());

						}

						if (FireLight == null) {
								FireLight = LightManager.GetWorldLight("CampfireLight", transform, Offset, true, WorldLightType.AlwaysOn);
								//this will turn the light into a proper fire light
								FireLight.ParentFire = this;
						}
	
						ThermalState = GooThermalState.Burning;
				}

				public static bool CanFireSeeItem(GameObject fireObject, GameObject nearbyWorldItem, int layerMask)
				{
						if (Physics.Linecast(fireObject.transform.position,
								 nearbyWorldItem.transform.position,
								 out mHitInfo,
								 Globals.LayerSolidTerrain)) {
								return false;
						}
						return true;
				}

				protected static RaycastHit mHitInfo;

				protected void UpdateVisibility()
				{
						if (Physics.Linecast(tr.position,
								 Player.Local.Position,
								 out mHitInfo,
								 Globals.LayerSolidTerrain | Globals.LayerStructureTerrain)) {
								if (mHitInfo.collider.gameObject != Player.Local.gameObject) {
										FireLight.IsOff = true;
										/*for (int i = 0; i < ParticleEmitters.Count; i++) {
												ParticleEmitters[i].enabled = false;
										}*/
								}
						} else {
								FireLight.IsOff = false;
								/*for (int i = 0; i < ParticleEmitters.Count; i++) {
										ParticleEmitters[i].enabled = true;
								}*/
						}
				}

				public void Extinguish()
				{
						mNearbyWorldItems.Clear();
						FireLight.Deactivate();
						for (int i = 0; i < ParticleEmitters.Count; i++) {
								ParticleEmitters[i].enabled = false;
						}
						GameObject.Destroy(FireObject, 0.5f);
						GameObject.Destroy(gameObject, 0.5f);
				}

				public void OnDrawGizmos()
				{
						/*
						Gizmos.color = Color.red;
						Gizmos.DrawWireSphere(FireLight.Position, BurnScale);
						Gizmos.color = Color.white;
						Gizmos.DrawWireSphere(FireLight.Position, CookScale);
						*/
				}

				protected void UpdateFireSize()
				{
						if (FireObject != null) {
								switch (ThermalState) {
										case GooThermalState.Burning:
												FireScale = MaxFireScale * FireScaleMultiplier;					
												break;
					
										default:
												FireScale = MinFireScale;
												break;
								}
								if (!Mathf.Approximately(FireScale, mLastFireScale)) {
										FireObject.SendMessage("UpdateScaleViaScript", FireScale, SendMessageOptions.DontRequireReceiver);
								}
								mLastFireScale = FireScale;
						}
				}

				protected List <GameObject>	mNearbyWorldItems = new List <GameObject>();
				protected float mUpdateTime = 0.0f;
				protected float mLastFireScale = -1f;
				public static float FireUpdateInterval = 1.0f;
				public static float MaxFireScale = 0.175f;
				public static float MinFireScale = 0.1f;
		}
}
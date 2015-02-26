using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;
using ExtensionMethods;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Ocean : Manager, IBodyOfWater
		{
				public static Ocean Get;

				public override string GameObjectName {
						get {
								return "Frontiers_Ocean";
						}
				}

				public Transform Pivot;
				public Transform OverlayPivot;

				public float WaterHeightAtPosition(Vector3 position)
				{
						return OceanSurfaceHeight;
				}

				public override void WakeUp()
				{
						Get = this;

						SubmergeTrigger = OceanTopCollider.gameObject.GetOrAdd <WaterSubmergeObjects>();
						SubmergeTrigger.OnItemOfInterestEnterWater += OnItemOfInterestEnterWater;
						SubmergeTrigger.OnItemOfInterestExitWater += OnItemOfInterestExitWater;
						SubmergeTrigger.Water = this;
				}

				public override void OnGameStart()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumFluidTerrain);

						GameObject LeviathanGameObject = GameObject.Instantiate(LeviathanPrefab) as GameObject;
						Leviathan = LeviathanGameObject.GetComponent <WaterLeviathan>();
						Leviathan.OnGameStart();

						if (!GameManager.Get.Ocean) {
								gameObject.SetActive(false);
						}
				}

				public WaterLeviathan Leviathan;
				public GameObject InlandLeviathanPrefab;
				public GameObject LeviathanPrefab;

				public float OceanSurfaceHeight {
						get {
								return OceanTopCollider.bounds.max.y;
						}
				}

				public Vector3 RandomPointOnOceanSurface(Vector3 point, float radius, bool outerEdgeOnly)
				{
						bool foundPoint = false;
						int maxTries = 10;
						int numTries = 0;

						mTerrainHit.groundedHeight = 5f;
						Vector3 normal = Vector3.up;

						Vector2 circlePoint = Vector2.zero;
						Vector3 newPoint = Vector3.zero;
						while (!foundPoint && numTries < maxTries) {
								if (outerEdgeOnly) {
										circlePoint = (UnityEngine.Random.insideUnitCircle.normalized * radius);
								} else {
										circlePoint = (UnityEngine.Random.insideUnitSphere.normalized * radius);
								}
								newPoint.x = circlePoint.x + point.x;
								newPoint.y = OceanSurfaceHeight;
								newPoint.z = circlePoint.y + point.z;
								mTerrainHit.feetPosition = newPoint;

								GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);//newPoint, false, ref hitWater, ref hitTerrainMesh, ref normal);
								if (mTerrainHit.hitWater) {
										foundPoint = true;
								} else {
										numTries++;
								}
						}
						return newPoint;
				}

				protected GameWorld.TerrainHeightSearch mTerrainHit;

				public bool SortSubmergedObjects(IItemOfInterest seeker, double interestInterval, out SubmergedObject subjergedObject)
				{
						subjergedObject = null;
						for (int i = SubmergedObjects.LastIndex(); i >= 0; i--) {
								SubmergedObject subObj = SubmergedObjects[i];
								if (subObj == null || subObj.Target == null || subObj.Target.Destroyed || (subObj.Target.IOIType == ItemOfInterestType.WorldItem && subObj.Target.worlditem.Is <WaterTrap>())) {
										SubmergedObjects.RemoveAt(i);
								} else {
										subObj.Seeker = seeker;
										if (subObj.Target.IOIType == ItemOfInterestType.Scenery) {
												//it's probably a fish or something
												//there's a very low probability that we care
												if (UnityEngine.Random.value < 0.0005f) {
														subObj.IsOfInterest = true;
												} else {
														subObj.IsOfInterest = false;
														SubmergedObjects.RemoveAt(i);
												}
										} else if (subObj.HasExitedWater && (WorldClock.AdjustedRealTime - subObj.TimeExitedWater) > interestInterval) {
												//just in case we're already targeting a submerged object
												subObj.IsOfInterest = false;
												SubmergedObjects.RemoveAt(i);
										} else if (subObj.Target.IOIType == ItemOfInterestType.Player && subObj.Target.player.IsDead) {
												subObj.IsOfInterest = false;
												SubmergedObjects.RemoveAt(i);
										} else if (subObj.Target.Position.y > Biomes.Get.TideWaterElevation) {
												//if the target's position is higher than the water position then it can't be underwater
												subObj.IsOfInterest = true;
												SubmergedObjects.RemoveAt(i);
										} else {
												subObj.IsOfInterest = true;
										}
								}
						}
						if (SubmergedObjects.Count > 0) {
								SubmergedObjects.Sort();
								if (SubmergedObjects[0].IsOfInterest) {
										subjergedObject = SubmergedObjects[0];
								}
						}
						return subjergedObject != null;
				}

				public void OnItemOfInterestEnterWater()
				{
						IItemOfInterest target = SubmergeTrigger.LastSubmergedItemOfInterest;
						for (int i = 0; i < SubmergedObjects.Count; i++) {
								//reset instead of adding new
								if (SubmergedObjects[i].Target == target) {
										SubmergedObjects[i].TimeExitedWater = -1f;
										return;
								}
						}
						SubmergedObjects.Add(new SubmergedObject(Leviathan, target, (float)WorldClock.AdjustedRealTime));
				}

				public void OnItemOfInterestExitWater()
				{
						IItemOfInterest target = SubmergeTrigger.LastExitedItemOfInterest;
						for (int i = SubmergedObjects.Count - 1; i >= 0; i--) {
								if (SubmergedObjects[i].Target == target) {
										SubmergedObjects[i].TimeExitedWater = (float)WorldClock.AdjustedRealTime;
										SubmergedObjects.RemoveAt(i);
										break;
								}
						}
				}

				public List <SubmergedObject> SubmergedObjects = new List <SubmergedObject>();
				public MeshRenderer OceanWaterRenderer;
				public MeshRenderer OceanFloorRenderer;
				public MeshRenderer OceanOverlayRenderer;
				public MeshFilter OceanSurfaceMeshFilter;
				public MeshFilter OceanOverlayMeshFilter;
				public Collider OceanBottomCollider;
				public Collider OceanTopCollider;
				public WaterSubmergeObjects SubmergeTrigger;
				public Mesh PartialMesh;
				public Mesh FullMesh;

				public OceanMode Mode {
						get {
								return mMode;
						}
				}

				public void SetMode(OceanMode mode)
				{
						Debug.Log("SETTING OCEAN MODE TO " + mode.ToString());
						mLastMode = mMode;
						mMode = mode;
						switch (mMode) {
								case OceanMode.Full:
								default:
										gameObject.SetLayerRecursively(Globals.LayerNumFluidTerrain);
										OceanWaterRenderer.enabled = true;
										OceanFloorRenderer.enabled = true;
										OceanOverlayRenderer.enabled = true;
										break;

								case OceanMode.Partial:
										gameObject.SetLayerRecursively(Globals.LayerNumScenery);
										OceanWaterRenderer.enabled = true;
										OceanFloorRenderer.enabled = true;
										OceanOverlayRenderer.enabled = true;
										break;

								case OceanMode.Disabled:
										gameObject.SetLayerRecursively(Globals.LayerNumHidden);
										OceanWaterRenderer.enabled = false;
										OceanFloorRenderer.enabled = false;
										OceanOverlayRenderer.enabled = false;
										break;
						}
				}

				public void Update()
				{
						if (GameManager.Is(FGameState.InGame) && !GameManager.Get.TestingEnvironment) {
								Mats.Get.WaterSurfaceMaterial.SetColor("_FogColor", TOD_Sky.GlobalFogColor);
								Mats.Get.WaveOverlayMaterial.SetColor("_FoamColor", Colors.Alpha(Color.Lerp(Color.white, RenderSettings.fogColor, 0.25f), 0.25f));
								Mats.Get.WaveOverlayMaterial.SetColor("_CrestColor", Colors.Alpha(Color.Lerp(Color.white, Colors.Brighten(RenderSettings.fogColor), 0.15f), 0.45f));
								Mats.Get.WaveOverlayMaterial.SetColor("_WaveColor", Colors.Alpha(Color.Lerp(Color.white, Colors.Desaturate(RenderSettings.fogColor), 0.15f), 0.55f));

								if (Player.Local.Surroundings.IsUnderground) {
										mMode = OceanMode.Disabled;
								} else {
										mMode = OceanMode.Full;
								}
						}
				}

				int layerLastFrame = 0;

				public void LateUpdate()
				{
						if (GameManager.Is(FGameState.InGame) && !GameManager.Get.TestingEnvironment) {
								Pivot.position = Player.Local.FPSController.SmoothPosition.WithY(Biomes.Get.TideWaterElevation);
								OverlayPivot.position = OverlayPivot.position.WithY(Biomes.Get.TideWaterElevation);
						}
				}

				protected OceanMode mLastMode = OceanMode.Full;
				public OceanMode mMode = OceanMode.Full;
		}

		[Serializable]
		public class SubmergedObject : IComparable <SubmergedObject>
		{
				public SubmergedObject(IItemOfInterest seeker, IItemOfInterest target, float timeEnteredWater)
				{
						Seeker = seeker;
						Target = target;
						TimeEnteredWater = timeEnteredWater;
						TimeExitedWater = -1f;
				}

				public int CompareTo(SubmergedObject other)
				{
						if (other.Seeker == null || this.Seeker == null) {
								return 0;
						}

						if (other.IsOfInterest == IsOfInterest) {
								//distance matters most
								float otherDist = Vector3.Distance(other.Target.Position, other.Seeker.Position);
								float thisDist = Vector3.Distance(Target.Position, Seeker.Position);
								return thisDist.CompareTo(otherDist);
						} else if (other.IsOfInterest) {
								//that means we're not of interest, so we lose
								return -1;
						} else {
								//that means we ARE of interest, so we win
								return 1;
						}
				}

				public IItemOfInterest Seeker;
				public IItemOfInterest Target;
				public float TimeEnteredWater;
				public float TimeExitedWater;
				public bool IsOfInterest = true;

				public bool HasExitedWater {
						get {
								return TimeExitedWater > TimeEnteredWater;
						}
				}
		}
}
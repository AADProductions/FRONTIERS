using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Reflection;

namespace Frontiers.World.Gameplay
{
		public class CreateCampsite : Skill
		{
				public EffectSphere SkillSphere;
				public Transform EffectSphereTransform;
				public Campsite ActiveCampsite;
				public List <IItemOfInterest> AvailableWaterSources = new List<IItemOfInterest>();
				public List <string> WaterSourceTypes;
				public Material PlacementMaterial;
				public bool WaterSourceAvailable = false;
				public float Orientation = -0.975f;

				public override void Initialize()
				{
						base.Initialize();
						WaterSourceTypes = new List<string>() { "BodyOfWater", "LiquidSource" };
				}

				public void OnEquipCampsite(Campsite activeCampsite)
				{
						AvailableWaterSources.Clear();
						ActiveCampsite = activeCampsite;
						enabled = true;
				}

				public void OnPlaceCampsite(Campsite campsite)
				{
						if (campsite == ActiveCampsite) {
								if (WaterSourceAvailable) {
										if (Player.Local.Surroundings.IsOutside) {
												StartCoroutine(CreateCampsiteOverTime(campsite));
												GUIManager.PostSuccess("Created new campsite");
										} else {
												campsite.State.HasBeenCreated = false;
												GUIManager.PostDanger("Couldn't create campsite: not outside");
										}
								} else {
										campsite.State.HasBeenCreated = false;
										GUIManager.PostDanger("Couldn't create campsite: no water sources nearby");
								}
						}
						StopPlacement();
				}

				protected IEnumerator CreateCampsiteOverTime(Campsite campsite)
				{
						//turn it into a proper campsite
						Location location = campsite.worlditem.GetOrAdd <Location>();
						location.State.Name.FileName = "Campsite_" + Mathf.Abs(campsite.worlditem.GetHashCode()).ToString();
						location.State.Name.CommonName = "Campsite";
						location.State.Type = "Campsite";
						location.State.IsCivilized = true;
						location.State.IsDangerous = false;
						location.State.Transform.CopyFrom(location.worlditem.tr);

						Revealable revealable = campsite.worlditem.GetOrAdd <Revealable>();
						revealable.State.IconName = "MapIconCampsite";
						revealable.State.LabelStyle = MapLabelStyle.MouseOver;

						Visitable visitable = campsite.worlditem.GetOrAdd <Visitable>();
						Player.Local.Surroundings.Reveal(campsite.worlditem.StaticReference);
						WorldMap.MarkLocation(campsite.worlditem.StaticReference);

						campsite.State.HasBeenCreated = true;
						campsite.State.CreatedByPlayer = true;
						campsite.RefreshFlag();
						yield return null;//wait a tick for the campsite to re-initialize
						campsite.worlditem.Save();//save state immediately - this will calculate stuff like chunk position

						yield break;
				}

				public void StartPlacement()
				{
						Debug.Log("Starting placement in create campsite");
						mPlacementStarted = true;
						GameObject skillSphere = new GameObject(name);
						SkillSphere = skillSphere.AddComponent <EffectSphere>();

						SkillSphere.TargetRadius = EffectRadius;
						SkillSphere.StartTime = WorldClock.Time;
						SkillSphere.RTDuration = Mathf.Infinity;
						SkillSphere.RTExpansionTime = 1.0f;
						SkillSphere.OnIntersectItemOfInterest += OnIntersectItemOfInterest;
						SkillSphere.RequireLineOfSight = false;

						GameObject effectSphere = new GameObject(name + " Effect");
						EffectSphereTransform = effectSphere.transform;
						effectSphere.layer = Globals.LayerNumScenery;
						MeshFilter mf = effectSphere.AddComponent <MeshFilter>();
						MeshRenderer mr = effectSphere.AddComponent <MeshRenderer>();
						mf.sharedMesh = Meshes.Get.EffectSphereMesh;
						mr.sharedMaterial = Mats.Get.ItemPlacementMaterial;
						mr.castShadows = false;
						mr.receiveShadows = false;
						PlacementMaterial = mr.material;
				}

				public void OnIntersectItemOfInterest()
				{
						while (SkillSphere.ItemsOfInterest.Count > 0) {
								IItemOfInterest ioi = SkillSphere.ItemsOfInterest.Dequeue();
								if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.HasAtLeastOne(WaterSourceTypes)) {
										AvailableWaterSources.SafeAdd(ioi);
								}
						}
				}

				public void StopPlacement()
				{
						if (EffectSphereTransform != null) {
								GameObject.Destroy(EffectSphereTransform.gameObject);
						}
						if (SkillSphere != null) {
								GameObject.Destroy(SkillSphere.gameObject);
						}
						if (PlacementMaterial != null) {
								GameObject.Destroy(PlacementMaterial);
						}
						WaterSourceAvailable = false;
						AvailableWaterSources.Clear();
						mPlacementStarted = false;
				}

				public bool CanBePlacedOn(Campsite campsite, IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						if (Vector3.Dot(normal, Vector3.down) < Orientation) {
								if (targetObject.IOIType == ItemOfInterestType.Scenery && targetObject.gameObject.CompareTag(Globals.TagGroundTerrain)) {
										return true;
								}
						}
						return false;
				}

				public void Update()
				{
						if (!GameManager.Is(FGameState.InGame) || !mPlacementStarted) {
								return;
						}

						if (ActiveCampsite == null || !ActiveCampsite.worlditem.Is(WIMode.Equipped)) {
								StopPlacement();
								return;
						}

						if (Player.Local.ItemPlacement.PlacementModeEnabled) {
								if (EffectSphereTransform == null) {
										StartPlacement();
								}
						} else {
								if (EffectSphereTransform != null) {
										StopPlacement();
								}
								return;
						}

						//if we've gotten this far it means placement mode is enabled and we've created our stuff
						//move our effect sphere to match the player item placement
						SkillSphere.tr.position = Player.Local.ItemPlacement.PlacementDopplegangerRigidbody.position;
						EffectSphereTransform.localScale = Vector3.one * SkillSphere.Collider.radius;
						EffectSphereTransform.localPosition = SkillSphere.tr.position;
						//check to make sure our water sources are still in range
						WaterSourceAvailable = false;
						for (int i = AvailableWaterSources.LastIndex(); i >= 0; i--) {
								//use their bounds to check
								float distance = Vector3.Distance(AvailableWaterSources[i].worlditem.Position, EffectSphereTransform.position)
								                 - Mathf.Max(AvailableWaterSources[i].worlditem.BaseObjectBounds.size.x, AvailableWaterSources[i].worlditem.BaseObjectBounds.size.z);
								if (distance < EffectRadius) {
										WaterSourceAvailable = true;
										break;
								}
						}
						//if placement is possible then make the effect sphere green (it will have asked us)
						if (Player.Local.ItemPlacement.PlacementPossible && WaterSourceAvailable && Player.Local.Surroundings.IsOutside) {
								PlacementMaterial.SetColor("_TintColor", Colors.Get.MessageSuccessColor);
						} else {
								PlacementMaterial.SetColor("_TintColor", Colors.Get.MessageDangerColor);
						}
				}

				protected bool mPlacementStarted = false;
		}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Locations
{
		public class City : WIScript
		{
				public CityState State = new CityState();
				public bool SpawnedCharacters = false;

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnActive += OnActive;
						worlditem.OnInactive += OnInactive;
						worlditem.OnInvisible += OnInvisible;
						worlditem.OnAddedToGroup += OnAddedToGroup;
						//get the location group
						Location location = null;
						if (worlditem.Is <Location>(out location)) {
								if (location.LocationGroup != null) {
										location.LocationGroup.Owner = worlditem;
								}
						}
				}

				public override bool FinishedUnloading {
						get {
								if (base.FinishedUnloading) {
										SpawnedCharacters = false;
										mSpawningCharacters = false;
										return true;
								}
								return false;
						}
				}

				public void OnGroupStateChange()
				{
						if (worlditem.Is(WIActiveState.Visible)) {
								OnVisible();
						}
				}

				public void OnVisible()
				{
						for (int i = 0; i < State.MinorStructures.Count; i++) {
								Structures.AddMinorToload(State.MinorStructures[i], i, worlditem);
						}

						if (!SpawnedCharacters && !mSpawningCharacters) {
								mSpawningCharacters = true;
								StartCoroutine(SpawnCharactersOverTime());
						}
				}

				public void OnAddedToGroup()
				{
						if (!State.HasSetFlags) {
								Region region = null;
								if (GameWorld.Get.RegionAtPosition(worlditem.Position, out region)) {
										State.ResidentFlags.Union(region.ResidentFlags);
										State.StructureFlags.Union(region.StructureFlags);
								}
								State.HasSetFlags = true;
						}
				}

				public void OnActive()
				{
						RefreshMinorStructureColliders();
				}

				public void OnInactive()
				{
						RefreshMinorStructureColliders();
				}

				public void OnInvisible()
				{
						RefreshMinorStructureColliders();
				}

				protected void RefreshMinorStructureColliders()
				{
						for (int i = 0; i < State.MinorStructures.Count; i++) {
								State.MinorStructures[i].RefreshColliders();
						}
				}

				public IEnumerator SpawnCharactersOverTime()
				{
						List <ActionNodeState> actionNodeStates = null;
						Location location = null;
						if (worlditem.Is <Location>(out location)) {
								while (location.LocationGroup == null || !location.LocationGroup.Is(WIGroupLoadState.Loaded)) {
										//wait for the location group to finish loading
										yield return null;
								}
								//TODO move this functionality into the Spawner script
								if (worlditem.Group.GetParentChunk().GetNodesForLocation(location.LocationGroup.Props.PathName, out actionNodeStates)) {
										Character spawnedCharacter = null;
										for (int i = 0; i < actionNodeStates.Count; i++) {
												ActionNodeState actionNodeState = actionNodeStates[i];
												if (actionNodeState.IsLoaded && actionNodeState.UseAsSpawnPoint) {
														if (string.IsNullOrEmpty(actionNodeState.OccupantName)) {
																Characters.SpawnRandomCharacter(
																		actionNodeState.actionNode,
																		State.RandomResidentTemplateNames,
																		State.ResidentFlags,
																		location.LocationGroup);
														} else {
																Characters.SpawnRandomCharacter(
																		actionNodeState.actionNode,
																		actionNodeState.OccupantName,
																		State.ResidentFlags,
																		location.LocationGroup,
																		out spawnedCharacter);
														}
												}
												yield return new WaitForSeconds(0.1f);
										}
								}
						}
						SpawnedCharacters = true;
						mSpawningCharacters = false;
						yield break;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.MinorStructures.Clear();
						foreach (Transform child in transform) {
								if (child.gameObject.HasComponent <StructureBuilder>() || child.name.Contains("-STR")) {
										MinorStructure ms = new MinorStructure();
										ms.TemplateName = StructureBuilder.GetTemplateName(child.name);
										ms.Position = child.transform.localPosition;
										ms.Rotation = child.transform.localRotation.eulerAngles;
										State.MinorStructures.Add(ms);
								}
						}
				}

				public override void OnEditorLoad()
				{
						foreach (MinorStructure ms in State.MinorStructures) {
								Transform msObject = gameObject.FindOrCreateChild(ms.TemplateName + "-STR");
								msObject.localPosition = ms.Position;
								msObject.localRotation = Quaternion.Euler(ms.Rotation);
						}
				}
				#endif
				protected bool mBuiltStructures = false;
				protected bool mSpawningCharacters = false;
		}

		[Serializable]
		public class CityState
		{
				public CharacterFlags ResidentFlags = new CharacterFlags();
				public WIFlags StructureFlags = new WIFlags();
				public bool HasSetFlags = false;
				public List <string> RandomResidentTemplateNames = new List <string>() { "Random" };
				public List <MinorStructure> MinorStructures = new List <MinorStructure>();
				public List <CityDistrictState> Districts = new List <CityDistrictState>();
		}

		[Serializable]
		public class CityDistrictState
		{
				public int Radius;
				public string Name;
		}
}
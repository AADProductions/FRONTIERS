using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;

namespace Frontiers.World.BaseWIScripts
{
		public class City : WIScript
		{
				public CityState State = new CityState();
				public bool HasSpawnedCharacters = false;
				public LookerBubble SharedLooker = null;
				public List <Character> SpawnedCharacters = new List<Character> ();

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
										HasSpawnedCharacters = false;
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

						if (!HasSpawnedCharacters && !mSpawningCharacters) {
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
								bool hasSpawnedMinorStructures = false;
								while (!hasSpawnedMinorStructures) {
										hasSpawnedMinorStructures = true;
										for (int i = 0; i < State.MinorStructures.Count; i++) {
												if (State.MinorStructures[i].LoadState != StructureLoadState.ExteriorLoaded) {
														hasSpawnedMinorStructures = false;
														break;
												}
										}
										yield return null;
								}
								//TODO move this functionality into the Spawner script
								if (worlditem.Group.GetParentChunk().GetNodesForLocation(location.LocationGroup.Props.PathName, out actionNodeStates)) {
										Character spawnedCharacter = null;
										for (int i = 0; i < actionNodeStates.Count; i++) {
												spawnedCharacter = null;
												ActionNodeState actionNodeState = actionNodeStates[i];
												if (actionNodeState.IsLoaded && actionNodeState.UseAsSpawnPoint && !actionNodeState.HasOccupant) {
														if (string.IsNullOrEmpty(actionNodeState.OccupantName)) {
																Characters.SpawnRandomCharacter (
																		actionNodeState.actionNode,
																		State.RandomResidentTemplateNames,
																		State.ResidentFlags,
																		location.LocationGroup,
																		out spawnedCharacter);
														} else {
																Characters.SpawnRandomCharacter(
																		actionNodeState.actionNode,
																		actionNodeState.OccupantName,
																		State.ResidentFlags,
																		location.LocationGroup,
																		out spawnedCharacter);
														}
														if (spawnedCharacter != null) {
																SpawnedCharacters.Add(spawnedCharacter);
														}
												}
												yield return WorldClock.WaitForSeconds(0.1);
										}
								}
						}
						HasSpawnedCharacters = true;
						//do we need to update our characters?
						if (SpawnedCharacters.Count > 0) {
								enabled = true;
						}
						mSpawningCharacters = false;
						yield break;
				}

				public void FixedUpdate ( ) {
						//TODO look into making this a coroutine
						mLookerCounter++;
						if (mLookerCounter > 2) {
								mLookerCounter = 0;
								if (SpawnedCharacters.Count > 0) {
										Looker looker = null;
										if (SharedLooker == null) {
												CreateSharedLooker();
										}
										if (!SharedLooker.IsInUse) {
												//if the looker is disabled that means it's done being used
												mUpdateCharacterIndex = SpawnedCharacters.NextIndex(mUpdateCharacterIndex);
												if (SpawnedCharacters[mUpdateCharacterIndex] != null) {
														if (SpawnedCharacters[mUpdateCharacterIndex].worlditem.Is <Looker>(out looker)) {
																//listener is passive but looker is active
																//it needs to be told to look for the player
																//we stagger this because it's an expensive operation
																looker.LookForStuff(SharedLooker);
														}
												} else {
														SpawnedCharacters.RemoveAt(mUpdateCharacterIndex);
												}
										}
								}
						}

						if (SpawnedCharacters.Count == 0) {
								enabled = false;
						}
				}

				public void CreateSharedLooker()
				{
						GameObject sharedLookerObject = gameObject.FindOrCreateChild("SharedLooker").gameObject;
						SharedLooker = sharedLookerObject.GetOrAdd <LookerBubble>();
						SharedLooker.FinishUsing();
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
				protected int mUpdateCharacterIndex = 0;
				protected int mLookerCounter = 0;
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
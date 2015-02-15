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
				public Location location = null;
				public bool HasSpawnedCharacters = false;
				public bool HasSpawnedMinorStructures = false;
				public LookerBubble SharedLooker = null;
				public List <Character> SpawnedCharacters = new List<Character>();
				public AudioSource CityAmbience;
				public AudioClip CityAmbienceClip;

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnActive += OnActive;
						worlditem.OnInactive += OnInactive;
						worlditem.OnInvisible += OnInvisible;
						worlditem.OnAddedToGroup += OnAddedToGroup;
						//get the location group
						location = worlditem.Get<Location>();
						location.OnLocationGroupLoaded += OnLocationGroupLoaded;
						location.OnLocationGroupUnloaded += OnLocationGroupUnloaded;
				}

				protected void OnLocationGroupLoaded()
				{
						//the minor structures will need to be created again
						//Debug.Log("ON LOCATION GROUP LOADED IN CITY " + name);
						OnVisible();
				}

				protected void OnLocationGroupUnloaded()
				{
						//the minor structures are going to be destroyed
						HasSpawnedCharacters = false;
						HasSpawnedMinorStructures = false;
						mSpawningCharacters = false;
						mSpawningMinorStructures = false;
				}

				public override void BeginUnload()
				{
						for (int i = 0; i < State.MinorStructures.Count; i++) {
								Structures.AddMinorToUnload(State.MinorStructures[i]);
						}
						HasSpawnedMinorStructures = false;
						mSpawningMinorStructures = false;
						SpawnedCharacters.Clear();
						HasSpawnedCharacters = false;
						mSpawningCharacters = false;
				}

				public void OnVisible()
				{
						if (!HasSpawnedMinorStructures && !mSpawningMinorStructures) {
								mSpawningMinorStructures = true;
								StartCoroutine(SpawnMinorStructuresOverTime());
						}

						if (!HasSpawnedCharacters && !mSpawningCharacters) {
								mSpawningCharacters = true;
								StartCoroutine(SpawnCharactersOverTime());
						}

						CityAmbience = gameObject.FindOrCreateChild("CityAmbience").gameObject.GetOrAdd <AudioSource> ();
						CityAmbience.clip = CityAmbienceClip;
						CityAmbience.volume = 0f;
						CityAmbience.loop = true;
						CityAmbience.enabled = true;
						CityAmbience.Play();
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
						if (HasSpawnedMinorStructures) {
								RefreshMinorStructureColliders();
						}
				}

				public void OnInactive()
				{
						if (HasSpawnedMinorStructures) {
								RefreshMinorStructureColliders();
						}
				}

				public void OnInvisible()
				{
						if (HasSpawnedMinorStructures) {
								RefreshMinorStructureColliders();
						}

						if (CityAmbience != null) {
								CityAmbience.enabled = false;
						}
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
						while (mSpawningCharacters && (location.LocationGroup == null || !location.LocationGroup.Is(WIGroupLoadState.Loaded))) {
								yield return null;
						}
						//set owner once it's available
						location.LocationGroup.Owner = worlditem;
						bool hasSpawnedMinorStructures = false;
						while (mSpawningCharacters && !hasSpawnedMinorStructures) {
								hasSpawnedMinorStructures = true;
								for (int i = 0; i < State.MinorStructures.Count; i++) {
										if (State.MinorStructures[i].LoadState != StructureLoadState.ExteriorLoaded) {
												hasSpawnedMinorStructures = false;
												break;
										}
								}
								yield return null;
						}

						if (!mSpawningCharacters) {
								yield break;
						}

						//TODO move this functionality into the Spawner script
						if (worlditem.Group.GetParentChunk().GetNodesForLocation(location.LocationGroup.Props.PathName, out actionNodeStates)) {
								Character spawnedCharacter = null;
								for (int i = 0; i < actionNodeStates.Count; i++) {
										if (!mSpawningCharacters) {
												//whoops, time to stop
												yield break;
										}

										spawnedCharacter = null;
										ActionNodeState actionNodeState = actionNodeStates[i];
										if (actionNodeState.IsLoaded && actionNodeState.UseAsSpawnPoint && !actionNodeState.HasOccupant) {
												if (string.IsNullOrEmpty(actionNodeState.OccupantName)) {
														Characters.SpawnRandomCharacter(
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
										double waitUntil = WorldClock.RealTime + 0.5f;
										while (WorldClock.RealTime < waitUntil) {
												yield return null;
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

				public IEnumerator SpawnMinorStructuresOverTime()
				{
						for (int i = 0; i < State.MinorStructures.Count; i++) {
								//they may have been asked to unload
								//so we'll need to wait until the structure manager is through with them
								while (mSpawningMinorStructures && Structures.IsUnloadingMinor(State.MinorStructures[i])) {
										Debug.Log("Waiting for minor structure to unload first");
										yield return null;
								}
								Structures.AddMinorToload(State.MinorStructures[i], i, worlditem);
								yield return null;
						}
						HasSpawnedMinorStructures = true;
						mSpawningMinorStructures = false;
						yield break;
				}

				public void FixedUpdate()
				{
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

						if (CityAmbience == null) {
								return;
						}

						float distance = Vector3.Distance(Player.Local.Position, worlditem.Position);
						if (distance < worlditem.ActiveRadius) {
								//if we're in the city put the ambience right there
								CityAmbience.transform.position = Player.Local.Position;
						} else {
								//otherwise put it at the edge of the active radius
								CityAmbience.transform.position = Vector3.MoveTowards(worlditem.Position, Player.Local.Position, worlditem.ActiveRadius);
						}

						if (Player.Local.Surroundings.IsUnderground) {
								CityAmbience.volume = Mathf.Lerp(CityAmbience.volume, 0f, 0.1f);
						} else {
								float volume = distance / worlditem.ActiveRadius;
								if (Player.Local.Surroundings.IsInsideStructure) {
										//cut the city ambience down by half
										CityAmbience.volume = Mathf.Lerp(CityAmbience.volume, volume * 0.125f, 0.1f);
								} else {
										CityAmbience.volume = Mathf.Lerp(CityAmbience.volume, volume, 0.1f);
								}
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
				protected bool mSpawningCharacters = false;
				protected bool mSpawningMinorStructures = false;
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
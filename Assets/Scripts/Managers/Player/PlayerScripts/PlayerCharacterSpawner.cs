using UnityEngine;
using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.Data;
using Frontiers.World.WIScripts;

namespace Frontiers
{
		public class PlayerCharacterSpawner : PlayerScript
		{
				public PlayerCharacterSpawnerState State = new PlayerCharacterSpawnerState();

				public void AddSpawnRequest(CharacterSpawnRequest spawnRequest)
				{
						if (State.SpawnRequests.SafeAdd(spawnRequest)) {
								spawnRequest.RTimeReceived = WorldClock.AdjustedRealTime;
								enabled = true;
						}
				}

				public void FixedUpdate()
				{
						if (State.SpawnRequests.Count == 0) {
								enabled = false;
						}

						for (int i = State.SpawnRequests.LastIndex(); i >= 0; i--) {
								CharacterSpawnRequest spawnRequest = State.SpawnRequests[i];
								if (WorldClock.AdjustedRealTime > spawnRequest.RTimeReceived + spawnRequest.MinimumDelay) {
										if (TryToSpawnCharacter(spawnRequest)) {
												State.SpawnRequests[i].Clear();
												State.SpawnRequests.RemoveAt(i);
										}
								}
						}
				}

				protected bool TryToSpawnCharacter(CharacterSpawnRequest spawnRequest)
				{
						if (spawnRequest.RequireOnFoot && !Player.Local.IsOnFoot) {
								//can't spawn character, player isn't on foot
								return false;
						}

						//get a point in the world behind the player
						mTerrainHit.feetPosition = Vector3.MoveTowards(Player.Local.Position, Player.Local.Position - Player.Local.ForwardVector, spawnRequest.MinimumDistanceFromPlayer);
						GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);

						if (mTerrainHit.hitWater) {
								//can't spawn character, hit water
								return false;
						}

						if (spawnRequest.SpawnNode == null) {
								//can't spawn character, spawn node was being created
								ActionNodeState spawnNodeState = null;
								GameWorld.Get.PrimaryChunk.GetOrCreateNode(WIGroups.Get.World, WIGroups.Get.World.transform, spawnRequest.ActionNodeName, out spawnNodeState);
								spawnRequest.SpawnNode = spawnNodeState.actionNode;
						}

						if (spawnRequest.SpawnNode == null) {
								//can't spawn character, spawn node was null
								return false;
						} else {
								spawnRequest.SpawnNode.transform.position = mTerrainHit.feetPosition;
								spawnRequest.SpawnNode.State.UseGenericTemplate = spawnRequest.UseGenericTemplate;
								spawnRequest.SpawnNode.State.CustomSpeech = spawnRequest.CustomSpeech;
								spawnRequest.SpawnNode.State.CustomConversation = spawnRequest.CustomConversation;
						}

						Character spawnCharacter = null;
						if (Characters.GetOrSpawnCharacter(spawnRequest.SpawnNode, spawnRequest.CharacterName, WIGroups.Get.World, out spawnCharacter)) {
								if (!string.IsNullOrEmpty(spawnRequest.DTSOnSpawn)) {
										spawnCharacter.LookAtPlayer();
										spawnCharacter.worlditem.Get <Talkative>().SayDTS(spawnRequest.DTSOnSpawn);
								}
								return true;
						} else {
								Debug.Log("Couldn't spawn character for some reason");
						}
						return false;
				}

				protected GameWorld.TerrainHeightSearch mTerrainHit;
		}

		[Serializable]
		public class PlayerCharacterSpawnerState
		{
				public List <CharacterSpawnRequest> SpawnRequests = new List <CharacterSpawnRequest>();
		}

		[Serializable]
		public class CharacterSpawnRequest
		{
				public void Clear()
				{
						SpawnNode = null;
						CharacterName = null;
						ActionNodeName = null;
						CustomSpeech = null;
						CustomConversation = null;
						DTSOnSpawn = null;
				}

				[XmlIgnore]
				public ActionNode SpawnNode;
				public string CharacterName;
				public bool UseGenericTemplate = false;
				public string ActionNodeName;
				public bool RequireOnFoot = true;
				public bool SpawnBehindPlayer = true;
				public bool FinishOnSpawn = true;
				public double RTimeReceived = 0f;
				public float MinimumDelay = 0f;
				public float MinimumDistanceFromPlayer = 4f;
				[FrontiersAvailableModsAttribute("Conversation")]
				public string CustomConversation;
				[FrontiersAvailableModsAttribute("Speech")]
				public string CustomSpeech;
				[FrontiersAvailableModsAttribute("Speech")]
				public string DTSOnSpawn;
		}
}
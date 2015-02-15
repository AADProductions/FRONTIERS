using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class TriggerCharacterSpawn : WorldTrigger
		{
				public TriggerCharacterSpawnState State = new TriggerCharacterSpawnState();
				public List <ActionNodeState> AvailableSpawnNodes = new List <ActionNodeState>();
				public ActionNode CurrentSpawnNode = null;
				public Character SpawnedCharacter = null;

				public override bool OnPlayerEnter()
				{
						if (AvailableSpawnNodes.Count == 0) {
								ParentChunk.GetNodes(State.AvailableSpawnNodes, true, AvailableSpawnNodes);
						}
						//now get the node we want to spawn at
						if (AvailableSpawnNodes.Count == 0) {
								Debug.Log("NO SPAWN NODES AVAILBLE in " + name);
								return false;
						}

						if (mSpawningOverTime) {
								return false;
						} else {
								mSpawningOverTime = true;
								StartCoroutine(SpawnOverTime());
								return true;
						}
				}

				public IEnumerator SpawnOverTime()
				{
						ActionNodeState nodeState = AvailableSpawnNodes[0];
						if (State.SpawnBehindPlayer) {
								Vector3 nodeDirection = Vector3.zero;
								float dot = 0f;
								float leastDotSoFar = Mathf.Infinity;
								for (int i = 0; i < AvailableSpawnNodes.Count; i++) {
										nodeDirection = (AvailableSpawnNodes[i].actionNode.Position - Player.Local.Position).normalized;
										dot = Vector3.Dot(Player.Local.FocusVector, nodeDirection);
										if (dot < leastDotSoFar) {
												leastDotSoFar = dot;
												nodeState = AvailableSpawnNodes[i];
										}
								}
						}
						CurrentSpawnNode = nodeState.actionNode;
						if (!string.IsNullOrEmpty(State.CustomSpeech)) {
								//overrides node state's speech
								nodeState.CustomSpeech = State.CustomSpeech;
						}
						if (!string.IsNullOrEmpty(State.CustomConversation)) {
								//overrides node state's speech
								nodeState.CustomConversation = State.CustomConversation;
						}
						//wait until we're ready to spawn
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + State.SpawnDelay;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}
						//then boom! go
						if (!Characters.GetOrSpawnCharacter(CurrentSpawnNode, State.CharacterName, ParentChunk.AboveGroundGroup, out SpawnedCharacter)) {
								Debug.Log("Couldn't spawn character");
						} else {
								if (!string.IsNullOrEmpty(State.DTSOnSpawn)) {
										if (!State.DTSFirstTimeOnly || State.NumTimesTriggered < 2) {
												Talkative talkative = null;
												if (SpawnedCharacter.worlditem.Is <Talkative>(out talkative)) {
														talkative.SayDTS(State.DTSOnSpawn);
														SpawnedCharacter.LookAtPlayer();
												}
										}
								}
						}
						mSpawningOverTime = false;
						yield break;
				}

				protected bool mSpawningOverTime = false;
		}

		[Serializable]
		public class TriggerCharacterSpawnState : WorldTriggerState
		{
				[FrontiersAvailableModsAttribute("Character")]
				public string CharacterName;
				[FrontiersAvailableModsAttribute("Conversation")]
				public string CustomConversation;
				[FrontiersAvailableModsAttribute("Speech")]
				public string CustomSpeech;
				[FrontiersAvailableModsAttribute("Speech")]
				public string DTSOnSpawn;
				public bool DTSFirstTimeOnly = true;
				public float SpawnDelay = 1.0f;
				public bool SpawnBehindPlayer = true;
				public bool ForceConversation = true;
				public bool DespawnOnExit = true;
				public float ForceConversationDelay = 2.0f;
				public float DespawnDelay = 5f;
				public WIFlags SpawnFlags = new WIFlags();
				public List <string> AvailableSpawnNodes = new List<string>();
		}
}
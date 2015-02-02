using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Residence : WIScript
		{
				public ResidenceState State = new ResidenceState();
				Structure structure;

				public override void OnInitialized()
				{
						if (worlditem.Is <Structure>(out structure)) {
								structure.OnOwnerCharacterSpawned += OnOwnerCharacterSpawned;
								structure.OnPlayerEnter += OnPlayerEnter;
						}
				}

				public void Knock ( ) {
						if (structure.State.OwnerSpawn.IsEmpty) {
								Debug.Log("No owner");
								GUI.GUIManager.PostInfo("You knock - no one answers.");
								//this structure has no owner so knocking accomplishes nothing
								return;
						}

						if (State.HasKnockedRecently) {
								//TODO maybe the character comments on how weird this is?
								return;
						}
						//this is called by doors
						State.LastKnockTime = WorldClock.AdjustedRealTime;
						//see if the interior is loaded
						//if it's not we know the player hasn't been here for a while
						if (structure.Is(StructureLoadState.InteriorLoaded)) {
								//do nothing
								return;
						} else {
								if (WorldClock.IsNight) {
										//if it's night then there's a random chance
										//that the resident won't answer
										if (UnityEngine.Random.value < 0.1f) {
												return;
										}
										GUI.NGUIScreenDialog.AddSpeech("It's too late for visitors, come back tomorrow!", string.Empty, 2f);
										return;
								} else {
										GUI.NGUIScreenDialog.AddSpeech("Come in!", string.Empty, 2f);
								}
						}
				}

				public void OnPlayerEnter ( ){
						if (!State.HasKnockedRecently) {
								//an owner character should have spawned by now
								//if they don't know the player, the character will be angry
								if (structure.OwnerCharacterSpawned) {
										if (!structure.StructureOwner.State.KnowsPlayer && !State.HasEnteredRecently) {
												GUI.NGUIScreenDialog.AddSpeech("How dare you just barge in here!",
														structure.StructureOwner.worlditem.DisplayName,
														3f);
												Profile.Get.CurrentGame.Character.Rep.LoseGlobalReputation(Globals.ReputationChangeSmall);
												Profile.Get.CurrentGame.Character.Rep.LosePersonalReputation(
														structure.StructureOwner.worlditem.FileName,
														structure.StructureOwner.worlditem.DisplayName,
														Globals.ReputationChangeHuge);
										}
								}
								State.LastEnteredTime = WorldClock.AdjustedRealTime;
						}
				}

				public void OnOwnerCharacterSpawned()
				{
						State.OwnerCharacterName = structure.StructureOwner.worlditem.FileName;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Location location = gameObject.GetComponent <Location>();
						location.State.Type = "Residence";
						if (string.IsNullOrEmpty(State.OwnerCharacterName)) {
								location.State.Name.CommonName = "Residence";
						}
				}
				#endif
		}

		[Serializable]
		public class ResidenceState
		{
				public bool HasKnockedRecently {
						get {
								return WorldClock.SecondsToHours (WorldClock.AdjustedRealTime - LastKnockTime) < Globals.NpcRequiredKnockHours;
						}
				}
				public bool HasEnteredRecently {
						get {
								return WorldClock.SecondsToHours (WorldClock.AdjustedRealTime - LastEnteredTime) < Globals.NpcRequiredKnockHours;
						}
				}
				public double LastKnockTime = 0f;
				public double LastEnteredTime = 0f;
				public string OwnerCharacterName = string.Empty;
		}
}

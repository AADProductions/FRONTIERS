using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class HouseOfHealing : WIScript
		{
				public string ConversationName;
				public Character Healer;
				public HouseOfHealingState State = new HouseOfHealingState();

				public override void OnInitialized()
				{
						Structure structure = worlditem.Get <Structure>();
						structure.State.IsRespawnStructure = true;
						structure.State.GenericEntrancesLockedTimes = TimeOfDay.a_None;
						structure.State.OwnerKnockAvailability = TimeOfDay.a_None;
						//make sure the structure can spawn a healer
						structure.State.OwnerSpawn.TemplateName = "Healer";
						structure.State.OwnerSpawn.CustomConversation = Globals.HouseOfHealingInteriorConversation;
						structure.State.OwnerSpawn.Interior = true;
				}

				public Vector3 WaitingToSpawnPosition {
						get { 
								return transform.position + (transform.forward * 5f);
						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						WIListOption listOption = new WIListOption("HouseOfHealing", "Request Rescue Services", "Rescue");
						if (State.ChosenByPlayer) {
								listOption.OptionText = "Cancel Rescue Services";
								listOption.NegateIcon = true;
						}
						options.Add(listOption);
				}

				public void OnPlayerUseWorldItemSecondary(object result)
				{
						WIListResult dialogResult = result as WIListResult;
						switch (dialogResult.SecondaryResult) {
								case "Rescue":
										if (State.ChosenByPlayer) {
												State.ChosenByPlayer = false;
												GUIManager.PostInfo("Cancelled rescue service");
										} else {
												State.ChosenByPlayer = true;
												GUIManager.PostInfo("Requested rescue service");
										}
										break;
						}
				}

				public IEnumerator SendPlayerToHealingBed()
				{
						worlditem.ActiveState = WIActiveState.Active;
						Structure structure = worlditem.Get <Structure>();
						while (!structure.Is(StructureLoadState.ExteriorLoaded)) {
								yield return null;
						}
						Structures.AddInteriorToLoad(structure);
						while (!structure.Is(StructureLoadState.InteriorLoaded)) {
								yield return null;
						}
						Player.Local.Spawn();
						//find the first bed
						List <WorldItem> bedWorldItems = structure.StructureGroup.GetChildrenOfType(new List<string>() { "Bed" });
						while (bedWorldItems.Count == 0) {
								double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.1f;
								while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
										yield return null;
								}
								bedWorldItems = structure.StructureGroup.GetChildrenOfType(new List<string>() { "Bed" });
						}
						WorldItem bedWorldItem = bedWorldItems[UnityEngine.Random.Range(0, bedWorldItems.Count)];
						Bed bed = bedWorldItem.Get <Bed>();
						Player.Local.transform.position = bed.BedsidePosition;
						bed.TryToSleep(WorldClock.Get.TimeOfDayAfter(WorldClock.TimeOfDayCurrent));
						yield break;
				}

				public static int CalculateHealDonation()
				{
						int cost = (int)(Player.Local.Inventory.InventoryBank.BaseCurrencyValue * Globals.HouseOfHealingHealCost);
						foreach (Condition condition in Player.Local.Status.State.ActiveConditions) {
								foreach (Symptom symptom in condition.Symptoms) {
										if (symptom.SeekType == StatusSeekType.Negative) {
												cost += Globals.HouseOfHealingCostPerNegativeSymptom;
										}
								}
						}
						return cost;
				}

				public static int CalculateRevivalDonation() {
						return (int)(Player.Local.Inventory.InventoryBank.BaseCurrencyValue * Globals.HouseOfHealingRevivalCost);
				}

				public static void HealAll(bool canAfford)
				{
						if (canAfford) {
								Player.Local.Inventory.InventoryBank.TryToRemove(CalculateHealDonation());
						} else {
								//reputation will automatically be clamped to min/max rep loss
								Profile.Get.CurrentGame.Character.Rep.LoseGlobalReputation(CalculateHealDonation());
						}

						foreach (Condition condition in Player.Local.Status.State.ActiveConditions) {
								foreach (Symptom symptom in condition.Symptoms) {
										if (symptom.SeekType == StatusSeekType.Negative) {
												//anything with a negative symptom is treatd as bad
												condition.Cancel();
												break;
										}
								}
						}

						Player.Local.Status.RestoreStatus(PlayerStatusRestore.F_Full, "Health");
						Player.Local.Status.RestoreStatus(PlayerStatusRestore.F_Full, "Strength");
				}

				#if UNITY_EDITOR
				public override void OnEditorLoad()
				{
						Structure structure = gameObject.GetComponent <Structure>();
						structure.State.IsRespawnStructure = true;
				}

				public override void OnEditorRefresh()
				{
						Structure structure = gameObject.GetComponent <Structure>();
						structure.State.IsRespawnStructure = true;
				}
				#endif
		}

		public class HouseOfHealingState
		{
				public int NumTimesHealedPlayer = 0;
				public bool ChosenByPlayer = false;
		}
}
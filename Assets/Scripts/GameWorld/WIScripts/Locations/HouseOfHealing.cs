using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class HouseOfHealing : WIScript
		{
				public HouseOfHealingState State = new HouseOfHealingState();

				public override void OnInitialized()
				{
						Structure structure = worlditem.Get <Structure>();
						structure.State.IsRespawnStructure = true;
						structure.State.GenericEntrancesLockedTimes = TimeOfDay.a_None;
						structure.State.OwnerKnockAvailability = TimeOfDay.a_None;
				}

				public Vector3 WaitingToSpawnPosition {
						get { 
								return transform.position + (transform.forward * 5f);
						}
				}

				public override void PopulateOptionsList(List<GUIListOption> options, List <string> message)
				{
						GUIListOption listOption = new GUIListOption("HouseOfHealing", "Request Rescue Services", "Rescue");
						if (State.ChosenByPlayer) {
								listOption.OptionText = "Cancel Rescue Services";
								listOption.NegateIcon = true;
						}
						options.Add(listOption);
				}

				public void OnPlayerUseWorldItemSecondary(object result)
				{
						OptionsListDialogResult dialogResult = result as OptionsListDialogResult;
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
								yield return new WaitForSeconds(0.1f);
								bedWorldItems = structure.StructureGroup.GetChildrenOfType(new List<string>() { "Bed" });
						}
						WorldItem bedWorldItem = bedWorldItems[UnityEngine.Random.Range(0, bedWorldItems.Count)];
						Bed bed = bedWorldItem.Get <Bed>();
						Player.Local.transform.position = bed.BedsidePosition;
						bed.TryToSleep(WorldClock.Get.TimeOfDayAfter(WorldClock.TimeOfDayCurrent));
						yield break;
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
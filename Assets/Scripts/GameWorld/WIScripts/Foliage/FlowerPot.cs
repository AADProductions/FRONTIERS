using UnityEngine;
using System.Collections;
using System;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class FlowerPot : WIScript
		{
				public FlowerPotState State = new FlowerPotState();
				public Transform DopplegangerParent;
				public GameObject PlantDoppleganger = null;

				public bool HasBeenPicked {
						get {
								return !string.IsNullOrEmpty(State.PlantName);
						}
				}

				public override void OnInitializedFirstTime()
				{
						if (State.AutoFill) {
								WorldItem plantPrefab = null;
								WorldItems.Get.PackPrefab("Plants", "WorldPlant", out plantPrefab);
								WorldItems.CloneWorldItem(plantPrefab, WIGroups.Get.Plants, out plantPrefab);

								if (!string.IsNullOrEmpty(State.PlantName)) {
										Plant props = null;
										if (Plants.Get.PlantProps(State.PlantName, ref props)) {
												//create a world plant
												//make it live in the pot
										}
								} else {

								}
						}
				}

				public override void PopulateOptionsList(List<WIListOption> options, List <string> message)
				{
						if (!HasBeenPicked && worlditem.Is(WIMode.Frozen)) {
								options.Add(new WIListOption("Pick " + worlditem.DisplayName, "PickPlant"));
						}
				}

				public virtual void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{	//this is where we handle skills
						WIListResult dialogResult = secondaryResult as WIListResult;
						switch (dialogResult.SecondaryResult) {
								case "PickPlant":
										PickPlant(true);
										break;

								default:
										break;
						}
				}

				public void PickPlant(bool addToInventory)
				{
						if (!HasBeenPicked) {
								//this will handle everything
								//Plants.Pick (this, addToInventory);
						}
				}

				public void OnVisible()
				{
						PlantDoppleganger = WorldItems.GetDoppleganger("Plants", "WorldPlant", DopplegangerParent, PlantDoppleganger, WIMode.World, string.Empty, "Default", State.PlantName, 1.0f, WorldClock.TimeOfDayCurrent, WorldClock.TimeOfYearCurrent);
				}
		}

		[Serializable]
		public class FlowerPotState
		{
				//public bool AutoFill = true;
				[FrontiersAvailableModsAttribute("Plant")]
				public string PlantName = "SwampFire";
				public bool AutoFill = true;
				//public StackItem Plant;
		}
}

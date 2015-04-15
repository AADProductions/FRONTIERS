using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
		//this serves as a base for crafting, brewing, food prep, etc
		public class CraftSkill : Skill
		{
				public CraftSkillExtensions Extensions;

				public override void Initialize()
				{
						base.Initialize();
						mCheckRequirements = new GenericWorldItem();
						if (Extensions == null) {
								Extensions = new CraftSkillExtensions();
						}
				}

				public override float ProgressValue {
						get {
								return mProgressValue;
						}
						set { }
				}

				public override float NormalizedEffectTimeLeft {
						get {
 								if (IsInUse) {
										return 1.0f - mProgressValue;
								}
								return 0f;
						}
				}

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (base.DoesContextAllowForUse(targetObject)) {
								CraftingItem craftingItem = targetObject.gameObject.GetComponent <CraftingItem>();
								if (craftingItem.SkillToUse == name) {
										return true;
								}
						}
						return false;
				}

				public IEnumerator CraftItems(
						WIBlueprint blueprint,
						CraftingItem craftingItem,
						int totalItems,
						List <List <InventorySquareCrafting>> rows,
						InventorySquareCraftingResult resultSquare)
				{
						int currentItem = 0;
						double startTime = 0f;
						double endTime = 0f;
						double timeOffset = 0f;
						double timeMultiplier = 1f;
						double craftTime = 0f;
						double craftSkillUsageValue = 0f;

						mProgressValue = 0f;

						UseStart(true);

						//here we go with the crafting...
						while (currentItem < totalItems && !mCancelled) {
								craftTime = blueprint.BaseCraftTime * EffectTime;
								craftSkillUsageValue = 1.0f;//TODO apply modifiers
			
								startTime = WorldClock.RealTime;
								endTime = startTime + craftTime;

								if (totalItems > 1) {
										timeOffset = (float)(currentItem - 1) / (float)totalItems;
										timeMultiplier = 1.0f / totalItems;
										//crafting time is reduced in proportion to the number of items you're crafting at once

								}
								//ok, if we can consume the requirements, we're good to go

								if (!ConsumeRequirements(rows)) {
										//Debug.Log ("Couldn't consume requirements");
										break;
								} else {
										while (WorldClock.RealTime < endTime && !ProgressCanceled) {
												double normalizedProgress = (WorldClock.RealTime - startTime) / (endTime - startTime);
												mProgressValue = (float)((normalizedProgress * timeMultiplier) + timeOffset);//make sure it doesn't accidentally stop
												mProgressValue = Mathf.Clamp(mProgressValue, 0.001f, 0.999f);
												yield return null;
										}

										if (!ProgressCanceled) {
												resultSquare.NumItemsCrafted++;
												currentItem++;
										}
								}
								yield return null;
						}
						//just in case
						mProgressValue = 1.0f;

						UseFinish();
						yield break;
				}

				public void LoadBlueprintsRows(List <List <InventorySquareCrafting>> rows, WIBlueprint blueprint, InventorySquareCraftingResult resultSquare)
				{
						LoadBlueprintRow(rows[0], blueprint.Row1, blueprint.Strictness);
						LoadBlueprintRow(rows[1], blueprint.Row2, blueprint.Strictness);
						LoadBlueprintRow(rows[2], blueprint.Row3, blueprint.Strictness);
						resultSquare.RequirementsMet = false;
				}

				protected void LoadBlueprintRow(List <InventorySquareCrafting> row, List <GenericWorldItem> requirements, BlueprintStrictness strictness)
				{
						for (int i = 0; i < row.Count; i++) {
								//always true in this case
								row[i].HasBlueprint = true;
								if (requirements[i] == null || requirements[i].IsEmpty) {
										row[i].DisableForBlueprint();
								} else {
										row[i].EnableForBlueprint(requirements[i], strictness);
										Blueprints.Get.IsCraftable(requirements[i], out row[i].RequirementBlueprint, false);//TODO change this to true eventually to cover revealed
								}
								row[i].RefreshRequest();
						}
				}

				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						//assume we're looking at a crafting object by this point
						targetObject.gameObject.SendMessage(Usage.SendMessageToTargetObject);
						return true;
				}

				public bool CheckRequirements(WIBlueprint blueprint, List <List <InventorySquareCrafting>> rows, InventorySquareCraftingResult resultSquare, ref int numCraftableItems)
				{
						numCraftableItems = Globals.NumItemsPerStack;
						if (CheckRequirementsRow(blueprint, rows[0], ref numCraftableItems)
						    && CheckRequirementsRow(blueprint, rows[1], ref numCraftableItems)
						    && CheckRequirementsRow(blueprint, rows[2], ref numCraftableItems)) {
								resultSquare.RequirementsMet = true;
						} else {
								numCraftableItems = 0;
								resultSquare.RequirementsMet = false;
						}
						resultSquare.NumItemsPossible = numCraftableItems;
						return resultSquare.RequirementsMet;
				}

				protected bool CheckRequirementsRow(WIBlueprint blueprint, List <InventorySquareCrafting> row, ref int numCraftableItems)
				{
						for (int i = 0; i < row.Count; i++) {
								InventorySquareCrafting square = row[i];
								if (square.EnabledForBlueprint) {
										if (square.AreRequirementsMet == false) {
												return false;
										} else if (square.NumCraftableItems < numCraftableItems) {
												numCraftableItems = square.NumCraftableItems;
										}
								}
						}
						return true;
				}

				protected bool ConsumeRequirementsRow(List <InventorySquareCrafting> row)
				{
						bool result = true;
						for (int i = 0; i < row.Count; i++) {
								InventorySquareCrafting square = row[i];
								if (square.EnabledForBlueprint && square.Stack.HasTopItem) {
										//one negative result means the whole thing is a bust
										result &= ConsumeRequirement(square.Stack, square.Stack.TopItem, square.RequiredItemTemplate);
								}
						}
						return result;
				}

				protected bool ConsumeRequirements(List <List <InventorySquareCrafting>> craftingRows)
				{
						return ConsumeRequirementsRow(craftingRows[0])
						&&	ConsumeRequirementsRow(craftingRows[1])
						&&	ConsumeRequirementsRow(craftingRows[2]);
				}

				public static bool ConsumeRequirement(WIStack itemStack, IWIBase item, GenericWorldItem template)
				{
						bool result = false;
						if (Stacks.Can.Stack(item, template)) {
								Stacks.Pop.AndToss(itemStack);
								result = true;
						} else if (item.Is <LiquidContainer>()) {
								//see if the thing inside the liquid container has what we need
								bool foundState = false;
								LiquidContainerState state = null;
								System.Object stateObject = null;
								if (item.GetStateOf <LiquidContainer>(out stateObject)) {
										state = (LiquidContainerState)stateObject;
										foundState = state != null;
								}

								if (foundState) {
										//hooray it is a liquid
										if (!state.IsEmpty) {
												//and it's not empty so steal one
												state.Contents.InstanceWeight--;
												result = true;
												item.SetStateOf <LiquidContainer>(state);
										}
								}
						}
						return result;
				}
				//this is used in a lot of places
				//eg in player inventory squares for crafting to check if the thing you've put in the square meets requirements
				public static bool AreRequirementsMet(IWIBase item, GenericWorldItem template, BlueprintStrictness strictness, int numItemsInStack, out int maxItemsToCraft)
				{
						maxItemsToCraft = 1;

						if (item != null && item.IsQuestItem) {
								return false;
						}

						bool prefabReqsMet = true;
						bool stackReqsMet = true;
						bool stateReqsMet = true;
						bool subCatReqsMet = true;

						//first we need to get the thing we're crafting from
						//if it's a liquid container we need to get the state data
						if (item.Is <LiquidContainer>()) {
								LiquidContainerState stateData = null;
								bool foundState = false;
								if (item.IsWorldItem) {
										stateData = item.worlditem.Get <LiquidContainer>().State;
										foundState = true;
								} else {
										if (item.GetStackItem(WIMode.None).GetStateData <LiquidContainerState>(out stateData)) {
												foundState = true;
										}
								}

								if (foundState) {
										//see if the liquid container contains what's needed
										mCheckRequirements.CopyFrom(stateData.Contents);
										mCheckRequirements.InstanceWeight = stateData.Contents.InstanceWeight;
								}
						} else {
								//if it's not a liquid container just get it from the item
								mCheckRequirements.CopyFrom(item);
								mCheckRequirements.InstanceWeight = numItemsInStack;
						}

						if (Flags.Check((uint)strictness, (uint)BlueprintStrictness.PrefabName, Flags.CheckType.MatchAny)) {
								prefabReqsMet = string.Equals(mCheckRequirements.PrefabName, template.PrefabName, StringComparison.InvariantCultureIgnoreCase);
						}
						if (Flags.Check((uint)strictness, (uint)BlueprintStrictness.StackName, Flags.CheckType.MatchAny)) {
								stackReqsMet = string.Equals(mCheckRequirements.StackName, template.StackName, StringComparison.InvariantCultureIgnoreCase);
						}
						if (Flags.Check((uint)strictness, (uint)BlueprintStrictness.StateName, Flags.CheckType.MatchAny)) {
								if ((string.IsNullOrEmpty(mCheckRequirements.State) || string.IsNullOrEmpty(template.State)) || (mCheckRequirements.State.Equals("Default") || template.State.Equals("Default"))) {
										stateReqsMet = true;
								} else {
										stateReqsMet = string.Equals(mCheckRequirements.State, template.State, StringComparison.InvariantCultureIgnoreCase);
								}
						}
						if (Flags.Check((uint)strictness, (uint)BlueprintStrictness.Subcategory, Flags.CheckType.MatchAny)) {
								if (string.IsNullOrEmpty(mCheckRequirements.Subcategory) || string.IsNullOrEmpty(template.Subcategory)) {
										subCatReqsMet = true;
								} else {
										subCatReqsMet = string.Equals(mCheckRequirements.Subcategory, template.Subcategory, StringComparison.InvariantCultureIgnoreCase);
								}
						}
						//Debug.Log (template.PrefabName + " prefab reqs met: " + prefabReqsMet.ToString () + "\nstackReqsMet: " + stackReqsMet.ToString () + "\nstateReqsMet: " + stateReqsMet.ToString () + "\nsubCatReqsMet: " + subCatReqsMet.ToString ());
						//max items to craft is the instance weight divided by instance weight of the template
						//instance weight of template tells us how many we need in that stack to craft
						maxItemsToCraft = mCheckRequirements.InstanceWeight / template.InstanceWeight;
						//if we have enough items to craft, we can proceed
						return prefabReqsMet && stackReqsMet && stateReqsMet && subCatReqsMet && maxItemsToCraft >= template.InstanceWeight;
				}

				protected static GenericWorldItem mCheckRequirements;
				// = new GenericWorldItem ();
				protected float mProgressValue = 0f;
		}

		[Serializable]
		public class CraftSkillExtensions
		{
				public string CraftOneDescription;
				public string CraftAllDescription;
		}
}
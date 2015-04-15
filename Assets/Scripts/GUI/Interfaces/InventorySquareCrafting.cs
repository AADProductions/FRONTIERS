using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.GUI
{
		public class InventorySquareCrafting : InventorySquare
		{
				public override bool IsEnabled {
						get {
								return HasStack;
						}
				}

				public bool AreRequirementsMet {
						get {
								mRequirementsMet = false;
								if (EnabledForBlueprint && HasStack && mStack.HasTopItem) {
										mRequirementsMet = CraftSkill.AreRequirementsMet(Stack.TopItem, RequiredItemTemplate, Strictness, Stack.NumItems, out mNumCraftableItems);
								}
								return mRequirementsMet;
						}
				}

				public Action BlueprintPotentiallyChanged;
				public bool HasBlueprint = false;
				public GenericWorldItem RequiredItemTemplate;
				public bool EnabledForBlueprint {
						get {
								return mEnabledForBlueprint;
						}
				}
				public BlueprintStrictness Strictness = BlueprintStrictness.Default;
				public string RequirementBlueprint;

				public void DisableForBlueprint ( ) {
						mEnabledForBlueprint = false;
				}

				public void EnableForBlueprint (GenericWorldItem requiredItemTemplate, BlueprintStrictness strictness) {
						mEnabledForBlueprint = true;
						Strictness = strictness;
						RequiredItemTemplate.CopyFrom(requiredItemTemplate);
				}

				public bool RequirementCanBeCrafted {
						get {
								return !string.IsNullOrEmpty(RequirementBlueprint);
						}
				}

				public int NumCraftableItems {
						get {
								if (EnabledForBlueprint && mRequirementsMet) {
										return mNumCraftableItems;
								}
								return 0;
						}
				}

				public override void OnClickSquare()
				{
						//keep the top item handy to see if we've changed
						IWIBase oldTopItem = null;
						IWIBase newTopItem = null;
						if (HasStack && Stack.HasTopItem) {
								oldTopItem = Stack.TopItem;
						}

						base.OnClickSquare();

						if (HasStack && Stack.HasTopItem) {
								newTopItem = Stack.TopItem;
						}

						if (newTopItem != oldTopItem) {
								if (!HasBlueprint || !EnabledForBlueprint) {
										//if we don't have a blueprint yet
										//or if we have one but aren't enabled yet
										//tell the crafting interface to look for a new blueprint
										BlueprintPotentiallyChanged.SafeInvoke();
								}
						}
				}

				public void OnSelectBlueprint (System.Object result)
				{
						UsingMenu = false;

						WIListResult dialogResult = result as WIListResult;
						switch (dialogResult.Result) {
								case "Craft":
										//we want to select a new blueprint
										WIBlueprint blueprint = null;
										if (Blueprints.Get.Blueprint (RequirementBlueprint, out blueprint)) {
												GUIInventoryInterface.Get.CraftingInterface.OnSelectBlueprint(blueprint);
										}
										break;

								default:
										break;
						}
				}

				protected override void OnRightClickSquare()
				{
						//right clicking a blueprint square opens a menu
						//where you can select the blueprint used to create the item
						//or you can bring the item in from your inventory
						if (EnabledForBlueprint) {
								bool canCraft = RequirementCanBeCrafted;
								bool canPlace = Player.Local.Inventory.FindFirstByKeyword(DopplegangerProps.PrefabName, out gCheckItem, out gCheckStack);
								SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
								optionsList.MessageType = string.Empty;//"Take " + mSkillUseTarget.DisplayName;
								if (canCraft && canPlace) {
										optionsList.Message = "Craft or Add Item";
								} else if (canCraft) {
										optionsList.Message = "Craft Item";
								} else {
										optionsList.Message = "Add Item";
								}
								optionsList.FunctionName = "OnSelectBlueprint";
								optionsList.RequireManualEnable = false;
								optionsList.OverrideBaseAvailabilty = true;
								optionsList.FunctionTarget = gameObject;
								if (gCraftOption == null) {
										gCraftOption = new WIListOption("Craft", "Craft");
										gPlaceOption = new WIListOption("Place", "Place");
										gCancelOption = new WIListOption("Cancel", "Cancel");
								}
								gCraftOption.OptionText = "Craft " + DopplegangerProps.DisplayName;
								gCraftOption.Disabled = !canCraft;
								gPlaceOption.OptionText = "Place " + DopplegangerProps.DisplayName;
								gPlaceOption.Divider = !canPlace;

								optionsList.AddOption(gCraftOption);
								optionsList.AddOption(gPlaceOption);
								optionsList.AddOption(gCancelOption);

								optionsList.ShowDoppleganger = false;
								GUIOptionListDialog dialog = null;
								if (optionsList.TryToSpawn(true, out dialog, NGUICamera)) {
										UsingMenu = true;
										optionsList.ScreenTarget = transform;
										optionsList.ScreenTargetCamera = NGUICamera;
								}
						}
				}

				public override void SetProperties()
				{
						DisplayMode = SquareDisplayMode.Disabled;
						ShowDoppleganger = false;
						MouseoverHover = false;
						DopplegangerMode = WIMode.Crafting;

						if (!EnabledForBlueprint || !HasBlueprint) {
								if (HasStack && mStack.HasTopItem) {
										MouseoverHover = true;
										ShowDoppleganger = true;
										DopplegangerMode = WIMode.Stacked;
										IWIBase topItem = mStack.TopItem;
										DopplegangerProps.CopyFrom(topItem);
										DisplayMode = SquareDisplayMode.Enabled;
										//?
								} else {
										DopplegangerProps.Clear();
										RequirementBlueprint = string.Empty;
								}
						} else {
								MouseoverHover = true;
								ShowDoppleganger = true;
								DopplegangerProps.Clear();
								if (HasStack && mStack.HasTopItem) {
										DopplegangerMode = WIMode.Stacked;
										IWIBase topItem = mStack.TopItem;
										DopplegangerProps.CopyFrom (topItem);
										if (AreRequirementsMet) {
												DisplayMode = SquareDisplayMode.Success;
										} else {
												DisplayMode = SquareDisplayMode.Error;
										}
								} else { 
										//we know this because we can't meet requirements without items
										mRequirementsMet = false;
										mNumCraftableItems = 0;
										DopplegangerProps.CopyFrom (RequiredItemTemplate);
										DisplayMode = SquareDisplayMode.Enabled;
								}
						}
				}

				public void SetRequiredItem(GenericWorldItem newRequirement)
				{
						if (newRequirement != RequiredItemTemplate) {
								RequiredItemTemplate = newRequirement;
								RefreshRequest();
						}
				}

				protected bool mEnabledForBlueprint = false;
				protected int mNumCraftableItems = 0;
				protected bool mRequirementsMet = false;
				protected IWIBase gCheckItem;
				protected WIStack gCheckStack;
				protected static WIListOption gCraftOption;
				protected static WIListOption gPlaceOption;
				protected static WIListOption gCancelOption;
		}
}
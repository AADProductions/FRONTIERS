using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
		public class InventorySquareCraftingResult : InventorySquareDisplay
		{
				public GenericWorldItem CraftedItemTemplate {
						get {
								return mCraftedItemTemplate;
						}
						set {
								if (NumItemsCrafted > 0 && mCraftedItemTemplate != value) {
										Debug.Log("Can't set crafted item template - we have to retrieve our other crafted items first");
										return;
								}
								mCraftedItemTemplate = value;
								UpdateDisplay();
						}
				}

				public bool RequirementsMet {
						get {
								return mRequirementsMet;
						}
						set {
								if (mRequirementsMet != value) {
										mRequirementsMet = value;
										RefreshRequest();
								}
						}
				}

				public int NumItemsPossible {
						get {
								return mNumItemsPossible;
						}
						set {
								if (mNumItemsPossible != value) {
										mNumItemsPossible = value;
										RefreshRequest();
								}
						}
				}

				public int NumItemsCrafted {
						get {
								return mNumItemsCrafted;
						}
						set {
								if (mNumItemsCrafted != value) {
										mNumItemsCrafted = value;
										RefreshRequest();
								}
						}
				}

				public Action RefreshAction;

				public bool HasItemTemplate {
						get {
								return CraftedItemTemplate != null && !CraftedItemTemplate.IsEmpty;
						}
				}

				public bool ReadyForRetrieval {
						get {
								return HasItemTemplate && NumItemsCrafted > 0;
						}
				}

				public void OnClickSquare()
				{
						Debug.Log("Clicking result square");
						WIStackError error = WIStackError.None;
						if (ReadyForRetrieval) {
								Debug.Log("Is ready for retrieval");
								while (NumItemsCrafted > 0) {
										StackItem craftedItem = CraftedItemTemplate.ToStackItem();
										craftedItem.Group = WIGroups.Get.Player;
										if (craftedItem.CanEnterInventory) {
												if (Player.Local.Inventory.AddItems(craftedItem, ref error)) {
														Debug.Log("Added item to inventory");
														NumItemsCrafted--;
												} else {
														Debug.Log("Couldn't add to inventory, what now?");
														break;
												}
										} else {
												Debug.Log("We have to carry the item");
												if (Player.Local.ItemPlacement.IsCarryingSomething) {
														Player.Local.ItemPlacement.PlaceOrDropCarriedItem();
												}
												//turn it into a worlditem and have the player carry it
												WorldItem craftedWorldItem = null;
												if (WorldItems.CloneFromStackItem(craftedItem, WIGroups.GetCurrent(), out craftedWorldItem)) {
														craftedWorldItem.Props.Local.CraftedByPlayer = true;
														craftedWorldItem.Initialize();
														craftedWorldItem.ActiveState = WIActiveState.Active;
														craftedWorldItem.Props.Local.FreezeOnStartup = false;
														craftedWorldItem.tr.rotation = Quaternion.identity;
														craftedWorldItem.SetMode(WIMode.World);
														craftedWorldItem.tr.position = Player.Local.ItemPlacement.GrabberIdealPosition;
														craftedWorldItem.LastActiveDistanceToPlayer = 0f;
														//if we have an interface open, close it now
														GUIInventoryInterface.Get.Minimize();
														//then force the player to carry the item
														if (Player.Local.ItemPlacement.ItemCarry(craftedWorldItem, true)) {
																NumItemsCrafted--;
														} else {
																GUIManager.PostWarning("You have to drop what you're carrying first");
														}
														//set placement mode to true immediately
														Player.Local.ItemPlacement.PlacementModeEnabled = true;
												}
												break;
										}
								}
						} else {
								Debug.Log("Not ready for retrieval");
						}

						RefreshRequest();
				}

				public override void UpdateDisplay()
				{
						InventoryItemName.text = string.Empty;
						DisplayMode = SquareDisplayMode.Disabled;
						string stackNumberLabelText = string.Empty;
						ShowDoppleganger = false;
						MouseoverHover = false;
						Collider.enabled = false;
						DopplegangerProps.CopyFrom(mCraftedItemTemplate);

						if (HasItemTemplate) {
								DisplayMode = SquareDisplayMode.Enabled;
								ShowDoppleganger = true;

								if (NumItemsCrafted > 0) {
										Collider.enabled = true;
										DisplayMode = SquareDisplayMode.Success;
										MouseoverHover = true;
										DopplegangerMode = WIMode.Stacked;
										stackNumberLabelText = NumItemsCrafted.ToString();
								} else {
										DopplegangerMode = WIMode.Stacked;
										if (NumItemsPossible > 0) {
												stackNumberLabelText = NumItemsPossible.ToString();
										}
								}
						}

						if (!string.IsNullOrEmpty(stackNumberLabelText)) {
								StackNumberLabel.enabled = true;
								StackNumberLabel.text = stackNumberLabelText;
						} else {
								StackNumberLabel.enabled = false;
						}
		
						base.UpdateDisplay();
				}

				protected override void OnRefresh()
				{
						base.OnRefresh();
						RefreshAction.SafeInvoke();
				}

				protected bool mRequirementsMet = false;
				protected GenericWorldItem mCraftedItemTemplate = null;
				protected int mNumItemsPossible = 1;
				protected int mNumItemsCrafted = 0;
		}
}
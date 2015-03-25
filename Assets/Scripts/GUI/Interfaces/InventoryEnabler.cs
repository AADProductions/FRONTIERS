using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class InventoryEnabler : InventorySquare
		{
				public override bool IsEnabled {
						get {
								return HasStack;
						}
				}

				public override void OnClickSquare()
				{					
						if (!IsEnabled) {
								return;
						}
			
						bool pickUp = false;
						bool playSound = false;
						bool showMenu = false;
						WIStackError error = WIStackError.None;

						if (InterfaceActionManager.LastMouseClick == 1) {
								showMenu = true;
						}

						if (showMenu) {
								if (mStack.HasTopItem) {
										WorldItem topItem = null;
										WorldItemUsable usable = null;
										if (!mStack.TopItem.IsWorldItem) {
												Stacks.Convert.TopItemToWorldItem(mStack, out topItem);
										} else {
												topItem = mStack.TopItem.worlditem;
										}
										if (mUsable != null) {
												mUsable.Finish();
												mUsable = null;
										}
										usable = topItem.gameObject.GetOrAdd <WorldItemUsable>();
										usable.ShowDoppleganger = false;
										usable.TryToSpawn(true, out mUsable, NGUICamera);
										usable.ScreenTarget = transform;
										usable.ScreenTargetCamera = NGUICamera;
										usable.RequirePlayerFocus = false;
										//the end result *should* affect the new item
										return;
								}
						}
			
						if (Player.Local.Inventory.SelectedStack.IsEmpty) {
								if (mStack.NumItems == 1) {
										playSound = Stacks.Add.Items(mStack, Player.Local.Inventory.SelectedStack, ref error);
										pickUp = true;
								} else {
										return;
								}
						} else if (Player.Local.Inventory.SelectedStack.NumItems == 1) {
								if (mStack.IsEmpty) {
										playSound = Stacks.Add.Items(Player.Local.Inventory.SelectedStack, mStack, ref error);
								} else {
										playSound = Stacks.Swap.Stacks(Player.Local.Inventory.SelectedStack, mStack, ref error);
								}
						}
			
						if (playSound) {
								if (pickUp) {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InventoryPickUpStack");
								} else {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InventoryPlaceStack");
								}
						} else {
								//we did nothing - show a help dialog
								GUIManager.PostIntrospection("This is a spot for containers. Containers let me carry things.", true);
						}
						Refresh();
				}

				public override void SetProperties()
				{
						DisplayMode = SquareDisplayMode.Empty;
						ShowDoppleganger = false;

						if (IsEnabled) {
								if (mStack.HasTopItem) {
										ShowDoppleganger = true;
										IWIBase topItem = mStack.TopItem;
										DopplegangerProps.PrefabName = topItem.PrefabName;
										DopplegangerProps.PackName = topItem.PackName;
										DopplegangerProps.State = topItem.State;
										if (topItem.IsStackContainer) {
												DisplayMode = SquareDisplayMode.Success;
										} else {
												DisplayMode = SquareDisplayMode.Error;
										}
								}
						} else {
								DisplayMode = SquareDisplayMode.Disabled;
						}
				}

				public override void SetInventoryStackNumber()
				{
						StackNumberLabel.enabled = false;
				}
		}
}
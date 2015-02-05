using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;
using System;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Container : WIScript
		{
				public string OpenText = "Open";
//changed by whatever uses it
				public bool CanOpen = true;
//whether it can be opened at all
				public bool CanUseToOpen = true;
//whether OnPlayerUse opens it automatically
				public override bool CanBeCarried {
						get {
								return State.Type != ContainerType.ShopGoods;
						}
				}

				public override bool CanEnterInventory {
						get {
								return State.Type != ContainerType.ShopGoods;
						}
				}

				public override bool UnloadWhenStacked {
						get {
								return false;
						}
				}

				public Action OnOpenContainer;

				public override void OnStartup()
				{
						if (!worlditem.IsStackContainer) {
								worlditem.StackContainer = Stacks.Create.StackContainer(worlditem, worlditem.Group);
						} else {
								worlditem.StackContainer.Owner = worlditem;
						}
				}

				public ContainerState State = new ContainerState();

				public override void OnInitialized()
				{
						worlditem.OnPlayerUse += OnPlayerUse;
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						if ((CanOpen && CanUseToOpen) && !worlditem.CanEnterInventory) {
								lastHudPriority++;
								GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Open", worlditem.HudTarget, GameManager.Get.GameCamera);
						}
						return lastHudPriority;
				}

				public void OnPlayerUse()
				{
						if (!CanOpen || !CanUseToOpen) {
								return;
						}
						//if we can't enter inventory
						//open the container
						OnOpenContainer.SafeInvoke();
						PrimaryInterface.MaximizeInterface("Inventory", "OpenStackContainer", worlditem.gameObject);
				}

				public override void PopulateOptionsList(System.Collections.Generic.List <WIListOption> options, List <string> message)
				{
						if (CanOpen) {
								options.Add(new WIListOption(OpenText, "Open"));
						}
				}

				public virtual void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						switch (dialogResult.SecondaryResult) {
								case "Open":
										OnOpenContainer.SafeInvoke();
										PrimaryInterface.MaximizeInterface("Inventory", "OpenStackContainer", worlditem.gameObject);
										if (State.ReputationChangeOnOpen != 0) {
												Profile.Get.CurrentGame.Character.Rep.ChangeGlobalReputation(State.ReputationChangeOnOpen);
										}
										break;

								default:
										break;
						}
				}

				public static GenericWorldItem DefaultContainerGenericWorldItem {
						get {
								if (gDefaultContainerGenericWorldItem == null) {
										gDefaultContainerGenericWorldItem = new GenericWorldItem();
										gDefaultContainerGenericWorldItem.PackName = "Containers";
										gDefaultContainerGenericWorldItem.PrefabName = "Sack 1";
								}
								return gDefaultContainerGenericWorldItem;
						}
				}

				protected static GenericWorldItem gDefaultContainerGenericWorldItem;
		}

		[Serializable]
		public class ContainerState
		{
				public ContainerType Type = ContainerType.PersonalEffects;
				public int ReputationChangeOnOpen = 0;
//for coffins mainly
		}
}
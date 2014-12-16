using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{		//this is for the 'container' tab in the player interface
		//it's used to manipulate containers
		public class GUIStackContainerInterface : GUIObject, IGUITabPageChild
		{
				//public UILabel CurrentPageNumberLabel;
				public GUIStackContainerDisplay	ContainerDisplay = null;
				public GameObject TakeContainerButton = null;
				public GameObject TakeAllButton = null;
				public WIStackEnabler Enabler = null;

				public bool HasEnabler {
						get {
								return Enabler != null;
						}
				}

				public void Start()
				{
						Enabler = Stacks.Create.StackEnabler(WIGroups.Get.Player);
						ContainerDisplay.EnablerDisplayPrefab = GUIManager.Get.InventorySquareEnablerDisplay;
						ContainerDisplay.UseVisualEnabler = true;
						ContainerDisplay.SetEnabler(Enabler);
				}

				public void OpenStackContainer(IWIBase containerItem)
				{
						StartCoroutine(OpenStackContainerOverTime(containerItem));
				}

				public void Show()
				{
						ContainerDisplay.Show();
						RefreshRequest();
				}

				public void Hide()
				{
						ClearContainer();
						ContainerDisplay.Hide();
				}

				public void OnClickTakeContainerButton()
				{
						/*
						WIStackError error = WIStackError.None;
						if (ContainerDisplay.ContainerToDisplay.HasOwner && ContainerDisplay.ContainerToDisplay.Owner.IsWorldItem)
						{
							if (Player.Local.Inventory.AddItems (ContainerDisplay.ContainerToDisplay.Owner.worlditem, WIGroups.Get.World, ref error))
							{
								ClearContainer ( );
							}			
						}
						*/
				}

				public void OnClickTakeAllButton()
				{
						WIStackError error = WIStackError.None;
						bool allItemsGone = true;
						foreach (WIStack stack in ContainerDisplay.Enabler.EnablerContainer.StackList) {
								if (stack.NumItems > 0 && !Player.Local.Inventory.AddItems(stack, ref error)) {
										allItemsGone = false;
								}
						}
						Refresh();
						/*
						if (allItemsGone) {
							TakeAllButton.SendMessage ("SetDisabled", SendMessageOptions.DontRequireReceiver);
						}
						*/
				}

				protected override void OnRefresh()
				{
						bool enableTakeAll = false;
						bool enableTakeContainer = false;
			
						if (ContainerDisplay.HasEnabler && Enabler.HasEnablerContainer) {
								WIStackContainer container = Enabler.EnablerContainer;
								IStackOwner owner = null;
								if (container.HasOwner(out owner) && owner.IsWorldItem) {
										//the owner will be the world item
										WorldItem containerWorldItem = container.Owner.worlditem;
										//now check to see if that world item belongs to a non-player group
										if (containerWorldItem.Group.HasOwner(out owner) && owner == Player.Local) {
												enableTakeAll = true;
												if (containerWorldItem.CanEnterInventory) {
														enableTakeContainer = true;
												}
										}
								}
						}
			
				}

				public void ClearContainer()
				{
						Stacks.Clear.Enabler(Enabler);
						RefreshRequest();
				}

				protected bool mOpeningContainer = false;

				protected IEnumerator OpenStackContainerOverTime(IWIBase containerItem)
				{
						mOpeningContainer = true;
						//wait a tick in case it's trying to fill itself with items
						yield return null;
						//clear our current enabler of items
						Stacks.Clear.Enabler(Enabler);
						Stacks.Display.ItemInEnabler(containerItem, Enabler);
						RefreshRequest();
						mOpeningContainer = false;
				}
		}
}
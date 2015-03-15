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
				public UIButtonMessage ShowHideButton;
				public UILabel ContainerName;
				public bool Visible = false;

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects)
				{
						ContainerDisplay.NGUICamera = NGUICamera;
						ContainerDisplay.GetActiveInterfaceObjects(currentObjects);
						FrontiersInterface.Widget w = new FrontiersInterface.Widget();
						w.SearchCamera = NGUICamera;

						if (TakeAllButton != null && TakeContainerButton != null) {
								w.BoxCollider = TakeContainerButton.GetComponent <BoxCollider>();
								currentObjects.Add(w);

								w.BoxCollider = TakeAllButton.GetComponent <BoxCollider>();
								currentObjects.Add(w);
						}

						w.BoxCollider = ShowHideButton.GetComponent <BoxCollider>();
						currentObjects.Add(w);
				}

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
						ContainerDisplay.Hide();

						ShowHideButton.target = gameObject;
						ShowHideButton.functionName = "OnClickShowHideButton";

						ContainerName.text = "(No container opened)";
				}

				public void OnClickShowHideButton ( )
				{
						Visible = false;
				}

				public void OpenStackContainer(IWIBase containerItem)
				{
						Visible = true;
						StartCoroutine(OpenStackContainerOverTime(containerItem));
				}

				public void Show()
				{
						Visible = true;
						ContainerDisplay.Show();
						RefreshRequest();
				}

				public void Hide()
				{
						Visible = false;
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
								ContainerName.text = "Contents of container:";
								WIStackContainer container = Enabler.EnablerContainer;
								IStackOwner owner = null;
								if (container.HasOwner(out owner) && owner.IsWorldItem) {
										//the owner will be the world item
										WorldItem containerWorldItem = container.Owner.worlditem;
										ContainerDisplay.EnablerDisplay.DopplegangerProps.CopyFrom(containerWorldItem);
										ContainerDisplay.RefreshRequest();
										//now check to see if that world item belongs to a non-player group
										ContainerName.text = "Contents of: " + containerWorldItem.DisplayName;
										if (containerWorldItem.Group.HasOwner(out owner) && owner == Player.Local) {
												enableTakeAll = true;
												if (containerWorldItem.CanEnterInventory) {
														enableTakeContainer = true;
												}
										}
								}
						} else {
								ContainerName.text = "(No container opened)";
						}
			
				}

				public void ClearContainer()
				{
						Stacks.Clear.Enabler(Enabler);
						Visible = false;
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
						yield return null;//wait for the request to hit
						//now see who owns this container
						if (containerItem.Group == WIGroups.Get.Player) {
								//turn OFF shift click
								for (int i = 0; i < ContainerDisplay.InventorySquares.Count; i++)
										ContainerDisplay.InventorySquares[i].AllowShiftClick = true;
						}
						else {
								//allow shift-click if it's not our container
								//it'll intercept 'remove item' requests
								for (int i = 0; i < ContainerDisplay.InventorySquares.Count; i++)
										ContainerDisplay.InventorySquares[i].AllowShiftClick = true;
						}
						mOpeningContainer = false;
				}
		}
}
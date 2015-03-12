using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
	public class GUIInventoryInterface : PrimaryInterface
	{
		public static GUIInventoryInterface Get;
		public GUIBank CurrencyInterface;
		public GUICraftingInterface CraftingInterface;
		public GUIPlayerClothingInterface ClothingInterface;
		public GUITabs InventoryTabs;
		//TODO get rid of this stupid variable
		public bool IsCrafting = false;
		public GUIStackContainerDisplay QuickslotsDisplay;
		public GUIStackDisplay QuickslotsCarryDisplay;
		public InventorySquare QuickslotsCarrySquare;
		public UIAnchor QuickslotsAnchor;
		public UIAnchor InventoryTabsAnchor;
		public UIPanel QuickslotsPanel;
		public GameObject QuickslotsParent;
		public List <GUIStackContainerDisplay> StackContainerDisplays = new List <GUIStackContainerDisplay>();
		public Vector3 QuickslotTarget = new Vector3(0f, 0f, 0f);
		public Vector2 QuickslotsAnchorHiddenOffset = new Vector2(0f, -1f);
		public Vector2 QuickslotsAnchorVisibleOffset = new Vector2(0f, 0f);
		public UISprite QuickslotHighlight;
		public UISprite EquippedIconRight;
		public UISprite EquippedIconLeft;
		public Transform QuickslotHighlightParent;
		public UILabel StackNumberLabel;
		public UILabel WeightLabel;
		public UILabel MouseoverItemLabel;
		public GameObject SelectedStackDisplay;
		public Transform SelectedStackDisplayTransform;
		public GameObject SelectedStackDoppleganger;
		public GameObject SelectedStackDisplayOffset;
		public Transform SelectedStackDisplayOffsetTransform;
		//container interface
		public GUIStackContainerInterface StackContainerInterface;
		public Transform StackContainerInterfaceTransform;
		public Vector3 StackContainerInterfaceTarget;
		public Vector3 StackContainerInterfaceHidden;
		public Vector3 StackContainerInterfaceVisible;
		//inventory containers
		public Transform ContainerDisplayParentTransform;
		public Vector3 ContainerDisplayTarget;
		public Vector3 ContainerDisplayHidden;
		public Vector3 ContainerDisplayContainerOpen;
		public Vector3 ContainerDisplayVisible;
		public static InventorySquareDisplay MouseOverSquare = null;
		public UIButton CloseButton;

		public void OnClickCloseButton()
		{
			ActionCancel(WorldClock.RealTime);
		}

		public override void GetActiveInterfaceObjects(List<Widget> currentObjects)
		{
			for (int i = 0; i < StackContainerDisplays.Count; i++) {
				StackContainerDisplays[i].GetActiveInterfaceObjects(currentObjects);
			}
			QuickslotsDisplay.GetActiveInterfaceObjects(currentObjects);
			InventoryTabs.GetActiveInterfaceObjects(currentObjects);
			ClothingInterface.GetActiveInterfaceObjects(currentObjects);
			CraftingInterface.GetActiveInterfaceObjects(currentObjects);

			if (StackContainerInterface.Visible) {
				StackContainerInterface.GetActiveInterfaceObjects(currentObjects);
			}

			Widget w = new Widget();
			w.SearchCamera = NGUICamera;

			w.Collider = QuickslotsCarrySquare.Collider;
			currentObjects.Add(w);

			w.Collider = CloseButton.GetComponent <BoxCollider>();
			currentObjects.Add(w);
		}

		public override bool Minimize()
		{
			if (base.Minimize()) {
				CloseButton.gameObject.SetActive(false);
				ContainerDisplayParentTransform.gameObject.SetActive(false);
				StackContainerInterfaceTransform.gameObject.SetActive(false);
				InventoryTabs.Hide();

				IsCrafting = false;
				//if the player is 'holding' anything put it back
				if (Player.Local != null) {
					if (!Player.Local.Inventory.PushSelectedStack()) {
						Player.Local.ItemPlacement.DropSelectedItems();
					}
				}
				ContainerDisplayTarget = ContainerDisplayHidden;
				StackContainerInterface.ClearContainer();
				CraftingInterface.ClearCrafting();
				if (QuickslotsDisplay != null) {
					QuickslotsDisplay.EnableColliders(false);
				}
				for (int i = 0; i < StackContainerDisplays.Count; i++) {
					StackContainerDisplays[i].Hide();
				}
				InventoryTabsAnchor.relativeOffset = Vector2.one;
				return true;
			}

			return false;
		}

		public override bool Maximize()
		{
			if (base.Maximize()) {	
				CloseButton.gameObject.SetActive(true);
				ContainerDisplayParentTransform.gameObject.SetActive(true);
				StackContainerInterfaceTransform.gameObject.SetActive(true);
				InventoryTabs.Show();

				CraftingInterface.ClearCrafting();
				//if we had a container open, clear it
				StackContainerInterface.ClearContainer();
				QuickslotsDisplay.EnableColliders(true);
				InventoryTabsAnchor.relativeOffset = Vector2.zero;
				GUIManager.Get.NGUIMessageDisplay.HideImmediately();
				for (int i = 0; i < StackContainerDisplays.Count; i++) {
					StackContainerDisplays[i].Show();
				}
				return true;
			}
			return false;
		}

		public override void WakeUp()
		{
			Get = this;
			ShowQuickslots = false;
			SelectedStackDisplayOffsetTransform = SelectedStackDisplayOffset.transform;
			SelectedStackDisplayTransform = SelectedStackDisplay.transform;
			ClothingInterface.NGUICamera = NGUICamera;
			CraftingInterface.NGUICamera = NGUICamera;

			Subscribe(InterfaceActionType.ToggleInventorySecondary, ToggleInventorySecondary);
		}

		public void Initialize()
		{	
			CreateContainerDisplays();
			CraftingInterface.Initialize(string.Empty);
			InventoryTabs.Initialize(this);
			ShowQuickslots = true;
			mInitialized = true;
		}

		public new void Update()
		{
			base.Update();

			if (!mInitialized)
				return;

			bool quickslotsVisible = true;

			if (GameManager.Is(FGameState.Cutscene)
			       || !Profile.Get.HasCurrentGame
			       || !PrimaryInterface.PrimaryShowQuickslots) {
				quickslotsVisible = false;
			}

			if (quickslotsVisible) {
				if (Player.Local.Inventory.QuickslotsEnabled) {
					//set the left and right hand visibility
					if (Player.Local.Inventory.HasActiveQuickslotItem) {
						EquippedIconRight.enabled = false;
					} else {
						EquippedIconRight.enabled = true;
					}
					QuickslotHighlight.enabled = true;
				} else {
					QuickslotHighlight.enabled = false;
				}

				if (Player.Local.Inventory.HasActiveCarryItem) {
					EquippedIconLeft.enabled = false;
				} else {
					EquippedIconLeft.enabled = true;
				}

				if (QuickslotHighlight.enabled) {
					if (Player.Local.ItemPlacement.PlacementModeEnabled) {
						QuickslotHighlight.color = Color.Lerp(Colors.Get.MessageSuccessColor, Colors.Darken(Colors.Get.MessageSuccessColor), Mathf.Sin(Time.time * 8));
					} else {
						QuickslotHighlight.color = Colors.Get.GeneralHighlightColor;
					}
					QuickslotHighlightParent.localPosition = Vector3.Lerp(QuickslotHighlightParent.localPosition, mQuickslotHighlightTarget, 0.75f);
				}
				if (QuickslotsAnchor.relativeOffset != QuickslotsAnchorVisibleOffset) {
					QuickslotsAnchor.relativeOffset = QuickslotsAnchorVisibleOffset;
				}
			} else {
				if (QuickslotsAnchor.relativeOffset != QuickslotsAnchorHiddenOffset) {
					QuickslotsAnchor.relativeOffset = QuickslotsAnchorHiddenOffset;
				}
			}

			if (Player.Local.Inventory.SelectedStack.HasTopItem) {
				SelectedStackDisplayTransform.position = NGUICamera.camera.ScreenToWorldPoint(Input.mousePosition);
				SelectedStackDoppleganger = WorldItems.GetDoppleganger(Player.Local.Inventory.SelectedStack.TopItem, SelectedStackDisplayOffsetTransform, SelectedStackDoppleganger, WIMode.Selected);
				StackNumberLabel.enabled = true;
			} else {
				if (SelectedStackDoppleganger != null) {
					GameObject.Destroy(SelectedStackDoppleganger);
					SelectedStackDoppleganger = null;
				}
				StackNumberLabel.enabled = false;
			}

			//set the container targets based on whether the container interface is open
			if (Maximized) {
				if (StackContainerInterface.Visible) {
					StackContainerInterfaceTarget = StackContainerInterfaceVisible;
					ContainerDisplayTarget = ContainerDisplayContainerOpen;
				} else {
					StackContainerInterfaceTarget = StackContainerInterfaceHidden;
					ContainerDisplayTarget = ContainerDisplayVisible;
				}
			} else {
				ContainerDisplayTarget = ContainerDisplayHidden;
				if (StackContainerInterface.Visible) {
					StackContainerInterfaceTarget = StackContainerInterfaceVisible;
				} else {
					StackContainerInterfaceTarget = StackContainerInterfaceHidden;
				}
			}

			if (StackContainerInterfaceTransform.localPosition != StackContainerInterfaceTarget) {
				StackContainerInterfaceTransform.localPosition = Vector3.Lerp(StackContainerInterfaceTransform.localPosition, StackContainerInterfaceTarget, 0.5f);
			}

			if (ContainerDisplayParentTransform.localPosition != ContainerDisplayTarget) {
				ContainerDisplayParentTransform.localPosition = Vector3.Lerp(ContainerDisplayParentTransform.localPosition, ContainerDisplayTarget, 0.5f);
			}


			if (Maximized) {
				if (Player.Local.ItemPlacement.PlacementModeEnabled) {
					MouseoverItemLabel.text = "Placement Mode (F)";
					MouseoverItemLabel.color = Colors.Get.MessageSuccessColor;
				} else {
					MouseoverItemLabel.color = Colors.Get.MenuButtonTextColorDefault;
					if (MouseOverSquare != null && MouseOverSquare.ShowDoppleganger) {
						MouseoverItemLabel.text = MouseOverSquare.DopplegangerProps.DisplayName;
					} else if (Player.Local.Inventory.HasActiveQuickslotItem) {
						MouseoverItemLabel.text = Player.Local.Inventory.ActiveQuickslotItem.DisplayName;
					} else {
						MouseoverItemLabel.text = string.Empty;
					}
				}
			} else {
				if (Player.Local.ItemPlacement.PlacementModeEnabled) { 
					MouseoverItemLabel.text = "Placement Mode (F)";
					MouseoverItemLabel.color = Colors.Get.MessageSuccessColor;
				} else if (Player.Local.Inventory.HasActiveQuickslotItem) {
					MouseoverItemLabel.text = Player.Local.Inventory.ActiveQuickslotItem.DisplayName;
					MouseoverItemLabel.color = Colors.Get.MenuButtonTextColorDefault;
				} else {
					MouseoverItemLabel.text = string.Empty;
				}
			}

			if (MouseOverSquare != null) {
				if (MouseOverSquare.CanSplitStack && InterfaceActionManager.Get.IsKeyDown(InterfaceActionType.StackSplit)) {
					GUICursor.Get.SetCursorTexture("StackSplit");
				} else {
					GUICursor.Get.SetCursorTexture("Default");
				}
			}
		}

		public void RefreshContainers()
		{
			QuickslotsDisplay.Refresh();
			foreach (GUIStackContainerDisplay container in StackContainerDisplays) {
				container.Refresh();
			}
		}

		public bool ToggleInventorySecondary(double timeStamp)
		{
			Minimize();
			return true;
		}

		public void OpenStackContainer(GameObject newEditObject)
		{
			WorldItem worlditem = newEditObject.GetComponent <WorldItem>();
			if (worlditem != null && worlditem.IsStackContainer) {
				OpenStackContainer(worlditem);
			}
		}

		public void OpenStackContainer(IWIBase newEditObject)
		{
			Maximize();
			StackContainerInterface.OpenStackContainer(newEditObject);
		}

		public void CloseStackContainer()
		{
			//		StackContainerInterface.Close ( );
		}

		public void SetActiveQuickslots(int activeQuickslot)
		{
			if (QuickslotsDisplay.HasEnabler && QuickslotsDisplay.Enabler.IsEnabled) {
				InventorySquare square = QuickslotsDisplay.InventorySquares[activeQuickslot];
				QuickslotHighlightParent.parent = square.transform;
				QuickslotHighlightParent.localPosition = Vector3.zero;
				QuickslotHighlightParent.parent = QuickslotsDisplay.transform;
				mQuickslotHighlightTarget = QuickslotHighlightParent.localPosition;
				mQuickslotHighlightTarget.z -= 50f;
			}
		}

		public void OnSelectedStackChanged(WIStack stack)
		{
			//		////Debug.Log ("Selected stack changed - inventory");
		}

		public void UpdateDisplay()
		{
			SetInventoryStackNumber();
			//		SetMouseoverItemLabelText (ActiveQuickslotStack);
		}

		public void SetInventoryStackNumber()
		{	//TODO move this to a square
			if (Player.Local.Inventory.SelectedStack.NumItems < 2) {
				StackNumberLabel.text = " ";
				StackNumberLabel.enabled = false;

			} else {
				StackNumberLabel.text = Player.Local.Inventory.SelectedStack.NumItems.ToString();
				StackNumberLabel.enabled = true;
			}
		}

		public void CraftBlueprint(string blueprintName)
		{
			if (IsCrafting) {
				return;
			}

			WIBlueprint blueprint = null;
			if (Blueprints.Get.Blueprint(blueprintName, out blueprint)) {
				CraftingInterface.OnSelectBlueprint(blueprint);
				InventoryTabs.Show();
				InventoryTabs.SetSelection("Crafting");
				//let the interface take care of whether we have a crafting item
			}
		}

		public void CraftingViaItem(GameObject craftingItem)
		{
			if (IsCrafting) {
				return;
			}

			InventoryTabs.Show();
			InventoryTabs.SetSelection("Crafting");
			CraftingInterface.SetCraftingItem(craftingItem.GetComponent <CraftingItem>());
		}

		public static void ClearMouseoverItemLabelText()
		{
			Get.MouseoverItemLabel.text = string.Empty;
		}

		public static void SetMouseoverItemLabelText(WIStack stack)
		{
			string finalText = string.Empty;
			if (stack != null && stack.HasTopItem) {
				string mouseOverLabelText = string.Empty;
				string sizeText = string.Empty;
				string stackNumberText = string.Empty;
				Color labelColor = Get.MouseoverItemLabel.color;

				mouseOverLabelText = stack.TopItem.DisplayName;
				sizeText = " (" + stack.TopItem.Size.ToString() + ") ";
				if (stack.NumItems > 1) {
					stackNumberText = stack.NumItems.ToString() + "/" + stack.MaxItems.ToString();
				}

				finalText = mouseOverLabelText + Colors.ColorWrap(sizeText + stackNumberText, Colors.Darken(labelColor));
			}
			Get.MouseoverItemLabel.text = finalText;
		}

		protected void CreateContainerDisplays()
		{
			if (QuickslotsDisplay == null) {	//don't build stuff twice!
				GameObject instantiatedContainerDisplay = null;
				GameObject instantiatedCarryDisplay = null;
				GUIStackContainerDisplay containerDisplay = null;
				InventorySquare carrySquare = null;

				for (int i = 0; i < Globals.NumInventoryStackContainers; i++) {
					instantiatedContainerDisplay = NGUITools.AddChild(ContainerDisplayParentTransform.gameObject, GUIManager.Get.StackContainerDisplay);
					containerDisplay = instantiatedContainerDisplay.GetComponent <GUIStackContainerDisplay>();
					containerDisplay.NGUICamera = NGUICamera;
					containerDisplay.transform.localPosition = new Vector3(0f, i * -containerDisplay.FrameHeight, 0f);
					StackContainerDisplays.Add(containerDisplay);
					containerDisplay.Refresh();
				}

				instantiatedContainerDisplay = NGUITools.AddChild(QuickslotsParent, GUIManager.Get.StackContainerDisplay);
				containerDisplay = instantiatedContainerDisplay.GetComponent <GUIStackContainerDisplay>();
				containerDisplay.NGUICamera = NGUICamera;
				containerDisplay.transform.localPosition = Vector3.zero;
				QuickslotsDisplay = containerDisplay;
				containerDisplay.Refresh();

				instantiatedCarryDisplay = NGUITools.AddChild(QuickslotsParent, GUIManager.Get.InventorySquare);
				carrySquare = instantiatedCarryDisplay.GetComponent <InventorySquare>();
				carrySquare.NGUICamera = NGUICamera;
				carrySquare.RequiresEnabler = false;
				//get the last position of the last square in the stack
				//then add 1.5 times a square's width to that so it feels 'left hand' ish
				carrySquare.transform.localPosition = new Vector3(-((carrySquare.Dimensions.x * (Globals.MaxStacksPerContainer + 2)) + (carrySquare.Dimensions.x / 2)), -60f, 0f);

				QuickslotsCarrySquare = carrySquare;
				//add the little hand icons
				EquippedIconLeft.transform.parent = QuickslotsCarrySquare.transform;
				EquippedIconLeft.transform.localPosition = Vector3.zero;
				EquippedIconLeft.color = Colors.Get.GeneralHighlightColor;
				EquippedIconRight.color = Colors.Get.GeneralHighlightColor;
				EquippedIconLeft.alpha = QuickslotHighlight.alpha;
				EquippedIconRight.alpha = QuickslotHighlight.alpha;

				QuickslotHighlight.transform.localScale = new Vector3(carrySquare.Dimensions.x, carrySquare.Dimensions.y, 1f);
			}
		}

		protected bool mRefreshNextFrame = false;
		protected bool mInitialized = false;
		protected Vector3 mQuickslotHighlightTarget = Vector3.zero;
	}
}
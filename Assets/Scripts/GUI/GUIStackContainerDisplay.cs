using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.GUI
{
		public class GUIStackContainerDisplay : GUIObject, IGUITabPageChild
		{
				//this is where the state of the stacks is kept
				//this is NEVER owned or created by this object or any GUI object
				//this is ALWAYS owned by the player or by world item
				public WIStackEnabler Enabler {
						get {
								return mEnabler;
						}
				}
				//the rest is pure GUI
				public virtual bool HasEnabler {
						get {
								return mEnabler != null;
						}
				}

				public override void Awake()
				{
						base.Awake();
						if (MainPanel == null) {
								MainPanel = gameObject.GetComponent <UIPanel>();
						}
				}

				public UIPanel MainPanel;
				public InventorySquare EnablerDisplay;
				public GameObject EnablerDisplayPrefab;
				public GameObject SquarePrefab;
				public Vector3 EnablerOffset = new Vector3(-65f, -60f, 0f);
				public UISprite FrameSprite;
				public StackContainerDisplayMode DisplayMode = StackContainerDisplayMode.OneRow;
				public GameObject InventorySlotsParent;
				public float FrameHeight = 117.5f;

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects)
				{
						FrontiersInterface.Widget w = new FrontiersInterface.Widget();
						for (int i = 0; i < InventorySquares.Count; i++) {
								w.Collider = InventorySquares[i].Collider;
								w.SearchCamera = NGUICamera;
								currentObjects.Add(w);
						}
						w.Collider = EnablerDisplay.Collider;
						w.SearchCamera = NGUICamera;
						currentObjects.Add(w);
				}

				public void Show()
				{
						if (!mInitialized)
								return;

						MainPanel.enabled = true;
						for (int i = 0; i < InventorySquares.Count; i++) {
								InventorySquares[i].enabled = true;
						}
				}

				public void Hide()
				{
						if (!mInitialized)
								return;

						MainPanel.enabled = false;
						for (int i = 0; i < InventorySquares.Count; i++) {
								InventorySquares[i].enabled = false;
						}
				}

				public void EnableColliders(bool enable)
				{
						for (int i = 0; i < InventorySquares.Count; i++) {
								InventorySquares[i].collider.enabled = enable;
						}
				}

				public virtual string DisplayName {
						get {
								string displayName = string.Empty;
								if (HasEnabler) {	//if we have an enabler and the enabler enables stacks
										//then we're good to go!
										displayName = Enabler.DisplayName;
								} else {	//just in case we don't have an enabler to display
										displayName = "(Empty)";
								}
								return displayName;
						}
				}

				public List <InventorySquare> InventorySquares = new List <InventorySquare>();

				public bool HasCreatedSquares {
						get {
								return InventorySquares.Count > 0;
						}
				}

				public bool UseVisualEnabler = true;

				public virtual void SetEnabler(WIStackEnabler enabler)
				{
						if (HasEnabler) {
								if (enabler != null) {
										if (mEnabler != enabler) {	//if it's not the same
												//and our current enabler uses our refresh action
												//clear the refresh action for the current enabler
												mEnabler.RefreshAction -= mRefreshRequest;
										}
										mEnabler = enabler;
										mEnabler.Display = this;
										mEnabler.RefreshAction += mRefreshRequest;
								} else {//clear the enabler's refresh action
										//set our enabler to null and we're done
										mEnabler.RefreshAction -= mRefreshRequest;
										mEnabler = null;
										if (UseVisualEnabler) {
												EnablerDisplay.SetStack(null);
										}
								}
						}
			//if we don't have an enabler and the new one isn't null
			else if (enabler != null) {
								mEnabler = enabler;
								mEnabler.RefreshAction += mRefreshRequest;
						}

						if (UseVisualEnabler && EnablerDisplay != null && mEnabler != null) {
								EnablerDisplay.SetStack(mEnabler.EnablerStack);
						}
						RefreshRequest();
				}

				public virtual void DropEnabler()
				{
						if (mEnabler != null) {
								mEnabler.RefreshAction -= mRefreshRequest;
						}
						for (int i = 0; i < InventorySquares.Count; i++) {
								if (InventorySquares[i] != null) {
										InventorySquares[i].DropStack();
								}
						}
				}

				public virtual void CreateSquares()
				{
						if (HasCreatedSquares)
								return;

						if (EnablerDisplayPrefab == null) {
								EnablerDisplayPrefab = GUIManager.Get.InventorySquareEnabler;
						}

						if (UseVisualEnabler) {
								if (EnablerDisplay == null) {	//create the square that will display the object
										GameObject enablerGameObject = NGUITools.AddChild(gameObject, EnablerDisplayPrefab);
										EnablerDisplay = enablerGameObject.GetComponent <InventoryEnabler>();
										if (EnablerDisplay == null) {
												Debug.Log("Couldn't get component inventory enabler from enabler display prefab " + EnablerDisplayPrefab.name + " in " + name);
										}
										//DO NOT ever set its stack here or create its stack for it
										//always let RefreshSquares set its stack
								}
								EnablerDisplay.transform.localPosition = EnablerOffset;
						}

						switch (DisplayMode) {
								case StackContainerDisplayMode.OneRow:
										CreateOneRowSquares();
										break;

								case StackContainerDisplayMode.TwoRow:
										CreateTwoRowSquares();
										break;

								case StackContainerDisplayMode.Circle:
										break;

								case StackContainerDisplayMode.TwoRowVertical:
										CreateTwoRowVerticalSquares();
										break;

								default:
										break;
						}
				}

				protected void CreateTwoRowSquares()
				{
						Vector2 squareDimensions = Vector2.zero;
						//int numberOfSquares = Globals.MaxStacksPerContainer;
						int squaresPerRow = Globals.MaxStacksPerContainer / 2;
						if (SquarePrefab == null) {
								SquarePrefab = GUIManager.Get.InventorySquare;
						}
						int currentRow = 0;
						int currentSquare = 0;

						for (int y = 0; y < 2; y++) {
								for (int x = 0; x < squaresPerRow; x++) {
										GameObject instantiatedSquare = NGUITools.AddChild(InventorySlotsParent, SquarePrefab);
										InventorySquare square = instantiatedSquare.GetComponent <InventorySquare>();
										square.NGUICamera = NGUICamera;
										squareDimensions = square.Dimensions;
										square.transform.localPosition	= new Vector3(x * (square.Dimensions.x), currentRow * (-square.Dimensions.y), 0f);
										square.Enabler = Enabler;
										square.Index = currentSquare;
										InventorySquares.Add(square);
										square.UpdateDisplay();
										currentSquare++;
								}
								currentRow++;
						}
						FrameSprite.transform.localScale = new Vector3((squaresPerRow * squareDimensions.x) - mFramePadding, (2 * squareDimensions.y) - mFramePadding, 0f);
						if (UseVisualEnabler) {
								EnablerDisplay.name = DisplayName + " enabler ";
						}
				}

				protected void CreateTwoRowVerticalSquares()
				{
						Vector2 squareDimensions = Vector2.zero;
						int squaresPerColumn = Globals.MaxStacksPerContainer / 2;
						if (SquarePrefab == null) {
								SquarePrefab = GUIManager.Get.InventorySquare;
						}
						int currentRow = 0;
						int currentSquare = 0;

						for (int y = 0; y < squaresPerColumn; y++) {
								for (int x = 0; x < 2; x++) {
										GameObject instantiatedSquare = NGUITools.AddChild(InventorySlotsParent, SquarePrefab);
										InventorySquare square = instantiatedSquare.GetComponent <InventorySquare>();
										square.NGUICamera = NGUICamera;
										squareDimensions = square.Dimensions;
										square.transform.localPosition	= new Vector3(x * (square.Dimensions.x), currentRow * (-square.Dimensions.y), 0f);
										square.Enabler = Enabler;
										square.Index = currentSquare;
										InventorySquares.Add(square);
										square.UpdateDisplay();
										currentSquare++;
								}
								currentRow++;
						}
						FrameSprite.transform.localScale = new Vector3((2 * squareDimensions.x) - mFramePadding, (2 * squareDimensions.y) - mFramePadding, 0f);
						if (UseVisualEnabler) {
								EnablerDisplay.name = DisplayName + " enabler ";
						}
				}

				protected void CreateOneRowSquares()
				{
						Vector2 squareDimensions = Vector2.zero;
						int numberOfSquares = Globals.MaxStacksPerContainer;
						int currentSquare = 0;
						if (SquarePrefab == null) {
								SquarePrefab = GUIManager.Get.InventorySquare;
						}

						for (int x = 0; x < numberOfSquares; x++) {
								GameObject instantiatedSquare = NGUITools.AddChild(InventorySlotsParent, SquarePrefab);
								InventorySquare square = instantiatedSquare.GetComponent <InventorySquare>();
								square.NGUICamera = NGUICamera;
								squareDimensions = square.Dimensions;
								square.transform.localPosition	= new Vector3(((numberOfSquares - 1) - x) * (-square.Dimensions.x), 0f, 0f);
								square.Enabler = Enabler;
								square.Index = currentSquare;
								InventorySquares.Add(square);
								square.UpdateDisplay();
								currentSquare++;
						}

						FrameSprite.transform.localScale = new Vector3((numberOfSquares * squareDimensions.x) + mFramePadding, (squareDimensions.y) + mFramePadding, 0f);
						if (UseVisualEnabler) {
								EnablerDisplay.name = DisplayName + " enabler ";
						}
				}

				protected void CreateCircleSquares()
				{
						//TODO - yes I said circle squares and no that's not a typo
				}

				protected override void OnRefresh()
				{
						//create squares if we haven't already
						CreateSquares();
						//if the enabler is enabled that means it has a container and that the container has stacks
						//the enabler display only needs to drop its stack if the enabler has no item
						//otherwise it will just display the item as being incorrect
						bool setEnablerDisplayStack = (HasEnabler && UseVisualEnabler);
						bool setSquareStacks = (HasEnabler && Enabler.IsEnabled);

						if (setEnablerDisplayStack) {	//top item in the enabler? set the stack
								EnablerDisplay.SetStack(Enabler.EnablerStack);
								EnablerDisplay.UpdateDisplay();
						} else if (UseVisualEnabler) {	//no top item? drop the stack
								EnablerDisplay.DropStack();
								EnablerDisplay.UpdateDisplay();
						}

						try {
								if (setSquareStacks) {
										//IN THEORY this should never result in an out of bounds error
										//because setSquareStacks can't be true unless it has the stacks
										//but we'll see what happens...
										List <WIStack> enablerStacks = Enabler.EnablerStacks;
										for (int i = 0; i < InventorySquares.Count; i++) {
												InventorySquares[i].name = DisplayName + " square " + i.ToString();
												InventorySquares[i].Enabler = Enabler;
												InventorySquares[i].SetStack(enablerStacks[i]);
												InventorySquares[i].UpdateDisplay();
										}
								} else {
										//if we're not setting them, we're dropping them
										for (int i = 0; i < InventorySquares.Count; i++) {
												InventorySquares[i].name = DisplayName + " square " + i.ToString();
												InventorySquares[i].DropStack();
												InventorySquares[i].UpdateDisplay();
										}
								}
						} catch (Exception e) {
								Debug.LogError("Error when setting stacks in stack container display, proceeding normally: " + e.ToString());
						}
				}

				public override void OnDestroy()
				{
						DropEnabler();
						base.OnDestroy();
				}

				protected WIStackEnabler mEnabler = null;
				public float mFramePadding = 17.5f;
		}

		public enum StackContainerDisplayMode
		{
				OneRow,
				Circle,
				TwoRow,
				TwoRowVertical,
		}
}
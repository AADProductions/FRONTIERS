using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.GUI;
using Frontiers.Data;
using Frontiers.World.BaseWIScripts;

namespace Frontiers
{
		public class PlayerInventory : PlayerScript, IInventory
		{
				public PlayerInventoryState State = new PlayerInventoryState();
				public List <IWIBase> LastAddedItems = new List <IWIBase>();
				public double LastAddedTime = Mathf.NegativeInfinity;
				public WIStack SelectedStack = null;
				public WIStackEnabler QuickslotEnabler = null;
				public List <WIStackEnabler> InventoryEnablers = new List <WIStackEnabler>();
				public List <string> NoSpaceNeededItems = new List <string> { "BookAvatar", "Currency", "Purse", "Key" };

				public override void Initialize()
				{
						base.Initialize();
						enabled = true;
				}

				public override bool LockQuickslots {
						get {
								return mChangingAqi;
						}
				}

				public WorldItem ActiveQuickslotItem {
						get {
								return mActiveQuickslotItem;
						}
				}

				public WorldItem ActiveCarryItem {
						get {
								return mActiveCarryItem;
						}
				}

				#region has this, has that

				public bool HasActiveQuickslotItem {
						get {//this will cover our asses a bit if we don't
								//get around to updating the quickslot status enough
								return 	!(mActiveQuickslotItem == null || !mActiveQuickslotItem.Is(WILoadState.Initialized));
						}
				}

				public bool HasActiveCarryItem { 
						get { 
								return !(mActiveCarryItem == null || !mActiveCarryItem.Is(WILoadState.Initialized));
						}
				}

				public bool HasItem(IWIBase item, out WIStack stack)
				{
						stack = null;
						return false;
				}

				public void AcquireStructure(MobileReference ownedStructure, bool announceOwnership)
				{
						if (State.OwnedStructures.SafeAdd(ownedStructure)) {
								//if we don't already own it
								//announce that we gained a thing
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.LocationAquire, WorldClock.AdjustedRealTime);
								if (announceOwnership) {
										GUIManager.PostGainedItem(ownedStructure);
								}
						}
				}

				public bool HasKey(string keyType, string keyTag)
				{
						for (int i = 0; i < State.PlayerKeyChain.Keys.Count; i++) {
								KeyState keyState = State.PlayerKeyChain.Keys[i];
								if (keyState.KeyType == keyType && keyState.KeyTag == keyTag) {
										return true;
								}
						}
						return false;
				}

				#endregion

				//used for searching - use sparingly
				public Dictionary <int,IWIBase>	QuickslotItems {
						get {
								Dictionary <int,IWIBase> quickslotItems = new Dictionary <int, IWIBase>();
								if (QuickslotsEnabled) {
										List <WIStack> stacks = QuickslotEnabler.EnablerStacks;
										for (int i = 0; i < stacks.Count; i++) {
												if (i == State.ActiveQuickslot && HasActiveQuickslotItem) {	//if it's the active quickslot then the item will be equipped as a tool
														//so get the tool's worlditem instead of the stack's
														quickslotItems.Add(i, ActiveQuickslotItem);
												} else if (stacks[i].HasTopItem) {
														quickslotItems.Add(i, stacks[i].TopItem);
												}
										}
								}
								return quickslotItems;
						}
				}

				public bool QuickslotsEnabled {
						get {	//we're initialized, updated, we have an enabler
								//and that enabler has a compatible stack container with stacks
								return State.ActiveQuickslot >= 0
								&& QuickslotEnabler != null
								&& QuickslotEnabler.IsEnabled;
						}
				}

				#region initialization

				public override void OnStateLoaded()
				{
						Stacks.Convert.UnloadedItemsToWorldItems(State.QuickslotsStack);
						for (int i = 0; i < State.InventoryStacks.Count; i++) {
								Stacks.Convert.UnloadedItemsToWorldItems(State.InventoryStacks[i]);
						}
				}

				public void FillInventory(string inventoryFillCategory)
				{
						StartCoroutine(FillInventoryOverTime(inventoryFillCategory));
				}

				protected IEnumerator FillInventoryOverTime(string inventoryFillCategory)
				{
						yield return null;
						WICategory startupCategory = null;
						WIStackError error = WIStackError.None;
						if (WorldItems.Get.Category(inventoryFillCategory, out startupCategory)) {
								for (int i = 0; i < startupCategory.GenericWorldItems.Count; i++) {
										for (int j = 0; j < startupCategory.GenericWorldItems[i].InstanceWeight; j++) {
												WorldItem worlditem = null;
												if (WorldItems.CloneWorldItem(startupCategory.GenericWorldItems[i], STransform.zero, false, WIGroups.Get.Player, out worlditem)) {
														if (!AddItems(worlditem, ref error)) {
																Debug.Log("PLAYERINVENTORY: Couldn't add item, error: " + error.ToString());
														}
												}
										}
										yield return null;
								}
						}
				}

				public override void OnGameLoadStart()
				{
						if (State.InventoryStacks == null || State.InventoryStacks.Count != Globals.NumInventoryStackContainers) {	//if it's null after setting state then we haven't created it before
								State.QuickslotsStack = Stacks.Create.Stack(WIGroups.Get.Player);
								State.QuickslotsStackCarry = Stacks.Create.Stack(WIGroups.Get.Player);
								State.InventoryStacks = Stacks.Create.Stacks(Globals.NumInventoryStackContainers, WIGroups.Get.Player);
						} else {
								State.QuickslotsStack.Group = WIGroups.Get.Player;
								State.QuickslotsStackCarry.Group = WIGroups.Get.Player;
								for (int i = 0; i < State.InventoryStacks.Count; i++) {
										State.InventoryStacks[i].Group = WIGroups.Get.Player;
								}
						}
						for (int i = 0; i < State.InventoryStacks.Count; i++) {
								State.InventoryStacks[i].Bank = State.PlayerBank;
						}
						State.QuickslotsStackCarry.Bank = State.PlayerBank;
						State.QuickslotsStack.Bank = State.PlayerBank;
						//always create enablers
						QuickslotEnabler = Stacks.Create.StackEnabler(State.QuickslotsStack, WIGroups.Get.Player);
						InventoryEnablers = Stacks.Create.StackEnablers(State.InventoryStacks, WIGroups.Get.Player);

						SelectedStack = Stacks.Create.Stack(WIGroups.Get.Player);
						SelectedStack.StackMaxSize = WISize.NoLimit;
						SelectedStack.Bank = State.PlayerBank;

						//make sure the curreny interface is using our actual inventory bank
						GUIInventoryInterface.Get.Initialize();

						GUIStackContainerDisplay display = GUIInventoryInterface.Get.QuickslotsDisplay;
						display.name = "Quickslots";
						display.SetEnabler(QuickslotEnabler);

						GUIInventoryInterface.Get.QuickslotsCarrySquare.SetStack(State.QuickslotsStackCarry);

						for (int i = 0; i < InventoryEnablers.Count; i++) {
								display = GUIInventoryInterface.Get.StackContainerDisplays[i];
								display.name = "Inventory enabler " + i.ToString();
								display.SetEnabler(InventoryEnablers[i]);
						}

						State.PlayerBank.RefreshAction += OnBankChange;
						GUIInventoryInterface.Get.CurrencyInterface.SetBank(State.PlayerBank);
				}

				public override void OnLocalPlayerDespawn()
				{
						mLastRelevantStack = null;
				}

				public override void OnGameStartFirstTime()
				{
						ClearInventory(false);//this will add a container
				}

				public override void OnGameStart()
				{
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.SelectionNext, new ActionListener(SelectionNext));
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.SelectionPrev, new ActionListener(SelectionPrev));
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.SelectionNumeric, new ActionListener(SelectionNumeric));
						GUIInventoryInterface.Get.UserActions.Subscribe(UserActionType.ToolSwap, new ActionListener(ToolSwap));

						StartCoroutine(CheckQuickslotStatus());
						StartCoroutine(CheckCarryItemStatus());
						if (State.ActiveQuickslot < 0) {
								SelectionNext(WorldClock.AdjustedRealTime);
						}
				}

				#endregion

				public bool PopAQIIntoWorld(out WorldItem aqi, WIGroup toGroup, ref WIStackError error)
				{
						if (HasActiveQuickslotItem) {
								IWIBase aqiBase = null;
								player.Tool.UnlockWorldItem();//in case the tool has locked it
								if (Stacks.Pop.Top(QuickslotEnabler.EnablerStacks[State.ActiveQuickslot], out aqiBase, toGroup, ref error)) {
										aqi = aqiBase.worlditem;
										aqi.SetMode(WIMode.World);
										return true;
								}
						}
						aqi = null;
						return false;
				}

				public void ClearInventory(bool destroyItems)
				{
						Debug.Log("Clearing inventory in player inventory state");
						for (int i = 0; i < InventoryEnablers.Count; i++) {
								Stacks.Clear.Items(InventoryEnablers[i], destroyItems);
						}
						Stacks.Clear.Items(QuickslotEnabler, destroyItems);
						Stacks.Clear.Items(State.QuickslotsStackCarry, destroyItems);
						mActiveQuickslotItem = null;
						mActiveCarryItem = null;
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemAQIChange, WorldClock.AdjustedRealTime);
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemACIChange, WorldClock.AdjustedRealTime);
						//always add a single sack, even if we've cleared everything else
						WorldItem containerItem = null;
						WIStackError error = WIStackError.None;
						mLastRelevantStack = null;
						if (WorldItems.CloneWorldItem("Containers", "Sack 1", STransform.zero, false, WIGroups.Get.Player, out containerItem)) {
								Debug.Log("adding sack to inventory");
								AddItems(containerItem, ref error);
						}
				}

				public void AddQuestItem(string questItem)
				{
						if (State.QuestItemsAcquired.SafeAdd(questItem)) {
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemQuestItemAddToInventory, WorldClock.AdjustedRealTime);
						}
				}

				#region user / interface actions / actions in general

				public bool SelectionNumeric(double timeStamp)
				{
						if (player.LockQuickslots)
								return true;

						if (!GameManager.Is(FGameState.InGame)) {
								return true;
						}
						int selection = -1;
						//Debug.Log("Last key pressed: " + InterfaceActionManager.LastKey.ToString());

						switch (InterfaceActionManager.LastKey) {
								case KeyCode.None:
								default:
										return true;

								case KeyCode.Alpha1:
										selection = 0;
										break;

								case KeyCode.Alpha2:
										selection = 1;
										break;

								case KeyCode.Alpha3:
										selection = 2;
										break;

								case KeyCode.Alpha4:
										selection = 3;
										break;

								case KeyCode.Alpha5:
										selection = 4;
										break;

								case KeyCode.Alpha6:
										selection = 5;
										break;

								case KeyCode.Alpha7:
										selection = 6;
										break;

								case KeyCode.Alpha8:
										selection = 7;
										break;

								case KeyCode.Alpha9:
										selection = 8;
										break;

								case KeyCode.Alpha0:
										selection = 9;
										break;
						}

						if (selection >= 0) {
								State.ActiveQuickslot = selection;
								if (State.ActiveQuickslot >= Globals.MaxStacksPerContainer) {	//quickslots will always cap at MaxStacksPerContainer
										State.ActiveQuickslot = 0;
								} else if (State.ActiveQuickslot < 0) {
										State.ActiveQuickslot = 0;
								}
								GUIInventoryInterface.Get.SetActiveQuickslots(State.ActiveQuickslot);
						}
						return true;
				}

				public bool SelectionNext(double timeStamp)
				{
						if (player.LockQuickslots)
								return true;

						if (GameManager.Is(FGameState.InGame)) {
								State.ActiveQuickslot++;
								if (State.ActiveQuickslot >= Globals.MaxStacksPerContainer) {	//quickslots will always cap at MaxStacksPerContainer
										State.ActiveQuickslot = 0;
								}
								GUIInventoryInterface.Get.SetActiveQuickslots(State.ActiveQuickslot);
						}
						return true;
				}

				public bool SelectionPrev(double timeStamp)
				{
						if (player.LockQuickslots)
								return true;

						if (GameManager.Is(FGameState.InGame)) {
								State.ActiveQuickslot--;
								if (State.ActiveQuickslot < 0) {	//quickslots will always cap at MaxStacksPerContainer
										State.ActiveQuickslot = Globals.MaxStacksPerContainer - 1;
								}
								GUIInventoryInterface.Get.SetActiveQuickslots(State.ActiveQuickslot);
						}
						return true;
				}

				public bool ToolSwap(double timeStamp)
				{
						if (player.LockQuickslots)
								return true;

						//swap the carry item with the active quickslot item
						WIStackError error = WIStackError.None;
						Stacks.Swap.Stacks(State.QuickslotsStack, State.QuickslotsStackCarry, ref error);
						return true;
				}

				public void OnBankChange()
				{
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemCurrencyExchange, WorldClock.AdjustedRealTime);
				}

				#endregion

				#region adding items

				//the main add item function
				//this is where all other 'add' functions eventually filter down to
				public bool AddItems(IWIBase item, ref WIStackError error)
				{
						if (item == null) {
								error = WIStackError.InvalidOperation;
								Debug.Log("Was null");
								return false;
						}

						if (!mInitialized) {
								mItemsToAddOnInitialization.Add(item);
								return true;
						}

						if (player.LockQuickslots) {
								//we'll add this once our quickslots aren't locked any more
								mItemsToAddOnQuickslotsUnlocked.SafeEnqueue(item);
								return true;
						}

						if (!item.Is <Stackable>()) {
								Debug.Log("Not stackable");
								//can't add non-stackable items, dummy
								return false;
						}

						//-----SPECIAL CASES-----//
						if (item.Is <Purse>()) {
								PurseState purseState = null;
								if (item.IsWorldItem) {	
										Purse purse = item.worlditem.Get <Purse>();
										purseState = purse.State;
										//book avatars inform their owner that they've been picked up
										//then they are discarded - they are never added to inventory
										//item.worlditem.SetMode (WIMode.RemovedFromGame);
								} else {
										System.Object purseStateObject = null;
										if (item.GetStateOf <Purse>(out purseStateObject)) {
												//get state using generic method since it may be a stackitem
												purseState = (PurseState)purseStateObject;
										}
								}
								GUIManager.PostGainedItem(purseState);
								//add the contents of our purse to the bank, then destroy the item
								State.PlayerBank.Add(purseState.Bronze, WICurrencyType.A_Bronze);
								State.PlayerBank.Add(purseState.Silver, WICurrencyType.B_Silver);
								State.PlayerBank.Add(purseState.Gold, WICurrencyType.C_Gold);
								State.PlayerBank.Add(purseState.Lumen, WICurrencyType.D_Luminite);
								State.PlayerBank.Add(purseState.Warlock, WICurrencyType.E_Warlock);
								item.RemoveFromGame();
								return true;
						}

						if (item.Is <Currency>()) {
								GUIManager.PostGainedItem(Mathf.Max(Mathf.FloorToInt(item.BaseCurrencyValue), 1), item.CurrencyType);
								State.PlayerBank.Add(Mathf.Max(Mathf.FloorToInt(item.BaseCurrencyValue), 1), item.CurrencyType);
								item.RemoveFromGame();
								return true;
						}

						if (item.Is <Key>()) {
								KeyState keyState = null;
								if (item.IsWorldItem) {
										Key key = item.worlditem.Get <Key>();
										keyState = key.State;
								} else {
										StackItem keyItem = item.GetStackItem(WIMode.Unloaded);
										keyItem.GetStateData <KeyState>(out keyState);
								}
								item.RemoveFromGame();
								if (State.PlayerKeyChain.AddKey(keyState)) {
										GUIManager.PostSuccess("Added key to log");
								}
								return true;
						}

						if (item.Is <BookAvatar>()) {
								Debug.Log("Item is book avatar");
								if (item.IsWorldItem) {	
										BookAvatar avatar = item.worlditem.Get <BookAvatar>();
										Books.AquireBook(avatar.State.BookName);
										//book avatars inform their owner that they've been picked up
										//then they are discarded - they are never added to inventory
										//item.worlditem.SetMode (WIMode.RemovedFromGame);
								} else {
										System.Object bookAvatarStateObject = null;
										if (item.GetStateOf <BookAvatar>(out bookAvatarStateObject)) {
												//get state using generic method since it may be a stackitem
												BookAvatarState bookAvatarState = (BookAvatarState)bookAvatarStateObject;
												Books.AquireBook(bookAvatarState.BookName);
										} else {
												Debug.Log("Couldn't get state of book avatar");
										}
								}
								//never put books in our inventory
								Debug.Log("Removing book from game");
								item.RemoveFromGame();
								return true;
						}

						//-----GENERAL CASE-----//
						mAddingItem = true;
						bool addResult = false;

						//this may have been set by CanItemFit
						//or it may have been set the last time we added something
						//try to use it again
						if (mLastRelevantStack != null && mLastRelevantStack.Group == WIGroups.Get.Player) {
								addResult = Stacks.Push.Item(mLastRelevantStack, item, true, StackPushMode.Manual, ref error);
						} else {
								//bleh, set it to null, it's outdated
								mLastRelevantStack = null;
						}
						if (!addResult) {
								if (FindMostRelevantStack(ref mLastRelevantStack, item, true, true, ref error)) {
										addResult = Stacks.Push.Item(mLastRelevantStack, item, true, StackPushMode.Manual, ref error);
								} else {
										Debug.Log("Couldn't find a relevant stack");
								}
						}

						if (!addResult) {
								//whoops! drop it at our feed
								GUIManager.PostInfo(item.DisplayName + " wouldn't fit.");
								item.Add("OwnedByPlayer");
								player.ItemPlacement.ItemDropAtFeet(item);
								return true;
						} else {
								mRefreshCurrency = true;
								if (item.IsWorldItem) {
										item.worlditem.OnAddedToPlayerInventory.SafeInvoke();
										item.worlditem.SetMode(WIMode.Stacked);
										LastAddedItems.Add(item);
										LastAddedTime = WorldClock.AdjustedRealTime;
										Player.Get.AvatarActions.ReceiveAction((AvatarAction.ItemAddToInventory), WorldClock.AdjustedRealTime);
								}
								if (item.IsQuestItem) {
										State.QuestItemsAcquired.SafeAdd(item.QuestName);
										Player.Get.AvatarActions.ReceiveAction((AvatarAction.ItemQuestItemAddToInventory), WorldClock.AdjustedRealTime);
								}
								Player.Get.AvatarActions.ReceiveAction((AvatarAction.ItemAddToInventory), WorldClock.AdjustedRealTime);
								GUIManager.PostInfo("Added " + item.DisplayName + " to inventory.");
						}
						mAddingItem = false;

						return addResult;
				}
				//this is where we find the best stack to put something when it's added to the inventory
				public bool FindMostRelevantStack(ref WIStack mostRelevantStack, IWIBase item, bool enablerStacksOk, bool compatibleOK, ref WIStackError error)
				{
						//first see if the stack we have will fit it
						if (mostRelevantStack != null) {
								if (!mostRelevantStack.IsFull && Stacks.Can.Stack(mostRelevantStack, item)) {
										return true;
								}
						}

						mostRelevantStack = null;
						bool checkedQuickslots = false;
						bool foundFirstEmpty = false;
						bool foundFirstCompatible = false;
						WIStack firstEmpty = null;
						WIStack firstCompatible = null;

						if (item.IsStackContainer) {	
								if (!QuickslotEnabler.HasEnablerTopItem) {	//if quickslots are empty
										//always fill the quickslots stack first
										//don't bother to check for stack size, it'll be NoLimit
										mostRelevantStack = QuickslotEnabler.EnablerStack;
										return true;
								}

								if (QuickslotEnabler.IsEnabled
								&& Stacks.Can.Fit(item.Size, QuickslotEnabler.EnablerContainer.Size)) {	//if quickslots have a stack and they're enabled
										//and the item will fit in a container that size
										//fill quickslots first
										checkedQuickslots = true;
										foreach (WIStack stack in QuickslotEnabler.EnablerStacks) {	
												if (Stacks.Can.Stack(stack, item)) {
														mostRelevantStack = stack;
														return true;
												}
										}
								}
								//if we've gotten this far then quickslots are full
								//so check the remaining enablers to see if they need containers
								foreach (WIStackEnabler enabler in InventoryEnablers) {
										if (!enabler.HasEnablerTopItem) {	//fill enabler stacks before adding items to existing containers
												//don't bother to check for size, it'll be NoLimit
												mostRelevantStack = enabler.EnablerStack;
												return true;
										}
								}
						}

						//if quickslots enabler is filled and
						//our other enablers are filled, start looking for
						//other compatible stacks
						if (!checkedQuickslots && QuickslotEnabler.IsEnabled) {
								List <WIStack> quickslotStacks = QuickslotEnabler.EnablerStacks;
								for (int i = 0; i < quickslotStacks.Count; i++) {
										WIStack stack = quickslotStacks[i];
										if (!foundFirstEmpty && stack.IsEmpty) {
												foundFirstEmpty = true;
												firstEmpty = stack;
										}
										if (!foundFirstCompatible && compatibleOK && Stacks.Can.Stack(stack, item) && !stack.IsFull) {
												foundFirstCompatible = true;
												firstCompatible = stack;
										}

										if (foundFirstCompatible && compatibleOK) {//that's all we need
												break;
										}
								}
						}

						if (!foundFirstCompatible || (!compatibleOK && !foundFirstEmpty)) {
								//we've already searched in quickslots so now search in the rest
								//next search in each of the other containers
								foreach (WIStackEnabler enabler in InventoryEnablers) {	//don't bother to check the enabler stack we already did that
										//just check the enabler's container stacks
										if (enabler.IsEnabled) {
												if (Stacks.Can.Fit(item.Size, enabler.EnablerContainer.Size)) {
														foreach (WIStack stack in enabler.EnablerStacks) {
																if (!foundFirstEmpty
																&& stack.IsEmpty) {
																		foundFirstEmpty = true;
																		firstEmpty = stack;
																}
																if (!foundFirstCompatible && compatibleOK && Stacks.Can.Stack(stack, item) && !stack.IsFull) {
																		foundFirstCompatible = true;
																		firstCompatible = stack;
																}

																if (foundFirstCompatible && compatibleOK) {
																		//that's all we need
																		break;
																}
														}
												}
										}

										if (foundFirstCompatible && compatibleOK) {//that's all we need
												break;
										}
								}
						}

						if (!foundFirstEmpty) {
								//last possible spot - the carry stack
								if (State.QuickslotsStackCarry.IsEmpty) {
										firstEmpty = State.QuickslotsStackCarry;
								}
						}

						if (foundFirstCompatible && compatibleOK) {
								mostRelevantStack = firstCompatible;
								return true;
						} else if (foundFirstEmpty) {
								mostRelevantStack = firstEmpty;
								return true;
						}
						//catch-all
						mostRelevantStack = null;
						return false;
				}
				//instead of popping and adding items one at a time
				//this function adds the whole stack to the supplied stack
				//this totally bypasses all checks so it's quick, like swapping stacks in an inventory square
				public bool QuickAddItems(WIStack fromStack, ref WIStackError error)
				{
						Debug.Log("Swapping stacks");
						return false;
						//return Stacks.Swap.Stacks(fromStack, toStack, ref error);
				}

				public bool AddItems(WIStack stack, ref WIStackError error)
				{
						if (stack == null) {
								error = WIStackError.NotCompatible;
								return false;
						}

						IWIBase topItem = null;
						while (!stack.IsEmpty) {
								if (!Stacks.Pop.Top(stack, out topItem, WIGroups.Get.Player, ref error)) {
										return false;
								} else if (!AddItems(topItem, ref error)) {
										Stacks.Push.Item(stack, topItem, ref error);
										return false;
								}
						}
						return true;
				}

				public bool CanItemFit(IWIBase item)
				{
						WIStackError error = WIStackError.None;
						return item.HasAtLeastOne(NoSpaceNeededItems) || FindMostRelevantStack(ref mLastRelevantStack, item, true, true, ref error);
				}

				protected WIStack mLastRelevantStack;

				public bool PushSelectedStack()
				{
						WIStackError error = WIStackError.None;
						return AddItems(SelectedStack, ref error);
				}

				#endregion

				#region search

				//TODO clean these up, most aren't fully implemented
				public void TakeQuestItemFromPlayer(string itemName)
				{
						IWIBase questItem = null;
						//check quickslots first
						foreach (WIStack stack in QuickslotEnabler.EnablerStacks) {
								for (int i = 0; i < stack.Items.Count; i++) {
										if (stack.Items[i] != null && stack.Items[i].QuestName == itemName) {
												questItem = stack.Items[i];
												break;
										}
								}
						}
						//if we haven't found it yet, keep going
						if (questItem == null) {
								foreach (WIStackEnabler enabler in InventoryEnablers) {	//don't bother to check the enabler stack we already did that
										foreach (WIStack stack in enabler.EnablerStacks) {
												for (int i = 0; i < stack.Items.Count; i++) {
														if (stack.Items[i] != null && stack.Items[i].QuestName == itemName) {
																questItem = stack.Items[i];
																break;
														}
												}
										}
								}
						}
						if (questItem != null) {
								questItem.RemoveFromGame();
						}
				}

				public void FindItemsOfType(string scriptName, List <IWIBase> itemsOfType)
				{
						for (int i = 0; i < State.InventoryStacks.Count; i++) {
								Stacks.Find.ItemsOfType(State.InventoryStacks[i], scriptName, true, itemsOfType);
						}
						Stacks.Find.ItemsOfType(State.QuickslotsStack, scriptName, true, itemsOfType);
						Stacks.Find.ItemsOfType(State.QuickslotsStackCarry, scriptName, true, itemsOfType);
				}

				public bool RemoveFirstByKeyword(string keyword, out WorldItem firstFoundItem)
				{
						//TODO re-implement
						mRunningSearch = true;
						firstFoundItem = null;
						bool result = false;

						mRunningSearch = false;
						return result;
				}

				public bool FindFirstByKeyword(string keyword, out IWIBase firstFoundItem)
				{
						//TODO re-implement
						mRunningSearch = true;
						firstFoundItem = null;
						mRunningSearch = false;
						return false;
				}

				public bool FindProjectileForWeapon(Weapon weapon, out IWIBase projectile, out WIStack projectileStack)
				{
						bool foundProjectile = false;
						projectile = null;
						projectileStack = null;
						if (QuickslotsEnabled) {
								//look through the quickslots - we only search for projectiles in quickslots
								List <WIStack> stacks = QuickslotEnabler.EnablerStacks;
								for (int i = 0; i < stacks.Count; i++) {
										if (stacks[i].HasTopItem) {
												IWIBase topItem = stacks[i].TopItem;
												if (Weapon.CanLaunch(weapon, topItem)) {
														//hooray, we've found one we can use
														WIStackError stackError = WIStackError.None;
														Stacks.Pop.Top(stacks[i], out projectile, WIGroups.Get.Player, ref stackError);
														projectileStack = stacks[i];
														foundProjectile = true;
														break;
												}
										}
								}
						}
						return foundProjectile;
				}

				public int NumberOf(string keyword)
				{
						//TODO re-implement
						int numItems = 0;
						return numItems;
				}

				public bool GiveQuestItemToCharacter(string questItemName, string characterName)
				{
						return false;
				}

				public bool HasQuestItem(string questItemName)
				{
						return State.QuestItemsAcquired.Contains(questItemName);
				}

				public bool HasItem(string itemName)
				{
						IWIBase foundObject = null;
						return FindFirstByKeyword(itemName, out foundObject);
				}

				public bool HasItem(string itemName, out IWIBase foundObject)
				{
						return FindFirstByKeyword(itemName, out foundObject);
				}

				#endregion

				#region IInventory implementation

				public string InventoryOwnerName {
						get {
								return Player.Local.DisplayName;
						}
				}

				public IEnumerator GetInventoryContainer(int currentIndex, bool forward, GetInventoryContainerResult result)
				{	//used primarily by barter
						if (result == null)
								yield break;

						//get the target index
						int targetIndex = currentIndex + 1;
						if (!forward) {
								targetIndex = currentIndex - 1;
						}

						List <WIStackEnabler> enablers = new List<WIStackEnabler>();
						enablers.Add(QuickslotEnabler);
						List <WIStackEnabler> inventoryEnablers = InventoryEnablers;
						for (int i = 0; i < inventoryEnablers.Count; i++) {
								if (inventoryEnablers[i].IsEnabled) {
										enablers.Add(inventoryEnablers[i]);
								}
						}
						//wrap the current index to the enablers count
						if (targetIndex >= enablers.Count) {
								targetIndex = 0;
						} else if (targetIndex < 0) {
								targetIndex = enablers.Count - 1;
						}

						result.ContainerEnabler = enablers[targetIndex];
						result.ContainerIndex = targetIndex;
						yield break;
				}

				public IEnumerator AddItem(IWIBase item)
				{
						WIStackError error = WIStackError.None;
						AddItems(item, ref error);
						yield break;
				}

				public IEnumerator AddItems(WIStack stack, int numItems)
				{
						IStackOwner owner = null;
						WIStackError error = WIStackError.None;
						for (int i = 0; i < numItems; i++) {
								if (!AddItems(stack.TopItem, ref error)) {
										WIGroup group = WIGroups.Get.World;
										if (player.Surroundings.IsVisitingLocation) {
												group = player.Surroundings.CurrentLocation.LocationGroup;
										}
										Stacks.Pop.ContentsIntoWorld(stack, Int32.MaxValue, Player.Local.Grabber.transform.position, group);
								} else {
										Stacks.Pop.Force(stack, false);
								}
						}
						yield break;
				}

				public bool HasBank { get { return true; } }

				public Bank InventoryBank { get { return State.PlayerBank; } }

				public Action OnAccessInventory { get { return mOnAccessInventory; } set { mOnAccessInventory = value; } }

				protected Action mOnAccessInventory;

				#endregion

				public void Update()
				{
						if (InterfaceActionManager.AvailableKeyDown) {
								SelectionNumeric(WorldClock.RealTime);
						}

						if (player.LockQuickslots)
								return;

						while (mItemsToAddOnQuickslotsUnlocked.Count > 0) {
								WIStackError error = WIStackError.None;
								AddItems(mItemsToAddOnQuickslotsUnlocked.Dequeue(), ref error);
						}
				}

				protected IEnumerator CheckCarryItemStatus()
				{
						bool qsEnabledPrev = false;
						bool hadItemPrev = false;
						bool changeACI = false;
						bool qsEnabledNow = false;
						bool hasItemNow = false;

						mCheckingCarryItemStatus = true;
						while (mCheckingCarryItemStatus) {
								while (!mInitialized && !mStateLoaded && State.QuickslotsStackCarry == null) {
										mChangingAci = false;
										yield return null;
								}

								mChangingAci = false;

								while (player.LockQuickslots) {
										yield return null;
								}

								if (mActiveCarryItem != null && mActiveCarryItem.Group != WIGroups.Get.Player) {
										mActiveCarryItem = null;
								}
								qsEnabledPrev = QuickslotsEnabled;
								hadItemPrev = qsEnabledPrev && State.QuickslotsStackCarry.HasTopItem && mActiveCarryItem == State.QuickslotsStackCarry.TopItem;

								double waitUntil = WorldClock.RealTime + 0.025f;
								while (WorldClock.RealTime < waitUntil) {
										yield return null;
								}

								mChangingAci = false;
								//do we want to change our active quickslot item?
								changeACI = false;
								qsEnabledNow = QuickslotsEnabled;
								hasItemNow = false;
								if (qsEnabledNow) {
										hasItemNow = State.QuickslotsStackCarry.HasTopItem;
								}

								if (hasItemNow) {	//if we have a different item
										//that's an instant change
										changeACI = (mActiveCarryItem != State.QuickslotsStackCarry.TopItem);
								}

								if (!changeACI) {	//if it's the same item, check the following
										//change it if:
										//- quickslot has changed
										//- we had an item but it's either null or set to RemovedFromGame
										//- we have an item now but didn't before
										//- the item is not the same item
										//- quickslots were enabled but are no longer enabled
										//- we have an item but quickslots aren't enabled
										changeACI |= (hadItemPrev && !hasItemNow);
										changeACI |= (!hadItemPrev && hasItemNow);
										changeACI |= (hasItemNow && !qsEnabledNow);
										changeACI |= (qsEnabledPrev && !qsEnabledNow);
								}

								if (changeACI && ChangeACI(hadItemPrev, qsEnabledNow)) {
										mChangingAci = true;
								}

								//---this part is relatively safe so don't bother with exception handling
								if (mChangingAci) {
										//Debug.Log ("----Changing quickslot ACI...");
										//if it has changed and we're supposed to announce the change do so now
										//this action should ONLY originate from here
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemACIChange, WorldClock.AdjustedRealTime);
										//give objects a bit to reaction before actually refreshing containers
										yield return null;
										//if the previous item was equipped then we have to wait for the tool to finish unequipping it
										//because if it doesn't use a doppleganger we'll get a jarring disappearing tool
										if (hadItemPrev) {
												while (player.Carrier.ToolState == PlayerToolState.Unequipping) {
														//Debug.Log ("Waiting for tool to UNequip in inventory...");
														yield return null;
												}
										}
										yield return null;
										//okay now we want to wait for the tool to equip before making any more changes
										//this will keep quickslots locked until the tool is ready to go
										while (player.Carrier.ToolState == PlayerToolState.Equipping) {
												//then wait for the tool to equip
												//Debug.Log ("Waiting for tool to equip in inventory...");
												yield return null;
										}
										//Debug.Log ("DONE waiting for tool to equip");
										GUIInventoryInterface.Get.RefreshContainers();
										mChangingAci = false;
								}
						}
				}

				protected IEnumerator CheckQuickslotStatus()
				{	//check over time to see if quickslot has changed in some way
						//if it has announce it to any subscribers
						int lastQuickslot = 0;
						bool qsEnabledPrev = false;
						bool hadItemPrev = false;
						bool changeAQI = false;
						bool qsEnabledNow = false;
						bool hasItemNow = false;

						mCheckingQuickslotStatus = true;
						while (mCheckingQuickslotStatus) {
								while (!mInitialized && !mStateLoaded && State.ActiveQuickslot <= 0) {	//wait for the player to initialize & for state to update
										mChangingAqi = false;
										yield return null;
								}

								mChangingAqi = false;

								while (player.LockQuickslots) {
										//other scripts are something important that we can't interrupt
										//Debug.Log ("Waiting for quickslots to unlock...");
										yield return null;
								}

								//---Check the current quickslot status
								//first check if the active quickslot item is gone / in the world
								if (mActiveQuickslotItem != null && mActiveQuickslotItem.Group != WIGroups.Get.Player) {
										//if it is set the aqi to null
										mActiveQuickslotItem = null;
								}
								//store the last quickslot to see if there's a change
								lastQuickslot = State.ActiveQuickslot;
								lastQuickslotStack = null;
								currentQuickslotStack = null;
								if (lastQuickslot >= 0 && lastQuickslot < QuickslotEnabler.EnablerStacks.Count) {
										lastQuickslotStack = QuickslotEnabler.EnablerStacks[lastQuickslot];
								}
								qsEnabledPrev = QuickslotsEnabled;
								hadItemPrev = qsEnabledPrev && lastQuickslotStack.HasTopItem && mActiveQuickslotItem == lastQuickslotStack.TopItem;
								//wait a bit (delay to prevent 'spamming' of quickslot changes)

								double waitUntil = WorldClock.RealTime + 0.025f;
								while (WorldClock.RealTime < waitUntil) {
										yield return null;
								}

								//---Check to see if anything has changed
								//try {
								mChangingAqi = false;
								//do we want to change our active quickslot item?
								changeAQI = false;
								qsEnabledNow = QuickslotsEnabled;
								hasItemNow = false;
								if (qsEnabledNow) {
										currentQuickslotStack = QuickslotEnabler.EnablerStacks[State.ActiveQuickslot];
										hasItemNow = currentQuickslotStack.HasTopItem;
								}

								if (hasItemNow) {	//if we have a different item
										//that's an instant change
										changeAQI = (mActiveQuickslotItem != QuickslotEnabler.EnablerStacks[State.ActiveQuickslot].TopItem);
								}

								if (!changeAQI) {	//if it's the same item, check the following
										//change it if:
										//- quickslot has changed
										//- we had an item but it's either null or set to RemovedFromGame
										//- we have an item now but didn't before
										//- the item is not the same item
										//- quickslots were enabled but are no longer enabled
										//- we have an item but quickslots aren't enabled
										changeAQI |= (lastQuickslot != State.ActiveQuickslot);
										changeAQI |= (hadItemPrev && !hasItemNow);
										changeAQI |= (!hadItemPrev && hasItemNow);
										changeAQI |= (hasItemNow && !qsEnabledNow);
										changeAQI |= (qsEnabledPrev && !qsEnabledNow);
								}

								//---Change the AQI if it's now different
								if (changeAQI && ChangeAQI(hadItemPrev, qsEnabledNow, lastQuickslotStack, currentQuickslotStack)) {
										mChangingAqi = true;
								}

								//---this part is relatively safe so don't bother with exception handling
								if (mChangingAqi) {
										Debug.Log("----Changing quickslot AQI...");
										//if it has changed and we're supposed to announce the change do so now
										//this action should ONLY originate from here
										Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemAQIChange, WorldClock.AdjustedRealTime);
										//give objects a bit to reaction before actually refreshing containers
										yield return null;
										//if the previous item was equipped then we have to wait for the tool to finish unequipping it
										//because if it doesn't use a doppleganger we'll get a jarring disappearing tool
										if (hadItemPrev && lastQuickslotStack != null && lastQuickslotStack != currentQuickslotStack) {
												while (player.Tool.ToolState == PlayerToolState.Unequipping) {
														//Debug.Log ("Waiting for tool to UNequip in inventory...");
														yield return null;
												}
												//tell the old AQI that it's no longer equipped / turn it into a stack item
												//(this request will be ignored if the item doesn't unload when stacked)
												StackItem newStackItem = null;
												Stacks.Convert.TopItemToStackItem(lastQuickslotStack, out newStackItem);
										}
										yield return null;
										//okay now we want to wait for the tool to equip before making any more changes
										//this will keep quickslots locked until the tool is ready to go
										while (player.Tool.ToolState == PlayerToolState.Equipping) {
												//then wait for the tool to equip
												yield return null;
										}
										GUIInventoryInterface.Get.RefreshContainers();
										mChangingAqi = false;
								}
						}
						mCheckingQuickslotStatus = false;
						yield break;
				}

				protected bool ChangeACI(bool hadItemPrev, bool qsEnabledNow)
				{
						bool announceChange = true;
						newACI = null;
						if (qsEnabledNow) {
								if (State.QuickslotsStackCarry.HasTopItem) {
										newACIBase = State.QuickslotsStackCarry.TopItem;
										if (newACIBase.IsWorldItem) {
												newACI = newACIBase.worlditem;
										} else {
												Stacks.Convert.TopItemToWorldItem(State.QuickslotsStackCarry, out newACI);
												newACI.SetMode(WIMode.Equipped);
										}
								} else if (!hadItemPrev) {
										announceChange = false;
								}
						}
						mActiveCarryItem = newACI;

						return announceChange;
				}

				protected IWIBase newACIBase = null;
				protected WorldItem newACI = null;

				protected bool ChangeAQI(bool hadItemPrev, bool qsEnabledNow, WIStack lastQuickslotStack, WIStack currentQuickslotStack)
				{
						bool announceChange = true;
						newAQI = null;

						try {
								if (qsEnabledNow) {
										//see if we need to send the previous AQI back
										//(for now ignore because they function fine)
										//now get the new AQI starting with quickslot stack
										//this SHOULD work because the active quickslot will be error checked
										//and we're guaranteed enabler stacks by quickslotsEnabled
										aqiQuickslotStack = QuickslotEnabler.EnablerStacks[State.ActiveQuickslot];
										//if the quickslot stack has a top item
										if (aqiQuickslotStack.HasTopItem) {
												//get the top item, then check if it's a world item
												newAQIBase = aqiQuickslotStack.TopItem;
												if (newAQIBase.IsWorldItem) {
														//if it's already a worlditem then great, we're done
														newAQI = newAQIBase.worlditem;
														if (newAQI == mActiveQuickslotItem) {
																//if they're already the same object for some reason
																//(maybe it was a stack swap?)
																//then there's no need to announce anything
																announceChange = false;
														}//otherwise just set & announce it at the end normally
												} else {
														//Debug.Log ("is quickslots stack group null? " + (aqiQuickslotStack.Group == null).ToString());
														//if it's not a world item then we need to turn it into one
														Stacks.Convert.TopItemToWorldItem(aqiQuickslotStack, out newAQI);
														newAQI.SetMode(WIMode.Equipped);
														//if this doesn't work... erm, TODO check for this not working lol
												}
										} else {
												if (!hadItemPrev) { //Debug.Log ("PLAYERINVENTORY: had NO item previously, not announcing"); 
														//if the quickslot stack doesn't have a top item
														//if we already DON'T have an AQI
														//then we don't need to announce anything
														announceChange = false;
												}
										}
								}
								//set the new AQI (even if it's null)
								mActiveQuickslotItem = newAQI;
						} catch (Exception e) {
								Debug.Log("Exception while changing AQI: " + e.ToString());
								return false;
						}

						return announceChange;
				}

				protected WIStack lastQuickslotStack = null;
				protected WIStack currentQuickslotStack = null;
				protected WIStack aqiQuickslotStack;
				protected WorldItem newAQI = null;
				protected IWIBase newAQIBase = null;
				protected bool mChangingAqi = false;
				protected bool mChangingAci = false;
				protected bool mQuickslotChanged = false;
				protected bool mRefreshCurrency = false;
				protected bool mRunningSearch = false;
				protected bool mAddingItem = false;
				protected bool mCheckingQuickslotsForTools = false;
				protected bool mCheckingQuickslotStatus	= false;
				protected bool mCheckingCarryItemStatus = false;
				//we don't have to save this in the state
				//because it will always be kept in the quickslot stack
				//and will be flattened when we save the stack state
				protected WorldItem mActiveQuickslotItem = null;
				protected WorldItem mActiveCarryItem = null;
				protected List <IWIBase> mItemsToAddOnInitialization = new List <IWIBase>();
				protected Queue <IWIBase> mItemsToAddOnQuickslotsUnlocked = new Queue <IWIBase>();
		}

		[Serializable]
		public class PlayerInventoryState
		{
				//this is where all of our inventory data is saved
				public Bank PlayerBank = new Bank();
				public KeyChain PlayerKeyChain = new KeyChain();
				public List <MobileReference> OwnedStructures = new List <MobileReference>();
				public int ActiveQuickslot = -1;
				public WIStack QuickslotsStack = null;
				public WIStack QuickslotsStackCarry = null;
				public List <WIStack> InventoryStacks = null;
				public List <string> QuestItemsAcquired = new List <string>();
				public List <string> QuestItemsRemoved = new List <string>();
		}

		[Serializable]
		public class KeyChain
		{
				public bool AddKey(string keyType, string keyTag, string keyName)
				{
						for (int i = 0; i < Keys.Count; i++) {
								//don't add the key if we already have a copy of it
								if (Keys[i].KeyType == keyType &&
								Keys[i].KeyTag == keyTag &&
								Keys[i].KeyName == keyName) {
										return false;
								}
						}
						//create a key state
						KeyState keyState = new KeyState();
						keyState.KeyName = keyName;
						keyState.KeyTag = keyTag;
						keyState.KeyType = keyType;
						Keys.Add(keyState);
						return true;
				}

				public bool AddKey(KeyState newKey)
				{
						if (newKey == null) {
								return false;
						}
						for (int i = 0; i < Keys.Count; i++) {
								//don't add the key if we already have a copy of it
								if (Keys[i].KeyType == newKey.KeyType &&
								Keys[i].KeyTag == newKey.KeyTag &&
								Keys[i].KeyName == newKey.KeyName) {
										return false;
								}
						}
						Keys.Add(newKey);
						return true;
				}

				public List <KeyState> Keys = new List <KeyState>();
		}
}
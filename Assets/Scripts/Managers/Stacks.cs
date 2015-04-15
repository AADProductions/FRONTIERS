using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.WIScripts;
using System.Linq;

namespace Frontiers
{
		public class Stacks : Manager
		{
				public static class Add
				{
						public static bool Items(IWIBase newItem, WIStackContainer toContainer, ref WIStackError error)
						{
								bool result = false;
								for (int i = 0; i < toContainer.StackList.Count; i++) {
										if (Stacks.Add.Items(newItem, toContainer.StackList[i], ref error)) {
												result = true;
												break;
										}
								}
								if (!result) {
										error = WIStackError.IsFull;
								}
								return result;
						}

						public static bool Items(WIStack fromStack, WIStackContainer toContainer, ref WIStackError error)
						{
								bool result = false;
								foreach (WIStack toStack in toContainer.StackList) {
										if (Stacks.Add.Items(fromStack, toStack, ref error)) {
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool Items(IWIBase newItem, WIStack toStack, ref WIStackError error)
						{
								return Stacks.Push.Item(toStack, newItem, ref error);
						}

						public static bool Items(WIStack fromStack, WIStack toStack, ref WIStackError error)
						{
								bool result	= false;

								while (Stacks.Pop.AndPush(fromStack, toStack, ref error)) {
										//Debug.Log ("Popping and pushing from stack to stack...");
										//do nothing;
								}

								if (fromStack.IsEmpty) {
										//Debug.Log ("From stack is now empty");
										//we cleared out the whole stack
										result = true;
								}

								toStack.Refresh();
								return result;
						}
				}

				public static class Contains {
			
						public static bool QuestItem (WIStack stack) {
								if (stack == null || stack.IsEmpty)
										return false;

								for (int i = 0; i < stack.Items.Count; i++) {
										if (stack.Items[i].IsQuestItem) {
												return true;
										}
								}
								return false;
						}
				}

				public static class Pop
				{
						public static void Force(WIStack stack)
						{
								Force(stack, false);
						}

						public static void Force(WIStack stack, bool removeFromGame)
						{
								if (stack.HasTopItem) {
										IWIBase topItem = stack.TopItem;
										int topItemIndex = stack.Items.Count - 1;
										stack.Items.RemoveAt(topItemIndex);
										//set the top item's OnRemovedFromStack to null to avoid doubling up
										//since we're here while it's happening it doesn't need to call it
										topItem.OnRemoveFromStack = null;
										if (removeFromGame && topItem.IsWorldItem) {
												//if it's not a world item it'll be up to the user to set the mode
												topItem.worlditem.SetMode(WIMode.RemovedFromGame);
										}
										//call this manually
										stack.OnItemRemoved();
								}
						}

						static IWIBase mTopItem;
						static WIGroup mToGroup;

						public static bool AndPush(WIStack fromStack, WIStack toStack, ref WIStackError error)
						{
								if (!fromStack.HasTopItem) {
										//Debug.Log ("from stack has no top item, returning");
										return false;
								}

								mToGroup = toStack.Group;
								mTopItem = fromStack.TopItem;

								//Debug.Log ("From stack started with " + fromStack.NumItems.ToString () + " items");
								if (!Stacks.Can.Add(mTopItem, toStack, ref error)) {
										//Debug.Log ("Can't add top item to stack because " + error.ToString ());
										return false;
								}

								if (!Stacks.Pop.Top(fromStack, out mTopItem, mToGroup, ref error)) {
										//Debug.Log ("Couldn't pop top from stack because " + error.ToString ());
										return false;
								}

								if (!Stacks.Push.Item(toStack, mTopItem, ref error)) {
										//Debug.Log ("Couldn't push top item to stack because " + error.ToString ());
										//oops, put it back where we found it
										Stacks.Push.Item(fromStack, mTopItem, ref error);
										//Debug.Log ("Putting item back where we found it");
										return false;
								}
								//Debug.Log ("From stack now has " + fromStack.NumItems.ToString () + " items");
								return true;
						}

						public static bool Top(WIStackContainer container, out IWIBase topItem, WIGroup group, ref WIStackError error)
						{
								topItem = null;
								bool result = false;
								foreach (WIStack stack in container.StackList) {
										if (Top(stack, out topItem, group, ref error)) {
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool Top(WIStack stack, out IWIBase topItem, WIGroup toGroup, ref WIStackError error)
						{
								topItem = null;
								if (stack.HasTopItem) {
										int topItemIndex = stack.Items.Count - 1;
										topItem = stack.Items[topItemIndex];
										stack.Items.RemoveAt(topItemIndex);
										//set the top item's OnRemovedFromStack to null to avoid doubling up
										//since we're here while it's happening it doesn't need to call it
										topItem.OnRemoveFromStack = null;
										if (topItem.IsWorldItem) {
												toGroup.AddChildItem(topItem.worlditem);
										} else if (!topItem.UnloadWhenStacked) {	//if we're not supposed to use a template item when stacked
												//then we have to create a world item to return
												WorldItem worlditem = null;
												if (WorldItems.CloneFromStackItem(topItem.GetStackItem(WIMode.Stacked), toGroup, out worlditem)) {
														//TODO something something
														topItem = worlditem;
												}
										}
										//call this manually
										stack.OnItemRemoved();
										return true;
								}
								return false;
						}

						public static void AndToss(WIStack stack)
						{
								//pops the top item off the stack and destroys it
								if (stack.HasTopItem) {
										IWIBase topItem = stack.TopItem;
										topItem.RemoveFromGame();
										//this should automatically refresh the stack
								}
						}

						public static bool TopIntoWorld(WIStack stack, out WorldItem newTopItem)
						{
								if (stack.HasTopItem) {
										IWIBase topItem = stack.TopItem;
										if (topItem.IsWorldItem) {
												newTopItem = topItem.worlditem;
												return true;
										} else if (Convert.TopItemToWorldItem(stack, out newTopItem)) {
												WIStackError error = WIStackError.None;
												if (Pop.Top(stack, out topItem, WIGroups.Get.World, ref error)) {
														//set to world mode
														newTopItem.SetMode(WIMode.World);
														return true;
												}
										}
								}

								newTopItem = null;
								return false;
						}

						public static void ContentsIntoWorld(WIStackContainer container, int numItems, Vector3 worldPosition)
						{
								foreach (WIStack stack in container.StackList) {
										ContentsIntoWorld(stack, numItems, worldPosition, WIGroups.Get.World);
								}
						}

						public static void ContentsIntoWorld(WIStack stack, int numItems, Vector3 worldPosition, WIGroup group)
						{
								IWIBase topItem = null;
								WorldItem worlditem = null;
								WIStackError error = WIStackError.None;
								for (int i = 0; i < numItems; i++) {
										if (Convert.TopItemToWorldItem(stack, out worlditem) && Pop.Top(stack, out topItem, group, ref error)) {
												worlditem.SetMode(WIMode.World);
												worlditem.ActiveState = WIActiveState.Active;
												worlditem.transform.position = worldPosition;
												worlditem.LastActiveDistanceToPlayer = 0f;
										}
								}
								stack.Refresh();
						}
				}

				public static class Push
				{
						public static bool Item(WIStackContainer container, IWIBase newTopItem, ref WIStackError error)
						{
								bool result = false;
								foreach (WIStack stack in container.StackList) {
										if (Stacks.Push.Item(stack, newTopItem, ref error)) {
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool Item(WIStack stack, IWIBase item, ref WIStackError error)
						{
								return Item(stack, item, true, StackPushMode.Auto, ref error);
						}

						public static bool Item(WIStack stack, IWIBase item, StackPushMode pushMode, ref WIStackError error)
						{
								return Item(stack, item, true, pushMode, ref error);
						}

						public static bool Item(WIStack stack, IWIBase item, bool toTop, StackPushMode pushMode, ref WIStackError error)
						{
								if (item == null) {
										//Debug.Log ("Item was null when pushing to stack");
										//TODO return false? define behavior
										return true;
								}

								WIGroup fromGroup = item.Group;
								if (Stacks.Can.Add(item, stack, ref error)) {
										if (stack.Group != fromGroup) {
												if (stack.Group != null) {
														//we have to move it from one group to another
														if (!stack.Group.AddChildItem(item)) { //Debug.Log ("couldn't add to group " + stack.Group.name + ", not adding to stack");
																error = WIStackError.InvalidOperation;
																return false;
														}
												} else {
														//Debug.Log ("Stack group was null");
												}
										}
										//this is kind of kludgey
										//if the stack sends the currency to a bank
										//and the item is currency
										//then send the currency to the bank and destroy the item
										//don't even bother pushing it onto the stack
										if (stack.SendCurrencyToBank && item.Is <Currency>()) {
												stack.Bank.AddBaseCurrencyOfType(item.BaseCurrencyValue, item.CurrencyType);
												item.RemoveFromGame();
												return true;
										}
										//also kind of kludgey - keep track of quest items
										if (stack.Group == WIGroups.Get.Player && item.IsQuestItem) {
												Player.Local.Inventory.AddQuestItem(item.QuestName);
										}
										//flattening worlditems into stack items - these are treated asymmetrically
										//worlditems are kept 'live' automatically
										//stack items are kept stack items automatically
										//so if you want stack items to be turned into worlditems
										//you need to use StackPushMode.Manual
										IWIBase newTopItem = item;
										//if it's supposed to unload when stacked
										if (newTopItem.UnloadWhenStacked) {
												//and it's a worlditem, and we're using auto push mode
												if (newTopItem.IsWorldItem && pushMode == StackPushMode.Auto) {
														//if we use template, just get a template and we're done
														newTopItem = item.GetStackItem(WIMode.Unloaded);
												}
										} else if (!newTopItem.IsWorldItem && pushMode == StackPushMode.Manual) {
												//if it's NOT supposed to unload when stacked and push mode is manual
												//turn it into a worlditem first
												WorldItem newWorldItem = null;
												if (WorldItems.CloneFromStackItem(item.GetStackItem(WIMode.Stacked), stack.Group, out newWorldItem)) {
														newTopItem = newWorldItem;
												} else {
														//whoops something went wrong
														//Debug.LogError("Couldn't clone from stack item in PUSH operation");
														error = WIStackError.InvalidOperation;
														return false;
												}
										}

										if (newTopItem.IsStackContainer) { //Debug.Log ("Item is stack container - setting group to new group " + stack.Group.name);
												newTopItem.StackContainer.Group = stack.Group;
										}
										//push to the top by default
										if (toTop) {
												stack.Items.Add(newTopItem);
										} else {
												stack.Items.Insert(0, newTopItem);
										}

										if (newTopItem.IsWorldItem) {
												newTopItem.worlditem.SetMode(WIMode.Stacked);
										}
										//set the OnStackRemoved action, it'll over-write the old one
										//this should have been called by now if it was pulled from a stack
										newTopItem.OnRemoveFromStack = stack.OnItemRemoved;
										stack.Refresh();
										return true;
								} else { //Debug.Log ("Can't stack, error: " + error.ToString ());
										return false;
								}
						}
				}

				public static class Can
				{
						public static bool Stack(IWIBase item1, string item2Name)
						{
								return string.Equals(item1.StackName, item2Name, System.StringComparison.InvariantCultureIgnoreCase);
						}

						public static bool Stack(GenericWorldItem item1, string item2Name)
						{
								return string.Equals(item1.StackName, item2Name, System.StringComparison.InvariantCultureIgnoreCase);
						}

						public static bool Stack(IWIBase item1, IWIBase item2) {
								if (item1.IsQuestItem || item2.IsQuestItem) {
										return false;
								}

								if (item1.StackName.Equals(item2.StackName)) {
										bool statesMatch = ((string.IsNullOrEmpty(item1.State) || string.IsNullOrEmpty(item2.State)) || (item1.State.Equals("Default") || item2.State.Equals("Default")) || item1.State.Equals(item2.State));
										bool subcatsMatch = ((string.IsNullOrEmpty(item1.Subcategory) || string.IsNullOrEmpty(item2.Subcategory)) || item1.Subcategory.Equals(item2.Subcategory));
										return statesMatch && subcatsMatch;
								}
								return false;
						}

						public static bool Stack(IWIBase item1, GenericWorldItem item2)
						{
								return item1.StackName.Equals(item2.StackName) &&
										((string.IsNullOrEmpty(item1.State) || string.IsNullOrEmpty(item2.State) || (item1.State.Equals(item2.State)))
												&& (((string.IsNullOrEmpty (item1.Subcategory) || string.IsNullOrEmpty (item2.Subcategory)) || item1.Subcategory.Equals(item2.Subcategory))));
						}

						public static bool Stack(WIStack stack1, WIStack stack2)
						{
								if (stack1.HasTopItem) {
										if (stack2.HasTopItem) {
												return Stacks.Can.Stack(stack1.TopItem, stack2.TopItem);
										}
								}
								//if either lacks a top item
								//then this will always be true
								return true;
						}

						public static bool Stack(WIStack stack1, GenericWorldItem item)
						{
								if (stack1.HasTopItem) {
										return Stacks.Can.Stack(stack1.TopItem, item);
								}
								//if either lacks a top item
								//then this will always be true
								return true;
						}

						public static bool Stack(WIStack stack1, IWIBase item)
						{
								if (stack1.HasTopItem) {
										return Stacks.Can.Stack(stack1.TopItem, item);
								}
								//if either lacks a top item
								//then this will always be true
								return true;
						}

						public static bool Fit(WISize itemSize, WISize containerSize)
						{
								bool result = false;

								if (containerSize == WISize.NoLimit) {
										//always true
										result = true;
								} else if (itemSize == WISize.NoLimit) {
										//always false
										result = false;
								} else {
										switch (itemSize) {
												case WISize.Huge:
														//only containers that hold huge are nolimit,
														//and we've already covered that possibility
														break;

												case WISize.Large:
														switch (containerSize) {
																case WISize.Huge:
																		result = true;
																		break;

																default:
																		break;
														}
														break;

												case WISize.Medium:
														switch (containerSize) {
																case WISize.Huge:
																case WISize.Large:
																		result = true;
																		break;

																default:
																		break;
														}
														break;

												case WISize.Small:
														switch (containerSize) {
																case WISize.Huge:
																case WISize.Large:
																case WISize.Medium:
																		result = true;
																		break;

																default:
																		break;
														}
														break;

												case WISize.Tiny:
														switch (containerSize) {
																case WISize.Huge:
																case WISize.Large:
																case WISize.Medium:
																case WISize.Small:
																		result = true;
																		break;

																default:
																		break;
														}
														break;

												default:
														break;
										}
								}
								return result;
						}

						public static bool AddFullContainers(WIStack stack)
						{
								//TODO other checks
								bool allowFullContainers = false;
								switch (stack.Mode) {
										case WIStackMode.Enabler:
												allowFullContainers = true;
												break;

										case WIStackMode.Wearable:
												break;

										case WIStackMode.Generic:
												allowFullContainers = !stack.BelongsToContainer;
												break;

										default:
												break;
								}
								return allowFullContainers;
						}

						public static bool Add(IWIBase newItem, WIStackContainer toContainer, ref WIStackError error)
						{
								bool result = false;
								if (!Stacks.Can.Fit(newItem.Size, toContainer.Size)) {
										error = WIStackError.TooLarge;
										return false;
								}

								foreach (WIStack toStack in toContainer.StackList) {
										if (Stacks.Can.Add(newItem, toStack, ref error)) {
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool Add(IWIBase newItem, WIStack toStack, ref WIStackError error)
						{
								if (newItem == null || toStack == null) {
										error = WIStackError.InvalidOperation;
										return false;
								}

								if (toStack.IsFull) {
										//this covers all modes
										error = WIStackError.IsFull;
										return false;
								}

								if (!Stacks.Can.Fit(newItem.Size, toStack.Size)) {
										error = WIStackError.TooLarge;
										return false;
								}

								if (!toStack.IsEmpty) {
										if (!Stacks.Can.Stack(newItem, toStack.TopItem)) {
												error = WIStackError.NotCompatible;
												return false;
										}
								}

								//we're good to go
								error = WIStackError.None;
								return true;
						}

						public static bool Add(IWIBase item, WIStack toStack)
						{
								if (toStack.HasTopItem) {
										return Stacks.Can.Stack(toStack.TopItem, item);
								}
								return true;
						}
				}

				public static class Clear
				{
						public static IWIBase mClearTopItem = null;

						public static void DestroyedOrMovedItems(WIStackContainer container)
						{
								for (int i = 0; i < container.StackList.Count; i++) {
										DestroyedOrMovedItems(container.StackList[i]);
								}
						}

						public static void DestroyedOrMovedItems(WIStack stack)
						{
								//TODO make this better at leaving legit items alone
								//TODO possibly look for duplicates of the item in other stacks?
								mClearTopItem = null;
								for (int i = stack.Items.Count - 1; i >= 0; i--) {
										mClearTopItem = stack.Items[i];
										if (mClearTopItem == null) {
												stack.Items.RemoveAt(i);
										} else if (mClearTopItem.Is(WIMode.RemovedFromGame)) {
												//Debug.Log("Item was removed from game, removing");
												mClearTopItem.Clear();
												stack.Items.RemoveAt(i);
										} else if (mClearTopItem.Group != stack.Group) {
												//Debug.Log("Item group wasn't the same as stack group, removing");
												if (mClearTopItem.Group != null && stack.Group != null) {
														//Debug.Log("Group " + mClearTopItem.Group.name + " vs " + stack.Group.name);
												}
												stack.Items.RemoveAt(i);
										}
								}
								stack.Refresh();
						}

						public static void Items(WIStackEnabler enabler, bool destroyClearedItems)
						{
								if (enabler == null)
										return;

								if (enabler.HasEnablerContainer) {
										Clear.Items(enabler.EnablerContainer, destroyClearedItems);
								}
								if (enabler.HasEnablerStack) {
										Clear.Items(enabler.EnablerStack, destroyClearedItems);
								}
						}

						public static void Items(WIStackContainer container, bool destroyClearedItems)
						{
								if (container == null)
										return;

								for (int i = 0; i < container.StackList.Count; i++) {
										Clear.Items(container.StackList[i], destroyClearedItems);
								}
						}

						public static List <IWIBase> mItemsToRemove = new List <IWIBase>();

						public static void Items(WIStack stack, bool destroyClearedItems)
						{
								if (stack == null)
										return;

								mItemsToRemove.Clear();
								mItemsToRemove.AddRange(stack.Items);
								for (int i = 0; i < mItemsToRemove.Count; i++) {
										IWIBase item = mItemsToRemove[i];
										if (destroyClearedItems && item != null) {
												item.RemoveFromGame();
										}
								}
								mItemsToRemove.Clear();
								stack.Items.Clear();
						}
						//safely un-links an enabler from the top item in its enabler stack
						//without destroying the item
						public static void Enabler(WIStackEnabler enabler)
						{
								if (enabler.HasEnablerTopItem) {
										enabler.EnablerStack.Clear();
								}
						}
				}

				public static class Find
				{
						public static IWIBase gItemsOfTypeCheck;

						public static bool FirstItemByPrefabName (WIStack stack, string prefabName, bool searchStackContainers, out IWIBase item, out WIStack inStack)
						{
								item = null;
								inStack = null;
								for (int i = 0; i < stack.Items.Count; i++) {
										if (stack.Items[i] != null) {
												if (stack.Items[i].PrefabName.Equals(prefabName)) {
														item = stack.Items[i];
														inStack = stack;
														break;
												} else if (searchStackContainers && item.IsStackContainer) {
														return FirstItemByPrefabName (item.StackContainer, prefabName, searchStackContainers, out item, out inStack);
												}
										}
								}
								return item != null;
						}

						public static bool FirstItemByPrefabName (WIStackContainer stackContainer, string prefabName, bool searchStackContainers, out IWIBase item, out WIStack inStack)
						{
								item = null;
								inStack = null;
								for (int i = 0; i < stackContainer.StackList.Count; i++) {
										if (stackContainer.StackList[i] != null) {
												if (FirstItemByPrefabName (stackContainer.StackList[i], prefabName, searchStackContainers, out item, out inStack)) {
														break;
												}
										}
								}
								return item != null;
						}

						public static void ItemsOfType(WIStack stack, string scriptName, bool searchStackContainers, List <IWIBase> itemsOfType)
						{
								for (int i = 0; i < stack.Items.Count; i++) {
										gItemsOfTypeCheck = stack.Items[i];
										if (gItemsOfTypeCheck != null)
										if (gItemsOfTypeCheck.Is(scriptName)) {
												itemsOfType.Add(stack.Items[i]);
										}
										if (searchStackContainers && gItemsOfTypeCheck.IsStackContainer) {
												ItemsOfType(gItemsOfTypeCheck.StackContainer, scriptName, searchStackContainers, itemsOfType);
										}
								}
						}

						public static void ItemsOfType(WIStackContainer stackContainer, string scriptName, bool searchStackContainers, List <IWIBase> itemsOfType)
						{
								for (int i = 0; i < stackContainer.StackList.Count; i++) {
										ItemsOfType(stackContainer.StackList[i], scriptName, searchStackContainers, itemsOfType);
								}
						}

						public static bool Item(WIStackContainer inContainer, IWIBase item, out WIStack stack)
						{
								bool result = false;
								stack = null;
								foreach (WIStack stackContender in inContainer.StackList) {
										if (!stackContender.IsEmpty && stackContender.Items.Contains (item)) {
												stack = stackContender;
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool StackContainingItem(WIStackContainer inContainer, string keyword, out WIStack stack)
						{
								bool result = false;
								stack = null;
								foreach (WIStack stackContender in inContainer.StackList) {
										if (!stackContender.IsEmpty && stackContender.TopItem.PrefabName == keyword) {
												stack = stackContender;
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool FirstCompatibleStack(WIStackContainer inContainer, IWIBase newItem, ref WIStackError error, out WIStack stack)
						{
								bool result = false;
								stack = null;
								foreach (WIStack toStack in inContainer.StackList) {
										if (Stacks.Can.Add(newItem, toStack, ref error)) {
												stack = toStack;
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool FirstEmptyStack(WIStackContainer inContainer, out WIStack stack)
						{
								bool result = false;
								stack = null;
								foreach (WIStack stackContender in inContainer.StackList) {
										if (stackContender.IsEmpty) {
												stack = stackContender;
												result = true;
												break;
										}
								}
								return result;
						}

						public static bool FirstItem(WIStackContainer inContainer, out IWIBase firstItem)
						{
								bool result = false;
								firstItem	= null;
								foreach (WIStack stack in inContainer.StackList) {
										if (!stack.Disabled && stack.HasTopItem) {
												firstItem = stack.TopItem;
												result = true;
										}
								}
								return result;
						}

						public static bool FirstItem(string keyword, WIStackContainer inContainer, out IWIBase firstItem)
						{
								bool result = false;
								firstItem = null;
								foreach (WIStack stack in inContainer.StackList) {	//TODO make this a proper search with constraints
										if (!stack.Disabled && stack.HasTopItem && stack.TopItem.StackName.Contains(keyword)) {
												firstItem = stack.TopItem;
												result = true;
										}
								}
								return result;
						}

						public static void CurrencyValues(WIStack stack, IBank bank)
						{

						}

						public static float CurrencyValue(WIStack stack)
						{
								float value = 0;
								foreach (IWIBase item in stack.Items) {
										value += item.BaseCurrencyValue;
								}
								return value;
						}

						public static float CurrencyValue(WIStackContainer container)
						{
								float value = 0;
								foreach (WIStack stack in container.StackList) {
										value += CurrencyValue(stack);
								}
								return value;
						}
				}

				public static class Swap
				{
						public static bool Top(WIStack stack, IWIBase newTopItem, bool removeExistingFromGame)
						{
								bool result = false;
								if (stack.HasTopItem) {
										IWIBase oldTopItem = stack.TopItem;
										stack.Items[stack.Items.Count - 1] = newTopItem;
										if (removeExistingFromGame) {
												//set the top item's OnRemovedFromStack to null
												//we're here seeing it so we don't need to double up on calls
												oldTopItem.OnRemoveFromGroup = null;
												oldTopItem.OnRemoveFromStack = null;
												oldTopItem.RemoveFromGame();
										}
										result = true;
								}
								//set the callback on the new item
								newTopItem.OnRemoveFromStack = stack.OnItemRemoved;
								return result;
						}

						public static bool Stacks(WIStack stack1, WIStack stack2, ref WIStackError error)
						{		//TODO make this generally more efficient
								//should be able to cut 2 or 3 loops from this
								if (stack1.NumItems > stack2.SpaceLeft || stack2.NumItems > stack1.SpaceLeft) {
										return false;
								}

								//checking this way ensures that a large container with small items can still swap
								for (int i = 0; i < stack1.Items.Count; i++) {
								//foreach (IWIBase stack1Item in stack1.Items) {
										if (!Can.Add(stack1.Items [i], stack2, ref error)) {
												return false;
										}
								}
								for (int i = 0; i < stack2.Items.Count; i++) {
								//foreach (IWIBase stack2Item in stack2.Items) {
										if (!Can.Add(stack2.Items [i], stack1, ref error)) {
												return false;
										}
								}

								//OK, we know they'll both fit and we know all items are compatible
								//time to swap
								List <IWIBase> stack1Items = stack1.Items;
								List <IWIBase> stack2Items = stack2.Items;

								for (int i = 0; i < stack1Items.Count; i++) {	//set on removed from stack 1 to stack 2
										stack1Items[i].OnRemoveFromStack = stack2.OnItemRemoved;
								}
								for (int i = 0; i < stack2Items.Count; i++) {	//set on removed from stack 2 to stack 1
										stack2Items[i].OnRemoveFromStack = stack1.OnItemRemoved;
								}

								//don't clear the items since we don't want to nuke one anothers' lists
								stack1.SetItems(stack2Items, false);
								stack2.SetItems(stack1Items, false);

								stack1.Refresh();
								stack2.Refresh();

								return true;
						}
				}

				public static class Create
				{
						public static WIStack Stack(WIGroup group)
						{
								WIStack newStack = new WIStack();
								newStack.Group = group;
								newStack.Mode = WIStackMode.Generic;
								return newStack;
						}

						public static WIStack Stack(WIStackContainer container, WIGroup group)
						{
								WIStack newStack = new WIStack();
								newStack.Container = container;
								newStack.Group = group;
								newStack.Mode = WIStackMode.Generic;
								return newStack;
						}

						public static List <WIStack> Stacks(int numberOfStacks, WIGroup group)
						{
								return Create.Stacks(numberOfStacks, null, group);
						}

						public static List <WIStack> Stacks(int numberOfStacks, WIStackContainer container, WIGroup group)
						{
								List <WIStack> stackList = new List <WIStack>();
								for (int i = 0; i < numberOfStacks; i++) {
										stackList.Add(Create.Stack(container, group));
								}
								return stackList;
						}

						public static WIStackContainer StackContainer(IStackOwner owner, WIGroup group)
						{
								WIStackContainer newStackContainer = new WIStackContainer(owner);
								newStackContainer.Group = group;
								newStackContainer.SetStackList(Create.Stacks(Globals.MaxStacksPerContainer, newStackContainer, group));
								return newStackContainer;
						}

						public static List <WIStackContainer> StackContainers(int numberOfStackContainers, IStackOwner owner, WIGroup group)
						{
								List <WIStackContainer> containersList = new List <WIStackContainer>();
								for (int i = 0; i < numberOfStackContainers; i++) {
										containersList.Add(Create.StackContainer(owner, group));
								}
								return containersList;
						}

						public static WIStackEnabler StackEnabler(WIStack enablerStack, WIGroup group)
						{
								WIStackEnabler enabler = new WIStackEnabler();
								enabler.EnablerStack = enablerStack;
								enabler.Group = group;
								enabler.Initialize();
								return enabler;
						}

						public static WIStackEnabler StackEnabler(WIStack enablerStack)
						{
								WIStackEnabler enabler = new WIStackEnabler();
								enabler.EnablerStack = enablerStack;
								enabler.Initialize();
								return enabler;
						}

						public static WIStackEnabler StackEnabler(WIGroup group)
						{
								WIStackEnabler enabler = new WIStackEnabler();
								//automatically create the enabler stack
								//this can only be set once
								enabler.EnablerStack = Create.Stack(group);
								enabler.Initialize();
								return enabler;
						}

						public static List <WIStackEnabler> StackEnablers(int numberOfStackEnablers, WIGroup group)
						{
								List <WIStackEnabler> enablerList = new List<WIStackEnabler>();
								for (int i = 0; i < numberOfStackEnablers; i++) {
										enablerList.Add(StackEnabler(group));
								}
								return enablerList;
						}

						public static List <WIStackEnabler> StackEnablers(List <WIStack> stackEnablers, WIGroup group)
						{
								List <WIStackEnabler> enablerList = new List<WIStackEnabler>();
								for (int i = 0; i < stackEnablers.Count; i++) {
										enablerList.Add(StackEnabler(stackEnablers[i], group));
								}
								return enablerList;
						}

						public static List <WIStackEnabler> StackEnablers(List <WIStack> stackEnablers)
						{
								List <WIStackEnabler> enablerList = new List<WIStackEnabler>();
								for (int i = 0; i < stackEnablers.Count; i++) {
										enablerList.Add(StackEnabler(stackEnablers[i]));
								}
								return enablerList;
						}
				}

				public static class Convert
				{
						public static bool TopItemToWorldItem(WIStack stack, out WorldItem newTopItem)
						{
								//worst case scenario, the top item is a sack or something, and is being displayed in an enabler
								//so swapping it out could make its items appear to vanish if we're not careful
								IWIBase topItem = null;
								newTopItem = null;
								bool result = false;
								if (stack.HasTopItem) {
										topItem = stack.TopItem;
										if (!topItem.IsWorldItem) {
												if (WorldItems.CloneFromStackItem(topItem.GetStackItem(WIMode.Stacked), stack.Group, out newTopItem)) {
														//set a few of the new top item's props so it behaves as expected
														newTopItem.Props.Local.FreezeOnStartup = false;
														newTopItem.Props.Local.Mode = WIMode.Frozen;
														newTopItem.Props.Local.PreviousMode = WIMode.Stacked;
														//now swap the top
														if (Swap.Top(stack, newTopItem, true)) {//if it's not a world item
																//and we're able to create a world item from the top item
																//and we're able to set the top item to the new world item
																result = true;
																stack.Refresh();
														} else {//TODO
																//clean up created worlditem
														}
												}
										} else {//nothing to do here, wuhoo!
												newTopItem = topItem.worlditem;
												result = true;
										}
								}

								return result;
						}

						public static bool TopItemToStackItem(WIStack stack, out StackItem newTopItem)
						{
								IWIBase topItem = null;
								newTopItem = null;
								bool result = false;
								if (stack.HasTopItem) {
										topItem = stack.TopItem;
										if (topItem.IsWorldItem) {
												if (topItem.worlditem.UnloadWhenStacked) {
														topItem.OnRemoveFromStack = null;
														newTopItem = topItem.GetStackItem(WIMode.Unloaded);
														Stacks.Swap.Top(stack, newTopItem, true);
														result = true;
												} else {
														result = true;
												}
										} else {
												result = true;
										}
								}

								return result;
						}

						public static void UnloadedItemsToWorldItems(WIStack stack)
						{
								for (int i = 0; i < stack.Items.Count; i++) {
										if (stack.Items[i] != null && !stack.Items[i].UnloadWhenStacked && !stack.Items[i].IsWorldItem) {
												WorldItem newWorldItem = null;
												if (WorldItems.CloneFromStackItem(stack.Items[i].GetStackItem(WIMode.Stacked), stack.Group, out newWorldItem)) {
														stack.Items[i] = newWorldItem;
												}
										}
								}
								stack.Refresh();
						}
				}

				public static class Destroy
				{
						public static void StackContainer(WIStackContainer stackContainer, bool destroyItems)
						{
								//TODO look for potential pitfalls - I *think* we can just nuke it and be OK
								if (stackContainer != null) {
										//call stack container destroyed before destroying stacks
										stackContainer.OnDestroyed();
										foreach (WIStack stack in stackContainer.StackList) {
												Destroy.Stack(stack, destroyItems);
										}
										stackContainer.StackList.Clear();
								}
						}

						public static void Stack(WIStack stack, bool destroyItems)
						{
								if (stack != null) {
										//call destroyed before clearing items
										stack.OnDestroyed();
										//TODO make sure we can just clear items like this
										if (destroyItems) {
												foreach (IWIBase item in stack.Items) {
														WorldItems.RemoveItemFromGame(item);
												}
										}
										stack.Items.Clear();
								}
						}
				}

				public static class Copy
				{
						//this copies the state of one stack container to another
						//this is done to preserve references to the existing container while updating its state
						//usually used for network updates
						public static void StackContainer(WIStackContainer fromContainer, WIStackContainer toContainer)
						{	//we're *assuming* that the items contained in the toContainer can be replaced outright
								//and don't need to be destroyed
								//this may cause problems with references to existing stackItems...
								if (fromContainer == null)//destroy toContainer?
					return;

								if (toContainer == null) {
										toContainer = fromContainer;//if it's null just copy the reference
										return;
								}
								//copy the contents of the containers
								for (int i = 0; i < Globals.MaxStacksPerContainer; i++) {
										Copy.Stack(fromContainer.StackList[i], toContainer.StackList[i]);
								}
						}

						public static void Stack(WIStack fromStack, WIStack toStack)
						{
								toStack.Items.Clear();//yikes this freaks me out
								toStack.Items.AddRange(fromStack.Items);
								//okay that should be it (?)
								toStack.Refresh();
						}
				}

				public static class Display
				{
						public static bool ItemInEnabler(IWIBase item, WIStackEnabler enabler)
						{	//this places an item in an enabler for display purposes
								//the item is not moved from its current stack
								//use sparingly
								if (enabler.HasEnablerStack) {
										if (enabler.HasEnablerTopItem) {
												//just set the top item to the new item
												enabler.EnablerStack.Items[0] = item;
										} else {
												//add this item to the items list
												enabler.EnablerStack.Items.Add(item);
										}
										//tell the enabler to refresh it's owners
										enabler.Refresh();
								}
								//no stack? no display
								return false;
						}

						public static WIStack ItemInContainer(IWIBase itemToHold, WIStackContainer enablerContainer)
						{
								WIStack stack = null;
								//this places an item in an enabler for display purposes
								//the item is not moved from its current stack
								//use sparingly
								if (enablerContainer.StackList.Count > 0) {
										stack = enablerContainer.StackList.First();
										stack.Items.Add(itemToHold);
								}
								return stack;
						}

						public static WIStack ItemsInContainer(WIStack itemsToHold, WIStackContainer enablerContainer)
						{
								WIStack stack = null;
								//this places an item in an enabler for display purposes
								//the item is not moved from its current stack
								//use sparingly
								if (enablerContainer.StackList.Count > 0) {
										stack = enablerContainer.StackList.First();
										stack.Items.AddRange(itemsToHold.Items);
								}
								return stack;
						}
				}

				public static void SendMessageToItems(WIStackContainer container, string message)
				{
						//TODO uh, everything I guess
				}

				public static WISize SmallerSize(WISize size1, WISize size2)
				{	//convenience function instead of casting to (int)
						if (size1 == size2) {
								return size1;
						}

						WISize smallerSize = size2;
						switch (size1) {
								case WISize.Huge:
										break;

								case WISize.Large:
										switch (size2) {
												case WISize.Huge:
														smallerSize = size1;
														break;
										}
										break;

								case WISize.Medium:
										switch (size2) {
												case WISize.Huge:
												case WISize.Large:
														smallerSize = size1;
														break;
										}
										break;

								case WISize.Small:
										switch (size2) {
												case WISize.Huge:
												case WISize.Large:
												case WISize.Medium:
														smallerSize = size1;
														break;
										}
										break;

								case WISize.Tiny:
										switch (size2) {
												case WISize.Huge:
												case WISize.Large:
												case WISize.Medium:
												case WISize.Small:
														smallerSize = size1;
														break;
										}
										break;
						}
						return smallerSize;
				}
		}
}
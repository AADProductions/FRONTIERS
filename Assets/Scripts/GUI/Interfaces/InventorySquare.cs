using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.GUI
{
		public class InventorySquare : InventorySquareDisplay
		{
				public WIStackEnabler Enabler;
				public Action FinishUsingSkillToRemoveItem;
				public bool RequiresEnabler = true;
				//making this opt-in instead of figuring it out on the fly
				public bool AllowShiftClick = false;
				public override bool CanSplitStack {
						get {
								return IsEnabled && mStack != null && mStack.NumItems > 1;
						}
				}

				public bool HasEnabler {
						get {
								return Enabler != null;
						}
				}

				public virtual bool IsEnabled {
						get {
								if (!RequiresEnabler) {
										return true;
								}

								if (HasEnabler && Enabler.IsEnabled) {
										if (HasStack) {
												return !mStack.Disabled;
										} else {
												return true;
										}
								}
								return false;
						}
				}

				public WIStack Stack {
						get {
								return mStack;
						}
				}

				public bool HasStack {
						get {
								return mStack != null && !mStack.Disabled;
						}
				}

				public override void OnEnable()
				{
						base.OnEnable();
						mHover = false;
						mMouseOverUpdate = true;
				}

				public virtual void OnDrag()
				{
						if (Player.Local.Inventory.SelectedStack.NumItems > 0) {
								//we can't drag with a full stack in hand
								return;
						}
						OnClickSquare();
						mHover = false;
				}

				public virtual void OnDrop()
				{
						OnClickSquare();
				}

				protected virtual void OnRightClickSquare()
				{
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
								usable.TryToSpawn(true, out mUsable);
								usable.ScreenTarget = transform;
								usable.ScreenTargetCamera = NGUICamera;
								usable.RequirePlayerFocus = false;
								//the end result *should* affect the new item
						}
				}

				public virtual void OnClickSquare()
				{
						if (!IsEnabled || !HasStack) {
								return;
						}
						if (InterfaceActionManager.LastMouseClick == 1) {
								OnRightClickSquare();
								return;
						}

						WIStackError error = WIStackError.None;
						bool playSound = false;
						bool splitStack = false;
						bool quickAdd = false;
						string soundName = "InventoryPlaceStack";
						//skill usage
						bool useSkillToRemove = false;
						//left clicking can pick up, split, or quick-add
						if (InterfaceActionManager.Get.IsKeyDown (InterfaceActionType.StackSplit)) {
								splitStack = true;
						}/* else if (AllowShiftClick && InterfaceActionManager.Get.IsKeyDown (InterfaceActionType.StackQuickAdd)) {
								quickAdd = true;
						}*/

						mRemoveItemSkillNames.Clear();

						if (mStack.HasOwner(out mSkillUseTarget)) {
								//SKILL USE
								if (mSkillUseTarget.UseRemoveItemSkill(mRemoveItemSkillNames, ref mSkillUseTarget)) {
										useSkillToRemove = true;
										quickAdd = false;
								}
						}

						WIStack selectedStack = Player.Local.Inventory.SelectedStack;

						if (mStack.HasTopItem) {
								//if our stack has items
								if (selectedStack.HasTopItem) {
										//and the selected stack ALSO has items
										//------Special cases------//
										//LIQUID CONTAINER CHECK - can we put the thing we're holding into the thing we've clicked?
										if (FillLiquidContainer(mStack.TopItem, selectedStack.TopItem, selectedStack)) {
												playSound = true;
												soundName = "FillLiquidContainer";
										} else if (Stacks.Can.Stack(mStack.TopItem, selectedStack.TopItem)) {
												//if the selected stack's items can stack with our stack's items,
												//then we're putting items IN the stack
												//this is only allowed if the stack isn't owned
												if (useSkillToRemove) {
														//play an error and get out
														MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, SoundNameFailure);
														return;
												} else {
														//otherwise try to add the items normally
														playSound = Stacks.Add.Items(selectedStack, mStack, ref error);
												}
										} else {
												//if the selected stack's items can't be stacked
												//then we're swapping the stack
												//ie, we're removing items FROM the stack
												//this is only allowed if the stack isn't owned
												if (useSkillToRemove) {
														//so play an error and get out
														MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, SoundNameFailure);
														return;
												} else {
														//otherwise try to swap stacks normally
														playSound = Stacks.Swap.Stacks(mStack, selectedStack, ref error);
												}
										}
								} else {
										//----SHIFT CLICK CHECK----//
										//check for shift click - this bypasses everything and adds the item to the player's inventory
										//this only works if the stack does not already belong to the player's group
										if (quickAdd) {
												//Debug.Log ("Starting quick add..");
												if (Player.Local.Inventory.CanItemFit(mStack.TopItem)) {
														if (Player.Local.Inventory.QuickAddItems(mStack, ref error)) {
																soundName = "InventoryPickUpStack";
																playSound = true;
														}
												} else {
														GUIManager.PostWarning(mStack.TopItem.DisplayName + " won't fit in your inventory");
												}
										} else {
												//if the selected stack does NOT have any items
												//we're putting items IN the stack
												if (splitStack && mStack.NumItems > 1) {
														//we're splitting the stack and adding the contents to the selected stack
														//this is only allowed if the stack isn't owned
														if (useSkillToRemove) { 
																//play an error and get out
																MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, SoundNameFailure);
																return;
														}
														int numToAdd = mStack.NumItems / 2;
														bool addResult = true;
														//IWIBase topItem = null;
														for (int i = 0; i < numToAdd; i++) {
																if (!Stacks.Pop.AndPush(mStack, selectedStack, ref error)) {
																		addResult = false;
																		break;
																}
														}
														playSound = addResult;
												} else {
														//if we're not splitting stacks
														//THIS is where we finally REMOVE items from this stack using a skill
														if (useSkillToRemove) {
																UseSkillsToRemoveStack();
																return;
														} else if (Stacks.Add.Items(mStack, selectedStack, ref error)) {
																playSound = true;
																soundName = "InventoryPickUpStack";
														}
												}
										}
								}
						} else if (selectedStack.HasTopItem) {
								//if our stack does NOT have items
								//we're adding items TO our stack
								//this is only allowed if the stack isn't owned
								if (useSkillToRemove) {
										//so play an error and get out
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, SoundNameFailure);
										return;
								} else if (Stacks.Add.Items(selectedStack, mStack, ref error)) {
										//otherwise try to add the items and get out
										playSound = true;
								}
						}

						if (error != WIStackError.None) {
								GUIManager.PostStackError(error);
						}

						if (playSound) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, soundName);
						}

						UpdateDisplay();
				}

				public bool FillLiquidContainer(IWIBase liquidContainer, IWIBase foodStuff, WIStack foodStuffStack)
				{
						if (!foodStuff.Is <FoodStuff>()) {
								//nope! can't do it
								return false;
						}

						System.Object fssObject = null;
						//check the foodstuff first since it has to be liquid
						if (foodStuff.GetStateOf <FoodStuff>(out fssObject)) {
								FoodStuffState fss = (FoodStuffState)fssObject;
								if (fss.IsLiquid(foodStuff.State)) {
										//okay we've confirmed it's a liquid
										//now see if the liquid container can hold it
										if (!liquidContainer.Is <LiquidContainer>()) {
												//we're returning true because it'll make the correct noise and the item will disappear
												GUIManager.PostWarning("The liquid cannot be held by this container. It seeps away.");
												foodStuff.RemoveFromGame();
												return true;
										}

										System.Object lcObject = null;
										if (liquidContainer.GetStateOf <LiquidContainer>(out lcObject)) {
												LiquidContainerState lcs = (LiquidContainerState)lcObject;
												string liquidFillError = string.Empty;
												int numFilled = 0;
												GenericWorldItem genericLiquid = new GenericWorldItem(foodStuff);
												if (lcs.TryToFillWith(genericLiquid, foodStuffStack.NumItems, out numFilled, out liquidFillError)) {
														//hooray, it worked
														//how we have to pop items off the top of the food stuff stack
														for (int i = 0; i < numFilled; i++) {
																Stacks.Pop.AndToss(foodStuffStack);
														}
														GUIManager.PostInfo("Filled " + numFilled.ToString());
														return true;
												} else {
														GUIManager.PostWarning(liquidFillError);
												}
										}
								}
						}
						return false;
				}

				public void SwapStackWithSelectedStack(WIStack selectedStack)
				{
						WIStackError error = WIStackError.None;
						if (Stacks.Add.Items(selectedStack, mStack, ref error)) {
								UpdateDisplay();
						}
				}

				public virtual void DropStack()
				{
						if (HasStack) {
								mStack.RefreshAction -= mRefreshRequest;
						}
						mStack = null;
						RefreshRequest();
				}

				public virtual void SetStack(WIStack stack)
				{
						if (HasStack) {
								if (stack != null) {
										if (stack != mStack) {
												mStack.RefreshAction -= mRefreshRequest;
												mStack = stack;
												mStack.RefreshAction += mRefreshRequest;
												//this will automatically refresh the display
												mStack.Refresh();
										}
								} else {
										mStack.RefreshAction -= mRefreshRequest;
										mStack = null;
										//update the display to reflect the dead stack
										RefreshRequest();
								}
						} else if (stack != null) {
								mStack = stack;
								mStack.RefreshAction += mRefreshRequest;
								//this will automatically refresh the display
								mStack.Refresh();
						}
				}

				public virtual void SetProperties()
				{
						ShowDoppleganger = false;
						DopplegangerMode = WIMode.Stacked;
						MouseoverHover = false;
						DisplayMode = SquareDisplayMode.Disabled;

						if (IsEnabled && HasStack) {
								DisplayMode = SquareDisplayMode.Empty;
								if (mStack.HasTopItem) {
										//TEMP
										DisplayMode = SquareDisplayMode.Enabled;
										IWIBase topItem = mStack.TopItem;
										ShowDoppleganger = true;
										DopplegangerProps.CopyFrom(topItem);
								}
								MouseoverHover = true;
						} else {
								DisplayMode = SquareDisplayMode.Disabled;
						}
				}

				public override void UpdateDisplay()
				{
						SetQuestItem();
						SetProperties();
						SetInventoryItemName();
						SetWeightLabel();
						SetShadow();
						UpdateDoppleganger();
						UpdateMouseoverHover();
						SetInventoryStackNumber();
				}

				public virtual void SetInventoryItemName()
				{
						if (InventoryItemName != null) {
								InventoryItemName.enabled = false;
						}
				}

				public virtual void SetQuestItem () {
					if (HasStack && mStack.HasTopItem && mStack.TopItem.IsQuestItem) {
						QuestItemHighlight.enabled = true;
						QuestItemHighlight.color = Colors.Get.MessageInfoColor;
					} else {
						QuestItemHighlight.enabled = false;
					}
				}

				public virtual void SetInventoryStackNumber()
				{
						string stackNumberLabelText = string.Empty;
						if (IsEnabled && HasStack && mStack.NumItems > 1) {
								if (mHover) {
										stackNumberLabelText = mStack.NumItems.ToString() + "/" + Colors.ColorWrap(mStack.MaxItems.ToString(), Colors.Darken(StackNumberLabel.color));
								} else {
										stackNumberLabelText = mStack.NumItems.ToString();
								}
						}
						StackNumberLabel.text = stackNumberLabelText;
						//make sure the doppleganger isn't covering us
						if (Doppleganger != null) {
								Vector3 labelPosition = StackNumberLabel.transform.position;
								if (Doppleganger.renderer != null) {
										labelPosition.z = Mathf.Min(labelPosition.z, -(Doppleganger.renderer.bounds.extents.z + 0.1f));
								}
								StackNumberLabel.transform.position = labelPosition;
						}
				}

				public virtual void SetWeightLabel()
				{
						if (WeightLabel != null) {
								WeightLabel.enabled = false;
						}
				}

				public void OnSelectRemoveSkill(System.Object result)
				{
						UsingMenu = false;

						WIListResult dialogResult = result as WIListResult;
						RemoveItemSkill skillToUse = null;
						foreach (Skill removeSkill in mRemoveSkillList) {
								if (removeSkill.name == dialogResult.Result) {
										skillToUse = removeSkill as RemoveItemSkill;
										break;
								}
						}

						if (skillToUse != null) {
								//set this global flag to true
								//this will prevent anything from closing
								RemovingItemUsingSkill = true;
								FinishUsingSkillToRemoveItem += OnFinishUsingSkillToRemoveItem;
								//SKILL USE
								//getting here guarantees that:
								//a) our selected stack is empty and
								//b) our stack has item
								//so proceed as though we know those are true
								skillToUse.TryToRemoveItem(mSkillUseTarget, mStack, Player.Local.Inventory.SelectedStack, WIGroups.Get.Player, FinishUsingSkillToRemoveItem);
								//now we just have to wait!
								//the skill will move stuff around
								//refresh requests will be automatic
						}

						mRemoveItemSkillNames.Clear();
				}

				public void OnFinishUsingSkillToRemoveItem()
				{
						FinishUsingSkillToRemoveItem = null;
						RemovingItemUsingSkill = false;
						UsingMenu = false;
				}

				protected void UseSkillsToRemoveStack()
				{	//this whole method is very brittle
						//look into ways to keep this from blowing things up
						if (UsingMenu) {
								return;
						}

						//add the option list we'll use to select the skill
						SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
						optionsList.MessageType = string.Empty;//"Take " + mSkillUseTarget.DisplayName;
						optionsList.Message = "Use a skill to take";
						optionsList.FunctionName = "OnSelectRemoveSkill";
						optionsList.RequireManualEnable = false;
						optionsList.OverrideBaseAvailabilty = true;
						optionsList.FunctionTarget = gameObject;
						mRemoveSkillList.Clear();
						mRemoveSkillList.AddRange(Skills.Get.SkillsByName(mRemoveItemSkillNames));
						foreach (Skill removeItemSkill in mRemoveSkillList) {
								if (mSkillUseTarget.IsWorldItem && mSkillUseTarget.worlditem != null) {
										optionsList.AddOption(removeItemSkill.GetListOption(mSkillUseTarget.worlditem));
								} else {
										//why are we here??
										Debug.LogError("Remove item skill worlditem was NULL or wasn't world item");
								}
						}
						optionsList.AddOption(new WIListOption("Cancel"));
						optionsList.ShowDoppleganger = false;
						GUIOptionListDialog dialog = null;
						if (optionsList.TryToSpawn(true, out dialog)) {
								UsingMenu = true;
								optionsList.ScreenTarget = transform;
								optionsList.ScreenTargetCamera = NGUICamera;
						}
				}

				public override void OnDestroy()
				{
						DropStack();
						base.OnDestroy();
				}
				//these are static because there can only be one menu open
				//at any time in the inventory
				protected static GUIOptionListDialog mUsable = null;
				public static bool RemovingItemUsingSkill = false;
				public static bool UsingMenu = false;
				protected WIStack mStack = null;
				//skill usage
				protected HashSet <string> mRemoveItemSkillNames = new HashSet <string>();
				protected List <Skill> mRemoveSkillList = new List <Skill>();
				protected IStackOwner mSkillUseTarget = null;
		}
}
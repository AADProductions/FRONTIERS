using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

namespace Frontiers.World.Gameplay
{		//used as base class for steal, clean animal, barter, etc.
		//intercepts the request to remove a thing
		//and forces you to pass some kind of skill test before it delivers
		public class RemoveItemSkill : Skill
		{
				public RemoveItemSkillExtensions Extensions = new RemoveItemSkillExtensions();

				public bool MoveItemToInventory {
						get {
								return LastInventory != null;
						}
				}

				public IInventory LastInventory;
				public IWIBase LastItemToMove;
				public WIStack LastFromStack;
				public WIStack LastToStack;
				public WIGroup LastToGroup;
				public Action LastCallback;

				public virtual void TryToRemoveItem(IStackOwner skillUseTarget, WIStack fromStack, WIStack toStack, WIGroup toGroup, Action callBack)
				{
						LastInventory = null;
						LastFromStack = fromStack;
						LastToStack = toStack;
						LastToGroup = toGroup;
						LastCallback = callBack;
						//make sure the item we're moving isn't immune to our skill
						if (!CheckForSkillImmunity(fromStack.TopItem, this, out mImmunityMessage)) {
								GUIManager.PostDanger(mImmunityMessage);
								FailImmediately();
								return;
						}
						//use the skill against the target
						Use(skillUseTarget.worlditem, 0);
				}

				public virtual void TryToRemoveItem(IStackOwner skillUseTarget, IWIBase itemToMove, IInventory toInventory, Action callBack)
				{
						LastFromStack = null;
						LastToStack = null;
						LastItemToMove = itemToMove;
						LastInventory = toInventory;
						LastCallback = callBack;
						if (!CheckForSkillImmunity(itemToMove, this, out mImmunityMessage)) {
								GUIManager.PostDanger(mImmunityMessage);
								FailImmediately();
								return;
						}
						//use the skill against the target
						Use(skillUseTarget.worlditem, 0);
				}

				protected override void OnUseFinish()
				{
						if (ProgressCanceled) {
								LastCallback.SafeInvoke();
								return;
						}

						if (LastUseImmune) {
								//don't do anything if skill immunity was used
								return;
						}

						bool removedItem = false;
						bool attachScript = false;
						IWIBase finalItem = null;
						string scriptToAttach = string.Empty;

						if (State.HasBeenMastered && !string.IsNullOrEmpty(Extensions.AttachScriptOnUseUnmastered)) {
								scriptToAttach = Extensions.AttachScriptOnUseMastered;
						} else if (!string.IsNullOrEmpty(Extensions.AttachScriptOnUseUnmastered)) {
								scriptToAttach = Extensions.AttachScriptOnUseUnmastered;
						}

						WIStackError error = WIStackError.None;
						if (LastSkillResult) {
								attachScript = Extensions.AttachScriptOnSuccess;
								//we either add it based on inventory or else stacks
								if (MoveItemToInventory) {
										//Debug.Log("Adding to inventory, we were successful");
										//convert it to a stack item BEFORE pushing it so we know we'll have the actual copy
										finalItem = LastItemToMove.GetStackItem(WIMode.Unloaded);
										StartCoroutine(LastInventory.AddItem(finalItem));
										removedItem = true;
								} else {
										finalItem = LastFromStack.TopItem;
										removedItem = (Stacks.Pop.AndPush(LastFromStack, LastToStack, ref error));
								}

								if (State.HasBeenMastered) {
										GUIManager.PostSuccess(Extensions.GUIMessageOnSuccessMastered);
								} else {
										GUIManager.PostSuccess(Extensions.GUIMessageOnSuccessUnmastered);
								}
						} else {
								attachScript = Extensions.AttachScriptOnFail;
								if (MoveItemToInventory) {
										//we're moving things into our inventory
										if (Extensions.DestroyItemOnFail) {
												//Debug.Log("Destroying item on failure");
												LastItemToMove.RemoveFromGame();
												removedItem = true;
												//don't attach a script because there's nothing to attach it to
										} else if (Extensions.SubstituteItemOnFail) {
												//Debug.Log("Adding substitution instead");
												LastItemToMove.RemoveFromGame();
												finalItem = Extensions.Substitution.ToStackItem();
												StartCoroutine(LastInventory.AddItem(finalItem));
												removedItem = true;
										} else if (Extensions.MoveItemOnFail) {
												//Debug.Log("Failed, but still moving item - item null? " + (LastItemToMove == null).ToString());
												finalItem = LastItemToMove.GetStackItem(WIMode.Unloaded);
												StartCoroutine(LastInventory.AddItem(finalItem));
												removedItem = true;
										}
								} else {
										//we're moving things from stack to stack
										if (Extensions.DestroyItemOnFail) {
												//just get rid of it
												Stacks.Pop.AndToss(LastFromStack);
												removedItem = true;
										} else if (Extensions.SubstituteItemOnFail) {
												//just get rid of it
												Stacks.Pop.AndToss(LastFromStack);
												removedItem = true;
												//then put substitute item in other stack
												//only attach a script if we actually push the item
												finalItem = Extensions.Substitution.ToStackItem();
												removedItem = Stacks.Push.Item(LastToStack, finalItem, ref error);
										} else if (Extensions.MoveItemOnFail) {
												finalItem = LastFromStack.TopItem;
												removedItem = Stacks.Pop.AndPush(LastFromStack, LastToStack, ref error);
										}
								}

								if (Extensions.UnskilledRepPenaltyOnFail > 0 || Extensions.SkilledRepPenaltyOnFail > 0) {
										//only do this if we CAN suffer a rep loss
										int globalRepLoss = 0;
										int ownerRepLoss = 0;
										if (State.HasBeenMastered) {
												globalRepLoss = Mathf.Max(1, Mathf.FloorToInt(
														(finalItem.BaseCurrencyValue * Globals.BaseCurrencyToReputationMultiplier) *
														Mathf.Lerp(Extensions.UnskilledRepPenaltyOnFail, Extensions.SkilledRepPenaltyOnFail, State.NormalizedMasteryLevel) *
														Extensions.MasterRepPenaltyOnFail));
												ownerRepLoss = Mathf.Max(1, Mathf.FloorToInt(
														(finalItem.BaseCurrencyValue * Globals.BaseCurrencyToReputationMultiplier) *
														Mathf.Lerp(Extensions.UnskilledOwnerRepPenaltyOnFail, Extensions.SkilledOwnerRepPenaltyOnFail, State.NormalizedMasteryLevel) *
														Extensions.MasterOwnerRepPenaltyOnFail));
												GUIManager.PostDanger(Extensions.GUIMessageOnFailureMastered);
										} else {
												globalRepLoss = Mathf.Max(1, Mathf.FloorToInt(
														(finalItem.BaseCurrencyValue * Globals.BaseCurrencyToReputationMultiplier) *
														Mathf.Lerp(Extensions.UnskilledRepPenaltyOnFail, Extensions.SkilledRepPenaltyOnFail, State.NormalizedMasteryLevel)));
												ownerRepLoss = Mathf.Max(1, Mathf.FloorToInt(
														(finalItem.BaseCurrencyValue * Globals.BaseCurrencyToReputationMultiplier) *
														Mathf.Lerp(Extensions.UnskilledOwnerRepPenaltyOnFail, Extensions.SkilledOwnerRepPenaltyOnFail, State.NormalizedMasteryLevel)));
												GUIManager.PostDanger(Extensions.GUIMessageOnFailureUnmastered);
										}
										Profile.Get.CurrentGame.Character.Rep.LoseGlobalReputation(globalRepLoss);
										//see if we've just stolen from a character
										Character character = null;
										if (LastSkillTarget != null && LastSkillTarget.IOIType == ItemOfInterestType.WorldItem && LastSkillTarget.worlditem.Is <Character>(out character)) {
												Profile.Get.CurrentGame.Character.Rep.LosePersonalReputation(character.worlditem.FileName, character.worlditem.DisplayName, ownerRepLoss);
										}
								}
						}

						if (attachScript && finalItem != null && !string.IsNullOrEmpty(scriptToAttach)) {
								//Debug.Log("Attaching script " + scriptToAttach + " to final item");
								finalItem.Add(scriptToAttach);
						}

						if (removedItem) {
								if (LastSkillResult) {
										MasterAudio.PlaySound(Extensions.SoundTypeOnSuccess, Extensions.SoundOnSuccess);
								} else {
										MasterAudio.PlaySound(Extensions.SoundTypeOnFailure, Extensions.SoundOnFailure);
								}
						}

						LastCallback.SafeInvoke();
				}
		}

		[Serializable]
		public class RemoveItemSkillExtensions
		{
				public int UnskilledRepPenaltyOnFail;
				public int SkilledRepPenaltyOnFail;
				public float MasterRepPenaltyOnFail;
				public int UnskilledOwnerRepPenaltyOnFail;
				public int SkilledOwnerRepPenaltyOnFail;
				public float MasterOwnerRepPenaltyOnFail;
				public string AttachScriptOnUseUnmastered;
				public string AttachScriptOnUseMastered;
				public string GUIMessageOnSuccessUnmastered;
				public string GUIMessageOnFailureUnmastered;
				public string GUIMessageOnSuccessMastered;
				public string GUIMessageOnFailureMastered;
				public float MaxNormaliedReputationDifference = 0.5f;
				public MasterAudio.SoundType SoundTypeOnSuccess = MasterAudio.SoundType.PlayerInterface;
				public string SoundOnSuccess = "InventoryPickUpStack";
				public MasterAudio.SoundType SoundTypeOnFailure = MasterAudio.SoundType.PlayerInterface;
				public string SoundOnFailure = "ButtonClickDisabled";
				public bool DestroyItemOnFail = false;
				public bool SubstituteItemOnFail = false;
				public bool MoveItemOnFail = false;
				public bool AttachScriptOnFail = false;
				public bool AttachScriptOnSuccess = false;
				public GenericWorldItem Substitution = new GenericWorldItem();
		}
}
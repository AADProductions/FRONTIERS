using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIBarter : GUIEditor <BarterSession>
		{
				public Vector3 ApprovedTradeCharacterToPlayerArrow;
				public Vector3 ApprovedTradePlayerToCharacterArrow;
				public Vector3 UnapprovedTradeCharacterToPlayerArrow;
				public Vector3 UnapprovedTradePlayerToCharacterArrow;
				public Vector3 CharacterToPlayerArrowTarget;
				public Vector3 PlayerToCharacterArrowTarget;
				public Vector3 PlayerToCharacterArrowBump;
				public Vector3 CharacterToPlayerArrowBump;
				public float CharacterToPlayerArrowAlphaTarget;
				public float PlayerToCharacterArrowAlphaTarget;
				public float PlayerToPlayerArrowAlphaTarget;
				public float CharacterToCharacterArrowAlphaTarget;
				public float ApprovedTradeCharacterLabelAlphaTarget;
				public UISprite CharacterToPlayerArrow;
				public UISprite PlayerToCharacterArrow;
				public UISprite PlayerToPlayerArrow;
				public UISprite CharacterToCharacterArrow;
				public UILabel CharacterNameLabel;
				public UILabel PlayerNameLabel;
				public UIButton ApproveTradeButtonPlayer;
				public UILabel ApproveTradeLabelPlayer;
				public UILabel ApproveTradeLabelCharacter;
				public UILabel BaseValuePlayerGoodsLabel;
				public UILabel BaseValueCharacterGoodsLabel;
				public UILabel TotalValueBuyLabel;
				public UILabel TotalValueSellLabel;
				//rep (debug)
				public UILabel SkillPenaltyLabel;
				public UILabel RepPenaltyLabel;
				public UILabel TotalPenaltyLabel;
				//display parents
				public GameObject PlayerInventoryDisplayParent;
				public GameObject CharacterInventoryDisplayParent;
				public GameObject PlayerInventoryBankDisplayParent;
				public GameObject CharacterInventoryBankDisplayParent;
				public GameObject PlayerGoodsDisplayParent;
				public GameObject CharacterGoodsDisplayParent;
				public GameObject PlayerGoodsBankDisplayParent;
				public GameObject CharacterGoodsBankDisplayParent;
				//displays
				public GUIBarterContainerDisplay PlayerInventoryDisplay;
				public GUIBarterContainerDisplay CharacterInventoryDisplay;
				public GUIBarterContainerDisplay PlayerGoodsDisplay;
				public GUIBarterContainerDisplay CharacterGoodsDisplay;
				public GUIBank PlayerInventoryBank;
				public GUIBank CharacterInventoryBank;
				public GUIBank PlayerGoodsBank;
				public GUIBank CharacterGoodsBank;

				public override void PushEditObjectToNGUIObject()
				{
						if (!mPushingObjectToNGUIObjectOverTime) {
								mPushingObjectToNGUIObjectOverTime = true;
								mEditObject.RefreshAction += mRefreshBarterGUIAction;
								StartCoroutine(PushEditObjectToNGUIObjectOverTime());
						}
				}

				protected bool mPushingObjectToNGUIObjectOverTime = false;

				public override void WakeUp()
				{
						base.WakeUp();
						//create stack container displays
						GameObject PlayerInventoryDisplayObject = NGUITools.AddChild(PlayerInventoryDisplayParent, GUIManager.Get.BarterContainerDisplay);
						GameObject CharacterInventoryDisplayObject = NGUITools.AddChild(CharacterInventoryDisplayParent, GUIManager.Get.BarterContainerDisplay);
						GameObject PlayerGoodsDisplayObject = NGUITools.AddChild(PlayerGoodsDisplayParent, GUIManager.Get.BarterContainerDisplay);
						GameObject CharacterGoodsDisplayObject = NGUITools.AddChild(CharacterGoodsDisplayParent, GUIManager.Get.BarterContainerDisplay);

						GameObject PlayerInventoryBankObject = NGUITools.AddChild(PlayerInventoryBankDisplayParent, GUIManager.Get.InventoryBank);
						GameObject PlayerGoodsBankObject = NGUITools.AddChild(PlayerGoodsBankDisplayParent, GUIManager.Get.InventoryBank);
						GameObject CharacterInventoryBankObject = NGUITools.AddChild(CharacterInventoryBankDisplayParent, GUIManager.Get.InventoryBank);
						GameObject CharacterGoodsBankObject = NGUITools.AddChild(CharacterGoodsBankDisplayParent, GUIManager.Get.InventoryBank);

						PlayerInventoryDisplay = PlayerInventoryDisplayObject.GetComponent <GUIBarterContainerDisplay>();
						PlayerInventoryDisplay.BarterMode = BarterContainerMode.Goods;
						PlayerInventoryDisplay.Party = BarterParty.Player;
						PlayerInventoryDisplay.EnablerDisplayPrefab = GUIManager.Get.InventorySquareEnablerDisplay;
						PlayerInventoryDisplay.EnablerOffset = new Vector3(-140f, 50f, 0f);
						PlayerInventoryDisplay.DisplayMode = StackContainerDisplayMode.TwoRowVertical;
						PlayerInventoryDisplay.FrameSprite.enabled = false;

						CharacterInventoryDisplay = CharacterInventoryDisplayObject.GetComponent <GUIBarterContainerDisplay>();
						CharacterInventoryDisplay.BarterMode = BarterContainerMode.Goods;
						CharacterInventoryDisplay.Party = BarterParty.Character;
						CharacterInventoryDisplay.EnablerDisplayPrefab = GUIManager.Get.InventorySquareEnablerDisplay;
						CharacterInventoryDisplay.EnablerOffset = new Vector3(-140f, 50f, 0f);
						CharacterInventoryDisplay.DisplayMode = StackContainerDisplayMode.TwoRowVertical;
						CharacterInventoryDisplay.FrameSprite.enabled = false;

						CharacterGoodsDisplay = CharacterGoodsDisplayObject.GetComponent <GUIBarterContainerDisplay>();
						CharacterGoodsDisplay.BarterMode = BarterContainerMode.Offer;
						CharacterGoodsDisplay.Party = BarterParty.Character;
						CharacterGoodsDisplay.DisplayMode = StackContainerDisplayMode.TwoRow;
						CharacterGoodsDisplay.FrameSprite.enabled = false;

						PlayerGoodsDisplay = PlayerGoodsDisplayObject.GetComponent <GUIBarterContainerDisplay>();
						PlayerGoodsDisplay.BarterMode = BarterContainerMode.Offer;
						PlayerGoodsDisplay.Party = BarterParty.Player;
						PlayerGoodsDisplay.DisplayMode = StackContainerDisplayMode.TwoRow;
						PlayerGoodsDisplay.FrameSprite.enabled = false;

						PlayerInventoryBank = PlayerInventoryBankObject.GetComponent <GUIBank>();
						CharacterInventoryBank = CharacterInventoryBankObject.GetComponent <GUIBank>();
						PlayerGoodsBank = PlayerGoodsBankObject.GetComponent <GUIBank>();
						CharacterGoodsBank = CharacterGoodsBankObject.GetComponent <GUIBank>();

						PlayerInventoryBank.DisplayMode = GUIBank.BankDisplayMode.SmallTwoRows;
						CharacterInventoryBank.DisplayMode = GUIBank.BankDisplayMode.SmallTwoRows;
						PlayerGoodsBank.DisplayMode = GUIBank.BankDisplayMode.SmallTwoRows;
						CharacterGoodsBank.DisplayMode = GUIBank.BankDisplayMode.SmallTwoRows;

						mRefreshBarterGUIAction = Refresh;
				}

				protected override void OnFinish()
				{
						mEditObject.RefreshAction -= mRefreshBarterGUIAction;
						mEditObject.BarterManager.StopBartering();
						base.OnFinish();
				}

				public void OnClickPlayerNextPageButton()
				{
						mWaitingForContainer = true;
						StartCoroutine(mEditObject.BarterManager.NextPlayerStackContainer());
				}

				public void OnClickPlayerPrevPageButton()
				{
						mWaitingForContainer = true;
						StartCoroutine(mEditObject.BarterManager.PrevPlayerStackContainer());
				}

				public void OnClickCharacterNextPageButton()
				{
						mWaitingForContainer = true;
						StartCoroutine(mEditObject.BarterManager.NextCharacterStackContainer());
				}

				public void OnClickCharacterPrevPageButton()
				{
						mWaitingForContainer = true;
						StartCoroutine(mEditObject.BarterManager.PrevCharacterStackContainer());
				}

				public void OnClickCancelButton()
				{
						Finish();
				}

				public void OnClickApproveTradeButton()
				{
						mEditObject.TryToMakeTrade();
				}

				public void SetPlayerInventoryContainer(WIStackEnabler enabler)
				{
						PlayerInventoryDisplay.SetEnabler(enabler);
						mWaitingForContainer = false;
				}

				public void SetCharacterInventoryContainer(WIStackEnabler enabler)
				{
						CharacterInventoryDisplay.SetEnabler(enabler);
						mWaitingForContainer = false;
				}

				public override void Refresh()
				{
						//don't bother with the stack containers and squares
						//they take care of themselves
						BaseValueCharacterGoodsLabel.text = mEditObject.TotalValueCharacterGoods.ToString();
						BaseValuePlayerGoodsLabel.text = mEditObject.TotalValuePlayerGoods.ToString();
						//TODO enable these only when some kind of dev global is set
						if (Skills.Get.DebugSkills) {
								SkillPenaltyLabel.enabled = true;
								RepPenaltyLabel.enabled = true;
								TotalPenaltyLabel.enabled = true;

								SkillPenaltyLabel.text = "Skill: " + mEditObject.SkillPriceModifier.ToString("0.00");
								RepPenaltyLabel.text = "Rep: " + mEditObject.ReputationPriceModifier.ToString("0.00");
								TotalPenaltyLabel.text = "Total: " + mEditObject.FinalPriceModifier.ToString("0.00");
						} else {
								SkillPenaltyLabel.enabled = false;
								RepPenaltyLabel.enabled = false;
								TotalPenaltyLabel.enabled = false;
						}

						if (mEditObject.TotalValuePlayerGoods > 0) {
								PlayerToPlayerArrowAlphaTarget = 1f;
								ApprovedTradeCharacterLabelAlphaTarget = 1f;
						} else {
								PlayerToPlayerArrowAlphaTarget = 0f;
								ApprovedTradeCharacterLabelAlphaTarget = 0f;
						}

						if (mEditObject.TotalValueCharacterGoods > 0) {
								CharacterToCharacterArrowAlphaTarget = 1f;
						} else {
								CharacterToCharacterArrowAlphaTarget = 0f;
						}

						if (mEditObject.CharacterApprovesTrade) {
								CharacterToPlayerArrowAlphaTarget = 1.0f;
								CharacterToPlayerArrowTarget = ApprovedTradeCharacterToPlayerArrow;
								CharacterToPlayerArrowBump.x = 5f;
								ApproveTradeLabelCharacter.text = mEditObject.BarteringCharacter.FullName + " thinks this trade is fair";
						} else {
								CharacterToPlayerArrowAlphaTarget = 0f;
								CharacterToPlayerArrowTarget = UnapprovedTradeCharacterToPlayerArrow;
								CharacterToPlayerArrowBump.x = 0f;
								ApproveTradeLabelCharacter.text = mEditObject.BarteringCharacter.FullName + " doesn't think this trade is fair";
						}

						if (mEditObject.CanMakeTrade) {
								PlayerToCharacterArrowAlphaTarget = 1.0f;
								PlayerToCharacterArrowTarget = ApprovedTradePlayerToCharacterArrow;
								PlayerToCharacterArrowBump.x = 5f;
								ApproveTradeButtonPlayer.SendMessage("SetEnabled");
								ApproveTradeLabelPlayer.text = "Make Trade";
						} else {
								PlayerToCharacterArrowAlphaTarget = 0f;
								PlayerToCharacterArrowBump.x = 0f;
								PlayerToCharacterArrowTarget = UnapprovedTradePlayerToCharacterArrow;
								ApproveTradeButtonPlayer.SendMessage("SetDisabled");
								ApproveTradeLabelPlayer.text = "Can't Make Trade";
						}

						//base.Refresh ();
						//		int balance = mEditObject.Balance;
						//		BalanceLabel.text = ("Balance: " + balance.ToString ());
						//		if (balance < 0) {
						//			BalanceLabel.color = Colors.Get.MessageDangerColor;
						//		} else {
						//			BalanceLabel.color = Colors.Get.MessageSuccessColor;
						//		}
				}

				public override void Update()
				{
						base.Update();

						if (mDestroyed || mFinished) {
								return;
						}

						if (!HasEditObject) {
								PlayerToCharacterArrow.alpha = 0f;
								CharacterToPlayerArrow.alpha = 0f;
								PlayerToPlayerArrow.alpha = 0f;
								CharacterToCharacterArrow.alpha = 0f;
								ApproveTradeLabelCharacter.alpha = 0f;
								PlayerToCharacterArrow.cachedTransform.localPosition = UnapprovedTradePlayerToCharacterArrow;
								CharacterToPlayerArrow.cachedTransform.localPosition = UnapprovedTradeCharacterToPlayerArrow;
						}

						ApproveTradeLabelCharacter.alpha = Mathf.Lerp(ApproveTradeLabelCharacter.alpha, ApprovedTradeCharacterLabelAlphaTarget, 0.25f);
						PlayerToCharacterArrow.alpha = Mathf.Lerp(PlayerToCharacterArrow.alpha, PlayerToCharacterArrowAlphaTarget, 0.25f);
						CharacterToPlayerArrow.alpha = Mathf.Lerp(CharacterToPlayerArrow.alpha, CharacterToPlayerArrowAlphaTarget, 0.25f);
						PlayerToPlayerArrow.alpha = Mathf.Lerp(PlayerToPlayerArrow.alpha, PlayerToPlayerArrowAlphaTarget, 0.25f);
						CharacterToCharacterArrow.alpha = Mathf.Lerp(CharacterToCharacterArrow.alpha, CharacterToCharacterArrowAlphaTarget, 0.25f);

						Vector3 playerToCharacterArrowTargetBumped = PlayerToCharacterArrowTarget + (PlayerToCharacterArrowBump * Mathf.Sin((float)WorldClock.RealTime));
						PlayerToCharacterArrow.cachedTransform.localPosition = Vector3.Lerp(PlayerToCharacterArrow.cachedTransform.localPosition, PlayerToCharacterArrowTarget, 0.25f);

						Vector3 characterToPlayerArrowTargetBumped = CharacterToPlayerArrowTarget + (CharacterToPlayerArrowBump * Mathf.Sin((float)WorldClock.RealTime));
						CharacterToPlayerArrow.cachedTransform.localPosition = Vector3.Lerp(CharacterToPlayerArrow.cachedTransform.localPosition, CharacterToPlayerArrowTarget, 0.25f);
				}

				protected IEnumerator PushEditObjectToNGUIObjectOverTime()
				{
						//wait a tick - we do this to allow our containers to be built, etc.
						while (PlayerInventoryDisplay == null) {
								yield return null;
						}

						PlayerInventoryDisplay.Session = mEditObject;
						CharacterInventoryDisplay.Session = mEditObject;
						PlayerGoodsDisplay.Session = mEditObject;
						CharacterGoodsDisplay.Session = mEditObject;
						//this will build the squares
						PlayerInventoryDisplay.Refresh();
						CharacterInventoryDisplay.Refresh();
						PlayerGoodsDisplay.Refresh();
						CharacterGoodsDisplay.Refresh();
						//set up the banks so they'll transfer to each other correctly when clicked
						PlayerInventoryBank.SetBank(mEditObject.PlayerInventory.InventoryBank, mEditObject.PlayerGoodsBank);
						CharacterInventoryBank.SetBank(mEditObject.CharacterInventory.InventoryBank, mEditObject.CharacterGoodsBank);
						PlayerGoodsBank.SetBank(mEditObject.PlayerGoodsBank, mEditObject.PlayerInventory.InventoryBank);
						CharacterGoodsBank.SetBank(mEditObject.CharacterGoodsBank, mEditObject.CharacterInventory.InventoryBank);

						while (!PlayerInventoryDisplay.HasCreatedSquares) {
								yield return null;
						}
						//this will automatically refresh everything
						EditObject.Activate();

						PlayerNameLabel.text = mEditObject.PlayerInventory.InventoryOwnerName;
						CharacterNameLabel.text = mEditObject.CharacterInventory.InventoryOwnerName;
						//BuyDisplay.SetEnabler (mEditObject.BuyEnabler);
						//SellDisplay.SetEnabler (mEditObject.SellEnabler);

						//are we being asked to barter a specific item?
						/*
						if (mEditObject.HasCharacterStartupItem) {
							BarterGoods startupGoods = new BarterGoods ();
							startupGoods.Add (mEditObject.CharacterStartupStack, 1);
							mEditObject.CharacterGoods.Add (startupGoods);
							mEditObject.CurrentCharacterStackEnablerIndex = 0;
							//the next page button will automatically set the goods to 1
							//which is where temporary items are held
						}
						*/
						//now find the stack enabler to start with
						bool goToNextCharacterPage = true;
						if (mEditObject.HasCharacterStartupItem) {
								bool foundStartupStack = false;
								int iterations = 0;
								int maxIterations = 100;
								while (!foundStartupStack) {
										OnClickCharacterNextPageButton();
										while (mWaitingForContainer) {
												yield return null;
										}
										//check the stack
										List <WIStack> stacks = mEditObject.CurrentCharacterStackEnabler.EnablerStacks;
										for (int i = 0; i < stacks.Count; i++) {
												if (stacks[i] == mEditObject.CharacterStartupStack) {
														foundStartupStack = true;
														break;
												} else {
														WIStack newStartupStack = null;
														if (Stacks.Find.Item(mEditObject.CurrentPlayerStackEnabler.EnablerContainer, mEditObject.CharacterStartupItem, out newStartupStack)) { 
																mEditObject.CharacterStartupStack = newStartupStack;
																foundStartupStack = true;
																break;
														}
												}
										}
										if (iterations > maxIterations) {
												Debug.Log("Reached max iterations, breaking");
												break;
										}
								}
								if (foundStartupStack) {
										//now put that good in the 'buy' column immediately for convenience
										mEditObject.AddGoods(mEditObject.CharacterStartupStack, 1, BarterParty.Character);
										goToNextCharacterPage = false;
								}
						}

						if (goToNextCharacterPage) {
								OnClickCharacterNextPageButton();
								while (mWaitingForContainer) {
										yield return null;
								}
						}
						OnClickPlayerNextPageButton();
						while (mWaitingForContainer) {
								yield return null;
						}
						yield return null;
						mPushingObjectToNGUIObjectOverTime = false;
						yield break;
				}

				protected System.Action mRefreshBarterGUIAction;
				protected bool mWaitingForContainer = false;
				protected bool mLastEnteredFromInterface = false;
		}
}
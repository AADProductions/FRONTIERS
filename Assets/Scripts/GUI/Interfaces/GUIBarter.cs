using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIBarter : GUIEditor <BarterSession>, IInfoDisplay
		{
				public UISprite BarterIconTop;
				public UISprite BarterIconBorderTop;
				public UISprite BarterIconBot;
				public UISprite BarterIconBorderBot;
				public UISprite MapIconBot;
				public UISprite MapIconBorderBot;
				public UISprite MapIconTop;
				public UISprite MapIconBorderTop;
				public UIPanel ModifierPanelTop;
				public UIPanel ModifierPanelBot;
				public GUIStatusKeeper StatusKeeperTop;
				public GUIStatusKeeper StatusKeeperBot;
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
				public UIPanel InfoPanel;
				public bool DisplayInfo = false;
				public GameObject CurrentInfoTarget;
				public float CurrentInfoTargetXOffset;
				public float CurrentInfoTargetYOffset;
				public string CurrentInfo;
				public UILabel InfoLabel;
				public UISprite InfoSpriteShadow;
				public UISprite InfoSpriteBackground;
				public Transform InfoOffset;

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

						#if UNITY_EDITOR
						if (VRManager.VRMode | VRManager.VRTestingMode) {
								#else
						if (VRManager.VRMode) {
								#endif
								Vector3 localPosition = transform.localPosition;
								localPosition.y = -75f;
								transform.localPosition = localPosition;
						}
						StatusKeeper s = null;
						Player.Local.Status.GetStatusKeeper("Personal Reputation", out s);
						StatusKeeperTop.Initialize(s, null, 0, 1f);
						StatusKeeperBot.Initialize(s, null, 0, 1f);
						StatusKeeperTop.DisplayInfo = this;
						StatusKeeperBot.DisplayInfo = this;

						Skill skill = null;
						Skills.Get.SkillByName("Barter", out skill);
						BarterIconBorderTop.color = skill.SkillBorderColor;
						BarterIconBorderBot.color = skill.SkillBorderColor;
						BarterIconTop.color = skill.SkillIconColor;
						BarterIconBot.color = skill.SkillIconColor;
						MapIconTop.color = Color.grey;
						MapIconBot.color = Color.grey;
						MapIconBorderTop.color = Color.gray;
						MapIconBorderBot.color = Color.gray;

						BarterIconBorderBot.transform.parent.GetComponent <GUIButtonHover>().OnButtonHover += OnHoverOnSkill;
						BarterIconBorderTop.transform.parent.GetComponent <GUIButtonHover>().OnButtonHover += OnHoverOnSkill;
						MapIconBorderTop.transform.parent.GetComponent <GUIButtonHover>().OnButtonHover += OnHoverOnMap;
						MapIconBorderBot.transform.parent.GetComponent <GUIButtonHover>().OnButtonHover += OnHoverOnMap;
				}

				protected void OnHoverOnMap()
				{

						string description = "All of your items are from around here.";
						PostInfo(UICamera.hoveredObject, description);
				}

				protected void OnHoverOnSkill()
				{

						Skill skill = null;
						Skills.Get.SkillByName("Barter", out skill);
						string description = "Your barter skill is " + Skill.MasteryAdjective(skill.State.NormalizedMasteryLevel);
						PostInfo(UICamera.hoveredObject, description);
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
						if (mEditObject.BarteringCharacter.State.Flags.Gender == 1) {
								BaseValueCharacterGoodsLabel.text = "His price for his goods:" + Colors.ColorWrap (" $", Colors.Darken (BaseValueCharacterGoodsLabel.color)) + mEditObject.TotalValueCharacterGoods.ToString();
								BaseValuePlayerGoodsLabel.text = "His offer for your goods:" + Colors.ColorWrap (" $", Colors.Darken (BaseValuePlayerGoodsLabel.color)) + mEditObject.TotalValuePlayerGoods.ToString();
						} else {
								BaseValueCharacterGoodsLabel.text = "Her price for her goods:" + Colors.ColorWrap (" $", Colors.Darken (BaseValueCharacterGoodsLabel.color)) + mEditObject.TotalValueCharacterGoods.ToString();
								BaseValuePlayerGoodsLabel.text = "Her offer for your goods:" + Colors.ColorWrap (" $", Colors.Darken (BaseValuePlayerGoodsLabel.color)) + mEditObject.TotalValuePlayerGoods.ToString();
						}

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
								ModifierPanelBot.enabled = true;
						} else {
								PlayerToPlayerArrowAlphaTarget = 0f;
								ApprovedTradeCharacterLabelAlphaTarget = 0f;
								ModifierPanelBot.enabled = false;
						}

						if (mEditObject.TotalValueCharacterGoods > 0) {
								CharacterToCharacterArrowAlphaTarget = 1f;
								ModifierPanelTop.enabled = true;
						} else {
								CharacterToCharacterArrowAlphaTarget = 0f;
								ModifierPanelTop.enabled = false;
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
								ApproveTradeLabelPlayer.text = "No Trade";
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

						if (DisplayInfo) {
								if (UICamera.hoveredObject == null || UICamera.hoveredObject != CurrentInfoTarget) {
										DisplayInfo = false;
								}
								if (InfoSpriteShadow.alpha < 1f) {
										InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 1f, 0.25f);
										if (InfoSpriteShadow.alpha > 0.99f) {
												InfoSpriteShadow.alpha = 1f;
										}
								}
								//make sure the info doesn't overlay an icon
								mInfoOffset.x = CurrentInfoTargetXOffset;
								mInfoOffset.y = CurrentInfoTargetYOffset;
								InfoOffset.localPosition = mInfoOffset;
						} else {
								if (InfoSpriteShadow.alpha > 0f) {
										InfoSpriteShadow.alpha = Mathf.Lerp(InfoSpriteShadow.alpha, 0f, 0.25f);
										if (InfoSpriteShadow.alpha < 0.01f) {
												InfoSpriteShadow.alpha = 0f;
										}
								}
						}
						InfoLabel.alpha = InfoSpriteShadow.alpha;
						InfoSpriteBackground.alpha = InfoSpriteShadow.alpha;

						if (!HasEditObject) {
								PlayerToCharacterArrow.alpha = 0f;
								CharacterToPlayerArrow.alpha = 0f;
								PlayerToPlayerArrow.alpha = 0f;
								CharacterToCharacterArrow.alpha = 0f;
								ApproveTradeLabelCharacter.alpha = 0f;
								PlayerToCharacterArrow.cachedTransform.localPosition = UnapprovedTradePlayerToCharacterArrow;
								CharacterToPlayerArrow.cachedTransform.localPosition = UnapprovedTradeCharacterToPlayerArrow;
								ModifierPanelBot.enabled = false;
								ModifierPanelTop.enabled = false;
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

				public void PostInfo(GameObject target, string info)
				{
						CurrentInfoTarget = target;
						CurrentInfoTargetXOffset = InfoPanel.transform.InverseTransformPoint(target.transform.position).x;
						CurrentInfoTargetYOffset = InfoPanel.transform.InverseTransformPoint(target.transform.position).y;
						CurrentInfo = info;
						InfoLabel.text = CurrentInfo;
						DisplayInfo = true;
						//update the box around the text to reflect its size
						Transform textTrans = InfoLabel.transform;
						Vector3 offset = textTrans.localPosition;
						Vector3 textScale = textTrans.localScale;

						// Calculate the dimensions of the printed text
						Vector3 size = InfoLabel.relativeSize;

						// Scale by the transform and adjust by the padding offset
						size.x *= textScale.x;
						size.y *= textScale.y;
						size.x += 50f;
						size.y += 50f;
						size.x += (InfoSpriteBackground.border.x + InfoSpriteBackground.border.z + (offset.x - InfoSpriteBackground.border.x) * 2f);
						size.y += (InfoSpriteBackground.border.y + InfoSpriteBackground.border.w + (-offset.y - InfoSpriteBackground.border.y) * 2f);
						size.z = 1f;

						InfoSpriteBackground.transform.localScale = size;
						InfoSpriteShadow.transform.localScale = size;
				}

				protected System.Action mRefreshBarterGUIAction;
				protected bool mWaitingForContainer = false;
				protected bool mLastEnteredFromInterface = false;
				protected Vector3 mInfoOffset;
		}

		public interface IInfoDisplay
		{
				void PostInfo(GameObject target, string info);
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System.Linq;
using System;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World.Gameplay
{
		public class Barter : RemoveItemSkill, IGUIParentEditor <BarterSession>
		{
				public override void Initialize()
				{
						base.Initialize();
						mCharacterGoods = new List <BarterGoods>();
						mPlayerGoods = new List <BarterGoods>();
						for (int i = 0; i < Globals.MaxStacksPerContainer; i++) {
								mCharacterGoods.Add(new BarterGoods());
								mPlayerGoods.Add(new BarterGoods());
						}
				}

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (base.DoesContextAllowForUse(targetObject)) {
								Character character = null;
								if (targetObject.worlditem.Is <Character>(out character)) {
										if (character.IsDead || character.IsStunned) {
												Debug.Log("Can't barter with dead characters");
												return false;
										} else {
												return true;
										}
								} else {
										return false;
								}
						}
						return false;
				}

				public override void TryToRemoveItem(IStackOwner skillUseTarget, IWIBase worldItemToMove, IInventory toInventory, Action callBack)
				{
						//instead of attempting to remove it now
						//this skill launched the barter dialog
						//and sets up a session
						IInventory barterInventory = null;
						Character character = null;
						//this only works with character-owned items
						if (skillUseTarget.IsWorldItem && skillUseTarget.worlditem.Is <Character>(out character)) {
								barterInventory = character;
								//make sure we can actually barter with the player
								ReputationState rep = Profile.Get.CurrentGame.Character.Rep.GetReputation(character.worlditem.FileName);
								if (rep.NormalizedReputationDifference(Profile.Get.CurrentGame.Character.Rep.GlobalReputation) > Extensions.MaxNormaliedReputationDifference) {
										GUIManager.PostWarning(character.worlditem.DisplayName + " is not interested in bartering with you.");
										return;
								}

								if (mCurrentSession == null) {
										mCurrentSession = new BarterSession(this, mCharacterGoods, mPlayerGoods);
								}
								mCurrentSession.Reset(Player.Local.Inventory, barterInventory);
								mCurrentSession.BarteringCharacter = character;
								//add the selected goods to the current session immediately
								//so it's there when we start bartering
								mCurrentSession.CharacterStartupItem = worldItemToMove;
								WIStack startupStack = null;
								//if the startup item is already in the character's inventory
								//no need to create a startup stack
								//but if it isn't we'll need the startup stack to display it
								if (!character.HasItem(worldItemToMove, out startupStack)) {
										//TODO put entire container in inventory
										Debug.Log ("Item " + worldItemToMove.FileName + " was NOT in character's inventory, creating temporary startup stack");
										mCurrentSession.CharacterStartupStack = character.HoldTemporaryItem(worldItemToMove);
								}
								SpawnBarterDialog();
						}
				}

				public override void TryToRemoveItem(IStackOwner skillUseTarget, WIStack fromStack, WIStack toStack, WIGroup toGroup, Action callBack)
				{
						//instead of attempting to remove it now
						//this skill launched the barter dialog
						//and sets up a session
						WorldItem worldItem = null;
						Stacks.Convert.TopItemToWorldItem(fromStack, out worldItem);
						IWIBase worldItemToMove = worldItem;
						IInventory barterInventory = null;
						Character character = null;
						//this only works with character-owned items
						if (skillUseTarget.IsWorldItem && skillUseTarget.worlditem.Is <Character>(out character)) {
								barterInventory = character;
								//make sure we can actually barter with the player
								ReputationState rep = Profile.Get.CurrentGame.Character.Rep.GetReputation(character.worlditem.FileName);
								if (rep.NormalizedReputationDifference(Profile.Get.CurrentGame.Character.Rep.GlobalReputation) > Extensions.MaxNormaliedReputationDifference) {
										GUIManager.PostWarning(character.worlditem.DisplayName + " is not interested in bartering with you.");
										return;
								}

								if (mCurrentSession == null) {
										mCurrentSession = new BarterSession(this, mCharacterGoods, mPlayerGoods);
								}
								mCurrentSession.Reset(Player.Local.Inventory, barterInventory);
								mCurrentSession.BarteringCharacter = character;
								//add the selected goods to the current session immediately
								//so it's there when we start bartering
								mCurrentSession.CharacterStartupItem = worldItemToMove;
								//if the startup item is already in the character's inventory
								//no need to create a startup stack
								//but if it isn't we'll need the startup stack to display it
								WIStack startupStack = null;
								if (!character.HasItem(worldItemToMove, out startupStack)) {
										Debug.Log("Item " + worldItemToMove.FileName + " was NOT in character's inventory, creating temporary startup stack");
										if (fromStack.NumItems == 1) {
												//just hold the one item
												mCurrentSession.CharacterStartupStack = character.HoldTemporaryItem(worldItemToMove);
										} else {
												mCurrentSession.CharacterStartupStack = character.HoldTemporaryItem(fromStack);
										}
								} else {
										Debug.Log("Item " + worldItemToMove.FileName + " was in character's inventory");
										mCurrentSession.CharacterStartupStack = startupStack;
								}
								SpawnBarterDialog();
						}
				}

				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						//first check if we're dealing directly with a barterable object
						IInventory barterInventory = targetObject.gameObject.GetComponent("IInventory") as IInventory;
						Container container = null;
						Character character = null;
						if (barterInventory == null) {
								//if not, check if we're dealing with an owned container
								if (targetObject.IOIType == ItemOfInterestType.WorldItem && targetObject.worlditem.Is <Container>(out container)) {
										//get the owner of the container
										IStackOwner owner = null;
										if (targetObject.worlditem.Group.HasOwner(out owner) && owner.IsWorldItem) {
												barterInventory = (IInventory)owner.worlditem.GetComponent(typeof(IInventory));
										} else {//if the owner isn't a world item then it hasn't been loaded
												//TODO handle unloaded shop owners
										}
										character = owner.worlditem.Get <Character>();
								}
						} else {
								character = targetObject.worlditem.Get <Character>();
						}

						if (character != null) {
								ReputationState rep = Profile.Get.CurrentGame.Character.Rep.GetReputation(character.worlditem.FileName);
								if (rep.NormalizedReputationDifference(Profile.Get.CurrentGame.Character.Rep.GlobalReputation) > Extensions.MaxNormaliedReputationDifference) {
										GUIManager.PostWarning(character.worlditem.DisplayName + " is not interested in bartering with you.");
										return false;
								}
						}

						if (barterInventory != null) {
								if (mCurrentSession == null) {
										mCurrentSession = new BarterSession(this, mCharacterGoods, mPlayerGoods);
								}
								mCurrentSession.Reset(Player.Local.Inventory, barterInventory);
								mCurrentSession.BarteringCharacter = character;
								if (flavorIndex > 0) {
										//flavor 0 is default mode
										//flavor 1 is zero cost mode (friends, multiplayer, etc)
										mCurrentSession.ZeroCostMode = true;
								}
								SpawnBarterDialog();
								return true;
						}
						return false;
				}

				public override bool Use(bool successfully)
				{
						//override this to prevent it from attempting to remove items again
						UseStart(successfully);
						return true;
				}

				protected override void OnUseFinish()
				{//barter actually manually moves items from one inventory to the other
						//so we don't even need RemoveItemSkill's use finish
						return;
				}

				protected void SpawnBarterDialog()
				{
						mIsInUse = true;
						Skills.Get.SkillsInUse.SafeAdd(this);
						mChildEditor = GUIManager.SpawnNGUIChildEditor(this.gameObject, GUIManager.Get.NGUIBarter, false);
						BarterInterface = mChildEditor.GetComponent <GUIBarter>();
						GUIManager.SendEditObjectToChildEditor <BarterSession>(new ChildEditorCallback <BarterSession>(ReceiveFromChildEditor), mChildEditor, mCurrentSession);
				}

				public void ReceiveFromChildEditor(BarterSession result, IGUIChildEditor <BarterSession> childEditor)
				{
						if (childEditor == null) {
								return;
						}

						GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
						mChildEditor = null;
				}

				public GameObject NGUIObject { get { return gameObject; } set { } }

				public static BarterSession CurrentSession {
						get {
								return mCurrentSession;
						}
				}

				protected GameObject mChildEditor;
				public GUIBarter BarterInterface;

				public static bool IsBartering {
						get {
								return mCurrentSession != null && mCurrentSession.IsActive && mCurrentSession.BarteringCharacter != null;
						}
				}

				public bool HasMadeTradeThisSession {
						get {
								return mHasMadeTradeThisSession;
						}
				}

				public void OnInterfaceCreated(GUIBarter barterInterface)
				{
						BarterInterface = barterInterface;
				}

				public bool CanPlayerBarterWith(Character character)
				{
						return character.worlditem.Is<Inventory>();// && character.HealthState != AnimalHealthState.Dead;
				}

				public void StopBartering()
				{
						mStopping = true;
						mCurrentSession.BarteringCharacter.DropTemporaryItem();
						mStopping = false;
						mIsInUse = false;
				}

				public IEnumerator NextPlayerStackContainer()
				{
						GetInventoryContainerResult result = new GetInventoryContainerResult();
						yield return StartCoroutine(mCurrentSession.PlayerInventory.GetInventoryContainer(mCurrentSession.CurrentPlayerStackEnablerIndex, true, result));
						if (result.FoundContainer) {
								mCurrentSession.CurrentPlayerStackEnablerIndex = result.ContainerIndex;
								mCurrentSession.CurrentPlayerStackEnabler = result.ContainerEnabler;
								BarterInterface.SetPlayerInventoryContainer(mCurrentSession.CurrentPlayerStackEnabler);
						}
						mCurrentSession.RefreshAction.SafeInvoke();
						yield break;
				}

				public IEnumerator PrevPlayerStackContainer()
				{
						GetInventoryContainerResult result = new GetInventoryContainerResult();
						yield return StartCoroutine(mCurrentSession.PlayerInventory.GetInventoryContainer(mCurrentSession.CurrentPlayerStackEnablerIndex, false, result));
						if (result.FoundContainer) {
								mCurrentSession.CurrentPlayerStackEnablerIndex = result.ContainerIndex;
								mCurrentSession.CurrentPlayerStackEnabler = result.ContainerEnabler;
								BarterInterface.SetPlayerInventoryContainer(mCurrentSession.CurrentPlayerStackEnabler);
						}
						mCurrentSession.RefreshAction.SafeInvoke();
						yield break;
				}

				public IEnumerator NextCharacterStackContainer()
				{
						GetInventoryContainerResult result = new GetInventoryContainerResult();
						yield return StartCoroutine(mCurrentSession.CharacterInventory.GetInventoryContainer(mCurrentSession.CurrentCharacterStackEnablerIndex, true, result));
						if (result.FoundContainer) {
								mCurrentSession.CurrentCharacterStackEnablerIndex = result.ContainerIndex;
								mCurrentSession.CurrentCharacterStackEnabler = result.ContainerEnabler;
								BarterInterface.SetCharacterInventoryContainer(mCurrentSession.CurrentCharacterStackEnabler);
								BarterInterface.CharacterNameLabel.text = (result.ContainerIndex + 1).ToString() + " / " + result.TotalContainers.ToString();
						}
						mCurrentSession.RefreshAction.SafeInvoke();
						yield break;
				}

				public IEnumerator PrevCharacterStackContainer()
				{
						GetInventoryContainerResult result = new GetInventoryContainerResult();
						yield return StartCoroutine(mCurrentSession.CharacterInventory.GetInventoryContainer(mCurrentSession.CurrentCharacterStackEnablerIndex, false, result));
						if (result.FoundContainer) {
								mCurrentSession.CurrentCharacterStackEnablerIndex = result.ContainerIndex;
								mCurrentSession.CurrentCharacterStackEnabler = result.ContainerEnabler;
								BarterInterface.SetCharacterInventoryContainer(mCurrentSession.CurrentCharacterStackEnabler);
								BarterInterface.CharacterNameLabel.text = (result.ContainerIndex + 1).ToString() + " / " + result.TotalContainers.ToString();
						}
						mCurrentSession.RefreshAction.SafeInvoke();
						yield break;
				}

				protected static BarterSession mCurrentSession;
				protected List <BarterGoods> mCharacterGoods;
				protected List <BarterGoods> mPlayerGoods;
				protected bool mHasMadeTradeThisSession = false;
				protected WIStackListner mStackContainerListner;
				protected bool mStopping = false;
				protected bool mStarting = false;
				protected bool mRefreshNextFrame = false;
		}

		[Serializable]
		public class BarterSession
		{
				public BarterSession(Barter barterManager, List <BarterGoods> characterGoods, List <BarterGoods> playerGoods)
				{
						BarterManager = barterManager;

						PlayerGoods = playerGoods;
						CharacterGoods = characterGoods;
						PlayerGoodsBank = new Bank();
						CharacterGoodsBank = new Bank();

						PlayerGoodsBank.RefreshAction += OnBankValuesChange;
						CharacterGoodsBank.RefreshAction += OnBankValuesChange;
				}

				public Action RefreshAction;

				public void Reset(IInventory playerInventory, IInventory characterInventory)
				{
						PlayerStartupItem = null;
						CharacterStartupItem = null;
						CharacterStartupStack = null;
						PlayerInventory = playerInventory;
						CharacterInventory = characterInventory;
						CurrentCharacterStackEnablerIndex = 0;
						TotalValuePlayerGoods = 0;
						TotalValueCharacterGoods = 0;
						BaseValuePlayerGoods = 0;
						BaseValueCharacterGoods = 0;
						ZeroCostMode = false;
						IsActive = false;
						ClearGoodsAndCurrency();
						//don't reset player index
						RefreshAction = null;
				}

				public int NumGoodsSent(BarterParty party, WIStack stack)
				{
						List <BarterGoods> goods = null;
						if (party == BarterParty.Player) {
								goods = PlayerGoods;
						} else {
								goods = CharacterGoods;
						}
						int numItems = 0;
						foreach (BarterGoods good in goods) {
								if (good.TryGetValue(stack, out numItems)) {
										break;
								}
						}
						return numItems;
				}

				public void OnBankValuesChange()
				{
						RecalculateValueOfGoods(BarterParty.Character, false);
						RecalculateValueOfGoods(BarterParty.Player, false);
				}

				public bool RemoveGoods(BarterParty party, IWIBase goodToRemove, int numToRemove)
				{
						if (goodToRemove == null || numToRemove == 0) {
								return false;
						}

						List <BarterGoods> goods = null;
						if (party == BarterParty.Player) {
								goods = PlayerGoods;
						} else {
								goods = CharacterGoods;
						}
						List <KeyValuePair <BarterGoods, WIStack>> goodStacksToRemove = new List <KeyValuePair <BarterGoods, WIStack>>();
						List <KeyValuePair <KeyValuePair<BarterGoods,WIStack>,int>> goodStacksToUpdate = new List <KeyValuePair <KeyValuePair<BarterGoods,WIStack>,int>>();
						foreach (BarterGoods good in goods) {
								foreach (KeyValuePair <WIStack,int> goodPair in good) {
										if (Stacks.Can.Stack(goodPair.Key, goodToRemove)) {
												if (numToRemove <= 1) {
														//simple removal
														int numItems = goodPair.Value;
														numItems--;
														if (numItems <= 0) {
																goodStacksToRemove.Add(new KeyValuePair<BarterGoods, WIStack>(good, goodPair.Key));
														} else {
																goodStacksToUpdate.Add(new KeyValuePair <KeyValuePair<BarterGoods,WIStack>,int>(new KeyValuePair<BarterGoods, WIStack>(good, goodPair.Key), numItems));
														}
														break;
												} else {
														//TODO removal of multiple items across multiple stacks
												}
										}
								}
						}
						foreach (KeyValuePair <BarterGoods, WIStack> goodStackToRemove in goodStacksToRemove) {
								goodStackToRemove.Key.Remove(goodStackToRemove.Value);
						}
						foreach (KeyValuePair <KeyValuePair<BarterGoods,WIStack>,int> goodToUpdate in goodStacksToUpdate) {
								goodToUpdate.Key.Key[goodToUpdate.Key.Value] = goodToUpdate.Value;
						}
						//recaluclate our total values
						RecalculateValueOfGoods(party, true);
						//update squares and interface
						RefreshAction.SafeInvoke();
						return true;
				}

				public bool AddGoods(WIStack newGoodsStack, int numGoodsToAdd, BarterParty party)
				{
						bool result = false;
						List<BarterGoods> goods = CharacterGoods;
						if (party == BarterParty.Player) {
								goods = PlayerGoods;
						}

						int goodIndex = 0;
						int numExistingGoods = 0;
						BarterGoods existingGood = null;
						BarterGoods emptyGood = null;
						BarterGoods compatibleGood = null;
						foreach (BarterGoods good in goods) {
								if (good.Count == 0) {
										if (emptyGood == null) {
												//save our first empty record for later
												emptyGood = good;
										}
								} else {
										//there's a chance this good already has a record for this stack
										//if that's the case we can drop it here and end our search
										if (good.TryGetValue(newGoodsStack, out numExistingGoods)) {
												//hooray, success
												existingGood = good;
										} else if (compatibleGood == null) {
												//okay, it wasn't in the records, but it may still stack
												//get the top item in this good and see if it stacks with the new item
												WIStack topRecord = good.Keys.First();
												if (Stacks.Can.Stack(topRecord, newGoodsStack)) {
														//save our first compatible record for later
														compatibleGood = good;
												}
										}
								}
								goodIndex++;
								//don't break the look if we've found a compatible good
								//because we don't want to lose the chance that we
								//find the existing record entry
						}
						//the only way we've left by now is if we found an exact mach
						//so settle for second and third best here
						if (existingGood != null) {
								//we set numExistingGoods when we found the existing good
								//so just add the goods to add and we're set
								existingGood[newGoodsStack] = (numExistingGoods + numGoodsToAdd);
								result = true;
						} else if (compatibleGood != null) {
								compatibleGood.Add(newGoodsStack, numGoodsToAdd);
								result = true;
						} else if (emptyGood != null) {
								emptyGood.Add(newGoodsStack, numGoodsToAdd);
								result = true;
						}
						if (result) {
								//recaluclate our total values
								RecalculateValueOfGoods(party, true);
								//refresh our squares and interface
								RefreshAction.SafeInvoke();
						}
						return result;
				}

				public void Activate()
				{
						IsActive = true;
						mMakingTrade = false;
						ClearGoodsAndCurrency();
				}

				protected void ClearGoodsAndCurrency()
				{
						Debug.Log("Clearing goods and inventory in barter");
						foreach (BarterGoods good in PlayerGoods) {
								good.Clear();
						}
						foreach (BarterGoods good in CharacterGoods) {
								good.Clear();
						}
						PlayerGoodsBank.Clear();
						CharacterGoodsBank.Clear();

						TotalValueCharacterGoods = 0;
						TotalValuePlayerGoods = 0;
						BaseValueCharacterGoods = 0;
						BaseValuePlayerGoods = 0;

						RefreshAction.SafeInvoke();
				}

				public int Balance {
						get {
								return TotalValuePlayerGoods - TotalValueCharacterGoods;
						}
				}

				public int PlayerContainerIndex {
						get {
								return CurrentPlayerStackEnablerIndex;
						}
				}

				public bool CharacterApprovesTrade {
						get {
								if (mMakingTrade)
										return false;
			
								return (TotalValuePlayerGoods > 0 && TotalValuePlayerGoods >= TotalValueCharacterGoods);
						}
				}

				public bool CanMakeTrade {
						get {
								if (mMakingTrade)
										return false;

								return (TotalValuePlayerGoods > 0 && TotalValueCharacterGoods > 0 && Balance >= 0);
						}
				}

				public bool TryToMakeTrade()
				{
						if (!CanMakeTrade) {
								return false;
						}

						//get the barter manager to handle this coroutine since we can't
						mMakingTrade = true;
						BarterManager.StartCoroutine(MakeTradeoverTime());
						return true;
				}

				protected IEnumerator MakeTradeoverTime()
				{
						Debug.Log("Starting to make trade");
						var loadStart = GUILoading.LoadStart(GUILoading.Mode.SmallInGame);
						while (loadStart.MoveNext()) {
								yield return null;
						}
						GUILoading.ActivityInfo = "Making Trade... " + CharacterGoods.Count.ToString () + " character goods going to player";
						//give stuff to player...
						foreach (BarterGoods good in CharacterGoods) {
								foreach (KeyValuePair <WIStack,int> goodPair in good) {
										if (goodPair.Key.HasTopItem) {
												Debug.Log("Adding " + goodPair.Key.TopItem.FileName + " to player inventory...");
										}
										var addItem = PlayerInventory.AddItems(goodPair.Key, goodPair.Value);
										while (addItem.MoveNext()) {
												yield return null;
										}
								}
								yield return null;
						}
						//wait a tick...
						yield return null;
						//give stuff to character...
						foreach (BarterGoods good in PlayerGoods) {
								foreach (KeyValuePair <WIStack,int> goodPair in good) {
										var enumerator = CharacterInventory.AddItems(goodPair.Key, goodPair.Value);
										while (enumerator.MoveNext()) {
												yield return null;
										}
								}
						}
						//TODO determine what counts as a 'successful use'
						BarterManager.Use(true);

						PlayerBank.Absorb(CharacterGoodsBank);
						CharacterBank.Absorb(PlayerGoodsBank);

						ClearGoodsAndCurrency();
						MadeTradeThisSession = true;
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.BarterMakeTrade, WorldClock.AdjustedRealTime);
						var loadFinish = GUILoading.LoadFinish();
						while (loadFinish.MoveNext()) {
								yield return null;
						}
						Debug.Log("Finished making trade");
						mMakingTrade = false;
						yield break;
				}

				public void RecalculateValueOfGoods(BarterParty party, bool recalculateItems)
				{
						if (!IsActive) {
								TotalValueCharacterGoods = 0;
								TotalValuePlayerGoods = 0;
								BaseValueCharacterGoods = 0;
								BaseValuePlayerGoods = 0;
								return;
						}

						List <BarterGoods> goods = CharacterGoods;
						if (party == BarterParty.Player) {
								goods = PlayerGoods;
						}

						if (goods.Count == 0) {
								BaseValueCharacterGoods = 0;
								BaseValuePlayerGoods = 0;
						} else if (recalculateItems) {
								float baseValueOfGoods = 0;
								foreach (BarterGoods good in goods) {
										int numItems = good.NumItems;
										if (numItems > 0) {
												IWIBase topItem = good.TopItem;
												float goodBaseValue = topItem.BaseCurrencyValue;
												if (topItem.Is <Stolen>()) {
														goodBaseValue *= Globals.StolenGoodsValueMultiplier;
												}
												baseValueOfGoods += goodBaseValue * numItems;
										}
								}
								if (party == BarterParty.Player) {
										BaseValuePlayerGoods = baseValueOfGoods;
								} else {
										BaseValueCharacterGoods = baseValueOfGoods;
								}
						}

						float repPriceModifier = 0f;
						ReputationState rep = null;
						if (IsBarteringWithCharacter) {
								rep = Profile.Get.CurrentGame.Character.Rep.GetReputation(BarteringCharacter.worlditem.FileName);
								repPriceModifier = rep.NormalizedOffsetReputation;//this will be a value from -1 to 1
						} else {
								repPriceModifier = Profile.Get.CurrentGame.Character.Rep.NormalizedOffsetGlobalReputation;
						}
						float skillPriceModifier = BarterManager.State.NormalizedOffsetUsageLevel;//this will be a value from -1 to 1
						float goodsPriceModifier = (repPriceModifier + skillPriceModifier) / 2f;
						if (BarterManager.HasBeenMastered) {
								//mastering the barter skill reduces all penalties to zero
								goodsPriceModifier = Mathf.Max(0, goodsPriceModifier);
						}
						goodsPriceModifier *= Globals.BarterMaximumPriceModifier;

						//divide the final modifier by 2 to get its effects on both sets of goods
						TotalValuePlayerGoods = Mathf.FloorToInt(BaseValuePlayerGoods + (BaseValuePlayerGoods * (goodsPriceModifier / 2f)));
						TotalValueCharacterGoods = Mathf.FloorToInt(BaseValueCharacterGoods - (BaseValueCharacterGoods * (goodsPriceModifier / 2f)));
						//the value of currency is not affected by reputation or skill
						TotalValuePlayerGoods += PlayerGoodsBank.BaseCurrencyValue;
						TotalValueCharacterGoods += CharacterGoodsBank.BaseCurrencyValue;
						ReputationPriceModifier = repPriceModifier;
						SkillPriceModifier = skillPriceModifier;
						FinalPriceModifier = goodsPriceModifier;

						RefreshAction.SafeInvoke();
				}
				//an item that will be added directly to the goods
				public IWIBase CharacterStartupItem;
				public IWIBase PlayerStartupItem;
				public WIStack CharacterStartupStack;

				public bool HasCharacterStartupItem {
						get {
								return CharacterStartupItem != null;
						}
				}

				public bool HasPlayerStartupItem {
						get {
								return PlayerStartupItem != null;
						}
				}

				public bool IsActive = false;
				public bool MadeTradeThisSession = false;
				public Character BarteringCharacter;

				public bool IsBarteringWithCharacter {
						get {
								return BarteringCharacter != null;
						}
				}

				public float BaseValuePlayerGoods;
				public float BaseValueCharacterGoods;
				public int TotalValuePlayerGoods;
				public int TotalValueCharacterGoods;
				public float ReputationPriceModifier = 0f;
				public float SkillPriceModifier = 0f;
				public float FinalPriceModifier = 0f;
				public bool ZeroCostMode = false;
				public Barter BarterManager;
				public IInventory PlayerInventory;
				public IInventory CharacterInventory;
				public int CurrentPlayerStackEnablerIndex = 0;
				public int CurrentCharacterStackEnablerIndex = 0;
				public WIStackEnabler CurrentCharacterStackEnabler;
				public WIStackEnabler CurrentPlayerStackEnabler;
				public List <BarterGoods> PlayerGoods;
				public List <BarterGoods> CharacterGoods;

				public IBank PlayerBank { get { return PlayerInventory.InventoryBank; } }

				public IBank CharacterBank { get { return CharacterInventory.InventoryBank; } }

				public IBank PlayerGoodsBank;
				public IBank CharacterGoodsBank;
				protected bool mMakingTrade = false;
		}

		public class BarterGoods : Dictionary <WIStack, int>
		{
				public int NumItems {
						get {
								int numItems = 0;
								foreach (int itemNum in Values) {
										numItems += itemNum;
								}
								return numItems;
						}
				}

				public WIStack TopItemStack {
						get {
								if (Count > 0) {
										return Keys.First();
								}
								return null;
						}
				}

				public IWIBase TopItem {
						get {
								IWIBase topItem = null;
								if (Count > 0) {
										WIStack firstStack = Keys.First();
										if (firstStack.HasTopItem) {
												topItem = firstStack.TopItem;
										}
								}
								return topItem;
						}
				}
		}
}
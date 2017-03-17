using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;
using Frontiers.World.WIScripts;

namespace Frontiers.GUI
{
		public class GUICraftingInterface : GUIObject, IStackOwner, IGUITabOwner, IGUITabPageChild
		{
				#region IStackOwner implementation

				public string StackName { get { return "Crafting"; } }

				public string FileName { get { return "Crafting"; } }

				public string DisplayName { get { return "Crafting"; } }

				public string QuestName { get { return string.Empty; } }

				public int GUIEditorID {
						get {
								if (InventoryInterface != null) {
										return InventoryInterface.GUIEditorID;
								}
								return -1;
						}
				}

				public WISize Size { get { return WISize.Huge; } }

				public WorldItem worlditem { get { return null; } }

				public bool IsWorldItem { get { return false; } }

				public bool UseRemoveItemSkill(HashSet <string> removeItemSkillNames, ref IStackOwner useTarget)
				{
						useTarget = null;
						return false;
				}

				public List <string> RemoveItemSkills { get { return new List <string>(); } }

				#endregion

				public WIBlueprint Blueprint {
						get {
								return mBlueprint;
						}set {
								mBlueprint = value;
						}
				}

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						/*FrontiersInterface.Widget w = new FrontiersInterface.Widget(flag);
						for (int i = 0; i < Squares.Count; i++) {
								w.Collider = Squares[i].Collider;
								w.SearchCamera = NGUICamera;
								currentObjects.Add(w);
						}
						w.Collider = ResultSquare.Collider;
						w.SearchCamera = NGUICamera;
						currentObjects.Add(w);

						//w.Collider = CraftOneButton.Collider;
						w.SearchCamera = NGUICamera;
						currentObjects.Add(w);
						//Tabs.GetActiveInterfaceObjects(currentObjects, flag);*/
				}

				public GUITabPage CraftingTabPage;
				public GUIInventoryInterface InventoryInterface;
				public CraftingItem CraftingWorldItem;
				public GUIBlueprintSelector BlueprintSelector;
				public GameObject GatherSuppliesButton;
				public UILabel BlueprintDescriptionLabel;
				public UILabel ContentsListLabel;
				public GUITabs Tabs;
				public List <InventorySquareDisplay> Squares = new List <InventorySquareDisplay>();
				protected WIBlueprint mBlueprint;
				public UISprite CanCraftArrow;
				public UISprite HasCraftedArrow;
				public float CanCraftArrowAlphaTarget = 0f;
				public float HasCraftedArrowAlphaTarget = 0f;
				public Color CanCraftArrowColorTarget = Color.white;
				public Color HasCraftedArrowColorTarget = Color.white;

				public Action OnShow { get; set; }

				public Action OnHide { get; set; }

				public bool Visible { get { return CraftingTabPage.Selected; } }

				public bool CanShowTab(string tabName, GUITabs tabs)
				{
						return true;
				}

				public override void Initialize(string argument)
				{
						base.Initialize(argument);
						Tabs.Initialize(this);
				}

				public bool HasBlueprint {
						get {
								return Blueprint != null && !Blueprint.IsEmpty;
						}
				}

				public bool HasCraftingItem {
						get {
								return CraftingWorldItem != null;
						}
				}

				public bool SkillRequirementsMet {
						get {
								if (mRequiredSkill == null) {
										return false;
								}
								return mRequiredSkill.HasBeenLearned;
						}
				}

				public bool MaterialRequirementsMet {
						get {
								if (SkillRequirementsMet) {
										return mRequiredSkill.CheckRequirements(Blueprint, Rows, ResultSquare, ref NumCraftableItems);
								}
								return false;
						}
				}

				public List <InventorySquareCrafting> CraftingSquaresRow0 = new List <InventorySquareCrafting>();
				public List <InventorySquareCrafting> CraftingSquaresRow1 = new List <InventorySquareCrafting>();
				public List <InventorySquareCrafting> CraftingSquaresRow2 = new List <InventorySquareCrafting>();
				public List <List <InventorySquareCrafting>> Rows = new List<List<InventorySquareCrafting>>();
				public InventorySquareCraftingResult ResultSquare;
				public int NumCraftableItems = 0;
				public UISlicedSprite SkillIconBackground;
				public UISprite SkillIconSprite;
				public UILabel ActivityTypeLabel;
				public UIButton CraftOneButton;
				public UIButton CraftAllButton;
				public UILabel CraftOneButtonLabel;
				public UILabel CraftAllButtonLabel;
				public UILabel SkillRequirementsLabel;
				protected WIStackContainer CraftingContainer;

				public void Hide()
				{
						SendItemsBackToInventory();
						//CraftingWorldItem = null;
						if (mRequiredSkill != null && mRequiredSkill.IsInUse) {
								mRequiredSkill.Cancel();
						}
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = false;
						}
						//OnHide.SafeInvoke ();
						//mRequiredSkill = null;
				}

				public void Show()
				{
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = true;
						}
						//OnShow.SafeInvoke ();
						if (GameManager.Is(FGameState.InGame)) {
								//Debug.Log ("Enabling crafting");
								RefreshRequest();
						}
				}

				public void Start()
				{
						CraftingContainer = Stacks.Create.StackContainer(this, WIGroups.Get.Player);
						CraftingContainer.RefreshAction += RefreshRequest;
						ResultSquare.RefreshAction += RefreshRequest;

						//temporarily display gather supplies button
						GatherSuppliesButton.gameObject.SetActive (false);

						CraftingSquaresRow0[0].SetStack(CraftingContainer.StackList[0]);
						CraftingSquaresRow0[0].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow0[1].SetStack(CraftingContainer.StackList[1]);
						CraftingSquaresRow0[1].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow0[2].SetStack(CraftingContainer.StackList[2]);
						CraftingSquaresRow0[2].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;

						CraftingSquaresRow1[0].SetStack(CraftingContainer.StackList[3]);
						CraftingSquaresRow1[0].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow1[1].SetStack(CraftingContainer.StackList[4]);
						CraftingSquaresRow1[1].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow1[2].SetStack(CraftingContainer.StackList[5]);
						CraftingSquaresRow1[2].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;

						CraftingSquaresRow2[0].SetStack(CraftingContainer.StackList[6]);
						CraftingSquaresRow2[0].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow2[1].SetStack(CraftingContainer.StackList[7]);
						CraftingSquaresRow2[1].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;
						CraftingSquaresRow2[2].SetStack(CraftingContainer.StackList[8]);
						CraftingSquaresRow2[2].BlueprintPotentiallyChanged += OnBlueprintPotentiallyChanged;

						Rows.Add(CraftingSquaresRow0);
						Rows.Add(CraftingSquaresRow1);
						Rows.Add(CraftingSquaresRow2);

						//make sure to add these in the right order
						//left to right, top to down
						//otherwise the patterns we generate won't match
						//TODO do we really need all these different lists
						mPatternSquares.AddRange(CraftingSquaresRow0);
						mPatternSquares.AddRange(CraftingSquaresRow1);
						mPatternSquares.AddRange(CraftingSquaresRow2);

						Squares.Add(CraftingSquaresRow0[0]);
						Squares.Add(CraftingSquaresRow0[1]);
						Squares.Add(CraftingSquaresRow0[2]);
						Squares.Add(CraftingSquaresRow1[0]);
						Squares.Add(CraftingSquaresRow1[1]);
						Squares.Add(CraftingSquaresRow1[2]);
						Squares.Add(CraftingSquaresRow2[0]);
						Squares.Add(CraftingSquaresRow2[1]);
						Squares.Add(CraftingSquaresRow2[2]);
						Squares.Add(ResultSquare);

						for (int i = 0; i < Squares.Count; i++) {
								//to make sure menus show up correctly etc
								Squares[i].NGUICamera = InventoryInterface.NGUICamera;
						}
				}

				public override void Awake()
				{
						CraftingTabPage = gameObject.GetComponent <GUITabPage>();
				}

				protected void OnClickGatherSuppliesButton()
				{
						if (HasBlueprint) {

						}
				}

				protected override void OnRefresh()
				{
						if (!GameManager.Is(FGameState.InGame | FGameState.GamePaused)) {
								//Debug.Log ("Not in game or paused, so not refreshing");
								return;
						}

						HasCraftedArrowColorTarget = Colors.Get.MessageInfoColor;
						CanCraftArrowColorTarget = Colors.Get.MessageInfoColor;
						HasCraftedArrowAlphaTarget = 0.25f;
						CanCraftArrowAlphaTarget = 0.25f;
						//GatherSuppliesButton.SendMessage("SetDisabled");

						//Debug.Log ("Refreshing crafting interface");
						Skill skillLookup = null;
						/*
						bool resultRequirementsMet = false;
						int resultNumItemsPossible = 0;
						int resultNumCraftableItems = 0;
						*/
						if (!InventoryInterface.IsCrafting) {
								if (!HasCraftingItem) {
										FindCraftingItemInFocus();//just in case
								}

								if (HasCraftingItem) {
										if (mRequiredSkill == null || mRequiredSkill.name != CraftingWorldItem.SkillToUse) {
												if (Skills.Get.SkillByName(CraftingWorldItem.SkillToUse, out skillLookup)) {
														mRequiredSkill = skillLookup as CraftSkill;
												}
										}
								} else {
										//we can ONLY craft without a crafting item
										Skills.Get.SkillByName("Craft", out skillLookup);
										mRequiredSkill = skillLookup as CraftSkill;
								}

								SkillRequirementsLabel.text = string.Empty;
			
								if (!SkillRequirementsMet) {
										SkillRequirementsLabel.text = "You don't have the required skill: " + mRequiredSkill.DisplayName;
								}

								SkillIconSprite.atlas = Mats.Get.IconsAtlas;
								SkillIconSprite.spriteName = mRequiredSkill.Info.IconName;
								SkillIconSprite.color = mRequiredSkill.SkillIconColor;
								SkillIconBackground.color = mRequiredSkill.SkillBorderColor;

								CraftOneButtonLabel.text = mRequiredSkill.Extensions.CraftOneDescription;
								CraftAllButtonLabel.text = mRequiredSkill.Extensions.CraftAllDescription;
								BlueprintSelector.BlueprintCategory = mRequiredSkill.name;
								ActivityTypeLabel.text = mRequiredSkill.DisplayName;

								//first round of blueprint checking
								if (HasBlueprint) {
										if (Blueprint.RequiredSkill != mRequiredSkill.name) {
												if (!HasCraftingItem) {
														SkillRequirementsLabel.text = "This skill requires a crafting item";
												}
												Blueprint = null;
										}
								}

								//now that we've checked, try again
								if (HasBlueprint) {
										//GatherSuppliesButton.SendMessage("SetEnabled");
										CanCraftArrowAlphaTarget = 0.5f;
										CanCraftArrowColorTarget = Colors.Get.MessageDangerColor;

										//Debug.Log ("We have a blueprint");
										ResultSquare.CraftedItemTemplate = Blueprint.GenericResult;
										BlueprintDescriptionLabel.text = Blueprint.CleanName;
										WorldItem prefab = null;
										if (WorldItems.Get.PackPrefab(Blueprint.GenericResult.PackName, Blueprint.GenericResult.PrefabName, out prefab)) {
												BlueprintDescriptionLabel.text = prefab.DisplayName;
												if (!string.IsNullOrEmpty(prefab.Props.Global.ExamineInfo.StaticExamineMessage)) {
														BlueprintDescriptionLabel.text += " - " + prefab.Props.Global.ExamineInfo.StaticExamineMessage;
												}
										}
										ContentsListLabel.text = "Contents:\n" + Blueprint.ContentsList;
										if (Blueprint.RequiredSkill == mRequiredSkill.name) {
												//do we have the skill we need to craft?
												if (mRequiredSkill == null || mRequiredSkill.name != Blueprint.RequiredSkill) {
														Skills.Get.SkillByName(Blueprint.RequiredSkill, out skillLookup);
														mRequiredSkill = skillLookup as CraftSkill;
												}
												if (!SkillRequirementsMet) {
														SkillRequirementsLabel.text = "You don't have the required skill: " + mRequiredSkill.DisplayName;
												}

												if (MaterialRequirementsMet) {

														//we haven't crafted anything yet
														CanCraftArrowAlphaTarget = 1f;
														CanCraftArrowColorTarget = Colors.Get.MessageSuccessColor;
														HasCraftedArrowColorTarget = Colors.Get.MessageSuccessColor;

														SetCraftOneButton(true);
														if (NumCraftableItems > 1) {
																SetCraftAllButton(true);
														} else {
																SetCraftAllButton(false);
														}
												} else {
														SetCraftOneButton(false);
														SetCraftAllButton(false);
												}
										}
								} else {
										//Debug.Log ("We have no blueprint. Blueprint null? " + (Blueprint == null).ToString ());
										//we have no blueprint at this stage
										Blueprint = null;
										//don't clear the crafted item template
										//we may still have to pick it up
										//ResultSquare.CraftedItemTemplate = null;
										ResultSquare.RequirementsMet = false;
										ResultSquare.NumItemsPossible = 0;
										ContentsListLabel.text = "(No blueprint selected)";
										BlueprintDescriptionLabel.text = string.Empty;
										SetCraftOneButton(false);
										SetCraftAllButton(false);
								}
						}


						UpdateRowDisplay(CraftingSquaresRow0);
						UpdateRowDisplay(CraftingSquaresRow1);
						UpdateRowDisplay(CraftingSquaresRow2);

						if (ResultSquare.NumItemsCrafted > 0) {
								HasCraftedArrowAlphaTarget = 1f;
								HasCraftedArrowColorTarget = Colors.Get.MessageSuccessColor;
								CanCraftArrowColorTarget = Colors.Get.MessageInfoColor;
						}
				}

				public bool FindCraftingItemInFocus()
				{
						if (Player.Local.Surroundings.IsWorldItemInPlayerFocus) {
								CraftingItem newCraftingItem = null;
								if (Player.Local.Surroundings.WorldItemFocus.worlditem.Is <CraftingItem>(out newCraftingItem)) {
										return true;
								}
						}
						return false;
				}

				public void OnClickCraftOneButton()
				{
						if (!mRequiredSkill.IsInUse) {
								StartCoroutine(CraftOverTime(1));
						}
				}

				public void OnClickCraftAllButton()
				{
						if (!mRequiredSkill.IsInUse) {
								StartCoroutine(CraftOverTime(NumCraftableItems));
						}
				}

				public void OnBlueprintPotentiallyChanged ( ) {
						Debug.Log("Blueprint potentially changed");
						//set blueprint to null in case we don't find a new one 
						Blueprint = null;
						//generate a pattern based on the arrangement of items
						//use that pattern to look up potential matches
						//use the blueprints skill to find a real match
						int pattern = 0;
						for (int i = 0; i < mPatternSquares.Count; i++) {
								if (mPatternSquares[i].Stack.HasTopItem) {
										pattern |= 1 << i;
								}
						}
						mPotentialMatches.Clear();
						int numCraftableItems = 0;
						if (Blueprints.Get.BlueprintsByPattern(pattern, mPotentialMatches)) {
								Blueprint = null;
								//we've found some potential matches
								//see if any actually match
								for (int i = 0; i < mPotentialMatches.Count; i++) {
										WIBlueprint potentialMatch = mPotentialMatches[i];
										bool matches = true;
										Debug.Log("Checking potential match " + potentialMatch.Name);
										//check each square
										for (int s = 0; s < mPatternSquares.Count; s++) {
												//we only have to check squares that aren't null
												//if they're null the pattern has ruled them out already
												if (mPatternSquares[s].HasStack && mPatternSquares[s].Stack.HasTopItem) {
														//here we perform an actual strictness check
														//if we blow it, it doesn't match
														//just pass '1' to the requirements
														if (!CraftSkill.AreRequirementsMet(
																    mPatternSquares[s].Stack.TopItem,
																    potentialMatch.Rows[s],
																    potentialMatch.Strictness,
																    mPatternSquares[s].Stack.NumItems,
																    out numCraftableItems)) {
																matches = false;
																break;
														}
												}
										}

										if (matches) {
												Debug.Log("MATCH! setting blueprint to " + potentialMatch.Name);
												//hooray, we're done
												OnSelectBlueprint(potentialMatch);
												break;
										}
								}
						} else {
								//if we didn't find ANY blueprints that match
								//just clear the blueprint
								RefreshRequest();
						}
				}

				public void OnSelectBlueprint(WIBlueprint blueprint)
				{
						if (ResultSquare.NumItemsCrafted > 0) {
								GUIManager.PostWarning("You have to remove your crafted items first");
								return;
						}

						Blueprint = blueprint;

						mRequiredSkill.LoadBlueprintsRows(Rows, Blueprint, ResultSquare);

						SetCraftOneButton(false);
						SetCraftAllButton(false);

						if (Tabs.SelectedTab != "Craft") {
								Tabs.SetSelection("Craft");
						}
						RefreshRequest();
				}

				protected void SetCraftAllButton(bool enabled)
				{
						//Debug.Log ("Setting craft all to " + enabled);
						if (enabled) {
								CraftAllButton.GetComponent<Collider>().enabled = true;
								CraftAllButton.GetComponent <GUIButtonSetup>().SetEnabled();
						} else {
								CraftAllButton.GetComponent<Collider>().enabled = false;
								CraftAllButton.GetComponent <GUIButtonSetup>().SetDisabled();
						}
				}

				protected void SetCraftOneButton(bool enabled)
				{
						//Debug.Log ("Setting craft one to " + enabled);
						if (enabled) {
								CraftOneButton.GetComponent<Collider>().enabled = true;
								CraftOneButton.GetComponent <GUIButtonSetup>().SetEnabled();
						} else {
								CraftOneButton.GetComponent<Collider>().enabled = false;
								CraftOneButton.GetComponent <GUIButtonSetup>().SetDisabled();
						}
				}

				protected void SendItemsBackToInventory()
				{
						if (GameManager.Is(FGameState.InGame)) {
								SendRequirementsRowBackToInventory(CraftingSquaresRow0);
								SendRequirementsRowBackToInventory(CraftingSquaresRow1);
								SendRequirementsRowBackToInventory(CraftingSquaresRow2);
								WIStackError error = WIStackError.None;
						}
				}

				protected void SendRequirementsRowBackToInventory(List <InventorySquareCrafting> row)
				{
						for (int i = 0; i < row.Count; i++) {
								InventorySquareCrafting square = row[i];
								if (square.HasStack && square.Stack.NumItems > 0) {
										WIStackError error = WIStackError.None;
										Player.Local.Inventory.AddItems(square.Stack, ref error);
								}
						}
				}

				protected void UpdateRowDisplay(List <InventorySquareCrafting> row)
				{
						for (int i = 0; i < row.Count; i++) {
								InventorySquareCrafting square = row[i];
								if (!InventoryInterface.IsCrafting) {
										if (!HasBlueprint) {
												square.HasBlueprint = false;
												square.DisableForBlueprint();
										}
								}
								square.UpdateDisplay();
						}
				}

				public void SetCraftingItem(CraftingItem newCraftingItem)
				{
						//Debug.Log ("Setting crafting item");
						if (HasCraftingItem) {
								if (CraftingWorldItem != newCraftingItem) {
										//it's not the same one - is it the same type?
										CraftingItem oldCraftingItem = CraftingWorldItem;
										CraftingWorldItem = newCraftingItem;
										if (oldCraftingItem.SkillToUse != newCraftingItem.SkillToUse) {
												mRequiredSkill = null;
												Blueprint = null;
												RefreshRequest();
										}
								}
						} else {
								if (CraftingWorldItem != newCraftingItem) {
										CraftingWorldItem = newCraftingItem;
										RefreshRequest();
								}
						}
				}

				public void ClearCrafting()
				{
						//Debug.Log ("Clearing crafting");
						SendItemsBackToInventory();
						if (CraftingWorldItem != null) {
								CraftingWorldItem = null;
								Blueprint = null;
								RefreshRequest();
						}
				}

				protected IEnumerator CraftOverTime(int numToCraft)
				{
						SetCraftOneButton(false);
						SetCraftAllButton(false);

						InventoryInterface.IsCrafting = true;

						StartCoroutine(mRequiredSkill.CraftItems(
								Blueprint,
								CraftingWorldItem,
								numToCraft,
								Rows,
								ResultSquare));

						while (mRequiredSkill.IsInUse) {
								if (!InventoryInterface.IsCrafting) {
										mRequiredSkill.Cancel();
								}
								yield return null;
						}

						InventoryInterface.IsCrafting = false;

						RefreshRequest();
						yield break;
				}

				protected void FillCraftingSquares()
				{
						foreach (InventorySquareCrafting square in CraftingSquaresRow0) {
								if (square.EnabledForBlueprint) {
										StackItem stackItem = square.RequiredItemTemplate.ToStackItem();
										WIStackError error = WIStackError.None;
										Stacks.Push.Item(square.Stack, stackItem, ref error);
								}
						}
						foreach (InventorySquareCrafting square in CraftingSquaresRow1) {
								if (square.EnabledForBlueprint) {
										StackItem stackItem = square.RequiredItemTemplate.ToStackItem();
										WIStackError error = WIStackError.None;
										Stacks.Push.Item(square.Stack, stackItem, ref error);
								}
						}
						foreach (InventorySquareCrafting square in CraftingSquaresRow2) {
								if (square.EnabledForBlueprint) {
										StackItem stackItem = square.RequiredItemTemplate.ToStackItem();
										WIStackError error = WIStackError.None;
										Stacks.Push.Item(square.Stack, stackItem, ref error);
								}
						}
				}

				public void Update()
				{
						if (HasBlueprint && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.V) && Input.GetKey(KeyCode.B)) {
								FillCraftingSquares();
						}

						CanCraftArrow.color = Color.Lerp(CanCraftArrow.color, Colors.Alpha(CanCraftArrowColorTarget, Mathf.Lerp(CanCraftArrow.alpha, CanCraftArrowAlphaTarget, 0.125f)), 0.125f);
						HasCraftedArrow.color = Color.Lerp(HasCraftedArrow.color, Colors.Alpha(HasCraftedArrowColorTarget, Mathf.Lerp(HasCraftedArrow.alpha, HasCraftedArrowAlphaTarget, 0.125f)), 0.125f);
				}

				protected List <WIBlueprint> mPotentialMatches = new List<WIBlueprint> ();
				protected List <InventorySquareCrafting> mPatternSquares = new List<InventorySquareCrafting>();
				protected bool mRefreshing = false;
				protected CraftSkill mRequiredSkill = null;
				protected bool mHasBeenInitialized = false;
		}
}

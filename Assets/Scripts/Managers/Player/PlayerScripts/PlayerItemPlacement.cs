using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers
{
		public class PlayerItemPlacement : PlayerScript
		{
				public WorldItem CarryObject;
				public GameObject PlacementDoppleganger = null;
				public BoxCollider PlacementDopplegangerContainer;
				public Rigidbody PlacementDopplegangerRigidbody;
				public Bounds PlacementDopplegangerBounds = new Bounds();
				public List <Vector3> SmoothPlacementPoints = new List<Vector3>();
				public int MaxSmoothPlacementPoints = 8;
				public int LastSmoothPlacementIndex = 0;
				public bool UsingSkillList = false;
				public bool PlacementModeEnabled = false;
				public string PlacementErrorMessage = string.Empty;
				public bool PlacementResultsInDrop = false;
				public bool PlacementPermitted = false;
				public Receptacle PlacementPreferredReceptacle	= null;
				public ReceptaclePivot PlacementPreferredReceptaclePivot = null;
				public WorldItem PlacementPreferredWorldItem = null;
				public IItemOfInterest PlacementPreferredTerrain = null;
				public Vector3 PlacementPreferredPoint = Vector3.zero;
				public Vector3 PlacementPreferredNormal = Vector3.zero;
				public Vector3 PlacementPreferredPointSmooth = Vector3.zero;
				public IItemOfInterest PlacementPreferredObject = null;
				public float ThrowForce = 8.0f;
				public GameObject LastItemDropped;
				//TODO put this in globals
				public GameObject LastItemCarried;
				public GameObject LastItemPlaced;
				public Vector3 GrabberIdealPosition	= new Vector3(0.5f, -0.15f, 2.5f);
				public WorldItem ItemToPlace = null;
				public IItemOfInterest LastItemUsed;
				public double UseCoolDown;
				public double CarryCoolDown;
				public double DropCoolDown;
				public double PlaceCoolDown;
				protected double CooldownInterval = 0.1f;
				protected double CarryInterval = 0.1f;
				public double CarrySoFar = 0.0f;
				public double ItemPlaceMinTime = 0.5f;
				public double ItemPlaceStartTime = 0.0f;
				public float PlacementRotationOffset = 0f;
				public float PlacementRotationPerSelection = 15f;
				public static bool RemovingItemUsingSkill = false;
				[HideInInspector]
				public YesNoCancelDialogResult DiscardQuestItemDialogResult;

				public Vector3 DefaultDropPosition {
						get {
								return Player.Local.HeadPosition + (Player.Local.FocusVector * 1.5f);
						}
				}

				public bool IsCarryingSomething {
						get {
								return CarryObject != null;
						}
				}

				public bool PlacementPossible {
						get {
								return PlacementOnTerrainPossible || PlacementOnWorldItemPossible || PlacementResultsInDrop;
						}
				}

				public bool PlacementInReceptaclePossible {
						get {
								return PlacementPreferredReceptacle != null;
						}
				}

				public bool PlacementOnTerrainPossible {
						get {
								return PlacementPreferredTerrain != null;
						}
				}

				public bool PlacementOnWorldItemPossible {
						get {
								return PlacementPreferredWorldItem != null;
						}
				}

				public bool IsPlacingSomething {
						get {
								return ItemToPlace != null;
						}
				}

				public override bool LockQuickslots {
						get {
								return PlacementModeEnabled;// && ItemToPlace == Player.Local.Inventory.ActiveQuickslotItem;
						}
				}

				public override void OnGameStart()
				{
						GameObject pdc = new GameObject("PlacementDopplegangerContainer");
						pdc.layer = Globals.LayerNumWorldItemActive;
						PlacementDopplegangerContainer = pdc.AddComponent <BoxCollider>();
						PlacementDopplegangerRigidbody = pdc.AddComponent <Rigidbody>();
						PlacementDopplegangerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
						PlacementDopplegangerRigidbody.useGravity = false;
						PlacementDopplegangerRigidbody.detectCollisions = false;

						Player.Get.UserActions.Subscribe(UserActionType.ItemUse, new ActionListener(ItemUse));
						Player.Get.UserActions.Subscribe(UserActionType.ItemThrow, new ActionListener(ItemThrow));
						Player.Get.UserActions.Subscribe(UserActionType.ItemInteract, new ActionListener(ItemInteract));
						Player.Get.UserActions.Subscribe(UserActionType.ItemPlace, new ActionListener(ItemPlace));
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.SelectionNext, new ActionListener(SelectionNext));
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.SelectionPrev, new ActionListener(SelectionPrev));
						GUIInventoryInterface.Get.Subscribe(InterfaceActionType.ToggleInterface, new ActionListener(ToggleInterface));
						Player.Get.AvatarActions.Subscribe((AvatarAction.ItemAQIChange), new ActionListener(ItemAQIChange));

						DiscardQuestItemDialogResult = new YesNoCancelDialogResult();
						DiscardQuestItemDialogResult.MessageType = "Discard Mission Item";
						DiscardQuestItemDialogResult.Message = "You are about to discard a mission-related item. Are you sure you want to do this?";
						DiscardQuestItemDialogResult.CancelButton = false;
						DiscardQuestItemDialogResult.Result = DialogResult.None;

						enabled = true;
				}
				//do this in late update so we don't get herky-jerky motion with the doppleganger
				public void LateUpdate()
				{
						Player.HideCrosshair = true;

						if (!GameManager.Is(FGameState.InGame))
								return;

						//just in case we've moved it somewhere
						player.GrabberTargetObject.localPosition = Vector3.Lerp(player.GrabberTargetObject.localPosition, GrabberIdealPosition, (float)(Frontiers.WorldClock.ARTDeltaTime * 16.0f));

						bool showDoppleganger = false;
						if (PlacementModeEnabled) {
								FindPreferredReceptacle();

								//TODO tie this to player strength?
								if (IsPlacingSomething) {
										if (PlacementPossible) {
												if (PlacementInReceptaclePossible) {
														//do nothing - the recepticle will be handling this on its own
														showDoppleganger = false;
												} else {
														//if we're placing something and it doesn't result in a drop and placement is possible
														//get a success-colored doppleganger
														showDoppleganger = true;
														Mats.Get.ItemPlacementOutlineMaterial.SetColor("_OutlineColor", Colors.Get.MessageSuccessColor);
														Mats.Get.ItemPlacementMaterial.SetColor("_TintColor", Colors.Get.MessageSuccessColor);
														if (PlacementResultsInDrop) {
																//if it'll drop, get a warning-colored doppleganger
																PlacementPreferredPoint = player.Grabber.Position;
																Mats.Get.ItemPlacementOutlineMaterial.SetColor("_OutlineColor", Colors.Get.MessageWarningColor);
																Mats.Get.ItemPlacementMaterial.SetColor("_TintColor", Colors.Get.MessageWarningColor);
																if (IsCarryingSomething) {
																		//if we're carrying something (as opposed to placing an equipped item)
																		//don't show the doppleganger, it'll just overlap with the carty item
																		showDoppleganger = false;
																}
														}
												}
										}
								}
						}

						if (showDoppleganger) {
								Player.HideCrosshair = false;
								//GET THE DOPPLEGANGER
								//put the doppleganger in player base so it doesn't rotate as we move
								if (PlacementDoppleganger == null) {
										PlacementDoppleganger = WorldItems.GetDoppleganger(ItemToPlace, Player.Get.transform, PlacementDoppleganger, WIMode.Placing, 1f);
										//dopplegangers will be placed at the item's regular pivot
										//this is ok for interfaces but we need to move the doppleganger so it's not intersecting with anything
										//because we use the doppleganger's position when placing it in the world
										PlacementDopplegangerContainer.size = ItemToPlace.BaseObjectBounds.size;
										PlacementDopplegangerContainer.center = ItemToPlace.BaseObjectBounds.center;//ItemToPlace.BasePivotOffset;
								}

								//TURN ON RIGIDBODY
								//if it's the first time, move it to our preferred position
								if (!PlacementDopplegangerRigidbody.detectCollisions) {
										PlacementDopplegangerContainer.transform.position = PlacementPreferredPoint;
										SmoothPlacementPoints.Clear();
										//ignore collisions with player, with the equipped worlditem, and so on
										//otherwise the rigidbody will bounce around as we try to place stuff
										Physics.IgnoreCollision(PlacementDopplegangerContainer, Player.Local.Controller);
										Physics.IgnoreCollision(PlacementDopplegangerContainer, Player.Local.EncountererObject.EncounterCollider);
										if (Player.Local.Tool.HasWorldItem) {
												for (int i = 0; i < Player.Local.Tool.ToolColliders.Count; i++) {
														if (Player.Local.Tool.ToolColliders[i].enabled) {
																Physics.IgnoreCollision(PlacementDopplegangerContainer, Player.Local.Tool.ToolColliders[i]);
														}
												}
										}

										if (IsCarryingSomething) {
												for (int i = 0; i < CarryObject.Colliders.Count; i++) {
														if (CarryObject.Colliders[i].enabled) {
																Physics.IgnoreCollision(PlacementDopplegangerContainer, CarryObject.Colliders[i]);
														}
												}
										}
								}
								//use velocity to move it so we get collisions
								PlacementDopplegangerRigidbody.detectCollisions = true;
								PlacementDopplegangerRigidbody.isKinematic = false;

								//MOVE THE RIGIDBODY IF IT'S BEEN STUCK TOO LONG
								if (mRigidBodyStuck && (WorldClock.RealTime - mRigidBodyStuckStartTime) > mRigidBodyStuckMaxTime) {
										PlacementDopplegangerContainer.transform.position = PlacementPreferredPoint;
										mRigidBodyStuck = false;
								} else {
										Vector3 velocity = (PlacementPreferredPoint - PlacementDopplegangerRigidbody.position) * 10f;
										PlacementDopplegangerRigidbody.velocity = velocity;
								}

								//SMOOTH POSITION
								//add our smooth positions and set the doppleganger to the smoothed position
								//don't add existing points - there's a chance of duplicate points since the coroutine updates slowly
								Vector3 rbPosition = PlacementDopplegangerRigidbody.position;

								//ROTATE THE RIGIDBODY BY PLACEMENTPREFERREDOFFSET - don't bother to smooth this
								PlacementDopplegangerRigidbody.rotation = Quaternion.Euler(0f, PlacementRotationOffset, 0f);

								//CHECK IF WE'RE STUCK
								if (!mRigidBodyStuck) {
										if (Vector3.Distance(PlacementPreferredPoint, rbPosition) > 1.0f) {
												//if the rigid body is closer to the player than the preferred point then we're fine
												//otherwise w'eve gotten stuck
												if (Vector3.Distance(Player.Local.HeadPosition, PlacementPreferredPoint) < Vector3.Distance(rbPosition, PlacementPreferredPoint)) {
														mRigidBodyStuck = true;
														mRigidBodyStuckStartTime = WorldClock.RealTime;
												}
										}
								}

								if (SmoothPlacementPoints.Count < MaxSmoothPlacementPoints && !SmoothPlacementPoints.Contains(rbPosition)) {
										//if we haven't reached the max steps yet add the next point
										SmoothPlacementPoints.Add(rbPosition);
										LastSmoothPlacementIndex = 0;
								} else {
										//otherwise replace the next point in the list
										LastSmoothPlacementIndex = SmoothPlacementPoints.NextIndex(LastSmoothPlacementIndex);
										SmoothPlacementPoints[LastSmoothPlacementIndex] = rbPosition;
								}
								for (int i = 0; i < SmoothPlacementPoints.Count; i++) {
										if (i == 0) {
												PlacementPreferredPointSmooth = SmoothPlacementPoints[i];
										} else {
												PlacementPreferredPointSmooth = Vector3.Lerp(PlacementPreferredPointSmooth, SmoothPlacementPoints[i], 1.0f / MaxSmoothPlacementPoints);
										}
								}
								//now we move the doppleganger into position
								//we'll need to apply an offset just in case
								PlacementDoppleganger.transform.position = PlacementPreferredPointSmooth;
								PlacementDoppleganger.transform.rotation = PlacementDopplegangerRigidbody.rotation;

								if (PlacementResultsInDrop) {
										if (!mShowingHUD) {
												mShowingHUD = true;
												GUIHud.Get.ShowAction(UserActionType.ItemUse, "Drop", PlacementDoppleganger.transform, GameManager.Get.GameCamera);
										}
								} else {
										if (!mShowingHUD) {
												mShowingHUD = true;
												GUIHud.Get.ShowActions (UserActionType.ItemUse, InterfaceActionType.SelectionNext, "Place", "Rotate", PlacementDoppleganger.transform, GameManager.Get.GameCamera);
										}
								}
						} else {
								mShowingHUD = false;
								PlacementDopplegangerRigidbody.detectCollisions = false;
								PlacementDopplegangerRigidbody.isKinematic = true;
								WorldItems.ReturnDoppleganger(PlacementDoppleganger);
								//start over with our smooth placement
								SmoothPlacementPoints.Clear();
								PlacementPreferredPointSmooth = player.Grabber.Position;
						}
				}

				protected bool mShowingHUD = false;
				protected bool mRigidBodyStuck = false;
				protected double mRigidBodyStuckStartTime = 0f;
				protected double mRigidBodyStuckMaxTime = 0.5f;

				public bool SelectionNext(double timeStamp)
				{
						if (IsPlacingSomething) {
								PlacementRotationOffset += PlacementRotationPerSelection;
						}
						return true;
				}

				public bool SelectionPrev(double timeStamp)
				{
						if (IsPlacingSomething) {
								PlacementRotationOffset -= PlacementRotationPerSelection;
						}
						return true;
				}

				public bool ToggleInterface(double timeStamp)
				{
						//this is to prevent accidentally spamming the ItemUse key
						UseCoolDown = WorldClock.RealTime + CooldownInterval;
						return true;
				}

				public bool ItemAQIChange(double timeStamp)
				{
						PlacementModeEnabled = false;
						return true;
				}

				public bool ItemInteract(double timeStamp)
				{
						if (DoingSomethingOverTime) {
								//if we're in the middle of an action, do nothing
								return true;
						}
						WorldItem worldItemInRange = null;
						WorldItemUsable usable = null;
						if (Player.Local.Surroundings.IsWorldItemInRange) {
								worldItemInRange = Player.Local.Surroundings.WorldItemFocus.worlditem;
								usable = worldItemInRange.gameObject.GetOrAdd<WorldItemUsable>();
								usable.RequirePlayerFocus = true;
								usable.ShowDoppleganger = true;

						} else if (Player.Local.Surroundings.IsTerrainInRange &&
						        Player.Local.Surroundings.TerrainFocus.IOIType == ItemOfInterestType.WorldItem) {
								worldItemInRange = Player.Local.Surroundings.TerrainFocus.worlditem;
								usable = worldItemInRange.gameObject.GetOrAdd<WorldItemUsable>();
								usable.RequirePlayerFocus = false;
								usable.ShowDoppleganger = false;
								//there might be a body of water we want to drink from
						} else {
								return true;
						}
						GUIOptionListDialog dialog = null;
						usable.TryToSpawn(true, out dialog);
						return true;
				}

				public bool ItemUse(double timeStamp)
				{
						if (DoingSomethingOverTime) {
								//if we're in the middle of an action, do nothing
								return true;
						}

						if (WorldClock.RealTime < UseCoolDown) {
								return true;
						}

						if (Player.Local.Surroundings.IsWorldItemInRange && LastItemUsed != null) {
								if (Player.Local.Surroundings.WorldItemFocus == LastItemUsed
								&& WorldClock.RealTime < UseCoolDown) {
										return true;
								}
						}

						UseCoolDown = WorldClock.RealTime + CooldownInterval;
						LastItemUsed = Player.Local.Surroundings.WorldItemFocus;//even if it's null that's fine

						if (PlacementModeEnabled) {
								if (Player.Local.Surroundings.IsReceptacleInPlayerFocus) {
										//recepticles handle their own business
										Player.Local.Surroundings.ReceptacleInPlayerFocus.worlditem.OnPlayerUse.SafeInvoke();
										PlacementModeEnabled = false;
								} else {
										if (IsPlacingSomething) {
												if (Player.Local.Tool.IsEquipped && ItemToPlace == Player.Local.Tool.worlditem) {
														PlaceOrDropEquippedItem();
												} else if (IsCarryingSomething && CarryObject == ItemToPlace) {
														PlaceOrDropCarriedItem();
												}

												GameObject.Destroy(PlacementDoppleganger);
												ItemToPlace = null;
												ItemPlaceStartTime = Mathf.Infinity;
												PlacementModeEnabled = false;
										}
								}
						} else {
								//deal with whatever we're carrying first
								if (FindObjectToPickUp()) {
										return true;
								}
						}
						return true;
				}

				public bool FindObjectToPickUp()
				{
						if (Player.Local.Surroundings.IsWorldItemInRange) {
								WorldItem worldItemInRange = Player.Local.Surroundings.WorldItemFocus.worlditem;
								if (worldItemInRange.Is <Character>()) {
										Player.Local.Focus.GetOrReleaseAttention(worldItemInRange);
										worldItemInRange.OnPlayerUse.SafeInvoke();
										//that's the only thing we can do with a character, so return
										return true;
								}

								//make sure we're not trying to interact with something we just dropped or threw
								if (LastItemPlaced == worldItemInRange.gameObject && WorldClock.RealTime < PlaceCoolDown
								|| LastItemCarried == worldItemInRange.gameObject && WorldClock.RealTime < CarryCoolDown
								|| LastItemDropped == worldItemInRange.gameObject && WorldClock.RealTime < DropCoolDown) {
										//return true, because if this is the case, we don't want to do anything else
										return true;
								}

								if (!ItemPickUp(worldItemInRange)) {
										//if we can't pick it up
										//interpret it as an interaction
										worldItemInRange.OnPlayerUse.SafeInvoke();
										return true;
								}
						} else if (Player.Local.Surroundings.IsTerrainInRange &&
						        Player.Local.Surroundings.TerrainFocus.IOIType == ItemOfInterestType.WorldItem) {
								//there might be a body of water we want to drink from
								Player.Local.Surroundings.TerrainFocus.worlditem.OnPlayerUse.SafeInvoke();
						}
						return false;
				}

				#region remove item with skill

				protected void UseSkillsToPickUpItem(WorldItem worlditemInRange)
				{
						if (UsingSkillList)
								return;

						mPickUpTarget = worlditemInRange;

						//add the option list we'll use to select the skill
						SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
						optionsList.Message = "Use a skill";
						optionsList.MessageType = "Pick up item";
						optionsList.FunctionName = "OnSelectRemoveSkill";
						optionsList.RequireManualEnable = false;
						optionsList.OverrideBaseAvailabilty = true;
						optionsList.FunctionTarget = gameObject;
						optionsList.ScreenTarget = transform;
						optionsList.ScreenTargetCamera = GameManager.Get.GameCamera.camera;
						mRemoveSkillList.Clear();
						mRemoveSkillList.AddRange(Skills.Get.SkillsByName(mRemoveItemSkillNames));
						foreach (Skill removeItemSkill in mRemoveSkillList) {
								optionsList.AddOption(removeItemSkill.GetListOption(mSkillUseTarget.worlditem));
						}
						optionsList.AddOption(new WIListOption("Cancel"));
						optionsList.ShowDoppleganger = false;
						GUIOptionListDialog dialog = null;
						if (optionsList.TryToSpawn(true, out dialog)) {
								UsingSkillList = true;
						}
				}

				public void OnSelectRemoveSkill(System.Object result)
				{
						UsingSkillList = false;

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
								//SKILL USE
								//getting here guarantees that:
								//a) our selected stack is empty and
								//b) our stack has item
								//so proceed as though we know those are true
								skillToUse.TryToRemoveItem(mSkillUseTarget, mPickUpTarget, Player.Local.Inventory, FinishUsingSkillToRemoveItem);
								//now we just have to wait!
								//the skill will move stuff around
								//refresh requests will be automatic
						}

						mRemoveItemSkillNames.Clear();
				}

				public System.Action FinishUsingSkillToRemoveItem;
				protected HashSet <string> mRemoveItemSkillNames = new HashSet <string>();
				protected List <Skill> mRemoveSkillList = new List <Skill>();
				protected IStackOwner mSkillUseTarget = null;
				protected WorldItem mPickUpTarget = null;

				#endregion

				public bool ItemPlace(double timeStamp)
				{
						if (DoingSomethingOverTime) {
								//if we're in the middle of an action, do nothing
								return true;
						}

						if (Player.Local.Inventory.SelectedStack.HasTopItem) {
								DropSelectedItems();
								return true;
						}

						if (PlacementModeEnabled) {
								//toggle it
								PlacementModeEnabled = false;
								return true;
						}

						if (IsCarryingSomething) {
								//this can't be disabled if we're carrying something
								//just set it to true and get out
								ItemToPlace = CarryObject;
								PlacementModeEnabled = true;
						} else if (Player.Local.Tool.IsEquipped) {
								//if we're not carrying something
								//then the equipped worlditem takes priority
								ItemToPlace = Player.Local.Tool.worlditem;
								PlacementModeEnabled = true;
						}
						return true;
				}

				public bool ItemThrow(double timeStamp)
				{
						if (DoingSomethingOverTime) {
								//if we're in the middle of an action, do nothing
								return true;
						}

						if (Player.Local.Inventory.SelectedStack.HasTopItem) {
								DropSelectedItems();
								return true;
						}

						if (Player.Local.Tool.IsEquipped) {
								ThrowEquippedItem();
								return true;
						}

						if (IsCarryingSomething) {
								ThrowCarriedItem();
						}
						return true;
				}

				public bool ItemPickUp(WorldItem item)
				{
						//if we've gotten this far we didn't place it, so pick it up instead
						WIStackError error = WIStackError.None;
						if (item.CanEnterInventory) {//quest items are automatically fine
								if (!item.Is <QuestItem>() && !item.Is <OwnedByPlayer>() && item.UseRemoveItemSkill(mRemoveItemSkillNames, ref mSkillUseTarget)) {
										UseSkillsToPickUpItem(item);
										return true;
								} else if (Player.Local.Inventory.CanItemFit(item)) {
										if (Player.Local.Inventory.AddItems(item, ref error)) {
												return true;
										} else if (item.CanBeCarried) {
												//try to carry it
												//add 'owned by player' script to make sure the player owns it
												item.Add("OwnedByPlayer");
												return ItemForceCarry(item);

										}
								} else if (item.CanBeCarried) {
										//try to carry it
										return ItemForceCarry(item);
								}
						}
						return false;
				}

				public void DropSelectedItems()
				{
						if (Player.Local.Inventory.SelectedStack.TopItem.Size == WISize.Tiny) {
								GUIManager.PostWarning("You can't drop tiny items");
								return;
						}

						if (!mDroppingSelectedItems) {
								mDroppingSelectedItems = true;
								StartCoroutine(DropSelectedItemssOverTime());
						}
				}

				public void PlaceOrDropEquippedItem()
				{
						if (!PlacementPermitted || !PlacementPossible) {
								GUIManager.PostWarning("Can't place " + ItemToPlace.DisplayName + ": " + PlacementErrorMessage);
								return;
						}

						if (!mPlacingOrDroppingEquippedItem) {
								mPlacingOrDroppingEquippedItem = true;
								StartCoroutine(PlaceOrDropEquippedItemOverTime());
						}
				}

				public bool PlaceOrDropCarriedItem()
				{
						if (!PlacementPermitted || !PlacementPossible) {
								GUIManager.PostWarning("Can't drop " + CarryObject.DisplayName);
								return false;
						}

						if (!mPlacingOrDroppingCarriedItem) {
								mPlacingOrDroppingCarriedItem = true;
								StartCoroutine(PlaceOrDropCarriedItemOverTime());
						}
						return true;
				}

				public void ItemDropAtFeet(IWIBase item)
				{
						WorldItem worlditem = null;
						if (item.IsWorldItem) {
								worlditem = item.worlditem;
						} else {
								WorldItems.CloneFromStackItem(item.GetStackItem(WIMode.World), WIGroups.Get.Player, out worlditem);
						}
						if (!worlditem.Is(WILoadState.Initialized)) {
								worlditem.Initialize();
						}
						worlditem.ActiveState = WIActiveState.Active;
						worlditem.Props.Local.FreezeOnStartup = false;
						worlditem.SetMode(WIMode.World);
						worlditem.tr.position = Player.Local.GrabberTargetObject.position;
						worlditem.tr.rotation = Player.Local.GrabberTargetObject.rotation;
						worlditem.LastActiveDistanceToPlayer = 0f;
				}

				public void ThrowEquippedItem()
				{
						if (!mThrowingEquippedItem) {
								mThrowingEquippedItem = true;
								StartCoroutine(ThrowEquippedItemOverTime());
						}
				}

				public bool ThrowCarriedItem()
				{
						if (!mThrowingCarriedItem) {
								mThrowingCarriedItem = true;
								StartCoroutine(ThrowCarriedItemOverTime());
								return true;
						}
						return false;
				}

				public bool ItemCarry(WorldItem item, bool forceCarry)
				{
						if (IsCarryingSomething) {
								if (!PlaceOrDropCarriedItem()) {
										return false;
								}
						}

						player.Grabber.Joint.connectedBody = item.rigidbody;
						player.Grabber.Joint.connectedAnchor = Vector3.zero;
						player.Grabber.Joint.anchor = Vector3.zero;
						player.Grabber.Position = item.Position;
						player.GrabberTargetObject.position = item.Position;
						//kludgey
						item.rigidbody.isKinematic = false;
						item.rigidbody.useGravity = false;

						item.OnPlayerCarry.SafeInvoke();
						item.RefreshHud();
						CarryObject = item;
						ItemToPlace = item;
						GUIManager.PostInfo("Carrying " + item.DisplayName);
						CarryCoolDown = WorldClock.RealTime + CooldownInterval;
						PlacementModeEnabled = true;
						return true;
				}

				public bool ItemForceCarry(WorldItem item)
				{
						return ItemCarry(item, true);
				}

				protected void FindPreferredReceptacle()
				{
						//order of prerefence:
						//placement of carried object
						//placement of equipped path editor
						//placement of equipped tool general
						PlacementPreferredReceptacle = null;
						PlacementPreferredReceptaclePivot = null;
						PlacementPreferredWorldItem = null;
						PlacementPreferredTerrain = null;
						PlacementPreferredObject = null;
						PlacementPermitted = false;
						PlacementResultsInDrop = true;
						PlacementErrorMessage = string.Empty;

						#region carrying something
						if (IsCarryingSomething) {
								if (Player.Local.Surroundings.IsReceptacleInPlayerFocus) {
										Receptacle recepticle = Player.Local.Surroundings.ReceptacleInPlayerFocus;
										if (recepticle.HasRoom(CarryObject, out PlacementPreferredReceptaclePivot) &&
										recepticle.IsObjectPermitted(CarryObject, PlacementPreferredReceptaclePivot.Settings)) {
												//we should affix the doppleganger to a pivot
												//the doppleganger color should be green
												PlacementPermitted = true;
												PlacementPreferredPoint = PlacementPreferredReceptaclePivot.tr.position;
												PlacementPreferredNormal = PlacementPreferredReceptaclePivot.tr.rotation.eulerAngles;
												PlacementResultsInDrop = false;
										}
										//if we can't place it, but it's placeable, but we CAN'T drop it, we're finished here.
										if (!CarryObject.CanBeDropped) {
												PlacementResultsInDrop = false;
												return;
										}
								} else {
										if (Player.Local.Surroundings.IsWorldItemInRange) {
												WorldItem worlditemInFocus = Player.Local.Surroundings.WorldItemFocus.worlditem;
												if (worlditemInFocus.Mode == WIMode.Frozen) {
														PlacementPreferredWorldItem = worlditemInFocus;
														PlacementPreferredPoint = Player.Local.Surroundings.WorldItemFocusHitInfo.point;
														PlacementPreferredNormal = Player.Local.Surroundings.WorldItemFocusHitInfo.normal;
														PlacementPreferredObject = PlacementPreferredWorldItem;

														if (CarryObject.CanBePlacedOn(PlacementPreferredObject, PlacementPreferredPoint, PlacementPreferredNormal, ref PlacementErrorMessage)) {
																PlacementPermitted = true;
																PlacementResultsInDrop = false;
														}
												}
										} else if (Player.Local.Surroundings.IsTerrainInRange) {
												PlacementPreferredTerrain = Player.Local.Surroundings.TerrainFocus;
												PlacementPreferredPoint = Player.Local.Surroundings.TerrainFocusHitInfo.point;
												PlacementPreferredNormal = Player.Local.Surroundings.TerrainFocusHitInfo.normal;
												PlacementPreferredObject = PlacementPreferredTerrain;

												if (CarryObject.CanBePlacedOn(PlacementPreferredObject, PlacementPreferredPoint, PlacementPreferredNormal, ref PlacementErrorMessage)) {
														PlacementPermitted = true;
														PlacementResultsInDrop = false;
												}
										} else if (CarryObject.CanBeDropped) {
												//if there's nothing in range, we just drop
												PlacementPermitted = true;
												PlacementResultsInDrop = true;
										} else {
												PlacementPermitted = false;
												PlacementResultsInDrop = false;
										}
								}
								//check to see if the thing we're placing will actually drop
								if (PlacementPermitted && !PlacementResultsInDrop) {	//first check the normal - the placement point may be on a wall or something
										if (Vector3.Dot(Vector3.up, PlacementPreferredNormal) < 0f) {	//turn off this collider before doing the raycast
												PlacementDopplegangerContainer.enabled = false;
												Vector3 raycastDown = PlacementPreferredPoint;
												raycastDown.y = raycastDown.y - PlacementDopplegangerContainer.bounds.extents.y;
												raycastDown.x = raycastDown.x - PlacementDopplegangerContainer.bounds.extents.x;
												raycastDown.z = raycastDown.z - PlacementDopplegangerContainer.bounds.extents.z;
												if (Physics.Raycast(raycastDown, Vector3.down, out mDropHit, 0.05f, Globals.LayersActive)) {
														PlacementResultsInDrop = true;
												}
												PlacementDopplegangerContainer.enabled = true;
										}
								}
								//if we're carrying something, we're done even if we can't place it
								return;
						}
						#endregion

						#region item general
						if (Player.Local.Tool.IsEquipped) {
								if (Player.Local.Surroundings.IsReceptacleInPlayerFocus) {
										Receptacle recepticle = Player.Local.Surroundings.ReceptacleInPlayerFocus;
										if (recepticle.HasRoom(Player.Local.Tool.worlditem, out PlacementPreferredReceptaclePivot) &&
										recepticle.IsObjectPermitted(Player.Local.Tool.worlditem, PlacementPreferredReceptaclePivot.Settings)) {
												//we should affix the doppleganger to a pivot
												//the doppleganger color should be green
												PlacementPreferredReceptacle = recepticle;
												PlacementPermitted = true;
												PlacementPreferredPoint = PlacementPreferredReceptaclePivot.tr.position;
												PlacementPreferredNormal = PlacementPreferredReceptaclePivot.tr.rotation.eulerAngles;
												PlacementResultsInDrop = false;
										}
								} else {
										if (Player.Local.Surroundings.IsWorldItemInRange) {
												WorldItem worlditemInFocus = Player.Local.Surroundings.WorldItemFocus.worlditem;
												if (worlditemInFocus.Mode == WIMode.Frozen) {
														PlacementPreferredWorldItem = worlditemInFocus;
														PlacementPreferredPoint = Player.Local.Surroundings.WorldItemFocusHitInfo.point;
														PlacementPreferredNormal = Player.Local.Surroundings.WorldItemFocusHitInfo.normal;
														PlacementPreferredObject = PlacementPreferredWorldItem;

														if (Player.Local.Tool.worlditem.CanBePlacedOn(PlacementPreferredObject, PlacementPreferredPoint, PlacementPreferredNormal, ref PlacementErrorMessage)) {
																PlacementPermitted = true;
																PlacementResultsInDrop = false;
														}
												}
										} else if (Player.Local.Surroundings.IsTerrainInRange) {
												PlacementPreferredTerrain = Player.Local.Surroundings.TerrainFocus;
												PlacementPreferredPoint = Player.Local.Surroundings.TerrainFocusHitInfo.point;
												PlacementPreferredNormal = Player.Local.Surroundings.TerrainFocusHitInfo.normal;
												PlacementPreferredObject = PlacementPreferredTerrain;

												if (Player.Local.Tool.worlditem.CanBePlacedOn(PlacementPreferredObject, PlacementPreferredPoint, PlacementPreferredNormal, ref PlacementErrorMessage)) {
														PlacementPermitted = true;
														PlacementResultsInDrop = false;
												}
										} else if (Player.Local.Tool.worlditem.CanBeDropped) {
												//if there's nothing in range, we just drop
												PlacementPermitted = true;
												PlacementResultsInDrop = true;
										} else {
												PlacementPermitted = false;
												PlacementResultsInDrop = false;
										}
								}
						}
						//check to see if the thing we're placing will actually drop
						if (PlacementPermitted && !PlacementResultsInDrop) {	//first check the normal - the placement point may be on a wall or something
								if (Vector3.Dot(Vector3.up, PlacementPreferredNormal) < 0f) {	//turn off this collider before doing the raycast
										PlacementDopplegangerContainer.enabled = false;
										Vector3 raycastDown = PlacementPreferredPoint;
										raycastDown.y -= PlacementDopplegangerContainer.bounds.extents.y;
										if (!Physics.Raycast(raycastDown, Vector3.down, out mDropHit, 0.05f, Globals.LayersActive)) {
												PlacementResultsInDrop = true;
										}
										PlacementDopplegangerContainer.enabled = true;
								}
						}
						#endregion
				}

				protected IEnumerator PlaceOrDropCarriedItemOverTime ( ) {

						if (CarryObject.IsQuestItem) {
								GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIYesNoCancelDialog"));
								GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
								DiscardQuestItemDialogResult.Result = DialogResult.None;
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(childEditorGameObject, DiscardQuestItemDialogResult);
								while (!childEditor.IsFinished) {
										//wait to see if we really want to toss it
										yield return null;
								}
								if (DiscardQuestItemDialogResult.Result != DialogResult.Yes) {
										//don't throw it
										mPlacingOrDroppingCarriedItem = false;
										yield break;
								}
						}

						if (CarryObject == null) {
								//whoops, something happened while the dialog was up
								mPlacingOrDroppingCarriedItem = false;
								yield break;
						}

						if (PlacementResultsInDrop) {
								//if placement results in drop we just drop it even if it's placeable
								//we've already checked if we're allowed to drop it
								LastItemDropped = CarryObject.gameObject;
								DropCoolDown = WorldClock.RealTime + CooldownInterval;
								player.Grabber.Joint.connectedBody = null;
								CarryObject.LastActiveDistanceToPlayer = 0f;
								CarryObject.SetMode(WIMode.World);
								CarryObject.tr.position = PlacementPreferredPointSmooth;
								if (PlacementDoppleganger != null) {
										CarryObject.tr.rotation = PlacementDoppleganger.transform.rotation;
								}
								GUIManager.PostInfo("Dropping " + CarryObject.DisplayName);
								CarryObject.OnPlayerDrop.SafeInvoke();
								//CarryObject.SendMessage ("OnDroppedByPlayer", SendMessageOptions.DontRequireReceiver);

								CarryObject = null;
						} else if (PlacementInReceptaclePossible) {
								//this is kind of risky but I think it works without a check
								CarryObject.SetMode(WIMode.Frozen);
								CarryObject.tr.position = PlacementPreferredPointSmooth;
								if (PlacementDoppleganger != null) {
										CarryObject.tr.rotation = PlacementDoppleganger.transform.rotation;
								} else {
										CarryObject.tr.Rotate(PlacementPreferredNormal);
										CarryObject.tr.Rotate(ItemToPlace.Props.Global.BaseRotation);
								}
								CarryObject.Props.Local.FreezeOnStartup	= true;
								CarryObject.LastActiveDistanceToPlayer = 0f;
								CarryObject = null;
						} else {
								LastItemPlaced = CarryObject.gameObject;
								PlaceCoolDown = WorldClock.RealTime + CooldownInterval;
								player.Grabber.Joint.connectedBody = null;
								CarryObject.SetMode(WIMode.Frozen);
								CarryObject.transform.position = PlacementPreferredPointSmooth;
								CarryObject.transform.rotation = Quaternion.identity;
								if (PlacementDoppleganger != null) {
										CarryObject.transform.rotation = PlacementDoppleganger.transform.rotation;
								} else {
										CarryObject.transform.Rotate(PlacementPreferredNormal);
										CarryObject.transform.Rotate(CarryObject.Props.Global.BaseRotation);
								}
								CarryObject.LastActiveDistanceToPlayer = 0f;
								CarryObject.OnPlayerPlace.SafeInvoke();
								GUIManager.PostSuccess("Placed " + CarryObject.DisplayName);
								CarryObject = null;
						}

						mPlacingOrDroppingCarriedItem = false;
						yield break;
				}

				protected IEnumerator ThrowCarriedItemOverTime () {

						if (CarryObject.IsQuestItem) {
								GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIYesNoCancelDialog"));
								GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
								DiscardQuestItemDialogResult.Result = DialogResult.None;
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(childEditorGameObject, DiscardQuestItemDialogResult);
								while (!childEditor.IsFinished) {
										//wait to see if we really want to toss it
										yield return null;
								}
								if (DiscardQuestItemDialogResult.Result != DialogResult.Yes) {
										//don't throw it
										mThrowingCarriedItem = false;
										yield break;
								}
						}

						if (CarryObject.CanBeDropped) {
								//if placement results in drop we just drop it even if it's placeable
								//we've already checked if we're allowed to drop it
								LastItemDropped = CarryObject.gameObject;
								DropCoolDown = WorldClock.RealTime + CooldownInterval;
								player.Grabber.Joint.connectedBody = null;
								CarryObject.LastActiveDistanceToPlayer = 0f;
								CarryObject.SetMode(WIMode.World);
								CarryObject.tr.position = PlacementPreferredPointSmooth;
								if (PlacementDoppleganger != null) {
										CarryObject.tr.rotation = PlacementDoppleganger.transform.rotation;
								}
								GUIManager.PostInfo("Dropping " + CarryObject.DisplayName);
								CarryObject.OnPlayerDrop.SafeInvoke();
								CarryObject.ApplyForce(Player.Local.FocusVector * ThrowForce, CarryObject.tr.position);
								CarryObject = null;
						} else {
								GUIManager.PostWarning("Can't throw " + CarryObject.DisplayName);
						}
						mThrowingCarriedItem = false;
						yield break;
				}

				protected IEnumerator ThrowEquippedItemOverTime () {

						if (Player.Local.Tool.IsEquipped) {
								if (Player.Local.Tool.worlditem.IsQuestItem) {
										GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIYesNoCancelDialog"));
										GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
										DiscardQuestItemDialogResult.Result = DialogResult.None;
										GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(childEditorGameObject, DiscardQuestItemDialogResult);
										while (!childEditor.IsFinished) {
												//wait to see if we really want to toss it
												yield return null;
										}
										if (DiscardQuestItemDialogResult.Result != DialogResult.Yes) {
												//don't throw it
												mThrowingEquippedItem = false;
												yield break;
										}
								}
						}

						//start again from scratch, things may have changed
						if (Player.Local.Tool.IsEquipped) {
								WIGroup locationGroup = WIGroups.GetCurrent();
								/*GameWorld.Get.PrimaryChunk.AboveGroundGroup;
								if (Player.Local.Surroundings.IsVisitingLocation) {
										locationGroup = Player.Local.Surroundings.CurrentLocation.LocationGroup;
								}*/
								//put it into the world (TODO make sure this works)
								WorldItem aqi = null;
								WIStackError error = WIStackError.None;
								Vector3 toolPosition = Player.Local.Tool.ItemPosition;
								Quaternion toolRotation = Player.Local.Tool.ItemRotation;
								if (Player.Local.Inventory.PopAQIIntoWorld(out aqi, locationGroup, ref error)) {
										aqi.ActiveState = WIActiveState.Active;
										aqi.Props.Local.FreezeOnStartup = false;
										aqi.SetMode(WIMode.World);
										aqi.tr.position = toolPosition;
										aqi.tr.rotation = toolRotation;
										aqi.LastActiveDistanceToPlayer = 0f;

										aqi.ApplyForce(Player.Local.FocusVector * ThrowForce, aqi.tr.position);

										ItemToPlace = null;
								}
						}

						mThrowingEquippedItem = false;
				}

				protected IEnumerator DropSelectedItemssOverTime()
				{
						WIGroup group = WIGroups.Get.World;
						if (player.Surroundings.IsVisitingLocation) {
								group = player.Surroundings.CurrentLocation.LocationGroup;
						}

						if (Stacks.Contains.QuestItem(player.Inventory.SelectedStack)) {
								GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIYesNoCancelDialog"));
								GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
								DiscardQuestItemDialogResult.Result = DialogResult.None;
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(childEditorGameObject, DiscardQuestItemDialogResult);
								while (!childEditor.IsFinished) {
										//wait to see if we really want to toss it
										yield return null;
								}
								if (DiscardQuestItemDialogResult.Result != DialogResult.Yes) {
										//don't throw it
										mDroppingSelectedItems = false;
										yield break;
								}
						}

						while (!Player.Local.Inventory.SelectedStack.IsEmpty) {
								Stacks.Pop.ContentsIntoWorld(Player.Local.Inventory.SelectedStack, 1, Player.Local.Grabber.Position, group);
								yield return WorldClock.WaitForRTSeconds(0.1f);
						}
						mDroppingSelectedItems = false;
						yield break;
				}

				protected IEnumerator PlaceOrDropEquippedItemOverTime () {
						//get this all out of the way BEFORE we try to place
						Vector3 placePosition = PlacementPreferredPointSmooth;
						Quaternion placeRotation = Quaternion.identity;
						if (PlacementResultsInDrop) {
								placePosition = player.GrabberTargetObject.position;
						} else if (PlacementDoppleganger != null) {
								placeRotation = PlacementDoppleganger.transform.rotation;
						} else {
								placeRotation = Quaternion.Euler(PlacementPreferredNormal);
						}

						if (ItemToPlace.Is<QuestItem>()) {
								GameObject childEditorGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIYesNoCancelDialog"));
								GUIYesNoCancelDialog childEditor = childEditorGameObject.GetComponent <GUIYesNoCancelDialog>();
								DiscardQuestItemDialogResult.Result = DialogResult.None;
								GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(childEditorGameObject, DiscardQuestItemDialogResult);
								while (!childEditor.IsFinished) {
										//wait to see if we really want to toss it
										yield return null;
								}
								if (DiscardQuestItemDialogResult.Result != DialogResult.Yes) {
										//don't throw it
										mPlacingOrDroppingEquippedItem = false;
										yield break;;
								}
						}

						if (Player.Local.Tool.IsEquipped) {
								WIGroup locationGroup = WIGroups.GetCurrent();
								WorldItem aqi = null;
								WIStackError error = WIStackError.None;
								if (Player.Local.Inventory.PopAQIIntoWorld(out aqi, locationGroup, ref error)) {
										if (PlacementInReceptaclePossible) {
												PlacementPreferredReceptacle.AddToReceptacle(aqi);
										} else {
												aqi.ActiveState = WIActiveState.Active;
												if (PlacementResultsInDrop) {
														aqi.Props.Local.FreezeOnStartup = false;
														aqi.SetMode(WIMode.World);
												} else {
														aqi.Props.Local.FreezeOnStartup = true;
														aqi.SetMode(WIMode.Frozen);
												}
												aqi.tr.position = placePosition;
												aqi.tr.rotation = placeRotation;
												aqi.LastActiveDistanceToPlayer = 0f;
												aqi.OnPlayerPlace.SafeInvoke();
										}
										ItemToPlace = null;
								}
						}

						mPlacingOrDroppingEquippedItem = false;
						yield break;
				}

				protected RaycastHit mDropHit;

				public SurfaceOrientation GetSurfaceOrientationFromNormal(Vector3 normal)
				{
						return SurfaceOrientation.None;
				}

				protected bool DoingSomethingOverTime {
						get {
								return mThrowingCarriedItem || mDroppingSelectedItems || mPlacingOrDroppingEquippedItem || mThrowingEquippedItem || mPlacingOrDroppingCarriedItem;
						}
				}

				protected bool mThrowingCarriedItem = false;
				protected bool mDroppingSelectedItems = false;
				protected bool mPlacingOrDroppingEquippedItem = false;
				protected bool mThrowingEquippedItem = false;
				protected bool mPlacingOrDroppingCarriedItem = false;
		}
}
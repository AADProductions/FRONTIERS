using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class Receptacle : WIScript, IGUIParentEditor <YesNoCancelDialogResult>
		{
				//recepticles are a nightmare out of hell
				//seriously 2/3 of the biggest bugs i've dealt with come from this class
				//but whatever, they're necessary so i put up with them
				//recepticles work closely with the PlayerItemPlacement script
				public ReceptacleState State = new ReceptacleState();
				public List <ReceptaclePivot> Pivots = new List <ReceptaclePivot>();
				public Action OnItemPlacedInReceptacle;
				public Action OnItemRemovedFromReceptacle;
				public GameObject FocusDoppleganger = null;
				public WorldItem ItemToPlace = null;
				public ReceptaclePivot PivotInFocus = null;
				public Container container;
				public bool FocusPlacementPermitted = false;
				public ReceptacleVisualStyle VisualStyle = ReceptacleVisualStyle.Projector;

				public ReceptaclePivot AddPivot(Transform pivotTransform)
				{
						Transform newPivotTransform = pivotTransform;
						ReceptaclePivot newPivot = newPivotTransform.gameObject.AddComponent <ReceptaclePivot>();
						newPivot.ParentReceptacle = this;
						newPivot.State = new ReceptaclePivotState();
						newPivot.State.InheritParentSettings = true;
						newPivot.State.Index = State.PivotStates.Count;
						Pivots.Add(newPivot);
						State.PivotStates.Add(newPivot.State);
						newPivot.OnInitialized();
						worlditem.StackContainer.SetNumStacks(Pivots.Count);
						return newPivot;
				}

				public override bool CanEnterInventory {
						get {
								if (mInitialized) {
										for (int i = 0; i < Pivots.Count; i++) {
												if (Pivots[i].IsOccupied) {
														return false;
												}
										}
								}
								return true;
						}
				}

				public override bool CanBeCarried {
						get {
								if (mInitialized) {
										for (int i = 0; i < Pivots.Count; i++) {
												if (Pivots[i].IsOccupied) {
														return false;
												}
										}
								}
								return true;
						}
				}

				public override void OnInitialized()
				{
						worlditem.OnGainPlayerFocus += OnGainPlayerFocus;
						worlditem.OnPlayerUse += OnPlayerUse;
						worlditem.OnVisible += OnVisible;
						worlditem.OnActive += OnActive;
						//if we're a container, make sure the container uses the right settings
						Container container = worlditem.GetOrAdd <Container>();
						worlditem.StackContainer.Mode = WIStackMode.Receptacle;
						container.CanUseToOpen = false;
						container.CanOpen = false;

						if (State.PivotStates.Count > 0) {
								//load the pivots from the states
								for (int i = 0; i < State.PivotStates.Count; i++) {
										ReceptaclePivot pivot = Pivots[i];
										pivot.ParentReceptacle = this;
										pivot.State = State.PivotStates[i];
										pivot.State.Index = i;
										pivot.OnInitialized();
								}
						} else {
								//this is our first time loading, load the pivot states
								//from the actual pivot objects
								int pivotIndex = 0;
								for (int i = 0; i < Pivots.Count; i++) {
										ReceptaclePivot pivot = Pivots[i];// child.GetComponent <ReceptaclePivot> ();
										pivot.ParentReceptacle = this;
										pivot.State.Offset = new STransform(pivot.transform, true);
										pivot.State.Index = pivotIndex;
										pivot.OnInitialized();
										State.PivotStates.Add(pivot.State);
										pivotIndex++;
								}
						}
						worlditem.StackContainer.SetNumStacks(Pivots.Count);
				}

				public void OnVisible()
				{
						Refresh();
				}

				public void OnActive()
				{ 
						Refresh();
				}

				public void OnGainPlayerFocus()
				{
						if (State.Locked) {
								return;
						}

						enabled = true;
				}

				protected bool mShowingHUD = false;

				public void FixedUpdate()
				{
						if (State.Locked) {
								return;
						}

						if (!worlditem.HasPlayerFocus) {
								mShowingHUD = false;
								HidePivotVisualizers();
								WorldItems.ReturnDoppleganger(FocusDoppleganger);
								if (PivotInFocus != null) {
										PivotInFocus.FocusOnOccupant(false);
								}
								ItemToPlace = null;
								PivotInFocus = null;
								enabled = false;
								return;
						} else if (!Player.Local.ItemPlacement.PlacementModeEnabled) {
								//don't let item placement force us to disable
								mShowingHUD = false;
								HidePivotVisualizers();
								WorldItems.ReturnDoppleganger(FocusDoppleganger);
								if (PivotInFocus != null) {
										PivotInFocus.FocusOnOccupant(false);
								}
								ItemToPlace = null;
								PivotInFocus = null;
								return;
						}

						bool useDoppleganger = false;
						ItemToPlace = null;
						ReceptaclePivot newPivotInFocus = GetClosestPivot(Player.Local.Surroundings.WorldItemFocusHitInfo.point);

						if (PivotInFocus != null && PivotInFocus != newPivotInFocus) {
								PivotInFocus.FocusOnOccupant(false);
						}
						PivotInFocus = newPivotInFocus;
						FocusPlacementPermitted = false;

						if (PivotInFocus.IsOccupied) {
								//we want to highlight the item so we can pick it up
								PivotInFocus.FocusOnOccupant(true);
						} else {
								//if the player is holding something, highlight it for placement
								if (Player.Local.Tool.IsEquipped) {
										ItemToPlace = Player.Local.Tool.worlditem;
								} else if (Player.Local.ItemPlacement.IsCarryingSomething) {
										ItemToPlace = Player.Local.ItemPlacement.CarryObject;
								}
								if (ItemToPlace != null) {
										//get where our player is focusing
										string errorMessage = string.Empty;
										if (IsObjectPermitted(ItemToPlace, PivotInFocus.Settings) && ItemToPlace.CanBePlacedOn(this.worlditem, PivotInFocus.tr.position, PivotInFocus.tr.up, ref errorMessage)) {
												//create a doppleganger showing what the player could place here if he wanted
												FocusDoppleganger = WorldItems.GetDoppleganger(ItemToPlace, PivotInFocus.tr, FocusDoppleganger, WIMode.Placing, 1f / PivotInFocus.tr.lossyScale.x);
												//check to see if the potential occupant will actually fix
												FocusDoppleganger.transform.parent = PivotInFocus.tr;
												FocusDoppleganger.transform.localPosition = Vector3.zero + ItemToPlace.BasePivotOffset;
												FocusDoppleganger.transform.localRotation = Quaternion.identity;

												Mats.Get.ItemPlacementOutlineMaterial.SetColor("_OutlineColor", Colors.Get.MessageSuccessColor);
												Mats.Get.ItemPlacementMaterial.SetColor("_TintColor", Colors.Get.MessageSuccessColor);

												if (CanOccupantFit(ItemToPlace, PivotInFocus, Pivots)) {
														if (!mShowingHUD) {
																mShowingHUD = true;
																GUIHud.Get.ShowControls(KeyCode.E, "Place", FocusDoppleganger.transform, GameManager.Get.GameCamera);
														}
														FocusPlacementPermitted = true;
												} else {
														if (!mShowingHUD) {
																mShowingHUD = true;
																GUIHud.Get.ShowControls(KeyCode.E, "Place", FocusDoppleganger.transform, GameManager.Get.GameCamera);
														}
														Mats.Get.ItemPlacementOutlineMaterial.SetColor("_OutlineColor", Colors.Get.MessageDangerColor);
														Mats.Get.ItemPlacementMaterial.SetColor("_TintColor", Colors.Get.MessageDangerColor);
												}
												useDoppleganger = true;
										} else {
												FocusPlacementPermitted = false;
										}
								}
						}

						if (ItemToPlace != null) {
								ShowPivotVisualizers(PivotInFocus, Pivots, VisualStyle);
						}

						if (!useDoppleganger && FocusDoppleganger != null) {
								GameObject.Destroy(FocusDoppleganger);
						}
				}

				public void OnPlayerUse()
				{
						if (State.Locked) {
								return;
						}

						if (WorldClock.RealTime < mCooldownStartTime + mCooldownInterval) {
								return;
						}

						if (PivotInFocus != null) {
								if (!PivotInFocus.IsOccupied && ItemToPlace != null && FocusPlacementPermitted) {
										AddToReceptaclePivot(ItemToPlace, PivotInFocus);
										mCooldownStartTime = WorldClock.RealTime;
								}
						}
				}

				public void SetLocked(bool locked)
				{ 
						State.Locked = locked;
				}

				protected double mCooldownStartTime = 0f;
				protected double mCooldownInterval = 0.25f;

				public bool ContainsQuestItem(string questItemName)
				{
						for (int i = 0; i < Pivots.Count; i++) {
								if (Pivots[i].IsOccupied
								    && Pivots[i].Occupant.Is <QuestItem>()
								    && Pivots[i].Occupant.QuestName == questItemName) {
										return true;
								}
						}
						return false;
				}

				public bool HasRoom(WorldItem potentialOccupant, out ReceptaclePivot emptyPivot)
				{
						emptyPivot = null;

						if (State.Locked) {
								return false;
						}

						for (int i = 0; i < Pivots.Count; i++) {
								ReceptaclePivot pivot = Pivots[i];
								if (!pivot.IsOccupied) {
										emptyPivot = pivot;
										break;
								}
						}
						return emptyPivot != null;
				}

				public bool HasRoom(WorldItem potentialOccupant, Vector3 point, out ReceptaclePivot emptyPivot)
				{
						emptyPivot = null;

						if (State.Locked) {
								return false;
						}

						float closestSoFar = Mathf.Infinity;
						for (int i = 0; i < Pivots.Count; i++) {
								ReceptaclePivot pivot = Pivots[i];
								if (!pivot.IsOccupied) {
										float distance = Vector3.Distance(point, pivot.PivotBounds.center);
										if (distance < closestSoFar) {
												emptyPivot = pivot;
												closestSoFar = distance;
										}
								}
						}
						return emptyPivot != null;
				}

				public ReceptaclePivot GetClosestPivot(Vector3 point)
				{
						ReceptaclePivot closestPivot = null;
						float closestSoFar = Mathf.Infinity;
						for (int i = 0; i < Pivots.Count; i++) {
								ReceptaclePivot pivot = Pivots[i];
								float distance = Vector3.Distance(point, pivot.tr.position);
								if (distance < closestSoFar) {
										closestPivot = pivot;
										closestSoFar = distance;
								}
						}
						return closestPivot;
				}

				public bool HasRoom()
				{
						if (State.Locked) {
								return false;
						}

						foreach (ReceptaclePivot pivot in Pivots) {
								if (!pivot.IsOccupied) {
										return true;
								}
						}
						return false;
				}

				public void Refresh()
				{
						if (!mInitialized)
								return;

						if (mRefreshingOverTime)
								return;

						if (!worlditem.Is(WIActiveState.Visible | WIActiveState.Active))
								return;

						mRefreshingOverTime = true;
						Stacks.Clear.DestroyedOrMovedItems(worlditem.StackContainer);
						StartCoroutine(RefreshingOverTime());
				}

				protected virtual bool AddToReceptaclePivot(WorldItem potentialOccupant, ReceptaclePivot pivotInFocus)
				{

						if (State.Locked || mAddingToReceptaclePivot) {
								return false;
						}

						if (pivotInFocus.IsOccupied || !IsObjectPermitted(potentialOccupant, pivotInFocus.Settings)) {
								return false;
						}

						mAddingToReceptaclePivot = true;
						StartCoroutine(AddToReceptaclePivotOverTime(potentialOccupant, pivotInFocus));
						return true;
				}

				protected IEnumerator RefreshingOverTime()
				{
						yield return null;
						for (int i = 0; i < Pivots.Count; i++) {
								Pivots[i].Refresh();
						}
						yield return null;
						mRefreshingOverTime = false;
						yield break;
				}

				protected IEnumerator AddToReceptaclePivotOverTime(WorldItem potentialOccupant, ReceptaclePivot pivotInFocus)
				{
						//remove it from any situation that could cause a cock-up
						worlditem.Group.AddChildItem(potentialOccupant);
						potentialOccupant.SetMode(WIMode.World);
						//this will put it into the world and un-equip it etc
						//wait for that to happen
						yield return null;
						WIStack pivotStack = worlditem.StackContainer.StackList[pivotInFocus.State.Index];
						WIStackError error = WIStackError.None;
						//do not auto convert to stack item
						if (!Stacks.Push.Item(pivotStack, potentialOccupant, StackPushMode.Manual, ref error)) {
								Debug.Log("Couldn't push item into group because " + error.ToString());
								yield break;
						}
						//wait again for the worlditem to get situated
						yield return null;
						potentialOccupant.OnRemoveFromStack += Refresh;
						potentialOccupant.OnModeChange += Refresh;
						potentialOccupant.tr.parent = pivotInFocus.tr;
						//this will move it into the recepticle position
						pivotInFocus.Refresh();
						yield return null;
						OnItemPlacedInReceptacle.SafeInvoke();
						mAddingToReceptaclePivot = false;
				}

				protected bool mRefreshingOverTime = false;
				protected bool mAddingToReceptaclePivot = false;

				public virtual bool AddToReceptacle(WorldItem potentialOccupant)
				{
						if (State.Locked) {
								return false;
						}

						if (mCooldownStartTime + mCooldownInterval > WorldClock.RealTime) {
								return false;
						}

						bool result = false;
						ReceptaclePivot emptyPivot = null;
						if (HasRoom(potentialOccupant, out emptyPivot) && IsObjectPermitted(potentialOccupant, emptyPivot.Settings)) {
								//first we have to move the potential occupant into our group
								WIStack pivotStack = worlditem.StackContainer.StackList[emptyPivot.State.Index];
								WIStackError error = WIStackError.None;
								result = Stacks.Push.Item(pivotStack, potentialOccupant, ref error);
						}

						if (result) {
								potentialOccupant.OnRemoveFromStack += Refresh;
								potentialOccupant.OnUnloaded += Refresh;
								potentialOccupant.OnAddedToPlayerInventory += Refresh;
								OnItemPlacedInReceptacle.SafeInvoke();
								emptyPivot.Refresh();
						}

						return result;
				}

				public virtual bool IsObjectPermitted(WorldItem potentialOccupant, ReceptacleSettings settings)
				{
						if (State.Locked) {
								return false;
						}

						if (potentialOccupant == null) {
								return false;
						}

						if (((int)potentialOccupant.Size) > ((int)settings.MaxSize)) {
								//TODO use Stacks size comparison, this is error prone
								Debug.Log("Too big");
								return false;
						}

						if (settings.PermittedScripts.Count > 0) {
								if (!potentialOccupant.HasAtLeastOne(settings.PermittedScripts)) {
										return false;
								}
						}
						if (settings.PermittedSubcats.Count > 0) {
								if (!settings.PermittedSubcats.Contains(potentialOccupant.Subcategory)) {
										return false;
								}
						}
						if (settings.PermittedMaterials != WIMaterialType.None) {
								if (!Flags.Check((uint)settings.PermittedMaterials, (uint)potentialOccupant.Props.Global.MaterialType, Flags.CheckType.MatchAny)) {
										return false;
								}
						}
						if (settings.PermittedItems.Count > 0) {
								foreach (string permittedItemName in settings.PermittedItems) {
										if (!Stacks.Can.Stack(permittedItemName, potentialOccupant.StackName)) {
												return false;
										}
								}
						}
						return true;
				}

				public void OnChildWorldItemRemoved()
				{
						//PlacedItem = null;
				}

				protected static Bounds gReceptacleBounds = new Bounds();
				protected static Bounds gColliderBounds = new Bounds();
				#if UNITY_EDITOR
				protected static GameObject gVisualizationItem = null;
				protected static GameObject gVisualizationIntersect = null;
				#endif
				public static bool CanOccupantFit(WorldItem potentialOccupant, ReceptaclePivot pivotInFocus, List <ReceptaclePivot> pivots)
				{
						#if UNITY_EDITOR
						/*
						if (gVisualizationIntersect == null) {
							gVisualizationIntersect = GameObject.CreatePrimitive (PrimitiveType.Cube);
							gVisualizationIntersect.renderer.material = Mats.Get.ItemPlacementMaterial;
							gVisualizationIntersect.renderer.material.color = Color.red;
							gVisualizationIntersect.collider.enabled = false;
							gVisualizationIntersect.layer = Globals.LayerNumScenery;

							gVisualizationItem = GameObject.CreatePrimitive (PrimitiveType.Cube);
							gVisualizationItem.renderer.material = Mats.Get.ItemPlacementMaterial;
							gVisualizationItem.renderer.material.color = Color.yellow;
							gVisualizationItem.collider.enabled = false;
							gVisualizationItem.layer = Globals.LayerNumScenery;
						}
						*/
						#endif
						//check the size collider against all the other colliders in the recepticle
						//start by figuring out how big the occupant is
						gReceptacleBounds.center = pivotInFocus.tr.position;
						gReceptacleBounds.size = potentialOccupant.BaseObjectBounds.size;

						#if UNITY_EDITOR
						/*
						gVisualizationItem.transform.localScale = gReceptacleBounds.size;
						gVisualizationItem.transform.position = gReceptacleBounds.center + potentialOccupant.BasePivotOffset;
						gVisualizationItem.renderer.enabled = true;
						gVisualizationIntersect.renderer.enabled = false;
						*/
						#endif

						for (int i = 0; i < pivots.Count; i++) {
								if (pivots[i] == pivotInFocus) {
										continue;
								}

								if (pivots[i].IsOccupied) {
										gColliderBounds = pivots[i].Occupant.BaseObjectBounds;
										gColliderBounds.center = pivots[i].tr.position + pivots[i].Occupant.BasePivotOffset;

										if (gReceptacleBounds.Intersects(gColliderBounds)) {
												#if UNITY_EDITOR
												/*
												gVisualizationIntersect.transform.localScale = gColliderBounds.size;
												gVisualizationIntersect.transform.position = gColliderBounds.center + pivots [i].Occupant.BasePivotOffset;
												gVisualizationIntersect.renderer.enabled = true;
												*/
												#endif
												return false;
										}
								}
						}

						return true;
				}

				public static void HidePivotVisualizers()
				{
						for (int i = 0; i < gReceptacleProjectors.Count; i++) {
								gReceptacleProjectors[i].enabled = false;
						}
				}

				public static void ShowPivotVisualizers(ReceptaclePivot pivotInFocus, List <ReceptaclePivot> pivots, ReceptacleVisualStyle visualStyle)
				{
						switch (visualStyle) {
								case ReceptacleVisualStyle.Projector:
								default:
										while (pivots.Count > gReceptacleProjectors.Count) {
												GameObject newProjectorGameObject = new GameObject("ReceptacleProjector");
												Projector newProjector = newProjectorGameObject.AddComponent <Projector>();
												newProjectorGameObject.layer = Globals.LayerNumScenery;
												newProjector.enabled = false;
												newProjector.isOrthoGraphic = true;
												newProjector.orthoGraphicSize = 0.15f;
												newProjector.nearClipPlane = 0.1f;
												newProjector.farClipPlane = 0.6f;
												newProjector.material = Mats.Get.ReceptacleProjectorMaterial;
												newProjector.ignoreLayers = ~Globals.LayerWorldItemActive;
												gReceptacleProjectors.Add(newProjector);
										}

										for (int i = 0; i < pivots.Count; i++) {
												if (pivots[i].IsOccupied) {
														gReceptacleProjectors[i].enabled = false;
												} else {
														gReceptacleProjectors[i].transform.position = pivots[i].tr.position + pivots[i].tr.up * 0.5f;
														gReceptacleProjectors[i].transform.LookAt(pivots[i].tr.position);
														gReceptacleProjectors[i].enabled = true;
												}
										}
										break;

								case ReceptacleVisualStyle.GeneralDoppleganger:
										break;

								case ReceptacleVisualStyle.SpecificDoppleganger:
										break;
						}
				}

				public void OnChildWorldItemDestroyed()
				{
//			PlacedItem = null;
				}

				protected ReceptaclePivot mLastOptionPivot = null;

				public GameObject NGUIObject { get { return gameObject; } set { return; } }

				public void ReceiveFromChildEditor(YesNoCancelDialogResult editObject, IGUIChildEditor<YesNoCancelDialogResult> childEditor)
				{
						mResult = editObject;
						GUIManager.ScaleDownEditor(childEditor.gameObject).Proceed(true);
				}

				protected IEnumerator PlaceItemOverTime(WorldItem itemToPlace)
				{
						bool addToReceptacle = true;
						//check and see if this will result in a change of ownership
						//if it will, confirm before placing
						IStackOwner owner = null;
						if (worlditem.StackContainer.HasOwner(out owner) && owner != Player.Local) {
								//send a dialog to confirm the player wants to do this
								mResult = null;
								if (!Profile.Get.CurrentPreferences.HideDialogs.Contains("TransferOwnership")) {
										//show the dialog explaining what happens
										YesNoCancelDialogResult editObject = new YesNoCancelDialogResult();
										editObject.CancelButton = false;
										editObject.DontShowInFutureCheckbox = true;
										editObject.DialogName = "TransferOwnership";
										editObject.MessageType = "Transfer Ownership";
										editObject.Message = "If you do this, the item will be owned by someone else.\nAdd item to container?";

										GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIYesNoCancelDialog, false);
										GUIManager.SendEditObjectToChildEditor <YesNoCancelDialogResult>(new ChildEditorCallback <YesNoCancelDialogResult>(ReceiveFromChildEditor), childEditor, editObject);

										while (mResult == null) {
												yield return null;
										}

										if (mResult.Result != DialogResult.Yes) {
												addToReceptacle = false;
										}

								}
						}

						if (addToReceptacle) {
								AddToReceptacle(itemToPlace);
						}
						yield break;
				}

				protected YesNoCancelDialogResult mResult;
				public static List<Projector> gReceptacleProjectors = new List<Projector>();
		}

		public enum ReceptacleVisualStyle
		{
				Projector,
				GeneralDoppleganger,
				SpecificDoppleganger,
		}

		[Serializable]
		public class ReceptacleState
		{
				public bool Locked = false;
				public ReceptacleSettings Settings = new ReceptacleSettings();
				public List <ReceptaclePivotState> PivotStates = new List <ReceptaclePivotState>();
		}
}
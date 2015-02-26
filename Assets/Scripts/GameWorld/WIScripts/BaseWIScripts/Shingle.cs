using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using System;

namespace Frontiers.World.BaseWIScripts
{
		public class Shingle : WIScript
		{
				//helps with buying / selling structures
				//shows when structures are available to buy
				public GameObject ForSaleSignPrefab;
				public Structure ParentStructure;
				public GameObject ForSaleSign;
				public TextMesh ShingleText;

				public bool PropertyIsDestroyed {
						get {
								return State.PropertyStatus == PropertyStatusType.Destroyed || State.PropertyStatus == PropertyStatusType.DestroyedForever;
						}
				}

				public ShingleState State = new ShingleState();

				public override void OnStartup()
				{
						if (mForSaleOption == null) {
								mClaimOption = new WIListOption("Claim this property", "Claim");
								mRenameOption = new WIListOption("Rename this property", "Rename");
								mForSaleOption = new WIListOption("Buy this property", "Buy");
								mRestoreOption = new WIListOption("Restore this property", "Restore");
						}
				}

				public override void OnInitialized()
				{
						if (mAbandonedExamineInfo == null) {
								mAbandonedExamineInfo = new WIExamineInfo("This property is abandoned");
								mPlayerOwnExamineInfo = new WIExamineInfo("You own this property");
								mDestroyedExamineInfo = new WIExamineInfo();
								mCharacterOwnExamineInfo = new WIExamineInfo();
								mBuyExamineInfo = new WIExamineInfo();
						}

						worlditem.OnInvisible += OnInvisible;
						worlditem.OnVisible += OnVisible;
						Player.Get.AvatarActions.Subscribe(AvatarAction.LocationAquire, new ActionListener(LocationAquire));
				}

				public bool LocationAquire(double timeStamp)
				{
						if (Player.Local.Inventory.State.OwnedStructures.Contains(worlditem.StaticReference)) {
								State.PropertyStatus = PropertyStatusType.OwnedByPlayer;
						}
						return true;
				}

				public void SetParentStructure(Structure parentStructure)
				{

						ParentStructure = parentStructure;
						State.ParentStructure = ParentStructure.worlditem.StaticReference;
						ParentStructure.OnStructureDestroyed += OnStructureDestroyed;
						ParentStructure.OnStructureRestored += OnStructureRestored;
						ParentStructure.OnPreparingToBuild += OnPreparingToBuild;
						//make sure parent structure knows if we're owned by player etc
						//this kind of data redundancy sucks but it's necessary here
						switch (State.PropertyStatus) {
								case PropertyStatusType.OwnedByPlayer:
										ParentStructure.State.IsOwnedByPlayer = true;
										ParentStructure.State.IsSafeLocation = true;
										//if we're owned by the player
										//let the player know
										//if they already know this does nothing
										Player.Local.Inventory.AcquireStructure(State.ParentStructure, State.AnnounceOwnership);
										break;

								default:
										ParentStructure.State.IsOwnedByPlayer = false;
										//let the structure handle its own safe location bit
										break;
						}

						if (State.PropertyStatus == PropertyStatusType.ForSale && ForSaleSign == null) {
								//can we do this some other way? another gameobject? i hate instantiating stuff like this
								ForSaleSign = GameObject.Instantiate(ForSaleSignPrefab) as GameObject;
								ForSaleSign.transform.parent = transform;
								State.ForSaleSignOffset.ApplyTo(ForSaleSign.transform);
						}

						if (ShingleText != null) {
								Location location = worlditem.Get<Location>();
								ShingleText.text = location.State.Name.CommonName;
						}
				}

				public void OnInvisible()
				{
						if (ForSaleSign != null) {
								ForSaleSign.SetActive(false);
						}
				}

				public void OnVisible()
				{
						if (ForSaleSign != null) {
								ForSaleSign.SetActive(true);
						}
				}

				public void OnStructureDestroyed()
				{
						State.PropertyStatus = PropertyStatusType.Destroyed;
						if (State.TimeStructureDestroyed <= 0f) {
								State.TimeStructureDestroyed = WorldClock.AdjustedRealTime;
						}
						if (ForSaleSign != null) {
								GameObject.Destroy(ForSaleSign);
						}
				}

				public void OnStructureRestored()
				{
						//...
				}

				public void OnPreparingToBuild()
				{
						//if the structure is abandoned
						//and we have abandoned interior layers
						//add those now
						//TODO implement this
				}

				public override void PopulateExamineList(List<WIExamineInfo> examine)
				{
						State.PriceInMarks = CalculatePriceInMarks(ParentStructure);

						switch (State.PropertyStatus) {
								case PropertyStatusType.Abandoned:
								default:
										examine.Add(mAbandonedExamineInfo);
										break;

								case PropertyStatusType.Destroyed:
										mDestroyedExamineInfo.StaticExamineMessage = 
						"This property has been destroyed and can be restored for "
										+ (State.PriceInMarks / 2).ToString()
												+ " Marks (" + Currency.ConvertToBaseCurrency(State.PriceInMarks / 2, WICurrencyType.D_Luminite) + " grains)";
										break;

								case PropertyStatusType.DestroyedForever:
										mDestroyedExamineInfo.StaticExamineMessage = "This property has been destroyed and can never be restored";
										break;

								case PropertyStatusType.ForSale:
										mBuyExamineInfo.StaticExamineMessage = 
						"This property can be purchased for "
										+ State.PriceInMarks.ToString()
												+ " marks (" + Currency.ConvertToBaseCurrency(State.PriceInMarks, WICurrencyType.D_Luminite) + " grains)";
										examine.Add(mBuyExamineInfo);
										break;

								case PropertyStatusType.OwnedByCharacter:
										mCharacterOwnExamineInfo.StaticExamineMessage = "Somebody lives here";
										//if we can get the owner character from the structure
										//we can be more specific about who lives here
										if (ParentStructure.OwnerCharacterSpawned) {
												mCharacterOwnExamineInfo.StaticExamineMessage = ParentStructure.StructureOwner.FullName + " lives here";
										}
										examine.Add(mCharacterOwnExamineInfo);
										break;

								case PropertyStatusType.OwnedByPlayer:
										examine.Add(mPlayerOwnExamineInfo);
										break;
						}
				}

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						State.PriceInMarks = CalculatePriceInMarks(ParentStructure);

						switch (State.PropertyStatus) {
								case PropertyStatusType.Abandoned:
										options.Add(mClaimOption);
										break;

								case PropertyStatusType.ForSale:
										mForSaleOption.OptionText = "Buy";
										mForSaleOption.CurrencyValue = State.PriceInMarks;
										mForSaleOption.RequiredCurrencyType = WICurrencyType.D_Luminite;
										if (Player.Local.Inventory.InventoryBank.CanAfford(Currency.ConvertToBaseCurrency(State.PriceInMarks, WICurrencyType.D_Luminite))) {
												mForSaleOption.Disabled = false;
										} else {
												mForSaleOption.Disabled = false;
										}
										options.Add(mForSaleOption);
										break;

								case PropertyStatusType.OwnedByCharacter:
								default:
										break;

								case PropertyStatusType.Destroyed:
										mRestoreOption.OptionText = "Restore";
										mForSaleOption.CurrencyValue = State.PriceInMarks / 2;
										mForSaleOption.RequiredCurrencyType = WICurrencyType.D_Luminite;
										if (!Player.Local.Inventory.InventoryBank.HasExactChange(State.PriceInMarks, WICurrencyType.D_Luminite)) {
												mForSaleOption.Disabled = true;
										} else {
												mForSaleOption.Disabled = false;
										}
										options.Add(mForSaleOption);
										break;

								case PropertyStatusType.OwnedByPlayer:
										options.Add(mRenameOption);
										break;
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;			
						switch (dialogResult.SecondaryResult) {
								case "Claim":
										PlayerClaimStructure(ParentStructure);
										break;

								case "Buy":
										if (PlayerBuyStructure(ParentStructure, Currency.ConvertToBaseCurrency(State.PriceInMarks, WICurrencyType.D_Luminite))) {
												if (ForSaleSign != null) {
														ForSaleSign.SetActive(false);
												}
										}
										break;

								case "Restore":
										PlayerRestoreStructure(ParentStructure);
										break;

								case "Rename":
										PlayerRenameStructure(ParentStructure);
										break;
				
								default:
										break;
						}
				}

				public static int CalculatePriceInMarks(Structure structure)
				{
						//figure out which region we're in
						Region region = GameWorld.Get.CurrentRegion;
						Debug.Log("Structure region is: " + region.Name);
						//use the economic value as a basline
						//the value is 1 2 4 8, so just multiply it for the value
						int templateSize = Mods.Get.ModSizeInBytes("Structure", structure.State.TemplateName);
						Debug.Log("Structure template size is: " + templateSize.ToString());
						int valueInMarks = Mathf.Max(1, Mathf.FloorToInt(Globals.StructureBaseValueInMarks * region.ResidentFlags.Wealth * (templateSize * Globals.StructureValueTemplateMultiplier)));
						//then use the size of the template file to very roughly determine how much stuff is in it
						Debug.Log("Value in marks: " + valueInMarks.ToString());
						return valueInMarks;
				}

				protected static void PlayerRenameStructure(Structure structure)
				{
						//set the location's display name to whatever the player wants
						//this doesn't affect the file name so it has no effect on gameplay
				}

				protected static void PlayerClaimStructure(Structure structure)
				{

				}

				protected static void PlayerRestoreStructure(Structure structure)
				{

				}

				protected static bool PlayerBuyStructure(Structure structure, int baseCurrencyValue)
				{
						if (Player.Local.Inventory.InventoryBank.TryToRemove(baseCurrencyValue)) {
								Player.Local.Inventory.AcquireStructure(structure.worlditem.StaticReference, true);
								return true;
						}
						return false;
				}

				protected static WIExamineInfo mAbandonedExamineInfo;
				protected static WIExamineInfo mPlayerOwnExamineInfo;
				protected static WIExamineInfo mDestroyedExamineInfo;
				protected static WIExamineInfo mCharacterOwnExamineInfo;
				protected static WIExamineInfo mBuyExamineInfo;
				protected static WIListOption mForSaleOption = null;
				protected static WIListOption mRenameOption = null;
				protected static WIListOption mRestoreOption = null;
				protected static WIListOption mClaimOption = null;
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						if (ParentStructure == null) {
								WorldItem parentStructureWorldItem = gameObject.GetComponent <WorldItem>();
								State.ParentStructure = parentStructureWorldItem.StaticReference;
								ParentStructure = gameObject.GetComponent <Structure>();
						}

						if (ParentStructure == null) {
								Debug.Log("PARENT STRUCTURE WAS NULL IN " + name);
						} else {
								if (!PropertyIsDestroyed) {
										if (ParentStructure.State.OwnerSpawn.IsEmpty) {
												Shop shop = gameObject.GetComponent <Shop>();
												if (shop == null) {
														if (State.PropertyStatus != PropertyStatusType.OwnedByPlayer) {
																Residence residence = gameObject.GetComponent <Residence>();
																if (residence == null || string.IsNullOrEmpty(residence.State.OwnerCharacterName)) {
																		//disabling this for now, structures are now for sale randomly
																		//State.PropertyStatus = PropertyStatusType.ForSale;
																}
														}
												}
										} else {
												State.PropertyStatus = PropertyStatusType.OwnedByCharacter;
										}
								}
						}

						Transform forSaleOffset = transform.FindChild("ForSaleSign");
						if (forSaleOffset != null) {
								State.ForSaleSignOffset.CopyFrom(forSaleOffset);
						}

						if (State.PropertyStatus == PropertyStatusType.ForSale && State.PriceInMarks < 0) {
								State.PriceInMarks = AutoCalculateStructureValue(ParentStructure);
						}

						UnityEditor.EditorUtility.SetDirty(this);
				}

				protected static int AutoCalculateStructureValue(Structure structure)
				{		//TODO this method sucks, do something better
						int structureValue = 1;
						//see how big the house is
						StructureTemplate template = null;
						if (Mods.Get.Editor.LoadMod <StructureTemplate>(ref template, "Structure", structure.State.TemplateName)) {
								int numDoorsAndWindows = template.Exterior.GenericDoors.Length + template.Exterior.GenericWindows.Length;
								int stackItems = template.Exterior.UniqueWorlditems.Count;
								List <ChildPiece> genericDoors = new List<ChildPiece>();
								List <ChildPiece> genericWindows = new List<ChildPiece>();
								StructureTemplate.ExtractChildPiecesFromLayer(genericDoors, template.Exterior.GenericDoors);
								StructureTemplate.ExtractChildPiecesFromLayer(genericWindows, template.Exterior.GenericWindows);
								for (int i = 0; i < template.InteriorVariants.Count; i++) {
										StructureTemplate.ExtractChildPiecesFromLayer(genericDoors, template.InteriorVariants[i].GenericDoors);
										StructureTemplate.ExtractChildPiecesFromLayer(genericWindows, template.InteriorVariants[i].GenericWindows);
										numDoorsAndWindows += (genericDoors.Count + genericWindows.Count);
										stackItems += template.InteriorVariants[i].UniqueWorlditems.Count;
								}
								structureValue += numDoorsAndWindows + stackItems;
						}
						return structureValue;
				}
				#endif
		}

		[Serializable]
		public class ShingleState
		{
				public bool IsOwnedBy(string ownerName)
				{
						switch (ownerName) {
								case "[Player]":
										Debug.Log("Are we owned by player?");
										return PropertyStatus == PropertyStatusType.OwnedByPlayer;

								default:
										return PropertyStatus == PropertyStatusType.OwnedByCharacter;
						}
				}

				public MobileReference ParentStructure;
				public int PriceInMarks = -1;
				public PropertyStatusType PropertyStatus = PropertyStatusType.Abandoned;
				public STransform ForSaleSignOffset = STransform.zero;
				public List <int> AbandonedInteriorLayers = new List <int>();
				public double TimeStructureDestroyed = -1f;
				public string MoneylenderOwner = string.Empty;
				public bool AnnounceOwnership = false;
		}
}
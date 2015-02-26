using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.GUI
{
		public class GUILibraryCatalogueBrowser : GUIBrowserSelectView <LibraryCatalogueEntry>
		{
				public string LibraryName;
				public Library library;
				public GameObject PlaceOrderButton;
				public UILabel PlaceOrderButtonLabel;
				public UISprite NoSkillOverlay;
				public UILabel LibraryNameLabel;
				public UILabel LibraryMottoLabel;
				public UILabel BookDescriptionLabel;
				public UILabel BookStatusLabel;
				public Transform BookDopplegangerParent;
				public Transform BookOrderDopplegangerParent;
				public float BookRotationSpeed = 25f;
				public GameObject BookDoppleganger;
				public GameObject BookOrderDoppleganger;
				public GameObject OrderStatusProgressBarParent;
				public UISlider OrderStatusProgressBar;
				public UILabel OrderStatusLabelTop;
				public UILabel OrderStatusLabelBottom;
				public UISprite OrderStatusProgressBarGlow;
				public UISprite BookOrderShadow;
				public GenericWorldItem DopplegangerProps = new GenericWorldItem("Books", "BookAvatar");
				public Skill RequiredSkill = null;
				public bool SkillRequirementsMet = true;
				public bool ReceivedOrder = false;
				public bool PlacedOrder = false;
				public GUITabs Tabs;
				public LibraryCatalogueEntry CurrentOrder;
				public bool HasDeliveredBooks = false;
				public bool HasOrderedBooks = false;

				public override void Start()
				{
						base.Start();
						Tabs.Initialize(this);
						Tabs.OnSetSelection += OnSetSelection;
				}

				public void SetLibraryName(string newLibraryName)
				{		//TODO have it pass the actual library
						LibraryName = newLibraryName;
						Books.Get.Library(LibraryName, out library);
						SkillRequirementsMet = true;
						RequiredSkill = null;
						if (!string.IsNullOrEmpty(library.RequiredSkill)) {
								if (!Skills.Get.HasLearnedSkill(library.RequiredSkill, out RequiredSkill)) {
										SkillRequirementsMet = false;
								}
						}
						//LibraryMottoLabel.text = library.Motto;
						LibraryNameLabel.text = library.DisplayName;

						LibraryCatalogueEntry order = mSelectedObject;
						if (SkillRequirementsMet && Books.Get.HasPlacedOrder(LibraryName, out order)) {
								//show the order
								mSelectedObject = order;
								CurrentOrder = order;
								Tabs.DefaultPanel = "CurrentOrder";
								//Tabs.SetSelection("CurrentOrder");
						} else {
								//show the catalogue
								mSelectedObject = order;
								CurrentOrder = null;
								Tabs.DefaultPanel = "Catalogue";
								//Tabs.SetSelection("Catalogue");
						}

						Show();
						OnSetSelection();
				}

				public void OnSetSelection()
				{
						NoSkillOverlay.enabled = false;
						if (Tabs.SelectedTab == "Catalogue") {
								Debug.Log("OnSetSelection in book browser and tab is Catalogue");
								//just let the catalogue do its thing
								WorldItems.ReturnDoppleganger(BookOrderDoppleganger);
								PushEditObjectToNGUIObject();
						} else {
								Debug.Log("OnSetSelection in book browser and tab is Order");
								WorldItems.ReturnDoppleganger(BookDoppleganger);
								CurrentOrder = null;
								if (Books.Get.HasPlacedOrder(library.Name, out CurrentOrder)) {
										//update the order doppleganger and stuff
										BookOrderShadow.enabled = true;
										OrderStatusLabelBottom.enabled = true;
										DopplegangerProps.CopyFrom(CurrentOrder.BookObject);
										BookOrderDoppleganger = WorldItems.GetDoppleganger(DopplegangerProps, BookOrderDopplegangerParent, BookOrderDoppleganger, WIMode.Stacked);
										Vector3 doppleGangerPosition = BookOrderDoppleganger.transform.localPosition;
										doppleGangerPosition.z = 0f;
										BookOrderDoppleganger.transform.localPosition = doppleGangerPosition;
										OrderStatusProgressBarParent.gameObject.SetActive(true);
										if (Books.Get.DeliverBookOrder(LibraryName)) {
												ReceivedOrder = true;
												OrderStatusLabelTop.text = CurrentOrder.BookObject.DisplayName + " has been added to your log";
												OrderStatusLabelBottom.text = "Delivered";
												OrderStatusProgressBarGlow.alpha = 0f;//Colors.Alpha(Colors.Get.MessageSuccessColor, 0.35f);
												OrderStatusProgressBar.sliderValue = 1f;
										} else {
												OrderStatusLabelTop.text = CurrentOrder.BookObject.DisplayName + " has been ordered";
												OrderStatusProgressBar.sliderValue = CurrentOrder.NormalizedTimeUntilDelivery;
												OrderStatusProgressBarGlow.enabled = true;
												OrderStatusLabelBottom.text = "In Transit:";
										}
								} else {
										//get rid of the doppleganger, hide everything
										WorldItems.ReturnDoppleganger(BookOrderDoppleganger);
										OrderStatusProgressBarParent.SetActive(false);
										OrderStatusLabelBottom.enabled = false;
										OrderStatusLabelTop.enabled = true;
										BookOrderShadow.enabled = false;
										OrderStatusLabelTop.text = "You have no outstanding orders";
								}
						}
				}

				public override IEnumerable <LibraryCatalogueEntry> FetchItems()
				{
						HasDeliveredBooks = false;
						HasOrderedBooks = false;
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						if (library != null) {
								if (mSelectedObject == null) {
										mSelectedObject = library.CatalogueEntries[0];
								}
								return library.CatalogueEntries.AsEnumerable();
						}
						return null;
				}

				public override void CreateDividerObjects()
				{
						GameObject newDivider = null;
						GUIGenericBrowserObject dividerObject = null;

						if (HasDeliveredBooks) {
								newDivider = CreateDivider();
								dividerObject = newDivider.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_empty";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Delivered:";
								dividerObject.Initialize("Divider");
						}
						if (HasOrderedBooks) {
								newDivider = CreateDivider();
								dividerObject = newDivider.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "c_empty";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Ordered:";
								dividerObject.Initialize("Divider");
						}
						newDivider = CreateDivider();
						dividerObject = newDivider.GetComponent <GUIGenericBrowserObject>();
						dividerObject.name = "d_empty";
						dividerObject.UseAsDivider = true;
						dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
						dividerObject.Name.text = "Available:";
						dividerObject.Initialize("Divider");
				}

				protected override GameObject ConvertEditObjectToBrowserObject(LibraryCatalogueEntry editObject)
				{
						GameObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						newBrowserObject.name = "a_" + editObject.BookName;
						GUIGenericBrowserObject bookBrowserObject = newBrowserObject.GetComponent <GUIGenericBrowserObject>();

						bookBrowserObject.EditButton.target = this.gameObject;
						bookBrowserObject.EditButton.functionName = "OnClickBrowserObject";

						Color bookColor = Colors.Get.BookColorGeneric;
						Color textColor = Color.white;

						bookBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						bookBrowserObject.BackgroundHighlight.enabled = false;
						bookBrowserObject.Icon.spriteName = "SkillIconGuildLibrary";
						bookBrowserObject.MiniIcon.enabled = false;
						bookBrowserObject.MiniIconBackground.enabled = false;

						if (editObject.HasBeenDelivered) {
								HasDeliveredBooks = true;
								newBrowserObject.name = "b_" + newBrowserObject.name;
								bookBrowserObject.Background.color = Colors.Darken(Colors.Get.MessageSuccessColor);
								bookBrowserObject.Name.text = editObject.BookObject.DisplayName + Colors.ColorWrap(" (Delivered)", Colors.Dim(textColor));
						} else if (editObject.HasBeenPlaced) {
								HasOrderedBooks = true;
								newBrowserObject.name = "d_" + newBrowserObject.name;
								bookBrowserObject.Background.color = Colors.Get.MessageSuccessColor;
								bookBrowserObject.AttentionIcon.enabled = true;
								bookBrowserObject.Name.text = editObject.BookObject.DisplayName + Colors.ColorWrap(" (Order Placed)", Colors.Dim(textColor));
						} else {
								newBrowserObject.name = "f_" + newBrowserObject.name;
								bookBrowserObject.Background.color = bookColor;
								bookBrowserObject.Name.text = editObject.BookObject.DisplayName + Colors.ColorWrap(" - " + editObject.OrderPrice.ToString(), Colors.Dim(textColor));
						}

						bookBrowserObject.Initialize(editObject.BookName);
						bookBrowserObject.Refresh();

						return newBrowserObject;
				}

				public void OnClickPlaceOrderButton()
				{
						string error;
						LibraryCatalogueEntry order = mSelectedObject;
						if (!Books.Get.TryToPlaceBookOrder(LibraryName, mSelectedObject, out error)) {
								BookStatusLabel.text = error;
						} else {
								//there's a chance we've actually delivered an order by doing this
								PlacedOrder = true;
								Tabs.SetSelection("CurrentOrder");
						}
						mSelectedObject = order;
						//refresh
						PushEditObjectToNGUIObject();
				}

				protected List <string> mDescription = new List<string>();

				public override void PushSelectedObjectToViewer()
				{
						if (Tabs.SelectedTab != "Catalogue") {
								Debug.Log("Not pushing selected object, tab is not catalogue");
								return;
						}

						mDescription.Clear();

						if (SkillRequirementsMet) {
								NoSkillOverlay.enabled = false;
								if (mSelectedObject.HasBeenPlaced) {
										PlaceOrderButton.SendMessage("SetDisabled");
								} else if (!Books.Get.HasPlacedOrder(LibraryName)) {
										PlaceOrderButton.SendMessage("SetEnabled");
								}
						} else {
								PlaceOrderButton.SendMessage("SetDisabled");
								NoSkillOverlay.enabled = true;
						}

						if (mSelectedObject.HasBeenPlaced) {
								BookStatusLabel.text = "In Transit";
								if (mSelectedObject.HasBeenDelivered) {
										BookStatusLabel.text = "Delivered";
								} else if (mSelectedObject.HasArrived) {
										BookStatusLabel.text = "Arrived";
								}
						} else {
								if (SkillRequirementsMet) {
										BookStatusLabel.text = "Not Ordered";
								} else {
										BookStatusLabel.text = "This catalogue requires a skill you don't have. You can browse its contents but you cannot place orders.";
								}
						}
						//TODO use a string builder
						mDescription.Add(mSelectedObject.BookObject.DisplayName);
						mDescription.Add("_");
						if (!mSelectedObject.HasBeenDelivered) {
								Book book = null;
								bool hasLearnedSkills = true;
								if (Books.Get.BookByName(mSelectedObject.BookName, out book)) {
										if (book.SkillsToLearn.Count == 0) {
												hasLearnedSkills = false;
										}
										foreach (string skillName in book.SkillsToLearn) {
												if (!Skills.Get.HasLearnedSkill(skillName)) {
														hasLearnedSkills = false;
														break;
												}
										}
								}
								if (hasLearnedSkills) {
										mDescription.Add("You have already learned the skills taught by this book. However, re-reading it can still increase your aptitude.");
										mDescription.Add("_");
								}
								mDescription.Add(Colors.ColorWrap("Delivery time: ", Colors.Darken(Colors.Get.MenuButtonTextColorDefault)) + mSelectedObject.DeliveryTimeInHours.ToString() + " hours");
								mDescription.Add(Colors.ColorWrap("Price: ", Colors.Darken(Colors.Get.MenuButtonTextColorDefault)) + mSelectedObject.OrderPrice.ToString() + " " + Currency.TypeToString(mSelectedObject.CurrencyType));
								Color affordColor = Colors.Get.MessageSuccessColor;
								if (!Player.Local.Inventory.InventoryBank.CanAfford(mSelectedObject.OrderPrice, mSelectedObject.CurrencyType)) {
										affordColor = Colors.Get.MessageDangerColor;
								}
								mDescription.Add(Colors.ColorWrap("Your Funds: ", Colors.Darken(Colors.Get.MenuButtonTextColorDefault)) + Colors.ColorWrap(Player.Local.Inventory.InventoryBank.BaseCurrencyValue.ToString() + " " + Currency.TypeToString(WICurrencyType.A_Bronze), affordColor));
						} else {
								mDescription.Add("(This book has been added to your log)");
						}
						BookDescriptionLabel.text = mDescription.JoinToString("\n");
						DopplegangerProps.CopyFrom(mSelectedObject.BookObject);
						BookDoppleganger = WorldItems.GetDoppleganger(DopplegangerProps, BookDopplegangerParent, BookDoppleganger, WIMode.Stacked);
						Vector3 doppleGangerPosition = BookDoppleganger.transform.localPosition;
						doppleGangerPosition.z = 0f;
						BookDoppleganger.transform.localPosition = doppleGangerPosition;
				}

				protected override void OnFinish()
				{
						Debug.Log("Finishing in catalogue");
						//have the librarian talk to the player
						base.OnFinish();
						if (PlacedOrder | ReceivedOrder) {
								//if (string.IsNullOrEmpty(library.LibrarianCharacterName)) {
								//Debug.Log("Setting defaults in catalogue");
								library.SetDefaults();
								//}
								Character character = null;
								if (Characters.Get.SpawnedCharacter(library.LibrarianCharacterName, out character)) {
										Debug.Log("We got librarian character name");
										character.LookAtPlayer();
										if (ReceivedOrder && !gReceivedOrderOnceThisSession) {
												gReceivedOrderOnceThisSession = true;

												NGUIScreenDialog.AddSpeech(library.LibrarianDTSOnReceivedOrder.Replace("{motto}", library.Motto), character.worlditem.DisplayName, 2f);
										} else if (!gPlacedOrderOnceThisSession) {
												gPlacedOrderOnceThisSession = true;
												NGUIScreenDialog.AddSpeech(library.LibrarianDTSOnPlacedOrder.Replace("{motto}", library.Motto), character.worlditem.DisplayName, 2f);
										}		
								} else {
										Debug.Log("Couldn't find character " + library.LibrarianCharacterName);
										if (ReceivedOrder && !gReceivedOrderOnceThisSession) {
												gReceivedOrderOnceThisSession = true;
												NGUIScreenDialog.AddSpeech(library.LibrarianDTSOnReceivedOrder.Replace("{motto}", library.Motto), "Librarian", 2f);
										} else if (!gPlacedOrderOnceThisSession) {
												gPlacedOrderOnceThisSession = true;
												NGUIScreenDialog.AddSpeech(library.LibrarianDTSOnPlacedOrder.Replace("{motto}", library.Motto), "Librarian", 2f);
										}
								}
						}
				}

				protected static bool gReceivedOrderOnceThisSession = false;
				protected static bool gPlacedOrderOnceThisSession = false;

				public void Update()
				{
						BookDopplegangerParent.Rotate(0f, (float)(WorldClock.RTDeltaTimeSmooth * BookRotationSpeed), 0f);
						BookOrderDopplegangerParent.rotation = BookDopplegangerParent.rotation;
						OrderStatusProgressBarGlow.alpha = GUIProgressDialog.PingPongProgressBarGlow();
						if (CurrentOrder != null) {
								OrderStatusProgressBar.sliderValue = CurrentOrder.NormalizedTimeUntilDelivery;
								if (CurrentOrder.HasArrived) {
										//if it arrives, send the message
										if (Tabs.SelectedTab != "CurrentOrder") {
												Tabs.SetSelection("CurrentOrder");
										} else {
												OnSetSelection();
										}
								}
						}
				}
		}
}
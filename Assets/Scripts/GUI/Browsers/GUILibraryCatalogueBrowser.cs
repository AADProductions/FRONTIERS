using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;
using Frontiers.World;

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
				public float BookRotationSpeed = 25f;
				public GameObject BookDoppleganger;
				public GenericWorldItem DopplegangerProps = new GenericWorldItem("Books", "BookAvatar");
				public Skill RequiredSkill = null;
				public bool SkillRequirementsMet = true;

				public override bool PushToViewerAutomatically {
						get {
								return true;
						}
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
						LibraryMottoLabel.text = library.Motto;
						LibraryNameLabel.text = library.DisplayName;
						if (SkillRequirementsMet) {
								NoSkillOverlay.enabled = false;
						} else {
								NoSkillOverlay.enabled = true;
						}

						if (library.CatalogueEntries.Count > 0) {
								mSelectedObject = library.CatalogueEntries[0];
						}

						Books.Get.DeliverBookOrder(LibraryName);//will automatically deliver an order if it exists / has arrived

						Show();
				}

				public override IEnumerable <LibraryCatalogueEntry> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						if (library != null) {
								return library.CatalogueEntries.AsEnumerable();
						}
						return null;
				}

				protected override GameObject ConvertEditObjectToBrowserObject(LibraryCatalogueEntry editObject)
				{
						GameObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						if (editObject.OrderPrice > 0) {
								newBrowserObject.name = "b_" + editObject.OrderPrice.ToString().PadLeft(10, '0') + "_" + newBrowserObject.name;
						} else {
								newBrowserObject.name = "a_" + editObject.BookName;
						}
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
								newBrowserObject.name = "a_" + newBrowserObject.name;
								bookBrowserObject.Background.color = Colors.Darken(Colors.Get.MessageSuccessColor);
								bookBrowserObject.Name.text = editObject.BookObject.DisplayName + Colors.ColorWrap(" (Delivered)", Colors.Dim(textColor));
						} else if (editObject.HasBeenPlaced) {
								newBrowserObject.name = "b_" + newBrowserObject.name;
								bookBrowserObject.Background.color = Colors.Get.MessageSuccessColor;
								bookBrowserObject.AttentionIcon.enabled = true;
								bookBrowserObject.Name.text = editObject.BookObject.DisplayName + Colors.ColorWrap(" (Order Placed)", Colors.Dim(textColor));
						} else {
								newBrowserObject.name = "c_" + newBrowserObject.name;
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
						if (!Books.Get.TryToPlaceBookOrder(LibraryName, mSelectedObject, out error)) {
								BookStatusLabel.text = error;
						} else {
								BookStatusLabel.text = "Ordered";
								PlaceOrderButton.SendMessage("SetDisabled");
						}
						//refresh
						PushEditObjectToNGUIObject();
				}

				protected List <string> mDescription = new List<string>();

				public override void PushSelectedObjectToViewer()
				{
						mDescription.Clear();

						if (mSelectedObject.HasBeenPlaced) {
								BookStatusLabel.text = "In Transit";
								if (mSelectedObject.HasBeenDelivered) {
										BookStatusLabel.text = "Delivered";
								} else if (mSelectedObject.HasArrived) {
										BookStatusLabel.text = "Arrived";
								}
								PlaceOrderButton.SendMessage("SetDisabled");
						} else {
								if (SkillRequirementsMet) {
										BookStatusLabel.text = "Not Ordered";
										PlaceOrderButton.SendMessage("SetEnabled");
								} else {
										BookStatusLabel.text = "This catalogue requires a skill you don't have. You can browse its contents but you cannot place orders.";
										PlaceOrderButton.SendMessage("SetDisabled");
								}
						}
						//TODO use a string builder
						mDescription.Add(mSelectedObject.BookObject.DisplayName);
						mDescription.Add("_");
						mDescription.Add("Delivery time:");
						mDescription.Add(mSelectedObject.DeliveryTimeInHours.ToString() + " hours");
						mDescription.Add("Price:");
						Color affordColor = Colors.Get.MessageSuccessColor;
						if (!Player.Local.Inventory.InventoryBank.CanAfford(mSelectedObject.OrderPrice, mSelectedObject.CurrencyType)) {
								affordColor = Colors.Get.MessageDangerColor;
						}
						mDescription.Add(Colors.ColorWrap(mSelectedObject.OrderPrice.ToString() + " " + Currency.TypeToString(mSelectedObject.CurrencyType), affordColor));
						BookDescriptionLabel.text = mDescription.JoinToString("\n");
						DopplegangerProps.CopyFrom(mSelectedObject.BookObject);
						BookDoppleganger = WorldItems.GetDoppleganger(DopplegangerProps, BookDopplegangerParent, BookDoppleganger, WIMode.Stacked);
						Vector3 doppleGangerPosition = BookDoppleganger.transform.localPosition;
						doppleGangerPosition.z = 0f;
						BookDoppleganger.transform.localPosition = doppleGangerPosition;
				}

				public void Update()
				{
						BookDopplegangerParent.Rotate(0f, (float)(WorldClock.RTDeltaTimeSmooth * BookRotationSpeed), 0f);
				}
		}
}
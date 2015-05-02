using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers.GUI
{
		public class InventorySquareCurrency : InventorySquareDisplay
		{
				public WICurrencyType CurrencyType;
				public int CurrencyAmount;
				public int NumToRemoveOnClick = 1;
				public IBank RemoveFromFromOnClick;
				public IBank TransferToOnClick;
				public bool DetailedPrices = false;
				public UILabel MultiplierLabel;
				public GameObject BronzeDoppleganger;
				public UISprite MultiplierArrow;
				public UISprite CurrencyShadow;

				public override void OnEnable()
				{
						base.OnEnable();
						if (BronzeDoppleganger != null) {
								BronzeDoppleganger.SetActive(true);
						}
				}

				public override void OnDisable()
				{
						base.OnDisable();
						if (BronzeDoppleganger != null) {
								BronzeDoppleganger.SetActive(false);
						}
				}

				public override void UpdateDisplay()
				{
						string inventoryItemName = string.Empty;
						switch (CurrencyType) {
								case WICurrencyType.A_Bronze:
								default:
										DopplegangerProps.CopyFrom(Currency.BronzeGenericWorldItem);
										inventoryItemName = Currency.BronzeCurrencyNamePlural;
										break;

								case WICurrencyType.B_Silver:
										DopplegangerProps.CopyFrom(Currency.SilverGenericWorldItem);
										inventoryItemName = Currency.SilverCurrencyNamePlural;
										break;

								case WICurrencyType.C_Gold:
										DopplegangerProps.CopyFrom(Currency.GoldIGenericWorldItem);
										inventoryItemName = Currency.GoldCurrencyNamePlural;
										break;

								case WICurrencyType.D_Luminite:
										DopplegangerProps.CopyFrom(Currency.LumenGenericWorldItem);
										inventoryItemName = Currency.LumenCurrencyNamePlural;
										break;

								case WICurrencyType.E_Warlock:
										DopplegangerProps.CopyFrom(Currency.WarlockGenericWorldItem);
										inventoryItemName = Currency.WarlockCurrencyNamePlural + Colors.ColorWrap("\n(Warlock Coin)", Colors.Darken(InventoryItemName.color));
										InventoryItemName.multiLine = true;
										break;
						}

						StackNumberLabel.text = Colors.ColorWrap ("$", Colors.Darken (StackNumberLabel.color)) + CurrencyAmount.ToString();

						if (DetailedPrices) {
								StackNumberLabel.transform.localPosition = new Vector3(0f, 0f, -120f);//new Vector3(80f, 0f, -120f);
								StackNumberLabel.pivot = UIWidget.Pivot.Center;
								InventoryItemName.pivot = UIWidget.Pivot.Right;
								InventoryItemName.lineWidth = 200;

								MultiplierLabel.enabled = true;
								if (CurrencyType != WICurrencyType.A_Bronze) {
										BronzeDoppleganger = WorldItems.GetDoppleganger(Currency.BronzeGenericWorldItem, transform, BronzeDoppleganger, WIMode.Stacked, 0.4f);
								} else {
										BronzeDoppleganger = WorldItems.GetDoppleganger(Currency.BronzeGenericWorldItem, gameObject.FindOrCreateChild("BronzeDopParent").transform, BronzeDoppleganger, WIMode.Stacked, 0.4f);
								}
								BronzeDoppleganger.transform.localPosition = new Vector3(72f, 0f, -125f);
								MultiplierLabel.text = "+ " + Colors.ColorWrap ("$", Colors.Darken (StackNumberLabel.color)) + Currency.ConvertToBaseCurrency(CurrencyAmount, CurrencyType).ToString();
								MultiplierLabel.transform.localScale = Vector3.one * 20f;
								MultiplierLabel.transform.localPosition = new Vector3(90f, 0f, -125f);
								MultiplierArrow.enabled = true;
								MultiplierArrow.transform.localPosition = new Vector3(80f, 10f, 0f);
								CurrencyShadow.enabled = true;
								CurrencyShadow.color = Colors.Alpha(Color.black, 0.25f);
								CurrencyShadow.transform.localPosition = new Vector3(0f, 0f, -80f);
						} else {
								if (MultiplierArrow != null) {
										MultiplierArrow.enabled = false;
								}
								if (MultiplierLabel != null) {
										MultiplierLabel.enabled = false;
								}
						}

						DisplayMode = SquareDisplayMode.Enabled;
						ShowDoppleganger = true;
						MouseoverHover = false;

						if (InventoryItemName != null) {
								InventoryItemName.text = inventoryItemName;
						}

						base.UpdateDisplay();

						if (RemoveFromFromOnClick != null && TransferToOnClick != null) {
								MouseoverHover = true;
						} else {
								MouseoverHover = false;
						}
				}

				public void Update()
				{
						if (RemoveFromFromOnClick == null || TransferToOnClick == null) {
								return;
						} else if (UICamera.hoveredObject == gameObject) {
								if (InterfaceActionManager.RawScrollWheelAxis > 0.01f || InterfaceActionManager.RawScrollWheelAxis < -0.01f) {
										bool playSound = false;
										if (RemoveFromFromOnClick.TryToRemove(NumToRemoveOnClick * 5, CurrencyType, false)) {
												TransferToOnClick.Add(NumToRemoveOnClick * 5, CurrencyType);
												playSound = true;
										}

										if (playSound) {
												MasterAudio.PlaySound(SoundType, SoundNameSuccess);
										} else {
												MasterAudio.PlaySound(SoundType, SoundNameFailure);
										}
								}
						}
				}

				public virtual void OnClickSquare()
				{
						if (RemoveFromFromOnClick == null || TransferToOnClick == null) {
								return;
						}
						//this doesn't override anything because we're inheritying from InventorySquareDisplay
						bool playSound = false;
						if (RemoveFromFromOnClick.TryToRemove(NumToRemoveOnClick, CurrencyType, false)) {
								TransferToOnClick.Add(NumToRemoveOnClick, CurrencyType);
								playSound = true;
						}

						if (playSound) {
								MasterAudio.PlaySound(SoundType, SoundNameSuccess);
						} else {
								MasterAudio.PlaySound(SoundType, SoundNameFailure);
						}
				}
		}
}
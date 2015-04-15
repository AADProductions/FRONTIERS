using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System.Collections.Generic;

namespace Frontiers.GUI
{		//the bank is ugly, fix this class
		public class GUIBank : GUIObject, IGUITabPageChild
		{
				public InventorySquareCurrency BronzeSquare;
				public InventorySquareCurrency SilverSquare;
				public InventorySquareCurrency GoldSquare;
				public InventorySquareCurrency LumenSquare;
				public InventorySquareCurrency WarlockSquare;
				public UILabel SilverMultiplierLabel;
				public UILabel WarlockMultiplierLabel;
				public UILabel GoldMultiplierLabel;
				public UILabel LuminiteMultiplierLabel;
				public List <InventorySquareCurrency> Squares = new List <InventorySquareCurrency>();
				public UILabel TotalCurrencyLabel;
				public UILabel TotalCurrencyNumberLabel;
				public GameObject DisplayBase;
				public BankDisplayMode DisplayMode = BankDisplayMode.SmallTwoRows;

				public IBank BankToDisplay { get { return mBankToDisplay; } }

				public IBank BankToTransferTo { get { return mBankToTransferTo; } }

				public void Show()
				{
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = true;
						}
				}

				public void Hide()
				{
						for (int i = 0; i < Squares.Count; i++) {
								Squares[i].enabled = false;
						}
				}

				public bool HasBankToDisplay {
						get {
								return mBankToDisplay != null;
						}
				}

				public bool HasBankToTransferTo {
						get {
								return mBankToTransferTo != null;
						}
				}

				public void SetBank(IBank bankToDisplay)
				{
						DropBank();
						if (bankToDisplay != null) {
								mBankToDisplay = bankToDisplay;
								mBankToDisplay.RefreshAction += RefreshRequest;
								//Debug.Log ("Setting new bank to display in " + name);
								//already request refresh from DropBank
						}
						RefreshRequest();
				}

				public void SetBank(IBank bankToDisplay, IBank bankToTransferTo)
				{
						DropBank();
						if (bankToDisplay != null && bankToTransferTo != null && bankToDisplay != bankToTransferTo) {
								mBankToDisplay = bankToDisplay;
								mBankToDisplay.RefreshAction += RefreshRequest;
								//Debug.Log ("Setting new bank to display in " + name + " AND new bank to transfer money to");
								mBankToTransferTo = bankToTransferTo;
						}
						RefreshRequest();
				}

				public void DropBank()
				{
						if (HasBankToDisplay) {
								mBankToDisplay.RefreshAction -= RefreshRequest;
						}
						mBankToDisplay = null;
						RefreshRequest();
				}

				protected override void OnRefresh()
				{
						CreateSquares();

						if (HasBankToDisplay) {
								BronzeSquare.CurrencyAmount = mBankToDisplay.Bronze;
								SilverSquare.CurrencyAmount = mBankToDisplay.Silver;
								GoldSquare.CurrencyAmount = mBankToDisplay.Gold;
								LumenSquare.CurrencyAmount = mBankToDisplay.Lumen;
								WarlockSquare.CurrencyAmount = mBankToDisplay.Warlock;

								if (TotalCurrencyNumberLabel != null) {
										TotalCurrencyNumberLabel.text = mBankToDisplay.BaseCurrencyValue.ToString();
								}
						}

						BronzeSquare.RemoveFromFromOnClick = mBankToDisplay;
						SilverSquare.RemoveFromFromOnClick = mBankToDisplay;
						GoldSquare.RemoveFromFromOnClick = mBankToDisplay;
						LumenSquare.RemoveFromFromOnClick = mBankToDisplay;
						WarlockSquare.RemoveFromFromOnClick = mBankToDisplay;

						BronzeSquare.TransferToOnClick = mBankToTransferTo;
						SilverSquare.TransferToOnClick = mBankToTransferTo;
						GoldSquare.TransferToOnClick = mBankToTransferTo;
						LumenSquare.TransferToOnClick = mBankToTransferTo;
						WarlockSquare.TransferToOnClick = mBankToTransferTo;

						BronzeSquare.RefreshRequest();
						SilverSquare.RefreshRequest();
						GoldSquare.RefreshRequest();
						LumenSquare.RefreshRequest();
						WarlockSquare.RefreshRequest();
				}

				protected void CreateSquares()
				{
						if (Squares.Count > 0) {
								return;
						}

						if (DisplayBase == null) {
								DisplayBase = gameObject;
						}

						switch (DisplayMode) {
								case BankDisplayMode.SmallTwoRows:
								default:
										CreateSmallTwoRowSquares();
										break;

								case BankDisplayMode.LargeTwoRows:
										CreateLargeTwoRows();
										break;

								case BankDisplayMode.VerticalRows:
										CreateVerticalSquares();
										break;
						}
				}

				protected void CreateLargeTwoRows()
				{
						GameObject squarePrefab = GUIManager.Get.InventorySquareCurrencyLarge;
						BronzeSquare = InstantiateSquare(squarePrefab, WICurrencyType.A_Bronze, new Vector3(0f, 0f, 0f));
						SilverSquare = InstantiateSquare(squarePrefab, WICurrencyType.B_Silver, new Vector3(125f, 0f, 0f));
						GoldSquare = InstantiateSquare(squarePrefab, WICurrencyType.C_Gold, new Vector3(250f, 0f, 0f));
						LumenSquare = InstantiateSquare(squarePrefab, WICurrencyType.D_Luminite, new Vector3(62.5f, -125f, 0f));
						WarlockSquare = InstantiateSquare(squarePrefab, WICurrencyType.E_Warlock, new Vector3(187.5f, -125f, 0f));
				}

				protected void CreateVerticalSquares() {
						GameObject squarePrefab = GUIManager.Get.InventorySquareCurrencyLarge;
						BronzeSquare = InstantiateSquare(squarePrefab, WICurrencyType.A_Bronze, new Vector3(40f, 75f, 0f));
						SilverSquare = InstantiateSquare(squarePrefab, WICurrencyType.B_Silver, new Vector3(40f, -25f, 0f));
						WarlockSquare = InstantiateSquare(squarePrefab, WICurrencyType.E_Warlock, new Vector3(40f, -125f, 0f));
						GoldSquare = InstantiateSquare(squarePrefab, WICurrencyType.C_Gold, new Vector3(40f, -225f, 0f));
						LumenSquare = InstantiateSquare(squarePrefab, WICurrencyType.D_Luminite, new Vector3(40f, -325f, 0f));

						Vector3 stackLabelPosition = new Vector3(50f, 0f, -125f);
						BronzeSquare.StackNumberLabel.transform.localPosition = stackLabelPosition;
						SilverSquare.StackNumberLabel.transform.localPosition = stackLabelPosition;
						GoldSquare.StackNumberLabel.transform.localPosition = stackLabelPosition;
						LumenSquare.StackNumberLabel.transform.localPosition = stackLabelPosition;
						WarlockSquare.StackNumberLabel.transform.localPosition = stackLabelPosition;

						Vector3 labelPosition = new Vector3(-70f, 0f, -125f);
						BronzeSquare.InventoryItemName.transform.localPosition = labelPosition;
						SilverSquare.InventoryItemName.transform.localPosition = labelPosition;
						GoldSquare.InventoryItemName.transform.localPosition = labelPosition;
						LumenSquare.InventoryItemName.transform.localPosition = labelPosition;
						WarlockSquare.InventoryItemName.transform.localPosition = labelPosition;

						BronzeSquare.DetailedPrices = true;
						SilverSquare.DetailedPrices = true;
						GoldSquare.DetailedPrices = true;
						LumenSquare.DetailedPrices = true;
						WarlockSquare.DetailedPrices = true;
				}

				protected void CreateSmallTwoRowSquares()
				{
						GameObject squarePrefab = GUIManager.Get.InventorySquareCurrencySmall;
						BronzeSquare = InstantiateSquare(squarePrefab, WICurrencyType.A_Bronze, new Vector3(0f, 0f, 0f));
						SilverSquare = InstantiateSquare(squarePrefab, WICurrencyType.B_Silver, new Vector3(75f, 0f, 0f));
						GoldSquare = InstantiateSquare(squarePrefab, WICurrencyType.C_Gold, new Vector3(150f, 0f, 0f));
						LumenSquare = InstantiateSquare(squarePrefab, WICurrencyType.D_Luminite, new Vector3(37.5f, -75f, 0f));
						WarlockSquare = InstantiateSquare(squarePrefab, WICurrencyType.E_Warlock, new Vector3(112.5f, -75f, 0f));
				}

				protected InventorySquareCurrency InstantiateSquare(GameObject prefab, WICurrencyType type, Vector3 position)
				{
						GameObject instantiatedSquareGameObject = NGUITools.AddChild(DisplayBase, prefab);
						instantiatedSquareGameObject.transform.localPosition = position;
						InventorySquareCurrency currencySquare = instantiatedSquareGameObject.GetComponent <InventorySquareCurrency>();
						currencySquare.CurrencyType = type;
						currencySquare.CurrencyAmount = 0;
						Squares.Add(currencySquare);
						return currencySquare;
				}

				public enum BankDisplayMode
				{
						SmallOneRow,
						SmallTwoRows,
						LargeOneRow,
						LargeTwoRows,
						VerticalRows,
				}

				protected IBank mBankToDisplay;
				protected IBank mBankToTransferTo;
		}
}

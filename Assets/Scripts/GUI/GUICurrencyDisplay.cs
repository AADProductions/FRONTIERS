using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI {
	public class GUICurrencyDisplay : MonoBehaviour
	{
	//	public Bank BankToDisplay;
	//
	//	public bool HasBankToDisplay {
	//		get {
	//			return BankToDisplay != null;
	//		}
	//	}
	//
	//	public InventorySquareDisplay CopperDisplay;
	//	public InventorySquareDisplay SilverDisplay;
	//	public InventorySquareDisplay GoldDisplay;
	//	public InventorySquareDisplay LumenDisplay;
	//	public InventorySquareDisplay WarlockDisplay;
	//
	//	public void OnEnable ()
	//	{
	//		if (!HasBankToDisplay) {
	//			BankToDisplay = Player.Local.Inventory.State.PlayerBank;
	//		}
	//		Refresh ();
	//	}
	//
	//	public void Refresh ()
	//	{
	//		if (HasBankToDisplay) {			
	//			RefreshCurrencyDisplay (IronDisplay, BankToDisplay.Copper, "Tribit");
	//			RefreshCurrencyDisplay (CopperDisplay, BankToDisplay.Copper, "Triskel");
	//			RefreshCurrencyDisplay (SilverDisplay, BankToDisplay.Silver, "Solv Coin");
	//			RefreshCurrencyDisplay (GoldDisplay, BankToDisplay.Gold, "Aurum Coin");
	//			RefreshCurrencyDisplay (LumenDisplay, BankToDisplay.Lumen, "Lumen");
	//			RefreshCurrencyDisplay (WarlockDisplay, BankToDisplay.Warlock, "Warlock");
	//		} else {
	//			CopperDisplay.DisplayMode = SquareDisplayMode.Disabled;
	//			SilverDisplay.DisplayMode = SquareDisplayMode.Disabled;
	//			GoldDisplay.DisplayMode = SquareDisplayMode.Disabled;
	//			LumenDisplay.DisplayMode = SquareDisplayMode.Disabled;
	//			WarlockDisplay.DisplayMode = SquareDisplayMode.Disabled;
	//		}
	//	}
	//
	//	protected void RefreshCurrencyDisplay (InventorySquareDisplay display, int currencyNum, string displayName)
	//	{
	//		if (currencyNum < 0) {
	//			display.DisplayMode = SquareDisplayMode.Enabled;
	//		} else {
	//			display.DisplayMode = SquareDisplayMode.Empty;
	//		}
	//		
	//		display.InventoryItemName.text = displayName;
	//		display.StackNumberLabel.text	= currencyNum.ToString ();
	//		display.UpdateDisplay ();
	//	}
	//
	//	public void DisplayBank (Bank bankToDisplay)
	//	{
	//		BankToDisplay = bankToDisplay;
	//		Refresh ();
	//	}
	}
}
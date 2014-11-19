using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI {
	public class InventorySquareCurrency : InventorySquareDisplay
	{
		public WICurrencyType CurrencyType; 
		public int CurrencyAmount;
		public int NumToRemoveOnClick = 1;
		public IBank RemoveFromFromOnClick;
		public IBank TransferToOnClick;

		public override void UpdateDisplay ()
		{
			StackNumberLabel.text = CurrencyAmount.ToString ();
			string inventoryItemName = string.Empty;
			switch (CurrencyType) {
			case WICurrencyType.A_Bronze:
			default:
				DopplegangerProps.CopyFrom (Currency.BronzeGenericWorldItem);
				inventoryItemName = Currency.BronzeCurrencyNamePlural;
				break;

			case WICurrencyType.B_Silver:
				DopplegangerProps.CopyFrom (Currency.SilverGenericWorldItem);
				inventoryItemName = Currency.SilverCurrencyNamePlural;
				break;

			case WICurrencyType.C_Gold:
				DopplegangerProps.CopyFrom (Currency.GoldIGenericWorldItem);
				inventoryItemName = Currency.GoldCurrencyNamePlural;
				break;

			case WICurrencyType.D_Luminite:
				DopplegangerProps.CopyFrom (Currency.LumenGenericWorldItem);
				inventoryItemName = Currency.LumenCurrencyNamePlural;
				break;

			case WICurrencyType.E_Warlock:
				DopplegangerProps.CopyFrom (Currency.WarlockGenericWorldItem);
				inventoryItemName = Currency.WarlockCurrencyNamePlural;
				break;
			}

			DisplayMode = SquareDisplayMode.Enabled;
			ShowDoppleganger = true;
			MouseoverHover = false;

			if (InventoryItemName != null) {
				InventoryItemName.text = inventoryItemName;
			}

			base.UpdateDisplay ();

			if (RemoveFromFromOnClick != null && TransferToOnClick != null) {
				MouseoverHover = true;
			} else {
				MouseoverHover = false;
			}
		}

		public virtual void OnClickSquare ()
		{
			if (RemoveFromFromOnClick == null || TransferToOnClick == null) {
				return;
			}
			//this doesn't override anything because we're inheritying from InventorySquareDisplay
			bool playSound = false;
			int numRemoved = 0;
			if (RemoveFromFromOnClick.TryToRemove (NumToRemoveOnClick, ref numRemoved, CurrencyType)) {
				TransferToOnClick.Add (numRemoved, CurrencyType);
				playSound = true;
			}

			if (playSound) {
				MasterAudio.PlaySound (SoundType, SoundNameSuccess);
			} else {
				MasterAudio.PlaySound (SoundType, SoundNameFailure);
			}
		}

	}
}
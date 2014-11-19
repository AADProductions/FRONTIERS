using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI {
	public class InventorySquareCraftingResult : InventorySquareDisplay
	{
		public GenericWorldItem CraftedItemTemplate
		{
			get{
				return mCraftedItemTemplate;
			}
			set {
				mCraftedItemTemplate = value;
				UpdateDisplay ();
			}
		}
		public bool RequirementsMet
		{
			get {
				return mRequirementsMet;
			}
			set {
				mRequirementsMet = value;
				RefreshRequest ();
			}
		}
		public int NumItemsPossible
		{
			get{
				return mNumItemsPossible;
			}
			set{
				mNumItemsPossible = value;
				RefreshRequest ();
			}
		}
		public int NumItemsCrafted
		{
			get{
				return mNumItemsCrafted;
			}
			set{
				mNumItemsCrafted = value;
				RefreshRequest ();
			}
		}

		public bool HasItemTemplate {
			get {
				return CraftedItemTemplate != null && !CraftedItemTemplate.IsEmpty;
			}
		}

		public bool ReadyForRetrieval {
			get {
				return CraftedItemTemplate != null && !CraftedItemTemplate.IsEmpty && NumItemsCrafted > 0;
			}
		}

		public void OnClickSquare ()
		{
			WIStackError error = WIStackError.None;
			if (ReadyForRetrieval) {
				while (NumItemsCrafted > 0) {
					StackItem craftedItem = CraftedItemTemplate.ToStackItem ();
					Stacks.Push.Item (Player.Local.Inventory.SelectedStack, craftedItem, ref error);
					NumItemsCrafted--;
				}
			}
		}

		public override void UpdateDisplay ()
		{
			InventoryItemName.text = string.Empty;
			DisplayMode = SquareDisplayMode.Disabled;
			string stackNumberLabelText = string.Empty;
			ShowDoppleganger = false;
			MouseoverHover = false;
			DopplegangerProps.CopyFrom (mCraftedItemTemplate);

			if (HasItemTemplate) {
				DisplayMode = SquareDisplayMode.Enabled;
				ShowDoppleganger = true;

				if (NumItemsCrafted > 0) {
					DisplayMode = SquareDisplayMode.Success;
					MouseoverHover = true;
					DopplegangerMode = WIMode.Stacked;
					stackNumberLabelText = NumItemsCrafted.ToString ();
				} else {
					DopplegangerMode = WIMode.Stacked;
					if (NumItemsPossible > 0) {
						stackNumberLabelText = NumItemsPossible.ToString ();
					}
				}
			}

			StackNumberLabel.text = stackNumberLabelText;
		
			base.UpdateDisplay ();
		}

		protected bool mRequirementsMet = false;
		protected GenericWorldItem mCraftedItemTemplate = null;
		protected int mNumItemsPossible = 1;
		protected int mNumItemsCrafted = 0;
	}
}
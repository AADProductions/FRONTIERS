using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI {
	public class InventorySquareBarterOffer : InventorySquareBarter
	{
		public BarterGoods Goods {
			get {
				return mGoods;
			}
		}

		public bool HasGoods {
			get {
				return mGoods != null;
			}
		}

		public override bool IsEnabled {
			get {
				return HasSession;		
			}
		}

		public bool HasItemsToOffer {
			get {
				bool result = false;
				if (HasGoods) {
					return mGoods.TopItem != null;
				}
				return result;
			}
		}

		public bool IsStolen {
			get {
				if (HasGoods && mGoods.TopItem != null) {
					return mGoods.TopItem.Is <Stolen> ();
				}
				return false;
			}
		}

		public override void OnClickSquare ()
		{
			if (!IsEnabled) {
				return;
			}

			if (HasGoods) {
				Session.RemoveGoods (Party, mGoods.TopItem, 1);
				MasterAudio.PlaySound (SoundType, "InventoryPickUpStack");
			} else {
				MasterAudio.PlaySound (SoundType, SoundNameFailure);
			}
			RefreshRequest ();
		}

		public override void SetSession (BarterSession newSession)
		{
			base.SetSession (newSession);

			if (HasSession)
			{
				if (Party == BarterParty.Player) {
					SetGoods (Session.PlayerGoods [Index]);
				} else {
					SetGoods (Session.CharacterGoods [Index]);
				}
			}
		}

		public void SetGoods (BarterGoods newGoods)
		{
			mGoods = newGoods;
			RefreshRequest ();
		}

		public override void SetStack (WIStack stack)
		{
			return;
		}

		public override void SetInventoryStackNumber ()
		{
			string stackNumberLabelText = string.Empty;
			if (IsEnabled && HasGoods) {
				if (IsStolen) {
					//you bastard you stole it!
					stackNumberLabelText = "(Stolen)"; 
				} else {
					int numItems = Goods.NumItems;
					if (numItems > 0) {
						stackNumberLabelText = numItems.ToString ();
					}
				}
			}
			StackNumberLabel.text = stackNumberLabelText;
		}

		public override void SetProperties ()
		{
			DisplayMode = SquareDisplayMode.Empty;
			MouseoverHover = false;
			DopplegangerMode = WIMode.Stacked;
			ShowDoppleganger = false;
			DopplegangerProps.State = "Default";

			if (IsEnabled) {
				DisplayMode = SquareDisplayMode.Enabled;
				if (HasItemsToOffer) {
					MouseoverHover = true;
					ShowDoppleganger = true;
					IWIBase topItem = Goods.TopItem;
					DopplegangerProps.CopyFrom (topItem);
					if (IsStolen) {
						DisplayMode = SquareDisplayMode.Error;
					}
				}
			}
		}

		protected BarterGoods mGoods;
	}
}
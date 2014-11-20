using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

#pragma warning disable 0219

namespace Frontiers.GUI {
	public class InventorySquareWearable : InventorySquare
	{
		public override bool IsEnabled {
			get {
				return HasStack;
			}
		}

		public WearableType Type = WearableType.Clothing;
		public BodyPartType BodyPart = BodyPartType.Head;
		public BodyOrientation Orientation = BodyOrientation.None;
		public float NormalizedDamage = 0f;
		public int FingerIndex = 0;

		public override void OnClickSquare ()
		{
			if (!IsEnabled) {
				return;
			}

			bool result = false;
			bool pickUp = false;
			bool playSound = false;
			bool playErrorSound = false;
			WIStackError error	= WIStackError.None;

			if (Player.Local.Inventory.SelectedStack.HasTopItem) {
				Wearable wearable = null;
				IWIBase topItem = Player.Local.Inventory.SelectedStack.TopItem;
				if (Wearable.CanWear (Type, BodyPart, Orientation, topItem)) {
					if (mStack.HasTopItem) {
						if (Stacks.Swap.Stacks (mStack, Player.Local.Inventory.SelectedStack, ref error)) {
							pickUp = true;
							playSound = true;
							result = true;
						} else {
							result = false;
							playSound = true;
						}
					} else {
						Stacks.Add.Items (Player.Local.Inventory.SelectedStack, mStack, ref error);
						playSound = true;
						result = true;
					}
				} else {
					result = false;
					playErrorSound = true;
				}
			} else {
				Stacks.Add.Items (mStack, Player.Local.Inventory.SelectedStack, ref error);
				pickUp = true;
				playSound = true;
				result = true;
			}
			
			if (playSound) {
				if (pickUp) {
					MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "InventoryPickUpStack");
				} else {
					MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "InventoryPlaceStack");
				}
			} else if (playErrorSound) {
				MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "ButtonClickDisabled");
			}
			
			RefreshRequest ();
		}

		public bool PushWearable (Wearable wearable) {
			if (!mStack.HasTopItem && Wearable.CanWear (Type, BodyPart, Orientation, wearable.worlditem)) {
				WIStackError error = WIStackError.None;
				return Stacks.Add.Items (Player.Local.Inventory.SelectedStack, mStack, ref error);
			}
			return false;
		}

		public override void SetProperties ()
		{
			ShowDoppleganger = false;
			DopplegangerProps.PackName = string.Empty;
			DopplegangerProps.PrefabName = string.Empty;
			DopplegangerMode = WIMode.Stacked;
			DopplegangerProps.State = "Default";
			MouseoverHover = false;
			NormalizedDamage = 0f;
			DisplayMode = SquareDisplayMode.Empty;

			if (IsEnabled) {
				MouseoverHover = true;
				if (mStack.HasTopItem) {
					DisplayMode = SquareDisplayMode.Enabled;
					IWIBase topItem = mStack.TopItem;
					DopplegangerProps.CopyFrom (topItem);
					ShowDoppleganger = true;
					//get the damageable properties
					System.Object damageableStateObject = null;
					if (topItem.GetStateOf <Damageable> (out damageableStateObject)) {
						DamageableState state = damageableStateObject as DamageableState;
						NormalizedDamage = state.NormalizedDamage;
					}
				}
			}
		}

		public override void SetInventoryStackNumber ()
		{
			if (HasStack && mStack.HasTopItem) {
				StackNumberLabel.text = string.Empty;
			} else {
				StackNumberLabel.text = BodyPart.ToString ();
				StackNumberLabel.color = Colors.Darken (Colors.Get.WorldMapLabelColor);
			}
		}

		public override void SetShadow ()
		{
			Color backgroundColor = Color.white;
			Color shadowColor = Colors.Alpha (Color.black, 0.5f);
			if (DisplayMode == SquareDisplayMode.Enabled) {
				Shadow.enabled = true;
				shadowColor = Colors.Alpha (Color.Lerp (Colors.Get.GenericHighValue, Colors.Get.GenericLowValue, NormalizedDamage), 0.5f);
			} else {
				Shadow.enabled = false;
			}

			Shadow.color = shadowColor;
			Background.color = backgroundColor;

			if (ActiveHighlight != null) {
				ActiveHighlight.enabled = false;
			}
		}

		public override void SetStack (WIStack stack)
		{
			stack.Mode = WIStackMode.Wearable;
			base.SetStack (stack);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI {
	public class InventorySquareCrafting : InventorySquare
	{
		public override bool IsEnabled {
			get {
				return HasStack;
			}
		}

		public bool AreRequirementsMet {
			get {
				mRequirementsMet = false;
				if (EnabledForBlueprint && HasStack && mStack.HasTopItem) {
					mRequirementsMet = CraftSkill.AreRequirementsMet (Stack.TopItem, RequiredItemTemplate, Strictness, Stack.NumItems, out mNumCraftableItems);
				}
				return mRequirementsMet;
			}
		}

		public GenericWorldItem RequiredItemTemplate;
		public bool EnabledForBlueprint = false;
		public BlueprintStrictness Strictness = BlueprintStrictness.Default;

		public int NumCraftableItems {
			get {
				if (EnabledForBlueprint && mRequirementsMet) {
					return mNumCraftableItems;
				}
				return 0;
			}
		}

		public override void OnClickSquare ()
		{
			if (EnabledForBlueprint) {
				base.OnClickSquare ();
			}
		}

		public override void SetProperties ()
		{
			DisplayMode = SquareDisplayMode.Disabled;
			ShowDoppleganger = false;
			MouseoverHover = false;
			DopplegangerMode = WIMode.Crafting;

			if (!EnabledForBlueprint) {
				DopplegangerProps.Clear ();
			} else {
				MouseoverHover = true;
				ShowDoppleganger = true;
				DopplegangerProps.Clear ();
				if (HasStack && mStack.HasTopItem) {
					DopplegangerMode = WIMode.Stacked;
					IWIBase topItem = mStack.TopItem;
					DopplegangerProps.CopyFrom (topItem);
					if (AreRequirementsMet) {
						DisplayMode = SquareDisplayMode.Success;
					} else {
						DisplayMode = SquareDisplayMode.Error;
					}
				} else { 
					//we know this because we can't meet requirements without items
					mRequirementsMet = false;
					mNumCraftableItems = 0;
					DopplegangerProps.CopyFrom (RequiredItemTemplate);
					DisplayMode = SquareDisplayMode.Enabled;
				}
			}
		
		}

		public void SetRequiredItem (GenericWorldItem newRequirement)
		{
			RequiredItemTemplate = newRequirement;
			RefreshRequest ();
		}

		protected int mNumCraftableItems = 0;
		protected bool mRequirementsMet = false;
	}
}
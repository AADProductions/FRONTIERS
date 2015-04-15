using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers.GUI
{
		public class InventoryEnablerDisplay : InventoryEnabler
		{
				public override bool IsEnabled {
						get {
								if (mContainerToDisplay != null) {
										return true;
								}
								return false;
						}
				}

				public WIStackContainer ContainerToDisplay {
						get {
								return mContainerToDisplay;
						}
						set {
								mContainerToDisplay = value;
						}
				}

				public bool ShowDefaultContainerWhenEmpty = false;

				public override void SetStack(WIStack stack)
				{
						//wtf are you doing
						return;
				}

				public override void SetProperties()
				{
						DisplayMode = SquareDisplayMode.Disabled;
						ShowDoppleganger = false;
						DopplegangerMode = WIMode.Stacked;
						DopplegangerProps.State = "Default";
						WorldItem worlditem = null;

						if (IsEnabled) {
								DisplayMode = SquareDisplayMode.Enabled;
								IStackOwner owner = null;
								if (mContainerToDisplay.HasOwner(out owner)) {
										IWIBase wiOwner = owner.worlditem;
										DopplegangerProps.CopyFrom(wiOwner);
										DopplegangerMode = WIMode.Stacked;
										ShowDoppleganger = true;
								} else {
										DisplayMode = SquareDisplayMode.Error;
								}
						} else if (ShowDefaultContainerWhenEmpty) {
								DisplayMode = SquareDisplayMode.Enabled;
								DopplegangerProps.CopyFrom(Container.DefaultContainerGenericWorldItem);
								DopplegangerMode = WIMode.Stacked;
								ShowDoppleganger = true;
						}
				}

				public override void OnClickSquare()
				{
						return;
				}

				public override void SetInventoryStackNumber()
				{
						StackNumberLabel.enabled = false;
				}

				protected WIStackContainer mContainerToDisplay;
		}
}
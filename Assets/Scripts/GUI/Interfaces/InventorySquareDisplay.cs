using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
#pragma warning disable 0219//TODO get rid of this crap

namespace Frontiers.GUI {
	public class InventorySquareDisplay : GUICircularBrowserObject
	{
		public SquareDisplayMode DisplayMode;
		public bool MouseoverHover = true;
		public string MouseoverIcon = string.Empty;
		public bool ShowDoppleganger;
		public GenericWorldItem DopplegangerProps = GenericWorldItem.Empty;
		public WIMode DopplegangerMode = WIMode.Stacked;

		public override void Awake ()
		{
			ShowDoppleganger = false;
			DopplegangerProps.Clear ();
			DopplegangerMode = WIMode.Stacked;
			MouseoverHover = false;
			DisplayMode = SquareDisplayMode.Disabled;

			base.Awake ();
		}

		public virtual void Start ()
		{
			if (InventoryItemName != null) {
				InventoryItemName.lineWidth = 95;
				InventoryItemName.depth = 103;
			}

			if (Background != null)
				Background.depth = 100;

			if (ActiveHighlight != null)
				ActiveHighlight.depth = 101;

			if (WeightLabel != null)
				WeightLabel.depth = 102;

			if (Shadow != null)
				Shadow.depth = 103;

			if (StackNumberLabel != null) {
				StackNumberLabel.depth = 104;
				Vector3 stackNumberPos = StackNumberLabel.transform.localPosition;
				stackNumberPos.z = -120f;
				StackNumberLabel.transform.localPosition = stackNumberPos;
			}
		}

		protected override void OnRefresh ( )
		{
			UpdateDisplay ( );
		}

		public virtual void UpdateDisplay ()
		{
			SetShadow ();
			UpdateDoppleganger ();
			UpdateMouseoverHover ();
		}

		public virtual void SetShadow ( )
		{
			Color backgroundColor = Color.white;
			Color shadowColor = Color.black;
			Color activeHighlightColor = Color.white;
			bool shadowEnabled = true;
			bool activeHighlightEnabled = false;

			switch (DisplayMode) {
			case SquareDisplayMode.Empty:
				shadowEnabled = false;
				backgroundColor = Colors.Alpha (backgroundColor, 0.5f);
				ShowDoppleganger = false;
				break;

			case SquareDisplayMode.Disabled:
				shadowEnabled = false;
				backgroundColor = Colors.Disabled (backgroundColor, 0.5f);
				ShowDoppleganger = false;
				MouseoverHover = false;
				break;

			case SquareDisplayMode.Error:
				shadowColor = Colors.Alpha (Colors.Get.MessageDangerColor, 0.5f);
				break;

			case SquareDisplayMode.Success:
				shadowColor = Colors.Alpha (Colors.Get.MessageSuccessColor, 0.5f);
				break;

			case SquareDisplayMode.GlassCase:
				shadowEnabled = false;
				break;

			case SquareDisplayMode.SoldOut:
				activeHighlightEnabled = true;
				activeHighlightColor = Colors.Disabled (activeHighlightColor, 0.5f);
				break;

			default:
			case SquareDisplayMode.Enabled:
				shadowEnabled = false;
				break;
			}

			Shadow.enabled = shadowEnabled;
			Shadow.color = shadowColor;
			Background.color = backgroundColor;
			if (ActiveHighlight != null) {
				ActiveHighlight.enabled = activeHighlightEnabled;
				ActiveHighlight.color = activeHighlightColor;
			}
		}

		public virtual void UpdateDoppleganger ( )
		{
			if (enabled && ShowDoppleganger) {
				Doppleganger = WorldItems.GetDoppleganger (DopplegangerProps, transform, Doppleganger, DopplegangerMode, Dimensions.x / 100f);
			} else {
				GameObject.Destroy (Doppleganger);
			}
		}

		public virtual void UpdateMouseoverHover ( )
		{
			if (MouseoverHover) {
				ButtonScale.hover = Vector3.one * 1.05f;
			} else {
				ButtonScale.hover = Vector3.one;
				mHover = false;
				mMouseOverUpdate = false;
			}
		}

		public virtual void OnHover (bool isOver)
		{
			if (isOver) {
				mHover = true;
				GUIInventoryInterface.MouseOverSquare = this;
				if (mMouseOverUpdate) {
					RefreshRequest ();
					mMouseOverUpdate = false;
				}
			} else {
				if (GUIInventoryInterface.MouseOverSquare == this) {
					GUIInventoryInterface.MouseOverSquare = null;
				}
				mHover = false;
				mMouseOverUpdate = true;
				RefreshRequest ();
			}
		}

		protected bool mHover = false;
		protected bool mMouseOverUpdate = false;
	}

	public enum SquareDisplayMode
	{
		Empty,
		Enabled,
		Disabled,
		Error,
		Success,
		GlassCase,
		SoldOut
	}
}
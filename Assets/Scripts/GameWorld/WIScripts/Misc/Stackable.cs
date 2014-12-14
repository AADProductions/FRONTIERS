using UnityEngine;
using System.Collections;

namespace Frontiers.World
{
		public class Stackable : WIScript
		{
				//items must be stackable to enter inventory
				//used mostly to store what size/rotation to use
				//when displaying the item in inventory squares
				public float SquareScale = 1.0f;
				public Vector3 SquareOffset = Vector3.zero;
				public Vector3 SquareRotation = Vector3.zero;

				public int MaxItems {
						get {
								return MaxItemsFromSize(worlditem.Flags.Size);
						}
				}

				public float SelectedScale {
						get {
								return SquareScale * 1.5f;
						}
				}

				public Vector3 SelectedRotation {
						get {
								return SquareRotation;
						}
				}

				public override void OnInitialized()
				{
						worlditem.OnModeChange += OnModeChange;
				}

				public override void OnModeChange()
				{
						if (worlditem.Is(WIMode.Stacked)) {
								gameObject.SendMessage("OnLosePlayerFocus");
								worlditem.ActiveStateLocked = false;
								worlditem.ActiveState = WIActiveState.Invisible;
								worlditem.ActiveStateLocked = true;
								//this will put us back in our group where we belong
								worlditem.UnlockTransform();
								transform.localPosition = new Vector3(-5000f, -5000f, -5000f);
								worlditem.rigidbody.isKinematic	= true;
						}
				}

				public static int MaxItemsFromSize(WISize size)
				{
						int maxItems = 1;

						switch (size) {
								case WISize.Tiny:
										maxItems = Globals.MaxTinyItemsPerStack;
										break;

								case WISize.Small:
										maxItems = Globals.MaxSmallItemsPerStack;
										break;

								case WISize.Medium:
										maxItems = Globals.MaxMediumItemsPerStack;
										break;

								case WISize.Large:
										maxItems = Globals.MaxLargeItemsPerStack;
										break;

								case WISize.Huge:
										maxItems = Globals.MaxHugeItemsPerStack;
										break;

								default:
										break;
						}

						return maxItems;
				}
		}
}

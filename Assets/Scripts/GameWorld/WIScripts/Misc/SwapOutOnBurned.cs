using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class SwapOutOnBurned : WIScript
		{
				public SwapOutOnBurnedState State = new SwapOutOnBurnedState();
				public Flammable flammable;

				public override void OnInitialized()
				{
						flammable = worlditem.Get <Flammable>();
						flammable.OnDepleted += OnDepleted;
				}

				public void OnDepleted()
				{
						GenericWorldItem swap = null;
						WICategory category = null;
						if (WorldItems.Get.Category(State.CategoryName, out category) && category.GetItem(worlditem.GetHashCode(), out swap)) {
								WorldItems.ReplaceWorldItem(worlditem, swap);
								Finish();
						}
				}
		}

		[Serializable]
		public class SwapOutOnBurnedState
		{
				public string CategoryName = string.Empty;
		}
}
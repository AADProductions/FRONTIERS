using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.World
{
		public class SwapOutOnDestroyed : WIScript
		{
				public SwapOutOnDestroyedState State = new SwapOutOnDestroyedState();
				public Damageable damageable;

				public override void OnInitialized()
				{
						damageable = worlditem.Get <Damageable>();
						damageable.OnDie += OnDie;
				}

				public void OnDie()
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
		public class SwapOutOnDestroyedState
		{
				public string CategoryName = string.Empty;
		}
}
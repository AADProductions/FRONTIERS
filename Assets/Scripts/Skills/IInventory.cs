using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
	public interface IInventory
	{
		string InventoryOwnerName { get; }
		IEnumerator GetInventoryContainer (int currentIndex, bool forward, GetInventoryContainerResult result);
		IEnumerator AddItems (WIStack stack, int numItems);
		IEnumerator AddItem (IWIBase item);
		bool HasItem (IWIBase item, out WIStack stack);
		bool HasBank { get; }
		Bank InventoryBank { get; }
		Action OnAccessInventory { get; set; }
	}

	public class GetInventoryContainerResult
	{
		public GetInventoryContainerResult ( )
		{
			ContainerIndex = -1;
			ContainerEnabler = null;
		}

		public GetInventoryContainerResult (WIStackEnabler containerEnabler, int containerIndex)
		{
			ContainerEnabler = containerEnabler;
			ContainerIndex = containerIndex;
		}

		public bool FoundContainer
		{
			get{
				return ContainerEnabler != null;
			}
		}

		public IBank InventoryBank;
		public WIStackEnabler ContainerEnabler;
		public int ContainerIndex;
		public int TotalContainers;
	}
}

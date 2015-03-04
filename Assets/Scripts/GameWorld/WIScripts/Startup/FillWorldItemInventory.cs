using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
	public class FillWorldItemInventory : WIScript
	{
		public ContainerFillMethod FillMethod = ContainerFillMethod.AllRandomItemsFromCategory;
		public ContainerFillInterval FillInterval = ContainerFillInterval.Daily;
		public ContainerFillTime FillTime = ContainerFillTime.OnOpen | ContainerFillTime.OnDie;

		public override void OnInitialized ()
		{
			Container container = null;
			if (!worlditem.Is <Container> (out container)) {
				Finish ();
			}

			container.OnOpenContainer += OnOpenContainer;
		}

		public void OnOpenContainer ( )
		{
			TryToFillInventory ();
		}

		public void TryToFillInventory () {

		}
	}
}
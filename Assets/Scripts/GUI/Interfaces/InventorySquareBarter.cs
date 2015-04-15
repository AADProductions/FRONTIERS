using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class InventorySquareBarter : InventorySquare
		{
				public BarterParty Party = BarterParty.Player;
				public BarterSession Session;

				public bool HasSession {
						get {
								return Session != null;
						}
				}

				public override void OnDrag()
				{	
						return;
				}

				public override void OnDrop()
				{
						return;
				}

				public virtual void SetSession(BarterSession newSession)
				{
						if (HasSession) {
								Session.RefreshAction -= mRefreshRequest;
						}
						Session = newSession;
						if (Session != null) {
								Session.RefreshAction += mRefreshRequest;
						}
				}

				public virtual void DropSession()
				{
						if (HasSession) {
								Session.RefreshAction -= mRefreshRequest;
								Session = null;
						}
				}

				public override void OnDestroy()
				{
						DropSession();
						base.OnDestroy();
				}
		}
}
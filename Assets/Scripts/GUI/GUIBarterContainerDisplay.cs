using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIBarterContainerDisplay : GUIStackContainerDisplay
		{
				public BarterContainerMode BarterMode = BarterContainerMode.Goods;
				public BarterParty Party = BarterParty.Player;
				public BarterSession Session;

				public override bool HasEnabler {
						get {
								bool result = Session != null && Session.IsActive;
								if (BarterMode == BarterContainerMode.Goods) {
										//we still care about enablers if we're goods
										return result & base.HasEnabler;
								}
								//we only care about session if we're offer
								return result;
						}
				}

				public override void CreateSquares()
				{	
						EnablerDisplayPrefab = GUIManager.Get.InventorySquareEnablerDisplay;

						if (BarterMode == BarterContainerMode.Goods) {
								//we're a mostly-normal stack container
								//the only twist is we'll use inventoy squares for goods
								UseVisualEnabler = true;
								SquarePrefab = GUIManager.Get.InventorySquareBarterGoods;
						} else {
								//we're a very strange stack container
								//we'll be using the Offer square
								UseVisualEnabler = false;
								SquarePrefab = GUIManager.Get.InventorySquareBarterOffer;
						}

						base.CreateSquares();
						//now we hook up the squares we've created to the session we're using
						for (int i = 0; i < InventorySquares.Count; i++) {
								InventorySquareBarter square = InventorySquares[i].GetComponent <InventorySquareBarter>();
								square.Party = Party;
								square.SetSession(Session);
						}
				}

				protected override void OnRefresh()
				{
						if (BarterMode == BarterContainerMode.Goods) {
								//if we're goods, then we have an enabler
								//and we're displaying its contents normally
								//so refresh normally
								base.OnRefresh();
						} else {
								CreateSquares();
								//can't do anything without a session
								if (Session == null || !Session.IsActive) {
										for (int i = 0; i < InventorySquares.Count; i++) {
												InventorySquares[i].DropStack();
										}
										return;
								}
								//if we're an offer container, then we're displaying something else
								//we're not using a stack to update squares, we're using goods
								List <BarterGoods> goods = new List <BarterGoods>();
								WIStackEnabler enabler = null;
								if (Party == BarterParty.Player) {
										goods = Session.PlayerGoods;
										enabler = Session.CurrentPlayerStackEnabler;
								} else {
										goods = Session.CharacterGoods;
										enabler = Session.CurrentCharacterStackEnabler;
								}
								for (int i = 0; i < InventorySquares.Count; i++) {
										InventorySquareBarterOffer square = InventorySquares[i].GetComponent <InventorySquareBarterOffer>();
										square.SetGoods(goods[i]);
								}
						}
				}
		}
}
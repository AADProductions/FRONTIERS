using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
		public class Purse : WIScript
		{
				public PurseState State = new PurseState();

				public static int CalculateLocalPrice (int basePrice, IWIBase item) {
						if (item == null)
								return basePrice;

						object purseStateObject = null;
						if (item.GetStateOf <Purse>(out purseStateObject)) {
								PurseState p = (PurseState)purseStateObject;
								if (p != null) {
										basePrice = basePrice + p.TotalValue;
								}
						}

						return basePrice;
				}

				public override void PopulateExamineList(System.Collections.Generic.List<WIExamineInfo> examine)
				{
						FillStackContainer fsc = null;
						if (worlditem.Has<FillStackContainer>(out fsc)) {
								if (!fsc.State.HasBeenFilled) {
										fsc.TryToFillContainer(true);
								}
						}
						WIExamineInfo e = new WIExamineInfo ("It's worth $" + State.TotalValue.ToString());
						examine.Add(e);
				}
		}

		[Serializable]
		public class PurseState
		{
				public int TotalValue {
						get {
								return Bronze
								+ (Silver * Globals.BaseValueSilver)
								+ (Gold * Globals.BaseValueGold)
								+ (Lumen * Globals.BaseValueLumen)
								+ (Warlock * Globals.BaseValueWarlock); 
						}
				}

				public int Bronze;
				public int Silver;
				public int Gold;
				public int Lumen;
				public int Warlock;
		}
}

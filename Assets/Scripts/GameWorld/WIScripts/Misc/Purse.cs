using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
		public class Purse : WIScript
		{
				public PurseState State = new PurseState();
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

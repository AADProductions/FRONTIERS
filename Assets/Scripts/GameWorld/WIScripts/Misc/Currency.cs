using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	[Serializable]
	public class Bank : IBank
	{
		public Bank ()
		{
			//TEMP
			//Randomize (100);
			//TEMP
		}

		[XmlIgnore]
		public Action RefreshAction { get; set; }

		[XmlIgnore]
		public Action OnMoneyAdded;
		[XmlIgnore]
		public Action OnMoneyRemoved;

		public void AddBaseCurrencyOfType (float numBaseCurrency, WICurrencyType type)
		{
			AddBaseCurrencyOfType (Mathf.FloorToInt (numBaseCurrency), type);
		}

		public void AddBaseCurrencyOfType (int numBaseCurrency, WICurrencyType type)
		{
			if (numBaseCurrency == 0 || type == WICurrencyType.None) {
				return;
			}
	
			int numActualCurrency = Currency.ConvertFromBaseCurrency (numBaseCurrency, type);
			Add (numActualCurrency, type);
		}

		public void MakeChange (int baseCurrencyValue, IBank bankToTransferTo)
		{
			if (baseCurrencyValue <= 0 || bankToTransferTo == this) {
				return;
			}

			int lumensTransferred = 0;
			int goldTransferred = 0;
			int warlockTransferred = 0;
			int silverTransferred = 0;
			int bronzeTransferred = 0;
			//probably a better way to do this but whatever
			while (mLumen > 0 && baseCurrencyValue >= (mLumen * Globals.BaseValueLumen)) {
				mLumen--;
				lumensTransferred++;
				baseCurrencyValue -= Globals.BaseValueLumen;
			}

			while (mGold > 0 && baseCurrencyValue >= (mGold * Globals.BaseValueGold)) {
				mGold--;
				goldTransferred++;
				baseCurrencyValue -= Globals.BaseValueGold;
			}

			while (mWarlock > 0 && baseCurrencyValue >= (mWarlock * Globals.BaseValueWarlock)) {
				mWarlock--;
				warlockTransferred++;
				baseCurrencyValue -= Globals.BaseValueWarlock;
			}

			while (mSilver > 0 && baseCurrencyValue >= (mSilver * Globals.BaseValueSilver)) {
				mSilver--;
				silverTransferred++;
				baseCurrencyValue -= Globals.BaseValueSilver;
			}

			//if we still have something left
			if (baseCurrencyValue > 0) {
				if (mBronze >= baseCurrencyValue) {
					//transfer everything
					mBronze -= baseCurrencyValue;
					bronzeTransferred = baseCurrencyValue;
					baseCurrencyValue = 0;
				} else {
					//transfer what we can
					bronzeTransferred = mBronze;
					baseCurrencyValue -= Bronze;
					mBronze = 0;
				}
			}

			bankToTransferTo.Add (bronzeTransferred, WICurrencyType.A_Bronze);
			bankToTransferTo.Add (silverTransferred, WICurrencyType.B_Silver);
			bankToTransferTo.Add (goldTransferred, WICurrencyType.C_Gold);
			bankToTransferTo.Add (lumensTransferred, WICurrencyType.D_Luminite);
			bankToTransferTo.Add (warlockTransferred, WICurrencyType.E_Warlock);

			RefreshAction.SafeInvoke ();
			OnMoneyAdded.SafeInvoke ();
		}

		public void Add (int numCurrency, WICurrencyType type)
		{
			if (numCurrency < 0) {
				//can't add negative currency
				return;
			}
			
			switch (type) {
			case WICurrencyType.A_Bronze:
			default:
				Bronze += numCurrency;
				break;
				
			case WICurrencyType.B_Silver:
				Silver += numCurrency;
				break;
				
			case WICurrencyType.C_Gold:
				Gold += numCurrency;
				break;
				
			case WICurrencyType.D_Luminite:
				Lumen += numCurrency;
				break;
				
			case WICurrencyType.E_Warlock:
				Warlock += numCurrency;
				break;
			}
			//Debug.Log ("Added " + numCurrency.ToString () + " currency of type " + type.ToString () + ", refresh action null? " + (RefreshAction == null).ToString ());
			RefreshAction.SafeInvoke ();
			OnMoneyAdded.SafeInvoke ();
		}

		public void Absorb (IBank otherBank)
		{
			Bronze += otherBank.Bronze;
			Silver += otherBank.Silver;
			Gold += otherBank.Gold;
			Lumen += otherBank.Lumen;
			Warlock += otherBank.Warlock;

			otherBank.Clear ();

			RefreshAction.SafeInvoke ();
			OnMoneyAdded.SafeInvoke ();
		}

		public bool CanAfford (int numBaseCurrency)
		{
			return BaseCurrencyValue >= numBaseCurrency;
		}

		public bool CanAfford (int numCurrency, WICurrencyType currencyType)
		{
			if (numCurrency <= 0) {
				return true;
			}
			return BaseCurrencyValue >= Currency.ConvertToBaseCurrency (numCurrency, currencyType);
		}

		public bool HasExactChange (int numCurrency, WICurrencyType type)
		{
			return false;
		}

		public static void FillWithRandomCurrency (IBank bank, int wealth)
		{
			int totalBaseCurrency = 0;
			//TODO make this random value based on global seed
			float randomValue = Mathf.Max (0.25f, UnityEngine.Random.value);

			switch (wealth) {
			case 0://poor
				totalBaseCurrency = Globals.WealthLevelPoorBaseCurrency;
				break;

			case 1://middle class
				totalBaseCurrency = Globals.WealthLevelMiddleClassBaseCurrency;
				break;

			case 2://wealthy
				totalBaseCurrency = Globals.WealthLevelWealthyBaseCurrency;
				break;

			case 3://aristocracy
				totalBaseCurrency = Globals.WealthLevelAristocracyBaseCurrency;
				break;
			}

			totalBaseCurrency = (int)(totalBaseCurrency * randomValue);

			//split into various currencies
			//TODO use mod operator, this is inefficient as hell
			while (totalBaseCurrency > Globals.BaseValueLumen) {
				bank.Lumen += Globals.BaseValueLumen;
				totalBaseCurrency -= Globals.BaseValueLumen;
			}

			while (totalBaseCurrency > Globals.BaseValueGold) {
				bank.Gold += Globals.BaseValueGold;
				totalBaseCurrency -= Globals.BaseValueGold;
			}

			while (totalBaseCurrency > Globals.BaseValueSilver) {
				bank.Silver += Globals.BaseValueSilver;
				totalBaseCurrency -= Globals.BaseValueSilver;
			}
			bank.Bronze += totalBaseCurrency;
			bank.RefreshAction.SafeInvoke ();
		}

		public bool TryToRemove (int numToRemove, WICurrencyType type)
		{
			return TryToRemove (numToRemove, type, true);
		}

		public bool TryToRemove (int numToRemove, WICurrencyType type, bool makeChange)
		{	
			if (makeChange) {
				return TryToRemove (Currency.ConvertToBaseCurrency (numToRemove, type));
			} else if (numToRemove == 0) {
				return true;
			} else {
				bool result = false;
				switch (type) {
				case WICurrencyType.A_Bronze:
				default:
					if (mBronze >= numToRemove) {
						mBronze -= numToRemove;
						result = true;
					}
					break;

				case WICurrencyType.B_Silver:
					if (mSilver >= numToRemove) {
						mSilver -= numToRemove;
						result = true;
					}
					break;

				case WICurrencyType.C_Gold:
					if (mGold >= numToRemove) {
						mGold -= numToRemove;
						result = true;
					}
					break;

				case WICurrencyType.D_Luminite:
					if (mLumen >= numToRemove) {
						mLumen -= numToRemove;
						result = true;
					}
					break;

				case WICurrencyType.E_Warlock:
					if (mWarlock >= numToRemove) {
						mWarlock -= numToRemove;
						result = true;
					}
					break;
				}
				if (result) {
					RefreshAction.SafeInvoke ();
					OnMoneyRemoved.SafeInvoke ();
					return true;
				}
			}
			return false;
		}

		public bool TryToRemove (int numBaseCurrency)
		{
			if (numBaseCurrency == 0) {
				return true;
			}
					
			if (numBaseCurrency > BaseCurrencyValue) {
				//there's no way we can make change for it
				return false;
			}

			//back up our current values so we can roll back if we don't have enough
			int bronze = mBronze;
			int silver = mSilver;
			int gold = mGold;
			int lumen = mLumen;
			int warlock = mWarlock;

			//keep converting stuff to base currency until we have enough to make change
			while (bronze < numBaseCurrency) {
				if (!ConvertNextAvailableToBaseCurrency (ref bronze, ref silver, ref gold, ref lumen, ref warlock)) {
					//whoops we don't have enough
					break;
				}
			}
			//if we're here and we have the bronze, we were successful
			if (bronze >= numBaseCurrency) {
				bronze -= numBaseCurrency;
				//copy all our temp values to our actual values
				mBronze = bronze;
				mSilver = silver;
				mGold = gold;
				mLumen = lumen;
				mWarlock = warlock;
				RefreshAction.SafeInvoke ();
				OnMoneyRemoved.SafeInvoke ();
				return true;
			} else {
				//we can't make change so don't copy anything
				//nothing has changed
				return false;
			}
		}

		protected bool ConvertNextAvailableToBaseCurrency (ref int baseCurrency, ref int silver, ref int gold, ref int lumen, ref int warlock)
		{
			if (silver > 0) {
				silver--;
				baseCurrency += Globals.BaseValueSilver;
				return true;
			} else if (gold > 0) {
				gold--;
				baseCurrency += Globals.BaseValueGold;
				return true;
			} else if (lumen > 0) {
				lumen--;
				baseCurrency += Globals.BaseValueLumen;
				return true;
			} else if (warlock > 0) {
				warlock--;
				baseCurrency += Globals.BaseValueWarlock;
				return true;
			}
			return false;
		}

		public int BaseCurrencyValue {
			get {
				//add up all our stuff as base currency
				return
					(Bronze * Globals.BaseValueBronze) +
				(Silver * Globals.BaseValueSilver) +
				(Gold * Globals.BaseValueGold) +
				(Warlock * Globals.BaseValueWarlock) +
				(Lumen * Globals.BaseValueLumen);
			}
		}

		public void Clear ()
		{
			Bronze = 0;
			Silver = 0;
			Gold = 0;
			Lumen = 0;
			Warlock = 0;		
			RefreshAction.SafeInvoke ();
		}

		public void Randomize (int baseCurrencyValue)
		{
			Bronze = UnityEngine.Random.Range (0, baseCurrencyValue);
			Silver = UnityEngine.Random.Range (0, baseCurrencyValue);
			Gold = UnityEngine.Random.Range (0, baseCurrencyValue);
			Lumen = UnityEngine.Random.Range (0, baseCurrencyValue);
			Warlock = UnityEngine.Random.Range (0, baseCurrencyValue);
		}

		public int Bronze { get { return mBronze; } set { mBronze = value; } }

		public int Silver { get { return mSilver; } set { mSilver = value; } }

		public int Gold { get { return mGold; } set { mGold = value; } }

		public int Lumen { get { return mLumen; } set { mLumen = value; } }

		public int Warlock { get { return mWarlock; } set { mWarlock = value; } }

		protected int mBronze = 0;
		protected int mSilver = 0;
		protected int mGold = 0;
		protected int mLumen = 0;
		protected int mWarlock = 0;
	}
	namespace WIScripts
	{
		public class Currency : WIScript
		{
			public static string TypeToString (WICurrencyType type)
			{
				switch (type) {
				default:
				case WICurrencyType.A_Bronze:
					return gBronzeGenericWorldItem.DisplayName;

				case WICurrencyType.B_Silver:
					return gSilverGenericWorldItem.DisplayName;

				case WICurrencyType.C_Gold:
					return gGoldIGenericWorldItem.DisplayName;

				case WICurrencyType.D_Luminite:
					return gLumenGenericWorldItem.DisplayName;

				case WICurrencyType.E_Warlock:
					return gWarlockGenericWorldItem.DisplayName;
				}
			}

			public static int ConvertToBaseCurrency (int numCurrency, WICurrencyType type)
			{
				int numBaseCurrency = 0;
				switch (type) {
				case WICurrencyType.A_Bronze:
					numBaseCurrency = numCurrency;
					break;

				case WICurrencyType.B_Silver:
					numBaseCurrency = numCurrency * Globals.BaseValueSilver;
					break;

				case WICurrencyType.C_Gold:
					numBaseCurrency = numCurrency * Globals.BaseValueGold;
					break;

				case WICurrencyType.D_Luminite:
					numBaseCurrency = numCurrency * Globals.BaseValueLumen;
					break;

				case WICurrencyType.E_Warlock:
					numBaseCurrency = numCurrency * Globals.BaseValueWarlock;
					break;

				default:
					break;
				}
				return numBaseCurrency;
			}

			public static int ConvertFromBaseCurrency (int numBaseCurrency, WICurrencyType type)
			{
				int numActualCurrency = 0;
				switch (type) {
				case WICurrencyType.A_Bronze:
					numActualCurrency = numBaseCurrency;
					break;

				case WICurrencyType.B_Silver:
					numActualCurrency = numBaseCurrency / Globals.BaseValueSilver;
					break;

				case WICurrencyType.C_Gold:
					numActualCurrency = numBaseCurrency / Globals.BaseValueGold;
					break;

				case WICurrencyType.D_Luminite:
					numActualCurrency = numBaseCurrency / Globals.BaseValueLumen;
					break;

				case WICurrencyType.E_Warlock:
					numActualCurrency = numBaseCurrency / Globals.BaseValueWarlock;
					break;

				default:
					break;
				}
				return numActualCurrency;
			}
			//IDs the object as currency for when it enters inventory
			//also provides a bunch of useful static functions to manipulate currency in one place
			public WICurrencyType Type = WICurrencyType.A_Bronze;
			public int NumCurrency = 1;
			//TODO potentially move default values into globals (?)
			public static string CurrencyPackName = "PreciousMetalsAndCurrency";
			public static string BronzePrefabName = "Grain Coin";
			public static string SilverPrefabName = "Quarter Coin";
			public static string GoldPrefabName = "Dram Coin";
			public static string LumenPrefabName = "Marble 1";
			public static string WarlockPrefabName = "Warlock Coin";
			public static string BronzeCurrencyName = "Grain";
			public static string SilverCurrencyName = "Quarter";
			public static string GoldCurrencyName = "Dram";
			public static string LumenCurrencyName = "Mark";
			public static string WarlockCurrencyName = "Kanitt";
			public static string BronzeCurrencyNamePlural = "Grain";
			public static string SilverCurrencyNamePlural = "Quarter";
			public static string GoldCurrencyNamePlural = "Dram";
			public static string LumenCurrencyNamePlural = "Mark";
			public static string WarlockCurrencyNamePlural = "Kanitt";

			public static GenericWorldItem BronzeGenericWorldItem {
				get {
					if (gBronzeGenericWorldItem == null) {
						gBronzeGenericWorldItem = new GenericWorldItem ();
						gBronzeGenericWorldItem.PackName = CurrencyPackName;
						gBronzeGenericWorldItem.PrefabName = BronzePrefabName;
					}
					return gBronzeGenericWorldItem;
				}
			}

			public static GenericWorldItem SilverGenericWorldItem {
				get {
					if (gSilverGenericWorldItem == null) {
						gSilverGenericWorldItem = new GenericWorldItem ();
						gSilverGenericWorldItem.PackName = CurrencyPackName;
						gSilverGenericWorldItem.PrefabName = SilverPrefabName;
					}
					return gSilverGenericWorldItem;
				}
			}

			public static GenericWorldItem GoldIGenericWorldItem {
				get {
					if (gGoldIGenericWorldItem == null) {
						gGoldIGenericWorldItem = new GenericWorldItem ();
						gGoldIGenericWorldItem.PackName = CurrencyPackName;
						gGoldIGenericWorldItem.PrefabName = GoldPrefabName;
					}
					return gGoldIGenericWorldItem;
				}
			}

			public static GenericWorldItem LumenGenericWorldItem {
				get {
					if (gLumenGenericWorldItem == null) {
						gLumenGenericWorldItem = new GenericWorldItem ();
						gLumenGenericWorldItem.PackName = CurrencyPackName;
						gLumenGenericWorldItem.PrefabName = LumenPrefabName;
					}
					return gLumenGenericWorldItem;
				}
			}

			public static GenericWorldItem WarlockGenericWorldItem {
				get {
					if (gWarlockGenericWorldItem == null) {
						gWarlockGenericWorldItem = new GenericWorldItem ();
						gWarlockGenericWorldItem.PackName = CurrencyPackName;
						gWarlockGenericWorldItem.PrefabName = WarlockPrefabName;
					}
					return gWarlockGenericWorldItem;
				}
			}

			protected static GenericWorldItem gBronzeGenericWorldItem;
			protected static GenericWorldItem gSilverGenericWorldItem;
			protected static GenericWorldItem gGoldIGenericWorldItem;
			protected static GenericWorldItem gLumenGenericWorldItem;
			protected static GenericWorldItem gWarlockGenericWorldItem;
		}
	}
}
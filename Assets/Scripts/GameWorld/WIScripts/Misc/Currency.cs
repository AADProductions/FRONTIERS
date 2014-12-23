using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Frontiers.World
{
		public interface IBank
		{
				Action RefreshAction { get; set; }

				void AddBaseCurrencyOfType(float numBaseCurrency, WICurrencyType type);

				void AddBaseCurrencyOfType(int numBaseCurrency, WICurrencyType type);

				void Add(int numCurrency, WICurrencyType type);

				bool TryToRemove(int numToRemove, ref int numRemoved, WICurrencyType type);

				void Absorb(IBank otherBank);

				void Clear();

				int BaseCurrencyValue { get; }

				int Bronze { get; set; }

				int Silver { get; set; }

				int Gold { get; set; }

				int Lumen { get; set; }

				int Warlock { get; set; }
		}

		[Serializable]
		public class Bank : IBank
		{
				public Bank()
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

				public void AddBaseCurrencyOfType(float numBaseCurrency, WICurrencyType type)
				{
						AddBaseCurrencyOfType(Mathf.FloorToInt(numBaseCurrency), type);
				}

				public void AddBaseCurrencyOfType(int numBaseCurrency, WICurrencyType type)
				{
						if (numBaseCurrency == 0 || type == WICurrencyType.None) {
								return;
						}
	
						int numActualCurrency = Currency.ConvertFromBaseCurrency(numBaseCurrency, type);
						Add(numActualCurrency, type);
				}

				public void Add(int numCurrency, WICurrencyType type)
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
						RefreshAction.SafeInvoke();
						OnMoneyAdded.SafeInvoke();
				}

				public void Absorb(IBank otherBank)
				{
						Bronze += otherBank.Bronze;
						Silver += otherBank.Silver;
						Gold += otherBank.Gold;
						Lumen += otherBank.Lumen;
						Warlock += otherBank.Warlock;

						otherBank.Clear();

						RefreshAction.SafeInvoke();
						OnMoneyAdded.SafeInvoke();
				}

				public bool CanAfford(int numBaseCurrency)
				{
						return BaseCurrencyValue >= numBaseCurrency;
				}

				public bool CanAfford(int numCurrency, WICurrencyType currencyType)
				{
						if (numCurrency <= 0) {
								return true;
						}
						return BaseCurrencyValue >= Currency.ConvertToBaseCurrency(numCurrency, currencyType);
				}

				public bool HasExactChange(int numCurrency, WICurrencyType type)
				{
						return false;
				}

				public static void FillWithRandomCurrency(IBank bank, int wealth)
				{
						int totalBaseCurrency = 0;
						//TODO make this random value based on global seed
						float randomValue = Mathf.Max(0.25f, UnityEngine.Random.value);

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
						bank.RefreshAction.SafeInvoke();
				}

				public bool TryToRemove(int numToRemove, ref int numRemoved, WICurrencyType type)
				{
						if (numToRemove == 0) {
								numRemoved = 0;
								return true;
						}

						bool result = false;
						switch (type) {
								case WICurrencyType.A_Bronze:
										result = TryToRemove(numToRemove, ref mBronze, ref numRemoved);
										break;
				
								case WICurrencyType.B_Silver:
										result = TryToRemove(numToRemove, ref mSilver, ref numRemoved);
										break;
				
								case WICurrencyType.C_Gold:
										result = TryToRemove(numToRemove, ref mGold, ref numRemoved);
										break;
				
								case WICurrencyType.D_Luminite:
										result = TryToRemove(numToRemove, ref mLumen, ref numRemoved);
										break;
				
								case WICurrencyType.E_Warlock:
										result = TryToRemove(numToRemove, ref mWarlock, ref numRemoved);
										break;
				
								default:
										break;
						}
						RefreshAction.SafeInvoke();
						return result;
				}

				protected bool TryToRemove(int numToRemove, ref int numCurrency, ref int numRemoved)
				{
						if (numToRemove == 0 || numCurrency == 0) {
								numRemoved = 0;
								return false;
						}
			
						if (numCurrency >= numToRemove) {
								numRemoved = numToRemove;
								numCurrency = numCurrency - numToRemove;
								RefreshAction.SafeInvoke();
								OnMoneyRemoved.SafeInvoke();
								return true;
						} else {//if currency is less
								numRemoved = numCurrency;
								numCurrency = 0;
								return false;
						}
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

				public void Clear()
				{
						Bronze = 0;
						Silver = 0;
						Gold = 0;
						Lumen = 0;
						Warlock = 0;		
						RefreshAction.SafeInvoke();
				}

				public void Randomize(int baseCurrencyValue)
				{
						Bronze = UnityEngine.Random.Range(0, baseCurrencyValue);
						Silver = UnityEngine.Random.Range(0, baseCurrencyValue);
						Gold = UnityEngine.Random.Range(0, baseCurrencyValue);
						Lumen = UnityEngine.Random.Range(0, baseCurrencyValue);
						Warlock = UnityEngine.Random.Range(0, baseCurrencyValue);
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

		public class Currency : WIScript
		{		
				public static string TypeToString(WICurrencyType type)
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

				public static int ConvertToBaseCurrency(int numCurrency, WICurrencyType type)
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

				public static int ConvertFromBaseCurrency(int numBaseCurrency, WICurrencyType type)
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
										gBronzeGenericWorldItem = new GenericWorldItem();
										gBronzeGenericWorldItem.PackName = CurrencyPackName;
										gBronzeGenericWorldItem.PrefabName = BronzePrefabName;
								}
								return gBronzeGenericWorldItem;
						}
				}

				public static GenericWorldItem SilverGenericWorldItem {
						get {
								if (gSilverGenericWorldItem == null) {
										gSilverGenericWorldItem = new GenericWorldItem();
										gSilverGenericWorldItem.PackName = CurrencyPackName;
										gSilverGenericWorldItem.PrefabName = SilverPrefabName;
								}
								return gSilverGenericWorldItem;
						}
				}

				public static GenericWorldItem GoldIGenericWorldItem {
						get {
								if (gGoldIGenericWorldItem == null) {
										gGoldIGenericWorldItem = new GenericWorldItem();
										gGoldIGenericWorldItem.PackName = CurrencyPackName;
										gGoldIGenericWorldItem.PrefabName = GoldPrefabName;
								}
								return gGoldIGenericWorldItem;
						}
				}

				public static GenericWorldItem LumenGenericWorldItem {
						get {
								if (gLumenGenericWorldItem == null) {
										gLumenGenericWorldItem = new GenericWorldItem();
										gLumenGenericWorldItem.PackName = CurrencyPackName;
										gLumenGenericWorldItem.PrefabName = LumenPrefabName;
								}
								return gLumenGenericWorldItem;
						}
				}

				public static GenericWorldItem WarlockGenericWorldItem {
						get {
								if (gWarlockGenericWorldItem == null) {
										gWarlockGenericWorldItem = new GenericWorldItem();
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

		public enum WICurrencyType
		{
				None,
				A_Bronze,
				B_Silver,
				C_Gold,
				D_Luminite,
				E_Warlock,
		}
}
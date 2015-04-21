using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Frontiers
{
		[Serializable]
		public class FlagSet : Mod, IComparable <FlagSet>
		{
				public List <string> Flags = new List <string>();
				public List <FlagCombo> FlagCombos = new List <FlagCombo>();

				public string [] GetItemNames()
				{	//convenience for property drawers
						//will add some checks in later
						return Flags.ToArray();
				}

				public int [] GetItemValues()
				{	//convenience for property drawers
						//will add some checks in later
						return Values;
				}

				public int CompareTo(FlagSet other)
				{
						return Name.CompareTo(other.Name);
				}

				public int [] Values {
						get {
								int[] values = new int [Flags.Count];
								for (int i = 0; i < Flags.Count; i++) {
										values[i] = 1 << i;
								}
								return values;
						}
				}

				public void Refresh()
				{
						mLookup.Clear();
						int flagValue = 1;
						mLookup.Add("None", 0);
						mReverseLookup.Add(0, "None");

						for (int i = 0; i < Flags.Count; i++) {
								if (!string.IsNullOrEmpty(Flags[i])) {
										if (mLookup.ContainsKey(Flags[i])) {
												mLookup[Flags[i]] = flagValue;
										} else {
												mLookup.Add(Flags[i], flagValue);
										}
										mReverseLookup.Add(flagValue, Flags[i]);
								}
								flagValue = flagValue * 2;
						}
				}

				public int GetFlagValue(List <string> flagNames)
				{
						int flagValue = 0;
						foreach (string flagName in flagNames) {
								flagValue |= GetFlagValue(flagName);
						}
						return flagValue;
				}

				public int GetFlagValue(string flagName)
				{
						int flagValue = 0;
						mLookup.TryGetValue(flagName, out flagValue);
						return flagValue;
				}

				public int GetFlagIndex(string flagName)
				{
						for (int i = 0; i < Flags.Count; i++) {
								if (flagName == Flags[i]) {
										return i;
								}
						}
						return -1;
				}

				public int GetFlagIndex(int flagValue)
				{
						//fix this
						for (int i = 0; i < 32; i++) {
								if (1 << i == flagValue) {
										return i;
								}
						}
						return -1;
				}

				public static int GetMaxValue(int flags)
				{
						int value = 0;
						for (int i = 0; i < 32; i++) {
								if (((1 << i) & flags) > 0) {
										value = i;
								}
						}
						return value;
				}

				public static int GetAverageValue(int flags)
				{
						int value = 0;
						int numValues = 0;
						for (int i = 0; i < 32; i++) {
								if (((1 << i) & flags) > 0) {
										value = i;
										numValues++;
								}
						}
						if (numValues > 1) {
								value /= numValues;
						}
						return value;
				}

				public int GetFlagValue(int index)
				{
						return 1 << index;
				}

				public string GetFlagName(int flagValue)
				{
						string flagName = string.Empty;
						if (!mReverseLookup.ContainsKey(flagValue)) {
								//Debug.Log("No flag name for value " + flagValue.ToString());
						}
						mReverseLookup.TryGetValue(flagValue, out flagName);
						return flagName;
				}

				protected Dictionary <int, string> mReverseLookup = new Dictionary <int, string>();
				protected Dictionary <string, int> mLookup = new Dictionary <string, int>();

				public static bool IsSame(int check, int againstThese)
				{
						return check == againstThese;
				}

				public static bool HasAll(int checkHas, int allOfThese)
				{
						return (checkHas & allOfThese) != 0;
				}

				public static bool HasAny(int checkHas, int anyOfThese)
				{
						return (checkHas & anyOfThese) == checkHas;
				}

				public static bool HasNone(int checkHas, int noneOfThese)
				{
						return !HasAll(checkHas, noneOfThese);
				}

				public static int [] GetFlagValues(int flags)
				{
						int numValues = 0;
						for (int i = 0; i < 32; i++) {
								if (((1 << i) & flags) > 0) {
										gflagValues[numValues] = i;
										numValues++;
								}
						}
						int[] finalFlagValues = new int [numValues];
						for (int i = 0; i < numValues; i++) {
								finalFlagValues[i] = gflagValues[i];
						}
						return finalFlagValues;
				}

				public static int GetFlagBitValue(int flags, int tieBreaker, int defaultValue)
				{
						return (flags > 0) ? 1 << GetFlagValue(flags, tieBreaker, defaultValue) : defaultValue;
				}

				public static int[] gflagValues = new int[32];

				public static int GetFlagValue(int flags, int tieBreaker, int defaultValue)
				{
						int flagValue = defaultValue;
						int numValues = 0;
						for (int i = 0; i < 32; i++) {
								if (((1 << i) & flags) > 0) {
										gflagValues[numValues] = i;
										numValues++;
								}
						}
						if (numValues > 0) {
								if (numValues > 1) {
										//if we have more than one index
										//use the hash value of the tiebreaker
										//to choose a final value
										flagValue = gflagValues[Math.Abs(tieBreaker) % gflagValues.Length];
								} else {
										flagValue = gflagValues[0];
								}
						}
						return flagValue;
				}

				public static int GetFlagValue(int flags, string tieBreaker, int defaultValue)
				{
						int flagValue = defaultValue;
						int numValues = 0;
						for (int i = 0; i < 32; i++) {
								if (((1 << i) & flags) > 0) {
										gflagValues[numValues] = i;
										numValues++;
								}
						}
						if (numValues > 0) {
								if (numValues > 1) {
										//if we have more than one index
										//use the hash value of the tiebreaker
										//to choose a final value
										flagValue = gflagValues[Math.Abs(tieBreaker.GetHashCode()) % numValues];
								} else {
										flagValue = gflagValues[0];
								}
						}
						return flagValue;
				}

				[Serializable]
				public sealed class FlagCombo
				{
						public string Name = "Combo";
						public List <int> Values = new List <int>();

						public int Combo {
								get {
										int c = 0;
										for (int i = 0; i < Values.Count; i++) {
												c |= 1 << Values[i];
										}
										return c;
								}
						}
				}
		}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
		public static class Flags
		{
				public enum CheckType
				{
						MatchAll,
						MatchAny,
						MatchExact
				}

				public static bool Has(uint objectFlagsEnum, uint checkFlagsEnum)
				{
						return Check(objectFlagsEnum, checkFlagsEnum, CheckType.MatchAny);
				}

				public static uint objectFlags;
				public static uint checkFlags;
				//this function was once a thing of beauty
				//it was generic, eg Check <TimeOfDay> (TimeOfDay objectFlagsEnum, TimeOfDay checkFlagsEnum, CheckType checkType)
				//but alas C# doesn't allow me to constrain generic types where T : enum and this required massive garbage allocation with System.Convert
				//so I was forced to downgrade it to this abomination, where every call now requires an explicit cast to uint. ugh.
				//please C#, allow where T : enum, let me have my beautiful function back...
				public static bool Check(uint objectFlagsEnum, uint checkFlagsEnum, CheckType checkType)
				{
						objectFlags = objectFlagsEnum;
						checkFlags = checkFlagsEnum;

						if (objectFlags == 0 || checkFlags == 0) {
								//we only want to check enum flags that start with 1
								//if one of these flags is set to 0, then something is fucked
								return false;
						}

						bool check = false;

						switch (checkType) {
								case CheckType.MatchExact:
										check = (objectFlags == checkFlags);
										break;

								case CheckType.MatchAny:
										check = ((objectFlags & checkFlags) != 0);
										break;

								case CheckType.MatchAll:
								default:
										check = ((objectFlags & checkFlags) == objectFlags);
										break;
						}
						return check;
				}

				public static bool Check(uint objectFlagsEnum, int checkFlagsEnum, CheckType checkType)
				{
						objectFlags = objectFlagsEnum;
						checkFlags = (uint)checkFlagsEnum;

						if (objectFlags == 0 || checkFlags == 0) {
								//we only want to check enum flags that start with 1
								//if one of these flags is set to 0, then something is fucked
								return false;
						}

						bool check = false;

						switch (checkType) {
								case CheckType.MatchExact:
										check = (objectFlags == checkFlags);
										break;

								case CheckType.MatchAny:
										check = ((objectFlags & checkFlags) != 0);
										break;

								case CheckType.MatchAll:
								default:
										check = ((objectFlags & checkFlags) == objectFlags);
										break;
						}
						return check;
				}

				public static bool Check(int objectFlagsEnum, int checkFlagsEnum, CheckType checkType)
				{
						objectFlags = (uint)objectFlagsEnum;
						checkFlags = (uint)checkFlagsEnum;

						if (objectFlags == 0 || checkFlags == 0) {
								//we only want to check enum flags that start with 1
								//if one of these flags is set to 0, then something is fucked
								return false;
						}

						bool check = false;

						switch (checkType) {
								case CheckType.MatchExact:
										check = (objectFlags == checkFlags);
										break;

								case CheckType.MatchAny:
										check = ((objectFlags & checkFlags) != 0);
										break;

								case CheckType.MatchAll:
								default:
										check = ((objectFlags & checkFlags) == objectFlags);
										break;
						}
						return check;
				}
				//		public static bool Check <T> (T objectFlagsEnum, int checkFlagsEnum, CheckType checkType)
				//		{
				//			objectFlags = (uint)(checkFlagsEnum);//System.Convert.ToUInt32 (objectFlagsEnum);
				//			checkFlags = (uint)checkFlagsEnum;
				//
				//			if (objectFlags == 0 || checkFlags == 0) {
				//				//we only want to check enum flags that start with 1
				//				//if one of these flags is set to 0, then something is fucked
				//				return false;
				//			}
				//
				//			bool check = false;
				//
				//			switch (checkType) {
				//			case CheckType.MatchExact:
				//				check = (objectFlags == checkFlags);
				//				break;
				//
				//			case CheckType.MatchAny:
				//				check = ((objectFlags & checkFlags) != 0);
				//				break;
				//
				//			case CheckType.MatchAll:
				//			default:
				//				check = ((objectFlags & checkFlags) == objectFlags);
				//				break;
				//			}
				//			return check;
				//		}
				//
				//		public static bool Check <T> (int objectFlagsEnum, T checkFlagsEnum, CheckType checkType)
				//		{
				//			objectFlags = (uint)objectFlagsEnum;
				//			checkFlags = (uint)(checkFlagsEnum);//System.Convert.ToUInt32 (checkFlagsEnum);
				//
				//			if (objectFlags == 0 || checkFlags == 0) {
				//				//we only want to check enum flags that start with 1
				//				//if one of these flags is set to 0, then something is fucked
				//				return false;
				//			}
				//
				//			bool check = false;
				//
				//			switch (checkType) {
				//			case CheckType.MatchExact:
				//				check = (objectFlags == checkFlags);
				//				break;
				//
				//			case CheckType.MatchAny:
				//				check = ((objectFlags & checkFlags) != 0);
				//				break;
				//
				//			case CheckType.MatchAll:
				//			default:
				//				check = ((objectFlags & checkFlags) == objectFlags);
				//				break;
				//			}
				//			return check;
				//		}
				//
				//		public static bool Check <T> (T objectFlagsEnum, T checkFlagsEnum, CheckType checkType)
				//		{
				//			objectFlags = (uint)(checkFlagsEnum);//System.Convert.ToUInt32 (objectFlagsEnum);
				//			checkFlags = (uint)(checkFlagsEnum);//System.Convert.ToUInt32 (checkFlagsEnum);
				//
				//			if (objectFlags == 0 || checkFlags == 0) {
				//				//we only want to check enum flags that start with 1
				//				//if one of these flags is set to 0, then something is fucked
				//				return false;
				//			}
				//
				//			bool check = false;
				//
				//			switch (checkType) {
				//			case CheckType.MatchExact:
				//				check = (objectFlags == checkFlags);
				//				break;
				//
				//			case CheckType.MatchAny:
				//				check = ((objectFlags & checkFlags) != 0);
				//				break;
				//
				//			case CheckType.MatchAll:
				//			default:
				//				check = ((objectFlags & checkFlags) == objectFlags);
				//				break;
				//			}
				//			return check;
				//		}
		}
}
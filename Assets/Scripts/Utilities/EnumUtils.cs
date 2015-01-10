/*
C# Generic Enum Parser With Extension Methods
Code Snippet By: Pinal Bhatt [www.PBDesk.com]
*/
using System;

public static class EnumUtils
{


	#region String to Enum
	public static T ParseEnum<T>(string inString, bool ignoreCase = true, bool throwException = true) where T : struct
	{
		return (T)ParseEnum<T>(inString, default(T), ignoreCase, throwException);
	}

	public static T ParseEnum<T>(string inString, T defaultValue,
		bool ignoreCase = true, bool throwException = false) where T : struct
	{
		T returnEnum = defaultValue;

		if (!typeof(T).IsEnum ||  String.IsNullOrEmpty(inString))
		{
			throw new InvalidOperationException("Invalid Enum Type or Input String 'inString'. " + typeof(T).ToString() + "  must be an Enum");
		}

		try
		{
			//we're not using .net 4 so we don't have TryParse
			//TODO implement our own TryParse
			//or get rid of this function entirely, since we only use Int to Enum
			bool success = false;//Enum.TryParse<T>(inString, ignoreCase, out returnEnum);
			if (!success && throwException)
			{
				throw new InvalidOperationException("Invalid Cast");
			}
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("Invalid Cast", ex);
		}

		return returnEnum;
	}
	#endregion

	#region Int to Enum
	public static T ParseEnum<T>(int input, bool throwException = true) where T : struct
	{
		return (T)ParseEnum<T>(input, default(T), throwException);
	}
	public static T ParseEnum<T>(int input, T defaultValue, bool throwException = false) where T : struct
	{
		T returnEnum = defaultValue;
		if (!typeof(T).IsEnum)
		{
			throw new InvalidOperationException("Invalid Enum Type. " + typeof(T).ToString() + "  must be an Enum");
		}
		if (Enum.IsDefined(typeof(T), input))
		{
			returnEnum = (T)Enum.ToObject(typeof(T), input);
		}
		else
		{
			if (throwException)
			{
				throw new InvalidOperationException("Invalid Cast");
			}
		}

		return returnEnum;

	}
	#endregion

	#region String Extension Methods for Enum Parsing
	public static T ToEnum<T>(this string inString, bool ignoreCase = true, bool throwException = true) where T : struct
	{
		return (T)ParseEnum<T>(inString, ignoreCase, throwException);
	}
	public static T ToEnum<T>(this string inString, T defaultValue, bool ignoreCase = true, bool throwException = false) where T : struct
	{
		return (T)ParseEnum<T>(inString, defaultValue, ignoreCase, throwException);
	}
	#endregion

	#region Int Extension Methods for Enum Parsing
	public static T ToEnum<T>(this int input, bool throwException = true) where T : struct
	{
		return (T)ParseEnum<T>(input, default(T), throwException);
	}

	public static T ToEnum<T>(this int input, T defaultValue, bool throwException = false) where T : struct
	{
		return (T)ParseEnum<T>(input, defaultValue, throwException);
	}
	#endregion
}
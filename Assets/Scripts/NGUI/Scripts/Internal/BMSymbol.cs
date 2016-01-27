//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Symbols are a sequence of characters such as ":)" that get replaced with a single glyph, such as the smiley face.
/// </summary>

[System.Serializable]
public class BMSymbol
{
	public string sequence;
	public int x;
	public int y;
	public int width;
	public int height;

	int mLength = 0;

	public int length { get { if (mLength == 0) mLength = sequence.Length; return mLength; } }
}
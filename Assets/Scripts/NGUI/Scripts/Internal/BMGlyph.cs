//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Glyph structure used by BMFont. For more information see http://www.angelcode.com/products/bmfont/
/// </summary>

[System.Serializable]
public class BMGlyph
{
	public struct Kerning
	{
		public int previousChar;
		public int amount;
	}

	public int index;	// Index of this glyph (used by BMFont)
	public int x;		// Offset from the left side of the texture to the left side of the glyph
	public int y;		// Offset from the top of the texture to the top of the glyph
	public int width;	// Glyph's width in pixels
	public int height;	// Glyph's height in pixels
	public int offsetX;	// Offset to apply to the cursor's left position before drawing this glyph
	public int offsetY; // Offset to apply to the cursor's top position before drawing this glyph
	public int advance;	// How much to move the cursor after printing this character

	public List<Kerning> kerning;

	/// <summary>
	/// Retrieves the special amount by which to adjust the cursor position, given the specified previous character.
	/// </summary>

	public int GetKerning (int previousChar)
	{
		if (kerning != null)
		{
			for (int i = 0, imax = kerning.Count; i < imax; ++i)
			{
				Kerning k = kerning[i];

				if (k.previousChar == previousChar)
				{
					return k.amount;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Add a new kerning entry to the character (or adjust an existing one).
	/// </summary>

	public void SetKerning (int previousChar, int amount)
	{
		if (kerning == null) kerning = new List<Kerning>();

		for (int i = 0; i < kerning.Count; ++i)
		{
			if (kerning[i].previousChar == previousChar)
			{
				Kerning k = kerning[i];
				k.amount = amount;
				kerning[i] = k;
				return;
			}
		}

		Kerning ker = new Kerning();
		ker.previousChar = previousChar;
		ker.amount = amount;
		kerning.Add(ker);
	}

	/// <summary>
	/// Trim the glyph, given the specified minimum and maximum dimensions in pixels.
	/// </summary>

	public void Trim (int xMin, int yMin, int xMax, int yMax)
	{
		int x1 = x + width;
		int y1 = y + height;

		if (x < xMin)
		{
			int offset = xMin - x;
			x += offset;
			width -= offset;
			offsetX += offset;
		}

		if (y < yMin)
		{
			int offset = yMin - y;
			y += offset;
			height -= offset;
			offsetY += offset;
		}

		if (x1 > xMax) width  -= x1 - xMax;
		if (y1 > yMax) height -= y1 - yMax;
	}
}
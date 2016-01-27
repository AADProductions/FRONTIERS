//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// UIFont contains everything needed to be able to print text.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
	public enum Alignment
	{
		Left,
		Center,
		Right,
	}

	[HideInInspector][SerializeField] Material mMat;
	[HideInInspector][SerializeField] Rect mUVRect = new Rect(0f, 0f, 1f, 1f);
	[HideInInspector][SerializeField] BMFont mFont = new BMFont();
	[HideInInspector][SerializeField] int mSpacingX = 0;
	[HideInInspector][SerializeField] int mSpacingY = 0;
	[HideInInspector][SerializeField] UIAtlas mAtlas;
	[HideInInspector][SerializeField] UIFont mReplacement;

	// Cached value
	UIAtlas.Sprite mSprite = null;

	// BUG: There is a bug in Unity 3.4.2 and all the way up to 3.5 b7 -- when instantiating from prefabs,
	// for some strange reason classes get initialized with default values. So for example, 'mSprite' above
	// gets initialized as if it was created with 'new UIAtlas.Sprite()' instead of 'null'. Fun, huh?

	bool mSpriteSet = false;

	// I'd use a Stack here, but then Flash export wouldn't work as it doesn't support it
	List<Color> mColors = new List<Color>();

	/// <summary>
	/// Access to the BMFont class directly.
	/// </summary>

	public BMFont bmFont { get { return (mReplacement != null) ? mReplacement.bmFont : mFont; } }

	/// <summary>
	/// Original width of the font's texture in pixels.
	/// </summary>

	public int texWidth { get { return (mReplacement != null) ? mReplacement.texWidth : ((mFont != null) ? mFont.texWidth : 1); } }

	/// <summary>
	/// Original height of the font's texture in pixels.
	/// </summary>

	public int texHeight { get { return (mReplacement != null) ? mReplacement.texHeight : ((mFont != null) ? mFont.texHeight : 1); } }

	/// <summary>
	/// Atlas used by the font, if any.
	/// </summary>

	public UIAtlas atlas
	{
		get
		{
			return (mReplacement != null) ? mReplacement.atlas : mAtlas;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.atlas = value;
			}
			else if (mAtlas != value)
			{
				if (value == null)
				{
					if (mAtlas != null) mMat = mAtlas.spriteMaterial;
					if (sprite != null) mUVRect = uvRect;
				}

				mAtlas = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Get or set the material used by this font.
	/// </summary>

	public Material material
	{
		get
		{
			if (mReplacement != null) return mReplacement.material;
			return (mAtlas != null) ? mAtlas.spriteMaterial : mMat;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.material = value;
			}
			else if (mAtlas == null && mMat != value)
			{
				mMat = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Convenience function that returns the texture used by the font.
	/// </summary>

	public Texture2D texture
	{
		get
		{
			if (mReplacement != null) return mReplacement.texture;
			Material mat = material;
			return (mat != null) ? mat.mainTexture as Texture2D : null;
		}
	}

	/// <summary>
	/// Offset and scale applied to all UV coordinates.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			if (mReplacement != null) return mReplacement.uvRect;

			if (mAtlas != null && (mSprite == null && sprite != null))
			{
				Texture tex = mAtlas.texture;

				if (tex != null)
				{
					mUVRect = mSprite.outer;

					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						mUVRect = NGUIMath.ConvertToTexCoords(mUVRect, tex.width, tex.height);
					}

					// Account for trimmed sprites
					if (mSprite.hasPadding)
					{
						Rect rect = mUVRect;
						mUVRect.xMin = rect.xMin - mSprite.paddingLeft * rect.width;
						mUVRect.yMin = rect.yMin - mSprite.paddingBottom * rect.height;
						mUVRect.xMax = rect.xMax + mSprite.paddingRight * rect.width;
						mUVRect.yMax = rect.yMax + mSprite.paddingTop * rect.height;
					}
#if UNITY_EDITOR
					// The font should always use the original texture size
					if (mFont != null)
					{
						float tw = (float)mFont.texWidth / tex.width;
						float th = (float)mFont.texHeight / tex.height;

						if (tw != mUVRect.width || th != mUVRect.height)
						{
							//Debug.LogWarning("Font sprite size doesn't match the expected font texture size.\n" +
							//	"Did you use the 'inner padding' setting on the Texture Packer? It must remain at '0'.", this);
							mUVRect.width = tw;
							mUVRect.height = th;
						}
					}
#endif
					// Trimmed sprite? Trim the glyphs
					if (mSprite.hasPadding) Trim();
				}
			}
			return mUVRect;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.uvRect = value;
			}
			else if (sprite == null && mUVRect != value)
			{
				mUVRect = value;
				MarkAsDirty();
			}
		}
	}

	public float defaultSize = 1f;
	public float sizeRelativeToPrimaryFont = 1f;

	/// <summary>
	/// Sprite used by the font, if any.
	/// </summary>

	public string spriteName
	{
		get
		{
			return (mReplacement != null) ? mReplacement.spriteName : mFont.spriteName;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.spriteName = value;
			}
			else if (mFont.spriteName != value)
			{
				mFont.spriteName = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Horizontal spacing applies to characters. If positive, it will add extra spacing between characters. If negative, it will make them be closer together.
	/// </summary>

	public int horizontalSpacing
	{
		get
		{
			return (mReplacement != null) ? mReplacement.horizontalSpacing : mSpacingX;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.horizontalSpacing = value;
			}
			else if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Vertical spacing applies to lines. If positive, it will add extra spacing between lines. If negative, it will make them be closer together.
	/// </summary>

	public int verticalSpacing
	{
		get
		{
			return (mReplacement != null) ? mReplacement.verticalSpacing : mSpacingY;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.verticalSpacing = value;
			}
			else if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Pixel-perfect size of this font.
	/// </summary>

	public int size { get { return (mReplacement != null) ? mReplacement.size : mFont.charSize; } }

	/// <summary>
	/// Retrieves the sprite used by the font, if any.
	/// </summary>

	public UIAtlas.Sprite sprite
	{
		get
		{
			if (mReplacement != null) return mReplacement.sprite;

			if (!mSpriteSet) mSprite = null;

			if (mSprite == null && mAtlas != null && !string.IsNullOrEmpty(mFont.spriteName))
			{
				mSprite = mAtlas.GetSprite(mFont.spriteName);
				if (mSprite == null) mSprite = mAtlas.GetSprite(name);
				mSpriteSet = true;

				if (mSprite == null)
				{
					Debug.LogError("Can't find the sprite '" + mFont.spriteName + "' in UIAtlas on " + NGUITools.GetHierarchy(mAtlas.gameObject));
					mFont.spriteName = null;
				}
			}
			return mSprite;
		}
	}

	/// <summary>
	/// Setting a replacement atlas value will cause everything using this font to use the replacement font instead.
	/// Suggested use: set up all your widgets to use a dummy font that points to the real font. Switching that font to
	/// another one (for example an eastern language one) is then a simple matter of setting this field on your dummy font.
	/// </summary>

	public UIFont replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIFont rep = value;
			if (rep == this) rep = null;

			if (mReplacement != rep)
			{
				if (rep != null && rep.replacement == this) rep.replacement = null;
				if (mReplacement != null) MarkAsDirty();
				mReplacement = rep;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Trim the glyphs, making sure they never go past the trimmed texture bounds.
	/// </summary>

	void Trim ()
	{
		Texture tex = mAtlas.texture;

		if (tex != null && mSprite != null)
		{
			Rect full = NGUIMath.ConvertToPixels(mUVRect, texture.width, texture.height, true);
			Rect trimmed = (mAtlas.coordinates == UIAtlas.Coordinates.TexCoords) ?
				NGUIMath.ConvertToPixels(mSprite.outer, tex.width, tex.height, true) : mSprite.outer;

			int xMin = Mathf.RoundToInt(trimmed.xMin - full.xMin);
			int yMin = Mathf.RoundToInt(trimmed.yMin - full.yMin);
			int xMax = Mathf.RoundToInt(trimmed.xMax - full.xMin);
			int yMax = Mathf.RoundToInt(trimmed.yMax - full.yMin);

			mFont.Trim(xMin, yMin, xMax, yMax);
		}
	}

	/// <summary>
	/// Helper function that determines whether the font uses the specified one, taking replacements into account.
	/// </summary>

	bool References (UIFont font)
	{
		if (font == null) return false;
		if (font == this) return true;
		return (mReplacement != null) ? mReplacement.References(font) : false;
	}

	/// <summary>
	/// Helper function that determines whether the two atlases are related.
	/// </summary>

	static public bool CheckIfRelated (UIFont a, UIFont b)
	{
		if (a == null || b == null) return false;
		return a == b || a.References(b) || b.References(a);
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	public void MarkAsDirty ()
	{
		mSprite = null;
		UILabel[] labels = NGUITools.FindActive<UILabel>();

		for (int i = 0, imax = labels.Length; i < imax; ++i)
		{
			UILabel lbl = labels[i];

			if (lbl.enabled && lbl.gameObject.activeSelf && CheckIfRelated(this, lbl.font))
			{
				UIFont fnt = lbl.font;
				lbl.font = null;
				lbl.font = fnt;
			}
		}
	}

	/// <summary>
	/// Get the printed size of the specified string. The returned value is in local coordinates. Multiply by transform's scale to get pixels.
	/// </summary>

	public Vector2 CalculatePrintedSize (string text, bool encoding)
	{
		if (mReplacement != null) return mReplacement.CalculatePrintedSize(text, encoding);

		Vector2 v = Vector2.zero;

		if (mFont != null && mFont.isValid && !string.IsNullOrEmpty(text))
		{
			if (encoding) text = NGUITools.StripSymbols(text);

			int length = text.Length;
			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			int lineHeight = (mFont.charSize + mSpacingY);

			for (int i = 0; i < length; ++i)
			{
				char c = text[i];

				// Start a new line
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += lineHeight;
					prev = 0;
					continue;
				}

				// Skip invalid characters
				if (c < ' ') { prev = 0; continue; }

				// See if there is a symbol matching this text
				BMSymbol symbol = encoding ? mFont.MatchSymbol(text, i, length) : null;

				if (symbol == null)
				{
					// Get the glyph for this character
					BMGlyph glyph = mFont.GetGlyph(c);

					if (glyph != null)
					{
						x += mSpacingX + ((prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance);
						prev = c;
					}
				}
				else
				{
					// Symbol found -- use it
					x += mSpacingX + symbol.width;
					i += symbol.length - 1;
					prev = 0;
				}
			}

			// Convert from pixel coordinates to local coordinates
			float scale = (mFont.charSize > 0) ? 1f / mFont.charSize : 1f;
			v.x = scale * ((x > maxX) ? x : maxX);
			v.y = scale * (y + lineHeight);
		}
		return v;
	}

	/// <summary>
	/// Convenience function that ends the line by either appending a new line character or replacing a space with one.
	/// </summary>

	static void EndLine (ref StringBuilder s)
	{
		int i = s.Length - 1;
		if (i > 0 && s[i] == ' ') s[i] = '\n';
		else s.Append('\n');
	}

	/// <summary>
	/// Text wrapping functionality. The 'maxWidth' should be in local coordinates (take pixels and divide them by transform's scale).
	/// </summary>

	public string WrapText (string text, float maxWidth, bool multiline, bool encoding)
	{
		if (mReplacement != null) return mReplacement.WrapText(text, maxWidth, multiline, encoding);

		// Width of the line in pixels
		int lineWidth = Mathf.RoundToInt(maxWidth * size);
		if (lineWidth < 1) return text;

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		int remainingWidth = lineWidth;
		int previousChar = 0;
		int start = 0;
		int offset = 0;
		bool lineIsEmpty = true;

		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (!multiline) break;
				remainingWidth = lineWidth;

				// Add the previous word to the final string
				if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
				else sb.Append(ch);

				lineIsEmpty = true;
				start = offset + 1;
				previousChar = 0;
				continue;
			}

			// If this marks the end of a word, add it to the final string.
			if (ch == ' ' && previousChar != ' ' && start < offset)
			{
				sb.Append(text.Substring(start, offset - start + 1));
				lineIsEmpty = false;
				start = offset + 1;
				previousChar = ch;
			}

			// When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
			if (encoding && ch == '[')
			{
				if (offset + 2 < textLength)
				{
					if (text[offset + 1] == '-' && text[offset + 2] == ']')
					{
						offset += 2;
						continue;
					}
					else if (offset + 7 < textLength && text[offset + 7] == ']')
					{
						offset += 7;
						continue;
					}
				}
			}

			// See if there is a symbol matching this text
			BMSymbol symbol = encoding ? mFont.MatchSymbol(text, offset, textLength) : null;

			// Find the glyph for this character
			BMGlyph glyph = (symbol == null) ? mFont.GetGlyph(ch) : null;

			// Calculate how wide this symbol or character is going to be
			int glyphWidth = mSpacingX;

			if (symbol != null)
			{
				glyphWidth += symbol.width;
			}
			else if (glyph != null)
			{
				glyphWidth += (previousChar != 0) ? glyph.advance + glyph.GetKerning(previousChar) : glyph.advance;
			}
			else continue;

			// Remaining width after this glyph gets printed
			remainingWidth -= glyphWidth;

			// Doesn't fit?
			if (remainingWidth < 0)
			{
				// Can't start a new line
				if (lineIsEmpty || !multiline)
				{
					// This is the first word on the line -- add it up to the character that fits
					sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));

					if (!multiline)
					{
						start = offset;
						break;
					}
					EndLine(ref sb);

					// Start a brand-new line
					lineIsEmpty = true;

					if (ch == ' ')
					{
						start = offset + 1;
						remainingWidth = lineWidth;
					}
					else
					{
						start = offset;
						remainingWidth = lineWidth - glyphWidth;
					}
					previousChar = 0;
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = lineWidth;
					offset = start - 1;
					previousChar = 0;
					if (!multiline) break;
					EndLine(ref sb);
					continue;
				}
			}
			else previousChar = ch;

			// Advance the offset past the symbol
			if (symbol != null)
			{
				offset += symbol.length - 1;
				previousChar = 0;
			}
		}

		if (start < offset) sb.Append(text.Substring(start, offset - start));
		return sb.ToString();
	}

	/// <summary>
	/// Align the vertices to be right or center-aligned given the specified line width.
	/// </summary>

	void Align (BetterList<Vector3> verts, int indexOffset, Alignment alignment, int x, int lineWidth)
	{
		if (alignment != Alignment.Left && mFont.charSize > 0)
		{
			float offset = (alignment == Alignment.Right) ? lineWidth - x : (lineWidth - x) * 0.5f;
			offset = Mathf.RoundToInt(offset);
			if (offset < 0f) offset = 0f;
			offset /= mFont.charSize;

			Vector3 temp;
			for (int i = indexOffset; i < verts.size; ++i)
			{
				temp = verts.buffer[i];
				temp.x += offset;
				verts.buffer[i] = temp;
			}
		}
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// Note: 'lineWidth' parameter should be in pixels.
	/// </summary>

	public void Print (string text, Color color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols,
		bool encoding, Alignment alignment, int lineWidth)
	{
		if (mReplacement != null)
		{
			mReplacement.Print(text, color, verts, uvs, cols, encoding, alignment, lineWidth);
		}
		else if (mFont != null && text != null)
		{
			if (!mFont.isValid)
			{
				Debug.LogError("Attempting to print using an invalid font!");
				return;
			}

			mColors.Clear();
			mColors.Add(color);

			Vector2 scale = mFont.charSize > 0 ? new Vector2(1f / mFont.charSize, 1f / mFont.charSize) : Vector2.one;

			int indexOffset = verts.size;
			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			int lineHeight = (mFont.charSize + mSpacingY);
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			float invX = uvRect.width / mFont.texWidth;
			float invY = mUVRect.height / mFont.texHeight;
			int textLength = text.Length;

			for (int i = 0; i < textLength; ++i)
			{
				char c = text[i];

				if (c == '\n')
				{
					if (x > maxX) maxX = x;

					if (alignment != Alignment.Left)
					{
						Align(verts, indexOffset, alignment, x, lineWidth);
						indexOffset = verts.size;
					}

					x = 0;
					y += lineHeight;
					prev = 0;
					continue;
				}

				if (c < ' ')
				{
					prev = 0;
					continue;
				}

				if (encoding && c == '[')
				{
					int retVal = NGUITools.ParseSymbol(text, i, mColors);

					if (retVal > 0)
					{
						color = mColors[mColors.Count - 1];
						i += retVal - 1;
						continue;
					}
				}

				// See if there is a symbol matching this text
				BMSymbol symbol = encoding ? mFont.MatchSymbol(text, i, textLength) : null;

				if (symbol == null)
				{
					BMGlyph glyph = mFont.GetGlyph(c);
					if (glyph == null) continue;

					if (prev != 0) x += glyph.GetKerning(prev);

					if (c == ' ')
					{
						x += mSpacingX + glyph.advance;
						prev = c;
						continue;
					}

					v0.x =  scale.x * (x + glyph.offsetX);
					v0.y = -scale.y * (y + glyph.offsetY);

					v1.x = v0.x + scale.x * glyph.width;
					v1.y = v0.y - scale.y * glyph.height;

					u0.x = mUVRect.xMin + invX * glyph.x;
					u0.y = mUVRect.yMax - invY * glyph.y;

					u1.x = u0.x + invX * glyph.width;
					u1.y = u0.y - invY * glyph.height;

					x += mSpacingX + glyph.advance;
					prev = c;
				}
				else
				{
					v0.x =  scale.x * x;
					v0.y = -scale.y * y;

					v1.x = v0.x + scale.x * symbol.width;
					v1.y = v0.y - scale.y * symbol.height;

					u0.x = mUVRect.xMin + invX * symbol.x;
					u0.y = mUVRect.yMax - invY * symbol.y;

					u1.x = u0.x + invX * symbol.width;
					u1.y = u0.y - invY * symbol.height;

					x += mSpacingX + symbol.width;
					i += symbol.length - 1;
					prev = 0;
				}

				verts.Add(new Vector3(v1.x, v0.y));
				verts.Add(new Vector3(v1.x, v1.y));
				verts.Add(new Vector3(v0.x, v1.y));
				verts.Add(new Vector3(v0.x, v0.y));

				uvs.Add(new Vector2(u1.x, u0.y));
				uvs.Add(new Vector2(u1.x, u1.y));
				uvs.Add(new Vector2(u0.x, u1.y));
				uvs.Add(new Vector2(u0.x, u0.y));

				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
			}

			if (alignment != Alignment.Left && indexOffset < verts.size)
			{
				Align(verts, indexOffset, alignment, x, lineWidth);
				indexOffset = verts.size;
			}
		}
	}
}
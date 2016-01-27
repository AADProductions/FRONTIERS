//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very simple UI sprite -- a simple quad of specified size, drawn using a part of the texture atlas.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Basic)")]
public class UISprite : UIWidget
{
	// Cached and saved values
	[HideInInspector][SerializeField] UIAtlas mAtlas;
	[HideInInspector][SerializeField] string mSpriteName;

	protected UIAtlas.Sprite mSprite;
	protected Rect mOuter;
	protected Rect mOuterUV;

	// BUG: There is a bug in Unity 3.4.2 and all the way up to 3.5 b7 -- when instantiating from prefabs,
	// for some strange reason classes get initialized with default values. So for example, 'mSprite' above
	// gets initialized as if it was created with 'new UIAtlas.Sprite()' instead of 'null'. Fun, huh?

	bool mSpriteSet = false;
	string mLastName = "";

	/// <summary>
	/// Outer set of UV coordinates.
	/// </summary>

	public Rect outerUV { get { UpdateUVs(false); return mOuterUV; } }

	/// <summary>
	/// Atlas used by this widget.
	/// </summary>
 
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				mAtlas = value;
				mSpriteSet = false;
				mSprite = null;

				// Update the material
				material = (mAtlas != null) ? mAtlas.spriteMaterial : null;

				// Automatically choose the first sprite
				if (string.IsNullOrEmpty(mSpriteName))
				{
					if (mAtlas != null && mAtlas.spriteList.Count > 0)
					{
						sprite = mAtlas.spriteList[0];
						mSpriteName = mSprite.name;
					}
				}

				// Re-link the sprite
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string sprite = mSpriteName;
					mSpriteName = "";
					spriteName = sprite;
					mChanged = true;
					UpdateUVs(true);
				}
			}
		}
	}

	/// <summary>
	/// Sprite within the atlas used to draw this widget.
	/// </summary>
 
	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				// If the sprite name hasn't been set yet, no need to do anything
				if (string.IsNullOrEmpty(mSpriteName)) return;

				// Clear the sprite name and the sprite reference
				mSpriteName = "";
				mSprite = null;
				mChanged = true;
			}
			else if (mSpriteName != value)
			{
				// If the sprite name changes, the sprite reference should also be updated
				mSpriteName = value;
				mSprite = null;
				mChanged = true;
				if (mSprite != null) UpdateUVs(true);
			}
		}
	}

	/// <summary>
	/// Get the sprite used by the atlas.
	/// </summary>

	public UIAtlas.Sprite sprite
	{
		get
		{
			if (!mSpriteSet) mSprite = null;

			if (mSprite == null && mAtlas != null)
			{
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					sprite = mAtlas.GetSprite(mSpriteName);
				}

				if (mSprite == null && mAtlas.spriteList.Count > 0)
				{
					sprite = mAtlas.spriteList[0];
					mSpriteName = mSprite.name;
				}

				// If the sprite has been set, update the material
				if (mSprite != null) material = mAtlas.spriteMaterial;
			}
			return mSprite;
		}
		set
		{
			mSprite = value;
			mSpriteSet = true;
			material = (mSprite != null && mAtlas != null) ? mAtlas.spriteMaterial : null;
		}
	}

	/// <summary>
	/// Helper function that calculates the relative offset based on the current pivot.
	/// </summary>

	override public Vector2 pivotOffset
	{
		get
		{
			Vector2 v = Vector2.zero;

			if (sprite != null)
			{
				Pivot pv = pivot;

				if (pv == Pivot.Top || pv == Pivot.Center || pv == Pivot.Bottom) v.x = (-1f - mSprite.paddingRight + mSprite.paddingLeft) * 0.5f;
				else if (pv == Pivot.TopRight || pv == Pivot.Right || pv == Pivot.BottomRight) v.x = -1f - mSprite.paddingRight;
				else v.x = mSprite.paddingLeft;

				if (pv == Pivot.Left || pv == Pivot.Center || pv == Pivot.Right) v.y = (1f + mSprite.paddingBottom - mSprite.paddingTop) * 0.5f;
				else if (pv == Pivot.BottomLeft || pv == Pivot.Bottom || pv == Pivot.BottomRight) v.y = 1f + mSprite.paddingBottom;
				else v.y = -mSprite.paddingTop;
			}
			return v;
		}
	}

	/// <summary>
	/// Retrieve the material used by the font.
	/// </summary>

	public override Material material
	{
		get
		{
			Material mat = base.material;

			if (mat == null)
			{
				mat = (mAtlas != null) ? mAtlas.spriteMaterial : null;
				mSprite = null;
				material = mat;
				if (mat != null) UpdateUVs(true);
			}
			return mat;
		}
	}

	/// <summary>
	/// Dimensions of the sprite's border, if any.
	/// </summary>

	public virtual Vector4 border { get { return Vector4.zero; } }

	/// <summary>
	/// Update the texture UVs used by the widget.
	/// </summary>

	virtual public void UpdateUVs (bool force)
	{
		if (sprite != null && (force || mOuter != mSprite.outer))
		{
			Texture tex = mainTexture;

			if (tex != null)
			{
				mOuter = mSprite.outer;
				mOuterUV = mOuter;

				if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
				{
					mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
				}
				mChanged = true;
			}
		}
	}

	/// <summary>
	/// Adjust the scale of the widget to make it pixel-perfect.
	/// </summary>

	override public void MakePixelPerfect ()
	{
		if (sprite == null) return;

		Texture tex = mainTexture;
		Vector3 scale = cachedTransform.localScale;

		if (tex != null)
		{
			Rect rect = NGUIMath.ConvertToPixels(outerUV, tex.width, tex.height, true);
			float pixelSize = atlas.pixelSize;
			scale.x = Mathf.RoundToInt(rect.width * pixelSize);
			scale.y = Mathf.RoundToInt(rect.height * pixelSize);
			scale.z = 1f;
			cachedTransform.localScale = scale;
		}

		int width  = Mathf.RoundToInt(scale.x * (1f + mSprite.paddingLeft + mSprite.paddingRight));
		int height = Mathf.RoundToInt(scale.y * (1f + mSprite.paddingTop + mSprite.paddingBottom));

		Vector3 pos = cachedTransform.localPosition;
		pos.z = Mathf.RoundToInt(pos.z);

		if (width % 2 == 1 && (pivot == Pivot.Top || pivot == Pivot.Center || pivot == Pivot.Bottom))
		{
			pos.x = Mathf.Floor(pos.x) + 0.5f;
		}
		else
		{
			pos.x = Mathf.Round(pos.x);
		}

		if (height % 2 == 1 && (pivot == Pivot.Left || pivot == Pivot.Center || pivot == Pivot.Right))
		{
			pos.y = Mathf.Ceil(pos.y) - 0.5f;
		}
		else
		{
			pos.y = Mathf.Round(pos.y);
		}

		cachedTransform.localPosition = pos;
	}

	/// <summary>
	/// Set the atlas and the sprite.
	/// </summary>

	override protected void OnStart ()
	{
		if (mAtlas != null)
		{
			UpdateUVs(true);
		}
	}

	/// <summary>
	/// Update the UV coordinates.
	/// </summary>

	override public bool OnUpdate ()
	{
		if (mLastName != mSpriteName)
		{
			mSprite = null;
			mChanged = true;
			mLastName = mSpriteName;
			UpdateUVs(false);
			return true;
		}
		UpdateUVs(false);
		return false;
	}

	/// <summary>
	/// Virtual function called by the UIScreen that fills the buffers.
	/// </summary>

	override public void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector2 uv0 = new Vector2(mOuterUV.xMin, mOuterUV.yMin);
		Vector2 uv1 = new Vector2(mOuterUV.xMax, mOuterUV.yMax);

		verts.Add(new Vector3(1f,  0f, 0f));
		verts.Add(new Vector3(1f, -1f, 0f));
		verts.Add(new Vector3(0f, -1f, 0f));
		verts.Add(new Vector3(0f,  0f, 0f));

		uvs.Add(uv1);
		uvs.Add(new Vector2(uv1.x, uv0.y));
		uvs.Add(uv0);
		uvs.Add(new Vector2(uv0.x, uv1.y));

		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
		cols.Add(color);
	}
}
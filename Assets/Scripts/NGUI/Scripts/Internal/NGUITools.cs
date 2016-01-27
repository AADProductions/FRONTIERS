//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUITools
{
	static AudioListener mListener;

	/// <summary>
	/// Play the specified audio clip.
	/// </summary>

	static public AudioSource PlaySound (AudioClip clip) { return PlaySound(clip, 1f, 1f); }

	/// <summary>
	/// Play the specified audio clip with the specified volume.
	/// </summary>

	static public AudioSource PlaySound (AudioClip clip, float volume) { return PlaySound(clip, volume, 1f); }

	/// <summary>
	/// Play the specified audio clip with the specified volume and pitch.
	/// </summary>

	static public AudioSource PlaySound (AudioClip clip, float volume, float pitch)
	{
		if (clip != null)
		{
			if (mListener == null)
			{
				mListener = GameObject.FindObjectOfType(typeof(AudioListener)) as AudioListener;

				if (mListener == null)
				{
					Camera cam = Camera.main;
					if (cam == null) cam = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
					if (cam != null) mListener = cam.gameObject.AddComponent<AudioListener>();
				}
			}

			if (mListener != null)
			{
				AudioSource source = mListener.audio;
				if (source == null) source = mListener.gameObject.AddComponent<AudioSource>();
				source.pitch = pitch;
				source.PlayOneShot(clip, volume);
				return source;
			}
		}
		return null;
	}

	/// <summary>
	/// New WWW call can fail if the crossdomain policy doesn't check out. Exceptions suck. It's much more elegant to check for null instead.
	/// </summary>

	static public WWW OpenURL (string url)
	{
		////Debug.Log(url);
#if UNITY_FLASH
		Debug.LogError("WWW is not yet implemented in Flash");
		return null;
#else
		WWW www = null;
		try { www = new WWW(url); }
		catch (System.Exception ex) { Debug.LogError(ex.Message); }
		return www;
#endif
	}

	/// <summary>
	/// Same as Random.Range, but the returned value is between min and max, inclusive.
	/// Unity's Random.Range is less than max instead, unless min == max.
	/// This means Range(0,1) produces 0 instead of 0 or 1. That's unacceptable.
	/// </summary>

	static public int RandomRange (int min, int max)
	{
		if (min == max) return min;
		return UnityEngine.Random.Range(min, max + 1);
	}

	/// <summary>
	/// Returns the hierarchy of the object in a human-readable format.
	/// </summary>

	static public string GetHierarchy (GameObject obj)
	{
		string path = obj.name;

		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return "\"" + path + "\"";
	}

	/// <summary>
	/// Parse a RrGgBb color encoded in the string.
	/// </summary>

	static public Color ParseColor (string text, int offset)
	{
		int r = (NGUIMath.HexToDecimal(text[offset])	 << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int g = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int b = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float f = 1f / 255f;
		return new Color(f * r, f * g, f * b);
	}

	/// <summary>
	/// The reverse of ParseColor -- encodes a color in RrGgBb format.
	/// </summary>

	static public string EncodeColor (Color c)
	{
#if UNITY_FLASH
		// int.ToString(format) doesn't seem to be supported on Flash as of 3.5.0 -- it simply silently crashes
		return "FFFFFF";
#else
		int i = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return i.ToString("X6");
#endif
	}

	/// <summary>
	/// Parse an embedded symbol, such as [FFAA00] (set color) or [-] (undo color change)
	/// </summary>

	static public int ParseSymbol (string text, int index, List<Color> colors)
	{
		int length = text.Length;

		if (index + 2 < length)
		{
			if (text[index + 1] == '-')
			{
				if (text[index + 2] == ']')
				{
					if (colors != null && colors.Count > 1) colors.RemoveAt(colors.Count - 1);
					return 3;
				}
			}
			else if (index + 7 < length)
			{
				if (text[index + 7] == ']')
				{
					if (colors != null)
					{
						Color c = ParseColor(text, index + 1);
						c.a = colors[colors.Count - 1].a;
						colors.Add(c);
					}
					return 8;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Runs through the specified string and removes all color-encoding symbols.
	/// </summary>

	static public string StripSymbols (string text)
	{
		if (text != null)
		{
			text = text.Replace("\\n", "\n");

			for (int i = 0, imax = text.Length; i < imax; )
			{
				char c = text[i];

				if (c == '[')
				{
					int retVal = ParseSymbol(text, i, null);

					if (retVal > 0)
					{
						text = text.Remove(i, retVal);
						imax = text.Length;
						continue;
					}
				}
				++i;
			}
		}
		return text;
	}

	/// <summary>
	/// Find all active objects of specified type.
	/// </summary>

	static public T[] FindActive<T> () where T : Component
	{
		return GameObject.FindObjectsOfType(typeof(T)) as T[];
	}

	/// <summary>
	/// Find the camera responsible for drawing the objects on the specified layer.
	/// </summary>

	static public Camera FindCameraForLayer (int layer)
	{
		int layerMask = 1 << layer;

		Camera[] cameras = NGUITools.FindActive <Camera>();

		for (int i = 0, imax = cameras.Length; i < imax; ++i)
		{
			Camera cam = cameras[i];

			if ((cam.cullingMask & layerMask) != 0)
			{
				return cam;
			}
		}
		return null;
	}

	/// <summary>
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public BoxCollider AddWidgetCollider (GameObject go)
	{
		if (go != null)
		{
			Collider col = go.GetComponent<Collider>();
			BoxCollider box = col as BoxCollider;

			if (box == null)
			{
				if (col != null)
				{
					if (Application.isPlaying) GameObject.Destroy(col);
					else GameObject.DestroyImmediate(col);
				}
				box = go.AddComponent<BoxCollider>();
			}

			int depth = NGUITools.CalculateNextDepth(go);

			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
			box.isTrigger = true;
			box.center = b.center + Vector3.back * (depth * 0.25f);
			box.size = new Vector3(b.size.x, b.size.y, 0f);
			return box;
		}
		return null;
	}

	/// <summary>
	/// Want to swap a low-res atlas for a hi-res one? Just use this function.
	/// </summary>

	[Obsolete("Use UIAtlas.replacement instead")]
	static public void ReplaceAtlas (UIAtlas before, UIAtlas after)
	{
		UISprite[] sprites = NGUITools.FindActive<UISprite>();

		for (int i = 0, imax = sprites.Length; i < imax; ++i)
		{
			UISprite sprite = sprites[i];

			if (sprite.atlas == before)
			{
				sprite.atlas = after;
			}
		}

		UILabel[] labels = NGUITools.FindActive<UILabel>();

		for (int i = 0, imax = labels.Length; i < imax; ++i)
		{
			UILabel lbl = labels[i];

			if (lbl.font != null && lbl.font.atlas == before)
			{
				lbl.font.atlas = after;
			}
		}
	}

	/// <summary>
	/// Want to swap a low-res font for a hi-res one? This is the way.
	/// </summary>

	[Obsolete("Use UIFont.replacement instead")]
	static public void ReplaceFont (UIFont before, UIFont after)
	{
		UILabel[] labels = NGUITools.FindActive<UILabel>();

		for (int i = 0, imax = labels.Length; i < imax; ++i)
		{
			UILabel lbl = labels[i];
			if (lbl.font == before) lbl.font = after;
		}
	}

	/// <summary>
	/// Helper function that returns the string name of the type.
	/// </summary>

	static public string GetName<T> () where T : Component
	{
		string s = typeof(T).ToString();
		if (s.StartsWith("UI")) s = s.Substring(2);
		else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
		return s;
	}

	/// <summary>
	/// Add a new child game object.
	/// </summary>

	static public GameObject AddChild (GameObject parent)
	{
		GameObject go = new GameObject("new");

		if (parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

	/// <summary>
	/// Instantiate an object and add it to the specified parent.
	/// </summary>

	static public GameObject AddChild (GameObject parent, GameObject prefab)
	{
		GameObject go = GameObject.Instantiate(prefab) as GameObject;

		if (go != null && parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

	/// <summary>
	/// Gathers all widgets and calculates the depth for the next widget.
	/// </summary>

	static public int CalculateNextDepth (GameObject go)
	{
		int depth = -1;
		UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
		for (int i = 0, imax = widgets.Length; i < imax; ++i) depth = Mathf.Max(depth, widgets[i].depth);
		return depth + 1;
	}

	/// <summary>
	/// Add a child object to the specified parent and attaches the specified script to it.
	/// </summary>

	static public T AddChild<T> (GameObject parent) where T : Component
	{
		GameObject go = AddChild(parent);
		go.name = GetName<T>();
		return go.AddComponent<T>();
	}

	/// <summary>
	/// Add a new widget of specified type.
	/// </summary>

	static public T AddWidget<T> (GameObject go) where T : UIWidget
	{
		int depth = CalculateNextDepth(go);

		// Create the widget and place it above other widgets
		T widget = AddChild<T>(go);
		widget.depth = depth;

		// Clear the local transform
		Transform t = widget.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = new Vector3(100f, 100f, 1f);
		widget.gameObject.layer = go.layer;
		return widget;
	}

	/// <summary>
	/// Add a sprite appropriate for the specified atlas sprite.
	/// It will be a UISlicedSprite if the sprite has an inner rect, and a regular sprite otherwise.
	/// </summary>

	static public UISprite AddSprite (GameObject go, UIAtlas atlas, string spriteName)
	{
		UIAtlas.Sprite sp = (atlas != null) ? atlas.GetSprite(spriteName) : null;
		UISprite sprite = (sp == null || sp.inner == sp.outer) ? AddWidget<UISprite>(go) : (UISprite)AddWidget<UISlicedSprite>(go);
		sprite.atlas = atlas;
		sprite.spriteName = spriteName;
		return sprite;
	}

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>

	static public T FindInParents<T> (GameObject go) where T : Component
	{
		if (go == null) return null;
		object comp = go.GetComponent<T>();

		if (comp == null)
		{
			Transform t = go.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}

	/// <summary>
	/// Destroy the specified object, immediately if in edit mode.
	/// </summary>

	static public void Destroy (UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
			else UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	/// <summary>
	/// Destroy the specified object immediately, unless not in the editor, in which case the regular Destroy is used instead.
	/// </summary>

	static public void DestroyImmediate (UnityEngine.Object obj)
	{
//		//Debug.Log (obj.name);
		if (obj != null)
		{
			if (Application.isEditor) UnityEngine.Object.DestroyImmediate(obj);
			else UnityEngine.Object.Destroy(obj);
		}
	}

	/// <summary>
	/// Call the specified function on all objects in the scene.
	/// </summary>

	static public void Broadcast (string funcName)
	{
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		for (int i = 0, imax = gos.Length; i < imax; ++i) gos[i].SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Call the specified function on all objects in the scene.
	/// </summary>

	static public void Broadcast (string funcName, object param)
	{
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		for (int i = 0, imax = gos.Length; i < imax; ++i) gos[i].SendMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Determines whether the 'parent' contains a 'child' in its hierarchy.
	/// </summary>

	static public bool IsChild (Transform parent, Transform child)
	{
		if (parent == null || child == null) return false;

		while (child != null)
		{
			if (child == parent) return true;
			child = child.parent;
		}
		return false;
	}

	/// <summary>
	/// Activate the specified object and all of its children.
	/// </summary>

	static void Activate (Transform t)
	{
		t.gameObject.SetActive (true);

		for (int i = 0, imax = t.childCount; i < imax; ++i)
		{
			Transform child = t.GetChild(i);
			Activate(child);
		}
	}

	/// <summary>
	/// Deactivate the specified object and all of its children.
	/// </summary>

	static void Deactivate (Transform t)
	{
		for (int i = 0, imax = t.childCount; i < imax; ++i)
		{
			Transform child = t.GetChild(i);
			Deactivate(child);
		}
		t.gameObject.SetActive (false);
	}

	/// <summary>
	/// SetActive has some strange side-effects... better to do it manually.
	/// </summary>

	static public void SetActive (GameObject go, bool state)
	{		
		if (state)
		{
			Activate(go.transform);
		}
		else
		{
			Deactivate(go.transform);
		}
	}
}
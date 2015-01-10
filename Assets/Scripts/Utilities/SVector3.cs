using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;

[Serializable]
public class SimpleVar
{
	[XmlAttribute ("DefaultValue")]
	public int DefaultValue;
	[XmlAttribute ("Value")]
	public int Value;
	[XmlAttribute ("Min")]
	public int Min;
	[XmlAttribute ("Max")]
	public int Max;

	public float NormalizedValue {
		get {
			float val = (float)Value;
			float min = (float)Min;
			float max = (float)Max;
			if (Min < 0) {
				val += Mathf.Abs (Min);
				min += Mathf.Abs (Min);
				max += Mathf.Abs (Min);
			}
			return Mathf.Clamp01 (((val - min) / (max - min)));
		}
	}
}

[Serializable]
public class SVector3
{
	public static SVector3 zero {
		get {
			return new SVector3 ();
		}
	}

	public static SVector3 one {
		get {
			return new SVector3 (1f, 1f, 1f);
		}
	}

	public SVector3 ()
	{
		x = 0f;
		y = 0f;
		z = 0f;
	}

	public SVector3 (Vector3 v3)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}

	public SVector3 (float vx, float vy, float vz)
	{
		x = vx;
		y = vy;
		z = vz;
	}

	public float x;
	public float y;
	public float z;

	public static implicit operator Vector3 (SVector3 sv3)
	{
		if (sv3 == null) {
			return Vector3.zero;
		}
		return new Vector3 (sv3.x, sv3.y, sv3.z);
	}

	public static implicit operator SVector3 (Vector3 v3)
	{
		return new SVector3 (v3);
	}

	public static Vector3 operator + (Vector3 sv1, SVector3 sv2)
	{
		return new Vector3 (sv1.x + sv2.x, sv1.y + sv2.y, sv1.z + sv2.z);
	}

	public static SVector3 operator + (SVector3 sv1, SVector3 sv2)
	{
		return new SVector3 (sv1.x + sv2.x, sv1.y + sv2.y, sv1.z + sv2.z);
	}

	public static SVector3 operator - (Vector3 sv1, SVector3 sv2)
	{
		return new SVector3 (sv1.x - sv2.x, sv1.y - sv2.y, sv1.z - sv2.z);
	}

	public static SVector3 operator - (SVector3 sv1, Vector3 sv2)
	{
		return new SVector3 (sv1.x - sv2.x, sv1.y - sv2.y, sv1.z - sv2.z);
	}

	public static SVector3 operator - (SVector3 sv1, SVector3 sv2)
	{
		return new SVector3 (sv1.x - sv2.x, sv1.y - sv2.y, sv1.z - sv2.z);
	}

	public static void CopyTo (SVector3[] fromArray, Vector3[] toArray)
	{
		toArray = new Vector3 [fromArray.Length];

		for (int i = 0; i < toArray.Length; i++) {
			toArray [i] = fromArray [i];
		}
	}

	public static void CopyTo (Vector3[] fromArray, SVector3[] toArray)
	{
		toArray = new SVector3 [fromArray.Length];

		for (int i = 0; i < toArray.Length; i++) {
			toArray [i] = fromArray [i];
		}
	}

	public override string ToString ()
	{
		return (x.ToString () + "," + y.ToString () + "," + z.ToString ());
	}

	public static SVector3 Parse (string sVector3String)
	{
		SVector3 sVector3 = new SVector3 ();
		char[ ] splitChar = new char [1] { ',' };
		string[ ] splitString = sVector3String.Split (splitChar, StringSplitOptions.None);

		if (splitString.Length >= 2) {
			sVector3.x = float.Parse (splitString [0]);
			sVector3.y = float.Parse (splitString [1]);
			sVector3.z = float.Parse (splitString [2]);
		}

		return sVector3;
	}

	public static SVector3 Random (float min, float max)
	{
		SVector3 sVector3 = new SVector3 ();
		sVector3.x = UnityEngine.Random.Range (min, max);
		sVector3.y = UnityEngine.Random.Range (min, max);
		sVector3.z = UnityEngine.Random.Range (min, max);
		return sVector3;
	}
}

[Serializable]
public struct SVector2
{
	public static SVector2 zero {
		get {
			return new SVector2 ();
		}
	}

	public static SVector2 one {
		get {
			return new SVector2 (1f, 1f);
		}
	}

	public SVector2 (Vector2 v2)
	{
		x = v2.x;
		y = v2.y;
	}

	public SVector2 (float vx, float vy)
	{
		x = vx;
		y = vy;
	}

	public float x;
	public float y;

	public static implicit operator Vector2 (SVector2 sv2)
	{
		return new Vector2 (sv2.x, sv2.y);
	}

	public static implicit operator SVector2 (Vector2 v2)
	{
		return new SVector2 (v2);
	}

	public static void CopyTo (SVector2[] fromArray, Vector2[] toArray)
	{
		toArray = new Vector2 [fromArray.Length];
		for (int i = 0; i < toArray.Length; i++) {
			toArray [i] = fromArray [i];
		}
	}

	public static void CopyTo (Vector2[] fromArray, SVector2[] toArray)
	{
		toArray = new SVector2 [fromArray.Length];
		for (int i = 0; i < toArray.Length; i++) {
			toArray [i] = fromArray [i];
		}
	}
}

[Serializable]
public class STransform
{
	public static STransform zero {
		get {
			STransform transzero = new STransform (Vector3.zero, Vector3.zero, Vector3.one);
			return transzero;
		}
	}

	public STransform ()
	{
		Position = SVector3.zero;
		Rotation = SVector3.zero;
		Scale = SVector3.one;
	}

	public bool IsApproximately (STransform transform)
	{
		return Mathf.Approximately (Position.x, transform.Position.x)
		&& Mathf.Approximately (Position.y, transform.Position.y)
		&& Mathf.Approximately (Position.z, transform.Position.z);
	}

	public STransform (SVector3 position, SVector3 rotation, SVector3 scale)
	{
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	public STransform (SVector3 position, SVector3 rotation)
	{
		Position = position;
		Rotation = rotation;
		Scale = SVector3.one;
	}

	public STransform (Transform tform)
	{
		Position = tform.localPosition;
		Rotation = tform.localRotation.eulerAngles;
		Scale = tform.localScale;
	}

	public STransform (SVector3 position)
	{
		Position = position;
		Rotation = SVector3.zero;
		Scale = SVector3.one;
	}

	public STransform (Transform tform, bool local)
	{
		if (local) {
			Position = tform.localPosition;
			Rotation = tform.localRotation.eulerAngles;
			Scale = tform.localScale;
		} else {
			Position = tform.position;
			Rotation = tform.rotation.eulerAngles;
			Scale = tform.lossyScale;
		}
	}

	public STransform (STransform tform)
	{
		Position = tform.Position;
		Rotation = tform.Rotation;
		Scale = tform.Scale;
	}

	public void CopyFrom (STransform transform)
	{
		Position = transform.Position;
		Rotation = transform.Rotation;
		Scale = transform.Scale;
	}

	public void ApplyTo (Transform transform)
	{
		ApplyTo (transform, false);
		//transform.localScale	= Scale;
	}

	public void ApplyTo (Transform transform, bool includeScale)
	{
		transform.localPosition = Position;
		transform.localRotation	= Quaternion.Euler (Rotation);
		if (includeScale) {
			transform.localScale = Scale;
		}
	}

	public void CopyFrom (Transform transform)
	{
		Position = transform.localPosition;
		Rotation = transform.localRotation.eulerAngles;
		Scale = transform.localScale;
	}

	public SVector3 Position = SVector3.zero;
	public SVector3 Rotation = SVector3.zero;
	public SVector3 Scale = SVector3.one;

	public static implicit operator STransform (Transform tform)
	{
		STransform stform = new STransform (tform.localPosition, tform.localRotation.eulerAngles, tform.localScale);
		return stform;
	}
}

[Serializable]
public struct Icon
{
	public static Icon	Empty {
		get {
			Icon emptyIcon = new Icon ();
			emptyIcon.AtlasName	= "FRONTIERS Icons Atlas";
			emptyIcon.IconName	= string.Empty;
			emptyIcon.IconColor	= Color.white;
			emptyIcon.BGColor	= Color.white;
			return emptyIcon;
		}
	}

	public static Icon	NeedDirections {
		get {
			Icon emptyIcon = new Icon ();
			emptyIcon.AtlasName	= "FRONTIERS Icons Atlas";
			emptyIcon.IconName	= "IconDirection";
			emptyIcon.IconColor	= Color.white;
			emptyIcon.BGColor	= Color.yellow;
			return emptyIcon;
		}
	}

	public bool			IsEmpty {
		get {
			return string.IsNullOrEmpty (IconName);
		}
	}

	public string AtlasName;
	public string IconName;

	public Color		IconColor {
		get {
			return new Color (ICR, ICG, ICB);
		}
		set {
			ICR = value.r;
			ICG = value.g;
			ICB = value.b;
		}
	}

	public Color		BGColor {
		get {
			return new Color (BGR, BGG, BGB);
		}
		set {
			BGR = value.r;
			BGG = value.g;
			BGB = value.b;
		}
	}

	public float ICR;
	public float ICG;
	public float ICB;
	public float BGR;
	public float BGG;
	public float BGB;
}

[Serializable]
public class SColor
{
	public float a	= 1f;
	public float r	= 0f;
	public float g	= 0f;
	public float b	= 0f;

	public SColor ()
	{

	}

	public SColor (float R, float G, float B, float A)
	{
		a = A;
		r = R;
		g = G;
		b = B;
	}

	public SColor (Color color)
	{
		a = color.a;
		r = color.r;
		g = color.g;
		b = color.b;
	}

	public static SColor Random {
		get {
			return new SColor (UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
		}
	}

	public static implicit operator string (SColor color)
	{
		return Colors.ColorToHex (new Color (color.r, color.g, color.b, color.a));
	}

	public static implicit operator SColor (string color)
	{
		return new SColor (Colors.HexToColor (color));
	}

	public static implicit operator SColor (Color color)
	{
		SColor sColor = new SColor (color);
		return sColor;
	}

	public static implicit operator SColor (Color32 color)
	{
		SColor sColor = new SColor (color.r, color.g, color.b, color.a);
		return sColor;
	}

	public static implicit operator Color (SColor sColor)
	{
		Color color = new Color (sColor.r, sColor.g, sColor.b, sColor.a);
		return color;
	}

	public static implicit operator Color32 (SColor sColor)
	{
		if (sColor == null) {
			return Color.white;
		}
		Color32 color = new Color32 ((byte) (sColor.r * 255), (byte) (sColor.g * 255), (byte) (sColor.b * 255), (byte) (sColor.a * 255));
		return color;
	}

	public override string ToString ()
	{
		return Colors.ColorToHex (new Color (r, g, b, a));
	}
}
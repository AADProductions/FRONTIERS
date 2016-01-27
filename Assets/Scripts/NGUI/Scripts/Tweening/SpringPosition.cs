//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Spring-like motion -- the farther away the object is from the target, the stronger the pull.
/// </summary>

[AddComponentMenu("NGUI/Tween/Spring Position")]
public class SpringPosition : IgnoreTimeScale
{
	public Vector3 target = Vector3.zero;
	public float strength = 10f;
	public bool worldSpace = false;
	public bool ignoreTimeScale = false;

	Transform mTrans;
	float mThreshold = 0f;

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Start () { mTrans = transform; }

	/// <summary>
	/// Advance toward the target position.
	/// </summary>

	void Update ()
	{
		float delta = ignoreTimeScale ? UpdateRealTimeDelta() : Time.deltaTime;

		if (worldSpace)
		{
			if (mThreshold == 0f) mThreshold = (target - mTrans.position).magnitude * 0.001f;
			mTrans.position = NGUIMath.SpringLerp(mTrans.position, target, strength, delta);

			if (mThreshold >= (target - mTrans.position).magnitude)
			{
				mTrans.position = target;
				enabled = false;
			}
		}
		else
		{
			if (mThreshold == 0f) mThreshold = (target - mTrans.localPosition).magnitude * 0.001f;
			mTrans.localPosition = NGUIMath.SpringLerp(mTrans.localPosition, target, strength, delta);

			if (mThreshold >= (target - mTrans.localPosition).magnitude)
			{
				mTrans.localPosition = target;
				enabled = false;
			}
		}
	}

	/// <summary>
	/// Start the tweening process.
	/// </summary>

	static public SpringPosition Begin (GameObject go, Vector3 pos, float strength)
	{
		SpringPosition sp = go.GetComponent<SpringPosition>();
		if (sp == null) sp = go.AddComponent<SpringPosition>();
		sp.target = pos;
		sp.strength = strength;

		if (!sp.enabled)
		{
			sp.mThreshold = 0f;
			sp.enabled = true;
		}
		return sp;
	}
}
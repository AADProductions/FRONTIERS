//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Implements common functionality for monobehaviours that wish to have a timeScale-independent deltaTime.
/// </summary>

[AddComponentMenu("NGUI/Internal/Ignore TimeScale Behaviour")]
public class IgnoreTimeScale : MonoBehaviour
{
	double mTime = 0;
	double mActual = 0;
	double mDelta = 0;

	/// <summary>
	/// Equivalent of Time.deltaTime not affected by timeScale, provided that UpdateRealTimeDelta() was called in the Update().
	/// </summary>

	public float realTimeDelta { get { return (float) mDelta; } }

	/// <summary>
	/// Record the current time.
	/// </summary>

	void OnEnable () { mTime = Frontiers.WorldClock.RealTime; }

	/// <summary>
	/// Record the time on start.
	/// </summary>

	void Start () { mTime = Frontiers.WorldClock.RealTime; }

	/// <summary>
	/// Update the 'realTimeDelta' parameter. Should be called once per frame.
	/// </summary>

	protected float UpdateRealTimeDelta ()
	{
		double time = Frontiers.WorldClock.RealTime;//Time.realtimeSinceStartup;
		double delta = time - mTime;
		mActual += System.Math.Max(0, delta);
		mDelta = 0.001 * System.Math.Round(mActual * 1000);
		mActual -= mDelta;
		mTime = time;
		return (float) mDelta;
	}
}
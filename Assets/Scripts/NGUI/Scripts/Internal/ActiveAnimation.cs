//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using AnimationOrTween;

/// <summary>
/// Mainly an internal script used by UIButtonPlayAnimation, but can also be used to call
/// the specified function on the game object after it finishes animating.
/// </summary>

[RequireComponent(typeof(Animation))]
[AddComponentMenu("NGUI/Internal/Active Animation")]
public class ActiveAnimation : IgnoreTimeScale
{
	/// <summary>
	/// Game object on which to call the callback function.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function to call when the animation finishes playing.
	/// </summary>

	public string callWhenFinished;

	Animation mAnim;
	Direction mLastDirection = Direction.Toggle;
	Direction mDisableDirection = Direction.Toggle;
	bool mNotify = false;

	/// <summary>
	/// Manually reset the active animation to the beginning.
	/// </summary>

	public void Reset ()
	{
		if (mAnim != null)
		{
			foreach (AnimationState state in mAnim)
			{
				if (mLastDirection == Direction.Reverse) state.time = state.length;
				else if (mLastDirection == Direction.Forward) state.time = 0f;
			}
		}
	}

	/// <summary>
	/// Notify the target when the animation finishes playing.
	/// </summary>

	void Update ()
	{
		float delta = UpdateRealTimeDelta();

		if (mAnim != null)
		{
			bool isPlaying = false;

			foreach (AnimationState state in mAnim)
			{
				float movement = state.speed * delta;
				state.time += movement;

				if (movement < 0f)
				{
					if (state.time > 0f) isPlaying = true;
					else state.time = 0f;
				}
				else
				{
					if (state.time < state.length) isPlaying = true;
					else state.time = state.length;
				}
			}

			mAnim.enabled = true;
			mAnim.Sample();
			mAnim.enabled = false;

			if (isPlaying) return;

			if (mNotify)
			{
				mNotify = false;

				if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
				{
					// Notify the event listener target
					eventReceiver.SendMessage(callWhenFinished, this, SendMessageOptions.DontRequireReceiver);
				}

				if (mDisableDirection != Direction.Toggle && mLastDirection == mDisableDirection)
				{
					NGUITools.SetActive(gameObject, false);
				}
			}
		}
		enabled = false;
	}

	/// <summary>
	/// Play the specified animation.
	/// </summary>

	void Play (string clipName, Direction playDirection)
	{
		if (mAnim != null)
		{
			// We will sample the animation manually so that it works when time is paused
			mAnim.enabled = false;

			// Determine the play direction
			if (playDirection == Direction.Toggle)
			{
				playDirection = (mLastDirection != Direction.Forward) ? Direction.Forward : Direction.Reverse;
			}

			bool noName = string.IsNullOrEmpty(clipName);

			// Play the animation if it's not already playing
			if (noName)
			{
				if (!mAnim.isPlaying) mAnim.Play();
			}
			else if (!mAnim.IsPlaying(clipName))
			{
				mAnim.Play(clipName);
			}

			// Update the animation speed based on direction -- forward or back
			foreach (AnimationState state in mAnim)
			{
				if (string.IsNullOrEmpty(clipName) || state.name == clipName)
				{
					float speed = Mathf.Abs(state.speed);
					state.speed = speed * (int)playDirection;

					// Automatically start the animation from the end if it's playing in reverse
					if (playDirection == Direction.Reverse && state.time == 0f) state.time = state.length;
					else if (playDirection == Direction.Forward && state.time == state.length) state.time = 0f;
				}
			}

			// Remember the direction for disable checks in Update() below
			mLastDirection = playDirection;
			mNotify = true;
		}
	}

	/// <summary>
	/// Play the specified animation on the specified object.
	/// </summary>

	static public ActiveAnimation Play (Animation anim, string clipName, Direction playDirection,
		EnableCondition enableBeforePlay, DisableCondition disableCondition)
	{
		if (!anim.gameObject.activeSelf)
		{
			// If the object is disabled, don't do anything
			if (enableBeforePlay != EnableCondition.EnableThenPlay) return null;

			// Enable the game object before animating it
			NGUITools.SetActive(anim.gameObject, true);
		}

		ActiveAnimation aa = anim.GetComponent<ActiveAnimation>();
		if (aa != null) aa.enabled = true;
		else aa = anim.gameObject.AddComponent<ActiveAnimation>();
		aa.mAnim = anim;
		aa.mDisableDirection = (Direction)(int)disableCondition;
		aa.Play(clipName, playDirection);
		return aa;
	}

	/// <summary>
	/// Play the specified animation.
	/// </summary>

	static public ActiveAnimation Play (Animation anim, string clipName, Direction playDirection)
	{
		return Play(anim, clipName, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
	}

	/// <summary>
	/// Play the specified animation.
	/// </summary>

	static public ActiveAnimation Play (Animation anim, Direction playDirection)
	{
		return Play(anim, null, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
	}
}
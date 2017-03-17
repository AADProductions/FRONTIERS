using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

public class BodyAnimator : MonoBehaviour
{
	public Animator animator;
	//send messages about our state to the sounds object
	public float MovementMultiplier	= 25f;
	public float RunVerticalAxisCutoff = 15f;
	public bool SupportsWalking = false;
	public bool ForceWalk = false;
	//these are the base names for the animation controller
	//they're arbitrarily based on the wolf creature animation
	public static string gAnimWalkName = "walk";
	public static string gAnimSprintName = "run";
	public static string gAnimIdle1Name = "idleNormal";
	public static string gAnimIdle2Name = "idleLookAround";
	public static string gAnimAttack1Name = "standBite";
	public static string gAnimAttack2Name = "runBite";
	public static string gAnimWarnName = "howl";
	public static string gAnimTakeDamageName = "getHit";
	public static string gAnimDyingName = "death";
	public static string gAnimDeadName = "dead";
	public static string gAnimJumpName = "run";
	//TODO new clip here
	//animations used for basic states
	public AnimationClip AnimWalk;
	public AnimationClip AnimSprint;
	public AnimationClip AnimIdle1;
	public AnimationClip AnimIdle2;
	public AnimationClip AnimAttack1;
	public AnimationClip AnimAttack2;
	public AnimationClip AnimWarn;
	public AnimationClip AnimTakeDamage;
	public AnimationClip AnimDying;
	public AnimationClip AnimDead;
	public AnimationClip AnimJump;
	public bool UseOverrideController = true;
	//replaces previous enum movement modes
	//this will be defined by motile scripts
	public static FlagSet IdleAnimationFlags = null;
	public List <string> ActiveMovementModes = new List <string> ();
	public List <string> AvailableMovementModes = new List <string> ();

	[NObjectSync]
	public int BaseMovementMode {
		get {
			return mBaseMovementMode;
		}
		set {
			mBaseMovementMode = value;
		}
	}

	[NObjectSync]
	public int IdleAnimation {
		get {
			return mIdleAnimation;
		}
		set {
			if (value != mIdleAnimation) {
				ApplyIdleAnimation (value);
			}
		}
	}

	[NObjectSync]
	public float VerticalAxisMovement {
		get {
			return mVerticalAxisMovement;
		}
		set {
			mVerticalAxisMovement = value;
		}
	}

	public float YRotation {
		get { 
			return mYRotation;
		}
		set {
			mYRotation = value;
		}
	}

	[NObjectSync]
	public float HorizontalAxisMovement {
		get {
			return mHorizontalAxisMovement;
		}
		set {
			mHorizontalAxisMovement = value;
		}
	}

	[NObjectSync]
	public bool TakingDamage {
		get {
			return mTakingDamage;
		}
		set {
			mTakingDamage = value;
		}
	}

	[NObjectSync]
	public virtual bool Dead {
		get {
			return mDead;
		}
		set {
			mDead = value;
		}
	}

	[NObjectSync]
	public bool Warn {
		get {
			return mWarn;
		}
		set {
			mWarn = value;
		}
	}

	[NObjectSync]
	public bool Attack1 {
		get {
			return mAttack1;
		}
		set {
			mAttack1 = value;
		}
	}

	[NObjectSync]
	public bool Attack2 {
		get {
			return mAttack2;
		}
		set {
			mAttack2 = value;
		}
	}

	[NObjectSync]
	public bool Grounded {
		get {
			return mGrounded;
		}
		set {
			mGrounded = value;
		}
	}

	[NObjectSync]
	public bool Jump {
		get {
			return mJump;
		}
		set {
			mJump = value;
		}
	}

	[NObjectSync]
	public bool Paused {
		get {
			return mPaused;
		}
		set {
			if (value != mPaused) {
				SetPaused (value);
			}
		}
	}

	[NObjectSync]
	public bool Idling {
		get {
			return mIdling;
		}
		set {
			mIdling = value;
		}
	}

	public virtual void Start ()
	{
		if (animator.avatar == null) {
			mPaused = true;
		}

		animator.applyRootMotion = false;
		animator.updateMode = AnimatorUpdateMode.Normal;
		animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

		if (UseOverrideController) {
			//override clips with this creature's animation clips
			RuntimeAnimatorController controller = animator.runtimeAnimatorController;
			AnimatorOverrideController overrideController = new AnimatorOverrideController ();
			overrideController.runtimeAnimatorController = controller;
			overrideController [gAnimWarnName] = AnimWarn;
			overrideController [gAnimWalkName] = AnimWalk;
			overrideController [gAnimSprintName] = AnimSprint;
			overrideController [gAnimIdle1Name] = AnimIdle1;
			overrideController [gAnimIdle2Name] = AnimIdle2;
			overrideController [gAnimAttack1Name] = AnimAttack1;
			overrideController [gAnimAttack2Name] = AnimAttack2;
			overrideController [gAnimTakeDamageName] = AnimTakeDamage;
			overrideController [gAnimDyingName] = AnimDying;
			overrideController [gAnimDeadName] = AnimDead;
			overrideController [gAnimJumpName] = AnimJump;

			animator.runtimeAnimatorController = overrideController;
		}

		mSmoothVerticalAxisMovement = 0f;
		mSmoothHorizontalAxisMovement = 0f;
		VerticalAxisMovement = 0f;
		HorizontalAxisMovement = 0f;

		animator.SetFloat ("HorizontalAxisMovement", 0f);
		animator.SetFloat ("VerticalAxisMovement", 0f);
		animator.SetFloat ("MouseX", 0f);
		animator.SetBool ("Falling", false);
		animator.SetBool ("Grounded", true);
		animator.SetBool ("Jump", false);

		if (IdleAnimationFlags == null) {
			GameWorld.Get.FlagSetByName ("IdleAnimation", out IdleAnimationFlags);
		}
	}

	protected void SetPaused (bool Paused)
	{
		mPaused = Paused;
	}

	public virtual void FixedUpdate ()
	{
		if (mPaused) {
			//animator.SetFloat ("AxisY", 0.0f);
			//animator.SetFloat ("AxisX", 0.0f);
			//animator.SetFloat ("MouseX", 0.0f);
			//we're done
			return;
		}

		if (mDead) {
			animator.SetBool ("Dead", mDead);
			if (mDead) {
				animator.SetFloat ("HorizontalAxisMovement", 0f);
				animator.SetFloat ("VerticalAxisMovement", 0f);
				animator.SetFloat ("MouseX", 0f);
				animator.SetBool ("Grounded", true);
				animator.SetBool ("Jump", false);
				animator.SetBool ("Walking", false);
			}
			return;
		}

		if (mToggle) {
			mToggle = false;

			mSmoothVerticalAxisMovement = Mathf.Lerp (mSmoothVerticalAxisMovement, VerticalAxisMovement, Time.deltaTime * 15f);
			mSmoothHorizontalAxisMovement = Mathf.Lerp (mSmoothHorizontalAxisMovement, HorizontalAxisMovement, Time.deltaTime * 15f);

			bool setWalking = true;

			//animator.SetBool ("Idling", Idling);
			if (Attack1) {
				animator.SetBool ("Attack1", true);
				setWalking = false;
			} else if (Attack2) {
				animator.SetBool ("Attack2", true);
				setWalking = false;
			} else if (Warn) {
				animator.SetBool ("Warn", true);
				setWalking = false;
			}
			if (TakingDamage) {
				animator.SetBool ("TakingDamage", true);
				setWalking = false;
			}

			float verticalAxisMovement = mSmoothVerticalAxisMovement * MovementMultiplier;
			animator.SetFloat ("VerticalAxisMovement", verticalAxisMovement);
			if (setWalking) {
				if (SupportsWalking) {
					if (ForceWalk) {
						animator.SetBool ("Walking", true);
					} else {
						animator.SetBool ("Walking", (verticalAxisMovement < RunVerticalAxisCutoff));
					}
				}
			} else {
				animator.SetBool ("Walking", false);
			}
			//animator.SetFloat ("HorizontalAxisMovement", mSmoothHorizontalAxisMovement * MovementMultiplier);
			mYRotationDifference = mYRotationDifference * 0.5f;
			mYRotationDifference += ((mYRotationLastFrame - mYRotation) * MovementMultiplier);
			if (Mathf.Abs (mYRotationDifference) < 0.001f) {
				mYRotationDifference = 0f;
			}
			mYRotationLastFrame = mYRotation;
			animator.SetFloat ("MouseX", mYRotationDifference);
		} else {
			mToggle = true;

			if (Jump) {
				animator.SetBool ("Grounded", false);
				if (Grounded) {
					animator.SetBool ("Jump", false);
					Jump = false;
				}
			} else {
				animator.SetBool ("Grounded", Grounded);
			}
			if (Attack1) {
				animator.SetBool ("Attack1", false);
				Attack1 = false;
			}
			if (Attack2) {
				animator.SetBool ("Attack2", false);
				Attack2 = false;
			}
			if (Warn) {
				animator.SetBool ("Warn", false);
				Warn = false;
			}
			if (TakingDamage) {
				animator.SetBool ("TakingDamage", false);
				TakingDamage = false;
			}
		}
	}

	protected bool mToggle = false;

	protected virtual void ApplyIdleAnimation (int idleAnimation)
	{
		mIdleAnimation = idleAnimation;
		if (idleAnimation == 0) {
			//set everything to zero
			//Debug.Log ("Setting idle animation to zero / default");
			ActiveMovementModes.Clear ();
		} else {
			ActiveMovementModes.Clear ();
			int[] flags = FlagSet.GetFlagValues (idleAnimation);
			//Debug.Log ("Setting idle animation to " + idleAnimation.ToString ());
			for (int i = 0; i < flags.Length; i++) {
				string flagValue = IdleAnimationFlags.GetFlagName (1 << flags [i]);
				if (!string.IsNullOrEmpty (flagValue)) {
					//Debug.Log ("Got flag value " + flagValue + " for flag " + (1 << flags [i]).ToString ());
					ActiveMovementModes.Add (flagValue);
				}
			}
		}

		for (int i = 0; i < AvailableMovementModes.Count; i++) {
			if (ActiveMovementModes.Contains (AvailableMovementModes [i])) {
				//Debug.Log ("Setting animation " + AvailableMovementModes [i] + " to true");
				animator.SetBool (AvailableMovementModes [i], true);
			} else {
				//Debug.Log ("Setting animation " + AvailableMovementModes [i] + " to false");
				animator.SetBool (AvailableMovementModes [i], false);
			}
		}
	}

	protected int mBaseMovementMode;
	protected int mIdleAnimation;
	protected string mPreviousMovementMode;
	protected float mVerticalAxisMovement = 0f;
	protected float mHorizontalAxisMovement = 0f;
	protected bool mAttack1 = false;
	protected bool mAttack2 = false;
	protected bool mWarn = false;
	protected bool mGrounded = true;
	protected bool mJump = false;
	protected float mSmoothVerticalAxisMovement = 0.0f;
	protected float mSmoothHorizontalAxisMovement = 0.0f;
	protected float mYRotationDifference = 0.0f;
	protected float mYRotation;
	protected float mYRotationLastFrame;
	protected bool mPaused = false;
	protected bool mDead = false;
	protected bool mTakingDamage = false;
	protected bool mIdling = false;
	protected float mIdleBlend = 0f;
	protected float mIdleTarget = 0f;
}

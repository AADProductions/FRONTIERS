using UnityEngine;
using System;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI {
	public class EditorTransition : GUITransition
	{
		public override void Awake ( )
		{
			base.Awake ( );
			mAutoRetire = true;
			mTransitionSpeed = 0.25f;
		}
		
		public void Update ( )
		{
			if (mIsFinished)
			{
				return;
			}

			if (mProceed)
			{
				if (CheckIfFinished ( )) {
					transform.localScale = mGoal;
					return;
				}
				
				double transitionAmount = ((Frontiers.WorldClock.RealTime - mStartTime) / (mEndTime - mStartTime));
				mCurrent = Vector3.Lerp (mCurrent, mGoal, (float) transitionAmount);
				transform.localScale = mCurrent;
			}
		}
		
		public override void Retire ( )
		{
			GameObject.Destroy (this);
		}

		protected Vector3 mGoal;
		protected Vector3 mCurrent;
	}
}
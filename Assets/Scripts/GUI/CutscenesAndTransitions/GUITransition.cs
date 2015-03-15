using UnityEngine;
using System;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUITransition : MonoBehaviour
		{
				public string OnFinishedMessage = "OnFinishScaleUp";
				public string OnProceedMessage = string.Empty;

				public double TransitionSpeed {
						get {
								return mTransitionSpeed;
						}
						set {
								mTransitionSpeed = Mathf.Clamp01((float)value);//ugh
						}
				}

				public bool IsFinished {
						get {
								return mIsFinished;
						}
				}

				public virtual void Awake()
				{
			
				}

				public void Proceed()
				{
						mProceed = true;
						mStartTime = WorldClock.RealTime;
						mEndTime = WorldClock.RealTime + mTransitionSpeed;
						if (!string.IsNullOrEmpty(OnProceedMessage)) {	////Debug.Log ("Sending message " + OnProceedMessage + " to GUITransition object " + name);
								gameObject.SendMessage(OnProceedMessage, SendMessageOptions.DontRequireReceiver);
						}
				}

				public void Proceed(bool autoRetire)
				{
						mAutoRetire = autoRetire;
						if (mProceed) {
								return;
						}
						Proceed();
				}

				public void Proceed(GUITransitionCallBack callBack)
				{
						mCallBack = callBack;
						if (mProceed) {
								return;
						}
						Proceed();
				}

				public void Proceed(GUITransitionCallBack callBack, bool autoRetire)
				{
						mAutoRetire = autoRetire;
						if (mProceed) {
								return;
						}
						Proceed(callBack);
				}

				public virtual void Retire()
				{
						GameObject.Destroy(gameObject);
				}

				protected bool CheckIfFinished()
				{
						if (WorldClock.RealTime >= mEndTime) {
								Finish();
								return true;
						}
			
						return false;
				}

				protected void Finish()
				{
						if (mIsFinished) {
								return;
						}

						mIsFinished = true;

						if (mCallBack != null) {
								mCallBack(this);
						}
			
						if (!string.IsNullOrEmpty(OnFinishedMessage)) {
								gameObject.SendMessage(OnFinishedMessage, SendMessageOptions.DontRequireReceiver);
						}

						OnFinish();
			
						if (mAutoRetire) {
								Retire();
						}
				}

				protected virtual void OnFinish() {

				}

				protected GUITransitionCallBack mCallBack = null;
				protected bool mProceed = false;
				protected double mTransitionSpeed = 0.35f;
				protected bool mIsFinished = false;
				protected double mStartTime = 0f;
				protected double mEndTime = 0f;
				protected bool mAutoRetire = false;
		}
}
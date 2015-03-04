using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
	public class FadeOutLoadingScreen : MonoBehaviour
	{
		public UISprite FadeOutSprite;

		public void StartFadingOut ()
		{
			mStartFadingIn = false;
			//		//Debug.Log ("Fading out!");
			if (!mFadedOutOnce) {
				FadeOutSprite.alpha = 1.0f;
				FadeOutSprite.enabled = true;
				mStartFadingOut = true;
				mFadedOutOnce = true;
			}
		}

		public void StartFadingIn ()
		{
			mStartFadingIn = true;
			if (!mFadedOutOnce) {
				FadeOutSprite.alpha = 1.0f;
				FadeOutSprite.enabled = true;
			}
		}

		void Update ()
		{
			if (mStartFadingIn) {
				FadeOutSprite.enabled = true;
				FadeOutSprite.alpha = 1.0f;
			} else if (mStartFadingOut) {
				FadeOutSprite.enabled = true;
				FadeOutSprite.alpha = Mathf.Lerp (FadeOutSprite.alpha, 0.0f, 0.05f);
				if (FadeOutSprite.alpha <= 0.001f) {
					mStartFadingOut = false;
				}
			} else {
				FadeOutSprite.enabled = false;
			}
		}

		protected bool mStartFadingIn	= false;
		protected bool mStartFadingOut	= false;
		protected bool mFadedOutOnce	= false;
	}
}
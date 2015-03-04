using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
	public class GUICircularBrowserFocus : MonoBehaviour
	{
		public GUICircularBrowserObject BrowserObject;

		public void OnHover ()
		{
			mIsOver = !mIsOver;
			
			if (mIsOver) {
				mTryToFocus = true;
			}
		}

		public virtual void OnClick ()
		{
			if (BrowserObject.ParentBrowser != null) {
				BrowserObject.ParentBrowser.ClickOn (BrowserObject.Index);
			}
		}

		public void Update ()
		{
			if (mTryToFocus && mIsOver) {
				if (BrowserObject.ParentBrowser.FocusOn (BrowserObject.Index)) {
					mTryToFocus = false;
				}
			} else {
				mTryToFocus = false;
			}
		}

		protected bool mIsOver = false;
		protected bool mTryToFocus = false;
	}
}
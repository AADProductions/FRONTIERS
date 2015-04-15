using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
		//base class for gui objects
		//will eventually be used to save / load NGUI setups for modding
		public class GUIObject : MonoBehaviour
		{
				public string PrefabName = "GUIObject";
				public string InitArgument = string.Empty;

				public bool IsDestroyed { get { return mIsDestroyed; } }

				public virtual Camera NGUICamera { get { return mNGUICamera; } set { mNGUICamera = value; } }

				public virtual void Awake()
				{
						mRefreshRequest = Refresh;
				}

				public virtual void Initialize(string argument)
				{
						InitArgument = argument;
						mInitialized = true;
				}

				public void Refresh()
				{
						if (!mIsDestroyed) {
								OnRefresh();
						}
						mIsRefreshing = false;
				}

				public virtual void RefreshRequest()
				{
						if (mIsRefreshing || mIsDestroyed)
								return;

						mIsRefreshing = true;
						GUIManager.RefreshObject(this);
				}

				protected virtual void OnRefresh()
				{

				}

				public virtual void OnDestroy()
				{
						mIsDestroyed = true;
				}

				protected bool mIsRefreshing = false;
				protected bool mIsDestroyed = false;
				protected bool mInitialized = false;
				protected System.Action mRefreshRequest = null;
				protected Camera mNGUICamera;
		}
}

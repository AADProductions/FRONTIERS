using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{		//TODO figure out how to get lock cursor working correctly on linux
		public class GUICursor : MonoBehaviour
		{
				public Texture2D CursorTexture;
				public float TargetOpacity = 1.0f;
				public float FadeTime = 1.0f;
				public float Alpha = 0.0f;

				public void Awake()
				{
						Screen.showCursor = false;
				}

				public void Update()
				{
						Screen.showCursor = GUIManager.ShowCursor;			
						if (!Screen.showCursor) {
								LockCursor();
						} else {
								ReleaseCursor();
						}
				}

				protected void LockCursor()
				{
						if (Application.platform != RuntimePlatform.LinuxPlayer) {
								Screen.lockCursor = true;
						}
				}

				protected void ReleaseCursor()
				{
						if (Application.platform != RuntimePlatform.LinuxPlayer) {
								Screen.lockCursor = false;
						}
				}

				public static bool gUseMouseLock = true;
		}
}
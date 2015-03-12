using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
		public class GUILogInterface : PrimaryInterface
		{
				public static GUILogInterface Get;
				public UIButtonMessage CloseButton;
				public GUITabs Tabs;

				public override void GetActiveInterfaceObjects(System.Collections.Generic.List<Widget> currentObjects)
				{
						Tabs.GetActiveInterfaceObjects(currentObjects);
				}

				public override void WakeUp()
				{
						Get = this;
						Tabs.NGUICamera = NGUICamera;
				}

				public void OnClickCloseButton()
				{
						ActionCancel(WorldClock.RealTime);
				}

				public override bool Minimize()
				{
						if (base.Minimize()) {
								CloseButton.gameObject.SetActive(false);
								Tabs.Hide();
								return true;
						}
						return false;
				}

				public override bool Maximize()
				{
						if (base.Maximize()) {
								CloseButton.gameObject.SetActive(true);
								Tabs.Show();
						}
						return false;
				}
		}
}
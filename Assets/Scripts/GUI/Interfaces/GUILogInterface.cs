using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUILogInterface : PrimaryInterface
		{
				public static GUILogInterface Get;
				public UIButtonMessage CloseButton;
				public GUITabs Tabs;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects)
				{
						Tabs.GetActiveInterfaceObjects(currentObjects);
						GUIDetailsPage.Get.GetActiveInterfaceObjects(currentObjects);
				}

				public override Widget FirstInterfaceObject {
						get {
								Widget w = new Widget();
								w.BoxCollider = Tabs.Buttons[0].gameObject.GetComponent <BoxCollider>();
								w.SearchCamera = NGUICamera;
								return w;
						}
				}

				public override void WakeUp()
				{
						base.WakeUp();

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
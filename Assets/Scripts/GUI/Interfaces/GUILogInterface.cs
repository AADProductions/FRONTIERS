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
				public Vector3 VRModeOffsetFocusLog = new Vector3(400f, 0f, 0f);
				public Vector3 VRModeOffsetFocusDetails = new Vector3(400f, 0f, 0f);
				public bool VRFocusDetailsPage = false;

				public override bool ShowQuickslots {
						get {
								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
								#else
								if (VRManager.VRMode) {
								#endif
										//the log takes up too much room in vr mode
										return !Maximized;
								}
								return base.ShowQuickslots;
						}
						set {
								base.ShowQuickslots = value;
						}
				}

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUIEditorID; }

						Tabs.GetActiveInterfaceObjects(currentObjects, flag);
						GUIDetailsPage.Get.GetActiveInterfaceObjects(currentObjects, flag);
				}

				public override Widget FirstInterfaceObject {
						get {
								Widget w = base.FirstInterfaceObject;
								w.BoxCollider = Tabs.Buttons[0].gameObject.GetComponent <BoxCollider>();
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
								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
								#else
								if (VRManager.VRMode) {
								#endif
										transform.localPosition = VRModeOffsetFocusLog;
								} else {
										transform.localPosition = Vector3.zero;
								}
								CloseButton.gameObject.SetActive(true);
								Tabs.Show();
						}
						return false;
				}

				public override void Update()
				{
						base.Update();
						if (!Maximized) {
								return;
						}

						VRFocusDetailsPage = GUIDetailsPage.Get.Visible;

						#if UNITY_EDITOR
						if (Input.GetKeyDown(KeyCode.K)) {
								VRFocusDetailsPage = !VRFocusDetailsPage;
								GUIDetailsPage.Get.Visible = VRFocusDetailsPage;
						}
						#endif

						#if UNITY_EDITOR
						if (VRManager.VRMode | VRManager.VRTestingModeEnabled) {
								#else
			if (VRManager.VRMode) {
								#endif
								if (VRFocusDetailsPage) {
										transform.localPosition = Vector3.Lerp(transform.localPosition, VRModeOffsetFocusDetails, 0.25f);
								} else {
										transform.localPosition = Vector3.Lerp(transform.localPosition, VRModeOffsetFocusLog, 0.25f);
								}
						}
				}
		}
}
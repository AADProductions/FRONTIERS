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
				public UIAnchor LogInterfaceAnchor;
				public bool VRFocusDetailsPage = false;

				public override bool ShowQuickslots {
						get {
								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingMode) {
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
						if (GUIDetailsPage.Get.Visible) {
								GUIDetailsPage.Get.GetActiveInterfaceObjects(currentObjects, GUIDetailsPage.Get.GUIEditorID);
						}
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
								LogInterfaceAnchor.enabled = false;
								CloseButton.gameObject.SetActive(false);
								Tabs.Hide();
								return true;
						}
						return false;
				}

				public override bool Maximize()
				{
						if (base.Maximize()) {
								LogInterfaceAnchor.enabled = true;

								#if UNITY_EDITOR
								if (VRManager.VRMode | VRManager.VRTestingMode) {
								#else
								if (VRManager.VRMode) {
								#endif
										VRManager.Get.ResetInterfacePosition();
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

						#if UNITY_EDITOR
						if (VRManager.VRMode | VRManager.VRTestingMode) {
						#else
						if (VRManager.VRMode) {
						#endif
								if (GUICursor.Get.TryToFollowCurrentWidget (GUIEditorID) || !GUIDetailsPage.Get.Visible) {
										VRFocusDetailsPage = false;
								} else {
										VRFocusDetailsPage = GUICursor.Get.TryToFollowCurrentWidget (GUIDetailsPage.Get.GUIEditorID);
								}
								transform.localPosition = Vector3.Lerp(transform.localPosition, VRFocusDetailsPage ? VRModeOffsetFocusDetails : VRModeOffsetFocusLog, 0.25f);
						}
				}
		}
}
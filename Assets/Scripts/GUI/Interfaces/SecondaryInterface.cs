using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.GUI
{
		public class SecondaryInterface : FrontiersInterface, IGUITabOwner, IGUITabPageChild
		{
				public override InterfaceType Type {
						get {
								return InterfaceType.Secondary;
						}
				}

				public List <UIPanel> Panels = new List <UIPanel>();

				public Action OnShow { get; set; }

				public Action OnHide { get; set; }

				public bool Visible { get; set; }

				public bool CanShowTab(string tabName, GUITabs tabs)
				{
						return true;
				}

				public override void Start()
				{
						base.Start();

						//secondary interfaces always let primary toggle
						if (!FilterPrimaryToggleActions) {
								FilterExceptions |= InterfaceActionType.FlagsTogglePrimary;
						} else {
								Filter |= InterfaceActionType.FlagsTogglePrimary;
						}
						Subscribe(InterfaceActionType.ToggleInterface, ToggleInterface);
						UserActions.Subscribe(UserActionType.ActionCancel, ActionCancel);
				}

				public virtual void Show()
				{
						for (int i = 0; i < Panels.Count; i++) {
								Panels[i].enabled = true;
						}
						Visible = true;
						OnShow.SafeInvoke();
						#if UNITY_EDITOR
						if ((VRManager.VRModeEnabled | VRManager.VRTestingModeEnabled)) {
								Debug.Log ("Pushing widget in editor");
								GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						#else
						if (VRManager.VRModeEnabled) {
								GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						#endif
				}

				public virtual void Hide()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return;
						}

						for (int i = 0; i < Panels.Count; i++) {
								if (Panels[i] == null) {
										Debug.Log ("WTF panel waws null");
								}
								Panels[i].enabled = false;
						}		
						Visible = false;
						GUIManager.Get.ReleaseFocus(this);
						OnHide.SafeInvoke();
				}

				public virtual bool ToggleInterface(double timeStamp)
				{
						if (mDestroyed)
								return true;

						return ActionCancel(timeStamp);
				}

				public virtual bool ActionCancel(double timeStamp)
				{
						if (mDestroyed)
								return true;

						Finish();
						return true;
				}

				public override void OnDestroy()
				{
						Hide();
						base.OnDestroy();
						UserActions.UnsubscribeAll();
						Finish();
				}

				public void Finish()
				{
						if (!mFinished) {
								OnFinish();
								mFinished = true;
						}
				}

				protected virtual void OnFinish()
				{
						GUIManager.Get.ReleaseFocus(this);
						if (ScaleDownOnFinish) {
								GUIManager.ScaleDownEditor(this.gameObject).Proceed(true);
						}
				}

				public void OnFinishScaleUp () {
						Debug.Log("On finish scaling up");
						#if UNITY_EDITOR
						if ((VRManager.VRModeEnabled | VRManager.VRTestingModeEnabled)) {
								GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						#else
						if (VRManager.VRModeEnabled) {
								GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						#endif
				}

				public bool	FilterPrimaryToggleActions = false;
				public bool ScaleDownOnFinish = false;
		}
}

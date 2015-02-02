using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;
using ExtensionMethods;

namespace Frontiers.GUI
{
		public class FrontiersInterface : InterfaceActionFilter
		{
				public UICamera CameraInput { get; set; }

				public Camera NGUICamera {
						get {
								Camera nguiCamera = null;
								switch (Type) {
										case InterfaceType.Base:
												nguiCamera = GUIManager.Get.NGUIBaseCamera.camera;
												break;
					
										case InterfaceType.Primary:
												nguiCamera = GUIManager.Get.NGUIPrimaryCamera.camera;
												break;
					
										case InterfaceType.Secondary:
												nguiCamera = GUIManager.Get.NGUISecondaryCamera.camera;
												break;
					
										default:
												break;
								}
								return nguiCamera;
						}
				}

				public UserActionReceiver UserActions;
				public UIAnchor.Side AnchorSide = UIAnchor.Side.Center;

				public bool IsDestroyed {
						get {
								return mDestroyed;
						}
				}

				public bool IsFinished {
						get {
								return mFinished;
						}
				}

				public override bool LoseFocus()
				{
						if (mDestroyed) {
								return true;
						}

						if (base.LoseFocus()) {
								UserActions.LoseFocus();
								DisableInput();
								OnLoseFocus.SafeInvoke();
								if (DeactivateOnLoseFocus) {
										GUIManager.Get.Deactivate(this);
								}
								return true;
						}

						return false;
				}

				public override bool GainFocus()
				{
						if (base.GainFocus()) {
								UserActions.GainFocus();
								EnableInput();
								OnGainFocus.SafeInvoke();
								return true;
						}
						return false;
				}

				public virtual void DisableInput()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycastIgnore);
				}

				public virtual void EnableInput()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycast);
				}

				public Action OnLoseFocus { get; set; }

				public Action OnGainFocus { get; set; }

				public string Name = "Generic";

				public virtual InterfaceType Type {
						get {
								return InterfaceType.Base;
						}
				}

				public bool GetPlayerAttention = false;
				public bool DeactivateOnLoseFocus = false;
				public List <UIPanel> MasterPanels = new List <UIPanel>();
				public List <UIAnchor> MasterAnchors = new List <UIAnchor>();
				public int MasterDepth = 0;

				public virtual void Start()
				{
						UserActions = gameObject.GetOrAdd <UserActionReceiver>();
				}

				public virtual void SetDepth(int masterDepth)
				{
						MasterDepth = masterDepth;
						Vector3 depthPosition = transform.localPosition;
						depthPosition.z = MasterDepth * Globals.SecondaryInterfaceDepthMultiplier;
						transform.localPosition = depthPosition;
						/*
						foreach (UIPanel panel in MasterPanels) {
							Vector3 depthPosition = panel.transform.localPosition;
							depthPosition.z = (MasterDepth * Globals.SecondaryInterfaceDepthMultiplier);
							if (Type == InterfaceType.Secondary) {
								depthPosition.z += Globals.SecondaryInterfaceDepthBase;
							}
							panel.transform.localPosition = depthPosition;
						}
						*/
				}

				public virtual void OnDestroy()
				{
						mDestroyed = true;
				}

				protected bool mDestroyed = false;
				protected bool mFinished = false;
		}
}
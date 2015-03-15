using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;
using ExtensionMethods;

namespace Frontiers.GUI
{
		public class FrontiersInterface : InterfaceActionFilter, IFrontiersInterface
		{
				public UICamera CameraInput { get; set; }

				public Camera NGUICamera {
						get {
								if (mNguiCamera == null) {
										switch (Type) {
												case InterfaceType.Base:
														mNguiCamera = GUIManager.Get.NGUIBaseCamera.camera;
														break;
					
												case InterfaceType.Primary:
														mNguiCamera = GUIManager.Get.NGUIPrimaryCamera.camera;
														break;
					
												case InterfaceType.Secondary:
														mNguiCamera = GUIManager.Get.NGUISecondaryCamera.camera;
														break;
					
												default:
														break;
										}
								}
								return mNguiCamera;
						}
						set {
								mNguiCamera = value;
						}
				}

				public UserActionReceiver UserActions;
				public UIAnchor.Side AnchorSide = UIAnchor.Side.Center;
				protected Camera mNguiCamera;

				public static void GetActiveInterfaceObjectsInTransform(Transform startTransform, Camera searchCamera, List<Widget> currentObjects)
				{
						gGetColliders.Clear();
						startTransform.GetComponentsInChildren <BoxCollider>(gGetColliders);
						FrontiersInterface.Widget w = new FrontiersInterface.Widget();
						IGUIBrowserObject bo = null;
						w.SearchCamera = searchCamera;
						for (int j = 0; j < gGetColliders.Count; j++) {
								w.BoxCollider = gGetColliders[j];
								if (w.BoxCollider.gameObject.layer == Globals.LayerNumGUIRaycastIgnore || !w.BoxCollider.gameObject.activeSelf) {
										continue;
								}
								w.BrowserObject = null;
								if (w.BoxCollider.CompareTag(Globals.TagBrowserObject)) {
										w.BrowserObject = (IGUIBrowserObject)w.BoxCollider.GetComponent(typeof(IGUIBrowserObject));
										if (w.BrowserObject == null) {
												w.BrowserObject = (IGUIBrowserObject)w.BoxCollider.transform.parent.GetComponent(typeof(IGUIBrowserObject));
										}
								} else if (w.BoxCollider.CompareTag(Globals.TagIgnoreTab)) {
										continue;
								} else if (w.BoxCollider.CompareTag(Globals.TagActiveObject)) {
										//never use scrollbars
										if (w.BoxCollider.gameObject.HasComponent <UIScrollBar>() || w.BoxCollider.transform.parent.gameObject.HasComponent<UIScrollBar>()) {
												continue;
										}
								}
								currentObjects.Add(w);
						}
				}

				public virtual void GetActiveInterfaceObjects(List<Widget> currentObjects)
				{
						//use the default method
						GetActiveInterfaceObjectsInTransform(transform, NGUICamera, currentObjects);
				}

				public virtual FrontiersInterface.Widget FirstInterfaceObject {
						get {
								return new Widget();
						}
				}

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
				public bool SupportsControllerSearch = true;
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

				[Serializable]
				public struct Widget
				{
						public Camera SearchCamera;
						public Collider BoxCollider;
						public IGUIBrowserObject BrowserObject;

						public bool IsEmpty {
								get {
										return BoxCollider == null || SearchCamera == null;
								}
								set {
										BoxCollider = null;
										SearchCamera = null;
								}
						}

						public bool IsBrowserObject {
								get {
										return BrowserObject != null;
								}
						}
				}

				protected static List <BoxCollider> gGetColliders = new List<BoxCollider>();
				protected IBrowser mBrowser;
		}

		public interface IFrontiersInterface
		{
				void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects);

				FrontiersInterface.Widget FirstInterfaceObject { get; }
		}
}
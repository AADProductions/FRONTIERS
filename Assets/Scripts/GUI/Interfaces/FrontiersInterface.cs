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
				public int GUIEditorID {
						get {
								if (mGUIEditorID < 0) {
										mGUIEditorID = GUIManager.GetNextGUIID();
								}
								return mGUIEditorID;
						}
				}

				public UICamera CameraInput { get; set; }

				public Camera NGUICamera {
						get {
								if (mNguiCamera == null) {
										switch (Type) {
												case InterfaceType.Base:
														mNguiCamera = GUIManager.Get.NGUIBaseCamera.GetComponent<Camera>();
														break;
					
												case InterfaceType.Primary:
														mNguiCamera = GUIManager.Get.NGUIPrimaryCamera.GetComponent<Camera>();
														break;
					
												case InterfaceType.Secondary:
														mNguiCamera = GUIManager.Get.NGUISecondaryCamera.GetComponent<Camera>();
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
				protected int mGUIEditorID = -1;
				protected bool mVRSettingsOverride = false;

				public virtual bool VRSettingsOverride {
						get {
								return mVRSettingsOverride;
						} set {
								mVRSettingsOverride = value;
						}
				}

				public virtual bool CustomVRSettings {
						get {
								return mCustomVRSettings;
						}
						set {
								mCustomVRSettings = value;
						}
				}

				public virtual bool CursorLock {
						get {
								return mCursorLock;
						}
						set {
								mCursorLock = value;
						}
				}

				public virtual bool AxisLock {
						get {
								return mAxisLock;
						}
						set {
								mAxisLock = value;
						}
				}

				public virtual Vector3 LockOffset {
						get {
								return mLockOffset;
						}
						set {
								mLockOffset = value;
						}
				}

				public virtual float QuadZOffset {
						get {
								if (mQuadZOffset < 0) {
										return VRManager.Get.DefaultRenderQuadOffset.z;
								}
								return mQuadZOffset;
						}
				}

				public Vector3 mLockOffset = Vector3.zero;
				public float mQuadZOffset = -1f;
				public bool mAxisLock = false;
				public bool mCursorLock = false;
				public bool mCustomVRSettings = false;

				public static void GetActiveInterfaceObjectsInTransform(Transform startTransform, Camera searchCamera, List<Widget> currentObjects, int flag)
				{
						gGetColliders.Clear();
						startTransform.GetComponentsInChildren <Collider>(gGetColliders);
						FrontiersInterface.Widget w = new FrontiersInterface.Widget(flag);
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

				public static bool IsClipPanelPositionVisible (Vector3 worldPosition, UIPanel clipPanel, float normalizedRange, out Bounds browserBounds) {
						//this is a crapton of vector3s getting allocated all at once
						//but we only use it once in a while and it helps me keep things straight
						//get the bounds of the browser's clipping area
						Vector3 browserPosition = clipPanel.transform.position;
						Vector4 browserClipRange = clipPanel.clipRange;
						Vector3 browserScale = clipPanel.transform.lossyScale;
						browserPosition.x = browserPosition.x + (browserClipRange.x * browserScale.x);
						browserPosition.y = browserPosition.y + (browserClipRange.y * browserScale.y);
						//make the clip range just slightly smaller than the real range
						Vector3 browserSize = new Vector3(browserClipRange.z * browserScale.x, (browserClipRange.w * normalizedRange) * browserScale.y, 100f);
						browserBounds = new Bounds(browserPosition, browserSize);
						Vector3 browserPanelLocalPosition = clipPanel.transform.localPosition;
						//move the object to the center of the clipping bounds
						return browserBounds.Contains(worldPosition);
				}

				public static Bounds FocusClipPanelOnPosition (Vector3 worldPosition, UIPanel clipPanel, UIDraggablePanel dragPanel, float normalizedRange, float minBrowserPosition) {
						Bounds browserBounds;
						if (!IsClipPanelPositionVisible(worldPosition, clipPanel, normalizedRange, out browserBounds)) {
								Vector3 targetPosition = browserBounds.center;
								targetPosition.y = targetPosition.y + (browserBounds.size.y / 2);
								targetPosition = clipPanel.transform.InverseTransformPoint(browserBounds.center);
								//move the background by the amount of space it would take to reach the object
								Vector3 relativeDifference = targetPosition - clipPanel.transform.InverseTransformPoint(worldPosition);
								relativeDifference.z = 0f;
								relativeDifference.x = 0f;//should we really do this...?
								dragPanel.MoveRelative(relativeDifference, true, minBrowserPosition);
								dragPanel.UpdateScrollbars(true, true);
								//Debug.Log("world position " + worldPosition.ToString() + " was NOT visible in clip panel " + clipPanel.name + ", moving " + relativeDifference.y.ToString());
						}
						return browserBounds;
				}

				public virtual void GetActiveInterfaceObjects(List<Widget> currentObjects, int flag)
				{
						if (flag < 0) { flag = GUIEditorID; }
						//use the default method
						GetActiveInterfaceObjectsInTransform(transform, NGUICamera, currentObjects, flag);
				}

				public virtual FrontiersInterface.Widget FirstInterfaceObject {
						get {
								Widget w = new Widget(GUIEditorID);
								w.SearchCamera = NGUICamera;
								return w;
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
						if ((VRManager.VRMode | VRManager.VRTestingMode)) {
								GUICursor.Get.SelectWidget(FirstInterfaceObject);
						}
						#else
						if (VRManager.VRMode) {
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
						public Widget (int flag) {
								Flag = flag;
								SearchCamera = null;
								BoxCollider = null;
								BrowserObject = null;
						}

						public Camera SearchCamera;
						public Collider BoxCollider;
						public IGUIBrowserObject BrowserObject;
						public int Flag;

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

						public bool Equals(Widget other) {
								if (IsEmpty) {
										return false;
								}
								if (other.IsEmpty) {
										return false;
								}
								return BoxCollider == other.BoxCollider;
						}
				}

				protected static List <Collider> gGetColliders = new List<Collider>();
				protected IBrowser mBrowser;
		}

		public interface IFrontiersInterface
		{
				int GUIEditorID { get; }

				bool VRSettingsOverride { get; set; }

				bool CustomVRSettings { get; }

				Vector3 LockOffset { get; }

				bool CursorLock { get; }

				bool AxisLock { get; }

				Camera NGUICamera { get; set; }

				void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag);

				FrontiersInterface.Widget FirstInterfaceObject { get; }
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class CutsceneInterface : MonoBehaviour, IFrontiersInterface
		{
				public Camera CutsceneInterfaceCamera;

				public bool VRSettingsOverride { get; set; }

				public int GUIEditorID {
						get {
								if (mGUIEditorID < 0) {
										mGUIEditorID = GUIManager.GetNextGUIID();
								}
								return mGUIEditorID;
						}
				}

				public Camera NGUICamera
				{
						get {
								return CutsceneInterfaceCamera;
						}
						set {
								return;
						}
				}
				//needed for vr dodads
				public UIAnchor BottomAnchor;

				public virtual void OnCutsceneStart()
				{
						Cutscene.CurrentCutscene.InterfaceList.Add(this);
				}

				public virtual bool CustomVRSettings {
						get {
								return false;
						}
				}

				public virtual bool CursorLock {
						get {
								return false;
						}
				}

				public virtual bool AxisLock {
						get {
								return false;
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

				public virtual void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects, int flag)
				{
						//use the default method
						FrontiersInterface.GetActiveInterfaceObjectsInTransform(transform, NGUICamera, currentObjects, flag);
				}

				public virtual FrontiersInterface.Widget FirstInterfaceObject {
						get {
								FrontiersInterface.Widget w = new FrontiersInterface.Widget(GUIEditorID);
								return w;
						}
				}

				public virtual void FixedUpdate()
				{
						#if UNITY_EDITOR
						if ((VRManager.VRMode | VRManager.VRTestingMode)) {
								#else
						if (VRManager.VRMode) {
								#endif
								NGUICamera.targetTexture = VRManager.Get.TargetRenderTexture;
						} else {
								NGUICamera.targetTexture = null;
						}
				}

				protected int mGUIEditorID = -1;
				protected float mQuadZOffset = -1f;
				protected Vector3 mLockOffset = Vector3.zero;
		}
}
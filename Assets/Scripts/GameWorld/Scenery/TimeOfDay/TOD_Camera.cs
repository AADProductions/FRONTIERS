using UnityEngine;

/// Camera class.
///
/// Component of the main camera of the scene to move and scale the sky dome.

[ExecuteInEditMode]
//[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Main Script")]
public class TOD_Camera : MonoBehaviour
{
		/// Sky dome reference inspector variable.
		/// Has to be manually set to the sky dome instance.
		public TOD_Sky sky;
		public Camera cachedCamera;
		public Vector3 size;
		public Vector3 pos;
		/// Inspector variable to automatically move the sky dome to the camera position in OnPreCull().
		public bool DomePosToCamera = true;
		/// Inspector variable to automatically scale the sky dome to the camera far clip plane in OnPreCull().
		public bool DomeScaleToFarClip = false;
		/// Inspector variable to adjust the sky dome scale factor relative to the camera far clip plane.
		public float DomeScaleFactor = 0.95f;

		public void Awake()
		{
				cachedCamera = GetComponent<Camera>();
		}
		#if UNITY_EDITOR
		protected void Update()
		{
				DomeScaleFactor = Mathf.Clamp(DomeScaleFactor, 0.01f, 1.0f);
		}
		#endif
		protected void OnPreCull()
		{
				if (!sky)
						return;

				if (DomeScaleToFarClip) {
						size.x = DomeScaleFactor * cachedCamera.farClipPlane;
						size.y = size.x;
						size.z = size.x;
						#if UNITY_EDITOR
						if (sky.transform.localScale != size)
			            #endif
			            {
								sky.transform.localScale = size;
						}
				}
				if (DomePosToCamera) {
						pos = cachedCamera.transform.position;
						#if UNITY_EDITOR
						if (sky.transform.position != pos)
			            #endif
			            {
								sky.transform.position = pos;
						}
				}
		}
}

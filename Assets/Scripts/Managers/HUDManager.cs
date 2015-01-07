using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;


namespace Frontiers.GUI
{
	public class HUDManager : Manager
	{
		public GameObject NGUIHUDPrefab;
		public GameObject NGUIHudLabelPrefab;
		public GameObject NGUIHudProgressBarPrefab;
		public GameObject NGUIHudIconPrefab;
		public GameObject WorldDestinationHUDPrefabObject;
		public GameObject WorldRouteMarkerHUDPrefabObject;
		public GameObject HUDTrash;
		public GameObject HUDEditorAnchor;
		public Camera HUDRootCamera;
		public static float gMaxHUDDistance = 25.0f;
		public static HUDManager Get;

		public void Update()
		{
			if (Cutscene.IsActive) {
				HUDRootCamera.enabled = false;
			} else {
				HUDRootCamera.enabled = true;
			}
		}

		public static bool DisableHUD {
			get {
				return Get.HUDRootCamera.enabled;
			}
			set {
				Get.HUDRootCamera.enabled = false;
			}
		}

		public override void Awake()
		{
			base.Awake();
			Get = this;
		}

		public void LateUpdate()
		{
			while (mObjectsToDestroy.Count > 0) {
				GameObject objectToDestroy = mObjectsToDestroy.Dequeue();
				GameObject.Destroy(objectToDestroy);
			}
		}

		public NGUIHUD CreateWorldItemHud(Transform target, Vector3 offset)
		{
			GameObject newWorldItemHUDGameObject = NGUITools.AddChild(HUDEditorAnchor, NGUIHUDPrefab);
			NGUIHUD newWorldItemHud = newWorldItemHUDGameObject.GetComponent <NGUIHUD>();
			newWorldItemHud.Initialize(target, offset);
			return newWorldItemHud;
		}

		public void RetireWorldItemHUD(NGUIHUD retiredHUD)
		{
			retiredHUD.gameObject.SetActive(false);
			retiredHUD.transform.localPosition = new Vector3(-20000f, 0f, 0f);
			mObjectsToDestroy.Enqueue(retiredHUD.gameObject);
		}

		protected List <WIHud> mWorldItemHUDs = new List<WIHud>();
		protected static Queue <GameObject> mObjectsToDestroy = new Queue <GameObject>();
	}
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Frontiers.World
{
	public class RiverAvatar : MonoBehaviour
	{
		public WorldChunk ParentChunk;
		public Rigidbody rigidBody;
		public River Props;
		public Spline MasterSpline;
		public SplineMesh MasterSplineMesh;
		public BoxCollider FlowCollider;
		public BoxCollider BottomCollider;
		public BoxCollider WaterTriggerCollider;
		public MeshCollider StaticTriggerCollider;
		public MeshCollider StaticBottomCollider;
		public Renderer WaterRenderer;
		public MeshFilter Mesh;
		public WaterAnimScrolling WaterAnimation;
		public WaterSubmergeObjects WaterSubmerge;
		public Vector3 FlowForce;
		public List <IItemOfInterest> SubmergedItems = new List <IItemOfInterest> ();
		public Transform tr;

		public float TargetWaterLevel {
			get {
				return Props.TargetWaterLevel;
			}
			set {
				Props.TargetWaterLevel = value;
			}
		}

		public void Start ()
		{
			tr = transform;
			WaterSubmerge.OnItemOfInterestEnterWater += OnItemOfInterestEnterWater;
			WaterSubmerge.OnItemOfInterestExitWater += OnItemOfInterestExitWater;
			mSubmergedUpdate = UnityEngine.Random.Range (0, 30);
			mColorUpdate = UnityEngine.Random.Range (0, 60);
			mColliderPositionUpdate = UnityEngine.Random.Range (0, 10);
		}

		public void RefreshProps ()
		{
			name = Props.Name;
			if (Application.isPlaying) {
				if (Props.DynamicMode) {
					RefreshPropsDynamic ();
				} else {
					RefreshPropsStatic ();
				}
			} else {
				RefreshPropsDynamic ();
			}
		}

		public void FixedUpdate ()
		{
			if (!GameManager.Is (FGameState.InGame) || !ParentChunk.Is (ChunkMode.Primary | ChunkMode.Adjascent | ChunkMode.Immediate) || !WaterRenderer.isVisible) {
				return;
			}

			if (Props.DynamicMode) {
				FixedUpdateDynamic ();
			} else {
				FixedUpdateStatic ();
			}
		}

		#region static avatar
		public void RefreshFlowColliderStatic () {
			rigidBody = gameObject.GetOrAdd <Rigidbody> ();
			rigidBody.isKinematic = true;

			GameObject bottomColliderObject = gameObject.FindOrCreateChild ("BottomCollider").gameObject;
			bottomColliderObject.layer = Globals.LayerNumFluidTerrain;
			StaticBottomCollider = bottomColliderObject.GetOrAdd <MeshCollider> ();
			StaticBottomCollider.sharedMesh = Mesh.sharedMesh;
			StaticBottomCollider.isTrigger = false;

			GameObject triggerColliderObject = gameObject.FindOrCreateChild ("FlowCollider").gameObject;
			triggerColliderObject.layer = Globals.LayerNumFluidTerrain;
			StaticTriggerCollider = triggerColliderObject.GetOrAdd <MeshCollider> ();
			StaticTriggerCollider.sharedMesh = Mesh.sharedMesh;
			StaticTriggerCollider.isTrigger = true;
		}

		public void FixedUpdateStatic ()
		{
			if (GameManager.Is (FGameState.InGame)) {

				mColorUpdate++;
				mSubmergedUpdate++;
				mAudioUpdate++;

				if (mAudioUpdate > 30) {
					mAudioUpdate = 0;
					UpdateAudioStatic ();
				}

				if (mSubmergedUpdate > 30) {
					mSubmergedUpdate = 0;
					for (int i = SubmergedItems.LastIndex (); i >= 0; i--) {
						IItemOfInterest ioi = SubmergedItems [i];
						if (ioi == null) {
							SubmergedItems.RemoveAt (i);
						} else {
							if (ioi.IOIType == ItemOfInterestType.Player) {
								//kludge - make the player local props accessible!
								Player.Local.FPSController.AddForce (FlowForce);
							}
						}
					}
				}

				if (mColorUpdate > 60) {
					mColorUpdate = 0;
					WaterAnimation.WaterMaterial.SetColor ("_FogColor", TOD_Sky.GlobalFogColor);
					WaterAnimation.WaterMaterial.SetColor ("_FoamColor", Colors.Alpha (Color.Lerp (Color.white, RenderSettings.fogColor, 0.25f), 0.25f));
					WaterAnimation.WaterMaterial.SetColor ("_CrestColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Brighten (RenderSettings.fogColor), 0.15f), 0.45f));
					WaterAnimation.WaterMaterial.SetColor ("_WaveColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Desaturate (RenderSettings.fogColor), 0.15f), 0.55f));
				}
			}
		}

		protected void RefreshPropsStatic ()
		{
			if (Props.Vertices != null) {
				Mesh m = Mesh.sharedMesh;
				if (m == null) {
					Vector3[] vertices = new Vector3 [Props.Vertices.Length];
					Vector2[] uvs = new Vector2 [Props.UVs.Length];
					for (int i = 0; i < Props.Vertices.Length; i++) {
						vertices [i] = Props.Vertices [i];
						uvs [i] = Props.UVs [i];
					}
					m = new UnityEngine.Mesh ();
					m.vertices = vertices;
					m.uv = uvs;
					m.triangles = Props.Triangles;
					m.RecalculateNormals ();
					m.RecalculateBounds ();
					Mesh.sharedMesh = m;
				}
				RefreshFlowColliderStatic ();
			} else {
				Debug.Log ("VERTICES WERE NULL IN " + Props.Name);
			}
		}

		protected void UpdateAudioStatic () {

		}
		#endregion

		#region dynamic avatar
		public void Update ()
		{
			if (!Props.DynamicMode) {
				return;
			}

			if (!ParentChunk.Is (ChunkMode.Primary | ChunkMode.Adjascent | ChunkMode.Immediate) || !WaterRenderer.isVisible) {
				return;
			}

			if (!Mathf.Approximately (Props.TargetWaterLevel, Props.CurrentWaterLevel)) {
				Props.CurrentWaterLevel = Mathf.Lerp (Props.CurrentWaterLevel, Props.TargetWaterLevel, (float)(WorldClock.ARTDeltaTime * Props.WaterLevelChangeSpeed));
				mWaterLevelPosition.y = Props.BaseHeight + Props.CurrentWaterLevel;
				tr.localPosition = mWaterLevelPosition;
			}
		}

		public void FixedUpdateDynamic ()
		{
			if (!MasterSpline.enabled) {
				//we need to refresh
				MasterSpline.enabled = true;
				MasterSpline.UpdateSpline ();
				MasterSplineMesh.UpdateMesh ();
			}

			mColliderPositionUpdate++;
			mColorUpdate++;
			mSubmergedUpdate++;

			if (mColliderPositionUpdate > 10) {
				mColliderPositionUpdate = 0;
				UpdateColliderPositionDynamic ();
			}

			if (mSubmergedUpdate > 30) {
				mSubmergedUpdate = 0;
				for (int i = SubmergedItems.LastIndex (); i >= 0; i--) {
					IItemOfInterest ioi = SubmergedItems [i];
					if (ioi == null) {
						SubmergedItems.RemoveAt (i);
					} else {
						if (ioi.IOIType == ItemOfInterestType.Player) {
							//kludge - make the player local props accessible!
							Player.Local.FPSController.AddForce (FlowForce);
						}
					}
				}
			}

			if (mColorUpdate > 60) {
				mColorUpdate = 0;
				WaterAnimation.WaterMaterial.SetColor ("_FogColor", TOD_Sky.GlobalFogColor);
				WaterAnimation.WaterMaterial.SetColor ("_FoamColor", Colors.Alpha (Color.Lerp (Color.white, RenderSettings.fogColor, 0.25f), 0.25f));
				WaterAnimation.WaterMaterial.SetColor ("_CrestColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Brighten (RenderSettings.fogColor), 0.15f), 0.45f));
				WaterAnimation.WaterMaterial.SetColor ("_WaveColor", Colors.Alpha (Color.Lerp (Color.white, Colors.Desaturate (RenderSettings.fogColor), 0.15f), 0.55f));
			}
		}

		public void RefreshFlowColliderDynamic ()
		{
			rigidBody = gameObject.GetOrAdd <Rigidbody> ();
			rigidBody.isKinematic = true;

			WorldItem RiverFlowCollider = null;
			GameObject flowColliderObject = gameObject.FindOrCreateChild ("FlowCollider").gameObject;
			flowColliderObject.layer = Globals.LayerNumFluidTerrain;
			FlowCollider = flowColliderObject.GetOrAdd <BoxCollider> ();
			FlowCollider.size = new Vector3 (Props.Width, Props.Width, Props.Width);
			FlowCollider.center = new Vector3 (0f, (Props.Width / 2) * -1, 0f);
			FlowCollider.isTrigger = true;

			GameObject bottomColliderObject = flowColliderObject.FindOrCreateChild ("BottomCollider").gameObject;
			bottomColliderObject.layer = Globals.LayerNumFluidTerrain;
			BottomCollider = bottomColliderObject.GetOrAdd <BoxCollider> ();
			BottomCollider.size = FlowCollider.size;
			BottomCollider.center = FlowCollider.center - Vector3.up;
			BottomCollider.isTrigger = false;

			if (!string.IsNullOrEmpty (Props.AudioClipName)) {
				AudioSource riverAudio = flowColliderObject.AddComponent <AudioSource> ();
				riverAudio.clip = AudioManager.Get.AmbientClip (Props.AudioClipName);
				riverAudio.loop = true;
				riverAudio.volume = 0.1f;
				riverAudio.maxDistance = Props.AudioMaxDistance * Globals.MaxRiverAudioDistance;
				riverAudio.Play ();
			}

			UpdateColliderPositionDynamic ();
		}

		public void RefreshSpline ()
		{
			MasterSpline.updateMode = Spline.UpdateMode.DontUpdate;
			MasterSplineMesh.updateMode = SplineMesh.UpdateMode.DontUpdate;
			MasterSpline.enabled = false;

			if (Application.isPlaying) {
				if (!mRefreshingSplineNodes) {
					mRefreshingSplineNodes = true;
					StartCoroutine (RefreshSplineNodes ());
				}
			} else {
				for (int i = 0; i < Props.Nodes.Count; i++) {
					SVector3 nodePosition = Props.Nodes [i];
					GameObject node = new GameObject ("Node");//MasterSpline.AddSplineNode();
					node.transform.parent = transform;
					node.transform.localPosition = nodePosition;
					SplineNode s = node.AddComponent <SplineNode> ();
					MasterSpline.splineNodesArray.Add (s);
				}
			}

			tr.localPosition = Vector3.up * (Props.BaseHeight + Props.CurrentWaterLevel);
			MasterSplineMesh.segmentCount = Props.SegmentCount;
			MasterSplineMesh.uvScale = Props.UVScale;
			MasterSplineMesh.xyScale = Props.MeshScale;

			WaterAnimation.FlowDirection = Props.FlowDirection;
			WaterAnimation.FlowSpeed = Props.FlowSpeed;
			WaterAnimation.FoamSpeed = Props.FoamSpeed;
		}

		protected void RefreshPropsDynamic ()
		{
			MasterSpline.enabled = false;
			MasterSplineMesh.enabled = false;
			RefreshSpline ();
			RefreshFlowColliderDynamic ();
		}

		public void UpdateColliderPositionDynamic ()
		{
			if (MasterSpline != null) {
				//update the position of our river collider
				param = MasterSpline.GetClosestPointParam (Player.Local.Position, 1, 0f, 1f, 0.05f);
				if (param > 0.99f || param < 0.01f) {
					//don't submerge the player past the end
					FlowCollider.enabled = false;
				} else {
					FlowCollider.enabled = true;
					positionAlongSpline = MasterSpline.GetPositionOnSpline (param);
					forwardPositionAlongSpline = MasterSpline.GetPositionOnSpline (param + ((Props.FlowDirection == PathDirection.Forward) ? 0.01f : -0.01f));
					FlowCollider.transform.position = positionAlongSpline;
					FlowForce = (positionAlongSpline - forwardPositionAlongSpline).normalized * Mathf.Max (Props.FlowSpeed.x, Props.FlowSpeed.y) * Globals.RiverFlowForceMultiplier;
					//randomize the flow force a bit
					FlowForce.x += UnityEngine.Random.Range (-0.005f, 0.005f);
					FlowForce.z += UnityEngine.Random.Range (-0.005f, 0.005f);
					FlowForce.y = 0f;
				}
			}
		}

		protected IEnumerator RefreshSplineNodes ()
		{
			for (int i = 0; i < Props.Nodes.Count; i++) {
				SVector3 nodePosition = Props.Nodes [i];
				GameObject node = MasterSpline.AddSplineNode ();
				node.transform.parent = tr;
				node.transform.localPosition = nodePosition;
				double waitUntil = WorldClock.RealTime + 0.01f;
				while (WorldClock.RealTime < waitUntil) {
					yield return null;
				}
			}
			mRefreshingSplineNodes = false;
			yield break;
		}

		protected bool mRefreshingSplineNodes = false;
		protected float param;
		protected Vector3 positionAlongSpline;
		protected Vector3 forwardPositionAlongSpline;
		#endregion

		public void OnItemOfInterestEnterWater ()
		{
			SubmergedItems.SafeAdd (WaterSubmerge.LastSubmergedItemOfInterest);
		}

		public void OnItemOfInterestExitWater ()
		{
			SubmergedItems.Remove (WaterSubmerge.LastExitedItemOfInterest);
		}
		#if UNITY_EDITOR
		public void SaveEditor ()
		{
			Props.Nodes.Clear ();
			foreach (SplineNode node in MasterSpline.splineNodesArray) {
				Props.Nodes.Add (new SVector3 (node.transform.localPosition));
			}

			if (tr == null) {
				tr = transform;
			}

			Props.BaseHeight = tr.localPosition.y;
			Props.SegmentCount = MasterSplineMesh.segmentCount;
			Props.MeshScale = MasterSplineMesh.xyScale;
			Props.UVScale = MasterSplineMesh.uvScale;
			Props.FlowSpeed = WaterAnimation.FlowSpeed;
			Props.FoamSpeed = WaterAnimation.FoamSpeed;
			Props.WaveSpeed = WaterAnimation.WaveSpeed;

			if (!Props.DynamicMode) {
				//get the vertexes of the mesh
				Mesh m = GetComponent <MeshFilter> ().sharedMesh;
				Vector3[] vertices = m.vertices;
				Vector2[] uvs = m.uv;
				Props.Vertices = new SVector3 [vertices.Length];
				Props.UVs = new SVector2 [uvs.Length];
				for (int i = 0; i < vertices.Length; i++) {
					Props.Vertices [i] = vertices [i];
					Props.UVs [i] = uvs [i];
				}
				Props.Triangles = m.triangles;
			}

			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor ();
			Mods.Get.Editor.SaveMod <River> (Props, "River", name);

			UnityEditor.EditorUtility.SetDirty (gameObject);
			UnityEditor.EditorUtility.SetDirty (this);
		}

		public void DrawEditor ()
		{
			UnityEngine.GUI.color = Color.yellow;
			if (GUILayout.Button ("\nRefresh Props\n")) {
				RefreshProps ();
			}
			if (GUILayout.Button ("\nGet Nodes from Spline\n")) {
				Props.Nodes.Clear ();
				foreach (SplineNode node in MasterSpline.splineNodesArray) {
					Props.Nodes.Add (new SVector3 (node.transform.localPosition));
				}
			}
			UnityEngine.GUI.color = Color.cyan;
			if (GUILayout.Button ("\nSave River\n")) {
				SaveEditor ();
			}
			if (GUILayout.Button ("\nLoad River\n")) {
				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor ();
				Mods.Get.Editor.LoadMod <River> (ref Props, "River", name);
				RefreshProps ();
			}
		}
		#endif
		public void OnDrawGizmos ()
		{
			Vector3 lastPosition = Vector3.zero;
			Vector3 currentPosition = Vector3.zero;
			Gizmos.color = Color.blue;
			for (int i = 0; i < Props.Nodes.Count; i++) {
				if (i == 0) {
					lastPosition = Props.Nodes [i];
					lastPosition.y += Props.BaseHeight;
				} else {
					currentPosition = Props.Nodes [i];
					currentPosition.y += Props.BaseHeight;
					Gizmos.DrawLine (lastPosition, currentPosition);
					lastPosition = currentPosition;
				}
			}

			if (FlowCollider != null && FlowForce != Vector3.zero) {
				Gizmos.color = Color.red;
				DrawArrow.ForGizmo (FlowCollider.center, FlowForce, 2f);
			}
		}

		protected Vector3 mWaterLevelPosition;
		protected int mAudioUpdate = 0;
		protected int mSubmergedUpdate = 0;
		protected int mColliderPositionUpdate = 0;
		protected int mColorUpdate = 0;
	}

	[Serializable]
	public class River : Mod
	{
		public float Width {
			get {
				return MeshScale.x;
			}
			set {
				MeshScale.x = value;
			}
		}

		[XmlIgnore]
		[NonSerialized]
		public RiverAvatar river;
		[FrontiersAudioClipAttribute]
		public string AudioClipName;
		public bool DynamicMode = false;
		public SVector3[] Vertices;
		public SVector2[] UVs;
		public int [] Triangles;
		public float AudioMaxDistance = 50f;
		public float TargetWaterLevel = 0f;
		public float CurrentWaterLevel = 0f;
		public float WaterLevelChangeSpeed = 1f;
		public PathDirection FlowDirection = PathDirection.Forward;
		public SVector2 FlowSpeed = new SVector2 (0.0015f, 0.0015f);
		public SVector2 WaveSpeed = new SVector2 (0.0015f, 0.0015f);
		public SVector2 FoamSpeed = new SVector2 (-0.02f, -0.02f);
		public float WaterForce = 1.0f;
		public SVector2 MeshScale = new SVector2 (0.5f, 0.5f);
		public SVector2 UVScale = new SVector2 (0.5f, 0.5f);
		public float BaseHeight = 10f;
		public int SegmentCount = 5;
		public List <SVector3> Nodes = new List <SVector3> ();
		public GenericWorldItem FillContainerItem;
	}
}


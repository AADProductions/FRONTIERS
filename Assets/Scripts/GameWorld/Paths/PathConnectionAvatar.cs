using UnityEngine;
using System.Collections;
using Frontiers;

using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class PathConnectionAvatar : MonoBehaviour
		{
				public bool IsFinished {
						get {
								return mFinished;
						}
						set {
								if (value && !mFinished) {
										Finish();
								}
						}
				}

				public PathMarker ConnectionPathMarker;
				public PathMarker ConnectionPathOrigin;
				public MarkerAlterAction PathAlterAction;
				public PathSkill SkillToUse;
				public bool IsActive = true;
				public Spline spline;
				public SplineMesh PathMesh;
				public MeshRenderer PathRenderer;
				public Transform SplineNode1;
				public Transform SplineNode2;
				public Color CurrentColor = Color.white;
				public float OriginDistanceToPlayer = Mathf.Infinity;
				public float FocusOffset = Mathf.Infinity;

				public void Awake()
				{
						gameObject.layer = Globals.LayerNumScenery;
						SplineNode1 = gameObject.CreateChild("Node1");
						SplineNode2 = gameObject.CreateChild("Node2");

						SplineNode sp1 = SplineNode1.gameObject.AddComponent <SplineNode>();
						SplineNode sp2 = SplineNode2.gameObject.AddComponent <SplineNode>();

						sp1.tension = 1f;
						sp1.normal = Vector3.up;
						sp1.transform.localPosition = new Vector3(1f, 1f, 1f);
		
						sp2.tension = 1f;
						sp2.normal = Vector3.up;
						sp2.transform.localPosition = new Vector3(-1f, -1f, -1f);

						spline = gameObject.AddComponent <Spline>();
						spline.updateMode = Spline.UpdateMode.DontUpdate;

						PathMesh = gameObject.AddComponent <SplineMesh>();
						PathMesh.spline = spline;
						PathMesh.startBaseMesh = Meshes.Get.GroundPathPlane;
						PathMesh.baseMesh = Meshes.Get.GroundPathPlane;
						PathMesh.endBaseMesh = Meshes.Get.GroundPathPlane;
						PathRenderer = gameObject.AddComponent <MeshRenderer>();
						PathRenderer.sharedMaterials = new Material [] { Mats.Get.WorldPathGroundMaterial };
						PathRenderer.enabled = false;

						spline.splineNodesArray.Add(sp1);
						spline.splineNodesArray.Add(sp2);
				}

				public void Update()
				{

						if (mFinished) {
								return;
						}

						if (ConnectionPathMarker == null || ConnectionPathOrigin == null || PathAlterAction == MarkerAlterAction.None) {
								Finish();
								return;
						}

						if (Vector3.Distance(ConnectionPathMarker.worlditem.tr.position, ConnectionPathOrigin.worlditem.tr.position) > Globals.PathOriginTriggerRadius) {
								Finish();
								return;
						}

						OriginDistanceToPlayer = Vector3.Distance(Player.Local.Position, ConnectionPathOrigin.worlditem.tr.position);
						FocusOffset = Player.Local.Focus.FocusOffset(ConnectionPathOrigin.worlditem.tr.position);

						if (!IsActive) {
								spline.enabled = false;
								PathMesh.enabled = false;
								PathMesh.renderer.enabled = false;
								PathMesh.updateMode = SplineMesh.UpdateMode.DontUpdate;
								return;
						}

						spline.enabled = true;
						PathMesh.enabled = true;
						PathMesh.renderer.enabled = true;
						PathMesh.updateMode = SplineMesh.UpdateMode.EveryFrame;

						SplineNode1.position = ConnectionPathMarker.worlditem.tr.position;
						SplineNode2.position = ConnectionPathOrigin.worlditem.tr.position;

						switch (PathAlterAction) {
								case MarkerAlterAction.None:
								default:
										break;

								case MarkerAlterAction.AppendToPath:
										CurrentColor = Color.blue;
										break;

								case MarkerAlterAction.CreateBranch:
										CurrentColor = Color.green;
										break;

								case MarkerAlterAction.CreatePath:
										CurrentColor = Color.red;
										break;

								case MarkerAlterAction.CreatePathAndBranch:
										CurrentColor = Color.yellow;
										break;
						}

						PathMesh.renderer.material.SetColor("_TintColor", CurrentColor);
				}

				public void Finish()
				{
						if (mFinished) {
								return;
						}

						ConnectionPathMarker = null;
						ConnectionPathOrigin = null;
						PathAlterAction = MarkerAlterAction.None;
						SkillToUse = null;

						mFinished = true;
						GameObject.Destroy(gameObject);
				}

				protected bool mFinished = false;

				public void SetConnection(PathMarker pathMarker, PathMarker pathOrigin, MarkerAlterAction alterAction, PathSkill skillToUse)
				{
						ConnectionPathMarker = pathMarker;
						ConnectionPathOrigin = pathOrigin;
						PathAlterAction = alterAction;
						SkillToUse = skillToUse;
				}
		}
}

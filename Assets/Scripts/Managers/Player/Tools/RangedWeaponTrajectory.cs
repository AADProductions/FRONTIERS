using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Frontiers.World;

namespace Frontiers
{
	public class RangedWeaponTrajectory : MonoBehaviour
	{
		public float gLaunchForceToScaleMultiplier = 10f;

		public Spline TrajectorySpline;
		public SplineMesh TrajectoryMesh;
		public Renderer TrajectoryRenderer;

		public float SmoothLaunchForce;
		public float LaunchForce;
		public float ProjectileWeight;
		public float TrajectoryParam;

		public Vector3 TargetMidPosition;
		public Vector3 TargetEndPositon;

		public void Awake ( )
		{
//			TrajectorySpline = gameObject.AddComponent <Spline> ();
//			TrajectoryMesh = gameObject.AddComponent <SplineMesh> ();
//			TrajectoryRenderer = gameObject.AddComponent <MeshRenderer> ();
		}

		public void UpdateForce (float force)
		{

		}

		public void Show (Transform launchObject, float force, float projectileWeight)
		{
 
		}

		public void Hide ( )
		{
//			TrajectoryRenderer.enabled = false;
//			TrajectorySpline.updateMode = Spline.UpdateMode.DontUpdate;
		}

		public void LateUpdate ( )
		{
//			if (!TrajectoryRenderer.enabled)
//				return;
//
//			SmoothLaunchForce = Mathf.Lerp (SmoothLaunchForce, LaunchForce, Time.deltaTime);
		}

		public Spline TakeSnapshot ( )
		{	//duplate the spline
//			GameObject splineObject = new GameObject ("ClonedTrajectory");
//			splineObject.transform.position = transform.position;
//			splineObject.transform.rotation = transform.rotation;
//			splineObject.transform.localScale = transform.localScale;
//			Spline trajectorySpline = splineObject.AddComponent <Spline> ();
//			trajectorySpline.interpolationMode = Spline.InterpolationMode.BSpline;
//			//create the spine nodes and set their positions
//			//now add the copies of the spline nodes
//			//it's static so we never need to update it
//			trajectorySpline.updateMode = Spline.UpdateMode.DontUpdate;
//			trajectorySpline.UpdateSpline ();
			//and we're done!
			return null;// trajectorySpline;
		}

		protected void CopyNode (GameObject fromNodeObject, GameObject toNodeObject, Spline addTo)
		{
//			toNodeObject.transform.position = fromNodeObject.transform.position;
//			toNodeObject.transform.rotation = fromNodeObject.transform.rotation;
//			SplineNode fromNode = fromNodeObject.GetComponent <SplineNode> ();
//			SplineNode toNode = toNodeObject.AddComponent <SplineNode> ();
//			toNode.tension = fromNode.tension;
//			addTo.splineNodesArray.Add (toNode);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using System.Linq;
using System;
using Frontiers.World.WIScripts;

public class PathDifficultyEvaluator : MonoBehaviour
{
		public bool Evaluating = false;
		public PathSegment CurrentSegment = null;
		public float CurrentMeters = 0.0f;
		public PathDirection Direction = PathDirection.None;
		public float YieldTimePerStep	= 0.1f;
		public float MetersPerStep = 1.0f;
		public float StartMeters = 0.0f;
		public float EndMeters = 0.0f;
		public float TotalMeters = 0.0f;
		public GameObject PositionPrefab;

		public void EvaluateSegment(PathSegment segment)
		{
				if (segment.State.Difficulty == PathDifficulty.None && !mUpcomingSegments.Contains(segment)) {
						mUpcomingSegments.Push(segment);
				}

				if (!Evaluating) {
						CurrentSegment = null;
						StartCoroutine(EvaluateSegmentOverTime());
				}
		}

		public IEnumerator EvaluateSegmentOverTime()
		{
				Evaluating = true;

				while (mUpcomingSegments.Count > 0) {
						if (CurrentSegment == null) {
								CurrentSegment = mUpcomingSegments.Pop();
								StartMeters = CurrentSegment.StartMeters;
								CurrentMeters = StartMeters;
								EndMeters = CurrentSegment.EndMeters;
								TotalMeters = CurrentSegment.LengthInMeters;
								Direction = CurrentSegment.Direction;
		
								if (StartMeters > EndMeters) {
										Direction = PathDirection.Backwards;
								} else {
										Direction = PathDirection.Forward;
								}		
								mEvalPoints.Clear();
						}
			
						//change layer so we don't hit ourselves in raycast
						gameObject.layer = Globals.LayerNumHidden;

						EvalPoint evalPoint = new EvalPoint();

						evalPoint.PathPosition = CurrentSegment.ParentPath.PositionFromMeters(CurrentMeters);
						evalPoint.TerrainPosition = evalPoint.PathPosition;
						evalPoint.Ground = GroundType.Dirt;
						//get the terrain data from the game world
						mTerrainHit.feetPosition = evalPoint.PathPosition;
						mTerrainHit.groundedHeight = Globals.PlayerControllerHeightDefault;
						evalPoint.TerrainPosition.y	= GameWorld.Get.TerrainHeightAtInGamePosition(ref mTerrainHit);//evalPoint.PathPosition, true, ref evalPoint.HitWater, ref evalPoint.HitTerrainMesh, ref evalPoint.PathNormal);

						mEvalPoints.Add(evalPoint);

						//change back so we encounter objects
						gameObject.layer = Globals.LayerNumTrigger;

						if (Paths.MoveAlongPath(ref CurrentMeters, MetersPerStep, Direction, StartMeters, EndMeters) == false) {//if we're out of range, then we're done with this segment
								InterpretEvalPoints();
								CurrentSegment = null;
						} else {
								transform.position = evalPoint.TerrainPosition;
						}
			
						double start = Frontiers.WorldClock.RealTime;
						while (Frontiers.WorldClock.RealTime < start + YieldTimePerStep) {
								yield return null;
						}
				}

				Evaluating = false;
				yield break;
		}

		protected GameWorld.TerrainHeightSearch mTerrainHit;

		protected void InterpretEvalPoints()
		{
				PathDifficulty finalDifficulty = PathDifficulty.None;
		
				if (mEvalPoints.Count > 0) {
						float lastElevation = 0f;
						float currentElevation = 0f;
						//float totalLength = MetersPerStep * mEvalPoints.Count;
						bool hitWater = false;
						bool hitTerrainMesh = false;
						bool skipFirstPoint = true;
						float currentDifference = 0.0f;

						List <float> differences = new List <float>();

						foreach (EvalPoint point in mEvalPoints) {
								if (point.HitWater) {
										hitWater = true;
								}

								if (point.HitTerrainMesh) {
										hitTerrainMesh = true;
								}
				
								if (skipFirstPoint) {
										skipFirstPoint = false;
										lastElevation = point.TerrainPosition.y * Globals.InGameUnitsToMeters;
								} else {			
										currentElevation = point.TerrainPosition.y * Globals.InGameUnitsToMeters;
										currentDifference = Mathf.Abs(currentElevation - lastElevation) / MetersPerStep;
										differences.Add(currentDifference);
										lastElevation = currentElevation;
								}
								//GameWorld.Get.AddGraphNode(new TerrainNode(point.TerrainPosition));
						}

						float averageSlopePerMeter = 0f;
						if (differences.Count > 0) {
								differences.Average();
						}

						if (averageSlopePerMeter < Globals.PathSlopeDifficultyEasy) {// 15.0f)
								finalDifficulty = PathDifficulty.Easy;
						} else if (averageSlopePerMeter < Globals.PathSlopeDifficultyModerate) {//25.0f)
								finalDifficulty = PathDifficulty.Moderate;
						} else if (averageSlopePerMeter < Globals.PathSlopeDifficultyDifficult) {//50.0f)
								finalDifficulty = PathDifficulty.Difficult;
						} else if (averageSlopePerMeter < Globals.PathSlopeDifficultyDeadly) {//100.0f)
								finalDifficulty = PathDifficulty.Deadly;
						} else {
								finalDifficulty = PathDifficulty.Impassable;
						}
			
						CurrentSegment.State.AverageSlopePerMeter = averageSlopePerMeter;
						CurrentSegment.State.AverageGroundType = GroundType.Dirt;

						CurrentSegment.State.PassesOverSolidTerrainMesh &= hitTerrainMesh;
						CurrentSegment.State.PassesOverFluidTerrain &= hitWater;

						if (CurrentSegment.State.PassesOverFluidTerrain) {
								finalDifficulty = PathDifficulty.Impassable;
						}
				}
				CurrentSegment.State.Difficulty = finalDifficulty;
		}

		public void OnTriggerEnter(Collider other)
		{
				switch (other.gameObject.layer) {
						case Globals.LayerNumWorldItemActive:
								Obstruction obstruction = null;
								if (other.gameObject.HasComponent <Obstruction>(out obstruction)) {
										mObstructions.Add(obstruction);
								}
								break;
			
						case Globals.LayerNumSolidTerrain:
								break;
				}
		}

		protected Stack <PathSegment> mUpcomingSegments = new Stack <PathSegment>();
		protected List <EvalPoint> mEvalPoints = new List <EvalPoint>();
		protected List <Obstruction> mObstructions = new List <Obstruction>();

		public class EvalPoint
		{
				public EvalPoint()
				{

				}

				public EvalPoint(Vector3 pathPosition, Vector3 pathNormal, Vector3 terrainPosition, GroundType ground, bool hitWater, bool hitTerrainMesh)
				{
						PathPosition = pathPosition;
						PathNormal = pathNormal;
						TerrainPosition	= terrainPosition;
						Ground = ground;
						HitWater = hitWater;
						HitTerrainMesh	= hitTerrainMesh;
				}

				public Vector3 PathPosition;
				public Vector3 PathNormal;
				public Vector3 TerrainPosition;
				public GroundType Ground;
				public bool HitTerrainMesh = false;
				public bool HitWater = false;
		}

		public void StopEvaluating()
		{
				StopAllCoroutines();
				mUpcomingSegments.Clear();
				mEvalPoints.Clear();
				mObstructions.Clear();
		}
}

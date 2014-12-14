using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.Data;
using System.Xml.Serialization;

namespace Frontiers.World
{
		[ExecuteInEditMode]
		public class PathAvatar : MonoBehaviour
		{		//most of this functionality is being slowly absorbed
				//into the Paths manager
				public Spline spline;
				public WorldChunk PathChunk;
				public bool IsLoadingOrFilling = false;
				public List <PathSegment> Segments = new List<PathSegment>();

				public bool IsAttachedToCivilization {
						get {
								return true;
						}
				}

				public float LengthInMeters {
						get {
								return spline.Length * Globals.InGameUnitsToMeters;
						}
				}

				protected void RefreshSegments()
				{
						spline.UpdateSpline();
						//create segments
						Segments.Clear();
						for (int i = 0; i < spline.SegmentCount; i++) {
								PathSegment segment = new PathSegment();
								segment.Initialize(this, i);
								Segments.Add(segment);
						}
				}

				public void Refresh()
				{
						RefreshSegments();
				}

				public bool RemoveLocation(Location location)
				{
						/*
						if (!ContainsLocation (location)) {
							return false;
						}
						mRebuildOnRefresh = true;

						PathMarker pathMarker = null;
						if (location.worlditem.Is <PathMarker> (out pathMarker)) {
							pathMarker.State.NumTimesEdited++;
							pathMarker.State.TimeLastEdited = WorldClock.Time;
						}

						Player.Local.GroundPath.Follower.SuspendNextFrame = true;

						spline.RemoveSplineNode (location.Node);
						location.RemovePath (this);
						Refresh (true);
						*/
						return true;
				}

				public bool AddLocation(Location location)
				{
						/*
						if (ContainsLocation (location)) {
							GUIManager.PostWarning ("Already connected to path");
							return false;
						}

						mRebuildOnRefresh = true;

						float param = spline.GetClosestPointParam (location.transform.position, 5, 0f, 1f, 0.01f);
						GameObject newNodeGameObject = spline.AddSplineNode (param);
						SplineNode newNode = newNodeGameObject.GetComponent <SplineNode> ();

						Player.Local.GroundPath.Follower.SuspendNextFrame = true;

						SwapNodes (newNode, location.Node);
						GameObject.Destroy (newNodeGameObject);

						GUIManager.PostSuccess ("Added to path");
						*/
						return true;
				}

				public bool SwapNodes(SplineNode removeNode, SplineNode replacementNode)
				{
						/*
						if (spline.splineNodesArray.Contains (removeNode)) {
							int index = spline.splineNodesArray.IndexOf (removeNode);
							spline.splineNodesArray [index] = replacementNode;
							spline.UpdateSpline ();
							return true;
						}
						*/
						return false;
				}

				public Vector3 PathPointFromPosition(Vector3 position)
				{
						return PositionFromMeters(MetersFromPosition(position));
				}

				public Vector3 PositionFromMeters(float meters)
				{
						return spline.GetPositionOnSpline(Paths.MetersToParam(meters, spline.Length));
				}

				public float MetersFromPosition(Vector3 position)
				{
						return spline.GetClosestPointParam(position, 2, 0f, 1f, 0.01f) * (spline.Length * Globals.InGameUnitsToMeters);
				}

				public float MetersPerHourFromMeters(float meters)
				{
						PathSegment segment = SegmentFromMeters(meters);
						return Paths.PathDifficultyToMetersPerHour(segment.Difficulty);
				}

				public PathSegment SegmentFromMeters(float meters)
				{
						float param = Paths.MetersToParam(meters, spline.Length);
						bool foundSegment = false;
						PathSegment pathSegment = null;
						//these can easily be out of 0-1 range
						//if they are it's no big deal, just return the first / last segment
						if (param > 1f) {
								pathSegment = Segments[Segments.LastIndex()];
						} else if (param < 0f) {
								pathSegment = Segments[0];
						} else {
								for (int i = 0; i < Segments.Count; i++) {
										if (Segments[i].ParentSegment.IsParameterInRange(param)) {
												pathSegment = Segments[i];
												foundSegment = true;
												break;
										}
								}
						}
						return pathSegment;
				}

				public Location LocationNearestMeters(float meters)
				{
						Location location = null;
						float closestMetersSoFar = Mathf.Infinity;
						SplineNode closestNodeSoFar = null;
						for (int i = 0; i < spline.splineNodesArray.Count; i++) {
								SplineNode currentNode = spline.splineNodesArray[i];
								if (currentNode != null) {
										float thisNodeMeters = MetersFromPosition(currentNode.transform.position);
										if (thisNodeMeters < closestMetersSoFar) {
												closestMetersSoFar = thisNodeMeters;
												closestNodeSoFar	= currentNode;
										}
								}
						}

						if (closestNodeSoFar != null) {
								location = closestNodeSoFar.GetComponent <Location>();
						}

						return location;
				}

				public Vector3 OrientationFromMeters(float meters, bool yOnly)
				{
						float param = (meters / Globals.InGameUnitsToMeters) / spline.Length;
						Vector3 orientation	= spline.GetOrientationOnSpline(param).eulerAngles;
						if (yOnly) {
								orientation.x = 0f;
								orientation.z = 0f;
						}
						return orientation;
				}

				public Vector3 OrientationFromPosition(Vector3 position, bool yOnly)
				{
						return OrientationFromMeters(MetersFromPosition(position), yOnly);
				}

				public bool ContinuesInDirection(Location location, PathDirection direction)
				{
						int index = spline.splineNodesArray.IndexOf(location.Node);
						if (index < 0) {
								return false;
						}
						bool result = false;
						if (direction == PathDirection.Forward) {//if we're going forward then see if the next is below the count
								result = ((index + 1) < spline.splineNodesArray.Count);
						} else {//if we're going backwards see if the prev index is greater than 0
								result = ((index - 1) >= 0);
						}
						return result;
				}

				public PathDirection Direction(float startMeters, float endMeters)
				{
						if (startMeters < endMeters) {
								return PathDirection.Forward;
						}
						return PathDirection.Backwards;
				}

				public PathDirection DirectionToPosition(float meters, Vector3 position)
				{
						float metersFromPosition = MetersFromPosition(position);

						if (Mathf.Approximately(meters, metersFromPosition)) {
								return PathDirection.None;
						} else if (meters < metersFromPosition) {
								return PathDirection.Forward;
						} else {
								return PathDirection.Backwards;
						}
				}

				public bool ContainsLocation(Location location)
				{
						return spline.splineNodesArray.Contains(location.Node);
				}

				public bool ContainsLocation(string locationName, out Location location)
				{
						bool result = false;
						location = null;
						for (int i = 0; i < spline.splineNodesArray.Count; i++) {
								if (spline.splineNodesArray[i].name == locationName) {
										location = spline.splineNodesArray[i].GetComponent <Location>();
										result = (location != null);
										break;
								}
						}
						return result;
				}

				public bool ContainsLocation(string locationName)
				{
						for (int i = 0; i < spline.splineNodesArray.Count; i++) {
								if (spline.splineNodesArray[i].name == locationName) {
										return true;
								}
						}
						return false;
				}

				public List <T> GetLocations <T>() where T : WIScript
				{
						List <T> locationList = new List <T>();
						for (int i = 0; i < spline.splineNodesArray.Count; i++) {
								T location = null;
								WorldItem locationWorldItem = null;
								if (spline.splineNodesArray[i].gameObject.HasComponent <WorldItem>(out locationWorldItem)) {
										if (locationWorldItem.Is <T>(out location)) {
												locationList.Add(location);
										}
								}
						}
						return locationList;
				}

				public bool GetNeighbor(Location start, PathDirection direction, List <string> filterTypes, out Location neighbor)
				{		//this is now handled by Paths
						neighbor = null;
						/*
						if (!ContinuesInDirection (start, direction)) {	//if there are no more paths in that direction we're done
							return false;
						}
						//no need to check index because ContinuesInDirection verified it
						int startIndex = spline.splineNodesArray.IndexOf (start.Node);
						int currentIndex = startIndex;
						int increment = 1;
						if (direction == PathDirection.Backwards) {	//reverse direction
							increment = -1;
						}
						//add the first increment
						currentIndex += increment;

						bool keepLooking = true;
						bool foundNeighbor = false;
						while (keepLooking) {
							if (currentIndex < spline.splineNodesArray.Count && currentIndex >= 0) {
								Location currentLocation = spline.splineNodesArray [currentIndex].GetComponent <Location> ();
								if (!filterTypes.Contains (currentLocation.State.Type)) {
									neighbor = currentLocation;
									keepLooking = false;
									foundNeighbor	= true;
								}
							} else {
								keepLooking = false;
							}
							currentIndex += increment;
						}
						return foundNeighbor;
						*/
						return false;
				}

				public List <Location> GetNeighbors(Location start, List <string> filterTypes)
				{
						List <Location> locations = new List <Location>();
						int startIndex = spline.splineNodesArray.IndexOf(start.Node);
						if (startIndex < 0) {
								return locations;
						}

						int forwardIndex = startIndex + 1;
						int backwardIndex = startIndex - 1;
						bool include = true;
						bool keepLookingForward = true;
						bool keepLookingBackward	= true;

						while (keepLookingForward) {
								if (forwardIndex < spline.splineNodesArray.Count) {
										include = true;
										Location forwardLocation = spline.splineNodesArray[forwardIndex].GetComponent <Location>();
										foreach (string filterType in filterTypes) {
												if (forwardLocation.State.Type == filterType) {
														include = false;
														break;
												}
										}
										if (include) {
												locations.Add(forwardLocation);
												keepLookingForward = false;
										}
								} else {
										keepLookingForward = false;
								}
								forwardIndex++;
						}

						while (keepLookingBackward) {
								if (backwardIndex >= 0) {
										include = true;
										Location backwardLocation = spline.splineNodesArray[backwardIndex].GetComponent <Location>();
										foreach (string filterType in filterTypes) {
												if (backwardLocation.State.Type == filterType) {
														include = false;
														break;
												}
										}
										if (include) {
												locations.Add(backwardLocation);
												keepLookingBackward = false;
										}
								} else {
										keepLookingBackward = false;
								}
								backwardIndex--;
						}

//			//Debug.Log ("Locations: " + locations.Count);

						return locations;
				}

				public Location StartLocation {
						get {
								return spline.splineNodesArray[0].GetComponent <Location>();
						}
				}

				public Location EndLocation {
						get {
								return spline.splineNodesArray[spline.splineNodesArray.Count - 1].GetComponent <Location>();
						}
				}

				protected HashSet <Obstruction> mObstructions = new HashSet <Obstruction>();
				protected bool mCreatedNodes = false;
				protected bool mAddGraphNodes = false;
				protected bool mRebuildOnRefresh = false;
		}

		public enum PathMarkerSize
		{
				Path,
				Street,
				Road,
		}

		[Flags]
		public enum PathMarkerType
		{
				None = 0,
				Marker = 1,
				Cross = 2,
				Location = 4,
				Campsite = 8,
				Path = 16,
				Street = 32,
				Road = 64,
				Landmark = 128,
				PathMarker = Marker | Path,
				CrossMarker = Cross | Path,
				StreetMarker = Marker | Street,
				CrossStreet = Cross | Street,
				RoadMarker = Marker | Road,
				CrossRoads = Cross | Road,
				PathOrigin = Cross | Campsite | Location | Landmark,
		}

		[Serializable]
		public class SegmentState
		{
				public PathDifficulty Difficulty = PathDifficulty.None;
				public bool IsObstructed = false;
				public bool PassesOverFluidTerrain = false;
				public bool PassesOverSolidTerrainMesh	= false;
				public float AverageSlopePerMeter = 0.0f;
				public float LengthInMeters = 0.0f;
				public GroundType AverageGroundType = GroundType.Dirt;
		}

		[Serializable]
		public class PathState
		{
				public LocationName Name = new LocationName();
				public PathType Type = PathType.None;
				public PathDifficulty Difficulty = PathDifficulty.None;
				public float LengthInMeters = 0.0f;
				public int NumNodes = 0;
				public bool HasEmptySpots = false;
				public List <PathSegment> Segments = new List <PathSegment>();
				public SDictionary <int, MobileReference> LocationReferences = new SDictionary <int, MobileReference>();
				public List <MobileReference> Obstructions = new List <MobileReference>();
				public bool IsDirty = false;
				public int NumTimesUsed = 0;
				public int NumTimesUsedByPlayer	= 0;
				public int NumTimesEdited = 0;
				public float FirstTimeUsed = 0.0f;
				public float LastTimeUsed = 0.0f;
				public float TimeCreated = 0.0f;
		}

		[Serializable]
		public class PathMarkerInstanceTemplate : IHasPosition
		{
				[XmlIgnore]
				[NonSerialized]
				[HideInInspector]
				public Frontiers.World.Locations.PathMarker Owner;
				[XmlIgnore]
				[NonSerialized]
				[HideInInspector]
				public Frontiers.World.Path ParentPath;

				public int IndexInPath(string pathName)
				{
						if (pathName == ParentPath.Name) {
								return IndexInParentPath;
						}
						int index;
						if (Branches.TryGetValue(pathName, out index)) {
								return index;
						}
						return -1;
				}

				[XmlIgnore]
				public int ID = -1;
				[XmlIgnore]
				public int Marker = -1;
				[XmlIgnore]
				public bool IsActive = false;

				[XmlIgnore]
				public bool HasInstance {
						get {
								return Owner != null && !Owner.IsFinished;
						}
						set {
								if (!value) {
										Owner = null;
								}
						}
				}

				[XmlIgnore]
				public bool HasParentPath {
						get {
								return ParentPath != null;
						}
				}

				public int IndexInParentPath = 0;
				public bool IsTerminal = false;

				public static PathMarkerInstanceTemplate Empty {
						get {
								if (gEmptyInstance == null) {
										gEmptyInstance = new PathMarkerInstanceTemplate();
								}
								return gEmptyInstance;
						}
				}

				public bool RequiresInstance {
						get {
								return Type != PathMarkerType.Location;
						}
				}

				public PathMarkerType Type = PathMarkerType.PathMarker;
				public MobileReference Location = MobileReference.Empty;
				public VisitableState Visitable = new VisitableState();
				public RevealableState Revealable = new RevealableState();
				public float XPos = 0f;
				public float YPos = 0f;
				public float ZPos = 0f;
				public float XRot = 0f;
				public float YRot = 0f;
				public float ZRot = 0f;

				[XmlIgnore]
				public Vector3 Position { 
						get {
								mPosition.Set(XPos, YPos, ZPos);
								return mPosition;
						}
						set {
								XPos = value.x;
								YPos = value.y;
								ZPos = value.z;
								mPosition.Set(XPos, YPos, ZPos);
						}
				}

				[XmlIgnore]
				public Vector3 Rotation {
						get {
								mRotation.Set(XRot, YRot, ZRot);
								return mRotation;
						}
						set {
								XRot = value.x;
								YRot = value.y;
								ZRot = value.z;
								mRotation.Set(XRot, YRot, ZRot);
						}
				}

				public string PathName = string.Empty;
				//outgoing branches are saved and then rebuilt
				[XmlIgnore]
				public SDictionary <string,int> Branches = new SDictionary <string,int>();
				protected Vector3 mPosition;
				protected Vector3 mRotation;
				protected static PathMarkerInstanceTemplate gEmptyInstance;
				public static int gID = 0;
		}
}
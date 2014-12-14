using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.Locations;

[Serializable]
public class PathSegment
{		//basically a wrapper for the underlying spline segment class
		[XmlIgnore]
		[NonSerialized]
		public PathAvatar ParentPath;
		public SegmentState State;

		public float StartMeters {
				get {
						if (IsInitialized) {
								return ParentPath.MetersFromPosition(ParentSegment.StartNode.transform.position);
						}
						return 0f;
				}
		}

		public float EndMeters {
				get {
						if (IsInitialized) {
								return ParentPath.MetersFromPosition(ParentSegment.EndNode.transform.position);
						}
						return 1f;
				}
		}

		public float LengthInMeters {
				get {
						if (IsInitialized) {
								return ParentSegment.Length * Globals.InGameUnitsToMeters;
						}
						return 1f;
				}
		}

		public bool IsInitialized {
				get {
						return (ParentPath != null);// && StartLocation != null && EndLocation != null);
				}
		}

		public bool HasBeenEvaluated {
				get {
						return State.Difficulty != PathDifficulty.None;
				}
		
		}

		public PathDifficulty Difficulty {
				get {
						if (State.IsObstructed) {
								return PathDifficulty.Impassable;
						} else {
								return State.Difficulty;
						}
				}
		}

		public SplineSegment ParentSegment {
				get {
						try {
								return ParentPath.spline.SplineSegments[mSegmentNumber];
						} catch (Exception e) {
								Debug.Log(e.ToString());
								return null;
						}
				}
		}

		public Spline spline {
				get {
						return ParentPath.spline;
				}
		}

		public void Initialize(PathAvatar parentPath, int segmentNumber)
		{
				ParentPath = parentPath;
				mSegmentNumber = segmentNumber;
				State = new SegmentState();
				State.Difficulty = PathDifficulty.None;
				//State.PassesOverSolidTerrainMesh	= StartLocation.State.PlacedOnSolidTerrainMesh || EndLocation.State.PlacedOnSolidTerrainMesh;
		}

		public PathDirection Direction {
				get {
						return PathDirection.None;// ParentPath.GetDirection (StartLocation, EndLocation);
				}
		}

		public bool HasBeenRevealed {
				get {
						return true;
						/*
						bool startRevealed 	= true;
						bool endRevealed	= true;
						Revealable revealable = null;
						if (StartLocation.worlditem.Is <Revealable> (out revealable))
						{
							startRevealed = revealable.State.HasBeenRevealed;
						}
						if (EndLocation.worlditem.Is <Revealable> (out revealable))
						{
							endRevealed = revealable.State.HasBeenRevealed;
						}
						return startRevealed || endRevealed;
						*/
				}
		}

		protected int mSegmentNumber = -1;

		public enum DisplayMode
		{
				None,
				InPath,
				InPathCurrent,
				Available,
				Highlight,
		}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	//utility for editing paths in the Unity editor
	[ExecuteInEditMode]
	public class PathEditor : MonoBehaviour
	{
		public Spline spline = null;
		public Path State = null;
		public WorldChunk Chunk = null;
		public Transform ChunkTransform;
		//can be used if the chunk isn't present / in world editor mode
		public Vector3 ChunkOffset {
			get {
				if (Chunk != null) {
					return Chunk.ChunkOffset;
				} else if (ChunkTransform != null) {
					return ChunkTransform.position;
				} else {
					return Vector3.zero;
				}
			}
		}

		public bool RevealPath = false;
		public bool IsAnExtension = false;
		public bool AddToEnd = true;
		public bool DoNotRefreshOrSave = true;
		public string Name = string.Empty;
		public float InGameLength = 0;
		public Color PathColor = Color.white;
		public List <KeyValuePair <string,int>> AttachedTo = new List<KeyValuePair<string, int>> ();

		public void Update ()
		{
			#if UNITY_EDITOR
			if (spline != null) {
				if (UnityEditor.Selection.activeGameObject == gameObject || UnityEditor.Selection.activeGameObject == spline.gameObject) {
					if (!spline.gameObject.activeSelf) {
						spline.enabled = true;
						spline.gameObject.SetActive (true);
					}
				} else {
					if (spline.gameObject.activeSelf) {
						spline.enabled = false;
						spline.gameObject.SetActive (false);
					}
				}
			}

			if (Chunk == null && ChunkTransform == null) {
				Chunk = GameObject.FindObjectOfType <WorldChunk> ();
			}

			if (string.IsNullOrEmpty (Name)) {
				Name = name;
			} else {
				name = Name;
			}

			PathColor = Colors.Saturate (Colors.ColorFromString (name, 100));
			#endif
		}

		public void RebuildPathSpacing ()
		{

		}
		#if UNITY_EDITOR
		public static float DrawPathGizmo (Path path, bool selected, Vector3 chunkOffset, Color pathColor)
		{
			Gizmos.color = Colors.Alpha (Color.yellow, 0.25f);
			Vector3 boundsCenter = path.PathBounds.center;
			boundsCenter -= chunkOffset;
			Gizmos.DrawWireCube (boundsCenter, path.PathBounds.size);

			float totalLength = 0f;
			Vector3 lastPos = Vector3.zero;
			for (int i = 0; i < path.Templates.Count; i++) {
				PathMarkerInstanceTemplate pm = path.Templates [i];
				Vector3 currentPos = (pm.Position - chunkOffset);

				if (i <= 0) {
					//we're at the very start
					Vector3 nextPos = path.Templates [i + 1].Position;
					Gizmos.DrawCube (currentPos, Vector3.one * 1f);
				} else if (i >= path.Templates.LastIndex ()) {
					//we're at the very end
					Gizmos.DrawWireCube (currentPos, Vector3.one * 1f);
				}

				float alpha = 1.0f;
				Gizmos.color = Color.Lerp (Color.green, pathColor, 0.5f);
				if (!pm.HasInstance) {
					//if this path marker exists in our spline, it's ours
					Gizmos.color = Color.Lerp (Color.red, pathColor, 0.5f);
					alpha = 0.5f;
				}
				float sphereRadius = 0.5f;
				float lineHeight = 2f;
				if (selected) {
					lineHeight = 5f;
					sphereRadius = 1.5f;
				}
				Gizmos.DrawSphere (currentPos, sphereRadius);
				Gizmos.color = pathColor;
				Gizmos.DrawLine (currentPos, currentPos + (Vector3.up * lineHeight));

				if (pm.Branches.Count > 1) {
					UnityEditor.Handles.Label (pm.Position, pm.Branches.Count.ToString ());
					Gizmos.color = Colors.Brighten (pathColor);
				} else {
					Gizmos.color = Colors.Darken (pathColor);
				}

				switch (pm.Type) {
				case PathMarkerType.CrossRoads:
				case PathMarkerType.CrossStreet:
				case PathMarkerType.CrossMarker:
					Gizmos.DrawWireSphere (currentPos, 10.0f);
					break;

				case PathMarkerType.None:
					pm.Type = PathMarkerType.PathMarker;
					break;

				default:
					if (Flags.Check ((uint)PathMarkerType.Campsite, (uint)pm.Type, Flags.CheckType.MatchAny)) {
						Gizmos.DrawWireSphere (currentPos, 20.0f);
					} else if (Flags.Check ((uint)PathMarkerType.Location, (uint)pm.Type, Flags.CheckType.MatchAny)) {
						Gizmos.DrawWireCube (currentPos, Vector3.one * 10f);
					}
					break;
				}

				if (lastPos != Vector3.zero) {
					float distanceBetween = Vector3.Distance (lastPos, currentPos);
					totalLength += distanceBetween;
					if (distanceBetween > 50) {
						Gizmos.color = Color.red;
					} else if (distanceBetween > 30) {
						Gizmos.color = Color.yellow;
					} else if (distanceBetween > 15) {
						Gizmos.color = Color.green;
					} else {
						Gizmos.color = Color.cyan;
					}
					Gizmos.color = Colors.Alpha (Gizmos.color, alpha);
					Gizmos.DrawLine (lastPos, currentPos);
					Gizmos.color = pathColor;
					Gizmos.DrawLine (lastPos + Vector3.up, currentPos + Vector3.up);

				}

				lastPos = currentPos;
			}
			return totalLength;
		}

		public void OnDrawGizmos ()
		{
			if (State == null) {
				return;
			}

			bool selected = UnityEditor.Selection.activeGameObject == gameObject;

			InGameLength = DrawPathGizmo (State, selected, ChunkOffset, PathColor);
		}
		#endif
		public void EditorSave ()
		{
			if (DoNotRefreshOrSave) {
				return;
			}

			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor ();
			Mods.Get.Editor.SaveMod <Path> (State, "Path", name);
		}

		public void EditorLoad ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor ();
			if (Mods.Get.Editor.LoadMod <Path> (ref State, "Path", name)) {
				Debug.Log ("Loaded state for path");
				EditorRefresh ();
			}
		}

		public void EditorRefresh ()
		{
			#if UNITY_EDITOR
			if (DoNotRefreshOrSave) {
				return;
			}

			if (!gameObject.activeSelf) {
				return;
			}

			if (spline == null) {
				if (!FindSpline ()) {
					return;
				}
			}

			UnityEditor.EditorUtility.SetDirty (gameObject);
			AttachedTo.Clear ();
			Bounds bounds = new Bounds ();

			//then check against the spline nodes
			bool isEmpty = State.Templates.Count == 0;
			for (int i = 0; i < spline.splineNodesArray.Count; i++) {
				Vector3 splinePosition = spline.splineNodesArray [i].transform.position + ChunkOffset;
				Vector3 splineRotation = spline.splineNodesArray [i].transform.rotation.eulerAngles;
				PathMarkerTemplateEditor pmit = spline.splineNodesArray [i].gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
				PathMarkerInstanceTemplate pm = pmit.Template;//HERE
				if (isEmpty) {
					pm.Position = splinePosition;
					pm.Rotation = splineRotation;
					pm.HasInstance = true;
					if (string.IsNullOrEmpty (pm.PathName)) {
						pm.PathName = Name;
					}
					State.Templates.Add (pm);//HERE
				}
			}

			if (IsAnExtension) {
				for (int i = 0; i < spline.splineNodesArray.Count; i++) {
					Vector3 splinePosition = spline.splineNodesArray [i].transform.position + ChunkOffset;
					Vector3 splineRotation = spline.splineNodesArray [i].transform.rotation.eulerAngles;
					PathMarkerTemplateEditor pmit = spline.splineNodesArray [i].gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
					PathMarkerInstanceTemplate pm = pmit.Template;
					pm.Position = splinePosition;
					pm.Rotation = splineRotation;
					if (AddToEnd) {
						State.Templates.Add (pm);
					} else {
						State.Templates.Insert (0, pm);
					}
				}
				IsAnExtension = false;
			}

			//first check against the state templates
			for (int i = 0; i < State.Templates.Count; i++) {
				PathMarkerInstanceTemplate pm = State.Templates [i];
				pm.HasInstance = false;
				pm.Type = PathMarkerType.None;
				//see if the template has a spline node counterpart
				for (int j = 0; j < spline.splineNodesArray.Count; j++) {
					Vector3 statePosition = spline.splineNodesArray [j].transform.position + ChunkOffset;
					Vector3 splineRotation = spline.splineNodesArray [j].transform.rotation.eulerAngles;
					float distance = Vector3.Distance (statePosition, State.Templates [i].Position);
					//if it's within 0.25, we're fine
					if (distance < 0.25f) {
						PathMarkerTemplateEditor pmEditor = spline.splineNodesArray [j].gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
						pmEditor.Template.HasInstance = true;
						State.Templates [i] = pmEditor.Template;
						//pm.HasInstance = true;
						//pm.Position = statePosition;
						//pm.Rotation = splineRotation;
						//PathMarkerTemplateEditor pmEditor = spline.splineNodesArray [j].gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
						//shared path marker??
						//pmEditor.Template = pm;//HERE
						break;
					}
				}

				if (string.IsNullOrEmpty (pm.PathName) || pm.PathName == " ") {
					pm.PathName = Name;
				}

				if (pm.Marker < 0) {
					pm.Marker = gPM++;
				}

				if (pm.PathName != Name) {
					//if the path's name is not our state name then it's a branch
					if (!pm.Branches.ContainsKey (Name)) {
						//add a branch to the template with the index of this path marker
						pm.Branches.Add (Name, i);
					}
				}

				if (i == 0) {
					bounds = new Bounds (pm.Position, Vector3.one);
				} else {
					bounds.Encapsulate (pm.Position);
				}

				if (RevealPath) {
					pm.Revealable.RevealMethod = LocationRevealMethod.ByDefault;
				} else {
					pm.Revealable.RevealMethod = LocationRevealMethod.None;
				}

				if (pm.Branches.ContainsKey ("")) {
					pm.Branches.Remove ("");
				}

				if (pm.Branches.Count > 0) {
					AttachedTo.AddRange (pm.Branches);
					List <KeyValuePair <string,int>> orderedAttachedTo = new List<KeyValuePair <string, int>> ();
					orderedAttachedTo.AddRange (AttachedTo.OrderBy (o => o.Value));
					AttachedTo = orderedAttachedTo;
					if (pm.Branches.Count > 1) {
						pm.Type |= PathMarkerType.Cross;
						pm.Type &= ~PathMarkerType.Marker;//remove
					} else {
						pm.Type &= ~PathMarkerType.Cross;//remove
						pm.Type |= PathMarkerType.Marker;
					}
				} else {
					pm.Type &= ~PathMarkerType.Cross;//remove
					pm.Type |= PathMarkerType.Marker;
				}

				if (!Flags.Check ((uint)(PathMarkerType.Path | PathMarkerType.Street | PathMarkerType.Road), (uint)pm.Type, Flags.CheckType.MatchAny)) {
					pm.Type |= PathMarkerType.Path;
				}
			}

			State.Bounds = bounds;
			#endif
		}

		protected static int gPM = 0;

		public void MergeExtendedPath ()
		{

		}

		public void EditorFindGround ()
		{

		}

		public void UpdateSplineNodes ()
		{
			if (spline == null) {
				FindSpline ();
			}
			for (int i = spline.splineNodesArray.LastIndex (); i >= 0; i--) {
				SplineNode node = spline.splineNodesArray [i];
				if (node == null) {
					spline.splineNodesArray.RemoveAt (i);
				} else {
					PathMarkerTemplateEditor template = node.gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
					if (template.Template != null && template.Template.ID > 0) {
						template.transform.position = template.Template.Position;
					}
				}
			}
		}

		public bool FindSpline ()
		{
			foreach (Transform child in transform) {
				if (child.gameObject.HasComponent <Spline> (out spline)) {
					return true;
				}
			}
			return false;
		}

		public void BuildSpline ()
		{
			if (spline != null) {
				foreach (SplineNode existingNode in spline.splineNodesArray) {
					GameObject.DestroyImmediate (existingNode.gameObject);
				}
				spline.splineNodesArray.Clear ();
				GameObject.DestroyImmediate (spline.gameObject);
			}

			spline = gameObject.FindOrCreateChild ("Spline").gameObject.GetOrAdd <Spline> ();
			//put the spline at the center of our path bounds so it's easy to select
			spline.transform.position = State.PathBounds.center;

			if (spline.splineNodesArray.Count != State.Templates.Count) {
				foreach (PathMarkerInstanceTemplate template in State.Templates) {
					GameObject splineNode = spline.AddSplineNode ();
					splineNode.transform.parent = spline.transform;
					splineNode.transform.position = template.Position;
					PathMarkerTemplateEditor pmte = splineNode.gameObject.AddComponent <PathMarkerTemplateEditor> ();
					pmte.Template = template;
				}
			}

		}

		public void ReverseSplineNodeOrder ()
		{
			spline.splineNodesArray.Reverse ();
			spline.UpdateSpline ();

			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty (spline);
			UnityEditor.EditorUtility.SetDirty (gameObject);
			#endif
		}
	}
}
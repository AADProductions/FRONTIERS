using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.WIScripts;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Frontiers
{
	#if UNITY_EDITOR
	public class WorldPathsEditor : MonoBehaviour
	{
		public List <Path> LoadedPaths = new List <Path> ();
		public List <PathEditor> PathEditors = new List<PathEditor> ();
		public List <Transform> ChunkTransforms = new List<Transform> ();
		public Transform ChunkParentTransform;
		public Transform PathParentTransform;
		public float SnapDistance = 0.25f;

		public void DrawEditor ()
		{
			UnityEngine.GUI.color = Color.cyan;
			if (GUILayout.Button ("\nLoad Paths\n")) {
				foreach (PathEditor pathEditor in PathEditors) {
					GameObject.DestroyImmediate (pathEditor.gameObject);
				}
				PathEditors.Clear ();
				LoadedPaths.Clear ();
				ChunkTransforms.Clear ();

				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				ChunkParentTransform = gameObject.FindOrCreateChild ("Chunks");
				PathParentTransform = gameObject.FindOrCreateChild ("Paths");

				List <string> chunkNames = Mods.Get.ModDataNames ("Chunk");
				foreach (string chunkName in chunkNames) {
					ChunkState chunkState = null;
					if (Mods.Get.Runtime.LoadMod <ChunkState> (ref chunkState, "Chunk", chunkName)) {
						GameObject newChunkGameObject = ChunkParentTransform.gameObject.FindOrCreateChild (chunkState.ID.ToString ()).gameObject;
						//look up the chunk terrain data and apply the offset
						Vector3 chunkPosition = chunkState.TileOffset;
						//chunkPosition.y = chunkState.YOffset;
						newChunkGameObject.transform.position = chunkPosition;
						//we'll use the ID for looking up path chunks later
						ChunkTransforms.Add (newChunkGameObject.transform);
						//now look for any terrain
						GameObject terrainObject = GameObject.Find (chunkState.XTilePosition.ToString () + " " + chunkState.ZTilePosition.ToString ());
						if (terrainObject != null) {
							terrainObject.transform.parent = newChunkGameObject.transform;
							terrainObject.transform.ResetLocal ();
							Terrain terrain = terrainObject.GetComponent <Terrain> ();
							terrain.heightmapPixelError = 50;
						}
					}
				}
				Mods.Get.Editor.LoadAvailableMods<Path> (LoadedPaths, "Path");
				foreach (Path path in LoadedPaths) {
					GameObject pathEditorGameObject = PathParentTransform.gameObject.FindOrCreateChild (path.Name).gameObject;
					PathEditor pathEditor = pathEditorGameObject.GetOrAdd <PathEditor> ();
					pathEditor.DoNotRefreshOrSave = false;
					pathEditor.Name = path.Name;
					pathEditor.State = path;
					pathEditor.EditorRefresh ();
					pathEditor.BuildSpline ();
				}

				MergeOverlappingTemplates ();
			}

			if (GUILayout.Button ("\nSnap Nearby Templates\n")) {
				SnapNearbyTemplates ();
				MergeOverlappingTemplates ();
			}

			if (GUILayout.Button ("\nMinimum Height\n")) {
				AdjustMinimumHeight ();
			}

			if (GUILayout.Button ("\nRefreshAll\n")) {
				foreach (PathEditor pe in PathEditors) {
					Debug.Log (pe.Name);
					pe.EditorRefresh ();
				}
			}

			if (GUILayout.Button ("\nSave Paths\n")) {
				PathEditors.Clear ();
				PathEditors.AddRange (PathParentTransform.GetComponentsInChildren <PathEditor> ());
				foreach (PathEditor editor in PathEditors) {
					editor.EditorSave ();
				}
			}
		}

		public void AdjustMinimumHeight ()
		{
			PathEditors.Clear ();
			PathEditors.AddRange (PathParentTransform.GetComponentsInChildren <PathEditor> ());
			foreach (PathEditor pathEditor in PathEditors) {
				if (pathEditor.spline == null) {
					pathEditor.FindSpline ();
				}
				foreach (SplineNode splineNode in pathEditor.spline.splineNodesArray) {
					//raycast to terrain
					//if terrain height is GREATER than pm height, adjust
					//otherwise leave it alone
					RaycastHit hit;
					if (Physics.Raycast (splineNode.Position + (Vector3.up * 100), Vector3.down, out hit, 150f, Globals.LayerSolidTerrain)) {
						if (splineNode.transform.position.y < hit.point.y) {
							Debug.Log ("Adjusted spline node to height");
							splineNode.transform.position = hit.point;
							PathMarkerTemplateEditor pmte = splineNode.gameObject.GetOrAdd <PathMarkerTemplateEditor> ();
							pmte.Template.Position = splineNode.transform.position;
						}
					}
				}
			}

		}

		public void SnapNearbyTemplates ()
		{
			foreach (Path path1 in LoadedPaths) {
				foreach (Path path2 in LoadedPaths) {
					if (path1 != path2) {
						foreach (PathMarkerInstanceTemplate pm1 in path1.Templates) {
							foreach (PathMarkerInstanceTemplate pm2 in path2.Templates) {
								if (pm1 != pm2) {
									if (Vector3.Distance (pm1.Position, pm2.Position) < SnapDistance) {
										pm1.Position = pm2.Position;
									}
								}
							}
						}
					}
				}
			}
			foreach (PathEditor editor in PathEditors) {
				editor.UpdateSplineNodes ();
			}
		}

		public int gID = 0;

		public void MergeOverlappingTemplates ()
		{

			gID = 0;
			foreach (Path path in LoadedPaths) {
				foreach (PathMarkerInstanceTemplate template in path.Templates) {
					template.ID = gID++;
				}
			}


			Path path1 = null;
			Path path2 = null;
			Path replaceCheckPath = null;
			PathMarkerInstanceTemplate pm1;
			PathMarkerInstanceTemplate pm2;
			Bounds path1Bounds;
			Bounds path2Bounds;

			float mergeDistance = 0.15f;
			for (int i = 0; i < LoadedPaths.Count; i++) {
				path1 = LoadedPaths [i];
				for (int j = 0; j < LoadedPaths.Count; j++) {
					if (i != j) {
						path2 = LoadedPaths [j];
						path1Bounds = path1.PathBounds;
						path2Bounds = path2.PathBounds;
						//extend the bounds a tad to leave room for error
						path1Bounds.size = path1Bounds.size * 1.15f;
						path2Bounds.size = path2Bounds.size * 1.15f;
						if (path1Bounds.Intersects (path2Bounds)) {
							//they occupy some of the same space, so check each path marker against every other
							for (int x = 0; x < path1.Templates.Count; x++) {
								for (int y = 0; y < path2.Templates.Count; y++) {
									pm1 = path1.Templates [x];
									pm2 = path2.Templates [y];
									if (pm1 != pm2 && Vector3.Distance (pm1.Position, pm2.Position) < mergeDistance) {
										//if they're not already the same (for some reason... just in case)
										//BRUUUUUTE FOOOOOOORCE!
										ReplaceAllTemplateInstances (pm1, pm2);
									}
								}
							}
						}
					}
				}
			}
			foreach (Path path in LoadedPaths) {
				path.RefreshBranches ();
			}
		}

		protected void ReplaceAllTemplateInstances (PathMarkerInstanceTemplate pm1, PathMarkerInstanceTemplate pm2)
		{
			for (int i = 0; i < LoadedPaths.Count; i++) {
				for (int j = 0; j < LoadedPaths [i].Templates.Count; j++) {
					if (LoadedPaths [i].Templates [j] == pm1) {
						LoadedPaths [i].Templates [j] = pm2;
					}
				}
			}
			//now make sure the editors are up to date
			PathEditors.Clear ();
			PathEditors.AddRange (PathParentTransform.GetComponentsInChildren <PathEditor> ());
			foreach (PathEditor editor in PathEditors) {
				foreach (SplineNode node in editor.spline.splineNodesArray) {
					PathMarkerTemplateEditor pmte = node.gameObject.GetComponent <PathMarkerTemplateEditor> ();
					if (pmte.Template == pm1) {
						pmte.Template = pm2;
					}
				}
			}
		}

		public void OnDrawGizmos ()
		{
			if (ChunkParentTransform != null) {
				foreach (Transform child in ChunkParentTransform) {
					Gizmos.color = Color.white;
					//just draw the corners
					Gizmos.DrawWireCube (child.position, Vector3.one * 10);
				}
			}
		}
	}
	#endif
}
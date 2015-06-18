using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
	public class ActionNodePlaceholder : MonoBehaviour
	{
		public List <ActionNodePlaceholder> InConnections = new List <ActionNodePlaceholder> ();
		public Vector3 HeadPosition;
		public int Index = 0;
		public bool SaveToStructure = true;
		public bool LinkToClosest = false;

		#if UNITY_EDITOR
		public MovementNode ToMovementNode ()
		{
			MovementNode m = new MovementNode (Index);
			m.X = transform.localPosition.x;
			m.Y = transform.localPosition.y;
			m.Z = transform.localPosition.z;
			return m;
		}

		void OnDrawGizmos ()
		{
			if (InConnections.Count > 0) {
				Gizmos.color = Color.cyan;
				name = "_NavCross_";
			} else {
				Gizmos.color = Color.white;
				name = "_NavIn_";
			}

			if (!SaveToStructure) {
				Gizmos.color = Frontiers.Colors.Alpha (Color.yellow, 0.15f);
			}

			Vector3 startFeet = transform.position;
			HeadPosition = startFeet + (transform.up * 2.0f);
			Gizmos.DrawLine (startFeet, startFeet + (transform.up * 2.0f));
			Gizmos.color = Frontiers.Colors.Alpha (Gizmos.color, Gizmos.color.a * 0.25f);
			if (SaveToStructure) {
				Gizmos.DrawSphere (startFeet, 0.55f);
			} else {
				Gizmos.DrawSphere (startFeet, 0.35f);
			}
			Gizmos.DrawWireCube (startFeet, new Vector3 (0.25f, 0.01f, 0.25f));

			for (int i = 0; i < InConnections.Count; i++) {
				Gizmos.color = Frontiers.Colors.Alpha (Gizmos.color, 0.5f);
				DrawArrow.ForGizmo (InConnections [i].HeadPosition, (HeadPosition - InConnections [i].HeadPosition) * 0.9f, 0.25f, 20f);
			}
		}

		public static void LinkPlaceholders (List<ActionNodePlaceholder> acps, List<MovementNode> movementNodes)
		{
			for (int i = 0; i < movementNodes.Count; i++) {
				ActionNodePlaceholder currentAcp = null;
				MovementNode m = movementNodes [i];
				foreach (ActionNodePlaceholder a in acps) {
					if (a.Index == m.Index) {
						currentAcp = a;
						break;
					}
				}

				if (currentAcp == null) {
					Debug.Log ("Couldn't find action node index " + m.Index.ToString ());
					continue;
				}

				if (m.ConnectingNodes == null) {
					Debug.Log ("No connecting nodes");
					continue;
				}

				//get all the other linked nodes
				currentAcp.InConnections.Clear ();
				for (int j = 0; j < m.ConnectingNodes.Count; j++) {
					foreach (ActionNodePlaceholder linkedAcp in acps) {
						if (linkedAcp.Index == m.ConnectingNodes [j]) {
							currentAcp.InConnections.Add (linkedAcp);
						}
					}
				}
			}
		}

		public static void LinkNodes (List<ActionNodePlaceholder> acps, List<ActionNode> nodes, List<MovementNode> movementNodes)
		{
			movementNodes.Clear ();
			for (int i = 0; i < acps.Count; i++) {
				ActionNodePlaceholder acp = acps [i];
				acp.Index = i;
				MovementNode mn = new MovementNode (i);
				mn.X = acp.transform.localPosition.x;
				mn.Y = acp.transform.localPosition.y;
				mn.Z = acp.transform.localPosition.z;
				movementNodes.Add (mn);
			}

			for (int i = 0; i < movementNodes.Count; i++) {
				MovementNode mn = movementNodes [i];
				for (int j = 0; j < acps.Count; j++) {
					if (acps [j].Index == i) {
						//check all connected nodes
						foreach (ActionNodePlaceholder otherAcp in acps [j].InConnections) {
							mn.ConnectingNodes.SafeAdd (otherAcp.Index);
						}
					}
				}
				movementNodes [i] = mn;
			}

			for (int i = 0; i < movementNodes.Count; i++) {
				for (int j = 0; j < movementNodes.Count; j++) {
					if (i != j) {
						MovementNode mn1 = movementNodes [i];
						MovementNode mn2 = movementNodes [j];
						if (mn1.ConnectingNodes.Contains (mn2.Index)) {
							mn2.ConnectingNodes.SafeAdd (mn1.Index);
							movementNodes [j] = mn2;
						}
					}
				}
			}
		}
		#endif
	}
}
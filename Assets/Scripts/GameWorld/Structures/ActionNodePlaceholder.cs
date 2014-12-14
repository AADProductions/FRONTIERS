using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionNodePlaceholder : MonoBehaviour
{
		public List <ActionNodePlaceholder> InConnections = new List <ActionNodePlaceholder>();
		public Vector3 HeadPosition;
		public bool SaveToStructure = true;

		void OnDrawGizmos()
		{
				if (InConnections.Count > 0) {
						Gizmos.color = Color.cyan;
						name = "_NavCross_";
				} else {
						Gizmos.color = Color.white;
						name = "_NavIn_";
				}

				if (!SaveToStructure) {
						Gizmos.color = Frontiers.Colors.Alpha(Color.yellow, 0.15f);
				}

				Vector3 startFeet = transform.position;
				HeadPosition = startFeet + (transform.up * 2.0f);
				Gizmos.DrawLine(startFeet, startFeet + (transform.up * 2.0f));
				Gizmos.color = Frontiers.Colors.Alpha(Gizmos.color, Gizmos.color.a * 0.25f);
				Gizmos.DrawSphere(startFeet, 0.25f);
				Gizmos.DrawWireCube(startFeet, new Vector3(0.25f, 0.01f, 0.25f));

				for (int i = 0; i < InConnections.Count; i++) {
						Gizmos.color = Frontiers.Colors.Alpha(Gizmos.color, 0.5f);
						DrawArrow.ForGizmo(InConnections[i].HeadPosition, (HeadPosition - InConnections[i].HeadPosition) * 0.9f, 0.25f, 20f);
				}
		}
}

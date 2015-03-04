using UnityEngine;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Frontiers.World
{
		public class TreeColliderTemplate : MonoBehaviour
		{
				public string SpawnDescription = string.Empty;
				[BitMaskAttribute(typeof(TreeColliderFlags))]
				public TreeColliderFlags MainColliderFlags = TreeColliderFlags.Solid;
				public float MainRadius = 1f;
				public float MainHeight = 1f;
				public Vector3 MainOffset = Vector3.zero;
				[BitMaskAttribute(typeof(TreeColliderFlags))]
				public TreeColliderFlags SecondaryColliderFlags = TreeColliderFlags.Rustle | TreeColliderFlags.Impede;
				public float SecondaryRadius = 1f;
				public float SecondaryHeight = 1f;
				public Vector3 SecondaryOffset = Vector3.zero;
				[FrontiersAvailableModsAttribute("Category")]
				public string SpawnCategory = "Branches";
				public float TotalDamage = 0f;
				[FrontiersAvailableMods("Category")]
				public float SpawnDamageMinimum	= 50.0f;
				public float SpawnDamageMaximum = 0f;
				public bool DepletedOnMaxDamage = false;
				public string IntrospectionOnDepleted;

				public bool HasLowQualitySubstitute {
						get {
								return !string.IsNullOrEmpty(SubstituteOnLowQuality);
						}
				}
				[FrontiersAvailablePlants]
				public string SubstituteOnLowQuality = string.Empty;

				#if UNITY_EDITOR
				public void OnDrawGizmos()
				{
						Gizmos.color = Color.green;
						Handles.color = Color.green;
						Handles.DrawWireDisc(transform.position + MainOffset, Vector3.up, MainRadius);
						Gizmos.DrawLine(transform.position + MainOffset, (transform.position + MainOffset) + (Vector3.up * MainHeight));
						Handles.DrawWireDisc((transform.position + MainOffset) + (Vector3.up * MainHeight), Vector3.up, MainRadius);

						if (SecondaryColliderFlags != TreeColliderFlags.None) {
								Gizmos.color = Color.cyan;
								Handles.color = Color.cyan;
								Handles.DrawWireDisc(transform.position + SecondaryOffset, Vector3.up, SecondaryRadius);
								Gizmos.DrawLine(transform.position + SecondaryOffset, (transform.position + SecondaryOffset) + (Vector3.up * SecondaryHeight));
								Handles.DrawWireDisc((transform.position + SecondaryOffset) + (Vector3.up * SecondaryHeight), Vector3.up, SecondaryRadius);
						}
				}
				#endif
		}
}
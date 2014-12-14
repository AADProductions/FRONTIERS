using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.Data;

public class PilgrimDirectionArrow : MonoBehaviour
{		//for directing characters to paths
		public GameObject ArrowObject;
		public Vector3 Alignment = Vector3.zero;
		public Vector3 TargetScale = Vector3.one;
		public bool HasPlayerFocus = false;
		public PlayerProjections	Projections = null;
		public PathAvatar path = null;
		public PathDirection direction = PathDirection.None;
		public MobileReference start = MobileReference.Empty;
		public MobileReference target = MobileReference.Empty;

		public void Start()
		{
				gameObject.layer = Globals.LayerNumGUIMap;
				transform.rotation = Quaternion.identity;
				Alignment = path.OrientationFromPosition(transform.position, true);

				transform.Rotate(Alignment);
				if (direction == PathDirection.Backwards) {
						ArrowObject.renderer.material.color = Color.red;
				} else {
						transform.Rotate(new Vector3(0f, 180f, 0f));
						ArrowObject.renderer.material.color = Color.green;
				}
		}

		public void Update()
		{
				if (Projections.ArrowInFocus == gameObject) {
						TargetScale = Vector3.one * 1.5f;
				} else {
						TargetScale = Vector3.one;
				}
				ArrowObject.transform.localScale = Vector3.Lerp(ArrowObject.transform.localScale, TargetScale, 0.25f);
		}

		public void OnChooseDirection()
		{
				StartCoroutine(ShrinkAndDestroy());
		}

		protected IEnumerator ShrinkAndDestroy()
		{
				if (Projections.ArrowInFocus == gameObject) {
						TargetScale = Vector3.one * 2.0f;
						yield return new WaitForSeconds(0.1f);
						TargetScale = Vector3.zero;
						yield return new WaitForSeconds(1.0f);
				} else {
						TargetScale = Vector3.zero;
						yield return new WaitForSeconds(1.0f);
				}
				GameObject.Destroy(gameObject, 0.01f);
				yield break;
		}
}

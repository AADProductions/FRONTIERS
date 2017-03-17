using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;

namespace Frontiers {
	[ExecuteInEditMode]
	public class ScaleToFitBounds : MonoBehaviour {

		public WorldItem worlditem;
		public Stackable stackable;
		public Vector3 parentCenter;
		public Vector3 objectCenter;
		public Bounds objectBounds;
		public Bounds parentBounds;
		public float parentMaxScale;
		public float objectMaxScale;
		public float zOffset = -50f;
		public float scaleMultiplier = 0.85f;

		public void Start ( ) 
		{
			worlditem = gameObject.GetComponent <WorldItem> ();
			if (!gameObject.HasComponent <Stackable> (out stackable)) {
				if (Application.isPlaying) {
					GameObject.Destroy (this);
				} else {
					GameObject.DestroyImmediate (this);
				}
			}

			worlditem.transform.localPosition = Vector3.zero;
			worlditem.transform.localScale = Vector3.one;

			parentBounds = transform.parent.GetComponent<Collider>().bounds;
			parentCenter = parentBounds.center;

			objectBounds = new Bounds (worlditem.transform.position, Vector3.zero);
			foreach (Renderer renderer in worlditem.Renderers) {
				objectBounds.Encapsulate (renderer.bounds);
			}
			objectCenter = objectBounds.center;

			parentMaxScale = Mathf.Max (new float [] {
				parentBounds.size.x,
				parentBounds.size.y,
				parentBounds.size.z
			});
			objectMaxScale = Mathf.Max (new float [] {
				objectBounds.size.x,
				objectBounds.size.y,
				objectBounds.size.z
			});

			ScaleToFitParentCollider ();
		}

		public void ScaleToFitParentCollider ( )
		{
			//scale the item so its max size fits within the parent max size
			worlditem.transform.localScale = (Vector3.one * parentMaxScale / objectMaxScale) * scaleMultiplier;
			//now that we've resized it, it will have a different position
			//so recalculate the bounds
			objectBounds = new Bounds (worlditem.transform.position, Vector3.zero);
			foreach (Renderer renderer in worlditem.Renderers) {
				objectBounds.Encapsulate (renderer.bounds);
			}
			objectCenter = objectBounds.center;
			//move the item so its center is the same as the parent center
			Vector3 offset = parentCenter - objectCenter;
			offset.z += zOffset;

			worlditem.transform.localPosition = offset;

			if (Application.isPlaying) {
				GameObject.Destroy (this);
			} else {
				GameObject.DestroyImmediate (this);
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;
using AnimationOrTween;

namespace Frontiers.World
{
		public class TouchTrigger : MonoBehaviour
		{
				public Frontiers.World.WIScripts.Trigger trigger;
				public SphereCollider TouchCollider;
				public Rigidbody rb;
				[BitMaskAttribute(typeof(ItemOfInterestType))]
				public ItemOfInterestType TriggeredBy = ItemOfInterestType.All;
				public IItemOfInterest LastItemOfInterest;

				public void OnEnable()
				{
						TouchCollider.enabled = true;
						if (rb != null) {
								rb.detectCollisions = true;
						}
				}

				public void OnDisable()
				{
						TouchCollider.enabled = false;
						if (rb != null) {
								rb.detectCollisions = false;
						}
				}

				public void OnTriggerEnter(Collider other)
				{
						if (other.isTrigger)
								return;

						if (WorldItems.GetIOIFromCollider(other, out LastItemOfInterest)
						 && Flags.Check((uint)TriggeredBy, (uint)LastItemOfInterest.IOIType, Flags.CheckType.MatchAny)) {
								trigger.TryToTrigger();
						}
				}
		}
}
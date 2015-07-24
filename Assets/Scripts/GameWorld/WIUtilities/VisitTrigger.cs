using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class VisitTrigger : MonoBehaviour
	{
		public Visitable VisitableLocation;
		public SphereCollider VisitableCollider;
		public Rigidbody rb;

		public void Initialize (Visitable visitable)
		{
			VisitableLocation = visitable;
			Location location = null;
			if (visitable.worlditem.Is <Location> (out location)) {
				rb.position = visitable.worlditem.tr.position;
				rb.rotation = visitable.worlditem.tr.rotation;
				VisitableCollider.radius = location.worlditem.ActiveRadius;
				VisitableCollider.isTrigger = true;
			}
		}

		public void Awake ()
		{
			gameObject.layer = Globals.LayerNumLocationBroadcaster;
			rb = gameObject.GetOrAdd <Rigidbody> ();
			rb.isKinematic = true;
			rb.detectCollisions = false;
			if (VisitableCollider == null) {
				VisitableCollider = gameObject.GetOrAdd <SphereCollider> ();
				VisitableCollider.isTrigger = true;
				VisitableCollider.enabled = false;
			}
		}

		public void OnEnable ()
		{
			/*if (VisitableCollider != null) {
				VisitableCollider.enabled = true;
			}*/
			if (rb != null) {
				rb.detectCollisions = true;
			}
		}

		public void OnDisable ()
		{
			/*if (VisitableCollider != null) {
				VisitableCollider.enabled = false;
			}*/
			if (rb != null) {
				rb.detectCollisions = false;
			}
		}

		IItemOfInterest mIoiCheck = null;

		public void OnTriggerEnter (Collider other)
		{
			if (VisitableLocation == null || !VisitableLocation.Initialized)
				return;

			if (other.isTrigger)
				return;

			if (other.gameObject.layer == Globals.LayerNumPlayer) {
				VisitableLocation.PlayerVisit (LocationVisitMethod.ByDefault);
				if (VisitableLocation.PlayerOnly) {
					return;
				}
			}
			//do we care about any world items?
			if (other.gameObject.layer == Globals.LayerNumWorldItemActive && VisitableLocation.ItemsOfInterest.Count > 0) {
				mIoiCheck = null;
				if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem && mIoiCheck.worlditem.HasAtLeastOne (VisitableLocation.ItemsOfInterest)) {
					VisitableLocation.ItemOfInterestVisit (mIoiCheck.worlditem);
				}
			}
		}

		public void OnTriggerExit (Collider other)
		{
			if (VisitableLocation == null || !VisitableLocation.Initialized)
				return;

			if (other.isTrigger)
				return;

			if (other.gameObject.layer == Globals.LayerNumPlayer) {
				VisitableLocation.PlayerLeave ();
				if (VisitableLocation.PlayerOnly) {
					return;
				}
			}
			//do we care about any world items?
			if (other.gameObject.layer == Globals.LayerNumWorldItemActive && VisitableLocation.ItemsOfInterest.Count > 0) {
				mIoiCheck = null;
				if (WorldItems.GetIOIFromCollider (other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem && mIoiCheck.worlditem.HasAtLeastOne (VisitableLocation.ItemsOfInterest)) {
					VisitableLocation.ItemOfInterestLeave (mIoiCheck.worlditem);
				}
			}
		}
	}
}

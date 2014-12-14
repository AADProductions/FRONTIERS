using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
		public class VisitTrigger : MonoBehaviour
		{
				public Visitable VisitableLocation;
				public SphereCollider VisitableCollider;
				public Rigidbody rb;

				public void Initialize(Visitable visitable)
				{
						VisitableLocation = visitable;
						Location location = null;
						if (visitable.worlditem.Is <Location>(out location)) {
								VisitableCollider.radius = location.worlditem.ActiveRadius;
						}
						VisitableCollider.isTrigger = true;
						rb.position = visitable.worlditem.tr.position;
						rb.rotation = visitable.worlditem.tr.rotation;
				}

				public void Awake()
				{
						gameObject.layer = Globals.LayerNumAwarenessBroadcaster;
						if (VisitableCollider == null) {
								VisitableCollider = gameObject.GetOrAdd <SphereCollider>();
						}
						rb = gameObject.GetOrAdd <Rigidbody>();
						rb.isKinematic = true;
						rb.detectCollisions = false;
				}

				public void OnEnable()
				{
						VisitableCollider.enabled = true;
						if (rb != null) {
								rb.detectCollisions = true;
						} else {
								Debug.Log("VISIT TRIGGER " + name + " HAD NO ATTACHED RIGID BODY");
						}
				}

				public void OnDisable()
				{
						VisitableCollider.enabled = false;
						if (rb != null) {
								rb.detectCollisions = false;
						} else {
								Debug.Log("VISIT TRIGGER " + name + " HAD NO ATTACHED RIGID BODY");
						}
				}

				IItemOfInterest mIoiCheck = null;

				public void OnTriggerEnter(Collider other)
				{
						if (other.isTrigger)
								return;

						if (other.gameObject.layer == Globals.LayerNumPlayer) {
								if (VisitableLocation != null) {
										VisitableLocation.PlayerVisit(LocationVisitMethod.ByDefault);
								}
								if (VisitableLocation.PlayerOnly) {
										return;
								}
						}
						//do we care about any world items?
						if (other.gameObject.layer == Globals.LayerNumWorldItemActive && VisitableLocation.ItemsOfInterest.Count > 0) {
								mIoiCheck = null;
								if (WorldItems.GetIOIFromCollider(other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem && mIoiCheck.worlditem.HasAtLeastOne(VisitableLocation.ItemsOfInterest)) {
										VisitableLocation.ItemOfInterestVisit(mIoiCheck.worlditem);
								}
						}
				}

				public void OnTriggerExit(Collider other)
				{
						if (other.isTrigger)
								return;

						if (other.gameObject.layer == Globals.LayerNumPlayer) {
								if (VisitableLocation != null) {
										VisitableLocation.PlayerLeave();
								}
								if (VisitableLocation.PlayerOnly) {
										return;
								}
						}
						//do we care about any world items?
						if (other.gameObject.layer == Globals.LayerNumWorldItemActive && VisitableLocation.ItemsOfInterest.Count > 0) {
								mIoiCheck = null;
								if (WorldItems.GetIOIFromCollider(other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem && mIoiCheck.worlditem.HasAtLeastOne(VisitableLocation.ItemsOfInterest)) {
										VisitableLocation.ItemOfInterestLeave(mIoiCheck.worlditem);
								}
						}
				}
		}
}

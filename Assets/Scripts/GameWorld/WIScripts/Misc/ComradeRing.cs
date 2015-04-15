using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.WIScripts
{
		public class ComradeRing : WIScript
		{
				public float MaxRange = 5f;
				public SphereCollider TriggerCollider;
				public List <ComradeRing> NearbyRings = new List<ComradeRing>();
				public LensFlare GlowingFlare;

				public override bool UnloadWhenStacked {
						get {
								return false;
						}
				}

				public override void OnInitialized()
				{
						//this will ensure that a ring won't exit the max range
						//without being able to re-trigger on entering max range
						TriggerCollider.radius = MaxRange * 0.95f;
				}

				public void OnTriggerEnter(Collider other)
				{
						IItemOfInterest ioi = null;
						ComradeRing otherComradeRing;
						if (WorldItems.GetIOIFromCollider(other, out ioi)) {
								if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.worlditem.Is <ComradeRing>(out otherComradeRing)) {
										NearbyRings.SafeAdd(otherComradeRing);
										worlditem.State = "Glowing";
										GlowingFlare.enabled = true;
										enabled = true;
								}
						}
				}

				public void Update()
				{
						if (!mInitialized) {
								return;
						}

						for (int i = NearbyRings.LastIndex(); i >= 0; i--) {
								if (NearbyRings[i] == null || Vector3.Distance(NearbyRings[i].worlditem.tr.position, worlditem.tr.position) > MaxRange) {
										NearbyRings.RemoveAt(i);
								}
						}

						if (NearbyRings.Count == 0) {
								worlditem.State = "Dormant";
								GlowingFlare.enabled = false;
								enabled = false;
						}
				}
		}
}

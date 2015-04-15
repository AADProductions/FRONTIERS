using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class Lure : WIScript
		{
				public LureState State = new LureState();
				public List <string> TargetCreatures = new List <string>();
				public string DormantState = "Full";
				public string ActiveState = "Active";
				public string EmptyState = "Empty";
				public float RTDuration = 30f;
				public float RTExpansionTime = 10f;
				public float LureRadius;
				public EffectSphere LureEffectSphere;

				public bool Luring {
						get {
								return LureEffectSphere != null && !LureEffectSphere.Depleted;
						}
				}

				public override void OnStateChange()
				{
						if (!worlditem.Is(WIMode.World | WIMode.Frozen | WIMode.Equipped)) {
								//we don't want to do this while dormant
								return;
						}

						if (worlditem.State == ActiveState) {
								if (!Luring) {
										//create the effect sphere
										GameObject effectSphere = gameObject.CreateChild("LureEffectSphere").gameObject;
										LureEffectSphere = effectSphere.AddComponent <EffectSphere>();

										LureEffectSphere.TargetRadius = LureRadius;
										LureEffectSphere.StartTime = WorldClock.AdjustedRealTime;
										LureEffectSphere.RTDuration = RTDuration;
										LureEffectSphere.RTCooldownTime = 0f;
										LureEffectSphere.RTExpansionTime = RTExpansionTime;
										LureEffectSphere.OnIntersectItemOfInterest += OnIntersectItemOfInterest;
										LureEffectSphere.OnDepleted += OnDepleted;
										LureEffectSphere.RequireLineOfSight = false;
								}
						} else {
								//if we've gone empty for some reason
								//and we're currently luring
								//then something has gone wrong, so kill the effect
								if (Luring) {
										LureEffectSphere.CancelEffect();
								}
						}
				}

				public void OnIntersectItemOfInterest()
				{
						//tell the creature to eat this thing
						Creature creature = null;
						IItemOfInterest ioi = null;
						while (LureEffectSphere.ItemsOfInterest.Count > 0) {
								ioi = LureEffectSphere.ItemsOfInterest.Dequeue();
								if (ioi.IOIType == ItemOfInterestType.WorldItem && ioi.worlditem.Is <Creature>(out creature)) {
										if (TargetCreatures.Contains(creature.Template.Name)) {
												creature.EatThing(this.worlditem);
										}
								}
						}
				}

				public void OnDepleted()
				{
						worlditem.State = EmptyState;
				}
		}

		public class LureState
		{
				public float LureStartTime;
		}
}
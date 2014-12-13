using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class Aggressive : WIScript
		{
				//creature script exclusively
				public Creature creature;
				public Photosensitive photosensitive;
				public Hostile hostile;

				public override void OnInitialized()
				{
						creature = worlditem.Get <Creature>();
						creature.OnPlayerLeaveDen += OnPlayerLeaveDen;
						creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
						photosensitive = worlditem.Get <Photosensitive>();
						photosensitive.OnExposureIncrease += FleeFromFire;

						Damageable damageable = worlditem.Get <Damageable>();
						damageable.OnTakeDamage += OnTakeDamage;
						damageable.OnDie += OnDie;
				}

				public void OnCollectiveThoughtStart()
				{
						if (mDestroyed) {
								return;
						}

						if (!creature.IsInDen) {
								creature.CurrentThought.Should(IOIReaction.IgnoreIt);
								return;
						}

						if (worlditem.Is <Hostile>(out hostile)) {
								if (hostile.HasPrimaryTarget &&
								hostile.PrimaryTarget == creature.CurrentThought.CurrentItemOfInterest) {
										//we don't need to think about the new thing
										creature.CurrentThought.Should(IOIReaction.IgnoreIt, 5);
										return;
								}
						}

						creature.Body.EyeMode = BodyEyeMode.Aggressive;
						switch (creature.CurrentThought.CurrentItemOfInterest.IOIType) {
								case ItemOfInterestType.Player:
								default:
										//creature.CurrentThought.Vote = IOIReaction.WatchIt;
										creature.CurrentThought.Should(IOIReaction.KillIt);
										break;

								case ItemOfInterestType.Scenery:
										creature.CurrentThought.Should(IOIReaction.WatchIt);
										break;

								case ItemOfInterestType.WorldItem:
										if (!creature.Den.BelongsToPack(creature.CurrentThought.CurrentItemOfInterest.worlditem)) {
												creature.CurrentThought.Should(IOIReaction.KillIt);
										}
										break;
						}
				}

				public void OnTakeDamage()
				{
						if (mDestroyed) {
								return;
						}

						Damageable damageable = worlditem.Get <Damageable>();
						FightOrFlight fightOrFlightResponse = creature.State.OnTakeDamageAggressive;
					
						if (damageable.NormalizedDamage > creature.State.FightOrFlightThreshold) {
								fightOrFlightResponse = creature.State.OnReachFFThresholdAggressive;
						} else {
								return;
						}

						switch (fightOrFlightResponse) {
								case FightOrFlight.Fight:
										creature.AttackThing(damageable.LastDamageSource);
										break;

								case FightOrFlight.Flee:
								default:
										creature.FleeFromThing(damageable.LastDamageSource);
										break;
						}
				}

				public void OnDie()
				{
						Finish();
				}

				public void OnPlayerVisitDen()
				{
						//if (WorldClock.IsTimeOfDay (creature.State.AggressiveTOD)) {
						////Debug.Log ("Is aggressive time of day, hostile to player");
						//creature.AttackPlayer ();
						//}
				}

				public void OnPlayerLeaveDen()
				{
						if (worlditem.Is <Hostile>(out hostile)) {
								if (hostile.HasPrimaryTarget && hostile.PrimaryTarget == Player.Local) {
										hostile.CoolOff();
								}
						}
				}

				public void OnLeaveDen()
				{
						creature.ReturnToDen();
				}

				public void FleeFromFire()
				{
						//we go through this daisy-chain
						//because we don't want to permanently link up creature's flee from fire
						creature.FleeFromFire();
				}
		}
}
using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class Timid : WIScript
		{
				//creature exclusive script
				public Creature creature;

				public override void OnInitialized()
				{
						creature = worlditem.Get <Creature>();
						creature.OnPlayerLeaveDen += OnPlayerLeaveDen;
						//creature.OnLeaveDen += OnLeaveDen;
						creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;

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

						creature.Body.TargetEyeColor = Color.green;//TODO put this somewhere else
						IItemOfInterest itemOfInterest = creature.CurrentThought.CurrentItemOfInterest;
						switch (itemOfInterest.IOIType) {
								case ItemOfInterestType.Player:
								default:
										if (creature.Template.StateTemplate.Domestication == DomesticatedState.Domesticated) {
												creature.CurrentThought.Should(IOIReaction.WatchIt);
										} else {
												Tamed tamed = null;
												if (worlditem.Is <Tamed>(out tamed) && tamed.State.ImprintedPlayer == itemOfInterest.player.ID) {
														creature.CurrentThought.Should(IOIReaction.IgnoreIt);
												} else {
														if (Player.Local.IsCrouching) {
																creature.CurrentThought.Should(IOIReaction.FleeFromIt);
														} else if (Player.Local.IsWalking) {
																creature.CurrentThought.Should(IOIReaction.FleeFromIt, 3);
														} else if (Player.Local.IsSprinting) {
																creature.CurrentThought.Should(IOIReaction.FleeFromIt, 4);
														} else {
																creature.CurrentThought.Should(IOIReaction.FleeFromIt, 2);
														}
												}
										}
										break;

								case ItemOfInterestType.Scenery:
										creature.CurrentThought.Should(IOIReaction.WatchIt);
										break;

								case ItemOfInterestType.WorldItem:
										if (creature.CurrentThought.CurrentItemOfInterest.worlditem.Is <Creature>() && !creature.Den.BelongsToPack(creature.CurrentThought.CurrentItemOfInterest.worlditem)) {
												creature.CurrentThought.Should(IOIReaction.FleeFromIt);
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
						FightOrFlight fightOrFlightResponse = creature.State.OnTakeDamageTimid;
						if (damageable.NormalizedDamage > creature.State.FightOrFlightThreshold) {
								fightOrFlightResponse = creature.State.OnReachFFThresholdTimid;
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

				public void OnPlayerLeaveDen()
				{
						creature.StopFleeingFromPlayer();
				}

				public void OnLeaveDen()
				{
						creature.ReturnToDen();
				}
		}
}
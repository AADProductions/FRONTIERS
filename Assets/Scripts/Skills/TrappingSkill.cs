using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World.Gameplay
{
		public class TrappingSkill : Skill
		{
				public TrappingSkillExtensions Extensions = new TrappingSkillExtensions();
				public List <ITrap> Traps = new List <ITrap>();
				//trapping skills work by accepting traps of a certain kind to update
				//if a trap object is added it will be updated until it has been unloaded or is no longer Frozen
				public void UpdateTrap(ITrap trap)
				{
						if (Traps.SafeAdd(trap)) {
								trap.SkillUpdating = true;
								trap.TimeLastChecked = WorldClock.AdjustedRealTime;
						}

						if (!mUpdatingTraps) {
								mUpdatingTraps = true;
								StartCoroutine(UpdateTraps());
						}
				}

				protected IEnumerator UpdateTraps()
				{
						while (Traps.Count > 0) {
								for (int i = Traps.LastIndex(); i >= 0; i--) {
										ITrap trap = Traps[i];
										if (trap == null) {
												Traps.RemoveAt(i);
										} else if (trap.IsFinished || trap.Mode != TrapMode.Set || trap.IntersectingDens.Count == 0) {
												trap.SkillUpdating = false;
												Traps.RemoveAt(i);
										} else {
												//okay, the trap is set and it's not finished and it has intersecting dens
												for (int j = trap.IntersectingDens.LastIndex(); j >= 0; j--) {
														ICreatureDen den = trap.IntersectingDens[j];
														if (den == null || den.IsFinished) {
																trap.IntersectingDens.RemoveAt(j);
														} else if (trap.CanCatch.Count > 0 && !trap.CanCatch.Contains(den.NameOfCreature)
														  || trap.Exceptions.Count > 0 && trap.Exceptions.Contains(den.NameOfCreature)) {
																//make sure this trap can actually catch what's in it
																trap.IntersectingDens.RemoveAt(j);
														} else {
																//is it time to check this trap yet? is the player nearby?
																if ((WorldClock.AdjustedRealTime - trap.TimeLastChecked) > Globals.TrappingMinimumRTCheckInterval
																&& Vector3.Distance(Player.Local.Position, trap.Owner.tr.position) > Globals.TrappingMinimumCorpseSpawnDistance) {
																		trap.TimeLastChecked = WorldClock.AdjustedRealTime;
																		//odds of catching something increases over time
																		float oddsOfCatchingSomething = trap.SkillOnSet;
																		double timeSinceSet = WorldClock.AdjustedRealTime - trap.TimeSet;
																		oddsOfCatchingSomething = Mathf.Clamp01((float)(oddsOfCatchingSomething + (Globals.TrappingOddsTimeMultiplier * timeSinceSet)));
																		//okay, figure out how close to the den we are
																		float distanceToCenterOfDen = Vector3.Distance(trap.Owner.tr.position, den.transform.position);
																		float distanceToDen = distanceToCenterOfDen - den.Radius;
																		float trapRadius = Skill.SkillEffectRadius(
																				          Effects.UnskilledEffectRadius,
																				          Effects.SkilledEffectRadius,
																				          Effects.MasteredEffectRadius,
																				          trap.SkillOnSet,//this is the only difference between this & a normal skill check
																				          State.HasBeenMastered);
																		if (trapRadius >= distanceToCenterOfDen) {
																				//wuhoo, huge bonus odds!
																				oddsOfCatchingSomething = Mathf.Clamp01(oddsOfCatchingSomething * Globals.TrappingOddsDistanceMultiplier);
																		} else if (trapRadius >= distanceToDen) {
																				//okay, no huge bonus but still cool
																		} else {
																				oddsOfCatchingSomething = 0f;
																		}

																		if (oddsOfCatchingSomething > 0f && UnityEngine.Random.value < oddsOfCatchingSomething) {
																				float timeSinceDeath = UnityEngine.Random.Range(0f, (float)timeSinceSet);
																				den.SpawnCreatureCorpse(trap.Owner.tr.position, "Trap", timeSinceDeath);
																				trap.Mode = TrapMode.Triggered;
																				//we're done with this trap until it's triggered again
																				Traps.RemoveAt(i);
																		}
																}
														}
												}
										}
										double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 1f;
										while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
												yield return null;
										}
								}
								yield return null;
						}
						mUpdatingTraps = false;
						yield break;
				}

				protected IEnumerator UpdateHuntingTrap()
				{		//TODO remove this it's not necessary any more
						/*
						mUpdatingHuntingTrap = true;
						while (Mode == TrapMode.Set && (worlditem.Is (WIMode.World | WIMode.Frozen))) {
							//Debug.Log ("Updating trap " + name + "...");
							float distance = Vector3.Distance (Player.Local.Position, transform.position);
							if (distance > MinimumTrappingDistance) {
								//if the player is in the viscinity
								State.PlayerInViscinity = false;
								//Debug.Log ("Player is NOT in viscinity in " + name + "...");
							} else {
								if (!State.PlayerInViscinity) {
									//if we're ENTERING the viscinity after having left, then see if we trapped anything
									yield return StartCoroutine (OnPlayerEnterViscinity ());
									State.PlayerInViscinity = true;
								}
							}
							yield return new WaitForSeconds (5.0f);
						}
						mUpdatingHuntingTrap = false;
						*/
						yield break;
				}

				protected bool mUpdatingTraps = false;
		}

		public class TrappingSkillExtensions
		{
				public string CreatureName = string.Empty;
		}
}
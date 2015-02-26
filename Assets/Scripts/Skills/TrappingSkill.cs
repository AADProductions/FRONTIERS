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
		public override float ProgressValue { get { return mNormalizedTweakTimeSoFar; } set { } }

		public override string ProgressMessage { get { return mProgressDialogMessage; } }

		public static int SetTrapFlavor = 0;
		public static int ImproveTrapFlavor = 1;
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
				//Debug.Log("Updating traps: " + Traps.Count.ToString());
				for (int i = Traps.LastIndex(); i >= 0; i--) {
										//Debug.Log("Checking trap");
					ITrap trap = Traps[i];
					if (trap == null) {
						Traps.RemoveAt(i);
					} else if (trap.IsFinished || trap.Mode != TrapMode.Set) {
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
								if ((WorldClock.AdjustedRealTime - trap.TimeLastChecked) > Globals.TrappingMinimumRTCheckInterval) {
									bool readyToCheck = true;
									if (trap.RequiresMinimumPlayerDistance && Vector3.Distance(Player.Local.Position, trap.Owner.tr.position) < Globals.TrappingMinimumCorpseSpawnDistance) {
										readyToCheck = false;
									}
									if (readyToCheck) {
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
											Debug.Log("Caught something at creature den");
											if (den.TrapsSpawnCorpse) {
												float timeSinceDeath = UnityEngine.Random.Range(0f, (float)timeSinceSet);
												den.SpawnCreatureCorpse(trap.Owner.Position + Vector3.up, "Trap", timeSinceDeath);
											}
											//the trap will take care of itself
											trap.OnCatchTarget(State.NormalizedMasteryLevel);
											//we're done with this trap until it's triggered again
											Traps.RemoveAt(i);
										}
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

		public override WIListOption GetListOption(IItemOfInterest targetObject)
		{
			mListOption = base.GetListOption(targetObject);
			mListOption.Flavors.Clear();
			LandTrap landTrap = null;
			WaterTrap waterTrap = null;
			if (targetObject.IOIType == ItemOfInterestType.WorldItem) {
				if (targetObject.worlditem.Is<LandTrap>(out landTrap)) {
					if (landTrap.State.Mode == TrapMode.Set) {
						mListOption.Flavors.Add("Un-set Trap");
						mListOption.Flavors.Add("Improve Setting");
					} else {
						mListOption.Flavors.Add("Set Trap");
					}
				} else if (targetObject.worlditem.Is<WaterTrap>(out waterTrap)) {
					if (waterTrap.State.Mode == TrapMode.Set) {
						mListOption.Flavors.Add("Un-set Trap");
						mListOption.Flavors.Add("Improve Setting");
					} else {
						mListOption.Flavors.Add("Set Trap");
					}
				}
			}
			return mListOption;
		}

		protected override void UseStart(bool forceSuccess)
		{
			if (LastSkillFlavor == ImproveTrapFlavor) {
				//see if we can improve it any more
				ITrap trap = null;
				WaterTrap waterTrap = null;
				LandTrap landTrap = null;
				if (LastSkillTarget.worlditem.Is<LandTrap>(out landTrap)) {
					trap = landTrap;
				}
				if (LastSkillTarget.worlditem.Is<WaterTrap>(out waterTrap)) {
					trap = waterTrap;
				}
				if (trap != null) {
					if (trap.SkillOnSet >= 0.95f) {
						GUI.GUIManager.PostWarning("This trap's setting can't be improved further");
						return;
					}
					mProgressDialogMessage = "Tweaking Trap...";
					GetProgressDialog();
					StartCoroutine(TweakTrapOverTime(trap));
				}
			} else {
				base.UseStart(forceSuccess);
			}
		}

		protected IEnumerator TweakTrapOverTime(ITrap trap)
		{
			double tweakStart = WorldClock.AdjustedRealTime;
			double tweakUntil = tweakStart + 10f;
			while (WorldClock.AdjustedRealTime < tweakUntil) {
				mNormalizedTweakTimeSoFar = (float)((WorldClock.AdjustedRealTime - tweakStart) / (tweakUntil - tweakStart));
				if (mCancelled || trap == null) {
					yield break;
				}
				yield return null;
			}
			if (mProgressDialog != null) {
				mProgressDialog.Finish();
			}
			if (trap != null) {
				GUI.GUIManager.PostSuccess("You have increased this trap's chance of success");
				trap.SkillOnSet = Mathf.Clamp01(trap.SkillOnSet + 0.15f);
			}
			yield break;
		}

		protected bool mUpdatingTraps = false;
		protected string mProgressDialogMessage;
		protected float mNormalizedTweakTimeSoFar;
	}

	public class TrappingSkillExtensions
	{
		public string CreatureName = string.Empty;
	}
}
using UnityEngine;
using System;
using System.Collections;

namespace Frontiers.World.WIScripts
{
		public class OrbSpawner : WIScript
		{
				public OrbSpawnerState State = new OrbSpawnerState();
				public Animation DispenseAnimator;
				public ActionNode SpawnerActionNode;
				public Transform SpawnerParent;
				public string DispenseAnimationName;
				public CreatureDen Den;

				public override void OnInitialized()
				{
						//hide on map
						Revealable revealable = worlditem.Get<Revealable>();
						revealable.State.CustomMapSettings = true;
						revealable.State.IconStyle = MapIconStyle.None;
						revealable.State.LabelStyle = MapLabelStyle.None;

						Den = worlditem.Get <CreatureDen>();
						WorldClock.Get.TimeActions.Subscribe(TimeActionType.DaytimeStart, new ActionListener(DaytimeStart));
						WorldClock.Get.TimeActions.Subscribe(TimeActionType.NightTimeStart, new ActionListener(NightTimeStart));

						if (WorldClock.IsNight) {
								if (!mDispensingOrbs && !mCallingOrbsHome) {
										mDispensingOrbs = true;
										StartCoroutine(DispenseOrbs());
								}
						}
				}

				public bool DaytimeStart(double timeStamp)
				{
						if (!mInitialized) {
								return true;
						}

						if (!mCallingOrbsHome && !mDispensingOrbs) {
								mCallingOrbsHome = true;
								StartCoroutine(CallOrbsHome());
						}
						return true;
				}

				public bool NightTimeStart(double timeStamp)
				{
						if (!mInitialized) {
								return true;
						}

						if (!mDispensingOrbs && !mCallingOrbsHome) {
								mDispensingOrbs = true;
								StartCoroutine(DispenseOrbs());
						}
						return true;
				}

				protected IEnumerator DispenseOrbs()
				{
						Location location = worlditem.Get <Location>();
						while (location.LocationGroup == null || !location.LocationGroup.Is(WIGroupLoadState.Loaded)) {
								double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.5f;
								while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
										yield return null;
								}
								if (worlditem.Is(WIActiveState.Invisible) || !worlditem.Is(WILoadState.Initialized)) {
										yield break;
								}
						}
						while (Den.SpawnedCreatures.Count < State.NumOrbs) {
								if (worlditem.Is(WIActiveState.Invisible) || !worlditem.Is(WILoadState.Initialized)) {
										yield break;
								}
								Creature orb = null;
								Motile motile = null;
								try {
									//spawn an orb and immobilize it
									if (!Creatures.SpawnCreature(Den, location.LocationGroup, SpawnerParent.position, out orb)) {
											yield break;
									}
									motile = orb.worlditem.Get <Motile>();
									motile.IsImmobilized = true;
									//start the animation and move the orb along with the spawner parent object
									//the body will follow it automatically
								}
								catch (Exception e) {
										Debug.LogError(e.ToString());
										yield break;
								}
								yield return null;

								if (DispenseAnimator == null) {
										yield break;
								}

								try {
										DispenseAnimator[DispenseAnimationName].normalizedTime = 0f;
										DispenseAnimator.Play(DispenseAnimationName);
								} catch (Exception e) {
										Debug.LogError(e.ToString());
										yield break;
								}

								while (DispenseAnimator != null && DispenseAnimator[DispenseAnimationName].normalizedTime < 1f) {
										orb.worlditem.tr.position = SpawnerParent.position;
										orb.worlditem.tr.rotation = SpawnerParent.rotation;
										yield return null;
								}
								//now release the orb and let it do whatever
								motile.IsImmobilized = false;
								yield return null;
								if (DispenseAnimator == null) {
										yield break;
								} else {
										DispenseAnimator.Stop();
								}
						}
						mDispensingOrbs = false;
						yield break;
				}

				protected IEnumerator CallOrbsHome()
				{
						mCallingOrbsHome = false;
						yield break;
				}

				protected bool mCallingOrbsHome = false;
				protected bool mDispensingOrbs = false;
		}

		[Serializable]
		public class OrbSpawnerState
		{
				public int NumOrbs = 2;
		}
}
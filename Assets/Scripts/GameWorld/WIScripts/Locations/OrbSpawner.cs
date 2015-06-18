using UnityEngine;
using System;
using System.Collections;

namespace Frontiers.World.WIScripts
{
	public class OrbSpawner : WIScript
	{
		public OrbSpawnerState State = new OrbSpawnerState ();
		public ParticleSystem SandGeyser;
		public ParticleSystem Vapor;
		public ActionNode SpawnerActionNode;
		public Transform SpawnerParent;
		public string DispenseAnimationName;
		public CreatureDen Den;

		public override void OnInitialized ()
		{
			//hide on map
			Revealable revealable = worlditem.Get<Revealable> ();
			revealable.State.CustomMapSettings = true;
			revealable.State.IconStyle = MapIconStyle.None;
			revealable.State.LabelStyle = MapLabelStyle.None;

			Den = worlditem.Get <CreatureDen> ();
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.NightTimeStart, new ActionListener (NightTimeStart));
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.DaytimeStart, new ActionListener (DaytimeStart));

			if (WorldClock.IsNight) {
				if (!mDispensingOrbs) {
					mDispensingOrbs = true;
					StartCoroutine (DispenseOrbs ());
				}
			}
		}

		public bool NightTimeStart (double timeStamp)
		{
			if (!mInitialized) {
				return true;
			}

			if (!mDispensingOrbs && !mDispensedOrbsTonight) {
				mDispensingOrbs = true;
				StartCoroutine (DispenseOrbs ());
			}
			return true;
		}

		public bool DaytimeStart (double timeStamp)
		{
			mDispensedOrbsTonight = false;
			return true;
		}

		protected IEnumerator DispenseOrbs ()
		{
			Location location = worlditem.Get <Location> ();
			while (location.LocationGroup == null || !location.LocationGroup.Is (WIGroupLoadState.Loaded)) {
				double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.5f;
				while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
			}

			while (Den.SpawnedCreatures.Count < State.NumOrbs) {
				Creature newCreature = null;
				if (!Creatures.SpawnCreature (Den, location.LocationGroup, Vector3.zero, out newCreature)) {
					Debug.Log ("Couldn't spawn orb");
					SandGeyser.enableEmission = false;
					Vapor.enableEmission = false;
					mDispensingOrbs = false;
					yield break;
				}

				newCreature.worlditem.tr.position = worlditem.Position + (Vector3.up * 2f);

				SandGeyser.enableEmission = true;
				Vapor.enableEmission = true;
				double waitUntil = WorldClock.AdjustedRealTime + 3f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
				SandGeyser.enableEmission = false;
				waitUntil = WorldClock.AdjustedRealTime + 3f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
				Vapor.enableEmission = false;
			}
			SandGeyser.enableEmission = false;
			Vapor.enableEmission = false;
			mDispensingOrbs = false;
			mDispensedOrbsTonight = true;
			yield break;
		}

		protected bool mDispensingOrbs = false;
		protected bool mDispensedOrbsTonight = false;
	}

	[Serializable]
	public class OrbSpawnerState
	{
		public int NumOrbs = 2;
	}
}
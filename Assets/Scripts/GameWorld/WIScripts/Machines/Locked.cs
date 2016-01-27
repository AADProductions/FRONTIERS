using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.WIScripts
{
	public class Locked : WIScript
	{
		//intercepts trigger attempts
		public Dynamic dynamic = null;
		public LockedState State = new LockedState ();

		public override void OnInitialized ()
		{
			dynamic = worlditem.Get <Dynamic> ();
			dynamic.OnTriggersLoaded += OnTriggersLoaded;
			Trigger trigger = null;
			if (worlditem.Is <Trigger> (out trigger)) {
				trigger.OnTriggerTryToStart += OnTriggerTryToStart;
			}
		}

		public void OnTriggersLoaded ()
		{

		}

		public void PickLock ()
		{		//TODO see if we still need this - do we still have lockpick skill?
			if (State.CanBePicked) {
				if (UnityEngine.Random.value <= State.LockpickDifficulty) {
					State.HasBeenUnlocked = true;
					GUI.GUIManager.PostSuccess ("Picked lock");
				} else {
					GUI.GUIManager.PostWarning ("Failed to pick lock");
				}
			}
		}

		public void OnTriggerTryToStart (Trigger trigger)
		{
			string failureMessage = string.Empty;
			if (State.HasBeenUnlocked)
				return;

			if (State.TimedLock && !WorldClock.IsTimeOfDay (State.TimesLocked)) {
				return;
			}

			string keyName = string.Empty;
			if (Player.Local.Inventory.HasKey (State.KeyType, State.KeyTag, out keyName)) {
				if (!State.HasBeenUnlocked) {
					State.HasBeenUnlocked = true;
					GUI.GUIManager.PostSuccess ("Unlocked with " + keyName);
				}
				return;
			}

			failureMessage = "[Target] is locked";
			trigger.TriggerFail (dynamic, failureMessage);
		}

		[Serializable]
		public class LockedState
		{
			public bool HasBeenUnlocked = false;
			//type of lock
			//these can be combined
			public bool TimedLock = false;
			public bool SkillLock = false;
			public bool KeyLock = false;
			[BitMask(typeof(TimeOfDay))]
			public TimeOfDay
				TimesLocked = TimeOfDay.cd_QuarterNight;
			public string RequiredSkill = string.Empty;
			public string KeyType = string.Empty;
			public string KeyTag = string.Empty;
			//how to get around lock
			public bool CanBePicked = true;
			public float LockpickDifficulty = 0.5f;
		}
	}
}
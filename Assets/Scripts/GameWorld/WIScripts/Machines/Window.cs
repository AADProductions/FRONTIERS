using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World.WIScripts
{
	public class Window : WIScript
	{
		public GameObject PivotObject;
		public WindowState State = new WindowState();
		public Dynamic dynamic = null;
		public PassThroughTriggerPair ClimbThroughInner;
		public PassThroughTriggerPair ClimbThroughOuter;
		public string ErrorMessage;
		public bool IsGeneric = false;

		public override void OnInitialized()
		{
			//doors always reset when they're loaded
			//locks and other trigger items do not
			State.CurrentState = EntranceState.Closed;
			State.TargetState = EntranceState.Closed;

			dynamic = worlditem.Get <Dynamic>();
			if (State.OuterEntrance) {
				dynamic.State.Type = WorldStructureObjectType.OuterEntrance;
			} else {
				dynamic.State.Type = WorldStructureObjectType.InnerEntrance;
			}
			dynamic.OnTriggersLoaded += OnTriggersLoaded;
			//GameWorld.Get.AddGraphNode (new TerrainNode (transform.position));

			if (dynamic.State.Type == WorldStructureObjectType.OuterEntrance) {
				Damageable damageable = null;
				if (worlditem.Is <Damageable> (out damageable)) {
					damageable.OnDie += OnDie;
					damageable.OnTakeDamage += OnTakeDamage;
				}
			}
		}

		public void OnTakeDamage ( ) {
			//add the structure's interior to load in case we break through
			if (dynamic.ParentStructure != null) {
				Structures.AddInteriorToLoad (dynamic.ParentStructure);
			}
		}

		public void OnDie ( ) {
			//force the interior to load since we're now missing a window forever
			if (dynamic.ParentStructure != null) {
				dynamic.ParentStructure.State.ForceBuildInterior = true;
				Structures.AddInteriorToLoad (dynamic.ParentStructure);
			}
		}

		public override int OnRefreshHud(int lastHudPriority)
		{
			lastHudPriority++;
			switch (State.CurrentState) {
				case EntranceState.Closed:
					GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Open", worlditem.HudTarget, GameManager.Get.GameCamera);
					break;

				case EntranceState.Open:
					GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Close", worlditem.HudTarget, GameManager.Get.GameCamera);
					break;

				default:
					break;
			}
			return lastHudPriority;
		}

		public void OnTriggersLoaded()
		{
			bool lockTriggers = false;
			TimeOfDay lockTod = dynamic.ParentStructure.State.GenericEntrancesLockedTimes;
			//if this is a generic door
			//then check if the structure expects generic doors to be locked during times of day
			if (IsGeneric && lockTod != TimeOfDay.a_None) {
				lockTriggers = true;
				lockTod = dynamic.ParentStructure.State.GenericEntrancesLockedTimes;
			}
			Trigger trigger = null;
			List <Trigger> triggersToCheck = new List <Trigger>(dynamic.Triggers);
			//check this just in case we don't have an 'officially' registered trigger
			if (worlditem.Is <Trigger>(out trigger)) {
				trigger.ActionDescription = "Open";
				triggersToCheck.SafeAdd(trigger);
			}

			for (int i = 0; i < triggersToCheck.Count; i++) {
				trigger = triggersToCheck[i];
				trigger.ActionDescription = "Open";
				if (lockTriggers) {
					Locked locked = trigger.worlditem.GetOrAdd <Locked>();
					locked.State.TimedLock = true;
					locked.State.TimesLocked = lockTod;
				}
				trigger.OnTriggerStart += OnTriggerStart;
			}
		}

		public IEnumerator ForceClose()
		{
			if (State.IsOpen) {
				var changeWindowState = ChangingWindowState(EntranceState.Closed);
				while (changeWindowState.MoveNext()) {
					yield return changeWindowState.Current;
				}
			}
			yield break;
		}

		public void OnTriggerStart(Trigger source)
		{
			EntranceState newState = EntranceState.Closed;
			switch (State.CurrentState) {
				case EntranceState.Open:
				case EntranceState.Opening:
					newState = EntranceState.Closed;
					break;
				
				case EntranceState.Closed:
				case EntranceState.Closing:
					newState = EntranceState.Open;
					break;
			}
			if (!mChangingWindowState) {
				StartCoroutine(ChangingWindowState(newState));
			}
		}

		protected IEnumerator ChangingWindowState(EntranceState newTargetState)
		{
			mChangingWindowState = true;
			State.TargetState = newTargetState;
			mStateLastFrame = State.CurrentState;
			while (State.CurrentState != State.TargetState) {
				switch (State.CurrentState) {
					case EntranceState.Closed:
					default:
						if (mStateLastFrame != EntranceState.Closed) {	//if we weren't closed last frame, we just finished the trigger
							MasterAudio.PlaySound(State.SoundType, transform, State.SoundOnClose);
						}
						if (State.TargetState == EntranceState.Open) {
							var onOpen = dynamic.ParentStructure.OnWindowOpen(this);
							while (onOpen.MoveNext()) {
								yield return onOpen.Current;
							}
							State.CurrentState = EntranceState.Opening;
							PivotObject.GetComponent<Animation>().Play(State.AnimationOpening);
							MasterAudio.PlaySound(State.SoundType, transform, State.SoundOnOpen);
						}
						break;

					case EntranceState.Open:
						if (State.TargetState == EntranceState.Closed) {
							PivotObject.GetComponent<Animation>().Play(State.AnimationClosing);
							State.CurrentState = EntranceState.Closing;
						}
						break;

					case EntranceState.Opening:
						if (PivotObject.GetComponent<Animation>()[State.AnimationOpening].normalizedTime > 1f) {
							State.CurrentState = EntranceState.Open;
						}
						break;

					case EntranceState.Closing:
						if (PivotObject.GetComponent<Animation>()[State.AnimationClosing].normalizedTime > 1f) {
							State.CurrentState = EntranceState.Closed;
						}
						break;
				}
				yield return null;
			}
			if (State.TargetState == EntranceState.Closed) {
				dynamic.ParentStructure.OnWindowClose(this);
			}
			mChangingWindowState = false;
			yield break;
		}

		public override void PopulateOptionsList(System.Collections.Generic.List<WIListOption> options, List <string> message)
		{
			if (mChangingWindowState | mClimbingThroughWindow) {
				return;
			}

			if (State.CurrentState == EntranceState.Open) {
				if (!State.IsBlocked) {
					options.Add(new WIListOption("Climb Through"));
				}
				options.Add(new WIListOption("Close"));
			} else {
				options.Add(new WIListOption("Open"));
			}
		}

		protected IEnumerator ClimbThroughWindow()
		{
			Vector3 startPosition = Player.Local.Position;
			Vector3 endPosition = Player.Local.Position;
			//figure out which one we're starting in
			if (ClimbThroughInner.IsIntersecting) {
				endPosition = ClimbThroughOuter.GetComponent<Collider>().bounds.center;
			} else if (ClimbThroughOuter.IsIntersecting) {
				endPosition = ClimbThroughInner.GetComponent<Collider>().bounds.center;
			} else {
				//if we're not intersecting either, get the closest
				float distanceToInner = Vector3.Distance(Player.Local.Position, ClimbThroughInner.transform.position);
				float distanceToOuter = Vector3.Distance(Player.Local.Position, ClimbThroughOuter.transform.position);
				if (distanceToInner < distanceToOuter) {
					endPosition = ClimbThroughOuter.GetComponent<Collider>().bounds.center;
				} else {
					endPosition = ClimbThroughInner.GetComponent<Collider>().bounds.center;
				}
			}

			if (gClimbThroughWindowTransform == null) {
				gClimbThroughWindowTransform = new GameObject("Window Climbing Helper").transform;
				gClimbThroughWindowTarget = new GameObject("Window Climbing Helper Target").transform;
			}

			mStartTime = WorldClock.AdjustedRealTime;
			mEndTime = WorldClock.AdjustedRealTime + gClimbThoughWindowTime;
			gClimbThroughWindowTarget.position = endPosition - Vector3.up;

			Player.Local.HijackControl();
			float normalizedClimbTime = 0f;
			while (normalizedClimbTime < 1f) {
				normalizedClimbTime = (float)((WorldClock.AdjustedRealTime - mStartTime) / (mEndTime - mStartTime));
				gClimbThroughWindowTransform.position = Vector3.Lerp(startPosition + Player.Local.Height, endPosition, normalizedClimbTime);
				Player.Local.SetHijackTargets(gClimbThroughWindowTransform, gClimbThroughWindowTarget);
				yield return null;
			}
			Player.Local.RestoreControl(true);
			Player.Local.Position = endPosition;

			dynamic.OnPassThrough();

			mClimbingThroughWindow = false;
			yield break;
		}

		public void OnPlayerUseWorldItemSecondary(object secondaryResult)
		{
			WIListResult dialogResult = secondaryResult as WIListResult;
			switch (dialogResult.SecondaryResult) {

				case "Open":
				case "Close":
				default:
					Trigger trigger = null;
					if (worlditem.Is <Trigger>(out trigger)) {
						trigger.TryToTrigger();
					}
					break;

				case "Climb Through":
					if (!mClimbingThroughWindow) {
						mClimbingThroughWindow = true;
						StartCoroutine(ClimbThroughWindow());
					}
					break;
			}
		}

		protected static double gClimbThoughWindowTime = 1f;
		protected static Transform gClimbThroughWindowTransform = null;
		protected static Transform gClimbThroughWindowTarget = null;
		protected EntranceState mStateLastFrame = EntranceState.Closed;
		protected bool mChangingWindowState = false;
		protected PassThroughTriggerPair mTargetTriggerPair = null;
		protected PassThroughTriggerPair mStartTriggerPair = null;
		protected double mStartTime = 0f;
		protected double mEndTime = 0f;
		protected bool mClimbingThroughWindow = false;

		public int CalculateLocalValue(int baseValue, IWIBase item)
		{
			if (item == null)
				return baseValue;

			//windows are pretty valuable
			return baseValue + Globals.BaseValueGold;
		}
	}

	[Serializable]
	public class WindowState
	{
		public bool IsOpen {
			get {
				return CurrentState == EntranceState.Open;
			}
		}

		public EntranceState TargetState = EntranceState.Closed;
		public EntranceState CurrentState = EntranceState.Closed;
		public bool OuterEntrance = true;
		public bool IsBlocked = false;
		public MasterAudio.SoundType SoundType = MasterAudio.SoundType.DoorsAndWindows;
		public string SoundOnClose = string.Empty;
		public string SoundOnOpen = string.Empty;
		public string SoundOnFail = string.Empty;
		public string SoundDuringOpening = string.Empty;
		public string SoundDuringClosing = string.Empty;
		public string AnimationOpening	= string.Empty;
		public string AnimationClosing	= string.Empty;
		public string AnimationOnClosed	= string.Empty;
		public string AnimationOnOpen = string.Empty;
	}
}

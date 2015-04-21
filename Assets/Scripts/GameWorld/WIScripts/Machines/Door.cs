using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class Door : WIScript
		{
				public GameObject PivotObject;
				public DoorState State = new DoorState();
				public Dynamic dynamic = null;
				public bool IsGeneric = false;
				public string ErrorMessage;

				public override void OnInitialized()
				{
						//doors always reset when they're loaded
						//locks and other trigger items do not
						State.CurrentState = EntranceState.Closed;
						State.TargetState = EntranceState.Closed;

						dynamic = worlditem.Get <Dynamic>();
						if (worlditem.Group.Props.Interior) {
								dynamic.State.Type = WorldStructureObjectType.InnerEntrance;
						} else {
								dynamic.State.Type = WorldStructureObjectType.OuterEntrance;
						}

						dynamic.OnTriggersLoaded += OnTriggersLoaded;
						if (dynamic.TriggersLoaded) {
								OnTriggersLoaded();
						}
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						if (!worlditem.Is <Trigger>()) {
								return lastHudPriority;
						}

						lastHudPriority++;
						switch (State.CurrentState) {
								case EntranceState.Closed:
										GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Open", Player.Local.FocusObject, GameManager.Get.GameCamera);
										break;

								case EntranceState.Open:
										GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Close", Player.Local.FocusObject, GameManager.Get.GameCamera);
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
						//TODO see if this is really necessary any more
						//if (worlditem.Is <Trigger> (out trigger)) {
						//	triggersToCheck.SafeAdd (trigger);
						//}

						for (int i = 0; i < triggersToCheck.Count; i++) {
								trigger = triggersToCheck[i];
								trigger.ActionDescription = "Open";
								if (lockTriggers) {
										Locked locked = trigger.worlditem.GetOrAdd <Locked>();
										locked.State.TimedLock = true;
										locked.State.TimesLocked = lockTod;
								}
								if (trigger.OnTriggerStart == null) {
										trigger.OnTriggerStart += OnTriggerStart;
								}
						}
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
						if (!mChangingDoorState) {
								StartCoroutine(ChangingDoorState(newState));
						}
				}

				public override void PopulateOptionsList(System.Collections.Generic.List<WIListOption> options, List <string> message)
				{
						if (mChangingDoorState) {
								return;
						}

						if (State.CurrentState == EntranceState.Closed) {
								if (worlditem.Is <Trigger>()) {
										if (State.OuterEntrance) {
												//if the structure is a residence
												//we can knock on it
												if (dynamic.ParentStructure.worlditem.Is <Residence>(out gCheckResidence)) {
														options.Add(new WIListOption("Knock"));
												}
										}
										options.Add(new WIListOption("Open"));
								}
						} else {
								if (worlditem.Is <Trigger>()) {
										options.Add(new WIListOption("Close"));
								}
						}
				}

				protected static Residence gCheckResidence = null;

				public IEnumerator ForceClose()
				{
						if (State.IsOpen) {
								var changeDoorState = ChangingDoorState(EntranceState.Closed);
								while (changeDoorState.MoveNext()) {
										yield return changeDoorState.Current;
								}
						}
						yield break;
				}

				protected IEnumerator ChangingDoorState(EntranceState newTargetState)
				{
						mChangingDoorState = true;
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
														var onOpen = dynamic.ParentStructure.OnDoorOpen(this);
														while (onOpen.MoveNext()) {
																yield return onOpen.Current;
														}
														State.CurrentState = EntranceState.Opening;
														PivotObject.animation.Play(State.AnimationOpening);
														MasterAudio.PlaySound(State.SoundType, transform, State.SoundOnOpen);
												}
												break;

										case EntranceState.Open:
												if (State.TargetState == EntranceState.Closed) {
														PivotObject.animation.Play(State.AnimationClosing);
														State.CurrentState = EntranceState.Closing;
												}
												break;

										case EntranceState.Opening:
												if (PivotObject.animation[State.AnimationOpening].normalizedTime > 1f) {
														State.CurrentState = EntranceState.Open;
												}
												break;

										case EntranceState.Closing:
												if (PivotObject.animation[State.AnimationClosing].normalizedTime > 1f) {
														State.CurrentState = EntranceState.Closed;
												}
												break;
								}
								yield return null;
						}
						if (State.TargetState == EntranceState.Closed) {
								dynamic.ParentStructure.OnDoorClose(this);
						}
						mChangingDoorState = false;
						yield break;
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;
						switch (dialogResult.SecondaryResult) {

								case "Open":
								case "Close":
										Trigger trigger = null;
										if (worlditem.Is <Trigger>(out trigger)) {
												trigger.TryToTrigger();
										}
										break;

								case "Knock":
										if (dynamic.ParentStructure.worlditem.Is <Residence>(out gCheckResidence)) {
												Debug.Log("Found residence, knocking now");
												gCheckResidence.Knock();
										}
										break;

								default:
										break;
						}
				}

				protected EntranceState mStateLastFrame = EntranceState.Closed;
				protected bool mChangingDoorState	= false;

				public int CalculateLocalValue (int baseValue, IWIBase item) {
						if (item == null)
								return baseValue;

						//doors are pretty valuable
						return baseValue + Globals.BaseValueGold;
				}
		}

		[Serializable]
		public class DoorState
		{
				public bool IsOpen {
						get {
								return CurrentState == EntranceState.Open;
						}
				}

				public EntranceState TargetState = EntranceState.Closed;
				public EntranceState CurrentState = EntranceState.Closed;
				public bool OuterEntrance = true;
				public MasterAudio.SoundType SoundType = MasterAudio.SoundType.DoorsAndWindows;
				public string SoundOnClose = string.Empty;
				public string SoundOnOpen = string.Empty;
				public string SoundOnFail = string.Empty;
				public string SoundDuringOpening = string.Empty;
				public string SoundDuringClosing = string.Empty;
				public string AnimationOpening = string.Empty;
				public string AnimationClosing = string.Empty;
				public string AnimationOnClosed	= string.Empty;
				public string AnimationOnOpen = string.Empty;
		}
}
using UnityEngine;
using System;
using System.Collections;
using Frontiers;
using System.Collections.Generic;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Elevator : WIScript
		{
				public GameObject PivotObject;
				Dynamic dynamic = null;
				public ElevatorState State = new ElevatorState();

				public override void OnInitialized()
				{
						Dynamic dynamic = worlditem.Get <Dynamic>();
						dynamic.State.Type = WorldStructureObjectType.Machine;
						dynamic.OnTriggersLoaded += OnTriggersLoaded;
				}

				public void OnTriggersLoaded()
				{
						try {
								Dynamic dynamic = worlditem.Get <Dynamic>();
								for (int i = 0; i < dynamic.Triggers.Count; i++) {
										if (dynamic.Triggers[i] != null) {
												dynamic.Triggers[i].OnTriggerStart += OnTriggerStart;
										}
								}
						} catch (Exception e) {
								Debug.LogException(e);
						}
				}

				public void OnTriggerStart(Trigger source)
				{
						PlatformState newState = PlatformState.Down;
						switch (State.CurrentState) {
								case PlatformState.Up:
								case PlatformState.GoingUp:
										newState = PlatformState.Down;
										break;

								case PlatformState.Down:
								case PlatformState.GoingDown:
										newState = PlatformState.Up;
										break;
						}
						if (!mChangingElevatorState) {
								StartCoroutine(ChangingElevatorState(newState));
						}
				}

				protected IEnumerator ChangingElevatorState(PlatformState newTargetState)
				{
						mChangingElevatorState = true;
						State.TargetState = newTargetState;
						mStateLastFrame = State.CurrentState;
						while (State.CurrentState != State.TargetState) {
								switch (State.CurrentState) {
										case PlatformState.Down:
										default:
												if (mStateLastFrame != PlatformState.Down) {//if we weren't closed last frame, we just finished the trigger
														MasterAudio.PlaySound(State.SoundType, transform, State.SoundOnReachState);
												}
												if (State.TargetState == PlatformState.Up) {
														State.CurrentState = PlatformState.GoingUp;
														PivotObject.animation.Play(State.AnimationGoingUp);
														MasterAudio.PlaySound(State.SoundType, transform, State.SoundOnReachState);
												}
												break;

										case PlatformState.Up:
												if (State.TargetState == PlatformState.Down) {
														PivotObject.animation.Play(State.AnimationGoingDown);
														State.CurrentState = PlatformState.GoingDown;
												}
												break;

										case PlatformState.GoingUp:
												if (PivotObject.animation[State.AnimationGoingUp].normalizedTime > 1f) {
														State.CurrentState = PlatformState.Up;
												}
												break;

										case PlatformState.GoingDown:
												if (PivotObject.animation[State.AnimationGoingDown].normalizedTime > 1f) {
														State.CurrentState = PlatformState.Down;
												}
												break;
								}
								yield return null;
						}
						mChangingElevatorState = false;
						yield break;
				}

				protected PlatformState mStateLastFrame = PlatformState.Down;
				protected bool mChangingElevatorState	= false;
		}

		[Serializable]
		public class ElevatorState
		{
				public bool IsUp {
						get {
								return CurrentState == PlatformState.Up;
						}
				}

				public PlatformState TargetState = PlatformState.Down;
				public PlatformState CurrentState = PlatformState.Down;
				public MasterAudio.SoundType SoundType = MasterAudio.SoundType.Machines;
				public string SoundOnSeekState = string.Empty;
				public string SoundOnReachState = string.Empty;
				public string SoundOnFail = string.Empty;
				public string AnimationGoingUp = string.Empty;
				public string AnimationGoingDown = string.Empty;
		}
}
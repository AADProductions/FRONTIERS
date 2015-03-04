using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World {
	public class HiddenCompartment : WIScript {
		public GameObject PivotObject;
		public HiddenCompartmentState State = new HiddenCompartmentState ();
		public Receptacle KeyReceptacle;

		public override void OnInitialized ()
		{
			KeyReceptacle = worlditem.Get <Receptacle> ();
			KeyReceptacle.OnItemPlacedInReceptacle += OnKeyItemChanged;
			KeyReceptacle.OnItemRemovedFromReceptacle += OnKeyItemChanged;
		}

		public void OnKeyItemChanged ( ) {
			if (CheckKeyItemRequirements ( )) {
				OpenCompartment ();
			} else {
				CloseCompartment ();
			}
		}

		public bool CheckKeyItemRequirements () {
			bool requirementsMet = false;
			for (int i = 0; i < KeyReceptacle.Pivots.Count; i++) {
				ReceptaclePivot pivot = KeyReceptacle.Pivots [i];
				if (pivot.IsOccupied) {
					if (pivot.Occupant.PackName == State.KeyItemRequirements.PackName
						&& pivot.Occupant.PrefabName == State.KeyItemRequirements.PrefabName
						&& (string.IsNullOrEmpty (State.KeyItemRequirements.State) || pivot.Occupant.State == State.KeyItemRequirements.State)
						&& (string.IsNullOrEmpty (State.QuestNameRequirement) || pivot.Occupant.QuestName == State.QuestNameRequirement)) {
						requirementsMet = true;
						break;
					} else {
						Debug.Log ("Occupant " + pivot.Occupant.DisplayName + " doesn't meet requirements " + State.KeyItemRequirements.ToString ());
					}
				}
			}
			return requirementsMet;
		}

		public void CloseCompartment ( ) {
			switch (State.CurrentState) {
			case EntranceState.Open:
			case EntranceState.Opening:
				if (!mChangingState) {
					StartCoroutine (ChangingState (EntranceState.Closed));
				} else {
					StartCoroutine (WaitToChangeState (EntranceState.Closed));
				}
				break;

			case EntranceState.Closed:
			case EntranceState.Closing:
				break;
			}
		}

		public void OpenCompartment ( ) {
			EntranceState newState = EntranceState.Open;
			switch (State.CurrentState) {
			case EntranceState.Open:
			case EntranceState.Opening:
				break;

			case EntranceState.Closed:
			case EntranceState.Closing:
				if (!mChangingState) {
					StartCoroutine (ChangingState (EntranceState.Open));
				} else {
					StartCoroutine (WaitToChangeState (EntranceState.Open));
				}
				break;
			}
		}

		protected IEnumerator ChangingState (EntranceState newTargetState)
		{
			mChangingState = true;
			State.TargetState = newTargetState;
			mStateLastFrame = State.CurrentState;
			////Debug.Log ("Going for state " + newTargetState.ToString () + " from state " + State.CurrentState.ToString ());
			while (State.CurrentState != State.TargetState) {
				switch (State.CurrentState) {
				case EntranceState.Closed:
				default:
					if (mStateLastFrame != EntranceState.Closed) {	//if we weren't closed last frame, we just finished the trigger
						MasterAudio.PlaySound (State.SoundType, transform, State.SoundOnClose);
					}
					if (State.TargetState == EntranceState.Open) {
						State.CurrentState = EntranceState.Opening;
						PivotObject.animation.Play (State.AnimationOpening);
						MasterAudio.PlaySound (State.SoundType, transform, State.SoundOnOpen);
					}
					break;

				case EntranceState.Open:
					if (State.TargetState == EntranceState.Closed) {
						PivotObject.animation.Play (State.AnimationClosing);
						State.CurrentState = EntranceState.Closing;
					}
					break;

				case EntranceState.Opening:
					if (PivotObject.animation [State.AnimationOpening].normalizedTime > 1f) {
						State.CurrentState = EntranceState.Open;
					}
					break;

				case EntranceState.Closing:
					if (PivotObject.animation [State.AnimationClosing].normalizedTime > 1f) {
						State.CurrentState = EntranceState.Closed;
					}
					break;
				}
				yield return null;
			}
			mChangingState = false;
			yield break;
		}

		protected IEnumerator WaitToChangeState (EntranceState newTargetState) {
			while (mChangingState) {
				yield return null;
			}
			StartCoroutine (ChangingState (newTargetState));
			yield break;
		}

		protected EntranceState mStateLastFrame = EntranceState.Closed;
		protected bool mChangingState = false;
	}

	[Serializable]
	public class HiddenCompartmentState {
		public bool IsOpen {
			get {
				return CurrentState == EntranceState.Open;
			}
		}

		public EntranceState TargetState = EntranceState.Closed;
		public EntranceState CurrentState = EntranceState.Closed;
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

		public GenericWorldItem KeyItemRequirements = new GenericWorldItem ();
		public string QuestNameRequirement;
	}
}
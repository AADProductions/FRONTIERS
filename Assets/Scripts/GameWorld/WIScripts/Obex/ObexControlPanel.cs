using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
	public class ObexControlPanel : WIScript
	{
		public ObexControlPanelState State = new ObexControlPanelState ();
		public LuminitePowered PowerSource;
		public TowerElevator TargetElevator;
		public bool HasUpperFloor {
			get { 
				if (!FindElevator ()) {
					return false;
				}
				return State.StopIndex < TargetElevator.MaxFloorIndex;
			}
		}
		public bool HasLowerFloor {
			get {
				if (!FindElevator ()) {
					return false;
				}
				return State.StopIndex > 0;
			}
		}
		public GameObject ElevatorDisplay;
		public AudioSource Audio;
		public Transform LuminiteGemPivot;

		public override void OnInitialized ()
		{
			PowerSource = worlditem.GetOrAdd <LuminitePowered> ();
			PowerSource.OnLosePower += OnLosePower;
			PowerSource.OnRestorePower += OnRestorePower;
			PowerSource.OnPowerSourceRemoved += OnLosePower;
			PowerSource.PowerSourceDopplegangerProps.CopyFrom (Orb.OrbGemGenericWorldItem);
			PowerSource.FXOnRestorePower = "ShieldEffectSubtleGold";
			PowerSource.FXOnLosePower = "RipEffect";
			PowerSource.FXOnPowerSourceRemoved = "ShieldEffectSubtleGold";
			PowerSource.PowerSourceDopplegangerParent = LuminiteGemPivot;
			PowerSource.PowerAudio = Audio;
			PowerSource.CanRemoveSource = true;
			PowerSource.CanReplaceSource = true;
			PowerSource.Refresh ();
		}

		public void OnLosePower () {
			ElevatorDisplay.SetActive (false);
			Audio.Stop ();
		}

		public void OnRestorePower () {
			ElevatorDisplay.SetActive (true);
			Audio.Play ();
		}

		public override void PopulateOptionsList (List<WIListOption> options, List<string> message)
		{
			if (!FindElevator ()) {
				return;
			}

			if (gElevatorUpOption == null) {
				gElevatorUpOption = new WIListOption ("Elevator Up", "Up");
				//gElevatorUpOption.ObexFont = true;

				gElevatorDownOption = new WIListOption ("Elevator Down", "Up");
				//gElevatorDownOption.ObexFont = true;

				gElevatorCallOption = new WIListOption ("Call Elevator", "Call");
				//gElevatorCallOption.ObexFont = true;
			}

			if (HasUpperFloor) {
				gElevatorUpOption.Disabled = !(PowerSource.HasPower && TargetElevator.PlayerIsOnElevator && MissionCondition.CheckCondition (State.UpperFloorCondition));
				options.Add (gElevatorUpOption);
				if (gElevatorUpOption.Disabled) {
					Debug.Log ("upper disabled. has power? " + PowerSource.HasPower.ToString () + " player on elevator? " + TargetElevator.PlayerIsOnElevator.ToString () + " condition? " + MissionCondition.CheckCondition (State.UpperFloorCondition).ToString () + " " + State.StopIndex.ToString ());
				}
			}
			if (HasLowerFloor) {
				gElevatorDownOption.Disabled = !(PowerSource.HasPower && !TargetElevator.PlayerIsOnElevator && !MissionCondition.CheckCondition (State.LowerFloorCondition));
				options.Add (gElevatorDownOption);
				if (gElevatorDownOption.Disabled) {
					Debug.Log ("lower disabled. has power? " + PowerSource.HasPower.ToString () + " player on elevator? " + TargetElevator.PlayerIsOnElevator.ToString () + " condition? " + MissionCondition.CheckCondition (State.LowerFloorCondition).ToString () + " " + State.StopIndex.ToString ());
				}
			}
			gElevatorCallOption.Disabled = !(PowerSource.HasPower && TargetElevator.CurrentStopIndex != State.StopIndex && MissionCondition.CheckCondition (State.CallElevatorCondition) && !TargetElevator.PlayerIsOnElevator);
			if (gElevatorCallOption.Disabled) {
				Debug.Log ("call disabled. has power? " + PowerSource.HasPower.ToString () + " already on floor? " + (TargetElevator.CurrentStopIndex == State.StopIndex).ToString () + " condition? " + MissionCondition.CheckCondition (State.CallElevatorCondition).ToString () + " " + State.StopIndex.ToString ());
			}
			options.Add (gElevatorCallOption);

			gElevatorUpOption.Disabled = false;
			gElevatorDownOption.Disabled = false;
			gElevatorCallOption.Disabled = false;
		}

		public void OnPlayerUseWorldItemSecondary (object dialogResult)
		{
			WIListResult result = (WIListResult)dialogResult;

			switch (result.SecondaryResult) {
			case "Up":
				if (HasUpperFloor && PowerSource.HasPower && MissionCondition.CheckCondition (State.UpperFloorCondition)) {
					TargetElevator.SendToStopIndex (State.StopIndex + 1);
				}
				break;

			case "Down":
				if (HasLowerFloor && PowerSource.HasPower && MissionCondition.CheckCondition (State.LowerFloorCondition)) {
					TargetElevator.SendToStopIndex (State.StopIndex - 1);
				}
				break;

			case "Call":
				if (HasLowerFloor && PowerSource.HasPower && MissionCondition.CheckCondition (State.LowerFloorCondition)) {
					TargetElevator.SendToStopIndex (State.StopIndex);
				}
				break;

			default:
				break;
			}
		}

		bool FindElevator ( ) {
			if (TargetElevator != null) {
				return true;
			}

			TowerElevator[] elevators = GameObject.FindObjectsOfType <TowerElevator> ();
			for (int i = 0; i < elevators.Length; i++) {
				if (elevators [i].name.Equals (State.TargetElevatorName)) {
					TargetElevator = elevators [i];
					break;
				}
			}

			if (TargetElevator == null) {
				Debug.LogError ("Obex control panel had no target elevator");
				return false;
			}
			return true;
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{

		}
		#endif

		public static WIListOption gElevatorCallOption;
		public static WIListOption gElevatorUpOption;
		public static WIListOption gElevatorDownOption;
	}

	[Serializable]
	public class ObexControlPanelState
	{
		public string TargetElevatorName = "TowerMainElevator";
		public int StopIndex = 0;
		public MissionCondition UpperFloorCondition = new MissionCondition ();
		public MissionCondition LowerFloorCondition = new MissionCondition ();
		public MissionCondition CallElevatorCondition = new MissionCondition ();
	}
}
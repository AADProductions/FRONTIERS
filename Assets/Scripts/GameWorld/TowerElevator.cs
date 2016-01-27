using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class TowerElevator : MonoBehaviour
	{
		public Transform[] Stops;
		public ElevatorTriggerLoad TriggerLoad;

		public bool WaitingForStructure { 
			get {
				return TriggerLoad != null && TriggerLoad.WaitForStructure ();
			}
		}

		public int MaxFloorIndex { 
			get {
				return Stops.Length - 1;
			}
		}

		public Transform ElevatorPivot;
		public Vector3 StartPosition;
		public Vector3 EndPosition;
		public double TravelTime;
		public double StartTime;
		public AnimationCurve ElevatorMoveCurve;
		public int CurrentStopIndex = 0;
		public int TargetStopIndex = 0;
		public float Speed = 0.25f;
		public bool PlayerIsOnElevator = false;
		public AudioSource Audio;
		public AudioClip OnStopAudio;
		public AudioClip OnStartAudio;
		protected IItemOfInterest ioi;
		public bool Moving = false;
		protected int mLastTargetStopIndex = -1;

		public void Start ()
		{
			List <Transform> stops = new List <Transform> ();
			foreach (Transform child in transform) {
				if (child.name.Contains ("Stop")) {
					stops.Add (child);
				}
			}
			stops.Sort (delegate(Transform t1, Transform t2) {
				return t1.name.CompareTo (t2.name);
			});
			Stops = stops.ToArray ();
			stops.Clear ();
			ElevatorPivot.position = Stops [CurrentStopIndex].position;
			Audio = ElevatorPivot.gameObject.GetComponent <AudioSource> ();
		}

		public void SendToStopIndex (int stop)
		{
			TargetStopIndex = stop;
			StartPosition = ElevatorPivot.position;
			TriggerLoad = Stops [TargetStopIndex].GetComponent <ElevatorTriggerLoad> ();
		}

		public void FixedUpdate ()
		{
			if (TargetStopIndex != mLastTargetStopIndex) {
				mLastTargetStopIndex = TargetStopIndex;
				Audio.Stop ();
				Audio.PlayOneShot (OnStopAudio);
				TriggerLoad = Stops [TargetStopIndex].GetComponent <ElevatorTriggerLoad> ();
			}

			EndPosition = Stops [TargetStopIndex].position;
			Moving = true;

			if (ElevatorPivot.position == EndPosition) {
				Moving = false;
				if (Audio.isPlaying) {
					Audio.Stop ();
					Audio.PlayOneShot (OnStopAudio);
					TriggerLoad = null;
				}
				CurrentStopIndex = TargetStopIndex;
				return;
			} else {

				if (WaitingForStructure) {
					//Debug.Log ("Elevator is waiting for structure...");
					return;
				}

				if (!Audio.isPlaying) {
					Audio.Play ();
					Audio.PlayOneShot (OnStartAudio);
					TravelTime = Mathf.Abs (StartPosition.y - EndPosition.y) * Speed;
					StartTime = WorldClock.AdjustedRealTime;
				}
			}

			double timeSoFar = WorldClock.AdjustedRealTime - StartTime;
			Vector3 newPosition = Vector3.Lerp (StartPosition, EndPosition, ElevatorMoveCurve.Evaluate ((float)(timeSoFar / TravelTime)));
			if (float.IsNaN (newPosition.x)) {
				newPosition.x = 0f;
			}
			if (float.IsNaN (newPosition.y)) {
				newPosition.y = 0f;
			}
			if (float.IsNaN (newPosition.z)) {
				newPosition.z = 0f;
			}
			ElevatorPivot.position = newPosition;
		}
	}
}

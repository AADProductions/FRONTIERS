using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World.WIScripts
{
	public class KarasNote : WIScript {
		//this is a total kludge script that could probably be done generically
		//i have lost the will to avoid kludges
		//dear god just let me finish this game

		public KarasNoteState State = new KarasNoteState ( );

		public string BalloonCampNote = @"On holiday in the mountains pursuing my own projects.\nIf needed, coordinates are below.\nDo not disturb unless absolutely necessary!";
		//public string BalloonCampPath = @"Root\World\C-2-6-17\WI\AG\GarsiourValley\BorskarForest\BalloonLaunch";
		public string ObservatoryNote = @"If you need me, I'm on extended assignment at the Willowpeak Observatory. Visitors are allowed. Just take the Path of the Hundred up the mountain and keep on climbing!\n\n-Kara";
		//public string ObservatoryPath = @"Root\World\C-2-7-18\WI\AG\SjonaukPeak\ObservatoryBuilding";

		public override void OnInitialized ()
		{
			worlditem.OnActive += OnActive;
		}

		public void OnActive ( ) {
			RefreshNoteProps ();
		}

		public void RefreshNoteProps ( ) {
			MissionStatus objectiveStatus = MissionStatus.Dormant;
			if (Missions.Get.ObjectiveStatusByName ("Friends", "SpeakToKara", ref objectiveStatus)) {
				if (Flags.Check ((uint)MissionStatus.Completed, (uint)objectiveStatus, Flags.CheckType.MatchAny)) {
					worlditem.RemoveFromGame ();
					return;
				}
			}

			mExamineInfo.LocationsToReveal.Clear ();

			if (Missions.Get.ObjectiveStatusByName ("Legacy", "GiveFigurine", ref objectiveStatus)
				&& Flags.Check ((uint)MissionStatus.Completed, (uint)objectiveStatus, Flags.CheckType.MatchAny)) {
				State.ObservatoryNote = false;
			}

			if (State.ObservatoryNote) {
				mExamineInfo.StaticExamineMessage = ObservatoryNote;
				MobileReference observatoryLocation = new MobileReference (State.ObservatoryPath);
				mExamineInfo.LocationsToReveal.Add (observatoryLocation);
			} else {
				mExamineInfo.StaticExamineMessage = BalloonCampNote;
				MobileReference balloonCampLocation = new MobileReference (State.BalloonCampPath);
				mExamineInfo.LocationsToReveal.Add (balloonCampLocation);
			}
		}

		public override void PopulateExamineList (List <WIExamineInfo> examine)
		{
			RefreshNoteProps ();
			examine.Add (mExamineInfo);
		}

		protected WIExamineInfo mExamineInfo = new WIExamineInfo ( );
	}

	[Serializable]
	public class KarasNoteState {
		public bool ObservatoryNote = true;
		public string BalloonCampPath;
		public string ObservatoryPath;
	}
}
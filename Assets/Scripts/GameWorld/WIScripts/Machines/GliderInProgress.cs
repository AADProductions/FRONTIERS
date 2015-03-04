using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers;
using Frontiers.GUI;

namespace Frontiers.World {
	public class GliderInProgress : WIScript {

		public GameObject BodyObject;
		public GameObject ShieldObject;
		public GameObject MechObject;

		public string BodyObjective;
		public string BodyMission;
		public bool BodyCompleted;

		public string ShieldObjective;
		public string ShieldMission;
		public bool ShieldCompleted;

		public string MechObjective;
		public string MechMission;
		public bool MechCompleted;

		public WIExamineInfo BodyExamineIncomplete = new WIExamineInfo ();
		public WIExamineInfo ShieldExamineIncomplete = new WIExamineInfo ();
		public WIExamineInfo MechExamineIncomplete = new WIExamineInfo ();

		public WIExamineInfo BodyExamineComplete = new WIExamineInfo ();
		public WIExamineInfo ShieldExamineComplete = new WIExamineInfo ();
		public WIExamineInfo MechExamineComplete = new WIExamineInfo ();

		public WIExamineInfo ExamineAllComplete = new WIExamineInfo ();

		public GenericWorldItem ReplacementItem = new GenericWorldItem ("Tools", "Glider");

		public override void PopulateOptionsList (List<WIListOption> options, List<string> message)
		{
			if (BodyCompleted && ShieldCompleted && MechCompleted) {
				options.Add (new WIListOption ("Pack Up Glider", "Pack"));
			}
		}

		public void OnPlayerUseWorldItemSecondary (object secondaryResult)
		{
			WIListResult dialogResult = secondaryResult as WIListResult;			
			switch (dialogResult.SecondaryResult)
			{
			case "Pack":
				WorldItems.ReplaceWorldItem (this.worlditem, ReplacementItem);
				GUIManager.PostIntrospection ("How am I supposed to use this thing? They must have left instructions somewhere...");
				break;

			default:
				break;
			}
		}

		public override void OnInitialized ()
		{
			worlditem.OnActive += OnActive;
		}

		public void OnActive ( ) {
			RefreshGliderProps ();
		}

		public override void PopulateExamineList (List<WIExamineInfo> examine)
		{
			if (BodyCompleted && ShieldCompleted && MechCompleted) {
				examine.Add (ExamineAllComplete);
				return;
			}

			if (BodyCompleted) {
				examine.Add (BodyExamineComplete);
			} else {
				examine.Add (BodyExamineIncomplete);
			}
			if (ShieldCompleted) {
				examine.Add (ShieldExamineComplete);
			} else {
				examine.Add (ShieldExamineIncomplete);
			}
			if (MechCompleted) {
				examine.Add (MechExamineComplete);
			} else {
				examine.Add (MechExamineIncomplete);
			}
		}

		protected void RefreshGliderProps () {
			if (!BodyCompleted) {
				Missions.Get.ObjectiveCompletedByName (BodyMission, BodyObjective, ref BodyCompleted);
			}
			if (!ShieldCompleted) {
				Missions.Get.ObjectiveCompletedByName (ShieldMission, ShieldObjective, ref ShieldCompleted);
			}
			if (!MechCompleted) {
				Missions.Get.ObjectiveCompletedByName (MechMission, MechObjective, ref MechCompleted);
			}

			if (BodyCompleted) {
				BodyObject.SetActive (true);
			}
			if (ShieldCompleted) {
				ShieldObject.SetActive (true);
			}
			if (MechCompleted) {
				MechObject.SetActive (true);
			}
		}
	}
}
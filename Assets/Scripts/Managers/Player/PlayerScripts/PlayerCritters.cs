using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using System;
using Frontiers.World.Gameplay;
using Frontiers.GUI;

namespace Frontiers
{
	public class PlayerCritters : PlayerScript
	{
		public PlayerCrittersState State = new PlayerCrittersState ();
		Skill beastMasterSkill;
		bool usingMenu = false;
		Critter lastCritterInFocus;

		public override void Start ()
		{
			enabled = true;
		}

		public override void OnGameStart ()
		{
			Player.Get.UserActions.Subscribe (UserActionType.ItemInteract, new ActionListener (ItemInteract));
		}

		public bool ItemInteract (double timeStamp)
		{
			if (usingMenu || mWaitingForRename) {
				//we're already busy
				return true;
			}
			//if a critter is in player focus
			//and it HASN'T been named yet
			//spawn a pop up menu that allows them to name the critter
			if (player.Surroundings.IsCritterInPlayerFocus && !player.Surroundings.CritterFocus.UseName) {
				//if they haven't learned the beast master skill don't do anything
				//TODO tie this to butterfly namer reward
				lastCritterInFocus = player.Surroundings.CritterFocus;
				if (lastCritterInFocus == null) {
					Debug.Log ("Last critter in focus was null!");
					return true;
				}
				if (Skills.Get.HasLearnedSkill ("AnimalNamer")) {
					//we won't use the actual skill in this case - it's enough that the player has the skill
					//add the option list we'll use to select the skill
					SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList> ();
					optionsList.MessageType = string.Empty;//"Take " + mSkillUseTarget.DisplayName;
					optionsList.Message = "Rename Critter";
					optionsList.FunctionName = "OnRenameCritter";
					optionsList.RequireManualEnable = false;
					optionsList.OverrideBaseAvailabilty = true;
					optionsList.FunctionTarget = gameObject;
					optionsList.AddOption (new WIListOption ("Give Name", "GiveNameToCritter"));
					optionsList.AddOption (new WIListOption ("Cancel", "CancelGiveNameToCritter"));
					optionsList.ShowDoppleganger = false;
					GUIOptionListDialog dialog = null;
					if (optionsList.TryToSpawn (true, out dialog, null)) {
						optionsList.FunctionTarget = gameObject;
						optionsList.PauseWhileOpen = true;
						usingMenu = true;
						Debug.Log ("Spawned menu!");
					}
				}
			}
			return true;
		}

		public void OnRenameCritter (object dialogResult)
		{
			Debug.Log ("Finished using gui option list dialog");

			usingMenu = false;

			if (lastCritterInFocus == null || lastCritterInFocus.Destroyed) {
				Debug.Log ("Critter was no longer in focus or null");
				return;
			}

			WIListResult result = (WIListResult)dialogResult;

			Debug.Log ("Result: " + result.Result);

			switch (result.Result) {
			case "GiveNameToCritter":
				//spawn a name dialog and apply the result to the critter
				StringDialogResult nameResult = new StringDialogResult ();
				nameResult.Message = "Name Critter";
				nameResult.Result = string.Empty;
				nameResult.MessageType = string.Empty;//this will display the result as we type
				nameResult.AllowEmptyResult = true;
				GameObject confirmEditor = GUIManager.SpawnNGUIChildEditor (gameObject, GUIManager.Get.NGUIStringDialog, false);
				GUIManager.SendEditObjectToChildEditor <StringDialogResult> (new ChildEditorCallback <StringDialogResult> (OnFinishRename),
					confirmEditor,
					nameResult);
				mWaitingForRename = true;
				break;

			default:
				break;
			}
		}

		public void OnFinishRename (StringDialogResult editObject, IGUIChildEditor <StringDialogResult> childEditor)
		{
			Debug.Log ("Finished renaming");

			mWaitingForRename = false;

			if (editObject.Cancelled) {
				return;
			}

			if (lastCritterInFocus == null || lastCritterInFocus.Destroyed) {
				Debug.Log ("Critter was no longer in focus or null");
				return;
			}

			if (string.IsNullOrEmpty (editObject.Result.Trim ())) {
				return;
			}
			lastCritterInFocus.Name = editObject.Result;
			GUIManager.PostSuccess ("Renamed critter to " + editObject.Result);
		}

		protected bool mWaitingForRename = false;

		void Update ()
		{
			if (Input.GetKeyDown (KeyCode.C)) {
				CritterSaveState newCritterState = new CritterSaveState ();
				newCritterState.TimeCreated = WorldClock.AdjustedRealTime;
				newCritterState.Name = string.Empty;
				newCritterState.Type = "Butterfly";
				newCritterState.Coloration = UnityEngine.Random.Range (0, 3);
				State.Critters.Add (newCritterState);
				Critters.Get.SpawnFriendlyFromSaveState (newCritterState);
			}

			for (int i = 0; i < Critters.Get.FriendlyCritters.Count; i++) {
				GUIHud.Get.ShowName (Critters.Get.FriendlyCritters [i],
				                     Critters.Get.FriendlyCritters [i].Name);
			}
		}

		public override void OnLocalPlayerSpawn ()
		{
			for (int i = 0; i < State.Critters.Count; i++) {
				Critters.Get.SpawnFriendlyFromSaveState (State.Critters [i]);
			}
		}
	}

	[Serializable]
	public class PlayerCrittersState
	{
		public List <CritterSaveState> Critters = new List <CritterSaveState> ();
	}

	[Serializable]
	public class CritterSaveState
	{
		public string Name = string.Empty;
		public string Type = "Butterfly";
		public int Coloration = 0;
		public double TimeCreated = 0;
	}
}
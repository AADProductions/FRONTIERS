using UnityEngine;
using System.Collections;
using System;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Mural : WIScript
		{
				public MuralState State = new MuralState();
				public MasterAudio.SoundType RubbingSoundType = MasterAudio.SoundType.PlayerInterface;
				public string RubbingSound = "Writing";

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (!State.HasTakenRubbing) {
								options.Add(new GUIListOption("Take Rubbing", "Rubbing"));
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "Rubbing":
										MasterAudio.PlaySound(RubbingSoundType, worlditem.tr, RubbingSound);
										if (State.ChangeMissionVarOnTakeRubbing) {
												Missions.Get.ChangeVariableValue(State.MissionName, State.VariableName, State.VariableValue, State.ChangeType);
										}
										State.HasTakenRubbing = true;
										GUIManager.PostSuccess(State.PostMessageOnRubbingTaken);
										break;

								default:
										break;
						}
				}
		}

		[Serializable]
		public class MuralState
		{
				public string PostMessageOnRubbingTaken = "You take a rubbing of the mural.";
				public bool HasTakenRubbing = false;
				public bool ChangeMissionVarOnTakeRubbing = false;
				public string MissionName;
				public string VariableName;
				public int VariableValue = 1;
				public ChangeVariableType ChangeType = ChangeVariableType.Increment;
		}
}

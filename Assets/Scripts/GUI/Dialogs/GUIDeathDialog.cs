using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
	public class GUIDeathDialog : SecondaryInterface
	{
		public static GUIDeathDialog Get;
		public UIPanel DialogPanel;
		public UILabel CauseOfDeathLabel;
		public UILabel OfLabel;
		public bool UsingSkillList = false;

		public override void WakeUp()
		{
			OnShow += OnShowAction;
			GUIManager.Get.GetFocus(this);
		}

		public override bool GainFocus()
		{
			if (base.GainFocus()) {
				Show();
				return true;
			}
			return false;
		}

		public void OnShowAction()
		{
			if (UsingSkillList) {
				return;
			}

			OfLabel.enabled = true;
			CauseOfDeathLabel.text = Player.Local.Status.LatestCauseOfDeath;

			Debug.Log("Showing death dialog");

			//add the option list we'll use to select the skill
			SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
			optionsList.Message = "Respawn";
			optionsList.FunctionName = "OnSelectSpawnSkill";
			optionsList.RequireManualEnable = false;
			optionsList.OverrideBaseAvailabilty = true;
			optionsList.FunctionTarget = gameObject;
			optionsList.PositionTarget = UICamera.currentCamera.WorldToScreenPoint(transform.position);
			mSpawnSkills.Clear();
			mSpawnSkills.AddRange(Skills.Get.SkillsByType <RespawnSkill>());
			foreach (Skill spawnSkill in mSpawnSkills) {
				optionsList.AddOption(spawnSkill.GetListOption(Player.Local));
			}
			//the list can't go away unless the player makes a choice
			optionsList.ForceChoice = true;
			GUIOptionListDialog dialog = null;
			if (optionsList.TryToSpawn(true, out dialog)) {
				Debug.Log("Trying to spawn options list...");
				UsingSkillList = true;
			}
		}

		public void OnSelectSpawnSkill(System.Object result)
		{
			OptionsListDialogResult dialogResult = result as OptionsListDialogResult;
			RespawnSkill skillToUse = null;
			foreach (Skill removeSkill in mSpawnSkills) {
				if (removeSkill.name == dialogResult.Result) {
					skillToUse = removeSkill as RespawnSkill;
					break;
				}
			}

			OfLabel.enabled = false;

			if (skillToUse != null) {
				//use this skill to respawn
				skillToUse.Use(Player.Local, dialogResult.SecondaryResultFlavor);
				mSpawnSkills.Clear();
				StartCoroutine(WaitForPlayerToSpawn(skillToUse));
			} else {
				Debug.Log("SKILL TO USE NOT FOUND!");
			}
		}

		protected IEnumerator WaitForPlayerToSpawn(Skill respawnSkill)
		{
			while (Player.Local.IsDead) {
				CauseOfDeathLabel.text = "Respawning...";
				yield return null;
			}

			CauseOfDeathLabel.text = "Respawned";
			Finish();
			yield break;
		}

		protected List <RespawnSkill> mSpawnSkills = new List<RespawnSkill>();
	}
}
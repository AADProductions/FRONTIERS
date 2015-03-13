using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUIDeathDialog : SecondaryInterface
		{
				public static GUIDeathDialog Get;
				public UIPanel DialogPanel;
				public UILabel CauseOfDeathLabel;
				public UILabel OfLabel;
				public UILabel TitleLabel;
				public bool UsingSkillList = false;

				public override void WakeUp()
				{
						base.WakeUp();

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

						switch (Profile.Get.CurrentGame.Difficulty.DeathStyle) {
								case DifficultyDeathStyle.Respawn:
										TitleLabel.text = "You have lost consciousness";
										OfLabel.enabled = false;
										CauseOfDeathLabel.text = Player.Local.Status.LatestCauseOfDeath;
										ShowRespawnList();
										break;

								case DifficultyDeathStyle.PermaDeath:
										TitleLabel.text = "YOU HAVE DIED";
										OfLabel.enabled = true;
										CauseOfDeathLabel.text = Player.Local.Status.LatestCauseOfDeath;
										break;

								case DifficultyDeathStyle.NoDeath:
								default:
										break;
						}

						Debug.Log("Showing death dialog");
				}

				public void ShowRespawnList()
				{
						//add the option list we'll use to select the skill
						SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
						optionsList.MessageType = "Use Respawn Skill";
						optionsList.Message = string.Empty;
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
						WIListResult dialogResult = result as WIListResult;
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

				public override bool ActionCancel(double timeStamp)
				{
						//can't cancel
						return true;
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

				public override void DisableInput()
				{
						return;
				}

				public override void EnableInput()
				{
						return;
				}
		}
}
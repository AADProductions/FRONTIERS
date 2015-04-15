using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World.WIScripts
{
		//uses a detection skill to draw attention to an object
		//if the object is detected FX will be spawned on it - if not, no FX
		//detecting it successfully uses the skill successfully
		public class Detectable : WIScript
		{
				public DetectableState State = new DetectableState();
				public string DetectionSkillName;
				public double DetectionInterval = 10f;

				public bool CanDetect {
						get {
								if (!State.HasBeenDetected)
										return true;

								if (WorldClock.AdjustedRealTime > (State.LastTimeDetected + DetectionInterval))
										return true;

								return false;
						}
				}

				public override void OnInitialized()
				{
						worlditem.OnPlayerEncounter += OnPlayerEncounter;
						if (State.DetectOnVisible) {
								worlditem.OnVisible += OnPlayerEncounter;
						}
				}

				public void OnPlayerEncounter()
				{
						if (CanDetect) {
								if (string.IsNullOrEmpty(DetectionSkillName)) {
										FXManager.Get.SpawnFX(worlditem.Position, "DrawAttentionToItem");
										State.LastTimeDetected = WorldClock.AdjustedRealTime;
								} else {
										Skill detectionSkill = null;
										if (Skills.Get.LearnedSkill(DetectionSkillName, out detectionSkill)) {
												//use the skill on this item
												//if it succeeds it'll handle the rest
												//SKILL USE
												detectionSkill.Use(worlditem, 0);
										}
								}
						}
				}

				public void PlayerDetect()
				{
						State.HasBeenDetected = true;
						State.LastTimeDetected = WorldClock.AdjustedRealTime;
						OnPlayerDetect.SafeInvoke();
				}

				public Action OnPlayerDetect;
		}

		public class DetectableState
		{
				public bool HasBeenDetected = false;
				public double LastTimeDetected = 0f;
				public bool DetectOnVisible = false;
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;

namespace Frontiers
{
		public class PlayerAudio : PlayerScript
		{
				//the player audio class mostly listens for events and plays the correct audio for them
				//having this all happen in one big class is a lot simpler than each event being responsible
				//(this only applies to events associated with an avatar action)
				//these are updated in FixedUpdate
				public float AudibleVolume = 1.0f;
				public float VolumeMovementMultiplier = 1.0f;
				public float AudibleRange = Globals.MaxAudibleRange;
				public bool IsAudible = true;
				public float BlackoutIntensity = 10f;
				public double FootStepIntervalWalk = 0.425f;
				public double FootStepIntervalSprint = 0.3f;
				public double FootstepIntervalWalkInterior = 0.55f;
				public double FootStepIntervalLand = 0.25f;
				public string OutOfBreathSound = "MaleOutOfBreath";
				public string TiredSound = "MaleTired";
				public string CoughSound = "MaleCough";

				public override void Initialize()
				{
						mFootstepInterval = FootStepIntervalWalk;
						//so our audio ignores sounds played by us
						IgnoreColliders.Add(player.Controller);

						mInitialized = true;
				}

				public void Cough()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, CoughSound);
				}

				public void GetPushed()
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, "SurvivalTakeDamage");
				}

				public override void OnGameStart()
				{
						Player.Get.AvatarActions.Subscribe(AvatarAction.Move, new ActionListener(Move));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveJump, new ActionListener(MoveJump));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveLandOnGround, new ActionListener(MoveLandOnGround));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveWalk, new ActionListener(MoveWalk));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveSprint, new ActionListener(MoveSprint));

						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveEnterWater, new ActionListener(MoveEnterWater));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveExitWater, new ActionListener(MoveExitWater));

						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveCrouch, new ActionListener(MoveCrouch));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MoveStand, new ActionListener(MoveStand));

						Player.Get.AvatarActions.Subscribe(AvatarAction.ItemAddToInventory, new ActionListener(ItemAddToInventory));
						Player.Get.AvatarActions.Subscribe(AvatarAction.BarterMakeTrade, new ActionListener(BarterMakeTrade));

						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDie, new ActionListener(SurvivalDie));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamage, new ActionListener(SurvivalTakeDamage));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamageCritical, new ActionListener(SurvivalTakeDamageCritical));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalTakeDamageOverkill, new ActionListener(SurvivalTakeDamageOverkill));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDangerEnter, new ActionListener(SurvivalDangerEnter));
						Player.Get.AvatarActions.Subscribe(AvatarAction.SurvivalDangerExit, new ActionListener(SurvivalDangerExit));

						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionActivate, new ActionListener(MissionActivate));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionComplete, new ActionListener(MissionComplete));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionObjectiveActiveate, new ActionListener(MissionObjectiveActivate));
						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionObjectiveComplete, new ActionListener(MissionObjectiveComplete));

						Player.Get.AvatarActions.Subscribe(AvatarAction.PathMarkerReveal, new ActionListener(PathMarkerReveal));
						Player.Get.AvatarActions.Subscribe(AvatarAction.PathStartFollow, new ActionListener(PathStartFollow));
						Player.Get.AvatarActions.Subscribe(AvatarAction.PathStopFollow, new ActionListener(PathStopFollow));

						Player.Get.AvatarActions.Subscribe(AvatarAction.SkillLearn, new ActionListener(SkillLearn));

						Player.Get.AvatarActions.Subscribe(AvatarAction.ToolUse, new ActionListener(ToolUse));

						Player.Get.AvatarActions.Subscribe(AvatarAction.BookAquire, new ActionListener(BookReadOrAquire));
						Player.Get.AvatarActions.Subscribe(AvatarAction.BookRead, new ActionListener(BookReadOrAquire));

						Player.Get.AvatarActions.Subscribe(AvatarAction.SkillCredentialsGain, new ActionListener(SkillCredentialsGain));

						Player.Get.AvatarActions.Subscribe(AvatarAction.ItemPlaceFail, new ActionListener(ItemPlaceFail));
						Player.Get.AvatarActions.Subscribe(AvatarAction.ItemAddToInventory, new ActionListener(ItemAddToInventory));

						player.Inventory.State.PlayerBank.OnMoneyAdded += OnBankChange;

						enabled = true;
				}

				public void OnBankChange()
				{
						if (GameManager.Is(FGameState.InGame)) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "PlayerHandleMoney");
						}
				}

				public bool Move(double timeStamp)
				{
						if (WorldClock.AdjustedRealTime > mNextFootStepTime && Player.Local.IsGrounded) {
								//make a world sound every 3 footsteps
								mFootprintCounter++;
								if (mFootprintCounter > 3) {
										AudioManager.MakeWorldSound(player, IgnoreColliders, mFootstepType);
										mFootprintCounter = 0;
								} else {
										MasterAudio.PlaySound(mFootstepType, player.tr);
								}
								#if UNITY_EDITOR
								player.AudibleVolumeGizmo = 1f;
								#endif
								//MasterAudio.PlaySoundVariable (mFootstepType, 1.0f, transform);
								mNextFootStepTime = WorldClock.AdjustedRealTime + mFootstepInterval;
						}
						return true;
				}
				protected int mFootprintCounter = 0;

				public void FixedUpdate()
				{
						if (!mInitialized) {
								return;
						}

						if (player.IsSprinting) {
								mFootstepInterval = FootStepIntervalSprint;
						} else if (player.Surroundings.IsOutside) {
								mFootstepInterval = FootStepIntervalWalk;
						} else {
								mFootstepInterval = FootstepIntervalWalkInterior;
						}

						AudibleVolume = 1.0f * VolumeMovementMultiplier;//crouching
						AudibleRange = Globals.MaxAudibleRange;
						IsAudible = true;
						for (int i = 0; i < Skills.Get.SkillsInUse.Count; i++) {
								StealthSkill stealthSkill = Skills.Get.SkillsInUse[i] as StealthSkill;
								if (stealthSkill != null) {
										AudibleVolume *= stealthSkill.AudibleVolumeMultiplier;
										AudibleRange *= stealthSkill.AudibleRangeMultiplier;
										IsAudible &= stealthSkill.UserIsAudible;
								}
						}
						AudibleVolume = Mathf.Clamp01(AudibleVolume);

						if (WorldClock.AdjustedRealTime >= mNextFootStepTime || WorldClock.AdjustedRealTime >= mNextLandTime) {
								switch (Player.Local.Surroundings.State.GroundBeneathPlayer) {
										case GroundType.Dirt:
												mFootstepType = MasterAudio.SoundType.FootstepDirt;
												mJumpLandType = MasterAudio.SoundType.JumpLandDirt;
												break;

										case GroundType.Leaves:
												mFootstepType = MasterAudio.SoundType.FootstepLeaves;
												mJumpLandType = MasterAudio.SoundType.JumpLandLeaves;
												break;

										case GroundType.Wood:
												mFootstepType = MasterAudio.SoundType.FootstepWood;
												mJumpLandType = MasterAudio.SoundType.JumpLandWood;
												break;

										case GroundType.Water:
												mFootstepType = MasterAudio.SoundType.FootstepWater;
												mJumpLandType = MasterAudio.SoundType.JumpLandWater;
												break;

										case GroundType.Stone:
												mFootstepType = MasterAudio.SoundType.FootstepStone;
												mJumpLandType = MasterAudio.SoundType.JumpLandStone;
												break;

										default:
												mFootstepType = MasterAudio.SoundType.FootstepDirt;
												mJumpLandType = MasterAudio.SoundType.JumpLandDirt;
												break;
								}
						}
				}

				public bool MoveExitWater(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.JumpLandWater, "Jump");
						return true;
				}

				public bool MoveEnterWater(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.JumpLandWater, "Land");
						return true;
				}

				public bool MoveCrouch(double timeStamp)
				{
						VolumeMovementMultiplier = 0.5f;
						return true;
				}

				public bool MoveStand(double timeStamp)
				{
						VolumeMovementMultiplier = 1.0f;
						return true;
				}

				public bool MoveJump(double timeStamp)
				{
						MasterAudio.PlaySound(mJumpLandType, transform, "Jump");
						return true;
				}

				public bool MoveLandOnGround(double timeStamp)
				{
						if (WorldClock.AdjustedRealTime > mNextLandTime) {
								MasterAudio.PlaySound(mJumpLandType, transform, "Land");
								mNextLandTime = WorldClock.AdjustedRealTime + FootStepIntervalLand;
						}
						return true;
				}

				public bool MoveWalk(double timeStamp)
				{
						float statusValue = player.Status.GetStatusValue("Strength");
						if (statusValue <= 0f) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, OutOfBreathSound);
								CameraFX.Get.BlackOut(2f, Mathf.Abs(statusValue) * BlackoutIntensity);
						} else if (statusValue < 0.25f) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, TiredSound);
						}

						VolumeMovementMultiplier = 1f;
						//mFootstepInterval = FootStepIntervalWalk;
						return true;
				}

				public bool MoveSprint(double timeStamp)
				{
						float statusValue = player.Status.GetStatusValue("Strength");
						if (statusValue <= 0f) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, OutOfBreathSound);
								CameraFX.Get.BlackOut(2f, Mathf.Abs(statusValue) * BlackoutIntensity);
						} else if (statusValue < 0.25f) {
								MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, TiredSound);
						}

						VolumeMovementMultiplier = 2f;
						//mFootstepInterval = FootStepIntervalSprint;
						return true;
				}

				public bool ItemAddToInventory(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "InventoryAddItem");
						return true;
				}

				public bool	BarterMakeTrade(double timeStamp)
				{
						//		InventorySounds.PlayOneShot (BarterMakeTradeClip);
						return true;
				}

				public bool SurvivalTakeDamage(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, "SurvivalTakeDamage");
						return true;
				}

				public bool SurvivalTakeDamageCritical(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, "SurvivalTakeDamageCritical");
						return true;
				}

				public bool SurvivalTakeDamageOverkill(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, "SurvivalTakeDamageOverkill");
						return true;
				}

				public bool SurvivalDangerEnter(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "DangerCue");
						return true;
				}

				public bool SurvivalDangerExit(double timeStamp)
				{
						return true;
				}

				public bool SurvivalDie(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, "SurvivalDie");
						return true;
				}

				public bool MissionActivate(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "MissionActivate");
						return true;
				}

				public bool MissionComplete(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "MissionComplete");
						return true;
				}

				public bool MissionObjectiveActivate(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "MissionObjectiveActivate");
						return true;
				}

				public bool MissionObjectiveComplete(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "MissionObjectiveComplete");
						return true;
				}

				public bool SkillLearn(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "SkillLearn");
						return true;
				}

				public bool ToolUse(double timeStamp)
				{
						if (player.Tool.worlditem.Is <Weapon>()) {
								float statusValue = player.Status.GetStatusValue("Strength");
								if (statusValue <= 0f) {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, OutOfBreathSound);
										CameraFX.Get.BlackOut(2f, Mathf.Abs(statusValue) * BlackoutIntensity);
								} else if (statusValue < 0.25f) {
										MasterAudio.PlaySound(MasterAudio.SoundType.PlayerVoice, TiredSound);
								}
						}
						return true;
				}

				public bool SkillDiscover(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "SkillDiscover");
						return true;
				}

				public bool PathStartFollow(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "PathStartFollow");
						return true;
				}

				public bool PathStopFollow(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "PathStopFollow");
						return true;
				}

				public bool BookReadOrAquire(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "PageTurn");
						return true;
				}

				public bool PathMarkerReveal(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "PathMarkerReveal");
						return true;
				}

				public bool	SkillCredentialsGain(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "SkillCredentialsGain");
						return true;
				}

				public bool	ItemPlaceFail(double timeStamp)
				{
						MasterAudio.PlaySound(MasterAudio.SoundType.PlayerInterface, "ItemPlacementFail");
						return true;
				}

				public List <Collider> IgnoreColliders = new List <Collider>();
				protected double mFootstepInterval = 1.0f;
				protected double mNextFootStepTime = 0.0f;
				protected double mNextLandTime = 0.0f;
				protected MasterAudio.SoundType	mFootstepType = MasterAudio.SoundType.FootstepDirt;
				protected MasterAudio.SoundType	mJumpLandType = MasterAudio.SoundType.JumpLandDirt;
		}
}
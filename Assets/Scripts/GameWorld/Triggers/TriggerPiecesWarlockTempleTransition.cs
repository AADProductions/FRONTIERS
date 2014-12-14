using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World
{
		public class TriggerPiecesWarlockTempleTransition : WorldTrigger
		{		//this was a really specific sequence that couldn't be handled generically
				//and we don't have a real scripting language
				//so it's done as a custom class istead
				public TriggerPiecesWarlockTempleTransitionState State = new TriggerPiecesWarlockTempleTransitionState();
				public Transform HijackedPosition;
				public Transform HijackedLookTarget;

				public override bool OnPlayerEnter()
				{
						if (mUpdatingTransition) {
								return false;
						}
						mUpdatingTransition = true;
						StartCoroutine(UpdateTransitionOverTime());
						return true;
				}

				protected IEnumerator UpdateTransitionOverTime()
				{
						//player sets off an explosion
						FXManager.Get.SpawnExplosionFX(State.ExplosionFXType, transform, State.ExplosionPosition);
						MasterAudio.PlaySound(State.ExplosionSoundType, transform, State.ExplosionSound);
						Player.Local.Audio.Cough();
						//explosion makes them go blind
						CameraFX.Get.SetBlind(true);
						CameraFX.Get.AddDamageOverlay(1f);
						yield return new WaitForSeconds(State.CharacterDTSDelay);
						//robert says some DTS
						Frontiers.GUI.NGUIScreenDialog.AddSpeech(State.CharacterDTSText, State.CharacterDTSName, State.CharacterDTSDuration);
						Player.Local.Audio.Cough();
						Player.Local.MovementLocked = true;
						//force the two camps to rebuild
						for (int i = 0; i < State.ForceRebuildLocations.Count; i++) {
								WorldItem activeLocation = null;
								if (WIGroups.FindChildItem(State.ForceRebuildLocations[i], out activeLocation)) {
										activeLocation.Get <MissionInteriorController>().ForceRebuild();
								} else {
										Debug.Log("Couldn't load location " + State.ForceRebuildLocations[i]);
								}
						}
						//wait for a bit while that settles in
						yield return new WaitForSeconds(State.CharacterDTSDuration);
						//player is moved to temple entrance
						GameWorld.Get.ShowAboveGround(true);
						Player.Local.Surroundings.ExitUnderground();
						yield return new WaitForSeconds(0.1f);//give gameworld a sec to catch up
						//lock the player's position for a moment
						Player.Local.Position = State.PlayerWakeUpPosition;
						//blindness goes away
						CameraFX.Get.SetBlind(false);
						//as the player wakes up have him look at Robert
						Player.Local.HijackControl();
						double startHijackedTime = WorldClock.AdjustedRealTime;
						HijackedPosition = gameObject.CreateChild("HijackedPosition");
						HijackedLookTarget = gameObject.CreateChild("HijackedLookTarget");
						HijackedPosition.position = State.PlayerWakeUpPosition;
						HijackedLookTarget.position = State.PlayerWakeUpLookTarget;

						while (WorldClock.AdjustedRealTime < startHijackedTime + State.HijackedTimeDuration) {
								Player.Local.SetHijackTargets(HijackedPosition, HijackedLookTarget);
								yield return null;
						}
						Player.Local.RestoreControl(true);
						Player.Local.MovementLocked = false;
						GameObject.Destroy(HijackedPosition.gameObject, 0.5f);
						GameObject.Destroy(HijackedLookTarget.gameObject, 0.5f);
						//and we're done!
						yield break;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Transform playerWakeUpPosition = transform.FindChild("PlayerWakeUpPosition");
						Transform playerWakeUpLooktarget = transform.FindChild("PlayerWakeUpLookTarget");
						//make sure this is a global position!
						State.PlayerWakeUpPosition = playerWakeUpPosition.position;
						State.PlayerWakeUpLookTarget = playerWakeUpLooktarget.position;
						Transform explosionPosition = transform.FindChild("ExplosionPosition");
						State.ExplosionPosition = explosionPosition.localPosition;
				}
				#endif
				protected bool mUpdatingTransition = false;
		}

		[Serializable]
		public class TriggerPiecesWarlockTempleTransitionState : WorldTriggerState
		{
				public SVector3 ExplosionPosition = new SVector3();
				public Frontiers.FXManager.ExplosionType ExplosionFXType;
				public string ExplosionSound;
				public MasterAudio.SoundType ExplosionSoundType;
				public float CharacterDTSDelay = 5;
				public string CharacterDTSName = "Robert Hammersmith";
				public string CharacterDTSText = "Player? Can you hear me?";
				public float CharacterDTSDuration = 1f;
				public SVector3 PlayerWakeUpPosition = new SVector3();
				public SVector3 PlayerWakeUpLookTarget = new SVector3();
				public float HijackedTimeDuration = 5f;
				public List <string> ForceRebuildLocations = new List<string>();
		}
}

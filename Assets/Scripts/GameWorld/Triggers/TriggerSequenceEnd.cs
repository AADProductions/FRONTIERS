using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class TriggerSequenceEnd : WorldTrigger
		{
				public TriggerSequenceEndState State = new TriggerSequenceEndState();

				public override bool OnPlayerEnter()
				{
						Cutscene.CurrentCutsceneAnchor = gameObject;
						Application.LoadLevelAdditive(State.CutsceneName);
						return true;
				}

				public void OnCutsceneFinished()
				{
						if (State.EndTimeOfDayOverride) {
								Biomes.Get.UseTimeOfDayOverride = false;
						}
						StartCoroutine(FinishSequenceOverTime());
				}

				protected IEnumerator FinishSequenceOverTime()
				{
						yield return StartCoroutine(GUILoading.LoadStart(GUILoading.Mode.FullScreenBlack));
						GUILoading.Lock(this);
						Player.Local.Despawn();
						yield return null;
						yield return StartCoroutine(SpawnManager.Get.SendPlayerToStartupPosition(State.StartupPositionName, State.Delay));
						GUILoading.Unlock(this);
						yield return null;
						yield return StartCoroutine(GUILoading.LoadFinish());
				}
		}

		[Serializable]
		public class TriggerSequenceEndState : WorldTriggerState
		{
				public string StartupPositionName = "Default";
				public int ChunkID = 0;
				public float Delay = 0.5f;
				public bool EndTimeOfDayOverride = true;
				public string CutsceneName = string.Empty;
		}
}
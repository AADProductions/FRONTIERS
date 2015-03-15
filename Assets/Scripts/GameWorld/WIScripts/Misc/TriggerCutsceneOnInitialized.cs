using UnityEngine;
using System;
using System.Collections;

namespace Frontiers.World
{
		public class TriggerCutsceneOnInitialized : WIScript
		{
				public TriggerCutsceneOnInitializedState State = new TriggerCutsceneOnInitializedState();

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
				}

				public void OnVisible()
				{
						if (!mStartingCutscene) {
								mStartingCutscene = true;
								StartCoroutine(StartCutsceneOverTime());
						}
				}

				protected bool mStartingCutscene = false;

				public IEnumerator StartCutsceneOverTime()
				{
						while (!GameManager.Is(FGameState.InGame)) {
								//Debug.Log("Waiting to start cutscene...");
								yield return null;
						}
						Cutscene.CurrentCutsceneAnchor = gameObject;
						Debug.Log("Starting cutscene!");
						Application.LoadLevelAdditive(State.CutsceneName);
						Finish();
						yield break;
				}
		}

		[Serializable]
		public class TriggerCutsceneOnInitializedState
		{
				public string CutsceneName;
		}
}
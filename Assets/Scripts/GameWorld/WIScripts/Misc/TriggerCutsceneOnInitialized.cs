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
								yield return null;
						}
						Cutscene.CurrentCutsceneAnchor = gameObject;
						Application.LoadLevelAdditive(State.CutsceneName);
						yield return new WaitForSeconds(1.0f);
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
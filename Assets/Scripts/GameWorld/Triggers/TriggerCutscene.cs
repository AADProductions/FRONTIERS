using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World;
using System;

public class TriggerCutscene : WorldTrigger
{
		public TriggerCutsceneState State = new TriggerCutsceneState();
		protected bool mStartingCutscene = false;

		public override bool OnPlayerEnter()
		{
				if (!mStartingCutscene) {
						mStartingCutscene = true;
						StartCoroutine(StartCutsceneOverTime());
						return true;
				}
				return false;
		}

		public IEnumerator StartCutsceneOverTime()
		{
				while (!GameManager.Is(FGameState.InGame)) {
						yield return null;
				}
				Cutscene.CurrentCutsceneAnchor = gameObject;
				Application.LoadLevelAdditive(State.CutsceneName);
				while (Cutscene.IsActive) {
						yield return null;
				}
				yield break;
		}
}

[Serializable]
public class TriggerCutsceneState : WorldTriggerState
{
		public string CutsceneName;
}

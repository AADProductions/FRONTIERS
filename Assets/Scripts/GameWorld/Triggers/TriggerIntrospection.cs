using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World
{
		public class TriggerIntrospection : WorldTrigger
		{
				public TriggerIntrospectionState State = new TriggerIntrospectionState();

				public override bool OnPlayerEnter()
				{
						if (!mPostingIntrospectionOverTime) {
								mPostingIntrospectionOverTime = true;
								StartCoroutine(PostIntrospectionOverTime());
						}
						return true;
				}

				protected IEnumerator PostIntrospectionOverTime()
				{
						//wait for cutscenes / loading / etc. to finsih
						while (!GameManager.Is(FGameState.InGame) || !Player.Local.HasSpawned) {
								double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.1f;
								while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
										yield return null;
								}
						}

						yield return null;

						for (int i = 0; i < State.IntrospectionMessages.Count; i++) {	//if this is the last one, trigger the mission (if any)
								if (i == State.IntrospectionMessages.Count - 1) {
										if (State.ActivateMission) {
												GUI.GUIManager.PostIntrospection(State.IntrospectionMessages[i], State.ActivatedMissionName, State.Delay);
										} else {
												GUI.GUIManager.PostIntrospection(State.IntrospectionMessages[i], State.Delay);
										}
								} else {
										GUI.GUIManager.PostIntrospection(State.IntrospectionMessages[i], State.Delay);
								}
						}

						if (State.PayAttentionToItem) {
								WorldItem attentionItem = null;
								if (WIGroups.FindChildItem(State.AttentionItem.GroupPath, State.AttentionItem.FileName, out attentionItem)) {
										Player.Local.Focus.GetOrReleaseAttention(attentionItem);
								}
						}
						mPostingIntrospectionOverTime = false;
						yield break;
				}

				protected bool mPostingIntrospectionOverTime = false;
		}

		[Serializable]
		public class TriggerIntrospectionState : WorldTriggerState
		{
				public List <string> IntrospectionMessages = new List <string>();
				public float Delay = 0.15f;
				public bool ActivateMission = false;
				public string ActivatedMissionName = string.Empty;
				public bool PayAttentionToItem = false;
				public MobileReference AttentionItem;
		}
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World
{
		public class TriggerDimmingLanterns : WorldTrigger
		{
				public TriggerDimmingLanternsState State = new TriggerDimmingLanternsState();
				public List <WorldItem> LanternsToDim = new List <WorldItem>();
				public TriggerGuardIntervention GuardInterventionTrigger;
				public WorldTrigger ReactivationTrigger;

				public override bool OnPlayerEnter()
				{
						if (mDimmingLanterns) {
								return false;
						}

						if (LanternsToDim.Count == 0) {
								WorldItem lanternWorldItem = null;
								for (int i = 0; i < State.LanternsToDim.Count; i++) {
										MobileReference mr = State.LanternsToDim[i];
										if (WIGroups.FindChildItem(mr.GroupPath, mr.FileName, out lanternWorldItem)) {
												LanternsToDim.Add(lanternWorldItem);
										}
								}
								if (State.LanternsToDim.Count == 0) {
										return false;
								}
						}

						if (GuardInterventionTrigger == null) {
								WorldTriggerState wts = null;
								if (ParentChunk.GetTriggerState(State.GuardInterventionTriggerName, out wts)) {
										GuardInterventionTrigger = wts.trigger as TriggerGuardIntervention;
								} else {
										return false;
								}
						}

						mDimmingLanterns = true;
						mStartTime = WorldClock.AdjustedRealTime;
						StartCoroutine(DimLanternsOverTime());

						return true;
				}

				protected IEnumerator DimLanternsOverTime()
				{

						Debug.Log("Dimming lanterns!");

						GUIManager.PostIntrospection(State.IntrospectionBeforeStarting, true);

						yield return new WaitForSeconds(State.InitialDelay);

						GuardInterventionTrigger.SuspendGuard();
						//GuardInterventionTrigger.gameObject.SetActive (false);

						for (int i = 0; i < LanternsToDim.Count; i++) {
								LanternDimmer ld = LanternsToDim[i].Get <LanternDimmer>();
								ld.StartDimming();
								yield return new WaitForSeconds(0.5f);
						}

						if (State.UseReactivationTrigger) {
								WorldTriggerState reactivationTriggerState = null;
								if (ParentChunk.GetTriggerState(State.ReactivationTriggerName, out reactivationTriggerState)) {
										int numTimesTriggeredOnStart = reactivationTriggerState.NumTimesTriggered;
										while (reactivationTriggerState.NumTimesTriggered == numTimesTriggeredOnStart) {
												//wait until it's been triggered again
												yield return new WaitForSeconds(0.5f);
										}
								}
						} else {
								while (WorldClock.AdjustedRealTime < mStartTime + State.RTDimDuration) {
										yield return null;
								}
						}

						for (int i = 0; i < LanternsToDim.Count; i++) {
								LanternDimmer ld = LanternsToDim[i].Get <LanternDimmer>();
								ld.StopDimming();
								yield return new WaitForSeconds(0.5f);
						}

						//GuardInterventionTrigger.gameObject.SetActive (true);
						GuardInterventionTrigger.ResumeGuard(false);

						mDimmingLanterns = false;
						yield break;
				}

				protected bool mDimmingLanterns = false;
				protected double mStartTime = 0f;
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						State.LanternsToDim.Clear();
						foreach (WorldItem lanternWorldItem in LanternsToDim) {
								State.LanternsToDim.Add(lanternWorldItem.StaticReference);
						}
						State.GuardInterventionTriggerName = GuardInterventionTrigger.State.Name;

						if (ReactivationTrigger != null) {
								State.ReactivationTriggerName = ReactivationTrigger.name;
								State.UseReactivationTrigger = true;
						} else {
								State.UseReactivationTrigger = false;
						}
				}
				#endif
		}

		[Serializable]
		public class TriggerDimmingLanternsState : WorldTriggerState
		{
				public string IntrospectionBeforeStarting;
				public float InitialDelay = 10f;
				public List <MobileReference> LanternsToDim = new List <MobileReference>();
				public string GuardInterventionTriggerName;
				public float RTDimDuration = 10f;
				public bool UseReactivationTrigger = false;
				public string ReactivationTriggerName = string.Empty;
		}
}
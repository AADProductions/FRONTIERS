using UnityEngine;
using System.Collections;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class Chair : WIScript
		{
				public Transform SitCameraPosition;
				public float ComfortLevel = 1f;

				public override void OnStartup()
				{
						if (gSitOption == null) {
								gSitOption = new WIListOption("Sit", "Sit");
								gWaitOption = new WIListOption("Wait", "Wait");
						}
				}

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						if (!mSitting) {
								options.Add(gSitOption);
								options.Add(gWaitOption);
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						switch (dialogResult.SecondaryResult) {
								case "Sit":
										if (Player.Local.Surroundings.IsInDanger) {
												GUIManager.PostDanger("You cannot sit while you are in danger");
										} else {
												Player.Local.HijackControl();
												Player.Local.State.HijackMode = PlayerHijackMode.LookAtTarget;
												Player.Local.SetHijackTargets(SitCameraPosition, SitCameraPosition);
												Player.Local.SetHijackCancel(FinishSitting);
												Player.Local.Status.CustomStateList.Add("Sitting");
												mSitting = true;
												StartCoroutine(SitOverTime());
										}
										break;

								case "Wait":
										if (Player.Local.Surroundings.IsInDanger) {
												GUIManager.PostDanger("You cannot sit while you are in danger");
										} else {
												Player.Local.HijackControl();
												Player.Local.State.HijackMode = PlayerHijackMode.LookAtTarget;
												Player.Local.SetHijackTargets(SitCameraPosition, SitCameraPosition);
												Player.Local.SetHijackCancel(FinishSitting);
												Player.Local.Status.CustomStateList.Add("Sitting");
												mSitting = true;
												StartCoroutine(WaitOverTime());
										}
										break;
						}
				}

				public IEnumerator WaitOverTime()
				{
						StatusKeeper keeper = null;
						Player.Local.Status.GetStatusKeeper("Strength", out keeper);
						WorldClock.Get.SetTargetSpeed(WorldClock.gTimeScaleSleep);
						while (mSitting) {
								Player.Local.Status.RestoreStatus(PlayerStatusRestore.F_Full, "Strength", (float)(ComfortLevel * Globals.RestStrengthRestoreSpeed));
								keeper.Ping = true;
								yield return WorldClock.WaitForRTSeconds(1.5f);
						}
						WorldClock.Get.SetTargetSpeed(1.0f);
				}

				public IEnumerator SitOverTime()
				{
						StatusKeeper keeper = null;
						Player.Local.Status.GetStatusKeeper("Strength", out keeper);
						while (mSitting) {
								Player.Local.Status.RestoreStatus(PlayerStatusRestore.F_Full, "Strength", (float)(ComfortLevel * Globals.RestStrengthRestoreSpeed));
								keeper.Ping = true;
								yield return WorldClock.WaitForRTSeconds(1f);
						}
				}

				public void FinishSitting()
				{
						Player.Local.Status.CustomStateList.Remove("Sitting");
						mSitting = false;
				}

				protected bool mSitting = false;

				public void OnDrawGizmos()
				{
						Gizmos.color = Color.yellow;
						DrawArrow.ForGizmo(SitCameraPosition.position, SitCameraPosition.forward, 0.25f, 20);
				}

				public static WIListOption gSitOption = null;
				public static WIListOption gWaitOption = null;
		}
}
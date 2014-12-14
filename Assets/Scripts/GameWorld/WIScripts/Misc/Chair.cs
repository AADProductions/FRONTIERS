using UnityEngine;
using System.Collections;
using Frontiers.GUI;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Chair : WIScript
		{
				public Transform SitCameraPosition;
				public float ComfortLevel = 1f;

				public override void OnStartup()
				{
						if (gSitOption == null) {
								gSitOption = new GUIListOption("Sit", "Sit");
						}
				}

				public override void PopulateOptionsList(List <GUIListOption> options, List <string> message)
				{
						if (!mSitting) {
								options.Add(gSitOption);
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;

						if (dialogResult.SecondaryResult == "Sit") {
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
						}
				}

				public IEnumerator SitOverTime()
				{
						StatusKeeper keeper = null;
						Player.Local.Status.GetStatusKeeper("Strength", out keeper);
						while (mSitting) {
								Player.Local.Status.RestoreStatus(PlayerStatusRestore.F_Full, "Strength", (float)(ComfortLevel * Globals.RestStrengthRestoreSpeed));
								keeper.Ping = true;
								yield return new WaitForSeconds(1.5f);
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

				public static GUIListOption gSitOption = null;
		}
}
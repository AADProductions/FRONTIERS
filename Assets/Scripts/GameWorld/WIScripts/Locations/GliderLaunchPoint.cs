using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
		public class GliderLaunchPoint : WIScript
		{
				Visitable visitable = null;
				public GameObject LaunchFX;
				public GameObject HighlightFX;
				public Glider glider = null;
				public bool TakingOff = false;
				public float UpForce = 0.005f;
				public float ForwardForce = 0.01f;

				public override void OnInitialized()
				{
						visitable = worlditem.Get <Visitable>();
						visitable.OnPlayerVisit += OnPlayerVisit;
						visitable.OnPlayerLeave += OnPlayerLeave;
						worlditem.OnVisible += OnVisible;
						worlditem.OnInvisible += OnInvisible;
				}

				public void OnVisible()
				{
						if (visitable.State.NumTimesVisited == 0) {
								if (HighlightFX == null) {
										HighlightFX = FXManager.Get.SpawnFX(worlditem.tr, "LaunchPoint");
								}
						}
						if (LaunchFX == null) {
								LaunchFX = FXManager.Get.SpawnFX(worlditem.tr, "DustDevil");
						}
				}

				public void OnInvisible()
				{
						FXManager.Get.DestroyFX(HighlightFX);
						FXManager.Get.DestroyFX(LaunchFX);
				}

				public void OnPlayerVisit()
				{
						//don't show it after the player knows it's here
						FXManager.Get.DestroyFX(HighlightFX);
						enabled = true;
				}

				public void OnPlayerLeave()
				{
						enabled = false;
						if (mChildEditor != null) {
								mChildEditor.Finish();
								mChildEditor = null;
						}
				}

				public void ReceiveFromChildEditor(MessageActionDialogResult editObject, IGUIChildEditor <MessageActionDialogResult> childEditor)
				{
						if (TakingOff) {
								mChildEditor = null;
								return;
						}

						if (editObject.DidAction1) {
								TakingOff = true;
								StartCoroutine(TakeOffOverTime(glider));
						}
						mChildEditor = null;
				}

				public void Update()
				{
						//only show the dialog if we have the skill
						if (!Skills.Get.HasLearnedSkill("HangGlider")) {
								return;
						}
						//if we're taking off, update that and close the dialog
						if (TakingOff) {
								if (mChildEditor != null) {
										mChildEditor.Finish();
										mChildEditor = null;
								}
								enabled = false;
								return;
						}
						//if the player is holding the glider and we're not showing the dialog, show it now
						if (Player.Local.Tool.HasWorldItem && Player.Local.Tool.worlditem.Is <Glider>(out glider)) {
								if (mChildEditor == null) {
										GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIMessageActionDialog"));
										mChildEditor = childEditor.GetComponent <GUIMessageActionDialog>();
										MessageActionDialogResult editObject = new MessageActionDialogResult();
										editObject.Message = "Launch Point";
										editObject.Prompt1 = new GUIHud.HudPrompt(UserActionType.MoveJump, "Take Off");
										editObject.CloseOnAction1 = true;
										GUIManager.SendEditObjectToChildEditor <MessageActionDialogResult>(new ChildEditorCallback <MessageActionDialogResult>(ReceiveFromChildEditor), mChildEditor.gameObject, editObject);
								}
						} else {
								//if the player is not holding the glider and we're showing the child editor, close it now
								if (mChildEditor != null) {
										mChildEditor.Finish();
										mChildEditor = null;
								}
						}
				}

				protected IEnumerator TakeOffOverTime(Glider glider)
				{
						//hang on to this glider reference
						glider.worlditem.SetMode(WIMode.Frozen);
						//give it a sec while it enters the world
						yield return null;
						//make it face in the same direction as the player
						glider.worlditem.tr.position = Player.Local.Position;
						glider.worlditem.tr.rotation = Player.Local.Rotation;
						//give it a sec while it unpacks
						yield return null;
						Vehicle vehicle = glider.worlditem.Get <Vehicle>();
						vehicle.Mount(Player.Local);
						//give the player a sec while we mount the vehicle
						yield return null;
						//give the player a push forward and up
						Player.Local.FPSController.AddForce(Vector3.up * UpForce);
						Player.Local.FPSController.AddForce(Player.Local.ForwardVector * ForwardForce);
						//give the force a sec to take effect
						yield return null;
						TakingOff = false;
						enabled = false;
						yield break;
				}

				protected GUIMessageActionDialog mChildEditor;
		}
}

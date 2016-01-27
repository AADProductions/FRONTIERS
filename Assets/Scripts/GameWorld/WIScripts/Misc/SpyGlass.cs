using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class SpyGlass : WIScript
		{
				public static bool IsInUse = false;
				protected float mFieldOfView = 15f;
				protected float mFieldOfViewCollapsed = 30f;
				public float ExtendedCameraSensitivity = 0.25f;
				public Skill SpyglassSkill;
				public Transform LookerRoot;
				public Transform LookerTransform;
				public bool CanPlaceMarker = false;
				public List <string> ItemsOfInterest = new List<string>() { "Revealable", "Location" };
				public Vector3 MapMarkerLocation;

				public override bool UnloadWhenStacked {
						get {
								return false;
						}
				}

				public override void OnInitialized()
				{
						Equippable equippable = worlditem.Get <Equippable>();
						equippable.OnUnequip += OnUnequip;
						Skills.Get.SkillByName("Spyglass", out SpyglassSkill);
				}

				public bool LookThroughSpyglass()
				{
						IsInUse = true;

						if (LookerRoot == null) {
								LookerRoot = new GameObject("SpyglassLookerRoot").transform;
								LookerTransform = LookerRoot.gameObject.CreateChild("LookerTransform").transform;
						}

						LookerRoot.rotation = Player.Local.Rotation;
						LookerRoot.position = Player.Local.HeadPosition;
						LookerTransform.localPosition = Player.Local.ForwardVector * SpyglassSkill.EffectRadius;
						//set the looker to the player's head position & rotation
						worlditem.tr.LookAt(LookerTransform);

						GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIMessageActionDialog"));
						mChildEditor = childEditor.GetComponent <GUIMessageActionDialog>();
						MessageActionDialogResult editObject = new MessageActionDialogResult();
						editObject.Message = "Spyglass (unequip to exit)";
						editObject.CloseOnAction1 = false;
						editObject.CloseOnAction2 = false;
						editObject.Prompt1 = new GUIHud.HudPrompt(UserActionType.MoveJump, "Place Marker");
						editObject.Prompt2 = new GUIHud.HudPrompt(UserActionType.ToolCycleNext, "Mode");
						mChildEditor.OnDidAction1 += OnPlacedMarker;
						mChildEditor.OnDidAction2 += OnCycleMode;
						GUIManager.SendEditObjectToChildEditor <MessageActionDialogResult>(new ChildEditorCallback <MessageActionDialogResult>(ReceiveFromChildEditor), mChildEditor.gameObject, editObject);

						enabled = true;
						return true;
				}

				public void Update()
				{
						CameraFX.Get.SetSpyglass(true);
						//from now on we'll control where the player looks
						if (worlditem.State == "Extended") {
								Player.Local.ZoomCamera (mFieldOfView, ExtendedCameraSensitivity);
						} else {
								Player.Local.ZoomCamera (mFieldOfViewCollapsed, ExtendedCameraSensitivity);
						}

						CanPlaceMarker = false;
						RaycastHit[] hits = Physics.RaycastAll(Player.Local.HeadPosition, Player.Local.FocusVector, SpyglassSkill.EffectRadius * Globals.RaycastSpyGlassDistanceMultiplier, Globals.LayersActive);
						bool hitSomething = false;
						float closestDistanceSoFar = Mathf.Infinity;
						float currentDistance;
						for (int i = 0; i < hits.Length; i++) {
								CanPlaceMarker = true;
								currentDistance = Vector3.Distance(hits[i].point, LookerRoot.position);
								if (currentDistance < closestDistanceSoFar) {
										//this is where we'll add our marker
										closestDistanceSoFar = currentDistance;
										MapMarkerLocation = hits[i].point;
								}
								//now see if we reveal anything
								if (WorldItems.GetIOIFromCollider(hits[i].collider, out ioi) && ioi.IOIType == ItemOfInterestType.WorldItem) {
										if (ioi.worlditem.Is <Revealable>(out revealable) && !revealable.State.HasBeenRevealed) {
												MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "PathMarkerReveal");
												GUIManager.PostInfo("Revealed location");
												revealable.Reveal(LocationRevealMethod.ByTool);
										}
										if (ioi.worlditem.Is <Location>(out location)) {
												if (location.IsCivilized) {
														Player.Local.Surroundings.AddCivilizationBoost(SpyglassSkill.EffectTime);
												}
										}
								}
						}
				}

				protected IItemOfInterest ioi;
				protected Revealable revealable;
				protected Location location;
				protected Vector3 mRotation;

				public void OnCycleMode()
				{
						if (mFinished)
								return;

						Debug.Log("Cycling mode");
						if (worlditem.State == "Extended") {
								worlditem.State = "Collapsed";
						} else {
								worlditem.State = "Extended";
						}
				}

				public void OnPlacedMarker()
				{
						if (mFinished)
								return;

						if (!CanPlaceMarker) {
								GUIManager.PostWarning("Terrain is out of range");
						} else {
								MasterAudio.PlaySound(MasterAudio.SoundType.Notifications, "PathMarkerReveal");
								Player.Local.Surroundings.AddMapMarker(MapMarkerLocation);
						}
				}

				public void ReceiveFromChildEditor(MessageActionDialogResult editObject, IGUIChildEditor <MessageActionDialogResult> childEditor)
				{
						if (mFinished)
								return;

						mChildEditor = null;
						OnUnequip();
				}

				public void OnUnequip()
				{
						if (LookerRoot != null) {
								GameObject.Destroy(LookerRoot.gameObject);
								GameObject.Destroy(LookerTransform.gameObject);
						}
						if (mChildEditor != null) {
								mChildEditor.Finish();
								mChildEditor = null;
						}
						enabled = false;
				}

				public override void OnFinish()
				{
						if (LookerRoot != null) {
								GameObject.Destroy(LookerRoot.gameObject);
								GameObject.Destroy(LookerTransform.gameObject);
						}
						if (mChildEditor != null) {
								mChildEditor.Finish();
								mChildEditor = null;
						}
						enabled = false;
				}

				public void OnDisable () {
						CameraFX.Get.SetSpyglass(false);
						Player.Local.UnzoomCamera();
						IsInUse = false;
						return;
				}

				protected GUIMessageActionDialog mChildEditor;
		}
}
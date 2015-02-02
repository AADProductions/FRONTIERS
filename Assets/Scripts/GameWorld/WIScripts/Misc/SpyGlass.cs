using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.GUI;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Spyglass : WIScript
		{
				public float FieldOfView = 20f;
				public float HijackSpeed = 0.25f;
				public float LookSpeed = 1f;
				public Skill SpyglassSkill;
				public Transform LookerRoot;
				public Transform LookerTransform;
				public bool CanPlaceMarker = false;
				public List <string> ItemsOfInterest = new List<string>() { "Revealable", "Location" };
				public Vector3 MapMarkerLocation;

				public override void OnInitialized()
				{
						Equippable equippable = worlditem.Get <Equippable>();
						equippable.OnUnequip += OnUnequip;
						Skills.Get.SkillByName("Spyglass", out SpyglassSkill);
				}

				public bool LookThroughSpyglass()
				{
						if (LookerRoot == null) {
								LookerRoot = new GameObject("SpyglassLookerRoot").transform;
								LookerTransform = LookerRoot.gameObject.CreateChild("LookerTransform").transform;
						}

						LookerRoot.rotation = Player.Local.Rotation;
						LookerRoot.position = Player.Local.HeadPosition;
						LookerTransform.localPosition = Player.Local.ForwardVector * SpyglassSkill.EffectRadius;
						//set the looker to the player's head position & rotation
						worlditem.tr.LookAt(LookerTransform);
						//from now on we'll control where the player looks
						Player.Local.HijackControl(FieldOfView, HijackSpeed);
						Player.Local.SetHijackTargets(LookerTransform);

						CameraFX.Get.SetSpyglass(true);

						GameObject childEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUIMessageActionDialog"));
						mChildEditor = childEditor.GetComponent <GUIMessageActionDialog>();
						MessageActionDialogResult editObject = new MessageActionDialogResult();
						editObject.Action = UserActionType.MoveJump;
						editObject.CloseOnAction = false;
						editObject.Message = "Press SPACE to set marker\nUnequip to exit";
						mChildEditor.OnDidAction += OnDidAction;
						GUIManager.SendEditObjectToChildEditor <MessageActionDialogResult>(new ChildEditorCallback <MessageActionDialogResult>(ReceiveFromChildEditor), mChildEditor.gameObject, editObject);

						enabled = true;
						return true;
				}

				public void Update()
				{
						//we want to move the hijacked position based on the player's mouse movements
						//move the looker appropriately
						//get the mouse raw input and use it to rotate the looker base
						mRotation.x += UserActionManager.RawMouseAxisY * LookSpeed;
						mRotation.y += UserActionManager.RawMouseAxisX * LookSpeed;
						mRotation.z = 0f;

						if (Profile.Get.HasCurrentProfile && Profile.Get.CurrentPreferences.Controls.MouseInvertYAxis) {
								mRotation.y = -mRotation.y;
						}

						LookerRoot.localEulerAngles = mRotation;
						Player.Local.SetHijackTargets(LookerTransform);

						CanPlaceMarker = false;
						RaycastHit[] hits = Physics.RaycastAll(Player.Local.HeadPosition, GameManager.Get.GameCamera.transform.forward, SpyglassSkill.EffectRadius, Globals.LayersActive);
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
										if (ioi.worlditem.Is <Revealable>(out revealable)) {
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

				public void OnDidAction()
				{
						if (mFinished)
								return;

						if (!CanPlaceMarker) {
								GUIManager.PostWarning("Terrain is out of range");
						} else {
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
								Player.Local.RestoreControl(true);
								GameObject.Destroy(LookerRoot.gameObject);
								GameObject.Destroy(LookerTransform.gameObject);
						}
						if (mChildEditor != null) {
								mChildEditor.Finish();
								mChildEditor = null;
						}
						CameraFX.Get.SetSpyglass(false);
						enabled = false;
				}

				public override void OnFinish()
				{
						if (LookerRoot != null) {
								Player.Local.RestoreControl(true);
								GameObject.Destroy(LookerRoot.gameObject);
								GameObject.Destroy(LookerTransform.gameObject);
						}
						if (mChildEditor != null) {
								mChildEditor.Finish();
								mChildEditor = null;
						}
						CameraFX.Get.SetSpyglass(false);
				}

				protected GUIMessageActionDialog mChildEditor;
		}
}
using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.Gameplay;
using System.Collections.Generic;
using Frontiers.GUI;

namespace Frontiers.World
{
		public class Campsite : WIScript
		{
				public CampsiteState State = new CampsiteState();
				//shows when the campsite is active
				public Renderer FlagRenderer;
				public CreateCampsite CreateCampsiteSkill;
				public Equippable equippable;

				public override bool CanEnterInventory {
						get {
								return !State.HasBeenCreated;
						}
				}

				public override bool CanBeCarried {
						get {
								return !State.HasBeenCreated;
						}
				}

				public override bool CanBeDropped {
						get {
								return false;
						}
				}

				public void RefreshFlag()
				{
						if (State.HasBeenCreated) {
								FlagRenderer.enabled = true;
								Location location = worlditem.Get <Location>();
								FlagRenderer.enabled = State.HasBeenCreated;
								FlagRenderer.material.color = Colors.ColorFromString(location.State.Name.CommonName, 100);
						} else {
								FlagRenderer.enabled = false;
						}
				}

				public override void PopulateOptionsList(List<GUIListOption> options, List<string> message)
				{
						if (State.HasBeenCreated) {
								if (gRenameOption == null) {
										gRenameOption = new GUIListOption("Rename Campsite", "Rename");
								}
								options.Add(gRenameOption);
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						OptionsListDialogResult dialogResult = secondaryResult as OptionsListDialogResult;			
						switch (dialogResult.SecondaryResult) {
								case "Rename":
										TryToRename();
										break;

								default:
										break;
						}
				}

				public void TryToRename()
				{
						if (mWaitingForRename) {
								return;
						}

						if (State.HasBeenCreated && State.CreatedByPlayer) {
								Location location = worlditem.Get <Location>();
								StringDialogResult result = new StringDialogResult();
								result.Message = "Rename Campsite";
								string currentName = location.State.Name.CommonName;
								//by default campsites are named 'Campsite'
								//otherwise they're named 'Camp [name]'
								if (currentName == "Campsite") {
										currentName = string.Empty;
								} else {
										currentName = currentName.Replace("Camp", "").Trim();
								}
								result.Result = currentName;
								result.Message = "Camp {Result}";
								result.MessageType = "Rename Campsite";//this will display the result as we type
								result.AllowEmptyResult = true;
								GameObject confirmEditor = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.NGUIStringDialog, false);
								GUIManager.SendEditObjectToChildEditor <StringDialogResult>(new ChildEditorCallback <StringDialogResult>(OnFinishRename),
										confirmEditor,
										result);

								mWaitingForRename = true;
						}
				}

				public void OnFinishRename(StringDialogResult editObject, IGUIChildEditor <StringDialogResult> childEditor)
				{
						mWaitingForRename = false;

						if (editObject.Cancelled) {
								return;
						}

						Location location = null;
						if (worlditem.Is <Location>(out location)) {
								if (string.IsNullOrEmpty(editObject.Result.Trim())) {
										location.State.Name.CommonName = "Campsite";
										return;
								}
								location.State.Name.CommonName = "Camp " + editObject.Result;
								RefreshFlag();
								GUIManager.PostSuccess("Renamed campsite to " + location.State.Name.CommonName);
						}
				}

				public override void PopulateExamineList(System.Collections.Generic.List<WIExamineInfo> examine)
				{
						if (State.HasBeenCreated) {
								examine.Add(new WIExamineInfo("This campfire belongs to a campsite."));
						}
				}

				public override bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						return CreateCampsiteSkill.CanBePlacedOn(this, targetObject, point, normal, ref errorMessage);
				}

				public override void OnInitialized()
				{
						if (!State.HasBeenCreated) {
								Skill skill = null;
								if (Skills.Get.SkillByName("CreateCampsite", out skill)) {
										CreateCampsiteSkill = skill as CreateCampsite;
								}
						}
						if (worlditem.Is <Equippable>(out equippable)) {
								equippable.OnEquip += OnEquip;
								worlditem.OnPlayerPlace += OnPlayerPlace;
						}
						worlditem.OnVisible += OnVisible;
				}

				public void OnVisible()
				{
						RefreshFlag();
				}

				public void OnPlayerPlace()
				{
						CreateCampsiteSkill.OnPlaceCampsite(this);
				}

				public void OnEquip()
				{
						CreateCampsiteSkill.OnEquipCampsite(this);
				}

				protected bool mWaitingForRename = false;
				protected static GUIListOption gRenameOption;
		}

		[Serializable]
		public class CampsiteState
		{
				public bool CreatedByPlayer = false;
				public bool HasBeenCreated = false;
		}
}

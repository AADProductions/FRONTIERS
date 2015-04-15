using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World.WIScripts
{
		public class Artifact : WIScript
		{
				public ArtifactState State = new ArtifactState();

				public override void PopulateExamineList(List<WIExamineInfo> examine)
				{
						WIExamineInfo examineInfo = new WIExamineInfo();
						examineInfo.RequiredSkill = "AppraiseArtifact";
						examineInfo.StaticExamineMessage = "This artifact is " + Colors.ColorWrap(State.Age.ToString(), Colors.Get.GenericHighValue)
						+ " and it is of "
						+ Colors.ColorWrap(State.Quality.ToString(), Colors.Get.GenericHighValue) + " quality.";
						if (State.HasBeenAppraised) {
								examineInfo.StaticExamineMessage += " It has been appraised.";
						}
						examineInfo.ExamineMessageOnFail = "I have no idea what kind of artifact it is.";
						examine.Add(examineInfo);
				}

				public override void OnInitializedFirstTime()
				{
						if (State.IsCuratedItem) {
								State.VisibleNow = State.VisibleOnStartup;
						}
				}

				public override void OnInitialized()
				{
						if (State.IsCuratedItem) {
								worlditem.OnAddedToGroup += OnAddedToGroup;
								Museums.Get.ActiveCuratedArtifacts.SafeAdd(this);
						}
				}

				public void OnAddedToGroup()
				{
						SetCuratedProperties(State.VisibleNow, State.Quality);
				}

				public void SetCuratedProperties(bool visibleNow, ArtifactQuality quality)
				{
						if (State.IsCuratedItem) {
								State.VisibleNow = visibleNow;
								worlditem.ActiveStateLocked = false;
								if (visibleNow) {
										worlditem.ActiveState = WIActiveState.Active;
										State.Quality = quality;
								} else {
										worlditem.ActiveState = WIActiveState.Invisible;
								}
								worlditem.ActiveStateLocked = true;
						}
				}

				public bool CanBeAppraised {
						get {
								return !State.HasBeenAppraised;
						}
				}

				public bool Appraise()
				{
						if (State.HasBeenAppraised) {
								return false;
						}
						State.HasBeenAppraised = true;
						return true;
						//GUIManager.PostIntrospection ("
				}

				public void Reconstruct()
				{
						GUI.GUIManager.PostIntrospection("This artifact has already been reconstructed");
				}

				public static float AgeToFloat(ArtifactAge age)
				{
						float normalizedFloat = 0.0f;
						switch (age) {
								case ArtifactAge.Recent:
										normalizedFloat = 0.0f;
										break;
								case ArtifactAge.Modern:
										normalizedFloat = 0.2f;
										break;
								case ArtifactAge.Old:
										normalizedFloat = 0.4f;
										break;
				
								case ArtifactAge.Antiquated:
										normalizedFloat = 0.6f;
										break;
				
								case ArtifactAge.Ancient:
										normalizedFloat = 0.8f;
										break;
				
								case ArtifactAge.Prehistoric:
										normalizedFloat = 1.0f;
										break;
				
								default:
										break;
						}
						return normalizedFloat;
				}
		}

		[Serializable]
		public class ArtifactState
		{
				public bool HasBeenAppraised = false;
				public bool HasBeenDated = false;
				public ArtifactAge Age = ArtifactAge.Ancient;
				public ArtifactQuality Quality = ArtifactQuality.Poor;
				public bool IsCuratedItem = false;//appears in a museum
				public string MuseumName = "GuildMuseum";
				public bool VisibleOnStartup = true;
				public bool VisibleNow = true;
		}
}

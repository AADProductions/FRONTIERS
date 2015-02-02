using UnityEngine;
using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.BaseWIScripts
{
		public class BookAvatar : WIScript
		{
				public BookAvatarState State = new BookAvatarState();
				public BookTemplate Template;

				public override string DisplayNamer(int increment)
				{
						if (string.IsNullOrEmpty(State.BookTitle)) {
								State.BookTitle = Books.Get.BookTitle(State.BookName);
						}
						return State.BookTitle;
				}

				public override void OnInitialized()
				{
						State.TemplateName = worlditem.Props.Local.Subcategory;
						State.BookName = worlditem.Props.Name.StackName;
						RefreshAppearance();
				}

				public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
				{
						bool canRead = false;
						if (WorldItems.IsOwnedByPlayer(worlditem)) {
								canRead = true;
						} else {
								BookTemplate template = null;
								if (Books.Get.BookTemplateByName(State.TemplateName, out template)) {
										if (template.CanBeReadWithoutAquiring) {
												canRead = true;
										}
								}
						}
						if (canRead) {
								options.Add(new WIListOption("Read"));
						}
				}

				public override void PopulateExamineList(List <WIExamineInfo> examine)
				{
						Book book = null;
						if (Books.Get.BookByName(State.BookName, out book)) {
								WIExamineInfo examineInfo = new WIExamineInfo();
								examineInfo.StaticExamineMessage = book.ContentsSummary;
								examine.Add(examineInfo);
						}
				}

				public void OnPlayerAquire()
				{
						Books.AquireBook(State.BookName);
				}

				public void OnPlayerUseWorldItemSecondary(object result)
				{
						WIListResult dialogResult = result as WIListResult;
						switch (dialogResult.SecondaryResult) {
								case "Read":
										Books.ReadBook(State.BookName, null);
										break;
						}
				}

				public void RefreshAppearance()
				{
						Books.Get.InitializeBookAvatarGameObject(gameObject, State.BookName, State.TemplateName);
				}

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						WorldItem worlditem = gameObject.GetComponent <WorldItem>();
						worlditem.Props.Local.Subcategory = State.TemplateName;
						worlditem.Props.Name.StackName = State.BookName;
				}

				public void EditorSaveTemplate()
				{
						if (!Manager.IsAwake <Books>()) {
								Manager.WakeUp <Books>("Frontiers_ObjectManagers");
						}

						if (Template.MeshIndex == 0) {
								Template.MeshIndex |= Books.Get.EditorMeshIndex(gameObject.GetComponent <MeshFilter>().sharedMesh.name);
						}
						if (Template.TextureIndex == 0) {
								Template.TextureIndex |= Books.Get.EditorTextureIndex(gameObject.renderer.sharedMaterial.mainTexture.name);
						}

						bool foundExisting = false;
						for (int i = 0; i < Books.Get.Templates.Count; i++) {
								if (Books.Get.Templates[i].Name == State.TemplateName) {
										if (!Books.Get.Templates[i].BookNames.Contains(State.BookName)) {
												Books.Get.Templates[i].BookNames.Add(State.BookName);
										}
										Books.Get.Templates[i] = ObjectClone.Clone <BookTemplate>(Template);
										foundExisting = true;
								}
						}
						if (!foundExisting) {
								Books.Get.Templates.Add(ObjectClone.Clone <BookTemplate>(Template));
						}
						//RefreshAppearance ();
				}

				public void EditorLoadTemplate()
				{
						if (!Manager.IsAwake <Books>()) {
								Manager.WakeUp <Books>("Frontiers_ObjectManagers");
						}

						for (int i = 0; i < Books.Get.Templates.Count; i++) {
								if (Books.Get.Templates[i].Name == State.TemplateName) {
										Template = ObjectClone.Clone <BookTemplate>(Books.Get.Templates[i]);
								}
						}
				}
				#endif

				//TODO most of these are no longer used, remove them
				protected float mStartReadTime;
				protected float mEndReadTime;
				protected bool mIsReading;
				protected Skill mCurrentSkill;
				protected static HashSet <string> mRemoveItemSkillCheck = new HashSet<string>();
		}

		[Serializable]
		public class BookAvatarState
		{
				[FrontiersAvailableModsAttribute("Book")]
				public string BookName;
				public string BookTitle;
				[FrontiersAvailableModsAttribute("BookTemplate")]
				public string TemplateName;
		}
}
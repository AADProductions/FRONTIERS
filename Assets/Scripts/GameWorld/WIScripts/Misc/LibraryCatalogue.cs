using UnityEngine;
using System.Collections;
using System;
using Frontiers;
using Frontiers.GUI;
using System.Collections.Generic;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
	public class LibraryCatalogue : WIScript
	{
		public LibraryCatalogueState State = new LibraryCatalogueState();

		public override void OnInitialized()
		{
			worlditem.OnPlayerUse += OnPlayerUse;
		}

		public override int OnRefreshHud(int lastHudPriority)
		{
			lastHudPriority++;
			GUIHud.Get.ShowAction(UserActionType.ItemUse, "Browse Catalogue", worlditem.HudTarget, GameManager.Get.GameCamera);
			return lastHudPriority;
		}

		public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
		{
			if (mBrowseOption == null) {
				mBrowseOption = new WIListOption("Browse Catalogue", "Browse");
			}

			Library library = null;
			if (Books.Get.Library(State.LibraryName, out library)) {
				//can we browse this catalogue?
				Skill learnedSkill = null;
				if (!string.IsNullOrEmpty(library.RequiredSkill)) {
					//match the icon to the skill
					bool hasLearnedSkill = Skills.Get.HasLearnedSkill(library.RequiredSkill, out learnedSkill);
					mBrowseOption.IconName = learnedSkill.Info.IconName;
					mBrowseOption.IconColor = learnedSkill.SkillIconColor;
					mBrowseOption.BackgroundColor = learnedSkill.SkillBorderColor;
				} else {
					mBrowseOption.BackgroundColor = Colors.Get.MenuButtonBackgroundColorDefault;
					mBrowseOption.IconName = string.Empty;
					mBrowseOption.Disabled = false;
				}
				options.Add(mBrowseOption);
			}
		}

		public void OnPlayerUse()
		{
			GameObject browserGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUILibraryCatalogueBrowser"));
			GUILibraryCatalogueBrowser browser = browserGameObject.GetComponent <GUILibraryCatalogueBrowser>();
			browser.SetLibraryName(State.LibraryName);
		}

		public void OnPlayerUseWorldItemSecondary(object result)
		{
			WIListResult secondaryResult = result as WIListResult;
			switch (secondaryResult.SecondaryResult) {
				case "Browse":
					GameObject browserGameObject = GUIManager.SpawnNGUIChildEditor(gameObject, GUIManager.Get.Dialog("NGUILibraryCatalogueBrowser"));
					GUILibraryCatalogueBrowser browser = browserGameObject.GetComponent <GUILibraryCatalogueBrowser>();
					browser.SetLibraryName(State.LibraryName);
					break;

				default:
					break;
			}
		}

		protected WIListOption mBrowseOption = null;
	}

	[Serializable]
	public class LibraryCatalogueState
	{
		public string LibraryName = "GuildLibrary";
	}
}
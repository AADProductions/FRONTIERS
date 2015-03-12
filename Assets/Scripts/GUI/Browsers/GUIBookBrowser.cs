using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIBookBrowser : GUIBrowserSelectView <Book>
		{
				[BitMask(typeof(BookStatus))]
				public BookStatus DisplayStatus	= BookStatus.Received;
				[BitMask(typeof(BookType))]
				public BookType DisplayType = BookType.Book;
				public bool CreateEmptyDivider;
				public bool CreateMissionDivider;
				public bool CreateSkillDivider;
				public bool CreateLoreDivider;
				public bool CreateMiscDivider;
				public bool CreateGuidebookDivider;
				GUITabPage TabPage;

				public override void GetActiveInterfaceObjects(List<Widget> currentObjects)
				{
						//this will get everything on all tabs
						GUILogInterface.Get.GetActiveInterfaceObjects(currentObjects);
				}

				public override void WakeUp()
				{
						TabPage = gameObject.GetComponent <GUITabPage>();
						TabPage.OnDeselected += OnDeselected;
				}

				public override void Start()
				{
						base.Start();
						//we're a parent of the log
						NGUICamera = GUIManager.Get.PrimaryCamera;
				}

				public override void Show()
				{
						base.Show();
						GUIDetailsPage.Get.Hide();//we don't use the details page in this browser
				}

				public void OnDeselected()
				{
						if (HasFocus) {
								GUIManager.Get.ReleaseFocus(this);
						}
				}

				public override IEnumerable <Book> FetchItems()
				{
						if (!Manager.IsAwake <GUIManager>()) {
								return null;
						}
						return Books.Get.BooksByStatusAndType(DisplayStatus, DisplayType);
				}

				public override bool PushToViewerAutomatically {
						get { 
								return false;
						}
				}

				public void OnFinishReading()
				{
						if (Visible) {
								GUIManager.Get.GetFocus(this);
						}
				}

				public override void CreateDividerObjects()
				{
						GUIGenericBrowserObject dividerObject = null;
						IGUIBrowserObject newDivider = null;

						if (CreateEmptyDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_empty";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "You haven't acquired any books.";
								dividerObject.Initialize("Divider");
						}

						if (CreateMissionDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "a_missionRelated";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Mission-related:";
								dividerObject.Initialize("Divider");
						}

						if (CreateSkillDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "e_skillRelated";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Skills:";
								dividerObject.Initialize("Divider");
						}

						if (CreateLoreDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "g_loreRelated";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Lore:";
								dividerObject.Initialize("Divider");
						}

						if (CreateMiscDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "t_miscRelated";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Miscellaneous:";
								dividerObject.Initialize("Divider");
						}

						if (CreateGuidebookDivider) {
								newDivider = CreateDivider();
								dividerObject = newDivider.gameObject.GetComponent <GUIGenericBrowserObject>();
								dividerObject.name = "x_guide";
								dividerObject.UseAsDivider = true;
								dividerObject.Name.color = Colors.Get.MenuButtonTextColorDefault;
								dividerObject.Name.text = "Guidebooks:";
								dividerObject.Initialize("Divider");
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						CreateEmptyDivider = true;
						CreateMissionDivider = false;
						CreateSkillDivider = false;
						CreateLoreDivider = false;
						CreateMiscDivider = false;
						CreateGuidebookDivider = false;
						base.PushEditObjectToNGUIObject();
				}

				protected override IGUIBrowserObject ConvertEditObjectToBrowserObject(Book editObject)
				{
						CreateEmptyDivider = false;

						IGUIBrowserObject newBrowserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIGenericBrowserObject bookBrowserObject = newBrowserObject.gameObject.GetComponent <GUIGenericBrowserObject>();
						//if the book hasn't been read yet keep it near the top
						Color bookColor = Colors.Get.BookColorGeneric;
						Color textColor = Color.white;
						string prefix = "z_";

						if (editObject.MissionRelated) {
								bookColor = Colors.Get.BookColorMission;
								CreateMissionDivider = true;
								prefix = "c_";
						} else if (editObject.SkillsToLearn.Count > 0 || editObject.SkillsToReveal.Count > 0) {
								bookColor = Colors.Get.BookColorSkill;
								CreateSkillDivider = true;
								prefix = "f_";
						} else if (editObject.CanonLore) {
								bookColor = Colors.Get.BookColorLore;
								CreateLoreDivider = true;
								prefix = "h_";
						} else if (editObject.Guidebook) {
								bookColor = Colors.Get.GeneralHighlightColor;
								CreateGuidebookDivider = true;
								prefix = "y_";
						} else {
								CreateMiscDivider = true;
								prefix = "u_";
						}

						bookBrowserObject.EditButton.target = this.gameObject;
						bookBrowserObject.EditButton.functionName = "OnClickBrowserObject";
						bookBrowserObject.Name.color = textColor;
						bookBrowserObject.Name.text = Data.GameData.InterpretScripts (editObject.Title, Profile.Get.CurrentGame.Character, null) + " - " + Colors.ColorWrap(editObject.ContentsSummary, Colors.Dim (textColor));
						bookBrowserObject.Icon.atlas = Mats.Get.IconsAtlas;
						bookBrowserObject.Icon.spriteName = Mats.Get.Icons.GetIconNameFromBookType(editObject.TypeOfBook);
						bookBrowserObject.Icon.color = Colors.Brighten(bookColor);
						bookBrowserObject.GeneralColor = bookColor;

						if (!Flags.Check((uint)editObject.Status, (uint)BookStatus.Read, Flags.CheckType.MatchAny)) {
								newBrowserObject.name = prefix + "_a_";
								bookBrowserObject.BackgroundHighlight.enabled = true;
								bookBrowserObject.BackgroundHighlight.color = Colors.Get.GeneralHighlightColor;
						} else {
								newBrowserObject.name = prefix + "_b_";
								bookBrowserObject.BackgroundHighlight.enabled = false;
						}
						newBrowserObject.name = prefix + editObject.Name;
						bookBrowserObject.Initialize(editObject.Name);
						bookBrowserObject.Refresh();
			
						return newBrowserObject;
				}

				protected override void RefreshEditObjectToBrowserObject(Book editObject, IGUIBrowserObject browserObject)
				{
						//		GUIGenericBrowserObject missionBrowserObject = browserObject.GetComponent <GUIGenericBrowserObject> ( );
						//		
						//		missionBrowserObject.EditButton.target 				= this.gameObject;
						//		missionBrowserObject.EditButton.functionName		= "OnClickBrowserObject";
						//		
						//		if (editObject.Props.Status == MissionStatus.Active)
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= true;
						//			missionBrowserObject.BackgroundHighlight.color		= Colors.Get.SuccessHighlightColor;
						//		}
						//		else if (editObject.Props.Status == MissionStatus.Failed)
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= true;
						//			missionBrowserObject.BackgroundHighlight.color		= Colors.Get.WarningHighlightColor;			
						//		}
						//		else
						//		{
						//			missionBrowserObject.BackgroundHighlight.enabled 	= false;
						//		}		
				}

				public override void PushSelectedObjectToViewer()
				{
						//Missions.Get.MissionStateByName (mSelectedObject.Name);
						Books.ReadBook(mSelectedObject.Name, OnFinishReading);
				}
		}
}
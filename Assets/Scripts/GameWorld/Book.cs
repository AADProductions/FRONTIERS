#pragma warning disable 0219
using UnityEngine;
using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Xml.Serialization;
using Frontiers.Data;

namespace Frontiers.World
{
		[Serializable]
		public class Book : Mod
		{
				public override bool IgnoreProfileDataIfOutdated {
						get {
								return true;
						}
				}

				public string Title = string.Empty;
				public BookStatus Status = BookStatus.Dormant;
				public BookType TypeOfBook = BookType.Book;
				public int PrototypeIndex = 0;
				public bool ManualPlacementOnly = false;
				public bool MissionRelated = false;
				public bool Guidebook = false;
				public bool CanonLore = false;
				public bool Fiction = false;
				public BookSealStatus SealStatus = BookSealStatus.None;
				public static string gDefaultBookTitle = "(Untitled)";
				public string DefaultTemplate = string.Empty;
				public List <string> Authors = new List <string>();
				public string ContentsSummary = string.Empty;
				public List <string> SkillsToReveal = new List<string>();
				public List <string> SkillsToLearn = new List<string>();
				public List <string> LocationsToReveal = new List<string>();
				public List <string> CharactersToReveal = new List<string>();
				public List <string> SkillsRead = new List<string>();
				public int NumChapters {
						get {
								return BookChapters.Count;
						}
				}
				public int LastChapterRead = 0;
				public int NumCopiesInExistence	= 0;
				public int NumCopiesSpawned = 0;
				public int NumCopiesReceived = 0;
				[Multiline (15)]
				public string Text = string.Empty;
				public bool MultiChapterType {
						get {
								return (TypeOfBook == BookType.Book
								||	TypeOfBook == BookType.Diary
								||	TypeOfBook == BookType.Scripture
								||	TypeOfBook == BookType.Scroll);
						}
				}

				public string CleanTitle {
						get {
								if (!string.IsNullOrEmpty(Title)) {
										return Title;
								}
								return gDefaultBookTitle;
						}
				}

				public bool ReadChapter(int chapterNumber, out Chapter chapter)
				{
						if (!mHasGeneratedChapters) {
								BuildBookChapters();
						}

						if (chapterNumber < 0 || chapterNumber >= BookChapters.Count) {
								chapter = null;
								return false;
						}
			
						if (chapterNumber > LastChapterRead) {
								LastChapterRead = chapterNumber;
						}

						chapter = BookChapters[chapterNumber];

						if (chapter.MissionsToActivate.Count > 0) {
								if (chapter.MissionsToActivate.Count > 1) {
										//if we have more than one mission to activate
										//we ignore any objectives and just activate the missions
										//(i don't like this but the format is locked so whatever)
										foreach (string missionToActivate in chapter.MissionsToActivate) {
												Missions.Get.ActivateMission(missionToActivate, MissionOriginType.Book, Name);
										}
								} else {
										string missionToActivate = chapter.MissionsToActivate[0];
										if (chapter.MissionObjectivesToActivate.Count > 0) {
												//activate the objectives instead of the mission
												foreach (string objectiveToActivate in chapter.MissionObjectivesToActivate) {
														Missions.Get.ActivateObjective(missionToActivate, objectiveToActivate, MissionOriginType.Book, Name);
												}
										} else {
												//otherwise just activate the mission
												Missions.Get.ActivateMission(missionToActivate, MissionOriginType.Book, Name);
										}
								}
						}

						foreach (string blueprintToReveal in chapter.BlueprintsToReveal) {
								Blueprints.Get.Reveal(blueprintToReveal, BlueprintRevealMethod.Book, CleanTitle);
						}
						foreach (string skillToLearn in chapter.SkillsToLearn) {
								Skills.LearnSkill(skillToLearn);
						}
						foreach (string skillToReveal in chapter.SkillsToReveal) {
								Skills.RevealSkill(skillToReveal);
						}
						foreach (string locationToReveal in chapter.LocationsToReveal) {
								MobileReference mr = new MobileReference(locationToReveal);
								Player.Local.Surroundings.Reveal(mr);
								Profile.Get.CurrentGame.MarkedLocations.SafeAdd(mr);
						}
						foreach (string structureToAquire in chapter.StructuresToAquire) {
								MobileReference mr = new MobileReference(structureToAquire);
								Player.Local.Inventory.AcquireStructure(mr, true);
						}

						//this is now done in the chapter on the fly
						//chapter.Contents = Frontiers.Data.GameData.InterpretScripts (chapter.Contents, Profile.Get.CurrentGame.Character.Gender, null);

						//if (chapterNumber == mBookChapters.LastIndex()) {
						//TODO temp - book reading will go back to chapter-based eventually
								OnFullyRead();
						//}
			
						return true;
				}

				public void BuildBookChapters()
				{		//this takes the raw book data and interprets it as actual chapters
						//i do this in game instead of on import to keep the system flexible
						//modders may be able to supply their own chapter-building functions, etc.
						BookChapters.Clear();
						//split the text into pages using chapter break
						string text = Text.Replace("[bookend]", "");
						string[] chapters = text.Split(new string [] { gChapterBreakSeparator }, StringSplitOptions.RemoveEmptyEntries);
						#if UNITY_EDITOR
						Debug.Log (Name + " - " + chapters.Length.ToString());
						#endif
						for (int i = 0; i < chapters.Length; i++) {
								Chapter bookChapter = new Chapter();
								//split the chpater into tags/content
								string[] chapterTags = chapters[i].Split(new string [] { gChapterStartSeparator }, StringSplitOptions.RemoveEmptyEntries);
								//anything before the chapter start will have tags
								if (chapterTags.Length > 1) {
										string[] splitChapterTags = chapterTags[0].Split(gNewlineSeparators, StringSplitOptions.RemoveEmptyEntries);
										for (int k = 0; k < splitChapterTags.Length; k++) {
												string[] splitFieldLine = splitChapterTags[k].Split(new string [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
												string fieldName = splitFieldLine[0].Replace(gTagSeparator, "");
												if (splitFieldLine.Length > 1) {
														string fieldValue = splitFieldLine[1];
														GameData.SetField(bookChapter, fieldName, fieldValue);
												}
										}
										//anything after the chapter start tag will be content
										bookChapter.Contents = chapterTags[1].Replace("\n", "");
										if (!string.IsNullOrEmpty(bookChapter.Contents)) {
												BookChapters.Add(bookChapter);
										}
								}
								#if UNITY_EDITOR
								bookChapter.EditorFormattedContents = bookChapter.FormattedContents;
								#endif
						}
						mHasGeneratedChapters	= true;
				}

				public void OnFullyRead()
				{
						foreach (string skillToLearn in SkillsToLearn) {
								Skills.LearnSkill(skillToLearn);
								Skills.MarkSkill(skillToLearn);
						}
						foreach (string locationToReveal in LocationsToReveal) {
								MobileReference mr = new MobileReference(locationToReveal);
								Player.Local.Surroundings.Reveal(mr);
								Profile.Get.CurrentGame.MarkedLocations.SafeAdd(mr);//mark the locatino even if we already know about it
						}			
						Status |= BookStatus.FullyRead;
				}

				[XmlIgnore]
				public List <Chapter> BookChapters = new List <Chapter>();
				protected bool mHasGeneratedChapters = false;
				protected static string[] gNewlineSeparators = new string [] { "\n", "\n\r" };
				protected static string gChapterBreakSeparator = "[chapterbreak]";
				protected static string gChapterStartSeparator = "[chapterstart]";
				protected static string gTagSeparator = "#set ";
		}

		[Serializable]
		public class Chapter
		{
				public Chapter()
				{
						Contents = string.Empty;
						Language = "Common";
						SkillsToReveal = new List <string>();
						SkillsToLearn = new List <string>();
						LocationsToReveal = new List <string>();
						MissionsToActivate = new List <string>();
						MissionObjectivesToActivate = new List <string>();
						StructuresToAquire = new List <string>();
						FontName = "Default";
				}
				[Multiline (5)]
				public string Contents;
				#if UNITY_EDITOR
				[Multiline (15)]
				public string EditorFormattedContents;
				public static PlayerCharacter gTempCharacter = PlayerCharacter.Default();
				#endif
				public string Language;

				public string FormattedContents {
						get {
								#if UNITY_EDITOR
								if (!Application.isPlaying) {
										return FormatString(GameData.InterpretScripts(Contents, gTempCharacter, null));
								}
								#endif
								return FormatString(GameData.InterpretScripts(Contents, Profile.Get.CurrentGame.Character, null));
						}
				}

				protected string FormatString(string contents)
				{
						contents = contents.Replace(breakString, breakStringReplacement);
						contents = contents.Replace(dividerString, dividerStringReplacement);
						if (AutoIndent) {
								contents = contents.Replace(paragraphString, autoIndentParagraphStringReplacement);
						} else {
								contents = contents.Replace(paragraphString, nonIndentParagraphStringReplacement);
						}
						return contents;
				}

				public List <string> SkillsToReveal = new List<string>();
				public List <string> SkillsToLearn = new List<string>();
				public List <string> LocationsToReveal = new List<string>();
				public List <string> MissionsToActivate = new List<string>();
				public List <string> MissionObjectivesToActivate = new List<string>();
				public List <string> BlueprintsToReveal = new List<string>();
				public List <string> StructuresToAquire = new List<string>();
				public string FontName;
				public string FontColor;
				public string HorizontalAlignment = "Left";
				public string VerticalAlignment = "Top";
				public bool AutoIndent = true;

				[XmlIgnore]
				public UIWidget.Pivot ChapterAlignment {
						get {
								UIWidget.Pivot chapterAlignment = UIWidget.Pivot.TopLeft;
								switch (HorizontalAlignment) {
										case "Left":
										default:
												switch (VerticalAlignment) {
														case "Top":
														default:
																chapterAlignment = UIWidget.Pivot.TopLeft;
																break;

														case "Center":
																chapterAlignment = UIWidget.Pivot.Left;
																break;

														case "Bottom":
																chapterAlignment = UIWidget.Pivot.BottomLeft;
																break;
												}
												break;

										case "Right":
												switch (VerticalAlignment) {
														case "Top":
														default:
																chapterAlignment = UIWidget.Pivot.TopRight;
																break;

														case "Center":
																chapterAlignment = UIWidget.Pivot.Right;
																break;

														case "Bottom":
																chapterAlignment = UIWidget.Pivot.BottomRight;
																break;
												}
												break;

										case "Center":
												switch (VerticalAlignment) {
														case "Top":
														default:
																chapterAlignment = UIWidget.Pivot.Top;
																break;

														case "Center":
																chapterAlignment = UIWidget.Pivot.Center;
																break;

														case "Bottom":
																chapterAlignment = UIWidget.Pivot.Bottom;
																break;
												}
												break;
								}
								return chapterAlignment;
						}
				}

				[XmlIgnore]
				public Color ChapterFontColor {
						get {
								Color fontColor = Color.black;
								if (!string.IsNullOrEmpty(FontColor)) {
										switch (FontColor.ToLower().Trim()) {
												case "black":
														break;

												case "blue":
														fontColor = Color.blue;//TEMP
														break;

												case "green":
														fontColor = Color.green;
														break;

												case "red":
														fontColor = Color.red;
														break;

												case "purple":
														fontColor = Color.Lerp(Color.blue, Color.red, 0.5f);
														break;
										}
								}
								return fontColor;
						}
				}

				protected static string breakString = "[break]";
				protected static string paragraphString = "[paragraph]";
				protected static string dividerString = "[divider]";
				protected static string autoIndentParagraphStringReplacement = "\n\n     ";
				protected static string nonIndentParagraphStringReplacement = "\n\n";
				protected static string breakStringReplacement = "\n";
				protected static string dividerStringReplacement = "_";
		}

		[Serializable]
		public class BookInk
		{
				public string InkName;
				public int FontColorIndex = 0;
				public bool LuminiteInk = false;
				public bool InvisibleInk = false;
		}

		[Serializable]
		public class BookFont
		{
				public string FontName;
				public UIFont NGUIFont;
				public float DisplayScale;
				public int MaxCharsPerChapter;
				public string RegexStrip = string.Empty;
		}
}
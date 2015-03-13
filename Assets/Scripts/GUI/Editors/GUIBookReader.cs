using UnityEngine;
using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUIBookReader : GUIEditor <Book>
		{
				public UICheckbox WhiteOnBlack;
				public UICheckbox SimpleMode;
				public UILabel TitleLabel;
				public UILabel AuthorsLabel;
				public UILabel FormattingLabel;
				public List <BookReaderDisplay> Displays = new List<BookReaderDisplay>();
				public BookReaderDisplay CurrentDisplayLeft;
				public BookReaderDisplay CurrentDisplayRight;
				public DisplayType Type;
				public List <Light> Lighting = new List <Light>();
				public bool ForceWhiteOnBlack = true;
				public Action OnFinishReading;
				public GameObject NextChapterButton;
				public GameObject PrevChapterButton;
				public UILabel NextChapterButtonLabel;
				public UILabel PrevChapterButtonLabel;
				public GameObject FinishedReadingButton;
				public UIScrollBar CurrentScrollBar;
				public float ScrollBarSensitivity = 0.05f;

				public override void WakeUp()
				{
						base.WakeUp();

						SimpleMode.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
						WhiteOnBlack.gameObject.SetLayerRecursively(Globals.LayerNumHidden);
						FormattingLabel.gameObject.SetActive(false);
						OnLoseFocus += Finish;
				}

				public override void Start()
				{
						base.Start();
						NextChapterButtonLabel.text = "Chapter 2";
						PrevChapterButtonLabel.text = "Chapter 1";
						NextChapterButton.SetActive(false);
						PrevChapterButton.SetActive(false);
						CurrentScrollBar.scrollValue = 0f;
						WhiteOnBlack.gameObject.SetActive(false);
						SimpleMode.gameObject.SetActive(false);
				}

				public override void EnableInput()
				{
						for (int i = 0; i < Lighting.Count; i++) {
								Lighting[i].enabled = true;
						}
						base.EnableInput();
				}

				public override void DisableInput()
				{
						for (int i = 0; i < Lighting.Count; i++) {
								Lighting[i].enabled = false;
						}
						base.DisableInput();
				}

				public void Hide()
				{
						if (CurrentDisplayLeft != null) {
								CurrentDisplayLeft.gameObject.SetActive(false);
						}
						if (CurrentDisplayRight != null) {
								CurrentDisplayRight.gameObject.SetActive(false);
						}
				}

				public void OnSetWhiteOnBlack()
				{
						if (mEditObject == null)
								return;

						mUpdateDisplayNextFrame = true;
				}

				public void OnSetSimpleMode()
				{
						if (mEditObject == null)
								return;

						mUpdateDisplayNextFrame = true;
				}

				public void SetDisplayType()
				{
						DisplayType displayType = DisplayType.DualPage;

						CurrentDisplayRight = null;
						CurrentDisplayLeft = null;

						if (SimpleMode.isChecked) {
								displayType = DisplayType.Simple;
						} else if (WhiteOnBlack.isChecked || ForceWhiteOnBlack) {
								displayType = DisplayType.WhiteOnBlack;
						} else {
								if (EditObject.MultiChapterType) {
										if (EditObject.TypeOfBook == BookType.Scroll) {
												displayType = DisplayType.SinglePageMulti;
										} else {
												displayType = DisplayType.DualPage;
										}
								} else {
										if (EditObject.TypeOfBook == BookType.Scrap) {
												displayType = DisplayType.SinglePageScrap;
										} else {
												displayType = DisplayType.SinglePageSingle;
										}
								}
						}

						Type = displayType;
						//Debug.Log ("GUIBookReader Set display type to " + Type.ToString ());

						foreach (BookReaderDisplay display in Displays) {
								if (display.DisplayTypes.Contains(Type)) {
										display.gameObject.SetActive(true);
										if (display.Left) {
												//Debug.Log ("GUIBookReader Found display left");
												CurrentDisplayLeft = display;
										} else {
												//Debug.Log ("GUIBookReader Found display right");
												CurrentDisplayRight = display;
										}
								} else {
										display.gameObject.SetActive(false);
								}
						}

						if (CurrentDisplayLeft == null) {
								//Debug.Log ("GUIBookReader WTF this should never happen - setting white on black as a failsafe");
								CurrentDisplayLeft = Displays[0];
						}

						FormattingLabel.gameObject.SetActive(false);
				}

				public override void PushEditObjectToNGUIObject()
				{
						//Debug.Log ("GUIBookReader Displaying book " + EditObject.Name);
						//refresh props that are always visibel
						if (string.IsNullOrEmpty(EditObject.Title)) {
								TitleLabel.text = "(No title)";
						} else {
								TitleLabel.text = EditObject.Title;
						}
						if (EditObject.Authors.Count > 0) {
								AuthorsLabel.text = "Written by: " + string.Join(", ", EditObject.Authors.ToArray());
						} else {
								AuthorsLabel.text = "(No author)";
						}

						SetDisplayType();
						mCurrentChapterNumber = 0;
						TryToDisplayChapter(mCurrentChapterNumber);
						mUpdateScrollbarNextFrame = true;
				}

				public void OnClickNextChapterButton()
				{
						int newChapterNumber = mCurrentChapterNumber + 1;
						//try to go to the next chapter
						if (TryToDisplayChapter(newChapterNumber)) {
								mCurrentChapterNumber = newChapterNumber;
								mUpdateScrollbarNextFrame = true;
						}
						//if we can't read the chapter then we've reached the end of the book
						//do nothing
				}

				public void OnClickPrevChapterButton()
				{
						int newChapterNumber = mCurrentChapterNumber - 1;
						//try to go to the next chapter
						if (TryToDisplayChapter(newChapterNumber)) {
								mCurrentChapterNumber = newChapterNumber;
								mUpdateScrollbarNextFrame = true;
						}
						//if we can't read the chapter then we've reached the end of the book
						//do nothing
				}

				protected bool TryToDisplayChapter(int chapterNumber)
				{
						bool result = false;
						//get the current chapter
						Chapter newChapter = null;
						if (EditObject.ReadChapter(chapterNumber, out newChapter)) {
								//Debug.Log ("GUIBookReader Trying to display chapter " + chapterNumber + ", failed");
								//if we can read this chapter number, great - set it as our official chapter number
								mCurrentChapterNumber = chapterNumber;
								mCurrentChapter = newChapter;
								result = true;
						}

						if (result) {
								//Debug.Log ("GUIBookReader successfully loaded chapter " + chapterNumber);
								//load the chapter
								BookFont font = Books.Get.SimpleFont;
								Color color = Color.black;//Color.white;
								//if (Type != DisplayType.Simple) {
								font = Books.Get.GetFont(mCurrentChapter.FontName);
								if (Type != DisplayType.WhiteOnBlack) {
										color = mCurrentChapter.ChapterFontColor;
								}
								//}
								if (font.NGUIFont == null) {
										//Debug.Log ("Font NGUI font was null using font name " + mCurrentChapter.FontName);
								}
								CurrentDisplayLeft.SetChapterProperties(
										mCurrentChapter.FormattedContents,
										mCurrentChapter.ChapterAlignment,
										font.NGUIFont,
										font.DisplayScale,
										color,
										(mEditObject.NumChapters > 0),
										mCurrentChapterNumber);

								CurrentDisplayLeft.RefreshChapterFormat();

								if (CurrentDisplayRight != null) {
										//Debug.Log ("GUIBookReader Using display right");
										CurrentDisplayRight.SetChapterProperties(
												mCurrentChapter.FormattedContents,
												mCurrentChapter.ChapterAlignment,
												font.NGUIFont,
												font.DisplayScale,
												color,
												(mEditObject.NumChapters > 0),
												mCurrentChapterNumber);

										CurrentDisplayRight.RefreshChapterFormat();
								}
						}
						//if we can't, then don't set our chapter number
						return result;
				}

				protected void SetChapterButtons()
				{
						if (mEditObject.NumChapters > 1) {
								//if we have more than one chapter
								if (mCurrentChapterNumber < (mEditObject.NumChapters - 1)) {
										//if the current chapter isn't the last chapter
										NextChapterButton.SetActive(true);
										NextChapterButtonLabel.text = "Chapter " + (mCurrentChapterNumber + 2).ToString();
								} else {
										NextChapterButton.SetActive(false);
								}
								if (mCurrentChapterNumber > 0) {
										//if the current chapter isn't the first chapter
										PrevChapterButton.SetActive(true);
										PrevChapterButtonLabel.text = "Chapter " + (mCurrentChapterNumber).ToString();
								} else {
										PrevChapterButton.SetActive(false);
								}
						} else {
								NextChapterButton.SetActive(false);
								PrevChapterButton.SetActive(false);
						}
				}

				public void Update()
				{
						if (mEditObject == null)
								return;

						SetChapterButtons();

						if (mUpdateDisplayNextFrame) {
								SetDisplayType();
								TryToDisplayChapter(mCurrentChapterNumber);
								CurrentScrollBar.scrollValue = 0f;
								mUpdateDisplayNextFrame = false;
								mUpdateScrollbarNextFrame = true;
						}

						if (mUpdateScrollbarNextFrame) {
								mUpdateScrollbarNextFrame = false;
								CurrentScrollBar.scrollValue = 0f;
								CurrentScrollBar.ForceUpdate();
						}
				}

				public void OnClickFinishedReadingButton()
				{
						Finish();
				}

				protected override void OnFinish()
				{
						if (mFinished)
								return;

						if (mEditObject != null) {
								Mods.Get.Runtime.SaveMod <Book>(mEditObject, "Book", mEditObject.Name);
								Player.Get.AvatarActions.ReceiveAction(AvatarAction.BookRead, WorldClock.AdjustedRealTime);
						}

						base.OnFinish();
						OnFinishReading.SafeInvoke();
				}

				protected bool mVisible = false;
				protected Chapter mCurrentChapter;
				protected int mCurrentChapterNumber = 0;
				protected bool mUpdateDisplayNextFrame = false;
				protected bool mUpdateScrollbarNextFrame = false;

				public enum DisplayType
				{
						WhiteOnBlack,
						DualPage,
						SinglePageSingle,
						SinglePageMulti,
						SinglePageScrap,
						Simple
				}
		}
}

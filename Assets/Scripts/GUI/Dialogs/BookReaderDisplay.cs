using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.GUI
{
	public class BookReaderDisplay : MonoBehaviour
	{
		public string ChapterText {
			get {
				return mChapterText;
			}
		}

		public UILabel ChapterLabel;
		public UILabel TextLabel;
		public UILabel FormatLabel;
		public UILabel NextChapterLabel;
		public UILabel PrevChapterLabel;
		public UIFont ChapterFont;
		public Color ChapterFontColor;
		public float ChapterFontSize;
		public bool DisplayChapter;
		public UIWidget.Pivot ChapterAlignment;
		public GameObject AlignmentObject;
		public List <GUIBookReader.DisplayType> DisplayTypes = new List<GUIBookReader.DisplayType>();
		public bool Left = true;

		public void SetChapterProperties(string chapterText, UIWidget.Pivot chapterAlignment, UIFont chapterFont, float chapterFontSize, Color chapterFontColor, bool displayChapter, int chapterNumber)
		{
			mChapterText = chapterText;
			DisplayChapter = displayChapter;
			ChapterFont = chapterFont;
			ChapterFontSize = chapterFontSize;
			ChapterAlignment = chapterAlignment;
			ChapterFontColor = chapterFontColor;
			mChapterFormatLoaded = true;
			mChapterNumber = chapterNumber;
		}

		public int ChapterNumber {
			get {
				return mChapterNumber;
			}
		}

		public void Start()
		{
			Initialize();
		}

		public void Initialize()
		{
			ChapterFontColor = Color.black;

			mTopLeftPosition = TextLabel.transform.localPosition;
			mLabelScale = TextLabel.relativeSize * TextLabel.transform.localScale.x;

			//Clear ();
		}

		public void Clear()
		{
			TextLabel.text = string.Empty;
			TextLabel.pivot = UIWidget.Pivot.TopLeft;
			mChapterNumber = 0;
		}

		public void RefreshChapterFormat()
		{
			if (!mChapterFormatLoaded) {
				//Debug.Log ("Chapter format not loeaded");
				return;
			}

			mTextScale.Set(ChapterFontSize, ChapterFontSize, 1f);

			TextLabel.font = ChapterFont;
			TextLabel.pivot = ChapterAlignment;
			//TextLabel.transform.localScale = mTextScale;
			TextLabel.color = ChapterFontColor;
			TextLabel.MarkAsChanged();

//						Vector2 fontSize = ChapterFont.CalculatePrintedSize("The Quick Brown Fox", true);
//						mMaxLineWidth = TextLabel.lineWidth;
//						mAverageLineHeight = fontSize.y * TextLabel.transform.localScale.y;
//						mMaxLabelHeight = TextLabel.relativeSize.y * mAverageLineHeight;

			//FormatLabel.font = ChapterFont;
			//FormatLabel.transform.localScale = Vector3.one * ChapterFontSize;
			//FormatLabel.lineWidth = mMaxLineWidth;
			//FormatLabel.transform.localScale = mTextScale;
			//FormatLabel.pivot = ChapterAlignment;
			//FormatLabel.MarkAsChanged();

			mAlignmentPosition = Vector3.zero;
			switch (ChapterAlignment) {
			//TOP
				case UIWidget.Pivot.TopLeft:
				default:
								//do nothing, it's top left by default
					break;

				case UIWidget.Pivot.Top://Center
					mAlignmentPosition.Set(mLabelScale.x / 2, 0f, 0f);
					break;

				case UIWidget.Pivot.TopRight:
					mAlignmentPosition.Set(mLabelScale.x, 0f, 0f);
					break;

			//CENTER
				case UIWidget.Pivot.Left:
					mAlignmentPosition.Set(0f, -mLabelScale.y / 2, 0f);
					break;

				case UIWidget.Pivot.Center:
					mAlignmentPosition.Set(mLabelScale.x / 2, -mLabelScale.y / 2, 0f);
					break;

				case UIWidget.Pivot.Right:
					mAlignmentPosition.Set(mLabelScale.x, -mLabelScale.y / 2, 0f);
					break;

			//BOTTOM
				case UIWidget.Pivot.BottomLeft:
					mAlignmentPosition.Set(0, -mLabelScale.y, 0f);
					break;

				case UIWidget.Pivot.Bottom:
					mAlignmentPosition.Set(mLabelScale.x / 2, -mLabelScale.y, 0f);
					break;

				case UIWidget.Pivot.BottomRight:
					mAlignmentPosition.Set(mLabelScale.x, -mLabelScale.y, 0f);
					break;
			}

			AlignmentObject.transform.localPosition = mAlignmentPosition;
			TextLabel.text = mChapterText;
			if (DisplayChapter) {
				ChapterLabel.text = "_Chapter " + (mChapterNumber + 1).ToString() + "_";
			} else {
				ChapterLabel.text = string.Empty;
			}
		}

		protected bool mChapterFormatLoaded = false;
		protected int mChapterNumber = 0;
		protected float mMaxLabelHeight = 0f;
		protected float mAverageLineHeight = 0f;
		protected int mMaxLineWidth = 0;
		protected Vector3 mTopLeftPosition = Vector3.zero;
		protected Vector3 mLabelScale = Vector3.zero;
		protected Vector3 mTextScale = new Vector3(30f, 30f, 30f);
		protected Vector3 mAlignmentPosition = Vector3.zero;
		protected string mChapterText;
	}
}
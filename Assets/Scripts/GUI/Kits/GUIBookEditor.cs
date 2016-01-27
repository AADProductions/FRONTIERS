//#pragma warning disable 0219
//using UnityEngine;
//using System;
//using System.Runtime.Serialization;
//using System.Xml;
//using System.Xml.Serialization;
//using System.IO;
//using System.Collections.Generic;
//using Frontiers;
//using Frontiers.World;
//
//public class GUIBookEditor : MonoBehaviour
//{	
//	public BackerKit			Kit;
//	
//	public UIAnchor				BookEditorAnchor;
//	public UIAnchor				DisplayOptionsAnchor;
//	public UIAnchor				BookReaderAnchor;
//	public Vector2				BookEditorAnchorTarget;
//	public Vector2				DisplayOptionsAnchorTarget;
//	public Vector2				BookReaderAnchorTarget;
//	public GUIBookReader		BookReader;
//	public UICheckbox			ViewFocusHighlightCheckbox;
//	public UICheckbox			ViewSpecialHighlightCheckbox;
//	public AudioSource			Music;
//	public UICheckbox			MusicCheckbox;
//	
//	public UILabel				AverageFPSLabel;
//	public SystemInfoCollector	FPS;
//
//	public Book					CurrentBook;
//	public BookAvatar			CurrentBookAvatar;
//	public GameObject			BookCameraPivot;
//	public GameObject			BookPreviewButton;
//	public Camera				ViewerCamera;
//
//	public GameObject			PageEditorEditorObject;
//	public UILabel				PageEditorCharsPerPageNumLabel;
//	public UILabel				PageEditorCharsSoFarNumLabel;
//	public UILabel				PageEditorPageNumLabel;
//	public UIInput				PageEditorInput;
//	public UICheckbox			PageEditorDisplayFontCheckbox;
//	public UIPopupList			PageEditorFontPopupList;
//	public UIPopupList			PageEditorFontInkPopupList;
//	public GameObject			PageEditorNextPageButton;
//	public GameObject			PageEditorPrevPageButton;
//	public Vector3				PageEditorTargetPosition;
//	public Vector3				PageEditorHiddenPosition;
//	public Vector3				PageEditorVisiblePosition;
//	public UIFont				PageEditorFont;
//	public UISprite				PageEditorBorderHighlight;
//	public string				CurrentPageContents
//	{
//		get
//		{
//			if (mCurrentPage >= 0)
//			{
//				return CurrentBook.Pages [mCurrentPage].Contents;
//			}
//			else
//			{
//				return string.Empty;
//			}
//		}
//		set
//		{
//			if (mCurrentPage >= 0)
//			{
//				CurrentBook.Pages [mCurrentPage].Contents = value;
//			}
//		}
//	}
//	
//	public GameObject			HelpWindowObject;
//	public GameObject			AddPageButton;
//	public GameObject			ConfirmSubmissionWindow;
//	
//	public Vector3				ConfirmWindowTargetPosition;
//	public Vector3				ConfirmWindowVisiblePosition;
//	public Vector3				ConfirmWindowHiddenPosition;
//
//	public UIInput				AuthorsInput;
//	public UIInput				SummaryInput;
//	public UIInput				TitleInput;
//
//	public UIPopupList			SealedWithWaxPopup;
//	public UISlider				AgeSlider;
//
//	public UISlider				CameraZoomSlider;
//	public UICheckbox			RotateCameraCheckbox;
//	public float				CameraZoomMultiplier 	= 100.0f;
//	public float				CameraZoomMin			= 50.0f;
//
//	public float				StartPosition;
//	public float				PositionOffset;
//	public float				PositionTarget;
//
//	public UISlider				BookStyleSlider;
//	public UIPopupList			Type;
//
//	public GameObject			PageBrowserObjectPrototype;
//	public UIGrid				PageBrowserGrid;
//
//	public UILabel				FileSavedTo;
//	public UILabel				SubmittingText;
//	public UIInput				KickstarterEmail;
//	public UICheckbox			LicenseCheckbox;
//	public UICheckbox			SystemInfoCheckbox;
//	public GameObject			ButtonToFreeze;
//	public GameObject			SubmitButton;
//	public GameObject			WorkingHaze;
//
//	public bool					RefreshOnStopWorking = false;
//
//	public void					Start ( )
//	{
//		BookReader.Show ( );
//		FileSavedTo.text 				= string.Empty;
//		SubmittingText.text				= string.Empty;
//		WorkingHaze.SetActive (false);
//		RefreshAll ( );
//		PositionTarget 					= StartPosition;
//		BookStyleSlider.eventReceiver	= gameObject;
//		Type.eventReceiver				= gameObject;
//		BookStyleSlider.functionName	= "OnBookStyleChange";
//		Type.functionName				= "OnChangetBookType";
//		Kit.CheckKitState ( );
//	}
//	
//	public void					OnSubmitWithEnter ( )
//	{
//		OnInsertNewline ( );
//	}
//
//	public void					OnClickPasteText ( )
//	{
//		PageEditorInput.text 		+= ClipboardHelper.clipBoard;
//		PageEditorInput.selected	= true;
//	}
//
//	public void					OnInsertNewline ( )
//	{
////		BookFont font = Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
//		PageEditorInput.text 		+= "\n";
//		mSelectPageInput			= true;
//	}
//
//	public void					OnInsertBreak ( )
//	{
//		BookFont font = Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
//		int newlineIndex = PageEditorInput.text.Length - 1;
//		if (newlineIndex > 0 && PageEditorInput.text [newlineIndex] == '\n')
//		{
//			PageEditorInput.text += (font.BreakString + "\n");
//		}
//		else
//		{
//			PageEditorInput.text += ("\n" + font.BreakString + "\n");
//		}
//		PageEditorInput.selected = true;
//	}
//
//	public void					OnInsertPageBreak ( )
//	{
//		mInitialized = false;
//		BookFont font = Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
//		List <string> replacements = new List <string> ( );
//		replacements.Add ("\n" + font.PageBreakString + "\n");
//		replacements.Add ("\n" + font.PageBreakString);
//		replacements.Add (font.PageBreakString + "\n");
//		replacements.Add (font.PageBreakString);
//
//		foreach (string replacement in replacements)
//		{
//			PageEditorInput.text = PageEditorInput.text.Replace (replacement, "\n");
//		}
//		mInitialized = true;
//		if (PageEditorInput.text [PageEditorInput.text.Length - 1] == '\n')
//		{
//			PageEditorInput.text += (font.PageBreakString + "\n");
//		}
//		else
//		{
//			PageEditorInput.text += ("\n" + font.PageBreakString + "\n");
//		}
//		PageEditorInput.selected = true;
//	}
//
//	public void					OnInsertDivider ( )
//	{
//		mInitialized = false;
//		BookFont font = Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
////		List <string> replacements = new List <string> ( );
////		replacements.Add (font.DividerString);
////		replacements.Add (font.DividerString + "\n");
////		replacements.Add ("\n" + font.DividerString + "\n");
////		replacements.Add ("\n" + font.DividerString);
////
////		foreach (string replacement in replacements)
////		{
////			PageEditorInput.text = PageEditorInput.text.Replace (replacement, string.Empty);
////		}
//		mInitialized = true;
//		if (PageEditorInput.text [PageEditorInput.text.Length - 1] == '\n')
//		{
//			PageEditorInput.text += (font.DividerString + "\n");
//		}
//		else
//		{
//			PageEditorInput.text += ("\n" + font.DividerString + "\n");
//		}
//		PageEditorInput.selected = true;
//	}
//
//	public void					OnClickClearText ( )
//	{
//		PageEditorInput.text		= string.Empty;
//		PageEditorInput.selected	= true;
//	}
//
//	public void					OnClearIllegalCharacters ( )
//	{
//		PageEditorInput.selected	= false;
//		CurrentPageContents 		= PageEditorInput.text;
//		BookFont font 				= Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
//		PageEditorInput.text 		= CurrentPageContents;//(font.RegexStrip, font.BreakString);
//	}
//
//	public void					Update ( )
//	{
//		if (MusicCheckbox.isChecked)
//		{
//			if (!Music.enabled)
//			{
//				Music.enabled = true;
//			}
//		}
//		else if (Music.enabled)
//		{
//			Music.enabled = false;
//		}
//		
//		AverageFPSLabel.text = ((int) FPS.FPS).ToString ( );
//		
//		ViewerCamera.fieldOfView = (CameraZoomSlider.sliderValue * CameraZoomMultiplier) + CameraZoomMin;
//
//		float currentPosition = transform.localPosition.x;
//		transform.localPosition = new Vector3 (Mathf.Lerp (currentPosition, PositionTarget, 10f * Time.deltaTime), 0f, 0f);
//		
//		ConfirmSubmissionWindow.transform.localPosition = Vector3.Lerp (ConfirmSubmissionWindow.transform.localPosition, ConfirmWindowTargetPosition, 10 * Time.deltaTime);
//
//		if (RotateCameraCheckbox.isChecked)
//		{
//			BookCameraPivot.transform.Rotate (0f, 5f * Time.deltaTime, 0f);
//		}
//
//		PageEditorEditorObject.transform.localPosition = Vector3.Lerp (PageEditorEditorObject.transform.localPosition, PageEditorTargetPosition, 10f * Time.deltaTime);
//
//		if (mCurrentPage >= 0)
//		{			
//			if (mSelectPageInput)
//			{
//				PageEditorInput.selected = true;
//				mSelectPageInput = false;
//			}
//
//			PageEditorCharsPerPageNumLabel.text = PageEditorInput.maxChars.ToString ( );
//			if (PageEditorInput.selected)
//			{
//				PageEditorBorderHighlight.color = Colors.Get.GeneralHighlightColor;
//				PageEditorCharsSoFarNumLabel.text	= (PageEditorInput.label.text.Length - 12).ToString ( );
//			}
//			else
//			{
//				PageEditorBorderHighlight.color = Colors.Get.MenuButtonBackgroundColorDefault;
//				PageEditorCharsSoFarNumLabel.text	= (PageEditorInput.label.text.Length - 1).ToString ( );
//			}
//		}
//
//		if (!mPreviewing)
//		{
//			DisplayOptionsAnchor.relativeOffset = Vector2.Lerp (DisplayOptionsAnchor.relativeOffset, Vector2.zero, 10f * Time.deltaTime);
//			BookEditorAnchor.relativeOffset = Vector2.Lerp (BookEditorAnchor.relativeOffset, Vector2.zero, 10f * Time.deltaTime);
//			BookReaderAnchor.relativeOffset = Vector2.Lerp (BookReaderAnchor.relativeOffset, BookReaderAnchorTarget, 10f * Time.deltaTime);
//		}
//		else
//		{
//			DisplayOptionsAnchor.relativeOffset = Vector2.Lerp (DisplayOptionsAnchor.relativeOffset, DisplayOptionsAnchorTarget, 10f * Time.deltaTime);
//			BookEditorAnchor.relativeOffset = Vector2.Lerp (BookEditorAnchor.relativeOffset, BookEditorAnchorTarget, 10f * Time.deltaTime);
//			BookReaderAnchor.relativeOffset = Vector2.Lerp (BookReaderAnchor.relativeOffset, Vector2.zero, 10f * Time.deltaTime);
//		}
//
//		//
//		if (Kit.IsWorking)
//		{
//			SubmittingText.text	= Kit.display;
//			
//			if (!WorkingHaze.activeSelf)
//			{
//				WorkingHaze.SetActive (true);
//				RefreshOnStopWorking = true;
//			}
//		}
//		else
//		{
//			if (RefreshOnStopWorking)
//			{
//				RefreshAll ( );
//				RefreshOnStopWorking = false;
//			}
//			if (WorkingHaze.activeSelf)
//			{
//				WorkingHaze.SetActive (false);
//			}
//			SubmittingText.text	= string.Empty;
//		}
//	}
//
//	public void 				OnSetFocusHighlight ( )
//	{
////		//Debug.Log ("Setting focus highlight");
//		if (ViewFocusHighlightCheckbox.isChecked)
//		{
//			CurrentBookAvatar.gameObject.SendMessage ("OnGainPlayerFocus");
//		}
//		else
//		{
////			if (ViewSpecialHighlightCheckbox.isChecked)
////			{
////				CurrentBook.worlditem.Highlight.IsSpecialObject = true;
////			}
////			else
////			{
////				CurrentBook.worlditem.Highlight.IsSpecialObject = false;
////			}
//
//			CurrentBookAvatar.SendMessage ("OnLosePlayerFocus");
//		}
//	}
//
//	public void 				OnSetSpecialHighlight ( )
//	{
////		//Debug.Log ("Setting special highlight");
//		if (ViewFocusHighlightCheckbox.isChecked)
//		{
//			CurrentBookAvatar.gameObject.SendMessage ("OnGainPlayerFocus");
//		}
//		else
//		{
////			if (ViewSpecialHighlightCheckbox.isChecked)
////			{
////				CurrentBook.worlditem.Highlight.IsSpecialObject = true;
////			}
////			else
////			{
////				CurrentBook.worlditem.Highlight.IsSpecialObject = false;
////			}
//
//			CurrentBookAvatar.SendMessage ("OnLosePlayerFocus");
//		}
//	}
//
//	public List <string>		GetSavedBookList ( )
//	{
//		List <string> filesInDirectory = new List <string> ( );
//		string dataPath	= System.IO.Path.Combine (Application.dataPath, "Saved");
//		#if UNITY_STANDALONE_WIN
//		if (!Directory.Exists (dataPath))
//		{
//			Directory.CreateDirectory (dataPath);
//		}
//
//		System.IO.DirectoryInfo filesDirectory = new System.IO.DirectoryInfo (dataPath);
//		foreach (System.IO.FileInfo file in filesDirectory.GetFiles ( ))
//		{
//			if (file.Extension == ".frontiers")
//			{
//				filesInDirectory.Add (System.IO.Path.GetFileNameWithoutExtension (file.Name));
//			}
//		}
//		#endif
//		return filesInDirectory;
//	}
//	
//	public void					OnChangeSubmitPage ( )
//	{
//		if (LicenseCheckbox.isChecked && TestEmail.IsEmail (KickstarterEmail.text))
//		{
//			SubmitButton.SendMessage ("SetEnabled");
//		}
//		else
//		{
//			SubmitButton.SendMessage ("SetDisabled");
//		}
//		
//		if (SystemInfoCheckbox.isChecked)
//		{
//			FPS.RefreshSystemInfo ( );
//		}
//	}
//	
//	public void					OnClickSubmitButtonCancel ( )
//	{
//		HelpWindowObject.SendMessage ("Close");
//		ConfirmWindowTargetPosition 	= ConfirmWindowHiddenPosition;
//		
//	}
//	
//	public void					OnClickSubmitButtonConfirm ( )
//	{
//		if (Kit.IsWorking)
//		{
//			return;
//		}
//		
//		HelpWindowObject.SendMessage ("Close");
//		ConfirmWindowTargetPosition 	= ConfirmWindowVisiblePosition;
//	}
//
//	public void					OnClickSubmitButton ( )
//	{
////		HelpWindowObject.SendMessage ("Close");
////		ConfirmWindowTargetPosition 	= ConfirmWindowHiddenPosition;
////		
////		WorldItemState bookState		= new WorldItemState (CurrentBook.worlditem);
//////		CurrentBook.SaveToState (bookState);
////			
////		if (Kit.LocalState.BackerVersion)
////		{
////			Kit.submissionKickstarterEmail	= KickstarterEmail.text;
////		}
////		else
////		{
////			Kit.submissionKickstarterEmail	= Kit.LocalState.KickstarterEmail;
////		}
////		
////		if (SystemInfoCheckbox.isChecked)
////		{
////			FPS.RefreshSystemInfo ( );
////			
////			XmlSerializer xmlSerializerSystemInfo = new XmlSerializer (typeof (SystemInfoCollector.SystemInfoData));
////			StringWriter stringWriterSystemInfo = new StringWriter ( );
////			xmlSerializerSystemInfo.Serialize (stringWriterSystemInfo, FPS.Info);
////			
////			Kit.systemInfoSubmission = stringWriterSystemInfo.ToString ( );
////		}
////		else
////		{
////			Kit.systemInfoSubmission = string.Empty;
////		}
////		
////		Kit.docName 					= CurrentBook.CleanTitle + "_" + Kit.submissionKickstarterEmail + ".book";
////		
////		XmlSerializer xmlSerializer = new XmlSerializer (typeof (WorldItemState));
////		StringWriter stringWriter = new StringWriter ( );
////		xmlSerializer.Serialize (stringWriter, bookState);
////		
////		Kit.submission = stringWriter.ToString ( );
////		Kit.SaveToDisk = true;
////		
////		Kit.SubmitToServer ( );
//
////		ButtonToFreeze.SendMessage ("SetDisabled");
////		SubmitButton.SendMessage ("SetDisabled");
//	}
//
//	public void					EditPage (int pageNumber)
//	{
////		BookPreviewButton.SendMessage ("SetDisabled");
//		mCurrentPage					= pageNumber;
//		PageEditorTargetPosition 		= PageEditorVisiblePosition;
//		PageEditorPageNumLabel.text		= ("Editing Page " + (pageNumber + 1).ToString ( ));
//
//		if (	CurrentBook.Type == BookType.Book
//			||	CurrentBook.Type == BookType.Diary)
//		{
//			PageEditorNextPageButton.SetActive (true);
//			PageEditorPrevPageButton.SetActive (true);
//
//			if ((mCurrentPage - 1) >= 0)
//			{
//				PageEditorPrevPageButton.SendMessage ("SetEnabled");
//			}
//			else
//			{
//				PageEditorPrevPageButton.SendMessage ("SetDisabled");
//			}
//
//			if ((mCurrentPage + 1) < CurrentBook.Pages.Count)
//			{
//				PageEditorNextPageButton.SendMessage ("SetEnabled");
//			}
//			else
//			{
//				PageEditorNextPageButton.SendMessage ("SetDisabled");
//			}
//		}
//		else
//		{
//			PageEditorNextPageButton.SetActive (false);
//			PageEditorPrevPageButton.SetActive (false);
//		}
//		RefreshAll ( );
//		PageEditorInput.selected		= true;
//	}
//
//	public void					OnClickEditNextPage ( )
//	{
//		CurrentPageContents 		= PageEditorInput.text;
//		int nextPage = mCurrentPage + 1;
//		if (nextPage < CurrentBook.Pages.Count)
//		{
//			OnClickFinishedEditingPage ( );
//			EditPage (nextPage);
//		}
//	}
//
//	public void					OnClickEditPrevPage ( )
//	{
//		CurrentPageContents 		= PageEditorInput.text;
//		int prevPage = mCurrentPage - 1;
//		if (prevPage >= 0)
//		{
//			OnClickFinishedEditingPage ( );
//			EditPage (prevPage);
//		}
//	}
//
//	public void					OnClickFinishedEditingPage ( )
//	{
////		BookPreviewButton.SendMessage ("SetEnabled");
//		CurrentPageContents 				= PageEditorInput.text;
//		PageEditorTargetPosition 			= PageEditorHiddenPosition;
//		//LOAD current page into book
//		mCurrentPage						= -1;
//	}
//
//	public void					OnChangeSettings ( )
//	{
//		if (!mInitialized) { return; }
//
//		switch (SealedWithWaxPopup.selection)
//		{
//		case "None":
//			CurrentBook.SealStatus = BookSealStatus.None;
//			break;
//
//		case "Broken":
//			CurrentBook.SealStatus = BookSealStatus.Broken;
//			break;
//
//		case "Sealed":
//			CurrentBook.SealStatus = BookSealStatus.Sealed;
//			break;
//
//		default:
//			break;
//		}
//		
//		ArtifactAge lastAge = CurrentBookAvatar.worlditem.Get <Artifact> ( ).Age;
//		ArtifactAge newAge = lastAge;
//
//		if (AgeSlider.sliderValue <= 0.0f)
//		{
////			//Debug.Log ("Setting Recent");
//			newAge = ArtifactAge.Recent;
//		}
//		else if (AgeSlider.sliderValue <= 0.2f)
//		{
////			//Debug.Log ("Setting Modern");
//			newAge = ArtifactAge.Modern;
//		}
//		else if (AgeSlider.sliderValue <= 0.4f)
//		{
////			//Debug.Log ("Setting Old");
//			newAge = ArtifactAge.Old;
//		}
//		else if (AgeSlider.sliderValue <= 0.6f)
//		{
////			//Debug.Log ("Setting Antiquated");
//			newAge = ArtifactAge.Antiquated;
//		}
//		else if (AgeSlider.sliderValue <= 0.8f)
//		{
////			//Debug.Log ("Setting Ancient");
//			newAge = ArtifactAge.Ancient;
//		}
//		else
//		{
////			//Debug.Log ("Setting Prehistoric");
//			newAge = ArtifactAge.Prehistoric;
//		}
//		
//		if (newAge != lastAge)
//		{
//			CurrentBookAvatar.worlditem.Get <Artifact> ( ).Age = newAge;
//			CurrentBookAvatar.RefreshAppearance ( );
//		}
//	}
//	
//	public void					OnClickNextToPages ( )
//	{
//		PageBrowserGrid.collider.enabled = true;
//		OnClickNext ( );
//	}
//	
//	public void					OnClickPrevToPages ( )
//	{
//		PageBrowserGrid.collider.enabled = true;
//		OnClickPrev ( );
//	}
//	
//	public void					OnClickNextFromPages ( )
//	{
//		PageBrowserGrid.collider.enabled = false;
//		OnClickNext ( );
//	}
//	
//	public void					OnClickPrevFromPages ( )
//	{
//		PageBrowserGrid.collider.enabled = false;
//		OnClickPrev ( );
//	}
//
//	public void 				RefreshAll ( )
//	{
//		CurrentBookAvatar.RefreshAppearance ( );
//
//		mInitialized = false;
//		
//		if (CurrentBook.MultiPageType)
//		{
//			AddPageButton.SetActive (true);
//			if (CurrentBook.NumPages < 30)
//			{
//				AddPageButton.SendMessage ("SetEnabled");
//			}
//			else
//			{
//				AddPageButton.SendMessage ("SetDisabled");
//			}
//		}
//		else
//		{
//			if (!AddPageButton.activeSelf)
//			{
//				AddPageButton.SetActive (false);
//			}
//			if (CurrentBook.Pages.Count > 1)
//			{
//				List <Page> pages = new List <Page> ( );
//				pages.Add (CurrentBook.Pages [0]);
//				CurrentBook.Pages = pages;
//			}
//		}
//		
//		UISlicedSprite titleBorder = TitleInput.transform.FindChild ("Border").GetComponent <UISlicedSprite> ( );
//		UISlicedSprite authorsBorder = AuthorsInput.transform.FindChild ("Border").GetComponent <UISlicedSprite> ( );
//		UISlicedSprite summaryBorder = SummaryInput.transform.FindChild ("Border").GetComponent <UISlicedSprite> ( );
//		
//		
//		switch (CurrentBook.Type)
//		{
//		case BookType.Book:
//			TitleInput.collider.enabled = true;
//			AuthorsInput.collider.enabled = true;
//			SummaryInput.collider.enabled = true;
//			
//			titleBorder.color	= Colors.Get.MenuButtonBackgroundColorDefault;
//			authorsBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			summaryBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			break;
//			
//		case BookType.Diary:
//			TitleInput.collider.enabled = false;
//			AuthorsInput.collider.enabled = true;
//			SummaryInput.collider.enabled = false;
//			
//			TitleInput.text 	= string.Empty;
//			SummaryInput.text 	= string.Empty;
//			titleBorder.color	= Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			summaryBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			summaryBorder.color = Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			break;
//			
//		case BookType.Parchment:
//			TitleInput.collider.enabled = true;
//			AuthorsInput.collider.enabled = true;
//			SummaryInput.collider.enabled = false;
//			
//			SummaryInput.text = string.Empty;
//			
//			titleBorder.color	= Colors.Get.MenuButtonBackgroundColorDefault;
//			authorsBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			summaryBorder.color = Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			break;
//			
//		case BookType.Scroll:
//			TitleInput.collider.enabled = true;
//			AuthorsInput.collider.enabled = true;
//			SummaryInput.collider.enabled = true;
//			
//			titleBorder.color	= Colors.Get.MenuButtonBackgroundColorDefault;
//			authorsBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			summaryBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			break;
//			
//		case BookType.Scrap:
//			TitleInput.collider.enabled = false;
//			AuthorsInput.collider.enabled = false;
//			SummaryInput.collider.enabled = false;
//			
//			TitleInput.text = string.Empty;
//			AuthorsInput.text = string.Empty;
//			SummaryInput.text = string.Empty;
//			
//			titleBorder.color	= Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			authorsBorder.color = Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			summaryBorder.color = Colors.Darken (Colors.Get.MenuButtonBackgroundColorDefault);
//			break;
//			
//		case BookType.Scripture:
//			TitleInput.collider.enabled = true;
//			AuthorsInput.collider.enabled = true;
//			SummaryInput.collider.enabled = true;
//			
//			titleBorder.color	= Colors.Get.MenuButtonBackgroundColorDefault;
//			authorsBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			summaryBorder.color = Colors.Get.MenuButtonBackgroundColorDefault;
//			break;
//			
//		default:
//			break;
//		}
//		
//
//		int numberOfPrototypes			= Books.Get.PrototypesByType (CurrentBook.Type).Count;
//		float sliderValue				= ((float) CurrentBook.PrototypeIndex) / numberOfPrototypes;
//		if (BookStyleSlider.numberOfSteps != numberOfPrototypes)
//		{
//			BookStyleSlider.numberOfSteps	= numberOfPrototypes;
//		}
//
//		if ( 	CurrentBook.Type == BookType.Scroll)
//		{
//			SealedWithWaxPopup.gameObject.SendMessage ("SetEnabled");
//			switch (CurrentBook.SealStatus)
//			{
//			case BookSealStatus.None:
//				SealedWithWaxPopup.selection = "None";
//				break;
//
//			case BookSealStatus.Broken:
//				SealedWithWaxPopup.selection = "Broken";
//				break;
//
//			case BookSealStatus.Sealed:
//				SealedWithWaxPopup.selection = "Sealed";
//				break;
//
//			default:
//				break;
//			}
//		}
//		else
//		{
//			SealedWithWaxPopup.gameObject.SendMessage ("SetDisabled");
//			SealedWithWaxPopup.selection = "None";
//		}
//
//		ArtifactAge age = CurrentBookAvatar.worlditem.Get <Artifact> ( ).Age;
//		switch (age)
//		{
//		case ArtifactAge.Recent:
//			AgeSlider.sliderValue = 0.0f;
//			break;
//
//		case ArtifactAge.Modern:
//			AgeSlider.sliderValue = 0.2f;
//			break;
//
//		case ArtifactAge.Old:
//			AgeSlider.sliderValue = 0.4f;
//			break;
//
//		case ArtifactAge.Antiquated:
//			AgeSlider.sliderValue = 0.6f;
//			break;
//
//		case ArtifactAge.Ancient:
//			AgeSlider.sliderValue = 0.8f;
//			break;
//
//		case ArtifactAge.Prehistoric:
//			AgeSlider.sliderValue = 1.0f;
//			break;
//
//		default:
//			break;
//		}
//
//		switch (CurrentBook.Type)
//		{
//		case BookType.Book:
//			Type.selection = "Book";
//			break;
//		case BookType.Diary:
//			Type.selection = "Diary";
//			break;
//		case BookType.Scripture:
//			Type.selection = "Scripture";
//			break;
//		case BookType.Scroll:
//			Type.selection = "Scroll";
//			break;
//		case BookType.Parchment:
//			Type.selection = "Parchment";
//			break;
//		case BookType.Scrap:
//			Type.selection = "Scrap";
//			break;
//		}
//
//		PageEditorFontInkPopupList.items.Clear ( );
//		foreach (BookInk ink in Books.Get.Inks)
//		{
//			PageEditorFontInkPopupList.items.Add (ink.InkName);
//		}
//		PageEditorFontInkPopupList.items.Sort ( );
//		PageEditorFontInkPopupList.items.Insert (0, "Default");
//
//		PageEditorFontPopupList.items.Clear ( );
//		foreach (BookFont font in Books.Get.Fonts)
//		{
//			PageEditorFontPopupList.items.Add (font.FontName);
//		}
//		PageEditorFontPopupList.items.Sort ( );
//		PageEditorFontPopupList.items.Insert (0, "Default");
//
//		int numberOfExistingPages = PageBrowserGrid.transform.childCount;
//		if (numberOfExistingPages < (CurrentBook.NumPages))
//		{
//			for (int i = 0; i < CurrentBook.NumPages - numberOfExistingPages; i++)
//			{
////				//Debug.Log ("Creating a new page browser object for page " + i);
//				GameObject pageBrowserGameObject 				= GameObject.Instantiate (PageBrowserObjectPrototype, Vector3.zero, Quaternion.identity) as GameObject;
//				pageBrowserGameObject.transform.parent 			= PageBrowserGrid.transform;
//				pageBrowserGameObject.transform.localScale		= Vector3.one;
//				pageBrowserGameObject.transform.localPosition	= new Vector3 (0f, 0f, -75f);
//			}
//		}
//
//		RefreshFont ( );
//
//		int currentPageCount = 0;
//		foreach (Transform pageBrowserTransform in PageBrowserGrid.transform)
//		{
////			//Debug.Log ("Setting page browser object for page " + currentPageCount);
//			GUIPageBrowserObject pageBrowserObject = pageBrowserTransform.GetComponent <GUIPageBrowserObject> ( );
//			pageBrowserObject.Refresh (this, currentPageCount);
//
//			currentPageCount++;
//			if (currentPageCount >= CurrentBook.NumPages)
//			{
//				currentPageCount = -1;
//			}
//		}
//
//		TitleInput.text 	= CurrentBook.Title;
//		AuthorsInput.text 	= string.Join (",", CurrentBook.Authors.ToArray ( ));
//		SummaryInput.text	= CurrentBook.ContentsSummary;
//
//		PageBrowserGrid.Reposition ( );
//
//		mInitialized = true;
//	}
//
//	public void					OnFontInkSelectionChange ( )
//	{
//		PageEditorInput.selected	= false;
//		CurrentPageContents 		= PageEditorInput.text;
//		RefreshFont ( );
//		mSelectPageInput			= true;
//	}
//
//	public void					OnFontSelectionChange ( )
//	{
//		PageEditorInput.selected	= false;
//		CurrentPageContents 		= PageEditorInput.text;
//		RefreshFont ( );
//		mSelectPageInput			= true;
//	}
//
//	public void					OnClickPreviewFont ( )
//	{
//		PageEditorInput.selected	= false;
//		CurrentPageContents 		= PageEditorInput.text;
//		RefreshAll ( );
//		mSelectPageInput			= true;
//	}
//
//	public void					RefreshFont ( )
//	{
////		//Debug.Log ("Changing font");
//		if (mCurrentPage >= 0)
//		{
//			string fontName = PageEditorFontPopupList.selection;
//			if (fontName == "Default")
//			{
//				fontName = string.Empty;
//			}
//			CurrentBook.Pages [mCurrentPage].FontName = fontName;
//
//			string fontInk = PageEditorFontInkPopupList.selection;
//			if (fontInk == "Default")
//			{
//				fontInk = string.Empty;
//			}
//			CurrentBook.Pages [mCurrentPage].InkName = fontInk;
//			
//			if (CurrentBook.Pages [mCurrentPage].FontName == string.Empty)
//			{
//				PageEditorFontPopupList.selection = "Default";
//			}
//			else
//			{
//				PageEditorFontPopupList.selection = CurrentBook.Pages [mCurrentPage].FontName;
//			}
//
//			PageEditorInput.text 		= CurrentPageContents;
//			BookFont font				= Books.Get.GetFont (CurrentBook.Pages [mCurrentPage].FontName);
//			PageEditorInput.caratChar	= font.CaratChar;
//			PageEditorInput.maxChars	= font.MaxCharsPerPage;
//
//			if (PageEditorDisplayFontCheckbox.isChecked)
//			{
////				//Debug.Log ("font name: " + font.FontName);
//				PageEditorInput.label.font 	= font.NGUIFont;
//				PageEditorInput.label.MakePixelPerfect ( );
//				PageEditorInput.label.transform.localScale = Vector3.one * font.DisplayScale * 0.9f;
//			}
//			else
//			{
//				PageEditorInput.label.font = PageEditorFont;
//				PageEditorInput.label.MakePixelPerfect ( );
//			}
//		}
//	}
//
//	public void					OnClickAddPage ( )
//	{
//		CurrentBook.Pages.Add (new Page ( ));
//
//		RefreshAll ( );
//	}
//
//	public void					OnChangeAuthorsAndTitle ( )
//	{
//		if (!mInitialized) { return; }
//
////		//Debug.Log ("Changin authors and title");
//
//		CurrentBook.Title 				= TitleInput.text;
//		CurrentBook.ContentsSummary 	= SummaryInput.text;
//		string [] separators			= new string [1];
//		separators [0]					= ",";
//		string [] authors 				= AuthorsInput.text.Split (separators, StringSplitOptions.RemoveEmptyEntries);
//		CurrentBook.Authors.Clear ( );
//		CurrentBook.Authors.AddRange (authors);
//
//		RefreshAll ( );
//	}
//
//	public void					OnClickPreviewBook ( )
//	{
//		HelpWindowObject.SendMessage ("Close");
//		PageEditorInput.selected 	= false;
//		CurrentPageContents 		= PageEditorInput.text;
//		mPreviewing 				= true;
//		BookReader.SendMessage ("Show");
//	}
//
//	public void					OnFinishedReading ( )
//	{
//		mPreviewing = false;
//	}
//
//	public void					OnChangetBookType ( )
//	{
//		if (!mInitialized) { return; }
//
//		switch (Type.selection)
//		{
//		case "Book":
//			CurrentBook.Type = BookType.Book;
//			break;
//		case "Diary":
//			CurrentBook.Type = BookType.Diary;
//			break;
//		case "Scripture":
//			CurrentBook.Type = BookType.Scripture;
//			break;
//		case "Scroll":
//			CurrentBook.Type = BookType.Scroll;
//			break;
//		case "Parchment":
//			CurrentBook.Type = BookType.Parchment;
//			break;
//		case "Scrap":
//			CurrentBook.Type = BookType.Scrap;
//			break;
//		default:
//			break;
//		}
//
//		RefreshAll ( );
//	}
//
//	public void					OnBookStyleChange ( )
//	{
//		if (!mInitialized) { return; }
//
//		int previousPrototype 	= CurrentBook.PrototypeIndex;
//		int numberOfPrototypes 	= Books.Get.PrototypesByType (CurrentBook.Type).Count;
//		int newPrototype		= (int) (BookStyleSlider.sliderValue * numberOfPrototypes);
//
//		if (previousPrototype != newPrototype)
//		{
////			//Debug.Log ("Previous prototype: " + previousPrototype.ToString ( ));
////			//Debug.Log ("New prototype: " + newPrototype.ToString ( ) + "\n");
//			CurrentBook.PrototypeIndex = newPrototype;
//			RefreshAll ( );
//		}
//	}
//
//	public void					OnClickNext ( )
//	{
//		ConfirmWindowTargetPosition = ConfirmWindowHiddenPosition;
//		HelpWindowObject.SendMessage ("Close");
//		if (Kit.IsWorking)
//		{
//			return;
//		}
//		mInitialized				= true;
//		BookPreviewButton.SendMessage ("SetEnabled");
//		OnChangeAuthorsAndTitle ( );
//		RefreshAll ( );
//		MoveNext ( );
//	}
//
//	public void					OnClickPrev ( )
//	{
//		ConfirmWindowTargetPosition = ConfirmWindowHiddenPosition;
//		HelpWindowObject.SendMessage ("Close");
//		if (Kit.IsWorking)
//		{
//			return;
//		}
//		OnChangeAuthorsAndTitle ( );
//		RefreshAll ( );
//		MovePrev ( );
//	}
//
//	public void 				MoveNext ( )
//	{
//		PositionTarget += PositionOffset;
//	}
//
//	public void					MovePrev ( )
//	{
//		PositionTarget -= PositionOffset;
//	}
//
//	protected bool				mPreviewing				= false;
//	protected bool				mInitialized 			= false;
//	protected int				mCurrentPage 			= -1;
//	protected bool				mSelectPageInput		= false;
//}

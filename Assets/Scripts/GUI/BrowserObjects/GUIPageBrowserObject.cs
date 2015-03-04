//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Frontiers.World;
//
//namespace Frontiers
//{
//	public class GUIPageBrowserObject : MonoBehaviour {
//	
//		public GameObject 		DeletePageButton;
//		public GameObject		EditPageButton;
//		public int				PageNumber			= 0;
//		public UILabel			PageNumberLabel;
//		public GUIBookEditor	ParentEditor;
//		
//		public void			Start ( )
//		{
//			
//		}
//		
//		public void			Refresh (GUIBookEditor editor, int pageNumber)
//		{
//			if (pageNumber == -1)
//			{
//				GameObject.Destroy (gameObject);
//				return;
//			}
//			
//			ParentEditor 	= editor;			
//			PageNumber 		= pageNumber;			
//			name			= "P_" + pageNumber.ToString ( );
//			
//			PageNumberLabel.text = "Page " + (PageNumber + 1).ToString ( );
//			
//			if (pageNumber == 0)
//			{
//				if (DeletePageButton.activeSelf)
//				{
//					DeletePageButton.SetActive (false);
//				}
//			}
//			else
//			{
//				DeletePageButton.SetActive (true);
//				DeletePageButton.SendMessage ("SetEnabled");
//			}
//			
//			if (editor.CurrentBook.MultiPageType)
//			{
//				EditPageButton.SendMessage ("SetEnabled");
//			}
//			else
//			{
//				//disable since we're bigger than we need to be
//				if (pageNumber > 0)
//				{
//					PageNumberLabel.text = ("(Disabled since it's a " + editor.CurrentBook.Type.ToString ( ) + ")");
//					DeletePageButton.SendMessage ("SetEnabled");
//					EditPageButton.SendMessage ("SetDisabled");
//				}
//				else
//				{
//					EditPageButton.SendMessage ("SetEnabled");
//				}
//			}
//		}
//		
//		public void			OnClickEditPage ( )
//		{
//			ParentEditor.EditPage (PageNumber);
//		}
//		
//		public void			OnClickDeletePage ( )
//		{
////			ParentEditor.CurrentBook.Pages.RemoveAt (PageNumber);
//			ParentEditor.RefreshAll ( );
//		}
//	}
//}
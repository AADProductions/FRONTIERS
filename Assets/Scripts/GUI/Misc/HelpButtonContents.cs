using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class HelpButtonContents : MonoBehaviour
	{
		public string 		Subject;	
		public string 		Contents;
		public GameObject	HelpWindow;
		
		public void			OnClickHelp ( )
		{
	//		//Debug.Log ("Clicking help");
			HelpWindow.SendMessage ("OnClickHelp", gameObject);
		}
	}
}
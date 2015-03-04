using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class HelpWindow : MonoBehaviour
	{
		public UILabel SubjectLabel;
		public UILabel ContentsLabel;
		public UIAnchor Anchor;
		public Vector2 TargetPositon;
		public Vector2 VisiblePosition;
		public Vector2 HiddenPosition;
		
		public void OnClickHelp (GameObject sender)
		{
	//		//Debug.Log ("Getting clicked by " + sender.name);
			HelpButtonContents contents = sender.GetComponent <HelpButtonContents> ( );
			SubjectLabel.text 			= contents.Subject;
			ContentsLabel.text			= contents.Contents;
			TargetPositon				= VisiblePosition;
		}
		
		public void Close ( )
		{
			TargetPositon = HiddenPosition;
		}
		
		public void OnClickClose ( )
		{
			Close ( );
		}
		
		public void Update ( )
		{
			Anchor.relativeOffset = Vector2.Lerp (Anchor.relativeOffset, TargetPositon, 10.0f * (float) Frontiers.WorldClock.ARTDeltaTime);
		}
	}
}
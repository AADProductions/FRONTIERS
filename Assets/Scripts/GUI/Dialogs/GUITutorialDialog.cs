using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI {
	public class GUITutorialDialog : GUIEditor <TutorialDialogResult>
	{	
		public UILabel			TutorialSubject;
		public UILabel 			TutorialMessage;
		public Material			TutorialImageMaterial;
		
		public override void PushEditObjectToNGUIObject ( )
		{
			TutorialSubject.text 	= EditObject.TutorialSubject;
			TutorialMessage.text 	= EditObject.TutorialMessage;
			TutorialImageMaterial.SetTexture ("_MainTex", EditObject.TutorialImage);
			
		}
		
	//	public void Update ( )
	//	{
	//		if (Input.GetKeyDown (KeyCode.Space))
	//		{
	//			OnCloseTutorial ( );
	//		}
	//	}
		
		public virtual void OnCloseTutorial ( )
		{
			EditObject.Result = "OK";
			
			OnFinish ( );
		}
	}

	public class TutorialDialogResult
	{
		public string 		TutorialSubject;
		public string 		TutorialMessage;
		public Texture2D 	TutorialImage;
		public string 		Result;
	}
}
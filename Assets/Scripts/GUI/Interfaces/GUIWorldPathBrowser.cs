using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI {
	public class GUIWorldPathBrowser : GUIBrowserSelectView <Path>
	{
		public override void ReceiveFromParentEditor (IEnumerable <Path> editObject, ChildEditorCallback <IEnumerable <Path>> callBack)
		{
			base.ReceiveFromParentEditor (editObject, callBack);
		}
		
		protected override GameObject 	ConvertEditObjectToBrowserObject (Path editObject)
		{
			GameObject newBrowserGameObject = base.ConvertEditObjectToBrowserObject (editObject);
			
			return newBrowserGameObject;
			
	//		GUIWorldPathBrowserObject newBrowserObject 	= newBrowserGameObject.GetComponent <GUIWorldPathBrowserObject> ( );
	//		newBrowserObject.StartLocationLabel.text 	= editObject.StartPathMarker.StackName;
	//		newBrowserObject.EndLocationLabel.text		= editObject.EndPathMarker.StackName;
	//		newBrowserObject.DistanceLabel.text			= (editObject.LengthInMeters / 100.0f).ToString ("F1");
	//		newBrowserObject.DifficultyLabel.text		= editObject.Difficulty.ToString ( );
	//		newBrowserObject.ColorBackground.color		= Colors.GetColorFromWorldPathDifficulty (editObject.Difficulty);
	//		newBrowserObject.ButtonMessage.target		= gameObject;
	//		newBrowserObject.ButtonMessage.functionName	= "OnClickBrowserObject";
	//		
	//		if (editObject.IsActivePath)
	//		{
	//			newBrowserObject.ActiveGlow.alpha 	= 1.0f;
	//			newBrowserObject.ColorOverlay.color	= Color.white;
	//		}
	//		else
	//		{
	//			newBrowserObject.ActiveGlow.alpha 	= 0.0f;
	//			newBrowserObject.ColorOverlay.color	= Color.grey;
	//		}
	//		
	//		return newBrowserGameObject;
		}

	//	protected override void 		RefreshEditObjectToBrowserObject (Path editObject, GameObject browserObject)
	//	{		
	//		GUIWorldPathBrowserObject newBrowserObject 	= browserObject.GetComponent <GUIWorldPathBrowserObject> ( );
	//		newBrowserObject.StartLocationLabel.text 	= editObject.StartLocation.StackName;
	//		newBrowserObject.EndLocationLabel.text		= editObject.EndLocation.StackName;
	//		newBrowserObject.DistanceLabel.text			= (editObject.LengthInMeters / 100.0f).ToString ("F1");
	//		newBrowserObject.DifficultyLabel.text		= editObject.State.Difficulty.ToString ( );
	//		newBrowserObject.ColorBackground.color		= Colors.GetColorFromWorldPathDifficulty (editObject.State.Difficulty);
	//		newBrowserObject.ButtonMessage.target		= gameObject;
	//		newBrowserObject.ButtonMessage.functionName	= "OnClickBrowserObject";
	//		
	//		if (editObject.IsActivePath)
	//		{
	//			newBrowserObject.ActiveGlow.alpha 	= 1.0f;
	//			newBrowserObject.ColorOverlay.color	= Color.white;
	//		}
	//		else
	//		{
	//			newBrowserObject.ActiveGlow.alpha 	= 0.0f;
	//			newBrowserObject.ColorOverlay.color	= Color.grey;
	//		}
	//	}
		
		public override void 			OnClickBrowserObject (GameObject obj)
		{		
			base.OnClickBrowserObject (obj.transform.parent.gameObject);		
			RefreshBrowserObjects ( );
		}
			
		public override void 			PushSelectedObjectToViewer ( )
		{

		}
	}
}

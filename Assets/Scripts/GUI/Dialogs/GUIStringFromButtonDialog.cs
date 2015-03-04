using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class GUIStringFromButtonDialog : GUIEditor<StringDialogResult>
	{
		public override void PushEditObjectToNGUIObject ( )
		{
			return;
		}
		
		public void OnClickButton (GameObject button)
		{
			EditObject.Result = button.name;
			OnFinish ( );
		}
	}
}
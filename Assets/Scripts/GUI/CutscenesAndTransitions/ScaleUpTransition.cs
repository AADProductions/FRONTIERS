using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class ScaleUpTransition : EditorTransition
	{
		public override void Awake ( )
		{
			//OnProceedMessage = "DisableInput";
			//OnFinishedMessage = "EnableInput";
			mCurrent = transform.localScale * 0.10f;
			mGoal = Vector3.one;
			
			transform.localScale = mCurrent;
			
			NGUITools.SetActive (gameObject, true);
			
			base.Awake ( );
		}
	}
}
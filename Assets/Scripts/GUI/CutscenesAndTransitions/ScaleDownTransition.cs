using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
		public class ScaleDownTransition : EditorTransition
		{
				public override void Awake()
				{
						OnFinishedMessage = string.Empty;
						OnProceedMessage = "DisableInput";
						mCurrent = transform.localScale;
						mGoal = transform.localScale * 0.01f;
						base.Awake();
				}

				protected override void OnFinish()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumHidden);
				}

				public override void Retire()
				{
						GUIManager.RetireGUIChildEditor(gameObject);
				}
		}
}
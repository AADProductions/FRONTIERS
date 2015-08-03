using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.World.WIScripts
{
	public class ObexControlPanel : WIScript
	{
		public ObexControlPanelState State = new ObexControlPanelState ();

		public override void OnInitialized ()
		{
			worlditem.OnPlayerUse += OnPlayerUse;
		}

		public void OnPlayerUse () {
			TowerElevator te = FindObjectOfType <TowerElevator> ();
			if (te != null) {
				te.SendToNextPosition ();
			}
		}

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{

		}
		#endif
	}

	[Serializable]
	public class ObexControlPanelState
	{
		public string TargetElevator = string.Empty;
	}
}
using UnityEngine;
using System.Collections;
using Frontiers.GUI;
using Frontiers.World;
using Frontiers;

public class GUIStackDisplay : GUIObject
{
		public UIPanel MainPanel;
		public InventorySquare StackDisplay;

		public void Show()
		{
				MainPanel.enabled = true;
				StackDisplay.enabled = true;
		}

		public void Hide()
		{
				MainPanel.enabled = false;
				StackDisplay.enabled = false;
		}

		public void EnableColliders(bool enable)
		{
				StackDisplay.GetComponent<Collider>().enabled = enable;
		}

		protected WIStackEnabler mEnabler = null;
}

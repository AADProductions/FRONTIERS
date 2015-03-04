using UnityEngine;
using System.Collections;

namespace Frontiers.GUI
{
	public class GUIBrowserDivider : MonoBehaviour
	{
		public Color DividerColor = Color.white;
		public UILabel Label;
		public UISprite SpikeLeft;
		public UISprite SpikeRight;
		public float DividerHeight = 45.0f;
		public float BrowserWidth = 300.0f;

		public void Awake ()
		{
			name = "DIVIDER";
		}

		public void Refresh ()
		{
			Label.color = DividerColor;
			SpikeLeft.color = DividerColor;
			SpikeRight.color = DividerColor;		
		}
	}
}
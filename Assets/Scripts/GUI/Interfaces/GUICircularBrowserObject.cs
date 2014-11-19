using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI {
	//the base class for all inventory squares (ironically)
	//95% of all the components we need are stored here
	public class GUICircularBrowserObject : GUIObject
	{
		public UIPanel Panel;
		public UILabel InventoryItemName;
		public UILabel StackNumberLabel;
		public UILabel WeightLabel;
		public UISprite ActiveHighlight;
		public UISprite Shadow;
		public UISprite Background;
		public UIButton Button;
		public UIButtonScale ButtonScale;
		public UIButtonMessage Message;
		public Vector2 Dimensions = new Vector2 (100.0f, 100.0f);
		public int Index = -1;
		public GameObject Doppleganger;
		public MasterAudio.SoundType SoundType = MasterAudio.SoundType.PlayerInterface;
		public string SoundNameSuccess = "InventoryClick";
		public string SoundNameFailure = "ButtonClickDisabled";

		public virtual void OnEnable ( )
		{
			Panel.enabled = true;
			RefreshRequest ();
		}

		public virtual void OnDisable ( )
		{
			if (Doppleganger != null) {
				GameObject.Destroy (Doppleganger);
			}
			Panel.enabled = false;
		}

		public override void Awake ()
		{
			Panel = gameObject.GetComponent <UIPanel> ();

			if (ActiveHighlight != null)
				ActiveHighlight.color = Colors.Alpha (ActiveHighlight.color, 0f);

			if (Button != null)
				Button.defaultColor = Colors.Alpha (Button.hover, 0f);

			if (ButtonScale == null) {
				ButtonScale = gameObject.GetComponent <UIButtonScale> ();
			}

			base.Awake ();
		}

		public GUICircleBrowser ParentBrowser;
	}
}
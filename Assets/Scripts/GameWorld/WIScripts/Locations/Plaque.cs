using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World {
	public class Plaque : WIScript {
		public UILabel TitleLabel;

		public override void OnInitialized ()
		{
			TitleLabel.text = worlditem.DisplayName;
		}
	}
}
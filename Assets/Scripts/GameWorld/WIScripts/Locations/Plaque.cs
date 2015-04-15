using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.WIScripts
{
	public class Plaque : WIScript {
		public UILabel TitleLabel;

		public override void OnInitialized ()
		{
			TitleLabel.text = worlditem.DisplayName;
		}
	}
}
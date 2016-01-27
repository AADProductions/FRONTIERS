using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class GUIDivider : GUIObject
	{
		public UILabel 		Label;
		public UISprite		LeftSpike;
		public UISprite		RightSpike;
		
		public override void Initialize (string argument)
		{
			base.Initialize (argument);
			
			Label.text = argument;
			
		}
	}
}
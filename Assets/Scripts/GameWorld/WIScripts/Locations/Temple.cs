using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Temple : WIScript
		{
				public TempleState State = new TempleState();

				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Location location = gameObject.GetComponent <Location>();
						location.State.Type = "Temple";		
				}
				#endif
		}

		[Serializable]
		public class TempleState
		{
				[FrontiersBitMaskAttribute("Alignment")]
				public int Alignment = 0;
				public STransform BannerOffset = new STransform();
		}
}

using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts
{
	public class SecretEntranceStatue : WIScript {
		public Animation CacheOpenAnimation;
		public string CacheOpenClipName;
		public string CacheCloseClipName;
		public string SecretEntranceOpenClipName;

		public override void OnInitialized ()
		{
			Receptacle recepticle = null;
			if (worlditem.Is <Receptacle> (out recepticle)) {
				recepticle.OnItemPlacedInReceptacle += OnItemPlacedInReceptacle;
				recepticle.OnItemRemovedFromReceptacle += OnItemRemovedFromReceptacle;
			}
		}

		public void OnItemPlacedInReceptacle ( )
		{
			CacheOpenAnimation.Play (CacheOpenClipName);
		}

		public void OnItemRemovedFromReceptacle ()
		{
			CacheOpenAnimation.Play (CacheCloseClipName);
		}
	}
}

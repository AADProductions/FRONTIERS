using UnityEngine;
using System.Collections;

namespace Frontiers.GUI {
	public class GUIInterfaceTabDisplay : MonoBehaviour
	{
	//	public InterfaceType				Interface;
		public UILabel 						InterfaceNameLabel;
		public UISlicedSprite 				BackgroundActiveSelectedSprite;
		public UISlicedSprite 				BackgroundActiveUnselectedSprite;
		public UISlicedSprite 				BackgroundInactiveSprite;
		public UILabel						InterfaceToggleKeyLabel;
		public UISlicedSprite				BackgroundGetAttentionSprite;
		
		public void							Update ( )
		{
			if (BackgroundGetAttentionSprite.enabled)
			{
				BackgroundGetAttentionSprite.alpha = (Mathf.Sin (Time.time * 4.0f) / 2.0f) + 0.5f;
			}
		}
	}
}
using UnityEngine;
using System.Collections;

namespace Frontiers.World.WIScripts {
	public class FamilyPortrait : WIScript {

		public Texture2D PortraitA;
		public Texture2D PortraitB;
		public Texture2D PortraitC;
		public Texture2D PortraitD;
		public Material PortraitMaterial;

		public override void OnInitialized ()
		{
			Texture2D tex;

			switch (Profile.Get.CurrentGame.Character.Ethnicity) {
			case CharacterEthnicity.BlackCarribean:
			default:
				tex = PortraitB;
				break;

			case CharacterEthnicity.Caucasian:
				tex = PortraitA;
				break;

			case CharacterEthnicity.EastIndian:
				tex = PortraitC;
				break;

			case CharacterEthnicity.HanChinese:
				tex = PortraitD;
				break;
			}

			PortraitMaterial.mainTexture = tex;
		}
	}
}

using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World
{
	public class CoatOfArms : WIScript
	{
		public CoatOfArmsState State = new CoatOfArmsState ();

		public Sigil Props {
			get {
				if (mProps == null) {
					LoadProps ();
				}
				return mProps;
			}
		}

		public List <Renderer> BannerItemObjects;
		public Renderer Banner4Symbol;
		public Renderer Banner2Symbol;
		public List <Renderer> Banner4SymbolObjects;
		public List <Renderer> Banner2SymbolObjects;
		public Renderer BannerObject;
		public SigilType Type = SigilType.BannerFourItems;
		public Material RoyaltyMaterial;
		public Material CommonMaterial;

		public void LoadProps ()
		{
			if (string.IsNullOrEmpty (State.SigilName)) {
				State.SigilName = "Benneton";
			}
			if (Mods.Get.Runtime.LoadMod <Sigil> (ref mProps, "Sigil", State.SigilName)) {
				RefreshBanner ();
			}
		}

		public void RefreshBanner ()
		{
			Banner2Symbol.enabled = false;
			Banner4Symbol.enabled = false;
			for (int i = 0; i < Banner2SymbolObjects.Count; i++) {
				Banner2SymbolObjects [i].enabled = false;
			}
			for (int i = 0; i < Banner4SymbolObjects.Count; i++) {
				Banner4SymbolObjects [i].enabled = false;
			}

			switch (Props.Style) {
			case Style.FourColorFourItems:
			case Style.OneColorFourItems:
			case Style.TwoColorFourItems:
				Banner4Symbol.enabled = true;
				BannerItemObjects.Clear ();
				BannerItemObjects.AddRange (Banner4SymbolObjects);
				BannerObject = Banner4Symbol;
				Type = SigilType.BannerFourItems;
				break;

			default:
				Banner2Symbol.enabled = true;
				BannerItemObjects.Clear ();
				BannerItemObjects.AddRange (Banner2SymbolObjects);
				BannerObject = Banner2Symbol;
				Type = SigilType.BannerTwoItems;
				break;
			}

			//make sure our indexes are in range
			Props.BannerColorIndex = Mathf.Clamp (Props.BannerColorIndex, 0, Colors.BannerColors.BannerColors.Count - 1);
			for (int colorIndex = 0; colorIndex < Props.BannerItemColorIndex.Length; colorIndex++) {
				Props.BannerItemColorIndex [colorIndex]	= Mathf.Clamp (Props.BannerItemColorIndex [colorIndex], 0, Colors.BannerColors.ObjectColors.Count - 1);
			}
			for (int iconIndex = 0; iconIndex < Props.BannerItemIconIndex.Length; iconIndex++) {
				Props.BannerItemIconIndex [iconIndex]	= Mathf.Clamp (Props.BannerItemIconIndex [iconIndex], 0, Colors.BannerColors.ObjectIcons.Count - 1);
			}
	
			//set background banner color & material
			if (Props.Royalty) {
				BannerObject.material = RoyaltyMaterial;
			} else {
				BannerObject.material = CommonMaterial;
			}
			BannerObject.material.SetColor ("_Color", Colors.BannerColors.BannerColors [Props.BannerColorIndex]);
			
			//set item colors and icons
			int colorIndexOneTwoThreeFour = 0;
			int colorIndexTwoFour = 0;
			int colorIndexOneThree = 0;
				
			switch (Props.Style) {
			case Style.OneColorFourItems:
				colorIndexOneTwoThreeFour = Props.BannerItemColorIndex [0];
				if (Type == SigilType.BannerTwoItems) {
					UpdateBannerItem (0, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [1], true);				
				} else {
					UpdateBannerItem (0, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [1], true);
					UpdateBannerItem (2, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [2], true);
					UpdateBannerItem (3, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [3], true);
				}
				break;
	
			case Style.FourColorFourItems:
				if (Type == SigilType.BannerTwoItems) {
					UpdateBannerItem (0, Props.BannerItemColorIndex [0], Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, Props.BannerItemColorIndex [1], Props.BannerItemIconIndex [1], true);				
				} else {
					UpdateBannerItem (0, Props.BannerItemColorIndex [0], Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, Props.BannerItemColorIndex [1], Props.BannerItemIconIndex [1], true);
					UpdateBannerItem (2, Props.BannerItemColorIndex [2], Props.BannerItemIconIndex [2], true);
					UpdateBannerItem (3, Props.BannerItemColorIndex [3], Props.BannerItemIconIndex [3], true);
				}
				break;
	
			case Style.OneColorTwoItems:
				colorIndexTwoFour = Props.BannerItemColorIndex [0];
				if (Type == SigilType.BannerTwoItems) {
					UpdateBannerItem (0, colorIndexTwoFour, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
				} else {
					UpdateBannerItem (0, colorIndexTwoFour, Props.BannerItemIconIndex [0], false);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
					UpdateBannerItem (2, colorIndexTwoFour, Props.BannerItemIconIndex [2], false);
					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
				}
				break;
	
			case Style.TwoColorFourItems:
				colorIndexTwoFour = Props.BannerItemColorIndex [0];
				colorIndexOneThree	= Props.BannerItemColorIndex [1];
				if (Type == SigilType.BannerTwoItems) {
					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
				} else {
					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
					UpdateBannerItem (2, colorIndexOneThree, Props.BannerItemIconIndex [2], true);
					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
				}
				break;
	
			case Style.TwoColorTwoItems:
				colorIndexTwoFour = Props.BannerItemColorIndex [0];
				colorIndexOneThree	= Props.BannerItemColorIndex [1];
				if (Type == SigilType.BannerTwoItems) {
					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
				} else {
					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], false);
					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
					UpdateBannerItem (2, colorIndexOneThree, Props.BannerItemIconIndex [2], false);
					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
				}
				break;
	
			default:
				break;
			}
		}

		public void UpdateBannerItem (int bannerItemIndex, int colorIndex, int iconIndex, bool visible)
		{
			if (bannerItemIndex >= BannerItemObjects.Count) {
				return;
			}
	
			if (!visible) {
				BannerItemObjects [bannerItemIndex].enabled = false;
			} else {
				BannerItemObjects [bannerItemIndex].enabled = true;
				BannerItemObjects [bannerItemIndex].material.SetColor ("_Color", Colors.BannerColors.ObjectColors [colorIndex]);
				BannerItemObjects [bannerItemIndex].material.SetTexture ("_AlphaMap", Colors.BannerColors.ObjectIcons [iconIndex]);
			}
		}

		public enum Style
		{
			TwoColorTwoItems,
			OneColorTwoItems,
			FourColorFourItems,
			TwoColorFourItems,
			OneColorFourItems,
		}

		protected Sigil mProps = null;

		#if UNITY_EDITOR
		public void DrawEditor ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);
			if (!Manager.IsAwake <Colors> ()) {
				Manager.WakeUp <Colors> ("Frontiers_ArtResourceManagers");
			}

			if (GUILayout.Button ("Load Sigil")) {
				if (!string.IsNullOrEmpty (State.SigilName)) {
					if (Mods.Get.Editor.LoadMod <Sigil> (ref mProps, "Sigil", State.SigilName)) {
						RefreshBanner ();
					}
				}
			}
		}
		#endif
	}

	[Serializable]
	public class CoatOfArmsState
	{
		[FrontiersAvailableModsAttribute ("Sigil")]
		public string SigilName = string.Empty;
	}

	[Serializable]
	public class Sigil : Mod
	{
		public Sigil () : base () {
			Type = "Sigil";
		}

		public string FamilyName = string.Empty;
		public bool Royalty = false;
		public CoatOfArms.Style Style = CoatOfArms.Style.TwoColorTwoItems;
		public int BannerColorIndex = 0;
		public int[] BannerItemIconIndex = new int [4];
		public int[] BannerItemColorIndex	= new int [4];
	}
}
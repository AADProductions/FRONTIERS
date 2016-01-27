using UnityEngine;
using System.Collections;
using Frontiers.World.WIScripts;

namespace Frontiers.World {
	public class BannerEditor : MonoBehaviour {

		#if UNITY_EDITOR
		[FrontiersAvailableModsAttribute ("Sigil")]
		public string BannerName = string.Empty;
		public Sigil Banner = new Sigil ();
		public bool CreatingNewBanner = false;

		int selectedItemIndex = 0;
		int selectedRowIndex = 0;
		bool openWindow = false;
		bool chooseResult = false;

		public void Update ()
		{
			LoadBanner (false);
		}

		public void DrawEditor ()
		{
			if (CreatingNewBanner) {
				UnityEngine.GUI.color = Color.red;
				GUILayout.Label ("[Editing new Banner - this Banner has not been saved]");
			}
			UnityEngine.GUI.color = Color.cyan;
			GUILayout.BeginHorizontal ();

			int colorIndexOneTwoThreeFour = 0;
			int colorIndexTwoFour = 0;
			int colorIndexOneThree = 0;

//			switch (Banner.Style) {
//			case CoatOfArms.Style.OneColorFourItems:
//				colorIndexOneTwoThreeFour = Props.BannerItemColorIndex [0];
//				if (Type == SigilType.BannerTwoItems) {
//					UpdateBannerItem (0, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [1], true);				
//				} else {
//					UpdateBannerItem (0, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [1], true);
//					UpdateBannerItem (2, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [2], true);
//					UpdateBannerItem (3, colorIndexOneTwoThreeFour, Props.BannerItemIconIndex [3], true);
//				}
//				break;
//
//			case CoatOfArms.Style.FourColorFourItems:
//				if (Type == SigilType.BannerTwoItems) {
//					UpdateBannerItem (0, Props.BannerItemColorIndex [0], Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, Props.BannerItemColorIndex [1], Props.BannerItemIconIndex [1], true);				
//				} else {
//					UpdateBannerItem (0, Props.BannerItemColorIndex [0], Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, Props.BannerItemColorIndex [1], Props.BannerItemIconIndex [1], true);
//					UpdateBannerItem (2, Props.BannerItemColorIndex [2], Props.BannerItemIconIndex [2], true);
//					UpdateBannerItem (3, Props.BannerItemColorIndex [3], Props.BannerItemIconIndex [3], true);
//				}
//				break;
//
//			case CoatOfArms.Style.OneColorTwoItems:
//				colorIndexTwoFour = Props.BannerItemColorIndex [0];
//				if (Type == SigilType.BannerTwoItems) {
//					UpdateBannerItem (0, colorIndexTwoFour, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//				} else {
//					UpdateBannerItem (0, colorIndexTwoFour, Props.BannerItemIconIndex [0], false);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//					UpdateBannerItem (2, colorIndexTwoFour, Props.BannerItemIconIndex [2], false);
//					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
//				}
//				break;
//
//			case CoatOfArms.Style.TwoColorFourItems:
//				colorIndexTwoFour = Props.BannerItemColorIndex [0];
//				colorIndexOneThree	= Props.BannerItemColorIndex [1];
//				if (Type == SigilType.BannerTwoItems) {
//					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//				} else {
//					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//					UpdateBannerItem (2, colorIndexOneThree, Props.BannerItemIconIndex [2], true);
//					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
//				}
//				break;
//
//			case CoatOfArms.Style.TwoColorTwoItems:
//				colorIndexTwoFour = Props.BannerItemColorIndex [0];
//				colorIndexOneThree	= Props.BannerItemColorIndex [1];
//				if (Type == SigilType.BannerTwoItems) {
//					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], true);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//				} else {
//					UpdateBannerItem (0, colorIndexOneThree, Props.BannerItemIconIndex [0], false);
//					UpdateBannerItem (1, colorIndexTwoFour, Props.BannerItemIconIndex [1], true);
//					UpdateBannerItem (2, colorIndexOneThree, Props.BannerItemIconIndex [2], false);
//					UpdateBannerItem (3, colorIndexTwoFour, Props.BannerItemIconIndex [3], true);
//				}
//				break;
//
//			default:
//				break;
//			}
		
			GUILayout.EndHorizontal ();
			UnityEngine.GUI.color = Color.cyan;
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Name:");
			Banner.Name = GUILayout.TextField (Banner.Name);
			if (string.IsNullOrEmpty (Banner.Name)) {
				Banner.Name = "New Sigil";
			}
			GUILayout.Label ("Family Name:");
			Banner.FamilyName = GUILayout.TextField (Banner.FamilyName);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Banner Style");
			Banner.Style = (CoatOfArms.Style) UnityEditor.EditorGUILayout.EnumPopup (Banner.Style, GUILayout.Width (100));

			if (Colors.BannerColors == null) {
				Manager.WakeUp <Colors> ("Frontiers_ArtResourceManagers");
			}

			int [] bannerColorIndexes = new int [Colors.BannerColors.BannerColors.Count];
			string [] bannerColorNames = new string [bannerColorIndexes.Length];
			for (int i = 0; i < bannerColorIndexes.Length; i++) {
				bannerColorIndexes [i] = i;
				bannerColorNames [i] = "Color " + (i + 1).ToString ();
			}
			GUILayout.Label ("Banner Color:", GUILayout.Width (200));
			Banner.BannerColorIndex = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerColorIndex, bannerColorNames, bannerColorIndexes, GUILayout.Width (200));
			Color selectedColor = Colors.BannerColors.BannerColors [Banner.BannerColorIndex];
			UnityEditor.EditorGUILayout.ColorField (selectedColor);
			Banner.Royalty = GUILayout.Toggle (Banner.Royalty, "Royalty");
			GUILayout.EndHorizontal ();

			int [] bannerIconColorIndexes = new int [Colors.BannerColors.ObjectColors.Count];
			string [] bannerIconColorNames = new string [bannerColorIndexes.Length];
			for (int i = 0; i < bannerIconColorIndexes.Length; i++) {
				bannerIconColorIndexes [i] = i;
				bannerIconColorNames [i] = "Icon Color " + (i + 1).ToString ();
			}
			Color color1 = Colors.BannerColors.ObjectColors [Banner.BannerItemColorIndex [0]];
			Color color2 = Colors.BannerColors.ObjectColors [Banner.BannerItemColorIndex [1]];
			Color color3 = Colors.BannerColors.ObjectColors [Banner.BannerItemColorIndex [2]];
			Color color4 = Colors.BannerColors.ObjectColors [Banner.BannerItemColorIndex [3]];

			string color1Text = "Color 1";
			string color2Text = "Color 2";
			string color3Text = "Color 3";
			string color4Text = "Color 4";

			Color color1Color = Color.cyan;
			Color color2Color = Color.cyan;
			Color color3Color = Color.cyan;
			Color color4Color = Color.cyan;

			if (Banner.Style == CoatOfArms.Style.OneColorFourItems || Banner.Style == CoatOfArms.Style.OneColorTwoItems) {
				color2Text = "(None)";
				color3Text = "(None)";
				color4Text = "(None)";

				color2 = color1;
				color3 = color1;
				color4 = color1;

				color2Color = Color.gray;
				color3Color = Color.gray;
				color4Color = Color.gray;

			} else if (Banner.Style == CoatOfArms.Style.TwoColorFourItems || Banner.Style == CoatOfArms.Style.TwoColorTwoItems) {
				color3Text = "(None)";
				color4Text = "(None)";

				color3 = color1;
				color4 = color2;

				color3Color = Color.gray;
				color4Color = Color.gray;
			}


			UnityEngine.GUI.color = Color.yellow;
			GUILayout.Label ("\nCOLORS:");
			UnityEngine.GUI.color = Color.cyan;

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = color1Color;
			GUILayout.Label (color1Text, GUILayout.Width (100));
			UnityEditor.EditorGUILayout.ColorField (color1);
			Banner.BannerItemColorIndex [0] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemColorIndex [0], bannerIconColorNames, bannerIconColorIndexes);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = color2Color;
			GUILayout.Label (color2Text, GUILayout.Width (100));
			UnityEditor.EditorGUILayout.ColorField (color2);
			Banner.BannerItemColorIndex [1] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemColorIndex [1], bannerIconColorNames, bannerIconColorIndexes);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = color3Color;
			GUILayout.Label (color3Text, GUILayout.Width (100));
			UnityEditor.EditorGUILayout.ColorField (color3);
			Banner.BannerItemColorIndex [2] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemColorIndex [2], bannerIconColorNames, bannerIconColorIndexes);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = color4Color;
			GUILayout.Label (color4Text, GUILayout.Width (100));
			UnityEditor.EditorGUILayout.ColorField (color4);
			Banner.BannerItemColorIndex [3] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemColorIndex [3], bannerIconColorNames, bannerIconColorIndexes);
			GUILayout.EndHorizontal ();

			Colors.BannerColors.Refresh ();

			UnityEngine.GUI.color = Color.yellow;
			GUILayout.Label ("\nICONS");
			UnityEngine.GUI.color = Color.cyan;
			string [] bannerIconNames = Colors.BannerColors.ObjectIconNames.ToArray ( );
			int [] bannerIconIndexes = new int [bannerIconNames.Length];
			for (int i = 0; i < bannerIconIndexes.Length; i++) {
				bannerIconIndexes [i] = i;
			}

			string icon1Text = "Icon 1";
			string icon2Text = "Icon 2";
			string icon3Text = "Icon 3";
			string icon4Text = "Icon 4";

			Color icon1Color = color1;
			Color icon2Color = color2;
			Color icon3Color = color3;
			Color icon4Color = color4;

			if (Banner.Style == CoatOfArms.Style.OneColorTwoItems || Banner.Style == CoatOfArms.Style.OneColorTwoItems) {
				icon3Text = "(None)";
				icon4Text = "(None)";
			}

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = icon1Color;
			GUILayout.Label (icon1Text, GUILayout.Width (100));
			Banner.BannerItemIconIndex [0] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemIconIndex [0], bannerIconNames, bannerIconIndexes, GUILayout.Width (150));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = icon2Color;
			GUILayout.Label (icon2Text, GUILayout.Width (100));
			Banner.BannerItemIconIndex [1] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemIconIndex [1], bannerIconNames, bannerIconIndexes, GUILayout.Width (150));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = icon3Color;
			GUILayout.Label (icon3Text, GUILayout.Width (100));
			Banner.BannerItemIconIndex [2] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemIconIndex [2], bannerIconNames, bannerIconIndexes, GUILayout.Width (150));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = icon4Color;
			GUILayout.Label (icon4Text, GUILayout.Width (100));
			Banner.BannerItemIconIndex [3] = UnityEditor.EditorGUILayout.IntPopup (Banner.BannerItemIconIndex [3], bannerIconNames, bannerIconIndexes, GUILayout.Width (150));
			GUILayout.EndHorizontal ();

			Texture2D icon1 = Colors.BannerColors.ObjectIcons [Banner.BannerItemIconIndex [0]];
			Texture2D icon2 = Colors.BannerColors.ObjectIcons [Banner.BannerItemIconIndex [1]];
			Texture2D icon3 = Colors.BannerColors.ObjectIcons [Banner.BannerItemIconIndex [2]];
			Texture2D icon4 = Colors.BannerColors.ObjectIcons [Banner.BannerItemIconIndex [3]];

			if (Banner.Style == CoatOfArms.Style.OneColorTwoItems || Banner.Style == CoatOfArms.Style.TwoColorTwoItems) {
				icon3Color = Color.gray;
				icon4Color = Color.gray;

				icon3 = null;
				icon4 = null;
			}

			//GUILayout.BeginHorizontal ();
			UnityEngine.GUI.color = color1;
			GUILayout.Box (icon1);
			UnityEngine.GUI.color = color2;
			GUILayout.Box (icon2);
			UnityEngine.GUI.color = color3;
			GUILayout.Box (icon3);
			UnityEngine.GUI.color = color4;
			GUILayout.Box (icon4);
			//GUILayout.EndHorizontal ();





			UnityEngine.GUI.color = Color.yellow;

			if (GUILayout.Button ("\nNew Banner\n")) {
				NewBanner ();
			}

			if (GUILayout.Button ("\nSave Banner\n")) {
				SaveBanner ();
			}

			if (GUILayout.Button ("\nLoad Banner\n")) {
				LoadBanner (true);
			}
		}

		public void NewBanner ( )
		{
			CreatingNewBanner = true;
			selectedRowIndex = -1;
			selectedItemIndex = -1;
			Banner = new Sigil ();
		}

		public void LoadBanner (bool force)
		{
			if (CreatingNewBanner && !force) {
				return;
			}
			if (Banner.Name != BannerName || force) {
				if (!Manager.IsAwake <Mods> ()) {
					Manager.WakeUp <Mods> ("__MODS");
				}
				Mods.Get.Editor.InitializeEditor (true);

				Mods.Get.Editor.LoadMod <Sigil> (ref Banner, "Sigil", BannerName);
			}
		}

		public void SaveBanner ( )
		{
			//get all the banner colors straight

			CoatOfArms.Style finalStyle = CoatOfArms.Style.FourColorFourItems;

			bool oneColor = false;
			bool twoColors = false;
			bool threeOrFourColors = false;

			bool fourSymbols = false;
			if (Banner.Style == CoatOfArms.Style.FourColorFourItems || Banner.Style == CoatOfArms.Style.OneColorFourItems || Banner.Style == CoatOfArms.Style.TwoColorFourItems) {
				fourSymbols = true;
			}
			bool twoSymbols = !fourSymbols;

			if (fourSymbols) {
				if (Banner.BannerItemColorIndex [0] == Banner.BannerItemColorIndex [1]
				    && Banner.BannerItemColorIndex [1] == Banner.BannerItemColorIndex [2]
				    && Banner.BannerItemColorIndex [2] == Banner.BannerItemColorIndex [3]) {
					Debug.Log ("Four symbols, one color");
					oneColor = true;
				} else if (Banner.BannerItemColorIndex [0] == Banner.BannerItemColorIndex [2]
					&& Banner.BannerItemColorIndex [1] == Banner.BannerItemColorIndex [3]) {
					Debug.Log ("Four symbols, two colors");
					twoColors = true;
				} else {
					threeOrFourColors = true;
					Debug.Log ("Four symbols, four colors");
				}
			} else {
				if (Banner.BannerItemColorIndex [0] == Banner.BannerItemColorIndex [1]) {
					oneColor = true;
					Debug.Log ("Two symbols, one color");
				}
			}

			if (twoSymbols) {
				if (oneColor) {
					finalStyle = CoatOfArms.Style.OneColorTwoItems;
				} else {
					finalStyle = CoatOfArms.Style.TwoColorTwoItems;
				}
			} else {
				if (oneColor) {
					finalStyle = CoatOfArms.Style.OneColorFourItems;
				} else if (twoColors) {
					finalStyle = CoatOfArms.Style.TwoColorFourItems;
				} else {
					finalStyle = CoatOfArms.Style.FourColorFourItems;
				}
			}
		
			Banner.Style = finalStyle;

			Debug.Log ("Final banner style: " + Banner.Style.ToString ());

			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
			}
			Mods.Get.Editor.InitializeEditor (true);
			BannerName = Banner.Name;
			Mods.Get.Editor.SaveMod <Sigil> (Banner, "Sigil", BannerName);
			CreatingNewBanner = false;
		}
		#endif

	}
}

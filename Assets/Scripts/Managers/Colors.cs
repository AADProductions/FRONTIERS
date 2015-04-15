using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers
{
	public class Colors : Manager
	{
		//used mostly to manipulate colors on the fly
		//will also store / load color schemes
		public static Colors Get;
		public static PathColorManager PathColors;
		public static BannerColorManager BannerColors;
		public FontColorManager FontColors;
		public List <FlagsetColor> FlagsetColors = new List <FlagsetColor>();

		[HideInInspector]
		public string [] ColorNames {
			get {
				if (mColorNames == null) {
					FindColorNames();
				}
				return mColorNames;
			}
		}

		[HideInInspector]
		protected string[] mColorNames = null;

		public void SaveColors()
		{		//find fields
			FieldInfo[] fields = this.GetType().GetFields();
			SDictionary <string, SColor> saveData = new SDictionary <string, SColor>();
			foreach (FieldInfo field in fields) {
				if (field.FieldType == typeof(Color)) {	//TODO find best way to clean fields
					string colorName = field.Name;
					Color color = (Color)field.GetValue(this);
					mStandardColorLookup.Add(colorName, color);
				}
			}
		}

		public void LoadColors()
		{

		}

		public void PushInterfaceColors(IEnumerable<ColorKey> interfaceColors)
		{
			if (interfaceColors == null) {
				return;
			}
			Type type = typeof(Colors);
			foreach (ColorKey key in interfaceColors) {
				FieldInfo field = type.GetField(key.Name);
				if (field != null && field.FieldType == typeof(Color)) {
					field.SetValue(this, key.color);
				} else {
					Debug.Log("Couldn't set field " + key.Name + ", was null or not type of color");
				}
			}

			//get all label and button setup components
			//set them all to the new colors
			UILabel[] labels = GameObject.FindObjectsOfType <UILabel>();
			UITiledSprite[] bgSprites = GameObject.FindObjectsOfType <UITiledSprite>();
			UISlicedSprite[] borderSprites = GameObject.FindObjectsOfType <UISlicedSprite>();
			GUI.GUIButtonSetup[] buttonSetup = GameObject.FindObjectsOfType <GUI.GUIButtonSetup>();
			GUI.GUISliderSetColors[] sliderColors = GameObject.FindObjectsOfType <GUI.GUISliderSetColors>();
			GUI.GUIPopupSetup[] popupSetup = GameObject.FindObjectsOfType <GUI.GUIPopupSetup>();

			for (int i = 0; i < labels.Length; i++) {
				if (labels[i].useDefaultLabelColor) {
					labels[i].color = Colors.Alpha(MenuButtonTextColorDefault, labels[i].alpha);
				}
			}

			for (int i = 0; i < bgSprites.Length; i++) {
				if (bgSprites[i].useDefaultBackgroundColor) {
					bgSprites[i].color = Colors.Alpha(TiledBackgroundColorDefault, bgSprites[i].alpha);
				}
			}

			for (int i = 0; i < borderSprites.Length; i++) {
				if (borderSprites[i].useDefaultBorderSpriteColor) {
					borderSprites[i].color = Colors.Alpha(WindowBorderColorDefault, borderSprites[i].alpha);
				}
			}

			for (int i = 0; i < buttonSetup.Length; i++) {
				buttonSetup[i].RefreshColors();
			}

			for (int i = 0; i < sliderColors.Length; i++) {
				sliderColors[i].RefreshColors();
			}

			for (int i = 0; i < popupSetup.Length; i++) {
				popupSetup[i].RefreshColors();
			}
		}

		public List <ColorKey> DefaultInterfaceColors()
		{
			return mDefaultInterfaceColors;
		}

		public void FindColorNames()
		{
			FieldInfo[] fields = this.GetType().GetFields();
			List <string> colorNames = new List<string>() { "" };
			foreach (FieldInfo field in fields) {
				if (field.FieldType == typeof(Color)) {	//TODO find best way to clean fields
					colorNames.Add(field.Name);
				}
			}
			foreach (ColorKey colorKey in ColorKeys) {
				colorNames.Add(colorKey.Name);
			}
			colorNames.Sort();
			colorNames.Insert(0, "[Default]");
			mColorNames = colorNames.ToArray();
		}

		public override void WakeUp()
		{
			base.WakeUp();

			PathColors = gameObject.GetComponent <PathColorManager>();
			BannerColors = gameObject.GetComponent <BannerColorManager>();

			if (mStandardColorLookup.Count == 0) {
				//build the color lookup
				FieldInfo[] fields = this.GetType().GetFields();
				foreach (FieldInfo field in fields) {
					if (field.FieldType == typeof(Color)) {	//TODO find best way to clean fields
						string colorName = field.Name;
						Color color = (Color)field.GetValue(this);
						if (field.IsDefined(typeof(InterfaceColorAttribute), true)) {
							ColorKey cc = new ColorKey();
							cc.Name = field.Name;
							cc.color = color;
							mDefaultInterfaceColors.Add(cc);
						}
						mStandardColorLookup.Add(colorName, color);
					}
				}

				foreach (FlagsetColor flagsetColor in FlagsetColors) {
					Dictionary <int, Color> lookup = null;
					if (!mColorFromFlagsetLookup.TryGetValue(flagsetColor.Flagset, out lookup)) {
						lookup = new Dictionary <int, Color>();
						mColorFromFlagsetLookup.Add(flagsetColor.Flagset, lookup);
					}
					lookup.Add(flagsetColor.Flags, flagsetColor.Color);
				}

				foreach (ColorKey colorKey in ColorKeys) {
					mStandardColorLookup.Add(colorKey.Name, colorKey.color);
				}
			}
			Get = this;
		}

		public Color SkillGroupColor(string skillGroup)
		{
			Color color = Color.white;
			mSkillGroupColorLookup.TryGetValue(skillGroup, out color);
			return color;
		}

		public Color ColorFromFlagset(int flags, string flagset)
		{		//TODO replace this we're no longer using it
			Color color = Color.white;
			Dictionary <int, Color> lookup = null;
			if (mColorFromFlagsetLookup.TryGetValue(flagset, out lookup)) {
				////Debug.Log ("Found flagset lookup " + flagset);
				if (lookup.TryGetValue(flags, out color)) {
					////Debug.Log ("Found credentials " + flagset + ", " + flags);
				}
			}
			return color;
		}
		#if UNITY_EDITOR
		public string GetOrCreateColor(Color existingColor, int tolerance, string newColorName)
		{
			string closest = string.Empty;
			int closestSoFar = Int32.MaxValue;
			foreach (KeyValuePair <string,Color> colorPair in mCustomColorLookup) {
				int rDiff = (int)Mathf.Abs(existingColor.r - colorPair.Value.r) * 255;
				int gDiff = (int)Mathf.Abs(existingColor.g - colorPair.Value.g) * 255;
				int bDiff = (int)Mathf.Abs(existingColor.b - colorPair.Value.b) * 255;
				if (rDiff < closestSoFar && rDiff <= tolerance
				        && gDiff < closestSoFar && gDiff <= tolerance
				        && bDiff < closestSoFar && bDiff <= tolerance) {
					closestSoFar = Mathf.Max(rDiff, gDiff);
					closestSoFar = Mathf.Max(closestSoFar, bDiff);
					closest = colorPair.Key;
				}
			}

			if (string.IsNullOrEmpty(closest)) {
				ColorKey ck = new ColorKey();
				ck.Name = newColorName;
				ck.color = existingColor;
				ColorKeys.Add(ck);
				UnityEditor.EditorUtility.SetDirty(gameObject);
				UnityEditor.EditorUtility.SetDirty(this);
				closest = newColorName;
			}

			return closest;
		}
		#endif
		public static Color RandomColor()
		{
			return new Color(UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f));
		}

		public static Color RandomColor(float alpha)
		{
			return new Color(UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f), alpha);
		}

		public static float InverseLuminance(Color result)
		{
			return 1.0f - ((result.r / 3) + (result.g / 3) + (result.b / 3));
		}

		public static int MaxIndexRGBA(Color color)
		{
			int index = 0;
			index = (color.g > color.r) ? 1 : index;
			index = (color.b > color.g) ? 2 : index;
			index = (color.a > color.b) ? 3 : index;
			return index;
		}

		public static int MaxIndexARGB(Color color)
		{
			int index = 0;
			index = (color.r > color.a) ? 1 : index;
			index = (color.g > color.r) ? 2 : index;
			index = (color.b > color.g) ? 3 : index;
			return index;
		}

		public static Color ColorFromString(string prefabName, int minValue)
		{
			System.Random rnd = new System.Random(prefabName.GetHashCode());
			return new Color(((float)rnd.Next(minValue, 255)) / 255f, ((float)rnd.Next(minValue, 255)) / 255f, ((float)rnd.Next(minValue, 255)) / 255f, 1.0f);
		}

		public Color RepColor(Color neutralColor, float normalizedRepDifference)
		{
			return BlendThree(GenericHighValue, neutralColor, GenericLowValue, normalizedRepDifference);
		}

		public Color HairColor(CharacterHairColor hairColor)
		{
			switch (hairColor) {
				case CharacterHairColor.Black:
				default:
					return HairColorBlack;

				case CharacterHairColor.Blonde:
					return HairColorBlonde;

				case CharacterHairColor.Brown:
					return HairColorBrown;

				case CharacterHairColor.Red:
					return HairColorRed;
			}
		}

		public List <ColorKey> ColorKeys = new List <ColorKey>();

		public List <ColorKey> InterfaceColorKeys()
		{
			List <ColorKey> colorKeys = new List<ColorKey>();
			System.Type type = typeof(Colors);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo f in fields) {
				if (f.IsDefined(typeof(InterfaceColorAttribute), true)) {
					ColorKey cc = new ColorKey();
					cc.Name = f.Name;
					cc.color = (Color)f.GetValue(this);
					colorKeys.Add(cc);
				}
			}
			return colorKeys;
		}

		public Color LuminiteStencilColorNight = Color.white;
		public Color LuminiteStencilColorDay = Color.white;
		public Color LuminiteStencilColorNightSpyglass = Color.white;
		public Color InteriorAmbientColorDay;
		public Color InteriorAmbientColorNight;
		public Color BelowGroundAmbientColor;
		public Color AboveGroundNightAmbientColor;
		public Color AboveGroundDayAmbientColor;
		public Color WorldMapLabelDescriptiveColor;
		public Color WorldMapLabelColor;
		public Color PlayerIlluminationColor = Color.white;
		public Color DayReflectionColor;
		public Color NightReflectionColor;
		public Color DayBaseColor;
		public Color NightBaseColor;
		public Color LightningFlashColor = Color.white;
		public Color DarkLuminiteMaterialColor = Color.white;
		public Color OptionDialogCredentialsText = Color.white;
		public Color PathEvaluatingColor1 = Color.white;
		public Color PathEvaluatingColor2 = Color.white;
		public Color WorldPathDifficultyEasy = Color.white;
		public Color WorldPathDifficultyModerate = Color.white;
		public Color WorldPathDifficultyDifficult = Color.white;
		public Color WorldPathDifficultyDangerous = Color.white;
		public Color WorldPathDifficultyDeadly = Color.white;
		public Color WorldPathDifficultyImpassable = Color.white;
		public Color WorldItemPlacementPermitted = Color.white;
		public Color WorldItemPlacementNotPermitted = Color.white;
		public Color FoodStuffEdible = Color.white;
		public Color FoodStuffPoisonous = Color.white;
		public Color FoodStuffHallucinogen = Color.white;
		public Color FoodStuffMedicinal = Color.white;
		public Color WorldRouteMarkerRevealed = Color.white;
		public Color WorldRouteMarkerVisited = Color.white;
		[InterfaceColorAttribute]
		public Color GenericHighValue = Color.white;
		[InterfaceColorAttribute]
		public Color GenericMidValue = Color.white;
		[InterfaceColorAttribute]
		public Color GenericLowValue = Color.white;
		[InterfaceColorAttribute]
		public Color GenericNeutralValue = Color.white;
		[InterfaceColorAttribute]
		public Color HUDNameColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDNegativeFGColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDNegativeBGColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDPositiveFGColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDPositiveBGColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDNeutralFGColor = Color.white;
		[InterfaceColorAttribute]
		public Color HUDNeutralBGColor = Color.white;
		[InterfaceColorAttribute]
		public Color PingNegativeColor = Color.white;
		[InterfaceColorAttribute]
		public Color PingPositiveColor = Color.white;
		[InterfaceColorAttribute]
		public Color PingNeutralColor = Color.white;
		[InterfaceColorAttribute]
		public Color ConversationBracketedText = Color.white;
		[InterfaceColorAttribute]
		public Color ConversationPlayerOption = Color.white;
		[InterfaceColorAttribute]
		public Color ConversationPlayerBackground = Color.white;
		[InterfaceColorAttribute]
		public Color MessageSuccessColor = Color.white;
		[InterfaceColorAttribute]
		public Color MessageInfoColor = Color.white;
		[InterfaceColorAttribute]
		public Color MessageWarningColor = Color.white;
		[InterfaceColorAttribute]
		public Color MessageDangerColor = Color.white;
		[InterfaceColorAttribute]
		public Color MenuButtonTextColorDefault = Color.white;
		[InterfaceColorAttribute]
		public Color MenuButtonTextOutlineColor = Color.white;
		[InterfaceColorAttribute]
		public Color MenuButtonBackgroundColorDefault = Color.white;
		[InterfaceColorAttribute]
		public Color MenuButtonOverlayColorDefault = Color.white;
		[InterfaceColorAttribute]
		public Color WindowBorderColorDefault = Color.white;
		[InterfaceColorAttribute]
		public Color PopupListBackgroundColor = Color.white;
		[InterfaceColorAttribute]
		public Color PopupListForegroundColor = Color.white;
		[InterfaceColorAttribute]
		public Color TiledBackgroundColorDefault = Color.white;
		[InterfaceColorAttribute]
		public Color SliderThumbColor = Color.white;
		[InterfaceColorAttribute]
		public Color SliderForegroundColor = Color.white;
		[InterfaceColorAttribute]
		public Color SliderBackgroundColor = Color.white;
		[InterfaceColorAttribute]
		public Color GeneralHighlightColor = Color.white;
		[InterfaceColorAttribute]
		public Color WarningHighlightColor = Color.white;
		[InterfaceColorAttribute]
		public Color SuccessHighlightColor = Color.white;
		[InterfaceColorAttribute]
		public Color SkillIconColor = Color.white;
		[InterfaceColorAttribute]
		public Color SkillMasteredColor = Color.white;
		[InterfaceColorAttribute]
		public Color SkillLearnedColorLow = Color.white;
		[InterfaceColorAttribute]
		public Color SkillLearnedColorMid = Color.white;
		[InterfaceColorAttribute]
		public Color SkillLearnedColorHigh = Color.white;
		[InterfaceColorAttribute]
		public Color SkillKnownColor = Color.white;
		[InterfaceColorAttribute]
		public Color SkillUnknownColor = Color.white;
		[InterfaceColorAttribute]
		public Color BookColorGeneric = Color.white;
		[InterfaceColorAttribute]
		public Color BookColorMission = Color.white;
		[InterfaceColorAttribute]
		public Color BookColorLore = Color.white;
		[InterfaceColorAttribute]
		public Color BookColorSkill = Color.white;
		[InterfaceColorAttribute]
		public Color WorldMapPathColor = Color.white;
		[InterfaceColorAttribute]
		public Color WorldMapActivePathColor = Color.white;
		[InterfaceColorAttribute]
		public Color VRIconColorOn = Color.white;
		[InterfaceColorAttribute]
		public Color VRIconColorOff = Color.gray;
		[InterfaceColorAttribute]
		public Color VRIconColorForceOn = Color.white;
		[InterfaceColorAttribute]
		public Color VRIconColorForceOff = Color.white;
		public Color HairColorBlack = Color.white;
		public Color HairColorBrown = Color.white;
		public Color HairColorBlonde = Color.white;
		public Color HairColorRed = Color.white;

		public Color ByName(string colorName)
		{
			Color color = Color.magenta;
			if (!mStandardColorLookup.TryGetValue(colorName, out color)) {
				color = Color.magenta;
			}
			return color;
		}

		public static Color Min(Color color, Color minColor)
		{
			return new Color(Mathf.Max(minColor.r, color.r), Mathf.Max(minColor.g, color.g), Mathf.Max(minColor.b, color.b), Mathf.Max(minColor.a, color.a));
		}

		public static Color BlendThree(Color c1, Color c2, Color c3, float normalizedBlend)
		{
			Color c = c1;
			if (normalizedBlend < 0.5f) {
				c = Color.Lerp(c1, c2, (normalizedBlend / 0.5f));
			} else {
				c = Color.Lerp(c2, c3, (normalizedBlend - 0.5f) / 1.0f);
			}
			return c;
		}

		public static Color BlendFour(Color c1, Color c2, Color c3, Color c4, float normalizedBlend)
		{
			if (normalizedBlend <= 0) {
				return c1;
			} else if (normalizedBlend <= 0.33) {
				return Color.Lerp(c1, c2, (normalizedBlend / 0.33F));
			} else if (normalizedBlend <= 0.66) {
				return Color.Lerp(c2, c3, ((normalizedBlend - 0.33F) / 0.33F));
			} else if (normalizedBlend <= 0.99) {
				return Color.Lerp(c3, c4, ((normalizedBlend - 0.66F) / 0.33F));
			} else {
				return c4;
			}
		}

		public static string NGUIColorCompletedObjective = "[7DC242]";
		public static string NGUIColorActiveObjective = "[FFF4DF]";
		public static string NGUIColorFailedObjective = "[AD2D1C]";
		public static Color WorldItemHUDFailure = Color.Lerp(Color.white, Color.red, 0.75f);
		public static Color WorldItemHUDNormal = Color.Lerp(Color.white, Color.green, 0.35f);
		public static Color WorldItemHUDFire = Color.Lerp(Color.yellow, Color.red, 0.25f);
		public static Color WorldItemHUDLiquid = Color.Lerp(Color.blue, Color.red, 0.25f);

		public static string ColorWrap(string text, string hex, bool noDefault)
		{
			if (noDefault) {
				return '[' + hex + ']' + text + "[-]";
			} else {
				return '[' + hex + ']' + text + '[' + ColorToHex(Get.MenuButtonTextColorDefault) + ']';
			}
		}

		public static string ColorWrap(string text, Color color)
		{
			return '[' + ColorToHex(color) + ']' + text + '[' + ColorToHex(Get.MenuButtonTextColorDefault) + ']';
		}

		public static string ColorWrap(string text, Color color, bool noDefault)
		{
			if (noDefault) {
				return '[' + ColorToHex(color) + ']' + text + "[-]";
			} else {
				return '[' + ColorToHex(color) + ']' + text + '[' + ColorToHex(Get.MenuButtonTextColorDefault) + ']';
			}
		}

		public static string ColorWrap(string text, Color wrapColor, Color defaultColor)
		{
			return '[' + ColorToHex(wrapColor) + ']' + text + '[' + ColorToHex(defaultColor) + ']';
		}

		public static Color Alpha(Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		public static Color GetColorFromWorldPathDifficulty(PathDifficulty difficulty)
		{
			if (Get == null) {
				return Color.white;
			}
			
			Color difficultyColor = Get.WorldPathDifficultyEasy;
			
			switch (difficulty) {
				case PathDifficulty.Easy:
					difficultyColor = Get.WorldPathDifficultyEasy;
					break;
				
				case PathDifficulty.Moderate:
					difficultyColor = Get.WorldPathDifficultyModerate;
					break;
				
				case PathDifficulty.Difficult:
					difficultyColor = Get.WorldPathDifficultyDifficult;
					break;
				
				case PathDifficulty.Deadly:
					difficultyColor = Get.WorldPathDifficultyDeadly;
					break;
				
				case PathDifficulty.Impassable:
					difficultyColor = Get.WorldPathDifficultyImpassable;
					break;
				
				default:
					break;
			}
			
			return difficultyColor;
		}

		public static float Value(Color c)
		{
			return (c.r + c.g + c.b) / 3;
		}

		public static Color Clamp(Color originalColor, float min)
		{
			if (originalColor.r < min) {
				originalColor.r = min;
			}
			if (originalColor.g < min) {
				originalColor.g = min;
			}
			if (originalColor.b < min) {
				originalColor.b = min;
			}
			return originalColor;
		}

		public static Color Brighten(Color originalColor)
		{
			return Saturate(Color.Lerp(originalColor, Color.white, 0.75f));
		}

		public static Color Desaturate(Color originalColor)
		{
			HSBColor hsbColor = new HSBColor(originalColor);
			hsbColor.s = 0.5f;
			return hsbColor.ToColor();
		}

		public static Color Lighten(Color originalColor)
		{
			return Color.Lerp(originalColor, Color.white, 0.45f);
		}

		public static Color	Lighten(Color originalColor, float lightenAmount, float alpha)
		{
			return Alpha(Color.Lerp(originalColor, Color.white, lightenAmount), alpha);
		}

		public static Color Darken(Color originalColor)
		{
			return Color.Lerp(originalColor, Color.black, 0.45f);
		}

		public static Color Dim(Color originalColor)
		{
			return Color.Lerp(originalColor, Color.black, 0.25f);
		}

		public static Color Saturate(Color originalColor)
		{
			HSBColor hsbColor = new HSBColor(originalColor);
			hsbColor.s = 1.0f;
			return hsbColor.ToColor();
		}

		public static Color Blacken(Color originalColor)
		{
			return Color.Lerp(originalColor, Color.black, 0.75f);
		}

		public static Color	Char(Color originalColor)
		{
			return Saturate(Blacken(originalColor));
		}

		public static Color	Disabled(Color originalColor)
		{
			return Color.Lerp(Darken(originalColor), Color.gray, 0.25f);
		}

		public static Color Muted(Color originalColor)
		{
			HSBColor hsbColor = new HSBColor(originalColor);
			hsbColor.s = 0.75f;
			return hsbColor.ToColor();
		}

		public static Color Lighten(Color originalColor, float alpha)
		{
			Color color = Lighten(originalColor);
			color.a = alpha;
			return color;
		}

		public static Color Darken(Color originalColor, float alpha)
		{
			Color color = Darken(originalColor);
			color.a = alpha;
			return color;
		}

		public static Color Blacken(Color originalColor, float alpha)
		{
			Color color = Blacken(originalColor);
			color.a = alpha;
			return color;
		}

		public static Color Disabled(Color originalColor, float alpha)
		{
			Color color = Disabled(originalColor);
			color.a = alpha;
			return color;
		}

		public static string ColorToHex(Color32 color)
		{
			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return hex;
		}

		public static Color HexToColor(string hex, Color failSafe)
		{
			byte r = 0;
			byte g = 0;
			byte b = 0;

			try {
				hex = hex.ToLower();
				//Debug.Log ("Converting hex " + hex);
				//byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				//byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				//byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
				r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			} catch (Exception e) {
				Debug.LogError("Error in HexToColor: " + e.ToString());
				return failSafe;
			}
			return new Color32(r, g, b, 255);
		}

		public static Color HexToColor(string hex)
		{
			hex = hex.ToLower();
			//Debug.Log ("Converting hex " + hex);
			byte r = 0;
			byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = 0;
			byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = 0;
			byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			try {
				r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			} catch (Exception e) {
				Debug.LogError("Error in HexToColor: " + e.ToString());
			}
			return new Color32(r, g, b, 255);
		}

		public static Color HueToColor(float hue, float saturation, float value)
		{
			HSBColor color = new HSBColor(hue, saturation, value, 1.0f);
			return color.ToColor();
		}

		public static Color HueToColor(float hue)
		{
			HSBColor color = new HSBColor(hue, 1.0f, 1.0f, 1.0f);
			return color.ToColor();
		}

		public static Color HueToColor(float hue, float saturation)
		{
			HSBColor color = new HSBColor(hue, saturation, 1.0f, 1.0f);
			return color.ToColor();
		}

		public static Color ShiftHue(Color baseColor, float hueShift)
		{
			HSBColor color = new HSBColor(baseColor);
			if (hueShift < 0) {
				hueShift = 1f + (hueShift % -1f);
			}
			color.h = hueShift % 1f;
			return color.ToColor();
		}

		public static Color ShiftHue(Color baseColor, float hueShift, float saturation)
		{
			HSBColor color = new HSBColor(baseColor);
			color.h = (color.h + hueShift) % 1f;
			color.s = saturation;
			return color.ToColor();
		}

		public static Color InvertColor(Color color)
		{
			color.r = 1f - color.r;
			color.g = 1f - color.g;
			color.b = 1f - color.b;
			return color;
		}

		public float MenuButtonOverlayAlpha = 0.25f;
		protected List <ColorKey> mDefaultInterfaceColors = new List<ColorKey>();
		protected Dictionary <string, Color> mStandardColorLookup = new Dictionary <string, Color>();
		protected Dictionary <string, Color> mCustomColorLookup = new Dictionary <string, Color>();
		protected Dictionary <string, Color> mSkillGroupColorLookup = new Dictionary <string, Color>();
		protected Dictionary <string, Dictionary <int, Color>> mColorFromFlagsetLookup = new Dictionary <string, Dictionary <int, Color>>();
		protected string mCurrentColorSchemeName = "Default";

		[Serializable]
		public class FlagsetColor
		{
			public string Flagset = "GuildCredentials";
			[FrontiersBitMaskAttribute("GuildCredentials")]//TODO fix this
						public int Flags = 0;
			public Color Color = new Color();
		}
	}

	[Serializable]
	public class ColorKey
	{
		public string Name;
		public Color color;
	}

	[Serializable]
	public class ColorScheme : Mod
	{
		public bool IsEmpty {
			get {
				return (GameColors == null || GameColors.Count == 0) && (InterfaceColors == null || InterfaceColors.Count == 0);
			}
		}

		public List <ColorKey> GameColors = new List<ColorKey>();
		public List <ColorKey> InterfaceColors = new List<ColorKey>();
	}
}
//class CompileBreaker { public CompileBreaker() { var i = 0 / 0; } }
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers
{
		public class Colors : Manager
		{		//used mostly to manipulate colors on the fly
				//will also store / load color schemes
				public static Colors Get;

				public static PathColorManager PathColors;
				public static BannerColorManager BannerColors;
				public FontColorManager FontColors;
				public List <FlagsetColor> FlagsetColors = new List <FlagsetColor>();
				public string CurrentColorSchemeName {
						get {
								return mCurrentColorSchemeName;
						}
						set {
								mCurrentColorSchemeName = value;
								//TODO load color scheme
						}
				}

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
						PathColors = gameObject.GetComponent <PathColorManager>();
						BannerColors = gameObject.GetComponent <BannerColorManager>();

						if (mStandardColorLookup.Count == 0) {
								//build the color lookup
								FieldInfo[] fields = this.GetType().GetFields();
								foreach (FieldInfo field in fields) {
										if (field.FieldType == typeof(Color)) {	//TODO find best way to clean fields
												string colorName = field.Name;
												Color color = (Color)field.GetValue(this);
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

				public List <ColorKey> ColorKeys = new List <ColorKey>();
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
				public Color GenericHighValue = Color.white;
				public Color GenericMidValue = Color.white;
				public Color GenericLowValue = Color.white;
				public Color GenericNeutralValue = Color.white;
				public Color HUDNameColor = Color.white;
				public Color HUDNegativeFGColor = Color.white;
				public Color HUDNegativeBGColor = Color.white;
				public Color HUDPositiveFGColor = Color.white;
				public Color HUDPositiveBGColor = Color.white;
				public Color HUDNeutralFGColor = Color.white;
				public Color HUDNeutralBGColor = Color.white;
				public Color PingNegativeColor = Color.white;
				public Color PingPositiveColor = Color.white;
				public Color PingNeutralColor = Color.white;
				public Color DarkLuminiteMaterialColor = Color.white;
				public Color OptionDialogCredentialsText = Color.white;
				public Color ConversationBracketedText = Color.white;
				public Color ConversationPlayerText = Color.white;
				public Color ConversationCharacterText = Color.white;
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
				public Color MessageSuccessColor = Color.white;
				public Color MessageInfoColor = Color.white;
				public Color MessageWarningColor = Color.white;
				public Color MessageDangerColor = Color.white;
				public Color MenuButtonTextColorDefault = Color.white;
				public Color MenuButtonTextOutlineColor = Color.white;
				public Color MenuButtonBackgroundColorDefault	= Color.white;
				public Color MenuButtonOverlayColorDefault = Color.white;
				public Color PopupListBackgroundColor = Color.white;
				public Color PopupListForegroundColor = Color.white;
				public Color SliderThumbColor = Color.white;
				public Color SliderForegroundColor = Color.white;
				public Color SliderBackgroundColor = Color.white;
				public Color GeneralHighlightColor = Color.white;
				public Color WarningHighlightColor = Color.white;
				public Color SuccessHighlightColor = Color.white;
				public Color SkillIconColor = Color.white;
				public Color FoodStuffEdible = Color.white;
				public Color FoodStuffPoisonous = Color.white;
				public Color FoodStuffHallucinogen = Color.white;
				public Color FoodStuffMedicinal = Color.white;
				public Color WorldRouteMarkerRevealed = Color.white;
				public Color WorldRouteMarkerVisited = Color.white;
				public Color SkillMasteredColor = Color.white;
				public Color SkillLearnedColorLow = Color.white;
				public Color SkillLearnedColorMid = Color.white;
				public Color SkillLearnedColorHigh = Color.white;
				public Color SkillKnownColor = Color.white;
				public Color SkillUnknownColor = Color.white;
				public Color BookColorGeneric = Color.white;
				public Color BookColorMission = Color.white;
				public Color BookColorLore = Color.white;
				public Color BookColorSkill = Color.white;

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

				public static Color HexToColor(string hex)
				{
						//Debug.Log ("Converting hex " + hex);
						byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
						byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
						byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
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

				public float MenuButtonOverlayAlpha = 0.25f;
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
				SDictionary <string, SColor> StandardColors = new SDictionary <string, SColor>();
				SDictionary <string, SColor> CustomColors = new SDictionary <string, SColor>();
		}
}
//class CompileBreaker { public CompileBreaker() { var i = 0 / 0; } }
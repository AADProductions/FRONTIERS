using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System.Text.RegularExpressions;
using System;

namespace Frontiers.GUI
{
		public class GUIColorBrowser : GUIBrowserSelectView <ColorKey>
		{
				public GUITabPage ControllingTabPage;
				public GameObject ApplySettingsButton;
				public UILabel ConfirmMessageLabel;
				public UILabel RLabel;
				public UILabel GLabel;
				public UILabel BLabel;
				public UILabel ALabel;
				public UILabel ColorNameLabel;
				public UISprite ColorSprite;
				public UIInput RInput;
				public UIInput GInput;
				public UIInput BInput;
				public UIInput AInput;
				public UIInput HexInput;
				public Color32 CurrentColor;
				public UILabel CurrentFontLabel;
				public UILabel FontButtonLabel;
				public UIFont CurrentFont;

				public override IEnumerable<ColorKey> FetchItems()
				{
						List <ColorKey> colorKeys = Colors.Get.InterfaceColorKeys();
						mSelectedObject = colorKeys[0];
						return colorKeys;
				}

				public override bool PushToViewerAutomatically {
						get { 
								return true;
						}
				}

				public override void Start()
				{
						base.Start();

						RInput.eventReceiver = gameObject;
						GInput.eventReceiver = gameObject;
						BInput.eventReceiver = gameObject;
						AInput.eventReceiver = gameObject;

						RInput.functionName = "OnColorValueChange";
						GInput.functionName = "OnColorValueChange";
						BInput.functionName = "OnColorValueChange";
						AInput.functionName = "OnColorValueChange";

						RInput.functionNameEnter = "OnColorValueChange";
						GInput.functionNameEnter = "OnColorValueChange";
						BInput.functionNameEnter = "OnColorValueChange";
						AInput.functionNameEnter = "OnColorValueChange";

						HexInput.eventReceiver = gameObject;
						HexInput.functionName = "OnHexValueChange";
						HexInput.functionNameEnter = "OnHexValueChangeEnter";
				}

				public override void ReceiveFromParentEditor(IEnumerable<ColorKey> editObject, ChildEditorCallback<IEnumerable<ColorKey>> callBack)
				{
						mEditObject = editObject;
						mCallBack = callBack;
						HasFocus = true;
						ConfirmMessageLabel.text = string.Empty;
						PushEditObjectToNGUIObject();
				}

				public override void WakeUp()
				{
						ControllingTabPage.OnSelected += Show;
						ControllingTabPage.OnDeselected += Hide;
				}

				protected override GameObject ConvertEditObjectToBrowserObject(ColorKey editObject)
				{
						GameObject browserObject = base.ConvertEditObjectToBrowserObject(editObject);
						GUIGenericBrowserObject bo = browserObject.GetComponent <GUIGenericBrowserObject>();
						bo.EditButton.target = this.gameObject;
						bo.EditButton.functionName = "OnClickBrowserObject";
						bo.name = editObject.Name;
						bo.Name.text = editObject.Name;
						bo.Name.font = Mats.Get.Arimo20Font;
						bo.Background.color = Colors.Darken(editObject.color);
						bo.Icon.color = editObject.color;
						bo.IconBackround.enabled = false;
						bo.MiniIcon.enabled = false;
						bo.MiniIconBackground.enabled = false;
						return browserObject;
				}

				public override void PushSelectedObjectToViewer()
				{
						mColorValueChanging = true;

						CurrentColor = mSelectedObject.color;

						RInput.text = CurrentColor.r.ToString();
						GInput.text = CurrentColor.g.ToString();
						BInput.text = CurrentColor.b.ToString();
						AInput.text = CurrentColor.a.ToString();

						RInput.label.text = RInput.text;
						GInput.label.text = GInput.text;
						BInput.label.text = BInput.text;
						AInput.label.text = AInput.text;

						HexInput.text = Colors.ColorToHex(CurrentColor);
						HexInput.label.text = HexInput.text;

						ColorNameLabel.text = mSelectedObject.Name;
						ColorSprite.color = mSelectedObject.color;

						if (CurrentFont == null || CurrentFont.name != Profile.Get.CurrentPreferences.Accessibility.DefaultFont) {
								CurrentFont = Mats.Get.FontByName(Profile.Get.CurrentPreferences.Accessibility.DefaultFont);
						}

						CurrentFontLabel.font = CurrentFont;
						FontButtonLabel.text = CurrentFont.name;

						mColorValueChanging = false;
				}

				public void OnHexValueChange()
				{
						if (mColorValueChanging) {
								return;
						}

						HexInput.label.text = HexInput.text + "[FF00FF]" + HexInput.caratChar + "[-]";
				}

				public void OnHexValueChangeEnter()
				{

						if (mColorValueChanging) {
								return;
						}

						mColorValueChanging = true;

						Regex rgx = new Regex("[^a-zA-Z0-9 -]");
						HexInput.text = rgx.Replace(HexInput.text, "0");
						if (HexInput.text.Length < 6) {
								HexInput.text = HexInput.text.PadRight(6, '0');		
						} else if (HexInput.text.Length >= 6) {
								HexInput.text = HexInput.text.Substring(0, 6);
						}

						CurrentColor = Colors.HexToColor(HexInput.text, CurrentColor);

						RInput.text = CurrentColor.r.ToString();
						GInput.text = CurrentColor.g.ToString();
						BInput.text = CurrentColor.b.ToString();
						AInput.text = CurrentColor.a.ToString();

						RInput.label.text = RInput.text;
						GInput.label.text = GInput.text;
						BInput.label.text = BInput.text;
						AInput.label.text = AInput.text;

						mSelectedObject.color = CurrentColor;
						ColorSprite.color = mSelectedObject.color;

						try {
								GUIGenericBrowserObject gbo = mBrowserObject.GetComponent <GUIGenericBrowserObject>();
								gbo.Background.color = mSelectedObject.color;
						} catch (Exception e) {
								Debug.Log("Non-critical error in color browser: " + e.ToString());
						}

						mColorValueChanging = false;
				}

				public void OnColorValueChange()
				{
						if (mColorValueChanging)
								return;

						mColorValueChanging = true;

						int r = 0;
						int g = 0;
						int b = 0;
						int a = 0;

						int.TryParse(RInput.text, out r);
						int.TryParse(GInput.text, out g);
						int.TryParse(BInput.text, out b);
						int.TryParse(AInput.text, out a);

						CurrentColor.r = (byte)r;
						CurrentColor.g = (byte)g;
						CurrentColor.b = (byte)b;
						CurrentColor.a = (byte)a;

						RInput.text = CurrentColor.r.ToString();
						GInput.text = CurrentColor.g.ToString();
						BInput.text = CurrentColor.b.ToString();
						AInput.text = CurrentColor.a.ToString();

						RInput.label.text = RInput.text;
						GInput.label.text = GInput.text;
						BInput.label.text = BInput.text;
						AInput.label.text = AInput.text;

						mSelectedObject.color = CurrentColor;
						ColorSprite.color = mSelectedObject.color;

						HexInput.text = Colors.ColorToHex(CurrentColor);
						HexInput.label.text = HexInput.text;
						try {
								GUIGenericBrowserObject gbo = mBrowserObject.GetComponent <GUIGenericBrowserObject>();
								gbo.Background.color = mSelectedObject.color;
						} catch (Exception e) {
								Debug.Log("Non-critical error in color browser: " + e.ToString());
						}

						mColorValueChanging = false;
				}

				public void OnClickChangeFontButton()
				{
						//by this point we should have a current font
						CurrentFont = Mats.Get.NextFont(CurrentFont);
						CurrentFontLabel.font = CurrentFont;
						FontButtonLabel.text = CurrentFont.name;
				}

				protected bool mColorValueChanging = false;

				public void OnClickSaveChangesButton()
				{
						Colors.Get.PushInterfaceColors(mEditObject);
						Profile.Get.CurrentPreferences.Accessibility.DefaultFont = CurrentFont.name;
						Mats.Get.PushDefaultFont(Profile.Get.CurrentPreferences.Accessibility.DefaultFont);
						if (Profile.Get.CurrentPreferences.CustomColors == null) {
								Profile.Get.CurrentPreferences.CustomColors = new ColorScheme();
						}
						Profile.Get.CurrentPreferences.CustomColors.InterfaceColors.Clear();
						Profile.Get.CurrentPreferences.CustomColors.InterfaceColors.AddRange(mEditObject);
						Profile.Get.SaveCurrent(ProfileComponents.Preferences);
				}

				public void OnClickResetAllButton()
				{
						Profile.Get.CurrentPreferences.Accessibility.DefaultFont = Globals.DefaultFontName;
						if (Profile.Get.CurrentPreferences.CustomColors == null) {
								Profile.Get.CurrentPreferences.CustomColors = new ColorScheme();
						}
						Profile.Get.CurrentPreferences.CustomColors.InterfaceColors.Clear();
						Profile.Get.CurrentPreferences.CustomColors.InterfaceColors.AddRange(Colors.Get.DefaultInterfaceColors());
						Colors.Get.PushInterfaceColors(Profile.Get.CurrentPreferences.CustomColors.InterfaceColors);
						Mats.Get.PushDefaultFont(Globals.DefaultFontName);
						ReceiveFromParentEditor(FetchItems(), null);
				}
		}
}
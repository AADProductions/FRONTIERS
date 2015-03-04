using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUISliderSetColors : MonoBehaviour
		{
				public bool DisableOnStartup = false;

				public void RefreshColors()
				{
						UISlider slider = gameObject.GetComponent <UISlider>();		
						if (slider.enabled) {
								LightenButton();
						} else {
								DarkenButton();
						}
				}

				public void DarkenButton()
				{
						Transform background = transform.FindChild("Background");		
						UISlider slider = gameObject.GetComponent <UISlider>();		
						slider.foreground.GetComponent <UISprite>().color = Colors.Darken(Colors.Get.SliderForegroundColor);
						background.GetComponent <UISprite>().color = Colors.Darken(Colors.Get.SliderBackgroundColor);
						slider.thumb.GetComponent <UISprite>().color = Colors.Get.SliderThumbColor;
			
						UIButtonColor thumbButton = slider.thumb.GetComponent <UIButtonColor>();
						thumbButton.defaultColor = Colors.Darken(Colors.Get.SliderThumbColor);
						thumbButton.hover = Colors.Darken(Colors.Get.SliderThumbColor);
						thumbButton.pressed = Colors.Darken(Colors.Get.SliderThumbColor);
			
						Transform labelTransform	= transform.FindChild("Label");
						if (labelTransform != null) {
								UILabel label = labelTransform.GetComponent <UILabel>();
								label.color = Colors.Darken(Colors.Get.MenuButtonTextColorDefault);
								label.effectColor = Colors.Get.MenuButtonTextOutlineColor;
								label.effectStyle = UILabel.Effect.Shadow;//UILabel.Effect.Outline;
						}
				}

				public void LightenButton()
				{
						Transform background = transform.FindChild("Background");		
						UISlider slider = gameObject.GetComponent <UISlider>();		
						slider.foreground.GetComponent <UISprite>().color = Colors.Get.SliderForegroundColor;
						background.GetComponent <UISprite>().color = Colors.Get.SliderBackgroundColor;
						slider.thumb.GetComponent <UISprite>().color = Colors.Get.SliderThumbColor;
			
						UIButtonColor thumbButton	= slider.thumb.GetComponent <UIButtonColor>();
						thumbButton.defaultColor	= Colors.Get.SliderThumbColor;
						thumbButton.hover = Colors.Get.GeneralHighlightColor;
						thumbButton.pressed = Colors.Get.GeneralHighlightColor;
			
						Transform labelTransform	= transform.FindChild("Label");
						if (labelTransform != null) {
								UILabel label = labelTransform.GetComponent <UILabel>();
								label.color = Colors.Get.MenuButtonTextColorDefault;
								label.effectColor = Colors.Get.MenuButtonTextOutlineColor;
								label.effectStyle = UILabel.Effect.Shadow;//UILabel.Effect.Outline;
						}
				}

				public void	SetDisabled()
				{
						DarkenButton();
			
						UISlider slider = gameObject.GetComponent <UISlider>();
						slider.enabled = false;
						mStartupSet = true;
						slider.collider.enabled	= false;
				}

				public void SetEnabled()
				{
						LightenButton();
			
						UISlider slider = gameObject.GetComponent <UISlider>();
						slider.enabled = true;
						mStartupSet = true;
						slider.collider.enabled	= true;
				}

				public void Start()
				{
						if (!mStartupSet) {
								if (DisableOnStartup) {
										SetDisabled();
								} else {
										SetEnabled();
								}
						}
				}

				protected bool mStartupSet = false;
		}
}
using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
		public class GUICrosshair : MonoBehaviour
		{
				public UISprite CrosshairSprite;
				public float TargetOpacity = 0.0f;
				public float CurrentOpacity = 0.0f;
				public float FadeTime = 1.0f;

				public void Update()
				{
						TargetOpacity = 0.0f;
						if (!GUIManager.HideCrosshair && GameManager.Is(FGameState.InGame)) {
								if (Player.Local.ItemPlacement.PlacementModeEnabled) {
										TargetOpacity = 1.0f;
										CrosshairSprite.color = Colors.Get.MessageSuccessColor;
								} else {
										CrosshairSprite.color = Colors.Get.GeneralHighlightColor;
										if (Player.Local.Surroundings.IsWorldItemInRange || (Player.Local.Surroundings.IsTerrainInRange && Player.Local.Surroundings.TerrainFocus.gameObject.layer == Globals.LayerNumFluidTerrain)) {
												TargetOpacity = 1.0f * AlphaInGeneral;
										} else if (Player.Local.Tool.LaunchForce > 0f) {
												TargetOpacity = 1.0f * AlphaInGeneral;
										} else {
												TargetOpacity = 0.0f + (AlphaWhenInactive * AlphaInGeneral);
										}
								}
						}	
						CurrentOpacity = Mathf.Lerp(CurrentOpacity, TargetOpacity, FadeTime);//color resets alpha
						CrosshairSprite.alpha = CurrentOpacity;
				}

				public static float AlphaWhenInactive = 0.0f;
				public static float	AlphaInGeneral = 1.0f;
				protected bool mHidden = false;
		}
}

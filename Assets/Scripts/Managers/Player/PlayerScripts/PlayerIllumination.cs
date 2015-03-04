using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.GUI;
using Frontiers.World;

namespace Frontiers
{
		public class PlayerIllumination : PlayerScript
		{
				public Material LightProjectorMaterial;
				public Projector LightSourceProjector;
				public WorldItem LastEquippedLightSource;
				public float TargetRange = 10f;
				public float TargetAngle = 10f;
				public Color CurrentColor;
				public Color TargetColor;

				public override void Initialize()
				{
						/*Debug.Log("initialize in player illumination");
						LightProjectorMaterial = new Material(Mats.Get.LightProjectorMaterial);
						LightProjectorMaterial.color = Color.black;
						LightSourceProjector = player.FPSCameraSeat.gameObject.GetOrAdd <Projector>();
						LightSourceProjector.nearClipPlane = 0.01f;
						LightSourceProjector.farClipPlane = 1f;
						LightSourceProjector.material = LightProjectorMaterial;
						base.Initialize();*/
						//enabled = true;
				}

				public void Update()
				{
						if (!GameManager.Is(FGameState.InGame) || !mInitialized) {
								return;
						}

						//if we've equipped a tool with a light source
						if (player.Tool.IsEquipped && player.Tool.worlditem.HasLightSource) {
								//enable our projector
								LightSourceProjector.enabled = true;
								if (LastEquippedLightSource != player.Tool.worlditem) {
										Debug.Log("Resetting color");
										//reset our color so we can fade up again
										CurrentColor = Color.black;
										LightSourceProjector.farClipPlane = 1f;
										LastEquippedLightSource = player.Tool.worlditem;
										//use its template to get the color, etc
										WorldLightTemplate wlt = LastEquippedLightSource.Light.Template;
										TargetRange = wlt.SpotlightRange;
										TargetAngle = wlt.SpotlightAngle * 1.1f;//because we're seeing it farther away
								}
								TargetColor = player.Tool.worlditem.Light.BaseLight.color;
								CurrentColor = Color.Lerp(CurrentColor, TargetColor, 0.05f);
								LightProjectorMaterial.color = CurrentColor;
								//lerp the range values etc of our projector
								LightSourceProjector.enabled = true;
								LightSourceProjector.farClipPlane = Mathf.Lerp(LightSourceProjector.farClipPlane, TargetRange, 0.25f);
								LightSourceProjector.fieldOfView = Mathf.Lerp(LightSourceProjector.fieldOfView, TargetAngle, 0.25f);
						} else {
								if (LastEquippedLightSource != null) {
										Debug.Log("Setting light source to null");
										LastEquippedLightSource = null;
										TargetColor = Color.black;
								}
								if (LightSourceProjector.enabled) {
										//if it's not already off, fade it to black
										CurrentColor = Color.Lerp(CurrentColor, TargetColor, 0.05f);
										if (CurrentColor.r < 0.01f) {
												LightSourceProjector.enabled = false;
										} else {
												LightProjectorMaterial.color = CurrentColor;
										}
								}
						}
				}
		}
}
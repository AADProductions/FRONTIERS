using UnityEngine;
using System.Collections;
using Frontiers;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUIStatusFlow : MonoBehaviour
		{
				public StatusFlow Flow;
				public UISprite Icon;
				public UISprite IconOverlay;
				public UISprite PingSprite;
				public GUIStatusKeeper ParentStatusKeeper;
				public GUIButtonHover ButtonHover;
				public Color IconColor;
				public float DisplaySize;
				public float DisplayOffset;
				public int DisplayPosition;

				public void Awake()
				{
						Flow = null;
						PingSprite.alpha = 0f;
						ButtonHover = gameObject.GetOrAdd <GUIButtonHover>();
						ButtonHover.OnButtonHover += OnButtonHover;
						transform.localScale = Vector3.one * 0.001f;
				}

				public void OnButtonHover()
				{
						GUIPlayerStatusInterface.Get.PostInfo(UICamera.hoveredObject, Flow.Description);
				}

				public void Initialize(StatusFlow flow)
				{
						Flow = flow;
						Icon.atlas = Mats.Get.ConditionIconsAtlas;
						Icon.spriteName = Flow.IconName;

						switch (Flow.FlowType) {
								case StatusSeekType.Negative:
								case StatusSeekType.Neutral:
										//TODO custom colors, different colors
										IconColor = Colors.Get.ByName("GenericLowValue");
										break;
				
								case StatusSeekType.Positive:
								default:
										IconColor = Colors.Get.ByName("GenericHighValue");
										break;
						}

						StartCoroutine(UpdateOverTime());
				}

				protected IEnumerator UpdateOverTime()
				{
						mScale = 0f;
						while (mScale < 1f) {
								UpdateColors();

								mScale = Mathf.Lerp(mScale, 1f, 0.25f).Snap01(0.01f);
								transform.localScale = Vector3.one * mScale;
								mTargetPosition.x = 0f;
								mTargetPosition.y = (DisplayPosition * DisplaySize) + DisplayOffset;
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								yield return null;
						}

						while (Flow != null && Flow.HasEffect) {
								UpdateColors();
								mTargetPosition.x = 0f;
								mTargetPosition.y = (DisplayPosition * DisplaySize) + DisplayOffset;
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								PingSprite.color = IconColor;
								PingSprite.alpha = Mathf.Lerp(Flow.FlowLastUpdate, PingSprite.alpha, 0.25f);
								yield return null;
						}

						while (mScale > 0f) {
								UpdateColors();
								mTargetPosition.x = 0f;
								mTargetPosition.y = (DisplayPosition * DisplaySize) + DisplayOffset;
								mScale = Mathf.Lerp(mScale, 0f, 0.25f).Snap01(0.01f);
								transform.localScale = Vector3.one * mScale;
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								yield return null;
						}

						GameObject.Destroy(gameObject);
				}

				protected void UpdateColors()
				{
						Icon.color = Colors.Lighten(IconColor);
						Icon.alpha = 1f;
						IconOverlay.color = IconColor;
						IconOverlay.alpha = 0.5f;
				}

				protected Vector3 mTargetPosition;
				protected float mScale = 0f;
		}
}

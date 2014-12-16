using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using ExtensionMethods;

namespace Frontiers.GUI
{
		public class GUIStatusCondition : MonoBehaviour
		{
				public Condition condition;
				public Symptom symptom;
				public UISprite Icon;
				public UISprite PingSprite;
				public UILabel StackLabel;
				public Color IconColor;
				public float DisplaySize;
				public float DisplayOffset;
				public int DisplayPosition;

				public void Awake()
				{
						condition = null;
						transform.localScale = Vector3.one * 0.001f;
				}

				public void Initialize(Condition newCondition, string keeperName)
				{
						condition = newCondition;
						//assume no error here
						condition.HasSymptomFor(keeperName, out symptom);
						Icon.atlas = Mats.Get.ConditionIconsAtlas;
						Icon.spriteName = condition.IconName;

						switch (symptom.SeekType) {
								case StatusSeekType.Negative:
										IconColor = Colors.Get.GenericLowValue;
										break;

								case StatusSeekType.Positive:
										IconColor = Colors.Get.GenericHighValue;
										break;

								case StatusSeekType.Neutral:
										IconColor = Colors.Get.GenericNeutralValue;
										break;
						}

						StartCoroutine(UpdateOverTime());
				}

				protected IEnumerator UpdateOverTime()
				{
						mScale = 0f;
						while (mScale < 1f) {
								UpdateColors(false);

								mScale = Mathf.Lerp(mScale, 1f, 0.25f).Snap01(0.01f);
								transform.localScale = Vector3.one * mScale;
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								yield return null;
						}

						while (condition != null && !condition.HasExpired) {
								UpdateColors(false);
								mTargetPosition = new Vector3(0f, (DisplayPosition * DisplaySize) + DisplayOffset, 0f);
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								yield return null;
						}

						while (mScale > 0f) {
								UpdateColors(true);

								mScale = Mathf.Lerp(mScale, 0f, 0.25f).Snap01(0.01f);
								transform.localScale = Vector3.one * mScale;
								transform.localPosition = Vector3.Lerp(transform.localPosition, mTargetPosition, 0.25f);
								yield return null;
						}

						GameObject.Destroy(gameObject);
				}

				protected void UpdateColors(bool expired)
				{
						PingSprite.color = IconColor;
						if (expired) {
								PingSprite.alpha = Mathf.Lerp(Mathf.Max(1.0f - condition.NormalizedTimeLeft, 0.5f), PingSprite.alpha, 0.25f);
						} else {
								PingSprite.alpha = Mathf.Lerp(PingSprite.alpha, 0f, 0.25f);
						}
						Icon.color = Colors.Lighten(IconColor);
						Icon.alpha = 1f;
				}

				protected Vector3 mTargetPosition;
				protected float mScale = 0f;
		}
}
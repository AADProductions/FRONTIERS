using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUISkillObject : GUIObject
		{
				public bool IsExpired {
						get {
								return mParentSkill == null;
						}
				}

				public int PositionInList {
						set {
								TargetPosition = new Vector3(0f, OffsetSize * value, 0f);
						}
				}

				public float OffsetSize = 75f;
				public double MinimumTimeDisplayed = 2.0f;
				public UISprite SkillIcon;
				public UISprite SkillBorder;
				public UISprite SkillGlow;
				public Vector3 TargetPosition;
				protected double mTimeDisplayed = 0f;
				protected float mScale = 1f;
				protected Skill mParentSkill;

				public void Start()
				{
						mScale = 0.001f;
						transform.localScale = Vector3.one * mScale;
						mTimeDisplayed = WorldClock.RealTime;
						if (!string.IsNullOrEmpty(InitArgument)) {
								mParentSkill = null;
								if (Skills.Get.SkillByName(InitArgument, out mParentSkill)) {
										//load props
										name = mParentSkill.name;
										SkillIcon.spriteName = mParentSkill.Info.IconName;
										SkillIcon.color = mParentSkill.SkillIconColor;
										SkillBorder.color = mParentSkill.SkillBorderColor;
										SkillGlow.color = SkillBorder.color;
										SkillGlow.alpha = 1f;
								}
						}
						StartCoroutine(UpdateSkillGlow());
				}

				protected IEnumerator UpdateSkillGlow()
				{
						//grow up
						while (mScale < 1f) {
								mScale = Mathf.Lerp(mScale, 1f, 0.5f);
								if (mScale > 0.99f) {
										mScale = 1f;
								}
								if (mParentSkill.Usage.Type == SkillUsageType.Duration) {
										SkillGlow.alpha = mParentSkill.NormalizedEffectTimeLeft;
								} else {
										SkillGlow.alpha = 1f;
								}
								transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 0.5f);
								transform.localScale = Vector3.one * mScale;
								yield return null;
						}
						//update usage
						while (mParentSkill.IsInUse) {
								if (mParentSkill.Usage.Type == SkillUsageType.Duration) {
										SkillGlow.alpha = mParentSkill.NormalizedEffectTimeLeft;
								} else {
										SkillGlow.alpha = 1f;
								}
								//move us into our position
								transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 0.5f);
								yield return null;
						}
						while (SkillGlow.alpha > 0f) {
								SkillGlow.alpha = Mathf.Lerp(SkillGlow.alpha, 0f, 0.125f);
								if (SkillGlow.alpha < 0.01f) {
										SkillGlow.alpha = 0f;
								}
								transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 0.5f);
								yield return null;
						}
						//wait until it's time to make it go away
						while (WorldClock.RealTime < mTimeDisplayed + MinimumTimeDisplayed) {
								//expire so we don't continue to use this one
								mParentSkill = null;
								yield return null;
						}
						//now make it go away
						while (mScale > 0f) {
								//scale down
								mScale = Mathf.Lerp(mScale, 0f, 0.5f);
								if (mScale < 0.01f) {
										mScale = 0f;
								}
								transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, 0.5f);
								transform.localScale = Vector3.one * mScale;
								yield return null;
						}
						GameObject.Destroy(gameObject);
						yield break;
				}
		}
}

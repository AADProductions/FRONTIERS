using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI
{
		public class GUISkillBrowserObject : GUIGenericBrowserObject
		{
				public UISlicedSprite ProgressionSprite;
				public float MaxProgressionSize = 890f;
				public Color SkillIconColor;
				public Color SkillBorderColor;
				public Color SkillNameColor;
				public Color SkillBGColor;

				public void SetColors(Color skillIconColor, Color skillBorderColor, SkillKnowledgeState knowledgeState, string iconName)
				{
						SkillBGColor = Colors.Darken(Color.Lerp(skillIconColor, skillBorderColor, 0.5f));
						SkillNameColor = Color.white;
						SkillBorderColor = skillBorderColor;
						SkillIconColor = skillIconColor;

						Icon.atlas = Mats.Get.IconsAtlas;
						Icon.spriteName = iconName;
						IconBackround.color = SkillBorderColor;

						switch (knowledgeState) {
								case SkillKnowledgeState.Enabled:
								case SkillKnowledgeState.Learned:
								default:
										Icon.color = Colors.Lighten(SkillIconColor, 0.5f, 1f);
										SkillBGColor = Color.Lerp(SkillIconColor, SkillBorderColor, 0.5f);
										ProgressionSprite.enabled = true;
										break;

								case SkillKnowledgeState.Known:
										Icon.color = SkillIconColor;
										SkillBGColor = Color.Lerp(SkillIconColor, SkillBorderColor, 0.5f);
										ProgressionSprite.enabled = false;
										break;

								case SkillKnowledgeState.Unknown:
										Icon.color = SkillIconColor;
										SkillBGColor = Colors.Desaturate(SkillBGColor);
										SkillNameColor = Colors.Lighten(SkillBGColor);
										ProgressionSprite.enabled = false;
										break;			
						}
						if (mHasBeenMastered) {
								ProgressionSprite.enabled = false;
								Background.color = Color.Lerp(SkillBGColor, Colors.Get.SkillMasteredColor, 0.5f);
						} else {
								ProgressionSprite.color = Colors.Alpha(Colors.BlendThree(Colors.Get.SkillLearnedColorLow, Colors.Get.SkillLearnedColorMid, Colors.Get.SkillLearnedColorHigh, mNormalizedMasteryLevel), 0.5f);
								Background.color = Colors.Darken(SkillBGColor);						
						}
						Name.color = SkillNameColor;
				}

				public void SetPrereq(Skill prereq)
				{
						if (prereq != null) {
								MiniIcon.enabled = true;
								MiniIcon.spriteName = prereq.Info.IconName;
								MiniIcon.color = prereq.SkillIconColor;
								MiniIcon.alpha = 1f;
								MiniIconBackground.enabled = true;
								MiniIconBackground.color = prereq.SkillBorderColor;
						} else {
								MiniIcon.enabled = false;
								MiniIconBackground.enabled = false;
						}
				}

				public void SetName(string skillName, string skillSubGroup, string skillDescription)
				{
						string displayText = skillName;
						if (!string.IsNullOrEmpty(skillSubGroup)) {
								displayText += "  (" + skillSubGroup + ")";
						}
						string wrappedDescription = "  " + skillDescription.Replace("\n", "");
						wrappedDescription = wrappedDescription.Replace("_", " ");
						if (wrappedDescription.Length > 165) {
								wrappedDescription = wrappedDescription.Substring(0, 165) + "...";
						}
						displayText += Colors.ColorWrap(wrappedDescription, Colors.Dim(SkillNameColor));
						Name.text = displayText;
				}

				public bool HasBeenMastered {
						set {
								mHasBeenMastered = value;
								if (mHasBeenMastered) {
										AttentionIcon.enabled = true;
										AttentionIcon.color = Colors.Get.SkillMasteredColor;
								} else {
										AttentionIcon.enabled = false;
								}
						}
				}

				public bool GetPlayerAttention {
						set {
								if (value) {
										BackgroundHighlight.enabled = true;
										BackgroundHighlight.color = Colors.Get.GeneralHighlightColor;
								} else {
										BackgroundHighlight.enabled = false;
								}
						}
				}

				public override void Initialize(string argument)
				{

				}

				public float NormalizedMasteryLevel {
						set {
								mNormalizedMasteryLevel = value;
								Vector3 progressionSpriteSize = ProgressionSprite.transform.localScale;
								progressionSpriteSize.x = MaxProgressionSize * mNormalizedMasteryLevel;
								ProgressionSprite.transform.localScale = progressionSpriteSize;
						}
				}

				protected bool mHasBeenMastered = false;
				protected float mNormalizedMasteryLevel;
		}
}
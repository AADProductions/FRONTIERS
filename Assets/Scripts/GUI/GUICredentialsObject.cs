using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.GUI {
	public class GUICredentialsObject : GUIObject
	{
		public void OnPlayerInitialized ( )
		{
			Player.Get.AvatarActions.Subscribe (AvatarAction.SkillExperienceGain, new ActionListener (SkillExperienceGain));
			Player.Get.AvatarActions.Subscribe (AvatarAction.SkillCredentialsGain, new ActionListener (SkillCredentialsGain));
			Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalSpawn, new ActionListener (SurvivalSpawn));
			UpdateCredentialsIcon ();
		}

		public int Credentials;
		public string CredentialsFlagset;
		public double CredentialsGlowAmount = 0f;
		public UISprite CredentialsIcon;
		public UISprite CredentialsGlow;
		public UISprite CredentialsShadow;

		public bool SurvivalSpawn (double timeStamp)
		{
			UpdateCredentialsIcon ();
			return true;
		}

		public bool SkillCredentialsGain (double timeStamp)
		{
			UpdateCredentialsIcon ();
			CredentialsGlowAmount = 0.25f;
			CredentialsGlow.alpha = 1.0f;
			UpdateCredentialsLevel ();
			return true;
		}

		public bool SkillExperienceGain (double timeStamp)
		{
			CredentialsGlowAmount = 0.25f;
			CredentialsGlow.alpha = 1.0f;
			UpdateCredentialsLevel ();
			return true;
		}

		public void Update ( )
		{
			if (!GameManager.Is (FGameState.InGame) || Cutscene.IsActive) {
				CredentialsIcon.enabled = false;
				CredentialsGlow.enabled = false;
				CredentialsShadow.enabled = false;
				return;
			}

			CredentialsIcon.enabled = true;
			CredentialsGlow.enabled = true;
			CredentialsShadow.enabled = true;
			CredentialsGlowAmount = Mathf.Lerp ((float) CredentialsGlowAmount, 0f, (float) Frontiers.WorldClock.RTDeltaTime);
			CredentialsGlow.alpha = Mathf.Lerp ((float) (CredentialsGlow.alpha + CredentialsGlowAmount), 0f, (float) Frontiers.WorldClock.RTDeltaTime);
		}

		protected void UpdateCredentialsIcon ( )
		{
			Credentials = Skills.Get.Credentials (CredentialsFlagset);
			CredentialsIcon.spriteName = Mats.Get.Icons.GetIconNameFromFlagset (Credentials, CredentialsFlagset);
			CredentialsGlow.color = Colors.Get.ColorFromFlagset (Credentials, CredentialsFlagset);
			UpdateCredentialsLevel ();
		}

		protected void UpdateCredentialsLevel ( )
		{
			double normalizedCredentialsValue = Skills.Get.NormalizedExpToNextCredentials (CredentialsFlagset);
			CredentialsIcon.color = Colors.BlendThree (Colors.Get.GenericLowValue, Colors.Get.GenericMidValue, Colors.Get.GenericHighValue, (float) normalizedCredentialsValue);
		}

		protected bool mUpdatingCredentialsGlow = false;
	}
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;

namespace Frontiers.World.Gameplay
{
		public class FoeToFriend : Skill
		{
				public FoeToFriendExtensions Extensions = new FoeToFriendExtensions();

				protected override void OnUseStart()
				{
						if (LastSkillRoll == SkillRollType.Success) {
								Character character = LastSkillTarget.worlditem.Get <Character>();
								ReputationState rep = Profile.Get.CurrentGame.Character.Rep.GetReputation(character.worlditem.FileName);
								int repChange = Mathf.FloorToInt(Mathf.Lerp(Extensions.PersonalRepBonusUnskilled, Extensions.PersonalRepBonusSkilled, State.NormalizedMasteryLevel));
								int repPenalty = Mathf.FloorToInt(Mathf.Lerp(Extensions.PersonalRepPenaltyUnskilled, Extensions.PersonalRepPenaltySkilled, State.NormalizedMasteryLevel));
								if (HasBeenMastered) {
										repPenalty = Mathf.FloorToInt(repPenalty * Extensions.PersonalRepPenaltyMaster);
								}

								ReputationModifier repMod = new ReputationModifier(
										                        DisplayName,
										                        repChange,
										                        1.0f,
										                        true,
										                        EffectTime,
										                        (float)WorldClock.AdjustedRealTime,
										                        repPenalty);
								rep.AddModifier(repMod);
						}
				}
		}

		[Serializable]
		public class FoeToFriendExtensions
		{
				//temporary during skill usage
				public int PersonalRepBonusUnskilled;
				public int PersonalRepBonusSkilled;
				//permanent penalty after skill usage
				public int PersonalRepPenaltyUnskilled;
				public int PersonalRepPenaltySkilled;
				public float PersonalRepPenaltyMaster;
		}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World.Gameplay
{		//changes the player's visibility / audibility / etc properties
		//so other items using Looker / Listener scripts have a harder time seeing / hearing
		public class StealthSkill : Skill
		{
				public StealthSkillExtensions Extensions = new StealthSkillExtensions();

				public virtual float AudibleRangeMultiplier {
						get {
								return Mathf.Lerp(Extensions.AudibleRangeMultiplierUnskilled, Extensions.AudibleRangeMultiplierSkilled, State.NormalizedUsageLevel);
						}
				}

				public virtual float AudibleVolumeMultiplier {
						get {
								return Mathf.Lerp(Extensions.AudibleVolumeMultiplierUnskilled, Extensions.AudibleVolumeMultiplierSkilled, State.NormalizedUsageLevel);
						}
				}

				public virtual float AwarenessDistanceMultiplier {
						get {
								return Mathf.Lerp(Extensions.AwarenessDistanceMultiplierUnskilled, Extensions.AwarenessDistanceMultiplierSkilled, State.NormalizedUsageLevel);
						}
				}

				public virtual float FieldOfViewMultiplier {
						get {
								return Mathf.Lerp(Extensions.FieldOfViewMultiplierUnskilled, Extensions.FieldOfViewMultiplierSkilled, State.NormalizedUsageLevel);
						}
				}

				public virtual bool UserIsAudible {
						get {
								if (State.HasBeenMastered) {
										return Extensions.AudibleWhenUsingMaster;
								} else {
										return Extensions.AudibleWhenUsing;
								}
								return false;
						}
				}

				public virtual bool UserIsVisible {
						get {
								if (State.HasBeenMastered) {
										return Extensions.VisibleWhenUsingMaster;
								} else {
										return Extensions.VisibleWhenUsing;
								}
						}
				}

				public virtual bool UserCanTriggerTraps {
						get {
								if (State.HasBeenMastered) {
										return Extensions.TriggersTrapsWhenUsingMaster;
								} else {
										return Extensions.TriggersTrapsWhenUsing;
								}
						}
				}
		}

		[Serializable]
		public class StealthSkillExtensions
		{
				public float AudibleRangeMultiplierUnskilled = 1.0f;
				public float AudibleRangeMultiplierSkilled = 1.0f;
				public float AudibleVolumeMultiplierUnskilled = 1.0f;
				public float AudibleVolumeMultiplierSkilled = 1.0f;
				public float AwarenessDistanceMultiplierUnskilled = 1.0f;
				public float AwarenessDistanceMultiplierSkilled = 1.0f;
				public float FieldOfViewMultiplierUnskilled = 1.0f;
				public float FieldOfViewMultiplierSkilled = 1.0f;
				public bool VisibleWhenUsing = true;
				public bool VisibleWhenUsingMaster = true;
				public bool AudibleWhenUsing = true;
				public bool AudibleWhenUsingMaster = true;
				public bool TriggersTrapsWhenUsing = true;
				public bool TriggersTrapsWhenUsingMaster = true;
		}
}
using UnityEngine;
using System.Collections;

namespace Frontiers.World.BaseWIScripts
{
		public class Wild : WIScript
		{
				//creature exclusive script
				public Creature creature;

				public override void OnInitialized()
				{
						creature = worlditem.Get <Creature>();
						worlditem.Get <Damageable>().OnDie += OnDie;
						creature.OnRefreshBehavior += OnRefreshBehavior;
						creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
				}

				public void OnCollectiveThoughtStart()
				{
						if (mDestroyed) {
								return;
						}

						switch (creature.CurrentThought.CurrentItemOfInterest.IOIType) {
								case ItemOfInterestType.Light:
										if (WorldClock.IsNight) {
												creature.CurrentThought.Should(IOIReaction.FleeFromIt);
										}
										break;

								case ItemOfInterestType.Fire:
										creature.CurrentThought.Should(IOIReaction.FleeFromIt);
										break;

								default:
										break;
						}
				}

				public void OnDie()
				{
						Finish();
				}

				public void OnRefreshBehavior()
				{
						if (!mInitialized || mDestroyed) {
								return;
						}

						if (creature.State.Domestication != DomesticatedState.Wild) {
								Finish();
								return;
						}

						if (WorldClock.IsTimeOfDay(creature.State.AggressiveTOD)) {
								Timid timid = null;
								if (worlditem.Is <Timid>(out timid)) {
										timid.Finish();
								}
								worlditem.GetOrAdd <Aggressive>();
								creature.Body.EyeMode = BodyEyeMode.Aggressive;
						} else {
								//if we're not aggressive, we're timid because we're wild
								Aggressive aggressive = null;
								if (worlditem.Is <Aggressive>(out aggressive)) {
										aggressive.Finish();
								}
								worlditem.GetOrAdd <Timid>();
								creature.Body.EyeMode = BodyEyeMode.Timid;
						}
				}
		}
}

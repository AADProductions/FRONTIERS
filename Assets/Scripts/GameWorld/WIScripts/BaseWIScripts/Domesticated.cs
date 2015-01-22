using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World.BaseWIScripts
{
	public class Domesticated : WIScript {

		public Creature creature;

		public override void OnInitialized ()
		{
			creature = worlditem.Get <Creature> ();
			creature.OnRefreshBehavior += OnRefreshBehavior;
			creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
		}

		public void OnCollectiveThoughtStart ( )
		{
			if (mDestroyed) {
				return;
			}

			switch (creature.CurrentThought.CurrentItemOfInterest.IOIType) {
			case ItemOfInterestType.Light:
				//domestic animals are 'hypnotized' by light
				if (WorldClock.IsNight) {
					creature.CurrentThought.Should (IOIReaction.WatchIt, 3);
				}
				break;

			case ItemOfInterestType.Fire:
				if (creature.CurrentThought.CurrentItemOfInterest.fire.IsBurning (creature.worlditem.Position)) {
					creature.CurrentThought.Should (IOIReaction.FleeFromIt);
				} else if (WorldClock.IsNight) {
					//domestic animals are comfortable around fire
					creature.CurrentThought.Should (IOIReaction.WatchIt, 2);
				}
				break;

			default:
				break;
			}
		}

		public void OnRefreshBehavior ( )
		{
			if (!mInitialized || mDestroyed) {
				return;
			}

			if (creature.State.Domestication != DomesticatedState.Domesticated) {
				Finish ();
				return;
			}

			Aggressive aggressive = null;
			if (worlditem.Is <Aggressive> (out aggressive)) {
				aggressive.Finish ();
			}
			Timid timid = worlditem.GetOrAdd <Timid> ();
			creature.Body.EyeMode = BodyEyeMode.Timid;
		}
	}
}
using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World.WIScripts
{
	public class DenSick : WIScript
	{
		//this will make the creature want to return to its den no matter what else is going on
		//it will still be able to react to other things in the meantime
		public Creature creature;
		public Motile motile;

		public override void OnInitialized ()
		{
			worlditem.OnAddedToGroup += OnAddedToGroup;
		}

		public void OnAddedToGroup ()
		{
			creature = worlditem.Get <Creature> ();
			motile = worlditem.Get <Motile> ();
			creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
		}

		public void OnReturnToDenActionFinish ()
		{
			if (creature.IsInDen) {
				Finish ();
			} else {
				//force the issue
				mReturnToDenAction = creature.ReturnToDen ();
				if (mReturnToDenAction.OnFinishAction == null) {
					mReturnToDenAction.OnFinishAction += OnReturnToDenActionFinish;
				}
			}
		}

		public void OnCollectiveThoughtStart ()
		{
			if (creature.IsInDen) {
				Finish ();
				return;
			}
			//if we're not in the den yet
			//ignore pretty much anything
			creature.CurrentThought.Should (IOIReaction.IgnoreIt, 3);
		}

		protected MotileAction mReturnToDenAction = null;
	}
}
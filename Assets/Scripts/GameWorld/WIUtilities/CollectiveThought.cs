using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World
{
		[Serializable]
		public class CollectiveThought
		{		//used as a sort of crude fuzzy logic in creatures and characters
				public bool StartedThinking {
						get {
								return mTimeStartedThinking > 0;
						}
				}

				public bool KeepThinking = false;

				public bool IsFinishedThinking(float rtShortTermMemory)
				{
						if (!HasItemOfInterest) {
								//if the IOI is gone then we're done
								return false;
						} else if (KeepThinking) {
								//if we're being asked to keep thinking
								//keep going until we forget
								return (mTimeStartedThinking + WorldClock.RTSecondsToGameSeconds(rtShortTermMemory)) > WorldClock.Time;
						}
						//otherwise we're just done
						return true;
				}

				public void StartThinking(double timeStartedThinking)
				{
						mTimeStartedThinking = timeStartedThinking;
				}

				public void Reset()
				{
						Reset(null);
				}

				public void Reset(IItemOfInterest newItemOfInterest)
				{
						CurrentItemOfInterest = newItemOfInterest;
						mIgnoreIt = 0;
						mWatchIt = 0;
						mFollowIt = 0;
						mEatIt = 0;
						mFleeFromIt = 0;
						mKillIt = 0;
						mMateWithIt = 0;
						KeepThinking = false;
						mTimeStartedThinking = -1f;
						mActionResult = null;
				}

				public void TryToSendThought()
				{
						if (HasItemOfInterest) {

								mResult = IOIReaction.IgnoreIt;
								mActionResult = null;
								int highestSoFar = -1;
								if (mIgnoreIt > highestSoFar) {
										highestSoFar = mIgnoreIt;
										mResult = IOIReaction.IgnoreIt;
										mActionResult = OnIgnoreIt;
								}
								if (mWatchIt > highestSoFar) {
										highestSoFar = mWatchIt;
										mResult = IOIReaction.WatchIt;
										mActionResult = OnWatchIt;
								}
								if (mFollowIt > highestSoFar) {
										highestSoFar = mFollowIt;
										mResult = IOIReaction.FollowIt;
										mActionResult = OnFollowIt;
								}
								if (mEatIt > highestSoFar) {
										highestSoFar = mEatIt;
										mResult = IOIReaction.EatIt;
										mActionResult = OnEatIt;
								}
								if (mFleeFromIt > highestSoFar) {
										highestSoFar = mFleeFromIt;
										mResult = IOIReaction.FleeFromIt;
										mActionResult = OnFleeFromIt;
								}
								if (mKillIt > highestSoFar) {
										highestSoFar = mKillIt;
										mResult = IOIReaction.KillIt;
										mActionResult = OnKillIt;
								}
								if (mMateWithIt > highestSoFar) {
										highestSoFar = mMateWithIt;
										mResult = IOIReaction.MateWithIt;
										mActionResult = OnMateWithIt;
								}

								//Debug.Log ("Result was " + mResult.ToString ());
								if (mActionResult != null) {
										//Debug.Log ("Sending result");
										mActionResult(CurrentItemOfInterest);
								}
						}
				}

				public IItemOfInterest CurrentItemOfInterest;
				public Action <IItemOfInterest> OnIgnoreIt;
				public Action <IItemOfInterest> OnWatchIt;
				public Action <IItemOfInterest> OnFollowIt;
				public Action <IItemOfInterest> OnEatIt;
				public Action <IItemOfInterest> OnFleeFromIt;
				public Action <IItemOfInterest> OnKillIt;
				public Action <IItemOfInterest> OnMateWithIt;

				public bool HasItemOfInterest {
						get {
								return CurrentItemOfInterest != null;
						}
				}

				public void Should(IOIReaction reaction)
				{
						switch (reaction) {
								case IOIReaction.IgnoreIt:
								default:
										mIgnoreIt++;
										break;

								case IOIReaction.WatchIt:
										mWatchIt++;
										break;

								case IOIReaction.FollowIt:
										mFollowIt++;
										break;

								case IOIReaction.EatIt:
										mEatIt++;
										break;

								case IOIReaction.FleeFromIt:
										mFleeFromIt++;
										break;

								case IOIReaction.KillIt:
										mKillIt++;
										break;

								case IOIReaction.MateWithIt:
										mMateWithIt++;
										break;
						}
				}

				public void Should(IOIReaction reaction, int strength)
				{
						switch (reaction) {
								case IOIReaction.IgnoreIt:
								default:
										mIgnoreIt += strength;
										break;

								case IOIReaction.WatchIt:
										mWatchIt += strength;
										break;

								case IOIReaction.FollowIt:
										mFollowIt += strength;
										break;

								case IOIReaction.EatIt:
										mEatIt += strength;
										break;

								case IOIReaction.FleeFromIt:
										mFleeFromIt += strength;
										break;

								case IOIReaction.KillIt:
										mKillIt += strength;
										break;

								case IOIReaction.MateWithIt:
										mMateWithIt += strength;
										break;
						}
				}

				public IOIReaction Vote {
						get {
								return mResult;
						}
				}

				public override string ToString()
				{
						return "Ignore: " + mIgnoreIt.ToString()
								+ "\nWatch: " + mWatchIt.ToString()
								+ "\nFollow: " + mFollowIt.ToString()
								+ "\nEat: " + mEatIt.ToString()
								+ "\nFlee: " + mFleeFromIt.ToString()
								+ "\nKill: " + mKillIt.ToString();
				}

				protected IOIReaction mResult = IOIReaction.IgnoreIt;
				protected int mIgnoreIt = 0;
				protected int mWatchIt = 0;
				protected int mFollowIt = 0;
				protected int mEatIt = 0;
				protected int mFleeFromIt = 0;
				protected int mKillIt = 0;
				protected int mMateWithIt = 0;
				protected double mTimeStartedThinking = -1f;
				protected Action <IItemOfInterest> mActionResult = null;
		}

		public enum IOIReaction
		{
				IgnoreIt,
				WatchIt,
				FollowIt,
				EatIt,
				KillIt,
				FleeFromIt,
				MateWithIt,
		}

		public enum FightOrFlight
		{
				Fight,
				Flee,
		}

		public enum DomesticatedState
		{
				Wild,
				Domesticated,
				Tamed,
				Custom,
		}
}
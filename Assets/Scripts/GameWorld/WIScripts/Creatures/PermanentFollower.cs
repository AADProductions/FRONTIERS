using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers;
using Frontiers.Data;
using ExtensionMethods;

namespace Frontiers.World.WIScripts
{
		public class PermanentFollower : WIScript
		{
				public PermanentFollowerState State = new PermanentFollowerState();

				public override void OnInitialized()
				{
						mMotile = worlditem.Get <Motile>();
						mFollowAction = new MotileAction();
						mFollowAction.Type = MotileActionType.FollowTargetHolder;
						mFollowAction.FollowType = MotileFollowType.Follower;
						mFollowAction.Expiration = MotileExpiration.Never;
						mFollowAction.LiveTarget = Player.Local;
						mFollowAction.Instructions = MotileInstructions.PilgrimInstructions;
						mMotile.PushMotileAction(mFollowAction, MotileActionPriority.ForceBase);

						StartCoroutine(CheckFollowAction());
				}

				public IEnumerator CheckFollowAction()
				{
						while (worlditem.Mode != WIMode.Destroyed && mFollowAction != null) {
								Debug.Log("Making sure we're still following");
								if (mMotile.BaseAction != mFollowAction) {
										mMotile.PushMotileAction(mFollowAction, MotileActionPriority.ForceBase);
								}
								double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 0.5f;
								while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
										yield return null;
								}
						}
						yield break;
				}

				protected MotileAction mFollowAction;
				protected Motile mMotile;
		}

		[Serializable]
		public class PermanentFollowerState
		{
				public string Target = "[Player]";
		}
}
using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
	public class Tamed : WIScript {

		public Creature creature;

		public TamedState State = new TamedState ( );

		public void OnRefreshBehavior ( )
		{
			if (!mInitialized) {
				return;
			}

			if (creature.State.Domestication != DomesticatedState.Tamed) {
				Finish ();
				return;
			}

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				//make the base action to follow whoever we're imprinted on
				//this way it'll be the default thing we do
				//and we won't have to worry about keeping up on it
				mFollowLeaderAction = motile.State.BaseAction;
				mFollowLeaderAction.Reset ();
				mFollowLeaderAction.Type = MotileActionType.FollowTargetHolder;
				mFollowLeaderAction.Expiration = MotileExpiration.Never;
				mFollowLeaderAction.Range = 2f;
				mFollowLeaderAction.LiveTarget = Player.Local;
				mFollowLeaderAction.FollowType = MotileFollowType.Companion;
				mFollowLeaderAction.Instructions = MotileInstructions.CompanionInstructions;
			}
		}

		public override void OnInitialized ()
		{
			creature = worlditem.Get <Creature> ();
			creature.OnRefreshBehavior += OnRefreshBehavior;

			Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalHostileAggro, new ActionListener (SurvivalHostileAggro));
			Player.Get.AvatarActions.Subscribe (AvatarAction.SurvivalHostileDeaggro, new ActionListener (SurvivalHostileDeaggro));
		}

		public bool SurvivalHostileAggro (double timeStamp)
		{
			if (Player.Local.Surroundings.HasHostiles) {
				Hostile hostile = worlditem.GetOrAdd <Hostile> ();
				if (!hostile.HasPrimaryTarget) {
					//we're not currently attacking anything
					//so attack the first thing that's attacking the player	
					IHostile target = Player.Local.Surroundings.Hostiles [0];
					hostile.PrimaryTarget = target.hostile;
				}
			}
			return true;
		}

		public bool SurvivalHostileDeaggro (double timeStamp)
		{
			if (!Player.Local.Surroundings.HasHostiles) {
				Hostile hostile = null;
				if (worlditem.Is <Hostile> (out hostile)) {
					hostile.Finish ();
				}
			}
			return true;
		}

		public void OnTakeDamage ( )
		{
//			Damageable damageable = worlditem.Get <Damageable> ();
//			FightOrFlight fightOrFlightResponse = creature.State.OnTakeDamageTimid;
//			//Debug.Log ("Took damage");
//			if (damageable.NormalizedDamage > creature.State.FightOrFlightThreshold) {
//				//Debug.Log ("Took greater than threshold damage");
//				fightOrFlightResponse = creature.State.OnReachFFThresholdTimid;
//			}
//
//			switch (fightOrFlightResponse) {
//			case FightOrFlight.Fight:
//				//Debug.Log ("We're aggressive, and ff result is flight, so attack player");
//				creature.AttackPlayer ();
//				break;
//
//			case FightOrFlight.Flee:
//			default:
//				//Debug.Log ("We're aggressive, and ff result is flee, so flee player");
//				creature.FleeFromPlayer ();
//				break;
//			}
		}

		public void OnDie ( )
		{
			Finish ();
		}

		public void Imprint (PlayerBase player, double tamedTime, double tamedDuration, bool isPermanent)
		{
			State.ImprintedPlayer = player.ID;
			State.TamedTime = tamedTime;
			State.TamedDuration = tamedDuration;
			State.IsPermanent = isPermanent;
		}

		protected MotileAction mFollowLeaderAction;
	}

	[Serializable]
	public class TamedState {
		public string TamedName;
		public double TamedTime;
		public double TamedDuration;
		public bool IsPermanent;
		public PlayerIDFlag ImprintedPlayer = PlayerIDFlag.Local;
	}
}
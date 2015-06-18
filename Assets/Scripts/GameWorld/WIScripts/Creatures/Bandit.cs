using UnityEngine;
using System;
using System.Collections;
using Frontiers.World.Gameplay;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	public class Bandit : WIScript
	{
		public BanditState State = new BanditState ();
		public BanditCamp ParentCamp;
		public Character character;
		public Damageable damageable;
		public Hostile hostile;
		public Looker looker;

		public bool WillAssociateWithPlayer {
			get {
				return Profile.Get.CurrentGame.Character.Rep.NormalizedGlobalReputation < 0.1f;
			}
		}

		public override void OnInitialized ()
		{
			character = worlditem.Get <Character> ();
			character.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
			character.State.GlobalReputation = 1;

			damageable = worlditem.Get <Damageable> ();
			damageable.OnTakeDamage += OnTakeDamage;
			damageable.OnDie += OnDie;

			//this script won't get added by default
			looker = worlditem.GetOrAdd <Looker> ();
			looker.State.ItemsOfInterest.SafeAdd ("Creature");
			looker.State.ItemsOfInterest.SafeAdd ("Character");
			looker.State.VisibleTypesOfInterest |= ItemOfInterestType.WorldItem;
			looker.State.VisibleTypesOfInterest |= ItemOfInterestType.Player;
			Listener listener = worlditem.GetOrAdd <Listener> ();
		}

		public void OnCollectiveThoughtStart ()
		{
			if (mDestroyed) {
				return;
			}

			IItemOfInterest itemOfInterest = character.CurrentThought.CurrentItemOfInterest;

			switch (itemOfInterest.IOIType) {
			case ItemOfInterestType.Player:
				if (worlditem.Is <Hostile> (out hostile) && hostile.PrimaryTarget == itemOfInterest) {
					//don't need to do anything else, we're already attacking the player
					character.CurrentThought.Should (IOIReaction.IgnoreIt);
					Debug.Log ("Already hostile, not attacking player");
					return;
				} else {
					if (WorldItems.HasLineOfSight (character.Body.Transforms.HeadTop.position, itemOfInterest, ref gTargetPosition, ref gHitPosition, out gHitIOI)) {
						if (ParentCamp.PlayerVisitingCamp) {
							ParentCamp.HasAttackedPlayerRecently = true;
							Debug.Log ("Player is visiting camp, time to kill the player");
							character.CurrentThought.Should (IOIReaction.KillIt);
							character.CurrentThought.Should (IOIReaction.KillIt);
							character.CurrentThought.Should (IOIReaction.KillIt);
						} else {
							Debug.Log ("Player is NOT visiting camp");
							character.CurrentThought.Should (IOIReaction.WatchIt);
							//if the player is on the outskirts then warn the player 
							if (ParentCamp.HasAttackedPlayerRecently && !ParentCamp.HasTauntedPlayerRecently) {
								ParentCamp.HasTauntedPlayerRecently = true;
								Talkative talkative = worlditem.Get<Talkative> ();
								Motile motile = worlditem.Get <Motile> ();
								//get the node closest to the bandit
								talkative.GiveSpeech (ParentCamp.State.SpeechTaunt, motile.LastOccupiedNode);
							} else if (!ParentCamp.HasWarnedPlayerRecently) {
								Debug.Log ("Warning player with speech" + ParentCamp.State.SpeechWarning);
								ParentCamp.HasWarnedPlayerRecently = true;
								Talkative talkative = worlditem.Get<Talkative> ();
								Motile motile = worlditem.Get <Motile> ();
								//get the node closest to the bandit
								talkative.GiveSpeech (ParentCamp.State.SpeechWarning, motile.LastOccupiedNode);
							}
						}
					} else {
						Debug.Log ("Didn't have line of sight");
					}
				}
				break;

			case ItemOfInterestType.Scenery:
			default:
				character.CurrentThought.Should (IOIReaction.IgnoreIt);
				break;

			case ItemOfInterestType.WorldItem:
				if (worlditem.Is <Hostile> (out hostile)) {
					//don't need to do anything else, we're already attacking the player
					character.CurrentThought.Should (IOIReaction.IgnoreIt);
					return;
				} else if (itemOfInterest.worlditem.Is <Creature> ()) {
					Debug.Log ("Voting to kill creature " + itemOfInterest.worlditem.name + " in bandit");
					character.CurrentThought.Should (IOIReaction.KillIt);
					character.CurrentThought.Should (IOIReaction.KillIt);
				} else {
					character.CurrentThought.Should (IOIReaction.WatchIt);
				}
				break;
			}
		}

		public void OnTakeDamage ()
		{
			if (mDestroyed) {
				return;
			}

			if (damageable.NormalizedDamage < 0.75f) {
				character.AttackThing (damageable.LastDamageSource);
			} else {
				character.FleeFromThing (damageable.LastDamageSource);
			}
		}

		public void OnDie ()
		{
			Finish ();
		}

		public static Vector3 gTargetPosition;
		public static Vector3 gHitPosition;
		public static IItemOfInterest gHitIOI;
	}

	[Serializable]
	public class BanditState
	{
		public string SpeechOnAttackPlayer;
	}
}
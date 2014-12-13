using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
		public class Bandit : WIScript
		{
				public BanditState State = new BanditState ( );

				public Character character;
				public Damageable damageable;
				public Hostile hostile;

				public override void OnInitialized()
				{
						character = worlditem.Get <Character>();
						character.OnCollectiveThoughtStart += OnCollectiveThoughtStart;

						damageable = worlditem.Get <Damageable>();
						damageable.OnTakeDamage += OnTakeDamage;
						damageable.OnDie += OnDie;

						//this script won't get added by default
						worlditem.GetOrAdd <Looker>();
						worlditem.GetOrAdd <Listener>();
				}

				public void OnCollectiveThoughtStart()
				{
						if (mDestroyed) {
								return;
						}

						IItemOfInterest itemOfInterest = character.CurrentThought.CurrentItemOfInterest;

						switch (itemOfInterest.IOIType) {
								case ItemOfInterestType.Player:
										if (character.AttackThing(itemOfInterest)) {
												Talkative talkative = worlditem.Get <Talkative>();
												talkative.SayDTS(State.SpeechOnAttackPlayer);
										}
										break;

								case ItemOfInterestType.Scenery:
								case ItemOfInterestType.WorldItem:
								default:
										break;
						}
				}

				public void OnTakeDamage()
				{
						if (mDestroyed) {
								return;
						}

						if (damageable.NormalizedDamage < 0.75f) {
								character.AttackThing(damageable.LastDamageSource);
						} else {
								character.FleeFromThing(damageable.LastDamageSource);
						}
				}

				public void OnDie()
				{
						Finish();
				}
		}

		[Serializable]
		public class BanditState {
				public string SpeechOnAttackPlayer;
		}
}
using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Tameable : WIScript
		{
				public Creature creature;

				public override void OnInitialized()
				{
						//if we're already tamed
						//then this script doesn't need to be here
						if (worlditem.Is <Tamed>()) {
								Finish();
								return;
						}
						creature = worlditem.Get <Creature>();
						creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
						creature.OnRefreshBehavior += OnRefreshBehavior;
				}

				public void OnRefreshBehavior()
				{
						if (!mInitialized) {
								return;
						}

						if (creature.State.Domestication == DomesticatedState.Tamed) {
								Finish();
						}
				}

				public void OnCollectiveThoughtStart()
				{
						//get the current item of interest and see how we feel about it
						if (creature.CurrentThought.HasItemOfInterest && creature.CurrentThought.CurrentItemOfInterest.IOIType == ItemOfInterestType.Player) {
								//we're interested in the player when they have a piece of food equipped
								FoodStuff foodStuff = null;
								if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <FoodStuff>(out foodStuff)) {
										//player has something tasty equipped, what is it?
										if (Stacks.Can.Stack(foodStuff.worlditem, creature.Template.Props.FavoriteFood)) {
												//player has our favorite food, we're going to watch the player
												//if it's the same as our favorite food
												//vote twice to stay put
												creature.CurrentThought.Should(IOIReaction.WatchIt, 2);
										}
								}
						}
				}

				public bool TryToTame(Skill skill)
				{
						bool result = false;
						if (skill.LastSkillValue > StubbornnessTypeToFloat(creature.Template.Props.Stubbornness)) {
								//this function is called by the Beastmaster skill to verify a context action
								//it consumes whatever edibles are in the player's hand
								FoodStuff foodStuff = null;
								if (Player.Local.Tool.IsEquipped && Player.Local.Tool.worlditem.Is <FoodStuff>(out foodStuff)) {
										if (Stacks.Can.Stack(foodStuff.worlditem, creature.Template.Props.FavoriteFood)) {
												Debug.Log("Player has the food we like equipped");
												creature.Eat(foodStuff);
												Tamed tamed = worlditem.GetOrAdd <Tamed>();
												tamed.Imprint(Player.Local, WorldClock.AdjustedRealTime, skill.EffectTime, skill.HasBeenMastered);
												result = true;
										}
								}
						}

						if (result) {
								//if we've been tamed then there's no need for this script any more
								creature.State.Domestication = DomesticatedState.Tamed;
								creature.RefreshBehavior();
						}

						return result;
				}

				public static float StubbornnessTypeToFloat(StubbornnessType stubbornness)
				{
						switch (stubbornness) {
								//TODO move these values into globals
								case StubbornnessType.Passive:
								default:
										return 0f;

								case StubbornnessType.Independent:
										return 0.15f;

								case StubbornnessType.Willful:
										return 0.5f;

								case StubbornnessType.Untrainable:
										return 0.99f;
						}
						return 0f;
				}
		}
}
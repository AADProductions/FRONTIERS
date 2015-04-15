using UnityEngine;
using System;
using System.Collections;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class Student : WIScript
		{
				public Character character;
				public Looker looker;

				public override void OnInitialized()
				{
						character = worlditem.Get <Character>();
						character.OnCollectiveThoughtStart += OnCollectiveThoughtStart;

						//this script won't get added by default
						looker = worlditem.GetOrAdd <Looker>();
				}

				public void OnCollectiveThoughtStart()
				{
						if (mDestroyed) {
								return;
						}

						IItemOfInterest itemOfInterest = character.CurrentThought.CurrentItemOfInterest;

						switch (itemOfInterest.IOIType) {
								case ItemOfInterestType.Player:
										break;

								case ItemOfInterestType.Scenery:
										break;


								case ItemOfInterestType.WorldItem:
										if (itemOfInterest.worlditem.StackName.Contains("Wood Dummy")) {
												character.AttackThing(itemOfInterest);						
										} else if (itemOfInterest.worlditem.Is <Obstruction>()) {
												character.AttackThing(itemOfInterest);
										}
										break;

								default:
										break;
						}
				}
		}
}
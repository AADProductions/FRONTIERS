using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.GUI;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
	public class Shearable : WIScript
	{
		public ShearableState State = new ShearableState();

		public static GenericWorldItem ShearedItem {
			get {
				if (gShearedItem == null) {
					gShearedItem = new GenericWorldItem();
					gShearedItem.PackName = "Textiles";
					gShearedItem.PrefabName = "Wool 1";
				}
				return gShearedItem;
			}
		}

		public int MaxShearedItems = 7;
		public int MinShearedItems = 2;
		public float RTSecondsToGrowBack = 100f;
		Creature creature = null;

		public override void OnInitialized()
		{
			creature = worlditem.Get <Creature>();
		}

		public override void PopulateOptionsList(List <WIListOption> options, List <string> message)
		{
			if (gShearOption == null) {
				gShearOption = new WIListOption("Shear", "Shear");
			}
			//TODO make it impossible to shear a creature more than [x] times per [y]
			if (!creature.IsDead) {
				//don't require shears or anything yet, we'll add that later
				options.Add(gShearOption);
			}
		}

		public void OnPlayerUseWorldItemSecondary(object secondaryResult)
		{

			WIListResult dialogResult = secondaryResult as WIListResult;			
			switch (dialogResult.SecondaryResult) {
				case "Shear":
					State.LastTimeSheared = WorldClock.AdjustedRealTime;
										//spawn a random number of wool items
										//make it pop off the chest body part, upwards
					WorldItem shearedWorldItem = null;
					BodyPart chestBodyPart = null;
					creature.Body.GetBodyPart(BodyPartType.Chest, out chestBodyPart);
					Bounds colliderBounds = chestBodyPart.PartCollider.bounds;
					Vector3 popPosition;
					Vector3 popDirection;
					System.Random random = new System.Random(Profile.Get.CurrentGame.Seed);
					int numShearedItems = random.Next(MinShearedItems, MaxShearedItems);
					Debug.Log("Shearing " + numShearedItems.ToString() + " Items in " + name);
					for (int i = 0; i < numShearedItems; i++) {
						popPosition = (UnityEngine.Random.onUnitSphere * 5f) + chestBodyPart.tr.position;
						popPosition = chestBodyPart.PartCollider.ClosestPointOnBounds(popPosition);
						//give it a push in a direction away from the body
						popDirection = Vector3.Normalize(chestBodyPart.tr.position - popPosition);

						if (WorldItems.CloneWorldItem(ShearedItem, STransform.zero, false, WIGroups.GetCurrent(), out shearedWorldItem)) {
							shearedWorldItem.Initialize();
							shearedWorldItem.SetMode(WIMode.World);
							shearedWorldItem.tr.position = popPosition;
							shearedWorldItem.ApplyForce(popDirection, popPosition);
						}
						//let them all fall away
					}
										//the creature freaks out but doesn't actually take damage
					creature.OnTakeDamage();
					break;

				default:
					break;
			}
		}

		protected static WIListOption gShearOption;
		protected static GenericWorldItem gShearedItem;
	}

	[Serializable]
	public class ShearableState
	{
		public double LastTimeSheared;
	}
}
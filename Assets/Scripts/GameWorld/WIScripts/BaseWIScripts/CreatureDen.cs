using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
	public class CreatureDen : WIScript, ICreatureDen
	{
		public CreatureDenState State = new CreatureDenState();
		public List <Creature> SpawnedCreatures = new List <Creature>();
		public List <Creature> DeadCreatures = new List <Creature>();
		public string SoundOnRemainInDen;
		//public CreatureTemplate StateTemplate = null;
		public LookerBubble SharedLooker = null;

		public IItemOfInterest IOI { get { return worlditem; } }

		public Vector3 Position { 
			get {
				return worlditem.Position;
			}
		}

		public bool AnnounceDenVisit = true;

		public string NameOfCreature {
			get {
				return State.NameOfCreature;
			}
		}

		public void AddCreature(IWIBase creatureBase)
		{
			Creature creature = creatureBase.worlditem.Get <Creature>();
			float distanceFromDen = Vector3.Distance(creature.worlditem.Position, worlditem.Position);
			creature.IsInDen = distanceFromDen < Radius;
			creature.IsInDenInnerRadius = distanceFromDen < InnerRadius;
			creature.Den = this;
			SpawnedCreatures.SafeAdd(creature);
			enabled = true;
		}

		public bool BelongsToPack(WorldItem worlditem)
		{
			Creature creature = null;
			return (worlditem.Is <Creature>(out creature) && String.Equals(creature.State.PackTag, State.PackTag));
		}

		public bool PlayerIsInDen = false;

		public bool TrapsSpawnCorpse  { get { return true; } }

		public bool PlayerIsInInnerRadius {
			get {
				return mPlayerVisitingDenInnerRadius;
			}
		}

		public float Radius {
			get {
				return worlditem.ActiveRadius;
			}
		}

		public float InnerRadius {
			get {
				return GetInnerRadius(Radius);
			}
		}

		public void CallForHelp(WorldItem creatureInNeed, IItemOfInterest sourceOfTrouble)
		{
			if (sourceOfTrouble != null) {
				for (int i = 0; i < SpawnedCreatures.Count; i++) {
					if (SpawnedCreatures[i] != null && SpawnedCreatures[i].worlditem != creatureInNeed) {
						SpawnedCreatures[i].AttackThing(sourceOfTrouble);
					}
				}
			}
		}

		public override bool CanEnterInventory {
			get {
				return State.CanEnterInventory;
			}
		}

		public override bool CanBeCarried {
			get {
				return State.CanBeCarried;
			}
		}

		public override void OnInitialized()
		{

			Spawner spawner = null;
			/*if (worlditem.Is <Spawner>(out spawner)) {
				//get the hostile template for creatures to draw from
				Creatures.GetTemplate (State.NameOfCreature, out StateTemplate);
			}*/

			Location location = null;
			if (worlditem.Is <Location>(out location)) {
				location.UnloadOnInvisible = false;
			}

			WorldClock.Get.TimeActions.Subscribe(TimeActionType.DaytimeStart, new ActionListener(DaytimeStart));
			WorldClock.Get.TimeActions.Subscribe(TimeActionType.NightTimeStart, new ActionListener(NightTimeStart));
			WorldClock.Get.TimeActions.Subscribe(TimeActionType.HourStart, new ActionListener(HourStart));

			//worlditem.OnActive += OnActive;
			//worlditem.OnInactive += OnInactive;
			worlditem.OnVisible += OnVisible;

			Visitable visitable = null;
			if (worlditem.Is <Visitable>(out visitable)) {
				visitable.PlayerOnly = false;
				visitable.OnPlayerVisit += OnPlayerVisit;
				visitable.OnPlayerLeave += OnPlayerLeave;
				visitable.OnItemOfInterestVisit += OnItemOfInterestVisit;
				visitable.OnItemOfInterestLeave += OnItemOfInterestLeave;
				visitable.ItemsOfInterest.Add("Creature");
				visitable.ItemsOfInterest.Add("LandTrap");
				visitable.ItemsOfInterest.Add("WaterTrap");
			}
		}

		public void CreateSharedLooker()
		{
			GameObject sharedLookerObject = gameObject.FindOrCreateChild("SharedLooker").gameObject;
			SharedLooker = sharedLookerObject.GetOrAdd <LookerBubble>();
			SharedLooker.FinishUsing();
		}

		public void SpawnCreatureCorpse(Vector3 spawnPosition, string causeOfDeath, float timeSinceDeath)
		{
			Location location = null;
			if (worlditem.Is <Location>(out location)) {
				gCorpsePosition.Position = location.LocationGroup.tr.InverseTransformPoint(spawnPosition);
				Creature deadCreature = null;
				if (Creatures.SpawnCreature (this, location.LocationGroup, gCorpsePosition.Position, out deadCreature)) {
					deadCreature.State.IsDead = true;
				}
				/*Debug.Log("Spawning creature at position " + spawnPosition.ToString());
				WorldItem corpse = null;
				if (WorldItems.CloneWorldItem ("Corpses", "Bone Pile 1", gCorpsePosition, true, location.LocationGroup, out corpse)) {
					CreatureTemplate t = null;
					Creatures.GetTemplate (State.NameOfCreature, out t);
					corpse.Initialize ();
					FillStackContainer fsc = corpse.Get <FillStackContainer> ();
					fsc.State.WICategoryName = t.Props.InventoryFillCategory;
					if (corpse.Is (WIActiveState.Active)) {
						fsc.TryToFillContainer (true);
					}
				}*/
			}
		}

		public void SpawnCreatureCorpse (Creature deadCreature) {
			SpawnCreatureCorpse (deadCreature.worlditem.Position, string.Empty, 0f);
		}

		public void OnVisible()
		{
			//the spawner will take care of creating any NEW creatures
			//this function will take care of re-spawning any saved creatures
			Location location = null;
			if (worlditem.Is <Location>(out location)) {
				WIGroup group = location.LocationGroup;
				if (!string.IsNullOrEmpty(State.DenStructure.TemplateName)) {
					Structures.AddMinorToload(State.DenStructure, 0, worlditem);
				}
			}
		}

		public void OnPlayerVisit()
		{
			PlayerIsInDen = true;
			if (AnnounceDenVisit) {
				Player.Local.Surroundings.CreatureDenEnter(this);
			}
			if (!mVisitingDen) {
				//this will enable the script
				StartCoroutine(OnPlayerVisitDen());
			}
		}

		public void OnPlayerLeave()
		{
			PlayerIsInDen = false;
			if (AnnounceDenVisit) {
				Player.Local.Surroundings.CreatureDenExit(this);
			}
			if (!mLeavingDen) {
				//this will disable the script
				StartCoroutine(OnPlayerLeaveDen());
			}
		}

		public void OnItemOfInterestVisit()
		{
			//Debug.Log("On item of interest visit");
			Visitable visitable = null;
			if (worlditem.Is <Visitable>(out visitable)) {
				Creature creature = null;
				ITrap trap = null;
				WorldItem lastItemOfInterest = visitable.LastItemOfInterestToVisit;
				LandTrap landTrap = null;
				WaterTrap waterTrap = null;
				if (lastItemOfInterest.Is <LandTrap>(out landTrap) && !landTrap.Exceptions.Contains(NameOfCreature)) {
					//Debug.Log("Found a land trap");
					landTrap.IntersectingDens.SafeAdd(this);
				} else if (lastItemOfInterest.Is <WaterTrap>(out waterTrap) && !waterTrap.Exceptions.Contains(NameOfCreature)) {
					//Debug.Log("Found a water trap");
					waterTrap.IntersectingDens.SafeAdd(this);
				}
			}
		}

		public void OnItemOfInterestLeave()
		{
			Visitable visitable = null;
			if (worlditem.Is <Visitable>(out visitable)) {
				Creature creature = null;
				WorldItem lastItemOfInterest = visitable.LastItemOfInterestToLeave;
				LandTrap landTrap = null;
				WaterTrap waterTrap = null;
				if (lastItemOfInterest.Is <LandTrap>(out landTrap)) {
					landTrap.IntersectingDens.Remove(this);
				} else if (lastItemOfInterest.Is <WaterTrap>(out waterTrap)) {
					waterTrap.IntersectingDens.Remove(this);
				}
			}
		}

		public bool DaytimeStart(double timeStamp)
		{
			if (mDestroyed) {
				return true;
			}

			for (int i = SpawnedCreatures.LastIndex(); i >= 0; i--) {
				if (SpawnedCreatures[i] == null) {
					SpawnedCreatures.RemoveAt(i);
				} else {
					SpawnedCreatures[i].OnDaytimeStart.SafeInvoke();
				}
			}
			return true;
		}

		public bool NightTimeStart(double timeStamp)
		{
			if (mDestroyed) {
				return true;
			}

			for (int i = SpawnedCreatures.LastIndex(); i >= 0; i--) {
				if (SpawnedCreatures[i] == null) {
					SpawnedCreatures.RemoveAt(i);
				} else {
					SpawnedCreatures[i].OnNightTimeStart.SafeInvoke();
				}
			}
			return true;
		}

		public bool HourStart(double timeStamp)
		{
			if (mDestroyed) {
				return true;
			}

			for (int i = SpawnedCreatures.LastIndex(); i >= 0; i--) {
				if (SpawnedCreatures[i] == null) {
					SpawnedCreatures.RemoveAt(i);
				} else {
					SpawnedCreatures[i].OnHourStart.SafeInvoke();
				}
			}
			return true;
		}

		protected int mLookerCounter = 0;
		protected int mDenCounter = 0;
		protected int mPlayerCounter = 0;
		protected bool mCreatureCanBeTrapped = true;
		#if UNITY_EDITOR
		public override void OnEditorRefresh()
		{
			bool foundStructure = false;
			foreach (Transform child in transform) {
				StructureBuilder builder = child.GetComponent <StructureBuilder>();
				if (builder != null) {
					State.DenStructure.TemplateName = StructureBuilder.GetTemplateName(builder.name);
					State.DenStructure.Position = child.localPosition;
					State.DenStructure.Rotation = child.localRotation.eulerAngles;
					foundStructure = true;
					break;
				}
			}
			if (!foundStructure) {
				State.DenStructure.TemplateName = string.Empty;
			}
		}
		#endif
		public void FixedUpdate()
		{
			if (Globals.MissionDevelopmentMode) {
				//we don't care about creatures in mission testing mode
				return;
			}

			if (SpawnedCreatures.Count == 0) {
				enabled = false;
			}
			//TODO look into making this a coroutine
			mLookerCounter++;
			if (mLookerCounter > 2) {
				mLookerCounter = 0;
				if (SpawnedCreatures.Count > 0) {
					Looker looker = null;
					if (SharedLooker == null) {
						CreateSharedLooker();
					}
					if (!SharedLooker.IsInUse) {
						//if the looker is disabled that means it's done being used
						mUpdateCreatureIndex = SpawnedCreatures.NextIndex(mUpdateCreatureIndex);
						if (SpawnedCreatures[mUpdateCreatureIndex] != null && SpawnedCreatures[mUpdateCreatureIndex].worlditem.Is <Looker>(out looker)) {
							//listener is passive but looker is active
							//it needs to be told to look for the player
							//we stagger this because it's an expensive operation
							looker.LookForStuff(SharedLooker);
						}
					}
				}
			}

			mDenCounter++;
			if (mDenCounter > 3) {
				mDenCounter = 0;
				for (int i = 0; i < SpawnedCreatures.Count; i++) {
					if (SpawnedCreatures[i] != null) {
						if (Vector3.Distance(SpawnedCreatures[i].worlditem.tr.position, worlditem.tr.position) > Radius) {
							SpawnedCreatures[i].IsInDen = false;
						} else {
							SpawnedCreatures[i].IsInDen = true;
						}
					}
				}
			}

			mPlayerCounter++;
			if (mPlayerCounter > 4) {
				//only when script is enabled
				//only when player is in den
				if (PlayerIsInDen) {
					//go through each creature in our list
					//tell it to look for the player
					//now check if the player has entered the den inner radius
					float distanceToDen = Vector3.Distance(Player.Local.Position, worlditem.tr.position);
					if (mPlayerVisitingDenInnerRadius) {
						if (distanceToDen > InnerRadius) {
							//the player has exited the inner radius
							//tell each creature
							mPlayerVisitingDenInnerRadius = false;
							for (int i = SpawnedCreatures.LastIndex(); i >= 0; i--) {
								if (SpawnedCreatures[i] == null) {
									SpawnedCreatures.RemoveAt(i);
								} else {
									SpawnedCreatures[i].OnPlayerLeaveDenInnerRadius.SafeInvoke();
								}
							}
						}
					} else {
						if (distanceToDen < InnerRadius) {
							//the player has entered the inner radius
							//tell each creature
							mPlayerVisitingDenInnerRadius = true;
							for (int i = SpawnedCreatures.LastIndex(); i >= 0; i--) {
								if (SpawnedCreatures[i] == null) {
									SpawnedCreatures.RemoveAt(i);
								} else {
									SpawnedCreatures[i].OnPlayerVisitDenInnerRadius.SafeInvoke();
								}
							}
						}
					}
				}
			}
		}

		protected IEnumerator OnPlayerLeaveDen()
		{
			mLeavingDen = true;
			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 1f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			//wait for creatures to finish spawning
			for (int i = SpawnedCreatures.Count - 1; i >= 0; i--) {
				if (SpawnedCreatures[i] == null) {
					SpawnedCreatures.RemoveAt(i);
				} else {
					SpawnedCreatures[i].OnPlayerLeaveDen.SafeInvoke();
				}
				yield return null;
			}
			mLeavingDen = false;
			yield break;
		}

		protected IEnumerator OnPlayerVisitDen()
		{
			mVisitingDen = true;
			double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 1f;
			while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}
			//wait for creatures to finish spawning
			bool playedSound = false;
			for (int i = SpawnedCreatures.Count - 1; i >= 0; i--) {
				if (SpawnedCreatures[i] == null) {
					SpawnedCreatures.RemoveAt(i);
				} else {
					if (!playedSound && !string.IsNullOrEmpty (SoundOnRemainInDen)) {
						MasterAudio.PlaySound (MasterAudio.SoundType.AnimalVoice, SpawnedCreatures [i].worlditem.tr, SoundOnRemainInDen);
					}
					SpawnedCreatures[i].OnPlayerVisitDen.SafeInvoke();
				}
				yield return null;
			}
			mVisitingDen = false;
			yield break;
		}

		public static float GetInnerRadius(float radius)
		{
			return radius * 0.25f;
		}

		protected int mUpdateCreatureIndex = -1;
		protected float mRadius = -1f;
		protected bool mLeavingDen = false;
		protected bool mVisitingDen = false;
		protected bool mPlayerVisitingDenInnerRadius = false;
		protected static STransform gCorpsePosition = new STransform ();
		public Spawner CreatureSpawner;
	}

	[Serializable]
	public class CreatureDenState
	{
		public MinorStructure DenStructure = new MinorStructure();
		public string NameOfCreature = "Rabbit";
		public string PackTag = "Pack";
		public bool CanEnterInventory = false;
		public bool CanBeCarried = false;
	}
}
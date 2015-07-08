using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.WIScripts
{
	public class FishDen : WIScript, ICreatureDen
	{
		public FishDenState State = new FishDenState ();
		public List <Critter> SpawnedFish = new List <Critter> ();
		public bool PlayerIsInDen = false;

		#region ICreatureDen implementation

		public IItemOfInterest IOI { get { return worlditem; } }

		public string NameOfCreature { get { return State.NameOfFish; } }

		public void AddCreature (IWIBase creature)
		{
			return;
		}

		public void SpawnCreatureCorpse (Vector3 position, string causeOfDeath, float timeSinceDeath)
		{
		}

		public void SpawnCreatureCorpse (Creature deadCreature)
		{
		}

		public bool BelongsToPack (WorldItem worlditem)
		{
			return false;
		}

		public bool TrapsSpawnCorpse { get { return false; } }

		public void CallForHelp (WorldItem creatureInNeed, IItemOfInterest sourceOfTrouble)
		{
		}

		public float Radius { get { return worlditem.ActiveRadius; } }

		public float InnerRadius { get { return Radius * 0.25f; } }

		public Vector3 Position { get { return worlditem.Position; } }

		#endregion

		public float WaterLevel = 0f;

		public int NumFishToSpawn { 
			get {
				return Mathf.Max (3, Mathf.CeilToInt (Radius * Globals.FishingHoldNumFishPerRadius));
			}
		}

		public override bool CanEnterInventory {
			get {
				return false;
			}
		}

		public override bool CanBeCarried {
			get {
				return false;
			}
		}

		public override void OnInitialized ()
		{
			Location location = null;
			if (worlditem.Is <Location> (out location)) {
				location.UnloadOnInvisible = false;
			}

			worlditem.OnActive += OnVisible;
			worlditem.OnVisible += OnVisible;
			worlditem.OnInvisible += OnInvisible;
			worlditem.OnAddedToGroup += OnAddedToGroup;

			mSpawningFish = false;
			mDespawningFish = false;

			Visitable visitable = null;
			if (worlditem.Is <Visitable> (out visitable)) {
				visitable.PlayerOnly = false;
				visitable.OnPlayerVisit += OnPlayerVisit;
				visitable.OnPlayerLeave += OnPlayerLeave;
				visitable.OnItemOfInterestVisit += OnItemOfInterestVisit;
				visitable.OnItemOfInterestLeave += OnItemOfInterestLeave;
				visitable.ItemsOfInterest.Add ("Creature");
				visitable.ItemsOfInterest.Add ("WaterTrap");
			}

			Revealable revealable = null;
			if (worlditem.Is <Revealable> (out revealable)) {
				revealable.State.CustomMapSettings = true;
				revealable.State.IconStyle = MapIconStyle.Small;
				revealable.State.LabelStyle = MapLabelStyle.None;
				revealable.State.IconName = "MapIconFishDen";
			}
		}

		public void OnAddedToGroup ()
		{
			OnVisible ();
		}

		public void OnVisible ()
		{
			if (!mSpawningFish && SpawnedFish.Count == 0) {
				//Debug.Log("Visible in fish den");
				//this will enable the script
				StartCoroutine (SpawnFish ());
			}
		}

		public void OnInvisible ()
		{
			if (worlditem.Group == null) {
				return;
			}

			if (!mSpawningFish && !mDespawningFish && SpawnedFish.Count > 0) {
				//Debug.Log("On invisible in fish den");
				//this will disable the script
				StartCoroutine (DespawnFish ());
			}
		}

		public void OnPlayerVisit ()
		{
			PlayerIsInDen = true;
			OnVisible ();
		}

		public void OnPlayerLeave ()
		{
			PlayerIsInDen = false;
		}

		public void OnItemOfInterestVisit ()
		{
			Visitable visitable = null;
			if (worlditem.Is <Visitable> (out visitable)) {
				Creature creature = null;
				ITrap trap = null;
				WorldItem lastItemOfInterest = visitable.LastItemOfInterestToVisit;
				WaterTrap waterTrap = null;
				if (lastItemOfInterest.Is <WaterTrap> (out waterTrap) && !waterTrap.Exceptions.Contains (State.NameOfFish)) {
					Debug.Log ("Found a water trap");
					waterTrap.IntersectingDens.SafeAdd (this);
				}
			}
		}

		public void OnItemOfInterestLeave ()
		{
			Visitable visitable = null;
			if (worlditem.Is <Visitable> (out visitable)) {
				Creature creature = null;
				WorldItem lastItemOfInterest = visitable.LastItemOfInterestToLeave;
				WaterTrap waterTrap = null;
				if (lastItemOfInterest.Is <WaterTrap> (out waterTrap)) {
					waterTrap.IntersectingDens.Remove (this);
				}
			}
		}

		protected int mPlayerCounter = 0;

		public void FixedUpdate ()
		{
			if (SpawnedFish.Count == 0) {
				enabled = false;
			}

			if (State.Type == FishDenType.Ocean) {
				//make sure we're at ocean-level
				WaterLevel = Biomes.Get.TideWaterElevation;
			} else {
				WaterLevel = Position.y;
			}

			mPlayerPosition = Player.Local.Position;
			mCritterPosition.y = WaterLevel;
			mUpdatePositions++;
			if (mUpdatePositions > 3) {
				mUpdatePositions = 0;
				for (int i = SpawnedFish.LastIndex (); i >= 0; i--) {
					//make sure the fish are staying in the den
					if (SpawnedFish [i] == null || SpawnedFish [i].Destroyed) {
						SpawnedFish.RemoveAt (i);
					} else {
						SpawnedFish [i].TargetHeight = WaterLevel;
						SpawnedFish [i].UpdateMovement (mPlayerPosition);
					}
				}
			}

			mPlayerCounter++;
			if (mPlayerCounter > 30) {
				//only when script is enabled
				//only when player is in den
				if (PlayerIsInDen) {
					//update tell fish to run away from player
				}
			}
		}
		#if UNITY_EDITOR
		public GameObject WaterSource;

		public override void OnEditorRefresh ()
		{
			Location location = gameObject.GetComponent <Location> ();
			location.State.Type = "FishingHole";
		}
		#endif
		protected IEnumerator SpawnFish ()
		{
			mSpawningFish = true;
			Vector3 critterPosition = Position;
			for (int i = 0; i < NumFishToSpawn; i++) {
				Critter critter = null;
				critterPosition = Position;
				critterPosition.x = critterPosition.x + UnityEngine.Random.Range (-Radius, Radius);
				critterPosition.z = critterPosition.z + UnityEngine.Random.Range (-Radius, Radius);
				if (Critters.Get.SpawnCritter (State.NameOfFish, critterPosition, out critter)) {
					//make sure the fish have to stay on the same plane
					critter.rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
					critter.UsePlayerTargetHeight = false;
					critter.TargetHeight = WaterLevel;
					critter.Den = this;
					SpawnedFish.Add (critter);
				}
				yield return null;
			}
			enabled = true;
			mSpawningFish = false;
			yield break;
		}

		protected IEnumerator DespawnFish ()
		{
			mDespawningFish = true;
			for (int i = SpawnedFish.LastIndex (); i > 0; i--) {
				//Debug.Log ("Despawning fish");
				GameObject.Destroy (SpawnedFish [i].gameObject);
				SpawnedFish.RemoveAt (i);
				yield return null;
			}
			mDespawningFish = false;
			yield break;
		}

		protected Vector3 mPlayerPosition;
		protected Vector3 mCritterPosition;
		protected int mUpdateCreatureIndex = -1;
		protected int mUpdatePositions = 0;
		protected float mRadius = -1f;
		protected bool mDespawningFish = false;
		protected bool mSpawningFish = false;

		public enum FishDenType
		{
			Ocean,
			River,
			BodyOfWater,
			Tank
		}
	}

	[Serializable]
	public class FishDenState
	{
		public FishDen.FishDenType Type = FishDen.FishDenType.Ocean;
		public string WaterSourceName = string.Empty;
		public string NameOfFish = "Fish";
		public string PackTag = "Pack";
	}
}
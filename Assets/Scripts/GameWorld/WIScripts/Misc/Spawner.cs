using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class Spawner : WIScript
	{
		//general purpose spawner script
		//this is used whenever we need random things spawned on a regular basis
		public SpawnerState State = new SpawnerState ();

		public override void OnInitialized ()
		{
			worlditem.OnVisible += OnVisible;
		}

		public virtual void OnActive ()
		{
			if (mIsSpawning) {
				return;
			}
			mIsSpawning = true;
			StartCoroutine (SpawnItemsOverTime ());
		}

		public virtual void OnVisible ()
		{
			if (mIsSpawning) {
				return;
			}
			mIsSpawning = true;
			StartCoroutine (SpawnItemsOverTime ());
		}

		public IEnumerator SpawnItemsOverTime ()
		{
			System.Random random = new System.Random (Profile.Get.CurrentGame.Seed + worlditem.GetHashCode ());

			Location location = null;
			if (!worlditem.Is <Location> (out location)) {
				mIsSpawning = false;
				yield break;
			}

			while (!Player.Local.HasSpawned || location.LocationGroup == null || !location.LocationGroup.Is (WIGroupLoadState.Loaded)) {
				yield return null;
				if (worlditem.Is (WIActiveState.Invisible) || !worlditem.Is (WILoadState.Initialized)) {
					//Debug.Log ("We went invisible before we could spawn in " + name);
					mIsSpawning = false;
					yield break;
				}
			}

			WIGroup spawnGroup = location.LocationGroup;
			for (int i = 0; i < State.SpawnerSettings.Count; i++) {
				SpawnerStateSetting setting = State.SpawnerSettings [i];
				if (setting.TimeToSpawn) {
					setting.LastManualSpawnPointIndex = 0;//reset this just in case
					var enumerator = GetSpawnPoints (setting, location, spawnGroup, Player.Local, random).GetEnumerator ();
					while (enumerator.MoveNext ()) {
						if (!spawnGroup.Is (WIGroupLoadState.Initialized | WIGroupLoadState.Loading | WIGroupLoadState.Loaded)) {
							//whoops, it unloaded
							yield break;
						}
						SpawnPoint spawnPoint = enumerator.Current;
						//wait a tick after fetching the spawn point
						yield return null;
						if (spawnPoint != SpawnPoint.Empty) {
							var spawn = Spawn (spawnPoint, setting, location, spawnGroup, random);
							while (spawn.MoveNext ()) {
								yield return spawn.Current;
							}
						}
						double waitUntil = WorldClock.RealTime + Globals.SpawnerRTYieldInterval + UnityEngine.Random.value * 0.25f;
						while (WorldClock.RealTime < waitUntil) {
							yield return null;
						}
					}
					int gameHoursToNextSpawnTime = random.Next (setting.MinHoursBetweenSpawns, setting.MaxHoursBetweenSpawns);
					setting.NextSpawnTime = WorldClock.AdjustedRealTime + WorldClock.HoursToSeconds (gameHoursToNextSpawnTime);
				}
				yield return null;
			}

			mIsSpawning = false;
			yield break;
		}

		#region static functions

		public IEnumerable <SpawnPoint> GetSpawnPoints (SpawnerStateSetting setting, Location location, WIGroup spawnGroup, PlayerBase player, System.Random random)
		{
			//figure out how many we need
			setting.NumAttempts = 0;
			setting.NumFailedAttempts = 0;

			int numExistingObjects = spawnGroup.NumChildItemsByCategory (setting.CategoryName, setting.MaxSpawnedObjects);//TODO make sure this works
			int numObjectsToSpawn = random.Next (setting.MinSpawnedObjects, setting.MaxSpawnedObjects) - numExistingObjects;
			if (numObjectsToSpawn <= 0) {
				//we don't need any more right now
				#if UNITY_EDITOR
				Debug.Log ("Skipping spawning in " + name + " because num objects to spawn is <= 0");
				#endif
				yield break;
			}
			bool succeededThisAttempt = false;

			switch (setting.Type) {
			case SpawnerType.Characters:
										//when we're spawning characters, we need action nodes
				WorldChunk chunk = spawnGroup.GetParentChunk ();
				List <ActionNodeState> nodeStates = null;
				if (chunk.GetNodesForLocation (location.worlditem.StaticReference.FullPath, out nodeStates)) {
					for (int i = 0; i < nodeStates.Count; i++) {
						mNextSpawnPoint.nodeState = nodeStates [i];
						yield return mNextSpawnPoint;
					}
				} else {
					nodeStates = new List<ActionNodeState> ();
					Debug.Log ("Didn't find spawn points at location, so we're going to create some");
					//if we can't find any nodes for this location
					//that means we've never spawned here before
					//we need to create some nodes
					for (int i = 0; i < numObjectsToSpawn; i++) {
						//are we totally done?
						if (setting.NumFailedAttempts >= setting.MaxFailedAttempts) {
							yield break;
						}
						//if not, try to get another spawn point
						if (spawnGroup.Props.Interior) {
							succeededThisAttempt = GetInteriorSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
						} else if (spawnGroup.Props.TerrainType == LocationTerrainType.AboveGround) {
							succeededThisAttempt = GetAboveGroundSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
						} else {
							succeededThisAttempt = GetBelowGroundSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
						}
						//did we do it?
						if (succeededThisAttempt) {
							//hooray we created a new action node
							setting.NumAttempts++;
							//turn this spawn point into an action node
							ActionNodeState nodeState = new ActionNodeState ();
							nodeState.Name = location.worlditem.FileName + "-Spawn-" + nodeStates.Count.ToString ();
							nodeState.Type = ActionNodeType.Generic;
							nodeState.Users = ActionNodeUsers.AnyOccupant;
							nodeState.Transform.CopyFrom (mNextSpawnPoint.Transform);
							mNextSpawnPoint.nodeState = nodeState;
							ActionNode node = chunk.SpawnActionNode (spawnGroup, nodeState, location.worlditem.tr);
							yield return mNextSpawnPoint;
						} else {
							//boo, we failed
							setting.NumFailedAttempts++;
						}
					}

				}
				nodeStates.Clear ();
				nodeStates = null;
				break;

			default:
										//when we're spawning other things, we just need spawn points
										//ok, try to spawn this stuff
				for (int i = 0; i < numObjectsToSpawn; i++) {
					//are we totally done?
					if (setting.NumFailedAttempts >= setting.MaxFailedAttempts) {
						yield break;
					}
					//if not, try to get another spawn point
					if (spawnGroup.Props.Interior) {
						succeededThisAttempt = GetInteriorSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
					} else if (spawnGroup.Props.TerrainType == LocationTerrainType.AboveGround) {
						succeededThisAttempt = GetAboveGroundSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
					} else {
						succeededThisAttempt = GetBelowGroundSpawnPoint (ref mNextSpawnPoint, setting, location, spawnGroup);
					}
					//did we do it?
					if (succeededThisAttempt) {
						//hooray, we got another spawn point
						setting.NumAttempts++;
						yield return mNextSpawnPoint;
					} else {
						//boo, we failed
						setting.NumFailedAttempts++;
						yield return SpawnPoint.Empty;
					}
				}
				break;
			}
			yield break;
		}

		protected SpawnPoint mNextSpawnPoint = new SpawnPoint ();

		public static bool GetInteriorSpawnPoint (ref SpawnPoint spawnPoint, SpawnerStateSetting setting, Location spawnLocation, WIGroup spawnGroup)
		{
			return true;
		}

		public static bool GetAboveGroundSpawnPoint (ref SpawnPoint spawnPoint, SpawnerStateSetting setting, Location spawnLocation, WIGroup spawnGroup)
		{
			bool succeeded = false;
			spawnPoint = new SpawnPoint ();
			switch (setting.Method) {
			case SpawnerPlacementMethod.SherePoint:
										//cast a ray from the center of the location outwards
				Vector3 starget = (UnityEngine.Random.onUnitSphere * spawnLocation.worlditem.ActiveRadius) + spawnLocation.worlditem.tr.position;
				Vector3 sscale = Vector3.one;
				RaycastHit hit;
				if (Physics.Linecast (spawnLocation.worlditem.tr.position, starget, out hit, Globals.LayersTerrain)) {
					spawnPoint.Transform.Position = spawnLocation.worlditem.tr.InverseTransformPoint (hit.point);
					spawnPoint.Transform.Rotation = Quaternion.FromToRotation (Vector3.up, hit.normal).eulerAngles;//random forward vector, normal up vector
					spawnPoint.Transform.Scale = sscale;
					switch (hit.collider.gameObject.layer) {
					case Globals.LayerNumFluidTerrain:
						spawnPoint.HitWater = true;
						break;

					case Globals.LayerNumSolidTerrain:
					default:
						spawnPoint.HitTerrainMesh = true;
						break;
					}
					succeeded = true;
				}
				break;

			case SpawnerPlacementMethod.SpawnPoint:
				STransform manualPoint = null;
				if (!setting.NextManualSpawnPoint (out manualPoint)) {
					//whoops, we're out
					return false;
				}
										//no need to do any searching
				spawnPoint.Transform.CopyFrom (manualPoint);
				spawnPoint.HitWater = false;
				spawnPoint.HitTerrainMesh = false;
				succeeded = true;
				break;

			case SpawnerPlacementMethod.TopDown:
			default:
										//get a random point in sphere scaled to the location's radius
				mTerrainHit.groundedHeight = spawnLocation.worlditem.ActiveRadius;
				mTerrainHit.feetPosition = (UnityEngine.Random.insideUnitSphere * mTerrainHit.groundedHeight) + spawnLocation.worlditem.tr.position;
				mTerrainHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mTerrainHit);
				if (mTerrainHit.hitTerrain || mTerrainHit.hitTerrainMesh && !mTerrainHit.hitStructureMesh) {
					spawnPoint.Transform.Position = spawnLocation.worlditem.tr.InverseTransformPoint (mTerrainHit.feetPosition);
					spawnPoint.Transform.Rotation = Quaternion.LookRotation (SVector3.Random (-1f, 1f), mTerrainHit.normal).eulerAngles;
					spawnPoint.Transform.Scale = Vector3.one;
					spawnPoint.HitWater = mTerrainHit.hitWater;
					spawnPoint.HitTerrainMesh = mTerrainHit.hitTerrainMesh;
					succeeded = true;
				} else {
					succeeded = false;
				}
				break;
			}
			return succeeded;
		}

		protected static GameWorld.TerrainHeightSearch mTerrainHit;

		public static bool GetBelowGroundSpawnPoint (ref SpawnPoint spawnPoint, SpawnerStateSetting setting, Location spawnLocation, WIGroup spawnGroup)
		{
			bool succeeded = false;
			spawnPoint = new SpawnPoint ();
			switch (setting.Method) {
			case SpawnerPlacementMethod.SherePoint:
										//cast a ray from the center of the location outwards
				Vector3 starget = (UnityEngine.Random.onUnitSphere * spawnLocation.worlditem.ActiveRadius) + spawnLocation.worlditem.tr.position;
				Vector3 sscale = Vector3.one;
				RaycastHit hit;
				if (Physics.Linecast (spawnLocation.worlditem.tr.position, starget, out hit, Globals.LayersTerrain)) {
					spawnPoint.Transform.Position = spawnLocation.worlditem.tr.InverseTransformPoint (hit.point);
					spawnPoint.Transform.Rotation = Quaternion.FromToRotation (Vector3.up, hit.normal).eulerAngles;//random forward vector, normal up vector
					spawnPoint.Transform.Scale = sscale;
					switch (hit.collider.gameObject.layer) {
					case Globals.LayerNumFluidTerrain:
						spawnPoint.HitWater = true;
						break;

					case Globals.LayerNumSolidTerrain:
					default:
						spawnPoint.HitTerrainMesh = true;
						break;
					}
					succeeded = true;
				}
				break;

			case SpawnerPlacementMethod.TopDown:
			default:
										//get a random point in sphere scaled to the location's radius
				mTerrainHit.groundedHeight = spawnLocation.worlditem.ActiveRadius;
				mRandomSphere = UnityEngine.Random.onUnitSphere * mTerrainHit.groundedHeight;
				mRandomSphere.y = 0f;
				mTerrainHit.feetPosition = mRandomSphere + spawnLocation.worlditem.tr.position;
										//get the terrain height in world space, then store it in local space
										//WorldChunk chunk = spawnLocation.worlditem.Group.GetParentChunk ();
				mTerrainHit.feetPosition.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mTerrainHit);
				spawnPoint.Transform.Position = spawnLocation.worlditem.tr.InverseTransformPoint (mTerrainHit.feetPosition);
				spawnPoint.Transform.Rotation = Quaternion.LookRotation (SVector3.Random (-1f, 1f), mTerrainHit.normal).eulerAngles;
				spawnPoint.Transform.Scale = Vector3.one;
				spawnPoint.HitWater = mTerrainHit.hitWater;
				spawnPoint.HitTerrainMesh = mTerrainHit.hitTerrainMesh;
				if (!mTerrainHit.isGrounded) {
					Debug.Log ("TERRAIN HIT WAS NOT GROUNDED");
				}
				succeeded = true;
				break;
			}
			return succeeded;
		}

		protected static Vector3 mRandomSphere;

		public static IEnumerator Spawn (SpawnPoint spawnPoint, SpawnerStateSetting setting, Location location, WIGroup spawnGroup, System.Random random)
		{
			switch (setting.Type) {
			case SpawnerType.WorldItems:
			default:
				WorldItem newSpawnedItem = null;
				WICategory cat = null;
				if (WorldItems.Get.Category (setting.CategoryName, out cat)) {
					spawnPoint.Transform.Position.y += setting.UpVectorPadding;
					if (WorldItems.CloneRandomFromCategory (cat, spawnGroup, spawnPoint.Transform, setting.Flags, setting.NumTimesSpawned, setting.NumAttempts, out newSpawnedItem)) {
						//adjust spawn point
						spawnPoint.Transform.Rotation = spawnPoint.Transform.Rotation + newSpawnedItem.Props.Global.BaseRotation;
						spawnPoint.Transform.Position.y += setting.UpVectorPadding;
						//apply to newly spawned item
						newSpawnedItem.Props.Local.Transform.CopyFrom (spawnPoint.Transform);
						if (setting.DropThenFreeze) {
							newSpawnedItem.Props.Local.FreezeOnStartup = false;
							newSpawnedItem.Props.Local.FreezeOnSleep = true;
							newSpawnedItem.gameObject.AddComponent <FreezeOnSleep> ().worlditem = newSpawnedItem;
						} else {
							yield return null;
						}
						if (!string.IsNullOrEmpty (setting.SendMessageToSpawnedItem)) {
							//Debug.Log ("Sending message to item: " + setting.SendMessageToSpawnedItem);
							newSpawnedItem.SendMessage (setting.SendMessageToSpawnedItem, SendMessageOptions.DontRequireReceiver);
						}
					} else {
						//Debug.Log ("Couldn't clone from category " + setting.CategoryName);
					}
				} else {
					//Debug.Log ("Couldn't get category " + setting.CategoryName);
				}
				break;

			case SpawnerType.Characters:
										//TEMP just use bandit camp since that's the only thing that uses spawners for characters
				BanditCamp camp = null;
				if (location.worlditem.Is <BanditCamp> (out camp)) {
					Character character = null;
					//if we've gotten this far we must have found or created a node, so assume it's there
					if (Characters.SpawnRandomCharacter (spawnPoint.nodeState.actionNode, camp.State.TemplateName, setting.Flags, location.LocationGroup, out character)) {
						camp.AddBandit (character);
					}
				}
				break;


			case SpawnerType.Creatures:
				spawnPoint.Transform.Position.y += setting.UpVectorPadding;
				CreatureDen den = null;
				if (location.worlditem.Is <CreatureDen> (out den)) {
					Creature creature = null;
					Creatures.SpawnCreature (den, spawnGroup, spawnPoint.Transform.Position, out creature);
				}
				break;
			}
			yield break;
		}

		public static bool TrySpawnPoint (SpawnerStateSetting setting, SpawnPoint spawnPoint, Location spawnLocation)
		{
			return true;
		}

		#endregion

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			foreach (SpawnerStateSetting setting in State.SpawnerSettings) {
				if (setting.Method == SpawnerPlacementMethod.SpawnPoint && setting.ManualSpawnPoints.Count == 0) {
					foreach (Transform child in transform) {
						if (child.name == "SpawnPoint") {
							setting.ManualSpawnPoints.Add (new STransform (child));
						}
					}
				}
			}
		}
		#endif
		protected bool mIsSpawning = false;
	}

	[Serializable]
	public class SpawnPoint
	{
		public static SpawnPoint Empty {
			get {
				return gEmpty;
			}
		}

		public SpawnPoint ()
		{

		}

		public SpawnPoint (ActionNodeState actionNodeState)
		{
			nodeState = actionNodeState;
		}

		public STransform Transform = STransform.zero;
		public bool HitWater = false;
		public bool HitTerrainMesh = false;
		[NonSerialized]
		[XmlIgnore]
		public ActionNodeState nodeState = null;
		protected static SpawnPoint gEmpty = new SpawnPoint ();
	}

	[Serializable]
	public class SpawnerStateSetting
	{
		public bool TimeToSpawn {
			get {
				bool canSpawn = true;
				switch (Availability) {
				case SpawnerAvailability.Always:
				default:
					break;

				case SpawnerAvailability.Max:
					canSpawn = NumTimesSpawned < MaxTimesSpawned;
					break;

				case SpawnerAvailability.Once:
					canSpawn = NumTimesSpawned < 1;
					break;
				}
				return canSpawn && (WorldClock.AdjustedRealTime >= NextSpawnTime);
			}
		}

		public bool NextManualSpawnPoint (out STransform manualPoint)
		{
			manualPoint = null;
			if (ManualSpawnPoints.Count > 0) {
				bool looped = false;
				LastManualSpawnPointIndex = ManualSpawnPoints.NextIndex <STransform> (0, LastManualSpawnPointIndex, out looped);
				if (!looped) {
					manualPoint = ManualSpawnPoints [LastManualSpawnPointIndex];
					return true;
				}
			}
			return false;
		}

		public int LastManualSpawnPointIndex = 0;
		public List <STransform> ManualSpawnPoints = new List<STransform> ();
		public SpawnerType Type = SpawnerType.WorldItems;
		public SpawnerPlacementMethod Method = SpawnerPlacementMethod.TopDown;
		public SpawnerAvailability Availability = SpawnerAvailability.Always;
		[FrontiersAvailableModsAttribute ("Category")]
		public string CategoryName = string.Empty;
		public bool UseNormal = false;
		public bool DropThenFreeze = false;
		public int NumTimesSpawned = 0;
		public int MaxTimesSpawned = 0;
		public float UpVectorPadding = 0f;
		public int MinSpawnedObjects = 0;
		public int MaxSpawnedObjects = 10;
		public int MidSpawnedObjects = 8;
		public int NumFailedAttempts = 0;
		public int MaxFailedAttempts = 10;
		public int NumAttempts = 0;
		public float MaxDistanceFromUnityTerrain = 5.0f;
		public float MinDistanceBetweenSpawns = 10.0f;
		public float MinDistanceFromPlayer = 10.0f;
		public int MinHoursBetweenSpawns = 1;
		public int MaxHoursBetweenSpawns = 1000;
		public double NextSpawnTime = 0.0f;
		public WIFlags Flags = WIFlags.All;
		public TerrainMapSpawnData TerrainMapAboveGround = new TerrainMapSpawnData ();
		public TerrainMapSpawnData TerrainMapBelowGround = new TerrainMapSpawnData ();
		public TerrainElevationSpawnData TerrainElevation = new TerrainElevationSpawnData ();
		public string SendMessageToSpawnedItem = string.Empty;

		public override int GetHashCode ()
		{		//used as a compliment to the game seed to produce random results
			//we generate a positive hash code to keep things simple
			if (mHashCode < 0) {
				mHashCode = Mathf.Abs (Flags.GetHashCode () + (int)Type + (int)Method + (int)Availability);
			}
			return mHashCode;
		}

		[NonSerialized]
		[XmlIgnore]
		protected int mHashCode = -1;
	}

	[Serializable]
	public class SpawnerState
	{
		public List <SpawnerStateSetting> SpawnerSettings = new List <SpawnerStateSetting> ();
	}

	[Serializable]
	public class TerrainMapSpawnData
	{
		public bool	Use	= true;
		public SColor Max	= Color.white;
		public SColor Min	= Color.black;

		public bool CompatibleWith (Color terrainMapColor)
		{
			return (!Use || (InRange (terrainMapColor.a, Min.a, Max.a)
			&&	InRange (terrainMapColor.r, Min.r, Max.r)
			&&	InRange (terrainMapColor.g, Min.g, Max.g)
			&&	InRange (terrainMapColor.b, Min.b, Max.b)));
		}

		protected static bool InRange (float channel, float min, float max)
		{
			return (channel > min && channel < max);
		}
	}

	[Serializable]
	public class TerrainElevationSpawnData
	{
		public bool	Use	= true;
		public float Min = 0.0f;
		public float Max = 1000.0f;

		public bool	CompatibleWith (float terrainElevation)
		{
			return (!Use || (terrainElevation < Max && terrainElevation > Min));
		}
	}
}
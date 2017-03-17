using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using ExtensionMethods;
using Frontiers.World.WIScripts;

namespace Frontiers.World
{
	[ExecuteInEditMode]
	public class Creatures : Manager
	{
		public GameObject DarkrotNodePrefab;
		public bool SpawnDarkrot = true;
		public List <DarkrotNode> DarkrotNodes = new List <DarkrotNode> ();
		public List <string> DarkrotAudioClips = new List<string> {
			"DarkrotRoam1",
			"DarkrotRoam2",
			"DarkrotRoam3",
			"DarkrotRoam4",
			"DarkrotRoam5",
			"DarkrotRoam6"
		};
		public static Creatures Get;
		public static bool CreatureShadows;

		public void RefreshCreatureShadowSettings (bool objectShadows)
		{
			if (CreatureShadows != objectShadows) {
				CreatureShadows = objectShadows;
				for (int i = 0; i < SpawnedCreatures.Count; i++) {
					if (SpawnedCreatures [i] != null && SpawnedCreatures [i].Body != null) {
						SpawnedCreatures [i].Body.RefreshShadowCasters ();
					}
				}
			}
		}

		public List <Texture2D> DarkrotFlowTextures = new List <Texture2D> ();
		public int DarkrotFlowTextureIndex = 0;
		public Material DarkrotFlowMaterial;
		[FrontiersAvailableModsAttribute ("Category")]
		public string DefaultInventoryFillCategory;

		public override void WakeUp ()
		{
			base.WakeUp ();

			Get = this;

			gDarkrotSpawner = new GenericWorldItem ();
			gDarkrotSpawner.PackName = "Oblox";
			gDarkrotSpawner.PrefabName = "DarkrotSpawner";

			mTemplateLookup = new Dictionary <string, CreatureTemplate> ();
			mBodyLookup = new Dictionary <string, CreatureBody> ();
		}

		public static GenericWorldItem gDarkrotSpawner;
		// = new GenericWorldItem ();
		public void CreateDarkrotSpawner (Vector3 spawnerPosition, Vector3 spawnerRotation, WIGroup group, int numDarkrotReleaesd, float releaseDelay, float releaseInterval)
		{
			StackItem spawnerStackItem = gDarkrotSpawner.ToStackItem ();
			spawnerStackItem.Transform.Position = spawnerPosition;
			spawnerStackItem.Transform.Rotation = spawnerRotation;
			spawnerStackItem.Props.Local.FreezeOnStartup = true;
			spawnerStackItem.Props.Local.Mode = WIMode.Frozen;
			WorldItem newSpawner = null;
			if (WorldItems.CloneFromStackItem (spawnerStackItem, group, out newSpawner)) {
				newSpawner.Initialize ();
				DarkrotSpawner ds = newSpawner.Get <DarkrotSpawner> ();
				ds.State.MaxDarkrotAtOneTime = numDarkrotReleaesd;
				ds.State.SpawnDelay = releaseDelay;
				ds.State.SpawnInterval = releaseInterval;
			}
			Debug.Log ("Created spawner");
		}

		public static Creature SpawnCreatureFromStackItem (StackItem stackItem, WIGroup group)
		{
			return null;
		}

		public static Creature SpawnRandomCreature (CreatureFlags flags, ActionNode node, WIGroup group)
		{
			Creature newCreature = null;
			return newCreature;
		}

		public static bool GetCutsceneClip (string clipName, out AnimationClip clip)
		{
			clip = null;
			for (int i = 0; i < Get.CreatureCutsceneAnimations.Count; i++) {
				if (Get.CreatureCutsceneAnimations [i].name == clipName) {
					clip = Get.CreatureCutsceneAnimations [i];
					break;
				}
			}
			return clip != null;
		}

		public static bool GetBody (string bodyName, out CreatureBody body)
		{
			return mBodyLookup.TryGetValue (bodyName, out body);
		}

		public static bool GetTemplate (string templateName, out CreatureTemplate template)
		{
			return mTemplateLookup.TryGetValue (templateName.Trim ().ToLower (), out template);
		}

		public override void OnGameStart ()
		{
			WorldClock.Get.TimeActions.Subscribe (TimeActionType.NightTimeStart, new ActionListener (NightTimeStart));

			if (GameManager.Get.TestingEnvironment) {
				return;
			}

			for (int i = 0; i < 100; i++) {
				mDarkrotSizes.Add (BiasedRandomNumber.GetBiasedRandomNumber (Globals.DarkrotMinAmount, Globals.DarkrotMaxAmount, Globals.DarkrotAvgAmount));
			}

			if (WorldClock.IsNight) {
				NightTimeStart (WorldClock.AdjustedRealTime);
			}
		}

		public override void OnModsLoadFinish ()
		{
			LoadCreatureTemplates ();
			mTemplateLookup.Clear ();
			for (int i = 0; i < CreatureTemplates.Count; i++) {
				mTemplateLookup.Add (CreatureTemplates [i].Name.ToLower ().Trim (), CreatureTemplates [i]);
			}

			for (int i = 0; i < CreatureBodies.Count; i++) {
				mBodyLookup.Add (CreatureBodies [i].name, CreatureBodies [i]);
			}

			mModsLoaded = true;
		}

		public static bool SpawnCreature (CreatureDen den, WIGroup group, Vector3 spawnPosition, out Creature newCreature)
		{
			return SpawnCreature (den, group, spawnPosition, false, string.Empty, 0f, out newCreature);
		}

		public static bool SpawnCreature (CreatureDen den, WIGroup group, Vector3 spawnPosition, bool isDead, string causeOfDeath, float timeSinceDeath, out Creature newCreature)
		{
			if (Globals.MissionDevelopmentMode) {
				//we don't care about creatures in mission dev mode
				newCreature = null;
				return false;
			}

			newCreature = null;
			CreatureTemplate template = null;
			if (mTemplateLookup.TryGetValue (den.State.NameOfCreature.ToLower ().Trim (), out template)) {
				WorldItem newCreatureWorldItem = null;
				if (WorldItems.CloneFromPrefab (Get.CreatureBase.GetComponent <WorldItem> (), group, out newCreatureWorldItem)) {
					//since this is the first time the creature is spawned
					//it has no idea what it is
					//so before we send it back we're going to set its template name
					newCreature = newCreatureWorldItem.gameObject.GetOrAdd <Creature> ();
					newCreature.State.TemplateName = template.Name;
					newCreature.Template = template;
					if (isDead) {
						Debug.Log ("Spawning dead creature");
						newCreature.State.IsDead = true;
						Damageable damageable = newCreature.GetComponent <Damageable> ();
						damageable.State.CauseOfDeath = causeOfDeath;
						damageable.State.DamageTaken = damageable.State.Durability;
						damageable.State.TimeKilled = WorldClock.AdjustedRealTime - timeSinceDeath;
					} else {
						newCreature.State.IsDead = false;
					}
					newCreature.Den = den;
					newCreatureWorldItem.Props.Local.Transform.Position = spawnPosition;
					newCreatureWorldItem.Props.Local.Transform.Rotation.y = UnityEngine.Random.Range (0f, 360f);
					//Debug.Log ("Setting position of creature to " + spawnPosition.ToString ());
				}
			}
			//and that's it! the rest will be taken care of by the creature
			return newCreature != null;
		}

		public void LoadCreatureTemplates ()
		{
			CreatureTemplates.Clear ();
			Mods.Get.Runtime.LoadAvailableMods (CreatureTemplates, "Creature");
			for (int i = 0; i < CreatureTemplates.Count; i++) {
				CreatureTemplate template = CreatureTemplates [i];
				//set the name of the template in the template props
				template.StateTemplate.TemplateName = template.Name;
			}
		}

		public bool NightTimeStart (double timeStamp)
		{
			return true;
		}

		protected int mUpdateDarkrot = 0;

		public void Update ()
		{
			if (Input.GetKeyDown (KeyCode.Y)) {
				CreatureDen creatureDen = null;
				WIGroup group = null;
				if (Player.Local.Surroundings.IsVisitingLocation) {
					foreach (Location location in Player.Local.Surroundings.VisitingLocations) {
						if (location.worlditem.Is<CreatureDen> (out creatureDen)) {
							group = location.LocationGroup;
							break;
						}
					}
				}
				if (creatureDen != null) {
					Creature creature = null;
					Debug.Log ("Spawning dead creature in den " + creatureDen.name);
					SpawnCreature (creatureDen, group, Vector3.up * 3, true, "Nothing", 0f, out creature);
					creature.worlditem.tr.parent = group.tr;
					creature.worlditem.tr.localPosition = Vector3.up * 3;
				}
			}
		}

		public void FixedUpdate ()
		{
			mUpdateDarkrot++;
			if (mUpdateDarkrot > 4) {
				mUpdateDarkrot = 0;
				if (GameManager.Is (FGameState.InGame)) {
					//update exisiting darkrot nodes
					for (int i = DarkrotNodes.LastIndex (); i >= 0; i--) {
						DarkrotNode node = DarkrotNodes [i];
						if (node == null) {
							DarkrotNodes.RemoveAt (i);
						} else if (node.IsDispersed) {
							node.Destroy ();
							DarkrotNodes.RemoveAt (i);
						} else if (!node.Dispersing && node.IsTimeToMove) {
							//move the darkrot node towards the player
							Vector3 newPosition = Vector3.MoveTowards (
								                             node.mTr.position, 
								                             Player.Local.Position, 
								                             Mathf.Min (Vector3.Distance (node.mTr.position, Player.Local.Position), Globals.DarkrotMaxSpeed));

							node.Move (newPosition);
							//there's a random chance it'll emit a sound
							MasterAudio.PlaySound (MasterAudio.SoundType.Darkrot, node.mTr, DarkrotAudioClips [UnityEngine.Random.Range (0, DarkrotAudioClips.Count)]);
						}
					}
					//see if we're supposed to create new nodes

					if (SpawnDarkrot && !GameWorld.Get.CurrentBiome.OuterSpace && (WorldClock.IsNight)) {
						if ((!Player.Local.Surroundings.IsInCivilization || Player.Local.Surroundings.IsUnderground) && DarkrotNodes.Count < Globals.DarkrotMaxNodes) {//darkrot only spawns in the wild regardless of difficulty
							if (UnityEngine.Random.value < Globals.DarkrotBaseSpawnProbability) {
								mRandomPointAroundPlayer = Vector3.MoveTowards (Player.Local.Position, Player.Local.FocusVector.ToXZ (), Globals.DarkrotSpawnDistance);
								//if we're underground we'll use different methods
								//mTerrainType = GameWorld.Get.TerrainTypeAtInGamePosition(mRandomPointAroundPlayer, Player.Local.Surroundings.IsUnderground);
								//is this civilized area? check the terrain type
								bool spawnDarkrot = true;
								if (Globals.DarkrotSpawnsOnlyInForests) {
									mTerrainType = GameWorld.Get.TerrainTypeAtInGamePosition (mRandomPointAroundPlayer, Player.Local.Surroundings.IsUnderground);
									if (mTerrainType.g <= 0) {
										spawnDarkrot = false;
									}
								}
								if (spawnDarkrot) {
									//if it's not civilized and we're in a forest / don't need to be in a forest
									mTerrainHit.feetPosition = mRandomPointAroundPlayer;
									mTerrainHit.groundedHeight = 5f;
									mRandomPointAroundPlayer.y = GameWorld.Get.TerrainHeightAtInGamePosition (ref mTerrainHit);//don't bother with terrain meshes
									//adjust the y position to the terrain
									//spawn the node
									SpawnDarkrotNode (mRandomPointAroundPlayer);
								}
							}
						}
					} else {
						//if it's not nightime or underground
						if (!Player.Local.Surroundings.IsUnderground) {
							//disperese all the darkrot, we're done here
							for (int i = 0; i < DarkrotNodes.Count; i++) {
								DarkrotNodes [i].Disperse (Mathf.Infinity);
							}
							DarkrotNodes.Clear ();
						}
					}
				}
			}
		}

		protected Vector3 mRandomPointAroundPlayer;
		protected Color mTerrainType;
		protected GameWorld.TerrainHeightSearch mTerrainHit;

		public void SpawnDarkrotNode (Vector3 position)
		{
			GameObject newDarkrotNodeObject = GameObject.Instantiate (DarkrotNodePrefab, position, Quaternion.identity) as GameObject;
			DarkrotNode newDarkrotNode = newDarkrotNodeObject.GetComponent <DarkrotNode> ();

			DarkrotNodes.Add (newDarkrotNode);
			newDarkrotNode.Form (mDarkrotSizes [UnityEngine.Random.Range (0, mDarkrotSizes.Count)]);
		}

		public void EditorLoadTemplates ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
				Mods.Get.Editor.InitializeEditor ();
			}

			CreatureTemplates.Clear ();
			List <string> creatureTemplateNames = Mods.Get.Available ("Creature");
			foreach (string creatureTemplateName in creatureTemplateNames) {
				CreatureTemplate creatureTemplate = null;
				if (Mods.Get.Editor.LoadMod (ref creatureTemplate, "Creature", creatureTemplateName)) {
					creatureTemplate.StateTemplate.TemplateName = creatureTemplate.Name;
					CreatureTemplates.Add (creatureTemplate);
				}
			}
		}

		public void EditorSaveTemplates ()
		{
			if (!Manager.IsAwake <Mods> ()) {
				Manager.WakeUp <Mods> ("__MODS");
				Mods.Get.Editor.InitializeEditor ();
			}

			foreach (CreatureTemplate template in CreatureTemplates) {
				Mods.Get.Editor.SaveMod <CreatureTemplate> (template, "Creature", template.Name);
			}
		}

		public void EditorSortTemplates ()
		{
			CreatureTemplates.Sort ();
		}

		protected bool mIsUpdatingDarkrot = false;
		public GameObject CreatureBase;
		public List <CreatureBody> CreatureBodies = new List <CreatureBody> ();
		public List <Creature> SpawnedCreatures	= new List <Creature> ();
		public List <CreatureTemplate> CreatureTemplates = new List <CreatureTemplate> ();
		public List <AnimationClip> CreatureCutsceneAnimations = new List <AnimationClip> ();
		protected static Dictionary <string, CreatureTemplate> mTemplateLookup;
		protected static Dictionary <string, CreatureBody> mBodyLookup;
		protected List <float> mDarkrotSizes = new List <float> (100);
	}

	[Serializable]
	public class CreatureFlags
	{
	}

	[Serializable]
	public class CreaturePack
	{
		public string Name = "CreaturePack";
		public List <Texture2D> BodyTextures = new List <Texture2D> ();
		public List <Texture2D> FaceTextures = new List <Texture2D> ();
		public List <GameObject> CreaturePrefabs = new List <GameObject> ();
		public CreatureFlags AvailableTypes = new CreatureFlags ();
	}

	[Serializable]
	public class CreatureTemplateProps
	{
		//general creature template
		//these don't need to be serialized because they don't vary within a creature type
		[FrontiersAvailableModsAttribute ("Category")]
		public string InventoryFillCategory = string.Empty;
		public ShortTermMemoryLength ShortTermMemory = ShortTermMemoryLength.Medium;
		public GrudgeLengthType FleeGrudge = GrudgeLengthType.Awhile;
		public GrudgeLengthType HostileGrudge = GrudgeLengthType.Awhile;
		public StubbornnessType Stubbornness = StubbornnessType.Passive;
		public string BodyName = string.Empty;
		public string BodyTextureName = string.Empty;
		public GenericWorldItem FavoriteFood = new GenericWorldItem ();
		public float EatItemRange = 1.5f;
		public List <string> CustomWIScripts = new List<string> ();
		public bool StunnedByOverkillDamage = true;
		public bool CanOpenContainerOnDie = true;
		public bool CanOpenContainerOnStunned = false;
		public bool DestroyBodyOnDie = false;
		public string ContainerOpenOptionText = "Search";
	}

	[Serializable]
	public class CreatureTemplate : Mod, IComparable <CreatureTemplate>
	{
		public int CompareTo (CreatureTemplate other)
		{
			return Name.CompareTo (other.Name);
		}
		//Props is globally shared among all creatures using the same template
		public CreatureTemplateProps Props = new CreatureTemplateProps ();
		public CreatureState StateTemplate = new CreatureState ();
		public MotileState MotileTemplate = new MotileState ();
		public ListenerState ListenerTemplate	= new ListenerState ();
		public LookerState LookerTemplate = new LookerState ();
		public DamageableState DamageableTemplate	= new DamageableState ();
		public HostileState HostileTemplate = new HostileState ();
	}
}
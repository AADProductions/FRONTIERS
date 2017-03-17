using UnityEngine;
using System.Collections;
using Frontiers.GUI;
using System.Collections.Generic;
using System;

namespace Frontiers.World.WIScripts
{
	//controls the routine of the orb
	//also interacts with several player scripts / skills
	public class Orb : WIScript
	{
		//convenience
		public Creature creature;
		public Motile motile;
		public Damageable damageable;
		public Hostile hostile;
		public OrbBehaviorState BehaviorState = OrbBehaviorState.Awakening;
		public float MeteorEatTime = 5f;
		public float MeteorSearchRange;
		public DropItemsOnDie DropItems;
		public LuminitePowered PowerSource;
		public TrailRenderer LuminteTrail;
		public ParticleSystem LuminiteParticles;
		public Transform LuminiteGemPivot;
		public Transform LuminiteLightPivot;
		public WorldLight SearchLight;
		public WorldItem Gem;
		public DamagePackage OrbExplosionDamage = new DamagePackage ();
		public Light OrbSpotlightBottom;
		public Light OrbSpotlightForward;
		public Light OrbPointLight;
		public Meteor MeteorToGather {
			get {
				if (mMeteorToGather != null) {
					if (mMeteorToGather.IsDestroyed || mMeteorToGather.IncomingGatherer != this.worlditem) {
						mMeteorToGather = null;
					}
				}
				return mMeteorToGather;
			}
			set {
				if (mMeteorToGather != value && mMeteorToGather != null && mMeteorToGather.IncomingGatherer == this.worlditem) {
					mMeteorToGather.IncomingGatherer = null;
				}
				mMeteorToGather = value;
				if (mMeteorToGather != null) {
					mMeteorToGather.IncomingGatherer = this.worlditem;
				}
			}
		}
		public Luminite LuminiteToGather {
			get {
				if (mLuminiteToGather != null) {
					if (mLuminiteToGather.IsDestroyed || mLuminiteToGather.IncomingGatherer != worlditem) {
						mLuminiteToGather = null;
					}
				}
				return mLuminiteToGather;
			}
			set {
				if (mLuminiteToGather != value && mLuminiteToGather != null && mLuminiteToGather.IncomingGatherer == this.worlditem) {
					mLuminiteToGather.IncomingGatherer = null;
				}
				mLuminiteToGather = value;
				if (mLuminiteToGather != null) {
					mLuminiteToGather.IncomingGatherer = this.worlditem;
				}
			}
		}
		public IItemOfInterest ThingToInvestigate {
			get {
				return mThingToInvestigate;
			}
			set {
				if (value == null) {
					mThingToInvestigate = null;
					return;
				}

				if (thingsInvestigatedTonight == null) {
					thingsInvestigatedTonight = new HashSet<IItemOfInterest> ();
					thingsInvestigatedTonight.Add (mThingToInvestigate);
					mThingToInvestigate = value;
				} else {
					if (thingsInvestigatedTonight.Contains (value)) {
						//don't need to look at it!
						//Debug.Log ("Don't care about this thing " + value.gameObject.name + " any more, already investigated it");
						return;
					} else {
						mThingToInvestigate = value;
						thingsInvestigatedTonight.Add (mThingToInvestigate);
					}
				}
			}
		}

		public Material LocalFlareMaterial;
		public Transform OrbFlarePlane;
		public int NumMeteorsGathered = 0;
		public int NumLuminiteGathered = 0;
		public static float FlareMinimumDistance = 30f;
		protected IItemOfInterest mThingToInvestigate;
		protected Luminite mLuminiteToGather;
		protected Meteor mMeteorToGather;

		public bool HasPowerBeam {
			get {
				return mPowerBeam != null && !mPowerBeam.IsDestroyed;
			}
		}

		public bool HasLuminiteToGather {
			get {
				return LuminiteToGather != null;
			}
		}

		public bool HasThingToInvestigate {
			get {
				return ThingToInvestigate != null;
			}
		}

		public bool HasMeteorToGather { 
			get {
				return MeteorToGather != null;
			}
		}

		public override void OnInitialized ()
		{
			creature = worlditem.Get <Creature> ();
			creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
			creature.OnRevived += OnRevived;
			creature.Template.Props.CanOpenContainerOnDie = false;

			Looker looker = worlditem.Get <Looker> ();
			looker.State.ItemsOfInterest.Clear ();
			looker.State.ItemsOfInterest.AddRange (ThingsOrbsFindInteresting);

			motile = worlditem.Get <Motile> ();

			OrbExplosionDamage.DamageSent = 15f;
			OrbExplosionDamage.ForceSent = 0.75f;
			OrbExplosionDamage.MaterialBonus = WIMaterialType.Flesh;
			OrbExplosionDamage.MaterialPenalty = WIMaterialType.Metal;

			damageable = worlditem.Get <Damageable> ();
			damageable.State.Result = DamageableResult.Die;
			damageable.OnDie += OnDie;

			DropItems = worlditem.GetOrAdd <DropItemsOnDie> ();
			DropItems.DropEffect = "DrawAttentionToItem";
			DropItems.DropForce = 0.05f;
			DropItems.WICategoryName = creature.Template.Props.InventoryFillCategory;
			DropItems.RandomDropout = 0f;
			DropItems.DropEveryItemInCategory = true;

			PowerSource = worlditem.GetOrAdd <LuminitePowered> ();

			worlditem.OnScriptAdded += OnScriptAdded;

			Meteors.Get.OnMeteorSpawned += OnMeteorSpawned;

			if (!mUpdatingBehavior) {
				mUpdatingBehavior = true;
				StartCoroutine (UpdateBehavior ());
			}

			MeteorSearchRange = creature.Den.Radius;

			BehaviorState = OrbBehaviorState.Awakening;

			//orbs never become inactive
			worlditem.ActiveState = WIActiveState.Active;
			worlditem.ActiveStateLocked = true;
		}

		public void OnMeteorSpawned ()
		{
			switch (BehaviorState) {
			default:
				//can't stop eating / awakening / whatever
				break;

			case OrbBehaviorState.SeekingItemOfInterest:
			case OrbBehaviorState.SeekingLuminite:
			case OrbBehaviorState.SeekingMeteor:
				//stop for a moment and think about getting a meteor
				BehaviorState = OrbBehaviorState.ConsideringOptions;
				break;
			}
		}

		#region actions

		public void OnDie ()
		{
			PowerSource.HasPower = false;
			if (PowerSource.HasPowerSource) {
				//drop the crystal into the world
				WorldItem droppedGem = null;
				STransform gemSpawnPoint = new STransform (LuminiteGemPivot.position, LuminiteGemPivot.rotation.eulerAngles, Vector3.zero);
				if (WorldItems.CloneWorldItem (PowerSource.PowerSourceDopplegangerProps, gemSpawnPoint, false, WIGroups.Get.World, out droppedGem)) {
					droppedGem.Props.Local.Mode = WIMode.World;
					droppedGem.Initialize ();
					droppedGem.SetMode (WIMode.World);
					FXManager.Get.SpawnFX (droppedGem.tr, "DrawAttentionToItem");
				}
				FXManager.Get.SpawnExplosion (ExplosionType.Simple, worlditem.Position, 1f, 1f, 0.1f, 0.5f, OrbExplosionDamage);
			}
			OrbSpeak (OrbSpeakUnit.UnitExpiring, worlditem.tr);
		}

		public void OnLosePower ()
		{
			BehaviorState = OrbBehaviorState.Unpowered;
			//become stunned indefinitely
			creature.TryToStun (Mathf.Infinity);
			//SearchLight.LightEnabled = false;
			LuminteTrail.enabled = false;
			if (HasPowerBeam) {
				mPowerBeam.StopFiring ();
			}
			OrbSpeak (OrbSpeakUnit.TargetIsLost, worlditem.tr);
			LightManager.DeactivateWorldLight (SearchLight);
			OrbSpotlightForward.enabled = false;
			OrbSpotlightBottom.enabled = false;
			OrbPointLight.enabled = false;
			LuminiteParticles.enableEmission = false;

			PowerSource.CanRemoveSource = true;
		}

		public void OnRestorePower ()
		{
			creature.TryToRevive ();//won't do anything if it's not stunned
			//SearchLight.LightEnabled = true;
			LuminteTrail.enabled = true;
			//create our search light
			if (SearchLight == null) {
				SearchLight = LightManager.GetWorldLight ("RefinedLightLuminite", LuminiteLightPivot, Vector3.zero, true, WorldLightType.AlwaysOn);
			}
			OrbSpotlightForward.enabled = true;
			OrbSpotlightBottom.enabled = true;
			OrbPointLight.enabled = true;
			LuminiteParticles.enableEmission = true;

			PowerSource.CanRemoveSource = false;

			OrbSpeak (OrbSpeakUnit.ResumingNormalRoutine, worlditem.tr);
		}

		public void OnPowerSourceRemoved ()
		{
			if (mSpawnedDeactivatedOrb) {
				return;
			}
			//this will swap it out for an item that can be carried
			mSpawnedDeactivatedOrb = true;
			WorldItem deactivatedOrb = null;
			STransform orbSpawnPoint = new STransform (LuminiteGemPivot.position, LuminiteGemPivot.rotation.eulerAngles, Vector3.zero);
			if (WorldItems.CloneWorldItem (DeactivatedOrbGenericWorldItem, orbSpawnPoint, false, WIGroups.Get.World, out deactivatedOrb)) {
				deactivatedOrb.Props.Local.Mode = WIMode.World;
				deactivatedOrb.Initialize ();
				deactivatedOrb.SetMode (WIMode.World);
				worlditem.RemoveFromGame ();
			}
		}

		public void OnRevived ()
		{
			if (PowerSource.HasPower) {
				OnRestorePower ();
			}
		}

		public void OnStalk ()
		{
			//TODO some sort of scanning thing
			OrbSpeak (OrbSpeakUnit.UnitIsInDangerFromHostileTarget, worlditem.tr);
		}

		public void OnAttackStart ()
		{
			//ColoredDebug.Log ("ORB: On Attack Start", "Green");
			if (!HasPowerBeam) {
				mPowerBeam = GetPowerBeam ();
			}
			mPowerBeam.AttachTo (LuminiteGemPivot, hostile.PrimaryTarget);
			mPowerBeam.WarmUp ();//go until we stop in update
			OrbSpeak (OrbSpeakUnit.TargetIsHostileEngagingTarget, worlditem.tr);
		}

		public void OnAttackHit (DamagePackage damage)
		{
			//see if we have line of sight with the target
			//hostile will have set our target object etc.
			//ColoredDebug.Log ("ORB: On attack HIT", "Green");
			IItemOfInterest innocentBystander = null;
			Vector3 staticHitPosition = Vector3.zero;
			Vector3 targetHitPosition = Vector3.zero;
			if (WorldItems.HasLineOfSight (LuminiteGemPivot.position, damage.Target, ref targetHitPosition, ref staticHitPosition, out innocentBystander)) {
				//don't alter the damage package, we saw the target
				damage.HasLineOfSight = true;

				#if UNITY_EDITOR
				//ColoredDebug.Log ("ORB: Had line of sight, and it was the thing we wanted", "Green");
				mFireStart = LuminiteGemPivot.position;
				mFireEnd = targetHitPosition;
				mFireColor = Color.green;
				#endif


			} else if (innocentBystander != null) {

				#if UNITY_EDITOR
				//ColoredDebug.Log ("ORB: Had line of sight, and it wasn't the thing we wanted, hitting anyway: " + innocentBystander.gameObject.name, "Yellow");
				mFireStart = LuminiteGemPivot.position;
				mFireEnd = targetHitPosition;
				mFireColor = Color.yellow;
				#endif

				//we didn't see the target, we saw something else - change the target
				damage.HasLineOfSight = true;
				damage.Target = innocentBystander;
			} else {

				#if UNITY_EDITOR
				//ColoredDebug.Log ("ORB: Didn't have line of sight", "Red");
				mFireStart = LuminiteGemPivot.position;
				mFireEnd = targetHitPosition;
				mFireColor = Color.red;
				#endif

				//we didn't see the target and didn't hit anything else
				damage.HasLineOfSight = false;
				mPowerBeam.StaticEndPoint = staticHitPosition;
				mPowerBeam.TargetObject = null;
			}

			mPowerBeam.Fire (Mathf.Infinity);
		}

		public void OnAttackFinish ()
		{
			//ColoredDebug.Log ("ORB: On Attack Finish", "Green");
			if (HasPowerBeam) {
				mPowerBeam.StopFiring ();
			}
		}

		public void OnScriptAdded ()
		{
			if ((hostile == null || hostile.IsFinished) && worlditem.Is <Hostile> (out hostile)) {
				hostile.OnAttack1Start += OnAttackStart;
				hostile.OnAttack2Start += OnAttackStart;
				hostile.OnAttack1Finish += OnAttackFinish;
				hostile.OnAttack2Finish += OnAttackFinish;
				hostile.OnAttack1Hit = OnAttackHit;
				hostile.OnAttack2Hit = OnAttackHit;
			}
		}

		public void OnCollectiveThoughtStart ()
		{
			IItemOfInterest ioi = creature.CurrentThought.CurrentItemOfInterest;

			if (ioi == mMeteorToGather || ioi == mLuminiteToGather || ioi == mThingToInvestigate) {
				return;
			}

			switch (BehaviorState) {
			case OrbBehaviorState.Burrowing:
			case OrbBehaviorState.Despawning:
			case OrbBehaviorState.Unpowered:
			case OrbBehaviorState.EatingLuminite:
			case OrbBehaviorState.EatingMeteor:
			default:
				return;

			case OrbBehaviorState.ConsideringOptions:
			case OrbBehaviorState.Awakening:
				if (ioi.IOIType == ItemOfInterestType.Player || ioi != null && ioi.HasAtLeastOne (ThingsOrbsFindInteresting)) {
					ThingToInvestigate = ioi;
				}
				break;

			case OrbBehaviorState.SeekingLuminite:
			case OrbBehaviorState.SeekingMeteor:
				if (ioi.IOIType == ItemOfInterestType.Player || ioi != null && ioi.HasAtLeastOne (ThingsOrbsFindInteresting)) {
					//stop and consider the thing for a moment against our other options
					ThingToInvestigate = ioi;
				}
				BehaviorState = OrbBehaviorState.ConsideringOptions;
				break;
			}

			//see what we're doing
			if (worlditem.Is <Hostile> (out hostile)) {
				return;
			}

			//ignore it by default
			creature.CurrentThought.Should (IOIReaction.IgnoreIt);
			if (ioi == damageable.LastDamageSource) {
				//always attack a threat
				creature.CurrentThought.Should (IOIReaction.KillIt, 3);
				OrbSpeak (OrbSpeakUnit.TargetIsHostileEngagingTarget, worlditem.tr);
				return;
			} else {
				//see if it's something we care about
				if (ioi.Has ("Luminite")) {
					Luminite luminite = ioi.worlditem.Get <Luminite> ();
					//if we're already gathering luminite
					if (HasLuminiteToGather) {
						if (mLuminiteToGather == luminite) {
							//d'oh, already gathering, forget it
							return;
						}
						//go for the closer one
						if (Vector3.Distance (luminite.worlditem.Position, worlditem.Position) < Vector3.Distance (LuminiteToGather.worlditem.Position, worlditem.Position)) {
							LuminiteToGather = luminite;
							BehaviorState = OrbBehaviorState.ConsideringOptions;
						}
					} else {
						LuminiteToGather = luminite;
						LuminiteToGather.IncomingGatherer = this.worlditem;
						BehaviorState = OrbBehaviorState.ConsideringOptions;
					}
			} else if (ioi.Has ("Meteor")) {
					Meteor meteor = ioi.worlditem.Get <Meteor> ();
					if (HasMeteorToGather) {
						if (mMeteorToGather == meteor) {
							//d'oh, already gathering, forget it
							return;
						}
						//go for the closer one
						if (Vector3.Distance (meteor.worlditem.Position, worlditem.Position) < Vector3.Distance (MeteorToGather.worlditem.Position, worlditem.Position)) {
							MeteorToGather = meteor;
							BehaviorState = OrbBehaviorState.ConsideringOptions;
						}
					} else {
						MeteorToGather = meteor;
						BehaviorState = OrbBehaviorState.ConsideringOptions;
					}
				} else {
					//see if it has any of the things we care about
					if (ioi.IOIType == ItemOfInterestType.Player || ioi.HasAtLeastOne (ThingsOrbsFindInteresting)) {
						ThingToInvestigate = ioi;
						BehaviorState = OrbBehaviorState.ConsideringOptions;
					}
				}
			}
		}

		#endregion

		protected bool mSpawnedDeactivatedOrb = false;
		public static string Speech1 = "eÉ—ÇuabaÉ­aÊ’É™É¡ uaÉ—aw Ê’ipuaws";
		public static string Speech2 = "eÉ—É™adeaÅ‹suiÉ  É¡luaÉ“ uiÉ­zzliimuej";
		public static string OrbName = "oajÊƒawÉ¡";
		protected static GenericWorldItem gDeactivatedOrbGenericWorldItem = null;
		protected static GenericWorldItem gOrbGemGenericWorldItem = null;
		protected PowerBeam mPowerBeam = null;

		protected PowerBeam GetPowerBeam ()
		{
			if (mPowerBeam == null) {
				GameObject newBeam = GameObject.Instantiate (FXManager.Get.BeamPrefab) as GameObject;
				mPowerBeam = newBeam.GetComponent <PowerBeam> ();
				mPowerBeam.WarmUpColor = Colors.Alpha (Color.white, 0.1f);
				mPowerBeam.FireColor = Colors.Alpha (Colors.Get.ByName ("RawLuminiteLightColor"), 0.1f);
				mPowerBeam.RequiresOriginAndTarget = false;
			}
			return mPowerBeam;
		}

		protected IEnumerator UpdateBehavior ()
		{
			while (!motile.HasBody || !motile.Body.HasSpawned) {
				yield return null;
			}

			Transform root = creature.Body.RootBodyPart.transform;
			Transform dropPoint = root.FindChild ("OrbShellBot");
			DropItems.SpawnPoints.Add (dropPoint.localPosition);
			dropPoint = root.FindChild ("OrbShellTop");
			DropItems.SpawnPoints.Add (dropPoint.localPosition);
			dropPoint = root.FindChild ("OrbInnards");
			DropItems.SpawnPoints.Add (dropPoint.localPosition);

			OrbFlarePlane = root.FindChild ("OrbFlarePlane");
			LocalFlareMaterial = OrbFlarePlane.GetComponent <Renderer> ().material;

			OrbSpotlightBottom = root.FindChild ("OrbSpotlightBottom").GetComponent<Light>();
			OrbSpotlightForward = root.FindChild ("OrbSpotlightForward").GetComponent<Light>();
			OrbPointLight = root.FindChild ("OrbPointLight").GetComponent<Light>();

			//we'll find the gem pivot and light pivot in the orb's root
			LuminiteGemPivot = root.FindChild ("OrbLuminiteGemPivot");
			LuminiteLightPivot = root.FindChild ("OrbLuminiteLightPivot");
			Transform luminiteBits = root.FindChild ("OrbTrailRenderer");
			LuminteTrail = luminiteBits.gameObject.GetComponent <TrailRenderer> ();
			luminiteBits = root.Find ("OrbTrailParticles");
			LuminiteParticles = luminiteBits.gameObject.GetComponent <ParticleSystem> ();

			PowerSource = worlditem.GetOrAdd <LuminitePowered> ();
			PowerSource.OnLosePower += OnLosePower;
			PowerSource.OnRestorePower += OnRestorePower;
			PowerSource.OnPowerSourceRemoved += OnPowerSourceRemoved;
			PowerSource.PowerSourceDopplegangerProps.CopyFrom (OrbGemGenericWorldItem);
			PowerSource.FXOnRestorePower = "ShieldEffectSubtleGold";
			PowerSource.FXOnLosePower = "RipEffect";
			PowerSource.FXOnPowerSourceRemoved = "ShieldEffectSubtleGold";
			PowerSource.PowerSourceDopplegangerParent = LuminiteGemPivot;
			PowerSource.PowerAudio = root.FindChild ("OrbPowerAudio").GetComponent<AudioSource>();
			PowerSource.Refresh ();

			if (PowerSource.HasPower) {
				OnRestorePower ();
				StartCoroutine (Burrow (true));
			} else {
				OnLosePower ();
			}

			yield return null;

			while (worlditem.Is (WILoadState.Initialized)) {

				if (!PowerSource.HasPower) {
					BehaviorState = OrbBehaviorState.Unpowered;
				} else if (WorldClock.IsDay) {
					MeteorToGather = null;
					LuminiteToGather = null;
					ThingToInvestigate = null;
					BehaviorState = OrbBehaviorState.Burrowing;
				}

				IEnumerator nextTask = null;

				switch (BehaviorState) {
				case OrbBehaviorState.Awakening:
					nextTask = Burrow (true);
					break;

				case OrbBehaviorState.ConsideringOptions:
					nextTask = ConsiderOptions ();
					break;

				case OrbBehaviorState.SeekingMeteor:
					nextTask = SeekResources (true);
					break;

				case OrbBehaviorState.EatingMeteor:
					nextTask = MineResources (MeteorToGather);
					break;

				case OrbBehaviorState.SeekingLuminite:
					nextTask = SeekResources (false);
					break;

				case OrbBehaviorState.EatingLuminite:
					nextTask = MineResources (LuminiteToGather);
					break;

				case OrbBehaviorState.SeekingItemOfInterest:
					nextTask = SeekItemOfInterest ();
					break;

				case OrbBehaviorState.Unpowered:
					MeteorToGather = null;
					LuminiteToGather = null;
					ThingToInvestigate = null;
					if (worlditem.Is <Hostile> (out hostile)) {
						hostile.Finish ();
					}
					nextTask = WaitForPower ();
					break;

				case OrbBehaviorState.Burrowing:
					MeteorToGather = null;
					LuminiteToGather = null;
					ThingToInvestigate = null;
					if (worlditem.Is <Hostile> (out hostile)) {
						hostile.Finish ();
					}
					nextTask = Burrow (false);
					break;

				case OrbBehaviorState.Despawning:
					Debug.Log ("Despawning");
					//never save orb states
					if (thingsInvestigatedTonight != null) {
						thingsInvestigatedTonight.Clear ();
						thingsInvestigatedTonight = null;
					}
					break;
				}

				if (nextTask != null) {
					while (nextTask.MoveNext ()) {
						yield return nextTask.Current;
					}
				}

				double waitUntil = WorldClock.AdjustedRealTime + 0.5f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
			}

			mUpdatingBehavior = false;
		}

		protected IEnumerator ConsiderOptions ()
		{
			//we care about luminite more than meteors
			//and meteors more than other things
			//if we're hostile then we care about that before items of interest
			if (worlditem.Is <Hostile> (out hostile)) {
				if (HasThingToInvestigate && hostile.PrimaryTarget != ThingToInvestigate) {
					//maybe the thing we want to investigate is closer than the target
					if (Vector3.Distance (ThingToInvestigate.Position, worlditem.Position) < Vector3.Distance (hostile.PrimaryTarget.Position, worlditem.Position)) {
						//Debug.Log ("Orb is more interested in thing than in hostile target, going for hostile target");
						//finish being hostile, check out the thing instead
						hostile.Finish ();
						//wait for the hostile to finish
						yield return null;
					}
				} else if (hostile.TimeSinceAdded > Creature.ShortTermMemoryToRT (creature.Template.Props.ShortTermMemory)) {
					//bored now!
					hostile.Finish ();
				} else {
					//keep being hostile for now
					yield break;
				}
			}

			double waitUntil = WorldClock.AdjustedRealTime + 0.05f + UnityEngine.Random.value;
			while (WorldClock.AdjustedRealTime < waitUntil) {
				yield return null;
			}

			yield return null;
			if (HasThingToInvestigate) {
				OrbSpeak (OrbSpeakUnit.TargetIsStrangeInvestigatingTarget, worlditem.tr);
				BehaviorState = OrbBehaviorState.SeekingItemOfInterest;
			} else if (HasLuminiteToGather) {
				OrbSpeak (OrbSpeakUnit.DetectedLuminiteInRawState, worlditem.tr);
				if (Vector3.Distance (LuminiteToGather.worlditem.Position, worlditem.Position) < motile.State.MotileProps.RVORadius * 5) {
					BehaviorState = OrbBehaviorState.EatingLuminite;
				} else {
					BehaviorState = OrbBehaviorState.SeekingLuminite;
				}
			} else if (HasMeteorToGather) {
				OrbSpeak (OrbSpeakUnit.TargetIsStrangeInvestigatingTarget, worlditem.tr);
				if (Vector3.Distance (MeteorToGather.worlditem.Position, worlditem.Position) < motile.State.MotileProps.RVORadius * 5) {
					BehaviorState = OrbBehaviorState.EatingMeteor;
				} else {
					BehaviorState = OrbBehaviorState.SeekingMeteor;
				}
			} else {
				//we don't have a thing to investigate, a meteor to grab or luminite to gather
				Meteor closestMeteor = null;
				Meteor meteorToCheck = null;
				float closestSoFar = Mathf.Infinity;
				float current = 0f;
				for (int i = 0; i < Meteors.Get.MeteorsSpawned.Count; i++) {
					current = Vector3.Distance (Meteors.Get.MeteorsSpawned [i].worlditem.Position, worlditem.Position);
					if (current < closestSoFar) {
						meteorToCheck = Meteors.Get.MeteorsSpawned [i];
						if (meteorToCheck.IncomingGatherer != null && meteorToCheck.IncomingGatherer != this) {
							//better check if we're closer
							if (Vector3.Distance (meteorToCheck.IncomingGatherer.Position, meteorToCheck.worlditem.Position) > current) {
								//we're stealing this
								closestSoFar = current;
								closestMeteor = meteorToCheck;
							}
						} else {
							closestSoFar = current;
							closestMeteor = meteorToCheck;
						}
					}
				}

				if (closestMeteor != null) {
					OrbSpeak (OrbSpeakUnit.DetectedLuminiteInRefinedState, worlditem.tr);
					MeteorToGather = closestMeteor;
					BehaviorState = OrbBehaviorState.SeekingMeteor;
				}
				yield return null;
			}

			yield break;
		}

		protected IEnumerator SeekItemOfInterest ()
		{
			if (ThingToInvestigate == null) {
				BehaviorState = OrbBehaviorState.ConsideringOptions;
				yield break;
			}

			IItemOfInterest startThingToInvestigate = ThingToInvestigate;
			MotileAction seekAction = creature.FollowThingAction (startThingToInvestigate);
			seekAction.Name = "Seek " + startThingToInvestigate.gameObject.name + " by Orb";
			seekAction.Expiration = MotileExpiration.Duration;
			seekAction.RTDuration = 5f;

			while (HasThingToInvestigate && ThingToInvestigate == startThingToInvestigate && BehaviorState == OrbBehaviorState.SeekingItemOfInterest) {
				if (seekAction.IsFinished) {
					OrbSpeak (OrbSpeakUnit.ResumingNormalRoutine, worlditem.tr);
					//if we're finished but not in range something happened
					BehaviorState = OrbBehaviorState.ConsideringOptions;
					yield break;
				} else if (seekAction.IsInRange) {
					OrbSpeak (OrbSpeakUnit.TargetIsStrangeInvestigatingTarget, worlditem.tr);
					double scanTime = WorldClock.AdjustedRealTime + 1.5f;
					FXManager.Get.SpawnFX (startThingToInvestigate, "ScanEffect");
					MasterAudio.PlaySound (MasterAudio.SoundType.Machines, startThingToInvestigate.gameObject.transform, "ScanSound");
					while (WorldClock.AdjustedRealTime < scanTime) {
						yield return null;
					}
					if (ThingToInvestigate == startThingToInvestigate) {
						ThingToInvestigate = null;
					}
				}
				double waitUntil = WorldClock.AdjustedRealTime + 1f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
			}

			if (!seekAction.IsFinished && seekAction.LiveTarget == startThingToInvestigate) {
				//if we're still seeking the meteor
				//try to stop motile
				seekAction.TryToFinish ();
			}

			if (ThingToInvestigate == startThingToInvestigate) {
				//clear it unless it has changed
				ThingToInvestigate = null;
			}

			yield break;
		}

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public void Update ()
		{
			if (OrbFlarePlane == null) {
				return;
			}

			if (worlditem.DistanceToPlayer > FlareMinimumDistance && BehaviorState != OrbBehaviorState.Burrowing) {
				flareColor = Color.Lerp (flareColor, Colors.Get.OrbFlareMaterialColor, Time.deltaTime);
			} else {
				flareColor = Color.Lerp (flareColor, Color.black, Time.deltaTime);
			}
			LocalFlareMaterial.SetColor ("_Color", flareColor);
			OrbFlarePlane.LookAt (Player.Local.Position);
		}

		protected IEnumerator Burrow (bool unBurrow)
		{
			yield return null;
			if (worlditem.Is (WIActiveState.Visible | WIActiveState.Active)) {
				OrbSpeak (unBurrow ? OrbSpeakUnit.BeginningRoutine : OrbSpeakUnit.DaylightDetected, worlditem.tr);
				//wait for a bit so this gets offset
				if (!unBurrow) {
					motile.StopMotileActions ();
				}
				double waitUntil = WorldClock.AdjustedRealTime + UnityEngine.Random.value * 2f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}

				string animName = unBurrow ? "OrbUnburrowingAnimation" : "OrbBurrowingAnimation";

				motile.IsImmobilized = true;

				MasterAudio.PlaySound (MasterAudio.SoundType.Machines, "BurrowingMachine");
				Animation anim = creature.Body.RootBodyPart.GetComponent <Animation> ();
				anim [animName].normalizedTime = 0f;
				anim [animName].normalizedSpeed = 1f;
				anim.Play (animName, PlayMode.StopAll);
				double nextEffectTime = WorldClock.AdjustedRealTime + 0.1f;
				while (anim [animName].normalizedTime < 1f) {
					//we can be killed in the meantime
					if (BehaviorState != (unBurrow ? OrbBehaviorState.Awakening : OrbBehaviorState.Burrowing)) {
						motile.IsImmobilized = false;
						anim.Stop ();
						motile.Body.RootBodyPart.tr.localRotation = Quaternion.identity;
						yield break;
					}
					if (WorldClock.AdjustedRealTime > nextEffectTime) {
						nextEffectTime = WorldClock.AdjustedRealTime + 0.75f;
						FXManager.Get.SpawnFX (gameObject, "BurrowEffect");
					}
					yield return null;
				}

				motile.IsImmobilized = false;
				anim.Stop ();
				motile.Body.RootBodyPart.tr.localRotation = Quaternion.identity;

				waitUntil = WorldClock.AdjustedRealTime + 3f;
				while (WorldClock.AdjustedRealTime < waitUntil) { 
					yield return null;
				}
			}
			BehaviorState = unBurrow ? OrbBehaviorState.ConsideringOptions : OrbBehaviorState.Despawning;

			if (!unBurrow) {
				worlditem.RemoveFromGame ();
				GameObject.Destroy (motile.Body.gameObject);
				Finish ();
			}

			yield break;
		}

		protected IEnumerator WaitForPower ()
		{
			yield return null;
			while (!PowerSource.HasPower) {
				yield return null;
			}
			yield return null;
			if (BehaviorState == OrbBehaviorState.Unpowered) {
				BehaviorState = OrbBehaviorState.Awakening;
			}
			yield break;
		}

		protected IEnumerator SeekResources (bool isMeteor)
		{
			//Debug.Log ("Seeking resources, meteor? " + isMeteor.ToString ());
			IItemOfInterest thingToSeek = null;
			if (isMeteor) {
				if (HasMeteorToGather) {
					thingToSeek = mMeteorToGather.worlditem;
				} else {
					BehaviorState = OrbBehaviorState.ConsideringOptions;
					yield break;
				}
				//Debug.Log ("No longer has meteor to seek, stoppping");
			} else {
				if (HasLuminiteToGather) {
					thingToSeek = mLuminiteToGather.worlditem;
				} else {
					BehaviorState = OrbBehaviorState.ConsideringOptions;
				}
				yield break;
			}

			//Debug.Log ("Seeking resource " + thingToSeek.gameObject.name);

			MotileAction seekAction = creature.FollowThingAction (thingToSeek);
			seekAction.Terrain = TerrainType.AllButCivilization;
			seekAction.Expiration = MotileExpiration.TargetInRange;
			seekAction.FollowType = MotileFollowType.Follower;
			seekAction.Range = motile.State.MotileProps.RVORadius * 5;
			bool keepSeeking = true;
			while (keepSeeking) {
				//Debug.Log ("Seeking meteor...");
				if (seekAction.IsFinished) {
					//if we're in range it's time to eat our meteor
					//Debug.Log ("Seek action was finished and we're in range, so we're eating");
					BehaviorState = isMeteor ? OrbBehaviorState.EatingMeteor : OrbBehaviorState.EatingLuminite;
					yield break;
				}

				double waitUntil = WorldClock.AdjustedRealTime + 0.05f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}

				if (isMeteor) {
					keepSeeking = HasMeteorToGather && BehaviorState == OrbBehaviorState.SeekingMeteor;
				} else {
					keepSeeking = HasLuminiteToGather && BehaviorState == OrbBehaviorState.SeekingLuminite;
				}
			}

			if (!seekAction.IsFinished) {
				//if we're still seeking the meteor
				//try to stop motile
				seekAction.TryToFinish ();
			}

			yield break;
		}

		protected IEnumerator MineResources (bool isMeteor)
		{
			WorldItem thingToGather = null;
			if (isMeteor) {
				if (HasMeteorToGather) {
					thingToGather = mMeteorToGather.worlditem;
				} else {
					BehaviorState = OrbBehaviorState.ConsideringOptions;
					yield break;
				}
			} else {
				if (HasLuminiteToGather) {
					thingToGather = mLuminiteToGather.worlditem;
				} else {
					BehaviorState = OrbBehaviorState.ConsideringOptions;
				}
				yield break;
			}

			//Debug.Log ("Starting item mining process");

			double startEatTime = WorldClock.AdjustedRealTime;
			double finishMiningTime = WorldClock.AdjustedRealTime + MeteorEatTime;
			float damageOnStart = 0f;
			Damageable itemDamageable = thingToGather.Get <Damageable> ();
			if (itemDamageable != null) {
				damageOnStart = itemDamageable.NormalizedDamage;
			}

			MotileAction waitAction = creature.WatchThingAction (thingToGather);
			waitAction.Expiration = MotileExpiration.TargetOutOfRange;
			waitAction.OutOfRange = 25f;

			OrbSpeak (OrbSpeakUnit.MiningLuminite, worlditem.tr);

			bool keepMining = true;
			while (keepMining) {

				if (mPowerBeam == null) {
					mPowerBeam = GetPowerBeam ();
				}
				mPowerBeam.AttachTo (LuminiteGemPivot, thingToGather);
				mPowerBeam.WarmUp ();
				mPowerBeam.Fire (0.45f);

				if (waitAction.IsFinished || waitAction.LiveTarget != thingToGather) {
					//something has interrupted us
					//Debug.Log ("Wait action was finished or had a different target, stopping motile action");
					BehaviorState = OrbBehaviorState.ConsideringOptions;
				} else {
					if (itemDamageable != null && itemDamageable.NormalizedDamage > damageOnStart) {
						//Debug.Log ("Something damaged the meteor, checking out what");
						mPowerBeam.StopFiring ();
						OrbSpeak (OrbSpeakUnit.TargetBehavingErratically, worlditem.tr);
						//look at the thing that last hit it
						ThingToInvestigate = itemDamageable.LastDamageSource;
						BehaviorState = OrbBehaviorState.SeekingItemOfInterest;
					} else if (WorldClock.AdjustedRealTime > finishMiningTime) {
						//Debug.Log ("Done eating!");
						//it's toast! kill it
						//force it to not spawn any items on die
						FXManager.Get.SpawnExplosionFX (ExplosionType.Base, null, thingToGather.Position);
						thingToGather.worlditem.RemoveFromGame ();
						if (isMeteor) {
							NumMeteorsGathered++;
						} else {
							NumLuminiteGathered++;
						}
					}
				}

				double waitUntil = WorldClock.AdjustedRealTime + 0.125f;
				while (WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}

				if (isMeteor) {
					keepMining = HasMeteorToGather && thingToGather == MeteorToGather.worlditem && BehaviorState == OrbBehaviorState.EatingMeteor;
				} else {
					keepMining = HasLuminiteToGather && thingToGather == MeteorToGather.worlditem && BehaviorState == OrbBehaviorState.EatingLuminite;
				}
			}

			if (mPowerBeam != null) {
				mPowerBeam.StopFiring ();
			}

			BehaviorState = OrbBehaviorState.ConsideringOptions;
			yield break;
		}

		public override void OnFinish ()
		{
			if (mPowerBeam != null) {
				GameObject.Destroy (mPowerBeam);
			}
		}

		MotileAction mGetMeteorAction;
		protected int mCheckMeteor = 0;
		protected bool mUpdatingBehavior = false;
		protected Color32 flareColor;
		#if UNITY_EDITOR
		protected Vector3 mFireStart;
		protected Vector3 mFireEnd;
		protected Color mFireColor;

		public void OnDrawGizmos ()
		{
			Gizmos.color = mFireColor;
			Gizmos.DrawLine (mFireStart, mFireEnd);
		}
		#endif
		public static List <string> ThingsOrbsFindInteresting = new List <string> {
			"Creature",
			"WorldPlant",
			"Character",
			"FoodStuff"
		};

		public static void OrbSpeak (OrbSpeakUnit unit, Transform origin)
		{
			if (gOrbSpeeches == null) {
				gOrbSpeeches = new Dictionary<OrbSpeakUnit, string> ();
				foreach (var enumValue in Enum.GetValues (typeof (OrbSpeakUnit))) {
					gOrbSpeeches.Add ((OrbSpeakUnit)enumValue, Data.GameData.AddSpacesToSentence (enumValue.ToString ()));
				}
			}

			if (Listener.IsInAudibleRange (Player.Local.Position, origin.position, Globals.MaxAudibleRange, Globals.MaxAudibleRange * 0.25f)) {
				GUI.NGUIScreenDialog.AddSpeech (gOrbSpeeches [unit], OrbName, 1f, true);
				MasterAudio.PlaySound (MasterAudio.SoundType.Obex, origin);
			}
		}

		public static GenericWorldItem DeactivatedOrbGenericWorldItem {
			get {
				if (gDeactivatedOrbGenericWorldItem == null) {
					gDeactivatedOrbGenericWorldItem = new GenericWorldItem ();
					gDeactivatedOrbGenericWorldItem.PackName = "Oblox";
					gDeactivatedOrbGenericWorldItem.PrefabName = "Deactivated Orb";
					gDeactivatedOrbGenericWorldItem.DisplayName = "Deactivated Orb";
				}
				return gDeactivatedOrbGenericWorldItem;
			}
		}

		public static GenericWorldItem OrbGemGenericWorldItem {
			get {
				if (gOrbGemGenericWorldItem == null) {
					gOrbGemGenericWorldItem = new GenericWorldItem ();
					gOrbGemGenericWorldItem.PackName = "Crystals";
					gOrbGemGenericWorldItem.PrefabName = "Cut Luminite 1";
					gOrbGemGenericWorldItem.State = "Light";
					gOrbGemGenericWorldItem.StackName = "Cut Luminite";
					gOrbGemGenericWorldItem.DisplayName = "Cut Luminite";
				}
				return gOrbGemGenericWorldItem;
			}
		}

		public static Dictionary <OrbSpeakUnit, string> gOrbSpeeches;
		protected HashSet <IItemOfInterest> thingsInvestigatedTonight;
	}

	public enum OrbBehaviorState
	{
		Awakening,
		Burrowing,
		SeekingMeteor,
		EatingMeteor,
		SeekingLuminite,
		EatingLuminite,
		SeekingItemOfInterest,
		ConsideringOptions,
		Unpowered,
		Despawning,
	}

	public enum OrbSpeakUnit
	{
		WaitingForInstructions,
		GoingToActionNode,
		ReachedActionNode,
		DeterminingNextObjective,
		ListenerDetectedObject,
		SeekerDeterminingOptimalPath,
		FollowingPath,
		DetectedLuminiteInRawState,
		DetectedLuminiteInRefinedState,
		MiningLuminite,
		FinishedminingLuminite,
		LuminitePurityIsInsufficient,
		RefiningLuminite,
		FinishedRefiningLuminite,
		NewTargetAcquired,
		ReachedTargetNowAssessingTarget,
		TargetIsDesirableAcquiringTarget,
		TargetIsHostileEngagingTarget,
		TargetIsBenignIgnoringTarget,
		TargetIsStrangeInvestigatingTarget,
		TargetBehavingErratically,
		TargetIsLost,
		SeekingTarget,
		BeginningRoutine,
		FindingNextWayStone,
		NextWayStoneFound,
		CargoSufficientQuotaIsMet,
		ReturningToGateway,
		ReturningToFacility,
		ResumingNormalRoutine,
		DaylightDetected,
		UnitIsInDangerFromHostileTarget,
		UnitIsDamagedByHostileTarget,
		UnitAttemptingToFleeHostileTarget,
		UnitRequiresAssistance,
		UnitSufferedCatastrophicDamage,
		UnitExpiring,
	}
}
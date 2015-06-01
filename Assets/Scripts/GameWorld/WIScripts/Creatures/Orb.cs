using UnityEngine;
using System.Collections;
using Frontiers.GUI;
using System.Collections.Generic;
using System;

namespace Frontiers.World.WIScripts
{	//controls the routine of the orb
	//also interacts with several player scripts / skills
	public class Orb : WIScript {
		//convenience
		public Creature creature;
		public Motile motile;
		public Damageable damageable;
		public Hostile hostile;

		public float MeteorEatTime = 5f;
		public float MeteorSearchRange;

		public DropItemsOnDie DropItems;
		public LuminitePowered PowerSource;
		public WorldItem LuminiteToGather = null;
		public TrailRenderer LuminteTrail;
		public ParticleSystem LuminiteParticles;
		public Transform LuminiteGemPivot;
		public Transform LuminiteLightPivot;
		public WorldLight SearchLight;
		public WorldItem Gem;

		public DamagePackage OrbExplosionDamage = new DamagePackage ( );

		public Light OrbSpotlightBottom;
		public Light OrbSpotlightForward;
		public Light OrbPointLight;

		public Meteor TargetMeteor;

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

		public override void OnInitialized ()
		{
			creature = worlditem.Get <Creature> ();
			creature.OnCollectiveThoughtStart += OnCollectiveThoughtStart;
			creature.OnRevived += OnRevived;
			creature.Template.Props.CanOpenContainerOnDie = false;

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

			worlditem.OnScriptAdded += OnScriptAdded;
			worlditem.OnVisible += OnVisible;

			Meteors.Get.OnMeteorSpawned += OnMeteorSpawned;

			MeteorSearchRange = creature.Den.Radius;
		}

		public void OnMeteorSpawned () {
			enabled = true;
		}

		public void OnVisible ( ) {

			if (!mInitialized) {
				return;
			}

			try {
				Transform root = creature.Body.RootBodyPart.transform;
				Transform dropPoint = root.FindChild ("OrbShellBot");
				DropItems.SpawnPoints.Add (dropPoint.localPosition);
				dropPoint = root.FindChild ("OrbShellTop");
				DropItems.SpawnPoints.Add (dropPoint.localPosition);
				dropPoint = root.FindChild ("OrbInnards");
				DropItems.SpawnPoints.Add (dropPoint.localPosition);

				OrbSpotlightBottom = root.FindChild ("OrbSpotlightBottom").light;
				OrbSpotlightForward = root.FindChild ("OrbSpotlightForward").light;
				OrbPointLight = root.FindChild ("OrbPointLight").light;

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
				PowerSource.PowerAudio = root.FindChild ("OrbPowerAudio").audio;

				if (PowerSource.HasPower) {
					OnRestorePower ();
				} else {
					OnLosePower ();
				}
			}
			catch (Exception e) {
				Debug.Log ("Error in Orb startup, proceding normally: " + e.ToString ());
			}
		}

		public void OnDie ( ) {
			Debug.Log ("ON DIE CALLED IN ORB");
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
			//become stunned indefinitely
			Debug.Log ("Losing power in orb, trying to stun");
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
			if (SearchLight != null) {
				SearchLight = LightManager.GetWorldLight ("RefinedLightLuminite", LuminiteLightPivot, Vector3.zero, true, WorldLightType.AlwaysOn);
			}
			OrbSpotlightForward.enabled = true;
			OrbSpotlightBottom.enabled = true;
			OrbPointLight.enabled = true;
			LuminiteParticles.enableEmission = true;

			PowerSource.CanRemoveSource = false;

			OrbSpeak (OrbSpeakUnit.ResumingNormalRoutine, worlditem.tr);
		}

		public void OnPowerSourceRemoved () {
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

		protected bool mSpawnedDeactivatedOrb = false;

		public void OnRevived ( )
		{
			if (PowerSource.HasPower) {
				OnRestorePower ();
			}
		}

		public void OnStalk ( ) {
			//TODO some sort of scanning thing
			OrbSpeak (OrbSpeakUnit.UnitIsInDangerFromHostileTarget, worlditem.tr);
		}

		public void OnAttackStart ( )
		{
			ColoredDebug.Log ("ORB: On Attack Start", "Green");
			if (!HasPowerBeam) {
				mPowerBeam = GetPowerBeam ();
			}
			mPowerBeam.AttachTo (LuminiteGemPivot, hostile.PrimaryTarget);
			mPowerBeam.WarmUp ( );//go until we stop in update
			OrbSpeak (OrbSpeakUnit.TargetIsHostileEngagingTarget, worlditem.tr);
		}

		public void OnAttackHit (DamagePackage damage) {
			//see if we have line of sight with the target
			//hostile will have set our target object etc.
			ColoredDebug.Log ("ORB: On attack HIT", "Green");
			IItemOfInterest innocentBystander = null;
			Vector3 staticHitPosition = Vector3.zero;
			Vector3 targetHitPosition = Vector3.zero;
			if (WorldItems.HasLineOfSight (LuminiteGemPivot.position, damage.Target, ref targetHitPosition, ref staticHitPosition, out innocentBystander)) {
				//don't alter the damage package, we saw the target
				damage.HasLineOfSight = true;

				#if UNITY_EDITOR
				ColoredDebug.Log ("ORB: Had line of sight, and it was the thing we wanted", "Green");
				mFireStart = LuminiteGemPivot.position;
				mFireEnd = targetHitPosition;
				mFireColor = Color.green;
				#endif


			} else if (innocentBystander != null) {

				#if UNITY_EDITOR
				ColoredDebug.Log ("ORB: Had line of sight, and it wasn't the thing we wanted, hitting anyway: " + innocentBystander.gameObject.name, "Yellow");
				mFireStart = LuminiteGemPivot.position;
				mFireEnd = targetHitPosition;
				mFireColor = Color.yellow;
				#endif

				//we didn't see the target, we saw something else - change the target
				damage.HasLineOfSight = true;
				damage.Target = innocentBystander;
			} else {

				#if UNITY_EDITOR
				ColoredDebug.Log ("ORB: Didn't have line of sight", "Red");
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

		public void OnAttackFinish ( ) {
			ColoredDebug.Log ("ORB: On Attack Finish", "Green");
			if (HasPowerBeam) {
				mPowerBeam.StopFiring ();
			}
		}

		public void OnScriptAdded ( )
		{
			if ((hostile == null || hostile.IsFinished) && worlditem.Is <Hostile> (out hostile)) {
				Debug.Log ("Adding NEW hostile in orb, setting OnAttack in both");
				hostile.OnAttack1Start += OnAttackStart;
				hostile.OnAttack2Start += OnAttackStart;
				hostile.OnAttack1Finish += OnAttackFinish;
				hostile.OnAttack2Finish += OnAttackFinish;
				hostile.OnAttack1Hit = OnAttackHit;
				hostile.OnAttack2Hit = OnAttackHit;
			}
		}

		public void OnCollectiveThoughtStart ( )
		{
			IItemOfInterest itemOfInterest = creature.CurrentThought.CurrentItemOfInterest;
			if (hostile != null && hostile.HasPrimaryTarget && hostile.PrimaryTarget == itemOfInterest) {
				creature.CurrentThought.Should (IOIReaction.IgnoreIt, 3);
				return;
			}

			if (itemOfInterest == damageable.LastDamageSource) {
				//always attack a threat
				creature.CurrentThought.Should (IOIReaction.KillIt, 3);
				OrbSpeak (OrbSpeakUnit.TargetIsHostileEngagingTarget, worlditem.tr);
				return;
			}

			switch (itemOfInterest.IOIType) {
			case ItemOfInterestType.WorldItem:
				if (HasLuminiteToGather) {
					creature.CurrentThought.Should (IOIReaction.IgnoreIt);
				} else {
					Luminite luminite = null;
					//orbs 'smell' luminite so luminite encased in glass doesn't count
					//orbs also don't care about dark luminite
					if (itemOfInterest.worlditem.Is <Luminite> (out luminite) && !luminite.State.IsEncasedInGlass && !luminite.State.IsDark) {
						Debug.Log ("Found luminite to eat!");
						creature.CurrentThought.Should (IOIReaction.EatIt, 3);
						LuminiteToGather = itemOfInterest.worlditem;
					} else {
						creature.CurrentThought.Should (IOIReaction.FollowIt);
					}
				}
				break;

			case ItemOfInterestType.Player:
				if (Vector3.Distance (Player.Local.Position, worlditem.tr.position) < 3f) {
					creature.CurrentThought.Should (IOIReaction.WatchIt);
				}
				break;

			default:
				break;
			}	
		}

		public static void OrbSpeak (OrbSpeakUnit unit, Transform origin) {
			if (Listener.IsInAudibleRange (Player.Local.Position, origin.position, Globals.MaxAudibleRange, Globals.MaxAudibleRange)) {
				GUI.NGUIScreenDialog.AddSpeech (unit.ToString ( ), "Orb", 1f);//TODO create a speak lookup table
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
					gOrbGemGenericWorldItem.DisplayName = "Cut Luminite";
				}
				return gOrbGemGenericWorldItem;
			}
		}

		public string Speech1 = "eɗǁuabaɭaʒəɡ uaɗaw ʒipuaws";
		public string Speech2 = "eɗəadeaŋsuiɠ ɡluaɓ uiɭzzliimuej";
		public string OrbName = "oajʃawɡ";

		protected static GenericWorldItem gDeactivatedOrbGenericWorldItem = null;
		protected static GenericWorldItem gOrbGemGenericWorldItem = null;
		protected PowerBeam mPowerBeam = null;

		public void FixedUpdate () {
			if (WorldClock.IsDay) {
				TargetMeteor = null;
				if (mGetMeteorAction != null) {
					mGetMeteorAction.Cancel ();
					mGetMeteorAction = null;
				}
				return;
			}

			if (mEatingMeteor) {
				return;
			}

			mCheckMeteor++;
			if (mCheckMeteor > 250) {
				mCheckMeteor = 0;
				if (TargetMeteor == null || TargetMeteor.IsDestroyed || TargetMeteor.IncomingOrb != this) {
					TargetMeteor = null;
					if (FindNewMeteor ()) {
						GoToMeteor ();
						return;
					}
				} else {
					//even if we have a meteor
					//check to make sure we're not missing an even closer meteor
					if (FindCloserMeteor ()) {
						GoToMeteor ();
						return;
					}
				}
			}

			if (TargetMeteor == null || TargetMeteor.IsDestroyed) {
				if (FindNewMeteor ()) {
					GoToMeteor ();
					return;
				} else {
					if (mGetMeteorAction != null) {
						mGetMeteorAction.Cancel ();
						mGetMeteorAction = null;
					}
					enabled = false;
					return;
				}
			} else { 
				if (mGetMeteorAction == null || mGetMeteorAction.IsFinished) {
					GoToMeteor ();
					return;
				}
			}
		}

		protected bool FindCloserMeteor ( ) {
			bool foundCloserMeteor = false;
			List <Meteor> meteors = Meteors.Get.MeteorsSpawned;
			Vector3 orbPos = worlditem.Position;
			float currentMag = (orbPos - TargetMeteor.worlditem.Position).magnitude;
			for (int i = 0; i < meteors.Count; i++) {
				Meteor o = meteors [i];
				if (o != null && o.IncomingOrb == null) {
					float potentialMag = (orbPos - o.worlditem.Position).magnitude;
					if (potentialMag < currentMag) {
						Debug.Log ("Found a closer meteor, going for it instead");
						TargetMeteor.IncomingOrb = null;
						TargetMeteor = o;
						TargetMeteor.IncomingOrb = this;
						foundCloserMeteor = true;
						break;
					}
				}
			}
			return foundCloserMeteor;
		}

		protected bool FindNewMeteor ( ) {
			bool foundNewMeteor = false;
			List <Meteor> meteors = Meteors.Get.MeteorsSpawned;
			Vector3 orbPos = worlditem.Position;
			for (int i = 0; i < meteors.Count; i++) {
				Meteor m = meteors [i];
				if (m != null && m.IncomingOrb == null && Vector3.Distance (orbPos, m.worlditem.Position) < MeteorSearchRange) {
					TargetMeteor = m;
					foundNewMeteor = true;
					break;
				}
			}
			if (foundNewMeteor) {
				OrbSpeak (OrbSpeakUnit.NewTargetAcquired, worlditem.tr);
				TargetMeteor.IncomingOrb = this;
			}
			return foundNewMeteor;
		}

		protected void GoToMeteor ( ) {
			if (mGetMeteorAction == null || mGetMeteorAction.IsFinished) {
				Debug.Log ("Going to meteor via eat thing");
				mGetMeteorAction = creature.EatThingAction (TargetMeteor.worlditem);
				mGetMeteorAction.OnFinishAction += OnReachMeteor;
			} else {
				mGetMeteorAction.LiveTarget = TargetMeteor.worlditem;
			}
		}

		public void OnReachMeteor ( ) {
			Debug.Log ("reached meteor in orb");
			if (!mEatingMeteor) {
				mEatingMeteor = true;
				StartCoroutine (EatMeteor (TargetMeteor));
			}
		}

		protected PowerBeam GetPowerBeam ( ) {
			GameObject newBeam = GameObject.Instantiate (FXManager.Get.BeamPrefab) as GameObject;
			mPowerBeam = newBeam.GetComponent <PowerBeam> ();
			mPowerBeam.WarmUpColor = Colors.Alpha (Color.white, 0.1f);
			mPowerBeam.FireColor = Colors.Alpha (Color.yellow, 0.1f);
			mPowerBeam.RequiresOriginAndTarget = false;
			return mPowerBeam;
		}

		protected IEnumerator EatMeteor (Meteor targetMeteor) {
			double startEatTime = WorldClock.AdjustedRealTime;
			Damageable meteorDamageable = targetMeteor.worlditem.Get <Damageable> ();
			float damageOnStart = meteorDamageable.NormalizedDamage;
			Debug.Log ("Starting meteor eating process, damage is " + damageOnStart.ToString ());
			OrbSpeak (OrbSpeakUnit.MiningLuminite, worlditem.tr);

			if (!HasPowerBeam) {
				mPowerBeam = GetPowerBeam ();
			}
			mPowerBeam.AttachTo (LuminiteGemPivot, TargetMeteor.worlditem);
			mPowerBeam.WarmUp ( );//go until we stop in update
			mPowerBeam.Fire (Mathf.Infinity);

			while (targetMeteor != null && !targetMeteor.IsDestroyed) {
				if (meteorDamageable.NormalizedDamage > damageOnStart) {
					Debug.Log ("Something attacked meteor in the meantime");
					mPowerBeam.StopFiring ();
					OrbSpeak (OrbSpeakUnit.TargetBehavingErratically, worlditem.tr);
					mEatingMeteor = false;
					yield break;

				} else if (WorldClock.AdjustedRealTime > startEatTime + MeteorEatTime) {
					//it's toast! kill it
					//force it to not spawn any items on die
					Debug.Log ("Eating time is done! destroying meteor and ending beam");
					targetMeteor.worlditem.Get <DropItemsOnDie> ().MaxRandomItems = 0;
					targetMeteor.worlditem.Get <Damageable> ().InstantKill ("Orb");
					mPowerBeam.StopFiring ();
					targetMeteor = null;
				}

				yield return null;
			}

			mEatingMeteor = false;
			yield break;
		}

		MotileAction mGetMeteorAction;
		protected bool mEatingMeteor = false;
		protected int mCheckMeteor = 0;

		#if UNITY_EDITOR
		protected Vector3 mFireStart;
		protected Vector3 mFireEnd;
		protected Color mFireColor;

		public void OnDrawGizmos ( ) {
			Gizmos.color = mFireColor;
			Gizmos.DrawLine (mFireStart, mFireEnd);
		}
		#endif
	}

	public enum OrbSpeakUnit {
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
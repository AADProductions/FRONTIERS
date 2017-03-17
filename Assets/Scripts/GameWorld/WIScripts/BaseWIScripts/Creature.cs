using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Reflection;
////using Pathfinding.RVO;
using ExtensionMethods;
using Frontiers;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class Creature : WIScript
	{
		static Creature ()
		{
			//add the tags we need to difficulty settings
			DifficultySetting.AvailableTags.Add ("NoHostileCreatures");
		}

		public CreatureState State = new CreatureState ();
		public CreatureTemplate Template {
			get {
				if (mTemplate == null) {
					CheckTemplate ();
				}
				return mTemplate;
			}
			set {
				mTemplate = value;
			}
		}
		public CreatureBody Body = null;
		public ICreatureDen Den = null;
		public Damageable damageable;

		protected CreatureTemplate mTemplate;

		public override bool CanBeCarried {
			get {
				return false;
			}
		}

		public override bool CanEnterInventory {
			get {
				return false;
			}
		}

		public bool HasDen {
			get {
				return Den != null;
			}
		}

		public bool IsInDen {
			get {
				return mIsInDen;
			}
			set {
				if (!HasDen) {
					mIsInDen = value;
					return;
				}

				if (mIsInDen) {
					//if we were in our den
					if (!value) {
						//we've left the den
						mIsInDen = value;
						//Debug.Log ("We've left the den!");
						OnLeaveDen.SafeInvoke ();
					}
				} else {//if we weren't in our den
					if (value) {
						//we've entered our den
						mIsInDen = value;
						//Debug.Log ("Phew we're returning to the den");
						OnVisitDen.SafeInvoke ();
					}
				}
			}
		}

		public bool IsInDenInnerRadius {
			get {
				return mIsInDenInnerRadius;
			}
			set {
				if (!HasDen) {
					mIsInDenInnerRadius = value;
					return;
				}

				if (mIsInDenInnerRadius) {
					//if we were in our den
					if (!value) {
						//we've left the den
						mIsInDenInnerRadius = value;
						OnLeaveDenInnerRadius.SafeInvoke ();
					}
				} else {//if we weren't in our den
					if (value) {
						//we've entered our den
						mIsInDenInnerRadius = value;
						OnVisitDenInnerRadius.SafeInvoke ();
					}
				}
			}
		}
		//behavior actions - behaviors subscribe to these
		public Action OnDaytimeStart;
		public Action OnNightTimeStart;
		public Action OnHourStart;
		public Action OnLeaveDen;
		public Action OnVisitDen;
		public Action OnLeaveDenInnerRadius;
		public Action OnVisitDenInnerRadius;
		public Action OnPlayerVisitDen;
		public Action OnPlayerLeaveDen;
		public Action OnPlayerVisitDenInnerRadius;
		public Action OnPlayerLeaveDenInnerRadius;
		public Action OnPlayerStayInDen;
		public Action OnPackMemberIssueWarning;
		//brain behavior
		public Action OnRefreshBehavior;
		public Action OnCollectiveThoughtStart;
		public Action OnCollectiveThoughtEnd;
		public CollectiveThought CurrentThought = new CollectiveThought ();
		//stunned/revived
		public Action OnStunned;
		public Action OnRevived;
		public Photosensitive photosensitive;

		public override string FileNamer (int increment)
		{
			gFileNameBuilder.Clear ();
			gFileNameBuilder.Append (State.TemplateName);
			gFileNameBuilder.Append ("-");
			gFileNameBuilder.Append (increment.ToString ());
			return gFileNameBuilder.ToString ();
		}

		public override void OnEnable ()
		{
			Template = null;
			base.OnEnable ();
		}

		public override void OnStartup ()
		{
			CheckTemplate ();
			//create body for creature
			CreatureBody body = null;
			if (!Creatures.GetBody (Template.Props.BodyName, out body)) {
				Debug.Log ("Couldn't get body in creature " + State.TemplateName);
				return;
			} else {
				GameObject newBody = GameObject.Instantiate (body.gameObject, worlditem.tr.position, Quaternion.identity) as GameObject;
				Body = newBody.GetComponent <CreatureBody> ();
			}
			//set the body's eye colors
			Body.HostileEyeColor = Color.red;
			Body.TimidEyeColor = Color.green;
			Body.AggressiveEyeColor = Color.yellow;
			Body.ScaredEyeColor = Color.white;
			//don't set the body's parent or name
			//let the body stay in the world to keep the heirarchy clean
			//Body.transform.parent = worlditem.Group.transform;
			//Body.name = worlditem.FileName + "-Body";
			//if we're dead, we need to set the body to ragdoll right away
		}

		public override void OnInitializedFirstTime ()
		{
			CheckTemplate ();
			//set the states of our looker, listener, motile etc. using our template
			//this will only happen the first time the creature is created
			//after that it will pull this info from its stack item state
			Looker looker = null;
			if (worlditem.Is <Looker> (out looker)) {
				Reflection.CopyProperties (Template.LookerTemplate, looker.State);
			}
			Listener listener = null;
			if (worlditem.Is <Listener> (out listener)) {
				Reflection.CopyProperties (Template.ListenerTemplate, listener.State);
			}
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				Reflection.CopyProperties (Template.MotileTemplate, motile.State);
			}
			FillStackContainer fsc = null;
			if (worlditem.Is <FillStackContainer> (out fsc)) {
				if (string.IsNullOrEmpty (Template.Props.InventoryFillCategory)) {
					Template.Props.InventoryFillCategory = Creatures.Get.DefaultInventoryFillCategory;
				}
				fsc.State.WICategoryName = Template.Props.InventoryFillCategory;
			}
			//all creatures are damageable
			damageable = worlditem.Get <Damageable> ();
			damageable.ApplyForceAutomatically = false;
			if (!State.IsDead) {
				Reflection.CopyProperties (Template.DamageableTemplate, damageable.State);
			}
			//add each of the custom scripts in the template
			for (int i = 0; i < Template.Props.CustomWIScripts.Count; i++) {
				worlditem.Add (Template.Props.CustomWIScripts [i]);
			}
			//and we're done!
		}

		public override void OnInitialized ()
		{	
			CheckTemplate ();

			worlditem.OnActive += OnActive;
			worlditem.OnVisible += OnVisible;
			worlditem.OnInactive += OnInactive;
			worlditem.OnPlayerEncounter += OnPlayerEncounter;
			worlditem.OnAddedToGroup += OnAddedToGroup;

			photosensitive = worlditem.GetOrAdd <Photosensitive> ();
			photosensitive.OnExposureDecrease += RefreshEyes;
			photosensitive.OnExposureDecrease += RefreshEyes;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				motile.SetBody (Body);
				//always set the motile props even if we've saved the state
				motile.State.MotileProps = Template.MotileTemplate.MotileProps;
			}
			Looker looker = null;
			if (worlditem.Is <Looker> (out looker)) {
				looker.OnSeeItemOfInterest += OnSeeItemOfInterest;
			}
			Listener listener = null;
			if (worlditem.Is <Listener> (out listener)) {
				listener.OnHearItemOfInterest += OnHearItemOfInterest;
			}
			damageable = worlditem.Get <Damageable> ();
			damageable.OnTakeDamage += OnTakeDamage;
			damageable.OnTakeCriticalDamage += OnTakeCriticalDamage;
			damageable.OnTakeOverkillDamage += OnTakeOverkillDamage;
			damageable.OnDie += OnDie;

			Container container = worlditem.Get <Container> ();
			container.CanOpen = false;
			container.CanUseToOpen = false;

			OnDaytimeStart += RefreshBehavior;
			OnNightTimeStart += RefreshBehavior;
			OnHourStart += RefreshBehavior;

			//WanderAction ();
		}

		protected void CheckTemplate () {
			if (mTemplate == null) {
				if (!Creatures.GetTemplate (State.TemplateName, out mTemplate)) {
					Debug.Log ("Couldn't get template in check template: " + State.TemplateName);
					return;
				}
			}
			State.TemplateName = mTemplate.Name;
			State.AggressiveTOD = mTemplate.StateTemplate.AggressiveTOD;
			State.Domestication = mTemplate.StateTemplate.Domestication;
			State.FightOrFlightThreshold = mTemplate.StateTemplate.FightOrFlightThreshold;
			State.MaxPursuitPastDen = mTemplate.StateTemplate.MaxPursuitPastDen;
			State.OnReachFFThresholdAggressive = mTemplate.StateTemplate.OnReachFFThresholdAggressive;
			State.OnReachFFThresholdTimid = mTemplate.StateTemplate.OnReachFFThresholdTimid;
			State.OnTakeDamageAggressive = mTemplate.StateTemplate.OnTakeDamageAggressive;
			State.OnTakeDamageTimid = mTemplate.StateTemplate.OnTakeDamageTimid;
		}

		public void OnActive ()
		{
			if (IsDead)
				return;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				motile.StartMotileActions ();
			}
		}

		public void OnVisible ()
		{
			if (IsDead)
				return;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				motile.StartMotileActions ();
			}
		}

		public void OnInactive ()
		{
			if (IsDead)
				return;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				motile.StartMotileActions ();
			}
		}

		public void OnPlayerEncounter ()
		{
			if (IsDead)
				return;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				motile.StartMotileActions ();
			}
		}

		public void OnAddedToGroup ()
		{
			//save so the creature den spawner won't get confused
			//WorldItems.Get.Save(worlditem, true);
			//initialize the body parts
			//Body.Initialize (worlditem);
			//add the body's renderers to worlditem renderers
			//so they're disabled when appropriate
			//worlditem.Renderers.AddRange (Body.Renderers);

			if (Den == null) {
				IStackOwner owner = null;
				if (worlditem.Group.HasOwner (out owner)) {
					Den = (ICreatureDen)owner.worlditem.GetComponent (typeof(ICreatureDen));
					//else - we don't have a den so we'll set default roaming distance from props
				}
			}
			if (Den != null) {
				Den.AddCreature (this.worlditem);
			}

			//set up the collective thought object
			CurrentThought.OnFleeFromIt += FleeFromThing;
			CurrentThought.OnKillIt += AttackThing;
			CurrentThought.OnEatIt += EatThing;
			CurrentThought.OnFollowIt += FollowThing;
			CurrentThought.OnWatchIt += WatchThing;
			//CurrentThought.OnMateWithIt += MateWithThing;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				//create motile behaviors
				//TODO create more logical behaviors for creatures without a den
				mFollowAction = new MotileAction ();
				mFollowAction.Name = "Follow action by Creature";
				mFollowAction.Type = MotileActionType.FollowTargetHolder;
				mFollowAction.FollowType = MotileFollowType.Follower;
				mFollowAction.Expiration = MotileExpiration.TargetOutOfRange;
				mFollowAction.OutOfRange = Den.Radius;
				mFollowAction.Range = Den.Radius;
				//mFollowAction.TerritoryType = MotileTerritoryType.Den;
				mFollowAction.TerritoryBase = Den;

				mEatAction = new MotileAction ();
				mEatAction.Name = "Eat action by Creature";
				mEatAction.Type = MotileActionType.FollowGoal;
				mEatAction.Expiration = MotileExpiration.TargetInRange;
				mEatAction.Range = Template.MotileTemplate.MotileProps.RVORadius * 2;
				//mEatAction.TerritoryType = MotileTerritoryType.Den;
				mEatAction.TerritoryBase = Den;

				mReturnToDenAction = new MotileAction ();
				mReturnToDenAction.Name = "Return to Den action by Creature";
				mReturnToDenAction.Type = MotileActionType.FollowGoal;
				mReturnToDenAction.Expiration = MotileExpiration.TargetInRange;
				mReturnToDenAction.Range = Template.MotileTemplate.MotileProps.RVORadius;
				mReturnToDenAction.LiveTarget = Den.IOI;
				//mReturnToDenAction.TerritoryType = MotileTerritoryType.Den;
				mReturnToDenAction.TerritoryBase = Den;

				mFleeThreatAction = new MotileAction ();
				mFleeThreatAction.Name = "Flee threat action by Creature";
				mFleeThreatAction.Type = MotileActionType.FleeGoal;
				mFleeThreatAction.Expiration = MotileExpiration.TargetOutOfRange;
				mFleeThreatAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
				mFleeThreatAction.OutOfRange = Den.Radius;
				mFleeThreatAction.Range = Looker.AwarenessDistanceTypeToVisibleDistance (Template.LookerTemplate.AwarenessDistance);
				//mFleeThreatAction.TerritoryType = MotileTerritoryType.Den;
				mFleeThreatAction.TerritoryBase = Den;

				mPursueGoalAction = new MotileAction ();
				mPursueGoalAction.Name = "Pursue goal action by Creature";
				mPursueGoalAction.Type = MotileActionType.FollowGoal;
				mPursueGoalAction.Expiration = MotileExpiration.TargetInRange;
				mPursueGoalAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
				mPursueGoalAction.Range = Template.MotileTemplate.MotileProps.RVORadius;
				//mPursueGoalAction.TerritoryType = MotileTerritoryType.Den;
				mPursueGoalAction.TerritoryBase = Den;

				mFocusAction = new MotileAction ();
				mFocusAction.Name = "Focus action by Creature";
				mFocusAction.Type = MotileActionType.FocusOnTarget;
				mFocusAction.Expiration = MotileExpiration.Duration;
				mFocusAction.RTDuration = ShortTermMemoryToRT (Template.Props.ShortTermMemory);
				mFocusAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
				//mFocusAction.TerritoryType = MotileTerritoryType.Den;
				mFocusAction.TerritoryBase = Den;

				mWanderAction = motile.BaseAction;
				mWanderAction.Name = "Base Action (wander) Set By Creature";
				mWanderAction.Type = MotileActionType.WanderIdly;
				mWanderAction.LiveTarget = Den.IOI;
				//mWanderAction.TerritoryType = MotileTerritoryType.Den;
				mWanderAction.TerritoryBase = Den;
				mWanderAction.Range = Den.Radius;
				mWanderAction.OutOfRange = Den.Radius;

				if (!IsDead) {
					motile.StartMotileActions ();
				}
			}

			if (IsDead) {
				motile.IsRagdoll = true;
				Body.SetRagdoll (true, 0.1f);
				Body.transform.position = worlditem.transform.position + Vector3.up;
				Body.transform.Rotate (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0f, 360f));
				OnDie ();
			} else {
				RefreshBehavior ();
			}
		}

		#region item of interest behavior

		public void Eat (FoodStuff foodStuff)
		{
			MasterAudio.PlaySound (MasterAudio.SoundType.PlayerVoiceMale, worlditem.tr, "EatFoodGeneric");
			foodStuff.worlditem.SetMode (WIMode.RemovedFromGame);
		}

		public void ThinkAboutItemOfInterest (IItemOfInterest newItemOfInterest)
		{
			if (CurrentThought.HasItemOfInterest && CurrentThought.StartedThinking) {
				if (CurrentThought.CurrentItemOfInterest != newItemOfInterest) {
					return;
				}
			}
			CurrentThought.Reset (newItemOfInterest);
			if (CurrentThought.HasItemOfInterest) {
				//this will enable FixedUpdate
				//which will start the process of thinking
				enabled = true;
			}
		}

		public void OnSeeItemOfInterest ()
		{
			Looker looker = worlditem.Get <Looker> ();
			ThinkAboutItemOfInterest (looker.LastSeenItemOfInterest);
		}

		public void OnHearItemOfInterest ()
		{
			Listener listener = worlditem.Get <Listener> ();
			ThinkAboutItemOfInterest (listener.LastHeardItemOfInterest);
		}

		public void RefreshBehavior ()
		{
			if (!mInitialized || IsDead || Template == null) {
				return;
			}

			//reset everything
			for (int i = 0; i < mMotileActions.Count; i++) {
				mMotileActions [i].TryToFinish ();
			}

			//TODO move this into a delegate so mods can link up something else
			switch (State.Domestication) {
			case DomesticatedState.Domesticated:
				//Debug.Log (Template.Name + " is domesticated, addng domesticated");
				worlditem.GetOrAdd <Domesticated> ();
				break;

			case DomesticatedState.Tamed:
				//Debug.Log (Template.Name + " is tamed, addng tamed");
				worlditem.GetOrAdd <Tamed> ();
				break;

			case DomesticatedState.Wild:
				//Debug.Log (Template.Name + " is wild, addng wild");
				worlditem.GetOrAdd <Wild> ();
				break;

			default:
			case DomesticatedState.Custom:
				break;
			}

			switch (Template.Props.Stubbornness) {
			case StubbornnessType.Untrainable:
				break;

			default:
				if (State.Domestication != DomesticatedState.Tamed) {
					worlditem.GetOrAdd <Tameable> ();
				}
				break;
			}
			OnRefreshBehavior.SafeInvoke ();
		}

		#endregion

		#region damage / death

		public void OnTakeDamage ()
		{
			if (mIsStunned || IsDead) {
				return;
			}

			Body.Animator.TakingDamage = true;
			Body.Sounds.Refresh ();
			Body.SetBloodOpacity (damageable.NormalizedDamage);

			if (State.Domestication == DomesticatedState.Wild) {
				Debug.Log (Template.Name + " is wild, calling for help on take damage");
				Den.CallForHelp (worlditem, damageable.LastDamageSource);
			}

			if (!worlditem.Is <Hostile> ()) {
				ThinkAboutItemOfInterest (damageable.LastDamageSource);
			}
		}

		public void OnTakeCriticalDamage ()
		{
			if (mIsStunned || IsDead) {
				return;
			}

			Body.Animator.TakingDamage = true;
			Body.Sounds.Refresh ();
			Body.SetBloodOpacity (damageable.NormalizedDamage);

			if (State.Domestication == DomesticatedState.Wild) {
				Debug.Log (Template.Name + " is wild, calling for help on take critical damage");
				Den.CallForHelp (worlditem, damageable.LastDamageSource);
			}

			if (!worlditem.Is <Hostile> ()) {
				ThinkAboutItemOfInterest (damageable.LastDamageSource);
			}
		}

		public void OnTakeOverkillDamage ()
		{

			if (mIsStunned || IsDead) {
				return;
			}

			Body.Sounds.Refresh ();
			Body.SetBloodOpacity (damageable.NormalizedDamage);
			if (Template.Props.StunnedByOverkillDamage) {
				TryToStun (10f);
			}
		}

		public void OnDie ()
		{
			Debug.Log ("On die in creature " + name);
			//can't be stunned while we're dead
			mIsStunned = false;

			Container container = null;
			if (worlditem.Is <Container> (out container)) {
				container.CanOpen = Template.Props.CanOpenContainerOnDie;
				container.CanUseToOpen = container.CanOpen;
				container.OpenText = Template.Props.ContainerOpenOptionText;
			}

			Body.Animator.Dead = true;
			Body.Sounds.Refresh ();
			Body.EyeMode = BodyEyeMode.Dead;
			Body.SetBloodOpacity (1f);

			if (Template.Props.DestroyBodyOnDie) {
				worlditem.Get <Motile> ().Finish ();
				GameObject.Destroy (Body.gameObject);
			}

			/*if (HasDen) {
				Den.SpawnCreatureCorpse (this);
			}*/
		}

		#endregion

		public override void PopulateRemoveItemSkills (HashSet <string> removeItemSkills)
		{
			removeItemSkills.Add ("CleanAnimal");
		}

		#region helper functions

		public void WatchThing (IItemOfInterest itemOfInterest)
		{
			WatchThingAction (itemOfInterest);
		}

		public void FollowThing (IItemOfInterest itemOfInterest)
		{
			FollowThingAction (itemOfInterest);
		}

		public void FleeFromThing (IItemOfInterest itemOfInterest)
		{
			FleeFromThingAction (itemOfInterest);
		}

		public void EatThing (IItemOfInterest itemOfInterest)
		{
			EatThingAction (itemOfInterest);
		}

		public void AttackThing (IItemOfInterest itemOfInterest)
		{
			if (Profile.Get.CurrentGame.Difficulty.IsDefined ("NoHostileCreatures")) {
				return;
			}

			Body.EyeMode = BodyEyeMode.Hostile;
			Hostile hostile = null;
			if (!worlditem.Is <Hostile> (out hostile)) {
				hostile = worlditem.GetOrAdd <Hostile> ();
				//copy the properties quickly
				Reflection.CopyProperties (Template.HostileTemplate, hostile.State);
				//copy the attacks directly
				hostile.TerritoryBase = Den;
				hostile.State.Attack1 = ObjectClone.Clone <AttackStyle> (Template.HostileTemplate.Attack1);
				hostile.State.Attack2 = ObjectClone.Clone <AttackStyle> (Template.HostileTemplate.Attack2);
				hostile.OnAttack1Start += OnAttack1;
				hostile.OnAttack2Start += OnAttack2;
				hostile.OnWarn += OnWarn;
				hostile.OnCoolOff += OnCoolOff;
			}
			if (!hostile.HasPrimaryTarget || hostile.PrimaryTarget != itemOfInterest) {
				hostile.PrimaryTarget = itemOfInterest;
			}
			hostile.RefreshAttackSettings ();
			Body.Animator.Idling = false;
		}

		public MotileAction WanderAction ()
		{
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mWanderAction == null) {
					mWanderAction = motile.BaseAction;
				}
				if (mWanderAction.State == MotileActionState.NotStarted || mWanderAction.IsFinished) {
					mWanderAction.Reset ();
					motile.PushMotileAction (mWanderAction, MotileActionPriority.ForceBase);
				}
			}
			return mWanderAction;
		}

		public MotileAction WatchThingAction (IItemOfInterest itemOfInterest)
		{
			//if the creature is idle, turn to look at the player
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mFocusAction.State == MotileActionState.NotStarted || mFocusAction.IsFinished || mFocusAction.LiveTarget != itemOfInterest) {
					mFocusAction.Reset ();
					mFocusAction.LiveTarget = itemOfInterest;
					motile.PushMotileAction (mFocusAction, MotileActionPriority.ForceTop);
				}
			}
			return mFocusAction;
		}

		public MotileAction FollowThingAction (IItemOfInterest itemOfInterest)
		{
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mFollowAction.State == MotileActionState.NotStarted || mFollowAction.IsFinished || mFollowAction.LiveTarget != itemOfInterest) {
					mFollowAction.Reset ();
					mFollowAction.LiveTarget = itemOfInterest;
					motile.PushMotileAction (mFollowAction, MotileActionPriority.ForceTop);
				}
			}
			return mFollowAction;
		}

		public MotileAction FleeFromThingAction (IItemOfInterest itemOfInterest)
		{
			Body.EyeMode = BodyEyeMode.Timid;
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mFleeThreatAction.State == MotileActionState.NotStarted || mFleeThreatAction.IsFinished || mFleeThreatAction.LiveTarget != itemOfInterest) {
					mFleeThreatAction.Reset ();
					mFleeThreatAction.LiveTarget = itemOfInterest;
					mFleeThreatAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					motile.PushMotileAction (mFleeThreatAction, MotileActionPriority.ForceTop);
				}
			}
			return mFleeThreatAction;
		}

		public MotileAction EatThingAction (IItemOfInterest itemOfInterest)
		{
			mLastThingTryToEat = itemOfInterest;
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mEatAction.State == MotileActionState.NotStarted || mEatAction.IsFinished || mEatAction.LiveTarget != itemOfInterest) {
					mEatAction.Reset ();
					mEatAction.LiveTarget = itemOfInterest;
					mEatAction.Range = Template.MotileTemplate.MotileProps.RVORadius;
					mEatAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					mEatAction.OnFinishAction += OnReachThingToEat;
					motile.PushMotileAction (mEatAction, MotileActionPriority.ForceTop);
				}
			}
			return mEatAction;
		}

		public void OnReachThingToEat ()
		{
			if (mLastThingTryToEat != null) {
				AttackThing (mLastThingTryToEat);
			}
		}

		public void OnAttack1 ()
		{
			Body.EyeMode = BodyEyeMode.Hostile;
			Body.Animator.Attack1 = true;
			Body.Sounds.Refresh ();
		}

		public void OnAttack2 ()
		{
			Body.EyeMode = BodyEyeMode.Hostile;
			Body.Animator.Attack2 = true;
			Body.Sounds.Refresh ();
		}

		public void OnWarn ()
		{
			Body.EyeMode = BodyEyeMode.Hostile;
			Body.Animator.Warn = true;
			Body.Sounds.Refresh ();
			OnPackMemberIssueWarning.SafeInvoke ();
		}

		public void RefreshEyes ()
		{
			if (WorldClock.IsNight) {
				Body.TargetEyeBrightness = Mathf.Max (photosensitive.LightExposure, 0.05f);
			} else {
				Body.TargetEyeBrightness = 0.05f;
			}
		}

		public void OnCoolOff ()
		{
			Body.EyeMode = BodyEyeMode.Aggressive;
			Body.Animator.Idling = true;
		}

		public MotileAction FleeFromFire ()
		{
			Body.EyeMode = BodyEyeMode.Scared;
			Motile motile = null;
			if (photosensitive.HasNearbyFires && worlditem.Is <Motile> (out motile)) {
				Fire nearestFire = photosensitive.NearestFire;
				if (mFleeThreatAction.IsFinished || mFleeThreatAction.LiveTarget != nearestFire) {
					mFleeThreatAction.Reset ();
					mFleeThreatAction.LiveTarget = nearestFire.FireLight;
					mFleeThreatAction.Range = nearestFire.FireScale;
					mFleeThreatAction.OutOfRange = nearestFire.FireScale + 1f;
					mFleeThreatAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					motile.PushMotileAction (mFleeThreatAction, MotileActionPriority.ForceTop);
				}
			}
			return mFleeThreatAction;
		}

		public MotileAction FleeFromLight ()
		{
			Body.EyeMode = BodyEyeMode.Scared;
			Motile motile = null;
			if (photosensitive.HasNearbyLights && worlditem.Is <Motile> (out motile)) {
				WorldLight nearestLight = photosensitive.NearestLight;
				if (mFleeThreatAction.IsFinished || mFleeThreatAction.LiveTarget != nearestLight) {
					mFleeThreatAction.Reset ();
					mFleeThreatAction.LiveTarget = nearestLight;
					mFleeThreatAction.Range = nearestLight.TargetBaseRange;
					mFleeThreatAction.OutOfRange = nearestLight.TargetBaseRange + 1f;
					mFleeThreatAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					motile.PushMotileAction (mFleeThreatAction, MotileActionPriority.ForceTop);
				}
			}
			return mFleeThreatAction;
		}

		public MotileAction ReturnToDen ()
		{
			if (mReturnToDenAction == null)
				return null;

			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mReturnToDenAction.IsFinished) {
					mReturnToDenAction.Reset ();
					mReturnToDenAction.Range = Den.Radius;
					mReturnToDenAction.LiveTarget = Den.IOI;
					mReturnToDenAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					motile.PushMotileAction (mReturnToDenAction, MotileActionPriority.ForceTop);
				}
			}
			return mReturnToDenAction;
		}

		public MotileAction ReturnToDenInnerRadius ()
		{
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				if (mReturnToDenAction.IsFinished) {
					mReturnToDenAction.Reset ();
					mReturnToDenAction.Expiration = MotileExpiration.TargetInRange;
					mReturnToDenAction.Range = Den.InnerRadius;
					mReturnToDenAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
					motile.PushMotileAction (mReturnToDenAction, MotileActionPriority.ForceTop);
				}
			}
			return mReturnToDenAction;
		}

		public void StopFleeingFromPlayer ()
		{
			if (mFleeThreatAction.HasStarted && mFleeThreatAction.LiveTarget == Player.Local) {
				mFleeThreatAction.TryToFinish ();
			}
		}

		public void StopFleeingFromThing ()
		{
			if (!mFleeThreatAction.IsFinished) {
				mFleeThreatAction.TryToFinish ();
			}
		}

		#endregion

		#region stun / revive

		public bool IsDead {
			get {
				if (damageable == null) {
					return State.IsDead;
				} else if (damageable.State.IsDead) {
					State.IsDead = true;
				} else {
					State.IsDead = false;
				}
				return State.IsDead;
			}
		}

		public bool IsStunned {
			get {
				return mIsStunned;
			}
		}

		public void TryToStun (float stunRTDuration)
		{
			//we can be stunned if we're not dead and not already stunned
			if (mIsStunned || IsDead) {
				return;
			}

			mIsStunned = true;
			StartCoroutine (StunnedOverTime (stunRTDuration));
			OnStunned.SafeInvoke ();
		}

		public void TryToRevive ()
		{
			//doesn't matter if we're dead, we shouldn't be stunned anyway
			mIsStunned = false;
		}

		public void FixedUpdate ()
		{
			if (Template == null)
				return;

			if (mIsStunned) {
				CurrentThought.Reset ();
				enabled = false;
				return;
			}

			if (!CurrentThought.HasItemOfInterest) {
				//CurrentThought.Reset ();
				enabled = false;
				return;
			}

			if (!CurrentThought.StartedThinking) {
				CurrentThought.StartThinking (WorldClock.AdjustedRealTime);
				OnCollectiveThoughtStart.SafeInvoke ();
			} else if (!CurrentThought.IsFinishedThinking (ShortTermMemoryToRT (Template.Props.ShortTermMemory))) {
				//let it keep thinking
				return;
			} else {
				//end the thought
				//use the results to call an action
				//then disable this script so FixedUpdate is no longer called
				OnCollectiveThoughtEnd.SafeInvoke ();
				CurrentThought.TryToSendThought ();
				//CurrentThought.Reset ();
				enabled = false;
			}
		}

		public static float ShortTermMemoryToRT (ShortTermMemoryLength shortTermMemory)
		{
			float shortTermMemoryTime = 1.0f;
			switch (shortTermMemory) {
			case ShortTermMemoryLength.Short:
				shortTermMemoryTime = 1.0f;
				break;
				
			case ShortTermMemoryLength.Medium:
				shortTermMemoryTime = 10.0f;
				break;
				
			case ShortTermMemoryLength.Long:
				shortTermMemoryTime = 30.0f;
				break;
				
			default:
				break;
			}
			return shortTermMemoryTime;
		}

		protected IEnumerator StunnedOverTime (double RTDuration)
		{
			//don't think about stuff in the meantime
			enabled = false;
			Motile motile = null;
			if (worlditem.Is <Motile> (out motile)) {
				//stop doing stuff in the meantime
				motile.IsRagdoll = true;
			}

			double reviveTime = WorldClock.AdjustedRealTime + RTDuration;
			while (mIsStunned && WorldClock.AdjustedRealTime < reviveTime) {
				double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 1f;
				while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
					yield return null;
				}
			}
			//if we're not dead
			if (!IsDead) {
				if (motile != null) {
					//start doing stuff again
					motile.IsRagdoll = false;
				}
				OnRevived.SafeInvoke ();
			}
			mIsStunned = false;
			yield break;
		}

		protected bool mIsStunned = false;

		#endregion

		#region light

		public void OnExposureIncrease ()
		{
			RefreshEyes ();
		}

		public void OnExposureDecrease ()
		{
			RefreshEyes ();
		}

		#endregion

		protected Location mLastLocationEntered;
		protected Vector3 mPositionLastFrame;
		protected Vector3 mLocalVelocityLastFrame;
		protected Vector3 mGlobalVelocityLastFrame;
		protected bool mIsInDen = false;
		protected bool mIsInDenInnerRadius = false;
		protected MotileAction mReturnToDenAction = null;
		protected MotileAction mPursueGoalAction = null;
		protected MotileAction mFleeThreatAction = null;
		protected MotileAction mFocusAction = null;
		protected MotileAction mFollowAction = null;
		protected MotileAction mEatAction = null;
		protected MotileAction mWanderAction = null;
		protected List <MotileAction> mMotileActions = new List <MotileAction> ();
		protected IItemOfInterest mLastThingTryToEat;
	}

	[Serializable]
	public class CreatureState
	{
		public List <string> BodyAccessories = new List <string> ();
		public List <string> ActionNodesVisited = new List <string> ();
		public MobileReference CreatureDen = new MobileReference ();
		public float MaxPursuitPastDen = 0.0f;
		public string PackTag = "Pack";
		public string TemplateName = string.Empty;
		public float FightOrFlightThreshold = 0.5f;
		public bool IsDead = false;
		public BehaviorTOD AggressiveTOD = BehaviorTOD.All;
		public FightOrFlight OnTakeDamageAggressive = FightOrFlight.Fight;
		public FightOrFlight OnReachFFThresholdAggressive = FightOrFlight.Flee;
		public FightOrFlight OnTakeDamageTimid = FightOrFlight.Flee;
		public FightOrFlight OnReachFFThresholdTimid = FightOrFlight.Flee;
		public DomesticatedState Domestication = DomesticatedState.Wild;
	}
}
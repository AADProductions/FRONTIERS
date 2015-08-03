using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Pathfinding;
using Pathfinding.RVO;
using ExtensionMethods;
using Frontiers.GUI;

namespace Frontiers.World.WIScripts
{
	//TODO: Implement GetComponent<TNObject> ().isMine
	public class Motile : WIScript, IBodyOwner
	{
		public MotileState State = new MotileState ();
		//Motile's job is to move the character's Goal around in ways that make sense
		//Motile also ensures that old actions are completed before new actions are undertaken
		//if there's a problem with character / creature movement
		//you'll find it in here
		[NObjectSync]//used to determine which options appear in option list
		public MotileInstructions CurrentInstructions {
			get {
				//update our motile instructions if we're the brain
				if (worlditem.IsNObject && worlditem.NObject.isMine) {
					if (State.Actions.Count > 0) {
						if (State.Actions [0].Instructions == MotileInstructions.InheritFromBase) {
							mMotileInstructions = State.BaseAction.Instructions;
						} else {
							mMotileInstructions = State.Actions [0].Instructions;
						}
					}
				}
				return mMotileInstructions;
			}
			set {
				mMotileInstructions = value;
			}
		}

		#region IBodyOwner implementation

		[NObjectSync]//used for stuff other than animation
		public double CurrentMovementSpeed {
			get {
				return mCurrentMovementSpeed;
			}
			set {
				mCurrentMovementSpeed = value;
			}
		}

		public double CurrentRotationSpeed {
			get {
				return mCurrentMovementSpeed;
			}
			set {
				mCurrentMovementSpeed = value;
			}
		}

		public double CurrentRotationChangeSpeed;

		public int CurrentIdleAnimation { get; set; }

		public Vector3 Position {
			get {
				return mPosition;
			}
			set {
				if (!enabled) {
					mTr.position = value;
				}
			}
		}

		public Quaternion Rotation { get { return mVisibleRotation; } }

		public WorldBody Body { get; set; }

		public override bool Initialized { get { return mInitialized; } }

		public bool IsImmobilized {
			get {
				return mIsImmobilized;
			}
			set {
				if (!mInitialized || !HasBody)
					return;

				mIsImmobilized = value;
			}
		}

		protected bool mIsImmobilized = true;

		public bool IsGrounded {
			get {
				if (!mInitialized) {
					return true;
				}
				if (!worlditem.Is (WIActiveState.Active)) {
					return true;
				}
				return terrainHit.isGrounded || State.MotileProps.Hovers;
			}
			set { terrainHit.isGrounded = value; }
		}

		public bool IsRagdoll { get; set; }

		public bool IsDead { get; set; }

		public bool UseGravity { get; set; }

		public bool ForceWalk { get; set; }

		public int IndleAnimation { get; set; }

		public bool IsKinematic {
			get {
				//making this super clear
				if (!mInitialized || !enabled || IsImmobilized)
					return true;

				if (State.MotileProps.Hovers)
					return false;

				if (State.MotileProps.UseKinematicBody)
					return true;

				if (!worlditem.Is (WIActiveState.Active)) 
					return true;

				return false;
			}
		}

		#endregion
		public bool AvoidingObstacle { 
			get {
				return WorldClock.AdjustedRealTime < mAvoidObstaclesUntil;
			} set {
				if (value) {
					mAvoidObstaclesUntil = -1f;
				} else {
					mAvoidObstaclesUntil = WorldClock.AdjustedRealTime + 1f;
				}
			}
		}
		protected double mAvoidObstaclesUntil;
		public double TargetMovementSpeed = 0.0f;
		public Transform GoalObject = null;
		public Vector3 GoalDirection;
		public Vector3 LookDirection;
		public Vector3 ForceDirection;
		public float GoalDistance;

		public bool HasGoalHolder {
			get {
				return GoalHolder != null && !GoalHolder.IsDestroyed;
			}
		}

		public RVOTargetHolder GoalHolder = null;
		public float AdjustedYPosition = 0f;
		//public RVOController rvoController;
		public ActionNode LastOccupiedNode = null;
		public float ThrowbackSpeed = 10f;
		public float JumpForce;

		[NonSerialized]
		[HideInInspector]
		public MotileAction LastFinishedAction = null;

		public bool HasBody {
			get {
				//this is like motile's version of mInitialized
				//since we have to wait till post-OnInitialized to receive our body
				return Body != null;
			}
		}

		public bool HasReachedGoal (double minRange)
		{
			return GoalDistance < minRange;
		}

		#region initialization

		public override void Awake ()
		{
			mTr = transform;
			terrainHit.isGrounded = true;
			base.Awake ();
		}

		public override void OnStartup ()
		{
			//rvoController = gameObject.GetComponent <RVOController> ();
			//just disable to the controller until we're ready
			//rvoController.enabled = false;
			//set immobilized to true
			//this will prevent anything from moving before we're ready
			mIsImmobilized = true;
			//create goal objects, focus objects etc.
			//we will only need these if we're the brain
			//put the goal object out in the world, not in the group
			GoalObject = new GameObject (worlditem.FileName + "-GoalObject").transform;
			State.BaseAction.State = MotileActionState.NotStarted;
			State.BaseAction.BaseAction = true;
		}

		public void SetBody (WorldBody body)
		{
			Body = body;
			//wait to actually spawn it until on added to group
			//that will zap the body to the correct position
		}

		public override void OnInitialized ()
		{
			worlditem.OnAddedToGroup += OnAddedToGroup;
			//don't subscribe to on visible / active in here
			//because we don't want those messages until we have a body
			//and that won't happen until we've been added to a group
			State.BaseAction.BaseAction = true;
			State.BaseAction.HudIcon = Icon.Empty;
			State.BaseAction.Expiration = MotileExpiration.Never;
			State.BaseAction.Instructions = MotileInstructions.None;
		}

		public void OnAddedToGroup ()
		{
			if (mFinished)
				return;

			if (worlditem.IsNObject && worlditem.NObject.isMine) {
				//worlditem.OnActive += OnActive;
				//worlditem.OnVisible += OnVisible;
				//worlditem.OnInactive += OnInactive;
				worlditem.OnInvisible += OnInvisible;
				//worlditem.OnPlayerEncounter += OnPlayerEncounter;

				TargetMovementSpeed = 0f;
				CurrentRotationChangeSpeed = 1f;

				mPosition = mTr.position;
				AdjustedYPosition = mPosition.y;
				//make sure we have something to do
				GoalObject.position = mPosition + (mTr.forward * 0.5f);

				terrainHit.overhangHeight = Globals.DefaultCharacterHeight;
				terrainHit.groundedHeight = Mathf.Max (Globals.DefaultCharacterGroundedHeight, State.MotileProps.GroundedHeight) * 2f;
				terrainHit.hitTerrain = true;
				terrainHit.feetPosition = mPosition;
				IsGrounded = true;

				Damageable damageable = null;
				if (worlditem.Is <Damageable> (out damageable)) {
					damageable.OnDie += OnDie;
					//we have to handle force on our own
					damageable.ApplyForceAutomatically = false;
					damageable.OnForceApplied += OnForceApplied;
				}

			} else {
				//initialize this motile script as a shell
				//it won't perform any actions on its own
				//but players will still be able to interact with it and give it commands
				//rvoController.canSearch = false;
				//TODO figure out what else to initialize
				//TODO figure out a way to send MotileActions across the network
			}

			if (!Body.HasSpawned) {
				bool spawnBodyNow = true;
				if (worlditem.Group.IsStructureGroup) {
					//wait for the structure to finish building before moving the creature
					if (worlditem.Group.Props.Interior) {
						if (!worlditem.Group.ParentStructure.Is (StructureLoadState.InteriorLoaded)) {
							spawnBodyNow = false;
							StartCoroutine (WaitToSpawnBodyInStructure (worlditem.Group.ParentStructure, StructureLoadState.InteriorLoaded));
						}
					} else if (!worlditem.Group.ParentStructure.Is (StructureLoadState.ExteriorLoaded)) {
						spawnBodyNow = false;
						StartCoroutine (WaitToSpawnBodyInStructure (worlditem.Group.ParentStructure, StructureLoadState.ExteriorLoaded));
					}
				}

				if (spawnBodyNow) {
					//Debug.Log ("Spawning body in " + name);
					//set immobilized to false and start updating everything
					IsImmobilized = false;
					//spawn the body so it will zap to our position
					Body.OnSpawn (this);
					Body.Initialize (worlditem);
				}
			}
		}

		public override void BeginUnload ()
		{		//don't fall through the ground, etc
			enabled = false;
		}

		public void StartMotileActions ()
		{
			if (mDestroyed || mFinished || !mInitialized)
				return;

			if (!mDoingActionsOverTime) {
				mDoingActionsOverTime = true;
				StartCoroutine (DoActionsOverTime ());
			}

			enabled = true;
		}

		public void StopMotileActions ()
		{
			//Debug.Log ("Stopping motile actions...");
			//this will timeout on its own now
			mDoingActionsOverTime = false;

			while (mNewActions.Count > 0) {
				KeyValuePair <MotileActionPriority, MotileAction> newAction = mNewActions.Dequeue ();
				newAction.Value.State = MotileActionState.Error;
				newAction.Value.Error = MotileActionError.MotileIsDead;
			}
			if (State.Actions.Count > 0) {
				for (int i = State.Actions.Count - 1; i >= 0; i--) {
					State.Actions [i].State = MotileActionState.Error;
					State.Actions [i].Error = MotileActionError.MotileIsDead;
				}
			}
			State.Actions.Clear ();
			if (worlditem.Is (WIActiveState.Invisible)) {
				enabled = false;
			}
		}

		protected IEnumerator WaitToSpawnBodyInStructure (Structure parentStructure, StructureLoadState loadState)
		{
			yield return null;
			//Debug.Log ("Waiting to spawn body in structure....");
			while (!Body.IsInitialized) {
				yield return null;
				if (parentStructure == null) {
					//Debug.Log ("Warning: parent structure null while waiting to spawn body");
					yield break;
				} else if (parentStructure.Is (loadState)) {
					//set immobilized to false and start updating everything
					IsImmobilized = false;
					//spawn the body so it will zap to our position
					Body.OnSpawn (this);
					Body.Initialize (worlditem);
				}
			}
			yield break;
		}

		public IEnumerator DoActionsOverTime ()
		{
			//give the world time to catch up with us
			while (!Initialized) {
				//Debug.Log("Not initialized, returning");
				yield return null;
			}

			while (!mFinished && mDoingActionsOverTime) {
	
				//keep going until stop motile actions is called or the script is finished
				MotileAction topAction = TopAction;//keep a copy of the current top action
				while (mNewActions.Count > 0) {
					//handle new actions first
					mNextNewAction = mNewActions.Dequeue ();
					if (IsDead) {
						mNextNewAction = mNewActions.Dequeue ();
						mNextNewAction.Value.State = MotileActionState.Error;
						mNextNewAction.Value.Error = MotileActionError.MotileIsDead;
						continue;
					}
					MotileActionPriority priority = mNextNewAction.Key;
					MotileAction action = mNextNewAction.Value;
					if (action != null) {
						//Debug.Log("Got new action " + action.Name + " with num actions: " + State.Actions.Count.ToString());
						//check the priority of the top action
						if (!topAction.BaseAction) {
							//if we have actions that AREN'T the base action
							switch (priority) {	//check the priority of the new action against the
							//top action's yield setting
							case MotileActionPriority.ForceBase:
								//top action doesn't matter, push it to the base
								//we don't have to finish the current base action
								//because it's not active
								State.BaseAction.CopyFrom (action);
								//we also don't bother to start it because it's at the base
								break;

							case MotileActionPriority.ForceTop:
								switch (topAction.YieldBehavior) {
								case MotileYieldBehavior.DoNotYield:
									//the top action won't let go
									//see if the interrupt behavior will let this action be stored
									switch (action.YieldBehavior) {
									case MotileYieldBehavior.YieldAndFinish:
									case MotileYieldBehavior.DoNotYield:
										//well crap, looks like motile action failed
										//don't add it anywhere
										action.State = MotileActionState.Error;
										action.Error = MotileActionError.PriorityConflict;
										break;

									case MotileYieldBehavior.YieldAndWait:
									default:
										//put it in the normal place, above the base action
										//interrupt the action - if it's not supposed to reset it may expire
										Characters.InterruptAction (this, topAction);
										State.Actions.Add (action);
										break;
									}
									break;

								case MotileYieldBehavior.YieldAndFinish:
									//if the top action will finish,
									//wait while we finish the top action
									Characters.FinishAction (this, topAction);
									//then add the next action
									State.Actions.Insert (0, action);
									break;

								case MotileYieldBehavior.YieldAndWait:
								default:
									//if the top action will wait, interrupt it
									Characters.InterruptAction (this, topAction);
									State.Actions.Insert (0, action);
									break;
								}
								break;

							case MotileActionPriority.Next:
								//insert it *before* the top
								if (State.Actions.Count > 0) {	//if we actually have a top action (0), insert it after the action
									State.Actions.Insert (1, action);
								} else {	//otherwise just add it normally
									State.Actions.Add (action);
								}
								break;


							case MotileActionPriority.Normal:
							default:
								//insert at the back
								State.Actions.Add (action);
								break;
							}
						} else {	//otherwise
							switch (priority) {
							case MotileActionPriority.ForceBase:
								//we have to finish the base action
								State.BaseAction.CopyFrom (action);
								break;

							case MotileActionPriority.ForceTop:
							case MotileActionPriority.Normal:
								//add the action to the regular queue
								State.Actions.Add (action);
								break;
							}
						}
						//Debug.Log("Num actions is now: " + State.Actions.Count.ToString());
					}
					yield return null;
				}
				//once we're done dealing with new actions
				//remove any actions that are finished or expired
				mActions.Clear ();
				mActions.AddRange (State.Actions);
				for (int i = mActions.Count - 1; i >= 0; i--) {
					MotileAction checkAction = mActions [i];
					if (checkAction == null
						|| checkAction.State == MotileActionState.Finished
						|| checkAction.State == MotileActionState.Error) {	//get rid of it
						//Debug.Log (checkAction.Name + " action is finished or error, removing");
						mActions.RemoveAt (i);
					}
				}
				State.Actions.Clear ();
				State.Actions.AddRange (mActions);
				//now that we're sure our top action is legitimate
				//get the new top action
				topAction = TopAction;
				//check to see if the action has been asked to finish externally
				if (topAction.FinishCalledExternally) {	//if finish was called externally deal with that now
					//this should only get called once, since FinishAction sets
					//to state 'Finishing' immediately
					//Debug.Log ("Finish called externally on top action " + topAction.Name);
					Characters.FinishAction (this, topAction);
				} else {//otherwise deal with the action by state
					switch (topAction.State) {
					case MotileActionState.NotStarted:
					case MotileActionState.Waiting:
						//removing a finished top action may have opened up a waiting action
						//wait for the top action to start before moving on
						while (!IsGrounded) {
							yield return null;
						}
						//Debug.Log ("Top action not started, starting now");
						Characters.StartAction (this, topAction);
						break;

					case MotileActionState.Started:
						//if the action has started, update it
						//then check to see if it has expired
						if (Characters.UpdateExpiration (this, topAction)) {
							//Debug.Log ("Expiration finished on top action " + topAction.Name);
							Characters.FinishAction (this, topAction);
						} else {
							if (topAction.UpdateCoroutine == null || !topAction.UpdateCoroutine.MoveNext ()) {
								Characters.GetUpdateCoroutine (this, topAction);
							}
							yield return topAction.UpdateCoroutine.Current;
						}
						break;

					default:
						//if we're finished/finishing, or still starting
						//don't do anything, this will be resolved in the next loop
						break;
					}
				}
				//wait a tick
				while (WorldClock.SkippingAhead) {
					yield return null;
				}
				yield return null;
			}
			//we're dead, blearg
			yield break;
		}

		public override void OnEnable ()
		{
			//make sure to keep this!
			base.OnEnable ();

			if (!HasBody || !mInitialized)
				return;

			Body.SetVisible (true);
			Body.IgnoreCollisions (false);
			/*if (IsGrounded) {
				rvoController.SetEnabled (true);
			}*/
		}

		public void OnDisable ()
		{
			if (!HasBody || !mInitialized)
				return;

			Body.SetVisible (false);
			Body.IgnoreCollisions (true);
			//rvoController.SetEnabled (false);
		}

		public override void OnFinish ()
		{
			/*if (rvoController != null) {
				rvoController.SetEnabled (false);
			}*/
			if (GoalObject != null) {
				GameObject.Destroy (GoalObject.gameObject);
			}
		}

		public void OnInvisible ()
		{
			StopMotileActions ();
		}

		public Vector3 LastKnownPosition {
			get {
				return mPosition;
			}
		}
		#endregion

		public void OnForceApplied ()
		{
			Damageable damageable = worlditem.Get <Damageable> ();
			ForceDirection = damageable.State.LastDamageForce;
		}
		//what we're doing right now - the top item on the MotileAction 'stack'
		public MotileAction TopAction {
			get {
				if (State.Actions.Count > 0) {
					return State.Actions [0];//lowest to highest
				} else {
					State.BaseAction.BaseAction = true;//set this first
					return State.BaseAction;
				}
			}
		}

		public MotileAction BaseAction {
			get {
				return State.BaseAction;
			}
		}

		#region commands

		//do something! priority tells us whether to put this at the front or back of the action stack
		public bool PushMotileAction (MotileAction newAction, MotileActionPriority priority)
		{
			if (IsDead) {
				//Debug.Log("Motile thing is dead, cancelling");
				newAction.State = MotileActionState.Error;
				newAction.Error = MotileActionError.MotileIsDead;
				return false;
			}

			if (newAction.State != MotileActionState.NotStarted) {
				//Debug.Log("State wasn't 'not started'");
				//interesting...
				return false;
			}

			/*if (worlditem.IsNObject && !worlditem.NObject.isMine) {
				//TODO send this motile action to the server somehow
				return true;
			}*/

			//otherwise handle it locally
			if (worlditem.Is (WIMode.RemovedFromGame)) {
				newAction.State = MotileActionState.Error;
				newAction.Error = MotileActionError.MotileIsDead;
				return false;
			}

			if (State.Actions.Contains (newAction)) {
				//Debug.Log("Already contains action, not adding");
				return false;
			} else {
				var newActionsEnum = mNewActions.GetEnumerator ();
				while (newActionsEnum.MoveNext ()) {
					if (newActionsEnum.Current.Value == newAction) {
						//Debug.Log("New actions aready contains action, not adding");
						return false;
					}
				}
			}

			//if it's our base action		
			newAction.WTAdded = WorldClock.AdjustedRealTime;
			newAction.State = MotileActionState.NotStarted;
			newAction.BaseAction = false;//just in case

			if (!worlditem.Is (WILoadState.Initialized)) {
				//Debug.Log ("Not initialized, putting in queue");
				mNewActions.Enqueue (new KeyValuePair <MotileActionPriority, MotileAction> (priority, newAction));
			} else {
				if (newAction == State.BaseAction) {
					//Debug.Log("Already base action, not adding");
					//we can't add our own base action, dimwit
					return false;
				} else if (State.Actions.Count > 0) {//check and see if any of these actions are 'duplicate'
					for (int i = State.Actions.Count - 1; i >= 0; i--) {//if it's the same type and live target
						//treat it as the same action
						//remove the existing action and push the new action
						MotileAction existingAction = State.Actions [i];
						if (existingAction == newAction) {	//whoops! it's already in there
							//Debug.Log ("Action is already in here, not adding");
							return true;
						}
						//TODO figure out some sensible replacement rules
						//} else if (existingAction.Type == newAction.Type
						//&&	existingAction.LiveTarget	== newAction.LiveTarget) {
						//newAction.State = MotileActionState.Error;
						//newAction.Error = MotileActionError.Replaced;
						//existingAction.Expiration = newAction.Expiration;
						//existingAction.Target = newAction.Target;
						//existingAction.LiveTarget	= newAction.LiveTarget;
						//}
					}
				}
				//Debug.Log("Enqueued " + newAction.Name);
				mNewActions.Enqueue (new KeyValuePair <MotileActionPriority, MotileAction> (priority, newAction));
			}
			return true;
		}

		public void TryToFinishMotileAction (MotileAction existingAction)
		{
			MotileAction topAction = TopAction;
			if (topAction.Type == existingAction.Type
				&& topAction.LiveTarget == existingAction.LiveTarget) {
				topAction.TryToFinish ();
			}
		}

		public void GoToActionNode (string actionNodeName)
		{
			WorldChunk chunk = worlditem.Group.GetParentChunk ();
			ActionNodeState actionNodeState = null;
			if (chunk.GetNode (actionNodeName, true, out actionNodeState)) {
				if (actionNodeState.actionNode.TryToReserve (worlditem)) {
					StopMotileActions ();
					MotileAction newAction = new MotileAction ();
					//TEMP
					newAction.Method = MotileGoToMethod.StraightShot;
					newAction.Type = MotileActionType.GoToActionNode;
					newAction.Target = new MobileReference (actionNodeState.Name, actionNodeState.ParentGroupPath);
					//newAction.LiveTarget = actionNodeState.actionNode.gameObject;
					newAction.Expiration = MotileExpiration.Never;

					PushMotileAction (newAction, MotileActionPriority.ForceTop);
				}
			}
		}

		#endregion

		#region interaction / state changes

		/*public override int OnRefreshHud(int lastHudPriority)
				{
						//if (!TopAction.HudIcon.IsEmpty) {
						//GUIHudElement element = hud.GetOrAddElement (HudElementType.Icon, "MotileIcon");
						//element.Initialize (TopAction.HudIcon);
						//} else {
						//	hud.RemoveElement ("MotileIcon");
						//}

						//if (TopAction.Instructions == MotileInstructions.CompanionInstructions) {
						//	hud.GetPlayerAttention = true;
						//} else {
						//	hud.GetPlayerAttention = false;
						//}
				}*/

		public override void PopulateOptionsList (List <WIListOption> options, List <string> message)
		{
			//TODO get options list from server object?
			MotileAction topAction = TopAction;
			if (topAction.Instructions == MotileInstructions.None || !worlditem.HasPlayerAttention) {//nothing we can do here
				return;
			}

			WIListOption followMeOption = null;
			WIListOption waitHereOption = null;

			switch (CurrentInstructions) {
			case MotileInstructions.CompanionInstructions:
				if (topAction.Type == MotileActionType.FollowTargetHolder && topAction.LiveTarget == Player.Local) {
					waitHereOption = new WIListOption ("SkillIconGuildFollowTrail", "Stop Following", "Wait Here");
					waitHereOption.NegateIcon	= true;
					options.Add (waitHereOption);
				} else {
					followMeOption = new WIListOption ("SkillIconGuildFollowTrail", "Follow Me", "Follow Me");
					options.Add (followMeOption);
				}
				break;

			case MotileInstructions.PilgrimInstructions:
				waitHereOption = new WIListOption ("SkillIconGuildFollowTrail", "Wait Here", "Wait Here");
				waitHereOption.NegateIcon = true;
				options.Add (waitHereOption);
				break;

			default:
				break;
			}
		}

		public void OnPlayerUseWorldItemSecondary (object secondaryResult)
		{
			WIListResult dialogResult = secondaryResult as WIListResult;

			MotileAction action = null;
			switch (dialogResult.SecondaryResult) {
			case "Follow Me":
				action = new MotileAction ();
				action.Type = MotileActionType.FollowTargetHolder;
				action.YieldBehavior = MotileYieldBehavior.YieldAndWait;
				action.FollowType = MotileFollowType.Follower;
				action.LiveTarget = Player.Local;
				action.Expiration = MotileExpiration.TargetOutOfRange;
				action.OutOfRange = 50.0f;
				action.Target.FileName	= "[Player]";
				break;

			case "Stop Following":
				if (TopAction.Type == MotileActionType.FollowTargetHolder
				    &&	TopAction.LiveTarget == Player.Local) {
					TopAction.TryToFinish ();
				}
				break;

			case "Wait Here":
				action = new MotileAction ();
				action.Type = MotileActionType.Wait;
				action.YieldBehavior	= MotileYieldBehavior.YieldAndWait;
				action.FollowType = MotileFollowType.Follower;
				action.LiveTarget = Player.Local;
				action.Expiration = MotileExpiration.Never;
				action.Target.FileName	= "[Player]";
				break;

			default:
				break;
			}

			if (action != null) {
				PushMotileAction (action, MotileActionPriority.ForceTop);
			}
		}

		public void OnDie ()
		{
			StopMotileActions ();
			IsDead = true;
			Debug.Log ("Setting dead in motile");
			//IsRagdoll = true;
		}

		#endregion

		#region Update / Falling / Landing

		protected int mCheckTerrainHeight = 0;
		protected int mCheckTopActionAnimation = 0;
		protected int mCheckUpdate = 0;

		public void Update ()
		{
			if (!GameManager.Is (FGameState.InGame))//don't update while paused
				return;

			if (WorldClock.SkippingAhead)
				return;

			if (!Initialized || !HasBody || IsDead)
				return;

			//if (worlditem.IsNObject && !worlditem.NObject.isMine)
			//return;

			if (!IsKinematic) {
				//mVisibleRotation = rvoController.targetRotation;
				//} else {
				if (Body.Velocity.magnitude > gMinLookDirMagnitude) {
					if (Body.LookDirection != Vector3.zero) {
						mVisibleRotation = Quaternion.Slerp (mVisibleRotation, Quaternion.LookRotation (Body.LookDirection), (float)(WorldClock.ARTDeltaTime * CurrentRotationSpeed));
					}
				} else {
					if (mDesiredLookDirection != Vector3.zero) {
						mVisibleRotation = Quaternion.Slerp (mVisibleRotation, Quaternion.LookRotation (mDesiredLookDirection), (float)(WorldClock.ARTDeltaTime * CurrentRotationSpeed));
					}
				}
			}
			CurrentMovementSpeed = WorldClock.Lerp (CurrentMovementSpeed, TargetMovementSpeed, WorldClock.ARTDeltaTime * State.MotileProps.MovementChangeSpeed);
		}

		public void FixedUpdate ()
		{
			if (!GameManager.Is (FGameState.InGame))//don't update while paused
				return;

			if (WorldClock.SkippingAhead)
				return;

			if (!Initialized || !HasBody || IsDead || mFinished || worlditem.Group == null)
				return;

			if (GameWorld.Get.ActiveTerrainType != worlditem.Group.Props.TerrainType) {
				UseGravity = false;
				return;
			}

			WorldChunk wc = worlditem.Group.GetParentChunk ();
			if (wc == null || !wc.HasCollider) {
				UseGravity = false;
				return;
			}

			//if (worlditem.IsNObject && !worlditem.NObject.isMine)
			//return;

			//get the latest position from our transform
			//set the feet position for our Y update
			mPosition = mTr.position;
			terrainHit.feetPosition = mPosition;
			if (State.MotileProps.Hovers) {
				//ignore worlditems so we don't zoom into the sky
				terrainHit.ignoreWorldItems = true;
			}

			//we're calling this a lot so store it as a bool
			bool isKinematic = IsKinematic;

			UseGravity = !State.MotileProps.Hovers;

			if (IsImmobilized) {
				IsGrounded = true;
				mLastGroundedTime = WorldClock.AdjustedRealTime;
				mLastGroundedHeight = mPosition.y;
				//setting this to false isn't overkill just do it every frame
				//rvoController.SetEnabled (false);
				return;
			} else if (IsRagdoll) {
				//ragdoll prohibits rvo controller from working
				//grounded ceases to have any meaning here so don't bother to set it
				//rvoController.SetEnabled (false);
				//update our position to the ragdoll's position
				//this and update fall are the only 2 places where we ever take direct control of our position
				//and this is the ONLY place where we take our position from the body & not the other way around
				mTr.position = Body.SmoothPosition;
				//don't bother with rotation
				return;
			} else if (IsGrounded) {
				//setting this to false isn't overkill just do it every frame
				//rvoController.SetEnabled (isKinematic);
			} else {
				//don't use our controller if we're falling
				//rvoController.SetEnabled (false);
			}

			if (Player.Local.Surroundings.IsUnderground) {
				return;
			}

			//if we're not immobilized
			//figure out our world elevation
			//if we're moving quickly check more often
			mCheckTerrainHeight++;
			if (mCheckTerrainHeight > 4 || mCurrentMovementSpeed > 0.1f) {
				terrainHit.ignoreWater = false;
				mCheckTerrainHeight = 0;
				float newAdjustedYPosition = AdjustedYPosition;
				if (worlditem.Group.Props.Interior || worlditem.Group.Props.TerrainType == LocationTerrainType.BelowGround) {
					newAdjustedYPosition = GameWorld.Get.InteriorHeightAtInGamePosition (ref terrainHit);
				} else {
					newAdjustedYPosition = GameWorld.Get.TerrainHeightAtInGamePosition (ref terrainHit);
				}

				AdjustedYPosition = newAdjustedYPosition;
				/*
				if (State.MotileProps.Hovers) {
					//update our adjusted y position over time to ease into our hover height
					//use max to ensure that you'll never be UNDER the terrain height
					//AdjustedYPosition = Mathf.Max (newAdjustedYPosition, Mathf.Lerp (mPosition.y, newAdjustedYPosition + State.MotileProps.HoverHeight, (float)(State.MotileProps.HoverChangeSpeed * Time.fixedDeltaTime)));
				} else {
					//make sure that we can continue to step up or down by this amount
					if (Mathf.Abs (newAdjustedYPosition - AdjustedYPosition) > this.State.MotileProps.MaxElevationChange) {
						//if we can't then we need to change direction
						//move our goal away from us in the opposite direction that we're moving
						GoalObject.position = Vector3.MoveTowards (GoalObject.position, mDesiredDirection, Mathf.Max (GoalDistance, 10f) * -1);
					}
					//in either case adjust the new y position
					AdjustedYPosition = newAdjustedYPosition;
				}*/
			}

			MotileAction topAction = TopAction;

			mCheckTopActionAnimation++;
			if (mCheckTopActionAnimation > 6 && Body.IsVisible) {
				mCheckTopActionAnimation = 0;
				ForceWalk = topAction.WalkingSpeed;
				CurrentIdleAnimation = topAction.IdleAnimation;
			}

			if (TargetMovementSpeed > 0.1f && (IsGrounded || State.MotileProps.Hovers) && !worlditem.Group.Props.Interior) {
				mCheckForObstacles++;
				if (mCheckForObstacles > 50) {
					mCheckForObstacles = 0;
					if (Characters.CheckForObstacles (this, ref mObstacleBounds)) {
						AvoidingObstacle = true;
						//time to jump
						//see if we could possibly clear it
						Characters.SendGoalAwayFromObstacle (this, worlditem.Position, 2f, 3f, mObstacleBounds);
						/*if (State.MotileProps.Hovers) {
							Characters.SendGoalAwayFromObstacle (this, worlditem.Position, 2f, 3f, mObstacleBounds);
						} else {
							float obstacleHeight = Mathf.Abs (mPosition.y - mObstacleBounds.max.y);
							if (State.MotileProps.CanJump && obstacleHeight < State.MotileProps.MaxElevationChange) {
								//Debug.Log ("Found obstacle, can clear it, jumping!");
								JumpForce = State.MotileProps.JumpForce;
							} else {
								//avoid it instead
								//Debug.Log ("Found obstacle but can't jump, avoiding obstacle of " + obstacleHeight.ToString () + " height");
								Characters.SendGoalAwayFromObstacle (this, worlditem.Position, 2f, 3f, mObstacleBounds);
							}
						}*/
					}
				}
			} else {
				JumpForce = 0f;
			}

			//update hovering / falling
			if (State.MotileProps.Hovers) {
				//hovering means we're always grounded
				//so no don't bother checking the ground position
				IsGrounded = true;
			} else {
				//if we weren't grounded before
				//and we are grounded now
				if (!IsGrounded) {
					if (terrainHit.isGrounded) {
						//yay, we weren't grounded but now we are, hit the ground
						StopFalling (isKinematic);
						//don't bother to update again this frame
						return;
					} else {
						//we're still not grounded, keep falling
						UpdateFalling (isKinematic);
						//don't bother to update direction
						return;
					}
					//if were grounded before
					//and we're not grounded now
				} else if (!terrainHit.isGrounded) {
					StartFalling (isKinematic);
					//don't bother to update direction
					return;
				}
			}

			//get the goal info
			//the direction is either to the goal object OR to the goal holder if we have one
			GoalDirection = (GoalObject.position - mPosition).normalized;
			LookDirection = GoalDirection;
			if (HasGoalHolder) {
				LookDirection = (GoalHolder.tr.position - mPosition).normalized;
			}
			//the distance is always to the goal object
			if (State.MotileProps.Hovers) {
				GoalDistance = Vector3.Distance (GoalObject.position, Body.BaseBodyPart.tr.position);
			} else {
				GoalDistance = Vector3.Distance (GoalObject.position, mPosition);
			}
			//figure out how fast we're going
			//target movement speed is set by our motile update scripts

			//always set this to zero to avoid weird look angles
			//TODO tie this to body length for navigating heights
			LookDirection.y = 0f;
			//figure out our desired velocity
			mDesiredLookDirection = LookDirection;
			switch (topAction.Type) {
			default:
				if (!HasReachedGoal (State.MotileProps.RVORadius)) {
					mDesiredDirection = GoalDirection;
				} else {
					mDesiredDirection = Vector3.zero;
					TargetMovementSpeed = 0f;
					CurrentMovementSpeed = 0f;
				}
				break;

			case MotileActionType.FocusOnTarget:
			case MotileActionType.Wait:
				mDesiredDirection = mDesiredLookDirection;
				TargetMovementSpeed = 0f;
				CurrentMovementSpeed = 0f;
				break;
			}

			//we're grounded now and we were before
			//so just move normally
			//update when we were last grounded
			mLastGroundedHeight = AdjustedYPosition;
			mLastGroundedTime = WorldClock.AdjustedRealTime;

			//if force is greater than zero
			//we have to go in that direction
			CurrentRotationSpeed = State.MotileProps.RotationChangeSpeed;
			if (ForceDirection != Vector3.zero) {
				CurrentMovementSpeed = ForceDirection.magnitude * ThrowbackSpeed;
				if (!isKinematic) {
					mUpdateBodyPosition++;
					if (mUpdateBodyPosition > 5 && Body.IsInitialized) {
						mTr.position = Body.SmoothPosition;
						mUpdateBodyPosition = 0;
					}
					mTr.rotation = Body.SmoothRotation;
					Body.UpdateForces (mPosition, mDesiredDirection, terrainHit.normal, IsGrounded, JumpForce, (float)TargetMovementSpeed);
					//blend between the movement-based direction and the desired look direction based on speed
					if (mDesiredLookDirection != Vector3.zero) {
						mVisibleRotation = Quaternion.LookRotation (mDesiredLookDirection.WithY (0f));
					}
				}
				//fade out the force direction
				ForceDirection = Vector3.Lerp (ForceDirection, Vector3.zero, Time.fixedDeltaTime);
			} else {
				if (!isKinematic) {
					//assign our position and rotation from the body before updating forces
					mUpdateBodyPosition++;
					if (mUpdateBodyPosition > 5 && Body.IsInitialized) {
						mTr.position = Body.SmoothPosition;
						mUpdateBodyPosition = 0;
					}
					mTr.rotation = Body.SmoothRotation;
					Body.UpdateForces (mPosition, mDesiredDirection, terrainHit.normal, IsGrounded, JumpForce, (float)TargetMovementSpeed);
				}
			}
		}

		protected int mUpdateBodyPosition = 0;

		public void UpdateFalling (bool isKinematic)
		{
			mFallAcceleration += Globals.DefaultCharacterFallAcceleration * (float)(WorldClock.AdjustedRealTime - mLastFallUpdate);
			mFallAcceleration = Mathf.Min (mFallAcceleration, Globals.MaxCharacterFallAcceleration);
			mPosition.y -= mFallAcceleration;
			//this and Ragdoll are the only two places where we update our position manually
			//because the RVO controller isn't doing it for us
			mTr.position = mPosition;
			//don't bother with rotation
			mLastFallUpdate = WorldClock.AdjustedRealTime;
		}

		public void StartFalling (bool isKinematic)
		{
			//we won't be needing our rvo sim while we fall
			//so deactivate it here
			//rvoController.SetEnabled (false);
			IsGrounded = false;
			mLastGroundedHeight = AdjustedYPosition;
			mLastGroundedTime = WorldClock.AdjustedRealTime;
			mLastFallUpdate = WorldClock.AdjustedRealTime;
			mFallAcceleration = Globals.DefaultCharacterFallAcceleration;
		}

		public void StopFalling (bool isKinematic)
		{
			//TODO use last grounded height to apply damage, if applicable
			IsGrounded = true;
			mLastGroundedHeight = AdjustedYPosition;
			mLastGroundedTime = WorldClock.AdjustedRealTime;
			terrainHit.overhangHeight = Globals.DefaultCharacterHeight;
			//now that we've hit the ground
			//set the rvo controller to active again
			//this will automatically teleport it to our current position
			//rvoController.SetEnabled (isKinematic);
		}

		#endregion

		//a list of action/priority pairs that are handled in the order they are received
		protected MotileInstructions mMotileInstructions;
		protected Queue <KeyValuePair <MotileActionPriority, MotileAction>>	mNewActions = new Queue <KeyValuePair <MotileActionPriority, MotileAction>> ();
		protected List <MotileAction> mActions = new List<MotileAction> ();
		protected KeyValuePair <MotileActionPriority, MotileAction> mNextNewAction;
		protected bool mHandlingNewActions = false;
		protected double mCurrentMovementSpeed;
		protected double mCurrentRotationSpeed;
		protected bool mDoingActionsOverTime = false;
		protected bool mRagdoll = false;
		protected Transform mTr;
		public GameWorld.TerrainHeightSearch terrainHit = new GameWorld.TerrainHeightSearch ();
		protected double mLastGroundedHeight = 0f;
		protected double mLastGroundedTime;
		protected double mLastFallUpdate;
		protected float mFallAcceleration;
		protected Vector3 mPosition;
		protected Vector3 mDesiredDirection;
		protected Vector3 mDesiredLookDirection;
		protected Quaternion mVisibleRotation;
		protected Bounds mObstacleBounds;
		protected int mCheckForObstacles;
		protected static float gMinLookDirMagnitude = 2.5f;

		#if UNITY_EDITOR
		public void OnDrawGizmos ()
		{
			if (!mInitialized)
				return;

			if (GoalObject != null) {
				Gizmos.color = Color.green;
				Gizmos.DrawLine (mPosition, GoalObject.position);
			}

			if (terrainHit.isGrounded) {
				Gizmos.color = Color.cyan;
			} else {
				Gizmos.color = Color.red;
			}
			Gizmos.DrawLine (terrainHit.feetPosition, terrainHit.feetPosition + Vector3.up * terrainHit.overhangHeight);
		}
		#endif
	}

	[Serializable]
	public class MotileState
	{
		public MotileProperties MotileProps = new MotileProperties ();
		//how fast we run, walk, etc.
		public List <MotileAction> Actions = new List <MotileAction> ();
		//what we're supposed to be doing now
		public MotileAction BaseAction = new MotileAction ();
		//what we do when there's nothing else to do (usually routine)
		public double MovementFatigue = 0.0f;
		//how much the character gets fatigued by running / walking, usually 0
		public List <string> QuestPointsReached = new List <string> ();
		protected MotileAction mBaseAction;
	}
}
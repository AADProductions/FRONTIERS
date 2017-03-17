using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
////using Pathfinding.RVO;

namespace Frontiers
{
	//base class for local and networked players
	public class PlayerBase : MonoBehaviour, IStackOwner, IBodyOwner, IVisible, IAudible
	{
		public TNObject Owner;
		public Transform tr;
		//public IAgent RVOAgent;

		#region basic properties

		public PlayerIDFlag ID {
			get {
				return mID;
			}set {
				mID = value;
			}
		}

		public virtual string Name {
			get {
				return "Player";
			}
			set {
				return;
			}
		}

		public virtual Vector3 Position {
			get {
				return tr.position;
			}
			set {
				tr.position = value;
			}
		}

		public Quaternion Rotation {
			get {
				return tr.rotation;
			}
			set {
				tr.rotation = value;
			}
		}

		public bool UseGravity { get { return true; } }

		public bool IsKinematic { get { return true; } }

		public double CurrentMovementSpeed { get; set; }

		public double CurrentRotationSpeed { get; set; }

		public int CurrentIdleAnimation { get; set; }

		public WorldBody Body { get; set; }

		public bool IsImmobilized { get { return IsDead; } }

		public bool IsRagdoll { get { return IsDead; } }

		public bool IsDestroyed { get { return false; } }

		public bool Initialized { get { return true; } }

		public int IndleAnimation { get; set; }

		public bool ForceWalk { get; set; }

		public virtual bool IsDead {
			get {
				return false;
			}
			set {
				return;
			}
		}

		protected PlayerIDFlag mID;

		#endregion

		#region body related properties

		public virtual Vector3 Height {
			get {
				mHeight.y = Body.Transforms.HeadTop.position.y - tr.position.y;
				return mHeight;
			}
		}

		public virtual Vector3 HeadPosition { get { return Body.Transforms.Head.position; } }

		public virtual Vector3 ChestPosition { get { return Body.Transforms.Chest.position; } }

		public virtual Vector3 EyePosition { //used for conversations
			get {
				mEyePosition.Set (0f, HeadPosition.y - 0.3f, 0f);
				return mEyePosition;
			}
		}

		protected Vector3 mHeight;
		protected Vector3 mEyePosition;

		public virtual Vector3 FocusPosition { //used for line of sight
			get {	//only the head top is guaranteed to have correct orientation
				return Body.Transforms.HeadTop.forward;
			}
		}

		[NObjectSync]
		public virtual bool IsGrounded { get { return true; } }

		[NObjectSync]
		public virtual bool IsCrouching { get { return false; } }

		[NObjectSync]
		public virtual bool IsWalking { get { return false; } }

		[NObjectSync]
		public virtual bool IsSprinting { get { return false; } }

		public virtual bool IsOnFoot { get { return true; } }

		#endregion

		#region vectors & bounds

		public virtual Vector3 FocusVector { get { return Vector3.Normalize (FocusPosition - HeadPosition); } }

		public virtual Vector3 ForwardVector { get { return transform.forward; } }

		public virtual Vector3 RightVector { get { return transform.right; } }

		public virtual Vector3 UpVector { get { return transform.up; } }

		public virtual Vector3 DownVector { get { return -transform.up; } }

		public float ColliderRadius { get { return Globals.PlayerColliderRadius; } }

		public Bounds ColliderBounds { get { return mColliderBounds; } }

		protected Bounds mColliderBounds = new Bounds ();

		#endregion

		//flags for appearance, etc.

		#region IItemOfInterest, IVisible, IAudible implementation

		//this is used to tell whether the player is visible or not
		public ItemOfInterestType IOIType { get { return ItemOfInterestType.Player; } }

		public virtual bool IsVisible { get { return true; } }

		public virtual float AwarenessDistanceMultiplier { get { return 1.0f; } }

		public virtual float FieldOfViewMultiplier { get { return 1.0f; } }

		public bool Destroyed { get { return false; } }

		public virtual bool Has (string scriptName)
		{
			return false;
		}

		public virtual bool HasAtLeastOne (List <string> scriptNames)
		{
			if (scriptNames == null || scriptNames.Count == 0) {
				return true;
			}
			return false;
		}

		public PlayerBase player { get { return this; } }

		public ActionNode node { get { return null; } }

		public WorldLight worldlight { get { return null; } }

		public Fire fire { get { return null; } }

		public bool HasPlayerFocus { get; set; }

		public virtual bool IsAudible { get { return true; } }

		public virtual float AudibleRange { get { return Globals.MaxAudibleRange; } }

		public virtual float AudibleVolume { get { return 1.0f; } }

		public virtual MasterAudio.SoundType LastSoundType { get; set; }

		public virtual string LastSoundName { get; set; }

		public virtual void ListenerFailToHear ()
		{
			//do nothing
		}

		public virtual void LookerFailToSee ()
		{
			//do nothing
		}

		#endregion

		#region IStackOwner implementation

		//this is used by containers that the player owns
		public WIGroup Group;

		public WorldItem worlditem { get { return null; } }

		public bool IsWorldItem { get { return false; } }

		public string StackName { get { return ID.ToString (); } }

		public string FileName { get { return ID.ToString (); } }

		public virtual string DisplayName { get { return Name; } }

		public string QuestName { get { return string.Empty; } }

		public WISize Size { get { return WISize.NoLimit; } }

		public virtual bool UseRemoveItemSkill (HashSet <string> removeItemSkillNames, ref IStackOwner useTarget)
		{
			return false;
		}

		public virtual List <string> RemoveItemSkills { get { return new List <string> (); } }

		public virtual void Refresh ()
		{
		}

		public virtual bool CheckVisibility (Vector3 position)
		{
			return false;
		}

		#endregion

		#region initialization & manager calls

		public virtual void SaveState ()
		{

		}

		public virtual void LoadState ()
		{

		}

		public virtual void Awake ()
		{
			tr = transform;
		}

		public virtual void Spawn ()
		{

		}

		public virtual void Despawn ()
		{

		}

		public virtual bool HasSpawned {
			get {
				return false;
			}
		}
		//these are called by the Player manager
		public virtual void Initialize ()
		{
			mInitialized = true;
			OnInitialized ();
		}

		public virtual void OnInitialized ()
		{
			//RVOAgent = GameWorld.Get.Simulator.GetSimulator().AddAgent(tr.position);
		}

		public virtual void OnGameStartFirstTime ()
		{

		}

		public virtual void OnGameReset ()
		{

		}

		public virtual void OnGameSaveStart ()
		{

		}

		public virtual void OnGameSave ()
		{

		}

		public virtual void OnModsLoadStart ()
		{

		}

		public virtual void OnModsLoadFinish ()
		{

		}

		public virtual void OnGameLoadStart ()
		{

		}

		public virtual void OnGameLoadFinish ()
		{

		}

		public virtual void OnLocalPlayerSpawn ()
		{

		}

		public virtual void OnRemotePlayerSpawn ()
		{

		}

		public virtual void OnLocalPlayerDespawn ()
		{

		}

		public virtual void OnLocalPlayerDie ()
		{

		}

		public virtual void OnRemotePlayerDie ()
		{

		}

		public virtual void OnGameStart ()
		{

		}

		public virtual void OnGameUnload ()
		{

		}

		public virtual void OnGamePause ()
		{

		}

		public virtual void OnGameContinue ()
		{

		}

		public virtual void OnExitProgram ()
		{

		}

		protected bool mInitialized = false;

		public virtual void FixedUpdate ()
		{
			/*if (RVOAgent == null) {
								return;
						}

						RVOAgent.Radius = 0.5f;
						RVOAgent.MaxSpeed = 10f;
						RVOAgent.Height = 1f;
						RVOAgent.AgentTimeHorizon = 2f;
						RVOAgent.ObstacleTimeHorizon = 2f;
						RVOAgent.Locked = false;
						RVOAgent.MaxNeighbours = 4;
						RVOAgent.NeighbourDist = 10;*/
		}

		#endregion

		public virtual void SetControllerState (string stateName, bool enabled)
		{
			return;
		}

		public virtual void SetCameraState (string stateName, bool enabled)
		{
			return;
		}

		public void OnDrawGizmos ()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine (transform.position, transform.position + (transform.up * 50));
			Gizmos.DrawSphere (transform.position + (transform.up * 50), 5f);
		}
	}
}
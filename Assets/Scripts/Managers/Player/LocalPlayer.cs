using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.GUI;

namespace Frontiers
{
		public class LocalPlayer : PlayerBase
		{
				//where all script info is stored on save/load
				public PlayerState State = new PlayerState();

				public override Vector3 Position {
						get {
								if (mInitialized && Status.IsStateActive("Traveling")) {
										return HijackedPosition.position;
								}
								return tr.position;
						}
						set {
								tr.position = value;
								State.Transform.Position = value;
						}
				}

				#region objects

				//fps camera scripts
				public CharacterController Controller;
				public vp_FPController FPSController;
				public vp_FPCamera FPSCamera;
				public vp_FPWeapon FPSWeapon;
				public vp_FPWeapon FPSWeaponCarry;
				public vp_FPWeaponMeleeAttack FPSMelee;
				public vp_FPPlayerEventHandler FPSEventHandler;
				//<--get rid of this POS
				//objects
				public Transform FPSCameraSeat;
				public Transform FocusObject;
				public Transform GrabberTargetObject;
				public Transform HijackedPosition;
				public Transform HijackedLookTarget;
				public GameObject ToolOffset;
				public GameObject CarryOffset;
				public Light PlayerLight;
				public PlayerGrabber Grabber;
				public PlayerEncounterer EncountererObject;
				public PlayerGroundPath GroundPath;
				//player scripts
				public PlayerScriptManager Scripts;
				public PlayerAudio Audio;
				public PlayerWeatherEffects WeatherEffects;
				public PlayerDamageHandler DamageHandler;
				public PlayerSurroundings Surroundings;
				public PlayerTool Tool;
				public PlayerCarrier Carrier;
				public PlayerInventory Inventory;
				public PlayerWearables Wearables;
				public PlayerFocus Focus;
				public PlayerStatus Status;
				public PlayerItemPlacement ItemPlacement;
				public PlayerProjections Projections;
				public PlayerCharacterSpawner CharacterSpawner;
				public PlayerIllumination Illumination;
				public float HijackLookSpeed = 1f;

				public override Vector3 HeadPosition {
						get {
								return FPSCameraSeat.position;
						}
				}

				public override Vector3 FocusVector {
						get {
								return FPSCameraSeat.forward;
						}
				}

				public bool LockQuickslots {
						get {
								for (int i = 0; i < Scripts.Scripts.Count; i++) {
										if (Scripts.Scripts[i].LockQuickslots) {
												return true;
										}
								}
								return false;
						}
				}

				#endregion

				#region initialization

				public void Start()
				{
						Scripts = new PlayerScriptManager();
						Scripts.player = this;
						//these are objects that don't require PlayerScripts to be initialized
						CreatePlayerPieces();
						FindPlayerScripts();
						FindFPSScripts();
						ApplyOffsets();//do this for the benefit of the fps scripts
						enabled = false;
				}

				public override void OnModsLoadStart()
				{
						State.SetDefaults();
						Scripts.Initialize();
						PlayerState state = null;
						if (Mods.Get.Runtime.LoadMod <PlayerState>(ref state, "PlayerState", ID.ToString())) {
								State = state;
								Scripts.LoadState(State.ScriptStates);
						} else {
								Debug.Log("Couldn't load player state " + ID.ToString());
						}
						//if the player state doesn't exist it's no big deal
						mInitialized = true;
				}

				public override void OnGameLoadStart()
				{
						Scripts.OnGameLoadStart();
				}

				public override void OnGameLoadFinish()
				{
						Scripts.OnGameLoadFinish();
				}

				public override void OnGameStartFirstTime()
				{
						//these defaults will be applied in player spawn
						Scripts.OnGameStartFirstTime();
				}

				public override void OnGameUnload()
				{
						Scripts.OnGameUnload();
				}

				public override void OnLocalPlayerDie()
				{
						Scripts.OnLocalPlayerDie();
				}

				public override void OnExitProgram()
				{
						enabled = false;
				}

				public override void OnLocalPlayerDespawn()
				{

				}

				public override void OnGameStart()
				{
						enabled = true;
						Player.Get.UserActions.Subscribe(UserActionType.ActionCancel, new ActionListener(ActionCancel));
						Scripts.OnGameStart();
						Spawn();
				}

				public override void OnLocalPlayerSpawn()
				{
						ApplyOffsets();
						StealGameCamera();
						Scripts.OnLocalPlayerSpawn();
						Body.OnSpawn(this);
						Body.IgnoreCollisions(true);
						PauseControls(false);
				}

				public override void OnGamePause()
				{
						//the game can be paused before we've spawned
						if (!mInitialized)
								return;
						PauseControls(true);
						//SaveState ();
				}

				public override void OnGameContinue()
				{
						//the game can continue before we've spawned
						if (!mInitialized)
								return;

						StealGameCamera();
						PauseControls(false);
				}

				public override void OnGameSaveStart()
				{
						Scripts.OnGameSaveStart();
				}

				public override void OnGameSave()
				{
						SaveState();
				}

				#endregion

				#region saving

				public override void SaveState()
				{
						State.Transform.Position = tr.position;
						State.Transform.Rotation.y = FPSCamera.Yaw;
						State.Transform.Rotation.x = FPSCamera.Pitch;
						State.ScriptStates = Scripts.OnGameSave();
						Mods.Get.Runtime.SaveMod <PlayerState>(State, "PlayerState", ID.ToString());
				}

				public PlayerStartupPosition GetStartupPosition()
				{
						PlayerStartupPosition psp = new PlayerStartupPosition();
						psp.ClearWearables = false;
						psp.ClearRevealedLocations = false;
						psp.ClearInventory = false;
						psp.ClearLog = false;
						psp.InventoryFillCategory = string.Empty;
						if (Surroundings.State.IsVisitingLocation) {
								psp.LocationReference = Surroundings.State.LastLocationVisited;
						}
						if (Surroundings.State.IsInsideStructure) {
								psp.Interior = true;
								psp.LocationReference = Surroundings.State.LastStructureEntered;
								psp.StructureName = Surroundings.State.LastStructureEntered.FileName;
						} else {
								psp.Interior = false;
								psp.RequiresMeshTerrain = Surroundings.State.IsOnTopOfMeshTerrain;
						}
						psp.ChunkID = Surroundings.State.LastChunkID;
						WorldChunk chunk = null;
						if (GameWorld.Get.ChunkByID(psp.ChunkID, out chunk)) {
								psp.WorldPosition = Surroundings.State.LastPosition;
								psp.ChunkPosition = new STransform((psp.WorldPosition.Position - chunk.ChunkOffset), psp.WorldPosition.Rotation);
						}
						return psp;
				}

				#endregion

				#region controls

				protected void StealGameCamera()
				{
						if (!GameManager.Is(FGameState.Cutscene)) {
								Camera gameCamera = GameManager.Get.GameCamera;
								gameCamera.transform.parent = FPSCameraSeat;
								gameCamera.transform.ResetLocal();
								gameCamera.fieldOfView = Profile.Get.CurrentPreferences.Video.FieldOfView;
						}
				}

				public override void SetControllerState(string stateName, bool enabled)
				{
						FPSController.SetState(stateName, enabled, false, false);
				}

				public override void SetCameraState(string stateName, bool enabled)
				{
						FPSCamera.SetState(stateName, enabled, false, false);
				}

				protected void ReleaseGameCamera()
				{
						GameManager.Get.GameCamera.transform.parent = null;
				}

				protected void PauseControls(bool pause)
				{
						Controller.enabled = pause;
						FPSController.SetState("Paused", pause);
						FPSCamera.SetState("Paused", pause);
				}

				public void SetGliderMode(bool enabled)
				{
						FPSController.SetState("Glider", enabled, false, false);
						FPSCamera.SetState("Glider", enabled, false, false);
				}

				public void DoEarthquake(float earthquakeDuration)
				{
						if (!mDoingEarthquake) {
								mDoingEarthquake = true;
								StartCoroutine(DoEarthquakeOverTime(earthquakeDuration, 0.25f));
						}
				}

				public void DoEarthquake(float earthquakeDuration, float earthquakeIntensity)
				{
						if (!mDoingEarthquake) {
								mDoingEarthquake = true;
								StartCoroutine(DoEarthquakeOverTime(earthquakeDuration, earthquakeIntensity));
						}
				}

				protected IEnumerator DoEarthquakeOverTime(float earthquakeDuration, float earthquakeIntensity)
				{
						double timeStart = WorldClock.AdjustedRealTime;
						while (WorldClock.AdjustedRealTime < timeStart + earthquakeDuration) {
								yield return WorldClock.WaitForSeconds(UnityEngine.Random.value);
								FPSCamera.DoBomb(Vector3.one, earthquakeIntensity, earthquakeIntensity);
						}
				}

				public void SetHijackTargets(Transform hijackedLookTarget)
				{
						//this is used with zoom mode
						HijackedPosition.position = HeadPosition;
						HijackedLookTarget.position = hijackedLookTarget.position;
				}

				public void SetHijackTargets(Transform hijackedPosition, Transform hijackedLookTarget)
				{
						HijackedPosition.transform.position = hijackedPosition.position;
						HijackedLookTarget.transform.position = hijackedLookTarget.position;
						//if they're the same object, put one slightly ahead of the other
						if (hijackedPosition == hijackedLookTarget) {
								HijackedLookTarget.position = hijackedLookTarget.position + hijackedLookTarget.forward;
						}
				}

				public void SetHijackTargets(GameObject hijackedPosition, GameObject hijackedLookTarget)
				{
						SetHijackTargets(hijackedPosition.transform, hijackedLookTarget.transform);
				}

				public void SnapToHijackedPosition()
				{
						GameManager.Get.GameCamera.transform.rotation = HijackedPosition.transform.rotation;
						GameManager.Get.GameCamera.transform.position = HijackedPosition.transform.position;
				}

				public void SetHijackCancel(Action cancelHijack)
				{
						mHijackCancel += cancelHijack;
				}

				public void LookThroughLooker(Looker looker, bool hijackControl)
				{
						if (mLookingThroughLooker) {
								return;
						}
						mLookingThroughLooker = true;
						StartCoroutine(LookThroughLookerOverTime(looker, hijackControl));
				}

				public void StopLookingThroughLooker()
				{
						mLookingThroughLooker = false;
				}

				protected IEnumerator LookThroughLookerOverTime(Looker looker, bool hijackControl)
				{
						while (looker != null && mLookingThroughLooker) {
								FPSCamera.RenderingFieldOfView = looker.State.FieldOfView * Globals.MaxFieldOfView;
						}
						mLookingThroughLooker = false;
						yield break;
				}

				public void ZoomCamera(float zoomFOV, float cameraSensitivity)
				{
						mZoomedIn = true;
						mZoomedInCameraSensitivity = cameraSensitivity;
						mZoomFOV = zoomFOV;
				}

				public void UnzoomCamera( ) {
						mZoomedIn = false;
						mZoomedInCameraSensitivity = 1f;
						mZoomFOV = 0f;
				}

				public void HijackControl()
				{
						HijackedPosition.transform.rotation = GameManager.Get.GameCamera.transform.rotation;
						HijackedPosition.transform.position = GameManager.Get.GameCamera.transform.position;
						HijackLookSpeed = Globals.PlayerHijackLerp;
						mPitchOnHijack = FPSCamera.Pitch;
						mYawOnHijack = FPSCamera.Yaw;

						Player.Get.AvatarActions.ReceiveAction(AvatarAction.ControlHijack, WorldClock.AdjustedRealTime);
						State.HijackMode = PlayerHijackMode.LookAtTarget;
						State.IsHijacked = true;
						Controller.enabled = false;
				}

				public void RestoreControl(bool keepLookDirection)
				{
						if (State.IsHijacked) {
								Camera gameCamera = GameManager.Get.GameCamera;
								gameCamera.transform.parent = FPSCameraSeat;
								gameCamera.transform.localPosition = Vector3.zero;
								gameCamera.fieldOfView = Profile.Get.CurrentPreferences.Video.FieldOfView;
								//we want to take the hijacked camera rotation and apply it to the fps camera's rotation
								//then we'll reset the game camera's rotation
								if (keepLookDirection) {
										FPSCamera.Pitch = gameCamera.transform.localRotation.eulerAngles.x;
										FPSCamera.Yaw = gameCamera.transform.localRotation.eulerAngles.z;
								} else {
										FPSCamera.Pitch = mPitchOnHijack;
										FPSCamera.Yaw = mYawOnHijack;
								}
								gameCamera.transform.localRotation = Quaternion.identity;

								State.IsHijacked = false;
								Controller.enabled = true;

								Player.Get.AvatarActions.ReceiveAction(AvatarAction.ControlRestore, WorldClock.AdjustedRealTime);
						}
				}

				public void SetMotionLocks(bool movement, bool camera, Transform lockSource)
				{
						State.MovementLocked = movement;
						State.CameraLocked = camera;
						mLockSource = lockSource;
				}

				public bool MovementLocked {
						get {
								if (mLockSource == null) {
										State.MovementLocked = false;
								}
								return State.MovementLocked || IsHijacked;
						}
						set {
								State.MovementLocked = value;
						}
				}

				public bool CameraLocked {
						get {
								if (mLockSource == null) {
										State.MovementLocked = false;
								}
								return State.CameraLocked || IsHijacked;
						}
				}

				protected Transform mLockSource;
				protected Action mHijackCancel;
				protected float mPitchOnHijack;
				protected float mYawOnHijack;

				#endregion

				#region spawning / despawning

				public override void Spawn()
				{
						if (HasSpawned && !IsDead) {
								//whoops, we've made a mistake
								return;
						}
						//reset the camera's local position to zero
						//it may have been used in cutscenes or something
						StealGameCamera();
						//this will result in an OnLocalPlayerSpawn call
						//handle our spawn business there
						State.HasSpawned = true;
						State.HasSpawnedFirstTime = true;
						State.IsDead = false;
						RestoreControl(false);
						//apply our state transform
						//this will send us to that position
						//now that we'e spawned, let the manager know that we've spawned
						tr.position = State.Transform.Position;
						FPSCamera.Yaw = State.Transform.Rotation.y;
						FPSCamera.Pitch = State.Transform.Rotation.x;
						FPSCamera.SnapZoom();
						FPSCamera.SnapSprings();
						SaveState();
						Manager.LocalPlayerSpawn();
				}

				public override void Despawn()
				{
						State.HasSpawned = false;
						Body.SetVisible(false);
						PauseControls(true);
						ReleaseGameCamera();
						if (GameManager.Is(FGameState.InGame)) {
								HijackControl();
								HijackedPosition.transform.position = Position + Globals.PlayerDeathHijackedOffset;
								HijackedLookTarget.transform.position = Position;
						}
						Manager.LocalPlayerDespawn();
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalDespawn, WorldClock.AdjustedRealTime);
				}

				public void Die(string causeOfDeath)
				{
						if (!HasSpawned || IsDead) {
								//whoops, we made a mistake
								return;
						}

						//if our difficulty setting doesn't allow death, skip this
						if (Profile.Get.CurrentGame.Difficulty.DeathStyle == DifficultyDeathStyle.NoDeath) {
								Debug.Log("Death mode is set to 'No Death' so not applying death");
								return;
						}

						Status.LatestCauseOfDeath = causeOfDeath;
						State.HasSpawned = false;
						State.IsDead = true;
						Despawn();
						Manager.LocalPlayerDie();
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.SurvivalDie, WorldClock.AdjustedRealTime);
						GameObject deathDialog = GUIManager.Get.Dialog("NGUIDeathDialog");
						//the death dialog will figure out what the player wants to do next
						GUIManager.SpawnNGUIChildEditor(gameObject, deathDialog, false);
						//this will result in an OnLocalPlayerDie call
						//we'll handle our death business there
				}

				public void SpawnAtPosition(STransform startupPosition)
				{
						State.Transform.CopyFrom(startupPosition);
						if (GameManager.Is(FGameState.InGame)) {
								Spawn();
						}
				}

				#endregion

				#region avatar actions

				public bool ActionCancel(double timeStamp)
				{		//the player receives all user actions after the GUI is done with them
						//if ActionCancel gets to this point it means we're hitting 'esc' with the expectation
						//of seeing the start menu
						if (IsHijacked && mHijackCancel != null) {
								mHijackCancel.Invoke();
								mHijackCancel = null;
								RestoreControl(false);
						} else if (Player.Get.UserActions.HasFocus && GameManager.Is(FGameState.InGame | FGameState.GamePaused) && !GUILoading.IsLoading) {
								GameManager.Get.SpawnStartMenu(FGameState.GamePaused, FGameState.InGame);
						}
						return true;
				}

				#endregion

				#region player building

				protected void CreatePlayerPieces()
				{
						Controller = gameObject.GetOrAdd <CharacterController>();
						Controller.center = new Vector3(0f, Globals.PlayerControllerYCenterDefault, 0f);
						Controller.height = Globals.PlayerControllerHeightDefault;
						Controller.radius = Globals.PlayerControllerRadiusDefault;
						Controller.slopeLimit = Globals.PlayerControllerSlopeLimitDefault;
						Controller.stepOffset = Globals.PlayerControllerStepOffsetDefault;
						//WTF why isn't skin width exposed...? whatever, unity
						//create feet collider
						GameObject feetCollider = gameObject.CreateChild("FeetCollider").gameObject;
						feetCollider.layer = Globals.LayerNumPlayer;
						feetCollider.AddComponent <Rigidbody>().isKinematic = true;
						SphereCollider fsc = feetCollider.AddComponent <SphereCollider>();
						fsc.radius = 0.25f;
						fsc.center = new Vector3(0f, 0.125f, 0f);

						GameObject fpsCameraObject = GameObject.Instantiate(Player.Get.LocalFPSCameraSeatPrefab) as GameObject;
						FPSCameraSeat = fpsCameraObject.transform;
						FPSCameraSeat.parent = transform;
						FPSCameraSeat.name = "FPSCameraSeat";

						CarryOffset = GameObject.Instantiate(Player.Get.LocalCarrierOffsetPrefab) as GameObject;
						CarryOffset.transform.parent = FPSCameraSeat;
						CarryOffset.name = "CarryOffset";

						ToolOffset = GameObject.Instantiate(Player.Get.LocalToolOffsetPrefab) as GameObject;
						ToolOffset.transform.parent = FPSCameraSeat;
						ToolOffset.name = "ToolOffset";

						GameObject toolObject = GameObject.Instantiate(Player.Get.LocalToolPrefab) as GameObject;
						toolObject.transform.parent = ToolOffset.transform;
						toolObject.transform.ResetLocal();
						toolObject.name = "Tool";
						Tool = toolObject.GetComponent <PlayerTool>();

						GameObject carryObject = GameObject.Instantiate(Player.Get.LocalCarrierPrefab) as GameObject;
						carryObject.transform.parent = CarryOffset.transform;
						carryObject.transform.ResetLocal();
						carryObject.name = "Carrier";
						Carrier = carryObject.GetComponent <PlayerCarrier>();

						PlayerLight = gameObject.CreateChild("Illumination").gameObject.AddComponent <Light>();
						FocusObject = FPSCameraSeat.gameObject.CreateChild("FocusObject");
						GrabberTargetObject = FPSCameraSeat.gameObject.CreateChild("GrabberTarget");

						//these objects are stored in the Player manager object
						GameObject globalParent = Player.Get.gameObject;
						HijackedPosition = globalParent.CreateChild("HijackedPosition");
						HijackedLookTarget = globalParent.CreateChild("HijackedLookTarget");
						Grabber = globalParent.CreateChild("Grabber").gameObject.AddComponent <PlayerGrabber>();
						Grabber.Target = GrabberTargetObject;
						EncountererObject = globalParent.CreateChild("Encounterer").gameObject.AddComponent <PlayerEncounterer>();
						EncountererObject.TargetPlayer = this;
						GroundPath = globalParent.CreateChild("GroundPath").gameObject.AddComponent <PlayerGroundPath>();
						GroundPath.FollowerTarget = FPSCameraSeat;
				}

				protected void FindPlayerScripts()
				{
						Audio = gameObject.GetOrAdd <PlayerAudio>();
						WeatherEffects = gameObject.GetOrAdd <PlayerWeatherEffects>();
						DamageHandler = gameObject.GetOrAdd <PlayerDamageHandler>();
						Surroundings = gameObject.GetOrAdd <PlayerSurroundings>();
						Inventory = gameObject.GetOrAdd <PlayerInventory>();
						Wearables = gameObject.GetOrAdd <PlayerWearables>();
						Focus = gameObject.GetOrAdd <PlayerFocus>();
						Status = gameObject.GetOrAdd <PlayerStatus>();
						ItemPlacement = gameObject.GetOrAdd <PlayerItemPlacement>();
						Projections = gameObject.GetOrAdd <PlayerProjections>();
						CharacterSpawner = gameObject.GetOrAdd <PlayerCharacterSpawner>();
			Illumination = gameObject.GetOrAdd <PlayerIllumination>();

						Audio.OnLocalPlayerCreated();
						WeatherEffects.OnLocalPlayerCreated();
						DamageHandler.OnLocalPlayerCreated();
						Surroundings.OnLocalPlayerCreated();
						Inventory.OnLocalPlayerCreated();
						Focus.OnLocalPlayerCreated();
						Status.OnLocalPlayerCreated();
						ItemPlacement.OnLocalPlayerCreated();
						Projections.OnLocalPlayerCreated();
						Tool.OnLocalPlayerCreated();
						Carrier.OnLocalPlayerCreated();
						Wearables.OnLocalPlayerCreated();
				}

				protected void FindFPSScripts()
				{
						FPSController = gameObject.GetComponent <vp_FPController>();
						FPSCamera = FPSCameraSeat.GetComponent <vp_FPCamera>();
						FPSWeapon = ToolOffset.GetComponent <vp_FPWeapon>();
						FPSWeaponCarry = CarryOffset.GetComponent <vp_FPWeapon>();
						FPSMelee = ToolOffset.GetComponent <vp_FPWeaponMeleeAttack>();
						FPSEventHandler = gameObject.GetComponent <vp_FPPlayerEventHandler>();
						vp_FPInput input = gameObject.GetComponent <vp_FPInput>();

						FPSController.OnLocalPlayerCreated();
						FPSCamera.OnLocalPlayerCreated();
						FPSWeapon.OnLocalPlayerCreated();
						FPSWeaponCarry.OnLocalPlayerCreated();
						FPSMelee.OnLocalPlayerCreated();
						FPSEventHandler.OnLocalPlayerCreated();
						input.OnLocalPlayerCreated();
				}

				protected void ApplyOffsets()
				{
						//apply our state transforms to all our pieces
						State.FocusObjectPosition.ApplyTo(FocusObject.transform);
						State.FPSCameraSeatPosition.ApplyTo(FPSCameraSeat);
						State.GrabberPosition.ApplyTo(Grabber.transform);
						State.GrabberTargetPosition.ApplyTo(GrabberTargetObject.transform);
						State.IlluminationPosition.ApplyTo(PlayerLight.transform);
						State.ToolOffsetPosition.ApplyTo(ToolOffset.transform);
						State.CarryOffsetPosition.ApplyTo(CarryOffset.transform);

						PlayerLight.intensity = State.IlluminationIntensity;
						PlayerLight.range = State.IlluminationRange;
						PlayerLight.color = Colors.Get.ByName("PlayerIlluminationColor");
						PlayerLight.cullingMask = Int32.MaxValue & ~Globals.LayerFluidTerrain;
				}

				#endregion

				#region state functions

				public override bool HasSpawned {
						get {
								return State.HasSpawned;
						}
				}

				public override bool IsDead {
						get {
								return State.IsDead;
						}
						set {
								State.IsDead = value;
						}
				}

				public bool IsHijacked {
						get {
								return State.IsHijacked;
						}
				}

				public override bool IsGrounded { get { return Controller.isGrounded; } }

				public override bool IsCrouching { get { return FPSController.IsCrouching; } }

				public override bool IsWalking { get { return FPSController.IsWalking; } }

				public override bool IsSprinting { get { return FPSController.IsSprinting; } }

				public override bool IsOnFoot { get { return IsGrounded && !FPSController.IsMounted && !FPSController.IsClimbingLadder; } }

				#endregion

				#region zooming

				public float CameraSensitivity {
						get {
								return mZoomedInCameraSensitivity;
						}
				}

				protected float mZoomFOV = 0f;
				protected bool mZoomedIn = false;
				protected float mZoomedInCameraSensitivity = 1f;

				#endregion

				#region IItemOfInterest, IAudible, IVisible, motor

				public float VisibilityMultiplier {
						get {
								return mVisibilityMultiplier;
						}
				}

				public float MotorAccelerationMultiplier {
						get {
								return mMotorAccelerationMultiplier;
						}
				}

				public float MotorJumpForceMultiplier {
						get {
								return mMotorJumpForceMultiplier;
						}
				}

				public float MotorSlopeAngleMultiplier {
						get {
								return mMotorSlopeAngleMultiplier;
						}
				}

				protected float mVisibilityMultiplier = 1f;
				protected float mMotorSlopeAngleMultiplier = 1f;
				protected float mMotorJumpForceMultiplier = 1f;
				protected float mMotorAccelerationMultiplier = 1f;

				public override float AudibleRange {
						get {
								return Audio.AudibleRange;
						}
				}

				public override float AudibleVolume {
						get {
								return Audio.AudibleVolume;
						}
				}

				public override float AwarenessDistanceMultiplier {
						get {
								return mAwarenessDistanceMultiplier;
						}
				}

				public override float FieldOfViewMultiplier {
						get {
								return mFieldOfViewMultiplier;
						}
				}

				public override bool IsVisible {
						get {
								return mPlayerIsVisible;
						}
				}

				public override bool IsAudible {
						get {
								return Audio.IsAudible;
						}
				}

				public override void ListenerFailToHear()
				{
						//TODO
						//reward our stealth skills
				}

				public override void LookerFailToSee()
				{
						//TODO
						//reward our stealth skills
				}

				protected float mAwarenessDistanceMultiplier = 1.0f;
				protected float mFieldOfViewMultiplier = 1.0f;
				protected bool mPlayerIsVisible;

				#endregion

				List <string> LightSourceScripts = new List <string>() { "Luminite" };

				public override void FixedUpdate()
				{
						if (!GameManager.Is(FGameState.InGame))
								return;

						if (IsHijacked) {
								return;
						}

						mAwarenessDistanceMultiplier = 1.0f;
						mFieldOfViewMultiplier = 1f;
						mPlayerIsVisible = true;

						for (int i = 0; i < Skills.Get.SkillsInUse.Count; i++) {
								StealthSkill stealthSkill = Skills.Get.SkillsInUse[i] as StealthSkill;
								if (stealthSkill != null) {
										mFieldOfViewMultiplier *= stealthSkill.FieldOfViewMultiplier;
										mAwarenessDistanceMultiplier *= stealthSkill.AwarenessDistanceMultiplier;
										mPlayerIsVisible &= stealthSkill.UserIsVisible;
								}
						}

						mVisibilityMultiplier = Surroundings.LightExposure;

						mMotorAccelerationMultiplier = 1f;
						mMotorJumpForceMultiplier = 1f;
						mMotorSlopeAngleMultiplier = 1f;
						for (int i = 0; i < Scripts.Scripts.Count; i++) {
								Scripts.Scripts[i].AdjustPlayerMotor(ref mMotorAccelerationMultiplier, ref mMotorJumpForceMultiplier, ref mMotorSlopeAngleMultiplier);
						}

						mColliderBounds.center = tr.position;
						mColliderBounds.size = Vector3.one * (ColliderRadius * 2);

						base.FixedUpdate();//RVO simulator
				}

				public void Update()
				{
						if (!mInitialized) {
								return;
						}

						if (State.IsHijacked) {
								GroundPath.Follower.target = HijackedPosition;
								Controller.enabled = false;
								switch (State.HijackMode) {
										case PlayerHijackMode.LookAtTarget:
												HijackedPosition.LookAt(HijackedLookTarget);
												HijackedLookTarget.rotation = HijackedPosition.rotation;
												break;

										case PlayerHijackMode.OrientToTarget:
										default:
												HijackedLookTarget.rotation = HijackedPosition.rotation;
												break;
								}
								//set these to lerp - stuff like tools will lag a bit behind but that's OK
								GameManager.Get.GameCamera.transform.rotation = Quaternion.Lerp(GameManager.Get.GameCamera.transform.rotation, HijackedPosition.transform.rotation, HijackLookSpeed);
								GameManager.Get.GameCamera.transform.position = Vector3.Lerp(GameManager.Get.GameCamera.transform.position, HijackedPosition.transform.position, HijackLookSpeed);
						} else if (!GameManager.Is(FGameState.InGame | FGameState.Cutscene) || !HasSpawned) {
								GroundPath.Follower.target = HijackedPosition;
								Controller.enabled = false;
						} else {
								GroundPath.Follower.target = tr;
								Controller.enabled = true;
								if (mZoomedIn) {
										GameManager.Get.GameCamera.fieldOfView = mZoomFOV;
								} else {
										GameManager.Get.GameCamera.fieldOfView = Profile.Get.CurrentPreferences.Video.FieldOfView;
										mZoomedInCameraSensitivity = 1f;
								}
						}
				}

				protected bool mLookingThroughLooker = false;
				protected bool mDoingEarthquake = false;
				protected bool mWaitingForStartMenu = false;

				[Serializable]
				public class PlayerScriptManager
				{
						public List <PlayerScript> Scripts {
								get {
										return mScripts;
								}
						}

						public PlayerScriptManager()
						{

						}

						public LocalPlayer player;

						public void Initialize()
						{
								foreach (PlayerScript script in mScripts) {
										script.Initialize();
								}
						}

						public void Add(PlayerScript script)
						{
								if (!mScripts.Contains(script)) {
										mScripts.Add(script);
								}
						}

						public void Remove(PlayerScript playerScript)
						{
								mScripts.Remove(playerScript);
						}

						protected List <PlayerScript> mScripts = new List <PlayerScript>();

						public SDictionary <string, string> OnGameSave()
						{
								SDictionary <string, string> scriptStates = new SDictionary <string, string>();
								foreach (PlayerScript script in mScripts) {
										string playerState = string.Empty;
										if (script.SaveState(out playerState)) {
												scriptStates.Add(script.ScriptName, playerState);
										}
								}
								return scriptStates;
						}

						public void LoadState(SDictionary <string, string> scriptStates)
						{
								foreach (PlayerScript script in mScripts) {
										string playerState = string.Empty;
										if (scriptStates.TryGetValue(script.ScriptName, out playerState)) {	//if we have a script state, call UpdatePlayerState
												script.LoadState(playerState);
										}
								}
						}

						public void OnGameLoadStart()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameLoadStart();
								}
						}

						public void OnGameLoadFinish()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameLoadFinish();
								}
						}

						public void OnGameStart()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameStart();
								}
						}

						public void OnGameUnload()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameUnload();
								}
						}

						public void OnGameStartFirstTime()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameStartFirstTime();
								}
						}

						public void OnLocalPlayerSpawn()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnLocalPlayerSpawn();
								}
						}

						public void OnLocalPlayerDespawn()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnLocalPlayerDespawn();
								}
						}

						public void OnRemotePlayerSpawn()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnRemotePlayerSpawn();
								}
						}

						public void OnLocalPlayerDie()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnLocalPlayerDie();
								}
						}

						public void OnRemotePlayerDie()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnRemotePlayerDie();
								}
						}

						public void OnGameSaveStart()
						{
								foreach (PlayerScript script in mScripts) {	//call this whether we had a script state or not
										//do it after all the script states are loaded
										script.OnGameSaveStart();
								}
						}
				}
				#if UNITY_EDITOR
				public float AudibleVolumeGizmo = 0f;

				public void OnDrawGizmos()
				{
						Gizmos.color = Colors.Alpha(Color.red, 0.1f);
						//Gizmos.DrawCube (tr.position, Vector3.one * (ColliderRadius * 2));
						Gizmos.DrawSphere(ChestPosition, 0.5f);

						Gizmos.color = Color.white;
						Gizmos.DrawLine(tr.position, tr.position + (tr.up * 50));
						Gizmos.DrawSphere(tr.position + (tr.up * 50), 5f);

						AudibleVolumeGizmo = Mathf.Lerp(AudibleVolumeGizmo, 0f, (float)Frontiers.WorldClock.ARTDeltaTime);
						if (!IsAudible) {
								AudibleVolumeGizmo = 0f;
						}
						UnityEditor.Handles.color = Colors.Alpha(Color.green, AudibleVolumeGizmo);
						UnityEditor.Handles.DrawWireDisc(Position, Vector3.up, AudibleRange);

						Gizmos.color = Colors.Alpha(Color.Lerp(Color.green, Color.red, Audio.AudibleVolume), Audio.AudibleVolume);
						Gizmos.DrawWireSphere(Position, Audio.AudibleRange);
				}
				#endif
		}

		[Serializable]
		public class PlayerState : Mod
		{
				public void SetDefaults()
				{
						GrabberPosition = new STransform(new Vector3(0f, 0f, 3.75f));
						GrabberTargetPosition = new STransform(new Vector3(0f, 0f, 3.5f));
						FocusObjectPosition = new STransform(new Vector3(0f, 0f, 256f));
						FPSCameraSeatPosition = new STransform(new Vector3(0f, 0f, 0.2f));
						IlluminationPosition = new STransform(new SVector3(0.25f, 2.0f, 0f));
						ToolOffsetPosition = new STransform(new SVector3(0.01375f, -0.1415f, 0.156f));
						IlluminationIntensity = 0.15f;
						IlluminationRange = 8.0f;
				}

				public STransform Transform = new STransform();
				public STransform GrabberPosition = new STransform(new Vector3(0f, 0f, 3.75f));
				public STransform GrabberTargetPosition = new STransform(new Vector3(0f, 0f, 3.5f));
				public STransform FocusObjectPosition = new STransform(new Vector3(0f, 0f, 256f));
				public STransform FPSCameraSeatPosition = new STransform(new Vector3(0f, 0f, 0.2f));
				public STransform IlluminationPosition = new STransform(new SVector3(0.25f, 2.0f, 0f));
				public STransform ToolOffsetPosition = new STransform(new SVector3(0.01375f, -0.1415f, 0.156f));
				public STransform CarryOffsetPosition = new STransform(new SVector3(-0.01375f, -0.1415f, 0.156f));
				public float IlluminationIntensity = 0.15f;
				public float IlluminationRange = 8.0f;
				public bool HasSpawnedFirstTime = false;
				public bool HasSpawned = false;
				public bool IsHijacked = false;
				public bool MovementLocked = false;
				public bool CameraLocked = false;
				public bool IsDead = false;
				public PlayerHijackMode HijackMode = PlayerHijackMode.LookAtTarget;
				public SDictionary <string, string> ScriptStates = new SDictionary <string, string>();
		}
}
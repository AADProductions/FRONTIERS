using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;
using ExtensionMethods;

public class WaterLeviathan : MonoBehaviour, IItemOfInterest, IHostile
{
		public Transform MouthObject;
		public ParticleSystem Wake;
		public List <Renderer> Renderers = new List<Renderer>();
		SubmergedObject PrimaryTargetObject = null;

		public void Awake()
		{
				Wake.enableEmission = false;
		}

		#region IHostile implementation

		public IItemOfInterest hostile { get { return this; } }

		public string DisplayName { get { return "Leviathan"; } }

		public IItemOfInterest PrimaryTarget {
				get {
						if (PrimaryTargetObject != null) {
								if (PrimaryTargetObject.IsOfInterest) {
										return PrimaryTargetObject.Target;
								}
								PrimaryTargetObject = null;
						}
						return null;
				}
		}

		public bool HasPrimaryTarget { get { return PrimaryTarget != null; } }

		public bool CanSeePrimaryTarget { get { return mCanSeePrimaryTarget; } }

		public HostileMode Mode { get { return mMode; } }

		#endregion

		#region ItemOfInterest implementation

		public ItemOfInterestType IOIType { get { return ItemOfInterestType.Scenery; } }

		public Vector3 Position { get { return MouthObject.position; } }

		public bool Has(string scriptName)
		{
				return false;
		}

		public bool HasAtLeastOne(List <string> scriptNames)
		{
				return false;
		}

		public void CoolOff ( ) {
				mMode = HostileMode.CoolingOff;
				if (HasPrimaryTarget) {
						PrimaryTargetObject.IsOfInterest = false;
				}
		}

		public WorldItem worlditem { get { return null; } }

		public PlayerBase player { get { return null; } }

		public ActionNode node { get { return null; } }

		public WorldLight worldlight { get { return null; } }

		public Fire fire { get { return null; } }

		public bool Destroyed { get { return mDestroyed; } }

		public bool HasPlayerFocus { get; set; }

		protected bool mDestroyed = false;

		public void OnDestroy()
		{
				mDestroyed = true;
		}

		#endregion

		public bool FindPrimaryTargetObject()
		{
				//Debug.Log ("Searching for primary target...");
				if (!HasPrimaryTarget) {
						//start over and try to find a new one
						return Ocean.Get.SortSubmergedObjects(this, Globals.LeviathanRTLoseInterestInterval, out PrimaryTargetObject);
				} else if (PrimaryTargetObject.HasExitedWater && (WorldClock.AdjustedRealTime - PrimaryTargetObject.TimeExitedWater) < Globals.LeviathanRTLoseInterestInterval) {
						//no longer interested
						PrimaryTargetObject = null;
				} else if (!PrimaryTargetObject.IsOfInterest) {
						//no longer interested
						PrimaryTargetObject = null;
				}
				if (PrimaryTargetObject != null) {
						if (PrimaryTargetObject.Target.IOIType == ItemOfInterestType.Player) {
								//we're targeting the player
								//are we allowed to be hostile?
								if (Profile.Get.CurrentGame.Difficulty.IsDefined("NoHostileCreatures")) {
										//if not, we didn't find anything
										PrimaryTargetObject = null;
								} else {
										//if so, alert the player
										Player.Local.Surroundings.AddHostile(this);
								}
						}
						return true;
				}
				return false;
		}

		public void OnGameStart()
		{
				SetVisible(false);

				mDamagePackage.DamageSent = 1000;
				mDamagePackage.SenderMaterial = WIMaterialType.Bone;
				mDamagePackage.SenderName = DisplayName;

				StartCoroutine(UpdateLeviathan());
		}

		protected void OnAwaken()
		{
				//Debug.Log ("Awakening...");
				//place ourselves within [x] meters of transform
				SetVisible(false);
				Vector3 awakenPosition = Ocean.Get.RandomPointOnOceanSurface(PrimaryTarget.Position, Globals.LeviathanStartDistance, true);
				transform.position = awakenPosition;
				mMode = HostileMode.Warning;
		}

		protected void OnWarn()
		{
				//Debug.Log ("Warning...");
				if (Listener.IsInAudibleRange(Player.Local.Position, Position, Player.Local.AudibleRange, Globals.LeviathanMaxAudibleRange)) {
						MasterAudio.PlaySound(MasterAudio.SoundType.Leviathan, transform, "Awaken");
				}
				FXManager.Get.SpawnFX(transform.position.WithY(Ocean.Get.OceanSurfaceHeight), "Leviathan Splash");
				mMode = HostileMode.Stalking;
		}

		protected IEnumerator OnAttack()
		{
				//have to be careful here
				if (HasPrimaryTarget) {
						IItemOfInterest target = PrimaryTarget;
						transform.position = PrimaryTarget.Position;
						mDamagePackage.Point = MouthObject.position;
						mDamagePackage.Origin = MouthObject.position;
						SetVisible(true);
						animation.Play();
						if (Listener.IsInAudibleRange(Player.Local.Position, Position, Player.Local.AudibleRange, Globals.LeviathanMaxAudibleRange)) {
								MasterAudio.PlaySound(MasterAudio.SoundType.Leviathan, transform, "Attack");
								MasterAudio.PlaySound(MasterAudio.SoundType.JumpLandWater, transform, "Land");
						}
						FXManager.Get.SpawnFX(transform.position.WithY(Ocean.Get.OceanSurfaceHeight), "Leviathan Splash");
						mDamagePackage.Target = target;
						mDamagePackage.InstantKill = true;
						DamageManager.Get.SendDamage(mDamagePackage);
						//pretty much a guarantee that this will instakill (TODO add instakill option to damage packages?)
						PrimaryTargetObject = null;

						while (animation.isPlaying) {
								yield return null;
						}

						SetVisible(false);
				}
				mMode = HostileMode.CoolingOff;
				yield break;
		}

		protected void SetVisible(bool visbile)
		{
				foreach (Renderer renderer in Renderers) {
						renderer.enabled = visbile;
				}
		}

		protected void BlowBubbles()
		{
				Vector3 bubblesPosition = transform.position;
				bubblesPosition.y = Ocean.Get.OceanTopCollider.bounds.max.y;
				FXManager.Get.SpawnFX(bubblesPosition, "Water Surface Bubbles");
				if (Listener.IsInAudibleRange(Player.Local.Position, Position, Player.Local.AudibleRange, Globals.LeviathanMaxAudibleRange)) {
						MasterAudio.PlaySound(MasterAudio.SoundType.AnimalVoice, transform, "CreatureSwim");
				}
		}

		public void LateUpdate()
		{
				if (Wake.enableEmission) {
						Vector3 wakePosition = Wake.transform.position;
						wakePosition.y = Ocean.Get.OceanSurfaceHeight;
						Wake.transform.position = wakePosition;
				}
		}

		protected WaitForSeconds mWaitForUpdate = new WaitForSeconds(0.15f);

		protected IEnumerator UpdateLeviathan()
		{
				while (GameManager.State != FGameState.Quitting) {
						while (!GameManager.Is(FGameState.InGame) || !GameWorld.Get.WorldLoaded) {
								yield return null;
						}

						yield return mWaitForUpdate;
						FindPrimaryTargetObject();

						switch (mMode) {
								case HostileMode.CoolingOff:
								case HostileMode.Dormant:
										Wake.enableEmission = false;
										if (HasPrimaryTarget) {
												OnAwaken();
										} else {
												yield return null;
										}
										break;

								case HostileMode.Warning:
										OnWarn();
										break;

								case HostileMode.Stalking:
										Wake.enableEmission = true;
										if (!FindPrimaryTargetObject()) {
												mMode = HostileMode.Dormant;
										} else {
												//now that we've found a target, start moving towards it
												yield return StartCoroutine(StalkTargetOverTime());
										}
										break;

								case HostileMode.Attacking:
										yield return StartCoroutine(OnAttack());
										break;
						}
				}
		}

		protected IEnumerator StalkTargetOverTime()
		{
				double startStalkTime = WorldClock.AdjustedRealTime;
				double lastBubblesTime = WorldClock.AdjustedRealTime;
				//Debug.Log ("Stalking...");
				//this makes the leviathan stalk targets for a period of time
				while (WorldClock.AdjustedRealTime < (startStalkTime + Globals.LeviathanRTMinimumStalkInterval)) {
						yield return null;
						if (WorldClock.AdjustedRealTime - lastBubblesTime > Globals.LeviathanRTBlowBubblesInterval) {
								lastBubblesTime = WorldClock.AdjustedRealTime;
								BlowBubbles();
						}
						//move towards the target
						if (!HasPrimaryTarget) {//oops, time to find a new target
								mMode = HostileMode.Dormant;
								yield break;
						}

						Vector3 targetPosition = PrimaryTarget.Position.To2D();
						Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, Globals.LeviathanMoveSpeed).To2D();
						transform.position = newPosition;
						if (!PrimaryTargetObject.HasExitedWater) {
								if (Vector3.Distance(targetPosition, newPosition) < Globals.LeviathanMinimumAttackDistance) {
										//time to attack the thing!
										mMode = HostileMode.Attacking;
										yield break;
								}
						}
				}
				yield break;
		}

		protected bool mCanSeePrimaryTarget;
		protected DamagePackage mDamagePackage = new DamagePackage();
		protected HostileMode mMode = HostileMode.Dormant;
}
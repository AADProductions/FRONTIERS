using UnityEngine;
using Frontiers.Data;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Serialization;
using Frontiers;

namespace Frontiers.World.BaseWIScripts
{
		public class Hostile : WIScript, IHostile
		{		//used by both characters and creatures
				//attacks player with 1 of 2 attacks
				//uses a strict pattern - watch, stalk, warn, attack
				//this script has problems with hostiles not attacking due to
				//RVOTargetHolder range - TODO need to make this script adjust target range
				public HostileState State = new HostileState();

				public Motile motile = null;
				public Looker looker = null;
				WorldBody body = null;
				public int NumTimesWarned;
				public float Attack1MinimumDistance = 0f;
				public float Attack2MinimumDistance = 0f;
				public float Attack1MaximumDistance = 0f;
				public float Attack2MaximumDistance = 0f;

				public float AttackMinimumDistance {
						get {
								return Mathf.Max(Attack1MinimumDistance, Attack2MinimumDistance);
						}
				}

				public float AttackMaximumDistance {
						get {
								return Mathf.Min(Attack1MaximumDistance, Attack2MaximumDistance);
						}
				}

				public BodyPart Attack1BodyPart;
				public BodyPart Attack2BodyPart;
				public float DistanceToTarget;

				public override void OnInitialized()
				{
						if (Profile.Get.CurrentGame.Difficulty.IsDefined("NoHostileCreatures") && worlditem.Is <Creature>()) {
								Finish();
								return;
						} else if (Profile.Get.CurrentGame.Difficulty.IsDefined("NoHostileCharacters") && worlditem.Is <Character>()) {
								Finish();
								return;
						}

						Damageable damageable = worlditem.Get <Damageable>();
						damageable.OnDie += OnDie;
						mStalkAction = new MotileAction();
						mWatchAction = new MotileAction();
						mWarnAction = new MotileAction();
						motile = worlditem.Get <Motile>();
						looker = worlditem.Get <Looker>();
						body = motile.Body;

						if (body == null) {
								Finish();
								return;
						}

						RefreshAttackSettings();

						Player.Local.Surroundings.AddHostile(this);
				}

				public void RefreshAttackSettings()
				{
						//calculate minimum attack distances, get body parts
						if (!body.GetBodyPart(State.Attack1.AttackSourceType, out Attack1BodyPart)) {
								Attack1BodyPart = body.RootBodyPart;
						}
						if (!body.GetBodyPart(State.Attack2.AttackSourceType, out Attack2BodyPart)) {
								Attack2BodyPart = body.RootBodyPart;
						}
						//attack minimum distance = distance from body part to center of body plus attack radius
						if (State.Attack1.RangedAttack) {
								Attack1MinimumDistance = State.Attack1.MinDistance;
								Attack1MaximumDistance = State.Attack1.MaxDistance;
						} else {
								Attack1MinimumDistance = State.Attack1.AttackRadius + Vector3.Distance(Attack1BodyPart.tr.position, worlditem.tr.position);
								Attack1MaximumDistance = Attack1MinimumDistance;
						}

						if (State.Attack2.RangedAttack) {
								Attack2MinimumDistance = State.Attack2.MinDistance;
								Attack2MaximumDistance = State.Attack2.MaxDistance;
						} else {
								Attack2MinimumDistance = State.Attack2.AttackRadius + Vector3.Distance(Attack2BodyPart.tr.position, worlditem.tr.position);
								Attack2MaximumDistance = Attack2MinimumDistance;
						}

						State.Attack1.RTPreAttackInterval = Mathf.Max(0.25f, State.Attack1.RTPreAttackInterval);
						State.Attack1.RTPostAttackInterval = Mathf.Max(0.5f, State.Attack1.RTPostAttackInterval);


						//ColoredDebug.Log("HOSTILE: Refreshing attack settings - " + AttackMinimumDistance.ToString() + ", " + AttackMaximumDistance.ToString(), "Red");
				}

				#region IHostile implementation

				public IItemOfInterest hostile { get { return worlditem; } }

				public string DisplayName { get { return worlditem.DisplayName; } }

				public IItemOfInterest PrimaryTarget { get { return mPrimaryTarget; } set { SetPrimaryTarget(value); } }

				public bool HasPrimaryTarget { get { return PrimaryTarget != null; } }

				public bool CanSeePrimaryTarget { get { return mCanSeePrimaryTarget; } }

				public HostileMode Mode { get; set; }

				#endregion

				public bool CanAttack {
						get {
								return (!mAttackingNow && DistanceToTarget < AttackMinimumDistance) && (DistanceToTarget < AttackMaximumDistance) && mCanSeePrimaryTarget;
						}
				}

				public bool AttackingNow { 
						get {
								return mAttackingNow;
						}
				}

				public Action OnWarn;
				public Action OnAttack1Start;
				public Action OnAttack2Start;
				public Action <DamagePackage> OnAttack1Hit;
				public Action <DamagePackage> OnAttack2Hit;
				public Action OnAttack1Finish;
				public Action OnAttack2Finish;
				public Action OnStalk;
				public Action OnCoolOff;

				public void OnDie()
				{
						Mode = HostileMode.None;
						Finish();
				}

				public void CoolOff()
				{
						Mode = HostileMode.CoolingOff;
						enabled = false;
						StartCoroutine(CoolOffOverTime());
				}

				protected void SetPrimaryTarget(IItemOfInterest newTarget)
				{
						if (newTarget == null) {
								return;
						}

						if (mFinished || Mode == HostileMode.CoolingOff) {
								return;
						}

						if (newTarget == mPrimaryTarget) {
								return;
						}

						mPrimaryTarget = newTarget;
						mCanSeePrimaryTarget = true;//set true for default now
						mTimeLastSawPrimaryTarget = WorldClock.AdjustedRealTime;

						Watch();

						enabled = true;
				}

				public void Stalk()
				{
						if (!mWatchAction.IsFinished) {
								mWatchAction.TryToFinish();
						}
						Mode = HostileMode.Stalking;
						//start stalking our target
						mStalkAction.Reset();
						mStalkAction.LiveTarget = mPrimaryTarget;
						mStalkAction.Type = MotileActionType.FollowTargetHolder;
						mStalkAction.Method = MotileGoToMethod.Pathfinding;
						mStalkAction.FollowType = MotileFollowType.Stalker;
						mStalkAction.Expiration = MotileExpiration.Duration;
						mStalkAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
						mStalkAction.RTDuration = State.StalkTime;

						motile.PushMotileAction(mStalkAction, MotileActionPriority.ForceTop);

						OnStalk.SafeInvoke();
				}

				public void Attack()
				{
						Mode = HostileMode.Attacking;

						mStalkAction.LiveTarget = mPrimaryTarget;
						mStalkAction.Type = MotileActionType.FollowTargetHolder;
						mStalkAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
						mStalkAction.FollowType = MotileFollowType.Attacker;
						mStalkAction.Expiration = MotileExpiration.TargetOutOfRange;
						mStalkAction.OutOfRange = State.PursuitDistance;
						//the range of our attacks is determined by the distance from the attack origin to our center point
						//plus the attack radius
						//use the max in both cases
						mStalkAction.Range = Mathf.Max(State.Attack1.AttackRadius, State.Attack2.AttackRadius);

						if (mStalkAction.IsFinished) {
								mStalkAction.Reset();
								motile.PushMotileAction(mStalkAction, MotileActionPriority.ForceTop);
						}
				}

				public void Watch()
				{
						Mode = HostileMode.Dormant;

						mWatchAction.Reset();
						mWatchAction.Type = MotileActionType.FocusOnTarget;
						mWatchAction.LiveTarget = mPrimaryTarget;
						mWatchAction.Expiration = MotileExpiration.Duration;
						mWatchAction.RTDuration = 5f;
						mWatchAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
						motile.PushMotileAction(mWatchAction, MotileActionPriority.ForceTop);
				}

				public void Warn()
				{
						if (!mStalkAction.IsFinished) {
								mStalkAction.TryToFinish();
						}

						Mode = HostileMode.Warning;

						mWarnAction.Reset();
						mWarnAction.Type = MotileActionType.FollowTargetHolder;
						mWarnAction.LiveTarget = mPrimaryTarget;
						mWarnAction.Expiration = MotileExpiration.TargetInRange;
						mWarnAction.Range = State.CautionDistance;
						mWarnAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
						motile.PushMotileAction(mWarnAction, MotileActionPriority.ForceTop);
				}

				public void FixedUpdate()
				{
						if (!HasPrimaryTarget) {
								if (Mode != HostileMode.Dormant) {
										Finish();
								}
								enabled = false;
								return;
						}

						if (body.IsRagdoll) {
								Finish();
								enabled = false;
								return;
						}

						//check this every fixed update
						DistanceToTarget = Vector3.Distance(mPrimaryTarget.Position, worlditem.tr.position);
						//do this only every once in a while
						mCheckTargetVisibility++;
						if (mCheckTargetVisibility > 5) {
								CheckTargetVisibility();
						}

						if (!CanSeePrimaryTarget) {
								//wait for a while - if we still can't see the target after max time, cool off / finish
								if (WorldClock.AdjustedRealTime > (mTimeLastSawPrimaryTarget + State.SeekTargetTime)) {
										CoolOff();
										return;
								}
						}

						//wait till we're done attacking before we try to update our state
						if (!AttackingNow) {
								//regardless of our state, check to see if we *can* attack
								if (CanAttack) {
										Debug.Log("We can attack, so attack now");
										//if we can attack, do it and forget the rest
										mAttackingNow = true;
										StartCoroutine(AttackImmediately());
										return;
								} else {
										//otherwise update our mode and whatnot
										switch (Mode) {
												case HostileMode.Dormant:
												default:
														if (mWatchAction.IsFinished) {
																Stalk();
														} else if (DistanceToTarget < State.CautionDistance) {
																Stalk();
														}
														break;

												case HostileMode.Stalking:
														if (mStalkAction.IsFinished) {
																Warn();
														} else if (!mStalkingOverTime) {
																mStalkingOverTime = true;
																StartCoroutine(StalkOverTime());
														}
														break;

												case HostileMode.Warning:
														if (NumTimesWarned >= State.NumWarnings) {
																Attack();
														} else if (!mWarningOverTime) {
																mWarningOverTime = true;
																StartCoroutine(WarnOverTime());
														}
														break;

												case HostileMode.Attacking:
														if (!mAttackingOverTime) {
																mAttackingOverTime = true;
																StartCoroutine(AttackOverTime());
														}
														break;

												case HostileMode.CoolingOff:
														break;
										}
								}
						}
				}

				public void CheckTargetVisibility()
				{
						if (!mInitialized) {
								//this is a safe bet if we're not initialized yet
								mCanSeePrimaryTarget = true;
								return;
						}

						if (looker == null) {
								//Debug.Log("LOOKER NULL");
								//TODO find a way to prevent this
								mCanSeePrimaryTarget = true;
								return;
						}
						//don't require FOV for visibility
						//since creatures will be stalking and walking in funny directions
						if (looker.CanSeeItemOfInterest(PrimaryTarget, false)) {
								mCanSeePrimaryTarget = true;
								mTimeLastSawPrimaryTarget = WorldClock.AdjustedRealTime;
						} else {
								mCanSeePrimaryTarget = false;
						}
				}

				public IEnumerator AttackImmediately()
				{
						Debug.Log("Attacking immediately");
						RefreshAttackSettings();
						//get random attack style
						mAttackingNow = true;
						AttackStyle style = State.Attack1;
						Action startAction = OnAttack1Start;
						Action <DamagePackage> attackAction = OnAttack1Hit;
						Action finishAction = OnAttack1Finish;
						BodyPart attackOrigin = Attack1BodyPart;
						//see if we're in range for attack2
						//and make sure we're either NOT ranged or our range is within the max
						if (DistanceToTarget < Attack2MinimumDistance && (!State.Attack2.RangedAttack || DistanceToTarget < Attack2MaximumDistance)) {
								style = State.Attack2;
								startAction = OnAttack2Start;
								attackAction = OnAttack2Hit;
								finishAction = OnAttack2Finish;
								attackOrigin = Attack2BodyPart;
						}
						State.LastAttemptedAttackWTime = WorldClock.AdjustedRealTime;
						startAction.SafeInvoke();
						DamagePackage damage = style.Damage;
						//wait until the attack is supposed to hit
						double waitUntil = WorldClock.AdjustedRealTime + style.RTPreAttackInterval;
						while (waitUntil > WorldClock.AdjustedRealTime) {
								/*if (Mode != HostileMode.Attacking) {
										mAttackingNow = false;
										yield break;
								}*/
								yield return null;
						}
						yield return null;
						//now check and see if we actually hit
						//by default use the position in case we don't have a body
						//or in case we aren't motile
						mLastAttackOrigin = attackOrigin.tr.position;
						mAttackBodyPart = attackOrigin.tr;
						mAttackRange = style.AttackRadius;
						mLastAttackHitPoint = mPrimaryTarget.Position;//we'll refine this in a moment
						style.Refresh(mLastAttackOrigin, mLastAttackHitPoint, worlditem.DisplayName, mPrimaryTarget);
						//initialize the damage package
						damage.SenderMaterial = style.AttackSourceMaterial;
						damage.Point = mLastAttackHitPoint;
						damage.DamageSent = UnityEngine.Random.Range(style.MinDamage, style.MaxDamage);
						damage.Target = mPrimaryTarget;
						damage.Source = worlditem;
						damage.HasLineOfSight = true;
						//if a script wants to check line of sight (esp for ranged attacks)
						//call that delegate now
						//it will determine line of sight
						if (attackAction != null) {
								attackAction(damage);
						}
						//if we have line of sight, send the package
						if (damage.HasLineOfSight) {
								DamageManager.Get.SendDamage(damage);
						}
						//did we hit? record our success
						if (damage.HitTarget) {
								State.NumSuccessfulAttacks++;
						} else {
								State.NumUnsuccessfulAttacks++;
						}

						if (damage.TargetIsDead) {
								//we're done being hostile
								mAttackingNow = false;
								Finish();
								yield break;
						}
						//wait out the required amount of time before attacking again
						waitUntil = WorldClock.AdjustedRealTime + style.RTPostAttackInterval;
						while (waitUntil > WorldClock.AdjustedRealTime) {
								/*if (Mode != HostileMode.Attacking) {
										mAttackingNow = false;
										yield break;
								}*/
								yield return null;
						}
						yield return null;
						finishAction.SafeInvoke();
						mAttackingNow = false;
				}

				protected IEnumerator StalkOverTime()
				{
						mStalkingOverTime = true;
						//wait for the action to start before waiting
						var waitForAction = mStalkAction.WaitForActionToStart(0.1f);
						while (waitForAction.MoveNext()) {
								yield return waitForAction.Current;
						}

						OnStalk.SafeInvoke();

						while (!mStalkAction.IsFinished && Mode == HostileMode.Stalking) {
								//if the stalk action is finished
								//that means our stalk time is up
								yield return null;
						}
						//if our state isn't stalking
						//and we're not finished
						//we don't need this any more
						mStalkingOverTime = false;
						yield break;
				}

				protected IEnumerator WarnOverTime()
				{
						mWarningOverTime = true;
						var waitForAction = mWarnAction.WaitForActionToStart(0.1f);
						while (waitForAction.MoveNext()) {
								yield return waitForAction.Current;
						}
						OnWarn.SafeInvoke();
						//even if the action is cancelled warn at least once before leaving
						//start the warning
						double waitUntil = WorldClock.AdjustedRealTime + State.Attack1.RTPreAttackInterval;
						while (waitUntil > WorldClock.AdjustedRealTime) {
								if (Mode != HostileMode.Warning) {
										mWarningOverTime = false;
										yield break;
								}
								yield return null;
						}
						//send this message so the character / creature / whatever can emit sounds and effects and set animation
						body.Animator.Warn = true;
						//finish the warning
						waitUntil = WorldClock.AdjustedRealTime + State.Attack1.RTPostAttackInterval;
						while (waitUntil > WorldClock.AdjustedRealTime) {
								if (Mode != HostileMode.Warning) {
										mWarningOverTime = false;
										yield break;
								}
								yield return null;
						}
						NumTimesWarned++;
						mWarningOverTime = false;
						yield break;
				}

				protected IEnumerator AttackOverTime()
				{
						var waitForAction = mStalkAction.WaitForActionToStart(0.1f);
						while (waitForAction.MoveNext()) {
								yield return waitForAction.Current;
						}

						while (!mStalkAction.IsFinished && Mode == HostileMode.Attacking) {
								if (CanAttack) {
										var attackImmediately = AttackImmediately();
										while (attackImmediately.MoveNext()) {
												yield return attackImmediately.Current;
										}
								}
								yield return null;
						}
						mAttackingOverTime = false;
						yield break;
				}

				protected IEnumerator CoolOffOverTime()
				{
						mCoolingOff = true;
						mStalkAction.TryToFinish();
						mWarnAction.TryToFinish();
						mWatchAction.TryToFinish();

						OnCoolOff.SafeInvoke();
						mPrimaryTarget = null;
						double waitUntil = WorldClock.AdjustedRealTime + 0.5f;
						while (waitUntil > WorldClock.AdjustedRealTime) {
								yield return null;
						}
						Finish();
						mCoolingOff = false;
						yield break;
				}

				public override void OnFinish()
				{
						Player.Local.Surroundings.RemoveHostile(this);

						if (mStalkAction != null) {
								mStalkAction.Cancel();
						}

						base.OnFinish();
				}

				public void OnDrawGizmos()
				{
						Gizmos.color = Colors.Alpha(Color.red, 0.25f);

						Gizmos.DrawSphere(transform.position, 0.5f);

						Gizmos.color = Colors.Alpha(Color.yellow, 0.5f);
						if (Mode == HostileMode.Attacking && mAttackBodyPart != null) {
								Gizmos.DrawSphere(mAttackBodyPart.position, mAttackRange);
						}

						Gizmos.color = Color.red;
						Gizmos.DrawLine(mLastAttackOrigin, mLastAttackHitPoint);

						if (mLastAttackOrigin != Vector3.zero) {
								Gizmos.color = Color.green;
								Gizmos.DrawLine(mLastAttackOrigin, mLastAttackTargetDirection);
						}
				}

				protected MotileAction mStalkAction;
				protected MotileAction mWatchAction;
				protected MotileAction mWarnAction;
				protected bool mHasAddedAction = false;
				protected float mLastAttackTime;
				protected Vector3 mLastAttackOrigin;
				protected Vector3 mLastAttackTargetDirection;
				protected Vector3 mLastAttackHitPoint;
				protected IItemOfInterest mPrimaryTarget;
				protected bool mCanSeePrimaryTarget;
				protected int mCheckTargetVisibility = 0;
				protected double mTimeLastSawPrimaryTarget = 0f;
				protected Transform mAttackBodyPart;
				protected float mAttackRange;
				protected bool mCoolingOff = false;
				protected bool mStalkingOverTime = false;
				protected bool mWarningOverTime = false;
				protected bool mAttackingOverTime = false;
				protected bool mUpdatingHostileTargeting = false;
				protected bool mAttackingNow = false;
		}

		[Serializable]
		public class HostileState
		{
				public MobileReference TargetReference = new MobileReference();
				public float StalkTime = 10f;
				//how long to stalk before warning the target
				public int NumWarnings = 1;
				//how many times to warn the target before attacking
				public double CooldownTime = 5f;
				//how long until no longer hostile after losing target
				public double Attack2Frequency = 0.5f;
				//how often attack2 style will be chosen
				public AttackStyle Attack1 = new AttackStyle();
				public AttackStyle Attack2 = new AttackStyle();
				//how close the player can be before the creature stops watching
				public float CautionDistance = 5f;
				public float SeekTargetTime = 30f;
				//how far the creature will pursue the player before quitting
				public float PursuitDistance = 25f;
				public int NumSuccessfulAttacks = 0;
				public int NumUnsuccessfulAttacks = 0;
				public double LastAttemptedAttackWTime = 0f;
		}

		[Serializable]
		public class AttackStyle
		{
				public void Refresh(Vector3 origin, Vector3 attackPoint, string senderName, IItemOfInterest target)
				{
						Damage.DamageSent = UnityEngine.Random.Range(MinDamage, MaxDamage);
						Damage.SenderMaterial = AttackSourceMaterial;
						Damage.Origin = origin;
						Damage.Point = attackPoint;
						Damage.SenderName = senderName;
						Damage.Target = target;
				}

				public DamagePackage Damage = new DamagePackage();
				public bool RangedAttack = false;
				public bool Attack1 = true;
				public float MinDamage = 5f;
				public float MaxDamage = 10f;
				public float RTPreAttackInterval = 0.25f;
				//how long before the damage hits
				public float RTPostAttackInterval = 0.5f;
				//how long after the damage hits
				public float MinDistance = 3f;
				public float MaxDistance = -1f;
				//spherecast radius
				public float AttackRadius = 0.25f;
				public WIMaterialType AttackSourceMaterial = WIMaterialType.Bone;
				//usually teeth
				public BodyPartType AttackSourceType = BodyPartType.Head;
		}
}
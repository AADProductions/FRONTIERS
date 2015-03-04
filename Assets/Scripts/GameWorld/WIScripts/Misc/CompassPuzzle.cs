using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
		public class CompassPuzzle : WIScript
		{
				public Transform FirstLockObject;
				public Transform SecondLockObject;
				public Transform ThirdLockObject;
				public Renderer FirstLockHighlight;
				public Renderer SecondLockHighlight;
				public Renderer ThirdLockHighlight;
				public Collider FirstLockCollider;
				public Collider SecondLockCollider;
				public Collider ThirdLockCollider;
				public MapDirection FirstLockUnlockPosition;
				public MapDirection SecondLockUnlockPosition;
				public MapDirection ThirdLockUnlockPosition;
				public Animation CompartmentAnimation;
				public string CompartmentOpenAnimation;
				public string CompartmentCloseAnimation;
				public Collider LastColliderInFocus;
				public AudioSource StoneGrindingAudio;
				public string UnlockSound;

				public override Transform HudTargeter()
				{
						return Player.Local.Focus.FocusTransform;
				}

				public override void OnInitialized()
				{
						if (mLockTarget == null) {
								mLockTarget = new GameObject("LockTarget").transform;
						}
						worlditem.HudTargeter = new HudTargetSupplier(HudTargeter);
						worlditem.OnAddedToGroup += OnAddedToGroup;
						worlditem.OnPlayerUse += OnPlayerUse;
						worlditem.OnGainPlayerFocus += OnGainPlayerFocus;

						CompartmentAnimation.wrapMode = WrapMode.ClampForever;

						FirstLockHighlight.sharedMaterial.SetColor("_Color", Colors.Get.GeneralHighlightColor);
				}

				public void OnAddedToGroup()
				{
						Transform firstLockParent = FirstLockObject.parent;
						Transform secondLockParent = SecondLockObject.parent;
						Transform thirdLockParent = ThirdLockObject.parent;

						FirstLockObject.parent = null;
						SecondLockObject.parent = null;
						ThirdLockObject.parent = null;

						if (State.HasBeenUnlocked) {
								//set the hidden bits to be open
								CompartmentAnimation.Play(CompartmentOpenAnimation);
								//set it to the targets immediately
								//unparent them all so they don't rotate each other
								FirstLockObject.rotation = Quaternion.Euler(0f, (int)FirstLockUnlockPosition, 0f);
								SecondLockObject.rotation = Quaternion.Euler(0f, (int)SecondLockUnlockPosition, 0f);
								ThirdLockObject.rotation = Quaternion.Euler(0f, (int)ThirdLockUnlockPosition, 0f);
						} else {
								if (State.HasBeenAltered) {
										//set to last state immediately
										FirstLockObject.rotation = Quaternion.Euler(0f, (int)State.FirstLockDirection, 0f);
										SecondLockObject.rotation = Quaternion.Euler(0f, (int)State.SecondLockDirection, 0f);
										ThirdLockObject.rotation = Quaternion.Euler(0f, (int)State.ThirdLockDirection, 0f);
								} else {
										//get the state from whatever the default state is
										State.FirstLockDirection = WorldMap.GetMapDirectionFromRotation(FirstLockObject);
										State.SecondLockDirection = WorldMap.GetMapDirectionFromRotation(SecondLockObject);
										State.ThirdLockDirection = WorldMap.GetMapDirectionFromRotation(ThirdLockObject);
								}
						}

						FirstLockObject.parent = firstLockParent;
						SecondLockObject.parent = secondLockParent;
						ThirdLockObject.parent = thirdLockParent;

						FirstLockObject.gameObject.layer = Globals.LayerNumWorldItemActive;
						SecondLockObject.gameObject.layer = Globals.LayerNumWorldItemActive;
						ThirdLockObject.gameObject.layer = Globals.LayerNumWorldItemActive;
				}

				public void OnPlayerUse()
				{
						if (State.HasBeenUnlocked) {
								return;
						}
						//Debug.Log("On player use in compass puzzle");
						if (!mRotatingLock && LastColliderInFocus != null) {
								Transform lockTransform = null;
								MapDirection unlockMapDirection = MapDirection.A_North;

								if (LastColliderInFocus == FirstLockCollider) {
										lockTransform = FirstLockObject;
										unlockMapDirection = FirstLockUnlockPosition;
								} else if (LastColliderInFocus == SecondLockCollider) {
										lockTransform = SecondLockObject;
										unlockMapDirection = SecondLockUnlockPosition;
								} else if (LastColliderInFocus == ThirdLockCollider) {
										lockTransform = ThirdLockObject;
										unlockMapDirection = ThirdLockUnlockPosition;
								}

								if (lockTransform != null) {
										mRotatingLock = true;
										worlditem.RefreshHud();
										StartCoroutine(RotateLock(lockTransform, WorldMap.GetClockwiseMapDirection(lockTransform), unlockMapDirection));
								} else {
										Debug.Log("Not looking at one of our three colliders, returning");
								}
						}
				}

				public CompassPuzzleState State = new CompassPuzzleState();

				public void OnGainPlayerFocus()
				{
						enabled = true;
				}

				public void Update()
				{
						if (!worlditem.HasPlayerFocus) {
								FirstLockHighlight.enabled = false;
								SecondLockHighlight.enabled = false;
								ThirdLockHighlight.enabled = false;
								LastColliderInFocus = null;
								enabled = false;
								return;
						}

						//check to see which collider we're looking at
						if (Player.Local.Surroundings.WorldItemFocusHitInfo.collider != null) {
								if (LastColliderInFocus != Player.Local.Surroundings.WorldItemFocusHitInfo.collider) {
										LastColliderInFocus = Player.Local.Surroundings.WorldItemFocusHitInfo.collider;
										worlditem.RefreshHud();
								}
						} else {
								if (LastColliderInFocus != null) {
										LastColliderInFocus = null;
										worlditem.RefreshHud();
								}
						}
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						FirstLockHighlight.enabled = false;
						SecondLockHighlight.enabled = false;
						ThirdLockHighlight.enabled = false;

						if (State.HasBeenUnlocked || mRotatingLock) {
								return lastHudPriority;
						}

						if (worlditem.HasPlayerFocus && LastColliderInFocus != null) {
								if (LastColliderInFocus == FirstLockCollider) {
										FirstLockHighlight.enabled = true;
								} else if (LastColliderInFocus == SecondLockCollider) {
										SecondLockHighlight.enabled = true;
								} else if (LastColliderInFocus == ThirdLockCollider) {
										ThirdLockHighlight.enabled = true;
								} else {		
										return lastHudPriority;
								}
								lastHudPriority++;
								GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Rotate", worlditem.HudTarget, GameManager.Get.GameCamera);
						}
						return lastHudPriority;
				}

				public IEnumerator UnlockOverTime()
				{
						CompartmentAnimation.Play(CompartmentOpenAnimation);
						StoneGrindingAudio.Play();
						while (CompartmentAnimation[CompartmentOpenAnimation].normalizedTime < 1f) {
								yield return null;
						}
						StoneGrindingAudio.Stop();
						yield break;
				}

				public IEnumerator RotateLock(Transform lockTransform, MapDirection targetDirection, MapDirection unlockDirection)
				{
						State.HasBeenAltered = true;
						Debug.Log("Rotating " + lockTransform.name + " to " + targetDirection.ToString());
						//mLockTarget is a tool that helps me debug TODO remove it
						StoneGrindingAudio.Play();
						mLockTarget.position = lockTransform.position;
						mLockTarget.rotation = Quaternion.Euler(0f, (int)targetDirection, 0f);
						Quaternion rotateStart = lockTransform.rotation;
						Quaternion rotateEnd = mLockTarget.rotation;
						double timeStarted = WorldClock.AdjustedRealTime;
						double duration = 1.25f;
						while (WorldClock.AdjustedRealTime < (timeStarted + duration)) {
								float normalizedAmount = (float)((WorldClock.AdjustedRealTime - timeStarted) / duration);
								lockTransform.rotation = Quaternion.Slerp(rotateStart, rotateEnd, normalizedAmount);
								yield return null;
						}
						lockTransform.rotation = rotateEnd;

						//check to see if we're unlocked
						if (targetDirection == unlockDirection) {
								//TODO play a sound or something
						}
						mRotatingLock = false;
						worlditem.RefreshHud();
						StoneGrindingAudio.Stop();
						CheckIfUnlocked();
						yield break;
				}

				protected void CheckIfUnlocked()
				{

						if (State.HasBeenUnlocked)
								return;

						State.FirstLockDirection = WorldMap.GetMapDirectionFromRotation(FirstLockObject);
						State.SecondLockDirection = WorldMap.GetMapDirectionFromRotation(SecondLockObject);
						State.ThirdLockDirection = WorldMap.GetMapDirectionFromRotation(ThirdLockObject);

						if (State.FirstLockDirection == FirstLockUnlockPosition
						 && State.SecondLockDirection == SecondLockUnlockPosition
						 && State.ThirdLockDirection == ThirdLockUnlockPosition) {
								MasterAudio.PlaySound(MasterAudio.SoundType.Machines, worlditem.tr, UnlockSound);
								State.HasBeenUnlocked = true;
								StartCoroutine(UnlockOverTime());
						}
				}

				protected bool mRotatingLock = false;
				protected static Transform mLockTarget;
		}

		[Serializable]
		public class CompassPuzzleState
		{
				public bool HasBeenAltered = false;
				public bool HasBeenUnlocked = false;
				public MapDirection FirstLockDirection = MapDirection.A_North;
				public MapDirection SecondLockDirection = MapDirection.A_North;
				public MapDirection ThirdLockDirection = MapDirection.A_North;
		}
}

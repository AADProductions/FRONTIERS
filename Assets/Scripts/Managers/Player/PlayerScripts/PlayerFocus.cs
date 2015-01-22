using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using Frontiers.World.BaseWIScripts;

namespace Frontiers
{
		public class PlayerFocus : PlayerScript
		{
				public IItemOfInterest LastFocusedObject = null;
				public List <IItemOfInterest> AttentionObjects = new List <IItemOfInterest>();
				public Dictionary <IItemOfInterest, double> LosingAttentionObjects = new Dictionary <IItemOfInterest, double>();
				public float LoseAttentionRT = 5.0f;
				public GameObject LastFocusedGameObject = null;

				public override void OnGameStart()
				{
						mTargetHolder = gameObject.GetOrAdd <RVOTargetHolder>();
						enabled = true;
				}

				public bool IsFocusingOnSomething {
						get {
								return LastFocusedObject != null && !LastFocusedObject.Destroyed;
						}
				}

				public void CheckForFocusItems()
				{
						if (player.Surroundings.IsSomethingInPlayerFocus) {	
								if (player.Surroundings.IsSomethingInRange) {		
										if (LastFocusedObject != null) {
												if (LastFocusedObject != player.Surroundings.ClosestObjectFocus) {
														LastFocusedObject.HasPlayerFocus = false;
														LastFocusedObject = player.Surroundings.ClosestObjectFocus;
														LastFocusedObject.HasPlayerFocus = true;
												}
										} else {
												LastFocusedObject = player.Surroundings.ClosestObjectFocus;
												LastFocusedObject.HasPlayerFocus = true;
										}
								} else {
										if (LastFocusedObject != null) {
												LastFocusedObject.HasPlayerFocus = false;
										}
										LastFocusedObject = null;
								}
						} else {
								if (LastFocusedObject != null) {
										LastFocusedObject.HasPlayerFocus = false;
										LastFocusedObject = null;
								}
						}

						if (IsFocusingOnSomething) {
								LastFocusedGameObject = LastFocusedObject.gameObject;
						}
				}

				public void FlushFocusItems()
				{

				}

				public void GetOrReleaseAttention(IItemOfInterest newAttentionObject)
				{
						if (newAttentionObject == null) {
								return;
						}

						if (AttentionObjects.Contains(newAttentionObject)) {	//if we're already paying attention to this object
								LosingAttentionObjects.Add(newAttentionObject, WorldClock.AdjustedRealTime);
						} else {
								//otherwise tell it we're paying attention again
								//if we're doubling up on attention that's ok, scripts know to deal with it
								newAttentionObject.gameObject.SendMessage("OnGainPlayerAttention", SendMessageOptions.DontRequireReceiver);
								AttentionObjects.Add(newAttentionObject);
						}
						if (LosingAttentionObjects.Remove(newAttentionObject)) {	//remove this in any case
								LosingAttentionObjects.Remove(newAttentionObject);
						}
				}

				public void LateUpdate()
				{
						if (!mInitialized)
								return;

						if (!GameManager.Is(FGameState.InGame) || Cutscene.IsActive)
								return;

						if (!player.HasSpawned || player.IsDead)
								return;

						CheckForFocusItems();
						if (AttentionObjects.Count > 0 && !mCheckingAttentionItems) {
								mCheckingAttentionItems = true;
								StartCoroutine(CheckAttentionItems());
						}
				}

				public float FocusOffset(Vector3 worldPosition)
				{
						//focus offset gives a general measure of how close to focusing on an object we are
						//-1 means the object is behind us
						//0 means it's within visual range
						//1 means it's dead center
						Vector3 screenPoint = GameManager.Get.GameCamera.WorldToScreenPoint(worldPosition);
						float distanceToScreenPoint = Vector3.Distance(Vector3.zero, screenPoint);
						return distanceToScreenPoint;
				}

				protected IEnumerator CheckAttentionItems()
				{
						while (AttentionObjects.Count > 0) {
								//go through each attention object and make sure it's not null or too far away
								for (int i = AttentionObjects.Count - 1; i >= 0; i--) {	
										IItemOfInterest attentionObject = AttentionObjects[i];
										if (attentionObject == null) {	//if it's null then we're not paying attention to it any more
												//remove it from checkup too just in case
												AttentionObjects.RemoveAt(i);
												LosingAttentionObjects.Remove(attentionObject);
										} else if (!LosingAttentionObjects.ContainsKey(attentionObject)) {	//if we're not ALREADY checking up on this object
												//this distance check works on motile too because we're attending to the object, not the follower
												float distance = Vector3.Distance(Player.Local.Position, attentionObject.Position);
												if (distance > Globals.PlayerPickUpRange) {	//if an attention object is out of range
														Motile motile = null;
														if (attentionObject.IOIType == ItemOfInterestType.WorldItem && attentionObject.worlditem.Is <Motile>(out motile)) {	//if it's motile
																Transform follower = motile.GoalObject.transform;
																if (!mTargetHolder.HasFollower(follower)) {	//and it's not following us
																		//then it's losing our attention
																		LosingAttentionObjects.Add(attentionObject, WorldClock.AdjustedRealTime);
																}
														} else {	//if it's not motile then just say it's in danger
																LosingAttentionObjects.Add(attentionObject, WorldClock.AdjustedRealTime);
														}
												}
										}
								}
								//wait a bit
								yield return null;
								//now go through the ones that are losing attention
								List <IItemOfInterest> keepAttentionObjects = new List <IItemOfInterest>();
								List <IItemOfInterest> loseAttentionObjects = new List <IItemOfInterest>();
								foreach (KeyValuePair <IItemOfInterest, double> losingAttentionObject in LosingAttentionObjects) {
										bool inRange = false;
										bool hasTime = false;
										bool isFollowing	= false;
										//if it's not null start by checking range
										float distance	= Vector3.Distance(Player.Local.Position, losingAttentionObject.Key.Position);
										inRange = (distance < Globals.PlayerPickUpRange);
										if (inRange) {	//wuhoo, we keep attention and no longer need to be checked
												keepAttentionObjects.Add(losingAttentionObject.Key);
										} else {	//if we're not in range
												//check if the motile is following us
												Motile motile = null;
												if (losingAttentionObject.Key.IOIType == ItemOfInterestType.WorldItem && losingAttentionObject.Key.worlditem.Is <Motile>(out motile)) {	//if it's motile
														Transform follower = motile.GoalObject.transform;
														isFollowing = mTargetHolder.HasFollower(follower);
												}
												if (isFollowing) {	//wuhoo, we keep attention and no longer need to be checked
														keepAttentionObjects.Add(losingAttentionObject.Key);
												} else {
														hasTime = (losingAttentionObject.Value + LoseAttentionRT > WorldClock.AdjustedRealTime);
														if (!hasTime) {	//if we don't have any time left, we lose focus
																//time is a last resort so if we DO have time we don't keep focus
																loseAttentionObjects.Add(losingAttentionObject.Key);
														}
												}
										}
								}

								//wait a tick
								yield return null;
								//alright we have our lists of keeps and loses so update our lists
								foreach (IItemOfInterest keep in keepAttentionObjects) {	//if we're keeping attention stop checking up on it
										LosingAttentionObjects.Remove(keep);
								}
								foreach (IItemOfInterest lose in loseAttentionObjects) {	//if we've lost attention tell it so and remove it
										////Debug.Log ("Finally lost attention of " + lose.name);
										AttentionObjects.Remove(lose);
										lose.gameObject.SendMessage("OnLosePlayerAttention", SendMessageOptions.DontRequireReceiver);
										//stop checking up on it
										LosingAttentionObjects.Remove(lose);
								}
						}
						mCheckingAttentionItems = false;
						yield break;
				}

				protected bool mCheckingAttentionItems	= false;
				protected RVOTargetHolder mTargetHolder = null;
		}
}
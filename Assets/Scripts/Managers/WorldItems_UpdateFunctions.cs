using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;
using Frontiers.World;
using Frontiers.World.Gameplay;
using ExtensionMethods;
using Frontiers.Data;
using System.Text;
using System.Reflection;

namespace Frontiers.World
{
		public partial class WorldItems : Manager
		{
				public List <WorldItem> RecentlyCreatedWorldItems = new List <WorldItem>();
				public List <WorldItem> WorldItemsCreatedWhileFlushing	= new List <WorldItem>();
				public List <WorldItem> InvisibleWorldItems = new List <WorldItem>();
				public List <WorldItem> VisibleWorldItems = new List <WorldItem>();
				public List <WorldItem> ActiveWorldItems = new List <WorldItem>();
				public List <WorldItem> LockedWorldItems = new List <WorldItem>();
				public static Vector3 LastPlayerSortPosition;
				public static Vector3 LastPlayerPosition;
				protected WorldItem awi = null;
				protected WorldItem vwi = null;
				protected WorldItem ivwi = null;
				protected WorldItem lwi = null;
				protected int mNextUpdate = 0;
				protected int mInvisibleCounter = 0;
				protected int mVisibleCounter = 0;
				protected int mActiveCounter = 0;
				protected bool mIsFlushingWorldItems = false;
				protected int mVisibleCounterMax;
				protected int mActiveCounterMax;
				protected int mInvisibleCounterMax;
				public bool SuspendWorldItemUpdates = false;

				public void SetAllWorldItemsToInvisible()
				{
						for (int i = 0; i < ActiveWorldItems.Count; i++) {
								if (ActiveWorldItems[i] != null) {
										ActiveWorldItems[i].ActiveState = WIActiveState.Invisible;
								}
						}
						for (int i = 0; i < VisibleWorldItems.Count; i++) {
								if (VisibleWorldItems[i] != null) {
										VisibleWorldItems[i].ActiveState = WIActiveState.Invisible;
								}
						}
						CleanWorldItemLists();
				}

				public void Update()
				{
						if (SuspendWorldItemUpdates)
								return;

						FlushWorldItems();

						#if UNITY_EDITOR
						if (!Application.isPlaying)
								return;
						#endif

						if (!Profile.Get.HasCurrentGame) {
								return;
						}

						LastPlayerPosition = Player.Local.Position;

						//always dequeue a worlditem to save
						if (WorldItemsToSave.Count > 0) {
								KeyValuePair <string,WorldItem> wiToSave = WorldItemsToSave.Dequeue();
								if (wiToSave.Value != null) {
										StackItem stackItem = wiToSave.Value.GetStackItem(WIMode.None);
										Mods.Get.Runtime.SaveStackItemToGroup(stackItem, wiToSave.Key);
										//now clear the stack item and destroy the worlditem
										stackItem.Clear();
										GameObject.Destroy(wiToSave.Value.gameObject);
										//just do one per update, give it some time to breathe
								} else {
										Debug.Log("Worlditem to save was null by the time we got to it");
								}
						}

						if (Player.Local.Status.IsStateActive("Traveling")) {
								mActiveCounterMax = 10;
								mVisibleCounterMax = 15;
								mInvisibleCounterMax = 50;
								if (Vector3.Distance(LastPlayerSortPosition, LastPlayerPosition) > Globals.PlayerMinimumActiveStateSortDistanceTraveling) {
										LastPlayerSortPosition = LastPlayerPosition;
										CleanWorldItemLists();
								}
						} else {
								mActiveCounterMax = 5;
								mVisibleCounterMax = 10;
								mInvisibleCounterMax = 15;
								if (Vector3.Distance(LastPlayerSortPosition, LastPlayerPosition) > Globals.PlayerMinimumActiveStateSortDistance) {
										LastPlayerSortPosition = LastPlayerPosition;
										CleanWorldItemLists();
								}
						}

						mActiveCounter++;
						if (mActiveCounter > mActiveCounterMax) {
								mActiveCounter = 0;
								ActiveRadiusComparer.PlayerPosition = LastPlayerPosition;
								ActiveWorldItems.Sort(ActiveRadiusComparer);
								for (int i = 0; i < ActiveWorldItems.Count; i++) {
										awi = ActiveWorldItems[i];//[NextActiveIndex];		
										if (awi != null && (Mathf.Clamp((awi.LastActiveDistanceToPlayer - Player.Local.ColliderRadius), 0f, float.MaxValue)) > awi.ActiveRadius) {
												//go through visible first
												awi.ActiveState = WIActiveState.Visible;
												//SendToStateList (awi);
										}
								}
						}

						mVisibleCounter++;
						if (mVisibleCounter > mVisibleCounterMax) {
								mVisibleCounter = 0;
								VisibleRadiusComparer.PlayerPosition = LastPlayerPosition;
								VisibleWorldItems.Sort(VisibleRadiusComparer);
								for (int i = 0; i < VisibleWorldItems.Count; i++) {
										vwi = VisibleWorldItems[i];//[NextVisibleIndex];
										if (vwi != null) {
												if (Mathf.Clamp((vwi.LastActiveDistanceToPlayer - Player.Local.ColliderRadius), 0f, float.MaxValue) > vwi.VisibleDistance) {
														vwi.ActiveState = WIActiveState.Invisible;
														SendToStateList(vwi);
												} else if (Mathf.Clamp((vwi.LastActiveDistanceToPlayer - Player.Local.ColliderRadius), 0f, float.MaxValue) < vwi.ActiveRadius) {
														vwi.ActiveState = WIActiveState.Active;
														//SendToStateList (vwi);
												}
										}
								}
						}

						mInvisibleCounter++;
						if (mInvisibleCounter > mInvisibleCounterMax) {
								mInvisibleCounter = 0;
								InvisibleRadiusComparer.PlayerPosition = LastPlayerPosition;
								InvisibleWorldItems.Sort(InvisibleRadiusComparer);
								for (int i = 0; i < InvisibleWorldItems.Count; i++) {
										ivwi = InvisibleWorldItems[i];
										if (ivwi != null && Mathf.Clamp((ivwi.LastActiveDistanceToPlayer - Player.Local.ColliderRadius), 0f, float.MaxValue) < ivwi.VisibleDistance) {
												//go through visible first even if it's in active range
												ivwi.ActiveState = WIActiveState.Visible;
												//SendToStateList (ivwi);
										}
								}

								for (int i = 0; i < LockedWorldItems.Count; i++) {
										lwi = LockedWorldItems[i];
										if (lwi != null && !lwi.ActiveStateLocked) {
												SendToStateList(lwi);
										}
								}
						}
				}

				protected void CleanWorldItemLists()
				{
						for (int i = LockedWorldItems.LastIndex(); i >= 0; i--) {
								lwi = LockedWorldItems[i];
								if (lwi == null || lwi.Destroyed || lwi.Is(WILoadState.Unloaded | WILoadState.Unloading)) {
										LockedWorldItems.RemoveAt(i);
								} else {
										//set this so it's calculated the next time
										lwi.LastActiveDistanceToPlayer = Vector3.Distance(lwi.Position, LastPlayerPosition);
								}
						}

						for (int i = ActiveWorldItems.LastIndex(); i >= 0; i--) {
								awi = ActiveWorldItems[i];
								if (awi == null || awi.Destroyed || awi.Is(WILoadState.Unloaded | WILoadState.Unloading)) {
										ActiveWorldItems.RemoveAt(i);
								} else {
										//set this so it's calculated the next time
										awi.LastActiveDistanceToPlayer = Vector3.Distance(awi.Position, LastPlayerPosition);
								}
						}

						for (int i = VisibleWorldItems.LastIndex(); i >= 0; i--) {
								vwi = VisibleWorldItems[i];
								if (vwi == null || vwi.Destroyed || vwi.Is(WILoadState.Unloaded | WILoadState.Unloading)) {
										VisibleWorldItems.RemoveAt(i);
								} else {
										//set this so it's calculated the next time
										vwi.LastActiveDistanceToPlayer = Vector3.Distance(vwi.Position, LastPlayerPosition);
								}
						}

						for (int i = InvisibleWorldItems.LastIndex(); i >= 0; i--) {
								ivwi = InvisibleWorldItems[i];
								if (ivwi == null || ivwi.Destroyed || ivwi.Is(WILoadState.Unloaded | WILoadState.Unloading)) {
										InvisibleWorldItems.RemoveAt(i);
								} else {
										//set this so it's calculated the next time
										ivwi.LastActiveDistanceToPlayer = Vector3.Distance(ivwi.Position, LastPlayerPosition);
								}
						}
				}

				public void SendToStateList(WorldItem worlditem)
				{
						if (worlditem.ActiveStateLocked) {
								LockedWorldItems.SafeAdd(worlditem);
								ActiveWorldItems.Remove(worlditem);
								VisibleWorldItems.Remove(worlditem);
								InvisibleWorldItems.Remove(worlditem);
						} else {
								switch (worlditem.ActiveState) {
										case WIActiveState.Invisible:
										default:
												ActiveWorldItems.Remove(worlditem);
												VisibleWorldItems.Remove(worlditem);
												InvisibleWorldItems.SafeAdd(worlditem);
												break;

										case WIActiveState.Visible:
												ActiveWorldItems.Remove(worlditem);
												VisibleWorldItems.SafeAdd(worlditem);
												InvisibleWorldItems.Remove(worlditem);
												break;

										case WIActiveState.Active:
												ActiveWorldItems.SafeAdd(worlditem);
												VisibleWorldItems.Remove(worlditem);
												InvisibleWorldItems.Remove(worlditem);
												break;
								}
						}
				}

				protected WIActiveRadiusComparer ActiveRadiusComparer = new WIActiveRadiusComparer();
				protected WIVisibleRadiusComparer VisibleRadiusComparer = new WIVisibleRadiusComparer();
				protected WIInvisibleRadiusComparer InvisibleRadiusComparer = new WIInvisibleRadiusComparer();

				public class WIActiveRadiusComparer : IComparer <WorldItem>
				{
						public Vector3 PlayerPosition;

						public int Compare(WorldItem a, WorldItem b)
						{
								//this is the first time it's been calculated this cycle
								//aldo check whether we need to refresh our active state
								return a.LastActiveDistanceToPlayer.CompareTo(b.LastActiveDistanceToPlayer);
						}
				}

				public class WIVisibleRadiusComparer : IComparer <WorldItem>
				{
						public Vector3 PlayerPosition;

						public int Compare(WorldItem a, WorldItem b)
						{
								//this is the first time it's been calculated this cycle
								//aldo check whether we need to refresh our active state
								return a.LastActiveDistanceToPlayer.CompareTo(b.LastActiveDistanceToPlayer);
						}
				}

				public class WIInvisibleRadiusComparer : IComparer <WorldItem>
				{
						public Vector3 PlayerPosition;

						public int Compare(WorldItem a, WorldItem b)
						{
								//this is the first time it's been calculated this cycle
								//aldo check whether we need to refresh our active state
								return a.LastActiveDistanceToPlayer.CompareTo(b.LastActiveDistanceToPlayer);
						}
				}

				protected IEnumerator UpdateStackItemsToLoad()
				{
						while (!GameManager.Is(FGameState.Quitting)) {
								for (int i = StackItemsToLoad.LastIndex(); i >= 0; i--) {
										while (!GameManager.Is(FGameState.InGame | FGameState.Cutscene | FGameState.GameLoading | FGameState.GameStarting)) {
												yield return null;
										}
										KeyValuePair <WIGroup, Queue <StackItem>> groupPair = StackItemsToLoad[i];
										if (groupPair.Key == null || groupPair.Value.Count == 0) {
												//if the group is gone or it's no longer loading, remove it, it's done
												StackItemsToLoad.RemoveAt(i);
										} else {
												WorldItem worlditem = null;
//						if (GameManager.Is (FGameState.InGame)) {
												////Debug.Log ("Loading stack items for " + StackItemsToLoad [i].Key.name);
												//otherwise pop the next stackitem  off the queue
												//and clone it
												CloneFromStackItem(groupPair.Value.Dequeue(), groupPair.Key, out worlditem);
//						} else {
//							while (groupPair.Value.Count > 0) {
//								//do it super quick
//								CloneFromStackItem (groupPair.Value.Dequeue (), groupPair.Key, out worlditem);
//								worlditem.Initialize ();
//							}
//						}
										}
										yield return null;
								}
								yield return null;
						}
						yield break;
				}

				public void FlushWorldItems()
				{
						mIsFlushingWorldItems = true;
						for (int i = RecentlyCreatedWorldItems.LastIndex(); i >= 0; i--) {
								WorldItem recentlyCreatedWorldItem = RecentlyCreatedWorldItems[i];
								if (recentlyCreatedWorldItem != null) {
										if (!recentlyCreatedWorldItem.gameObject.activeSelf) {
												recentlyCreatedWorldItem.gameObject.SetActive(true);
										}
										recentlyCreatedWorldItem.Initialize();
										//after calling intialize it will have been added to its group
										//and its position will have been updated
										//so we can safely predict its active state here
								}
						}
						RecentlyCreatedWorldItems.Clear();
						mIsFlushingWorldItems = false;

						RecentlyCreatedWorldItems.AddRange(WorldItemsCreatedWhileFlushing);
						WorldItemsCreatedWhileFlushing.Clear();
				}

				public static void OnWorldItemInitialized(WorldItem worldItem)
				{
						worldItem.LastActiveDistanceToPlayer = Vector3.Distance(worldItem.Position, LastPlayerPosition);
						if (worldItem.ActiveStateLocked) {
								Get.LockedWorldItems.SafeAdd(worldItem);
						} else {
								if (worldItem.LastActiveDistanceToPlayer > worldItem.VisibleDistance) {
										worldItem.ActiveState = WIActiveState.Invisible;
										Get.InvisibleWorldItems.SafeAdd(worldItem);
								} else if (worldItem.LastActiveDistanceToPlayer > worldItem.ActiveRadius) {
										worldItem.ActiveState = WIActiveState.Visible;
										Get.VisibleWorldItems.SafeAdd(worldItem);
								} else {
										worldItem.ActiveState = WIActiveState.Active;
										Get.ActiveWorldItems.SafeAdd(worldItem);
								}
						}
				}

				public static void InitializeWorldItem(WorldItem newWorldItem)
				{
						if (Get.mIsFlushingWorldItems) {
								Get.WorldItemsCreatedWhileFlushing.Add(newWorldItem);
						} else {
								Get.RecentlyCreatedWorldItems.Add(newWorldItem);
						}
				}
		}
}
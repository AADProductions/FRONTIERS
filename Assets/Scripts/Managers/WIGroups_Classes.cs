using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using System;

namespace Frontiers
{
		public class WIGroupUnloader : IComparable <WIGroupUnloader>
		{
				public WIGroupUnloader(WIGroup rootGroup)
				{
						RootGroup = rootGroup;
						TimeStarted = WorldClock.RealTime;
						NotPreparedToUnload = new List <WIGroup>();
						PreparingToUnload = new List <WIGroup>();
						ReadyToUnload = new List <WIGroup>();
						Unloading = new List <WIGroup>();
						FinishedUnloading = new List <WIGroup>();
						ReadyToDestroy = new List <WIGroup>();
				}

				public double TimeStarted;
				public double Timeout = 10f;
				public double AddedChildGroup = 0f;
				public int MaxDepth = 0;

				public void AddChildGroup(WIGroup group)
				{
						MaxDepth = Mathf.Max(MaxDepth, group.Depth);
						AddedChildGroup = WorldClock.RealTime;
						switch (group.LoadState) {
						//we may have to back up a few steps for this to work
								case WIGroupLoadState.Uninitialized:
								case WIGroupLoadState.Initializing:
								case WIGroupLoadState.Initialized:
								case WIGroupLoadState.PreparingToLoad:
								case WIGroupLoadState.Loading:
								case WIGroupLoadState.Loaded:
								case WIGroupLoadState.PreparingToUnload:
								default:
										NotPreparedToUnload.SafeAdd(group);
										break;

								case WIGroupLoadState.Unloading:
										Unloading.SafeAdd(group);
										break;

								case WIGroupLoadState.Unloaded:
										ReadyToDestroy.SafeAdd(group);
										break;
						}
				}

				protected void AddGroupsToUnload(List <WIGroup> childGroups, List <WIGroup> unloadList)
				{
						for (int i = 0; i < childGroups.Count; i++) {
								MaxDepth = Mathf.Max(MaxDepth, childGroups [i].Depth);
								unloadList.Add(childGroups[i]);
								AddGroupsToUnload(childGroups[i].ChildGroups, unloadList);
						}
				}

				public WIGroupLoadState LoadState = WIGroupLoadState.Loaded;

				public void Initialize()
				{
						if (mInitialized) {
								return;
						}
						//go down the tree and put groups in their appropriate lists
						AddGroupsToUnload(RootGroup.ChildGroups, NotPreparedToUnload);
						mInitialized = true;
				}

				public void Clear()
				{
						if (!mInitialized)
								return;

						LoadState = WIGroupLoadState.Loaded;
						RootGroup = null;

						NotPreparedToUnload.Clear();
						PreparingToUnload.Clear();
						ReadyToUnload.Clear();
						Unloading.Clear();
						FinishedUnloading.Clear();
						ReadyToDestroy.Clear();

						NotPreparedToUnload = null;
						PreparingToUnload = null;
						ReadyToUnload = null;
						Unloading = null;
						FinishedUnloading = null;
						ReadyToDestroy = null;
				}

				public void GetDeepestUnloadedChildGroups(List <WIGroup> childGroups)
				{
						if (!mInitialized)
								return;

						if (ReadyToDestroy.Count > 0) {
								ReadyToDestroy.Sort();
								//after sorting the greatest depth will be at the end
								//so get the max depth and then add the first child
								int lastIndex = ReadyToDestroy.LastIndex();
								int maxDepth = ReadyToDestroy[lastIndex].Depth;
								childGroups.Add(ReadyToDestroy[lastIndex]);
								ReadyToDestroy.RemoveAt(lastIndex);
								//then return all children that are as deep as the first
								for (int i = ReadyToDestroy.LastIndex(); i >= 0; i--) {
										if (ReadyToDestroy[i].Depth >= maxDepth) {
												childGroups.Add(ReadyToDestroy[i]);
												ReadyToDestroy.RemoveAt(i);
										} else {
												//if the depth isn't equal / greater
												//then we've gotten to shallower children
												//so we're done here
												break;
										}
								}
						}
				}
				//called by Groups manager
				public IEnumerator CheckGroupLoadStates()
				{
						if (!mInitialized)
								yield break;

						if (RootGroup == null || RootGroup.IsDestroyed) {
								Debug.Log("ROOT GROUP was null or destroyed in unloader, we're finished");
								yield break;
						}
						//we may have to backtrack if new groups were added
						if (NotPreparedToUnload.Count > 0) {
								LoadState = WIGroupLoadState.Loaded;
						} else if (PreparingToUnload.Count > 0) {
								LoadState = WIGroupLoadState.PreparingToUnload;
						} else if (Unloading.Count > 0) {
								LoadState = WIGroupLoadState.Unloading;
						}

						switch (LoadState) {
								case WIGroupLoadState.Loaded:
										//tell everyone in the Loaded list to prepare to unload
										//then move them all into preparing to unload
										//---TRANSITION TO PREPARED TO UNLOAD---//
										for (int i = NotPreparedToUnload.LastIndex(); i >= 0; i--) {
												if (NotPreparedToUnload[i].PrepareToUnload()) {
														PreparingToUnload.Add(NotPreparedToUnload[i]);
														NotPreparedToUnload.RemoveAt(i);
												}
												yield return null;
										}
										if (NotPreparedToUnload.Count == 0 && RootGroup.PrepareToUnload()) {
												LoadState = WIGroupLoadState.PreparingToUnload;
												//RootGroup.LoadState = WIGroupLoadState.PreparingToUnload;
										}
										yield return null;
										break;

								case WIGroupLoadState.PreparingToUnload:
										//go through each child group and ask if it's ready to unload
										//this will force the group to ask each of its child items
										//if it's prepared we move it to the prepared to unload group
										for (int i = PreparingToUnload.LastIndex(); i >= 0; i--) {
												WIGroup loadedGroup = PreparingToUnload[i];
												if (loadedGroup.ReadyToUnload) {
														PreparingToUnload.RemoveAt(i);
														ReadyToUnload.Add(loadedGroup);
												}
												yield return null;
												//yield return null;
										}
										//---TRANSITION TO UNLOADING---//
										//if all are ready to unload and there are no more not prepared to unload
										//then begin unload - there's no turning back at this point!
										if (PreparingToUnload.Count == 0 && RootGroup.ReadyToUnload) {
												Unloading.AddRange(ReadyToUnload);
												ReadyToUnload.Clear();
												for (int i = 0; i < Unloading.Count; i++) {
														Unloading[i].BeginUnload();
												}
												RootGroup.BeginUnload();
												LoadState = WIGroupLoadState.Unloading;
												//RootGroup.LoadState = WIGroupLoadState.Unloading;
										}
										yield return null;
										break;

								case WIGroupLoadState.Unloading:
										if (Unloading.Count > 0) {
												for (int i = Unloading.LastIndex(); i >= 0; i--) {
														if (Unloading[i].FinishedUnloading) {
																FinishedUnloading.Add(Unloading[i]);
																Unloading.RemoveAt(i);
																//this generates a huge amount of garbage
																yield break;
														}
														yield return null;
												}
										} else {
												for (int i = FinishedUnloading.LastIndex(); i >= 0; i--) {
														if (FinishedUnloading[i] == null || FinishedUnloading[i].IsDestroyed) {
																FinishedUnloading.RemoveAt(i);
														} else {
																//we see if it's ready to actually be destroyed
																//no groups greater than [x] depth that have not been destroyed
																if (!FinishedUnloading[i].HasChildGroups) {
																		ReadyToDestroy.Add(FinishedUnloading[i]);
																		FinishedUnloading.RemoveAt(i);
																}
														}
														yield return null;
												}
										}

										yield return null;
										//---TRANSITION TO UNLOADED---//
										//if we have more than just the root group then we wait until the root group is finished unloading to kill everything
										if (FinishedUnloading.Count == 0 && ReadyToDestroy.Count == 0 && RootGroup.FinishedUnloading) {
												//now it will be detsroyed by WIGroups
												LoadState = WIGroupLoadState.Unloaded;
										}
										break;

								case WIGroupLoadState.Unloaded:
										//nothing left to do
										break;

								default:
										Debug.Log("Weird load state in WIGroupUnloader: " + LoadState.ToString());
										break;
						}

						yield break;
				}

				protected static WaitForSeconds gWaitForUnloading = new WaitForSeconds(0.01f);
				public WIGroup RootGroup;

				#region IComparable implementation

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						WIGroupUnloader other = obj as WIGroupUnloader;
						if (this == other) {
								return true;
						}

						return false;
				}

				public bool Equals(WIGroupUnloader other)
				{
						if (other == null) {
								return false;
						}

						return this == other;
				}

				public int CompareTo(WIGroupUnloader other)
				{
						int compareTo = RootGroup.CompareTo(other.RootGroup);
						if (compareTo == 0) {
								compareTo = MaxDepth.CompareTo(other.MaxDepth);
						}
						return compareTo;
				}

				public override int GetHashCode()
				{
						return RootGroup.GetHashCode();
				}

				#endregion

				public List <WIGroup> NotPreparedToUnload;
				public List <WIGroup> PreparingToUnload;
				public List <WIGroup> ReadyToUnload;
				public List <WIGroup> Unloading;
				public List <WIGroup> FinishedUnloading;
				public List <WIGroup> ReadyToDestroy;
				protected bool mInitialized = false;
		}
		//this is used to keep track of requests to load a group
		//groups can be asked to load even when they're about to be destroyed
		//so keeping track of load requests with these things instead of the
		//groups themselves ensures that no requests are ever lost
		public class WIGroupLoadRequest : IComparable <WIGroupLoadRequest>, IEquatable <WIGroupLoadRequest>
		{
				//you can only make a load request with a live group
				//even if that request ends up being fulfilled for a destroyed group
				public WIGroupLoadRequest(WIGroup group)
				{
						Group = group;
						GroupPath = Group.Path;
						UniqueID = Group.Props.UniqueID;
						TimeAdded = WorldClock.AdjustedRealTime;
						OnHold = false;
				}

				public WIGroup Group;
				public string GroupPath;
				public string UniqueID;
				public double TimeAdded;
				public bool OnHold = false;

				public double Timeout {
						get {
								return TimeAdded + Globals.GroupLoadRequestTimeout;
						}
				}

				public void Clear()
				{
						Group = null;
						GroupPath = null;
						UniqueID = null;
				}

				public bool HasGroupReference {
						get {
								return Group != null && !Group.IsDestroyed;
						}
				}

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						WIGroupLoadRequest other = obj as WIGroupLoadRequest;
						if (UniqueID.Equals(other.UniqueID)) {
								return true;
						}

						return false;
				}

				public bool Equals(WIGroupLoadRequest other)
				{
						if (other == null) {
								return false;
						}

						return this == other || UniqueID.Equals(other.UniqueID);
				}

				public int CompareTo(WIGroupLoadRequest other)
				{
						if (other == null) {
								return 0;
						}

						int compareTo = other.GroupPath.Length.CompareTo(GroupPath.Length);
						if (compareTo == 0 || (other.HasGroupReference && HasGroupReference)) {
								compareTo = other.Group.Depth.CompareTo (Group.Depth);
						}
						return compareTo;
				}

				public override int GetHashCode()
				{
						return UniqueID.GetHashCode();
				}
		}

		public class WIGroupsState
		{
				public WIGroupsState()
				{
						LocationExcludeTypes = new List <string>();
						LocationLookup = new SDictionary <string, string>();
						QuestItemLookup = new SDictionary <string, string>();
						CharacterLookup = new SDictionary <string, string>();
						LocationExcludeTypes.Add("PathMarker");
				}

				public string WorldName = "FRONTIERS";
				public List <string> LocationExcludeTypes;
				public SDictionary <string, string>	LocationLookup;
				public SDictionary <string, string>	QuestItemLookup;
				public SDictionary <string, string>	CharacterLookup;
		}
}
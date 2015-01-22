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

				protected void AddGroupsToUnload(List <WIGroup> childGroups, List <WIGroup> unloadList)
				{
						for (int i = 0; i < childGroups.Count; i++) {
								unloadList.Add(childGroups[i]);
								AddGroupsToUnload(childGroups[i].ChildGroups, unloadList);
						}
				}

				public WIGroupLoadState LoadState = WIGroupLoadState.None;
				public bool IsCanceled = false;

				public void Initialize()
				{
						if (mInitialized) {
								return;
						}
						//go down the tree and put groups in their appropriate lists
						AddGroupsToUnload(RootGroup.ChildGroups, NotPreparedToUnload);
						mInitialized = true;
				}

				public void TryToCancel()
				{
						//the only way we can cancel is if the group doesn't have a parent group unloading
						if (RootGroup.Is(WIGroupLoadState.PreparingToUnload) && !RootGroup.HasUnloadingParent) {
								for (int i = 0; i < PreparingToUnload.Count; i++) {
										PreparingToUnload[i].CancelUnload();
								}
								RootGroup.LoadState = WIGroupLoadState.Loaded;
								IsCanceled = true;
						}
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
								IsCanceled = true;
								yield break;
						}

						switch (LoadState) {
								case WIGroupLoadState.Loaded:
								default:
										//tell everyone in the Loaded list to prepare to unload
										//then move them all into preparing to unload
										//---TRANSITION TO PREPARED TO UNLOAD---//
										for (int i = NotPreparedToUnload.LastIndex(); i >= 0; i--) {
												if (NotPreparedToUnload[i].PrepareToUnload()) {
														PreparingToUnload.Add(NotPreparedToUnload[i]);
														NotPreparedToUnload.RemoveAt(i);
												}
										}
										if (NotPreparedToUnload.Count == 0 && RootGroup.PrepareToUnload()) {
												LoadState = WIGroupLoadState.PreparingToUnload;
												//RootGroup.LoadState = WIGroupLoadState.PreparingToUnload;
										}
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
										}

										bool allReadyToUnload = true;
										for (int i = ReadyToUnload.LastIndex(); i >= 0; i--) {
												WIGroup preparedGroup = ReadyToUnload[i];
												if (!preparedGroup.ReadyToUnload) {
														//verify that it's actually still ready to unload
														//if it's not then move it back into the not prepared
														ReadyToUnload.RemoveAt(i);
														NotPreparedToUnload.Add(preparedGroup);
														allReadyToUnload = false;
														break;
												}
										}
										//---TRANSITION TO UNLOADING---//
										//if all are ready to unload and there are no more not prepared to unload
										//then begin unload - there's no turning back at this point!
										if (allReadyToUnload && RootGroup.ReadyToUnload) {
												Unloading.AddRange(ReadyToUnload);
												ReadyToUnload.Clear();
												for (int i = 0; i < Unloading.Count; i++) {
														Unloading[i].BeginUnload();
												}
												RootGroup.BeginUnload();
												LoadState = WIGroupLoadState.Unloading;
												//RootGroup.LoadState = WIGroupLoadState.Unloading;
										}
										break;

								case WIGroupLoadState.Unloading:
										for (int i = Unloading.LastIndex(); i >= 0; i--) {
												if (Unloading[i].FinishedUnloading) {
														FinishedUnloading.Add(Unloading[i]);
														Unloading.RemoveAt(i);
														//this generates a huge amount of garbage
														yield return gWaitForUnloading;
												}
										}

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
										}
										//---TRANSITION TO UNLOADED---//
										if (FinishedUnloading.Count == 0 && ReadyToDestroy.Count == 0 && RootGroup.FinishedUnloading) {
												LoadState = WIGroupLoadState.Unloaded;
										}
										break;

								case WIGroupLoadState.Unloaded:
										//nothing left to do
										break;
						}

						yield break;
				}

				protected static WaitForSeconds gWaitForUnloading = new WaitForSeconds (0.01f);

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
						return RootGroup.CompareTo(other.RootGroup);
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
}
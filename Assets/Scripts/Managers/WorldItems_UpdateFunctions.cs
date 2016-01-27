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
using Wintellect.PowerCollections;

namespace Frontiers.World
{
	public partial class WorldItems : Manager
	{
		public List <WorldItem> RecentlyCreatedWorldItems = new List <WorldItem> ();
		public List <WorldItem> WorldItemsCreatedWhileFlushing	= new List <WorldItem> ();
		public HashSet <WorldItem> ItemsToSort = new HashSet<WorldItem> ();
		public HashSet <WorldItem> ActiveWorldItems;
		public HashSet <WorldItem> VisibleWorldItems;
		public HashSet <WorldItem> InvisibleWorldItems;
		public HashSet <WorldItem> LockedWorldItems;
		public List <WorldItem> ActiveSet;
		public List <WorldItem> VisibleSet;
		public List <WorldItem> InvisibleSet;
		public List <WorldItem> LockedSet;
		public static Vector3 LastPlayerSortPosition;
		public static Vector3 LastPlayerPosition;
		public static int LastActiveDistanceUpdate;
		protected WorldItem wi = null;
		protected int mNextUpdate = 0;
		protected int mInvisibleCounter = 0;
		protected int mVisibleCounter = 0;
		protected int mActiveCounter = 0;
		protected bool mIsFlushingWorldItems = false;
		protected int mVisibleCounterMax;
		protected int mActiveCounterMax;
		protected int mInvisibleCounterMax;
		public bool SuspendWorldItemUpdates = false;
		protected StackItem mSaveStackItem;

		public override void OnInitialized ()
		{
			base.OnInitialized ();

			ActiveRadiusComparer = new WIRadiusComparer ();
			VisibleRadiusComparer = new WIRadiusComparer ();
			InvisibleRadiusComparer = new WIRadiusComparer ();
			LockedComparer = new WIRadiusComparer ();

			ActiveRadiusComparer.Locked = false;
			VisibleRadiusComparer.Locked = false;
			InvisibleRadiusComparer.Locked = false;
			LockedComparer.Locked = true;

			ActiveRadiusComparer.State = WIActiveState.Active;
			VisibleRadiusComparer.State = WIActiveState.Visible;
			InvisibleRadiusComparer.State = WIActiveState.Invisible;
			LockedComparer.State = WIActiveState.Any;

			ActiveWorldItems = new HashSet<WorldItem> ();
			VisibleWorldItems = new HashSet<WorldItem> ();
			InvisibleWorldItems = new HashSet<WorldItem> ();
			LockedWorldItems = new HashSet<WorldItem> ();

			ActiveSet = new List<WorldItem> ();
			VisibleSet = new List<WorldItem> ();
			InvisibleSet = new List<WorldItem> ();
			LockedSet = new List<WorldItem> ();
		}

		public void SetAllWorldItemsToInvisible ()
		{
			var activeWorldItems = ActiveWorldItems.GetEnumerator ();
			while (activeWorldItems.MoveNext ()) {
				activeWorldItems.Current.ActiveState = WIActiveState.Invisible;
			}
			var visibleWorldItems = VisibleWorldItems.GetEnumerator ();
			while (visibleWorldItems.MoveNext ()) {
				visibleWorldItems.Current.ActiveState = WIActiveState.Invisible;
			}
			//force clean
			LastPlayerPosition = Player.Local.Position;
			LastActiveDistanceUpdate++;
		}

		public void Update ()
		{
			if (SuspendWorldItemUpdates)
				return;

			if (FlushWorldItems ()) {
				return;
			}

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			#endif

			if (!Profile.Get.HasCurrentGame) {
				return;
			}

			//if we've left any dangling items behind sort them out now
			if (ItemsToSort.Count > 0) {
				//copy everything to a new hashset and clear
				HashSet<WorldItem> itemsToSort = new HashSet<WorldItem> (ItemsToSort);
				ItemsToSort.Clear ();
				var sortEnum = itemsToSort.GetEnumerator ();
				while (sortEnum.MoveNext ()) {
					SendToStateList (sortEnum.Current);
				}
				itemsToSort.Clear ();
			}

			LastPlayerPosition = Player.Local.Position;

			if (Player.Local.Status.IsStateActive ("Traveling")) {
				mActiveCounterMax = 10;
				mVisibleCounterMax = 15;
				mInvisibleCounterMax = 50;
				if (Vector3.Distance (LastPlayerSortPosition, LastPlayerPosition) > Globals.PlayerMinimumActiveStateSortDistanceTraveling) {
					LastPlayerSortPosition = LastPlayerPosition;
					LastActiveDistanceUpdate++;
				}
			} else {
				mActiveCounterMax = 5;
				mVisibleCounterMax = 10;
				mInvisibleCounterMax = 15;
				if (Vector3.Distance (LastPlayerSortPosition, LastPlayerPosition) > Globals.PlayerMinimumActiveStateSortDistance) {
					LastPlayerSortPosition = LastPlayerPosition;
					LastActiveDistanceUpdate++;
				}
			}



			if (ActiveSet.Count == 0) {
				mActiveCounter++;
			}
			if (VisibleSet.Count == 0) {
				mVisibleCounter++;
			}
			if (InvisibleSet.Count == 0) {
				mInvisibleCounter++;
			}

			if (mActiveCounter > mActiveCounterMax) {
				mActiveCounter = 0;
				ActiveRadiusComparer.PlayerPosition = LastPlayerPosition;
				ActiveRadiusComparer.LastUpdate = LastActiveDistanceUpdate;
				StartCoroutine (CleanWorldItemList (ActiveSet, ActiveWorldItems, ActiveRadiusComparer, 10, 50));
			}

			if (mVisibleCounter > mVisibleCounterMax) {
				mVisibleCounter = 0;
				VisibleRadiusComparer.PlayerPosition = LastPlayerPosition;
				VisibleRadiusComparer.LastUpdate = LastActiveDistanceUpdate;
				StartCoroutine (CleanWorldItemList (VisibleSet, VisibleWorldItems, VisibleRadiusComparer, 25, 75));
			}

			if (mInvisibleCounter > mInvisibleCounterMax) {
				mInvisibleCounter = 0;
				InvisibleRadiusComparer.PlayerPosition = LastPlayerPosition;
				InvisibleRadiusComparer.LastUpdate = LastActiveDistanceUpdate;
				LockedComparer.PlayerPosition = LastPlayerPosition;
				LockedComparer.LastUpdate = LastActiveDistanceUpdate;
				StartCoroutine (CleanWorldItemList (InvisibleSet, InvisibleWorldItems, InvisibleRadiusComparer, 50, 100));
				StartCoroutine (CleanWorldItemList (LockedSet, LockedWorldItems, LockedComparer, 50, 200));
			}
		}

		protected IEnumerator CleanWorldItemList (List <WorldItem> sortedList, HashSet <WorldItem> masterList, WIRadiusComparer sorter, int removalsPerFrame, int numToSortAtOnce)
		{
			sortedList.Clear ();
			sortedList.AddRange (masterList);
			//if (sortedList.Count <= numToSortAtOnce) {
				//sort everything in one list
				var sortOneList = CheckIfDirty (sortedList, masterList, sorter, removalsPerFrame);
				while (sortOneList.MoveNext ()) {
					yield return sortOneList.Current;
				}
			/*} else {
				//we need to do it in sections
				List <WorldItem> subList = new List<WorldItem> (numToSortAtOnce);
				int currentIndex = 0;
				while (currentIndex < numToSortAtOnce) {
					//sort everything bit by bit
					for (int i = 0; i < numToSortAtOnce; i++) {
						subList.Add (sortedList [currentIndex]);
						currentIndex++;
						if (currentIndex >= sortedList.Count) {
							break;
						}
					}
					var sortMultipleLists = CheckIfDirty (subList, masterList, sorter, removalsPerFrame);
					while (sortMultipleLists.MoveNext ()) {
						yield return sortMultipleLists.Current;
					}
					yield return null;
				}
			}*/
			if (sortedList.Count > 0) {
				sortedList.Clear ();
			}
			yield break;
		}

		protected IEnumerator CheckIfDirty (List <WorldItem> listToCheck, HashSet <WorldItem> masterList, WIRadiusComparer sorter, int removalsPerFrame) {
			listToCheck.Sort (sorter);
			int frame = 0;
			int removals = 0;
			WorldItem w = null;
			for (int i = 0; i < listToCheck.Count; i++) {
				w = listToCheck [i];
				if (sorter.IsDirty (listToCheck [i])) {
					//masterList.Remove (w);
					//ItemsToSort.Add (w);
					SendToStateList (w);
					removals++;
					if (removals > removalsPerFrame) {
						removals = 0;
						yield return null;
					}
				} else {
					break;
				}
			}
			listToCheck.Clear ();
		}

		public void SendToStateList (WorldItem w)
		{
			if (w == null || w.Is (WILoadState.Unloading | WILoadState.Unloaded))
				return;

			if (w.ActiveStateLocked) {
				LockedWorldItems.Add (w);
				ActiveWorldItems.Remove (w);
				VisibleWorldItems.Remove (w);
				InvisibleWorldItems.Remove (w);
			} else {
				switch (w.ActiveState) {
				case WIActiveState.Invisible:
				default:
					LockedWorldItems.Remove (w);
					ActiveWorldItems.Remove (w);
					VisibleWorldItems.Remove (w);
					InvisibleWorldItems.Add (w);
					break;

				case WIActiveState.Visible:
					LockedWorldItems.Remove (w);
					ActiveWorldItems.Remove (w);
					VisibleWorldItems.Add (w);
					InvisibleWorldItems.Remove (w);
					break;

				case WIActiveState.Active:
					LockedWorldItems.Remove (w);
					ActiveWorldItems.Add (w);
					VisibleWorldItems.Remove (w);
					InvisibleWorldItems.Remove (w);
					break;
				}
			}
		}
		//sets any worlditems within range of the target position to active for this frame
		//they will naturally return to non-active on the next update cycle
		public void SetActiveStateOverride (Vector3 position, float range)
		{		//active worlditems are fine, invisible worlditems don't count
			var visibleEnum = Get.VisibleWorldItems.GetEnumerator ();
			while (visibleEnum.MoveNext ()) {
				if (Vector3.Distance (visibleEnum.Current.Position, position) < range) {
					visibleEnum.Current.ActiveState = WIActiveState.Active;
				}
			}
			LastPlayerPosition = Player.Local.Position;
			LastActiveDistanceUpdate++;
		}

		protected WIRadiusComparer ActiveRadiusComparer;
		protected WIRadiusComparer VisibleRadiusComparer;
		protected WIRadiusComparer InvisibleRadiusComparer;
		protected WIRadiusComparer LockedComparer;

		public class WIRadiusComparer : IComparer <WorldItem>
		{
			public Vector3 PlayerPosition;
			public WIActiveState State;
			public bool Locked;
			public int LastUpdate;

			public int Compare (WorldItem a, WorldItem b)
			{
				if (a == null) {
					if (b == null) {
						return 0;
					}
					//move null to the front
					return -1;
				} else if (b == null) {
					//move null to the front
					return 1;
				}
				//this is the first time it's been calculated this cycle
				//aldo check whether we need to refresh our active state
				if (!Locked) {
					if (a.LastActiveDistanceUpdate < LastUpdate) {
						a.LastActiveDistanceToPlayer = Vector3.Distance (a.Position, PlayerPosition) - Player.Local.ColliderRadius;
						a.LastActiveDistanceUpdate = LastActiveDistanceUpdate;
					}
					if (b.LastActiveDistanceUpdate < LastUpdate) {
						b.LastActiveDistanceToPlayer = Vector3.Distance (b.Position, PlayerPosition) - Player.Local.ColliderRadius;
						b.LastActiveDistanceUpdate = LastActiveDistanceUpdate;
					}
				}

				if (IsDirty (a)) {
					if (IsDirty (b)) {
						return 0;
					}
					return -1;
				} else if (IsDirty (b)) {
					return -1;
				}

				int result = a.DistanceToPlayer.CompareTo (b.DistanceToPlayer);
				if (result == 0) {
					//invert the active radius sizes - small is more important than large
					result = b.ActiveRadius.CompareTo (a.ActiveRadius);
				}
				return result;
			}

			public bool IsDirty (WorldItem w)
			{
				bool isDirty = (w.ActiveStateLocked != Locked) || w.Is (WILoadState.Unloaded | WILoadState.Unloading);
				if (!isDirty) {
					switch (w.ActiveState) {
					case WIActiveState.Active:
					default:
						if (w.DistanceToPlayer > w.ActiveRadius) {
							w.ActiveState = WIActiveState.Visible;
						}
						break;

					case WIActiveState.Visible:
						if (w.DistanceToPlayer < w.ActiveRadius) {
							w.ActiveState = WIActiveState.Active;
						} else if (w.DistanceToPlayer > w.VisibleDistance) {
							w.ActiveState = WIActiveState.Invisible;
						}
						break;

					case WIActiveState.Invisible:
						if (w.DistanceToPlayer < w.VisibleDistance) {
							w.ActiveState = WIActiveState.Visible;
						}
						break;
					}
					isDirty = !w.Is (State);
				}
				return isDirty;
			}
		}

		protected IEnumerator UpdateStackItemsToLoad ()
		{
			while (!GameManager.Is (FGameState.Quitting)) {
				for (int i = StackItemsToLoad.LastIndex (); i >= 0; i--) {
					while (!GameManager.Is (FGameState.InGame | FGameState.Cutscene | FGameState.GameLoading | FGameState.GameStarting)) {
						yield return null;
					}
					KeyValuePair <WIGroup, Queue <StackItem>> groupPair = StackItemsToLoad [i];
					if (groupPair.Key == null || groupPair.Value.Count == 0) {
						//if the group is gone or it's no longer loading, remove it, it's done
						StackItemsToLoad.RemoveAt (i);
					} else {
						WorldItem worlditem = null;
						//if (GameManager.Is (FGameState.InGame)) {
						////Debug.Log ("Loading stack items for " + StackItemsToLoad [i].Key.name);
						//otherwise pop the next stackitem  off the queue
						//and clone it
						CloneFromStackItem (groupPair.Value.Dequeue (), groupPair.Key, out worlditem);
					}
					yield return null;
				}
				yield return null;
			}
			yield break;
		}

		protected IEnumerator UnloadWorldItemsOverTime ()
		{
			//always dequeue a worlditem to save
			while (mInitialized) {
				if (WorldItemsToSave.Count > 0) {
					if (mSaveStackItem == null) {
						mSaveStackItem = new StackItem ();
					}
					if (mSaveStackItem.SaveState == null) {
						mSaveStackItem.SaveState = new WISaveState ();
					}
					mSaveStackItem.SaveState.Saved = false;
					KeyValuePair <string,WorldItem> wiToSave = WorldItemsToSave.Dequeue ();
					if (wiToSave.Value != null) {
						var saveEnum = wiToSave.Value.GetStackItemOverTime (mSaveStackItem, WIMode.Unloaded);
						while (saveEnum.MoveNext ()) {
							yield return saveEnum.Current;
						}
						Mods.Get.Runtime.SaveStackItemToGroup (mSaveStackItem, wiToSave.Key);
						//now clear the stack item and destroy the worlditem
						mSaveStackItem.Clear ();
						GameObject.Destroy (wiToSave.Value.gameObject);
						//just do one per update, give it some time to breathe
					} else {
						Debug.Log ("Worlditem to save was null by the time we got to it");
					}
				}
				yield return null;
			}
		}

		public IEnumerator UnloadAllWorldItems ()
		{
			yield break;
		}

		public bool FlushWorldItems ()
		{
			bool flushedLimit = RecentlyCreatedWorldItems.Count > 10;
			mIsFlushingWorldItems = true;
			for (int i = RecentlyCreatedWorldItems.LastIndex (); i >= 0; i--) {
				WorldItem recentlyCreatedWorldItem = RecentlyCreatedWorldItems [i];
				if (recentlyCreatedWorldItem != null) {
					if (!recentlyCreatedWorldItem.gameObject.activeSelf) {
						recentlyCreatedWorldItem.gameObject.SetActive (true);
					}
					recentlyCreatedWorldItem.Initialize ();
					//after calling intialize it will have been added to its group
					//and its position will have been updated
					//so we can safely predict its active state here
				}
			}
			RecentlyCreatedWorldItems.Clear ();
			mIsFlushingWorldItems = false;

			RecentlyCreatedWorldItems.AddRange (WorldItemsCreatedWhileFlushing);
			WorldItemsCreatedWhileFlushing.Clear ();

			return flushedLimit;
		}

		public static void OnWorldItemInitialized (WorldItem worldItem)
		{
			worldItem.LastActiveDistanceToPlayer = Vector3.Distance (worldItem.Position, LastPlayerPosition) - Player.Local.ColliderRadius;
			if (worldItem.ActiveStateLocked) {
				Get.LockedWorldItems.Add (worldItem);
			} else {
				if (worldItem.DistanceToPlayer > worldItem.VisibleDistance) {
					worldItem.ActiveState = WIActiveState.Invisible;
					Get.InvisibleWorldItems.Add (worldItem);
				} else if (worldItem.DistanceToPlayer > worldItem.ActiveRadius) {
					worldItem.ActiveState = WIActiveState.Visible;
					Get.VisibleWorldItems.Add (worldItem);
				} else {
					worldItem.ActiveState = WIActiveState.Active;
					Get.ActiveWorldItems.Add (worldItem);
				}
			}
		}

		public static void InitializeWorldItem (WorldItem newWorldItem)
		{
			if (Get.mIsFlushingWorldItems) {
				Get.WorldItemsCreatedWhileFlushing.Add (newWorldItem);
			} else {
				Get.RecentlyCreatedWorldItems.Add (newWorldItem);
			}
		}
	}
}
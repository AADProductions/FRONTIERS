using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Data;
using Frontiers.World;
using System;
using Frontiers.World.WIScripts;

namespace Frontiers
{
	public class WIGroups : Manager
	{
		public static WIGroups Get;

		public override void WakeUp()
		{
			base.WakeUp();

			Get = this;
			//create all the collections we're using
			//wheee collections
			mParentUnderManager = false;
			mGroupLookup = new Dictionary <string, WIGroup>();
			UnloaderMappings = new Dictionary <string, WIGroupUnloader>();
			Unloaders = new List<WIGroupUnloader>();
			LoadRequests = new List <WIGroupLoadRequest>();
			GroupsLoading = new List<WIGroup>();
			UnloadersToMerge = new List<WIGroupUnloader>();
			Groups = new List <WIGroup>();
			mChildGroupsToDestroy = new List<WIGroup>();
			mFildChildItemMr = new MobileReference();
			State = new WIGroupsState();
			DirtyGroups = new HashSet<WIGroup>();
		}

		public override void Initialize()
		{		//we have a couple of groups that are always created by default
			//set their props here before doing anything else
			Root.Props.ID = 1;
			World.Props.ID = 2;
			Paths.Props.ID = 3;
			Player.Props.ID = 4;
			Graveyard.Props.ID = 5;
			Multiplayer.Props.ID = 6;
			Plants.Props.ID = 7;
			Special.Props.ID = 8;
			Rivers.Props.ID = 9;

			Root.Props.FileName = "Root";
			World.Props.FileName = "World";
			Paths.Props.FileName = "Paths";
			Player.Props.FileName = "Player";
			Graveyard.Props.FileName = "Graveyard";
			Multiplayer.Props.FileName = "Multiplayer";
			Plants.Props.FileName = "Plants";
			Special.Props.FileName = "Special";
			Rivers.Props.FileName = "Rivers";

			Root.ParentGroup = null;
			World.ParentGroup = Root;
			Paths.ParentGroup = World;
			Player.ParentGroup = Root;
			Graveyard.ParentGroup = Root;
			Multiplayer.ParentGroup = Root;
			Plants.ParentGroup = Root;
			Special.ParentGroup = Root;
			Rivers.ParentGroup = World;

			Root.Owner = null;
			World.Owner = null;
			Paths.Owner = null;
			Player.Owner = Frontiers.Player.Local;
			Graveyard.Owner = null;
			Multiplayer.Owner = null;
			Plants.Owner = null;
			Special.Owner = null;
			Rivers.Owner = null;

			Plants.SaveOnUnload = false;

			Root.Initialize();
			World.Initialize();
			Paths.Initialize();
			Player.Initialize();
			Graveyard.Initialize();
			Multiplayer.Initialize();
			Plants.Initialize();
			Special.Initialize();
			Rivers.Initialize();
			mInitialized = true;
		}

		public override void OnModsLoadFinish()
		{
			mGroupLookup.Clear();

			//root is always loaded - it has nothing but dynamic groups inside it, never any WIs
			//graveyard we set to loaded because we never load anything there
			Root.Load();
			World.Load();
			Paths.Load();
			Player.Load();
			Graveyard.Load();
			Multiplayer.Load();
			Plants.Load();
			Special.Load();
			Rivers.Load();

			Root.Props.IgnoreOnSave = true;
			World.Props.IgnoreOnSave = true;
			Paths.Props.IgnoreOnSave = true;
			Player.Props.IgnoreOnSave = true;
			Graveyard.Props.IgnoreOnSave = true;
			Multiplayer.Props.IgnoreOnSave = true;
			Plants.Props.IgnoreOnSave = true;
			Special.Props.IgnoreOnSave = true;
			Rivers.Props.IgnoreOnSave = true;

			mGroupLookup.Add(Root.Props.UniqueID, Root);
			mGroupLookup.Add(World.Props.UniqueID, World);
			mGroupLookup.Add(Paths.Props.UniqueID, Paths);
			mGroupLookup.Add(Player.Props.UniqueID, Player);
			mGroupLookup.Add(Graveyard.Props.UniqueID, Graveyard);
			mGroupLookup.Add(Multiplayer.Props.UniqueID, Multiplayer);
			mGroupLookup.Add(Plants.Props.UniqueID, Plants);
			mGroupLookup.Add(Special.Props.UniqueID, Special);
			mGroupLookup.Add(Rivers.Props.UniqueID, Rivers);

			mModsLoaded = true;
		}

		public override void OnGameUnload()
		{
			StartCoroutine(UnloadPrimaryGroups());
		}

		protected IEnumerator UnloadPrimaryGroups()
		{
			yield return null;
			while (!GameWorld.Get.GameEnded) {
				//let gameworld reclaim chunk prefabs 
				yield return null;
			}

			yield return StartCoroutine(DestroyGroup(Root));
			yield return StartCoroutine(DestroyGroup(World));
			yield return StartCoroutine(DestroyGroup(Paths));
			yield return StartCoroutine(DestroyGroup(Player));
			yield return StartCoroutine(DestroyGroup(Graveyard));
			yield return StartCoroutine(DestroyGroup(Multiplayer));
			yield return StartCoroutine(DestroyGroup(Plants));
			yield return StartCoroutine(DestroyGroup(Special));
			yield return StartCoroutine(DestroyGroup(Rivers));

			mGameLoaded = false;
		}

		public WIGroup Root;
		public WIGroup World;
		public WIGroup Paths;
		public WIGroup Player;
		public WIGroup Graveyard;
		public WIGroup Multiplayer;
		public WIGroup Plants;
		public WIGroup Special;
		public WIGroup Rivers;
		protected static WIGroup mCurrentStructureGroup;
		protected static WIGroup mCurrentCityGroup;
		protected static WIGroup mCurrentRegionGroup;
		public int NumActiveRefreshers = 0;
		public bool PauseRefresh = false;
		public WIGroupsState State;
		public static Dictionary <string, WIGroupUnloader> UnloaderMappings;
		public static List <WIGroupUnloader> Unloaders;
		public static List <WIGroupLoadRequest> LoadRequests;
		public static List <WIGroup> GroupsLoading;
		public static HashSet <WIGroup> DirtyGroups;
		protected static List <WIGroupUnloader> UnloadersToMerge;
		protected static Dictionary <string, WIGroup> mGroupLookup;
		protected List <WIGroup> mChildGroupsToDestroy;
		public List <WIGroup> Groups;
		public static int NumWorldItemsLoadedThisFrame = 0;
		public static int MaxWorldItemsLoadedPerFrame = 10;

		public override void OnGameSave()
		{
			StartCoroutine(SaveGroupsOverTime());
		}

		public override void OnGameLoadStart()
		{
			StartCoroutine(UpdateGroupLoading());
		}

		#region refresh / load / unload / destroy

		public static void Refresh(WIGroup group)
		{
			//TODO refresh other groups affected by this group
			group.Refresh();
		}

		public static void UpdateDirtyGroup(WIGroup group)
		{
			DirtyGroups.Add(group);
		}

		public static void Load(WIGroup group)
		{
			if (group == null) {
				return;
			}

			WIGroupLoadRequest glr = null;

			switch (group.LoadState) {
				case WIGroupLoadState.PreparingToLoad:
				case WIGroupLoadState.Loading:
				case WIGroupLoadState.Loaded:
										//the only reason to load a group that's already loaded is if its parent is being UNloaded
										//which means it may disappear soon
					if (group.HasUnloadingParent) {
						//so add a request to load it later
						AddLoadRequest(group);
					}
					break;

				case WIGroupLoadState.Uninitialized:
				case WIGroupLoadState.Initializing:
				case WIGroupLoadState.Initialized:
										//load it normally, everything is dandy
					AddLoadRequest(group);
					break;

				case WIGroupLoadState.PreparingToUnload:
										//there's still a chance to cancel this unload request
					if (!group.TryToCancelLoad()) {
						//great! nothing left to do here
						return;
					} else {
						//if we can't cancel the unload (usually due to having an unloading parent)
						//there's nothing we can do but wait for it to unload
						//and then try to load it again
						AddLoadRequest(group);
					}
					break;

				case WIGroupLoadState.Unloading:
				case WIGroupLoadState.Unloaded:
										//it's going to disappear soon, so add the request now
										//and we'll load it again when the time comes
					AddLoadRequest(group);
					break;

				default:
					break;
			}
			//and that's it
			//we don't load groups directly any more
			//we let them get handled by UpdateGroups
			//that way there's no non-linear funny business
		}

		protected static void AddLoadRequest(WIGroup group)
		{
			for (int i = 0; i < LoadRequests.Count; i++) {
				if (LoadRequests[i].Group == group || LoadRequests[i].UniqueID == group.Props.UniqueID) {
					//it's already in there
					return;
				}		
			}
			LoadRequests.Add(new WIGroupLoadRequest(group));
		}

		public static void Unload(WIGroup group)
		{
			if (group.Is(WIGroupLoadState.PreparingToUnload
			       | WIGroupLoadState.Unloading
			       | WIGroupLoadState.Unloaded)) {
				//no need, it's already doing its thing
				return;
			}

			if (group.Is(WIGroupLoadState.Loading)) {
				group.AttemptedToUnload = true;
				//we have to wait for it to finish loading first
				return;
			}

			if (group.Is(WIGroupLoadState.Uninitialized
			       | WIGroupLoadState.Initializing)) {
				//this could be bad, potentially
				group.AttemptedToUnload = true;
				return;
			}

			//if there's already a key for this group then we don't need to create an unloader
			if (UnloaderMappings.ContainsKey(group.Path)) {
				return;
			}

			bool foundExistingUnloader = false;
			//if there isn't then we need to try and find an unloader that's the parent first
			for (int i = 0; i < Unloaders.Count; i++) {
				if (Unloaders[i].LoadState != WIGroupLoadState.Unloaded && Unloaders[i].RootGroup.IsParentOf(group)) {
					//in a perfect world it would already be in the unloader
					//but whatever, weird shit happens
					//add it now and the unloader will sort it out
					Unloaders[i].AddChildGroup(group);
					foundExistingUnloader = true;
					break;
				}
			}
			//if we STILL haven't found an unloader then we'll start a new one
			if (!foundExistingUnloader) {
				WIGroupUnloader unloader = null;
				if (!UnloaderMappings.TryGetValue(group.Path, out unloader)) {
					//if there isn't already a group working on this group
					//create one and add it to the list
					unloader = new WIGroupUnloader(group);
					unloader.Initialize();
					UnloaderMappings.Add(group.Path, unloader);
					Unloaders.Add(unloader);
					//newly irrelevant unloaders will be
					//taken care of in the update function
				}
			}
		}

		public static bool TryToCancelUnload(WIGroup group)
		{
			//we're going to reject all cancellation requests across the board
			/*
						WIGroupUnloader unloader = null;
						if (UnloaderMappings.TryGetValue(group, out unloader)) {
							unloader.TryToCancel();
						} else if (group.Is(WIGroupLoadState.PreparingToUnload)) {
							group.LoadState = WIGroupLoadState.Loaded;
						}
						*/
			return false;
		}

		public static void TryToAbsorbUnloaders(WIGroupUnloader parentUnloader)
		{
			if (parentUnloader.LoadState == WIGroupLoadState.Unloaded) {
				//there's no point
				return;
			}

			for (int i = Unloaders.LastIndex(); i >= 0; i--) {
				WIGroupUnloader childUnloader = Unloaders[i];
				if (childUnloader != parentUnloader && parentUnloader.RootGroup.IsParentOf(childUnloader.RootGroup)) {
					//absorb the child group into the parent group
					parentUnloader.AddChildGroup(childUnloader.RootGroup);
					//then remove all trace of the existing child group unloader
					UnloaderMappings.Remove(childUnloader.RootGroup.Path);
					Unloaders.RemoveAt(i);
					childUnloader.Clear();
				}
			}
		}

		public static void SaveToDisk(WIGroup group)
		{
			foreach (WIGroup childGroup in group.ChildGroups) {
				SaveToDisk(childGroup);
			}

			foreach (WorldItem childItem in group.ChildItems) {
				if (childItem != null) {
					StackItem stackItem = childItem.GetStackItem(WIMode.Frozen);
					Mods.Get.Runtime.SaveStackItemToGroup(stackItem, group.Props.PathName);
				}
			}
		}

		public static void SaveToGame(WIGroup group)
		{
			//create a clone of the group's props
			//we're going to add all of its child item names to its unloaded child item names
			WIGroupProps groupProps = ObjectClone.Clone <WIGroupProps>(group.Props);
			WorldItem childItem = null;
			var childItemEnumerator = group.ChildItems.GetEnumerator();
			while (childItemEnumerator.MoveNext()) {
				//for (int i = 0; i < group.ChildItems.Count; i++) {
				childItem = childItemEnumerator.Current;//group.ChildItems[i];
				if (childItem.SaveItemOnUnloaded) {
					groupProps.UnloadedChildItems.Add(childItem.FileName);
					StackItem stackItem = childItem.GetStackItem(childItem.Mode);
					Mods.Get.Runtime.SaveStackItemToGroup(stackItem, group.Props.UniqueID);
					//now that it's saved clear it immediately
					stackItem.Clear();
				}
			}
			for (int i = 0; i < group.ChildGroups.Count; i++) {
				groupProps.UnloadedChildGroups.Add(group.ChildGroups[i].FileName);
			}
			Mods.Get.Runtime.SaveGroupProps(groupProps);
			groupProps.Clear();
		}

		protected IEnumerator UpdateGroupLoading()
		{
			while (GameManager.State != FGameState.Quitting) {
				while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
					yield return null;
				}
				//start with our load requests
				//sort them shortest path to longest
				LoadRequests.Sort();
				for (int i = LoadRequests.LastIndex(); i >= 0; i--) {
					WIGroupLoadRequest glr = LoadRequests[i];
					if (glr.HasGroupReference) {
						//see if it's being unloaded already
						glr.OnHold = false;
						for (int j = 0; j < Unloaders.Count; j++) {
							if (Unloaders[j].RootGroup == glr.Group || Unloaders[j].RootGroup.IsParentOf(glr.Group)) {
								//whoops, can't do anything yet
								glr.OnHold = true;
								break;
							}
						}

						if (!glr.OnHold) {
							if (glr.Group.PrepareToLoad()) {
								//ooh, very exciting, put it in groups to load
								GroupsLoading.Add(glr.Group);
								LoadRequests.RemoveAt(i);
								glr.Clear();
								//we're done with load requests this round
								//we don't want to overload the frame
								break;
							} else {
								//see if we've timed out
								if (WorldClock.AdjustedRealTime > glr.Timeout) {
									GroupsLoading.Add(glr.Group);
									glr.Clear();
								}
							}
						}
					} else {
						//the group is gone but the requst lingers!
						glr.Clear();
						LoadRequests.RemoveAt(i);
					}
				}

				yield return null;

				for (int i = GroupsLoading.LastIndex(); i >= 0; i--) {
					WIGroup groupLoading = GroupsLoading[i];
					try {
						if (groupLoading == null || groupLoading.FinishedLoading) {
							GroupsLoading.RemoveAt(i);
						} else if (groupLoading.ReadyToLoad) {
							groupLoading.BeginLoad();
						}
					} catch (Exception e) {
						Debug.Log("ERROR while loading groups: " + e.ToString());
					}
					if (GameManager.Is(FGameState.InGame)) {
						yield return new WaitForSeconds(0.05f);
					}
				}

				yield return null;

				//now check to see if there are any unloaders that need to be merged
				//unloaders may have been created for child groups
				//and parents of those child groups may have received unloaders in the meantime
				//so sort that out and merge them all
				for (int i = 0; i < Unloaders.Count; i++) {
					TryToAbsorbUnloaders(Unloaders[i]);
				}

				yield return null;
				//now that we're all properly merged, sort everything
				//sorting the unloaders arranged them by depth
				//so we're getting the deepest groups first
				Unloaders.Sort();
				for (int i = Unloaders.LastIndex(); i >= 0; i--) {
					while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
						yield return null;
					}
					//first get any root groups to destroy
					//this means that the entire chain has been destroyed
					//and the group transforms are ready to be nuked
					WIGroupUnloader unloader = Unloaders[i];
					if (unloader == null) {
						Unloaders.RemoveAt(i);
					} else {
						if (unloader.LoadState == WIGroupLoadState.Unloaded) {
							//BOOM it's dead, get rid of it
							string rootGroupPath = unloader.RootGroup.Path;
							var destroyGroup = DestroyGroup(unloader.RootGroup);
							unloader.Clear();
							Unloaders.RemoveAt(i);
							while (destroyGroup.MoveNext()) {
								yield return destroyGroup.Current;
							}
							UnloaderMappings.Remove(rootGroupPath);
							break;
						}
					}
					yield return new WaitForSeconds(0.05f);
				}
				//that was intenst so wait a tick
				yield return null;
				//then check if there are any child groups to be destroyed
				//the unloaders will return a list of nodes at their greatest depth
				mChildGroupsToDestroy.Clear();
				for (int i = 0; i < Unloaders.Count; i++) {
					Unloaders[i].GetDeepestUnloadedChildGroups(mChildGroupsToDestroy);
				}
				if (mChildGroupsToDestroy.Count > 0) {
					for (int i = 0; i < mChildGroupsToDestroy.Count; i++) {
						while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
							yield return null;
						}
						var destroyGroup = DestroyGroup(mChildGroupsToDestroy[i]);
						while (destroyGroup.MoveNext()) {
							yield return destroyGroup.Current;
						}
						yield return new WaitForSeconds(0.05f);
					}
					mChildGroupsToDestroy.Clear();
				}
				//that was intense so wait a tick
				yield return null;
				//finally update any groups being unloaded
				for (int i = 0; i < Unloaders.Count; i++) {
					while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
						yield return null;
					}
					var checkGroupLoadStates = Unloaders[i].CheckGroupLoadStates();
					while (checkGroupLoadStates.MoveNext()) {
						yield return checkGroupLoadStates.Current;
					}
					yield return new WaitForSeconds(0.05f);
				}
				//don't bother to check for unloaded groups yet
				//we'll get them on the next cycle
				//just wait a tick to cool off
				yield return null;
			}
			yield break;
		}

		protected IEnumerator DestroyGroup(WIGroup group)
		{
			if (group != null && !group.IsDestroyed) {
				//GameObject.Destroy (group.gameObject, 0.1f);
				List <Transform> transformsToDestroy = new List <Transform>();
				List <Transform> transformsToReclaim = new List <Transform>();
				foreach (Transform childTransform in group.tr) {
					transformsToDestroy.Add(childTransform);
				}
				for (int i = 0; i < transformsToDestroy.Count; i++) {
					if (transformsToDestroy[i].gameObject.layer == Globals.LayerNumStructureTerrain) {
						transformsToDestroy[i].SendMessage("OnGroupUnloaded", SendMessageOptions.DontRequireReceiver);
					}
					GameObject.Destroy(transformsToDestroy[i].gameObject);
				}
				yield return null;
			}
			yield break;
		}

		public void Update () {
			if (DirtyGroups.Count > 0) {
				var dirtyGroupEnum = DirtyGroups.GetEnumerator();
				while (dirtyGroupEnum.MoveNext()) {
					dirtyGroupEnum.Current.UpdateDirty();
				}
				DirtyGroups.Clear();
			}
		}

		public void LateUpdate()
		{
			//reset every frame for WIGroups
			NumWorldItemsLoadedThisFrame = 0;
		}

		#endregion

		#region search

		public IEnumerator SaveGroupsOverTime()
		{

			WIGroup group = null;

			for (int i = Groups.LastIndex(); i >= 0; i--) {
				group = Groups[i];
				if (group == null) {
					Groups.RemoveAt(i);
				} else if (!group.Is(WIGroupLoadState.Uninitialized | WIGroupLoadState.Initialized | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
					SaveToGame(group);
				}
				//yield return null;
			}
			mGameSaved = true;
			yield break;
		}

		public static WIGroup GetCurrent()
		{
			WIGroup currentGroup = Get.World;
			if (Frontiers.Player.Local.HasSpawned) {
				if (Frontiers.Player.Local.Surroundings.IsInsideStructure) {
					//Debug.Log("Inside structure, returning structure group");
					currentGroup = Frontiers.Player.Local.Surroundings.LastStructureEntered.StructureGroup;
				} else if (Frontiers.Player.Local.Surroundings.IsVisitingLocation) {
					//Debug.Log("Not inside structure, returning first outside structure group");
					//make sure we don't return the structure group of the structure we're visiting
					for (int i = 0; i < Frontiers.Player.Local.Surroundings.VisitingLocations.Count; i++) {
						Frontiers.World.WIScripts.Location location = Frontiers.Player.Local.Surroundings.VisitingLocations[i];
						currentGroup = location.LocationGroup;
						if (!location.worlditem.Is<Frontiers.World.WIScripts.Structure>()) {
							//Debug.Log("Location group is NOT shingle, calling this good");
							break;
						} else {
							//Debug.Log("Location was a structure, continuing till we find something better");
						}
					}
				} else { 
					currentGroup = GameWorld.Get.PrimaryChunk.AboveGroundGroup;
				}
			}
			//Debug.Log("Final result: " + currentGroup.Path);
			return currentGroup;
		}

		public static IEnumerator GetAllChildrenByType(string startGroup, List <string> wiScriptTypes, List <WorldItem> childrenOfType, Vector3 searchOrigin, float searchRadius, int maxItems)
		{	//get all the live groups immediately below us
			Queue <string> groupPathsQueue = new Queue<string>();
			groupPathsQueue.Enqueue(startGroup);
			yield return Get.StartCoroutine(GetAllPaths(startGroup, GroupSearchType.LiveOnly, groupPathsQueue));
			//then search all the groups for items OR until we hit our max items
			while (childrenOfType.Count < maxItems) {
				//get the next group and check its child items
				if (groupPathsQueue.Count > 0) {
					string nextGroupPath = groupPathsQueue.Dequeue();
					WIGroup nextGroup = null;
					if (FindGroup(nextGroupPath, out nextGroup)) {
						List <WorldItem> children = nextGroup.GetChildrenOfType(wiScriptTypes);
						for (int i = 0; i < children.Count; i++) {
							if (Vector3.Distance(children[i].transform.position, searchOrigin) < searchRadius) {
								//if it's in range then add it to the list
								childrenOfType.Add(children[i]);
							}
							if (childrenOfType.Count >= maxItems) {
								//are we over our max item count?
								//if so we're done here
								break;
							}
						}
					}
					if (GameManager.Is(FGameState.InGame)) {
						//wait a tick
						yield return null;
					}
				} else {
					break;
				}
			}
			yield break;
		}

		public static bool GetAllContainers(WIGroup group, List<Container> containers)
		{
			if (group == null)
				return false;

			Container container = null;
			var childItemEnum = group.ChildItems.GetEnumerator();
			while (childItemEnum.MoveNext()) {
				if (childItemEnum.Current.Is<Container>(out container)) {
					containers.Add(container);
				}
			}
			for (int i = 0; i < group.ChildGroups.Count; i++) {
				GetAllContainers(group.ChildGroups[i], containers);
			}
			return containers.Count > 0;
		}

		public static IEnumerator GetAllStackItemsByType(string startGroup, List <string> wiScriptTypes, GroupSearchType searchType, Queue <StackItem> stackItemQueue)
		{
			return Get.GetAllStackItemsByTypeOverTime(startGroup, wiScriptTypes, searchType, stackItemQueue);
		}

		protected IEnumerator GetAllStackItemsByTypeOverTime(string groupPath, List <string> wiScriptTypes, GroupSearchType searchType, Queue <StackItem> stackItemQueue)
		{	//start by getting all the paths to search for
			//Debug.Log("Getting all stack items by type");
			Queue <string> groupPathsQueue = new Queue <string>();
			yield return StartCoroutine(GetAllPaths(groupPath, searchType, groupPathsQueue));
			//once we've got all the paths, start searching them for stack items
			while (groupPathsQueue.Count > 0) {	//while we've got paths to search...
				//get the next group and load its stack items
				//if the stack items
				//TODO tie this to search type, currently this is all saved only
				mGetStackItemsNextGroupPath = groupPathsQueue.Dequeue();
				mGetStackItemsChildNames = Mods.Get.Runtime.GroupChildItemNames(mGetStackItemsNextGroupPath, false);
				for (int i = 0; i < mGetStackItemsChildNames.Count; i++) {
					StackItem stackItem = null;
					if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, mGetStackItemsNextGroupPath, mGetStackItemsChildNames[i], true)) {
						//check to see if it has any of the scripts indicated
						if (stackItem.HasAtLeastOne(wiScriptTypes)) {
							stackItemQueue.Enqueue(stackItem);
							//break;
						}
					}

					if (GameManager.Is(FGameState.InGame)) {
						//wait a tick
						yield return null;
					}
				}
				if (GameManager.Is(FGameState.InGame)) {
					//wait a tick
					yield return null;
				}

				mGetStackItemsChildNames.Clear();
				mGetStackItemsChildNames = null;
			}
			groupPathsQueue.Clear();
			groupPathsQueue = null;
			yield break;
		}

		protected string mGetStackItemsNextGroupPath;
		protected List <string> mGetStackItemsChildNames;

		public static IEnumerator GetAllPaths(string groupPath, GroupSearchType searchType, Queue <string> groupPathsQueue)
		{
			return Get.GetAllPathsOverTime(groupPath, searchType, groupPathsQueue);
		}
		//returns a full recursive search of the tree with groups
		//search type specifies whether to use 'live' data (slow) or directory data (fast)
		//since live is potentially expensive as hell we use an enumerator
		protected IEnumerator GetAllPathsOverTime(string groupPath, GroupSearchType searchType, Queue <string> groupPathQueue)
		{
			//get the first group
			//		WIGroup startGroup = null;
			//		if (!WIGroups.FindGroup (groupPath, out startGroup))
			//		{	//whoops
			//			yield break;
			//		}
			if (GameManager.Is(FGameState.InGame)) {
				yield return null;
			}
			List <string> childGroupPaths = GetChildGroupPaths(groupPath, searchType);
			for (int i = 0; i < childGroupPaths.Count; i++) {	//put the result in the queue
				groupPathQueue.Enqueue(childGroupPaths[i]);
				//start the coroutine recursively
				yield return StartCoroutine(GetAllPathsOverTime(childGroupPaths[i], searchType, groupPathQueue));
			}
			yield break;
		}

		public static List <string> GetChildGroupPaths(string groupPath, GroupSearchType searchType)
		{
			List <string> groupPaths = new List <string>();
			switch (searchType) {
				case GroupSearchType.LiveOnly:
					WIGroup group = null;
					if (WIGroups.FindGroup(groupPath, out group)) {
						for (int i = 0; i < group.ChildGroups.Count; i++) {
							groupPaths.Add(group.ChildGroups[i].Props.PathName);
						}
					}
					break;

				case GroupSearchType.SavedOnly:
				default:
					groupPaths.AddRange(Mods.Get.Runtime.GroupChildGroupNames(groupPath, true));
					break;
			}
			return groupPaths;
		}
		//loads the stack item immediately, no delays
		public static bool LoadStackItem(MobileReference reference, out StackItem stackItem)
		{
			WIGroup group = null;
			//first check to see if the group is loaded
			if (FindGroup(reference.GroupPath, out group)) {//wuhoo, we don't even need to load anything
				WorldItem childItem = null;
				if (group.FindChildItem(reference.FileName, out childItem)) {//hooray, send back the stack item right away
					stackItem = childItem.GetStackItem(childItem.Mode);
					return true;
				}
			}
			//if we've made it this far, load the stack item from disk
			stackItem = null;
			if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, WIGroup.GetUniqueID(reference.GroupPath), reference.FileName, true)) {	//hooray we found it
				return true;
			}
			return false;
		}

		public static void SuperLoadStackItem(string groupPath, string childItemFileName, IWIBaseCallback callBack)
		{
			WIGroup group = null;
			IWIBase iwiBase = null;
			//first check to see if the group is loaded
			if (FindGroup(groupPath, out group)) {	//wuhoo, we don't even need to load anything
				WorldItem childItem = null;
				if (group.FindChildItem(childItemFileName, out childItem)) {
					//hooray, send back the stack item right away
					iwiBase = childItem;
					if (callBack != null) {
						callBack(iwiBase);
					}
					return;
				}
			}
			
			//if we've made it this far, load the stack item from disk
			StackItem stackItem = null;
			if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, groupPath, childItemFileName, true)) {
				iwiBase = stackItem;
			}
			
			//even if we didn't successfully load it, call the callback
			if (callBack != null) {
				callBack(iwiBase);
			}
		}

		public static bool FindGroup(string groupPath, out WIGroup group)
		{
			string uniqueID = WIGroup.GetUniqueID(groupPath);
			if (mGroupLookup.TryGetValue(WIGroup.GetUniqueID(groupPath), out group)) {
				if (group == null) {
					mGroupLookup.Remove(uniqueID);
					return false;
				}
				return true;
			}
			return false;
		}

		public static bool FindChildItem(string childItemPath, out WorldItem childItem)
		{
			WIGroup group = null;
			childItem = null;
			mFildChildItemMr.FullPath = childItemPath;
			if (FindGroup(mFildChildItemMr.GroupPath, out group)) {
				return (group.FindChildItem(mFildChildItemMr.FileName, out childItem));
			}
			return false;
		}

		protected static MobileReference mFildChildItemMr;

		public static bool FindChildItem(string groupPath, string childItemFileName, out WorldItem childItem)
		{
			WIGroup group	= null;
			childItem = null;
			if (FindGroup(groupPath, out group)) {
				return (group.FindChildItem(childItemFileName, out childItem));
			}
			return false;
		}

		public static IEnumerator SuperLoadChildItem(string groupPath, string childItemFileName, Action <WorldItem> callBack, float minimumDelay)
		{
			return Get.SuperLoadChildItemOverTime(groupPath, childItemFileName, callBack, minimumDelay);
		}

		public static bool IsLoaded(string groupPath, out WIGroup group)
		{
			return Get.Root.FindChildGroup(WIGroup.SplitPath(groupPath), out group);
		}

		protected IEnumerator SuperLoadChildItemOverTime(string groupPath, string childItemFileName, Action <WorldItem> callBack, float minimumDelay)
		{
			//first see if we even need to superload it
			WIGroup group = null;
			if (FindGroup(groupPath, out group)) {
				WorldItem childItem = null;
				if (group.FindChildItem(childItemFileName, out childItem)) {
					yield return null;
					//yield return new WaitForSeconds (minimumDelay);
					if (callBack != null)
						callBack(childItem);
					yield break;
				}
			}
			//if we don't find the group, create the superloader
			GameObject newSuperLoader = Get.gameObject.CreateChild("WorldItemSuperLoader: " + childItemFileName).gameObject;
			WorldItemSuperLoader superLoader = newSuperLoader.AddComponent <WorldItemSuperLoader>();
			superLoader.GroupPath = groupPath;
			superLoader.ChildItemFileName = childItemFileName;
			superLoader.CallBack = callBack;

			yield return superLoader.StartCoroutine(superLoader.LoadGroupsOverTime());
		}

		#endregion

		#region group creation

		public static WIGroup GetOrAdd(GameObject attachTo, string groupName, WIGroup parentGroup, IStackOwner owner)
		{
			WIGroup group = null;
			if (attachTo.HasComponent <WIGroup>(out group)) {
				group.Props.IgnoreOnSave = false;
				group.Owner = owner;
				parentGroup.AddChildGroup(group);
				group.Initialize();
				return group;
			} else {
				string uniqueID = WIGroup.GetUniqueID(WIGroup.GetChildPathName(parentGroup.Path, groupName));
				if (mGroupLookup.TryGetValue(uniqueID, out group)) {
					if (group != null && !group.IsDestroyed) {
						return group;
					} else {
						mGroupLookup.Remove(uniqueID);
					}
				}
			}

			if (parentGroup != null) {
				group = attachTo.AddComponent <WIGroup>();
				group.Props.FileName = groupName;
				group.Owner = owner;
				//try to load props
				//don't bother to check for success, we do the same thing either way
				Mods.Get.Runtime.LoadGroupProps(ref group.Props, WIGroup.GetUniqueID(WIGroup.GetChildPathName(parentGroup.Path, groupName)));
				group.Props.IgnoreOnSave = false;
				parentGroup.AddChildGroup(group);

				Get.Groups.Add(group);
				group.Initialize();
				if (!mGroupLookup.ContainsKey(group.Props.UniqueID)) {
					mGroupLookup.Add(group.Props.UniqueID, group);
				} else {
					//this is now the latest and greatest group
					mGroupLookup[group.Props.UniqueID] = group;
				}
			}
			return group;
		}

		public static WIGroup GetOrAdd(string groupName, WIGroup parentGroup, IStackOwner owner)
		{
			WIGroup group = null;
			string uniqueID = WIGroup.GetUniqueID(WIGroup.GetChildPathName(parentGroup.Path, groupName));
			if (mGroupLookup.TryGetValue(uniqueID, out group)) {
				//if it exists in the lookup and isn't null / destroyed
				if (group != null && !group.IsDestroyed) {
					//return that instead
					return group;
				} else {
					//if it is null or destroyed, remove it
					//it will be created below
					mGroupLookup.Remove(uniqueID);
				}
			}

			if (parentGroup != null) {
				GameObject wiGroupGameObject = parentGroup.gameObject.FindOrCreateChild(groupName).gameObject;
				group = wiGroupGameObject.AddComponent <WIGroup>();
				group.Props.IgnoreOnSave = false;
				group.Owner = owner;
				group.Props.FileName = groupName;
				//try to load props
				//don't bother to check for success, we do the same thing either way
				Mods.Get.Runtime.LoadGroupProps(ref group.Props, WIGroup.GetUniqueID(WIGroup.GetChildPathName(parentGroup.Path, groupName)));
				parentGroup.AddChildGroup(group);
				group.Initialize();
				Get.Groups.SafeAdd(group);
				if (!mGroupLookup.ContainsKey(group.Props.UniqueID)) {
					mGroupLookup.Add(group.Props.UniqueID, group);
				} else {
					//this is now the latest and greatest group
					mGroupLookup[group.Props.UniqueID] = group;
				}
			}
			return group;
		}

		#endregion

		#region static helpers and enums

		protected static int GetNextID(int managerID)
		{
			return managerID++;
		}

		#endregion

		#if UNITY_EDITOR
		WIGroupUnloader unloader;

		public void DrawEditor()
		{
			if (Application.isPlaying) {
				var enumerator = Unloaders.GetEnumerator();
				while (enumerator.MoveNext()) {
					//foreach (WIGroupUnloader unloader in Unloaders) {
					unloader = enumerator.Current;
					switch (unloader.LoadState) {
						case WIGroupLoadState.Loaded:
						default:
							UnityEngine.GUI.color = Color.green;
							break;

						case WIGroupLoadState.Unloaded:
							UnityEngine.GUI.color = Color.red;
							break;

						case WIGroupLoadState.PreparingToUnload:
						case WIGroupLoadState.Unloading:
							UnityEngine.GUI.color = Color.yellow;
							break;
					}
					string rootGroup = "(NULL)";
					if (unloader.RootGroup != null) {
						rootGroup = unloader.RootGroup.name;
						rootGroup += "(" + unloader.NotPreparedToUnload.Count.ToString() + " NOT PREPARED)\n";
						rootGroup += "(" + unloader.PreparingToUnload.Count.ToString() + " PREPARING)\n";
						rootGroup += "(" + unloader.ReadyToUnload.Count.ToString() + " READY TO UNLOAD)\n";
						rootGroup += "(" + unloader.Unloading.Count.ToString() + " UNLOADING)\n";
						rootGroup += "(" + unloader.FinishedUnloading.Count.ToString() + " FINISHED UNLOADING)\n";
					}
					UnityEngine.GUILayout.Button(rootGroup + ": " + unloader.LoadState.ToString());
				}
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		#endif
	}
}
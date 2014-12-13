using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.Data;
using Frontiers.World;
using Frontiers.World.Locations;
using System;

namespace Frontiers
{
		public class WIGroups : Manager
		{
				public static WIGroups Get;

				public override void WakeUp()
				{
						Get = this;
						//create all the collections we're using
						//wheee collections
						mParentUnderManager = false;
						mGroupLookup = new Dictionary <string, WIGroup>();
						UnloaderMappings = new Dictionary <IUnloadableParent, WIGroupUnloader>();
						Unloaders = new List<WIGroupUnloader>();
						GroupsToLoad = new List <WIGroup>();
						GroupsLoading = new List<WIGroup>();
						UnloadersToMerge = new List<WIGroupUnloader>();
						Groups = new List <WIGroup>();
						mChildGroupsToDestroy = new List<WIGroup>();
						mFildChildItemMr = new MobileReference();
						State = new WIGroupsState();
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

						Root.DestroyChildren();
						World.DestroyChildren();
						Paths.DestroyChildren();
						Player.DestroyChildren();
						Graveyard.DestroyChildren();
						Multiplayer.DestroyChildren();
						Plants.DestroyChildren();
						Special.DestroyChildren();
						Rivers.DestroyChildren();

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
				public static Dictionary <IUnloadableParent, WIGroupUnloader> UnloaderMappings;
				public static List <WIGroupUnloader> Unloaders;
				public static List <WIGroup> GroupsToLoad;
				public static List <WIGroup> GroupsLoading;
				protected static List <WIGroupUnloader> UnloadersToMerge;
				protected static Dictionary <string, WIGroup> mGroupLookup;
				protected List <WIGroup> mChildGroupsToDestroy;
				public List <WIGroup> Groups;

				public override void OnGameSave()
				{
						StartCoroutine(SaveGroupsOverTime());
				}

				public override void OnGameLoadStart()
				{
						StartCoroutine(UpdateGroups());
				}

				#if UNITY_EDITOR
				WIGroupUnloader unloader;

				public void DrawEditor()
				{
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
				#endif
				#region refresh / load / unload / destroy

				public static void Refresh(WIGroup group)
				{
						//TODO refresh other groups affected by this group
						group.Refresh();
				}

				public static void Load(WIGroup group)
				{
						if (group.Is(WIGroupLoadState.Loaded)) {
								return;
						}

						bool loadLater = false;
						if (group.Is(WIGroupLoadState.PreparingToUnload)) {
								CancelUnload(group);
						}

						if (GameManager.Is(FGameState.InGame)) {
								if (!GroupsLoading.Contains(group)) {
										GroupsToLoad.SafeAdd(group);
								}
						} else if (group.PrepareToLoad() && group.ReadyToLoad) {
								group.BeginLoad();
						} else {
								GroupsToLoad.SafeAdd(group);
						}
				}

				public static void Unload(WIGroup group)
				{
						if (group.Is(
								 WIGroupLoadState.PreparingToUnload
								 | WIGroupLoadState.Unloading
								 | WIGroupLoadState.Unloaded
								 | WIGroupLoadState.Uninitialized
								 | WIGroupLoadState.Initializing)) {
								//no need
								return;
						}

						GroupsToLoad.Remove(group);
						GroupsLoading.Remove(group);

						WIGroupUnloader unloader = null;
						if (!UnloaderMappings.TryGetValue(group, out unloader)) {
								//if there isn't already a group working on this group
								//create one and add it to the list
								unloader = new WIGroupUnloader(group);
								unloader.Initialize();
								UnloaderMappings.Add(group, unloader);
								Unloaders.Add(unloader);
								//this unload request may have resulted in 'lower' unloaders
								//becoming irrelevant
								ResolveUnloaders(unloader);
						}
				}

				public static void CancelUnload(WIGroup group)
				{
						WIGroupUnloader unloader = null;
						if (UnloaderMappings.TryGetValue(group, out unloader)) {
								unloader.TryToCancel();
						} else if (group.Is(WIGroupLoadState.PreparingToUnload)) {
								group.LoadState = WIGroupLoadState.Loaded;
						}
				}

				public static void ResolveUnloaders(WIGroupUnloader newUnloader)
				{
						//the root group might have been made irrelevant now
						//if this new group is the new shallowest unloading parent of any earlier nodes
						//then those old unloaders need to be gathered up and added to this new unloader
						//otherwise just initialize it normally
						UnloadersToMerge.Clear();
						for (int i = Unloaders.LastIndex(); i >= 0; i--) {
								WIGroupUnloader oldUnloader = Unloaders[i];
								if (oldUnloader.RootGroup.HasUnloadingParent) {
										//if it has an unloading parent then it's no longer a root
										//this can only be caused by the new unloader
										//(TODO verify that this is true??)
										UnloadersToMerge.Add(oldUnloader);
										UnloaderMappings.Remove(oldUnloader.RootGroup);
										Unloaders.RemoveAt(i);
								}
						}
						//the state of the merged unloaders isn't important
						//because it will be recreated in the new unloader
						//so clear them and destroy them
						if (UnloadersToMerge.Count > 0) {
								for (int i = 0; i < UnloadersToMerge.Count; i++) {
										UnloadersToMerge[i].Clear();
								}
								UnloadersToMerge.Clear();
						}

						//now initialize the new unloader
						//this will find all the old groups that were unloading
						//and recreate their state
						newUnloader.Initialize();
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
						for (int i = 0; i < group.ChildItems.Count; i++) {
								childItem = group.ChildItems[i];
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

				protected IEnumerator UpdateGroups()
				{
						while (GameManager.State != FGameState.Quitting) {
								while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
										yield return null;
								}
								//first get any root groups to destroy
								//this means that the entire chain has been destroyed
								//and the group transforms are ready to be nuked
								Unloaders.Sort();
								//sorting the unloaders arranged them by depth
								//so we're getting the deepest groups first
								for (int i = Unloaders.LastIndex(); i >= 0; i--) {
										while (!GameManager.Is(FGameState.InGame | FGameState.GameLoading | FGameState.GameStarting)) {
												yield return null;
										}
										WIGroupUnloader unloader = Unloaders[i];
										if (unloader == null) {
												Unloaders.RemoveAt(i);
										} else {
												if (unloader.IsCanceled) {
														UnloaderMappings.Remove(unloader.RootGroup);
														unloader.Clear();
														Unloaders.RemoveAt(i);
												} else if (unloader.LoadState == WIGroupLoadState.Unloaded) {
														UnloaderMappings.Remove(unloader.RootGroup);
														yield return StartCoroutine(DestroyGroup(unloader.RootGroup));
														unloader.Clear();
														Unloaders.RemoveAt(i);
														break;
												}
										}
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
												yield return StartCoroutine(DestroyGroup(mChildGroupsToDestroy[i]));
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
										yield return StartCoroutine(Unloaders[i].CheckGroupLoadStates());
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
								yield return null;

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

				protected int mUpdateGroups = 0;

				public void Update()
				{
						if (GameManager.Is(FGameState.InGame)) {
								if (mUpdateGroups < 9) {
										mUpdateGroups++;
										return;
								}
						}
						mUpdateGroups = 0;

						//now take care of groups that need to be loaded
						for (int i = GroupsToLoad.LastIndex(); i >= 0; i--) {
								WIGroup groupToLoad = GroupsToLoad[i];
								if (groupToLoad == null) {
										GroupsToLoad.RemoveAt(i);
								} else if (groupToLoad.PrepareToLoad()) {
										GroupsToLoad.RemoveAt(i);
										GroupsLoading.Add(groupToLoad);
								} else {
										GroupsToLoad.RemoveAt(i);
								}
						}

						for (int i = GroupsLoading.LastIndex(); i >= 0; i--) {
								WIGroup groupLoading = GroupsLoading[i];
								if (groupLoading == null || groupLoading.FinishedLoading) {
										GroupsLoading.RemoveAt(i);
								} else if (groupLoading.ReadyToLoad) {
										groupLoading.BeginLoad();
								}
						}
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
						//TEMP
						//TODO use player last visited location instead
						//then remove this altogether
						return Get.World;
				}

				public static IEnumerator GetAllChildrenByType(string startGroup, List <string> wiScriptTypes, List <WorldItem> childrenOfType, Vector3 searchOrigin, float searchRadius, int maxItems)
				{	//get all the live groups immediately below us
						Queue <string> groupPathsQueue = new Queue<string>();
						groupPathsQueue.Enqueue(startGroup);
						yield return Get.StartCoroutine(GetAllPaths(startGroup, SearchType.LiveOnly, groupPathsQueue));
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

				public static IEnumerator GetAllStackItemsByType(string startGroup, List <string> wiScriptTypes, SearchType searchType, Queue <StackItem> stackItemQueue)
				{
						return Get.GetAllStackItemsByTypeOverTime(startGroup, wiScriptTypes, searchType, stackItemQueue);
				}

				protected IEnumerator GetAllStackItemsByTypeOverTime(string groupPath, List <string> wiScriptTypes, SearchType searchType, Queue <StackItem> stackItemQueue)
				{	//start by getting all the paths to search for
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
														break;
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

				public static IEnumerator GetAllPaths(string groupPath, SearchType searchType, Queue <string> groupPathsQueue) {
						return Get.GetAllPathsOverTime(groupPath, searchType, groupPathsQueue);
				}
				//returns a full recursive search of the tree with groups
				//search type specifies whether to use 'live' data (slow) or directory data (fast)
				//since live is potentially expensive as hell we use an enumerator
				protected IEnumerator GetAllPathsOverTime(string groupPath, SearchType searchType, Queue <string> groupPathQueue)
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

				public static List <string> GetChildGroupPaths(string groupPath, SearchType searchType)
				{
						List <string> groupPaths = new List <string>();
						switch (searchType) {
								case SearchType.LiveOnly:
										WIGroup group = null;
										if (WIGroups.FindGroup(groupPath, out group)) {
												for (int i = 0; i < group.ChildGroups.Count; i++) {
														groupPaths.Add(group.ChildGroups[i].Props.PathName);
												}
										}
										break;

								case WIGroups.SearchType.SavedOnly:
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
								mGroupLookup.Add(group.Props.UniqueID, group);
								group.Initialize();
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
								Get.Groups.SafeAdd(group);
								mGroupLookup.Add(group.Props.UniqueID, group);
								group.Initialize();
						}
						return group;
				}

				#endregion

				#region static helpers and enums

				protected static int GetNextID(int managerID)
				{
						return managerID++;
				}

				public enum SearchType
				{
						LiveOnly,
						LiveThenSaved,
						SavedOnly
				}

				#endregion

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

		public enum GroupLookupType
		{
				Location,
				QuestItem,
				Character,
		}
}
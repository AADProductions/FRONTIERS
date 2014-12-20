using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World.Gameplay;
using Frontiers.World.Locations;
using System.Xml.Serialization;

namespace Frontiers.World
{
		[ExecuteInEditMode]
		public class WIGroup : MonoBehaviour, IUnloadableChild, IUnloadableParent, ILoadable, IUnloadable, IComparable <WIGroup>
		{
				public WIGroupProps Props;
				public WIGroup ParentGroup;
				public Structure ParentStructure;
				public WorldChunk Chunk;
				public List <WIGroup> ChildGroups;
				public List <WorldItem> ChildItems;
				public Transform tr;
				//convenience
				public string Path {
						get {
								return Props.PathName;
						}
				}

				public string FileName {
						get {
								return Props.FileName;
						}
				}

				public bool SaveOnUnload = false;

				public void DestroyChildren()
				{
						List <Transform> childrenToDestroy = new List<Transform>();
						foreach (Transform child in transform) {
								childrenToDestroy.Add(child);
						}
						for (int i = 0;i < childrenToDestroy.Count;i++) {
								GameObject.Destroy(childrenToDestroy [i].gameObject);
						}
						ChildGroups.Clear();
						ChildItems.Clear();
						Chunk = null;
						Props.UnloadedChildGroups.Clear();
						Props.UnloadedChildItems.Clear();
						mSavedState = false;
						mLoadState = WIGroupLoadState.Unloaded;
				}

				public List <ActionNodeState> GetActionNodes()
				{
						WorldChunk chunk = GetParentChunk();
						List <ActionNodeState> nodeStates = null;
						if (!chunk.GetNodesForLocation(Path, out nodeStates)) {
								nodeStates = new List<ActionNodeState>();
						}
						return nodeStates;
				}

				public bool IsDestroyed {
						get {
								return mDestroyed;
						}
				}

				public IStackOwner Owner {
						get {
								IStackOwner owner = null;
								if (mOwner != null) {
										owner = mOwner;
								} else if (!IsRoot) {
										owner = ParentGroup.Owner;
								}
								return owner;
						}
						set {
								//TODO avoid loop?
								mOwner = value;
						}
				}

				public bool Is(WIGroupLoadState loadState)
				{
						return Frontiers.Flags.Check((uint)mLoadState, (uint)loadState, Frontiers.Flags.CheckType.MatchAny);
				}

				public WIGroupLoadState LoadState {
						get {
								return mLoadState;
						}
						set {
								if (mLoadState != value) {
										mLoadState = value;
										RefreshRequest();
										#if UNITY_EDITOR
										if (!mDestroyed) {
												UnityEditor.EditorUtility.SetDirty(this);
												UnityEditor.EditorUtility.SetDirty(gameObject);
										}
										#endif
								}
								OnLoadStateChange.SafeInvoke();
						}
				}

				public Action OnLoadStateChange;
				public Action OnChildItemAdded;
				public Action OnChildItemRemoved;
				protected IStackOwner mOwner;
				protected bool mIsDirty;
				protected bool mDestroyed;
				protected bool mSavedState = false;
				protected WIGroupLoadState mLoadState;

				public void OnDestroy()
				{
						mDestroyed = true;
						if (!mSavedState) {
								Debug.Log ("WIGROUP " + name + " WAS DESTROYED WITHOUT SAVING STATE");
						}
				}

				#region ownership / parent / manager

				public bool FindChildGroup(Stack <string> splitPath, out WIGroup childGroup)
				{
						bool result = false;
						childGroup = null;
						if (splitPath.Count > 0) {
								WIGroup nextChildGroup = null;
								string nextChildGroupName = splitPath.Pop();
								if (mChildGroupLookup.TryGetValue(nextChildGroupName, out nextChildGroup)) {
										//we've found the next child group!
										if (splitPath.Count > 0) {
												//if the count is greater than zero, then the search continues
												return nextChildGroup.FindChildGroup(splitPath, out childGroup);
										} else {
												//if the count is zero, then this is the group we were supposed to find
												childGroup = nextChildGroup;
												result = true;
										}
								}
						}
						//if we've gotten this far without a true result
						//then that means the group didn't exist or wasn't loaded
						return result;
				}

				public bool IsRoot {
						get {
								return ParentGroup == null;
						}
				}

				public bool IsChunk {
						get {
								return Chunk != null;
						}
				}

				public bool IsStructureGroup {
						get {
								return ParentStructure != null;
						}
				}

				public bool IsDirty {
						get {
								return mIsDirty;
						}
						set {
								mIsDirty = false;
						}
				}

				public bool HasParentGroup {
						get {
								return ParentGroup != null;
						}
				}

				public bool HasOwner(out IStackOwner owner)
				{
						owner = Owner;
						if (owner == null && HasParentGroup) {
								ParentGroup.HasOwner(out owner);
						}
						return owner != null;
				}

				public bool HasChildGroups {
						get {
								return ChildGroups.Count > 0;
						}
				}

				public bool HasChildItems {
						get {
								return (ChildItems.Count == 0);
						}
				}

				public WorldChunk GetParentChunk()
				{
						if (mCachedChunk != null) {
								return mCachedChunk;
						}
						if (IsChunk) {
								mCachedChunk = Chunk;
						} else if (HasParentGroup) {
								if (ParentGroup.IsChunk) {
										mCachedChunk = ParentGroup.Chunk;
								} else {
										mCachedChunk = ParentGroup.GetParentChunk();
								}
						}
						return mCachedChunk;
				}

				public Structure GetParentStructure()
				{
						if (mCachedStructure != null) {
								return mCachedStructure;
						}
						if (IsStructureGroup) {
								mCachedStructure = ParentStructure;
						} else if (HasParentGroup) {
								if (ParentGroup.IsStructureGroup) {
										mCachedStructure = ParentGroup.ParentStructure;
								} else {
										mCachedStructure = ParentGroup.GetParentStructure();
								}
						}
						return mCachedStructure;
				}

				protected Structure mCachedStructure = null;
				protected WorldChunk mCachedChunk = null;

				#endregion

				#region initialization and refresh

				public void Awake()
				{
						if (Props == null)
								Props = new WIGroupProps();
		
						if (ChildGroups == null)
								ChildGroups = new List <WIGroup>();

						if (ChildItems == null)
								ChildItems = new List <WorldItem>();


						tr = transform;
						DontDestroyOnLoad(tr);
						mLoadState = WIGroupLoadState.Uninitialized;
						mChildItemLookup = new Dictionary<string, WorldItem>();
						mChildGroupLookup = new Dictionary<string, WIGroup>();
						mStackItemsToLoad = new Queue<StackItem>();
				}

				public void Initialize()
				{
						if (!Is(WIGroupLoadState.Uninitialized)) {
								return;
						}

						Props.PathName = GetPathName(this);
						Props.UniqueID = GetUniqueID(this);
						Props.Name = Props.UniqueID;
						if (HasParentGroup && ParentGroup.Props.TerrainType == LocationTerrainType.BelowGround) {
								Props.TerrainType = LocationTerrainType.BelowGround;
						}

						LoadState = WIGroupLoadState.Initializing;
						mCachedChunk = gameObject.GetComponent <WorldChunk>();

						for (int i = 0; i < ChildGroups.Count; i++) {
								WIGroup childGroup = ChildGroups[i];
								if (!mChildGroupLookup.ContainsKey(childGroup.FileName)) {
										mChildGroupLookup.Add(childGroup.FileName, childGroup);
								}
						}
						LoadState = WIGroupLoadState.Initialized;
				}

				protected void RefreshRequest()
				{
						WIGroups.Refresh(this);
				}

				public void Refresh()
				{
						Props.PathName = GetPathName(this);
						Props.UniqueID = GetUniqueID(this);

						RefreshChildItems();
						RefreshChildGroups();
				}

				#endregion

				#region add / remove /search children and groups

				public bool AddChildItem(IWIBase childItem)
				{
						if (childItem.IsWorldItem) {
								return AddChildItem(childItem.worlditem);
						}
						//otherwise just add it by setting its group 
						childItem.Group = this;
						return true;
				}

				public bool AddChildItem(StackItem childItem)
				{
						if (mDestroyed)
								return false;

						if (!Is(WIGroupLoadState.Loading | WIGroupLoadState.Loaded | WIGroupLoadState.PreparingToLoad)) {
								return false;
						}

						WorldItem instaniatedChildItem = null;
						if (WorldItems.CloneFromStackItem(childItem, this, out instaniatedChildItem)) {
								return AddChildItem(instaniatedChildItem);
						}
						return false;
				}

				public bool AddChildItem(WorldItem childItem)
				{
						if (mDestroyed)
								return false;

						if (!Is(WIGroupLoadState.Loading | WIGroupLoadState.Loaded | WIGroupLoadState.PreparingToLoad)) {
								return false;
						}
						//first we have to remove it from its existing group
						if (childItem.Group != null && childItem.Group != this) {
								if (!childItem.Group.RemoveChildItemFromGroup(childItem)) {
										return false;
								}
						}


						bool result = ChildItems.SafeAdd(childItem);
						bool broadcast = result;

						if (result) {
								//we're assuming that group ownership has been checked
								//and that there's no problem with assigning this group as
								//the child item's group at this point
								childItem.Group = this;
								childItem.SendToGroupPosition();
								//now we have to make sure that the child item's name isn't a duplicate
								//many child items (eg locations) are required to have a unique name
								//and they will use a namer script that doesn't increment its file name
								//
								//if a location doesn't have a unique name, it will max out the increments
								//and the child item will fail to be added
								int maxIncrement = 10;//TEMP
								int currentIncrement = childItem.Props.Name.FileNameIncrement;
								string fileName = childItem.GenerateFileName(currentIncrement);
								if (Props.UnloadedChildItems.Remove(fileName)) {
										//this is one of our unloaded child items
										//we can add it without bothering to increment
										if (mChildItemLookup.ContainsKey(fileName)) {
												mChildItemLookup[fileName] = childItem;
										} else {
												mChildItemLookup.Add(fileName, childItem);
										}
								} else {
										while (mChildItemLookup.ContainsKey(fileName)) {
												//if it contains the key but not the item, we need to rename it
												//there may be a case where 1000 child items with the same name
												//can reasonably be added to a group but i can't think of one
												//in the future, maybe split off into two groups? not sure
												currentIncrement++;
												if (currentIncrement > maxIncrement) {
														return false;
												}
												fileName = childItem.GenerateFileName(currentIncrement);
										}
										try {
												mChildItemLookup.Add(fileName, childItem);
										} catch (Exception e) {
												Debug.LogException(e);
										}
								}
								//call OnAddToGroup before calling load or unload children
								//some WIScripts may need to update their properties before
								//they know if they should load or unload
								childItem.OnAddedToGroup.SafeInvoke();
								if (Is(WIGroupLoadState.PreparingToLoad | WIGroupLoadState.Loading | WIGroupLoadState.Loaded)) {
										childItem.OnGroupLoaded.SafeInvoke();
								}
						} else {
								Debug.Log("Child item " + childItem.name + " was already in group " + name);
								result = true;
								broadcast = false;
						}

						if (broadcast) {
								OnChildItemAdded.SafeInvoke();
						}
						return result;
				}

				public bool AddChildGroup(WIGroup childGroup)
				{
						if (mDestroyed)
								return false;

						if (!Is(WIGroupLoadState.Loading | WIGroupLoadState.Loaded | WIGroupLoadState.PreparingToLoad)) {
								return false;
						}

						//we assume that group ownership check and the like have been
						//resolved by the time this function is called
						if (childGroup != this && !ChildGroups.Contains(childGroup)) {
								//increment file name - this shouldn't be necessary very often
								//but better safe than sorry
								string fileName	= childGroup.FileName;
								int maxIncrements = 100;
								int increment = 0;
								if (!Props.UnloadedChildGroups.Remove(fileName)) {
										while (mChildGroupLookup.ContainsKey(fileName)) {
												fileName = IncrementFileName(fileName, ref increment);
												if (increment > maxIncrements) {
														Debug.Log("Reached max increments in child groups: " + fileName);
														break;
												}
										}
								}
								mChildGroupLookup.Add(fileName, childGroup);
								childGroup.Props.FileName = fileName;
								childGroup.ParentGroup = this;
								//childGroup.transform.parent = transform;
								ChildGroups.Add(childGroup);
								childGroup.Refresh();
								//we've been changed forever so save the group props
								//Mods.Get.Runtime.SaveGroupProps (Props);
						}
						return true;
				}

				public bool UnloadChildItem(WorldItem childItem)
				{
						if (mDestroyed)
								return false;

						//we assume that group ownership check and the like have been
						//resolved by the time this function is called
						if (ChildItems.Remove(childItem)) {
								//tell the child item it has been removed
								childItem.OnRemoveFromGroup.SafeInvoke();
								//remove from lookup
								string fileName = childItem.FileName;
								mChildItemLookup.Remove(fileName);
								if (!childItem.Is(WIMode.RemovedFromGame)) {
										//add it to the unloaded child item list
										Props.UnloadedChildItems.Add(fileName);
								}
								return true;
						}
						return false;
				}

				public bool UnloadChildGroup(WIGroup childGroup)
				{
						if (mDestroyed)
								return false;

						//we assume that group ownership check and the like have been
						//resolved by the time this function is called
						if (ChildGroups.Remove(childGroup)) {
								//remove from lookup
								mChildGroupLookup.Remove(childGroup.FileName);
								Props.UnloadedChildGroups.Add(childGroup.FileName);
								return true;
						}
						return false;
				}

				public bool RemoveChildItemFromGroup(WorldItem childItem)
				{
						if (mDestroyed)
								return false;

						//we assume that group ownership check and the like have been
						//resolved by the time this function is called
						if (ChildItems.Remove(childItem)) {
								//remove from lookup
								string fileName = childItem.FileName;
								mChildItemLookup.Remove(fileName);
								Props.UnloadedChildItems.Remove(fileName);
								//tell the child item it has been removed
								childItem.OnRemoveFromGroup.SafeInvoke();
								//then set the group to null
								//childItem.Group = null;
								//we've been changed forever so save the group props
								//Mods.Get.Runtime.SaveGroupProps (Props);
								OnChildItemRemoved.SafeInvoke();
								return true;
						}
						return false;
				}

				public bool RemoveChildGroupFromGroup(WIGroup childGroup)
				{
						if (mDestroyed)
								return false;

						//we assume that group ownership check and the like have been
						//resolved by the time this function is called
						if (ChildGroups.Remove(childGroup)) {
								//remove from lookup
								mChildGroupLookup.Remove(childGroup.FileName);
								Props.UnloadedChildGroups.Remove(childGroup.FileName);
								//we've been changed forever so save the group props
								//Mods.Get.Runtime.SaveGroupProps (Props);
								return true;
						}
						return false;
				}

				public bool FindChildItem(string itemName, out WorldItem childItem)
				{
						if (mDestroyed) {
								childItem = null;
								return false;
						}

						return mChildItemLookup.TryGetValue(itemName, out childItem);
				}

				public bool FindOrLoadChildItem(string itemName, out WorldItem childItem)
				{
						if (mDestroyed) {
								childItem = null;
								return false;
						}

						bool result = false;
						//if our lookup doesn't have it
						if (!mChildItemLookup.TryGetValue(itemName, out childItem)) {
								//try to load it from disk
								StackItem stackItem = null;
								if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, Props.UniqueID, itemName, true)) {
										result = WorldItems.CloneFromStackItem(stackItem, this, out childItem);
										//childItem.Initialize ( );
										//result = true;
										//this might result in strange behavior
										//keep an eye on it
								}
						} else {
								//we found it!
								result = true;
						}
						return result;
				}

				public List <WorldItem> GetChildrenOfType(List <string> wiScriptTypes)
				{		//TODO find a way to use this that doesn't allocate crap
						List <WorldItem> childrenOfType = new List <WorldItem>();
						for (int i = ChildItems.Count - 1; i >= 0; i--) {
								WorldItem childItem = ChildItems[i];
								if (childItem == null || childItem.Mode == WIMode.RemovedFromGame) {
										ChildItems.RemoveAt(i);
								} else if (childItem.HasAtLeastOne(wiScriptTypes)) {	//if the child item is within the search radius and
										childrenOfType.Add(childItem);
								}
						}
						return childrenOfType;
				}

				public bool GetChildGroup(out WIGroup group, string childGroupName)
				{
						group = null;
						if (mDestroyed) {
								return false;
						}

						if (mChildGroupLookup.TryGetValue(childGroupName, out group)) {
								if (group == null || group.IsDestroyed) {
										mChildGroupLookup.Remove(childGroupName);
										return false;
								}
						}

						return group;
				}

				public int NumChildItemsByCategory(string categoryName, int maxNeeded)
				{
						//this is used by Spawners - they spawn objects from categories
						//and on startup they need to know how many already exist
						//this checks child item names against names in a category
						int numChildItems = 0;
						WICategory category = null;
						if (WorldItems.Get.Category(categoryName, out category)) {
								for (int i = 0; i < category.GenericWorldItems.Count; i++) {
										for (int j = 0; j < ChildItems.Count; j++) {
												if (Stacks.Can.Stack(ChildItems[j].PrefabName, category.GenericWorldItems[i].PrefabName)) {
														numChildItems++;
														if (numChildItems >= maxNeeded) {
																break;
														}
												}
										}
								}
						}
						return numChildItems;
				}

				#endregion

				#region ILoadableChild implementation

				public bool PrepareToLoad()
				{
						if (Is(WIGroupLoadState.PreparingToLoad | WIGroupLoadState.Loading)) {
								return true;
						}
						if (!Is(WIGroupLoadState.Initialized | WIGroupLoadState.Unloaded) | (HasParentGroup && ParentGroup.Is(WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded))) {
								return false;
						}
						LoadState = WIGroupLoadState.PreparingToLoad;
						return true;
				}

				public bool ReadyToLoad {
						get {
								return Is(WIGroupLoadState.PreparingToLoad);
						}
				}

				public void BeginLoad()
				{
						if (!Is(WIGroupLoadState.PreparingToLoad)) {
								return;
						}
						LoadState = WIGroupLoadState.Loading;
						LoadChildItems();
				}

				public void CancelLoad()
				{
						mStackItemsToLoad.Clear();
						LoadState = WIGroupLoadState.Unloaded;
				}

				public bool FinishedLoading {
						get {
								if (Is(WIGroupLoadState.Loaded)) {
										//now that our child items are loaded
										//we can save our state on unload
										SaveOnUnload = true;
										return true;
								} else if (Is(WIGroupLoadState.Loading)) {
										if (mStackItemsToLoad.Count == 0) {
												LoadState = WIGroupLoadState.Loaded;
												SaveOnUnload = true;
												return true;
										}
								}
								return false;
						}
				}

				public bool PrepareToUnload()
				{
						if (Is(WIGroupLoadState.PreparingToUnload)) {
								return true;
						} else if (Is(WIGroupLoadState.Loaded)) {
								//call prepare to unload on each child object
								//don't call it on child groups, that will be handle elsewhere
								bool prepareToUnload = true;
								for (int i = 0; i < ChildItems.Count; i++) {
										prepareToUnload &= ChildItems[i].PrepareToUnload();
								}
								if (prepareToUnload) {
										LoadState = WIGroupLoadState.PreparingToUnload;
								}
								return prepareToUnload;
						} else if (Is(WIGroupLoadState.Initialized)) {
								//if we're initialized but not loaded
								//we can remove ourselves immediately
								//just don't save our state on unload
								//since we don't have any worlditems
								LoadState = WIGroupLoadState.PreparingToUnload;
								SaveOnUnload = false;
								return true;
						}
						return false;
				}

				public bool ReadyToUnload {
						get {
								if (Is(WIGroupLoadState.PreparingToUnload)) {
										bool readyToUnload = true;
										for (int i = 0; i < ChildItems.Count; i++) {
												readyToUnload &= ChildItems[i].ReadyToUnload;
										}
										return readyToUnload;
								}
								return false;
						}
				}

				public void BeginUnload()
				{
						mSavedState = false;
						LoadState = WIGroupLoadState.Unloading;
						for (int i = 0; i < ChildItems.Count; i++) {
								ChildItems[i].BeginUnload();
						}
				}

				public void CancelUnload()
				{
						if (Is(WIGroupLoadState.Unloading)) {
								for (int i = 0; i < ChildItems.Count; i++) {
										ChildItems[i].CancelUnload();
								}
						}
				}

				public bool FinishedUnloading {
						get {

								if (Is(WIGroupLoadState.Unloaded)) {
										if (!mSavedState) {
												Debug.Log ("WIGROUP: " + name + " was UNLOADED before saving state, this should never happen!");
										}
										return true;
								}

								bool finishedUnloading = ChildItems.Count == 0;
								for (int i = ChildItems.LastIndex(); i >= 0; i--) {
										finishedUnloading &= ChildItems[i].FinishedUnloading;
								}
								//check all child groups
								if (finishedUnloading) {
										LoadState = WIGroupLoadState.Unloaded;
										if (!Props.IgnoreOnSave || !SaveOnUnload) {
												Mods.Get.Runtime.SaveGroupProps(Props);
												mSavedState = true;
												ParentGroup.UnloadChildGroup(this);
												mChildItemLookup.Clear();
												mChildGroupLookup.Clear();
												ChildItems.Clear();
										}
								}
								return finishedUnloading;
						}
				}

				public int Depth {
						get {
								if (HasParentGroup) {
										return ParentGroup.Depth + 1;
								}
								return 0;
						}
				}

				public bool Terminal {
						get {
								return !HasChildGroups;
						}
				}

				public bool HasUnloadingParent {
						get {
								if (HasParentGroup) {
										return ParentGroup.Is(WIGroupLoadState.Unloading | WIGroupLoadState.PreparingToUnload);
								}
								return false;
						}
				}

				public IUnloadableChild ShallowestUnloadingParent {
						get {
								IUnloadableChild shallowest = this;
								if (HasParentGroup) {
										if (ParentGroup.Is(WIGroupLoadState.Unloaded | WIGroupLoadState.PreparingToUnload)) {
												shallowest = ParentGroup.ShallowestUnloadingParent;
										}
								}
								return shallowest;
						}
				}

				public void GetChildItems(List <IUnloadableChild> unloadableChildItems)
				{
						for (int i = 0; i < ChildItems.Count; i++) {
								if (!unloadableChildItems.Contains(ChildItems[i])) {
										unloadableChildItems.Add(ChildItems[i]);
								}
						}
						for (int i = 0; i < ChildGroups.Count; i++) {
								if (!unloadableChildItems.Contains(ChildGroups[i])) {
										unloadableChildItems.Add(ChildGroups[i]);
								}
						}
				}

				#endregion

				public void Load()
				{		//TODO remove this now that WIGroups handle loading / unloading
						WIGroups.Load(this);
				}

				public void Unload()
				{		//TODO remove this now that WIGroups handle loading / unloading
						WIGroups.Unload(this);
				}

				protected void LoadChildItems()
				{
						List <string> childItemNames = Mods.Get.Runtime.GroupChildItemNames(Props.UniqueID, false);
						mStackItemsToLoad.Clear();
						for (int i = 0; i < childItemNames.Count; i++) {
								string childItemName = childItemNames[i];
								if (Props.UnloadedChildItems.Contains(childItemName)) {
										StackItem stackItem = null;
										if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, Props.UniqueID, childItemName, false)) {
												if (stackItem.Mode != WIMode.RemovedFromGame) {
														mStackItemsToLoad.Enqueue(stackItem);
												}
										}
								}
						}

						if (mStackItemsToLoad.Count == 0) {
								LoadState = WIGroupLoadState.Loaded;
						} else {
								if (GameManager.Is(FGameState.InGame)) {
										//if we're in game, load them over time
										WorldItems.LoadStackItems(mStackItemsToLoad, this);
								} else {
										//otherwise load them all at once so we don't have to wait
										while (mStackItemsToLoad.Count > 0) {
												WorldItem worlditem = null;
												WorldItems.CloneFromStackItem(mStackItemsToLoad.Dequeue(), this, out worlditem);
										}
										LoadState = WIGroupLoadState.Loaded;
								}
						}
				}

				protected void RefreshChildItems()
				{
						if (mRefreshingChildItems || ChildItems.Count <= 0) {
								return;
						}

						mRefreshingChildItems = true;

						List <WorldItem> itemsToRemove = new List <WorldItem>();

						for (int i = ChildItems.LastIndex(); i >= 0; i--) {
								WorldItem childItem = ChildItems[i];
								if (childItem == null || childItem.Group != this) {	//this may be unwise - if the group isn't set to this
										//I don't know what it would be doing here, and the worlditem
										//may end up unmanaged. But I'm adding a check here just in case.
										ChildItems.RemoveAt(i);
								} else if (childItem.Is(WILoadState.Unloaded)) {
										ChildItems.RemoveAt(i);
								}
						}

						mRefreshingChildItems = false;
				}

				protected void RefreshChildGroups()
				{
						if (mRefreshingChildGroups) {
								return;
						}

						mRefreshingChildGroups = true;

						if (ChildGroups.Count > 0) {
								bool refreshChildGroupLookup = mChildGroupLookup.Count == 0;

								for (int j = ChildGroups.LastIndex(); j >= 0; j--) {
										WIGroup childGroup = ChildGroups[j];
										if (childGroup == null || childGroup.Is(WIGroupLoadState.Unloaded)) {
												ChildGroups.RemoveAt(j);
										}
								}
						}

						mRefreshingChildGroups = false;
				}

				#region IComparable implementation

				public override bool Equals(object obj)
				{
						if (obj == null) {
								return false;
						}

						WIGroup other = obj as WIGroup;
						if (this == other) {
								return true;
						}

						return false;
				}

				public bool Equals(WIGroup other)
				{
						if (other == null) {
								return false;
						}

						return string.Equals(this.Props.UniqueID, other.Props.UniqueID);
				}

				public int CompareTo(WIGroup other)
				{
						return Depth.CompareTo(other.Depth);
				}

				public override int GetHashCode()
				{
						return Props.UniqueID.GetHashCode();
				}

				#endregion

				#region static helper functions

				public static string IncrementFileName(string fileName, ref int increment)
				{
						increment++;
						//TODO put this in a string builder
						return (fileName + "_" + increment.ToString());
				}

				public static string GetPathName(WIGroup group)
				{
						if (string.IsNullOrEmpty(group.FileName)) {
								group.Props.FileName = group.name;
						}
						List <string> paths = new List <string>();
						WIGroup currentGroup	= group;
						bool isRoot = false;

						if (Application.isPlaying) {
								while (!isRoot) {
										paths.Add(currentGroup.FileName);
										isRoot = currentGroup.IsRoot;
										currentGroup = currentGroup.ParentGroup;
								}
						} else {
								while (!isRoot) {
										if (currentGroup.IsChunk) {
												paths.Add(currentGroup.Props.PathName);
										} else {
												paths.Add(currentGroup.FileName);
										}
										if (currentGroup.transform.parent != null) {
												WIGroup parentGroup = currentGroup.transform.parent.GetComponent <WIGroup>();
												if (parentGroup != null) {
														currentGroup = parentGroup;
												} else {
														isRoot = true;
												}
										} else {
												isRoot = true;
										}
								}
						}
						paths.Reverse();
						string pathName = string.Join(gPathJoinString, paths.ToArray());
						return pathName;
				}

				public static string GetChildPathName(string groupPathName, string childName)
				{
						return groupPathName + gPathJoinString + childName;
				}

				public static string GetUniqueID(string groupPathName)
				{
						return ShortUrl.GetUniqueID(groupPathName);
				}

				public static string GetUniqueID(WIGroup group)
				{
						return ShortUrl.GetUniqueID(group.Props.PathName);
				}

				public static string CombinePath(Stack <string> splitPath)
				{
						List <string> splitPathList	= new List <string>();
						while (splitPath.Count > 0) {
								splitPathList.Insert(0, splitPath.Pop());
						}
						return String.Join(gPathJoinString, splitPathList.ToArray());
				}

				public static KeyValuePair <string,string>	GroupChildPair(string path)
				{
						return new KeyValuePair <string, string>(AllButLastInPath(path), LastInPath(path));
				}

				public static string AllButLastInPath(string path)
				{
						string[] splitPathArray = path.Split(new string [] { gPathJoinString }, StringSplitOptions.RemoveEmptyEntries);
						splitPathArray[splitPathArray.Length - 1] = "";
						return splitPathArray.JoinToString(gPathJoinString);
				}

				public static string LastInPath(string path)
				{
						string[] splitPathArray = path.Split(new string [] { gPathJoinString }, StringSplitOptions.RemoveEmptyEntries);
						return splitPathArray[splitPathArray.Length - 1];
				}

				public static Stack <string> SplitPath(string path)
				{
						string[] splitPathArray = path.Split(new string [] { gPathJoinString }, StringSplitOptions.RemoveEmptyEntries);
						Stack <string> splitPath = new Stack <string>();
						for (int i = splitPathArray.Length - 1; i >= 0; i--) {
								splitPath.Push(splitPathArray[i]);
						}
						return splitPath;
				}

				public static void MoveChildGroup(WIGroup childGroup, WIGroup fromGroup, WIGroup toGroup)
				{
						string oldPathName = childGroup.Props.PathName;

						toGroup.ChildGroups.Add(childGroup);
						fromGroup.ChildGroups.Remove(fromGroup);
						childGroup.ParentGroup = toGroup;
						childGroup.Refresh();

						string newPathName = childGroup.Props.PathName;
						//Mods.Get.MoveData ("WorldItem", oldPathName, newPathName);
				}

				#endregion

				public static bool TryToMoveChildItem(WorldItem childItem, WIGroup fromGroup, WIGroup toGroup, ref string error)
				{
						//ASSUMPTIONS:
						//the child item is containerd in fromGroup and not contained in toGroup
						//the child item is not being unloaded or destroyed
						bool result = true;

						IStackOwner owner = null;
						HashSet <string> removeItemSkillNames = new HashSet <string>();
						if (fromGroup.HasOwner(out owner) && owner.UseRemoveItemSkill(removeItemSkillNames, ref owner)) {
								//RemoveItemSkill skill	= Skills.Get.RemoveItemSkillFromName (fromGroup.Owner.RemoveItemSkill);
								//result = skill.TryToRemoveItem (fromGroup, childItem, ref error);
						} else {
								fromGroup.ChildItems.Remove(childItem);
								toGroup.ChildItems.Add(childItem);
								childItem.Group = toGroup;
								result = true;
						}
						return result;
				}

				protected double TimeSinceUnloaded {
						get {
								return WorldClock.RealTime - mTimeLastUnloaded;
						}
				}

				protected double TimeSinceLoaded {
						get {
								return WorldClock.RealTime - mTimeLastLoaded;
						}
				}

				#if UNITY_EDITOR
				public bool EditorLoaded = false;

				public void LoadEditor()
				{
						if (!Manager.IsAwake <Mods>()) {
								Manager.WakeUp <Mods>("__MODS");
						}
						Mods.Get.Editor.InitializeEditor();

						if (!Manager.IsAwake <WorldItems>()) {
								Manager.WakeUp <WorldItems>("Frontiers_WorldItems");
								WorldItems.Get.Initialize();
						}

						UnityEditor.EditorUtility.DisplayProgressBar("Loading group " + name, "Loading", 1f);
						DoneLoading = false;
						mEditorStackItemsToLoad.Clear();
						mEditorChildGroupsToLoad.Clear();
						ChildGroups.Clear();
						ChildItems.Clear();

						mEditorStackItemsToLoad.AddRange(Mods.Get.Editor.GroupChildItemNames(Props.UniqueID));
						mEditorChildGroupsToLoad.AddRange(Mods.Get.Editor.GroupChildGroupNames(Props.UniqueID));
						EditorNumChildrenToLoad = mEditorStackItemsToLoad.Count;
						EditorNumGroupsToLoad = mEditorChildGroupsToLoad.Count;

						foreach (string nextChild in mEditorStackItemsToLoad) {
								//Debug.Log ("Loading child item " + nextChild);
								StackItem stackItem = null;
								if (Mods.Get.Editor.LoadStackItemFromGroup(ref stackItem, Props.UniqueID, nextChild)) {
										WorldItem newChildItem = null;
										Transform childItemTransform = transform.FindChild(nextChild);
										if (childItemTransform == null || !childItemTransform.gameObject.HasComponent <WorldItem>(out newChildItem)) {
												//we have to instantiate it from scratch
												WorldItem packPrefab = null;
												if (WorldItems.Get.PackPrefab(stackItem.PackName, stackItem.PrefabName, out packPrefab)) {
														GameObject basePrefab = UnityEditor.PrefabUtility.FindPrefabRoot(packPrefab.gameObject);
														if (basePrefab != null) {
																//Debug.Log ("Creating non-generic world item " + basePrefab.name);
																GameObject instantiatedUnWi = UnityEditor.PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
																//instantiate a new prefab - keep it as a prefab!
																instantiatedUnWi.name = basePrefab.name;
																instantiatedUnWi.transform.parent = transform;
																//put it in the right place
																stackItem.Props.Local.Transform.ApplyTo(instantiatedUnWi.transform, false);
																//instantiatedUnWi.transform.localScale = Vector3.one * packPrefab.Props.Global.ScaleModifier;

																newChildItem = instantiatedUnWi.GetComponent <WorldItem>();
																newChildItem.Props.Global = packPrefab.Props.Global;
														}
												}
										}
										if (newChildItem != null) {
												newChildItem.ReceiveState(ref stackItem);
												ChildItems.SafeAdd(newChildItem);
												newChildItem.OnEditorLoad();
										}
								}
						}

						foreach (string nextGroup in mEditorChildGroupsToLoad) {
								Transform childGroupTransform = gameObject.FindOrCreateChild(nextGroup);
								WIGroup childGroup = childGroupTransform.gameObject.GetOrAdd <WIGroup>();
								childGroup.ParentGroup = this;
								ChildGroups.SafeAdd(childGroup);
								childGroup.Refresh();
						}

						UnityEditor.EditorUtility.ClearProgressBar();
						EditorLoaded = true;
				}

				protected List <string> mEditorStackItemsToLoad = new List<string>();
				protected List <string> mEditorChildGroupsToLoad = new List<string>();
				public bool DoneLoading = false;
				public int EditorNumChildrenToLoad = 0;
				public int EditorNumGroupsToLoad = 0;

				public void UnloadEditor()
				{
						foreach (WorldItem childItem in ChildItems) {
								if (childItem != null) {
										GameObject.DestroyImmediate(childItem.gameObject);
								}
						}
						foreach (WIGroup childGroup in ChildGroups) {
								if (childGroup != null) {
										GameObject.DestroyImmediate(childGroup.gameObject);
								}
						}
						ChildItems.Clear();
						ChildGroups.Clear();
						EditorLoaded = false;
				}

				public void RefreshEditor()
				{
						//refreshes the group in the editor while building a level
						//it prepares the worlditems to be saved to disk by a chunk
						//this should not be called during gameplay
						if (Application.isPlaying) {
								return;
						}

						Dictionary <string, WorldItem> addedItems = new Dictionary <string, WorldItem>();

						Props.FileName = gameObject.name;
						Chunk = gameObject.GetComponent <WorldChunk>();

						if (Chunk != null) {
								Props.PathName = GetPathName(this);
								Props.UniqueID = GetUniqueID(this);
						} else {
								Props.PathName = GetPathName(this);
								Props.UniqueID = GetUniqueID(this);
						}

						Props.UnloadedChildGroups.Clear();
						Props.UnloadedChildItems.Clear();
						ChildItems.Clear();
						ChildGroups.Clear();

						foreach (Transform child in transform) {
								WorldItem worlditem = child.GetComponent <WorldItem>();
								//Location location = child.GetComponent <Location> ();
								if (worlditem != null) {
										worlditem.Group = this;
										worlditem.OnEditorRefresh();
										int increment = 0;
										string fileName = worlditem.GenerateFileName(increment);
										while (Props.UnloadedChildItems.Contains(fileName)) {
												increment += 1;
												fileName = worlditem.GenerateFileName(increment);
												if (increment > 999) {
														Debug.Log("WHaaaaah " + fileName + " - " + child.name + " file name increment broke 999");
														break;
												}
										}
										Props.UnloadedChildItems.Add(fileName);
										ChildItems.Add(worlditem);
								}

								WIGroup group = child.GetComponent <WIGroup>();
								if (group != null) {
										group.ParentGroup = this;
										group.RefreshEditor();
										Props.UnloadedChildGroups.Add(group.FileName);
										ChildGroups.Add(group);
								}
						}
				}

				public void SaveEditor()
				{
						if (!mSavingEditorOverTime) {
								mSavingEditorOverTime = true;
								//saves all child items to disk in the editor
								//does not save child groups to disk
								//this should not be called during gameplay

								//this function assumes RefreshEditor has been called
								if (Application.isPlaying) {
										return;
								}

								if (!EditorLoaded) {
										Debug.Log("GROUP " + Props.PathName + " IS NOT SAFE TO SAVE, SKIPPING");
										return;
								}

								foreach (Transform child in transform) {
										WorldItem worlditem = child.GetComponent <WorldItem>();
										if (worlditem != null) {
												StackItem stackItem = worlditem.GetStackItem(WIMode.Frozen);
												//ColoredDebug.Log ("Saving worlditem " + worlditem.name + " in group " + Props.Name + " Position: " + stackItem.Props.Local.Transform.Position.ToString ( ));
												Mods.Get.Editor.SaveMod <StackItem>(stackItem, "Group", Props.UniqueID, worlditem.FileName);
										}
								}

								Mods.Get.Editor.SaveGroupProps(Props);
						}
						mSavingEditorOverTime = false;
				}

				public void DrawEditor()
				{
						IStackOwner owner = null;
						UnityEngine.GUI.color = Color.cyan;
						if (HasOwner(out owner)) {
								GUILayout.Button("Owner: " + owner.FileName);
								HashSet <string> removeItemSkillNames = new HashSet<string>();
								UnityEngine.GUI.color = Color.yellow;
								IStackOwner useTarget = null;
								if (owner.UseRemoveItemSkill(removeItemSkillNames, ref useTarget)) {
										foreach (string removeItemSkillName in removeItemSkillNames) {
												GUILayout.Button("Skill: " + removeItemSkillName);
										}
								}
						} else {
								GUILayout.Button("(Has no owner)");
						}
						switch (LoadState) {
								case WIGroupLoadState.None:
								case WIGroupLoadState.Uninitialized:
								default:
										UnityEngine.GUI.color = Color.red;
										break;

								case WIGroupLoadState.Initializing:
								case WIGroupLoadState.Initialized:
								case WIGroupLoadState.Loading:
										UnityEngine.GUI.color = Color.yellow;
										break;

								case WIGroupLoadState.Loaded:
										UnityEngine.GUI.color = Color.green;
										break;

						}
						GUILayout.Button(LoadState.ToString());
						WIGroupUnloader unloader = null;
						if (WIGroups.UnloaderMappings.TryGetValue(this, out unloader)) {
								UnityEngine.GUI.color = Color.red;
								GUILayout.Button("HAS UNLOADER");
						} else if (HasUnloadingParent) {
								UnityEngine.GUI.color = Color.yellow;
								GUILayout.Button("Parent is unloading");
						}
						UnityEngine.GUI.color = Color.yellow;
						if (EditorLoaded) {
								UnityEngine.GUI.color = Color.gray;
						}
						if (GUILayout.Button("\n\nLOAD GROUP\n\n") && !EditorLoaded) {
								Debug.Log("Loading editor...");
								LoadEditor();
						}
						UnityEngine.GUI.color = Color.red;
						if (!EditorLoaded) {
								UnityEngine.GUI.color = Color.gray;
						}
						if (GUILayout.Button("\n\nUNLOAD GROUP\n\n") && EditorLoaded) {
								Debug.Log("Unloading editor...");
								UnloadEditor();
						}

				}

				protected bool mSavingEditorOverTime = false;
				#endif

				protected double mTimeLastUnloaded = 0.0f;
				protected double mTimeLastLoaded = 0.0f;
				protected bool mRefreshingChildGroups = false;
				protected bool mRefreshingChildItems = false;
				protected bool mChildItemsLoaded = false;
				public static char gPathJoinChar = '\\';
				public static string gPathJoinString = "\\";
				protected Dictionary <string, WorldItem> mChildItemLookup;
				protected Dictionary <string, WIGroup> mChildGroupLookup;
				protected Queue <StackItem> mStackItemsToLoad;
		}

		public enum WIGroupType
		{
				AlwaysActive,
				VisibilityBased,
				Manual,
		}

		[Serializable]
		[XmlRoot(ElementName = "WIGroupProps")]
		public class WIGroupProps : Mod
		{
				public WIGroupProps()
				{

				}

				public string FileName = "NewGroup";
				public bool IgnoreOnSave = true;
				//child objects
				public List <string> UnloadedChildGroups = new List <string>();
				public List <string> UnloadedChildItems = new List <string>();
				//general props
				public string UniqueID = string.Empty;
				public string PathName = string.Empty;
				public LocationTerrainType TerrainType = LocationTerrainType.AboveGround;
				public bool Interior = false;
				public int ID = 0;

				public void Clear()
				{
						UnloadedChildGroups.Clear();
						UnloadedChildItems.Clear();
						UnloadedChildGroups = null;
						UnloadedChildItems = null;
				}
		}
}
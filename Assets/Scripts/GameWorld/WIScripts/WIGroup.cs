using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers;
using Frontiers.World.WIScripts;

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
		public HashSet <WorldItem> ChildItems;
		public Transform tr;
		public bool AttemptedToUnload = false;
		//convenience
		public string Path {
			get {
				return Props.PathName;
			}
		}

		#if UNITY_EDITOR
		public string HoldoutChildItem;
		#endif

		public string FileName {
			get {
				return Props.FileName;
			}
		}

		public bool SaveOnUnload = false;

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
		protected bool mDestroyed;
		protected bool mSavedState = false;
		protected WIGroupLoadState mLoadState;

		public void UpdateDirty()
		{
			mChildItemsToUpdateChanged = false;
			bool announceLoad = Is(WIGroupLoadState.Loaded | WIGroupLoadState.Loading);
			bool continueUpdating = true;
			var childItemsToUpdate = mChildItemsToUpdate.GetEnumerator();
			while (continueUpdating) {
				continueUpdating = false;
				if (mChildItemsToUpdateChanged) {
					#if UNITY_EDITOR
					Debug.Log ("Skipping update in WIGroup because child items were changed, will update next frame");
					#endif
					WIGroups.UpdateDirtyGroup (this);
					return;
				} else if (childItemsToUpdate.MoveNext ()) {
					continueUpdating = true;
					childItemsToUpdate.Current.Group = this;
					childItemsToUpdate.Current.OnAddedToGroup.SafeInvoke ();
					if (announceLoad) {
						childItemsToUpdate.Current.OnGroupLoaded.SafeInvoke ();
					}
				}
			}
			mChildItemsToUpdate.Clear();
		}

		public void OnDestroy()
		{
			mDestroyed = true;
			if (!mSavedState) {
				//Debug.Log("WIGROUP " + name + " WAS DESTROYED WITHOUT SAVING STATE");
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

		public bool IsParentOf(WIGroup group)
		{
			if (group == null) {
				return false;
			}
			//the group's depth is greater than ours
			//AND it contains our path
			//then they're definitely a child group
			//even if they're not an immediate child group
			if (group.Depth > Depth && group.Path.Contains(Path)) {
				return true;
			}
			return false;
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
				return mChildItemsToUpdate != null && mChildItemsToUpdate.Count > 0;
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
				ChildGroups = new List <WIGroup>(Props.UnloadedChildGroups.Count + 1);

			if (ChildItems == null)
				ChildItems = new HashSet <WorldItem>();


			tr = transform;
			DontDestroyOnLoad(tr);
			mLoadState = WIGroupLoadState.Uninitialized;
			mChildItemLookup = new Dictionary<string, WorldItem>();
			mChildGroupLookup = new Dictionary<string, WIGroup>();
			//mStackItemsToLoad = new Queue<StackItem>();
		}

		public void Initialize()
		{
			if (!Is(WIGroupLoadState.Uninitialized)) {
				Debug.Log("Group " + Path + " isn't uninitialized");
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
			mInstantiatePosition = tr.position;
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

			if (Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
				//Debug.Log("Trying to add child item " + childItem.Name + " to group while it's unloading / unloaded");
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

			if (Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
				//Debug.Log("Trying to add child item " + childItem.FileName + " to group while it's unloading / unloaded");
				return false;
			}
			//does it already have a group?
			if (childItem.Group != null) {
				//if it has a group and it's not this group
				//first we have to remove it from its existing group
				if (childItem.Group != this && !childItem.Group.RemoveChildItemFromGroup(childItem)) {
					//we can't proceed if the other group won't let it go
					//Debug.Log("Couldn't remove from existing group");
					return false;
				}
			} else {
				//it should really be set already
				//but just in case, set it here
				childItem.Group = this;
			}

			bool result = ChildItems.Add(childItem);
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
				if (mChildItemsToUpdate == null) {
					mChildItemsToUpdate = new List <WorldItem>();
				}
				if (mChildItemsToUpdate.Count != 0) {
					mChildItemsToUpdateChanged = true;
				}
				mChildItemsToUpdate.Add(childItem);
			} else {
				//Debug.Log("Child item " + childItem.name + " was already in group " + name);
				result = true;
				broadcast = false;
			}
			if (broadcast) {
				OnChildItemAdded.SafeInvoke();
			}
			if (mChildItemsToUpdate.Count > 0) {
				WIGroups.UpdateDirtyGroup(this);
			}
			return result;
		}

		public bool AddChildGroup(WIGroup childGroup)
		{
			if (mDestroyed)
				return false;

			if (Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
				//Debug.Log("Trying to add child group " + childGroup.Path + " to group while it's unloading / unloaded");
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
			if (mDestroyed) {
				Debug.Log ("Attempting to remove child item " + childItem.FileName + " from group " + Props.FileName + " but group was destroyed");
				return false;
			}

			//we assume that group ownership check and the like have been
			//resolved by the time this function is called
			if (ChildItems.Remove(childItem)) {
				//tell the child item it has been removed
				childItem.OnRemoveFromGroup.SafeInvoke();
				//remove from lookup
				string fileName = childItem.FileName;
				mChildItemLookup.Remove(fileName);
				if (!childItem.Is(WIMode.RemovedFromGame) && childItem.SaveItemOnUnloaded) {
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
			}
			//return true in either case
			//if it's not in the group, no harm done
			return true;
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
			List <WorldItem> childrenToRemove = new List<WorldItem>();
			var childItemEnum = ChildItems.GetEnumerator();
			while (childItemEnum.MoveNext()) {
				WorldItem childItem = childItemEnum.Current;
				if (childItem == null || childItem.Mode == WIMode.RemovedFromGame) {
					childrenToRemove.Add(childItem);
				} else if (childItem.HasAtLeastOne(wiScriptTypes)) {	//if the child item is within the search radius and
					childrenOfType.Add(childItem);
				}
			}
			for (int i = 0; i < childrenToRemove.Count; i++) {
				ChildItems.Remove(childrenToRemove[i]);
			}
			childrenToRemove.Clear();
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
					var childItemEnum = ChildItems.GetEnumerator();
					while (childItemEnum.MoveNext ()) {
						if (Stacks.Can.Stack(childItemEnum.Current, category.GenericWorldItems[i])) {
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

			if (!Is(WIGroupLoadState.Initialized | WIGroupLoadState.Unloaded)) {
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
			//Debug.Log("Beginning load in " + name);
			LoadState = WIGroupLoadState.Loading;
			if (!gameObject.activeSelf) {
				gameObject.SetActive(true);
			}
			StartCoroutine(LoadChildItemsOverTime());
		}

		public bool TryToCancelLoad()
		{
			//mStackItemsToLoad.Clear();
			if (Is(WIGroupLoadState.PreparingToLoad)) {
				LoadState = WIGroupLoadState.Unloaded;
				return true;
			}
			return false;
		}

		public bool FinishedLoading {
			get {
				if (Is(WIGroupLoadState.Loaded)) {
					//now that our child items are loaded
					//we can save our state on unload
					SaveOnUnload = true;
					return true;
				}
				/*else if (Is(WIGroupLoadState.Loading)) {
										if (mStackItemsToLoad.Count == 0) {
												LoadState = WIGroupLoadState.Loaded;
												SaveOnUnload = true;
												return true;
										}
								}*/
				return false;
			}
		}

		public bool PrepareToUnload()
		{
			if (Is(WIGroupLoadState.PreparingToUnload | WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
				//this is true for any step down the line
				return true;
			} else if (Is(WIGroupLoadState.Loaded)) {
				//call prepare to unload on each child object
				//don't call it on child groups, that will be handle elsewhere
				bool prepareToUnload = true;
				var childItemEnum = ChildItems.GetEnumerator();
				while (childItemEnum.MoveNext ()) {
					#if UNITY_EDITOR
					if (!childItemEnum.Current.PrepareToUnload ()) {
						HoldoutChildItem = childItemEnum.Current.FileName;
						prepareToUnload = false;
					}
					#else
					prepareToUnload &= childItemEnum.Current.PrepareToUnload();
					#endif
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
				if (IsDirty)
					return false;

				if (Is(WIGroupLoadState.PreparingToUnload)) {
					bool readyToUnload = true;
					var childItemEnum = ChildItems.GetEnumerator();
					while (childItemEnum.MoveNext ()) {
						#if UNITY_EDITOR
						if (!childItemEnum.Current.ReadyToUnload) {
							HoldoutChildItem = childItemEnum.Current.FileName;
							readyToUnload = false;
						}
						#else
						readyToUnload &= childItemEnum.Current.ReadyToUnload;
						#endif
					}
					return readyToUnload;
				} else if (Is(WIGroupLoadState.Unloading | WIGroupLoadState.Unloaded)) {
					//true for any step down theline
					return true;
				}
				return false;
			}
		}

		public void BeginUnload()
		{
			if (LoadState == WIGroupLoadState.PreparingToUnload) {
				mSavedState = false;
				LoadState = WIGroupLoadState.Unloading;
				if (!gameObject.activeSelf) {
					gameObject.SetActive (true);
				}
				StartCoroutine(UnloadOverTime());
			}
		}

		protected IEnumerator UnloadOverTime()
		{
			#if UNITY_EDITOR
			Debug.Log("Unloading over time in " + Props.FileName);
			#endif
			//start by telling the worlditems to begin unloading
			var childItemEnum = ChildItems.GetEnumerator();
			while (childItemEnum.MoveNext()) {
				if (childItemEnum.Current != null) {
					childItemEnum.Current.BeginUnload();
				}
				yield return null;
			}
			yield return null;
			while (ChildItems.Count > 0) {
				//save each child item one at a time
				//this will remove them from the child items list automatically
				WorldItem childItemToRemove = null;
				childItemEnum = ChildItems.GetEnumerator();
				if (childItemEnum.MoveNext()) {
					childItemToRemove = childItemEnum.Current;
					if (childItemToRemove != null) {
						if (childItemToRemove.FinishedUnloading) {
							childItemToRemove = childItemEnum.Current;
							yield return null;
						} else {
							#if UNITY_EDITOR
							HoldoutChildItem = childItemEnum.Current.FileName;
							#endif
							childItemToRemove = null;
						}
					}
				}
				//removes null as well
				ChildItems.Remove(childItemToRemove);
				//wait a moment regardless
				yield return null;
			}
			if (!Props.IgnoreOnSave && SaveOnUnload) {
				Mods.Get.Runtime.SaveGroupProps(Props);
				mSavedState = true;
			}
			//mark this as true even if we're supposed to be ignored
			mSavedState = true;
			LoadState = WIGroupLoadState.Unloaded;
			ParentGroup.UnloadChildGroup(this);
			mChildItemLookup.Clear();
			mChildGroupLookup.Clear();
			ChildItems.Clear();
			//Debug.Log("Finished unloading in " + name);
			yield break;
		}

		public bool TryToCancelUnload()
		{
			/*
			if (WIGroups.TryToCancelUnload(this)) {

					for (int i = 0; i < ChildItems.Count; i++) {
							ChildItems[i].TryToCancelUnload();
					}
					return true;
			}
			*/
			//we're going to try an experiment
			//and fail cancel unload requests across the board
			return false;
		}

		public bool FinishedUnloading {
			get {
				if (Is (WIGroupLoadState.Unloading) && (ChildItems.Count == 0 && ChildGroups.Count == 0)) {
					LoadState = WIGroupLoadState.Unloaded;
				}
				if (Is(WIGroupLoadState.Unloaded)) {
					return true;
				}
				return false;
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
			var childItemEnum = ChildItems.GetEnumerator();
			while (childItemEnum.MoveNext()) {
				if (!unloadableChildItems.Contains(childItemEnum.Current)) {
					unloadableChildItems.Add(childItemEnum.Current);
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

		protected IEnumerator LoadChildItemsOverTime()
		{
			List <string> childItemNames = Mods.Get.Runtime.GroupChildItemNames(Props.UniqueID, false);
			//mStackItemsToLoad.Clear();
			yield return null;
			for (int i = 0; i < childItemNames.Count; i++) {
				//first check to see if we've loaded too many worlditems this frame
				while (WIGroups.NumWorldItemsLoadedThisFrame > WIGroups.MaxWorldItemsLoadedPerFrame) {
					//this will eventually be reset
					yield return null;
				}
				//now increment so other groups know not to load too many
				WIGroups.NumWorldItemsLoadedThisFrame++;
				string childItemName = childItemNames[i];
				if (Props.UnloadedChildItems.Contains(childItemName)) {
					StackItem stackItem = null;
					if (Mods.Get.Runtime.LoadStackItemFromGroup(ref stackItem, Props.UniqueID, childItemName, false)) {
						if (stackItem.Mode != WIMode.RemovedFromGame) {
							WorldItem worlditem = null;
							WorldItems.CloneFromStackItem(stackItem, this, out worlditem);
							yield return null;
							if (worlditem != null) {
								worlditem.Initialize ();
							} else {
								Debug.Log ("CHILD ITEM WAS NULL in group " + Props.PathName);
							}
						}
					}
				}
				yield return null;
			}
			LoadState = WIGroupLoadState.Loaded;
			yield break;
		}

		protected void RefreshChildItems()
		{
			if (mRefreshingChildItems || ChildItems.Count <= 0) {
				return;
			}

			mRefreshingChildItems = true;

			List <WorldItem> itemsToRemove = new List <WorldItem>();
			var childItemEnum = ChildItems.GetEnumerator();
			while (childItemEnum.MoveNext ()) {
				WorldItem childItem = childItemEnum.Current;
				if (childItem == null || childItem.Group != this) {	//this may be unwise - if the group isn't set to this
					//I don't know what it would be doing here, and the worlditem
					//may end up unmanaged. But I'm adding a check here just in case.
					itemsToRemove.Add(childItem);
				} else if (childItem.Is(WILoadState.Unloaded)) {
					itemsToRemove.Add(childItem);
				}
			}
			for (int i = 0; i < itemsToRemove.Count; i++) {
				ChildItems.Remove(itemsToRemove[i]);
			}
			itemsToRemove.Clear();

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

		protected Vector3 mInstantiatePosition;

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
				Debug.Log("Loading child item " + nextChild);
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
								//Debug.Log("Creating non-generic world item " + basePrefab.name);
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
						ChildItems.Add(newChildItem);
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
			if (ChildItems != null) {
				ChildItems.Clear ();
			} else {
				ChildItems = new HashSet<WorldItem> ();
			}
			if (ChildGroups != null) {
				ChildGroups.Clear ();
			} else {
				ChildGroups = new List<WIGroup> ();
			}

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
			if (Application.isPlaying) {
				WIGroupUnloader unloader = null;
				if (WIGroups.UnloaderMappings.TryGetValue(Path, out unloader)) {
					UnityEngine.GUI.color = Color.red;
					GUILayout.Button("HAS UNLOADER");
				} else if (HasUnloadingParent) {
					UnityEngine.GUI.color = Color.yellow;
					GUILayout.Button("Parent is unloading");
				}
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
			foreach (WorldItem item in ChildItems) {
				if (item == null) {
					GUILayout.Button ("NULL");
				} else {
					if (GUILayout.Button (item.FileName)) {
						UnityEditor.Selection.activeGameObject = item.gameObject;
					}
				}
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
		protected List <WorldItem> mChildItemsToUpdate;
		protected bool mChildItemsToUpdateChanged = false;
		//protected Queue <StackItem> mStackItemsToLoad;
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